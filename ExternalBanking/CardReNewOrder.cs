using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.CreditLineActivatorARCA;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

namespace ExternalBanking
{
    public class CardRenewOrder : Order
    {
        /// <summary>
        /// Քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Ցույց է տալիս վերաթողարկումն արվում է քարտի այլ թե նույն տեսակով
        /// </summary>
        public bool RenewWithCardNewType { get; set; }

        /// <summary>
        /// Ցույց է տալիս վերաթողարկումն արվում է վ/գ-ի դադարեցումով թե երկարաձգումով
        /// </summary>
        public bool? WithCreditLineClosing { get; set; } = null;

        /// <summary>
        /// Ներգրավողի ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }

        /// <summary>
        /// Սպասարկողի ՊԿ
        /// </summary>
        public int ServingSetNumber { get; set; }

        /// <summary>
        /// Աշխ․ ծրագիր
        /// </summary>
        public int RelatedOfficeNumber { get; set; }

        /// <summary>
        /// Հաստատության անվանում
        /// </summary>
        public string OrganisationNameEng { get; set; }

        /// <summary>
        /// Քարտի ստացման եղանակ
        /// </summary>
        public int CardReceivingType { get; set; }

        /// <summary>
        /// Առաքման հասցե
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// SMS հեռախոսահամար
        /// </summary>
        public string CardSMSPhone { get; set; }

        /// <summary>
        /// PIN-ի ստացման եղանակ
        /// </summary>
        public int CardPINCodeReceivingType { get; set; }

        /// <summary>
        /// Նոր քարտի համար(միայն նոր տեսակով վերաթողարկման դեպքում)
        /// </summary>
        public string CardNewNumber { get; set; }

        /// <summary>
        /// Նոր քարտի տեսակ և արժույթ(միայն նոր տեսակով վերաթողարկման դեպքում)
        /// </summary>
        public string CardNewTypeAndCurrency { get; set; }

        /// <summary>
        ///Վարկային գծի ունիկալ համար
        /// </summary>
        public long CreditLineProductId { get; set; }

        private void Complete()
        {
            SubType = 1;

            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
            {
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            }              

            Type = OrderType.CardRenewOrder;
            OPPerson = SetOrderOPPerson(CustomerNumber);
        }

        /// <summary>
        /// "Վերաթողարկել քարտը" հայտի պահպանում և ուղարկում
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
                result = CardRenewOrderDB.Save(this, source, user);

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

            result = base.Confirm(user);

