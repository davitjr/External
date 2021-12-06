using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class CardAccountRemovalOrderDB
    {
        internal static ActionResult Save(CardAccountRemovalOrder order, ACBAServiceReference.User user, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNewCardAccountRemovalOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = user.userName;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Card.Currency;
                    cmd.Parameters.Add("@cardType", SqlDbType.SmallInt).Value = order.Card.Type;
                    cmd.Parameters.Add("@cardSystem", SqlDbType.Int).Value = order.Card.CardSystem;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = order.Card.CardNumber;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = order.Card.ProductId;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.Card.CardAccount.AccountNumber;
                    cmd.Parameters.Add("@removalReason", SqlDbType.Int).Value = order.RemovalReason;
                    cmd.Parameters.Add("@removalInfo", SqlDbType.NVarChar).Value = (object)order.RemovalReasonInfo ?? DBNull.Value;

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
            }

            return result;
        }

        internal static bool CheckCardReissuement(long appID)
        {
            bool reissued = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT NULL
                                        FROM Tbl_Visa_Numbers_Accounts v 
                                        INNER JOIN Tbl_CardChanges C ON V.App_Id = C.app_id 
                                        INNER JOIN Tbl_Visa_Numbers_Accounts V2 ON V2.App_Id = C.old_app_id 
                                        WHERE V.App_Id =@appID  and V.visa_number <> V2.visa_number";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = appID;
                   

                   using SqlDataReader rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        reissued = true;
                    }
                }
            }
            return reissued;
        }

        internal static CardAccountRemovalOrder GetCardAccountRemovalOrder(CardAccountRemovalOrder cardAccountRemovalOrder)
        {
            cardAccountRemovalOrder.Card = new Card();
            cardAccountRemovalOrder.Card.OverdraftAccount = new Account();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sql = @"select  ROD.*, HB.registration_date, HB.document_number, HB.document_type, HB.quality, HB.operation_date, HB.operationFilialCode, HB.description,
                                ISNULL(CT.ApplicationsCardType,'') ApplicationsCardType,
                                RR.reason_description
                                from Tbl_HB_documents HB
                                INNER JOIN tbl_card_account_remove_order_details ROD on HB.doc_ID = ROD.doc_id
                                LEFT JOIN tbl_type_of_card CT on CT.id = ROD.card_type
                                LEFT JOIN tbl_card_remove_reasons RR on RR.id = ROD.reason_Id
                                WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = cardAccountRemovalOrder.CustomerNumber;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = cardAccountRemovalOrder.Id;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            cardAccountRemovalOrder.Card.Currency = dr["currency"].ToString();
                            cardAccountRemovalOrder.Card.CardNumber = dr["card_number"].ToString();
                            cardAccountRemovalOrder.Card.CardAccount.AccountNumber = dr["card_account"].ToString();                            
                            cardAccountRemovalOrder.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            cardAccountRemovalOrder.Type = (OrderType)dr["document_type"];
                            cardAccountRemovalOrder.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                            cardAccountRemovalOrder.OperationDate = Convert.ToDateTime(dr["operation_date"].ToString());
                            cardAccountRemovalOrder.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                            cardAccountRemovalOrder.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                            cardAccountRemovalOrder.Card.CardType = dr["ApplicationsCardType"].ToString();
                            cardAccountRemovalOrder.RemovalReasonDescription = dr["reason_description"].ToString();
                        }
                    }
                }
            }

            return cardAccountRemovalOrder;
        }
    }
}