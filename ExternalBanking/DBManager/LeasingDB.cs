using ExternalBanking.Leasing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
                    using SqlDataReader dr = cmd.ExecuteReader();
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

        internal static void GetPaymentOrder(LeasingPaymentOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = @"Select rs.description as ReasonIdDescription,AD.doc_id, AD.pay_type, AD.leasing_insurance_Id, AD.insurance_description, AD.app_id, *  
                                                            from Tbl_Hb_Documents D  
															LEFT JOIN Tbl_New_Transfer_Doc TD on D.doc_ID = TD.Doc_id   
															LEFT JOIN dbo.tbl_type_of_card_debit_reasons rs ON rs.TYPE = d.reason_type  
                                                            OUTER APPLY (select top 1  CONVERT(VARCHAR(5),change_date, 108)  change_time  from Tbl_HB_quality_history where doc_ID=D.doc_ID  order by quality )time
                                                            INNER JOIN (SELECT OD.doc_id, OD.pay_type, OD.leasing_insurance_Id, LD.insurance_description, LD.app_id
																				FROM [dbo].[Tbl_Leasing_Payments_Order_Details]  OD
																				INNER JOIN V_Leasing_Details LD ON LD.loan_full_number  = OD.loan_full_number AND  LD.date_of_beginning  = OD.date_of_beginning) AD ON AD.doc_id = D.Doc_id
														   where D.doc_id=@id and D.customer_number=case when @customerNumber = 0 then customer_number else @customerNumber end";

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
            
            using SqlDataReader dr = cmd.ExecuteReader();
          
            if (dr.Read())
            {
                order.Id = long.Parse(dr["doc_id"].ToString());
                order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                order.Type = (OrderType)Convert.ToInt16(dr["document_type"]);
                order.SubType = Convert.ToByte(dr["document_subtype"]);
                order.Receiver = dr["receiver_name"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString()) : default;
                order.ReceiverBankCode = dr["credit_bank_code"] != DBNull.Value ? Convert.ToInt32(dr["credit_bank_code"]) : default;
                order.Amount = dr["amount"] != DBNull.Value ? Convert.ToDouble(dr["amount"]) : default;
                order.Currency = dr["currency"] != DBNull.Value ? dr["currency"].ToString() : default;
                order.SubType = Convert.ToByte(dr["document_subtype"]);
                order.OrderNumber = dr["document_number"].ToString();
                order.Description = dr["description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["description"].ToString()) : default;
                order.ConvertationRate = dr["rate_sell_buy"] != DBNull.Value ? Convert.ToDouble(dr["rate_sell_buy"]) : default;
                order.ConvertationRate1 = dr["rate_sell_buy_cross"] != DBNull.Value ? Convert.ToDouble(dr["rate_sell_buy_cross"]) : default;
                order.TransferFee = dr["amount_for_payment"] != DBNull.Value ? Convert.ToDouble(dr["amount_for_payment"]) : default;
                order.FeeAccount = dr["deb_for_transfer_payment"] != DBNull.Value ? Account.GetAccount(Convert.ToUInt64(dr["deb_for_transfer_payment"])) : default;
                order.RegistrationTime = dr["change_time"] != DBNull.Value ? dr["change_time"].ToString() : default;
                order.Source = dr["source_type"] != DBNull.Value ? (SourceType)Convert.ToInt16(dr["source_type"]) : default;
                order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                order.UseCreditLine = dr["use_credit_line"] != DBNull.Value && Convert.ToBoolean(dr["use_credit_line"]);
                order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : default;
                order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                order.FilialCode = Convert.ToUInt16(dr["filial"].ToString());
                order.PayType = dr["pay_type"] != DBNull.Value ? Convert.ToInt32(dr["pay_type"].ToString()) : default;
                order.ProductId = dr["app_id"] != DBNull.Value ? ulong.Parse(dr["app_id"].ToString()) : default;
                order.InsuranceId = dr["leasing_insurance_Id"] != DBNull.Value ? Convert.ToInt32(dr["leasing_insurance_Id"].ToString()) : default;
                order.InsuranceDescription = dr["insurance_description"] != DBNull.Value ? dr["insurance_description"].ToString() : default;

                if (dr["reason_type"] != DBNull.Value && Convert.ToInt32(dr["reason_type"]) != 0)
                {
                    order.ReasonId = Convert.ToInt32(dr["reason_type"]);
                    order.ReasonIdDescription = Utility.ConvertAnsiToUnicode(dr["ReasonIdDescription"].ToString());
                    if (order.ReasonId == 99)
                    {
                        order.ReasonIdDescription = Utility.ConvertAnsiToUnicode(dr["reason_type_description"].ToString());
                    }
                }

                if (dr["debet_account"] != DBNull.Value)
                {
                    order.DebitAccount = new Account
                    {
                        AccountNumber = dr["debet_account"].ToString()
                    };
                }

                if (dr["credit_account"] != DBNull.Value)
                {
                    order.ReceiverAccount = new Account
                    {
                        AccountNumber = dr["credit_bank_code"].ToString() + dr["credit_account"].ToString()
                    };
                }
            }
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
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand(@"Select f.id,fo.oper_description,f.sum_amd,f.pay_date, f.date_of_registr 
                                                                                            From Tbl_for_1704 f
                                                                                            Inner Join Tbl_type_of_operations_1704 fo on f.operation_type = fo.oper_type
                                                                                            where f.date_of_repay is null and f.operation_type in (5, 6, 7, 12, 16, 17, 19, 20, 21, 23, 24, 28) and f.loan_full_number = @loan_full_number and f.date_of_beginning = @date_of_beginning group by f.id,fo.oper_description,f.sum_amd,f.pay_date, f.date_of_registr ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loan_full_number", SqlDbType.BigInt).Value = loanFullNumber;
                cmd.Parameters.Add("@date_of_beginning", SqlDbType.DateTime).Value = dateOfBeginning;
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LeasingInsuranceDetails insuranceInfoObj = new LeasingInsuranceDetails();

                        insuranceInfoObj.Id = Convert.ToInt32(reader["id"]);
                        insuranceInfoObj.OperDescription = Convert.ToString(reader["oper_description"]);
                        insuranceInfoObj.SumAmd = Convert.ToDouble(reader["sum_amd"]);
                        insuranceInfoObj.PayDate = Convert.ToDateTime(reader["pay_date"]);
                        if(reader["date_of_registr"] != DBNull.Value)
                            insuranceInfoObj.RegistrationDate = Convert.ToDateTime(reader["date_of_registr"]);

                        insuranceInfo.Add(insuranceInfoObj);
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
                    using SqlDataReader dr = cmd.ExecuteReader();
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


                    if (order.AdditionalParametrs.ElementAtOrDefault(9) != null && Convert.ToInt32(order.AdditionalParametrs[9].AdditionValue) != 0)
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


        internal static List<CustomerLeasingLoans> GetLeasings(ulong customerNumber)
        {
            List<CustomerLeasingLoans> customerLeasingLoans = new List<CustomerLeasingLoans>();
            string leasingRecieverAccountNumber = Account.GetSystemAccountByNN(118007, 22000).AccountNumber;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_Customer_Leasing_Loans_Details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        CustomerLeasingLoans oneResult = new CustomerLeasingLoans
                        {
                            ProductId = dr["app_id"] != DBNull.Value ? ulong.Parse(dr["app_id"].ToString()) : 0,
                            ProductType = Utility.ConvertAnsiToUnicode(dr["loan_description"].ToString()),
                            StartCapital = dr["start_capital"] != DBNull.Value ? Convert.ToDouble(dr["start_capital"].ToString()) : 0,
                            CurrentCapital = dr["current_capital"] != DBNull.Value ? Convert.ToDouble(dr["current_capital"].ToString()) : 0,
                            CurrentCapitalAMD = dr["current_capital_AMD"] != DBNull.Value ? Convert.ToDouble(dr["current_capital"].ToString()) : 0,
                            Currency = dr["currency"].ToString(),
                            StartDate = Convert.ToDateTime(dr["date_of_beginning"].ToString()),
                            EndDate = dr["date_of_normal_end"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_normal_end"].ToString()) : (DateTime?)null,
                            LeasingPayment = dr["payable_amount"] != DBNull.Value ? Convert.ToDouble(dr["payable_amount"].ToString()) : 0,
                            CreditCode = dr["Credit_Code"].ToString(),
                            NextRepaymentDate = dr["next_repayment_date"] != DBNull.Value ? Convert.ToDateTime(dr["next_repayment_date"].ToString()) : (DateTime?)null,
                            LoanFullNumber = dr["loan_full_number"] != DBNull.Value ? ulong.Parse(dr["loan_full_number"].ToString()) : 0,
                            Quality = dr["quality"] != DBNull.Value ? Convert.ToByte(dr["quality"].ToString()) : (byte)0,
                            QualityDescription = Utility.ConvertAnsiToUnicode(dr["quality_description"].ToString()),
                            GeneralNumber = dr["general_number"].ToString(),
                            RecieverAccountNumber = leasingRecieverAccountNumber
                        };
                        customerLeasingLoans.Add(oneResult);
                    }
                }
            }

            return customerLeasingLoans;
        }

        internal static LeasingLoanDetails GetLeasing(ulong productId)
        {
            LeasingLoanDetails leasingLoanDetails = new LeasingLoanDetails();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_Customer_Leasing_Loans_Details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = 0;
                    cmd.Parameters.Add("@appID", SqlDbType.BigInt).Value = productId;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        leasingLoanDetails.ProductId = dr["app_id"] != DBNull.Value ? ulong.Parse(dr["app_id"].ToString()) : 0;
                        leasingLoanDetails.ProductType = dr["loan_description"].ToString();
                        leasingLoanDetails.StartCapital = dr["start_capital"] != DBNull.Value ? Convert.ToDouble(dr["start_capital"].ToString()) : 0;
                        leasingLoanDetails.CurrentCapital = dr["current_capital"] != DBNull.Value ? Convert.ToDouble(dr["current_capital"].ToString()) : 0;
                        leasingLoanDetails.CurrentCapitalAMD = dr["current_capital_AMD"] != DBNull.Value ? Convert.ToDouble(dr["current_capital_AMD"].ToString()) : 0;
                        leasingLoanDetails.LoanFullNumber = dr["loan_full_number"] != DBNull.Value ? ulong.Parse(dr["loan_full_number"].ToString()) : 0;
                        leasingLoanDetails.AccountBalance = dr["account_balance"] != DBNull.Value ? Convert.ToDouble(dr["account_balance"].ToString()) : 0;
                        leasingLoanDetails.AccountBalanceAMD = dr["account_balance_AMD"] != DBNull.Value ? Convert.ToDouble(dr["account_balance_AMD"].ToString()) : 0;
                        leasingLoanDetails.StartDate = Convert.ToDateTime(dr["date_of_beginning"].ToString());
                        leasingLoanDetails.EndDate = dr["date_of_normal_end"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_normal_end"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.Currency = dr["currency"].ToString();
                        leasingLoanDetails.InterestRate = float.Parse(dr["interest_rate"].ToString());
                        leasingLoanDetails.ActualInterestRate = float.Parse(dr["actual_interest_rate"].ToString());
                        leasingLoanDetails.LoanAccount = dr["account_number"] != DBNull.Value ? long.Parse(dr["account_number"].ToString()) : 0;
                        leasingLoanDetails.CreditCode = dr["Credit_Code"].ToString();
                        leasingLoanDetails.LeasingPayment = dr["payable_amount"] != DBNull.Value ? Convert.ToDouble(dr["payable_amount"].ToString()) : 0;
                        leasingLoanDetails.NextRepaymentDate = dr["next_repayment_date"] != DBNull.Value ? Convert.ToDateTime(dr["next_repayment_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.Quality = dr["quality"] != DBNull.Value ? Convert.ToByte(dr["quality"].ToString()) : (byte)0;
                        leasingLoanDetails.QualityDescription = dr["quality_description"].ToString();
                        leasingLoanDetails.Kurs = dr["kurs"] != DBNull.Value ? Convert.ToDouble(dr["kurs"].ToString()) : 0;
                        leasingLoanDetails.GeneralNumber = dr["general_number"].ToString();
                        leasingLoanDetails.OverdueCapital = dr["overdue_capital"] != DBNull.Value ? Convert.ToDouble(dr["overdue_capital"].ToString()) : 0;
                        leasingLoanDetails.OverduePercent = dr["overdue_percent"] != DBNull.Value ? Convert.ToDouble(dr["overdue_percent"].ToString()) : 0;
                        leasingLoanDetails.OverdueLoanDate = dr["overdue_loan_date"] != DBNull.Value ? Convert.ToDateTime(dr["overdue_loan_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.OverdueDays = dr["overdue_days"] != DBNull.Value ? short.Parse(dr["overdue_days"].ToString()) : (short)0;
                        leasingLoanDetails.SubsidRate = float.Parse(dr["subsid_rate"].ToString());
                        leasingLoanDetails.Name = dr["name"].ToString();
                        leasingLoanDetails.LastName = dr["lastname"].ToString();
                        leasingLoanDetails.OrganizationName = dr["organization_name"].ToString();
                        leasingLoanDetails.LeasingCustomerNumber = Convert.ToInt32(dr["leasing_customer_number"].ToString());
                        leasingLoanDetails.CustomerNumber = dr["Customer_number"] != DBNull.Value ? long.Parse(dr["Customer_number"].ToString()) : 0;
                        leasingLoanDetails.FilialCode = dr["fillial_code"] != DBNull.Value ? ushort.Parse(dr["fillial_code"].ToString()) : (ushort)0;
                        leasingLoanDetails.FilialName = dr["filial_name"].ToString();
                        leasingLoanDetails.DateOfRateCalculation = dr["date_of_rate_calculation"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_rate_calculation"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.DateOfLastPayment = dr["date_of_last_payment"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_last_payment"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.AdvanceAmount = dr["advance_amount"] != DBNull.Value ? Convert.ToDouble(dr["advance_amount"].ToString()) : 0;
                        leasingLoanDetails.AccumulatedPercent = dr["accumulated_percent"] != DBNull.Value ? Convert.ToDouble(dr["accumulated_percent"].ToString()) : 0;
                        leasingLoanDetails.AccumulatedPercentPaied = dr["accumulated_percent_paied"] != DBNull.Value ? Convert.ToDouble(dr["accumulated_percent_paied"].ToString()) : 0;
                        leasingLoanDetails.PenaltyRate = dr["penalty_rate"] != DBNull.Value ? Convert.ToDouble(dr["penalty_rate"].ToString()) : 0;
                        leasingLoanDetails.PenaltyRatePaied = dr["penalty_rate_paied"] != DBNull.Value ? Convert.ToDouble(dr["penalty_rate_paied"].ToString()) : 0;
                        leasingLoanDetails.InsuranceAmount = dr["insurance_amount"] != DBNull.Value ? Convert.ToDouble(dr["insurance_amount"].ToString()) : 0;
                        leasingLoanDetails.InsuranceCalculationDate = dr["insurance_calculation_date"] != DBNull.Value ? Convert.ToDateTime(dr["insurance_calculation_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.InsurancePaymentDate = dr["insurance_payment_date"] != DBNull.Value ? Convert.ToDateTime(dr["insurance_payment_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.FeeAmount = dr["fee_amount"] != DBNull.Value ? Convert.ToDouble(dr["fee_amount"].ToString()) : 0;
                        leasingLoanDetails.FeePaymentDate = dr["fee_payment_date"] != DBNull.Value ? Convert.ToDateTime(dr["fee_payment_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.OtherPaymentAmount = dr["other_payment_amount"] != DBNull.Value ? Convert.ToDouble(dr["other_payment_amount"].ToString()) : 0;
                        leasingLoanDetails.OtherPaymentDate = dr["other_payment_date"] != DBNull.Value ? Convert.ToDateTime(dr["other_payment_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.PrepaymentAmount = dr["prepayment_amount"] != DBNull.Value ? Convert.ToDouble(dr["prepayment_amount"].ToString()) : 0;
                        leasingLoanDetails.PrepaymentPaymentDate = dr["prepayment_amount_date"] != DBNull.Value ? Convert.ToDateTime(dr["prepayment_amount_date"].ToString()) : (DateTime?)null;
                        leasingLoanDetails.FinalFee = dr["final_fee"] != DBNull.Value ? Convert.ToDouble(dr["final_fee"].ToString()) : 0;
                        leasingLoanDetails.PaymentsCount = dr["payments_count"] != DBNull.Value ? short.Parse(dr["payments_count"].ToString()) : (short)0;
                        leasingLoanDetails.OrdinalRepayment = dr["ordinal_repayment"] != DBNull.Value ? (Convert.ToDouble(dr["ordinal_repayment"].ToString()) > 0 ? Convert.ToDouble(dr["ordinal_repayment"].ToString()) : 0) : 0;
                    }
                }
            }

            GetLeasingFees(leasingLoanDetails);
            GetLeasingOtherPayments(leasingLoanDetails);

            return leasingLoanDetails;
        }

        internal static void GetLeasingFees(LeasingLoanDetails leasingLoanDetails)
        {
            List<OtherPayments> fees = new List<OtherPayments>();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                if (leasingLoanDetails.Quality == 10 || leasingLoanDetails.Quality == 6 || leasingLoanDetails.Quality == 14)
                {
                    sql = @"SELECT  1 AS id, ROUND(SL.fee_prcent * M.start_capital * dbo.fnc_kurs_for_date_bank(SL.currency,dateadd(day,datediff(day,1,M.date_of_beginning_contract),0)),1) AS sum_amd,
                            dbo.fnc_get_n_th_working_day(date_of_beginning_contract, 5) AS pay_date
		                                                FROM [Tbl_short_time_loans;] SL 
														INNER JOIN Tbl_main_loans_contracts M ON M.ID_Contract = SL.ID_Contract
		                                                WHERE SL.loan_full_number = @loanFullNumber AND SL.date_of_beginning = @DateOfBeginning";
                }
                else
                {
                    sql = @"SELECT id, sum_amd, pay_date
		                                                FROM Tbl_for_1704 deb 
		                                                WHERE deb.loan_full_number = @loanFullNumber AND deb.date_of_beginning = @DateOfBeginning
			                                            AND deb.date_of_repay IS NULL AND deb.operation_type IN (1,2,3,4,18,25,26) 
                                                        group by id, sum_amd, pay_date";
                }

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loanFullNumber", SqlDbType.BigInt).Value = leasingLoanDetails.LoanFullNumber;
                cmd.Parameters.Add("@DateOfBeginning", SqlDbType.DateTime).Value = leasingLoanDetails.StartDate;
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        OtherPayments otherPayment = new OtherPayments();

                        otherPayment.Id = Convert.ToInt32(reader["id"]);
                        otherPayment.Amount = Convert.ToDouble(reader["sum_amd"]);
                        otherPayment.PayDate = Convert.ToDateTime(reader["pay_date"]);

                        fees.Add(otherPayment);
                    }
                }
            }

            leasingLoanDetails.Fees = fees;
        }

        internal static void GetLeasingOtherPayments(LeasingLoanDetails leasingLoanDetails)
        {
            List<OtherPayments> fees = new List<OtherPayments>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand(@"SELECT id, sum_amd, pay_date
		                                                FROM Tbl_for_1704 deb 
		                                                WHERE deb.loan_full_number = @loanFullNumber AND deb.date_of_beginning = @DateOfBeginning
			                                            AND deb.date_of_repay IS NULL AND deb.operation_type IN (8,9,10,11,13,14,15,22,27,250) 
                                                        group by id, sum_amd, pay_date", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loanFullNumber", SqlDbType.BigInt).Value = leasingLoanDetails.LoanFullNumber;
                cmd.Parameters.Add("@DateOfBeginning", SqlDbType.DateTime).Value = leasingLoanDetails.StartDate;
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        OtherPayments otherPayment = new OtherPayments();

                        otherPayment.Id = Convert.ToInt32(reader["id"]);
                        otherPayment.Amount = Convert.ToDouble(reader["sum_amd"]);
                        otherPayment.PayDate = Convert.ToDateTime(reader["pay_date"]);

                        fees.Add(otherPayment);
                    }
                }
            }

            leasingLoanDetails.OtherPayments = fees;
        }

        internal static List<LeasingLoanRepayments> GetLeasingRepayments(ulong productId, byte firstReschedule = 0)
        {

            List<LeasingLoanRepayments> leasingLoanRepayments = new List<LeasingLoanRepayments>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_Customer_Leasing_Repayments";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@appID", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@firstReschedule", SqlDbType.TinyInt).Value = firstReschedule;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingLoanRepayments oneResult = new LeasingLoanRepayments
                        {
                            CapitalRepayment = dr["capital_repayment"] != DBNull.Value ? Convert.ToDouble(dr["capital_repayment"].ToString()) : 0,
                            RateRepayment = dr["rate_repayment"] != DBNull.Value ? Convert.ToDouble(dr["rate_repayment"].ToString()) : 0,
                            PayableAmount = dr["payable_amount"] != DBNull.Value ? Convert.ToDouble(dr["payable_amount"].ToString()) : 0,
                            CurrentCapital = dr["current_capital"] != DBNull.Value ? Convert.ToDouble(dr["current_capital"].ToString()) : 0,
                            DateOfRepayment = Convert.ToDateTime(dr["date_of_repayment"].ToString()),
                            InterestRate = dr["interest_rate"] != DBNull.Value ? float.Parse(dr["interest_rate"].ToString()) : 0,
                            Currency = dr["currency"].ToString()
                        };
                        leasingLoanRepayments.Add(oneResult);
                    }
                }
            }

            return leasingLoanRepayments;
        }

        internal static List<LeasingLoanStatements> GetLeasingLoanStatements(ulong productId, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, int pageNumber = 1, int pageRowCount = 15, short orderByAscDesc = 0)
        {

            List<LeasingLoanStatements> leasingLoanStatements = new List<LeasingLoanStatements>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_Customer_Leasing_Statements";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@startDate", SqlDbType.DateTime2).Value = dateFrom;
                    cmd.Parameters.Add("@endDate", SqlDbType.DateTime2).Value = dateTo;
                    cmd.Parameters.Add("@minAmount", SqlDbType.Float).Value = minAmount;
                    cmd.Parameters.Add("@maxAmount", SqlDbType.Float).Value = maxAmount;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.SmallInt).Value = pageNumber;
                    cmd.Parameters.Add("@pageRowCount", SqlDbType.SmallInt).Value = pageRowCount;
                    cmd.Parameters.Add("@orderByAmountAscDesc", SqlDbType.TinyInt).Value = orderByAscDesc;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingLoanStatements oneResult = new LeasingLoanStatements()
                        {
                            DateOfStatement = Convert.ToDateTime(dr["date_of_accounting"].ToString()),
                            Amount = dr["amount"] != DBNull.Value ? Convert.ToDouble(dr["amount"].ToString()) : 0,
                            Wording = Utility.ConvertAnsiToUnicode(dr["wording"].ToString())
                        };
                        leasingLoanStatements.Add(oneResult);
                    }
                }
            }

            return leasingLoanStatements;
        }

        internal static List<LeasingPaymentsType> GetLeasingPaymentsType()
        {
            List<LeasingPaymentsType> leasingPaymentsTypes = new List<LeasingPaymentsType>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT dbo.fnc_convertAnsiToUnicode(paymentType) paymentType, dbo.fnc_convertAnsiToUnicode(paymentName) paymentName FROM tbl_leasing_payments_type";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingPaymentsType oneResult = new LeasingPaymentsType
                        {
                            PaymentType = byte.Parse(dr["paymentType"].ToString()),
                            PaymentName = dr["PaymentName"].ToString()
                        };
                        leasingPaymentsTypes.Add(oneResult);
                    }
                }
            }

            return leasingPaymentsTypes;
        }

        internal static List<AdditionalDetails> GetLeasingDetailsByAppID(ulong productId, int leasingInsuranceId = 0)
        {
            List<AdditionalDetails> details = new List<AdditionalDetails>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                string query = @"SELECT * FROM V_Leasing_Details WHERE app_id=@productId";
                if (leasingInsuranceId > 0)
                    query += @" AND leasing_insurance_Id = @leasingInsuranceId";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                    if (leasingInsuranceId > 0)
                        cmd.Parameters.Add("@leasingInsuranceId", SqlDbType.Int).Value = leasingInsuranceId;
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "LeasingCustomerNumber", AdditionValue = dr["number"].ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "LoanFullNumber", AdditionValue = dr["loan_full_number"].ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "StartDate", AdditionValue = (Convert.ToDateTime(dr["date_of_beginning"])).ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "StartCapital", AdditionValue = (dr["start_capital"]).ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "Currency", AdditionValue = dr["currency"].ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "Description", AdditionValue = (dr["descr"]).ToString(), AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "AddDescription", AdditionValue = "", AdditionalValueType = AdditionalValueType.String });
                        details.Add(new AdditionalDetails() { AdditionTypeDescription = "AccountType", AdditionValue = "LeasingAccount", AdditionalValueType = AdditionalValueType.String });
                    }
                }
            }
            return details;
        }

        internal static Account SetLeasingReceiver()
        {
            Account receiverAccount = new Account();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT dbo.fnc_convertAnsiToUnicode(account_description) account_description, account_number, account_permission_group, account_type, currency, filial_code FROM Tbl_Leasing_Details";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        receiverAccount.AccountDescription = dr["account_description"].ToString();
                        receiverAccount.AccountNumber = dr["account_number"].ToString();
                        receiverAccount.AccountPermissionGroup = dr["account_number"].ToString();
                        receiverAccount.AccountType = dr["account_number"] != DBNull.Value ? Convert.ToByte(dr["account_type"].ToString()) : (byte)0;
                        receiverAccount.Currency = dr["currency"].ToString();
                        receiverAccount.FilialCode = dr["account_number"] != DBNull.Value ? Convert.ToInt32(dr["account_permission_group"].ToString()) : 22000;
                        receiverAccount.FreezeDate = null;
                        receiverAccount.IsAttachedCard = false;
                        receiverAccount.JointType = 0;
                        receiverAccount.Status = 0;
                        receiverAccount.TypeOfAccount = 0;
                    }
                }
            }

            return receiverAccount;
        }

        internal static string GetLeasingPaymentDescription(short paymentType, short paymentSubType)
        {
            string description = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT description FROM Tbl_sub_types_of_HB_products WHERE document_type = @paymentType AND document_sub_type = @paymentSubType", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@paymentType", SqlDbType.SmallInt).Value = paymentType;
                    cmd.Parameters.Add("@paymentSubType", SqlDbType.SmallInt).Value = paymentSubType;
                    description = Convert.ToString(cmd.ExecuteScalar());
                }
            }
            return description;
        }

        internal static LeasingLoanRepayments GetLeasingPaymentDetails(ulong productId)
        {

            LeasingLoanRepayments leasingLoanRepayments = new LeasingLoanRepayments();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT TOP 1 ABS(SL.current_capital) + dbo.Fnc_Get_Capital_difference(SL.loan_full_number,Sl.date_of_beginning,GETDATE(),SL.currency) current_capital, 
	                                        R.capital_repayment, R.rate_repayment, R.payable_amount, R.date_of_repayment, SL.currency
                                        FROM [Tbl_short_time_loans;] SL INNER JOIN [Tbl_repayments_of_bl;] R ON SL.loan_full_number = R.loan_full_number AND SL.date_of_beginning = R.date_of_beginning
                                        WHERE SL.app_id = @productId AND R.date_of_repayment >= GETDATE() ORDER BY R.date_of_repayment";
                    cmd.CommandType = CommandType.Text;
                    SqlParameter param = new SqlParameter();
                    param = cmd.Parameters.Add("@productId", SqlDbType.BigInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = productId;
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        leasingLoanRepayments.CapitalRepayment = dr["capital_repayment"] != DBNull.Value ? Convert.ToDouble(dr["capital_repayment"].ToString()) : 0;
                        leasingLoanRepayments.RateRepayment = dr["rate_repayment"] != DBNull.Value ? Convert.ToDouble(dr["rate_repayment"].ToString()) : 0;
                        leasingLoanRepayments.PayableAmount = dr["payable_amount"] != DBNull.Value ? Convert.ToDouble(dr["payable_amount"].ToString()) : 0;
                        leasingLoanRepayments.CurrentCapital = dr["current_capital"] != DBNull.Value ? Convert.ToDouble(dr["current_capital"].ToString()) : 0;
                        leasingLoanRepayments.DateOfRepayment = Convert.ToDateTime(dr["date_of_repayment"].ToString());
                        leasingLoanRepayments.Currency = dr["currency"].ToString();
                    }
                }
            }

            return leasingLoanRepayments;
        }

        internal static List<LeasingOverdueDetail> GetLeasingOverdueDetails(ulong productId)
        {
            List<LeasingOverdueDetail> leasingOverdueDetails = new List<LeasingOverdueDetail>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_Leasing_Overdue_Details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@appID", SqlDbType.BigInt).Value = productId;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingOverdueDetail oneResult = new LeasingOverdueDetail
                        {
                            Productid = dr["app_id"] != DBNull.Value ? long.Parse(dr["app_id"].ToString()) : 0,
                            ProductType = dr["product_type"] != DBNull.Value ? Convert.ToByte(dr["product_type"].ToString()) : (byte)0,
                            ProductTypeDescription = Utility.ConvertAnsiToUnicode(dr["product_description"].ToString()),
                            ProductStartDate = Convert.ToDateTime(dr["date_of_beginning"].ToString()),
                            ProductEndDate = Convert.ToDateTime(dr["date_of_normal_end"].ToString()),
                            StartCapital = dr["start_capital"] != DBNull.Value ? Convert.ToDouble(dr["start_capital"].ToString()) : 0,
                            Currency = dr["currency"].ToString(),
                            StartDate = Convert.ToDateTime(dr["overdue_start_date"].ToString()),
                            EndDate = dr["overdue_end_date"] != DBNull.Value ? Convert.ToDateTime(dr["overdue_end_date"].ToString()) : (DateTime?)null,
                            OverdueDaysCount = dr["overdue_days"] != DBNull.Value ? Convert.ToUInt16(dr["overdue_days"].ToString()) : (ushort)0,
                            Amount = dr["overdue_amount"] != DBNull.Value ? Convert.ToDouble(dr["overdue_amount"].ToString()) : 0,
                            RateAmount = dr["overdue_rate"] != DBNull.Value ? Convert.ToDouble(dr["overdue_rate"].ToString()) : 0,
                            OverdueCurrency = dr["overdue_amount_currency"].ToString(),
                            Quality = dr["quality"] != DBNull.Value ? Convert.ToByte(dr["quality"].ToString()) : (byte)0
                        };
                        leasingOverdueDetails.Add(oneResult);
                    }
                }
            }

            return leasingOverdueDetails;
        }

        internal static ulong GetManagerCustomerNumber(ulong customerNumber)
        {
            ulong managerCustomerNumber = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT [dbo].[Fn_Get_Manager_Customer_Number](@customerNumber) customer_number", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    managerCustomerNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return managerCustomerNumber;
        }

        internal static List<LeasingInsurance> GetLeasingInsurances(ulong productId)
        {
            List<LeasingInsurance> incurances = new List<LeasingInsurance>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM V_Leasing_Details WHERE app_id=@productId ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        LeasingInsurance oneResult = new LeasingInsurance()
                        {
                            InsuranceId = dr["leasing_insurance_Id"] != DBNull.Value ? Convert.ToInt32(dr["leasing_insurance_Id"].ToString()) : 0,
                            InsuranceDescription = dr["insurance_description"] != DBNull.Value ? dr["insurance_description"].ToString() : string.Empty,
                            Amount = dr["insurance_sum"] != DBNull.Value ? Convert.ToDouble(dr["insurance_sum"].ToString()) : 0,
                            PayDate = dr["pay_date"] != DBNull.Value ? Convert.ToDateTime(dr["pay_date"].ToString()) : (DateTime?)null
                        };
                        incurances.Add(oneResult);
                    }
                }
            }
            return incurances;
        }

        internal static bool CheckNeedToFindoutLeasingDetails(long appId)
        {
            bool need = false;
            DateTime bankOperDay = UtilityDB.GetCurrentOperDay();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT R.date_of_repayment , [dbo].[get_oper_day]() AS current_oper_day, [dbo].[Fnc_Check_need_to_find_out_leasing_details] (SL.app_id) AS need
                                                        FROM [Tbl_short_time_loans;] SL
	                                                        OUTER APPLY (SELECT TOP 1 capital_repayment + CASE WHEN ISNULL(SL.subsid_rate,0)>0 THEN subsidy_rate_repayment ELSE rate_repayment END  payable_amount, date_of_repayment 
					                                                        FROM [Tbl_repayments_of_bl;]						
					                                                        WHERE loan_full_number = SL.loan_full_number AND date_of_beginning = SL.date_of_beginning AND date_of_repayment >= CONVERT(DATETIME,CONVERT(NVARCHAR(50),GETDATE(),101))
					                                                        ORDER BY date_of_repayment) R
                                                        WHERE SL.app_id = @appId", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = appId;

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        if(Convert.ToBoolean(dr["need"]) == true)
                        {
                            if(Convert.ToDateTime(dr["date_of_repayment"]) >= Convert.ToDateTime(dr["current_oper_day"]) && Convert.ToDateTime(dr["date_of_repayment"]) < bankOperDay)
                            {
                                need = true;
                            }
                        }
                    }
                }
            }
            return need;
        }
    }
}
