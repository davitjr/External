using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class ProductNote
    {
        /// <summary>
        /// Ունիկալ համար (հաշվեհամար, պրոդուկտի ունիկալ համար՝ app_id, ...)
        /// </summary>
        public double UniqueId { get; set; }

        /// <summary>
        /// Նշում
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Փոփոխման ամսաթիվ
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// Պահպանում է պրոդուկտի նշումը
        /// </summary>
        /// <returns></returns>
        public ActionResult Save()
        {
            ActionResult result = Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = ProductNoteDB.SaveProductNote(this);

                    scope.Complete();
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }


        /// <summary>
        /// Վերադարձնում է պրոդուկտի տրված նշումը
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public static ProductNote GetProductNote(double uniqueId)
        {
            return ProductNoteDB.GetProductNote(uniqueId);
        }

        /// <summary>
        /// Պրոդուկտի նշման պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            return result;
        }
    }
}
