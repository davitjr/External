using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class BondIssueFilterDB
    {
        /// <summary>
        /// Պարտատոմսերի թողարկման որոնում նշված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        internal static List<BondIssue> SearchBondIssue(BondIssueFilter searchParams)
        {
            List<BondIssue> bondIssueList = new List<BondIssue>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    sql = @"SELECT B.*, Q.[Description] , it.description as issuer_type_description, P.Description as PeriodTypeDescription,
                                  ISNULL((SELECT SUM([bond_count]) FROM [dbo].[Tbl_bonds] WHERE [bond_issue_id] = B.ID and quality not in (21, 22, 31,41) GROUP BY [bond_issue_id]),0) AS distributed_bonds_count
                                  FROM Tbl_bond_issue B 
                                        INNER JOIN tbl_type_of_bond_issue_quality Q ON B.quality = Q.id 
			                            INNER JOIN tbl_type_of_bond_issuer it ON b.issuer_type = it.id
                                        LEFT JOIN Tbl_Type_of_Bond_Issues_by_Period P on b.issue_period_type = P.id
                                  WHERE B.quality <> 99 ";

                    if (searchParams.BondIssueId != 0)
                    {
                        sql += " and b.id = @bondIssueId ";
                        cmd.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = searchParams.BondIssueId;
                    }

                    if (!string.IsNullOrEmpty(searchParams.ISIN))
                    {
                        sql += " and ISIN = @ISIN ";
                        cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = searchParams.ISIN;
                    }

                    if (!string.IsNullOrEmpty(searchParams.Currency))
                    {
                        sql += " and currency = @currency";
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = searchParams.Currency;
                    }

                    if (searchParams.StartDate != default)
                    {
                        sql += " and registration_date >= @StartDate ";
                        cmd.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = searchParams.StartDate;
                    }

                    if (searchParams.EndDate != default)
                    {
                        sql += " and registration_date <= @EndDate ";
                        cmd.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = searchParams.EndDate;
                    }

                    if (searchParams.Quality != BondIssueQuality.None)
                    {
                        sql += " and quality = @quality ";
                        cmd.Parameters.Add("@quality", SqlDbType.Int).Value = searchParams.Quality;
                    }

                    if (searchParams.IssuerType != BondIssuerType.None)
                    {
                        sql += " and issuer_type = @issuer_type ";
                        cmd.Parameters.Add("@issuer_type", SqlDbType.Int).Value = searchParams.IssuerType;
                    }

                    if (searchParams.ShareType != SharesTypes.None)
                    {
                        sql += " and [share_type] = @share_type ";
                        cmd.Parameters.Add("@share_type", SqlDbType.Int).Value = (int)searchParams.ShareType;
                    }

                    if (searchParams.IssueSeria != null && searchParams.IssueSeria != 0)
                    {
                        sql += " and [issue_seria] = @issue_seria ";
                        cmd.Parameters.Add("@issue_seria", SqlDbType.Int).Value = searchParams.IssueSeria;
                    }

                    sql += " ORDER BY B.id desc ";

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            bondIssueList = new List<BondIssue>();
                        }

                        while (dr.Read())
                        {
                            BondIssue result = new BondIssue
                            {
                                ISIN = dr["ISIN"].ToString(),
                                Currency = dr["currency"].ToString(),
                                ID = Convert.ToInt32(dr["id"]),
                                TotalVolume = Convert.ToDouble(dr["total_volume"]),
                                NominalPrice = dr["nominal_price"] != DBNull.Value ? Convert.ToDouble(dr["nominal_price"]) : default,
                                InterestRate = dr["interest_rate"] != DBNull.Value ? Convert.ToDouble(dr["interest_rate"]) : default,
                                RepaymentDate = dr["repayment_date"] != DBNull.Value ? Convert.ToDateTime(dr["repayment_date"]) : default,
                                TotalCount = dr["total_count"] != DBNull.Value ? Convert.ToInt32(dr["total_count"]) : default,
                                MinSaleQuantity = dr["min_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dr["min_sale_quantity"]) : default,
                                MaxSaleQuantity = dr["max_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dr["max_sale_quantity"]) : default,
                                RegistrationDate = Convert.ToDateTime(dr["registration_date"]),
                                PurchaseDeadlineTime = dr["purchase_deadline_time"] != DBNull.Value ? (TimeSpan)(dr["purchase_deadline_time"]) : default,
                                Quality = (BondIssueQuality)(Convert.ToInt16(dr["quality"].ToString())),
                                QualityDescription = (dr["Description"]).ToString(),
                                IssuerType = (BondIssuerType)Convert.ToInt32(dr["issuer_type"].ToString()),
                                IssuerTypeDescription = dr["issuer_type_description"].ToString(),
                                PeriodType = dr["issue_period_type"] != DBNull.Value ? (BondIssuePeriod)Convert.ToInt16(dr["issue_period_type"].ToString()) : default,
                                PeriodTypeDescription = dr["PeriodTypeDescription"].ToString(),
                                IssueDate = Convert.ToDateTime(dr["issue_date"]),
                                ShareType = dr["share_type"] != DBNull.Value ? (SharesTypes)Convert.ToInt32(dr["share_type"]) : SharesTypes.None,
                                IssueSeria = dr["issue_seria"] != DBNull.Value ? Convert.ToInt32(dr["issue_seria"]) : default,
                                ReplacementFactualEndDate = dr["replacement_factual_end_date"] != DBNull.Value ? Convert.ToDateTime(dr["replacement_factual_end_date"]) : default,
                                PlacementPrice = dr["placement_price"] != DBNull.Value ? Convert.ToInt32(dr["placement_price"]) : default,
                                PlacementFactualCount = dr["placement_factual_count"] != DBNull.Value ? Convert.ToInt32(dr["placement_factual_count"]) : default,
                                OperationDescription = dr["operation_description"] != DBNull.Value ? Convert.ToString(dr["operation_description"]) : default,
                                Description = dr["description"] != DBNull.Value ? Convert.ToString(dr["description"]) : default,
                                DecisionDate = dr["decision_date"] != DBNull.Value ? Convert.ToDateTime(dr["decision_date"]) : default,
                                BankAccountForNonResident = new Account
                                {
                                    AccountNumber = dr["non_resident_bank_account_number"] != DBNull.Value ? Convert.ToString(dr["non_resident_bank_account_number"]) : default
                                },
                                SetNumber = dr["set_number"] != DBNull.Value ? Convert.ToInt32(dr["set_number"]) : default
                            };

                            if (result.IssuerType == BondIssuerType.ACBA)
                            {
                                result.EditionCirculation = dr["edition_circulation"] != DBNull.Value ? Convert.ToInt32(dr["edition_circulation"]) : default;
                                result.ReplacementDate = Convert.ToDateTime(dr["replacement_date"]);
                                result.ReplacementEndDate = Convert.ToDateTime(dr["replacement_end_date"]);
                                result.CouponPaymentPeriodicity = dr["coupon_payment_periodicity"] != DBNull.Value ? Convert.ToInt32(dr["coupon_payment_periodicity"]) : default;
                                result.BankAccount = new Account
                                {
                                    AccountNumber = dr["bank_account_number"].ToString()
                                };
                            }

                            result.NonDistributedBondsCount = (result.TotalCount - Convert.ToInt32(dr["distributed_bonds_count"])) < 0 ? 0 : result.TotalCount - Convert.ToInt32(dr["distributed_bonds_count"]);
                            bondIssueList.Add(result);
                        }
                    }
                }

                return bondIssueList;
            }
        }
    }
}
