using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
namespace ExternalBanking.DBManager
{
    class PeriodicTerminationOrderDB
    {
        internal static ActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SP_Terminate_Periodic_Transfer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Periodic_App_ID", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@filial", SqlDbType.Float).Value = order.FilialCode;
                    cmd.Parameters.Add("@Debet_Account", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@Credit_Account", SqlDbType.VarChar, 50).Value = order.CreditAccount;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }

        internal static bool IsSecondTermination(ulong periodicProductId)
        {
            bool secondTermination;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select HB.doc_id from Tbl_HB_documents HB
                        INNER JOIN Tbl_HB_Products_Identity HBPI
                        ON HB.doc_ID = HBPI.HB_Doc_ID
                        WHERE (HB.quality=2 or HB.quality = 3 or HB.quality = 5) and HB.document_type = 11 and HB.document_subtype = 1 and HBPI.App_ID =@periodicAppId", conn);
                cmd.Parameters.Add("@periodicAppId", SqlDbType.Float).Value = periodicProductId;
                if (cmd.ExecuteReader().Read())
                {
                    secondTermination = true;
                }
                else
                    secondTermination = false;
            }
            return secondTermination;
        }

        internal static PeriodicTerminationOrder GetPeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select  d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                          d.debet_account,d.credit_account,PI.App_ID,d.operation_date,d.confirmation_date
                                                  from Tbl_HB_documents as d left join Tbl_HB_Products_Identity as PI on  d.doc_ID=PI.HB_Doc_ID

                                                  where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.ProductId = dt.Rows[0]["App_ID"] != DBNull.Value ? ulong.Parse(dt.Rows[0]["App_ID"].ToString()) : default(ulong);
                order.DebitAccount = Account.GetAccount(ulong.Parse(dt.Rows[0]["debet_account"].ToString()));
                order.CreditAccount = dt.Rows[0]["credit_account"].ToString();
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
            }
            return order;
        }

        internal static ActionResult SavePeriodicTerminationOrderDetails(PeriodicTerminationOrder periodicTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_periodic_termination_order_details(order_id, debit_account, credit_account) VALUES(@orderId, @debitAccount, @creditAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@debitAccount", SqlDbType.BigInt).Value = periodicTerminationOrder.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.NVarChar, 50).Value = periodicTerminationOrder.CreditAccount;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

    }
}
