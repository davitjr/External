using ExternalBanking.DBManager;
using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class OrderGroup
    {
        /// <summary>
        /// Խմբի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Խմբի անվանում
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Խմբի կարգավիճակ (0` պասիվ, 1՝ ակտիվ)
        /// </summary>
        public OrderGroupStatus Status { get; set; }

        /// <summary>
        /// Խմբի տեսակ (1` հաճախորդի կողմից մուտքագրված, 2՝ ծրագրի կողմից ավտոմատ ստեղծված)
        /// </summary>
        public OrderGroupType Type { get; set; }

        /// <summary>
        /// Խումբը ստեղծող օգտագործողի id
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Խմբի ստեղծման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Խմբում ներառված ծառայությունների ցանկ
        /// </summary>
        public List<Template> GroupTemplates { get; set; }


        /// <summary>
        /// Պահպանում է գործարքների խումբը
        /// </summary>
        /// <returns></returns>
        public ActionResult Save()
        {
            Complete();
            this.Validate();
            ActionResult result = Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                Action action;
                if (this.ID > 0)
                {
                    action = Action.Update;
                }
                else
                {
                    action = Action.Add;
                }
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = OrderGroupDB.SubmitOrderGroup(this, action);

                    scope.Complete();
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        internal static bool CheckGroupId(int groupId)
        {
            return OrderGroupDB.CheckGroupId(groupId);
        }

        /// <summary>
        /// Խմբի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.Type == OrderGroupType.CreatedByCustomer && string.IsNullOrEmpty(this.GroupName))
            {
                //Խմբի անվանումը մուտքագրված չէ:
                result.Errors.Add(new ActionError(1555));
            }

            return result;
        }


        /// <summary>
        /// Լրացնում է խմբի ավտոմատ լրացման դաշտերը
        /// </summary>
        private void Complete()
        {
            //Ակտիվ
            this.Status = OrderGroupStatus.Active;
        }

        /// <summary>
        /// Խմբի հեռացում
        /// </summary>
        /// <returns></returns>
        public ActionResult Delete()
        {
            ActionResult result = ValidateForDelete();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                Action action = Action.Delete;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = OrderGroupDB.SubmitOrderGroup(this, action);

                    scope.Complete();
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Խմբի հեռացման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForDelete()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ծառայությունների խմբերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<OrderGroup> GetOrderGroups(ulong customerNumber, OrderGroupStatus status, OrderGroupType groupType)
        {
            List<OrderGroup> groups = OrderGroupDB.GetOrderGroups(customerNumber, status, groupType);

            return groups;
        }



    }
}
