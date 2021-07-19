using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class OperDayMode
    {
        #region Properties
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 24/7 ռեժիմի տեսակ
        /// </summary>
        public string ModeTypeText { get; set; }

        /// <summary>
        /// Օգտագործողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Օգտագործողի Փոփոխման ամսաթիվ
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են ACCOPERBASE ի tbl_type_of_24_7_mode -ում)
        /// </summary>
        public OperDayModeType Option { get; set; }

        #endregion

        /// <summary>
        /// 24/7 ռեժիմի տեսակ պահպանում
        /// </summary>
        /// <param name="operDayMode"></param>
        /// <returns></returns>
        public ActionResult SaveOperDayMode(OperDayMode operDayMode)
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
                    result = OperDayModeDB.SaveOpenMode(operDayMode);
                    scope.Complete();
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է 24/7 ռեժիմի տեսակը
        /// </summary>
        /// <param name="GetCurrentOperDay24_7_Mode"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetCurrentOperDay24_7_Mode()
        {
            KeyValuePair<string, string> dictionary = new KeyValuePair<string, string>();
            dictionary = OperDayModeDB.GetCurrentOperDay24_7_Mode();

            return dictionary;
        }

        /// <summary>
        /// 24/7 ռեժիմի տեսակի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = Validation.Validate24_7_mode(this);
            return result;
        }
    }
}
