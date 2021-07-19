using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
   public class MatureOrder : Order
    {
        /// <summary>
        /// Մարման տեսակ
        /// </summary>
        public MatureType MatureType { get; set; }

        /// <summary>
        /// Մարման ենթատեսակ
        /// </summary>
        public int MatureMode { get; set; }

        /// <summary>
        /// Միայն տոկոսի մարման գումար
        /// </summary>
        public double PercentAmount { get; set; }

        /// <summary>
        /// Միայն տոկոսի մարման հաշիվ
        /// </summary>
        public Account PercentAccount { get; set; }

        /// <summary>
        /// Մայր գումարի և տոկոսի մարման հաշիվ
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set;}

        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Պրոդուկտի հաշիվ
        /// </summary>
        public Account ProductAccount { get; set;}

        /// <summary>
        /// Պրոդուկտի արժույթ
        /// </summary>
        public string ProductCurrency { get; set;}

        /// <summary>
        /// Պրոդուկտի տոկոսագումարի հաշվարկի օր
        /// </summary>
        public DateTime DayOfProductRateCalculation { get; set;}

       /// <summary>
       /// Խնդրահարույց վարկի մարում է թե ոչ
       /// </summary>
        public bool IsProblematic { get; set; }

        /// <summary>
        /// Վարկի մարման աղբյուր
        /// </summary>
        public ushort RepaymentSourceType { get; set; }

        /// <summary>
        /// Վարկի մարման աղբյուրի նկարագրություն
        /// </summary>
        public string RepaymentSourceTypeDescription { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        /// Միայն տոկոսի մարման գումարը Դրամով
        /// </summary>
        public double PercentAmountInAMD { get; set; }


        //public void SetMatureMode()
        //{
        //    if (this.MatureType == ExternalBanking.MatureType.PartialRepayment)
        //    {
        //        this.MatureType = ExternalBanking.MatureType.PartialRepaymentByGrafik;
        //        this.MatureMode = 1;
        //    }

        //}

        /// <summary>
        /// Մարման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete(source);

            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = MatureOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

       /// <summary>
       /// Մարման հայտի ուղարկում
       /// </summary>
       /// <param name="schemaType"></param>
       /// <param name="userName"></param>
       /// <param name="user"></param>
       /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = base.Approve(schemaType, userName);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }     

  
        /// <summary>
        /// Մարման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = MatureOrderDB.Save(this, userName, source);
                                               
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
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
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateMatureOrder(this));
            return result;


        }
        /// <summary>
        /// Մարման հայտի հերթական համարի ստացում
        /// </summary>
        /// <param name="source"></param>
        internal void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;

            if(source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
            {
                IsProblematic = false;
            }
            if (this.MatureType == ExternalBanking.MatureType.PartialRepaymentByGrafik)
            {
                this.MatureType = ExternalBanking.MatureType.PartialRepayment;
                this.SubType = 2;
                this.MatureMode = 1;
            }
            if (this.IsProblematic)
            {
                if (source != SourceType.SSTerminal)
                { 
                    if (this.MatureType == MatureType.ClaimRepayment)
                    {
                        this.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.ProductCurrency, user.filialCode);
                    }
                    else
                    {
                        this.Account = Account.GetProductAccount(this.ProductId, 18, 224);

                        //this.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.ProductCurrency, user.filialCode);
                        if (this.ProductCurrency != "AMD")
                        {
                            //this.PercentAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                            this.PercentAccount = Account.GetProductAccount(this.ProductId, 18, 279);
                        }
                    }
                }
            }


            else if (this.ProductType == ProductType.PaidFactoring && this.ProductCurrency != "AMD"
                && (this.PercentAccount == null || string.IsNullOrEmpty(this.PercentAccount.AccountNumber) || this.PercentAccount.AccountNumber == "0"))
            {
                this.PercentAccount = Account.GetProductAccount(this.ProductId, 18, 179);
                this.PercentAmount = 0;
            }


            //Հայտի համար
            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if (this.Source != SourceType.Bank)
                this.RepaymentSourceType = 1;//Սեփական միջոցների հաշվին


        }
       /// <summary>
       /// Հայտի ուղարկման ստուգումներ
       /// </summary>
       /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Source!=SourceType.Bank && Source != SourceType.MobileBanking && Source != SourceType.SSTerminal && Source != SourceType.CashInTerminal && this.DayOfProductRateCalculation < Utility.GetNextOperDay() )
            {
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(new ActionError(157));
            }
            
            if (this.SubType == 5)
            {

                if (this.IsProblematic)
                {
                    //Տարանցիկ հաշվի մնացորդի ստուգումներ:

                    double debitAccountBalance = Account.GetAcccountAvailableBalance(this.Account.AccountNumber);

                    if (debitAccountBalance < this.Amount)
                    {
                        result.ResultCode = ResultCode.ValidationError;
                        result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.Account.Currency, this.Account.Balance.ToString("#,0.00") + " " + this.Account.Currency }));
                    }

                    if (this.ProductCurrency != "AMD")
                    {
                        double percentAccountBalance = Account.GetAcccountAvailableBalance(this.PercentAccount.AccountNumber);

                        if (percentAccountBalance < this.PercentAmount)
                        {
                            result.ResultCode = ResultCode.ValidationError;
                            result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.Account.Currency, this.Account.Balance.ToString("#,0.00") + " " + this.Account.Currency }));
                        }

                    }


                }
                else
                {
                    double debitAccountBalance = Account.GetAcccountAvailableBalance(this.Account.AccountNumber);

                    if (Convert.ToDecimal(debitAccountBalance) < Convert.ToDecimal(this.Amount))
                    {
                        if (Account.AccountAccessible(this.Account.AccountNumber, user.AccountGroup))
                        {
                            //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                            result.Errors.Add(new ActionError(499, new string[] { this.Account.AccountNumber.ToString(), debitAccountBalance.ToString("#,0.00") + " " + this.Account.Currency, this.Account.Balance.ToString("#,0.00") + " " + this.Account.Currency }));
                        }
                        else
                        {
                            //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                            result.Errors.Add(new ActionError(788, new string[] { this.Account.AccountNumber }));
                        }
                    }
                }
            }

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                List<ActionError> checkMature = MatureOrder.CheckMature(this);
                result.Errors.AddRange(checkMature);
            }

            //if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking || Source ==
            //    SourceType.ArmSoft || Source == SourceType.AcbaOnlineXML)
            //{
            //    if (this.MatureMode == 1 && this.ProductType != ProductType.AparikTexum && this.ProductType != ProductType.Loan)
            //    {
            //        //Նշված տեսակի վարկերի համար հնարավոր չէ մարում կատարել online/mobile համակարգերով։
            //        result.ResultCode = ResultCode.ValidationError;
            //        result.Errors.Add(new ActionError(1821));
            //    }
            //}

            if (result.Errors.Count==0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        /// <summary>
        /// Մարման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> CheckMature(MatureOrder order)
        {
            return MatureOrderDB.CheckMature(order);
        }
        /// <summary>
        /// Մարման հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            MatureOrderDB.Get(this);
        }

        public static double GetThreeMonthLoanRate(ulong productId)
        {
            return MatureOrderDB.GetThreeMonthLoanRate(productId);
        }

        /// <summary>
        /// Ստուգում է տվյալ վարկի գծով առկա է վաճառված գրավ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckLoanEquipment(ulong productId)
        {
            return MatureOrderDB.CheckLoanEquipment(productId);
        }


        public static double GetLoanMatureCapitalPenalty(MatureOrder order, ACBAServiceReference.User user)
        {
            if (order.IsProblematic)
            {
                if (order.MatureType == MatureType.ClaimRepayment)
                {
                    order.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.DebitAccount), order.ProductCurrency, user.filialCode);
                }
                else
                {
                    order.Account = Account.GetProductAccount(order.ProductId, 18, 224);

                    if (order.ProductCurrency != "AMD")
                    {
                        order.PercentAccount = Account.GetProductAccount(order.ProductId, 18, 279);
                    }
                }
            }

            return MatureOrderDB.GetLoanMatureCapitalPenalty(order, user);
        }

       
    }
}
