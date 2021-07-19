using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class BOOrderProduct
    {
        /// <summary>
        /// Հայտի համար
        /// </summary>
        public ulong OrderId { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>                        
        /// <returns></returns>
        public ActionResult Save()
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = BOOrderProductDB.Save(this);

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(MatureOrder matureOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();
                
                orderProduct.OrderId = orderId;
                orderProduct.ProductId = matureOrder.ProductId;
                orderProduct.ProductType = matureOrder.ProductType;
                //if (matureOrder.Type == OrderType.OverdraftRepayment)
                //{
                //    orderProduct.ProductType = ProductType.CommercialCreditLine;
                //}                    
                //else
                //{
                //    orderProduct.ProductType = ProductType.Loan; //petq e voroshel vark, aparik texum ...
                //}                    
                orderProduct.Save();                

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(CardClosingOrder cardClosingOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = cardClosingOrder.ProductId;
                orderProduct.ProductType = ProductType.Card;
                
                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(CreditLineTerminationOrder creditLineTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = creditLineTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.Card;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(DepositTerminationOrder depositTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = depositTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.Deposit;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(PeriodicTerminationOrder periodicTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = periodicTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.PeriodicTransfer;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(CredentialTerminationOrder credentialTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = credentialTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.Credential;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }
        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>        
        /// <returns></returns>
        public static ActionResult Save(FactoringTerminationOrder factoringTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = factoringTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.Factoring;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }
        public static ActionResult Save(LoanProductTerminationOrder guaranteeTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                BOOrderProduct orderProduct = new BOOrderProduct();

                orderProduct.OrderId = orderId;
                orderProduct.ProductId = guaranteeTerminationOrder.ProductId;
                orderProduct.ProductType = ProductType.Guarantee;

                orderProduct.Save();

                scope.Complete();
            }
            return result;
        }
    }
}
