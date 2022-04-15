using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class CardReOpenOrderDB
    {
        /// <summary>
        /// Հաշվի բացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveCardReOpenOrder(CardReOpenOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_reopen_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductID;
                    cmd.Parameters.Add("@reopen_reason", SqlDbType.SmallInt).Value = order.ReopenReason;
                    if (order.ReopenDescription != null)
                    {
                        cmd.Parameters.Add("@reopen_description", SqlDbType.NVarChar, 4000).Value = order.ReopenDescription;
                    }

                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 16).Value = order.CardNumber;
                    cmd.Parameters.Add("@card_type", SqlDbType.SmallInt).Value = order.CardType;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = order.SetNumber;


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
        internal static CardReOpenOrder GetCardReOpenOrder(CardReOpenOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"  SELECT  H.doc_ID, H.currency,H.document_number,H.document_type,H.quality,H.operation_date,H.registration_date,H.customer_number,R.app_id,
                        R.card_number, C.ApplicationsCardType,T.reason,R.reopen_description,R.set_number,description
                        FROM Tbl_HB_documents H
                        INNER JOIN tbl_card_reopen_order_details R ON H.doc_ID = R.doc_ID 
                        INNER JOIN Tbl_type_of_Card C  ON C.ID = R.card_type 
						INNER JOIN tbl_type_of_card_reopen_reason T on T.ID=R.reopen_reason 
                        WHERE H.doc_ID = @DocID", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    dt.Load(cmd.ExecuteReader());

                    order.Id = Convert.ToInt64(dt.Rows[0]["doc_id"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardNumber = dt.Rows[0]["card_number"].ToString();
                    order.CardTypeDescription = dt.Rows[0]["ApplicationsCardType"].ToString();
                    order.ReopenReasonString = dt.Rows[0]["reason"].ToString();
                    order.ReopenDescription = dt.Rows[0]["reopen_description"].ToString();
                    order.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["customer_number"]);
                    order.ProductID = Convert.ToUInt64(dt.Rows[0]["app_id"]);
                    order.SetNumber = Convert.ToUInt16(dt.Rows[0]["set_number"]);
                    order.OrderName = dt.Rows[0]["description"].ToString();
                }
            }
            return order;
        }



        /// <summary>
        /// ստուգում ենք բաց ընթացիկ դրամային հաշիվ կա թե չէ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool IsOpenAMDCurrentAccout(ulong customerNumber)
        {
            bool isOpenAMDCurrentAccout = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"If not exists(	select * from [tbl_all_accounts;]	a 
	                                                	    join (select distinct sint_acc_new from Tbl_define_sint_acc where  type_of_product=10 and type_of_account=24) s
										                    on a.type_of_account_new = s.sint_acc_new
		                                                    where customer_number=@customerNumber   and  closing_date is null and currency='AMD')	select 0", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (cmd.ExecuteReader().Read())
                    {
                        isOpenAMDCurrentAccout = false;
                    }
                }
            }
            return isOpenAMDCurrentAccout;
        }


        /// <summary>
        /// ստուգում ենք քարտը բաց է թե ոչ
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <returns></returns>
        internal static bool IsCardOpen(string CardNumber)
        {
            bool isCardOpen = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"IF not EXISTS 	(SELECT filialcode	FROM Tbl_visa_numbers_accounts
			                                        	WHERE visa_number = @CardNumber	AND closing_date IS  NULL)	select 0", conn))
                {
                    cmd.Parameters.Add("@CardNumber", SqlDbType.NVarChar, 16).Value = CardNumber;

                    if (cmd.ExecuteReader().Read())
                    {
                        isCardOpen = false;
                    }
                }
            }
            return isCardOpen;
        }



        /// <summary>
        /// Վերադարցնում է վերաթողարկման պատճառների ցուցակը
        /// </summary>
        /// <returns></returns>

        public static DataTable GetCardReOpenReason()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = @"select id,reason from [tbl_type_of_card_reopen_reason] ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                }
            }
            return dt;
        }



        /// <summary>
        /// Ստուգում է տվյալ քարտի համար  app_id-ի առկայությունը Tbl_closed_credit_lines-ում
        /// </summary>
        /// <returns></returns>

        public static bool IsExistsOverdraftAppId(string cardNumber)
        {
            bool isExistsOverdraftAppId = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = @" SELECT top 1  app_id FROM Tbl_closed_credit_lines
                                                    WHERE	visa_number = @cardNumber
	                                                AND quality = 40 AND loan_type = 9  
                                                    order by [last_day_of rate calculation] desc";


                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        isExistsOverdraftAppId = row.ItemArray[0] != DBNull.Value;
                    }
                    else
                        isExistsOverdraftAppId = false;

                }

            }
            return isExistsOverdraftAppId;
        }



        /// <summary>
        /// վերադարձնում  է տվյալ քարտի փոխարինված ,վերաթողարկված ,տեղափոխված լիելու ստատուսը
        /// </summary>
        /// <returns></returns>

        public static ushort GetTypeOfCardChanges(ulong ippId)
        {
            ushort typeId = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = @" SELECT [typeID]     FROM [Tbl_CardChanges]
                                    where old_app_id=@ippId";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ippId", SqlDbType.Float).Value = ippId;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        typeId = Convert.ToUInt16(row.ItemArray[0]);
                    }
                }

            }
            return typeId;
        }

    }
}
