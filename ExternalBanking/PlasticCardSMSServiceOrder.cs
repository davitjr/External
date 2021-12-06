using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի SMS ծառայության ակտիվացման/փոփոխման կամ դադարեցման հայտ
    /// </summary>
    public class PlasticCardSMSServiceOrder : Order
    {
        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductID { get; set; }

        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// 1-գրանցել, 2-դադարեցնել, 3-փոփոխել
        /// </summary>
        public short OperationType { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// SMSFilter 
        /// </summary>
        public string SMSFilter { get; set; }

        /// <summary>
        ///   SMS-ի տեսակ
        /// </summary>
        public short SMSType { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// SMS ծառայության գրանցման գործողության տեսակի նկարագրություն(1-գրանցել, 2-դադարեցնել, 3-փոփոխել)
        /// </summary>
        public string OperationTypeDescription { get; set; }

        /// <summary>
        /// SMS-ի տեսակի նկարագրություն
        /// </summary>
        public string SMSTypeDescription { get; set; }

        /// <summary>
        /// հիմնական քարտի ունիկալ համար
        /// </summary>
        public ulong MainCardProductID { get; set; }

        /// <summary>
        /// Հեռախոսահամարը հայկական է թե ոչ
        /// </summary>
        public bool IsArmenia { get; set; }


        public new ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = PlasticCardSMSServiceOrderDB.SavePlasticCardSMSServiceOrder(this, userName);
                //**********                
                //ulong orderId = base.Save(this, source, user);
                //Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                ////BOOrderPaymentDetails.Save(this, orderId);
                //ActionResult res = BOOrderCustomer.Save(this, orderId, user);
                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                    ActionResult actionResult = new ActionResult();
                    actionResult = base.SaveOrderOPPerson();
                    actionResult = base.SaveOrderFee();
                    if (actionResult.ResultCode != ResultCode.Normal)
                    {
                        return actionResult;
                    }
                }
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;

        }



        /// <summary>
        /// Քարտի SMS ծառայության ակտիվացման/փոփոխման կամ դադարեցման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {


                    result = base.Approve(schemaType, userName);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
                        {
                            user.userName = userName;
                            this.Quality = OrderQuality.Sent;
                        }
                        else
                        {
                            base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                            base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        }
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
            }

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
        /// Քարտի SMS ծառայության ակտիվացման/փոփոխման կամ դադարեցման հայտի հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
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
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                this.Type = OrderType.PlasticCardSMSServiceOrder;
                if(this.OperationType == 2)
                {
                    this.SMSType = 5;
                }
            }

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidatePlasticCardSMSServiceOrder(this));
            return result;
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
                result = PlasticCardSMSServiceOrderDB.SavePlasticCardSMSServiceOrder(this, userName);
                //**********                
                //ulong orderId = base.Save(this, source, user);
                //Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                ////BOOrderPaymentDetails.Save(this, orderId);
                //ActionResult res = BOOrderCustomer.Save(this, orderId, user);
                //**********
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
                if (source == SourceType.Bank)
                    this.Quality = OrderQuality.Draft;

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    if (source == SourceType.Bank)
                        this.Quality = OrderQuality.Sent3;
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


        internal static bool IsSecondSMSServiceOrder(ulong productID, short OperationType)
        {
            bool secondService = PlasticCardSMSServiceOrderDB.IsSecondSMSServiceOrder(productID, OperationType);
            return secondService;
        }

        public void Get()
        {
            PlasticCardSMSServiceOrder order = PlasticCardSMSServiceOrderDB.GetPlasticCardSMSServiceOrder(this);
            order.CardNumber = String.Empty;
            order.CardNumber = Card.GetCardWithOutBallance(order.ProductID).CardNumber;
            order.MainCardProductID = Card.GetMainCardProductId(order.CardNumber);
        }

        public static DataTable GetPlasticCardSmsServiceActions()
        {
            string cacheKey = "PlasticCardSMSServiceOrder_GetPlasticCardSmsServiceActions";
            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = PlasticCardSMSServiceOrderDB.GetPlasticCardSmsServiceActions();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static DataTable GetTypeOfPlasticCardsSMS()
        {
            string cacheKey = "PlasticCardSMSServiceOrder_GetTypeOfPlasticCardsSMS";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = PlasticCardSMSServiceOrderDB.GetTypeOfPlasticCardsSMS();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static DataTable GetAllTypesOfPlasticCardsSMS()
        {
            string cacheKey = "PlasticCardSMSServiceOrder_GetAllTypesOfPlasticCardsSMS";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = PlasticCardSMSServiceOrderDB.GetAllTypesOfPlasticCardsSMS();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }


        public static List<Tuple<string, bool>> GetCardMobilePhones(ulong customerNumber, ulong curdNumber)
        {
            return PlasticCardSMSServiceOrderDB.GetCardMobilePhones(customerNumber, curdNumber);
        }

        public static string GetCurrentPhone(ulong curdNumber)
        {
            return PlasticCardSMSServiceOrderDB.GetCurrentPhone(curdNumber);
        }
        public static string SMSTypeAndValue(string curdNumber)
        {
            return PlasticCardSMSServiceOrderDB.SMSTypeAndValue(curdNumber);
        }

        
    }
}
