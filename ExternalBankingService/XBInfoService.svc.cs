using System;
using System.Web;
using System.Collections.Generic;
using System.ServiceModel;
using ExternalBanking;
using System.Data;
using NLog;
using System.Collections;
using ExternalBankingService.Interfaces;
using System.Web.Configuration;
using NLog.Targets;
using System.Configuration;
using System.Linq;
using ExternalBanking.ServiceClient;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "XBInfoService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select XBInfoService.svc or XBInfoService.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class XBInfoService : IXBInfoService
    {

        string ClientIp { get; set; }

        byte Language { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }

        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        public Dictionary<string, string> GetCurrencies()
        {
            try
            {
                Dictionary<string, string> currencies = new Dictionary<string, string>();
                DataTable dt = Info.GetCurrencies();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    currencies.Add(dt.Rows[i]["currency"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return currencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetInternationalPaymentCurrencies()
        {
            try
            {
                Dictionary<string, string> currencies = new Dictionary<string, string>();
                DataTable dt = Info.GetInternationalPaymentCurrencies();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    currencies.Add(dt.Rows[i]["currency"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return currencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetActiveDepositTypes()
        {
            try
            {
                Dictionary<string, string> depositType = new Dictionary<string, string>();
                DataTable dt = Info.GetActiveDepositTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    depositType.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return depositType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetActiveDepositTypesForNewDepositOrder(int accountType, int customerType)
        {
            try
            {
                Dictionary<string, string> depositType = new Dictionary<string, string>();

                short allowableCustomerType = 0;
                short allowableForThirdhPerson = 0;
                short allowableForCooperative = 0;

                if (accountType == 2)
                {
                    allowableCustomerType = 1;
                    allowableForThirdhPerson = 1;
                    allowableForCooperative = 0;
                }
                else
                {
                    if (customerType == 6)
                    {
                        allowableCustomerType = 1;
                        allowableForCooperative = 1;
                    }
                    else
                    {
                        allowableCustomerType = 2;
                        allowableForCooperative = 0;
                    }
                }

                DataTable dt = Info.GetActiveDepositTypesForNewDepositOrder(allowableCustomerType, allowableForThirdhPerson, allowableForCooperative);


                DataRow[] dr = dt.Select("code=" + accountType.ToString());

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = Language == (byte)Languages.hy ? dt.Rows[i]["description"].ToString() : dt.Rows[i]["description_Engl"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    depositType.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return depositType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetEmbassyList(List<ushort> referenceTypes)
        {
            try
            {

                return Info.GetEmbassyList((Languages)Language, referenceTypes);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetReferenceLanguages()
        {
            try
            {
                return Info.GetReferenceLanguages((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<KeyValuePair<long, string>> GetReferenceTypes()
        {
            try
            {
                return Info.GetReferenceTypes((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetReferenceTypesDict()
        {
            try
            {
                List<KeyValuePair<long, string>> types = Info.GetReferenceTypes((Languages)Language);
                Dictionary<string, string> dict = new Dictionary<string, string>();

                types.ForEach(m =>
                {
                    dict.Add(m.Key.ToString(), m.Value);

                });


                return dict;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<KeyValuePair<long, string>> GetFilialAddressList()
        {

            try
            {
                return Info.GetFilialAddressList((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCashOrderTypes()
        {

            try
            {
                Dictionary<string, string> cashTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetCashOrderTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    cashTypes.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return cashTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }





        public Dictionary<string, string> GetTransferTypes(int isActive)
        {
            try
            {
                Dictionary<string, string> transferType = new Dictionary<string, string>();
                if (isActive == 1)
                {

                    DataTable dt = Info.GetActiveTransferTypes();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        transferType.Add(dt.Rows[i]["code"].ToString(), dt.Rows[i]["transfersystem"].ToString());
                    }
                    return transferType;
                }
                else
                {
                    DataTable dt = Info.GetTransferTypes();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        transferType.Add(dt.Rows[i]["code"].ToString(), dt.Rows[i]["transfersystem"].ToString());
                    }
                    return transferType;
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetAllTransferTypes()
        {
            try
            {
                Dictionary<string, string> transferType = new Dictionary<string, string>();

                DataTable dt = Info.GetAllTransferTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    transferType.Add(dt.Rows[i]["code"].ToString(), dt.Rows[i]["transfersystem"].ToString());
                }
                return transferType;


            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransferCallQuality()
        {
            try
            {
                Dictionary<string, string> transferQuality = new Dictionary<string, string>();
                DataTable dt = Info.GetTransferCallQuality();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    transferQuality.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return transferQuality;


            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetStatementFrequency()
        {

            try
            {
                Dictionary<string, string> frequency = new Dictionary<string, string>();
                DataTable dt = Info.GetStatementFrequency();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    frequency.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }
                return frequency;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetOrderTypes()
        {

            try
            {
                Dictionary<string, string> orderTypes = new Dictionary<string, string>();

                DataTable dt = Info.GetOrderTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()) : dt.Rows[i]["Description_eng"].ToString();
                    orderTypes.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return orderTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetOrderQualityTypes()
        {

            try
            {
                Dictionary<string, string> orderQualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetOrderQualityTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Language == (byte)Languages.hy ? dt.Rows[i]["description_arm"].ToString() : dt.Rows[i]["description_eng"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    orderQualityTypes.Add(dt.Rows[i]["quality"].ToString(), description);
                }
                return orderQualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanQualityTypes()
        {

            try
            {
                Dictionary<string, string> loanQualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanQualityTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    loanQualityTypes.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return loanQualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProblemLoanTaxQualityTypes()
        {

            try
            {
                Dictionary<string, string> problemLoanTaxQualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetProblemLoanTaxQualityTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    problemLoanTaxQualityTypes.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return problemLoanTaxQualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetProblemLoanTaxCourtDecisionTypes()
        {

            try
            {
                Dictionary<string, string> getProblemLoanTaxCourtDecisionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetProblemLoanTaxCourtDecisionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    getProblemLoanTaxCourtDecisionTypes.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return getProblemLoanTaxCourtDecisionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransferSystemCurrency(int transfersystem)
        {
            try
            {
                Dictionary<string, string> transferSystemCurrency = new Dictionary<string, string>();

                DataRow[] dr = Info.GetTransferSystemCurrency(transfersystem);

                for (int i = 0; i < dr.Length; i++)
                {

                    transferSystemCurrency.Add(dr[i]["currency"].ToString(), dr[i]["currency"].ToString());
                }
                return transferSystemCurrency;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetDepositTypeCurrency(short depositType)
        {
            try
            {
                return Info.GetDepositTypeCurrency(depositType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }


        public Dictionary<string, string> GetLTACodes()
        {

            try
            {
                Dictionary<string, string> LTACodes = new Dictionary<string, string>();
                DataTable dt = Info.GetLTACodes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString())
                        : dt.Rows[i]["description_engl"].ToString();
                    LTACodes.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return LTACodes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPoliceCodes(string accountNumber = "")
        {

            try
            {
                Dictionary<string, string> PoliceCodes = new Dictionary<string, string>();
                DataTable dt = Info.GetPoliceCodes(accountNumber);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["description_eng"].ToString();
                    PoliceCodes.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return PoliceCodes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetSyntheticStatuses()
        {

            try
            {
                Dictionary<string, string> SyntheticStatuses = new Dictionary<string, string>();
                DataTable dt = Info.GetSyntheticStatuses();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SyntheticStatuses.Add(dt.Rows[i]["value_for8"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_forHB"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }

                return SyntheticStatuses;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetSyntheticStatus(string value)
        {
            try
            {
                return Info.GetSyntheticStatus(value);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardSystemTypes()
        {
            try
            {
                Dictionary<string, string> cardSystemTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardSystemTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    cardSystemTypes.Add(infoTable.Rows[i]["code"].ToString(), infoTable.Rows[i]["description"].ToString());
                }

                return cardSystemTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardReportReceivingTypes()
        {
            try
            {
                Dictionary<string, string> cardReportReceivingTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardReportReceivingTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {

                    cardReportReceivingTypes.Add(infoTable.Rows[i]["type_id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(infoTable.Rows[i]["description"].ToString()) : Utility.ConvertAnsiToUnicode(infoTable.Rows[i]["description_eng"].ToString()));
                }

                return cardReportReceivingTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCustomerLastMotherName(ulong customerNumber)
        {
            try
            {
                string lastMotherName = "";
                PlasticCardOrder plasticCardOrder = new PlasticCardOrder();
                lastMotherName = plasticCardOrder.GetCustomerLastMotherName(customerNumber);
                return lastMotherName;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardPINCodeReceivingTypes()
        {
            try
            {
                Dictionary<string, string> cardPINCodeReceivingTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardPINCodeReceivingTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    cardPINCodeReceivingTypes.Add(infoTable.Rows[i]["type_id"].ToString(), description);
                }

                return cardPINCodeReceivingTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCardTechnologyTypes()
        {
            try
            {
                Dictionary<string, string> cardTechnologyTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardTechnologyTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    cardTechnologyTypes.Add(infoTable.Rows[i]["abbreviation"].ToString(), infoTable.Rows[i]["armenian"].ToString());
                }

                return cardTechnologyTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetCardTypes(int cardSystem)
        {
            try
            {
                Dictionary<string, string> cardTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    if (infoTable.Rows[i]["CardSystemID"].ToString() == cardSystem.ToString())
                    {
                        cardTypes.Add(infoTable.Rows[i]["ID"].ToString(), infoTable.Rows[i]["CardType"].ToString());
                    }

                }

                return cardTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAllCardTypes(int cardSystem)
        {
            try
            {
                Dictionary<string, string> openCardTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetAllCardTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    if (infoTable.Rows[i]["CardSystemID"].ToString() == cardSystem.ToString())
                    {
                        openCardTypes.Add(infoTable.Rows[i]["ID"].ToString(), infoTable.Rows[i]["CardType"].ToString());
                    }

                }

                return openCardTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> SearchRelatedOfficeTypes(string officeId, string officeName)
        {
            try
            {
                Dictionary<string, string> relatedOfficeTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.SearchRelatedOfficeTypes(officeId, officeName);

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    relatedOfficeTypes.Add(infoTable.Rows[i]["office_id"].ToString(), Utility.ConvertAnsiToUnicode(infoTable.Rows[i]["office_name"].ToString()));
                }

                return relatedOfficeTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPeriodicsSubTypes(Languages language)
        {
            try
            {
                Dictionary<string, string> periodicsSubTypes = new Dictionary<string, string>();
                DataTable dt = new DataTable();
                dt = Info.GetPeriodicsSubTypes(language);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    periodicsSubTypes.Add(dt.Rows[i]["Amount_Type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                }
                return periodicsSubTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCommunalBranchList(CommunalTypes communalType, Languages language)
        {
            try
            {
                Dictionary<string, string> type = new Dictionary<string, string>();

                if (communalType == CommunalTypes.Gas)
                {
                    type = Info.GetGasPromSectionCode();
                    return type;
                }

                DataTable dt = Info.GetCommunalBranchList(communalType, language);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    type.Add(dt.Rows[i]["ValueField"].ToString(), dt.Rows[i]["TextField"].ToString());
                }
                return type;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPeriodicUtilityTypes(Languages language)
        {
            try
            {
                Dictionary<string, string> periodicsSubTypes = new Dictionary<string, string>();
                DataTable dt = new DataTable();
                dt = Info.GetPeriodicUtilityTypes(language);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (language == Languages.hy)
                    {
                        periodicsSubTypes.Add(dt.Rows[i]["ID_Transfer"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                    }
                    else
                    {
                        periodicsSubTypes.Add(dt.Rows[i]["ID_Transfer"].ToString(), dt.Rows[i]["Description_Engl"].ToString());
                    }

                }
                return periodicsSubTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetFilialList()
        {

            try
            {
                return Info.GetFilialList((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardClosingReasons()
        {

            try
            {
                if (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
                    return Info.GetCardClosingReasonsForDigitalBanking((Languages)Language);
                else
                    return Info.GetCardClosingReasons((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetPeriodicityTypes()
        {

            try
            {
                Dictionary<string, string> PeriodicityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetPeriodicityTypes();
                string description = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if ((Languages)Language == Languages.eng)
                    {
                        description = dt.Rows[i]["description_eng"].ToString();
                    }
                    else
                    {
                        description = dt.Rows[i]["description"].ToString();
                    }
                    description = Utility.ConvertAnsiToUnicode(description);
                    PeriodicityTypes.Add(dt.Rows[i]["days_count"].ToString(), description);
                }
                return PeriodicityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetJointTypes()
        {

            try
            {
                Dictionary<string, string> jointTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetJointTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    jointTypes.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return jointTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetStatementDeliveryTypes()
        {

            try
            {
                Dictionary<string, string> statementTypes = new Dictionary<string, string>();

                DataTable dt = Info.GetStatementDeliveryTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["description_eng"].ToString();

                    statementTypes.Add(dt.Rows[i]["ID"].ToString(), description);

                }
                return statementTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetOperationsList()
        {

            try
            {
                return Info.GetOperationsList((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCountries()
        {
            try
            {
                Dictionary<string, string> countries = new Dictionary<string, string>();
                DataTable dt = Info.GetCountries();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    countries.Add(dt.Rows[i]["CountryCodeN"].ToString(), dt.Rows[i]["CountryName"].ToString());
                }
                return countries;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetInfoFromSwiftCode(string swiftCode, short type)
        {
            try
            {

                return Info.GetInfoFromSwiftCode(swiftCode, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }



        public string GetCountyRiskQuality(string country)
        {
            try
            {

                return Info.GetCountyRiskQuality(country);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Dictionary<string, string> GetBankOperationFeeTypes(int type)
        {
            try
            {
                Dictionary<string, string> bankOperationFeeTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetBankOperationFeeTypes(type);

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    bankOperationFeeTypes.Add(infoTable.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(infoTable.Rows[i]["description"].ToString()));
                }

                return bankOperationFeeTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountStatuses()
        {
            try
            {
                Dictionary<string, string> accountStatuses = new Dictionary<string, string>();

                DataTable dt = Info.GetAccountStatuses();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());

                    accountStatuses.Add(dt.Rows[i]["ID"].ToString(), description);
                }
                return accountStatuses;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransitAccountTypes(bool forLoanMature)
        {
            try
            {
                Dictionary<string, string> transitAccountTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetTransitAccountTypes(forLoanMature);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    transitAccountTypes.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return transitAccountTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCurrentAccountCurrencies()
        {
            try
            {
                Dictionary<string, string> currencies = new Dictionary<string, string>();
                DataTable dt = Info.GetCurrentAccountCurrencies();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    currencies.Add(dt.Rows[i]["currency"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return currencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetBank(int code, Languages language)
        {
            try
            {
                return Info.GetBank(code, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<KeyValuePair<int, int>> GetAutoConfirmOrderTypes()
        {

            List<KeyValuePair<int, int>> productTypes;
            try
            {
                productTypes = Info.GetAutoConfirmOrderTypes();
                return productTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="clientIp">IP որից եկել է հարցումը</param>
        public void Init(string clientIp, byte language)
        {
            try
            {
                ClientIp = clientIp;
                Language = language;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="clientIp">IP որից եկել է հարցումը</param>
        public void InitOnline(string clientIp, byte language, SourceType sourceType, AuthorizedCustomer authorizedCustomer)
        {
            try
            {
                ClientIp = clientIp;
                Language = language;
                AuthorizedCustomer = authorizedCustomer;
                Source = sourceType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public void WriteLog(Exception ex)
        {

            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingServiceInfo");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingServiceInfo-Test");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());

            if (Source == SourceType.Bank || Source == SourceType.PhoneBanking)
                GlobalDiagnosticsContext.Set("UserName", User.userName);
            else
            {
                GlobalDiagnosticsContext.Set("UserName", "");
            }

            if (ClientIp != null)
                GlobalDiagnosticsContext.Set("ClientIp", ClientIp);
            else
                GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message != null ? ex.Message : " ") + Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = ConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);
        }


        public Dictionary<string, string> GetAccountFreezeReasonsTypes()
        {
            Dictionary<string, string> reasonTypes;
            try
            {
                reasonTypes = Info.GetAccountFreezeReasonsTypes();
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetDisputeResolutions()
        {

            try
            {
                Dictionary<string, string> disputes = new Dictionary<string, string>();
                DataTable dt = Info.GetDisputeResolutions();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["dispute_description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    disputes.Add(dt.Rows[i]["id_dispute"].ToString(), description);
                }
                return disputes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCommunicationTypes()
        {

            try
            {
                Dictionary<string, string> commTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetCommunicationTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    commTypes.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return commTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public void ClearAllCache()
        {
            IDictionaryEnumerator enumerator = System.Web.HttpRuntime.Cache.GetEnumerator();
            List<string> keys = new List<string>();
            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key.ToString());
            }

            for (int i = 0; i < keys.Count; i++)
            {
                HttpRuntime.Cache.Remove(keys[i]);
            }
        }

        public Dictionary<string, string> GetServiceProvidedTypes()
        {
            Dictionary<string, string> types = new Dictionary<string, string>();

            try
            {
                DataTable dt = Info.GetServiceProvidedTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["Description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }

                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetOrderRemovingReasons()
        {

            try
            {
                Dictionary<string, string> removingTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetOrderRemovingReasons();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["reason_description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    removingTypes.Add(dt.Rows[i]["reason_id"].ToString(), description);
                }
                return removingTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsWorkingDay(DateTime date)
        {
            try
            {
                return Utility.IsWorkingDay(date);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Tuple<int, int, string, bool>> GetAssigneeOperationTypes(int groupId, int typeOfCustomer)
        {

            try
            {
                List<Tuple<int, int, string, bool>> opTypes = new List<Tuple<int, int, string, bool>>();
                DataTable dt = Info.GetAssigneeOperationTypes(groupId, typeOfCustomer);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["description_eng"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()) : string.Empty;
                    Tuple<int, int, string, bool> operType = new Tuple<int, int, string, bool>((int)dt.Rows[i]["Groupid"], (int)dt.Rows[i]["id"], description, Convert.ToBoolean(dt.Rows[i]["CanChangeAllAccounts"]));
                    opTypes.Add(operType);
                }
                return opTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAssigneeOperationGroupTypes(int typeOfCustomer)
        {

            try
            {
                Dictionary<string, string> commTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetAssigneeOperationGroupTypes(typeOfCustomer);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    commTypes.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }
                return commTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCredentialTypes(int typeOfCustomer, int customerFilialCode, int userFilialCode)
        {
            try
            {
                Dictionary<string, string> credentialTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetCredentialTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    credentialTypes.Add(dt.Rows[i]["id"].ToString(), description);
                }
                if (typeOfCustomer == (int)CustomerTypes.physical)
                {
                    credentialTypes.Remove("1");
                }
                return credentialTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCredentialClosingReasons()
        {
            try
            {
                Dictionary<string, string> credentialClosingReasons = new Dictionary<string, string>();
                DataTable dt = Info.GetCredentialClosingReasons();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    credentialClosingReasons.Add(dt.Rows[i]["id"].ToString(), description);
                }

                return credentialClosingReasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ActionPermission> GetActionPermissionTypes()
        {

            try
            {
                List<ActionPermission> actionTypes = new List<ActionPermission>();
                DataTable dt = Info.GetActionPermissionTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int ActionID = Int32.Parse(dt.Rows[i]["ActionID"].ToString());
                    string Description = dt.Rows[i]["Description"].ToString();

                    OrderType OrderType = OrderType.NotDefined;

                    if (Enum.IsDefined(typeof(OrderType), Convert.ToInt16(dt.Rows[i]["OrderType"].ToString())))
                    {
                        OrderType = (OrderType)Convert.ToInt16(dt.Rows[i]["OrderType"].ToString());
                    }


                    byte OrderSubType = Byte.Parse(dt.Rows[i]["OrderSubType"].ToString());

                    ActionPermission actType = new ActionPermission(ActionID, Description, OrderType, OrderSubType);
                    actionTypes.Add(actType);
                }
                return actionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountClosingReasonsTypes()
        {
            Dictionary<string, string> reasonTypes;
            try
            {
                reasonTypes = Info.GetAccountClosingReasonsTypes();
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetMonths()
        {

            try
            {
                Dictionary<string, string> months = new Dictionary<string, string>();
                DataTable dt = Info.GetMonths();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    months.Add(dt.Rows[i]["number"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["name"].ToString()));
                }
                return months;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public KeyValuePair<string, string> GetCommunalDate(CommunalTypes cmnlType, short abonentType = 1)
        {

            try
            {
                return SearchCommunal.GetCommunalDate(cmnlType, abonentType);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<KeyValuePair<ushort, string>> GetServicePaymentNoteReasons()
        {
            List<KeyValuePair<ushort, string>> noteReasons;
            try
            {
                noteReasons = Info.GetServicePaymentNoteReasons();
                return noteReasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<KeyValuePair<ushort, string>> GetTransferAmountPurposes()
        {
            List<KeyValuePair<ushort, string>> transferAmountPurposes;
            try
            {
                transferAmountPurposes = Info.GetTransferAmountPurposes();
                return transferAmountPurposes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<KeyValuePair<ushort, string>> GetTransferSenderLivingPlaceTypes()
        {
            List<KeyValuePair<ushort, string>> transferSenderLivingPlaceTypes;
            try
            {
                transferSenderLivingPlaceTypes = Info.GetTransferSenderLivingPlaceTypes();
                return transferSenderLivingPlaceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<KeyValuePair<ushort, string>> GetTransferReceiverLivingPlaceTypes()
        {
            List<KeyValuePair<ushort, string>> transferReceiverLivingPlaceTypes;
            try
            {
                transferReceiverLivingPlaceTypes = Info.GetTransferReceiverLivingPlaceTypes();
                return transferReceiverLivingPlaceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<KeyValuePair<ushort, string>> GetTransferAmountTypes()
        {
            List<KeyValuePair<ushort, string>> transferAmountTypes;
            try
            {
                transferAmountTypes = Info.GetTransferAmountTypes();
                return transferAmountTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPensionAppliactionQualityTypes()
        {

            try
            {
                Dictionary<string, string> qualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetPensionAppliactionQualityTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    qualityTypes.Add(dt.Rows[i]["quality"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["quality_description"].ToString()));
                }
                return qualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPensionAppliactionServiceTypes()
        {
            try
            {
                Dictionary<string, string> serviceTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetPensionAppliactionServiceTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //if (bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString()))
                    //{
                        if (Convert.ToBoolean(dt.Rows[i]["activ"]))
                            serviceTypes.Add(dt.Rows[i]["type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["type_description"].ToString()));
                    //}

                    //else
                    //{
                    //    serviceTypes.Add(dt.Rows[i]["type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["type_description"].ToString()));
                    //}

                }
                return serviceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPensionAppliactionClosingTypes()
        {

            try
            {
                Dictionary<string, string> closingTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetPensionAppliactionClosingTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    closingTypes.Add(dt.Rows[i]["closing_number"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["closing_description"].ToString()));
                }
                return closingTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardsType()
        {
            try
            {
                Dictionary<string, string> cardTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    cardTypes.Add(infoTable.Rows[i]["ID"].ToString(), infoTable.Rows[i]["CardType"].ToString());

                }

                return cardTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetOpenCardsType()
        {
            try
            {
                Dictionary<string, string> openCardsType = new Dictionary<string, string>();
                DataTable infoTable = Info.GetAllCardTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    openCardsType.Add(infoTable.Rows[i]["ID"].ToString(), infoTable.Rows[i]["CardType"].ToString());
                }

                return openCardsType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ulong GetLastKeyNumber(int keyId, ushort filialCode)
        {
            try
            {
                ulong keyNumber;

                keyNumber = Utility.GetLastKeyNumber(keyId, filialCode);

                return keyNumber;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<KeyValuePair<string, string>> GetCurNominals(string currency)
        {
            try
            {
                List<KeyValuePair<string, string>> curNominals = new List<KeyValuePair<string, string>>();
                DataTable dt = Info.GetCurNominals(currency);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    curNominals.Add(new KeyValuePair<string, string>(dt.Rows[i]["Banknot"].ToString(), dt.Rows[i]["Nominal"].ToString()));
                }
                return curNominals;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DateTime> GetWaterCoDebtDates(ushort code)
        {
            try
            {
                return SearchCommunal.GetWaterCoDebtDates(code);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Dictionary<string, string> GetWaterCoBranches(ushort filialCode)
        {
            try
            {
                return SearchCommunal.GetWaterCoBranches(filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Dictionary<string, string> GetReestrWaterCoBranches(ushort filialCode)
        {
            try
            {
                return SearchCommunal.GetReestrWaterCoBranches(filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Dictionary<string, string> GetWaterCoCitys(ushort code)
        {
            try
            {
                return SearchCommunal.GetWaterCoCitys(code);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public List<WaterCoDetail> GetWaterCoDetails()
        {
            try
            {
                return SearchCommunal.GetWaterCoDetails();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public Dictionary<string, string> GetProvisionTypes()
        {
            try
            {
                Dictionary<string, string> provisions = new Dictionary<string, string>();
                DataTable dt = Info.GetProvisionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    provisions.Add(dt.Rows[i]["Type_Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                }
                return provisions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<int?, string> GetSMSMessagingStatusTypes()
        {
            try
            {
                Dictionary<int?, string> smsMessagingStatusTypes = new Dictionary<int?, string>();
                DataTable dt = Info.GetSMSMessagingStatusTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    smsMessagingStatusTypes.Add(Convert.ToInt32(dt.Rows[i]["ID"]), dt.Rows[i]["description"].ToString());
                }
                return smsMessagingStatusTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetInsuranceTypes()
        {

            try
            {
                Dictionary<string, string> insuranceTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetInsuranceTypes();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    insuranceTypes.Add(dt.Rows[i]["insurance_type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return insuranceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetInsuranceCompanies()
        {

            try
            {
                Dictionary<string, string> insuranceCompanies = new Dictionary<string, string>();
                DataTable dt = Info.GetInsuranceCompanies();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    insuranceCompanies.Add(dt.Rows[i]["company_ID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["company_name"].ToString()));
                }
                return insuranceCompanies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetInsuranceCompaniesByInsuranceType(ushort insuranceType)
        {

            try
            {
                Dictionary<string, string> insuranceCompanies = new Dictionary<string, string>();
                DataTable dt = Info.GetInsuranceCompaniesByInsuranceType(insuranceType);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    insuranceCompanies.Add(dt.Rows[i]["company_ID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["company_name"].ToString()));
                }
                return insuranceCompanies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetInsuranceTypesByProductType(bool isLoanProduct, bool isSeparatelyProduct)
        {

            try
            {
                Dictionary<string, string> insuranceTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetInsuranceTypesByProductType(isLoanProduct, isSeparatelyProduct);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    insuranceTypes.Add(dt.Rows[i]["insurance_type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return insuranceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string GetCardDataChangeFieldTypeDescription(ushort type)
        {
            try
            {
                return Info.GetCardDataChangeFieldTypeDescription(type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public string GetCardRelatedOfficeName(ushort relatedOfficeNumber)
        {
            try
            {
                return Info.GetCardRelatedOfficeName(relatedOfficeNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public string GetCOWaterBranchID(string branch, string abonentFilialCode)
        {
            try
            {
                return SearchCommunal.GetCOWaterBranchID(branch, abonentFilialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public Dictionary<string, string> GetDepositClosingReasonTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetDepositClosingReasonTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    types.Add(dt.Rows[i]["reason_type"].ToString(), Language == (byte)Languages.hy
                        ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["reason_type_description"].ToString())
                        : dt.Rows[i]["reason_type_description_eng"].ToString());
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAllDepositClosingReasonTypes(bool showinList = true)
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetDepositClosingReasonTypes(showinList);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    types.Add(dt.Rows[i]["reason_type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["reason_type_description"].ToString()));
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPrintReportTypes()
        {
            try
            {
                Dictionary<string, string> reportTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetPrintReportTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reportTypes.Add(dt.Rows[i]["number"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return reportTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransferRejectReasonTypes()
        {
            try
            {
                Dictionary<string, string> reasonTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetTransferRejectReasonTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonTypes.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetTransferRequestStepTypes()
        {
            try
            {
                Dictionary<string, string> reasonTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetTransferRequestStepTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonTypes.Add(dt.Rows[i]["step_id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["step_description"].ToString()));
                }
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransferRequestStatusTypes()
        {
            try
            {
                Dictionary<string, string> reasonTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetTransferRequestStatusTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonTypes.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetTypeOfLoanRepaymentSource()
        {
            try
            {
                Dictionary<string, string> repaymentSourceTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeOfLoanRepaymentSource();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    repaymentSourceTypes.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return repaymentSourceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<DepositOption> GetBusinesDepositOptions()
        {
            try
            {
                return Deposit.GetBusinesDepositOptions(Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<int, string> GetTransferSessions(DateTime dateStart, DateTime dateEnd, short transferGroup)
        {
            try
            {
                Dictionary<int, string> sessions = new Dictionary<int, string>();
                DataTable dt = Info.GetTransferSessions(dateStart, dateEnd, transferGroup);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sessions.Add(Convert.ToInt32(dt.Rows[i]["session"]), dt.Rows[i]["session"].ToString());
                }
                return sessions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetRegions(int country)
        {
            try
            {
                Dictionary<string, string> regions = new Dictionary<string, string>();
                DataTable dt = Info.GetRegions(country);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    regions.Add(dt.Rows[i]["region"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["name"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["name_english"].ToString()));
                }
                return regions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetArmenianPlaces(int region)
        {
            try
            {
                Dictionary<string, string> places = new Dictionary<string, string>();
                DataTable dt = Info.GetArmenianPlaces(region);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    places.Add(dt.Rows[i]["number"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["description_english"].ToString());
                }
                return places;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardTariffAdditionalInformation GetCardTariffAdditionalInformation(int officeID, int cardType)
        {
            try
            {
                CardTariffAdditionalInformation cardTariffAdditionalInformation = new CardTariffAdditionalInformation();
                cardTariffAdditionalInformation = Info.GetCardTariffAdditionalInformation(officeID, cardType);
                return cardTariffAdditionalInformation;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetDepositDataChangeFieldTypeDescription(ushort type)
        {
            try
            {
                return Info.GetDepositDataChangeFieldTypeDescription(type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }


        public Dictionary<string, string> GetLoanTypes()
        {
            string cacheKey = "Info_LoanTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = Info.GetLoanTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            Dictionary<string, string> loanTypes = new Dictionary<string, string>();
            string description = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                loanTypes.Add(dt.Rows[i]["code"].ToString(), description);
            }
            return loanTypes;
        }
        public Dictionary<string, string> GetSubTypesOfTokens()
        {
            try
            {
                Dictionary<string, string> subTypesOfTokens = new Dictionary<string, string>();
                DataTable dt = Info.GetSubTypesOfTokens();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    subTypesOfTokens.Add(dt.Rows[i]["Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                }
                return subTypesOfTokens;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanMatureTypes()
        {
            try
            {
                Dictionary<string, string> matureTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMatureTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    matureTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return matureTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<KeyValuePair<int, string>> GetPhoneBankingContractQuestions()
        {

            try
            {
                return Info.GetPhoneBankingContractQuestions();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetCashBookRowTypes()
        {
            try
            {
                Dictionary<string, string> cashBookRowTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetCashBookRowTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashBookRowTypes.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["row_type"].ToString()));
                }
                return cashBookRowTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetExternalPaymentTerm(short id, string[] param, Languages language)
        {
            try
            {
                return Info.GetExternalPaymentTerm(id, param, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetExternalPaymentProductDetailCode(short id, Languages language)
        {
            try
            {
                return Info.GetExternalPaymentProductDetailCode(id, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetExternalPaymentStatusCode(short id, Languages language)
        {
            try
            {
                return Info.GetExternalPaymentStatusCode(id, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetListOfLoanApplicationAmounts()
        {
            try
            {
                Dictionary<string, string> listOfLoanApplicationAmounts = new Dictionary<string, string>();
                listOfLoanApplicationAmounts.Add("50000", "50,000.00 AMD");
                listOfLoanApplicationAmounts.Add("100000", "100,000.00 AMD");

                return listOfLoanApplicationAmounts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetFastOverdraftFeeAmount(double amount)
        {
            try
            {
                return LoanProductOrder.GetFastOverdraftFeeAmount(amount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetLoanMonitoringTypes()
        {
            try
            {
                Dictionary<string, string> monitoringTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMonitoringTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    monitoringTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return monitoringTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanMonitoringFactorGroupes()
        {
            try
            {
                Dictionary<string, string> factorGroupes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMonitoringFactorGroupes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    factorGroupes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return factorGroupes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanMonitoringFactors(int loanType, int groupId)
        {
            try
            {
                Dictionary<string, string> factors = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMonitoringFactors(loanType, groupId);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    factors.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return factors;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProfitReductionTypes()
        {
            try
            {
                Dictionary<string, string> reductionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetProfitReductionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reductionTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return reductionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProvisionCostConclusionTypes()
        {
            try
            {
                Dictionary<string, string> costConclusionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetProvisionCostConclusionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    costConclusionTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return costConclusionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProvisionQualityConclusionTypes()
        {
            try
            {
                Dictionary<string, string> qualityConclusionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetProvisionQualityConclusionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    qualityConclusionTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return qualityConclusionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanMonitoringConclusions()
        {
            try
            {
                Dictionary<string, string> monitoringConclusions = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMonitoringConclusions();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    monitoringConclusions.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return monitoringConclusions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLoanMonitoringSubTypes()
        {
            try
            {
                Dictionary<string, string> monitoringTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMonitoringSubTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    monitoringTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return monitoringTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetDemandDepositsTariffGroups()
        {
            try
            {
                Dictionary<string, string> demandDepositsTariffGroups = new Dictionary<string, string>();
                DataTable dt = Info.GetDemandDepositsTariffGroups();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    demandDepositsTariffGroups.Add(dt.Rows[i]["ID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return demandDepositsTariffGroups;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetAccountRestrictionGroups(ulong customerNumber)
        {
            try
            {
                Dictionary<string, string> accountRestrictionGroups = new Dictionary<string, string>();
                DataTable dt = Info.GetAccountRestrictionGroups();
                bool checkSocialSecurityAccount = Info.HasSocialSecurityAccount(customerNumber);
                var custType = ACBAOperationService.GetCustomerType(customerNumber);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if ((checkSocialSecurityAccount && Convert.ToInt32(dt.Rows[i]["code"]) == 118 && custType != 6) || (Convert.ToInt32(dt.Rows[i]["code"]) == 119 && custType == 6)) 
                        continue;
                    accountRestrictionGroups.Add(dt.Rows[i]["row"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["prod_descr"].ToString()));
                }
                return accountRestrictionGroups;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetPenaltyRateOfLoans(int productType, DateTime startDate)
        {
            try
            {
                return Info.GetPenaltyRateOfLoans(productType, startDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public KeyValuePair<string, DateTime>? GetDemandDepositRateTariffDocument(byte documentType)
        {
            try
            {
                return DemandDepositRate.GetDemandDepositRateTariffDocument(documentType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Dictionary<string, string> GetOrderQualityTypesForAcbaOnline()
        {

            try
            {
                Dictionary<string, string> orderQualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetOrderQualityTypesForAcbaOnline();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["action_description_arm"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    orderQualityTypes.Add(dt.Rows[i]["quality"].ToString(), description);
                }
                return orderQualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetProductNotificationInformationTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetProductNotificationInformationTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetProductNotificationFrequencyTypes(byte informationType)
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetProductNotificationFrequencyTypes(informationType);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProductNotificationOptionTypes(byte informationType)
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetProductNotificationOptionTypes(informationType);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetProductNotificationLanguageTypes(byte informationType)
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetProductNotificationLanguageTypes(informationType);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetProductNotificationFileFormatTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetProductNotificationFileFormatTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetSwiftMessageTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                types = Info.GetSwiftMessageTypes();
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetSwiftMessageSystemTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                types = Info.GetSwiftMessageSystemTypes();
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetSwiftMessagMtCodes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetSwiftMessagMtCodes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string description = dt.Rows[i]["description"] != DBNull.Value ? dt.Rows[i]["description"].ToString() : "";
                    description = Utility.ConvertAnsiToUnicode(description);
                    types.Add(dt.Rows[i]["MT"].ToString(), description);
                }
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetSwiftMessageAttachmentExistenceTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                types = Info.GetSwiftMessageAttachmentExistenceTypes();
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetArcaCardSMSServiceActionTypes()
        {
            try
            {
                Dictionary<string, string> actionTypes = Info.GetArcaCardSMSServiceActionTypes();
                return actionTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public Dictionary<string, string> GetBondIssueQuality()
        {
            try
            {
                Dictionary<string, string> bondIssueQuality = new Dictionary<string, string>();
                DataTable dt = Info.GetBondIssueQualities();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["Description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    bondIssueQuality.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return bondIssueQuality;


            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetBondIssuerTypes()
        {
            try
            {
                Dictionary<string, string> bondIssuerTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetBondIssuerTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    bondIssuerTypes.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return bondIssuerTypes;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetBondIssuePeriodTypes()
        {
            try
            {
                Dictionary<string, string> bondIssuePeriodTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetBondIssuePeriodTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    bondIssuePeriodTypes.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return bondIssuePeriodTypes;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DateTime GetFastOverdrafEndDate(DateTime startDate)
        {
            try
            {
                return Utility.GetFastOverdrafEndDate(startDate);


            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetBanks(Languages language)
        {
            try
            {
                Dictionary<string, string> banks = new Dictionary<string, string>();
                DataTable dt = Info.GetBanks();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["Description_Engl"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    banks.Add(dt.Rows[i]["code"].ToString(), description);
                }

                return banks;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<int> GetATSFilials()
        {
            try
            {
                List<int> list = new List<int>();
                DataTable dt = Info.GetATSFilials();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(int.Parse(dt.Rows[i]["filialcode"].ToString()));
                }
                return list;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetBondRejectReasonTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetBondRejectReasonTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    types.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetBondQualityTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetBondQualityTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description"].ToString();
                    types.Add(dt.Rows[i]["Id"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetTypeOfPaymentDescriptions()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeOfPaymentDescriptions();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    types.Add(dt.Rows[i]["type"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetTypeofPaymentReasonAboutOutTransfering()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeofPaymentReasonAboutOutTransfering();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    types.Add(dt.Rows[i]["type"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public Dictionary<string, string> GetTypeofOperDayClosingOptions()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeofOperDayClosingOptions();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["code_description"].ToString());
                    types.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTypeOf24_7Modes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();

                DataTable dt = Info.GetTypeOf24_7Modes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTypeOfCommunals()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeOfCommunals();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString());
                    types.Add(dt.Rows[i]["id"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetActionsForCardTransaction()
        {
            try
            {
                Dictionary<string, string> actionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetActionsForCardTransaction();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    actionTypes.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_arm"].ToString());
                }
                return actionTypes;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardLimitChangeOrderActionTypes()
        {
            try
            {
                Dictionary<string, string> CardLimitChangeOrderActionTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetCardLimitChangeOrderActionTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CardLimitChangeOrderActionTypes.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_arm"].ToString());
                }
                return CardLimitChangeOrderActionTypes;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetUnFreezeReasonTypesForOrder(int freezeId)
        {
            Dictionary<string, string> reasonTypes;
            try
            {
                reasonTypes = Info.GetUnFreezeReasonsTypesForOrder(freezeId);
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetDepositTypes()
        {
            try
            {
                Dictionary<string, string> deposits = new Dictionary<string, string>();
                DataTable dt = Info.GetDepositTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    deposits.Add(dt.Rows[i]["description"].ToString(), dt.Rows[i]["code"].ToString());
                }
                return deposits;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetDepositCurrencies()
        {
            try
            {
                Dictionary<string, string> currencies = new Dictionary<string, string>();
                DataTable dt = Info.GetDepositCurrencies();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    currencies.Add(dt.Rows[i]["currency"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return currencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTypeOfDepositStatus()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeOfDepositStatus();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    types.Add(dt.Rows[i]["status"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTypeOfDeposit()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTypeOfDeposit();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                    types.Add(dt.Rows[i]["code"].ToString(), description);
                }
                return types;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetPenaltyRateForDate(DateTime date)
        {
            try
            {
                return LoanProduct.GetPenaltyRateForDate(date);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetSwiftPurposeCode()
        {
            try
            {
                Dictionary<string, string> purposeCode = new Dictionary<string, string>();
                DataTable dt = Info.GetSwiftPurposeCode();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    purposeCode.Add(dt.Rows[i]["code"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_rus"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }
                return purposeCode;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        //public List<Shop> GetShopList()
        //{
        //    try
        //    {
        //        List<Shop> shopList = new List<Shop>();
        //        shopList = Info.GetShopList();
        //        return shopList;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }
        //}

        public Dictionary<string, string> GetSSTOperationTypes()
        {
            try
            {
                Dictionary<string, string> SSTOperationTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetSSTOperationTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SSTOperationTypes.Add(dt.Rows[i]["providerID"].ToString(), dt.Rows[i]["providerName"].ToString());
                }
                return SSTOperationTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetSSTerminals(ushort filialCode)
        {
            try
            {
                Dictionary<string, string> SSTerminals = new Dictionary<string, string>();

                DataTable dt = Info.GetSSTerminals(filialCode);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SSTerminals.Add(dt.Rows[i]["SSTerminalID"].ToString(), dt.Rows[i]["SSTerminalName"].ToString());
                }
                return SSTerminals;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCashOrderCurrencies()
        {
            try
            {
                Dictionary<string, string> cashOrderCurrencies = new Dictionary<string, string>();
                DataTable dt = Info.GetCashOrderCurrencies(AuthorizedCustomer.CustomerNumber);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashOrderCurrencies.Add(dt.Rows[i]["int_code"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return cashOrderCurrencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCreditLineMandatoryInstallmentTypes()
        {
            try
            {
                Dictionary<string, string> CreditLineMandatoryReceiptsType = Info.GetCreditLineMandatoryInstallmentTypesDescription((Languages)Language);
                return CreditLineMandatoryReceiptsType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTransferMethod()
        {
            try
            {
                Dictionary<string, string> TransferMethod = Info.GetTransferMethod();
                return TransferMethod;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetReasonsForDigitalCardTransactionAction(bool useBank)
        {
            try
            {
                Dictionary<string, string> reasonsForCardTransactionActions = new Dictionary<string, string>();
                DataTable dt = Info.GetReasonsForCardTransactionAction(useBank);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonsForCardTransactionActions.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }
                return reasonsForCardTransactionActions;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetReasonsForCardTransactionAction()
        {
            try
            {
                Dictionary<string, string> reasonsForCardTransactionActions = new Dictionary<string, string>();
                DataTable dt = Info.GetReasonsForCardTransactionAction(true);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonsForCardTransactionActions.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_arm"].ToString());
                }
                return reasonsForCardTransactionActions;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAllReasonsForCardTransactionAction()
        {
            try
            {
                Dictionary<string, string> reasonsForCardTransactionActions = new Dictionary<string, string>();
                DataTable dt = Info.GetReasonsForCardTransactionAction(true);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonsForCardTransactionActions.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_arm"].ToString());
                }
                return reasonsForCardTransactionActions;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCurrenciesForReceivedFastTransfer()
        {
            try
            {
                Dictionary<string, string> currencies = new Dictionary<string, string>();
                DataTable dt = Info.GetCurrenciesForReceivedFastTransfer();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    currencies.Add(dt.Rows[i]["currency"].ToString(), dt.Rows[i]["currency"].ToString());
                }
                return currencies;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public Dictionary<string, string> GetCreditLineTypesForOnlineMobile(Languages language)
        {
            try
            {
                Dictionary<string, string> creditLinesTypes = new Dictionary<string, string>();
                DataTable dt = new DataTable();
                dt = Info.GetCreditLineTypesForOnlineMobile(language);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (language == Languages.hy)
                    {
                        creditLinesTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                    }
                    else
                    {
                        creditLinesTypes.Add(dt.Rows[i]["code"].ToString(), dt.Rows[i]["Description_Engl"].ToString());
                    }

                }
                return creditLinesTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTemplateDocumentTypes()
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                DataTable dt = Info.GetTemplateDocumentTypes();

                if ((Languages)Language == Languages.hy)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        types.Add(dt.Rows[i]["document_type"].ToString(), dt.Rows[i]["description"].ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        types.Add(dt.Rows[i]["document_type"].ToString(), dt.Rows[i]["description_eng"].ToString());
                    }

                }

                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAttachedCardTypes(string mainCardNumber)
        {
            try
            {
                Dictionary<string, string> types = new Dictionary<string, string>();
                types = Info.GetAttachedCardTypes(mainCardNumber, AuthorizedCustomer.CustomerNumber);
                return types;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCardOfficeTypesForIBanking(ushort cardType, short periodicityType)
        {
            try
            {

                return Info.GetCardOfficeTypesForIBanking(cardType, periodicityType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardTariffContract GetCardTariffsByCardType(ushort cardType, short periodicityType)
        {
            try
            {
                CardTariffContract cardTarrif = new CardTariffContract();
                cardTarrif = CardTariffContract.GetCardTariffsByCardType(cardType, periodicityType);
                cardTarrif.StartDate = DateTime.Now;
                return cardTarrif;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCardServiceFee(int cardType, int officeId, string currency)
        {
            try
            {
                return Info.GetCardServiceFee(cardType, officeId, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountClosingReasonsHB()
        {
            try
            {
                DataTable dt = Info.GetAccountClosingReasonsHB();
                Dictionary<string, string> reasons = new Dictionary<string, string>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasons.Add(dt.Rows[i]["idx_closing"].ToString(), Language == (byte)Languages.hy ? dt.Rows[i]["decsription"].ToString() : dt.Rows[i]["Description_eng"].ToString());
                }
                return reasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Վերադարձնում է աբոնենտի տեսակները կոմունալ վճարման համար
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<byte, string>> GetUtilityAbonentTypes()
        {
            try
            {
                return Info.GetUtilityAbonentTypes((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<KeyValuePair<short, string>> GetUtilityPaymentTypesOnlineBanking()
        {
            try
            {
                return Info.GetUtilityPaymentTypesOnlineBanking((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալի պարբերականների դեպքում վճարման տեսակների ցանկը (Պարտքի առկայության դեպքում/Անկախ պարտքի առկայությունից)
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<short, string>> GetPayIfNoDebtTypes()
        {
            try
            {
                return Info.GetPayIfNoDebtTypes((Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<int, String> GetServiceFeePeriodocityTypes()
        {
            try
            {
                Dictionary<int, String> list = new Dictionary<int, String>();
                DataTable dt = Info.GetServiceFeePeriodocityTypes();
                if ((Languages)Language == Languages.hy)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        list.Add(Convert.ToInt32(dt.Rows[i]["ID"].ToString()), dt.Rows[i]["Description_arm"].ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        list.Add(Convert.ToInt32(dt.Rows[i]["ID"].ToString()), dt.Rows[i]["Description_eng"].ToString());
                    }

                }
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCurrenciesPlasticCardOrder(ushort cardType, short periodicityType)
        {
            try
            {
                return Info.GetCurrenciesPlasticCardOrder(cardType, periodicityType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public Dictionary<string, string> GetLinkedCardTariffsByCardType(ushort cardType)
        {
            try
            {
                return Info.GetLinkedCardTariffsByCardType(cardType, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public CardTariff GetAttachedCardTariffs(string mainCardNumber, uint cardType)
        {
            try
            {
                return CardTariffContract.GetAttachedCardTariffs(mainCardNumber, cardType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }
        public Dictionary<string, string> GetLoanMatureTypesForIBanking()
        {
            try
            {
                Dictionary<string, string> matureTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetLoanMatureTypesForIBanking();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    matureTypes.Add(dt.Rows[i]["id"].ToString(), Language == (byte)Languages.hy ? dt.Rows[i]["description_arm"].ToString() : dt.Rows[i]["description_eng"].ToString());
                }
                return matureTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPlasticCardSmsServiceActions(string cardNumber)
        {

            try
            {
                Dictionary<string, string> cashTypes = new Dictionary<string, string>();
                DataTable dt = PlasticCardSMSServiceOrder.GetPlasticCardSmsServiceActions();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashTypes.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));

                }

                var history = PlasticCardSMSServiceHistory.GetPlasticCardSMSServiceHistory(cardNumber);
                if (history.SMSType != 5 && history.SMSType != 6 && history.SMSType != 9)
                {
                    cashTypes.Remove("1");
                }
                else
                {
                    cashTypes.Remove("2");
                    cashTypes.Remove("3");
                }
                return cashTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Shop> GetShopList()
        {
            try
            {
                List<Shop> shopList = new List<Shop>();
                shopList = Info.GetShopList();
                return shopList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetHBApplicationReportType()
        {
            try
            {
                Dictionary<string, string> reportTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBApplicationReportType();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reportTypes.Add(dt.Rows[i]["id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }
                return reportTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string TranslateArmToEnglish(string armString, bool isUnicode)
        {
            try
            {
                return Utility.TranslateToEnglish(armString, isUnicode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetOrderableCardSystemTypes()
        {
            try
            {
                Dictionary<string, string> cardSystemTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetOrderableCardSystemTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    cardSystemTypes.Add(infoTable.Rows[i]["code"].ToString(), infoTable.Rows[i]["description"].ToString());
                }

                return cardSystemTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string GetCustomerAddressEng(ulong customerNumber)
        {
            try
            {
                string addressEng = "";
                PlasticCardOrder plasticCardOrder = new PlasticCardOrder();
                addressEng = plasticCardOrder.GetCustomerAddressEng(customerNumber);
                return addressEng;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCardRemovalReasons()
        {
            try
            {
                Dictionary<string, string> reasons = new Dictionary<string, string>();
                reasons = Info.GetCardRemovalReasons();
                return reasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetInsuranceContractTypes(bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            Dictionary<string, string> contrtype = new Dictionary<string, string>();
            try
            {
                contrtype = InsuranceOrder.GetInsuranceContractTypes(isLoanProduct, isSeparatelyProduct, isProvision);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

            return contrtype;
        }

        public Dictionary<string, string> GetInsuranceTypesByContractType(int insuranceContractType, bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            Dictionary<string, string> contrtype = new Dictionary<string, string>();
            try
            {
                contrtype = InsuranceOrder.GetInsuranceTypesByContractType(insuranceContractType, isLoanProduct, isSeparatelyProduct, isProvision);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

            return contrtype;
        }

        //HB
        public Dictionary<string, string> GetHBSourceTypes()
        {
            try
            {

                Dictionary<string, string> sourceTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBSourceTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sourceTypes.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }

                return sourceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Dictionary<string, string> GetHBQualityTypes()
        {
            try
            {
                Dictionary<string, string> qualityTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBQualityTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    qualityTypes.Add(dt.Rows[i]["quality"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString()));
                }

                return qualityTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Dictionary<string, string> GetHBDocumentTypes()
        {
            try
            {
                Dictionary<string, string> documentTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBDocumentTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    documentTypes.Add(dt.Rows[i]["product_type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }

                return documentTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetHBDocumentSubtypes()
        {
            try
            {
                Dictionary<string, string> documentSubtypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBDocumentSubtypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    documentSubtypes.Add(dt.Rows[i]["document_sub_type"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()));
                }

                return documentSubtypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Dictionary<string, string> GetHBRejectTypes()
        {
            try
            {
                Dictionary<string, string> rejectTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetHBRejectTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    rejectTypes.Add(dt.Rows[i]["for_orderby"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Reject_description"].ToString()));
                }

                return rejectTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Dictionary<string, string> GetCallTransferRejectionReasons()
        {
            try
            {
                return TransferByCallChangeOrder.GetCallTransferRejectionReasons();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }






        public Dictionary<string, string> GetTypeOfPlasticCardsSMS()
        {
            try
            {
                Dictionary<string, string> cashTypes = new Dictionary<string, string>();
                DataTable dt = PlasticCardSMSServiceOrder.GetTypeOfPlasticCardsSMS();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashTypes.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[1].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[2].ToString()));

                }
                return cashTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<KeyValuePair<string, string>> GetDetailsOfCharges()
        {
            try
            {
                return Info.GetDetailsOfCharges();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetFilialName(int filialCode)
        {
            try
            {
                return Info.GetFilialName(filialCode, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetDigitalOrderTypes(TypeOfHbProductTypes hbProductType)
        {
            try
            {
                Dictionary<string, string> orderTypes = new Dictionary<string, string>();

                DataTable dt = Info.GetOrderTypes();
                DataRow[] dr;
                if (hbProductType != TypeOfHbProductTypes.None)
                    dr = dt.Select("is_hb_product = 1 AND hb_product_type = " + ((int)hbProductType).ToString());
                else
                    dr = dt.Select("is_hb_product = 1 ");

                if (hbProductType == TypeOfHbProductTypes.Payment)
                    orderTypes.Add("1", Language == (byte)Languages.hy ? "Ճանապարհային ոստիկանություն" : "Road police penalties");

                for (int i = 0; i < dr.Length; i++)
                    orderTypes.Add(dr[i]["Id"].ToString(), Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dr[i]["Description"].ToString()) : dr[i]["Description_eng"].ToString());

                return orderTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetAccountRestTypes()
        {
            try
            {
                Dictionary<string, string> rejectTypes = new Dictionary<string, string>();
                DataTable dt = Info.GetAccountRestTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    rejectTypes.Add(dt.Rows[i]["id"].ToString(), Language == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
                }

                return rejectTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<byte, string> GetTransferReceiverTypes()
        {
            try
            {
                Dictionary<byte, string> ReceiverTypes = new Dictionary<byte, string>();
                DataTable dt = Info.GetTransferReceiverTypes();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ReceiverTypes.Add(Convert.ToByte(dt.Rows[i]["id"].ToString()), Language == (byte)Languages.hy ? dt.Rows[i]["description_arm"].ToString() : dt.Rows[i]["description_eng"].ToString());
                }

                return ReceiverTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte CommunicationTypeExistence(ulong customerNumber)
        {
            try
            {
                return Info.CommunicationTypeExistence(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetVirtualCardChangeActions(int status)
        {
            try
            {
                Dictionary<string, string> statuses = new Dictionary<string, string>();
                DataTable infoTable = Info.GetVirtualCardChangeActions(status);

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    statuses.Add(infoTable.Rows[i]["code"].ToString(), description);
                }

                return statuses;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCardReceivingTypes()
        {
            try
            {
                Dictionary<string, string> cardReceivingTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardReceivingTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    cardReceivingTypes.Add(infoTable.Rows[i]["type_id"].ToString(), description);
                }

                return cardReceivingTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardApplicationAcceptanceTypes()
        {
            try
            {
                Dictionary<string, string> cardApplicationAcceptanceTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardApplicationAcceptanceTypes();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    cardApplicationAcceptanceTypes.Add(infoTable.Rows[i]["type_id"].ToString(), description);
                }

                return cardApplicationAcceptanceTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetVirtualCardStatusChangeReasons()
        {
            try
            {
                Dictionary<string, string> changeReasons = new Dictionary<string, string>();
                DataTable infoTable = Info.GetVirtualCardStatusChangeReasons();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    changeReasons.Add(infoTable.Rows[i]["code"].ToString(), description);
                }

                return changeReasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCurrencyLetter(string currency, byte operationType)
        {
            string serialNumber = "";

            serialNumber = Info.GetCurrencyLetter(currency);
            serialNumber += operationType == 2 || operationType == 3 ? "Վ" : "Գ";

            return serialNumber;
        }

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="clientIp"></param>
        /// <param name="language"></param>
        /// <param name="user"></param>
        public void InitForARUS(string clientIp, byte language, ExternalBanking.ACBAServiceReference.User user)
        {
            try
            {
                ClientIp = clientIp;
                Language = language;
                User = user;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է սեռերի տեղեկատուն
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <returns></returns>
        public Dictionary<string, string> GetARUSSexes(string authorizedUserSessionToken)
        {
            try
            {
                Dictionary<string, string> sexes = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetSexes(authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sexes.Add(dt.Rows[i][0].ToString(), dt.Rows[i][0].ToString() == "F" ? "Իգական" : (dt.Rows[i][0].ToString() == "M" ? "Արական" : dt.Rows[i][1].ToString()));
                }
                return sexes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է Այո/Ոչ արժեքների տեղեկատուն
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <returns></returns>
        public Dictionary<string, string> GetARUSYesNo(string authorizedUserSessionToken)
        {
            try
            {
                Dictionary<string, string> yesNoList = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetYesNo(authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    yesNoList.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString() == "Yes" ? "Այո" : (dt.Rows[i][1].ToString() == "No" ? "Ոչ" : dt.Rows[i][1].ToString()));
                }
                return yesNoList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO գործակալին համապատասխան փաստաթղթի տեսակների ցանկը
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <returns></returns>
        public Dictionary<string, string> GetARUSDocumentTypes(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> yesNoList = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetDocumentTypes(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    yesNoList.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return yesNoList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO-ին համապատասխան երկրների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <returns></returns>
        public Dictionary<string, string> GetARUSCountriesByMTO(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> countries = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetCountriesByMTO(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    countries.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return countries;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO-ին համապատասխան արժույթների ցանկը՝ ուղարկման գործողության համար հասանելի
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <returns></returns>
        public Dictionary<string, string> GetARUSSendingCurrencies(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> countries = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetSendingCurrencies(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    countries.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return countries;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSCitiesByCountry(string authorizedUserSessionToken, string MTOAgentCode, string countryCode)
        {
            try
            {
                Dictionary<string, string> cities = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetCitiesByCountry(MTOAgentCode, countryCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cities.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return cities;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSStates(string authorizedUserSessionToken, string MTOAgentCode, string countryCode)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetStates(MTOAgentCode, countryCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSCitiesByState(string authorizedUserSessionToken, string MTOAgentCode, string countryCode, string stateCode)
        {
            try
            {
                Dictionary<string, string> cities = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetCitiesByState(MTOAgentCode, countryCode, stateCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cities.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return cities;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSMTOList(string authorizedUserSessionToken)
        {
            try
            {
                Dictionary<string, string> MTOList = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetMTOList(authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MTOList.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return MTOList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetCountriesWithA3()
        {
            try
            {
                Dictionary<string, string> countries = new Dictionary<string, string>();
                DataTable dt = Info.GetCountries();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    countries.Add(dt.Rows[i]["CountryCodeN"].ToString(), dt.Rows[i]["CountryCodeA3"].ToString());
                }
                return countries;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetARUSDocumentTypeCode(int ACBADocumentTypeCode)
        {
            try
            {
                return ARUSInfo.GetARUSDocumentTypeCode(ACBADocumentTypeCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> GetARUSCancellationReversalCodes(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetCancellationReversalCodes(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSPayoutDeliveryCodes(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetPayoutDeliveryCodes(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetRemittancePurposes(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetRemittancePurposes(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetMTOAgencies(string authorizedUserSessionToken, string MTOAgentCode, string countryCode, string cityCode, string currencyCode, string stateCode)
        {
            try
            {
                Dictionary<string, string> cities = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetMTOAgencies(MTOAgentCode, countryCode, cityCode, currencyCode, stateCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cities.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return cities;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetARUSAmendmentReasons(string authorizedUserSessionToken, string MTOAgentCode)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetAmendmentReasons(MTOAgentCode, authorizedUserSessionToken, User.userName, ClientIp);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetMandatoryEntryInfo(byte id)
        {
            try
            {
                return Info.GetMandatoryEntryInfo(id, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetReferenceReceiptTypes()
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                DataTable dt = Info.GetReferenceReceiptTypes(Language);

                foreach (DataRow item in dt.Rows)
                    result.Add(item["id"].ToString(), item["description"].ToString());

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCustomerAllPassports(ulong customerNumber)
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                DataTable dt = Info.GetCustomerAllPassports(customerNumber);

                foreach (DataRow item in dt.Rows)
                {
                    string documentGivenDate = Convert.ToDateTime(item["document_given_date"]).ToString("dd/MM/yyyy");
                    result.Add(item["document_number"].ToString() + ", " + item["document_given_by"].ToString() + ", " + documentGivenDate, item["document_number"].ToString() + ", " + item["document_given_by"].ToString() + ", " + documentGivenDate);
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCustomerAllPhones(ulong customerNumber)
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                DataTable dt = Info.GetCustomerAllPhones(customerNumber);

                foreach (DataRow item in dt.Rows)
                    result.Add(item["phoneNumber"].ToString(), item["phoneNumber"].ToString());

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetSTAKPayoutDeliveryCodesByBenificiaryAgentCode(string authorizedUserSessionToken, string MTOAgentCode, string parent)
        {
            try
            {
                Dictionary<string, string> states = new Dictionary<string, string>();
                DataTable dt = ARUSInfo.GetPayoutDeliveryCodesByBenificiaryAgentCode(MTOAgentCode, parent, authorizedUserSessionToken, User.userName, ClientIp);


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    states.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                }
                return states;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetCardAdditionalDataTypes(string cardNumber, string expiryDate)
        {
            try
            {
                Dictionary<string, string> CardAdditionalDataTypes = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardAdditionalDataTypes(cardNumber, expiryDate);

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["AdditionDescription"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    CardAdditionalDataTypes.Add(infoTable.Rows[i]["additionID"].ToString(), description);
                }

                return CardAdditionalDataTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetSentCityBySwiftCode(string swiftCode)
        {
            try
            {
                return Info.GetSentCityBySwiftCode(swiftCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardNotRenewReasons()
        {
            try
            {
                Dictionary<string, string> cardNotRenewReasons = new Dictionary<string, string>();
                DataTable infoTable = Info.GetCardNotRenewReasons();

                for (int i = 0; i < infoTable.Rows.Count; i++)
                {
                    string description = infoTable.Rows[i]["description"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);

                    cardNotRenewReasons.Add(infoTable.Rows[i]["id"].ToString(), description);
                }

                return cardNotRenewReasons;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetReasonsForCardTransactionActionForUnblocking()
        {
            try
            {
                Dictionary<string, string> reasonsForCardTransactionActions = new Dictionary<string, string>();
                DataTable dt = Info.GetReasonsForCardTransactionActionForUnblocking();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reasonsForCardTransactionActions.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_arm"].ToString());
                }
                return reasonsForCardTransactionActions;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<string, string> GetAllTypesOfPlasticCardsSMS()
        {
            try
            {
                Dictionary<string, string> cashTypes = new Dictionary<string, string>();
                DataTable dt = PlasticCardSMSServiceOrder.GetAllTypesOfPlasticCardsSMS();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashTypes.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[1].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[2].ToString()));

                }
                return cashTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<string> GetCardMobilePhones(ulong customerNumber, ulong curdNumber)
        {
            try
            {
                List<string> list = new List<string>();
                DataTable dt = PlasticCardSMSServiceOrder.GetCardMobilePhones(customerNumber, curdNumber);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    list.Add(dt.Rows[i]["phone"].ToString());

                }
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCurrentPhone(ulong curdNumber)
        {
            try
            {
                return PlasticCardSMSServiceOrder.GetCurrentPhone(curdNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string SMSTypeAndValue(string curdNumber)
        {
            try
            {
                return PlasticCardSMSServiceOrder.SMSTypeAndValue(curdNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetTypeOfLoanDelete()
        {
            Dictionary<string, string> reasonTypes;
            try
            {
                reasonTypes = Info.GetTypeOfLoanDelete();
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCardOpen(string cardNumber)
        {
            try
            {
                return CardReOpenOrder.IsCardOpen(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

    }
}
