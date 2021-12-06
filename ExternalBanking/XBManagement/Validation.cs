using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.XBManagement.Interfaces;
using ExternalBanking.ACBAServiceReference;
using System.Text.RegularExpressions;
using ExternalBanking.ServiceClient;
using System.Configuration;
using ExternalBanking.DBManager;
using System.Web.Configuration;

namespace ExternalBanking.XBManagement
{
    class Validation
    {
        public static List<ActionError> ValidateXBUserGroupForRemove(XBUserGroup group)
        {
            List<ActionError> result = new List<ActionError>();

            if (group == null)
            {
                //Խումբը բացակայում է
                result.Add(new ActionError(914));
            }

            if (group.BelongsToSchema())
            {
                //Տվյալ խումբը ներառված է հաստատման սխեմայի մեջ:
                result.Add(new ActionError(915));
            }

            return result;
        }

        public static List<ActionError> ValidateXBUserGroup(XBUserGroup group, ulong customerNumber, int appId)
        {
            List<ActionError> result = new List<ActionError>();

            if (group == null)
            {
                //Խումբը բացակայում է
                result.Add(new ActionError(914));
            }

            else if (String.IsNullOrEmpty(group.GroupName))
            {
                //Մուտքագրեք խմբի անվանումը:
                result.Add(new ActionError(916));
            }

            else if (group.ExistsXBUserGroupWithName(customerNumber))
            {
                //Նշված անունով խումբ արդեն գոյություն ունի:
                result.Add(new ActionError(958));
            }

            else if(group.HBUsers != null)
            {
                foreach(HBUser hbUser in group.HBUsers)
                {
                    if(hbUser.HBAppID != appId)
                    {
                        result.Add(new ActionError(1039, new string[] { hbUser.UserName }));
                    }
                }              
            }

            return result;
        }

        public static List<ActionError> ValidateHBUser(HBUser user, int appId)
        {
            List<ActionError> result = new List<ActionError>();

            if (user == null)
            {
                //Օգտագործողը բացակայում է
                result.Add(new ActionError(917));
            }
            else if (user.HBAppID != appId)
            {
                result.Add(new ActionError(1039, new string[] { user.UserName }));
            }

            return result;
        }

        public static List<ActionError> ValidateHBToken(HBToken token)
        {
            List<ActionError> result = new List<ActionError>();

            if (token.TransLimit > token.DayLimit)
            { 
                result.Add(new ActionError(1036)); //Գործարքի սահմանաչափը չի կարող մեծ լինել օրական սահմանաչափից
            }

            if (!token.CheckTokenQuantityAvailability())
            {
                result.Add(new ActionError(1711));//Հարգելի հաճախորդ, տոկենների քանակը չի կարող գերազանցել սահմանված առավելագույն քանակից:
            }

            if (!token.CheckTokenNumberAvailability())
            {
               result.Add(new ActionError(1113, new string[] { token.TokenNumber }));//Տվյալ տոկենի համարը զբաղված է, ընտրեք այլ համար:
            }

            return result;
        }
        public static List<ActionError> ValidateHBTokenUpdate(HBToken token)
        {
            List<ActionError> result = new List<ActionError>();

            if (token.TransLimit > token.DayLimit)
            {
                result.Add(new ActionError(1036)); //Գործարքի սահմանաչափը չի կարող մեծ լինել օրական սահմանաչափից
            }
            
            return result;
        }
        
        public static List<ActionError> ValidateHBUserForSave(HBUser user,ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();

            if (!user.CheckHBUserNameAvailability(customerNumber))
                result.Add(new ActionError(1009)); //Օգտագործողի անունը կրկնվում է 

            return result;
        }
            


        public static List<ActionError> ValidateApprovementSchema(ApprovementSchema schema, ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();

            if (schema == null)
            {
                //Սխեման բացակայում է
                result.Add(new ActionError(918));
            }
            else
            {
                if (schema.ExistsApprovementSchema(customerNumber))
                {
                    //Տվյալ հաճախորդի համար արդեն գոյություն ունի հաստատման սխեմա
                    result.Add(new ActionError(956));
                }
            }
            return result;
        }

