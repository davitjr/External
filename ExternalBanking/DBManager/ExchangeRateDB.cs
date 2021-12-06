using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;

namespace ExternalBanking.DBManager
{
    class ExchangeRateDB
    {

        public static List<ExchangeRate> GetExchangeRates(int filial=22000)
        {
            
            List<ExchangeRate> rateList = new List<ExchangeRate>();

          
             using(SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
             {
                 conn.Open();
                 string sqlString = @"SELECT * FROM (select m.source_currency,r.buy_rate, r.sale_rate,m.m_date,
                                    r.buy_rate_ATM ,r.sale_rate_ATM ,
                                    r.buy_rate_cash ,r.sale_rate_Cash ,
                                    r.buy_rate_for_cross ,r.sale_rate_for_cross,r.buy_rate_ACBA_transfer,r.sale_rate_ACBA_transfer 
                                    from (
                                    select MAX(with_date) as m_date,source_currency from dbo.Tbl_rates_sale_buy where source_currency<>'AMD' and filialcode=@filial group by source_currency) m
                                    inner join dbo.Tbl_rates_sale_buy r on m.m_date=r.with_date and m.source_currency=r.source_currency 
                                    inner join dbo.[Tbl_currency;] c on c.currency=r.source_currency where c.Quality=1  and r.filialcode=@filial) a
                                    inner join (
                                    Select m.source_currency,exchange_rate ,r.with_date     from (
                                    select MAX(with_date) as m_date,source_currency  from dbo.Tbl_rates_CB where source_currency<>'AMD' group by source_currency) m
                                    inner join dbo.Tbl_rates_CB r on m.m_date=r.with_date and m.source_currency=r.source_currency 
                                    inner join dbo.[Tbl_currency;] c on c.currency=r.source_currency where c.Quality=1) cb
                                    on a.source_currency=cb.source_currency
									ORDER BY Case when a.source_currency='USD' then 1 when a.source_currency='EUR' then 2 when a.source_currency='RUR' then 3
									 when a.source_currency='GBP' then 4 when a.source_currency='CHF' then 5 when a.source_currency='GEL' then 6 end";
                 using SqlCommand cmd = new SqlCommand(sqlString, conn);
                 cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filial;
                 cmd.CommandType = CommandType.Text;
                 using SqlDataReader dr = cmd.ExecuteReader();
                 while (dr.Read())
                 {
                     ExchangeRate rate = new ExchangeRate();
                     rate.SourceCurrency = dr["source_currency"].ToString();
                     rate.BuyRate =float.Parse(dr["buy_rate"].ToString());
                     rate.SaleRate = float.Parse(dr["sale_rate"].ToString());
                     rate.BuyRateATM = float.Parse(dr["buy_rate_ATM"].ToString());
                     rate.SaleRateATM = float.Parse(dr["sale_rate_ATM"].ToString());
                     rate.BuyRateCash = float.Parse(dr["buy_rate_Cash"].ToString());
                     rate.SaleRateCash = float.Parse(dr["sale_rate_Cash"].ToString());
                     rate.BuyRateCross = float.Parse(dr["buy_rate_for_cross"].ToString());
                     rate.SaleRateCross = float.Parse(dr["sale_rate_for_cross"].ToString());
                     rate.BuyRateTransfer = float.Parse(dr["buy_rate_ACBA_transfer"].ToString());
                     rate.SaleRateTransfer = float.Parse(dr["sale_rate_ACBA_transfer"].ToString());
                     rate.RegistrationDate = DateTime.Parse(dr["m_date"].ToString());
                     rate.RateCB = float.Parse(dr["exchange_rate"].ToString());
                     rateList.Add(rate);

                 }
                 
             }
            return rateList;
        }


