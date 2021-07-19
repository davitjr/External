using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;

namespace ExternalBanking.UtilityPaymentsManagment
{
    public class BeelineAbonentSearch
    {
        public string BeelineAbonentNumber { get; set; }
        public string Balance { get; set; }
        public string TimeStamp { get; set; }

        public BeelineAbonentSearch GetBeelineAbonentBalance(string abonentNumber, string amount = "10")
        {

            BeelineAbonentSearch beelineAbonentSearch = new BeelineAbonentSearch();
            BeelineAbonentCheckRequestResponse beelineAbonentCheckRequestResponse = new BeelineAbonentCheckRequestResponse();
            try
            {
                    beelineAbonentCheckRequestResponse = UtilityOperationService.BeelineAbonentNumberCheck(abonentNumber, amount);
                    beelineAbonentSearch.Balance = beelineAbonentCheckRequestResponse.BeelineAbonentCheckOutput.Balancek__BackingField;
                    beelineAbonentSearch.BeelineAbonentNumber = abonentNumber;
                    beelineAbonentSearch.TimeStamp = beelineAbonentCheckRequestResponse.BeelineAbonentCheckOutput.TimeStampk__BackingField;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return beelineAbonentSearch;
        }

    }
}
