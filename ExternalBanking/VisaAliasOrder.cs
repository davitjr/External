using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class VisaAliasOrder : Order
    {
        /// <summary>
        /// Card Number for Visa Alias
        /// </summary>
        public string RecipientPrimaryAccountNumber { get; set; }

        /// <summary>
        /// CardHolder Full Name
        /// </summary>
        public string RecipientFullName { get; set; }

        /// <summary>
        /// Visa Alias(հեռախոսահամար)
        /// </summary>
        public string Alias { get; set; }

        public string CardType { get; set; }

        public string ReasonTypeDescription { get; set; }

        public ActionResult Save(string userName, SourceType source)
        {
            Complete();

            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {

                result = VisaAliasDB.VisaAliasOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                //hishenq hanenq ete petq chga Dav
                ActionResult resultOpPerson = SaveOrderOPPerson();

                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                scope.Complete();
            }

            return result;
        }

        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;

            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            OPPerson = SetOrderOPPerson(CustomerNumber);
        }

        public ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = base.Approve(schemaType, userName);

                    if (result.ResultCode == ResultCode.Normal)
                    {
                        Quality = OrderQuality.Sent3;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            result = base.Confirm(user);

            return result;
        }

        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || RegistrationDate.Date > DateTime.Now.Date)
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

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result = Save(userName, source);

            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }

            result = Approve(schemaType, userName, user);

            return result;
        }

        public VisaAliasOrder GetVisaAliasOrder(long orderId)
        {
            return VisaAliasDB.GetVisaAliasOrder(orderId);
        }

        public void Get()
        {
            VisaAliasDB.Get(this);
        }
        public CardHolderAndCardType GetCardTypeAndCardHolder(string cardNumber)
        {
            return VisaAliasDB.GetCardTypeAndCardHolder(cardNumber);
        }
    }
}
