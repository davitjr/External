using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class C2CTransferOrderDB
    {
        internal static void SaveC2CTransferOrder(C2CTransferOrder order, C2CTransferResult result)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("pr_save_card_to_card_transfer_order", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@debetCardNumber", SqlDbType.NVarChar, 16).Value = order.SourceCard.CardNumber;
                cmd.Parameters.Add("@creditCardNumber", SqlDbType.NVarChar, 16).Value = order.DestinationCardNumber;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = order.Status;
                cmd.Parameters.Add("@orderID", SqlDbType.BigInt).Value = order.OrderID;
                cmd.Parameters.Add("@sourceID", SqlDbType.Int).Value = order.SourceID;
                cmd.Parameters.Add("@receiver", SqlDbType.NVarChar).Value = order.Receiver;
                if (result.ResponseCode != null)
                    cmd.Parameters.Add("@responseCode", SqlDbType.NVarChar).Value = result.ResponseCode;
                if (result.ResponseCodeDescription != null)
                    cmd.Parameters.Add("@responseCodeDescription", SqlDbType.NVarChar, 1000).Value = result.ResponseCodeDescription;
                cmd.Parameters.Add("@resultCode", SqlDbType.NVarChar).Value = result.ResultCode;
                if (result.ResultCodeDescription != null)
                    cmd.Parameters.Add("@resultCodeDescription", SqlDbType.NVarChar, 1000).Value = result.ResultCodeDescription;
                cmd.Parameters.Add("@processingCode", SqlDbType.NVarChar).Value = result.ResultCodeDescription;
                cmd.Parameters.Add("@systemTraceAuditNumber", SqlDbType.Int).Value = result.SystemTraceAuditNumber;
                if (result.LocalTransactionDate != DateTime.MinValue)
                    cmd.Parameters.Add("@localTransactionDate", SqlDbType.SmallDateTime).Value = result.LocalTransactionDate;
                cmd.Parameters.Add("@rrn", SqlDbType.NVarChar).Value = result.rrn;
                cmd.Parameters.Add("@autorizationIDResponse", SqlDbType.NVarChar).Value = result.AuthorizationIdResponse;
                SqlParameter param = new SqlParameter("@transferID", SqlDbType.Int);
                param.Direction = ParameterDirection.InputOutput;
                param.Value = order.TransferID;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                order.TransferID = Convert.ToUInt64(cmd.Parameters["@transferID"].Value);
            }

            result.TransferID = order.TransferID;
        }

        internal static CardIdentificationData GetCTCardIdentificationData(ulong customerNumber)
        {
            CardIdentificationData card = new CardIdentificationData();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT card_number, dbo.fnc_getCardExpireDate(card_number) ExpiryDate 
                                                                              FROM tbl_external_payments_system_configurations
                                                                              WHERE customer_number=@customerNumber ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {

                    card.CardNumber = dr["card_number"].ToString();
                    card.ExpiryDate = dr["ExpiryDate"].ToString();
                }
                else
                {
                    card = null;
                }

            }

            return card;
        }

        internal static bool CheckOrderID(C2CTransferOrder order)
        {
            CardIdentificationData card = new CardIdentificationData();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT 1
                                                                          FROM tbl_card_to_card_transfers
                                                                          WHERE order_ID = @orderID AND source_ID = @sourceID", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@orderID", SqlDbType.Float).Value = order.OrderID;
                    cmd.Parameters.Add("@sourceID", SqlDbType.Int).Value = order.SourceID;
                    SqlDataReader dr = cmd.ExecuteReader();
                    return dr.Read();
                }

            }

        }


        internal static long GetC2CTransferIDFromOrderID(long orderID)
        {
            long transferID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT transfer_ID
        					                                                        FROM Tbl_card_to_card_transfers
                                                                                    WHERE order_ID = @orderID", conn))
                {
                    //transfer_ID
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@orderID", SqlDbType.Float).Value = orderID;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        transferID = Convert.ToUInt32(dr["transfer_ID"]);

                    }
                    else
                    {
                        transferID = 0;
                    }

                }
            }

            return transferID;
        }

        //internal static C2CTransferStatus GetC2CTransferStatus(long transferID)
        //{
        //    C2CTransferStatus status = new C2CTransferStatus();

        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
        //    {
        //        conn.Open();

        //        SqlCommand cmd = new SqlCommand(@"SELECT status
        //					FROM Tbl_card_to_card_transfers
        //                                            WHERE transfer_ID = @transferID", conn);
        //        //transfer_ID
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.Add("@transferID", SqlDbType.Float).Value = transferID;

        //        SqlDataReader dr = cmd.ExecuteReader();

        //        if (dr.Read())
        //        {
        //            status.StatusCode = Convert.ToUInt16(dr["status"]);

        //        }
        //        else
        //        {
        //            status.StatusCode = 0;
        //        }

        //    }

        //    return status;
        //}

        //internal static C2CTransferStatus GetC2CTransferStatusByOrderID(long orderID, int sourceID)
        //{
        //    C2CTransferStatus status = new C2CTransferStatus();

        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
        //    {
        //        conn.Open();

        //        SqlCommand cmd = new SqlCommand(@"SELECT status
        //                                            FROM tbl_card_to_card_transfers
        //                                            WHERE order_id=@orderID AND source_ID = @sourceID", conn);
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderID;
        //        cmd.Parameters.Add("@sourceID", SqlDbType.Float).Value = sourceID;

        //        SqlDataReader dr = cmd.ExecuteReader();

        //        if (dr.Read())
        //        {
        //            status.StatusCode = Convert.ToUInt16(dr["status"]);
        //            //1 նոր գրանցում
        //            //10 հաջողված փոխանցում
        //            // 20 արտաքին սխալ
        //            // 30 ներքին սխալ
        //            switch (status.StatusCode)
        //            {
        //                case 1:
        //                    {
        //                        status.StatusDescription = "Նոր գրանցում";
        //                        break;
        //                    }
        //                case 10:
        //                    {
        //                        status.StatusDescription = "Հաջողված փոխանցում";
        //                        break;
        //                    }
        //                case 20:
        //                    {
        //                        status.StatusDescription = "Արտաքին սխալ";
        //                        break;
        //                    }
        //                case 30:
        //                    {
        //                        status.StatusDescription = "Ներքին սխալ";
        //                        break;
        //                    }
        //            }
        //        }
        //        else
        //        {
        //            status.StatusCode = 0;
        //            status.StatusDescription = "Գտնված չէ։";
        //        }

        //    }

        //    return status;
        //}

    }
}
