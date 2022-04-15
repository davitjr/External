using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class TransferCallContractDetails
    {
        /// <summary>
        /// Համարայնագրի ունիկալ համար
        /// </summary>
        public long ContractId { get; set; }
        /// <summary>
        /// Համաձայնագրի համար
        /// </summary>
        public long ContractNumber { get; set; }

        /// <summary>
        /// Համաձայնագրի գաղտնաբառ
        /// </summary>
        public string ContractPassword { get; set; }

        /// <summary>
        /// Համաձայնագրի ա/թ
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Ներգրավող ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }

        /// <summary>
        /// Համաձայնագրի հաշիվ
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// Համաձայնագրի կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Համաձայնագրի կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }




        public static List<TransferCallContractDetails> GetTransferCallContractsDetails(ulong customerNumber)
        {
            return TransferCallContractDetailsDB.GetTransferCallContractsDetails(customerNumber);
        }

        public static TransferCallContractDetails GetTransferCallContractDetails(long contractId, ulong customerNumber)
        {
            return TransferCallContractDetailsDB.GetTransferCallContractDetails(contractId, customerNumber);
        }



    }
}
