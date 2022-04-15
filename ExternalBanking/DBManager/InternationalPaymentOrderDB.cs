using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal static class InternationalPaymentOrderDB
    {

        internal static ActionResult Save(InternationalPaymentOrder order, string userName, SourceType source)
        {

            string descriptionForPayment = "";
            string receiverBankAddInf = "";
            if (order.Currency != "RUR")
            {
                receiverBankAddInf = order.ReceiverBankAddInf;
                descriptionForPayment = order.DescriptionForPayment;
            }
            else
            {
                receiverBankAddInf = "БИК-" + order.BIK + ", К/с-" + order.CorrAccount;
                if (order.KPP != "")
                {
                    receiverBankAddInf = receiverBankAddInf + ", КПП-" + order.KPP;
                }
                if (order.DescriptionForPaymentRUR1 != "Другое")
                {
                    descriptionForPayment = order.DescriptionForPaymentRUR1 + " ";
                }
                descriptionForPayment = descriptionForPayment + order.DescriptionForPaymentRUR2;
                if (order.DescriptionForPaymentRUR1 != "Материальная помощь" && order.DescriptionForPaymentRUR1 != "Другое")
                {
                    descriptionForPayment = descriptionForPayment + " " + order.DescriptionForPaymentRUR3 + " " + order.DescriptionForPaymentRUR4 + " " + order.DescriptionForPaymentRUR5;
                    if (order.DescriptionForPaymentRUR5 == "с НДС")
                    {
                        descriptionForPayment = descriptionForPayment + " " + order.DescriptionForPaymentRUR6 + " RUB";
                    }
                }

            }

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_transfer_international";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 100).Value = order.Sender;
                    cmd.Parameters.Add("@sender_address", SqlDbType.NVarChar, 100).Value = order.SenderAddress;
                    cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = order.ConvertationRate;
                    cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = order.ConvertationRate1;

                    if (!string.IsNullOrEmpty(order.SwiftPurposeCode))
                    {
                        cmd.Parameters.Add("@SwiftPurposeCode", SqlDbType.NVarChar, 3).Value = order.SwiftPurposeCode;
                        cmd.Parameters.Add("@PurposeCodeOther", SqlDbType.NVarChar, 3).Value = order.PurposeCodeOther;
                    }



                    if (order.SenderType == 6)
                    {
                        cmd.Parameters.Add("@sender_passport", SqlDbType.NVarChar, 50).Value = order.SenderPassport;
                        cmd.Parameters.Add("@sender_date_of_birth", SqlDbType.SmallDateTime).Value = order.SenderDateOfBirth;
                    }
                    else
                    {
                        cmd.Parameters.Add("@sender_code_of_tax", SqlDbType.NVarChar, 8).Value = order.SenderCodeOfTax;
                    }
                    cmd.Parameters.Add("@sender_email", SqlDbType.NVarChar, 50).Value = order.SenderEmail;

                    cmd.Parameters.Add("@sender_phone", SqlDbType.NVarChar, 50).Value = order.SenderPhone;
                    if (order.SenderOtherBankAccount != "" && order.SenderOtherBankAccount != null)
                        cmd.Parameters.Add("@sender_other_bank_account", SqlDbType.NVarChar, 50).Value = order.SenderOtherBankAccount;

                    if (order.Currency != "RUR")
                    {
                        if (order.IntermediaryBankSwift != "" && order.IntermediaryBankSwift != null)
                        {
                            cmd.Parameters.Add("@intermediary_bank_swift", SqlDbType.NVarChar, 11).Value = order.IntermediaryBankSwift;
                            cmd.Parameters.Add("@intermediary_bank", SqlDbType.NVarChar, 255).Value = order.IntermediaryBank;
                        }
                        cmd.Parameters.Add("@Receiver_bank_swift", SqlDbType.NVarChar, 11).Value = order.ReceiverBankSwift;
                        cmd.Parameters.Add("@sender_town", SqlDbType.NVarChar, 50).Value = order.SenderTown;
                        cmd.Parameters.Add("@sender_country", SqlDbType.NVarChar, 50).Value = order.SenderCountry;

                    }
                    else
                    {
                        cmd.Parameters.Add("@BIK", SqlDbType.NVarChar, 9).Value = order.BIK;
                        cmd.Parameters.Add("@Corr_account", SqlDbType.NVarChar, 20).Value = order.CorrAccount;
                        cmd.Parameters.Add("@KPP", SqlDbType.NVarChar, 9).Value = order.KPP;
                        if (order.INN != "" && order.INN != null)
                        {
                            cmd.Parameters.Add("@INN", SqlDbType.NVarChar, 12).Value = order.INN;
                        }
                        cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = order.ReceiverType;
                        cmd.Parameters.Add("@descr_for_payment_RUR_1", SqlDbType.NVarChar, 25).Value = order.DescriptionForPaymentRUR1;
                        cmd.Parameters.Add("@descr_for_payment_RUR_2", SqlDbType.NVarChar, 125).Value = order.DescriptionForPaymentRUR2;
                        cmd.Parameters.Add("@descr_for_payment_RUR_3", SqlDbType.NVarChar, 25).Value = order.DescriptionForPaymentRUR3;
                        cmd.Parameters.Add("@descr_for_payment_RUR_4", SqlDbType.NVarChar, 50).Value = order.DescriptionForPaymentRUR4;
                        if (order.DescriptionForPaymentRUR5 != "" && order.DescriptionForPaymentRUR5 != null)
                        {
                            cmd.Parameters.Add("@descr_for_payment_RUR_5", SqlDbType.NVarChar, 00).Value = order.DescriptionForPaymentRUR5;
                        }
                        cmd.Parameters.Add("@descr_for_payment_RUR_6", SqlDbType.NVarChar, 10).Value = order.DescriptionForPaymentRUR6;

                    }

                    cmd.Parameters.Add("@Receiver_bank", SqlDbType.NVarChar, 255).Value = order.ReceiverBank;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    if (order.ReceiverBankAddInf != "" && order.ReceiverBankAddInf != null)
                    {
                        cmd.Parameters.Add("@Receiver_bank_add_inf", SqlDbType.NVarChar, 210).Value = receiverBankAddInf;
                    }

                    cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 50).Value = order.Country;
                    cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 50).Value = order.ReceiverAccount.AccountNumber;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 255).Value = order.Receiver;
                    if (order.ReceiverAddInf != "" && order.ReceiverAddInf != null)
                    {
                        cmd.Parameters.Add("@receiver_add_inf", SqlDbType.NVarChar, 200).Value = order.ReceiverAddInf;
                    }


                    cmd.Parameters.Add("@descr_for_payment", SqlDbType.NVarChar, 150).Value = descriptionForPayment;
                    cmd.Parameters.Add("@details_of_charges", SqlDbType.NVarChar, 50).Value = order.DetailsOfCharges;
                    cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;

                    cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.NVarChar, 50).Value = order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@fee_currency2", SqlDbType.NVarChar, 20).Value = order.FeeAccount.Currency;
                    cmd.Parameters.Add("@fee_type2", SqlDbType.TinyInt).Value = order.Type == OrderType.CashInternationalTransfer ? 5 : 20;

                    if (order.CardFee != 0)
                    {
                        cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.CardFee;
                        cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = order.DebitAccount.Currency;
                        cmd.Parameters.Add("@fee_type", SqlDbType.TinyInt).Value = 7;
                    }
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = (short)order.SubType;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@urgent", SqlDbType.Bit).Value = Convert.ToByte(order.UrgentSign);
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = order.Description;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    if (order.Country == "840" && order.Currency == "USD" && !String.IsNullOrEmpty(order.FedwireRoutingCode))
                    {
                        cmd.Parameters.Add("@routing_code", SqlDbType.NVarChar, 50).Value = order.FedwireRoutingCode;
                    }

                    cmd.Parameters.Add("@reason_type", SqlDbType.SmallInt).Value = order.ReasonId;
                    if (!string.IsNullOrEmpty(order.ReasonIdDescription))
                    {
                        cmd.Parameters.Add("@reason_type_description", SqlDbType.NVarChar, 350).Value = order.ReasonIdDescription;
                    }
                    else
                    {
                        cmd.Parameters.Add("@reason_type_description", SqlDbType.NVarChar, 350).Value = DBNull.Value;
                    }

                    cmd.Parameters.Add("@mt", SqlDbType.NVarChar, 3).Value = order.MT;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    cmd.Parameters.Add("@receiver_swift", SqlDbType.NVarChar, 11).Value = order.ReceiverSwift;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });



                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    string msg = Convert.ToString(cmd.Parameters["@msg"].Value);

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

        internal static InternationalPaymentOrder Get(InternationalPaymentOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT rs.description as ReasonIdDescription,D.*, C.CountryName CountryName,  C1.CountryName SenderCountryName,   type_of_client
                                                         FROM Tbl_Hb_Documents D  
			                                                        LEFT JOIN Tbl_Countries  C  on D.country=C.CountryCodeN
			                                                        LEFT JOIN Tbl_Countries  C1  on D.sender_country=C1.CountryCodeN
                                                                    LEFT JOIN Tbl_Customers  Cust  on D.customer_number=Cust.customer_number
                                                                    LEFT JOIN tbl_type_of_card_debit_reasons rs on rs.type= d.reason_type 
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

                            if (dr["reason_type"] != DBNull.Value && Convert.ToInt32(dr["reason_type"]) != 0)
                            {
                                order.ReasonId = Convert.ToInt32(dr["reason_type"]);
                                order.ReasonIdDescription = Utility.ConvertAnsiToUnicode(dr["ReasonIdDescription"].ToString());
                                if (order.ReasonId == 99)
                                {
                                    order.ReasonIdDescription = Utility.ConvertAnsiToUnicode(dr["reason_type_description"].ToString());
                                }
                            }


                            order.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                order.ReceiverAccount.AccountNumber = dr["credit_account"].ToString();
                            }

                            if (dr["receiver_name"] != DBNull.Value)
                                order.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                            if (dr["credit_bank_code"] != DBNull.Value)
                                order.ReceiverBankCode = Convert.ToInt32(dr["credit_bank_code"]);

                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();

                            order.OrderNumber = dr["document_number"].ToString();

                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            if (dr["rate_sell_buy"] != DBNull.Value)
                                order.ConvertationRate = Convert.ToDouble(dr["rate_sell_buy"]);

                            if (dr["rate_sell_buy_cross"] != DBNull.Value)
                                order.ConvertationRate1 = Convert.ToDouble(dr["rate_sell_buy_cross"]);

                            //if (dr["amount_for_payment"] != DBNull.Value)
                            //    order.TransferFee = Convert.ToDouble(dr["amount_for_payment"]);

                            //if (dr["deb_for_transfer_payment"] != DBNull.Value)
                            //    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dr["deb_for_transfer_payment"]));

                            if (dr["urgent"] != DBNull.Value)
                                order.UrgentSign = Convert.ToBoolean(dr["urgent"]);

                            //if (dr["FeeAmount"] != DBNull.Value && Convert.ToInt16(dr["FeeType"]) == 1)
                            //{
                            //    order.CardFee = Convert.ToDouble(dr["FeeAmount"]);
                            //    order.CardFeeCurrency = dr["FeeCurrency"].ToString();
                            //}

                            if (dr["sender_name"] != DBNull.Value)
                                order.Sender = Utility.ConvertAnsiToUnicode(dr["sender_name"].ToString());

                            if (dr["sender_address"] != DBNull.Value)
                                order.SenderAddress = Utility.ConvertAnsiToUnicode(dr["sender_address"].ToString());

                            if (dr["sender_town"] != DBNull.Value)
                                order.SenderTown = Utility.ConvertAnsiToUnicode(dr["sender_town"].ToString());

                            if (dr["sender_country"] != DBNull.Value)
                                order.SenderCountry = Utility.ConvertAnsiToUnicode(dr["sender_country"].ToString());

                            if (dr["sender_passport"] != DBNull.Value)
                                order.SenderPassport = Utility.ConvertAnsiToUnicode(dr["sender_passport"].ToString());

                            if (dr["sender_date_of_birth"] != DBNull.Value)
                                order.SenderDateOfBirth = Convert.ToDateTime(dr["sender_date_of_birth"]);

                            if (dr["sender_email"] != DBNull.Value)
                                order.SenderEmail = Utility.ConvertAnsiToUnicode(dr["sender_email"].ToString());

                            if (dr["sender_code_of_tax"] != DBNull.Value)
                                order.SenderCodeOfTax = Utility.ConvertAnsiToUnicode(dr["sender_code_of_tax"].ToString());

                            if (dr["sender_phone"] != DBNull.Value)
                                order.SenderPhone = Utility.ConvertAnsiToUnicode(dr["sender_phone"].ToString());

                            if (dr["sender_other_bank_account"] != DBNull.Value)
                                order.SenderOtherBankAccount = Utility.ConvertAnsiToUnicode(dr["sender_other_bank_account"].ToString());

                            if (dr["intermediary_bank_swift"] != DBNull.Value)
                                order.IntermediaryBankSwift = Utility.ConvertAnsiToUnicode(dr["intermediary_bank_swift"].ToString());

                            if (dr["intermediary_bank"] != DBNull.Value)
                                order.IntermediaryBank = Utility.ConvertAnsiToUnicode(dr["intermediary_bank"].ToString());

                            if (dr["Receiver_bank_swift"] != DBNull.Value)
                                order.ReceiverBankSwift = Utility.ConvertAnsiToUnicode(dr["Receiver_bank_swift"].ToString());

                            if (dr["Receiver_bank"] != DBNull.Value)
                                order.ReceiverBank = Utility.ConvertAnsiToUnicode(dr["Receiver_bank"].ToString());

                            if (dr["Receiver_bank_add_inf"] != DBNull.Value)
                                order.ReceiverBankAddInf = Utility.ConvertAnsiToUnicode(dr["Receiver_bank_add_inf"].ToString());

                            if (dr["BIK"] != DBNull.Value)
                                order.BIK = Utility.ConvertAnsiToUnicode(dr["BIK"].ToString());

                            if (dr["Corr_account"] != DBNull.Value)
                                order.CorrAccount = Utility.ConvertAnsiToUnicode(dr["Corr_account"].ToString());

                            if (dr["KPP"] != DBNull.Value)
                                order.KPP = Utility.ConvertAnsiToUnicode(dr["KPP"].ToString());

                            if (dr["descr_for_payment_RUR_1"] != DBNull.Value)
                                order.DescriptionForPaymentRUR1 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_1"].ToString());

                            if (dr["descr_for_payment_RUR_2"] != DBNull.Value)
                                order.DescriptionForPaymentRUR2 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_2"].ToString());

                            if (dr["descr_for_payment_RUR_3"] != DBNull.Value)
                                order.DescriptionForPaymentRUR3 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_3"].ToString());

                            if (dr["descr_for_payment_RUR_4"] != DBNull.Value)
                                order.DescriptionForPaymentRUR4 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_4"].ToString());

                            if (dr["descr_for_payment_RUR_5"] != DBNull.Value)
                                order.DescriptionForPaymentRUR5 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_5"].ToString());

                            if (dr["descr_for_payment_RUR_6"] != DBNull.Value)
                                order.DescriptionForPaymentRUR6 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_6"].ToString());

                            if (dr["receiver_add_inf"] != DBNull.Value)
                                order.ReceiverAddInf = Utility.ConvertAnsiToUnicode(dr["receiver_add_inf"].ToString());

                            if (dr["Receiver_type"] != DBNull.Value)
                                order.ReceiverType = Convert.ToByte((dr["Receiver_type"]));

                            if (dr["INN"] != DBNull.Value)
                                order.INN = Utility.ConvertAnsiToUnicode(dr["INN"].ToString());

                            if (dr["descr_for_payment"] != DBNull.Value)
                                order.DescriptionForPayment = Utility.ConvertAnsiToUnicode(dr["descr_for_payment"].ToString());

                            if (dr["Country"] != DBNull.Value)
                                order.Country = dr["Country"].ToString();

                            if (dr["details_of_charges"] != DBNull.Value)
                                order.DetailsOfCharges = Utility.ConvertAnsiToUnicode(dr["details_of_charges"].ToString());

                            if (dr["CountryName"] != DBNull.Value)
                                order.CountryName = dr["CountryName"].ToString();

                            if (dr["SenderCountryName"] != DBNull.Value)
                                order.SenderCountryName = dr["SenderCountryName"].ToString();

                            if (dr["routing_code"] != DBNull.Value)
                                order.FedwireRoutingCode = dr["routing_code"].ToString();

                            if (dr["type_of_client"] != DBNull.Value)
                                order.SenderType = Convert.ToInt16(dr["type_of_client"]);

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);


                            order.TransferAdditionalData = TransferAdditionalDataDB.GetTransferAdditionalData(order.Id);
                        }
                        else
                        {
                            order = null;
                        }
                    }
                }
            }
            return order;
        }


        /// <summary>
        /// Միջազգային վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="internationalPaymentOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SaveInternationalPaymentOrderDetails(InternationalPaymentOrder internationalPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_payment_international";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@senderName", SqlDbType.NVarChar, 100).Value = (object)internationalPaymentOrder.Sender ?? DBNull.Value;
                    cmd.Parameters.Add("@senderAddress", SqlDbType.NVarChar, 100).Value = (object)internationalPaymentOrder.SenderAddress ?? DBNull.Value;

                    if (internationalPaymentOrder.SenderType == 6)
                    {
                        cmd.Parameters.Add("@senderPassport", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderPassport ?? DBNull.Value;
                        cmd.Parameters.Add("@senderDateOfBirth", SqlDbType.DateTime).Value = (object)internationalPaymentOrder.SenderDateOfBirth ?? DBNull.Value;
                        cmd.Parameters.Add("@senderEmail", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderEmail ?? DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@senderCodeOfTax", SqlDbType.NVarChar, 8).Value = (object)internationalPaymentOrder.SenderCodeOfTax ?? DBNull.Value;
                    }

                    cmd.Parameters.Add("@senderPhone", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderPhone ?? DBNull.Value;
                    cmd.Parameters.Add("@senderType", SqlDbType.SmallInt).Value = (object)internationalPaymentOrder.SenderType ?? DBNull.Value;

                    if (internationalPaymentOrder.SenderOtherBankAccount != "" && internationalPaymentOrder.SenderOtherBankAccount != null)
                    {
                        cmd.Parameters.Add("@senderOtherBankAccount", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderOtherBankAccount ?? DBNull.Value;
                    }

                    if (internationalPaymentOrder.Currency != "RUR")
                    {
                        if (internationalPaymentOrder.IntermediaryBankSwift != "" && internationalPaymentOrder.IntermediaryBankSwift != null)
                        {
                            cmd.Parameters.Add("@intermediaryBankSwift", SqlDbType.NVarChar, 11).Value = (object)internationalPaymentOrder.IntermediaryBankSwift ?? DBNull.Value;
                            cmd.Parameters.Add("@intermediaryBank", SqlDbType.NVarChar, 255).Value = (object)internationalPaymentOrder.IntermediaryBank ?? DBNull.Value;
                        }
                        cmd.Parameters.Add("@receiverBankSwift", SqlDbType.NVarChar, 11).Value = (object)internationalPaymentOrder.ReceiverBankSwift ?? DBNull.Value;
                        cmd.Parameters.Add("@senderTown", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderTown ?? DBNull.Value;
                        cmd.Parameters.Add("@senderCountry", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.SenderCountry ?? DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@BIK", SqlDbType.NVarChar, 9).Value = (object)internationalPaymentOrder.BIK ?? DBNull.Value;
                        cmd.Parameters.Add("@corrAccount", SqlDbType.NVarChar, 20).Value = (object)internationalPaymentOrder.CorrAccount ?? DBNull.Value;
                        cmd.Parameters.Add("@KPP", SqlDbType.NVarChar, 9).Value = (object)internationalPaymentOrder.KPP ?? DBNull.Value;
                        if (internationalPaymentOrder.INN != "" && internationalPaymentOrder.INN != null)
                        {
                            cmd.Parameters.Add("@INN", SqlDbType.NVarChar, 12).Value = (object)internationalPaymentOrder.INN ?? DBNull.Value;
                        }
                        cmd.Parameters.Add("@receiverType", SqlDbType.SmallInt).Value = (object)internationalPaymentOrder.ReceiverType ?? DBNull.Value;
                        cmd.Parameters.Add("@descrForPaymentRUR1", SqlDbType.NVarChar, 25).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR1 ?? DBNull.Value;
                        cmd.Parameters.Add("@descrForPaymentRUR2", SqlDbType.NVarChar, 125).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR2 ?? DBNull.Value;
                        cmd.Parameters.Add("@descrForPaymentRUR3", SqlDbType.NVarChar, 25).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR3 ?? DBNull.Value;
                        cmd.Parameters.Add("@descrForPaymentRUR4", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR4 ?? DBNull.Value;
                        if (internationalPaymentOrder.DescriptionForPaymentRUR5 != "" && internationalPaymentOrder.DescriptionForPaymentRUR5 != null)
                        {
                            cmd.Parameters.Add("@descrForPaymentRUR5", SqlDbType.NVarChar, 20).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR5 ?? DBNull.Value;
                        }
                        cmd.Parameters.Add("@descrForPaymentRUR6", SqlDbType.NVarChar, 10).Value = (object)internationalPaymentOrder.DescriptionForPaymentRUR6 ?? DBNull.Value;
                    }

                    cmd.Parameters.Add("@receiverBank", SqlDbType.NVarChar, 255).Value = (object)internationalPaymentOrder.ReceiverBank ?? DBNull.Value;
                    if (internationalPaymentOrder.ReceiverBankAddInf != "" && internationalPaymentOrder.ReceiverBankAddInf != null)
                    {
                        cmd.Parameters.Add("@receiverBankAddInf", SqlDbType.NVarChar, 200).Value = (object)internationalPaymentOrder.ReceiverBankAddInf ?? DBNull.Value;
                    }
                    if (internationalPaymentOrder.ReceiverAddInf != "" && internationalPaymentOrder.ReceiverAddInf != null)
                    {
                        cmd.Parameters.Add("@receiverAddInf", SqlDbType.NVarChar, 200).Value = (object)internationalPaymentOrder.ReceiverAddInf ?? DBNull.Value;
                    }

                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 250).Value = (object)internationalPaymentOrder.Receiver ?? DBNull.Value;


                    cmd.Parameters.Add("@descrForPayment", SqlDbType.NVarChar, 150).Value = (object)internationalPaymentOrder.DescriptionForPayment ?? DBNull.Value;

                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.Country ?? DBNull.Value;
                    cmd.Parameters.Add("@detailsOfCharges", SqlDbType.NVarChar, 50).Value = (object)internationalPaymentOrder.DetailsOfCharges ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static short IsCustomerSwiftTransferVerified(ulong customerNummber, SourceType source, string swiftCode = "", string receiverAaccount = "")
        {
            short isVerified = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("  exec  pr_check_customer_swift_transfer_verified @customer_number ,	@swift_code,	@receiver_account  ", conn);
                if (swiftCode == null)
                    swiftCode = "";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = Convert.ToUInt64(customerNummber);
                cmd.Parameters.Add("@swift_code", SqlDbType.NVarChar, 50).Value = swiftCode;
                cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 50).Value = receiverAaccount;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (source == SourceType.Bank)
                        isVerified = short.Parse(dr["code_front"].ToString());
                    else
                        isVerified = short.Parse(dr["code_HB"].ToString());

                }

            }

            return isVerified;
        }

        internal static short CheckIBANCodeLength(string accountNumber)
        {
            short isVerified = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("  SELECT dbo.fn_check_IBAN_code_length( @accountNumber) isVerified ", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@accountNumber", SqlDbType.NVarChar, 255).Value = accountNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {

                    isVerified = short.Parse(dr["isVerified"].ToString());

                }

            }

            return isVerified;
        }

        internal static string GetInternationalTransferSentTime(int docID)
        {
            DateTime sentTime = default(DateTime);
            string confirmTime = "";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT change_date FROM Tbl_HB_quality_history WHERE quality = 3 and Doc_ID = @Doc_ID", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;

                    sentTime = Convert.ToDateTime(cmd.ExecuteScalar());
                    if (sentTime != default(DateTime))
                    {
                        confirmTime = sentTime.ToString("HH:mm");

                    }
                    else
                    {
                        confirmTime = "";

                    }
                }

            }
            return confirmTime;

        }

        internal static InternationalPaymentOrder GetCustomerDateForInternationalPayment(ulong customerNumber)
        {
            InternationalPaymentOrder internationalPayment = new InternationalPaymentOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT top 1 F.emailAddress,E.document_number, E.document_given_by, E.document_valid_date, E.document_given_date,b.nameEng,a.birth,b.lastNameEng,d.CountryName,C.customer_number FROM Tbl_Customers C
							LEFT JOIN (SELECT identityid,fullNameId,birth FROM Tbl_Persons) a 
										ON a.identityId = C.identityId
								LEFT JOIN (SELECT nameEng,lastNameEng,id FROM [V_FullNames]) b
										 ON b.id = a.fullNameId
										LEFT JOIN (SELECT CC.CountryName,V.country,V.identityId FROM V_CustomersAddresses V
														LEFT JOIN [Tbl_Country_Currency_codes] CC ON CC.CountryCodeN = V.country) d
														 ON C.identityId = d.identityId
																LEFT JOIN (SELECT document_number, document_given_by, document_valid_date, document_given_date, identityId FROM   tbl_customer_documents_current WITH (NOLOCK) WHERE is_default=1) E 
																		ON E.identityId=C.identityId 
																	LEFT JOIN(SELECT identityId,e.emailAddress FROM Tbl_Customer_Emails ce inner join Tbl_Emails e ON ce.emailId=e.id) F 
																			ON F.identityId = C.identityId
																			Where C.customer_number = @CustomerNumber ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    internationalPayment.SenderEmail = dr["emailAddress"] != DBNull.Value ? dr["emailAddress"].ToString() : "";
                    internationalPayment.SenderPassport = dr["document_number"] != DBNull.Value ? dr["document_number"].ToString() : "";
                    internationalPayment.SenderPassport = internationalPayment.SenderPassport + ", " + (dr["document_given_by"] != DBNull.Value ? dr["document_given_by"].ToString() : "");
                    internationalPayment.SenderPassport = internationalPayment.SenderPassport + ", " + (dr["document_given_date"] != DBNull.Value ? Convert.ToDateTime(dr["document_given_date"].ToString()).ToString() : "");
                    internationalPayment.Sender = dr["nameEng"] != DBNull.Value ? dr["nameEng"].ToString() : "";
                    internationalPayment.Sender = internationalPayment.Sender + ", " + (dr["lastNameEng"] != DBNull.Value ? dr["lastNameEng"].ToString() : "");
                    internationalPayment.SenderDateOfBirth = dr["birth"] != DBNull.Value ? Convert.ToDateTime(dr["birth"].ToString()) : default(DateTime);
                    internationalPayment.Country = dr["CountryName"] != DBNull.Value ? dr["CountryName"].ToString() : "";
                }
            }

            return internationalPayment;
        }
    }
}
