using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class AccountDataChangeOrder : Order
    {
        /// <summary>
        /// Խմբագրման ենթակա հաշվի խմբագրվող լրացուցիչ տվյալներ
        /// </summary>
        public AdditionalDetails AdditionalDetails { get; set; }

        /// <summary>
        /// Խմբագրման ենթակա հաշվեհամար
        /// </summary>
        public Account DataChangeAccount { get; set; }


        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            int filialCode = user.filialCode;
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountDataChangeOrderDB.Save(this, userName, source, filialCode);
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
        /// Հաշվի տվյալների խմբագրման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAccountDataChangeOrder(this));
            return result;
        }

        /// <summary>
        /// Հաշվի տվյալների խմբագրման կրկնակի հայտի առկայություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondDataChange(ulong customerNumber, string accountNumber)
        {
            bool secondDataChange = AccountDataChangeOrderDB.IsSecondDataChange(customerNumber, accountNumber);
            return secondDataChange;
        }

        /// <summary>
        /// Խմբագրման ենթակա նույն լրացուցիչ տվյալի ու արժեքի  առկայություն
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="additionalDetails"></param>
        /// <returns></returns>
        internal static bool IsSameAdditionalDataChange(string accountNumber, AdditionalDetails additionalDetails)
        {
            bool isSameType = AccountDataChangeOrderDB.IsSameAdditionalDataChange(accountNumber, additionalDetails);
            return isSameType;
        }

        /// <summary>
        /// Հաճախորդի մյուս հաշիվներում սպասարկման վարձի գանձման նշանի առկայություն
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="additionalDetails"></param>
        /// <returns></returns>
        internal static bool IfExistsServiceFeeCharge(AccountDataChangeOrder order)
        {
            bool exists = AccountDataChangeOrderDB.IfExistsServiceFeeCharge(order);
            return exists;
        }

        /// <summary>
        /// Լրացնում է հաշվի տվյալների խմբագրման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.DataChangeAccount = Account.GetAccount(this.DataChangeAccount.AccountNumber);
            this.AdditionalDetails.AdditionalValueType = AccountDataChangeOrderDB.GetAccountAdditionValueType(this.AdditionalDetails.AdditionType);

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            AccountDataChangeOrderDB.Get(this);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լրացուցիչ տվյալները
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAccountAdditionsTypes(ACBAServiceReference.User user)
        {

            Dictionary<string, string> allAdditionalDetails = AccountDataChangeOrderDB.GetAccountAdditionsTypes();
            Dictionary<string, string> additionalDetails = new Dictionary<string, string>();
            string canChangeAllAccountAdditionsTypes = "";
            user?.AdvancedOptions?.TryGetValue("canChangeAllAccountAdditionsTypes", out canChangeAllAccountAdditionsTypes);

            foreach (var item in allAdditionalDetails)
            {
                if (item.Key != "6" && item.Key != "7" && item.Key != "9" && item.Key != "10" && item.Key != "11" && item.Key != "12")
                {

                    if (canChangeAllAccountAdditionsTypes == "1")
                    {
                        additionalDetails.Add(item.Key, item.Value);
                    }
                    else
                    {
                        //Բացառում ենք չօգտագործվող տեսակները և ոչ հասանելիները ((6,7,11,12  , 3,4,9,10,14)
                        if (Convert.ToUInt16(item.Key) == 1
                            || Convert.ToUInt16(item.Key) == 2
                            || Convert.ToUInt16(item.Key) == 5
                            || Convert.ToUInt16(item.Key) == 8
                            || Convert.ToUInt16(item.Key) == 13
                            || Convert.ToUInt16(item.Key) == 16)
                        {
                            additionalDetails.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            return additionalDetails;

        }

        /// <summary>
        /// Հարկային տեսչության կողմից հաստատման ենթակա է թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsApprovedByTaxService(string accountNumber)
        {
            return AccountDataChangeOrderDB.IsApprovedByTaxService(accountNumber);
        }
    }
}
