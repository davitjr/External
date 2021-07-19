using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class PosTerminalDB
    {

        internal static bool HasPosTerminal(ulong customerNumber)
        {

            bool hasPosTerminal = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select se.SE_ID  from Tbl_Service_Establishment se 
                               inner join Tbl_SE_Locations s
                               on se.SE_ID=se.SE_ID
                               inner join Tbl_ARCA_points_list a 
                               on a.Location_id = s.Location_id
                               where se.customer_number = @customerNumber and a.closing_date is null";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        hasPosTerminal = dr.HasRows;
                    }
                }
            }
            return hasPosTerminal;
        }
        internal static List<PosRate> GetPosRates(int terminalId)
        {

            List<PosRate> posRates = new List<PosRate>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT ac.Rate,ac.Local_rate,ac.Fixed_rate, ac.Code,ar.SystemName,ar.ID 
                                                  From Tbl_ArCa_Cards_Rates ac 
                                                  INNER JOIN [Tbl_Type_of_ArCa_Card_Rates] ar
                                                  ON ac.Rate_Type = ar.ID
                                                  Where ac.code = @code", conn))
                {
                    cmd.Parameters.AddWithValue("@code", terminalId);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            PosRate posRate = new PosRate();
                            posRate.CardSystem = Convert.ToUInt16(dr["ID"].ToString());
                            posRate.CardSystemDescription = dr["SystemName"].ToString();
                            posRate.Rate = float.Parse(dr["Rate"].ToString());
                            posRate.LocalRate = float.Parse(dr["Local_rate"].ToString());
                            posRate.FixedRate = Convert.ToUInt16(dr["Fixed_rate"].ToString());
                            posRates.Add(posRate);
                        }
                    }
                }

            }
            return posRates;
        }
        internal static List<PosCashbackRate> GetPosCashbackRates(int terminalId)
        {

            List<PosCashbackRate> cashBackRates = new List<PosCashbackRate>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT R.ID,R.Card_type, T.applicationscardtype, R.rate, R.date_of_beginning, R.deleted, R.rate_from_ACBA 
                                                  FROM Tbl_Cashback_Rates R 
                                                  INNER JOIN  tbl_type_of_card T 
                                                  ON R.Card_type = T.ID
                                                  WHERE  code = @code and deleted = 0 Order by applicationscardtype,  R.ID",conn))
                {
                    cmd.Parameters.AddWithValue("@code", terminalId);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            PosCashbackRate posCashbackRate = new PosCashbackRate();
                            posCashbackRate.CardType = Convert.ToUInt16(dr["Card_type"].ToString());
                            posCashbackRate.CardTypeDescription = dr["applicationscardtype"].ToString();
                            posCashbackRate.Rate = float.Parse(dr["rate"].ToString());
                            posCashbackRate.RateFromACBA = float.Parse(dr["rate_from_ACBA"].ToString());
                            posCashbackRate.StartDate = Convert.ToDateTime(dr["date_of_beginning"].ToString());
                            cashBackRates.Add(posCashbackRate);

                        }
                    }
                }

                    
            }
            return cashBackRates;
        }

        internal static MerchantStatement GetStatement(string cardAccount, DateTime dateFrom, DateTime dateTo, byte option)
        {
            MerchantStatement listTransaction = new MerchantStatement();
            listTransaction.Transactions = new List<TransactionDetail>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("pr_Merchant_statment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@card_acc", SqlDbType.VarChar, 20).Value = cardAccount;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@statment_option", SqlDbType.SmallInt).Value = option;
                    cmd.Parameters.Add("@fil", SqlDbType.Float).Value = 22000;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                   
                        while (dr.Read())
                        {
                            TransactionDetail trDetail = new TransactionDetail();
                            trDetail.Authcode = dr["autorisation_code"] == DBNull.Value ? "" : dr["autorisation_code"].ToString();
                            trDetail.CardNumber = (dr["CardNumber"] == DBNull.Value) ? "" : dr["CardNumber"].ToString();
                            trDetail.TransactionFee = (dr["Fee"] == DBNull.Value) ? 0 : Convert.ToDecimal(dr["Fee"]);
                            trDetail.Settlementdate = (dr["date_of_accounting"] == DBNull.Value) ? default(DateTime) : Convert.ToDateTime(dr["date_of_accounting"]);
                            trDetail.DebitCredit = dr["debet_credit"].ToString();
                            trDetail.Description = dr["Description"].ToString();
                            trDetail.TransactionAmount = Convert.ToDecimal(dr["Amount"]);
                            trDetail.TransactionDate = Convert.ToDateTime(dr["Operation_date"]).Date;
                            trDetail.OrderID = dr["order_number"].ToString();
                            listTransaction.Transactions.Add(trDetail);
                        }
                    }
                }
                return listTransaction;
            }
        }
    }
}
