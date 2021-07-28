using System;
using System.Collections.Generic;
using System.Transactions;
using ExternalBanking.DBManager;
namespace ExternalBanking
{
    public class RenewedCardAccountRegOrder : Order
    {
        /// <summary>
        /// Վերաթողարկման ենթքակա քարտը
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Գերածախսի հաշիվ
        /// </summary>
        public Account OverdraftAccount { get; set; }

        /// <summary>
        /// Քարտային հաշիվ
        /// </summary>
        public Account CardAccount { get; set; }


        /// <summary>
        /// Լրացուցիչ ինֆորմացիա
        /// </summary>
        public string AddInf { get; set; }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            RegistrationDate = DateTime.Now.Date;
            SubType = 1;
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);

            OPPerson = SetOrderOPPerson(CustomerNumber);

            Card mainCard = Card.GetCard(Card.MainCardNumber);
            if (Card.SupplementaryType != SupplementaryType.Main && mainCard != null)
            {
                CardAccount = mainCard.CardAccount;
                OverdraftAccount = mainCard.OverdraftAccount;
            }
        }
        /// <summary>
        /// Քարտի վերաթողարկման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            Complete();
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validate(this, user));

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = RenewedCardAccountRegOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);
            if (resultConfirm.ResultCode == ResultCode.Normal)
            {
                resultConfirm.ResultCode = ResultCode.DoneAndReturnedValues;
                //Հայտը կատարված է։
                resultConfirm.Errors.Add(new ActionError(1944));
            }
            return resultConfirm;
        }

        public bool CheckRenewCardProductId()
        {
            return RenewedCardAccountRegOrderDB.CheckRenewCardProductId((ulong)Card.ProductId, Card.FilialCode);
        }
        public static DateTime LastClosedCard(string cardNumber)
        {
            return RenewedCardAccountRegOrderDB.LastClosedCard(cardNumber);
        }

        /// <summary>
        /// Երբ մուտքագրվում է հայտ՝ երկրորդ անգամ
        /// </summary>
        /// <returns></returns>
        public bool IsSecondRenew()
        {
            return RenewedCardAccountRegOrderDB.IsSecondRenew((ulong)Card.ProductId);
        }

        /// <summary>
        /// Վերադարձնում է Վերաթողարկված քարտի հաշվի կցման տվյալները
        /// </summary>
        public RenewedCardAccountRegOrder GetRenewedCardAccountRegOrder(long ID)
        {
            return RenewedCardAccountRegOrderDB.GetRenewedCardAccountRegOrder(ID);
        }

        /// <summary>
        /// վերադարձնում է վերաթողարկվող քարտի հին app_id-ն
        /// </summary>
        /// <returns></returns>
        public ulong GetRenewCardOldProductId()
        {
            return RenewedCardAccountRegOrderDB.GetRenewCardOldProductId((ulong)Card.ProductId, Card.FilialCode);
        }


        public static List<ActionError> GetCreditLineWarningForReNew(Card oldCard, Culture culture)
        {
            ActionResult result = new ActionResult();
            PlasticCard plasticCard = PlasticCard.GetPlasticCard((ulong)oldCard.ProductId, false);
            if (plasticCard.CardChangeType == CardChangeType.ReNew)
            {
                if (oldCard != null)
                {
                    if (plasticCard.SupplementaryType == SupplementaryType.Main && oldCard.Type != 23 && oldCard.Type != 40)
                    {
                        if (oldCard.CreditLine != null)
                        {
                            if (oldCard.ClosingDate == null)
                            {
                                double accountRest = oldCard.CreditLine.CurrentCapital;
                                List<LoanProductProlongation> list = LoanProduct.GetLoanProductProlongations((ulong)oldCard.CreditLine.ProductId);
                                LoanProductProlongation loanProlong = null;
                                if (list != null)
                                {
                                    loanProlong = list.Find(m => m.ActivationDate == null);
                                }

                                if (accountRest == 0 && loanProlong == null)
                                {
                                    //Վերաթողարկման ժամանակ վարկային գիծը դադարեցվելու է։
                                    result.Errors.Add(new ActionError(1019));
                                    Localization.SetCulture(result, culture);
                                }
                            }
                        }
                    }
                }
            }

            return result.Errors;
        }

        /// <summary>
        /// Քարտի վերաթողարկման/փոխարինման հայտի փաստաթղթի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static List<ActionError> Validate(RenewedCardAccountRegOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();
            if (!order.CheckRenewCardProductId())
            {
                //Քարտը գտնված չի VisaCardApplication-ում
                result.Add(new ActionError(995));
                return result;
            }

            if (order.IsSecondRenew())
            {
                //Նշված քարտի համար գոյություն ունի վերաթողարկման հայտ
                result.Add(new ActionError(1001, new string[] { "վերաթողարկված քարտի հաշվի կցման " }));
            }

            CreditLine creditLineOverdraft = CreditLine.GetCardOverdraft(order.Card.CardNumber);
            if (!(creditLineOverdraft is null) && Math.Abs(creditLineOverdraft.CurrentCapital) + Math.Abs(creditLineOverdraft.OutCapital) + Math.Abs(creditLineOverdraft.CurrentRateValue) != 0)
            {
                //Քարտային հաշվին առկա է գերածախս:Վերաթողարկված քարտի հաշվի կցման համար անհրաժեշտ է մարել գերածախսը: 
                result.Add(new ActionError(1931, new string[] { "Վերաթողարկված քարտի հաշվի կցման " }));
            }

            PlasticCard plasticCard = new PlasticCard();
            plasticCard = PlasticCard.GetPlasticCard((ulong)order.Card.ProductId, true);

            CardTariffContract cardTariff = new CardTariffContract();
            cardTariff.TariffID = order.Card.RelatedOfficeNumber;
            CardTariffContractDB.GetCardTariffContract(cardTariff);
            if (cardTariff.Quality == 0 || cardTariff.Quality == 2)
            {
                //Քարտի աշխատավարձային ծրագիրը դադարեցված է կամ սառեցված է:
                result.Add(new ActionError(996));
                return result;
            }
            CardTariffContractDB.GetCardTariffs(cardTariff);
            if (!cardTariff.CardTariffs.Exists(m => m.CardType == plasticCard.CardType && m.Currency == order.Card.Currency))
            {
                //Ընտրված աշխատավարձային ծրագրում նախատեսված չէ տվյալ քարտի տիպը
                result.Add(new ActionError(888));
            }
            else
            {
                CardTariff tariff = cardTariff.CardTariffs.Find(m => m.CardType == plasticCard.CardType && m.Currency == order.Card.Currency);
                if (tariff.Quality == 0)
                {
                    //Այս տեսակի և արժույթի քարտի կարգավիճակը դադարեցված է:
                    result.Add(new ActionError(1007));
                    return result;
                }
            }
            ulong oldProductId = order.GetRenewCardOldProductId();
            if (oldProductId == 0)
            {
                //Քարտը գտնված չի VisaCardApplication-ում
                result.Add(new ActionError(995));
                return result;
            }

            Card oldCard = Card.GetCard(oldProductId, order.CustomerNumber);
            //Card oldCard = Card.GetCard(oldProductId, order.CustomerNumber);
            if (oldCard == null)
            {
                //Քարտը գտնված չի VisaCardApplication-ում
                result.Add(new ActionError(995));
                return result;
            }

            if (oldCard.ClosingDate != null && (plasticCard.CardType == 23 || plasticCard.CardType == 40))
            {
                //Փակված Amex Blue քարտը հնարավոր չէ վերաթողարկել։
                result.Add(new ActionError(1390));
            }

            List<Card> attachedCards = Card.GetAttachedCards((ulong)oldCard.ProductId, order.CustomerNumber);

            if (attachedCards.Count > 0 && (oldCard.Type == 23 || oldCard.Type == 20 || (oldCard.Type == 21 && oldCard.CardNumber.Substring(0, 8) == "90513410")))
            {
                //Հնարավոր չէ իրականացնել գործողությունը։ Անհրաժեշտ է փակել կից քարտերը։
                result.Add(new ActionError(1273));
            }


            if (order.Card.SupplementaryType == SupplementaryType.Main && oldCard.Type != 23 && oldCard.Type != 40)
            {
                if (oldCard.CreditLine != null)
                {
                    if (oldCard.ClosingDate == null)
                    {
                        double accountRest = oldCard.CreditLine.CurrentCapital;
                        List<LoanProductProlongation> list = LoanProduct.GetLoanProductProlongations((ulong)oldCard.CreditLine.ProductId);
                        LoanProductProlongation loanProlong = null;
                        if (list != null)
                        {
                            loanProlong = list.Find(m => m.ActivationDate == null);
                        }
                        //if (loanProlong is null)
                        //{
                        //    //Անհրաժեշտ է երկարաձգել կամ դադարեցնել վարկային գիծը:
                        //    result.Add(new ActionError(1893));
                        //}

                        if (accountRest != 0 && loanProlong == null)
                        {
                            //Քարտի վարկային հաշիվը մնացորդ ունի
                            result.Add(new ActionError(1004));
                        }
                        else if (loanProlong != null && loanProlong.ConfirmationDate == null)
                        {
                            //Քարտի վարկային գծի երկարաձգման դիմումը հաստատված չէ
                            result.Add(new ActionError(1005));
                        }
                    }
                }
                if (oldCard.Overdraft != null && order.Type == OrderType.RenewedCardAccountRegOrder)
                {
                    double accountRest = oldCard.Overdraft.CurrentCapital;
                    if (accountRest != 0)
                        //Քարտի օվերդրաֆտի հաշիվը մնացորդ ունի
                        result.Add(new ActionError(1006));
                }

            }

            if (oldCard.ClosingDate != null)
            {
                DateTime openDate = LastClosedCard(oldCard.CardNumber);
                if (openDate != oldCard.OpenDate)
                {
                    //Ընտրեք տվյալ համարն ունեցող ամենավերջինը փակված քարտը:
                    result.Add(new ActionError(999));
                    return result;
                }
                Card card = Card.GetCard(oldCard.CardNumber);
                if (card != null)
                {
                    //Տվյալ համարով գոյություն ունի գործող քարտ:
                    result.Add(new ActionError(1000));
                    return result;
                }

                if (order.Card.SupplementaryType != SupplementaryType.Main)
                {
                    Card mainCard = Card.GetCard(order.Card.MainCardNumber);
                    if (mainCard == null)
                    {
                        //Հիմնական քարտը գտնված չէ
                        result.Add(new ActionError(899));
                    }
                }



                int accountType = 1; // 1 - Քարտային հաշիվ, 2 - Գերածախսի հաշիվ

                result.AddRange(Validation.ValidateCardProductAccounts(plasticCard, order.CardAccount, accountType,
                    order.CustomerNumber, user));

                accountType = 2;
                result.AddRange(Validation.ValidateCardProductAccounts(plasticCard, order.OverdraftAccount, accountType,
                    order.CustomerNumber, user));

                accountType = 3;
                if (oldCard.CreditLine != null)
                    result.AddRange(Validation.ValidateCardProductAccounts(plasticCard, oldCard.CreditLine.LoanAccount,
                        accountType, order.CustomerNumber, user));
            }
            return result;
        }

        /// <summary>
        /// Քարտի վերաթողարկման(փոխարինման) վերաբերյալ զգուշացումների վերադարձ
        /// </summary>
        /// <param name="oldCard"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static List<string> GetRenewedCardAccountRegWarnings(Card oldCard, Culture culture)
        {

            List<string> warnings = new List<string>();
            ActionResult result = new ActionResult();

            if (RenewedCardAccountRegOrderDB.CheckPeriodicOperation(oldCard.CardNumber))
            {
                //Հաշիվը ներառված է պարբերական փոխանցման հանձնարարականում
                result.Errors.Add(new ActionError(512, new string[] { oldCard.CardAccount.AccountNumber }));
                Localization.SetCulture(result, culture);
            }

            result.Errors.AddRange(GetCreditLineWarningForReNew(oldCard, culture));
            result.Errors.ForEach(m =>
            {
                warnings.Add(m.Description);
            });

            return warnings;
        }
    }
}
