using ExternalBanking.ARUSDataService;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{
    /// <summary>
    /// ARUS համակարգով ուղարկված/ստացված փոխանցման տվյալներ
    /// </summary>
    public class RemittanceDetails
    {
        /// <summary>
        /// Փոխանցման MTO գործակալի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }

        /// <summary>
        /// Փոխանցման հսկիչ համար
        /// </summary>
        public string URN { get; set; }

        /// <summary>
        /// Փոխանցման կարգավիճակի կոդ
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Փոխանցման կարգավիճակի անվանում
        /// </summary>
        public string StatusCodeName { get; set; }

        /// <summary>
        /// Գործողության տեսակի կոդ (10: Send Money, 20: Payout)
        /// </summary>
        public string SendPayoutDivCode { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի տեսակի կոդ
        /// </summary>
        public string SendAgentTypeCode { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի կոդ
        /// </summary>
        public string SendAgentCode { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի անվանում
        /// </summary>
        public string SendAgentName { get; set; }

        /// <summary>
        /// Ստացող Agent-ի տեսակի կոդ
        /// </summary>
        public string BeneAgentTypeCode { get; set; }

        /// <summary>
        /// Ստացող Agent-ի կոդ
        /// </summary>
        public string BeneAgentCode { get; set; }

        /// <summary>
        /// Ստացող Agent-ի անվանում
        /// </summary>
        public string BeneAgentName { get; set; }

        /// <summary>
        /// Փոխանցման ստացման տեսակի կոդ
        /// </summary>
        public string DeliveryTypeCode { get; set; }

        /// <summary>
        /// Փոխանցման ստացման տեսակի անվանում
        /// </summary>
        public string DeliveryTypeName { get; set; }

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// ԿԲ—ի կողմից հայտարարված փոխարժեք՝ միջնորդավճարը ՀՀ դրամի փոխարկելու համար  (######.######)
        /// </summary>
        public decimal SettlementExchangeRate { get; set; }

        /// <summary>
        /// Արժույթի կոդ
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Փոխանցման գումար (#############.##)
        /// </summary>
        public decimal PrincipalAmount { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար (#############.##)
        /// </summary>
        public decimal RemittanceFee { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար՝ ՀՀ դրամի փոխարկված (#############.##)
        /// </summary>
        public decimal AMDFee { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի միջնորդավճար (#############.##)
        /// </summary>
        public decimal SendingFee { get; set; }

        /// <summary>
        /// Փոխանցման գումար՝ ՀՀ դրամի փոխարկված (#############.##)
        /// </summary>
        public decimal AMDConvertAmount { get; set; }

        /// <summary>
        /// Փոխանցման նպատակ
        /// </summary>
        public string PurposeRemittanceCode { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն անգլերենով
        /// </summary>
        public string SenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն անգլերենով
        /// </summary>
        public string SenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն անգլերենով
        /// </summary>
        public string SenderFirstName { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderFirstName { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի կոդ
        /// </summary>
        public string SenderCountryCode { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի անվանում
        /// </summary>
        public string SenderCountryName { get; set; }

        /// <summary>
        /// Ուղարկողի մարզի կոդ
        /// </summary>
        public string SenderStateCode { get; set; }

        /// <summary>
        /// Ուղարկողի մարզի անվանում
        /// </summary>
        public string SenderStateName { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաքի կոդ
        /// </summary>
        public string SenderCityCode { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաքի անվանում
        /// </summary>
        public string SenderCityName { get; set; }

        /// <summary>
        /// Ուղարկողի հասցե
        /// </summary>
        public string SenderAddressName { get; set; }

        /// <summary>
        /// Ուղարկողի փոստային կոդ
        /// </summary>
        public string SenderZipCode { get; set; }

        /// <summary>
        /// Ուղարկողի զբաղվածության անվանում
        /// </summary>
        public string SenderOccupationName { get; set; }


        /// <summary>
        /// Ուղարկողի փաստաթղթի տեսակի կոդ
        /// </summary>
        public string SenderDocumentTypeCode { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի տեսակի անվանում
        /// </summary>
        public string SenderDocumentTypeName { get; set; }

        /// <summary>
        /// Ուղարկողի ID համարի տրման ամսաթիվ
        /// </summary>
        public string SenderIssueDate { get; set; }

        /// <summary>
        /// Ամսաթիվ՝ ցույց տվող, թե մինչև երբ է ուժի մեջ Ուղարկողի ID համարը 
        /// </summary>
        public string SenderExpirationDate { get; set; }

        /// <summary>
        /// Երկրի կոդը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCountryCode { get; set; }

        /// <summary>
        /// Երկրի անվանումը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCountryName { get; set; }

        /// <summary>
        /// Քաղաքի կոդը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCityCode { get; set; }

        /// <summary>
        /// Քաղաքի անվանումը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCityName { get; set; }

        /// <summary>
        ///Ուղարկողի ID համարը
        /// </summary>
        public string SenderIssueIDNo { get; set; }

        /// <summary>
        ///Ուղարկողի ծննդյան ամսաթիվ
        /// </summary>
        public string SenderBirthDate { get; set; }

        /// <summary>
        ///Ուղարկողի ծննդավայր
        /// </summary>
        public string SenderBirthPlaceName { get; set; }

        /// <summary>
        ///Ուղարկողի սեռի կոդ
        /// </summary>
        public string SenderSexCode { get; set; }

        /// <summary>
        ///Ուղարկողի էլեկտրոնային հասցե
        /// </summary>
        public string SenderEMailName { get; set; }

        /// <summary>
        ///Ուղարկողի հեռախոսահամար
        /// </summary>
        public string SenderPhoneNo { get; set; }

        /// <summary>
        ///Ուղարկողի բջջային հեռախոսահամար
        /// </summary>
        public string SenderMobileNo { get; set; }

        /// <summary>
        ///Ստուգիչ հարց
        /// </summary>
        public string ControlQuestionName { get; set; }

        /// <summary>
        ///Ստուգիչ հարցի պատասխան
        /// </summary>
        public string ControlAnswerName { get; set; }

        /// <summary>
        ///Ստացողի ազգանուն
        /// </summary>
        public string NATBeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն
        /// </summary>
        public string NATBeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն
        /// </summary>
        public string NATBeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի ազգանուն անգլերենով
        /// </summary>
        public string BeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն անգլերենով
        /// </summary>
        public string BeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն անգլերենով
        /// </summary>
        public string BeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի երկրի կոդ
        /// </summary>
        public string BeneficiaryCountryCode { get; set; }

        /// <summary>
        ///Ստացողի երկրի անվանում
        /// </summary>
        public string BeneficiaryCountryName { get; set; }

        /// <summary>
        ///Ստացողի մարզի կոդ
        /// </summary>
        public string BeneficiaryStateCode { get; set; }

        /// <summary>
        ///Ստացողի մարզի անվանում
        /// </summary>
        public string BeneficiaryStateName { get; set; }

        /// <summary>
        ///Ստացողի քաղաքի կոդ
        /// </summary>
        public string BeneficiaryCityCode { get; set; }

        /// <summary>
        ///Ստացողի քաղաքի անվանում
        /// </summary>
        public string BeneficiaryCityName { get; set; }

        /// <summary>
        ///Ստացողի հասցե
        /// </summary>
        public string BeneficiaryAddressName { get; set; }

        /// <summary>
        ///Ստացողի փոստային կոդ
        /// </summary>
        public string BeneficiaryZipCode { get; set; }

        /// <summary>
        ///Ստացողի զբաղվածություն
        /// </summary>
        public string BeneficiaryOccupationName { get; set; }

        /// <summary>
        ///Ստացողի փաստաթղթի տեսակի կոդ
        /// </summary>
        public string BeneficiaryDocmentTypeCode { get; set; }

        /// <summary>
        ///Ստացողի փաստաթղթի տեսակի անվանում
        /// </summary>
        public string BeneficiaryDocymentTypeName { get; set; }

        /// <summary>
        ///Ստացողի ID համարի տրման ամսաթիվ
        /// </summary>
        public string BeneficiaryIssueDate { get; set; }

        /// <summary>
        ///Ամսաթիվ՝ ցույց տվող, թե մինչև երբ է ուժի մեջ ստացողի ID համարը
        /// </summary>
        public string BeneficiaryExpirationDate { get; set; }

        /// <summary>
        ///Երկրի կոդը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCountryCode { get; set; }

        /// <summary>
        ///Երկրի անվանումը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCountryName { get; set; }

        /// <summary>
        ///Քաղաքի կոդը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCityCode { get; set; }

        /// <summary>
        ///Քաղաքի անվանումը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCityName { get; set; }

        /// <summary>
        ///Ստացողի ID համարը
        /// </summary>
        public string BeneficiaryIssueIDNo { get; set; }

        /// <summary>
        ///Ստացողի ծննդյան ամսաթիվ
        /// </summary>
        public string BeneficiaryBirthDate { get; set; }

        /// <summary>
        ///Ստացողի ծննդավայր
        /// </summary>
        public string BeneficiaryBirthPlaceName { get; set; }

        /// <summary>
        ///Ստացողի սեռի կոդ
        /// </summary>
        public string BeneficiarySexCode { get; set; }

        /// <summary>
        ///Ստացողի էլեկտրոնային հասցե
        /// </summary>
        public string BeneficiaryEMailName { get; set; }

        /// <summary>
        ///Ստացողի հեռախոսահամար
        /// </summary>
        public string BeneficiaryPhoneNo { get; set; }

        /// <summary>
        ///Ստացողի բջջային հեռախոսահամար
        /// </summary>
        public string BeneficiaryMobileNo { get; set; }

        /// <summary>
        /// Ստացողի բնակարան
        /// </summary>
        public string BeneficiaryAddressAppartment { get; set; }

        /// <summary>
        /// Ստացողի տուն
        /// </summary>
        public string BeneficiaryAddressHouse { get; set; }

        /// <summary>
        /// Ստացողի քաղաքացիության երկրի կոդ
        /// </summary>
        public string BeneficiaryCitizenship { get; set; }

        /// <summary>
        /// Ստացողի փաստաթղթի սերիա
        /// </summary>
        public string BeneficiaryIssueIDSeries { get; set; }

        /// <summary>
        /// Ստացողի փաստաթղթի թողարկող
        /// </summary>
        public string BeneficiaryIssuer { get; set; }

        /// <summary>
        /// Ստացողի փաստաթղթի մարզի կոդ
        /// </summary>
        public string BeneficiaryIssueStateCode { get; set; }

        /// <summary>
        /// Ստացողի փաստաթղթի մարզի անվանում
        /// </summary>
        public string BeneficiaryIssueStateName { get; set; }

        /// <summary>
        /// Ստացողի ռեզիդենտության կոդ (Այո/Ոչ տեղեկատուի կոդ)
        /// </summary>
        public string BeneficiaryResidencyCode { get; set; }

        /// <summary>
        /// Դրամական փոխանցման օպերատորի Ստացողի Գործակալի կոդ
        /// </summary>
        public string PayoutAgentCode { get; set; }

        /// <summary>
        /// Դրամական Փոխանցման Օպերատորի(MTO) Ստացողի Գործակալի անվանում
        /// </summary>
        public string PayoutAgentName { get; set; }

        /// <summary>
        /// Ուղարկողի բնակարան
        /// </summary>
        public string SenderAddressAppartment { get; set; }

        /// <summary>
        /// Ուղարկողի տուն
        /// </summary>
        public string SenderAddressHouse { get; set; }

        /// <summary>
        /// Ուղարկողի ծննդյան երկրի կոդ
        /// </summary>
        public string SenderBirthCountryCode { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաքացիության երկրի կոդ
        /// </summary>
        public string SenderCitizenship { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի սերիա
        /// </summary>
        public string SenderIssueIDSeries { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի թողարկող
        /// </summary>
        public string SenderIssuer { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի թողարկողի մարզի կոդ
        /// </summary>
        public string SenderIssueStateCode { get; set; }

        /// <summary>
        /// Ուղարկող գործակալի երկրի կոդ
        /// </summary>
        public string SendingAgentCountryCode { get; set; }

        /// <summary>
        /// MTO OwnerKey՝ միայն ուղարկող գործակալի համար
        /// </summary>
        public string OwnerKey { get; set; }

        /// <summary>
        /// Փոխանցման ուղարկման(նպատակակետ հանդիսացող) երկրի կոդ
        /// </summary>
        public string DestinationCountryCode { get; set; }

        /// <summary>
        /// Փոխանցման նշում
        /// </summary>
        public string DestinationText { get; set; }

        /// <summary>
        /// Ստացող գործակալի միջնորդավճար վճարման ժամանակ
        /// </summary>
        public double BeneficiaryFee { get; set; }

        /// <summary>
        /// Ստացողի ֆիսկալ կոդ
        /// </summary>
        public string BeneficiaryFiscalCode { get; set; }

        /// <summary>
        /// Ուղարկողի ռեզիդենտության կոդ
        /// </summary>
        public string SenderResidencyCode { get; set; }

        //**************************************************************************************************************
        //ACBA-ի կողմից ավելացրած դաշտեր

        /// <summary>
        /// Դրամական փոխանցման օպերատորի անվանում
        /// </summary>
        public string MTOAgentName { get; set; }

        /// <summary>
        /// Գործողության տեսակի անվանում
        /// </summary>
        public string SendPayoutDivName { get; set; }

        /// <summary>
        /// Ուղարկող գործակալի տեսակի անվանում
        /// </summary>
        public string SendAgentTypeName { get; set; }

        /// <summary>
        /// Ստացող գործակալի տեսակի անվանում
        /// </summary>
        public string BeneAgentTypeName { get; set; }

        /// <summary>
        /// Փոխանցման նպատակի անվանում
        /// </summary>
        public string PurposeRemittanceName { get; set; }

        /// <summary>
        /// Ուղարկողի սեռի անվանում
        /// </summary>
        public string SenderSexName { get; set; }

        /// <summary>
        ///Ստացողի սեռի անվանում
        /// </summary>
        public string BeneficiarySexName { get; set; }

        /// <summary>
        /// Ստացողի քաղաքացիության երկրի անվանում
        /// </summary>
        public string BeneficiaryCitizenshipName { get; set; }

        /// <summary>
        /// Ուղարկող գործակալի երկրի անվանում
        /// </summary>
        public string SendingAgentCountryName { get; set; }

        /// <summary>
        /// Փոխանցման ուղարկման(նպատակակետ հանդիսացող) երկրի անվանում
        /// </summary>
        public string DestinationCountryName { get; set; }

        /// <summary>
        /// Ուղարկողի ռեզիդենտության անվանում (Այո / Ոչ)
        /// </summary>
        public string SenderResidencyName { get; set; }


        /// <summary>
        /// Ստացողի ռեզիդենտության անվանում (Այո / Ոչ)
        /// </summary>
        public string BeneficiaryResidencyName { get; set; }

        //**************************************************************************************************************

        /// <summary>
        ///Պատասխանի կոդ
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        ///Պատասխանի կոդի նկարագրություն
        /// </summary>
        public string ResponseMessage { get; set; }



        /// <summary>
        /// Վերադարձնում է արագ փոխանցման տվյալները՝ ըստ փոխանցման հսկիչ համարի
        /// </summary>
        /// <param name="URN"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static RemittanceDetailsRequestResponse GetRemittanceDetailsByURN(string URN, string authorizedUserSessionToken, string userName, string clientIP)
        {
            RemittanceDetailsRequestResponse remittanceDetailsRequestResponse = new RemittanceDetailsRequestResponse();
            remittanceDetailsRequestResponse.ActionResult = new ActionResult();
            RemittanceDetails remittanceDetails = new RemittanceDetails();
            string transactionCode = "";
            ARUSDataService.TransactionDetailsRequestResponse result = new ARUSDataService.TransactionDetailsRequestResponse();

            //Մուտքային դաշտերի ստուգումներ
            List<ActionError> errors = Validation.ValidateURN(URN);
            if (errors != null && errors.Count > 0)
            {
                remittanceDetailsRequestResponse.ActionResult.Errors = new List<ActionError>();
                remittanceDetailsRequestResponse.ActionResult.Errors.AddRange(errors);
                remittanceDetailsRequestResponse.ActionResult.ResultCode = ResultCode.ValidationError;


                return remittanceDetailsRequestResponse;
            }

            //ARUS հարցում
            ARUSHelper.Use(client =>
            {
                result = client.GetTransactionDetailsByURN(URN, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
            }, authorizedUserSessionToken, userName, clientIP);

            //ARUS համակարգից ստացվել է սխալի հաղորդագրություն
            if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
            {
                remittanceDetails = null;

                remittanceDetailsRequestResponse.ActionResult = ARUSHelper.ConvertARUSActionResultToXBActionResult(result.ActionResult);
            }
            //ARUS համակարգին դիմելու պահին տեղի է ունեցել սխալ
            else if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                remittanceDetailsRequestResponse.ActionResult = ARUSHelper.ConvertARUSActionResultToXBActionResult(result.ActionResult);
                remittanceDetails = null;
            }
            //ARUS համակարգի հարցումն ունեցել է հաջող ավարտ
            else if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                TransactionDetails transactionDetails = result.TransactionDetails;
                ARUSHelper.ConvertObject(transactionDetails, ref remittanceDetails);
                remittanceDetails.ConvertDates();
                remittanceDetails.GetCodeNames(authorizedUserSessionToken, userName, clientIP);
                remittanceDetailsRequestResponse.ActionResult.ResultCode = ResultCode.Normal;
            }

            remittanceDetailsRequestResponse.RemittanceDetails = remittanceDetails;

            return remittanceDetailsRequestResponse;
        }

        /// <summary>
        /// ARUS համակարգին համապատասխան ամսաթիվը (YYYYMMDD ֆորմատով) վերափոխում է dd/MM/yyyy ֆորմատով ամսաթվի
        /// </summary>
        void ConvertDates()
        {
            SenderIssueDate = ARUSHelper.ConvertARUSDateStringToString(SenderIssueDate);
            SenderExpirationDate = ARUSHelper.ConvertARUSDateStringToString(SenderExpirationDate);
            SenderBirthDate = ARUSHelper.ConvertARUSDateStringToString(SenderBirthDate);
            BeneficiaryIssueDate = ARUSHelper.ConvertARUSDateStringToString(BeneficiaryIssueDate);
            BeneficiaryExpirationDate = ARUSHelper.ConvertARUSDateStringToString(BeneficiaryExpirationDate);
            BeneficiaryBirthDate = ARUSHelper.ConvertARUSDateStringToString(BeneficiaryBirthDate);
        }

        /// <summary>
        /// Ստանում է դաշտերի կոդերին համապատասխան անվանումները
        /// </summary>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        void GetCodeNames(string authorizedUserSessionToken, string userName, string clientIP)
        {
            DataTable MTOList = ARUSInfo.GetMTOList(authorizedUserSessionToken, userName, clientIP);


            DataTable countries = ARUSInfo.GetCountriesByMTO(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
            DataTable sexes = ARUSInfo.GetSexes(authorizedUserSessionToken, userName, clientIP);

            //PurposeRemittanceCode = "RW";   //Հանել հետագայում



            if (!String.IsNullOrEmpty(PurposeRemittanceCode))
            {
                DataTable purposes = ARUSInfo.GetRemittancePurposes(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                PurposeRemittanceName = purposes.Select("code = '" + PurposeRemittanceCode + "'")[0]["name"].ToString();
            }

            if (!String.IsNullOrEmpty(SendPayoutDivCode))
            {
                DataTable divisions = ARUSInfo.GetSendPayoutDivisionCodes(authorizedUserSessionToken, userName, clientIP);
                SendPayoutDivName = divisions.Select("code = '" + SendPayoutDivCode + "'")[0]["name"].ToString();

            }

            if (!String.IsNullOrEmpty(MTOAgentCode))
            {
                MTOAgentName = MTOList.Select("code = '" + MTOAgentCode + "'")[0]["name"].ToString();
            }

            if (!String.IsNullOrEmpty(BeneficiaryCitizenship))
            {
                BeneficiaryCitizenshipName = countries.Select("code = '" + BeneficiaryCitizenship + "'")[0]["name"].ToString();
            }

            //if (!String.IsNullOrEmpty(SendingAgentCountryCode))
            //{
            //    SendingAgentCountryName = countries.Select("code = '" + SendingAgentCountryCode + "'")[0]["name"].ToString();
            //}

            if (!String.IsNullOrEmpty(DestinationCountryCode))
            {
                DestinationCountryName = countries.Select("code = '" + DestinationCountryCode + "'")[0]["name"].ToString();
            }

            if (!String.IsNullOrEmpty(SenderSexCode))
            {
                SenderSexName = sexes.Select("code = '" + SenderSexCode + "'")[0]["name"].ToString();
                SenderSexName = SenderSexName == "Female" ? "Female (Իգական)" : (SenderSexName == "Male" ? "Male (Արական)" : SenderSexName);
            }

            if (!String.IsNullOrEmpty(BeneficiarySexCode))
            {
                BeneficiarySexName = sexes.Select("code = '" + BeneficiarySexCode + "'")[0]["name"].ToString();
                BeneficiarySexName = BeneficiarySexName == "Female" ? "Female (Իգական)" : (BeneficiarySexName == "Male" ? "Male (Արական)" : BeneficiarySexName);
            }


            if (!String.IsNullOrEmpty(SendAgentTypeCode))
            {
                SendAgentTypeName = ARUSInfo.GetAgentTypeNameByCode(SendAgentTypeCode);
            }

            if (!String.IsNullOrEmpty(BeneAgentTypeCode))
            {
                BeneAgentTypeName = ARUSInfo.GetAgentTypeNameByCode(BeneAgentTypeCode);
            }

            if (!String.IsNullOrEmpty(SenderResidencyCode) || !String.IsNullOrEmpty(BeneficiaryResidencyCode))
            {
                DataTable YesNoList = ARUSInfo.GetYesNo(authorizedUserSessionToken, userName, clientIP);
                if (!String.IsNullOrEmpty(SenderResidencyCode))
                {
                    SenderResidencyName = YesNoList.Select("code = '" + SenderResidencyCode + "'")[0]["name"].ToString();
                    SenderResidencyName = SenderResidencyName == "Yes" ? "Yes (Այո) " : (SenderResidencyName == "No" ? "No (Ոչ)" : SenderResidencyName);
                }

                if (!String.IsNullOrEmpty(BeneficiaryResidencyCode))
                {
                    BeneficiaryResidencyName = YesNoList.Select("code = '" + BeneficiaryResidencyCode + "'")[0]["name"].ToString();
                    BeneficiaryResidencyName = BeneficiaryResidencyName == "Yes" ? "Yes (Այո)" : (BeneficiaryResidencyName == "No" ? "No (Ոչ)" : BeneficiaryResidencyName);
                }
            }
        }
    }

    /// <summary>
    /// ARUS համակարգից փոխանցման մանրամասների հարցման պատասխան
    /// </summary>
    public class RemittanceDetailsRequestResponse
    {
        /// <summary>
        /// Փոխանցման մանրամասներ
        /// </summary>
        public RemittanceDetails RemittanceDetails { get; set; }

        /// <summary>
        /// Հարցման պատասխանի արդյունք
        /// </summary>
        public ActionResult ActionResult { get; set; }
    }
}
