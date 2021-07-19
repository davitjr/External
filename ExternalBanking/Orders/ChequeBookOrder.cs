using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{
    public class ChequeBookOrder : Order
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account ChequeBookAccount { get; set; }
        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public string FeeAmount { get; set; }
        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Չեկային գրքույկի տիրոջ անուն ազգանուն
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Չեկային գրքույքի ստացման հայտ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result =ChequeBookDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// Չեկային գրքույքի ստացման հայտ ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateChequeBookOrder(this));
            return result;
        }
        /// <summary>
        /// Չեկային գրքույքի ստացման հայտի հաստատում:
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

            return result;
        }
        /// <summary>
        /// Վերադարձնում է չեկային գրքույքի հայտի տվյալները
        /// </summary>
        public void Get()
        {
            ChequeBookDB.Get(this);
        }
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            if (IsExistRequest(OrderType.ChequeBookOrder, ChequeBookAccount.AccountNumber.ToString(), CustomerNumber) == true)
            {
                //Տվյալ ընթացիք հաշվի համար գոյություն ունի չեկային գրքույքի չհաստատված հայտ:
                result.Errors.Add(new ActionError(397));
            }
            if (this.Fees[0].Type == 17)
            {


                result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));


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

        private void Complete()
        {
            
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.SubType = 1;
            byte customerType = Customer.GetCustomerType(this.CustomerNumber);

                this.Fees.ForEach(m =>
                {

                    m.CreditAccount = Account.GetOperationSystemAccount(3000, "AMD", this.FilialCode, Convert.ToUInt16(customerType==6?1:2));
                    if (m.Type == 16)
                    {
                        m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", this.FilialCode);
                        m.OrderNumber = this.OrderNumber;
                    }
                    
                });
        }
        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();
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
                result = ChequeBookDB.Save(this, userName, source);
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

                result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && this.Fees[0].Type == 16)
                {
                    warnings.AddRange(user.CheckForNextCashOperation(this));
                }
            }
            catch
            {

            }
           

            result.Errors = warnings;
            return result;
        }

        public static double GetOrderServiceFee(ulong customerNumber,OrderType type,int urgent)
        {
            double serviceFee = 0;
            int customerType = 0;
            int vipType = 0;

            customerType = Customer.GetCustomerType(customerNumber);

            VipData vip = ACBAOperationService.GetCustomerVipData(customerNumber);
            vipType = vip.vipType.key;
            
            if (vipType<7 || vipType>9)
            {
                if (type==OrderType.ChequeBookReceiveOrder)
                {
                    if (customerType == 6)
                      serviceFee = Utility.GetPriceInfoByIndex(201, "price");
                    else
                      serviceFee = Utility.GetPriceInfoByIndex(202, "price");
                }
                else if (type==OrderType.ReferenceOrder)
                {
                    if (urgent==0)
                    {
                        serviceFee = Utility.GetPriceInfoByIndex(205, "price");
                    }
                    else if(urgent==1)
                    {
                        serviceFee = Utility.GetPriceInfoByIndex(214, "price");
                    }
                }
                else if (type== OrderType.SwiftCopyOrder)
                {
                    serviceFee = Utility.GetPriceInfoByIndex(213, "price");
                }
            }
            return serviceFee;
        }
    }
}
