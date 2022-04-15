using ExternalBanking.SecuritiesTrading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class SecuritiesMarketTradingOrderDB
    {

        internal static ActionResult Save(SecuritiesMarketTradingOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "pr_submit_securities_market_trading_order";
                cmd.CommandType = CommandType.StoredProcedure;

                if (order.Id != 0)
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = order.user.userName;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)order.Source;
                cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = order.ISIN;
                cmd.Parameters.Add("@trading_order_type", SqlDbType.SmallInt).Value = order.TradingOrderType;
                cmd.Parameters.Add("@order_id", SqlDbType.Int).Value = order.OrderId;
                cmd.Parameters.Add("@description", SqlDbType.NVarChar, 200).Value = order.Description;
                cmd.Parameters.Add("@quality", SqlDbType.Int).Value = order.Quality;

                cmd.Parameters.Add("@full_name", SqlDbType.NVarChar, 200).Value = order.FullName;
                cmd.Parameters.Add("@confirm_date", SqlDbType.Date).Value = order.ConfirmDate;
                cmd.Parameters.Add("@actually_quantity", SqlDbType.Int).Value = order.ActuallyQuantity;
                cmd.Parameters.Add("@unit_amount", SqlDbType.Float).Value = order.UnitAmount;
                cmd.Parameters.Add("@total_volume", SqlDbType.Float).Value = order.TotalVolume;
                cmd.Parameters.Add("@order_deal_other_side", SqlDbType.NVarChar, 200).Value = order.OrderDealOtherSide;
                cmd.Parameters.Add("@confirm_order_user_name", SqlDbType.NVarChar, 200).Value = order.ConfirmOrderUserName;
                cmd.Parameters.Add("@transaction_place", SqlDbType.NVarChar, 200).Value = order.TransactionPlace;
                cmd.Parameters.Add("@residual_quantity", SqlDbType.Float).Value = order.ResidualQuantity;

                if (order.GroupId != 0)
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;


                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();

                ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                {
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;

                }
                else if (actionResult == 0 || actionResult == 8)
                {
                    result.ResultCode = ResultCode.Failed;
                    result.Id = -1;
                    result.Errors.Add(new ActionError((short)actionResult));
                }

                return result;

            }

        }

        internal static List<SecuritiesMarketTradingOrder> Get(long orderId)
        {
            DataTable dt = new DataTable();
            List<SecuritiesMarketTradingOrder> result = new List<SecuritiesMarketTradingOrder>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT *
		                                           FROM Tbl_HB_documents HB 
                                                   INNER JOIN Tbl_securities_market_trading_order s ON hb.doc_ID = s.market_trading_order_id                                                 
                                                   WHERE s.order_id=@DocID ", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = orderId;
                dt.Load(cmd.ExecuteReader());

                foreach (DataRow item in dt.Rows)
                {
                    SecuritiesMarketTradingOrder order = new SecuritiesMarketTradingOrder();
                    order.Id = long.Parse(item["order_id"].ToString());
                    order.RegistrationDate = Convert.ToDateTime(item["registration_date"]);
                    order.Type = (OrderType)item["document_type"];
                    order.SubType = Convert.ToByte(item["document_subtype"]);
                    order.Quality = (OrderQuality)item["quality"];

                    order.OrderId = long.Parse(item["market_trading_order_id"].ToString());
                    order.FullName = item["full_name"].ToString();
                    order.ConfirmDate = Convert.ToDateTime(item["confirm_date"].ToString());
                    order.ActuallyQuantity = int.Parse(item["actually_quantity"].ToString());
                    order.UnitAmount = decimal.Parse(item["unit_amount"].ToString());
                    order.TotalVolume = decimal.Parse(item["total_volume"].ToString());
                    order.OrderDealOtherSide = item["order_deal_other_side"].ToString();
                    order.ConfirmOrderUserName = item["confirm_order_user_name"].ToString();
                    order.TransactionPlace = item["transaction_place"].ToString();
                    order.ResidualQuantity = decimal.Parse(item["residual_quantity"].ToString());

                    result.Add(order);
                }
            }
            return result;
        }

        internal static int GetMarketTradedQuantity(long orderId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(SUM(actually_quantity), 0) as actually_quantity
		                                           FROM Tbl_HB_documents HB 
                                                   INNER JOIN Tbl_securities_market_trading_order s ON hb.doc_ID = s.market_trading_order_id    
												   where order_id = @DocID AND quality = 30 ", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = orderId;
                SqlDataReader dr = cmd.ExecuteReader();
                return dr.Read() ? int.Parse(dr["actually_quantity"].ToString()) : 0;

            }
        }

    }
}
