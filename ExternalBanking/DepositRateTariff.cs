using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{
    public class DepositRateTariff
    {
        public short? RateRepaymentFrequency { get; set; }
        public string FrequencyDescription { get; set; }

        //  public DepositType DepositType { get; set; }

        public List<DepositRateTariffItem> DepositRateTariffItems { get; set; }


        public static DepositRateTariff GetDepositRateTariff(DepositType depositType, byte lang)
        {
            DataTable dt = new DataTable();
            DepositRateTariff depositRateTariff = new DepositRateTariff();
            dt = DepositRateTariffDB.GetDepositRateTariff(depositType);

            if (dt.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dt.Rows[0]["FreqDescr"].ToString()))
                    depositRateTariff.FrequencyDescription = lang == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[0]["FreqDescr"].ToString()) : dt.Rows[0]["FreqDescr_eng"].ToString();

                if (depositType == DepositType.DepositClassic || depositType == DepositType.DepositFamily)
                {
                    depositRateTariff.RateRepaymentFrequency = Convert.ToInt16(dt.Rows[0]["rate_repayment_frequency"]);
                }
                else
                {
                    depositRateTariff.RateRepaymentFrequency = null;
                }

                depositRateTariff.DepositRateTariffItems = GetDepositTariffList(depositType);
            }

            return depositRateTariff;
        }

        public static List<DepositRateTariffItem> GetDepositTariffList(DepositType depositType)
        {
            return DepositRateTariffDB.GetDepositTariffList(depositType);
        }

    }

    public class DepositRateTariffItem
    {
        public string Currency { get; set; }
        public short? PeriodInMonthsMin { get; set; }
        public short? PeriodInMonthsMax { get; set; }
        public double InterestRate { get; set; }
        public double BonusInterestRateForHB { get; set; }
        public double BonusInterestRateForEmployee { get; set; }
    }




}
