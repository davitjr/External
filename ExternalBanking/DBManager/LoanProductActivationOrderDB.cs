using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using ExternalBanking.ACBAServiceReference;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    class LoanProductActivationOrderDB
    {

        internal static ActionResult Save(LoanProductActivationOrder order, string userName, SourceType source, string cardNumber = null)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_addNewLoanProductActivationDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@product_app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    if (order.Type == OrderType.PaidFactoringActivation)
                    {
                        cmd.Parameters.Add("@feeAccount", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                        cmd.Parameters.Add("@factoringCustomerAccount", SqlDbType.Float).Value = order.FactoringCustomerAccount;
                    }

                    if (order.Source == SourceType.EContract && !string.IsNullOrEmpty(order.Currency))
                    {
                        cmd.Parameters.Add("@loanAmount", SqlDbType.Float).Value = order.Amount;
                        cmd.Parameters.Add("@loanCurrency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    }
                    if (cardNumber != null)
                        cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = cardNumber;
                    else
                        cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = DBNull.Value;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }

            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static LoanProductActivationOrder Get(LoanProductActivationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT registration_date,document_number,customer_number,document_type, description, document_subtype,quality,I.App_ID,D.operation_date                                             
		                                           FROM Tbl_HB_documents D INNER JOIN Tbl_HB_Products_Identity  I ON D.doc_ID=I.HB_Doc_ID
                                                   
                                                   WHERE D.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.ProductId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Description = dt.Rows[0]["description"].ToString();
                order.Fees = new List<OrderFee>();
                order.Fees = Order.GetOrderFees(order.Id);
                if (order.Fees.Count != 0)
                {
                    foreach (var fee in order.Fees)
                    {
                        if (fee.Type == 4 || fee.Type == 21 || fee.Type == 26 || fee.Type == 27)
                        {
                            order.FeeAccount = fee.Account;
                            order.FeeAmount = fee.Amount;
                        }
                        else if (fee.Type == 22)
                        {
                            order.FeeAmountWithTax = fee.Amount;
                            order.FeeAccountWithTax = fee.Account;
                        }
                    }
                }

                if (order.FeeAccount != null)
                    order.FeeAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccount.AccountTypeDescription);
                if (order.FeeAccountWithTax != null)
                    order.FeeAccountWithTax.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccountWithTax.AccountTypeDescription);


                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

            }
            return order;
        }

        internal static bool IsSecondActivation(LoanProductActivationOrder order)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select doc_id from Tbl_HB_documents D INNER JOIN Tbl_HB_Products_Identity  I ON D.doc_ID=I.HB_Doc_ID
                                                WHERE quality in (1,2,3,5,30)  and customer_number=@customerNumber and I.App_ID=@productId
                                                AND document_type in(73,74,141,152)", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductId;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }

        internal static double GetLoanProductActivationFee(ulong productId, short withTax)
        {
            double feeAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(dbo.fnc_get_loan_commission_amount(@productId,@withTax,0),0) repayment_amount", conn);

                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@withTax", SqlDbType.Float).Value = withTax;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    feeAmount = Convert.ToDouble(dr["repayment_amount"].ToString());
                }
            }
            return feeAmount;
        }

        internal static bool CheckLoanDocumentAttachment(int loanType, long productId, int sourceType, double amount, ulong customerNumber)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT dbo.fn_check_consume_loan_document_attachment(@loanType,@sourceType,@amountInAmd,@customerNumber,@productId) as result", conn);

                cmd.Parameters.Add("@loanType", SqlDbType.Int).Value = loanType;
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = sourceType;
                cmd.Parameters.Add("@amountInAmd", SqlDbType.Money).Value = amount;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = Convert.ToBoolean(short.Parse(dr["result"].ToString()));
                }
            }
            return check;
        }

        internal static List<ActionError> LoanActivationValidation(Loan loan, LoanProductActivationOrder order)
        {
            List<ActionError> errors = new List<ActionError>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {


                using (SqlCommand cmd = new SqlCommand(@"SELECT top 1 [last_day_of rate calculation] day_of_rate_calculation FROM [Tbl_closed_short_loans] WHERE customer_number=@customerNumber  ORDER BY [last_day_of rate calculation] DESC ", conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        DateTime lastDay = Convert.ToDateTime(dr["day_of_rate_calculation"].ToString());
                        DateTime day = loan.StartDate.AddDays(-1);
                        int day_count = 0;
                        while (day_count < 1 && day > lastDay)
                        {
                            if (Utility.IsWorkingDay(day))
                            {
                                day_count = day_count + 1;
                            }
                            day = day.AddDays(-1);
                        }

                        if (day_count == 0)
                        {
                            errors.Add(new ActionError(755));
                        }
                    }
                }
                conn.Close();

                //using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_insurance_contracts WHERE quality =10  AND product_app_id =@appID", conn))
                //{
                //    conn.Open();
                //    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = loan.ProductId;
                //    SqlDataReader dr = cmd.ExecuteReader();
                //    if (dr.Read())
                //    {
                //        errors.Add(new ActionError(759));
                //    }
                //}
                //conn.Close();

                //using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_insurance_contracts ins INNER JOIN Tbl_paid_factoring p ON ins.app_id = p.main_app_id  WHERE ins.product_app_id =@appID", conn))
                //{
                //    conn.Open();
                //    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = loan.ProductId;
                //    SqlDataReader dr = cmd.ExecuteReader();
                //    if (dr.Read())
                //    {
                //        errors.Add(new ActionError(760));
                //    }
                //}

            }

            List<ulong> customernumbers = GetOwnerCustomerNumbers(loan.ProductId, "provision_owners");
            if (customernumbers.Count != 0)
            {
                foreach (var customerNumber in customernumbers)
                {
                    errors.AddRange(Validation.ValidateCustomerSignature(customerNumber));
                }
            }
            if (loan.LoanType == 13)
            {
                customernumbers = GetOwnerCustomerNumbers(loan.ProductId, "students");
                if (customernumbers.Count != 0)
                {
                    foreach (var customerNumber in customernumbers)
                    {
                        errors.AddRange(Validation.ValidateCustomerSignature(customerNumber));
                    }
                }
            }


            return errors;

        }

        internal static List<string> GetLoanActivationWarnings(long productId, ulong customerNumber, short productType)
        {
            Loan loan = null;
            CreditLine crLine = null;
            List<string> warnings = new List<string>();
            if (productType == 1)
            {
                loan = Loan.GetLoan((ulong)productId, customerNumber);
            }
            else if (productType == 2)
            {
                crLine = CreditLine.GetCreditLine((ulong)productId, customerNumber);
            }


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                bool check = false;
                SqlCommand cmd = new SqlCommand(@"SELECT pr.IdPro FROM Tbl_provision_of_clients pr INNER JOIN  Tbl_Link_application_Provision link on pr.IdPro =  link.IdPro 
                                                WHERE Type in(5,6) AND matured_date is null AND pr.idpro not in (SELECT idpro FROM Tbl_Link_application_Provision
                                                WHERE activated_date is not null AND app_id <>@appId) AND app_id = @appId", conn);
                cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                if (cmd.ExecuteReader().Read())
                {
                    check = true;
                }
                conn.Close();

                if (check)
                {
                    if (productType == 1)
                    {
                        if (loan?.Fond != 22 && loan?.Fond != 30)
                        {
                            conn.Open();
                            cmd = new SqlCommand(@"SELECT loan_full_number FROM Tbl_repayments_add WHERE repayment_type = 4 AND app_id = @appId", conn);
                            cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                            if (!cmd.ExecuteReader().Read())
                            {
                                warnings.Add("Տրանսպորտային միջոցի/գյուղ. տեխնիկայի գծով ծախսը մուտքագրված չէ:");
                            }
                        }
                    }
                    if (productType == 2)
                    {
                        if (crLine?.Fond != 22)
                        {
                            conn.Open();
                            cmd = new SqlCommand(@"SELECT loan_full_number FROM Tbl_repayments_add WHERE repayment_type = 4 AND app_id = @appId", conn);
                            cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                            if (!cmd.ExecuteReader().Read())
                            {
                                warnings.Add("Տրանսպորտային միջոցի/գյուղ. տեխնիկայի գծով ծախսը մուտքագրված չէ:");
                            }
                        }
                    }

                }
            }
            if (productType == 1)
            {
                if (loan?.LoanType == 7 && loan?.InterestRate == 0)
                {
                    if (((loan?.StartDate.Year - loan?.EndDate.Year) * 12) + loan?.StartDate.Month - loan?.EndDate.Month > 8)
                    {
                        warnings.Add("Վարկի ժամկետը 8 ամսից ավել է");
                    }
                }
                if (loan?.LoanType == 38)
                {
                    double profitAmount = 0;
                    profitAmount = GetAparikShopFee(productId);
                    if (profitAmount == 0)
                    {
                        warnings.Add("Խանութի միջնորդավճարը 0 է նշված");
                    }
                }

            }


            return warnings;
        }

        internal static List<ulong> GetOwnerCustomerNumbers(long productId, string forCheck)
        {
            List<ulong> customerNumbers = new List<ulong>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqlString = "";

                if (forCheck == "provision_owners")
                {
                    sqlString = @"SELECT customer_number FROM Tbl_provision_owners PO INNER JOIN Tbl_Link_application_Provision LP on PO.IdPro=LP.IdPro
                                    WHERE PO.owner_type <> 4 AND app_id =@appId
                                    GROUP BY customer_number";
                }
                if (forCheck == "students")
                {
                    sqlString = @"SELECT customer_number FROM Tbl_loan_joint_students WHERE loan_app_id=@appId GROUP BY customer_number";
                }

                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    customerNumbers.Add(ulong.Parse(dr["customer_number"].ToString()));
                }

            }
            return customerNumbers;

        }

        internal static double GetAparikShopFee(long productId)
        {
            double profitAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT A.[Percent], P.Keeper_Change, Cust.type_of_client, Cust.residence as Sitizen, A.fee_type, A.amount_for_1,A.Contract_Fillial+22000 as filialcode,require_billing,P.start_capital
                 FROM Tbl_paid_factoring P INNER JOIN Tbl_Contract_Aparik A
                 ON P.keeper_change = A.New_ID
                 INNER JOIN Tbl_customers Cust ON  P.Customer_Number = Cust.Customer_Number
                 Inner join Tbl_aparik_shops On P.shop_id=Tbl_aparik_shops.shop_id
                 WHERE P.App_Id =@appId ", conn);

                cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                SqlDataReader dr = cmd.ExecuteReader();

                double percent = 0;
                short feeType = 0;
                double minAmount = 0;

                double startCapital = 0;
                double keeperChange = 0;

                if (dr.Read())
                {
                    percent = Convert.ToDouble(dr["percent"].ToString());
                    feeType = Convert.ToInt16(dr["fee_type"].ToString());
                    minAmount = Convert.ToDouble(dr["amount_for_1"].ToString());
                    startCapital = Convert.ToDouble(dr["start_capital"].ToString());
                    keeperChange = Convert.ToDouble(dr["Keeper_Change"].ToString());
                }
                conn.Close();
                if (feeType == 1)
                {
                    profitAmount = Math.Round(startCapital * percent, 1);
                }
                else
                {
                    conn.Open();
                    cmd = new SqlCommand("Sp_get_loan_fee_amount", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@loan_amount", SqlDbType.Float).Value = startCapital;
                    cmd.Parameters.Add("@Shop_ID", SqlDbType.Float).Value = keeperChange;

                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        profitAmount = Convert.ToDouble(dr["fee_amount"].ToString());
                    }

                }


            }

            return profitAmount;
        }



        internal static bool CheckLoanProductActivationStatus(ulong productId)
        {
            bool status = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT dbo.Fn_is_online_formulation_checked(@productId) as result", conn);

                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    status = ushort.Parse(dr["result"].ToString()) == 1 ? false : true;
                }
            }
            return status;

        }

        internal static bool IsTransportExpensePaid(long productId)
        {
            bool check = false;
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT customer_number FROM Tbl_repayments_add WHERE repayment_type = 4 AND app_id =@productId", conn);
            cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
            using SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                check = true;
            }
            return check;
        }

        internal static double GetLoanTotalInsuranceAmount(ulong productId)
        {

            double totalInsuranceAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT  ISNULL(SUM(P.start_capital),0)
                                                  FROM Tbl_paid_factoring P INNER JOIN Tbl_insurance_contracts INS ON P.main_app_id = INS.app_id
                                                  OUTER APPLY
                                                  (SELECT family_with_children FROM tbl_loan_applications_add_inf LA WHERE LA.app_id = INS.product_app_id) LAA 
                                                  WHERE P.quality =10 AND INS.product_app_id= @productId AND INS.insurance_type <> CASE WHEN LAA.family_with_children = 3 THEN 7 ELSE INS.insurance_type - 1 END", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                totalInsuranceAmount = Convert.ToDouble(cmd.ExecuteScalar());
            }
            return totalInsuranceAmount;

        }
        /// <summary>
        /// Վարկերի ակտիվացման հայտերի ավտոմատ ստեղծում նախնական հայտերի հիման վրա
        /// </summary>
        /// <param name="preOrderId"></param>
        /// <returns></returns>
        internal static async Task<List<LoanProductActivationOrder>> GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(long preOrderId)
        {
            List<LoanProductActivationOrder> loanProductActivationOrders = new List<LoanProductActivationOrder>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string script = "";
                script = @"SELECT customer_number,app_ID,quality FROM Tbl_Automatic_HB_Documents_Generation_PreOrder_Details WHERE preOrder_ID=@preOrder_Id";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(script, conn);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@preOrder_Id", SqlDbType.BigInt).Value = preOrderId;

                using DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    LoanProductActivationOrder product = SetLoanProductActivationOrder(row);
                    loanProductActivationOrders.Add(product);
                }

            }



            return loanProductActivationOrders;
        }

        /// <summary>
        /// Վարկի ակտիվացման ավտոմատ ստեղծված հայտի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static LoanProductActivationOrder SetLoanProductActivationOrder(DataRow row)
        {
            LoanProductActivationOrder product = new LoanProductActivationOrder();
            if (row != null)
            {
                product.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                product.ProductId = Convert.ToUInt64(row["app_id"].ToString());
                product.Type = OrderType.LoanActivation;
                product.Quality = (OrderQuality)Convert.ToUInt16(row["quality"].ToString());
            }
            return product;
        }

        internal static bool SignedOutOfBankCheck(ulong AppId)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_loan_applications LA
                                                  INNER JOIN Tbl_Link_application_Provision AP ON AP.app_id = LA.app_id
                                                  INNER JOIN Tbl_provision_of_clients P ON P.idpro = AP.IdPro
                                                  WHERE P.signed_out_of_bank = 1 AND ISNULL(P.signing_out_set_number, 0) = 0 AND LA.app_id = @productApp_ID", conn);
            cmd.Parameters.Add("@productApp_ID", SqlDbType.Float).Value = AppId;
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read();
        }

    }
}
