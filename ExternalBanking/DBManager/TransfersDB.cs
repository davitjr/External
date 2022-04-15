using ExternalBanking.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
namespace ExternalBanking.DBManager
{
    class TransferDB
    {


        /// <summary>
        /// Վերադարձնում է  փոխանցումը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static Transfer Get(Transfer transfer, User user = null)
        {

            DataTable dt = new DataTable();
            DataTable dtDocFlow = new DataTable();


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string str = @"Select T.*,TR.*, isnull(S.Instant_money_transfer,0) as Instant_money_transfer,
                                                S.transfersystem transfersystemDesc,   case when Transfer_group=1 then 'Bank_Mail' when Transfer_group=3 then N'Միջազգային' when Transfer_group=4 then N'Միջ. առանց հաշվի բացման' else '' end transfer_group_desc,  
                                                ISNULL(N'Անձնագիր` '+ debtor_passport + ' ' ,'') +  ISNULL(N'ՀՎՀՀ` ' + debtor_code_of_tax + ' ' ,'') +ISNULL(N'ՀԾՀ` ' + debtor_social_number,ISNULL (N'ՀԾՀ` ' +debtor_nosocial_number , ISNULL(N'Մահվան վկայական` ' + debtor_death_ditails,''))) debtor_document,
                                                C.CountryName CountryName  , SD.description customer_type_description, SDD.description debtor_type_description, SDR.description receiver_type_description,
                                                SC.CountryName senderCountryName ,  case when credit_bank_code like '9%' and cast(TR.add_inf as nvarchar(1))<>'' then CAST(TR.add_inf  as nvarchar(5)) else 0 end police_code,   BC.rate_sell rateSell, BC.rate_buy rateBuy,LM.description matureDesc,
                                                case when T.Add_tbl_name = 'Tbl_HB_documents' then replace(replace([dbo].[fn_get_document_source_text] (Add_tbl_unic_number),'§',''), '¦','')       else isnull(TN.Table_description,'') end   transfer_sourse, isnull(BC.registered_by, 0) callCenter,
                                                isnull(registration_set_number, 0 ) registration_set_number,isnull( VC.First_Name + ' ' + VC.Last_Name, '')  registration_name, isnull(T.confirmation_set_number,0)  confirmation_set_number, isnull( VConf.First_Name + ' ' + VConf.Last_Name, '')  confirmation_name,
					                            CASE WHEN ISNULL(debtor_type,0)=0 THEN '' WHEN debtor_type not in (10, 20) THEN debtor_code_of_tax  ELSE ISNULL(debtor_social_number , debtor_nosocial_number) END debtorDocumentNumber, 
                                                CASE WHEN T.Add_tbl_name = 'Tbl_HB_documents' THEN dbo.fn_get_HB_document_source(T.Add_tbl_unic_number) ELSE 0 END AS hb_source_type
                                    From Tbl_bank_mail_in T Inner join Tbl_bank_mail_in_reestr TR ON T.registration_date = TR.registration_date AND T.unic_number = TR.unic_number  
							                LEFT JOIN Tbl_TransferSystems S ON T.transfersystem=S.code
                                            LEFT JOIN  Tbl_Countries  C  on T.country=C.CountryCodeN
                                            LEFT JOIN Tbl_transfer_Add_table_name TN ON T.Add_tbl_name=TN.Table_name 
                                            LEFT JOIN  Tbl_Countries  SC  on TR.sender_country =SC.CountryCodeN
                                            LEFT JOIN Tbl_sint_acc_details SD on SD.value_for8=sender_type 
                                            LEFT JOIN Tbl_sint_acc_details SDD on SDD.value_for8=debtor_type  
                                            LEFT JOIN Tbl_sint_acc_details SDR on SDR.value_for8=receiver_type  
                                            LEFT JOIN V_cashers_list VC on VC.new_id=registration_set_number
                                            LEFT JOIN V_cashers_list VConf on VConf.new_id=T.confirmation_set_number
                                            LEFT JOIN (select department_id, transfer_unic_number, transfer_registration_date, registered_by, rate_sell, rate_buy  from Tbl_transfers_by_call ) BC ON T.unic_number = BC.transfer_unic_number and T.registration_date = BC.transfer_registration_date
	                                       LEFT JOIN tbl_types_of_loan_mature LM ON LM.code=TR.mature_type
                                      Where  T.id=@id ";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = transfer.Id;

                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count != 0)
                    {

                        transfer.AmlCheck = (dt.Rows[0]["aml_check"] != DBNull.Value) ? Convert.ToInt16(dt.Rows[0]["aml_check"]) : default(short);
                        transfer.Amount = (dt.Rows[0]["amount"] != DBNull.Value) ? Convert.ToDouble(dt.Rows[0]["amount"]) : default(double);
                        if (dt.Rows[0]["Confirmation_date"] != DBNull.Value)
                        {
                            transfer.ConfirmationDate = Convert.ToDateTime(dt.Rows[0]["Confirmation_date"]);
                            transfer.ConfirmationUserName = Utility.ConvertAnsiToUnicode(dt.Rows[0]["confirmation_name"].ToString() + " (ՊԿ՝ " + Convert.ToInt16(dt.Rows[0]["confirmation_set_number"]).ToString() + ")");
                        }
                        if (dt.Rows[0]["Confirmation_time"] != DBNull.Value)
                        {
                            transfer.ConfirmationTime = (TimeSpan)dt.Rows[0]["Confirmation_time"];
                        }

                        if (dt.Rows[0]["operation_cred_account"] != DBNull.Value)
                        {
                            transfer.CreditAccount.AccountNumber = dt.Rows[0]["operation_cred_account"].ToString();
                            transfer.CreditAccount = Account.GetSystemAccount(transfer.CreditAccount.AccountNumber);
                        }
                        transfer.Currency = dt.Rows[0]["currency"].ToString();
                        if (dt.Rows[0]["transfer_customer_number"] != DBNull.Value)
                        {
                            transfer.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["transfer_customer_number"]);
                        }
                        if (dt.Rows[0]["operation_deb_account"] != DBNull.Value)
                        {
                            transfer.DebitAccount.AccountNumber = dt.Rows[0]["operation_deb_account"].ToString();
                            transfer.DebitAccount = Account.GetSystemAccount(transfer.DebitAccount.AccountNumber);
                        }
                        if (dt.Rows[0]["deb_for_transfer_payment"] != DBNull.Value)
                        {
                            transfer.DebForTransferPayment.AccountNumber = dt.Rows[0]["deb_for_transfer_payment"].ToString();
                            transfer.DebForTransferPayment = Account.GetSystemAccount(transfer.DebForTransferPayment.AccountNumber);
                        }

