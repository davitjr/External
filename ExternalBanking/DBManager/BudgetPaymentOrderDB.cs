using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace ExternalBanking.DBManager
{
    internal static class BudgetPaymentOrderDB
    {
        internal static PaymentOrder GetPaymentOrder(BudgetPaymentOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select rs.description as ReasonIdDescription ,D.*, F.id as fee_ID, F.FeeAmount, F.Currency as FeeCurrency, F.FeeType, TD.* ,change_time, p.*
                                                            from Tbl_Hb_Documents D  left join Tbl_Transfer_Fees F on D.doc_ID = F.doc_ID left join Tbl_New_Transfer_Doc TD on D.doc_ID = TD.Doc_id   left join tbl_type_of_card_debit_reasons rs on d.reason_type = rs.type 
															LEFT JOIN (SELECT ID, violation_ID, violation_date, badge_number FROM Tbl_Police_Response_Details) p on D.police_response_details_ID = p.id
                                                                     outer apply (select top 1 CONVERT(VARCHAR(5),change_date, 108) change_time  from Tbl_HB_quality_history where doc_ID=D.doc_ID  order by quality )time
                                                            where D.doc_id=@id and customer_number=case when @customerNumber = 0 then customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            order.Id = long.Parse(dr["doc_id"].ToString());

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);


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

                            SourceType sourceType = (SourceType)short.Parse(dr["source_type"].ToString());
                            order.Source = sourceType;

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                ulong debitAccount = Convert.ToUInt64(dr["debet_account"]);
                                order.DebitAccount = Account.GetAccount(debitAccount);
                            }

                            order.ReceiverAccount = new Account();


                            order.ViolationID = dr["violation_ID"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["violation_ID"].ToString()) : string.Empty;
                            order.ViolationDate = dr["violation_date"] != DBNull.Value ? Convert.ToDateTime(dr["violation_date"]) : default(DateTime?);
                            order.BadgeNumber = dr["badge_number"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["badge_number"].ToString()) : string.Empty;
                            if (!string.IsNullOrEmpty(order.BadgeNumber))
                                order.IsPoliceInspectorAct = true;


                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();



                                if (order.Type == OrderType.RATransfer || order.Type == OrderType.CashForRATransfer)
                                {
                                    if ((order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline) || order.IsPoliceInspectorAct)
                                    {
                                        creditAccount = dr["credit_bank_code"].ToString() + creditAccount;
                                    }
                                }

                                if ((order.Type == OrderType.RATransfer && order.SubType == 3) || order.Type == OrderType.Convertation)
                                {
                                    order.ReceiverAccount = Account.GetAccount(ulong.Parse(creditAccount));
                                }
                                else
                                {
                                    order.ReceiverAccount.AccountNumber = creditAccount;
                                    order.ReceiverAccount.Currency = "";
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

                            if (dr["isExaction"] != DBNull.Value)
                                order.Exaction = Convert.ToBoolean(dr["isExaction"]);

                            if (dr["urgent"] != DBNull.Value)
                                order.UrgentSign = Convert.ToBoolean(dr["urgent"]);

                            if (dr["FeeAmount"] != DBNull.Value && Convert.ToInt16(dr["FeeType"]) == 1)
                            {
                                order.CardFee = Convert.ToDouble(dr["FeeAmount"]);
                                order.CardFeeCurrency = dr["FeeCurrency"].ToString();
                            }

                            if (dr["budj_transfer_reg_code"] != DBNull.Value)
                                order.LTACode = (short)(dr["budj_transfer_reg_code"]);

                            if (dr["police_code"] != DBNull.Value)
                                order.PoliceCode = Convert.ToInt32(dr["police_code"]);

                            if (dr["Soc_NoSoc_Number"] != DBNull.Value)
                            {
                                order.PayerDocumentNumber = dr["Soc_NoSoc_Number"].ToString();
                                order.PayerDocumentType = 1;
                            }

                            if (dr["NoSoc_number"] != DBNull.Value)
                            {
                                order.PayerDocumentNumber = dr["NoSoc_number"].ToString();
                                order.PayerDocumentType = 2;
                            }

                            if (dr["Passport_Number"] != DBNull.Value)
                            {
                                order.PayerDocumentNumber = dr["Passport_Number"].ToString();
                                order.PayerDocumentType = 3;
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

                            if (dr["Debitor_Death_Document"] != DBNull.Value)
                            {
                                order.CreditorDeathDocument = dr["Debitor_Death_Document"].ToString();

                            }
                            if (dr["change_time"] != DBNull.Value)
                            {
                                order.RegistrationTime = dr["change_time"].ToString();
                            }

                            if (dr["police_response_details_ID"] != DBNull.Value)
                                order.PoliceResponseDetailsId = Convert.ToInt32(dr["police_response_details_ID"]);
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);

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
        internal static ActionResult Save(BudgetPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

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
                        if (order.Type != OrderType.Convertation)
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);
                        }
                        else
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
                        }
                    }

                    if (order.Type == OrderType.Convertation)
                    {
                        cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = order.ConvertationRate;
                        cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = order.ConvertationRate1;
                    }

                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;

                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    if (source != SourceType.AcbaOnline && source != SourceType.MobileBanking)
                    {
                        if (!String.IsNullOrEmpty(order.ViolationID))
                            order.Description += ", որոշում " + order.ViolationID;
                        if (order.ViolationDate.HasValue)
                            order.Description += " " + order.ViolationDate.Value.ToString("dd/MM/yy");
                    }

                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 50).Value = order.Receiver;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;

                    if (order.CardFee != 0)
                    {
                        cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.CardFee;
                        cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = order.DebitAccount.Currency;
                        cmd.Parameters.Add("@fee_type", SqlDbType.TinyInt).Value = 7;
                    }

                    if (order.PayerDocumentType == 1 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
                    }
                    else if (order.PayerDocumentType == 2 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
                    }
                    else if (order.PayerDocumentType == 3 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
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

                    if (order.LTACode != 0)
                    {
                        cmd.Parameters.Add("@reg_code", SqlDbType.Int).Value = order.LTACode;
                    }

                    if (order.PoliceCode != 0)
                    {
                        cmd.Parameters.Add("@police_code", SqlDbType.Int).Value = order.PoliceCode;
                    }

                    if (order.CreditorStatus != 0)
                    {
                        cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = order.CreditorStatus;
                    }

                    if (order.CreditorDescription != null)
                    {
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = order.CreditorDescription;
                    }

                    if (order.CreditorDeathDocument != null)
                    {
                        cmd.Parameters.Add("@debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;
                    }

                    cmd.Parameters.Add("@debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                    if (order.PoliceResponseDetailsId != 0)
                    {
                        cmd.Parameters.Add("@police_response_details_ID", SqlDbType.Int).Value = order.PoliceResponseDetailsId;
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

                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    }

                    if (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0")
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    }


                    if (order.TransferFee != 0)
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;
                    }

                    if (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0" && order.TransferFee != 0)
                    {
                        cmd.Parameters.Add("@fee_currency3", SqlDbType.NVarChar).Value = order.FeeAccount.Currency;
                        cmd.Parameters.Add("@fee_type3", SqlDbType.TinyInt).Value = 20;
                    }

                    cmd.Parameters.Add("@urgent", SqlDbType.TinyInt).Value = Convert.ToByte(order.UrgentSign);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@isExaction", SqlDbType.TinyInt).Value = Convert.ToByte(order.Exaction);

                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("username", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    if (order.IsPoliceInspectorAct)
                    {
                        cmd.Parameters.Add("@violation_id", SqlDbType.NVarChar, 20).Value = order.ViolationID;
                        cmd.Parameters.Add("@violation_date", SqlDbType.DateTime).Value = order.ViolationDate;
                        cmd.Parameters.Add("@badge_number", SqlDbType.NVarChar, 20).Value = order.BadgeNumber;
                    }

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

        internal static int CheckTransferVerification(double accountNumber, int LTACode, int cust_type, string TIN = "", int creditorCustomerType = 0, string creditorTIN = "")
        {

            int errorCode = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"sp_transfer_verification", conn))
                {
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

                    errorCode = Convert.ToInt32(cmd.Parameters["@result"].Value);
                }
            }

            return errorCode;
        }
        internal static ActionResult SaveCash(BudgetPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"	declare @filial as int
                                                    if @customer_number<>0
                                                    begin
                                                        select @filial=filialcode from Tbl_customers where customer_number=@customer_number
                                                    end
                                                    else
                                                    begin 
                                                        set @filial=0
                                                    end    
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    [description],quality,receiver_name,credit_bank_code,rate_sell_buy,rate_sell_buy_cross,
                                                    urgent,source_type,operationFilialCode,operation_date,budj_transfer_reg_code,police_code,police_response_details_ID)
                                                    VALUES
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,@descr,1,@receiver_name,@credit_bank_code,@cust_rate,@cross_rate,
                                                    @urgent,@source_type,@operationFilialCode,@oper_day,@reg_code,@police_code,@police_response_details_ID)
                                                    DECLARE @id as float
                                                    SET @id=Scope_identity()
                                                    Select Scope_identity() as ID
                                                    if (@document_subtype=2 or @document_subtype = 5 or @document_subtype = 6) and @doc_type=56
													BEGIN
														INSERT INTO dbo.tbl_new_transfer_doc (doc_id, Soc_NoSoc_Number, Passport_Number, SintAccDetailsValue, DebitorName,
                                                        Debitor_SocNoSoc, DebitorHVHH, Debitor_Passport_Number, NoSoc_number, Debitor_NoSoc_Number,Debitor_Customer_Number,Debitor_Death_Document) 
														VALUES (@id, @Soc_NoSoc_Number,@Passport_Number, @SintAccDetailsValue, @DebitorName, @Debitor_SocNoSoc, 
                                                        @DebitorHVHH , @Debitor_Passport_Number, @NoSoc_number, @Debitor_NoSoc_Number,@Debitor_Customer_Number,@Debitor_Death_Document)
												    END
                                            ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    string creditAccountNumber;
                    if (order.ReceiverBankCode == 10300 && order.ReceiverAccount.AccountNumber.ToString().Substring(0, 1) == "9")
                    {
                        creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
                    }
                    else
                    {
                        if (order.Type != OrderType.Convertation && order.Type != OrderType.CashCredit && order.Type != OrderType.CashConvertation && order.Type != OrderType.CashCreditConvertation)
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);
                        }
                        else
                        {
                            creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
                        }
                    }

                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;

                    if (!String.IsNullOrEmpty(order.ViolationID) && order.Description != null)
                        order.Description += ", որոշում " + order.ViolationID;
                    if (order.ViolationDate.HasValue && order.Description != null)
                        order.Description += " " + order.ViolationDate.Value.ToString("dd/MM/yy");

                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description == null ? "" : order.Description;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 50).Value = order.Receiver == null ? "" : order.Receiver;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;
                    if (order.Type == OrderType.Convertation || order.Type == OrderType.CashConvertation || order.Type == OrderType.CashCreditConvertation || order.Type == OrderType.CashDebitConvertation)
                    {
                        cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = order.ConvertationRate;
                        cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = order.ConvertationRate1;
                    }
                    else
                    {
                        cmd.Parameters.Add("@cust_rate", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@cross_rate", SqlDbType.Float).Value = DBNull.Value;
                    }
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Parameters.Add("@urgent", SqlDbType.TinyInt).Value = Convert.ToByte(order.UrgentSign);
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    if (order.PayerDocumentType == 1 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    }
                    else if (order.PayerDocumentType == 2 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                    }
                    else if (order.PayerDocumentType == 3 && order.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = order.PayerDocumentNumber;
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = DBNull.Value;
                    }

                    if (order.CreditorDocumentType == 1 && order.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        if (order.CreditorDeathDocument == null)
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = DBNull.Value;
                        else
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;
                        cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;
                    }
                    else if (order.CreditorDocumentType == 2 && order.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        if (order.CreditorDeathDocument == null)
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = DBNull.Value;
                        else
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;

                        cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                    }
                    else if (order.CreditorDocumentType == 3 && order.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                        if (order.CreditorDeathDocument == null)
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = DBNull.Value;
                        else
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;

                        cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                    }
                    else if (order.CreditorDocumentType == 4 && order.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = order.CreditorDocumentNumber;
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        if (order.CreditorDeathDocument == null)
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = DBNull.Value;
                        else
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;

                        cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                    }
                    else if (order.CreditorDocumentType == 0 || string.IsNullOrEmpty(order.CreditorDocumentNumber))
                    {
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = DBNull.Value;
                        if (order.CreditorDeathDocument == null)
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = DBNull.Value;
                        else
                            cmd.Parameters.Add("@Debitor_Death_Document", SqlDbType.NVarChar).Value = order.CreditorDeathDocument;

                        cmd.Parameters.Add("@Debitor_Customer_Number", SqlDbType.NVarChar).Value = order.CreditorCustomerNumber;

                    }
                    if (order.LTACode != 0)
                    {
                        cmd.Parameters.Add("@reg_code", SqlDbType.Int).Value = order.LTACode;
                    }
                    else
                    {
                        cmd.Parameters.Add("@reg_code", SqlDbType.Int).Value = DBNull.Value;
                    }
                    if (order.PoliceCode != 0)
                    {
                        cmd.Parameters.Add("@police_code", SqlDbType.Int).Value = order.PoliceCode;
                    }
                    else
                    {
                        cmd.Parameters.Add("@police_code", SqlDbType.Int).Value = DBNull.Value;
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
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar, 100).Value = order.CreditorDescription;
                    }
                    else
                    {
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = DBNull.Value;
                    }


                    if (order.PoliceResponseDetailsId != 0)
                    {
                        cmd.Parameters.Add("@police_response_details_ID", SqlDbType.Int).Value = order.PoliceResponseDetailsId;
                    }
                    else
                    {
                        cmd.Parameters.Add("@police_response_details_ID", SqlDbType.Int).Value = DBNull.Value;
                    }

                    order.Id = Convert.ToInt64(cmd.ExecuteScalar());
                }


                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }

        internal static ActionResult SaveBudgetOrderPaymentDetails(BudgetPaymentOrder budgetPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_payment_budget_details(order_id, LTA_code, urgent, creditor_status, police_code) VALUES(@orderId, @LTACode, @urgent, @creditorStatus, @policeCode)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@LTACode", SqlDbType.SmallInt).Value = budgetPaymentOrder.LTACode;
                    cmd.Parameters.Add("@urgent", SqlDbType.Int).Value = budgetPaymentOrder.UrgentSign;
                    cmd.Parameters.Add("@creditorStatus", SqlDbType.Int).Value = budgetPaymentOrder.CreditorStatus;
                    cmd.Parameters.Add("@policeCode", SqlDbType.Int).Value = budgetPaymentOrder.PoliceCode;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static int GetPoliceResponseDetailsIDWithoutRequest(string violationID, DateTime? violationDate, string badgeNumber, int responseDetailsID = 0)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_police_response_details_without_request";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@violation_id", SqlDbType.NVarChar, 50).Value = violationID;
                    cmd.Parameters.Add("@violation_date", SqlDbType.SmallDateTime).Value = violationDate;
                    cmd.Parameters.Add("@badge_number", SqlDbType.NVarChar, 50).Value = badgeNumber;
                    if (responseDetailsID != 0)
                        cmd.Parameters.Add("@details_id", SqlDbType.Int).Value = responseDetailsID;
                    SqlParameter param = new SqlParameter("@response_details_ID", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                    int id = Convert.ToInt32(cmd.Parameters["@response_details_ID"].Value);

                    return id;
                }
            }
        }

        internal static string GetOrderViolationId(long policeResponseId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"select violation_id 
                                        from[dbo].[Tbl_Police_Response_Details]
                                        where ID = @policeResponseId";
                cmd.Parameters.Add("@policeResponseId", SqlDbType.Int).Value = policeResponseId;
                conn.Open();
                using SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                    return rd["violation_id"].ToString();
            }
            return null;
        }
    }
}
