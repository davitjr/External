using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.CreditLineActivatorARCA;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// 
    /// </summary>
    public class CreditLineProlongationOrder : Order
    {

        /// <summary>
        /// Վարկային գծի ունիկալ համար
        /// </summary>
        public ulong ProductAppID { get;set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateCreditLineProlongationOrder(this));

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

        /// <summary>
        /// Կրկնակի հայտի առկայության ստուգում 
        /// </summary>
        internal static bool IsSecontCreditLineProlongationApplication(ulong ProductAppID, ushort orderType, long docId)
        {
            return CreditLineProlongationOrderDB.IsSecontCreditLineProlongationApplication(ProductAppID, orderType, docId);
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

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CreditLineProlongationOrderDB.SaveCreditLineProlongationOrder(this, userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                Order.SaveOrderProductId(this.ProductAppID,this.Id);

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

            // Վարկային գծի Երկարաձգում հայտի կատարման ժամանակ

            bool isCreditLineOnline = bool.Parse(ConfigurationManager.AppSettings["IsCreditLineOnline"].ToString());

            if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CreditLineProlongationOrder && isCreditLineOnline == true && source == SourceType.Bank)
            {
                try
                {
                    Utility.SaveCreditLineLogs(ProductAppID, "UntilIsProlongApiGate", " ");
                    if (ChangeExceedLimitRequest.IsProlongApiGate(ProductAppID))
                    {
                        Utility.SaveCreditLineLogs(ProductAppID, "IsProlongApiGate", " ");
                        warnings = ChangeExceedLimitRequest.ProlongCreditLine(CustomerNumber, ProductAppID, this.Id, user.userID, this.Type);
                    }
                }
                catch (Exception ex)
                {
                    warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                    
                    string message = (ex.Message != null ? ex.Message : " ") +
                                      Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "")
                                    + " stacktrace:" + (ex.StackTrace != null ? ex.StackTrace : " ");

                    Utility.SaveCreditLineLogs(ProductAppID, " ", message);
                }
            }
            resultConfirm.Errors = warnings;

            return resultConfirm;


        }

        private void Complete()
        {
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }


        public void GetCreditLineProlongationOrder()
        {
            CreditLineProlongationOrderDB.GetCreditLineProlongationOrder(this);
        }

        internal static bool IsCreditLineProlongation(ulong ProductAppID)
        {
            bool IsCreditLineProlongation = CreditLineProlongationOrderDB.IsCreditLineProlongation(ProductAppID);
            return IsCreditLineProlongation;
        }

    }

}
