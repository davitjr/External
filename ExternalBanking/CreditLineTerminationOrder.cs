using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Configuration;
using ExternalBanking.CreditLineActivatorARCA;

namespace ExternalBanking
{
    public class CreditLineTerminationOrder : Order
    {
        /// <summary>
        /// Վարկային գծի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Վարկային գծի արժույթ
        /// </summary>
        //public new string Currency { get; set; }

        /// <summary>
        /// Վարկային գծի հաշիվ
        /// </summary>
        public Account CreditLineAccount { get; set; }


        /// <summary>
        /// Վարկային գծի դադարեցման հայտ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            List<ActionError> warnings = new List<ActionError>();
            this.Complete();

            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {              
                result = CreditLineDB.SaveCreditLineTerminationOrder(this, userName, source, user.filialCode);

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

            if(this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }

            // Վարկային գծի դադարեցում հայտի կատարման ժամանակ

            bool isCreditLineOnline = bool.Parse(ConfigurationManager.AppSettings["IsCreditLineOnline"].ToString());

            if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CreditLineMature && isCreditLineOnline == true && source == SourceType.Bank)
            {
                try
                {
                    warnings = ChangeExceedLimitRequest.CloseCreditLine(CustomerNumber, ProductId, Id, user.userID);
                }
                catch
                {
                    warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                }

            }

            result.Errors = warnings;
            return result;
        }


        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CreditLineDB.SaveCreditLineTerminationOrder(this, userName, source, user.filialCode);

                ActionResult resultOPPerson = base.SaveOrderOPPerson();

                if (resultOPPerson.ResultCode != ResultCode.Normal)
                {
                    return resultOPPerson;
                }



                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            CreditLineDB.GetCreditLineTerminationOrder(this);
        }



        /// <summary>
        /// Վարկային գծի դադարեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCreditLineTerminationOrder(this, user));
            return result;
        }

        /// <summary>
        /// Լրացնում է վարկային գծի դադարեցման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.SubType = 1;
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            CreditLine creditLine = CreditLineDB.GetCreditLine(this.ProductId, this.CustomerNumber);
            this.Currency = creditLine.Currency;
            this.CreditLineAccount = creditLine.LoanAccount;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        public static CreditLineTerminationOrder order(ulong productId, ulong customerNumber)
        {
            CreditLineTerminationOrder order = new CreditLineTerminationOrder();
            CreditLine creditLine = CreditLineDB.GetCreditLine(productId, customerNumber);
            order.ProductId = productId;
            order.Currency = creditLine.Currency;
            order.CreditLineAccount = creditLine.LoanAccount;

            return order;
        }





        /// <summary>
        /// Վարկային գծի դադարեցման հայտի հաստատում
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
        /// Վարկային գծի դադարեցման հայտի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(785));
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {             
                //ԱրՔա - ում հաշվի ստուգում               
                    try
                    {
                        if (!ChangeExceedLimitRequest.ChekArCaBalance(this.ProductId))
                            result.Errors.Add(new ActionError(1829));
                    }
                    catch { }
                

                CreditLine creditLine = CreditLineDB.GetCreditLine(this.ProductId, this.CustomerNumber);
                string cardNumber = CreditLine.GetCreditLineCardNumber(this.ProductId);
                int cardType = Card.GetCardType(cardNumber, this.CustomerNumber);

                if (cardType == 40 || cardType == 41)   //Amex Blue և Amex Gold քարտատեսակներով դադարեցման հնարավորության բացառում
                {
                    //Նշված տեսկի քարտատեսակի համար հնարավոր չէ ուղարկել դադարեցման հայտ։
                    result.Errors.Add(new ActionError(1449));
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
    }
}
