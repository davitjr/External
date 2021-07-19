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
    class CardTariffContractDB
    {

        public static CardTariffContract GetCardTariffContract(CardTariffContract tariffContract)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT Office_ID , Office_Name, Contract_beginning, Contract_end, Contract_City, Address,Filial,ContractStatus,ReasonId,OfficeaccountNumber
                FROM Tbl_Contract_salary_by_cards WHERE Office_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", tariffContract.TariffID);

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        tariffContract.TariffID = Convert.ToInt64(dr["Office_ID"]);
                        tariffContract.Description = Utility.ConvertAnsiToUnicode(dr["Office_Name"].ToString());
                        tariffContract.StartDate = Convert.ToDateTime(dr["Contract_beginning"]);
                        tariffContract.EndDate = dr["Contract_end"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["Contract_end"]);
                        tariffContract.Address = Utility.ConvertAnsiToUnicode(dr["Contract_City"].ToString() + "," + dr["Address"].ToString());
                        tariffContract.FilialCode = Convert.ToInt32(dr["Filial"].ToString());
                        tariffContract.Quality = Convert.ToUInt16(dr["ContractStatus"].ToString());
                        tariffContract.Reason = Convert.ToUInt16(dr["ReasonId"].ToString());
                        tariffContract.AccountNumber = Utility.ConvertAnsiToUnicode(dr["OfficeaccountNumber"].ToString());
                    }
                }
            }

            return tariffContract;
        }

        public static void GetCardTariffs(CardTariffContract tariffContract)
        {
            tariffContract.CardTariffs = new List<CardTariff>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT R.CardName + ' ' +  C.CardType as CardName, R.CardType,R.currency, R.CashRate, R.GracePeriod, R.Positive_Rate, R.loan_Rate, R.cash_rate_other,R.Quality, R.cardRateID , 
                                                R.Cash_rate_int,
                                                CASE R.currency 
	                                                WHEN 'AMD' THEN R.Min_fee_local_AMD 
	                                                WHEN 'USD' THEN R.Min_fee_local_USD 
	                                                WHEN 'EUR' THEN R.Min_fee_local_EUR  
                                                END AS Min_fee_local,
                                                R.Min_fee_int_AMD AS Min_fee_int, 
                                                R.CashInFeeRate_ACBA,R.CashInFeeRate_Other,R.SMSFeeFromCustomer,R.SMSFeeFromBank,R.C2CFeeOur,R.C2CFeeOther, isnull(R.Period, 0) as Period,R.Service_fee,tbl_type_of_card.CardSystemID,
                                                C.ValidityPeriod/12 ValidityPeriod
                                                FROM Tbl_cards_rates R INNER JOIN Tbl_type_of_Card C  On R.CardType = C.ID LEFT JOIN tbl_type_of_card on R.CardType = tbl_type_of_card.ID
                                                WHERE R.Office_ID = @OfficeID and R.quality=1", conn);
                cmd.Parameters.AddWithValue("@OfficeID", tariffContract.TariffID);

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        CardTariff cardTariff = new CardTariff();
                        cardTariff.Id = Convert.ToInt32(dr["cardRateID"]);
                        cardTariff.CardType = Convert.ToInt32(dr["CardType"]);
                        cardTariff.CardTypeDescription = dr["CardName"].ToString();
                        cardTariff.Currency = dr["currency"].ToString();
                        cardTariff.CashRateOur = (dr["CashRate"] != DBNull.Value) ? Convert.ToDouble(dr["CashRate"]) : default(double);//Convert.ToDouble(dr["CashRate"]);
                        cardTariff.CashRateOther = (dr["cash_rate_other"] != DBNull.Value) ? Convert.ToDouble(dr["cash_rate_other"]) : default(double);  //Convert.ToDouble(dr["cash_rate_other"]);
                        cardTariff.CashRateInternational = (dr["Cash_rate_int"] != DBNull.Value) ? Convert.ToDouble(dr["Cash_rate_int"]) : default(double); //Convert.ToDouble(dr["Cash_rate_int"]);
                        cardTariff.GracePeriod = Convert.ToInt32(dr["GracePeriod"]);
                        cardTariff.PositiveRate = (dr["Positive_Rate"] != DBNull.Value) ? Convert.ToDouble(dr["Positive_Rate"]) : default(double); //Convert.ToDouble(dr["Positive_Rate"]);
                        cardTariff.NegativeRate = (dr["loan_Rate"] != DBNull.Value) ? Convert.ToDouble(dr["loan_Rate"]) : default(double); //Convert.ToDouble(dr["loan_Rate"]);
                        cardTariff.Quality = Convert.ToByte(dr["Quality"]);
                        cardTariff.MinFeeLocal = (dr["Min_fee_local"] != DBNull.Value) ? Convert.ToDouble(dr["Min_fee_local"]) : default(double); //Convert.ToDouble(dr["Min_fee_local"]);
                        cardTariff.MinFeeInternational = (dr["Min_fee_int"] != DBNull.Value) ? Convert.ToDouble(dr["Min_fee_int"]) : default(double);//Convert.ToDouble(dr["Min_fee_int"]);
                        cardTariff.CashInFeeRateOur = (dr["CashInFeeRate_ACBA"] != DBNull.Value) ? Convert.ToDouble(dr["CashInFeeRate_ACBA"]) : default(double);//Convert.ToDouble(dr["CashInFeeRate_ACBA"]);
                        cardTariff.CashInFeeRateOther = (dr["CashInFeeRate_Other"] != DBNull.Value) ? Convert.ToDouble(dr["CashInFeeRate_Other"]) : default(double);//Convert.ToDouble(dr["CashInFeeRate_Other"]);
                        cardTariff.SMSFeeFromCustomer = (dr["SMSFeeFromCustomer"] != DBNull.Value) ? Convert.ToDouble(dr["SMSFeeFromCustomer"]) : default(double); //Convert.ToDouble(dr["SMSFeeFromCustomer"]);
                        cardTariff.SMSFeeFromBank = (dr["SMSFeeFromBank"] != DBNull.Value) ? Convert.ToDouble(dr["SMSFeeFromBank"]) : default(double);//Convert.ToDouble(dr["SMSFeeFromBank"]);
                        cardTariff.CardToCardFeeOur = (dr["C2CFeeOur"] != DBNull.Value) ? Convert.ToDouble(dr["C2CFeeOur"]) : default(double);//Convert.ToDouble(dr["C2CFeeOur"]);
                        cardTariff.CardToCardFeeOther = (dr["C2CFeeOther"] != DBNull.Value) ? Convert.ToDouble(dr["C2CFeeOther"]) : default(double);//Convert.ToDouble(dr["C2CFeeOther"]);
                        cardTariff.CardSystem = Convert.ToInt32(dr["CardSystemID"]);
                        cardTariff.Period = Convert.ToInt32(dr["Period"]);
                        cardTariff.ServiceFee = Convert.ToInt64(dr["Service_fee"]);
                        cardTariff.CardValidityPeriod = Convert.ToInt32(dr["ValidityPeriod"]);
                        tariffContract.CardTariffs.Add(cardTariff);
                    }
                }
            }
        }    
        internal static List<CardTariffContract> GetCustomerCardTariffContracts(ulong customerNumber,ProductQualityFilter filter)
        {
            List<CardTariffContract> cardTariffContracts = new List<CardTariffContract>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";
                sql = @"SELECT Office_ID , Office_Name, Contract_beginning, Contract_end,Filial,ContractStatus,ReasonId,OfficeaccountNumber
                          FROM Tbl_Contract_salary_by_cards WHERE isnull(customer_number,0)<> 0 and customer_number = @customerNumber";

                string whereCondition = "";
                if (filter == ProductQualityFilter.Opened)
                {
                    whereCondition = " AND isnull(ContractStatus,0)<>0 ";
                }
                else if (filter == ProductQualityFilter.Closed)
                {
                    whereCondition = " AND isnull(ContractStatus,0)=0 ";
                }
                sql += whereCondition;
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
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            CardTariffContract cardTariffContract = SetCardTariffContract(row);
                            cardTariffContracts.Add(cardTariffContract);
                        }
                    }                  
                }
            }
            return cardTariffContracts;
        }
        internal static bool HasCardTariffContract(ulong customerNumber)
        {
            bool hasCardTariffContract = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT Office_ID
                                FROM Tbl_Contract_salary_by_cards
                                WHERE customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        hasCardTariffContract = dr.HasRows;
                    }
                }
            }
            return hasCardTariffContract;
        }        
        private static CardTariffContract SetCardTariffContract(DataRow dr)
        {
            CardTariffContract cardTariffContract = new CardTariffContract();
            if (dr != null)
            {
                cardTariffContract.TariffID = Convert.ToInt64(dr["Office_ID"]);
                cardTariffContract.Description = Utility.ConvertAnsiToUnicode(dr["Office_Name"].ToString());
                cardTariffContract.StartDate = Convert.ToDateTime(dr["Contract_beginning"]);
                cardTariffContract.EndDate = dr["Contract_end"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["Contract_end"]);
                cardTariffContract.FilialCode = Convert.ToInt32(dr["Filial"].ToString());
                cardTariffContract.Quality = Convert.ToUInt16(dr["ContractStatus"].ToString());
                cardTariffContract.Reason = Convert.ToUInt16(dr["ReasonId"].ToString());
                cardTariffContract.AccountNumber = Utility.ConvertAnsiToUnicode(dr["OfficeaccountNumber"].ToString());
            }
            return cardTariffContract;
        }


      internal static int GetActiveCardsCount(int contractId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select count(*) as cnt from Tbl_Visa_Numbers_Accounts where related_office_number  = @officeId and closing_date is null";
                    cmd.Parameters.Add("@officeId", SqlDbType.Int).Value = contractId;
                    int cardsCount = (int)cmd.ExecuteScalar();
                    return cardsCount;
                }

            }
        }

        internal static List<float> GetCardUSSDServiceTariff(ulong productId)
        {
            List<float> list = new List<float>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT USSD_fee,USSD_fee_from_client,USSD_fee_from_bank FROM tbl_visa_numbers_accounts WHERE app_id = @productID";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            list.Add((dr["USSD_fee"] != DBNull.Value)?float.Parse(dr["USSD_fee"].ToString()):0);
                            list.Add((dr["USSD_fee_from_client"] != DBNull.Value) ? float.Parse(dr["USSD_fee_from_client"].ToString()) : 0);
                            list.Add((dr["USSD_fee_from_bank"] != DBNull.Value) ? float.Parse(dr["USSD_fee_from_bank"].ToString()) : 0);
                        }
                    }
                }
                return list;

            }
        }

        internal static List<float> GetPlasticCardSMSServiceTariff(ulong productId)
        {
            List<float> list = new List<float>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT SMSFee,SMSFeeFromClient,SMSFeeFromBank  FROM tbl_visa_numbers_accounts WHERE app_id = @productID";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            list.Add((dr["SMSFee"] != DBNull.Value) ? float.Parse(dr["SMSFee"].ToString()) : 0);
                            list.Add((dr["SMSFeeFromClient"] != DBNull.Value) ? float.Parse(dr["SMSFeeFromClient"].ToString()) : 0);
                            list.Add((dr["SMSFeeFromBank"] != DBNull.Value) ? float.Parse(dr["SMSFeeFromBank"].ToString()) : 0);
                        }
                    }
                }
                return list;

            }
        }






    }
}
