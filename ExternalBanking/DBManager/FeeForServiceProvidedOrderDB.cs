using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class FeeForServiceProvidedOrderDB
    {


        internal static ActionResult Save(FeeForServiceProvidedOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_FeeForServiceProvidedOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 200).Value = order.Description;
                    cmd.Parameters.Add("@service_type", SqlDbType.Int).Value = order.ServiceType;
                    cmd.Parameters.Add("@customer_residence", SqlDbType.Int).Value = order.CustomerResidence;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@TaxAccountProvision", SqlDbType.Bit).Value = order.TaxAccountProvision;
                    cmd.Parameters.Add("@CardNumber", SqlDbType.NVarChar, 30).Value = order.CardNumber;




                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static FeeForServiceProvidedOrder Get(FeeForServiceProvidedOrder order)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT d.registration_date,
                                                         d.document_type,
                                                         d.document_number,
                                                         d.amount,
                                                         d.debet_account,
                                                         d.credit_account,
                                                         d.quality,
                                                         d.document_subtype,   
                                                         d.description,
                                                         d.filial,
                                                         d.currency,
                                                         r.service_type,
                                                         isnull(r.customer_residence,0) as customer_residence,
                                                         d.operation_date,
                                                         r.TaxAccountProvision,
                                                         r.CardNumber
                                                         FROM Tbl_HB_documents AS d 
                                                         INNER JOIN Tbl_fee_for_service_provided_order_details AS r
                                                         ON d.doc_ID=r.Doc_ID
                                                         WHERE d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                if (dt.Rows.Count > 0)
                {
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"].ToString());
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.DebitAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                    order.Description = dt.Rows[0]["description"].ToString();
                    order.ServiceType = Convert.ToUInt16(dt.Rows[0]["service_type"]);
                    order.CustomerResidence = short.Parse(dt.Rows[0]["customer_residence"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.TaxAccountProvision = Convert.ToBoolean(dt.Rows[0]["TaxAccountProvision"]);
                    order.CardNumber = dt.Rows[0]["CardNumber"].ToString();

                }
            }
            return order;
        }

        internal static ActionResult SaveFeeForServiceOrderDetails(FeeForServiceProvidedOrder feeForServiceProvidedOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_fee_for_service_provided_details(order_id, service_type, customer_residence,TaxAccountProvision) 
                                        VALUES(@orderId, @serviceType, @customerResidence,@TaxAccountProvision)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@serviceType", SqlDbType.Int).Value = feeForServiceProvidedOrder.ServiceType;
                    cmd.Parameters.Add("@customerResidence", SqlDbType.Int).Value = feeForServiceProvidedOrder.CustomerResidence;
                    cmd.Parameters.Add("@TaxAccountProvision", SqlDbType.Bit).Value = feeForServiceProvidedOrder.TaxAccountProvision;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static Account GetCreditAccount(FeeForServiceProvidedOrder order)
        {
            Account creditAccount = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_GetAccountsForValueAddedTaxOperation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@HBdocumentType", SqlDbType.SmallInt).Value = order.Type;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@customerResidenceFromOrder", SqlDbType.TinyInt).Value = order.CustomerResidence;
                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@rowNumberInPrices", SqlDbType.Int).Value = order.ServiceType;
                    cmd.Parameters.Add("@CardNumber", SqlDbType.NVarChar, 30).Value = order.CardNumber;


                    SqlParameter param = new SqlParameter("@account1", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@account2", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@account3", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@account4", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@account5", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@account6", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    string account1 = cmd.Parameters["@account1"].Value.ToString();
                    string account2 = cmd.Parameters["@account2"].Value.ToString();

                    creditAccount = AccountDB.GetSystemAccount(account1);
                }
            }

            return creditAccount;
        }
    }
}