            if (result.ResultCode == ResultCode.Normal)
            {
                ulong cardNewAppID = GetCardNewAppID();
                PlasticCard newCard = new PlasticCard();
                newCard = PlasticCard.GetPlasticCard(cardNewAppID, true);
                CardRenewOrder order = new CardRenewOrder();

                try
                {
                    order = CardRenewOrderDB.GetCardRenewOrder(Id);
                }
                catch (Exception)
                {
                    result.ResultCode = ResultCode.DoneErrorInFurtherActions;
                    //Քարտի վերաթողարկման հայտը կատարված է։ Սխալի պատճառով հնարավոր չէ ցուցադրել տվյալ քարտի նոր համարը։
                    result.Errors.Add(new ActionError(1941));
                    return result;
                }

                if (newCard.CardNumber != "" && order.Quality == OrderQuality.Completed)
                {
                    result.ResultCode = ResultCode.DoneAndReturnedValues;
                    if (RenewWithCardNewType)
                    {
                        //Քարտի վերաթողարկման հայտը հաստատվել է: Գրանցվել է newCard.CardNumber համարով քարտ:
                        result.Errors.Add(new ActionError(1940, new string[] { newCard.CardNumber }));
                    }
                    else
                    {
                        //Քարտի վերաթողարկման հայտը հաստատվել է:
                        result.Errors.Add(new ActionError(1939, new string[] { newCard.CardNumber }));
                    }

                    //if (order.WithCreditLineClosing is null)
                    //{
                    //    if (RenewWithCardNewType)
                    //    {
                    //        //Քարտի վերաթողարկման հայտը հաստատվել է։ Գրանցվել է newCard.CardNumber համարով քարտ, որին կցվել են հին քարտի քարտային և գերածախսի հաշիվները:
                    //        result.Errors.Add(new ActionError(1936, new string[] { newCard.CardNumber }));
                    //    }
                    //    else
                    //    {
                    //        //Քարտի վերաթողարկման հայտը հաստատվել է։ Վերաթողարկված քարտին կցվել են հին քարտի քարտային և գերածախսի հաշիվները:
                    //        result.Errors.Add(new ActionError(1935));
                    //    }
                    //}
                    //else
                    //{
                    //    if (order.WithCreditLineClosing is true)
                    //    {
                    //        if (RenewWithCardNewType)
                    //        {
                    //            //Քարտի վերաթողարկման հայտը հաստատվել է: Գրանցվել է newCard.CardNumber համարով քարտ, որին կցվել են հին քարտի քարտային և գերածախսի հաշիվները, իսկ քարտին առկա վարկային գիծը դադարեցվել է:
                    //            result.Errors.Add(new ActionError(1940, new string[] { newCard.CardNumber }));
                    //        }
                    //        else
                    //        {
                    //            //Քարտի վերաթողարկման հայտը հաստատվել է։ Վերաթողարկված քարտին կցվել են հին քարտի քարտային և գերածախսի հաշիվները, իսկ քարտին առկա վարկային գիծը դադարեցվել է:
                    //            result.Errors.Add(new ActionError(1939));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (RenewWithCardNewType)
                    //        {
                    //            //Քարտի վերաթողարկման հայտը հաստատվել է: Գրանցվել է newCard.CardNumber համարով քարտ։ Վ/գ երկարաձգումից հետո անհրաժեշտ է իրականացնել հաշվի կցում <<Վերաթողարկված քարտի հաշվի կցում>> հայտով:
                    //            result.Errors.Add(new ActionError(1938, new string[] { newCard.CardNumber }));
                    //        }
                    //        else
                    //        {
                    //            //Քարտի վերաթողարկման հայտը հաստատվել է, քարտը վերաթողարկվել է: Վ/գ երկարաձգումից հետո անհրաժեշտ է իրականացնել հաշվի կցում <<Վերաթողարկված քարտի հաշվի կցում>> հայտով:
                    //            result.Errors.Add(new ActionError(1937));
                    //        }
                    //    }
                    //}
                }
                else if (newCard.CardNumber == "" && order.Quality == OrderQuality.TransactionLimitApprovement)
                {
                    result.ResultCode = ResultCode.SaveAndSendToConfirm;

                    //Հայտի մուտքագրումը կատարված է։ Հայտն ուղարկվել է ՓԼ/ԱՖ բաժնի հաստատման։
                    result.Errors.Add(new ActionError(1934));
                }
            }
            return result;
        }

        /// <summary>
        /// Վերաթողարկել քարտը հայտի ստուգումներ
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

            if (Card is null)
            {
                //Հնարավոր չէ կատարել նշված գործողությունը։ Վերաթողարկվող քարտը գործող չէ:
                result.Errors.Add(new ActionError(1925));
                return result;
            }

            int currentBankCode = user.filialCode;

            if (currentBankCode == 22059 && Card.FilialCode == 22000)
            {
                currentBankCode = Card.FilialCode;
            }
            if (Card.FilialCode != currentBankCode)
            {
                //Այլ մասնաճյուղի քարտ:
                result.Errors.Add(new ActionError(1515));
            }

            if (!IsNormCardStatus(Card.CardNumber, Card.ProductId))
            {
                //Հնարավոր չէ կատարել գործողությունը։ Քարտի կարգավիճակը NORM չէ:
                result.Errors.Add(new ActionError(1810));
            }

            if (Customer.IsCustomerUpdateExpired(customerNumber))
            {
                // Հաճախորդի տվյալները թարմացված չեն:
                result.Errors.Add(new ActionError(1664));
            }

            if (IsAlreadyRenewed(Card.ProductId))
            {
                //Նշված քարտի համար արդեն կատարվել է վերաթողարկման գործողություն:
                result.Errors.Add(new ActionError(1811, new string[] { "վերաթողարկման" }));
            }

            if (IsAlreadyExistRenewOrder(Card.ProductId))
            {
                // Առկա է չկատարված կարգավիճակով հայտ: Անհրաժեշտ է այն հաստատել կամ հեռացնել:
                result.Errors.Add(new ActionError(1896));
            }

            if (Card.Type == 16 || Card.Type == 1)
            {
                // Visa Electron քարտը հնարավոր չէ վերաթողարկել:
                result.Errors.Add(new ActionError(1674, new string[] { "վերաթողարկել" }));
            }

