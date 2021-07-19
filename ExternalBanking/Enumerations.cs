using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ExternalBanking
{
    public class Enumerations
    {
        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;
            else
                return value.ToString();
        }
    }
    public class DescriptionAttribute : System.Attribute
    {
        private string _value;
        public DescriptionAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

    }
    /// <summary>
    /// Հայտի տեսակներ
    /// </summary>
    public enum OrderType : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Փոխանցում ՀՀ տարածքում
        /// </summary>
        RATransfer = 1,
        /// <summary>
        /// Փոխանակում
        /// </summary>
        Convertation = 2,
        /// <summary>
        /// Միջազգային փոխացում
        /// </summary>
        InternationalTransfer = 3,
        /// <summary>
        /// Ավանդի դադարեցում
        /// </summary>
        DepositTermination = 4,
        /// <summary>
        /// Վարկի մարում
        /// </summary>
        LoanMature = 5,
        /// <summary>
        /// Փոխանցում ռեեստրով
        /// </summary>
        RosterTransfer = 6,
        /// <summary>
        /// Ընթացիկ հաշվի բացում
        /// </summary>
        CurrentAccountOpen = 7,
        /// <summary>
        /// Առևտրային վարկային գծի/օվերդրաֆտի մարում
        /// </summary>
        OverdraftRepayment = 8,
        /// <summary>
        /// Ավանդի ձևակերպում
        /// </summary>
        Deposit = 9,
        /// <summary>
        /// Պարբերական փոխանցում
        /// </summary>
        PeriodicTransfer = 10,
        /// <summary>
        /// Պարբերական փոխանցման դադարեցում
        /// </summary>
        PeriodicTransferStop = 11,
        /// <summary>
        /// Ընթացիկ հաշվի վերաբացում
        /// </summary>
        CurrentAccountReOpen = 12,
        /// <summary>
        /// Ավանդի գրավով վարկի ձևակերպում
        /// </summary>
        CreditSecureDeposit = 13,
        /// <summary>
        /// Ավանդի գրավով վարկային գծի ձևակերպում
        /// </summary>
        CreditLineSecureDeposit = 14,
        /// <summary>
        /// Կոմունալ վճարում
        /// </summary>
        CommunalPayment = 15,
        /// <summary>
        /// Ավանդի փոխարկում
        /// </summary>
        ExchangeDeposit = 16,
        /// <summary>
        /// Հօգուտ 3-րդ անձի ընթացիկ հաշվի բացում
        /// </summary>
        ThirdPersonDeposit = 17,
        /// <summary>
        /// Գործարքից հրաժարում
        /// </summary>
        CancelTransaction = 18,
        /// <summary>
        /// Գործարքի հեռացում
        /// </summary>
        RemoveTransaction = 19,
        /// <summary>
        /// Տեղեկանքի ստացում
        /// </summary>
        ReferenceOrder = 20,
        /// <summary>
        /// Վարկային գծի դադարեցում
        /// </summary>
        CreditLineMature = 21,
        /// <summary>
        /// Չեկային գրքույքի պատվիրում
        /// </summary>
        ChequeBookOrder = 22,
        /// <summary>
        /// Գումարի ստացում
        /// </summary>
        CashOrder = 23,
        /// <summary>
        /// Քաղվածքի էլեկտրոնային ստացում
        /// </summary>
        StatmentByEmailOrder = 24,
        /// <summary>
        /// SWIFT հաղորդագրության պատճենի ստացում
        /// </summary>
        SwiftCopyOrder = 26,
        /// <summary>
        /// Տվյալների խմբագրում
        /// </summary>
        CustomerDataOrder = 27,
        /// <summary>
        /// Համատեղ ընթացիկ հաշվի բացում
        /// </summary>
        JointCurrentAccountOpen = 28,
        /// <summary>
        /// Հաշվի փակում
        /// </summary>
        CurrentAccountClose = 29,
        /// <summary>
        /// Քարտի փակում
        /// </summary>
        CardClosing = 30,
        /// <summary>
        /// Հաշվի տվյալների խմբագրում
        /// </summary>
        AccountDataChange = 50,
        /// <summary>
        /// Կանխիկ մուտք հաշվին
        /// </summary>
        CashDebit = 51,
        /// <summary>
        /// Կանխիկ ելք հաշվից
        /// </summary>
        CashCredit = 52,
        /// <summary>
        /// Կանխիկ կոնվերտացիա
        /// </summary>
        CashConvertation = 53,
        /// <summary>
        /// Կանխիկ մուտք հաշվին կոնվերտացիայով
        /// </summary>
        CashDebitConvertation = 54,
        /// <summary>
        /// Կանխիկ ելք հաշվից կոնվերտացիայով
        /// </summary>
        CashCreditConvertation = 55,
        /// <summary>
        /// Կանխիկ ՀՀ տարածքում
        /// </summary>
        CashForRATransfer = 56,
        /// <summary>
        /// Հաշիվների սպասրկման վարձի գծով  պարտավորության մարում
        /// </summary>
        AccountServicePayment = 58,
        /// <summary>
        /// HB սպասարկման վարձի գծով  պարտավորության մարում
        /// </summary>
        HBServicePayment = 59,
        /// <summary>
        /// Կանխիկ կոմունալի վճարում
        /// </summary>
        CashCommunalPayment = 60,
        /// <summary>
        /// Հաշիվների սպասրկման վարձի գծով  պարտավորության մարում խնդրահարույց վարկերի տարանցիկ հաշվից
        /// </summary>
        AccountServicePaymentXnd = 61,
        /// <summary>
        /// HB սպասարկման վարձի գծով  պարտավորության մարում խնդրահարույց վարկերի տարանցիկ հաշվից
        /// </summary>
        HBServicePaymentXnd = 62,
        /// <summary>
        /// Փոխանցում տարանցիկ հաշվին
        /// </summary>
        TransitPaymentOrder = 63,
        /// <summary>
        /// Միջազգային փոխանցում տարանցիկ հաշվից 
        /// </summary>
        CashInternationalTransfer = 64,
        /// <summary>
        /// Բանկի ներսում փոխարկում
        /// </summary>
        InBankConvertation = 65,
        /// <summary>
        /// Հաշվի սառեցում
        /// </summary>
        AccountFreeze = 66,
        /// <summary>
        /// Հաշվի ապասառեցում
        /// </summary>
        AccountUnfreeze = 67,
        /// <summary>
        ///Կանխիկացում POSտերմինալով 
        /// </summary>
        CashPosPayment = 68,
        /// <summary>
        /// ՀԲ ակտիվացման/փոփոխման հայտ
        /// </summary>
        HBActivation = 69,
        /// <summary>
        /// Քարտային հաշվի չվճարված դրական տոկոսագումարի վճարում
        /// </summary>
        CardUnpayedPercentPayment = 70,
        /// <summary>
        /// Հաճախորդին տրամադրվող ծառայությունների միջնորդավճարի գանձում
        /// </summary>
        FeeForServiceProvided = 71,
        /// <summary>
        /// Հաճախորդին տրամադրվող ծառայությունների միջնորդավճարի գանձում կանխիկ
        /// </summary>
        CashFeeForServiceProvided = 72,
        /// <summary>
        /// Վարկի ակտիվացում
        /// </summary>
        LoanActivation = 73,
        /// <summary>
        /// Վարկային գծի ակտիվացում
        /// </summary>
        CreditLineActivation = 74,
        /// <summary>
        /// Չեկային գրքույքի ստացում
        /// </summary>
        ChequeBookReceiveOrder = 75,
        /// <summary>
        /// Արագ փոխանցման հայտ
        /// </summary>
        FastTransferPaymentOrder = 76,
        /// <summary>
        /// Լիազորագրի մուտքագրման հայտ
        /// </summary>
        CredentialOrder = 77,
        /// <summary>
        /// Քարտային վարկային գծի մարում
        /// </summary>
        CardCreditLineRepayment = 78,
        /// <summary>
        /// Արագ համակարգերով ստացված փոխանցումներ
        /// </summary>
        ReceivedFastTransferPaymentOrder = 79,
        /// <summary>
        /// Քարտի վերաթողարկում
        /// </summary>
        CardReRelease = 25,

        /// <summary>
        /// Տարանցիկ հաշվին մուտք փոխարկումով
        /// </summary>
        CashTransitCurrencyExchangeOrder = 80,

        /// <summary>
        /// Տարանցիկ հաշվից կանխիկ ելք փոխարկումով 
        /// </summary>
        TransitCashOutCurrencyExchangeOrder = 81,

        /// <summary>
        /// Տարանցիկ հաշվից անկանխիկ ելք փոխարկումով 
        /// </summary>
        TransitNonCashOutCurrencyExchangeOrder = 82,

        /// <summary>
        /// Քարտի սպասարկման միջնորդավճարի գանձում
        /// </summary>
        CardServiceFeePayment = 83,
        /// <summary>
        /// Տարանցիկ հաշվից կանխիկ ելք  
        /// </summary>
        TransitCashOut = 84,

        /// <summary>
        /// Տարանցիկ հաշվից անկանխիկ ելք  
        /// </summary>
        TransitNonCashOut = 85,
        /// <summary>
        /// Միջմասնաճյուղային կանխիկ փոխանցում
        /// </summary>
        InterBankTransferCash = 86,

        /// <summary>
        /// Սպասարկան վարձի գանձման նշում
        /// </summary>
        ServicePaymentNote = 87,
        /// <summary>
        /// Սպասարկան վարձի գանձման նշման հեռացում
        /// </summary>
        DeleteServicePaymentNote = 88,

        /// <summary>
        /// Կենսաթոշակ ստանալու դիմում
        /// </summary>
        PensionApplicationOrder = 89,

        /// <summary>
        /// Միջմասնաճյուղային անկանխիկ փոխանցում
        /// </summary>
        InterBankTransferNonCash = 90,
        /// <summary>
        /// Կենսաթոշակ ստանալու դիմումի ակտիվացում
        /// </summary>
        PensionApplicationActivationOrder = 91,
        /// <summary>
        /// Կենսաթոշակ ստանալու դիմումի հեռացում
        /// </summary>
        PensionApplicationActivationRemovalOrder = 92,
        /// <summary>
        /// Կենսաթոշակի ստացման դադարեցում
        /// </summary>
        PensionApplicationTerminationOrder = 93,
        /// <summary>
        /// Կենսաթոշակի ստացման վերագրանցում
        /// </summary>
        PensionApplicationOverwriteOrder = 94,
        /// <summary>
        /// Կանխիկ մուտք ռեեստրով
        /// </summary>
        ReestrTransferOrder = 95,
        /// <summary>
        /// Հեռախոսազանգով փոխանցման համաձայնագրի մուտքագրում
        /// </summary>
        TransferCallContractOrder = 96,
        /// <summary>
        /// Հեռախոսազանգով փոխանցման համաձայնագրի դադարեցում
        /// </summary>
        TransferCallContractTerminationOrder = 97,
        /// <summary>
        /// Պահատուփի դիմում
        /// </summary>
        DepositCaseOrder = 98,
        /// <summary>
        /// Պահատուփի ակտիվացման հայտ
        /// </summary>
        DepositCaseActivationOrder = 99,

        /// <summary>
        /// Պահատուփի հեռացման հայտ
        /// </summary>
        DepositCaseDeleteOrder = 100,

        /// <summary>
        /// Պահատուփի դադարեցման հայտ
        /// </summary>
        DepositCaseTermiationOrder = 101,

        /// <summary>
        /// Փոխանցում արագ համակարգ»րով հաճախորդի հաշվից
        /// </summary>
        FastTransferFromCustomerAccount = 102,

        /// <summary>
        /// Քարտի մուտքագրման հայտ
        /// </summary>
        CardRegistrationOrder = 103,

        /// <summary>
        /// Պահատուփի տուժանքի մարում
        /// </summary>
        DepositCasePenaltyMatureOrder = 104,

        /// <summary>
        /// Կանխիկ պահատուփի տուժանքի մարում
        /// </summary>
        CashDepositCasePenaltyMatureOrder = 105,

        /// <summary>
        /// Փոխանցում խանութի հաշվին
        /// </summary>
        TransferToShopOrder = 106,

        /// <summary>
        /// Ապահովագրության հայտ
        /// </summary>
        InsuranceOrder = 107,

        /// <summary>
        /// Ապահովագրության հայտ(կանխիկ)
        /// </summary>
        CashInsuranceOrder = 108,

        /// <summary>
        /// Երաշխիքի ակտիվացում
        /// </summary>
        GuaranteeActivation = 109,
        /// <summary>
        /// Ակրեդիտիվի ակտիվացում
        /// </summary>
        AccreditiveActivation = 110,
        /// <summary>
        /// Ֆակտորինգի ակտիվացում
        /// </summary>
        FactoringActivation = 111,
        /// <summary>
        /// Քարտի տվյալների փոփոխման հայտ
        /// </summary>
        CardDataChangeOrder = 112,

        /// <summary>
        /// Քարտի սպասարկման վարձի գրաֆիկի տվյալների փոփոխման հայտ
        /// </summary>
        CardServiceFeeGrafikDataChangeOrder = 114,

        /// <summary>
        ///Փոխանցման ձևակերպում
        /// </summary>
        TransferConfirmOrder = 113,
        /// <summary>
        /// Քարտի սպասարկման վարձի գրաֆիկի հեռացման հայտ
        /// </summary>
        CardServiceFeeGrafikRemovableOrder = 115,

        /// <summary>
        /// Հեռահար բանկինգի պայմանագրի մուտքագրման հայտ
        /// </summary>
        HBApplicationOrder = 116,

        /// <summary>
        /// Ռեեստրով կոմունալ վճարում
        /// </summary>
        ReestrCommunalPayment = 117,

        /// <summary>
        /// Ռեեստրով կանխիկ կոմունալ վճարում
        /// </summary>
        ReestrCashCommunalPayment = 118,

        /// <summary>
        /// Հեռահար բանկինգի պայմանագրի վերականգնում
        /// </summary>
        HBApplicationRestoreOrder = 119,
        /// <summary>
        /// Հեռահար բանկինգի պայմանագրի դադարեցում
        /// </summary>
        HBApplicationTerminationOrder = 120,
        /// <summary>
        /// Քարտի սպասարկման միջնորդավճարի գանձում խնդրահարույց վարկերի տարանցիկ հաշվից
        /// </summary>
        CardServiceFeePaymentFromProblematicLoanTransitAccount = 121,
        /// <summary>
        /// Ռեեստրով փոխանցում
        /// </summary>
        ReestrPaymentOrder = 122,
        /// <summary>
        ///Փոխանցման հեռացում
        /// </summary>
        TransferDeleteOrder = 123,

        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների հեռացման հայտ
        /// </summary>
        AccountAdditionalDataRemovableOrder = 124,

        /// <summary>
        /// Քարտի վերաթողարկման հայտ
        /// </summary>
        RenewedCardAccountRegOrder = 125,

        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի մուտքագրման հայտ
        /// </summary>
        PhoneBankingContractOrder = 126,

        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի մուտքագրման հայտ
        /// </summary>
        PhoneBankingContractEditOrder = 127,

        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի դադարեցման հայտ
        /// </summary>
        PhoneBankingContractClosingOrder = 128,

        /// <summary>
        /// Հեռահար բանկինգի փոփոխման  հայտ
        /// </summary>
        HBApplicationUpdateOrder = 132,
        /// <summary>
        /// Հեռահար բանկինգի տոկենի սինխրոնիզացիայի հայտ Servlet-ին
        /// </summary>
        HBServletRequestTokenUnBlockOrder = 135,
        /// <summary>
        /// Հեռահար բանկինգի տոկենի ակտիվացման հայտ Servlet-ին
        /// </summary>
        HBServletRequestTokenActivationOrder = 137,
        /// <summary>
        /// Հեռահար բանկինգի տոկենի ապաակտիվացման հայտ Servlet-ին
        /// </summary>
        HBServletRequestTokenDeactivationOrder = 138,
        /// <summary>
        /// Կանխիկ ելք ելքային գործարքների տարանցիկ հաշվից
        /// </summary>
        CashOutFromTransitAccountsOrder = 133,
        /// <summary>
        /// Փոխանցման խմբագրման հայտ
        /// </summary>
        TransferApproveOrder = 134,
        /// <summary>
        /// Քարտի կարգավիճակի փոփոխման հայտ
        /// </summary>
        CardStatusChangeOrder = 136,

        /// <summary>
        /// Փոխարինված քարտի հաշվի կցման հայտ
        /// </summary>
        ReplacedCardAccountRegOrder = 129,

        /// <summary>
        /// Լիազորագրի փակման հայտ
        /// </summary>
        CredentialTerminationOrder = 130,

        /// <summary>
        /// Լիազորագրի հեռացման հայտ
        /// </summary>
        CredentialDeleteOrder = 131,
        /// <summary>
        /// Վճարված երաշխիքի ակտիվացման հայտ
        /// </summary>
        PaidGuaranteeActivation = 141,
        /// <summary>
        /// Ֆակտորինգի դադարեցում
        /// </summary>
        FactoringTermination = 142,
        /// <summary>
        /// Երաշխիքի դադարեցում
        /// </summary>
        GuaranteeTermination = 143,
        /// <summary>
        /// Դրամարկղի մատյանի ավելցուկ պակասորդ
        /// </summary>
        CashBookSurPlusDeficit = 139,
        /// <summary>
        /// Դրամարկղի մատյանի մուտք ելք
        /// </summary>
        CashBookInOut = 140,
        /// <summary>
        /// Դրամարկղի մատյանի ավելցուկ պակասորդ ի հաստատում
        /// </summary>
        CashBookSurPlusDeficitApprove = 144,
        /// <summary>
        /// Դրամարկղի մատյանի  մուտք ելք ի հաստատում
        /// </summary>
        CashBookInOutApprove = 146,
        /// <summary>
        /// Դրամարկղի կանխիկի հայտի մերժում
        /// </summary>
        CashBookReject = 147,
        /// <summary>
        /// Ավանդային տվյալների փոփոխման հայտ
        /// </summary>
        DepositDataChangeOrder = 148,
        /// <summary>
        /// Դրամարկղի մատյանի ավելցուկ պակասորդ ի փակում
        /// </summary>
        CashBookSurPlusDeficitClosing = 149,
        /// <summary>
        /// Դրամարկղի մատյանի ավելցուկ պակասորդ ի փակում
        /// </summary>
        CashBookSurPlusDeficitClosingApprove = 150,
        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի ապաակտիվացման հայտ Servlet-ին
        /// </summary>
        HBServletRequestUserDeactivationOrder = 151,
        /// <summary>
        /// Հեռախոսազանգով ստացված փոխանցման տվյալների/կարգավիճակի փոփոխման հայտ
        /// </summary>
        TransferByCallChangeOrder = 145,
        /// <summary>
        /// Վճարված ֆակտորինգի ակտիվացման հայտ
        /// </summary>
        PaidFactoringActivation = 152,
        /// <summary>
        /// Մուտք հաշվին այլ կանխիկ տերմինալով
        /// </summary>
        CashInByOtherCashTerminal = 153,
        /// <summary>
        /// Վարկի մարում այլ կանխիկ տերմինալով
        /// </summary>
        LoanMatureByOtherCashTerminal = 154,
        /// <summary>
        /// Մուտք հաշվին փոխարկումով այլ կանխիկ տերմինալով
        /// </summary>
        CashConvertationByOtherCashTerminal = 155,
        /// <summary>
        /// Երաշխիքի դադարեցում
        /// </summary>
        AccreditiveTerminationOrder = 156,
        /// <summary>
        /// Տոկենի գրանցման կոդի վերաուղրկում
        /// </summary>
        HBRegistrationCodeResendOrder = 157,
        /// <summary>
        /// Տոկենի PIN  կոդի ցուցադրում
        /// </summary>
        HBServletRequestShowPINCodeOrder = 158,
        /// <summary>
        /// Արագ օվերդրաֆտի դիմում
        /// </summary>
        FastOverdraftApplication = 159,
        /// <summary>
        ///Վարկային դիմումի վերլուծություն 
        /// </summary>
        LoanApplicationAnalysis = 160,
        /// <summary>
        /// Վարկային դիմումի հաստատում
        /// </summary>
        LoanApplicationConfirmation = 161,

        /// <summary>
        /// Վարկային դիմումի հրաժարում
        /// </summary>
        CancelLoanApplication = 162,

        /// <summary>
        /// Վարկային դիմումի հեռացում
        /// </summary>
        DeleteLoanApplication = 163,

        /// <summary>
        /// Հեռահար բանկինգի ծառաայությունների ակտիվացումից հրաժարում
        /// </summary>
        HBActivationRejection = 164,

        /// <summary>
        /// Պահատուփի տուժանքի դադարեցման հայտ
        /// </summary>
        DepositCaseStoppingPenaltyCalculationOrder = 165,
        /// <summary>
        /// Հեռախոսային բանկինգի ակտիվացման հայտ
        /// </summary>
        PhoneBankingContractActivation = 166,

        /// <summary>
        /// Դրամարկղի հայտի հեռացում
        /// </summary>
        CashBookDelete = 167,

        /// <summary>
        /// Լիազորագրի ատիվացման հայտ
        /// </summary>
        CredentialActivationOrder = 168,
        /// <summary>
        /// Լիազորագրի ատիվացման հայտ կանխիկ
        /// </summary>
        CredentialActivationOrderCash = 169,
        /// <summary>
        /// Լիազորված անձի նույնականացման հայտ
        /// </summary>
        AssigneeIdentificationOrder = 170,
        /// <summary>
        /// Վարկի դասի հեռացման հայտ
        /// </summary>
        LoanProductClassificationRemoveOrder = 171,
        /// <summary>
        /// Վարկի դուրսգրման հայտ
        /// </summary>
        LoanProductMakeOutOrder = 172,

        /// <summary>
        /// Ցպահանջ ավանդի տոկոսադրույքի փոփոխման հայտ
        /// </summary>
        DemandDepositRateChangeOrder = 173,

        /// <summary>
        /// MR ծրագրի գրանցման հայտ
        /// </summary>
        CardMRRegistrationOrder = 174,

        /// <summary>
        /// Պրոդուկտի ներկայացվող տեղեկատվությունների կարգավորումների հայտի պահպանում
        /// </summary>
        ProductNotificationConfigurationsOrder = 175,

        /// <summary>
        /// MR ծրագրի սպասարկման վարձի գանձման հայտ
        /// </summary>
        CardMRServiceFeeChargingOrder = 176,

        /// <summary>
        /// MR ծրագրի վերաթողարկման հայտ
        /// </summary>
        CardMRReNewOrder = 177,

        /// <summary>
        /// MR ծրագրի դադարեցման հայտ
        /// </summary>
        CardMRCancelOrder = 178,

        /// <summary>
        /// Պրոդուկտի ներկայացվող տեղեկատվությունների կարգավորումների փոփոխման հայտի պահպանում
        /// </summary>
        ProductNotificationConfigurationsUpdateOrder = 179,

        /// <summary>
        /// Պրոդուկտի ներկայացվող տեղեկատվությունների կարգավորումների հեռացման հայտի պահպանում
        /// </summary>
        ProductNotificationConfigurationsDeleteOrder = 180,

        /// <summary>
        /// Քարտի USSD ծառայության գրանցման հայտ
        /// </summary>
        CardUSSDServiceOrder = 181,

        /// <summary>
        /// Գաղտնաբառի զրոյացում և ուղարկում էլեկտրոնային հասցեին
        /// </summary>
        HBRequestResetHBUserPasswordManually = 182,

        /// <summary>
        /// Գործարքի SWIFT ծանուցման հայտ
        /// </summary>
        TransactionSwiftConfirmOrder = 183,

        /// <summary>
        /// Անկանխիկ փոխանցում տարանցիկ հաշվին
        /// </summary>
        NonCashTransitPaymentOrder = 184,

        /// <summary>
        /// Տարանցիկ հաշվին մուտք փոխարկումով (անկանխիկ)
        /// </summary>
        NonCashTransitCurrencyExchangeOrder = 185,

        /// <summary>
        /// Պարտատոմսերի ձեռքբերման գրանցման հայտ
        /// </summary>
        BondRegistrationOrder = 186,

        /// <summary>
        /// SWIFT հաղորդագրության մերժման հայտ
        /// </summary>
        SWIFTMessageRejectOrder = 187,

        /// <summary>
        /// Քարտի 3DSecure ծառայության գրանցում/հանում
        /// </summary>
        Card3DSecureOrder = 188,

        /// <summary>
        /// Պարտատոմսի հեռացման հայտ
        /// </summary>
        BondQualityUpdateOrder = 189,

        /// <summary>
        /// Նոր ֆոնդի հայտի մուտքագրում
        /// </summary>
        AddFondOrder = 190,

        /// <summary>
        /// Արժեթղթերի հաշվի բացման հայտ
        /// </summary>
        DepositaryAccountOrder = 191,

        /// <summary>
        /// ֆոնդի փոփոխման հայտ
        /// </summary>
        ChangeFondOrder = 192,

        /// <summary>
        /// Պարտատոմսի գումարի գանձման հայտ
        /// </summary>
        BondAmountChargeOrder = 194,

        /// <summary>
        /// Անկանխիկ վարկի մարում ոչ գրաֆիկով այլ կանխիկ տերմինալով
        /// </summary>
        LoanMatureByOtherCashTerminalNotByGraphikByNonCash = 195,

        /// <summary>
        /// Կանխիկ վարկի մարում ոչ գրաֆիկով այլ կանխիկ տերմինալով
        /// </summary>
        LoanMatureByOtherCashTerminalNotByGraphikByCash = 196,

        /// <summary>
        /// Վարկի արտաբալանսից հանելու հայտ
        /// </summary>
        LoanProductMakeOutBalanceOrder = 197,

        /// <summary>
        /// Արագ Դրամական Համակարգերով փոխանցման չեղարկում
        /// </summary>
        RemittanceCancellationOrder = 198,

        /// <summary>
        /// Վարկային գծի երկարաձգում
        /// </summary>
        CreditLineProlongationOrder = 199,

        /// <summary>
        /// Դրամարկղի մատյանի ավելցուկ պակասորդ մասնակի
        /// </summary>
        CashBookSurPlusDeficitPartiallyClosing = 200,

        /// <summary>
        /// Պարբերական փոխանցման փոփոխման հայտ
        /// </summary>
        PeriodicTransferDataChangeOrder = 201,

        /// <summary>
        /// Արագ դրամական համակարգով փոխանցման տվյալների փոփոխման հայտ
        /// </summary>
        RemittanceAmendmentOrder = 202,

        /// <summary>
        /// Վարկի տվյալների փոփոխման հայտ
        /// </summary>
        LoanProductDataChangeOrder = 203,

        /// <summary>
        /// սեփական միջոցների տոկոսների փոփոխման հայտ (ֆոնդեր)
        /// </summary>
        ChangeFTPRateOrder = 205,

        /// <summary>
        /// քարտի բլոկավորման/ապաբլոկավորման հայտ
        /// </summary>
        ArcaCardsTransactionOrder = 206,

        /// <summary>
        /// քարտի սահմանաչափերի փոփոխման հայտ
        /// </summary>
        CardLimitChangeOrder = 207,

        // <summary>
        /// Վարկային պարտավորությունները ներելու հայտ
        /// </summary>
        CreditCommitmentForgiveness = 208,

        /// <summary>
        /// Քարտից քարտ փոխանցման հայտ
        /// </summary>
        CardToCardOrder = 209,

        /// <summary>
        /// Նոր քարտի հայտ
        /// </summary>
        PlasticCardOrder = 210,
        /// <summary>
        /// Լրացուցիչ քարտի հայտ
        /// </summary>
        AttachedPlasticCardOrder = 211,
        /// <summary>
        /// Կից քարտի հայտ
        /// </summary>
        LinkedPlasticCardOrder = 212,

        /// <summary>
        /// Վճարման ուղարկում Արքա համակարգ
        /// </summary>
        PaymentToArcaOrder = 213,

        /// <summary>
        /// Քարտի հեռացման հայտ
        /// </summary>
        PlasticCardRemovalOrder = 214,

        /// <summary>
        /// Նոր սերնդի կանխիկ տերմինալի տարանցիկ հաշվին մուտք
        /// </summary>
        SSTerminalCashInOrder = 216,

        /// <summary>
        /// Նոր սերնդի կանխիկ տերմինալի տարանցիկ հաշվից ելք
        /// </summary>
        SSTerminalCashOutOrder = 217,

        /// <summary>
        /// vorpes vark chdzevakerpvox hayt
        /// </summary>
        OtherInsuranceOrder = 219,

        /// <summary>
        /// Քարտային հաշվի հեռացման հայտ
        /// </summary>
        CardAccountRemovalOrder = 220,

        /// <summary>
        /// Տոկոսային մարժայի փոփոխում
        /// </summary>
        InterestMarginOrder = 221,

        /// <summary>
        /// ՀԲ պայմանագրի ամբողջական հասանելիությունների տրամադրում
        /// </summary>
        HBApplicationFullPermissionsGrantingOrder = 222,

        /// <summary>
        /// Քարտի SMS ծառայություն 
        /// </summary>
        PlasticCardSMSServiceOrder = 223,

        /// <summary>
		/// Թոքենի կարգավիճակի փոփոխություն
		/// </summary>
		VirtualCardStatusChangeOrder = 224,

        /// <summary>
        /// Նույն համարով և նույն ժամկետով քարտի փոխարինում
        /// </summary>
        PINRegenerationOrder = 225,

        /// <summary>
        /// Փոխարինում` նոր համար, նոր ժամկետ - առանց վ․գ․
        /// </summary>
        NonCreditLineCardReplaceOrder = 226,

        /// <summary>
        ///Փոխարինում` նոր համար, նոր ժամկետ -  վ․գ․
        /// </summary>
        CreditLineCardReplaceOrder = 227,

        /// <summary>
        /// Visa Direct/Money Send փոխանցում
        /// </summary>
        CardToOtherCardsOrder = 228,

        /// <summary>
        /// Վարկի հայտի հետաձգման հայտ
        /// </summary>
        LoanDelayOrder = 229,

        /// <summary>
        /// Հետաձգված հայտի չեղարկման հայտ
        /// </summary>
        CancelLoanDelayOrder = 230,

        /// <summary>
        /// Քարտի մասնաճյուղի տեղափոխում
        /// </summary>
        ChangeBranch = 231,

        /// <summary>
        /// Քարտի լրացուցիչ տվյալների խմբագրում
        /// </summary>
        CardAdditionalDataOrder = 232,

        /// <summary>
        /// Չվերաթողարկել քարտը
        /// </summary>
        CardNotRenewOrder = 233,

        /// <summary>
        /// Քարտային հաշվի փակման հայտ
        /// </summary>
        CardAccountClosingOrder = 234,

        /// <summary>
        /// Կցված Քարտից քարտ փոխանցման հայտ
        /// </summary>
        AttachedCardToCardOrder = 235,        

        /// <summary>
        /// Վարկի տոկոսագումարի զիջման հայտ
        /// </summary>
        LoanInterestRateConcessionOrder = 236,

        /// <summary>
        /// Կենսաթոշակի/նպաստի գումարի փոխանցման հայտ
        /// </summary>
        PensionPaymentOrder = 237,

        /// <summary>
        /// Անքարտ կանխիկացման հայտ
        /// </summary>
        CardLessCashOrder = 238,

        /// <summary>
        /// SberBank փոխանցման հայտ
        /// </summary>
        SberBankTransferOrder = 239,
    
        /// <summary>
        /// Քարտի վերաթողարկման հայտ
        /// </summary>
        LinkPaymentOrder = 242,

        /// <summary>
        /// Քարտի վերաթողարկման հայտ
        /// </summary>
        CardRenewOrder = 243,
       
        /// <summary>
        /// Քարտի վերաբացման հայտ
        /// </summary>
        CardReOpenOrder = 244,

        /// <summary>
        /// Գումարի հարցում
        /// </summary>
        BillSplit = 245,

        /// <summary>
        /// BillSplit փոխանցող մասնակցի չեղարկում
        /// </summary>
        BillSplitSenderRejection = 246,

        /// <summary>
        /// Bill Split Հիշեցման ուղարկում
        /// </summary>
        BillSplitReminder = 247,
        /// <summary>
        /// Ավանդի գրավով սպառողական վարկի հեռացում
        /// </summary>
        DeleteLoanOrder = 248,

        /// <summary>
        /// Հաշվի հեռացում
        /// </summary>
        AccountRemove = 249,
        /// <summary>
        /// Visa Alias հայտեր
        /// </summary>
        VisaAlias = 250
    }


    /// <summary>
    /// Հանձնարարականի համար նախատեսված հաշիվների տեսակներ
    /// </summary>
    public enum OrderAccountType : byte
    {
        /// <summary>
        /// Դեբետ հաշիվ
        /// </summary>
        DebitAccount = 1,

        /// <summary>
        /// Կրեդիտ հաշիվ
        /// </summary>
        CreditAccount = 2,

        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        FeeAccount = 3
    }

    public enum Action
    {
        Add = 1,
        Update = 2,
        Delete = 3,
        Deactivate = 4
    }
    /// <summary>
    /// Օբեկտների տեսակներ
    /// </summary>
    public enum ObjectTypes
    {
        /// <summary>
        /// Կոնտակտ (Գործընկեր)
        /// </summary>
        Contact = 1,
        /// <summary>
        /// Կոնտակտի հաշվե համար
        /// </summary>
        ContactAccount = 2,
        /// <summary>
        /// Հայտ
        /// </summary>
        Order = 3

    }

    /// <summary>
    /// Փոխանակման փոխարժեքի տեսակ
    /// </summary>
    public enum RateType
    {
        /// <summary>
        /// ԿԲ փոխարժեք
        /// </summary>
        CB = 1,
        /// <summary>
        /// Անկանխիկ գործարքի փոխարժեք
        /// </summary>
        NonCash = 2,
        /// <summary>
        /// Կանխիկ գործարքի փոխարժեք
        /// </summary>
        Cash = 3,
        /// <summary>
        /// Քարտային գործարքի փոխարժեք
        /// </summary>
        Card = 4,
        /// <summary>
        /// Կրկնակի փոխարկման փոխարժեք
        /// </summary>
        Cross = 5,
        /// Հեռախոսազանգով փոխանցման փոխարկման փոխարժեք
        /// </summary>
        Transfer = 6
    }

    /// <summary>
    /// Փոխանակման տեսակ
    /// </summary>
    public enum ExchangeDirection
    {
        /// <summary>
        /// Վաճառք
        /// </summary>
        Sell = 1,
        /// <summary>
        /// Գնում
        /// </summary>
        Buy = 2
    }

    /// <summary>
    /// Գործողության արդյունք
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// Նորմալ ավարտ
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Անհաջող ավարտ
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Չնույնակացված օգտագործող
        /// </summary>
        NotAutorized = 3,

        /// <summary>
        /// Վալիդացիայի սխալ
        /// </summary>
        ValidationError = 4,

        /// <summary>
        /// Հայտը պահպանված է, սակայն սխալի պատճառով կատարված չէ
        /// </summary>
        SavedNotConfirmed = 5,

        /// <summary>
        /// Նախազգուշացում
        /// </summary>
        Warning = 6,

        /// <summary>
        /// Ավտոմատ չկատարվող 
        /// </summary>
        NoneAutoConfirm = 7,

        /// <summary>
        /// Հայտը պահպանված է և ուղարկվել է հաստատման
        /// </summary>
        SaveAndSendToConfirm = 8,

        /// <summary>
        /// Կատարված է, վերդարձվել է արժեք
        /// </summary>
        DoneAndReturnedValues = 9,

        /// <summary>
        /// Սխալ հարցում
        /// </summary>
        InvalidRequest = 10,

        /// <summary>
        /// Կատարված է մասնակի
        /// </summary>
        PartiallyCompleted = 13,

        /// <summary>
        ///  Կատարված է , հետագա գործողություններում առաջացել է սխալ
        /// </summary>
        DoneErrorInFurtherActions = 14

    }

    /// <summary>
    /// Փոխանցման միջնորդավճարի գանձնման եղանակ
    /// </summary>
    public enum TransferType
    {
        /// <summary>
        /// ՀՀ Տարածքում
        /// </summary>
        Local = 1,
        /// <summary>
        /// OUR
        /// </summary>
        Our = 2,

        /// <summary>
        /// OUR OUR
        /// </summary>
        OurOur = 3,

        /// <summary>
        /// BEN
        /// </summary>
        Ben = 4
    }

    /// <summary>
    /// Երկրի կոդ
    /// </summary>
    public enum CountryCode : short
    {
        Armenia = 51,
        USA = 840,
        Bulgaria = 100,
        Turkey = 792,
        Romania = 642
    }

    /// <summary>
    /// Հայտի կարգավիճակ
    /// </summary>
    public enum OrderQuality : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Խմբագրվում է
        /// </summary>
        Draft = 1,
        /// <summary>
        /// Ուղարկված է
        /// </summary>
        Sent = 2,
        /// <summary>
        /// Ուղարկված է ԳՕԿ բաժին
        /// </summary>
        Sent3 = 3,
        /// <summary>
        /// Հաստատված է (հաճախորդի կողմից)
        /// </summary>
        Approved = 5,
        /// <summary>
        /// Մերժված է հաստատողի կողմից
        /// </summary>
        DeclinedByApprover = 6,
        /// <summary>
        /// Կատարված է 
        /// </summary>
        Completed = 30,
        /// <summary>
        /// Մերժված է
        /// </summary>
        Declined = 31,
        /// <summary>
        /// Մերժված է համաձայն հայտի
        /// </summary>
        Canceled = 32,
        /// <summary>
        /// Հեռացված է
        /// </summary>
        Removed = 40,
        /// <summary>
        /// Հեռացված է համաձայն հայտի
        /// </summary>
        RemovedByOrder = 41,
        /// <summary>
        /// Ընթացքում է
        /// </summary>
        Processing = 100,
        /// <summary>
        /// Ձևակերպվող գումարի սահմանաչափը անցնելիս
        /// հաստման ուղղարկելու կարգավիճակ:
        /// </summary>
        TransactionLimitApprovement = 50,
        /// <summary>
        /// Շեղումով փոխարկման հաստատման ուղղարկելու կարգավիճակ:
        /// </summary>
        CurrencyExchangeWithVariation = 55,
        /// <summary>
        /// Ոչ լիարժեք փաստաթղթերով տրամադրված քարտից ելքագրման 
        /// հաստատման ուղղարկելու կարգավիճակ:
        /// </summary>
        DebitFromNotGivenCard = 56,

        /// <summary>
        /// SBQ կատարված գործարքներ որոնց ձևակերպումները դեռևս չեն կատարվել
        /// </summary>
        SBQprocessed = 20,
        /// <summary>
        /// Ուղարկված է ՓԼԱՖ բաժնի հաստատման
        /// </summary>
        SentToAML = 57
    }

    /// <summary>
    /// Տվյալների աղբյուր
    /// </summary>
    public enum SourceType : short
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// Բանկի մասնաճյուղեր
        /// </summary>
        Bank = 2,
        /// <summary>
        /// ACBA Online համակարգ
        /// </summary>
        AcbaOnline = 1,
        /// <summary>
        /// ACBA Online համակարգ XML-ով
        /// </summary>
        AcbaOnlineXML = 3,
        /// <summary>
        /// Հակյական ծրագրեր
        /// </summary>
        ArmSoft = 4,
        /// <summary>
        /// Մոբայլ բանկինգ
        /// </summary>
        MobileBanking = 5,
        /// <summary>
        /// Հեռախոսային բանկինգ
        /// </summary>
        PhoneBanking = 6,
        /// <summary>
        /// Կանխիկ ընդունող տերմինալ
        /// </summary>
        CashInTerminal = 7,
        /// <summary>
        /// Արտաքին վճարային տերմինալներ
        /// </summary>
        ExternalCashTerminal = 8,
        /// <summary>
        /// ACBA BUSINESS TAB հավելված
        /// </summary>
        BusinesTab = 9,
        /// <summary>
        /// SSTerminal
        /// </summary>
        SSTerminal = 10,

        /// <summary>
        /// Էլեկտրոնային պայմանագիր
        /// </summary>
        EContract = 12,
        /// <summary>
        /// ՍՏԱԿ
        /// </summary>
        STAK = 13,
        /// <summary>
        /// Սբերբանկ փոխանցում
        /// </summary>
        SberBankTransfer = 14
    }

    /// <summary>
    /// Հաղորդագրությունների տեսակներ
    /// </summary>
    public enum MessageType : short
    {
        /// <summary>
        /// Ուղղարկված է հաճախորդի կողմից
        /// </summary>
        SentFromClient = 1,
        /// <summary>
        /// Հաղորդագրություն
        /// </summary>
        Message = 2,
        /// <summary>
        /// Գործարքի մերժում
        /// </summary>
        TransactionRefusal = 3,
        /// <summary>
        /// Հիշեցում
        /// </summary>
        Reminder = 4
    }


    /// <summary>
    /// Պրոդուկտների տեսակներ
    /// </summary>
    public enum ProductType : short
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        None = 0,

        /// <summary>
        /// Վարկ
        /// </summary>
        Loan = 1,
        /// <summary>
        /// Վարկային գիծ
        /// </summary>
        CreditLine = 2,

        /// <summary>
        /// Առևտրային վարկային գիծ
        /// </summary>
        CommercialCreditLine = 5,
        /// <summary>
        /// Ընթացիկ հաշիվ
        /// </summary>
        CurrentAccount = 10,
        /// <summary>
        /// Քարտ
        /// </summary>
        Card = 11,
        /// <summary>
        /// Ժամկետային ավանդ
        /// </summary>
        Deposit = 13,
        /// <summary>
        /// Այլ պարտավորություններ(Ապառիկ տեղում-ի "ընթացիկ հաշիվ")
        /// </summary>
        OtherLabilities = 18,
        /// <summary>
        /// Ապառիկ տեղում
        /// </summary>
        AparikTexum = 58,
        /// <summary>
        /// Պարբերական փոխանցում
        /// </summary>
        PeriodicTransfer = 112,
        /// <summary>
        /// Ակրեդիտիվ
        /// </summary>
        Accreditive = 7,
        /// <summary>
        /// Երաշխիք
        /// </summary>
        Guarantee = 6,
        /// <summary>
        /// Ֆակտորինգ
        /// </summary>
        Factoring = 28,
        /// <summary>
        /// Վճարած ակրեդիտիվ
        /// </summary>
        PaidAccreditive = 52,
        /// <summary>
        /// Վճարած ֆակտորինգ
        /// </summary>
        PaidFactoring = 54,
        /// <summary>
        /// Վճարած երաշխիք
        /// </summary>
        PaidGuarantee = 51,

        /// <summary>
        /// Amex քարտ
        /// </summary>
        AmexCard = 103,

        /// <summary>
        /// Visa քարտ
        /// </summary>
        VisaCard = 104,

        /// <summary>
        /// Maset քարտ
        /// </summary>
        MasterCard = 105,

        /// <summary>
        /// Arca քարտ
        /// </summary>
        ArcaCard = 109,

        /// <summary>
        /// Լիազորագիր
        /// </summary>
        Credential = 120,
        /// <summary>
        /// ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվ
        /// </summary>
        CardDahkAccount = 115,
        /// <summary>
        /// Սոցիալական ապահովության հաշիվ
        /// </summary>
        SocialSecurityAccount = 118,
        /// <summary>
        /// Կառուցապատողի հատուկ հաշիվներ
        /// </summary>
        DevelopersAccount = 119

    }
    /// <summary>
    /// Ավամդի տեսակները
    /// </summary>
    public enum DepositType : short
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Ավանդ դասական ժամկետային
        /// </summary>
        DepositClassic = 2,
        /// <summary>
        /// Ավանդ երեխանների համար
        /// </summary>
        ChildrensDeposit = 4,
        /// <summary>
        /// Ավանդ կուտակվող
        /// </summary>
        DepositAccumulative = 6,
        /// <summary>
        ///  Ցպահանջ ավանդ
        /// </summary>
        DepositGeneral = 10,
        /// <summary>
        /// Ավանդ ընտանեկան
        /// </summary>
        DepositFamily = 12,
        /// <summary>
        /// Ավանդ Փոխարկելի
        /// </summary>
        ConvertibleDeposit = 14,

        /// <summary>
        /// Բիզնես ավանդ
        /// </summary>
        BusinesDeposit = 15,
    }
    /// <summary>
    ///Ավանդի ավտոմատ երկարաձգում
    /// </summary>
    public enum YesNo : short
    {
        No = 1,
        Yes = 2,
        /// <summary>
        /// Չնշված
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Փոխանցման տեսակը
    /// </summary>
    public enum TransferSystem : short
    {
        None = 0,
        AVERS = 9,
        MONEYGRAM = 12,
        RIA = 15,
        UNISTREAM = 16,
        XPRESSMONEY = 17,
        ANELIK_ONLINE = 18,
        INTELEXPRESS = 19,
        TANDEM = 20,
        CONVERSE = 21,
        SIGUE = 22
    }
    public enum TransferCallQuality : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// Չհաստատված
        /// </summary>
        NotConfirmed = 0,
        /// <summary>
        /// Մերժված
        /// </summary>
        Denied = 1,
        /// <summary>
        /// Վերջնական հաստատված
        /// </summary>
        FinallyConfirmed = 2,
        /// <summary>
        /// Վճարման ենթակա
        /// </summary>
        ToPayment = 3,
        /// <summary>
        /// Վճարված
        /// </summary>
        Paied = 4,
        /// <summary>
        /// Մեկ անգամ հաստատված
        /// </summary>
        OneTimeConfirmed = 5,
        /// <summary>
        /// Վճարված/Չեղարկված
        /// </summary>
        PaidCancelled = 6,
        /// <summary>
        /// Ուղարկված/Վերադարձված
        /// </summary>
        SentCancelled = 7
    }

    public enum CustomerTypes : ushort
    {
        physical = 6,
        legal,
        /// <summary>
        /// Ֆիզ.անձ տնտեսվարող սուբյեկտներ
        /// </summary>
        physCustomerUndertakings = 2,
        /// <summary>
        /// Հիմնարմներ
        /// </summary>
        factories = 9,
        /// <summary>
        /// Այլ առևտրային կազմակերպություններ
        /// </summary>
        otherCommercialOrganizations = 5,
        /// <summary>
        /// Ոչ առևտրային կազմակերպություն
        /// </summary>
        notCommercialOrganizations = 8,
        /// <summary>
        /// բանկեր
        /// </summary>
        banks = 4,
        /// <summary>
        /// բանկի մասնաճյուղեր
        /// </summary>
        bankBranches = 3,
        /// <summary>
        /// Պետական ոչ առևտրային կազմակերպություններ
        /// </summary>
        stateNotCommercialOrganizations = 7,
        /// <summary>
        /// Պետական առևտրային կազմակերպություններ
        /// </summary>
        stateCommercialOrganizations = 15,
        /// <summary>
        /// Կենտրոնական բանկ
        /// </summary>
        centralBank = 1,
        /// <summary>
        /// Այլ ֆինանսական ընկերություններ
        /// </summary>
        otherFinansialCompanies = 10,
        /// <summary>
        /// Ծրագրերի իրականացման գրասենյակներ
        /// </summary>
        projectImplementationOffices = 11,
        /// <summary>
        /// Վարկային կազմակերպություն
        /// </summary>
        creditOrganizations = 12,
        /// <summary>
        /// ներդրումային ընկերություն
        /// </summary>
        investmentCompany = 13,
        /// <summary>
        /// Ապահովագրական ընկերություն
        /// </summary>
        insuranceCompany = 14,
        /// <summary>
        /// Վճարահաշվարկային կազմակերպություն
        /// </summary>
        paymentAndSattlementOrganizations = 16
    }

    /// <summary>
    /// Պրոդուկտների կարգավիճակի ֆիլտր
    /// </summary>
    public enum ProductQualityFilter : short
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        NotSet = 0,
        /// <summary>
        ///  Բաց տվյալ պրոդուկտներ 
        /// </summary>
        Opened = 1,
        /// <summary>
        /// Փակ տվյալ պրոդուկտներ
        /// </summary>
        Closed = 2,
        /// <summary>
        /// Դեռևս չակտիվացված պրոդուկտներ
        /// </summary>
        StillNotActive = 3,
        /// <summary>
        /// Միայն պայմանագիր տեսակի պրոդուկտներ
        /// </summary>
        Contracts = 4,
        /// <summary>
        ///  Բացի պայմանագիր տեսակի պրոդուկտներից
        /// </summary>
        AllExceptContracts = 5,
        /// <summary>
        /// Ակտիվ և դեռևս չակտիվացված պրոդուկտներ
        /// </summary>
        OpenedAndNotActive = 6,
        /// <summary>
        /// Բոլոր տվյալ պրոդուկտներ
        /// </summary>
        All = 100
    }
    public enum DebtTypes : short
    {
        /// <summary>
        /// Պարտավորություն հաշիվների սպ. գծով
        /// </summary>
        CurrentAccount = 1,
        /// <summary>
        /// Պարտավորություն ՀԲ սպ. գծով
        /// </summary>
        HomeBanking = 2,
        /// <summary>
        /// Քարտի սպ. միջնորդավճար
        /// </summary>
        Card = 3,
        /// <summary>
        /// Գերածախս
        /// </summary>
        Overdraft = 4,
        /// <summary>
        /// Գրավադրված գումար
        /// </summary>
        Provision = 5,
        /// <summary>
        /// Դուրս գրված կամ ժամկետանց վարկեր
        /// </summary>
        Loan = 6,
        /// <summary>
        /// Դուրս գրված կամ ժամկետանց վարկային գծեր
        /// </summary>
        CreditLine = 7,
        /// <summary>
        /// Ժամկետանց երաշխիք
        /// </summary>
        GivenGuarantee = 8,
        /// <summary>
        /// ԴԱՀԿ արգելանք
        /// </summary>
        Dahk = 9,
        /// <summary>
        /// ՊԵԿ արգելանք
        /// </summary>
        PEK = 10

    }
    public enum MatureType : short
    {
        /// <summary>
        /// Վարկի տոկոսի մարում
        /// </summary>
        RateRepayment = 1,
        /// <summary>
        /// Վարկի մասնակի մարում
        /// </summary>
        PartialRepayment = 2,
        /// <summary>
        /// Վարկի լրիվ մարում
        /// </summary>
        FullRepayment = 4,

        /// <summary>
        /// Պետ տուրք
        /// </summary>
        ClaimRepayment = 5,
        /// <summary>
        /// Մարում վարկային կոդով
        /// </summary>
        RepaymentByCreditCode = 6,
        /// <summary>
        /// Վարկի մասնակի համաձայն գրաֆիկի
        /// </summary>
        PartialRepaymentByGrafik = 9

    }


    public enum AddressTypes : ushort { real = 1, registration = 2, businness = 3 }


    public enum EmploymentsTypes : short
    {
        main = 1,
        joint = 2,
        past = 3
    }


    public enum AbonentTypes : short
    {
        physical = 1,
        legal = 2
    }

    public enum LinkPersonsTypes : ushort
    {
        linkPerson = 0/*փոխկապակցված անձ*/,
        manager = 4/*տնօրեն*/,
        realBeneficiary = 7/*իրական շահառու*/,
        chiefAcc = 8 /*գլխ.հաշվապահ*/,
        owner = 1/*Սեփականատեր/ կազմ.մասնակից*/,
        realAccountOwner = 9/*իրական հաշվետեր*/
    } //և այլն...

    public enum OrderNumberTypes : short
    {
        /// <summary>
        /// Կանխիկ մուտք
        /// </summary>
        CashIn = 1,
        /// <summary>
        /// Կանխիկ ելք
        /// </summary>
        CashOut = 2,
        /// <summary>
        /// Մեմօրդեր (կոմունալ )
        /// </summary>
        MemOrder = 3,
        /// <summary>
        /// Միջազգային փոխանցում
        /// </summary>
        InternationalOrder = 4,
        /// <summary>
        ///Արտհաշվեկշռային օրդեր
        /// </summary>
        OutMemOrder = 5,
        /// <summary>
        /// Փոխանցում ՀՀ տարածքում
        /// </summary>
        RATransfer = 6,
        /// <summary>
        /// Փոխանակում
        /// </summary>
        Convertation = 7,
        /// <summary>
        /// Ուղղիչ օրդեր
        /// </summary>
        CorrectMemOrder = 8,
        /// <summary>
        /// Պարբերական փոխանցում
        /// </summary>
        OperationByPeriod = 9,
        /// <summary>
        /// Սեփական հաշիվների միջև
        /// </summary>
        PaymentOrder = 10
    }

    public enum TransitAccountTypes : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        None = 0,
        /// <summary>
        /// ՀՀ փոխանցման համար
        /// </summary>
        ForArmTransfer = 1,
        /// <summary>
        /// Խնդրահարույց վարկ
        /// </summary>
        ForProblemLoans = 2,

        /// <summary>
        /// Խնդրահարույց վարկ
        /// </summary>
        ForLeasingLoans = 3,

        /// <summary>
        /// Ռեեստրի վարման և պահառության վճարներ
        /// </summary>
        ForReestrManagement = 4,
        /// <summary>
        /// Վարկային կոդով մարման համար
        /// </summary>
        ForMatureByCreditCode = 5,

        /// <summary>
        /// Պարտատոմսի գնման համար գումարի ապահովման տարանցիկ հաշիվ
        /// </summary>
        ForBond = 6
    }

    public enum AdditionalValueType : ushort
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Տեքստային արժեք
        /// </summary>
        String = 1,

        /// <summary>
        /// Կրկնակի ճշտությամբ թվային արժեք
        /// </summary>
        Double = 2,
        /// <summary>
        /// Ամբողջ թվային արժեք
        /// </summary>
        Int = 3,

        /// <summary>
        /// Տոկոսային արժեք
        /// </summary>
        Percent = 4,

        /// <summary>
        /// Ամսաթիվ
        /// </summary>
        Date = 5
    }

    /// <summary>
    /// Փոխարկման կլորացման ուղղություն
    /// </summary>
    public enum ExchangeRoundingDirectionType : byte
    {
        /// <summary>
        /// Արտարժույթի նկատմամբ
        /// </summary>
        ToCurrency = 1,
        /// <summary>
        /// ՀՀ Դրամի նկատմամբ
        /// </summary>
        ToAMD = 2
    }

    public enum ExchangeRateVariationType : byte
    {
        /// <summary>
        /// Փոխարժեքի շեղում չկա
        /// </summary>
        None = 0,
        /// <summary>
        /// Փոխարժեքը տարբերվում է գլխամասի փոխարժեքից
        /// </summary>
        DillingVariation = 1,
        /// <summary>
        /// Փթխարժեքը տարբերվում է մասնաճյուղի փոխարժեքից
        /// </summary>
        BranchVariation = 2
    }

    public enum ConfirmationOperationType : byte
    {
        /// <summary>
        /// Կանխիկ մուտք ելք
        /// </summary>
        Cash = 3,
        /// <summary>
        /// Փոխարկում
        /// </summary>
        Convertation = 8,
        /// <summary>
        /// Մեմ օրդեր
        /// </summary>
        MemOrder = 6,
        /// <summary>
        /// Փոխանցում դուրս
        /// </summary>
        ExternalTransfer = 1,
        /// <summary>
        /// Ներքին փոխանցում
        /// </summary>
        InternalTransfer = 4
    }


    /// <summary>
    /// ՀԲ ունենալու կարգավիճակ
    /// </summary>
    public enum HasHB : byte
    {
        /// <summary>
        /// Առկա չէ
        /// </summary>
        No = 1,
        /// <summary>
        /// Առկա է
        /// </summary>
        Yes = 2,
        /// <summary>
        /// Պահանջվում է ակտիվացում
        /// </summary>
        ForActivate = 3,

    }

    public enum Languages : byte
    {
        hy = 1,
        eng = 2
    }

    /// <summary>
    /// Հեռախոսահամարի տեսակը
    /// </summary>
    public enum PrepaidSign : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Հետվճարային
        /// </summary>
        Prepaid = 1,
        /// <summary>
        /// Կանխավճարային
        /// </summary>
        NotPrepaid = 2

    }

    public enum ServiceType : short
    {
        /// <summary>
        /// Հաճախորդի սպասարկում
        /// </summary>
        CustomerService = 1,
        /// <summary>
        /// Ոչ հաճախորդի սպասարկում
        /// </summary>
        NonCustomerService = 2
    }

    public enum ServicePaymenteNoteType
    {
        /// <summary>
        /// Չգանձել 
        /// </summary>
        Chgandzel = 0,
        /// <summary>
        /// Գանձել
        /// </summary>
        Gandzel = 1

    }

    public enum HBApplicationQuality : byte
    {
        /// <summary>
        /// Հեռացված
        /// </summary>
        Deleted = 0,
        /// <summary>
        /// Դիմում
        /// </summary>
        Request = 1,
        /// <summary>
        /// Հաստատված
        /// </summary>
        Approved = 3,
        /// <summary>
        /// Գործող
        /// </summary>
        Active = 5,
        /// <summary>
        /// Դադարեցված
        /// </summary>
        Deactivated = 6
    }

    public enum HBTokenQuality : byte
    {
        /// <summary>
        /// Նկարագրված չէ
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Գործող
        /// </summary>
        Active = 1,
        /// <summary>
        /// Դադարեցված
        /// </summary>
        Deactivated = 2,
        /// <summary>
        /// Չհաստատված
        /// </summary>
        StillNotConfirmed = 3,
        /// <summary>
        /// Չակտիվացված
        /// </summary>
        StillNotActive = 4,
        /// <summary>
        /// Բլոկաբորված
        /// </summary>
        Blocked = 5
    }
    public enum HBTokenTypes : byte
    {
        /// <summary>
        /// Նկարագրված չէ
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Տոկեն
        /// </summary>
        Token = 1,
        /// <summary>
        /// Մոբայլ տոկեն
        /// </summary>
        MobileToken = 2,
        /// <summary>
        /// Մոբայլ բանկինգ
        /// </summary>
        MobileBanking = 3

    }
    public enum HBServletAction
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        [Description("")]
        NotSpecified = 0,
        /// <summary>
        /// Տոկենի ակտիվացում
        /// </summary>
        [Description("activatetoken")]
        ActivateToken = 1,
        /// <summary>
        /// Տոկենի ապաբլոկավորում
        /// </summary>
        [Description("unlocktoken")]
        UnlockToken = 2,
        /// <summary>
        /// Տոկենի վերջնական ապաակտիվացում
        /// </summary>
        [Description("")]
        DeactivateToken = 3,
        /// <summary>
        /// Օգտագործողի ապաակտիվացում(կցված տոկենները վերջնականապես կապաակտիվանան): 
        /// Նոր տոկեն կցելու դեպքում օգտագործողը ավտոմատ կակտիվանա)
        /// </summary>
        [Description("")]
        DeactivateUser = 4,
        /// <summary>
        /// Տոկենի PIN  կոդի ցուցադրում
        /// </summary>
        ShowPINCode = 5,
        /// <summary>
        /// Գաղտնաբառի զրոյացում և ուղարկում էլեկտրոնային հասցեին
        /// </summary>
        ResetUserPasswordManually = 6,
        /// <summary>
        /// Օգտագործողի ակտիվացում
        /// </summary>
        ActivateUser = 7,
        /// <summary>
        /// Օգտագործողի ապաբլոկավորում
        /// </summary>
        UnlockUser = 8
    }
    public enum HBTokenSubType : short
    {
        /// <summary>
        /// Նկարագրված չէ
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// Նոր
        /// </summary>
        New = 1,
        /// <summary>
        /// Հավելյալ
        /// </summary>
        Additional = 2,
        /// <summary>
        /// Փոխարինված
        /// </summary>
        Replacement = 3,
        /// <summary>
        /// Վնասվածի դիմաց
        /// </summary>
        InsteadAffected = 4,
        /// <summary>
        /// Կորցրածի դիմաց
        /// </summary>
        InsteadLosted = 5
    }

    public enum HBServiceFeeRequestTypes
    {
        NotSpecified = 0,
        /// <summary>
        /// Նոր պայմանագիր
        /// </summary>
        NewApplication = 1,
        /// <summary>
        /// Տոկենի ավելացում
        /// </summary>
        NewToken = 2,
        /// <summary>
        /// Մուտքագրման հասանելիությունների տրամադրում
        /// </summary>
        AllowDataEntryPermission = 3

    }

    /// <summary>
    /// Լիազորագրերի տեսակներ
    /// </summary>
    public enum CredentialType : short
    {
        /// <summary>
        /// Լիազորագիր
        /// </summary>
        Credentials = 0,
        /// <summary>
        /// Ստորագրության նմուշ
        /// </summary>
        Signature = 1,
        /// <summary>
        /// Չեկային գրքույկ
        /// </summary>
        checkbook = 2
    }


    /// <summary>
    /// Արտաքին կանխիկ տերմինալի գործողությունների հասանելիությունների տեսակներ
    /// </summary>
    public enum CTAction : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        None = 0,
        /// <summary>
        /// Վճարման ռեկվիզիտների հարցում
        /// </summary>
        SearchForDetailsOfPayment = 1,

        /// <summary>
        /// Վճարման գրանցում
        /// </summary>
        RegisterPayment = 2,

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում հայտի համարով
        /// </summary>
        GetPaymentStatus = 3,

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում տերմինալի կողմից գեներացված գործարքի համարով
        /// </summary>
        GetPaymentStatusByOrderID = 4,

        /// <summary>
        /// Փոխանցում քարտին
        /// </summary>
        TransferToCard = 5,

        /// <summary>
        /// Փոխանցում քարտին գործարքի կարգավիճակի հարցում հայտի համարով
        /// </summary>
        GetC2CTransferStatus = 6,

        /// <summary>
        /// Փոխանցում քարտին գործարքի կարգավիճակի հարցում տերմինալի կողմից գեներացված գործարքի համարով
        /// </summary>
        GetC2CTransferStatusByOrderID = 7,

        /// <summary>
        /// Հաճախորդի որոնման հարցում
        /// </summary>
        GetClient = 8,

        /// <summary>
        /// Փոխանցման կատարման հարցում
        /// </summary>
        MakeTransfer = 9

    }
    /// <summary>
    /// Նախնական հայտի տեսակ
    /// </summary>
    public enum PreOrderType : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        None = 0,
        /// <summary>
        /// Ապառիկ տեղում տեսակի վարկերի ակտիվացման հայտերի ստեղծում
        /// </summary>
        CreditHereAndNowActivationOrdersCreation = 1,
        /// <summary>
        /// Դասակարգված վարկի հետ դասակարգման հայտերի ստեղծում
        /// </summary>
        ClassifiedLoanRemoveClassificationOrdersCreation = 2,
        /// <summary>
        /// Դասակարգված վարկի դուրսգրման հայտերի ստեղծում
        /// </summary>
        ClassifiedLoanMakeOutOrdersCreation = 3

    }
    public enum PreOrderQuality : short
    {
        /// <summary>
        /// Չնշված
        /// </summary>
        None = 0,
        /// <summary>
        /// Նոր
        /// </summary>
        New = 10,
        /// <summary>
        /// Չձևավորված
        /// </summary>
        NotCreated = 20,
        /// <summary>
        /// Կատարված
        /// </summary>
        Done = 30,
        /// <summary>
        /// Հեռացված
        /// </summary>
        Deleted = 40,
        /// <summary>
        /// Ձևավորված է 'ուղարկված է բանկ' կարգավիճակով
        /// </summary>
        CreatedNotDone = 3
    }
    public enum ClassifiedLoanActionType : short
    {
        /// <summary>
        /// Դասակարգված վարկի դասի հեռացում
        /// </summary>
        RemoveClassification = 1,
        /// <summary>
        /// Դասակարգված վարկի դուրսգրում
        /// </summary>
        MakeOut = 2
    }
    /// <summary>
    /// Վարկի դասի տեսակներ
    /// </summary>
    public enum RiskClassCode : short
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// Ստանդարտ
        /// </summary>
        Standard = 1,
        /// <summary>
        /// Հսկվող
        /// </summary>
        Guarded = 2,
        /// <summary>
        /// Ոչ ստանդարտ
        /// </summary>
        NotStandard = 3,
        /// <summary>
        /// Կասկածելի
        /// </summary>
        Suspicious = 4,
        /// <summary>
        /// Անհուսալի
        /// </summary>
        Precarious = 5

    }
    /// <summary>
    /// Դասակարգված վարկերի ցանկի տեսակ
    /// </summary>
    public enum ClassifiedLoanListType : short
    {
        /// <summary>
        /// Սխալ դասակարգված վարկեր
        /// </summary>
        WrongClassifiedLoansList = 1,
        /// <summary>
        /// Դուրս չգրված վարկեր
        /// </summary>
        NotOutLoansList = 2

    }

    /// <summary>
    /// Պարտատոմսի կարգավիճակ
    /// </summary>
    public enum BondIssueQuality : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Նոր
        /// </summary>
        New = 1,
        /// <summary>
        ///  Հաստատված
        /// </summary>
        Approved = 11,
        /// <summary>
        /// Հեռացված
        /// </summary>
        Deleted = 99,
        /// <summary>
        /// Բոլոր տեսակաները
        /// </summary>
        All = 100
    }

    public enum BondIssuerType : int
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,

        /// <summary>
        /// ՖԻՆԱՆՍՆԵՐԻ ՆԱԽԱՐԱՐՈՒԹՅՈՒՆ
        /// </summary>
        FN = 1,
        /// <summary>
        /// ԿԵՆՏՐՈՆԱԿԱՆ ԲԱՆԿ
        /// </summary>
        CB = 2,
        /// <summary>
        /// ԱԿԲԱ-ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ
        /// </summary>
        ACBA = 3
    }

    public enum BondIssuePeriod : short
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,

        /// <summary>
        /// Երկարաժամկետ
        /// </summary>
        LongTerm = 1,

        /// <summary>
        /// Կարճաժամկետ
        /// </summary>
        ShortTerm = 2
    }

    public enum BondQuality : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Պայմանագիր
        /// </summary>
        New = 10,
        /// <summary>
        ///  Գործող
        /// </summary>
        Approved = 1,
        /// <summary>
        /// Փակ
        /// </summary>
        Closed = 40,
        /// <summary>
        /// Հեռացված
        /// </summary>
        Deleted = 41,
        /// <summary>
        /// Հաստատման ենթակա
        /// </summary>
        AvailableForApprove = 20,
        /// <summary>
        /// Մերժված
        /// </summary>
        Rejected = 31

    }

    public enum DepositaryAccountExistence : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Առկա է Բանկի բազայում
        /// </summary>
        ExistsInBank = 1,
        /// <summary>
        ///  Առկա է
        /// </summary>
        Exists = 2,
        /// <summary>
        /// Առկա չէ
        /// </summary>
        NonExisted = 3

    }

    public enum BondRejectReason : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Ոչ ամբողջական փաստաթղթեր
        /// </summary>
        NonProperDocuments = 1,
        /// <summary>
        ///  Սխալ մուտքագրման ժամ
        /// </summary>
        WrongDate = 2,
        /// <summary>
        /// Այլ
        /// </summary>
        Other = 99

    }

    /// <summary>
    /// Քարտի տվյալ տեսակի(օրնակ՝ USSD)  ծառայության կարգավիճակներ
    /// </summary>
    public enum CardServiceQualities : short
    {
        /// <summary>
        /// Քարտի համար ծառայությունը գրանցված չէ
        /// </summary>
        NotRegistered = 0,

        /// <summary>
        /// Քարտի համար կա "Գրանցել" հայտ , որը ARCA-ում դեռ հաստատված չէ
        /// </summary>
        RegistrateNotConfirmedInArca = 1,

        /// <summary>
        /// Քարտի համար կա "Գրանցել" հայտ , որը ARCA-ում հաստատված է
        /// </summary>
        RegistrateConfirmedInArca = 2,

        /// <summary>
        /// Քարտի համար կա "Հանել" հայտ , որը ARCA-ում դեռ հաստատված չէ
        /// </summary>
        TerminateNotConfirmedInArca = 3,

        /// <summary>
        /// Քարտի համար կա  "Հանել" հայտ, որը ARCA -ում հաստատված է
        /// </summary>
        TerminateConfirmedInArca = 4,

        /// <summary>
        /// Քարտի համար կա  "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
        /// </summary>
        ChangeNotConfirmedInArca = 5,


        /// <summary>
        /// Քարտի համար կա  "Փոփոխել" հայտ, որը ARCA -ում հաստատված է
        /// </summary>
        ChangeConfirmedInArca = 6

    }

    public enum OperDayClosingStatus : short
    {
        /// <summary>
        /// Գոռծարնական օրը փակ է
        /// </summary>
        OperDayClosed = 1,


        /// <summary>
        /// Գոռծարնական օրը բաց է
        /// </summary>
        OperDayOpened = 0
    }


    /// <summary>
    /// FTP տոկոսադրույքների տեսակներ
    /// </summary>
    public enum FTPRateType : byte
    {
        /// <summary>
        /// Սեփական միջոցներ
        /// </summary>
        OwnFunds = 1,

        /// <summary>
        /// Կրեդիտ հաշիվ
        /// </summary>
        ResourcePayments = 2

    }


    public enum OperDayOptionsType : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,
        /// <summary>
        /// Գործառնական օրը չի համապատասխանում ընթացիկ գործառնական օրվան
        /// </summary>
        NotOperDay = 1,

        /// <summary>
        /// Փակման հերթականությունը չի համապատասխանում սահմանված հերթականությանը
        /// </summary>
        NotMatchedSequence = 2,

        /// <summary>
        /// Գոյություն ունեն վարկերի մարումներ, որոնք նշված օրը ընտրելու դեպքում կդառնան ժամկետանց
        /// </summary>
        LoanRepaymentOverdue = 3,

        /// <summary>
        ///Նշված գործառնական օրը հանդիասանում է շաբաթ, կիրակի կամ այլ ոչ աշխատանքային օր
        /// </summary>
        WeekendOperDay = 4
    }

    public enum OperDayModeType : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 99,
        /// <summary>
        /// Բաց գործառնական օր 
        /// </summary>
        OpenOperDay = 0,

        /// <summary>
        /// Օրվա փակումից մինչև տոկոսների կատարում
        /// </summary>
        OperDay = 1,

        /// <summary>
        /// Փակ գործառնական օր
        /// </summary>
        CloseOperDay = 2,

    }
    public enum IsTransferRegistratoinDateExists : byte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = 0,

        /// <summary>
        /// Պետ. տուրքի փոխանցման ամսաթիվ առկա է
        /// </summary>
        TaxCourtDecisionIsTrue = 1,

        /// <summary>
        /// Պետ. տուրքի փոխանցման ամսաթիվ առկա չէ
        /// </summary>
        TaxCourtDecisionIsFalse = 2,

    }

    /// <summary>
    /// Քարտի լիմիտի տեսակ
    /// </summary>
    public enum LimitType : short
    {
        /// <summary>
        /// Մեկ օրվա ընթացքում կանխիկացման գործարքների գումար
        /// </summary>
        DailyCashingAmountLimit = 4,

        /// <summary>
        /// Մեկ օրվա ընթացքում կանխիկացման գործարքների քանակ
        /// </summary>
        DailyCashingQuantityLimit = 3,

        /// <summary>
        /// Մեկ օրվա ընթացքում վճարային գործարքների գումար
        /// </summary>
        DailyPaymentsAmountLimit = 8,

        /// <summary>
        /// Կից քարտի գործարքների ամսական ընդհանուր գումար
        /// </summary>
        MonthlyAggregateLimit = 7
    }

    /// <summary>
    /// Ձևանմուշների տեսակներ
    /// </summary>
    public enum TemplateType : short
    {
        /// <summary>
        /// Նշված չէ
        /// </summary>
        /// </summary>
        None = 0,

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված ձևանմուշ
        /// </summary>
        CreatedByCustomer = 1,

        /// <summary>
        /// Գործարքների խմբի մեջ ներառված ծառայության ձևանմուշ
        /// </summary>
        CreatedAsGroupService = 2
    }
    public enum TaxQuality : SByte
    {
        /// <summary>
        /// Լրացված չէ
        /// </summary>
        None = -1,

        /// <summary>
        /// Պետ. տուրքի գործող կարգավիճակ
        /// </summary>
        TaxQualityOperating = 0,

        /// <summary>
        /// Պետ. տուրքի պայմանագիր կարգավիճակ
        /// </summary>
        TaxQualityContract = 10,

        /// <summary>
        /// Պետ. տուրքի Դուրսգրված կարգավիճակ
        /// </summary>
        TaxQualityWrittenOff = 11,

        /// <summary>
        /// Պետ. տուրքի Դ/գր.արտաբալ.հանված կարգավիճակ
        /// </summary>
        TaxQualityRemoved = 12,

        /// <summary>
        /// Պետ. տուրքի Մարված կարգավիճակ
        /// </summary>
        TaxQualityDischarged = 40,

        /// <summary>
        /// Պետ. տուրքի Զիջված կարգավիճակ
        /// </summary>
        TaxQualityFreed = 41,

    }

}


