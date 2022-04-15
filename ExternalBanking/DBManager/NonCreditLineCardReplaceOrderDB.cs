using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class NonCreditLineCardReplaceOrderDB
    {
        /// <summary>
        /// Առանց վարկային գծի քարտի փոխարինման հայտի տվյալների պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(NonCreditLineCardReplaceOrder order, SourceType source, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_non_credit_line_card_replace_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = user.userName;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@card_type", SqlDbType.Int).Value = order.Card.Type;
                    cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Card.Currency;
                    cmd.Parameters.Add("@organisation_name", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OrganisationNameEng) ? (object)order.OrganisationNameEng : DBNull.Value;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = order.Card.ProductId;
                    cmd.Parameters.Add("@card_account", SqlDbType.Float).Value = (order.Card.CardAccount != null) ? double.Parse(order.Card.CardAccount.AccountNumber) : 0;
                    cmd.Parameters.Add("@overdraft_account", SqlDbType.Float).Value = (order.Card.OverdraftAccount != null) ? double.Parse(order.Card.OverdraftAccount.AccountNumber) : 0;
                    cmd.Parameters.Add("@related_office_number", SqlDbType.Int).Value = order.RelatedOfficeNumber;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = order.InvolvingSetNumber;
                    cmd.Parameters.Add("@servicing_set_number", SqlDbType.Int).Value = order.ServingSetNumber;
                    cmd.Parameters.Add("@card_report_receiving_type", SqlDbType.Int).Value = order.CardReportReceivingType;
                    cmd.Parameters.Add("@report_receiving_email", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.ReportReceivingEmail) ? (object)order.ReportReceivingEmail : DBNull.Value;
                    cmd.Parameters.Add("@card_PIN_code_receiving_type", SqlDbType.Int).Value = order.CardPINCodeReceivingType;
                    cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(order.CardSMSPhone) ? "37400000000" : order.CardSMSPhone;
                    cmd.Parameters.Add("@cardReceivingType", SqlDbType.Int).Value = order.CardReceivingType;
                    cmd.Parameters.Add("@cardApplicationAcceptanceType", SqlDbType.Int).Value = order.CardApplicationAcceptanceType;

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
                    else if (actionResult == 8 || actionResult == 885)
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
        /// Վերադարձնում է Առանց վարկային գծի քարտի փոխարինման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static NonCreditLineCardReplaceOrder GetNonCreditLineCardReplaceOrder(NonCreditLineCardReplaceOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT D.app_id AS app_id,
                                                          H.customer_number,
                                                          H.document_number,
                                                          H.currency,
                                                          H.document_type,
                                                          H.document_subtype,
                                                          H.quality,
                                                          H.operation_date,
                                                          H.registration_date,
                                                          H.description ,
                                                          D.card_PIN_code_receiving_type,
                                                          D.phone
                                                    FROM Tbl_HB_documents H 
                                                         LEFT JOIN tbl_non_credit_Line_card_replace_order_details D 
                                                         ON H.doc_ID = D.doc_ID  
                                                         INNER JOIN tbl_visa_applications VA
                                                         ON VA.app_id = D.app_id 
                                                         WHERE H.doc_ID = @DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    ulong productId = dt.Rows[0]["app_id"] != DBNull.Value ? ulong.Parse(dt.Rows[0]["app_id"].ToString()) : ulong.Parse(dt.Rows[0]["appID"].ToString());
                    order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                    order.Card = Card.GetCard(productId, order.CustomerNumber);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardPINCodeReceivingType = dt.Rows[0]["card_PIN_code_receiving_type"] != DBNull.Value ? int.Parse(dt.Rows[0]["card_PIN_code_receiving_type"].ToString()) : 0;
                    order.PhoneNumber = dt.Rows[0]["phone"] != DBNull.Value ? dt.Rows[0]["phone"].ToString() : "";
                    order.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                }
            }
            return order;
        }

        internal static bool CheckCustomerDocument(ulong customerNumber)
        {
            bool hasDefaultDocument = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT 1  FROM  V_CustomerDesriptionDocs 
                                                        WHERE passport_number IS NOT NULL 
                                                        AND passport_inf IS NOT NULL 
                                                        AND passport_date IS NOT NULL 
                                                        AND customer_number = @customerNumber";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                using SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    hasDefaultDocument = true;
                }
            }
            return hasDefaultDocument;
        }

        internal static bool IfArcaContractIdExists(int cardTypeID, string currency)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT contractID 
                               FROM Tbl_Type_Of_ArcaContractID A 
                               INNER JOIN [Tbl_currency;] C 
                               ON A.currencyCode=C.currencyCodeN 
                               WHERE cardType = @cardTypeID AND currency= @currency";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardTypeID", SqlDbType.Int).Value = cardTypeID;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                    conn.Open();
                    var temp = cmd.ExecuteScalar();
                    return !(temp is null);
                }
            }
        }

        internal static bool IfCardAccountExists(NonCreditLineCardReplaceOrder order)
        {
            bool exist = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"Select *  from [tbl_all_accounts;] WHERE Arm_number = @cardAccount AND closing_date IS NULL";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@cardAccount", SqlDbType.NVarChar).Value = order.Card.CardAccount.AccountNumber;

                using SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    exist = true;
                }
            }
            return exist;
        }

        internal static ulong GetCardNewAppID(NonCreditLineCardReplaceOrder order)
        {
            ulong newAppID = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT app_id FROM Tbl_CardChanges   
                                        WHERE typeID = 3 AND old_app_id = @old_app_id";
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

        /// <summary>
        /// Քարտը փոխարինված է, բայց կարգավիճակը դեռ ՉՏՐ է
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static bool IsAlreadyReplacedButNotGiven(long productId)
        {
            bool isAlreadyReplacedButNotGiven = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT TOP 1 * FROM Tbl_CardChanges C
                                                   INNER JOIN 
                                                   Tbl_VISA_applications V
                                                   ON C.app_id = V.app_id
                                                   WHERE C.app_id = @app_id 
                                                   AND C.typeID = 3 AND V.GivenBy IS NULL AND V.GivenDate IS NULL";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyReplacedButNotGiven = true;
                    }
                }
            }
            return isAlreadyReplacedButNotGiven;
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
        /// Վերադարձնում է քարտապանի customerNumber-ը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static ulong GetCardHolderCustomerNumber(long productId)
        {
            ulong cardHolderCustomerNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT customer_number FROM Tbl_SupplementaryCards WHERE app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        cardHolderCustomerNumber = Convert.ToUInt64(temp);
                    }

                    return cardHolderCustomerNumber;
                }
            }
        }

        /// <summary>
        /// Քարտը փոխարինված է
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static bool IsAlreadyReplaced(long productId)
        {
            bool isAlreadyReplaced = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT TOP 1 * FROM Tbl_CardChanges 
                                                    WHERE old_app_id = @app_id 
                                                   AND typeID = 3 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyReplaced = true;
                    }
                }
            }
            return isAlreadyReplaced;
        }

        /// <summary>
        /// Քարտի համար գոյություն ունի մուտքագրված փախարինման հայտ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static bool IsAlreadyExistReplaceOrder(long productId)
        {
            bool isAlreadyExistReplaceOrder = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_hb_documents H
                                    INNER JOIN tbl_non_credit_line_card_replace_order_details D
                                    ON D.doc_id = H.doc_id
                                    WHERE quality  NOT IN  (5,6,30,31,32,40,41)  AND D.app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isAlreadyExistReplaceOrder = true;
                    }
                }
            }
            return isAlreadyExistReplaceOrder;
        }
    }
}
