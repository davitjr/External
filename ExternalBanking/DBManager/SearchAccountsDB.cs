using System;
using System.Collections.Generic;


namespace ExternalBanking.DBManager
{
    class SearchAccountsDB
    {
        internal static List<SearchAccountResult> SearchAccounts(SearchAccounts searchParams, ACBAServiceReference.User currentUser)
        {
            List<SearchAccountResult> accountList = new List<SearchAccountResult>();
            SearchAccountResult searchAccountResult = new SearchAccountResult();
            if (!String.IsNullOrEmpty(searchParams.accountNumber))
            {

                Account account = null;
                if(currentUser!=null && currentUser.isOnlineAcc)
                {
                    account = Account.GetSystemAccount(searchParams.accountNumber);
                }
                else
                {
                    account = Account.GetAccount(ulong.Parse(searchParams.accountNumber));
                }

                
                if (account != null)
                {
                    ulong customerNumber = account.GetAccountCustomerNumber();

                    searchAccountResult = SetSearchAccountResult(account, customerNumber);
                    accountList.Add(searchAccountResult);
                }


            }
            else if (!String.IsNullOrEmpty(searchParams.customerNumber))
            {
                List<Account> account = new List<Account>();
                account.AddRange(Account.GetAccounts(ulong.Parse(searchParams.customerNumber)));

                if (searchParams.includeClosedAccounts)
                {
                    account.AddRange(Account.GetClosedAccounts(ulong.Parse(searchParams.customerNumber)));
                }

                account.ForEach(m =>
                    {
                        searchAccountResult = SetSearchAccountResult(m, ulong.Parse(searchParams.customerNumber));
                        accountList.Add(searchAccountResult);
                    }
                    );
            }

            if (!searchParams.includeClosedAccounts)
            {
                accountList.RemoveAll(m => m.CloseDate != null);
            }

            if (!String.IsNullOrEmpty(searchParams.currency))
            {
                accountList.RemoveAll(m => m.Currency != searchParams.currency);
            }
            if (searchParams.filialCode != 0)
            {
                accountList.RemoveAll(m => m.FilialCode != searchParams.filialCode);
            }

            return accountList;


        }

        internal static SearchAccountResult SetSearchAccountResult(Account account, ulong customerNumber)
        {
            SearchAccountResult searchAccountResult = new SearchAccountResult();
            searchAccountResult.AccountNumber = account.AccountNumber;
            searchAccountResult.CustomerNumber = customerNumber;
            searchAccountResult.Currency = account.Currency;
            searchAccountResult.Description = account.AccountDescription;
            searchAccountResult.CloseDate = account.ClosingDate;
            searchAccountResult.ProductNumber = account.ProductNumber;
            searchAccountResult.FilialCode = account.FilialCode;
            searchAccountResult.AccountType = account.AccountType;
            searchAccountResult.AccountTypeDescription = account.AccountTypeDescription;

            return searchAccountResult;
        }
    }
}
