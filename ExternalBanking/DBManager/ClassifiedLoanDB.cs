using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    public class ClassifiedLoanDB
    {
        /// <summary>
        /// Դասակարգված վարկեր
        /// </summary>
        public static List<ClassifiedLoan> GetClassifiedLoans(SearchClassifiedLoan searchParams, out int RowCount)
        {
            List<ClassifiedLoan> credits = new List<ClassifiedLoan>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "pr_customers_products_classification_paging";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    if (searchParams.ListType == ClassifiedLoanListType.WrongClassifiedLoansList)
                    {
                        cmd.Parameters.Add("@show_wrong_classified_loans", SqlDbType.Bit).Value = 1;
                        cmd.Parameters.Add("@show_not_out_loans", SqlDbType.Bit).Value = 0;
                    }
                    else if (searchParams.ListType == ClassifiedLoanListType.NotOutLoansList)
                    {
                        cmd.Parameters.Add("@show_wrong_classified_loans", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@show_not_out_loans", SqlDbType.Bit).Value = 1;
                    }

                    cmd.Parameters.Add("@from_tmp", SqlDbType.SmallInt).Value = 1;
                    cmd.Parameters.Add("@row_start", SqlDbType.Int).Value = searchParams.StartRow;
                    cmd.Parameters.Add("@row_end", SqlDbType.Int).Value = searchParams.EndRow;

                    if (searchParams.Currency != "0")
                        cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = searchParams.Currency;

                    if (searchParams.FilialCode != 0)
                        cmd.Parameters.Add("@filialcode", SqlDbType.Int).Value = searchParams.FilialCode;
                    if (searchParams.Quality != 0)
                        cmd.Parameters.Add("@quality", SqlDbType.SmallInt).Value = searchParams.Quality;
                    if (searchParams.CustomerNumber != 0)
                        cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = searchParams.CustomerNumber;
                    if (searchParams.LoanFullNumber != 0)
                        cmd.Parameters.Add("@loan_full_number", SqlDbType.VarChar, 20).Value = searchParams.LoanFullNumber;
                    if (searchParams.AllData == true)
                        cmd.Parameters.Add("@allData", SqlDbType.Bit).Value = 1;


                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        ClassifiedLoan loan = SetLoan(row);

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
        /// <summary>
        /// Դասակարգված վարկի ինիցիալիզացում
        /// </summary>
        private static ClassifiedLoan SetLoan(DataRow row)
        {
            ClassifiedLoan loan = new ClassifiedLoan();
            loan.LoanAccount = new Account();

            if (row != null)
            {

                loan.CustomerNumber = ulong.Parse(row["customer_number"].ToString());
                loan.ProductId = long.Parse(row["app_id"].ToString());
                loan.QualityDescription = row["quality_description"].ToString();
                loan.Quality = short.Parse(row["quality"].ToString());
                loan.Currency = row["currency"].ToString();
                loan.StartCapital = double.Parse(row["start_capital"].ToString());
                loan.CurrentCapital = double.Parse(row["current_capital"].ToString());
                loan.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                loan.Filial = row["filialcode"].ToString();
                loan.LoanTypeDescription = row["description"].ToString();
                loan.ClassificationDate = Convert.ToDateTime(row["classification_date"].ToString());
                loan.LoanClassType = (RiskClassCode)short.Parse(row["class"].ToString());
                loan.LoanAccount.AccountNumber = row["loan_full_number"].ToString();
                loan.LoanClassTypeDescription = row["classDescription"].ToString();
                loan.ProductType = short.Parse(row["product_type"].ToString());

            }
            return loan;
        }
        /// <summary>
        /// Դասակարգված վարկերի համապատասխան գործողության` վարկի դուրսգրման կամ հետ դասակարգման, նախնական հայտերի մուտքագրում
        /// </summary>
        public static ActionResult SaveClassifiedLoanActionPreOrder(ClassifiedLoanActionOrders preOrder, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_ClassifiedLoan_Action_PreOrder_Save";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = preOrder.RegistrationDate.Date;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@preOrder_type", SqlDbType.SmallInt).Value = preOrder.PreOrderType;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = preOrder.OperationFilialCode;
                    cmd.Parameters.Add("@set_user_name", SqlDbType.NVarChar, 50).Value = userName;
                    cmd.Parameters.Add("@preOrder_products", SqlDbType.Structured).Value = PreOrderDetails.ConvertToDataTable(preOrder.ClassifiedLoanActionDetails);


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

        public static void CustomersClassification(ACBAServiceReference.User user, SourceType source)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 300;
                    cmd.CommandText = "pr_customers_classification";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = user.userID;
                    cmd.ExecuteNonQuery();

                }
            }
        }

    }
}
