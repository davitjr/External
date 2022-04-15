using ExternalBanking.ContractServiceRef;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class ConsumeLoanApplicationOrderDB
    {
        internal static ActionResult SaveConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_consume_loan_application_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;



                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@Duration", SqlDbType.Int).Value = order.Duration;

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


        internal static ConsumeLoanApplicationOrder GetConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,n.*,d.operation_date,d.order_group_id,d.confirmation_date,reject_id                                             
		                                           FROM Tbl_HB_documents as d left join Tbl_Consume_Loan_Application_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
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
                order.Duration = Convert.ToInt32(dt.Rows[0]["duration"]);

                order.ProductType = int.Parse(dt.Rows[0]["loan_type"].ToString());

                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.RefuseReasonType = dt.Rows[0]["reject_id"] != DBNull.Value ? Convert.ToByte(dt.Rows[0]["reject_id"]) : default(byte);


            }
            return order;
        }


        internal static (long, DateTime) ExistsConsumeLoanApplicationOrder(ulong customerNumber, List<OrderQuality> qualities)
        {
            string qualitiesCondition = "";
            (long, DateTime) result = (0, DateTime.MinValue);
            if (qualities.Count > 0)
            {
                qualitiesCondition = " and quality in (";


                foreach (OrderQuality quality in qualities)
                {
                    qualitiesCondition += (short)quality + ",";
                }

                qualitiesCondition = qualitiesCondition.Substring(0, qualitiesCondition.Length - 1);
                qualitiesCondition += ") ";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT doc_id, registration_date
		                                           FROM Tbl_HB_documents                                            
                                                   WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end and document_type = 255 " + qualitiesCondition, conn);
                {

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Item1 = Convert.ToInt64(dr["doc_id"].ToString());
                            result.Item2 = Convert.ToDateTime(dr["registration_date"]);
                        }
                    }
                }
            }
            return result;
        }


        internal static ActionResult SaveConsumeLoanSettlementOrder(ConsumeLoanSettlementOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_consume_loan_settlement_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;



                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@Loan_Type", SqlDbType.SmallInt).Value = order.ProductType;
                    cmd.Parameters.Add("@start_date", SqlDbType.DateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.DateTime).Value = order.EndDate;
                    cmd.Parameters.Add("@loan_percent", SqlDbType.Float).Value = order.InterestRate;
                    cmd.Parameters.Add("@monthly_repayment_amount", SqlDbType.Float).Value = order.MonthlyRepaymentAmount;
                    cmd.Parameters.Add("@current_account", SqlDbType.VarChar).Value = order.CurrentAccount.AccountNumber;
                    cmd.Parameters.Add("@repayment_period", SqlDbType.Float).Value = 1;
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.DateTime).Value = order.FirstRepaymentDate;
                    cmd.Parameters.Add("@Duration", SqlDbType.Int).Value = order.Duration;
                    cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.Fees[0].Amount;


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

        internal static ConsumeLoanSettlementOrder GetConsumeLoanSettlementOrder(ConsumeLoanSettlementOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,d.amount_for_payment, n.*,d.operation_date,d.order_group_id,d.confirmation_date,reject_id
		                                           FROM Tbl_HB_documents as d left join Tbl_Consume_Loan_Activation_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
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
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                order.InterestRate = double.Parse(dt.Rows[0]["loan_percent"].ToString());
                order.MonthlyRepaymentAmount = Convert.ToDouble(dt.Rows[0]["monthly_repayment_amount"]);
                order.CurrentAccount = new Account();
                order.CurrentAccount.AccountNumber = dt.Rows[0]["current_account"].ToString();
                order.FirstRepaymentDate = DateTime.Parse(dt.Rows[0]["first_repayment_date"].ToString());
                order.AcknowledgedByCheckBox = dt.Rows[0]["acknowledged_by_checkbox"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["acknowledged_by_checkbox"]) : false;
                order.AcknowledgementText = dt.Rows[0]["acknowledgement_text"] != DBNull.Value ? dt.Rows[0]["acknowledgement_text"].ToString() : "";

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.ProductId = Convert.ToUInt64(dt.Rows[0]["app_id"]);
                order.Duration = Convert.ToInt32(dt.Rows[0]["duration"]);
                order.Fees = new List<OrderFee>();
                order.Fees.Add(new OrderFee());
                order.Fees[0].Amount = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                order.Duration = Convert.ToInt32(dt.Rows[0]["duration"]);
                order.RefuseReasonType = dt.Rows[0]["reject_id"] != DBNull.Value ? Convert.ToByte(dt.Rows[0]["reject_id"]) : default(byte);



            }
            return order;
        }

        public static byte[] PrintConsumeLoanSettlement(long docId, ulong customerNumber, bool fromApprove = false)
        {
            Customer customer = new Customer();
            ConsumeLoanSettlementOrder Order = customer.GetConsumeLoanSettlementOrder(docId);
            Dictionary<string, string> deposticContractInfo = GetDepositLoanContractDetails(docId);

            /*if (deposticContractInfo.Any())
            {
                contractInfo.Add("security_code_2", deposticContractInfo["security_code_2"]);
                contractInfo.Add("interest_rate_effective", deposticContractInfo["interest_rate_effective"]);
                contractInfo.Add("credit_code", deposticContractInfo["credit_code"]);
                contractInfo.Add("interest_rate_effective_without_account_service_fee", deposticContractInfo["interest_rate_effective_without_account_service_fee"]);
            }*/


            Dictionary<string, string> parameters = new Dictionary<string, string>();
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;


            string connectAccountFullNumberHB = LoanProductOrder.GetConnectAccountFullNumber(customerNumber, Order.Currency);
            double penaltyRate = Info.GetPenaltyRateOfLoans(29, Order.StartDate);
            string contractName = "ConsumeLoanContract";

            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());
            parameters.Add(key: "clientTypeHB", value: "");
            parameters.Add(key: "filialCodeHB", value: filialCode.ToString());
            parameters.Add(key: "loanTypeHB", value: Order.ProductType.ToString());
            parameters.Add(key: "securityCodeHB", value: deposticContractInfo["security_code_2"].ToString());
            parameters.Add(key: "dateOfBeginningHB", value: Order.StartDate.ToString("dd/MMM/yy"));
            parameters.Add(key: "currencyHB", value: Order.Currency);
            parameters.Add(key: "startCapitalHB", value: Order.Amount.ToString());
            parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFee", value: deposticContractInfo["interest_rate_effective_without_account_service_fee"] == null ? "0" : deposticContractInfo["interest_rate_effective_without_account_service_fee"]);


            Contract contract = null;
            if (fromApprove == true)
            {
                contract = new Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 7;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<StringKeyValue>();
            }

            parameters.Add(key: "appID", value: "0");
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "dateOfNormalEndHB", value: Order.EndDate.ToString("dd/MMM/yy"));
            parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
            parameters.Add(key: "repaymentAmountHB", value: Order.Fees[0].Amount.ToString());
            parameters.Add(key: "interestRateHB", value: Order.InterestRate.ToString());
            parameters.Add(key: "interestRateFullHB", value: (Convert.ToDouble(deposticContractInfo["interest_rate_effective"]) / 100).ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: connectAccountFullNumberHB);
            parameters.Add(key: "creditCodeHB", value: deposticContractInfo["credit_code"].ToString());
            if (Info.IsACBAGroupEmployee(customerNumber))
            {
                parameters.Add(key: "changedRate", value: "1");
                parameters.Add(key: "employeeWorkBeginning", value: Info.DateOfACBAGroupEmployee(customerNumber).ToString());
                parameters.Add(key: "firstRepaymentDate", value: Order.FirstRepaymentDate.ToString("dd/MMM/yy"));
            }


            return Contracts.RenderContract(contractName, parameters, "ConsumeLoan.pdf", contract);
        }



        internal static Dictionary<string, string> GetDepositLoanContractDetails(long docid)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBASEConn"].ToString()))
            {
                string sql = @"SELECT l.security_code_2,
                                      p.interest_rate_effective,
                                      l.credit_code,
                                      p.interest_rate_effective_without_account_service_fee 
								FROM  [dbo].Tbl_Consume_Loan_Activation_Order_Details L 
                                INNER JOIN [dbo].[Tbl_Consume_Loan_precontract_data] P ON L.doc_id = P.doc_id 
                                WHERE L.doc_id = @docid ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docid", SqlDbType.Int).Value = docid;

                    conn.Open();


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Add("security_code_2", dr["security_code_2"].ToString());
                            result.Add("interest_rate_effective", dr["interest_rate_effective"].ToString());
                            result.Add("credit_code", dr["credit_code"].ToString());
                            result.Add("interest_rate_effective_without_account_service_fee", dr["interest_rate_effective_without_account_service_fee"].ToString());
                        }
                    }
                }
            }

            return result;
        }

        internal static void SaveLoanAcknowledgementText(bool acknowledgedByCheckBox, string acknowledgementText, long id)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                string query = @"UPDATE tbl_consume_loan_activation_order_details
                                    SET acknowledged_by_checkbox = @acknowledgedByCheckBox, acknowledgement_text = @acknowledgementText 
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

        internal static Dictionary<string, string> GetConsumeLoanSettlementSchedult(long docId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBASEConn"].ToString()))
            {
                string sql = @"SELECT start_date, end_date, amount, loan_percent, first_repayment_date, credit_code FROM Tbl_Consume_Loan_Activation_Order_Details
												   WHERE Doc_ID=@docid ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docid", SqlDbType.Int).Value = docId;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Add("dateOfBeginning", dr["start_date"].ToString());
                            result.Add("dateOfNormalEnd", dr["end_date"].ToString());
                            result.Add("startCapital", dr["amount"].ToString());
                            result.Add("interestRate", dr["loan_percent"].ToString());
                            result.Add("firstRepaymentDate", dr["first_repayment_date"].ToString());
                            result.Add("creditCode", dr["credit_code"].ToString());
                        }
                    }
                }
            }

            return result;
        }


        internal static bool CheckConsumeLoanApplicationAppId(long docId)
        {
            string query = @"SELECT 1 FROM [dbo].[Tbl_HB_Products_Identity] P 
												   INNER JOIN [Tbl_Short_time_loans;] L ON P.app_id = L.app_id
												   where p.hb_doc_id = @doc_id";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                    return cmd.ExecuteReader().Read();
                }
            }
        }

        internal static void UpdateConsumeLoanSettlementScheduleContractDate(long orderId)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_Consume_Loan_Activation_Order_Details
                                                    SET contract_date = @contract_date
                                                    WHERE doc_id = @doc_id", conn);
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;


                    cmd.ExecuteScalar();
                }
            }
        }


        internal static (long, DateTime) ExistsConsumeLoanSettlementOrder(ulong customerNumber, List<OrderQuality> qualities, long orderId, ulong productId)
        {
            string qualitiesCondition = "";
            (long, DateTime) result = (0, DateTime.MinValue);
            if (qualities.Count > 0)
            {
                qualitiesCondition = " and quality in (";


                foreach (OrderQuality quality in qualities)
                {
                    qualitiesCondition += (short)quality + ",";
                }

                qualitiesCondition = qualitiesCondition.Substring(0, qualitiesCondition.Length - 1);
                qualitiesCondition += ") ";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT d.doc_id, registration_date
		                                           FROM Tbl_HB_documents d inner join  Tbl_Consume_Loan_Activation_Order_Details C on d.doc_id = C.doc_id                                           
                                                   WHERE customer_number=case when @customer_number = 0 then customer_number 
                                                  else @customer_number end and document_type = 258 and 
                                                    not (quality = 1 and d.doc_id = @doc_id) and C.app_id = @app_id" + qualitiesCondition, conn);
                {

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Item1 = Convert.ToInt64(dr["doc_id"].ToString());
                            result.Item2 = Convert.ToDateTime(dr["registration_date"]);
                        }
                    }
                }
            }
            return result;
        }

        internal static bool ExistsRefusedConsumeLoanApplication(ulong productId)
        {
            string query = @"select 1 from Tbl_loan_applications where app_id=@app_id and status not in (2,3)";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    return cmd.ExecuteReader().Read();
                }
            }
        }

        internal static ulong GetConsumeLoanApplicationAppId(long docId)
        {
            string query = @"SELECT app_id FROM [dbo].[Tbl_HB_Products_Identity]
							 WHERE hb_doc_id = @doc_id";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                        return Convert.ToUInt64(dr["app_id"]);
                    else
                        return 0;

                }
            }
        }


        internal static void ValidateSetConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_validate_set_loan_application";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = order.user.userID;
                    cmd.Parameters.Add("@operationType", SqlDbType.TinyInt).Value = 2;
                    cmd.Parameters.Add("@refuseDate", SqlDbType.SmallDateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@refuseReason", SqlDbType.NVarChar, 250).Value = string.Empty;
                    cmd.Parameters.Add("@wrongApp", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@analyseResult", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@analyseRejectionReason", SqlDbType.NVarChar, 250).Value = DBNull.Value;
                    cmd.Parameters.Add("@refuseReasonCause", SqlDbType.Int).Value = 5; //Վարկն այլևս անհրաժեշտ չէ
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@analyseRejectionCause", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@onlyValidate", SqlDbType.Bit).Value = 0;


                    cmd.ExecuteNonQuery();
                }

            }
        }

    }
}
