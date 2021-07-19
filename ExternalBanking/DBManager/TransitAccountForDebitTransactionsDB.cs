using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class TransitAccountForDebitTransactionsDB
    {


        private static string transitAccountsForDebitTransactionsSelectScript = @"
                                                        SELECT 
                                                        tr.ID,
                                                        a.open_date
                                                        ,21 as type_of_product
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,a.closing_date as closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                        ,tr.description as ac_description
                                                        ,tr.closing_date as tr_closing_date
                                                        ,tr.requires_approval
                                                        ,tr.open_date as tr_open_date
                                                        ,tr.filial_code as tr_filial_code
                                                        ,tr.fee_rate
                                                        ,tr.min_fee_amount
                                                        ,tr.set_number
                                                        ,tr.for_all_branches
                                                    FROM [tbl_all_accounts;] a
                                                INNER JOIN
										        Tbl_transit_accounts_for_debit_transactions tr
                                                ON a.Arm_number = tr.arm_number";

        internal static void SaveTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account, short userSetNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_transit_accounts_for_debit_transactions
                                                         (
                                                          arm_number,
                                                          description,
                                                          open_date,
                                                          closing_date,
                                                          filial_code,
                                                          requires_approval,
                                                          set_number,
                                                          fee_rate,
                                                          min_fee_amount,
                                                          customer_number,
                                                          for_all_branches
                                                         )
                                                         VALUES
                                                         ( 
                                                         @armNumber,
                                                         @description,
                                                         @openDate,
                                                         @closingDate,
                                                         @filialCode,
                                                         @requiresApproval,
                                                         @setNumber,
                                                         @feeRate,
                                                         @minFeeAmount,
                                                         @customerNumber,
                                                         @forAllBranches
                                                         )", conn))
                {
                    cmd.Parameters.Add("@armNumber", SqlDbType.Float).Value = account.TransitAccount.AccountNumber;

                    cmd.Parameters.Add("@openDate", SqlDbType.SmallDateTime).Value = account.OpenDate.Date;
                    if (account.ClosingDate != null)
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = account.ClosingDate;
                    else
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = DBNull.Value;


                    if (account.ForAllBranches != true)
                    {
                        cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = account.FilialCode;
                    }
                    else
                    {
                        cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = 0;

                    }
                    cmd.Parameters.Add("@requiresApproval", SqlDbType.Bit).Value = account.RequiresApproval;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = userSetNumber;
                    if (account.IsCustomerTransitAccount)
                    {
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = account.CustomerNumber;
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 64).Value = DBNull.Value;
                        cmd.Parameters.Add("@feeRate", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@minFeeAmount", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@forAllBranches", SqlDbType.Bit).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@forAllBranches", SqlDbType.Bit).Value = account.ForAllBranches;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 64).Value = account.Description;
                        cmd.Parameters.Add("@feeRate", SqlDbType.Float).Value = account.FeeRate / 100;
                        cmd.Parameters.Add("@minFeeAmount", SqlDbType.Float).Value = account.MinFeeAmount;
                    }


                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static void UpdateTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_transit_accounts_for_debit_transactions
                                                         SET
                                                         fee_rate=@feeRate,
                                                         min_fee_amount=@minFeeAmount
                                                         WHERE ID=@accountID", conn))
                {
                    cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = account.ID;
                    cmd.Parameters.Add("@feeRate", SqlDbType.Float).Value = account.FeeRate / 100;
                    cmd.Parameters.Add("@minFeeAmount", SqlDbType.Float).Value = account.MinFeeAmount;

                    cmd.ExecuteNonQuery();
                }

            }
        }

        internal static void CloseTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_transit_accounts_for_debit_transactions
                                                          SET closing_date=@closingDate
                                                          WHERE ID=@accountID                                                    
                                                          ", conn))
                {
                    cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = account.ID;
                    if (account.ClosingDate != null)
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = account.ClosingDate.Value.Date;
                    else
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void ReopenTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_transit_accounts_for_debit_transactions
                                                          SET closing_date=null
                                                          WHERE ID=@accountID                                                     
                                                          ", conn))
                {
                    cmd.Parameters.Add("@accountID", SqlDbType.Int).Value = account.ID;
                    if (account.ClosingDate != null)
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = account.ClosingDate.Value.Date;
                    else
                        cmd.Parameters.Add("@closingDate", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        internal static List<TransitAccountForDebitTransactions> GetTransitAccountsForDebitTransactions()
        {
            List<TransitAccountForDebitTransactions> accounts = new List<TransitAccountForDebitTransactions>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(transitAccountsForDebitTransactionsSelectScript + @" WHERE  tr.closing_date IS NULL AND tr.customer_number IS NULL ORDER BY tr.ID desc", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            TransitAccountForDebitTransactions account = SetAccount(row);
                            accounts.Add(account);
                        }
                    }
                }

            }

            return accounts;

        }

        internal static TransitAccountForDebitTransactions GetTransitAccountsForDebitTransaction(string accountNumber, ushort filialCode)
        {
            TransitAccountForDebitTransactions account = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(transitAccountsForDebitTransactionsSelectScript + @" WHERE  tr.arm_number=@armNumber and (tr.filial_code=@filiaCode OR tr.for_all_branches=1) AND tr.customer_number IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@armNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@filiaCode", SqlDbType.Int).Value = filialCode;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            account = SetAccount(row);

                        }
                    }
                }

            }

            return account;

        }


        internal static List<TransitAccountForDebitTransactions> GetClosedTransitAccountsForDebitTransactions()
        {
            List<TransitAccountForDebitTransactions> accounts = new List<TransitAccountForDebitTransactions>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(transitAccountsForDebitTransactionsSelectScript + @" WHERE  tr.closing_date is not null and tr.customer_number is null ORDER BY tr.ID desc", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            TransitAccountForDebitTransactions account = SetAccount(row);
                            accounts.Add(account);
                        }
                    }
                }

            }

            return accounts;

        }

        private static TransitAccountForDebitTransactions SetAccount(DataRow row)
        {
            TransitAccountForDebitTransactions account = new TransitAccountForDebitTransactions
            {
                TransitAccount = new Account()
            };

            if (row != null)
            {
                account.ID = Convert.ToInt32(row["ID"]);


                if (row["tr_closing_date"] != DBNull.Value)
                {
                    account.ClosingDate = Convert.ToDateTime(row["tr_closing_date"]);
                }
                else
                {
                    account.ClosingDate = default(DateTime?);
                }
                account.TransitAccount.OpenDate = default(DateTime?);
                account.TransitAccount.ClosingDate = default(DateTime?);
                account.TransitAccount.FreezeDate = default(DateTime?);
                account.TransitAccount.AccountNumber = row["Arm_number"].ToString();
                account.TransitAccount.Balance = double.Parse(row["balance"].ToString());
                account.TransitAccount.Currency = row["Currency"].ToString();
                account.TransitAccount.AccountType = ushort.Parse(row["type_of_product"].ToString());
                account.TransitAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                account.TransitAccount.ProductNumber = row["card_number"].ToString();
                account.TransitAccount.FilialCode = int.Parse(row["filialcode"].ToString());
                account.TransitAccount.Status = short.Parse(row["account_type"].ToString());
                account.TransitAccount.FreezeDate = row["freeze_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["freeze_date"].ToString());
                account.TransitAccount.UnUsedAmount = row["UnUsed_amount"] == DBNull.Value ? default(double?) : double.Parse(row["UnUsed_amount"].ToString());
                account.TransitAccount.UnUsedAmountDate = row["UnUsed_amount_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["UnUsed_amount_date"].ToString());

                if (row.Table.Columns.Contains("account_access_group"))
                    account.TransitAccount.AccountPermissionGroup = row["account_access_group"].ToString();
                else
                    account.TransitAccount.AccountPermissionGroup = "0";

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    account.TransitAccount.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }
                if (!String.IsNullOrEmpty(row["tr_closing_date"].ToString()))
                {
                    account.ClosingDate = (DateTime.Parse(row["tr_closing_date"].ToString())).Date;
                }

                if (row.Table.Columns.Contains("Co_type"))
                {
                    account.TransitAccount.JointType = ushort.Parse(row["Co_Type"].ToString());
                }
                else
                    account.TransitAccount.JointType = 0;
                account.TransitAccount.OpenDate = (DateTime.Parse(row["open_date"].ToString())).Date;
                account.OpenDate = (DateTime.Parse(row["tr_open_date"].ToString())).Date;
                if (row.Table.Columns.Contains("ac_description"))
                {
                    account.TransitAccount.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("ac_description"));
                }
                account.RequiresApproval = Convert.ToBoolean(row["requires_approval"]);

                if (row["for_all_branches"] != DBNull.Value)
                {
                    account.ForAllBranches = Convert.ToBoolean(row["for_all_branches"]);
                }
                if (!account.ForAllBranches)
                    account.FilialCode = ushort.Parse(row["tr_filial_code"].ToString());

                if (row["fee_rate"] != DBNull.Value)
                {
                    account.FeeRate = Convert.ToDouble(row["fee_rate"].ToString()) * 100;
                }
                if (row["min_fee_amount"] != DBNull.Value)
                {
                    account.MinFeeAmount = Convert.ToDouble(row["min_fee_amount"].ToString());
                }
                if (row["set_number"] != DBNull.Value)
                {
                    account.SetNumber = Convert.ToInt32(row["set_number"].ToString());
                }




            }
            return account;
        }
    }
}
