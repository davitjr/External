using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class UtilityOptions
    {

        public int ConfigId { get; set; }

        public int TypeID { get; set; }
        /// <summary>
        /// PK
        /// </summary>
        public int NumberOfSet { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակ
        /// </summary>
        public CommunalTypes Type { get; set; }


        /// <summary>
        /// նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Միացված է թե անջատած
        /// </summary>
        public bool IsEnabled { get; set; }

        public DateTime RegistrationDate { get; set; }


        public static List<UtilityOptions> GetUtiltyForChange()
        {
            List<UtilityOptions> list = UtilityOptionsDB.GetUtiltyForChange();
            return list;
        }



        public static ActionResult SaveUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions)
        {
            ActionResult result = new ActionResult();

            List<UtilityOptions> oldOptions = UtilityOptions.GetUtiltyForChange();

            utilityOptions.ForEach(m =>
            {
                if (!oldOptions.Exists(o => o.IsEnabled == m.IsEnabled && o.TypeID == m.TypeID))
                {
                    UtilityOptionsDB.SaveUtilityConfigurationsAndHistory(m);
                }

            });

            result.ResultCode = ResultCode.Normal;
            return result;
        }


        public static ActionResult SaveAllUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions, int a)
        {
            ActionResult actionResult = new ActionResult();
            actionResult = UtilityOptionsDB.SaveAllUtilityConfigurationsAndHistory(utilityOptions, a);
            return actionResult;
        }



        public static List<string> GetExistsNotSentAndSettledRows(Dictionary<int, bool> keyValues)
        {
            List<string> list = UtilityOptionsDB.GetExistsNotSentAndSettledRows(keyValues);

            return list;
        }



    }
}
