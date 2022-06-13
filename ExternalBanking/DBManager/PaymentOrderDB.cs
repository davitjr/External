using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal static class PaymentOrderDB
    {
        internal static PaymentOrder GetPaymentOrder(PaymentOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"Select rs.description as ReasonIdDescription,*  
                                                            from Tbl_Hb_Documents D  left join Tbl_New_Transfer_Doc TD on D.doc_ID = TD.Doc_id   LEFT JOIN dbo.tbl_type_of_card_debit_reasons rs ON rs.TYPE = d.reason_type  
                                                                         outer apply (select top 1  CONVERT(VARCHAR(5),change_date, 108)  change_time  from Tbl_HB_quality_history where doc_ID=D.doc_ID  order by quality )time
                                                            where D.doc_id=@id and D.customer_number=case when @customerNumber = 0 then customer_number else @customerNumber end ";
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    order.Id = long.Parse(dr["doc_id"].ToString());

                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                    order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                    order.SubType = Convert.ToByte((dr["document_subtype"]));

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
                        string debitAccount = dr["debet_account"].ToString();

                        if (order.Type == OrderType.CashDebit || order.Type == OrderType.CashDebitConvertation || order.Type == OrderType.CashConvertation || order.Type == OrderType.CashTransitCurrencyExchangeOrder
                                 || order.Type == OrderType.TransitCashOutCurrencyExchangeOrder || order.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                                 || order.Type == OrderType.TransitNonCashOut || order.Type == OrderType.TransitCashOut || order.Type == OrderType.ReestrTransferOrder)
                        {
                            order.DebitAccount = Account.GetSystemAccount(debitAccount);
                        }
                        else
                        {
                            order.DebitAccount = Account.GetAccount(debitAccount);
                        }

                        if (order.DebitAccount != null && order.DebitAccount.IsIPayAccount())
                        {
                            order.DebitAccount.IsAttachedCard = true;
                            order.DebitAccount.BindingId = Account.GetAttachedCardBindingId(order.Id);
                            order.DebitAccount.AttachedCardNumber = Account.GetAttachedCardNumber(order.Id);
                            order.CardFee = Account.GetAttachedCardFee(order.Id);
                            order.CardFeeCurrency = "AMD";
                        }
                    }

                    order.ReceiverAccount = new Account();
                    if (dr["credit_account"] != DBNull.Value)
                    {
                        string creditAccount = dr["credit_account"].ToString();

                        if (order.Type == OrderType.RATransfer || order.Type == OrderType.CashDebit || order.Type == OrderType.CashForRATransfer || order.Type == OrderType.ReestrTransferOrder || order.Type == OrderType.EventTicketOrder)
                            creditAccount = dr["credit_bank_code"].ToString() + creditAccount;

                        if ((order.Type == OrderType.RATransfer && order.SubType == 3) || order.Type == OrderType.Convertation || order.Type == OrderType.CashDebit ||
                            order.Type == OrderType.CashDebitConvertation || order.Type == OrderType.InBankConvertation || order.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                            || order.Type == OrderType.TransitNonCashOut || order.Type == OrderType.ReestrTransferOrder)
                        {
                            order.ReceiverAccount = Account.GetAccount(creditAccount);
                        }
                        else if (order.Type == OrderType.CashCredit || order.Type == OrderType.CashCreditConvertation || order.Type == OrderType.CashConvertation || order.Type == OrderType.CashTransitCurrencyExchangeOrder || order.Type == OrderType.TransitCashOutCurrencyExchangeOrder || order.Type == OrderType.TransitCashOut)
                        {
                            order.ReceiverAccount = Account.GetSystemAccount(creditAccount);
                        }
                        else
                        {
                            order.ReceiverAccount.AccountNumber = creditAccount;
                            order.ReceiverAccount.OpenDate = default;
                            order.ReceiverAccount.ClosingDate = default;
                            order.ReceiverAccount.FreezeDate = default;
                        }
                    }

                    if (dr["receiver_name"] != DBNull.Value)
                        order.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                    if (dr["credit_bank_code"] != DBNull.Value)
                        order.ReceiverBankCode = Convert.ToInt32(dr["credit_bank_code"]);

                    if (dr["amount"] != DBNull.Value)
                        order.Amount = Convert.ToDouble(dr["amount"]);

                    if (dr["currency"] != DBNull.Value)
                        order.Currency = dr["currency"].ToString();

                    order.SubType = Convert.ToByte(dr["document_subtype"]);

                    order.OrderNumber = dr["document_number"].ToString();

                    if (dr["description"] != DBNull.Value)
                        order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                    if (dr["rate_sell_buy"] != DBNull.Value)
                        order.ConvertationRate = Convert.ToDouble(dr["rate_sell_buy"]);

                    if (dr["rate_sell_buy_cross"] != DBNull.Value)
                        order.ConvertationRate1 = Convert.ToDouble(dr["rate_sell_buy_cross"]);

                    if (dr["amount_for_payment"] != DBNull.Value)
                        order.TransferFee = Convert.ToDouble(dr["amount_for_payment"]);

                    if (dr["deb_for_transfer_payment"] != DBNull.Value)
                        order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dr["deb_for_transfer_payment"]));

                    if (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0" && order.TransferFee != 0)
                    {
                        cmd.Parameters.Add("@fee_currency3", SqlDbType.NVarChar).Value = order.FeeAccount.Currency;
                        cmd.Parameters.Add("@fee_type3", SqlDbType.TinyInt).Value = 20;
                    }

                    if (dr["urgent"] != DBNull.Value)
                        order.UrgentSign = Convert.ToBoolean(dr["urgent"]);
                    if (dr["transfer_id"] != DBNull.Value)
                        order.TransferID = Convert.ToUInt64(dr["transfer_id"]);
                    if (order.Type == OrderType.InterBankTransferCash || order.Type == OrderType.InterBankTransferNonCash)
                    {
                        order.AdditionalParametrs = new List<AdditionalDetails>
                                {
                                    new AdditionalDetails
                                    {
                                        AdditionValue = Info.GetSyntheticStatus(dr["Receiver_type"].ToString())
                                    }
                                };
                    }

                    if (order.Type == OrderType.RATransfer && order.SubType == 1 && !string.IsNullOrEmpty(dr["descr_for_payment"].ToString()))
                    {
                        order.AdditionalParametrs = new List<AdditionalDetails>
                                {
                                    new AdditionalDetails
                                    {
                                        AdditionValue = dr["descr_for_payment"].ToString(),
                                        AdditionTypeDescription = "BillSplitSenderId"
                                    }
                                };
                    }

                    if (dr["credit_code"] != DBNull.Value)
                    {
                        order.CreditCode = dr["credit_code"].ToString();
                        order.Borrower = dr["borrower_name"].ToString();
                        order.MatureType = dr["mature_type"].ToString();
                    }

                    if (dr["SintAccDetailsValue"] != DBNull.Value)
                    {
                        order.CreditorStatus = Convert.ToInt32(dr["SintAccDetailsValue"]);
                        order.CreditorStatusDescription = Info.GetSyntheticStatus(order.CreditorStatus.ToString());
                        order.ForThirdPerson = true;
                    }

                    if (dr["DebitorName"] != DBNull.Value)
                        order.CreditorDescription = dr["DebitorName"].ToString();

                    if (dr["Debitor_SocNoSoc"] != DBNull.Value)
                    {
                        order.CreditorDocumentNumber = dr["Debitor_SocNoSoc"].ToString();
                        order.CreditorDocumentType = 1;
                    }

                    if (dr["Debitor_NoSoc_Number"] != DBNull.Value)
                    {
                        order.CreditorDocumentNumber = dr["Debitor_NoSoc_Number"].ToString();
                        order.CreditorDocumentType = 2;
                    }

                    if (dr["Debitor_Passport_Number"] != DBNull.Value)
                    {
                        order.CreditorDocumentNumber = dr["Debitor_Passport_Number"].ToString();
                        order.CreditorDocumentType = 3;
                    }

                    if (dr["DebitorHVHH"] != DBNull.Value)
                    {
                        order.CreditorDocumentNumber = dr["DebitorHVHH"].ToString();
                        order.CreditorDocumentType = 4;
                    }

                    if (dr["change_time"] != DBNull.Value)
                    {
                        order.RegistrationTime = dr["change_time"].ToString();
                    }

                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.UseCreditLine = dr["use_credit_line"] != DBNull.Value && Convert.ToBoolean(dr["use_credit_line"]);

                    if (dr["source_type"] != DBNull.Value)
                    {
                        order.Source = (SourceType)Convert.ToInt16(dr["source_type"]);
                    }

                    order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                    order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["filial"].ToString());
                }
                else
                {
                    order = null;
                }
            }

            if (order != null && (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking || order.Source == SourceType.AcbaOnlineXML
                     || order.Source == SourceType.ArmSoft) && order.Type == OrderType.RATransfer && order.SubType == 1)
            {
                using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;

                cmd.CommandText = @"SELECT [dbo].[fnc_check_DAHK_availability] ((SELECT customer_number FROM [tbl_all_accounts;] WHERE Arm_number =
                        (SELECT CASE WHEN LEN(credit_account) > 10 THEN credit_account ELSE CAST(credit_bank_code AS varchar(10)) + credit_account END
                        FROM dbo.[Tbl_Hb_Documents]  WHERE Doc_Id = @id))) as hasDAHK";
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        order.CreditorHasDAHK = !Convert.ToBoolean(dr["hasDAHK"].ToString());
                    }
                }

                cmd.CommandText = @"SELECT payment_Type FROM [dbo].[tbl_DAHK_bypass_of_HB_transfers] WHERE Doc_Id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        order.ArmPaymentType = Convert.ToInt32(dr["payment_Type"].ToString());
                    }
                }
            }

            return order;
        }

        internal static ActionResult Save(PaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "sp_Submit";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;

            string creditAccountNumber;
            if (order.ReceiverBankCode == 10300 && order.ReceiverAccount.AccountNumber.ToString().Substring(0, 1) == "9")
            {
                creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
            }
            else
            {
                if (order.Type != OrderType.Convertation && order.Type != OrderType.CashCredit
                    && order.Type != OrderType.CashConvertation
                    && order.Type != OrderType.TransitCashOutCurrencyExchangeOrder
                    && order.Type != OrderType.CashCreditConvertation
                    && order.Type != OrderType.InBankConvertation
                    && order.Type != OrderType.NonCashTransitCurrencyExchangeOrder)
                {
                    if (source == SourceType.SSTerminal && order.Type == OrderType.CashDebit)
                    {
                        creditAccountNumber = order.ReceiverAccount.AccountNumber;
                    }
                    else
                    {
                        creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }
                }
                else
                {
                    creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
                }
            }

            if (order?.DebitAccount?.IsAttachedCard == true && order.Type == OrderType.RATransfer && order.ReceiverBankCode == 0 && int.TryParse(order.ReceiverAccount.AccountNumber.Substring(0, 5), out int bankCode)) // Կցված քարտով վարկի մարման համար (Կցված քարտից փոխանցում վարկային կոդին)
            {
                order.ReceiverBankCode = bankCode;
            }

            if (order.ValidateForConvertation)
            {
                cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = order.ConvertationRate;
                cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = order.ConvertationRate1;
            }

            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;

            cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
            cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description;
            cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 250).Value = order.Receiver;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
            cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;
            cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = order.TransferID;

            if (order.GroupId != 0)
            {
                cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
            }

            if (order.CardFee != 0 && order.Source != SourceType.MobileBanking
                && order.Source != SourceType.AcbaOnline)
            {
                cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.CardFee;
                cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = order.DebitAccount.Currency;
                cmd.Parameters.Add("@fee_type", SqlDbType.TinyInt).Value = 7;
            }

            if (!string.IsNullOrEmpty(order.CreditCode))
            {
                cmd.Parameters.Add("@credit_code", SqlDbType.NVarChar, 16).Value = order.CreditCode;
                cmd.Parameters.Add("@borrower_name", SqlDbType.NVarChar, 31).Value = order.Borrower;
                cmd.Parameters.Add("@mature_type", SqlDbType.NVarChar, 1).Value = order.MatureType;
            }

            if (order.CreditorDocumentType == 1 && order.CreditorDocumentNumber != null)
            {
                cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
            }
            else if (order.CreditorDocumentType == 2 && order.CreditorDocumentNumber != null)
            {
                cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
            }
            else if (order.CreditorDocumentType == 3 && order.CreditorDocumentNumber != null)
            {
                cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
            }

            else if (order.CreditorDocumentType == 4 && order.CreditorDocumentNumber != null)
            {
                cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
            }

            if (order.CreditorStatus != 0)
            {
                cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = order.CreditorStatus;
            }

            if (order.CreditorDescription != null)
            {
                cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = order.CreditorDescription;
            }

            if (order.ReceiverAccount.AccountNumber.ToString() == "103008661003")
                cmd.Parameters.Add("@public_contributor", SqlDbType.TinyInt).Value = order.PublicContributor;

            cmd.Parameters.Add("@debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

            if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking) && order.Type == OrderType.RATransfer && order.SubType == 1 && order.AdditionalParametrs != null && order.AdditionalParametrs.Exists(m => m.AdditionTypeDescription == "BillSplitSenderId"))
                cmd.Parameters.Add("@descr_for_payment", SqlDbType.NVarChar).Value = order.AdditionalParametrs[0].AdditionValue;

            SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(param);

            param = new SqlParameter("@id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(param);

            if (order.Id != 0)
            {
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
            }

            if (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0")
            {
                cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
            }

            if (order.TransferFee != 0 && order.Source != SourceType.MobileBanking
                && order.Source != SourceType.AcbaOnline)
            {
                cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;
            }

            cmd.Parameters.Add("@urgent", SqlDbType.TinyInt).Value = Convert.ToByte(order.UrgentSign);
            cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
            cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
            cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = order.UseCreditLine;
            cmd.Parameters.Add("@isOtherBankCard", SqlDbType.Bit).Value = order?.DebitAccount?.IsAttachedCard ?? false;

            cmd.Parameters.Add("@reason_type", SqlDbType.SmallInt).Value = order.ReasonId;

            if (!string.IsNullOrEmpty(order.ReasonIdDescription))
            {
                cmd.Parameters.Add("@reason_type_description", SqlDbType.NVarChar, 350).Value = order.ReasonIdDescription;

            }
            else
            {
                cmd.Parameters.Add("@reason_type_description", SqlDbType.NVarChar, 350).Value = DBNull.Value;
            }

            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

            if (order.Type == OrderType.InterBankTransferCash || order.Type == OrderType.InterBankTransferNonCash)
            {
                if (order.AdditionalParametrs != null && order.AdditionalParametrs.Exists(m => m.AdditionTypeDescription == "InterBankTransfer"))
                {
                    cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = Convert.ToInt16(order.AdditionalParametrs[0].AdditionValue);
                }
                else
                {
                    cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = DBNull.Value;
                }
            }
            else
            {
                cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = DBNull.Value;
            }

            cmd.ExecuteNonQuery();

            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            short actionResult = Convert.ToInt16(cmd.Parameters["@result"].Value);

            if (actionResult == 1)
            {
                result.ResultCode = ResultCode.Normal;
                result.Id = order.Id;
                if (order.Source == SourceType.SSTerminal)
                {
                    OrderDB.SaveOrderDetails(order);
                }
            }
            else if (actionResult == 0)
            {
                result.ResultCode = ResultCode.Failed;
                result.Id = -1;
                result.Errors.Add(new ActionError(actionResult));
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
                result.Id = -1;
                result.Errors.Add(new ActionError(actionResult));
            }

            return result;
        }

        internal static int CheckTransferVerification(double accountNumber, int LTACode, int cust_type, string TIN = "", int creditorCustomerType = 0, string creditorTIN = "")
        {

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString());

            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "sp_transfer_verification";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@account", SqlDbType.Float).Value = accountNumber;
            cmd.Parameters.Add("@recipient_code", SqlDbType.VarChar, 2).Value = LTACode;
            cmd.Parameters.Add("@cust_type_1", SqlDbType.Int).Value = cust_type;
            cmd.Parameters.Add("@tax_code_1", SqlDbType.VarChar, 20).Value = TIN;

            if (creditorCustomerType != 0)
            {
                cmd.Parameters.Add("@cust_type_2", SqlDbType.Int).Value = creditorCustomerType;
                cmd.Parameters.Add("@tax_code_2", SqlDbType.VarChar, 20).Value = creditorTIN;
            }

            cmd.Parameters.Add("@return_result", SqlDbType.TinyInt).Value = 0;
            cmd.Parameters.Add("@is_HB", SqlDbType.TinyInt).Value = 1;

            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

            cmd.ExecuteNonQuery();

            return Convert.ToInt32(cmd.Parameters["@result"].Value);

        }

        //Ստանում է անդորրագրի համարը
        internal static string CreateSerialNumber(int currencyCode, byte operationType)
        {
            string SerialNumber = Utility.ConvertAnsiToUnicode(((char)178).ToString());

            SerialNumber += Utility.ConvertAnsiToUnicode(GetCurrencyLetter(currencyCode));

            if (operationType == 1 || operationType == 3)
            {
                SerialNumber += 'Վ';
            }
            else if (operationType == 2)
            {
                SerialNumber += 'Գ';
            }

            return SerialNumber;
        }

        //Ստանում է արժույթի տառը
        internal static string GetCurrencyLetter(int code)
        {
            string currencyLetter = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"select letter from  [tbl_currency;] where code=@code", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@code", SqlDbType.Int).Value = code;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    currencyLetter = dr["letter"].ToString();
                }
            }

            return currencyLetter;
        }

        internal static ActionResult SaveCash(PaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());

            using (SqlCommand cmd = new SqlCommand("pr_save_payment_order_cash", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                string creditAccountNumber;
                if (order.ReceiverBankCode == 10300 && order.ReceiverAccount.AccountNumber.Substring(0, 1) == "9")
                {
                    creditAccountNumber = order.ReceiverAccount.AccountNumber;
                }
                else
                {
                    if (!order.ValidateForConvertation && order.Type != OrderType.CashCredit && order.Type != OrderType.CashOutFromTransitAccountsOrder
                        && order.Type != OrderType.CardServiceFeePayment && order.Type != OrderType.TransitCashOut
                        && order.Type != OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount
                        && order.Type != OrderType.ReestrPaymentOrder)
                    {
                        if ((source == SourceType.SSTerminal || source == SourceType.CashInTerminal) && order.Type == OrderType.CashDebit)
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber;
                        }
                        else
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber.Substring(5);
                        }
                    }
                    else
                    {
                        creditAccountNumber = order.ReceiverAccount.AccountNumber;
                    }
                }

                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description ?? "";
                cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 250).Value = order.Receiver ?? "";
                cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;
                if (order.ValidateForConvertation)
                {
                    cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = order.ConvertationRate;
                    cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = order.ConvertationRate1;
                }
                else
                {
                    cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = DBNull.Value;
                    cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = DBNull.Value;
                }

                if (order.ValidateForConvertation && order.SubType == 3)
                {
                    cmd.Parameters.Add("@cross_currency", SqlDbType.VarChar, 50).Value = order.ReceiverAccount.Currency;
                }
                else
                {
                    cmd.Parameters.Add("@cross_currency", SqlDbType.VarChar, 50).Value = DBNull.Value;
                }

                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                cmd.Parameters.Add("@urgent", SqlDbType.TinyInt).Value = Convert.ToByte(order.UrgentSign);
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = order.TransferID;
                if (order.ReceiverAccount.AccountNumber == "103008661003")
                    cmd.Parameters.Add("@public_contributor", SqlDbType.TinyInt).Value = order.PublicContributor;
                else
                    cmd.Parameters.Add("@public_contributor", SqlDbType.TinyInt).Value = DBNull.Value;
                if (order.Type == OrderType.InterBankTransferCash || order.Type == OrderType.InterBankTransferNonCash)
                {

                    if (order.AdditionalParametrs != null && order.AdditionalParametrs.Exists(m => m.AdditionTypeDescription == "InterBankTransfer"))
                    {
                        cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = Convert.ToInt16(order.AdditionalParametrs[0].AdditionValue);
                    }
                    else
                    {
                        cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = DBNull.Value;
                    }

                    if (order.Fees != null && order.Fees.Count > 0 && order.Fees.Exists(m => m.Type == 5 || m.Type == 20))
                        cmd.Parameters.Add("@details_of_charges", SqlDbType.VarChar, 20).Value = "OUR";
                    else
                        cmd.Parameters.Add("@details_of_charges", SqlDbType.VarChar, 20).Value = "BEN";
                }
                else
                {
                    cmd.Parameters.Add("@Receiver_type", SqlDbType.TinyInt).Value = DBNull.Value;
                    cmd.Parameters.Add("@details_of_charges", SqlDbType.VarChar, 20).Value = DBNull.Value;
                }

                if (!string.IsNullOrEmpty(order.CreditCode))
                {
                    cmd.Parameters.Add("@credit_code", SqlDbType.NVarChar, 16).Value = order.CreditCode;
                    cmd.Parameters.Add("@borrower_name", SqlDbType.NVarChar, 31).Value = order.Borrower;
                    cmd.Parameters.Add("@mature_type", SqlDbType.NVarChar, 1).Value = order.MatureType;
                }
                else
                {
                    cmd.Parameters.Add("@credit_code", SqlDbType.NVarChar, 16).Value = DBNull.Value;
                    cmd.Parameters.Add("@borrower_name", SqlDbType.NVarChar, 31).Value = DBNull.Value;
                    cmd.Parameters.Add("@mature_type", SqlDbType.NVarChar, 1).Value = DBNull.Value;
                }

                if (order.CreditorDocumentType == 1 && order.CreditorDocumentNumber != null)
                {
                    cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                    cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;
                }
                else if (order.CreditorDocumentType == 2 && order.CreditorDocumentNumber != null)
                {
                    cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                    cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                }
                else if (order.CreditorDocumentType == 3 && order.CreditorDocumentNumber != null)
                {
                    cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                    cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                }
                else if (order.CreditorDocumentType == 4 && order.CreditorDocumentNumber != null)
                {
                    cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                    cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                }
                else if (order.CreditorDocumentType == 0 || string.IsNullOrEmpty(order.CreditorDocumentNumber))
                {
                    cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                }

                if (order.CreditorStatus != 0)
                {
                    cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = order.CreditorStatus;
                }
                else
                {
                    cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = DBNull.Value;
                }

                if (order.CreditorDescription != null)
                {
                    cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = order.CreditorDescription;
                }
                else
                {
                    cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = DBNull.Value;
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

                conn.Open();

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            }

            result.ResultCode = ResultCode.Normal;
            if (order.Source == SourceType.SSTerminal || order.Source == SourceType.CashInTerminal)
            {
                OrderDB.SaveOrderDetails(order);
            }

            return result;
        }

        /// <summary>
        /// Ստուգում է արդյոք հաճախորդի գործարքի և օրվա ընթացքում կատարված գործարքների հանրագումարը գերազանցում է Խոշոր կանխիկ գործարքի վերաբերյալ հայտարարության սահմանաչափը
        /// </summary>
        /// <param name="type"></param>
        /// <param name="accountNumber"></param>
        /// <param name="amount"></param>
        /// <param name="customerNumber"></param>
        /// <param name="operationDate"></param>
        /// <returns></returns>
        internal static Tuple<bool, string> IsBigAmount(string type, string accountNumber, double amount, string currency, ulong customerNumber, DateTime operationDate)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "[dbo].[pr_is_big_amount]";
            cmd.CommandType = CommandType.StoredProcedure;
            if (type == "in_out")
            {
                cmd.Parameters.Add("@isIncoming", SqlDbType.TinyInt).Value = 3;
            }
            else
            {
                cmd.Parameters.Add("@isIncoming", SqlDbType.TinyInt).Value = (type == "in") ? 1 : 2;
            }

            cmd.Parameters.Add("@armNumber", SqlDbType.Float).Value = (string.IsNullOrEmpty(accountNumber)) ? 0 : ulong.Parse(accountNumber.ToString());
            cmd.Parameters.Add("@currentSum", SqlDbType.Money).Value = amount;
            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
            cmd.Parameters.Add("@calcDate", SqlDbType.SmallDateTime).Value = operationDate;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
            cmd.Parameters.Add(new SqlParameter("@isBigAmount", SqlDbType.Bit) { Direction = ParameterDirection.Output });
            cmd.Parameters.Add(new SqlParameter("@inOut", SqlDbType.NVarChar, 6) { Direction = ParameterDirection.Output });
            cmd.ExecuteNonQuery();
            bool isBigAmount = Convert.ToInt32(cmd.Parameters["@isBigAmount"].Value) == 1;
            string inOut = cmd.Parameters["@inOut"].Value.ToString();
            return Tuple.Create<bool, string>(isBigAmount, inOut);
        }

        internal static ActionResult SaveOrderPaymentDetails(PaymentOrder paymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO Tbl_BO_payment_RA_details(order_id, urgent) VALUES(@orderId, @urgent)";

            cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@urgent", SqlDbType.Int).Value = paymentOrder.UrgentSign;

            cmd.ExecuteNonQuery();

            result.ResultCode = ResultCode.Normal;

            return result;
        }

        /// <summary>
        /// Վերադարձնում է վճարման հանձնարարականի նկարագրությունը
        /// </summary>
        /// <returns></returns>
        internal static string GetPaymentOrderDescription(PaymentOrder order, ulong customerNumber)
        {
            string description = "";
            if (order.Type == OrderType.RATransfer && (order.SubType == 1 || order.SubType == 3))
                description = "Փոխանցում հաշվին";
            if (order.ReceiverAccount != null)
            {

                if (order.ReceiverAccount.IsCardAccount())
                {
                    if (order.Type == OrderType.CashDebit)
                    {
                        description = "Քարտային հաշվին մուծում";
                        description += '(' + Card.GetCardWithOutBallance(order.ReceiverAccount.AccountNumber).CardType + ',' + order.ReceiverAccount.Currency + ')';
                    }

                    else if (order.Type == OrderType.RATransfer)
                    {
                        description = "Փոխանցում քարտային հաշվին";
                        description += '(' + Card.GetCardWithOutBallance(order.ReceiverAccount.AccountNumber).CardType + ',' + order.ReceiverAccount.Currency + ')';
                    }
                }
                else if (order.ReceiverAccount.IsDepositAccount())
                {
                    Deposit deposit = Deposit.GetDeposit(Int64.Parse(order.ReceiverAccount.ProductNumber), customerNumber);
                    description = "Ավանդի համալրում" + '(' + deposit.DepositNumber.ToString() + ',' + deposit.StartDate.ToString("dd/MM/yy") + ')';
                }
                else if (order.Type == OrderType.CashDebit)
                {
                    description = "Հաշվին մուծում";
                }
            }
            else if (order.Type == OrderType.CashDebit)
            {
                description = "Հաշվին մուծում";
            }

            return description;
        }

        public static bool IsTransferFromBusinessmanToOwnerAccount(string debitAccountNumber, string creditAccountNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "select dbo.fn_is_transfer_from_businessman_to_own_card (@debit_acc_number,@credit_acc_number)";

                cmd.Parameters.Add("@debit_acc_number", SqlDbType.Float).Value = debitAccountNumber;
                cmd.Parameters.Add("@credit_acc_number", SqlDbType.Float).Value = creditAccountNumber;

                cmd.ExecuteNonQuery();
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    check = true;
                }
            }

            return check;

        }

        public static void SaveReestrTransferDetails(ReestrTransferOrder order)
        {
            order.ReestrTransferAdditionalDetails.ForEach(m =>
            {
                using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.Tbl_HB_Transfers_Registry(IndexID, docID, name_surname, credit_account,amount,quality, reason,payment_type)
                                                    VALUES (@indexID,@docID, @name_surname,  @credit_account,@amount,@quality, @reason,@paymentType)", conn);

                cmd.Parameters.Add("@indexID", SqlDbType.Int).Value = m.Index;

                cmd.Parameters.Add("@docID", SqlDbType.Int).Value = order.Id;

                if (order.Type == OrderType.ReestrTransferOrder)
                    cmd.Parameters.Add("@name_surname", SqlDbType.NVarChar, 255).Value = DBNull.Value;
                else if (order.Type == OrderType.RosterTransfer)
                    cmd.Parameters.Add("@name_surname", SqlDbType.NVarChar, 255).Value = m.NameSurename;
                else
                    cmd.Parameters.Add("@name_surname", SqlDbType.NVarChar, 255).Value = m.Description;

                if (order.Type == OrderType.ReestrTransferOrder)
                    cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.ReceiverAccount.AccountNumber;
                else
                    cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = m.CreditAccount.AccountNumber;

                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = m.Amount;
                cmd.Parameters.Add("@quality", SqlDbType.Int).Value = DBNull.Value;

                if (order.Type == OrderType.ReestrTransferOrder || order.Type == OrderType.RosterTransfer)
                    cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 4000).Value = m.Description;
                else
                    cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 4000).Value = DBNull.Value;

                if (m.PaymentType != 0)
                {
                    cmd.Parameters.Add("@paymentType", SqlDbType.Int).Value = m.PaymentType;
                }
                else
                {
                    cmd.Parameters.Add("@paymentType", SqlDbType.Int).Value = DBNull.Value;
                }

                cmd.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Վերադարձնում է ռեեստրով փոխանցման տվյալները
        /// </summary>
        /// <param name="order"></param>
        public static void GetReestrTransferDetails(ReestrTransferOrder order)
        {
            order.ReestrTransferAdditionalDetails = new List<ReestrTransferAdditionalDetails>();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();

            string query = "select * from Tbl_HB_Transfers_Registry  where docID = @docId";

            if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.AcbaOnlineXML || order.Source == SourceType.ArmSoft || order.Source == SourceType.MobileBanking)
                && (order.Quality == OrderQuality.Sent3 || order.Quality == OrderQuality.SBQprocessed))
            {
                query += " AND quality = 0";
            }

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@docId", SqlDbType.Float).Value = order.Id;
            DataTable dt = new DataTable();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                dt.Load(dr);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                ReestrTransferAdditionalDetails reestrTransferAdditionalDetails = new ReestrTransferAdditionalDetails
                {
                    Amount = Convert.ToDouble(row["amount"].ToString())
                };

                if (order.Type == OrderType.RosterTransfer)
                {
                    reestrTransferAdditionalDetails.TransactionDescription = Utility.ConvertAnsiToUnicode(row["reason"].ToString());
                }

                if (order.Type == OrderType.ReestrTransferOrder)
                    reestrTransferAdditionalDetails.Description = Utility.ConvertAnsiToUnicode(row["reason"].ToString());
                else if (order.Type == OrderType.ReestrPaymentOrder || order.Type == OrderType.RosterTransfer)
                {
                    reestrTransferAdditionalDetails.Description = Utility.ConvertAnsiToUnicode(row["name_surname"].ToString());
                }

                if (order.Type == OrderType.RosterTransfer)
                {
                    reestrTransferAdditionalDetails.NameSurename = Utility.ConvertAnsiToUnicode(row["name_surname"].ToString());
                }

                reestrTransferAdditionalDetails.Index = Convert.ToInt32(row["IndexID"].ToString());
                if (row["credit_account"] != DBNull.Value)
                    reestrTransferAdditionalDetails.CreditAccount = new Account(row["credit_account"].ToString());

                if (order.Type != OrderType.RosterTransfer)
                {
                    reestrTransferAdditionalDetails.TransactionsGroupNumber = GetReestrTransferTransactionGroupNumber(order.Id, reestrTransferAdditionalDetails.Index);
                }

                order.ReestrTransferAdditionalDetails.Add(reestrTransferAdditionalDetails);
            }
        }

        /// <summary>
        /// Ստուգում է կանխիկ ելք տարանցիկ հաշվի ժամանակ անհրաժեշտ է գնա հաստատամնա թե ոչ
        /// </summary>
        /// <param name="debitAccountNumber"></param>
        /// <returns></returns>
        public static bool IsRequireApprovalCashOutFromTransitAccounts(string debitAccountNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT ISNULL(requires_approval,0) FROM Tbl_transit_accounts_for_debit_transactions 
                                        WHERE arm_number=@debit_acc_number";

                cmd.Parameters.Add("@debit_acc_number", SqlDbType.Float).Value = debitAccountNumber;

                cmd.ExecuteNonQuery();
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    check = true;
                }
            }

            return check;

        }

        public static ulong? GetReestrTransferTransactionGroupNumber(long docID, int indexID)
        {
            ulong? transactionGroupNumber = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT  Transactions_Group_number 
                                        FROM Tbl_transactions_from_excel
                                        WHERE HB_doc_ID=@docID AND number_in_list=@indexID";

                cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;
                cmd.Parameters.Add("@indexID", SqlDbType.Int).Value = indexID;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read() && dr["Transactions_Group_number"] != DBNull.Value)
                {
                    transactionGroupNumber = Convert.ToUInt64(dr["Transactions_Group_number"]);
                }
            }

            return transactionGroupNumber;

        }

        public static bool IsReestrOrderDonePartially(ulong customerNumber, DateTime registrationDate)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT doc_ID FROM Tbl_HB_documents
                                                      WHERE customer_number=@customerNumber 
                                                      AND document_type=122 AND quality=20 AND registration_date=@registrationDate";

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = registrationDate.Date;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    check = true;
                }
            }

            return check;

        }
        internal static ActionResult SaveUtilityTransferDetails(string transactionId, long orderId, CommunalTypes utilityServiceType, DateTime? externalStartDate, DateTime actionStartDate)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM Tbl_Utility_Transfer_Details WHERE doc_id = @docId " +
                               "INSERT INTO Tbl_Utility_Transfer_Details(external_transaction_id, doc_id, action_start_date, utility_service_type, external_start_date) " +
                              "VALUES(@transactionId, @docId, @action_start_date, @utility_service_type, @external_start_date)";

            cmd.Parameters.Add("@transactionId", SqlDbType.NVarChar).Value = transactionId;
            cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@action_start_date", SqlDbType.SmallDateTime).Value = actionStartDate;
            cmd.Parameters.Add("@utility_service_type", SqlDbType.Int).Value = utilityServiceType;
            cmd.Parameters.Add("@external_start_date", SqlDbType.SmallDateTime).Value = externalStartDate;

            cmd.ExecuteNonQuery();

            result.ResultCode = ResultCode.Normal;

            return result;
        }

        public static ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, string debetAccount, Languages languages, long id)
        {
            ActionResult result = new ActionResult();
            DataTable dataTable = new DataTable();
            if (reestrTransferAdditionalDetails != null)
            {
                using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "sp_checkExcelRows";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 50).Value = String.IsNullOrEmpty(debetAccount) ? "-1" : debetAccount;
                cmd.Parameters.Add("@lang_id", SqlDbType.VarChar, 2).Value = languages;

                SqlParameter prm = new SqlParameter("@dt", SqlDbType.Structured)
                {
                    Value = ReestrTransferAdditionalDetails.ConvertAdditionalReestrDetailsToDataTable(reestrTransferAdditionalDetails, languages, id),
                    TypeName = "dbo.RegisterTransfersDetails"
                };
                cmd.Parameters.Add(prm);

                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int)
                { Direction = ParameterDirection.Output });
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    dataTable.Load(dataReader);
                }

                int res = Convert.ToInt32(cmd.Parameters["@result"].Value);

                if (res == 0)
                {
                    result.ResultCode = ResultCode.Normal;
                }
                else
                {
                    string rowsN_invalidCred = dataTable.Rows[0]["rowsEnum"].ToString();
                    string rowsN_sameAcc = dataTable.Rows[1]["rowsEnum"].ToString();
                    string rowsN = dataTable.Rows[2]["rowsEnum"].ToString();
                    string rowsN_unicode = dataTable.Rows[3]["rowsEnum"].ToString();
                    string rowsN_cent = dataTable.Rows[4]["rowsEnum"].ToString();
                    string rowsN_Buge_Transfer = dataTable.Rows[5]["rowsEnum"].ToString();
                    string rowsN_Bank_Closed = dataTable.Rows[6]["rowsEnum"].ToString();
                    if (!string.IsNullOrEmpty(rowsN))
                    {
                        result.Errors.Add(new ActionError(169, new string[] { rowsN }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_unicode))

                    {
                        result.Errors.Add(new ActionError(170, new string[] { rowsN_unicode }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_cent))
                    {
                        result.Errors.Add(new ActionError(175, new string[] { rowsN_cent }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_sameAcc))
                    {
                        result.Errors.Add(new ActionError(174, new string[] { rowsN_sameAcc }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_invalidCred))
                    {
                        result.Errors.Add(new ActionError(176, new string[] { rowsN_invalidCred }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_Buge_Transfer))
                    {
                        result.Errors.Add(new ActionError(415, new string[] { rowsN_Buge_Transfer }));
                    }

                    if (!string.IsNullOrEmpty(rowsN_Bank_Closed))
                    {
                        result.Errors.Add(new ActionError(923, new string[] { rowsN_Bank_Closed }));
                    }

                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        internal static ActionResult SaveReestr(ReestrTransferOrder order, string userName, SourceType source, string fileId, Action action, Languages languages = Languages.hy)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand("sp_submit_registry_trans_2", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                if (order.Id != 0)
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                }

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = order.Description ?? "";
                cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = languages;

                SqlParameter prm = new SqlParameter("@dt", SqlDbType.Structured)
                {
                    Value = ReestrTransferAdditionalDetails.ConvertAdditionalReestrDetailsToDataTable(order.ReestrTransferAdditionalDetails, languages, (int)order.Id),
                    TypeName = "dbo.RegisterTransfersDetails"
                };
                cmd.Parameters.Add(prm);

                if (fileId != null)
                {
                    cmd.Parameters.Add("@fileID", SqlDbType.VarChar).Value = fileId;
                }

                cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = userName;

                SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(param);

                cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = order.Id });

                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();

                byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                order.Id = id;

                if (actionResult == 1)
                {
                    result.ResultCode = ResultCode.Normal;
                    result.Id = id;
                }
                else if (actionResult == 0)
                {
                    result.ResultCode = ResultCode.Failed;
                    result.Id = -1;
                }
            }

            return result;
        }

        public static void AddInBankTransferDetailsForAML(PaymentOrder payment, short amlCheck)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = " INSERT INTO tbl_in_bank_transfer_details_for_AML ([doc_ID] ,[customerNumber] ,[amount] ,[currency] ,[debet_account] ,[description] ," +
                "                       [operation_type] ,[credit_account]  ,[registration_date] ,[aml_check] ,[filial],sender_name, receiver_name, confirmation_type)  " +
                "                               VALUES( @doc_ID, @customerNumber, @amount, @currency, @debet_account, @description," +
                "                                               @operation_type, @credit_account, @registration_date, @aml_check,@filial, @sender_name, @receiver_name, @confirmation_type)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@doc_ID", SqlDbType.BigInt).Value = payment.Id;
            cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = payment.CustomerNumber;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = payment.Amount;
            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 50).Value = payment.Currency;
            cmd.Parameters.Add("@debet_account", SqlDbType.Float).Value = Convert.ToUInt64(payment.DebitAccount.AccountNumber);
            cmd.Parameters.Add("@description", SqlDbType.NVarChar, 255).Value = Utility.ConvertUnicodeToAnsi(payment.Description);
            cmd.Parameters.Add("@operation_type", SqlDbType.TinyInt).Value = 1;
            cmd.Parameters.Add("@credit_account", SqlDbType.Float).Value = Convert.ToUInt64(payment.ReceiverAccount.AccountNumber);
            cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = payment.RegistrationDate;
            cmd.Parameters.Add("@aml_check", SqlDbType.TinyInt).Value = amlCheck;
            cmd.Parameters.Add("@filial", SqlDbType.Int).Value = payment.FilialCode;
            cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 250).Value = Utility.ConvertUnicodeToAnsi(payment.DebitAccount.AccountDescription);
            payment.ReceiverAccount = Account.GetAccount(payment.ReceiverAccount.AccountNumber);
            cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 250).Value = Utility.ConvertUnicodeToAnsi(payment.ReceiverAccount.AccountDescription);
            cmd.Parameters.Add("@confirmation_type", SqlDbType.TinyInt).Value = payment.OnlySaveAndApprove ? 2 : 1;

            cmd.ExecuteNonQuery();
        }

        internal static short MatchAMLConditions(PaymentOrder paymentOrder, ulong receiver_customer_number = 0)
        {
            short amlCheck = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "[sp_match_AML_conditions]";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@transfer_system", SqlDbType.Int).Value = 3;
                cmd.Parameters.Add("@operation_type", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@country_code", SqlDbType.Int).Value = 51;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = paymentOrder.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = paymentOrder.Currency;
                cmd.Parameters.Add("@bank_code", SqlDbType.Int).Value = -1;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = receiver_customer_number == 0 ? paymentOrder.CustomerNumber : receiver_customer_number;
                cmd.Parameters.Add("@unic_number", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = DBNull.Value;
                cmd.Parameters.Add("@sender_account", SqlDbType.NVarChar, 25).Value = paymentOrder.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 25).Value = paymentOrder.ReceiverAccount.AccountNumber;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = paymentOrder.Id;
                cmd.Parameters.Add("@analys_subtype_id", SqlDbType.Int).Value = 90;

                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read() && dr["aml_check"] != DBNull.Value)
                {
                    amlCheck = Convert.ToInt16(dr["aml_check"]);
                }
            }

            return amlCheck;
        }

        internal static void SaveDAHKPaymentType(long orderId, int paymentType, int setNumber)
        {

            if (orderId != 0)
            {
                using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "pr_Set_DAHK_Payment_Type_For_HB";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@doc_ID", SqlDbType.BigInt).Value = orderId;
                cmd.Parameters.Add("@payment_type", SqlDbType.Int).Value = paymentType;
                cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;
                cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = DateTime.Now;

                cmd.ExecuteNonQuery();
            }
        }

        internal static Tuple<string, string> GetSintAccountForHB(string accountNumber)
        {
            string oldSintAccount = "";
            string newSintAccount = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string str = @"select type_of_account,type_of_account_new from [tbl_all_accounts;] where Arm_number = @arm_number";

                using SqlCommand cmd = new SqlCommand(str, conn);
                cmd.Parameters.Add("@arm_number", SqlDbType.NVarChar).Value = accountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    oldSintAccount = Convert.ToString(dr["type_of_account"]);
                    newSintAccount = Convert.ToString(dr["type_of_account_new"]);
                }
            }

            return Tuple.Create(oldSintAccount, newSintAccount);
        }

        internal static DateTime GetNormalEndOfDeposit(string productNumber)
        {

            DateTime depositEnd = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string str = @"select date_of_normal_end from [tbl_deposits;] where deposit_number = @product_number";

                using SqlCommand cmd = new SqlCommand(str, conn);
                cmd.Parameters.Add("@product_number", SqlDbType.NVarChar).Value = productNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    depositEnd = Convert.ToDateTime(dr["date_of_normal_end"]);
                }
            }

            return depositEnd;
        }

        internal static bool IsDebetExportAndImportCreditLine(string debAccountNumber)
        {
            bool res = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string str = @"select credit_line_type from tbl_credit_lines   where loan_full_number = @deb_account_number and credit_line_type = 60";

                using SqlCommand cmd = new SqlCommand(str, conn);
                cmd.Parameters.Add("@deb_account_number", SqlDbType.NVarChar).Value = debAccountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    res = true;

                }
            }

            return res;
        }
    }
}
