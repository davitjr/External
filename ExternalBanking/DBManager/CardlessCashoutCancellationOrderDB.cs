using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class CardlessCashoutCancellationOrderDB
    {
        internal static ActionResult Save(CardlessCashoutCancellationOrder order)
        {
            ActionResult result = new ActionResult();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "pr_save_cardless_cashout_cancellation_order";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Doc_ID", SqlDbType.BigInt).Value = order.CancellationDocId;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = order.user.userName;
            cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 10).Value = order.OrderNumber;
            cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar, 20).Value = order.CreditAccount.AccountNumber;
            cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = order.Description;
            cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@ducument_sub_type", SqlDbType.TinyInt).Value = order.SubType;
            cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (int)order.Source;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
            cmd.Parameters.Add("@transfer_fee", SqlDbType.Float).Value = order.TransferFee;
            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = order.Currency;


            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


            cmd.ExecuteNonQuery();

            byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
            int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
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

        internal static void GetCardlessCashOutOrderFromCodeOrDocId(CardlessCashoutCancellationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (order.CancellationDocId != 0)
                    {
                        cmd.CommandText = @"SELECT amount, 
                                               debet_account, 
                                               currency,  
                                               amount_for_payment
                                      FROM tbl_hb_documents
                                        WHERE doc_id = @id";
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.CancellationDocId;

                    }
                    else
                    {
                        cmd.CommandText = @"SELECT hb.doc_id, customer_number,
                                               amount, 
                                               debet_account, 
                                               currency,  
                                               amount_for_payment 
                                            FROM tbL_hb_documents HB 
										    INNER JOIN 
										    TBl_cardless_cashout_order_details COD 
										    ON hb.doc_id = cod.doc_id
									        WHERE  cod.order_OTP = @cardlessCashOutCode";
                        cmd.Parameters.Add("@cardlessCashOutCode", SqlDbType.NVarChar, 10).Value = order.CardlessCashoutCode;
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();

                                order.CreditAccount = Account.GetAccount(debitAccount);
                            }

                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["amount_for_payment"] != DBNull.Value)
                                order.TransferFee = Convert.ToDouble(dr["amount_for_payment"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();

                            if (!string.IsNullOrEmpty(order.CardlessCashoutCode))
                                order.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);

                            if (!string.IsNullOrEmpty(order.CardlessCashoutCode))
                                order.CancellationDocId = Convert.ToInt64(dr["doc_id"]);
                        }
                    }
                }
            }
        }


    }
}