/// <summary>
/// Քարտի տեսակներ
/// </summary>
public enum PlasticCardType
{
    VISA_BUSINESS = 3,
    VISA_BUSINESS_PW = 45,
    VISA_BSNS_CHP = 18,
    ARCA_BUSINESS = 22,
    ARCA_PENSION = 21,
    AMEX_GOLD_CHP = 41,
    ACBA_FEDERATION = 43,
    VISA_VIRTUAL = 51

}

/// <summary>
/// Գործարքների խմբի տեսակ
/// </summary>
public enum OrderGroupType : short
{
    /// <summary>
    /// Նշված չէ
    /// </summary>
    None = 0,

    /// <summary>
    /// Հաճախորդի կողմից ստեղծված
    /// </summary>
    CreatedByCustomer = 1,

    /// <summary>
    /// Ծրագրի կողմից ավտոմատ ստեղծված (մի քանի հայտ միավորելու համար)
    /// </summary>
    CreatedAutomatically = 2
}

/// <summary>
/// Աշխատավարձային ծրագրերի տեսակներ
/// </summary>
public enum RelatedOfficeTypes
{
    PensionCards = 1158,
    AmexGoldFoundersDirectors = 1227,
    AmexPAPEMonthly = 1663,
    AMexPAPEYearly = 1664,
    AmexGoldMasterCardGold = 2650,
    AMEXLinkedCards = 1314,
    AMEXLinkedReplacementCards = 1692,
    GPMMembers = 2769
}