        public static List<ActionError> ValidateApprovementSchemaDetails(ApprovementSchemaDetails schemaDetails, int schemaId, ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();
            //bool isOldGroup = false;
            bool isOldStepOrder = false;

            if(schemaDetails.Id > 0)
            {
                ApprovementSchemaDetails oldSchemaDetails = ApprovementSchemaDetails.GetApprovementSchemaDetailsById(schemaDetails.Id);
                //if(oldSchemaDetails.Group.Id == schemaDetails.Group.Id)
                //{
                //    isOldGroup = true;
                //}
                if(oldSchemaDetails.Order == schemaDetails.Order)
                {
                    isOldStepOrder = true;
                }
            }


            //else if (!isOldGroup && schemaDetails.Group.BelongsToSchema())
            //{
            //    //Տվյալ խումբը ներառված է հաստատման սխեմայի մեջ:
            //    result.Add(new ActionError(915));
            //}

            else if(schemaDetails.Group.HBUsers == null || schemaDetails.Group.HBUsers.Count < 1)
            {
                //Նշված խմբում առկա չեն օգտագործողներ։
                result.Add(new ActionError(960));
            }

            if(schemaId > 0)
            {
                ApprovementSchema schema = ApprovementSchema.Get(customerNumber);
                foreach(ApprovementSchemaDetails schemaDetail in schema.SchemaDetails)
                {
                    if (!isOldStepOrder && (schemaDetail.Order == schemaDetails.Order))
                    {
                        //Նշված կարգահամարով քայլ արդեն գոյություն ունի
                        result.Add(new ActionError(961));
                        break;
                    }
                }              
            }
          

            return result;
        }

        public static List<ActionError> ValidateHBUserForRemove(XBUserGroup group, HBUser user)
        {
            List<ActionError> result = new List<ActionError>();

            if (user == null)
            {
                //Օգտագործողը բացակայում է
                result.Add(new ActionError(917));
            }
            //else if(!group.BelongsUserToGroup(user))
            //{
            //    //Օգտագործողը չի պատկանում նշված խմբին
            //    result.Add(new ActionError(924, new string[] { user.UserName }));
            //}

            //else if(group.HBUsers.Count <= 1)
            //{
            //    if(group.BelongsToSchema())
            //    {
            //        //Նշված խմբում օգտագործողների ցանկը չի կարող լինել դատարկ, քանի որ այն ներառված է հաստատման սխեմայի մեջ։
            //        result.Add(new ActionError(959));
            //    }
            //}

            return result;
        }

        public static List<ActionError> ValidateApprovementSchemaDetailsForRemove(ApprovementSchemaDetails schemaDetails)
        {
            List<ActionError> result = new List<ActionError>();

            if (schemaDetails == null)
            {
                //Սխեմայի մանրամասները բացակայում են
                result.Add(new ActionError(962));
            }
       
            return result;
        }

