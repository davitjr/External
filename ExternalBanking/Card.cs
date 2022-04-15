using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public enum CardChangeType : short
    {
        /// <summary>
        /// Նոր քարտ
        /// </summary>
        New = 0,

        /// <summary>
        /// Վերաթողարկում նույն տեսակով
        /// </summary>
        RenewWithSameType = 1,

        /// <summary>
        /// Վերաթողարկում այլ տեսակով
        /// </summary>
        RenewWithNewType = 2,

        /// <summary>
        /// Փոխարինում՝ նոր համար, նոր ժամկետ - առանց վ.գ.
        /// </summary>
        NonCreditLineCardReplace = 3,

        /// <summary>
        /// Փոխարինում՝ նոր համար, նոր ժամկետ - վ.գ.
        /// </summary>
        CreditLineCardReplace = 4,

        /// <summary>
        /// Տեղափոխում
        /// </summary>
        Move = 5
    }

    public class Card
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Քարտային հաշիվ
        /// </summary>
        public Account CardAccount { get; set; }

        /// <summary>
        /// Քարտի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Քարտի մնացորդ
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// Քարտի տեսակ (նկարագրություն)
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Քարտային համակարգ
        /// </summary>
        public int CardSystem { get; set; }

        /// <summary>
        /// Հիմնական քարտի համար
        /// </summary>
        public string MainCardNumber { get; set; }

        /// <summary>
        /// Քարտի վերջ
        /// </summary>
        public DateTime ValidationDate { get; set; }

        /// <summary>
        /// Փակման ամսաթիվ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Նշումներ
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Օվերդրաֆտի հաշվեհամար
        /// </summary>
        public Account OverdraftAccount { get; set; }

        /// <summary>
        /// Ցույց է տալիս SMS ծառայությունը ակտիվացված է թե ոչ
        /// </summary>
        public bool SMSApplicationPresent { get; set; }

        /// <summary>
        /// Էֆեկտիվ տոկոսադրույք
        /// </summary>
        public float InterestRateEffective { get; set; }

        /// <summary>
        /// Ցպահանջ ավանդի տարեկան տոկոսադրույք
        /// </summary>
        public float PositiveInterestRate { get; set; }

        /// <summary>
        /// Ցպահանջ ավանդի կուտակված տոկոսագումար
        /// </summary>
        public float PositiveRate { get; set; }

        /// <summary>
        /// Ընդամենը չվճարված և վճարված ցպահանջ ավանդ
        /// </summary>
        public float TotalPositiveRate { get; set; }

        /// <summary>
        /// Ցպահանջ ավանդի տոկոսի հաշվարկի օրը
        /// </summary>
        public DateTime? LastDayOfPositiveRateCalculation { get; set; }

        /// <summary>
        /// Ցպահանջ ավանդի վերջին վճարման օրը
        /// </summary>
        public DateTime? LastDayOfPositiveRateRepair { get; set; }

        /// <summary>
        /// Ցպահանջ ավանդի հաշվարկի դադարեցման օրը
        /// </summary>
        public DateTime? PositiveRateStoppingDay { get; set; }

        /// <summary>
        /// Քարտի վարկային գիծ
        /// </summary>
        public CreditLine CreditLine { get; set; }

        /// <summary>
        /// Քարտի գերածախս
        /// </summary>
        public CreditLine Overdraft { get; set; }

        /// <summary>
        /// Կանխիկացման միջնորդավճար սեփական կետերում 
        /// </summary>
        public double FeeForCashTransaction { get; set; }

        /// <summary>
        /// Քարտի մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// Աշխատանքային ծրագրի համար
        /// </summary>
        public ushort RelatedOfficeNumber { get; set; }

        /// <summary>
        /// Աշխատանքային ծրագրի անվանում
        /// </summary>
        public string RelatedOfficeName { get; set; }

        /// <summary>
        /// Քարտի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Քարտի բացման ա/թ
        /// </summary>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Փակողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }

        /// <summary>
        /// Փակման պատճառի տեսակ
        /// </summary>
        public ushort ClosingReasonType { get; set; }

        /// <summary>
        /// Փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonTypeDescription { get; set; }

        /// <summary>
        /// Լրացուցիչ տվյալներ
        /// </summary>
        public string AddInf { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string CreditCode { get; set; }

        ///// <summary>
        ///// Քարտի ավարտ
        ///// </summary>
        //public string ExpiryDate { get; set; }

        /// <summary>
        /// Քարտի Տեսակ (հիմնական/կից/լրացուցիչ)
        /// </summary>
        public SupplementaryType SupplementaryType { get; set; }

        /// <summary>
        /// Կից / լրացուցից քարտապանի անուն ազգանուն
        /// </summary>
        public string SupplementaryCustomerName { get; set; }

        /// <summary>
        /// Քարտի ստացման եղանակ
        /// </summary>
        public int CardReceivingType { get; set; }

        /// <summary>
        /// Դիմումի ընդունման տարբերակ
        /// </summary>
        public int CardApplicationAcceptanceType { get; set; }

        public CardChangeType CardChangeType { get; set; }

        /// <summary>
        /// Քարտը կից է թէ ոչ
        /// </summary>
        public bool IsSupplementary
        {
            get
            {
                if (String.IsNullOrEmpty(MainCardNumber))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// OTB
        /// </summary>
        public double? ArCaBalance { get; set; }

        /// <summary>
        /// Պարտք
        /// </summary>
        public double Debt { get; set; }

        /// <summary>
        /// Քարտի ստուգման արժեք (CVV)
        /// </summary>
        public string RealCVV { get; set; }

        /// <summary>
        /// Կցված քարտի բանկի անվանում
        /// </summary>
        public string AttachedCardBankName { get; set; }
        /// <summary>
        /// Կցված քարտի ID
        /// </summary>
        public int AttachedCardId { get; set; }

        /// <summary>
        /// Digital քարտի դիզայնի համար
        /// </summary>
        public int DesignID { get; set; }

        /// <summary>
        /// Digital քարտի դիզայնի նկար
        /// </summary>
        public string CardDesignImage { get; set; }


        public Card()
        {
            CardAccount = new Account();
            CreditLine = new CreditLine();
            Overdraft = new CreditLine();
        }


        public static Card GetCard(ulong productId, ulong customerNumber)
        {
            return CardDB.GetCard(productId, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի տեսակը քարտի համարը և քարտի տեսակի նկարագրությունը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Card GetCardMainData(ulong productId, ulong customerNumber)
        {
            return CardDB.GetCardMainData(productId, customerNumber);
        }

        public static List<Card> GetCards(ulong customerNumber, ProductQualityFilter filter, bool includingAttachedCards = true)
        {
            List<Card> cards = new List<Card>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                cards.AddRange(CardDB.GetCards(customerNumber));
            }
            else if (filter == ProductQualityFilter.Closed)
            {
                cards.AddRange(CardDB.GetClosedCards(customerNumber));
            }
            else if (filter == ProductQualityFilter.All)
            {
                cards.AddRange(CardDB.GetCards(customerNumber));
                cards.AddRange(CardDB.GetClosedCards(customerNumber));
            }
            else if (filter == ProductQualityFilter.OpenedAndNotActive)
            {
                cards.AddRange(CardDB.GetCards(customerNumber));
                cards.AddRange(Card.GetNotActivatedVirtualCards(customerNumber));
            }
            if (includingAttachedCards && (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet ||
                filter == ProductQualityFilter.All || filter == ProductQualityFilter.OpenedAndNotActive))
            {
                AddOtherBankAttachedCards(cards, customerNumber);
            }
            return cards;
        }

        public static List<Card> GetCardsAsync(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Card> cards = new List<Card>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                cards.AddRange(CardDB.GetCardsAsync(customerNumber).Result);
                cards.AddRange(CardDB.GetOtherBankAttachedCardsAsync(customerNumber).Result);
            }
            if (filter == ProductQualityFilter.Closed)
            {
                cards.AddRange(CardDB.GetClosedCards(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                cards.AddRange(CardDB.GetCards(customerNumber));
                cards.AddRange(CardDB.GetOtherBankAttachedCards(customerNumber));
                cards.AddRange(CardDB.GetClosedCards(customerNumber));
            }
            return cards;

        }

        /// <summary>
        /// Վերադարձնում է քարտի մնացորդը ԱՐՔԱ-ում
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<String, double> GetArCaBalance(int setNumber, string clientIp = "")
        {
            KeyValuePair<string, double> arcaBalance = new KeyValuePair<string, double>();
            try
            {

                ArcaBalanceResponseData arcaResponce = new ArcaBalanceResponseData();
                arcaResponce = GetArCaBalanceResponseData(setNumber, clientIp);
                arcaBalance = new KeyValuePair<string, double>(key: arcaResponce.ResponseCode, value: arcaResponce.Amount);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return arcaBalance;
        }

        /// <summary>
        /// Վերադարձնում է քարտի մնացորդը ԱՐՔԱ-ում
        /// </summary>
        /// <returns></returns>
        public ArcaBalanceResponseData GetArCaBalanceResponseData(int setNumber, string clientIp = "")
        {
            ArcaBalanceResponseData arcaBalance = new ArcaBalanceResponseData();

            try
            {

                UtilityDB.InsertActionLog("GetCardBalance", this.CardNumber, setNumber, clientIp);

                bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());

                if (!isTestVersion)
                {
                    ArcaDataServiceReference.ArcaBalanceResponseData arcaResponseData = new ArcaDataServiceReference.ArcaBalanceResponseData();

                    arcaResponseData = ArcaDataService.GetBalance(this.CardNumber);

                    double balance = 0;
                    if (double.TryParse(arcaResponseData.Amount, out balance))
                    {
                        arcaBalance.Amount = balance;
                    }
                    arcaBalance.Currency = arcaResponseData.Currency;

                    double availableExceedLimit = 0;
                    if (double.TryParse(arcaResponseData.AvailableExceedLimit, out availableExceedLimit))
                    {
                        arcaBalance.AvailableExceedLimit = availableExceedLimit;
                    }
                    arcaBalance.AvailableExceedLimitCurrency = arcaResponseData.AvailableExceedLimitCurrency;

                    double ownFunds = 0;
                    if (double.TryParse(arcaResponseData.OwnFunds, out ownFunds))
                    {
                        arcaBalance.OwnFunds = ownFunds;
                    }
                    arcaBalance.OwnFundsCurrency = arcaResponseData.OwnFundsCurrency;

                    arcaBalance.ResponseCode = arcaResponseData.ResponseCode;
                    arcaBalance.ResponseDescription = arcaResponseData.ResponseDescription;

                }
                else
                {
                    arcaBalance.ResponseCode = "00";
                    arcaBalance.Amount = 1500000;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return arcaBalance;
        }

        /// <summary>
        /// Վերադարձնում է քարտի քաղվածքը նշաված ժամանակահատվածում և նշված լեզվով
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static CardStatement GetStatement(string cardNumber, DateTime dateFrom, DateTime dateTo, byte language, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            CardStatement cardStatement = CardDB.GetStatement(cardNumber, dateFrom, dateTo, language, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc);

            return cardStatement;
        }

        /// <summary>
        /// Վերադարձնում է քարտը , որին կցված է նշված հաշիվը
        /// </summary>
        /// <param name="account">հաշվեհամար</param>
        /// <returns></returns>
        public static Card GetCard(Account account)
        {
            return CardDB.GetCard(account);
        }
        /// <summary>
        /// AMEX_MR ի սպասարկման գումար
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static double GetMRFee(string cardNumber)
        {
            return CardDB.GetMRFee(cardNumber);
        }

        /// <summary>
        /// Քարտի սասարկման գումար
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static double GetCardTotalDebt(string cardNumber)
        {
            return CardDB.GetCardTotalDebt(cardNumber);
        }

        /// <summary>
        /// Քարտի սասարկման գումար
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static CardServiceFee GetCardServiceFee(ulong productId)
        {
            CardServiceFee serviceFee = CardDB.GetCardServiceFee(productId);
            return serviceFee;
        }
        /// <summary>
        /// Քարտի պետ տուրք
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static double GetPetTurk(long productId)
        {
            return CardDB.GetPetTurk(productId);
        }

        /// <summary>
        /// Վերադարձնում է կանխիկացման միջնորդավճարը
        /// </summary>
        /// <param name="amount">Կանխիկացվող գումար</param>
        /// <returns></returns>
        public Double WithdrawalFee(double amount, SourceType source)
        {
            return Utility.RoundAmount((double)(decimal)(amount * FeeForCashTransaction), Currency, source);
        }
        /// <summary>
        /// Վերադարձնում է քարտը
        /// </summary>
        /// <param name="cardNumber">Քարտի համար</param>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <returns></returns>
        public static Card GetCard(string cardNumber, ulong customerNumber)
        {
            return CardDB.GetCard(cardNumber, customerNumber);
        }

        /// <summary>
        /// Ստուգում ենք քարտի պատկանելությունը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static bool CheckCardOwner(string cardNumber, ulong customerNumber)
        {
            return CardDB.CheckCardOwner(cardNumber, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ քարտին կցված քարտերը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static List<Card> GetLinkedCards(string cardNumber)
        {
            return CardDB.GetLinkedCards(cardNumber);
        }

        public static double GetCashBackAmount(ulong productId)
        {
            Account cashBackAccount = Account.GetProductAccount(productId, 11, 70);
            double cashBackAmount = Account.GetAccountBalance(cashBackAccount.AccountNumber);
            return cashBackAmount;
        }

        public static Account GetCashBackAccount(ulong productId)
        {
            Account cashBackAccount = Account.GetProductAccount(productId, 11, 70);
            return cashBackAccount;
        }

        /// <summary>
        /// Վերադարձնում է քարտը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static Card GetCard(string cardNumber)
        {
            return CardDB.GetCard(cardNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի հիմնական տվյալները
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static Card GetCardMainData(string cardNumber)
        {
            return CardDB.GetCardMainData(cardNumber);
        }

        public static int GetCardType(string cardNumber, ulong customerNumber)
        {
            return CardDB.GetCardType(cardNumber, customerNumber);
        }

        public static bool IsOurCard(string cardNumber)
        {
            return CardDB.IsOurCard(cardNumber);
        }

        /// <summary>
        ///  Վերադարձնում է քարտի սպասարկման վարձի գրաֆիկը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="openDate"></param>
        /// <returns></returns>
        public static List<CardServiceFeeGrafik> GetCardServiceFeeGrafik(string cardNumber, DateTime openDate)
        {
            //cardNumber = "375127100000783";
            return CardDB.GetCardServiceFeeGrafik(cardNumber, openDate);
        }

        /// <summary>
        /// Վերադարձնում է քարտի սակագինը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static CardTariff GetCardTariff(ulong productId, ulong customerNumber)
        {
            return CardDB.GetCardTariff(productId, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում Է քարտի կարգավիճակը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <returns></returns>
        public static CardStatus GetCardStatus(ulong productId, ulong customerNumber)
        {
            return CardDB.GetCardStatus(productId, customerNumber);
        }

        //Այլ տեսակի քարտով վերաթողարկված քարտի ստացման հաստատում տպելիս քարտի վերաթողարկման, նույն տեսակով վերաթողարկման առկայության ստուգում
        public static ActionResult ValidateRenewedOtherTypeCardApplicationForPrint(string cardNumber)
        {
            ActionResult result = new ActionResult();

            if (!CardDB.IsCardChanged(cardNumber))
            {
                result.Errors.Add(new ActionError(741));
            }
            if (CardDB.IsSameTypeCardChange(cardNumber))
            {
                result.Errors.Add(new ActionError(742));
            }
            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;

            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            Culture Culture = new Culture((Languages)1);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի գարգավիճակը NORM է թե ոչ
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static bool IsNormCardStatus(string cardNumber)
        {
            return CardDB.IsNormCardStatus(cardNumber);
        }

        /// <summary>
        /// Քարտը գրանցված է թե ոչ
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static bool IsCardRegistered(string cardNumber)
        {
            return CardDB.IsCardRegistered(cardNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի տեսակը քարտի համարը և քարտի տեսակի նկարագրությունը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static Card GetCardWithOutBallance(string accountNumber)
        {
            return CardDB.GetCardWithOutBallance(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի տեսակը քարտի համարը և քարտի տեսակի նկարագրությունը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Card GetCardWithOutBallance(ulong productId)
        {
            return CardDB.GetCardWithOutBallance(productId);
        }


        /// <summary>
        ///  Քարտի սպասարկման վարձի նոր գրաֆիկի մուտքագրում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="openDate"></param>
        /// <returns></returns>
        public static List<CardServiceFeeGrafik> SetNewCardServiceFeeGrafik(ulong productId, ulong customerNumber)
        {
            List<CardServiceFeeGrafik> cardServiceFeeGrafiklist = null;
            Card card = GetCard(productId, customerNumber);
            if (card == null || card.ClosingDate != null)
            {
                return cardServiceFeeGrafiklist;
            }
            cardServiceFeeGrafiklist = new List<CardServiceFeeGrafik>();
            CardServiceFee cardServiceFee = GetCardServiceFee((ulong)card.ProductId);
            DateTime? nextDayOfServiceFeeLast = cardServiceFee.NextDayOfServiceFeePayment;
            DateTime? nextDayOfServiceFee = cardServiceFee.NextDayOfServiceFeePayment;

            if (cardServiceFee.FirstCharge == "1")
            {
                nextDayOfServiceFeeLast = card.OpenDate.AddMonths(1);
                nextDayOfServiceFeeLast = new DateTime(nextDayOfServiceFeeLast.Value.Year, nextDayOfServiceFeeLast.Value.Month, 1);
            }
            if (cardServiceFee.Period != 0)
            {

                while (nextDayOfServiceFee.Value.Date <= card.ValidationDate.Date)
                {
                    CardServiceFeeGrafik grafik = new CardServiceFeeGrafik();

                    grafik.PeriodStart = nextDayOfServiceFee.Value;


                    nextDayOfServiceFee = nextDayOfServiceFeeLast.Value.AddMonths(cardServiceFee.Period);
                    nextDayOfServiceFee = new DateTime(nextDayOfServiceFee.Value.Year, nextDayOfServiceFee.Value.Month, 1);
                    nextDayOfServiceFeeLast = nextDayOfServiceFee;
                    if (nextDayOfServiceFee.Value.Date > card.ValidationDate.Date)
                    {
                        nextDayOfServiceFee = card.EndDate;
                    }

                    grafik.PeriodEnd = nextDayOfServiceFee.Value;

                    grafik.Currency = "AMD";
                    cardServiceFeeGrafiklist.Add(grafik);
                }
            }
            return cardServiceFeeGrafiklist;
        }

        /// <summary>
        /// վերադարձնում է քարտի հին app_id-ն
        /// </summary>
        /// <param name="productId">հին app_id</param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static ulong GetCardOldProductId(long productId, int changeType)
        {
            return CardDB.GetCardOldProductId(productId, changeType);
        }

        /// <summary>
        /// Վերադարձնում է քարտի վրա եղած արգելանքի գումարը և հաշիվը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static DAHKDetail GetCardDAHKDetails(string cardNumber)
        {
            return CardDB.GetCardDAHKDetails(cardNumber);
        }


        public static string GetCardMotherName(ulong productId)
        {
            string motherName = CardDB.GetCardMotherName(productId);
            return motherName;
        }

        /// <summary>
        /// Վերադարձնում է քարտի քաղվածքի ստացման եղանակը
        /// </summary>
        /// <returns></returns>
        public short GetCardStatementReceivingType()
        {
            return CardDB.GetCardStatementReceivingType(this.CardNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտին կցված քարտերը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<Card> GetAttachedCards(ulong productId, ulong customerNumber)
        {
            return CardDB.GetAttachedCards(productId, customerNumber);
        }


        public static int GetStatementFixedReceivingType(string cardnumber)
        {

            short cardStatementCommunicationType = CardDB.GetCardStatementReceivingType(cardnumber);
            if (cardStatementCommunicationType == 1)
            {
                cardStatementCommunicationType = 3;
            }
            else if (cardStatementCommunicationType == 3)
            {
                cardStatementCommunicationType = 1;
            }
            else if (cardStatementCommunicationType == 4)
            {
                cardStatementCommunicationType = 2;
            }
            return cardStatementCommunicationType;
        }

        /// <summary>
        /// Վերադարձնում է քարտի USSD ծառայության կարգավիճակը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static CardServiceQualities GetCardUSSDService(ulong productID)
        {
            return CardDB.GetCardUSSDService(productID);
        }

        /// <summary>
        /// Վերադարձնում է քարտի 3DSecure ծառայությունը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static Card3DSecureService GetCard3DSecureService(ulong productID)
        {
            return CardDB.GetCard3DSecureService(productID);
        }
        /// <summary>
        /// Քարտի թոքենի փոփոխություն(քարտի տվյալների թարմացում արտաքին աշխարհում),կանչվում է քարտի վերաթողարկման և փոխարինման ժամանակ
        /// </summary>
        /// <param name="productId"></param>
        public static void SaveVirtualCardRequest(ulong productId, VirtualCardRequestTypes type, int userId, short statusChangeAction = 0)
        {
            CardDB.SaveVirtualCardUpdateRequest(productId, type, statusChangeAction, userId);
        }

        public static bool HasVirtualCard(ulong productId)
        {
            return CardDB.HasVirtualCard(productId);
        }

        public static ActionResult ReSendVirtualCardrequest(int requestId, int userId)
        {
            ActionResult result = new ActionResult();
            result = CardDB.ReSendVirtualCardRequest(requestId, userId);

            return result;
        }


        /// <summary>
        /// Վերադարձնում է քարտի SMS ծառայության կարգավիճակը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static CardServiceQualities GetPlasticCardSMSService(string cardNumber)
        {
            return CardDB.GetPlasticCardSMSService(cardNumber);
        }
        /// <summary>
        /// Վերադարձնում է քարտի վրա գրված հաճախորդի անունը և ազգանունը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static string GetEmbossingName(string cardNumber, ulong productId = 0)
        {
            return CardDB.GetEmbossingName(cardNumber, productId);
        }
        /// <summary>
        /// Վերադարձնում է կցված քարտի վրա գրված հաճախորդի անունը և ազգանունը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static string GetAttachedCardEmbossingName(string cardNumber)
        {
            return CardDB.GetAttachedCardEmbossingName(cardNumber);
        }
        public static double GetCardToCardTransferFee(string debitCardNumber, string creditCardNumber, double amount, string currency)
        {
            double fee = CardDB.GetCardToCardTransferFee(debitCardNumber, amount);
            return Utility.RoundAmount((fee), currency);
        }

        public static int GetCardServicingFilialCode(ulong productID)
        {
            return CardDB.GetCardServicingFilialCode(productID);
        }

        public static ulong GetCardCustomerNumber(string cardNumber)
        {
            return CardDB.GetCardCustomerNumber(cardNumber);
        }

        public static List<Card> GetLinkedCards(ulong customerNumber)
        {
            return CardDB.GetLinkedCards(customerNumber);
        }

        public static List<Card> GetLinkedAndAttachedCards(ulong productId, ProductQualityFilter productFilter = ProductQualityFilter.Opened)
        {
            return CardDB.GetLinkedAndAttachedCards(productId, productFilter);
        }

        public static CardAdditionalInfo GetCardAdditionalInfo(ulong productId, Languages language)
        {
            return CardDB.GetCardAdditionalInfo(productId, language);
        }

        /// <summary>
        /// Պահպանում է հաճախորդի կողմից մուտքագրված CVV-ի վերաբերյալ նշումը
        /// </summary>
        /// <param name="productId">Քարտի ունկալ համար</param>
        /// <param name="CVVNote">Հաճախորդի կողմից մուտքագրված CVV</param>
        /// <returns></returns>
        public static ActionResult SaveCVVNote(ulong productId, string CVVNote)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardDB.SaveCVVNote(productId, CVVNote);

                scope.Complete();
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի կողմից մուտքագրված CVV-ի վերաբերյալ նշումը
        /// </summary>
        /// <param name="productId">Քարտի ունկալ համար</param>
        /// <returns></returns>
        public static string GetCVVNote(ulong productId)
        {
            return CardDB.GetCVVNote(productId);
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի հայտի համար հասանելի քարտերի ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public static List<Card> GetCardsForNewCreditLine(ulong customerNumber, OrderType orderType)
        {
            return CardDB.GetCardsForNewCreditLine(customerNumber, orderType);
        }

        public static long GetCardProductId(string cardNumber, ulong customerNumber)
        {
            return CardDB.GetCardProductId(cardNumber, customerNumber);
        }

        public static Card SetSupplementaryCard(Card card)
        {
            return CardDB.SetSupplementaryCard(card);
        }
        public static List<Card> GetNotActivatedVirtualCards(ulong customerNumber)
        {
            return CardDB.GetNotActivatedVirtualCards(customerNumber);
        }

        public static ulong GetCardProductIdByAccountNumber(string cardAccountNumber, ulong customerNumber)
        {
            return CardDB.GetCardProductIdByAccountNumber(cardAccountNumber, customerNumber);
        }
        public static List<Card> GetClosedCardsForDigitalBanking(ulong customerNumber)
        {
            List<Card> cards = CardDB.GetClosedCardsForDigitalBanking(customerNumber);
            cards = cards.OrderByDescending(c => c.ClosingDate).ToList();
            List<Card> filteredCards = new List<Card>();
            for (int i = 0; i < cards.Count; i++)
            {
                if (filteredCards.Count == 0)
                {
                    filteredCards.Add(cards[i]);
                }
                else
                {
                    int c = 0;
                    foreach (var item in filteredCards)
                    {
                        if (item.CardAccount.AccountNumber == cards[i].CardAccount.AccountNumber)
                        {
                            c++;
                        }
                    }
                    if (c > 0)
                        c = 0;
                    else
                    {
                        filteredCards.Add(cards[i]);
                        c = 0;
                    }
                }
            }
            return filteredCards;
        }

        public static int GetCardSystem(string cardNumber)
        {
            return CardDB.GetCardSystem(cardNumber);
        }

        public static string GetCardNumber(long productId)
        {
            return CardDB.GetCardNumber(productId);
        }
        /// <summary>
        /// Վերադարձնում է քարտի Expire date-ը
        /// </summary>
        public static string GetExDate(string cardNumber)
        {
            return CardDB.GetExDate(cardNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի համար
        /// </summary>
        public static string GetCardNumberWithCreditLineAppId(ulong appId, CredilLineActivatorType reason)
        {
            return CardDB.GetCardNumberWithCreditLineAppId(appId, reason);
        }
        /// <summary>
        /// Վերադարձնում է քարտի ունիկալ համար
        /// </summary>
        public static ulong GetCardAppId(string cardNumber)
        {
            return CardDB.GetCardAppId(cardNumber);
        }
        public static string GetCardExpiryDateForArca(string cardNumber)
        {
            return CardDB.GetCardExpiryDateForArca(cardNumber);
        }

        public static bool CheckCardIsClosed(string cardNumber)
        {
            return CardDB.CheckCardIsClosed(cardNumber);
        }

        public static int GetCardArCaStatus(ulong productId, ulong customerNumber)
        {
            Card card = GetCardMainData(productId, customerNumber);
            CardIdentification cardIdentification = new CardIdentification();
            cardIdentification.CardNumber = card.CardNumber;
            cardIdentification.ExpiryDate = card.ValidationDate.ToString("yyyyMM");

            return ArcaDataService.GetCardArCaStatus(cardIdentification);
        }

        public static CardStatementAddInf GetFullCardStatement(CardStatement statement, string cardnumber, DateTime dateFrom, DateTime dateTo, byte language)
        {
            return CardDB.GetFullCardStatement(statement, cardnumber, dateFrom, dateTo, language);
        }

        public static string GetCardAccountNumber(string cardNumber)
        {
            return CardDB.GetCardAccountNumber(cardNumber);
        }

        public static string GetCardTechnology(ulong productId)
        {
            return CardDB.GetCardTechnology(productId);
        }

        public static string GetCardHolderFullName(ulong productId)
        {
            return CardDB.GetCardHolderFullName(productId);
        }
        public static ulong GetMainCardProductId(string notMainCardNumber)
        {
            return CardDB.GetMainCardProductId(notMainCardNumber);
        }

        public static void AddOtherBankAttachedCards(List<Card> cards, ulong customerNumber)
        {
            cards.AddRange(CardDB.GetOtherBankAttachedCards(customerNumber));
        }

        public static List<string> GetArmenianCardsBIN()
        {
            return CardDB.GetArmenianCardsBIN();
        }
        public static ulong GetCardHolderCustomerNumber(long productID)
        {
            return CardDB.GetCardHolderCustomerNumber(productID);
        }
        public static string GetCardHolderData(ulong productId, string dataType)
        {
            return CardDB.GetCardHolderData(productId, dataType);
        }
        public static string GetCardExpireDateActivatedInArCa(string cardNumber)
        {
            return CardDB.GetCardExpireDateActivatedInArCa(cardNumber);
        }

        public static List<CardRetainHistory> GetCardRetainHistory(string cardNumber)
        {
            return CardDB.GetCardRetainHistory(cardNumber);
        }
        public static string GetCustomerEmailByCardNumber(string cardNumber)
        {
            return CardDB.GetCustomerEmailByCardNumber(cardNumber);
        }
        public static string GetCardDesignImageByDesignId(int id)
        {
            return CardDB.GetCardDesignImageByDesignId(id);
        }

        public static void UpdateCardDesign(ulong productID, int designId)
        {
            CardDB.UpdateCardDesign(productID, designId);
            ChangeVirtualCardDesignInWallet(productID, designId);
        }

        public static short GetCardChangeType(long productId)
        {
            return CardDB.GetCardChangeType(productId);
        }

        public static int GetRelatedOfficeQuality(long productId, int officeID, short? cardNewType)
        {
            return CardDB.GetRelatedOfficeQuality(productId, officeID, cardNewType);
        }

        /// <summary>
        /// change virtual card design in wallet if user has tokenisated card
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="designId"></param>
        internal static void ChangeVirtualCardDesignInWallet(ulong productID, int designId)
        {
            CardDB.ChangeVirtualCardDesignInWallet(productID, designId);
        }
        /// <summary>
        /// Վերադարձնում է կից/լրացուցիչ քարտի հիմնական քարտի ՎԵՐՋԻՆ app_id-ն
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static long GetMainAppId(long productId)
        {
            return CardDB.GetMainAppId(productId);
        }
    }
}

