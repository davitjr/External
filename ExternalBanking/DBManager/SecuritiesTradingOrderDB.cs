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
    internal class SecuritiesTradingOrderDB
    {
        internal static ActionResult SaveSecuritiesTradingOrder(SecuritiesTradingOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            string creditAccountNumber = "";
            int creditBankCode = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_securities_trading_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    if (order.Type == OrderType.SecuritiesSellOrder)
                    {
                        creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);
                        if (int.TryParse(order.ReceiverAccount.AccountNumber.Substring(0, 5), out int bankCode))
                        {
                            creditBankCode = bankCode;
                        }

                        cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = creditBankCode;
                        cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;
                    }
                    else if (order.Type == OrderType.SecuritiesBuyOrder)
                    {
                        cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;
                        if (order.FeeAccount != null && order.FeeAccount.AccountNumber != "0")
                        {
                            cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                        }
                    }

                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;

                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = order.SubType;

                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@limit_price", SqlDbType.Float).Value = order.LimitPrice;
                    cmd.Parameters.Add("@stop_price", SqlDbType.Float).Value = order.StopPrice;

                    cmd.Parameters.Add("@market_price", SqlDbType.Float).Value = order.MarketPrice;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@trading_platform", SqlDbType.NVarChar).Value = order.TradingPlatform;

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    cmd.Parameters.Add("@depository_account", SqlDbType.BigInt).Value = order.DepositoryAccount != null ? order.DepositoryAccount.AccountNumber : 0;
                    cmd.Parameters.Add("@depository_account_bank_code", SqlDbType.Int).Value = order.DepositoryAccount != null ? order.DepositoryAccount.BankCode : 0;
                    cmd.Parameters.Add("@depository_account_description", SqlDbType.NVarChar).Value = order.DepositoryAccount != null ? order.DepositoryAccount.Description : null;
                    cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = order.Quantity;
                    cmd.Parameters.Add("@security_type", SqlDbType.Int).Value = order.SecurityType;
                    cmd.Parameters.Add("@security_sub_type", SqlDbType.SmallInt).Value = order.SecuritySubType;
                    cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = order.ISIN;
                    cmd.Parameters.Add("@issuer_name_am", SqlDbType.NVarChar, 250).Value = order.IssuerNameAM;
                    cmd.Parameters.Add("@issuer_name_en", SqlDbType.NVarChar, 250).Value = order.IssuerNameEN;
                    cmd.Parameters.Add("@is_bank_security", SqlDbType.Bit).Value = order.IsBankSecurity;
                    cmd.Parameters.Add("@brokerage_code", SqlDbType.NVarChar,50).Value = order.BrokerageCode;
                    cmd.Parameters.Add("@ticker", SqlDbType.NVarChar, 50).Value = order.Ticker;
                    cmd.Parameters.Add("@trading_order_type", SqlDbType.SmallInt).Value = order.TradingOrderType;
                    cmd.Parameters.Add("@trading_order_expiration_type", SqlDbType.SmallInt).Value = order.ExpirationType;
                    cmd.Parameters.Add("@acknowledgedByCheckBox", SqlDbType.Bit).Value = order.AcknowledgedByCheckBox;
                    cmd.Parameters.Add("@acknowledgementText", SqlDbType.NVarChar, 500).Value = order.AcknowledgementText ?? "";

                    cmd.Parameters.Add("@volume", SqlDbType.Float).Value = order.Volume;
                    cmd.Parameters.Add("@reference_number", SqlDbType.NVarChar).Value = order.ReferenceNumber;
                    cmd.Parameters.Add("@yield", SqlDbType.Float).Value = order.Yield;
                    cmd.Parameters.Add("@stop_yield", SqlDbType.Float).Value = order.StopYield;

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
        }

        internal static SecuritiesTradingOrder GetSecuritiesTradingOrder(SecuritiesTradingOrder order, Languages Language)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.debet_account,d.credit_account,d.credit_bank_code,d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,d.amount_for_payment,d.deb_for_transfer_payment,d.currency, n.*,d.operation_date,d.order_group_id,d.confirmation_date,reject_id
		                                           FROM Tbl_HB_documents as d inner join Tbl_Securities_Trading_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                SetSecuritiesTradingOrder(dt, order, Language, true);
            }
            return order;
        }

        internal static SecuritiesTradingOrder GetSecuritiesTradingOrderById(long docId, Languages Language)
        {
            DataTable dt = new DataTable();
            SecuritiesTradingOrder order = new SecuritiesTradingOrder();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT *
		                                           FROM Tbl_HB_documents as d inner join Tbl_Securities_Trading_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE d.Doc_ID=@DocID ", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = docId;
                dt.Load(cmd.ExecuteReader());

                SetSecuritiesTradingOrder(dt, order, Language, false);
            }
            return order;
        }

        private static SecuritiesTradingOrder SetSecuritiesTradingOrder(DataTable dt, SecuritiesTradingOrder order, Languages Language, bool getMarketTrandingOrder)
        {
            order.Id = long.Parse(dt.Rows[0]["doc_id"].ToString());
            order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
            order.OrderNumber = dt.Rows[0]["document_number"].ToString();
            order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
            order.Currency = dt.Rows[0]["currency"].ToString();
            order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
            order.Type = (OrderType)(dt.Rows[0]["document_type"]);
            order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
            order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);

            if (getMarketTrandingOrder)
                order.SecuritiesMarketTradingOrderList = SecuritiesMarketTradingOrderDB.Get(order.Id);
            
            if (order.Type == OrderType.SecuritiesSellOrder)
            {
                string creditAccountNumber = dt.Rows[0]["credit_bank_code"].ToString() + dt.Rows[0]["credit_account"].ToString();
                order.ReceiverAccount = Account.GetSystemAccount(creditAccountNumber);
            }
            else if (order.Type == OrderType.SecuritiesBuyOrder)
            {
                order.DebitAccount = Account.GetSystemAccount(dt.Rows[0]["debet_account"].ToString());
                order.TransferFee = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                order.FeeAccount = Account.GetSystemAccount(dt.Rows[0]["deb_for_transfer_payment"].ToString());
            }


            var SecuritiesTypes = Info.GetSecuritiesTypes(Language);
            order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
            order.Quantity = int.Parse(dt.Rows[0]["quantity"].ToString());
            order.SecurityType = (SharesTypes)int.Parse(dt.Rows[0]["security_type"].ToString());
            order.SecurityTypeDescription = SecuritiesTypes.Where(x => (SharesTypes)x.Key == order.SecurityType).First().Value;
            order.SecuritySubType = byte.Parse(dt.Rows[0]["security_sub_type"].ToString());
            order.ISIN = dt.Rows[0]["ISIN"].ToString();
            order.IssuerNameAM = dt.Rows[0]["issuer_name_am"].ToString();
            order.IssuerNameEN = dt.Rows[0]["issuer_name_en"].ToString();
            order.IsBankSecurity = dt.Rows[0]["is_bank_security"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["is_bank_security"].ToString()) : false;
            order.BrokerageCode = dt.Rows[0]["brokerage_code"].ToString();
            order.Ticker = dt.Rows[0]["ticker"].ToString();
            order.MarketPrice = Convert.ToDouble(dt.Rows[0]["market_price"]);
            order.AcknowledgedByCheckBox = dt.Rows[0]["acknowledged_by_checkbox"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["acknowledged_by_checkbox"].ToString()) : false;
            order.AcknowledgementText= dt.Rows[0]["acknowledgement_text"].ToString();

            order.TradingOrderType = (SecuritiesTradingOrderTypes)short.Parse(dt.Rows[0]["trading_order_type"].ToString());
            var TradingTypes = Info.GetTradingOrderTypes(Language);
            order.TradingOrderTypeDescription = TradingTypes.Where(x => x.Key == (short)order.TradingOrderType).First().Value;

            order.ExpirationType = (SecuritiesTradingOrderExpirationType)short.Parse(dt.Rows[0]["trading_order_expiration_type"].ToString());
            var ExpirationTypes = Info.GetTradingOrderExpirationTypes(Language);
            order.ExpirationTypeDescription = ExpirationTypes.Where(x => x.Key == (short)order.ExpirationType).First().Value;
            
            order.LimitPrice = Convert.ToDouble(dt.Rows[0]["limit_price"]);
            order.StopPrice = Convert.ToDouble(dt.Rows[0]["stop_price"]);
            order.Yield = Convert.ToDouble(dt.Rows[0]["yield"]);
            order.StopYield = Convert.ToDouble(dt.Rows[0]["stop_yield"]);

            order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
            order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
            order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

            //order.Fees = new List<OrderFee>();
            //order.Fees.Add(new OrderFee());
            //order.Fees[0].Amount = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
            order.DepositoryAccount = new DepositaryAccount();
            order.DepositoryAccount.AccountNumber = dt.Rows[0]["depository_account"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["depository_account"].ToString()) : 0;
            order.DepositoryAccount.Description = dt.Rows[0]["depository_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt.Rows[0]["depository_account_description"].ToString()) : "";
            order.DepositoryAccount.BankCode = dt.Rows[0]["depository_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["depository_account_bank_code"]) : 0;
            order.Volume = Convert.ToDouble(dt.Rows[0]["volume"]);
            order.ReferenceNumber = dt.Rows[0]["reference_number"].ToString();
            order.TradingPlatform = dt.Rows[0]["trading_platform"].ToString();
            //order.MarketTradedQuantity = SecuritiesMarketTradingOrder.GetMarketTradedQuantity(order.Id);
            order.RejectReasonDescription = dt.Rows[0]["reject_reason"] != DBNull.Value ? dt.Rows[0]["reject_reason"].ToString() : null;
            order.BrokerContractId = BrokerContractOrder.GetBrokerContractId(order.CustomerNumber).Result;


            return order;
        }

        internal static List<SecuritiesTradingOrder> GetSecuritiesTradingOrders(ulong CustomerNumber, short QualityType, DateTime StartDate, DateTime EndDate, Languages Language)
        {
            var SecuritiesTypes = Info.GetSecuritiesTypes(Language);
            List<SecuritiesTradingOrder> orders=new List<SecuritiesTradingOrder>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string Quality = string.Empty;
                if (QualityType == 1)
                {
                    Quality = " d.quality in (3,20,60) and ";
                }
                else if (QualityType == 2)
                {
                    Quality = " d.quality in (30,31,32) and ";
                }
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.registration_date,d.document_type,d.document_subtype,d.quality, n.*
		                                           FROM Tbl_HB_documents as d inner join Tbl_Securities_Trading_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE "+ Quality + " d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.registration_date>= @startDate AND d.registration_date <= @endDate ORDER BY d.registration_date ASC ", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = CustomerNumber;
                cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = StartDate.Date;
                cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = EndDate.Date;
                dt.Load(cmd.ExecuteReader());

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];
                    SecuritiesTradingOrder order = new SecuritiesTradingOrder();

                    order.Id = long.Parse(row["doc_id"].ToString());
                    order.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                    order.Type = (OrderType)(row["document_type"]);
                    order.SubType = Convert.ToByte(row["document_subtype"]);
                    order.Quality = (OrderQuality)(row["quality"]);

                    order.Quantity = int.Parse(row["quantity"].ToString());
                    order.SecurityType = (SharesTypes)int.Parse(row["security_type"].ToString());
                    order.SecurityTypeDescription = SecuritiesTypes.Where(x => (SharesTypes)x.Key == order.SecurityType).First().Value;


                    order.ISIN = row["ISIN"].ToString();
                    order.IssuerNameAM = row["issuer_name_am"].ToString();
                    order.IssuerNameEN = row["issuer_name_en"].ToString();


                    orders.Add(order);
                }

            }
            return orders;
        }


        internal static void RejectOrder(long id, string description, OrderType type, int setNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = type == OrderType.SecuritiesTradingOrderCancellationOrder ? "pr_reject_securities_trading_order_cancellation_order" :  "pr_reject_securities_trading_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Float).Value = id;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetNextOperDay();
                    cmd.Parameters.Add("@setNumber", SqlDbType.Float).Value = setNumber;
                    cmd.Parameters.Add("@reject_reason_description", SqlDbType.NVarChar, 200).Value = description;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void UpdateDeposited(ulong docId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"UPDATE Tbl_Securities_Trading_Order_Details
                                        SET is_deposited = 1
                                        WHERE doc_id = @docId";
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static int GetCustomerSentSecuritiesTradingOrdersQuantity(string iSIN, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT SUM(quantity) as quantity FROM tbl_hb_documents hb
                                                                        INNER JOIN (SELECT ISIN, quantity, doc_id FROM  Tbl_Securities_Trading_Order_Details) d ON hb.doc_id = d.doc_id
                                                                        WHERE document_type = 262 AND quality IN (3, 50, 60) AND ISIN = @isin AND customer_number = @customerNumber ", conn);

                cmd.Parameters.Add("@isin", SqlDbType.NVarChar, 50).Value = iSIN;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                SqlDataReader dr =  cmd.ExecuteReader();
                if (dr.Read())
                    return dr["quantity"] != DBNull.Value ? int.Parse(dr["quantity"].ToString()) : 0;
                else
                    return 0;
            }
        }


    }
}
