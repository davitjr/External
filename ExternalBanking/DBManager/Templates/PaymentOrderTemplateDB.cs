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
    internal class PaymentOrderTemplateDB
    {
        internal static ActionResult SavePaymentOrderTemplate(PaymentOrderTemplate template, Action action)
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

                    string creditAccountNumber="";
                   
                        
                    if (template.PaymentOrder.ReceiverBankCode == 10300 && template.PaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(0, 1) == "9")
                    {
                        creditAccountNumber = template.PaymentOrder.ReceiverAccount.AccountNumber.ToString();
                    }
                    else
                    {
                        creditAccountNumber = template.PaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }
                   

                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = template.PaymentOrder.Description;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar,50).Value = template.PaymentOrder.Receiver;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = template.PaymentOrder.DebitAccount.AccountNumber;
                    
                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar).Value = creditAccountNumber;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar).Value = template.PaymentOrder.ReceiverBankCode.ToString();
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    if (template.TemplateType != TemplateType.CreatedByCustomer)
                    {
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = template.PaymentOrder.Amount;
                    }
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;
                    cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = template.PaymentOrder.UseCreditLine;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.PaymentOrder.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.PaymentOrder.Type;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;


                    if (template.PaymentOrder.CreditorDocumentType == 1 && template.PaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = template.PaymentOrder.CreditorDocumentNumber;
                    }
                    else if (template.PaymentOrder.CreditorDocumentType == 2 && template.PaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = template.PaymentOrder.CreditorDocumentNumber;
                    }
                    else if (template.PaymentOrder.CreditorDocumentType == 3 && template.PaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = template.PaymentOrder.CreditorDocumentNumber;
                    }

                    else if (template.PaymentOrder.CreditorDocumentType == 4 && template.PaymentOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = template.PaymentOrder.CreditorDocumentNumber;
                    }

               

                    if (template.PaymentOrder.CreditorStatus != 0)
                    {
                        cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = template.PaymentOrder.CreditorStatus;
                    }

                    if (template.PaymentOrder.CreditorDescription != null)
                    {
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = template.PaymentOrder.CreditorDescription;
                    }

                    if(template.TemplateType == TemplateType.CreatedAsGroupService)
                    {
                        cmd.Parameters.Add("@fee_acc", SqlDbType.NVarChar,50).Value = template.PaymentOrder.FeeAccount.AccountNumber;
                        cmd.Parameters.Add("@Transfer_Fee", SqlDbType.Float).Value = template.PaymentOrder.TransferFee;
                    }

                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = template.ID });

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

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
        /// Փոխանցում ՀՀ տարածքում/սեփական հաշիվների միջև ձևանմուշի ստացում
        /// </summary>
        /// <param name="paymentOrderTemplateId">Ձևանմուշի ունիկալ համար</param>
        /// <returns></returns>
        internal static PaymentOrderTemplate GetPaymentOrderTemplate(int paymentOrderTemplateId, ulong customerNumber)
        {
            PaymentOrderTemplate template = new PaymentOrderTemplate();
            template.PaymentOrder = new PaymentOrder();

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

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = paymentOrderTemplateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //Ձևանմուշ
                            template.ID = Convert.ToInt32((dr["templateId"]));
                            template.TemplateName = Utility.ConvertAnsiToUnicode(dr["TemplateDescription"].ToString());
                            template.TemplateRegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            template.TemplateCustomerNumber = Convert.ToUInt64((dr["customer_number"]));
                            template.SerialNumber = Convert.ToInt32(dr["serial_number"].ToString());
                            template.TemplateDocumentType = (OrderType)(dr["document_type"]);
                            template.TemplateDocumentSubType = Convert.ToByte((dr["document_subtype"]));
                            template.Status = (TemplateStatus)Convert.ToInt32((dr["status"]));
                            template.TemplateType = (TemplateType)Convert.ToInt32((dr["type"]));
                            template.ChangeUserId = Convert.ToInt32(dr["user_id"].ToString());

                            if (dr["order_group_id"] != DBNull.Value)
                            {
                                template.TemplateGroupId = Convert.ToInt32(dr["order_group_id"].ToString());
                                template.PaymentOrder.GroupId = template.TemplateGroupId;
                            }

                            template.TemplateSourceType = (SourceType)Convert.ToInt32((dr["source_type"]));

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
                            template.PaymentOrderTemplateId = Convert.ToInt32(dr["id"].ToString());

                            template.PaymentOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.PaymentOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                            template.TemplateDocumentType = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.TemplateDocumentSubType = Convert.ToByte((dr["document_subtype"]));


                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();
                                
                                template.PaymentOrder.DebitAccount = Account.GetAccount(debitAccount);
                            }


                            template.PaymentOrder.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();

                                if (template.PaymentOrder.Type == OrderType.RATransfer || template.PaymentOrder.Type == OrderType.CashDebit || template.PaymentOrder.Type == OrderType.CashForRATransfer || template.PaymentOrder.Type == OrderType.ReestrTransferOrder)
                                    creditAccount = dr["credit_bank_code"].ToString() + creditAccount;


                                if ((template.PaymentOrder.Type == OrderType.RATransfer && template.PaymentOrder.SubType == 3) || template.PaymentOrder.Type == OrderType.Convertation || template.PaymentOrder.Type == OrderType.CashDebit ||
                                    template.PaymentOrder.Type == OrderType.CashDebitConvertation || template.PaymentOrder.Type == OrderType.InBankConvertation || template.PaymentOrder.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                                    || template.PaymentOrder.Type == OrderType.TransitNonCashOut || template.PaymentOrder.Type == OrderType.ReestrTransferOrder)
                                {
                                    template.PaymentOrder.ReceiverAccount = Account.GetAccount(creditAccount);
                                }
                               
                                else
                                {
                                    template.PaymentOrder.ReceiverAccount.AccountNumber = creditAccount;
                                    template.PaymentOrder.ReceiverAccount.OpenDate = default(DateTime?);    
                                    template.PaymentOrder.ReceiverAccount.ClosingDate = default(DateTime?); 
                                    template.PaymentOrder.ReceiverAccount.FreezeDate = default(DateTime?);  
                                }
                            }

                            if (dr["receiver_name"] != DBNull.Value)
                                template.PaymentOrder.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                            if (dr["credit_bank_code"] != DBNull.Value)
                                template.PaymentOrder.ReceiverBankCode = Convert.ToInt32(dr["credit_bank_code"]);

                            if (dr["amount"] != DBNull.Value)
                                template.PaymentOrder.Amount = Convert.ToDouble(dr["amount"]);

                            //if (dr["currency"] != DBNull.Value)
                              //  template.PaymentOrder.Currency = dr["currency"].ToString();


                            template.PaymentOrder.SubType = Convert.ToByte(dr["document_subtype"]);



                            if (dr["description"] != DBNull.Value)
                                template.PaymentOrder.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            //if (dr["rate_sell_buy"] != DBNull.Value)
                            //    template.PaymentOrder.ConvertationRate = Convert.ToDouble(dr["rate_sell_buy"]);

                            //if (dr["rate_sell_buy_cross"] != DBNull.Value)
                            //    template.PaymentOrder.ConvertationRate1 = Convert.ToDouble(dr["rate_sell_buy_cross"]);
                           

                            if (dr["SintAccDetailsValue"] != DBNull.Value)
                            {
                                template.PaymentOrder.CreditorStatus = Convert.ToInt32(dr["SintAccDetailsValue"]);
                                template.PaymentOrder.CreditorStatusDescription = Info.GetSyntheticStatus(template.PaymentOrder.CreditorStatus.ToString());
                                template.PaymentOrder.ForThirdPerson = true;
                            }


                            if (dr["Debitor_Name"] != DBNull.Value)
                                template.PaymentOrder.CreditorDescription = dr["Debitor_Name"].ToString();

                            if (dr["Debitor_SocNoSoc"] != DBNull.Value)
                            {
                                template.PaymentOrder.CreditorDocumentNumber = dr["Debitor_SocNoSoc"].ToString();
                                template.PaymentOrder.CreditorDocumentType = 1;
                            }

                            if (dr["Debitor_NoSoc_Number"] != DBNull.Value)
                            {
                                template.PaymentOrder.CreditorDocumentNumber = dr["Debitor_NoSoc_Number"].ToString();
                                template.PaymentOrder.CreditorDocumentType = 2;
                            }

                            if (dr["Debitor_Passport_Number"] != DBNull.Value)
                            {
                                template.PaymentOrder.CreditorDocumentNumber = dr["Debitor_Passport_Number"].ToString();
                                template.PaymentOrder.CreditorDocumentType = 3;
                            }

                            if (dr["Debitor_HVHH"] != DBNull.Value)
                            {
                                template.PaymentOrder.CreditorDocumentNumber = dr["Debitor_HVHH"].ToString();
                                template.PaymentOrder.CreditorDocumentType = 4;
                            }


                            template.PaymentOrder.UseCreditLine = dr["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dr["use_credit_line"]) : false;
                        }
                        else
                        {
                            template = null;
                        }
                    }
                }
            }
            
            template.PaymentOrder.RegistrationDate = DateTime.Now;   //default(DateTime?)

            return template;
        }
    }
}
