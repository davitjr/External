using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class InternationalOrderTemplateDB
    {
        /// <summary>
        /// Միջազգային փոխանցման հայտի ձևանմուշի պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static ActionResult SaveInternationalOrderTemplate(InternationalOrderTemplate template, Action action)
        {

            string descriptionForPayment = "";
            string receiverBankAddInf = "";
            if (template.InternationalPaymentOrder.Currency != "RUR")
            {
                receiverBankAddInf = template.InternationalPaymentOrder.ReceiverBankAddInf;
                descriptionForPayment = template.InternationalPaymentOrder.DescriptionForPayment;
            }
            else
            {
                receiverBankAddInf = "БИК-" + template.InternationalPaymentOrder.BIK + ", К/с-" + template.InternationalPaymentOrder.CorrAccount;
                if (template.InternationalPaymentOrder.KPP != "")
                {
                    receiverBankAddInf = receiverBankAddInf + ", КПП-" + template.InternationalPaymentOrder.KPP;
                }
                if (template.InternationalPaymentOrder.DescriptionForPaymentRUR1 != "Другое")
                {
                    descriptionForPayment = template.InternationalPaymentOrder.DescriptionForPaymentRUR1 + " ";
                }
                descriptionForPayment = descriptionForPayment + template.InternationalPaymentOrder.DescriptionForPaymentRUR2;
                if (template.InternationalPaymentOrder.DescriptionForPaymentRUR1 != "Материальная помощь" && template.InternationalPaymentOrder.DescriptionForPaymentRUR1 != "Другое")
                {
                    descriptionForPayment = descriptionForPayment + " " + template.InternationalPaymentOrder.DescriptionForPaymentRUR3 + " "
                        + template.InternationalPaymentOrder.DescriptionForPaymentRUR4 + " "
                        + template.InternationalPaymentOrder.DescriptionForPaymentRUR5;
                    if (template.InternationalPaymentOrder.DescriptionForPaymentRUR5 == "с НДС")
                    {
                        descriptionForPayment = descriptionForPayment + " " + template.InternationalPaymentOrder.DescriptionForPaymentRUR6 + " RUB";
                    }
                }

            }

            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_internationalTrasnfer_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;



                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = template.InternationalPaymentOrder.DebitAccount.AccountNumber;


                    if (template.TemplateType != TemplateType.CreatedAsGroupService)
                    {
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = template.InternationalPaymentOrder.Amount;
                    }
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;

                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.InternationalPaymentOrder.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.InternationalPaymentOrder.Type;


                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (template.InternationalPaymentOrder.Currency != "RUR")
                    {
                        if (template.InternationalPaymentOrder.IntermediaryBankSwift != "")
                        {
                            cmd.Parameters.Add("@intermediary_bank_swift", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.IntermediaryBankSwift;
                            cmd.Parameters.Add("@intermediary_bank", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.IntermediaryBank;

                        }
                        cmd.Parameters.Add("@Receiver_bank_swift", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.ReceiverBankSwift;
                        if (template.InternationalPaymentOrder.Currency == "USD" && template.InternationalPaymentOrder.Country == "840" && template.InternationalPaymentOrder.FedwireRoutingCode != "")
                        {
                            cmd.Parameters.Add("@routing_code", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.FedwireRoutingCode;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add("@BIK", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.BIK;
                        cmd.Parameters.Add("@Corr_account", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.CorrAccount;
                        cmd.Parameters.Add("@KPP", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.KPP;

                        if (template.InternationalPaymentOrder.INN != "")
                        {
                            cmd.Parameters.Add("@INN", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.INN;
                        }

                        cmd.Parameters.Add("@Receiver_type", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.ReceiverType;

                        cmd.Parameters.Add("@descr_for_payment_RUR_1", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR1;
                        cmd.Parameters.Add("@descr_for_payment_RUR_2", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR2;
                        cmd.Parameters.Add("@descr_for_payment_RUR_3", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR3;
                        cmd.Parameters.Add("@descr_for_payment_RUR_4", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR4;


                        if (template.InternationalPaymentOrder.DescriptionForPaymentRUR5 != "")
                        {
                            cmd.Parameters.Add("@descr_for_payment_RUR_5", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR5;
                        }
                        cmd.Parameters.Add("@descr_for_payment_RUR_6", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DescriptionForPaymentRUR6;
                    }

                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.Receiver;
                    cmd.Parameters.Add("@Receiver_bank", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.ReceiverBank;

                    if (receiverBankAddInf != "")
                    {
                        cmd.Parameters.Add("@Receiver_bank_add_inf", SqlDbType.NVarChar).Value = receiverBankAddInf;

                    }

                    cmd.Parameters.Add("@Country", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.Country;

                    cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.ReceiverAccount.AccountNumber;

                    if (template.InternationalPaymentOrder.ReceiverAddInf != "")
                    {
                        cmd.Parameters.Add("@receiver_add_inf", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.ReceiverAddInf;
                    }

                    cmd.Parameters.Add("@descr_for_payment", SqlDbType.NVarChar).Value = descriptionForPayment;

                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.Currency;


                    cmd.Parameters.Add("@details_of_charges", SqlDbType.NVarChar).Value = template.InternationalPaymentOrder.DetailsOfCharges;




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
        /// Միջազգային փոխանցման ձևանմուշի ստացում/Get
        /// </summary>
        /// <param name="internationalOrderTemplateId">Ձևանմուշի ունիկալ համար</param>
        /// <returns></returns>
        internal static InternationalOrderTemplate GetInternationalOrderTemplate(int internationalOrderTemplateId, ulong customerNumber)
        {
            InternationalOrderTemplate template = new InternationalOrderTemplate();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"Select T.id as templateId, T.description as TemplateDescription, T.customer_number, T.serial_number, T.registration_date, T.document_type, T.document_subtype, T.status, 
                                        T.type, T.order_group_id, T.source_type, T.amount, T.debet_account, T.user_id, IT.*  
                                        FROM Tbl_Templates T  inner join Tbl_International_Order_Template_Details IT on T.id = IT.template_id                                                                       
                                        WHERE T.id = @id and T.customer_number = @customer_number";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = internationalOrderTemplateId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        template = SetInternationalOrderTemplate(row);
                    }
                }
            }
            return template;
        }

        private static InternationalOrderTemplate SetInternationalOrderTemplate(DataRow dr)
        {
            InternationalOrderTemplate template = new InternationalOrderTemplate();
            template.InternationalPaymentOrder = new InternationalPaymentOrder();

            if (dr != null)
            {
                template.ID = Convert.ToInt32((dr["templateId"]));
                template.TemplateName = Utility.ConvertAnsiToUnicode(dr["TemplateDescription"].ToString());
                template.TemplateRegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                template.TemplateCustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());
                template.SerialNumber = Convert.ToInt32(dr["serial_number"].ToString());
                template.TemplateDocumentType = (OrderType)(dr["document_type"]);
                template.TemplateDocumentSubType = Convert.ToByte(dr["document_subtype"]);
                template.Status = (TemplateStatus)(Convert.ToInt32(dr["status"]));
                template.TemplateType = (TemplateType)(Convert.ToInt32(dr["type"]));
                template.ChangeUserId = Convert.ToInt32(dr["user_id"].ToString());

                if (dr["order_group_id"] != DBNull.Value)
                {
                    template.TemplateGroupId = Convert.ToInt32(dr["order_group_id"].ToString());
                }

                template.TemplateSourceType = (SourceType)(Convert.ToInt32(dr["source_type"]));

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
                template.InternationalOrderTemplateId = Convert.ToInt32(dr["id"].ToString());

                template.InternationalPaymentOrder.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                template.InternationalPaymentOrder.SubType = Convert.ToByte((dr["document_subtype"]));

                template.TemplateDocumentType = (OrderType)Convert.ToInt16((dr["document_type"]));




                if (dr["debet_account"] != DBNull.Value)
                {
                    string debitAccount = dr["debet_account"].ToString();

                    template.InternationalPaymentOrder.DebitAccount = Account.GetAccount(debitAccount);
                }


                template.InternationalPaymentOrder.ReceiverAccount = new Account();
                template.InternationalPaymentOrder.ReceiverAccount.AccountNumber = dr["credit_account"].ToString();

                if (dr["receiver_name"] != DBNull.Value)
                    template.InternationalPaymentOrder.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                if (dr["description"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPayment = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                if (dr["descr_for_payment_RUR_1"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR1 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_1"].ToString());

                if (dr["descr_for_payment_RUR_2"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR2 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_2"].ToString());

                if (dr["descr_for_payment_RUR_3"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR3 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_3"].ToString());

                if (dr["descr_for_payment_RUR_4"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR4 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_4"].ToString());

                if (dr["descr_for_payment_RUR_5"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR5 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_5"].ToString());

                if (dr["descr_for_payment_RUR_6"] != DBNull.Value)
                    template.InternationalPaymentOrder.DescriptionForPaymentRUR6 = Utility.ConvertAnsiToUnicode(dr["descr_for_payment_RUR_6"].ToString());


                if (dr["amount"] != DBNull.Value)
                    template.InternationalPaymentOrder.Amount = Convert.ToDouble(dr["amount"]);

                if (dr["currency"] != DBNull.Value)
                    template.InternationalPaymentOrder.Currency = dr["currency"].ToString();

                if (dr["Intermediary_bank"] != DBNull.Value)
                    template.InternationalPaymentOrder.IntermediaryBank = dr["Intermediary_bank"].ToString();

                if (dr["Intermediary_bank_swift"] != DBNull.Value)
                    template.InternationalPaymentOrder.IntermediaryBankSwift = dr["Intermediary_bank_swift"].ToString();

                if (dr["receiver_bank"] != DBNull.Value)
                    template.InternationalPaymentOrder.ReceiverBank = dr["receiver_bank"].ToString();

                if (dr["receiver_bank_swift"] != DBNull.Value)
                    template.InternationalPaymentOrder.ReceiverBankSwift = dr["receiver_bank_swift"].ToString();

                if (dr["receiver_bank_add_inf"] != DBNull.Value)
                    template.InternationalPaymentOrder.ReceiverBankAddInf = dr["receiver_bank_add_inf"].ToString();

                if (dr["routing_code"] != DBNull.Value)
                    template.InternationalPaymentOrder.FedwireRoutingCode = dr["routing_code"].ToString();

                if (dr["receiver_add_inf"] != DBNull.Value)
                    template.InternationalPaymentOrder.ReceiverAddInf = dr["receiver_add_inf"].ToString();

                if (dr["details_of_charges"] != DBNull.Value)
                    template.InternationalPaymentOrder.DetailsOfCharges = dr["details_of_charges"].ToString();

                if (dr["country"] != DBNull.Value)
                    template.InternationalPaymentOrder.Country = dr["country"].ToString();

                if (dr["Inn"] != DBNull.Value)
                    template.InternationalPaymentOrder.INN = dr["inn"].ToString();

                if (dr["bik"] != DBNull.Value)
                    template.InternationalPaymentOrder.BIK = dr["bik"].ToString();

                if (dr["kpp"] != DBNull.Value)
                    template.InternationalPaymentOrder.KPP = dr["kpp"].ToString();

                if (dr["corr_account"] != DBNull.Value)
                    template.InternationalPaymentOrder.CorrAccount = dr["corr_account"].ToString();


                template.InternationalPaymentOrder.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                if (dr["Receiver_type"] != DBNull.Value)
                    template.InternationalPaymentOrder.ReceiverType = Convert.ToByte((dr["Receiver_type"]));

            }
            else
            {
                template = null;
            }


            return template;
        }

    }
}
