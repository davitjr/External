using ExternalBanking.DBManager;
using ExternalBanking.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class LinkPaymentOrder : Order
    {
        /// <summary>
        /// Փոխանցման տեսակ
        /// </summary>
        public LinkPaymentSourceType PaymentSourceType { get; set; }

        /// <summary>
        /// Գեներացված հղման url-ը։
        /// </summary>
        public string LinkURL { get; set; }

        /// <summary>
        /// Հղման url-ի 16ն նիշ
        /// </summary>
        public string ShortId { get; set; }

        /// <summary>
        /// Նկարագրություն – լրացվում է հաճախորդի կողմից (ոչ պարտադիր դաշտ)
        /// </summary>
        public string LinkPaymentDescription { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ դրամարկղ) հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// if 1 → ակտիվ 0 → ժամկետանց
        /// </summary>
        public bool Activ { get; set; }

        /// <summary>
        /// User-ի ունիկալ համար
        /// </summary>
        public double UserId { get; set; }

        /// <summary>
        /// User-ի նկար
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(ACBAServiceReference.User user)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.LinkPaymentOrder;
            this.SubType = 1;
            this.Description = "Հղումով փոխանցում";
            this.Receiver = Account.GetAccountDescription(CreditAccount.AccountNumber);
            this.Currency = "AMD";

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }

            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            return result;
        }

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
           
            result.ResultCode = result.Errors.Count > 0 ? ResultCode.ValidationError : ResultCode.Normal;

            return result;
        }

        public ActionResult Save()
        {
            this.Complete(user);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = LinkPaymentOrderDB.Save(this);

                LogOrderChange(user, action);

                if (result.ResultCode != ResultCode.Normal)
                    return result;
                else
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հղումով փոխանցում հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ContentResult<string> Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ContentResult<string> result = new ContentResult<string>();
            ActionResult validationResult = ValidateForSend();
            result.Errors = validationResult.Errors;
            result.ResultCode = validationResult.ResultCode;

            if (result.ResultCode == ResultCode.Normal)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    ActionResult res = base.Approve(schemaType, userName);
                    if (res.ResultCode == ResultCode.Normal)
                    {
                        this.Quality = OrderQuality.Sent3;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                    }

                    this.ShortId = ShortIdHelper.Generate(true, false);
                    //this.LinkURL = "acba_" + this.ShortId;
                    string url = ConfigurationManager.AppSettings["LinkPaymentURL"];
                    this.LinkURL = url + this.ShortId;

                    while (LinkPaymentOrderDB.CheckShortId(this.ShortId))
                        ShortIdHelper.Generate(true, false);

                    this.PaymentSourceType = LinkPaymentSourceType.FromLinkPayment;

                    LinkPaymentOrderDB.SaveLink(this);
                    result.Content = this.LinkURL;
                    res = OrderDB.UpdateHBdocumentQuality(this.Id, user);
                    if (res.ResultCode != ResultCode.Normal)
                    {
                        result.Errors = res.Errors;
                        result.ResultCode = res.ResultCode;
                        result.Content = string.Empty;
                        return result;
                    }

                    if (result.ResultCode == ResultCode.Normal)
                    {
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
            }


            return result;
        }

        public static LinkPaymentOrder Get(long docId) => LinkPaymentOrderDB.GetDetails(docId);

        public static LinkPaymentOrder GetLinkPaymentOrderWithShortId(string ShortId) => LinkPaymentOrderDB.GetLinkPaymentOrderWithShortId(ShortId);


    }


}
