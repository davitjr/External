using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class LeasingDB
    {
        internal static List<SearchLeasingCustomer> GetLeasingCustomers(SearchLeasingCustomer searchParams)
        {

            List<SearchLeasingCustomer> leasingCustomers = new List<SearchLeasingCustomer>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SynDBConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_Find_Leasing_Customer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@find_type", SqlDbType.SmallInt).Value = 0;
                    if (searchParams.CustomerNumber != null)
                    {
                        cmd.Parameters.Add("@customer_number", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.CustomerNumber);
                    }
                    if (searchParams.LeasingCustomerNumber != 0)
                    {
                        cmd.Parameters.Add("@number", SqlDbType.SmallInt).Value = searchParams.LeasingCustomerNumber;
                    }

                    if (searchParams.Name != null)
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.Name);
                    }
                    if (searchParams.LastName != null)
                    {
                        cmd.Parameters.Add("@lastname", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.LastName);
                    }
                    if (searchParams.OrganizationName != null)
                    {
                        cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.OrganizationName);
                    }
                    if (searchParams.TaxCode != null)
                    {
                        cmd.Parameters.Add("@codeoftax", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.TaxCode);
                    }
                    if (searchParams.PassportNumber != null)
                    {
                        cmd.Parameters.Add("@passport", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.PassportNumber);
                    }
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        SearchLeasingCustomer oneResult = new SearchLeasingCustomer();
                        oneResult.CustomerNumber = Utility.ConvertAnsiToUnicode(dr["customer_number"].ToString());
                        oneResult.LeasingCustomerNumber = Convert.ToInt16(dr["number"].ToString());
                        oneResult.Name = Utility.ConvertAnsiToUnicode(dr["name"].ToString());
                        oneResult.LastName = Utility.ConvertAnsiToUnicode(dr["lastname"].ToString());
                        oneResult.OrganizationName = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                        oneResult.TaxCode = Utility.ConvertAnsiToUnicode(dr["code_of_tax"].ToString());
                        oneResult.PassportNumber = Utility.ConvertAnsiToUnicode(dr["passport_number"].ToString());
                        leasingCustomers.Add(oneResult);
                    }
                }

            }

            return leasingCustomers;
        }

        internal static LeasingDetailedInformation GetLeasingDetailedInformation(long loanFullName, DateTime dateOfBeginning)
        {
            LeasingDetailedInformation detailedInformation = new LeasingDetailedInformation();
            List<LeasingAccountsDetails> accountsDetails = new List<LeasingAccountsDetails>();
            List<LeasingDebtsDetails> debtsDetails = new List<LeasingDebtsDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    da.SelectCommand = new SqlCommand();
                    da.SelectCommand.Connection = conn;
                    da.SelectCommand.CommandText = "sp_get_leasing_detailed_information";
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.Add("@loan_full_number", SqlDbType.Float).Value = loanFullName;
                    da.SelectCommand.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = dateOfBeginning;

                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        System.Data.DataTable dt = ds.Tables[0];
                        if (dt != null)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                detailedInformation.CurrentPeriodCapital = Convert.ToDouble(row["period_capital"]);
                                detailedInformation.CurrentPeriodRent = Convert.ToDouble(row["period_rent"]);
                                detailedInformation.IsSubsid = Convert.ToInt16(row["is_subsidia"]);
                            }
                        }
                    }
                    if (ds.Tables[1].Rows.Count != 0)
                    {
                        System.Data.DataTable dt = ds.Tables[1];
                        if (dt != null)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                LeasingDebtsDetails debtsDetailsObj = new LeasingDebtsDetails();
                                debtsDetailsObj.Description = Convert.ToString(row["description"]);
                                debtsDetailsObj.GeneratedDate = Convert.ToDateTime(row["generated_date"]);
                                debtsDetailsObj.PayDate = Convert.ToDateTime(row["pay_date"]);
                                debtsDetailsObj.Amount = Convert.ToDouble(row["amount"]);
                                debtsDetails.Add(debtsDetailsObj);
                            }
                        }
                    }
                    if (ds.Tables[2].Rows.Count != 0)
                    {
                        System.Data.DataTable dt = ds.Tables[2];
                        if (dt != null)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                LeasingAccountsDetails accountsDetailsObj = new LeasingAccountsDetails();
                                accountsDetailsObj.Accounts = Convert.ToInt64(row["accounts"]);
                                accountsDetailsObj.Currency = Convert.ToString(row["currency"]);
                                accountsDetailsObj.Balance = Convert.ToDouble(row["balance"]);
                                accountsDetailsObj.TypeOfAccount = Convert.ToString(row["type_of_account"]);
                                accountsDetailsObj.Description = Convert.ToString(row["description"]);
                                accountsDetails.Add(accountsDetailsObj);
                            }
                        }
                    }

                }

            }
            detailedInformation.DebtsDetails = debtsDetails;
            detailedInformation.AccountsDetails = accountsDetails;
            return detailedInformation;
        }
        internal static List<LeasingInsuranceDetails> GetLeasingInsuranceInformation(long loanFullNumber, DateTime dateOfBeginning)
        {
            DataTable dt = new DataTable();
            List<LeasingInsuranceDetails> insuranceInfo = new List<LeasingInsuranceDetails>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"Select f.id,fo.oper_description,f.sum_amd,f.pay_date 
                                                                                            From Tbl_for_1704 f
                                                                                            Inner Join Tbl_type_of_operations_1704 fo on f.operation_type = fo.oper_type
                                                                                            where f.date_of_repay is null and f.operation_type in (5, 6, 7, 12, 16, 17, 19, 20, 21, 23, 24, 28) and f.loan_full_number = @loan_full_number and f.date_of_beginning = @date_of_beginning group by f.id,fo.oper_description,f.sum_amd,f.pay_date", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@loan_full_number", SqlDbType.BigInt).Value = loanFullNumber;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.DateTime).Value = dateOfBeginning;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LeasingInsuranceDetails insuranceInfoObj = new LeasingInsuranceDetails();

                            insuranceInfoObj.Id = Convert.ToInt32(reader["id"]);
                            insuranceInfoObj.OperDescription = Convert.ToString(reader["oper_description"]);
                            insuranceInfoObj.SumAmd = Convert.ToDouble(reader["sum_amd"]);
                            insuranceInfoObj.PayDate = Convert.ToDateTime(reader["pay_date"]);
                            insuranceInfo.Add(insuranceInfoObj);
                        }
                    }

                }
            }
            return insuranceInfo;
        }

        internal static List<LeasingLoan> GetLeasingLoans(SearchLeasingLoan searchParams)
        {

            List<LeasingLoan> leasingLoans = new List<LeasingLoan>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SynDBConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_Find_Leasing_Customer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@find_type", SqlDbType.SmallInt).Value = 1;
                    if (searchParams.CustomerNumber != null)
                    {
                        cmd.Parameters.Add("@customer_number", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.CustomerNumber);
                    }
                    if (searchParams.LeasingCustomerNumber != 0)
                    {
                        cmd.Parameters.Add("@number", SqlDbType.SmallInt).Value = searchParams.LeasingCustomerNumber;
                    }

                    if (searchParams.Name != null)
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.Name);
                    }
                    if (searchParams.LastName != null)
                    {
                        cmd.Parameters.Add("@lastname", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.LastName);
                    }
                    if (searchParams.OrganizationName != null)
                    {
                        cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.OrganizationName);
                    }
                    if (searchParams.TaxCode != null)
                    {
                        cmd.Parameters.Add("@codeoftax", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.TaxCode);
                    }
                    if (searchParams.PassportNumber != null)
                    {
                        cmd.Parameters.Add("@passport", SqlDbType.VarChar).Value = Utility.ConvertUnicodeToAnsi(searchParams.PassportNumber);
                    }
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingLoan oneResult = new LeasingLoan();
                        oneResult.LoanAccount = new Account();
                        oneResult.GeneralNumber = Utility.ConvertAnsiToUnicode(dr["general_number"].ToString());
                        oneResult.Currency = Utility.ConvertAnsiToUnicode(dr["currency"].ToString());

                        oneResult.StartCapital = dr["start_capital"] != DBNull.Value ? Convert.ToDouble(dr["start_capital"].ToString()) : 0;

                        oneResult.StartDate = Convert.ToDateTime(dr["date_of_beginning"].ToString());
                        oneResult.EndDate = Convert.ToDateTime(dr["date_of_normal_end"].ToString());
                        oneResult.OverdueLoanDate = dr["overdue_loan_date"] != DBNull.Value ? Convert.ToDateTime(dr["overdue_loan_date"].ToString()) : (DateTime?)null;
                        oneResult.NextRepaymentDate = dr["date_of_repayment"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_repayment"].ToString()) : (DateTime?)null;

                        oneResult.QualityDescription = Utility.ConvertAnsiToUnicode(dr["Leasing_status"].ToString());
                        oneResult.LoanAccount.AccountNumber = Utility.ConvertAnsiToUnicode(dr["loan_full_number"].ToString());

                        oneResult.LeasingPayment = dr["leasing_payment"] != DBNull.Value ? Convert.ToDouble(dr["leasing_payment"].ToString()) : 0;
                        oneResult.AdvanceAndFee = dr["advanceandfee"] != DBNull.Value ? Convert.ToDouble(dr["advanceandfee"].ToString()) : 0;
                        oneResult.FeeAmount = dr["fee_amount"] != DBNull.Value ? Convert.ToDouble(dr["fee_amount"].ToString()) : 0;
                        oneResult.AdvanceAmount = dr["advance_amount"] != DBNull.Value ? Convert.ToDouble(dr["advance_amount"].ToString()) : 0;
                        oneResult.InsurancePayments = dr["insurance_payment"] != DBNull.Value ? Convert.ToDouble(dr["insurance_payment"].ToString()) : 0;
                        oneResult.OtherPayments = dr["other_payments"] != DBNull.Value ? Convert.ToDouble(dr["other_payments"].ToString()) : 0;

                        oneResult.PenaltyRate = dr["penalty"] != DBNull.Value ? Convert.ToDouble(dr["penalty"].ToString()) : 0;
                        oneResult.PrepaymentAmount = dr["prepayment_amount"] != DBNull.Value ? Convert.ToDouble(dr["prepayment_amount"].ToString()) : 0;

                        leasingLoans.Add(oneResult);
                    }
                }

            }
            return leasingLoans;
        }


        internal static void SaveLeasingPaymentDetails(Order order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO 
                                            Tbl_Leasing_Payments_Order_Details(doc_ID,loan_full_number,date_of_beginning,number,currency,description,add_description,pay_type,leasing_insurance_Id) 
                        VALUES(@doc_ID,@loan_full_number,@date_of_beginning,@number,@currency,@description,@add_description,@pay_type,@leasingInsuranceId)";

                    cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@number", SqlDbType.SmallInt).Value = order.AdditionalParametrs[0] != null ? Convert.ToInt16(order.AdditionalParametrs[0].AdditionValue) : 0; 
                    cmd.Parameters.Add("@loan_full_number", SqlDbType.Float).Value = order.AdditionalParametrs[1] != null ? Convert.ToDouble(order.AdditionalParametrs[1].AdditionValue) : 0; 
                    
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = order.AdditionalParametrs[5].AdditionValue != null ? order.AdditionalParametrs[5].AdditionValue : "";
                    cmd.Parameters.Add("@add_description", SqlDbType.NVarChar).Value = order.AdditionalParametrs[6].AdditionValue != null ? order.AdditionalParametrs[6].AdditionValue : "";

                    if (order.AdditionalParametrs[2] != null && order.AdditionalParametrs[2].AdditionValue != null)
                        cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = Convert.ToDateTime(order.AdditionalParametrs[2].AdditionValue);
                    else
                        cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = DBNull.Value;

                    if (order.AdditionalParametrs[4] != null && order.AdditionalParametrs[4].AdditionValue != null)
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.AdditionalParametrs[4].AdditionValue;
                    else
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = DBNull.Value;
                    if ((order.PayType ?? 0) == 0)
                        cmd.Parameters.Add("@pay_type", SqlDbType.Int).Value = DBNull.Value;
                    else
                        cmd.Parameters.Add("@pay_type", SqlDbType.Int).Value = order.PayType;


                    if (order.AdditionalParametrs[9] != null && Convert.ToInt32(order.AdditionalParametrs[9].AdditionValue) != 0)
                    {
                        cmd.Parameters.Add("@leasingInsuranceId", SqlDbType.Int).Value = Convert.ToInt32(order.AdditionalParametrs[9].AdditionValue);
                    }
                    else
                    {
                        cmd.Parameters.Add("@leasingInsuranceId", SqlDbType.Int).Value = DBNull.Value;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static double GetPartlyMatureAmount(string contractNumber)
        {
            double payableAmount = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@" Select payable_amount from [dbo].[v_get_partlyMatureAmount] where general_number = @contractNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@contractNumber", SqlDbType.NVarChar).Value = contractNumber;
                    payableAmount = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            return payableAmount;
        }

    }
}
