using ExternalBanking.SecuritiesTrading;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.SecuritiesTrading
{
    internal class SecuritiesTradingOrderCancellationOrderDB
    {

        internal static ActionResult Save(SecuritiesTradingOrderCancellationOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "pr_submit_securities_trading_order_cancellation_order";
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
                cmd.Parameters.Add("@quality", SqlDbType.Int).Value = order.Quality;
                cmd.Parameters.Add("@description", SqlDbType.NVarChar, 200).Value = order.Description;
                cmd.Parameters.Add("@securities_trading_order_id", SqlDbType.Int).Value = order.SecuritiesTradingOrderId;


                if (order.GroupId != 0)
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;


                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

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

        internal static bool CheckCancellationOrder(long securitiesTradingOrderId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_securities_trading_cancellation_order_details WHERE security_trading_order_doc_id = @DocID ", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = securitiesTradingOrderId;
                return cmd.ExecuteReader().Read();
            }
        }


        internal static SecuritiesTradingOrderCancellationOrder Get(long id)
        {
            DataTable dt = new DataTable();
            SecuritiesTradingOrderCancellationOrder order = new SecuritiesTradingOrderCancellationOrder();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT hb.*, c.*, d.*, tt.description_arm AS trading_order_types_description, st.description_arm AS securities_type_description
		                                                FROM Tbl_HB_documents hb 
                                                        INNER JOIN Tbl_securities_trading_cancellation_order_details c ON  hb.doc_ID = c.Doc_ID
                                                        INNER JOIN Tbl_Securities_Trading_Order_Details d ON c.security_trading_order_doc_id = d.doc_id  
									                    LEFT JOIN Tbl_type_of_trading_order_types tt ON d.trading_order_type = tt.id
									                    LEFT JOIN Tbl_type_of_securities st ON d.security_type = st.id                                         
                                                        WHERE hb.Doc_ID=@DocID ", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = id;
                dt.Load(cmd.ExecuteReader());

                order = SetSecuritiesTradingOrderCancellationOrder(dt.Rows[0]);
            }
            return order;
        }

        private static SecuritiesTradingOrderCancellationOrder SetSecuritiesTradingOrderCancellationOrder(DataRow dt)
        {
            SecuritiesTradingOrderCancellationOrder order = new SecuritiesTradingOrderCancellationOrder();

            order.Id = long.Parse(dt["doc_id"].ToString());
            order.CustomerNumber = ulong.Parse(dt["customer_number"].ToString());
            order.OrderNumber = dt["document_number"].ToString();
            order.RegistrationDate = Convert.ToDateTime(dt["registration_date"]);
            order.Type = (OrderType)dt["document_type"];
            order.SubType = Convert.ToByte(dt["document_subtype"]);
            order.Quality = (OrderQuality)dt["quality"];
            order.SecuritiesTradingOrderId = long.Parse(dt["security_trading_order_doc_id"].ToString());
            order.Source = (SourceType)int.Parse(dt["source_type"].ToString());

            order.ISIN = dt["ISIN"] != DBNull.Value ? dt["ISIN"].ToString() : string.Empty;
            order.Quantity = dt["quantity"] != DBNull.Value ? int.Parse(dt["quantity"].ToString()) : default;
            order.TradingOrderType = dt["trading_order_type"] != DBNull.Value ? (SecuritiesTradingOrderTypes)short.Parse(dt["trading_order_type"].ToString()) : default;
            order.TradingOrderTypeDescription = dt["trading_order_types_description"].ToString();
            order.SecurityType = (SharesTypes)int.Parse(dt["security_type"].ToString());
            order.SecurityTypeDescription = dt["securities_type_description"].ToString();
            order.DepositoryAccount = new DepositaryAccount();
            order.DepositoryAccount.AccountNumber = dt["depository_account"] != DBNull.Value ? Convert.ToDouble(dt["depository_account"].ToString()) : 0;
            order.DepositoryAccount.Description = dt["depository_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt["depository_account_description"].ToString()) : "";
            order.DepositoryAccount.BankCode = dt["depository_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dt["depository_account_bank_code"]) : 0;
            order.ReferenceNumber = dt["reference_number"].ToString();
            order.ExpirationType = (SecuritiesTradingOrderExpirationType)short.Parse(dt["trading_order_expiration_type"].ToString());
            order.ExpirationTypeDescription = order.ExpirationType == SecuritiesTradingOrderExpirationType.MarketDayEnd ? "Մինչև առևտրային օրվա ավարտը" : "Ուժի մեջ է մինչև հաճախորդի կողմից չեղարկումը";
            order.LimitPrice = dt["limit_price"] != DBNull.Value ? Convert.ToDouble(dt["limit_price"]) : default;
            order.StopPrice = dt["stop_price"] != DBNull.Value ? Convert.ToDouble(dt["stop_price"]) : default;
            order.MarketTradedQuantity = SecuritiesMarketTradingOrder.GetMarketTradedQuantity(order.SecuritiesTradingOrderId);


            return order;
        }

    }
}
