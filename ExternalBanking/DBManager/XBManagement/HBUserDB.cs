using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using ExternalBanking.XBManagement;

namespace ExternalBanking.DBManager
{
    class HBUserDB
    {
        /// <summary>
        /// Վերադարձնում է նշված օգտագործողին
        /// </summary>
        /// <param name="hbUserID"></param>
        /// <returns></returns>
        internal static HBUser GetHBUser(int hbUserID)
        {
            HBUser HBUser = new HBUser();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT * FROM [v_Users] WHERE  Id = @HBUserID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@HBUserID", SqlDbType.Int).Value = hbUserID;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    HBUser = SetHBUser(row);
                }
                else
                {
                    HBUser = null;
                }

            }
            return HBUser;
        }

        /// <summary>
        /// Վերադարձնում է նշված օգտագործողին 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static HBUser GetHBUserByUserName(string userName)
        {
            HBUser HBUser = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"	SELECT U.Id, U.user_name, A.customer_number, U.is_cas  from Tbl_Applications A
                                        INNER JOIN tbl_users U on a.id = u.global_id WHERE  user_name = @HBUserName";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@HBUserName", SqlDbType.NVarChar).Value = userName;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            HBUser = new HBUser();
                            while (dr.Read())
                            {
             
                                HBUser.ID = Convert.ToInt32(dr["ID"].ToString());
                                HBUser.CustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());
                                HBUser.UserName = dr["user_name"].ToString();
                                HBUser.IsCas = dr["is_cas"] != DBNull.Value ? Convert.ToBoolean(dr["is_cas"].ToString()) : default;
                            }
                        }
                    }
                }
            }
            return HBUser;
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static HBUser SetHBUser(DataRow row)
        {
            HBUser HBUser = new HBUser();

            if (row != null)
            {
                HBUser.ID = Convert.ToInt32(row["ID"].ToString());
                HBUser.HBAppID = Convert.ToInt32(row["global_id"].ToString());
                HBUser.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                HBUser.UserName = row["user_name"].ToString();
                HBUser.RegistrationDate = row["date_of_registration"] != DBNull.Value ? Convert.ToDateTime(row["date_of_registration"].ToString()) : default(DateTime?);
                HBUser.IsBlocked = row["blocked"] != DBNull.Value ? Convert.ToBoolean(row["blocked"].ToString()) : default;
                HBUser.IsLocked = row["locked"] != DBNull.Value ? Convert.ToBoolean(row["locked"].ToString()) : default;
                HBUser.BlockingSetID = row["blocking_set_id"] != DBNull.Value ? Convert.ToInt32(row["blocking_set_id"]) : default;
                HBUser.BlockingDate = row["blocking_date"] != DBNull.Value ? Convert.ToDateTime(row["blocking_date"]) : default(DateTime?);
                HBUser.IdentificationPerOrder = Convert.ToBoolean(row["identification_per_order"].ToString());
                HBUser.LimitedAccess = Convert.ToBoolean(row["limited_access"].ToString());
                HBUser.Email.email.emailAddress = row["email"] != DBNull.Value ? row["email"].ToString() : default;
                HBUser.Email.email.id = row["emailId"] != DBNull.Value ? Convert.ToUInt32(row["emailId"].ToString()) : default(int);
                HBUser.Email.id = row["emailId"] != DBNull.Value ? Convert.ToUInt32(row["emailId"].ToString()) : default(int);
                HBUser.AllowDataEntry = row["allow_entry_data"] != DBNull.Value ? Convert.ToBoolean(row["allow_entry_data"]) : false;
                HBUser.PassChangeReq = row["pwd_change_requirement"] != DBNull.Value ? Convert.ToBoolean(row["pwd_change_requirement"].ToString()) : default;
                HBUser.UserFullNameEng = row["user_full_name_eng"] != DBNull.Value ? row["user_full_name_eng"].ToString() : default;
                HBUser.LogonInformation.GoodLogons = row["good_logons"] != DBNull.Value ? Convert.ToInt32(row["good_logons"].ToString()) : default;
                HBUser.LogonInformation.BadLogons = row["bad_logons"] != DBNull.Value ? Convert.ToInt32(row["bad_logons"].ToString()) : default;
                HBUser.LogonInformation.LastGoodLogonDate = row["last_good_logon_date"] != DBNull.Value ? Convert.ToDateTime(row["last_good_logon_date"]) : default;
                HBUser.LogonInformation.LastBadLogonDate = row["last_bad_logon_date"] != DBNull.Value ? Convert.ToDateTime(row["last_bad_logon_date"]) : default;
                HBUser.LogonInformation.BadResetCounter = row["bad_reset_counter"] != DBNull.Value ? Convert.ToInt16(row["bad_reset_counter"]) : default;
                HBUser.IsCas = row["is_cas"] != DBNull.Value ? Convert.ToBoolean(row["is_cas"].ToString()) : default;
            }
            return HBUser;
        }

        /// <summary>
        /// Վերադարձնում է պայմանագրի բոլոր գործող ՀԲ օգտագործողնորին
        /// </summary>
        /// <param name="hbAppID"></param>
        /// <returns></returns>
        internal static List<HBUser> GetHBUsers(int hbAppID)
        {
            List<HBUser> HBUsers = new List<HBUser>();
            HBUser HBUser = new HBUser();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT * FROM [v_Users] WHERE global_id = @hbAppID and isnull(blocked,0)=0";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@hbAppID", SqlDbType.Int).Value = hbAppID;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    HBUser = SetHBUser(row);
                    HBUsers.Add(HBUser);
                }
            }
            return HBUsers;
        }

        /// <summary>
        /// Վերադարձնում է պայմանագրի բոլոր փակված ՀԲ օգտագործողնորին
        /// </summary>
        /// <param name="HBGlobalID"></param>
        /// <returns></returns>
        internal static List<HBUser> GetClosedHBUsers(int HBGlobalID)
        {
            List<HBUser> HBUsers = new List<HBUser>();
            HBUser HBUser = new HBUser();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT * FROM [v_Users] WHERE global_id = @HBGlobalID and isnull(blocked,0)=1";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@HBGlobalID", SqlDbType.Int).Value = HBGlobalID;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                foreach (DataRow row in dt.Rows)
                {
                    HBUser = SetHBUser(row);
                    HBUsers.Add(HBUser);
                }
            }
            return HBUsers;
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի հաշիվների/պրոդուկտների հասանելիություն
        /// </summary>
        /// <param name="hbUser"></param>
        internal static void GetHBUserProductsPermissions(HBUser hbUser)
        {
            HBProductPermission permission = new HBProductPermission();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT * FROM [Tbl_Account_Permissions] where user_Id=@hbUserId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@hbUserId", SqlDbType.Int).Value = hbUser.ID;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                hbUser.ProductsPermissions = new List<HBProductPermission>();
                foreach (DataRow row in dt.Rows)
                {
                    permission = SetHBUserProductsPermission(row);
                    hbUser.ProductsPermissions.Add(permission);
                }
            }
        }

        private static HBProductPermission SetHBUserProductsPermission(DataRow row)
        {
            HBProductPermission hbProductPermission = new HBProductPermission();

            if (row != null)
            {
                hbProductPermission.ProductAppID = Convert.ToUInt64(row["app_id"].ToString());
                hbProductPermission.ProductAccountNumber = row["arm_number"].ToString();
                hbProductPermission.ProductType = (HBProductPermissionType)Convert.ToInt16(row["product_type"].ToString());
                hbProductPermission.IsActive = Convert.ToBoolean(row["is_active"].ToString());
            }
            return hbProductPermission;
        }

        private static DataTable ConvertProductPermissionsToDataTable(List<HBProductPermission> productPermissions)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("productType");
            dt.Columns.Add("productAccountNumber");
            dt.Columns.Add("productAppID");
            dt.Columns.Add("isActive");
            if (productPermissions != null)
            {

                foreach (HBProductPermission p in productPermissions)
                {
                    DataRow dr = dt.NewRow();
                    dr["productType"] = (short)p.ProductType;
                    dr["productAccountNumber"] = p.ProductAccountNumber;
                    dr["productAppID"] = p.ProductAppID;
                    dr["isActive"] = p.IsActive;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի պահմապնում
        /// </summary>
        /// <param name="hbUser"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static void Save(int userID, SourceType source, HBUser hbUser, long docId, Action action)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBUser_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@global_id", SqlDbType.Int).Value = hbUser.HBAppID;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docId;
                    cmd.Parameters.Add("@userSetNumber", SqlDbType.Int).Value = userID;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = hbUser.CustomerNumber;
                    cmd.Parameters.Add("@hb_user_customer_number", SqlDbType.Float).Value = hbUser.CustomerNumber == 0 ? hbUser.CustomerNumber : hbUser.CustomerNumber;
                    cmd.Parameters.Add("@hb_user_name", SqlDbType.NVarChar, 24).Value = hbUser.UserName;
                    cmd.Parameters.Add("@identification_per_order", SqlDbType.Bit).Value = hbUser.IdentificationPerOrder;
                    cmd.Parameters.Add("@limited_access", SqlDbType.Bit).Value = hbUser.LimitedAccess;
                    cmd.Parameters.Add("@allow_entry_data", SqlDbType.TinyInt).Value = hbUser.AllowDataEntry;
                    cmd.Parameters.Add("@emailID", SqlDbType.Int).Value = hbUser.Email != null ? hbUser.Email.id : 0;
                    cmd.Parameters.Add("@productPermissions", SqlDbType.Structured).Value = ConvertProductPermissionsToDataTable(hbUser.ProductsPermissions);
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@hb_user_ID", SqlDbType.Int).Value = hbUser.ID;
                    cmd.Parameters.Add("@pwd_change_requirement", SqlDbType.Bit).Value = hbUser.PassChangeReq;

                    cmd.ExecuteNonQuery();

                }
            }

        }


        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի անվան հասանելիության ստուգում
        /// </summary>
        /// <param name="hbUser"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool CheckHBUserNameAvailability(HBUser hbUser, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                bool result;

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT dbo.get_hbUserAvailability(@user_name,@customer_number,@global_id) res";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = hbUser.CustomerNumber;
                    cmd.Parameters.Add("@user_name", SqlDbType.NChar, 24).Value = hbUser.UserName;
                    cmd.Parameters.Add("@global_id", SqlDbType.Int).Value = hbUser.HBAppID;

                    result = Convert.ToBoolean(cmd.ExecuteScalar());

                }
                return result;
            }
        }

        /// <summary>
        /// ՀԲ օգտագործողի լոգ
        /// </summary>
        /// <param name="hbUser"></param>
        /// <returns></returns>
        internal static List<HBUserLog> GetHBUserLog(String userName)
        {
            List<HBUserLog> hbUserLog = new List<HBUserLog>();
            HBUserLog log;

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = "pr_get_online_log";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                log = new HBUserLog();
                foreach (DataRow row in dt.Rows)
                {
                    log = SetHBUserLog(row);
                    hbUserLog.Add(log);
                }
            }
            return hbUserLog;
        }

        private static HBUserLog SetHBUserLog(DataRow row)
        {
            HBUserLog hbUserLoginLog = new HBUserLog();

            if (row != null)
            {
                hbUserLoginLog.UserName = row["USERID"].ToString();
                hbUserLoginLog.Description = row["LOG_MESSAGE"].ToString();
                hbUserLoginLog.TimeStamp = Convert.ToDateTime(row["TIME_STAMP"].ToString());
                hbUserLoginLog.TokenNumber = "";
            }
            return hbUserLoginLog;
        }

        internal static ulong GetHBUserCustomerNumber(string hbUser)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                ulong hbUserCustomerNumber = 0;

                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT customer_number as  UserCustomerNumber 
                                                     FROM Tbl_Users 
                                                     WHERE [user_name] = @userName";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 50).Value = hbUser;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hbUserCustomerNumber = Convert.ToUInt64(dr["UserCustomerNumber"]);
                        }
                    }
                }
                return hbUserCustomerNumber;
            }
        }


        internal static List<HBProductPermission> GetHBUserProductsPermissions(string hbUserName)
        {
            List<HBProductPermission> productsPermissions = new List<HBProductPermission>();

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT app_id,arm_number,product_type,is_active 
                                      FROM Tbl_Account_Permissions 
                                      WHERE todos_user=@hbUserName AND is_active = 1 AND confirmed = 1 ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@hbUserName", SqlDbType.NVarChar).Value = hbUserName;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    HBProductPermission permission = new HBProductPermission();
                    permission = SetHBUserProductsPermission(row);
                    productsPermissions.Add(permission);
                }
            }
            return productsPermissions;
        }


    }
}
