using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace ExternalBanking
{
    public class PlasticCardOrder : Order
    {
        /// <summary>
        /// Քարտ
        /// </summary>
        public PlasticCard PlasticCard { get; set; }
        /// <summary>
        /// Քարտապանի ունիկալ համար
        /// </summary>
        public ulong IdentityId { get; set; }
        /// <summary>
        /// Գաղտնաբառ
        /// </summary>
        public string MotherName { get; set; }
        /// <summary>
        /// Հայտի տեսակ ` Նոր,Վերաթողարկում...
        /// </summary>
        public uint CardActionType { get; set; }
        ///// <summary>
        ///// Հայտի կարգավիճակ
        ///// </summary>
        //public double CashLimit { get; set; }
        ///// <summary>
        ///// Հայտի կարգավիճակ
        ///// </summary>
        //public double PurchaseLimit { get; set; }
        /// <summary>
        /// Ներգրավողի ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }
        /// <summary>
        /// Սպասարկողի ՊԿ
        /// </summary>
        public int ServingSetNumber { get; set; }
        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Քաղվածքի ստացման եղանակ
        /// </summary>
        public int? CardReportReceivingType { get; set; }
        /// <summary>
        /// Քաղվածքի ստացման եղանակի նկարագրություն
        /// </summary>
        public string CardReportReceivingTypeDescription { get; set; }
        /// <summary>
        /// PIN-ի ստացման եղանակ
        /// </summary>
        public int CardPINCodeReceivingType { get; set; }
        /// <summary>
        /// PIN-ի ստացման եղանակ նկարագրություն
        /// </summary>
        public string CardPINCodeReceivingTypeDescription { get; set; }
        /// <summary>
        /// Հաստատության անվանում
        /// </summary>
        public string OrganisationNameEng { get; set; }
        /// <summary>
        /// Քարտապանի հաճախորդի համար
        /// </summary>
        public ulong CardHolderCustomerNumber { get; set; }
        /// <summary>
        /// SMS հեռախոսահամար
        /// </summary>
        public string CardSMSPhone { get; set; }
        /// <summary>
        /// Էլեկտրոնային փոստ
        /// </summary>
        public string ReportReceivingEmail { get; set; }
        /// <summary>
        /// PIN-ի ստացման եղանակի հեռախոսահամար 
        /// </summary>
        public string CardPINCodeReceivingPhone { get; set; }
        /// <summary>
        /// Քարտի սպասարկման վարձի պարբերականության տեսակ
        /// </summary>
        public ServiceFeePeriodicityType ServiceFeePeriodicityType { get; set; }
        /// <summary>
        /// Տրամադրման մասնաճյուղ
        /// </summary>
        public ushort ProvidingFilialCode { get; set; }
        /// <summary>
        /// Տրամադրման մասնաճյուղ նկարագրություն
        /// </summary>
        public string ProvidingFilialCodeDescription { get; set; }
        /// <summary>
        /// Կից քարտի ամսեկան սահմանաչափ
        /// </summary>
        public double LinkedCardLimit { get; set; } = -1;
        /// <summary>
        /// Հաճախորդի անգլերեն թարգմանված հասցեն
        /// </summary>
        public string AdrressEngTranslated { get; set; }
        /// <summary>
        /// Քարտի տեխնոլոգիա
        /// </summary>
        public string CardTechnology { get; set; }
        /// <summary>
        /// Քարտի ստացման եղանակ
        /// </summary>
        public int CardReceivingType { get; set; }
        /// <summary>
        /// Քարտի ստացման եղանակի նկարագրություն
        /// </summary>
        public string CardReceivingTypeDescription { get; set; }
        /// <summary>
        /// Դիմումի ընդունման տարբերակ
        /// </summary>
        public int CardApplicationAcceptanceType { get; set; }


        private void Complete()
        {
            this.SubType = 1;
            switch (this.PlasticCard.SupplementaryType)
            {
                case SupplementaryType.Main:
                    this.Type = OrderType.PlasticCardOrder;
                    break;
                case SupplementaryType.Attached:
                    this.Type = OrderType.AttachedPlasticCardOrder;
                    break;
                case SupplementaryType.Linked:
                    this.Type = OrderType.LinkedPlasticCardOrder;
                    break;
            }

            this.RegistrationDate = DateTime.Now.Date;

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.Bank)
            {
                this.UserId = user.userID;
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {

                if (PlasticCard.SupplementaryType == SupplementaryType.Attached)
                {
                    this.MotherName = "";
                    var card = Card.GetCard(PlasticCard.MainCardNumber, CustomerNumber);
                    ProvidingFilialCode = (ushort)card.FilialCode;
                    PlasticCard.Currency = card.Currency;
                    if (card.Type == 41)
                    {
                        PlasticCard.RelatedOfficeNumber = 2650;
                    }
                    else if (card.RelatedOfficeNumber == 174)
                    {
                        PlasticCard.RelatedOfficeNumber = 940;
                    }
                    else if (card.RelatedOfficeNumber == 24)
                    {
                        PlasticCard.RelatedOfficeNumber = 24;
                    }
                    else if (card.RelatedOfficeNumber != 174 && (PlasticCard.CardType == 34 || PlasticCard.CardType == 50))
                    {
                        PlasticCard.RelatedOfficeNumber = 1946;
                    }
                    else
                    {
                        if (card.FeeForCashTransaction * 100 == 0.5)
                        {
                            PlasticCard.RelatedOfficeNumber = 1998;
                        }
                        else if (card.FeeForCashTransaction * 100 == 0)
                        {
                            PlasticCard.RelatedOfficeNumber = 1999;
                        }
                        else if (card.FeeForCashTransaction * 100 == 1)
                        {
                            PlasticCard.RelatedOfficeNumber = 940;
                        }
                    }
                }
                else if (PlasticCard.SupplementaryType == SupplementaryType.Linked)
                {
                    var card = Card.GetCard(PlasticCard.MainCardNumber, CustomerNumber);
                    ProvidingFilialCode = (ushort)card.FilialCode;
                    PlasticCard.CardType = (uint)card.Type;
                    PlasticCard.Currency = card.Currency;
                    if (PlasticCard.CardType == 34 || PlasticCard.CardType == 40 || PlasticCard.CardType == 41 || PlasticCard.CardType == 50)
                    {
                        PlasticCard.RelatedOfficeNumber = 1314;
                    }
                    else
                    {
                        PlasticCard.RelatedOfficeNumber = card.RelatedOfficeNumber;
                    }
                }

                InvolvingSetNumber = 88;
                ACBAServiceReference.Customer customer = Customer.GetCustomer(CustomerNumber);
                CustomerServingEmployee cse = new CustomerServingEmployee();
                cse = customer.servingEmployeeList.Find(m => m.type.key == 2);

                if(cse == null)
                {
                    ServingSetNumber = InvolvingSetNumber;
                }
                else if (cse.servingEmployee.setNumber != 0 && this.PlasticCard.CardType != (uint)PlasticCardType.VISA_VIRTUAL)
                {
                    ServingSetNumber = (int)cse.servingEmployee.setNumber;
                }
                else
                {
                    ServingSetNumber = InvolvingSetNumber;
                }

                // PIN կոդի ստացման եղանակը ՝ SMS-ի միջոցով
                CardPINCodeReceivingType = 2;


                ACBAOperationServiceClient client = new ACBAOperationServiceClient();
                CustomerMainData mainData;
                mainData = (CustomerMainData)client.GetCustomerMainData(CustomerNumber);
                if (mainData.CustomerType != (byte)CustomerTypes.physical)
                {
                    PlasticCard.RelatedOfficeNumber = 174;
                    OrganisationNameEng = mainData.CustomerDescriptionEng;
                }
                else
                {
                    OrganisationNameEng = null;
                }


                if (PlasticCard.CardType == (uint)PlasticCardType.VISA_VIRTUAL)
                {
                    this.ProvidingFilialCode = (ushort)customer.filial.key;
                }
                    

                if (PlasticCard.SupplementaryType != SupplementaryType.Linked)
                {
                    if (mainData.Phones.Count != 0)
                    {
                        var mainMobilePhone = mainData.Phones.FirstOrDefault(x => x.phoneType.key == 1);
                        string number = mainMobilePhone == null? "37400000000" : 
                            mainMobilePhone.phone.countryCode.Substring(1) 
                            + mainMobilePhone.phone.areaCode 
                            + mainMobilePhone.phone.phoneNumber;
                        CardSMSPhone = number;
                    }
                    else
                    {
                        CardSMSPhone = "37400000000";
                    }
                }
                if (PlasticCard.SupplementaryType == SupplementaryType.Main)
                {
                    if (mainData.Emails.Count != 0)
                    {
                        ReportReceivingEmail = mainData.Emails.Last().email.emailAddress;
                    }
                }

                if (mainData.CustomerType == (byte)CustomerTypes.physical)
                {
                    short periodicityType = 0;
                    if (ServiceFeePeriodicityType == ServiceFeePeriodicityType.Monthly)
                    {
                        periodicityType = 1;
                    }
                    else if (ServiceFeePeriodicityType == ServiceFeePeriodicityType.Yearly)
                    {
                        periodicityType = 12;
                    }

                    if (PlasticCard.SupplementaryType == SupplementaryType.Main)
                    {
                        PlasticCard.RelatedOfficeNumber = Info.GetCardOfficeTypesForIBanking(Convert.ToUInt16(PlasticCard.CardType), periodicityType);
                    }
                }


                //CardSystem-ի որոշում՝ ըստ Card Type-ի
                if (PlasticCard.CardType == 34 || PlasticCard.CardType == 40 || PlasticCard.CardType == 41 || PlasticCard.CardType == 50)
                {
                    PlasticCard.CardSystem = 3;
                }
                else if (PlasticCard.CardType == 16 || PlasticCard.CardType == 45 || PlasticCard.CardType == 18 || PlasticCard.CardType == 29 || PlasticCard.CardType == 46 ||
                    PlasticCard.CardType == 36 || PlasticCard.CardType == 37 || PlasticCard.CardType == 38 || PlasticCard.CardType == (uint)PlasticCardType.VISA_VIRTUAL || PlasticCard.CardType == 52)
                {
                    PlasticCard.CardSystem = 4;
                }
                else if ((PlasticCard.CardType == 47 || PlasticCard.CardType == 14 || PlasticCard.CardType == 48 || PlasticCard.CardType == 49 ||
                    PlasticCard.CardType == 32))
                {
                    PlasticCard.CardSystem = 5;
                }
                else if (PlasticCard.CardType == 11 || PlasticCard.CardType == 21 || PlasticCard.CardType == 22 || PlasticCard.CardType == 31 ||
                    PlasticCard.CardType == 35 || PlasticCard.CardType == 39 || PlasticCard.CardType == 42 || PlasticCard.CardType == 43)
                {
                    PlasticCard.CardSystem = 9;
                }

                this.PlasticCard.OpenDate = this.RegistrationDate.Date;

                if (PlasticCard.CardType == 52)
                    PlasticCard.RelatedOfficeNumber = 174;
                
            }
        }

        /// <summary>
        /// Քարտային հայտի պահպանում
        /// </summary>
        /// <param name="user">Հայտը մուտքագրող/խմբագրող աշխատակից</param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(ACBAServiceReference.User user, SourceType source, ulong customerNumber, short schemaType)
        {
            this.Complete();

            ActionResult result = this.Validate(customerNumber);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PlasticCardOrderDB.Save(this, user, source);                

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                LogOrderChange(user, action);

                ActionResult res = base.Approve(schemaType, user.userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    this.SubType = 1;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);

                }
                else
                {
                    return res;
                }
                scope.Complete();
            }

            result = base.Confirm(user);


            if (result.ResultCode == ResultCode.Normal)
            {
                PlasticCardOrder plasticCardOrder = new PlasticCardOrder();
                try
                {
                    plasticCardOrder = PlasticCardOrderDB.GetPlasticCardOrder(this);
                }
                catch (Exception ex)
                {
                    result.ResultCode = ResultCode.DoneErrorInFurtherActions;
                    return result;
                }

                if (plasticCardOrder.PlasticCard.CardNumber != "" && plasticCardOrder.Quality == OrderQuality.Completed)
                {
                    result.ResultCode = ResultCode.DoneAndReturnedValues;

                    ActionError actionError = new ActionError
                    {
                        Description = plasticCardOrder.PlasticCard.CardNumber
                    };

                    result.Errors.Add(actionError);
                }
                else if (plasticCardOrder.PlasticCard.CardNumber == "" && plasticCardOrder.Quality == OrderQuality.TransactionLimitApprovement)
                {
                    result.ResultCode = ResultCode.SaveAndSendToConfirm;
                }
            }

            return result;
        }
        /// <summary>
        /// Քարտային հայտի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.PlasticCard.CardType == 0)
            { //Քարտի տեակը ընտրված չէ:
                result.Errors.Add(new ActionError(1578));
            }

            if (this.Currency == "")
            {// Արժույթը ընտրված չէ:
                result.Errors.Add(new ActionError(1579));
            }

            if (InvolvingSetNumber == 0)
            {//Նշեք ՊԿ-Ն
                result.Errors.Add(new ActionError(1589));
            }           

            if (this.PlasticCard.SupplementaryType == SupplementaryType.Main)
            {
                int quantity = 0;
                quantity = PlasticCardOrderDB.PlasticCardQuantityRestriction(this);
                if (quantity > 0)
                {//Տվյալ քարտապանի համար քարտերի թույլատրելի քանակը գերազանցված է:
                    result.Errors.Add(new ActionError(1636));
                }

            }
            if (this.PlasticCard.CardType == (int)PlasticCardType.ACBA_FEDERATION)
            {
                if (this.PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.GPMMembers)
                {//ACBA Federation քարտի համար անհրաժեշտ է ընտրել 2769 աշխատավարձային ծրագիրը: 
                    result.Errors.Add(new ActionError(1650));
                }

                int customerGPMStatus = (int)PlasticCardOrderDB.CheckCustomerGPMStatus(this.CustomerNumber);

                if (customerGPMStatus == (int)GPMMemberStatus.NoMemberDate)
                {//Անհրաժեշտ է մուտքագրել ԳՓՄ անդամակցության ամսաթիվը:
                    result.Errors.Add(new ActionError(1651));
                }

                if (customerGPMStatus == (int)GPMMemberStatus.NotMember)
                {//ACBA Federation քարտատեսակը հնարավոր է պատվիրել միայն ԳՓՄ անդամների համար
                    result.Errors.Add(new ActionError(1652));
                }
            }
            if (this.PlasticCard.SupplementaryType != SupplementaryType.Main)
            {
                PlasticCard mainplasticCard = PlasticCardOrderDB.GetMainCard(this.PlasticCard.MainCardNumber);
                if (this.FilialCode != mainplasticCard.FilialCode && (mainplasticCard.FilialCode != 22000 || this.FilialCode != 22059))
                {
                    if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                    {
                        //Այլ մասնաճյուղի քարտ:
                        result.Errors.Add(new ActionError(1653));
                        return result;
                    }
                }
                if (ulong.TryParse(this.PlasticCard.MainCardNumber, out ulong mainCardNumber) && mainCardNumber == Constants.AMEX_PAPE_AmEx_CARD_NUMBER)
                {//AmEx տիպի քարտի համար հնարավոր չէ կատարել ընտրված գործողությունը:
                    result.Errors.Add(new ActionError(1656));
                    return result;
                }
                if (user.filialCode == 22059 && mainplasticCard.FilialCode == 22000)
                {
                    this.PlasticCard.FilialCode = mainplasticCard.FilialCode;
                }
            }
            if (this.PlasticCard.SupplementaryType != SupplementaryType.Attached)
            {
                if (this.MotherName.Length > 24)
                { // Մայրանուն դաշտի նիշերի քանակը գերազանցում է թույլատրելի 24 նիշը:
                    result.Errors.Add(new ActionError(1580));
                }
                else if (this.MotherName.Contains(" ") || this.MotherName.Contains((char)9) || this.MotherName.Contains((char)10) || this.MotherName.Contains((char)13))
                { //Մայրանուն դաշտում առկա է անթույլատրելի նիշ:
                    result.Errors.Add(new ActionError(1581));
                }
            }

            int productType = Utility.GetCardProductType(this.PlasticCard.CardType);

            if (!Validation.CheckProductAvailabilityByCustomerCountry(customerNumber, productType))
            { // Հաճախորդը չի կարող դիմել տվյալ տիպի քարտերի համար քաղաքացիություն,ռեզիդենտություն կամ կենսական շահերի կենտրոն հանդիսացող երկրի պատճառով:
                result.Errors.Add(new ActionError(1591));
            }

            if (this.PlasticCard.RelatedOfficeNumber == 0)
            {//Ընտրեք աշխատավարձային ծրագիրը:
                result.Errors.Add(new ActionError(1592));
            }
            else
            {
                if ((this.PlasticCard.RelatedOfficeNumber == (int)RelatedOfficeTypes.AMexPAPEYearly || this.PlasticCard.RelatedOfficeNumber == (int)RelatedOfficeTypes.AmexPAPEMonthly) &&
                    (user.DepartmentId != 220 && user.DepartmentId != 223))
                {//Նշված քարտային ծրագրով քարտ կարող է պատվիրել միայն Բանկի քարտային կենտրոնը:
                    result.Errors.Add(new ActionError(1647));
                }

                int contractStatus = PlasticCardOrderDB.GetRelatedOfficeContractStatus(this.PlasticCard.RelatedOfficeNumber);
                if (contractStatus == (int)RelatedOfficeContractStatusTypes.Suspended || contractStatus == (int)RelatedOfficeContractStatusTypes.Frozen)
                {//Աշխատավարձային ծրագիրը դադարեցված է կամ սառեցված է:
                    result.Errors.Add(new ActionError(1624));
                }
                else if (contractStatus == -1)
                {//Աշխատավարձային ծրագիրը գտնված չէ:
                    result.Errors.Add(new ActionError(1625));
                }
                else if (!PlasticCardOrderDB.CheckPlatsicCardRelatedOffice(this))
                {//Տվյալ աշխատավարձային ծրագրում նախատեսված չէ տվյալ արժույթով տվյալ քարտի տեսակը
                    result.Errors.Add(new ActionError(1633));
                }
            }

            if (RegistrationDate == null)
            {//Քարտի դիմումի ամսաթիվ դաշտի լրացումը պարտադիր է:
                result.Errors.Add(new ActionError(1593));
            }

            if (this.PlasticCard.CardType == (uint)PlasticCardType.ARCA_PENSION & this.PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.PensionCards)
            {//Աշխատավարձային ծրագիրը չի համապատասխանում ընտրված քարտի տեսակին
                result.Errors.Add(new ActionError(1594));
            }

            if (!PlasticCardOrderDB.CheckCustomerDefaultDocument(customerNumber))
            {// Հաճախորդի հիմնական փաստափուղթը լրացված չէ:
                result.Errors.Add(new ActionError(1588));
            }

            List<PlasticCard> maincards = new List<PlasticCard>();
            maincards = PlasticCard.GetCustomerMainCards(customerNumber);

            if (this.PlasticCard.CardType == 51)//Միայն վիրտուալ քարտերի դեպքում
            {
                if (maincards.FindAll(m => m.CardType == (uint)PlasticCardType.VISA_VIRTUAL && m.Currency == this.PlasticCard.Currency).Count >= 1)
                {//Քարտի պատվերը հնարավոր չէ իրականացնել: Միևնույն արժույթով հնարավոր է ունենալ միայն մեկ վիրտուալ քարտ:
                    result.Errors.Add(new ActionError(1718));
                }
            }

            ACBAServiceReference.Customer customer = Customer.GetCustomer(customerNumber);


            ACBAOperationServiceClient client = new ACBAOperationServiceClient();
            CustomerMainData mainData;
            mainData = (CustomerMainData)client.GetCustomerMainData(CustomerNumber);
            if (PlasticCard.SupplementaryType == SupplementaryType.Main)
            {
                if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
                {
                    if (mainData.Emails.Count == 0)
                    {
                        //Էլեկտրոնային հասցեն մուտքագրված չէ
                        result.Errors.Add(new ActionError(446));
                    }
                }

                var infoTable = Info.GetCardReportReceivingTypes();
                bool ok = false;
                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    if (this.CardReportReceivingType == Convert.ToInt32(infoTable.Rows[i]["type_id"].ToString()))
                    {
                        ok = true;
                    }
                }
                if (ok == false && InfoDB.CommunicationTypeExistence(this.CustomerNumber) == 0)
                {
                    //Քաղվածքի ստացման եղանակը նշված չէ
                    result.Errors.Add(new ActionError(1708));
                }
            }
            else if (PlasticCard.SupplementaryType == SupplementaryType.Linked)
            {
                if (CardSMSPhone == "" || CardSMSPhone is null)
                {
                    //Բջջային հեռախոսահամարը մուտքագրված չէ
                    result.Errors.Add(new ActionError(464));
                }
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                if ((PlasticCard.CardType == 34 || PlasticCard.CardType == 40 || PlasticCard.CardType == 41 || PlasticCard.CardType == 50)
               && ServiceFeePeriodicityType == ServiceFeePeriodicityType.None && this.Type == OrderType.PlasticCardOrder)
                {
                    //Քարտի սպասարկման վարձի պարբերականության տեսակն ընտրված չէ
                    result.Errors.Add(new ActionError(1741));
                }
            }

            if (PlasticCard.SupplementaryType == SupplementaryType.Attached)
            {//Լրացուցիչ քարտի ստուգումներ
                result.Errors.AddRange(ValidateAsAttachedPlasticCard().Errors);
            }

            if (PlasticCard.SupplementaryType == SupplementaryType.Linked)
            {//Կից քարտի ստուգումներ
                result.Errors.AddRange(ValidateAsLinkedPlasticCard().Errors);
            }

            if ((customer is PhysicalCustomer) && (PlasticCard.CardType == 3 || PlasticCard.CardType == 22 || PlasticCard.CardType == 45))
            {
                //Բիզնես քարտերը  նախատեսված չեն  ֆիզիկական անձ հաճախորդի համար:
                result.Errors.Add(new ActionError(1839));
            }

                //եթե գործարքը կատարվել է Օնլայն կամ մոբայլ համակարգով
                if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                //ֆիզիկական հաճախորդ 
                if (customer is PhysicalCustomer)
                {
                    if (this.ProvidingFilialCode == 0 && this.PlasticCard.CardType != 51)
                    {
                        //Տրամադրման մասնաճյուն ընտրված չէ
                        result.Errors.Add(new ActionError(1786));
                    }
                    if (this.PlasticCard.SupplementaryType == SupplementaryType.Main)
                    {
                        if (PlasticCardOrderDB.GetPlasticCardOrderCount(CustomerNumber)
                        + CardDB.GetCards(CustomerNumber).Count
                        + PlasticCardOrderDB.GetNotGivenCardsCount(CustomerNumber) >= 12)
                        {
                            //Քարտի պատվերը հնարավոր չէ իրականացնել օնլայն եղանակով: Խնդրում ենք մոտենալ Բանկի մասնաճյուղ:
                            result.Errors.Add(new ActionError(1742));
                        }
                    }

                }
                //ոչ ֆիզիկական հաճախորդ 
                else if (!(customer is PhysicalCustomer))
                {
                    if (Attachments == null || Attachments.Count == 0)
                    {
                        //Անհրաժեշտ է կցել քարտապանի Անձնագրի լուսանկարով էջի և ՀԾՀ-ի կամ Նույնականացման քարտի 2 կողմերի լուսապատճենները
                        result.Errors.Add(new ActionError(1787));
                    }
                    else
                    {
                        foreach (var attachment in Attachments)
                        {
                            if (String.IsNullOrEmpty(attachment.Id))
                            {
                                if (string.IsNullOrWhiteSpace(attachment.AttachmentInBase64))
                                {
                                    //Հայտին փաստաթուղթ կցված չէ
                                    result.Errors.Add(new ActionError(1704));
                                }
                                else if (!Validation.IsBase64String(attachment.AttachmentInBase64))
                                {
                                    //Մուտքագրված Base64 տողը վավեր չէ:
                                    result.Errors.Add(new ActionError(1707));
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(attachment.FileName))
                                    {
                                        //Կցված Փաստաթղթի անվանումը բացակայում է
                                        result.Errors.Add(new ActionError(1705));
                                    }
                                    if (string.IsNullOrWhiteSpace(attachment.FileExtension))
                                    {
                                        //Կցված Փաստաթղթի ընդլայնումը բացակայում է
                                        result.Errors.Add(new ActionError(1706));
                                    }
                                }
                            }
                        }
                    }
                }

                if (PlasticCard.SupplementaryType == SupplementaryType.Linked)
                {
                    if (PlasticCard.CardType == 21)
                    {
                        //Տվյալ քարտատեսակի համար կից քարտի պատվեր հնարավոր չէ իրականացնել:
                        result.Errors.Add(new ActionError(1702));
                    }
                    else if (PlasticCard.CardType == 41 || PlasticCard.CardType == 42)
                    {
                        int cardsCount = PlasticCardOrderDB.GetLinkedCardsCount(PlasticCard.MainCardNumber);
                        if (cardsCount >= 4)
                        {
                            //Տվյալ քարտատեսակի համար համար կից քարտի պատվեր այլևս հնարավոր չէ իրականացնել: Դուք գերազանցում եք կից քարտերի հնարավոր առավելագույն քանակը
                            result.Errors.Add(new ActionError(1703));
                        }
                    }
                    else if (PlasticCard.CardType != 22 || PlasticCard.CardType != 45)
                    {
                        int cardsCount = PlasticCardOrderDB.GetLinkedCardsCount(PlasticCard.MainCardNumber);
                        if (cardsCount >= 2)
                        {
                            //Տվյալ քարտատեսակի համար համար կից քարտի պատվեր այլևս հնարավոր չէ իրականացնել: Դուք գերազանցում եք կից քարտերի հնարավոր առավելագույն քանակը
                            result.Errors.Add(new ActionError(1703));
                        }
                    }

                    if (Attachments == null || Attachments.Count == 0)
                    {
                        //Անհրաժեշտ է կցել կից քարտապանի Անձնագրի լուսանկարով էջի և ՀԾՀ-ի կամ Նույնականացման քարտի 2 կողմերի լուսապատճենները:
                        result.Errors.Add(new ActionError(1700));
                    }
                    else
                    {
                        foreach (var attachment in Attachments)
                        {
                            if (String.IsNullOrEmpty(attachment.Id))
                            {
                                if (string.IsNullOrWhiteSpace(attachment.AttachmentInBase64))
                                {
                                    //Հայտին փաստաթուղթ կցված չէ
                                    result.Errors.Add(new ActionError(1704));
                                }
                                else if (!Validation.IsBase64String(attachment.AttachmentInBase64))
                                {
                                    //Մուտքագրված Base64 տողը վավեր չէ:
                                    result.Errors.Add(new ActionError(1707));
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(attachment.FileName))
                                    {
                                        //Կցված Փաստաթղթի անվանումը բացակայում է
                                        result.Errors.Add(new ActionError(1705));
                                    }
                                    if (string.IsNullOrWhiteSpace(attachment.FileExtension))
                                    {
                                        //Կցված Փաստաթղթի ընդլայնումը բացակայում է
                                        result.Errors.Add(new ActionError(1706));
                                    }
                                }
                            }
                        }
                    }
                    if (LinkedCardLimit != -1 && LinkedCardLimit <= 0)
                    {
                        //քարտի սահմանաչափը նշված չէ
                        result.Errors.Add(new ActionError(1701));
                    }
                }
            }
            if (this.CardReportReceivingType == (int)PlasticCardReportRecievingTypes.byEmail && String.IsNullOrEmpty(this.ReportReceivingEmail))
            {//Հաճախորդի Էլ. փոստ դաշտը լրացված չէ
                result.Errors.Add(new ActionError(1613));
            }



            if (customer is PhysicalCustomer physicalCustomer)
            {

                if (this.PlasticCard.SupplementaryType == SupplementaryType.Linked)
                {
                    if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                    {
                        ACBAServiceReference.PhysicalCustomer cardHolderCustomer = (PhysicalCustomer)Customer.GetCustomer(this.CardHolderCustomerNumber);
                        result.Errors.AddRange(ValidateAsPhysicalCustomer(cardHolderCustomer).Errors);
                    }

                }
                else
                {
                    result.Errors.AddRange(ValidateAsPhysicalCustomer(physicalCustomer).Errors);
                }


            }
            else if (customer is LegalCustomer legalcustomer)
            {
                ACBAServiceReference.Customer cardHolderCustomer = Customer.GetCustomer(this.CardHolderCustomerNumber);

                if (string.IsNullOrEmpty(this.OrganisationNameEng))
                {//Հաստատության անվանում դաշտը լրացված չէ
                    result.Errors.Add(new ActionError(1611));
                }
                else if (this.OrganisationNameEng.Length > 30)
                {//Հաստատության անվանում դաշտի նիշերի քանակը գերազանցում է թույլատրելի 30 նիշը:
                    result.Errors.Add(new ActionError(1585));
                }
                else if ((Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking) && !Regex.IsMatch(this.OrganisationNameEng, "^[A-Za-z- ./\\d]+$"))
                {//«Հաստատության անվանումը» դաշտում առկա է անթույլատրելի նշան:
                    result.Errors.Add(new ActionError(1612));
                }
                if (this.PlasticCard.CardType != (uint)PlasticCardType.ARCA_BUSINESS &&
                    this.PlasticCard.CardType != (uint)PlasticCardType.VISA_BSNS_CHP &&
                    this.PlasticCard.CardType != (uint)PlasticCardType.VISA_BUSINESS &&
                    this.PlasticCard.CardType != (uint)PlasticCardType.VISA_BUSINESS_PW)
                {//Նշված տիպի քարտ կարելի է պատվիրել միայն ֆիզիկական անձանց համար:
                    result.Errors.Add(new ActionError(1586));
                }

                if (cardHolderCustomer is PhysicalCustomer physicalCustomerCardHolder)
                {
                    result.Errors.AddRange(ValidateAsPhysicalCustomer(physicalCustomerCardHolder).Errors);
                }

                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                {
                    if (InfoDB.CommunicationTypeExistence(this.CustomerNumber) == 1 && this.CardReportReceivingType != null)
                    {
                        //Դիմումը հնարավոր չէ ուղարկել, քանի որ հաղորդակցման եղանակը սխալ է ընտրված։ Անհրաժեշտ է մուտքագրել նոր դիմում։
                        result.Errors.Add(new ActionError(1732));
                    }
                }


            }

            bool checkSocialSecurityAccount = Info.HasSocialSecurityAccount(customerNumber);

            if (this.PlasticCard.CardType == 21 && checkSocialSecurityAccount)
            {
                //Հաճախորդն ունի սոցիալական ապահովության նշանակությամբ հաշիվ
                result.Errors.Add(new ActionError(1881));
            }

            return result;
        }



        public ActionResult ValidateAsPhysicalCustomer(PhysicalCustomer physicalCustomer)
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (string.IsNullOrEmpty(physicalCustomer.person.fullName.firstNameEng) || string.IsNullOrEmpty(physicalCustomer.person.fullName.lastNameEng))
            {//Մուտքագրեք քարտապանի անունն ու ազգանունը:
                result.Errors.Add(new ActionError(1582));
            }
            else if (physicalCustomer.person.fullName.firstNameEng.Length + physicalCustomer.person.fullName.lastNameEng.Length > 25)
            { // Քարտապանի անվան և ազգանվան նիշերի քանակը գերազանցում է թույլատրելի 25 նիշը:
                result.Errors.Add(new ActionError(1583));
            }
            else
            {
                if (!Regex.IsMatch(physicalCustomer.person.fullName.firstNameEng, "^[A-Za-z- ]+$"))
                {//Քարտապանի անվան նիշերի մեջ առկա է անթույլատրելի սիմվոլ
                    result.Errors.Add(new ActionError(1608));
                }

                if (!Regex.IsMatch(physicalCustomer.person.fullName.lastNameEng, "^[A-Za-z- ]+$"))
                {//Քարտապանի ազգանվան նիշերի մեջ առկա է անթույլատրելի սիմվոլ
                    result.Errors.Add(new ActionError(1609));
                }
            }

            if (string.IsNullOrEmpty(physicalCustomer.person.fullName.MiddleName))
            {//Քարտապանի հայրանունը լրացված չէ
                result.Errors.Add(new ActionError(1614));
            }

            if (!physicalCustomer.person.birthDate.HasValue)
            {//Քարտապանի ծննդյան ա/թ-ն լրացված չէ
                result.Errors.Add(new ActionError(1615));
            }

            //if(this.CardReportReceivingType == (int)PlasticCardReportRecievingTypes.byEmail && physicalCustomer.person.emailList.Count == 0)
            //{//Հաճախորդի Էլ. փոստ դաշտը լրացված չէ
            //    result.Errors.Add(new ActionError(1613));
            //}

            //Երևի պետք չի
            CustomerAddress customerAddress = physicalCustomer.person.addressList.Find(m => m.addressType.key == (short)AddressTypes.registration);

            if (customerAddress != null)
            {
                //string customerCity = customerAddress.address.TownVillage.value;

                //if (customerCity.Length > 20)
                //{//Գրանցման հասցե` Քաղաք դաշտի նիշերի քանակը գերազանցում է թույլատրելի 20 նիշը:
                //    result.Errors.Add(new ActionError(1584));
                //}
            }
            else
            {
                //Գրանցման հասցե չկա
                result.Errors.Add(new ActionError(1607));
            }

            CustomerDocument socialServiceDocument = new CustomerDocument();
            socialServiceDocument = physicalCustomer.person.documentList.Find(m => m.documentType.key == (short)DocumentType.PublicServiceNumber);
            if (socialServiceDocument == null)
            {//Հաճախորդի hանրային ծառայությունների համարանիշը մուտքագրված չէ: 
                //result.Errors.Add(new ActionError(1587));
            }

            //string socSecurityNumber = PlasticCardOrderDB.GetSocSecurityNumber(physicalCustomer.customerNumber);

            //if (socSecurityNumber.Length > 30)
            //{//Անձնագիր դաշտի նիշերի քանակը գերազանցում է թույլատրելի 30 նիշը
            //    result.Errors.Add(new ActionError(1590));
            //}

            if (physicalCustomer.customerNumber == Constants.AMEX_PAPE_AmEx_CARD_NUMBER &&
                (this.PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.AmexGoldFoundersDirectors ||
                 this.PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.AmexPAPEMonthly ||
                 this.PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.AMexPAPEYearly))
            {//Տվյալ աշխատավարձային ծրագրում նախատեսված չէ տվյալ քարտի տեսակը
                result.Errors.Add(new ActionError(1616));
            }

            if (this.PlasticCard.RelatedOfficeNumber == (int)RelatedOfficeTypes.PensionCards)
            {
                ulong pensionContractID = PlasticCardOrderDB.GetCustomerPensionContractID(physicalCustomer.customerNumber);
                if (pensionContractID == 0)
                {//Հաճախորդի կենսաթոշակի դիմումի համարը մուտքագրված չէ
                    result.Errors.Add(new ActionError(1617));
                }
            }


            return result;
        }

        /// <summary>
        /// Լրացուցիչ քարտի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateAsAttachedPlasticCard()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (!PlasticCardOrderDB.GetRelatedOfficeNumbersForAttachedCards().Contains(PlasticCard.RelatedOfficeNumber))
            {//Նշված քարտային ծրագրով հնարավոր չէ պատվիրել լրացուցիչ քարտ:
                result.Errors.Add(new ActionError(1622));
            }

            if (!PlasticCardOrderDB.CheckAdditionalCardAvailability(PlasticCard.CardType, PlasticCard.MainCardNumber, 1))
            {//Տվյալ հիմնական քարտի համար նշված քարտատեսակով լրացուցիչ քարտի պատվիրումը թույլատրված չէ:
                result.Errors.Add(new ActionError(1618));
            }

            PlasticCard mainPlasticCard = PlasticCardOrderDB.GetMainCard(PlasticCard.MainCardNumber);

            if (mainPlasticCard.CardType == (uint)PlasticCardType.AMEX_GOLD_CHP && PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.AmexGoldMasterCardGold)
            {//AMEX GOLD քարի լրացուցիչ Master Gold քատրը անհրաժեշտ է պատվիրել 2650 աշխատավարձային ծրագրով
                result.Errors.Add(new ActionError(1619));
            }

            if (mainPlasticCard.Currency != this.PlasticCard.Currency)
            {//Հիմնական և լրացուցիչ քարտերի արժույթները տարբերվում են
                result.Errors.Add(new ActionError(1623));
            }

            if (PlasticCardOrderDB.CheckAdditionalCardConditions(PlasticCard.MainCardNumber))
            {//Տվյալ քարտատեսակի համար լրացուցիչ քարտի պատվիրումը թույլատրված չէ:
                result.Errors.Add(new ActionError(1657));
            }

            return result;
        }

        /// <summary>
        /// Կից քարտի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateAsLinkedPlasticCard()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            PlasticCard mainPlasticCard = PlasticCardOrderDB.GetMainCard(PlasticCard.MainCardNumber);

            if (mainPlasticCard.CardType != PlasticCard.CardType && !PlasticCardOrderDB.CheckAdditionalCardAvailability(PlasticCard.CardType, PlasticCard.MainCardNumber, 2))
            {//Կից քարտի տեսակը չի համընկնում հիմնական քարտի տեսակի հետ:
                result.Errors.Add(new ActionError(1620));
            }

            if (mainPlasticCard.RelatedOfficeNumber != PlasticCard.RelatedOfficeNumber && PlasticCard.CardSystem != (int)PlasticCardSystemType.Amex)
            {//Կից քարտի աշխատավարձային ծրագիրը տարբերվում է հիմնական քարտի աշխատավարձային ծրագրից
                result.Errors.Add(new ActionError(1634));
            }

            if (mainPlasticCard.Currency != PlasticCard.Currency)
            {//Կից քարտի արժութը տաբրերվում է հիմանական քարտի արժույթից
                result.Errors.Add(new ActionError(1635));
            }
            if (PlasticCard.CardSystem == (int)PlasticCardSystemType.Amex && PlasticCard.RelatedOfficeNumber != (int)RelatedOfficeTypes.AMEXLinkedCards)
            {//AmEx կից քարտեր հնարավոր է պատվիրել միայն 1314 քարտային ծրագրով:
                result.Errors.Add(new ActionError(1621));
            }

            return result;
        }



        private ActionResult RedirectToDocFlow(short schemaType)
        {
            if (PlasticCard.SupplementaryType == SupplementaryType.Main)
            {
                return DocFlowManagement.DocFlow.SendPlasticCardOrderToConfirm(this, user, schemaType);
            }
            else if (PlasticCard.SupplementaryType == SupplementaryType.Linked)
            {
                return DocFlowManagement.DocFlow.SendLinkedPlasticCardOrderToConfirm(this, user, schemaType);
            }
            else
                return DocFlowManagement.DocFlow.SendAttachedPlasticCardOrderToConfirm(this, user, schemaType);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի վերջին բանկային գաղտնաբառը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public string GetCustomerLastMotherName(ulong customerNumber)
        {
            return PlasticCardOrderDB.GetCustomerLastMotherName(customerNumber);
        }

        public string GetCustomerAddressEng(ulong customerNumber)
        {
            return PlasticCardOrderDB.GetCustomerAddressEng(customerNumber);
        }
        public ActionResult Save(ACBAServiceReference.User user, SourceType source, ulong customerNumber, string userName)
        {
            this.Complete();

            ActionResult result = this.Validate(customerNumber);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PlasticCardOrderDB.Save(this, user, source, userName);
                ACBAServiceReference.Customer customer = Customer.GetCustomer(customerNumber);
                if ((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && (customer.customerType.key != (byte)CustomerTypes.physical || PlasticCard.SupplementaryType == SupplementaryType.Linked))
                {
                    result.ResultCode = SaveOrderAttachments().ResultCode;
                }
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
            }
            return result;
        }
        public ActionResult Approve(ACBAServiceReference.User user, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = ValidateForSend();

                if (result.ResultCode == ResultCode.Normal)
                {
                    ActionResult res = base.Approve(schemaType, userName);
                    if (res.ResultCode == ResultCode.Normal)
                    {
                        if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
                        {
                            if (Customer.GetCustomer(CustomerNumber).customerType.key != (byte)CustomerTypes.physical)
                            {
                                user.userName = userName;
                                result = RedirectToDocFlow(schemaType);
                            }
                            else
                            {
                                if (PlasticCard.SupplementaryType != SupplementaryType.Main)
                                {
                                    result = RedirectToDocFlow(schemaType);
                                }
                                else
                                {
                                    PlasticCardOrderDB.GeneratePlasticCardOrderMessages(this);
                                }
                            }
                        }
                        this.Quality = OrderQuality.Sent3;
                        this.SubType = 1;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                    }
                    else
                    {
                        return res;
                    }
                    scope.Complete();
                }
                else
                {
                    return result;
                }


            }

            if (Customer.GetCustomer(CustomerNumber).customerType.key == (byte)CustomerTypes.physical && PlasticCard.SupplementaryType == SupplementaryType.Main)
            {
                OrderQuality quality = GetOrderQualityByDocID(Id);
                if (quality != OrderQuality.Approved)
                {
                    result = ConfirmOrderOnline(Id, user);
                }


                if (result.ResultCode == ResultCode.Normal)
                {
                    PlasticCardOrder plasticCardOrder = PlasticCardOrderDB.GetPlasticCardOrder(this);

                    if (plasticCardOrder.PlasticCard.CardNumber != "" && plasticCardOrder.Quality == OrderQuality.Completed)
                    {
                        result.ResultCode = ResultCode.DoneAndReturnedValues;

                        ActionError actionError = new ActionError
                        {
                            Description = plasticCardOrder.PlasticCard.CardNumber
                        };

                        result.Errors.Add(actionError);
                    }
                    else if (plasticCardOrder.PlasticCard.CardNumber == "" && plasticCardOrder.Quality == OrderQuality.TransactionLimitApprovement)
                    {
                        result.ResultCode = ResultCode.SaveAndSendToConfirm;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Վերադարձնում է քարտի պատվերի տվյալները
        /// </summary>
        /// <returns></returns>
        public PlasticCardOrder Get()
        {
            byte customerType = Customer.GetCustomerType(CustomerNumber);
            var order = PlasticCardOrderDB.GetPlasticCardOrder(this, Source);
            if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                && customerType != (byte)CustomerTypes.physical || (PlasticCard.SupplementaryType == SupplementaryType.Linked))
            {
                order.Attachments = Order.GetOrderAttachments(order.Id);
            }
            if (!string.IsNullOrWhiteSpace(PlasticCard.MainCardNumber))
            {
                PlasticCard.MainCardSystem = Card.GetCardSystem(PlasticCard.MainCardNumber);
            }
            return order;
        }

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (InfoDB.CommunicationTypeExistence(this.CustomerNumber) == 1 && this.CardReportReceivingType != null)
                {
                    //Դիմումը հնարավոր չէ ուղարկել, քանի որ հաղորդակցման եղանակը սխալ է ընտրված։ Անհրաժեշտ է մուտքագրել նոր դիմում։
                    result.Errors.Add(new ActionError(1732));
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
    }
}
