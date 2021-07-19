using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class TransferToShopOrderDB
    {

        internal static ActionResult SaveTransferToShopOrder(TransferToShopOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"	declare @filial as int
                                                    select @filial=filialcode from Tbl_customers where customer_number=@customer_number
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    [description],quality,
                                                    source_type,operationFilialCode,operation_date)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,@descr,1,@source_type,@operationFilialCode,@oper_day)
                                                    Select Scope_identity() as ID
                                            ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = order.ReceiverAccount.AccountNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description == null ? "" : order.Description;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                order.Id = Convert.ToInt64(cmd.ExecuteScalar());

                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }



        /// <summary>
        /// Վերադարձնում է Փոխանցում խանութի հաշվին հայտի տվյալները:
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static TransferToShopOrder GetTransferToShopOrder(TransferToShopOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT
                                                        filial,
                                                        customer_number,
                                                        registration_date,
                                                        document_type,
                                                        document_number,
                                                        document_subtype,
                                                        amount,
                                                        currency,
                                                        debet_account,
                                                        credit_account,
                                                        description,
                                                        quality,
                                                        source_type,
                                                        operationFilialCode,
                                                        operation_date,
                                                        id.*
													FROM Tbl_HB_documents hb INNER JOIN Tbl_HB_Products_Identity id
													ON hb.doc_ID=id.HB_Doc_ID
													WHERE Doc_id=@Docid AND hb.customer_number=CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)Convert.ToInt16(dt.Rows[0]["document_type"]);
                    order.Description = dt.Rows[0]["description"].ToString();
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.DebitAccount = Account.GetSystemAccount(dt.Rows[0]["debet_account"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.DebitAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                    order.ReceiverAccount = Account.GetAccount(dt.Rows[0]["credit_account"].ToString());
                    order.ProductId = Convert.ToUInt64(dt.Rows[0]["App_ID"]);
                    order.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());

                }

            }
            return order;
        }



        internal static bool CheckTransferToShopPayment(ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select product_app_ID FROM Tbl_prepayments_contracts WHERE status=1 and [type] = 200 and product_app_ID =@app_Id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }        
        }


        internal static Account GetShopAccount(ulong productId)
        {
            Account shopAccount = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT debet_account_number FROM [Tbl_prepayments_contracts] WHERE product_app_id = @app_Id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;
                if (cmd.ExecuteScalar() != null)
                {
                    shopAccount = Account.GetAccount(cmd.ExecuteScalar().ToString());
                }
            }
            return shopAccount;
        }


        internal static Account GetShopDebitAccount(ulong productId)
        {
            Account shopDebitAccount = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT arm_number FROM [Tbl_prepayments_contracts] WHERE product_app_id = @app_Id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;
                if (cmd.ExecuteScalar() != null)
                {
                    shopDebitAccount = Account.GetAccount(cmd.ExecuteScalar().ToString());
                }
            }
            return shopDebitAccount;
        }

        internal static double GetShopTransferAmount(ulong productId)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT amount FROM [Tbl_prepayments_contracts] WHERE product_app_id = @app_Id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;
                if (cmd.ExecuteScalar() != null)
                {
                    amount = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            return amount;
        }



    }
}
