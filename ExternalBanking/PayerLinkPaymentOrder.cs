using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class PayerLinkPaymentOrder : Order
    {
        /// <summary>
        /// Հղման url-ի 16ն նիշ
        /// </summary>
        public string ShortId { get; set; }

        /// <summary>
        /// CheckBox 
        /// </summary>
        public bool CheckBox { get; set; }

        /// <summary>
        /// Վճարողի Անուն ազգանուն
        /// </summary>
        public string PayerName { get; set; }

        /// <summary>
        /// Թողարկող բանկի երկրի կոդը
        /// </summary>
        public string BankCountryCode { get; set; }

        /// <summary>
        /// Թողարկող բանկի երկրի անվանումը լեզվով փոխանցվել է հարցման լեզվի պարամետրով, կամ մեթոդը կանչած օգտվողի լեզվով, եթե լեզուն նշված չէ հարցման մեջ
        /// </summary>
        public string BankCountryName { get; set; }

        /// <summary>
        /// վճարողի քարտի համար
        /// </summary>
        public string PayerCard { get; set; }

        /// <summary>
        /// Arca-ի Տերմինալ համար/Terminal ID
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// Arca-ի ունիկալ համար (Գործարքի համար/TRN)
        /// </summary>
        public string ArcaOrderNumber { get; set; }

        /// <summary>
        /// Պատվերի կարգավիճակը վճարային համակարգում:
        /// </summary>
        public short OrderStatus { get; set; }

        /// <summary>
        /// Արձագանքի կոդ: 0 Approve
        /// </summary>
        public short ActionCode { get; set; }

        /// <summary>
        /// Քարտի գործողության ժամկետը ձևաչափով YYYYMM.Նշված է միայն պատվերի վճարումից հետո
        /// </summary>
        public string Expiration { get; set; }

        /// <summary>
        /// Arca-ի Հ.կոդ/Auth ID
        /// </summary>
        public string ArcaAuthId { get; set; }

        /// <summary>
        /// Arca-ի Հարցման համար
        /// </summary>
        public string RRN { get; set; }

        /// <summary>
        /// Arca-ի Հարցման համար
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.LinkTransferPaymentConfirmation;
            this.SubType = 1;
            this.Description = "Հղումով փոխանցման վճարման կատարում";
            this.Quality = OrderQuality.Draft;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = "0";

        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (IfExistsLinkPaymentWithArcaOrderId(this.ArcaOrderNumber, this.ShortId))
            {
                //Տվյալ վճարման համար արդեն առկա է մուտքագրված հայտ։
                result.Errors.Add(new ActionError(2035));
                return result;
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

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            //ActionResult result = new ActionResult();// = this.Validate(user);
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            //result = this.ValidateForSend(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PayerLinkPaymentOrderDB.Save(this);

                if (result.ResultCode != ResultCode.Normal)
                    return result;
                else
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                if (result.ResultCode != ResultCode.Normal)
                    return result;

                result = base.Approve(schemaType, userName);


                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);


                    LogOrderChange(user, Action.Update);
                    result.Id = this.Id;
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }
            }
            //base.Confirm(user);

            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք առկա է տվյալ ArCa վճարման կոդով գործարք, թե ոչ
        /// </summary>
        /// <param name="arCaOrderId"></param>
        /// <param name="shortId"></param>
        /// <returns></returns>
        public static bool IfExistsLinkPaymentWithArcaOrderId(string arCaOrderId, string shortId)
        {
            return PayerLinkPaymentOrderDB.IfExistsLinkPaymentWithArcaOrderId(arCaOrderId, shortId);
        }


    }
}
