using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


namespace ExternalBanking.XBManagement
{
    public class PhoneBankingContractOrder : Order
    {      
        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public String ContractNumber { get; set; }


        /// <summary>
        /// Պայմանագրի ա/թ
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        ///Մեկ գործարքի սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double OneTransactionLimitToOwnAccount { get; set; }

        /// <summary>
        ///Մեկ գործարքի սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double OneTransactionLimitToAnothersAccount { get; set; }

        /// <summary>
        /// Օրական սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double DayLimitToOwnAccount { get; set; }

        /// <summary>
        /// Օրական սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double DayLimitToAnothersAccount { get; set; }   

        /// <summary>
        /// Հարցերի պատասխանների ցուցակ
        /// </summary>
        public List<PhoneBankingContractQuestionAnswer> QuestionAnswers { get; set; }

        /// <summary>
        /// Հաճախորդի էլ․հասցեի ունիկալ համար (Id)
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// Հաճախորդի հեռախոսահամարի ունիկալ համար (Id)
        /// </summary>
        public int PhoneId { get; set; }

        /// <summary>
        /// Phone Banking պայմանագրի հայտի տեսակ (մուտքագրման / խմբագրման)
        /// </summary>
        //public Action Action { get; set; }


        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
           
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PhoneBankingContractOrderDB.Save(this, userName, source);
               
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            result = base.Confirm(user);
            return result;
        }

        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }


        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidatePhoneBankingContractOrder(this));


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        public void Get()
        {
            PhoneBankingContractOrderDB.Get(this);
        }

        public void GetPhoneBankingContractOrder()
        {
            PhoneBankingContractOrderDB.Get(this);
        }


    }
}
