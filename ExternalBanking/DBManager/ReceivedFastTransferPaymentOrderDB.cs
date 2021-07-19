using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class ReceivedFastTransferPaymentOrderDB
    {

        internal static ActionResult Save(ReceivedFastTransferPaymentOrder order, string userName, SourceType source, ushort isCallCenter)
        {


            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_received_transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 100).Value = order.Sender;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = order.Code;
                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 50).Value = order.ReceiverPassport;
                    cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 50).Value = order.ReceiverAccount.AccountNumber;
                    cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 50).Value = order.Country;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 255).Value = order.Receiver;
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
                    cmd.Parameters.Add("@transfer_contract_id", SqlDbType.BigInt).Value = order.ContractId;
                    cmd.Parameters.Add("@registerBy", SqlDbType.TinyInt).Value = isCallCenter;

                    //ARUS համակարգով փոխանցման համար անհրաժեշտ դաշտեր
                    if (order.SubType == 23)
                    {
                        cmd.Parameters.Add("@beneficiary_last_name", SqlDbType.NVarChar).Value = order.BeneficiaryLastName;
                        cmd.Parameters.Add("@beneficiary_first_name", SqlDbType.NVarChar).Value = order.BeneficiaryFirstName;
                        cmd.Parameters.Add("@beneficiary_middle_name", SqlDbType.NVarChar).Value = order.BeneficiaryMiddleName;
                        cmd.Parameters.Add("@beneficiary_sex_code", SqlDbType.NVarChar).Value = order.BeneficiarySexCode;
                        cmd.Parameters.Add("@beneficiary_country_code", SqlDbType.NVarChar).Value = order.BeneficiaryCountryCode;
                        cmd.Parameters.Add("@beneficiary_state_code", SqlDbType.NVarChar).Value = order.BeneficiaryStateCode;
                        cmd.Parameters.Add("@beneficiary_city_code", SqlDbType.NVarChar).Value = order.BeneficiaryCityCode;
                        cmd.Parameters.Add("@beneficiary_address", SqlDbType.NVarChar).Value = order.BeneficiaryAddressName;
                        cmd.Parameters.Add("@beneficiary_zip_code", SqlDbType.NVarChar).Value = order.BeneficiaryZipCode;
                        cmd.Parameters.Add("@beneficiary_document_type_code", SqlDbType.NVarChar).Value = order.BeneficiaryDocumentTypeCode;
                        cmd.Parameters.Add("@beneficiary_occupation", SqlDbType.NVarChar).Value = order.BeneficiaryOccupationName;
                        cmd.Parameters.Add("@beneficiary_issue_country_code", SqlDbType.NVarChar).Value = order.BeneficiaryIssueCountryCode;
                        cmd.Parameters.Add("@beneficiary_issue_city_code", SqlDbType.NVarChar).Value = order.BeneficiaryIssueCityCode;
                        cmd.Parameters.Add("@beneficiary_issue_date", SqlDbType.NVarChar).Value = order.BeneficiaryIssueDate;
                        cmd.Parameters.Add("@beneficiary_expiration_date", SqlDbType.NVarChar).Value = order.BeneficiaryExpirationDate;
                        cmd.Parameters.Add("@beneficiary_fiscal_code", SqlDbType.NVarChar).Value = order.BeneficiaryFiscalCode;
                        cmd.Parameters.Add("@beneficiary_birth_date", SqlDbType.NVarChar).Value = order.BeneficiaryBirthDate;
                        cmd.Parameters.Add("@beneficiary_birth_place", SqlDbType.NVarChar).Value = order.BeneficiaryBirthPlaceName;
                        cmd.Parameters.Add("@beneficiary_residency_code", SqlDbType.NVarChar).Value = order.BeneficiaryResidencyCode;
                        cmd.Parameters.Add("@beneficiary_email", SqlDbType.NVarChar).Value = order.BeneficiaryEMailName;
                        cmd.Parameters.Add("@beneficiary_mobile_number", SqlDbType.NVarChar).Value = order.BeneficiaryMobileNo;
                        cmd.Parameters.Add("@beneficiary_issue_ID_No", SqlDbType.NVarChar).Value = order.BeneficiaryIssueIDNo;
                        cmd.Parameters.Add("@MTO_agent_code", SqlDbType.NVarChar).Value = order.MTOAgentCode;
                        cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar).Value = order.FeeCurrency;
                        cmd.Parameters.Add("@nat_beneficiary_last_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryLastName;
                        cmd.Parameters.Add("@nat_beneficiary_first_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryFirstName;
                        cmd.Parameters.Add("@nat_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryMiddleName;
                        cmd.Parameters.Add("@NAT_sender_name", SqlDbType.NVarChar).Value = order.NATSender;
                        cmd.Parameters.Add("@sender_phone", SqlDbType.NVarChar).Value = order.SenderPhone;
                        cmd.Parameters.Add("@sender_agent_name", SqlDbType.NVarChar).Value = order.SenderAgentName;
                    }
                    
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    if (source == SourceType.AcbaOnline)
                    {
                        cmd.Parameters.Add("@document_number", SqlDbType.NVarChar).Value = order.OrderNumber;
                    }

                    if (!String.IsNullOrEmpty(order.ReceiverPhone))
                    {
                        cmd.Parameters.Add("@receiverPhone", SqlDbType.NVarChar, 50).Value = order.ReceiverPhone;
                    }

                    if (order.ConvertationRate != 0 && order.ConvertationRate1 != 0)
                    {
                        cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = order.ConvertationRate;
                        cmd.Parameters.Add("@rate_sell", SqlDbType.Float).Value = order.ConvertationRate1;
                    }
                    else if (order.ConvertationRate != 0  )
                    {
                        cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = order.ConvertationRate;
                    }
                    else if (order.ConvertationRate1 !=0)
                    {
                        cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = order.ConvertationRate1;
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

        public static double GetFastTransferFeePercent(byte transferType = 0, string code = "", string countryCode = "", double amount = 0, string currency = "", DateTime date =new DateTime() )
        {

            double result;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select  [dbo].[fn_calculate_price_for_fast_transfer](@transfer_system, @code_word, @country_code, @Amount, @currency, @calc_date) ";
                    cmd.Parameters.Add("@transfer_system", SqlDbType.SmallInt).Value = transferType;
                    cmd.Parameters.Add("@code_word", SqlDbType.NVarChar, 30).Value = code;
                    cmd.Parameters.Add("@country_code", SqlDbType.NVarChar, 5).Value = countryCode;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = amount;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = currency;
                    cmd.Parameters.Add("@calc_date", SqlDbType.DateTime).Value = date;

                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }

            return result;

        }

        public static byte GetFastTransferAcbaCommisionType(byte transferType, string code="")
        {

            byte result;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fn_define_ACBA_commision_type_of_fast_transfer(@transfer_type, @code_word)  ";
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = transferType;
                    cmd.Parameters.Add("@code_word", SqlDbType.NVarChar, 30).Value = code;
                    result = byte.Parse(cmd.ExecuteScalar().ToString());
                }
            }

            return result;

        }

        internal static ReceivedFastTransferPaymentOrder Get(ReceivedFastTransferPaymentOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT D.*, C.CountryName CountryName,    type_of_client, transferSystem,
TD.ARUS_success,
                                                TD.beneficiary_last_name,
                                                TD.beneficiary_first_name,
                                                TD.beneficiary_middle_name,
                                                TD.beneficiary_sex_code,
                                                TD.beneficiary_country_code,
                                                TD.beneficiary_state_code,
                                                TD.beneficiary_city_code,
                                                TD.beneficiary_address,
                                                TD.beneficiary_zip_code,
                                                TD.beneficiary_issue_ID_No,
                                                TD.beneficiary_document_type_code,
                                                TD.beneficiary_occupation,
                                                TD.beneficiary_issue_country_code,
                                                TD.beneficiary_issue_city_code,
                                                TD.beneficiary_issue_date,
                                                TD.beneficiary_expiration_date,
                                                TD.beneficiary_fiscal_code,
                                                TD.beneficiary_birth_date,
                                                TD.beneficiary_birth_place,
                                                TD.beneficiary_residency_code,
                                                TD.beneficiary_email,
                                                TD.beneficiary_mobile_number,
                                                TD.ARUS_message,
                                                TD.MTO_agent_code,
                                                TD.fee_currency,
                                                TD.NAT_beneficiary_last_name,
                                                TD.NAT_beneficiary_first_name,
                                                TD.NAT_beneficiary_middle_name,
                                                TD.NAT_sender_name,
                                                TD.sender_phone,
                                                TD.sender_agent_name
                                                         FROM Tbl_Hb_Documents D  
			                                                        LEFT JOIN Tbl_Countries  C  on D.country=C.CountryCodeN
			                                                        LEFT JOIN Tbl_Customers  Cust  on D.customer_number=Cust.customer_number
                                                                    LEFT JOIN Tbl_TransferSystems  T  on D.document_subtype=T.code
                                                                    LEFT JOIN Tbl_Received_Fast_Transfer_Data TD on D.doc_id = TD.doc_id
                                                         WHERE D.doc_id=@id ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                  //  cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
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

                            if (dr["fast_transfer_code"] != DBNull.Value)
                                order.Code = Utility.ConvertAnsiToUnicode(dr["fast_transfer_code"].ToString());
 
                            if (dr["transferSystem"] != DBNull.Value)
                                order.TransferSystemDescription = Utility.ConvertAnsiToUnicode(dr["transferSystem"].ToString());

                            if (dr["receiver_passport"] != DBNull.Value)
                                order.ReceiverPassport = Utility.ConvertAnsiToUnicode(dr["receiver_passport"].ToString());

                            if (dr["Country"] != DBNull.Value)
                                order.Country = dr["Country"].ToString();

                            if (dr["CountryName"] != DBNull.Value)
                                order.CountryName = dr["CountryName"].ToString();


                            if (dr["fee_in_currency"] != DBNull.Value)
                                order.Fee = Convert.ToDouble(dr["fee_in_currency"]);

                            if (dr["fee_Acba"] != DBNull.Value)
                                order.FeeAcba = Convert.ToDouble(dr["fee_Acba"]);

                            if (dr["transfer_contract_id"] != DBNull.Value)
                                order.ContractId = Convert.ToInt64(dr["transfer_contract_id"]);

                            if(((SourceType)int.Parse(dr["source_type"].ToString()) == SourceType.AcbaOnline || (SourceType)int.Parse(dr["source_type"].ToString()) == SourceType.MobileBanking))
                            {
                                if(dr["credit_account"] != DBNull.Value)
                                {
                                    order.ReceiverAccount = new Account();
                                    order.ReceiverAccount = Account.GetAccount(dr["credit_account"].ToString());
                                }

                                if(dr["rate_sell_buy"] != DBNull.Value && Convert.ToDouble((dr["rate_sell_buy"]).ToString()) != 0)
                                {
                                    if (dr["rate_sell_buy_cross"] != DBNull.Value && Convert.ToDouble((dr["rate_sell_buy_cross"]).ToString()) != 0)
                                    {
                                        order.ConvertationRate = Convert.ToDouble((dr["rate_sell_buy"]).ToString());
                                        order.ConvertationRate1 = Convert.ToDouble((dr["rate_sell_buy_cross"]).ToString());
                                    }
                                    else
                                    {
                                        if(dr["currency"].ToString() == "AMD")
                                        {
                                            order.ConvertationRate = Convert.ToDouble((dr["rate_sell_buy"]).ToString());
                                            order.ConvertationRate1 = 0;
                                        }
                                        else
                                        {
                                            order.ConvertationRate = 0;
                                            order.ConvertationRate1 = Convert.ToDouble((dr["rate_sell_buy"]).ToString());                                           
                                        }
                                    }
                                }                             

                            }

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                            //ARUS համակարգով ստացված փոխանցում
                            if (order.SubType == 23)
                            {
                                order.ARUSSuccess = Convert.ToInt16(dr["ARUS_success"]);

                                if (dr["beneficiary_last_name"] != DBNull.Value)
                                    order.BeneficiaryLastName = dr["beneficiary_last_name"].ToString();

                                if (dr["beneficiary_first_name"] != DBNull.Value)
                                    order.BeneficiaryFirstName = dr["beneficiary_first_name"].ToString();

                                if (dr["beneficiary_middle_name"] != DBNull.Value)
                                    order.BeneficiaryMiddleName = dr["beneficiary_middle_name"].ToString();

                                if (dr["beneficiary_sex_code"] != DBNull.Value)
                                    order.BeneficiarySexCode = dr["beneficiary_sex_code"].ToString();

                                if (dr["beneficiary_country_code"] != DBNull.Value)
                                    order.BeneficiaryCountryCode = dr["beneficiary_country_code"].ToString();

                                if (dr["beneficiary_state_code"] != DBNull.Value)
                                    order.BeneficiaryStateCode = dr["beneficiary_state_code"].ToString();

                                if (dr["beneficiary_city_code"] != DBNull.Value)
                                    order.BeneficiaryCityCode = dr["beneficiary_city_code"].ToString();

                                if (dr["beneficiary_address"] != DBNull.Value)
                                    order.BeneficiaryAddressName = dr["beneficiary_address"].ToString();

                                if (dr["beneficiary_zip_code"] != DBNull.Value)
                                    order.BeneficiaryZipCode = dr["beneficiary_zip_code"].ToString();

                                if (dr["beneficiary_issue_ID_No"] != DBNull.Value)
                                    order.BeneficiaryIssueIDNo = dr["beneficiary_issue_ID_No"].ToString();

                                if (dr["beneficiary_document_type_code"] != DBNull.Value)
                                    order.BeneficiaryDocumentTypeCode = dr["beneficiary_document_type_code"].ToString();

                                if (dr["beneficiary_occupation"] != DBNull.Value)
                                    order.BeneficiaryOccupationName = dr["beneficiary_occupation"].ToString();

                                if (dr["beneficiary_issue_country_code"] != DBNull.Value)
                                    order.BeneficiaryIssueCountryCode = dr["beneficiary_issue_country_code"].ToString();

                                if (dr["beneficiary_issue_city_code"] != DBNull.Value)
                                    order.BeneficiaryIssueCityCode = dr["beneficiary_issue_city_code"].ToString();

                                if (dr["beneficiary_issue_date"] != DBNull.Value)
                                    order.BeneficiaryIssueDate = dr["beneficiary_issue_date"].ToString();

                                if (dr["beneficiary_expiration_date"] != DBNull.Value)
                                    order.BeneficiaryExpirationDate = dr["beneficiary_expiration_date"].ToString();

                                if (dr["beneficiary_fiscal_code"] != DBNull.Value)
                                    order.BeneficiaryFiscalCode = dr["beneficiary_fiscal_code"].ToString();

                                if (dr["beneficiary_birth_date"] != DBNull.Value)
                                    order.BeneficiaryBirthDate = dr["beneficiary_birth_date"].ToString();

                                if (dr["beneficiary_birth_place"] != DBNull.Value)
                                    order.BeneficiaryBirthPlaceName = dr["beneficiary_birth_place"].ToString();

                                if (dr["beneficiary_residency_code"] != DBNull.Value)
                                    order.BeneficiaryResidencyCode = dr["beneficiary_residency_code"].ToString();

                                if (dr["beneficiary_email"] != DBNull.Value)
                                    order.BeneficiaryEMailName = dr["beneficiary_email"].ToString();

                                if (dr["beneficiary_mobile_number"] != DBNull.Value)
                                    order.BeneficiaryMobileNo = dr["beneficiary_mobile_number"].ToString();

                                if (dr["ARUS_message"] != DBNull.Value)
                                    order.ARUSErrorMessage = dr["ARUS_message"].ToString();

                                if (dr["MTO_agent_code"] != DBNull.Value)
                                    order.MTOAgentCode = dr["MTO_agent_code"].ToString();

                                if (dr["receiver_add_inf"] != DBNull.Value)
                                    order.ReceiverPhone = dr["receiver_add_inf"].ToString();

                                if (dr["fee_currency"] != DBNull.Value)
                                    order.FeeCurrency = dr["fee_currency"].ToString();

                                if (dr["NAT_beneficiary_last_name"] != DBNull.Value)
                                    order.NATBeneficiaryLastName = dr["NAT_beneficiary_last_name"].ToString();
                                
                                if (dr["NAT_beneficiary_first_name"] != DBNull.Value)
                                    order.NATBeneficiaryFirstName = dr["NAT_beneficiary_first_name"].ToString();
                                
                                if (dr["NAT_beneficiary_middle_name"] != DBNull.Value)
                                    order.NATBeneficiaryMiddleName = dr["NAT_beneficiary_middle_name"].ToString();
                                
                                if (dr["NAT_sender_name"] != DBNull.Value)
                                    order.NATSender = dr["NAT_sender_name"].ToString();
                                
                                if (dr["sender_phone"] != DBNull.Value)
                                    order.SenderPhone = dr["sender_phone"].ToString();
                                
                                if (dr["sender_agent_name"] != DBNull.Value)
                                    order.SenderAgentName = dr["sender_agent_name"].ToString();
                            }



                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
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
        /// <param name="receivedFastTransferPaymentOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SaveReceivedFastTransferPaymentOrderDetails(ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_payment_received_fast_transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@transfer_system", SqlDbType.TinyInt).Value = receivedFastTransferPaymentOrder.SubType;
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = (object)receivedFastTransferPaymentOrder.Code ?? DBNull.Value;

                    cmd.Parameters.Add("@senderName", SqlDbType.NVarChar, 100).Value = (object)receivedFastTransferPaymentOrder.Sender ?? DBNull.Value;


                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 250).Value = (object)receivedFastTransferPaymentOrder.Receiver ?? DBNull.Value;
                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 250).Value = (object)receivedFastTransferPaymentOrder.ReceiverPassport ?? DBNull.Value;
 
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = (object)receivedFastTransferPaymentOrder.Country ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_in_currency", SqlDbType.Float).Value = (object)receivedFastTransferPaymentOrder.Fee ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_Acba", SqlDbType.Float).Value = (object)receivedFastTransferPaymentOrder.FeeAcba ?? DBNull.Value;


                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Նույն տվյալներով փոխանցման առկայության ստուգում
        /// </summary>
        internal static bool IsExistingTransferByCall(short transferSystem, string code, long transferId)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"IF exists(SELECT 1 FROM Tbl_transfers_by_call WHERE quality<>1  and transfer_system =@transfer_system and code_word =@code_word And ID <>@Id    and deleting_date is null)
                                                        BEGIN
	                                                        SELECT 1
                                                        END
                                                        ELSE IF EXISTS(SELECT 1  FROM tbl_bank_mail_in B JOIN Tbl_Bank_Mail_IN_Reestr R ON B.unic_number =R.unic_number and B.registration_date=R.registration_date  where deleted=0 and transfersystem =@transfer_system and code_word =@code_word   and   [i/o] =0 )
                                                        BEGIN
	                                                        SELECT 1
                                                        END", conn))
                {
                    cmd.Parameters.Add("@transfer_system", SqlDbType.SmallInt).Value = transferSystem;
                    cmd.Parameters.Add("@code_word", SqlDbType.NVarChar, 20).Value = code;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = transferId;
                    if (cmd.ExecuteReader().Read())
                    {
                        check = true;
                    }
                }
            }

            return check;
        }


        /// <summary>
        /// Վերադարձնում է ստացված արագ փոխանցման հայտի մերժման պատճառը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static string GetReceivedFastTransferOrderRejectReason(long orderId, Languages language)
        {

            string reason = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_get_fast_transfer_reject_reason ", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@language", SqlDbType.Int).Value = (int)language;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["regect_descr"] != DBNull.Value)
                                reason = Utility.ConvertAnsiToUnicode(dr["regect_descr"].ToString());
                        }
                    }     
                }
            }
            return reason;
        }


       internal static void  UpdateOrderCurrencyFastTransfer(double convertationRate, double convertationRate1, long docID)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"update tbl_hb_documents set rate_sell_buy  = @ConvertationRate , rate_sell_buy_cross = @ConvertationRate1 where doc_id  = @docID", conn))
                {
                    cmd.Parameters.Add("@ConvertationRate", SqlDbType.Float).Value = convertationRate;
                    cmd.Parameters.Add("@ConvertationRate1", SqlDbType.Float).Value = convertationRate1;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;

                    cmd.ExecuteNonQuery();
                }

            }
        }

        internal static void SetTransferByCallType(short type,  long id)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"update tbl_transfers_by_call set transfer_system  = @type where id  = @id", conn))
                {
                    cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;

                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Նշում է՝ արդյոք փոխանցումը հաջողությամբ ստացվել է ARUS համակարգում, թե ոչ
        /// </summary>
        /// <param name="orderId">doc_id</param>
        /// <param name="successValue">1՝ բարեհաջող ավարտ, 0՝ անհաջող ավարտ</param>
        internal static void UpdateARUSSuccess(long orderId, short successValue)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE TBl_Received_Fast_Transfer_Data SET ARUS_success = @successValue WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@successValue", SqlDbType.TinyInt).Value = successValue;

                    cmd.ExecuteScalar();

                }
            }
        }

        /// <summary>
        /// Հայտի վրա թարմացնում է ARUS համակարգից ստացված սխալի հաղորդագրությունը
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="successValue"></param>
        /// <param name="message"></param>
        internal static void UpdateARUSMessage(long orderId, string message)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE TBl_Received_Fast_Transfer_Data SET ARUS_message = @message WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = message;

                    cmd.ExecuteScalar();

                }
            }
        }

    }
}
