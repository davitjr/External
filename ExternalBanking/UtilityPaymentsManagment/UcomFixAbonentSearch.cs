using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;

namespace ExternalBanking.UtilityPaymentsManagment
{
    public class UcomFixAbonentSearch
    {
        public string AbonentNumber { get; set; }
        public string Client { get; set; }

        public BalanceElement Balance { get; set; }

        public class BalanceElement
        {
            public double Internet { get; set; }

            public double Phone { get; set; }

            public double TV { get; set; }

            public double Other { get; set; }

            public double Total { get; set; }
        }
        public string TotalBalance { get; set; } = null;

        public UcomFixAbonentSearch GetUcomFixAbonentSearch(string AbonentNumber)
        {

            var ucomAbonentSearch = new UcomFixAbonentSearch();
            try
            {
                UcomFixAbonentCheckRequestResponse ucomAccountData = UtilityOperationService.UcomFixAbonentNumberCheck(AbonentNumber);
                if (ucomAccountData.ActionResult.ResultCode == UtilityPaymentsServiceReference.ResultCode.Normal)
                {
                    ucomAbonentSearch.Client = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Client;
                    ucomAbonentSearch.AbonentNumber = AbonentNumber;
                    ucomAbonentSearch.Balance = new BalanceElement();
                    ucomAbonentSearch.Balance.Phone = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Balance.Phone;
                    ucomAbonentSearch.Balance.Internet = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Balance.Internet;
                    ucomAbonentSearch.Balance.TV = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Balance.TV;
                    ucomAbonentSearch.Balance.Other = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Balance.Other;
                    ucomAbonentSearch.Balance.Total = ucomAccountData.UcomFixAbonentCheckOutput.Extrak__BackingField.Balance.Total;
                    ucomAbonentSearch.TotalBalance = ucomAbonentSearch.Balance.Total.ToString("F");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ucomAbonentSearch;
        }
    }
}
