using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ARUSDataService;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// ARUS համակարգից ստացված տեղեկատուներ
    /// </summary>
    public static class ARUSInfo
    {
        /// <summary>
        /// Վերադարձնում է սեռերի տեղեկատուն
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetSexes(string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_Sexes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {

                List<InfoResponse> sexes = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();

                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    sexes = client.GetSexes(ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in sexes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է Այո/Ոչ արժեքների տեղեկատուն
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetYesNo(string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_YesNo";

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> YesNoList = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    YesNoList = client.GetYesNo(ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in YesNoList)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO գործակալին համապատասխան փաստաթղթի տեսակների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetDocumentTypes(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_DocumentTypes_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> types = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    types = client.GetDocumentTypes(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in types)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ MTO-ին համապատասխան երկրների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetCountriesByMTO(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_Countries_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> countries = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    countries = client.GetCountriesByMTO(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in countries)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO-ին համապատասխան արժույթների ցանկը՝ ուղարկման գործողության համար հասանելի
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetSendingCurrencies(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            DataTable dt = new DataTable();

            List<InfoResponse> currencies = new List<InfoResponse>();
            string transactionCode = "";

            dt = new DataTable();
            dt.Columns.Add("code");
            dt.Columns.Add("name");

            ARUSHelper.Use(client =>
            {
                currencies = client.GetSendingCurrencies(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
            }, authorizedUserSessionToken, userName, clientIP);

            foreach (InfoResponse infoResponse in currencies)
            {
                dt.Rows.Add(infoResponse.code, infoResponse.name);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ երկրի քաղաքների ցանկը՝ տվյալ MTO գործակալին համապատասխան
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="countryCode">Երկրի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetCitiesByCountry(string MTOAgentCode, string countryCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_CitiesByCountry_" + MTOAgentCode + "_" + countryCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> cities = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    cities = client.GetCitiesByCountry(MTOAgentCode, countryCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in cities)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ երկրի մարզերի ցանկը՝ տվյալ MTO գործակալին համապատասխան
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="countryCode">Երկրի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetStates(string MTOAgentCode, string countryCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_States_" + MTOAgentCode + "_" + countryCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> states = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    states = client.GetStates(MTOAgentCode, countryCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in states)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ երկրի մարզերի ցանկը՝ տվյալ MTO գործակալին համապատասխան
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="countryCode">Երկրի կոդ</param>
        /// <param name="stateCode">Մարզի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetCitiesByState(string MTOAgentCode, string countryCode, string stateCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_Cities_" + MTOAgentCode + "_" + countryCode + "_" + stateCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> states = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    states = client.GetCitiesByState(MTOAgentCode, countryCode, stateCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in states)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է գործակալի տեսակի անվանումը՝ ըստ կոդի
        /// </summary>
        /// <param name="code">Գործակալի տեսակի կոդ</param>
        /// <returns></returns>
        public static string GetAgentTypeNameByCode(string code)
        {
            string name = "";
            switch (code)
            {
                case "AB":
                    name = "ArmenianBank (Հայկական Բանկ)";
                    break;
                case "PS":
                    name = "PSO (Վճարային Համակարգի Օպերատոր)";
                    break;
                case "MT":
                    name = "MTO (Դրամական Փոխանցման Համակարգի Օպերատոր)";
                    break;
                case "FB":
                    name = "ForeignBank (Օտարերկրյա Բանկ)";
                    break;
                case "CB":
                    name = "CBA (ՀՀ Կենտրոնական Բանկ)";
                    break;
                case "AR":
                    name = "ARUS (Դրամական Փոխանցումների Հայկական Միասնական Համակարգ)";
                    break;
                default:
                    break;
            }
            return name;
        }

        /// <summary>
        /// Փոխանցման ստացման տեսակի կոդեր
        /// </summary>
        public enum DeliveryTypeCode : short
        {
            /// <summary>
            /// Կանխիկ
            /// </summary>
            Cash = 10,
            /// <summary>
            /// Քարտ
            /// </summary>
            Card = 20,
            /// <summary>
            /// Էլեկտրոնային գումար
            /// </summary>
            EMoney = 30,
            /// <summary>
            /// Հաշվեհամար (Իրական ռեժիմով)
            /// </summary>
            AccountRealtime = 40,
            /// <summary>
            /// Հաշվեհամար (Ընդունման ռեժիմով)
            /// </summary>
            AccountAcceptance = 50,
            /// <summary>
            /// Հաշվեհամար (Ձեռքով)
            /// </summary>
            AccountManual = 60
        }

        /// <summary>
        /// Վերադարձնում է Դրամական Համակարգերի ցանկը
        /// </summary>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetMTOList(string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_MTO";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {

                List<InfoResponse> MTOList = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();

                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    MTOList = client.GetMTOList(ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in MTOList)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ MTO-ին համապատասխան փոխանցման նպատակների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetRemittancePurposes(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_RemittancePurposes_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> remittancePurposes = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    remittancePurposes = client.GetRemittancePurposes(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in remittancePurposes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ուղարկման/ստացման գործողությունների տեսակների տարանջատման տեղեկատուն
        /// </summary>
        /// <param name="MTOAgentCode">MTO գործակալի կոդ</param>
        /// <param name="authorizedUserSessionToken">Համակարգ մուտք գործած օգտագործողի սեսիայի թոքենի համար</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի IP</param>
        /// <returns></returns>
        public static DataTable GetSendPayoutDivisionCodes(string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_DivisionCodes";

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> remittancePurposes = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    remittancePurposes = client.GetSendPayoutDivisionCodes(ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in remittancePurposes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ARUS համակարգում համապատասխան փաստաթղթի տեսակի կոդը
        /// </summary>
        /// <param name="ACBADocumentTypeCode">ACBA փաստաթղթի տեսակի կոդ</param>
        /// <returns></returns>
        public static string GetARUSDocumentTypeCode(int ACBADocumentTypeCode)
        {
            string cacheKey = "ARUSInfo_DivisionCodes" + ACBADocumentTypeCode;

            string result = CacheHelper.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(result))
            {
                result = UtilityDB.GetARUSDocumentTypeCode(ACBADocumentTypeCode);
                CacheHelper.AddForSTAK(result, cacheKey);
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է չեղարկման/վերադարձման գործողությունների կոդերը
        /// (պատճառը մատնանշելու և որոշ դեպքերում վերադարձման միջնորդավճարի հաշվարկման համար)
        /// </summary>
        /// <param name="MTOAgentCode"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static DataTable GetCancellationReversalCodes(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_CancellationReversalCodes_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> remittancePurposes = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    remittancePurposes = client.GetCancellationReversalCodes(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in remittancePurposes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է վճարման եղանակի տեսակի կոդերը
        /// </summary>
        /// <param name="MTOAgentCode"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static DataTable GetPayoutDeliveryCodes(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_PayoutDeliveryCodes_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> remittancePurposes = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    remittancePurposes = client.GetPayoutDeliveryCodes(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in remittancePurposes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տրված MTO-ին հասանելի փոխանցման տրման Agent-ների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode">ARUS-ի կամ Դրամական Փոխանցման Օպերատորի Agent-ի կոդ</param>
        /// <param name="countryCode">Երկրի կոդ</param>
        /// <param name="cityCode">Քաղաքի կոդ</param>
        /// <param name="currencyCode">Արժույթի կոդ</param>
        /// <param name="stateCode">Մարզի կոդ(ոչ պարտադիր)</param>
        /// <param name="authorizedUserSessionToken">Վավերականացման տոկեն</param>
        /// <param name="userName">Օգտագործողի մուտքանուն</param>
        /// <param name="clientIP">Օգտագործողի համակարգչի IP</param>
        /// <returns></returns>
        public static DataTable GetMTOAgencies(string MTOAgentCode, string countryCode, string cityCode, string currencyCode, string stateCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_MTOAgencies_" + MTOAgentCode + "_" + countryCode + "_" + stateCode + "_" + cityCode + "_" + currencyCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> states = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    states = client.GetMTOAgencies(MTOAgentCode, countryCode, cityCode, stateCode, currencyCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in states)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է փոփոխման հայտի պատճառների ցանկը
        /// </summary>
        /// <param name="MTOAgentCode"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static DataTable GetAmendmentReasons(string MTOAgentCode, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_AmendmentReasons_" + MTOAgentCode;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {

                List<InfoResponse> amendmentReasons = new List<InfoResponse>();
                string transactionCode = "";

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");

                ARUSHelper.Use(client =>
                {
                    amendmentReasons = client.GetAmendmentReasonCodes(MTOAgentCode, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
                }, authorizedUserSessionToken, userName, clientIP);

                foreach (InfoResponse infoResponse in amendmentReasons)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է վճարման եղանակի տեսակի կոդերը` շահառուի գործակալի կոդով
        /// </summary>
        /// <param name="MTOAgentCode"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="parent"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public static DataTable GetPayoutDeliveryCodesByBenificiaryAgentCode(string MTOAgentCode, string parent, string authorizedUserSessionToken, string userName, string clientIP)
        {
            string cacheKey = "ARUSInfo_PayoutDeliveryCodesByBenificiaryAgentCode_" + MTOAgentCode + "_" + parent;

            DataTable dt = CacheHelper.Get(cacheKey);


            if (dt == null)
            {
                string transactionCode = "";
                List<InfoResponse> payoutDeliveryCodes = new List<InfoResponse>();

                dt = new DataTable();
                dt.Columns.Add("code");
                dt.Columns.Add("name");


                ARUSHelper.Use(client =>
                {
                    payoutDeliveryCodes = client.GetPayoutDeliveryCodesByBenificiaryAgentCode(ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode, MTOAgentCode, parent);
                }, authorizedUserSessionToken, userName, clientIP);


                foreach (InfoResponse infoResponse in payoutDeliveryCodes)
                {
                    dt.Rows.Add(infoResponse.code, infoResponse.name);
                }

                CacheHelper.AddForSTAK(dt, cacheKey);
            }

            return dt;
        }


    }
}