            if (Card.Type == 14 || Card.Type == 32)
            {
                // Maestro տեսակի քարտը հնարավոր չէ վերաթողարկել:
                result.Errors.Add(new ActionError(1675, new string[] { "վերաթողարկել" }));
            }

            if (Card.Type == 51)
            {
                // Virtual տեսակի քարտը հնարավոր չէ վերաթողարկել:
                result.Errors.Add(new ActionError(1808, new string[] { "վերաթողարկել" }));
            }

            string cardExpiryDate = Card.GetExDate(Card.CardNumber);

            if (!(Card.MainCardNumber is null || Card.MainCardNumber == ""))
            {
                long mainAppID = Card.GetCardProductId(Card.MainCardNumber, customerNumber);
                if (mainAppID != Card.ProductId && (Card.Type == 20 || Card.Type == 23 ||
                    (Card.Type == 21 && Card.CardNumber.Substring(0, 8) == "90513410")))
                {
                    //Հնարավոր չէ կատարել։
                    result.Errors.Add(new ActionError(50));
                }

                string mainCardNumber = Card.GetCardNumber(mainAppID);
                string mainCardExpiryDate = Card.GetExDate(mainCardNumber);
                if (mainAppID != Card.ProductId)
                {
                    if (!(mainCardNumber is null || mainCardNumber == ""))
                    {
                        if (mainCardNumber != Card.MainCardNumber)
                        {
                            //Տվյալ քարտի հիմնական քարտի տեսակը(համարը) փոխվել է (MainCardNumber->Cardnumber): Քարտի վերաթողակումը հնարավոր չէ:
                            result.Errors.Add(new ActionError(1927, new string[] { mainCardNumber + " >> " + Card.CardNumber }));
                        }
                    }
                    else
                    {
                        //Հիմանական քարտը գտնված չէ։
                        result.Errors.Add(new ActionError(899));
                    }
                    if (Convert.ToDateTime("01/" + mainCardExpiryDate.Substring(0, 2) + "/" + mainCardExpiryDate.Substring(2, 4)) <=
                        Convert.ToDateTime("01/" + cardExpiryDate.Substring(0, 2) + "/" + cardExpiryDate.Substring(2, 4)))
                    {
                        //Հիմնական քարտը վերաթողարկված չէ: Առաջնահերթ վերաթողարկեք հիմնական քարտը:
                        result.Errors.Add(new ActionError(1928));
                    }
                }
            }

            if (cardExpiryDate.Substring(0, 2) != DateTime.Now.Month.ToString() || cardExpiryDate.Substring(2, 4) != DateTime.Now.Year.ToString())
            {
                string expDatee = "01/" + cardExpiryDate.Substring(0, 2) + "/" + cardExpiryDate.Substring(2, 4);
                DateTime expDate = DateTime.ParseExact(expDatee, "dd/MM/yyyy", null);
                DateTime operDay = (DateTime)OperationDate;
                if (Math.Abs(expDate.Month - operDay.Month + 12 * (expDate.Year - operDay.Year)) > 3)
                {
                    if (user.userID != 98 && user.userID != 1781)
                    {
                        //Տվյալ քարտը չի կարող վերաթողարկվել:
                        result.Errors.Add(new ActionError(1929));
                    }                   
                }
            }

            ACBAServiceReference.Customer customer = Customer.GetCustomer(customerNumber);

            if (customer.quality.value == "43")
            {
                //Ընտրված է կրկնակի հաճախորդ: Հնարավոր չէ քարտը վերաթողարկել կրկնակի հաճախորդի համար:
                result.Errors.Add(new ActionError(1662, new string[] { "վերաթողարկել" }));
            }

            List<Card> supplementaryCards = new List<Card>();
            supplementaryCards.AddRange(CardDB.GetLinkedCards(Card.CardNumber));
            if (RenewWithCardNewType && supplementaryCards.Count > 0)
            {
                //Հնարավոր չէ իրականացնել գործողությունը, անհրաժեշտ է փակել կից քարտերը։
                result.Errors.Add(new ActionError(1930));
            }

            int productType = Utility.GetCardProductType(Convert.ToUInt32(Card.Type));
            if (!Validation.CheckProductAvailabilityByCustomerCountry(customerNumber, productType))
            {
                // Հաճախորդի տվյալ տիպի քարտը հնարավոր չէ վերաթողարկել քաղաքացիության, ռեզիդենտության կամ կենսական շահերի կենտրոն հանդիսացող տվյալ երկրի պատճառով։
                result.Errors.Add(new ActionError(1666, new string[] { "վերաթողարկել" }));
            }

