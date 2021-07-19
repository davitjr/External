using ExternalBankingRESTService.XBS;
using ExternalBankingRESTService.XBSInfo;
using ExternalBankingRESTService.MAuthorizationServiceReference;
using NLog;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Configuration;
using static ExternalBankingRESTService.Enumerations;
using System.Configuration;
using ExternalBankingRESTService.OnlineBankingSecServRef;

namespace ExternalBankingRESTService
{

    public class XBRESTService : IXBRESTService
    {

        //Տվյալների վերադարձման լեզու
        byte Language { get; set; }

        //Նույնականացման ստուգման արդյունք
        Result AutorisationResult { get; set; }

        //Կլիենտի IP հասցե
        string ClientIp { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        //Նույնականացում անցած օգտագործող
        XBS.AuthorizedCustomer AutorizedCustomer { get; set; }

        private List<HBProductPermission> _userProductPermission { get; set; }

        /// <summary>
        /// Օգտագործող
        /// </summary>
        private ExternalBankingRESTService.XBS.User user = new ExternalBankingRESTService.XBS.User();

        public LoginRequestResponse MobileAuthorization()
        {
            LoginRequestResponse loginRequestResponse = new LoginRequestResponse();

            try
            {

                bool isStopFlagSign = bool.Parse(WebConfigurationManager.AppSettings["StopFlagSign"].ToString());

                if (isStopFlagSign)
                {
                    loginRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loginRequestResponse.Result.Description = "Համակարգը ժամանակավորապես չի գործում:";
                    loginRequestResponse.Result.Description = loginRequestResponse.Result.Description + Environment.NewLine + "System is currently unavailable due to preventive maintenance.";
                    return loginRequestResponse;
                }

                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    OnlineBankingSecServRef.LoginInfo loginInfo = new OnlineBankingSecServRef.LoginInfo();
                    OnlineBankingUser mobileUserData = new OnlineBankingUser();
                    ///Օգտագործող 
                    if (WebOperationContext.Current.IncomingRequest.Headers["UserName"] != null)
                        loginInfo.UserName = WebOperationContext.Current.IncomingRequest.Headers["UserName"];

                    ///Գաղտնաբառ  
                    if (WebOperationContext.Current.IncomingRequest.Headers["Password"] != null)
                        loginInfo.Password = WebOperationContext.Current.IncomingRequest.Headers["Password"];

                    ///Թվային կոդ
                    if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                        loginInfo.OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                    ///IP հասցե
                    loginInfo.IpAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                    //Օպերացիոն համակարգ
                    loginInfo.AdditionalDetails = new Dictionary<string, string>();
                    if (WebOperationContext.Current.IncomingRequest.Headers["Dev"] != null)
                    {
                        loginInfo.AdditionalDetails.Add("OS", WebOperationContext.Current.IncomingRequest.Headers["Dev"].ToString());
                    }
                    else
                    {
                        loginInfo.AdditionalDetails.Add("OS", "0");
                    }

                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        mobileUserData = proxyClient.AuthorizeMobileUser(loginInfo, 1);
                    }

                    ///Եթե անցել է նույնականացում
                    if (mobileUserData.AuthorizationResult.IsAuthorized)
                    {
                        loginRequestResponse.Result.ResultCode = ResultCodes.normal;
                        loginRequestResponse.SessionId = mobileUserData.SessionID;
                        loginRequestResponse.PasswordChangeRequirement = mobileUserData.ChangeRequirement;
                        loginRequestResponse.UserPermission = mobileUserData.Permission;

                        XBS.CustomerMainData customerData = new CustomerMainData();

                        using (XBServiceClient xbsProxy = new XBServiceClient())
                        {
                            customerData = xbsProxy.GetCustomerMainData(ulong.Parse(mobileUserData.CustomerNumber));
                        }

                        loginRequestResponse.FullName = customerData.CustomerDescription;
                        loginRequestResponse.FullNameEnglish = customerData.CustomerDescriptionEng;
                        loginRequestResponse.IsLegal = customerData.CustomerType == 6 ? false : true;
                    }
                    else
                    {
                        loginRequestResponse.Result.ResultCode = ResultCodes.failed;
                        loginRequestResponse.Result.Description = mobileUserData.AuthorizationResult.Description;
                    }
                }
                else
                {
                    MAuthorizationServiceReference.LoginInfo loginInfo = new MAuthorizationServiceReference.LoginInfo();
                    MobileUserData mobileUserData = new MobileUserData();
                    ///Օգտագործող 
                    if (WebOperationContext.Current.IncomingRequest.Headers["UserName"] != null)
                        loginInfo.UserName = WebOperationContext.Current.IncomingRequest.Headers["UserName"];

                    ///Գաղտնաբառ  
                    if (WebOperationContext.Current.IncomingRequest.Headers["Password"] != null)
                        loginInfo.Password = WebOperationContext.Current.IncomingRequest.Headers["Password"];

                    ///Թվային կոդ
                    if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                        loginInfo.OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                    ///IP հասցե
                    loginInfo.IpAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                    //Օպերացիոն համակարգ
                    loginInfo.AdditionalDetails = new Dictionary<string, string>();
                    if (WebOperationContext.Current.IncomingRequest.Headers["Dev"] != null)
                    {
                        loginInfo.AdditionalDetails.Add("OS", WebOperationContext.Current.IncomingRequest.Headers["Dev"].ToString());
                    }
                    else
                    {
                        loginInfo.AdditionalDetails.Add("OS", "0");
                    }

                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        mobileUserData = proxyClient.AuthorizeMobileUser(loginInfo, 1);
                    }

                    ///Եթե անցել է նույնականացում
                    if (mobileUserData.AuthorizationResult.IsAuthorized)
                    {
                        loginRequestResponse.Result.ResultCode = ResultCodes.normal;
                        loginRequestResponse.SessionId = mobileUserData.SessionID;
                        loginRequestResponse.PasswordChangeRequirement = mobileUserData.ChangeRequirement;
                        loginRequestResponse.UserPermission = mobileUserData.Permission;

                        XBS.CustomerMainData customerData = new CustomerMainData();

                        using (XBServiceClient xbsProxy = new XBServiceClient())
                        {
                            customerData = xbsProxy.GetCustomerMainData(ulong.Parse(mobileUserData.CustomerNumber));
                        }

                        loginRequestResponse.FullName = customerData.CustomerDescription;
                        loginRequestResponse.FullNameEnglish = customerData.CustomerDescriptionEng;
                        loginRequestResponse.IsLegal = customerData.CustomerType == 6 ? false : true;
                    }
                    else
                    {
                        loginRequestResponse.Result.ResultCode = ResultCodes.failed;
                        loginRequestResponse.Result.Description = mobileUserData.AuthorizationResult.Description;
                    }
                }           
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                loginRequestResponse.Result.ResultCode = ResultCodes.failed;
                loginRequestResponse.Result.Description = Resource.InternalError;
            }

