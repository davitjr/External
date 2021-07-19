using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class HBDocumentTransactionError
    {
        public int ID { get; set; }
        public int OperationID { get; set; }
        public string RegistrationDate { get; set; }
        public string ErrorDescription { get; set; }
        public int SetNumber { get; set; }
        public int ExternalBankingType { get; set; }

        public string SoftwareError { get; set; }
    }
}
