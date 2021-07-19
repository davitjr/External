using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class FastTransferPaymentOrderDB
    {

        internal static ActionResult Save(FastTransferPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_transfer_fast";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 100).Value = order.Sender;
                    cmd.Parameters.Add("@sender_address", SqlDbType.NVarChar, 100).Value = order.SenderAddress;
                    cmd.Parameters.Add("@sender_passport", SqlDbType.NVarChar, 50).Value = order.SenderPassport;
                    cmd.Parameters.Add("@sender_date_of_birth", SqlDbType.SmallDateTime).Value = order.SenderDateOfBirth;
                    cmd.Parameters.Add("@sender_email", SqlDbType.NVarChar, 50).Value = order.SenderEmail;
                    cmd.Parameters.Add("@sender_phone", SqlDbType.NVarChar, 50).Value = order.SenderPhone;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = order.Code;

                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 50).Value = order.ReceiverPassport;

                    cmd.Parameters.Add("@quality", SqlDbType.SmallInt).Value = order.Quality;

                    cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 50).Value = order.Country;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 255).Value = order.Receiver;
                    if (order.ReceiverAddInf != "" && order.ReceiverAddInf != null)
                    {
                        cmd.Parameters.Add("@receiver_add_inf", SqlDbType.NVarChar, 200).Value = order.ReceiverAddInf;
                    }

                    cmd.Parameters.Add("@descr_for_payment", SqlDbType.NVarChar, 150).Value = order.DescriptionForPayment;
                    cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;

                    cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.NVarChar, 50).Value = order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = order.FeeAccount.Currency;
                    cmd.Parameters.Add("@fee_type", SqlDbType.TinyInt).Value = 5;
                    if (order.CardFee != 0 && order.Type == OrderType.FastTransferFromCustomerAccount)
                    {
                        cmd.Parameters.Add("@card_fee_amount", SqlDbType.Float).Value = order.CardFee;
                        cmd.Parameters.Add("@card_fee_currency", SqlDbType.NVarChar, 20).Value = order.DebitAccount.Currency;
                        cmd.Parameters.Add("@card_fee_type", SqlDbType.TinyInt).Value = 7;
                    }
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = order.Description;
                    cmd.Parameters.Add("@fee", SqlDbType.NVarChar, 400).Value = order.Fee;
                    cmd.Parameters.Add("@fee_ACBA", SqlDbType.NVarChar, 400).Value = order.FeeAcba;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = (short)order.SubType;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    //ARUS
                    if (order.SubType == 23)
                    {
                        cmd.Parameters.Add("@MTO_agent_code", SqlDbType.NVarChar).Value = order.MTOAgentCode;
                        cmd.Parameters.Add("@payout_delivery_code", SqlDbType.NVarChar).Value = order.PayoutDeliveryCode;
                        cmd.Parameters.Add("@beneficiary_agent_code", SqlDbType.NVarChar).Value = order.BeneficiaryAgentCode;
                        cmd.Parameters.Add("@beneficiary_country_code", SqlDbType.NVarChar).Value = order.Country;
                        cmd.Parameters.Add("@beneficiary_state_code", SqlDbType.NVarChar).Value = order.BeneficiaryStateCode;
                        cmd.Parameters.Add("@beneficiary_city_code", SqlDbType.NVarChar).Value = order.BeneficiaryCityCode;
                        cmd.Parameters.Add("@payout_agent_code", SqlDbType.NVarChar).Value = order.PayOutAgentCode;
                        cmd.Parameters.Add("@sender_last_name", SqlDbType.NVarChar).Value = order.NATSenderLastName;
                        cmd.Parameters.Add("@sender_middle_name", SqlDbType.NVarChar).Value = order.NATSenderMiddleName;
                        cmd.Parameters.Add("@sender_first_name", SqlDbType.NVarChar).Value = order.NATSenderFirstName;
                        cmd.Parameters.Add("@eng_sender_last_name", SqlDbType.NVarChar).Value = order.SenderLastName;
                        cmd.Parameters.Add("@eng_sender_middle_name", SqlDbType.NVarChar).Value = order.SenderMiddleName;
                        cmd.Parameters.Add("@eng_sender_first_name", SqlDbType.NVarChar).Value = order.SenderFirstName;
                        cmd.Parameters.Add("@sender_country_code", SqlDbType.NVarChar).Value = order.SenderCountryCode;
                        cmd.Parameters.Add("@sender_state_code", SqlDbType.NVarChar).Value = order.SenderStateCode;
                        cmd.Parameters.Add("@sender_city_code", SqlDbType.NVarChar).Value = order.SenderCityCode;
                        cmd.Parameters.Add("@sender_zip_code", SqlDbType.NVarChar).Value = order.SenderZipCode;
                        cmd.Parameters.Add("@sender_occupation_name", SqlDbType.NVarChar).Value = order.SenderOccupationName;
                        cmd.Parameters.Add("@sender_document_type_code", SqlDbType.NVarChar).Value = order.SenderDocumentTypeCode;
                        cmd.Parameters.Add("@sender_issue_date", SqlDbType.NVarChar).Value = order.SenderIssueDate;
                        cmd.Parameters.Add("@sender_expiration_date", SqlDbType.NVarChar).Value = order.SenderExpirationDate;
                        cmd.Parameters.Add("@sender_issue_country_code", SqlDbType.NVarChar).Value = order.SenderIssueCountryCode;
                        cmd.Parameters.Add("@sender_issue_city_code", SqlDbType.NVarChar).Value = order.SenderIssueCityCode;
                        cmd.Parameters.Add("@sender_issue_id_no", SqlDbType.NVarChar).Value = order.SenderIssueIDNo;
                        cmd.Parameters.Add("@sender_birth_place_name", SqlDbType.NVarChar).Value = order.SenderBirthPlaceName;
                        cmd.Parameters.Add("@sender_sex_code", SqlDbType.NVarChar).Value = order.SenderSexCode;
                        cmd.Parameters.Add("@sender_mobile_no", SqlDbType.NVarChar).Value = order.SenderMobileNo;
                        cmd.Parameters.Add("@sender_fiscal_code", SqlDbType.NVarChar).Value = order.SenderFiscalCode;
                        cmd.Parameters.Add("@sender_residency_code", SqlDbType.NVarChar).Value = order.SenderResidencyCode;
                        cmd.Parameters.Add("@beneficiary_last_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryLastName;
                        cmd.Parameters.Add("@beneficiary_middle_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryMiddleName;
                        cmd.Parameters.Add("@beneficiary_first_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryFirstName;
                        cmd.Parameters.Add("@eng_beneficiary_last_name", SqlDbType.NVarChar).Value = order.BeneficiaryLastName;
                        cmd.Parameters.Add("@eng_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.BeneficiaryMiddleName;
                        cmd.Parameters.Add("@eng_beneficiary_first_name", SqlDbType.NVarChar).Value = order.BeneficiaryFirstName;
                        cmd.Parameters.Add("@beneficiary_address", SqlDbType.NVarChar).Value = order.BeneficiaryAddress;
                        cmd.Parameters.Add("@beneficiary_email_name", SqlDbType.NVarChar).Value = order.BeneficiaryEMailName;
                        cmd.Parameters.Add("@beneficiary_phone_no", SqlDbType.NVarChar).Value = order.BeneficiaryPhoneNo;
                        cmd.Parameters.Add("@beneficiary_mobile_no", SqlDbType.NVarChar).Value = order.BeneficiaryMobileNo;
                        cmd.Parameters.Add("@control_question_name", SqlDbType.NVarChar).Value = order.ControlQuestionName;
                        cmd.Parameters.Add("@control_answer_name", SqlDbType.NVarChar).Value = order.ControlAnswerName;
                        cmd.Parameters.Add("@purpose_remittance_code", SqlDbType.NVarChar).Value = order.PurposeRemittanceCode;
                        cmd.Parameters.Add("@promotion_code", SqlDbType.NVarChar).Value = order.PromotionCode;
                        cmd.Parameters.Add("@destination_text", SqlDbType.NVarChar).Value = order.DestinationText;
                        cmd.Parameters.Add("@settlement_exchange_rate", SqlDbType.Float).Value = order.SettlementExchangeRate;

                    }

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = order.Id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return result;
                }
            }
        }

        public static double GetFastTransferFeeAcbaPercent(byte transferType)
        {

            double result;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select Profit_Percent FROM Tbl_TransferSystems WHERE code =@transfer_type ";
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = transferType;
                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }

            return result;

        }


        public static byte GetFastTransferCodeLength(byte transferType, byte type)
        {

            byte result;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select case when @type=1 then isnull(code_length_min,0) else isnull(code_length_max,0) end  length FROM Tbl_TransferSystems WHERE code =@transfer_type ";
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = transferType;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = type;
                    result = byte.Parse(cmd.ExecuteScalar().ToString());
                }
            }

            return result;

        }




        internal static FastTransferPaymentOrder Get(FastTransferPaymentOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT D.*, C.CountryName CountryName,    type_of_client, transferSystem,
F.ARUS_success,
														F.MTO_agent_code,
														F.payout_delivery_code,
														F.beneficiary_agent_code,
														F.beneficiary_country_code,
														F.beneficiary_state_code,
														F.beneficiary_city_code,
														F.payout_agent_code,
														F.sender_last_name,
														F.sender_middle_name,
														F.sender_first_name,
														F.eng_sender_last_name,
														F.eng_sender_middle_name,
														F.eng_sender_first_name,
														F.sender_country_code,
														F.sender_state_code,
														F.sender_city_code,
														F.sender_zip_code,
														F.sender_occupation_name,
														F.sender_document_type_code,
														F.sender_issue_date,
														F.sender_expiration_date,
														F.sender_issue_country_code,
														F.sender_issue_city_code,
														F.sender_issue_id_no,
														F.sender_birth_place_name,
														F.sender_sex_code,
														F.sender_mobile_no,
														F.sender_fiscal_code,
														F.sender_residency_code,
														F.beneficiary_last_name,
														F.beneficiary_middle_name,
														F.beneficiary_first_name,
														F.eng_beneficiary_last_name,
														F.eng_beneficiary_middle_name,
														F.eng_beneficiary_first_name,
														F.beneficiary_address,
														F.beneficiary_email_name,
														F.beneficiary_phone_no,
														F.beneficiary_mobile_no,
														F.control_question_name,
														F.control_answer_name,
														F.purpose_remittance_code,
														F.promotion_code,
														F.destination_text,
                                                        F.ARUS_Message,
                                                        F.settlement_exchange_rate
                                                         FROM Tbl_Hb_Documents D  
			                                                        LEFT JOIN Tbl_Countries  C  on D.country=C.CountryCodeN
			                                                        LEFT JOIN Tbl_Customers  Cust  on D.customer_number=Cust.customer_number
                                                                    LEFT JOIN Tbl_TransferSystems  T  on D.document_subtype=T.code
                                                                    LEFT JOIN Tbl_Fast_Transfer_Order_Details F ON D.doc_id = F.doc_id
                                                         WHERE D.doc_id=@id and d.customer_number=case when @customerNumber = 0 then d.customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            order.SubType = Convert.ToByte((dr["document_subtype"]));

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                ulong debitAccount = Convert.ToUInt64(dr["debet_account"]);
                                order.DebitAccount = Account.GetAccount(debitAccount);
                            }


                            if (dr["receiver_name"] != DBNull.Value)
                                order.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());


                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();

                            order.OrderNumber = dr["document_number"].ToString();

                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            if (dr["sender_name"] != DBNull.Value)
                                order.Sender = Utility.ConvertAnsiToUnicode(dr["sender_name"].ToString());

                            if (dr["sender_address"] != DBNull.Value)
                                order.SenderAddress = Utility.ConvertAnsiToUnicode(dr["sender_address"].ToString());


                            if (dr["sender_passport"] != DBNull.Value)
                                order.SenderPassport = Utility.ConvertAnsiToUnicode(dr["sender_passport"].ToString());

                            if (dr["sender_date_of_birth"] != DBNull.Value)
                                order.SenderDateOfBirth = Convert.ToDateTime(dr["sender_date_of_birth"]);

                            if (dr["sender_email"] != DBNull.Value)
                                order.SenderEmail = Utility.ConvertAnsiToUnicode(dr["sender_email"].ToString());


                            if (dr["sender_phone"] != DBNull.Value)
                                order.SenderPhone = Utility.ConvertAnsiToUnicode(dr["sender_phone"].ToString());

                            if (dr["fast_transfer_code"] != DBNull.Value)
                                order.Code = Utility.ConvertAnsiToUnicode(dr["fast_transfer_code"].ToString());

                            if (dr["receiver_add_inf"] != DBNull.Value)
                                order.ReceiverAddInf = Utility.ConvertAnsiToUnicode(dr["receiver_add_inf"].ToString());

                            if (dr["descr_for_payment"] != DBNull.Value)
                                order.DescriptionForPayment = Utility.ConvertAnsiToUnicode(dr["descr_for_payment"].ToString());

                            if (dr["transferSystem"] != DBNull.Value)
                                order.TransferSystemDescription = Utility.ConvertAnsiToUnicode(dr["transferSystem"].ToString());

                            if (dr["receiver_passport"] != DBNull.Value)
                                order.ReceiverPassport = Utility.ConvertAnsiToUnicode(dr["receiver_passport"].ToString());

                            if (dr["Country"] != DBNull.Value)
                                order.Country = dr["Country"].ToString();

                            if (dr["CountryName"] != DBNull.Value)
                                order.CountryName = dr["CountryName"].ToString();

                            if (dr["type_of_client"] != DBNull.Value)
                                order.SenderType = Convert.ToInt16(dr["type_of_client"]);

                            if (dr["fee_in_currency"] != DBNull.Value)
                                order.Fee = Convert.ToDouble(dr["fee_in_currency"]);

                            if (dr["fee_Acba"] != DBNull.Value)
                                order.FeeAcba = Convert.ToDouble(dr["fee_Acba"]);

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                            order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());

                            //ARUS
                            if (order.SubType == 23)
                            {
                                order.ARUSSuccess = Convert.ToInt16(dr["ARUS_success"].ToString());

                                order.MTOAgentCode = dr["MTO_agent_code"].ToString();

                                order.PayoutDeliveryCode = dr["payout_delivery_code"].ToString();

                                if (dr["beneficiary_agent_code"] != DBNull.Value)
                                    order.BeneficiaryAgentCode = dr["beneficiary_agent_code"].ToString();

                                if (dr["beneficiary_state_code"] != DBNull.Value)
                                    order.BeneficiaryStateCode = dr["beneficiary_state_code"].ToString();

                                if (dr["beneficiary_city_code"] != DBNull.Value)
                                    order.BeneficiaryCityCode = dr["beneficiary_city_code"].ToString();

                                if (dr["payout_agent_code"] != DBNull.Value)
                                    order.PayOutAgentCode = dr["payout_agent_code"].ToString();

                                order.NATSenderLastName = dr["sender_last_name"].ToString();
                                if (dr["sender_middle_name"] != DBNull.Value)
                                    order.NATSenderMiddleName = dr["sender_middle_name"].ToString();
                                order.NATSenderFirstName = dr["sender_first_name"].ToString();

                                order.SenderLastName = dr["eng_sender_last_name"].ToString();

                                if (dr["eng_sender_middle_name"] != DBNull.Value)
                                    order.SenderMiddleName = dr["eng_sender_middle_name"].ToString();

                                order.SenderFirstName = dr["eng_sender_first_name"].ToString();
                                order.SenderCountryCode = dr["sender_country_code"].ToString();

                                if (dr["sender_state_code"] != DBNull.Value)
                                    order.SenderStateCode = dr["sender_state_code"].ToString();

                                if (dr["sender_city_code"] != DBNull.Value)
                                    order.SenderCityCode = dr["sender_city_code"].ToString();

                                if (dr["sender_zip_code"] != DBNull.Value)
                                    order.SenderZipCode = dr["sender_zip_code"].ToString();

                                if (dr["sender_occupation_name"] != DBNull.Value)
                                    order.SenderOccupationName = dr["sender_occupation_name"].ToString();

                                order.SenderDocumentTypeCode = dr["sender_document_type_code"].ToString();
                                order.SenderIssueDate = dr["sender_issue_date"].ToString();
                                order.SenderExpirationDate = dr["sender_expiration_date"].ToString();
                                order.SenderIssueCountryCode = dr["sender_issue_country_code"].ToString();

                                if (dr["sender_issue_city_code"] != DBNull.Value)
                                    order.SenderIssueCityCode = dr["sender_issue_city_code"].ToString();

                                order.SenderIssueIDNo = dr["sender_issue_id_no"].ToString();

                                if (dr["sender_birth_place_name"] != DBNull.Value)
                                    order.SenderBirthPlaceName = dr["sender_birth_place_name"].ToString();

                                order.SenderSexCode = dr["sender_sex_code"].ToString();
                                order.SenderMobileNo = dr["sender_mobile_no"].ToString();

                                if (dr["sender_fiscal_code"] != DBNull.Value)
                                    order.SenderBirthPlaceName = dr["sender_fiscal_code"].ToString();

                                if (dr["sender_residency_code"] != DBNull.Value)
                                    order.SenderResidencyCode = dr["sender_residency_code"].ToString();

                                order.NATBeneficiaryLastName = dr["beneficiary_last_name"].ToString();

                                if (dr["beneficiary_middle_name"] != DBNull.Value)
                                    order.NATBeneficiaryMiddleName = dr["beneficiary_middle_name"].ToString();

                                order.NATBeneficiaryFirstName = dr["beneficiary_first_name"].ToString();

                                order.BeneficiaryLastName = dr["eng_beneficiary_last_name"].ToString();

                                if (dr["eng_beneficiary_middle_name"] != DBNull.Value)
                                    order.BeneficiaryMiddleName = dr["eng_beneficiary_middle_name"].ToString();

                                order.BeneficiaryFirstName = dr["eng_beneficiary_first_name"].ToString();

                                if (dr["beneficiary_address"] != DBNull.Value)
                                    order.BeneficiaryAddress = dr["beneficiary_address"].ToString();

                                if (dr["beneficiary_email_name"] != DBNull.Value)
                                    order.BeneficiaryEMailName = dr["beneficiary_email_name"].ToString();

                                if (dr["beneficiary_phone_no"] != DBNull.Value)
                                    order.BeneficiaryPhoneNo = dr["beneficiary_phone_no"].ToString();

                                order.BeneficiaryMobileNo = dr["beneficiary_mobile_no"].ToString();

                                if (dr["control_question_name"] != DBNull.Value)
                                    order.ControlQuestionName = dr["control_question_name"].ToString();

                                if (dr["control_answer_name"] != DBNull.Value)
                                    order.ControlAnswerName = dr["control_answer_name"].ToString();

                                order.PurposeRemittanceCode = dr["purpose_remittance_code"].ToString();

                                if (dr["promotion_code"] != DBNull.Value)
                                    order.PromotionCode = dr["promotion_code"].ToString();

                                if (dr["destination_text"] != DBNull.Value)
                                    order.DestinationText = dr["destination_text"].ToString();

                                if (dr["ARUS_Message"] != DBNull.Value)
                                {
                                    order.ARUSErrorMessage = dr["ARUS_Message"].ToString();
                                }

                                if (dr["settlement_exchange_rate"] != DBNull.Value)
                                    order.SettlementExchangeRate = Convert.ToDouble(dr["settlement_exchange_rate"].ToString());

                            }



                        }
                        else
                        {
                            order = null;
                        }
                    }
                }
            }
            order.TransferAdditionalData = TransferAdditionalDataDB.GetTransferAdditionalData(order.Id);
            return order;
        }


        /// <summary>
        /// Միջազգային վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="fastTransferPaymentOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SaveFastTransferPaymentOrderDetails(FastTransferPaymentOrder fastTransferPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[pr_Save_BO_payment_fast_transfer]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@transfer_system", SqlDbType.TinyInt).Value = fastTransferPaymentOrder.SubType;
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = (object)fastTransferPaymentOrder.Code ?? DBNull.Value;

                    cmd.Parameters.Add("@senderName", SqlDbType.NVarChar, 100).Value = (object)fastTransferPaymentOrder.Sender ?? DBNull.Value;
                    cmd.Parameters.Add("@senderAddress", SqlDbType.NVarChar, 100).Value = (object)fastTransferPaymentOrder.SenderAddress ?? DBNull.Value;

                    cmd.Parameters.Add("@senderPassport", SqlDbType.NVarChar, 50).Value = (object)fastTransferPaymentOrder.SenderPassport ?? DBNull.Value;
                    cmd.Parameters.Add("@senderDateOfBirth", SqlDbType.DateTime).Value = (object)fastTransferPaymentOrder.SenderDateOfBirth ?? DBNull.Value;
                    cmd.Parameters.Add("@senderEmail", SqlDbType.NVarChar, 50).Value = (object)fastTransferPaymentOrder.SenderEmail ?? DBNull.Value;


                    cmd.Parameters.Add("@senderPhone", SqlDbType.NVarChar, 50).Value = (object)fastTransferPaymentOrder.SenderPhone ?? DBNull.Value;
                    cmd.Parameters.Add("@senderType", SqlDbType.SmallInt).Value = (object)fastTransferPaymentOrder.SenderType ?? DBNull.Value;


                    if (fastTransferPaymentOrder.ReceiverAddInf != "" && fastTransferPaymentOrder.ReceiverAddInf != null)
                    {
                        cmd.Parameters.Add("@receiverAddInf", SqlDbType.NVarChar, 200).Value = (object)fastTransferPaymentOrder.ReceiverAddInf ?? DBNull.Value;
                    }

                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 250).Value = (object)fastTransferPaymentOrder.Receiver ?? DBNull.Value;
                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 250).Value = (object)fastTransferPaymentOrder.ReceiverPassport ?? DBNull.Value;


                    cmd.Parameters.Add("@descrForPayment", SqlDbType.NVarChar, 150).Value = (object)fastTransferPaymentOrder.DescriptionForPayment ?? DBNull.Value;

                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = (object)fastTransferPaymentOrder.Country ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_in_currency", SqlDbType.Float).Value = (object)fastTransferPaymentOrder.Fee ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_Acba", SqlDbType.Float).Value = (object)fastTransferPaymentOrder.FeeAcba ?? DBNull.Value;



                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Նշում է՝ արդյոք փոխանցումը հաջողությամբ ուղարկվել է է ARUS համակարգում, թե ոչ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="successValue"></param>
        internal static void UpdateARUSSuccess(long orderId, short successValue, string URN)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_Fast_Transfer_Order_Details
                                                        SET ARUS_success = @successValue WHERE Doc_id=@doc_id 
                                                        UPDATE TBL_HB_documents
                                                        SET fast_transfer_code = @URN WHERE doc_id = @doc_id", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@successValue", SqlDbType.TinyInt).Value = successValue;
                    cmd.Parameters.Add("@URN", SqlDbType.NVarChar).Value = URN;

                    cmd.ExecuteScalar();

                }
            }
        }

        /// <summary>
        /// Հայտի վրա թարմացնում է ARUS համակարգից ստացված սխալի հաղորդագրությունը
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="message"></param>
        internal static void UpdateARUSMessage(long orderId, string message)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_Fast_Transfer_Order_Details SET ARUS_message = @message WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = message;

                    cmd.ExecuteScalar();

                }
            }
        }

        internal static Dictionary<string, string> GetRemittanceContractDetails(ulong docId)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT sender_issue_country_code, beneficiary_mobile_no, MTO_agent_code FROM Tbl_Fast_Transfer_Order_Details
                                                         WHERE doc_id  = @doc_id ", conn))
                {
                    cmd.Parameters.Add("doc_id", SqlDbType.Int).Value = docId;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        details.Add("countryName", dr["sender_issue_country_code"].ToString());
                        details.Add("recipientPhoneNumber", dr["beneficiary_mobile_no"].ToString());
                        details.Add("MTOAgentCode", dr["MTO_agent_code"].ToString());
                    }

                }
            }

            return details;
        }

    }
}