public enum TemplateStatus : short
{
    /// <summary>
    /// Նշված չէ
    /// </summary>
    None = 0,

    /// <summary>
    /// Գործող
    /// </summary>
    Active = 1,

    /// <summary>
    /// Հեռացված 
    /// </summary>
    Deleted = 40
}

public enum PlasticCardReportRecievingTypes : int
{
    /// <summary>
    /// Փոստային կապի միջոցով
    /// </summary>
    byMail = 1,
    /// <summary>
    /// Էլ․ փոստի միջոցով
    /// </summary>
    byEmail = 4,
    /// <summary>
    /// Հեռացված
    /// </summary>
    Deleted = 0,
    /// <summary>
    /// Առձեռն՝ Բանկի տարածքում
    /// </summary>
    byHandBankTerritory = 3
}

/// <summary>
/// 24/7 ռեժիմով գործարք կատարելու թույլատրելիություն
/// </summary>
public enum MakingTransactionIn24_7ModeAllowbility : ushort
{
    /// <summary>
    /// Չի թույլատրվում
    /// </summary>
    NotAllowed = 0,
    /// <summary>
    /// Թույլատրվում է
    /// </summary>
    Allowed = 1,
    /// <summary>
    /// Թույլատրվում է որոշակի պայմանի դեպքում
    /// </summary>
    ConditionallyAllowed = 2
}


