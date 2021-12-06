using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ExternalBanking.DBManager
{
    internal static class DemandDepositRateDB
    {

        internal static DemandDepositRate GetDemandDepositRate(string accountNumber)
        {
            DataTable dt = new DataTable();
            DemandDepositRate demandDepositRate = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT tf.ID,
                                                  account_number,
                                                  percent_credit_account,
                                                  tp.product_currency,
                                                  gp.description ,
                                                  tp.tariff_group_id,
                                                  tf.tariff_type_id
                                                  FROM tbl_demand_deposits_tariffs tf
                                                  INNER JOIN tbl_demand_deposits_tariff_types tp
                                                  ON tf.tariff_type_id=tp.ID
                                                  INNER JOIN tbl_demand_deposits_tariff_groups gp
                                                  ON gp.ID=tp.tariff_group_id
                                                  WHERE account_number=@account_number", conn);

                cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 16).Value = accountNumber;
                dt.Load(cmd.ExecuteReader());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    demandDepositRate = new DemandDepositRate();
                    demandDepositRate.Id = Convert.ToInt32(dt.Rows[i]["ID"]);
                    demandDepositRate.DemandDepositAccount = Account.GetAccount(dt.Rows[i]["account_number"].ToString());
                    demandDepositRate.PercentCreditAccount = Convert.ToDouble(dt.Rows[i]["percent_credit_account"]);
                    demandDepositRate.TariffGroupDescription = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    demandDepositRate.TariffGroup = Convert.ToInt32(dt.Rows[i]["tariff_group_id"]);
                    demandDepositRate.TariffTypeId = Convert.ToInt32(dt.Rows[i]["tariff_type_id"]);
                    if (demandDepositRate.TariffGroup == 2)
                    {
                        demandDepositRate.DemandDepositRateTariffDetails.AddRange(GetDemandDepositRateTariffIndividual(accountNumber));
                        KeyValuePair<string, DateTime>? document = GetDemandDepositRateTariffIndividualDocument(accountNumber);
                        demandDepositRate.DocumentNumber = document?.Key;
                        demandDepositRate.DocumentDate = document?.Value;
                    }
                    else
                    {
                        List<DemandDepositRate> list = GetDemandDepositRateTariffs();
                        demandDepositRate.DemandDepositRateTariffDetails.AddRange(list.Find(m => m.TariffGroup == demandDepositRate.TariffGroup).DemandDepositRateTariffDetails.FindAll(m => m.TariffTypeId == demandDepositRate.TariffTypeId));
                    }

                    if (demandDepositRate.TariffGroup == 3)
                    {
                        KeyValuePair<string, DateTime>? document = DemandDepositRate.GetDemandDepositRateTariffDocument(1);
                        demandDepositRate.DocumentNumber = document?.Key;
                        demandDepositRate.DocumentDate = document?.Value;

                    }


                }

            }
            return demandDepositRate;
        }


        internal static List<DemandDepositRate> GetDemandDepositRateTariffs()
        {
            List<DemandDepositRate> result = new List<DemandDepositRate>();
            DataTable dt = new DataTable();
            DataTable dtTariffGroups = Info.GetDemandDepositsTariffGroups();
            if (dtTariffGroups.Rows.Count > 0)
            {
                for (int i = 0; i < dtTariffGroups.Rows.Count; i++)
                {
                    DemandDepositRate demandDepositRate = new DemandDepositRate();
                    demandDepositRate.TariffGroupDescription = Utility.ConvertAnsiToUnicode(dtTariffGroups.Rows[i]["description"].ToString());
                    demandDepositRate.TariffGroup = Convert.ToInt32(dtTariffGroups.Rows[i]["ID"]);
                    result.Add(demandDepositRate);
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT dt.tariff_type_id,
                                                         dt.amount_from,
                                                         dt.amount_to,
                                                         dt.interest_rate,
												         tp.tariff_group_id,
                                                         tp.product_currency
												         FROM tbl_demand_deposits_tariff_types tp
												         INNER JOIN tbl_demand_deposits_tariff_type_details dt
												         ON tp.id=dt.tariff_type_id
                                                         WHERE  tp.tariff_group_id<>3 OR
                                                         ( tp.tariff_group_id=3 AND product_currency='AMD')", conn);


                dt.Load(cmd.ExecuteReader());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DemandDepositRateTariffDetail details = new DemandDepositRateTariffDetail();
                    details.TariffTypeId = Convert.ToInt32(dt.Rows[i]["tariff_type_id"]);
                    if (dt.Rows[i]["amount_from"] != DBNull.Value)
                        details.AmountFrom = Convert.ToDouble(dt.Rows[i]["amount_from"]);
                    if (dt.Rows[i]["amount_to"] != DBNull.Value)
                        details.AmountTo = Convert.ToDouble(dt.Rows[i]["amount_to"]);
                    if (dt.Rows[i]["interest_rate"] != DBNull.Value)
                        details.InterestRate = Convert.ToDouble(dt.Rows[i]["interest_rate"]);
                    if (dt.Rows[i]["tariff_group_id"] != DBNull.Value)
                        details.TariffGroup = Convert.ToInt32(dt.Rows[i]["tariff_group_id"]);
                    if (dt.Rows[i]["product_currency"] != DBNull.Value)
                        details.ProductCurrency = dt.Rows[i]["product_currency"].ToString();
                    result.FindAll(m => m.TariffGroup == details.TariffGroup).ForEach(m =>
                    {
                        m.DemandDepositRateTariffDetails.Add(details);
                    });
                }

            }
            return result;
        }



        internal static KeyValuePair<string, DateTime>? GetDemandDepositRateTariffDocument(byte documentType)
        {
            DataTable dt = new DataTable();
            KeyValuePair<string, DateTime>? document = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"      SELECT  document_number,
                                                        document_date
                                                        FROM tbl_demand_deposits_documents
														WHERE registration_date=(SELECT MAX(registration_date)  
                                                        FROM tbl_demand_deposits_documents WHERE document_type=@documentType) AND document_type=@documentType", conn);
                cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = documentType;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    document = new KeyValuePair<string, DateTime>(Utility.ConvertAnsiToUnicode(dt.Rows[0]["document_number"].ToString()), Convert.ToDateTime(dt.Rows[0]["document_date"].ToString()));
                }
            }
            return document;
        }

        internal static KeyValuePair<string, DateTime>? GetDemandDepositRateTariffIndividualDocument(string accountNumber)
        {
            DataTable dt = new DataTable();
            KeyValuePair<string, DateTime>? document = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT document_number,document_date 
                                                  FROM tbl_demand_deposits_documents_individual
                                                  WHERE account_number=@account_number AND
                                                  registration_date=(SELECT MAX(registration_date)
                                                  FROM tbl_demand_deposits_documents_individual 
                                                  WHERE account_number=@account_number)", conn);

                cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 16).Value = accountNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    document = new KeyValuePair<string, DateTime>(Utility.ConvertAnsiToUnicode(dt.Rows[0]["document_number"].ToString()), Convert.ToDateTime(dt.Rows[0]["document_date"].ToString()));
                }

            }
            return document;
        }


        internal static List<DemandDepositRateTariffDetail> GetDemandDepositRateTariffIndividual(string accountNumber)
        {
            DataTable dt = new DataTable();
            List<DemandDepositRateTariffDetail> result = new List<DemandDepositRateTariffDetail>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT amount_from,amount_to,interest_rate
                                                  FROM tbl_demand_deposits_tariff_individual
                                                  WHERE account_number=@account_number", conn);

                cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 16).Value = accountNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DemandDepositRateTariffDetail details = new DemandDepositRateTariffDetail();
                        if (dt.Rows[i]["amount_from"] != DBNull.Value)
                            details.AmountFrom = Convert.ToDouble(dt.Rows[i]["amount_from"]);
                        if (dt.Rows[i]["amount_to"] != DBNull.Value)
                            details.AmountTo = Convert.ToDouble(dt.Rows[i]["amount_to"]);
                        if (dt.Rows[i]["interest_rate"] != DBNull.Value)
                            details.InterestRate = Convert.ToDouble(dt.Rows[i]["interest_rate"]);
                        result.Add(details);
                    }
                }


            }
            return result;
        }

    }
}
