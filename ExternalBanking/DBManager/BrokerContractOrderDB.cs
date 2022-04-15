using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class BrokerContractOrderDB
    {
        /// <summary>
        /// Broker contract հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult SaveBrokerContractOrder(BrokerContractOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_save_broker_contract_order";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
            cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

            cmd.Parameters.Add("@contract_id", SqlDbType.NVarChar, 50).Value = order.ContractId;
            cmd.Parameters.Add("@education_id", SqlDbType.Int).Value = order.EducationId;
            cmd.Parameters.Add("@profession", SqlDbType.NVarChar, 150).Value = order.Profession;
            cmd.Parameters.Add("@activity_sphere_id", SqlDbType.Int).Value = order.ActivitySphereId;
            cmd.Parameters.Add("@stock_knowledge_id", SqlDbType.Int).Value = order.StockKnowledgeId;
            cmd.Parameters.Add("@risk_leaning_id", SqlDbType.Int).Value = order.RiskLeaningId;
            cmd.Parameters.Add("@financial_experience_id", SqlDbType.Int).Value = order.FinancialExperienceId;
            cmd.Parameters.Add("@financial_experience_duration", SqlDbType.Int).Value = order.FinancialExperienceDuration;
            cmd.Parameters.Add("@financial_experience_duration_type", SqlDbType.NVarChar, 20).Value = order.FinancialExperienceDurationType;
            cmd.Parameters.Add("@stock_tools_exp_duration_id", SqlDbType.Int).Value = order.StockToolsExpDurationId;
            cmd.Parameters.Add("@last_year_stock_order_count", SqlDbType.Int).Value = order.LastYearStockOrderCount;
            cmd.Parameters.Add("@one_order_avg", SqlDbType.Money).Value = order.OneOrderAvg;
            cmd.Parameters.Add("@stock_portfolio", SqlDbType.Int).Value = order.StockPortfolio;
            cmd.Parameters.Add("@financial_situation_Id", SqlDbType.Int).Value = order.FinancialSituationId;
            cmd.Parameters.Add("@last_year_profit", SqlDbType.Money).Value = order.LastYearProfit;
            cmd.Parameters.Add("@investment_purpose_id", SqlDbType.NVarChar, 150).Value = order.InvestmentPurposeId;
            cmd.Parameters.Add("@book_value_of_previous_year_of_asset_id", SqlDbType.Int).Value = order.BookValueOfPreviousYearOfAssetId;
            cmd.Parameters.Add("@last_year_sales_turnover_id", SqlDbType.Int).Value = order.LastYearSalesTurnoverId;
            cmd.Parameters.Add("@last_year_capital_id", SqlDbType.Int).Value = order.LastYearCapitalId;

            DataTable dtRef = GetStockToolDataTable(order.StockToolIds);
            SqlParameter sqlParam = cmd.Parameters.AddWithValue("@stock_Tool_Ids", dtRef);
            sqlParam.SqlDbType = SqlDbType.Structured;

            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });

            cmd.ExecuteNonQuery();

            result.ResultCode = ResultCode.Normal;
            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            result.Id = order.Id;
            order.Quality = OrderQuality.Draft;

            return result;
        }

        internal static async Task<bool> HasBrokerContract(ulong customerNumber)
        {
            bool hasBrokerContract = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                await conn.OpenAsync();

                using SqlCommand cmd = new SqlCommand(@"Select top 1 1 from tbl_broker_contract_products where customer_number=@customerNumber", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                if (dr.Read())
                {
                    hasBrokerContract = true;
                }
            }

            return hasBrokerContract;
        }

        internal static async Task<BrokerContractSurvey> GetBrokerContractSurvey(Languages language)
        {
            BrokerContractSurvey brokerContractSurvey = new BrokerContractSurvey();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand("pr_get_broker_contract_survey", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                await conn.OpenAsync();
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.StockKnowledges.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.RiskLeanings.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.FinancialExperiences.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.StockTools.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.StockExperiencDurations.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.FinancialSituations.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.Ocupations.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.Educations.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.InvestmentPurposes.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.BookValueOfPreviousYearOfAssets.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.LastYearSalesTurnovers.Add(SetBrokerContractSelection(dr, language));
                }

                await dr.NextResultAsync();
                while (await dr.ReadAsync())
                {
                    brokerContractSurvey.LastYearCapitals.Add(SetBrokerContractSelection(dr, language));
                }
            }
            return brokerContractSurvey;
        }



        private static BrokerContractSelection SetBrokerContractSelection(SqlDataReader dr, Languages language)
        {
            return new BrokerContractSelection
            {
                Id = int.Parse(dr["id"].ToString()),
                Description = language == Languages.hy ? Utility.ConvertAnsiToUnicode(dr["description_arm"].ToString()) : dr["description"].ToString()
            };
        }

        private static DataTable GetStockToolDataTable(List<int> stockToolIds)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("tool_id");

            if (stockToolIds.Any())
            {
                foreach (var item in stockToolIds)
                {
                    DataRow dr = dt.NewRow();
                    dr["tool_id"] = item;
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        internal static BrokerContractOrder GetBrokerContractOrder(BrokerContractOrder order)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT   hb.customer_number,
                                    hb.registration_date, 
                                    hb.document_number,
                                    hb.registration_date,
                                    hb.document_subtype,
                                    hb.quality,
                                    hb.currency, 
                                    hb.operation_date,
                                    hb.source_type,
                                    hb.document_type,
                                    hb.confirmation_date,
                                    D.*
                                        FROM Tbl_HB_documents hb
                                        INNER JOIN Tbl_broker_contract_details D
                                        ON hb.doc_ID = d.doc_id
                                        WHERE d.Doc_ID = @DocID
                                        AND hb.customer_number  = CASE WHEN @customer_number = 0 THEN hb.customer_number   ELSE @customer_number END";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.Type = (OrderType)dt.Rows[0]["document_type"];
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)dt.Rows[0]["quality"];
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                order.ContractId = dt.Rows[0]["contract_id"].ToString();
                order.EducationId = Convert.ToInt32(dt.Rows[0]["education_id"]);
                order.Profession = dt.Rows[0]["profession"].ToString();
                order.ActivitySphereId = Convert.ToInt32(dt.Rows[0]["activity_sphere_id"]);
                order.StockKnowledgeId = Convert.ToInt32(dt.Rows[0]["stock_knowledge_id"]);
                order.RiskLeaningId = Convert.ToInt32(dt.Rows[0]["risk_leaning_Id"]);
                order.FinancialExperienceId = Convert.ToInt32(dt.Rows[0]["financial_experience_id"]);
                order.FinancialExperienceDuration = dt.Rows[0]["financial_experience_duration"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["financial_experience_duration"]) : default(int?);
                order.FinancialExperienceDurationType = dt.Rows[0]["financial_experience_duration_type"].ToString();
                order.StockToolsExpDurationId = dt.Rows[0]["stock_tools_exp_duration_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["stock_tools_exp_duration_id"]) : default(int?);
                order.LastYearStockOrderCount = Convert.ToInt32(dt.Rows[0]["last_year_stock_order_count"]);
                order.OneOrderAvg = dt.Rows[0]["one_order_avg"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["one_order_avg"]) : 0;
                order.StockPortfolio = Convert.ToInt32(dt.Rows[0]["stock_portfolio"]);
                order.FinancialSituationId = Convert.ToInt32(dt.Rows[0]["financial_situation_Id"]);
                order.LastYearProfit = dt.Rows[0]["last_year_profit"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["last_year_profit"]) : 0;
                order.InvestmentPurposeId = Convert.ToInt32(dt.Rows[0]["investment_purpose_id"]);
                order.BookValueOfPreviousYearOfAssetId = dt.Rows[0]["book_value_of_previous_year_of_asset_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["book_value_of_previous_year_of_asset_id"]) : default(int?);
                order.LastYearSalesTurnoverId = dt.Rows[0]["last_year_sales_turnover_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["last_year_sales_turnover_id"]) : default(int?);
                order.LastYearCapitalId = dt.Rows[0]["last_year_capital_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["last_year_capital_id"]) : default(int?);
            }
            return order;
        }

        internal static async Task<BrokerContract> GetBrokerContractProduct(ulong customerNumber)
        {
            BrokerContract brokerContract = new BrokerContract();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(@"Select top 1  contract_number ,registration_date from tbl_broker_contract_products where customer_number=@customerNumber", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                if (dr.Read())
                {
                    brokerContract.ContractNumber = dr["contract_number"].ToString();
                    brokerContract.ContractDate = Convert.ToDateTime(dr["registration_date"]);
                }
            }
            return brokerContract;
        }

        internal static async Task<string> GetBrokerContractId(ulong customerNumber)
        {
            string brokerContractId = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                await conn.OpenAsync();
                using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1  d.[contract_id] FROM tbl_hb_documents hb 
                                                                                      INNER JOIN [Tbl_broker_contract_details] d
                                                                                      ON hb.doc_ID = d.doc_id 
                                                                                      WHERE hb.customer_number = @customerNumber AND hb.quality= 30 AND hb.document_type  = 263", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                if (dr.Read())
                    brokerContractId = dr["contract_id"].ToString();
            }
            return brokerContractId;
        }

    }
}
