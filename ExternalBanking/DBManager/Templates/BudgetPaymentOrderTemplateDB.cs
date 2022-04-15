using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    internal class BudgetPaymentOrderTemplateDB
    {
        internal static ActionResult SaveBudgetPaymentOrderTemplate(BudgetPaymentOrderTemplate template, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_payment_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;

                    string creditAccountNumber = "";


                    if (template.BudgetPaymentOrder.ReceiverBankCode == 10300 && template.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(0, 1) == "9")
                    {
                        creditAccountNumber = template.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString();
                    }
                    else
                    {
                        creditAccountNumber = template.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }


                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = template.BudgetPaymentOrder.DebitAccount.AccountNumber;

                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.Description;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 500).Value = template.BudgetPaymentOrder.Receiver;
                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar).Value = creditAccountNumber;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar).Value = template.BudgetPaymentOrder.ReceiverBankCode.ToString();
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.TemplateDocumentSubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.TemplateDocumentType;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = template.BudgetPaymentOrder.Amount;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;

                    if (template.BudgetPaymentOrder.PayerDocumentType == 1 && template.BudgetPaymentOrder.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Soc_NoSoc_Number", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.PayerDocumentNumber;
                    }
                    else if (template.BudgetPaymentOrder.PayerDocumentType == 2 && template.BudgetPaymentOrder.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@NoSoc_number", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.PayerDocumentNumber;
                    }
                    else if (template.BudgetPaymentOrder.PayerDocumentType == 3 && template.BudgetPaymentOrder.PayerDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Passport_Number", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.PayerDocumentNumber;
                    }

                    if (template.BudgetPaymentOrder.LTACode != 0)
                    {
                        cmd.Parameters.Add("@reg_code", SqlDbType.Int).Value = template.BudgetPaymentOrder.LTACode;
                    }

                    if (template.BudgetPaymentOrder.CreditorDocumentType == 1 && template.BudgetPaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.CreditorDocumentNumber;
                    }
                    else if (template.BudgetPaymentOrder.CreditorDocumentType == 2 && template.BudgetPaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.CreditorDocumentNumber;
                    }
                    else if (template.BudgetPaymentOrder.CreditorDocumentType == 3 && template.BudgetPaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.CreditorDocumentNumber;
                    }

                    else if (template.BudgetPaymentOrder.CreditorDocumentType == 4 && template.BudgetPaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.CreditorDocumentNumber;
                    }


                    if (template.BudgetPaymentOrder.CreditorStatus != 0)
                    {
                        cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = template.BudgetPaymentOrder.CreditorStatus;
                    }

                    if (template.BudgetPaymentOrder.CreditorDescription != null)
                    {
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = template.BudgetPaymentOrder.CreditorDescription;
                    }

                    if (template.BudgetPaymentOrder.PoliceCode != 0)
                    {
                        cmd.Parameters.Add("@police_code", SqlDbType.Int).Value = template.BudgetPaymentOrder.PoliceCode;
                    }

                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = template.ID });

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    if (template.TemplateType == TemplateType.CreatedAsGroupService)
                    {
                        cmd.Parameters.Add("@fee_acc", SqlDbType.NVarChar, 50).Value = template.BudgetPaymentOrder.FeeAccount.AccountNumber;
                        cmd.Parameters.Add("@Transfer_Fee", SqlDbType.Float).Value = template.BudgetPaymentOrder.TransferFee;
                    }

                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                    template.ID = id;

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
            }

            return result;
        }

        /// <summary>
        /// Բյուջե փոխանցման ձևանմուշի ստացում
        /// </summary>
        /// <param name="budgetPaymentOrderTemplateId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static BudgetPaymentOrderTemplate GetBudgetPaymentOrderTemplate(int budgetPaymentOrderTemplateId, ulong customerNumber)
        {
            BudgetPaymentOrderTemplate template = new BudgetPaymentOrderTemplate();
            template.BudgetPaymentOrder = new BudgetPaymentOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select T.id as templateId, T.description as TemplateDescription, T.customer_number, T.serial_number, T.registration_date, T.document_type, T.document_subtype, T.status, 
                                        T.type, T.order_group_id, T.source_type, T.amount, T.debet_account, T.user_id, P.*  
                                        FROM Tbl_Templates T  inner join Tbl_PaymentOrder_Template_Details P on T.id = P.template_id                                                                       
                                        WHERE T.id = @id and T.customer_number = @customer_number";

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = budgetPaymentOrderTemplateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //Ձևանմուշ
                            template.ID = Convert.ToInt32((dr["templateId"]));
                            template.TemplateName = Utility.ConvertAnsiToUnicode(dr["TemplateDescription"].ToString());
                            template.TemplateRegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            template.TemplateCustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());
                            template.SerialNumber = Convert.ToInt32(dr["serial_number"].ToString());
                            template.TemplateDocumentType = (OrderType)(dr["document_type"]);
                            template.TemplateDocumentSubType = Convert.ToByte(dr["document_subtype"].ToString());
                            template.Status = (TemplateStatus)Convert.ToInt16(dr["status"].ToString());
                            template.TemplateType = (TemplateType)Convert.ToInt16(dr["type"].ToString());
                            template.ChangeUserId = Convert.ToInt32(dr["user_id"].ToString());

                            if (dr["order_group_id"] != DBNull.Value)
                            {
                                template.TemplateGroupId = Convert.ToInt32(dr["order_group_id"].ToString());
                                template.BudgetPaymentOrder.GroupId = template.TemplateGroupId;
                            }

                            template.TemplateSourceType = (SourceType)Convert.ToInt16(dr["source_type"].ToString());

                            if (dr["amount"] != DBNull.Value)
                            {
                                template.TemplateAmount = Convert.ToDouble(dr["amount"].ToString());
                            }

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                template.TemplateDebetAccount = new Account();
                                template.TemplateDebetAccount.AccountNumber = dr["debet_account"].ToString();
                            }

                            //Ձևանմուշի մանրամասներ
                            template.BudgetPaymentOrderTemplateId = Convert.ToInt32(dr["id"].ToString());

                            template.BudgetPaymentOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.BudgetPaymentOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                            template.TemplateDocumentType = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.TemplateDocumentSubType = Convert.ToByte((dr["document_subtype"]));


                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();

                                template.BudgetPaymentOrder.DebitAccount = Account.GetAccount(debitAccount);
                            }


                            template.BudgetPaymentOrder.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();

                                if (template.BudgetPaymentOrder.Type == OrderType.RATransfer || template.BudgetPaymentOrder.Type == OrderType.CashDebit || template.BudgetPaymentOrder.Type == OrderType.CashForRATransfer || template.BudgetPaymentOrder.Type == OrderType.ReestrTransferOrder)
                                    creditAccount = dr["credit_bank_code"].ToString() + creditAccount;


                                if ((template.BudgetPaymentOrder.Type == OrderType.RATransfer && template.BudgetPaymentOrder.SubType == 3) || template.BudgetPaymentOrder.Type == OrderType.Convertation || template.BudgetPaymentOrder.Type == OrderType.CashDebit ||
                                    template.BudgetPaymentOrder.Type == OrderType.CashDebitConvertation || template.BudgetPaymentOrder.Type == OrderType.InBankConvertation || template.BudgetPaymentOrder.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                                    || template.BudgetPaymentOrder.Type == OrderType.TransitNonCashOut || template.BudgetPaymentOrder.Type == OrderType.ReestrTransferOrder)
                                {
                                    template.BudgetPaymentOrder.ReceiverAccount = Account.GetAccount(creditAccount);
                                }

                                else
                                {
                                    template.BudgetPaymentOrder.ReceiverAccount.AccountNumber = creditAccount;
                                    template.BudgetPaymentOrder.ReceiverAccount.OpenDate = default(DateTime?);
                                    template.BudgetPaymentOrder.ReceiverAccount.ClosingDate = default(DateTime?);
                                    template.BudgetPaymentOrder.ReceiverAccount.FreezeDate = default(DateTime?);
                                }
                            }

                            if (dr["receiver_name"] != DBNull.Value)
                                template.BudgetPaymentOrder.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                            if (dr["credit_bank_code"] != DBNull.Value)
                                template.BudgetPaymentOrder.ReceiverBankCode = Convert.ToInt32(dr["credit_bank_code"]);

                            if (dr["amount"] != DBNull.Value)
                                template.BudgetPaymentOrder.Amount = Convert.ToDouble(dr["amount"]);

                            //if (dr["currency"] != DBNull.Value)
                            //    template.BudgetPaymentOrder.Currency = dr["currency"].ToString();


                            template.BudgetPaymentOrder.SubType = Convert.ToByte(dr["document_subtype"]);

                            if (dr["description"] != DBNull.Value)
                                template.BudgetPaymentOrder.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            if (dr["SintAccDetailsValue"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.CreditorStatus = Convert.ToInt32(dr["SintAccDetailsValue"]);
                                template.BudgetPaymentOrder.CreditorStatusDescription = Info.GetSyntheticStatus(template.BudgetPaymentOrder.CreditorStatus.ToString());
                                template.BudgetPaymentOrder.ForThirdPerson = true;
                            }


                            if (dr["Debitor_Name"] != DBNull.Value)
                                template.BudgetPaymentOrder.CreditorDescription = Utility.ConvertAnsiToUnicode(dr["Debitor_Name"].ToString());

                            if (dr["Debitor_SocNoSoc"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.CreditorDocumentNumber = dr["Debitor_SocNoSoc"].ToString();
                                template.BudgetPaymentOrder.CreditorDocumentType = 1;
                            }

                            if (dr["Debitor_NoSoc_Number"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.CreditorDocumentNumber = dr["Debitor_NoSoc_Number"].ToString();
                                template.BudgetPaymentOrder.CreditorDocumentType = 2;
                            }

                            if (dr["Debitor_Passport_Number"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.CreditorDocumentNumber = dr["Debitor_Passport_Number"].ToString();
                                template.BudgetPaymentOrder.CreditorDocumentType = 3;
                            }

                            if (dr["Debitor_HVHH"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.CreditorDocumentNumber = dr["Debitor_HVHH"].ToString();
                                template.BudgetPaymentOrder.CreditorDocumentType = 4;
                            }

                            if (dr["police_code"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.PoliceCode = Convert.ToInt32(dr["police_code"]);
                            }

                            if (dr["reg_code"] != DBNull.Value)
                                template.BudgetPaymentOrder.LTACode = Convert.ToInt16(dr["reg_code"].ToString());

                            if (dr["Soc_NoSoc_Number"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.PayerDocumentNumber = dr["Soc_NoSoc_Number"].ToString();
                                template.BudgetPaymentOrder.PayerDocumentType = 1;
                            }

                            if (dr["NoSoc_number"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.PayerDocumentNumber = dr["NoSoc_number"].ToString();
                                template.BudgetPaymentOrder.PayerDocumentType = 2;
                            }

                            if (dr["Passport_Number"] != DBNull.Value)
                            {
                                template.BudgetPaymentOrder.PayerDocumentNumber = dr["Passport_Number"].ToString();
                                template.BudgetPaymentOrder.PayerDocumentType = 3;
                            }


                            template.BudgetPaymentOrder.UseCreditLine = dr["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dr["use_credit_line"]) : false;
                        }
                        else
                        {
                            template = null;
                        }
                    }
                }
            }
            return template;
        }
    }
}
