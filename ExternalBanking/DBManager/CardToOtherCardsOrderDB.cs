using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class CardToOtherCardsOrderDB
    {
        internal static ActionResult Save(CardToOtherCardsOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_CardToOtherCardsOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    //cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = 1;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.TinyInt).Value = order.TransferType;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@sender_card_number", SqlDbType.NVarChar, 20).Value = order.SenderCardNumber;
                    cmd.Parameters.Add("@receiver_card_number", SqlDbType.NVarChar, 20).Value = order.ReceiverCardNumber;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 30).Value = order.ReceiverName;
                    cmd.Parameters.Add("@visa_alias", SqlDbType.NVarChar, 250).Value = order.VisaAlias;
                    cmd.Parameters.Add("@country_code", SqlDbType.NVarChar, 20).Value = order.CountryCode;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9)
                    {
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 10)
                    {
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
                return result;
            }
        }

        internal static CardToOtherCardsOrder Get(CardToOtherCardsOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT QH.change_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.quality,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
                                                         HB.amount,
                                                         HB.amount_for_payment,
                                                         HB.currency,
                                                         DT.*,
														 HB.order_group_id,
                                                         HB.confirmation_date
                                    FROM Tbl_HB_documents AS HB 
		                            INNER JOIN tbl_cardToOtherCards_order_details AS DT ON HB.doc_ID = DT.doc_id
                                    INNER JOIN tbl_hb_quality_history AS QH on HB.doc_ID = QH.Doc_ID and QH.quality = 1
                                    WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["change_date"]);
                    order.TransferType = (InternationalCardTransferTypes)Convert.ToInt32(dt.Rows[0]["transfer_type"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = Convert.ToDateTime(dt.Rows[0]["operation_date"]);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"].ToString());
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.SenderCardNumber = dt.Rows[0]["sender_card_number"].ToString();
                    order.ReceiverCardNumber = dt.Rows[0]["receiver_card_number"].ToString();
                    order.ReceiverName = dt.Rows[0]["receiver_name"].ToString();
                    order.VisaAlias = dt.Rows[0]["visa_alias"].ToString();
                    order.CountryCode = dt.Rows[0]["country_code"].ToString();
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.ArcaExtensionID = dt.Rows[0]["arca_ext_id"] != DBNull.Value ? Convert.ToUInt64(dt.Rows[0]["arca_ext_id"].ToString()) : 0;
                    order.RRN = dt.Rows[0]["rrn"].ToString();
                    order.AuthId = dt.Rows[0]["auth_id"].ToString();

                    order.Fees = Order.GetOrderFees(order.Id);
                }
            }
            return order;
        }

        public static List<ActionError> CheckLimits(string currency, int count, double amount, double dailyAmount)
        {
            List<ActionError> result = new List<ActionError>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT min_amount,max_amount,max_count,total_max_amount,currency_description
                                FROM tbl_c2oc_limits
                                WHERE currency=@currency";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var limit = new
                            {
                                MinAmount = Convert.ToDouble(dr["min_amount"]),
                                MaxAmount = Convert.ToDouble(dr["max_amount"]),
                                MaxCount = Convert.ToInt16(dr["max_count"]),
                                MaxTotalAmount = Convert.ToDouble(dr["total_max_amount"]),
                                CurrencyDescription = dr["currency_description"].ToString()
                            };
                            if (amount < limit.MinAmount)
                            {
                                result.Add(new ActionError(1723, new string[] { limit.MinAmount.ToString("0,0.00") + limit.CurrencyDescription }));
                            }
                            if (amount > limit.MaxAmount)
                            {
                                result.Add(new ActionError(1722, new string[] { limit.MaxAmount.ToString("0,0.00") + limit.CurrencyDescription }));
                            }
                            if (count >= limit.MaxCount)
                            {
                                result.Add(new ActionError(1725, new string[] { limit.MaxCount.ToString() }));
                            }
                            if (dailyAmount > limit.MaxTotalAmount)
                            {
                                result.Add(new ActionError(1724, new string[] { limit.MaxTotalAmount.ToString("0,0.00") + "ՀՀ դրամ" }));
                            }

                        }

                        return result;
                    }
                }
            }
        }

        public static bool IsNotArcaProcessingCard(string cardNumber)
        {
            bool result = false;
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM tbl_without_arca_bank_card_bins where card_bin=@cardBin", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

            }

            return result;
        }

        public static bool IsNotAllowedCardType(string cardNumber)
        {
            bool result = false;
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM tbl_type_of_card where cardBin=@cardBin AND not_allow_c2oc_transfer=1", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

            }

            return result;
        }

        public static bool IsBinUnderSanctions(string cardNumber)
        {
            bool result = false;
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT bin FROM [dbo].[Tbl_Countries]  C
                                                        inner join (select visa_bin bin,country_code from tbl_international_visa_card_bins
                                                                    UNION ALL select master_bin bin,country_code from tbl_international_master_card_bins) B ON C.CountryCodeA3=B.country_code
                                                        where under_sanction=1 and B.bin = @cardBin", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

            }

            return result;
        }
        public static bool IsBinUnderVisaSanctions(string cardNumber)
        {
            bool result = false;
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT visa_bin FROM [dbo].[tbl_visa_sanction_countries]  C
                                                        inner join tbl_international_visa_card_bins B ON C.country_code=B.country_code
                                                        where  visa_bin = @cardBin", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

            }

            return result;
        }

        public static bool IsBinUnderMasterSanctions(string cardNumber)
        {
            bool result = false;
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT visa_bin FROM [dbo].[tbl_master_sanction_countries]  C
                                                        inner join tbl_international_visa_card_bins B ON C.country_code=B.country_code
                                                        where  visa_bin = @cardBin", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

            }

            return result;
        }

        public static short GetRejectIdFromResponse(string responseCode)
        {
            short result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT reject_id from [tbl_arca_response_code_to_reject_id] WHERE response_code=@responseCode", conn);
                cmd.Parameters.Add("@responseCode", SqlDbType.NVarChar, 50).Value = responseCode;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result = Convert.ToInt16(dr["reject_id"].ToString());
                }

            }

            return result;
        }

        public static void SaveArcaResponseData(ulong orderId, ulong arcaExtID, ArcaDataServiceReference.CreditCardEcommerceResponse response)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE tbl_cardToOtherCards_order_details set arca_ext_id=@arcaExtId,response_code=@responseCode,processing_code=@processingCode,send_date=GETDATE(),rrn=@rrn,auth_id=@authId,is_checked=@isChecked WHERE doc_ID=@docId", conn))
                {
                    cmd.Parameters.Add("@responseCode", SqlDbType.NVarChar, 50).Value = response.ResponseCode;
                    cmd.Parameters.Add("@arcaExtId", SqlDbType.BigInt).Value = arcaExtID;
                    if (response.ProcessingCode == null)
                    {
                        cmd.Parameters.Add("@processingCode", SqlDbType.NVarChar, 20).Value = DBNull.Value;
                        cmd.Parameters.Add("@rrn", SqlDbType.NVarChar, 20).Value = DBNull.Value;
                        cmd.Parameters.Add("@authId", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                    }
                    else
                    {
                        cmd.Parameters.Add("@processingCode", SqlDbType.NVarChar, 20).Value = response.ProcessingCode;
                        cmd.Parameters.Add("@rrn", SqlDbType.NVarChar, 20).Value = response.RRN;
                        cmd.Parameters.Add("@authId", SqlDbType.NVarChar, 20).Value = response.AuthorizationIdResponse;
                    }

                    if (response.ResponseCode.Length == 4)
                    {
                        cmd.Parameters.Add("@isChecked", SqlDbType.Bit).Value = 0;
                    }
                    else
                    {
                        cmd.Parameters.Add("@isChecked", SqlDbType.Bit).Value = DBNull.Value;
                    }

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;
                    cmd.ExecuteNonQuery();
                }

            }

        }

        public static double GetCardToOtherCardFee(double amount, string currency, SourceType sourceType)
        {
            double fee = 0;
            double feeAmount = 0;
            double minFee = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select price,case when @currency='AMD' then min_level when @currency='USD' 
                                                        then price_for_group_1 when @currency='EUR' then price_for_group_2 end min_fee from tbl_prices  where idx_price=926 ", conn))
                {
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    using SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        fee = Convert.ToDouble(dr["price"].ToString());
                        minFee = Convert.ToDouble(dr["min_fee"].ToString());
                    }

                }

                feeAmount = Utility.RoundAmount(fee * amount, currency, sourceType);
                if (feeAmount < minFee)
                    feeAmount = minFee;

            }

            return feeAmount;
        }

        public static string GetCountryCodeByBin(string cardNumber)
        {
            string result = "";
            string cardBin = cardNumber.Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT visa_bin as bin,country_code  FROM [dbo].[tbl_international_visa_card_bins]
                                                          WHERE visa_bin=@cardBin
                                                         UNION 
                                                         SELECT master_bin ,country_code  FROM [dbo].[tbl_international_master_card_bins]
                                                          WHERE master_bin=@cardBin", conn))
                {
                    cmd.Parameters.Add("@cardBin", SqlDbType.NVarChar, 50).Value = cardBin;
                    using SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        result = dr["country_code"].ToString();
                    }
                    else
                    {
                        result = "ARM";
                    }
                }

            }
            return result;
        }
    }
}
