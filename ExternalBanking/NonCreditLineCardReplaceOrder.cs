using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Text.RegularExpressions;

namespace ExternalBanking
{
    public class NonCreditLineCardReplaceOrder : Order
    {
        /// <summary>
        /// Քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Հաստատության անվանում
        /// </summary>
        public string OrganisationNameEng { get; set; }

        /// <summary>
        /// Ցույց է տալիս քարտը կից է թե ոչ
        /// </summary>
        public SupplementaryType SupplementaryType { get; set; }

        /// <summary>
        /// Ներգրավողի ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }

        /// <summary>
        /// Սպասարկողի ՊԿ
        /// </summary>
        public int ServingSetNumber { get; set; }

        /// <summary>
        /// Քաղվածքի ստացման եղանակ
        /// </summary>
        public int CardReportReceivingType { get; set; }

        /// <summary>
        /// Էլեկտրոնային փոստ
        /// </summary>
        public string ReportReceivingEmail { get; set; }

        /// <summary>
        /// PIN-ի ստացման եղանակ
        /// </summary>
        public int CardPINCodeReceivingType { get; set; }

        /// <summary>
        /// SMS հեռախոսահամար
        /// </summary>
        public string CardSMSPhone { get; set; }

        /// <summary>
        /// Աշխ․ ծրագիր
        /// </summary>
        public int RelatedOfficeNumber { get; set; }

        /// <summary>
        /// Քարտի ստացման եղանակ
        /// </summary>
        public int CardReceivingType { get; set; }

        /// <summary>
        /// Դիմումի ընդունման տարբերակ
        /// </summary>
        public int CardApplicationAcceptanceType { get; set; }

        private void Complete()
        {
            SubType = 1;

            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);

            if (Source == SourceType.Bank)
            {
                UserId = user.userID;
            }

            Type = OrderType.NonCreditLineCardReplaceOrder;
            OPPerson = SetOrderOPPerson(CustomerNumber);
        }

        /// <summary>
        /// Առանց վարկային գծի քարտի փոխարինման հայտի պահպանում և ուղարկում
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
                result = NonCreditLineCardReplaceOrderDB.Save(this, source, user);

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

                ActionResult res = Approve(schemaType, user.userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                }
                else
                {
                    return res;
                }
                scope.Complete();
            }

            result = Confirm(user);

            if (result.ResultCode == ResultCode.Normal)
            {
                ulong cardNewAppID = GetCardNewAppID();
                PlasticCard newCard = new PlasticCard();
                newCard = PlasticCard.GetPlasticCard(cardNewAppID, true);
                NonCreditLineCardReplaceOrder order = new NonCreditLineCardReplaceOrder();

                try
                {
                    order = NonCreditLineCardReplaceOrderDB.GetNonCreditLineCardReplaceOrder(this);
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
        /// Առանց վարկային գծի քարտի փոխարինման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Validate(ulong customerNumber)
        {

            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

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

            if (IsAlreadyReplaced(Card.ProductId))
            {
                //Նշված քարտի համար արդեն կատարվել է փոխարինման գործողություն:
                result.Errors.Add(new ActionError(1811, new string[] { "փոխարինման" }));
                return result;
            }

            if (IsAlreadyExistReplaceOrder(Card.ProductId))
            {
                //Նշված քարտի համար արդեն կատարվել է փոխարինման գործողություն:
                result.Errors.Add(new ActionError(1811, new string[] { "փոխարինման" }));
                return result;
            }

            if (IsAlreadyReplacedButNotGiven(Card.ProductId))
            {
                //Հնարավոր չէ փոխարինել։ Նշված քարտն արդեն փոխարինվել է և նոր քարտի կարգավիճակը «ՉՏՐ» է:
                result.Errors.Add(new ActionError(1809));
                return result;
            }

            if (!IsNormCardStatus(Card.CardNumber, Card.ProductId))
            {
                //Հնարավոր չէ կատարել գործողությունը։ Քարտի կարգավիճակը NORM չէ:
                result.Errors.Add(new ActionError(1810));
                return result;
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

            CardTariffContract cardTariff = new CardTariffContract();
            cardTariff.TariffID = RelatedOfficeNumber;
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

            if (Customer.IsCustomerUpdateExpired(customerNumber))
            {
                // Հաճախորդի տվյալները թարմացված չեն:
                result.Errors.Add(new ActionError(1664));
            }

            ulong cardHolderCustomerNumber = 0;
            if (customer is LegalCustomer legalcustomer)
            {
                if (!Regex.IsMatch(OrganisationNameEng, "^[A-Za-z- ./\\d]+$") || OrganisationNameEng.Contains('<') || OrganisationNameEng.Contains('>'))
                {
                    //«Հաստատության անվանումը» դաշտում առկա է անթույլատրելի նշան:
                    result.Errors.Add(new ActionError(1612));
                }

                cardHolderCustomerNumber = GetCardHolderCustomerNumber(Card.ProductId);
            }
            else
            {
                cardHolderCustomerNumber = customerNumber;
            }

            if (!NonCreditLineCardReplaceOrderDB.CheckCustomerDocument(cardHolderCustomerNumber))
            {
                // Հաճախորդի հիմնական փաստափուղթը լրացված չէ:
                result.Errors.Add(new ActionError(1588));
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
            if (!NonCreditLineCardReplaceOrderDB.CheckCardRelatedOffice(this))
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

            if (Card.Type == 16 || Card.Type == 1)
            {
                // Visa Electron քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1674, new string[] { "փոխարինել" }));
            }

            if (Card.Type == 14 || Card.Type == 32)
            {
                // Maestro տեսակի քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1675, new string[] { "փոխարինել" }));
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
                result.Errors.Add(new ActionError(1808, new string[] { "փոխարինել" }));
            }

            //Հաշվի կցման ստուգումներ

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
            if (!(NonCreditLineCardReplaceOrderDB.IfCardAccountExists(this)))
            {
                //Քարտային հաշիվը բացակայում է։
                result.Errors.Add(new ActionError(1672));
            }
            return result;

        }

        public ulong GetCardNewAppID()
        {
            return NonCreditLineCardReplaceOrderDB.GetCardNewAppID(this);
        }

        public void GetNonCreditLineCardReplaceOrder()
        {
            NonCreditLineCardReplaceOrderDB.GetNonCreditLineCardReplaceOrder(this);
        }

        public bool IsAlreadyReplacedButNotGiven(long productID)
        {
            return NonCreditLineCardReplaceOrderDB.IsAlreadyReplacedButNotGiven(productID);
        }


        public static bool IsNormCardStatus(string cardNumber, long productID)
        {
            return NonCreditLineCardReplaceOrderDB.IsNormCardStatus(cardNumber, productID);
        }

        public static ulong GetCardHolderCustomerNumber(long productID)
        {
            return NonCreditLineCardReplaceOrderDB.GetCardHolderCustomerNumber(productID);
        }

        public bool IsAlreadyReplaced(long productID)
        {
            return NonCreditLineCardReplaceOrderDB.IsAlreadyReplaced(productID);
        }

        public bool IsAlreadyExistReplaceOrder(long productID)
        {
            return NonCreditLineCardReplaceOrderDB.IsAlreadyExistReplaceOrder(productID);
        }
    }
}
