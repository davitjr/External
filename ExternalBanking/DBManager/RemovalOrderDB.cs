using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    static class RemovalOrderDB
    {
        internal static ActionResult Save(RemovalOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewDeclineRequest";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@Connected_Doc_Id", SqlDbType.Int).Value = order.RemovingOrderId;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@removing_reason", SqlDbType.Int).Value = order.RemovingReason;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 200).Value = order.RemovingReasonAdd;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    SqlParameter param2 = new SqlParameter("@result", SqlDbType.Int);
                    param2.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param2);

                    cmd.ExecuteNonQuery();
                    if(cmd.Parameters["@id"].Value != DBNull.Value && Convert.ToInt32(cmd.Parameters["@id"].Value) != -1)
                    {
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        result.ResultCode = ResultCode.Normal;
                    }
                    else
                    {
                        result.Errors = new List<ActionError>();
                        result.Errors.Add(new ActionError(Convert.ToInt16(cmd.Parameters["@result"].Value)));
                        result.ResultCode = ResultCode.ValidationError;
                    }
                
                   
                }
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static RemovalOrder Get(RemovalOrder order)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT d.registration_date,
                                                         d.document_type,
                                                         d.document_number,
                                                         d.quality,
                                                         d.document_subtype,   
                                                         d.filial,
                                                         r.Connected_doc_id as removing_doc_id,
                                                         r.reason,
                                                         r.description as removing_reason_add,
                                                         d.operationFilialCode,
                                                         d.debet_account,
                                                         d.operation_date,
                                                         d.confirmation_date
                                                         FROM Tbl_HB_documents AS d 
                                                         INNER JOIN Tbl_DeclineRequest AS r
                                                         ON d.doc_ID=r.Doc_ID
                                                         WHERE d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.doc_ID=@DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                if (dt.Rows.Count > 0)
                {
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.FilialCode = ushort.Parse(dt.Rows[0]["operationFilialCode"].ToString());
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.RemovingOrderId = long.Parse(dt.Rows[0]["removing_doc_id"].ToString());
                    if (dt.Rows[0]["reason"].ToString() != "")
                    {
                        order.RemovingReason = short.Parse(dt.Rows[0]["reason"].ToString());
                    }
                    order.RemovingReasonAdd = dt.Rows[0]["removing_reason_add"].ToString();
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.DebitAccount = new Account();
                    order.DebitAccount.AccountNumber = dt.Rows[0]["debet_account"].ToString();

                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                }
            }
            return order;
        }

        internal static bool CanRemoveOrder(long removingOrderId, long userId)
        {
            bool canRemoveOrder;
            canRemoveOrder = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select dbo.fnc_can_remove_order(@userId,@removingOrderId) ";
                    cmd.Parameters.Add("@userId", SqlDbType.BigInt).Value = userId;
                    cmd.Parameters.Add("@removingOrderId", SqlDbType.BigInt).Value = removingOrderId;
                    canRemoveOrder = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }

            return canRemoveOrder;
        }

        /// <summary>
        /// Գործարքի հեռացման հայտի կրկնակի ստուգում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="removingOrderId"></param>
        /// <returns></returns>
        internal static bool IsSecondRemoving(ulong customerNumber, long removingOrderId)
        {
            bool secondRemoving;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select d.doc_ID FROM Tbl_HB_documents AS d 
                                                         INNER JOIN Tbl_DeclineRequest AS r
                                                         ON d.doc_ID=r.Doc_ID
                                where d.quality in (1,2,3,5) and d.document_type in (18,19) and d.document_subtype=1 and
                                                customer_number=@customerNumber and r.Connected_doc_id=@removingOrderId", conn);

                cmd.Parameters.Add("@removingOrderId", SqlDbType.Int).Value = removingOrderId;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    secondRemoving = true;
                }
                else
                    secondRemoving = false;
            }
            return secondRemoving;
        }
    }
}
