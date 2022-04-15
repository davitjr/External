using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LeasingAccountsDetails
    {
        public long Accounts { get; set; }
        public string Currency { get; set; }
        public double Balance { get; set; }
        public string TypeOfAccount { get; set; }

        public string Description { get; set; }

    }

    public class LeasingDebtsDetails
    {
        public string Description { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime PayDate { get; set; }
        public double Amount { get; set; }
    }

    public class LeasingDetailedInformation
    {
        public double CurrentPeriodCapital { get; set; }
        public double CurrentPeriodRent { get; set; }
        public int IsSubsid { get; set; }

        public double PropertyTaxLiability { get; set; }

        public List<LeasingAccountsDetails> AccountsDetails { get; set; }
        public List<LeasingDebtsDetails> DebtsDetails { get; set; }

    }
    public class LeasingInsuranceDetails
    {
        public int Id { get; set; }
        public double SumAmd { get; set; }
        public string OperDescription { get; set; }
        public DateTime PayDate { get; set; }

        //public List<LeasingInsuranceDetails> InsuranceDetails { get; set; }

    }

}