        public static List<ExchangeRate> GetExchangeRatesHistory(int filialCode,string currency,DateTime startDate)
        {

            List<ExchangeRate> rateList = new List<ExchangeRate>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqlString = @"SELECT with_date,
                                            buy_rate_cash,
                                            sale_rate_cash,
                                            buy_rate, 
                                            sale_rate,
                                            buy_rate_ATM, 
                                            sale_rate_ATM, 
                                            buy_rate_for_cross,
                                            sale_rate_for_cross,
                                            buy_rate_ACBA_transfer, 
                                            sale_rate_ACBA_transfer,
                                            source_currency
                                            FROM Tbl_rates_sale_buy_total 
                                            WHERE 
                                            (source_currency)=@currency
                                            and with_date>=@startDate and filialcode=@filial 
                                            ORDER BY with_date DESC";
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar,3).Value = currency;
                cmd.Parameters.Add("@startDate", SqlDbType.NVarChar).Value = startDate.ToString("dd/MMM/yy");
                cmd.CommandType = CommandType.Text;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ExchangeRate rate = new ExchangeRate();
                    rate.SourceCurrency = dr["source_currency"].ToString();
                    rate.BuyRate = float.Parse(dr["buy_rate"].ToString());
                    rate.SaleRate = float.Parse(dr["sale_rate"].ToString());
                    rate.BuyRateATM = float.Parse(dr["buy_rate_ATM"].ToString());
                    rate.SaleRateATM = float.Parse(dr["sale_rate_ATM"].ToString());
                    rate.BuyRateCash = float.Parse(dr["buy_rate_cash"].ToString());
                    rate.SaleRateCash = float.Parse(dr["sale_rate_cash"].ToString());
                    rate.BuyRateCross = float.Parse(dr["buy_rate_for_cross"].ToString());
                    rate.SaleRateCross = float.Parse(dr["sale_rate_for_cross"].ToString());
                    rate.BuyRateTransfer = float.Parse(dr["buy_rate_ACBA_transfer"].ToString());
                    rate.SaleRateTransfer = float.Parse(dr["sale_rate_ACBA_transfer"].ToString());
                    rate.RegistrationDate = DateTime.Parse(dr["with_date"].ToString());
                    rateList.Add(rate);

                }

            }
            return rateList;
        }



        public static List<CrossExchangeRate> GetCrossExchangeRatesHistory(int filialCode,DateTime startDate)
        {

            List<CrossExchangeRate> rateList = new List<CrossExchangeRate>();

           
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqlString = @"SELECT 
                                    DISTINCT convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)) as with_date,
                                    Tbl_rates_sale_buy_total.source_currency, 
                                    [dbo].fnc_kurs_s_for_date('EUR',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS EUR_S,
                                    [dbo].fnc_kurs_s_for_date('USD',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS USD_S, 
                                    [dbo].fnc_kurs_b_for_date('USD',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS USD_B,
                                    [dbo].fnc_kurs_b_for_date('RUR',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS RUR_B, 
                                    [dbo].fnc_kurs_b_for_date('EUR',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS EUR_B,
                                    [dbo].fnc_kurs_s_for_date('RUR',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS RUR_S,
                                    [dbo].fnc_kurs_s_for_date('GBP',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS GBP_S, 
                                    [dbo].fnc_kurs_b_for_date('GBP',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS GBP_B,
                                    [dbo].fnc_kurs_s_for_date('CHF',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS CHF_S,
                                    [dbo].fnc_kurs_b_for_date('CHF',convert(datetime,round(convert(float,Tbl_rates_sale_buy_total.with_date),0,1)),3,@filialCode) AS CHF_B 
                                    FROM Tbl_rates_sale_buy_total 
                                    WHERE (source_currency) = 'USD' and with_date>=@startDate
                                    ORDER BY convert(datetime,round(convert(float,with_date),0,1)) DESC;";
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                cmd.Parameters.Add("@startDate", SqlDbType.NVarChar).Value = startDate.ToString("dd/MMM/yy");
                cmd.CommandType = CommandType.Text;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CrossExchangeRate rate = new CrossExchangeRate();
                    rate.EURSale = float.Parse(dr["EUR_S"].ToString());
                    rate.EURBuy = float.Parse(dr["EUR_B"].ToString());
                    rate.USDSale = float.Parse(dr["USD_S"].ToString());
                    rate.USDBuy = float.Parse(dr["USD_B"].ToString());
                    rate.RURBuy = float.Parse(dr["RUR_B"].ToString());
                    rate.RURSale = float.Parse(dr["RUR_S"].ToString());
                    rate.GBPSale = float.Parse(dr["GBP_S"].ToString());
                    rate.GBPBuy = float.Parse(dr["GBP_B"].ToString());
                    rate.CHFSale = float.Parse(dr["CHF_S"].ToString());
                    rate.CHFBuy = float.Parse(dr["CHF_B"].ToString());
                    rate.RegistrationDate = DateTime.Parse(dr["with_date"].ToString());
                    rateList.Add(rate);

                }

            }
            return rateList;
        }


        public static List<ExchangeRate> GetCBExchangeRatesHistory(string currency, DateTime startDate)
        {

            List<ExchangeRate> rateList = new List<ExchangeRate>();

            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqlString = @"SELECT with_date,exchange_rate,source_currency
                                                FROM Tbl_rates_CB_total
                                                WHERE source_currency = @currency
	                                                AND with_date >= @startDate
                                                ORDER BY with_date DESC";
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                cmd.Parameters.Add("@startDate", SqlDbType.NVarChar).Value = startDate.ToString("dd/MMM/yy");
                cmd.CommandType = CommandType.Text;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ExchangeRate rate = new ExchangeRate();
                    rate.SourceCurrency = dr["source_currency"].ToString();
                    rate.RegistrationDate = DateTime.Parse(dr["with_date"].ToString());
                    rate.RateCB = float.Parse(dr["exchange_rate"].ToString());
                    rateList.Add(rate);

                }

            }
            return rateList;
        }
    }
}
