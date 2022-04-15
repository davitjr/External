using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Ռեեստրով կոմունալ փոխանցում
    /// </summary>
    public class ReestrUtilityPaymentOrder : UtilityPaymentOrder
    {
        /// <summary>
        /// ՋՕԸ-ի ռեեստրի տվյալներ
        /// </summary>
        public List<COWaterReestrDetails> COWaterReestrDetails { get; set; }

        /// <summary>
        /// Ստուգում է ՋՕԸ-ի աբոնոնտ առկա է թե ոչ
        /// </summary>
        /// <param name="abonentNumber"></param>
        /// <param name="branchCode"></param>
        /// <returns></returns>
        public bool CheckCOWaterAbonent(string abonentNumber, string branchCode)
        {
            return UtilityPaymentOrderDB.CheckCOWaterAbonent(abonentNumber, branchCode);
        }



        public new ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();




            if (this.Type == OrderType.ReestrCashCommunalPayment || this.Type == OrderType.ReestrCommunalPayment)
            {
                if (this.COWaterReestrDetails == null || this.COWaterReestrDetails.Count == 0)
                {
                    //Ներբեռնեք Excel-ի ֆայլը:
                    result.Errors.Add(new ActionError(966));
                }
                else if (string.IsNullOrEmpty(this.Branch))
                {
                    //Որոնման դաշտում նշեք մասնաճյուղը:
                    result.Errors.Add(new ActionError(317));
                }
                else if (this.AbonentFilialCode == 0)
                {
                    //Որոնման դաշտում նշեք ՋՕԸ-ի մասնաճյուղը:
                    result.Errors.Add(new ActionError(967));
                }

                if (this.COWaterReestrDetails.Count > 300)
                {
                    //Գործարքների քանակը մեծ է 300-ից:Առավելագույն գործարքների քանակը պետք է լինի 300:
                    result.Errors.Add(new ActionError(1266));

                }
                else
                {
                    foreach (COWaterReestrDetails COWaterReestrDetail in this.COWaterReestrDetails)
                    {

                        if (!CheckCOWaterAbonent(COWaterReestrDetail.AbonentNumber, this.AbonentFilialCode.ToString()))
                        {
                            //{0} փաստաթղթի համարով կոդը գտնված չէ
                            result.Errors.Add(new ActionError(1012, new string[] { "«" + COWaterReestrDetail.OrderNumber.ToString() + "»" }));
                        }
                    }
                }

            }

            if (result.Errors.Count == 0)
                result.Errors.AddRange(base.Validate(user).Errors);


            return result;
        }


        internal void GetReestrUtilityPaymentOrder()
        {
            UtilityPaymentOrderDB.GetUtilityPaymentOrder(this);
            UtilityPaymentOrderDB.GetReestrUtilityPaymentOrderDetails(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);

        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            base.Complete();
            ActionResult result = this.Validate(user);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = UtilityPaymentOrderDB.SaveReestrUtilityPaymentOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                UtilityPaymentOrderDB.SaveReestrUtilityPaymentOrderDetails(this);

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                result = base.SaveOrderOPPerson();

                if (source == SourceType.Bank || ((source == SourceType.MobileBanking || source == SourceType.AcbaOnline) && bool.Parse(ConfigurationManager.AppSettings["TransactionTypeByAMLForMobile"].ToString())))
                {
                    result = base.SaveTransactionTypeByAML(this);
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }

            }

            //Ժամանակավոր մինչև բոլորի ավտոմատ մասը լինի

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && (this.Type == OrderType.CashCommunalPayment || this.Type == OrderType.ReestrCashCommunalPayment))
                {
                    warnings.AddRange(user.CheckForNextCashOperation(this));
                }
            }
            catch
            {

            }

            result.Errors = warnings;

            return result;
        }

    }
}
