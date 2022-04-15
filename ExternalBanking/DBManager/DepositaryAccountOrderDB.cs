using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class DepositaryAccountOrderDB
    {
        /// <summary>
        /// Պարտատոմսի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult SaveDepositaryAccountOrder(DepositaryAccountOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_depositary_account_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = 1;

                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@depositary_account", SqlDbType.Float).Value = order.AccountNumber.AccountNumber;
                    cmd.Parameters.Add("@depositary_account_description", SqlDbType.NVarChar, 250).Value = order.AccountNumber.Description;
                    cmd.Parameters.Add("@depositary_account_bank_code", SqlDbType.Int).Value = order.AccountNumber.BankCode;
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = order.AccountNumber.Status;
                    cmd.Parameters.Add("@stock_income_account", SqlDbType.NVarChar, 25).Value = order.AccountNumber.StockIncomeAccountNumber;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
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

        internal static DepositaryAccountOrder GetDepositaryAccountOrder(DepositaryAccountOrder order)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT   hb.customer_number,
                                    hb.registration_date, 
                                    hb.document_number,
                                    hb.registration_date,
                                    hb.document_subtype,
                                    hb.quality,
                                    hb.currency, 
                                    hb.operation_date,
                                    hb.source_type,
                                    hb.document_type,
                                    hb.confirmation_date,
                                    D.depositary_account,
                                    D.depositary_account_bank_code,
                                    D.depositary_account_description,
                                    D.stock_income_account
                                        FROM Tbl_HB_documents hb
                                        INNER JOIN Tbl_depositary_account_order_details D
                                        ON hb.doc_ID = d.doc_id
                                        WHERE hb.Doc_ID = @DocID
                                        AND hb.customer_number  = CASE WHEN @customer_number = 0 THEN hb.customer_number   ELSE @customer_number END";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                    order.AccountNumber = new DepositaryAccount
                    {
                        AccountNumber = dt.Rows[0]["depositary_account"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["depositary_account"].ToString()) : 0,
                        BankCode = dt.Rows[0]["depositary_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["depositary_account_bank_code"]) : 0,
                        Description = dt.Rows[0]["depositary_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt.Rows[0]["depositary_account_description"].ToString()) : "",
                        StockIncomeAccountNumber = dt.Rows[0]["stock_income_account"].ToString()
                    };
                }
            }

            return order;
        }

        internal static ActionResult UpdateDepositoryAccountOrder(DepositaryAccountOrder order)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"UPDATE  [Tbl_depositary_account_order_details] set depositary_account = @depository_account, [status] = @status WHERE Doc_ID = @doc_id";

                    cmd.Parameters.Add("@doc_id", SqlDbType.BigInt).Value = order.Id;
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = order.AccountNumber.Status;
                    cmd.Parameters.Add("@depository_account", SqlDbType.Float).Value = order.AccountNumber.AccountNumber;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        internal static bool ExistsNotConfirmedDepositaryAccountOrder(ulong customerNumber, byte subType)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM tbl_hb_documents D inner join Tbl_depositary_account_order_details DA on D.doc_id = DA.doc_id
                                        WHERE document_type = @document_type AND quality = 3 AND D.customer_number = @customer_number 
                                        and document_type = @document_type and document_subtype = @document_sub_type ) 
                                        SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@document_type", SqlDbType.SmallInt).Value = OrderType.DepositaryAccountOrder;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = subType;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
    }
}