                        if (dt.Rows[0]["cash_operation_date"] != DBNull.Value)
                        {
                            transfer.CashOperationDate = Convert.ToDateTime(dt.Rows[0]["cash_operation_date"]);
                        }
                        transfer.Deleted = Convert.ToByte(dt.Rows[0]["deleted"]);
                        transfer.AcbaTransfer = Convert.ToByte(dt.Rows[0]["acba_transfer"]);
                        transfer.TransferDocumentDebAccount = dt.Rows[0]["transfer_document_deb_account"].ToString();
                        transfer.TransferDocumentCredAccount = dt.Rows[0]["transfer_document_cred_account"].ToString();
                        transfer.Id = Convert.ToUInt64(dt.Rows[0]["ID"]);
                        transfer.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                        if (dt.Rows[0]["registration_time"] != DBNull.Value)
                        {
                            transfer.RegistrationTime = (TimeSpan)dt.Rows[0]["registration_time"];
                        }
                        transfer.TransferGroup = Convert.ToInt16(dt.Rows[0]["Transfer_group"]);
                        transfer.TransferGroupDescription = dt.Rows[0]["transfer_group_desc"].ToString();

                        if (dt.Rows[0]["TransferSystem"] != DBNull.Value)
                        {
                            transfer.TransferSystem = Convert.ToInt16(dt.Rows[0]["TransferSystem"]);
                            transfer.TransferSystemDescription = dt.Rows[0]["transfersystemDesc"].ToString();
                        }
                        transfer.FilialCode = (dt.Rows[0]["filial"] != DBNull.Value) ? Convert.ToUInt16(dt.Rows[0]["filial"]) : default(ushort);
                        transfer.VerifiedAml = (dt.Rows[0]["Verified_AML"] != DBNull.Value) ? Convert.ToInt16(dt.Rows[0]["Verified_AML"]) : default(short);
                        transfer.Verified = (dt.Rows[0]["Verified"] != DBNull.Value) ? Convert.ToInt16(dt.Rows[0]["Verified"]) : default(short);
                        transfer.DocumentNumber = dt.Rows[0]["document_number"].ToString();

                        if (transfer.TransferGroup == 3)
                        {
                            transfer.DescriptionForPayment = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment"].ToString());
                            transfer.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Description"].ToString());
                        }
                        else
                        {
                            transfer.DescriptionForPayment = Utility.ConvertAnsiToUnicode(dt.Rows[0]["descr_for_payment"].ToString());
                            transfer.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Description"].ToString());
                        }
                        transfer.Quality = Convert.ToByte(dt.Rows[0]["Quality"]);
                        transfer.SendOrReceived = Convert.ToByte(dt.Rows[0]["I/O"]);
                        transfer.IsCallCenter = Convert.ToByte(dt.Rows[0]["callCenter"]);
                        if (transfer.SendOrReceived == 0 && ((transfer.TransferGroup == 3 && transfer.TransferSystem != 1 && transfer.TransferSystem != 0) || transfer.TransferGroup == 4))
                            transfer.PaidAmount = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                        else
                            transfer.AmountForPayment = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);

                        if (dt.Rows[0]["receiver_account"] != DBNull.Value)
                            transfer.ReceiverAccount = dt.Rows[0]["receiver_account"].ToString();

                        if (dt.Rows[0]["sender_account"] != DBNull.Value)
                            transfer.SenderAccount = dt.Rows[0]["sender_account"].ToString();

                        if (dt.Rows[0]["add_inf"] != DBNull.Value)
                            transfer.AddInf = Utility.ConvertAnsiToUnicode(dt.Rows[0]["add_inf"].ToString());

                        if (dt.Rows[0]["sender_bank_swift"] != DBNull.Value)
                            transfer.SenderBankSwift = dt.Rows[0]["sender_bank_swift"].ToString();

                        if (dt.Rows[0]["sender_bank"] != DBNull.Value)
                            transfer.SenderBank = dt.Rows[0]["sender_bank"].ToString();

                        if (!String.IsNullOrEmpty(dt.Rows[0]["sender_reg_code"].ToString()))
                            transfer.LTACode = Convert.ToInt16(dt.Rows[0]["sender_reg_code"]);
                        if (!String.IsNullOrEmpty(dt.Rows[0]["police_code"].ToString()))
                            transfer.PoliceCode = Convert.ToInt16(dt.Rows[0]["police_code"]);