            ulong cardHolderCustomerNumber;
            if (customer is LegalCustomer)
            {
                if (!Regex.IsMatch(OrganisationNameEng, "^[A-Za-z- ./\\d]+$") || OrganisationNameEng.Contains('<') || OrganisationNameEng.Contains('>'))
                {
                    //«Հաստատության անվանումը» դաշտում առկա է անթույլատրելի նշան:
                    result.Errors.Add(new ActionError(1612));
                }
                cardHolderCustomerNumber = Card.GetCardHolderCustomerNumber(Card.ProductId);
            }
            else
            {
                cardHolderCustomerNumber = customerNumber;
            }
            string cardHolderFullName = Card.GetCardHolderData((ulong)Card.ProductId, "fullName");
            if (cardHolderFullName.Length > 31)
            {
                // Անուն/Ազգանվան նիշերը գերազանցում են թույլատրելի քանակը:
                result.Errors.Add(new ActionError(1969));
            }

            if (!CardRenewOrderDB.CheckCustomerDocument(cardHolderCustomerNumber, 1))
            {
                // Հաճախորդի հիմնական փաստափուղթը լրացված չէ:
                result.Errors.Add(new ActionError(1588));
            }

            string motherName = CardDB.GetCardMotherName((ulong)Card.ProductId);
            if (motherName.Equals(""))
            {
                if (!CardRenewOrderDB.CheckCustomerDocument(cardHolderCustomerNumber, 2))
                {
                    // Գաղտնաբառ դաշտը բացակայում է:
                    result.Errors.Add(new ActionError(1947));
                }
            }

            if (Card.RelatedOfficeNumber == 1692)
            {
                //Տվյալ աշխ. ծրագրով պատվիրված քարտերի վերաթողարկումը հնարավոր չէ:
                result.Errors.Add(new ActionError(1926));
            }

            CardTariffContract contract = new CardTariffContract
            {
                TariffID = RelatedOfficeNumber
            };
            CardTariffContractDB.GetCardTariffContract(contract);

            if (contract.Quality == 0)
            //Եթե վերաթողարկվող քարտի աշխ ծրագիրը փակված է, նշանակում է քարտը պետք է վերաթողարկվի 174 աշխ ծրագրով։
            {
                RelatedOfficeNumber = 174;
            }
            int quality = GetRelatedOfficeQuality(Card.ProductId, RelatedOfficeNumber, RenewWithCardNewType);
            if (quality == -1)
            {
                //Տվյալ տեսակի և արժույթի վճարային քարտ աշխատավարձային ծրագրի տակ գրանցված չէ:
                result.Errors.Add(new ActionError(1943));
            }
            else
            {
                if (quality == 0)
                {
                    //Տվյալ տեսակի և արժույթի վճարային քարտի աշխատավարձային ծրագրի կարգավիճակը «<<Գործող>> չէ:
                    result.Errors.Add(new ActionError(1942));
                }
            }

            //CreditLine creditLineOverdraft = CreditLine.GetCardOverdraft(Card.CardNumber);

            //if (!(WithCreditLineClosing is false))
            //{
            //    if (!(creditLineOverdraft is null) && Math.Abs(creditLineOverdraft.CurrentCapital) + Math.Abs(creditLineOverdraft.OutCapital) + Math.Abs(creditLineOverdraft.CurrentRateValue) != 0)
            //    {
            //        //Քարտային հաշվին առկա է գերածախս: Քարտի վերաթողարկման համար անհրաժեշտ է մարել գերածախսը: 
            //        result.Errors.Add(new ActionError(1931, new string[] { "Քարտի վերաթողարկման " }));
            //    }
            //}

            if (!RenewWithCardNewType)
            {
                ActionResult arCaResult = CheckArcaStatus();
                if (arCaResult.Errors.Count > 0)
                {
                    foreach (ActionError error in arCaResult.Errors)
                    {
                        result.Errors.Add(error);
                    }
                }
            }

            if (WithCreditLineClosing is true)
            {
                CreditLine creditLine = CreditLine.GetCardCreditLine(Card.CardNumber);

                if (!ChangeExceedLimitRequest.ChekArCaBalance((ulong)creditLine.ProductId))
                {
                    //Քարտի վերաթողարկումը մերժված է: Հնարավոր չէ դադարեցնել վարկային գիծը:
                    result.Errors.Add(new ActionError(1932));
                }
            }

