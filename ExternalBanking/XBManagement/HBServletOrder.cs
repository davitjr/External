using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Configuration;
using System.Transactions;

namespace ExternalBanking.XBManagement
{
    public class HBServletOrder : Order
    {

        public HBServletOrder()
        {
            SecServletRequest = new TokenOperationsSecService.TokenOperationsInfo();
            ServletRequest = new TokenOperationsServiceReference.TokenOperationsInfo();
            CasServletRequest = new TokenOperationsCasServiceReference.TokenOperationsInfo();
        }
        /// <summary>
        /// Հարցում
        /// </summary>
        public TokenOperationsServiceReference.TokenOperationsInfo ServletRequest { get; set; }


        /// <summary>
        /// Հարցում
        /// </summary>
        public TokenOperationsSecService.TokenOperationsInfo SecServletRequest { get; set; }
        /// <summary>
        /// Հարցում
        /// </summary>
        public TokenOperationsCasServiceReference.TokenOperationsInfo CasServletRequest { get; set; }
        /// <summary>
        /// Պատասխան
        /// </summary>
        public TokenOperationsServiceReference.TokenOperationsResult ServletResult { get; set; }


        /// <summary>
        /// Պատասխան
        /// </summary>
        public TokenOperationsSecService.TokenOperationsResult SecServletResult { get; set; }
        /// <summary>
        /// Պատասխան
        /// </summary>
        public TokenOperationsCasServiceReference.TokenOperationsResult CasServletResult { get; set; }
        /// <summary>
        /// Տոկեն
        /// </summary>
        public HBToken HBtoken { get; set; }

        /// <summary>
        /// Հաճախորդի IP
        /// </summary>
        public string ClientIP { get; set; }

        /// <summary>
        /// Հաճախորդի կարգավիճակ
        /// </summary>
        public int CustomerQuality { get; set; } = 1;

        /// <summary>
        /// Հաճախորդի Ընտրած լեզուն
        /// </summary>
        public byte Language { get; set; }
        /// <summary>
        /// Հարցման տեսակ
        /// </summary>
        public HBServletAction ServletAction { get; set; }


        /// <summary>
        /// Պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public virtual ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            return new ActionResult();
        }
        /// <summary>
        /// Պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <param name="clientIp"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, User user, short schemaType, string clientIp)
        {
            ClientIP = clientIp;
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = this.Save(userName, source, user, schemaType);

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

            result = Confirm(user);
            return result;

        }

        public ActionResult SaveOrder(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, String clientIp)
        {
            ClientIP = clientIp;
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = this.Save(userName, source, user, schemaType);
                //**********
                //ulong docId = base.Save(this, source, user);
                ulong docId = 0;
                //Order.SaveLinkHBDocumentOrder(this.Id, docId);
                //ActionResult res = BOOrderCustomer.Save(this, docId, user);
                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }


