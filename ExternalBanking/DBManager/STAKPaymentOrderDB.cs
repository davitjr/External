using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ARUSDataService;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    internal static class STAKPaymentOrderDB
    {
        //internal static ActionResult Save(STAKPaymentOrder order, string userName, SourceType source, ref R2ARequestOutput r2ARequestOutput)
        internal static ActionResult Save(STAKPaymentOrder order, string userName, SourceType source)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_STAK_R2A_request";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 100).Value = order.R2ADetails.SenderFirstName + ' ' + order.R2ADetails.SenderLastName;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@fast_transfer_code", SqlDbType.NVarChar, 20).Value = order.Code;
                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 50).Value = order.ReceiverPassport != null ? order.ReceiverPassport : (object)DBNull.Value;
                    cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 50).Value = order.ReceiverAccount.AccountNumber;
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = order.Country != null ? order.Country : (object)DBNull.Value;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 255).Value = order.Receiver != null ? order.Receiver : (object)DBNull.Value;
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 4000).Value = order.Description != null ? order.Description : (object)DBNull.Value;
                    cmd.Parameters.Add("@fee_ACBA", SqlDbType.Float).Value = order.FeeAcba;
                    cmd.Parameters.Add("@acba_commission_currency", SqlDbType.NVarChar, 3).Value = order.ACBACommissionCurrency;

                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = (short)order.SubType;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@receiver_add_inf", SqlDbType.NVarChar, 50).Value = !String.IsNullOrEmpty(order.R2ADetails.BeneficiaryMobileNo) ? order.R2ADetails.BeneficiaryMobileNo : (object)DBNull.Value;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = order.user.userID;


                    // DETAILS 
                    cmd.Parameters.Add("@MTO_agent_code", SqlDbType.NVarChar).Value = order.R2ADetails.MTOAgentCode != null ? order.R2ADetails.MTOAgentCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@send_agent_code", SqlDbType.NVarChar).Value = order.R2ADetails.SendAgentCode;
                    cmd.Parameters.Add("@URN", SqlDbType.NVarChar).Value = order.Code;
                    cmd.Parameters.Add("@account_no", SqlDbType.NVarChar).Value = order.R2ADetails.AccountNo;
                    cmd.Parameters.Add("@payout_delivery_code", SqlDbType.NVarChar).Value = order.R2ADetails.PayoutDeliveryCode;
                    cmd.Parameters.Add("@bank_code", SqlDbType.NVarChar).Value = order.R2ADetails.BankCode;
                    cmd.Parameters.Add("@beneficiary_agent_code", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryAgentCode;
                    cmd.Parameters.Add("@beneficiary_country_code", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryCountryCode;
                    cmd.Parameters.Add("@beneficiary_state_code", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryStateCode != null ? order.R2ADetails.BeneficiaryStateCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_city_code", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryCityCode != null ? order.R2ADetails.BeneficiaryCityCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@send_currency_code", SqlDbType.NVarChar).Value = order.SendCurrencyCode;
                    cmd.Parameters.Add("@send_amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@beneficiary_fee_currency_code", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryFeeCurrencyCode;
                    cmd.Parameters.Add("@beneficiary_fee", SqlDbType.Float).Value = Convert.ToDouble(order.R2ADetails.BeneficiaryFee);
                    cmd.Parameters.Add("@sender_last_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderLastName;
                    cmd.Parameters.Add("@sender_middle_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderMiddleName != null ? order.R2ADetails.SenderMiddleName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_first_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderFirstName;
                    cmd.Parameters.Add("@nat_sender_last_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATSenderLastName != null ? order.R2ADetails.NATSenderLastName : (object)DBNull.Value;
                    cmd.Parameters.Add("@nat_sender_middle_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATSenderMiddleName != null ? order.R2ADetails.NATSenderMiddleName : (object)DBNull.Value;
                    cmd.Parameters.Add("@nat_sender_first_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATSenderFirstName != null ? order.R2ADetails.NATSenderFirstName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_country_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderCountryCode;
                    cmd.Parameters.Add("@sender_state_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderStateCode != null ? order.R2ADetails.SenderStateCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_city_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderCityCode != null ? order.R2ADetails.SenderCityCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_address_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderAddressName != null ? order.R2ADetails.SenderAddressName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_zip_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderZipCode != null ? order.R2ADetails.SenderZipCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_occupation_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderOccupationName != null ? order.R2ADetails.SenderOccupationName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_document_type_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderDocumentTypeCode;
                    cmd.Parameters.Add("@sender_issue_date", SqlDbType.NVarChar).Value = order.R2ADetails.SenderIssueDate;
                    cmd.Parameters.Add("@sender_expiration_date", SqlDbType.NVarChar).Value = order.R2ADetails.SenderExpirationDate;
                    cmd.Parameters.Add("@sender_issue_country_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderIssueCountryCode;
                    cmd.Parameters.Add("@sender_issue_city_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderIssueCityCode != null ? order.R2ADetails.SenderIssueCityCode : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_issue_id_no", SqlDbType.NVarChar).Value = order.R2ADetails.SenderIssueIDNo;
                    cmd.Parameters.Add("@sender_birth_date", SqlDbType.NVarChar).Value = order.R2ADetails.SenderBirthDate;
                    cmd.Parameters.Add("@sender_birth_place_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderBirthPlaceName != null ? order.R2ADetails.SenderBirthPlaceName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_sex_code", SqlDbType.NVarChar).Value = order.R2ADetails.SenderSexCode;
                    cmd.Parameters.Add("@sender_email_name", SqlDbType.NVarChar).Value = order.R2ADetails.SenderEMailName != null ? order.R2ADetails.SenderEMailName : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_phone_no", SqlDbType.NVarChar).Value = order.R2ADetails.SenderPhoneNo != null ? order.R2ADetails.SenderPhoneNo : (object)DBNull.Value;
                    cmd.Parameters.Add("@sender_mobile_no", SqlDbType.NVarChar).Value = order.R2ADetails.SenderMobileNo;
                    cmd.Parameters.Add("@beneficiary_last_name", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryLastName;
                    cmd.Parameters.Add("@beneficiary_middle_name", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryMiddleName != null ? order.R2ADetails.BeneficiaryMiddleName : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_first_name", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryFirstName;
                    cmd.Parameters.Add("@nat_beneficiary_last_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATBeneficiaryLastName != null ? order.R2ADetails.NATBeneficiaryLastName : (object)DBNull.Value;
                    cmd.Parameters.Add("@nat_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATBeneficiaryMiddleName != null ? order.R2ADetails.NATBeneficiaryMiddleName : (object)DBNull.Value;
                    cmd.Parameters.Add("@nat_beneficiary_first_name", SqlDbType.NVarChar).Value = order.R2ADetails.NATBeneficiaryFirstName != null ? order.R2ADetails.NATBeneficiaryFirstName : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_address", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryAddress != null ? order.R2ADetails.BeneficiaryAddress : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_email_name", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryEMailName != null ? order.R2ADetails.BeneficiaryEMailName : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_phone_no", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryPhoneNo != null ? order.R2ADetails.BeneficiaryPhoneNo : (object)DBNull.Value;
                    cmd.Parameters.Add("@beneficiary_mobile_no", SqlDbType.NVarChar).Value = order.R2ADetails.BeneficiaryMobileNo;
                    cmd.Parameters.Add("@purpose_remittance_code", SqlDbType.NVarChar, 4000).Value = order.R2ADetails.PurposeRemittanceCode;
                    cmd.Parameters.Add("@destination_text", SqlDbType.NVarChar).Value = order.R2ADetails.DestinationText != null ? order.R2ADetails.DestinationText : (object)DBNull.Value;

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    order.Quality = OrderQuality.Draft;


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = order.Id;
                        //r2ARequestOutput = new R2ARequestOutput();
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

        internal static ulong GetBankMailID(long docID, ref ulong bankMailID, ref string transferDebitAccount)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT ID, operation_deb_account
                                                            FROM Tbl_Bank_Mail_IN T INNER JOIN Tbl_bank_mail_in_reestr TR ON T.registration_date = TR.registration_date AND T.unic_number = TR.unic_number
                                                            WHERE T.Add_tbl_unic_number = @doc_id";

                    cmd.Parameters.Add("@doc_id", SqlDbType.Float).Value = docID;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["ID"] != DBNull.Value)
                                bankMailID = ulong.Parse(dr["ID"].ToString());

                            if (dr["operation_deb_account"] != DBNull.Value)
                                transferDebitAccount = dr["operation_deb_account"].ToString();
                        }
                    }

                }
            }

            return bankMailID;

        }

        internal static R2ARequestOutput GetR2ARequestOutput(STAKPaymentOrder order)
        {

            R2ARequestOutput output = new R2ARequestOutput();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_get_STAK_R2A_request";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.R2ATransferDocID;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            PhysicalCustomer physicalCustomer = (PhysicalCustomer)ACBAOperationService.GetCustomer(order.CustomerNumber);


                            if (dr["MTO_agent_code"] != DBNull.Value)
                                output.MTOAgentCode = dr["MTO_agent_code"].ToString();

                            if (dr["send_agent_code"] != DBNull.Value)
                                output.SendAgentCode = dr["send_agent_code"].ToString();

                            if (dr["URN"] != DBNull.Value)
                                output.URN = dr["URN"].ToString();

                            if (dr["account_no"] != DBNull.Value)
                                output.AccountNo = dr["account_no"].ToString();

                            if (dr["payout_delivery_code"] != DBNull.Value)
                                output.PayoutDeliveryCode = dr["payout_delivery_code"].ToString();

                            if (dr["bank_code"] != DBNull.Value)
                                output.BankCode = dr["bank_code"].ToString();

                            if (dr["beneficiary_agent_code"] != DBNull.Value)
                                output.BeneficiaryAgentCode = dr["beneficiary_agent_code"].ToString();

                            if (dr["beneficiary_country_code"] != DBNull.Value)
                                output.BeneficiaryCountryCode = dr["beneficiary_country_code"].ToString();

                            if (dr["beneficiary_state_code"] != DBNull.Value)
                                output.BeneficiaryStateCode = dr["beneficiary_state_code"].ToString();

                            if (dr["beneficiary_city_code"] != DBNull.Value)
                                output.BeneficiaryCityCode = dr["beneficiary_city_code"].ToString();

                            if (dr["send_currency_code"] != DBNull.Value)
                                output.SendCurrencyCode = dr["send_currency_code"].ToString();

                            if (dr["send_amount"] != DBNull.Value)
                                output.SendAmount = dr["send_amount"].ToString();

                            output.PayoutCurrencyCode = order.ReceiverAccount.Currency;

                            /////////// ToDo STAK voroshel vcharvac gumary ??????????????????? 
                            output.PayoutAmount = Convert.ToDecimal(dr["send_amount"]);

                            if (dr["beneficiary_fee_currency_code"] != DBNull.Value)
                                output.BeneficiaryFeeCurrencyCode = dr["beneficiary_fee_currency_code"].ToString();

                            if (dr["beneficiary_fee"] != DBNull.Value)
                                output.BeneficiaryFee = dr["beneficiary_fee"].ToString();

                            if (dr["sender_last_name"] != DBNull.Value)
                                output.SenderLastName = dr["sender_last_name"].ToString();

                            if (dr["sender_middle_name"] != DBNull.Value)
                                output.SenderMiddleName = dr["sender_middle_name"].ToString();

                            if (dr["sender_first_name"] != DBNull.Value)
                                output.SenderFirstName = dr["sender_first_name"].ToString();

                            if (dr["nat_sender_last_name"] != DBNull.Value)
                                output.NATSenderLastName = dr["nat_sender_last_name"].ToString();

                            if (dr["nat_sender_middle_name"] != DBNull.Value)
                                output.NATSenderMiddleName = dr["nat_sender_middle_name"].ToString();

                            if (dr["nat_sender_first_name"] != DBNull.Value)
                                output.NATSenderFirstName = dr["nat_sender_first_name"].ToString();

                            if (dr["sender_country_code"] != DBNull.Value)
                                output.SenderCountryCode = dr["sender_country_code"].ToString();

                            if (dr["sender_state_code"] != DBNull.Value)
                                output.SenderStateCode = dr["sender_state_code"].ToString();

                            if (dr["sender_city_code"] != DBNull.Value)
                                output.SenderCityCode = dr["sender_city_code"].ToString();

                            if (dr["sender_address_name"] != DBNull.Value)
                                output.SenderAddressName = dr["sender_address_name"].ToString();

                            if (dr["sender_zip_code"] != DBNull.Value)
                                output.SenderZipCode = dr["sender_zip_code"].ToString();

                            if (dr["sender_occupation_name"] != DBNull.Value)
                                output.SenderOccupationName = dr["sender_occupation_name"].ToString();

                            if (dr["sender_document_type_code"] != DBNull.Value)
                                output.SenderDocumentTypeCode = dr["sender_document_type_code"].ToString();

                            if (dr["sender_issue_date"] != DBNull.Value)
                                output.SenderIssueDate = dr["sender_issue_date"].ToString();

                            if (dr["sender_expiration_date"] != DBNull.Value)
                                output.SenderExpirationDate = dr["sender_expiration_date"].ToString();

                            if (dr["sender_issue_country_code"] != DBNull.Value)
                                output.SenderIssueCountryCode = dr["sender_issue_country_code"].ToString();

                            if (dr["sender_issue_city_code"] != DBNull.Value)
                                output.SenderIssueCityCode = dr["sender_issue_city_code"].ToString();

                            if (dr["sender_issue_id_no"] != DBNull.Value)
                                output.SenderIssueIDNo = dr["sender_issue_id_no"].ToString();

                            if (dr["sender_birth_date"] != DBNull.Value)
                                output.SenderBirthDate = dr["sender_birth_date"].ToString();

                            if (dr["sender_birth_place_name"] != DBNull.Value)
                                output.SenderBirthPlaceName = dr["sender_birth_place_name"].ToString();

                            if (dr["sender_sex_code"] != DBNull.Value)
                                output.SenderSexCode = dr["sender_sex_code"].ToString();

                            if (dr["sender_email_name"] != DBNull.Value)
                                output.SenderEMailName = dr["sender_email_name"].ToString();

                            if (dr["sender_phone_no"] != DBNull.Value)
                                output.SenderPhoneNo = dr["sender_phone_no"].ToString();

                            if (dr["sender_mobile_no"] != DBNull.Value)
                                output.SenderMobileNo = dr["sender_mobile_no"].ToString();

                            if (dr["beneficiary_last_name"] != DBNull.Value)
                                output.BeneficiaryLastName = dr["beneficiary_last_name"].ToString();

                            if (dr["beneficiary_middle_name"] != DBNull.Value)
                                output.BeneficiaryMiddleName = dr["beneficiary_middle_name"].ToString();

                            if (dr["beneficiary_first_name"] != DBNull.Value)
                                output.BeneficiaryFirstName = dr["beneficiary_first_name"].ToString();

                            if (dr["nat_beneficiary_last_name"] != DBNull.Value)
                                output.NATBeneficiaryLastName = dr["nat_beneficiary_last_name"].ToString();

                            if (dr["nat_beneficiary_middle_name"] != DBNull.Value)
                                output.NATBeneficiaryMiddleName = dr["nat_beneficiary_middle_name"].ToString();

                            if (dr["nat_beneficiary_first_name"] != DBNull.Value)
                                output.NATBeneficiaryFirstName = dr["nat_beneficiary_first_name"].ToString();

                            if (dr["beneficiary_address"] != DBNull.Value)
                                output.BeneficiaryAddress = dr["beneficiary_address"].ToString();

                            if (physicalCustomer.person.gender.key.ToString() == "1")
                            {
                                output.BeneficiarySexCode = "M";
                            }
                            else if (physicalCustomer.person.gender.key.ToString() == "2")
                            {
                                output.BeneficiarySexCode = "F";
                            }
                            else
                                output.BeneficiarySexCode = "";


                            //output.BeneficiaryZipCode = "0051";  // IS Optional

                            // ToDo STAK chshtel DocumentType-y ev zbaxvacutyuny
                            output.BeneficiaryDocumentTypeCode = "1";
                            output.BeneficiaryOccupationName = "Electrician";


                            DataTable dt = Info.GetCountries();

                            string country = physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.documentCountry.key;
                            string birthPlace = physicalCustomer.person.birthPlace.key;


                            output.BeneficiaryIssueCountryCode = dt.Select("CountryCodeN = '" + country + "'")[0]["CountryCodeA3"].ToString();

                            // output.BeneficiaryIssueCityCode = "055";  // IS Optional

                            ////////////////////////////////   CHSHTEL CustomerDocument-y   ////////////////////////////////////////
                            //CustomerDocument pass = person.documentList.Find(cd => cd.documentNumber == physicalCustomer.DefaultDocument && cd.defaultSign);
                            output.BeneficiaryIssueIDNo = String.IsNullOrEmpty(physicalCustomer.passportNumber) ? "" : physicalCustomer.passportNumber;
                            output.BeneficiaryIssueDate = (physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.givenDate).HasValue ? (DateTime)(physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.givenDate) : default(DateTime);
                            output.BeneficiaryExpirationDate = (physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.validDate).HasValue ? (DateTime)(physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.validDate) : default(DateTime);
                            output.BeneficiaryBirthDate = physicalCustomer.person.birthDate.HasValue ? (DateTime)physicalCustomer.person.birthDate : default(DateTime);
                            output.BeneficiaryBirthPlaceName = dt.Select("CountryCodeN = '" + birthPlace + "'")[0]["CountryName"].ToString();


                            if (dr["beneficiary_email_name"] != DBNull.Value)
                                output.BeneficiaryEMailName = dr["beneficiary_email_name"].ToString();

                            if (dr["beneficiary_phone_no"] != DBNull.Value)
                                output.BeneficiaryPhoneNo = dr["beneficiary_phone_no"].ToString();

                            if (dr["beneficiary_mobile_no"] != DBNull.Value)
                                output.BeneficiaryMobileNo = dr["beneficiary_mobile_no"].ToString();

                            //output.BeneficiaryFiscalCode =  "0024146";  // IS Optional

                            if (physicalCustomer.residence.key == 1)    // Ռեզիդենտ հաճախորդ
                            {
                                output.BeneficiaryResidencyCode = "1";
                            }
                            else if (physicalCustomer.residence.key == 2)   // Ոչ ռեզիդենտ հաճախորդ
                            {
                                output.BeneficiaryResidencyCode = "0";
                            }
                            else
                            {
                                output.BeneficiaryResidencyCode = "";
                            }

                            if (dr["purpose_remittance_code"] != DBNull.Value)
                                output.PurposeRemittanceCode = dr["purpose_remittance_code"].ToString();

                            if (dr["destination_text"] != DBNull.Value)
                                output.DestinationText = dr["destination_text"].ToString();

                            if (order.Type == OrderType.TransitNonCashOut)
                            {
                                output.TransactionCode = Order.GetOrderTransactionsGroupNumber(order.R2APaymentDocID).ToString();
                            }
                            else if (order.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                            {
                                output.TransactionCode = Order.GetOrderTransactionsGroupNumber(order.R2ACurrencyExchangeDocID).ToString();
                            }

                            return output;
                        }
                        else
                        {
                            output = null;
                        }

                    }
                }
            }

            return output;
        }


        internal static ActionResult CheckWithCriminalLists(STAKPaymentOrder order)
        {
            ActionResult actionResult = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_check_with_criminal_lists", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = order.user.userID;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@transferSystem", SqlDbType.SmallInt).Value = (short)order.SubType;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@operationCurrency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@countryCode", SqlDbType.NVarChar, 200).Value = order.Country;
                    cmd.Parameters.Add("@senderName", SqlDbType.NVarChar, 500).Value = order.R2ADetails.SenderFirstName + ' ' + order.R2ADetails.SenderLastName;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;


                    try
                    {
                        cmd.ExecuteNonQuery();
                        actionResult.ResultCode = ResultCode.Normal;
                    }
                    catch (Exception ex)
                    {
                        if (((System.Data.SqlClient.SqlException)ex).Class == 11)
                        {
                            actionResult.Errors.Add(new ActionError(ex.Message));
                            actionResult.ResultCode = ResultCode.Failed;
                        }
                        else
                        {
                            throw;
                        }
                    }

                }
            }

            return actionResult;
        }

        internal static void SaveValidationErrors(STAKPaymentOrder order, short errorCode, string errorDescription, string guid)
        {
            //ActionResult actionResult = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_save_R2A_validation_errors ", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@term_id", SqlDbType.Int).Value = errorCode;
                    cmd.Parameters.Add("@term_description", SqlDbType.NVarChar, 4000).Value = errorDescription;
                    cmd.Parameters.Add("@URN", SqlDbType.NVarChar, 100).Value = order.Code;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = order.Source;
                    cmd.Parameters.Add("@transfer_doc_id", SqlDbType.Int).Value = order.R2ATransferDocID;
                    cmd.Parameters.Add("@payment_doc_id", SqlDbType.Int).Value = order.R2APaymentDocID;
                    cmd.Parameters.Add("@currency_exchange_doc_id", SqlDbType.Int).Value = order.R2ACurrencyExchangeDocID;
                    cmd.Parameters.Add("@GuID", SqlDbType.NVarChar, 500).Value = guid;
                    cmd.Parameters.Add("@registration_set_number", SqlDbType.Int).Value = order.user.userID;

                    try
                    {
                        cmd.ExecuteNonQuery();
                        //actionResult.ResultCode = ResultCode.Normal;
                    }
                    catch (Exception ex)
                    {
                        //if (((System.Data.SqlClient.SqlException)ex).Class == 11)
                        //{
                        //    actionResult.Errors.Add(new ActionError(ex.Message));
                        //    actionResult.ResultCode = ResultCode.Failed;
                        //}
                        //else
                        //{
                        throw;
                        //}
                    }

                }
            }

            //return actionResult;
        }
    }
}

