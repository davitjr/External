using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CreditLinePrecontractData
    {
        public double InterestRate { get; set; }

        public double InterestRateEffectiveWithoutAccountServiceFee { get; set; }

        public double RepaymentRate { get; set; }

        public static CreditLinePrecontractData GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType)
        {

            CreditLinePrecontractData result = new CreditLinePrecontractData();
             DataTable dt = new DataTable();
                dt = CreditLine.GetCreditLinePrecontractData(startDate, endDate, interestRate, repaymentPercent, cardNumber, currency, amount, loanType);
                if (dt.Rows.Count > 0)
                {
                    result.InterestRate = Convert.ToDouble(dt.Rows[0][0]);
                    result.InterestRateEffectiveWithoutAccountServiceFee = Convert.ToDouble(dt.Rows[0][1]);
                    result.RepaymentRate = Convert.ToDouble(dt.Rows[0][2]);             
                }

                return result;
           
        }
    }
}