public enum PlasticCardPINCodeRecievingTypes
{
    /// <summary>
    /// PIN կոդի ծրագրով
    /// </summary>
    byPINCodeProgram = 1,
    /// <summary>
    /// SMS-ի միջոցով
    /// </summary>
    bySMS = 2
}

/// <summary>
/// Տոկոսային մարժաայի տեսակներ
/// </summary>
public enum InterestMarginType : byte
{
    /// <summary>
    /// Ընթացիկ (ցպահանջ) հաշիվների տեղաբաշխում
    /// </summary>
    CurrentAccounts = 1,

    /// <summary>
    /// Ժամկետային ավանդների տեղաբաշխում
    /// </summary>
    TermDeposits = 2,

    /// <summary>
    /// Բանկի կողմից թողարկված պարտատոմսերի դիմաց ներգրաված միջոցների տեղաբաշխում
    /// </summary>
    BankBonds = 3,
}

/// <summary>
/// Գործարքների խմբի կարգավիճակ
/// </summary>
public enum OrderGroupStatus : short
{
    /// <summary>
    /// Գործող
    /// </summary>
    Active = 1,

    /// <summary>
    /// Հեռացված
    /// </summary>
    Deleted = 0
}

public enum CardStatementDataTypes : short
{
    /// <summary>
    /// MR
    /// </summary>
    MR = 0,

