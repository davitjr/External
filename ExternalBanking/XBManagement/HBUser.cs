using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.XBManagement.Interfaces;
using System;
using System.Collections.Generic;

namespace ExternalBanking.XBManagement
{
    public class HBUser : IEditableHBItem
    {
        /// <summary>
        /// Հերթական ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// ՀԲ պայմանգրի ID
        /// </summary>
        public int HBAppID { get; set; }
        /// <summary>
        /// Օգտագործողի հաճ.համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Օգտ.անուն
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// Հաճ. լրիվ Ա.Ա.
        /// </summary>
        public String UserFullName { get; set; }
        /// <summary>
        /// Հաճ. լրիվ Ա.Ա. անգլերեն
        /// </summary>
        public String UserFullNameEng { get; set; }
        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime? RegistrationDate { get; set; }
        /// <summary>
        /// Բլոկավորման կարգավիճակ (Ադմինի կողմից բլոկավորում (BLOCKED))
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Բլոկավորման կարգավիճակ (Սխալ նույնականացման հետևանքով բլոկավորում (LOCKED))
        /// </summary>
        public bool IsLocked { get; set; }
        /// <summary>
        /// Բլոկավորման ա/թ
        /// </summary>
        public DateTime? BlockingDate { get; set; }
        /// <summary>
        /// Բլոկավորող աշխատակցի ՊԿ
        /// </summary>
        public int BlockingSetID { get; set; }
        ///// <summary>
        ///// Հասանելիությունների սահմանափակում
        ///// </summary>
        public bool LimitedAccess { get; set; }
        /// <summary>
        /// Մուտքագրման իրավունք
        /// </summary>
        public bool AllowDataEntry { get; set; }
        /// <summary>
        /// Հաճախորդի էլ. հասցե
        /// </summary>
        public CustomerEmail Email { get; set; }
        /// <summary>
        /// Յուրաքանչյուր գործարքի հաստատում
        /// </summary>
        public bool IdentificationPerOrder { get; set; }
        /// <summary>
        /// Գաղտնաբառի փոփոխության անհարժեշտություն
        /// </summary>
        public bool PassChangeReq { get; set; }
        /// <summary>
        /// Հեռահար բանկինգի պրոդուկտների/հաշիվների հասանելիություն
        /// </summary>
        public List<HBProductPermission> ProductsPermissions { get; set; }
        /// <summary>
        /// Տեղեկություն կատարված մուտքերի վերաբերյալ
        /// </summary>
        public LogonInfo LogonInformation { get; set; }
        /// <summary>
        /// Գաղտնաբառ (mobile-ից գրանցման ժամանա, տվյալ դաշտով է հաճախորդը նշում իր ցանկալի գաղտնաբառը)
        /// </summary>
        public string Password { get; set; }

        public bool IsCas { get; set; }

        public HBUser()
        {
            this.Email = new CustomerEmail();
            this.Email.email = new Email();
            this.LogonInformation = new LogonInfo();
        }
        /// <summary>
        /// Վերադարձնում է պայմանագրի տվյալ կարգավիճակով ՀԲ օգտագործողներին
        /// </summary>
        /// <param name="HBAppID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<HBUser> GetHBUsers(int HBAppID, ProductQualityFilter filter)
        {
            List<HBUser> HBUsers = new List<HBUser>();
            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                HBUsers.AddRange(HBUserDB.GetHBUsers(HBAppID));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                HBUsers.AddRange(HBUserDB.GetClosedHBUsers(HBAppID));
            }
            if (filter == ProductQualityFilter.All)
            {
                HBUsers.AddRange(HBUserDB.GetHBUsers(HBAppID));
                HBUsers.AddRange(HBUserDB.GetClosedHBUsers(HBAppID));
            }
            foreach (HBUser user in HBUsers)
            {
                if (user.CustomerNumber != 0)
                    user.UserFullName = Utility.GetCustomerDescription(user.CustomerNumber);
                user.GetHBUserProductsPermissions();
            }
            return HBUsers;

        }
        /// <summary>
        /// Վերադարձնում է նշված օգտագործողին
        /// </summary>
        /// <param name="hbUserID"></param>
        /// <returns></returns>
        public static HBUser GetHBUser(int hbUserID)
        {
            HBUser hbUser = HBUserDB.GetHBUser(hbUserID);
            //hbUser.GetHBUserProductsPermissions();
            return hbUser;

        }

