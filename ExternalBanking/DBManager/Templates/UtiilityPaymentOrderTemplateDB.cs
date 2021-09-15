using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    internal class UtiilityPaymentOrderTemplateDB
    {
        internal static ActionResult SaveUtilityPaymentOrderTemplate(UtilityPaymentOrderTemplate template, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_utility_payment_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;

                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = template.UtilityPaymentOrder.DebitAccount.AccountNumber;

                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = template.UtilityPaymentOrder.Amount;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;

                    cmd.Parameters.Add("@service_amount", SqlDbType.Float).Value = template.UtilityPaymentOrder.ServiceAmount;
                    cmd.Parameters.Add("@cod", SqlDbType.NVarChar).Value = template.UtilityPaymentOrder.Code;
                    cmd.Parameters.Add("@comunal_type", SqlDbType.Int).Value = template.UtilityPaymentOrder.CommunalType;
                    cmd.Parameters.Add("@branch", SqlDbType.Int).Value = template.UtilityPaymentOrder.Branch;
                    cmd.Parameters.Add("@abonent_type", SqlDbType.TinyInt).Value = template.UtilityPaymentOrder.AbonentType;
                    cmd.Parameters.Add("@isPrepaid", SqlDbType.Bit).Value = template.UtilityPaymentOrder.PrepaidSign;
                    if (template.UtilityPaymentOrder.CommunalType == CommunalTypes.COWater)
                    {
                        cmd.Parameters.Add("@service_provided_filialcode", SqlDbType.SmallInt).Value = template.UtilityPaymentOrder.AbonentFilialCode;
                        cmd.Parameters.Add("@payment_type", SqlDbType.SmallInt).Value = template.UtilityPaymentOrder.PaymentType;
                    }

                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.UtilityPaymentOrder.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.UtilityPaymentOrder.Type;
                    cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = template.UtilityPaymentOrder.UseCreditLine;


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


        internal static UtilityPaymentOrderTemplate GetUtilityPaymentOrderTemplate(int templateId, ulong customerNumber)
        {
            UtilityPaymentOrderTemplate template = new UtilityPaymentOrderTemplate();
            template.UtilityPaymentOrder = new UtilityPaymentOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select T.id as templateId, T.description as TemplateDescription, T.customer_number, T.serial_number, T.registration_date, T.document_type, T.document_subtype, T.status, 
                                        T.type, T.order_group_id, T.source_type, T.amount, T.debet_account, T.user_id, P.*  
                                        FROM Tbl_Templates T  inner join [Tbl_Utility_Payment_Order_Templates] P on T.id = P.template_id                                                                       
                                        WHERE T.id = @id and T.customer_number = @customer_number";

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = templateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //Ձևանմուշ
                            template.ID = Convert.ToInt32(dr["templateId"]);

                            template.TemplateName = dr.FieldOrDefault<string>("TemplateDescription");
                            template.TemplateRegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            template.TemplateCustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                            template.SerialNumber = Convert.ToInt32(dr["serial_number"].ToString());
                            template.TemplateDocumentType = (OrderType)(dr["document_type"]);
                            template.TemplateDocumentSubType = Convert.ToByte(dr["document_subtype"]);
                            template.Status = (TemplateStatus)Convert.ToByte(dr["status"]);
                            template.TemplateType = (TemplateType)Convert.ToByte(dr["type"]);
                            template.ChangeUserId = Convert.ToInt32(dr["user_id"].ToString());

                            if (dr["order_group_id"] != DBNull.Value)
                            {
                                template.TemplateGroupId = Convert.ToInt32(dr["order_group_id"].ToString());
                                template.UtilityPaymentOrder.GroupId = template.TemplateGroupId;
                            }

                            template.TemplateSourceType = (SourceType)Convert.ToByte(dr["source_type"]);



                            if (dr["amount"] != DBNull.Value)
                            {
                                template.TemplateAmount = Convert.ToDouble(dr["amount"].ToString());
                                template.UtilityPaymentOrder.Amount = Convert.ToDouble(dr["amount"].ToString());
                            }

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();
                                template.TemplateDebetAccount = new Account();
                                template.TemplateDebetAccount.AccountNumber = debitAccount;

                                template.UtilityPaymentOrder.DebitAccount = Account.GetAccount(debitAccount);
                            }

                            //Ձևանմուշի մանրամասներ
                            template.UtilityPaymentOrder.Id = long.Parse(dr["id"].ToString());
                            template.UtilityPaymentOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));
                            template.UtilityPaymentOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                            if (dr["service_amount"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.ServiceAmount = Convert.ToDouble(dr["service_amount"].ToString());
                            }

                            if (dr["cod"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.Code = dr["cod"].ToString();
                            }

                            if (dr["branch"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.Branch = dr["branch"].ToString();
                            }

                            if (dr["abonent_type"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.AbonentType = Convert.ToInt32(dr["abonent_type"].ToString());
                            }
                            if (dr["comunal_type"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.CommunalType = (CommunalTypes)Convert.ToInt32(dr["comunal_type"].ToString());
                            }

                            if (dr["prepaid"] != DBNull.Value)
                            {
                                var a = Convert.ToBoolean(dr["prepaid"].ToString());
                                template.UtilityPaymentOrder.PrepaidSign = Convert.ToInt32(a);
                            }

                            if (dr["payment_type"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.PaymentType = Convert.ToUInt16(dr["payment_type"]);
                            }

                            if (dr["service_provided_filialcode"] != DBNull.Value)
                            {
                                template.UtilityPaymentOrder.AbonentFilialCode = Convert.ToUInt16(dr["service_provided_filialcode"]);
                            }

                            template.UtilityPaymentOrder.UseCreditLine = dr["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dr["use_credit_line"]) : false;

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
