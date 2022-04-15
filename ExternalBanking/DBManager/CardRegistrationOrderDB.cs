using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class CardRegistrationOrderDB
    {
        /// <summary>
        /// Հաշվի բացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(CardRegistrationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_registration_document";
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
                    cmd.Parameters.Add("@cardAccount", SqlDbType.Float).Value = (order.CardAccount != null) ? double.Parse(order.CardAccount.AccountNumber) : 0;
                    cmd.Parameters.Add("@overdraftAccount", SqlDbType.Float).Value = (order.OverdraftAccount != null) ? double.Parse(order.OverdraftAccount.AccountNumber) : 0;
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

        /// <summary>
        /// Վերադարձնում է հաշվի բացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static CardRegistrationOrder GetCardRegistrationOrder(CardRegistrationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @" SELECT D.app_id,D.card_account,D.overdraft_account,D.add_inf,H.customer_number,H.document_number,
                                                  H.currency,H.document_type,H.quality,H.operation_date,H.registration_date
                                                  FROM Tbl_HB_documents H INNER JOIN tbl_card_registration_order_details D ON H.doc_ID = D.doc_ID  WHERE H.doc_ID = @DocID", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    dt.Load(cmd.ExecuteReader());
                    ulong productId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                    order.Card = PlasticCard.GetPlasticCard(productId, true);
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

        internal static bool IsAccountUseForAnotherCard(string accountNumber, int accountType, string mainCardNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string str = "";

                if (accountType == 1)
                    if (mainCardNumber == "")
                        str = @"SELECT 1 FROM tbl_visa_numbers_accounts WHERE card_account = @accountNumber AND closing_date IS NULL ";
                    else
                        str = @"SELECT 1 FROM tbl_visa_numbers_accounts WHERE card_account = @accountNumber AND closing_date IS NULL AND main_card_number <>  @mainCardNumber";
                else if (accountType == 2)
                    if (mainCardNumber == "")
                        str = @"SELECT 1 FROM tbl_visa_numbers_accounts WHERE overdraft_account = @accountNumber AND closing_date IS NULL ";
                    else
                        str = @"SELECT 1 FROM tbl_visa_numbers_accounts WHERE overdraft_account = @accountNumber AND closing_date IS NULL AND main_card_number <>  @mainCardNumber";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar, 20).Value = mainCardNumber;

                    if (cmd.ExecuteReader().Read())
                        return false;
                }

            }
            return true;
        }

        /// <summary>
        /// Երբ մուտքագրվում է հայտ՝ երկրորդ անգամ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static bool IsSecondRegistration(ulong productId)
        {
            bool secondRegistration = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT  d.doc_ID                                            
                                                  FROM Tbl_HB_documents AS D LEFT JOIN tbl_card_registration_order_details AS C on  D.doc_ID=C.Doc_ID
                                                  WHERE C.app_id=@appID AND D.quality IN(1,2,3,5,100)", conn))
                {
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;

                    if (cmd.ExecuteReader().Read())
                    {
                        secondRegistration = true;
                    }
                }
            }
            return secondRegistration;
        }
    }
}
