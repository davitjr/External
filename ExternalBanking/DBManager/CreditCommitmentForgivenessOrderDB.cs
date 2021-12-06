using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class CreditCommitmentForgivenessOrderDB
    {

        public static CreditCommitmentForgivenessOrder GetForgivableLoanCommitment(ulong CustomerNumber, CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            CreditCommitmentForgivenessOrder creditCommitment = new CreditCommitmentForgivenessOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "pr_get_credit_commitment_forgiveness";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = creditCommitmentForgiveness.AppId;
                cmd.Parameters.Add("@loan_type", SqlDbType.Int).Value = creditCommitmentForgiveness.LoanType;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    creditCommitment.Currency = Convert.ToString(dr["currency"]);
                    creditCommitmentForgiveness.IsCreditLine = HasCreditLineRow(creditCommitmentForgiveness.LoanType);


                    if (!creditCommitmentForgiveness.IsCreditLine)
                    {

                        if (creditCommitment.Currency == "AMD")
                        {
                            creditCommitment.CurrentFee = dr["current_fee"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_fee"].ToString())), 1) : default(double);
                        }
                        else
                        {
                            creditCommitment.CurrentFee = dr["current_fee"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_fee"].ToString())), 2) : default(double);
                        }
                    }
                    if (creditCommitment.Currency == "AMD")
                    {
                        creditCommitment.JudgmentPenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["judgment_penalty_rate"])), 1);
                        creditCommitment.PenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["penalty_rate"])), 1);
                        creditCommitment.CurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value"])), 1);
                        creditCommitment.CurrentCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["current_capital"])), 1);
                        creditCommitment.OutCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["out_capital"])), 1);
                    }
                    else
                    {
                        creditCommitment.JudgmentPenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["judgment_penalty_rate"])), 2);
                        creditCommitment.PenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["penalty_rate"])), 2);
                        creditCommitment.CurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value"])), 2);
                        creditCommitment.CurrentCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["current_capital"])), 2);
                        creditCommitment.OutCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["out_capital"])), 2);
                    }

                    creditCommitment.LoanQuality = Math.Abs(Convert.ToInt32(dr["quality"]));
                    creditCommitment.OutLoanDate = dr["out_loan_date"] != DBNull.Value ? Convert.ToDateTime(dr["out_loan_date"]) : (DateTime?)null;
                    creditCommitment.FilialCode = Convert.ToUInt16(dr["filialcode"]);
                    creditCommitment.LoanFilialCode = Convert.ToInt16(dr["filialcode"]);
                    creditCommitment.LoanType = Convert.ToInt32(dr["loan_type"]);
                    creditCommitment.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                    if (creditCommitmentForgiveness.LoanType == 18)
                    {
                        if (creditCommitment.Currency == "AMD")
                        {
                            creditCommitment.CurrentRateValueNused = dr["current_rate_value_nused"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value_nused"])), 1) : default(double);
                        }
                        else
                        {
                            creditCommitment.CurrentRateValueNused = dr["current_rate_value_nused"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value_nused"])), 2) : default(double);
                        }
                    }
                }

                dr.NextResult();

                if (dr.Read())
                {
                    creditCommitment.NumberOfDeath = dr["document_number"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["document_number"].ToString()) : default(string);
                    creditCommitment.DateOfDeath = dr["document_given_date"] != DBNull.Value ? Convert.ToDateTime(dr["document_given_date"]) : default(DateTime);

                }
            }

            return creditCommitment;
        }


        internal static bool HasCreditLineRow(int loanType)
        {

            bool hasCreditLineRow = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT top 1  1 FROM Tbl_type_of_credit_lines WHERE code  = @loan_type";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@loan_type", SqlDbType.TinyInt).Value = loanType;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                hasCreditLineRow = dr.HasRows;
            }
            return hasCreditLineRow;
        }

        public static ActionResult SaveForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness, string UserNname)
        {
            double? Amount;
            Amount = creditCommitmentForgiveness.PenaltyRate + creditCommitmentForgiveness.CurrentCapital + creditCommitmentForgiveness.CurrentFee + creditCommitmentForgiveness.CurrentRateValue + creditCommitmentForgiveness.CurrentRateValueNused + creditCommitmentForgiveness.JudgmentPenaltyRate + creditCommitmentForgiveness.OutCapital;

            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "pr_save_credit_commitment_forgiveness";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = creditCommitmentForgiveness.AppId;

            if (Convert.ToInt32(creditCommitmentForgiveness.RebateType) == 14)
            {
                cmd.Parameters.Add("@document_given_date", SqlDbType.DateTime).Value = creditCommitmentForgiveness.DateOfDeath;
                cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 50).Value = creditCommitmentForgiveness.NumberOfDeath;
            }
            else
            {
                DateTime dt = (DateTime)creditCommitmentForgiveness.DateOfFoundation;
                string formattedDate = dt.ToString("dd/MMM/yyyy");
                cmd.Parameters.Add("@document_given_date", SqlDbType.DateTime).Value = formattedDate;

                if (creditCommitmentForgiveness.NumberOfFoundation != null)
                {
                    cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 50).Value = creditCommitmentForgiveness.NumberOfFoundation;
                }
                else
                {
                    cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                }
            }
            cmd.Parameters.Add("@current_fee", SqlDbType.Float).Value = creditCommitmentForgiveness.CurrentFee;
            cmd.Parameters.Add("@judgment_penalty_rate", SqlDbType.Float).Value = creditCommitmentForgiveness.JudgmentPenaltyRate;
            cmd.Parameters.Add("@penalty_rate", SqlDbType.Float).Value = creditCommitmentForgiveness.PenaltyRate;
            cmd.Parameters.Add("@current_rate_value", SqlDbType.Float).Value = creditCommitmentForgiveness.CurrentRateValue;
            cmd.Parameters.Add("@current_rate_value_nused", SqlDbType.Float).Value = creditCommitmentForgiveness.CurrentRateValueNused;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = creditCommitmentForgiveness.CustomerNumber;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = creditCommitmentForgiveness.FilialCode;
            cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = Amount;
            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = creditCommitmentForgiveness.Currency;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = UserNname;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = creditCommitmentForgiveness.OperationDate;
            cmd.Parameters.Add("@complete_date", SqlDbType.DateTime).Value = DateTime.Now.Date;
            cmd.Parameters.Add("@out_loan_date", SqlDbType.DateTime).Value = creditCommitmentForgiveness.OutLoanDate;
            cmd.Parameters.Add("@tax", SqlDbType.Float).Value = creditCommitmentForgiveness.Tax;
            cmd.Parameters.Add("@Loan_type", SqlDbType.TinyInt).Value = creditCommitmentForgiveness.LoanType;
            cmd.Parameters.Add("@Quality", SqlDbType.TinyInt).Value = creditCommitmentForgiveness.LoanQuality;
            cmd.Parameters.Add("@Product_Type", SqlDbType.TinyInt).Value = Utility.GetProductTypeByAppId(creditCommitmentForgiveness.AppId);

            if (creditCommitmentForgiveness.OutCapital != 0)
            {
                cmd.Parameters.Add("@capital", SqlDbType.Float).Value = creditCommitmentForgiveness.OutCapital != null ? creditCommitmentForgiveness.OutCapital : default(double);
            }
            else
            {
                cmd.Parameters.Add("@capital", SqlDbType.Float).Value = creditCommitmentForgiveness.CurrentCapital;
            }

            cmd.Parameters.Add("@rebate_type", SqlDbType.Int).Value = creditCommitmentForgiveness.RebateType;
            cmd.Parameters.Add("@loan_filial_code", SqlDbType.Int).Value = creditCommitmentForgiveness.LoanFilialCode;

            SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);

            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


            cmd.ExecuteNonQuery();

            byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
            int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

            creditCommitmentForgiveness.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            result.Id = creditCommitmentForgiveness.Id;
            creditCommitmentForgiveness.Quality = OrderQuality.Draft;

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
            return result;
        }

        internal static CreditCommitmentForgivenessOrder GetForgivableLoanCommitmentDetails(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            DataTable dt = new DataTable();

            CreditCommitmentForgivenessOrder creditCommitment = new CreditCommitmentForgivenessOrder();

            string sql = @"SELECT 
                                        hb.customer_number,
                                        hb.registration_date,
                                        hb.document_type,
                                        hb.document_number,
                                        hb.document_subtype,
                                        hb.quality,
                                        hb.currency, 
                                        hb.source_type,
                                        hb.operationFilialCode,
                                        hb.operation_date,
										d.current_fee,
										d.current_rate_value,
										d.current_rate_value_nused,
										d.document_given_date,
										d.document_number as NumberOfFoundation,
										d.judgment_penalty_rate,
										d.capital,
										d.tax,
										d.penalty_rate,
									    d.rebate_type,
										hp.Product_Type
										FROM Tbl_HB_documents hb
                                        INNER JOIN Tbl_credit_commitment_forgiveness  d  ON hb.doc_ID = d.doc_id
                                        INNER JOIN Tbl_HB_Products_Identity hp
										ON hp.HB_Doc_ID = hb.doc_ID 
                                        WHERE hb.Doc_ID = @DocID AND hb.customer_number = CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = creditCommitmentForgiveness.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = creditCommitmentForgiveness.CustomerNumber;

                    dt.Load(cmd.ExecuteReader());

                    creditCommitment.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    creditCommitment.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    creditCommitment.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    creditCommitment.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    creditCommitment.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    creditCommitment.Currency = dt.Rows[0]["currency"].ToString();
                    creditCommitment.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    creditCommitment.CurrentFee = Convert.ToDouble(dt.Rows[0]["current_fee"]);
                    creditCommitment.CurrentRateValue = Convert.ToDouble(dt.Rows[0]["current_rate_value"]);
                    creditCommitment.CurrentRateValueNused = Convert.ToDouble(dt.Rows[0]["current_rate_value_nused"]);
                    creditCommitment.DateOfFoundation = Convert.ToDateTime(dt.Rows[0]["document_given_date"]);
                    creditCommitment.NumberOfFoundation = dt.Rows[0]["NumberOfFoundation"].ToString();
                    creditCommitment.JudgmentPenaltyRate = Convert.ToDouble(dt.Rows[0]["judgment_penalty_rate"]);
                    creditCommitment.OutCapital = Convert.ToDouble(dt.Rows[0]["capital"]);
                    creditCommitment.PenaltyRate = Convert.ToDouble(dt.Rows[0]["penalty_rate"]);
                    creditCommitment.Tax = Convert.ToDouble(dt.Rows[0]["tax"]);
                    creditCommitment.RebateType = dt.Rows[0]["rebate_type"].ToString();
                    creditCommitment.ProductType = Convert.ToInt32(dt.Rows[0]["Product_Type"]);
                }
            }

            return creditCommitment;

        }

        /// <summary>
        /// Հաշվում է հարկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="capital"></param>
        /// <returns></returns>
        internal static double GetTax(ulong customerNumber, double? capital, string RebetType, string currency)
        {
            double tax = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select dbo.fn_get_rebate_tax_amount(@customerNumber,@capitalRebate,@rebateType,@currency)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@capitalRebate", SqlDbType.Float).Value = capital;
                    cmd.Parameters.Add("@rebateType", SqlDbType.Float).Value = Convert.ToInt32(RebetType);
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency.ToString();

                    tax = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }

            return Math.Round(tax, 2);
        }


        public static List<ActionError> ValidateCreditCommitmentForgivenes(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            List<ActionError> result = new List<ActionError>();
            double CurrentFee = 0, PenaltyRate = 0, CurrentRateValue = 0, OutCapital = 0, CurrentRateValueNused = 0;
            double JudgmentPenaltyRate = 0, CurrentCapital = 0, SubsidiaCurrentRateValue = 0;


            if (Convert.ToInt32(creditCommitmentForgiveness.RebateType) != 14)
            {
                if (creditCommitmentForgiveness.DateOfFoundation == default(DateTime) || creditCommitmentForgiveness.NumberOfFoundation == null)
                {
                    result.Add(new ActionError(1547));
                }
            }

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    
                    cmd.CommandText = "pr_get_credit_commitment_forgiveness";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@app_id", SqlDbType.BigInt).Value = creditCommitmentForgiveness.AppId;
                    cmd.Parameters.Add("@loan_type", SqlDbType.Int).Value = creditCommitmentForgiveness.LoanType;
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = creditCommitmentForgiveness.CustomerNumber;

                   using  SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        creditCommitmentForgiveness.Currency = Convert.ToString(dr["currency"]);
                        creditCommitmentForgiveness.IsCreditLine = HasCreditLineRow(creditCommitmentForgiveness.LoanType);


                        if (!creditCommitmentForgiveness.IsCreditLine)
                        {
                            if (creditCommitmentForgiveness.Currency == "AMD")
                            {
                                CurrentFee = dr["current_fee"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_fee"].ToString())), 1) : default(double);
                                SubsidiaCurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["Subsidia_Current_rate_value"].ToString())), 1);
                            }
                            else
                            {
                                CurrentFee = dr["current_fee"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_fee"].ToString())), 2) : default(double);
                                SubsidiaCurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["Subsidia_Current_rate_value"].ToString())), 2);
                            }


                        }




                        if (creditCommitmentForgiveness.Currency == "AMD")
                        {
                            JudgmentPenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["judgment_penalty_rate"])), 1);
                            PenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["penalty_rate"])), 1);
                            CurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value"])), 1);
                            CurrentCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["current_capital"])), 1);
                            OutCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["out_capital"])), 1);
                        }
                        else
                        {
                            JudgmentPenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["judgment_penalty_rate"])), 2);
                            PenaltyRate = Math.Round(Math.Abs(Convert.ToDouble(dr["penalty_rate"])), 2);
                            CurrentRateValue = Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value"])), 2);
                            CurrentCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["current_capital"])), 2);
                            OutCapital = Math.Round(Math.Abs(Convert.ToDouble(dr["out_capital"])), 2);
                        }

                        creditCommitmentForgiveness.LoanQuality = Convert.ToInt32(dr["quality"].ToString());
                        creditCommitmentForgiveness.OutLoanDate = dr["out_loan_date"] != DBNull.Value ? Convert.ToDateTime(dr["out_loan_date"]) : (DateTime?)null;
                        if (creditCommitmentForgiveness.LoanType == 18)
                        {
                            if (creditCommitmentForgiveness.Currency == "AMD")
                            {
                                CurrentRateValueNused = dr["current_rate_value_nused"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value_nused"])), 1) : default(double);
                            }
                            else
                            {
                                CurrentRateValueNused = dr["current_rate_value_nused"] != DBNull.Value ? Math.Round(Math.Abs(Convert.ToDouble(dr["current_rate_value_nused"])), 2) : default(double);
                            }
                        }

                    }


                    if (!creditCommitmentForgiveness.IsCreditLine)
                    {
                        //9527 առաջարկ
                        if (creditCommitmentForgiveness.CurrentRateValue > (CurrentRateValue - SubsidiaCurrentRateValue))
                        {
                            //Ներվող տոկոսագումարը գերազանցում է հաշվեգրված տոկոսի և սուբսիդավորվող տոկոսի տարբերությունից:
                            result.Add(new ActionError(1885));
                        }
                    }
                    if (creditCommitmentForgiveness.LoanQuality == 11 || creditCommitmentForgiveness.LoanQuality == 12)
                    {
                        if (CurrentFee < creditCommitmentForgiveness.CurrentFee)
                        {
                            result.Add(new ActionError(1558));
                        }

                        if (CurrentRateValueNused < creditCommitmentForgiveness.CurrentRateValueNused)
                        {
                            result.Add(new ActionError(1559));
                        }

                        if (JudgmentPenaltyRate < creditCommitmentForgiveness.JudgmentPenaltyRate)
                        {
                            result.Add(new ActionError(1560));
                        }

                        if (PenaltyRate < creditCommitmentForgiveness.PenaltyRate)
                        {
                            result.Add(new ActionError(1561));
                        }

                        if (CurrentRateValue < creditCommitmentForgiveness.CurrentRateValue)
                        {
                            result.Add(new ActionError(1562));
                        }

                        if (OutCapital < creditCommitmentForgiveness.OutCapital)
                        {
                            result.Add(new ActionError(1563));
                        }

                    }
                    else
                    {
                        if (CurrentFee < creditCommitmentForgiveness.CurrentFee)
                        {
                            result.Add(new ActionError(1558));
                        }

                        if (CurrentRateValueNused < creditCommitmentForgiveness.CurrentRateValueNused)
                        {
                            result.Add(new ActionError(1559));
                        }

                        if (JudgmentPenaltyRate < creditCommitmentForgiveness.JudgmentPenaltyRate)
                        {
                            result.Add(new ActionError(1560));
                        }

                        if (PenaltyRate < creditCommitmentForgiveness.PenaltyRate)
                        {
                            result.Add(new ActionError(1561));
                        }

                        if (CurrentRateValue < creditCommitmentForgiveness.CurrentRateValue)
                        {
                            result.Add(new ActionError(1562));
                        }

                        if (CurrentCapital < creditCommitmentForgiveness.CurrentCapital)
                        {
                            result.Add(new ActionError(1563));
                        }
                    }


                    int productTypeByAppId = Utility.GetProductTypeByAppId(creditCommitmentForgiveness.AppId);

                    if (productTypeByAppId != 2 && productTypeByAppId != 4
                        && productTypeByAppId != 3 && productTypeByAppId != 5)
                    {
                        if (creditCommitmentForgiveness.LoanQuality == 11 || creditCommitmentForgiveness.LoanQuality == 12)
                        {
                            if (creditCommitmentForgiveness.OutCapital == OutCapital)
                            {
                                result.Add(new ActionError(1545));
                            }
                        }
                        else
                        {
                            if (creditCommitmentForgiveness.CurrentCapital == CurrentCapital)
                            {
                                result.Add(new ActionError(1545));
                            }
                        }
                    }

                    if (creditCommitmentForgiveness.CurrentRateValue != 0 && creditCommitmentForgiveness.CurrentFee != CurrentFee)
                    {
                        result.Add(new ActionError(1546));
                    }

                    if (creditCommitmentForgiveness.DateOfFoundation.HasValue)
                    {
                        if (creditCommitmentForgiveness.DateOfFoundation.Value.Date > DateTime.Now.Date)
                        {
                            result.Add(new ActionError(1574));
                        }
                    }
                    else
                    {
                        result.Add(new ActionError(1573));
                    }


                }
            }
            return result;
        }

    }
}

