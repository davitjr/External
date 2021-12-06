using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class ChequeBookDB
    {
        /// <summary>
        /// Չեկային գրքույքի ստացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(ChequeBookOrder order, string userName,SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewChequebookDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@chequebook_account", SqlDbType.Float).Value = order.ChequeBookAccount.AccountNumber;
                    cmd.Parameters.Add("@service_fee_account", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.FeeAmount;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar,50).Value = order.PersonFullName;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9)
                    {                        
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;                        
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 10)
                    {
                        result.Id = order.Id;                        
                        result.ResultCode = ResultCode.Normal;                 
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
                return result;
            }
        }
        /// <summary>
        /// Վերադարձնում է չեկային գրքույքի հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static ChequeBookOrder Get(ChequeBookOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select registration_date,
                                                         document_number,
                                                         document_type,
                                                         debet_account,
                                                         deb_for_transfer_payment,
                                                         source_type,
                                                         amount,
                                                         quality,
                                                         document_subtype,
                                                         operation_date,
                                                         order_group_id
                                                         from Tbl_HB_documents
                                                         where customer_number=case when @customer_number = 0 then customer_number else @customer_number end and doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count>0)
                {
                    order.ChequeBookAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["debet_account"]));
                    order.ChequeBookAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.ChequeBookAccount.AccountTypeDescription);
                    order.FeeAmount = dt.Rows[0]["amount"].ToString();
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["deb_for_transfer_payment"]));
                    if (order.FeeAccount!=null)
                    {
                        order.FeeAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccount.AccountTypeDescription);
                    }
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

                }


            }
            return order;
        }

       
    }
}
