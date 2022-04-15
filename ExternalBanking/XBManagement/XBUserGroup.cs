using ExternalBanking.DBManager;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking.XBManagement
{
    public class XBUserGroup
    {
        /// <summary>
        /// Խմբի ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Խմբի անվանում
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Խմբում ներառված օգտագործողներ
        /// </summary>
        public List<HBUser> HBUsers { get; set; }

        /// <summary>
        /// Խմբի պահպանում
        /// </summary>
        public ActionResult Save(ulong customerNumber, long orderId, int appId)
        {
            ActionResult result = this.ValidateXBUserGroup(customerNumber, appId);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = XBUserGroupDB.Save(this, orderId, Action.Add);
            if (result.ResultCode == ResultCode.Normal && this.HBUsers != null && this.HBUsers.Count > 0)
            {
                foreach (HBUser hbUser in this.HBUsers)
                {
                    ActionResult resultXBUser = AddHBUserIntoGroup(hbUser, orderId, appId);
                    if (resultXBUser.ResultCode != ResultCode.Normal)
                    {
                        if (resultXBUser.ResultCode == ResultCode.Failed)
                        {
                            result.ResultCode = resultXBUser.ResultCode;
                            break;
                        }
                        result.Errors.AddRange(resultXBUser.Errors);
                        result.ResultCode = resultXBUser.ResultCode;
                    }
                }
            }
            if (result.Errors.Count < 1)
            {
                result.ResultCode = ResultCode.Normal;
            }


            return result;
        }

        /// <summary>
        /// Խմբում օգտագործողի ավելացում
        /// </summary>
        /// <param name="user"></param>
        public ActionResult AddHBUserIntoGroup(HBUser user, long orderId, int appId)
        {
            ActionResult result = ValidateHBUser(user, appId);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = XBUserGroupDB.AddHBUserIntoGroup(this, user, Action.Add, orderId);

            return result;
        }

        /// <summary>
        /// Խմբի հեռացում
        /// </summary>
        public ActionResult RemoveGroup(long doc_id)
        {
            ActionResult result = this.ValidateXBUserGroupForRemove();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = XBUserGroupDB.RemoveGroup(this, doc_id);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք խումբը պատկանում է որևէ սխեմայի, թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool BelongsToSchema()
        {
            return XBUserGroupDB.BelongsToSchema(this);
        }


        /// <summary>
        /// Խմբի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateXBUserGroup(ulong customerNumber, int appId)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateXBUserGroup(this, customerNumber, appId));
            return result;
        }


        /// <summary>
        /// Խմբի հեռացման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateXBUserGroupForRemove()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateXBUserGroupForRemove(this));
            return result;
        }

        /// <summary>
        /// Հասանելիության խմբից օգտագործողի հեռացում
        /// </summary>
        /// <param name="user"></param>
        /// <param name="acbaUser"></param>
        /// <returns></returns>
        public ActionResult RemoveHBUserFromGroup(HBUser user, long orderId)
        {
            ActionResult result = this.ValidateHBUserForRemove(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = XBUserGroupDB.RemoveHBUserFromGroup(this, user, orderId);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Օգտագործողի` հասանելիության խմբից հեռացման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateHBUserForRemove(HBUser user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBUserForRemove(this, user));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ օգտագործողը պատկանում է նշված խմբին, թե ոչ
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool BelongsUserToGroup(HBUser user)
        {
            return XBUserGroupDB.BelongsUserToGroup(this, user);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հասանելիության խմբերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<XBUserGroup> GetXBUserGroups(ulong customerNumber)
        {
            List<XBUserGroup> groups = new List<XBUserGroup>();

            groups = XBUserGroupDB.GetXBUserGroups(customerNumber);
            foreach (XBUserGroup group in groups)
            {
                group.HBUsers = GetHBUsersByGroup(group.Id);
            }

            return groups;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հասանելիության խմբերը
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static List<XBUserGroup> GetXBUserGroups(string userName)
        {
            List<XBUserGroup> groups = new List<XBUserGroup>();
            groups = XBUserGroupDB.GetXBUserGroups(userName);
            return groups;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ խմբում առկա օգտագործողների ցանկը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<HBUser> GetHBUsersByGroup(int id)
        {
            List<HBUser> hbUsers = new List<HBUser>();

            hbUsers = XBUserGroupDB.GetHBUsersByGroup(id);

            if (hbUsers != null)
            {
                for (int i = 0; i < hbUsers.Count; i++)
                {
                    hbUsers[i] = HBUser.GetHBUser(hbUsers[i].ID);
                }
            }

            return hbUsers;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի` տրված հասանելիության խումբը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static XBUserGroup Get(int id)
        {
            XBUserGroup group = new XBUserGroup();

            group = XBUserGroupDB.GetXBUserGroup(id);

            group.HBUsers = GetHBUsersByGroup(id);

            return group;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք գոյություն ունի նշված անունով խումբ, թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool ExistsXBUserGroupWithName(ulong customerNumber)
        {
            HBApplication hbApplication = HBApplication.GetHBApplication(customerNumber);

            if (hbApplication == null)
            {
                return false;
            }
            else
            {
                int HBApplicationId = HBApplication.GetHBApplication(customerNumber).ID;
                return XBUserGroupDB.ExistsXBUserGroupWithName(HBApplicationId, this.GroupName);
            }

        }


        /// <summary>
        /// Ստեղծում է հերթական խմբի անվանում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static string GenerateNextGroupName(ulong customerNumber)
        {
            return XBUserGroupDB.GenerateNextGroupName(customerNumber);
        }

        /// <summary>
        /// Օգտագործողի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public static ActionResult ValidateHBUser(HBUser user, int appId)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBUser(user, appId));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հասանելիության խմբերը (որոնք ավելացվել են HBBase-ում` հայտի հաստատվելուց հետո HB_Logins-ում գրանցվելու համար)
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<XBUserGroup> GetXBUserGroupsByOrder(long docId)
        {
            List<XBUserGroup> groups = new List<XBUserGroup>();

            groups = XBUserGroupDB.GetXBUserGroupsByOrder(docId);
            foreach (XBUserGroup group in groups)
            {
                group.HBUsers = GetHBUsersByGroupByOrder(group.Id, docId);
            }

            return groups;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ խմբում առկա օգտագործողների ցանկը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<HBUser> GetHBUsersByGroupByOrder(int groupId, long docId)
        {
            List<HBUser> hbUsers = new List<HBUser>();

            hbUsers = XBUserGroupDB.GetHBUsersByGroupByOrder(groupId, docId);

            if (hbUsers != null)
            {
                for (int i = 0; i < hbUsers.Count; i++)
                {
                    hbUsers[i] = HBUser.GetHBUser(hbUsers[i].ID);
                }
            }


            return hbUsers;
        }
    }
}
