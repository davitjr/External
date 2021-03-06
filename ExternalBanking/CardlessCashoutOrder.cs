using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.Helpers;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class CardlessCashoutOrder : Order
    {

        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ConvertationRate { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճարի հաշվեհամար
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false հակառակ դեպքում
        /// </summary>
        public bool AcknowledgedByCheckBox { get; set; }

        /// <summary>
        /// Գումարը ՀՀ դրամով
        /// </summary>
        public double AmountInAmd { get; set; }

        /// <summary>
        /// Հայտի ուղարկման պահին գեներացվող թվային կոդ ("34" + գործարքի կոդի վերջին 6 նիշ)
        /// </summary>
        public string OrderOTP { get; set; }

        /// <summary>
        /// Sms ուղարկվող OTP
        /// </summary>
        public string AtmOTP { get; set; }
        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված գումարը՝ ՀՀ դրամով
        /// </summary>
        public double AmountInAMDNotConverted { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double TransferFee { get; set; }
        /// <summary>
        ///Քարտային ելքագրման միջնորդավճար
        /// </summary>
        public double CardFee { get; set; }
        /// <summary>
        /// OTP Գրանցման ամսաթիվ
        /// </summary>
        public DateTime? OTPGenerationDate { get; set; }
        /// <summary>
        /// օգտագործե՞լ վարկային գիծ
        /// </summary>
        public bool UseCreditLine { get; set; }
        /// <summary>
        /// հասանելի ժամանակահատված
        /// </summary>
        public byte ValidHours { get; set; } = 24;
        /// <summary>
        /// տրանզակցիայի ունիկալ համար
        /// </summary>
        public string TransactionID { get; set; }
        /// <summary>
        /// բանկոմատի ունիկալ համար
        /// </summary>
        public string ATMID { get; set; }
        /// <summary>
        /// կանխիկացման փորձի ամսաթիվ
        /// </summary>
        public DateTime? CashoutAttemptDate { get; set; }
        /// <summary>
        /// կանխիկացման մերժման պատճառ
        /// </summary>
        public string RejectionMessage { get; set; }
        /// <summary>
        /// կանխիկացման խնդրի նկարագրություն
        /// </summary>
        public SvipErrorResponse SvipError { get; set; }
        public class SvipErrorResponse
        {
            public string ErrorCode { get; set; }
            public string ErrorDescription { get; set; }
        }
        /// <summary>
        /// կանխիկացման կարգավիճակ
        /// </summary>
        public CardLessCashoutStatus AttemptStatus { get; set; }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            Complete();
            ActionResult result = Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardlessCashoutOrderDB.Save(this, userName, source);

                ActionResult resultOrderFee = SaveOrderFee();
                ActionResult resultOpPerson = base.SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public ContentResult<string> Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ContentResult<string> result = new ContentResult<string>();
            ActionResult validationResult = ValidateForSend();
            result.Errors = validationResult.Errors;
            result.ResultCode = validationResult.ResultCode;
            if (validationResult.ResultCode == ResultCode.Normal)
            {
                using TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                ActionResult actionResult = base.Approve(schemaType, userName);

                if (actionResult.ResultCode == ResultCode.Normal)
                {
                    result.ResultCode = ResultCode.Normal;
                    Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    result.Content = GenerateOrderOTP(Id);
                    UpdateInDB(result.Content, Id);
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }
            }
            //result = base.Confirm(user);

            return result;

        }

        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));
            result.ResultCode = result.Errors.Count > 0 ? ResultCode.ValidationError : ResultCode.Normal;

            if (result.Errors.Exists(a => a.Code == 1099))
                SendNotSufficientFundsSms();

            return result;
        }

        public void Get(Languages lang)
        {
            CardlessCashoutOrderDB.Get(this, lang);
            OPPerson = OrderDB.GetOrderOPPerson(Id);
            Fees = GetOrderFees(Id);
            CardFee = Fees.Where(x => x.Type == 7).FirstOrDefault() is null ? 0 : Fees.Where(x => x.Type == 7).FirstOrDefault().Amount;
            TransferFee = Fees.Where(x => x.Type == 20).FirstOrDefault().Amount;
        }

        /// <summary>
        /// Գեներացնում է 6 նիշանոց ՕՏՊ սմս ուղարկելու համար
        /// </summary>
        public static string GenerateAtmOtp(int otpLenght)
        {
            Random rnd = new Random();
            string chars = "0123456789";
            string verificationCode = new string(Enumerable.Repeat(chars, otpLenght).Select(s => s[rnd.Next(s.Length)]).ToArray());
            return verificationCode;
        }

        public static string GenerateOrderOTP(long docId)
        {
            return "34" + docId.ToString().Substring(docId.ToString().Length - 6);
        }

        public static void UpdateInDB(string OTP, long docId)
        {
            CardlessCashoutOrderDB.UpdateInDB(OTP, docId);
        }

        public static CardlessCashoutOrder GetCardlessCashoutOrderWithVerification(string cardlessCashOutCode)
        {
            CardlessCashoutOrder cardlessCashoutOrder = CardlessCashoutOrderDB.GetCardlessCashoutOrder(cardlessCashOutCode);
            cardlessCashoutOrder.SvipError = Validation.ValidateGetCardlessCashoutOrderWithVerification(cardlessCashoutOrder);
            if (string.IsNullOrEmpty(cardlessCashoutOrder.SvipError.ErrorCode))
            {
                SendAtmOtpWithSms(cardlessCashoutOrder);
            }

            return cardlessCashoutOrder;
        }
        private ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            if (AmountInAMDNotConverted == 0 || (AmountInAMDNotConverted % 1000) != 0)
            {//Մուտքագրված գումարը սխալ է։ Գումարը պետք է լինի 1000 ՀՀ դրամի և ամբողջ թվի բազմապատիկ։
                result.Errors.Add(new ActionError(1884));
            }

            if (AmountInAMDNotConverted > 399_000)
            {
                //Գործարքի առավելագույն սահմանաչափը 399,000 ՀՀ դրամ է:
                result.Errors.Add(new ActionError(1886));
            }

            if (Currency != "AMD")
            {
                double roundedNumber = Math.Round((Utility.RoundAmount((double)((decimal)Amount * (decimal)ConvertationRate), "AMD") / 1000)) * 1000;
                if (roundedNumber != AmountInAMDNotConverted)
                {
                    //Դրամային գումարը սխալ է:
                    result.Errors.Add(new ActionError(781));
                }

                if (Utility.RoundAmount(AmountInAMDNotConverted / ConvertationRate, Currency) != Amount)
                {
                    //Արժույթի գումարը սխալ է:
                    result.Errors.Add(new ActionError(781));
                }
            }
            //Դեբետ հաշվի ստուգում
            result.Errors.AddRange(Validation.ValidateDebitAccount(this, DebitAccount));

            return result;
        }

        private void Complete()
        {
            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber))
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            SubType = 1;
            //միջնորդավճարների պահպանում
            if (Fees == null)
            {
                Fees = new List<OrderFee>();
            }

            if (TransferFee > 0)//փոխանցման միջնորդավճար
            {
                OrderFee orderFee = new OrderFee
                {
                    Type = 20,
                    Account = FeeAccount,
                    Currency = FeeAccount?.Currency,
                    Amount = TransferFee
                };
                Fees.Add(orderFee);
            }

            if (CardFee > 0)//քարտի կանխիկացման միջնորդավճար
            {
                OrderFee orderFee = new OrderFee
                {
                    Type = 7,
                    Account = DebitAccount,
                    Currency = DebitAccount?.Currency,
                    Amount = CardFee
                };
                Fees.Add(orderFee);
            }
            //գործարքը կատարող անձի ինիցիալիզացում
            if (Source != SourceType.Bank || (Source == SourceType.Bank && OPPerson == null))
            {
                OPPerson = Order.SetOrderOPPerson(CustomerNumber);
            }

            //վերահաշվարկված գումար
            if (Currency != "AMD")
            {
                AmountInAmd = Math.Round(Amount * ConvertationRate, 1, MidpointRounding.AwayFromZero);
            }
            else
            {
                AmountInAmd = AmountInAMDNotConverted;
            }

            Description = $"Փոխանցում բանկոմատին (հեռ․ {MobilePhoneNumber})";
        }
        public static double GetOrderFee(double orderAmountInAMD)
        {
            return CardlessCashoutOrderDB.GetServiceFee(orderAmountInAMD); //թեստային, մինչև սակագնային հանձնաժողովի որոշում
        }

        public static (bool IsCodeVerified, CardlessCashoutOrder cardlessCashoutOrder) GetCardlessCashoutOrderWithVerificationForNCR(string otp)
        {
            CardlessCashoutOrder cardLessCashOutOrder = new CardlessCashoutOrder();
            bool IsCodeVerified = CardlessCashoutOrderDB.IsCardlessCashCodeCorrect(otp);
            if (IsCodeVerified)
            {
                cardLessCashOutOrder = CardlessCashoutOrderDB.GetCardlessCashoutOrderForAtmView(otp);
                SendAtmOtpWithSms(cardLessCashOutOrder);
            }

            return (IsCodeVerified, cardLessCashOutOrder);
        }

        public ActionResult Confirm(ulong docID, string TransactionId, string AtmId, SourceType source)
        {
            this.user = new User() { userID = 88, userName = "Auto" };
            ActionResult validationResult = ValidateForConfirm(this);
            SaveInProcess(this.Id, AtmId, TransactionId);
            if (validationResult.ResultCode == ResultCode.ValidationError)
                return validationResult;
            return CardlessCashoutOrderDB.Confirm(docID, TransactionId, AtmId, source);
        }

        private static void SendAtmOtpWithSms(CardlessCashoutOrder cardLessCashOutOrder)
        {
            cardLessCashOutOrder.AtmOTP = GenerateAtmOtp(6);
            string message = $"Kankhikacman mekangamya kodn e: {cardLessCashOutOrder.AtmOTP}​";

            if (!cardLessCashOutOrder.MobilePhoneNumber.StartsWith("0") && cardLessCashOutOrder.MobilePhoneNumber.Length == 8)
            {
                cardLessCashOutOrder.MobilePhoneNumber = cardLessCashOutOrder.MobilePhoneNumber.Insert(0, "+374");
            }
            else if (cardLessCashOutOrder.MobilePhoneNumber.StartsWith("0") && cardLessCashOutOrder.MobilePhoneNumber.Length == 9)
            {
                cardLessCashOutOrder.MobilePhoneNumber = cardLessCashOutOrder.MobilePhoneNumber.Remove(0, 1).Insert(0, "+374");
            }

            SmsHelper.SendSms(cardLessCashOutOrder.MobilePhoneNumber, cardLessCashOutOrder.CustomerNumber, message, 88, 42);
        }
        private void SendNotSufficientFundsSms()
        {
            Phone phone = ACBAOperationService.GetCustomerMainMobilePhone(CustomerNumber)?.phone;
            string phoneNumber = phone?.countryCode + phone?.areaCode + phone?.phoneNumber;
            string message = $"Dzer {AmountInAmd} gumarov anqart kankhikacman gorcarqy merzhvats e. Anbavarar mijocner. Gortsarqn avartelu hamar hamalreq hashivy.";
            SmsHelper.SendSms(phoneNumber, CustomerNumber, message, 88, 42);
        }

        public static bool IsCardlessCashCodeCorrect(string cardlessCashoutCode)
        {
            return CardlessCashoutOrderDB.IsCardlessCashCodeCorrect(cardlessCashoutCode);
        }

        public static void WriteCardlessCashoutLog(ulong docID, bool isOk, string msgArm, string msgEng, string AtmId, byte step)
        {
            CardlessCashoutOrderDB.WriteCardlessCashoutLog(docID, isOk, msgArm, msgEng, AtmId, step);
        }

        public static void SaveCancelNotificationMessage(string request)
        {
            CardlessCashoutOrderDB.SaveCancelNotificationMessage(request);
        }

        private static void SendNotSufficientFundsSms(ulong customerNumber, double amountInAmd)
        {
            Phone phone = ACBAOperationService.GetCustomerMainMobilePhone(customerNumber)?.phone;
            string phoneNumber = phone?.countryCode + phone?.areaCode + phone?.phoneNumber;
            string message = $"Dzer {amountInAmd} gumarov anqart kankhikacman gorcarqy merzhvats e. Anbavarar mijocner. Gortsarqn avartelu hamar hamalreq hashivy.";
            SmsHelper.SendSms(phoneNumber, customerNumber, message, 88, 42);
        }

        private static ActionResult ValidateForConfirm(CardlessCashoutOrder order)
        {
            ActionResult result = new ActionResult();

            if (order.Quality != OrderQuality.Sent3)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
            }

            if (CheckOrderProcess(order.Id))
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(2046));
            }

            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(order));

            if (result.Errors.Exists(a => a.Code == 1099))
                SendNotSufficientFundsSms(order.CustomerNumber, order.AmountInAmd);

            result.ResultCode = result.Errors.Count > 0 ? ResultCode.ValidationError : ResultCode.Normal;
            return result;
        }
        private void SaveInProcess(long id, string atmId, string transactionId) => CardlessCashoutOrderDB.SaveInProcess(id, atmId, transactionId);

        private static bool CheckOrderProcess(long id) => CardlessCashoutOrderDB.CheckOrderProcess(id);

        /// <summary>
        /// Վճարման հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public ActionResult ApproveForApproves(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            ActionResult validationResult = ValidateForSend();
            result.Errors = validationResult.Errors;
            result.ResultCode = validationResult.ResultCode;
            if (validationResult.ResultCode == ResultCode.Normal)
            {
                using TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                ActionResult actionResult = base.Approve(schemaType, userName);

                if (actionResult.ResultCode == ResultCode.Normal)
                {
                    result.ResultCode = ResultCode.Normal;
                    Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    result.Id = Convert.ToInt32("34" + Id.ToString().Substring(Id.ToString().Length - 6));
                    UpdateInDB(GenerateOrderOTP(Id), Id);
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }
            }

            return result;

        }
    }
}
