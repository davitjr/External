using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.XBManagement;

namespace ExternalBanking.DBManager
{
    class PhoneBankingContractOrderDB
    {
        internal static ActionResult Save(PhoneBankingContractOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_phone_banking_contract_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)order.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;

                    cmd.Parameters.Add("@contract_number", SqlDbType.NVarChar).Value = order.ContractNumber;
                    cmd.Parameters.Add("@limit_of_day_other_account", SqlDbType.Float).Value = order.DayLimitToAnothersAccount;
                    cmd.Parameters.Add("@limit_of_day_own_account", SqlDbType.Float).Value = order.DayLimitToOwnAccount;
                    cmd.Parameters.Add("@limit_of_one_transaction_other_account", SqlDbType.Float).Value = order.OneTransactionLimitToAnothersAccount;
                    cmd.Parameters.Add("@limit_of_one_transaction_own_aacount", SqlDbType.Float).Value = order.OneTransactionLimitToOwnAccount;
                    cmd.Parameters.Add("@customer_email_id", SqlDbType.Int).Value = order.EmailId;
                    cmd.Parameters.Add("@customer_phone_id", SqlDbType.Int).Value = order.PhoneId;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    QuestionAnswerCollection questionAnswerCollection = new QuestionAnswerCollection();

                    foreach (PhoneBankingContractQuestionAnswer qa in order.QuestionAnswers)
                    {
                        questionAnswerCollection.Add(qa);
                    }

                    SqlParameter paramAnswers = new SqlParameter("@question_answers_list", SqlDbType.Structured);
                    paramAnswers.Value = questionAnswerCollection;
                    paramAnswers.TypeName = "dbo.QuestionAnswer";
                    cmd.Parameters.Add(paramAnswers);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);

                    result.Id = order.Id;

                }
                return result;
            }

        }

        internal static PhoneBankingContractOrder Get(PhoneBankingContractOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                         doc.document_number,
                                                         doc.document_type,
                                                         doc.debet_account,
                                                         doc.deb_for_transfer_payment,
                                                         doc.source_type,
                                                         doc.quality,
                                                         doc.document_subtype ,
                                                         doc.registration_date,
                                                         doc.operation_date,

                                                         det.contract_number   
                                                                                                   
                                                         FROM Tbl_HB_documents as doc

                                                         INNER JOIN TBl_PhoneBanking_Contract_Details det
                                                         on doc.doc_ID=det.doc_ID
                                                         WHERE doc.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;

                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {                  
                    order.ContractNumber = dt.Rows[0]["contract_number"].ToString();

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }

            }
            return order;


        }
    }
}
