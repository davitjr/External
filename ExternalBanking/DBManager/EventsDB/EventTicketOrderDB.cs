using ExternalBanking.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager.EventsDB
{
    internal class EventTicketOrderDB
    {
        internal static ActionResult Save(EventTicketOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_submit_event_ticket_order";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;

            string creditAccountNumber;
         
              
            creditAccountNumber = order.ReceiverAccount.AccountNumber.ToString().Substring(5);
                      
            cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = creditAccountNumber;

            cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
            cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description;
            cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 250).Value = order.Receiver;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
            cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar, 5).Value = order.ReceiverBankCode;


            SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(param);

            param = new SqlParameter("@id", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(param);

            if (order.Id != 0)
            {
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
            }

     
            cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
            cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
            cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = order.UseCreditLine;

            cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = order.Quantity;
            cmd.Parameters.Add("@event_type_id", SqlDbType.Int).Value = (int)order.Event.Id;
            cmd.Parameters.Add("@event_sub_type_id", SqlDbType.Int).Value = order.Event.SubTypeId;
            cmd.Parameters.Add("@non_discounted_amount", SqlDbType.Float).Value = order.NonDiscountedAmount;
            

            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

          
            cmd.ExecuteNonQuery();

            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            short actionResult = Convert.ToInt16(cmd.Parameters["@result"].Value);

            if (actionResult == 1)
            {
                result.ResultCode = ResultCode.Normal;
                result.Id = order.Id;
               
            }
            else if (actionResult == 0)
            {
                result.ResultCode = ResultCode.Failed;
                result.Id = -1;
                result.Errors.Add(new ActionError(actionResult));
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
                result.Id = -1;
                result.Errors.Add(new ActionError(actionResult));
            }

            return result;
        }

        internal static EventTicketOrder GetEventTicketOrder(EventTicketOrder order, Languages Language)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.debet_account,d.credit_account,d.credit_bank_code,d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,d.currency, n.*,d.operation_date,d.order_group_id,d.confirmation_date,reject_id,d.use_credit_line,d.filial,d.description,d.receiver_name
		                                           FROM Tbl_HB_documents as d inner join Tbl_event_ticket_order_details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.Id = long.Parse(dt.Rows[0]["doc_id"].ToString());
                order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                string creditAccountNumber = dt.Rows[0]["credit_bank_code"].ToString() + dt.Rows[0]["credit_account"].ToString();
                order.ReceiverAccount = Account.GetAccount(creditAccountNumber);
                order.DebitAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());

                order.Quantity = int.Parse(dt.Rows[0]["quantity"].ToString());
                order.NonDiscountedAmount= Convert.ToDouble(dt.Rows[0]["non_discounted_amount"]);

                int _id = int.Parse(dt.Rows[0]["event_type_id"].ToString());
                int _sub_id = int.Parse(dt.Rows[0]["event_sub_type_id"].ToString());

                order.Event = Events.Events.GetEventSubTypes((EventTypes)_id, Language).Where(e => e.SubTypeId == _sub_id).First();
                order.Description = dt.Rows[0]["description"].ToString(); 
                order.Receiver = dt.Rows[0]["receiver_name"].ToString();

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"].ToString());
                order.UseCreditLine = dt.Rows[0]["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["use_credit_line"]) : false;
                order.ReceiverBankCode = Convert.ToInt32(dt.Rows[0]["credit_bank_code"]);

            }
            return order;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ուղարկած հայտերի քանակը տվյալ միջոցառման ենթատեսակի համար
        /// </summary>
        /// <param name="eventSubTypeId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static int GetCustomerEventTicketsCount(int eventSubTypeId, ulong customerNumber, DateTime eventStartDate)
        {
            int ticketsCount = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
               
                string str = @"select SUM(E.quantity) as orderCount from TBl_HB_Documents D inner join Tbl_event_ticket_order_details E on D.doc_id = E.doc_id where customer_number = @customerNumber and document_type = 266 and quality in (3, 30, 20) and  event_sub_type_id = @eventSubTypeId and d.registration_date >= @eventStartDate";


                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@eventSubTypeId", SqlDbType.Int).Value = eventSubTypeId;
                    cmd.Parameters.Add("@eventStartDate", SqlDbType.DateTime).Value = eventStartDate;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ticketsCount = dr["orderCount"] != DBNull.Value ? Convert.ToInt32(dr["orderCount"]) : 0;
                        }
                    }
                }

            }

            return ticketsCount;
        }
    }
}
