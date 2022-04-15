using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;


namespace ExternalBanking
{
    public class ExchangeRate
    {
        /// <summary>
        /// Փոխարժեքի անվանում
        /// </summary>
        public string SourceCurrency { get; set; }
        /// <summary>
        /// Անկանխիք գնման փոխարժեք
        /// </summary>
        public float BuyRate { get; set; }
        /// <summary>
        /// Անկանխիք վաճառքի փոխարժեք
        /// </summary>
        public float SaleRate { get; set; }
        /// <summary>
        /// Գնման քարտային փոխարժեք
        /// </summary>
        public float BuyRateATM { get; set; }
        /// <summary>
        /// Վաճառքի քարտային փոխարժեք
        /// </summary>
        public float SaleRateATM { get; set; }
        /// <summary>
        /// Կանխիք գնման փոխարժեք
        /// </summary>
        public float BuyRateCash { get; set; }
        /// <summary>
        /// Կանխիք վաճառքի փոխարժեք
        /// </summary>
        public float SaleRateCash { get; set; }
        /// <summary>
        /// Քրոս գնման փոխարժեք
        /// </summary>
        public float BuyRateCross { get; set; }
        /// <summary>
        /// Քրոս վաճառքի փոխարժեք
        /// </summary>
        public float SaleRateCross { get; set; }
        /// <summary>
        /// Հեռախոսազանգով փոխանցման գնման փոխարժեք
        /// </summary>
        public float BuyRateTransfer { get; set; }
        /// <summary>
        /// Հեռախոսազանգով փոխանցման վաճառքի փոխարժեք
        /// </summary>
        public float SaleRateTransfer { get; set; }
        /// <summary>
        /// Կենտրոնական բանկի փոխարժեք
        /// </summary>
        public float RateCB { get; set; }
        /// <summary>
        /// Փոխարժեքի հաստատման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }


        public static List<ExchangeRate> GetExchangeRates(int filial = 22000)
        {
            return ExchangeRateDB.GetExchangeRates(filial);
        }
        /// <summary>
        /// Փոխարժեքների պատմություն
        /// </summary>
        /// <param name="filialCode"></param>
        /// <param name="currency"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<ExchangeRate> GetExchangeRatesHistory(int filialCode, string currency, DateTime startDate)
        {
            return ExchangeRateDB.GetExchangeRatesHistory(filialCode, currency, startDate);
        }

        /// <summary>
        /// Քրոսս փոխարժեքների պատմություն
        /// </summary>
        /// <param name="filialCode"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<CrossExchangeRate> GetCrossExchangeRatesHistory(int filialCode, DateTime startDate)
        {
            return ExchangeRateDB.GetCrossExchangeRatesHistory(filialCode, startDate);
        }

        /// <summary>
        /// ԿԲ փոխարժեքների պատմություն
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<ExchangeRate> GetCBExchangeRatesHistory(string currency, DateTime startDate)
        {
            return ExchangeRateDB.GetCBExchangeRatesHistory(currency, startDate);
        }

    }
}
