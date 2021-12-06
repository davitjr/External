using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    internal class OrderGroupDB
    {
        /// <summary>
        /// Պահպանում է ծառայությունների խումբը
        /// </summary>
        /// <param name="orderGroup"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static ActionResult SubmitOrderGroup(OrderGroup orderGroup, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_order_group";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@group_name", SqlDbType.NVarChar).Value = action != Action.Delete ? orderGroup.GroupName : "";
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = orderGroup.CustomerNumber;
                    cmd.Parameters.Add("@group_id", SqlDbType.Float).Value = orderGroup.ID;
                    cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = orderGroup.Status;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = orderGroup.Type;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = orderGroup.UserId;


                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = orderGroup.ID });

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                    orderGroup.ID = id;

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
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
        /// Վերադարձնում է տվյալ հաճախորդի ծառայությունների խումբը՝ ըստ ունիկալ համարի
        /// </summary>
        /// <param name="transactionGroup"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static OrderGroup GetOrderGroupById(int id, ulong customerNumber)
        {
            OrderGroup group = null;


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select * from Tbl_Order_Groups where id=@groupId and customer_number=@customerNumber", conn))
                {
                    cmd.Parameters.Add("@groupId", SqlDbType.NVarChar).Value = id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        group = SetOrderGroup(row);
                    }

                }

            }

            return group;
        }

        /// <summary>
        /// Ինիցիալիզացնում է ծառայությունների խումբը
        /// </summary>
        /// <param name="row"></param>
        /// <param name="group"></param>
        internal static OrderGroup SetOrderGroup(DataRow row)
        {
            OrderGroup group = new OrderGroup();
            group.GroupName = row["description"].ToString();
            group.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
            group.ID = Convert.ToInt32(row["id"].ToString());
            group.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
            group.Status = (OrderGroupStatus)(row["status"]);
            group.Type = (OrderGroupType)(row["type"]);
            group.UserId = Convert.ToInt32(row["user_id"].ToString());

            return group;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ծառայությունների խմբերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<OrderGroup> GetOrderGroups(ulong customerNumber, OrderGroupStatus status, OrderGroupType groupType)
        {
            List<OrderGroup> groups = new List<OrderGroup>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                string sql = "";


                sql = @" SELECT * FROM Tbl_Order_Groups WHERE customer_number = @customerNumber AND status = @status AND type=@type";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = status;
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = groupType;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        groups = new List<OrderGroup>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        OrderGroup group = SetOrderGroup(row);

                        groups.Add(group);
                    }
                }

            }
            return groups;
        }

        internal static bool CheckGroupId(int groupId)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.CommandText = @"SELECT TOP 1 * FROM Tbl_Order_Groups WHERE id = @groupId";
            cmd.Connection = conn;

            cmd.Parameters.Add("@groupId", SqlDbType.Int).Value = groupId;
            using SqlDataReader dr = cmd.ExecuteReader();
            return dr.Read();
        }

        internal static void SaveGroupTemplateShortInfo(GroupTemplateShrotInfo info, long templateId , Action action)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_insert_group_templates_short_info";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 20).Value = info.ReceiverAccount;
            cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 100).Value = info.ReceiverName;
            cmd.Parameters.Add("@debit_account", SqlDbType.NVarChar, 20).Value = info.DebitAccount;
            cmd.Parameters.Add("@fee_Account", SqlDbType.NVarChar, 20).Value = info.FeeAccount;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = info.Amount;
            cmd.Parameters.Add("@customer_id", SqlDbType.NVarChar, 50).Value = info.CustomerId;
            cmd.Parameters.Add("@loan_app_id", SqlDbType.BigInt).Value = (long)info.LoanAppId;
            cmd.Parameters.Add("@group_id", SqlDbType.Float).Value = info.GroupId;
            cmd.Parameters.Add("@template_id", SqlDbType.Int).Value = templateId;
            cmd.Parameters.Add("@percent_account", SqlDbType.NVarChar, 50).Value = info.PercentAccount;
            cmd.Parameters.Add("@action", SqlDbType.Bit).Value = action == Action.Add ? 0 : 1;// 0 ձևանմուշի ստեղծման համար, 1 փոփոխման համար
            cmd.ExecuteNonQuery();


        }
    }
}