        public static List<ActionError> ValidatePhoneBankingContractOrder(PhoneBankingContractOrder order)
        {
            List<ActionError> result = new List<ActionError>();

           
            short customerType = Customer.GetCustomerType(order.CustomerNumber);


            if (customerType != (short)CustomerTypes.physical)
            {
                //Հեռախոսային բանկինգի պայմանագիր կարող եք մուտքագրել միայն ֆիզիկական անձ հաճախորդների համար:
                result.Add(new ActionError(1031));
                return result;
            }

            if (order.QuestionAnswers == null || (order.QuestionAnswers != null && order.QuestionAnswers.Count < 4))
            {
                //Հարցերի քանակը պետք է լինի առնվազն 4
                result.Add(new ActionError(1022));
            }

            if(order.DayLimitToOwnAccount <= 0 || order.DayLimitToAnothersAccount <= 0)
            {
                //Գործարքի օրական սահմանաչափը մուտքագրված չէ:
                result.Add(new ActionError(1023));
            }

            if (order.OneTransactionLimitToOwnAccount <= 0)
            {
                //Մեկ գործարքի սահմանաչափը (փոխանցում սեփական հաշիվներ միջև) մուտքագրված չէ:
                result.Add(new ActionError(1024));
            }

            if (order.OneTransactionLimitToAnothersAccount <= 0)
            {
                //Մեկ գործարքի սահմանաչափը (փոխանցում այլ հաճախորդի հաշվին) մուտքագրված չէ:
                result.Add(new ActionError(1025));
            }

            if(order.EmailId <= 0)
            {
                //Էլ.հասցեն մուտքագրված չէ:
                result.Add(new ActionError(1026));
            }

            if (order.PhoneId <= 0)
            {
                //Հեռախոսահամարը մուտքագրված չէ:
                result.Add(new ActionError(1027));
            }

            if(order.QuestionAnswers != null)
            {
                //i համարի հարցի պատասխանը մուտքագրված չէ:
                for (int i = 0; i < order.QuestionAnswers.Count; i++)
                {
                    if (String.IsNullOrEmpty(order.QuestionAnswers[i].Answer))
                    {
                        result.Add(new ActionError(1028, new string[] { (i + 1).ToString() }));
                    }
                }
            }

            if(order.DayLimitToOwnAccount > 10000000)
            {
                //Գործարքի օրական սահմանաչափը չի կարող գերազանցել սահմանված 10,000,000  ՀՀ դրամ օրական սահմանաչափը:
                result.Add(new ActionError(1030, new string[] { "10,000,000" }));
            }         

            if (order.DayLimitToAnothersAccount > 1500000)
            {
                //Գործարքի օրական սահմանաչափը չի կարող գերազանցել սահմանված 1,500,000  ՀՀ դրամ օրական սահմանաչափը:
                result.Add(new ActionError(1030, new string[] { "1,500,000" }));
            }

            if(order.OneTransactionLimitToOwnAccount > order.DayLimitToOwnAccount || order.OneTransactionLimitToAnothersAccount > order.DayLimitToAnothersAccount)
            {
                //Օրական սահմանաչափի արժեքը պետք է լինի մեծ կամ հավասար Գործարքի համապատասխան սահմանաչափին
                result.Add(new ActionError(1032));
            }

        
            return result;
        }

