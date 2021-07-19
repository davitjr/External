using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class HBDocumentAccountDetails
    {
        public long? CustomerNumber { get; set; }
        public string Description { get; set; }

        public string CardNumber { get; set; }

        public string CardType { get; set; }
        public long Account { get; set; }
        public string AccountCurrency { get; set; }

        public double BalanceAMD { get; set; }
        public double BalanceCurrency { get; set; }

        public double DebitAMD { get; set; }
        public double DebitCurrency { get; set; }

        public double CreditAMD { get; set; }
        public double CreditCurrency { get; set; }

        public double EntryAMD { get; set; }
        public double EntryCurrency { get; set; }


        public Int16 VIPCustomer { get; set; }
        public Int16 HBMember { get; set; }
        public Int16 HasPOS { get; set; }
    }
}
