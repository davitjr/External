using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Չեկային գրքույկի ստացման հայտ
    /// </summary>
    public class ChequeBookReceiveOrder : Order
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account ChequeBookAccount { get; set; }

        /// <summary>
        /// Էջի սկիզբ
        /// </summary>
        public string PageNumberStart { get; set; }

        /// <summary>
        /// Էջի վերջ
        /// </summary>
        public string PageNumberEnd { get; set; }

        /// <summary>
        /// Չեկային գրքույկի ստացման հայտի միջնորդավճար 
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Միջնորդավճարի տարանցիկ հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Ինքնարժեք
        /// </summary>
        public double CostPrice { get; set; }

        /// <summary>
        /// Չեկային գրքույկի տիրոջ անուն ազգանուն
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Չեկային գրքույքի ստացման պահպանում և ողղարկում
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

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ChequeBookReceiveOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }
                base.SaveOrderFee();
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
        /// Վերադարձնում է չեկային գրքույքի ստացման հայտի տվյալները
        /// </summary>
        public void Get()
        {
            ChequeBookReceiveOrderDB.Get(this);
        }

        private void Complete()
        {
            byte customerType = Customer.GetCustomerType(this.CustomerNumber);
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.Fees = new List<OrderFee>();
            this.Fees.Add(new OrderFee { Account = this.FeeAccount, Amount = this.FeeAmount, Currency = "AMD", Type = 17 });
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateChequeBookReceiveOrder(this));
            return result;
        }
        public ActionResult ValidateForSend(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            List<KeyValuePair<Account, double>> list = new List<KeyValuePair<Account, double>>();
            list.Add(new KeyValuePair<Account, double>(this.FeeAccount, Convert.ToDouble(this.FeeAmount)));
            result.Errors.AddRange(Validation.ValidateOrderAmount(user, this.Source, list, this.Type));
            return result;
        }

        /// <summary>
        /// Չեկային գրքույկի ստացման հայտի միջնորդավճար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static double GetOrderServiceFee(ulong customerNumber)
        {

            return ChequeBookReceiveOrderDB.GetOrderServiceFee(customerNumber);
        }

        /// <summary>
        /// Ստուգում է չեկային գրքույկի պատվիրման հայտ առկա է թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static bool HasChequeBookOrder(ulong customerNumber, string accountNumber)
        {

            return ChequeBookReceiveOrderDB.HasChequeBookOrder(customerNumber, accountNumber);

        }

        /// <summary>
        /// Չեկային գրքույքի վերաբերյալ զգուշացումներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static List<string> GetChequeBookReceiveOrderWarnings(ulong customerNumber, string accountNumber, Culture culture)
        {

            List<string> warnings = new List<string>();

            if (customerNumber != 0 && accountNumber != null && !HasChequeBookOrder(customerNumber, accountNumber))
            {
                warnings.Add(Info.GetTerm(721, new string[] { }, culture.Language));
            }
            return warnings;
        }

    }













}
