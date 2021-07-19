using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    internal class LoanMatureOrderTemplateDB
    {
        internal static ActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplate template, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_loan_mature_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;

                    
                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = template.MatureOrder.Description;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar).Value = template.MatureOrder.Currency;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.MatureOrder.MatureType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.TemplateDocumentType;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = template.MatureOrder.ProductId;


                    if (template.MatureOrder.PercentAccount != null)
                    {
                        cmd.Parameters.Add("@AccountAMD", SqlDbType.VarChar, 50).Value = template.MatureOrder.PercentAccount.AccountNumber;
                    }

                    cmd.Parameters.Add("@AmountAMD", SqlDbType.Float).Value = template.MatureOrder.PercentAmount;

                    if (template.MatureOrder.Account != null)
                    {
                        cmd.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = template.MatureOrder.Account.AccountNumber;
                    }

                    cmd.Parameters.Add("@isProblematic", SqlDbType.Bit).Value = template.MatureOrder.IsProblematic;


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
        /// Վարկի մարման ձևանմուշի/խմբային ծառայության ստացում
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static LoanMatureOrderTemplate GetLoanMatureOrderTemplate(int templateId, ulong customerNumber)
        {
            LoanMatureOrderTemplate template = new LoanMatureOrderTemplate();
            template.MatureOrder = new MatureOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select T.id as templateId, T.description as TemplateDescription, T.customer_number, T.serial_number, T.registration_date, T.document_type, T.document_subtype, T.status, 
                                        T.type, T.order_group_id, T.source_type, T.amount, T.debet_account, T.user_id, P.*  
                                        FROM Tbl_Templates T  inner join Tbl_Loan_Repayment_Order_Template_Details P on T.id = P.template_id                                                                       
                                        WHERE T.id = @id and T.customer_number = @customer_number";

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = templateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //Ձևանմուշ
                            template.ID = Convert.ToInt32((dr["templateId"]));
                            template.TemplateName = dr.FieldOrDefault<string>("TemplateDescription");
                            template.TemplateRegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            template.TemplateCustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                            template.SerialNumber = Convert.ToInt32(dr["serial_number"].ToString());
                            template.TemplateDocumentType = (OrderType)Convert.ToInt32(dr["document_type"]);
                            template.TemplateDocumentSubType = Convert.ToByte(dr["document_subtype"]);
                            template.Status = (TemplateStatus)Convert.ToByte(dr["status"]);
                            template.TemplateType = (TemplateType)Convert.ToByte(dr["type"]);
                            template.ChangeUserId = Convert.ToInt32(dr["user_id"].ToString());

                            if (dr["order_group_id"] != DBNull.Value)
                            {
                                template.TemplateGroupId = Convert.ToInt32(dr["order_group_id"].ToString());
                                template.MatureOrder.GroupId = template.TemplateGroupId;
                            }

                            template.TemplateSourceType = (SourceType)Convert.ToInt32(dr["source_type"]);


                            if (dr["debet_account"] != DBNull.Value)
                            {
                                template.TemplateDebetAccount = new Account();
                                template.TemplateDebetAccount.AccountNumber = dr["debet_account"].ToString();
                            }

                            //Ձևանմուշի մանրամասներ
                            template.MatureOrder.Id = long.Parse(dr["id"].ToString());
                            template.MatureOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));
                            template.MatureOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();

                                template.MatureOrder.DebitAccount = Account.GetAccount(debitAccount);

                                template.MatureOrder.Account = Account.GetAccount(Convert.ToUInt64(dr["Account"].ToString()));
                                if (template.MatureOrder.Account != null)
                                {
                                    template.MatureOrder.Account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(template.MatureOrder.Account.AccountTypeDescription);
                                }
                            }
                            template.MatureOrder.DayOfProductRateCalculation = Convert.ToDateTime(dr["last_day_of rate_calculation"].ToString());

                            template.MatureOrder.ProductId = Convert.ToUInt64(dr["App_ID"]);

                            if (dr["repayment_source"] != DBNull.Value)
                                template.MatureOrder.RepaymentSourceType = Convert.ToUInt16(dr["repayment_source"].ToString());

                            template.MatureOrder.MatureType = (MatureType)Convert.ToByte(dr["document_subtype"].ToString());

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
