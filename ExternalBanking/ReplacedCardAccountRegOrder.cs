using System;
using System.Collections.Generic;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    public class ReplacedCardAccountRegOrder : Order
    {
        /// <summary>
        /// Վերաթողարկման ենթքակա քարտը
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Քարտային հաշիվ
        /// </summary>
        public Account CardAccount { get; set; }
        /// <summary>
        /// Գերածախսի հաշիվ
        /// </summary>
        public Account OverdraftAccount { get; set; }

        /// <summary>
        /// Լրացուցիչ ինֆորմացիա
        /// </summary>
        public string AddInf { get; set; }


        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        private void Complete()
        {
            SubType = 1;
            RegistrationDate = DateTime.Now.Date;

            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);

            if (Source == SourceType.Bank)
            {
                UserId = user.userID;
            }

            Type = OrderType.ReplacedCardAccountRegOrder;
            OPPerson = SetOrderOPPerson(CustomerNumber);
        }
        /// <summary>
        /// Քարտի փոխարինման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(SourceType source, User user, short schemaType, ulong customerNumber)
        {
            Complete();

            ActionResult result = Validate(customerNumber);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = ReplacedCardAccountRegOrderDB.Save(this, source, user.userName);

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

                result = Approve(schemaType, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            result = Confirm(user);
            if (result.ResultCode == ResultCode.Normal)
            {
                PlasticCard newCard = new PlasticCard();
                newCard = PlasticCard.GetPlasticCard((ulong)Card.ProductId, true);

                ReplacedCardAccountRegOrder order = new ReplacedCardAccountRegOrder();
                try
                {
                    order = ReplacedCardAccountRegOrderDB.GetReplacedCardAccountRegOrder(this);
                }
                catch (Exception)
                {
                    result.ResultCode = ResultCode.DoneErrorInFurtherActions;
                    return result;
                }
                if (newCard.CardNumber != "" && order.Quality == OrderQuality.Completed)
                {
                    result.ResultCode = ResultCode.DoneAndReturnedValues;

                    ActionError actionError = new ActionError
                    {
                        Description = newCard.CardNumber
                    };

                    result.Errors.Add(actionError);
                }
                else if (newCard.CardNumber == "" && order.Quality == OrderQuality.TransactionLimitApprovement)
                {
                    result.ResultCode = ResultCode.SaveAndSendToConfirm;
                }
            }
            return result;
        }

        /// <summary>
        /// Ստուգում է վերաթողարկման ենթակա քարտի առկայությունը
        /// </summary>
        /// <returns></returns>
        public bool CheckReNewRePlaceCardProductId()
        {
            return ReplacedCardAccountRegOrderDB.CheckRePlaceCardProductId((ulong)Card.ProductId, Card.FilialCode);
        }

        /// <summary>
        /// Վերադարձնում է քարտի վերաթողարկման տվյալները
        /// </summary>
        public void GetReplacedCardAccountRegOrder()
        {
            ReplacedCardAccountRegOrderDB.GetReplacedCardAccountRegOrder(this);
        }

        /// <summary>
        /// Փոխարինված քարտի հաշվի կցում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Validate(ulong customerNumber)
        {
            ActionResult result = new ActionResult
            {
                Errors = new List<ActionError>()
            };

            int currentBankCode = user.filialCode;
            if (currentBankCode == 22059 && Card.FilialCode == 22000)
            {
                currentBankCode = Card.FilialCode;
            }
            if (Card.FilialCode != currentBankCode)
            {
                //Այլ մասնաճյուղի քարտ:
                result.Errors.Add(new ActionError(1515));
                return result;
            }

            if (!CheckCustomerUpdateExpired)
            {
                // Հաճախորդի տվյալները թարմացված չեն:
                result.Errors.Add(new ActionError(1664));
            }

            List<Card> supplementaryCards = new List<Card>();
            supplementaryCards.AddRange(CardDB.GetLinkedCards(Card.CardNumber));
            supplementaryCards.AddRange(CardDB.GetAttachedCards((ulong)Card.ProductId, CustomerNumber));
            if (supplementaryCards.Count > 0)
            {
                //Առկա են կից կամ լրացուցիչ քարտեր։ Հիմնական քարտը փոխարինելու համար անհրաժեշտ է փակել կից կամ լրացուցիչ քարտերը:
                result.Errors.Add(new ActionError(1669));
            }
            PlasticCard plasticCard = new PlasticCard();
            plasticCard = PlasticCard.GetPlasticCard((ulong)Card.ProductId, true);
            if (plasticCard.SupplementaryType == SupplementaryType.Attached || plasticCard.SupplementaryType == SupplementaryType.Linked)
            {
                //Կից/լրացուցիչ քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1807));
            }

            CardTariffContract cardTariff = new CardTariffContract
            {
                TariffID = plasticCard.RelatedOfficeNumber
            };
            CardTariffContractDB.GetCardTariffContract(cardTariff);

            if (cardTariff.Quality == 0)
            {
                //Տվյալ աշխ․ ծրագիրը գործող չէ։ Խնդրում ենք ընտրել համապատասխան աշխ․ ծրագիր, որը հնարավոր չէ փոփոխել հայտը պահպանելուց հետո։
                result.Errors.Add(new ActionError(1663));
            }

            ACBAServiceReference.Customer customer = Customer.GetCustomer(customerNumber);
            if (customer.quality.value == "43")
            {
                //Ընտրված է կրկնակի հաճախորդ: Հնարավոր չէ քարտը փոխարինել կրկնակի հաճախորդի համար:
                result.Errors.Add(new ActionError(1662, new string[] { "փոխարինել" }));
            }

            string MotherName = Card.GetCardMotherName((ulong)Card.ProductId);
            if (string.IsNullOrEmpty(MotherName))
            {
                //Հաճախորդի գաղտնաբառ դաշտը բացակայում է:
                result.Errors.Add(new ActionError(1665));
            }

            int productType = Utility.GetCardProductType(Convert.ToUInt32(Card.Type));
            if (!Validation.CheckProductAvailabilityByCustomerCountry(customerNumber, productType))
            {
                // Հաճախորդի տվյալ տիպի քարտը հնարավոր չէ փոխարինել քաղաքացիության, ռեզիդենտության կամ կենսական շահերի կենտրոն հանդիսացող տվյալ երկրի պատճառով։
                result.Errors.Add(new ActionError(1666, new string[] { "փոխարինել" }));
            }

            CardTariffContractDB.GetCardTariffs(cardTariff);
            CardTariff tariff = cardTariff.CardTariffs.Find(m => m.CardType == Card.Type && m.Currency == Card.Currency);
            if (!ReplacedCardAccountRegOrderDB.CheckCardRelatedOffice(this))
            {
                //Տվյալ աշխատավարձային ծրագրում նախատեսված չէ տվյալ արժույթով տվյալ քարտի տեսակը:
                result.Errors.Add(new ActionError(1633));
            }
            if (customer is PhysicalCustomer physicalCustomer)
            {
                if (string.IsNullOrEmpty(physicalCustomer.person.fullName.firstNameEng) || string.IsNullOrEmpty(physicalCustomer.person.fullName.lastNameEng))
                {
                    //Մուտքագրեք քարտապանի անունն ու ազգանունը:
                    result.Errors.Add(new ActionError(1582));
                }
                if (string.IsNullOrEmpty(physicalCustomer.person.fullName.MiddleName))
                {
                    //Քարտապանի հայրանունը լրացված չէ
                    result.Errors.Add(new ActionError(1614));
                }

                if (!physicalCustomer.person.birthDate.HasValue)
                {
                    //Քարտապանի ծննդյան ա/թ-ն լրացված չէ
                    result.Errors.Add(new ActionError(1615));
                }
                CustomerAddress customerAddress = physicalCustomer.person.addressList.Find(m => m.addressType.key == (short)AddressTypes.registration);
                if (customerAddress is null)
                {
                    //Գրանցման հասցե չկա
                    result.Errors.Add(new ActionError(1607));
                }
            }

            if (Card.ProductId == 0)
            {
                //Քարտը գտնված չի VisaCardApplication-ում
                result.Errors.Add(new ActionError(995));
                return result;
            }

            if (Card.Type == 16 || Card.Type == 1)
            {
                // Visa Electron քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1678));
            }

            if (Card.Type == 14 || Card.Type == 32)
            {
                // Maestro տեսակի քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1679));
            }

            if (!NonCreditLineCardReplaceOrderDB.IfArcaContractIdExists(Card.CardSystem, Card.Currency))
            {
                //ARCA պայմանագրի համարը մուտքագրված չէ:
                result.Errors.Add(new ActionError(1667));
            }

            if (customer.residence.key == 2 && Card.CardSystem == (int)PlasticCardSystemType.Amex)
            {
                //Ոչ ռեզիդենտ հաճախորդների համար AmEx քարտերի պատվիրումը թույլատրված չէ։
                result.Errors.Add(new ActionError(1668));
            }

            if (Card.Type == 51)
            {
                // Virtual տեսակի քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1775));
            }

            if (cardTariff.Quality == 0 || cardTariff.Quality == 2)
            {
                //Քարտի աշխատավարձային ծրագիրը դադարեցված է կամ սառեցված է:
                result.Errors.Add(new ActionError(996));
                return result;
            }
            if (Card.ClosingDate != null && Type == OrderType.NonCreditLineCardReplaceOrder)
            {
                //Ընտրված է փակված քարտ:
                result.Errors.Add(new ActionError(532));
            }
            if (Card.OpenDate == DateTime.Today)
            {
                //Քարտի ժամկետը փոխված չէ:
                result.Errors.Add(new ActionError(1670));
            }
            if (!ReplacedCardAccountRegOrderDB.IfCardAccountExists(this))
            {
                //Քարտային հաշիվը բացակայում է։
                result.Errors.Add(new ActionError(1672));
            }
            return result;
        }
    }
}
