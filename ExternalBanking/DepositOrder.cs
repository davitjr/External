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
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{


    public class DepositOrder : Order
    {

        public Deposit Deposit { get; set; }
        /// <summary>
        /// Ավանդի տեսակները
        /// </summary>
        public DepositType DepositType { get; set; }
        /// <summary>
        /// Ընթացիք հաշեհամար
        /// </summary>
      //  public override Account DebitAccount { get; set; }
        /// <summary>
        /// Տոկոսագումարի Հաշվեհամար
        /// </summary>
        public Account PercentAccount { get; set; }

        /// <summary>
        /// Ավանդային Հաշվեհամար
        /// </summary>
        public Account DepositAccount { get; set; }

        /// <summary>
        /// Որոշում է ավանդը երրորդ անձի համար է թե ոչ
        /// </summary>
        public ushort AccountType { get; set; }
        /// <summary>
        /// Երրորդ անձանանց հաճախորդի համարներ
        /// </summary>
        public List<KeyValuePair<ulong, string>> ThirdPersonCustomerNumbers { get; set; }
        /// <summary>
        /// Ավանդի ավտոմատ երկարաձգում
        /// </summary>
        public YesNo RecontractPossibility { get; set; }

        /// <summary>
        /// Ավանդի տոկոսադրույքը փոխվել է թե ոչ
        /// </summary>
        public bool InterestRateChanged { get; set; }

        /// <summary>
        /// Ավանդը ակցիայով է թե ոչ
        /// </summary>
        public bool IsActionDeposit { get; set; }

        /// <summary>
        /// Ավանդի ակցիան
        /// </summary>
        public DepositAction DepositAction { get; set; }
        /// <summary>
        ///Ավանդի հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
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
                result = DepositDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// Ավանդի հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateDepositOrderDocument(this, this.CustomerNumber));
            return result;


        }
        /// <summary>
        /// Վերադարձնում է ավանդի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            var depositOrder =  DepositDB.GetDepositOrder(this);


            if (depositOrder.DepositType == DepositType.BusinesDeposit)
            {
                var depositRates = GetRateForBusinessDeposits();
                foreach (var item in depositOrder.Deposit.DepositOption)
                {
                    item.Rate = depositRates.Where(x => x.Key == item.Type).Select(x => x.Value).First();
                }
            }
        }
        /// <summary>
        /// Ավանդի հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                {
                    Deposit.PrintDepositContract(this.Id, true, this.CustomerNumber);
                }

                if ((Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                 || (((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && OrderAttachment.HasAttachedFile(this.Id, 1))))
                {
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
        /// Ավանդի հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            DateTime operDay = Utility.GetNextOperDay().Date;
            if (Deposit.StartDate.Date != operDay.Date)
            {
                //Ավանդի սկիզբն ուղարկման պահին պետք է լինի operDay: 238
                result.Errors.Add(new ActionError(238, new string[] { operDay.ToString("dd/MM/yyyy") }));
            }
            else
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            Account mainAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
            double mainAccountBalance = Account.GetAcccountAvailableBalanceNotFreezed(mainAccount.AccountNumber, mainAccount.Currency) + Order.GetSentOrdersAmount(this.DebitAccount.AccountNumber, this.Source);
            if (mainAccountBalance < this.Amount)
            {

                // հաշվի մնացորդը չի բավարարում ավանդի ձևակերպումը կատարելու համար:
                result.Errors.Add(new ActionError(239, new string[] { this.DebitAccount.AccountNumber.ToString() }));
            }

           

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (InfoDB.CommunicationTypeExistence(this.CustomerNumber) == 1 && this.Deposit.StatementDeliveryType != null)
                {
                    //Դիմումը հնարավոր չէ ուղարկել, քանի որ հաղորդակցման եղանակը սխալ է ընտրված։ Անհրաժեշտ է մուտքագրել նոր դիմում։
                    result.Errors.Add(new ActionError(1732));
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
        public ActionResult CheckDepositOrderCondition()
        {
            ActionResult result = new ActionResult();

            bool isEmployeeDeposit = false;
            if (Deposit.CheckCustomerForEmployeeDeposit(this.CustomerNumber))
            {
                isEmployeeDeposit = true;
            }

            result = DepositDB.CheckDepositOrderCondition(this, isEmployeeDeposit);
            return result;
        }


        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Deposit.StartDate = Utility.GetNextOperDay();
            this.SubType = 1;
            //Հայտի համար
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.ThirdPersonCustomerNumbers == null)
            {
                this.ThirdPersonCustomerNumbers = new List<KeyValuePair<ulong, string>>();
            }

            if (this.AccountType != 1 && (this.ThirdPersonCustomerNumbers.Count > 0))
            {
                ACBAServiceReference.Customer jointCustomer;
                short jointCustomerType;

                List<KeyValuePair<ulong, string>> jointList = new List<KeyValuePair<ulong, string>>();


                this.ThirdPersonCustomerNumbers.ForEach(m =>
                {
                    if (m.Key.ToString().Length == 12)
                    {
                        jointCustomer = ACBAOperationService.GetCustomer(m.Key);
                        jointCustomerType = jointCustomer.customerType.key;

                        string jointCustomerFullName = "";

                        if (jointCustomer.identityId != 0 && jointCustomerType == (short)CustomerTypes.physical)
                        {
                            jointCustomerFullName = (jointCustomer as PhysicalCustomer).person.fullName.firstName + " " + (jointCustomer as PhysicalCustomer).person.fullName.lastName;

                        }

                        jointList.Add(new KeyValuePair<ulong, string>(key: m.Key, value: jointCustomerFullName));
                    }
                });
                this.ThirdPersonCustomerNumbers = jointList;


            }

            //Ավանդի տոկոսադրույք
            bool isEmployeeDeposit = false;

            if (Deposit.CheckCustomerForEmployeeDeposit(this.CustomerNumber))
            {
                isEmployeeDeposit = true;
            }

            if (this.AccountType == 3 && isEmployeeDeposit == false && this.ThirdPersonCustomerNumbers != null && this.ThirdPersonCustomerNumbers.Count > 0)
            {
                this.ThirdPersonCustomerNumbers.ForEach(m =>
                {
                    if (m.Key.ToString().Length == 12)
                    {
                        if (Deposit.CheckCustomerForEmployeeDeposit(m.Key))
                        {
                            isEmployeeDeposit = true;
                        }

                    }
                });

            }
            else if (this.AccountType == 2)
            {
                isEmployeeDeposit = false;
            }

            if (CheckDepositOrderCondition().Errors.Count == 0)
            {
                double interestRate = Deposit.GetDepositOrderCondition(this, source, isEmployeeDeposit).Percent;
                if (this.Source != SourceType.Bank)
                {
                    this.Deposit.InterestRate = interestRate;
                }
                else
                {

                    this.Deposit.InterestRate = this.Deposit.InterestRate / 100;




                    if (this.DepositType != DepositType.BusinesDeposit)
                    {
                        this.Deposit.DepositOption = new List<DepositOption>();
                    }

                    if (Convert.ToDecimal(this.Deposit.InterestRate) == (decimal)interestRate)
                    {
                        this.InterestRateChanged = false;
                    }
                    else
                    {
                        if (this.InterestRateChanged != true)
                        {
                            this.InterestRateChanged = true;
                        }
                    }


                }

            }

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                this.Currency = this.Deposit.Currency;
            }
        }

        /// <summary>
        /// Վերադարձնում է ակցիաները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<DepositAction> GetDepositActions(DepositOrder order)
        {
            return DepositDB.GetDepositActions(order);
        }

        /// <summary>
        /// Վերադարձնում է ակցիայով նախատեսված առավելագույն գումարը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static double GetDepositActionAmountMaxLimit(DepositOrder order)
        {
            return DepositDB.GetDepositActionAmountMaxLimit(order);
        }



        /// <summary>
        ///Ավանդի հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
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
                result = DepositDB.Save(this, userName, source);

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

            if (this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }
            result = base.Confirm(user);
            


            return result;
        }

        /// <summary>
        /// Ավանդի հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveDepositOrder(short schemaType, string userName, ACBAServiceReference.User user)
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

        private Dictionary<int, double> GetRateForBusinessDeposits()
        {
            DataTable dt = DepositDB.GetRateForBusinessDeposits(Deposit.Currency, Deposit.StartDate, Deposit.EndDate,CustomerNumber);
            
            Dictionary<int, double> dictionary = new Dictionary<int, double>
            {
                { 1, Convert.ToDouble(dt.Rows[0]["Type1"].ToString()) },
                { 2, Convert.ToDouble(dt.Rows[0]["Type2"].ToString()) },
                { 3, Convert.ToDouble(dt.Rows[0]["Type3"].ToString()) },
                { 4, Convert.ToDouble(dt.Rows[0]["Type4"].ToString()) },
                { 5, 0 },
                { 6, 0 }
            };
            Dictionary<int, double> types = dictionary;

            return types;

        }

        

    }


}