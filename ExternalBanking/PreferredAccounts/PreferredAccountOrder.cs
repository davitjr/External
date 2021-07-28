using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.DBManager.PreferredAccounts;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking.PreferredAccounts
{
    public class PreferredAccountOrder : Order
    {
        public PreferredAccountServiceTypes? PreferredAccountServiceType { get; set; }

        public string AccountNumber { get; set; }

        public bool IsActive { get; set; }


        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;
            user = new User() { userID = 88 };
            OrderNumber = GenerateNextOrderNumber(CustomerNumber);
        }
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            if (PreferredAccountServiceType == PreferredAccountServiceTypes.PhoneNumberOrEmail)
            {
                CustomerMainData customer = ACBAOperationService.GetCustomerMainData(CustomerNumber);
                if (customer != null)
                {
                    if (customer.CustomerType == (int)CustomerTypes.physical)
                    {
                        if (customer.Phones.Count == 0 && customer.Emails.Count == 0)
                        {
                            //Ակտիվացումը հնարավոր չէ։ Խնդրում ենք դիմել Բանկ։
                            result.Errors.Add(new ActionError(1924));
                        }
                    }
                    else
                    {
                        //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                        result.Errors.Add(new ActionError(726));
                    }
                }
                else
                {
                    //Հաճախորդը գտնված չէ:
                    result.Errors.Add(new ActionError(771));
                }
            }
            return result;
        }

        public ActionResult SavePreferredAccountOrder(PreferredAccountOrder order, string userName)
        {
            Complete();
            ActionResult result = Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = PreferredAccountOrderDB.SavePreferredAccountOrder(order, userName);
                if (result.Id == 0)
                {
                    OrderDB.UpdateQuality(Id, OrderQuality.Declined);
                    SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
                    result.ResultCode = ResultCode.Failed;
                }
                scope.Complete();
            }
            return result;
        }

        public ActionResult ApprovePreferredAccountOrder(long doc_id)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = PreferredAccountOrderDB.ApprovePreferredAccountOrder(doc_id);
                OrderDB.UpdateQuality(doc_id, OrderQuality.Completed);
                scope.Complete();
            }
            return result;
        }

        public void Get()
        {
            PreferredAccountOrderDB.Get(this);
        }
    }

}
