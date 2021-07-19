using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    /// <summary>
    /// Ձևանմուշներ
    /// </summary>
    internal class TemplateDB
    {
        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հաճախորդն ունի տվյալ անունով ձևանմուշ, թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        internal static bool ExistsTemplateByName(ulong customerNumber, string templateName,int id)
        {
            bool exists = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"Select id from Tbl_Templates where [description]=@templateName and customer_number=@customerNumber and id<>@id", conn))
                {
                    cmd.Parameters.Add("@templateName", SqlDbType.NVarChar).Value = templateName;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    if (cmd.ExecuteReader().Read())
                    {
                        exists = true;
                    }
                    else
                        exists = false;
                }

            }
            return exists;
        }

        /// <summary>
        /// Ձևանմուշի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        internal static ActionResult ChangeTemplateStatus(int id, TemplateStatus status)
        {
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Failed;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string txt = @"UPDATE Tbl_Templates SET status = @status WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = status;

                    conn.Open();

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ խմբի ծառայությունների ցանկը
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<Template> GetGroupTemplates(int groupId)
        {
            List<Template> templates = new List<Template>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT t.*, s.[description] as sub_type_description ,i.* 
                                FROM Tbl_Templates t
                                INNER JOIN Tbl_sub_types_of_HB_products s 
                                ON t.document_type = s.document_type and t.document_subtype = s.document_sub_type  
                                INNER JOIN [dbo].[tbl_group_template_short_info] I
                                ON t.id = i.template_id
                                WHERE [type] = 2 and t.order_group_id = @groupId";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@groupId", SqlDbType.Int).Value = groupId;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        templates = new List<Template>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Template template = SetTemplate(row);

                        templates.Add(template);
                    }
                }

            }
            return templates;
        }

        /// <summary>
        /// Ինիցիալիզացնում է ձևանմուշը
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        internal static Template SetTemplate(DataRow row)
        {
            Template template = new Template();
            template.ID = Convert.ToInt32(row["id"].ToString());

            template.TemplateName = Utility.ConvertAnsiToUnicode(row["description"].ToString());
            template.TemplateRegistrationDate = Convert.ToDateTime(row["registration_date"]);
            template.TemplateCustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
            template.SerialNumber = Convert.ToInt32(row["serial_number"].ToString());
            template.TemplateDocumentType = (OrderType)(row["document_type"]);
            template.TemplateDocumentSubType = Convert.ToByte(row["document_subtype"].ToString());
            template.Status = (TemplateStatus)Convert.ToInt16(row["status"].ToString());
            template.TemplateType = (TemplateType)Convert.ToInt16(row["type"].ToString());
            template.ChangeUserId = Convert.ToInt32(row["user_id"].ToString());
            


            template.TemplateSourceType = (SourceType)Convert.ToInt16(row["source_type"].ToString());

            if (row["amount"] != DBNull.Value)
            {
                template.TemplateAmount = Convert.ToDouble(row["amount"].ToString());
            }

            if (row["debet_account"] != DBNull.Value)
            {
                template.TemplateDebetAccount = new Account();
                template.TemplateDebetAccount.AccountNumber = row["debet_account"].ToString();
            }

            if (row.FieldOrDefault<int>("order_group_id") > 0)
            {
                template.TemplateGroupId = Convert.ToInt32(row["order_group_id"].ToString());
                template.GroupTemplateShrotInfo = new GroupTemplateShrotInfo();
                template.GroupTemplateShrotInfo.ReceiverAccount = row.FieldOrDefault<string>("receiver_account");
                template.GroupTemplateShrotInfo.ReceiverName = row.FieldOrDefault<string>("receiver_name");
                template.GroupTemplateShrotInfo.DebitAccount = row.FieldOrDefault<string>("debit_account");
                template.GroupTemplateShrotInfo.FeeAccount = row.FieldOrDefault<string>("fee_account");
                template.GroupTemplateShrotInfo.FeeAccount = row.FieldOrDefault<string>("percent_account");
                template.GroupTemplateShrotInfo.Amount = row.FieldOrDefault<double>("amount");
                template.GroupTemplateShrotInfo.CustomerId = row.FieldOrDefault<string>("customer_id");
                template.GroupTemplateShrotInfo.LoanAppId = Convert.ToUInt64(row["loan_app_id"].ToString());
                if (row["debit_account"] != DBNull.Value)
                {
                    template.TemplateDebetAccount = new Account();
                    template.TemplateDebetAccount.AccountNumber = row["debit_account"].ToString();
                }
            }


            return template;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ձևանմուշների ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<Template> GetCustomerTemplates(ulong customerNumber, TemplateStatus status)
        {
            List<Template> templates = new List<Template>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                string sql = "";

                sql = @" SELECT t.*
                               FROM Tbl_Templates t 
                               WHERE [type] = 1 AND customer_number = @customerNumber AND t.status = @status  order by id desc ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = (short)status;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);

                    }

                    if (dt.Rows.Count > 0)
                        templates = new List<Template>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        Template template = SetTemplate(row);

                        templates.Add(template);
                    }
                }

            }
            return templates;
        }

        /// <summary>
        /// Պահպանում է խմբային ծառայության միջնորդավճարը
        /// </summary>
        /// <param name="order"></param>
        public static void SaveTemplateFee(Template template, Order order)
        {
            order.Fees.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.Tbl_Template_Fees(template_id, FeeAmount, Currency, FeeType,debet_account,credit_account, Description)
                                                    VALUES (@template_id,@fee_amount, @fee_currency,  @fee_type,@debit_acc,@credit_acc, @Description)", conn))
                    {
                        if (m.Account != null)
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = m.Account.AccountNumber;
                        else
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
                        if (m.CreditAccount != null)
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = m.CreditAccount.AccountNumber;
                        else
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;

                        if (m.Description != null)
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = m.Description;
                        else
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = DBNull.Value;

                        cmd.Parameters.Add("@template_id", SqlDbType.Int).Value = template.ID;
                        cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = m.Amount;
                        if (!string.IsNullOrEmpty(m.Currency))
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = m.Currency;
                        else
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                        cmd.Parameters.Add("@fee_type", SqlDbType.NVarChar, 20).Value = m.Type;                    

                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }


        /// <summary>
        /// Վերադարձնում է խմբային ծառայության միջնորդավճարի տվյալները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderFee> GetTemplateFees(int templateId)
        {
            List<OrderFee> fees = new List<OrderFee>();


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select * from Tbl_Template_Fees  where template_id=@template_id", conn))
                {
                    cmd.Parameters.Add("@template_id", SqlDbType.Int).Value = templateId; 
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        OrderFee fee = new OrderFee();
                        fee.Amount = Convert.ToDouble(row["FeeAmount"].ToString());
                        fee.Currency = row["Currency"].ToString();
                        fee.Type = Convert.ToInt16(row["FeeType"].ToString());
                        fee.TypeDescription = Info.BankOperationFeeTypeDescription(fee.Type);
                        if (row["debet_account"] != DBNull.Value)
                        {
                            fee.Account = Account.GetAccount(row["debet_account"].ToString());
                        }

                        if (row["credit_account"] != DBNull.Value)
                        {
                            fee.CreditAccount = Account.GetSystemAccount(row["credit_account"].ToString());
                        }

                        if (row["Description"] != DBNull.Value)
                        {
                            fee.Description = row["Description"].ToString();
                        }

                        fees.Add(fee);
                    }
                }


            }
            return fees;
        }

        public static int GetCustomerTemplatesCounts(ulong customerNumber)
        {
            int count = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                string sql = "";

                sql = @" SELECT count(t.id) as count
                               FROM Tbl_Templates t 
                               WHERE [type] = 1 AND customer_number = @customerNumber AND t.status = @status";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = (short)TemplateStatus.Active;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        count = Convert.ToInt32(dt.Rows[0]["count"].ToString());
                }
            }
            return count;
        }


    }
}
