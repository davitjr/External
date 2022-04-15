using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking
{
    internal class CashBookOrderDB
    {
        public static ActionResult Save(CashBookOrder order, List<CashBook> cashBooks, string userName, short userId, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_HB_documents (
                                                    registration_date,document_type,document_number,document_subtype,
                                                    debet_account, credit_account,
                                                    quality, source_type,operationFilialCode,operation_date,filial) 
                                                    VALUES(@registrationDate,@documentType,
                                                    @documentNumber,@documentSubType,@debetAccount,
                                                    @creditAccount, @quality,
                                                    @sourceType,@operationFilialCode,@oper_day,@filial) SELECT @ID = SCOPE_IDENTITY()", conn))
                {
                    cmd.Parameters.AddWithValue("@operationFilialCode", (short)order.FilialCode);
                    cmd.Parameters.AddWithValue("@filial", (short)order.FilialCode);
                    cmd.Parameters.AddWithValue("@registrationDate", order.RegistrationDate);
                    cmd.Parameters.AddWithValue("@documentType", (short)order.Type);
                    cmd.Parameters.AddWithValue("@documentNumber", order.OrderNumber);
                    cmd.Parameters.AddWithValue("@documentSubType", order.SubType);

                    cmd.Parameters.AddWithValue("@debetAccount", order.DebitAccount.AccountNumber);
                    cmd.Parameters.AddWithValue("@creditAccount", order.CreditAccount.AccountNumber);

                    cmd.Parameters.AddWithValue("@quality", (short)1);
                    cmd.Parameters.AddWithValue("@sourceType", (short)source);
                    cmd.Parameters.AddWithValue("@username", userName);
                    cmd.Parameters.AddWithValue("@oper_day", order.OperationDate);

                    cmd.Parameters.Add("@ID", SqlDbType.BigInt);
                    cmd.Parameters["@ID"].Direction = ParameterDirection.Output;

                    conn.Open();

                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt32(cmd.Parameters["@ID"].Value);
                    order.Quality = OrderQuality.Draft;

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT INTO tbl_cash_book_order_details 
                    (doc_ID,amount, addinf, currency, debit_credit, filialcode, 
                    set_number,cor_set_number,debet_account,credit_account,cash_book_ID,status,row_type,cor_account_type) 
                    VALUES(@docID,@amount, @addinf, @currency, @debit_credit, @filialcode, @set_number, @cor_set_number,
                    @debetAccount,@creditAccount,@cashBookID,@status,@type,@CorAccountType)";
                    foreach (CashBook cashBook in cashBooks)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@docID", order.Id);

                        cmd.Parameters.AddWithValue("@amount", cashBook.Amount);




                        if (String.IsNullOrWhiteSpace(cashBook.Description))
                        {
                            cmd.Parameters.AddWithValue("@addinf", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addinf", cashBook.Description);
                        }
                        cmd.Parameters.AddWithValue("@currency", cashBook.Currency);
                        cmd.Parameters.AddWithValue("@debit_credit", cashBook.OperationType == 1 ? 'd' : 'c');
                        cmd.Parameters.AddWithValue("@filialcode", cashBook.FillialCode);
                        cmd.Parameters.AddWithValue("@debetAccount", order.DebitAccount.AccountNumber);
                        cmd.Parameters.AddWithValue("@creditAccount", order.CreditAccount.AccountNumber);

                        if (cashBook.Type == 1 || cashBook.Type == 3)
                        {
                            cmd.Parameters.AddWithValue("@status", 2);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@status", cashBook.Quality);
                        }
                        cmd.Parameters.AddWithValue("@type", cashBook.Type);
                        if (cashBook.ID == 0)
                        {
                            cmd.Parameters.AddWithValue("@cor_set_number", cashBook.CorrespondentSetNumber);
                            cmd.Parameters.AddWithValue("@set_number", cashBook.RegisteredUserID);
                            cmd.Parameters.AddWithValue("@cashBookID", DBNull.Value);

                        }
                        else if (cashBook.Quality == 2 || cashBook.Quality == -2 || cashBook.Quality == 1 || cashBook.Quality == -1 || cashBook.Quality == 0)
                        {
                            if (cashBook.Type == 0)
                            {
                                cmd.Parameters.AddWithValue("@set_number", cashBook.CorrespondentSetNumber);
                            }
                            if (cashBook.Type == 2 && (order.Type == OrderType.CashBookSurPlusDeficitClosing || order.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing))
                            {
                                cmd.Parameters.AddWithValue("@cor_set_number", 0);
                                //cmd.Parameters.AddWithValue("@set_number", cashBook.CorrespondentSetNumber);
                                cmd.Parameters.AddWithValue("@set_number", userId);
                            }
                            else if (cashBook.Type == 2 && order.Type == OrderType.CashBookSurPlusDeficitClosingApprove)
                            {
                                cmd.Parameters.AddWithValue("@cor_set_number", cashBook.CorrespondentSetNumber);
                                cmd.Parameters.AddWithValue("@set_number", 0);
                            }
                            else if (cashBook.Type == 4 && order.Type == OrderType.CashBookSurPlusDeficitClosingApprove)
                            {
                                cmd.Parameters.AddWithValue("@cor_set_number", cashBook.CorrespondentSetNumber);
                                cmd.Parameters.AddWithValue("@set_number", 0);
                            }
                            else if (cashBook.Type == 4 && (order.Type == OrderType.CashBookSurPlusDeficitClosing || order.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing))
                            {
                                cmd.Parameters.AddWithValue("@cor_set_number", 0);
                                //cmd.Parameters.AddWithValue("@set_number", cashBook.CorrespondentSetNumber);
                                cmd.Parameters.AddWithValue("@set_number", userId);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@cor_set_number", cashBook.RegisteredUserID);
                            }

                            if (cashBook.Type == 1 || cashBook.Type == 3)
                            {
                                cmd.Parameters.AddWithValue("@set_number", 0);
                            }
                            cmd.Parameters.AddWithValue("@cashBookID", cashBook.ID);
                        }


                        if (cashBook.Type == 1 || cashBook.Type == 3 || cashBook.Type == 2 || cashBook.Type == 4)
                        {
                            cmd.Parameters.AddWithValue("@CorAccountType", order.CorrespondentAccountType);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@CorAccountType", 0);
                        }
                        if (order.Type == OrderType.CashBookDelete && cashBook.Quality == -3)
                        {
                            cmd.Parameters.AddWithValue("@cor_set_number", cashBook.CorrespondentSetNumber);
                            cmd.Parameters.AddWithValue("@set_number", cashBook.RegisteredUserID);
                            cmd.Parameters.AddWithValue("@cashBookID", cashBook.ID);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

            }
            result.ResultCode = ResultCode.Normal;
            return result;
        }


        public static List<double> GetOperationAccounts(CashBook cashBook, int userFilialCode = 0, string isHeadCashBook = "")
        {
            double debitAccountNumber = 0;
            double creditAccountNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_get_operation_account ", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@currency", cashBook.Currency);

                    if (cashBook.Type == 2 || cashBook.Type == 4)
                    {
                        cashBook.CashBookFilialCodeForLinkedRow = CashBook.GetCashBookFilialCodeByLinkedRow(cashBook.ID);
                    }

                    if (cashBook.FillialCode == -1)
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", 22000);
                        cmd.Parameters.AddWithValue("@accountType", 1011);
                    }
                    else if (cashBook.Type == 1)    // Ավելցուկի մուտքագրում
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", cashBook.FillialCode);
                        cmd.Parameters.AddWithValue("@accountType", 1); //  Կանխիկ դրամ դրամարկղում
                    }
                    else if (cashBook.Type == 2 && isHeadCashBook == "1")   // Ավելցուկի փակում, Երբ դրամվարիչն է
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", userFilialCode);    //  դրամարկղի հաշիվը որոշվում է userFilialCode-ով
                        cmd.Parameters.AddWithValue("@accountType", 1); //  Կանխիկ դրամ դրամարկղում
                    }
                    else if (cashBook.Type == 3)
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", cashBook.FillialCode);
                        cmd.Parameters.AddWithValue("@accountType", 1); //  Կանխիկ դրամ դրամարկղում
                    }
                    else if (cashBook.Type == 4 && isHeadCashBook == "1")   //  Պակասորդի փակում, Երբ դրամվարիչն է
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", userFilialCode);    //  դրամարկղի հաշիվը որոշվում է userFilialCode-ով
                        cmd.Parameters.AddWithValue("@accountType", 1); //  Կանխիկ դրամ դրամարկղում
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@operationFilialCode", cashBook.FillialCode);
                        cmd.Parameters.AddWithValue("@accountType", 1); //  Կանխիկ դրամ դրամարկղում
                    }


                    SqlParameter patam = new SqlParameter("@account", SqlDbType.Float);
                    cmd.Parameters.Add(patam).Direction = ParameterDirection.Output;


                    //Եթե ավելցուկ է ապա դրամարկղի հաշիվը դեբետագրվում է
                    //Եթե պակասորդ է ապա դրամարկղի հաշիվը կրեդիտագրվում է 
                    //******դրամարկղի հաշիվ*******//
                    cmd.ExecuteNonQuery();  //  Վերադարձվում է դրամարկղի հաշիվը

                    if (cashBook.Type == 1) // Ավելցուկի մուտքագրում
                    {
                        debitAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                        cmd.Parameters["@accountType"].Value = 3018;    //  ´³ÝÏÇ ³ßË³ï³ÏÇóÝ»ñÇ Ùáï Ñ³ÛïÝ³µ»ñí³Í ³í»ÉóáõÏ ·áõÙ³ñÝ»ñ
                    }
                    else if (cashBook.Type == 2) // Ավելցուկի փակում
                    {
                        cmd.Parameters["@accountType"].Value = 3018;
                        creditAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                    }
                    else if (cashBook.Type == 4) // Պակասորդի փակում
                    {
                        debitAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                        cmd.Parameters["@accountType"].Value = 1913;    //  Բանկի աշխատակիցների մոտ հայտնաբերված պակասորդ գումարներ
                    }
                    else
                    {
                        creditAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                        cmd.Parameters["@accountType"].Value = 1913;
                    }
                    //*************//



                    if (cashBook.FillialCode == -1)
                    {
                        cmd.Parameters["@operationFilialCode"].Value = 22000;
                    }
                    else if ((cashBook.Type == 2 || cashBook.Type == 4) && isHeadCashBook == "1")
                    {
                        cmd.Parameters["@operationFilialCode"].Value = cashBook.CashBookFilialCodeForLinkedRow;
                    }
                    else
                    {
                        cmd.Parameters["@operationFilialCode"].Value = cashBook.FillialCode;
                    }


                    //Եթե ավելցուկ է ապա տարանցիկ հաշիվը կրեդիտագրվում է
                    //Եթե պակասորդ է ապա տարանցիկ հաշիվը դեբետագրվում է
                    //******տարանցիկ հաշիվ*******//
                    cmd.ExecuteNonQuery();

                    if (debitAccountNumber == 0)
                    {
                        debitAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                    }
                    else
                    {
                        creditAccountNumber = Convert.ToDouble(cmd.Parameters["@account"].Value);
                    }
                    //*************//

                }
            }
            return new List<double>() { debitAccountNumber, creditAccountNumber };
        }

        internal static ActionResult ChangeCashBookStatus(int cashBookID, int setnumber, int newStatus, int oldStatus)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_cash_book_status_change", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@cash_book_ID", cashBookID);
                    cmd.Parameters.AddWithValue("@set_number", setnumber);
                    cmd.Parameters.AddWithValue("@new_status", newStatus);
                    cmd.Parameters.AddWithValue("@old_status", oldStatus);
                    cmd.ExecuteNonQuery();
                }

            }
            result.ResultCode = ResultCode.Normal;
            return result;
        }


        public static Account GetCashBookDebitAccount(int cashBookID)
        {
            Account account = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select debet_account from tbl_Cash_book where ID=@cashBookID";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@cashBookID", cashBookID);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string accountNumber = dr["debet_account"].ToString();
                            account = Account.GetAccount(accountNumber);
                            if (account == null)
                                account = Account.GetSystemAccount(accountNumber);
                        }
                    }
                }

            }
            return account;
        }


        public static Account GetCashBookCreditAccount(int cashBookID)
        {
            Account account = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select credit_account from tbl_Cash_book where ID=@cashBookID";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@cashBookID", cashBookID);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string accountNumber = dr["credit_account"].ToString();
                            account = Account.GetAccount(accountNumber);
                            if (account == null)
                                account = Account.GetSystemAccount(accountNumber);

                        }
                    }
                }

            }
            return account;
        }

        public static byte GetCorrespondentAccountType(int cashBookID)
        {
            byte correspondentAccountType = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select cor_account_type from tbl_Cash_book where ID=@cashBookID";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@cashBookID", cashBookID);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            correspondentAccountType = Convert.ToByte(dr["cor_account_type"]);

                        }
                    }
                }

            }
            return correspondentAccountType;
        }


        internal static CashBookOrder Get(CashBookOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string sqlString = @"
                                        SELECT  registration_date,document_type,document_number,document_subtype,
                                                debet_account, credit_account,
                                                quality, source_type,operationFilialCode,operation_date from Tbl_HB_documents
                                        WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number END AND doc_ID=@docID ";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            order.OrderNumber = dr["document_number"].ToString();
                            order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                            order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                            order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                            order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"]);
                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                        }
                    }
                }

            }
            order.CashBooks = GetCashBookOrderData(order);
            return order;
        }


        internal static bool IsSecondApproveOrReject(int linkedRowID)
        {
            bool secondApproveOrReject = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id FROM tbl_Cash_book WHERE linked_row_id=@linkedRowID", conn))
                {
                    cmd.Parameters.Add("@linkedRowID", SqlDbType.Int).Value = linkedRowID;
                    if (cmd.ExecuteReader().Read())
                    {
                        secondApproveOrReject = true;
                    }
                }


            }
            return secondApproveOrReject;
        }

        internal static List<CashBook> GetCashBookOrderData(CashBookOrder order)
        {

            List<CashBook> cashBooks = new List<CashBook>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string sqlString = @"select * from tbl_cash_book_order_details where doc_ID=@docID";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        CashBook cashBook = new CashBook();
                        cashBook.Amount = Convert.ToDouble(row["amount"]);
                        cashBook.CorrespondentSetNumber = Convert.ToInt32(row["cor_set_number"]);
                        cashBook.Currency = row["currency"].ToString();
                        cashBook.Description = row["addinf"] != DBNull.Value ? row["addinf"].ToString() : null;
                        cashBook.ID = row["cash_book_ID"] != DBNull.Value ? Convert.ToInt32(row["cash_book_ID"]) : 0;
                        cashBook.RegisteredUserID = Convert.ToInt32(row["set_number"]);
                        cashBook.DebetAccount = row["debet_account"] != DBNull.Value ? row["debet_account"].ToString() : null;
                        cashBook.CreditAccount = row["credit_account"] != DBNull.Value ? row["credit_account"].ToString() : null;

                        cashBooks.Add(cashBook);
                    }
                }
            }

            return cashBooks;
        }



        public static bool IsExistUnconfirmedOrder(int cashBookID)
        {

            bool status = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT d.doc_ID FROM Tbl_HB_documents d                                                        
                                                           INNER JOIN tbl_cash_book_order_details o   
                                                           ON o.doc_id = d.doc_id 
                                                           WHERE d.quality = 3 AND o.cash_book_ID=@cashBookID", conn))
                {
                    cmd.Parameters.Add("@cashBookID", SqlDbType.Int).Value = cashBookID;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            status = true;
                        }
                    }

                }

            }
            return status;
        }
    }
}