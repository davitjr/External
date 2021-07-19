using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking.XBManagement
{
    public class ApprovementSchemaStep
    {
        /// <summary>
        /// Քայլի ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Քայլի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Հաստատման սխեմայի քայլի պահպանում
        /// </summary>
        /// <param name="schemaId"></param>
        public ActionResult Save(int schemaId, ACBAServiceReference.User user)
        {
            return ApprovementSchemaStepDB.Save(this, schemaId, user);
        }

        /// <summary>
        /// Վերադարձնում է հաստատման սխեմայի տրված քայլը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ApprovementSchemaStep Get(int id)
        {
            ApprovementSchemaStep step = new ApprovementSchemaStep();
           
                step = ApprovementSchemaStepDB.GetApprovementSchemaStep(id);
            
            return step;
        }

    }
}
