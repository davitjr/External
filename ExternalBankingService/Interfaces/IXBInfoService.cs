using System;
using System.Collections.Generic;
using System.ServiceModel;
using ExternalBanking;

namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    [ServiceKnownType(typeof(Dictionary<string,string>))]
    [ServiceKnownType(typeof(KeyValuePair<long, string>))]
    [ServiceKnownType(typeof(DepositOption))]
    [ServiceKnownType(typeof(CardTariffAdditionalInformation))]
    [ServiceKnownType(typeof(SourceType))]

    [ServiceKnownType(typeof(KeyValuePair<int, string>))]
    public interface IXBInfoService
    {

        [OperationContract]
        Dictionary<string, string> GetAccountStatuses();

        [OperationContract]
        Dictionary<string, string> GetCurrencies();

        [OperationContract]
        Dictionary<string, string> GetInternationalPaymentCurrencies();

        [OperationContract]
        Dictionary<string, string> GetActiveDepositTypes();

        [OperationContract]
        Dictionary<string, string> GetEmbassyList(List<ushort> referenceTypes);

        [OperationContract]
        Dictionary<string, string> GetReferenceLanguages();

        [OperationContract]
        List<KeyValuePair<long, string>> GetReferenceTypes();

        [OperationContract]
        List<KeyValuePair<long, string>> GetFilialAddressList();

        [OperationContract]
        Dictionary<string, string> GetStatementFrequency();

        [OperationContract]
        Dictionary<string, string> GetTransferTypes(int isActive);

        [OperationContract]
        Dictionary<string, string> GetAllTransferTypes();

        [OperationContract]
        Dictionary<string, string> GetTransferCallQuality();

        [OperationContract]
        Dictionary<string, string> GetCashOrderTypes();

        [OperationContract]
        Dictionary<string, string> GetOrderTypes();

        [OperationContract]
        Dictionary<string, string> GetOrderQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetLoanQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetProblemLoanTaxQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetProblemLoanTaxCourtDecisionTypes();

        [OperationContract]
        Dictionary<string, string> GetCardSystemTypes();

        [OperationContract]
        Dictionary<string, string> GetCardReportReceivingTypes();

        [OperationContract]
        string GetCustomerLastMotherName(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetCardPINCodeReceivingTypes();

        [OperationContract]
        string TranslateArmToEnglish(string armString, bool isUnicode);

        [OperationContract]
        Dictionary<string, string> GetCardTechnologyTypes();

        [OperationContract]
        Dictionary<string, string> GetCardTypes(int cardSystem);

        [OperationContract]
        Dictionary<string, string> GetAllCardTypes(int cardSystem);

        [OperationContract]
        Dictionary<string, string> GetBankOperationFeeTypes(int type);

        [OperationContract]
        Dictionary<string, string> GetCardClosingReasons();

        [OperationContract]
        Dictionary<string, string> GetPeriodicsSubTypes(Languages language);

        [OperationContract]
        Dictionary<string, string> GetCommunalBranchList(CommunalTypes communalType, Languages language);

        [OperationContract]
        Dictionary<string, string> GetPeriodicUtilityTypes(Languages language);
        
        [OperationContract]
        Dictionary<string, string> GetCountries();

        [OperationContract]
        string GetCountyRiskQuality(string country);

        [OperationContract]
        Dictionary<string, string> GetCurrentAccountCurrencies();

        [OperationContract]
        Dictionary<string, string> GetDepositTypeCurrency(short depositType);

        [OperationContract]
        Dictionary<string, string> GetFilialList();

        [OperationContract]
        string GetInfoFromSwiftCode(string swiftCode, short type);

        [OperationContract]
        Dictionary<string, string> GetJointTypes();

        [OperationContract]
        Dictionary<string, string> GetLTACodes();

        [OperationContract]
        Dictionary<string, string> GetPoliceCodes(string accountNumber = "");

        [OperationContract]
        Dictionary<string, string> GetSyntheticStatuses();

        [OperationContract]
        Dictionary<string, string> GetOperationsList();

        [OperationContract]
        Dictionary<string, string> GetPeriodicityTypes();

        [OperationContract]
        Dictionary<string, string> GetStatementDeliveryTypes();

        [OperationContract]
        string GetSyntheticStatus(string value);

        [OperationContract]
        Dictionary<string, string> GetTransferSystemCurrency(int transfersystem);

        [OperationContract]
        Dictionary<string, string> GetTransitAccountTypes(bool forLoanMature);

        [OperationContract]
        Dictionary<string, string> SearchRelatedOfficeTypes(string officeId, string officeName);

        [OperationContract]
        string GetBank(int code, Languages language);

        [OperationContract]
        List<KeyValuePair<int, int>> GetAutoConfirmOrderTypes();

        [OperationContract]
        void Init(string clientIp, byte language);

        [OperationContract]
        Dictionary<string, string> GetAccountFreezeReasonsTypes();

        [OperationContract]
        Dictionary<string, string> GetDisputeResolutions();

        [OperationContract]
        Dictionary<string, string> GetCommunicationTypes();


        [OperationContract]
        void ClearAllCache();

        [OperationContract]
        Dictionary<string, string> GetServiceProvidedTypes();

        [OperationContract]
        Dictionary<string, string> GetOrderRemovingReasons();

        [OperationContract]
        bool IsWorkingDay(DateTime date);

        [OperationContract]
        List<Tuple<int, int, string, bool>> GetAssigneeOperationTypes(int groupId, int typeOfCustomer);

        [OperationContract]
        Dictionary<string, string> GetAssigneeOperationGroupTypes(int typeOfCustomer);

        [OperationContract]
        Dictionary<string, string> GetCredentialTypes(int typeOfCustomer, int customerFilialCode, int userFilialCode);

        [OperationContract]
        Dictionary<string, string> GetCredentialClosingReasons();

        [OperationContract]
        Dictionary<string, string> GetAccountClosingReasonsTypes();

        [OperationContract]
        List<ActionPermission> GetActionPermissionTypes();

        [OperationContract]
        Dictionary<string, string> GetReferenceTypesDict();

        [OperationContract]
        Dictionary<string, string> GetMonths();

        [OperationContract]
        KeyValuePair<string, string> GetCommunalDate(CommunalTypes cmnlType, short abonentType = 1);

        [OperationContract]
        List<KeyValuePair<ushort, string>> GetServicePaymentNoteReasons();

        [OperationContract]
        List<KeyValuePair<ushort, string>> GetTransferAmountPurposes();

        [OperationContract]
        List<KeyValuePair<ushort, string>> GetTransferSenderLivingPlaceTypes();

        [OperationContract]
        List<KeyValuePair<ushort, string>> GetTransferReceiverLivingPlaceTypes();

        [OperationContract]
        List<KeyValuePair<ushort, string>> GetTransferAmountTypes();


        [OperationContract]
        Dictionary<string, string> GetPensionAppliactionQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetPensionAppliactionServiceTypes();

        [OperationContract]
        Dictionary<string, string> GetPensionAppliactionClosingTypes();

        [OperationContract]
        Dictionary<string, string> GetCardsType();

        [OperationContract]
        Dictionary<string, string> GetOpenCardsType();

        [OperationContract]
        ulong GetLastKeyNumber(int keyId,ushort filialCode);

        [OperationContract]
        List<KeyValuePair<string, string>> GetCurNominals(string currency);

        [OperationContract]
        List<DateTime> GetWaterCoDebtDates(ushort code);

        [OperationContract]
        Dictionary<string, string> GetWaterCoBranches(ushort filialCode);

        [OperationContract]
        Dictionary<string, string> GetWaterCoCitys(ushort code);

        [OperationContract]
        List<WaterCoDetail> GetWaterCoDetails();
        [OperationContract]
        Dictionary<string, string> GetProvisionTypes();


        [OperationContract]
        Dictionary<int?, string> GetSMSMessagingStatusTypes();

        [OperationContract]
        Dictionary<string, string> GetInsuranceTypes();

        [OperationContract]
        Dictionary<string, string> GetInsuranceCompanies();

        [OperationContract]
        string GetCardDataChangeFieldTypeDescription(ushort type);

        [OperationContract]
        string GetCardRelatedOfficeName(ushort relatedOfficeNumber);

        [OperationContract]
        string GetCOWaterBranchID(string branch, string abonentFilialCode);

        [OperationContract]
        Dictionary<string, string> GetReestrWaterCoBranches(ushort filialCode);

        [OperationContract]
        Dictionary<string, string> GetDepositClosingReasonTypes();

        [OperationContract]
        Dictionary<string, string> GetAllDepositClosingReasonTypes(bool showinList);


        [OperationContract]
        Dictionary<string, string> GetSubTypesOfTokens();

        [OperationContract]
        Dictionary<string, string> GetPrintReportTypes();

        [OperationContract]
        Dictionary<string, string> GetTransferRejectReasonTypes();

        [OperationContract]
        Dictionary<string, string> GetTransferRequestStepTypes();

        [OperationContract]
        Dictionary<string, string> GetTransferRequestStatusTypes();

        [OperationContract]
        List<DepositOption> GetBusinesDepositOptions();

        [OperationContract]
        Dictionary<int, string> GetTransferSessions(DateTime dateStart, DateTime dateEnd, short transferGroup);

        [OperationContract]
        Dictionary<string, string> GetArmenianPlaces(int region);

        [OperationContract]
        Dictionary<string, string> GetRegions(int country);

        [OperationContract]
        Dictionary<string, string> GetTypeOfLoanRepaymentSource();

        [OperationContract]
        Dictionary<string, string> GetActiveDepositTypesForNewDepositOrder(int accountType, int customerType);

        [OperationContract]
        CardTariffAdditionalInformation GetCardTariffAdditionalInformation(int officeID, int cardType);

        [OperationContract]
        string GetDepositDataChangeFieldTypeDescription(ushort type);

        [OperationContract]
        Dictionary<string, string> GetInsuranceCompaniesByInsuranceType(ushort insuranceType);

        [OperationContract]
        Dictionary<string, string> GetInsuranceTypesByProductType(bool isLoanProduct, bool isSeparatelyProduct);


        [OperationContract]
        Dictionary<string, string> GetLoanTypes();

        [OperationContract]
        Dictionary<string, string> GetLoanMatureTypes();

        [OperationContract]
        List<KeyValuePair<int, string>> GetPhoneBankingContractQuestions();

        [OperationContract]
        Dictionary<string, string> GetCashBookRowTypes();

        [OperationContract]
        string GetExternalPaymentTerm(short id, string[] param, Languages language);

        [OperationContract]
        string GetExternalPaymentProductDetailCode(short id, Languages language);

        [OperationContract]
        string GetExternalPaymentStatusCode(short id, Languages language);

        [OperationContract]
        Dictionary<string, string> GetListOfLoanApplicationAmounts();

        [OperationContract]
        double GetFastOverdraftFeeAmount(double amount);

        [OperationContract]
        Dictionary<string, string> GetLoanMonitoringTypes();

        [OperationContract]
        Dictionary<string, string> GetLoanMonitoringFactorGroupes();

        [OperationContract]
        Dictionary<string, string> GetLoanMonitoringFactors(int loanType, int groupId);

        [OperationContract]
        Dictionary<string, string> GetProfitReductionTypes();

        [OperationContract]
        Dictionary<string, string> GetProvisionCostConclusionTypes();

        [OperationContract]
        Dictionary<string, string> GetProvisionQualityConclusionTypes();

        [OperationContract]
        Dictionary<string, string> GetLoanMonitoringConclusions();

        [OperationContract]
        Dictionary<string, string> GetLoanMonitoringSubTypes();

        [OperationContract]
        Dictionary<string, string> GetDemandDepositsTariffGroups();

        [OperationContract]
        Dictionary<string, string> GetAccountRestrictionGroups(ulong customerNumber);

        [OperationContract]
        double GetPenaltyRateOfLoans(int productType, DateTime startDate);

        [OperationContract]
        KeyValuePair<string, DateTime>? GetDemandDepositRateTariffDocument(byte documentType);


        [OperationContract]
        Dictionary<string, string> GetOrderQualityTypesForAcbaOnline();

        [OperationContract]
        Dictionary<string, string> GetProductNotificationInformationTypes();

        [OperationContract]
        Dictionary<string, string> GetProductNotificationFrequencyTypes(byte informationType);

        [OperationContract]
        Dictionary<string, string> GetProductNotificationOptionTypes(byte informationType);

        [OperationContract]
        Dictionary<string, string> GetProductNotificationLanguageTypes(byte informationType);

        [OperationContract]
        Dictionary<string, string> GetProductNotificationFileFormatTypes();

        [OperationContract]
        Dictionary<string, string> GetSwiftMessageTypes();

        [OperationContract]
        Dictionary<string, string> GetSwiftMessageSystemTypes();

        [OperationContract]
        Dictionary<string, string> GetSwiftMessagMtCodes();

        [OperationContract]
        Dictionary<string, string> GetSwiftMessageAttachmentExistenceTypes();



        [OperationContract]
        Dictionary<string, string> GetArcaCardSMSServiceActionTypes();
        

        [OperationContract]
        Dictionary<string, string> GetBondIssueQuality();

        [OperationContract]
        Dictionary<string, string> GetBondIssuerTypes();

        [OperationContract]
        Dictionary<string, string> GetBondIssuePeriodTypes();

        [OperationContract]
        DateTime GetFastOverdrafEndDate(DateTime startDate);

        [OperationContract]
        List<int> GetATSFilials();

        [OperationContract]
        Dictionary<string, string> GetBanks(Languages language);

        [OperationContract]
        Dictionary<string, string> GetBondRejectReasonTypes();

        [OperationContract]
        Dictionary<string, string> GetBondQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetTypeOfPaymentDescriptions();

        [OperationContract]
        Dictionary<string, string> GetTypeofPaymentReasonAboutOutTransfering();

        [OperationContract]
        Dictionary<string, string> GetTypeofOperDayClosingOptions();

        [OperationContract]
        Dictionary<string, string> GetTypeOf24_7Modes();
        
        [OperationContract]
        Dictionary<string,string> GetTypeOfCommunals();

        [OperationContract]
        Dictionary<string, string> GetActionsForCardTransaction();


        [OperationContract]
        Dictionary<string, string> GetCardLimitChangeOrderActionTypes();

        [OperationContract]
        Dictionary<string, string> GetUnFreezeReasonTypesForOrder(int freezeId);

        [OperationContract]
        double GetPenaltyRateForDate(DateTime date);

        [OperationContract]
        Dictionary<string, string> GetTypeOfDepositStatus();

        [OperationContract]
        Dictionary<string, string> GetTypeOfDeposit();

        [OperationContract]
        Dictionary<string, string> GetDepositCurrencies();

        [OperationContract]
        Dictionary<string, string> GetDepositTypes();



        [OperationContract]
        Dictionary<string, string> GetSwiftPurposeCode();

        [OperationContract]
        List<Shop> GetShopList();

        [OperationContract]
        Dictionary<string, string> GetSSTOperationTypes();

        [OperationContract]
        Dictionary<string, string> GetSSTerminals(ushort filialCode);

        [OperationContract]
        Dictionary<string, string> GetHBApplicationReportType();

        [OperationContract]
        Dictionary<string, string> GetCashOrderCurrencies();

        [OperationContract]
        Dictionary<string, string> GetReasonsForDigitalCardTransactionAction(bool useBank);

        [OperationContract]
        Dictionary<string, string> GetCreditLineMandatoryInstallmentTypes();


        [OperationContract]
        Dictionary<string, string> GetTransferMethod();

        [OperationContract]
        Dictionary<string, string> GetCurrenciesForReceivedFastTransfer();


        [OperationContract]
        Dictionary<string, string> GetCreditLineTypesForOnlineMobile(Languages language);

        [OperationContract]
        Dictionary<string, string> GetTemplateDocumentTypes();

        [OperationContract]
        Dictionary<string, string> GetAttachedCardTypes(string mainCardNumber);

        [OperationContract]
        int GetCardOfficeTypesForIBanking(ushort cardType, short periodicityType);

        [OperationContract]
        CardTariffContract GetCardTariffsByCardType(ushort cardType, short periodicityType);

        [OperationContract]
        double GetCardServiceFee(int cardType, int officeId, string currency);


        [OperationContract]
        Dictionary<string, string> GetAccountClosingReasonsHB();

        [OperationContract]
        List<KeyValuePair<byte, string>> GetUtilityAbonentTypes();

        [OperationContract]
        List<KeyValuePair<short, string>> GetUtilityPaymentTypesOnlineBanking();

        [OperationContract]
        List<KeyValuePair<short, string>> GetPayIfNoDebtTypes();

        [OperationContract]
        Dictionary<int, String> GetServiceFeePeriodocityTypes();

        [OperationContract]
        void InitOnline(string clientIp, byte language, SourceType sourceType, AuthorizedCustomer authorizedCustomer);

        [OperationContract]
        Dictionary<string, string> GetCurrenciesPlasticCardOrder(ushort cardType, short periodicityType);

        [OperationContract]
        Dictionary<string, string> GetLinkedCardTariffsByCardType(ushort cardType);

        [OperationContract]
        CardTariff GetAttachedCardTariffs(string mainCardNumber, uint cardType);

        [OperationContract]
        Dictionary<string, string> GetLoanMatureTypesForIBanking();

        [OperationContract]
        Dictionary<string, string> GetPlasticCardSmsServiceActions(string cardNumber);

        [OperationContract]
        Dictionary<string, string> GetTypeOfPlasticCardsSMS();

        [OperationContract]
        List<KeyValuePair<string, string>> GetDetailsOfCharges();

        [OperationContract]
        string GetFilialName(int filialCode);

        [OperationContract]
        Dictionary<string, string> GetDigitalOrderTypes(TypeOfHbProductTypes hbProductType);

        [OperationContract]
        Dictionary<string, string> GetOrderableCardSystemTypes();
        [OperationContract]
        string GetCustomerAddressEng(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetCardRemovalReasons();

        [OperationContract]
        Dictionary<string, string> GetInsuranceContractTypes(bool isLoanProduct, bool isSeparatelyProduct, bool isProvision);


        [OperationContract]
        Dictionary<string, string> GetInsuranceTypesByContractType(int insuranceContractType, bool isLoanProduct, bool isSeparatelyProduct, bool isProvision);

        //HB        
        [OperationContract]
        Dictionary<string, string> GetHBSourceTypes();

        [OperationContract]
        Dictionary<string, string> GetHBRejectTypes();

        [OperationContract]
        Dictionary<string, string> GetHBQualityTypes();

        [OperationContract]
        Dictionary<string, string> GetHBDocumentTypes();

        [OperationContract]
        Dictionary<string, string> GetHBDocumentSubtypes();

        [OperationContract]
        Dictionary<string, string> GetCallTransferRejectionReasons();
        [OperationContract]
        Dictionary<string, string> GetCardReceivingTypes();

        [OperationContract]
        Dictionary<string, string> GetAccountRestTypes();

        [OperationContract]
        Dictionary<byte, string> GetTransferReceiverTypes();

        [OperationContract]
        Dictionary<string, string> GetCardApplicationAcceptanceTypes();
  
		[OperationContract]
		Dictionary<string, string> GetVirtualCardStatusChangeReasons();

        [OperationContract]
        byte CommunicationTypeExistence(ulong customerNumber);
    
		[OperationContract]
		Dictionary<string, string> GetVirtualCardChangeActions(int status);

        [OperationContract]
        Dictionary<string, string> GetReasonsForCardTransactionAction();

        [OperationContract]
        Dictionary<string, string> GetAllReasonsForCardTransactionAction();

        [OperationContract]
        string GetCurrencyLetter(string currency, byte operationType);

        [OperationContract]
        Dictionary<string, string> GetARUSSexes(string authorizedUserSessionToken);

        [OperationContract]
        Dictionary<string, string> GetARUSYesNo(string authorizedUserSessionToken);

        [OperationContract]
        void InitForARUS(string clientIp, byte language, ExternalBanking.ACBAServiceReference.User user);

        [OperationContract]
        Dictionary<string, string> GetARUSDocumentTypes(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetARUSCountriesByMTO(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetARUSSendingCurrencies(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetARUSCitiesByCountry(string authorizedUserSessionToken, string MTOAgentCode, string countryCode);

        [OperationContract]
        Dictionary<string, string> GetARUSStates(string authorizedUserSessionToken, string MTOAgentCode, string countryCode);

        [OperationContract]
        Dictionary<string, string> GetARUSCitiesByState(string authorizedUserSessionToken, string MTOAgentCode, string countryCode, string stateCode);

        [OperationContract]
        Dictionary<string, string> GetARUSMTOList(string authorizedUserSessionToken);

        [OperationContract]
        Dictionary<string, string> GetCountriesWithA3();

        [OperationContract]
        string GetARUSDocumentTypeCode(int ACBADocumentTypeCode);

        [OperationContract]
        Dictionary<string, string> GetARUSCancellationReversalCodes(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetARUSPayoutDeliveryCodes(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetRemittancePurposes(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        Dictionary<string, string> GetMTOAgencies(string authorizedUserSessionToken, string MTOAgentCode, string countryCode, string cityCode, string currencyCode, string stateCode);

        [OperationContract]
        Dictionary<string, string> GetARUSAmendmentReasons(string authorizedUserSessionToken, string MTOAgentCode);

        [OperationContract]
        string GetMandatoryEntryInfo(byte id);

        [OperationContract]
        Dictionary<string, string> GetReferenceReceiptTypes();

        [OperationContract]
        Dictionary<string, string> GetCustomerAllPassports(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetCustomerAllPhones(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetSTAKPayoutDeliveryCodesByBenificiaryAgentCode(string authorizedUserSessionToken, string MTOAgentCode, string parent);

        [OperationContract]
        Dictionary<string, string> GetCardAdditionalDataTypes(string cardNumber, string expiryDate);

        [OperationContract]
        string GetSentCityBySwiftCode(string swiftCode);

        [OperationContract]
        Dictionary<string, string> GetCardNotRenewReasons();

        [OperationContract]
        Dictionary<string, string> GetReasonsForCardTransactionActionForUnblocking();

        [OperationContract]
        Dictionary<string, string> GetAllTypesOfPlasticCardsSMS();

        [OperationContract]
        List<string> GetCardMobilePhones(ulong customerNumber, ulong curdNumber);

        [OperationContract]
        string GetCurrentPhone(ulong curdNumber);

        [OperationContract]
        string SMSTypeAndValue(string curdNumber);

        [OperationContract]
        Dictionary<string, string> GetTypeOfLoanDelete();

        [OperationContract]
        bool IsCardOpen(string cardNumber);

        [OperationContract]
        Dictionary<string, string> GetJointDepositAvailableCurrencies(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetCommissionNonCollectionReasons();
    }
}
