using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Հայտի/երի որոնման պարամետրեր
    /// </summary>
    public class Transfer
    {

        public ulong Id { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>     
        /// Գրանցման ժամ 
        /// </summary>
        public TimeSpan? RegistrationTime { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
        public Account DebitAccount { get; set; }


        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account DebForTransferPayment { get; set; }


        /// <summary>
        /// ÂÕÃ³Ïó³ÛÇÝ դեբետ Ñ³ßÇí
        /// </summary>
        public string TransferDocumentDebAccount { get; set; }

        /// <summary>
        /// ÂÕÃ³Ïó³ÛÇÝ կրեդիր Ñ³ßÇí
        /// </summary>
        public string TransferDocumentCredAccount { get; set; }

        /// <summary>
        // Ստացված/ուղարկված
        /// </summary>
        public byte SendOrReceived { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Փոխանցման արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Փոխանցման համակարգը
        /// </summary>
        public short TransferSystem { get; set; }

        /// <summary>
        /// Փոխանցման համակարգի անվանումը
        /// </summary>
        public string TransferSystemDescription { get; set; }

        /// <summary>
        /// Փոխանցման տեսակը
        /// </summary>
        public short TransferGroup { get; set; }

        /// <summary>
        /// Փոխանցման տեսակի անվանումը
        /// </summary>
        public string TransferGroupDescription { get; set; }

        /// <summary>     
        /// Հաստատման-մերժման ամսաթիվ 
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>     
        /// Հաստատման-մերժման ժամ 
        /// </summary>
        public TimeSpan? ConfirmationTime { get; set; }

        /// <summary>
        /// Կասկածելիություն
        /// </summary>
        public short Verified { get; set; }

        /// <summary>
        ///AML բաժնի հաստատում
        /// </summary>
        public short AmlCheck { get; set; }

        /// <summary>
        ///Կասկածելի է AML բաժնի կողմից 
        /// </summary>
        public short VerifiedAml { get; set; }

        /// <summary>
        ///Մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }

        /// <summary>
        ///Փաստաթղթի համար
        /// </summary>
        public string DocumentNumber { get; set; }


        /// <summary>
        /// Ուղարկողի անուն/անվանում
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Ստացող 
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverPassport { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Թվային կոդ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Երկրի անվանում
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Միջնորդավճար արժույթով
        /// </summary>
        public double FeeInCurrency { get; set; }

        /// <summary>
        /// Միջնորդավհար ACBA արժույթով
        /// </summary>
        public double FeeAcba { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPayment { get; set; }

        /// <summary>
        /// Կարգավիճակ (Նորմալ, ետ վերադարձրած, Ուղարկված/Վերադարձված, Ստացված/Չեղարկված, Փոփոխությունը հաստատված է)
        /// </summary>
        public byte Quality { get; set; }

        /// <summary>
        ///Հաճախորդի տեսակ
        /// </summary>
        public string CustomerTypeDescription { get; set; }

        /// <summary>
        /// Ուղարկողի հասցե
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաք
        /// </summary>
        public string SenderTown { get; set; }

        /// <summary>
        /// Ուղարկողի երկիր
        /// </summary>
        public string SenderCountry { get; set; }

        /// <summary>
        /// Ուղարկողի անձնագիր
        /// </summary>
        public string SenderPassport { get; set; }

        /// <summary>
        /// Ուղարկողի ծննդյան ա/թ
        /// </summary>
        public DateTime? SenderDateOfBirth { get; set; }

        /// <summary>
        /// Ուղարկողի էլ. հասցե
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Ուղարկողի ՀՎՀՀ
        /// </summary>
        public string SenderCodeOfTax { get; set; }

        /// <summary>
        /// Ուղարկողի հեռախոս
        /// </summary>
        public string SenderPhone { get; set; }

        /// <summary>
        /// Այլ բանկերում Ուղարկողի հաշվեհամարներ
        /// </summary>
        public string SenderOtherBankAccount { get; set; }

        /// <summary>
        /// Միջնորդ բանկի SWIFT կոդ
        /// </summary>
        public string IntermediaryBankSwift { get; set; }

        /// <summary>
        /// Միջնորդ բանկի անվանում
        /// </summary>
        public string IntermediaryBank { get; set; }

        /// <summary>
        /// Ստացող բանկի SWIFT կոդ
        /// </summary>
        public string ReceiverBankSwift { get; set; }

        /// <summary>
        /// Ստացող բանկի անվանում
        /// </summary>
        public string ReceiverBank { get; set; }

        /// <summary>
        /// Ստացող բանկի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverBankAddInf { get; set; }

        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverAddInf { get; set; }

        /// <summary>
        /// Փոխանցման եղանակ
        /// </summary>
        public string DetailsOfCharges { get; set; }

        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public double AmountForPayment { get; set; }

        /// <summary>
        /// Վճարված գումար
        /// </summary>
        public double PaidAmount { get; set; }

        /// <summary>
        /// ՏՀՏ կոդ   
        /// </summary>
        public short LTACode { get; set; }

        /// <summary>
        /// Ոստիկանության կոդ 
        /// </summary>
        public int PoliceCode { get; set; }

        /// <summary>
        /// Ստացողի հաշիվ
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// Ուղարկողի հաշիվ
        /// </summary>
        public string SenderAccount { get; set; }


        /// <summary>
        /// Ուղարկող բանկի SWIFT կոդ
        /// </summary>
        public string SenderBankSwift { get; set; }

        /// <summary>
        /// Ուղարկող բանկ 
        /// </summary>
        public string SenderBank { get; set; }

        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ
        /// </summary>
        public string AddInf { get; set; }

        /// <summary>
        /// Պարտատեր
        /// </summary>
        public string Debtor { get; set; }

        /// <summary>
        /// Պարտատիրոջ կարգավիճակ
        /// </summary>
        public string DebtorType { get; set; }

        /// <summary>
        /// Պարտատիրոջ կարգավիճակ
        /// </summary>
        public ushort DebtorTypeCode { get; set; }

        /// <summary>
        /// ä³ñï³ïÇñáç ÷³ëï³ÃáõÕÃ
        /// </summary>
        public string DebtorDocumentNumber { get; set; }

        /// <summary>
        /// ä³ñï³ïÇñáç ÷³ëï³ÃáõÕÃ
        /// </summary>
        public string DebtorDocument { get; set; }

        /// <summary>
        /// Ստացողի կարգավիճակ
        /// </summary>
        public string ReceiverType { get; set; }

        /// <summary>
        /// Միջազգային փոխանցման տեսակ
        /// </summary>
        public string MT { get; set; }

        /// <summary>
        /// Գումարը դեբետ հաշվի արժույթով
        /// </summary>
        public double AmountInDebCurrency { get; set; }

        /// <summary>
        /// Փոխանցման աղբյուր
        /// </summary>
        public string TransferSource { get; set; }

        /// <summary>
        // Արագ համակարգերով ÷áË³ÝóáõÙ
        /// </summary>
        public byte InstantMoneyTransfer { get; set; }

        public DateTime? CashOperationDate { get; set; }

        public string SenderReferance { get; set; }

        public byte IsCallCenter { get; set; }

        public string AddTableName { get; set; }

        public int UnicNumber { get; set; }

        /// <summary>
        ///Գրացող  
        /// </summary>
        public string RegisteredUserName { get; set; }

        public double RateSell { get; set; }
        public double RateBuy { get; set; }

        public long PoliceResponseDetailsID { get; set; }

        /// <summary>
        ///Հեռացված 
        /// </summary>
        public byte Deleted { get; set; }

        public uint ListCount { get; set; }

        public int DocflowConfirmationId { get; set; }

        public byte AcbaTransfer { get; set; }

        public string VOCode { get; set; }

        public ulong TransactionGroupNumber { get; set; }

        public ulong CashTransactionGroupNumber { get; set; }

        public string Transit { get; set; }

        public string ReceiverSwift { get; set; }

        /// <summary>
        /// ԲԻԿ (9 նիշ)
        /// </summary>
        public string BIK { get; set; }

        /// <summary>
        /// Թղթակցային հաշիվ (20 նիշ)
        /// </summary>
        public string CorrAccount { get; set; }

        /// <summary>
        /// ԿՊՊ (9 նիշ)
        /// </summary>
        public string KPP { get; set; }

        /// <summary>
        /// Ստացողի ԻՆՆ (10 կամ 12 նիշ)
        /// </summary>
        public string INN { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR1 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR2 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR3 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR4 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR5 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR6 { get; set; }

        /// <summary>
        /// վարկային/կրեդիտ կոդը
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        ///վարկառուի անվանումը
        /// </summary>
        public string Borrower { get; set; }

        /// <summary>
        /// վարկի մարման տեսակի նկարագրություն
        /// </summary>
        public string MatureTypeDescription { get; set; }


        /// <summary>
        ///Ձևակերպող
        /// </summary>
        public string ConfirmationUserName { get; set; }

        /// <summary>
        /// Բանկի Fedwire Routing կոդ
        /// </summary>
        public string FedwireRoutingCode { get; set; }

        /// <summary>
        /// Բանկի Fedwire Routing կոդ
        /// </summary>
        public int CallRegSetNumber { get; set; }

        /// <summary>
        /// Դրամական Փոխանցման Օպերատորի գործակալի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }

        /// <summary>
        /// Փոխանցման հայտի ունիկալ համար (doc_id, transfers_by_call_id, ...)
        /// </summary>
        public double AddTableUnicNumber { get; set; }

        /// <summary>
        /// ARUS արագ դրամական փոխանցման համակարգով կատարված փոխանցման կարգավիճակ(ներ)ի փոփոխման պատմություն
        /// </summary>
        public RemittanceChangeHistory RemittanceChangeHistory { get; set; }

        /// <summary>
        /// Swift փոխանցման ունիկալ համար
        /// </summary>
        public string UETR { get; set; }

        /// <summary>
        /// Tbl_HB_documents-ում հայտի source_type
        /// </summary>
        public short HBSourceType { get; set; }

        public Transfer()
        {
            CreditAccount = new Account();
            DebitAccount = new Account();
            DebForTransferPayment = new Account();
        }


        public void Get(ACBAServiceReference.User user = null)
        {
            TransferDB.Get(this, user);

            //ARUS
            if (TransferSystem == 23)
            {
                RemittanceChangeHistory = new RemittanceChangeHistory();
                RemittanceChangeHistory.TransferId = this.Id;
                RemittanceChangeHistory.GetRemittanceChangeHistory();
            }

        }

        public void GetApprovedTransfer()
        {
            TransferDB.GetApprovedTransfer(this);

        }


        ///// <summary>
        ///// Վերադարձնում է տվյալ փոխանցմանը կցված փաստաթուղթը (առանց scan-ի)
        ///// </summary>
        ///// <param name="orderId">Փոխանցման ունիկալ համար</param>
        ///// <returns></returns>
        //public static string GetTransferDebitAccountNumber(ulong Id)
        //{
        //    return TransferDB.GetTransferDebitAccountNumber(Id);
        //}



        /// <summary>
        /// Վերադարձնում է տվյալ փոխանցմանը կցված փաստաթուղթը (առանց scan-ի)
        /// </summary>
        /// <param name="orderId">Փոխանցման ունիկալ համար</param>
        /// <returns></returns>
        public static OrderAttachment GetTransferAttachmentInfo(long Id)
        {
            return TransferDB.GetTransferAttachmentInfo(Id);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ փոխանցմանը կցված փաստաթղթի սկանը
        /// </summary>
        /// <param name="attachmentId">Սկանի ունիկալ համար</param>
        /// <returns></returns>
        public static OrderAttachment GetTransferAttachment(ulong attachmentId)
        {
            return TransferDB.GetTransferAttachment(attachmentId);
        }

        public List<int> GetTransferCriminalLogId()
        {
            return TransferDB.GetTransferCriminalLogId(this);

        }



        public ActionResult UpdateTransferVerifiedStatus(int userId, int verified)
        {
            ActionResult result = new ActionResult();

            if (this.ConfirmationDate != null)
            {
                result.ResultCode = ResultCode.ValidationError;
                //Նշված տողի կարգավիճակը չի համապատասխանում տվյալ գործողությանը
                result.Errors.Add(new ActionError(813));
            }
            else
            {
                TransferDB.UpdateTransferVerifiedStatus(userId, verified, this.Id);
                result.ResultCode = ResultCode.Normal;
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }
        public ulong GetTransferIdFromTransferByCallId(ulong transferByCallID)
        {
            return TransferDB.GetTransferIdFromTransferByCallId(transferByCallID);
        }

        public ulong GetTransferIdByDocId(long docID) => TransferDB.GetTransferIdByDocId(docID);
    }
}
