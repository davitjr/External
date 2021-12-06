using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class StatmentByEmailOrderDB
    {  
        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(StatmentByEmailOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            DataTable dt = GetAccountDataTable(order.Accounts);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNewApplicationForStatment";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@MainEmail", SqlDbType.NVarChar, 50).Value = order.MainEmail;
                    cmd.Parameters.Add("@SecondaryEmail", SqlDbType.NVarChar, 50).Value = order.SecondaryEmail;
                    cmd.Parameters.Add("@Frequency", SqlDbType.Float).Value = order.Periodicity;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter prm = new SqlParameter("@dt", SqlDbType.Structured);
                    prm.Value = dt;
                    prm.TypeName = "dbo.Reference_accounts";
                    cmd.Parameters.Add(prm);
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = order.Id;

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
        /// <summary>
        /// Վերադարձնում է քաղվածքների էլեկտրոնային ստացման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static StatmentByEmailOrder Get(StatmentByEmailOrder order)
        {
           using DataTable dt = new DataTable();
            order.Accounts = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT a.MainEmail,
                                                         a.SecondaryEmail,
                                                         a.Frequency,
                                                         a.AccountNumber,
                                                         b.document_number,
                                                         b.customer_number,
                                                         b.registration_date,
                                                         b.document_type,
                                                         b.quality,
                                                         b.document_subtype,
                                                         b.operation_date,
                                                         b.order_group_id,
                                                         b.confirmation_date
                                                         FROM Tbl_Application_For_Statment AS a
                                                         INNER JOIN Tbl_HB_documents AS b
                                                         ON a.Doc_id=b.doc_ID
                                                         WHERE b.customer_number=case when @customer_number = 0 then b.customer_number else @customer_number end and a.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count>0)
                {
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.MainEmail = dt.Rows[0]["MainEmail"].ToString();
                    order.SecondaryEmail = dt.Rows[0]["SecondaryEmail"].ToString();
                    order.Periodicity = Convert.ToInt32(dt.Rows[0]["Frequency"]);
                    order.OrderNumber = Convert.ToString(dt.Rows[0]["document_number"]);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Account account = Account.GetAccount(Convert.ToUInt64(dt.Rows[i]["AccountNumber"]));
                        account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(account.AccountTypeDescription);
                        order.Accounts.Add(account);
                    }
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

                }
            }
            return order;
        }
        /// <summary>
        /// Որոշում է հաշվի տեսակը և ավելացնում Datatable-ի մեջ
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        internal static DataTable GetAccountDataTable(List<Account> account)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("type");
            dt.Columns.Add("uID");
            dt.Columns.Add("status");
            dt.Columns.Add("acc_number");

            int i = 1;
            foreach (Account a in account)
            {
                Account accounts = Account.GetAccount(a.AccountNumber);
                a.AccountType = accounts.AccountType;
                if (a.AccountType == 11)
                {
                    dt.Rows.Add("3", i, "1", a.AccountNumber);
                    i++;
                }
                if (a.AccountType == 10)
                {
                    dt.Rows.Add("1", i, "1", a.AccountNumber);
                    i++;
                }
                if (a.AccountType == 13)
                {
                    dt.Rows.Add("2", i, "1", a.AccountNumber);
                    i++;
                }

            }
            return dt;
        }
    }
}