    /// <summary>
    /// ՎԱՐԿԱՅԻՆ ԳԾԻ ՎԵՐԱԲԵՐՅԱԼ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
    /// </summary>
    SummaryCreditLine = 1,

    /// <summary>
    /// ՎԱՍՏԱԿԱԾ ԵԿԱՄՈՒՏՆԵՐ ԵՎ ԲՈՆՈՒՍՆԵՐ
    /// </summary>
    SummaryBonus = 2,

    /// <summary>
    /// Լրացուցիչ տվյալներ
    /// </summary>
    AddInfo = 3,

    /// <summary>
    /// ՉՁԵՎԱԿԵՐՊՎԱԾ ԵՎ ԱՐԳԵԼԱԴՐՎԱԾ  ԳՈՐԾԱՐՔՆԵՐԻ/ԳՈՐԾԱՌՆՈՒԹՅՈՒՆՆԵՐԻ ՎԵՐԱԲԵՐՅԱԼ ՄԱՆՐԱՄԱՍՆ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
    /// </summary>
    UnsettledTransactions = 4,

    /// <summary>
    /// ԿԱՏԱՐՎԱԾ ԳՈՐԾԱՐՔՆԵՐԻ ՎԵՐԱԲԵՐՅԱԼ ԱՄՓՈՓ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
    /// </summary>
    Summery = 5,