                LogOrderChange(user, action);
                scope.Complete();
                return result;
            }
        }


        public ActionResult Approve(string userName, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
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
            result = Confirm(user);
            return result;

        }


        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.RegistrationDate = DateTime.Now;
        }

        /// <summary>
        /// Ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateHBServletRequestOrder(this));

            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        /// <summary>
        /// Հայտի կատարում
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        override public ActionResult Confirm(User user, ConfirmationSourceType confirmationSourceType = ConfirmationSourceType.None)
        {
            ActionResult result = new ActionResult();

            try
            {
                if (Id == 0)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {

                    if (Quality == OrderQuality.Sent3)
                    {
                        if (!IsAutomatConfirm(this.Type, this.SubType))
                        {
                            result.ResultCode = ResultCode.NoneAutoConfirm;
                            ActionError actionError = new ActionError
                            {
                                Code = 0,
                                Description = "Տվյալ տեսակի հայտը ավտոմատ չի կատարվում: Խնդրում ենք սպասել կատարմանը:"
                            };
                            result.Errors.Add(actionError);
                        }
                        else
                        {
                            bool EnabledOldMobileCompatibility = bool.Parse(ConfigurationManager.AppSettings["EnabledOldMobileCompatibility"]);
                            bool EnableSecurityService = bool.Parse(ConfigurationManager.AppSettings["EnableSecurityService"]);
                             short typeOfCustomer = ACBAOperationService.GetCustomerType(this.CustomerNumber);
                            if ((EnabledOldMobileCompatibility && HBtoken?.HBUser?.IsCas != true) || 
                                (HBtoken?.HBUser?.IsCas != true && ( ServletAction == HBServletAction.DeactivateToken || ServletAction == HBServletAction.DeactivateUser )) ||
                                (HBtoken?.HBUser?.IsCas != true &&  ServletAction == HBServletAction.UnlockToken  && typeOfCustomer != 6 ))
                            {
                                if (EnableSecurityService)
                                {
                                    result = MakeSecServletRequest(this, user);
                                }
                                else
                                {
                                    result = MakeServletRequest(this, user);
                                }
                            }
                            else
                            {
                                result = MakeCasServletRequest(this, user);
                            }

                            if (result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.Normal)
                            {
                                this.Quality = OrderQuality.Completed;
                                base.SetQualityHistoryUserId(OrderQuality.Completed, user.userID);
                            }
                            else
                            {                                
                                this.Quality = OrderQuality.Canceled;
                                base.UpdateQuality(this.Quality);
                                base.SetQualityHistoryUserId(this.Quality, user.userID);
                                result.ResultCode = ResultCode.Failed;
                            }
                        }
                    }
                    else
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Errors.Add(new ActionError(35));//Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.SavedNotConfirmed;
                ActionError actionError = new ActionError
                {
                    Code = 0,
                    Description = ex.Message
                };
                result.Errors.Add(actionError);
            }


            return result;
        }
        /// <summary>
        /// Servlet հարցման կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult MakeServletRequest(HBServletOrder order, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            ActionError err = new ActionError();
            try
            {

                InitServletRequest(order);

                switch (ServletAction)
                {
                    case HBServletAction.ActivateToken:
                        if ((HBTokenTypes)HBtoken.TokenType == HBTokenTypes.Token)
                            ServletResult = TokenOperationsService.ActivateToken(ServletRequest);

                        else if ((HBTokenTypes)HBtoken.TokenType == HBTokenTypes.MobileBanking || (HBTokenTypes)HBtoken.TokenType == HBTokenTypes.MobileToken)
                            ServletResult = TokenOperationsService.ActivateMobileToken(ServletRequest);
                        break;

                    case HBServletAction.UnlockToken:
                        ServletResult = TokenOperationsService.UnlockToken(ServletRequest);
                        break;

                    case HBServletAction.DeactivateToken:
                        ServletResult = TokenOperationsService.BlockToken(ServletRequest);
                        break;

                    case HBServletAction.DeactivateUser:
                        ServletResult = TokenOperationsService.BlockUser(ServletRequest);
                        break;

                    case HBServletAction.ShowPINCode:
                        ServletResult.ResultValue = TokenOperationsService.GetPinCode(ServletRequest.CardNumber);
                        break;

                    case HBServletAction.ResetUserPasswordManually:
                        ServletResult = TokenOperationsService.ResetUserPasswordManualy(ServletRequest);
                        break;

                    case HBServletAction.NotSpecified:
                        result.ResultCode = ResultCode.Failed;
                        err.Description = "Նշված հարցում գոյություն չունի";
                        result.Errors.Add(err);
                        break;

                }

                if (ServletResult.ResultCode == 0)
                {
                    HBServletRequestOrderDB.UpdateHBdocumentQuality(order.Id, user);
                    if (ServletResult.ResultValue != null)
                        if (ServletResult.ResultValue != string.Empty)
                        {
                            result.ResultCode = ResultCode.DoneAndReturnedValues;
                            result.Errors.Add(new ActionError
                            {
                                Description = ServletResult.ResultValue
                            });
                        }
                        else
                        {
                            result.ResultCode = ResultCode.Normal;
                        }
                    else
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    err.Description = $"Հարցումը ձախողվեց";
                    result.Errors.Add(err);
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                err.Description = $"Հարցումը ձախողվեց";
                result.Errors.Add(err);
            }
            return result;
        }

        /// <summary>
        /// Servlet հարցման կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult MakeCasServletRequest(HBServletOrder order, User user)
        {
            ActionResult result = new ActionResult();
            ActionError err = new ActionError();
            try
            {
                if(Source == SourceType.Bank && order?.HBtoken?.IsRegistered != null)
                {
                    order.HBtoken.IsRegistered = HBToken.HasCustomerOneActiveToken(order.CustomerNumber);
                }
                InitCasServletRequest(order);

                switch (ServletAction)
                {
                    case HBServletAction.ActivateToken:
                        if (HBtoken.TokenType == HBTokenTypes.Token)
                            CasServletResult = TokenOperationsCasService.ActivateToken(CasServletRequest, order.CustomerNumber, order?.HBtoken?.HBUser?.Email?.email?.emailAddress, order.CustomerQuality, order.HBtoken.IsRegistered);

                        else if (HBtoken.TokenType == HBTokenTypes.MobileBanking || HBtoken.TokenType == HBTokenTypes.MobileToken)
                            CasServletResult = TokenOperationsCasService.ActivateMobileToken(CasServletRequest, order.HBtoken.HBUser.Password, (TokenOperationsCasServiceReference.SourceType)order.Source, order.HBtoken.IsRegistered, order.PhoneNumber, order.CustomerNumber, order?.HBtoken?.HBUser?.Email?.email?.emailAddress, order.CustomerQuality);
                            break;

                    case HBServletAction.UnlockToken:
                        CasServletResult = TokenOperationsCasService.UnlockToken(CasServletRequest);
                        break;

                    case HBServletAction.UnlockUser:
                        CasServletResult = TokenOperationsCasService.UnlockUser(CasServletRequest, order.PhoneNumber, order.CustomerNumber, (TokenOperationsCasServiceReference.SourceType)order.Source, order.Language);
                        break;

                    case HBServletAction.DeactivateToken:
                        CasServletResult = TokenOperationsCasService.BlockToken(CasServletRequest);
                        break;

                    case HBServletAction.DeactivateUser:
                        CasServletResult = TokenOperationsCasService.DeactivateUser(CasServletRequest);
                        break;
                    case HBServletAction.ActivateUser:
                        CasServletResult = TokenOperationsCasService.UnBlockUser(CasServletRequest);
                        break;
                    case HBServletAction.ResetUserPasswordManually:
                        CasServletResult = TokenOperationsCasService.ResetUserPasswordManualy(CasServletRequest);
                        break;
                    case HBServletAction.ShowPINCode:
                        CasServletResult.ResultValue = TokenOperationsCasService.GetPinCode(CasServletRequest.CardNumber);
                        break;
                    case HBServletAction.NotSpecified:
                        result.ResultCode = ResultCode.Failed;
                        err.Description = "Նշված հարցում գոյություն չունի";
                        result.Errors.Add(err);
                        break;

                }

                if (CasServletResult.ResultCode == 0)
                {
                    HBServletRequestOrderDB.UpdateHBdocumentQuality(order.Id, user);
                    if (string.IsNullOrEmpty(CasServletResult.ResultValue))
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (CasServletResult.ResultValue.Contains("ACBA"))
                    {
                        result.ResultCode = ResultCode.Normal;
                        try
                        {
                            bool hasRisks = Customer.CheckMobileBankingCustomerDetailsRiskyChanges(CasServletResult.ResultValue);
                            if (hasRisks)
                            {
                                HBTokenDB.SaveMobileTokenActivationDetailsForRiskyChanges(CasServletResult.ResultValue);
                                EmailMessagingOperations.SendMobileBankingCustomerDetailsRiskyChangeAlert(CasServletResult.ResultValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = ex.Message;
                        }
                    }
                    else if (CasServletResult.ResultValue.Contains("Pin")) // for development
                    {
                        result.ResultCode = ResultCode.DoneAndReturnedValues;
                        result.Errors.Add(new ActionError
                        {
                            Description = CasServletResult.ResultValue
                        });
                    }
                    else if(ServletAction == HBServletAction.ShowPINCode)
                    {
                        result.ResultCode = ResultCode.DoneAndReturnedValues;
                        result.Errors.Add(new ActionError
                        {
                            Description = CasServletResult.ResultValue
                        });
                    }
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    err.Description = CasServletResult.ResultDescription;
                    result.Errors.Add(err);
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                err.Description = $"Հարցումը ձախողվեց";
                result.Errors.Add(err);
            }
            return result;
        }




        /// <summary>
        /// Servlet հարցման կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult MakeSecServletRequest(HBServletOrder order, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            ActionError err = new ActionError();
            try
            {
                
                InitSecServletRequest(order);

                switch (ServletAction)
                {
                    case HBServletAction.ActivateToken:
                        if ((HBTokenTypes)HBtoken.TokenType == HBTokenTypes.Token)
                            SecServletResult = TokenOperationsSecurityService.ActivateToken(SecServletRequest);

                        else if ((HBTokenTypes)HBtoken.TokenType == HBTokenTypes.MobileBanking || (HBTokenTypes)HBtoken.TokenType == HBTokenTypes.MobileToken)
                            SecServletResult = TokenOperationsSecurityService.ActivateMobileToken(SecServletRequest);
                        break;

                    case HBServletAction.UnlockToken:
                        SecServletResult = TokenOperationsSecurityService.UnlockToken(SecServletRequest);
                        break;

                    case HBServletAction.DeactivateToken:
                        SecServletResult = TokenOperationsSecurityService.BlockToken(SecServletRequest);
                        break;

                    case HBServletAction.DeactivateUser:
                        SecServletResult = TokenOperationsSecurityService.BlockUser(SecServletRequest);
                        break;

                    case HBServletAction.ShowPINCode:
                        SecServletResult.ResultValue = TokenOperationsSecurityService.GetPinCode(SecServletRequest.CardNumber);
                        break;

                    case HBServletAction.ResetUserPasswordManually:
                        SecServletResult = TokenOperationsSecurityService.ResetUserPasswordManualy(SecServletRequest);
                        break;

                    case HBServletAction.NotSpecified:
                        result.ResultCode = ResultCode.Failed;
                        err.Description = "Նշված հարցում գոյություն չունի";
                        result.Errors.Add(err);
                        break;

                }

                if (SecServletResult.ResultCode == 0)
                {
                    HBServletRequestOrderDB.UpdateHBdocumentQuality(order.Id, user);
                    if (SecServletResult.ResultValue != null)
                        if (SecServletResult.ResultValue != string.Empty)
                        {
                            result.ResultCode = ResultCode.DoneAndReturnedValues;
                            result.Errors.Add(new ActionError
                            {
                                Description = SecServletResult.ResultValue
                            });
                        }
                        else
                        {
                            result.ResultCode = ResultCode.Normal;
                        }
                    else
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    err.Description = $"Հարցումը ձախողվեց";
                    result.Errors.Add(err);
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                err.Description = $"Հարցումը ձախողվեց";
                result.Errors.Add(err);
            }
            return result;
        }



        /// <summary>
        /// Servlet-ի ինիցիալիզացում
        /// </summary>
        /// <param name="order"></param>
        private void InitCasServletRequest(HBServletOrder order)
        {
            if (CasServletRequest == null)
                CasServletRequest = new TokenOperationsCasServiceReference.TokenOperationsInfo();
            CasServletResult = new TokenOperationsCasServiceReference.TokenOperationsResult();

            if (ServletAction == HBServletAction.ActivateToken || ServletAction == HBServletAction.DeactivateToken || ServletAction == HBServletAction.UnlockToken)
            {
                CasServletRequest.CardNumber = order.HBtoken.TokenNumber;
                CasServletRequest.GID = order.HBtoken.GID;
                CasServletRequest.UserName = order.HBtoken.HBUser.UserName;
                CasServletRequest.UserDescription = order.HBtoken.HBUser.UserFullNameEng;
                CasServletRequest.IpAddress = order.ClientIP;
                CasServletRequest.CreatorID = user.userID;
                CasServletRequest.TokenId = order.HBtoken.ID;
            }
            else if (ServletAction == HBServletAction.DeactivateUser || ServletAction == HBServletAction.ActivateUser)
            {
                CasServletRequest.UserName = order.HBtoken.HBUser.UserName;
                CasServletRequest.IpAddress = ClientIP;
                CasServletRequest.CreatorID = user.userID;
                CasServletRequest.TokenId = order.HBtoken.ID;
            }
            else if (ServletAction == HBServletAction.ShowPINCode)
            {
                CasServletRequest.CardNumber = order.HBtoken.TokenNumber;
            }
            else if (ServletAction == HBServletAction.ResetUserPasswordManually)
            {
                CasServletRequest.UserName = order.HBtoken.HBUser.UserName;
                CasServletRequest.IpAddress = ClientIP;
                CasServletRequest.CreatorID = user.userID;
                CasServletRequest.CardNumber = order.HBtoken.TokenNumber;
                CasServletRequest.TokenId = order.HBtoken.ID;
            }
            else if (ServletAction == HBServletAction.UnlockUser)
            {
                CasServletRequest.UserName = order.HBtoken.HBUser.UserName;
                CasServletRequest.IpAddress = ClientIP;
                CasServletRequest.CreatorID = user.userID;
            }
        }
        /// <summary>
        /// Servlet-ի ինիցիալիզացում
        /// </summary>
        /// <param name="order"></param>
        private void InitServletRequest(HBServletOrder order)
        {
            if (ServletRequest == null)
                ServletRequest = new TokenOperationsServiceReference.TokenOperationsInfo();
            ServletResult = new TokenOperationsServiceReference.TokenOperationsResult();

            if (ServletAction == HBServletAction.ActivateToken || ServletAction == HBServletAction.DeactivateToken || ServletAction == HBServletAction.UnlockToken || ServletAction == HBServletAction.UnlockUser)
            {
                ServletRequest.CardNumber = order.HBtoken.TokenNumber;
                ServletRequest.GID = order.HBtoken.GID;
                ServletRequest.UserName = order.HBtoken.HBUser.UserName;
                ServletRequest.UserDescription = order.HBtoken.HBUser.UserFullNameEng;
                ServletRequest.IpAddress = order.ClientIP;
                ServletRequest.CreatorID = user.userID;
            }
            else if (ServletAction == HBServletAction.DeactivateUser)
            {
                ServletRequest.UserName = order.HBtoken.HBUser.UserName;
                ServletRequest.IpAddress = ClientIP;
                ServletRequest.CreatorID = user.userID;
            }
            else if (ServletAction == HBServletAction.ShowPINCode)
            {
                ServletRequest.CardNumber = order.HBtoken.TokenNumber;
            }
            else if (ServletAction == HBServletAction.ResetUserPasswordManually)
            {
                ServletRequest.UserName = order.HBtoken.HBUser.UserName;
                ServletRequest.IpAddress = ClientIP;
                ServletRequest.CreatorID = user.userID;
                ServletRequest.CardNumber = order.HBtoken.TokenNumber;
            }
        }


        /// <summary>
        /// Servlet-ի ինիցիալիզացում
        /// </summary>
        /// <param name="order"></param>
        private void InitSecServletRequest(HBServletOrder order)
        {
            if (SecServletRequest == null)
                SecServletRequest = new TokenOperationsSecService.TokenOperationsInfo();
            SecServletResult = new TokenOperationsSecService.TokenOperationsResult();

            if (ServletAction == HBServletAction.ActivateToken || ServletAction == HBServletAction.DeactivateToken || ServletAction == HBServletAction.UnlockToken || ServletAction == HBServletAction.UnlockUser)
            {
                SecServletRequest.CardNumber = order.HBtoken.TokenNumber;
                SecServletRequest.GID = order.HBtoken.GID;
                SecServletRequest.UserName = order.HBtoken.HBUser.UserName;
                SecServletRequest.UserDescription = order.HBtoken.HBUser.UserFullNameEng;
                SecServletRequest.IpAddress = order.ClientIP;
                SecServletRequest.CreatorID = user.userID;
                SecServletRequest.OTP = order?.ServletRequest?.OTP ?? string.Empty;
            }
            else if (ServletAction == HBServletAction.DeactivateUser)
            {
                SecServletRequest.UserName = order.HBtoken.HBUser.UserName;
                SecServletRequest.IpAddress = ClientIP;
                SecServletRequest.CreatorID = user.userID;
            }
            else if (ServletAction == HBServletAction.ShowPINCode)
            {
                SecServletRequest.CardNumber = order.HBtoken.TokenNumber;
            }
            else if (ServletAction == HBServletAction.ResetUserPasswordManually)
            {
                SecServletRequest.UserName = order.HBtoken.HBUser.UserName;
                SecServletRequest.IpAddress = ClientIP;
                SecServletRequest.CreatorID = user.userID;
                SecServletRequest.CardNumber = order.HBtoken.TokenNumber;
            }
        }

    }
}
