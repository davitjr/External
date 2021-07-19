using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.SberTransfers.Models
{
    public class SberPreTransferRequisites
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CreditCurrency { get; set; }
        public string ReceiverAccount { get; set; }
        public string CreditCard { get; set; }
    }
}
