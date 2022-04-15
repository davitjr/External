using ExternalBanking.XBManagement;
using ExternalBanking.XBManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class HBApplicationDB
    {
        /// <summary>
        /// Վերադարձնում է նշված ՀԲ պայմանագիրը
        /// </summary>
        /// <param name="hbUserID"></param>
        /// <returns></returns>
        internal static HBApplication GetHBApplication(ulong customerNumber)
        {
            HBApplication hbApplication = new HBApplication();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT  a.*,s.description as quality_description  FROM Tbl_Applications a
                               INNER JOIN  [dbo].[Tbl_application_statuses] s
                                       ON a.application_status=s.app_status						
                                       WHERE a.customer_number = @customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.NVarChar, 50).Value = customerNumber.ToString();

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    hbApplication = SetHBApplication(row);
                }
                else
                {
                    hbApplication = null;
                }

            }
            return hbApplication;
        }

        /// <summary>
        /// HBApplication-ի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static HBApplication SetHBApplication(DataRow row)
        {
            HBApplication hbApplication = new HBApplication();

            if (row != null)
            {
                hbApplication.ID = Convert.ToInt32(row["ID"].ToString());
                hbApplication.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                hbApplication.ContractNumber = row["contract_number"].ToString();
                hbApplication.ContractType = Convert.ToByte(row["contract_type"].ToString());
                hbApplication.FilialCode = Convert.ToInt32(row["filial"].ToString());
                hbApplication.Quality = (HBApplicationQuality)Convert.ToByte(row["application_status"].ToString());
                hbApplication.QualityDescription = row["quality_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(row["quality_description"].ToString()) : default(string);
                hbApplication.SetID = Convert.ToInt32(row["set_id"].ToString());
                hbApplication.InvolvingSetNumber = Convert.ToInt32(row["set_id"].ToString());
                hbApplication.PermissionType = Convert.ToByte(row["permission_type"].ToString());
                hbApplication.SetName = Utility.ConvertAnsiToUnicode(Utility.GetUserFullName(hbApplication.SetID));

                hbApplication.StatusChangeSetID = row["status_change_set_id"] != DBNull.Value ? Convert.ToInt32(row["status_change_set_id"].ToString()) : default(int);

                hbApplication.ContractDate = row["contract_date"] != DBNull.Value ? Convert.ToDateTime(row["contract_date"]) : default(DateTime?);                 //DateTime.Parse(row["contract_date"].ToString());
                hbApplication.ApplicationDate = row["application_date"] != DBNull.Value ? Convert.ToDateTime(row["application_date"]) : default(DateTime?);       //DateTime.Parse(row["application_date"].ToString());
                hbApplication.StatusChangeDate = row["status_change_date"] != DBNull.Value ? Convert.ToDateTime(row["status_change_date"]) : default(DateTime?); //DateTime.Parse(row["status_change_date"].ToString());                
            }
            return hbApplication;
        }

        /// <summary>
        /// Հեռահար բանկինգի հայտի պահպանում
        /// </summary>
        /// <param name="hbApplicationOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        internal static void Save(HBApplicationOrder hbApplicationOrder, ACBAServiceReference.User user, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBApplication_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = hbApplicationOrder.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = hbApplicationOrder.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = hbApplicationOrder.RegistrationDate.Date;
                    cmd.Parameters.Add("@set_id", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)hbApplicationOrder.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = hbApplicationOrder.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = hbApplicationOrder.FilialCode;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = hbApplicationOrder.Type == OrderType.HBApplicationOrder ? Action.Add : Action.Update;
                    cmd.Parameters.Add("@global_id", SqlDbType.Int).Value = hbApplicationOrder.HBApplication.ID;
                    cmd.Parameters.Add("@contract_number", SqlDbType.VarChar, 30).Value = hbApplicationOrder.HBApplication.ContractNumber;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = hbApplicationOrder.HBApplication.InvolvingSetNumber;
                    cmd.Parameters.Add("@permission_type", SqlDbType.TinyInt).Value = hbApplicationOrder.HBApplication.PermissionType;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    hbApplicationOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    hbApplicationOrder.Quality = OrderQuality.Draft;
                }
            }

        }

        /// <summary>
        /// Վերադարձնում է հեռահար բանկինգի հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static HBApplicationOrder GetHBApplicationOrder(HBApplicationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT registration_date,
                                                         doc.document_number,
                                                         doc.document_type,
                                                         doc.debet_account,
                                                         doc.deb_for_transfer_payment,
                                                         doc.source_type,
                                                         doc.quality,
                                                         doc.document_subtype ,
                                                         doc.registration_date,
                                                         doc.operation_date,
                                                         doc.confirmation_date,

                                                         det.contract_number,   
                                                         det.contract_date                                                           
                                                         FROM Tbl_HB_documents as doc

                                                         INNER JOIN Tbl_HBApplication_Order_Details det
                                                         on doc.doc_ID=det.doc_ID
                                                         WHERE doc.customer_number=case when @customer_number = 0 then doc.customer_number else @customer_number end AND doc.doc_ID=@DocID", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                order.HBApplication = new HBApplication();
                                order.HBApplication.ContractNumber = dr["contract_number"].ToString();
                                order.HBApplication.ContractDate = dr["contract_date"] != DBNull.Value ? Convert.ToDateTime(dr["contract_date"]) : default(DateTime?);

                                order.OrderNumber = dr["document_number"].ToString();
                                order.Type = (OrderType)dr["document_type"];
                                order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                                order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                                order.SubType = Convert.ToByte(dr["document_subtype"]);
                                order.Source = (SourceType)int.Parse(dr["source_type"].ToString());
                                order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                                order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                            }
                        }
                    }
                }
            }
            return order;
        }

        /// <summary>
        /// Ստուգվում է գոյություն ունի դեռրևս չհաստատված հեռահար բանկինգի մուտքագրման/փոփոխության կամ ակտիվացման հայտ, թե ոչ կամ կա չակտիվացված ծառայություն
        /// </summary>
        internal static bool isExistsNotConfirmedHBOrder(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM tbl_hb_documents WHERE document_type in(116,132,69,164) AND quality in(3,50) AND customer_number = @customer_number ) OR 
                                                             EXISTS (SELECT 1 FROM dbo.Tbl_Requests_For_Fee_Charges C inner join dbo.tbl_applications A ON A.id = C.global_id  AND  A.contract_type = C.contract_type WHERE A.customer_number = @customer_number  AND is_complete = 0 AND A.contract_type = 1) 
                                                         SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
        /// <summary>
        /// Նոր հեռահար բանկինգի հայտ մուտքագրելիս ստուգվում է արդյոք տվյալ հաճախորդը արդեն ունի հեռահար բանկինգի ծառայություն
        /// </summary>
        internal static bool isExistsHB(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM tbl_Applications WHERE customer_number = @customer_number) SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
        internal static int? GetHbAppicationStatus(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT application_status FROM tbl_Applications WHERE customer_number = @customer_number";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        internal static int? GetHbActivationTokenStatus(long docId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT quality FROM dbo.Tbl_Tokens WHERE token_serial = (select token_serial From Tbl_HB_Token_Order_Details where doc_Id = @doc_Id)";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@doc_Id", SqlDbType.BigInt).Value = docId;

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        internal static List<string> GetHBApplicationStateBeforeAndAfterConfirm(HBApplicationOrder order)
        {
            DataTable dt = new DataTable();
            List<string> hbApplicationState = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBApplicationStateBeforeAndAfterConfirm";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_id", SqlDbType.BigInt).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        hbApplicationState.Add(dt.Rows[0]["tokens_before_approve"].ToString());
                        hbApplicationState.Add(dt.Rows[0]["added_tokens"].ToString());
                        hbApplicationState.Add(dt.Rows[0]["tokens_after_approve"].ToString());
                    }
                }
                return hbApplicationState;
            }
        }
        /// <summary>
        /// Հեռահար բանկինգի իրավունքներով լիազորված անձանց ցանկ
        /// </summary>
        internal static List<AssigneeCustomer> GetHBAssigneeCustomers(ulong customerNumber)
        {
            List<AssigneeCustomer> assigneeList = new List<AssigneeCustomer>();
            DataTable dt = new DataTable();
            AssigneeCustomer assignee;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_getHBAssignees";

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                foreach (DataRow row in dt.Rows)
                {
                    if (row != null)
                    {
                        assignee = new AssigneeCustomer();
                        assignee.CustomerNumber = Convert.ToInt64(row["customerNumber"].ToString());
                        assignee.FullName = Utility.ConvertAnsiToUnicode(row["fullname"] != DBNull.Value ? row["fullname"].ToString() : default(string));
                        assignee.DefaultDocument = Utility.ConvertAnsiToUnicode(row["DefDoc"] != DBNull.Value ? row["DefDoc"].ToString() : default(string));
                        assigneeList.Add(assignee);
                    }
                }
            }
            return assigneeList;
        }

        internal static (long?, bool) IsExistsNotConfirmedHBOrder(ulong customerNumber, OrderType documentType)
        {
            bool isFind = false;
            int? notConfirmOrderId = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"select top 1 doc_ID from tbl_hb_documents where customer_number = @customer_number and document_type = @document_type and quality = 3 order by doc_ID desc";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (int)documentType;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    if (dr.Read())
                    {
                        isFind = true;
                        notConfirmOrderId = (int?)dr["doc_ID"];
                    }
                }
            }
            return (notConfirmOrderId, isFind);
        }
        internal static HBApplicationOrder GetHBApplicationOrder(ulong customerNumber, OrderType documentType)
        {
            HBApplicationOrder order = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_getHBAppOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = (int)documentType;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                order = new HBApplicationOrder
                                {
                                    HBApplication = new HBApplication
                                    {
                                        ID = int.Parse(dr["id"].ToString()),
                                        ContractNumber = dr["contract_number"].ToString(),
                                        ContractDate = dr["contract_date"] != DBNull.Value ? Convert.ToDateTime(dr["contract_date"]) : default(DateTime?),
                                        Quality = dr["application_status"] != DBNull.Value ? (HBApplicationQuality)int.Parse(dr["application_status"].ToString()) : default
                                    },
                                    HBApplicationUpdate = new HBApplicationUpdate
                                    {
                                        AddedItems = new List<IEditableHBItem>(),
                                        DeactivatedItems = new List<IEditableHBItem>(),
                                        UpdatedItems = new List<IEditableHBItem>()
                                    },
                                    OrderNumber = dr["document_number"].ToString(),
                                    Type = (OrderType)dr["document_type"],
                                    RegistrationDate = Convert.ToDateTime(dr["registration_date"]),
                                    Quality = (OrderQuality)Convert.ToInt16(dr["quality"]),
                                    SubType = Convert.ToByte(dr["document_subtype"]),
                                    Source = (SourceType)int.Parse(dr["source_type"].ToString()),
                                    Id = int.Parse(dr["doc_id"].ToString()),
                                    OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?)
                                };
                            };
                        }
                    }
                }
            }
            return order;
        }

        public static ActionResult MigrateOldUserToCas(int hbUserId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE tbl_users set is_cas = 1 where id = @hbUserId UPDATE Tbl_Tokens set quality = 2, blocked = 1 where user_id = @hbUserId and token_serial NOT LIKE 'ACBA%' and GID in ('02','03')";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@hbUserId", SqlDbType.Int).Value = hbUserId;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }


        public static List<string> GetTempTokenList(int tokenCount)
        {
            List<string> listToken = new List<string>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "get_temp_token_number";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@tokenCount", SqlDbType.Int).Value = tokenCount;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                listToken.Add(dr["tempToken"].ToString());
                            }
                        }
                    }
                }

                return listToken;
            }
        }
    }
}
