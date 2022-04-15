using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class TransferCallContract
    {
        /// Համաձայնագրի ունիկալ համար        
        public long ContractId { get; set; }

        /// Համաձայնագրի համար
        public long ContractNumber { get; set; }

        /// Հաճախորդի համար    
        public ulong CustomerNumber { get; set; }

        /// Հաշվեհամար
        public ulong AccountNumber { get; set; }

        /// Հաշվի նկարագրություն
        public string AccountDescription { get; set; }

        /// Քարտի համար 
        public string CardNumber { get; set; }

        /// Վարկային գծի գումար
        public double StartCapital { get; set; }

        /// Վարկային գծի կարգավիճակ
        public Int16 CreditLineQuality { get; set; }

        /// Արժույթ
        public string Currency { get; set; }

        /// Քարտի տեսակ
        public string CardType { get; set; }

        /// Համաձայնագրի մասնաճյուղ       
        public ushort ContractFilialCode { get; set; }

        /// Աշխատավարձային ծրագրի համար
        public long RelatedOfficeNumber { get; set; }

        /// Համաձայնագրի գաղտնաբառ
        public string ContractPassword { get; set; }

        /// Քարտի գաղտնաբառ
        public string MotherName { get; set; }

        /// Քատի դիմումի հեռախոսահամարներ
        public string CardPhone { get; set; }

        /// Քատի գործողության ժամկետ
        public DateTime? CardValidationDate { get; set; }

        public static List<TransferCallContract> GetTransferCallContracts(string customerNumber, string accountNumber, string cardNumber)
        {
            return TransferCallContractDB.GetTransferCallContracts(customerNumber, accountNumber, cardNumber);
        }

        public static TransferCallContract GetContractDetails(long contractId)
        {
            return TransferCallContractDB.GetContractDetails(contractId);
        }

    }
}
