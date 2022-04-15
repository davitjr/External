using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class CardRegistrationOrder : Order
    {
        /// <summary>
        /// Մուտքագրման ենթքակա քարտը
        /// </summary>
        public PlasticCard Card { get; set; }

        /// <summary>
        /// Քարտային հաշիվ
        /// </summary>
        public Account CardAccount { get; set; }

        /// <summary>
        /// 1 - կցվում է նոր հաշիվ, 2 - կցվում է առկա հաշիվ  
        /// </summary>
        public ushort IsNewAccount { get; set; }

        /// <summary>
        /// 1 - կցվում է նոր հաշիվ, 2 - կցվում է առկա հաշիվ  
        /// </summary>
        public ushort IsNewOverdraftAccount { get; set; }

        /// <summary>
        /// Գերածախսի հաշիվ
        /// </summary>
        public Account OverdraftAccount { get; set; }

        public string AddInf { get; set; }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.Card = PlasticCard.GetPlasticCard(this.Card.ProductId, true);
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);


            if (this.Card.SupplementaryType != SupplementaryType.Main)
            {
                Card mainCard = ExternalBanking.Card.GetCard(this.Card.MainCardNumber);
                if (mainCard != null)
                {
                    this.CardAccount = mainCard.CardAccount;
                    this.OverdraftAccount = mainCard.OverdraftAccount;
                }
            }
        }

        /// <summary>
        /// Քարտի գրանցման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, bool forActivateAndOpenProductAccounts = false)
        {
            this.Complete(source);
            ActionResult result = new ActionResult();
            ActionResult resultConfirm = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardRegistrationOrderDocument(this, user));

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardRegistrationOrderDB.Save(this, userName, source);

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

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    if (forActivateAndOpenProductAccounts)
                    {
                        resultConfirm = base.Confirm(user);

                        if (resultConfirm.ResultCode == ResultCode.Normal)
                        {
                            ArcaCardsTransactionOrderData data = new ArcaCardsTransactionOrderData();
                            data.CardNumber = this.Card.CardNumber;
                            data.ExpDate = this.Card.ExpiryDate.Substring(2, 4) + this.Card.ExpiryDate.Substring(0, 2);
                            ArcaCardsTransactionResponse arcaCardsTransactionResponse = ArcaDataService.MakeCardTransaction(CardTransactionType.Unblocking, data);

                            //arcaCardsTransactionResponse.ResultCode = TransactionProcessingResultType.Normal; //REMOVE WHEN Testing With ARCA OR When Production

                            if (arcaCardsTransactionResponse.ResultCode == TransactionProcessingResultType.Normal)
                            {
                                resultConfirm = PlasticCard.UpdateCardStatusWithoutOrder(this.Card.ProductId);

                                if (resultConfirm.ResultCode == ResultCode.Normal)
                                {
                                    scope.Complete();
                                }
                                else
                                {
                                    resultConfirm.Errors.Add(new ActionError());
                                    resultConfirm.Errors[0].Description = "Տեղի ունեցավ սխալ";
                                }
                            }
                            else
                            {
                                resultConfirm.Errors.Add(new ActionError());
                                resultConfirm.Errors[0].Description = arcaCardsTransactionResponse.ResultCodeDescription;
                                resultConfirm.ResultCode = ResultCode.Failed;
                            }
                        }

                    }
                    else
                    {
                        scope.Complete();
                    }



                }
                else
                {
                    return result;
                }
            }
            if (!forActivateAndOpenProductAccounts)
            {
                resultConfirm = base.Confirm(user);
            }

            return resultConfirm;
        }



        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardRegistrationOrderDocument(this, user));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է քարտի մուտքագրման հայտի տվյալները
        /// </summary>
        /// <returns></returns>
        public void Get()
        {
            CardRegistrationOrderDB.GetCardRegistrationOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }


        /// <summary>
        ///Ստուգում է հաշվի հասանելիությունը քարտի գրանցման համար
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountType"></param>
        /// <param name="mainCardNumber"></param>
        /// <returns></returns>
        public static bool IsAccountUseForAnotherCard(string account, int accountType, string mainCardNumber = "")
        {
            return CardRegistrationOrderDB.IsAccountUseForAnotherCard(account, accountType, mainCardNumber);
        }


        /// <summary>
        /// Քարտի մուտքագրման վերաբերյալ զգուշացումների վերադարձ
        /// </summary>
        /// <param name="plasticCard"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static List<string> GetCardRegistrationWarnings(PlasticCard plasticCard, Culture culture)
        {

            List<string> warnings = new List<string>();
            ActionResult result = new ActionResult();

            //Հաշիվ չընտրելու դեպքում քարտին կկցվի հաճախորդի ազատ քարտային/գերածախսի հաշիվներից մեկը
            result.Errors.Add(new ActionError(1072));
            Localization.SetCulture(result, culture);

            if (plasticCard.CardType == 41 && PlasticCardDB.IsThirdAttachedCard(plasticCard))
            {
                //Անհրաժեշտ է կատարել 3-րդ/4-րդ կից քարտի սպասարկման միջնորդավճարի գանձում:
                result.Errors.Add(new ActionError(892));
                Localization.SetCulture(result, culture);
            }

            result.Errors.ForEach(m =>
            {
                warnings.Add(m.Description);
            });


            return warnings;
        }

        public static bool IsSecondRegistration(ulong productId)
        {
            return CardRegistrationOrderDB.IsSecondRegistration(productId);
        }


    }
}