            if (CardPINCodeReceivingType == 2 && (CardSMSPhone == "" || CardSMSPhone is null))
            {
                //Բջջային հեռախոսահամարը մուտքագրված չէ:
                result.Errors.Add(new ActionError(464));
            }

            if (CardRenewOrderDB.IsAlreadyRenewedOrReplaced(Card.ProductId))
            {
                //Նշված քարտի համար արդեն կատարվել է վերաթողարկման/փոխարինման գործողություն:
                result.Errors.Add(new ActionError(1811, new string[] { "վերաթողարկման/փոխարինման" }));
            }

            return result;
        }


        public static List<string> CheckRenew(CardRenewOrder order)
        {
            List<string> messages = new List<string>();

            CardTariffContract contract = new CardTariffContract
            {
                TariffID = order.RelatedOfficeNumber
            };
            CardTariffContractDB.GetCardTariffContract(contract);
            int quality = GetRelatedOfficeQuality(order.Card.ProductId, order.RelatedOfficeNumber, order.RenewWithCardNewType);
            if (order.Card.RelatedOfficeNumber != 1692 || quality != -1 || quality != 0)
            {
                if (contract.Quality == 0)
                {
                    messages.Add("Քարտը վերաթողարկվելու է N174 քարտային ծրագրով(փակված աշխ. ծրագրերով քարտերը վերաթողարկվում են N174 քարտային ծրագրով)։");
                }
            }

            if (CardRenewOrderDB.IsAlreadyNotRenewed(order.Card.ProductId))
            {
                messages.Add("Քարտի համար արդեն սեղմվել է «Չվերաթողարկել»:");
            }
            return messages;
        }

        /// <summary>
        /// Ստուգում է վերաթողարկվող քարտի կարգավիճակը Արմենիան Քարդ ՊԿ-ում
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckArcaStatus()
        {
            ActionResult result = new ActionResult
            {
                Errors = new List<ActionError>()
            };

            CardIdentification cardIdentification = new CardIdentification
            {
                CardNumber = Card.CardNumber,
                ExpiryDate = Card.ValidationDate.ToString("yyyyMM")
            };

            try
            {
                int status = ArcaDataService.GetCardArCaStatus(cardIdentification);
                if (status == 2)
                {
                    // Քարտը բլոկավորված է։ Հնարավոր չէ իրականացնել վերաթողարկում։ Խնդրում ենք անհրաժեշտության դեպքում ապաբլոկավորել քարտը և կրկին հայտ ուղարկել։
                    result.Errors.Add(new ActionError(1661, new string[] { "վերաթողարկում" }));
                    return result;
                }
                else if (status == 3)
                {
                    // Կապի խնդիր։ Խնդրում ենք կրկին փորձել մի փոքր ուշ
                    result.Errors.Add(new ActionError(1660));
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        public static CardRenewOrder GetCardRenewOrder(long Id)
        {
            return CardRenewOrderDB.GetCardRenewOrder(Id);
        }

        public static bool IsNormCardStatus(string cardNumber, long productID)
        {
            return CardRenewOrderDB.IsNormCardStatus(cardNumber, productID);
        }

        public bool IsAlreadyRenewed(long productID)
        {
            return CardRenewOrderDB.IsAlreadyRenewed(productID);
        }

        public bool IsAlreadyExistRenewOrder(long productID)
        {
            return CardRenewOrderDB.IsAlreadyExistRenewOrder(productID);
        }

        public ulong GetCardNewAppID()
        {
            return CardRenewOrderDB.GetCardNewAppID(this);
        }
        public static int GetRelatedOfficeQuality(long productId, int officeID, bool withNewType)
        {
            return CardRenewOrderDB.GetRelatedOfficeQuality(productId, officeID, withNewType);
        }

        public static string GetPhoneForCardRenew(long productId)
        {
            ulong cardHolder;
            cardHolder = Card.GetCardHolderCustomerNumber(productId);
            if (cardHolder == 0)
            {
                string cardNumber = Card.GetCardNumber(productId);
                ulong customerNumber = Card.GetCardCustomerNumber(cardNumber);
                cardHolder = customerNumber;
            }
            return CardRenewOrderDB.GetPhoneForCardRenew(cardHolder);
        }
    }
}
