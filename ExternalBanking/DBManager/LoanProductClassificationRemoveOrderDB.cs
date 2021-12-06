using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using ExternalBanking.ACBAServiceReference;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class LoanProductClassificationRemoveOrderDB
    {
        /// <summary>
        /// Դասակարգված վարկի դասի հեռացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(LoanProductClassificationRemoveOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_addNewLoanProductClassificationRemoveDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@product_app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_type", SqlDbType.Int).Value = order.ProductType;
                    //cmd.Parameters.Add("@product_type", SqlDbType.Int).Value = Utility.GetProductTypeFromLoanType((short)order.ProductType);


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }

            }
        }
        /// <summary>
        /// Դասակարգված վարկերի դասի հեռացման ավտոմատ ստեղծում նախնական հայտերի հիման վրա
        /// </summary>
        /// <param name="preOrderId"></param>
        /// <returns></returns>
        internal static async Task<List<LoanProductClassificationRemoveOrder>> GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(long preOrderId)
        {
            List<LoanProductClassificationRemoveOrder> loanProductClassificationRemoveOrders = new List<LoanProductClassificationRemoveOrder>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string script = @"SELECT customer_number,app_ID,quality,product_type FROM Tbl_Automatic_HB_Documents_Generation_PreOrder_Details WHERE preOrder_ID=@preOrder_Id";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(script, conn);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@preOrder_Id", SqlDbType.BigInt).Value = preOrderId;

                using DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    LoanProductClassificationRemoveOrder product = SetLoanProductClassificationRemoveOrder(row);
                    loanProductClassificationRemoveOrders.Add(product);
                }

            }
            
            return loanProductClassificationRemoveOrders;
        }
        /// <summary>
        ///  Դասակարգված վարկի դասի հեռացման ավտոմատ ստեղծված հայտի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static LoanProductClassificationRemoveOrder SetLoanProductClassificationRemoveOrder(DataRow row)
        {
            LoanProductClassificationRemoveOrder product = new LoanProductClassificationRemoveOrder();
            if (row != null)
            {
                product.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                product.ProductId = Convert.ToUInt64(row["app_id"].ToString());
                product.ProductType = Convert.ToInt32(row["product_type"].ToString());
                product.Type = OrderType.LoanProductClassificationRemoveOrder;
                product.Quality = (OrderQuality)Convert.ToUInt16(row["quality"].ToString());
            }
            return product;
        }
        /// <summary>
        /// Ստուգում է գոյություն ունի՞ արդեն մուտքագրված հայտ, թե ոչ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static bool IsExistsNotConfirmedOrder(LoanProductClassificationRemoveOrder order)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select doc_id from Tbl_HB_documents D INNER JOIN Tbl_HB_Products_Identity  I ON D.doc_ID=I.HB_Doc_ID
                                                WHERE quality in (1,2,3,5)  and customer_number = @customerNumber and I.App_ID=@productId
                                                AND document_type = 171", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductId;

               using  SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }
        /// <summary>
        /// Ստուգվում է տվյալ կարգավիճակով վարկը հնարավոր է հետ բերել, թե ոչ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static bool IsQualityWrong(LoanProductClassificationRemoveOrder order)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"IF exists(SELECT 1 FROM V_ShortLoans_small WHERE app_id = @productApp_ID AND (quality <> 11 OR loan_type = 49))
												  BEGIN
													SELECT 1
												  END
												  ELSE IF exists(SELECT 1 FROM Tbl_credit_lines WHERE app_id = @productApp_ID AND quality <> 11)
												  BEGIN
													SELECT 1
												  END
												  ELSE IF exists( SELECT 1 FROM Tbl_given_guarantee WHERE app_id = @productApp_ID AND quality <> 11)
												  BEGIN
													SELECT 1
												  END", conn);
                    

                cmd.Parameters.Add("@productApp_ID", SqlDbType.BigInt).Value = order.ProductId;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    check = true;
                }
            }
            return check;
        }
    }
}
