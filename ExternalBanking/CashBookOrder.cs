using ExternalBanking.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Դրամարկղի մատյանի ավելցուկ/պակասորդ
    /// </summary>
    public class CashBookOrder : Order
    {
        public Account CreditAccount { get; set; }
        public List<CashBook> CashBooks { get; set; }
        public byte CorrespondentAccountType { get; set; }

        public ActionResult SaveAndApprove(User user, string userName, short schemaType, SourceType source)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.CheckOpDayClosingStatus(user.filialCode));
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            //Գումար մուտքագրված չէ
            if (CashBooks == null)
            {
                result.Errors.Add(new ActionError(419));
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Complete(CashBooks, user);
            result = Validate(CashBooks, user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                if (result.ResultCode != ResultCode.ValidationError)
                {
                    result = CashBookOrderDB.Save(this, CashBooks, userName, user.userID, source);
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            result = base.Confirm(user);
            return result;
        }

        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        public void Complete(List<CashBook> cashBooks, User user)
        {
            CashBook cashbook = CashBooks[0];
            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();
            this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            foreach (CashBook cashBook in cashBooks)
            {
                cashBook.Complete(user);
            }

            if (DebitAccount == null)
            {
                DebitAccount = new Account();
            }

            if (CreditAccount == null)
            {
                CreditAccount = new Account();
            }

            if (this.Type == OrderType.CashBookSurPlusDeficitClosingApprove)
            {
                DebitAccount = CashBookOrderDB.GetCashBookDebitAccount(cashbook.ID);
                CreditAccount = CashBookOrderDB.GetCashBookCreditAccount(cashbook.ID);
                CorrespondentAccountType = CashBookOrderDB.GetCorrespondentAccountType(cashbook.ID);
            }
            else if (Type != OrderType.CashBookSurPlusDeficitClosing && Type != OrderType.CashBookSurPlusDeficitPartiallyClosing)
            {
                CreditAccount = new Account();
                if (cashbook.Type == 1 || cashbook.Type == 3 || cashbook.Type == 2 || cashbook.Type == 4)
                {
                    List<double> operationAccounts = CashBookOrderDB.GetOperationAccounts(cashbook, user.filialCode, user.AdvancedOptions["isHeadCashBook"]);
                    this.DebitAccount.AccountNumber = operationAccounts[0].ToString();
                    this.CreditAccount.AccountNumber = operationAccounts[1].ToString();
                }
            }
            else
            {
                if (cashbook.Type == 1)//ավելցուկ
                {
                    List<double> operationAccounts = CashBookOrderDB.GetOperationAccounts(cashbook);
                    DebitAccount = new Account();
                    DebitAccount.AccountNumber = operationAccounts[1].ToString();
                    DebitAccount = Account.GetAccount(DebitAccount.AccountNumber);
                    if (CorrespondentAccountType == 1)
                    {
                        CreditAccount = Account.GetAccount(CreditAccount.AccountNumber);
                    }
                    //else if (CorrespondentAccountType == 2)
                    //{
                    //    //CreditAccount = new Account();
                    //}
                    else if (CorrespondentAccountType == 3)
                    {
                        //income account 
                        //CreditAccount = Account.GetAccount(CreditAccount.AccountNumber);
                    }
                }
                else if (cashbook.Type == 2)//ավելցուկի փակում
                {
                    List<double> operationAccounts = CashBookOrderDB.GetOperationAccounts(cashbook, user.filialCode, user.AdvancedOptions["isHeadCashBook"]);
                    DebitAccount.AccountNumber = operationAccounts[0].ToString();
                    DebitAccount = Account.GetAccount(DebitAccount.AccountNumber);
                    if (CorrespondentAccountType == 1)//Հաճախորդի հաշիվ
                    {
                        CreditAccount = Account.GetAccount(CreditAccount.AccountNumber);
                    }
                    else if (CorrespondentAccountType == 2)//Դրամարկղ
                    {
                        CreditAccount = Account.GetSystemAccount(operationAccounts[1].ToString());
                    }
                    else if (CorrespondentAccountType == 3)//Եկամուտ
                    {
                        CreditAccount = Account.GetOperationSystemAccount(6035, "AMD", user.filialCode);
                    }
                }
                else if (cashbook.Type == 4)//Պակասորդի փակում
                {
                    List<double> operationAccounts = CashBookOrderDB.GetOperationAccounts(cashbook, user.filialCode, user.AdvancedOptions["isHeadCashBook"]);
                    CreditAccount.AccountNumber = operationAccounts[1].ToString();
                    CreditAccount = Account.GetAccount(CreditAccount.AccountNumber);
                    if (CorrespondentAccountType == 1)//Հաճախորդի հաշիվ
                    {
                        DebitAccount = Account.GetAccount(DebitAccount.AccountNumber);
                    }
                    else if (CorrespondentAccountType == 2)//Դրամարկղ
                    {
                        DebitAccount = Account.GetSystemAccount(operationAccounts[0].ToString());
                    }
                }
                else
                {
                    List<double> operationAccounts = CashBookOrderDB.GetOperationAccounts(cashbook);
                    CreditAccount.AccountNumber = operationAccounts[0].ToString();
                    CreditAccount = Account.GetAccount(CreditAccount.AccountNumber);
                    if (CorrespondentAccountType == 1)
                    {
                        DebitAccount = Account.GetAccount(DebitAccount.AccountNumber);
                    }
                    //else if (CorrespondentAccountType == 2)
                    //{
                    //    //opperson
                    //}
                }
            }
        }

        /// <summary>
        /// Դրամարկղի մատյանի հայտի մուտք/ելք ի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(List<CashBook> cashBooks, User user)
        {
            ActionResult result = new ActionResult();

            foreach (CashBook cashBook in cashBooks)
            {
                result.Errors.AddRange(cashBook.Validate(user).Errors);
            }

            if (this.Type == OrderType.CashBookSurPlusDeficitClosing || this.Type == OrderType.CashBookSurPlusDeficitClosingApprove || this.Type == OrderType.CashBookDelete || this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing)
            {
                result.Errors.RemoveAll(m => m.Code == 774);
            }

            CashBook cashbook = CashBooks[0];

            if (cashbook.Type == 1)//ավելցուկ
            {
                if (CorrespondentAccountType == 1)
                {
                    Validation.ValidateCashBookReceiverAccount(this);
                }
                else if (CorrespondentAccountType == 2)
                {
                    result.Errors.AddRange(Validation.ValidateOPPerson(this, CreditAccount, DebitAccount.AccountNumber));
                }
            }
            else
            {
                if (CorrespondentAccountType == 1)
                {
                    result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                    if (result.Errors.Count == 0)
                    {
                        double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);

                        if (this.DebitAccount.IsCardAccount())
                        {
                            Card card = new Card();
                            card = Card.GetCardWithOutBallance(this.DebitAccount.AccountNumber);
                            if (card.ClosingDate == null)
                            {
                                KeyValuePair<String, double> arcaBalance = card.GetArCaBalance(user.userID);

                                if (arcaBalance.Key == "00" && arcaBalance.Value <= debitAccountBalance)
                                {
                                    debitAccountBalance = arcaBalance.Value;
                                }
                            }
                        }

                        if (this.Amount > debitAccountBalance)
                        {
                            string errorText = "գործարքը կատարելու";
                            if (Account.AccountAccessible(this.DebitAccount.AccountNumber, user.AccountGroup))
                            {
                                //{0} հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար(հասանելի գումար՝ {1},մնացորդ ՝ {2})
                                result.Errors.Add(new ActionError(1098, new string[] { this.DebitAccount.AccountNumber, this.DebitAccount.AvailableBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency, errorText }));
                            }
                            else
                            {
                                //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                                result.Errors.Add(new ActionError(1099, new string[] { this.DebitAccount.AccountNumber, errorText }));
                            }
                        }
                    }
                }
                else if (CorrespondentAccountType == 2 && this.Type != OrderType.CashBookSurPlusDeficitClosingApprove)
                {
                    result.Errors.AddRange(Validation.ValidateOPPerson(this, CreditAccount, DebitAccount.AccountNumber));
                }
            }
            if ((this.Type == OrderType.CashBookSurPlusDeficitClosing || this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing) && this.CorrespondentAccountType != 3)
            {
                Account debitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
                if (debitAccount == null)
                    debitAccount = Account.GetSystemAccount(this.DebitAccount.AccountNumber);

                Account creditAccount = Account.GetAccount(this.CreditAccount.AccountNumber);
                if (creditAccount == null)
                    creditAccount = Account.GetSystemAccount(this.CreditAccount.AccountNumber);

                if (debitAccount.Currency != creditAccount.Currency)
                {
                    //Դեբետ և Կրեդիտ հաշվների  արժույթները տարբեր են
                    result.Errors.Add(new ActionError(945));
                }
            }
            //todo stugum@ hanvel e Babken Makaryan
            //if (this.Type==OrderType.CashBookSurPlusDeficitClosing && IsSecondApproveOrReject(cashbook.ID))
            //{
            //    //Նշված տողը արդեն փակված է:
            //    result.Errors.Add(new ActionError(1112));
            //}
            decimal cashBookAmount = (decimal)CashBook.GetCashBookAmount(cashbook.ID);
            decimal amount = (decimal)cashbook.Amount;
            decimal maturedAmount = (decimal)cashbook.MaturedAmount;
            if (this.Type == OrderType.CashBookSurPlusDeficitClosing && (cashBookAmount - amount - maturedAmount) != 0)
            {

                result.Errors.Add(new ActionError(1510));
            }

            if (this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing && (cashBookAmount - amount - maturedAmount) < 0)
			{

                result.Errors.Add(new ActionError(1514));
            }


            if (this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing && amount == 0)
            {

                result.Errors.Add(new ActionError(1513));
            }



            if (this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing && (cashBookAmount - amount - maturedAmount) == 0)
            {
                result.Errors.Add(new ActionError(1511));

            }
            if ((this.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing || this.Type == OrderType.CashBookSurPlusDeficitClosing) && CashBook.HasUnconfirmedOrder(cashbook.ID) == true)
            {
                result.Errors.Add(new ActionError(1512));
            }

            return result;
        }

        /// <summary>
        /// Փոփոխել գրառումը
        /// </summary>
        /// <param name="cashBookID"></param>
        /// <returns></returns>
        public ActionResult ChangeCashBookStatus(int setnumber, int newStatus, User user)
        {
            ActionResult result = new ActionResult();
            if (CashBooks == null || CashBooks.Count < 1)
            {
                result.Errors.Add(new ActionError(772));
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            else
            {
                return new ActionResult() { ResultCode = ResultCode.Failed };
            }
        }

        public void Get()
        {
            CashBookOrderDB.Get(this);
        }

        /// <summary>
        /// Ստուգում է տվյալ տողի համար արդեն գոյություն ունի հաստատաման կամ մերժման տող
        /// </summary>
        /// <param name="linkedRowID"></param>
        /// <returns></returns>
        public static bool IsSecondApproveOrReject(int linkedRowID)
        {
            return CashBookOrderDB.IsSecondApproveOrReject(linkedRowID);
        }

        public static Account GetCashBookOperationSystemAccount(CashBookOrder order, OrderAccountType orderAccountType, ushort filialCode)
        {
            Account account = null;
            uint accountType = 0;
            CashBook cashbook = new CashBook();
            if (order.Type == OrderType.CashBookSurPlusDeficit)
            {
                cashbook = order.CashBooks[0];
            }
            if (order.Type == OrderType.CashBookSurPlusDeficit && orderAccountType == OrderAccountType.DebitAccount && cashbook.Type == 1)
            {
                accountType = 1;
            }
            else if (order.Type == OrderType.CashBookSurPlusDeficit && orderAccountType == OrderAccountType.CreditAccount && cashbook.Type == 1)
            {
                accountType = 3018;
            }
            else if (order.Type == OrderType.CashBookSurPlusDeficit && orderAccountType == OrderAccountType.DebitAccount && cashbook.Type == 3)
            {
                accountType = 1913;
            }
            else if (order.Type == OrderType.CashBookSurPlusDeficit && orderAccountType == OrderAccountType.CreditAccount && cashbook.Type == 3)
            {
                accountType = 1;
            }
            else if ((order.Type == OrderType.CashBookSurPlusDeficitClosing || order.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing) && orderAccountType == OrderAccountType.DebitAccount)
            {
                accountType = 3018;
            }
            else if ((order.Type == OrderType.CashBookSurPlusDeficitClosing || order.Type == OrderType.CashBookSurPlusDeficitPartiallyClosing) && orderAccountType == OrderAccountType.CreditAccount)
            {
                accountType = 1;
            }

            account = Account.GetOperationSystemAccount(accountType, order.Currency, filialCode);

            return account;
        }

    }
}
