using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;



namespace ExternalBanking.UtilityPaymentsManagment
{
    public class VivaCellBTFCheckDetails
    {
        #region Properties

        /// <summary>
        /// Գործարքի ունիկալ համար, որը նախատեսված է հետագայում տվյալ վճարմանը հղվելու համար (օր. RPCP.BTFProcess, RPCP.Status կամ RPCP.Cancel-ի միջոցով)
        /// </summary>
        public string PaymentTransactionID { get; set; }

        /// <summary>
        /// Date and time (set by RPCS, RPCS server time) of the last state transition indicated by current statusCode
        /// </summary>
        public string PaymentTransactionDateTime { get; set; }

        /// <summary>
        /// VivaCell-ի տրամադրած PaymentTransactionDateTime դաշտը` DateTime ֆորմատավորմամբ
        /// </summary>
        public DateTime? PaymentTransactionDateTimeFormated { get; set; }

        #endregion


        public VivaCellBTFCheckDetails CheckTransferPossibility(string TransferNote, double amount)
        {
            VivaCellPaymentBTFCheckRequestResponse vivaCellPaymentBTFCheckRequestResponse = new VivaCellPaymentBTFCheckRequestResponse();

            try
            {
                vivaCellPaymentBTFCheckRequestResponse = UtilityOperationService.VivaCellBTFCheck(TransferNote, amount);

                if ((ResultCode)vivaCellPaymentBTFCheckRequestResponse.ActionResult.ResultCode == ResultCode.Normal)
                {
                    this.PaymentTransactionID = vivaCellPaymentBTFCheckRequestResponse.VivaCellPaymentBTFCheckOutput.PaymentTransactionID;
                    this.PaymentTransactionDateTime = vivaCellPaymentBTFCheckRequestResponse.VivaCellPaymentBTFCheckOutput.PaymentTransactionDateTime;
                    this.PaymentTransactionDateTimeFormated = vivaCellPaymentBTFCheckRequestResponse.VivaCellPaymentBTFCheckOutput.PaymentTransactionDateTimeFormated;
                }
                else
                {
                    this.PaymentTransactionID = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return this;
        }
    }
}
