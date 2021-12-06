using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class FactoringDB
    {
        internal static List<Factoring> GetFactorings(ulong customerNumber)
        {
            List<Factoring> factoringList = new List<Factoring>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"select start_capital,date_of_beginning,date_of_normal_end,quality,interest_rate,currency,App_Id,loan_type,filialcode,
                              ISNULL(factoring_type,0) factoring_type,ISNULL(factoring_regres_type,0) factoring_regres_type,
                              factoring_currency,ISNULL(commission_percent,0) commission_percent  from Tbl_Factoring
                            where quality not in(40) and customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Factoring factoring = SetFactoring(row);

                        factoringList.Add(factoring);
                    }

                }
            }
            return factoringList;
        }

        internal static Factoring GetFactoring(ulong customerNumber, ulong productId)
        {
            Factoring factoring = new Factoring();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select start_capital,date_of_beginning,date_of_normal_end,quality,interest_rate,currency,App_Id,loan_type,filialcode,
                              ISNULL(factoring_type,0) factoring_type,ISNULL(factoring_regres_type,0) factoring_regres_type,
                              factoring_currency,ISNULL(commission_percent,0) commission_percent from Tbl_Factoring
                            where  customer_number=@customerNumber and app_id=@productId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        factoring = SetFactoring(dt.Rows[0]);

                    }
                }
            }
            return factoring;
        }

        internal static List<Factoring> GetClosedFactorings(ulong customerNumber)
        {
            List<Factoring> factoringList = new List<Factoring>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"select start_capital,date_of_beginning,date_of_normal_end,quality,interest_rate,currency,App_Id,loan_type,filialcode,
                              ISNULL(factoring_type,0) factoring_type,ISNULL(factoring_regres_type,0) factoring_regres_type,
                              factoring_currency,ISNULL(commission_percent,0) commission_percent  from Tbl_Factoring
                              where quality=40 and customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Factoring factoring = SetFactoring(row);

                        factoringList.Add(factoring);
                    }

                }
            }
            return factoringList;
        }

        private static Factoring SetFactoring(DataRow row)
        {
            Factoring factoring = new Factoring();
            if (row != null)
            {
                factoring.FillialCode = Convert.ToInt32(row["filialcode"].ToString());
                factoring.ProductId = long.Parse(row["app_id"].ToString());
                factoring.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                factoring.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                factoring.StartCapital = double.Parse(row["start_capital"].ToString());
                factoring.Type = short.Parse(row["loan_type"].ToString());
                factoring.Quality = short.Parse(row["quality"].ToString());
                factoring.InterestRate = float.Parse(row["interest_rate"].ToString());
                factoring.Currency = row["currency"].ToString();
                factoring.FactoringType = short.Parse(row["factoring_type"].ToString());
                factoring.FactoirngRegresType = short.Parse(row["factoring_regres_type"].ToString());
                factoring.FactoringCurrency = row["factoring_currency"].ToString();
                factoring.CommissionPercent = float.Parse(row["commission_percent"].ToString());
            }
            return factoring;

        }


        internal static bool CheckFactoringProvisionCurrency(long productId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT f.app_id   
                                                  FROM (SELECT app_id,currency FROM Tbl_Factoring WHERE App_Id=@productId or main_app_id=@productId) f 
                                                  INNER JOIN Tbl_Link_application_Provision lp ON f.App_Id=lp.app_id 
                                                  INNER JOIN Tbl_provision_of_clients p ON lp.IdPro=p.IdPro 
                                                  WHERE matured_date is null And p.type = 13 And p.currency <> f.currency", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows)
                {
                    check = true;
                }
            }
            return check;


        }

        internal static bool FactoringValidation1(long productId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT f.app_id  FROM Tbl_Factoring f 
						                                      INNER JOIN Tbl_Factoring mf 
															  ON f.main_app_id=mf.App_Id 
                                                              WHERE f.factoring_regres_type = 1 And f.main_app_id =@productId And 
                                                              NOT EXISTS( SELECT 1 
                                                              FROM Tbl_Link_application_Provision lp
							                                  INNER JOIN Tbl_provision_of_clients p
							                                  ON lp.IdPro=p.IdPro and p.type=15 
                                                              INNER JOIN Tbl_provision_owners o
							                                  ON p.IdPro=o.IdPro and o.customer_number=mf.customer_number and lp.app_id=f.app_id)", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows)
                {
                    check = true;
                }
            }
            return check;
        }


        /// <summary>
        /// Ֆակտորինգի դադարեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveFactoringTerminationOrder(FactoringTerminationOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_addNewFactoringTerminationDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@product_Id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

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

        internal static FactoringTerminationOrder GetFactoringTerminationOrder(FactoringTerminationOrder order)
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());

            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT  d.document_number,d.currency,d.debet_account,d.quality,d.description,
                                                  d.registration_date,d.document_subtype,d.source_type,d.operation_date,I.app_id
                                                  FROM Tbl_HB_documents as d
                                                  INNER JOIN Tbl_HB_Products_Identity I ON d.doc_id=I.hb_doc_id 
                                                  where d.Doc_ID=@DocID and d.customer_number=@customer_number", conn);

            cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            dt.Load(cmd.ExecuteReader());

            order.ProductId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
            order.Currency = dt.Rows[0]["currency"].ToString();
            order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
            order.Description = dt.Rows[0]["description"].ToString();
            order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
            order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
            order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
            order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
            return order;

        }

        internal static bool IsPaidFactoring(ulong productId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT F.app_id FROM tbl_factoring F INNER JOIN tbl_paid_factoring P ON F.app_id=P.main_app_id WHERE F.main_app_id=@productId", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows)
                {
                    check = true;
                }
            }
            return check;
        }

        internal static bool IsSecondTermination(FactoringTerminationOrder order)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select doc_id from Tbl_HB_documents D INNER JOIN Tbl_HB_Products_Identity  I ON D.doc_ID=I.HB_Doc_ID
                                                WHERE quality in (1,2,3,5) and document_type=142 and document_subtype=1 and
                                                customer_number=@customerNumber and I.App_ID=@productId", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductId;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }
        internal static ulong GetFactoringCustomerNumber(ulong productId)
        {
            ulong factoringCustomerNumber;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT mf.customer_number 
														 FROM Tbl_paid_factoring pf 
															INNER JOIN tbl_factoring f ON pf.main_app_id=f.app_id 
															INNER JOIN tbl_factoring mf ON f.main_app_id=mf.app_id 
														 WHERE pf.app_id = @app_id", conn);
                cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                factoringCustomerNumber = Convert.ToUInt64(cmd.ExecuteScalar().ToString());
            }
            return factoringCustomerNumber;
        }


    }
}
