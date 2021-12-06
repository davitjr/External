using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class ReplacedCardAccountRegOrderDB
    {
        /// <summary>
        ///  Փոխարինված քարտի հաշվի կցման հայտի տվյալների պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(ReplacedCardAccountRegOrder order, SourceType source, string userName)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_replace_document";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Card.Currency;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.Card.ProductId;
                    cmd.Parameters.Add("@cardAccount", SqlDbType.Float).Value = (order.Card.CardAccount.AccountNumber != null) ? double.Parse(order.Card.CardAccount.AccountNumber) : 0;
                    cmd.Parameters.Add("@overdraftAccount", SqlDbType.Float).Value = (order.Card.OverdraftAccount.AccountNumber != null) ? double.Parse(order.Card.OverdraftAccount.AccountNumber) : 0;
                    cmd.Parameters.Add("@addInf", SqlDbType.NVarChar, 50).Value = order.AddInf;

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

        internal static bool CheckRePlaceCardProductId(ulong productId, int filialCode)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT C.app_id 
                               FROM Tbl_VISA_applications V 
                                    INNER JOIN Tbl_CardChanges C 
                                    ON V.app_id =C.app_id 
                                    AND typeID = 3 
                                    WHERE CardStatus ='NORM' 
                                    AND V.app_id NOT IN (SELECT app_id FROM Tbl_Visa_Numbers_Accounts)  
                                    AND filial = @userFilial
                                    AND C.app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@userFilial", SqlDbType.Float).Value = filialCode;
                    conn.Open();
                    var temp = cmd.ExecuteScalar();
                    if (temp != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Վերադարձնում է փոխարինված քարտի հաշվի կցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static ReplacedCardAccountRegOrder GetReplacedCardAccountRegOrder(ReplacedCardAccountRegOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

               using SqlCommand cmd = new SqlCommand(@" SELECT D.app_id,
                                                          D.card_account,
                                                          D.overdraft_account,
                                                          D.add_inf,
                                                          H.customer_number,
                                                          H.document_number,
                                                          H.currency,
                                                          H.document_type,
                                                          H.quality,
                                                          H.operation_date,
                                                          H.registration_date
                                                  FROM Tbl_HB_documents H 
                                                  INNER JOIN tbl_card_replace_order_details D 
                                                  ON H.doc_ID = D.doc_ID  
                                                  WHERE H.doc_ID = @DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    ulong productId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                    order.Card = Card.GetCard(productId, order.CustomerNumber);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                    order.AddInf = dt.Rows[0]["add_inf"].ToString();

                    string accountNumber = dt.Rows[0]["card_account"].ToString();
                    order.CardAccount = Account.GetAccount(accountNumber, order.CustomerNumber);

                    accountNumber = dt.Rows[0]["overdraft_account"].ToString();
                    order.OverdraftAccount = Account.GetAccount(accountNumber, order.CustomerNumber);
                }
            }
            return order;
        }


        internal static bool CheckCardRelatedOffice(ReplacedCardAccountRegOrder order)
        {
            bool suitable = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"Select *  from Tbl_cards_rates where office_id= @relOfficeID  and cardid=@cardSystem and cardtype=@cardType and currency=@cardCurrency and isnull(Quality,1)= 1";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@cardSystem", SqlDbType.Int).Value = order.Card.CardSystem;
                cmd.Parameters.Add("@cardType", SqlDbType.BigInt).Value = order.Card.Type;
                cmd.Parameters.Add("@cardCurrency", SqlDbType.NVarChar, 3).Value = order.Card.Currency;
                cmd.Parameters.Add("@relOfficeID", SqlDbType.Int).Value = order.Card.RelatedOfficeNumber;

                using SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    suitable = true;
                }
            }
            return suitable;
        }

        internal static bool IfCardAccountExists(ReplacedCardAccountRegOrder order)
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
    }
}
