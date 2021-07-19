using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    /// <summary>
    /// class PeriodicTransferDB նկարագրում է ՊԱՐԲԵՐԱԿԱՆ ՓՈԽԱՆՑՈՒՄ տվյալների բազայի մակարդակի վրա
    /// </summary>
    internal class PeriodicTransferDB
    {

        /// <summary>
        /// Պարբերական փոխանցումների տվյալների բազայից կարդացում ըստ հաճախորդի համարի 
        /// </summary>

        /// <param name="customerNumber">
        /// Հաճախորդի համար
        /// </param> 

        /// <returns>
        /// Վերադարձնում է List<PeriodicTransfer> տեսակի օբյեկտ` գործող պարբերական փոխանցուևմների ցանկ մեկ հաճախորդի համար  
        /// </returns>

        internal static List<PeriodicTransfer> GetPeriodicTransfers(ulong customerNumber)
        {
            List<PeriodicTransfer> periodicTransfers = new List<PeriodicTransfer>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = PeriodicTransferDB.GetSelectForPeriodicTransfer() +
                            " WHERE Quality = 1 AND Customer_Number =@customerNumber ORDER BY Date_of_beginning";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        periodicTransfers = new List<PeriodicTransfer>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        PeriodicTransfer periodicTransfer = SetPeriodicTransfer(row);

                        periodicTransfers.Add(periodicTransfer);
                    }
                }
            }

            return periodicTransfers;
        }
        internal static List<PeriodicTransfer> GetClosedPeriodicTransfers(ulong customerNumber)
        {
            List<PeriodicTransfer> periodicTransfers = new List<PeriodicTransfer>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = PeriodicTransferDB.GetSelectForPeriodicTransfer() +
                            " WHERE Quality = 0 AND Customer_Number =@customerNumber ORDER BY Date_of_beginning";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        periodicTransfers = new List<PeriodicTransfer>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        PeriodicTransfer periodicTransfer = SetPeriodicTransfer(row);

                        periodicTransfers.Add(periodicTransfer);
                    }
                }
            }

            return periodicTransfers;
        }


        /// <summary>
        /// Մեկ պարբերական փոխանցման կարդացում ըստ ունիկալ համարի և հաճախորդի համարի 
        /// </summary>

        /// <param name="customerNumber">
        /// Հաճախորդի համար
        /// </param> 

        /// <param name="productId">
        /// Ունիկալ համար
        /// </param> 

        /// <returns>
        /// Վերադարձնում է PeriodicTransfer տեսակի օբյեկտ` ըստ ունիկալ համարի և հաճախորդի համարի
        /// </returns>
        internal static PeriodicTransfer GetPeriodicTransfer(ulong productId, ulong customerNumber)
        {
            PeriodicTransfer oneTransfer = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = PeriodicTransferDB.GetSelectForPeriodicTransfer() + " WHERE app_ID =@productId AND Customer_Number =@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        oneTransfer = SetPeriodicTransfer(row);
                    }

                }
            }

            return oneTransfer;
        }



        /// <summary>
        /// Պարբերական փոխանցումների տվյալների բազայից կարդացում ըստ ունիկալ համարի (app_ID)
        /// </summary>

        /// <param name="productId">
        /// Ունիկալ համար (app_ID)
        /// </param> 

        /// <returns>
        /// Վերադարձնում է PeriodicTransfer տեսակի օբյեկտ `մեկ գործող պարբերական փոխանցում ըստ ունիկալ համարի (app_ID)  
        /// </returns>

        internal static PeriodicTransfer GetPeriodicTransfer(ulong productId)
        {
            PeriodicTransfer oneTransfer = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = PeriodicTransferDB.GetSelectForPeriodicTransfer() + " WHERE Quality = 1 AND app_ID =@productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        oneTransfer = SetPeriodicTransfer(row);
                    }

                }
            }

            return oneTransfer;
        }


        /// <summary>
        /// Պարբերական փոխանցման տվյալների ստացնան համար SELECT տողը, որը օգտագործվում է մի քանի տեղ
        /// </summary>

        /// <returns>
        /// Վերադարձնում է string տեսակ   
        /// </returns>

        internal static string GetSelectForPeriodicTransfer()
        {


            string sql = @"Select abonent_type, App_Id, doc_number,
                                     S.Transfer_type as TransferType,
                                     dbo.fnc_convertAnsiToUnicode(T.Description) AS transferTypeDescription,
                                     debet_account,credit_account AS creditInternalAccount,
                                     CASE WHEN S.Transfer_type=2
                                                   THEN S.receiver_account
                                                   ELSE cast(cast(S.credit_account as bigint) AS varchar(100))
		                                           END AS creditExternalAccount, 
                                    CASE WHEN isnull(date_of_normal_end,'')='' THEN 1 ELSE 2 END AS duration_type,
                                    dbo.fnc_convertAnsiToUnicode(DT.description) AS duration_type_description,
                                    S.date_of_beginning,S.date_of_normal_end,Currency, 
                                    Case When Total_Rest = 0 THEN 0 Else case when S.Transfer_type > 2 then 1 ELSE 2 END END AS charge_type, 
                                    dbo.fnc_convertAnsiToUnicode(CH.description) as charge_type_description,
                                    S.amount,FindField1 as abonent_number,FindField2 as abonent_add_inf,FilialCode,
                                    dbo.fnc_convertAnsiToUnicode(S.description) as TransferDescription,
                                    first_repayment_date,period,
                                    Partial_Payments,Receiver_bank_swift,Max_Amount,Min_Amount,Minimal_Rest,
                                    Sender_Code_Of_Tax,Sender_Reg_Code,
                                    Credit_Bank_Code,dbo.fnc_convertAnsiToUnicode(Receiver_Name) AS Receiver_Name,
                                    police_code,payment_fee,receiver_code_of_tax,
                                    deb_for_transfer_payment as fee_Account,
                                    S.amount_type as AmountSubTypeByPurpose,
                                    dbo.fnc_convertAnsiToUnicode(AT.Description) AS AmountSubTypeByPurposeDescription,
                                    transfer_purpose, 
                                    dbo.fnc_convertAnsiToUnicode(TP.purpose) AS TransferPurposeDescription,       
                                    Check_Days_Count,PayIfNoDebt,S.date_of_operation,
                                    HB_doc_ID,HB_doc_ID_Termination,nn ,
                                    s.existence_of_circulation,
                                    s.Editing_Date
                                    FROM [Tbl_operations_by_period] S INNER JOIN 
                                    Tbl_operations_by_period_Types T 
                                    ON S.Transfer_Type = T.ID_Transfer
                                    LEFT JOIN tbl_type_of_transfer_purpose TP
                                    ON S.transfer_purpose = TP.ID  
                                    LEFT JOIN Tbl_Op_By_Per_Amount_Types AT
                                    ON S.Transfer_Type = AT.Transfer_Type AND S.Amount_Type = AT.Amount_Type 
                                    LEFT JOIN Tbl_Operations_by_period_duration_types DT 
                                    ON DT.id=Case when isnull(date_of_normal_end,'')='' THEN 1 ELSE 2 END
                                    LEFT JOIN dbo.Tbl_Operations_by_period_charges_types CH ON CH.id=
                                    CASE WHEN Total_Rest = 0 THEN 0 ELSE
                                    CASE WHEN S.Transfer_type > 2 THEN 1 ELSE 2 END END";

            return sql;
        }


        private static PeriodicTransfer SetPeriodicTransfer(DataRow row)
        {
            PeriodicTransfer periodicTransfer = new PeriodicTransfer();

            if (row != null)
            {
                periodicTransfer.AbonentType = byte.Parse(row["abonent_type"].ToString());
                periodicTransfer.ProductId = ulong.Parse(row["app_id"].ToString());
                periodicTransfer.DocumentNumber = ulong.Parse(row["doc_number"].ToString());
                periodicTransfer.Type = int.Parse(row["TransferType"].ToString());
                periodicTransfer.TypeDescription = row["transferTypeDescription"].ToString();
                periodicTransfer.Description = row["TransferDescription"].ToString();
                periodicTransfer.DebitAccount = Account.GetSystemAccountWithoutBallance(row["debet_account"].ToString());
                periodicTransfer.CreditInternalAccount = Account.GetSystemAccountWithoutBallance(row["creditInternalAccount"].ToString());
                periodicTransfer.CreditAccount = row["creditExternalAccount"].ToString();
                periodicTransfer.DurationType = int.Parse(row["duration_type"].ToString());
                periodicTransfer.DurationTypeDescription = row["duration_type_description"] != DBNull.Value ? row["duration_type_description"].ToString() : string.Empty;
                periodicTransfer.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                periodicTransfer.EndDate = row["date_of_normal_end"] != DBNull.Value ? DateTime.Parse(row["date_of_normal_end"].ToString()) : default(DateTime?);
                periodicTransfer.EditingDate = row["Editing_Date"] != DBNull.Value ? DateTime.Parse(row["Editing_Date"].ToString()) : default(DateTime?);
                periodicTransfer.Currency = row["Currency"].ToString();
                periodicTransfer.ChargeType = int.Parse(row["charge_type"].ToString());
                periodicTransfer.ChargeTypeDescription = row["charge_type_description"].ToString();
                periodicTransfer.Amount = double.Parse(row["amount"].ToString());
                periodicTransfer.AbonentNumber = row["abonent_number"].ToString();
                periodicTransfer.AbonentAddInf = row["abonent_add_inf"] != DBNull.Value ? row["abonent_add_inf"].ToString() : "".ToString();
                periodicTransfer.FirstTransferDate = DateTime.Parse(row["first_repayment_date"].ToString());
                periodicTransfer.Periodicity = int.Parse(row["period"].ToString());
                periodicTransfer.FilialCode = ulong.Parse(row["FilialCode"].ToString());
                periodicTransfer.PartialPaymentSign = byte.Parse(row["Partial_Payments"].ToString());
                periodicTransfer.MaxAmountLevel = double.Parse(row["Max_Amount"].ToString());
                periodicTransfer.MinAmountLevel = double.Parse(row["Min_Amount"].ToString());
                periodicTransfer.MinDebetAccountRest = double.Parse(row["Minimal_Rest"].ToString());
                periodicTransfer.AmountSubTypeByPurpose = row["AmountSubTypeByPurpose"] != DBNull.Value ? byte.Parse(row["AmountSubTypeByPurpose"].ToString()) : byte.Parse("0".ToString());
                periodicTransfer.AmountSubTypeByPurposeDescription = row["AmountSubTypeByPurposeDescription"] != DBNull.Value ? row["AmountSubTypeByPurposeDescription"].ToString() : "".ToString();
                periodicTransfer.FeeAmount = row["payment_fee"] != DBNull.Value ? double.Parse(row["payment_fee"].ToString()) : double.Parse("0".ToString());
                periodicTransfer.FeeAccount = row["fee_Account"] != DBNull.Value ? Account.GetSystemAccountWithoutBallance(row["fee_Account"].ToString()) : null;
                periodicTransfer.SenderCodeOfTax = row["Sender_Code_Of_Tax"] != DBNull.Value ? row["Sender_Code_Of_Tax"].ToString() : "".ToString();
                periodicTransfer.SenderRegionCode = row["Sender_Reg_Code"] != DBNull.Value & row["Sender_Reg_Code"].ToString() != "" ? ulong.Parse(row["Sender_Reg_Code"].ToString()) : 0;
                periodicTransfer.ReceiverBankCode = ulong.Parse(row["Credit_Bank_Code"].ToString());
                periodicTransfer.ReceiverBankSwiftCode = row["Receiver_bank_swift"] != DBNull.Value ? row["Receiver_bank_swift"].ToString() : "".ToString();
                periodicTransfer.ReceiverName = row["Receiver_Name"] != DBNull.Value ? row["Receiver_Name"].ToString() : "".ToString();
                periodicTransfer.ReceiverCodeOfTax = row["receiver_code_of_tax"] != DBNull.Value ? row["receiver_code_of_tax"].ToString() : "".ToString();
                periodicTransfer.TransferPurposeCode = row["transfer_purpose"] != DBNull.Value ? int.Parse(row["transfer_purpose"].ToString()) : 0;
                periodicTransfer.TransferPurposeDescription = row["TransferPurposeDescription"] != DBNull.Value ? row["TransferPurposeDescription"].ToString() : "".ToString();
                periodicTransfer.TransferPoliceCode = int.Parse(row["police_code"].ToString());
                periodicTransfer.CheckDaysCount = ushort.Parse(row["Check_Days_Count"].ToString());
                periodicTransfer.PayIfNoDebt = ushort.Parse(row["PayIfNoDebt"].ToString());
                periodicTransfer.LastOperationDate = row["date_of_operation"] != DBNull.Value ? DateTime.Parse(row["date_of_operation"].ToString()) : default(DateTime?);
                periodicTransfer.HBDocID = ulong.Parse(row["HB_doc_ID"].ToString());
                periodicTransfer.TerminationHBDocID = ulong.Parse(row["HB_doc_ID_Termination"].ToString());
                periodicTransfer.Number = ulong.Parse(row["nn"].ToString());
                periodicTransfer.ExistenceOfCirculation = row["existence_of_circulation"] != DBNull.Value ? Convert.ToBoolean(row["existence_of_circulation"]) : false;
                if (periodicTransfer.Type == 1)
                {
                    ulong DebitAccountCustomerNumber = AccountDB.GetAccountCustomerNumber(periodicTransfer.DebitAccount.AccountNumber);
                    ulong CreditInternalCustomerNumber = AccountDB.GetAccountCustomerNumber(periodicTransfer.CreditInternalAccount.AccountNumber);

                    if (DebitAccountCustomerNumber == CreditInternalCustomerNumber)
                        periodicTransfer.SubType = 1;
                    else 
                        periodicTransfer.SubType = 2;

                    Account account = Account.GetSystemAccountWithoutBallance(periodicTransfer.CreditAccount);
                    if (account != null)
                    {
                        if (account.IsCardAccount())
                        {
                            Card card = Card.GetCardWithOutBallance(account.AccountNumber);
                            periodicTransfer.CrediAccountDescription = card.CardNumber + " " + card.CardType + " " + account.AccountDescription;
                        }
                        else
                        {
                            periodicTransfer.CrediAccountDescription = account.AccountType != 11 ? account.AccountTypeDescription + " " + account.AccountDescription : account.AccountDescription;
                        }
                    }

                }




            }
            return periodicTransfer;
        }
        internal static List<PeriodicTransferHistory> GetHistory(long ProductId, DateTime dateFrom, DateTime dateTo)
        {

            List<PeriodicTransferHistory> transferHistory = new List<PeriodicTransferHistory>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@" SELECT L.app_id,Reg_Date,Oper_Result,Deny_Reason,
                                                    Case when L.currency='AMD' then L.Amount else L.Amount_Currency end as amount,L.Currency 
                                                    from Tbl_operations_by_period_log L INNER JOIN
                                                    Tbl_operations_by_period P ON L.App_Id=P.App_Id 
                                                    WHERE L.App_Id=@app_id And Reg_Date>=@dateFrom  and Reg_Date<=@dateTo ORDER BY Reg_Date ASC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = ProductId;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.DateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.DateTime).Value = dateTo;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        PeriodicTransferHistory transfer = new PeriodicTransferHistory();
                        transfer.RegistrationDate = Convert.ToDateTime(dr["Reg_Date"]);
                        transfer.Currency = (dr["Currency"]).ToString();
                        transfer.OperationResult = Convert.ToInt16(dr["Oper_Result"]);
                        transfer.OperationResultDescription = (dr["Deny_Reason"]).ToString();
                        transfer.OperationResultDescription = Utility.ConvertAnsiToUnicode(transfer.OperationResultDescription);
                        transfer.Amount = Convert.ToDouble(dr["amount"]);
                        transferHistory.Add(transfer);
                    }
                }
            }
            return transferHistory;
        }
        public static ActionResult AddPeriodicTransferLog(PeriodicTransfer periodicTransfer, int operResult, string denyReason)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_insert_Into_operations_by_period_log";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@transferType", SqlDbType.Int).Value = periodicTransfer.Type;
                    cmd.Parameters.Add("@transferID", SqlDbType.Int).Value = periodicTransfer.DocumentNumber;

                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = periodicTransfer.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@dateOfBeginning", SqlDbType.SmallDateTime).Value = periodicTransfer.StartDate;
                    if (periodicTransfer.EndDate != null)
                        cmd.Parameters.Add("@dateOfNormalEnd", SqlDbType.SmallDateTime).Value = periodicTransfer.EndDate;
                    else
                        cmd.Parameters.Add("@dateOfNormalEnd", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.Parameters.Add("@firstRepaymentDate", SqlDbType.SmallDateTime).Value = periodicTransfer.FirstTransferDate;
                    if (periodicTransfer.LastOperationDate != null)
                        cmd.Parameters.Add("@dateOfLastOper", SqlDbType.SmallDateTime).Value = periodicTransfer.LastOperationDate;
                    else
                        cmd.Parameters.Add("@dateOfLastOper", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.Parameters.Add("@operResult", SqlDbType.TinyInt).Value = operResult;
                    cmd.Parameters.Add("@denyReason", SqlDbType.NVarChar).Value = denyReason;
                     cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = periodicTransfer.Currency;
                     cmd.Parameters.Add("@amountByDocument", SqlDbType.Float).Value = periodicTransfer.Amount;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = periodicTransfer.CreditAccount;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = periodicTransfer.FilialCode;
                    cmd.Parameters.Add("@partialPayment", SqlDbType.Int).Value = periodicTransfer.PartialPaymentSign;
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = periodicTransfer.ProductId;
                    cmd.Parameters.Add("@minimalRest", SqlDbType.Float).Value = periodicTransfer.MinDebetAccountRest;
 
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }
            internal static ActionResult PeriodicTransfersComplete(DateTime dateOfOperation, DateTime dayOfRateCalculation, ulong appID)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_operations_by_period 
											SET date_of_operation = @dateOfOperation,[last_day_of rate calculation] = @dayOfRateCalculation 	
											WHERE App_Id = @appID ", conn))
                {
                    cmd.Parameters.Add("@dateOfOperation", SqlDbType.DateTime).Value = dateOfOperation;
                    cmd.Parameters.Add("@dayOfRateCalculation", SqlDbType.DateTime).Value = dayOfRateCalculation;
                    cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;

                    conn.Open();

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }

        internal static ActionResult PeriodicTransfersClosed(ulong appID)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_operations_by_period SET quality = 0 WHERE App_Id = @appID ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = appID;
                                      
                    conn.Open();

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }

        internal static ActionResult CloseExpiredPeriodicTransfers(DateTime currentOperDay, DateTime orderDate)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"UPDATE  Tbl_operations_by_period SET quality = 0 , Editing_Date = @currentOperDay  
                                                            WHERE (date_of_normal_end < = @orderDate OR date_of_normal_end < = @currentOperDay) AND ISNULL(quality,0) = 1", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@currentOperDay", SqlDbType.DateTime).Value = currentOperDay;
                    cmd.Parameters.Add("@orderDate", SqlDbType.DateTime).Value = orderDate;

                    conn.Open();

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }

        }
        internal static int GetTransferTypeByAppId(ulong appId)
        {
            int TransferType = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandText = "SELECT App_Id, Transfer_Type FROM [dbo].Tbl_operations_by_period where App_Id = @app_id";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = appId;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        TransferType = Convert.ToInt32(dr["Transfer_Type"].ToString());
                    }
                }
            }

            return TransferType;
        }

        internal static int GetPeriodicTransfersCount(ulong customerNumber, PeriodicTransferTypes transferType)
        {
            int count = 0;
            string typeCondition = "";
            switch (transferType)
            {
                case PeriodicTransferTypes.Transfer:
                    typeCondition = @"AND transfer_type in (1,2)";
                    break;
                case PeriodicTransferTypes.Payment:
                    typeCondition = @"AND transfer_type in (3,4,6,7,8,10,11,12,17)";
                    break;
            }
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT COUNT(transfer_Type) as Count
                               FROM [Tbl_operations_by_period]
                               WHERE Quality = 1 AND Customer_Number = @customerNumber " + typeCondition;

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            count = Convert.ToInt32(dr["Count"].ToString());
                        }
                    }
                }
            }
            return count;
        }
    }
}
