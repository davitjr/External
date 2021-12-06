using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class PlasticCardRemovalOrderDB
    {
        internal static ActionResult Save(PlasticCardRemovalOrder order, ACBAServiceReference.User user, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNewPlasticCardRemovalOrder";
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
                    cmd.Parameters.Add("@cardType", SqlDbType.SmallInt).Value = int.TryParse(order.Card.CardType, out var cardtype) ? cardtype : order.Card.Type;
                    cmd.Parameters.Add("@cardSystem", SqlDbType.Int).Value = order.Card.CardSystem;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = order.Card.CardNumber;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = order.Card.ProductId;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.VarChar, 50).Value = (object)order.Card.CardAccount.AccountNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@overdraftAccountNumber", SqlDbType.VarChar, 50).Value = order.Card.OverdraftAccount != null ? (object)order.Card.OverdraftAccount.AccountNumber : DBNull.Value;
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
        internal static PlasticCardSentToArcaStatus PlasticCardSentToArca(long productId)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = @"SELECT ISNULL(fileID, 0) fileID
                                        FROM  Tbl_ArcaRequestHeaders                                        
                                        WHERE appId = @appId and commandType in (1, 2, 4)";
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;

            using SqlDataReader rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                return int.Parse(rd["fileID"].ToString()) == 0 ? PlasticCardSentToArcaStatus.NoFiles : PlasticCardSentToArcaStatus.SentToArca;
            }
            else return PlasticCardSentToArcaStatus.NoInfo;


        }
        internal static bool CheckForArcaFileGenerationProcess()
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = @"select Lock_for_reading
                                        from Tbl_locking_for_reading
                                        where [File_ID] = 26";

            cmd.CommandType = CommandType.Text;

            using SqlDataReader rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                return bool.Parse(rd["Lock_for_reading"].ToString());
            }
            else
            {
                return false;
            }
        }

        internal static PlasticCardRemovalOrder GetPlasticCardRemovalOrder(PlasticCardRemovalOrder plasticCardRemovalOrder)
        {
            plasticCardRemovalOrder.Card = new Card();
            plasticCardRemovalOrder.Card.OverdraftAccount = new Account();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sql = @"select  ROD.*, HB.registration_date, HB.document_number, HB.document_type, HB.quality, HB.operation_date, HB.operationFilialCode, HB.description,
                                ISNULL(CT.ApplicationsCardType,'') ApplicationsCardType,
                                RR.reason_description
                                from Tbl_HB_documents HB
                                INNER JOIN tbl_card_remove_order_details ROD on HB.doc_ID = ROD.doc_id
                                LEFT JOIN dbo.tbl_type_of_card CT on CT.id = ROD.card_type
                                LEFT JOIN tbl_card_remove_reasons RR on RR.id = ROD.reason_Id
                                WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = plasticCardRemovalOrder.CustomerNumber;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = plasticCardRemovalOrder.Id;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            plasticCardRemovalOrder.Card.Currency = dr["currency"].ToString();
                            plasticCardRemovalOrder.Card.CardNumber = dr["card_number"].ToString();
                            plasticCardRemovalOrder.Card.CardAccount.AccountNumber = dr["card_account"].ToString();
                            plasticCardRemovalOrder.Card.OverdraftAccount.AccountNumber = dr["overdraft_account"].ToString();
                            plasticCardRemovalOrder.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            plasticCardRemovalOrder.Type = (OrderType)dr["document_type"];
                            plasticCardRemovalOrder.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                            plasticCardRemovalOrder.OperationDate = Convert.ToDateTime(dr["operation_date"].ToString());
                            plasticCardRemovalOrder.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                            plasticCardRemovalOrder.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                            plasticCardRemovalOrder.Card.CardType = dr["ApplicationsCardType"].ToString();
                            plasticCardRemovalOrder.RemovalReasonDescription = dr["reason_description"].ToString();
                        }
                    }
                }
            }

            return plasticCardRemovalOrder;
        }

    }
}
