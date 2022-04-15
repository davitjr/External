using ExternalBanking.DBManager.Acbamat;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class AcbamatExchangeOrder : Order
    {
        public string TransactionId { get; set; }
        public string PartnerId { get; set; }
        public string AtmId { get; set; }
        public float Rate { get; set; }
        public float Dispened { get; set; }
        public float Fee { get; set; }
        public string Mobile { get; set; }
        public CommunalTypes CommunalType { get; set; }
        public FeeTransferTypes FeeTransferType { get; set; }

        public void SaveAcbamatExchangeDetails()
        {
            AcbamatExchangeOrderDB.SaveAcbamatExchangeDetails(this);
        }

        public void FinalizeExchange(short schemaType)
        {
            ActionResult actionResult = SaveAndApproveAcbamatExchangeOrder(schemaType, user);

            if (actionResult.ResultCode != ResultCode.Normal)
                throw new Exception($"Acbamat փոխարկման ժամանակ տեղի ունեցավ սխալ TransactionID - {TransactionId}");

            if (FeeTransferType != FeeTransferTypes.None)
            {
                try
                {
                    SaveAndApproveAcbamatFeeTransactions();
                }
                catch (Exception)
                {
                    throw new AcbamatFeeTransactionException($"Acbamat փոխարկման մնացորդի փոխանցման ժամանակ տեղի ունեցավ սխալ, բայց փոխարկումը հաջողությամբ կատարվել է TransactionID - {TransactionId}");
                }
            }
        }

        public void GetAcbamatExchangeOrder()
        {
            AcbamatExchangeOrderDB.GetAcbamatExchangeOrder(this);
        }

        public ActionResult SaveAndApproveAcbamatExchangeOrder(short schemaType, ACBAServiceReference.User user)
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
                result = AcbamatExchangeOrderDB.SaveAcbamatExchangeOrder(this, user.userName);

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

        private void SaveAndApproveAcbamatFeeTransactions()
        {
            switch (FeeTransferType)
            {
                case FeeTransferTypes.MobileNumber when Fee >= 100:
                    {
                        UtilityPaymentOrder utilityPaymentOrder = new UtilityPaymentOrder()
                        {
                            OperationDate = Utility.GetCurrentOperDay(),
                            FilialCode = 22000,
                            CommunalType = CommunalType,
                            Amount = Fee,
                            Currency = "AMD",
                            Code = Mobile,
                            Type = OrderType.CommunalPayment,
                            SubType = 1,
                            user = user,
                            AbonentType = (int)AbonentTypes.physical,
                            OPPerson = OPPerson,
                            Description = "հեռախոսահամարի լիցքավորում",
                            RegistrationDate = DateTime.Now,
                            CustomerNumber = CustomerNumber,
                            Source = SourceType.AcbaMat,
                            Quality = OrderQuality.Draft,
                            DebitAccount = GetTerminalAccount()
                        };
                        switch (CommunalType)
                        {
                            case CommunalTypes.ArmenTel:
                            case CommunalTypes.VivaCell:
                            case CommunalTypes.Orange:
                                {
                                    utilityPaymentOrder.SaveAndApprove(user.userName, utilityPaymentOrder.Source, user, 1);
                                }

                                break;
                            default:
                                break;
                        }
                    }

                    break;
                case FeeTransferTypes.HayastanFund:
                case FeeTransferTypes.MilitaryFund:
                case FeeTransferTypes.MobileNumber when Fee < 100:
                    {
                        Account Account = GetFundAccount(FeeTransferType);
                        PaymentOrder paymentOrder = new PaymentOrder()
                        {
                            OperationDate = Utility.GetCurrentOperDay(),
                            FilialCode = 22000,
                            UseCreditLine = false,
                            Amount = Fee,
                            Currency = "AMD",
                            ReceiverBankCode = Account.FilialCode,
                            Type = OrderType.RATransfer,
                            SubType = 1,
                            OPPerson = OPPerson,
                            Description = "հանգանակություն արտարժույթի փոխարկումից",
                            RegistrationDate = DateTime.Now,
                            CustomerNumber = CustomerNumber,
                            Source = SourceType.AcbaMat,
                            Quality = OrderQuality.Draft,
                            user = user,
                            DebitAccount = GetTerminalAccount(),
                            ReceiverAccount = Account
                        };
                        paymentOrder.SaveAndApprove(user.userName, paymentOrder.Source, user, 1);
                    }

                    break;
                default:
                    break;
            }
        }

        private Account GetFundAccount(FeeTransferTypes FeeTransferType)
        {
            switch (FeeTransferType)
            {
                case FeeTransferTypes.HayastanFund:
                    {
                        return Account.GetSystemAccountByNN(406707782, 22000);
                    }
                case FeeTransferTypes.MilitaryFund:
                case FeeTransferTypes.MobileNumber when Fee < 100:
                    {
                        return Account.GetSystemAccountByNN(406707783, 22041);
                    }
                default:
                    throw new InvalidOperationException($"Invalid FeeTransferType - {FeeTransferType}");
            }
        }

        private Account GetTerminalAccount()
        {
            return Account.GetSystemAccountByNN(406707781, 22000);
        }

        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;
            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
            {
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            }

            DebitAccount = new Account
            {
                AccountNumber = AcbamatSharedDB.GetAcbamatAccountNumber(AtmId, Currency)
            };
            OPPerson = SetOrderOPPerson(CustomerNumber);
            Type = OrderType.AcbamatOrder;
            SubType = (byte)AcbamatSubType.AcbamatExchangeOrder;
        }

        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAcbamatExchangeOrder(this));
            return result;
        }
    }
    public class AcbamatFeeTransactionException : Exception
    {
        public AcbamatFeeTransactionException(string message) : base(message)
        {
        }
    }
}
