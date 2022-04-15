using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class SwiftCopyOrderDB
    {
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(SwiftCopyOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_addNewSwiftCopyApplication";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@service_fee_account", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.FeeAmount;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@lang_id", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@Connected_DocId", SqlDbType.Int).Value = order.ContractNumber;
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
            }

            return result;
        }
        /// <summary>
        /// Ստուգում է եղել ե այդպիսի Swift փոխանցում թե ոչ 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool CheckForSwiftCopy(long Id, ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT Doc_Id FROM tbl_hb_documents WHERE doc_id = @doc_id AND document_type  in (3,64) AND document_subtype = 1 AND customer_number = @customernumber AND quality = 30 AND DATEDIFF(day, registration_date, getdate()) < = 183", conn);
                cmd.Parameters.Add("@doc_id", SqlDbType.Float).Value = Id;
                cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    check = true;
                }
            }
            return check;
        }
        /// <summary>
        /// ՎՎերադարձնում է Swift հաղորդագրության պատճենի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static SwiftCopyOrder Get(SwiftCopyOrder order)
        {
            using DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select d.registration_date,
                                                         d.document_number,
                                                         d.document_type,
                                                         d.deb_for_transfer_payment,
                                                         d.source_type,
                                                         d.description,
                                                         h.change_user_name,
                                                         d.amount_for_payment,
                                                         d.quality,
                                                         d.document_subtype,
                                                         d.operation_date, 
                                                         d.order_group_id,
                                                         d.confirmation_date
                                                         from Tbl_HB_documents as d
                                                         inner join Tbl_HB_quality_history as h
                                                         on d.doc_ID=h.Doc_ID
                                                         where d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end and d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.FeeAmount = dt.Rows[0]["amount_for_payment"].ToString();
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["deb_for_transfer_payment"]));
                    if (order.FeeAccount != null)
                    {
                        order.FeeAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccount.AccountTypeDescription);
                    }
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.ContractNumber = int.Parse(dt.Rows[0]["description"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                }
            }
            return order;
        }


        internal static bool CheckSwiftCopy(long docID)
        {

            bool val = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("select top 1 b.add_tbl_unic_number from tbl_bank_mail_in b inner join(select * from dbo.tbl_hb_documents  where document_type = 134) h " +
                    " on b.id = h.transfer_id where b.add_tbl_name = 'tbl_hb_documents' and  b.add_tbl_unic_number = @Doc_ID", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            val = true;
                    }
                }
            }

            return val;

        }

        internal static byte[] PrintSwiftCopyOrderFile(long docID)
        {
            byte[] attachment = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sqlString = @"select top 1 attachment from tbl_HB_Attached_Documents where doc_ID = @Id order by id desc";


                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = docID;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    attachment = (byte[])dr["attachment"];
                }
            }
            return attachment;
        }


    }
}





