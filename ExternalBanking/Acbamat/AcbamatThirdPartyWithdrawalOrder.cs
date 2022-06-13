using ExternalBanking.DBManager.Acbamat;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class AcbamatThirdPartyWithdrawalOrder : Order
    {
        public string TransactionId { get; set; }
        public string PartnerId { get; set; }
        public string AtmId { get; set; }
        public string UserId { get; set; }
        public string ReferenceNumber { get; set; }
        public ThirdPartyOrganizationTypes ThirdPartyOrganizationType { get; set; }

        public void SaveAcbamatThirdPartyWithdrawalDetails()
        {
            AcbamatThirdPartyWithdrawalOrderDB.SaveAcbamatThirdPartyWithdrawalDetails(this);
        }

        public void FinalizeThirdPartyWithdrawal(short schemaType)
        {
            ActionResult actionResult = SaveAndApproveAcbamatThirdPartyWithdrawalOrder(schemaType, user);

            if (actionResult.ResultCode != ResultCode.Normal)
                throw new AcbamatThirdPartyWithdrawalException($"Acbamat 3-րդ կողմ կազմակերպություններից կանխիկացման ժամանակ տեղի ունեցավ սխալ TransactionID - {TransactionId}");
        }

        public void GetAcbamatThirdPartyWithdrawalOrder()
        {
            AcbamatThirdPartyWithdrawalOrderDB.GetAcbamatThirdPartyWithdrawalOrder(this);
        }

        public ActionResult SaveAndApproveAcbamatThirdPartyWithdrawalOrder(short schemaType, ACBAServiceReference.User user)
        {
            Complete();
            ActionResult result = Validate();
            
            if (result.Errors.Count > 0)
            {
                Reject(100, user);
                SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = AcbamatThirdPartyWithdrawalOrderDB.SaveAcbamatThirdPartyWithdrawalOrder(this, user.userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, Action.Add);

                result = Approve(schemaType, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);
            return resultConfirm;
        }

        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;
            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
            {
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            }

            OPPerson = SetOrderOPPerson(CustomerNumber);
            Type = OrderType.AcbamatOrder;
            SubType = (byte)AcbamatSubType.AcbamatThirdPartyWithdrawalOrder;
        }

        private ActionResult Validate()
        {
            try
            {
                ActionResult result = new ActionResult();
                result.Errors.AddRange(Validation.ValidateAcbamatThirdPartyWithdrawalOrder(this));
                return result;
            }
            catch
            {
                Reject(1, user);
                SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
                throw;
            }
        }

        public class AcbamatThirdPartyWithdrawalException : Exception
        {
            public AcbamatThirdPartyWithdrawalException(string message) : base(message)
            {
            }
        }
    }
}
