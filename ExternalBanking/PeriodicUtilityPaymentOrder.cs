using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Transactions;


namespace ExternalBanking
{
    /// <summary>
    /// Պարբերական փոխանցման հայտ(Կոմունալ)
    /// </summary>
    public class PeriodicUtilityPaymentOrder : PeriodicOrder
    {

        public UtilityPaymentOrder UtilityPaymentOrder { get; set; }

        /// <summary>
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicUtilityPaymentOrder(string userName, SourceType source, ACBAServiceReference.User user)
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
                result = PeriodicTransferOrderDB.SavePeriodicUtilityPaymentOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }
        /// <summary>
        /// Հայտի պահմապնման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrder());
            this.UtilityPaymentOrder.OrderNumber = this.OrderNumber;
            this.UtilityPaymentOrder.CustomerNumber = this.CustomerNumber;
            this.UtilityPaymentOrder.RegistrationDate = this.RegistrationDate;
            this.UtilityPaymentOrder.Amount = this.Amount;
            this.UtilityPaymentOrder.Id = this.Id;
            this.UtilityPaymentOrder.Source = this.Source;
            this.UtilityPaymentOrder.Type = OrderType.CommunalPayment;
            this.UtilityPaymentOrder.SubType = this.SubType;
            this.UtilityPaymentOrder.user = user;

            result.Errors.AddRange(UtilityPaymentOrder.Validate(user).Errors);

            //Եթե ընտրված է ամբողջ պարտքը տեսակը գումարի ստուգման մասը ջնջում ենք 
            if (this.AllDebt == true)
            {
                ActionError err = result.Errors.Find(m => m.Code == 22);
                result.Errors.Remove(err);
            }

            if (this.DebitAccount.Currency != "AMD")
            {
                //Ընտրված տեսակի փոխանցման դեպքում հաշվի արժույթը պետք է լինի <<AMD>>:
                result.Errors.Add(new ActionError(661));
            }