                        if (dt.Rows[0]["receiver_name"] != DBNull.Value)
                            if (transfer.TransferGroup == 3)
                                transfer.Receiver = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["receiver_name"].ToString());
                            else
                                transfer.Receiver = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_name"].ToString());

                        if (dt.Rows[0]["sender_name"] != DBNull.Value)
                            if (transfer.TransferGroup == 3)
                                transfer.Sender = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["sender_name"].ToString());
                            else
                                transfer.Sender = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_name"].ToString());

                        if (dt.Rows[0]["code_word"] != DBNull.Value)
                            transfer.Code = Utility.ConvertAnsiToUnicode(dt.Rows[0]["code_word"].ToString());

                        if (dt.Rows[0]["receiver_passport"] != DBNull.Value)
                            transfer.ReceiverPassport = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_passport"].ToString());

                        if (dt.Rows[0]["Country"] != DBNull.Value)
                            transfer.Country = dt.Rows[0]["Country"].ToString();

                        if (dt.Rows[0]["CountryName"] != DBNull.Value)
                            transfer.CountryName = dt.Rows[0]["CountryName"].ToString();


                        if (dt.Rows[0]["amount_for_system_payment"] != DBNull.Value)
                            transfer.FeeInCurrency = Convert.ToDouble(dt.Rows[0]["amount_for_system_payment"]);

                        if (dt.Rows[0]["acba_commission"] != DBNull.Value)
                            transfer.FeeAcba = Convert.ToDouble(dt.Rows[0]["acba_commission"]);

                        if (dt.Rows[0]["customer_type_description"] != DBNull.Value)
                            transfer.CustomerTypeDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["customer_type_description"].ToString());

                        if (dt.Rows[0]["sender_address"] != DBNull.Value)
                            if (transfer.TransferGroup == 3)
                                transfer.SenderAddress = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["sender_address"].ToString());
                            else
                                transfer.SenderAddress = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_address"].ToString());

                        if (dt.Rows[0]["sender_town"] != DBNull.Value)
                            transfer.SenderTown = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_town"].ToString());

                        if (dt.Rows[0]["senderCountryName"] != DBNull.Value)
                            transfer.SenderCountry = Utility.ConvertAnsiToUnicode(dt.Rows[0]["senderCountryName"].ToString());

                        if (dt.Rows[0]["sender_passport"] != DBNull.Value)
                            transfer.SenderPassport = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_passport"].ToString());

                        if (dt.Rows[0]["sender_date_of_birth"] != DBNull.Value)
                            transfer.SenderDateOfBirth = Convert.ToDateTime(dt.Rows[0]["sender_date_of_birth"]);

                        if (dt.Rows[0]["sender_email"] != DBNull.Value)
                            transfer.SenderEmail = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_email"].ToString());

                        if (dt.Rows[0]["sender_code_of_tax"] != DBNull.Value)
                            transfer.SenderCodeOfTax = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_code_of_tax"].ToString());

                        if (dt.Rows[0]["sender_phone"] != DBNull.Value)
                            transfer.SenderPhone = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_phone"].ToString());

                        if (dt.Rows[0]["sender_other_bank_account"] != DBNull.Value)
                            transfer.SenderOtherBankAccount = Utility.ConvertAnsiToUnicode(dt.Rows[0]["sender_other_bank_account"].ToString());

                        if (dt.Rows[0]["Intermidate_bank_swift"] != DBNull.Value)
                            transfer.IntermediaryBankSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Intermidate_bank_swift"].ToString());

                        if (dt.Rows[0]["Intermidate_bank"] != DBNull.Value)
                            transfer.IntermediaryBank = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Intermidate_bank"].ToString());

                        if (dt.Rows[0]["Receiver_bank_swift"] != DBNull.Value)
                            transfer.ReceiverBankSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Receiver_bank_swift"].ToString());

                        if (dt.Rows[0]["Receiver_bank"] != DBNull.Value && transfer.TransferGroup == 3)
                            transfer.ReceiverBank = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Receiver_bank"].ToString());
                        else if (dt.Rows[0]["credit_bank_code"] != DBNull.Value && transfer.TransferGroup == 1)
                            transfer.ReceiverBank = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["credit_bank_code"].ToString());

                        if (dt.Rows[0]["Receiver_bank_add_inf"] != DBNull.Value)
                            transfer.ReceiverBankAddInf = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Receiver_bank_add_inf"].ToString());

                        if (dt.Rows[0]["receiver_swift"] != DBNull.Value)
                            transfer.ReceiverSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_swift"].ToString());

                        if (dt.Rows[0]["receiver_add_inf"] != DBNull.Value)
                            if (transfer.TransferGroup == 3)
                                transfer.ReceiverAddInf = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["receiver_add_inf"].ToString());
                            else
                                transfer.ReceiverAddInf = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_add_inf"].ToString());
                        if (dt.Rows[0]["ben_our"] != DBNull.Value)
                            transfer.DetailsOfCharges = Utility.ConvertAnsiToUnicode(dt.Rows[0]["ben_our"].ToString());

                        if (dt.Rows[0]["debtor_type_description"] != DBNull.Value)
                            transfer.DebtorType = Utility.ConvertAnsiToUnicode(dt.Rows[0]["debtor_type_description"].ToString());

                        if (dt.Rows[0]["debtor_name"] != DBNull.Value)
                            transfer.Debtor = Utility.ConvertAnsiToUnicode(dt.Rows[0]["debtor_name"].ToString());

                        transfer.DebtorTypeCode = (dt.Rows[0]["debtor_type"] != DBNull.Value) ? Convert.ToUInt16(dt.Rows[0]["debtor_type"]) : default(ushort);
                        if (dt.Rows[0]["debtorDocumentNumber"] != DBNull.Value)
                            transfer.DebtorDocumentNumber = Utility.ConvertAnsiToUnicode(dt.Rows[0]["debtorDocumentNumber"].ToString());

                        if (!String.IsNullOrEmpty(dt.Rows[0]["debtor_document"].ToString()))
                            transfer.DebtorDocument = Utility.ConvertAnsiToUnicode(dt.Rows[0]["debtor_document"].ToString());

                        if (dt.Rows[0]["receiver_type_description"] != DBNull.Value)
                            transfer.ReceiverType = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_type_description"].ToString());

                        if (dt.Rows[0]["mt"] != DBNull.Value)
                            transfer.MT = Utility.ConvertAnsiToUnicode(dt.Rows[0]["mt"].ToString());

                        if (dt.Rows[0]["transfer_sourse"] != DBNull.Value)
                        {
                            transfer.TransferSource = Utility.ConvertAnsiToUnicode(dt.Rows[0]["transfer_sourse"].ToString());
                            if (dt.Rows[0]["Add_tbl_unic_number"] != DBNull.Value)
                            {
                                transfer.TransferSource = transfer.TransferSource + " (" + dt.Rows[0]["Add_tbl_unic_number"].ToString() + ")";
                            }
                        }

                        if (dt.Rows[0]["Add_tbl_unic_number"] != DBNull.Value)
                        {
                            transfer.AddTableUnicNumber = Convert.ToDouble(dt.Rows[0]["Add_tbl_unic_number"].ToString());
                        }

                        if (dt.Rows[0]["amount_in_deb_currency"] != DBNull.Value)
                            transfer.AmountInDebCurrency = Convert.ToDouble(dt.Rows[0]["amount_in_deb_currency"]);

                        if (dt.Rows[0]["sender_referance"] != DBNull.Value)
                            transfer.SenderReferance = dt.Rows[0]["sender_referance"].ToString();

                        transfer.InstantMoneyTransfer = Convert.ToByte(dt.Rows[0]["Instant_money_transfer"]);

                        if (dt.Rows[0]["Add_tbl_name"] != DBNull.Value)
                            transfer.AddTableName = dt.Rows[0]["Add_tbl_name"].ToString();
                        transfer.UnicNumber = Convert.ToInt32(dt.Rows[0]["unic_number"]);
                        transfer.RegisteredUserName = Utility.ConvertAnsiToUnicode(dt.Rows[0]["registration_name"].ToString() + " (ՊԿ՝ " + Convert.ToInt16(dt.Rows[0]["registration_set_number"]).ToString() + ")");

                        transfer.RateSell = (dt.Rows[0]["rateSell"] != DBNull.Value) ? Convert.ToDouble(dt.Rows[0]["rateSell"]) : default(double);
                        transfer.RateBuy = (dt.Rows[0]["rateBuy"] != DBNull.Value) ? Convert.ToDouble(dt.Rows[0]["rateBuy"]) : default(double);

                        if (dt.Rows[0]["police_response_details_ID"] != DBNull.Value)
                            transfer.PoliceResponseDetailsID = Convert.ToInt32(dt.Rows[0]["police_response_details_ID"]);

                        if (dt.Rows[0]["transactions_group_number"] != DBNull.Value)
                            transfer.TransactionGroupNumber = Convert.ToUInt64(dt.Rows[0]["transactions_group_number"]);

                        if (dt.Rows[0]["cash_transactions_group_number"] != DBNull.Value)
                            transfer.CashTransactionGroupNumber = Convert.ToUInt64(dt.Rows[0]["cash_transactions_group_number"]);

                        if (dt.Rows[0]["tranzit"] != DBNull.Value)
                            transfer.Transit = dt.Rows[0]["tranzit"].ToString();

                        if (dt.Rows[0]["BIK"] != DBNull.Value)
                            transfer.BIK = Utility.ConvertAnsiToUnicode(dt.Rows[0]["BIK"].ToString());

                        if (dt.Rows[0]["Corr_account"] != DBNull.Value)
                            transfer.CorrAccount = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Corr_account"].ToString());

                        if (dt.Rows[0]["KPP"] != DBNull.Value)
                            transfer.KPP = Utility.ConvertAnsiToUnicode(dt.Rows[0]["KPP"].ToString());

                        if (dt.Rows[0]["INN"] != DBNull.Value)
                            transfer.INN = Utility.ConvertAnsiToUnicode(dt.Rows[0]["INN"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_1"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR1 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_1"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_2"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR2 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_2"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_3"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR3 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_3"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_4"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR4 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_4"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_5"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR5 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_5"].ToString());

                        if (dt.Rows[0]["descr_for_payment_RUR_6"] != DBNull.Value)
                            transfer.DescriptionForPaymentRUR6 = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment_RUR_6"].ToString());

                        if (dt.Rows[0]["routing_code"] != DBNull.Value)
                            transfer.FedwireRoutingCode = dt.Rows[0]["routing_code"].ToString();

                        if (dt.Rows[0]["credit_code"] != DBNull.Value)
                        {
                            transfer.CreditCode = dt.Rows[0]["credit_code"].ToString();
                            transfer.Borrower = Utility.ConvertAnsiToUnicode(dt.Rows[0]["borrower_name"].ToString());
                            transfer.MatureTypeDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["matureDesc"].ToString());
                        }

                        if (dt.Rows[0]["MTO_Agent_Code"] != DBNull.Value)
                        {
                            transfer.MTOAgentCode = dt.Rows[0]["MTO_Agent_Code"].ToString();
                        }

                        if (dt.Rows[0]["hb_source_type"] != DBNull.Value)
                        {
                            transfer.HBSourceType = Convert.ToInt16(dt.Rows[0]["hb_source_type"].ToString());
                        }

                    }
                }

                if (user != null && transfer.TransferGroup == 3 && transfer.TransferSystem == 1)
                {
                    using (SqlConnection connDocFlow = new SqlConnection(ConfigurationManager.ConnectionStrings["DocFlowConnRO"].ToString()))
                    {
                        connDocFlow.Open();

                        string strDocFlow = @" SELECT ISNULL(R.id,0) docflowConfirmationId
                                                                    FROM (SELECT key2_num,key3_date,Tbl_request_confirmations.id,Tbl_request.request_type,Tbl_request_confirmations.step 
                                                                                    FROM Docflow.dbo.Tbl_request_link Inner join Docflow.dbo.Tbl_request_confirmations ON Tbl_request_link.request_id=Tbl_request_confirmations.request_id
                                                                                    Inner join Docflow.dbo.Tbl_request ON Tbl_request_link.request_id=Tbl_request.request_id 
                                                                                    WHERE Tbl_request.deleted is null and Tbl_request_confirmations.confirmation_date is null) R 
                                                                    Inner join Docflow.dbo.Tbl_request_confirmation_template ON Tbl_request_confirmation_template.request_type=R.request_type and Tbl_request_confirmation_template.step=R.step
                                                                    Inner join Docflow.dbo.Tbl_request_signers ON R.id=Tbl_request_signers.request_conf_id
                                                                    WHERE step_id=136 and ((signer_type=1 and signer=@setNumber) or (signer_type=2 and signer= @permissionID) ) and R.key2_num=@unic_number and R.key3_date= @registration_date  ";

                        using (SqlCommand cmdDocFlow = new SqlCommand(strDocFlow, connDocFlow))
                        {
                            cmdDocFlow.Parameters.Add("@unic_number", SqlDbType.Int).Value = transfer.UnicNumber;
                            cmdDocFlow.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = transfer.RegistrationDate.ToString("dd/MMM/yy");
                            cmdDocFlow.Parameters.Add("@setNumber", SqlDbType.Int).Value = user.userID;
                            cmdDocFlow.Parameters.Add("@permissionID", SqlDbType.Int).Value = user.userPermissionId;

                            dtDocFlow.Load(cmdDocFlow.ExecuteReader());

                            if (dtDocFlow.Rows.Count != 0)
                            {
                                transfer.DocflowConfirmationId = Convert.ToInt32(dtDocFlow.Rows[0]["docflowConfirmationId"]);
                            }
                        }
                    }
                }

            }
            return transfer;
        }


        internal static Transfer SetTransfer(DataRow row)
        {
            Transfer transfer = new Transfer();
            if (row != null)
            {
                transfer.AmlCheck = (row["aml_check"] != DBNull.Value) ? Convert.ToInt16(row["aml_check"]) : default(short);
                transfer.Amount = (row["amount"] != DBNull.Value) ? Convert.ToDouble(row["amount"]) : default(double);
                if (row["Confirmation_date"] != DBNull.Value)
                {
                    transfer.ConfirmationDate = Convert.ToDateTime(row["Confirmation_date"]);
                }
                if (row["cash_operation_date"] != DBNull.Value)
                {
                    transfer.CashOperationDate = Convert.ToDateTime(row["cash_operation_date"]);
                }
                if (row["Confirmation_time"] != DBNull.Value)
                {
                    transfer.ConfirmationTime = (TimeSpan)row["Confirmation_time"];
                }

                transfer.Currency = row["currency"].ToString();
                if (row["transfer_customer_number"] != DBNull.Value)
                {
                    transfer.CustomerNumber = Convert.ToUInt64(row["transfer_customer_number"]);
                }
                transfer.Id = Convert.ToUInt64(row["ID"]);
                transfer.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                transfer.TransferGroup = Convert.ToInt16(row["Transfer_group"]);
                transfer.TransferGroupDescription = row["transfer_group_desc"].ToString();

                if (row["TransferSystem"] != DBNull.Value)
                {
                    transfer.TransferSystem = Convert.ToInt16(row["TransferSystem"]);
                    transfer.TransferSystemDescription = row["transfersystemDesc"].ToString();
                }
                transfer.FilialCode = (row["filial"] != DBNull.Value) ? Convert.ToUInt16(row["filial"]) : default(ushort);
                transfer.VerifiedAml = (row["Verified_AML"] != DBNull.Value) ? Convert.ToInt16(row["Verified_AML"]) : default(short);
                transfer.Verified = (row["Verified"] != DBNull.Value) ? Convert.ToInt16(row["Verified"]) : default(short);
                transfer.DocumentNumber = row["document_number"].ToString();
                if (row["operation_deb_account"] != DBNull.Value)
                {
                    transfer.DebitAccount.AccountNumber = row["operation_deb_account"].ToString();
                    transfer.DebitAccount = Account.GetSystemAccount(transfer.DebitAccount.AccountNumber);
                }
                if (row["operation_cred_account"] != DBNull.Value)
                {
                    transfer.CreditAccount.AccountNumber = row["operation_cred_account"].ToString();
                    transfer.CreditAccount = Account.GetSystemAccount(transfer.CreditAccount.AccountNumber);
                }
                transfer.SendOrReceived = Convert.ToByte(row["I/O"]);
                transfer.InstantMoneyTransfer = Convert.ToByte(row["Instant_money_transfer"]);
                transfer.Quality = Convert.ToByte(row["Quality"]);
                transfer.IsCallCenter = Convert.ToByte(row["callCenter"]);
                if (row["sender_referance"] != DBNull.Value)
                    transfer.SenderReferance = row["sender_referance"].ToString();
                if (transfer.SendOrReceived == 0 && ((transfer.TransferGroup == 3 && transfer.TransferSystem != 1 && transfer.TransferSystem != 0) || transfer.TransferGroup == 4))
                    transfer.PaidAmount = Convert.ToDouble(row["amount_for_payment"]);
                else
                    transfer.AmountForPayment = Convert.ToDouble(row["amount_for_payment"]);
                transfer.UnicNumber = Convert.ToInt32(row["unic_number"]);

                if (row["receiver_name"] != DBNull.Value)
                    transfer.Receiver = Utility.ConvertAnsiToUnicode(row["receiver_name"].ToString());
                if (row["Add_tbl_name"] != DBNull.Value)
                    transfer.AddTableName = row["Add_tbl_name"].ToString();

                transfer.RateSell = (row["rate_sell"] != DBNull.Value) ? Convert.ToDouble(row["rate_sell"]) : default(double);
                transfer.RateBuy = (row["rate_buy"] != DBNull.Value) ? Convert.ToDouble(row["rate_buy"]) : default(double);
                transfer.Deleted = Convert.ToByte(row["deleted"]);
                if (row["ben_our"] != DBNull.Value)
                    transfer.DetailsOfCharges = Utility.ConvertAnsiToUnicode(row["ben_our"].ToString());


            }
            return transfer;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ փոխանցմանը կցված փաստաթուղթը (առանց scan-ի)
        /// </summary>
        /// <param name="Id">Հայտի ունիկալ համար</param>
        /// <returns></returns>
        internal static OrderAttachment GetTransferAttachmentInfo(long Id)
        {
            OrderAttachment attachments = new OrderAttachment();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@" SELECT D.Id,D.File_Name FROM Tbl_bank_mail_in T
                                                                                                    JOIN  Tbl_scaned_documents D  on D.unic_number =T.unic_number   and T.registration_date=D.registration_date  
                                                                                WHERE T.ID = @Id", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Float).Value = Id;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        attachments.Id = dt.Rows[0]["Id"].ToString();
                        attachments.FileName = dt.Rows[0]["File_Name"].ToString();
                        attachments.FileExtension = dt.Rows[0]["File_Name"].ToString().Split('.').Last();

                    }
                }


            }
            return attachments;
        }

        public static OrderAttachment GetTransferAttachment(ulong attachmentId)
        {
            OrderAttachment attachment = new OrderAttachment();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sqlString = @"SELECT D.Id,D.File_Name,D.image_content 
                                            FROM Tbl_bank_mail_in T JOIN  Tbl_scaned_documents D  on D.unic_number =T.unic_number   and T.registration_date=D.registration_date  
                                            WHERE D.ID = @Id";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = attachmentId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            attachment.Id = dr["Id"].ToString();
                            attachment.FileName = dr["File_Name"].ToString();
                            attachment.FileExtension = dr["File_Name"].ToString().Split('.').Last();
                            attachment.Attachment = (byte[])dr["image_content"];
                        }
                    }


                }

            }
            return attachment;
        }

        /// <summary>
        /// Վերադարձնում է  փոխանցման կասկածելիության պատճառի ունիկալ համարը
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        internal static List<int> GetTransferCriminalLogId(Transfer transfer)
        {
            List<int> logId = new List<int>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string str = @"select ID from Tbl_Criminal_LOG where uniq_num=@uniqeNumber and transaction_date=@transactionDate and transaction_type=3";

                using SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = str;

                cmd.Parameters.Add("@uniqeNumber", SqlDbType.Int).Value = transfer.UnicNumber;
                cmd.Parameters.Add("@transactionDate", SqlDbType.SmallDateTime).Value = transfer.RegistrationDate;

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    logId.Add(int.Parse(dr["id"].ToString()));
                }

            }
            return logId;

        }

        internal static void UpdateTransferVerifiedStatus(int userId, int verified, ulong transferId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = @"UPDATE tbl_bank_mail_in set Verified=@verifiedStatus,Verifier_set_number=@verifierSetNumber where id=@transferId ";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@verifiedStatus", SqlDbType.Int).Value = verified;
                    cmd.Parameters.Add("@verifierSetNumber", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@transferId", SqlDbType.Int).Value = transferId;

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

        }

        internal static List<short> CheckForConfirm(ulong transferID, string filialCode, short allowTransferConfirm, DateTime setDate, short setNumber)
        {
            List<short> errors = new List<short>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = "pr_check_for_confirm_transfer";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@transferID", SqlDbType.BigInt).Value = transferID;
                    cmd.Parameters.Add("@filial_code", SqlDbType.NVarChar, 5).Value = filialCode;
                    cmd.Parameters.Add("@allow_transfer_confirm", SqlDbType.TinyInt).Value = allowTransferConfirm;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = setNumber;
                    cmd.Parameters.Add("@return_error_codes", SqlDbType.TinyInt).Value = 1;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            errors.Add(short.Parse(dr["code"].ToString()));
                        }
                    }


                }

            }
            return errors;

        }


        internal static List<short> CheckForApprove(Transfer transfer, DateTime setDate, byte type)
        {
            List<short> errors = new List<short>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = "[pr_check_for_approve_transfer]";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@transferID", SqlDbType.BigInt).Value = transfer.Id;
                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@return_error_codes", SqlDbType.TinyInt).Value = 1;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = transfer.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = transfer.Amount;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = transfer.CreditAccount.AccountNumber;
                    cmd.Parameters.Add("@docflowConfirmationId", SqlDbType.Int).Value = transfer.DocflowConfirmationId;
                    cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            errors.Add(short.Parse(dr["code"].ToString()));

                        }
                    }


                }

            }
            return errors;

        }



        internal static ActionResult SaveConfirmOrder(TransferConfirmOrder transferConfirmOrder, string userName, SourceType source, short allowTransferConfirm)
        {



            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_transfer_confirm_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = transferConfirmOrder.Transfer.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = transferConfirmOrder.RegistrationDate;



                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 1).Value = allowTransferConfirm.ToString();

                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)transferConfirmOrder.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = (short)transferConfirmOrder.SubType;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = transferConfirmOrder.Id;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = transferConfirmOrder.FilialCode;
                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = transferConfirmOrder.Transfer.Id;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = transferConfirmOrder.OperationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = transferConfirmOrder.Transfer.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = transferConfirmOrder.Transfer.Amount;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    transferConfirmOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = transferConfirmOrder.Id;
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

        internal static ActionResult TransferDeleteOrder(TransferDeleteOrder transferDeleteOrder, string userName, SourceType source, ushort isCallCenter)
        {



            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_transfer_delete_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = transferDeleteOrder.Transfer.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = transferDeleteOrder.RegistrationDate;



                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 255).Value = transferDeleteOrder.Description;

                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)transferDeleteOrder.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = (short)transferDeleteOrder.SubType;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = transferDeleteOrder.Id;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = transferDeleteOrder.FilialCode;
                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = transferDeleteOrder.Transfer.Id;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = transferDeleteOrder.OperationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = transferDeleteOrder.Transfer.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = transferDeleteOrder.Transfer.Amount;
                    cmd.Parameters.Add("@isCallCenter", SqlDbType.TinyInt).Value = isCallCenter;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    transferDeleteOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = transferDeleteOrder.Id;
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

        internal static List<short> CheckForDelete(ulong transferID, string filialCode, DateTime setDate, User user, ushort isCallCenter)
        {
            List<short> errors = new List<short>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = "pr_check_for_delete_transfer";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@transferID", SqlDbType.BigInt).Value = transferID;
                    cmd.Parameters.Add("@filial_code", SqlDbType.NVarChar, 5).Value = filialCode;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = user.userID;
                    cmd.Parameters.Add("@return_error_codes", SqlDbType.TinyInt).Value = 1;
                    cmd.Parameters.Add("@isCallCenter", SqlDbType.TinyInt).Value = isCallCenter;


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            errors.Add(short.Parse(dr["code"].ToString()));
                        }
                    }


                }

            }
            return errors;

        }
        internal static bool CheckPolicePayment(long policeResponseDetailsID)
        {

            bool isPolicePaymentSended = false;
            string paymentDocumentNumber = "";
            int responseID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select dbo.fn_check_police_payment(@police_response_details_ID)  payment_document_number, response_ID FROM Tbl_Police_Response_Details WHERE id=@police_response_details_ID  ";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@police_response_details_ID", SqlDbType.BigInt).Value = policeResponseDetailsID;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            paymentDocumentNumber = dr["payment_document_number"].ToString();
                            responseID = Convert.ToInt32(dr["response_ID"]);

                        }
                        if ((String.IsNullOrEmpty(paymentDocumentNumber) || paymentDocumentNumber == "0") && responseID != -1)
                            isPolicePaymentSended = false;
                        else
                            isPolicePaymentSended = true;
                    }

                }
            }
            return isPolicePaymentSended;
        }


        internal static CBViolationPayment GetPolicePayment(long policeResponseDetailsID)
        {

            CBViolationPayment policePayment = new CBViolationPayment();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT D.task_ID, max_ordering_Id +1 max_ordering_Id, getdate() as currdate
                                        FROM Tbl_Police_Response_Details   D  
                                                    outer apply (SELECT Max(ordering_Id) as max_ordering_Id FROM Tbl_Police_Payment_Registry) O

                                        WHERE ID= @police_response_details_ID ";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@police_response_details_ID", SqlDbType.BigInt).Value = policeResponseDetailsID;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            policePayment.OrderingId = Convert.ToInt32(dr["max_ordering_Id"]);
                            policePayment.TaskId = Convert.ToInt32(dr["task_ID"]);
                            policePayment.PaymentDate = Convert.ToDateTime(dr["currdate"]);
                            policePayment.PaymentDocNumber = policeResponseDetailsID.ToString();
                        }
                    }


                }

            }
            return policePayment;
        }



        /// <summary>
        /// Վերադարձնում է  փոխանցումները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static List<Transfer> GetList(TransferFilter filter, ACBAServiceReference.User user, ulong CustomerNumber = 0)
        {
            List<Transfer> transferList = new List<Transfer>();

            DataTable dt = new DataTable();



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                List<SqlParameter> prms = new List<SqlParameter>();

                string str = "1=1 ";

                string strDocFlow = " WHERE 1=1  ";
                string strAccess = " ";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {



                    if (CustomerNumber != 0)
                    {
                        str += " and (transfer_customer_number= " + CustomerNumber.ToString() + " Or (Transfer_group=4 and    T.Quality = 0  and cash_operation_date is null )  )";
                    }

                    if (filter.DateFrom != default(DateTime))
                    {
                        str += " and T.registration_date >= '" + String.Format("{0:dd/MMM/yy}", filter.DateFrom) + "' ";

                    }

                    if (filter.DateTo != default(DateTime))
                    {
                        str += " and T.registration_date  <= '" + String.Format("{0:dd/MMM/yy}", filter.DateTo) + "' ";
                    }

                    if (filter.TransferSystem != 0)
                    {
                        str += " and T.transfersystem= " + filter.TransferSystem.ToString();
                    }

                    if (filter.SendOrReceived != 99)
                    {
                        str += " and [I/O]=" + filter.SendOrReceived.ToString();
                    }


                    if (filter.TransferGroup != 0)
                    {
                        str += " and Transfer_group= " + filter.TransferGroup.ToString();

                    }

                    if (filter.Quality != 99)
                    {
                        str += " and T.quality=" + filter.Quality.ToString();
                    }

                    if (filter.Amount != 0)
                    {
                        str += " and  amount = " + filter.Amount.ToString();


                    }

                    if (!String.IsNullOrEmpty(filter.Currency))
                    {
                        str += " and  currency='" + filter.Currency + "'";
                    }

                    if (filter.Status == 1)
                    {
                        str += " and not(confirmation_date is null) ";
                    }
                    else if (filter.Status == 2)
                    {
                        str += " and (confirmation_date is null or ([I/O]=0 and (amount - amount_for_payment)<>0 and isnull(Instant_money_transfer,0)=1) )";
                    }


                    if (filter.IsCallTranasfer != 99)
                    {
                        str += " and isnull(BC.registered_by, 0) =" + filter.IsCallTranasfer.ToString();
                    }
                    if (filter.Deleted == 1)
                    {
                        str += " and deleted=1 ";
                    }
                    else
                    {
                        str += " and deleted=0 ";
                    }


                    if (!String.IsNullOrEmpty(filter.Country))
                    {
                        str += " and T.country = '" + filter.Country + "' ";
                    }
                    if (!String.IsNullOrEmpty(filter.Filial) && filter.Filial == user.filialCode.ToString())
                    {
                        str += " and (filial =" + filter.Filial.ToString() + "   or transfer_group=4   or operation_deb_account=220001720394000 or (transfer_type=2 and confirmation_date is not null and cust.filialcode=" + filter.Filial.ToString() + " )) ";
                    }
                    else if (!String.IsNullOrEmpty(filter.Filial) && filter.Filial != user.filialCode.ToString())
                    {
                        strAccess = " LEFT JOIN  (SELECT A.arm_number FROM [tbl_all_accounts;] A LEFT JOIN Tbl_Accounts_Groups_Permissions AG ON A.account_access_group=AG.access_level  WHERE ag.group_id= " + user.AccountGroup.ToString() + "  GROUP BY arm_number) AC ON  AC.arm_number= TR.operation_deb_account  ";
                        str += " and (filial =" + filter.Filial.ToString() + "   or transfer_group=4   or operation_deb_account=220001720394000 or (transfer_type=2 and confirmation_date is not null and cust.filialcode=" + filter.Filial.ToString() + " )  and  AC.arm_number IS NOT NULL ) ";
                    }
                    else if (user.filialCode != 22000)
                    {
                        strAccess = " LEFT JOIN  (SELECT A.arm_number FROM [tbl_all_accounts;] A LEFT JOIN Tbl_Accounts_Groups_Permissions AG ON A.account_access_group=AG.access_level  WHERE ag.group_id= " + user.AccountGroup.ToString() + "  GROUP BY arm_number) AC ON  AC.arm_number= TR.operation_deb_account  ";
                        str += " and (filial =" + user.filialCode.ToString() + "   or transfer_group=4   or operation_deb_account=220001720394000 or (transfer_type=2 and confirmation_date is not null and cust.filialcode=" + user.filialCode.ToString() + " ) or AC.arm_number IS NOT NULL ) ";
                    }


                    if (filter.TransferType != 0)
                    {
                        str += "  and T.MT =" + filter.TransferType.ToString();
                    }


                    if (filter.DocumentNumber != 0)
                    {
                        str += " and document_number =" + filter.DocumentNumber.ToString();
                    }

                    if (filter.RegisteredUserID != 0)
                    {
                        str += "  and registration_set_number =" + filter.RegisteredUserID.ToString();
                    }

                    if (filter.IsPayed == 1)
                    {
                        str += " and not(cash_operation_date is null) ";
                    }
                    else if (filter.IsPayed == 2)
                    {
                        str += " and cash_operation_date is null ";
                    }

                    if (!String.IsNullOrEmpty(filter.Receiver))
                    {

                        str += " and dbo.fnc_convertAnsiToUnicode(receiver_name) like N'%" + filter.Receiver + "%' ";

                    }

                    if (filter.IsHBTransfer != 99)
                    {
                        str += " and transfer_type=" + filter.IsHBTransfer.ToString();
                    }

                    if (filter.TransferRequestStatus != 0)
                    {
                        strDocFlow = strDocFlow + " and status = " + filter.TransferRequestStatus.ToString();
                    }


                    if (filter.Session != 0)
                    {
                        str = str + " and session = " + filter.Session.ToString();
                    }

                    if (filter.TransferRequestStep != 0)
                    {
                        strDocFlow = strDocFlow + "  and Tbl_request_confirmation_template.step_id =  " + filter.TransferRequestStep.ToString();
                    }

                    if (filter.RegisteredBy != 0)
                    {
                        if (filter.RegisteredBy == 1)
                            str += " and registered_by=1";
                        else
                            str += " and registered_by=0";
                    }
                    if (filter.UETR != null)
                    {
                        str += " and  UETR = '" + filter.UETR + "'";
                    }
                    if (filter.TransferSource == 1)
                    {
                        str += " and Add_tbl_name = 'Tbl_problem_loan_taxes'";
                    }
                    cmd.CommandText = "sp_get_transfer_list";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@bank_code", SqlDbType.Int).Value = user.filialCode;
                    cmd.Parameters.Add("@wherecond", SqlDbType.NVarChar).Value = str;
                    cmd.Parameters.Add("@wherecond_df", SqlDbType.NVarChar).Value = strDocFlow;
                    cmd.Parameters.Add("@archive", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@top_for_select", SqlDbType.Int).Value = 200;
                    cmd.Parameters.Add("@joinForAccess", SqlDbType.NVarChar).Value = strAccess;
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        while (dr.Read())
                        {

                            Transfer transfer = new Transfer();

                            transfer.AmlCheck = (dr["aml_check"] != DBNull.Value) ? Convert.ToInt16(dr["aml_check"]) : default(short);
                            transfer.Amount = (dr["amount"] != DBNull.Value) ? Convert.ToDouble(dr["amount"]) : default(double);
                            if (dr["Confirmation_date"] != DBNull.Value)
                            {
                                transfer.ConfirmationDate = Convert.ToDateTime(dr["Confirmation_date"]);
                            }
                            if (dr["Confirmation_time"] != DBNull.Value)
                            {
                                transfer.ConfirmationTime = (TimeSpan)dr["Confirmation_time"];
                            }

                            transfer.Currency = dr["currency"].ToString();
                            if (dr["transfer_customer_number"] != DBNull.Value)
                            {
                                transfer.CustomerNumber = Convert.ToUInt64(dr["transfer_customer_number"]);
                            }
                            transfer.Id = Convert.ToUInt64(dr["ID"]);
                            transfer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            transfer.TransferGroup = Convert.ToInt16(dr["Transfer_group"]);
                            transfer.TransferGroupDescription = Utility.ConvertAnsiToUnicode(dr["transfer_group_desc"].ToString());

                            if (dr["transfersystem"] != DBNull.Value)
                            {
                                transfer.TransferSystemDescription = Utility.ConvertAnsiToUnicode(dr["transfersystem"].ToString());
                            }
                            if (dr["call_set_number"] != DBNull.Value)
                            {
                                transfer.CallRegSetNumber = Convert.ToInt32(dr["call_set_number"]);
                            }


                            transfer.FilialCode = (dr["filial"] != DBNull.Value) ? Convert.ToUInt16(dr["filial"]) : default(ushort);
                            transfer.VerifiedAml = (dr["Verified_AML"] != DBNull.Value) ? Convert.ToInt16(dr["Verified_AML"]) : default(short);
                            transfer.Verified = (dr["Ver"] != DBNull.Value) ? Convert.ToInt16(dr["Ver"]) : default(short);
                            transfer.DocumentNumber = dr["document_number"].ToString();
                            transfer.SendOrReceived = Convert.ToByte(dr["I/O"]);
                            transfer.InstantMoneyTransfer = Convert.ToByte(dr["Instant_money_transfer"]);
                            if (dr["acba_commission"] != DBNull.Value)
                                transfer.FeeAcba = Convert.ToDouble(dr["acba_commission"]);

                            if (dr["amount_for_system_payment"] != DBNull.Value)
                                transfer.FeeInCurrency = Convert.ToDouble(dr["amount_for_system_payment"]);

                            transfer.Quality = Convert.ToByte(dr["Quality"]);
                            transfer.IsCallCenter = Convert.ToByte(dr["call_receiver"]);

                            if (transfer.SendOrReceived == 0 && ((transfer.TransferGroup == 3 && transfer.TransferSystem != 1 && transfer.TransferSystem != 0) || transfer.TransferGroup == 4))
                                transfer.PaidAmount = Convert.ToDouble(dr["amount_for_payment"]);
                            else
                                transfer.AmountForPayment = Convert.ToDouble(dr["amount_for_payment"]);
                            transfer.UnicNumber = Convert.ToInt32(dr["unic_number"]);

                            if (dr["UETR"] != DBNull.Value)
                            {
                                transfer.UETR = dr["UETR"].ToString();
                            }
                            if (dr["MTO_Agent_Code"] != DBNull.Value)
                            {
                                transfer.MTOAgentCode = dr["MTO_Agent_Code"].ToString();
                            }

                            if (dr["hb_source_type"] != DBNull.Value)
                            {
                                transfer.HBSourceType = Convert.ToInt16(dr["hb_source_type"].ToString());
                            }

                            transferList.Add(transfer);
                        }


                        if (dr.NextResult())
                        {
                            if (dr.Read() && transferList.Count != 0)
                                transferList.First().ListCount = Convert.ToUInt32(dr["qanak"]);
                        }

                    }

                }




            }



            return transferList;
        }



        internal static ActionResult TransferApproveOrder(TransferApproveOrder transferApproveOrder, string userName, SourceType source)
        {



            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_transfer_aprove_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = transferApproveOrder.Transfer.CustomerNumber;
                    cmd.Parameters.Add("@documentSubType", SqlDbType.Float).Value = transferApproveOrder.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = transferApproveOrder.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = transferApproveOrder.Id;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = transferApproveOrder.FilialCode;
                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = transferApproveOrder.Transfer.Id;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = transferApproveOrder.OperationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = transferApproveOrder.Transfer.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = transferApproveOrder.Transfer.Amount;
                    cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar, 20).Value = transferApproveOrder.Transfer.CreditAccount.AccountNumber;
                    cmd.Parameters.Add("@receiverBankSwiftCode", SqlDbType.NVarChar, 15).Value = transferApproveOrder.Transfer.ReceiverBankSwift;
                    cmd.Parameters.Add("@receiverBank", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.ReceiverBank;
                    cmd.Parameters.Add("@receiverBankAddInf", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.ReceiverBankAddInf;
                    cmd.Parameters.Add("@intermediaryBankSwift", SqlDbType.NVarChar, 15).Value = transferApproveOrder.Transfer.IntermediaryBankSwift;
                    cmd.Parameters.Add("@intermediaryBank", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.IntermediaryBank;
                    cmd.Parameters.Add("@descrForPayment", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.DescriptionForPayment;
                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.Receiver;
                    cmd.Parameters.Add("@receiverSwiftCode", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.ReceiverSwift;
                    cmd.Parameters.Add("@receiverAccount", SqlDbType.NVarChar, 255).Value = transferApproveOrder.Transfer.ReceiverAccount;
                    cmd.Parameters.Add("@valueDate", SqlDbType.SmallDateTime).Value = transferApproveOrder.ValueDate;

                    cmd.Parameters.Add("@uip", SqlDbType.NVarChar, 25).Value = transferApproveOrder.UIP;

                    if (!String.IsNullOrEmpty(transferApproveOrder.AccountInIntermediaryBank))
                        cmd.Parameters.Add("@AccountInIntermediaryBank", SqlDbType.NVarChar, 50).Value = transferApproveOrder.AccountInIntermediaryBank;


                    if (!String.IsNullOrEmpty(transferApproveOrder.TransactionType26))
                        cmd.Parameters.Add("@transactionType26", SqlDbType.NVarChar, 3).Value = transferApproveOrder.TransactionType26;
                    if (!String.IsNullOrEmpty(transferApproveOrder.AccountAbility77B))
                        cmd.Parameters.Add("@accountAbility77B", SqlDbType.NVarChar, 120).Value = transferApproveOrder.AccountAbility77B;

                    if (transferApproveOrder.Transfer.Country == "840" && transferApproveOrder.Transfer.Currency == "USD" && !String.IsNullOrEmpty(transferApproveOrder.Transfer.FedwireRoutingCode))
                    {
                        cmd.Parameters.Add("@routing_code", SqlDbType.NVarChar, 50).Value = transferApproveOrder.Transfer.FedwireRoutingCode;
                    }
                    if (transferApproveOrder.SubType == 1)
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = transferApproveOrder.Transfer.VOCode;
                    else
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = transferApproveOrder.Description;

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    transferApproveOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = transferApproveOrder.Id;
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


        /// <summary>
        /// Վերադարձնում է  փոխանցուման այն դաշտերը, որոնք կարող են փոխված լինել հաստատելուց
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static Transfer GetApprovedTransfer(Transfer transfer)
        {

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string str = @"SELECT *
                                    FROM Tbl_swift_transfers_approve_data
                                    Where  transfer_id=@id ";

                using SqlCommand cmd = new SqlCommand(str, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = transfer.Id;

                dt.Load(cmd.ExecuteReader());

                if (dt.Rows.Count != 0)
                {
                    if (dt.Rows[0]["receiver_account"] != DBNull.Value)
                        transfer.ReceiverAccount = dt.Rows[0]["receiver_account"].ToString();

                    if (dt.Rows[0]["receiver_name"] != DBNull.Value)
                        transfer.Receiver = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["receiver_name"].ToString());

                    if (dt.Rows[0]["descr_for_payment"] != DBNull.Value)
                        transfer.DescriptionForPayment = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["descr_for_payment"].ToString());

                    if (dt.Rows[0]["Intermidate_bank_swift"] != DBNull.Value)
                        transfer.IntermediaryBankSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Intermidate_bank_swift"].ToString());

                    if (dt.Rows[0]["Intermidate_bank"] != DBNull.Value)
                        transfer.IntermediaryBank = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Intermidate_bank"].ToString());

                    if (dt.Rows[0]["Receiver_bank_swift"] != DBNull.Value)
                        transfer.ReceiverBankSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Receiver_bank_swift"].ToString());

                    if (dt.Rows[0]["Receiver_bank"] != DBNull.Value)
                        transfer.ReceiverBank = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Receiver_bank"].ToString());

                    if (dt.Rows[0]["Receiver_bank_add_inf"] != DBNull.Value)
                        transfer.ReceiverBankAddInf = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[0]["Receiver_bank_add_inf"].ToString());

                    if (dt.Rows[0]["receiver_swift_code"] != DBNull.Value)
                        transfer.ReceiverSwift = Utility.ConvertAnsiToUnicode(dt.Rows[0]["receiver_swift_code"].ToString());

                }

            }
            return transfer;
        }


        internal static void UpdateTransferAfterARUSRequest(ulong id, string URN)
        {

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            conn.Open();

            string str = @"UPDATE Tbl_bank_mail_in_reestr 
                               SET code_word = @URN
                               WHERE registration_date = (select registration_date from Tbl_BAnk_mail_in where id = @id) 
                               AND unic_number = (select unic_number from Tbl_BAnk_mail_in where id = @id)";

            using (SqlCommand cmd = new SqlCommand(str, conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@URN", SqlDbType.NVarChar).Value = URN;

                cmd.ExecuteScalar();

            }
        }

        /// <summary>
        /// Վերադարձնում է  փոխանցումը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static ulong GetTransferIdFromTransferByCallId(ulong transferByCallId)
        {
            DataTable dt = new DataTable();

            ulong transferId;

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();

            string str = @"SELECT   B.id as transferId
                                      FROM Tbl_transfers_by_call  C join tbl_bank_mail_in  B on C.[transfer_registration_date]= B.registration_date and C.[transfer_unic_number]=B.unic_number
                                       where C.id= @id ";

            using SqlCommand cmd = new SqlCommand(str, conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = transferByCallId;
            dt.Load(cmd.ExecuteReader());

            transferId = (dt.Rows[0]["transferId"] != DBNull.Value) ? Convert.ToUInt64(dt.Rows[0]["transferId"]) : 0;

            return transferId;
        }

        /// <summary>
        /// Վերադարձնում է  փոխանցումը
        /// </summary>
        internal static ulong GetTransferIdByDocId(long docID)
        {
            ulong transferId;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string str = @"Select id from tbl_bank_mail_in where Add_tbl_unic_number = @docID ";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;
                    transferId = Convert.ToUInt64(cmd.ExecuteScalar());

                    return transferId;
                }
            }
        }


    }
}
