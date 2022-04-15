using System.Collections.Generic;

namespace ExternalBanking
{
    public class HBDocumentCustomerDetails
    {
        public string CustomerDescription { get; set; }
        public string PosOnlineDescription { get; set; }
        public Dictionary<string, string> ExtractsReceivingTypeDescription { get; set; }
        //public string ExtractsReceivingType { get; set; }

        public string ProvisionAMDDescription { get; set; }
        public double ProvisionAMDAmount { get; set; }

        public string ProvisionUSDDescription { get; set; }
        public double ProvisionUSDAmount { get; set; }

        public string ProvisionEURDescription { get; set; }
        public double ProvisionEURAmount { get; set; }

        public string ProvisionRURDescription { get; set; }
        public double ProvisionRURAmount { get; set; }

        public string DAHKDescription { get; set; }
        public string DAHKAmount { get; set; }

        public string MovedAmountDescription { get; set; }
        public string MovedAmount { get; set; }

        public string LastUpdatedInfoDescription { get; set; }
        public string LastUpdatedDate { get; set; }

        public string AMLDescription { get; set; }
        public string AML { get; set; }

        public string DocDeficientReasonDescription { get; set; }
        public string DocDeficientReason { get; set; }
        public string DocDeficientReasonDate { get; set; }

        public string CustomerVIPTypes { get; set; }

        public int CustomerPOS { get; set; }

        public int CustomerIsMember { get; set; }

        public bool IsCustomerBirthday { get; set; }

        public bool ObjectEmpty { get; set; }

        public int AccountType { get; set; }

        public string AparikDescription { get; set; }

        public string ClosingDate { get; set; }

        public string FreezeDate { get; set; }

        public double Debt { get; set; }

        public double DebtAcc { get; set; }

        public double DebtHb { get; set; }

        public double UnUsedAmount { get; set; }

        public string UnUsedAmountDate { get; set; }

    }
}
