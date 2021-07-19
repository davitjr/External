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
    class PensionPaymentOrderDB
    {
        internal static List<PensionPaymentOrder> GetAllPensionPayment(string socialCardNumber)
        {
            List<PensionPaymentOrder> pensionPayments = new List<PensionPaymentOrder>();
            DataTable dt = new DataTable();
            string query = @"SELECT P.Id, dateget, year, month, amount social_card_number, amount, description_ACBA, firstname, LastName, fatersname, account_number, ISNULL(file_type, 0) as file_type
                                FROM [Tbl_pension_payments] P
                            INNER JOIN Tbl_type_of_pension_payments T ON P.payment_type = T.id
                            WHERE social_card_number = @socCardNumber and [is_verified] = 0 and [is_complete] = 0 AND TransOK = 1 
                            ORDER BY P.Id DESC";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@socCardNumber", SqlDbType.NVarChar, 30).Value = socialCardNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        pensionPayments.Add(SetPensionPayment(row));
                    }
                }
            }
            return pensionPayments;
        }
        internal static PensionPaymentOrder GetPensionPaymentOrderDetails(uint id)
        {
            PensionPaymentOrder pensionPayments = new PensionPaymentOrder();
            DataTable dt = new DataTable();
            string query = @"SELECT hb.doc_id, registration_date, operation_date, currency, pp.id, dateget, year, month, social_card_number, hb.amount, description_ACBA, 
							firstname, lastname, fatersname, pp.account_number, ISNULL(file_type, 0)  as file_type FROM Tbl_HB_documents hb
							JOIN [Tbl_HB_Products_Identity] p ON hb.doc_id = p.hb_doc_id 
							JOIN (SELECT id, dateget, year, month, social_card_number, firstname, lastname, fatersname, account_number, file_type, payment_type FROM Tbl_pension_payments) pp ON p.app_id = pp.id
							JOIN Tbl_type_of_pension_payments T ON pp.payment_type = T.id 
							WHERE hb.doc_id = @id ";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        pensionPayments = SetPensionPayment(dt.Rows[0], true);
                    }
                }
            }
            return pensionPayments;
        }
        private static PensionPaymentOrder SetPensionPayment(DataRow row, bool forDetails = false)
        {
            PensionPaymentOrder order = new PensionPaymentOrder();
            order.PensionPaymentId = Convert.ToInt32(row["Id"]);
            order.DateGet = Convert.ToDateTime(row["dateget"]);
            order.Year = Convert.ToInt16(row["year"]);
            order.Month = Convert.ToInt16(row["month"]);
            order.SocialCardNumber = row["social_card_number"].ToString();
            order.Amount = Convert.ToDouble(row["amount"]);
            order.Description = Utility.ConvertAnsiToUnicode(row["description_ACBA"].ToString());
            order.FirstName = row["firstname"].ToString().Replace("ԵՒ", "և").Replace("եւ", "և");
            order.LastName = row["lastname"].ToString().Replace("ԵՒ", "և").Replace("եւ", "և");
            order.FatherName = row["fatersname"] != DBNull.Value ? row["fatersname"].ToString().Replace("ԵՒ", "և").Replace("եւ", "և") : string.Empty;
            order.DebitAccount.AccountNumber = row["account_number"].ToString();
            order.FileTypeDescription = Convert.ToInt32(row["file_type"]) == 1 ? "կենսաթոշակ" : Convert.ToInt32(row["file_type"]) == 2 ? "Նպաստ" : string.Empty;

            if (forDetails)
            {
                order.Id = Convert.ToInt64(row["doc_id"]);
                order.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                order.OperationDate = Convert.ToDateTime(row["operation_date"]);
                order.Currency = Convert.ToString(row["currency"]);
            }
            return order;
        }

        public static ActionResult Save(PensionPaymentOrder order, string UserNname)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "pr_save_pension_payment_order";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = order.Currency;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = UserNname;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@debet_account", SqlDbType.NVarChar, 50).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar, 50).Value = order.CreditAccount;
                cmd.Parameters.Add("@pension_id", SqlDbType.BigInt).Value = order.PensionPaymentId;
                cmd.Parameters.Add("@document_type", SqlDbType.TinyInt).Value = order.Type;
                cmd.Parameters.Add("@ducument_sub_type", SqlDbType.TinyInt).Value = order.SubType;
                cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = order.Description;
                cmd.Parameters.Add("@document_number", SqlDbType.Int).Value = order.OrderNumber;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = order.Source;

                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


                cmd.ExecuteNonQuery();

                byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                result.Id = order.Id;
                order.Quality = OrderQuality.Draft;

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
                return result;
            }
        }


    }
}
