
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using ExternalBanking.ACBAServiceReference;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace ExternalBanking.DBManager
{
    internal class OrderDB
    {
        internal static List<Order> GetDraftOrders(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.document_number,
                                                d.registration_date,
                                                d.description,
                                                d.receiver_name,
                                                d.amount,
                                                d.currency,
                                                d.quality,
                                                d.operationFilialCode,d.operation_date 
                                    FROM Tbl_HB_documents d WHERE customer_number=@customerNumber and registration_date>=@dateFrom and registration_date<=@dateTo and quality=1 and source_type<>2 order by registration_date desc,doc_id desc";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);

                        orders.Add(order);
                    }

                }
            }
            return orders;
        }


        internal static List<Order> GetSentOrders(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.document_number,
                                                d.registration_date,
                                                d.description,
                                                d.receiver_name,
                                                d.amount,
                                                d.currency,
                                                d.quality,
                                                d.operationFilialCode,d.operation_date 
                                FROM Tbl_HB_documents d WHERE customer_number=@customerNumber and registration_date>=@dateFrom and registration_date<=@dateTo and quality<>1 and quality<>40 and source_type<>2 order by registration_date desc,doc_id desc";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);

                        orders.Add(order);
                    }

                }
            }
            return orders;
        }

        internal static List<Order> GetOrders(SearchOrders searchParams)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                d.customer_number,
                                d.document_type,
                                d.document_subtype,
                                d.document_number,
                                d.registration_date,
                                d.description,
                                d.receiver_name,
                                d.amount,
                                d.currency,
                                d.quality,
                                d.operationFilialCode,
                                d.operation_date
                            FROM Tbl_HB_documents d LEFT JOIN  Tbl_HB_quality_history q ON d.doc_ID = q.Doc_ID AND q.quality = 1 ";

                if (searchParams.Type == OrderType.ArcaCardsTransactionOrder)
                {
                    sql += "INNER JOIN tbl_arca_cards_transaction_order_details ON d.doc_id = tbl_arca_cards_transaction_order_details.doc_id ";
                }
                if (searchParams.Type == OrderType.CardLimitChangeOrder)
                {
                    sql += "INNER JOIN tbl_card_limit_change_order_details ON d.doc_id = tbl_card_limit_change_order_details.doc_id ";
                }
                if (searchParams.Type == OrderType.PlasticCardOrder || searchParams.Type == OrderType.AttachedPlasticCardOrder || searchParams.Type == OrderType.LinkedPlasticCardOrder)
                {
                    sql += @"INNER JOIN tbl_card_order_details CD ON CD.doc_ID = D.doc_ID
                             LEFT JOIN dbo.Tbl_Visa_Applications A ON A.app_id = CD.app_id ";
                }
                if (searchParams.Type == OrderType.NonCreditLineCardReplaceOrder)
                {
                    sql += @"INNER JOIN tbl_non_credit_Line_card_replace_order_details NCD ON NCD.doc_ID = D.doc_ID
                             LEFT JOIN dbo.Tbl_Visa_Applications A ON A.app_id = NCD.app_id ";
                }
                if (searchParams.Type == OrderType.CreditLineCardReplaceOrder)
                {
                    sql += @"INNER JOIN tbl_credit_Line_card_replace_order_details CRD ON CRD.doc_ID = D.doc_ID
                             LEFT JOIN dbo.Tbl_Visa_Applications A ON A.app_id = CRD.app_id ";
                }
                if (searchParams.Type == OrderType.PINRegenerationOrder)
                {
                    sql += @"INNER JOIN tbl_PIN_regeneration_order_details PR ON PR.doc_ID = D.doc_ID
                             LEFT JOIN dbo.Tbl_Visa_Applications A ON A.app_id = PR.app_id ";
                }
                if (searchParams.Type == OrderType.CardRenewOrder)
                {
                    sql += @"INNER JOIN tbl_card_renew_order_details CR ON CR.doc_ID = D.doc_ID
                             LEFT JOIN dbo.Tbl_Visa_Applications A ON A.app_id = CR.app_id ";
                }

                sql += " WHERE d.quality <> 1 and d.quality <> 40 and document_type not in (135, 137, 138, 151, 157, 158) ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (searchParams.OperationFilialCode != 0)
                    {
                        cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = searchParams.OperationFilialCode;
                        sql += " AND operationFilialCode = @operationFilialCode ";
                    }

                    if (searchParams.CustomerNumber != 0)
                    {
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = searchParams.CustomerNumber;
                        sql += " AND d.customer_number = @customerNumber ";
                    }
                    else
                    {   //Եթե կատարվում է որոնում <Ոչ հաճախորդի սպասարկում> բաժնից, ապա միայն բանկում կատարված գործարքներ
                        sql += " and source_type in (2,6,8) ";
                    }

                    if (searchParams.Amount != 0)
                    {
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = searchParams.Amount;
                        sql = sql + " and d.amount=@amount";
                    }

                    if (searchParams.IsATSAccountOrders)
                    {

                        List<Account> atsAccounts = Account.GetATSSystemAccounts((uint)searchParams.OperationFilialCode);

                        string debitAccounts = "";

                        foreach (Account account in atsAccounts)
                        {
                            debitAccounts += account.AccountNumber + ",";
                        }
                        debitAccounts = debitAccounts.Substring(0, debitAccounts.Length - 1);
                        sql = sql + " and d.debet_account in(" + debitAccounts + ") ";
                    }

                    if (searchParams.IsCashBookOrder)
                    {
                        sql = sql + " and document_type in(139,140,144,146,147,149,150,167,200)";
                    }

                    else if (searchParams.IsFondOrder)
                    {
                        sql = sql + " and document_type in(190,192,205,221)";
                    }
                    else
                    {
                        sql = sql + " and document_type not in(139,140,144,146,147,149,150,167,200,190,192,205)";
                    }
                    if (searchParams.Id != 0)
                    {
                        cmd.Parameters.Add("@docId", SqlDbType.Int).Value = searchParams.Id;
                        sql = sql + " and d.doc_id=@docId";
                    }
                    else
                    {
                        if (searchParams.DateFrom != null)
                        {
                            cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = searchParams.DateFrom;
                            sql = sql + " and registration_date>=@dateFrom";
                        }

                        if (searchParams.DateTo != null)
                        {
                            cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = searchParams.DateTo;

                            sql = sql + " and registration_date<=@dateTo";
                        }

                        if (searchParams.OrderQuality != OrderQuality.NotDefined)
                        {
                            cmd.Parameters.Add("@quality", SqlDbType.Int).Value = searchParams.OrderQuality;
                            sql = sql + " and d.quality=@quality";
                        }

                        if (searchParams.Source != SourceType.NotSpecified)
                        {
                            cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = searchParams.Source;
                            sql = sql + " and source_type=@sourceType";
                        }


                        if (searchParams.Type != OrderType.NotDefined)
                        {
                            cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = searchParams.Type;
                            sql = sql + " and document_type=@documentType";
                        }

                        if (searchParams.RegisteredUserID != 0)
                        {
                            cmd.Parameters.AddWithValue("@userID", searchParams.RegisteredUserID);
                            sql += " AND change_set_number = @userID";
                        }
                    }
                    if (searchParams.Type == OrderType.ArcaCardsTransactionOrder && searchParams.CardNumber != null)
                    {
                        cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = searchParams.CardNumber;
                        sql += " AND tbl_arca_cards_transaction_order_details.card_number = @cardNumber ";
                    }
                    if (searchParams.Type == OrderType.CardLimitChangeOrder && searchParams.CardNumber != null)
                    {
                        cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = searchParams.CardNumber;
                        sql += " AND tbl_card_limit_change_order_details.card_number = @cardNumber ";
                    }
                    if ((searchParams.Type == OrderType.PlasticCardOrder || searchParams.Type == OrderType.AttachedPlasticCardOrder || searchParams.Type == OrderType.LinkedPlasticCardOrder ||
                        searchParams.Type == OrderType.PINRegenerationOrder || searchParams.Type == OrderType.NonCreditLineCardReplaceOrder || searchParams.Type == OrderType.CreditLineCardReplaceOrder ||
                        searchParams.Type == OrderType.CardRenewOrder || searchParams.Type == OrderType.RenewedCardAccountRegOrder)
                        && searchParams.CardNumber != null)
                    {
                        cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = searchParams.CardNumber;
                        sql += " AND A.CardNumber = @cardNumber ";
                    }
                    if (searchParams.CardRenewType == "WithNewType")
                    {
                        sql += @" AND is_new_card_type = 1 ";
                    }
                    else if (searchParams.CardRenewType == "WithSameType")
                    {
                        sql += @" AND is_new_card_type = 0 ";
                    }

                    sql += " order by registration_date desc,d.doc_id desc";


                    cmd.CommandText = sql;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);
                        orders.Add(order);
                    }

                }
            }
            return orders;
        }

        private static Order SetOrder(DataRow row, bool withDebetAccount = false)
        {
            Order order = new Order() { DebitAccount = new Account()};

            if (row != null)
            {
                order.Id = int.Parse(row["doc_id"].ToString());
                order.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                order.Type = (OrderType)short.Parse(row["document_type"].ToString());
                order.SubType = byte.Parse(row["document_subtype"].ToString());
                order.OrderNumber = row["document_number"].ToString();
                order.RegistrationDate = DateTime.Parse(row["registration_date"].ToString());
                order.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString()) + " " + Utility.ConvertAnsiToUnicode(row["receiver_name"].ToString());
                order.Amount = double.Parse(row["amount"].ToString());
                order.Currency = row["currency"].ToString();
                order.Quality = (OrderQuality)short.Parse(row["quality"].ToString());
                order.Source = (SourceType)int.Parse(row["source"].ToString());
                order.FilialCode = ushort.Parse(row["operationFilialCode"].ToString());

                order.OperationDate = row["operation_date"] != DBNull.Value ? Convert.ToDateTime(row["operation_date"]) : default(DateTime?);

                if (withDebetAccount)
                {
                    string debetAccountNumber = "";
                    debetAccountNumber = row["debet_account"] != DBNull.Value ? row["debet_account"].ToString() : "";
                    if (!(String.IsNullOrEmpty(debetAccountNumber) || debetAccountNumber == "0"))
                    {
                        order.DebitAccount = Account.GetAccount(debetAccountNumber);
                    }
                }

                if (row.Table.Columns.Contains("confirmation_date"))
                {
                    order.ConfirmationDate = row["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(row["confirmation_date"]) : default(DateTime?);
                }

                if (order.Type == OrderType.CommunalPayment)
                {
                    order.Amount += UtilityPaymentOrderDB.GetGasServiceFeeAmount(order.Id) ?? 0;
                }
            }

            return order;
        }

        internal static double GetSentOrdersAmount(string accountNumber)
        {
            double amount = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.[fnc_Get_Sent_transactions_amount](@accountNumber) as future_amount", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        amount = double.Parse(dr["future_amount"].ToString());
                    }
                }
            }

            return amount;
        }


        internal static double GetDayOrdersAmount(ulong customerNumber, long orderId, DateTime registrationDate, SourceType source = 0, short isToOwnAccount = 0)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"GetDayTransactionsSum", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    string customerNumberCond = " in(" + customerNumber.ToString() + ")";

                    cmd.Parameters.Add("@customer_Number", SqlDbType.VarChar, 50).Value = customerNumberCond;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = String.Format("{0:dd/MMM/yy}", registrationDate);

                    cmd.Parameters.Add(new SqlParameter("@sum", SqlDbType.Float) { Direction = ParameterDirection.Output });

                    if (source != 0)
                    {
                        cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = source;
                    }

                    if (isToOwnAccount != 0)
                    {
                        cmd.Parameters.Add("@isToOwnAccount", SqlDbType.SmallInt).Value = isToOwnAccount;
                    }


                    cmd.ExecuteNonQuery();


                    amount = Convert.ToDouble(cmd.Parameters["@sum"].Value);
                }
            }

            return amount;
        }
        public static OrderQuality GetNextQuality(short schemaType, long id)
        {
            short result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT dbo.fnc_Get_Next_Quality(@approveType,@Doc_id) as nextQuality";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@approveType", SqlDbType.TinyInt).Value = schemaType;
                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = id;
                    result = Convert.ToInt16(cmd.ExecuteScalar());
                }
            }
            return (OrderQuality)result;
        }

        public static void ChangeQuality(long id, OrderQuality quality, string userName)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Update_Quality";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = (short)quality;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = userName;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void ConfirmOrder(Order order, ACBAServiceReference.User user)
        {
            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                if ((order.Source != SourceType.Bank && order.Source != SourceType.SSTerminal && order.Source != SourceType.EContract && order.Source != SourceType.STAK
                    && order.Source != SourceType.SberBankTransfer) && order.Source != SourceType.CashInTerminal
                    && order.Type != OrderType.ReceivedFastTransferPaymentOrder && order.Type != OrderType.CredentialOrder && order.Type != OrderType.PlasticCardOrder
                     && order.Type != OrderType.HBApplicationUpdateOrder && order.Type != OrderType.HBApplicationOrder && order.Type != OrderType.HBApplicationRestoreOrder
                     && order.Type != OrderType.HBActivation && order.Type != OrderType.HBApplicationTerminationOrder
                     && order.Type != OrderType.ArcaCardsTransactionOrder && order.Type != OrderType.CardLimitChangeOrder && order.Type != OrderType.CardRegistrationOrder
                     && order.Type != OrderType.SberBankTransferOrder && order.Type != OrderType.BillSplitReminder && order.Type != OrderType.BillSplitSenderRejection)
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_HB_manual_confirmation";
                        cmd.CommandTimeout = 220;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                        cmd.ExecuteNonQuery();
                    }

                }
                else
                {

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_FRONT_OFFICE_order_confirm";
                        cmd.CommandTimeout = 120;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }


        public static string GenerateNextOrderNumber(ulong customerNumber)
        {
            string orderNumber = "0";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT isnull(max (cast(document_number as bigint)),0) + 1 as NextDocumentNumber FROM Tbl_HB_documents  
                                                                 WHERE customer_number =@customerNumber
                                                                 and isnumeric(document_number) = 1 
                                                                 and not document_number like '%.%' 
                                                                 and not  document_number like '%,%' 
                                                                 and  registration_date =  (CONVERT (VARCHAR(10),GETDATE(), 101))  
                                                                 and document_type not in (4, 16, 21,25,130,131) ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            orderNumber = dr["NextDocumentNumber"].ToString();
                        }
                    }

                }
            }

            return orderNumber;
        }
        /// <summary>
        /// Ստուգում է Doc_Id-ի և customerNumber-ի համատեղելիությունը
        /// </summary>
        /// <param name="ID">Doc_ID</param>
        /// <param name="customerNumber">customerNumber</param>
        /// <returns></returns>
        internal static bool CheckDocumentID(long ID, ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select doc_ID,customer_number from Tbl_HB_documents where doc_ID=@DocID and customer_number=@customernumber", conn))
                {
                    cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = ID;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            check = true;
                        }
                    }
                }
            }
            return check;
        }

        internal static Order GetOrder(long orderId, ulong customerNumber)
        {
            Order order = new Order();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                        d.customer_number,
                                        d.document_type,
                                        d.document_subtype,
                                        d.document_number,
                                        d.registration_date,
                                        d.description,
                                        d.receiver_name,
                                        d.amount,
                                        d.currency,
                                        d.quality,
                                        d.operationFilialCode,d.operation_date, d.confirmation_date 
                            FROM Tbl_HB_documents d WHERE customer_number=@customerNumber and doc_id=@orderId ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        order = SetOrder(row);
                    }
                }
            }

            return order;
        }
        /// <summary>
        /// Վերադարձնում է տվյալ հայտի պատմությունը
        /// </summary>
        /// <param name="orderId">Հայտի ունիկալ համար</param>
        /// <returns></returns>
        internal static List<OrderHistory> GetOrderHistory(long orderId)
        {
            List<OrderHistory> historys = new List<OrderHistory>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT h.Doc_ID
                                                      ,h.quality
                                                      ,h.change_date
                                                      ,h.change_set_number
                                                      ,h.change_user_name,d.reject_ID FROM Tbl_HB_quality_history AS h INNER JOIN Tbl_HB_documents AS d ON h.Doc_ID=d.doc_ID WHERE h.Doc_ID=@DocID order by h.uniq_number ", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = orderId;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];
                    OrderHistory history = SetOrderHistory(row);
                    historys.Add(history);
                }

            }
            return historys;
        }




        private static OrderHistory SetOrderHistory(DataRow row)
        {
            OrderHistory history = new OrderHistory();

            if (row != null)
            {
                history.Id = int.Parse(row["Doc_id"].ToString());
                history.ChangeDate = Convert.ToDateTime(row["change_date"].ToString());
                history.Quality = (OrderQuality)short.Parse(row["quality"].ToString());

                if (row["change_set_number"] != DBNull.Value && row["change_set_number"].ToString() != "0" && !String.IsNullOrEmpty(row["change_set_number"].ToString()))
                {
                    history.ChangeUserId = long.Parse(row["change_set_number"].ToString());
                    history.ChangeUserName = Utility.ConvertAnsiToUnicode(Utility.GetUserFullName(history.ChangeUserId));
                }
                if (row["change_user_name"] != DBNull.Value && !String.IsNullOrEmpty(row["change_user_name"].ToString()))
                {
                    history.ChangeUserName = row["change_user_name"].ToString();
                }

                if (!String.IsNullOrEmpty(row["reject_ID"].ToString()) && history.Quality == OrderQuality.Declined)
                {
                    history.ReasonId = ushort.Parse(row["reject_ID"].ToString());
                }

            }

            return history;
        }

        internal static ulong Save(Order order, short userId, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {


                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_Order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    int communalType = 0;

                    if (order.Type == OrderType.CommunalPayment || order.Type == OrderType.CashCommunalPayment ||
                        order.Type == OrderType.ReestrCommunalPayment || order.Type == OrderType.ReestrCashCommunalPayment)
                    {
                        UtilityPaymentOrder uOrder = (UtilityPaymentOrder)order;
                        communalType = (int)uOrder.CommunalType;
                    }

                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = GetBOOrderType(order.Type, order.SubType, communalType);


                    cmd.Parameters.Add("@orderDate", SqlDbType.DateTime).Value = (object)order.OperationDate ?? DBNull.Value;
                    cmd.Parameters.Add("@orderNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@mainOrderId", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@orderFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@action", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@actionSetNumber", SqlDbType.SmallInt).Value = userId;


                    SqlParameter param = new SqlParameter("@Id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = uint.Parse(param.Value.ToString());
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return ulong.Parse(param.Value.ToString());
                }
            }
        }

        internal static ActionResult SaveLinkHBDocumentOrder(long documentId, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_link_HB_document_order(document_id, order_id) VALUES(@documentid, @orderId)";
                    //cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@documentId", SqlDbType.Float).Value = documentId;
                    cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderId;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        public static void SaveOrderAttachments(Order order)
        {
            List<OrderAttachment> ExistingAttachments = Order.GetOrderAttachments(order.Id);

            order.Attachments.ForEach(m =>
            {
                if (String.IsNullOrEmpty(m.Id))
                {
                    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            conn.Open();
                            cmd.Connection = conn;
                            cmd.CommandText = "sp_SaveUploadedFile";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@SystemNumber", SqlDbType.Int).Value = 1;
                            cmd.Parameters.Add("@FileType", SqlDbType.NVarChar, 5).Value = m.FileExtension;
                            cmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 50).Value = m.FileName;
                            cmd.Parameters.Add("@FileSize", SqlDbType.BigInt).Value = 0;
                            if (string.IsNullOrEmpty(m.AttachmentInBase64))
                            {
                                cmd.Parameters.Add("@File", SqlDbType.VarBinary).Value = m.Attachment;
                            }
                            else
                            {
                                cmd.Parameters.Add("@File", SqlDbType.VarBinary).Value = Convert.FromBase64String(m.AttachmentInBase64);
                            }
                            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                            cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = order.Id;

                            SqlParameter param = new SqlParameter("@fileID", SqlDbType.VarChar);
                            param.Direction = ParameterDirection.Output;
                            param.Size = 4000;
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();

                            string fileId = cmd.Parameters["@fileID"].Value.ToString();
                        }
                    }
                }
            });
            foreach (var existingItem in ExistingAttachments)
            {
                bool HasFile = false;
                foreach (var item in order.Attachments)
                {
                    if ((!String.IsNullOrEmpty(item.Id)) && existingItem.Id.ToLower() == item.Id.ToLower())
                    {
                        HasFile = true;
                        break;
                    }
                }
                if (HasFile == false)
                    OrderDB.DeleteOrderAttachment(existingItem.Id);
            }
        }

        internal static ActionResult SaveOrderOPPerson(Order order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO [dbo].[Tbl_Operation_Performing_Person]
                                       ([doc_ID],[customer_number],[name],[last_name],[Person_Document],[Document_Type],[Assign_Id],[Social_Number],address,phone,residence,birth,email, no_social_number)
                                        VALUES
                                       (@docID,@customerNumber,@name,@lastName,@passport,@documentType,@assignId,@socialNumber,@address,@phone,@residence,@birth,@email, @no_social_number)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.OPPerson.CustomerNumber;
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonName) ? (object)order.OPPerson.PersonName : DBNull.Value;
                    cmd.Parameters.Add("@lastName", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonLastName) ? (object)order.OPPerson.PersonLastName : DBNull.Value;
                    cmd.Parameters.Add("@passport", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonDocument) ? (object)order.OPPerson.PersonDocument : DBNull.Value;
                    cmd.Parameters.Add("@assignId", SqlDbType.Float).Value = order.OPPerson.AssignId;
                    cmd.Parameters.Add("@socialNumber", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonSocialNumber) ? (object)order.OPPerson.PersonSocialNumber : DBNull.Value;
                    cmd.Parameters.Add("@no_social_number", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonNoSocialNumber) ? (object)order.OPPerson.PersonNoSocialNumber : DBNull.Value;
                    cmd.Parameters.Add("@documentType", SqlDbType.TinyInt).Value = !string.IsNullOrEmpty(order.OPPerson.PersonDocument) ? (object)1 : DBNull.Value;
                    cmd.Parameters.Add("@address", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonAddress) ? (object)order.OPPerson.PersonAddress : DBNull.Value;
                    cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = !string.IsNullOrEmpty(order.OPPerson.PersonPhone) ? (object)order.OPPerson.PersonPhone : DBNull.Value;
                    cmd.Parameters.Add("@residence", SqlDbType.TinyInt).Value = order.OPPerson.PersonResidence != 0 ? (object)order.OPPerson.PersonResidence : DBNull.Value;
                    cmd.Parameters.Add("@birth", SqlDbType.SmallDateTime).Value = order.OPPerson.PersonBirth != null ? (object)order.OPPerson.PersonBirth : DBNull.Value;
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = order.OPPerson.PersonEmail != null ? (object)order.OPPerson.PersonEmail : DBNull.Value;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;

                    return result;


                }
            }

        }

        internal static OPPerson GetOrderOPPerson(long id)
        {
            OPPerson person = new OPPerson();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * FROM Tbl_Operation_Performing_Person where doc_id=@docID";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = id;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            person.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                            person.PersonName = dr["name"].ToString();
                            person.PersonLastName = dr["last_name"].ToString();
                            person.PersonDocument = dr["Person_Document"] != DBNull.Value ? dr["Person_Document"].ToString() : null;
                            person.AssignId = long.Parse(dr["assign_id"].ToString());
                            person.PersonSocialNumber = dr["social_number"] != DBNull.Value ? dr["social_number"].ToString() : null;
                            person.PersonNoSocialNumber = dr["no_social_number"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["no_social_number"].ToString()) : null;
                            person.DocumentType = dr["document_type"] != DBNull.Value ? ushort.Parse(dr["document_type"].ToString()) : (ushort)0;
                            person.PersonAddress = dr["address"] != DBNull.Value ? dr["address"].ToString() : null;
                            person.PersonPhone = dr["phone"] != DBNull.Value ? dr["phone"].ToString() : null;
                        }
                    }
                }
            }

            return person;

        }

        public static void SetQualityHistoryUserId(long id, OrderQuality quality, int userId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE Tbl_HB_quality_history SET change_set_number=@userId where doc_id = @Doc_id and quality = @quality ";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = (short)quality;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static int GetBOOrderType(OrderType documentType, int documentSubType, int documentCommunalType)
        {
            int orderType = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.[fnc_Get_BO_order_type](@docType, @docSubType, @docCommunalType) as orderType", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docType", SqlDbType.SmallInt).Value = documentType;
                    cmd.Parameters.Add("@docSubType", SqlDbType.SmallInt).Value = (documentType == OrderType.FastTransferPaymentOrder || documentType == OrderType.ReceivedFastTransferPaymentOrder || documentType == OrderType.FastTransferFromCustomerAccount) ? 1 : documentSubType;
                    cmd.Parameters.Add("@docCommunalType", SqlDbType.SmallInt).Value = documentCommunalType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            orderType = int.Parse(dr["orderType"].ToString());
                        }
                    }

                }
            }

            return orderType;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հայտին կցված փաստաթղթերը (առանց scan-ի)
        /// </summary>
        /// <param name="orderId">Հայտի ունիկալ համար</param>
        /// <returns></returns>
        internal static List<OrderAttachment> GetOrderAttachments(long orderId)
        {
            List<OrderAttachment> attachments = new List<OrderAttachment>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select u.Id,u.FileName,u.FileType from Tbl_HB_documents d inner join Tbl_Uploaded_files u on d.doc_ID=u.doc_id where d.doc_ID=@docId", conn))
                {
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = orderId;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        OrderAttachment oneAttachment = new OrderAttachment();
                        oneAttachment.Id = row["Id"].ToString();
                        oneAttachment.FileName = row["FileName"].ToString();
                        oneAttachment.FileExtension = row["FileType"].ToString();
                        attachments.Add(oneAttachment);
                    }
                }


            }
            return attachments;
        }

        public static OrderAttachment GetOrderAttachment(string attachmentId)
        {
            OrderAttachment attachment = new OrderAttachment();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sqlString = @"SELECT Id,FileName,FileType,SystemFile FROM Tbl_Uploaded_files WHERE Id=@Id";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = attachmentId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            attachment.Id = dr["Id"].ToString();
                            attachment.FileName = dr["FileName"].ToString();
                            attachment.FileExtension = dr["FileType"].ToString();
                            attachment.Attachment = (byte[])dr["SystemFile"];
                        }
                    }

                }

            }
            return attachment;
        }


        public static void SaveOrderFee(Order order)
        {
            DataTable dt = new DataTable();
            order.Fees.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_Transfer_Fees where doc_ID = @id AND FeeType = @fee_type", conn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@fee_type", SqlDbType.Int).Value = m.Type;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }
                    }
                }
                if (dt?.Rows.Count == 0 || dt == null)
                {
                    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.Tbl_Transfer_Fees(doc_ID, FeeAmount, Currency, FeeType,debet_account,credit_account, OrderNumber,Description,description_for_reject_fee_type)
                                                    VALUES (@id,@fee_amount, @fee_currency,  @fee_type,@debit_acc,@credit_acc, @order_number,@Description,@descriptionForRejectFeeType)", conn))
                        {
                            if (m.Account != null)
                                cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = m.Account.AccountNumber;
                            else
                                cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
                            if (m.CreditAccount != null)
                                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = m.CreditAccount.AccountNumber;
                            else
                                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
                            if (m.Description != null)
                                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = m.Description;
                            else
                                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = DBNull.Value;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                            cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = m.Amount;
                            if (!string.IsNullOrEmpty(m.Currency))
                                cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = m.Currency;
                            else
                                cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = DBNull.Value;
                            cmd.Parameters.Add("@fee_type", SqlDbType.NVarChar, 20).Value = m.Type;
                            if (!string.IsNullOrEmpty(m.OrderNumber))
                                cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = m.OrderNumber;
                            else
                                cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                            if (!string.IsNullOrEmpty(m.DescriptionForRejectFeeType))
                                cmd.Parameters.Add("@descriptionForRejectFeeType", SqlDbType.NVarChar, 250).Value = m.DescriptionForRejectFeeType;
                            else
                                cmd.Parameters.Add("@descriptionForRejectFeeType", SqlDbType.NVarChar, 250).Value = DBNull.Value;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            });
        }

        public static void SaveBOOrderAttachments(Order order, ulong orderId)
        {
            order.Attachments.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_save_BO_order_attachment";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                        cmd.Parameters.Add("@fileType", SqlDbType.NVarChar, 5).Value = m.FileExtension;
                        cmd.Parameters.Add("@attachment", SqlDbType.VarBinary).Value = m.Attachment;
                        cmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 50).Value = m.FileName;

                        SqlParameter param = new SqlParameter("@fileID", SqlDbType.VarChar);
                        param.Direction = ParameterDirection.Output;
                        param.Size = 4000;
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        string fileId = cmd.Parameters["@fileID"].Value.ToString();
                    }
                }

            });
        }

        public static void ChangeBOOrderQuality(long docId, OrderQuality quality, ACBAServiceReference.User user)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_update_BO_order_quality";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DocId", SqlDbType.Int).Value = docId;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = (short)quality;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = user.userID;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Վերադարձնում է փոխանցման միջնորդավճարի տվյալները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderFee> GetOrderFees(long orderId)
        {
            List<OrderFee> fees = new List<OrderFee>();


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select * from Tbl_Transfer_Fees  where doc_ID=@docId", conn))
                {
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = orderId;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        OrderFee fee = new OrderFee();
                        fee.Amount = Convert.ToDouble(row["FeeAmount"].ToString());
                        fee.Currency = row["Currency"].ToString();
                        fee.Type = Convert.ToInt16(row["FeeType"].ToString());
                        fee.TypeDescription = Info.BankOperationFeeTypeDescription(fee.Type);
                        if (row["debet_account"] != DBNull.Value)
                        {
                            fee.Account = Account.GetAccount(row["debet_account"].ToString());
                        }

                        if (row["credit_account"] != DBNull.Value)
                        {
                            fee.CreditAccount = Account.GetSystemAccount(row["credit_account"].ToString());
                        }

                        if (row["Description"] != DBNull.Value)
                        {
                            fee.Description = row["Description"].ToString();
                        }

                        if (row["description_for_reject_fee_type"] != DBNull.Value)
                        {
                            fee.DescriptionForRejectFeeType = row["description_for_reject_fee_type"].ToString();
                        }

                        fee.OrderNumber = row["OrderNumber"].ToString();
                        fees.Add(fee);
                    }
                }


            }
            return fees;
        }

        /// <summary>
        /// Ստոգում է եղել է տվյալ հաշվեհամարի համար չհաստատված հայտ թե ոչ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Accountnumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool IsExistRequest(OrderType type, string Accountnumber, ulong customerNumber)
        {
            bool IsExistRequest = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT Doc_ID FROM tbl_hb_documents hb where document_type =@type and debet_account =@account  and quality in (2,3) and document_subtype =1  and customer_number =@customer_number", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@account", SqlDbType.NVarChar).Value = Accountnumber;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = (short)type;
                    if (cmd.ExecuteReader().Read())
                    {
                        IsExistRequest = true;
                    }
                }

            }
            return IsExistRequest;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ օգտագործողը կարող է ուղարկել հաստատման տվյալ վճարման հանձնարարականը:
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userGroups"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsAbleToChangeQuality(string userName, string userGroups, int id)
        {
            bool result = false;
            if (String.IsNullOrEmpty(userGroups))
            {
                return result;
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT distinct H.doc_id as doc_id FROM Tbl_approvement_process AP inner join TBl_HB_documents  H on AP.doc_id = H.doc_id
                                                        WHERE AP.step_status = 1 and H.doc_id = @docID" +
                    " and @username not in (select isnull([user_name], '') from Tbl_approvement_process where doc_id = H.doc_ID) and H.quality not in (40, 6, 31) " +
                    "and group_id in " + userGroups + " order by H.doc_id desc", conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = id;

                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar).Value = userName;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            result = true;
                    }
                }


            }

            return result;
        }

        public static bool IsAutomatConfirm(OrderType type, byte subType)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select automat_confirm_FRONT_OFFICE_order from Tbl_sub_types_of_HB_products where document_type=@type and document_sub_type=@subType ", conn))
                {
                    cmd.Parameters.Add("@type", SqlDbType.Float).Value = type;
                    cmd.Parameters.Add("@subType", SqlDbType.NVarChar).Value = (type == OrderType.ReceivedFastTransferPaymentOrder || type == OrderType.FastTransferFromCustomerAccount || type == OrderType.FastTransferPaymentOrder ? 1 : subType);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            result = Convert.ToBoolean(short.Parse(dr["automat_confirm_FRONT_OFFICE_order"].ToString()));
                    }
                }
            }
            return result;
        }


        public static OrderQuality GetNextQualityByStep(long id)
        {
            short result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT dbo.Fnc_Get_Next_Quality_By_Step(@Doc_id) as nextQuality";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = id;
                    result = Convert.ToInt16(cmd.ExecuteScalar());
                }
            }
            return (OrderQuality)result;
        }

        /// <summary>
        /// Ընթացիկ քայլի կարգավիճակը դարձնում է 2` կատարված, լրացնում է քայլը կատարողին և հաջորդ քայլի կարգավիճակը դարձնում է 1` ակտիվ
        /// </summary>
        /// <returns></returns>
        public static void UpdateApprovementProcess(int id, string userName)
        {

            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_update_approvement_process";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@user_name", SqlDbType.NVarChar).Value = userName;

                    cmd.ExecuteNonQuery();

                }
            }

        }



        public static void SaveOrderProductId(ulong appId, long orderId)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_HB_Products_Identity(HB_Doc_ID, App_ID)
                                                    VALUES (@docId,@appId)", conn))
                {
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = orderId;
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = appId;
                    cmd.ExecuteNonQuery();
                }
            }

        }

        internal static double GetSentNotConfirmedAmounts(string debitaccountNumber, OrderType ordertype, bool withConvertation = true)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT CASE WHEN @withConvertation = 0 THEN SUM(amount) 
			                                                    ELSE  SUM(amount  * ( CASE WHEN currency = 'AMD' THEN 1
											                                                    ELSE dbo.fnc_kurs_for_date(currency ,GETDATE()) END)) END amount
			                                                FROM Tbl_HB_documents 
			                                                WHERE quality = 3 
                                                            AND document_type = @ordertype  
                                                            AND credit_account = @cerdit_account
                                                            GROUP BY credit_account", conn))
                {
                    cmd.Parameters.Add("@cerdit_account", SqlDbType.NVarChar).Value = ordertype == OrderType.TransitPaymentOrder || debitaccountNumber == "0" ? debitaccountNumber : debitaccountNumber.Remove(0, 5);
                    cmd.Parameters.Add("@ordertype", SqlDbType.SmallInt).Value = ordertype;
                    cmd.Parameters.Add("@withConvertation", SqlDbType.Bit).Value = withConvertation;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            amount = double.Parse(dr["amount"].ToString());
                        }
                    }
                }
            }
            return amount;
        }

        public static void SaveOrderLinkId(int linkId, long orderId, ushort linkType)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_HB_document_link_identity(Doc_ID, link_ID, link_type)
                                                    VALUES (@docId,@link_Id,@linkType)", conn))
                {
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = orderId;
                    cmd.Parameters.Add("@link_Id", SqlDbType.Int).Value = linkId;
                    cmd.Parameters.Add("@linkType", SqlDbType.TinyInt).Value = linkType;
                    cmd.ExecuteNonQuery();
                }
            }

        }


        /// <summary>
        /// Վերադարձնում է օգտագործողի չհաստատված գործարքները
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        internal static List<Order> GetNotConfirmedOrders(short setNumber, int start = 0, int end = 0)
        {
            int initialCount = 3;
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = "";
                if (start == 0 && end == 0)
                {
                    sql = @"SELECT TOP(@initialCount)
                                ISNULL(d.source_type,0) as source,d.doc_ID,
                                    d.customer_number,
                                    d.document_type,
                                    d.document_subtype,
                                    d.document_number,
                                    d.registration_date,
                                    d.description,
                                    d.receiver_name,
                                    d.amount,
                                    d.currency,
                                    d.quality,
                                    d.operationFilialCode,d.operation_date 
                            FROM Tbl_HB_documents d with(nolock) 
                         INNER JOIN  Tbl_HB_quality_history q with(nolock) ON d.doc_ID = q.Doc_ID AND q.quality = 1 
                               WHERE d.quality in(2,3,20,50,100) and d.document_type not in ( 210, 211, 212, 226, 227, 125, 233, 243) and change_set_number = @userID order by d.doc_ID desc";
                }
                else
                {
                    sql = @"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY d.doc_ID desc) as row, ISNULL(d.source_type,0) as source,d.doc_ID,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.document_number,
                                                d.registration_date,
                                                d.description,
                                                d.receiver_name,
                                                d.amount,
                                                d.currency,
                                                d.quality,
                                                d.operationFilialCode,
                                                d.operation_date
                                          FROM Tbl_HB_documents d INNER JOIN  Tbl_HB_quality_history q ON d.doc_ID = q.Doc_ID AND q.quality = 1
                                          WHERE d.quality in(2,3,20,50,100) and d.document_type not in ( 210, 211, 212, 226, 227) and change_set_number = @userID) AS result
                                          WHERE result.row > (@initialCount+@start)  and result.row <= (@initialCount+@end)";
                }
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add("@initialCount", SqlDbType.Int).Value = initialCount;
                    cmd.Parameters.Add("@start", SqlDbType.Int).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.Int).Value = end;
                    cmd.CommandText = sql;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);

                        orders.Add(order);
                    }

                }
            }
            return orders;
        }


        internal static Order GetOrder(long orderId)
        {
            Order order = new Order();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.document_number,
                                                d.registration_date,
                                                d.description,
                                                d.receiver_name,
                                                d.amount,
                                                d.currency,
                                                d.quality,
                                                d.operationFilialCode,
                                                d.operation_date, 
                                                d.order_group_id,
                                                d.confirmation_date 
                                FROM Tbl_HB_documents d WHERE doc_id=@orderId ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        order = SetOrder(row);

                        if (row != null)
                            order.GroupId = row["order_group_id"] != DBNull.Value ? Convert.ToInt32(row["order_group_id"]) : 0;
                    }
                }
            }

            return order;
        }


        /// <summary>
        /// Պահպանում է հայտին կցված հաճախորդի համարները
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        public static void SaveOrderJointCustomer(long orderId, ulong customerNumber)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_order_joint_customer(Doc_ID, customer_number)
                                                    VALUES (@doc_ID,@customer_number)", conn))
                {

                    cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    cmd.ExecuteNonQuery();
                }
            }

        }

        /// <summary>
        /// Վերադարձնում է հայտին կցված հաճախորդի համարները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static List<KeyValuePair<ulong, string>> GetOrderJointCustomer(long orderId)
        {
            List<KeyValuePair<ulong, string>> jointCustomers = new List<KeyValuePair<ulong, string>>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT customer_number FROM Tbl_order_joint_customer WHERE doc_id=@orderId ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            KeyValuePair<ulong, string> customer = new KeyValuePair<ulong, string>(Convert.ToUInt64(dt.Rows[i]["customer_number"]), "");
                            jointCustomers.Add(customer);
                        }

                    }
                }
            }

            return jointCustomers;
        }


        internal static int GetTotalNotConfirmedOrder(int userId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    string sql = @"SELECT COUNT(*) 
                                  FROM Tbl_HB_documents d with(nolock)
                                  INNER JOIN  Tbl_HB_quality_history q with(nolock) ON d.doc_ID = q.Doc_ID AND q.quality = 1 
                                  WHERE d.quality in (2,3,20,50,100) and d.document_type not in (210, 211, 212, 226, 227,  125, 233, 243)
                                  and change_set_number = @userID ";

                    cmd.CommandText = sql;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    return Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }
            }

        }



        internal static List<Order> GetApproveReqOrder(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.document_number,
                                                d.registration_date,
                                                d.description,
                                                d.receiver_name,
                                                d.amount,
                                                d.currency,
                                                d.quality,
                                                d.operationFilialCode,d.operation_date 
                                FROM Tbl_HB_documents d WHERE customer_number=@customerNumber and registration_date>=@dateFrom and registration_date<=@dateTo and quality=5 order by registration_date desc,doc_id desc";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);

                        orders.Add(order);
                    }

                }
            }
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա վճարման հանձնարարականները
        /// <param name="customerNumber"></param>
        /// <param name="userName"></param>
        /// <param name="subTypeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="langId"></param>
        /// <param name="receiverName"></param>
        /// <param name="account"></param>
        /// <param name="period"></param>
        /// <param name="groups"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static List<Order> GetConfirmRequiredOrders(ulong customerNumber, string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1)
        {
            List<Order> ordersList = new List<Order>();
            if (!String.IsNullOrEmpty(groups))
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
                {
                    conn.Open();
                    string dateStr = "";
                    string accountStr = "";
                    string receiverNameStr = "";
                    string subTypeCond = "";
                    string subTypeJoin = "";
                    string qualityStr = "";

                    using (SqlCommand cmd = new SqlCommand(@"", conn))
                    {
                        if (startDate != default(DateTime) && period == true)
                        {
                            dateStr += " and H.Registration_Date >=@startDate";
                            cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate.Date;
                        }

                        if (endDate != default(DateTime) && period == true)
                        {
                            dateStr += " and H.Registration_Date <= @endDate";
                            cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = endDate.Date;
                        }

                        if (!String.IsNullOrEmpty(account))
                        {
                            accountStr = " And case when isnull(credit_bank_code,0)<>0 then cast(credit_bank_code as varchar(5))  else '' end + Credit_account LIKE  '%'+@account +'%' ";
                            cmd.Parameters.Add("@account", SqlDbType.VarChar).Value = account;
                        }
                        if (quality > -1)
                        {
                            qualityStr = " and quality = @quality ";
                            cmd.Parameters.Add("@quality", SqlDbType.VarChar).Value = quality;
                        }

                        if (subTypeId > 0)
                        {
                            subTypeJoin = " inner join Tbl_sub_types_of_HB_products P on H.document_type = P.document_type and H.document_subtype = P.document_sub_type ";
                            if (subTypeId == 2)
                            {
                                subTypeCond = " and P.sub_type_id in (@subTypeId,1,6) ";
                            }
                            else if (subTypeId == 3)
                            {
                                subTypeCond = " and P.sub_type_id in ( @subTypeId,4)  ";
                            }
                            else if (subTypeId == 15)
                            {
                                subTypeCond = " and P.sub_type_id in (@subTypeId,16)  ";
                            }
                            else
                            {
                                subTypeCond = " and P.sub_type_id = @subTypeId";
                            }
                            cmd.Parameters.Add("@subTypeId", SqlDbType.SmallInt).Value = subTypeId;

                        }




                        if (!String.IsNullOrEmpty(receiverName))
                        {
                            String[] names = receiverName.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            int j = 0;

                            receiverNameStr = " AND ( 1=1 ";

                            for (j = 0; j < names.Length; j++)
                            {
                                cmd.Parameters.Add("@name" + j, SqlDbType.NVarChar).Value = names[j];
                                receiverNameStr = receiverNameStr + " AND  [dbo].[armUpper](dbo.getascii(d.receiver_name)) LIKE '%'+[dbo].[armUpper](@name" + j + ") +'%' ";
                            }

                            receiverNameStr += ") ";

                        }

                        string sqlString = @"
                                                       SELECT	                                               
                                                       ISNULL(source_type,0) as source,
								                       doc_ID,
                                                       customer_number,
                                                       document_type,
                                                       document_subtype,
                                                       document_number,
                                                       registration_date,
                                                       description,
                                                       receiver_name,
                                                       amount,
                                                       currency,
                                                       quality,
                                                       operationFilialCode,
                                                       operation_date,
                                                       debet_account 
								                       FROM TBl_HB_documents 
                                                      WHERE doc_ID IN
                        (
                        SELECT distinct H.doc_id as doc_id FROM Tbl_approvement_process AP inner join TBl_HB_documents  H on AP.doc_id = H.doc_id" +
                        subTypeJoin + receiverNameStr +
                        " WHERE AP.step_status = 1 and H.customer_number=@customer_number" + dateStr + accountStr +
                        subTypeCond +
                        " and @username not in (select isnull([user_name], '') from Tbl_approvement_process where doc_id = H.doc_ID) and H.quality not in (40, 6, 31) " +
                        " and H.source_type NOT IN ( 2,6)  " +
                        "and group_id in " + groups +
                        qualityStr +
                        " ) and document_type NOT IN (132, 137, 69, 135, 116, 138, 18) and quality in (1,5) order by doc_id desc";

                        cmd.CommandText = sqlString;



                        cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                        cmd.Parameters.Add("@userName", SqlDbType.NVarChar).Value = userName;

                        DataTable dt = new DataTable();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ordersList.Add(SetOrder(dt.Rows[i], true));
                        }
                    }


                }
            }

            return ordersList;
        }
        internal static ulong GetOrderTransactionsGroupNumber(long OrderID)
        {
            ulong transactionsGroupNumber = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select Transactions_Group_number from Tbl_HB_products_accordance where doc_ID=@DocID", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = OrderID;
                    transactionsGroupNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return transactionsGroupNumber;
        }


        internal static bool CheckOrderHasCustomerNumber(long orderId)
        {
            bool check = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT customer_number FROM Tbl_HB_documents WHERE doc_id=@orderId ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                    conn.Open();

                    ulong customerNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                    if (customerNumber != 0)
                    {
                        check = true;
                    }
                }
            }

            return check;
        }



        public static void UpdateOrderFee(Order order)
        {
            order.Fees.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_Transfer_Fees
                                                            SET FeeAmount=@fee_amount,
                                                            Currency=@fee_currency,
                                                            debet_account=@debit_acc,
                                                            credit_account=@credit_acc,
                                                            OrderNumber=@order_number,
                                                            Description=@Description
                                                            WHERE doc_ID=@docId and FeeType=@fee_type", conn))
                    {
                        if (m.Account != null)
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = m.Account.AccountNumber;
                        else
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
                        if (m.CreditAccount != null)
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = m.CreditAccount.AccountNumber;
                        else
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;

                        if (m.Description != null)
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = m.Description;
                        else
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = DBNull.Value;

                        cmd.Parameters.Add("@docId", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = m.Amount;
                        if (!string.IsNullOrEmpty(m.Currency))
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = m.Currency;
                        else
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                        cmd.Parameters.Add("@fee_type", SqlDbType.NVarChar, 20).Value = m.Type;

                        if (!string.IsNullOrEmpty(m.OrderNumber))
                            cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = m.OrderNumber;
                        else
                            cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }
        public static void UpdateQuality(long id, OrderQuality quality)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE Tbl_HB_documents SET quality = @quality WHERE doc_id = @Doc_id ";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = (short)quality;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void SaveOrderDetails(Order order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"	
                                                    INSERT INTO tbl_payment_registration_request_details
                                                    (Doc_ID,order_id,payment_date,terminal_id,phone_number,payment_session_id)
                                                    values
                                                    (@Doc_ID,@order_id,@payment_date,@terminal_id,@phone_number,@payment_session_id)
                                                    Select Scope_identity() as ID
                                            ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@order_id", SqlDbType.BigInt).Value = order.OrderId;
                cmd.Parameters.Add("@payment_date", SqlDbType.SmallDateTime).Value = order.PaymentDateTime;
                cmd.Parameters.Add("@terminal_id", SqlDbType.NVarChar, 255).Value = order.TerminalID;
                cmd.Parameters.Add("@phone_number", SqlDbType.VarChar, 30).Value = order.PhoneNumber;
                cmd.Parameters.Add("@payment_session_id", SqlDbType.VarChar, 50).Value = order.PaymentSessionID ?? "";
                cmd.ExecuteScalar();


            }
        }
        internal static bool IsPaymentIdUnique(Order order)
        {
            bool isUnique = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"IF @source_type <> 10
	                                                        SELECT  hb.doc_ID  from Tbl_HB_documents hb
	                                                        INNER JOIN tbl_payment_registration_request_details req
	                                                        ON hb.doc_ID=req.Doc_ID
	                                                        WHERE hb.debet_account=@accountNumber and req.order_id=@paymentId
                                                        ELSE
	                                                        SELECT  hb.doc_ID  from Tbl_HB_documents hb
	                                                        INNER JOIN tbl_payment_registration_request_details req
	                                                        ON hb.doc_ID=req.Doc_ID
	                                                        WHERE req.order_id=@paymentId and document_type = @document_type", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@paymentId", SqlDbType.Int).Value = order.OrderId;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = order.Source;
                    if (order.Source == SourceType.SSTerminal)
                        cmd.Parameters.Add("@terminalID", SqlDbType.VarChar, 30).Value = order.TerminalID;
                    else
                        cmd.Parameters.Add("@terminalID", SqlDbType.VarChar, 30).Value = DBNull.Value;


                    cmd.Parameters.Add("@document_type", SqlDbType.VarChar, 30).Value = (short)order.Type;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        isUnique = true;
                    }
                }

            }
            return isUnique;
        }

        internal static Order GetOrderDetails(long orderId)
        {
            Order order = new Order();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = "pr_GetOrderDetails";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@orderID", SqlDbType.Int).Value = orderId;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        order = SetOrderDetails(row);
                    }
                }
            }
            return order;
        }

        private static Order SetOrderDetails(DataRow row)
        {
            Order order = new Order();

            if (row != null)
            {
                order.Amount = double.Parse(row["amount"].ToString());
                order.Currency = row["currency"].ToString();
                order.RegistrationDate = row["PaymentReceiveDateTime"] != DBNull.Value ? Convert.ToDateTime(row["PaymentReceiveDateTime"]) : default(DateTime);
                order.PaymentDateTime = row["PaymentConfirmDateTime"] != DBNull.Value ? Convert.ToDateTime(row["PaymentConfirmDateTime"]) : default(DateTime);
                order.Quality = (OrderQuality)short.Parse(row["Quality"].ToString());
                order.AdditionalParametrs = new List<AdditionalDetails>();
                AdditionalDetails detail = new AdditionalDetails();
                detail.AdditionValue = row["Rate"].ToString();
                detail.AdditionType = 100;
                detail.AdditionTypeDescription = "Փոխարժեք";
                order.AdditionalParametrs.Add(detail);
            }
            return order;

        }

        public static void SubmitOrderFee(Order order)
        {
            order.Fees.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.Tbl_Transfer_Fees(doc_ID, FeeAmount, Currency, FeeType,debet_account,credit_account, OrderNumber,Description)
                                                    VALUES (@id,@fee_amount, @fee_currency,  @fee_type,@debit_acc,@credit_acc, @order_number,@Description)", conn))
                    {
                        if (m.Account != null)
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = m.Account.AccountNumber;
                        else
                            cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
                        if (m.CreditAccount != null)
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = m.CreditAccount.AccountNumber;
                        else
                            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;

                        if (m.Description != null)
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = m.Description;
                        else
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = DBNull.Value;

                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = m.Amount;
                        if (!string.IsNullOrEmpty(m.Currency))
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = m.Currency;
                        else
                            cmd.Parameters.Add("@fee_currency", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                        cmd.Parameters.Add("@fee_type", SqlDbType.NVarChar, 20).Value = m.Type;

                        if (!string.IsNullOrEmpty(m.OrderNumber))
                            cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = m.OrderNumber;
                        else
                            cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        internal static List<Order> GetOrders(OrderFilter searchParams, ulong customerNumber)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ISNULL(d.source_type,0) as source,d.doc_ID,
                                d.customer_number,
                                d.document_type,
                                d.document_subtype,
                                d.document_number,
                                d.registration_date,
                                d.description,
                                d.receiver_name,
                                d.amount,
                                d.currency,
                                d.quality,
                                d.operationFilialCode,
                                d.operation_date
                            FROM Tbl_HB_documents d WHERE d.quality<>40 and document_type not in (135,137,138,151,157,158) ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;


                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    sql += " AND customer_number = @customerNumber ";


                    if (searchParams.Id != 0)
                    {
                        cmd.Parameters.Add("@docId", SqlDbType.Int).Value = searchParams.Id;
                        sql = sql + " and d.doc_id=@docId";
                    }
                    else
                    {
                        if (searchParams.DateFrom != null)
                        {
                            cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = searchParams.DateFrom;
                            sql = sql + " and registration_date>=@dateFrom";
                        }

                        if (searchParams.DateTo != null)
                        {
                            cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = searchParams.DateTo;

                            sql = sql + " and registration_date<=@dateTo";
                        }

                        if (searchParams.OrderQuality != OrderQuality.NotDefined)
                        {
                            cmd.Parameters.Add("@quality", SqlDbType.Int).Value = searchParams.OrderQuality;
                            sql = sql + " and d.quality=@quality";
                        }

                        if (searchParams.Source != SourceType.NotSpecified)
                        {
                            cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = searchParams.Source;
                            sql = sql + " and source_type=@sourceType";
                        }


                        if (searchParams.Type != OrderType.NotDefined)
                        {
                            cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = searchParams.Type;
                            sql = sql + " and document_type=@documentType";
                        }

                        if (searchParams.SubType > 0)
                        {
                            cmd.Parameters.Add("@documentSubType", SqlDbType.SmallInt).Value = searchParams.SubType;
                            sql = sql + " and document_subtype=@documentSubType";
                        }


                    }

                    sql = sql + " order by registration_date desc,d.doc_id desc";


                    cmd.CommandText = sql;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Order order = SetOrder(row);

                        orders.Add(order);
                    }

                }
            }
            return orders;
        }


        internal static List<Order> GetOrdersList(ulong customerNumber, OrderListFilter orderListFilter)
        {
            List<Order> orders = new List<Order>();

            List<string> IPayAccounts = GetIPayAccounts();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                string dateStr = "";
                string accountStr = "";
                string receiverNameStr = "";
                string orderTypeStr = "";
                string qualityStr = "";
                string abonentNumberStr = "";
                string abonentNumberJoin = "";
                string sourceStr = "";
                string receiverCardNumberStr = "";
                string receiverCardNumberJoin = "";
                string groupIdFilter = "";

                using (SqlCommand cmd = new SqlCommand(@"", conn))
                {
                    if (orderListFilter.DateFrom != default(DateTime))
                    {
                        dateStr += " and H.Registration_Date >=@startDate";
                        cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = orderListFilter.DateFrom;
                    }

                    if (orderListFilter.DateTo != default(DateTime))
                    {
                        dateStr += " and H.Registration_Date <= @endDate";
                        cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = orderListFilter.DateTo;
                    }

                    if (!string.IsNullOrEmpty(orderListFilter.ReceiverAccount))
                    {
                        accountStr = " And case when isnull(h.credit_bank_code,0)<>0 then cast(h.credit_bank_code as varchar(5))  else '' end + h.Credit_account LIKE  '%'+@account +'%' ";
                        cmd.Parameters.Add("@account", SqlDbType.VarChar).Value = orderListFilter.ReceiverAccount;
                    }

                    if (orderListFilter.GroupId > 0)
                    {
                        groupIdFilter = " and order_group_id = @group_id";
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = orderListFilter.GroupId;
                    }

                    if (orderListFilter.OrderQualities != null && orderListFilter.OrderQualities.Count != 0)
                    {
                        string orderQualitiesString = "";
                        orderListFilter.OrderQualities.ForEach(m =>
                                orderQualitiesString += m + ","
                            );
                        if (!string.IsNullOrEmpty(orderQualitiesString))
                        {
                            orderQualitiesString = "(" + orderQualitiesString.Substring(0, orderQualitiesString.Length - 1) + ")";
                        }
                        qualityStr = " and h.quality in" + orderQualitiesString;

                    }


                    if (orderListFilter.OrderTypes != null && orderListFilter.OrderTypes.Count != 0)
                    {
                        string orderTypeString = "";
                        orderListFilter.OrderTypes.ForEach(m =>
                                orderTypeString += m + ","
                            );
                        if (!string.IsNullOrEmpty(orderTypeString))
                        {
                            orderTypeString = "(" + orderTypeString.Substring(0, orderTypeString.Length - 1) + ")";
                        }


                        orderTypeStr = " and h.document_type in " + orderTypeString;

                    }




                    if (!string.IsNullOrEmpty(orderListFilter.ReceiverName))
                    {
                        string[] names = orderListFilter.ReceiverName.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int j = 0;



                        for (j = 0; j < names.Length; j++)
                        {
                            cmd.Parameters.Add("@name" + j, SqlDbType.NVarChar).Value = names[j];
                            receiverNameStr = receiverNameStr + " AND h.receiver_name LIKE N'%'+dbo.fnc_convertAnsiToUnicode(@name" + j + ")+'%' ";
                        }

                    }


                    if (!string.IsNullOrEmpty(orderListFilter.AbonentNumber))
                    {
                        abonentNumberJoin = " inner join Tbl_HB_UtilityPayments UP ON h.DOC_ID = UP.docid";
                        abonentNumberStr = " and cod   LIKE  '%'+@abonentNumber +'%' ";
                        cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 25).Value = orderListFilter.AbonentNumber.Trim();

                    }

                    if (orderListFilter.Source != SourceType.NotSpecified)
                    {
                        cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = orderListFilter.Source;
                        sourceStr = sourceStr + " and h.source_type=@sourceType";
                    }
                    else
                    {
                        sourceStr = sourceStr + " and h.source_type in (1,3,4,5) ";

                    }


                    if (!string.IsNullOrEmpty(orderListFilter.ReceiverCardNumber))
                    {
                        receiverCardNumberJoin = " inner join tbl_cardtocard_order_details CC ON h.DOC_ID = CC.doc_id";
                        receiverCardNumberStr = " and credit_card_number LIKE  '%'+@creditCard +'%' ";
                        cmd.Parameters.Add("@creditCard", SqlDbType.NVarChar, 25).Value = orderListFilter.ReceiverCardNumber.Trim();

                    }

                    string sqlString = @"SELECT	                                               
                                                       ISNULL(source_type,0) as source,
								                       doc_ID,
                                                       customer_number,
                                                       document_type,
                                                       document_subtype,
                                                       document_number,
                                                       registration_date,
                                                       description,
                                                       receiver_name,
                                                       amount,
                                                       currency,
                                                       quality,
                                                       operationFilialCode,
                                                       operation_date,
                                                       debet_account
								                       FROM TBl_HB_documents 
                                                      WHERE doc_ID IN
                        (select  H.doc_ID from Tbl_HB_documents H " + abonentNumberJoin + abonentNumberStr + receiverCardNumberJoin + receiverCardNumberStr + " WHERE H.quality <> 40 AND H.document_type NOT IN (132, 137, 69, 135, 116, 138) AND H.customer_number=@customer_number " + groupIdFilter + receiverNameStr + dateStr + accountStr + sourceStr + qualityStr + orderTypeStr + receiverCardNumberStr + " ) order by doc_id desc";
                    cmd.CommandText = sqlString;



                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;



                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        orders.Add(SetOrder(dt.Rows[i], true));
                    }
                    //Կցված քարտով գործարքների ժամանակ ցույց ենք տալիս կցված քարտի համարը POS Terminal-ի համարի փոխարեն
                    if (IPayAccounts != null && orders != null && orders.Any(order => IPayAccounts.Contains(order?.DebitAccount?.AccountNumber) || order.Type == OrderType.AttachedCardToCardOrder))
                    {
                        List<Order> IPayOrders = orders.Where(order => IPayAccounts.Contains(order?.DebitAccount?.AccountNumber) || order.Type == OrderType.AttachedCardToCardOrder).ToList();
                        Dictionary<long, string> AttachedCards = GetAttachedCardsByDocId(IPayOrders.Select(order => order.Id).ToList());
                        foreach (Order order in IPayOrders)
                        {
                            order.DebitAccount.IsAttachedCard = true;
                            order.DebitAccount.AccountNumber = AttachedCards.ContainsKey(order.Id) ? AttachedCards[order.Id] : "-";
                        }
                    }
                }

            }

            return orders;
        }
        internal static List<string> GetIPayAccounts()
        {
            List<string> IPayAccounts = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT Account_number FROM [tbl_IPay_account_numbers]", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            IPayAccounts.Add(reader["Account_number"].ToString());
                        }
                    }
                }
            }
            return IPayAccounts;

        }
        internal static ActionResult UpdateHBdocumentQuality(long docID, User user)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "HB_OK_confirm";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@Uniq_Item_number", SqlDbType.BigInt).Value = DBNull.Value;
                    cmd.Parameters.Add("@transactions_Group_number", SqlDbType.BigInt).Value = DBNull.Value;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = user.userID;

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;

        }

        internal static ActionResult ConfirmOrderOnline(long docID, User user)
        {

            ActionResult result = new ActionResult();
            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_confirm_online_transfers";
                    cmd.CommandTimeout = 120;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                    cmd.ExecuteNonQuery();
                }
            }
            return result;

        }
        /// <summary>
        /// Վերադարձնում է տվյալ հայտին կցված բոլոր փաստաթղթերը 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static List<OrderAttachment> GetFullOrderAttachments(long orderId)
        {
            List<OrderAttachment> attachments = new List<OrderAttachment>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select u.Id,u.FileName,u.FileType,u.SystemFile from Tbl_HB_documents d inner join Tbl_Uploaded_files u on d.doc_ID=u.doc_id where d.doc_ID=@docId", conn))
                {
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = orderId; ;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        OrderAttachment oneAttachment = new OrderAttachment();
                        oneAttachment.Id = row["Id"].ToString();
                        oneAttachment.FileName = row["FileName"].ToString();
                        oneAttachment.FileExtension = row["FileType"].ToString();
                        oneAttachment.AttachmentInBase64 = Convert.ToBase64String((byte[])row["SystemFile"]);
                        attachments.Add(oneAttachment);
                    }
                }


            }
            return attachments;
        }

        internal static ushort GetHBDocumentOperationFilialcode(long orderID)
        {
            ushort filialCode = 0;
            if (orderID != 0)
            {
                SqlConnection conn;
                using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "Select dbo.fn_get_HB_document_operation_filialcode(" + orderID.ToString() + ")";
                        cmd.CommandType = CommandType.Text;

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToString(reader[0]) != "")
                                {
                                    filialCode = Convert.ToUInt16(reader[0]);
                                }
                            }
                        }
                    }
                }
            }

            return filialCode;
        }

        internal static OrderQuality GetOrderQualityByDocID(long docID)
        {
            OrderQuality quality = new OrderQuality();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select quality from tbl_hb_documents where doc_id = @docID";
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = docID;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                        }
                    }
                }
            }
            return quality;
        }

        internal static void PostCurrencyMarketStatus(long docID)
        {
            long uniqNumber = 0;
            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT unique_number  FROM [dbo].[Tbl_journal_of_confirmations] where HB_doc_ID = " + docID.ToString();
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToString(reader["unique_number"]) != "")
                            {
                                uniqNumber = Convert.ToInt64(reader["unique_number"]);
                            }
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "Update_tbl_currency_market_status";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@unique_number", SqlDbType.BigInt).Value = uniqNumber;
                    cmd.Parameters.Add("@Status", SqlDbType.Int).Value = 1;

                    cmd.ExecuteNonQuery();
                }

            }
        }

        /// <summary>
        /// Վերադարձնում է հայտին կցված փաստաթուղթը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static string GetOrderAttachmentInBase64(string attachememntId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select SystemFile from Tbl_Uploaded_files where id = @id", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = attachememntId;
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    DataRow row = dt.Rows[0];
                    return Convert.ToBase64String((byte[])row["SystemFile"]);
                }
            }
        }


        internal static bool CheckCustomerConvertationLimit(ulong customerNumber, double amount, string currency, double rate, DateTime? orderDate, bool isCross)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT dbo.fn_check_customer_limit_for_convertation(
                                                                                    @customer_number,
                                                                                    @operation_date,
                                                                                    @current_operation_amount,
                                                                                    @current_operation_currency,
                                                                                    @is_cross
                                                                                    ) result";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@operation_date", SqlDbType.SmallDateTime).Value = orderDate.Value;
                    cmd.Parameters.Add("@current_operation_amount", SqlDbType.Float).Value = amount;
                    cmd.Parameters.Add("@current_operation_currency", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@is_cross", SqlDbType.Float).Value = isCross;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = (bool)dr["result"];
                        }

                    }

                }
            }

            return result;
        }
        public static void DeleteOrderAttachment(string attachmentId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM Tbl_Uploaded_files WHERE id = @id", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = attachmentId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static bool HasUploadedLoanContract(long orderId, int type)
        {

            bool result = false;


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select * FROM tbl_hb_attached_documents where doc_id = @docID and attachment_type = @type", conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = orderId;

                    cmd.Parameters.Add("@type", SqlDbType.Int).Value = type;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            result = true;
                    }
                }


            }

            return result;
        }

        internal static string GetDocFlowRejectReason(long orderId)
        {
            string rejectReason = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select reject_description from Tbl_link_to_docflow   where doc_id = @docID", conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = orderId;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            rejectReason = Utility.ConvertAnsiToUnicode(dr["reject_description"].ToString());
                        }
                    }
                }
            }
            return rejectReason;
        }




        public static ActionResult ConfirmRestrictedOrder(long id, ACBAServiceReference.User user)
        {
            ActionResult actionResult = new ActionResult();
            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_FRONT_OFFICE_order_confirm";
                    cmd.CommandTimeout = 120;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                    cmd.ExecuteNonQuery();
                }

                actionResult.ResultCode = ResultCode.Normal;
                actionResult.Id = id;

                return actionResult;
            }
        }

        public static int GetOrderDailyCount(short orderType,short subType, DateTime date, ulong customerNumber)
        {
            int orderCount = 0;
            using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(doc_id) order_count  FROM [dbo].[tbl_hb_documents] where document_type = @orderType and document_subtype=@subType and registration_date=@regDate and quality in(2,3,4,30) and customer_number=@customerNumber";
                    cmd.Parameters.Add("@orderType", SqlDbType.TinyInt).Value = orderType;
                    cmd.Parameters.Add("@subType", SqlDbType.TinyInt).Value = subType;
                    cmd.Parameters.Add("@regDate", SqlDbType.SmallDateTime).Value = date;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        orderCount = Convert.ToInt32(reader["order_count"].ToString());
                    }
                }
            }
            return orderCount;
        }
        public static double GetOrderDailyAmount(short orderType,short orderSubType, DateTime date, ulong customerNumber)
        {
            double orderAmount = 0;
            using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT ISNULL(sum(case currency when 'AMD' then amount 
                                            else amount* dbo.fnc_kurs_for_date([currency],@regDate) end),0) total_amount  FROM [dbo].[tbl_hb_documents] where document_type = @orderType and registration_date=@regDate and quality in(2,3,4,30) and customer_number=@customerNumber and document_subtype=@subType";
                    cmd.Parameters.Add("@orderType", SqlDbType.TinyInt).Value = orderType;
                    cmd.Parameters.Add("@subType", SqlDbType.TinyInt).Value = orderSubType;
                    cmd.Parameters.Add("@regDate", SqlDbType.SmallDateTime).Value = date;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        orderAmount = Convert.ToDouble(reader["total_amount"].ToString());
                    }
                }
            }

            return orderAmount;
        }

        public static ActionResult Reject(long orderId, OrderQuality quality, int rejectId, short userId)
        {
            ActionResult result = new ActionResult();

            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hbbaseconn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HB_Rejection_confirm";
                    cmd.CommandTimeout = 120;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@set_Date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@set_Number", SqlDbType.SmallInt).Value = userId;
                    cmd.Parameters.Add("@RejectionReasonID", SqlDbType.Int).Value = rejectId;
                    cmd.Parameters.Add("@RejectionQuality", SqlDbType.SmallInt).Value = (short)quality;
                    cmd.ExecuteNonQuery();
                }
            }
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        internal static Dictionary<long, string> GetAttachedCardsByDocId(List<long> doc_ids)
        {
            Dictionary<long, string> AttachedCards = new Dictionary<long, string>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"SELECT doc_id, card_number FROM tbl_other_bank_card_orders ord JOIN (SELECT id, card_number  FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION SELECT id, card_number FROM [tbl_other_bank_cards_deleted]) crd ON ord.card_id = crd.Id WHERE doc_id IN ({string.Join(", ", doc_ids)})";
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AttachedCards.Add(Convert.ToInt64(dr["doc_id"]), dr["card_number"].ToString());
                        }
                    }
                }
            }
            return AttachedCards;
        }

        internal static List<long> GetAttachedCardOrdersByDocId(List<int> doc_ids)
        {
            List<long> AttachedCardDocIds = new List<long>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"select doc_id from tbl_other_bank_card_orders where doc_id in ({string.Join(", ", doc_ids)})";
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AttachedCardDocIds.Add(Convert.ToInt64(dr["doc_id"]));
                        }
                    }
                }
            }
            return AttachedCardDocIds;
        }

        public static int GetConfirmRequiredOrdersCount(ulong customerNumber, string userName, string groups = "")
        {
            int ordersCount = 0;
            if (!String.IsNullOrEmpty(groups))
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"", conn))
                    {

                        string sqlString = @"
                                                       SELECT	                                               
                                                       count(1) as ordersCount
								                       FROM TBl_HB_documents 
                                                      WHERE doc_ID IN
                        (
                        SELECT distinct H.doc_id as doc_id FROM Tbl_approvement_process AP inner join TBl_HB_documents  H on AP.doc_id = H.doc_id" +
                        " WHERE AP.step_status = 1 and H.customer_number=@customer_number" +
                        " and @username not in (select isnull([user_name], '') from Tbl_approvement_process where doc_id = H.doc_ID) and H.quality not in (40, 6, 31) " +
                        " and H.source_type IN ( 1,3,4,5 )  " +
                        "and group_id in " + groups +
                        " ) and document_type NOT IN (132, 137, 69, 135, 116, 138, 18) and quality in (1,5) ";

                        cmd.CommandText = sqlString;


                        cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                        cmd.Parameters.Add("@userName", SqlDbType.NVarChar).Value = userName;


                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            ordersCount = Convert.ToInt32(reader["ordersCount"].ToString());
                        }
                    }


                }
            }

            return ordersCount;
        }

        internal static DateTime GetTransferSentDateTime(int docID)
        {
            DateTime sentDateTime = default(DateTime);


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT change_date FROM Tbl_HB_quality_history WHERE quality = 3 and Doc_ID = @Doc_ID", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;

                    sentDateTime = Convert.ToDateTime(cmd.ExecuteScalar());

                }

            }
            return sentDateTime;
        }

        public static void SetDefaultRestConfigs(string userName)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT U.id, A.customer_number 
                                                            FROM Tbl_Users U
                                                            INNER JOIN Tbl_Applications a
                                                            on u.global_id = a.ID
                                                            Where user_Name = @userName and u.id not in (
																SELECT digital_user_id
																FROM [dbo].[tbl_digital_account_rest_configuration]
															)", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 50).Value = userName;

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int digitalUserID = Convert.ToInt32(reader["id"].ToString());
                        ulong customerNumber = Convert.ToUInt64(reader["customer_number"].ToString());
                        DigitalAccountRestConfigurationsDB.SaveDefaultAccountRestConfigurations(digitalUserID, customerNumber);
                    }
                }
            }
        }

        internal static double GetSentNotConfirmedWithdrawalAmount(string debitaccountNumber)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT SUM( amount ) amount
			                                                FROM Tbl_HB_documents 
			                                                WHERE quality = 3 
                                                            AND document_type in ( 52 , 55)  
                                                            AND  debet_account = @debet_account
                                                            GROUP BY debet_account", conn))
                {
                    cmd.Parameters.Add("@debet_account", SqlDbType.NVarChar).Value = debitaccountNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            amount = double.Parse(dr["amount"].ToString());
                        }
                    }
                }
            }
            return amount;
        }

        internal static OrderType GetOrderType(long docId)
        {
            OrderType type = OrderType.NotDefined;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string query = "SELECT document_type FROM Tbl_HB_documents WHERE doc_ID = @doc_id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
                    type = (OrderType)Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return type;
        }

    }
}
