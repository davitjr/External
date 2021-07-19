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
    /// <summary>
    /// Պարբերական SWIFT-ով ուղարկվող քաղվածք
    /// </summary>
    public class PeriodicSwiftStatementOrder:PeriodicOrder
    {
        /// <summary>
        /// SWIFT քաղվածքի պարբերական փոխանցման ստացող բանկի SWIFT կոդ
        /// Առնվազն 12 նիշ:
        /// </summary>
        public string ReceiverBankSwiftCode { get; set; }

        /// <summary>
        /// SWIFT քաղվածքի հաշիվ
        /// </summary>
        public Account StatementAccount { get; set; }

        /// <summary>
        /// Շրջանառության առկայություն
        /// </summary>
        public bool ExistenceOfCirculation { get; set; }


        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.Currency = this.StatementAccount.Currency;
            this.DebitAccount = this.StatementAccount;
        }

        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            PeriodicTransferOrderDB.GetPeriodicSwiftStatementOrder(this);
        }

        /// <summary>
        /// Հայտի պահմապնման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(ValidatePeriodicOrder());
            if (this.Currency == "GEL")
            {
                //{0} արժույթով գործարք կատարել հնարավոր չէ:
                result.Errors.Add(new ActionError(1478, new string[] { this.Currency }));
            }
            if (!string.IsNullOrEmpty(this.ReceiverBankSwiftCode) && this.ReceiverBankSwiftCode.Length<12)
            {
                //S.W.I.F.T . կոդը պետք է լինի առնվազն 12 նիշ:
                result.Errors.Add(new ActionError(1419));
            }

            return result;
        }


        /// <summary>
        /// Նոր պարբերականի հայտի հաստատում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);

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
                result = PeriodicTransferOrderDB.SavePeriodicSwiftStatementOrder(this, userName, source);
                if (this.Fee > 0 && this.FeeAccount != null || (this.ChargeType == 2 && this.Fee == -1 && this.FeeAccount != null))
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee fee = new OrderFee();
                    fee.Amount = this.Fee;
                    fee.Account = this.FeeAccount;
                    fee.Currency = this.FeeAccount.Currency;
                    fee.Type = 25;
                    this.Fees.Add(fee);
                }
                base.SaveOrderFee();
               
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
        /// Հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrderForSend().Errors);
            return result;
        }


        /// <summary>
        /// Պարբեարականի հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


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
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicSwiftStatementOrder(string userName, SourceType source, ACBAServiceReference.User user)
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
                result = PeriodicTransferOrderDB.SavePeriodicSwiftStatementOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }

    }
}
