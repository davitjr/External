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
    class ChangeBranchOrderDB
    {
        internal static ActionResult Save(ChangeBranchOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_add_change_branch_order";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@customer_number", (double)order.CustomerNumber);//
                    cmd.Parameters.AddWithValue("@registration_date", order.RegistrationDate);//
                    cmd.Parameters.AddWithValue("@document_type", (short)OrderType.ChangeBranch);//
                    cmd.Parameters.AddWithValue("@operation_filial_code", (short)order.FilialCode);//
                    cmd.Parameters.AddWithValue("@document_number", order.OrderNumber);//
                    if (order.Card.Currency.Equals("AMD"))
                    {
                        cmd.Parameters.AddWithValue("@amount", Math.Round(order.Card.PositiveRate, 1));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@amount", Math.Round(order.Card.PositiveRate, 2));
                    }
                    cmd.Parameters.AddWithValue("@currency", order.Card.Currency);
                    cmd.Parameters.AddWithValue("@app_ID", (double)order.ProductId);
                    //cmd.Parameters.AddWithValue("@credit_account", order.Account.AccountNumber);
                    cmd.Parameters.AddWithValue("@source_type", (short)source);//
                    cmd.Parameters.AddWithValue("@document_subtype", (short)order.SubType);//
                    cmd.Parameters.AddWithValue("@username", userName);//
                    cmd.Parameters.AddWithValue("@oper_day", order.OperationDate);

                    cmd.Parameters.AddWithValue("@filial_code", (int)order.Filial);
                    cmd.Parameters.AddWithValue("@cardnumber", (long)order.CardNumber);//Convert.ToInt64(order.Card.CardNumber));  
                    cmd.Parameters.AddWithValue("@moved_filial_code", (int)order.MovedFilial);

                    cmd.Parameters.Add("@ID", SqlDbType.BigInt);
                    cmd.Parameters["@ID"].Direction = ParameterDirection.Output;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@ID"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }

        internal static ChangeBranchOrder getChangeBranchOrder(ChangeBranchOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pr_get_change_branch_order", conn);
                cmd.Parameters.AddWithValue("@ID", order.Id);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        // order.Id= Convert.ToInt64(dr["HB_Doc_ID"]);
                        order.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                        order.CardNumber = Convert.ToInt64(dr["CardNumber"]);
                        order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        order.Type = (OrderType)Convert.ToInt16(dr["document_type"]);
                        order.FilialCode = Convert.ToUInt16(dr["Filial"]);
                        order.MovedFilial = Convert.ToUInt16(dr["MovedFilial"]);
                        order.Quality = (OrderQuality)dr["quality"];
                        order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                    }
                }
            }
            return order;
        }



        internal static ChangeBranchOrder GetFilialCode(ChangeBranchOrder order)
        {
            string sql = @"select 22000+filial as filial, app_id  
                           from Tbl_VISA_applications	 
                           where cardnumber=@cardNumber  and CardStatus='NORM'  ";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = order.CardNumber;
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            order.Filial = Convert.ToInt32(dr["filial"]);
                            order.ProductId = Convert.ToDouble(dr["app_id"]);
                        }
                    }

                    dr.Close();
                    conn.Close();
                }
            }
            return order;
        }

        internal static short CheckCardReplacedOrReissued(ChangeBranchOrder order)
        {
            short typeID = 0;
            string sql = @"SELECT typeID  FROM [Tbl_CardChanges]
                               where old_app_id= @old_app_id";

            string config = ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    cmd.Parameters.Add("@old_app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Connection = conn;
                    conn.Open();
                    typeID = Convert.ToInt16(cmd.ExecuteScalar());
                }
            }
            return typeID;
        }
    }
}

