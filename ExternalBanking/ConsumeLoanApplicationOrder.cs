﻿using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class ConsumeLoanApplicationOrder : Order
    {
        /// <summary>
        /// Վարկային պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Տևողություն
        /// </summary>
        public int Duration { get; set; }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = ConsumeLoanApplicationOrderDB.SaveConsumeLoanApplicationOrder(this, userName, source);

                //**********
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();


            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }


            return result;
        }

        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            this.Type = OrderType.ConsumeLoanApplicationOrder;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            //Առկա է խմբագրվող կարգավիճակով հայտ
            long editableOrderId = ExistsConsumeLoanApplicationOrder(this.CustomerNumber, new List<OrderQuality> { OrderQuality.Draft }).Item1;
            if (editableOrderId > 0)
            {
                this.Id = editableOrderId;
            }

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        public void GetConsumeLoanApplicationOrder()
        {
            ConsumeLoanApplicationOrderDB.GetConsumeLoanApplicationOrder(this);
        }

        /// <summary>
        /// Ստուգում է՝ արդյոք տվյալ հաճախորդի համար առկա է տվյալ կարգավիճակով հայտ, թե ոչ
        /// Առկայության դեպքում վերադարձնում է տվյալ հայտի գործարքի կոդը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="qualities"></param>
        /// <returns></returns>
        public static (long, DateTime) ExistsConsumeLoanApplicationOrder(ulong customerNumber, List<OrderQuality> qualities)
        {
            return ConsumeLoanApplicationOrderDB.ExistsConsumeLoanApplicationOrder(customerNumber, qualities);
        }
    }
}