    /// <summary>
    /// ՎԱՐԿԱՅԻՆ ԳԾԻ ՎԵՐԱԲԵՐՅԱԼ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
    /// </summary>
    Link = 6,

    /// <summary>
    /// card_statement_BankData (PR)
    /// </summary>
    BankDate = 7,
}


public enum TaxCourtDecision : SByte
{
    /// <summary>
    /// Լրացված չէ
    /// </summary>
    None = -1,

    /// <summary>
    /// Պետ. տուրքը բավարարել հօգուտ հաճախորդի
    /// </summary>
    TaxQualityOperating = 0,

    /// <summary>
    /// Պետ. տուրքը բավարարել հօգուտ Բանկի
    /// </summary>
    TaxQualityContract = 1,

    /// <summary>
    /// Պետ. տուրքը բավարարել հօգուտ Բանկի
    /// </summary>
    TaxQualityDecision = 2,

}

public enum PlasticCardSystemType
{
    Amex = 3
}

public enum RelatedOfficeContractStatusTypes
{
    /// <summary>
    /// Դադարեցված
    /// </summary>
    Suspended = 0,
    /// <summary>
    /// Սառեցված
    /// </summary>
    Frozen = 2
}
public enum PlasticCardSentToArcaStatus
{
    /// <summary>
    /// Տվյալներ հայտնաբերված չեն 
    /// </summary>
    NoInfo = 0,
    /// <summary>
    /// ֆայլերը ուղղարկված են արքա
    /// </summary>
    SentToArca = 1,
    /// <summary>
    /// ֆայլերը ուղղարկված չեն
    /// </summary>
    NoFiles = 2

}
public enum ServiceFeePeriodicityType : int
{
    None = 0,
    Monthly = 1,
    Yearly = 2
}

