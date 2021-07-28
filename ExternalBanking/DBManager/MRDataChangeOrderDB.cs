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
    internal static class MRDataChangeOrderDB
    {
        /// <summary>
        /// Երրորդ անձի իրավունքի փոխանցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(MRDataChangeOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_MR_data_change_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@filial", SqlDbType.Float).Value = filialCode;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar, 50).Value = order.DataChangeAccount.AccountNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.DataChangeAccount.Currency;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@card_number", SqlDbType.VarChar, 150).Value = order.DataChangeCard.CardNumber;
                    cmd.Parameters.Add("@mr_Id", SqlDbType.Int).Value = order.MRId;
                    cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.ServiceFee;

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    SqlParameter param = new SqlParameter("@Doc_ID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@Doc_ID"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;

                    return result;
                }
            }

        }

        internal static MRStatus GetCardMRStatus(int mrId)
        {
            MRStatus status = MRStatus.NotFound;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT TS.id AS statusId FROM Tbl_MR_Applications A
                                INNER JOIN Tbl_type_of_MR_status TS ON A.status = TS.id
                                WHERE A.mr_id = @mr_Id";
                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mr_Id", SqlDbType.Int).Value = mrId;
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            status = (MRStatus)Convert.ToInt32(reader["statusId"]);
                        }
                    }
                }
            }

            return status;
        }
    }
}
