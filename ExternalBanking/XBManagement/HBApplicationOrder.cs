using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.DocFlowManagement;
using ExternalBanking.TokenOperationsCasServiceReference;
using ExternalBanking.XBManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking.XBManagement
{
    public class HBApplicationOrder : Order
    {
        public HBApplication HBApplication { get; set; }
        public HBApplicationUpdate HBApplicationUpdate { get; set; }
        public ApprovementSchema ApprovementSchema { get; set; }

        /// <summary>
        /// Հեռահար բանկինգի հայտի պահպանում
        /// </summary>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;


            short customerType = Customer.GetCustomerType(this.CustomerNumber);


            bool needToConfirm = false;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = Save(user.userID, source, user, schemaType);
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();

                //Ոչ ֆիզիկական անձ հաճախորդների համար հաստատման սխեմայի ստեղծում/խմբագրում
                if (customerType != (short)CustomerTypes.physical)
                {
                    ActionResult res1 = new ActionResult();

                    if (ApprovementSchema != null && ApprovementSchema.Id == 0)
                    {
                        res1 = ApprovementSchema.CreateApprovementSchema(this.Id, this.CustomerNumber, this.HBApplication.ID);

                        result.Errors.AddRange(res1.Errors);
                    }
                    else

                    {
                        if (!(ApprovementSchema == null || ApprovementSchema.Id == 0))
                        {
                            res1 = ApprovementSchema.UpdateApprovementSchema(this.Id, this.CustomerNumber, this.HBApplication.ID);
                            result.Errors.AddRange(res1.Errors);
                        }

                    }
                }
                //Ֆիզիկական անձանց պայմանագրի վերականգնում
                else if (this.Type == OrderType.HBApplicationRestoreOrder)
                {
                    List<HBUser> hbUsers = HBUser.GetHBUsers(this.HBApplication.ID, ProductQualityFilter.All);
                    ActionResult res2 = new ActionResult();
                    foreach (HBUser hbUser in hbUsers)
                    {
                        if (hbUser.AllowDataEntry == true)
                        {
                            res2 = ApprovementSchema.CreateAutomaticApprovementSchema(hbUser, this.CustomerNumber, this.Id, Action.Add);
                            result.Errors.AddRange(res2.Errors);
                        }
                    }
                }

                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                if (source != SourceType.MobileBanking && source != SourceType.AcbaOnline)
                {
                    needToConfirm = NeedToConfirm(customerType);
                }

                //անհրաժեշտության դեպքում հայտը ուղարկվելու է հաստատման DocFlow ծրագրով
                if (needToConfirm)
                {
                    result = this.RedirectToDocFlow();
                    Quality = OrderQuality.TransactionLimitApprovement;
                    OrderDB.ChangeQuality(this.Id, OrderQuality.TransactionLimitApprovement, userName);
                    base.SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                }
                else
                {
                    Quality = OrderQuality.Sent3;
                    OrderDB.ChangeQuality(this.Id, OrderQuality.Sent3, userName);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                }

                scope.Complete();
            }

            if (!needToConfirm)
            {
                result = base.Confirm(user);
            }
            return result;
        }

        /// <summary>
        /// Հայտը ուղղորդվում է DocFlow
        /// </summary>
        private ActionResult RedirectToDocFlow()
        {
            return DocFlow.SendHBApplicationOrderToConfirm(this, user);
        }

        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Հեռահար բանկինգի հայտի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBApplication(this));
            return result;
        }

        private ActionResult Save(int userID, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result = new ActionResult();
            ActionResult retResult = new ActionResult();

            //միայն հեռահար բանկինգի հայտի պահպանում
            HBApplicationDB.Save(this, user, source);

            //Հայտում ավելացված օգտ./տոկենների պահպանում
            HBApplicationUpdate.AddedItems.ForEach(m =>
            {
                result = m.Add(userID, source, this.Id, this.CustomerNumber);
                if (result.ResultCode != ResultCode.Normal)
                {
                    retResult.Errors.AddRange(result.Errors);
                }
            }
            );
            //Հայտում փոփխված օգտ./տոկենների պահպանում
            HBApplicationUpdate.UpdatedItems.ForEach(m =>
            {
                result = m.Update(userID, source, this.Id, this.CustomerNumber);
                if (result.ResultCode != ResultCode.Normal)
                {
                    retResult.Errors.AddRange(result.Errors);
                }
            }
            );
            //Հայտում ապաակտիվացված օգտ./տոկենների պահպանում
            HBApplicationUpdate.DeactivatedItems.ForEach(m =>
            {
                result = m.Deactivate(userID, source, this.Id, this.CustomerNumber);
                if (result.ResultCode != ResultCode.Normal)
                {
                    retResult.Errors.AddRange(result.Errors);
                }
            }
            );

            if (retResult.Errors.Count == 0)
            {
                retResult.ResultCode = ResultCode.Normal;
            }
            else
            {
                retResult.ResultCode = ResultCode.Failed;
            }
            return retResult;
        }

        /// <summary>
        /// Հաստատման ուղարկելու անհրաժեշտության որոշում
        /// </summary>
        private bool NeedToConfirm(short customerType)
        {
            //ոչ ֆիզ. հաճախորդների դեպքում ուղարկել հաստատման, եթե առկա է նոր տոկեն կամ փոփոխվել է մուտքագրման իրավունք դաշտը
            if (customerType != (short)CustomerTypes.physical)
            {
                if (this.HBApplicationUpdate.AddedItems.Any(x => x.GetType().Name == "HBToken") || this.ApprovementSchema.isModified)
                    return true;
                else
                    return false;
            }
            else//ֆիզ. հաճախորդների դեպքում ուղարկել հաստատման, եթե փոփոխվել է գործարքների իրականացում դաշտը            
            {
                if (this.Type == OrderType.HBApplicationOrder || this.HBApplication.Quality == HBApplicationQuality.Request)
                {
                    return false;
                }
                List<HBUser> oldlist = HBUser.GetHBUsers(this.HBApplication.ID, ProductQualityFilter.Opened);
                List<HBUser> newlist = this.HBApplicationUpdate.UpdatedItems.Where(x => x.GetType().Name == "HBUser").Cast<HBUser>().ToList();

                var hbusersWithModifiedDataEntryPerm = from hbuser1 in oldlist
                                                       join hbuser2 in newlist
                                                       on hbuser1.ID equals hbuser2.ID
                                                       where hbuser1.AllowDataEntry != hbuser2.AllowDataEntry
                                                       select new { hbuser1, hbuser2 };


                if (hbusersWithModifiedDataEntryPerm.ToList().Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public void GetHBApplicationOrder()
        {
            HBApplicationDB.GetHBApplicationOrder(this);

        }
        /// <summary>
        /// Ստուգվում է գոյություն ունի դեռրևս չհաստատված հեռահար բանկինգի մուտքագրման կամ փոփոխության հայտ, թե ոչ
        /// </summary>
        public static bool IsExistsNotConfirmedHBOrder(ulong customerNumber)
        {
            return HBApplicationDB.isExistsNotConfirmedHBOrder(customerNumber);
        }
        /// <summary>
        /// Ստուգվում է հաճախորդը արդեն ունի հեռահար բանկինգի ծառայություն, թե ոչ
        /// </summary>
        public static bool IsExistsHB(ulong customerNumber)
        {
            return HBApplicationDB.isExistsHB(customerNumber);
        }
        public static int? GetHbAppicationStatus(ulong customerNumber)
        {
            return HBApplicationDB.GetHbAppicationStatus(customerNumber);
        }
        public static int? GetHbActivationTokenStatus(long docId)
        {
            return HBApplicationDB.GetHbActivationTokenStatus(docId);
        }
        public List<string> GetHBApplicationStateBeforeAndAfterConfirm()
        {
            List<string> hbApplicationStates = HBApplicationDB.GetHBApplicationStateBeforeAndAfterConfirm(this);
            return hbApplicationStates;
        }

        /// <summary>
        /// Ստուգվում է գոյություն ունի դեռրևս չհաստատված հեռահար բանկինգի մուտքագրման կամ փոփոխության հայտ, թե ոչ հայտի տեսակով 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public static (long?, bool) IsExistsNotConfirmedHBOrder(ulong customerNumber, OrderType documentType)
        {
            return HBApplicationDB.IsExistsNotConfirmedHBOrder(customerNumber, documentType);
        }

        public override bool CheckConditionForMakingTransactionIn24_7Mode()
        {
            List<HBToken> newtokenslist = this.HBApplicationUpdate.AddedItems.Where(x => x.GetType().Name == "HBToken").Cast<HBToken>().ToList();
            var tokenswithfees = from hbtoken in newtokenslist
                                 where
                                 hbtoken.TokenType == HBTokenTypes.Token
                                 ||
                                 ((hbtoken.TokenType == HBTokenTypes.MobileToken || hbtoken.TokenType == HBTokenTypes.MobileBanking) && hbtoken.TokenSubType == HBTokenSubType.Additional)
                                 select new { hbtoken };

            if (tokenswithfees.Count() > 0)
                return false;
            else
                return true;

        }

        public ActionResult AddEmailForCustomer(string emailAddress, ulong customerNumber, User user)
        {
            ActionResult actionResult = new ActionResult();
            CustomerEmail email = new CustomerEmail
            {
                emailType = new KeyValue(),
                email = new Email(),
                quality = new KeyValue(),
                priority = new KeyValue()
            };
            email.priority.key = 1;
            email.emailType.key = 5;
            email.quality.key = 1;
            email.email.emailAddress = emailAddress;

            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
            {
                proxy.SetCurrentUserAsync(user).Wait();
                Result result = proxy.SaveCustomerEmail(customerNumber, email);
                if (result.resultCode == 1)
                    actionResult.ResultCode = ResultCode.Normal;
            }

            return actionResult;
        }
        public ActionResult GenerateAcbaOnline(string userName, string password, ulong customerNumber, Languages languages, User user, string phoneNumber, string clientIp, int customerQuality, string email)
        {
            ActionResult orderResult = new ActionResult
            {
                ResultCode = ResultCode.Normal
            };
            Culture culture = new Culture(languages);
            user.filialCode = (ushort)Customer.GetCustomerFilial(customerNumber).key; //setting user fillial for all orders
            if (customerQuality != 1)
            {
                AccountOrder accountOrder = AccountOrderDB.GetRestrictedAccountOrder(customerNumber, OrderType.CurrentAccountOpen);
                if (accountOrder == null)
                {
                    accountOrder = CreateHBRestrictedAccountOrder(customerNumber, user);
                }
                if (accountOrder.Quality != OrderQuality.Completed)
                {
                    (long? notConfirmedOrderId, bool isFind) = IsExistsNotConfirmedHBOrder(customerNumber, OrderType.CurrentAccountOpen);
                    if (!isFind)
                    {
                        bool forRestrictedAccount = true;
                        orderResult = accountOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1, forRestrictedAccount);
                    }
                    else
                    {
                        orderResult = OrderDB.ConfirmRestrictedOrder((long)notConfirmedOrderId, user);
                    }
                }
                if (orderResult.ResultCode != ResultCode.Normal)
                {
                    Localization.SetCulture(orderResult, culture);
                    return orderResult;
                }
            }
            HBUser hbuser = CreateHBOrderUser(userName, password, customerNumber, user, email);
            HBToken hbtoken = CreateHBOrderToken(hbuser, user);
            HBApplicationOrder hbOrder = CreateHBApplicationOrder(customerNumber, user, (byte)customerQuality, hbtoken, out bool isFirstHbApplication);
            HBActivationOrder hbActivationOrder = CreateHBActivationOrder(customerNumber, user);
            HBServletRequestOrder hbServletOrder = CreateHBServletOrder(customerNumber, hbtoken, user, phoneNumber, customerQuality);
            HBApplicationQualityChangeOrder hbAppRestoreOrder = CreateApplicationQualityChangeOrder(customerNumber, user, hbOrder, OrderType.HBApplicationRestoreOrder);
            HBApplicationQualityChangeOrder hbAppDeactivateOrder = CreateApplicationQualityChangeOrder(customerNumber, user, hbOrder, OrderType.HBApplicationTerminationOrder);

            if (isFirstHbApplication)  // 1. Չունի ՀԲ (116,69,137)
            {
                //116
                orderResult = hbOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                if (orderResult.Errors.Count == 0)
                {
                    //69
                    hbActivationOrder.HBActivationRequests = HBActivationOrder.GetHBRequests(customerNumber);
                    orderResult = hbActivationOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                    if (orderResult.Errors.Count == 0)
                    {
                        //137
                        orderResult = hbServletOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1, clientIp);
                    }
                }
            }
            else if (hbOrder.HBApplication.Quality == HBApplicationQuality.Approved || hbOrder.HBApplication.Quality == HBApplicationQuality.Request || (hbOrder.HBApplication.Quality == HBApplicationQuality.Active && !HBToken.HasCustomerOneActiveToken(customerNumber))) // 2. ՈՒնի ՀԲ Հաստատված կամ դիմում կարգավիճակով կամ Գործող կարգավիճակով բայց աոանց գործող տոկենի (120, 119, 132, 69, 137)
            {
                //120
                orderResult = hbAppDeactivateOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                if (orderResult.Errors.Count == 0)
                {
                    //119
                    orderResult = hbAppRestoreOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                    if (orderResult.Errors.Count == 0)
                    {
                        //132
                        orderResult = hbOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                        if (orderResult.Errors.Count == 0)
                        {
                            //69
                            hbActivationOrder.HBActivationRequests = HBActivationOrder.GetHBRequests(customerNumber);
                            orderResult = hbActivationOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                            if (orderResult.Errors.Count == 0)
                            {
                                //137
                                orderResult = hbServletOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1, clientIp);
                            }
                        }
                    }
                }
            }
            else if (hbOrder.HBApplication.Quality == HBApplicationQuality.Deactivated)  // 3. ՈՒնի ՀԲ Դադարեցված կամ դիմում կարգավիճակով  (119, 132, 69, 137)
            {
                //119
                orderResult = hbAppRestoreOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                if (orderResult.Errors.Count == 0)
                {
                    //132
                    orderResult = hbOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                    if (orderResult.Errors.Count == 0)
                    {
                        //69
                        hbActivationOrder.HBActivationRequests = HBActivationOrder.GetHBRequests(customerNumber);
                        orderResult = hbActivationOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1);
                        if (orderResult.Errors.Count == 0)
                        {
                            //137
                            orderResult = hbServletOrder.SaveAndApprove(userName, SourceType.MobileBanking, user, 1, clientIp);
                        }
                    }
                }
            }
            else if (hbOrder.HBApplication.Quality == HBApplicationQuality.Active) // 4. ՈՒնի ՀԲ Գործող  կարգավիճակով և ունի գոնե մեկ գործող տոկեն
            {
                orderResult.Errors.Add(new ActionError { Description = "Հնարավոր չէ կատարել գրանցում։ Արդեն առկա է գործող կարգավիճակով Հեռահար Բանկինգ։" });
                orderResult.ResultCode = ResultCode.ValidationError;
                return orderResult;
            }
            Localization.SetCulture(orderResult, culture);
            return orderResult;
        }

        private HBApplicationOrder CreateHBApplicationOrder(ulong customerNumber, User user, byte customerQuality, HBToken hbToken, out bool isFirstHbApplication)
        {
            //Initializing HBOrder;
            HBApplication userApplication = HBApplication.GetHBApplication(customerNumber);
            isFirstHbApplication = userApplication == null;
            HBApplicationOrder order = new HBApplicationOrder
            {
                HBApplication = userApplication ?? new HBApplication
                {
                    ID = hbToken.HBUser.HBAppID,
                    ContractNumber = Utility.GetLastKeyNumber(72, user.filialCode).ToString(),
                    QualityDescription = "Դիմում",
                    Quality = HBApplicationQuality.Request,
                    InvolvingSetNumber = user.userID, //registration user id 
                    FilialCode = user.filialCode,
                    PermissionType = customerQuality == 1 ? (byte)1 : (byte)2,
                },
                HBApplicationUpdate = new HBApplicationUpdate
                {
                    AddedItems = new List<IEditableHBItem>() {
                        hbToken.HBUser,
                        hbToken
                    },
                    DeactivatedItems = new List<IEditableHBItem>(),
                    UpdatedItems = new List<IEditableHBItem>()
                },
                CustomerNumber = customerNumber,
                Type = userApplication != null ? OrderType.HBApplicationUpdateOrder : OrderType.HBApplicationOrder, // Եթե գոյություն ունի Հբ, ապա կատարում ենք 132 (Հեռահար բանկինգի փոփոխման  հայտ) հայտ, հակառակ դեպքում 116 (Հեռահար բանկինգի պայմանագրի մուտքագրման հայտ )
                RegistrationDate = DateTime.Now,
                SubType = 1,
                OperationDate = Utility.GetNextOperDay(),
                Source = SourceType.MobileBanking,
                user = user,
                FilialCode = user.filialCode,
                Quality = OrderQuality.Draft
            };
            return order;
        }
        private HBToken CreateHBOrderToken(HBUser hbuser, User user)
        {
            HBToken token = new HBToken
            {
                ID = (int)Utility.GetLastKeyNumber(74, user.filialCode),
                DayLimit = 400000,
                TransLimit = 400000,
                GID = "03", //GID 01 Fiz. token, 02 mobile token (chi gorcum), 03 mobile banking
                Quality = HBTokenQuality.StillNotConfirmed,
                TokenType = HBTokenTypes.MobileBanking,
                TokenSubType = HBTokenSubType.New,
                TokenSubTypeDescription = "Մոբայլ բանկինգ",
                TokenNumber = GetTempTokenList(1).FirstOrDefault(),
                HBUser = hbuser
            };
            return token;
        }
        private HBUser CreateHBOrderUser(string userName, string password, ulong customerNumber, User user, string gemail)
        {
            // Initializing HBUser 
            HBUser hbuser = new HBUser
            {
                ID = (int)(Utility.GetLastKeyNumber(73, user.filialCode)),
                HBAppID = (int)Utility.GetLastKeyNumber(77, user.filialCode),
                CustomerNumber = customerNumber,
                UserName = userName,
                Password = password,
                PassChangeReq = false,
                AllowDataEntry = true,
                IsCas = true,
                Email = new CustomerEmail
                {
                    email = new Email
                    {
                        emailAddress = gemail
                    }
                }
            };
            return hbuser;
        }
        private HBActivationOrder CreateHBActivationOrder(ulong customerNumber, User user)
        {
            HBActivationOrder order = new HBActivationOrder
            {
                Currency = "AMD",
                RegistrationDate = DateTime.Now,
                OperationDate = Utility.GetNextOperDay(),
                Quality = OrderQuality.Draft,
                Type = OrderType.HBActivation,
                SubType = 1,
                user = user, //registration user id 
                CustomerNumber = customerNumber,
                FilialCode = user.filialCode,
                HBActivationRequests = new List<HBActivationRequest>()
            };
            return order;
        }
        private HBApplicationQualityChangeOrder CreateApplicationQualityChangeOrder(ulong customerNumber, User user, HBApplicationOrder hbAppOrder, OrderType orderType)
        {
            HBApplicationQualityChangeOrder order = new HBApplicationQualityChangeOrder()
            {
                CustomerNumber = customerNumber,
                Type = orderType,
                RegistrationDate = DateTime.Now,
                SubType = 1,
                OperationDate = Utility.GetNextOperDay(),
                Source = SourceType.MobileBanking,
                user = user,
                Quality = OrderQuality.Draft,
                FilialCode = user.filialCode,
                HBApplication = hbAppOrder.HBApplication
            };
            return order;
        }
        private AccountOrder CreateHBRestrictedAccountOrder(ulong customerNumber, User user)
        {
            AccountOrder order = new AccountOrder
            {
                Currency = "AMD",
                RegistrationDate = DateTime.Now,
                OperationDate = Utility.GetNextOperDay(),
                Quality = OrderQuality.Draft,
                Type = OrderType.CurrentAccountOpen,
                SubType = 1,
                user = user, //registration user id 
                CustomerNumber = customerNumber,
                Source = SourceType.MobileBanking,
                FilialCode = user.filialCode,
                RestrictionGroup = 18,
                AccountType = 1
            };
            return order;
        }
        private HBServletRequestOrder CreateHBServletOrder(ulong customerNumber, HBToken token, User user, string phoneNumber, int customerQuality)
        {
            HBServletRequestOrder order = new HBServletRequestOrder
            {
                ServletAction = HBServletAction.ActivateToken,
                CustomerNumber = customerNumber,
                OperationDate = OperationDate,
                Quality = OrderQuality.Draft,
                Source = SourceType.MobileBanking,
                RegistrationDate = DateTime.Now,
                Type = OrderType.HBServletRequestTokenActivationOrder,
                SubType = 1,
                HBtoken = token,
                PhoneNumber = phoneNumber,
                user = user, //registration user id 
                CustomerQuality = customerQuality,
                CasServletRequest = new TokenOperationsInfo()
            };
            return order;
        }

        public static ActionResult MigrateOldUserToCas(int hbUserId)
        {
            return HBApplicationDB.MigrateOldUserToCas(hbUserId);
        }


        public List<string> GetTempTokenList(int tokenCount)
        {
            return HBApplicationDB.GetTempTokenList(tokenCount);
        }

    }
}
