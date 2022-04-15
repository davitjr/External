using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class CashBookDB
    {
        public static List<CashBook> GetCashBooks(SearchCashBook searchParams)
        {
            List<CashBook> cashBooks = new List<CashBook>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT c.*,t.*,c.id AS close_id FROM tbl_cash_book c LEFT JOIN Tbl_type_of_cash_book_statuses t ON c.status = t.id  WHERE c.date = @date";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (searchParams.FillialCode != null)
                    {
                        sql = sql + " AND c.filialcode = @filialcode";
                        cmd.Parameters.AddWithValue("@filialcode", searchParams.FillialCode);
                    }
                    if (searchParams.RegisteredUserID != null)
                    {
                        sql = sql + " AND (c.set_number = @set_number OR c.cor_set_number = @set_number) ";
                        cmd.Parameters.AddWithValue("@set_number", searchParams.RegisteredUserID);
                    }
                    else if (searchParams.RegisteredUserID == null && searchParams.SearchUserID != null)
                    {
                        sql = sql + " AND (c.set_number = @search_user_id) ";
                        cmd.Parameters.AddWithValue("@search_user_id", searchParams.SearchUserID);
                    }

                    if (!String.IsNullOrWhiteSpace(searchParams.Currency))
                    {
                        sql = sql + " AND c.currency = @currency";
                        cmd.Parameters.AddWithValue("@currency", searchParams.Currency);
                    }
                    if (searchParams.RowType != null && searchParams.RowType != 0)
                    {
                        sql = sql + " AND c.row_type = @row_type";
                        cmd.Parameters.AddWithValue("@row_type", searchParams.RowType);

                    }
                    if (searchParams.Quality != null)
                    {
                        sql = sql + " AND c.status = @status";
                        cmd.Parameters.AddWithValue("@status", searchParams.Quality);
                    }
                    if (searchParams.OperationType != null)
                    {
                        sql = sql + " AND c.debit_credit = @debit_credit";
                        cmd.Parameters.AddWithValue("@debit_credit", searchParams.OperationType == 1 ? "d" : "c");
                    }


                    sql = sql + " ORDER BY c.[date]  + c.[time] desc";
                    cmd.Parameters.AddWithValue("@date", new DateTime(searchParams.RegistrationDate.Year, searchParams.RegistrationDate.Month, searchParams.RegistrationDate.Day, 0, 0, 0));
                    cmd.CommandText = sql;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CashBook cashBook = new CashBook();
                            cashBook.ID = Convert.ToInt32(dr["ID"]);
                            cashBook.FillialCode = Convert.ToInt32(dr["filialcode"]);
                            DateTime regDate = Convert.ToDateTime(dr["date"]);
                            DateTime regTime = Convert.ToDateTime(dr["time"]);
                            cashBook.RegistrationDate = new DateTime(regDate.Year, regDate.Month, regDate.Day, regTime.Hour, regTime.Minute, regTime.Second);
                            cashBook.RegisteredUserID = Convert.ToInt32(dr["set_number"]);
                            if (dr["cor_ID"] != DBNull.Value)
                            {
                                cashBook.CorrespondentID = Convert.ToInt32(dr["cor_ID"]);
                            }
                            cashBook.Amount = Convert.ToDouble(dr["amount"]);
                            cashBook.Currency = dr["currency"].ToString();
                            cashBook.Type = Convert.ToInt32(dr["row_type"]);
                            cashBook.OperationType = dr["debit_credit"].ToString().Equals("d") ? 1 : 2;
                            cashBook.Quality = Convert.ToInt32(dr["status"]);
                            cashBook.QualityDescription = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                            cashBook.Description = dr["addinf"] == DBNull.Value ? null : dr["addinf"].ToString();
                            if (dr["maturedAmount"] != DBNull.Value)
                            {
                                cashBook.MaturedAmount = Convert.ToDouble(dr["maturedAmount"]);
                            }

                            if (dr["cor_set_number"] != DBNull.Value)
                            {
                                cashBook.CorrespondentSetNumber = Convert.ToInt32(dr["cor_set_number"]);
                            }
                            cashBook.LinkedRowID = Convert.ToInt32(dr["linked_row_id"] != DBNull.Value ? dr["linked_row_id"] : 0);
                            if (cashBook.LinkedRowID == 0)
                            {
                                cashBook.LinkedRowID = CashBookDB.GetLinkedRowID(cashBook.ID);
                            }
                            cashBook.IsClosed = dr["close_id"] == DBNull.Value ? false : true;
                            cashBooks.Add(cashBook);
                        }
                    }
                }

            }

            return cashBooks;
        }

        private static int GetLinkedRowID(int ID)
        {
            int linkedRowID = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select ID from tbl_Cash_book where linked_row_id=@ID";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@ID", ID);

                    conn.Open();
                    linkedRowID = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            return linkedRowID;
        }


        public static int GetCashBookFilialCodeByLinkedRow(int linkedRowID)
        {
            int linkedRowFilialCode = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT filialcode FROM tbl_Cash_book WHERE id = ISNULL(@linkedRowID, 0)";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@linkedRowID", linkedRowID);

                    conn.Open();
                    linkedRowFilialCode = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            return linkedRowFilialCode;
        }


        internal static ActionResult RemoveCashBook(int cashBookID)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Delete from tbl_cash_book where ID = @ID AND status=0", conn))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", cashBookID);
                    cmd.ExecuteNonQuery();
                }

            }
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        public static ActionResult Save(List<CashBook> cashBooks)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_cash_book (date, time, amount, addinf, currency, debit_credit, filialcode, set_number,cor_set_number, row_type,linked_row_id) VALUES(@date, @time, @amount, @addinf, @currency, @debit_credit, @filialcode, @set_number, @cor_set_number, @row_type,@linked_row_id)",
                    conn))
                {
                    foreach (CashBook cashBook in cashBooks)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@date", new DateTime(cashBook.RegistrationDate.Year, cashBook.RegistrationDate.Month, cashBook.RegistrationDate.Day, 0, 0, 0));
                        cmd.Parameters.AddWithValue("@time", cashBook.RegistrationDate.TimeOfDay);
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
                        cmd.Parameters.AddWithValue("@set_number", cashBook.RegisteredUserID);
                        cmd.Parameters.AddWithValue("@cor_set_number", cashBook.CorrespondentSetNumber);
                        cmd.Parameters.AddWithValue("@row_type", cashBook.Type);
                        if (cashBook.LinkedRowID != 0)
                        {
                            cmd.Parameters.AddWithValue("@linked_row_id", cashBook.LinkedRowID);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@linked_row_id", DBNull.Value);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            result.ResultCode = ResultCode.Normal;

            return result;
        }

        public static int GetCorrespondentSetNumber(int filialCode, bool isEncashmentDepartment)
        {
            int correspondentSetNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "";
                if (!isEncashmentDepartment)
                {
                    sql = @"SELECT TOP 1 new_id FROM v_cashers_list WHERE group_id in (201,194) and filial_code=@filial_code and active_user=0";
                }
                else
                {
                    sql = @"SELECT TOP 1 new_id FROM v_cashers_list WHERE group_id=198  and filial_code=@filial_code and active_user=0";
                }


                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@filial_code", filialCode);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            correspondentSetNumber = Convert.ToInt32(dr["new_id"]);
                        }
                    }
                }

            }
            return correspondentSetNumber;
        }


        public static int GetCorrespondentFilialCode(int correspondentSetNumber)
        {
            int filialCode = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT filial_code FROM v_cashers_list WHERE new_id=@coresNumb";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@coresNumb", correspondentSetNumber);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            filialCode = Convert.ToInt32(dr["filial_code"]);
                        }
                    }
                }

            }
            return filialCode;
        }


        public static double GetRestTransactions(SearchCashBook searchParams)
        {
            double restTransactions = 0;

            if (!String.IsNullOrWhiteSpace(searchParams.Currency))
            {
                string currency = String.Empty;

                if (!searchParams.Currency.Equals("AMD"))
                {
                    currency = "_currency";
                }
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    string sql = String.Empty;
                    if (searchParams.FillialCode == 22000)
                    {
                        sql = @"DECLARE @sysAccount as float ,@sysAccount1 as float 
                                SELECT @sysAccount=dbo.system_account(1, -1, 0) 
                                SELECT @sysAccount1=dbo.system_account(95,22000,0) 
                                SELECT isnull(sum( case debit_credit when 'd' then amount" + currency + " else 0 end) - sum( case debit_credit when 'c' then amount" + currency + " else 0 end),0) as rest FROM [tbl_accounting_operations;] WHERE collective_account_number='1011A' and current_account_full_number <> @sysAccount and current_account_full_number <> @sysAccount1 and currency = @currency and date_of_accounting = @date";
                    }
                    else if (searchParams.FillialCode == -1)
                    {
                        sql = @"DECLARE @sysAccount as float ,@sysAccount1 as float 
                                SELECT @sysAccount=dbo.system_account(1, -1, 0) 
                                SELECT @sysAccount1=dbo.system_account(95,22000,0) 
                                SELECT isnull(sum( case debit_credit when 'd' then amount" + currency + " else 0 end) - sum( case debit_credit when 'c' then amount" + currency + " else 0 end),0) as rest FROM [tbl_accounting_operations;]  WHERE (current_account_full_number = @sysAccount or current_account_full_number = @sysAccount1) and currency = @currency and date_of_accounting = @date";
                    }
                    else
                    {
                        sql = @"SELECT isnull(sum( case debit_credit when 'd' then amount" + currency + " else 0 end) - sum( case debit_credit when 'c' then amount" + currency + " else 0 end),0) as rest FROM [tbl_accounting_operations;] WHERE collective_account_number='1011A' and currency = @currency and date_of_accounting = @date";
                    }

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;

                        cmd.Parameters.AddWithValue("@currency", searchParams.Currency);
                        cmd.Parameters.AddWithValue("@date", new DateTime(searchParams.RegistrationDate.Year, searchParams.RegistrationDate.Month, searchParams.RegistrationDate.Day, 0, 0, 0));

                        if (searchParams.SearchUserID != null)
                        {
                            sql = sql + " AND number_of_set = @number_of_set";
                            cmd.Parameters.AddWithValue("@number_of_set", searchParams.SearchUserID);
                        }

                        cmd.CommandText = sql;
                        conn.Open();
                        restTransactions = Convert.ToDouble(cmd.ExecuteScalar());
                    }

                }
            }
            return restTransactions;
        }

        public static double GetRestCashBook(SearchCashBook searchParams)
        {
            double restCashBook = 0;

            if (!String.IsNullOrWhiteSpace(searchParams.Currency) && searchParams.FillialCode != null)
            {
                string currency = String.Empty;

                if (!searchParams.Currency.Equals("AMD"))
                {
                    currency = "_currency";
                }
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    string sql = @"SELECT isnull(SUM(case when debit_credit='c' then amount else -amount end),0) as sum_amount FROM Tbl_Cash_book WHERE (set_number=0 or date = @date) and currency = @currency and filialcode = @filialcode and status=2 and row_type=0";

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;

                        cmd.Parameters.AddWithValue("@currency", searchParams.Currency);
                        cmd.Parameters.AddWithValue("@filialcode", searchParams.FillialCode);
                        cmd.Parameters.AddWithValue("@date", new DateTime(searchParams.RegistrationDate.Year, searchParams.RegistrationDate.Month, searchParams.RegistrationDate.Day, 0, 0, 0));

                        if (searchParams.SearchUserID != null)
                        {
                            sql = sql + " AND set_number = @set_number";
                            cmd.Parameters.AddWithValue("@set_number", searchParams.SearchUserID);
                        }

                        cmd.CommandText = sql;
                        conn.Open();
                        restCashBook = Math.Round(Convert.ToDouble(cmd.ExecuteScalar()), 2);
                    }

                }
            }
            return restCashBook + GetRestTransactions(searchParams);
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


        public static int GetCashBookSetnumber(int cashBookID)
        {
            int setNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select set_number from tbl_Cash_book where ID=@cashBookID";

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
                            setNumber = Convert.ToInt32(dr["set_number"]);
                        }
                    }
                }

            }
            return setNumber;
        }


        public static void DeleteCashBook(int Id)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE tbl_cash_book WHERE ID=@id AND status=0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = Id;
                    cmd.ExecuteReader();
                }

            }
        }

        public static double GetCashBookAmount(int cashBookID)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select amount from tbl_Cash_book where ID=@cashBookID";

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
                            amount = Convert.ToDouble(dr["amount"]);
                        }
                    }
                }

            }
            return amount;
        }


        public static bool HasUnconfirmedOrder(int cashbookId)
        {
            bool flag = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" select 1 from tbl_cash_book 
										where linked_row_id =  @cashbookId and status = 0";

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@cashbookId", cashbookId);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            flag = true;
                        }
                    }
                }

            }
            return flag;
        }


    }
}
