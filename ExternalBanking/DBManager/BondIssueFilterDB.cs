using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class BondIssueFilterDB
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
                    sql = @"SELECT B.*, Q.[Description] , it.description as issuer_type_description, P.Description as PeriodTypeDescription
                                  FROM Tbl_bond_issue B 
                                        INNER JOIN tbl_type_of_bond_issue_quality Q ON B.quality = Q.id 
			                            INNER JOIN tbl_type_of_bond_issuer it ON b.issuer_type = it.id
                                        INNER JOIN Tbl_Type_of_Bond_Issues_by_Period P on b.issue_period_type = P.id
                                  WHERE B.quality <> 99 ";


                    if (searchParams.BondIssueId != 0)
                    {
                        sql = sql + " and b.id = @bondIssueId ";
                        cmd.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = searchParams.BondIssueId;
                    }
                    if (!string.IsNullOrEmpty(searchParams.ISIN))
                    {
                        sql = sql + " and ISIN = @ISIN ";
                        cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = searchParams.ISIN;
                    }

                    if (!string.IsNullOrEmpty(searchParams.Currency))
                    {
                        sql = sql + " and currency = @currency" ;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = searchParams.Currency;
                    }

                    if (searchParams.StartDate != default(DateTime))
                    {
                        sql = sql + " and registration_date >= @StartDate ";
                        cmd.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = searchParams.StartDate;
                    }

                    if (searchParams.EndDate != default(DateTime))
                    {
                        sql = sql + " and registration_date <= @EndDate ";
                        cmd.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = searchParams.EndDate;
                    }

                    if(searchParams.Quality != BondIssueQuality.None)
                    {
                        sql = sql + " and quality = @quality ";
                        cmd.Parameters.Add("@quality", SqlDbType.Int).Value = searchParams.Quality;
                    }

                    if (searchParams.IssuerType != BondIssuerType.None)
                    {
                        sql = sql + " and issuer_type = @issuer_type ";
                        cmd.Parameters.Add("@issuer_type", SqlDbType.Int).Value = searchParams.IssuerType;
                    }

                    sql = sql + " ORDER BY B.id desc ";

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
                            BondIssue result = new BondIssue();
                      
                            result.ISIN = dr["ISIN"].ToString();
                            result.Currency = dr["currency"].ToString();
                            result.ID = Convert.ToInt32(dr["id"]);
                            result.TotalVolume = Convert.ToDouble(dr["total_volume"]);
                            result.NominalPrice = dr["nominal_price"] != DBNull.Value ? Convert.ToDouble(dr["nominal_price"]) : default(Double);                           
                            result.InterestRate = dr["interest_rate"] != DBNull.Value ? Convert.ToDouble(dr["interest_rate"]) : default(Double);
                            result.RepaymentDate = Convert.ToDateTime(dr["repayment_date"]);                   
                            result.TotalCount = dr["total_count"] != DBNull.Value ? Convert.ToInt32(dr["total_count"]) : default(Int32);
                            result.MinSaleQuantity = dr["min_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dr["min_sale_quantity"]) : default(Int32);
                            result.MaxSaleQuantity = dr["max_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dr["max_sale_quantity"]) : default(Int32);
                            result.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            result.PurchaseDeadlineTime = dr["purchase_deadline_time"]!= DBNull.Value ? (TimeSpan)(dr["purchase_deadline_time"]) : default(TimeSpan) ;
                            result.Quality = (BondIssueQuality)(Convert.ToInt16(dr["quality"].ToString()));
                            result.QualityDescription = (dr["Description"]).ToString();
                            result.RegistrationDate = Convert.ToDateTime((dr["registration_date"]));
                            result.IssuerType = (BondIssuerType)Convert.ToInt32(dr["issuer_type"].ToString());
                            result.IssuerTypeDescription = dr["issuer_type_description"].ToString();
                            result.PeriodType = (BondIssuePeriod)Convert.ToInt16(dr["issue_period_type"].ToString());
                            result.PeriodTypeDescription = dr["PeriodTypeDescription"].ToString();
                            result.IssueDate = Convert.ToDateTime(dr["issue_date"]);


                            if (result.IssuerType == BondIssuerType.ACBA)
                            {
                                result.EditionCirculation = Convert.ToInt32(dr["edition_circulation"]);
                                result.ReplacementDate = Convert.ToDateTime(dr["replacement_date"]);
                                result.ReplacementEndDate = Convert.ToDateTime(dr["replacement_end_date"]);
                                result.CouponPaymentPeriodicity = Convert.ToInt32(dr["coupon_payment_periodicity"]);
                                result.BankAccount = new Account();
                                result.BankAccount.AccountNumber = dr["bank_account_number"].ToString();


                            }

                            bondIssueList.Add(result);
                        }
                    }
                }

                return bondIssueList;
            }
        }


    }
}
