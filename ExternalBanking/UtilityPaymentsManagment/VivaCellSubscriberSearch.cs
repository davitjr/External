using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.UtilityPaymentsManagment
{
    public class VivaCellSubscriberSearch
    {
        #region Properties

        /// <summary>
        /// Բաժանորդի GSM համարը
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Հաճախորդի տեսակը 
        /// </summary>
        public VivaCellSubscriberType SubscriberType { get; set; }

        /// <summary>
        /// Վճարման նվազագույն գումար (կանխավճարային դեպքում)
        /// </summary>
        public int? MinAmount { get; set; }

        /// <summary>
        /// Առավելագույն վճարային գումարը (կանխավճարային դեպքում)
        /// </summary>
        public int? MaxAmount { get; set; }

        /// <summary>
        /// Հաճախորդի անունը (հետվճարային դեպքում եւ թույլտվության դեպքում)
        /// </summary>
        public string SubscriberName { get; set; }

        /// <summary>
        /// Պայմանագրի մնացորդը (հետվճարային դեպքում եւ մուտքի թույլատրելիության դեպքում)
        /// </summary>
        public float? BalanceContract { get; set; }

        /// <summary>
        /// Բաժանորդային մնացորդը (հետվճարային դեպքում եւ մուտքի թույլատրելի դեպքում)
        /// </summary>
        public float? BalanceSub { get; set; }

        
        /// <summary>
        /// Հաշվառման ամսաթիվ
        /// Ավելացել է մեր պահանջով
        /// </summary>
        public string BilledToDate { get; set; }

        #endregion


        public VivaCellSubscriberSearch GetVivaCellSubscriberDetails(string phoneNumber)
        {
            VivaCellPaymentCheckRequestResponse vivaCellPaymentCheckRequestResponse = new VivaCellPaymentCheckRequestResponse();

            try
            {
                    vivaCellPaymentCheckRequestResponse = UtilityOperationService.VivaCellSubscriberCheck(phoneNumber);

                    if ((ResultCode)vivaCellPaymentCheckRequestResponse.ActionResult.ResultCode == ResultCode.Normal)
                    {
                        this.PhoneNumber = phoneNumber;
                        this.BalanceContract = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.BalanceContract;
                        this.BalanceSub = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.BalanceSub;
                        this.BilledToDate = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.BilledToDate;
                        this.MaxAmount = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.MaxAmount;
                        this.MinAmount = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.MinAmount;
                        this.SubscriberName = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.SubscriberName;
                        this.SubscriberType = vivaCellPaymentCheckRequestResponse.VivaCellPaymentCheckOutput.SubscriberType;
                    }
                    else
                    {
                        this.PhoneNumber = null;
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