        /// <summary>
        /// Հեռահար բանկինգի հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateHBApplication(HBApplicationOrder order)
        {
            var customerType = ACBAOperationService.GetCustomerType(order.CustomerNumber);

            List<ActionError> result = new List<ActionError>();

            if (order.Source != SourceType.MobileBanking && HBApplicationDB.GetHBApplication(order.CustomerNumber)?.PermissionType == 2)
            {
                //Ուշադրություն! Տվյալ փոփոխությունը հնարավոր չէ իրականացնել սահմանափակ հասանելիությամբ հեռահար բանկիգ ծառայության համար։
                result.Add(new ActionError(1803));
            }

            if (order.user.filialCode != order.FilialCode)
            {
                result.Add(new ActionError(1058));//Տվյալ գործողությունը հնարավոր է կատարել միայն հաճախորդի հիմնական մասնաճյուղում
                return result;
            }

            //if (HBApplicationOrder.isExistsNotConfirmedHBOrder(order.CustomerNumber))
            //    result.Add(new ActionError(1041)); //Տվյալ հաճախորդի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված հեռահար բանկինգի մուտքագրման/փոփոխության կամ ակտիվացման հայտ կամ չակտիվացված ծառայություն:         
            
           if (order.Type == OrderType.HBApplicationOrder && HBApplicationOrder.IsExistsHB(order.CustomerNumber))
                result.Add(new ActionError(1042)); //Տվյալ հաճախորդը արդեն ունի հեռահար բանկինգի ծառայություն:

            //Օգտագործողները
            List<HBUser> hbuserlist_add = order.HBApplicationUpdate.AddedItems.FindAll(x => x.GetType().Name == "HBUser").Cast<HBUser>().ToList();
            List<HBUser> hbuserlist_upd = order.HBApplicationUpdate.UpdatedItems.FindAll(x => x.GetType().Name == "HBUser").Cast<HBUser>().ToList();
            List<HBUser> hbuserlist_old = HBUser.GetHBUsers(order.HBApplication.ID, ProductQualityFilter.Opened);
            hbuserlist_old = hbuserlist_old.Where(old => !hbuserlist_upd.Any(upd => upd.ID == old.ID)).ToList();

            List<HBUser> hbuserlist_all = new List<HBUser>(hbuserlist_add.Count + hbuserlist_upd.Count + hbuserlist_old.Count);
            hbuserlist_all.AddRange(hbuserlist_add);
            hbuserlist_all.AddRange(hbuserlist_upd);
            hbuserlist_all.AddRange(hbuserlist_old);

            //Տոկենները
            List<HBToken> hbtokenlist_add = order.HBApplicationUpdate.AddedItems.FindAll(x => x.GetType().Name == "HBToken").Cast<HBToken>().ToList();
            List<HBToken> hbtokenlist_upd = order.HBApplicationUpdate.UpdatedItems.FindAll(x => x.GetType().Name == "HBToken").Cast<HBToken>().ToList();
            List<HBToken> hbtokenlist_old = HBToken.GetHBTokens(order.HBApplication.ID);
            hbtokenlist_old = hbtokenlist_old.Where(old => !hbtokenlist_upd.Any(upd => upd.ID == old.ID)).ToList();

            List<HBToken> hbtokenlist_all = new List<HBToken>(hbtokenlist_add.Count + hbtokenlist_upd.Count + hbtokenlist_old.Count);
            hbtokenlist_all.AddRange(hbtokenlist_add);
            hbtokenlist_all.AddRange(hbtokenlist_upd);
            hbtokenlist_all.AddRange(hbtokenlist_old);

            var list = from hbuser in hbuserlist_add
                       join hbtoken in hbtokenlist_add
                       on hbuser.ID equals hbtoken.HBUser.ID                                                   
                       select new { hbuser };

            List<HBUser> listofhbnewuserswithnewtokens = list.Select(o => o.hbuser).Distinct().ToList();
            
            if (listofhbnewuserswithnewtokens.Count < hbuserlist_add.Count()) 
                result.Add(new ActionError(1045));//Գոյություն ունի մուտքագրված օգտագործող առանց տոկենի:Կցեք տոկեն կամ հեռացրեք տվյալ օգտագործողին:

            
        var listT = from token in hbtokenlist_all group token.TokenNumber by token.TokenNumber into g where g.Count()>1 select new { g.Key };
            
            if (listT.ToList().Count > 0) 
                result.Add(new ActionError(1115));//Հայտում հայտնաբերվել է կրկնվող համարով տոկեն:


            if (customerType == (short)CustomerTypes.physical)
            {
                var list2 = from hbuser in hbuserlist_all
                            join hbtoken in hbtokenlist_all
                            on hbuser.ID equals hbtoken.HBUser.ID
                            where hbuser.AllowDataEntry == true && (hbtoken.DayLimit == 0 || hbtoken.TransLimit == 0) && hbtoken.Quality!=HBTokenQuality.Deactivated
                            select new { hbtoken };

            List<HBToken> listofuserswithwronglimits = list2.Select(o => o.hbtoken).Distinct().ToList();
                if(listofuserswithwronglimits.Count>0)
                    result.Add(new ActionError(1096));//Տոկենի սահմանաչափերը սխալ են

                var list3 = from hbuser in hbuserlist_all
                            join hbtoken in hbtokenlist_all
                            on hbuser.ID equals hbtoken.HBUser.ID
                            where hbuser.AllowDataEntry == false && (hbtoken.DayLimit != 0 || hbtoken.TransLimit != 0) && hbtoken.Quality != HBTokenQuality.Deactivated
                            select new { hbtoken };

                List<HBToken> listofuserswithwronglimits2 = list3.Select(o => o.hbtoken).Distinct().ToList();
                if (listofuserswithwronglimits2.Count > 0)
                    result.Add(new ActionError(1096));//Տոկենի սահմանաչափերը սխալ են
            }

            //Sevak stugum gorcogh ugharkeluc hanel isTestVersion yev Terme pokhel 1904
            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
            if (isTestVersion && CheckCustomerPhoneNumber(order.CustomerNumber))
            {
                //Հաճախորդի համար չկա մուտքագրված հեռախոսահամար:
                result.Add(new ActionError(1918));
            }

            return result;
        }
        internal static List<ActionError> ValidateHBActivationOrder(HBActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.HasHBActivationOrder() == true)
            {
                //Գոյություն ունի ՀԲ ի սպասարկման հայտ:
                result.Add(new ActionError(699));
            }
            

