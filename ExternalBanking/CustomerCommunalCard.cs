using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Հաճախորդի կոմունալի քարդ
    /// </summary>
    public class CustomerCommunalCard
    {
        /// <summary>
        /// Կոմունալի քարդի ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Կոմունալի տեսակ
        /// </summary>
        public ushort CommunalType { get; set; }

        /// <summary>
        /// Աբոնոնետի տեսակ
        /// </summary>
        public AbonentTypes AbonentType { get; set; }

        /// <summary>
        /// Բացման ա/թ
        /// </summary>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Բացողի ՊԿ
        /// </summary>
        public ushort OpenerSetNumber { get; set; }

        /// <summary>
        /// Փոփոխման ա/թ
        /// </summary>
        public DateTime? EditingDate { get; set; }

        /// <summary>
        /// Փոփոխողի ՊԿ
        /// </summary>
        public ushort EditorSetNumber { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Բացողի մ/ճ
        /// </summary>
        public ushort OpenerFilialCode { get; set; }

        /// <summary>
        /// Աբոնոնտի համար
        /// </summary>
        public string AbonentNumber { get; set; }

        /// <summary>
        /// կոմունալի մ/ճ
        /// </summary>
        public string BranchCode { get; set; }

        /// <summary>
        /// Վերադարձնում է հաճախորդի գործող կոմունալի քարտերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<CustomerCommunalCard> GetCustomerCommunalCards(ulong customerNumber)
        {
            return CustomerCommunalCardDB.GetCustomerCommunalCards(customerNumber);
        }

        /// <summary>
        /// Կոմունալի քարտի պահպանում
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveCustomerCommunalCard()
        {
            ActionResult result = new ActionResult();

            result = Validation.ValidateCustomerCommunalCard(this);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = CustomerCommunalCardDB.SaveCustomerCommunalCard(this);
            return result;
        }

        /// <summary>
        /// Կոմունալի քարտի կարգավիճակի փոփոխում
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeCustomerCommunalCardQuality()
        {
            ActionResult result = new ActionResult();
            result = CustomerCommunalCardDB.ChangeCustomerCommunalCardQuality(this);
            return result;
        }

        /// <summary>
        /// Ստուգում է կա արդեն մուտքագրված տվյալ կոմունալ քարտը թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool CheckCustomerCommunalCard()
        {
            return CustomerCommunalCardDB.CheckCustomerCommunalCard(this);
        }
    }
}
