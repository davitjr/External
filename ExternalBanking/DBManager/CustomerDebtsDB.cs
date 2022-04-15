using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class CustomerDebtsDB
    {
        public static List<CustomerDebts> GetCustomerDebts(ulong customerNumber, string accountNumber = "")
        {
            List<CustomerDebts> debts = new List<CustomerDebts>();


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Sp_CustomerDebtCheckings", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar).Value = "";
                    cmd.Parameters.Add("@only_checking", SqlDbType.SmallInt).Value = 2;
                    if (accountNumber != "" && accountNumber != "0")
                    {
                        cmd.Parameters.Add("@accNumber", SqlDbType.Float).Value = double.Parse(accountNumber.Trim());
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CustomerDebts cust = new CustomerDebts();
                            cust.ObjectNumber = dr["customer_number"].ToString();
                            cust.Amount = dr["amount"].ToString();
                            cust.Currency = dr["currency"].ToString();
                            cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                            cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                            cust.DebtType = DebtTypes.CurrentAccount;
                            debts.Add(cust);
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["customer_number"].ToString();
                                cust.Amount = dr["amount"].ToString();
                                cust.Currency = dr["currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.HomeBanking;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["visa_number"].ToString();
                                cust.Amount = dr["debt"].ToString();
                                cust.Currency = dr["currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.Card;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["visa_number"].ToString();
                                cust.Amount = dr["current_capital"].ToString();
                                cust.Currency = dr["currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.Overdraft;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["idpro"].ToString();
                                cust.Amount = dr["Amount"].ToString();
                                cust.Currency = dr["Currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.Provision;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["loan_full_number"].ToString();
                                cust.Amount = dr["debt"].ToString();
                                cust.Currency = dr["Currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.Loan;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["loan_full_number"].ToString();
                                cust.Amount = dr["debt"].ToString();
                                cust.Currency = dr["Currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.CreditLine;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["unic_number"].ToString();
                                cust.Amount = dr["debt"].ToString();
                                cust.Currency = dr["Currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["descr"].ToString());
                                cust.DebtType = DebtTypes.GivenGuarantee;
                                debts.Add(cust);
                            }
                        }
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["customer_number"].ToString();
                                double k = 0;
                                if (double.TryParse(dr["amount"].ToString(), out k))
                                {
                                    cust.Amount = k.ToString();
                                }
                                else
                                {
                                    cust.AmountDescription = Utility.ConvertAnsiToUnicode(dr["amount"].ToString());
                                    cust.Amount = null;
                                }



                                cust.Currency = dr["Currency"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["blockage_type"].ToString());
                                cust.DebtType = DebtTypes.Dahk;
                                debts.Add(cust);
                            }
                        }

                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                CustomerDebts cust = new CustomerDebts();
                                cust.ObjectNumber = dr["customer_number"].ToString();
                                cust.AlowTransaction = (short)Convert.ToInt16((dr["allow_transaction"]));
                                cust.DebtDescription = Utility.ConvertAnsiToUnicode(dr["debt_description"].ToString());
                                cust.DebtType = DebtTypes.PEK;
                                debts.Add(cust);
                            }
                        }
                    }


                }

            }
            return debts;
        }


        internal static double GetCustomerServiceFeeDebt(ulong customerNumber, DebtTypes debtType)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT dbo.From_CustomerNumberDebt(@customer_number,@debt_type)";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@debt_type", SqlDbType.TinyInt).Value = debtType;
                    double amount = Convert.ToDouble(cmd.ExecuteScalar());
                    return amount;
                }


            }
        }

        internal static List<DebtTypes> GetCustomerServiceFeeDebts(ulong customerNumber)
        {
            List<DebtTypes> debtTypes = new List<DebtTypes>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = "select debt_type from Tbl_customers_debts where customer_number = @customer_number and allow_transaction = 0 and amount > 0";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            debtTypes.Add((DebtTypes)Convert.ToInt16(dr["debt_type"]));
                        }
                    }

                }
            }
            return debtTypes;
        }

    }
}
