using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class LoanMonitoringConclusionDB
    {
        internal static List<LoanMonitoringConclusion> GetConclusions(long productId)
        {
            List<LoanMonitoringConclusion> monitorings = new List<LoanMonitoringConclusion>();
            LoanMonitoringConclusion monitoring;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT * FROM Tbl_loan_monitoring_conclusions C
                               INNER JOIN Tbl_loan_monitoring_conclusion_linked_loans L
                                ON C.id=L.monitoring_id
                              WHERE app_id=@productId and linked=1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            monitoring = Set(dr);

                            monitorings.Add(monitoring);
                        }
                    }
                }
            }
            return monitorings;
        }

        internal static LoanMonitoringConclusion Get(long monitoringId, long productId)
        {
            LoanMonitoringConclusion monitoring = new LoanMonitoringConclusion();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT * FROM Tbl_loan_monitoring_conclusions C
                               INNER JOIN Tbl_loan_monitoring_conclusion_linked_loans L
                               ON C.id=L.monitoring_id
                               WHERE Id=@monitoringId AND L.app_id=@productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            monitoring = Set(dr);
                            monitoring.GetLinkedMonitoringLoans();
                            monitoring.GetProvisionQualityConclusion();
                            monitoring.ProvisionQualityConclusion.ForEach(x => monitoring.ProvisionQualityConclusionsDescription = Info.GetProvisionQualityConclusionTypeDescription(x) + ",");
                        }
                    }
                }
            }
            return monitoring;
        }

        internal static LoanMonitoringConclusion Set(SqlDataReader dr)
        {
            LoanMonitoringConclusion monitoring = new LoanMonitoringConclusion();
            monitoring.MonitoringId = long.Parse(dr["id"].ToString());
            monitoring.LoanProductId = long.Parse(dr["app_id"].ToString());
            monitoring.Conclusion = short.Parse(dr["conclusion"].ToString());
            monitoring.ConclusionDescription = Info.GetLoanMonitoringConclusionDescription(monitoring.Conclusion);
            monitoring.MonitoringType = short.Parse(dr["monitoring_type"].ToString());
            monitoring.MonitoringSubType = short.Parse(dr["monitoring_sub_type"].ToString());
            monitoring.MonitoringTypeDescription = Info.GetLoanMonitoringTypeDescription(monitoring.MonitoringType);
            monitoring.MonitoringDate = DateTime.Parse(dr["monitoring_Date"].ToString());
            monitoring.ProfitReduced = bool.Parse(dr["profit_reduced"].ToString());
            monitoring.ProfitReduceType = short.Parse(dr["profit_reduce_type"].ToString());
            monitoring.ProfitReduceTypeDescritpion = Info.GetProfitReductionTypeDescription(monitoring.ProfitReduceType);
            monitoring.ProfitReductionSize = float.Parse(dr["reduction_size"].ToString());
            monitoring.ProvisionCostConclusion = dr["provision_cost_conclusion"] == DBNull.Value ? (short)0 : short.Parse(dr["provision_cost_conclusion"].ToString());
            monitoring.ProvisionCostConclusionDescription = Info.GetProvisionCostConclusionTypeDescription(monitoring.ProvisionCostConclusion);
            monitoring.ProvisionCoverCoefficient = dr["provision_cover_coefficient"] == DBNull.Value ? (short)0 : float.Parse(dr["provision_cover_coefficient"].ToString());
            monitoring.Comments = dr["comments"].ToString();
            monitoring.RegistrationDate = DateTime.Parse(dr["registration_Date"].ToString());
            monitoring.MonitoringSetNumber = int.Parse(dr["monitoring_set_number"].ToString());
            monitoring.Status = short.Parse(dr["status"].ToString());
            monitoring.GetFactors();

            return monitoring;
        }

        internal static List<MonitoringConclusionFactor> GetFactors(long monitoringId)
        {
            List<MonitoringConclusionFactor> factors = new List<MonitoringConclusionFactor>();
            MonitoringConclusionFactor factor;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT factor_id FROM Tbl_loan_monitoring_conclusion_factors WHERE monitoring_id=@monitoringId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            factor = new MonitoringConclusionFactor();
                            factor.FactorId = Convert.ToInt16(dr["factor_id"].ToString());
                            factor.FactorDescription = Info.GetLoanMonitoringFactorDescription(factor.FactorId);
                            factors.Add(factor);
                        }
                    }

                }

                return factors;
            }
        }

        internal static List<short> GetProvisionQualityConclusion(long monitoringId, long productId)
        {
            List<short> conclusions = new List<short>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT provision_quality_conclusion FROM Tbl_loan_monitoring_provision_quality_conclusions WHERE monitoring_id=@monitoringId AND app_id=@productId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            conclusions.Add(Convert.ToInt16(dr["provision_quality_conclusion"].ToString()));
                        }
                    }
                }
                return conclusions;
            }
        }

        internal static List<MonitoringConclusionLinkedLoan> GetLinkedMonitoringLoans(long monitoringId)
        {
            List<MonitoringConclusionLinkedLoan> linkedLoans = new List<MonitoringConclusionLinkedLoan>();
            MonitoringConclusionLinkedLoan linkedLoan;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseconn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_get_monitoring_conclusion_linked_loans";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            linkedLoan = new MonitoringConclusionLinkedLoan();

                            linkedLoan.ProductId = long.Parse(dr["app_id"].ToString());
                            linkedLoan.CustomerNumber = long.Parse(dr["customer_number"].ToString());
                            linkedLoan.CustomerDescription = Utility.ConvertAnsiToUnicode(dr["customer_description"].ToString());
                            linkedLoan.StartCapital = double.Parse(dr["start_capital"].ToString());
                            linkedLoan.Currency = dr["currency"].ToString();
                            linkedLoan.StartDate = DateTime.Parse(dr["date_of_beginning"].ToString());
                            linkedLoan.EndDate = DateTime.Parse(dr["end_date"].ToString());
                            linkedLoan.Linked = bool.Parse(dr["linked"].ToString());
                            linkedLoan.MainProduct = bool.Parse(dr["main_product"].ToString());
                            linkedLoans.Add(linkedLoan);
                        }
                    }
                }
            }
            return linkedLoans;
        }


        internal static ActionResult SaveMonitoringConclusion(LoanMonitoringConclusion monitoring, int userId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseconn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_create_loan_monitoring_conclusion";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = monitoring.LoanProductId;
                    cmd.Parameters.Add("@conclusion", SqlDbType.SmallInt).Value = monitoring.Conclusion;
                    cmd.Parameters.Add("@monitoringType", SqlDbType.Float).Value = monitoring.MonitoringType;
                    cmd.Parameters.Add("@monitoringSubType", SqlDbType.Float).Value = monitoring.MonitoringSubType;
                    cmd.Parameters.Add("@monitoringDate", SqlDbType.SmallDateTime).Value = monitoring.MonitoringDate;
                    cmd.Parameters.Add("@monitoringSetNumber", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@profitReduced", SqlDbType.Bit).Value = monitoring.ProfitReduced;
                    cmd.Parameters.Add("@profitReduceType", SqlDbType.SmallInt).Value = monitoring.ProfitReduceType;
                    cmd.Parameters.Add("@reductionSize", SqlDbType.Float).Value = monitoring.ProfitReductionSize;
                    cmd.Parameters.Add("@provisionCostConclusion", SqlDbType.TinyInt).Value = monitoring.ProvisionCostConclusion;
                    cmd.Parameters.Add("@provisionCoverCoefficient", SqlDbType.Float).Value = monitoring.ProvisionCoverCoefficient;
                    cmd.Parameters.Add("@comments", SqlDbType.NVarChar, 2500).Value = monitoring.Comments;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = monitoring.RegistrationDate;
                    cmd.Parameters.Add("@registrationSetNumber", SqlDbType.Int).Value = userId;

                    SqlParameter fact = new SqlParameter("@loanMonitoringFactors", SqlDbType.Structured);
                    fact.Value = GetDetailsDataTable<MonitoringConclusionFactor>(monitoring.MonitoringFactors);
                    fact.TypeName = "loanMonitoringConclusionDetails";
                    cmd.Parameters.Add(fact);

                    SqlParameter prov = new SqlParameter("@provisionQualityConclusions", SqlDbType.Structured);
                    prov.Value = GetDetailsDataTable<short>(monitoring.ProvisionQualityConclusion);
                    prov.TypeName = "loanMonitoringConclusionDetails";
                    cmd.Parameters.Add(prov);

                    SqlParameter link = new SqlParameter("@linkedMonitoringLoans", SqlDbType.Structured);
                    link.Value = GetDetailsDataTable<MonitoringConclusionLinkedLoan>(monitoring.LinkedMonitoringLoans);
                    link.TypeName = "loanMonitoringConclusionDetails";
                    cmd.Parameters.Add(link);

                    SqlParameter param = new SqlParameter("@monitoringId", SqlDbType.BigInt);
                    param.Direction = ParameterDirection.InputOutput;
                    param.Value = monitoring.MonitoringId;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    monitoring.MonitoringId = Convert.ToInt64(cmd.Parameters["@monitoringId"].Value);
                    result.Id = monitoring.MonitoringId;
                }

            }
            return result;
        }

        internal static ActionResult ApproveMonitoringConclusion(long monitoringId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseconn"].ToString()))
            {
                string sql = @"UPDATE Tbl_loan_monitoring_conclusions SET status=2 WHERE id=@monitoringId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }


            }
            return result;
        }

        internal static ActionResult DeleteMonitoringConclusion(long monitoringId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseconn"].ToString()))
            {
                string sql = @"DELETE FROM Tbl_loan_monitoring_conclusions WHERE id=@monitoringId and status=1";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@monitoringId", SqlDbType.Float).Value = monitoringId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }


            }
            return result;
        }

        internal static DataTable GetDetailsDataTable<T>(List<T> details)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("app_id");
            dt.Columns.Add("provision_quality_conclusion");
            dt.Columns.Add("factor_id");
            dt.Columns.Add("linked");
            MonitoringConclusionFactor factor = new MonitoringConclusionFactor();
            MonitoringConclusionLinkedLoan linkedLoan = new MonitoringConclusionLinkedLoan();
            short qualityConclusion;
            if (details != null)
            {
                foreach (var det in details)
                {
                    if (det is MonitoringConclusionFactor)
                    {
                        factor = (MonitoringConclusionFactor)(object)det;
                        dt.Rows.Add(null, null, factor.FactorId, null);
                    }
                    if (det is MonitoringConclusionLinkedLoan)
                    {
                        linkedLoan = (MonitoringConclusionLinkedLoan)(object)det;
                        if (linkedLoan.MainProduct == false)
                        {
                            dt.Rows.Add(linkedLoan.ProductId, null, null, linkedLoan.Linked);
                        }

                    }
                    if (det is short)
                    {
                        qualityConclusion = (short)(object)det;
                        dt.Rows.Add(null, qualityConclusion, null, null);
                    }
                }
            }



            return dt;
        }

        internal static float GetProvisionCoverCoefficient(long productId)
        {
            float coefficient = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fnc_get_current_provision_cover_coefficient(@productId) coefficient";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            coefficient = float.Parse(dr["coefficient"].ToString()) * 100;
                        }
                    }
                }
                return coefficient;
            }
        }

        internal static List<MonitoringConclusionLinkedLoan> GetLinkedLoans(long productId, ulong customerNumber)
        {
            List<MonitoringConclusionLinkedLoan> linkedLoans = new List<MonitoringConclusionLinkedLoan>();
            MonitoringConclusionLinkedLoan linkedLoan;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseconn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_get_customer_linked_loan_products";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            linkedLoan = new MonitoringConclusionLinkedLoan();

                            linkedLoan.ProductId = long.Parse(dr["app_id"].ToString());
                            linkedLoan.CustomerNumber = long.Parse(dr["customer_number"].ToString());
                            linkedLoan.CustomerDescription = Utility.ConvertAnsiToUnicode(dr["customer_description"].ToString());
                            linkedLoan.StartCapital = double.Parse(dr["start_capital"].ToString());
                            linkedLoan.Currency = dr["currency"].ToString();
                            linkedLoan.StartDate = DateTime.Parse(dr["date_of_beginning"].ToString());
                            linkedLoan.EndDate = DateTime.Parse(dr["end_date"].ToString());
                            linkedLoans.Add(linkedLoan);
                        }
                    }
                }
            }

            return linkedLoans;
        }

        internal static short GetLoanMonitoringType(int departmentId)
        {
            short type = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fn_get_loan_monitoring_type(@departmentId) monitoring_type";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@departmentId", SqlDbType.Int).Value = departmentId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            type = short.Parse(dr["monitoring_type"].ToString());
                        }
                    }
                }
                return type;
            }
        }

        internal static bool IsSetProvisionConclusions(long monitoringId)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();

                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT  [dbo].[fn_is_set_provision_conclusions](@monitoringId) result";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@monitoringId", SqlDbType.VarChar, 50).Value = monitoringId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result = bool.Parse(dr["result"].ToString());
                }
            }
            return result;
        }


    }
}
