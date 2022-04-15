using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Փոխանցում խանութի հաշվին
    /// </summary>
    public class TransferToShopOrder : Order
    {
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
       // public override Account DebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }



        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.RegistrationDate = DateTime.Now.Date;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.ReceiverAccount = GetShopAccount(this.ProductId);
            this.DebitAccount = GetShopDebitAccount(this.ProductId);
            this.Amount = GetShopTransferAmount(this.ProductId);
        }


        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {
            TransferToShopOrderDB.GetTransferToShopOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateTransferToShopOrder(this));
            return result;
        }


        /// <summary>
        /// Ստուգում է արդեն վճարված է խանութին թե ոչ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckTransferToShopPayment(ulong productId)
        {
            return TransferToShopOrderDB.CheckTransferToShopPayment(productId);
        }

        /// <summary>
        /// Վերադարձնում է պրոդուկտին կցված խանութի հաշիվը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Account GetShopAccount(ulong productId)
        {
            return TransferToShopOrderDB.GetShopAccount(productId);
        }



        /// <summary>
        /// Վերադարձնում է խանութին փոխանցվող գումարը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static double GetShopTransferAmount(ulong productId)
        {
            return TransferToShopOrderDB.GetShopTransferAmount(productId);
        }

        /// <summary>
        /// Վերադարձնում է պրոդուկտին կցված խանութի դեբետ հաշիվը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Account GetShopDebitAccount(ulong productId)
        {
            return TransferToShopOrderDB.GetShopDebitAccount(productId);
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
                result = TransferToShopOrderDB.SaveTransferToShopOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = base.SaveOrderOPPerson();

                Order.SaveOrderProductId(this.ProductId, this.Id);

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

    }
}
