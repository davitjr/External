using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class TransferAdditionalData
    {
        /// <summary>
        /// Փոխանցման տվյալներին համապատասխան հայտի համարը
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// Գումար ուղարկողի բնակության վայրը
        /// </summary>
        public int SenderLivingPlace { get; set;}
        /// <summary>
        /// Գումարի ուղարկողի բնակության վայրի նկարագրությունը
        /// </summary>
        public string SenderLivingPlaceDescription { get; set; }
        /// <summary>
        /// / Գումար ստացողի բնակության վայրը
        /// </summary>
        public int ReceiverLivingPlace { get; set; }
        /// <summary>
        ///  Գումարի ստացողի բնակության վայրի նկարագրությունը
        /// </summary>
        public string ReceiverLivingPlaceDescription { get; set;}
        /// <summary>
        /// Փոխանցման գումարի բնույթը
        /// </summary>
        public int TransferAmountType  { get; set; }
        /// <summary>
        /// Փոխանցման գումարի բնույթի նկարագրությունը
        /// </summary>
        public string TransferAmountTypeDescription { get; set; }
        /// <summary>
        /// Փոխանցվող գումարի նպատակները (նպատակ, գումար)
        /// </summary>
        public List<TransferAmountPurposeDetail> TransferAmountPurposes { get; set;}
        /// <summary>
        /// Պահպանում է փոխանցման լրացուցիչ տվյալները
        /// </summary>
        /// <param name="transferData"></param>
        public static  void Save(TransferAdditionalData transferData) {
            TransferAdditionalDataDB.Save(transferData);
        }

        public static TransferAdditionalData GetTransferAdditionalData(long orderId) {
           return TransferAdditionalDataDB.GetTransferAdditionalData(orderId);
        }
    }
    public class TransferAmountPurposeDetail {
        /// <summary>
        /// Նպատակ
        /// </summary>
        public int PurposeCode { get; set; }
        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        ///Գումար 
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        ///Լրացուցիչ նկարագրություն 
        /// </summary>
        public string AddInfo { get; set; }
    }

}
