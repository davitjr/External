using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking.XBManagement
{
    public class ApprovementSchemaDetails
    {
        /// <summary>
        /// Սխեմայի մանրամասների ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Սխեմայի քայլ
        /// </summary>
        public ApprovementSchemaStep Step { get; set; }

        /// <summary>
        /// Քայլը կատարող օգտագործողների խումբ
        /// </summary>
        public XBUserGroup Group { get; set; }

        /// <summary>
        /// Քայլի հերթական համար
        /// </summary>
        public byte Order { get; set; }

       

        /// <summary>
        /// Հաստատման սխեմայի մանրամասների պահպանում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="schemaId"></param>
        /// /// <param name="user"></param>
        public ActionResult Save(ulong customerNumber, int schemaId, long orderId)
        {
            ActionResult result = this.Validate(schemaId, customerNumber);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }
                            
                if(result.ResultCode == ResultCode.Normal)
                {
                    //result = this.Step.Save(schemaId, user);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        this.Step = new ApprovementSchemaStep();
                        this.Step.Description = "Քայլ " + this.Order.ToString();
                        result = ApprovementSchemaDetailsDB.Save(this, schemaId, orderId);
                    }                           
                }       
                
            return result;
        }

        /// <summary>
        /// Հաստատման սխեմայի մանրամասների ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(int schemaId, ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateApprovementSchemaDetails(this, schemaId, customerNumber));
            return result;
        }

       

        /// <summary>
        /// Վերադարձնում է հաճախորդի հաստատման սխեմայի մանրամասները
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        public static List<ApprovementSchemaDetails> Get(int schemaId)
        {
            List<ApprovementSchemaDetails> schemaDetailsList = new List<ApprovementSchemaDetails>();
                            schemaDetailsList = ApprovementSchemaDetailsDB.GetApprovementSchemaDetails(schemaId);
                foreach (ApprovementSchemaDetails schemaDetails in schemaDetailsList)
                {
                    schemaDetails.Group = XBUserGroup.Get(schemaDetails.Group.Id);
                    schemaDetails.Step = ApprovementSchemaStep.Get(schemaDetails.Step.Id);
                }

            return schemaDetailsList;
        }

        /// <summary>
        /// Վերադարձնում է հաստատման սխեմայի տրված քայլի մանրամասները
        /// </summary>
        /// <param name="schemaDetailsId"></param>
        /// <returns></returns>
        public static ApprovementSchemaDetails GetApprovementSchemaDetailsById(int schemaDetailsId)
        {
            ApprovementSchemaDetails schemaDetails = new ApprovementSchemaDetails();
           
                schemaDetails = ApprovementSchemaDetailsDB.GetApprovementSchemaDetailsById(schemaDetailsId);            
                schemaDetails.Group = XBUserGroup.Get(schemaDetails.Group.Id);
                schemaDetails.Step = ApprovementSchemaStep.Get(schemaDetails.Step.Id);
            
            return schemaDetails;
        }
        /// <summary>
        /// Հաստատման սխեմայի մանրամասների հեռացում
        /// </summary>
        public ActionResult Remove(long orderId)
        {
            ActionResult result = this.ValidateApprovementSchemaDetailsForRemove();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = ApprovementSchemaDB.RemoveApprovementSchemaDetails(this, orderId);
             
            return result;
        }


        /// <summary>
        /// Հաստատման սխեմայի մանրամասների հեռացման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateApprovementSchemaDetailsForRemove()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateApprovementSchemaDetailsForRemove(this));
            return result;
        }
   
    }
}