public enum ArcaCardsTransactionActionTypes : short
{
    None = 0,
    Block = 1,
    Unblock = 2
}
/// <summary>
/// հասանելի մնացորդի որոշման տեսակներ
/// </summary>
public enum DigitalAccountRestConfigurationType : short
{
    None = 0,
    Defalut = 1,
    Custom = 2
}
public enum GPMMemberStatus
{
    /// <summary>
    /// ԳՓՄ անդամ չի հանդիսանում
    /// </summary>
    NotMember = 0,
    /// <summary>
    /// ԳՓՄ անդամակցության ամսաթիվը մուտքագրված չէ
    /// </summary>
    NoMemberDate = 1,
    /// <summary>
    /// ԳՓՄ անդամ
    /// </summary>
    IsMember = 2
}
public enum PaymentToARCAStatus : short
{
    None = 0,
    Waiting = 1,
    Sending = 2,
    InProgress = 3,
    Failure = 4,
    Success = 5,
    Resend = 6,
    Manual = 7
}

public enum TypeOfHbProductTypes
{
    /// <summary>
    /// Բոլորը
    /// </summary>
    None,
    /// <summary>
    /// Փոխանցում
    /// </summary>
    Transfers,
    /// <summary>
    /// Դիմում
    /// </summary>
    Application,
    /// <summary>
    /// Վճարում 
    /// </summary>
    Payment
}

