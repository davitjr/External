using System;
using System.Collections.Generic;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի տվյալների փոփոխման հայտ
    /// </summary>
    public class CardDataChangeOrder:Order
    {

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են HBBase ի Tbl_card_data_change_field_types -ում)
        /// </summary>
        public short FieldType { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակի նկարագրություն
        /// </summary>
        public string FieldTypeDescription { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի արժեք
        /// </summary>
        public string FieldValue { get; set; }

        /// <summary>
        /// Փոփոխման հիմք
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Փոփոխման  հիմքի փաստաթղթի ա/թ
        /// </summary>
        public DateTime? DocumentDate { get; set; }


        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(double,DateTime,int.......)
        /// </summary>
        public AdditionalValueType ValueType { get; set; }


        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.ValueType = Info.GetFieldType((ushort)this.FieldType);

            if (this.ValueType == AdditionalValueType.Percent)
            {
                this.FieldValue = (Convert.ToDouble(this.FieldValue)/100).ToString();
            }
            if(this.FieldType==20 || this.FieldType == 21)
            {
                Card card = Card.GetCardWithOutBallance((ulong)this.ProductAppId);
                if(card.Type==23 || card.Type == 40 || card.Type == 34 || card.Type == 50)
                {
                    double price = Utility.GetPriceInfoByIndex(218, "price");
                    this.FieldValue = price.ToString();
                }
                else
                {
                    CardTariffContract contract = CardTariffContract.GetCardTariffs(card.RelatedOfficeNumber);
                    CardTariff tariff = contract.CardTariffs.Find(m => m.Currency == card.Currency && m.CardType == card.Type);
                    if (tariff.SMSFeeFromBank != 0)
                    {
                        this.FieldValue = tariff.SMSFeeFromBank.ToString();
                    }
                    else
                    {
                        this.FieldValue = tariff.SMSFeeFromCustomer.ToString();
                    }

                }
            }



        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardDataChangeOrder(this));
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
                result = CardDataChangeOrderDB.SaveCardDataChangeOrder(this, userName, source);
                
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

        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {
            CardDataChangeOrderDB.GetCardServiceFeeDataChangeOrder(this);
        }

        /// <summary>
        /// Ստուգում է տվյալ դաշտը պարտադիր լրացման է թե ոչ
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static bool CheckFieldTypeIsRequaried(short fieldType)
        {
            return CardDataChangeOrderDB.CheckFieldTypeIsRequaried(fieldType);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ քարտի տվյալների փոփոխությունները ըստ տեսակի
        /// </summary>
        /// <param name="ProductAppId"></param>
        /// <param name="FieldType"></param>
        /// <returns></returns>
        public static List<CardDataChangeOrder> GetCardDataChangesByProduct(long ProductAppId, short FieldType)
        {
            return CardDataChangeOrderDB.GetCardDataChangesByProduct(ProductAppId, FieldType);
        }

    }
}
