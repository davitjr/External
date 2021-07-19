using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class AccountReOpenOrderDB
    {
        /// <summary>
        /// Վերադարձնում է հաշվի վերաբացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static AccountReOpenOrder GetAccountReOpenOrder(AccountReOpenOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select d.amount, d.currency, d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,d.debet_account,                                               
		                                                isnull(n.account_type,1) account_type,
                                                        isnull(statement_type,-1)  as statement_type,
                                                        isnull(n.fee_charge_type ,0) as fee_charge_type,isnull(reopen_reason,'') as reopen_reason,operation_date,d.order_group_id,d.confirmation_date
                                                  from Tbl_HB_documents as d left join Tbl_New_Account_Documents as n on  d.doc_ID=n.Doc_ID
                                                  where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end",
                    conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.AccountType = Convert.ToUInt16(dt.Rows[0]["account_type"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.StatementDeliveryType = short.Parse(dt.Rows[0]["statement_type"].ToString());
                    order.FeeChargeType = ushort.Parse(dt.Rows[0]["fee_charge_type"].ToString());
                    order.ReopenReasonDescription = dt.Rows[0]["reopen_reason"].ToString();
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"SELECT account_number FROM Tbl_account_order_selected_accounts WHERE Doc_ID=@Doc_ID";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;

                    order.ReOpeningAccounts = new List<Account>();
                    dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        order.ReOpeningAccounts = new List<Account>();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            Account currentAccount = AccountDB.GetAccount(row["account_number"].ToString());
                            order.ReOpeningAccounts.Add(currentAccount);
                        }
                    }
                }

                   
            }
            return order;
        }


        /// <summary>
        /// Հաշվի վերաբացման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondReOpen(ulong customerNumber, string accountNumber)
        {
            bool secondReOpen;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=12 and document_subtype=1 and
                                                debet_account=@accountNumber and customer_number=@customerNumber",conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (cmd.ExecuteReader().Read())
                    {
                        secondReOpen = true;
                    }
                    else
                        secondReOpen = false;
                }
                   
            }
            return secondReOpen;
        }

        /// <summary>
        /// Հաշվի վերաբացման թույլատրում:Հաշվի սինթետիկ հաշվի համապատասխանություն հաճախորդի կարգավիճակին:
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool CheckSintAcc(ulong customerNumber, string accountNumber)
        {
            bool canReOpen;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select a.arm_number from Tbl_Customers c
								                                        inner join [tbl_all_accounts;] a 
					                                    on c.customer_number = a.customer_number
								                                    inner join Tbl_define_sint_acc d
					                                    on d.sint_acc = a.type_of_account 
			                                    where d.type_of_client = c.type_of_client 
			                                    and d.sitizen=c.residence 
			                                    and d.link =case c.link when 3 then 1 else c.link end 
			                                    and Arm_number = @accountNumber and c.customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (cmd.ExecuteReader().Read())
                    {
                        canReOpen = true;
                    }
                    else
                    {
                        canReOpen = false;
                    }
                }
            }

            return canReOpen;
        }

        /// <summary>
        /// Հաշվի վերաբացման թույլատրում:Հաշվի նոր սինթետիկ հաշվի համապատասխանություն հաճախորդի կարգավիճակին:
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool CheckNewSintAcc(ulong customerNumber, string accountNumber)
        {
            bool canReOpen;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select a.arm_number from Tbl_Customers c
								                                        inner join [tbl_all_accounts;] a 
					                                    on c.customer_number = a.customer_number
								                                    inner join Tbl_define_sint_acc d
					                                    on d.sint_acc_new = a.type_of_account_new 
			                                    where d.type_of_client = c.type_of_client 
			                                    and d.sitizen=c.residence 
			                                    and d.link =case c.link when 3 then 1 else c.link end 
			                                    and Arm_number = @accountNumber and c.customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    if (cmd.ExecuteReader().Read())
                    {
                        canReOpen = true;
                    }
                    else
                    {
                        canReOpen = false;
                    }
                }
            }

            return canReOpen;
        }

    }
}