public enum LoanProductType
{
    /// <summary>
    /// Ունի գոնե մեկ ընթացիկ հաշիվ
    /// </summary>
    Loan = 1,
    /// <summary>
    /// Ունի գոնե մեկ ընթացիկ հաշիվ և գոնե մեկ քարտ
    /// </summary>
    CreditLine = 2,
    /// <summary>
    /// Ունի գոնե մեկ քարտ
    /// </summary>
    FastOverdraft = 3
}
public enum VirtualCardRequestTypes
{
    /// <summary>
    /// Քարտի վերաթողարկում, փոխարինում
    /// </summary>
    ReNewReplace = 1,
    /// <summary>
    /// Քարտի հաշվի հեռացում
    /// </summary>
    Remove = 2,
    /// <summary>
    /// Քարտի փակում
    /// </summary>
    Closing = 3

}

public enum ReferenceReceiptTypes : int
{

    /// <summary>
    /// Ընտրված չէ
    /// </summary>
    None = 0,

    /// <summary>
    /// Մասնաճյուղի տարածքում՝ նույն աշխ. օրվա ընթացքում
    /// </summary>
    AtBranchSameWorkingDay = 1,

    /// <summary>
    /// Մասնաճյուղի տարածքում
    /// </summary>
    AtBranch = 2,

    /// <summary>
    /// Առաքման ծառայության միջոցով
    /// </summary>
    DeliveryService = 3
}

public enum PeriodicTransferTypes : byte
{
    /// <summary>
    /// Ընտրված չէ
    /// </summary>
    NotSet = 0,
    /// <summary>
    /// Փոխանցումներ
    /// </summary>
    Transfer = 1,
    /// <summary>
    /// Վճարումներ
    /// </summary>
    Payment = 2
}

public enum EmailSenderProfiles
{
    /// <summary>
    /// Notifications
    /// </summary>
    Notifications = 1,
    /// <summary>
    /// ACBA Notification
    /// </summary>
    Noreply = 2,
    /// <summary>
    /// ACBA-CA BANK
    /// </summary>
    Info = 3,
    /// <summary>
    /// AcbaOnline
    /// </summary>
    Acbaonline = 4,
    /// <summary>
    /// ACBA Electronic Contracts
    /// </summary>
    Econtracts = 5,
}
public enum InternationalCardTransferTypes
{
    VisaDirect = 1,
    MasterCardMoneySend = 2
}
public enum CredilLineActivatorType : byte
{
    /// <summary>
    /// Վարկային գծի ակտիվացում
    /// </summary>
    ActivateCreditLine = 1,
    /// <summary>
    /// Վարկային գծի դադարեցում
    /// </summary>
    CloseCreditLine = 2
}

/// <summary>
/// ՍՏԱԿ R2A փոխանցման վճարման եղանակները
/// </summary>
public enum PayoutDeliveryCodeForR2A : ushort
{
    /// <summary>
    /// Կանխիկ ստացման եղանակով
    /// </summary>
    Cash = 10,
    /// <summary>
    /// Քարտին վճարման եղանակով
    /// </summary>
    Card = 20,
    /// <summary>
    /// Էլ․ դրամապանակի եղանակով
    /// </summary>
    EMoney = 30,
    /// <summary>
    /// Հաշվին վճարման եղանակով
    /// </summary>
    Account = 40
}

public enum CardLessCashoutStatus
{
    Waiting = 0,
    Rejected = 1,
    Completed = 2
}

public enum BillSplitStatus
{
    Completed = 1,
    InProgress = 0
}

public enum LinkPaymentSourceType : byte
{
    /// <summary>
    /// Նշված չէ
    /// </summary>
    None,

    /// <summary>
    /// հղումով փոխանցում
    /// </summary>
    FromLinkPayment,

    /// <summary>
    /// BillSplit-ից եկած հղումով փոխանցում
    /// </summary>
    FromBillSplit
}



