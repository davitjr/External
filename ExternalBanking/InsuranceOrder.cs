using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using System.ServiceModel;

namespace ExternalBanking
{
    /// <summary>
    /// Ապահովագրության հայտ
    /// </summary>
    public class InsuranceOrder : Order
    {

        /// <summary>
        /// Ապահովագրություն
        /// </summary>
        public Insurance Insurance { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        ///ajl apahovagrakan company
        /// </summary>
        public int OtherCompany { get; set; }

        /////// <summary>
        ///////apahovagrakan paymanagri tesak
        /////// </summary>
        //public int ContractType { get; set; }




        // -- Hayk Khachatryan//
        

        public static Dictionary<string, string> GetInsuranceContractTypes(bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            return OtherInsuranceOrderDB.GetInsuranceContractTypes(isLoanProduct, isSeparatelyProduct, isProvision);
        }

        public static Dictionary<string, string> GetInsuranceTypesByContractType(int insuranceContractType,bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            return OtherInsuranceOrderDB.GetInsuranceTypesByContractType(insuranceContractType, isLoanProduct, isSeparatelyProduct, isProvision);
        }

        public static bool HasPermissionForDelete(short setNumber)
        {
            return OtherInsuranceOrderDB.HasPermissionForDelete(setNumber);

        }


        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            if (this.Type == OrderType.CashInsuranceOrder)
            {
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.DebitAccount.Currency, user.filialCode);
            }
            if(this.DebitAccount!= null)
            {
                this.Currency = this.DebitAccount.Currency;
            }
            if(this.DebitAccount == null)
            {
                this.Currency = null;
            }
            

            
            this.ReceiverAccount = Insurance.GetInsuraceCompanySystemAccount(this.Insurance.Company, this.Insurance.InsuranceType);



        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateInsuranceOrder(this));
            return result;
        }


        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }


        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {
            InsuranceOrderDB.GetInsuranceOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }


        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if(this.Insurance.InsuranceContractType != 2)
            {
                if (this.Type == OrderType.InsuranceOrder)
                {
                    result = this.ValidateForSend();
                    if (result.Errors.Count > 0)
                    {
                        result.ResultCode = ResultCode.ValidationError;
                        return result;
                    }
                }
            }

            

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = InsuranceOrderDB.SaveInsuranceOrder(this, userName, source);
                
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = base.SaveOrderOPPerson();
                result = base.SaveOrderFee();
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
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);

            return resultConfirm;
        }

        public static void DeleteInsurance(long insuranceId)
        {
            OtherInsuranceOrderDB.DeleteInsurance(insuranceId);
        }


    }
}
