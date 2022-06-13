using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class AccountOrder : Order
    {
        /// <summary>
        /// Որոշում է հաշիվը համատեղ է թե ոչ
        /// </summary>
        public ushort AccountType { get; set; }

        /// <summary>
        /// Համատեղ հաճախորդի համարը  
        /// </summary>
        public List<KeyValuePair<ulong, string>> JointCustomers { get; set; }

        /// <summary>
        /// Քաղվածքի սատցման եղանակ
        /// </summary>
        public short? StatementDeliveryType { get; set; }

        /// <summary>
        /// Քաղվածքի սատցման եղանակի նկարագրություն
        /// </summary>
        public string StatementDeliveryTypeDescription { get; set; }

        /// <summary>
        /// Հաշվի տեսակ
        /// </summary>
        public short AccountStatus { get; set; }


        /// <summary>
        ///  Հաշվի սահմանափակման խումբ
        /// </summary>
        public uint RestrictionGroup { get; set; }

        /// <summary>
        /// Սնանկության գործով կառավարիչ
        /// </summary>
        public ulong? BankruptcyManager { get; set; }

        /// <summary>
        ///Հաշվի հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }


            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            if (RestrictionGroup != 18)
            {
                result.Errors.AddRange(Validation.ValidateAccountOrderDocument(this));
            }

            if (Validation.CheckCustomerPhoneNumber(this.CustomerNumber))
            {
                //Հաճախորդի համար չկա մուտքագրված հեռախոսահամար:
                result.Errors.Add(new ActionError(1904));
            }

            //else
            //{              
            //    if (!Validation.ValidateRestrictionAccountOrder(this)) //ToDo sahmanapak hashvi hamar jamanakavorapes ancnum enq validacian minchev busines@ kta konkret validacianeri cucak@
            //    {
            //        //Գրանցումը հնարավոր չէ իրականցնել։ Խնդրում ենք մոտենալ Բանկի մոտակա մասնաճյուղ։
            //        result.Errors.Add(new ActionError(1804));
            //    }
            //}
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի բացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            AccountOrderDB.GetAccountOrder(this);
        }
        /// <summary>
        /// Հաշվի բացման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft) && result.ResultCode == ResultCode.Normal)
            {
                Account.GetCurrentAccountContractBefore(Id, CustomerNumber, 3);
            }
            

            if ((Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                || (((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && OrderAttachment.HasAttachedFile(this.Id, 3))))
            {
                if (result.ResultCode == ResultCode.Normal)
                {

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                    {
                        result = base.Approve(schemaType, userName);
                        if (result.ResultCode == ResultCode.Normal)
                        {
                            LogOrderChange(user, Action.Update);
                            scope.Complete();
                        }
                    }
                }
            }
            if (result.Errors.Count > 0)
            {
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                base.SetQualityHistoryUserId(this.Quality, user.userID);

                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }
        /// <summary>
        /// Հաշվի բացման հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (AccountDB.HasAccountOrder(this.Currency, this.CustomerNumber))
                    // Նշված արժույթով ընթացիկ հաշիվ արդեն առկա է: Նույն արժույթով ևս մեկ ընթացիկ հաշիվ բացելու համար անհրաժեշտ է մոտենալ Բանկի Ձեզ սպասարկող մասնաճյուղ:
                    result.Errors.Add(new ActionError(1816));
            }

            if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking) && this.RestrictionGroup != 18)
            {

                result.Errors.AddRange(Validation.ValidateKYCDocument(CustomerNumber,this.Source,true));

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
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            //Համատեղ հաճախորդի անուն ազգանուն 
            //if (this.AccountType == 2 && (this.JointCustomerNumber.ToString()).Length == 12)

            if (this.JointCustomers == null)
            {
                this.JointCustomers = new List<KeyValuePair<ulong, string>>();
            }

            if ((this.AccountType == 2 || this.AccountType == 3) && (this.JointCustomers.Count > 0))
            {
                ACBAServiceReference.Customer jointCustomer;
                short jointCustomerType;

                List<KeyValuePair<ulong, string>> jointList = new List<KeyValuePair<ulong, string>>();


                this.JointCustomers.ForEach(m =>
                {
                    if (m.Key.ToString().Length == 12)
                    {

                        jointCustomer = ACBAOperationService.GetCustomer(m.Key);
                        jointCustomerType = jointCustomer.customerType.key;

                        string jointCustomerFullName = "";

                        if (jointCustomer.identityId != 0 && jointCustomerType == (short)CustomerTypes.physical)
                        {
                            jointCustomerFullName = (jointCustomer as PhysicalCustomer).person.fullName.firstName + " " + (jointCustomer as PhysicalCustomer).person.fullName.lastName;

                        }

                        jointList.Add(new KeyValuePair<ulong, string>(key: m.Key, value: jointCustomerFullName));
                    }
                });

                this.JointCustomers = jointList;


            }
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        ///Հաշվի բացման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, bool forRestrictedAccount = false)
        {

            this.Complete(source);
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

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
                result = AccountOrderDB.Save(this, userName, source);

                //if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                //{
                //    //**********  
                //    ulong orderId = base.Save(this, source, user);
                //    //**********
                //}

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
                else
                {
                    return result;
                }
            }



            if (Source != SourceType.AcbaOnline)
            {
                if (forRestrictedAccount)
                {
                    result = OrderDB.ConfirmRestrictedOrder(this.Id, user);
                }
                else
                {
                    result = base.Confirm(user);
                }


                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(CheckAfterOrderConfirm(this));
                    result.Errors = warnings;
                }
                warnings.AddRange(CheckAfterOrderConfirm(this));
                result.Errors = warnings;
            }



            return result;
        }

        /// <summary>
        /// Հաշվի բացման վերաբերյալ զգուշացումների վերադարձ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<string> GetAccountOpenWarnings(ulong customerNumber, Culture culture)
        {

            List<string> warnings = new List<string>();
            short customerType;
            ActionResult result = new ActionResult();

            customerType = ACBAOperationService.GetCustomerType(customerNumber);

            if (customerType == (short)CustomerTypes.physical || customerType == (short)CustomerTypes.physCustomerUndertakings)
            {
                result.Errors.AddRange(Validation.ValidateCustomerDocument(customerNumber));
                Localization.SetCulture(result, culture);

                result.Errors.ForEach(m =>
                {
                    warnings.Add(m.Description);
                }
                );
            }
            return warnings;
        }

        public List<ActionError> CheckAfterOrderConfirm(AccountOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            if (ACBAOperationService.GetCustomerRiskQuality(order.CustomerNumber).key == 3)
            {
                //Հաշիվը կսառեցվի, քանի որ հաճախորդը բարձր ռիսկային է
                result.Add(new ActionError(637, new string[] { " հաճախորդը բարձր ռիսկային է" }));
            }

            if (ACBAOperationService.GetCustomerType(order.CustomerNumber) != (short)CustomerTypes.physical)
            {

                if ((order.Type == OrderType.CurrentAccountReOpen && order.AccountStatus == 0) || (order.Type == OrderType.CurrentAccountOpen && order.AccountStatus == 1))
                {
                    //Հաշիվը կսառեցվի, քանի որ հաստատված ստորագրության նմուշը բացակայում է
                    result.Add(new ActionError(638));
                }

                //Հաշիվը կսառեցվի, քանի որ հաշիվը հարկային տեսչություն ուղարկման ենթակա է
                result.Add(new ActionError(720));
            }

            return result;
        }


        public static KeyValuePair<uint, uint> GetRestrictionType(uint restGroup)
        {
            KeyValuePair<uint, uint> result = new KeyValuePair<uint, uint>();
            if (restGroup == 1)
            {
                result = new KeyValuePair<uint, uint>(116, 24);
            }
            else if (restGroup == 18)
            {
                result = new KeyValuePair<uint, uint>(18, 283);    //Սահմանափակ հասանելիությամ հաշվիներ
            }
            else if (restGroup == 4)
            {
                result = new KeyValuePair<uint, uint>(119, 24);    //Սահմանափակ հասանելիությամ հաշվիներ
            }

            else if (restGroup == 3)
            {
                result = new KeyValuePair<uint, uint>(118, 24);    //Սահմանափակ հասանելիությամ հաշվիներ
            }
            else
            {
                result = new KeyValuePair<uint, uint>(10, 24);
            }

            return result;
        }
        public static byte[] GetOpenedAccountContract(string accountNumber)
        {
            return AccountOrderDB.GetOpenedAccountContract(accountNumber);
        }
    }
}
