using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class CardRenewOrderDB
    {
        /// <summary>
        /// Քարտի վերաթողարկման հայտի տվյալների պահպանում
        /// </summary>
        internal static ActionResult Save(CardRenewOrder order, SourceType source, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_renew_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = user.userName;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Card.Currency;
                    if (order.Card.CreditLine != null)
                    {
                        cmd.Parameters.Add("@renew_with_credit_line_closing", SqlDbType.Bit).Value = order.WithCreditLineClosing;
                        cmd.Parameters.Add("@credit_line_app_id", SqlDbType.Float).Value = order.Card.CreditLine.ProductId;
                    }
                    cmd.Parameters.Add("@is_new_card_type", SqlDbType.Bit).Value = order.RenewWithCardNewType;
                    cmd.Parameters.Add("@organisation_name", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OrganisationNameEng) ? (object)order.OrganisationNameEng : DBNull.Value;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.Card.ProductId;
                    cmd.Parameters.Add("@related_office_number", SqlDbType.Int).Value = order.RelatedOfficeNumber;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = order.InvolvingSetNumber;
                    cmd.Parameters.Add("@servicing_set_number", SqlDbType.Int).Value = order.ServingSetNumber;
                    cmd.Parameters.Add("@card_PIN_code_receiving_type", SqlDbType.Int).Value = order.CardPINCodeReceivingType;
                    cmd.Parameters.Add("@delivery_address", SqlDbType.NVarChar).Value = order.DeliveryAddress;
                    cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(order.CardSMSPhone) ? "37400000000" : order.CardSMSPhone;
                    cmd.Parameters.Add("@cardReceivingType", SqlDbType.Int).Value = order.CardReceivingType;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 9)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                    }
                    else if (actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Քարտի վերաթողարկման հայտի տվյալները
        /// </summary>
        internal static CardRenewOrder GetCardRenewOrder(long Id)
        {
            CardRenewOrder order = new CardRenewOrder();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@" SELECT D.app_id AS app_id,
                                                          H.customer_number,
                                                          H.document_number,
                                                          H.currency,
                                                          H.document_type,
                                                          H.document_subtype,
                                                          H.quality,
                                                          H.operation_date,
                                                          H.registration_date,
                                                          H.description ,
                                                          D.renew_with_credit_line_closing,
                                                          D.is_new_card_type,
                                                          D.card_PIN_code_receiving_type,
                                                          D.phone,
                                                          D.credit_line_app_id,
														  VA.cardNumber,
														  S.CardSystemType + ' ' + Upper(TP.CardType) + ', ' + currency  AS CardType
                                                    FROM dbo.Tbl_HB_documents H 
                                                         INNER JOIN dbo.tbl_card_renew_order_details D 
                                                         ON H.doc_ID = D.doc_ID  
														 LEFT JOIN Tbl_cardchanges CH
														 ON CH.old_app_id = D.app_id
                                                         LEFT JOIN tbl_visa_applications VA
                                                         ON VA.app_id = CH.app_id
                                                         LEFT JOIN tbl_type_of_card TP
														 ON VA.cardtype = TP.id 
														 LEFT JOIN tbl_type_of_CardSystem S
														 ON TP.CardSystemId = S.Id 
                                                         WHERE H.doc_ID = @DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = Id;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    ulong productId = dt.Rows[0]["app_id"] != DBNull.Value ? ulong.Parse(dt.Rows[0]["app_id"].ToString()) : ulong.Parse(dt.Rows[0]["appID"].ToString());
                    order.Id = Id;
                    order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                    order.Card = Card.GetCard(productId, order.CustomerNumber);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.Quality = (OrderQuality)dt.Rows[0]["quality"];
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardPINCodeReceivingType = dt.Rows[0]["card_PIN_code_receiving_type"] != DBNull.Value ? int.Parse(dt.Rows[0]["card_PIN_code_receiving_type"].ToString()) : 0;
                    order.CreditLineProductId = dt.Rows[0]["credit_line_app_id"] != DBNull.Value ? long.Parse(dt.Rows[0]["credit_line_app_id"].ToString()) : 0;

                    if (dt.Rows[0]["renew_with_credit_line_closing"] != DBNull.Value)
                    {
                        order.WithCreditLineClosing = bool.Parse(dt.Rows[0]["renew_with_credit_line_closing"].ToString());
                    }

                    order.RenewWithCardNewType = bool.Parse(dt.Rows[0]["is_new_card_type"].ToString());
                    order.PhoneNumber = dt.Rows[0]["phone"] != DBNull.Value ? dt.Rows[0]["phone"].ToString() : "";
                    order.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                    order.CardNewNumber = dt.Rows[0]["cardNumber"] != DBNull.Value ? dt.Rows[0]["cardNumber"].ToString() : "";
                    order.CardNewTypeAndCurrency = dt.Rows[0]["CardType"] != DBNull.Value ? dt.Rows[0]["CardType"].ToString() : "";
                }
            }
            return order;
        }

        /// <summary>
        /// Քարտի գարգավիճակը NORM է թե ոչ
        /// </summary>
        internal static bool IsNormCardStatus(string cardNumber, long productId)
        {
            bool isNorm = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_visa_applications WHERE cardstatus='NORM' AND cardnumber= @cardNumber AND app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isNorm = true;
                    }
                }
            }
            return isNorm;
        }

        /// <summary>
        /// Քարտը վերաթողարկված է
        /// </summary>
        internal static bool IsAlreadyRenewed(long productId)
        {
            bool isAlreadyRenewed = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM Tbl_CardChanges C
                                             INNER JOIN tbl_visa_applications V
                                             ON C.app_id = V.app_id
                                              WHERE C.app_id = @app_id 
                                             AND typeID = 1
                                             AND GivenBy IS NULL AND GivenDate IS NULL";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyRenewed = true;
                    }
                }
            }
            return isAlreadyRenewed;
        }

        /// <summary>
        /// Քարտը չվերաթողարկված է
        /// </summary>
        internal static bool IsAlreadyNotRenewed(long productId)
        {
            bool isAlreadyNotRenewed = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM Tbl_VisaAppAdditions WHERE AdditionID = 11 AND app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyNotRenewed = true;
                    }
                }
            }
            return isAlreadyNotRenewed;
        }

        /// <summary>
        /// Քարտի համար գոյություն ունի մուտքագրված վերաթողարկման հայտ
        /// </summary>
        internal static bool IsAlreadyExistRenewOrder(long productId)
        {
            bool isAlreadyExistRenewOrder = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_hb_documents H
                                    INNER JOIN tbl_card_renew_order_details D
                                    ON D.doc_id = H.doc_id
                                    WHERE quality not in  (5,6,30,31,32,40,41) AND D.app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyExistRenewOrder = true;
                    }
                }
            }
            return isAlreadyExistRenewOrder;
        }


        internal static bool CheckCustomerDocument(ulong customerNumber, int docType)
        {
            //docType = 1 - հիմնական փաստափուղթ
            //docType = 2 - ՀԾՀ/ՀԾՀ-ից հրաժարման տեղեկանք
            bool hasDefaultDocument = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (docType == 1)
                    {
                        cmd.CommandText = @"SELECT 1  FROM  V_CustomerDesriptionDocs 
                                                        WHERE customer_number = @customerNumber AND 
                                                        (passport_number IS NOT NULL 
                                                        OR passport_inf IS NOT NULL 
                                                        OR passport_date IS NOT NULL)";
                    }
                    else if (docType == 2)
                    {
                        cmd.CommandText = @"SELECT TOP 1 document_number
                                            FROM[dbo].[V_Customer_Active_Documents]
                                            WHERE customer_number = @customerNumber
                                            AND document_type IN(56, 57)
                                            ORDER BY document_given_date DESC";
                    }
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        hasDefaultDocument = true;
                    }
                }
            }
            return hasDefaultDocument;
        }


        internal static ulong GetCardNewAppID(CardRenewOrder order)
        {
            ulong newAppID = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT app_id FROM Tbl_CardChanges   
                                        WHERE typeID = 1 AND old_app_id = @old_app_id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@old_app_id", SqlDbType.Float).Value = order.Card.ProductId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        newAppID = Convert.ToUInt64(temp);
                    }
                    return newAppID;
                }
            }
        }

        internal static string GetPhoneForCardRenew(ulong cardHolder)
        {
            string phone = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"  SELECT P.countryCode + P.areaCode + P.phoneNumber AS phone FROM tbl_customers C
                                        INNER JOIN Tbl_Customer_Phones CP
                                        ON CP.identityId = C.identityId
                                        INNER JOIN Tbl_Phones P
                                        ON P.id = CP.phoneId
                                        WHERE phoneType = 1 AND priority = 1
                                        AND customer_number = @cardHolder";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@cardHolder", SqlDbType.Float).Value = cardHolder;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        phone = Convert.ToString(temp);
                    }

                    return phone;
                }
            }
        }

        internal static int GetRelatedOfficeQuality(long productId, int officeID, bool withNewType)
        {
            int quality = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql;

                sql = @"SELECT Quality FROM Tbl_VISA_applications V 
                                       INNER JOIN  Tbl_cards_rates CR
                                       ON CR.CardType = ";
                if (withNewType)
                {
                    sql += "46 ";
                }
                else
                {
                    sql += @"CASE WHEN V.cardType = 17 THEN 36
                                  WHEN V.cardType = 19 THEN 37
                                  WHEN V.cardType = 25 THEN 38
                                  WHEN V.cardType = 20 THEN 41
                                  WHEN V.cardType = 23 THEN 40
                                  ELSE V.cardType END ";
                }
                sql += @"AND V.BillingCurrency = CR.currency
                         WHERE App_ID = @productId AND office_id = @officeId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@officeId", SqlDbType.Int).Value = officeID;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        quality = Convert.ToInt16(temp);
                    }
                }
            }
            return quality;
        }

        /// <summary>
        /// Նշված քարտի համար արդեն կատարվել է փոխարինման/վերաթողարկման գործողություն:
        /// </summary>
        internal static bool IsAlreadyRenewedOrReplaced(long productId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM Tbl_CardChanges C
                                             INNER JOIN tbl_visa_applications V
                                             ON C.old_app_id = V.app_id
                                              WHERE V.app_id = @app_id 
                                             AND typeID IN (1, 3)
                                             AND ReNew_Date IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    return temp != null;
                }
            }
        }
    }
}
