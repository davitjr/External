using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class InternationalOrderTemplate :Template
    {

        public int InternationalOrderTemplateId { get; set; }

        public InternationalPaymentOrder InternationalPaymentOrder { get; set; }

        protected void Complete(SourceType source)
        {
            this.TemplateRegistrationDate = DateTime.Now.Date;
            this.TemplateSourceType = source;
            this.InternationalPaymentOrder.Type = this.TemplateDocumentType;
            this.InternationalPaymentOrder.SubType = this.TemplateDocumentSubType;
            if (this.InternationalPaymentOrder.DebitAccount != null && this.InternationalPaymentOrder.Type != OrderType.CashInternationalTransfer)
            {
                this.InternationalPaymentOrder.DebitAccount = Account.GetAccount(this.InternationalPaymentOrder.DebitAccount.AccountNumber);
            }
            else if (this.InternationalPaymentOrder.Type == OrderType.CashInternationalTransfer)
            {
                //Տարանցիկ հաշվի լրացում
                this.InternationalPaymentOrder.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this.InternationalPaymentOrder, OrderAccountType.DebitAccount), this.InternationalPaymentOrder.Currency, InternationalPaymentOrder.user.filialCode);
            }
            this.TemplateCustomerNumber = this.InternationalPaymentOrder.CustomerNumber;
        }
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            this.Complete(source);
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.InternationalPaymentOrder.Validate(this.TemplateType);
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                }
                else
                {
                    Action action;
                    if (this.ID > 0)
                    {
                        action = Action.Update;
                    }
                    else
                    {
                        action = Action.Add;
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                    {
                        result = InternationalOrderTemplateDB.SaveInternationalOrderTemplate(this, action);

                        scope.Complete();
                    }
                }
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        public static InternationalOrderTemplate Get(int internationalOrderTemplateId, ulong customerNumber)
        {
            return InternationalOrderTemplateDB.GetInternationalOrderTemplate(internationalOrderTemplateId, customerNumber);
        }



    }
}
