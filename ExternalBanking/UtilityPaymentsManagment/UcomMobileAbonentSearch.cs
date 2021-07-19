using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.UtilityPaymentsManagment
{
    public class UcomMobileAbonentSearch
    {
        public string Balance { get; set; }
        public string PhoneNumber { get; set; }


        public int Prepaid { get; set; }

        public string AbonentName { get; set; }

        public const string purpose = "Օրանժ հեռ․ վճար";

        public const string TypeOfDate = "Հաշվարկային ամիս";

        public UcomMobileAbonentSearch GetUcomAbonentSearch(string PhoneNumber)
        {

            var ucomAbonentSearch = new UcomMobileAbonentSearch();
            var ucomAccountData = new UcomMobileSearchAccountData();
            try
            {
                    ucomAccountData = UtilityOperationService.UcomMobileAccountData(PhoneNumber);
                    ucomAbonentSearch.Balance = ucomAccountData.Balance;
                    ucomAbonentSearch.PhoneNumber = ucomAccountData.PhoneNumber;
                    ucomAbonentSearch.AbonentName = ucomAccountData.AbonentName;
                    ucomAbonentSearch.Prepaid = ucomAccountData.Prepaid;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ucomAbonentSearch;
        }
    }
}
