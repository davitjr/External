using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    internal class CardToCardOrderTemplateDB
    {
        internal static ActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplate template, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_card_to_card_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = OrderType.CardToCardOrder;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.TemplateDocumentSubType;
                    cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = template.Status;
                    cmd.Parameters.Add("@debit_card_number", SqlDbType.NVarChar).Value = template.CardToCardOrder.DebitCardNumber;
                    cmd.Parameters.Add("@credit_card_number", SqlDbType.NVarChar).Value = template.CardToCardOrder.CreditCardNumber;
                    cmd.Parameters.Add("@is_our_card", SqlDbType.NVarChar).Value = template.CardToCardOrder.IsOurCard;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = template.CardToCardOrder.Currency;

                    if (template.TemplateType != TemplateType.CreatedByCustomer)
                    {
                        cmd.Parameters.Add("@amount", SqlDbType.NVarChar).Value = template.CardToCardOrder.Amount;
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

        internal static CardToCardOrderTemplate GetCardToCardOrderTemplate(int templateId, ulong customerNumber)
        {
            CardToCardOrderTemplate template = new CardToCardOrderTemplate();
            template.CardToCardOrder = new CardToCardOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select T.id as templateId, T.description as TemplateDescription, T.customer_number, T.serial_number, T.registration_date, T.document_type, T.document_subtype, T.status, 
                                                                        T.type, T.order_group_id, T.source_type, T.amount, T.debet_account, T.user_id, P.*  
                                                            FROM Tbl_Templates T
                                                                        INNER JOIN tbl_cardtocard_order_template_details P on T.id = P.template_id       
                                                            WHERE T.id = @id and T.customer_number = @customer_number";

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = templateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //Ձևանմուշ
                            template.ID = Convert.ToInt32((dr["templateId"]));
                            template.TemplateName = dr["TemplateDescription"].ToString();
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
                            }

                            template.TemplateSourceType = (SourceType)Convert.ToInt16(dr["source_type"].ToString());

                            if (dr["amount"] != DBNull.Value)
                            {
                                template.TemplateAmount = Convert.ToDouble(dr["amount"].ToString());
                            }


                            //Ձևանմուշի մանրամասներ
                            template.CardToCardOrderTemplateId = Convert.ToInt32(dr["id"].ToString());

                            template.CardToCardOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.CardToCardOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                            template.TemplateDocumentType = (OrderType)Convert.ToInt16((dr["document_type"]));

                            template.TemplateDocumentSubType = Convert.ToByte((dr["document_subtype"]));


                            // CardToCard օբյեկտի արժեքավորում
                            template.CardToCardOrder.DebitCardNumber = dr["debit_card_number"].ToString();
                            template.CardToCardOrder.CreditCardNumber = dr["credit_card_number"].ToString();
                            template.CardToCardOrder.EmbossingName = dr["embossing_name"].ToString() == null ? "" : dr["embossing_name"].ToString();
                            template.CardToCardOrder.IsOurCard = Convert.ToBoolean(dr["is_our_card"].ToString());
                            template.CardToCardOrder.RegistrationDate = DateTime.Now;
                            template.CardToCardOrder.Currency = dr["currency"].ToString();

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
