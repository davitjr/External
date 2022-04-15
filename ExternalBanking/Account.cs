using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ContractServiceRef;
using ExternalBanking.DBManager;
using ExternalBanking.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class Account : IAttachedCard
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Հաշվի տեսակ
        /// </summary>
        public ushort AccountType { get; set; }

        /// <summary>
        /// Հաշվի տեսակի նկարագրություն
        /// </summary>
        public string AccountTypeDescription { get; set; }

        /// <summary>
        /// Հաշվի նկարագրություն (օր. քարտային հաշվի դեպքում քարտի վերջին 4 նիշերը)
        /// </summary>
        public string AccountDescription { get; set; }
        /// <summary>
        /// Հաշվի փակման օր
        /// </summary>
        public DateTime? ClosingDate { get; set; }
        /// <summary>
        /// Համատեղության տեսակ
        /// </summary>
        public ushort JointType { get; set; }

        /// <summary>
        /// Համատեղության տեսակի նկարագրություն
        /// </summary>
        public string JointTypeDescription { get; set; }

        /// <summary>
        /// Հաշվի բացման օր
        /// </summary>
        public DateTime? OpenDate { get; set; }

        /// <summary>
        /// Պրոդուկտի (քարտի/ավանդի) համար
        /// </summary>
        public string ProductNumber { get; set; }

        /// <summary>
        /// Հաշվի մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Հաշվի ստատուս
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Հաշվի սառեցման ա/թ
        /// </summary>
        public DateTime? FreezeDate { get; set; }

        /// <summary>
        ///Սառեցված գումար 
        /// </summary>
        public double? UnUsedAmount { get; set; }

        /// <summary>
        /// Հաշվի սառեցման ա/թ
        /// </summary>
        public DateTime? UnUsedAmountDate { get; set; }

        /// <summary>
        /// Հաշվի հասանելիության խումբ
        /// </summary>
        public string AccountPermissionGroup { get; set; }

        /// <summary>
        /// Հաշվի հասանելի մնացորդ
        /// </summary>
        public double AvailableBalance { get; set; }

        /// <summary>
        /// Հաշվի տեսակ Tbl_define_sint_acc աղյուսակից
        /// </summary>
        public int TypeOfAccount { get; set; }

        /// <summary>
        /// Հաշվի տեսակի նկարագրություն /անգլերեն/
        /// </summary>
        public string AccountTypeDescriptionEng { get; set; }
        /// <summary>
        /// Կցված քարտ
        /// </summary>
        public bool IsAttachedCard { get; set; }
        /// <summary>
        /// Կցված քարտով ipay-ում գործարք կատարելու համար քարտի ունիկալ իդենտիֆիկատոր
        /// </summary>
        public string BindingId { get; set; }
        /// <summary>
        /// Կցված քարտի համար (պատմության մեջ ցուցադրելու համար)
        /// </summary>
        public string AttachedCardNumber { get; set; }

        public string ProductId { get; set; }

        public Account()
        {
            AccountNumber = "0";
        }

        public Account(string accountNumber)
            : base()
        {
            AccountNumber = accountNumber;
        }

        public Account(ulong accountNumber)
            : base()
        {
            AccountNumber = accountNumber.ToString();
        }

        public static Account GetAccount(string accountNumber, ulong customerNumber)
        {
            Account account = AccountDB.GetAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetJointAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetJointDepositAccount(accountNumber, customerNumber);
            return account;
        }

        public static Account GetAccount(ulong accountNumber)
        {
            return AccountDB.GetAccount(accountNumber.ToString());
        }
        public static Account GetAccount(string accountNumber)
        {
            Account account = AccountDB.GetAccount(accountNumber);
            if (account != null && (account.AccountType == 2 || account.AccountType == 3 || account.AccountType == 5))
            {
                CreditLine creditLine = CreditLine.GetCreditLine(accountNumber);
                if (creditLine != null && creditLine.Type != 0)
                {
                    account.AccountType = (ushort)creditLine.Type;
                }
            }

            return account;
        }

        public static async Task<Account> GetAccountAsync(string accountNumber)
        {
            Account account = await AccountDB.GetAccountAsync(accountNumber);
            if (account != null && (account.AccountType == 2 || account.AccountType == 3 || account.AccountType == 5))
            {
                CreditLine creditLine = CreditLine.GetCreditLine(accountNumber);
                if (creditLine != null && creditLine.Type != 0)
                {
                    account.AccountType = (ushort)creditLine.Type;
                }
            }

            return account;


        }

        /// <summary>
        /// Վերադարձնում է ներբանկային հաշվի տվյալները
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static Account GetSystemAccount(string accountNumber)
        {
            return AccountDB.GetSystemAccount(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է ներբանկային հաշվի տվյալները առանց մնացորդի
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static Account GetSystemAccountWithoutBallance(string accountNumber)
        {
            return AccountDB.GetSystemAccountWithoutBallance(accountNumber);
        }

        /// <summary>
        /// Ընթացիկ հաշվի կամ ապառիկ տեղում հաշվեհամարի տվյալներ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static Account GetCurrentAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            account = AccountDB.GetCurrentAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetAparikTexumAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetCardDahkAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetClosedAparikTexumAccount(accountNumber, customerNumber);
            if (account == null)
                account = AccountDB.GetBankruptAccount(accountNumber, customerNumber);
            if (account == null)
            {
                var acc = Account.GetCardAccounts(customerNumber).Where(x => x.AccountNumber == accountNumber);
                if (acc?.Any() == true)
                {
                    account = acc.First();
                }
            }
            if (account == null)
                account = AccountDB.GetSystemAccount(accountNumber);

            return account;
        }


        public static List<Account> GetAccounts(ulong customerNumber)
        {
            return AccountDB.GetAccounts(customerNumber);
        }

        public static List<Account> GetClosedAccounts(ulong customerNumber)
        {
            return AccountDB.GetClosedAccounts(customerNumber);
        }

        public static List<Account> GetOtherBankAttachedCards(ulong customerNumber)
        {
            return AccountDB.GetOtherBankAttachedCards(customerNumber);
        }

        public static List<Account> GetCurrentAccounts(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Account> currentAccounts = new List<Account>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                currentAccounts.AddRange(AccountDB.GetCurrentAccounts(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                currentAccounts.AddRange(AccountDB.GetCurrentClosedAccounts(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                currentAccounts.AddRange(AccountDB.GetCurrentAccounts(customerNumber));
                currentAccounts.AddRange(AccountDB.GetCurrentClosedAccounts(customerNumber));
            }
            return currentAccounts;
        }

        public static List<Account> GetCardAccounts(ulong customerNumber)
        {
            return AccountDB.GetCardAccountList(customerNumber);
        }

        public static List<Account> GetDepositAccounts(ulong customerNumber)
        {
            return AccountDB.GetDepositAccountList(customerNumber);
        }
        public static List<Account> GetDecreasingDepositAccountList(ulong customerNumber)
        {
            return AccountDB.GetDecreasingDepositAccountList(customerNumber);
        }



        public ulong GetAccountCustomerNumber()
        {
            return AccountDB.GetAccountCustomerNumber(AccountNumber);
        }

        public AccountStatement GetStatement(DateTime dateFrom, DateTime dateTo, byte language, byte includeCurrencyRecalculations = 0, SourceType sourceType = 0, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {



            AccountStatement statement = AccountDB.GetStatement(AccountNumber, dateFrom, dateTo, language, sourceType, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc);

            if (includeCurrencyRecalculations == 0)
            {
                statement.Transactions.RemoveAll(m => m.OperationType == 499 || m.CurrentAccountNumber == 499);
            }

            Culture culture = new Culture((Languages)language);
            Localization.SetCulture(statement.Transactions, culture);

            return statement;
        }


        public bool IsCardAccount()
        {
            return AccountDB.IsCardAccount(AccountNumber);
        }

        public bool IsDepositAccount()
        {
            return AccountDB.IsDepositAccount(AccountNumber);
        }
        public bool IsIPayAccount()
        {
            return AccountDB.IsIPayAccount(AccountNumber);
        }

        public bool IsCurrentAccount()
        {
            return AccountDB.IsCurrentAccount(AccountNumber);
        }

        public bool IsRestrictedAccount()
        {
            return AccountDB.IsRestrictedAccount(AccountNumber);
        }

        public bool HaveActiveCard()
        {
            return AccountDB.HaveActiveCard(AccountNumber);
        }

        public bool HasActiveDeposit()
        {
            return AccountDB.HasActiveDeposit(AccountNumber);
        }

        public bool HasPensionApplication()
        {
            return AccountDB.HasPensionApplication(AccountNumber);
        }

        public static double GetAccountBalance(string accountNumber)
        {
            return AccountDB.GetAccountBalance(accountNumber);
        }

        public static List<Account> GetAccountsForOrder(ulong customerNumber, OrderType orderType, byte orderSubType, OrderAccountType accountType, SourceType sourceType, User user)
        {
            Customer customer = new Customer(user, customerNumber, Languages.hy);
            return customer.GetAccountsForOrder(orderType, orderSubType, accountType, sourceType);
        }

        public static List<KeyValuePair<ulong, double>> GetAccountJointCustomers(string accountNumber)
        {
            return AccountDB.GetAccountJointCustomers(accountNumber);
        }

        public static bool IsPoliceAccount(string accountNumber)
        {
            return AccountDB.IsPoliceAccount(accountNumber);
        }

        public static bool CheckAccountForPSN(string accountNumber)
        {
            return AccountDB.CheckAccountForPSN(accountNumber);
        }

        public bool HaveActiveProduct()
        {
            return AccountDB.HaveActiveProduct(AccountNumber);
        }

        public bool HaveActiveDepositForCurrentAccount()
        {
            return AccountDB.HaveActiveDepositForCurrentAccount(AccountNumber);
        }

        public bool HaveActiveLoanForCurrentAccount()
        {
            return AccountDB.HaveActiveLoanForCurrentAccount(AccountNumber);
        }

        public bool HaveActiveCreditLineForCurrentAccount()
        {
            return AccountDB.HaveActiveCreditLineForCurrentAccount(AccountNumber);
        }

        public bool HaveActiveSocPackageCreditLineForCurrentAccount()
        {
            return AccountDB.HaveActiveSocPackageCreditLineForCurrentAccount(AccountNumber, GetAccountCustomerNumber());
        }
        public bool HaveActiveOperationByPeriodForCurrentAccount()
        {
            return AccountDB.HaveActiveOperationByPeriodForCurrentAccount(AccountNumber);
        }
        public bool HaveHBForCurrentAccount()
        {
            return AccountDB.HaveHBForCurrentAccount(AccountNumber, GetAccountCustomerNumber());
        }
        public bool HaveCurrencyCardsForCurrentAccount()
        {
            return AccountDB.HaveCurrencyCardsForCurrentAccount(AccountNumber, GetAccountCustomerNumber());
        }
        public bool IsDAHKAvailability()
        {
            return AccountDB.IsDAHKAvailability(AccountNumber);
        }
        public bool HaveTaxInspectorateApproval()
        {
            return AccountDB.HaveTaxInspectorateApproval(AccountNumber);
        }

        /// <summary>
        /// Որոշում ենք պրոդուկտին կցված ընթացիկ հաշիվը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productType"></param>
        /// <param name="accountType"></param>
        /// <returns></returns>
        public static Account GetProductAccount(ulong productId, ushort productType, ushort accountType)
        {
            return AccountDB.GetProductAccount(productId, productType, accountType);
        }

        /// <summary>
        /// Ստուգում ենք արդյոք փոխանցումը օգտագործողի հաշիվների միջև է թե ոչ 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="debitAccountNumber"></param>
        /// <param name="recieverAccountNumber"></param>
        /// <returns></returns>
        public static bool IsUserAccounts(ulong customerNumber, string debitAccountNumber, string recieverAccountNumber)
        {
            return AccountDB.IsUserAccounts(customerNumber, debitAccountNumber, recieverAccountNumber);
        }

        /// <summary>
        /// Ստուգում ենք հաշվի  արգելանքի կամ բռնագանձման ենթակա լինելը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static bool CheckAccountForDAHK(string accountNumber)
        {
            return AccountDB.CheckAccountForDAHK(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լրացուցիչ տվյալները
        /// </summary>
        /// <returns></returns>
        public List<AdditionalDetails> GetAccountAdditions()
        {
            List<AdditionalDetails> additionalDetails = AccountDB.GetAccountAdditions(this.AccountNumber);

            DataTable additionsTypes = Info.GetAccountAdditionsTypes();
            additionalDetails.ForEach(m =>
            {
                DataRow[] dr = additionsTypes.Select(" AdditionId=" + m.AdditionType.ToString());
                m.AdditionTypeDescription = (m.AdditionType == 11) ? "Հայտի համար " : Utility.ConvertAnsiToUnicode(dr[0]["AdditionDescription"].ToString());


                //Քաղվածքի ստացման դիմումի արժեքը բառերով
                if (m.AdditionType == 5)
                {
                    if (m.AdditionValue != "-1" && m.AdditionValue != "")
                    {
                        m.AdditionValue = Info.GetStatementDeliveryTypeDescription(int.Parse(m.AdditionValue), Languages.hy);
                    }
                    else
                    {
                        m.AdditionValue = "Մուտքագրված չէ";
                    }

                }

            });

            return additionalDetails;
        }

        /// <summary>
        /// Վերադարձնում է քաղվածքների ստացման տեսակը
        /// </summary>
        /// <returns></returns>
        public int GetAccountStatementDeliveryType()
        {
            int AccountStatementDeliveryType = -1;
            List<AdditionalDetails> additionalDetails = AccountDB.GetAccountAdditions(this.AccountNumber);

            if (additionalDetails.Exists(m => m.AdditionType == 5))
            {
                AdditionalDetails additionalDetailsStatementDeliveryType = additionalDetails.Find(m => m.AdditionType == 5);
                AccountStatementDeliveryType = int.Parse(additionalDetailsStatementDeliveryType.AdditionValue);
            }

            return AccountStatementDeliveryType;
        }
        public static List<Account> GetAccountsForNewDeposit(DepositOrder order)
        {
            return AccountDB.GetAccountsForNewDeposit(order);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        public static double GetAcccountAvailableBalance(string accountNumber, string creditAccountNumber = "0")
        {
            return Convert.ToDouble(AccountDB.GetAcccountAvailableBalance(accountNumber, creditAccountNumber));
        }

        public static async Task<double> GetAcccountAvailableBalanceAsync(string accountNumber, string creditAccountNumber = "0")
        {
            return Convert.ToDouble(await AccountDB.GetAcccountAvailableBalanceAsync(accountNumber, creditAccountNumber));
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լիազորված անձանց
        /// </summary>
        /// <returns></returns>
        public static List<OPPerson> GetAccountAssigneeCustomers(string accountNumber, OrderType orderType)
        {
            return AccountDB.GetAccountAssigneeCustomers(accountNumber, orderType);
        }
        public static List<Account> GetDAHKAccountList(ulong customerNumber)
        {
            return AccountDB.GetDAHKAccountList(customerNumber);
        }
        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        public static double GetAcccountAvailableBalance(string accountNumber, DateTime setDate, string currency = null, bool attentionNotFreezed = true)
        {
            return Convert.ToDouble(AccountDB.GetAcccountAvailableBalance(accountNumber, setDate, currency, attentionNotFreezed));
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդի ոչ սառեցված հաշիվը
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="currency">Արժույթ</param>
        /// <returns></returns>
        public static string GetNotFreezedCurrentAccount(ulong customerNumber, string currency)
        {
            return AccountDB.GetNotFreezedCurrentAccount(customerNumber, currency);

        }

        /// <summary>
        /// Վերադարձնում է ներբանկային հաշիվը տարբեր տեսակի գործառնությունների համար
        /// </summary>
        /// <param name="accountType">Հաշվի տեսակ</param>
        /// <param name="operationCurrency">Գործարքի արժույթ</param>
        /// <param name="operationFilialCode">Գորածարքի մասնաճյուղ</param>
        /// <returns></returns>
        public static Account GetOperationSystemAccount(uint accountType, string operationCurrency, ushort operationFilialCode, ushort customerType = 0, string debitAccountNumber = "0", string utilityBranch = "", ulong customerNumber = 0)
        {
            return AccountDB.GetOperationSystemAccount(accountType, operationCurrency, operationFilialCode, customerType, debitAccountNumber, utilityBranch, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հիմնադրամի հաշիվը
        /// </summary>
        /// <returns></returns>
        public static Account GetRAFoundAccount()
        {
            return AccountDB.GetRAFoundAccount();
        }

        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        public static double GetAcccountAvailableBalanceNotFreezed(string accountNumber, string currency)
        {
            return Convert.ToDouble(AccountDB.GetAcccountAvailableBalanceNotFreezed(accountNumber, currency));
        }


        /// <summary>
        /// Վերադարձնում է հաշվի բացման աղբյուրը
        /// </summary>
        /// <returns></returns>
        public static SourceType GetAccountSource(string accountNumber, ulong customerNumber)
        {
            return AccountDB.GetAccountSource(accountNumber, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաշվեհամարի վրա սահմանված հատուկ սակագնի վերաբերյալ հաղորդագրություն,եթե այն սահմանված է, եթե ոչ՝ վերադարձնում է ""
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="additionID">3 Փոխանցումների դեպքում, 4 կանխիկացման</param>
        /// <returns></returns>
        public static string GetSpesialPriceMessage(string accountNumber, short additionID)
        {
            return AccountDB.GetSpesialPriceMessage(accountNumber, additionID);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լիազորված անձանց կատարած գործարքի խումբը
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public static short GetAssigneeOperationGroup(OrderType orderType)
        {
            short operationGroup = 1;

            if (orderType == OrderType.CashCredit || orderType == OrderType.CashCreditConvertation || orderType == OrderType.CashDebit || orderType == OrderType.ReestrTransferOrder)
            {
                operationGroup = 2;
            }
            if (orderType == OrderType.Convertation || orderType == OrderType.InBankConvertation)
            {
                operationGroup = 3;
            }

            return operationGroup;

        }

        public static bool AccountAccessible(string accountNumber, long accountGroup)
        {
            return AccountDB.AccountAccessible(accountNumber, accountGroup);
        }

        /// <summary>
        /// Հաշվի փակման պատմություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<AccountClosingHistory> GetAccountClosinghistory(ulong customerNumber)
        {
            return AccountDB.GetAccountClosinghistory(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է քարտի սպասարկման միջնորդավճարի հաշիվը
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="accountType"></param>
        /// <returns></returns>
        public static Account GetCardServiceFeeAccount(ulong appId, int accountType)
        {
            return AccountDB.GetCardServiceFeeAccount(appId, accountType);
        }

        public static Account GetCardServiceFeeAccountNew(ulong appId)
        {
            return AccountDB.GetCardServiceFeeAccountNew(appId);
        }


        /// <summary>
        /// Վերադարձնում է հաշվի շարժը
        /// </summary>
        /// <param name="startDate">սկիզբ</param>
        /// <param name="endDate">վերջ</param>
        /// <returns></returns>
        public AccountFlowDetails GetAccountFlowDetails(DateTime startDate, DateTime endDate)
        {
            return AccountDB.GetAccountFlowDetails(this.AccountNumber, startDate, endDate);
        }

        public static List<Account> GetClosedDepositAccountList(DepositOrder order)
        {
            return AccountDB.GetClosedDepositAccountList(order);
        }

        public static List<Account> GetAccountListForCardRegistration(ulong customerNumber, string cardCurrency, int cardFilial)
        {
            return AccountDB.GetAccountListForCardRegistration(customerNumber, cardCurrency, cardFilial);
        }

        public static List<Account> GetOverdraftAccountsForCardRegistration(ulong customerNumber, string cardCurrency, int cardFilial)
        {
            return AccountDB.GetOverdraftAccountsForCardRegistration(customerNumber, cardCurrency, cardFilial);
        }


        /// <summary>
        /// Վերադարձնում է սիստենային հաշիվը nn ից կախված
        /// </summary>
        /// <returns></returns>
        public static Account GetSystemAccountByNN(uint nn, uint filialCode = 22000)
        {
            return AccountDB.GetSystemAccountByNN(nn, filialCode);
        }

        /// <summary>
        /// Վերադարձնում է ԱԳՍ ի հաշիվները կածված մասնաճյուղից և արժույթից
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static List<Account> GetATSSystemAccounts(string currency, uint filialCode)
        {
            return AccountDB.GetATSSystemAccounts(currency, filialCode);
        }
        /// <summary>
        /// Վերադարձնում է ԱԳՍ ի հաշիվների միայն հաշվեհամարները
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static List<Account> GetATSSystemAccounts(uint filialCode)
        {
            return AccountDB.GetATSSystemAccounts(filialCode);
        }

        /// <summary>
        /// Որոշում է տվյալ մասնաճյուղում առկա է ԱՏՍ-ի հաշիվ
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static bool HasATSSystemAccountInFilial(uint filialCode)
        {
            return AccountDB.HasATSSystemAccountInFilial(filialCode);
        }

        /// <summary>
        /// Հաճախորդներ տարանցիկ հաշիվներ
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static List<Account> GetTransitAccountsForDebitTransactions(uint filialCode)
        {
            return AccountDB.GetTransitAccountsForDebitTransactions(filialCode);
        }

        /// <summary>
        /// Վերադարձնում է անհրաժեշտ հաշիվը պրոդուկտների հաշիվների ցանկից ըստ վարկի կոդի	
        /// </summary>
        /// <param name="creditCode"></param>
        /// <param name="productType"></param>
        /// <param name="accountType"></param>
        /// <returns></returns>
        public static Account GetProductAccountFromCreditCode(string creditCode, ushort productType, ushort accountType)
        {
            return AccountDB.GetProductAccountFromCreditCode(creditCode, productType, accountType);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի նկարագրությունը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        /// 
        public static string GetAccountDescription(string accountNumber)
        {
            string accountDescription = null;
            int bankCode = int.Parse(accountNumber.Substring(0, 5));
            if (bankCode >= 22000 && bankCode < 22300)
            {
                accountDescription = AccountDB.GetAccountDescription(accountNumber);
            }
            return accountDescription;
        }


        public static ulong GetBankruptcyManager(string accountNumber)
        {
            return AccountDB.GetBankruptcyManager(accountNumber);
        }

        /// <summary>
        /// Վերդարձնում է պարտատոմսի արժեկտրոնների վճարման համար նախատեսված հաշիվները (դրամային ընթացիկ)
        /// </summary>
        /// <returns></returns>
        public static List<Account> GetAccountsForCouponRepayment(ulong customerNumber)
        {
            List<Account> accounts = GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);
            accounts.RemoveAll(m => m.Currency != "AMD");

            if (accounts != null && accounts.Count > 0)
                accounts = accounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount);
            accounts.RemoveAll(m => m.Status == 1);

            return accounts;
        }

        /// <summary>
        /// Վերդարձնում է պարտատոմսի գումարների մարման համար նախատեսված հաշիվները (պարտատոմսի արժույթով)
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="currency">Պարտատոմսի արժույթ</param>
        /// <returns></returns>
        public static List<Account> GetAccountsForBondRepayment(ulong customerNumber, string currency)
        {
            List<Account> accounts = GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);
            accounts.RemoveAll(m => m.Currency != currency);

            if (accounts != null && accounts.Count > 0)
                accounts = accounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount);
            accounts.RemoveAll(m => m.Status == 1);

            return accounts;
        }

        /// <summary>
        /// Ստուհում է հաշիվը փակ է թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static bool CheckAccountIsClosed(string accountNumber)
        {
            return AccountDB.CheckAccountIsClosed(accountNumber);
        }

        public static List<Account> GetCustomerTransitAccounts(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Account> currentAccounts = new List<Account>();

            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                currentAccounts.AddRange(AccountDB.GetCustomerTransitAccounts(customerNumber));

            }
            if (filter == ProductQualityFilter.Closed)
            {
                currentAccounts.AddRange(AccountDB.GetClosedCustomerTransitAccounts(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                currentAccounts.AddRange(AccountDB.GetCustomerTransitAccounts(customerNumber));
                currentAccounts.AddRange(AccountDB.GetClosedCustomerTransitAccounts(customerNumber));
            }

            return currentAccounts;
        }


        /// <summary>
        /// Վերադարձնում է հաշիվը [Tbl_all_accounts;]-ում առկայության դեպքում
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static Account GetAccountFromAllAccounts(string accountNumber)
        {
            Account account = AccountDB.GetAccountFromAllAccounts(accountNumber);
            if (account != null && (account.AccountType == 2 || account.AccountType == 3 || account.AccountType == 5))
            {
                CreditLine creditLine = CreditLine.GetCreditLine(accountNumber);
                if (creditLine != null && creditLine.Type != 0)
                {
                    account.AccountType = (ushort)creditLine.Type;
                }
            }
            return account;
        }
        public static List<Account> GetCreditCodesTransitAccounts(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Account> currentAccounts = new List<Account>();

            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                currentAccounts.AddRange(AccountDB.GetCreditCodesTransitAccounts(customerNumber));
            }
            return currentAccounts;
        }

        public static bool HasOrHadAccount(ulong customerNumber)
        {
            return AccountDB.HasOrHadAccount(customerNumber);
        }

        public static string GetAccountCurrency(string accountNumber)
        {
            return AccountDB.GetAccountCurrency(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք հաշիվը ժամանակավոր է և/կամ սառեցված է ստորագրության նմուշի և/կամ 
        /// հարկային տեսչություն չուղարկված լինելու հիմքով
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static bool IsAccountForbiddenForTransfer(Account account)
        {
            bool isForbidden = false;

            if (account != null)
            {
                //Ժամանակավոր հաշիվ
                if (account.Status == 1)
                {
                    isForbidden = true;
                }
                else if (account.FreezeDate != null)
                {
                    List<AccountFreezeDetails> freezeDetails = AccountFreezeDetails.GetAccountFreezeHistory(account.AccountNumber.ToString(), 1, 0);
                    //Սառեցված հաշիվ
                    if (freezeDetails != null)
                    {
                        //սառեցված է ստորագրության նմուշի և / կամ հարկային տեսչություն չուղարկված լինելու հիմքով
                        if (freezeDetails.Find(m => m.ReasonId == 10 || m.ReasonId == 14) != null)
                        {
                            isForbidden = true;
                        }
                    }
                }
            }

            return isForbidden;
        }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք հաշիվը POS հաշիվ է, թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool IsPOSAccount()
        {
            return AccountDB.IsPOSAccount(AccountNumber);
        }

        public static List<Account> GetCredentialOrderFeeAccounts(ulong customerNumber)
        {
            return AccountDB.GetCredentialOrderFeeAccounts(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հաշվեհամարի համար գոյություն ունի կցված պայմանագիր, թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static bool HasUploadedAccountContract(string accountNumber)
        {
            return AccountDB.HasUploadedAccountContract(accountNumber);
        }

        public static string GetAccountsForLeasing(ulong CustomerNumber)
        {
            return LeasingAccountDB.GetCurrentLeasingAccountList(CustomerNumber);
        }
        public static string GetAccountCustomerFullNameEng(string accountNumber)
        {
            return AccountDB.GetAccountCustomerFullNameEng(accountNumber);
        }

        public bool ISDahkCardTransitAccount(string accountNumber)
        {
            return AccountDB.ISDahkCardTransitAccount(accountNumber);
        }

        public static List<Account> GetAccountsDigitalBanking(ulong accountNumber)
        {
            return AccountDB.GetAccountsDigitalBankingAsync(accountNumber).Result;
        }

        public static double GetUserTotalAvailableBalance(int digitalUserId, ulong customerNumber, int digitalUserID, int lang)
        {
            double balance = 0;
            List<Deposit> deposits = null;
            DigitalAccountRestConfigurations restConfigurations = new DigitalAccountRestConfigurations();
            var configs = restConfigurations.GetCustomerAccountRestConfig(digitalUserId, customerNumber, lang);
            foreach (var item in configs.Configurations.Where(x => x.AccountRestAttributeValue))
            {
                switch (item.AccountRestTypeId)
                {
                    case 1:
                        List<Account> curAcc = AccountDB.GetCurrentAccountsForTotalBalance(customerNumber).Result;
                        foreach (var acc in curAcc)
                        {
                            if (acc.Currency == "AMD")
                                balance += acc.AvailableBalance;
                            else
                            {
                                balance += Utility.GetLastExchangeRate(acc.Currency, RateType.NonCash, ExchangeDirection.Buy) * acc.AvailableBalance;
                            }
                        }
                        break;
                    case 2:
                        List<Card> cards = CardDB.GetCardsForTotalBalance(customerNumber).Result;
                        foreach (Card card in cards)
                        {
                            var arcaBalance = card.GetArCaBalance(digitalUserID); //  for Online/Mobile User
                            if (arcaBalance.Key == "00")
                            {
                                if (card.Currency == "AMD")
                                    balance += arcaBalance.Value;
                                else
                                {
                                    balance += Utility.GetLastExchangeRate(card.Currency, RateType.NonCash, ExchangeDirection.Buy) * arcaBalance.Value;
                                }
                            }
                        }
                        break;
                    case 3:
                        List<CreditLine> creditLines = CreditLineDB.GetCreditLinesForTotalBalance(customerNumber);
                        foreach (CreditLine creditLine in creditLines)
                        {
                            var unUsedAmount = creditLine.StartCapital + (creditLine.Quality == 11 || creditLine.Quality == 12 ? creditLine.OutCapital : creditLine.CurrentCapital);
                            if (creditLine.Currency == "AMD")
                                balance += unUsedAmount;
                            else
                            {
                                balance += Utility.GetLastExchangeRate(creditLine.Currency, RateType.NonCash, ExchangeDirection.Buy) * unUsedAmount;
                            }
                        }
                        break;
                    case 4:
                        if (deposits == null)
                            deposits = DepositDB.GetDepositsForTotalBalance(customerNumber);
                        foreach (Deposit deposit in deposits)
                        {
                            if (deposit.Currency == "AMD")
                                balance += deposit.StartCapital;
                            else
                            {
                                balance += Utility.GetLastExchangeRate(deposit.Currency, RateType.NonCash, ExchangeDirection.Buy) * deposit.StartCapital;
                            }
                        }
                        break;
                    case 5:
                        if (deposits == null)
                            deposits = DepositDB.GetDepositsForTotalBalance(customerNumber);
                        foreach (Deposit deposit in deposits)
                        {
                            if (deposit.Currency == "AMD")
                                balance += deposit.CurrentRateValue;
                            else
                            {
                                balance += Utility.GetLastExchangeRate(deposit.Currency, RateType.NonCash, ExchangeDirection.Buy) * deposit.CurrentRateValue;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return balance;
        }

        /// <summary>
        /// Վերադարձնում է այն հաշիվները , որոնց համար անհրաժեշտ է ստուգել կանխիկ մուտքի հայտեր,սպասարկամ պատքի հայտը ուղարկելուց առաջ 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<string> GetAccountsForServicePaymentChecking(ulong customerNumber)
        {
            return AccountDB.GetAccountsForServicePaymentChecking(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաշիվը սպասարկող մասնաճյուղը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static int GetAccountServicingFilialCode(string accountNumber)
        {
            return AccountDB.GetAccountServicingFilialCode(accountNumber);
        }

        public static bool IsOurAccount(string accountNumber)
        {
            return AccountDB.IsOurAccount(accountNumber);
        }
        public static string GetCurrentAccountContractBefore(long docId, ulong customerNumber, int attacheDocType = 0)
        {
            string result = null;
            Customer customer = new Customer();
            AccountOrder order = customer.GetAccountOrder(docId);
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Contract contract = null;
            string contractName = "CurrentAccContract";

            if (attacheDocType != 0)
            {
                contract = new Contract();
                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = attacheDocType;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }

            parameters.Add(key: "customerNumber", value: customerNumber.ToString());
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "currencyHB", value: order.Currency);
            parameters.Add(key: "reopen", value: "0");
            parameters.Add(key: "armNumber", value: "0");
            parameters.Add(key: "armNumberStr", value: "0");
            parameters.Add(key: "accountTypeHB", value: (order.AccountType - 1).ToString());
            parameters.Add(key: "thirdPersonCustomerNumberHB", value: "0");
            parameters.Add(key: "filialCode", value: filialCode.ToString());
            parameters.Add(key: "receiveTypeHB", value: order.StatementDeliveryType.ToString());
            result = Convert.ToBase64String(Contracts.RenderContract(contractName, parameters, "CurrentAccContract.pdf", contract));
            return result;
        }
        public static string GetAttachedCardBindingId(long docId)
        {
            return AccountDB.GetAttachedCardBindingId(docId);
        }
        public static string GetAttachedCardNumber(long docId)
        {
            return AccountDB.GetAttachedCardNumber(docId);
        }
        public static double GetAttachedCardFee(long docId)
        {
            return AccountDB.GetAttachedCardFee(docId);
        }
        public bool DAHKRestrictionForCardAccount()
        {
            return AccountDB.DAHKRestrictionForCardAccount(AccountNumber);
        }

        public ActionResult CreditAccountValidation(short notStrictFreezeChecking = 1, short notStrictDAHKChecking = 1, short notStrictDebtType = 1)
        {
            return AccountDB.CreditAccountValidation(Int64.Parse(this.AccountNumber), notStrictFreezeChecking, notStrictDAHKChecking, notStrictDebtType);
        }

        public bool CheckStateRevenueCommitteeArrest()
        {
            return AccountDB.CheckStateRevenueCommitteeArrest(this.AccountNumber);
        }

        public static byte CheckAccessToThisAccounts(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return 0;
            else
                return AccountDB.CheckAccessToThisAccounts(accountNumber);
        }

        public static ulong CheckCustomerFreeFunds(string accountNumber)
        {
            return AccountDB.CheckCustomerFreeFunds(accountNumber);
        }
        public static bool GetRightsTransferAvailability(string accountNumber)
        {
            return AccountDB.GetRightsTransferAvailability(accountNumber);
        }
        public static bool GetRightsTransferVisibility(string accountNumber)
        {
            return AccountDB.GetRightsTransferVisibility(accountNumber);
        }
        public static bool GetCheckCustomerIsThirdPerson(string accountNumber, ulong customerNumber)
        {
            return AccountDB.GetCheckCustomerIsThirdPerson(accountNumber, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ առժույթով փակված ընթացիկ հաշիվները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        internal static string GetClosingCurrentAccountsNumber(ulong customerNumber, string currency) => AccountDB.GetClosingCurrentAccountsNumber(customerNumber, currency);


        /// <summary>
        /// Վերդարձնում է բաժնետոմսի արժեկտրոնների վճարման համար նախատեսված հաշիվները (դրամային ընթացիկ)
        /// </summary>
        /// <returns></returns>
        public static List<Account> GetAccountsForStock(ulong customerNumber)
        {
            List<Account> currentAccounts = GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);

            currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount && m.JointType == 0 && m.TypeOfAccount != 283 && m.TypeOfAccount != 282);
            return currentAccounts;
        }


        internal static double GetAccountAvailableBalanceForStocksInAmd(string accountNumber) => AccountDB.GetAccountAvailableBalanceForStocksInAmd(accountNumber);

        public static List<Account> GetAllCurrentAccounts(ulong customerNumber)
        {
            List<Account> currentAccounts = new List<Account>();

            currentAccounts.AddRange(AccountDB.GetAllCurrentAccounts(customerNumber));

            return currentAccounts;
        }

    }
}
