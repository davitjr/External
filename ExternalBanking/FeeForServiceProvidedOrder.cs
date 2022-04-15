using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Հաճախորդին տրամադրվող ծառայությունների դիմաց միջնորդավճարի գանձման հայտ
    /// </summary>
    public class FeeForServiceProvidedOrder : Order
    {
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ (դրամարկղի հաշիվ)
        /// </summary>
      //  public override Account DebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշիվ
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Ծառայության տեսակ
        /// </summary>
        public ushort ServiceType { get; set; }

        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string ServiceTypeDescription { get; set; }


        /// <summary>
        /// Հաճախորդի ռեզիդենտություն
        /// </summary>
        public short CustomerResidence { get; set; }


        /// <summary>
        /// Հարկային հաշվի տրամադրում
        /// </summary>

        public Boolean TaxAccountProvision { get; set; }






        /// <summary>
        /// Պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateFeeForServiceProvidedOrder(this, user));
            return result;
        }

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        public void Complete()
        {
            this.SubType = 1;

            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Type == OrderType.FeeForServiceProvided)
            {
                this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);

                var customer = ACBAOperationService.GetCustomerMainData(this.CustomerNumber);

                this.CustomerResidence = (short)customer.ResidenceType;
            }
            else
            {
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.DebitAccount.Currency, user.filialCode);
            }

            this.ReceiverAccount = GetCreditAccount();

            this.Currency = this.DebitAccount.Currency;

        }

        /// <summary>
        ///Հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            if (this.Type == OrderType.FeeForServiceProvided) // անկանխիկի դեպքում
            {
                result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }


        public static double GetServiceFee(ulong customerNumber, OrderType type, ushort serviceType)
        {
            double serviceFee = 0;
            int vipType = 0;

            string fieldName = "price";

            if (type == OrderType.FeeForServiceProvided)
            {
                var VipData = ACBAOperationService.GetCustomerVipData(customerNumber);
                vipType = VipData.vipType.key;

                if (vipType == 7)
                {
                    fieldName = "price_for_group_1";
                }
                else if (vipType == 8)
                {
                    fieldName = "price_for_group_2";
                }
                else if (vipType == 9)
                {
                    fieldName = "price_for_group_3";
                }
            }

            serviceFee = Utility.GetPriceInfoByIndex(serviceType, fieldName);

            return serviceFee;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
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
                result = FeeForServiceProvidedOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderFee();

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

            result = base.Confirm(user);


            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CashFeeForServiceProvided)
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

        public void Get()
        {
            FeeForServiceProvidedOrderDB.Get(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }

        public Account GetCreditAccount()
        {
            return FeeForServiceProvidedOrderDB.GetCreditAccount(this);
        }

    }
}
