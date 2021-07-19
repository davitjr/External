using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class OrderRejection
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; } 

        /// <summary>
        /// Մերժման ենթակա գործարքի կոդը
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Մերժման պատճառ
        /// </summary>
        public string RejectReason { get; set; }

        /// <summary>
        /// Մերժող օգտագործողի մուտքանուն
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Մերժող օգտագործողի ունիկալ համար
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Գործարքի մերժման իրականացում
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public ActionResult Reject(Languages language)
        {
            ActionResult result = this.Validate();

            List<ActionError> warnings = new List<ActionError>();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = OrderRejectionDB.Save(this, language);

            return result;
        }

        /// <summary>
        /// Գործարքի մերժման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (String.IsNullOrEmpty(RejectReason))
            {
                result.Errors.Add(new ActionError(437));
            }

            return result;
        }

    }
}