            if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.Gas)
            {
                if (this.ServicePaymentType == -1)
                {
                    //Կոմունալ պարբերական վճարման տեսակն ընտրված չէ:
                    result.Errors.Add(new ActionError(258));
                }

                if (this.ServicePaymentType == 1 && this.AllDebt == false)
                {
                    //Տվյալ ենթատեսակի համար պետք է ընտրել ամբողջ պարտքը
                    result.Errors.Add(new ActionError(407));
                }

            }
            if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.ArmenTel || this.UtilityPaymentOrder.CommunalType == CommunalTypes.BeelineInternet || this.UtilityPaymentOrder.CommunalType == CommunalTypes.Orange)
            {
                if (this.MaxAmountLevel != 0)
                {
                    if (this.AllDebt == true && this.MaxAmountLevel < 100)
                    {
                        //Փոխանցման առավելագույն գումարը պետք է լինի 100 դրամից ոչ պակաս:
                        result.Errors.Add(new ActionError(320));
                    }
                }


            }
            if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.VivaCell)
            {
                if (this.MaxAmountLevel != 0)
                {
                    if (this.AllDebt == true && this.MaxAmountLevel < 50)
                    {
                        //Փոխանցման առավելագույն գումարը պետք է լինի 50 դրամից ոչ պակաս:
                        result.Errors.Add(new ActionError(338));
                    }
                }
                //Ստուգումը առկա է ACBAOnline-ում սակայն New_project ում չկա նման ստուգում տեսատավորման արդյունքում հանվել է նշված ստուգումը

                //if (this.AllDebt == true)
                //{
                //    if (this.MaxAmountLevel == 0 || this.MaxAmountLevel < 0)
                //    {
                //        //Առավելագույն գումարի դաշտը լրացված չէ
                //        result.Errors.Add(new ActionError(322));
                //    }
                //}
            }

            if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.UCom || this.UtilityPaymentOrder.CommunalType == CommunalTypes.Orange)
            {
                DateTime operDay = Utility.GetNextOperDay();
                if (this.FirstTransferDate < operDay)
                {
                    //Տվյալ տեսակի համար առաջին փոխանցման օրը պետք է մեծ լինի @var1-ից:
                    result.Errors.Add(new ActionError(346, new string[] { operDay.ToString("dd/MM/yyyy") }));
                }
                else
                {
                    if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.Orange)
                    {
                        if (this.FirstTransferDate.Day < 3)
                        {
                            //Տվյալ տեսակի համար առաջին փոխանցման օրը պետք է մեծ լինի @var1-ից:
                            result.Errors.Add(new ActionError(346, new string[] { "03/" + this.FirstTransferDate.ToString("MM/yyyy") }));
                        }
                    }
                    if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.UCom)
                    {
                        if (this.FirstTransferDate.Day < 5)
                        {
                            //Տվյալ տեսակի համար առաջին փոխանցման օրը պետք է մեծ լինի @var1-ից:
                            result.Errors.Add(new ActionError(346, new string[] { "05/" + this.FirstTransferDate.ToString("MM/yyyy") }));
                        }
                    }
                }

            }

            //if(string.IsNullOrEmpty(this.UtilityPaymentOrder.Code))
            //{
            //    //Կատարեք որոնում հաճախորդին նույնականացնելու համար:
            //     result.Errors.Add(new ActionError(321));

            //}
            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            return result;


        }
        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.UtilityPaymentOrder.OPPerson = this.OPPerson;
            this.DebitAccount = this.UtilityPaymentOrder.DebitAccount;

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                if (ChargeType == 2)
                {
                    this.AllDebt = true;
                }

                if (this.Amount != 0)
                {
                    this.PayIfNoDebt = 1;
                }
                else
                {
                    this.PayIfNoDebt = 0;
                }


                SearchCommunal searchCommunal = new SearchCommunal()
                {
                    AbonentType = (short)UtilityPaymentOrder.AbonentType,
                    CommunalType = (CommunalTypes)UtilityPaymentOrder.CommunalType,
                    AbonentNumber = UtilityPaymentOrder.Code,
                    PhoneNumber = UtilityPaymentOrder.PhoneNumber,
                    Branch = UtilityPaymentOrder.Branch,
                    PaymentType = UtilityPaymentOrder.PaymentType
                };
                UtilityPaymentOrder.Description = searchCommunal.GetCommunalPaymentDescription();
                Description = searchCommunal.SearchCommunalByType(Source)[0].Description;
                if (UtilityPaymentOrder.Description != null)
                {
                    if (searchCommunal.CommunalType == CommunalTypes.UCom)
                    {
                        PeriodicDescription = UtilityPaymentOrder.Description;
                    }
                    else
                    {
                        int lastIdx = UtilityPaymentOrder.Description.IndexOf("\r\n");
                        PeriodicDescription = searchCommunal.CommunalType != CommunalTypes.Trash ? (UtilityPaymentOrder.Description.Substring(0, lastIdx == -1 ? UtilityPaymentOrder.Description.IndexOf('/') : lastIdx)) : UtilityPaymentOrder.Description;
                    }


                }
                else
                {
                    PeriodicDescription = UtilityPaymentOrder.Description;

                }

                this.StartDate = Utility.GetNextOperDay();
            }
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
                result = PeriodicTransferOrderDB.SavePeriodicUtilityPaymentOrder(this, userName, source);

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

            if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }


            return result;
        }
        /// <summary>
        /// Ստուգում է  գոյություն ունի գործող պարբերական թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool IsAlreadyExistsThisCommunal()
        {
            return PeriodicTransferOrderDB.IsAlreadyExistsThisCommunal((short)this.UtilityPaymentOrder.CommunalType, this.ServicePaymentType, this.UtilityPaymentOrder.Code, this.UtilityPaymentOrder.Branch);
        }
        /// <summary>
        /// Ստուգում է կա պարբերականի ուղարկված բայց չհաստատված հայտ
        /// </summary>
        /// <returns></returns>
        public bool IsAlreadyExistsCommunalTransfersHBDocument()
        {
            return PeriodicTransferOrderDB.IsAlreadyExistsCommunalTransfersHBDocument(this.CustomerNumber, (short)this.UtilityPaymentOrder.CommunalType, this.ServicePaymentType, this.UtilityPaymentOrder.Code, this.UtilityPaymentOrder.Branch);
        }
        /// <summary>
        /// Հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrderForSend().Errors);
            if (IsAlreadyExistsThisCommunal() == true)
            {
                if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.VivaCell || this.UtilityPaymentOrder.CommunalType == CommunalTypes.Orange || this.UtilityPaymentOrder.CommunalType == CommunalTypes.ArmenTel || this.UtilityPaymentOrder.CommunalType == CommunalTypes.BeelineInternet)
                {
                    //Տվյալ հեռախոսահամարի գծով առկա է պարբերական փոխանցում: Անհրաժեշտ է սկզբում կատարել այդ պարբերական փոխանցման դադարեցում:
                    result.Errors.Add(new ActionError(325));
                }
                else
                {
                    //Տվյալ բաժանորդային քարտի գծով առկա է պարբերական փոխանցում: Անհրաժեշտ է սկզբում կատարել այդ պարբերական փոխանցման դադարեցում:
                    result.Errors.Add(new ActionError(324));
                }
            }
            if (IsAlreadyExistsCommunalTransfersHBDocument() == true)
            {
                if (this.UtilityPaymentOrder.CommunalType == CommunalTypes.VivaCell || this.UtilityPaymentOrder.CommunalType == CommunalTypes.Orange || this.UtilityPaymentOrder.CommunalType == CommunalTypes.ArmenTel || this.UtilityPaymentOrder.CommunalType == CommunalTypes.BeelineInternet)
                {
                    //Նշված հեռախոսահամարի գծով առկա է ուղարկված,սակայն չհաստատված պարբերական փոխանցում: 
                    result.Errors.Add(new ActionError(327));
                }
                else
                {
                    //Նշված բաժանորդային քարտի գծով առկա է ուղարկված,սակայն չհաստատված պարբերական փոխանցում: 
                    result.Errors.Add(new ActionError(326));
                }

            }
            return result;
        }
        /// <summary>
        /// Կոմունալ պարբերական հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            PeriodicTransferOrderDB.GetPeriodicUtilityPaymentOrder(this);
        }
    }
}
