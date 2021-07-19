using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.XBManagement;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class XBUserGroupDB
    {
        internal static ActionResult Save(XBUserGroup group, long orderId, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Submit_User_Group";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@group_name", SqlDbType.NVarChar, 250).Value = group.GroupName;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    param.Value = group.Id;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                 
                    cmd.ExecuteNonQuery();

                    id = int.Parse(cmd.Parameters["@id"].Value.ToString());

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

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

        internal static ActionResult AddHBUserIntoGroup(XBUserGroup group, HBUser user, Action action, long orderId)
        {
            ActionResult result = new ActionResult();
            int id = -1;
           

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
               
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_insert_access_group_member";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = user.ID;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;

                    SqlParameter param = new SqlParameter("@Id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                 
                    cmd.ExecuteNonQuery();

                    id = int.Parse(cmd.Parameters["@Id"].Value.ToString());

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

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

        internal static ActionResult RemoveGroup(XBUserGroup group, long doc_id)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Remove_Group";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = doc_id;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = Action.Delete;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                  
                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }
                }
            }

            //if(BelongsToSchema(group))
            //{
            //    RemoveGroupFromSchemaDetails(group);
            //}
            return result;
        }

        internal static bool BelongsToSchema(XBUserGroup group)
        {
            bool belongs = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT group_id FROM Tbl_Approvement_Schema_Details_order_Details WHERE group_id = @group_id", conn))
                {
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        belongs = true;
                    }
                }
            }
            return belongs;
        }

        internal static void RemoveGroupFromSchemaDetails(XBUserGroup group, int doc_id)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_remove_group_from_schema_details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = doc_id;
                    cmd.Parameters.Add("@action", SqlDbType.Int).Value = Action.Delete;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static ActionResult RemoveHBUserFromGroup(XBUserGroup group, HBUser user, long orderid)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandText = "pr_Remove_Access_Group_Member";
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = user.ID;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderid;

                    cmd.Parameters.Add("@action", SqlDbType.Int).Value = Action.Delete;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);        

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }
                }
            }
        
            return result;
        }

        internal static bool BelongsUserToGroup(XBUserGroup group, HBUser user)
        {
            bool belongs = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id FROM Tbl_Access_Group_Members WHERE user_id = @id and group_id = @group_id", conn))
                {
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = group.Id;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = user.ID;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        belongs = true;
                    }
                }
            }
            return belongs;
        }

        internal static List<XBUserGroup> GetXBUserGroups(ulong customerNumber)
        {
            List<XBUserGroup> groupList = new List<XBUserGroup>();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT AG.id,AG.group_name FROM Tbl_Access_Groups AG INNER JOIN Tbl_Applications GC on AG.global_cust_id = GC.id                                        
                                             WHERE GC.customer_number = @customerNumber Order by group_name asc";

                SqlCommand cmd = new SqlCommand(script, conn);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    XBUserGroup group = new XBUserGroup();
                    group.Id = Convert.ToInt32(row["id"]);
                    group.GroupName = row["group_name"].ToString();

                    groupList.Add(group);
                }
            }
           
            return groupList;
        }

        internal static List<HBUser> GetHBUsersByGroup(int groupId)
        {
            List<HBUser> userList = new List<HBUser>();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT M.user_id FROM Tbl_Access_Group_Members M inner join Tbl_Access_Groups G on M.group_id = G.id                                        
                                             WHERE G.id = @groupId";

                SqlCommand cmd = new SqlCommand(script, conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@groupId", SqlDbType.Int).Value = groupId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    HBUser user = new HBUser();

                    user.ID = Convert.ToInt32(row["user_id"]);
                  
                   
                    userList.Add(user);
                }
            }
            return userList;
        }

        internal static XBUserGroup GetXBUserGroup(int id)
        {
            XBUserGroup group = new XBUserGroup();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT id,group_name FROM Tbl_Access_Groups WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        group.Id = Convert.ToInt32(row["id"]);
                        group.GroupName = row["group_name"].ToString();

                    }
                }
  
            }

            return group;
        }

        internal static bool ExistsXBUserGroupWithName(int hbAppId, string groupName)
        {
            bool exists = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id FROM Tbl_Access_Groups WHERE global_cust_id = @appId and group_name = @groupName", conn))
                {
                    cmd.Parameters.Add("@groupName", SqlDbType.NVarChar, 250).Value = groupName;
                    cmd.Parameters.Add("@appId", SqlDbType.Int).Value = hbAppId;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        exists = true;
                    }
                }
            }
            return exists;
        }

        public static string GenerateNextGroupName(ulong customerNumber)
        {
            string groupName = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT isnull(max (cast(substring(group_name, 7, LEN(group_name) - 6) as int)),0) + 1 as NextGroupNumber FROM Tbl_HB_documents 
                                                                 WHERE customer_number =@customerNumber", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    groupName = "Խումբ" + dr["NextGroupNumber"].ToString();
                }

            }

            return groupName;
        }

        internal static List<XBUserGroup> GetXBUserGroupsByOrder(long docId)
        {
            List<XBUserGroup> groupList = new List<XBUserGroup>();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT group_id,group_name FROM Tbl_Access_Groups_Order_Details                                    
                           WHERE doc_id = @doc_id and action = 1";

                SqlCommand cmd = new SqlCommand(script, conn);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    XBUserGroup group = new XBUserGroup();
                    group.Id = Convert.ToInt32(row["group_id"]);
                    group.GroupName = row["group_name"].ToString();

                    groupList.Add(group);
                }
            }

            return groupList;
        }

        internal static List<HBUser> GetHBUsersByGroupByOrder(int groupId, long docId)
        {
            List<HBUser> userList = new List<HBUser>();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT user_id FROM Tbl_Access_Group_Members_Order_Details                                        
                                             WHERE group_id = @groupId and doc_id = @doc_id and action = 1";

                SqlCommand cmd = new SqlCommand(script, conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@groupId", SqlDbType.Int).Value = groupId;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    HBUser user = new HBUser();

                    user.ID = Convert.ToInt32(row["user_id"]);


                    userList.Add(user);
                }
            }
            return userList;
        }


        internal static List<XBUserGroup> GetXBUserGroups(string userName)
        {
            List<XBUserGroup> groupList = new List<XBUserGroup>();
            string script = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                script = @"SELECT m.group_id,g.group_name 
                                  FROM Tbl_Access_Group_Members m
                                  INNER JOIN Tbl_Users u
                                  ON u.id=m.user_id
                                  INNER JOIN Tbl_Access_Groups G
                                  ON M.group_id = G.id
                                  WHERE u.user_name=@userName";

                SqlCommand cmd = new SqlCommand(script, conn);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userName", SqlDbType.NVarChar).Value = userName;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    XBUserGroup group = new XBUserGroup();
                    group.Id = Convert.ToInt32(row["group_id"]);
                    group.GroupName = row["group_name"].ToString();

                    groupList.Add(group);
                }
            }

            return groupList;
        }


    }
}
