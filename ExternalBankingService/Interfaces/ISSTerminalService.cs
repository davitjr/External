using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExternalBanking;

using ACBALibrary;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICTPaymentService" in both code and config file together.
    [ServiceContract]
    [ServiceKnownType(typeof(CTLoanMatureOrder))]
    [ServiceKnownType(typeof(CTPaymentOrder))]
    public interface ISSTerminalService
    {
        [OperationContract]
        void Init(AuthorizedCustomer authorizedCustomer, SSTerminal ssTerminal, string clientIp, ExternalBanking.ACBAServiceReference.User user, byte language, SourceType source);

        [OperationContract]
        DateTime GetCurrentOperDay();

        [OperationContract]
        List<Communal> GetCommunals(SearchCommunal searchCommunal);

        [OperationContract]
        List<Card> GetCards(ProductQualityFilter filter);

        [OperationContract]
        List<Account> GetAccounts();

        [OperationContract]
        List<Deposit> GetDeposits(ProductQualityFilter filter);

        [OperationContract]
        List<Loan> GetLoans(ProductQualityFilter filter);

        [OperationContract]
        Account GetAccount(string accountNumber);

        [OperationContract]
        ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType);

        [OperationContract]
        double GetLastRates(string currency, RateType rateType, ExchangeDirection direction);

        [OperationContract]
        ActionResult SaveAndApproveCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        ActionResult SaveAndApprovePaymentOrder(PaymentOrder order);

        [OperationContract]
        ushort GetCrossConvertationVariant(string debitCurrency, string creditCurrency);
        [OperationContract]
        ulong GetAccountCustomerNumber(string accountNumber);

        [OperationContract]
        double GetLastCrossExchangeRate(string dCur, string cCur, ushort filialCode = 22000);

        [OperationContract]
        List<ExchangeRate> GetExchangeRates(int filialCode);
 
        [OperationContract]
        Loan GetLoanByLoanFullNumber(string loanFullNumber);

        [OperationContract]
        CreditLine GetCreditLineByLoanFullNumber(string loanFullNumber);

        [OperationContract]
        ActionResult SaveAndApproveUtilityPaymentOrder(UtilityPaymentOrder order);

        [OperationContract]
        string GetCommunalPaymentDescription(short utilityType, short abonentType, string searchData, string branch);

        [OperationContract]
        Deposit GetActiveDeposit(string depositFullNumber);

        [OperationContract]
        string GetAccountCurrency(string accountNumber);

        [OperationContract]
        ActionResult ConfirmOrder(Order order);

        [OperationContract]
        ActionResult SaveAndApproveMatureOrder(MatureOrder order);

        [OperationContract]
        Double GetLoanAmountForFullRepayment(Loan loan);

        [OperationContract]
        Order GetOrderDetails(long orderId);

        [OperationContract]
        double GetCardFeeForCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        List<Account> GetAccountsForUtility();

        [OperationContract]
        List<Account> GetAccountsForDebet();

        [OperationContract]
        List<Account> GetAccountsForCredit();

        [OperationContract]
        Account GetProductAccountFromCreditCode(string creditCode, ushort productType, ushort accountType);
        [OperationContract]
        double GetPaymentOrderFee(PaymentOrder order);
        [OperationContract]
        short CheckTerminalAuthorization(string terminalID, string ipAddress, string password);

        [OperationContract]
        Account GetCreditCodeAccountForMature(string creditCode, string loanCurrency, string amountCurrency);

        [OperationContract]
        ActionResult SendSMS(string phoneNumber, string messageText, int messageTypeID,  SourceType sourceType);

        [OperationContract]
        ushort GetTerminalFilial(string TerminalID);

        [OperationContract]
        List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType);

        [OperationContract]
        ArcaBalanceResponseData GetArCaBalanceResponseData(string cardNumber);

        [OperationContract]
        OperDayClosingStatus GetOperDayClosingStatus(ushort filialCode);

        [OperationContract]
        double GetAcccountAvailableBalance(string accountNumber);

        [OperationContract]
        Account GetOperationSystemAccount(string currency);

        [OperationContract]
        Account GetOperationSystemTransitAccount(string currency);

        [OperationContract]
        ActionResult SaveAndApprovePaymentToARCAOrder(PaymentToARCAOrder order);

        [OperationContract]
        bool IsCardAccount(string AccountNumber);

        [OperationContract]
        double GetCardFee(PaymentToARCAOrder paymentOrder);

        [OperationContract]
        bool HasActiveCreditLineForCardAccount(string cardAccount);

        [OperationContract]
        string GetAccountCustomerFullNameEng(string accountNumber);

        [OperationContract]
        string GetLoanTypeDescriptionEng(string loanFullNumber);

        [OperationContract]
        string GetJointTypeDescription(ushort jointType, Languages language);


        [OperationContract]
        Dictionary<int, string> GetCommunalDetailsTypes();

        [OperationContract]
        Loan GetLoan(string loanFullNumber);
    }
}
