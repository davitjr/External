using ExternalBanking.SecuritiesTrading;
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
    class SecuritiesTradingDB
    {
        internal static Dictionary<int, List<SentSecuritiesTradingOrder>> GetSentSecuritiesTradingOrders(SecuritiesTradingFilter filter)
        {
            List<SentSecuritiesTradingOrder> securitiesTradingList = new List<SentSecuritiesTradingOrder>();
            Dictionary<int, List<SentSecuritiesTradingOrder>> result = new Dictionary<int, List<SentSecuritiesTradingOrder>>();
            string sql = "";
            int count = 0;
            string where = "";
            string join;
            string selectPropertys = " ";
            if (filter.Page == 0)
                filter.Page++;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    if (filter.Id != 0)
                    {
                        where += " and hb.doc_id = @id ";
                        command.Parameters.Add("@id", SqlDbType.Int).Value = filter.Id;
                    }

                    if (filter.CustomerNumber != 0)
                    {
                        where += " and hb.customer_number = @customer_number ";
                        command.Parameters.Add("@customer_number", SqlDbType.Float).Value = filter.CustomerNumber;
                    }

                    if (filter.Quality != OrderQuality.NotDefined)
                    {
                        where += " and hb.quality = @quality ";
                        command.Parameters.Add("@quality", SqlDbType.Int).Value = filter.Quality;
                    }
                    else
                    {
                        if (filter.OrderType != OrderType.SecuritiesTradingOrderCancellationOrder)
                            where += " and hb.quality NOT IN (1, 3) ";
                    }

                    if (filter.StartDate != default(DateTime))
                    {
                        where += " and hb.registration_date >= @start_date ";
                        command.Parameters.Add("@start_date", SqlDbType.DateTime).Value = filter.StartDate.ToString("dd/MMM/yyyy");
                    }

                    if (filter.EndDate != default(DateTime))
                    {
                        where += " and hb.registration_date <= @end_date ";
                        command.Parameters.Add("@end_date", SqlDbType.DateTime).Value = filter.EndDate.ToString("dd/MMM/yyyy");
                    }

                    if (!string.IsNullOrEmpty(filter.Currency))
                    {
                        if (filter.OrderType == OrderType.SecuritiesTradingOrderCancellationOrder)
                            where += " and ohb.currency_order = @currency ";
                        else
                            where += " and hb.currency = @currency ";

                        command.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = filter.Currency;
                    }

                    if (!String.IsNullOrEmpty(filter.ISIN))
                    {
                        where += " and d.ISIN = @ISIN ";
                        command.Parameters.Add("@ISIN", SqlDbType.NVarChar, 40).Value = filter.ISIN;
                    }

                    if (filter.IssueSeria != null)
                    {
                        where += " and b.issue_seria = @issueSeria ";
                        command.Parameters.Add("@issueSeria", SqlDbType.Int).Value = filter.IssueSeria;
                    }

                    if (filter.TradingOrderType != SecuritiesTradingOrderTypes.None)
                    {
                        where += " and d.trading_order_type = @tradingOrderType ";
                        command.Parameters.Add("@tradingOrderType", SqlDbType.Int).Value = filter.TradingOrderType;
                    }

                    if (filter.SecurityType != SharesTypes.None)
                    {
                        where += " and d.security_type = @securityType ";
                        command.Parameters.Add("@securityType", SqlDbType.Int).Value = filter.SecurityType;
                    }

                    if (!string.IsNullOrEmpty(filter.BrokerageCode))
                    {
                        where += " and brokerage_code = @brokerage_code ";
                        command.Parameters.Add("@brokerage_code", SqlDbType.NVarChar, 100).Value = filter.BrokerageCode;
                    }

                    if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                    {
                        where += " and reference_number = @reference_number ";
                        command.Parameters.Add("@reference_number", SqlDbType.NVarChar, 100).Value = filter.ReferenceNumber;
                    }

                    if (!string.IsNullOrEmpty(filter.Ticker))
                    {
                        where += " and ticker = @ticker ";
                        command.Parameters.Add("@ticker", SqlDbType.NVarChar, 100).Value = filter.Ticker;
                    }

                    if (filter.ExpirationType != SecuritiesTradingOrderExpirationType.None)
                    {
                        where += " and trading_order_expiration_type = @trading_order_expiration_type ";
                        command.Parameters.Add("@trading_order_expiration_type", SqlDbType.Int).Value = filter.ExpirationType;
                    }

                    if (filter.OrderType == OrderType.SecuritiesTradingOrderCancellationOrder)
                    {
                        where += " and hb.document_type = @document_type ";
                        command.Parameters.Add("@document_type", SqlDbType.Int).Value = filter.OrderType;
                        join = @" LEFT JOIN Tbl_securities_trading_cancellation_order_details co on hb.doc_ID = co.doc_id
                                          LEFT JOIN Tbl_Securities_Trading_Order_Details d on co.security_trading_order_doc_id = d.doc_id 
                                          LEFT JOIN (SELECT doc_id, currency as currency_order, amount as amount_order FROM Tbl_HB_documents) ohb on d.doc_id = ohb.doc_id  ";
                        selectPropertys = ", currency_order, amount_order ";
                    }
                    else if (filter.OrderType == OrderType.SecuritiesBuyOrder || filter.OrderType == OrderType.SecuritiesSellOrder)
                    {
                        where += " and hb.document_type = @document_type  ";
                        command.Parameters.Add("@document_type", SqlDbType.Int).Value = filter.OrderType;
                        join = " LEFT JOIN Tbl_Securities_Trading_Order_Details d on hb.doc_ID = d.doc_id ";

                    }
                    else
                    {
                        where += " and hb.document_type IN (261, 262) ";
                        join = " LEFT JOIN Tbl_Securities_Trading_Order_Details d on hb.doc_ID = d.doc_id ";
                    }


                    sql = @"DECLARE @count INT  = 1 
                                SET @count = (SELECT  COUNT(*) AS count
                                    FROM Tbl_HB_documents as hb "
                                        + join +
                                    @"LEFT JOIN (SELECT TOP 1 [ISIN], [issue_seria] FROM Tbl_bond_issue  WHERE issue_seria IS NOT NULL) b on d.ISIN = b.ISIN
                                    WHERE 1 = 1 " + where + @")
                                SELECT @count as count
                                SELECT row, doc_id, filial, customer_number, registration_date, currency, amount, document_type, quality, source_type, ISNULL(full_name, '') as full_name, ISIN, description_arm, quantity, trading_order_type, issue_seria, depository_account,depository_account_description, depository_account_bank_code, reference_number, security_type, trading_order_expiration_type, trading_order_types_description, securities_type_description, limit_price, stop_price, brokerage_code, ticker, change_date, is_deposited, market_price " + selectPropertys +
                                    @"FROM (SELECT ROW_NUMBER() OVER(order by hb.doc_id desc) as row, hb.doc_id,filial, hb.customer_number, registration_date, currency, amount, hb.document_type , hb.quality, source_type, full_name, d.ISIN, q.description_arm, quantity, trading_order_type, issue_seria, depository_account,depository_account_description, depository_account_bank_code, reference_number, security_type, trading_order_expiration_type, tt.description_arm AS trading_order_types_description, st.description_arm AS securities_type_description, limit_price, stop_price, brokerage_code, ticker, change_date, is_deposited, market_price " + selectPropertys +
                                    @"FROM Tbl_HB_documents as hb
									INNER JOIN (SELECT quality, description_arm FROM Tbl_types_of_HB_quality) q on hb.quality = q.quality
									INNER JOIN tbl_hb_quality_history qh ON hb.doc_id = qh.doc_id 
									LEFT JOIN (SELECT  customer_number, CASE WHEN type_of_client = 6 THEN  name + ' ' + lastName + ' ' + middleName ELSE Description END AS full_name FROM V_U_CustomerDescription) n on hb.customer_number = n.customer_number"
                                    + join +
                                    @"LEFT JOIN Tbl_type_of_trading_order_types tt on d.trading_order_type = tt.id
									LEFT JOIN Tbl_type_of_securities st on d.security_type = st.id
                                    LEFT JOIN (SELECT TOP 1 [ISIN], [issue_seria] FROM Tbl_bond_issue WHERE issue_seria IS NOT NULL) b on d.ISIN = b.ISIN
                                    WHERE 1 = 1  and qh.quality = 3 " + where + " ) a WHERE row >= @first_count and row <= CASE WHEN @count  < 100 THEN @count  ELSE @secont_count END ";

                    command.Parameters.Add("@first_count", SqlDbType.Int).Value = (filter.Page - 1) * 100;
                    command.Parameters.Add("@secont_count", SqlDbType.Int).Value = filter.Page * 100;

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    conn.Open();

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                            count = dr["count"] != DBNull.Value ? Convert.ToInt32(dr["count"]) : 0;
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                SentSecuritiesTradingOrder order = new SentSecuritiesTradingOrder();

                                order.Row = int.Parse(dr["row"].ToString());
                                order.OrderId = int.Parse(dr["doc_Id"].ToString());
                                order.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                                order.FullName = dr["full_name"].ToString();
                                order.FilialCode = dr["filial"] != DBNull.Value ? int.Parse(dr["filial"].ToString()) : default;
                                order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                                if (filter.OrderType == OrderType.SecuritiesTradingOrderCancellationOrder)
                                {
                                    order.Currency = dr["currency_order"].ToString();
                                    order.Amount = Convert.ToDouble(dr["amount_order"]);
                                }
                                else
                                {
                                    order.Currency = dr["currency"].ToString();
                                    order.Amount = Convert.ToDouble(dr["amount"]);
                                }

                                order.Type = (OrderType)dr["document_type"];
                                order.TypeDescription = order.Type == OrderType.SecuritiesBuyOrder ? "Առք" : order.Type == OrderType.SecuritiesSellOrder ? "Վաճառք" : "Չեղարկում";
                                order.Quality = (OrderQuality)dr["quality"];
                                order.QualityDescription = Utility.ConvertAnsiToUnicode(dr["description_arm"].ToString());
                                if (order.Quality == OrderQuality.Sent3 && order.Type == OrderType.SecuritiesTradingOrderCancellationOrder)
                                    order.QualityDescription = "ՈՒղարկված է բանկ";
                                order.Source = (SourceType)int.Parse(dr["source_type"].ToString());
                                order.ChangeDate = Convert.ToDateTime(dr["change_date"]);
                                order.ISIN = dr["ISIN"] != DBNull.Value ? dr["ISIN"].ToString() : string.Empty;
                                order.Quantity = dr["quantity"] != DBNull.Value ? int.Parse(dr["quantity"].ToString()) : default;
                                order.TradingOrderType = dr["trading_order_type"] != DBNull.Value ? (SecuritiesTradingOrderTypes)short.Parse(dr["trading_order_type"].ToString()) : default;
                                order.TradingOrderTypeDescription = dr["trading_order_types_description"].ToString();
                                order.SecurityType = (SharesTypes)int.Parse(dr["security_type"].ToString());
                                order.SecurityTypeDescription = dr["securities_type_description"].ToString();
                                order.IssueSeria = dr["issue_seria"] != DBNull.Value ? int.Parse(dr["issue_seria"].ToString()) : default;
                                order.DepositoryAccount = new DepositaryAccount();
                                order.DepositoryAccount.AccountNumber = dr["depository_account"] != DBNull.Value ? Convert.ToDouble(dr["depository_account"].ToString()) : 0;
                                order.DepositoryAccount.Description = dr["depository_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["depository_account_description"].ToString()) : "";
                                order.DepositoryAccount.BankCode = dr["depository_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dr["depository_account_bank_code"]) : 0;
                                order.ReferenceNumber = dr["reference_number"].ToString();
                                order.ExpirationType = (SecuritiesTradingOrderExpirationType)short.Parse(dr["trading_order_expiration_type"].ToString());
                                order.ExpirationTypeDescription = order.ExpirationType == SecuritiesTradingOrderExpirationType.MarketDayEnd ? "GTD" : "GTC";
                                order.LimitPrice = dr["limit_price"] != DBNull.Value ? Convert.ToDouble(dr["limit_price"]) : default;
                                order.StopPrice = dr["stop_price"] != DBNull.Value ? Convert.ToDouble(dr["stop_price"]) : default;
                                order.BrokerageCode = dr["brokerage_code"] != DBNull.Value ? dr["brokerage_code"].ToString() : String.Empty;
                                order.Ticker = dr["ticker"] != DBNull.Value ? dr["ticker"].ToString() : String.Empty;
                                order.IsDeposited = dr["is_deposited"] != DBNull.Value ? Convert.ToBoolean(dr["is_deposited"].ToString()) : false;
                                order.MarketPrice = dr["market_price"] != DBNull.Value ? Convert.ToDouble(dr["market_price"]) : default;
                                order.MarketTradedQuantity = SecuritiesMarketTradingOrder.GetMarketTradedQuantity(order.OrderId);

                                securitiesTradingList.Add(order);
                            }
                        }
                    }

                }
            }
            result.Add(count, securitiesTradingList);
            return result;
        }

    }
}
