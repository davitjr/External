using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class SearchDepositProductPrices
    {
        public string Status { get; set; }
      
        public byte ProductCode { get; set; }

        public string Currency { get; set; }

        public short TypeOfClient { get; set; }

        public DateTime DateOfBeginning { get; set; }

        public int PeriodInMonthsMin { get; set; }

        public int PeriodInMonthsMax { get; set; }

        public List<DepositProductPrices> GetSearchedLoanEquipments()
        {
            return DepositProductPricesDB.GetDepositProductPrices(this);
        }
        
    }
}
