using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ExternalBanking.DBManager
{
    class DepositProductPricesDB
    {
        internal static ActionResult ConfirmOrRejectDepositProductPrices(string listOfId, int confirmationSetNumber, byte status, string rejectionDescription)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "pr_confirm_or_reject_deposit_prices";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add("@list_of_id", SqlDbType.VarChar, 100).Value = listOfId;
                cmd.Parameters.Add("@confirmation_set_number", SqlDbType.Int).Value = confirmationSetNumber;
                cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
                cmd.Parameters.Add("@rejection_description", SqlDbType.Int).Value = rejectionDescription;

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    result.ResultCode = ResultCode.Normal;
                }
                reader.Close();
            }
            return result;
        }
        internal static ActionResult DeleteDepositProductPrices(int id, int registrationSetNumber)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "pr_delete_from_intermediate_deposit_product_history";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@registration_set_number", SqlDbType.Int).Value = registrationSetNumber;

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    result.ResultCode = ResultCode.Normal;
                }
                reader.Close();
            }
            return result;
        }

        /*internal static ActionResult InsertDepositProductPrices(byte productCode, string currency, float interestRate, DateTime dateOfBeginning,int periodInMonthsMin, int periodInMonthsMax,
                                        float minAmount, float bonusInterestRate, float bonusInterestRateForHB,float bonusInterestRateForEmployee, float minAmountJur, short typeOfClient,
                                        float bonusInterestRateForRepaymentType,float interestRateForAllowAddition, float interestRateForAllowDecreasing,
                                        float interestRateForAllowAdditionAndDecreasing,float bonusInterestRateForClassic, float bonusInterestRateForAvangard, float bonusInterestRateForPremium, 
                                        float maxAdditionPercent,float maxDecreasingPercent, int registrationSetNumber)*/
        internal static ActionResult AddDepositProductPrices(DepositProductPrices product)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "pr_insert_into_intermediate_deposit_product_history";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add("@product_code", SqlDbType.TinyInt).Value = product.ProductCode;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = product.Currency;
                cmd.Parameters.Add("@interest_rate", SqlDbType.Float).Value = product.InterestRate;
                cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = product.DateOfBeginning;
                cmd.Parameters.Add("@period_in_months_min", SqlDbType.Int).Value = product.PeriodInMonthsMin;
                cmd.Parameters.Add("@period_in_months_max", SqlDbType.Int).Value = product.PeriodInMonthsMax;
                cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = product.MinAmount;
                cmd.Parameters.Add("@bonus_interest_rate", SqlDbType.Float).Value = product.BonusInterestRate;
                cmd.Parameters.Add("@bonus_interest_rate_for_HB", SqlDbType.Float).Value = product.BonusInterestRateForHB;
                cmd.Parameters.Add("@bonus_interest_rate_for_employee", SqlDbType.Float).Value = product.BonusInterestRateForEmployee;
                cmd.Parameters.Add("@min_amount_jur", SqlDbType.Float).Value = product.MinAmountJur;
                cmd.Parameters.Add("@type_of_client", SqlDbType.SmallInt).Value = product.TypeOfClient;
                cmd.Parameters.Add("@bonus_interest_rate_for_repayment_type", SqlDbType.Float).Value = product.BonusInterestRateForRepaymentType;
                cmd.Parameters.Add("@interest_rate_for_allow_addition", SqlDbType.Float).Value = product.InterestRateForAllowAddition;
                cmd.Parameters.Add("@interest_rate_for_allow_decreasing", SqlDbType.Float).Value = product.InterestRateForAllowDecreasing;
                cmd.Parameters.Add("@interest_rate_for_allow_addition_and_decreasing", SqlDbType.Float).Value = product.InterestRateForAllowAdditionAndDecreasing;
                cmd.Parameters.Add("@bonus_interest_rate_for_classic", SqlDbType.Float).Value = product.BonusInterestRateForClassic;
                cmd.Parameters.Add("@bonus_interest_rate_for_avangard", SqlDbType.Float).Value = product.BonusInterestRateForAvangard;
                cmd.Parameters.Add("@bonus_interest_rate_for_premium", SqlDbType.Float).Value = product.BonusInterestRateForPremium;
                cmd.Parameters.Add("@max_addition_percent", SqlDbType.Float).Value = product.MaxAdditionPercent;
                cmd.Parameters.Add("@max_decreasing_percent", SqlDbType.Float).Value = product.MaxDecreasingPercent;
                cmd.Parameters.Add("@registration_set_number", SqlDbType.Float).Value = product.RegistrationSetNumber;

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    result.ResultCode = ResultCode.Normal;
                }
                reader.Close();

                //cmd.ExecuteNonQuery();
            }
            return result;
        }

        internal static List<DepositProductPrices> GetDepositProductPrices(SearchDepositProductPrices searchProduct)
        {
            List<DepositProductPrices> products = new List<DepositProductPrices>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_select_from_intermediate_deposit_product";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = searchProduct.Status;
                    cmd.Parameters.Add("@product_code", SqlDbType.TinyInt).Value = searchProduct.ProductCode;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = searchProduct.Currency;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = searchProduct.DateOfBeginning;
                    cmd.Parameters.Add("@period_in_months_min", SqlDbType.Int).Value = searchProduct.PeriodInMonthsMin;
                    cmd.Parameters.Add("@period_in_months_max", SqlDbType.Int).Value = searchProduct.PeriodInMonthsMax;
                    cmd.Parameters.Add("@type_of_client", SqlDbType.SmallInt).Value = searchProduct.TypeOfClient;

                    //cmd.ExecuteNonQuery();

                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DepositProductPrices dp = new DepositProductPrices();

                            if (reader["id"] != DBNull.Value)
                                dp.ID = Convert.ToInt32(reader["id"]);
                            if (reader["product_code"] != DBNull.Value)
                                dp.ProductCode = Convert.ToByte(reader["product_code"]);
                            if (reader["currency"] != DBNull.Value)
                                dp.Currency = reader["currency"].ToString();
                            if (reader["interest_rate"] != DBNull.Value)
                                dp.InterestRate = Convert.ToSingle(reader["interest_rate"]);
                            if (reader["date_of_beginning"] != DBNull.Value)
                                dp.DateOfBeginning = Convert.ToDateTime(reader["date_of_beginning"]);
                            if (reader["period_in_months_min"] != DBNull.Value)
                                dp.PeriodInMonthsMin = Convert.ToInt32(reader["period_in_months_min"]);
                            if (reader["period_in_months_max"] != DBNull.Value)
                                dp.PeriodInMonthsMax = Convert.ToInt32(reader["period_in_months_max"]);
                            if (reader["min_amount"] != DBNull.Value)
                                dp.MinAmount = Convert.ToSingle(reader["min_amount"]);
                            if (reader["bonus_interest_rate"] != DBNull.Value)
                                dp.BonusInterestRate = Convert.ToSingle(reader["bonus_interest_rate"]);
                            if (reader["bonus_interest_rate_for_HB"] != DBNull.Value)
                                dp.BonusInterestRateForHB = Convert.ToSingle(reader["bonus_interest_rate_for_HB"]);
                            if (reader["bonus_interest_rate_for_employee"] != DBNull.Value)
                                dp.BonusInterestRateForEmployee = Convert.ToSingle(reader["bonus_interest_rate_for_employee"]);
                            if (reader["min_amount_jur"] != DBNull.Value)
                                dp.MinAmountJur = Convert.ToSingle(reader["min_amount_jur"]);
                            if (reader["min_amount_jur"] != DBNull.Value)
                                dp.TypeOfClient = Convert.ToInt16(reader["min_amount_jur"]);
                            if (reader["bonus_interest_rate_for_repayment_type"] != DBNull.Value)
                                dp.BonusInterestRateForRepaymentType = Convert.ToSingle(reader["bonus_interest_rate_for_repayment_type"]);
                            if (reader["interest_rate_for_allow_addition"] != DBNull.Value)
                                dp.InterestRateForAllowAddition = Convert.ToSingle(reader["interest_rate_for_allow_addition"]);
                            if (reader["interest_rate_for_allow_decreasing"] != DBNull.Value)
                                dp.InterestRateForAllowDecreasing = Convert.ToSingle(reader["interest_rate_for_allow_decreasing"]);
                            if (reader["interest_rate_for_allow_addition_and_decreasing"] != DBNull.Value)
                                dp.InterestRateForAllowAdditionAndDecreasing = Convert.ToSingle(reader["interest_rate_for_allow_addition_and_decreasing"]);
                            if (reader["bonus_interest_rate_for_classic"] != DBNull.Value)
                                dp.BonusInterestRateForClassic = Convert.ToSingle(reader["bonus_interest_rate_for_classic"]);
                            if (reader["bonus_interest_rate_for_avangard"] != DBNull.Value)
                                dp.BonusInterestRateForAvangard = Convert.ToSingle(reader["bonus_interest_rate_for_avangard"]);
                            if (reader["bonus_interest_rate_for_premium"] != DBNull.Value)
                                dp.BonusInterestRateForPremium = Convert.ToSingle(reader["bonus_interest_rate_for_premium"]);
                            if (reader["max_addition_percent"] != DBNull.Value)
                                dp.MaxAdditionPercent = Convert.ToSingle(reader["max_addition_percent"]);
                            if (reader["max_decreasing_percent"] != DBNull.Value)
                                dp.MaxDecreasingPercent = Convert.ToSingle(reader["max_decreasing_percent"]);
                            if (reader["registration_set_number"] != DBNull.Value)
                                dp.RegistrationSetNumber = Convert.ToInt32(reader["registration_set_number"]);
                            if (reader["registration_date"] != DBNull.Value)
                                dp.RegistrationDate = Convert.ToDateTime(reader["registration_date"]);
                            if (reader["confirmation_set_number"] != DBNull.Value)
                                dp.ConfirmationSetNumber = Convert.ToInt32(reader["confirmation_set_number"]);
                            if (reader["confirmation_date"] != DBNull.Value)
                                dp.ConfirmationDate = Convert.ToDateTime(reader["confirmation_date"]);
                            if (reader["status"] != DBNull.Value)
                                dp.Status = Convert.ToByte(reader["status"]);
                            if (reader["rejection_description"] != DBNull.Value)
                                dp.RejectionDescription = Convert.ToString(reader["rejection_description"]);

                            products.Add(dp);
                        }
                    }
                    reader.Close();
                }
            }
            return products;
        }

        /*internal static ActionResult UpdateDepositPrices(int id,byte productCode, string currency, float interestRate, DateTime dateOfBeginning,
                                        int periodInMonthsMin, int periodInMonthsMax, float minAmount, float bonusInterestRate, float bonusInterestRateForHB,
                                        float bonusInterestRateForEmployee, float minAmountJur, short typeOfClient, float bonusInterestRateForRepaymentType,
                                        float interestRateForAllowAddition, float interestRateForAllowDecreasing, float interestRateForAllowAdditionAndDecreasing,
                                        float bonusInterestRateForClassic, float bonusInterestRateForAvangard, float bonusInterestRateForPremium, float maxAdditionPercent,
                                        float maxDecreasingPercent, int registrationSetNumber)*/
        internal static ActionResult UpdateDepositPrices(DepositProductPrices product)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_update_data_in_intermediate_deposit_product";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@id", SqlDbType.TinyInt).Value = product.ID;
                    cmd.Parameters.Add("@product_code", SqlDbType.TinyInt).Value = product.ProductCode;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = product.Currency;
                    cmd.Parameters.Add("@interest_rate", SqlDbType.Float).Value = product.InterestRate;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = product.DateOfBeginning;
                    cmd.Parameters.Add("@period_in_months_min", SqlDbType.Int).Value = product.PeriodInMonthsMin;
                    cmd.Parameters.Add("@period_in_months_max", SqlDbType.Int).Value = product.PeriodInMonthsMax;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = product.MinAmount;
                    cmd.Parameters.Add("@bonus_interest_rate", SqlDbType.Float).Value = product.BonusInterestRate;
                    cmd.Parameters.Add("@bonus_interest_rate_for_HB", SqlDbType.Float).Value = product.BonusInterestRateForHB;
                    cmd.Parameters.Add("@bonus_interest_rate_for_employee", SqlDbType.Float).Value = product.BonusInterestRateForEmployee;
                    cmd.Parameters.Add("@min_amount_jur", SqlDbType.Float).Value = product.MinAmountJur;
                    cmd.Parameters.Add("@type_of_client", SqlDbType.SmallInt).Value = product.TypeOfClient;
                    cmd.Parameters.Add("@bonus_interest_rate_for_repayment_type", SqlDbType.Float).Value = product.BonusInterestRateForRepaymentType;
                    cmd.Parameters.Add("@interest_rate_for_allow_addition", SqlDbType.Float).Value = product.InterestRateForAllowAddition;
                    cmd.Parameters.Add("@interest_rate_for_allow_decreasing", SqlDbType.Float).Value = product.InterestRateForAllowDecreasing;
                    cmd.Parameters.Add("@interest_rate_for_allow_addition_and_decreasing", SqlDbType.Float).Value = product.InterestRateForAllowAdditionAndDecreasing;
                    cmd.Parameters.Add("@bonus_interest_rate_for_classic", SqlDbType.Float).Value = product.BonusInterestRateForClassic;
                    cmd.Parameters.Add("@bonus_interest_rate_for_avangard", SqlDbType.Float).Value = product.BonusInterestRateForAvangard;
                    cmd.Parameters.Add("@bonus_interest_rate_for_premium", SqlDbType.Float).Value = product.BonusInterestRateForPremium;
                    cmd.Parameters.Add("@max_addition_percent", SqlDbType.Float).Value = product.MaxAdditionPercent;
                    cmd.Parameters.Add("@max_decreasing_percent", SqlDbType.Float).Value = product.MaxDecreasingPercent;
                    cmd.Parameters.Add("@registration_set_number", SqlDbType.Float).Value = product.RegistrationSetNumber;

                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }
                    else
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    reader.Close();

                }
            }
            return result;
        }

    }
}