            else //if (order.HBActivationRequest.RequestType == 1)
            {

                foreach (HBActivationRequest request in order.HBActivationRequests)
                {
                    if (request.RequestType == 1)
                    {
                        Customer cust = new Customer();
                        cust.CustomerNumber = order.CustomerNumber;
                        if (cust.HasACBAOnline() == HasHB.Yes)
                        {
                            //Ծառայությունը  արդեն ակտիվացված է:
                            result.Add(new ActionError(698));
                        }
                    }
                    if (request.RequestType == 1 || request.RequestType == 2)
                    {
                        if (request.HBToken == null)
                        {
                            //Լրացրեք տոկենի համարը:
                            result.Add(new ActionError(722));
                        }

                        if (string.IsNullOrEmpty(request.HBToken.TokenNumber))
                        {
                            //Լրացրեք տոկենի համարը:
                            result.Add(new ActionError(722));
                        }
                       
                    }
                }
            }

            if (result.Count > 0)
            {
                return result;
            }
            if (order.Amount > 0)
            {
                result.AddRange(ExternalBanking.Validation.ValidateDebitAccount(order, order.DebitAccount));

                if (order.DebitAccount != null || order.DebitAccount.AccountNumber != "0")
                {
                    if (order.DebitAccount.Currency != "AMD")
                    {
                        //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                        result.Add(new ActionError(669));
                    }
                }
            }

            return result;
        }

        internal static List<ActionError> SetAmountsForCheckBalance(Order order)
        {
            return ExternalBanking.Validation.SetAmountsForCheckBalance(order);
        }

