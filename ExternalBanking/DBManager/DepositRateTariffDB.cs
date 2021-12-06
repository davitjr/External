using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class DepositRateTariffDB
    {

        internal static List<DepositRateTariffItem> GetDepositTariffList(DepositType depositType)
        {
            List<DepositRateTariffItem> depositRateTariffs = new List<DepositRateTariffItem>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"DECLARE @max_Date smallDateTime
                                    select @max_date = MAX(date_of_beginning)
                                    FROM Tbl_deposit_product_history

                                    SELECT DISTINCT D.type_of_client, D.product_code, D.currency,
                                     CASE WHEN D.product_code = 6 THEN 0 ELSE D.period_in_months_min END period_in_months_min, 
                                     CASE WHEN D.product_code = 6 THEN 0 ELSE D.period_in_months_max END period_in_months_max, 
                                     @max_Date, interest_rate, H.bonus_interest_rate_for_HB, H.bonus_interest_rate_for_employee
                                     FROM Tbl_deposit_product_history H inner join
                                    (SELECT type_of_client,product_code, currency, period_in_months_min,period_in_months_max
                                    FROM Tbl_deposit_product_history
                                    group by product_code, currency, period_in_months_max, period_in_months_min, type_of_client
                                    having  product_code = @productCode and type_of_client = 			
									CASE
										WHEN @productCode <> 15 THEN 6
										ELSE 2
									END
									) D ON H.product_code = D.product_code and
                                    H.currency = D.currency and H.period_in_months_min = D.period_in_months_min and H.period_in_months_max=
                                    D.period_in_months_max and H.date_of_beginning = @max_Date and H.type_of_client = D.type_of_client";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productCode", SqlDbType.Float).Value = (short)depositType;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        depositRateTariffs = new List<DepositRateTariffItem>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DepositRateTariffItem depositRateTariffItem = new DepositRateTariffItem();
                        depositRateTariffItem.Currency = dt.Rows[i]["currency"].ToString();
                        depositRateTariffItem.BonusInterestRateForHB = Convert.ToDouble(dt.Rows[i]["bonus_interest_rate_for_HB"].ToString());
                        depositRateTariffItem.BonusInterestRateForEmployee = Convert.ToDouble(dt.Rows[i]["bonus_interest_rate_for_employee"].ToString());
                        if (depositType != DepositType.DepositAccumulative)
                        {
                            depositRateTariffItem.PeriodInMonthsMin = Convert.ToInt16(dt.Rows[i]["period_in_months_min"].ToString());
                            depositRateTariffItem.PeriodInMonthsMax = Convert.ToInt16(dt.Rows[i]["period_in_months_max"].ToString());

                        }
                        else
                        {
                            depositRateTariffItem.PeriodInMonthsMin = null;
                            depositRateTariffItem.PeriodInMonthsMax = null;

                        }


                        depositRateTariffItem.InterestRate = Convert.ToDouble(dt.Rows[i]["interest_rate"].ToString());
                        depositRateTariffs.Add(depositRateTariffItem);

                    }


                }
            }

            return depositRateTariffs;
        }


        internal static DataTable GetDepositRateTariff(DepositType depositType)
        {
            
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select a.code,a.description,b.description as FreqDescr,b.description_eng as FreqDescr_eng,
                        rate_repayment_frequency from [Tbl_type_of_deposits;] a left  join Tbl_Type_of_deposit_repayment_frequency  b 
                                     on a.rate_repayment_frequency = b.id where code  = @depositType", conn);

              
                cmd.Parameters.Add("@depositType", SqlDbType.Float).Value = (short)depositType;
                dt.Load(cmd.ExecuteReader());

                
            }
            return dt;
        }

    }
}
