using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class PreOrderDB
    {
        /// <summary>
        /// Վերադարձնում է հայտերի ցանկը
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="RowCount"></param>
        /// <returns></returns>
        public static List<PreOrderDetails> GetSearchedPreOrderDetails(SearchPreOrderDetails searchParams, out int RowCount)
        {
            List<PreOrderDetails> list = new List<PreOrderDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_PreOrder_Details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@date_from", SqlDbType.DateTime).Value = searchParams.DateFrom;
                    cmd.Parameters.Add("@date_to", SqlDbType.DateTime).Value = searchParams.DateTo;
                    cmd.Parameters.Add("@row_start", SqlDbType.Int).Value = searchParams.StartRow;
                    cmd.Parameters.Add("@row_end", SqlDbType.Int).Value = searchParams.EndRow;
                    cmd.Parameters.Add("@quality", SqlDbType.SmallInt).Value = searchParams.Quality;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = searchParams.CustomerNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = searchParams.AppID;
                    cmd.Parameters.Add("@preorder_type", SqlDbType.SmallInt).Value = searchParams.Type;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        PreOrderDetails preorder = new PreOrderDetails();

                        if (row != null)
                        {
                            preorder.AppID = ulong.Parse(row["app_ID"].ToString());
                            preorder.CustomerNumber = ulong.Parse(row["customer_number"].ToString());
                            preorder.QualityDescription = row["description_arm"].ToString();
                            preorder.Quality = (PreOrderQuality)short.Parse(row["quality"].ToString());
                            preorder.PreOrderID = int.Parse(row["preOrder_ID"].ToString());
                            preorder.Amount = Double.Parse(row["start_capital"].ToString());
                            list.Add(preorder);
                        }
                    }

                    if (dt.Rows.Count > 0)
                        RowCount = int.Parse(dt.Rows[0]["totalRows"].ToString());
                    else
                        RowCount = 0;
                }
            }

            return list;
        }
        /// <summary>
        /// Փոխում է կարգավիճակը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="appID"></param>
        /// <param name="quality"></param>
        public static void UpdatePreOrderDetailQuality(ulong customerNumber, ulong appID, PreOrderQuality quality)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"UPDATE  U SET U.quality = @quality FROM
                                                (SELECT top 1 quality  FROM Tbl_Automatic_HB_Documents_Generation_PreOrder_Details WHERE customer_number = @customerNumber AND app_Id = @appID 
                                                AND quality = 10 ORDER BY id DESC) U", conn);

                cmd.Parameters.Add("@quality", SqlDbType.SmallInt).Value = quality;
                cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                cmd.Parameters.Add("@appID", SqlDbType.BigInt).Value = appID;

                cmd.ExecuteNonQuery();

            }
        }

        internal static bool IsExistIncompletePreOrders(PreOrderType preOrderType)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM Tbl_Automatic_HB_Documents_Generation_PreOrders O LEFT JOIN Tbl_Automatic_HB_Documents_Generation_PreOrder_Details OD ON O.preOrder_ID = OD.preOrder_ID
                                        WHERE O.preOrder_Type = @preOrderType AND OD.quality = 10) 
                                        SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@preOrderType", SqlDbType.Float).Value = preOrderType;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
        /// <summary>
        /// Նախորդ խմբաքանակի չձևավորված բոլոր հայտերի կարգավիճակը դարձնում է "Հեռացված"
        /// </summary>
        public static ActionResult ResetIncompletePreOrderDetailQuality()
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE U SET U.quality = @quality FROM (SELECT quality FROM Tbl_Automatic_HB_Documents_Generation_PreOrder_Details WHERE quality = 10) U";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = PreOrderQuality.Deleted;
                    cmd.ExecuteNonQuery();
                }

                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }
        /// <summary>
        /// Վերադարձնում է չձևավորված ապառիկ հայտերի քանակը
        /// </summary>
        public static int GetIncompletePreOrdersCount()
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(*) AS Count FROM Tbl_Automatic_HB_Documents_Generation_PreOrder_Details WHERE quality = 10";
                    cmd.CommandType = CommandType.Text;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
