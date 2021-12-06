using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class BondIssueDB
    {
        internal static BondIssue GetBondIssue(BondIssue bondissue)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT b.*, q.Description as quality_description, it.description as issuer_type_description, P.Description as PeriodTypeDescription
                                                  FROM tbl_bond_issue b 
                                                  INNER JOIN tbl_type_of_bond_issue_quality q ON b.quality = q.ID
                                                  INNER JOIN tbl_type_of_bond_issuer it ON b.issuer_type = it.id
                                                  LEFT JOIN Tbl_Type_of_Bond_Issues_by_Period P on b.issue_period_type = P.id
                                                  WHERE b.id = @bondId", conn);

                cmd.Parameters.Add("@bondId", SqlDbType.Int).Value = bondissue.ID;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    bondissue.ISIN = dt.Rows[0]["ISIN"].ToString();
                    bondissue.Currency = dt.Rows[0]["currency"].ToString();
                    bondissue.TotalVolume = Convert.ToDouble(dt.Rows[0]["total_volume"]);
                    bondissue.NominalPrice = dt.Rows[0]["nominal_price"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["nominal_price"]) : default(Double);

                    bondissue.InterestRate = dt.Rows[0]["interest_rate"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["interest_rate"]) : default(Double);

                    bondissue.RepaymentDate = dt.Rows[0]["repayment_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["repayment_date"]) : default;

                    bondissue.TotalCount = dt.Rows[0]["total_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["total_count"]) : default(Int32);
                    bondissue.MinSaleQuantity = dt.Rows[0]["min_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["min_sale_quantity"]) : default(Int32);
                    bondissue.MaxSaleQuantity = dt.Rows[0]["max_sale_quantity"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["max_sale_quantity"]) : default(Int32);
                    bondissue.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    bondissue.PurchaseDeadlineTime = dt.Rows[0]["purchase_deadline_time"] != DBNull.Value ? (TimeSpan)dt.Rows[0]["purchase_deadline_time"] : default(TimeSpan);
                    bondissue.Quality = (BondIssueQuality)Convert.ToInt16(dt.Rows[0]["quality"].ToString());
                    bondissue.QualityDescription = dt.Rows[0]["quality_description"].ToString();
                    bondissue.IssuerType = (BondIssuerType)Convert.ToInt32(dt.Rows[0]["issuer_type"].ToString());
                    bondissue.IssuerTypeDescription = dt.Rows[0]["issuer_type_description"].ToString();
                    bondissue.IssueDate = Convert.ToDateTime(dt.Rows[0]["issue_date"]);
                    bondissue.CouponPaymentCount = dt.Rows[0]["coupon_payment_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["coupon_payment_count"].ToString()) : default;
                    bondissue.PeriodType = dt.Rows[0]["issue_period_type"] != DBNull.Value ? (BondIssuePeriod)Convert.ToInt16(dt.Rows[0]["issue_period_type"].ToString()) : default;
                    bondissue.PeriodTypeDescription = dt.Rows[0]["PeriodTypeDescription"].ToString();

                    if (bondissue.IssuerType == BondIssuerType.ACBA)
                    {
                        bondissue.EditionCirculation = dt.Rows[0]["edition_circulation"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["edition_circulation"]) : default
                            ;
                        bondissue.ReplacementDate = Convert.ToDateTime(dt.Rows[0]["replacement_date"]);
                        bondissue.ReplacementEndDate = Convert.ToDateTime(dt.Rows[0]["replacement_end_date"]);
                        bondissue.CouponPaymentPeriodicity = dt.Rows[0]["coupon_payment_periodicity"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["coupon_payment_periodicity"]) : default;

                        bondissue.BankAccount = new Account
                        {
                            AccountNumber = dt.Rows[0]["bank_account_number"].ToString()
                        };
                    }

                    bondissue.ShareType = (SharesTypes)Convert.ToInt32(dt.Rows[0]["share_type"]);
                    bondissue.IssueSeria = dt.Rows[0]["issue_seria"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["issue_seria"]) : default;
                    bondissue.ReplacementFactualEndDate = dt.Rows[0]["replacement_factual_end_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["replacement_factual_end_date"]) : default;
                    bondissue.PlacementPrice = dt.Rows[0]["placement_price"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["placement_price"]) : default;
                    bondissue.PlacementFactualCount = dt.Rows[0]["placement_factual_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["placement_factual_count"]) : default;
                    bondissue.OperationDescription = dt.Rows[0]["operation_description"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["operation_description"]) : default;
                    bondissue.Description = dt.Rows[0]["description"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["description"]) : default;
                    bondissue.DecisionDate = dt.Rows[0]["decision_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["decision_date"]) : default;

                    bondissue.BankAccountForNonResident = new Account
                    {
                        AccountNumber = dt.Rows[0]["non_resident_bank_account_number"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["non_resident_bank_account_number"]) : default
                    };

                    bondissue.SetNumber = dt.Rows[0]["set_number"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["set_number"]) : default;
                }
            }
            return bondissue;
        }


        internal static ActionResult SaveBondIssue(BondIssue bondissue, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_bond_issue";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar, 500).Value = bondissue.ISIN;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = bondissue.Currency;
                    cmd.Parameters.Add("@total_volume", SqlDbType.Float).Value = bondissue.TotalVolume;
                    cmd.Parameters.Add("@nominal_price", SqlDbType.Money).Value = bondissue.NominalPrice;
                    cmd.Parameters.Add("@interest_rate", SqlDbType.Float).Value = bondissue.InterestRate / 100;

                    if(bondissue.RepaymentDate != null)
                        cmd.Parameters.Add("@repayment_date", SqlDbType.SmallDateTime).Value = bondissue.RepaymentDate;
                    else
                        cmd.Parameters.Add("@repayment_date", SqlDbType.SmallDateTime).Value = DBNull.Value;

                    cmd.Parameters.Add("@total_count", SqlDbType.Int).Value = bondissue.TotalCount;
                    cmd.Parameters.Add("@min_sale_quantity", SqlDbType.Int).Value = bondissue.MinSaleQuantity;
                    cmd.Parameters.Add("@max_sale_quantity", SqlDbType.Int).Value = bondissue.MaxSaleQuantity;
                    cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = bondissue.RegistrationDate;
                    cmd.Parameters.Add("@purchase_deadline_time", SqlDbType.Time, 7).Value = bondissue.PurchaseDeadlineTime;
                    cmd.Parameters.Add("@issuer_type", SqlDbType.Int).Value = bondissue.IssuerType;
                    cmd.Parameters.Add("@issue_date", SqlDbType.SmallDateTime).Value = bondissue.IssueDate;

                    if (bondissue.CouponPaymentCount != null)
                        cmd.Parameters.Add("@coupon_payment_count", SqlDbType.Int).Value = bondissue.CouponPaymentCount;
                    else
                        cmd.Parameters.Add("@coupon_payment_count", SqlDbType.Int).Value = DBNull.Value;

                    if (bondissue.PeriodType != BondIssuePeriod.None)
                        cmd.Parameters.Add("@issue_period_type", SqlDbType.TinyInt).Value = bondissue.PeriodType;
                    else
                        cmd.Parameters.Add("@issue_period_type", SqlDbType.Int).Value = DBNull.Value;

                    cmd.Parameters.Add("@quality", SqlDbType.TinyInt).Value = bondissue.Quality;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;

                    if (bondissue.IssuerType == BondIssuerType.ACBA)
                    {
                        cmd.Parameters.Add("@replacement_date", SqlDbType.SmallDateTime).Value = bondissue.ReplacementDate;
                        cmd.Parameters.Add("@replacement_end_date", SqlDbType.SmallDateTime).Value = bondissue.ReplacementEndDate;
                        cmd.Parameters.Add("@coupon_payment_periodicity", SqlDbType.Int).Value = bondissue.CouponPaymentPeriodicity;
                        cmd.Parameters.Add("@edition_circulation", SqlDbType.Int).Value = bondissue.EditionCirculation;
                        cmd.Parameters.Add("@bank_account_number", SqlDbType.Float).Value = bondissue.BankAccount.AccountNumber;
                        //ernest
                        if (bondissue.ShareType == SharesTypes.Bonds)
                        {
                            CouponRepaymentDateCollection couponRepaymentCollection = new CouponRepaymentDateCollection();
                            List<DateTime> couponRepaymentSchedule = bondissue.CalculateCouponRepaymentSchedule();

                            foreach (DateTime repaymnetDate in couponRepaymentSchedule)
                            {
                                CouponRepaymentScheduleItem item = new CouponRepaymentScheduleItem
                                {
                                    CouponRepaymentDate = repaymnetDate
                                };
                                couponRepaymentCollection.Add(item);
                            }

                            SqlParameter paramCouponRepaymentDates = new SqlParameter("@coupon_repayment_schedule", SqlDbType.Structured)
                            {
                                Value = couponRepaymentCollection,
                                TypeName = "dbo.CouponRepaymentDate"
                            };
                            cmd.Parameters.Add(paramCouponRepaymentDates);
                        }
                        else
                        {
                            SqlParameter paramCouponRepaymentDates = new SqlParameter("@coupon_repayment_schedule", SqlDbType.Structured)
                            {
                                Value = DBNull.Value,
                                TypeName = "dbo.CouponRepaymentDate"
                            };
                        }
                    }

                    if(bondissue.ShareType != SharesTypes.None)
                        cmd.Parameters.Add("@share_type", SqlDbType.Int).Value = (int)bondissue.ShareType;
                    else
                        cmd.Parameters.Add("@share_type", SqlDbType.Int).Value = DBNull.Value;

                    if (bondissue.IssueSeria != null)
                        cmd.Parameters.Add("@issue_seria", SqlDbType.Int).Value = bondissue.IssueSeria;
                    else
                        cmd.Parameters.Add("@issue_seria", SqlDbType.Int).Value = DBNull.Value;

                    if (bondissue.ReplacementFactualEndDate != null)
                        cmd.Parameters.Add("@replacement_factual_end_date", SqlDbType.SmallDateTime).Value = bondissue.ReplacementFactualEndDate;
                    else
                        cmd.Parameters.Add("@replacement_factual_end_date", SqlDbType.SmallDateTime).Value = DBNull.Value;

                    if (bondissue.BankAccountForNonResident != null && !string.IsNullOrEmpty(bondissue.BankAccountForNonResident.AccountNumber))
                        cmd.Parameters.Add("@non_resident_bank_account_number", SqlDbType.Float).Value = bondissue.BankAccountForNonResident.AccountNumber;
                    else
                        cmd.Parameters.Add("@non_resident_bank_account_number", SqlDbType.Float).Value = DBNull.Value;

                    if (bondissue.PlacementPrice != null)
                        cmd.Parameters.Add("@placement_price", SqlDbType.Money).Value = bondissue.PlacementPrice;
                    else
                        cmd.Parameters.Add("@placement_price", SqlDbType.Money).Value = DBNull.Value;

                    if (!string.IsNullOrEmpty(bondissue.OperationDescription))
                        cmd.Parameters.Add("@operation_description", SqlDbType.NVarChar).Value = bondissue.OperationDescription;
                    else
                        cmd.Parameters.Add("@operation_description", SqlDbType.NVarChar).Value = DBNull.Value;

                    if (bondissue.DecisionDate != null)
                        cmd.Parameters.Add("@decision_date", SqlDbType.SmallDateTime).Value = bondissue.DecisionDate;
                    else
                        cmd.Parameters.Add("@decision_date", SqlDbType.SmallDateTime).Value = DBNull.Value;

                    if (!string.IsNullOrEmpty(bondissue.Description))
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = bondissue.Description;
                    else
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = DBNull.Value;

                    if (bondissue.PlacementFactualCount != null)
                        cmd.Parameters.Add("@placement_factual_count", SqlDbType.Int).Value = bondissue.PlacementFactualCount;
                    else
                        cmd.Parameters.Add("@placement_factual_count", SqlDbType.Int).Value = DBNull.Value;

                    if(bondissue.SetNumber != 0)
                        cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = bondissue.SetNumber;
                    else
                        cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = DBNull.Value;

                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = bondissue.ID });

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                    bondissue.ID = id;

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
            }

            return result;
        }


        internal static ActionResult DeleteBondIssue(BondIssue bondissue)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE Tbl_bond_issue SET quality = @quality WHERE id = @bondIssueId ";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = bondissue.ID;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = BondIssueQuality.Deleted;
                    cmd.ExecuteNonQuery();
                }

                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }

        internal static ActionResult ApproveBondIssue(BondIssue bondissue)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE Tbl_bond_issue SET quality = @quality WHERE id = @bondIssueId ";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = bondissue.ID;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = BondIssueQuality.Approved;
                    cmd.ExecuteNonQuery();
                }

                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }

        internal static ActionResult SaveBondIssueHistory(int bondIssueId, Action action, int setNumber)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_insert_bond_issue_history";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = bondIssueId;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@actionSetNumber", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });



                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);



                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }

                }
            }

            return result;
        }

        internal static List<DateTime> GetCouponRepaymentSchedule(int bondIssueId)
        {
            List<DateTime> schedule = new List<DateTime>();


            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT id, coupon_repayment_date FROM Tbl_Coupon_Repayment_Schedule 
                                                  WHERE bond_issue_id = @bondIssueId", conn);

                cmd.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = bondIssueId;
                dt.Load(cmd.ExecuteReader());

                foreach (DataRow row in dt.Rows)
                {
                    schedule.Add(Convert.ToDateTime(row["coupon_repayment_date"]));
                }

            }

            return schedule;
        }

        internal static bool GetCheckISINandSeria(string isin, int seria)
        {
            bool exists = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM [dbo].[Tbl_bond_issue] WHERE [ISIN] = @isin AND [issue_seria] = @issue_seria", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@isin", SqlDbType.NVarChar).Value = isin;
                cmd.Parameters.Add("@issue_seria", SqlDbType.Int).Value = seria;
               using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    exists = true;
                }

            }

            return exists;

        }

        internal static bool CheckBondIssueReplacementDate(int bondIssueId)
        {
            bool result = false;
            DateTime dtNow = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT [replacement_end_date] FROM [dbo].[Tbl_bond_issue] WHERE [id] = @id", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = bondIssueId;
               using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if(dtNow <= Convert.ToDateTime(reader["replacement_end_date"]).AddHours(16).AddMinutes(30))
                            result = true;
                    }                    
                }

            }

            return result;

        }

    }

}




