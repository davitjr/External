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
    internal class BillSplitOrderDB
    {
        internal static ActionResult Save(BillSplitOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_bill_split_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;


                    string creditAccountNumber;


                    creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);

                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;

                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 4000).Value = order.Description;
                    cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 250).Value = order.Receiver;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;
                    cmd.Parameters.Add("@receiver_add_inf", SqlDbType.NVarChar).Value = order.OtherDescription;

                    using (var table = new DataTable())
                    {
                        table.Columns.Add("customer_number", typeof(double));
                        table.Columns.Add("amount", typeof(double));
                        table.Columns.Add("is_link_payment", typeof(bool));
                        table.Columns.Add("phone_number", typeof(string));
                        table.Columns.Add("email", typeof(string));
                        table.Columns.Add("link_url", typeof(string));
                        table.Columns.Add("link_payment_doc_id", typeof(long));

                        foreach (BillSplitSenderInfo sender in order.Senders)
                        {
                            table.Rows.Add(sender.CustomerNumber, sender.Amount, sender.IsLinkPayment, sender.PhoneNumber, sender.EmailAddress, null, sender.LinkPaymnentOrderId);
                        }


                        var pList = new SqlParameter("@senders", SqlDbType.Structured);
                        pList.TypeName = "dbo.BillSplitSender";
                        pList.Value = table;

                        cmd.Parameters.Add(pList);
                    }







                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }



                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    }

                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;



                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    int actionResult = Convert.ToInt32(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return result;
                }
            }
        }


        internal static int GetBillSplitOrdersCount(OrderQuality quality, ulong customerNumber)
        {
            int ordersCount = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                //string str = @"select count(id) as orderCount from TBl_bill_split_order_details B inner join TBl_HB_Documents D on B.doc_id = D.doc_id where customer_number = @customerNumber and status = @status";
                string str = @"select count(doc_id) as orderCount from TBl_HB_Documents where customer_number = @customerNumber and quality = @quality and document_type = 245";


                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@quality", SqlDbType.Float).Value = quality;
                    //cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = status;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ordersCount = Convert.ToInt32(dr["orderCount"]);
                        }
                    }
                }

            }

            return ordersCount;
        }

        internal static BillSplitOrder GetBillSplitOrder(BillSplitOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select *  
                                                            from Tbl_Hb_Documents
                                                            where doc_id=@id and customer_number=case when @customerNumber = 0 then customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            order.SubType = Convert.ToByte((dr["document_subtype"]));


                            order.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();

                                if (order.Type == OrderType.RATransfer || order.Type == OrderType.CashDebit || order.Type == OrderType.CashForRATransfer || order.Type == OrderType.ReestrTransferOrder)
                                    creditAccount = dr["credit_bank_code"].ToString() + creditAccount;


                                if ((order.Type == OrderType.RATransfer && order.SubType == 3) || order.Type == OrderType.Convertation || order.Type == OrderType.CashDebit ||
                                    order.Type == OrderType.CashDebitConvertation || order.Type == OrderType.InBankConvertation || order.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                                    || order.Type == OrderType.TransitNonCashOut || order.Type == OrderType.ReestrTransferOrder)
                                {
                                    order.ReceiverAccount = Account.GetAccount(creditAccount);
                                }
                                else if (order.Type == OrderType.CashCredit || order.Type == OrderType.CashCreditConvertation || order.Type == OrderType.CashConvertation || order.Type == OrderType.CashTransitCurrencyExchangeOrder || order.Type == OrderType.TransitCashOutCurrencyExchangeOrder || order.Type == OrderType.TransitCashOut)
                                {
                                    order.ReceiverAccount = Account.GetSystemAccount(creditAccount);
                                }
                                else
                                {
                                    order.ReceiverAccount.AccountNumber = creditAccount;
                                    order.ReceiverAccount.OpenDate = default(DateTime?);
                                    order.ReceiverAccount.ClosingDate = default(DateTime?);
                                    order.ReceiverAccount.FreezeDate = default(DateTime?);
                                }
                            }

                            if (dr["receiver_name"] != DBNull.Value)
                                order.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                            if (dr["credit_bank_code"] != DBNull.Value)
                                order.ReceiverBankCode = Convert.ToInt32(dr["credit_bank_code"]);

                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();


                            order.SubType = Convert.ToByte(dr["document_subtype"]);

                            order.OrderNumber = dr["document_number"].ToString();

                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());


                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                            if (dr["receiver_name"] != DBNull.Value)
                                order.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());



                            if (dr["source_type"] != DBNull.Value)
                            {
                                order.Source = (SourceType)Convert.ToInt16(dr["source_type"]);
                            }
                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                            order.FilialCode = Convert.ToUInt16(dr["filial"].ToString());

                            if (dr["receiver_add_inf"] != DBNull.Value)
                                order.OtherDescription = Utility.ConvertAnsiToUnicode(dr["receiver_add_inf"].ToString());



                        }
                        else
                        {
                            order = null;
                        }
                    }
                }
            }
            return order;
        }

        internal static List<BillSplitSenderInfo> GetBillSplitSenders(long orderId = 0, int senderId = 0, Languages language = Languages.hy)
        {
            List<BillSplitSenderInfo> senders = new List<BillSplitSenderInfo>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    if (senderId == 0)
                    {
                        cmd.CommandText = @"Select * from Tbl_bill_split_senders where doc_id = @doc_id";
                        cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;

                    }
                    else
                    {
                        cmd.CommandText = @"Select * from Tbl_bill_split_senders where  id = @id ";

                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = senderId;


                    }


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        BillSplitSenderInfo sender = new BillSplitSenderInfo();

                        sender.Id = Convert.ToInt32(row["Id"]);
                        sender.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                        if(sender.CustomerNumber > 0)
                        {
                            sender.FullName = language == Languages.hy ? Utility.ConvertAnsiToUnicode(Utility.GetCustomerDescription(sender.CustomerNumber)) : Utility.GetCustomerDescriptionEnglish(sender.CustomerNumber);
                        }
                     
                        sender.Amount = Convert.ToDouble(row["amount"]);
                        sender.IsLinkPayment = Convert.ToBoolean(row["is_link_payment"]);
                        if (row["phone_number"] != DBNull.Value)
                            sender.PhoneNumber = row["phone_number"].ToString();
                        if (row["email"] != DBNull.Value)
                            sender.EmailAddress = row["email"].ToString();
                        sender.Status = Convert.ToInt16(row["status"]);
                        if (row["link_url"] != DBNull.Value)
                            sender.LinkURL = row["link_url"].ToString();
                        if (row["link_payment_doc_id"] != DBNull.Value)
                            sender.LinkPaymnentOrderId = Convert.ToUInt32(row["link_payment_doc_id"]);
                        sender.Status = Convert.ToInt16((row["status"]));

                        if (sender.Status == 0)
                        {
                            if (language == Languages.hy)
                            {
                                sender.StatusDescription = "Ընթացիկ";
                            }
                            else
                            {
                                sender.StatusDescription = "In progress";
                            }
                        }
                        else if (sender.Status == 1)
                        {
                            if (language == Languages.hy)
                            {
                                sender.StatusDescription = "Կատարված";
                            }
                            else
                            {
                                sender.StatusDescription = "Done";
                            }
                        }
                        else if (sender.Status == 40)
                        {
                            if (language == Languages.hy)
                            {
                                sender.StatusDescription = "Չեղարկված";
                            }
                            else
                            {
                                sender.StatusDescription = "Canceled";
                            }
                        }


                        senders.Add(sender);
                    }
                }
            }

            return senders;

        }

        internal static bool HasBillSplitSenderSentTransfer(int senderId)
        {
            bool hasTransfer = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                string str = @"select 1 from TBl_HB_Documents where document_type = 1 and document_subtype = 1 and cast (descr_for_payment as int) = @descr_for_payment and quality in (3,20,30)";


                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@descr_for_payment", SqlDbType.NVarChar).Value = senderId.ToString();


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hasTransfer = true;
                        }
                    }
                }

            }

            return hasTransfer;
        }


        internal static List<SentBillSplitRequest> GetSentBillSplitRequests(ulong customerNumber, Languages language)
        {
            List<SentBillSplitRequest> requests = new List<SentBillSplitRequest>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;


                    cmd.CommandText = @"Select D.doc_id, D.description, D.amount, D.registration_date, 
                        D.quality,D.currency,
                                            (select count(1) from TBl_bill_split_senders S where S.doc_id = D.doc_id) as sendersCount,
                                            (select count(1) from TBl_bill_split_senders S where S.doc_id = D.doc_id and status = 1) as completedSendersCount
                                            from Tbl_HB_Documents D where D.customer_number = @customer_number  and D.document_Type = 245 and D.quality in (3,30)";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        SentBillSplitRequest request = new SentBillSplitRequest();

                        request.OrderId = Convert.ToInt32(row["doc_id"]);
                        request.Amount = Convert.ToDouble(row["amount"]);
                        request.Description = row["description"].ToString();
                        request.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                        request.SendersCount = Convert.ToInt32(row["sendersCount"]);
                        request.CompletedSendersCount = Convert.ToInt32(row["completedSendersCount"]);
                        request.Currency = row["currency"].ToString();

                        if ((OrderQuality)Convert.ToInt16(row["quality"]) == OrderQuality.Sent3)
                        {
                            request.Status = 0;
                            if (language == Languages.hy)
                            {
                                request.StatusDescription = "Ընթացիկ";
                            }
                            else
                            {
                                request.StatusDescription = "In progress";
                            }
                        }

                        else
                        {
                            request.Status = 1;
                            if (language == Languages.hy)
                            {
                                request.StatusDescription = "Կատարված";
                            }
                            else
                            {
                                request.StatusDescription = "Completed";
                            }
                        }
                        requests.Add(request);
                    }



                }
            }

            return requests;

        }

        internal static List<ReceivedBillSplitRequest> GetReceivedBillSplitRequests(ulong customerNumber, Languages language)
        {
            List<ReceivedBillSplitRequest> requests = new List<ReceivedBillSplitRequest>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;


                    cmd.CommandText = @"select D.[description], d.amount, d.currency, d.registration_date, s.[status], s.id, D.receiver_name, D.credit_account, D.credit_bank_code
											from Tbl_BIll_split_senders S inner join TBl_HB_Documents D on S.doc_id = D.doc_id
											where S.customer_number = @customer_number and status in (0,1) and D.quality in (3,30)";

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];
                        ReceivedBillSplitRequest request = new ReceivedBillSplitRequest();

                        request.SenderId = Convert.ToInt32(row["id"]);
                        request.Amount = Convert.ToDouble(row["amount"]);
                        request.Description = row["description"].ToString();
                        request.RegistrationDate = Convert.ToDateTime(row["registration_date"]);

                        request.ReceiverAccount = new Account();
                        if (row["credit_account"] != DBNull.Value)
                        {
                            string creditAccount = row["credit_account"].ToString();
                            creditAccount = row["credit_bank_code"].ToString() + creditAccount;
                            request.ReceiverAccount.AccountNumber = creditAccount;
                            request.ReceiverAccount.OpenDate = default(DateTime?);
                            request.ReceiverAccount.ClosingDate = default(DateTime?);
                            request.ReceiverAccount.FreezeDate = default(DateTime?);

                        }


                        request.Currency = row["currency"].ToString();

                        if (row["receiver_name"] != DBNull.Value)
                            request.Receiver = Utility.ConvertAnsiToUnicode(row["receiver_name"].ToString());

                        request.Status = Convert.ToInt16(row["status"]);
                        if (request.Status == 0)
                        {
                            if (language == Languages.hy)
                            {
                                request.StatusDescription = "Նոր";
                            }
                            else
                            {
                                request.StatusDescription = "New";
                            }
                        }

                        else
                        {
                            if (language == Languages.hy)
                            {
                                request.StatusDescription = "Կատարված";
                            }
                            else
                            {
                                request.StatusDescription = "Completed";
                            }
                        }
                        requests.Add(request);
                    }



                }
            }

            return requests;

        }

        internal static SentBillSplitRequest GetSentBillSplitRequest(ulong customerNumber, long orderId, Languages language)
        {
            SentBillSplitRequest request = new SentBillSplitRequest();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select doc_id, [description], amount, currency, registration_date, receiver_name, credit_account, credit_bank_code, receiver_add_inf
											from TBl_HB_Documents 
											where doc_id=@id and customer_number=case when @customerNumber = 0 then customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            request.OrderId = long.Parse(dr["doc_id"].ToString());



                            request.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);



                            request.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();


                                creditAccount = dr["credit_bank_code"].ToString() + creditAccount;



                                request.ReceiverAccount.AccountNumber = creditAccount;
                                request.ReceiverAccount.OpenDate = default(DateTime?);
                                request.ReceiverAccount.ClosingDate = default(DateTime?);
                                request.ReceiverAccount.FreezeDate = default(DateTime?);

                            }



                            if (dr["amount"] != DBNull.Value)
                                request.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                request.Currency = dr["currency"].ToString();

                            if (dr["description"] != DBNull.Value)
                                request.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());


                            if (dr["receiver_add_inf"] != DBNull.Value)
                                request.OtherDescription = Utility.ConvertAnsiToUnicode(dr["receiver_add_inf"].ToString());



                        }
                        else
                        {
                            request = null;
                        }
                    }
                }
            }
            return request;
        }


        internal static void SendBillSplitSendersNotifications(long orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_send_bill_split_sender_notifications";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        internal static ReceivedBillSplitRequest GetReceivedBillSplitRequest(int billSplitSenderId)
        {
            ReceivedBillSplitRequest request = new ReceivedBillSplitRequest();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;


                    cmd.CommandText = @"select D.[description], d.amount, d.currency, d.registration_date, s.[status], s.id, D.receiver_name, D.credit_account, D.credit_bank_code
											from Tbl_BIll_split_senders S inner join TBl_HB_Documents D on S.doc_id = D.doc_id
											where S.id = @sender_id";

                    cmd.Parameters.Add("@sender_id", SqlDbType.Float).Value = billSplitSenderId;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {


                            request.SenderId = Convert.ToInt32(dr["id"]);
                            request.Amount = Convert.ToDouble(dr["amount"]);
                            request.Description = dr["description"].ToString();
                            request.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            request.ReceiverAccount = new Account();
                            if (dr["credit_account"] != DBNull.Value)
                            {
                                string creditAccount = dr["credit_account"].ToString();
                                creditAccount = dr["credit_bank_code"].ToString() + creditAccount;
                                request.ReceiverAccount.AccountNumber = creditAccount;
                                request.ReceiverAccount.OpenDate = default(DateTime?);
                                request.ReceiverAccount.ClosingDate = default(DateTime?);
                                request.ReceiverAccount.FreezeDate = default(DateTime?);

                            }


                            request.Currency = dr["currency"].ToString();

                            if (dr["receiver_name"] != DBNull.Value)
                                request.Receiver = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());

                            request.Status = Convert.ToInt16(dr["status"]);

                        }

                    }

                }
            }

            return request;

        }
    }
}
