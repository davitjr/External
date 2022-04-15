using ExternalBanking.DBManager;
using System;

namespace ExternalBanking
{
    public class TransferByCall
    {
        /// <summary>
        /// Կատարված զանգի ունիկալ համար
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Փոխանցման արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Փոխանցման տեսակի անվանումը
        /// </summary>
        public string TransferSystemDescription { get; set; }
        /// <summary>
        /// Փոխանցման տեսակը
        /// </summary>
        public short TransferSystem { get; set; }
        /// <summary>
        /// Փոխանցման կոդը
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Առքի փոխարժեք
        /// </summary>
        public double RateBuy { get; set; }
        /// <summary>
        /// Վաճառքի փոխարժեք
        /// </summary>
        public double RateSell { get; set; }
        /// <summary>
        /// Զանգի ժամանակը
        /// </summary>
        public DateTime CallTime { get; set; }
        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public long RegistrationSetNumber { get; set; }
        /// <summary>
        /// Հաճախորդի հեռախոսահամար
        /// </summary>
        public string ContactPhone { get; set; }
        /// <summary>
        /// Հաճախորդի մասնաճյուղ
        /// </summary>
        public ushort ContractFilialCode { get; set; }
        /// <summary>
        /// Համաձայնագրի համար
        /// </summary>
        public ulong ContractID { get; set; }
        /// <summary>     
        /// Հաստատման-մերժման ամսաթիվ 
        /// </summary>
        public DateTime ConfirmationDate { get; set; }
        /// <summary>
        /// Հաստատման-մերժման ՊԿ
        /// </summary>
        public long ConfirmationSetNumber { get; set; }
        /// </summary>
        /// Երկորդ Հաստատման-մերժման ամսաթիվ  
        /// </summary>
        public DateTime ConfirmationDate2 { get; set; }
        /// <summary>
        /// Երկորդ Հաստատման-մերժման ՊԿ
        /// </summary>
        public long ConfirmationSetNumber2 { get; set; }
        /// </summary>
        /// Նստեցման ամսաթիվ  
        /// </summary>
        public DateTime TransferConfirmationDate { get; set; }
        /// <summary>
        /// Հաճախորդի հաղորդած գումար
        /// </summary>
        public double CustomerAmount { get; set; }
        /// <summary>
        /// Մերժման պատճառ
        /// </summary>
        public string RegectDescription { get; set; }
        /// <summary>
        /// Զանգի կարգավիճակի նկարագրություն
        /// </summary>
        public string TransferQualityDescription { get; set; }
        /// <summary>
        /// Հաշվի արժույթ
        /// </summary>
        public string AccountCurrency { get; set; }

        /// Հաշվի նկարագրություն
        /// </summary>
        public string AccountDescription { get; set; }

        /// Զանգի կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }

        ///  Գրանցողի ՊԿ
        /// </summary>
        public short RegisteredBy { get; set; }

        ///  Երկիր
        /// </summary>
        public string Country { get; set; }

        ///  Ուղարկող
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double AmountForPayment { get; set; }

        /// <summary>
        /// Փոխանցման համապատասխան հայտի մուտքագրման աղբյուր
        /// </summary>
        public SourceType Source { get; set; }

        /// <summary>           
        /// Փոխանցման հայտի պահպանում         
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(ACBAServiceReference.User user)
        {
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }
            return TransferByCallDB.Save(this, user);
        }

        /// <summary>           
        /// Փոխանցումը ուղարկել վճարման  
        /// <param name="user"></param>
        /// <returns></returns>
        public static ActionResult SendTransfeerCallForPay(ulong transferID, ACBAServiceReference.User user, DateTime SetDate)
        {
            TransferByCall transfer = TransferByCall.Get((long)transferID);
            ActionResult result = transfer.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }
            return TransferByCallDB.SendTransfeerCallForPay(transferID, user, SetDate);
        }

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Quality != 0 || this.RegisteredBy == 1)
            {
                //Հաճախորդը գտնված չէ:
                result.Errors.Add(new ActionError(813));
            }

            return result;
        }
        /// <summary>
        /// Փոխանցման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateTransferbyCall(this));
            return result;
        }
        /// <summary>
        /// Փոխանցման կոդի մինիմալ երկարություն
        /// </summary>
        /// <param name="transfersystem"></param>
        /// <returns></returns>
        public int CodeMinLenght(int transfersystem)
        {
            return TransferByCallDB.CodeMinLenght(transfersystem);
        }
        /// <summary>
        /// Փոխանցման կոդի մաքսիմալ երկարություն
        /// </summary>
        /// <param name="transfersystem"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public int CodeMaxLenght(int transfersystem)
        {
            return TransferByCallDB.CodeMaxLenght(transfersystem);
        }
        /// <summary>
        /// Հաճախորդի համաձայնագրի ստուգում
        /// </summary>
        /// <returns></returns>
        public bool HasContract()
        {
            return TransferByCallDB.HasContract(this.CustomerNumber);
        }
        /// <summary>
        /// Վերադարձնում է մեկ զանգի հայտ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static TransferByCall Get(long Id)
        {
            return TransferByCallDB.Get(Id);
        }

        /// <summary>
        /// Ստուգում է գոյություն ունի նշված տեսակի ակտիվ փոխանցում
        /// </summary>
        /// <returns></returns>
        public bool CheckActiveTransferSystem()
        {

            return TransferByCallDB.CheckActiveTransferSystem(this.TransferSystem);

        }
        /// <summary>
        /// Նույն տվյալներով փոխանցման առկայության ստուգում
        /// </summary>
        /// <returns></returns>
        public bool CheckExistingTransfer()
        {
            return TransferByCallDB.CheckExistingTransfer(this);
        }
    }
}