        public static List<ActionError> ValidateHBServletRequestOrder(HBServletOrder ServletOrder)
        {
            List<ActionError> result = new List<ActionError>();
            if(ServletOrder.Source != SourceType.MobileBanking && HBApplicationDB.GetHBApplication(ServletOrder.CustomerNumber)?.PermissionType == 2)
            {
                //Ուշադրություն! Տվյալ փոփոխությունը հնարավոր չէ իրականացնել սահմանափակ հասանելիությամբ հեռահար բանկիգ ծառայության համար։
                result.Add(new ActionError(1803));
            }

            if (ServletOrder.ServletAction == HBServletAction.ActivateToken)
            {
                 if(string.IsNullOrEmpty(ServletOrder.HBtoken.HBUser.Email.email.emailAddress))
                    //Էլեկտրոնային հասցեն մուտքագրված չէ:
                    result.Add(new ActionError(446));
            }
            if (ServletOrder.ServletAction == HBServletAction.ShowPINCode)
            {
                if (ServletOrder.HBtoken.TokenNumber == null)
                    //Տոկենը նշված չէ:
                    result.Add(new ActionError(1238));
                else if (ServletOrder.HBtoken.TokenNumber == String.Empty)
                    //Տոկենը նշված չէ:
                    result.Add(new ActionError(1238));

            }
            
            if ((ServletOrder.GroupId != 0) ? !OrderGroup.CheckGroupId(ServletOrder.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }

        public static List<ActionError> ValidateHBRegistrationCodeResendOrder(HBRegistrationCodeResendOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.Token.TokenType != HBTokenTypes.MobileBanking && order.Token.TokenType != HBTokenTypes.MobileToken)
            {
                //Գրանցման կոդը հնարավոր է վերաուղարկել միայն մոբայլ տոկենի կամ մոբայլ բանկինգի համար:
                result.Add(new ActionError(1235));
            }
            if (order.Token.Quality != HBTokenQuality.Active)
            {
                //Գրանցման կոդի վերաուղարկում հնարավոր է կատարել միայն գործող կարգավիճակով տոկենների համար:
                result.Add(new ActionError(1236));
            }
            if (order.Token.HBUser.Email.email.id == 0)
                //Էլեկտրոնային հասցեն մուտքագրված չէ:
                result.Add(new ActionError(446));

            return result;
        }
        internal static List<ActionError> ValidateHBActivationRejectionOrder(HBActivationRejectionOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (Customer.GetCustomerType(order.CustomerNumber) != 6 && order.FilialCode != Customer.GetCustomerFilial(order.CustomerNumber).key)
            {
                //Հեռահար բանկինգի ծառայություններից հրաժարում հնարավոր է կատարել միայն հաճախորդի մասնաճյուղում
                result.Add(new ActionError(1272));
            }

            if (result.Count > 0)
            {
                return result;
            }
         
            return result;

        }

        internal static List<ActionError> ValidatePhoneBankingContractActivationOrder(PhoneBankingContractActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Customer C = new Customer();
            C.CustomerNumber = order.CustomerNumber;
            HasHB hb = C.HasPhoneBanking();

            if (hb == HasHB.Yes)
                result.Add(new ActionError(1278)); //Տվյալ հաճախորդն արդեն ունի հեռախոսային բանկինգի ծառայություն
            
             var customer = ACBAOperationService.GetCustomer(order.CustomerNumber);

            if (order.FilialCode != customer.filial.key)
            {
                //ՀԲ ի սպասարկման վարձի գանձումը կատարվում է միայն հաճախորդի մասնաճյուղում:
                result.Add(new ActionError(697));
            }

            if (PhoneBankingContract.isExistsNotConfirmedPBOrder(order.CustomerNumber))
                result.Add(new ActionError(1277)); //Տվյալ հաճախորդի համար արդեն գոյություն ունի դեռրևս չակտիվացված հեռախոսային բանկինգի ծառայության հայտ

            if (result.Count > 0)
            {
                return result;
            }

            if (order.Amount > 0)
            {
                result.AddRange(ExternalBanking.Validation.ValidateDebitAccount(order, order.DebitAccount));

                if (order.DebitAccount != null || order.DebitAccount.AccountNumber != "0")
                {
                    if (order.DebitAccount.Currency != "AMD")
                    {
                        //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                        result.Add(new ActionError(669));
                    }
                }

            }

            return result;
        }
        /// <summary>
        /// Վարկի ամսաթվի փոփոխման  հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateLoanDelayOrder(LoanDelayOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Loan loan = new Loan();

            loan = Loan.GetLoan(Convert.ToUInt64(order.ProductAppId), order.CustomerNumber);

            if (loan.LoanType == 7 || loan.LoanType == 13 || loan.LoanType == 15 && loan.Fond == 44)
            {
                result.Add(new ActionError(1740));
            }

            if (loan.Fond == 22 || loan.Fond == 54 || loan?.SubsidiaInterestRate > 0 && loan.NextRepayment.RepaymentDate == loan.EndDate)
            {
                result.Add(new ActionError(1740));
            }

            if (loan.Fond == 55)
            {
                result.Add(new ActionError(1740));
            }

            return result;
        }
        /// <summary>
        /// Հետաձգված հայտի չեղարկման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateLoanCancelDelayOrder(CancelDelayOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.DelayReason == "Սխալ դիմում")
            {
                if (!LoanDelayOrderDB.CheckValidLoanDelayCancelOrder(order.ProductAppId))
                    result.Add(new ActionError(1743));
            }

            return result;
        }
        public static List<ActionError> ValidateHBApplicationQualityChangeOrder(HBApplicationQualityChangeOrder Order)
        {
            List<ActionError> result = new List<ActionError>();
            if (Order.Source != SourceType.MobileBanking && HBApplicationDB.GetHBApplication(Order.CustomerNumber)?.PermissionType == 2)
            {
                //Ուշադրություն! Տվյալ փոփոխությունը հնարավոր չէ իրականացնել սահմանափակ հասանելիությամբ հեռահար բանկիգ ծառայության համար։
                result.Add(new ActionError(1803));
            }

            return result;
        }
        /// <summary>
        /// ՀԲ պայմանագրի ամբողջական հասանելիությունների տրամադրման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateHBApplicationFullPermissionsGrantingOrder(HBApplicationFullPermissionsGrantingOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            short customerQuality = ACBAOperationService.GetCustomerQuality(order.CustomerNumber);

            if (customerQuality != 1)
            {
                //Լիարժեք հասանելիություններ ստանալու համար Օգտագործողը  պետք է ունենա «Հաճախորդ»  կարգավիճակ
                result.Add(new ActionError(1815));

            }

            return result;
        }

        public static bool CheckCustomerPhoneNumber(ulong customerNumber) => Info.GetCustomerAllPhones(customerNumber).Rows.Count > 0 ? false : true;

    }
}
