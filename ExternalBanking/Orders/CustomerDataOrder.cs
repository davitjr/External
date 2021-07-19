using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class CustomerDataOrder:Order
    {
        /// <summary>
        /// Քաղաքային հեռախոսահամար
        /// </summary> 
        public string HomePhoneNumber { get; set; }
        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhoneNumber { get; set; }
        /// <summary>
        ///  Էլ.հասցե
        /// </summary>
        public List<string> EmailAddress { get; set; }
        /// <summary>
        /// Գաղտանաբառ
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
         public ActionResult Save(string userName, SourceType source,ACBAServiceReference.User user)
        {
            //ActionResult result = this.Validate();
            //if (result.Errors.Count > 0)
            //{
            //    result.ResultCode = ResultCode.ValidationError;
            //    return result;
            //}
            //result =CustomerDataOrderDB.Save(this, userName, source);
            //return result;
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
                result =CustomerDataOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
         /// Տվյալների խմբագրման հայտի ստուգումներ
        /// </summary>
        /// <returns></returns>
         public ActionResult Validate()
         {
             ActionResult result = new ActionResult();
             result.Errors.AddRange(Validation.ValidateCustomerDataOrder(this));
             return result;
         }
        /// <summary>
         /// Տվյալների խմբագրման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
         public new ActionResult Approve(short schemaType, string userName,ACBAServiceReference.User user)
         {
             //ActionResult result = ValidateForSend();
             //if (result.Errors.Count > 0)
             //{
             //    result.ResultCode = ResultCode.ValidationError;
             //}
             //else
             //{
             //    result = base.Approve(schemaType, userName);
             //    result.ResultCode = ResultCode.Normal;
             //}
             //return result;
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
         /// Տվյալների խմբագրման հայտի հաստատման ստուգում
        /// </summary>
        /// <returns></returns>
         public ActionResult ValidateForSend()
         {
             ActionResult result = new ActionResult();
             if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
             {
                 //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                 result.Errors.Add(new ActionError(451));
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
         /// Վերադարձնոմ է տվյալների խմբագրման հայտի տվյալները
        /// </summary>
         public void Get()
         {
             CustomerDataOrderDB.Get(this);
         }
         /// <summary>
         /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
         /// </summary>
         private void Complete()
         {
             if (this.OrderNumber == null || this.OrderNumber == "")
                 this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
             this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                this.RegistrationDate = DateTime.Now;
            }
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
                 result =CustomerDataOrderDB.Save(this, userName, source);
                 if (result.ResultCode != ResultCode.Normal)
                 {
                     return result;
                 }
                 else
                 {
                     base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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
            
            

             return result;
         }
    }
}
