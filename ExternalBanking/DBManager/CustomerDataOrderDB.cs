using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class CustomerDataOrderDB
    {
        /// <summary>
        /// Տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(CustomerDataOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_AddNewCustDataApplication";
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
                    cmd.Parameters.Add("@phone", SqlDbType.NVarChar, 70).Value = order.HomePhoneNumber!=null?order.HomePhoneNumber:"";
                    cmd.Parameters.Add("@mobile", SqlDbType.NVarChar, 70).Value = order.MobilePhoneNumber!=null?order.MobilePhoneNumber:"";
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    string Email = "";
                    int count=0;
                    for (int j = 0; j < order.EmailAddress.Count;j++ )
                    {
                        if (!string.IsNullOrEmpty(order.EmailAddress[j]))
                        {
                            count = 1;
                        }
                    }
                    if (count > 0)
                    {
                        Email = order.EmailAddress[0];
                        for (int i = 1; i < order.EmailAddress.Count; i++)
                        {
                            Email = Email + ";" + order.EmailAddress[i];
                        }
                    }
                    cmd.Parameters.Add("@e_mail", SqlDbType.NVarChar, 100).Value = Email;
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 30).Value = order.Password!=null?order.Password:"";
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
        /// Վերադարձնոմ է տվյալների խմբագրման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static CustomerDataOrder Get(CustomerDataOrder order)
        {
            DataTable dt = new DataTable();
            order.EmailAddress = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT d.registration_date,
                                                         d.document_type,
                                                         d.document_number,
                                                         d.document_subtype,
                                                         d.quality,
                                                         c.mobile,
                                                         c.e_mail,
                                                         c.password,
                                                         c.phone,
                                                         d.order_group_id,
                                                         d.confirmation_date
                                                         FROM Tbl_HB_documents AS d
                                                         INNER JOIN Tbl_New_CustDataChange_Application AS c
                                                         ON d.doc_ID=c.Doc_ID
                                                         WHERE d.customer_number=@customer_number AND d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count>0)
                {
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    if (dt.Rows[0]["e_mail"].ToString() != "")
                    {
                        order.EmailAddress.Add(dt.Rows[0]["e_mail"].ToString());
                    }
                    order.HomePhoneNumber = dt.Rows[0]["phone"].ToString();
                    order.MobilePhoneNumber = dt.Rows[0]["mobile"].ToString();
                    order.Password = dt.Rows[0]["password"].ToString();
                    order.RegistrationDate = DateTime.Parse(dt.Rows[0]["registration_date"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                }
            }
            return order;
        }

    }
}
