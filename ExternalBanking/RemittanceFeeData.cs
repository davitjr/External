using ExternalBanking.ARUSDataService;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class RemittanceFeeData
    {
        /// <summary>
        /// Փոխանցման միջնորդավճար (#############.##)
        /// </summary>
        public decimal RemittanceFee { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար` ՀՀ դրամի փոխարկված (#############.##)
        /// </summary>
        public decimal AMDFee { get; set; }

        /// <summary>
        /// ԿԲ—ի կողմից հայտարարված փոխարժեք՝ միջնորդավճարը ՀՀ դրամի փոխարկելու համար  (######.######)
        /// </summary>
        public decimal SettlementExchangeRate { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի միջնորդավճարի արժույթի կոդ
        /// </summary>
        public string SendingFeeCurrencyCode { get; set; }

        /// <summary>
        /// Ուղարկող Agent-ի միջնորդավճար (#############.##)
        /// </summary>
        public decimal SendingFee { get; set; }


        //**********************Հետևյալ դաշտերը տրվում են նաև որպես մուտքային պարամետր
        /// <summary>
        /// ARUS-ի կոդ կամ հատուկ MTO(Դրամական Փոխանցման Օպերատոր)-ի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }


        /// <summary>
        /// Փոխանցման ստացման տեսակի կոդ
        /// </summary>
        public string PayoutDeliveryCode { get; set; }

        /// <summary>
        /// Արժույթի կոդ (3 նիշ, ISO)
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի կոդ (3 նիշ, ISO)
        /// </summary>
        public string SendingCountryCode { get; set; }

        /// <summary>
        /// Ստացողի երկրի կոդ (3 նիշ, ISO)
        /// </summary>
        public string BeneficiaryCountryCode { get; set; }

        /// <summary>
        /// Փոխանցման գումար (#############.##)
        /// </summary>
        public decimal PrincipalAmount { get; set; }

        /// <summary>
        /// Promo կոդ 
        /// </summary>
        public string PromotionCode { get; set; }

        /// <summary>
        /// Փոխանցման ստացումն իրականացնող Agent-ի կոդ 
        /// </summary>
        public string PayOutAgentCode { get; set; }

        //*****************************************************************************************

        //****************ACBA-ի կողմից ավելացված դաշտեր****************************************

        /// <summary>
        /// Ուղարկող Agent-ի միջնորդավճար (#############.##)` արժույթով
        /// </summary>
        public decimal SendingFeeInCurrency { get; set; }

        //****************************************************************************************

        /// <summary>
        ///Պատասխանի կոդ
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        ///Պատասխանի կոդի նկարագրություն
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման միջնորդավճարը ARUS համակարգում
        /// </summary>
        /// <param name="feeInput"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static RemittanceFeeDataRequestResponse GetRemittanceFeeData(RemittanceFeeInput feeInput, string authorizedUserSessionToken, string userName, string clientIP)
        {
            RemittanceFeeDataRequestResponse remittanceFeeDataRequestResponse = new RemittanceFeeDataRequestResponse();
            remittanceFeeDataRequestResponse.ActionResult = new ActionResult();
            RemittanceFeeData remittanceFeeData = new RemittanceFeeData();
            string transactionCode = "";
            ARUSDataService.RemittanceFeeRequestResponse result = new ARUSDataService.RemittanceFeeRequestResponse();

            ARUSDataService.RemittanceFeeInformationInput ARUSFeeInput = new RemittanceFeeInformationInput();

            feeInput.SendingCountryCode = "ARM";

            ARUSHelper.ConvertObject(feeInput, ref ARUSFeeInput);

            //Մուտքային դաշտերի ստուգումներ
            List<ActionError> errors = Validation.ValidateRemittanceFeeInformation(ARUSFeeInput);
            if (errors != null && errors.Count > 0)
            {
                remittanceFeeDataRequestResponse.ActionResult.Errors = new List<ActionError>();
                remittanceFeeDataRequestResponse.ActionResult.Errors.AddRange(errors);
                remittanceFeeDataRequestResponse.ActionResult.ResultCode = ResultCode.ValidationError;

                return remittanceFeeDataRequestResponse;
            }


            //ARUS հարցում
            ARUSHelper.Use(client =>
            {
                result = client.GetRemittanceFee(ARUSFeeInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
            }, authorizedUserSessionToken, userName, clientIP);

            //ARUS համակարգից ստացվել է սխալի հաղորդագրություն
            if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
            {
                remittanceFeeData = null;

                remittanceFeeDataRequestResponse.ActionResult = ARUSHelper.ConvertARUSActionResultToXBActionResult(result.ActionResult);
            }
            //ARUS համակարգին դիմելու պահին տեղի է ունեցել սխալ
            else if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                remittanceFeeDataRequestResponse.ActionResult = ARUSHelper.ConvertARUSActionResultToXBActionResult(result.ActionResult);
                remittanceFeeData = null;
            }
            //ARUS համակարգի հարցումն ունեցել է հաջող ավարտ
            else if (result.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                RemittanceFeeInformationOutput transactionDetails = result.RemittanceFeeInformationOutput;
                ARUSHelper.ConvertObject(transactionDetails, ref remittanceFeeData);
                if (remittanceFeeData.SendingFeeCurrencyCode == "AMD")
                {
                    remittanceFeeData.SendingFeeInCurrency = Math.Round(remittanceFeeData.SendingFee / remittanceFeeData.SettlementExchangeRate, 2);
                }
                else
                {
                    remittanceFeeData.SendingFeeInCurrency = remittanceFeeData.SendingFee;
                }
                remittanceFeeDataRequestResponse.ActionResult.ResultCode = ResultCode.Normal;
            }

            remittanceFeeDataRequestResponse.RemittanceFeeData = remittanceFeeData;

            return remittanceFeeDataRequestResponse;
        }

        /// <summary>
        /// Վերադարձնում է նշված ուղղությամբ դրամական փոխանցում իրականացնող MTO-ների ցանկը` ներառելով փոխանցման միջնորդավճարները:
        /// </summary>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static List<MTOListAndBestChoiceOutput> GetSTAKMTOListAndBestChoice(MTOListAndBestChoiceInput bestChoice, string authorizedUserSessionToken, string userName, string clientIP)
        {
            List<MTOListAndBestChoiceOutput> MTOList = new List<MTOListAndBestChoiceOutput>();
            string transactionCode = "";
            ARUSHelper.Use(client =>
            {
                MTOList = client.GetMTOListAndBestChoice(bestChoice, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
            }, authorizedUserSessionToken, userName, clientIP);

            return MTOList;
        }
    }
    /// <summary>
    /// ARUS համակարգից ստացված փոխանցման միջնորդավճարի հարցման պատասխան
    /// </summary>
    public class RemittanceFeeDataRequestResponse
    {
        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public RemittanceFeeData RemittanceFeeData { get; set; }

        /// <summary>
        /// Հարցման պատասխանի արդյունք
        /// </summary>
        public ActionResult ActionResult { get; set; }
    }

    public class RemittanceFeeInput
    {
        /// <summary>
        /// ARUS-ի կոդ կամ հատուկ MTO(Դրամական Փոխանցման Օպերատոր)-ի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }


        /// <summary>
        /// Փոխանցման ստացման տեսակի կոդ
        /// </summary>
        public string PayoutDeliveryCode { get; set; }

        /// <summary>
        /// Արժույթի կոդ (3 նիշ, ISO)
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի կոդ (3 նիշ, ISO)
        /// </summary>
        public string SendingCountryCode { get; set; }

        /// <summary>
        /// Ստացողի երկրի կոդ (3 նիշ, ISO)
        /// </summary>
        public string BeneficiaryCountryCode { get; set; }

        /// <summary>
        /// Փոխանցման գումար (#############.##)
        /// </summary>
        public decimal PrincipalAmount { get; set; }

        /// <summary>
        /// Promo կոդ 
        /// </summary>
        public string PromotionCode { get; set; }

        /// <summary>
        /// Փոխանցման ստացումն իրականացնող Agent-ի կոդ 
        /// </summary>
        public string PayOutAgentCode { get; set; }
    }

}
