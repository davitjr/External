using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Data.SqlTypes;
using System.IO;
using ExternalBanking.ContractServiceRef;
using System.Linq;

namespace ExternalBanking.DBManager
{
    class LoanProductOrderDB
    {

        /// <summary>
        /// Ավանդի գրավով վարկի հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveLoanOrder(LoanProductOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewLoanDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;


                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = order.StartDate.Date;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = order.EndDate.Date;
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.SmallDateTime).Value = order.FirstRepaymentDate.Date;

                    cmd.Parameters.Add("@repayment_period", SqlDbType.SmallInt).Value = 1;


                    cmd.Parameters.Add("@loan_percent", SqlDbType.Float).Value = order.InterestRate;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;

                    if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                    {
                        cmd.Parameters.Add("@provision_currency", SqlDbType.VarChar, 5).Value = order.PledgeCurrency;
                        if (order.Type == OrderType.CreditSecureDeposit)
                        {
                            cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.FeeAmount;
                            cmd.Parameters.Add("@service_fee_account", SqlDbType.VarChar, 50).Value = order.FeeAccount.AccountNumber;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add("@provision_account", SqlDbType.VarChar, 50).Value = order.ProvisionAccount.AccountNumber;
                        cmd.Parameters.Add("@provision_currency", SqlDbType.VarChar, 5).Value = order.ProvisionCurrency;
                        cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = 0;
                        cmd.Parameters.Add("@service_fee_account", SqlDbType.VarChar, 50).Value = 0;

                    }


                    cmd.Parameters.Add("@provision_amount", SqlDbType.Float).Value = order.ProvisionAmount;
                    cmd.Parameters.Add("@dispute_resolution", SqlDbType.TinyInt).Value = order.DisputeResolution;
                    cmd.Parameters.Add("@communication_type", SqlDbType.TinyInt).Value = order.CommunicationType;
                    cmd.Parameters.Add("@use_country", SqlDbType.Int).Value = order.LoanUseCountry;
                    cmd.Parameters.Add("@use_locality", SqlDbType.Int).Value = order.LoanUseLocality;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0 || actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }
        }

        /// <summary>
        /// Ավանդի գրավով վարկային գծի հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveCreditLineOrder(LoanProductOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewCreditLineDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking) && order.Currency != "AMD")
                    {
                        cmd.Parameters.Add("@amountInAMD", SqlDbType.Float).Value = order.AmountInAMD;
                    }

                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = order.StartDate.Date;

                    if (order.ProductType == 50 || order.ProductType == 51)
                    {
                        cmd.Parameters.Add("@end_date", SqlDbType.DateTime).Value = order.ValidationDate?.Date;

                    }
                    else
                    {
                        cmd.Parameters.Add("@end_date", SqlDbType.DateTime).Value = order.EndDate.Date;

                    }
                    cmd.Parameters.Add("@credit_line_percent", SqlDbType.Float).Value = order.InterestRate;
                    cmd.Parameters.Add("@credit_line_account", SqlDbType.VarChar, 50).Value = order.ProductAccount.AccountNumber;
                    if (order.Type != OrderType.FastOverdraftApplication && order.Type != OrderType.LoanApplicationConfirmation)
                    {
                        if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                        {
                            cmd.Parameters.Add("@provision_currency", SqlDbType.VarChar, 5).Value = order.PledgeCurrency;
                        }
                        else
                        {
                            cmd.Parameters.Add("@provision_account", SqlDbType.VarChar, 50).Value = order.ProvisionAccount.AccountNumber;
                            cmd.Parameters.Add("@provision_currency", SqlDbType.VarChar, 5).Value = order.ProvisionCurrency;
                        }
                        cmd.Parameters.Add("@provision_amount", SqlDbType.Float).Value = order.ProvisionAmount;
                        cmd.Parameters.Add("@credit_line_type ", SqlDbType.SmallInt).Value = order.ProductType;

                    }
                    else
                    {
                        cmd.Parameters.Add("@credit_line_type ", SqlDbType.SmallInt).Value = 54;
                    }

                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@dispute_resolution", SqlDbType.TinyInt).Value = order.DisputeResolution;
                    cmd.Parameters.Add("@communication_type", SqlDbType.TinyInt).Value = order.CommunicationType;
                    cmd.Parameters.Add("@use_country", SqlDbType.Int).Value = order.LoanUseCountry;
                    cmd.Parameters.Add("@use_locality", SqlDbType.Int).Value = order.LoanUseLocality;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;

                    if (order.ProductType == 51 || order.ProductType == 50)
                    {
                        cmd.Parameters.Add("@mandatoryPayment", SqlDbType.Bit).Value = true;
                        order.MandatoryPayment = true;
                    }
                    else if (order.ProductType == 30)
                    {
                        cmd.Parameters.Add("@mandatoryPayment", SqlDbType.Bit).Value = order.MandatoryPayment;
                    }

                    if (order.Type == OrderType.FastOverdraftApplication || order.Type == OrderType.LoanApplicationConfirmation)
                    {
                        cmd.Parameters.Add("@repayment_percent", SqlDbType.Float).Value = (order.Currency == order.ProvisionCurrency || order.Type == OrderType.FastOverdraftApplication || order.Type == OrderType.LoanApplicationConfirmation) ? 0 : 0.1;
                    }
                    else
                    {
                        cmd.Parameters.Add("@repayment_percent", SqlDbType.Float).Value = (order.ProductType == 51 || order.ProductType == 50) ? 0 : GetDepositCreditLineRepaymentPercent(order.Currency, order.ProvisionCurrency, order.MandatoryPayment, order.ProductAccount.AccountNumber);

                    }
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@doc_type ", SqlDbType.SmallInt).Value = (ushort)order.Type;

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0 || actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }
        }


        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static LoanProductOrder GetLoanOrder(LoanProductOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@" SELECT d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,n.*,d.operation_date,A.region,d.order_group_id,d.confirmation_date                                             
		                                           FROM Tbl_HB_documents as d left join Tbl_New_Loan_Documents as n on  d.doc_ID=n.Doc_ID
                                                   LEFT JOIN tbl_armenian_places A ON N.use_of_arm_place=A.number 
                                                   WHERE d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
              
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.StartDate = DateTime.Parse(dt.Rows[0]["start_date"].ToString());
                order.EndDate = DateTime.Parse(dt.Rows[0]["end_date"].ToString());
                order.ProductType = int.Parse(dt.Rows[0]["loan_type"].ToString());
                order.InterestRate = double.Parse(dt.Rows[0]["loan_percent"].ToString());
                order.ProductAccount = dt.Rows[0]["loan_account"] != DBNull.Value ? Account.GetAccount(dt.Rows[0]["loan_account"].ToString()) : null;
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                {
                    order.PledgeCurrency = dt.Rows[0]["provision_currency"] != DBNull.Value ? dt.Rows[0]["provision_currency"].ToString() : null;
                }
                else
                {
                    order.ProvisionAccount = Account.GetAccount(dt.Rows[0]["provision_account"].ToString());
                    order.ProvisionAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.ProvisionAccount.AccountTypeDescription);
                    order.ProvisionCurrency = dt.Rows[0]["provision_currency"].ToString();

                }
                order.ProvisionAmount = double.Parse(dt.Rows[0]["provision_amount"].ToString());
                order.FirstRepaymentDate = DateTime.Parse(dt.Rows[0]["first_repayment_date"].ToString());
                order.FeeAccount = Account.GetAccount(dt.Rows[0]["service_fee_account"].ToString());
                if (order.FeeAccount != null)
                    order.FeeAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccount.AccountTypeDescription);
                order.FeeAmount = double.Parse(dt.Rows[0]["service_fee"].ToString());
                order.DisputeResolution = dt.Rows[0]["dispute_resolution"] != DBNull.Value ? short.Parse(dt.Rows[0]["dispute_resolution"].ToString()) : (short)0;
                order.CommunicationType = dt.Rows[0]["communication_type"] != DBNull.Value ? short.Parse(dt.Rows[0]["communication_type"].ToString()) : (short)0;
                order.LoanUseCountry = dt.Rows[0]["use_of_Country"] != DBNull.Value ? int.Parse(dt.Rows[0]["use_of_Country"].ToString()) : 0;
                order.LoanUseRegion = dt.Rows[0]["region"] != DBNull.Value ? int.Parse(dt.Rows[0]["region"].ToString()) : 0;
                order.LoanUseLocality = dt.Rows[0]["use_of_arm_place"] != DBNull.Value ? int.Parse(dt.Rows[0]["use_of_arm_place"].ToString()) : 0;
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.AcknowledgedByCheckBox = dt.Rows[0]["acknowledged_by_checkbox"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["acknowledged_by_checkbox"]) : false;
                order.AcknowledgementText = dt.Rows[0]["acknowledgement_text"] != DBNull.Value ? dt.Rows[0]["acknowledgement_text"].ToString() : "";

            }
            return order;
        }


        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկային գծի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static LoanProductOrder GetCreditLineOrder(LoanProductOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@" SELECT
                                                   
                                                    d.amount,
                                                    d.currency,
                                                    d.registration_date,
                                                    d.document_number,
                                                    d.customer_number,
                                                    d.document_type,
                                                    d.document_subtype,
                                                    d.quality,
                                                    n.*,
                                                    d.operation_date,
                                                    d.source_type,
                                                    A.region,
                                                    id.App_ID,
                                                    fee.FeeAmount,
                                                    d.order_group_id,
                                                    d.confirmation_date
		                                            FROM Tbl_HB_documents AS d 
                                                    LEFT JOIN Tbl_New_Credit_Line_Documents AS n
                                                    ON  d.doc_ID=n.Doc_ID
                                                    LEFT JOIN [dbo].[Tbl_armenian_places] A ON N.use_of_arm_place=A.number  
	                                                LEFT JOIN Tbl_HB_Products_Identity id
													ON id.HB_Doc_ID=d.doc_ID
                                                    LEFT JOIN Tbl_Transfer_Fees fee
													ON fee.doc_ID=d.doc_ID
                                                    WHERE d.Doc_ID=@DocID AND d.customer_number=CASE WHEN @customer_number = 0 THEN d.customer_number ELSE @customer_number END", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["start_date"].ToString());
                order.ProductType = int.Parse(dt.Rows[0]["credit_line_type"].ToString());

                if (order.ProductType == 51 || order.ProductType == 50)
                {
                    order.ValidationDate = DateTime.Parse(dt.Rows[0]["end_date"].ToString());

                }
                else
                {
                    order.EndDate = DateTime.Parse(dt.Rows[0]["end_date"].ToString());

                }

                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());


                order.InterestRate = double.Parse(dt.Rows[0]["credit_line_percent"].ToString());
                order.ProductAccount = dt.Rows[0]["credit_line_account"] != DBNull.Value ? Account.GetAccount(dt.Rows[0]["credit_line_account"].ToString()) : null;
                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                {
                    order.PledgeCurrency = dt.Rows[0]["provision_currency"] != DBNull.Value ? dt.Rows[0]["provision_currency"].ToString() : null;
                }
                else
                {
                    order.ProvisionAccount = dt.Rows[0]["provision_account"] != DBNull.Value ? Account.GetAccount(dt.Rows[0]["provision_account"].ToString()) : null;
                    if (order.ProvisionAccount != null)
                        order.ProvisionAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.ProvisionAccount.AccountTypeDescription);
                    order.ProvisionCurrency = dt.Rows[0]["provision_currency"] != DBNull.Value ? dt.Rows[0]["provision_currency"].ToString() : null;
                }



                order.ProvisionAmount = dt.Rows[0]["provision_amount"] != DBNull.Value ? double.Parse(dt.Rows[0]["provision_amount"].ToString()) : 0;
                if (Convert.ToInt16(dt.Rows[0]["source_type"].ToString()) != (short)SourceType.AcbaOnline)
                {
                    order.DisputeResolution = short.Parse(dt.Rows[0]["dispute_resolution"].ToString());
                    order.CommunicationType = short.Parse(dt.Rows[0]["communication_type"].ToString());
                }
                order.LoanUseCountry = dt.Rows[0]["use_of_Country"] != DBNull.Value ? int.Parse(dt.Rows[0]["use_of_Country"].ToString()) : 0;
                order.LoanUseRegion = dt.Rows[0]["region"] != DBNull.Value ? int.Parse(dt.Rows[0]["region"].ToString()) : 0;
                order.LoanUseLocality = dt.Rows[0]["use_of_arm_place"] != DBNull.Value ? int.Parse(dt.Rows[0]["use_of_arm_place"].ToString()) : 0;

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                if (dt.Rows[0]["App_ID"] != DBNull.Value)
                    order.ProductId = Convert.ToUInt64(dt.Rows[0]["App_ID"]);
                if (order.Currency != "AMD" && dt.Rows[0]["amount_in_AMD"] != DBNull.Value)
                    order.AmountInAMD = Convert.ToDouble(dt.Rows[0]["amount_in_AMD"]);
                else
                    order.AmountInAMD = order.Amount;
                if (dt.Rows[0]["FeeAmount"] != DBNull.Value)
                    order.FeeAmount = Convert.ToDouble(dt.Rows[0]["FeeAmount"]);


                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

                order.AcknowledgedByCheckBox = dt.Rows[0]["acknowledged_by_checkbox"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["acknowledged_by_checkbox"]) : false;
                order.AcknowledgementText = dt.Rows[0]["acknowledgement_text"] != DBNull.Value ? dt.Rows[0]["acknowledgement_text"].ToString() : "";
                order.MandatoryPayment = dt.Rows[0]["mandatory_payment"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["mandatory_payment"]) : false;
            }
            return order;
        }

        internal static double GetInterestRateForCreditLine(string cardNumber)
        {
            double interestRate = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT [dbo].[fn_GetInterestRateForCreditLine](@cardNumber) as interest_rate", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        Double.TryParse(dr["interest_rate"].ToString(), out interestRate);
                    }
                    if (interestRate < 0)
                        interestRate = 0;
                }
            }

            return interestRate;
        }

        internal static bool CheckLoanRequest(LoanProductOrder order)
        {
            bool check = false;

            using(SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string tbl = "", condition = "";
                if (order.Type == OrderType.CreditSecureDeposit)
                {
                    tbl = "[tbl_short_time_loans;]";
                    condition = " and app_id not in (SELECT S.app_id FROM[tbl_short_time_loans;] S INNER JOIN dbo.Tbl_HB_Products_Identity IDENT ON S.APP_ID = IDENT.App_ID INNER JOIN Tbl_HB_documents HB ON IDENT.HB_Doc_ID = HB.DOC_Id AND HB.quality = 20) ";
                }
                else if (order.Type == OrderType.CreditLineSecureDeposit)
                {
                    tbl = "[Tbl_credit_lines]";
                }

                SqlCommand cmd = new SqlCommand("SELECT loan_full_number FROM " + tbl + " WHERE quality = 10 and customer_number = @customerNumber" + condition, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.VarChar, 16).Value = order.CustomerNumber;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        check = true;
                    }
                }
            }

            return check;

        }

        internal static ActionError ValidateLoanProduct(LoanProductOrder order)
        {
            ActionError result = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_deposit_loan_validations";
                    cmd.CommandType = CommandType.StoredProcedure;

                    int loanType = 0;

                    if (order.Type == OrderType.CreditSecureDeposit)
                    {
                        loanType = 29;
                    }
                    else if (order.Type == OrderType.CreditLineSecureDeposit)
                    {
                        loanType = order.ProductType;
                    }

                    cmd.Parameters.Add("@loanType", SqlDbType.Int).Value = loanType;
                    cmd.Parameters.Add("@startCapital", SqlDbType.Money).Value = order.Amount;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@interestRate", SqlDbType.Float).Value = order.InterestRate;
                    cmd.Parameters.Add("@dateOfBeginning", SqlDbType.SmallDateTime).Value = order.StartDate.Date;
                    if (order.ProductType == 50 || order.ProductType == 51)
                    {
                        cmd.Parameters.Add("@dateOfNormalEnd", SqlDbType.DateTime).Value = order.ValidationDate?.Date;

                    }
                    else
                    {
                        cmd.Parameters.Add("@dateOfNormalEnd", SqlDbType.DateTime).Value = order.EndDate.Date;

                    }

                    if (order.Type == OrderType.CreditLineSecureDeposit)
                    {
                        string cardNumber = "";
                        Card card = Card.GetCardWithOutBallance(order.ProductAccount.AccountNumber);
                        if (card != null)
                        {
                            cardNumber = card.CardNumber;
                        }
                        cmd.Parameters.Add("@visaNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                    }


                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        short errCode = 0;
                        Int16.TryParse(dr["errorHBNumber"].ToString(), out errCode);
                        if (errCode != 0)
                        {
                            result = new ActionError(errCode);
                        }

                    }

                    return result;
                }

            }
        }




        /// <summary>
        /// Վերադարձնում է Ավանդի գրավով վարկի գումարի և գրավի գումարի հարաբերակցության առավելագույն տոկոսը
        /// </summary>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <returns></returns>
        internal static double GetDepositLoanAndProvisionCoefficent(string loanCurrency, string provisionCurrency)
        {
            double coefficent = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select dbo.fn_get_deposit_loan_and_provision_coefficent(@loanCurrency,@provisionCurrency) as syntheticStatus", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loanCurrency", SqlDbType.NVarChar, 3).Value = loanCurrency;
                cmd.Parameters.Add("@provisionCurrency", SqlDbType.NVarChar, 3).Value = provisionCurrency;


                coefficent = Convert.ToDouble(cmd.ExecuteScalar());
            }



            return coefficent;
        }
        /// <summary>
        /// Վերադարձնում է Ավանդի գրավով վարկային գծի գումարի և գրավի գումարի հարաբերակցության առավելագույն տոկոսը
        /// </summary>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <param name="mandatoryPayment"></param>
        /// <returns></returns>
        internal static double GetDepositLoanCreditLineAndProfisionCoefficent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType)
        {
            double coefficent = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select dbo.fn_get_deposit_credit_line_and_provision_coefficent(@loanCurrency,@provisionCurrency, @needRepayment,@creditLineType) as syntheticStatus", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loanCurrency", SqlDbType.NVarChar, 3).Value = loanCurrency;
                cmd.Parameters.Add("@provisionCurrency", SqlDbType.NVarChar, 3).Value = provisionCurrency;
                cmd.Parameters.Add("@needRepayment", SqlDbType.Bit).Value = mandatoryPayment;
                cmd.Parameters.Add("@creditLineType", SqlDbType.Int).Value = creditLineType;

                coefficent = Convert.ToDouble(cmd.ExecuteScalar());
            }

            return coefficent;
        }




        internal static double GetInterestRateForDepositLoan(DateTime startDate, DateTime endDate)
        {
            double interestRate = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT [dbo].[fn_get_interest_rate_for_DepositLoan](@startDate,@endDate) as interest_rate", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate;
                cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = endDate;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        Double.TryParse(dr["interest_rate"].ToString(), out interestRate);
                    }
                    if (interestRate < 0)
                        interestRate = 0;
                }
            }

            return interestRate;
        }


        internal static List<ActionError> FastOverdraftValidations(ulong customerNumber, SourceType source, string cardNumber)
        {
            List<ActionError> errors = new List<ActionError>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[fn_fast_overdraft_validations](@customerNumber,@cardNumber,@bankingSource)", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                if (!string.IsNullOrEmpty(cardNumber))
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                else
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = DBNull.Value;
                cmd.Parameters.Add("@bankingSource", SqlDbType.Int).Value = (ushort)source;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ActionError error = new ActionError(Convert.ToInt16(dt.Rows[i]["err_number"]));
                        errors.Add(error);
                    }

                }
            }

            return errors;
        }



        internal static ActionResult SaveLoanApplicationQualityChangeOrder(LoanProductOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                                                    declare @filial as int
                                                    SELECT @filial=filialcode FROM dbo.Tbl_customers WHERE customer_number=@customer_number   
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,quality, 
                                                    source_type,operationFilialCode,operation_date,description,order_group_id)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@doc_sub_type,
                                                    1,@source_type,@operation_filial_code,@oper_day,@description,@group_id)
                                                    Select Scope_identity() as ID
                                                     ", conn);
                cmd.CommandType = CommandType.Text;


                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                if (!string.IsNullOrEmpty(order.Description))
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 150).Value = Utility.ConvertUnicodeToAnsi(order.Description);
                else
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 150).Value = DBNull.Value;

                if (order.GroupId != 0)
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                else
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = DBNull.Value;

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());
                result.Id = order.Id;
                result.ResultCode = ResultCode.Normal;
                return result;
            }

        }



        internal static bool CheckLoanApplication(string cardNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select app_id from Tbl_loan_applications where CardNumber=@card_number and (status=1 or status=2) and loan_type=54 and wrong_app = 0", conn);

                cmd.Parameters.Add("@card_number", SqlDbType.NVarChar).Value = cardNumber;

                if (cmd.ExecuteReader().Read())
                {
                    return true;
                }
                else
                    return false;


            }

        }

        internal static bool IsSecontLoanApplication(ulong productId, ushort orderType, long docId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT doc_ID FROM Tbl_HB_Products_Identity id
                                                    INNER JOIN Tbl_HB_documents hb
                                                    ON hb.doc_ID=id.HB_Doc_ID
                                                    WHERE App_ID=@AppID and hb.quality in(2,3,5,50,100) AND document_type=@documentType AND doc_ID<>@docId", conn);

                cmd.Parameters.Add("@AppID", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@documentType", SqlDbType.SmallInt).Value = orderType;
                cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;

                if (cmd.ExecuteReader().Read())
                {
                    return true;
                }
                else
                    return false;


            }

        }

        internal static double GetDepositCreditLineRepaymentPercent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, string cardAccountNumber)
        {
            double repaymentPercent = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT [dbo].[fn_get_deposit_credit_line_repayment_percent](@loanCurrency,@provisionCurrency,@needRepayment,@cardAccountNumber) as repaymentPercent", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loanCurrency", SqlDbType.NVarChar, 16).Value = loanCurrency;
                cmd.Parameters.Add("@provisionCurrency", SqlDbType.NVarChar, 3).Value = provisionCurrency;
                cmd.Parameters.Add("@needRepayment", SqlDbType.Bit).Value = mandatoryPayment;
                cmd.Parameters.Add("@cardAccountNumber", SqlDbType.NVarChar, 16).Value = cardAccountNumber;

                repaymentPercent = Convert.ToDouble(cmd.ExecuteScalar());
            }

            return repaymentPercent;
        }

        internal static Tuple<int, int> SetCustomerCountryAndLocality(ulong customerNumber)
        {
            int country = 0;
            int useLocality = 0;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select case when ISNULL(C.country, 0)= 0 then d.country else c.country end as country,    
                                        case when isnull(d.townVillage,0) = 0 then c.townVillage_code else  d.townVillage  end as townvillage 
                                        from Tbl_Customers  a 
                                        inner join Tbl_Customer_Addresses b
                                        on a.identityId = b.identityId 
                                        left  join Tbl_Foreign_Addresses C 
                                        ON B.addressid = c.id
                                        left join  Tbl_Addresses d 
                                        on b.addressid = d.id
                                        where b.addressType =1  and customer_number = @customer_Number", conn);


                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (dr["country"] == DBNull.Value)
                    {
                        country = 51;
                        useLocality = 1009;
                        return Tuple.Create(country, useLocality);
                    }
                    else
                    {
                        country = Convert.ToInt32(dr["country"]);
                    }

                    if (country != 999 && country != 51)
                    {
                        useLocality = 9999;
                        return Tuple.Create(country, useLocality);
                    }

                    if ((country == 51 || country == 999) && (dr["townvillage"] == DBNull.Value || dr["townvillage"].ToString() == "-1" || dr["townvillage"].ToString() == "0"))
                    {
                        if (country == 51)
                        {
                            useLocality = 1009;
                        }
                        else if (country == 999)
                        {
                            useLocality = 9998;
                        }

                        return Tuple.Create(country, useLocality);
                    }

                    if (dr["townvillage"] != DBNull.Value && dr["townvillage"].ToString() != "-1" && dr["townvillage"].ToString() != "0")
                    {
                        useLocality = Convert.ToInt32(dr["townvillage"]);
                    }
                }
            }
            return Tuple.Create(country, useLocality);
        }

        internal static double GetRedemptionAmountForDepositLoan(double startCapital, double interestRate, DateTime dateOfBeginning, DateTime dateOfNormalEnd, DateTime firstRepaymentDate)
        {
            double redemptionAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("pr_get_array_of_repayments", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@grafikMode", SqlDbType.TinyInt).Value = 1;
                cmd.Parameters.Add("@startCapital", SqlDbType.Float).Value = startCapital;
                cmd.Parameters.Add("@interestRate", SqlDbType.Float).Value = interestRate;
                cmd.Parameters.Add("@dateOfBeginning", SqlDbType.SmallDateTime).Value = dateOfBeginning;
                cmd.Parameters.Add("@dateOfNormalEnd", SqlDbType.SmallDateTime).Value = dateOfNormalEnd;
                cmd.Parameters.Add("@firstRepaymentDate", SqlDbType.SmallDateTime).Value = firstRepaymentDate;
                cmd.Parameters.Add("@period", SqlDbType.NVarChar, 500).Value = "m";
                cmd.Parameters.Add("@periodCoefficient", SqlDbType.Float).Value = 1;
                cmd.Parameters.Add("@yearDays", SqlDbType.NVarChar, 500).Value = 365;

                cmd.Parameters.Add("@onlyRatePeriodsNumber", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@changedRateCalculationType", SqlDbType.TinyInt).Value = 0;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    redemptionAmount = Convert.ToDouble(dt.Rows[0]["capital_repayment"]) + Convert.ToDouble(dt.Rows[0]["rate_repayment"]) + Convert.ToDouble(dt.Rows[0]["fee_repayment"]);
                }

            }

            return redemptionAmount;
        }


        internal static double GetCommisionAmountForDepositLoan(double startCapital, DateTime dateOfBeginning, DateTime dateofNormalEnd, string currency, ulong customerNumber)
        {
            double commisionAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT dbo.fnc_calculate_commission_amount(0,@dateofbeginning,@customerNumber,@StartCapital,@Currency,29,@dateofbeginning,@DateOfNormalEnd,0,0,0,0,0,0)", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@dateofbeginning", SqlDbType.DateTime).Value = dateOfBeginning;
                cmd.Parameters.Add("@DateOfNormalEnd", SqlDbType.DateTime).Value = dateofNormalEnd;
                cmd.Parameters.Add("@StartCapital", SqlDbType.Float).Value = startCapital;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                commisionAmount = Convert.ToDouble(cmd.ExecuteScalar());
            }

            return commisionAmount;
        }




        internal static double GetCreditLineDecreasingAmount(double startCapital, string currency, DateTime startDate, DateTime endDate)
        {
            double decrAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("pr_get_credit_line_decreasing_amount", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = startDate;
                cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = endDate;
                cmd.Parameters.Add("@start_capital", SqlDbType.Float).Value = startCapital;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    decrAmount = Convert.ToDouble(dt.Rows[0]["decreasingAmount"]);
                }
            }
            return decrAmount;
        }

        internal static DataTable GetDepositCreditLineContractInfo(int docid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("security_code_2");
            dt.Columns.Add("repayment_percent");
            dt.Columns.Add("repayment_kurs");
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBASEConn"].ToString()))
            {
                string sql = @"select n.security_code_2,
                                n.repayment_percent,
                                pre.repayment_kurs from  Tbl_New_Credit_Line_Documents AS n
								left join  Tbl_HB_loan_precontract_data pre 
								on n.doc_id =  pre.doc_id where n.doc_id =  @docid ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docid", SqlDbType.Int).Value = docid;

                    conn.Open();


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        internal static DataTable GetDepositLoanContractInfo(int docid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("security_code_2");
            dt.Columns.Add("interest_rate_effective");
            dt.Columns.Add("credit_code");
            dt.Columns.Add("interest_rate_effective_without_account_service_fee");
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBASEConn"].ToString()))
            {
                string sql = @"SELECT l.security_code_2,
                                      p.interest_rate_effective,
                                      l.credit_code,
                                      p.interest_rate_effective_without_account_service_fee 
								FROM  [dbo].[Tbl_New_Loan_Documents] L 
                                INNER JOIN [dbo].[Tbl_HB_loan_precontract_data] P ON L.doc_id = P.doc_id 
                                WHERE L.doc_id =  @docid ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docid", SqlDbType.Int).Value = docid;

                    conn.Open();


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }


        public static string GetConnectAccountFullNumber(ulong customerNumber, string currency)
        {

            string accountNumber = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.From_CustomerNumberCurrentAccount(@customerNumber,@currency)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    accountNumber = cmd.ExecuteScalar().ToString();
                }
            }
            return accountNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loanAccount"></param>
        /// <param name="type">1 ավանդի գրավով վարկ,  2 ավանդի գրավով վարկային գիծ</param>
        /// <returns></returns>
        internal static byte[] GetDepositLoanOrDepositCreditLineContract(string loanAccount, byte type)
        {
            byte[] arr = null;
            int docid = 0;
            string sql = "";
            int attachType = 0;
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                if (type == 1)
                {
                    sql = @"select HB_doc_ID  from [Tbl_short_time_loans;] where loan_full_number = @loanAccount";
                }
                else if (type == 2)
                {
                    sql = @"select HB_doc_ID  from Tbl_credit_lines where loan_full_number = @loanAccount";
                }
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@loanAccount", SqlDbType.Float).Value = loanAccount;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        docid = Convert.ToInt32(dt.Rows[0]["HB_doc_ID"].ToString());
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (type == 1)
            {
                attachType = 7;
            }
            else if (type == 2)
            {
                attachType = 12;
            }

            return UploadedFile.GetAttachedFile(docid, attachType);

        }
        
        public static byte[] PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            byte[] result;
            Customer customer = new Customer();
            LoanProductOrder loanProductOrder = customer.GetLoanOrder(docId);


            Dictionary<string, string> contractInfo = new Dictionary<string, string>();
            DataTable dt = new DataTable();


            dt = LoanProductOrder.GetDepositLoanContractInfo((int)docId);

            if (dt.Rows.Count > 0)
            {
                contractInfo.Add("security_code_2", dt.Rows[0][0].ToString());
                contractInfo.Add("interest_rate_effective", dt.Rows[0][1].ToString());
                contractInfo.Add("credit_code", dt.Rows[0][2].ToString());
                contractInfo.Add("interest_rate_effective_without_account_service_fee", dt.Rows[0][3].ToString());
            }


            Dictionary<string, string> parameters = new Dictionary<string, string>();
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;
            double kurs;
            if (loanProductOrder.Source == SourceType.AcbaOnline || loanProductOrder.Source == SourceType.MobileBanking)
            {
                kurs = Utility.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.PledgeCurrency);
            }
            else
            {
                kurs = Utility.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.ProvisionCurrency);
            }

            string connectAccountFullNumberHB = LoanProductOrder.GetConnectAccountFullNumber(customerNumber, loanProductOrder.Currency);
            double penaltyRate = Info.GetPenaltyRateOfLoans(29, loanProductOrder.StartDate);
            string contractName = "ConsumeLoanContract";

            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());

            ContractServiceRef.Contract contract = null;
            if (fromApprove == true)
            {
                contract = new ContractServiceRef.Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 7;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }

            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "dateOfBeginningHB", value: DateTime.Now.Date.ToString("dd/MMM/yy"));
            parameters.Add(key: "dateOfNormalEndHB", value: loanProductOrder.EndDate.ToString("dd/MMM/yy"));
            parameters.Add(key: "currencyHB", value: loanProductOrder.Currency);
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "startCapitalHB", value: loanProductOrder.Amount.ToString());
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: filialCode.ToString());
            parameters.Add(key: "loanTypeHB", value: loanProductOrder.ProductType.ToString());
            parameters.Add(key: "securityCodeHB", value: contractInfo["security_code_2"].ToString());
            parameters.Add(key: "loanProvisionPercentHB", value: ((loanProductOrder.Amount / loanProductOrder.ProvisionAmount) * kurs * 100).ToString());
            parameters.Add(key: "provisionNumberHB", value: "01");
            parameters.Add(key: "repaymentAmountHB", value: loanProductOrder.FeeAmount.ToString());
            parameters.Add(key: "interestRateHB", value: loanProductOrder.InterestRate.ToString());
            parameters.Add(key: "interestRateFullHB", value: (Convert.ToDouble(contractInfo["interest_rate_effective"]) / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: connectAccountFullNumberHB);
            parameters.Add(key: "creditCodeHB", value: contractInfo["credit_code"].ToString());
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFee", value: contractInfo["interest_rate_effective_without_account_service_fee"] == null ? "0" : contractInfo["interest_rate_effective_without_account_service_fee"]);

            result = Contracts.RenderContract(contractName, parameters, "ConsumeLoan.pdf", contract);
            return result;
        }

        internal static bool CheckActiveCreditLine(string loanAccountNumber, ulong customerNumber )
        {
            bool check = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT loan_full_number
                                                  FROM [Tbl_credit_lines] 
                                                  WHERE quality = 0 
                                                        AND customer_number = @customerNumber 
                                                        AND connect_account_full_number = @LoanFullNumber and credit_line_type <> 9"
                                                , conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.VarChar, 16).Value = customerNumber;
                cmd.Parameters.Add("@LoanFullNumber", SqlDbType.Float).Value = loanAccountNumber;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        check = true;
                    }
                }
            }

            return check;

        }

        internal static bool CheckCreditLineOrder(string loanAccountNumber)
        {
            bool check = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT n.credit_line_account
                                                FROM Tbl_HB_documents AS d 
                                                LEFT JOIN Tbl_New_Credit_Line_Documents AS n
                                                ON  d.doc_ID=n.Doc_ID
                                                WHERE credit_line_account = @credit_line_account 
                                                AND quality = 3"
                                                , conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@credit_line_account", SqlDbType.Float).Value = loanAccountNumber;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        check = true;
                    }
                }
            }

            return check;

        }

        internal static void UpdateLoanProductOrderContractDate(long orderId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                                                    UPDATE Tbl_new_loan_documents
                                                    SET contract_date = @contract_date
                                                    WHERE doc_id = @doc_id", conn);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;


                cmd.ExecuteScalar();

            }

        }

        internal static byte[] PrintDepositCreditLineContract(long docID, ulong cusotmerNumber, bool fromApprove = false)
        {
            byte[] result;
            LoanProductOrder loanProductOrder = new LoanProductOrder();
            Customer customer = new Customer();
            loanProductOrder = customer.GetCreditLineOrder(docID);
            Card card = Card.GetCardWithOutBallance(loanProductOrder.ProductAccount.AccountNumber);
            CreditLinePrecontractData preContractDate = CreditLinePrecontractData.GetCreditLinePrecontractData(loanProductOrder.StartDate,
                (loanProductOrder.ProductType == 51 || loanProductOrder.ProductType == 50) ? loanProductOrder.ValidationDate.Value : loanProductOrder.EndDate,
                loanProductOrder.InterestRate, 0, card.CardNumber, loanProductOrder.Currency, loanProductOrder.Amount, 8);


            Dictionary<string, string> contractInfo = LoanProductOrder.GetDepositCreditLineContractInfo((int)docID).ToDictionary(x => x.Key, x => x.Value);
            short fillialCode = Customer.GetCustomerFilial(cusotmerNumber).key;
            int cardType = customer.GetCardType(card.CardNumber);
            double kursForLoan = Utility.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.Currency);
            double kursForProvision = Utility.GetCBKursForDate(loanProductOrder.StartDate, loanProductOrder.PledgeCurrency);
            double decreasingAmount = LoanProductOrder.GetCreditLineDecreasingAmount(loanProductOrder.Amount, loanProductOrder.Currency,
                loanProductOrder.StartDate, (loanProductOrder.ProductType == 51 || loanProductOrder.ProductType == 50) ? loanProductOrder.ValidationDate.Value : loanProductOrder.EndDate);
            double penaltyRate = Info.GetPenaltyRateOfLoans(30, loanProductOrder.StartDate);

            string contractName = string.Empty;


            if (cardType != 40 && cardType != 34 && cardType != 50)
            {
                contractName = "CreditLineContract";
            }
            else
            {
                contractName = "CreditLineAmex";
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumberHB", value: cusotmerNumber.ToString());
            Contract contract = null;
            if (fromApprove == true)
            {
                contract = new Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 12;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docID;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }


            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "bln_with_enddate", value: "True");
            parameters.Add(key: "visaNumberHB", value: card.CardNumber);
            parameters.Add(key: "HbDocID", value: docID.ToString());
            parameters.Add(key: "dateOfBeginningHB", value: loanProductOrder.StartDate.ToString("MM/dd/yyyy"));
            parameters.Add(key: "currencyHB", value: loanProductOrder.Currency);
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "startCapitalHB", value: loanProductOrder.Amount.ToString());
            parameters.Add(key: "provisionNumberHB", value: "01");
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: fillialCode.ToString());
            parameters.Add(key: "creditLineTypeHB", value: loanProductOrder.ProductType.ToString());

            parameters.Add(key: "securityCodeHB", value: contractInfo["security_code_2"].ToString());
            parameters.Add(key: "loanProvisionPercentHB", value: ((loanProductOrder.Amount * kursForLoan) / (loanProductOrder.ProvisionAmount * kursForProvision) * 100).ToString());
            parameters.Add(key: "interestRateHB", value: loanProductOrder.InterestRate.ToString());


            parameters.Add(key: "interestRateFullHB", value: (preContractDate.InterestRate / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: loanProductOrder.ProductAccount.AccountNumber.ToString());
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFeeHB", value: preContractDate.InterestRateEffectiveWithoutAccountServiceFee.ToString());
            if (cardType == 40 || cardType == 34 || cardType == 50)
            {
                parameters.Add(key: "contractPersonCountHB", value: "2");

            }

            parameters.Add(key: "repaymentPercentHB", value: contractInfo["repayment_percent"].ToString());
            parameters.Add(key: "RepaymentKurs", value: contractInfo["repayment_kurs"].ToString() == null ? "0" : contractInfo["repayment_kurs"].ToString());
            parameters.Add(key: "decrAmountHB", value: loanProductOrder.ProductType == 50 ? decreasingAmount.ToString() : "0");

            result = Contracts.RenderContract(contractName, parameters, contractName + ".pdf", contract);
            return result;
        }

        internal static byte[] PrintFastOverdraftContract(long docID, ulong customerNumber, bool fromApprove = false)
        {
            byte[] result;

            LoanProductOrder productOrder = new LoanProductOrder();
            Customer customer = new Customer();

            productOrder = customer.GetCreditLineOrder(docID);
            Card card = Card.GetCardWithOutBallance(productOrder.ProductAccount.AccountNumber);
            Dictionary<string, string> contractInfo = LoanProductOrder.GetDepositCreditLineContractInfo((int)docID).ToDictionary(x => x.Key, x => x.Value);

            CreditLinePrecontractData precontractData = CreditLinePrecontractData.GetCreditLinePrecontractData(productOrder.StartDate, productOrder.EndDate, productOrder.InterestRate,
                0, card.CardNumber, productOrder.Currency, productOrder.Amount, 8);
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;
            double penaltyRate = Info.GetPenaltyRateOfLoans(54, productOrder.StartDate);
            string contractName = "CreditLineContract";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());

            Contract contract = null;
            if (fromApprove == true)
            {
                contract = new Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 12;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docID;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }

            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "bln_with_enddate", value: "True");
            parameters.Add(key: "visaNumberHB", value: card.CardNumber);
            parameters.Add(key: "HbDocID", value: docID.ToString());
            parameters.Add(key: "dateOfBeginningHB", value: productOrder.StartDate.ToString("dd/MMM/yy"));


            parameters.Add(key: "currencyHB", value: productOrder.Currency);
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "startCapitalHB", value: productOrder.Amount.ToString());
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: filialCode.ToString());
            parameters.Add(key: "creditLineTypeHB", value: "54");
            parameters.Add(key: "provisionNumberHB", value: "01");
            parameters.Add(key: "interestRateHB", value: productOrder.InterestRate.ToString());
            parameters.Add(key: "securityCodeHB", value: contractInfo["security_code_2"].ToString());

            parameters.Add(key: "loanProvisionPercentHB", value: "0");

            parameters.Add(key: "interestRateFullHB", value: (precontractData.InterestRate / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: productOrder.ProductAccount.AccountNumber);
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFee", value: precontractData.InterestRateEffectiveWithoutAccountServiceFee.ToString());
            parameters.Add(key: "dateOfNormalEndHB", value: productOrder.EndDate.ToString("dd/MMM/yy"));
            parameters.Add(key: "RepaymentKurs", value: contractInfo["repayment_kurs"].ToString() == null ? "0" : contractInfo["repayment_kurs"].ToString());

            result = Contracts.RenderContract(contractName, parameters, "FastOverdraftContract.pdf", contract);

            return result;
        }

        internal static void SaveLoanAcknowledgementText(bool acknowledgedByCheckBox, string acknowledgementText, long id, OrderType orderType)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                string query = @"UPDATE " + (orderType == OrderType.CreditSecureDeposit ? "Tbl_New_Loan_Documents" : "Tbl_New_Credit_Line_Documents")
                    + @" SET acknowledged_by_checkbox = @acknowledgedByCheckBox, acknowledgement_text = @acknowledgementText 
                        WHERE doc_Id = @docId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@acknowledgedByCheckBox", SqlDbType.Bit).Value = acknowledgedByCheckBox;
                    cmd.Parameters.Add("@acknowledgementText", SqlDbType.NVarChar, 500).Value = acknowledgementText ?? "";

                    cmd.ExecuteNonQuery();
                }
            }

        }

        internal static bool IsSecondTime(string loanFullNumber, DateTime startDate)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string query = @"SELECT TOP 1 app_id FROM [Tbl_liability_add]
                                 WHERE loan_full_number = @loan_full_number and date_of_beginning = @date"; 

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@loan_full_number", SqlDbType.NVarChar,20).Value = loanFullNumber;
                    cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = startDate;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            check = true;
                        }
                    }

                }
            }
            return check;
        }
        internal static void UpdateCreditLineProductOrderContractDate(long orderId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                                                    UPDATE Tbl_New_Credit_Line_Documents
                                                    SET contract_date = @contract_date
                                                    WHERE doc_id = @doc_id", conn);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;


                cmd.ExecuteScalar();

            }

        }

        public static ulong GetAmexGoldProductId(string account)
        {
            ulong productID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT C.App_Id 
                                                        FROM Tbl_Visa_Numbers_Accounts V 
	                                                        INNER JOIN Tbl_credit_lines C ON V.loan_account = C.loan_full_number 
                                                        WHERE C.loan_full_number = @account AND V.closing_date IS NULL AND V.card_type IN (20,41)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = account;

                    productID = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return productID;
        }
    }
}

