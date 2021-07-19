using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class CreditHereAndNowDB
    {
        /// <summary>
        /// Ապառիկ տեղում տեսակի պայմանագիր կարգավիճակով վարկերը
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="shopFilial"></param>
        /// <returns></returns>
        internal static List<CreditHereAndNow> GetCreditsHereAndNowForActivate(SearchCreditHereAndNow searchParams, out int RowCount )
        {
            List<CreditHereAndNow> credits = new List<CreditHereAndNow>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "pr_Get_CreditsHereAndNow";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (searchParams.QualityFilter == ProductQualityFilter.Contracts)
                        cmd.Parameters.Add("@contracts_only", SqlDbType.Bit).Value = 1;
                    else if((searchParams.QualityFilter == ProductQualityFilter.AllExceptContracts))
                        cmd.Parameters.Add("@contracts_only", SqlDbType.Bit).Value = 0;

                    cmd.Parameters.Add("@date_from", SqlDbType.SmallDateTime).Value = searchParams.DateFrom;
                    cmd.Parameters.Add("@date_to", SqlDbType.SmallDateTime).Value = searchParams.DateTo;
                    cmd.Parameters.Add("@shopFilial", SqlDbType.Int).Value = searchParams.ShopFilial;
                    cmd.Parameters.Add("@row_start", SqlDbType.Int).Value = searchParams.StartRow;
                    cmd.Parameters.Add("@row_end", SqlDbType.Int).Value = searchParams.EndRow;
                    
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        CreditHereAndNow loan = SetLoan(row);

                        credits.Add(loan);
                    }

                    if (dt.Rows.Count > 0)
                        RowCount = int.Parse(dt.Rows[0]["totalRows"].ToString());
                    else
                        RowCount = 0;
                }
            }

            return credits;
        }

        private static CreditHereAndNow SetLoan(DataRow row)
        {
            CreditHereAndNow loan = new CreditHereAndNow();

            if (row != null)
            {

                loan.CustomerNumber = ulong.Parse(row["customer_number"].ToString());
                loan.ProductId = long.Parse(row["app_id"].ToString());
                loan.Quality = short.Parse(row["quality"].ToString());
                loan.Currency = row["currency"].ToString();
                loan.StartCapital = double.Parse(row["start_capital"].ToString());
                loan.CurrentCapital = double.Parse(row["current_capital"].ToString());
                loan.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                loan.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                loan.ProductType = 1;

                loan.CreditCode = LoanProduct.GetCreditCode(loan.ProductId, loan.ProductType);


            }
            return loan;
        }
       
        /// <summary>
        /// Ապառիկ տեղում տեսակի վարկերի ակտիվացման հայտերի նախնական հայտի մուտքագրում
        /// </summary>
        /// <param name="preOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SaveCreditHereAndNowActivationPreOrder(CreditHereAndNowActivationOrders preOrder, string userName,SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_CreditHereAndNow_Activation_PreOrder_Save";
                    cmd.CommandType = CommandType.StoredProcedure;
                                            
                    cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = preOrder.RegistrationDate.Date;  
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;  
                    cmd.Parameters.Add("@preOrder_type", SqlDbType.SmallInt).Value = preOrder.PreOrderType;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = preOrder.OperationFilialCode;
                    cmd.Parameters.Add("@set_user_name", SqlDbType.NVarChar,50 ).Value = userName;
                    cmd.Parameters.Add("@preOrder_products", SqlDbType.Structured).Value = PreOrderDetails.ConvertToDataTable(preOrder.CreditHereAndNowActivationDetails);
                     

                    SqlParameter param = new SqlParameter("@preOrder_ID", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    
                    cmd.ExecuteNonQuery();
                    preOrder.PreOrderID = Convert.ToInt32(cmd.Parameters["@preOrder_ID"].Value);

                    if (preOrder.PreOrderID > 0)
                    {
                        result.ResultCode = ResultCode.Normal;  
                        result.Id = preOrder.PreOrderID;

                    }
                    else
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
                return result;
            } 
        }

    }
}
