using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Linq;

namespace ExternalBanking.DBManager
{
    internal class DahkDetailsDB
    {
        /// <summary>
        /// Վերադարձնում է հաճախորդի արգելանքները և ազատումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<DahkDetails> GetDahkBlockages(ulong customerNumber)
        {
            List<DahkDetails> blockages = new List<DahkDetails>();

            string sql = "pr_customer_DAHK_details";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        DahkDetails blockage = SetDahkDetails(row);

                        blockages.Add(blockage);
                    }

                }
            }

            return blockages;
        }


        /// <summary>
        /// Վերադարձնում է ԴԱՀԿ գործող վարույթների տվյալները տվյալ հաճախորդի համար
        /// </summary>
        internal static List<AccountDAHKfreezeDetails> GetCurrentInquestDetails(ulong customerNumber)
        {
            List<AccountDAHKfreezeDetails> details = new List<AccountDAHKfreezeDetails>();

            string sql = @"SELECT a.Request_Date, a.MESSAGEID,a.INQUESTID, a.BBLOCKSUM1, a.BBLOCKCUR1,ISNULL(DF.freezed_amount,0) freezed_amount
                            INTO #tmp 
                           FROM Tbl_customers_dahk_details d 
		                        INNER JOIN (SELECT INQUESTID,customer_number,blockage_type,MAX(set_ID) AS lsetID 
							                            FROM Tbl_customers_dahk_details 
							                            WHERE blockage_type=0
							                            GROUP BY INQUESTID,customer_number,blockage_type) ld ON d.INQUESTID=ld.INQUESTID AND d.customer_number=ld.customer_number   
		                        INNER JOIN DAHK_Base.dbo.Tbl_DAHK_customers dc ON d.reasonID=dc.ID  
		                        INNER JOIN (SELECT 1 AS [type],MESSAGEID,Request_Date,ID,INQUESTID,BBLOCKSUM1,BBLOCKCUR1
						                        FROM DAHK_Base.dbo.Tbl_CheckMsg_Response_Attach  					
						                        ) a  ON dc.message_type = a.[type] AND dc.message_tbl_ID = a.ID                              
		                        OUTER APPLY(SELECT SUM(f.freeze_amount) freezed_amount 
                                            FROM Tbl_DAHK_freeze_details f INNER JOIN Tbl_acc_freeze_history h ON h.ID = f.freeze_ID
		                                    WHERE f.customer_number = d.customer_number AND f.message_ID=a.MESSAGEID AND f.inquestID=a.INQUESTID
		                                    AND f.closing_message_table_ID IS NULL AND f.closing_message_type IS NULL AND h.closing_date IS NULL
		                                    GROUP BY f.inquestID,f.message_ID) DF
	                        WHERE d.customer_number = @customerNumber and ld.lsetID = d.set_ID AND d.quality = 1 
                            CREATE CLUSTERED INDEX ix_inquestid ON #tmp (inquestid) 
                            SELECT t.* FROM #tmp t
                               OUTER APPLY (SELECT TOP 1 Request_Date FROM DAHK_Base.dbo.Tbl_CheckMsg_Response_Attach WHERE INQUESTID = t.INQUESTID ORDER BY Request_Date) req_date
					         ORDER BY req_date.Request_Date
					        DROP TABLE #tmp";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i<dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        AccountDAHKfreezeDetails detail = new AccountDAHKfreezeDetails();

                        detail.RequestDate = DateTime.Parse(row["request_date"].ToString());
                        detail.MessageID = Utility.ConvertAnsiToUnicode(row["MESSAGEID"].ToString());
                        detail.InquestID = row["INQUESTID"].ToString();
                        detail.BlockedAmount = float.Parse(row["BBLOCKSUM1"].ToString());
                        detail.AttachCurrency = row["BBLOCKCUR1"].ToString();

                        if (row["freezed_amount"] != DBNull.Value)
                        {
                            detail.FreezedAmount = float.Parse(row["freezed_amount"].ToString());
                        }

                        details.Add(detail);
                    }

                }
            }
            return details;
        }

        internal static double GetTransitAccountNumberFromCardAccount(double cardAccountNumber)
        {
            double transitAccount = 0;

            string sql = "pr_get_DAHK_product_accounts";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = cardAccountNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        if (row["transitAccount"] != DBNull.Value)
                        {
                            transitAccount = double.Parse(row["transitAccount"].ToString());
                        }
                    }

                }
            }

            return transitAccount;
        }

        internal static List<AccountDAHKfreezeDetails> GetAccountDAHKFreezeDetails(ulong customerNumber, string inquestId, ulong accountNumber)
        {
            List<AccountDAHKfreezeDetails> freezeDetails = new List<AccountDAHKfreezeDetails>();

            string sql = @"SELECT fh.Id, a.request_date, a.MESSAGEID, a.INQUESTID, a.BBLOCKSUM1, a.BBLOCKCUR1, acc.currency, fh.UnUsed_amount, df.freeze_amount, fh.account_number, fh.UnUsed_amount_date
                           FROM [dbo].[Tbl_DAHK_freeze_details] df
                              INNER JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_Attach a ON a.MESSAGEID =df.message_ID
                              LEFT JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_FreeAttach f ON f.MESSAGEID_key=a.MESSAGEID_key
                              INNER JOIN tbl_acc_freeze_history fh ON fh.Id=df.freeze_ID
                              INNER JOIN [tbl_all_accounts;] acc ON acc.Arm_number=fh.account_number
                           WHERE f.MESSAGEID IS NULL AND df.customer_number = @customerNumber AND fh.closing_date IS NULL ";

            if (!String.IsNullOrEmpty(inquestId))
            {
                sql = sql + " AND a.INQUESTID = @inquestId";
            }

            if (accountNumber != 0)
            {
                sql = sql + " AND fh.account_number = @accountNumber";
            }

            sql = sql + " ORDER BY a.request_date";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (!String.IsNullOrEmpty(inquestId))
                    {
                        cmd.Parameters.Add("@inquestId", SqlDbType.NVarChar).Value = inquestId;
                    }

                    if (accountNumber != 0)
                    {
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    }

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        AccountDAHKfreezeDetails freezeDetail = new AccountDAHKfreezeDetails();

                        freezeDetail.FreezeID = long.Parse(row["Id"].ToString());
                        freezeDetail.RequestDate = DateTime.Parse(row["request_date"].ToString());
                        freezeDetail.MessageID = Utility.ConvertAnsiToUnicode(row["MESSAGEID"].ToString());
                        freezeDetail.InquestID = row["INQUESTID"].ToString();
                        freezeDetail.BlockedAmount = float.Parse(row["BBLOCKSUM1"].ToString());
                        freezeDetail.BlockedAmountCurrency = row["currency"].ToString();
                        freezeDetail.FreezedAmount = float.Parse(row["UnUsed_amount"].ToString());
                        freezeDetail.FreezedAmountAMD = float.Parse(row["freeze_amount"].ToString());
                        freezeDetail.FreezedAccount = row["account_number"].ToString();
                        freezeDetail.FreezeDate = DateTime.Parse(row["UnUsed_amount_date"].ToString());
                        freezeDetail.AttachCurrency = row["BBLOCKCUR1"].ToString();

                        freezeDetails.Add(freezeDetail);
                    }

                }
            }
            return freezeDetails;
        }

        internal static ActionResult BlockingAmountFromAvailableAccount(double accountNumber, float blockingAmount, string MessageID, string InquestID, int userID, DateTime operationDate)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[dbo].[pr_blocking_amount_from_available_account]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@accountForFreeze", SqlDbType.BigInt).Value = accountNumber;
                    cmd.Parameters.Add("@amountForFreeze", SqlDbType.Float).Value = blockingAmount;
                    cmd.Parameters.Add("@messageID", SqlDbType.NVarChar).Value = MessageID;
                    cmd.Parameters.Add("@inquestID", SqlDbType.NVarChar).Value = InquestID;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = operationDate;
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = userID;

                    cmd.ExecuteNonQuery();
                }

            }

            result.ResultCode = ResultCode.Normal;

            return result;

        }

        internal static ActionResult MakeAvailable(List<long> freezeIdList, float availableAmount, ushort filialCode, short userId, DateTime operationDate)
        {
            ActionResult result = new ActionResult();

            DataTable dt = new DataTable();
            dt.Columns.Add("value");

            for (int i = 0; i < freezeIdList.Count; i++)
            {
                dt.Rows.Add(freezeIdList[i].ToString());
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[dbo].[pr_make_amount_available]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = operationDate;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@availableAmount", SqlDbType.Float).Value = availableAmount;
                    //cmd.Parameters.Add("@freezeIDList", SqlDbType.Udt).Value = freezeIdList;

                    SqlParameter param = new SqlParameter("@freezeIDList", SqlDbType.Structured)
                    {
                        TypeName = "dbo.BigintTable",
                        Value = dt
                    };

                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                }

            }

            result.ResultCode = ResultCode.Normal;

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի բռնագանձումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<DahkDetails> GetDahkCollections(ulong customerNumber)
        {
            List<DahkDetails> collections = new List<DahkDetails>();

            string sql = @"SELECT c.ID,c.Request_Date,c.MESSAGEID,c.INQUESTNUMBER,a.INQUESTID,c.BRECOVERSUM1  as amount,BRECOVERSUM4 as total_debt,BRECOVERCUR1 as currency,BBLOCKCANCEL as release_remaining,
                            isnull((SELECT top(1) (case when RIGHT(tdc.MESSAGEID,9)=RIGHT(c.MESSAGEID,9) then quality else 0 end) FROM Tbl_customers_dahk_details dd INNER JOIN DAHK_Base.dbo.Tbl_DAHK_customers tdc ON dd.reasonID=tdc.ID WHERE dd.inquestID=a.inquestID and dd.customer_number=dc.customer_number ORDER BY set_ID desc),0) as quality,NULL  as description,
                            case when f.ID is null then 0 else 1 end as isBold
                            FROM DAHK_Base.dbo.Tbl_DAHK_customers dc
                                    INNER JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_Catch c ON dc.message_tbl_ID=c.ID and message_type=2 
                                    INNER JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_Attach a ON c.MESSAGEID_key=a.MESSAGEID_key and a.Status<>7
                                    OUTER APPLY(SELECT top 1 ID FROM DAHK_Base.dbo.Tbl_CheckMsg_Response_FreeAttach WHERE c.MESSAGEID_key= MESSAGEID_key ) f
                            WHERE dc.customer_number =@customerNumber  
                            UNION ALL 
                            SELECT ID,Request_Date,MESSAGEID,INQUESTNUMBER,'0',BRECOVERSUM1,BRECOVERSUM4,BRECOVERCUR1,BBLOCKCANCEL,
                            quality,description,0 FROM Tbl_customers_DAHK_catches WHERE customer_number = @customerNumber";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        DahkDetails collection = SetDahkDetails(row);

                        collections.Add(collection);
                    }

                }
            }

            return collections;
        }


        internal static DahkDetails SetDahkDetails(DataRow row)
        {
            DahkDetails dahkDetails = new DahkDetails();

            dahkDetails.RequestDate = row["Request_Date"] != DBNull.Value ? DateTime.Parse(row["Request_Date"].ToString()) : default(DateTime?);
            dahkDetails.RequestNumber = Utility.ConvertAnsiToUnicode(row["MESSAGEID"].ToString());
            dahkDetails.InquestNumber = row["INQUESTNUMBER"].ToString();
            dahkDetails.InquestCode = row["inquestID"].ToString();

            if (row.Table.Columns.Contains("set_date"))
                dahkDetails.SetDate = row["set_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["set_date"].ToString());

            dahkDetails.Amount = float.Parse(row["amount"].ToString());
            dahkDetails.Currency = row["currency"].ToString();

            if (row.Table.Columns.Contains("set_number"))
                dahkDetails.UserId = int.Parse(row["set_number"].ToString());

            if (row.Table.Columns.Contains("quality"))
            {
                dahkDetails.ActionType = int.Parse(row["quality"].ToString());

                if (dahkDetails.ActionType == 0)
                {
                    dahkDetails.ActionTypeDescription = "Ազատում";
                }
                else if (dahkDetails.ActionType == 1)
                {
                    dahkDetails.ActionTypeDescription = "Արգելադրում";
                }
                else if (dahkDetails.ActionType == 2)
                {
                    dahkDetails.ActionTypeDescription = "Ժամանակավոր ազատում";
                }

            }

            dahkDetails.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());

            if (row.Table.Columns.Contains("total_debt"))
                dahkDetails.TotalDebt = float.Parse(row["total_debt"].ToString());

            if (row.Table.Columns.Contains("release_remaining"))
                dahkDetails.ReleaseRemaining = bool.Parse(row["release_remaining"].ToString());

            if (row.Table.Columns.Contains("isBold"))
                dahkDetails.ShowPriority = int.Parse(row["isBold"].ToString());


            return dahkDetails;
        }

        public static DataTable GetFreezedAccounts(ulong customerNumber)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT DISTINCT a.Arm_number, a.currency, a.type_of_account
                            FROM [tbl_all_accounts;] a
                            INNER JOIN Tbl_acc_freeze_history f ON f.account_number=a.Arm_number
                            INNER JOIN Tbl_DAHK_freeze_details d ON d.freeze_ID=f.Id
                            WHERE a.closing_date IS NULL AND f.closing_date IS NULL AND d.customer_number=@customerNumber";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }

            return dt;
        }

        internal static List<ulong> GetDAHKproductAccounts(ulong accountNumber)
        {
            List<ulong> accounts = new List<ulong>();

            string sql = "pr_get_DAHK_product_accounts";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        accounts.Add(ulong.Parse(row["accountNumber"].ToString()));
                        if (row["availableAmountAccount"] != DBNull.Value)
                        {
                            accounts.Add(ulong.Parse(row["availableAmountAccount"].ToString()));
                        }
                        if (row["cardAccount"] != DBNull.Value)
                        {
                            accounts.Add(ulong.Parse(row["cardAccount"].ToString()));
                        }
                        if (row["transitAccount"] != DBNull.Value)
                        {
                            accounts.Add(ulong.Parse(row["transitAccount"].ToString()));
                        }
                    }

                }
            }

            return accounts;

        }

        /// <summary>
        /// Վերադարձնում է ԴԱՀԿ գործատուներին
        /// </summary>
        internal static List<DahkEmployer> GetDahkEmployers(ulong customerNumber,ProductQualityFilter quality, string inquestId = "")
        {

            List<DahkEmployer> employers = new List<DahkEmployer>();

            string sql = "";

            if (quality == ProductQualityFilter.Opened)
            {
                //                sql = sql + " AND closing_date is null";
                sql = @"SELECT ID, customer_number, MESSAGEID as msg_number, INQUESTID as inquestID, registration_date,  CAST(arm_number AS BIGINT) arm_number, closing_date 
			            FROM (SELECT e.*, null as closing_date FROM Tbl_DAHK_employers_acc e 
						            LEFT JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_FreeAttach f ON f.INQUESTID = e.INQUESTID AND f.MESSAGEID_key=RIGHT(e.MESSAGEID,9) 
						            WHERE f.MESSAGEID IS NULL 
						            UNION ALL 
						            SELECT h.* FROM Tbl_DAHK_employers_acc_hystory h 
						            LEFT JOIN DAHK_Base.dbo.Tbl_CheckMsg_Response_FreeAttach f ON f.INQUESTID = h.INQUESTID AND f.MESSAGEID_key=RIGHT(h.MESSAGEID,9) 
						            LEFT JOIN (SELECT MAX(MESSAGEID_key) AS lastMessageKey, inquestID 
											    FROM DAHK_Base.dbo.Tbl_CheckMsg_Response_Attach 
											    GROUP BY inquestID)  A ON A.inquestID = h.inquestID 
											    WHERE f.MESSAGEID IS NULL AND A.lastMessageKey = RIGHT(h.MESSAGEID,9)) UN 
                        WHERE customer_number = @customerNumber ";
            }
            else
            {
                sql = @"SELECT ID, customer_number, MESSAGEID as msg_number, INQUESTID as inquestID, registration_date, arm_number, closing_date FROM Tbl_DAHK_employers_acc_hystory WHERE customer_number = @customerNumber ";
            }

            if (!String.IsNullOrEmpty(inquestId))
            {
                sql = sql + " AND INQUESTID = @inquestId";
            }

            sql = sql + " ORDER BY INQUESTID";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (!String.IsNullOrEmpty(inquestId))
                    {
                        cmd.Parameters.Add("@inquestId", SqlDbType.NVarChar).Value = inquestId;
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        DahkEmployer employer = new DahkEmployer();
                        employer.CustomerNumber = ulong.Parse(row["customer_number"].ToString());
                        employer.RequestNumber = Utility.ConvertAnsiToUnicode(row["msg_number"].ToString());
                        employer.InquestCode = row["inquestID"].ToString();
                        employer.AccountNumber = row["arm_number"].ToString();
                        if (row["closing_date"] == DBNull.Value)
                        {
                            employer.ClosingDate = default(DateTime?);
                        }
                        else
                        {
                            employer.ClosingDate = Convert.ToDateTime(row["closing_date"].ToString());
                        }


                        employers.Add(employer);
                    }

                }
            }

            return employers;
        }

        /// <summary>
        /// Վերադարձնում է ԴԱՀԿ ընդհանուր գումարները
        /// </summary>
        internal static List<DahkAmountTotals> GetDahkAmountTotals(ulong customerNumber)
        {

            List<DahkAmountTotals> amounts = new List<DahkAmountTotals>();

            string sql = @"SELECT amount,Ma.currency,type,Bt.description AS  blockageTypes,attached_amount , not_attached_amount,customer_number
                             FROM v_customers_DAHK_attach_message_amounts Ma LEFT JOIN Tbl_Type_Of_Blockages BT ON BT.id = ISNULL(Ma.blockage_type, 0) 
                             LEFT JOIN [Tbl_currency;] Cur ON Cur.currency = MA.currency
                             WHERE customer_number=@customerNumber
                             Order By BT.id ,type , ISNULL(Cur.int_code,0)";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        DahkAmountTotals amountTotal = new DahkAmountTotals();
                        amountTotal.Amount = float.Parse(row["amount"].ToString());
                        amountTotal.Currency = row["currency"].ToString();
                        amountTotal.FreezeType = int.Parse(row["type"].ToString());

                        if (amountTotal.FreezeType == 0)
                        {
                            amountTotal.FreezeTypeDescription = "Գումարով";
                        }
                        else if (amountTotal.FreezeType == 1)
                        {
                            amountTotal.FreezeTypeDescription = "Ամբողջությամբ";
                        }

                        amountTotal.BlockageTypeDescription = Utility.ConvertAnsiToUnicode(row["blockageTypes"].ToString());

                        amountTotal.BlockedAmount = float.Parse(row["attached_amount"].ToString());
                        amountTotal.UnBlockedAmount = float.Parse(row["not_attached_amount"].ToString());

                        amounts.Add(amountTotal);
                    }

                }
            }

            return amounts;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ԴԱՀԿ սառեցումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<DAHKFreezing> GetDahkFreezings(ulong customerNumber)
        {
            List<DAHKFreezing> freezings = new List<DAHKFreezing>();

            string sql = @"select   message_ID  , UnUsed_amount , freeze_amount
                        from Tbl_acc_freeze_history F join [Tbl_DAHK_freeze_details] D on F.id=D.freeze_ID 
                        where closing_date is null and reason_type =13 ​ and customer_number = @customerNumber";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        DAHKFreezing freezing = new DAHKFreezing();
                        freezing.InquestNumber = row["message_ID"].ToString();
                        freezing.Amount = Convert.ToDouble(row["UnUsed_amount"]);
                        freezing.AmountAMD = Convert.ToDouble(row["freeze_amount"]);

                        freezings.Add(freezing);
                    }

                }
            }

            return freezings;
        }

        internal static List<DahkDetails> GetDahkDetailsForDigital(ulong customerNumber)
        {
            List<DahkDetails> dahklist = new List<DahkDetails>();
            List<DahkDetails> newlist = new List<DahkDetails>();

            string sql = "pr_customer_DAHK_details_for_digital";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        DahkDetails dahk = new DahkDetails();
                        dahk.ShowPriority = int.Parse(row["isBold"].ToString());
                        if (dahk.ShowPriority == 1)
                        {
                            dahk.Amount = float.Parse(row["amount"].ToString());
                            dahk.SetDate = DateTime.Parse(row["set_date"].ToString());
                            dahk.InquestCode = row["inquestID"].ToString();
                            dahk.BlockedAmount = Convert.ToDouble(row["blocked_amount"]);
                            dahk.BlockedAmountInAMD = Convert.ToDouble(row["blocked_amount_in_amd"]);
                            dahk.BlockedCurency = row["blocked_currency"] != DBNull.Value ? row["blocked_currency"].ToString() : "AMD";
                            dahk.BlockedAmountWithCurrency = Convert.ToDouble(row["blocked_amount"]).ToString() + " " + row["blocked_currency"].ToString();
                            if (dahk.BlockedCurency == "AMD")
                                dahk.BlockedAmountInAMD = dahk.BlockedAmount;
                            dahklist.Add(dahk);
                        }
                    }

                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    for (int i = 0; i < dahklist.Count; i++)
                    {
                        double blockedAmount = dahklist[i].BlockedAmount;
                        double blockedAmountINAMD = dahklist[i].BlockedAmountInAMD;
                        string blockedAmountWithCurrency = dahklist[i].BlockedAmountWithCurrency;
                        if (!keyValuePairs.ContainsKey(dahklist[i].InquestCode))
                        {
                            for (int j = i + 1; j < dahklist.Count; j++)
                            {
                                if (dahklist[i].BlockedCurency == dahklist[j].BlockedCurency && dahklist[i].InquestCode == dahklist[j].InquestCode)
                                {
                                    if (dahklist[i].BlockedCurency == "AMD")
                                    {
                                        blockedAmountINAMD = 0;
                                        blockedAmount += dahklist[j].BlockedAmount;
                                        blockedAmountWithCurrency = blockedAmount + " AMD";
                                    }

                                    else
                                    {
                                        blockedAmountINAMD += dahklist[j].BlockedAmountInAMD;
                                        blockedAmount += dahklist[j].BlockedAmount;
                                        blockedAmountWithCurrency = blockedAmount + dahklist[i].BlockedCurency;
                                    }

                                }
                                else if (dahklist[i].BlockedCurency != dahklist[j].BlockedCurency && dahklist[i].InquestCode == dahklist[j].InquestCode)
                                {
                                    blockedAmountINAMD += dahklist[j].BlockedAmountInAMD;
                                    blockedAmountWithCurrency += (" " + dahklist[j].BlockedAmountWithCurrency);
                                }
                            }
                            keyValuePairs.Add(dahklist[i].InquestCode, blockedAmountWithCurrency + "," + blockedAmountINAMD
                                + "," + dahklist[i].SetDate + "," + dahklist[i].Amount);
                        }
                    }

                    newlist = keyValuePairs.Select(p => new DahkDetails { InquestCode = p.Key, BlockedAmountWithCurrency = p.Value }).ToList();

                    for (int i = 0; i < newlist.Count; i++)
                    {
                        List<string> items = newlist[i].BlockedAmountWithCurrency.Split(',').ToList<string>();

                        newlist[i].BlockedAmountWithCurrency = items[0];

                        string[] currencies = newlist[i].BlockedAmountWithCurrency.Split(' ');


                        if (currencies.Length == 2 && currencies[1] == "AMD")
                            newlist[i].BlockedAmountInAMD = 0;
                        else
                            newlist[i].BlockedAmountInAMD = Convert.ToDouble(items[1]);
                        newlist[i].SetDate = DateTime.Parse(items[2].ToString());
                        newlist[i].Amount = float.Parse(items[3].ToString());
                    }

                }
            }

            return newlist;
        }

        internal static ShowNewDahk ShowDAHKMessage(ulong customerNumber)
        {
            ShowNewDahk dahk = new ShowNewDahk();

            dahk.ActiveDahkList = new List<string>();

            string sql = @"SELECT (CASE WHEN (ld.lsetID = d.set_ID AND d.quality = 1 or (ld.lsetID = d.set_ID AND d.quality = 0 and IsTemporar = 1)) THEN 1	 
						WHEN (d.quality =1 AND NOT EXISTS(SELECT 1 FROM Tbl_customers_dahk_details	WHERE customer_number = d.customer_number AND inquestID = d.inquestID AND 
                        blockage_type = d.blockage_type  AND (quality = 1 OR (quality = 0 AND IsTemporar = 0 )) AND set_ID >d.set_ID))  THEN 2	   ELSE   0  END) AS isBold ,
                        d.inquestid  FROM Tbl_customers_dahk_details d INNER JOIN (SELECT INQUESTID,customer_number,blockage_type,MAX(set_ID) AS lsetID 
                        FROM Tbl_customers_dahk_details WHERE blockage_type=0 GROUP BY INQUESTID,customer_number,blockage_type) ld ON d.INQUESTID=ld.INQUESTID AND
                        d.customer_number=ld.customer_number inner join tbl_dahk_customers c on d.reasonID=c.ID inner join v_DAHK_attach_message_amounts  v	
                        on c.message_tbl_ID = v.message_tbl_ID OUTER APPLY(SELECT h.UnUsed_amount freezed_amount,CASE WHEN ISNULL(f.kurs_for_account_currency,0)<>1 THEN
                        f.freeze_amount ELSE 0 END freezed_amount_in_amd, all_acc.currency blocked_currency FROM Tbl_DAHK_freeze_details f INNER JOIN Tbl_acc_freeze_history h
                        ON h.ID = f.freeze_ID INNER JOIN [tbl_all_accounts;] all_acc on h.account_number = all_acc.Arm_number WHERE f.customer_number = d.customer_number AND
                        f.inquestID=d.INQUESTID	AND f.closing_message_table_ID IS NULL AND f.closing_message_type IS NULL AND h.closing_date IS NULL) DF
                        where d.customer_number = @customerNumber and d.INQUESTID not in (select inquest_code from tbl_dahk_read_inquests where isread=1) and message_type=1";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int isBold = int.Parse(dt.Rows[i]["isBold"].ToString());
                        if (isBold == 1)
                        {
                            DataRow row = dt.Rows[i];
                            string inquestCode = row["inquestID"].ToString();
                            dahk.ActiveDahkList.Add(inquestCode);
                        }
                    }

                    if(dahk.ActiveDahkList.Count > 0)
                        dahk.HasNewDAHK = true;

                }
            }
            return dahk;
        }

        internal static void MakeDAHKMessageRead(List<string> inquestCodes, ulong customerNumber)
        {
            foreach (var Code in inquestCodes)
            {
                string sql = @"SELECT  inquest_code FROM tbl_dahk_read_inquests 
						where isread = 1 and customer_number = @customerNumber and inquest_code = @inquestCode";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@inquestCode", SqlDbType.NVarChar).Value = Code;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    string sql1 = dt.Rows.Count == 0 ? @"insert into tbl_dahk_read_inquests values(@inquestCode, @customerNumber, 1)" : @"update tbl_dahk_read_inquests set isread = 1 where customer_number = @customerNumber and inquest_code = @inquestCode";
                    SqlCommand cmd1 = new SqlCommand(sql1, conn);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd1.Parameters.Add("@inquestCode", SqlDbType.NVarChar).Value = Code;
                    cmd1.ExecuteNonQuery();
                }
            }
        }

    }

}
