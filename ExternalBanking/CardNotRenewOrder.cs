

using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class CardNotRenewOrder : Order
    {
        /// <summary>
        /// Քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Քարտի չվերաթողարկման պատճառ
        /// </summary>
        public int Reason { get; set; }

        /// <summary>
        /// Քարտի չվերաթողարկման պատճառի նկարագրություն
        /// </summary>
        public string ReasonDesc { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        private void Complete()
        {
            this.SubType = 1;
            this.RegistrationDate = DateTime.Now.Date;

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.Bank)
            {
                this.UserId = user.userID;
            }

            this.Type = OrderType.CardNotRenewOrder;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        ///"Չվերաթողարկել քարտը" հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardNotRenewOrder(SourceType source, ACBAServiceReference.User user, short schemaType, ulong customerNumber)
        {
            this.Complete();
            ActionResult result = this.ValidateCardNotRenewOrder(customerNumber);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardNotRenewOrderDB.Save(this, source, user.userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.Approve(schemaType, user.userName);

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

            result = base.Confirm(user);

            return result;
        }

        /// <summary>
        /// "Չվերաթողարկել քարտը" հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult ValidateCardNotRenewOrder(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (!IsNormCardStatus(this.Card.CardNumber, this.Card.ProductId))
            {
                //Հնարավոր չէ կատարել գործողությունը։ Քարտի կարգավիճակը NORM չէ:
                result.Errors.Add(new ActionError(1810));
                return result;
            }

            if (IsCardAlreadyNotRenewed(this.Card.ProductId))
            {
                //Քարտն արդեն չվերաթողարկվել է։
                result.Errors.Add(new ActionError(1833));
                return result;
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է "Չվերաթողարկել քարտը" հայտի տվյալները
        /// </summary>
        public void GetCardNotRenewOrder()
        {
            CardNotRenewOrderDB.GetCardNotRenewOrder(this);
        }

        public static bool IsNormCardStatus(string cardNumber, long productID)
        {
            return CardNotRenewOrderDB.IsNormCardStatus(cardNumber, productID);
        }

        public static bool IsCardAlreadyNotRenewed(long productID)
        {
            return CardNotRenewOrderDB.IsCardAlreadyNotRenewed(productID);
        }
    }
}
