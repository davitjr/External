using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.XBManagement
{
    public class ApprovementSchema
    {
        /// <summary>
        /// Սխեմայի ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Սխեմայի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Սխեմայի քայլեր
        /// </summary>
        public List<ApprovementSchemaDetails> SchemaDetails { get; set; }

        /// <summary>
        /// True` եթե հաստատման սխեմայում տեղի է ունեցել փոփոխություն, False` հակառակ դեպքում
        /// </summary>
        public bool isModified { get; set; }

        /// <summary>
        /// Սխեմայի անվանման մաս հանդիսացող բառ
        /// </summary>
        const string SchemaNamePart = "սխեմա";

        /// <summary>
        /// Հաստատման սխեմայի պահպանում
        /// </summary>
        /// <param name="customerNumber"></param>
        public ActionResult Save(ulong customerNumber, Action action, long orderId)
        {
            ActionResult result = this.ValidateSchema(customerNumber);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = ApprovementSchemaDB.Save(this, customerNumber, action, orderId);
               
            return result;       
        }

        /// <summary>
        /// Հաստատման սխեմայի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateSchema(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateApprovementSchema(this, customerNumber));
            return result;
        }

        /// <summary>
        /// </summary>
        /// Վերադարձնում է հաճախորդի հաստատման սխեման
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static ApprovementSchema Get(ulong customerNumber)
        {
            ApprovementSchema schema;
           
                schema = ApprovementSchemaDB.GetApprovementSchema(customerNumber);
                schema.SchemaDetails = ApprovementSchemaDetails.Get(schema.Id);
            
            return schema;
        }

        public bool ExistsApprovementSchema(ulong customerNumber)
        {
            return ApprovementSchemaDB.ExistsApprovementSchema(customerNumber);
        }

        /// <summary>
        /// Օգտագործվում է ֆիզիկական անձանց դեպքում
        /// </summary>
        /// <param name="hbusr"></param>
        /// <param name="customerNumber"></param>
        /// <param name="orderId"></param>
        /// <param name="action"></param>
        /// <returns></returns>  
        public static ActionResult CreateAutomaticApprovementSchema(HBUser hbusr, ulong customerNumber,  long orderId, Action action)
        {
            ActionResult result = new ActionResult();
            HBUser oldHBUser = new HBUser();
            int hbApplicationId = hbusr.HBAppID;

            //Հաստատման հասանելիության խմբի ստեղծում
            //******************************************
            XBUserGroup group = new XBUserGroup();
            List<XBUserGroup> xbUserGroups = XBUserGroup.GetXBUserGroups(customerNumber);
            if (action == Action.Update)
            {
                oldHBUser = HBUser.GetHBUser(hbusr.ID);
            }

            //Եթե մուտքագրվում է նոր օգտագործող՝ Գործարքների մուտքագրման իրավունքով,
            //կամ խմբագրվում է Գործարքների մուտքագրման իրավունք չունեցող օգտագործող, որն արդեն պետք է ունենա Գործարքների մուտքագրման իրավունք
            if ((action == Action.Add && hbusr.AllowDataEntry == true) || (action == Action.Update && oldHBUser.AllowDataEntry == false && hbusr.AllowDataEntry == true) || (action == Action.Update && oldHBUser.IsBlocked == true && hbusr.IsBlocked == false && hbusr.AllowDataEntry == true))
            {
                
                if (xbUserGroups == null || (xbUserGroups != null && xbUserGroups.Count < 1))
                {

                    List<XBUserGroup> xbUserGroupsOfOrder = XBUserGroup.GetXBUserGroupsByOrder(orderId);

                    if (xbUserGroupsOfOrder == null || (xbUserGroupsOfOrder != null && xbUserGroupsOfOrder.Count < 1))
                    {
                        group.GroupName = "Խումբ 1";
                        group.Id = Convert.ToInt32(Utility.GetLastKeyNumber(75, 22000));

                        ActionResult resultXB = group.Save(customerNumber, orderId, hbApplicationId);
                        result.Errors.AddRange(resultXB.Errors);


                        //Հաստատման հասանելիության խմբում օգտագործողի ավելացում
                        HBUser hbUser = new HBUser();
                        hbUser = hbusr;
                        resultXB = group.AddHBUserIntoGroup(hbUser, orderId, hbApplicationId);
                        result.Errors.AddRange(resultXB.Errors);
                        group.HBUsers = new List<HBUser>();
                        group.HBUsers.Add(hbUser);

                        //Հաստատման սխեմայի քայլի ավելացում
                        ApprovementSchemaDetails approvementSchemaDetails = new ApprovementSchemaDetails();
                        approvementSchemaDetails.Group = group;
                        approvementSchemaDetails.Order = 1;
                        approvementSchemaDetails.Step = new ApprovementSchemaStep();
                        approvementSchemaDetails.Step.Description = "Քայլ 1";

                        ApprovementSchema s = ApprovementSchema.Get(customerNumber);

                        if (s.Id == 0)
                        {
                            ApprovementSchema schema = new ApprovementSchema();
                            schema.Description = schema.GenerateApprovementSchemaName(customerNumber);
                            ActionResult res = schema.Save(customerNumber, Action.Add, orderId);
                            result.Errors.AddRange(res.Errors);
                        }

                        if (result.Errors.Count < 1)
                        {
                            resultXB = approvementSchemaDetails.Save(hbusr.CustomerNumber, s.Id, orderId);
                            result.Errors.AddRange(resultXB.Errors);
                        }
                }

                else
                {
                    group = xbUserGroupsOfOrder[0];

                    //Հաստատման հասանելիության խմբում օգտագործողի ավելացում
                    HBUser hbUser = new HBUser();
                    hbUser = hbusr;
                    result = group.AddHBUserIntoGroup(hbUser, orderId, hbApplicationId);
                }

            }
                else
                {
                    group = xbUserGroups[0];

                    //Հաստատման հասանելիության խմբում օգտագործողի ավելացում
                    HBUser hbUser = new HBUser();
                    hbUser = hbusr;
                    result = group.AddHBUserIntoGroup(hbUser, orderId, hbApplicationId);
                }
            }

            //Եթե խմբագրվում է Գործարքների իրականացման իրավունք ունեցող օգտագործող, որն այլևս պետք է չունենա Գործարքների իրականացման իրավունք, կամ
            //ապաակտիվացվում է Գործարքների իրականացման իրավունք ունեցող օգտագործող
            else if ((action == Action.Update && oldHBUser.AllowDataEntry == true && hbusr.AllowDataEntry == false) || (action == Action.Deactivate && oldHBUser.AllowDataEntry == true))
            {                         
                if(xbUserGroups[0].HBUsers.Exists(x => x.ID == hbusr.ID) && xbUserGroups[0].HBUsers.Count == 1)
                {
                    ApprovementSchema approvementSchema = ApprovementSchema.Get(customerNumber);
                    result = approvementSchema.SchemaDetails[0].Remove(orderId);                  
                }
                else
                {
                    ActionResult res = xbUserGroups[0].RemoveHBUserFromGroup(hbusr, orderId);

                    if (res.ResultCode == ResultCode.Failed)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }
                    result.Errors.AddRange(res.Errors);
                }                                               
            }

            if(result.Errors.Count < 1 && result.ResultCode != ResultCode.Failed)
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
        /// Օգտագործվում է իրավաբանական անձանց դեպքում
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        /// <param name="hbApplicationId"></param>
        /// <returns></returns>
        public ActionResult CreateApprovementSchema(long orderId, ulong customerNumber, int hbApplicationId)
        {
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Normal;


            if(this.SchemaDetails != null && this.SchemaDetails.Count > 0)
            {
                    ApprovementSchema schema = new ApprovementSchema();
                    schema.Description = GenerateApprovementSchemaName(customerNumber);
                    result = schema.Save(customerNumber, Action.Add, orderId);

                    if (result.Errors.Count < 1)
                    {
                        foreach (ApprovementSchemaDetails approvementSchemaDetail in this.SchemaDetails)
                        {
                            if (approvementSchemaDetail.Group.HBUsers == null || (approvementSchemaDetail.Group.HBUsers != null && approvementSchemaDetail.Group.HBUsers.Count < 1))
                            {
                                //Նշված խմբում առկա չեն օգտագործողներ։
                                result.Errors.Add(new ActionError(960, new string[] { approvementSchemaDetail.Group.GroupName }));
                            }

                            else
                            {
                                ActionResult res = approvementSchemaDetail.Group.Save(customerNumber, orderId, hbApplicationId);
                                result.Errors.AddRange(res.Errors);

                                if (result.Errors.Count < 1)
                                {
                                    ActionResult res1 = approvementSchemaDetail.Save(customerNumber, this.Id, orderId);
                                    result.Errors.AddRange(res1.Errors);
                                }

                            }

                        }
                    }
            }

           

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;             
            }

            return result;
        }


        /// <summary>
        /// Օգտագործվում է իրավաբանական անձանց դեպքում
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        /// <param name="hbApplicationId"></param>
        /// <returns></returns>
        public ActionResult UpdateApprovementSchema(long orderId, ulong customerNumber, int hbApplicationId)
        {   
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            
            if (this.SchemaDetails != null && this.SchemaDetails.Count > 0)
            {
                    ApprovementSchema approvementSchemaOld = new ApprovementSchema();
                    approvementSchemaOld = ApprovementSchema.Get(customerNumber);

                    ActionResult res = new ActionResult();

                    foreach (ApprovementSchemaDetails approvementSchemaDetail in this.SchemaDetails)
                    {

                        // Քայլը նոր սխեմայում կա, իսկ հնում՝ ոչ
                        if (approvementSchemaOld.SchemaDetails == null || (approvementSchemaOld.SchemaDetails != null && !approvementSchemaOld.SchemaDetails.Exists(x => x.Id == approvementSchemaDetail.Id)))
                        {
                            if (approvementSchemaDetail.Group.HBUsers == null || (approvementSchemaDetail.Group.HBUsers != null && approvementSchemaDetail.Group.HBUsers.Count < 1))
                            {
                                //Նշված խմբում առկա չեն օգտագործողներ։
                                result.Errors.Add(new ActionError(960, new string[] { approvementSchemaDetail.Group.GroupName }));
                            }

                            else
                            {
                                //Նոր քայլի ավելացում

                                res = approvementSchemaDetail.Group.Save(customerNumber, orderId, hbApplicationId);
                                result.Errors.AddRange(res.Errors);
                                if (result.Errors.Count < 1)
                                {
                                    res = approvementSchemaDetail.Save(customerNumber, this.Id, orderId);
                                }

                                result.Errors.AddRange(res.Errors);

                                if (result.Errors.Count < 1)
                                {
                                    foreach (HBUser hbu in approvementSchemaDetail.Group.HBUsers)
                                    {
                                        List<HBToken> hbTokens = HBToken.GetHBTokens(hbu.ID, ProductQualityFilter.Opened);
                                        if (hbTokens != null && hbTokens.Count > 0)
                                        {
                                            this.isModified = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (result.Errors.Count < 1)
                        {
                            if (approvementSchemaDetail.Group.HBUsers == null || (approvementSchemaDetail.Group.HBUsers != null && approvementSchemaDetail.Group.HBUsers.Count < 1))
                            {
                                //Նշված խմբում առկա չեն օգտագործողներ։
                                result.Errors.Add(new ActionError(960, new string[] { approvementSchemaDetail.Group.GroupName }));
                            }

                            else
                            {
                                foreach (HBUser hbUser in approvementSchemaDetail.Group.HBUsers)
                                {
                                    foreach (ApprovementSchemaDetails asdOld in approvementSchemaOld.SchemaDetails)
                                    {
                                        if (asdOld.Group.Id == approvementSchemaDetail.Group.Id)
                                        {
                                            // Օգտագործողը նոր սխեմայում կա, իսկ հնում՝ ոչ
                                            if (asdOld.Group.HBUsers == null || !(asdOld.Group.HBUsers.Exists(x => x.ID == hbUser.ID)))
                                            {
                                                //Նոր օգտագործողի ավելացում
                                                res = approvementSchemaDetail.Group.AddHBUserIntoGroup(hbUser, orderId, hbApplicationId);
                                                result.Errors.AddRange(res.Errors);



                                                if (result.Errors.Count < 1)
                                                {

                                                    List<HBToken> hbTokens = HBToken.GetHBTokens(hbUser.ID, ProductQualityFilter.Opened);
                                                    if (hbTokens != null && hbTokens.Count > 0)
                                                    {
                                                        this.isModified = true;
                                                        break;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (ApprovementSchemaDetails asdOld in approvementSchemaOld.SchemaDetails)
                    {
                        // Քայլը հին սխեմայում կա, իսկ նորում՝ ոչ
                        if (this.SchemaDetails == null || (this.SchemaDetails != null && !this.SchemaDetails.Exists(x => x.Id == asdOld.Id)))
                        {
                            //Գոյություն ունեցող քայլի հեռացում
                            res = asdOld.Remove(orderId);
                            result.Errors.AddRange(res.Errors);

                            if (result.Errors.Count < 1)
                            {
                                this.isModified = true;
                            }
                        }

                        foreach (HBUser hbUser in asdOld.Group.HBUsers)
                        {
                            foreach (ApprovementSchemaDetails asdNew in this.SchemaDetails)
                            {
                                if (asdOld.Group.Id == asdNew.Group.Id)
                                {
                                    // Օգտագործողը հին սխեմայում կա, իսկ նորում՝ ոչ
                                    if (asdNew.Group.HBUsers == null || !(asdNew.Group.HBUsers.Exists(x => x.ID == hbUser.ID)))
                                    {
                                        //Գոյություն ունեցող օգտագործողի հեռացում
                                        res = asdNew.Group.RemoveHBUserFromGroup(hbUser, orderId);
                                        result.Errors.AddRange(res.Errors);

                                        if (result.Errors.Count < 1)
                                        {
                                            this.isModified = true;
                                        }
                                    }
                                }

                            }
                        }
                    }
            }
            
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
                   
        }

        /// <summary>
        /// Գեներացնում է հաճախորդին համապատասխան սխեմայի անվանում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public string GenerateApprovementSchemaName(ulong customerNumber)
        {
            string schemaName = "";
            ACBAServiceReference.Customer customer = Customer.GetCustomer(customerNumber);
            if (customer.customerType.key == 6)
            {
                PhysicalCustomer physicalCustomer = (PhysicalCustomer)customer;
                Person person = physicalCustomer.person;

                string FirstName = Utility.ConvertAnsiToUnicode(person.fullName.firstName);
                string LastName = Utility.ConvertAnsiToUnicode(person.fullName.lastName);

                schemaName = FirstName + " " + LastName + " " + SchemaNamePart;
            }
            else
            {
                LegalCustomer legalCustomer = (LegalCustomer)customer;
                Organisation organisation = legalCustomer.Organisation;
                schemaName = Utility.ConvertAnsiToUnicode(organisation.Description + " " + SchemaNamePart);
            }

            return schemaName;
        }
    }
}
