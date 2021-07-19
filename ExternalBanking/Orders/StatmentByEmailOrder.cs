using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Transactions;

namespace ExternalBanking
{
    public class StatmentByEmailOrder:Order
    {
        /// <summary>
        /// Email ների ցանկ
        /// </summary>
        public string MainEmail { get; set; }
        public string SecondaryEmail { get; set; }
        /// <summary>
        /// Հաշվեհամարների ցանկ
        /// </summary>
        public List<Account> Accounts { get; set; }
        /// <summary>
        /// Պարբերականություն
        /// </summary>
        public int Periodicity { get; set; }

        /// <summary>
        /// Պարբերականություն
        /// </summary>
        public string PeriodicityDescription { get; set; }

        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի պահպանում
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
                result = StatmentByEmailOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }
        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateStatmentByEmailOrder(this));
            return result;
        }
        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի հաստատում
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
        /// Քաղվածքների էլեկտրոնային ստացման հայտի հաստատման ստուգում
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
        /// Վերադարձնում է քաղվածքների էլեկտրոնային ստացման հայտի տվյալները
        /// </summary>
        public void Get()
        {
            StatmentByEmailOrderDB.Get(this);
        }

         /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (this.OrderNumber == null || this.OrderNumber == "") 
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.SubType = 1;

            if(Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
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
                result = StatmentByEmailOrderDB.Save(this, userName, source);
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
            result=base.Confirm(user);
           

            if(Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);

            }

            return result;
        }
          
    }
}
