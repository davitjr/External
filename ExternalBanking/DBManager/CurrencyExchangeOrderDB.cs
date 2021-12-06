using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class CurrencyExchangeOrderDB
    {
        public static void GetCurrencyExchangeDetails(CurrencyExchangeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM tbl_convertation_details where doc_ID=@docId";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = order.Id;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.AmountInAmd = Convert.ToDouble(dr["amount_amd"]);
                            order.AmountInCrossCurrency = Convert.ToDouble(dr["amount_cross"]);
                            order.ShortChange = Convert.ToDouble(dr["short_change"]);
                            order.RoundingDirection = (ExchangeRoundingDirectionType)Convert.ToByte(dr["rounding_direction"]);

                            if (dr["order_number_for_debet"] != DBNull.Value)
                            {
                                order.OrderNumberForDebet = dr["order_number_for_debet"].ToString();
                            }

                            if (dr["order_number_for_credit"] != DBNull.Value)
                            {
                                order.OrderNumberForCredit = dr["order_number_for_credit"].ToString();
                            }

                            if (dr["order_number_for_short_change"] != DBNull.Value)
                            {
                                order.OrderNumberForShortChange = dr["order_number_for_short_change"].ToString();
                            }

                            if (dr["is_variation"] != DBNull.Value)
                            {
                                order.IsVariation = (ExchangeRateVariationType)(dr["is_variation"]);
                            }

                            if (dr["cross_rate_full"] != DBNull.Value)
                            {
                                order.ConvertationCrossRate = Convert.ToDouble(dr["cross_rate_full"]);
                            }

                        }
                    }

                }
            }
        }

        public static void Save(CurrencyExchangeOrder order, string userName, SourceType source)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"

                        DELETE FROM tbl_convertation_details
                        WHERE doc_ID = @docID

                        Insert Into tbl_convertation_details 
                        ( 
                            doc_ID,
                            amount_amd,
                            amount_cross,
                            short_change,
                            rounding_direction,
                            order_number_for_debet,
                            order_number_for_credit,
                            order_number_for_short_change,
                            is_variation,
                            cross_rate_full,
                            unique_number) 
                            VALUES
                        (
                            @docID,
                            @amountInAmd,
                            @amountInCrossCurrency,
                            @shortChange,
                            @roundingDirection,
                            @orderNumberForDebet,
                            @orderNumberForCredit,
                            @orderNumberForShortChange,
                            @isVariation,
                            @crossRateFull,
                            @uniqueNumber
                        )";

                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@amountInAmd", SqlDbType.Float).Value = order.AmountInAmd;
                    cmd.Parameters.Add("@amountInCrossCurrency", SqlDbType.Float).Value = order.AmountInCrossCurrency;
                    cmd.Parameters.Add("@shortChange", SqlDbType.Float).Value = order.ShortChange;
                    cmd.Parameters.Add("@roundingDirection", SqlDbType.TinyInt).Value = order.RoundingDirection;
                    cmd.Parameters.Add("@orderNumberForDebet", SqlDbType.NVarChar, 20).Value = order.OrderNumberForDebet == null ? "" : order.OrderNumberForDebet;
                    cmd.Parameters.Add("@orderNumberForCredit", SqlDbType.NVarChar, 20).Value = order.OrderNumberForCredit==null?"":order.OrderNumberForCredit;
                    cmd.Parameters.Add("@orderNumberForShortChange", SqlDbType.NVarChar, 20).Value = order.OrderNumberForShortChange == null ? "" : order.OrderNumberForShortChange;
                    cmd.Parameters.Add("@isVariation", SqlDbType.TinyInt).Value = order.IsVariation;
                    cmd.Parameters.Add("@crossRateFull", SqlDbType.Float).Value = order.ConvertationCrossRate;
                    cmd.Parameters.Add("@uniqueNumber", SqlDbType.BigInt).Value = order.UniqueNumber;

                    cmd.ExecuteNonQuery();
                }
            } 
        }

        internal static ActionResult SaveCurrencyExchangeOrderDetails(CurrencyExchangeOrder currencyExchangeOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_currency_exchange_order_details(order_id, amount_in_amd
                                            ,amount_in_cross_currency,short_change,rounding_direction
                                            ,order_number_for_debet,order_number_for_credit
                                            ,order_number_for_short_change,is_variation,cross_rate_full
                                            ,unique_number) 
                                      VALUES(@orderId, @amountInAmd, @amountInCrossCurrency, @shortChange, @roundingDirection
                                            ,@orderNumberForDebet, @orderNumberForCredit, @orderNumberForShortChange, @isVariation
                                            ,@crossRateFull, @uniqueNumber)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@amountInAmd", SqlDbType.Float).Value = (object)currencyExchangeOrder.AmountInAmd ?? DBNull.Value;
                    cmd.Parameters.Add("@amountInCrossCurrency", SqlDbType.Float).Value = (object)currencyExchangeOrder.AmountInCrossCurrency ?? DBNull.Value;
                    cmd.Parameters.Add("@shortChange", SqlDbType.Float).Value = (object)currencyExchangeOrder.ShortChange ?? DBNull.Value;
                    cmd.Parameters.Add("@roundingDirection", SqlDbType.TinyInt).Value = (object)currencyExchangeOrder.RoundingDirection ?? DBNull.Value;
                    cmd.Parameters.Add("@orderNumberForDebet", SqlDbType.NVarChar, 20).Value = (object)currencyExchangeOrder.OrderNumberForDebet ?? DBNull.Value;
                    cmd.Parameters.Add("@orderNumberForCredit", SqlDbType.NVarChar, 20).Value = (object)currencyExchangeOrder.OrderNumberForCredit ?? DBNull.Value;
                    cmd.Parameters.Add("@orderNumberForShortChange", SqlDbType.NVarChar, 20).Value = (object)currencyExchangeOrder.OrderNumberForShortChange ?? DBNull.Value;
                    cmd.Parameters.Add("@isVariation", SqlDbType.Bit).Value = (object)currencyExchangeOrder.IsVariation ?? DBNull.Value;
                    cmd.Parameters.Add("@crossRateFull", SqlDbType.Float).Value = (object)currencyExchangeOrder.ConvertationCrossRate ?? DBNull.Value;
                    cmd.Parameters.Add("@uniqueNumber", SqlDbType.BigInt).Value = (object)currencyExchangeOrder.UniqueNumber ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        internal static void GetHBCurrencyExchangeDetails(CurrencyExchangeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT amount,rate_sell_buy,rate_sell_buy_cross,amount_cross,is_variation,rounding_direction,cross_rate_full 
                                        FROM Tbl_HB_documents d 
                                        LEFT JOIN tbl_convertation_details cd
									    ON d.doc_id = cd.doc_ID
                                        where d.doc_ID=@docId";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = order.Id;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                       
                            if (dr.Read())
                            {
                                order.AmountInAmd = Convert.ToDouble(dr["amount"]) * Convert.ToDouble(dr["rate_sell_buy"]);
                                if (!string.IsNullOrEmpty(dr["amount_cross"].ToString()))
                                {
                                    order.AmountInCrossCurrency = Convert.ToDouble(dr["amount_cross"].ToString());
                                }
                                order.ShortChange = 0;
                                if (!string.IsNullOrEmpty(dr["is_variation"].ToString()))
                                {
                                    order.IsVariation = (ExchangeRateVariationType)(Convert.ToByte(dr["is_variation"].ToString()));
                                }

                                if (!string.IsNullOrEmpty(dr["rounding_direction"].ToString()))
                                {
                                    order.RoundingDirection = (ExchangeRoundingDirectionType)(Convert.ToByte(dr["rounding_direction"].ToString()));
                                }
                                else
                                {
                                    order.RoundingDirection = ExchangeRoundingDirectionType.ToAMD;
                                }

                            if (!string.IsNullOrEmpty(dr["cross_rate_full"].ToString()))
                            {
                                order.ConvertationCrossRate = Convert.ToDouble(dr["cross_rate_full"].ToString());
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(dr["rate_sell_buy_cross"].ToString()))
                                {
                                    order.ConvertationCrossRate = Convert.ToDouble(dr["rate_sell_buy_cross"]);
                                }
                            }
                            
                        }
                        
                    }

                }
            }
        }

        internal static SourceType GetOrderSourceType(long Id)
        {
            SourceType type = SourceType.Bank;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "select source_type from Tbl_HB_documents where doc_ID = " + Id.ToString();

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        type = (SourceType)Convert.ToInt32(reader["source_type"]);
                    }
                }
            }

            return type;
        }

        internal static Dictionary<string, string> GetOrderDetailsForReport(long orderId, ulong customerNumber)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "sp_printExchange";

               using SqlCommand cmd = new SqlCommand(sqltext, conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Float).Value = orderId;
                cmd.CommandType = CommandType.StoredProcedure;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }

            details.Add("cust_adress", dt.Rows[0]["cust_adress"].ToString());

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = "select C.customer_number, C.residence[sitizen], P.birth[birth],  FN .nameEng[name_eng], FN .lastNameEng[lastname_eng], " +
                    "O.description_eng[descr_eng], DocTax.document_number[code_of_tax], Em.emailAddress[email], " +
                    "DocDef.document_number[passport_number], DocDef.document_given_date[passport_date], DocDef.document_given_by[passport_inf], " +
                    "Addr.* from  Tbl_Customers C WITH (nolock) LEFT JOIN Tbl_Persons P WITH (nolock) ON C.identityId = P.identityId " +
                    "LEFT JOIN V_FullNames FN WITH (nolock) ON FN .id = P.fullNameId LEFT JOIN Tbl_Organisations O WITH (nolock) " +
                    "ON C.identityId = O.identityId OUTER APPLY (SELECT TOP 1 document_number " +
                    "FROM Tbl_customer_documents_current WITH (nolock) WHERE document_type = 19 AND identityId = c.identityId) DocTax " +
                    "OUTER APPLY (SELECT  TOP 1 emailAddress FROM   V_CustomerEmails WITH (nolock) WHERE  emailType = 5 " +
                    "AND identityId = c.identityId) Em OUTER APPLY (SELECT TOP 1 document_number, document_given_date, document_given_by " +
                    "FROM Tbl_customer_documents_current WITH (nolock) WHERE is_default = 1 AND identityId = c.identityId AND  c.type_of_client = 6) " +
                    "DocDef OUTER APPLY (SELECT  country sender_country FROM   V_CustomersAddresses WITH (nolock) " +
                    "WHERE  identityId=c.identityId and addressType = 2) Addr  where C.customer_number= @customerNumber";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt1.Load(dr);

            }
            if (dt1.Rows[0]["passport_number"].ToString() != null)
            {
                details.Add("passport_number", dt1.Rows[0]["passport_number"].ToString());
                details.Add("passport_inf", dt1.Rows[0]["passport_inf"].ToString());
                details.Add("passport_date", dt1.Rows[0]["passport_date"].ToString());
            }
            else
            {
                ACBAServiceReference.CustomerDocument managerDocument = new ACBAServiceReference.CustomerDocument();
                ACBAServiceReference.CustomerOperationsClient client = new ACBAServiceReference.CustomerOperationsClient();
                managerDocument = client.GetManagerDocument(customerNumber);
                details.Add("passport_number", managerDocument.documentNumber.ToString());
                details.Add("passport_inf", managerDocument.givenBy.ToString());
                details.Add("passport_date", managerDocument.givenDate.ToString());
            }

            return details;
        }

    }
}
