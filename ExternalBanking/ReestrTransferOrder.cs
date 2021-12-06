using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{
    /// <summary>
    /// Ռեեստրով փոխանցում
    /// </summary>
    public class ReestrTransferOrder : PaymentOrder
    {

        public string FileID { get; set; }

        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ
        /// </summary>
        public List<ReestrTransferAdditionalDetails> ReestrTransferAdditionalDetails { get; set; }

        public void Get()
        {
            base.Get();
            PaymentOrderDB.GetReestrTransferDetails(this);


        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PaymentOrderDB.SaveCash(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();
                               
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderFee();

                //Պահպանում է ռեեստրով փոխանցման տվյալները
                PaymentOrderDB.SaveReestrTransferDetails(this);


                if (this.Type == OrderType.CardServiceFeePayment)
                {
                    foreach (AdditionalDetails additionalDetail in this.AdditionalParametrs)
                    {
                        if (additionalDetail.Id == Constants.CARD_SERVICE_FEE_PAYMENT_ADDITIONAL_ID)
                        {
                            Order.SaveOrderProductId(Convert.ToUInt64(additionalDetail.AdditionValue), this.Id);
                        }
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

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && (this.Type == OrderType.CashConvertation || this.Type == OrderType.CashCredit || this.Type == OrderType.CashCreditConvertation
                    || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashForRATransfer || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder || this.Type == OrderType.TransitCashOut))
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

        public ActionResult ValidateReestr(User user, long id,TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (Source == SourceType.AcbaOnline && this.Type == OrderType.RosterTransfer)
            {
                result = CheckExcelRows(this.ReestrTransferAdditionalDetails, this.DebitAccount.AccountNumber, Languages.hy,id);
            }

            if (this.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && this.Type != OrderType.TransitNonCashOut)
                result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));


              //result.Errors.AddRange(Validation.ValidateOPPerson(this, this.ReceiverAccount, this.DebitAccount.AccountNumber));
            }
            if (this.TransferID != 0)
                result.Errors.AddRange(Validation.ValidateForTransfer(this, user));


            if (this.ReceiverAccount != null && this.DebitAccount != null && this.ReceiverAccount.AccountNumber == this.DebitAccount.AccountNumber && this.Type != OrderType.CashConvertation)
            {
                //Կրեդիտ և դեբիտ հաշիվները նույնն են:
                result.Errors.Add(new ActionError(11));
            }
            else
            {
                if (this.ValidateDebitAccount && this.Type != OrderType.CashOutFromTransitAccountsOrder)
                {
                    //Դեբետ հաշվի ստուգում
                    result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                }

                if (result.Errors.Count > 0)
                {
                    return result;
                }

               
            }

            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
            }
            if (this.Amount < 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Errors.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }

            byte customerType = 0;
            if (this.CustomerNumber != 0)
            {
                using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
                {
                    customerType = proxy.GetCustomerType(this.CustomerNumber);
                }
            }
            else
            {
                customerType = 6;
            }

            if (this.Type == OrderType.RosterTransfer)
            {
                ReestrTransferOrder reestrTransferOrder = (ReestrTransferOrder)this;
                int index = 1;

              
                if (reestrTransferOrder.ReestrTransferAdditionalDetails == null || reestrTransferOrder.ReestrTransferAdditionalDetails.Count == 0)
                {
                    //Մուտքագրեք փոխանցման տվյալները
                    result.Errors.Add(new ActionError(847));
                }
                else
                {
                    if (this.Type != OrderType.ReestrPaymentOrder && reestrTransferOrder.ReestrTransferAdditionalDetails.Count > 300)
                    {
                        //Գործարքների քանակը մեծ է 300-ից:Առավելագույն գործարքների քանակը պետք է լինի 300:
                        result.Errors.Add(new ActionError(1266));

                    }
                    else
                    {


                        foreach (ReestrTransferAdditionalDetails details in reestrTransferOrder.ReestrTransferAdditionalDetails)
                        {
                            details.Index = index;
                            index++;
                            if (this.Type == OrderType.RosterTransfer)
                            {
                                if (string.IsNullOrEmpty(details.Description))
                                {
                                    if((Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft) && SubType == 1)
                                    {
                                        //{0} փոխանցման վճարման նպատակը մուտքագրված չէ
                                        result.Errors.Add(new ActionError(848, new string[] { "«" + details.Index.ToString() + "»" }));
                                    }
                                    
                                }
                                else if (details.Description.Length > 130)
                                {
                                    //{0} փոխանցման «վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 125
                                    result.Errors.Add(new ActionError(849, new string[] { "«" + details.Index.ToString() + "»" }));
                                }
                                else if (Utility.IsExistForbiddenCharacter(details.Description))
                                {
                                    //{0} փոխանցման «Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                                    result.Errors.Add(new ActionError(850, new string[] { "«" + details.Index.ToString() + "»" }));
                                }
                            }

                            if (details.Amount < 0.01)
                            {
                                //{0} փոխանցման մուտքագրված գումարը սխալ է
                                result.Errors.Add(new ActionError(851, new string[] { "«" + details.Index.ToString() + "»" }));
                            }
                            else if (!Utility.IsCorrectAmount(details.Amount, this.Currency))
                            {
                                //{0} փոխանցման մուտքագրված գումարը սխալ է
                                result.Errors.Add(new ActionError(851, new string[] { "«" + details.Index.ToString() + "»" }));
                            }

                        }

                        if (Math.Round(reestrTransferOrder.ReestrTransferAdditionalDetails.Sum(m => m.Amount), 2) != reestrTransferOrder.Amount)
                        {
                            //Փոխանցման տվյալների գումարը չի համապասխանում գործարքի գումարին
                            result.Errors.Add(new ActionError(853));
                        }
                    }
                }
            }



            if (this.Source != SourceType.SSTerminal)
            {
                if (this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.CashForRATransfer && this.ValidateForCash && !this.ValidateForConvertation && this.Currency != "AMD" && !Validation.IsCurrencyAmountCorrect(this.Amount, this.Currency) || ((this.Type == OrderType.RosterTransfer || this.Type == OrderType.CashOutFromTransitAccountsOrder) && this.Currency != "AMD" && !Validation.IsCurrencyAmountCorrect(this.Amount, this.Currency)))
                {
                    //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                    result.Errors.Add(new ActionError(1053, new string[] { this.Currency }));
                }
            }
            if ((this.ValidateForCash || this.Type == OrderType.RosterTransfer) && this.Type != OrderType.CashConvertation && this.Type != OrderType.CashCreditConvertation)
            {
                if (this.Currency == "GEL")
                {
                    //{0} արժույթով գործարք կատարել հնարավոր չէ:
                    result.Errors.Add(new ActionError(1478, new string[] { this.Currency }));
                }
            }

            if (templateType != TemplateType.CreatedAsGroupService)
            {
                if (this.DebitAccount.AccountType == 115)
                {

                    Card card = Card.GetCardWithOutBallance(this.ReceiverAccount.AccountNumber);
                    if (card == null || (card.CardNumber != this.DebitAccount.ProductNumber))
                    {
                        //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվից փոխանցում հնարավոր է կատարել միայն տվյալ քարտին
                        result.Errors.Add(new ActionError(1064));
                    }

                }
            }
                       
            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }
                        
            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            return result;
        }



        public ActionResult SaveReestrTransferOrder(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string fileId)
        {

            this.CompleteReestr();
            ActionResult result = this.ValidateReestr(user,this.Id);
            
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PaymentOrderDB.SaveReestr(this, userName, source, fileId, action);

                //**********                
                //ulong orderId = base.Save(this, source, user);
                
                //Պահպանում է ռեեստրով փոխանցման տվյալները
               // PaymentOrderDB.SaveReestrTransferDetails(this);
                
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        protected void CompleteReestr()
        {
            

            if (this.DebitAccount != null && ((!this.ValidateForTransit && this.ValidateDebitAccount) || this.Type == OrderType.CardServiceFeePayment))
            {
                this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
            }

            this.RegistrationDate = DateTime.Now.Date;


            if ((this.OrderNumber == null || this.OrderNumber == ""))
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            if(Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft)
            {
                if(ReestrTransferAdditionalDetails != null)
                {
                    //this.Amount = ReestrTransferAdditionalDetails.Sum(x => x.Amount);
                }
                
            }
        }

        public static ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails,string debetAccount, Languages languages,long id = 0 )
        {
            return PaymentOrderDB.CheckExcelRows(reestrTransferAdditionalDetails, debetAccount,languages,id);
        }


    }
}