            return loginRequestResponse;
        }

        public AccountRequestResponse GetAccount(string accountNumber)
        {
            AccountRequestResponse accountRequestResponse = new AccountRequestResponse();
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {


                    Account account;

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        account = proxyClient.GetCurrentAccount(accountNumber);
                        account.Balance = proxyClient.GetAcccountAvailableBalance(accountNumber);//5555
                    }
                    accountRequestResponse.Result.ResultCode = ResultCodes.normal;
                    accountRequestResponse.Account = account;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountRequestResponse.Result.Description = Resource.InternalError;
                }
            }
            else
                accountRequestResponse.Result = AutorisationResult;

            return accountRequestResponse;
        }

        public AccountsRequestResponse GetAccounts()
        {
            AccountsRequestResponse accountsRequestResponse = new AccountsRequestResponse();
            List<Account> accounts;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accounts = proxyClient.GetAccountsDigitalBanking();
                        foreach (Account account in accounts)
                        {
                            account.Balance = proxyClient.GetAcccountAvailableBalance(account.AccountNumber);
                        }
                    }

                    accountsRequestResponse.Accounts = accounts;
                    accountsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accountsRequestResponse.Result = AutorisationResult;

            return accountsRequestResponse;
        }

        private bool HasProductPermission(string accountNumber)
        {
            bool hasPermission = false;
            if (_userProductPermission.Exists(m => m.ProductAccountNumber == accountNumber && m.ProductType!=HBProductPermissionType.Periodic))
            {
                hasPermission = true;
            }
            return hasPermission;
        }

        private bool HasProductPermission(string accountNumber,ulong productID)
        {
            bool hasPermission = false;
            if (_userProductPermission.Exists(m => m.ProductAccountNumber == accountNumber && m.ProductAppID== productID))
            {
                hasPermission = true;
            }
            return hasPermission;
        }
        private bool HasProductPermissionByProductID(ulong productID)
        {
            bool hasPermission = false;
            if (_userProductPermission.Exists(m=> m.ProductAppID == productID))
            {
                hasPermission = true;
            }
            return hasPermission;
        }


        public AccountsRequestResponse GetCurrentAccounts(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            AccountsRequestResponse accountsRequestResponse = new AccountsRequestResponse();
            List<Account> accounts;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accounts = proxyClient.GetCurrentAccounts(filter);
                        foreach (Account account in accounts)
                        {
                            account.Balance = proxyClient.GetAcccountAvailableBalance(account.AccountNumber);
                        }
                        accounts.RemoveAll(a => (a.AccountType == 18 && a.TypeOfAccount == 282) || (a.AccountType == 18 && a.TypeOfAccount == 283) || a.AccountType == (ushort)ProductType.CardDahkAccount);
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        accounts.RemoveAll(m => !HasProductPermission(m.AccountNumber));
                    }

                    accountsRequestResponse.Accounts = accounts;
                    accountsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accountsRequestResponse.Result = AutorisationResult;

            return accountsRequestResponse;
        }

        public CardsRequestResponse GetCards(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            CardsRequestResponse cardsRequestResponse = new CardsRequestResponse();
            List<Card> cards;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        cards = proxyClient.GetCards(filter);
                        foreach (Card card in cards)
                        {
                            card.Balance = proxyClient.GetAcccountAvailableBalance(card.CardAccount.AccountNumber);
                        }
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        cards.RemoveAll(m => !HasProductPermission(m.CardAccount.AccountNumber,(ulong)m.ProductId));
                    }


                    cardsRequestResponse.Cards = cards;
                    cardsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    cardsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    cardsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardsRequestResponse.Result.Description = ex.Message;
                }
            }
            else
                cardsRequestResponse.Result = AutorisationResult;

            return cardsRequestResponse;

        }

        public CardRequestResponse GetCard(ulong productId)
        {
            CardRequestResponse cardRequestResponse = new CardRequestResponse();
            Card card;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        card = proxyClient.GetCard(productId);
                        card.Balance = proxyClient.GetAcccountAvailableBalance(card.CardAccount.AccountNumber);
                    }

                    cardRequestResponse.Card = card;
                    cardRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    cardRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    cardRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                cardRequestResponse.Result = AutorisationResult;

            return cardRequestResponse;
        }


        public DepositsRequestResponse GetDeposits(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            DepositsRequestResponse depositsRequestResponse = new DepositsRequestResponse();
            List<Deposit> deposits;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        deposits = proxyClient.GetDeposits(filter);
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        deposits.RemoveAll(m => !HasProductPermission(m.DepositAccount.AccountNumber, (ulong)m.ProductId));
                    }



                    depositsRequestResponse.Deposits = deposits;
                    depositsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    depositsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    depositsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                depositsRequestResponse.Result = AutorisationResult;

            return depositsRequestResponse;
        }


        public DepositRequestResponse GetDeposit(ulong productId)
        {
            DepositRequestResponse depositRequestResponse = new DepositRequestResponse();
            Deposit deposit;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        deposit = proxyClient.GetDeposit(productId);
                    }

                    depositRequestResponse.Deposit = deposit;
                    depositRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    depositRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    depositRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                depositRequestResponse.Result = AutorisationResult;

            return depositRequestResponse;

        }

        public LoansRequestResponse GetLoans(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            LoansRequestResponse loansRequestResponse = new LoansRequestResponse();
            List<Loan> loans;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        loans = proxyClient.GetLoans(filter);
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        loans.RemoveAll(m => !HasProductPermissionByProductID((ulong)m.ProductId));
                    }

                    //Որոշվել է <<Պայմանագիր>> կարգավիճակով վարկերը չցուցադրել
                    loans.RemoveAll(m => m.Quality == 10 && !m.Is_24_7);

                    foreach (Loan loan in loans)
                    {
                        if(loan.ContractDate != null)
                        {
                            loan.StartDate = loan.ContractDate ?? loan.StartDate;
                        }

                        if(loan.Is_24_7)
                        {
                            loan.CurrentCapital = loan.ContractAmount;
                        }
                    }

                    loansRequestResponse.Loans = loans;
                    loansRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loansRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loansRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loansRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loansRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loansRequestResponse.Result = AutorisationResult;

            return loansRequestResponse;
        }

        public LoanRequestResponse GetLoan(ulong productId)
        {
            LoanRequestResponse loanRequestResponse = new LoanRequestResponse();
            Loan loan;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        loan = proxyClient.GetLoan(productId);
                    }
                    if (loan.ContractDate != null)
                    {
                        loan.StartDate = loan.ContractDate ?? loan.StartDate;
                        if (loan.Is_24_7)
                        {
                            loan.CurrentCapital = loan.ContractAmount;
                        }
                    }
                    loanRequestResponse.Loan = loan;
                    loanRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loanRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loanRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loanRequestResponse.Result = AutorisationResult;

            return loanRequestResponse;
        }

        public PeriodicTransfersRequestResponse GetPeriodicTransfers(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            PeriodicTransfersRequestResponse periodicTransfersRequestResponse = new PeriodicTransfersRequestResponse();
            List<PeriodicTransfer> periodicTransfers;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        periodicTransfers = proxyClient.GetPeriodicTransfers(filter);
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        periodicTransfers.RemoveAll(m => !HasProductPermissionByProductID(m.ProductId));
                    }


                    periodicTransfersRequestResponse.PeriodicTransfers = periodicTransfers;
                    periodicTransfersRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    periodicTransfersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    periodicTransfersRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    periodicTransfersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    periodicTransfersRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                periodicTransfersRequestResponse.Result = AutorisationResult;

            return periodicTransfersRequestResponse;
        }

        public PeriodicTransferRequestResponse GetPeriodicTransfer(ulong productId)
        {
            PeriodicTransferRequestResponse periodicTransferRequestResponse = new PeriodicTransferRequestResponse();
            PeriodicTransfer periodicTransfer;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        periodicTransfer = proxyClient.GetPeriodicTransfer(productId);
                    }

                    periodicTransferRequestResponse.PeriodicTransfer = periodicTransfer;
                    periodicTransferRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    periodicTransferRequestResponse.Result.ResultCode = ResultCodes.failed;
                    periodicTransferRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    periodicTransferRequestResponse.Result.ResultCode = ResultCodes.failed;
                    periodicTransferRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                periodicTransferRequestResponse.Result = AutorisationResult;

            return periodicTransferRequestResponse;
        }

        public CardStatementRequestResponse GetCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo)
        {
            CardStatementRequestResponse cardStatementRequestResponse = new CardStatementRequestResponse();
            CardStatement cardStatement;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        cardStatement = proxyClient.GetCardStatement(cardNumber, dateFrom, dateTo, -1, -1, null, 0, 0);
                    }

                    cardStatement?.Transactions?.Sort((x, y) => y.OperationDate.CompareTo(x.OperationDate));
                    cardStatementRequestResponse.CardStatement = cardStatement;
                    cardStatementRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    cardStatementRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardStatementRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    cardStatementRequestResponse.Result.ResultCode = ResultCodes.failed;
                    cardStatementRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                cardStatementRequestResponse.Result = AutorisationResult;


            return cardStatementRequestResponse;
        }

        public AccountStatementRequestResponse GetAccountStatement(string accountNumber, DateTime dateFrom, DateTime dateTo)
        {
            AccountStatementRequestResponse accountStatementRequestResponse = new AccountStatementRequestResponse();
            AccountStatement accountStatement;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accountStatement = proxyClient.AccountStatement(accountNumber, dateFrom, dateTo,-1,-1,null, 0,0);
                    }

                    accountStatement?.Transactions?.Sort((x, y) => y.TransactionDate.CompareTo(x.TransactionDate));
                    accountStatementRequestResponse.AccountStatement = accountStatement;
                    accountStatementRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountStatementRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountStatementRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountStatementRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountStatementRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accountStatementRequestResponse.Result = AutorisationResult;


            return accountStatementRequestResponse;
        }

        public ArcaBalanceRequestResponse GetArcaBalance(string cardNumber)
        {
            ArcaBalanceRequestResponse arcaBalanceRequestResponse = new ArcaBalanceRequestResponse();
            KeyValuePair<string, double> arcaBalance;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        arcaBalance = proxyClient.GetArCaBalance(cardNumber);
                    }

                    arcaBalanceRequestResponse.ArcaBalance = new KeyValuePair<string, double>(arcaBalance.Key, arcaBalance.Value);
                    //arcaBalanceRequestResponse.ArcaBalance.Key = arcaBalance.key;
                    arcaBalanceRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    arcaBalanceRequestResponse.Result.ResultCode = ResultCodes.failed;
                    arcaBalanceRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    arcaBalanceRequestResponse.Result.ResultCode = ResultCodes.failed;
                    arcaBalanceRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                arcaBalanceRequestResponse.Result = AutorisationResult;


            return arcaBalanceRequestResponse;
        }

        public OrdersRequestResponse GetDraftOrders(DateTime dateFrom, DateTime dateTo)
        {
            OrdersRequestResponse ordersRequestResponse = new OrdersRequestResponse();
            List<Order> orders;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        orders = proxyClient.GetDraftOrders(dateFrom, dateTo);
                        orders.RemoveAll(m => m.Source == XBS.SourceType.Bank);
                    }

                    ordersRequestResponse.Orders = orders;
                    ordersRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                ordersRequestResponse.Result = AutorisationResult;


            return ordersRequestResponse;
        }

        public OrdersRequestResponse GetSentOrders(DateTime dateFrom, DateTime dateTo)
        {
            OrdersRequestResponse ordersRequestResponse = new OrdersRequestResponse();
            List<Order> orders;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        orders = proxyClient.GetSentOrders(dateFrom, dateTo);
                        orders.RemoveAll(m => m.Source == XBS.SourceType.Bank);

                    }

                    ordersRequestResponse.Orders = orders;
                    ordersRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                ordersRequestResponse.Result = AutorisationResult;


            return ordersRequestResponse;
        }

        public MessagesRequestResponse GetMessages(DateTime dateFrom, DateTime dateTo, short type)
        {
            MessagesRequestResponse messagesRequestResponse = new MessagesRequestResponse();
            List<Message> messages;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        messages = proxyClient.GetMessages(dateFrom, dateTo, type);
                    }

                    messagesRequestResponse.Messages = messages;
                    messagesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    messagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    messagesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    messagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    messagesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                messagesRequestResponse.Result = AutorisationResult;


            return messagesRequestResponse;
        }

        public MessagesRequestResponse GetNumberOfMessages(short messagesCount, MessageType type)
        {
            MessagesRequestResponse messagesRequestResponse = new MessagesRequestResponse();
            List<Message> messages;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        messages = proxyClient.GetNumberOfMessages(messagesCount, type);
                    }

                    messagesRequestResponse.Messages = messages;
                    messagesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    messagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    messagesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    messagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    messagesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                messagesRequestResponse.Result = AutorisationResult;


            return messagesRequestResponse;
        }

        public Result AddMessage(Message message)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.AddMessage(message);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }

        public Result DeleteMessage(int messageId)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.DeleteMessage(messageId);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }

        public Result MarkMessageReaded(int messageId)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.MarkMessageReaded(messageId);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }

        public SearchCommunalRequestResponse GetCommunals(SearchCommunal searchCommunal)
        {
            SearchCommunalRequestResponse comunalsRequestResponse = new SearchCommunalRequestResponse();
            List<Communal> communals;

            //if (searchCommunal.CommunalType == ExternalBankingRESTService.XBS.CommunalTypes.UCom)
            //searchCommunal.CommunalType = ExternalBankingRESTService.XBS.CommunalTypes.UComWrong;

            Authorize();

            if (searchCommunal.CommunalType == XBS.CommunalTypes.ArmWater)
            {
                comunalsRequestResponse.Result.ResultCode = ResultCodes.failed;
                string errorMessage = Language == 1 ? "Հարգելի հաճախորդ, «ՀայՋրմուղԿոյուղի» ծառայության պարտքը հարկավոր է վճարել «Երևան Ջուր» բաժնում" : "Dear client, please pay for the service «ArmWater service» in the part of «Yerevan Jur»";
                comunalsRequestResponse.Result.Description = errorMessage;
                return comunalsRequestResponse;
            }

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        communals = proxyClient.GetCommunals(searchCommunal, true);
                    }

                    comunalsRequestResponse.Communals = communals;
                    comunalsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    comunalsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    comunalsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    comunalsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    comunalsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                comunalsRequestResponse.Result = AutorisationResult;


            return comunalsRequestResponse;
        }

        public CommunalDetailsRequestResponse GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType = AbonentTypes.physical)
        {
            CommunalDetailsRequestResponse comunalDeatilsRequestResponse = new CommunalDetailsRequestResponse();
            List<CommunalDetails> communalDetails;

            if (communalType == 11)
                communalType = 9;

            Authorize();

            if (abonentType == 0)
                abonentType = AbonentTypes.physical;

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        communalDetails = proxyClient.GetCommunalDetails(communalType, abonentNumber, checkType, branchCode, abonentType);
                    }
                    communalDetails.RemoveAll(m => m.Description == "Operator Name");
                    comunalDeatilsRequestResponse.CommunalDetails = communalDetails;
                    comunalDeatilsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    comunalDeatilsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    comunalDeatilsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    comunalDeatilsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    comunalDeatilsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                comunalDeatilsRequestResponse.Result = AutorisationResult;


            return comunalDeatilsRequestResponse;
        }

        void Authorize()
        {

            byte language = 0;
            string sessionId = "";
            string ipAddress = "";
            Result result = new Result();
            AutorizedCustomer = new XBS.AuthorizedCustomer();
            user.userID = 88;

            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];


            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            ////IP հասցե
            ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();


            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            ////TEST SESSION ID
            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                result.ResultCode = ResultCodes.notAutorized;

                using (XBServiceClient proxyClient = new XBServiceClient())
                {
                    AutorizedCustomer = proxyClient.GetTestMobileBankingUser();
                }

                this.ClientIp = "169.169.169.166";
                this.Language = language;

                if (AutorizedCustomer.LimitedAccess != 0)
                {
                    _userProductPermission = GetUserProductsPermissions();
                }

                result.ResultCode = ResultCodes.normal;

                //result.ResultCode = ResultCodes.failed;
                //result.Description = "Անհրաժեշտ է թարմացնել հավելվածի տարբերակը:";
            }
            else
            {

                if (sessionId != "")
                {
                    bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                    if (EnableSecurityService)
                    {
                        OnlineBankingUser mobileUserData = new OnlineBankingUser();

                        using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                        {
                            mobileUserData = proxyClient.CheckAuthorization(sessionId, language);
                        }

                        if (mobileUserData.AuthorizationResult.IsAuthorized)
                        {
                            AutorizedCustomer.CustomerNumber = ulong.Parse(mobileUserData.CustomerNumber);
                            AutorizedCustomer.UserName = mobileUserData.UserName;
                            AutorizedCustomer.DailyTransactionsLimit = mobileUserData.DailyTransactionsLimit;
                            AutorizedCustomer.OneTransactionLimit = mobileUserData.OneTransactionLimit;
                            this.ClientIp = ipAddress;
                            AutorizedCustomer.ApprovementScheme = short.Parse(mobileUserData.ApprovementScheme.ToString());
                            AutorizedCustomer.LimitedAccess = mobileUserData.LimitedAccess;
                            this.Language = language;
                            if (AutorizedCustomer.LimitedAccess != 0)
                            {
                                _userProductPermission = GetUserProductsPermissions();
                            }
                            result.ResultCode = ResultCodes.normal;
                        }
                        else
                        {
                            result.ResultCode = ResultCodes.notAutorized;
                        }
                    }
                    else
                    {
                        MobileUserData mobileUserData = new MobileUserData();

                        using (MobileLoginClient proxyClient = new MobileLoginClient())
                        {
                            mobileUserData = proxyClient.CheckAuthorization(sessionId, language);
                        }

                        if (mobileUserData.AuthorizationResult.IsAuthorized)
                        {
                            AutorizedCustomer.CustomerNumber = ulong.Parse(mobileUserData.CustomerNumber);
                            AutorizedCustomer.UserName = mobileUserData.UserName;
                            AutorizedCustomer.DailyTransactionsLimit = mobileUserData.DailyTransactionsLimit;
                            AutorizedCustomer.OneTransactionLimit = mobileUserData.OneTransactionLimit;
                            this.ClientIp = ipAddress;
                            AutorizedCustomer.ApprovementScheme = short.Parse(mobileUserData.ApprovementScheme.ToString());
                            AutorizedCustomer.LimitedAccess = mobileUserData.LimitedAccess;
                            this.Language = language;
                            if (AutorizedCustomer.LimitedAccess != 0)
                            {
                                _userProductPermission = GetUserProductsPermissions();
                            }
                            result.ResultCode = ResultCodes.normal;
                        }
                        else
                        {
                            result.ResultCode = ResultCodes.notAutorized;
                        }
                    }
                  
                }
                else
                {
                    result.ResultCode = ResultCodes.notAutorized;
                }
            }

            this.AutorisationResult = result;
            Language = language;
        }


        public void WriteLog(Exception ex)
        {
            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);
            GlobalDiagnosticsContext.Set("Logger", "ExternalBankingRESTService");
            GlobalDiagnosticsContext.Set("StackTrace", ex.StackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());

            if (AutorizedCustomer != null && AutorizedCustomer.UserName != null)
                GlobalDiagnosticsContext.Set("UserName", AutorizedCustomer.UserName);
            else
                GlobalDiagnosticsContext.Set("UserName", "");

            if (ClientIp != null)
                GlobalDiagnosticsContext.Set("ClientIp", ClientIp);
            else
                GlobalDiagnosticsContext.Set("ClientIp", "");

            _logger.Error(ex.Message);

        }

        public ContactRequestResponse GetContact(ulong contactId)
        {
            ContactRequestResponse contactRequestResponse = new ContactRequestResponse();
            Contact contact;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        contact = proxyClient.GetContact(contactId);
                    }

                    contactRequestResponse.Contact = contact;
                    contactRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    contactRequestResponse.Result.ResultCode = ResultCodes.failed;
                    contactRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    contactRequestResponse.Result.ResultCode = ResultCodes.failed;
                    contactRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                contactRequestResponse.Result = AutorisationResult;

            return contactRequestResponse;
        }

        public ContactsRequestResponse GetContacts()
        {
            ContactsRequestResponse contactsRequestResponse = new ContactsRequestResponse();
            List<Contact> contacts;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        contacts = proxyClient.GetContacts();
                    }

                    contactsRequestResponse.Contacts = contacts;
                    contactsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    contactsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    contactsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    contactsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    contactsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                contactsRequestResponse.Result = AutorisationResult;

            return contactsRequestResponse;
        }

        public Result AddContact(Contact contact)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.AddContact(contact);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }

        public Result UpdateContact(Contact contact)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.UpdateContact(contact);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }

        public Result DeleteContact(ulong contactId)
        {
            Result result = new Result();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        proxyClient.DeleteContact(contactId);
                    }

                    result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.ResultCode = ResultCodes.failed;
                    result.Description = "Error!Please try later";
                }
            }
            else
                result = AutorisationResult;


            return result;
        }
        public ActionRequestResponse SavePaymentOrder(PaymentOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    if ((order.ReceiverBankCode == 10300 && order.ReceiverAccount != null && order.ReceiverAccount.AccountNumber?.ToString().Length >= 5 && order.ReceiverAccount.AccountNumber?.ToString()[5] == '9') || order.ReceiverBankCode.ToString()[0] == '9')
                    {
                        result.Result.ResultCode = ResultCodes.validationError;
                        result.Result.Description = "Բյուջե փոխանցում նախատեսված չէ:Գործարքը կատարեք Home Banking համակարգի միջոցով:";
                    }
                    else
                    {
                        using (XBServiceClient proxyClient = new XBServiceClient())
                        {
                            proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                            ActionResult saveResult = proxyClient.SavePaymentOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";

                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }


                            result.Result.Description = actionErrors;
                        }
                    }

                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public PaymentOrderRequestResponse GetPaymentOrder(long id)
        {
            PaymentOrderRequestResponse paymentOrderRequestResponse = new PaymentOrderRequestResponse();
            PaymentOrder paymentOrder;



            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrder = proxyClient.GetPaymentOrder(id);
                    }

                    bool hasPermission = true;
                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        if (!HasProductPermission(paymentOrder.DebitAccount.AccountNumber) || 
                            (paymentOrder.SubType==3 && !HasProductPermission(paymentOrder.ReceiverAccount.AccountNumber))
                            || (paymentOrder.FeeAccount!=null && paymentOrder.FeeAccount.AccountNumber!="0" &&
                            !HasProductPermission(paymentOrder.FeeAccount.AccountNumber)))
                        {
                            hasPermission = false;
                            paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                                paymentOrderRequestResponse.Result.Description = "Տվյալները հասանելի չեն։";
                            else
                                paymentOrderRequestResponse.Result.Description = "Permission denied";
                        }
                    }
                    if (hasPermission)
                    {
                        paymentOrderRequestResponse.PaymentOrder = paymentOrder;
                        paymentOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                paymentOrderRequestResponse.Result = AutorisationResult;

            return paymentOrderRequestResponse;
        }

        public ExchangeRateRequestResponse GetLastExhangeRate(string currency, byte rateType, byte direction)
        {
            ExchangeRateRequestResponse exchangeRateRequestResponse = new ExchangeRateRequestResponse();
            double exchangeRate;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        exchangeRate = proxyClient.GetLastExchangeRate(currency, (RateType)rateType, (ExchangeDirection)direction, 22000);
                    }

                    exchangeRateRequestResponse.ExchangeRate = exchangeRate;
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    exchangeRateRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    exchangeRateRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                exchangeRateRequestResponse.Result = AutorisationResult;

            return exchangeRateRequestResponse;
        }

        public PaymentOrderFeeRequestResponse GetPaymentOrderFee(PaymentOrder order)
        {
            PaymentOrderFeeRequestResponse paymentOrderFeeRequestResponse = new PaymentOrderFeeRequestResponse();
            double paymentOrderFee;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrderFee = proxyClient.GetPaymentOrderFee(order, 0);
                    }

                    paymentOrderFeeRequestResponse.PaymentOrderFee = paymentOrderFee;
                    paymentOrderFeeRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    paymentOrderFeeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderFeeRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    paymentOrderFeeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderFeeRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                paymentOrderFeeRequestResponse.Result = AutorisationResult;

            return paymentOrderFeeRequestResponse;
        }

        public PaymentOrderFutureBalanceRequestResponse GetPaymentOrderFutureBalance(long id)
        {
            PaymentOrderFutureBalanceRequestResponse paymentOrderFutureBalanceRequestResponse = new PaymentOrderFutureBalanceRequestResponse();
            PaymentOrderFutureBalance paymentOrderFutureBalance;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrderFutureBalance = proxyClient.GetPaymentOrderFutureBalanceById(id);
                    }

                    paymentOrderFutureBalanceRequestResponse.FutureBalance = paymentOrderFutureBalance;
                    paymentOrderFutureBalanceRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    paymentOrderFutureBalanceRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderFutureBalanceRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    paymentOrderFutureBalanceRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderFutureBalanceRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                paymentOrderFutureBalanceRequestResponse.Result = AutorisationResult;

            return paymentOrderFutureBalanceRequestResponse;
        }

        public ActionRequestResponse SaveUtilityPaymentOrder(UtilityPaymentOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            if (order.CommunalType == ExternalBankingRESTService.XBS.CommunalTypes.Trash)
                order.CommunalType = ExternalBankingRESTService.XBS.CommunalTypes.UCom;

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        ActionResult saveResult = proxyClient.SaveUtiliyPaymentOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public UtilityPaymentOrderRequestResponse GetUtilityPaymentOrder(long id)
        {
            UtilityPaymentOrderRequestResponse utilityPaymentOrderRequestResponse = new UtilityPaymentOrderRequestResponse();
            UtilityPaymentOrder paymentOrder;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrder = proxyClient.GetUtilityPaymentOrder(id);
                    }

                    bool hasPermission = true;
                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        if (!HasProductPermission(paymentOrder.DebitAccount.AccountNumber))
                        {
                            hasPermission = false;
                            utilityPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                                utilityPaymentOrderRequestResponse.Result.Description = "Տվյալները հասանելի չեն։";
                            else
                                utilityPaymentOrderRequestResponse.Result.Description = "Permission denied";
                        }
                    }
                    if (hasPermission)
                    {
                        utilityPaymentOrderRequestResponse.PaymentOrder = paymentOrder;
                        utilityPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                    }
                    
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    utilityPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    utilityPaymentOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    utilityPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    utilityPaymentOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                utilityPaymentOrderRequestResponse.Result = AutorisationResult;

            return utilityPaymentOrderRequestResponse;
        }
        public AccountsRequestResponse GetAccountsForOrder(short orderType, byte orderSubType, byte accountType)
        {
            AccountsRequestResponse accountsRequestResponse = new AccountsRequestResponse();
            List<Account> accounts;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accounts = proxyClient.GetAccountsForOrder(orderType, orderSubType, accountType);
                        foreach (Account account in accounts)
                        {
                            account.Balance = proxyClient.GetAcccountAvailableBalance(account.AccountNumber);
                        }
                        //Դեբետ հաշվի դեպքում ջնջում ենք ապառիկի հաշիվները
                        if (accountType == 1)
                        {
                            accounts.RemoveAll(m => m.AccountType == 54 || m.AccountType == 58);
                        }
                        else if (accountType == 2)
                        {
                            if (accounts.Exists(m => m.AccountType == 54 || m.AccountType == 58))
                            {
                                List<Loan> aparikTexumLoans = proxyClient.GetAparikTexumLoans();
                                foreach (Account account in accounts.FindAll(m => m.AccountType == 54 || m.AccountType == 58))
                                {
                                    if (!aparikTexumLoans.Exists(m => m.ConnectAccount.AccountNumber == account.AccountNumber))
                                    {
                                        accounts.RemoveAll(m => m.AccountNumber == account.AccountNumber);
                                    }
                                }

                            }
                        }



                        accounts.ForEach(m =>
                        {
                            if (m.AccountType == 11) // Hetagayum erb poxvi mobile-um hanel
                            {
                                m.AccountDescription = (m.AccountDescription.Length <= 16 ? m.AccountDescription : m.AccountDescription.Substring(0, 16)).Trim();
                            }
                            else
                            {
                                m.AccountDescription = "";
                            }
                        });
                    }

                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        accounts.RemoveAll(m => !HasProductPermission(m.AccountNumber));
                    }

                    
                    accountsRequestResponse.Accounts = accounts;
                    accountsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accountsRequestResponse.Result = AutorisationResult;

            return accountsRequestResponse;
        }


        private bool CheckSign(PaymentOrder order)
        {

            bool isSigned = false;
            string sessionId = "";
            byte language = 0;

            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];


            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {

                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.Amount).ToString());

                string debitAccount = order.DebitAccount.AccountNumber.ToString();
                signData.Add(key: "SenderAccountFirstPart", value: debitAccount.Substring(0, 10));
                signData.Add(key: "SenderAccountSecondPart", value: debitAccount.Substring(10, 5));

                string creditAccount = "";
                creditAccount = order.ReceiverAccount.AccountNumber.ToString();

                signData.Add(key: "RecepientAccountFirstPart", value: creditAccount.Substring(0, 10));
                signData.Add(key: "RecepientAccountSecondPart", value: creditAccount.Substring(10, creditAccount.Length - 10));

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }           
            }
            return isSigned;

        }

        private bool CheckSign(UtilityPaymentOrder order)
        {
            bool isSigned = false;
            string sessionId = "";
            byte language = 0;


            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {
                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.Amount).ToString());

                string debitAccount = order.DebitAccount.AccountNumber.ToString();
                signData.Add(key: "SenderAccountFirstPart", value: debitAccount.Substring(0, 10));
                signData.Add(key: "SenderAccountSecondPart", value: debitAccount.Substring(10, 5));

                signData.Add(key: "RecepientAccountFirstPart", value: "0");
                signData.Add(key: "RecepientAccountSecondPart", value: "0");

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }    
            }
            return isSigned;
        }
        private bool CheckSign(MatureOrder order)
        {
            bool isSigned = false;
            string sessionId = "";
            byte language = 0;


            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {
                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.Amount).ToString());

                string debitAccount = order.Account.AccountNumber.ToString();
                signData.Add(key: "SenderAccountFirstPart", value: debitAccount.Substring(0, 10));
                signData.Add(key: "SenderAccountSecondPart", value: debitAccount.Substring(10, 5));

                signData.Add(key: "RecepientAccountFirstPart", value: "0");
                signData.Add(key: "RecepientAccountSecondPart", value: "0");

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];


                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }

            }
            return isSigned;
        }

        public ActionRequestResponse ApprovePaymentOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        PaymentOrder order = proxyClient.GetPaymentOrder(id);

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApprovePaymentOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }
                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }

                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public ActionRequestResponse ApproveUtilityPaymentOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        UtilityPaymentOrder order = proxyClient.GetUtilityPaymentOrder(id);

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApproveUtilityPaymentOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }

                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }
                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public ActionRequestResponse DeletePaymentOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        Order order = proxyClient.GetOrder(id);
                        ActionResult deleteResult = proxyClient.DeleteOrder(order);

                        result.Result.ResultCode = (ResultCodes)((short)deleteResult.ResultCode);
                        result.Id = deleteResult.Id;
                        string actionErrors = "";
                        if (deleteResult.Errors != null && deleteResult.Errors.Count > 0)
                        {
                            deleteResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public MessagesCountResponceRequest GetUnreadedMessagesCount()
        {
            MessagesCountResponceRequest result = new MessagesCountResponceRequest();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        int count = proxyClient.GetUnreadedMessagesCount();

                        result.MessagesCount = count;
                        result.Result.ResultCode = ResultCodes.normal;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public MessagesCountResponceRequest GetUnreadMessagesCountByType(MessageType type)
        {
            MessagesCountResponceRequest result = new MessagesCountResponceRequest();
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        int count = proxyClient.GetUnreadMessagesCountByType(type);

                        result.MessagesCount = count;
                        result.Result.ResultCode = ResultCodes.normal;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public Result ChangeMobileUserPassword(string password, string newPassword, string retypeNewPassword)
        {
            Result result = new Result();

            try
            {
                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    OnlineBankingSecServRef.ChangePasswordInfo changePasswordInfo = new OnlineBankingSecServRef.ChangePasswordInfo();
                    changePasswordInfo.Password = password;
                    changePasswordInfo.NewPassword = newPassword;
                    changePasswordInfo.RetypePassword = retypeNewPassword;
                    OnlineBankingUser mobileUserData = new OnlineBankingUser();

                    string sessionId = "";
                    byte language = 0;

                    ///Սեսիայի նունականացման համար
                    if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                        sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

                    ///IP հասցե
                    changePasswordInfo.IpAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                    ///Լեզու
                    if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                        byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        mobileUserData = proxyClient.ChangeUserPassword(changePasswordInfo, sessionId, language);
                    }

                    ///Եթե անցել է նույնականացում
                    if (mobileUserData.AuthorizationResult.IsAuthorized)
                    {
                        if (mobileUserData.PasswordChangeResult.IsChanged)
                        {
                            result.ResultCode = ResultCodes.normal;
                        }
                        else
                        {
                            result.ResultCode = ResultCodes.failed;
                            result.Description = mobileUserData.PasswordChangeResult.Description;
                        }
                    }
                    else
                    {
                        result.ResultCode = ResultCodes.failed;
                        result.Description = mobileUserData.AuthorizationResult.Description;
                    }
                }
                else
                {
                    MAuthorizationServiceReference.ChangePasswordInfo changePasswordInfo = new MAuthorizationServiceReference.ChangePasswordInfo();
                    changePasswordInfo.Password = password;
                    changePasswordInfo.NewPassword = newPassword;
                    changePasswordInfo.RetypePassword = retypeNewPassword;
                    MobileUserData mobileUserData = new MobileUserData();

                    string sessionId = "";
                    byte language = 0;

                    ///Սեսիայի նունականացման համար
                    if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                        sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

                    ///IP հասցե
                    changePasswordInfo.IpAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                    ///Լեզու
                    if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                        byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        mobileUserData = proxyClient.ChangeUserPassword(changePasswordInfo, sessionId, language);
                    }

                    ///Եթե անցել է նույնականացում
                    if (mobileUserData.AuthorizationResult.IsAuthorized)
                    {
                        if (mobileUserData.PasswordChangeResult.IsChanged)
                        {
                            result.ResultCode = ResultCodes.normal;
                        }
                        else
                        {
                            result.ResultCode = ResultCodes.failed;
                            result.Description = mobileUserData.PasswordChangeResult.Description;
                        }
                    }
                    else
                    {
                        result.ResultCode = ResultCodes.failed;
                        result.Description = mobileUserData.AuthorizationResult.Description;
                    }
                }            

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                result.ResultCode = ResultCodes.failed;
                result.Description = Resource.InternalError;
            }

            return result;
        }

        public Result ValidateOTP()
        {
            Result otpValidationResult = new Result();

            try
            {
                string sessionId = "";
                byte language = 0;
                string ipAddress = "";
                string OTP = "";
                bool isValid = false;

                ///Սեսիայի նունականացման համար
                if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                    sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

                ///IP հասցե
                ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                ///Լեզու
                if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                    byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);
                
                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isValid = proxyClient.VerifyToken(sessionId, OTP, ipAddress, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isValid = proxyClient.VerifyToken(sessionId, OTP, ipAddress, language);
                    }
                }

                ///Եթե անցել է նույնականացում
                if (isValid)
                {
                    otpValidationResult.ResultCode = ResultCodes.normal;
                }
                else
                {
                    otpValidationResult.ResultCode = ResultCodes.failed;
                    otpValidationResult.Description = "";
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                otpValidationResult.ResultCode = ResultCodes.failed;
                otpValidationResult.Description = Resource.InternalError;
            }

            return otpValidationResult;
        }

        public ValidateRegistrationCodeRequestResponse ValidateRegistrationCode()
        {
            ValidateRegistrationCodeRequestResponse validationResult = new ValidateRegistrationCodeRequestResponse();

            try
            {

                String registrationCode = "";

                ///Լեզու
                if (WebOperationContext.Current.IncomingRequest.Headers["RegistrationCode"] != null)
                    registrationCode = WebOperationContext.Current.IncomingRequest.Headers["RegistrationCode"].ToString();

                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {

                        validationResult.UserName = proxyClient.CheckRegistrationCode(registrationCode);
                        validationResult.Result.ResultCode = ResultCodes.normal;
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        validationResult.UserName = proxyClient.CheckRegistrationCode(registrationCode);
                        validationResult.Result.ResultCode = ResultCodes.normal;
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                validationResult.Result.ResultCode = ResultCodes.failed;
                validationResult.Result.Description = Resource.InternalError;
            }

            return validationResult;
        }


        public LoanRepaymentResponse GetLoanRepaymentGrafik(ulong productId)
        {
            LoanRepaymentResponse loanRepaymentResponse = new LoanRepaymentResponse();
            Loan loan;
            List<LoanRepaymentGrafik> loanRepaymentGrafik;

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        loan = proxyClient.GetLoan(productId);
                        loanRepaymentGrafik = proxyClient.GetLoanGrafik(loan);
                    }

                    loanRepaymentResponse.LoanRepaymentGrafik = loanRepaymentGrafik;
                    loanRepaymentResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loanRepaymentResponse.Result.ResultCode = ResultCodes.failed;
                    loanRepaymentResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loanRepaymentResponse.Result.ResultCode = ResultCodes.failed;
                    loanRepaymentResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loanRepaymentResponse.Result = AutorisationResult;

            return loanRepaymentResponse;
        }

        public DepositRepaymentResponse GetDepositRepaymentGrafik(ulong productId)
        {
            DepositRepaymentResponse depositRepaymentResponse = new DepositRepaymentResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);

                        List<DepositRepayment> depositRepaymentGrafik = proxyClient.GetDepositRepayments(productId);
                        depositRepaymentResponse.DepositRepaymentGrafik = depositRepaymentGrafik;
                    }

                  
                    depositRepaymentResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    depositRepaymentResponse.Result.ResultCode = ResultCodes.failed;
                    depositRepaymentResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    depositRepaymentResponse.Result.ResultCode = ResultCodes.failed;
                    depositRepaymentResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                depositRepaymentResponse.Result = AutorisationResult;

            return depositRepaymentResponse;
        }
        public ActionRequestResponse SaveReferenceOrder(ReferenceOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);

                        double referenceOrderFee = proxyClient.GetOrderServiceFee(XBS.OrderType.ReferenceOrder, Convert.ToInt32(order.Urgent));
                        order.Fees = new List<OrderFee>();
                        OrderFee orderFee = new OrderFee();
                        orderFee.Amount = referenceOrderFee;
                        orderFee.Type = 15;
                        orderFee.Account = order.FeeAccount;
                        orderFee.Currency = "AMD";
                        order.Fees.Add(orderFee);

                        order.ReferenceTypes = new List<ushort>();
                        order.ReferenceTypes.Add(order.ReferenceType);

                        if (order.Urgent == 1)
                        {
                            order.ReceiveDate = order.RegistrationDate;
                        }

                        ActionResult saveResult = proxyClient.SaveReferenceOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }
        public ActionRequestResponse SaveMatureOrder(MatureOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        Loan loan = proxyClient.GetLoan(order.ProductId);
                        order.ProductAccount = loan.LoanAccount;
                        order.ProductCurrency = loan.Currency;
                        order.DayOfProductRateCalculation = loan.DayOfRateCalculation.Value;
                        if (order.Account != null && order.Account.AccountNumber == "0")
                        {
                            order.Account = null;
                        }
                        ActionResult saveResult = proxyClient.SaveMatureOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public ActionRequestResponse ApproveMatureOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        MatureOrder order = proxyClient.GetMatureOrder(id);

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApproveMatureOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }

                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }
                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public MatureOrderRequestResponse GetMatureOrder(long id)
        {
            MatureOrderRequestResponse MatureOrderResponse = new MatureOrderRequestResponse();
            MatureOrder MatureOrder;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        MatureOrder = proxyClient.GetMatureOrder(id);
                    }
                    bool hasPermission = true;
                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        if (!HasProductPermission(MatureOrder.Account.AccountNumber))
                        {
                            hasPermission = false;
                            MatureOrderResponse.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                                MatureOrderResponse.Result.Description = "Տվյալները հասանելի չեն։";
                            else
                                MatureOrderResponse.Result.Description = "Permission denied";
                        }
                    }
                    if (hasPermission)
                    {
                        MatureOrderResponse.MatureOrder = MatureOrder;
                        MatureOrderResponse.Result.ResultCode = ResultCodes.normal;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    MatureOrderResponse.Result.ResultCode = ResultCodes.failed;
                    MatureOrderResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    MatureOrderResponse.Result.ResultCode = ResultCodes.failed;
                    MatureOrderResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                MatureOrderResponse.Result = AutorisationResult;

            return MatureOrderResponse;

        }

        public ActionRequestResponse ApproveReferenceOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        ReferenceOrder order = proxyClient.GetReferenceOrder(id);

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApproveReferenceOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }
                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }
                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }
        public ReferenceOrderRequestResponse GetReferenceOrder(long id)
        {
            ReferenceOrderRequestResponse referenceOrderRequestResponse = new ReferenceOrderRequestResponse();
            ReferenceOrder referenceOrder;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        referenceOrder = proxyClient.GetReferenceOrder(id);
                    }

                    referenceOrderRequestResponse.ReferenceOrder = referenceOrder;
                    referenceOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    referenceOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    referenceOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    referenceOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    referenceOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                referenceOrderRequestResponse.Result = AutorisationResult;

            return referenceOrderRequestResponse;
        }
        private bool CheckSign(ReferenceOrder order)
        {
            bool isSigned = false;
            string sessionId = "";
            byte language = 0;


            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {
                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.FeeAmount).ToString());

                string debitAccount = "";
                if (order.FeeAccount != null)
                {
                    debitAccount = order.FeeAccount.AccountNumber.ToString();
                    signData.Add(key: "SenderAccountFirstPart", value: debitAccount.Substring(0, 10));
                    signData.Add(key: "SenderAccountSecondPart", value: debitAccount.Substring(10, 5));
                }
                else
                {
                    signData.Add(key: "SenderAccountFirstPart", value: "0");
                    signData.Add(key: "SenderAccountSecondPart", value: "0");
                }

                signData.Add(key: "RecepientAccountFirstPart", value: "0");
                signData.Add(key: "RecepientAccountSecondPart", value: "0");

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];

                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }

            }
            return isSigned;
        }

        public InfoListRequestResponse GetEmbassyList()
        {
            InfoListRequestResponse embassyListRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> embassyList;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        embassyList = proxyClient.GetEmbassyList(new List<ushort> { 1 });
                    }

                    embassyListRequestResponse.InfoList = embassyList;
                    embassyListRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    embassyListRequestResponse.Result.ResultCode = ResultCodes.failed;
                    embassyListRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    embassyListRequestResponse.Result.ResultCode = ResultCodes.failed;
                    embassyListRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                embassyListRequestResponse.Result = AutorisationResult;

            return embassyListRequestResponse;
        }

        public InfoListRequestResponse GetFilialList()
        {
            InfoListRequestResponse filialListRequestResponse = new InfoListRequestResponse();
            List<KeyValuePair<long, String>> filialList;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        filialList = proxyClient.GetFilialAddressList();
                    }

                    Dictionary<string, string> filialsDict = new Dictionary<string, string>();

                    filialList.ForEach(m =>
                    {
                        filialsDict.Add(m.Key.ToString(), m.Value);
                    });

                    filialListRequestResponse.InfoList = filialsDict;


                    filialListRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    filialListRequestResponse.Result.ResultCode = ResultCodes.failed;
                    filialListRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    filialListRequestResponse.Result.ResultCode = ResultCodes.failed;
                    filialListRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                filialListRequestResponse.Result = AutorisationResult;

            return filialListRequestResponse;
        }

        public InfoListRequestResponse GetReferenceLanguages()
        {
            InfoListRequestResponse languagesRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> languages;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        languages = proxyClient.GetReferenceLanguages();
                    }

                    languagesRequestResponse.InfoList = languages;
                    languagesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    languagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    languagesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    languagesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    languagesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                languagesRequestResponse.Result = AutorisationResult;

            return languagesRequestResponse;
        }

        public InfoListRequestResponse GetReferenceTypes()
        {
            InfoListRequestResponse referenceTypesRequestResponse = new InfoListRequestResponse();
            List<KeyValuePair<long, string>> referenceTypes = new List<KeyValuePair<long, string>>();


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        referenceTypes = proxyClient.GetReferenceTypes();
                    }

                    Dictionary<string, string> referenceTypesDict = new Dictionary<string, string>();
                    referenceTypes.ForEach(m =>
                    {
                        if (m.Key != 8 && m.Key != 9)
                        {
                            referenceTypesDict.Add(m.Key.ToString(), m.Value);
                        }

                    });

                    referenceTypesRequestResponse.InfoList = referenceTypesDict;
                    referenceTypesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    referenceTypesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    referenceTypesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    referenceTypesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    referenceTypesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                referenceTypesRequestResponse.Result = AutorisationResult;

            return referenceTypesRequestResponse;
        }

        public PaymentOrderFeeRequestResponse GetReferenceOrderFee(bool UrgentSign)
        {
            PaymentOrderFeeRequestResponse referenceOrderFeeRequestResponse = new PaymentOrderFeeRequestResponse();
            double referenceOrderFee;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                using (XBServiceClient proxyClient = new XBServiceClient())
                {
                    proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                    referenceOrderFee = proxyClient.GetOrderServiceFee(XBS.OrderType.ReferenceOrder, Convert.ToInt32(UrgentSign));
                }

                referenceOrderFeeRequestResponse.Result.ResultCode = ResultCodes.normal;
                referenceOrderFeeRequestResponse.PaymentOrderFee = referenceOrderFee;
            }
            else
                referenceOrderFeeRequestResponse.Result = AutorisationResult;

            return referenceOrderFeeRequestResponse;
        }

        public MembershipRewardsRequestResponse GetCardMembershipRewards(string cardNumber)
        {
            MembershipRewardsRequestResponse mrRequestResponse = new MembershipRewardsRequestResponse();
            MembershipRewards mr = new MembershipRewards();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        mr = proxyClient.GetCardMembershipRewards(cardNumber);
                    }

                    mrRequestResponse.MembershipRewards = mr;
                    mrRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    mrRequestResponse.Result.ResultCode = ResultCodes.failed;
                    mrRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    mrRequestResponse.Result.ResultCode = ResultCodes.failed;
                    mrRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                mrRequestResponse.Result = AutorisationResult;

            return mrRequestResponse;
        }

        public GuaranteeRequestResponse GetGuarantee(ulong productId)
        {
            GuaranteeRequestResponse guaranteeRequestResponse = new GuaranteeRequestResponse();
            Guarantee guarantee;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        guarantee = proxyClient.GetGuarantee(productId);
                    }

                    guaranteeRequestResponse.Guarantee = guarantee;
                    guaranteeRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    guaranteeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    guaranteeRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    guaranteeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    guaranteeRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                guaranteeRequestResponse.Result = AutorisationResult;

            return guaranteeRequestResponse;
        }


        public GuaranteesRequestResponse GetGuarantees(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            GuaranteesRequestResponse guaranteesRequestResponse = new GuaranteesRequestResponse();
            List<Guarantee> guarantees;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        guarantees = proxyClient.GetGuarantees(filter);
                    }

                    guaranteesRequestResponse.Guarantees = guarantees;
                    guaranteesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    guaranteesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    guaranteesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    guaranteesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    guaranteesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                guaranteesRequestResponse.Result = AutorisationResult;

            return guaranteesRequestResponse;
        }


        public AccreditiveRequestResponse GetAccreditive(ulong productId)
        {
            AccreditiveRequestResponse accreditiveRequestResponse = new AccreditiveRequestResponse();
            Accreditive accreditive;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accreditive = proxyClient.GetAccreditive(productId);
                    }

                    accreditiveRequestResponse.Accreditive = accreditive;
                    accreditiveRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accreditiveRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accreditiveRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accreditiveRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accreditiveRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accreditiveRequestResponse.Result = AutorisationResult;

            return accreditiveRequestResponse;
        }


        public PeriodicTransferHistoryRequestResponse GetPeriodicTransferHistory(long productId, DateTime dateFrom, DateTime dateTo)
        {
            PeriodicTransferHistoryRequestResponse transferHistoryRequestResponse = new PeriodicTransferHistoryRequestResponse();
            List<PeriodicTransferHistory> periodicTransferHistory;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        periodicTransferHistory = proxyClient.GetPeriodicTransferHistory(productId, dateFrom, dateTo);
                    }

                    transferHistoryRequestResponse.PeriodicTransferHistory = periodicTransferHistory;
                    transferHistoryRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    transferHistoryRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferHistoryRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    transferHistoryRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferHistoryRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                transferHistoryRequestResponse.Result = AutorisationResult;

            return transferHistoryRequestResponse;
        }



        public AccreditivesRequestResponse GetAccreditives(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            AccreditivesRequestResponse accreditivesRequestResponse = new AccreditivesRequestResponse();
            List<Accreditive> accreditives;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accreditives = proxyClient.GetAccreditives(filter);
                    }

                    accreditivesRequestResponse.Accreditives = accreditives;
                    accreditivesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accreditivesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accreditivesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accreditivesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accreditivesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accreditivesRequestResponse.Result = AutorisationResult;

            return accreditivesRequestResponse;
        }

        public DepositCaseRequestResponse GetDepositCase(ulong productId)
        {
            DepositCaseRequestResponse depositCaseRequestResponse = new DepositCaseRequestResponse();
            DepositCase depositCase;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        depositCase = proxyClient.GetDepositCase(productId);
                    }

                    depositCaseRequestResponse.DepositCase = depositCase;
                    depositCaseRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    depositCaseRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositCaseRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    depositCaseRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositCaseRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                depositCaseRequestResponse.Result = AutorisationResult;

            return depositCaseRequestResponse;
        }


        public DepositCasesRequestResponse GetDepositCases(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            DepositCasesRequestResponse depositCasesRequestResponse = new DepositCasesRequestResponse();
            List<DepositCase> depositCases;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        depositCases = proxyClient.GetDepositCases(filter);
                    }

                    depositCasesRequestResponse.DepositCases = depositCases;
                    depositCasesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    depositCasesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositCasesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    depositCasesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    depositCasesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                depositCasesRequestResponse.Result = AutorisationResult;

            return depositCasesRequestResponse;
        }



        public FactoringRequestResponse GetFactoring(ulong productId)
        {
            FactoringRequestResponse factoringRequestResponse = new FactoringRequestResponse();
            Factoring factoring;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        factoring = proxyClient.GetFactoring(productId);
                    }

                    factoringRequestResponse.Factoring = factoring;
                    factoringRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    factoringRequestResponse.Result.ResultCode = ResultCodes.failed;
                    factoringRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    factoringRequestResponse.Result.ResultCode = ResultCodes.failed;
                    factoringRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                factoringRequestResponse.Result = AutorisationResult;

            return factoringRequestResponse;
        }


        public FactoringsRequestResponse GetFactorings(ProductQualityFilter filter = ProductQualityFilter.Opened)
        {
            FactoringsRequestResponse factoringsRequestResponse = new FactoringsRequestResponse();
            List<Factoring> factorings;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        factorings = proxyClient.GetFactorings(filter);
                    }

                    factoringsRequestResponse.Factorings = factorings;
                    factoringsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    factoringsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    factoringsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    factoringsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    factoringsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                factoringsRequestResponse.Result = AutorisationResult;

            return factoringsRequestResponse;
        }

        public ExchangeRateRequestResponse GetCBKursForDate(string currency, DateTime date)
        {
            ExchangeRateRequestResponse exchangeRateRequestResponse = new ExchangeRateRequestResponse();
            double exchangeRate;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        exchangeRate = proxyClient.GetCBKursForDate(date, currency);
                    }

                    exchangeRateRequestResponse.ExchangeRate = exchangeRate;
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    exchangeRateRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    exchangeRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    exchangeRateRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                exchangeRateRequestResponse.Result = AutorisationResult;

            return exchangeRateRequestResponse;
        }

        public Result CheckAuthorization()
        {
            Authorize();
            return AutorisationResult;
        }


        public LoanMatureCapitalPenaltyRequestResponse GetLoanMatureCapitalPenalty(MatureOrder order)
        {
            LoanMatureCapitalPenaltyRequestResponse loanMatureCapitalPenaltyRequestResponse = new LoanMatureCapitalPenaltyRequestResponse();
            double loanMature;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);

                        if (order.Account != null && order.Account.AccountNumber != "")
                        {
                            order.Account = proxyClient.GetAccount(order.Account.AccountNumber);
                        }

                        if (order.PercentAccount != null && order.PercentAccount.AccountNumber != "")
                        {
                            order.PercentAccount = proxyClient.GetAccount(order.PercentAccount.AccountNumber);
                        }

                        loanMature = proxyClient.GetLoanMatureCapitalPenalty(order, user);
                    }

                    loanMatureCapitalPenaltyRequestResponse.LoanMatureCapitalPenalty = loanMature;
                    loanMatureCapitalPenaltyRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loanMatureCapitalPenaltyRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanMatureCapitalPenaltyRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loanMatureCapitalPenaltyRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanMatureCapitalPenaltyRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loanMatureCapitalPenaltyRequestResponse.Result = AutorisationResult;

            return loanMatureCapitalPenaltyRequestResponse;
        }
        public ActionRequestResponse SaveAndApproveRemovalOrder(RemovalOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();


            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        order.Quality = OrderQuality.Draft;
                        ActionResult saveResult = proxyClient.SaveAndApproveRemovalOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }
        public RemovalOrderRequestResponse GetRemovalOrder(long id)
        {
            RemovalOrderRequestResponse removalOrderRequestResponse = new RemovalOrderRequestResponse();
            RemovalOrder order;



            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        order = proxyClient.GetRemovalOrder(id);
                    }

                    removalOrderRequestResponse.RemovalOrder = order;
                    removalOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    removalOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    removalOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    removalOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    removalOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                removalOrderRequestResponse.Result = AutorisationResult;

            return removalOrderRequestResponse;
        }
        public VehicleViolationRequestRespone GetVehicleViolationById(string violationId)
        {
            VehicleViolationRequestRespone vehicleViolationRequestRespone = new VehicleViolationRequestRespone();
            List<VehicleViolationResponse> vehicleViolations = new List<VehicleViolationResponse>();
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        vehicleViolations = proxyClient.GetVehicleViolationById(violationId);
                    }

                    ///TO DO ....TEXEKACNEL MOBILE DEVELOPMENTIN,VOR ID-n SXAL EN VERCNUM,UXXELUC HETO HANEL
                    vehicleViolations.ForEach(m =>
                    {
                        m.ResponseId = m.Id;
                    });

                    vehicleViolationRequestRespone.VehicleViolations = vehicleViolations;
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.failed;
                    vehicleViolationRequestRespone.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.failed;
                    vehicleViolationRequestRespone.Result.Description = "Error!Please try later";
                }
            }
            else
                vehicleViolationRequestRespone.Result = AutorisationResult;

            return vehicleViolationRequestRespone;
        }
        public VehicleViolationRequestRespone GetVehicleViolationByPsnVehNum(string psn, string vehNum)
        {
            VehicleViolationRequestRespone vehicleViolationRequestRespone = new VehicleViolationRequestRespone();

            List<VehicleViolationResponse> vehicleViolations = new List<VehicleViolationResponse>();
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        vehicleViolations = proxyClient.GetVehicleViolationByPsnVehNum(psn, vehNum);
                    }

                    ///TO DO... TEXEKACNEL MOBILE DEVELOPMENTIN,VOR ID-n SXAL EN VERCNUM,UXXELUC HETO HANEL
                    vehicleViolations.ForEach(m =>
                    {
                        m.ResponseId = m.Id;
                    });

                    vehicleViolationRequestRespone.VehicleViolations = vehicleViolations;
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.failed;
                    vehicleViolationRequestRespone.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    vehicleViolationRequestRespone.Result.ResultCode = ResultCodes.failed;
                    vehicleViolationRequestRespone.Result.Description = "Error!Please try later";
                }
            }
            else
                vehicleViolationRequestRespone.Result = AutorisationResult;

            return vehicleViolationRequestRespone;
        }
        public OrdersRequestResponse GetApproveReqOrder(DateTime dateFrom, DateTime dateTo)
        {
            OrdersRequestResponse ordersRequestResponse = new OrdersRequestResponse();
            List<Order> orders;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        orders = proxyClient.GetApproveReqOrder(dateFrom, dateTo);

                    }

                    ordersRequestResponse.Orders = orders;
                    ordersRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                ordersRequestResponse.Result = AutorisationResult;
            return ordersRequestResponse;
        }

        public ActionRequestResponse SaveBudgetPaymentOrder(BudgetPaymentOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        ActionResult saveResult = proxyClient.SaveBudgetPaymentOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";

                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }


                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public BudgetPaymentOrderRequestResponse GetBudgetPaymentOrder(long id)
        {
            BudgetPaymentOrderRequestResponse paymentOrderRequestResponse = new BudgetPaymentOrderRequestResponse();
            BudgetPaymentOrder paymentOrder;



            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrder = proxyClient.GetBudgetPaymentOrder(id);
                    }

                    bool hasPermission = true;
                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        if (!HasProductPermission(paymentOrder.DebitAccount.AccountNumber) 
                            || (paymentOrder.FeeAccount != null && paymentOrder.FeeAccount.AccountNumber != "0" &&
                            !HasProductPermission(paymentOrder.FeeAccount.AccountNumber)))
                        {
                            hasPermission = false;
                            paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                                paymentOrderRequestResponse.Result.Description = "Տվյալները հասանելի չեն։";
                            else
                                paymentOrderRequestResponse.Result.Description = "Permission denied";
                        }
                    }
                    if (hasPermission)
                    {
                        paymentOrderRequestResponse.PaymentOrder = paymentOrder;
                        paymentOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                    }

                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                paymentOrderRequestResponse.Result = AutorisationResult;

            return paymentOrderRequestResponse;
        }


        public CreditLineRequestResponse GetCreditLine(ulong productId)
        {
            CreditLineRequestResponse creditLineRequestResponse = new CreditLineRequestResponse();
            CreditLine creditLine;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        creditLine = proxyClient.GetCreditLine(productId);
                    }

                    creditLineRequestResponse.CreditLine = creditLine;
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.failed;
                    creditLineRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.failed;
                    creditLineRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                creditLineRequestResponse.Result = AutorisationResult;

            return creditLineRequestResponse;
        }

        public CreditLineGrafikRequestResponse GetCreditLineGrafik(ulong productId)
        {
            CreditLineGrafikRequestResponse creditLineRequestResponse = new CreditLineGrafikRequestResponse();
            List<CreditLineGrafik> creditLineGrafik;
            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        creditLineGrafik = proxyClient.GetCreditLineGrafik(productId);
                    }

                    creditLineRequestResponse.CreditLineGrafik = creditLineGrafik;
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.failed;
                    creditLineRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    creditLineRequestResponse.Result.ResultCode = ResultCodes.failed;
                    creditLineRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                creditLineRequestResponse.Result = AutorisationResult;

            return creditLineRequestResponse;
        }

        private bool CheckSign(LoanProductOrder order)
        {
            bool isSigned = false;
            string sessionId = "";
            byte language = 0;
            
            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];

            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {
                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.AmountInAMD).ToString());

                signData.Add(key: "SenderAccountFirstPart", value: "0");
                signData.Add(key: "SenderAccountSecondPart", value: "0");

                signData.Add(key: "RecepientAccountFirstPart", value: "0");
                signData.Add(key: "RecepientAccountSecondPart", value: "0");

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];
                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }

            }
            return isSigned;
        }

        public ActionRequestResponse SaveLoanProductOrder(LoanProductOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {

                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        ActionResult saveResult = proxyClient.SaveLoanProductOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public ActionRequestResponse ApproveLoanProductOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        LoanProductOrder order = proxyClient.GetCreditLineOrder(id);

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApproveLoanProductOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }
                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }

                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public LoanProductOrderRequestResponse GetCreditLineOrder(long id)
        {
            LoanProductOrderRequestResponse loanProductOrderRequestResponse = new LoanProductOrderRequestResponse();
            LoanProductOrder loanProductOrder;

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        loanProductOrder = proxyClient.GetCreditLineOrder(id);
                    }
                    loanProductOrder.FirstRepaymentDate = loanProductOrder.EndDate;
                    loanProductOrderRequestResponse.LoanProductOrder = loanProductOrder;
                    loanProductOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loanProductOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanProductOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loanProductOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanProductOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loanProductOrderRequestResponse.Result = AutorisationResult;

            return loanProductOrderRequestResponse;
        }

        public LoanProductInterestRateRequestResponse GetLoanProductInterestRate(LoanProductOrder order, string cardNumber)
        {
            LoanProductInterestRateRequestResponse loanProductInterestRateRequestResponse = new LoanProductInterestRateRequestResponse();
            double loanProductInterestRate;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);



                        loanProductInterestRate = proxyClient.GetLoanProductInterestRate(order, cardNumber);
                    }

                    loanProductInterestRateRequestResponse.LoanProductInterestRate = loanProductInterestRate;
                    loanProductInterestRateRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    loanProductInterestRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanProductInterestRateRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    loanProductInterestRateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    loanProductInterestRateRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                loanProductInterestRateRequestResponse.Result = AutorisationResult;

            return loanProductInterestRateRequestResponse;
        }

        public InfoListRequestResponse GetListOfLoanApplicationAmounts()
        {
            InfoListRequestResponse listOfLoanApplicationAmountsRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> listOfLoanApplicationAmounts;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        listOfLoanApplicationAmounts = proxyClient.GetListOfLoanApplicationAmounts();
                    }

                    listOfLoanApplicationAmountsRequestResponse.InfoList = listOfLoanApplicationAmounts;
                    listOfLoanApplicationAmountsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    listOfLoanApplicationAmountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    listOfLoanApplicationAmountsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    listOfLoanApplicationAmountsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    listOfLoanApplicationAmountsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                listOfLoanApplicationAmountsRequestResponse.Result = AutorisationResult;

            return listOfLoanApplicationAmountsRequestResponse;
        }

        public FastOverdraftFeeAmountRequestResponse GetFastOverdraftFeeAmount(double amount)
        {
            FastOverdraftFeeAmountRequestResponse fastOverdraftFeeAmountRequestResponse = new FastOverdraftFeeAmountRequestResponse();
            double fastOverdraftFeeAmount;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);

                        fastOverdraftFeeAmount = proxyClient.GetFastOverdraftFeeAmount(amount);
                    }

                    fastOverdraftFeeAmountRequestResponse.FastOverdraftFeeAmount = fastOverdraftFeeAmount;
                    fastOverdraftFeeAmountRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    fastOverdraftFeeAmountRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdraftFeeAmountRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    fastOverdraftFeeAmountRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdraftFeeAmountRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                fastOverdraftFeeAmountRequestResponse.Result = AutorisationResult;

            return fastOverdraftFeeAmountRequestResponse;
        }

        public InfoListRequestResponse GetCommunicationTypes()
        {
            InfoListRequestResponse communicationTypesRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> communicationTypes;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        communicationTypes = proxyClient.GetCommunicationTypes();
                    }

                    communicationTypesRequestResponse.InfoList = communicationTypes;
                    communicationTypesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    communicationTypesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    communicationTypesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    communicationTypesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    communicationTypesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                communicationTypesRequestResponse.Result = AutorisationResult;

            return communicationTypesRequestResponse;
        }

        public InfoListRequestResponse GetCountries()
        {
            InfoListRequestResponse countriesRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> countries;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        countries = proxyClient.GetCountries();
                    }

                    countriesRequestResponse.InfoList = countries;
                    countriesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    countriesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    countriesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    countriesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    countriesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                countriesRequestResponse.Result = AutorisationResult;

            return countriesRequestResponse;
        }

        public InfoListRequestResponse GetRegions(int country)
        {
            InfoListRequestResponse regionsRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> regions;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        regions = proxyClient.GetRegions(country);
                    }

                    regionsRequestResponse.InfoList = regions;
                    regionsRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    regionsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    regionsRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    regionsRequestResponse.Result.ResultCode = ResultCodes.failed;
                    regionsRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                regionsRequestResponse.Result = AutorisationResult;

            return regionsRequestResponse;
        }

        public InfoListRequestResponse GetArmenianPlaces(int region)
        {
            InfoListRequestResponse armenianPlacesRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> armenianPlaces;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        armenianPlaces = proxyClient.GetArmenianPlaces(region);
                    }

                    armenianPlacesRequestResponse.InfoList = armenianPlaces;
                    armenianPlacesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    armenianPlacesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    armenianPlacesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    armenianPlacesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    armenianPlacesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                armenianPlacesRequestResponse.Result = AutorisationResult;

            return armenianPlacesRequestResponse;
        }

        public AccountDescriptionRequestResponse GetAccountDescription(string accountNumber)
        {
            AccountDescriptionRequestResponse accountDescriptionRequestResponse = new AccountDescriptionRequestResponse();
            string accountDescription = null;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {

                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        accountDescription = proxyClient.GetAccountDescription(accountNumber);
                    }

                    accountDescriptionRequestResponse.AccountDescription = accountDescription;
                    accountDescriptionRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    accountDescriptionRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountDescriptionRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    accountDescriptionRequestResponse.Result.ResultCode = ResultCodes.failed;
                    accountDescriptionRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                accountDescriptionRequestResponse.Result = AutorisationResult;

            return accountDescriptionRequestResponse;
        }
        public ActionRequestResponse FastOverdraftValidations(string cardNumber)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        List<ActionError> actionError = proxyClient.FastOverdraftValidations(cardNumber);
                        string actionErrors = "";
                        if (actionError != null && actionError.Count > 0)
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            actionError.ForEach(m => actionErrors += m.Description + ";");
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.normal;
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        public FastOverdrafStartAndEndDateRequestResponse GetFastOverdrafStartAndEndDate()
        {
            FastOverdrafStartAndEndDateRequestResponse fastOverdrafStartAndEndDateRequestResponse = new FastOverdrafStartAndEndDateRequestResponse();
            DateTime startDate;
            DateTime endDate;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        startDate = proxyClient.GetNextOperDay();
                    }
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        endDate = proxyClient.GetFastOverdrafEndDate(startDate);
                    }

                    fastOverdrafStartAndEndDateRequestResponse.StartDate = startDate;
                    fastOverdrafStartAndEndDateRequestResponse.EndDate = endDate;
                    fastOverdrafStartAndEndDateRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    fastOverdrafStartAndEndDateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdrafStartAndEndDateRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    fastOverdrafStartAndEndDateRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdrafStartAndEndDateRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                fastOverdrafStartAndEndDateRequestResponse.Result = AutorisationResult;

            return fastOverdrafStartAndEndDateRequestResponse;
        }

        public FastOverdraftContractRequestResponse GetFastOverdraftContract(DateTime startDate, DateTime endDate, string cardNumber, string creditLineAccount, string currency, double amount, double interestRate)
        {

            FastOverdraftContractRequestResponse fastOverdraftContractRequestResponse = new FastOverdraftContractRequestResponse();
            CreditLinePrecontractData d = new CreditLinePrecontractData();
            short fillialCode = 0;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        d = proxyClient.GetCreditLinePrecontractData(startDate, endDate, interestRate, 0, cardNumber, currency, amount, 8);
                        fillialCode = proxyClient.GetCustomerFilial();
                    }

                    double penaltyRate = 0;

                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        penaltyRate = proxyClient.GetPenaltyRateOfLoans(54, startDate);
                    }


                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add(key: "customerNumberHB", value: AutorizedCustomer.CustomerNumber.ToString());
                    parameters.Add(key: "appID", value: "0");
                    parameters.Add(key: "bln_with_enddate", value: "True");
                    parameters.Add(key: "visaNumberHB", value: cardNumber);
                    parameters.Add(key: "dateOfBeginningHB", value: startDate.ToString("dd/MMM/yy"));
                    parameters.Add(key: "currencyHB", value: currency);
                    parameters.Add(key: "penaltyAddPercentHB", value: penaltyRate.ToString());
                    parameters.Add(key: "startCapitalHB", value: amount.ToString());
                    parameters.Add(key: "clientTypeHB", value: "");
                    parameters.Add(key: "filialCodeHB", value: fillialCode.ToString());
                    parameters.Add(key: "creditLineTypeHB", value: "54");

                    parameters.Add(key: "securityCodeHB", value: "");

                    parameters.Add(key: "loanProvisionPercentHB", value: "0");
                    parameters.Add(key: "provisionNumberHB", value: "01");
                    parameters.Add(key: "interestRateHB", value: interestRate.ToString());
                    parameters.Add(key: "interestRateFullHB", value: d.InterestRate.ToString());
                    parameters.Add(key: "connectAccountFullNumberHB", value: creditLineAccount);
                    parameters.Add(key: "interestRateEffectiveWithoutAccountServiceFee", value: d.InterestRateEffectiveWithoutAccountServiceFee.ToString());
                    parameters.Add(key: "dateOfNormalEndHB", value: endDate.ToString("dd/MMM/yy"));
                    parameters.Add(key: "HbDocID", value: "-1");
                    parameters.Add(key: "RepaymentKurs", value: d.RepaymentRate.ToString());

                    string content = Contracts.RenderContractHTML("CreditLineContract", parameters, "CreditLineContract");
                    fastOverdraftContractRequestResponse.ContractContent = ConvertAnsiToUnicode(System.Net.WebUtility.HtmlDecode(content));
                    fastOverdraftContractRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    fastOverdraftContractRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdraftContractRequestResponse.Result.Description = ConvertAnsiToUnicode(ex.Message);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    fastOverdraftContractRequestResponse.Result.ResultCode = ResultCodes.failed;
                    fastOverdraftContractRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                fastOverdraftContractRequestResponse.Result = AutorisationResult;

            return fastOverdraftContractRequestResponse;
        }


        public static string ConvertAnsiToUnicode(string str)
        {

            if (str == null)
            {
                return str;
            }

            if (!HasAnsiCharacters(str))
            {
                return str;
            }

            string result = "";
            int strLen = str.Length;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                char uChar;
                if (charCode >= 178 && charCode <= 253)
                {
                    if (charCode % 2 == 0)
                    {
                        uChar = (char)((charCode - 178) / 2 + 1329);
                    }
                    else
                    {
                        uChar = (char)((charCode - 179) / 2 + 1377);
                    }
                }
                else
                {
                    if (charCode >= 32 && charCode <= 126)
                    {
                        uChar = str[i];
                    }
                    else
                    {
                        switch (charCode)
                        {
                            case 162:
                                uChar = (char)1415;
                                break;
                            case 168:
                                uChar = (char)1415;
                                break;
                            case 176:
                                uChar = (char)1371;
                                break;
                            case 175:
                                uChar = (char)1372;
                                break;
                            case 177:
                                uChar = (char)1374;
                                break;
                            case 170:
                                uChar = (char)1373;
                                break;
                            case 173:
                                uChar = '-';
                                break;
                            case 163:
                                uChar = (char)1417;
                                break;
                            case 169:
                                uChar = '.';
                                break;
                            case 166:
                                uChar = '»'; //187
                                break;
                            case 167:
                                uChar = '«'; //171
                                break;
                            case 164:
                                uChar = ')';
                                break;
                            case 165:
                                uChar = '(';
                                break;
                            case 46:
                                uChar = '.';
                                break;
                            default:
                                uChar = str[i];
                                break;
                        }
                    }
                }
                result += uChar;
            }
            return result;
        }

        public static bool HasAnsiCharacters(string str)
        {
            bool hasAnsi = false;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                if (charCode >= 178 && charCode <= 253 && charCode != 187)
                {
                    hasAnsi = true;
                }
            }

            return hasAnsi;
        }

        public ACRAAgreementTextRequestResponse GetACRAAgreementText()
        {
            ACRAAgreementTextRequestResponse ACRAAgreementTextRequestResponse = new ACRAAgreementTextRequestResponse();
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                string acraText = "";

                if (Language == 1)
                {
                    acraText = "«Սույնով տալիս եմ իմ համաձայնությունը, որ.<br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից հարցում կատարվի «ԱՔՌԱ Քրեդիտ Ռեփորթինգ» ՓԲԸ - ին և վերջինիս խնդրում եմ «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ին տրամադրել իմ ներկա և անցյալ ֆինանսական պարտավորությունների վերաբերյալ տեղեկություններ, ինչպես նաև այլ տվյալներ, որոնք «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից կարող են հաշվի առնվել ինձ հետ վարկային (փոխառության և այլն) պայմանագիր կնքելու վերաբերյալ որոշում կայացնելիս: < br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից վարկային (փոխառության և այլն) պայմանագիր կնքելու դեպքում տվյալ վարկային(փոխառության և այլն) պայմանագրի գործողության ողջ ընթացքում ցանկացած պահի առանց ինձ նախապես տեղյակ պահելու «ԱՔՌԱ Քրեդիտ Ռեփորթինգ» ՓԲԸ - ն «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ին տրամադրի իմ ապագա ֆինանսական պարտավորությունների վերաբերյալ տեղեկություններ, ինչպես նաև այլ տվյալներ:< br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից սեփականության կամ ընդհանուր սեփականության իրավունքով ինձ պատկանող գույքերի վերաբերյալ հարցում կատարի ՀՀ ԿԱ Անշարժ գույքի կադաստրի պետական կոմիտե և ստանա սպառիչ տեղեկատվություն իմ գույքային իրավունքների, այդ թվում` դրանց ծագման հիմքերի վերաբերյալ, ինչպես նաև կադաստրային գործից ստանա սեփականության իրավունքի վկայականի, հատակագծի և այլ անհրաժեշտ փաստաթղթերի պատճենները` մինչև իմ վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը:< br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն սեփականության կամ ընդհանուր սեփականության իրավունքով ինձ պատկանող տրանսպորտային միջոցների վերաբերյալ հարցում կատարի ՀՀ Ճանապարհային ոստիկանություն և ստանա սպառիչ տեղեկատվություն տրանսպորտային միջոցների նկատմամբ իմ գույքային իրավունքների, այդ թվում` դրանց ծագման հիմքերի վերաբերյալ, ինչպես նաև ստանա սեփականության իրավունքի վկայականի և այլ անհրաժեշտ փաստաթղթերի պատճենները` մինչև իմ վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը:< br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն հարցում կատարի «Հայաստանի ավտոապահովագրողների բյուրո» ԻԱՄ - ին և ապահովագրական ընկերություններին և ստանա ցանկացած տեղեկատվություն ինձ սեփականության իրավունքով պատկանող տրանսպորտային միջոցների ապահովագրության վերաբերյալ(այդ թվում՝ ապահովադրի և շահառուների վերաբերյալ տեղեկություններ)` մինչև իմ վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը:< br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն հարցում կատարի «Նորք» սոցիալական ծառայությունների տեխնոլոգիական և իրազեկման կենտրոն հիմնադրամ, և խնդրում եմ վերջինիս տրամադրել իմ վերաբերյալ ցանկացած տեղեկատվություն, որը կարող է կիրառվել Բանկի կողմից:< br />•	«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից ինձ ուղարկվեն ծանուցումներ՝ «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից մատուցվող ծառայությունների հետ կապված ցանկացած տեղեկատվության վերաբերյալ:< br />•	Քարտային հաշվի դրական մնացորդի բացակայության դեպքում «Արագ օվերդրաֆտ»-ի միջնորդավճարը գանձվի «Արագ օվերդրաֆտ»-ի գումարից:< br />•	Քարտային հաշվին «Արագ օվերդրաֆտ»-ի ակտիվացման դեպքում ՀՀ օրենսդրությամբ սահմանված պարտադիր ներկայացման ենթակա տեղեկատվությունը տրամադրվի դեբետային / կրեդիտային քարտի դիմումով սահմանված եղանակով:< br />Գիտակցում եմ, որ տրամադրված տեղեկությունները և տվյալները, կախված դրանց բովանդակությունից, կարող են ազդել «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից կայացված համապատասխան որոշման վրա:Սույն համաձայնությունը կարդացել եմ և հավաստում եմ, որ այն ինձ համար ամբողջությամբ հասկանալի և ընդունելի է:»";

                }
                else
                {
                    acraText = "Hereby I agree that:<br />• a request will be sent by “ACBA - CREDIT AGRICOLE BANK” CJSC to “Acra Credit Reporting”  and I ask the latter to provide “ACBA - CREDIT AGRICOLE BANK” CJSC with information regarding my current and past financial obligations, as well as data which can be considered by “ACBA - CREDIT AGRICOLE BANK” CJSC while making a decision on signing a loan or other agreements with me.<br />• in case of signing a loan or other agreement with “ACBA - CREDIT AGRICOLE BANK” CJSC, at any moment during the whole period of loan or other agreement validity period, “ACRA Credit Reporting” CJSC can provide “ACBA - CREDIT AGRICOLE BANK” CJSC with information regarding my future financial obligations without informing me, as well as other data.<br />• make a request by “ACBA - CREDIT AGRICOLE BANK” CJSC on the property owned by me with the right of ownership to the State Committee of Real Property Cadastre of the Government of the RA and receive comprehensive information about my property rights, including  their origins,  as well as receive copies of ownership certificate, layout and other necessary documents until the complete fulfillment of my loan obligations.<br />• “ACBA - CREDIT AGRICOLE BANK” CJSC makes a request regarding the vehicles that belong to me with the right of general ownership to RA Traffic police and receive comprehensive information on my property rights towards the vehicles, including their origins, as well as receive the copies of ownership certificate and other necessary documents until the complete fulfillment of my loan obligations.<br />• “ACBA - CREDIT AGRICOLE BANK” CJSC makes a request to Armenian Motor Insurers’ Bureau ULE and insurance companies and receive any information regarding the insurance of the vehicles that belong to me with the right of ownership (including information regarding the insurer and beneficiaries) until the complete fulfillment of my loan obligation. <br />• “ACBA - CREDIT AGRICOLE BANK” CJSC makes a request to \"Nork\" Social Services Technology And Awareness Center Foundation, and I ask the latter to provide any information about me which can be used by the Bank. <br />• notifications are sent by “ACBA - CREDIT AGRICOLE BANK” CJSC to me regarding any information connected with the services provided by “ACBA - CREDIT AGRICOLE BANK” CJSC.<br />• In case of absence of a positive balance on the card account the commission fee of “Fast overdraft” will be charged from the “Fast overdraft” amount.<br />• In case a “Fast overdraft” is activated on the card account the information of mandatory presentation according to RA legislation is provided to in the way defined according to the debt / credit card application. ‘I am aware that the provided information and data can affect the corresponding decision made by “ACBA - CREDIT AGRICOLE BANK” CJSC depending on their contents.I have read the current agreement and I confirm that it is fully understandable and acceptable for me.";
                }
                ACRAAgreementTextRequestResponse.AcraText = acraText;
            }

            return ACRAAgreementTextRequestResponse;
        }

        public ReceivedFastTransferPaymentOrderRequestResponse GetReceivedFastTransferPaymentOrder(long id)
        {
            ReceivedFastTransferPaymentOrderRequestResponse receivedFastTransferPaymentOrderRequestResponse = new ReceivedFastTransferPaymentOrderRequestResponse();
            ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        receivedFastTransferPaymentOrder = proxyClient.GetReceivedFastTransferPaymentOrder(id,"");
                    }

                    receivedFastTransferPaymentOrderRequestResponse.ReceivedFastTransferPaymentOrder = receivedFastTransferPaymentOrder;
                    receivedFastTransferPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    receivedFastTransferPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    receivedFastTransferPaymentOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    receivedFastTransferPaymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    receivedFastTransferPaymentOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                receivedFastTransferPaymentOrderRequestResponse.Result = AutorisationResult;

            return receivedFastTransferPaymentOrderRequestResponse;

        }
        
        public InfoListRequestResponse GetTransferSystemCurrency(int transfersystem)
        {
            InfoListRequestResponse transferSystemCurrenciesRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> transferSystemCurrencies;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        transferSystemCurrencies = proxyClient.GetTransferSystemCurrency(transfersystem);
                    }

                    transferSystemCurrenciesRequestResponse.InfoList = transferSystemCurrencies;
                    transferSystemCurrenciesRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    transferSystemCurrenciesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferSystemCurrenciesRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    transferSystemCurrenciesRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferSystemCurrenciesRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                transferSystemCurrenciesRequestResponse.Result = AutorisationResult;

            return transferSystemCurrenciesRequestResponse;
        }

        public InfoListRequestResponse GetFastTransferSystemTypes()
        {
            InfoListRequestResponse transferTypeRequestResponse = new InfoListRequestResponse();
            Dictionary<string, string> transferTypes;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBInfoServiceClient proxyClient = new XBInfoServiceClient())
                    {
                        proxyClient.Init(this.ClientIp, this.Language);
                        transferTypes = proxyClient.GetTransferTypes(1);
                        transferTypes.Remove("12");
                    }

                    transferTypeRequestResponse.InfoList = transferTypes;
                    transferTypeRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    transferTypeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferTypeRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    transferTypeRequestResponse.Result.ResultCode = ResultCodes.failed;
                    transferTypeRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                transferTypeRequestResponse.Result = AutorisationResult;

            return transferTypeRequestResponse;
        }

        public ActionRequestResponse SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order)
        {
            ActionRequestResponse result = new ActionRequestResponse();


            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        if (user.AdvancedOptions == null)
                        {
                            user.AdvancedOptions = new Dictionary<string, string>();
                        }

                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        order.Quality = OrderQuality.Draft;
                        order.FilialCode = 22000;
                        order.Description = "Non-commercial transfer for personal needs";
                        ActionResult saveResult = proxyClient.SaveReceivedFastTransferPaymentOrder(order);
                        result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                        result.Id = saveResult.Id;
                        string actionErrors = "";
                        if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                        {
                            saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                        }
                        result.Result.Description = actionErrors;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }


        public ActionRequestResponse ApproveReceivedFastTransferPaymentOrder(long id)
        {
            ActionRequestResponse result = new ActionRequestResponse();

            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        ReceivedFastTransferPaymentOrder order = proxyClient.GetReceivedFastTransferPaymentOrder(id, "");

                        if (CheckSign(order))
                        {
                            ActionResult saveResult = proxyClient.ApproveFastTransferPaymentOrder(order);
                            result.Result.ResultCode = (ResultCodes)((short)saveResult.ResultCode);
                            result.Id = saveResult.Id;
                            string actionErrors = "";
                            if (saveResult.Errors != null && saveResult.Errors.Count > 0)
                            {
                                saveResult.Errors.ForEach(m => actionErrors += m.Description + ";");
                            }

                            result.Result.Description = actionErrors;
                        }
                        else
                        {
                            result.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                            {
                                result.Result.Description = "Սխալ PIN կոդ։";
                            }
                            else
                            {
                                result.Result.Description = "Incorrect PIN code.";
                            }

                        }
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }
            else
                result.Result = AutorisationResult;

            return result;
        }

        /// <summary>
        /// Վերադարձնում է ստացված արագ փոխանցման հայտի մերժման պատճառը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DescriptionRequestResponse GetReceivedFastTransferOrderRejectReason(int orderId)
        {
            DescriptionRequestResponse result = new DescriptionRequestResponse();
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);

                        string reason = proxyClient.GetReceivedFastTransferOrderRejectReason(orderId);
                        result.Result.ResultCode = ResultCodes.normal;
                        result.Description = reason;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    result.Result.ResultCode = ResultCodes.failed;
                    result.Result.Description = "Error!Please try later";
                }
            }

            return result;
        }

        private bool CheckSign(ReceivedFastTransferPaymentOrder order)
        {

            bool isSigned = false;
            string sessionId = "";
            byte language = 0;

            ///Սեսիայի նունականացման համար
            if (WebOperationContext.Current.IncomingRequest.Headers["SessionId"] != null)
                sessionId = WebOperationContext.Current.IncomingRequest.Headers["SessionId"];


            ///Լեզու
            if (WebOperationContext.Current.IncomingRequest.Headers["language"] != null)
                byte.TryParse(WebOperationContext.Current.IncomingRequest.Headers["language"], out language);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (isTestVersion && sessionId == "ba0f312d-8487-445e-aee2-d5877ac1d4de")
            {
                isSigned = true;
            }
            else
            {

                Dictionary<string, string> signData = new Dictionary<string, string>();
                signData.Add(key: "TransactionID", value: order.Id.ToString());
                signData.Add(key: "Amount", value: Math.Truncate(order.Amount).ToString());

              

                string creditAccount = "";
                creditAccount = order.ReceiverAccount.AccountNumber.ToString();

                signData.Add(key: "SenderAccountFirstPart", value: "0");
                signData.Add(key: "SenderAccountSecondPart", value: "0");

                signData.Add(key: "RecepientAccountFirstPart", value: creditAccount.Substring(0, 10));
                signData.Add(key: "RecepientAccountSecondPart", value: creditAccount.Substring(10, creditAccount.Length - 10));

                signData.Add(key: "IpAddress", value: ClientIp);

                string OTP = "";

                ///Թվային կոդ
                if (WebOperationContext.Current.IncomingRequest.Headers["OTP"] != null)
                    OTP = WebOperationContext.Current.IncomingRequest.Headers["OTP"];
         
                bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                if (EnableSecurityService)
                {
                    using (OnlineBankingSecurityClient proxyClient = new OnlineBankingSecurityClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }
                else
                {
                    using (MobileLoginClient proxyClient = new MobileLoginClient())
                    {
                        isSigned = proxyClient.SingData(sessionId, OTP, signData, language);
                    }
                }

            }
            return isSigned;

        }

      

        public OrderHistoryRequestResponse GetOnlineOrderHistory(int orderId)
        {
            OrderHistoryRequestResponse descriptionRequestResponse = new OrderHistoryRequestResponse();
            List<OrderHistory> orderHistory;
            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        orderHistory = proxyClient.GetOnlineOrderHistory(orderId);
                    }

                    descriptionRequestResponse.OrderHistory = orderHistory;
                    descriptionRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    descriptionRequestResponse.Result.ResultCode = ResultCodes.failed;
                    descriptionRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    descriptionRequestResponse.Result.ResultCode = ResultCodes.failed;
                    descriptionRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                descriptionRequestResponse.Result = AutorisationResult;

            return descriptionRequestResponse;
        }


        private List<HBProductPermission> GetUserProductsPermissions()
        {
            List<HBProductPermission> list = new List<HBProductPermission>();
            using (XBServiceClient proxyClient = new XBServiceClient())
            {
                proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                list = proxyClient.GetHBUserProductsPermissions(AutorizedCustomer.UserName);
            }
            return list;
        }

        public OrdersRequestResponse GetConfirmRequiredOrders(DateTime startDate, DateTime endDate)
        {
            OrdersRequestResponse ordersRequestResponse = new OrdersRequestResponse();
            List<Order> orders;


            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        orders = proxyClient.GetConfirmRequiredOrders(AutorizedCustomer.UserName,0,startDate,endDate,"","","",true,null,-1);
                        orders.RemoveAll(m => m.Source == XBS.SourceType.Bank);

                    }

                    ordersRequestResponse.Orders = orders;
                    ordersRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    ordersRequestResponse.Result.ResultCode = ResultCodes.failed;
                    ordersRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                ordersRequestResponse.Result = AutorisationResult;


            return ordersRequestResponse;
        }


        public InternationalPaymentOrderRequestResponse GetInternationalPaymentOrder(long id)
        {
            InternationalPaymentOrderRequestResponse paymentOrderRequestResponse = new InternationalPaymentOrderRequestResponse();
            InternationalPaymentOrder paymentOrder;



            Authorize();

            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    using (XBServiceClient proxyClient = new XBServiceClient())
                    {
                        proxyClient.SetUser(AutorizedCustomer, Language, ClientIp, user, XBS.SourceType.MobileBanking);
                        paymentOrder = proxyClient.GetInternationalPaymentOrder(id);
                    }

                    bool hasPermission = true;
                    if (AutorizedCustomer.LimitedAccess != 0)
                    {
                        if (!HasProductPermission(paymentOrder.DebitAccount.AccountNumber) ||
                            (paymentOrder.SubType == 3 && !HasProductPermission(paymentOrder.ReceiverAccount.AccountNumber))
                            || (paymentOrder.FeeAccount != null && paymentOrder.FeeAccount.AccountNumber != "0" &&
                            !HasProductPermission(paymentOrder.FeeAccount.AccountNumber)))
                        {
                            hasPermission = false;
                            paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                            if (Language == (byte)XBS.Languages.hy)
                                paymentOrderRequestResponse.Result.Description = "Տվյալները հասանելի չեն։";
                            else
                                paymentOrderRequestResponse.Result.Description = "Permission denied";
                        }
                    }
                    if (hasPermission)
                    {
                        paymentOrderRequestResponse.InternationalPaymentOrder = paymentOrder;
                        paymentOrderRequestResponse.Result.ResultCode = ResultCodes.normal;
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    paymentOrderRequestResponse.Result.ResultCode = ResultCodes.failed;
                    paymentOrderRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                paymentOrderRequestResponse.Result = AutorisationResult;

            return paymentOrderRequestResponse;
        }

        //For Real Sipan
        //public void ChangeMailVerificationStatus(string emailGuid)
        //{
        //    ActionRequestResponse result = new ActionRequestResponse();
        //    try
        //    {
        //        string status = "";
        //        using (ACBAServiceReference.CustomerOperationsClient proxyClient = new ACBAServiceReference.CustomerOperationsClient())
        //        {
        //            status = proxyClient.ChangeMailVerificationStatus(emailGuid);
        //        }
        //        if (status == "1")
        //            result.Result.ResultCode = ResultCodes.normal;
        //        else
        //            result.Result.ResultCode = ResultCodes.failed;
        //    }
        //    catch (FaultException ex)
        //    {
        //        WriteLog(ex);
        //        result.Result.ResultCode = ResultCodes.failed;
        //        result.Result.Description = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        result.Result.ResultCode = ResultCodes.failed;
        //        result.Result.Description = "Error!Please try later";
        //    }

        //    WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
        //    WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", WebConfigurationManager.AppSettings["RedirectURL"].ToString());

        //}

        //For Test Sipan
        public void ChangeMailVerificationStatus(string emailGuid, string lang)
        {
            ActionRequestResponse result = new ActionRequestResponse();
            try
            {
                string status = "";
                using (ACBAServiceReference.CustomerOperationsClient proxyClient = new ACBAServiceReference.CustomerOperationsClient())
                {
                    status = proxyClient.ChangeMailVerificationStatus(emailGuid);
                }
                if (status == "0")
                    result.Result.ResultCode = ResultCodes.normal;
                else
                    result.Result.ResultCode = ResultCodes.failed;
            }
            catch (FaultException ex)
            {
                WriteLog(ex);
                result.Result.ResultCode = ResultCodes.failed;
                result.Result.Description = ex.Message;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                result.Result.ResultCode = ResultCodes.failed;
                result.Result.Description = "Error!Please try later";
            }

            WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
            if (result.Result.ResultCode == ResultCodes.normal)
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", WebConfigurationManager.AppSettings["RedirectURLSuccess"].ToString());
            }
            else
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Location", WebConfigurationManager.AppSettings["RedirectURLError"].ToString());
            }

        }

        public CustomerInfoForAuthenticationRequestResponse GetCustomerInfoForAuthentication(DocumentType documentType, string documentValue)
        {
            CustomerInfoForAuthenticationRequestResponse customerInfoForAuthenticationRequestResponse = new CustomerInfoForAuthenticationRequestResponse();

            //Եթե նշված չէ կամ սխալ է փաստաթղթի տեսակը։
            if (documentType != DocumentType.IdentifierCard && documentType != DocumentType.RApassport && documentType != DocumentType.BiometricPassport)
            {
                customerInfoForAuthenticationRequestResponse.Result.ResultCode = ResultCodes.failed;
                customerInfoForAuthenticationRequestResponse.Info = null;
                return customerInfoForAuthenticationRequestResponse;
            }

            ACBAServiceReference.Person person = new ACBAServiceReference.Person();
            ACBAServiceReference.SearchCustomers param = new ACBAServiceReference.SearchCustomers();
            ACBAServiceReference.Customer customer = new ACBAServiceReference.Customer();
            if (WebOperationContext.Current.IncomingRequest.Headers["ACBACustomerIdentifier"] == System.Configuration.ConfigurationManager.AppSettings["ACBACustomerIdentifier"].ToString())
            {
                try
                {
                    //Անկախ CustomerAuthenticationResult-ից, եթե Exception չի առաջացել, ապա համարում ենք հարցումը դրական
                    customerInfoForAuthenticationRequestResponse.Result.ResultCode = ResultCodes.normal;

                    using (ACBAServiceReference.CustomerOperationsClient proxyClient = new ACBAServiceReference.CustomerOperationsClient())
                    {
                        person = proxyClient.FindPersonInNorq(documentValue);
                        //if person not found returnes new instance instead of null
                        if (string.IsNullOrEmpty(person.fullName.firstName))
                        {
                            //Not found NORQ person։
                            customerInfoForAuthenticationRequestResponse.Info.Result = CustomerAuthenticationResult.NonCustomer;
                            customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                            customerInfoForAuthenticationRequestResponse.Info.ResultDescription = "Հաճախորդը գտնված չէ ՆՈՐՔ համակարգում։";
                            return customerInfoForAuthenticationRequestResponse;
                        }

                        param.firstName = Helper.ConvertUnicodeToAnsi(person.fullName.firstName.Replace("եվ", "և"));
                        param.lastName = Helper.ConvertUnicodeToAnsi(person.fullName.lastName.Replace("եվ", "և"));
                        param.passportNumber = person.documentList.Exists(item => item.documentType.key == (short)DocumentType.RApassport) ? person.documentList.Find(item => item.documentType.key == (short)DocumentType.RApassport).documentNumber : null;
                        param.socCardNumber = person.documentList.Exists(item => item.documentType.key == (short)DocumentType.SocialServiceNumber) ? person.documentList.Find(item => item.documentType.key == (short)DocumentType.SocialServiceNumber).documentNumber : null;
                        param.birthDate = person.birthDate;

                        param.quality = -1;
                        param.customerType = 6;
                        param.filialCode = -1;
                        param.residence = -1;


                        customer = proxyClient.GetIdentifiedCustomer(param);

                        //Not found person in ACBA
                        if (customer == null)
                        {
                            customerInfoForAuthenticationRequestResponse.Info.Result = CustomerAuthenticationResult.NonCustomer;
                            customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                            customerInfoForAuthenticationRequestResponse.Info.ResultDescription = "Հաճախորդը գտնված չէ։";
                            return customerInfoForAuthenticationRequestResponse;
                        }


                        //using (XBServiceClient proxy = new XBServiceClient())
                        //{
                        //    //Customer with online banking product
                        //    bool hasCustomerOnlineBanking = proxy.HasCustomerOnlineBanking(customer.customerNumber);
                        //    if (hasCustomerOnlineBanking)
                        //    {
                        //        customerInfoForAuthenticationRequestResponse.Info.Result = CustomerAuthenticationResult.CustomerWithOnlineBanking;
                        //        customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                        //        customerInfoForAuthenticationRequestResponse.Info.CustomerNumber = customer.customerNumber;
                        //        customerInfoForAuthenticationRequestResponse.Info.ResultDescription = "Հաճախորդը ունի օնլայն բանկինգ։";
                        //        return customerInfoForAuthenticationRequestResponse;
                        //    }
                        //}


                        customerInfoForAuthenticationRequestResponse.Info.CustomerNumber = customer.customerNumber;
                        customerInfoForAuthenticationRequestResponse.Info.Result = CustomerAuthenticationResult.CustomerWithAttachment;
                        customerInfoForAuthenticationRequestResponse.Info.ResultDescription = "Հաճախորդը գտնված է։";

                        var customerPhoto = proxyClient.GetCustomerPhoto(customer.customerNumber);

                        //if (customerPhoto != null)
                        //{
                        //    byte[] BigSizePhoto = proxyClient.GetCustomerOnePhoto(customerPhoto.Id);
                        //    customerInfoForAuthenticationRequestResponse.Info.Data.Add(BigSizePhoto, customerPhoto.FileExtension);
                        //    customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Photo;
                        //}
                        //else
                        //{
                            //Հաճախորդի անձը հաստատող փաստաթղթեր
                            var documents = proxyClient.GetCustomerDocumentList(customer.identityId, 1).FindAll(doc => doc.documentGroup.key == 1);
                            documents.Sort((x, y) => y.id.CompareTo(x.id));

                            foreach (var document in documents)
                            {
                                var attachments = proxyClient.GetAttachmentDocumentList(Convert.ToUInt64(document.id), true, 1);
                                if (attachments.Count != 0)
                                {

                                    attachments.Sort((x, y) => x.id.CompareTo(y.id));
                                    attachments.ForEach(item =>
                                    {
                                        customerInfoForAuthenticationRequestResponse.Info.Data.Add(proxyClient.GetOneAttachment(item.id), ((TypeOfAttachments)item.FileExtension).ToString());
                                    });
                                    customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Document;
                                    break;
                                }
                            }

                            if (customerInfoForAuthenticationRequestResponse.Info.Data.Count == 0)
                            {
                                customerInfoForAuthenticationRequestResponse.Info.Result = CustomerAuthenticationResult.CustomerWithNoAttachments;
                                customerInfoForAuthenticationRequestResponse.Info.TypeOfDocument = CustomerAuthenticationInfoType.Empty;
                                customerInfoForAuthenticationRequestResponse.Info.ResultDescription = "Առկա չէ հաճախորդին կցված փաստաթուղթ։";
                            }
                        //}
                    }
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    customerInfoForAuthenticationRequestResponse.Result.ResultCode = ResultCodes.failed;
                    customerInfoForAuthenticationRequestResponse.Result.Description = ex.Message;
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    customerInfoForAuthenticationRequestResponse.Result.ResultCode = ResultCodes.failed;
                    customerInfoForAuthenticationRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
            {
                customerInfoForAuthenticationRequestResponse.Result.ResultCode = ResultCodes.notAutorized;
                customerInfoForAuthenticationRequestResponse.Result.Description = "Not Autorized";
            }
            return customerInfoForAuthenticationRequestResponse;
        }

        public ByteArrayRequestResponse PrintLoanTermsSheet(byte loanType, long orderid)
        {
            ByteArrayRequestResponse fileContent = new ByteArrayRequestResponse();

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    
                    fileContent.Content = GetLoanTermsSheetContent(loanType, orderid);

                    fileContent.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    fileContent.Result.ResultCode = ResultCodes.failed;
                    fileContent.Result.Description = ConvertAnsiToUnicode(ex.Message);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    fileContent.Result.ResultCode = ResultCodes.failed;
                    fileContent.Result.Description = "Error!Please try later";
                }
            }
            else
                fileContent.Result = AutorisationResult;

            return fileContent;
        }

        public StringRequestResponse PrintLoanTermsSheetBase64(byte loanType, long orderid)
        {
            StringRequestResponse stringRequestResponse = new StringRequestResponse();
            byte[] fileContent;

            Authorize();
            if (AutorisationResult.ResultCode == ResultCodes.normal)
            {
                try
                {
                    fileContent = GetLoanTermsSheetContent(loanType, orderid);

                    stringRequestResponse.Content = Convert.ToBase64String(fileContent);
                    stringRequestResponse.Result.ResultCode = ResultCodes.normal;
                }
                catch (FaultException ex)
                {
                    WriteLog(ex);
                    stringRequestResponse.Result.ResultCode = ResultCodes.failed;
                    stringRequestResponse.Result.Description = ConvertAnsiToUnicode(ex.Message);
                }
                catch (Exception ex)
                {
                    WriteLog(ex);
                    stringRequestResponse.Result.ResultCode = ResultCodes.failed;
                    stringRequestResponse.Result.Description = "Error!Please try later";
                }
            }
            else
                stringRequestResponse.Result = AutorisationResult;

            return stringRequestResponse;
        }

        /// <summary>
        /// Էական պայմանների անհատական թերթիկի ստացում byte[] ֆորմատով
        /// </summary>
        /// <param name="loanType"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        private byte[] GetLoanTermsSheetContent(byte loanType, long orderid)
        {
            byte[] fileContent;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string contractName = "";
            if (loanType == 1) contractName = "LoanTerms";
            else if (loanType == 2 || loanType == 3) contractName = "CreditLineTerms";

            parameters.Add(key: "hbDocID", value: orderid.ToString());
            parameters.Add(key: "HB", value: "True");

            fileContent = Contracts.RenderContract(contractName, parameters, "PrintLoanTermsSheet");                
   
            return fileContent;
        }

        

    }
}