        /// <summary>
        /// Վերադարձնում է նշված օգտագործողին
        /// </summary>
        /// <param name="hbUserName"></param>
        /// <returns></returns>
        public static HBUser GetHBUserByUserName(string hbUserName)
        {
            HBUser hbUser = HBUserDB.GetHBUserByUserName(hbUserName);
            //hbUser.GetHBUserProductsPermissions();
            return hbUser;

        }
        /// <summary>
        /// Հասանելիություններ
        /// </summary>
        public void GetHBUserProductsPermissions()
        {

            HBUserDB.GetHBUserProductsPermissions(this);
        }

        public ActionResult Validate(ulong customerNumber)
        {

            ActionResult result = new ActionResult();
            ActionResult retResult = new ActionResult();

            result.Errors.AddRange(Validation.ValidateHBUserForSave(this, customerNumber));

            return result;
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի անվան հասանելիության ստուգում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public bool CheckHBUserNameAvailability(ulong customerNumber)
        {
            if (this.UserName == null)
                return false;
            return HBUserDB.CheckHBUserNameAvailability(this, customerNumber);
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի ավելացում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public ActionResult Add(int userID, SourceType source, long docId, ulong customerNumber)
        {
            ActionResult retResult = new ActionResult();
            ActionResult result = this.Validate(customerNumber);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            HBUserDB.Save(userID, source, this, docId, Action.Add);


            byte customerType = Customer.GetCustomerType(customerNumber);

            if (customerType == 6 && this.AllowDataEntry)
            {

                result = ApprovementSchema.CreateAutomaticApprovementSchema(this, customerNumber, docId, Action.Add);
                retResult.Errors.AddRange(result.Errors);
                if (result.ResultCode == ResultCode.Failed)
                {
                    retResult.ResultCode = ResultCode.Failed;
                }

            }

            if (retResult.Errors.Count == 0 && retResult.ResultCode != ResultCode.Failed)
            {
                retResult.ResultCode = ResultCode.Normal;
            }
            return retResult;

        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի խմբագրում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns ></returns>
        public ActionResult Update(int userID, SourceType source, long docId, ulong customerNumber = 0)
        {
            ActionResult retResult = new ActionResult();

            HBUserDB.Save(userID, source, this, docId, Action.Update);

            byte customerType = Customer.GetCustomerType(customerNumber);

            if (customerType == 6)
            {
                ActionResult resultXB = ApprovementSchema.CreateAutomaticApprovementSchema(this, customerNumber, docId, Action.Update);
                retResult.Errors.AddRange(resultXB.Errors);
                if (resultXB.ResultCode == ResultCode.Failed)
                {
                    retResult.ResultCode = ResultCode.Failed;
                }
            }

            if (retResult.Errors.Count == 0 && retResult.ResultCode != ResultCode.Failed)
            {
                retResult.ResultCode = ResultCode.Normal;
            }
            return retResult;
        }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի ապաակտիվացում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public ActionResult Deactivate(int userID, SourceType source, long docId, ulong customerNumber = 0)
        {
            ActionResult retResult = new ActionResult();

            HBUserDB.Save(userID, source, this, docId, Action.Deactivate);

            byte customerType = Customer.GetCustomerType(customerNumber);

            if (customerType == 6)
            {
                ActionResult resultXB = ApprovementSchema.CreateAutomaticApprovementSchema(this, customerNumber, docId, Action.Deactivate);
                retResult.Errors.AddRange(resultXB.Errors);
                if (resultXB.ResultCode == ResultCode.Failed)
                {
                    retResult.ResultCode = ResultCode.Failed;
                }
            }

            if (retResult.Errors.Count == 0 && retResult.ResultCode != ResultCode.Failed)
            {
                retResult.ResultCode = ResultCode.Normal;
            }
            return retResult;
        }

        /// <summary>
        /// ՀԲ օգտագործողի լոգ
        /// </summary>
        /// <returns></returns>
        public static List<HBUserLog> GetHBUserLog(String userName)
        {
            return HBUserDB.GetHBUserLog(userName);
        }

        public static ulong GetHBUserCustomerNumber(string hbUser)
        {
            return HBUserDB.GetHBUserCustomerNumber(hbUser);
        }

        /// <summary>
        /// ՀԲ օգտագործողի հասանելիություններ
        /// </summary>
        /// <param name="hbUserName"></param>
        /// <returns></returns>
        public static List<HBProductPermission> GetHBUserProductsPermissions(string hbUserName)
        {
            return HBUserDB.GetHBUserProductsPermissions(hbUserName);
        }
    }
}
