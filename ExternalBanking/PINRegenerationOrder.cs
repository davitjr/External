using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class PINRegenerationOrder : Order
    {
        /// <summary>
        /// Քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Քարտի տեխնոլոգիա
        /// </summary>
        public string CardTechnology { get; set; }

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

            this.Type = OrderType.PINRegenerationOrder;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        ///Նույն համարով և նույն ժամկետով քարտ հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePINRegOrder(SourceType source, ACBAServiceReference.User user, short schemaType, ulong customerNumber)
        {
            Complete();
            ActionResult result = ValidatePINReg();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = PINRegenerationOrderDB.SavePINRegOrderDetails(this, user.userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = Approve(schemaType, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            result = Confirm(user);

            return result;
        }

        /// <summary>
        /// Նույն համարով և նույն ժամկետով քարտ հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult ValidatePINReg()
        {
            ActionResult result = new ActionResult
            {
                Errors = new List<ActionError>()
            };

            int currentBankCode = user.filialCode;
            if (currentBankCode == 22059 && Card.FilialCode == 22000)
            {
                currentBankCode = Card.FilialCode;
            }
            if (Card.FilialCode != currentBankCode)
            {
                //Այլ մասնաճյուղի քարտ:
                result.Errors.Add(new ActionError(1515));
                return result;
            }

            if (Card.Type == 16 || Card.Type == 1)
            {
                // Visa Electron քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1674, new string[] { "փոխարինել" }));
                return result;
            }

            if (Card.Type == 14 || Card.Type == 32)
            {
                // Maestro տեսակի քարտը հնարավոր չէ փոխարինել:
                result.Errors.Add(new ActionError(1675, new string[] { "փոխարինել" }));
                return result;
            }

            result = CheckArcaStatus();

            return result;
        }


        /// <summary>
        /// Ստուգում է փոխարինվող քարտի կարգավիճակը Արմենիան Քարդ ՊԿ-ում
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckArcaStatus()
        {
            ActionResult result = new ActionResult
            {
                Errors = new List<ActionError>()
            };

            CardIdentification cardIdentification = new CardIdentification();
            cardIdentification.CardNumber = Card.CardNumber;
            cardIdentification.ExpiryDate = Card.ValidationDate.ToString("yyyyMM");

            int status = ArcaDataService.GetCardArCaStatus(cardIdentification);
            if (status == 2)
            {
                // Քարտը բլոկավորված է։ Հնարավոր չէ իրականացնել փոխարինում։ Խնդրում ենք անհրաժեշտության դեպքում ապաբլոկավորել քարտը և կրկին հայտ ուղարկել։
                result.Errors.Add(new ActionError(1661, new string[] { " փոխարինում" }));
                return result;
            }
            else if (status == 3)
            {
                // Կապի խնդիր։ Խնդրում ենք կրկին փորձել մի փոքր ուշ
                result.Errors.Add(new ActionError(1660));
                return result;
            }
            else if (status == 4)
            {
                // Քարտը գտնված չէ
                result.Errors.Add(new ActionError(534));
                return result;
            }
            return result;
        }


        /// <summary>
        /// Վերադարձնում է "Փոխարինում` նույն համար, նույն ժամկետ" հայտի տվյալները
        /// </summary>
        public void GetPINRegenerationOrder()
        {
            PINRegenerationOrderDB.GetPINRegenerationOrder(this);
        }
    }
}
