using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class UtilityPaymentOrderTemplate : Template
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int UtilityPaymentOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխանցում
        /// </summary>
        public UtilityPaymentOrder UtilityPaymentOrder { get; set; }

        /// <summary>
        /// Կոմունալ վճարման՝ որպես խմբային վճարման ծառայություն պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            this.Complete(source);
            this.UtilityPaymentOrder.Complete();
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.UtilityPaymentOrder.Validate(user, this.TemplateType);
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
                        result = UtiilityPaymentOrderTemplateDB.SaveUtilityPaymentOrderTemplate(this, action);                      
                        if(result.ResultCode == ResultCode.Normal )
                        {
                            if (TemplateType == TemplateType.CreatedAsGroupService)
                                OrderGroupDB.SaveGroupTemplateShortInfo(GroupTemplateShrotInfo,result.Id, action);
                        }
                        scope.Complete();
                    }
                }
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Ձևանմուշի դաշտերի ավտոմատ լրացում
        /// </summary>
        public void Complete(SourceType source)
        {
            this.TemplateSourceType = source;
            this.UtilityPaymentOrder.Type = this.TemplateDocumentType;
            this.UtilityPaymentOrder.SubType = this.TemplateDocumentSubType;

            ushort abonentType = ushort.Parse(this.UtilityPaymentOrder.AbonentType.ToString());

            if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.COWater)
            {
                string branchID = SearchCommunal.GetCOWaterBranchID(this.UtilityPaymentOrder.Branch, this.UtilityPaymentOrder.AbonentFilialCode.ToString());
                this.UtilityPaymentOrder.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this.UtilityPaymentOrder, OrderAccountType.CreditAccount), "AMD", this.UtilityPaymentOrder.AbonentFilialCode, abonentType, "0", branchID);
            }
            else
            {
                this.UtilityPaymentOrder.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this.UtilityPaymentOrder, OrderAccountType.CreditAccount), "AMD", this.UtilityPaymentOrder.user.filialCode, abonentType, "0", this.UtilityPaymentOrder.Branch);
            }

            if (TemplateType == TemplateType.CreatedAsGroupService)
            {
                GroupTemplateShrotInfo shortinfo = new GroupTemplateShrotInfo
                {
                    DebitAccount = UtilityPaymentOrder.DebitAccount.AccountNumber,
                    CustomerId = UtilityPaymentOrder.Code,
                    GroupId = TemplateGroupId
                };
                this.GroupTemplateShrotInfo = shortinfo;
            }

        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="paymentOrderTemplateId"></param>
        /// <returns></returns>
        public static UtilityPaymentOrderTemplate Get(int paymentOrderTemplateId, ulong customerNumber)
        {
            return UtiilityPaymentOrderTemplateDB.GetUtilityPaymentOrderTemplate(paymentOrderTemplateId, customerNumber);
        }

    }
}
