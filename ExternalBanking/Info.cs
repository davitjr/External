using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ExternalBanking
{
    public static class Info
    {
        /// <summary>
        /// Ավանդների տեսակներ
        /// </summary>
        public static DataTable GetDepositTypes()
        {

            string cacheKey = "Info_DepositTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDepositTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Գործող ավանդների տեսակներ
        /// </summary>
        public static DataTable GetActiveDepositTypes()
        {

            string cacheKey = "Info_ActiveDepositTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetActiveDepositTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Վերադարձնում է նոր ավանդի հայտի ժամանակ առաջարկվող ավանդատեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetActiveDepositTypesForNewDepositOrder(short allowableCustomerType, short allowableForThirdhPerson, short allowableForCooperative)
        {
            DataTable dt = InfoDB.GetActiveDepositTypesForNewDepositOrder(allowableCustomerType, allowableForThirdhPerson, allowableForCooperative);
            return dt;
        }

        /// <summary>
        /// Արժույթները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCurrencies()
        {

            string cacheKey = "Info_Currencies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCurrencies();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static string AccountTypeDescription(ushort accountType, Languages language)
        {
            string result = null;

            DataTable dt = AccountTypeTypes();
            DataRow[] dr = dt.Select("code=" + accountType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["DescriptionEng"].ToString();
                }
            }
            return result;
        }

        private static DataTable AccountTypeTypes()
        {
            string cacheKey = "Info_AccountTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetDepositTypeDescription(short depositType, Languages language)
        {
            string result = null;

            DataTable dt = GetDepositTypes();
            DataRow[] dr = dt.Select("code=" + depositType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Վարկերի տեսակներ
        /// </summary>
        public static DataTable GetLoanTypes()
        {

            string cacheKey = "Info_LoanTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanTypeDescription(short loanType, Languages language)
        {
            string result = null;

            DataTable dt = GetLoanTypes();
            DataRow[] dr = dt.Select("code=" + loanType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Վարկերի կարգավիճակներ
        /// </summary>
        public static DataTable GetLoanQualityTypes()
        {

            string cacheKey = "Info_LoanQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Խնդրահարույց վարկերի պետ. տուրքերի կարգավիճակներ
        /// </summary>
        public static DataTable GetProblemLoanTaxQualityTypes()
        {

            string cacheKey = "Info_ProblemLoanTaxQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProblemLoanTaxQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Համաձայն դատարանի որոշման ում կողմից է մարվում խնդրահարույց վարկի պետ. տուրքը
        /// </summary>
        public static DataTable GetProblemLoanTaxCourtDecisionTypes()
        {

            string cacheKey = "Info_ProblemLoanTaxCourtDecisionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProblemLoanTaxCourtDecisionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanQualityTypeDescription(short qualityType, Languages language)
        {
            string result = null;

            DataTable dt = GetLoanQualityTypes();
            DataRow[] dr = dt.Select("code=" + qualityType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Պարբերական փոխանցումների տեսակներ
        /// </summary>
        public static DataTable GetPeriodicTransferTypes()
        {

            string cacheKey = "Info_PeriodicTransferTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPeriodicTransferTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetPeriodicTransferTypeDescription(short type, Languages language)
        {
            string result = null;

            DataTable dt = GetPeriodicTransferTypes();
            DataRow[] dr = dt.Select("ID_Transfer=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Պարբերական փոխանցումների վերջնաժամկետի տեսակներ
        /// </summary>
        public static DataTable GetPeriodicTransferDurationTypes()
        {

            string cacheKey = "Info_PeriodicTransferDurationTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPeriodicTransfersDurationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetPeriodicTransferDurationTypeDescription(short durationType, Languages language)
        {
            string result = null;

            DataTable dt = GetPeriodicTransferDurationTypes();
            DataRow[] dr = dt.Select("id=" + durationType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Eng"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Պարբերական փոխանցումների գանձման տեսակներ
        /// </summary>
        public static DataTable GetPeriodicTransferChargeTypes()
        {

            string cacheKey = "Info_PeriodicTransferChargeTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPeriodicTransfersChargeTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetPeriodicTransferChargeTypeDescription(short chargeType, Languages language)
        {
            string result = null;

            DataTable dt = GetPeriodicTransferChargeTypes();
            DataRow[] dr = dt.Select("id=" + chargeType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Eng"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Հանձնարարականի տեսակներ
        /// </summary>
        public static DataTable GetOrderSubTypes()
        {

            string cacheKey = "Info_OrderSubTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderSubTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Հայտերի տեսակներ
        /// </summary>
        public static DataTable GetOrderTypes()
        {

            string cacheKey = "Info_OrderTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetOrderSubTypeDescription(OrderType orderType, short orderSubType, Languages language)
        {
            string result = null;

            DataTable dt = GetOrderSubTypes();
            if (orderType == OrderType.Convertation || orderType == OrderType.CashConvertation || orderType == OrderType.CashCreditConvertation || orderType == OrderType.CashDebitConvertation || orderType == OrderType.TransitCashOutCurrencyExchangeOrder || orderType == OrderType.TransitNonCashOutCurrencyExchangeOrder)
            {
                if (orderSubType == 1)
                    orderSubType = 2;
                else if (orderSubType == 2)
                    orderSubType = 1;
            }


            DataRow[] dr = dt.Select("document_type=" + ((short)orderType).ToString() + " and document_sub_type=" + ((orderType == OrderType.FastTransferPaymentOrder || orderType == OrderType.ReceivedFastTransferPaymentOrder || orderType == OrderType.FastTransferFromCustomerAccount) ? "1" : orderSubType.ToString()));

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description_arm"].ToString());
                }
                else
                {
                    result = dr[0]["description_eng"].ToString();
                }
            }
            return result;
        }


        public static DataTable GetOrderQualityTypes()
        {

            string cacheKey = "Info_OrderQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetOrderQualityTypeDescription(short quality, Languages language)
        {
            string result = null;

            DataTable dt = GetOrderQualityTypes();

            if (quality == 3)
                quality = 2;

            DataRow[] dr = dt.Select("quality=" + quality.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description_arm"].ToString());
                }
                else
                {
                    result = dr[0]["description_eng"].ToString();
                }
            }
            return result;
        }

        public static DataTable GetCommunalDetailsTypes()
        {

            string cacheKey = "Info_CommunalDetailsTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCommunalDetailsTypes();
                CacheHelper.Add(dt, cacheKey);
            }



            return dt;
        }

        public static string GetCommunalDetailTypeDescription(int type, Languages language)
        {
            string result = null;
            if (type == 0)
                return "Operator Name";
            DataTable dt = GetCommunalDetailsTypes();
            DataRow[] dr = dt.Select(" id=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["description_eng"].ToString();
                }
            }

            return result;
        }



        /// <summary>
        /// Վերադարձնում է HB սխալների (արտահայտությունների  նկարագրությունները)
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTerms()
        {
            string cacheKey = "Info_Terms";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTerms();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTerm(short id, string[] param, Languages language)
        {
            string result = null;
            DataTable dt = GetTerms();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("id=" + id.ToString());
                if (dr.Length > 0)
                {
                    if (language == Languages.hy)
                    {
                        result = dr[0]["Description"].ToString();
                    }
                    else
                    {
                        result = dr[0]["Description_Eng"].ToString();
                    }

                    if (param != null)
                    {
                        result = string.Format(result, param);
                    }
                }
            }
            return result;
        }


        public static DataTable GetCreditLineTypes()
        {

            string cacheKey = "Info_CreditLineTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCreditLineTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetCreditLineTypeDescription(int type, Languages language)
        {
            string result = null;

            DataTable dt = GetCreditLineTypes();
            DataRow[] dr = dt.Select(" id=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }
        public static Dictionary<string, string> GetEmbassyList(Languages language, List<ushort> referenceTypes)
        {
            string cacheKey;
            if (referenceTypes.Count > 1 && referenceTypes.Contains(9))
            {
                cacheKey = "Info_EmbassyList1";
            }
            else if (referenceTypes.Count == 1 && referenceTypes.Contains(9))
            {
                cacheKey = "Info_EmbassyList2";
            }
            else
            {
                cacheKey = "Info_EmbassyList3";
            }
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetEmbassyList(referenceTypes);
                CacheHelper.Add(dt, cacheKey);
            }

            Dictionary<string, string> embassyList = new Dictionary<string, string>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description_arm"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    embassyList.Add(dt.Rows[i]["id"].ToString(), description);
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    embassyList.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_eng"].ToString());

            }

            return embassyList;
        }
        public static Dictionary<string, string> GetReferenceLanguages(Languages language)
        {
            string cacheKey = "Info_ReferenceLanguages";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetReferenceLanguages();
                CacheHelper.Add(dt, cacheKey);
            }

            Dictionary<string, string> referenceLanguages = new Dictionary<string, string>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description_arm"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    referenceLanguages.Add(dt.Rows[i]["id"].ToString(), description);
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    referenceLanguages.Add(dt.Rows[i]["id"].ToString(), dt.Rows[i]["description_eng"].ToString());
                }
            }


            return referenceLanguages;
        }
        public static List<KeyValuePair<long, string>> GetReferenceTypes(Languages language)
        {
            string cacheKey = "Info_ReferenceTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetReferenceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<long, string>> referenceTypes = new List<KeyValuePair<long, string>>();
            if (language == Languages.hy)
            {
                string description;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i != 7)
                    {
                        description = dt.Rows[i]["description_arm"].ToString();
                        description = Utility.ConvertAnsiToUnicode(description);
                        referenceTypes.Add(new KeyValuePair<long, string>(Convert.ToInt64(dt.Rows[i]["id"]), description));
                    }
                }
                description = dt.Rows[7]["description_arm"].ToString();
                description = Utility.ConvertAnsiToUnicode(description);
                referenceTypes.Add(new KeyValuePair<long, string>(Convert.ToInt64(dt.Rows[7]["id"]), description));

            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i != 7)
                    {
                        referenceTypes.Add(new KeyValuePair<long, string>(Convert.ToInt64(dt.Rows[i]["id"]), dt.Rows[i]["description_eng"].ToString()));
                    }
                }
                referenceTypes.Add(new KeyValuePair<long, string>(Convert.ToInt64(dt.Rows[7]["id"]), dt.Rows[7]["description_eng"].ToString()));

            }



            return referenceTypes;

        }
        public static List<KeyValuePair<long, string>> GetFilialAddressList(Languages language)
        {
            string cacheKey = "Info_FilialAddressList";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetFilialAddressList();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<long, string>> filialList = new List<KeyValuePair<long, string>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string description = language == Languages.hy ? dt.Rows[i]["description_Arm"].ToString() : dt.Rows[i]["description_eng"].ToString();
                description = Utility.ConvertAnsiToUnicode(description);
                description = description.Replace("§", "«");
                description = description.Replace("¦", "»");
                filialList.Add(new KeyValuePair<long, string>(Convert.ToInt64(dt.Rows[i]["id"]), description));
            }


            return filialList;
        }

        public static DataTable GetCashOrderTypes()
        {
            string cacheKey = "Info_CashOrderTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetCashOrderTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ակտիվ փոխանցման տեսակները
        /// </summary>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public static DataTable GetActiveTransferTypes()
        {

            string cacheKey = "Info_ActiveTransferTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferTypes(1);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Վերադարձնում է բոլոր փոխանցման տեսակները
        /// </summary>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public static DataTable GetTransferTypes()
        {

            string cacheKey = "Info_AllTransferTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferTypes(0);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է բոլոր փոխանցման տեսակները
        /// </summary>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public static DataTable GetAllTransferTypes()
        {

            string cacheKey = "Info_GetAllTransferTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAllTransferTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Վերադարձնում է հեռախոսազանգով մուտքագրված փոխանցումների կարգավիճակները
        /// </summary>       
        public static DataTable GetTransferCallQuality()
        {

            string cacheKey = "Info_CallQuality";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferCallQuality();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է Բանկերի անվանումները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetBanks()
        {
            string cacheKey = "Info_Banks";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBanks();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ՏՀՏ կոդերի ցանկը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLTACodes()
        {
            string cacheKey = "Info_LTACodes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLTACodes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է Ոստիկանության կոդերի ցանկը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPoliceCodes(string accountNumber = "")
        {
            DataTable dt = InfoDB.GetPoliceCodes(accountNumber);
            return dt;
        }

        /// <summary>
        /// Վերադարձնում է Անձի կարգավիճակների ցանկը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSyntheticStatuses()
        {
            string cacheKey = "Info_SyntheticStatuses";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSyntheticStatuses();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է Անձի կարգավիճակը
        /// </summary>
        /// <returns></returns>
        public static string GetSyntheticStatus(string value)
        {
            string result = null;
            DataTable dt = GetSyntheticStatuses();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("value_for8=" + value);
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description_forHB"].ToString());
                }
            }
            return result;
        }

        public static string GetBank(int code, Languages language)
        {
            string result = null;
            DataTable dt = GetBanks();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + code.ToString());
                if (dr.Length > 0)
                {
                    if (language == Languages.hy)
                    {
                        result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                    }
                    else
                    {
                        result = dr[0]["Description_Engl"].ToString();
                    }
                }
            }
            return result;
        }

        public static DataTable GetOperationTypes()
        {

            string cacheKey = "Info_OperationTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOperationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetOperationTypeDescription(int type, Languages language)
        {
            string result = "";

            DataTable dt = GetOperationTypes();
            DataRow[] dr = dt.Select(" code =" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["descr_extract_english"].ToString();
                }
            }
            return result;
        }
        public static DataTable GetStatementFrequency()
        {
            string cacheKey = "Info_Frequency";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetStatementFrequency();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }
        public static DataTable GetJointTypes()
        {
            string cacheKey = "Info_JointTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetJointTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static string GetJointTypeDescription(ushort jointType, Languages language)
        {
            string result = null;

            DataTable dt = GetJointTypes();
            DataRow[] dr = dt.Select("Id=" + jointType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["description_eng"].ToString();
                }
            }
            return result;
        }
        public static string GetOverdueProductTypeDescription(ushort productType, Languages language)
        {
            string result = GetLoanTypeDescription((short)productType, language);
            if (string.IsNullOrEmpty(result))
                result = GetCreditLineTypeDescription((int)productType, language);
            return result;


        }

        public static DataTable GetTransferSystemCurrency()
        {

            string cacheKey = "Info_TransferSystemCurrency";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferSystemCurrency();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataRow[] GetTransferSystemCurrency(int transfersystem)
        {

            DataTable dt = GetTransferSystemCurrency();
            DataRow[] dr = dt.Select(" transfersystem =" + transfersystem.ToString());

            return dr;
        }

        /// <summary>
        /// Ավանդի արժույթը կախված ավանդի տեսակից
        /// </summary>
        /// <param name="depositType"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDepositTypeCurrency(short depositType)
        {
            Dictionary<string, string> depositOrderCurrencies = new Dictionary<string, string>();
            string cacheKey = "Info_DepositTypeCurrency";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetDepositTypeCurrency();
                CacheHelper.Add(dt, cacheKey);
            }
            DataRow[] dr = dt.Select("code=" + depositType.ToString());
            for (int i = 0; i < dr.Length; i++)
            {
                depositOrderCurrencies.Add(dr[i]["currency"].ToString(), dr[i]["currency"].ToString());
            }
            return depositOrderCurrencies;

        }


        /// <summary>
        /// Քարտային համակարգեր
        /// </summary>
        public static DataTable GetCardSystemTypes()
        {
            string cacheKey = "Info_CardSystemTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardSystemTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Քարտի քաղվածքի ստացման եղանակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCardReportReceivingTypes()
        {
            string cacheKey = "Info_CardReportReceivingTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardReportReceivingTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Քարտի PIN կոդի ստացման եղանակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCardPINCodeReceivingTypes()
        {
            string cacheKey = "Info_CardPINCodeReceivingTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardPINCodeReceivingTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Քարտային տեխնոլոգիաներ `օր.(Չիպային,մագնիսական)
        /// </summary>
        public static DataTable GetCardTechnologyTypes()
        {
            string cacheKey = "Info_CardTechnologyTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardTechnologyTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Բաց (գործող) քարտերի տեսակներ
        /// </summary>
        public static DataTable GetCardTypes()
        {
            string cacheKey = "Info_CardTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        internal static DataTable GetReasonForCardTransactionAction(byte actionReasonId)
        {
            return InfoDB.GetReasonForCardTransactionAction(actionReasonId);
        }

        /// <summary>
        /// Վերադարձնում է այն քարտատեսակները, որոնց համար կան գործող քարտեր
        /// </summary>
        public static DataTable GetAllCardTypes()
        {
            string cacheKey = "Info_OpenCardTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAllCardTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Աշխատավարձային ծրագրի որոնում համարով,անվանումով
        /// </summary>
        public static DataTable SearchRelatedOfficeTypes(string officeId, string officeName)
        {
            DataTable dt = InfoDB.SearchRelatedOfficeTypes(officeId, officeName);
            return dt;
        }
        public static DataTable GetPeriodicsSubTypes(Languages language)
        {
            string cacheKey = "Info_PeriodicsSubTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetPeriodicsSubTypes(language);
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;

        }
        public static DataTable GetCommunalBranchList(CommunalTypes communalType, Languages language)
        {
            string cacheKey = "Info_CommunalBranchList_" + communalType.ToString();

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCommunalBranchList(communalType, language);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetPeriodicUtilityTypes(Languages language)
        {
            string cacheKey = "Info_PeriodicUtilityTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetPeriodicUtilityTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;

        }

        public static DataTable GetFactoringTypes()
        {
            string cacheKey = "Info_FactoringTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetFactoringTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;

        }

        public static string GetFactoringTypeDescription(short factoringType, Languages language)
        {
            string result = null;

            DataTable dt = GetFactoringTypes();
            DataRow[] dr = dt.Select("code=" + factoringType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        public static DataTable GetFactoringRegresTypes()
        {
            string cacheKey = "Info_FactoringRegresTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetFactoringRegresTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetFactoringRegresTypeDescription(short factoringRegresType, Languages language)
        {
            string result = null;

            DataTable dt = GetFactoringRegresTypes();
            DataRow[] dr = dt.Select("code=" + factoringRegresType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
                }
                else
                {
                    result = dr[0]["Description_Engl"].ToString();
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetFilialList(Languages language)
        {
            string cacheKey = "Info_FilialList";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetFilialList();
                CacheHelper.Add(dt, cacheKey);
            }


            Dictionary<string, string> filialList = new Dictionary<string, string>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description_Arm"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    filialList.Add(dt.Rows[i]["id"].ToString(), description);
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description_eng"].ToString();
                    description = Utility.ConvertAnsiToUnicode(description);
                    description = description.Replace("§", "«");
                    description = description.Replace("¦", "»");
                    filialList.Add(dt.Rows[i]["id"].ToString(), description);
                }
            }

            return filialList;
        }

        public static Dictionary<string, string> GetCardClosingReasons(Languages language)
        {
            string cacheKey = "Info_CardClosingReasons";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetCardClosingReasons();
                CacheHelper.Add(dt, cacheKey);
            }


            Dictionary<string, string> closingReasons = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string description = dt.Rows[i]["description"].ToString();
                description = Utility.ConvertAnsiToUnicode(description);
                closingReasons.Add(dt.Rows[i]["id"].ToString(), description);
            }


            return closingReasons;
        }

        public static string GetCardClosingReasonDescription(short closingReason, Languages language)
        {
            string result = null;

            DataTable dt = InfoDB.GetCardClosingReasons();
            DataRow[] dr = dt.Select("id=" + closingReason.ToString());

            if (dr.Length > 0)
            {

                result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());


            }
            return result;
        }


        /// <summary>
        /// Վերադարձնում է պարբերական փոխանցման պարբերականությունը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPeriodicityTypes()
        {
            string cacheKey = "Info_PeriodicityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPeriodicityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Հաշվի քաղվածքի ստացման տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetStatementDeliveryTypes()
        {
            string cacheKey = "Info_StatementDeliveryTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetStatementDeliveryTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetStatementDeliveryTypeDescription(int? type, Languages language)
        {
            string result = null;

            DataTable dt = GetStatementDeliveryTypes();
            DataRow[] dr = dt.Select(" ID=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description_eng"].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Երկրների ցուցակ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCountries()
        {

            string cacheKey = "Info_Countries";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCountries();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountAdditionsTypes()
        {

            string cacheKey = "Info_AccountAdditions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountAdditionsTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static Dictionary<string, string> GetOperationsList(Languages language)
        {
            string cacheKey = "Info_OperationsList";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetOperationsList();
                CacheHelper.Add(dt, cacheKey);
            }


            Dictionary<string, string> operations = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string description = dt.Rows[i]["description"].ToString();
                description = Utility.ConvertAnsiToUnicode(description);
                operations.Add(dt.Rows[i]["number"].ToString(), description);
            }


            return operations;
        }

        /// <summary>
        /// Միջազգային փոխանցման արժույթների ցուցակ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetInternationalPaymentCurrencies()
        {

            string cacheKey = "Info_InternationalPaymentCurrencies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetInternationalPaymentCurrencies();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        ///SwiftCod-ից ստանում է բանկի տվյալները(type=1` bankName, type=2` countryCode)
        /// </summary>
        /// <returns></returns>
        public static string GetInfoFromSwiftCode(string swiftCode, short type)
        {
            return InfoDB.GetInfoFromSwiftCode(swiftCode, type);
        }

        /// <summary>
        ///SwiftCod-ից ստանում է բանկի տվյալները(type=1` bankName, type=2` countryCode)
        /// </summary>
        /// <returns></returns>
        public static string GetCountyRiskQuality(string country)
        {
            return InfoDB.GetCountyRiskQuality(country);
        }


        /// <summary>
        /// Հայտի մերժման պատճառներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetOrderRejectTypes()
        {

            string cacheKey = "Info_OrderRejectTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderRejectTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Հայտի մերժման պատճառի նկարագրություն
        /// </summary>
        /// <param name="rejectType"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string GetOrderRejectTypeDescription(ushort rejectType, Languages language)
        {
            string result = null;

            DataTable dt = GetOrderRejectTypes();
            DataRow[] dr = dt.Select("Reject_ID=" + rejectType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["Reject_description"].ToString());
                }
                else
                {
                    result = dr[0]["Reject_description_eng"].ToString();
                }
            }
            return result;
        }


        public static DataTable GetBankOperationFeeTypes(int type)
        {
            string cacheKey = "";
            //type -ը 1 կանխիկ ելքերի միջնորդավճարի տեսակները
            if (type == 1)
            {
                cacheKey = "Info_BankOperationFeeTypes1";
            }
            //type -ը 2 -ը կանխիկ մուտքի միջնորդավճարի տեսակները
            if (type == 2)
            {
                cacheKey = "Info_BankOperationFeeTypes2";
            }
            //Բոլոր միջնորդավճարների տեսակները
            if (type == 3)
            {
                cacheKey = "Info_BankOperationFeeTypes3";
            }
            //ՀՀ տարածքում միջնորդավճարների տեսակները
            if (type == 4)
            {
                cacheKey = "Info_BankOperationFeeTypes4";
            }
            if (type == 5)
            {
                cacheKey = "Info_BankOperationFeeTypes5";
            }
            if (type == 6)
            {
                cacheKey = "Info_BankOperationFeeTypes6";
            }
            DataTable dt = CacheHelper.Get(cacheKey);
            if (type != 0)
            {
                if (dt == null)
                {
                    dt = InfoDB.GetBankOperationFeeTypes(type);
                    CacheHelper.Add(dt, cacheKey);
                }
            }
            return dt;
        }

        /// <summary>
        /// Հաշվի բացման տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountStatuses()
        {
            string cacheKey = "Info_AccountStatuses";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountStatuses();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Քարտի աշխ. ծրագրի անվանում
        /// </summary>
        /// <returns></returns>
        public static string GetCardRelatedOfficeName(ushort officeNumber)
        {
            string result = InfoDB.GetCardRelatedOfficeName(officeNumber);

            result = Utility.ConvertAnsiToUnicode(result);
            return result;
        }

        public static DataTable GetTransitAccountTypes(bool forLoanMature = false)
        {
            DataTable dt = InfoDB.GetTransitAccountTypes(forLoanMature);
            return dt;
        }
        public static string GetTransitAccountTypeDescription(int transitAccountType, Languages language)
        {
            string result = null;

            DataTable dt = GetTransitAccountTypes();


            DataRow[] dr = dt.Select("id=" + transitAccountType.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }


        /// <summary>
        /// Հայտի մերժման պատճառներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetFreezeReasonTypes()
        {

            string cacheKey = "Info_FreezeReasonTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetFreezeReasonTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetFreezeReasonDescription(ushort reasonId)
        {
            string result = null;

            DataTable dt = GetFreezeReasonTypes();
            DataRow[] dr = dt.Select("Id=" + reasonId.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
            }
            return result;
        }

        public static byte GetCardTransactionReasonByFreezeReasonId(ushort reasonId)
        {
            byte result = 0;

            DataTable dt = GetFreezeReasonTypes();
            DataRow[] dr = dt.Select("Id=" + reasonId.ToString());

            if (dr.Length > 0)
            {
                result = Byte.Parse(dr[0]["card_transaction_reason_type"].ToString());
            }
            return result;
        }


        public static string BankOperationFeeTypeDescription(short feeType)
        {
            string result = null;

            DataTable dt = BankOperationFeeTypes();
            DataRow[] dr = dt.Select("code=" + feeType.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
            }
            return result;
        }

        private static DataTable BankOperationFeeTypes()
        {
            string cacheKey = "Info_BankOperationFeeTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.BankOperationFeeTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Ընթացիկ հաշվի բացման համար առաջարկվող արժույթներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCurrentAccountCurrencies()
        {

            string cacheKey = "Info_CurrentAccountCurrencies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCurrentAccountCurrencies();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Հաճախորդին տրամադրվող ծառայությունների տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetServiceProvidedTypes()
        {

            string cacheKey = "Info_ServiceProvidedTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetServiceProvidedTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ավտոմատ ձևակերպվող հայտերի տեսակները
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<int, int>> GetAutoConfirmOrderTypes()
        {
            DataTable dt = Info.GetOrderSubTypes();

            List<KeyValuePair<int, int>> autoConfirmTypes = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ushort confirmType = ushort.Parse(dt.Rows[i]["autoconfirm"].ToString());
                //Բացառում ենք ոչ ավտոմատ ձևակերպվող տեսակները
                if (confirmType == 1)
                {
                    autoConfirmTypes.Add(new KeyValuePair<int, int>(key: UInt16.Parse(dt.Rows[i]["document_type"].ToString()), value: UInt16.Parse(dt.Rows[i]["document_sub_type"].ToString())));
                }
            }

            return autoConfirmTypes;
        }


        public static DataTable GetDisputeResolutions()
        {
            string cacheKey = "Info_DisputeResolutions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDisputeResolutions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetCommunicationTypes()
        {
            string cacheKey = "Info_CommunicationTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCommunicationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetDisputeResolutionDescription(short dispute, Languages language)
        {
            string result = null;

            DataTable dt = GetDisputeResolutions();
            DataRow[] dr = dt.Select("id_dispute=" + dispute.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["dispute_description"].ToString());
                }
                else
                {
                    result = dr[0]["dispute_description"].ToString();
                }
            }
            return result;
        }

        public static string GetCommunicationTypeDescription(short communicationType, Languages language)
        {
            string result = null;

            DataTable dt = GetCommunicationTypes();
            DataRow[] dr = dt.Select("id=" + communicationType.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["description"].ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի սառեցման պատճառների տեսակները (2,3,9,10 բացառած)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAccountFreezeReasonsTypes()
        {
            DataTable dt = Info.GetFreezeReasonTypes();

            Dictionary<string, string> reasonTypes = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ushort additionId = ushort.Parse(dt.Rows[i]["Id"].ToString());
                //Բացառում ենք չօգտագործվող տեսակները
                if ((additionId < 2 || additionId > 3) && (additionId < 9 || additionId > 10) && additionId != 13 && additionId != 17 && additionId != 18 && additionId != 19 && additionId != 20)
                {
                    reasonTypes.Add(dt.Rows[i]["Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                }
            }
            return reasonTypes;
        }

        public static string GetServiceProvidedTypeDescription(int type, Languages language)
        {
            string result = null;

            DataTable dt = GetServiceProvidedTypes();
            DataRow[] dr = dt.Select(" ID=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                else
                {
                    result = dr[0]["description_eng"].ToString();
                }
            }
            return result;
        }


        /// <summary>
        /// Հայտի հեռացման պատճառներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetOrderRemovingReasons()
        {
            string cacheKey = "Info_OrderRemovingReasons";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderRemovingReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetOrderRemovingReasonDescription(int type, Languages language)
        {
            string result = null;

            DataTable dt = GetOrderRemovingReasons();
            DataRow[] dr = dt.Select(" reason_id=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["reason_description"].ToString());
                }
                else
                {
                    result = dr[0]["reason_description"].ToString();
                }
            }
            return result;
        }



        public static DataTable GetFondsDescriptions()
        {
            string cacheKey = "Info_Fonds";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetFondDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetFondDescription(short fondType)
        {
            string result = null;

            DataTable dt = GetFondsDescriptions();
            DataRow[] dr = dt.Select("code=" + fondType.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }

        public static DataTable GetLoanProgramsDescriptions()
        {
            string cacheKey = "Info_LoanPrograms";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanProgramDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanProgramDescription(short loanProgramType)
        {
            string result = null;

            DataTable dt = GetLoanProgramsDescriptions();
            DataRow[] dr = dt.Select("code=" + loanProgramType.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }


        public static DataTable GetLoanActionDescriptions()
        {
            string cacheKey = "Info_LoanActions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanActionDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanActionDescription(short actionType)
        {
            string result = null;

            DataTable dt = GetLoanActionDescriptions();
            DataRow[] dr = dt.Select("code=" + actionType.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["action"].ToString());
            }
            return result;
        }

        public static DataTable GetClaimQualities()
        {
            string cacheKey = "Info_ClaimQualities";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetClaimQualities();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetClaimQualityDescription(short quality)
        {
            string result = null;

            DataTable dt = GetClaimQualities();
            DataRow[] dr = dt.Select("claim_quality=" + quality.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["claim_quality_description"].ToString());
            }
            return result;
        }

        public static DataTable GetClaimPurposes()
        {
            string cacheKey = "Info_ClaimPurposes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetClaimPurposes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetClaimPurposeDescription(short purpose)
        {
            string result = null;

            DataTable dt = GetClaimPurposes();
            DataRow[] dr = dt.Select("code=" + purpose.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }

        public static DataTable GetClaimEventTypes()
        {
            string cacheKey = "Info_ClaimEventTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetClaimEventTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetClaimEventTypeDescription(short type)
        {
            string result = null;

            DataTable dt = GetClaimEventTypes();
            DataRow[] dr = dt.Select("claim_event=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["claim_event_description"].ToString());
            }
            return result;
        }

        public static DataTable GetClaimEventPurposes()
        {
            string cacheKey = "Info_ClaimEventPurposes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetClaimEventPurposes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetClaimEventPurposeDescription(short purpose)
        {
            string result = null;

            DataTable dt = GetClaimEventPurposes();
            DataRow[] dr = dt.Select("code=" + purpose.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }

        public static DataTable GetCourtTypes()
        {
            string cacheKey = "Info_CourtTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCourtTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetCourtTypeDescription(short type)
        {
            string result = null;

            DataTable dt = GetCourtTypes();
            DataRow[] dr = dt.Select("number=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }

        public static DataTable GetTaxTypes()
        {
            string cacheKey = "Info_TaxTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTaxTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTaxTypeDescription(short type)
        {
            string result = null;

            DataTable dt = GetTaxTypes();
            DataRow[] dr = dt.Select("id=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["tax_descr"].ToString());
            }
            return result;
        }

        public static DataTable GetAssigneeOperationTypes(int groupId, int typeOfCustomer)
        {

            DataTable dt = InfoDB.GetAssigneeOperationTypes(groupId, typeOfCustomer);

            return dt;
        }

        public static DataTable GetAssigneeOperationGroupTypes(int typeOfCustomer)
        {

            DataTable dt = InfoDB.GetAssigneeOperationGroupTypes(typeOfCustomer);

            return dt;
        }

        public static DataTable GetCredentialTypes()
        {
            string cacheKey = "Info_CredentialTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCredentialTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetCredentialClosingReasons()
        {
            string cacheKey = "Info_CredentialClosingReason";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCredentialClosingReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetActionPermissionTypes()
        {

            DataTable dt = InfoDB.GetActionPermissionTypes();
            return dt;
        }


        /// <summary>
        /// Վերադարձնում է հաշվի փակման պատճառների տեսակները (1,6)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAccountClosingReasonsTypes()
        {
            string cacheKey = "Info_AccountClosingReasonTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountClosingReasonTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            Dictionary<string, string> closingTypes = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ushort reasonId = ushort.Parse(dt.Rows[i]["Idx_Closing"].ToString());
                //Բացառում ենք չօգտագործվող տեսակները
                if ((reasonId == 1 || reasonId == 6))
                {
                    closingTypes.Add(dt.Rows[i]["Idx_Closing"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Decsription"].ToString()));
                }
            }
            return closingTypes;
        }


        public static string GetAccountClosingReasonTypeDescription(ushort type)
        {
            string result = "";

            DataTable dt = InfoDB.GetAccountClosingReasonTypes();
            DataRow[] dr = dt.Select("Idx_Closing=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Decsription"].ToString());
            }
            return result;
        }

        public static DataTable GetTokenTypes()
        {
            string cacheKey = "Info_TokenTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTokenTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTokenTypeDescription(HBTokenTypes type)
        {
            string result = null;

            DataTable dt = GetTokenTypes();
            DataRow[] dr = dt.Select("id=" + ((ushort)type).ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["token_type"].ToString());
            }
            return result;
        }


        /// <summary>
        /// Տարվա ամիսներ
        /// </summary>
        public static DataTable GetMonths()
        {

            string cacheKey = "Info_Months";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetMonths();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Վերադարձնում է սպասարկման վարձի գանձման նշման պատճառները
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<ushort, string>> GetServicePaymentNoteReasons()
        {
            string cacheKey = "Info_Service_Payment_Note_Reasons";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetServicePaymentNoteReasons();
                CacheHelper.Add(dt, cacheKey);
            }
            List<KeyValuePair<ushort, string>> noteReasons = new List<KeyValuePair<ushort, string>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                noteReasons.Add(new KeyValuePair<ushort, string>(Convert.ToUInt16(dt.Rows[i]["Id"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Note_description"].ToString()).Trim()));
            }
            return noteReasons;
        }
        /// <summary>
        /// վերադարձնում խոխանցման գումարի նպատակները
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<ushort, string>> GetTransferAmountPurposes()
        {
            string cacheKey = "Info_International_Transfer_Purposes";
            List<KeyValuePair<ushort, string>> transferAmountPurposes = new List<KeyValuePair<ushort, string>>();
            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferAmountPurposes();
                CacheHelper.Add(dt, cacheKey);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                transferAmountPurposes.Add(new KeyValuePair<ushort, string>(Convert.ToUInt16(dt.Rows[i]["id"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()).Trim()));
            }
            return transferAmountPurposes;
        }
        public static List<KeyValuePair<ushort, string>> GetTransferReceiverLivingPlaceTypes()
        {
            List<KeyValuePair<ushort, string>> transferReceiverLivingPlaceTypes = new List<KeyValuePair<ushort, string>>();
            string cacheKey = "Info_TransferReceiverLivingPlaceTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTransferReceiverLivingPlaceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                transferReceiverLivingPlaceTypes.Add(new KeyValuePair<ushort, string>(Convert.ToUInt16(dt.Rows[i]["code"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()).Trim()));
            }
            return transferReceiverLivingPlaceTypes;
        }
        public static List<KeyValuePair<ushort, string>> GetTransferSenderLivingPlaceTypes()
        {
            List<KeyValuePair<ushort, string>> transferSenderLivingPlaceTypes = new List<KeyValuePair<ushort, string>>();

            string cacheKey = "Info_TransferSenderLivingPlaceTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTransferSenderLivingPlaceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                transferSenderLivingPlaceTypes.Add(new KeyValuePair<ushort, string>(Convert.ToUInt16(dt.Rows[i]["code"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()).Trim()));
            }
            return transferSenderLivingPlaceTypes;
        }
        public static List<KeyValuePair<ushort, string>> GetTransferAmountTypes()
        {
            List<KeyValuePair<ushort, string>> transferAmountTypes = new List<KeyValuePair<ushort, string>>();

            string cacheKey = "Info_TransferAmountTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTransferAmountTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                transferAmountTypes.Add(new KeyValuePair<ushort, string>(Convert.ToUInt16(dt.Rows[i]["code"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()).Trim()));
            }
            return transferAmountTypes;
        }

        public static DataTable GetPensionAppliactionQualityTypes()
        {

            string cacheKey = "Info_PensionAppliactionQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPensionAppliactionQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static string GetPensionAppliactionQualityType(short quality)
        {
            string result = null;

            DataTable dt = GetPensionAppliactionQualityTypes();
            DataRow[] dr = dt.Select("quality=" + quality.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["quality_description"].ToString());
            }
            return result;
        }



        public static DataTable GetPensionAppliactionServiceTypes()
        {

            string cacheKey = "Info_PensionAppliactionServiceTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPensionAppliactionServiceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetPensionAppliactionClosingTypes()
        {

            string cacheKey = "Info_PensionAppliactionClosingTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPensionAppliactionClosingTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetPensionAppliactionClosingType(short type)
        {
            string result = null;

            DataTable dt = GetPensionAppliactionClosingTypes();
            DataRow[] dr = dt.Select("closing_number=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["closing_description"].ToString());
            }
            return result;
        }

        public static string GetPensionAppliactionServiceType(short type)
        {
            string result = null;

            DataTable dt = GetPensionAppliactionServiceTypes();
            DataRow[] dr = dt.Select("type=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["type_description"].ToString());
            }
            return result;
        }

        public static string GetTransferCallContractQualityType(short type)
        {
            string result = null;

            switch (type)
            {
                case 0:
                    result = "Գործող";
                    break;
                case 40:
                    result = "Փակված";
                    break;
                default:
                    break;
            }

            return result;
        }
        public static DataTable GetCurNominals(string currency)
        {

            string cacheKey = "Info_CurNominals" + "_" + currency;

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCurNominals(currency);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static string GetCardTariffContractReasonDescription(ushort reasonId)
        {
            string result = null;
            DataTable dt = GetCardTariffContractReasonDescriptions();
            DataRow[] dr = dt.Select("ReasonId=" + reasonId.ToString());
            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Reason"].ToString());
            }
            return result;
        }
        private static DataTable GetCardTariffContractReasonDescriptions()
        {
            string cacheKey = "Info_CardTariffContractDescriptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardTariffContractReasonDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static string GetCardTariffContractQualityDescription(ushort quality)
        {

            if (quality == 1)
            {
                return "Գործող";
            }
            else if (quality == 2)
            {
                return "Սառեցված";
            }
            else
            {
                return "Դադարեցված";
            }

        }


        public static string GetPosLocationQualityDesc(ushort quality)
        {
            if (quality == 0)
            {
                return "Գործող";
            }
            else
            {
                return "Փակ";
            }
        }

        public static string GetPosTerminalQualityDesc(ushort quality)
        {
            string qualityDesc = "";
            switch (quality)
            {
                case 1: { qualityDesc = "Գործող"; break; }
                case 0: { qualityDesc = "Փակ"; break; }
                case 10: { qualityDesc = "Պայմանագիր"; break; }
                case 2: { qualityDesc = "Սառեցված"; break; }
            }

            return qualityDesc;
        }

        public static string GetPosLocationTypeDescription(ushort type)
        {
            string result = null;
            DataTable dt = GetPosLocationTypeDescriptions();
            DataRow[] dr = dt.Select("id=" + type);
            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }
        private static DataTable GetPosLocationTypeDescriptions()
        {
            string cacheKey = "Info_PosLocationTypeDescriptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPosLocationTypeDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static string GetPosTerminalTypeDescription(ushort type)
        {
            string result = null;
            DataTable dt = GetPosTerminalTypeDescriptions();
            DataRow[] dr = dt.Select("id=" + type);
            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["device_description"].ToString());
            }
            return result;
        }
        private static DataTable GetPosTerminalTypeDescriptions()
        {
            string cacheKey = "Info_PosTerminalTypeDescriptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetPosTerminalTypeDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static DataTable GetSMSMessagingStatusTypes()
        {
            string cacheKey = "Info_SMSMessagingStatusTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSMSMessagingStatusTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        /// <summary>
        /// Գրավի տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetProvisionTypes()
        {

            string cacheKey = "Info_Provisiontypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProvisionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static DataTable GetInsuranceTypes()
        {

            string cacheKey = "Info_InsuranceTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetInsuranceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetInsuranceTypeDescription(ushort type, Languages language)
        {
            string result = null;

            DataTable dt = GetInsuranceTypes();
            DataRow[] dr = dt.Select(" insurance_type=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.eng)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description_eng"].ToString());
                }
                else
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }


        public static DataTable GetInsuranceCompanies()
        {

            string cacheKey = "Info_InsuranceCompanies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetInsuranceCompanies();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetInsuranceCompaniesByInsuranceType(ushort insuranceType)
        {
            DataTable dt = InfoDB.GetInsuranceCompaniesByInsuranceType(insuranceType);
            return dt;
        }
        public static DataTable GetInsuranceTypesByProductType(bool isLoanProduct, bool isSeparatelyProduct)
        {
            DataTable dt = InfoDB.GetInsuranceTypesByProductType(isLoanProduct, isSeparatelyProduct);
            return dt;
        }

        public static string GetInsuranceCompanyDescription(ushort companyID)
        {
            string result = null;

            DataTable dt = GetInsuranceCompanies();
            DataRow[] dr = dt.Select(" company_ID=" + companyID.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["company_name"].ToString());
            }
            return result;
        }

        public static DataTable GetCardDataChangeFieldTypes()
        {

            string cacheKey = "Info_CardDataChangeFieldTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardDataChangeFieldTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetCardDataChangeFieldTypeDescription(ushort type)
        {
            string result = null;

            DataTable dt = GetCardDataChangeFieldTypes();
            DataRow[] dr = dt.Select(" field_type=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["field_type_description"].ToString());
            }
            return result;
        }


        public static DataTable GetDepositClosingReasonTypes(bool showinLIst = true)
        {
            DataTable dt = InfoDB.GetDepositClosingReasonTypes(showinLIst);
            return dt;
        }

        public static string GetDepositClosingReasonTypeDescription(ushort type, Languages language)
        {
            string result = null;

            DataTable dt = GetDepositClosingReasonTypes(false);
            DataRow[] dr = dt.Select(" reason_type=" + type.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.eng)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["reason_type_description_eng"].ToString());
                }
                else
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["reason_type_description"].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է դաշտի տեսակը
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static AdditionalValueType GetFieldType(ushort type)
        {
            AdditionalValueType additionalValueType;
            additionalValueType = AdditionalValueType.NotSpecified;

            DataTable dt = GetCardDataChangeFieldTypes();
            DataRow[] dr = dt.Select(" field_type=" + type.ToString());

            switch (dr[0]["field_column_type"].ToString())
            {
                case "double":
                    additionalValueType = AdditionalValueType.Double;
                    break;
                case "int":
                    additionalValueType = AdditionalValueType.Int;
                    break;
                case "date":
                    additionalValueType = AdditionalValueType.Date;
                    break;
                case "percent":
                    additionalValueType = AdditionalValueType.Percent;
                    break;
                default:
                    break;

            }


            return additionalValueType;

        }

        /// <summary>
        /// Տոկենի ենթատեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSubTypesOfTokens()
        {

            string cacheKey = "Info_SubTypeOfTokens";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSubTypesOfTokens();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Ստանում է տոկենի ենթատեսակի նկարագրությունը
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTokenSubTypeDescription(short type)
        {
            string result = null;

            DataTable dt = GetSubTypesOfTokens();
            DataRow[] dr = dt.Select("Id=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Ելքային ձևերի հաշվետվությունների տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPrintReportTypes()
        {
            DataTable dt = InfoDB.GetPrintReportTypes();

            return dt;
        }

        /// <summary>
        /// Հեռահար բանկինգի գործող պայմանագրերի քանակի և գործող օգտագործողների քանակի և ակտիվության վերաբերյալ հաշվետվության տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetHBApplicationReportType()
        {
            string cacheKey = "Info_HBApplicationReportType";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBApplicationReportType();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Ելքային ձևերի հաշվետվությունների տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTransferRejectReasonTypes()
        {

            string cacheKey = "Info_TransferRejectReasonTypes";

            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTransferRejectReasonTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }



        /// <summary>
        /// Փոխանցման կարգավիճակների տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTransferRequestStepTypes()
        {

            string cacheKey = "Info_TransferRequestStepTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferRequestStepTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetTransferRequestStatusTypes()
        {

            string cacheKey = "Info_TransferRequestStatusTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferRequestStatusTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetBusinesDepositOptions()
        {

            string cacheKey = "Info_BusinesDepositOptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBusinesDepositOptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetBusinesDepositOptionsDescription(ushort optionType, Languages language)
        {
            string result = null;

            DataTable dt = GetBusinesDepositOptions();
            DataRow[] dr = dt.Select("option_type=" + optionType.ToString());

            if (dr.Length > 0)
            {
                if (language != Languages.hy)
                {
                    result = dr[0]["description_eng"].ToString();

                }
                else
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DepositOption GetBusinesDepositOption(ushort optionType, Languages language)
        {
            DepositOption option = new DepositOption();
            DataTable dt = GetBusinesDepositOptions();
            DataRow[] dr = dt.Select("option_type=" + optionType.ToString());


            if (dr.Length > 0)
            {
                if (language != Languages.hy)
                {
                    option.TypeDescription = dr[0]["description_eng"].ToString();

                }
                else
                {
                    option.TypeDescription = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
                option.OptionGroup = Convert.ToUInt16(dr[0]["option_group"]);

            }


            return option;
        }

        public static DataTable GetTransferSessions(DateTime dateStart, DateTime dateEnd, short transferGroup)
        {

            DataTable dt = InfoDB.GetTransferSessions(dateStart, dateEnd, transferGroup);
            return dt;
        }

        public static DataTable GetTypeOfCardPaymentsToArca()
        {

            string cacheKey = "Info_TypeOfCardPaymentsToArca";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfCardPaymentsToArca();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTypeOfCardPaymentsToArcaDescription(ushort status)
        {
            string result = null;

            DataTable dt = GetTypeOfCardPaymentsToArca();
            DataRow[] dr = dt.Select("id=" + status.ToString());

            if (dr.Length > 0)
            {

                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());

            }
            return result;
        }


        public static DataTable GetTypeOfLoanRepaymentSource()
        {

            string cacheKey = "Info_TypeOfLoanRepaymentSource";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfLoanRepaymentSource();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTypeOfLoanRepaymentSourceDescription(ushort repaymentSourceType)
        {
            string result = null;

            DataTable dt = GetTypeOfLoanRepaymentSource();
            DataRow[] dr = dt.Select("id=" + repaymentSourceType.ToString());

            if (dr.Length > 0)
            {

                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());

            }
            return result;
        }


        public static DataTable GetRegions(int country)
        {

            DataTable dt = InfoDB.GetRegions(country);
            return dt;
        }

        public static DataTable GetArmenianPlaces(int region)
        {

            DataTable dt = InfoDB.GetArmenianPlaces(region);
            return dt;
        }

        public static CardTariffAdditionalInformation GetCardTariffAdditionalInformation(int officeID, int cardType)
        {
            return InfoDB.GetCardTariffAdditionalInformation(officeID, cardType);
        }

        public static DataTable GetDepositDataChangeFieldTypes()
        {

            string cacheKey = "Info_DepositDataChangeFieldTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDepositDataChangeFieldTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetDepositDataChangeFieldTypeDescription(ushort type)
        {
            string result = null;

            DataTable dt = GetDepositDataChangeFieldTypes();
            DataRow[] dr = dt.Select(" field_type=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["field_type_description"].ToString());
            }
            return result;
        }

        public static List<KeyValuePair<int, string>> GetPhoneBankingContractQuestions()
        {
            string cacheKey = "Info_PhoneBankingContractQuestions";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetPhoneBankingContractQuestions();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<int, string>> questionsList = new List<KeyValuePair<int, string>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string description = dt.Rows[i]["Question"].ToString();
                description = Utility.ConvertAnsiToUnicode(description);
                questionsList.Add(new KeyValuePair<int, string>(Convert.ToInt32(dt.Rows[i]["ID"]), description));
            }

            return questionsList;
        }

        public static DataTable GetTypeOfRequestsForFeeCharges()
        {

            string cacheKey = "Info_TypeOfRequestsForFeeCharges";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfRequestsForFeeCharges();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetTypeOfRequestsForFeeCharge(short type)
        {
            string result = null;

            DataTable dt = GetTypeOfRequestsForFeeCharges();
            DataRow[] dr = dt.Select("Id=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["Description"].ToString());
            }
            return result;
        }
        public static DataTable GetLoanMatureTypes()
        {

            string cacheKey = "Info_LoanMatureTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMatureTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static string GetLoanMatureTypeDescription(string code, Languages language)
        {
            string result = null;

            DataTable dt = GetLoanMatureTypes();



            DataRow[] dr = dt.Select("code='" + code + "'");

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());

            }
            return result;
        }


        public static DataTable GetCashBookRowTypes()
        {

            string cacheKey = "Info_CashBookRowTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCashBookRowTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է EasyPay-ի սխալների (արտահայտությունների  նկարագրությունները)
        /// </summary>
        /// <returns></returns>
        public static DataTable GetExternalPaymentTerms()
        {
            string cacheKey = "Info_ExternalPaymentTerms";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetExternalPaymentTerms();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static string GetExternalPaymentTerm(short id, string[] param, Languages language)
        {
            string result = null;
            DataTable dt = GetExternalPaymentTerms();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    if (language == Languages.hy)
                    {
                        result = dr[0]["Description"].ToString();
                    }
                    else
                    {
                        result = dr[0]["Description_Eng"].ToString();
                    }


                    if (param != null)
                    {
                        result = string.Format(result, param);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է EasyPay-ի պրոդուկտի անվանումները(օր.՝ 1-Քարտի համար,2-Հաշվեհամար ....)
        /// </summary>
        /// <returns></returns>
        public static DataTable GetExternalPaymentProductDetailCodes()
        {
            string cacheKey = "Info_ExternalPaymentProductDetailCodes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetExternalPaymentProductDetailCodes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetExternalPaymentProductDetailCode(short id, Languages language)
        {
            string result = null;
            DataTable dt = GetExternalPaymentProductDetailCodes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    if (language == Languages.hy)
                    {
                        result = dr[0]["Description"].ToString();
                    }
                    else
                    {
                        result = dr[0]["Description_Eng"].ToString();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է այլ կանխիկ տերմինալով կատարված գործարքի կարգավիճակի նկարագրությունը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetExternalPaymentStatusCodes()
        {
            string cacheKey = "Info_ExternalPaymentStatusCodes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetExternalPaymentStatusCodes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetExternalPaymentStatusCode(short id, Languages language)
        {
            string result = null;
            DataTable dt = GetExternalPaymentStatusCodes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    if (language == Languages.hy)
                    {
                        result = dr[0]["Description"].ToString();
                    }
                    else
                    {
                        result = dr[0]["Description_Eng"].ToString();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է վարկային դիմումի կարգավիճակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLoanApplicationQualityTypes()
        {
            string cacheKey = "Info_LoanApplicationQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanApplicationQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanApplicationQualityTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetLoanApplicationQualityTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("number=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["status"].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է վարկային դիմումի կարգավիճակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLoanApplicationProductTypes()
        {
            string cacheKey = "Info_LoanAplicationProductTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanApplicationProductTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanApplicationProductTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetLoanApplicationProductTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetLoanMonitoringTypes()
        {
            string cacheKey = "Info_LoanMonitoringTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMonitoringTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanMonitoringTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetLoanMonitoringTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetLoanMonitoringFactorGroupes()
        {
            string cacheKey = "Info_LoanMonitoringFactorGroupes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMonitoringFactorGroupes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static Dictionary<string, string> GetTransferMethod()
        {
            string cacheKey = "Info_TransferMethod";

            Dictionary<string, string> TransferMethod = CacheHelper.Get<Dictionary<string, string>>(cacheKey);

            if (TransferMethod == null)
            {
                TransferMethod = InfoDB.GetTransferMethod();
                CacheHelper.Add(TransferMethod, cacheKey);
            }

            return TransferMethod;
        }

        public static DataTable GetLoanMonitoringFactors(int loanType, int groupId = 0)
        {
            DataTable dt = InfoDB.GetLoanMonitoringFactors(loanType, groupId);
            return dt;
        }

        public static string GetLoanMonitoringFactorDescription(short id)
        {
            string result = null;
            DataTable dt = GetLoanMonitoringFactors(0);
            if (dt != null)
            {
                DataRow[] dr = dt.Select("id=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetProfitReductionTypes()
        {
            string cacheKey = "Info_ProfitReductionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProfitReductionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetProfitReductionTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetProfitReductionTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetProvisionCostConclusionTypes()
        {
            string cacheKey = "Info_ProvisionCostConclusionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProvisionCostConclusionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetProvisionCostConclusionTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetProvisionCostConclusionTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetProvisionQualityConclusionTypes()
        {
            string cacheKey = "Info_ProvisionQualityConclusionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProvisionQualityConclusionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetProvisionQualityConclusionTypeDescription(short id)
        {
            string result = null;
            DataTable dt = GetProvisionQualityConclusionTypes();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetLoanMonitoringConclusions()
        {
            string cacheKey = "Info_LoanMonitoringConclusions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMonitoringConclusions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static string GetLoanMonitoringConclusionDescription(short id)
        {
            string result = null;
            DataTable dt = GetLoanMonitoringConclusions();
            if (dt != null)
            {
                DataRow[] dr = dt.Select("code=" + id.ToString());
                if (dr.Length > 0)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
                }
            }
            return result;
        }

        public static DataTable GetLoanMonitoringSubTypes()
        {
            string cacheKey = "Info_LoanMonitoringSubTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMonitoringSubTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Հաշվի սակագների խումբ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDemandDepositsTariffGroups()
        {
            string cacheKey = "Info_DemandDepositsTariffGroups";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDemandDepositsTariffGroups();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static string GetDemandDepositsTariffGroup(int type)
        {
            string result = null;

            DataTable dt = GetDemandDepositsTariffGroups();
            DataRow[] dr = dt.Select("ID=" + type.ToString());

            if (dr.Length > 0)
            {
                result = Utility.ConvertAnsiToUnicode(dr[0]["description"].ToString());
            }
            return result;
        }

        /// <summary>
        ///  Հաշվի սահմանափակման խումբ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountRestrictionGroups()
        {
            string cacheKey = "Info_AccountRestrictionGroups";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountRestrictionGroups();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ վարկատեսակի համար տուգանային տոկոսը
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static double GetPenaltyRateOfLoans(int productType, DateTime startDate)
        {
            return InfoDB.GetPenaltyRateOfLoans(productType, startDate);
        }



        public static DataTable GetOrderQualityTypesForAcbaOnline()
        {

            string cacheKey = "Info_OrderQualityTypesForAcbaOnline";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderQualityTypesForAcbaOnline();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static string GetOrderQualityTypeDescriptionForAcbaOnline(short quality, Languages language)
        {
            string result = null;

            DataTable dt = GetOrderQualityTypesForAcbaOnline();

            if (quality == 2)
                quality = 3;

            DataRow[] dr = dt.Select("quality=" + quality.ToString());

            if (dr.Length > 0)
            {
                if (language == Languages.hy)
                {
                    result = Utility.ConvertAnsiToUnicode(dr[0]["action_description_arm"].ToString());
                }
                else
                {
                    result = dr[0]["action_description_eng"].ToString();
                }
            }
            return result;
        }


        public static DataTable GetProductNotificationInformationTypes()
        {
            string cacheKey = "Info_ProductNotificationInformationTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProductNotificationInformationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetProductNotificationFrequencyTypes(byte informationType)
        {
            string cacheKey = "Info_ProductNotificationFrequencyTypes_" + informationType;

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProductNotificationFrequencyTypes(informationType);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetProductNotificationOptionTypes(byte informationType)
        {
            string cacheKey = "Info_ProductNotificationOptionTypes_" + informationType;

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProductNotificationOptionTypes(informationType);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetProductNotificationLanguageTypes(byte informationType)
        {
            string cacheKey = "Info_ProductNotificationLanguageTypes_" + informationType;

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProductNotificationLanguageTypes(informationType);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetProductNotificationFileFormatTypes()
        {
            string cacheKey = "Info_ProductNotificationFileFormatTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetProductNotificationFileFormatTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Swift հաղորդագրության տեսակները
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetSwiftMessageTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("0", "Ստացված");
            result.Add("1", "Ուղարկված");
            return result;
        }

        /// <summary>
        /// Swift հաղորդագրության համակարգի տեսակները
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetSwiftMessageSystemTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("1", "Swift");
            result.Add("2", "Bank Mail");
            return result;
        }

        /// <summary>
        /// Swift հաղորդագրության MT կոդեր
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSwiftMessagMtCodes()
        {
            string cacheKey = "Info_SwiftMessagMtCodes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSwiftMessagMtCodes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Swift հաղորդագրության ֆայլի առկայության նշում
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetSwiftMessageAttachmentExistenceTypes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("0", "Չկցված");
            result.Add("1", "Կցված");
            return result;
        }

        public static Dictionary<string, string> GetArcaCardSMSServiceActionTypes()
        {
            return InfoDB.GetArcaCardSMSServiceActionTypes();
        }



        public static DataTable GetBondIssueQualities()
        {

            string cacheKey = "Info_CallQuality";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBondIssueQualities();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է պարտատոմս թողարկող կազմակերպությունները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetBondIssuerTypes()
        {
            string cacheKey = "Info_BondIssuerTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBondIssuerTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Վերադարձնում է պարտատոմս թողարկող կազմակերպությունները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetBondIssuePeriodTypes()
        {
            string cacheKey = "Info_BondIssuePeriodTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBondIssuePeriodTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static DataTable GetATSFilials()
        {
            string cacheKey = "Info_ATS_FILIALS";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetATSFilials();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Վերադարձնում է պարտատոմսի մերժման պատճառների ցանկը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetBondRejectReasonTypes()
        {
            string cacheKey = "Info_BondRejectReasonTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBondRejectReasonTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է պարտատոմսի կարգավիճակների տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetBondQualityTypes()
        {
            string cacheKey = "Info_BondQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetBondQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Արգելադրման նպատակ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTypeOfPaymentDescriptions()
        {
            string cacheKey = "Info_TypeOfPaymentDescriptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfPaymentDescriptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Քարտից գումարի ելքագրման պատճառ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTypeofPaymentReasonAboutOutTransfering()
        {
            string cacheKey = "Info_TypeofPaymentReasonAboutOutTransfering";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeofPaymentReasonAboutOutTransfering();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        /// <summary>
        /// Գործառնական օրվա ստուգումներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTypeofOperDayClosingOptions()
        {
            string cacheKey = "Info_TypeofTypeofOperDayClosingOptions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeofOperDayClosingOptions();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է 24/7 Mode-ի տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTypeOf24_7Modes()
        {
            string cacheKey = "Info_TypeOf24_7Modes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOf24_7Modes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static DataTable GetTypeOfCommunals()
        {
            string cacheKey = "Info_TypeOfCommunals";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfCommunals();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static DataTable GetCashOrderCurrencies(ulong customerNumber)
        {

            string cacheKey = "Info_CashOrderCurrencies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCashOrderCurrencies(customerNumber);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetActionsForCardTransaction()
        {
            string cacheKey = "Info_TypeOfActions";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetActionsForCardTransaction();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetReasonsForCardTransactionAction(bool useBank)
        {
            string cacheKey = "ReasonsForCardTransactionAction_" + useBank;

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetReasonsForCardTransactionAction(useBank);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetCardLimitChangeOrderActionTypes()
        {
            string cacheKey = "CardLimitChangeOrderActionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardLimitChangeOrderActionTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի սառեցման տեսակները՝ սառեցման հայտի համար
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAccountFreezeReasonsTypesForOrder(User user, bool isHb)
        {
            DataTable dt = Info.GetFreezeReasonTypes(user);

            Dictionary<string, string> reasonTypes = new Dictionary<string, string>();
            var list = new List<int>();

            if (user.isBranchAccountsDiv)
            {
                list = new List<int> { 1, 2, 3, 9, 10, 11, 13, 14, 16, 17, 18, 19 };
            }
            else if (!user.isOnlineAcc)
            {
                list = new List<int> { 1, 2, 3, 9, 10, 11, 13, 14, 16, 17, 18, 19, 21 };
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (isHb)
                {
                    reasonTypes.Add(dt.Rows[i]["Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                }
                else if (user.isOnlineAcc)
                {
                    int isManual = int.Parse(dt.Rows[i]["manual"].ToString());
                    if (isManual != 0)
                    {
                        reasonTypes.Add(dt.Rows[i]["Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                    }
                }
                else
                {
                    ushort additionId = ushort.Parse(dt.Rows[i]["Id"].ToString());

                    if (!list.Contains(additionId))
                    {
                        reasonTypes.Add(dt.Rows[i]["Id"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
                    }
                }


            }
            return reasonTypes;
        }

        public static DataTable GetFreezeReasonTypes(User user)
        {

            string cacheKey = "Info_FreezeReasonTypes_" + user.userPermissionId.ToString();

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetFreezeReasonTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static DataTable GetUnfreezingReasonTypesForOrder(int freezeId)
        {

            string cacheKey = "Info_UnFreezeReasonTypes_" + freezeId.ToString();

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetUnFreezeReasonTypesForOrder(freezeId);
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է սառեցմանը համապատասխան ապասառեցման պատճառները
        /// </summary>
        /// <param name="freezeId"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetUnFreezeReasonsTypesForOrder(int freezeId)
        {
            DataTable dt = Info.GetUnfreezingReasonTypesForOrder(freezeId);

            Dictionary<string, string> reasonTypes = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ushort additionId = ushort.Parse(dt.Rows[i]["unfreeze_ID"].ToString());
                reasonTypes.Add(dt.Rows[i]["unfreeze_ID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
            }
            return reasonTypes;
        }

        public static DataTable GetTypeOfDepositStatus()
        {

            string cacheKey = "Info_TypeOfDepositStatus";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfDepositStatus();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static DataTable GetCreditLineMandatoryInstallmentTypes()
        {
            string cacheKey = "CreditLineMandatoryInstallmentTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetCreditLineMandatoryInstallmentTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;

        }

        public static DataTable GetTypeOfDeposit()
        {

            string cacheKey = "Info_TypeOfDeposit";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfDeposit();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetDepositCurrencies()
        {

            string cacheKey = "Info_DepositCurrencies";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDepositCurrencies();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }



        public static DataTable GetSwiftPurposeCode()
        {
            string cacheKey = "Info_GetSwiftPurposeCode";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSwiftPurposeCode();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        ///// <summary>
        ///// Ապառիկ տեղում խանութների ցուցակ
        ///// </summary>
        ///// <returns></returns>
        public static List<Shop> GetShopList()
        {
            return InfoDB.GetShopList();
        }

        /// <summary>
        /// Նոր սերնդի կանխիկի ընդունման տերմինալներով իրականացվող գործարքների տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSSTOperationTypes()
        {
            string cacheKey = "Info_SSTOperationTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetSSTOperationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Նոր սերնդի կանխիկի ընդունման տերմինալների ցանկ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSSTerminals(ushort filialCode)
        {
            DataTable dt = InfoDB.GetSSTerminals(filialCode);
            return dt;
        }
        public static Dictionary<string, string> GetCreditLineMandatoryInstallmentTypesDescription(Languages language)
        {
            Dictionary<string, string> CreditLineMandatoryInstallmentTypes = new Dictionary<string, string>();
            DataTable dt = GetCreditLineMandatoryInstallmentTypes();

            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CreditLineMandatoryInstallmentTypes.Add(dt.Rows[i]["Value"].ToString(), dt.Rows[i]["Description_Arm"].ToString());
                }
                return CreditLineMandatoryInstallmentTypes;
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CreditLineMandatoryInstallmentTypes.Add(dt.Rows[i]["Value"].ToString(), dt.Rows[i]["Description_Eng"].ToString());
                }
                return CreditLineMandatoryInstallmentTypes;
            }
        }

        public static DataTable GetCurrenciesForReceivedFastTransfer()
        {

            string cacheKey = "Info_Currencies_ReceivedFastTransfers";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCurrenciesForReceivedFastTransfer();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetTemplateDocumentTypes()
        {
            string cacheKey = "Info_TemplateDocumentTypes";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTemplateDocumentTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }


        public static DataTable GetCreditLineTypesForOnlineMobile(Languages language)
        {
            string cacheKey = "Info_GetCreditLineTypesforOnlineMobile";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCreditLineTypesForOnlineMobile();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static Dictionary<string, string> GetAttachedCardTypes(string mainCardNumber, ulong customerNumber)
        {
            int type = CardDB.GetCardType(mainCardNumber, customerNumber);
            DataTable dt = InfoDB.GetAttachedCardTypes(type);
            Dictionary<string, string> cards = dt.AsEnumerable()
                .ToDictionary(row => row[0].ToString(), row => row[1].ToString());
            return cards;
        }



        /// <summary>
        /// Վերադարձնում է աշխատավարձային ծրագրի համարը քարտի տիպից կախված
        /// </summary>
        /// <param name="cardType">Քարտի տեսակ</param>
        /// <param name="periodicityType">Հաճախականության տեսակ(ամիսների քանակ)</param>
        /// <returns></returns>
        public static int GetCardOfficeTypesForIBanking(ushort cardType, short periodicityType)
        {
            return InfoDB.GetCardOfficeTypesForIBanking(cardType, periodicityType);
        }

        /// <summary>
        /// Վերադարձնում է աշխատավարձային ծրագրի համարը տվյալ պահին առկա մարքեթինքային արշավից կախված
        /// </summary>
        /// <param name="cardType">Քարտի տեսակ</param>
        /// <param name="registrationDate">Հայտի մուտքագրման ամսաթիվ</param>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <returns></returns>
        public static int? GetCardOfficeTypesForMarketingCampaign(ushort cardType, DateTime registrationDate, ulong customerNumber)
        {
            return InfoDB.GetCardOfficeTypesForMarketingCampaign(cardType, registrationDate, customerNumber);
        }

        public static double GetCardServiceFee(int cardType, int officeId, string currency)
        {
            return InfoDB.GetCardServiceFee(cardType, officeId, currency);
        }

        public static DataTable GetAccountClosingReasonsHB()
        {
            string cacheKey = "Info_AccountClosingReasonsHB";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetAccountClosingReasonsHB();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է աբոնենտի տեսակները կոմունալ վճարման համար
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<byte, string>> GetUtilityAbonentTypes(Languages language)
        {
            string cacheKey = "Info_UtilityAbonentTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetUtilityAbonentTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<byte, string>> utilityAbonentTypes = new List<KeyValuePair<byte, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["description_arm"].ToString();
                    utilityAbonentTypes.Add(new KeyValuePair<byte, string>(Convert.ToByte(dt.Rows[i]["id"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityAbonentTypes.Add(new KeyValuePair<byte, string>(Convert.ToByte(dt.Rows[i]["id"]), dt.Rows[i]["description_eng"].ToString()));
                }
            }

            return utilityAbonentTypes;

        }

        /// <summary>
        /// Վերադարձնում է Online Banking-ում հասանելի կոմունալ վճարումների տեսակների ցանկը
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static List<KeyValuePair<short, string>> GetUtilityPaymentTypesOnlineBanking(Languages language)
        {
            string cacheKey = "Info_UtilityTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetUtilityPaymentTypesOnlineBanking();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<short, string>> utilityTypes = new List<KeyValuePair<short, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString());
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["ID_Transfer"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["ID_Transfer"]), dt.Rows[i]["Description_Engl"].ToString()));
                }
            }

            return utilityTypes;

        }
        /// <summary>
        /// Քարտային համակարգեր որոնք հնարավոր է պատվիրել
        /// </summary>
        public static DataTable GetOrderableCardSystemTypes()
        {
            string cacheKey = "Info_OrderableCardSystemTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetOrderableCardSystemTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Վերադարձնում է կոմունալի պարբերականների դեպքում վճարման տեսակների ցանկը (Պարտքի առկայության դեպքում/Անկախ պարտքի առկայությունից)
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static List<KeyValuePair<short, string>> GetPayIfNoDebtTypes(Languages language)
        {
            string cacheKey = "Info_NoDebTTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetPayIfNoDebtTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<short, string>> utilityTypes = new List<KeyValuePair<short, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = dt.Rows[i]["Description_arm"].ToString();
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), dt.Rows[i]["Description_Eng"].ToString()));
                }
            }

            return utilityTypes;

        }
        /// <summary>
        /// Քարտային համակարգեր որոնք հնարավոր է պատվիրել
        /// </summary>
        public static Dictionary<string, string> GetGasPromSectionCode()
        {
            string cacheKey = "Info_GetGasPromSectionCode";

            Dictionary<string, string> dt = CacheHelper.Get<Dictionary<string, string>>(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetGasPromSectionCode();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetVirtualCardStatusChangeReasons()
        {
            string cacheKey = "Info_VirtualCardStatusChangeReasons";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetVirtualCardStatusChangeReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetVirtualCardChangeActions(int status)
        {

            DataTable dt = InfoDB.GetVirtualCardChangeActions(status);

            return dt;
        }

        public static DataTable GetServiceFeePeriodocityTypes()
        {
            string cacheKey = "Info_GetServiceFeePeriodocityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetServiceFeePeriodocityTypes();
            }
            return dt;
        }
        /// <summary>
        /// Քարտի հեռացման հայտի պատճառներ
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetCardRemovalReasons()
        {
            string cacheKey = "Info_GetCardRemovalReasons";

            Dictionary<string, string> dt = CacheHelper.Get<Dictionary<string, string>>(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardRemovalReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        public static Dictionary<string, string> GetCurrenciesPlasticCardOrder(ushort cardType, short periodicityType)
        {
            int officeId = GetCardOfficeTypesForIBanking(cardType, periodicityType);
            DataTable data = InfoDB.GetCurrenciesPlasticCardOrder(cardType, officeId);
            Dictionary<string, string> currencies = data.AsEnumerable().ToDictionary(row => row.Field<string>(0),
                                row => row.Field<string>(0));
            return currencies;
        }
        //hb

        /// <summary>
        /// ՀԲ-ի աղբյուրի տեսակներ
        /// </summary>
        public static DataTable GetHBSourceTypes()
        {

            string cacheKey = "Info_HBSourceTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBSourceTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static Dictionary<string, string> GetLinkedCardTariffsByCardType(ushort cardType, byte language)
        {
            Dictionary<string, string> tariffs = new Dictionary<string, string>();

            switch (cardType)
            {
                case 42:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "անվճար");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "2․000 AMD");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "Free");
                        tariffs.Add("In case of three and more supplementery cards", "2․000 AMD");
                    }
                    break;
                case 49:
                case 24:
                case 34:
                case 50:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "2․000 AMD");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "-");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "2․000 AMD");
                        tariffs.Add("In case of three and more supplementery cards", "-");
                    }
                    break;
                case 40:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "4․000 AMD");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "-");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "4․000 AMD");
                        tariffs.Add("In case of three and more supplementery cards", "-");
                    }
                    break;
                case 41:
                case 20:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "անվճար");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "10․000 AMD");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "Free");
                        tariffs.Add("In case of three and more supplementery cards", "10.000 AMD");
                    }
                    break;
                case 22:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "անվճար");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "1․000 AMD");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "Free");
                        tariffs.Add("In case of three and more supplementery cards", "1.000 AMD");
                    }
                    break;
                case 45:
                case 3:
                case 18:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "անվճար");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "3․000 AMD");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "Free");
                        tariffs.Add("In case of three and more supplementery cards", "3.000 AMD");
                    }
                    break;
                default:
                    if (language == 1)
                    {
                        tariffs.Add("Առաջին երկու կից քարտերի դեպքում", "անվճար");
                        tariffs.Add("Երեք և ավելի կից քարտերի դեպքում", "-");
                    }
                    else
                    {
                        tariffs.Add("In case of the first two supplementery cards", "Free");
                        tariffs.Add("In case of three and more supplementery cards", "-");
                    }
                    break;
            }

            return tariffs;
        }

        public static DataTable GetLoanMatureTypesForIBanking()
        {

            string cacheKey = "Info_LoanMatureTypesForIBanking";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLoanMatureTypesForIBanking();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static List<KeyValuePair<string, string>> GetDetailsOfCharges()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("OUR","OUR"),
                new KeyValuePair<string, string>("BEN","BEN"),
                new KeyValuePair<string, string>("OUROUR","OUROUR")
            };
        }

        public static string GetFilialName(int filialCode, Languages language)
        {
            string description = InfoDB.GetFilialName(filialCode, language);
            description = description.Replace("§", "«");
            description = description.Replace("¦", "»");
            return description;
        }
        public static Dictionary<string, string> GetCardClosingReasonsForDigitalBanking(Languages language)
        {
            string cacheKey = "Info_CardClosingReasonsForDigitalBanking";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetCardClosingReasonsForDigitalBanking();
                CacheHelper.Add(dt, cacheKey);
            }


            Dictionary<string, string> closingReasons = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                closingReasons.Add(dt.Rows[i]["id"].ToString(), language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_eng"].ToString()));
            }


            return closingReasons;
        }
        /// <summary>
        /// ՀԲ-ի գործարքի կարգավիճակի տեսակներ
        /// </summary>
        public static DataTable GetHBQualityTypes()
        {

            string cacheKey = "Info_HBQualityTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBQualityTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// ՀԲ-ի գործարքի տեսակներ
        /// </summary>
        public static DataTable GetHBDocumentTypes()
        {

            string cacheKey = "Info_HBDocumentTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBDocumentTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// ՀԲ-ի փոխանցման տեսակներ
        /// </summary>
        public static DataTable GetHBDocumentSubtypes()
        {

            string cacheKey = "Info_HBDocumentSubTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBDocumentSubtypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// ՀԲ-ի մերժման պատճառի տեսակներ
        /// </summary>
        public static DataTable GetHBRejectTypes()
        {

            string cacheKey = "Info_HBRejectTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetHBRejectTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է ընդհանուր հասանելի մնացորդի կարգավորման տեսակը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountRestTypes()
        {

            string cacheKey = "Info_GetAccountRestTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetAccountRestTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        /// <summary>
        /// Փոխանցում ստացող տեսակների աղյուսակ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTransferReceiverTypes()
        {

            string cacheKey = "Info_GetTransferReceiverTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransferReceiverTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static byte CommunicationTypeExistence(ulong Customer_number)
        {
            return InfoDB.CommunicationTypeExistence(Customer_number);
        }

        public static double GetMaxAvailableAmountForNewCreditLine(double productId, int creditLineType, string provisionCurrency, bool existRequiredEntries, ulong customerNumber)
        {
            return InfoDB.GetMaxAvailableAmountForNewCreditLine(productId, creditLineType, provisionCurrency, existRequiredEntries, customerNumber);
        }
        /// <summary>
        /// Քարտի ստացման եղանակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCardReceivingTypes()
        {
            string cacheKey = "Info_CardReceivingTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardReceivingTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        /// <summary>
        /// Քարտի դիմումի ընդունման տարբերակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCardApplicationAcceptanceTypes()
        {
            string cacheKey = "Info_CardApplicationAcceptance";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardApplicationAcceptanceTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static string GetCurrencyLetter(string currency)
        {
            return InfoDB.GetCurrencyLetter(currency);
        }

        public static string GetMandatoryEntryInfo(byte id, byte language)
        {
            string cacheKey = "Info_GetMandatoryEntryInfo" + id + (int)language;

            string result = CacheHelper.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(result))
            {
                result = InfoDB.GetMandatoryEntryInfo(id, language);
                CacheHelper.Add(result, cacheKey);
            }

            return result;
        }

        /// <summary>
        /// Տեղեկանքի ստացման հայտի տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetReferenceReceiptTypes(byte language)
        {
            string cacheKey = "Info_ReferenceReceiptTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetReferenceReceiptTypes(language);
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static DataTable GetCustomerAllPassports(ulong customerNumber)
        {
            DataTable dt;
            dt = InfoDB.GetCustomerAllPassports(customerNumber);
            return dt;
        }

        public static DataTable GetCustomerAllPhones(ulong customerNumber)
        {
            DataTable dt;
            dt = InfoDB.GetCustomerAllPhones(customerNumber);
            return dt;
        }

        public static DataTable GetCardAdditionalDataTypes(string cardNumber, string expiryDate)
        {
            DataTable dt = InfoDB.GetCardAdditionalDataTypes(cardNumber, expiryDate);
            return dt;
        }

        public static string GetSentCityBySwiftCode(string swiftCode)
        {
            return InfoDB.GetSentCityBySwiftCode(swiftCode);
        }

        public static DataTable GetCardNotRenewReasons()
        {
            string cacheKey = "Info_CardNotRenewReasons";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCardNotRenewReasons();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static DataTable GetReasonsForCardTransactionActionForUnblocking()
        {
            string cacheKey = "ReasonsForCardTransactionActionForUnblocking_";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetReasonsForCardTransactionActionForUnblocking();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Հաճախորդը ունի սոցիալական ապահովության հաշիվ կամ ARCA PENSION card
        /// </summary>
        /// <param name="customerNumber"></param>
        public static bool HasSocialSecurityAccount(ulong customerNumber) => InfoDB.HasSocialSecurityAccount(customerNumber);

        public static string GetLoanMatureTypeDescriptionForIBankingByMatureType(int matureType, byte lang)
        {
            var types = GetLoanMatureTypesForIBanking();

            for (int i = 0; i < types.Rows.Count; i++)
            {
                if (types.Rows[i]["id"].ToString() == matureType.ToString())
                {
                    return lang == (byte)Languages.hy ? types.Rows[i]["description_arm"].ToString() : types.Rows[i]["description_eng"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Վերադարձնում է պայմանագիր կարգավիճակով վարկի հեռացման պատճառը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTypeOfLoanDeleteInfo()
        {

            string cacheKey = "Info_TypeOfLoanDelete";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTypeOfLoanDelete();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }
        /// <summary>
        /// Վերադարձնում է պայմանագիր կարգավիճակով վարկի հեռացման պատճառը
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetTypeOfLoanDelete()
        {
            DataTable dt = GetTypeOfLoanDeleteInfo();

            Dictionary<string, string> reasonTypes = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                reasonTypes.Add(dt.Rows[i]["ID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString()));
            }
            return reasonTypes;
        }


        /// <summary>
        /// Վերադարձնում է հօգուտ 3-րդ անձի հաշիվները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetThirdPersonAccounts(ulong customerNumber)
        {
            DataTable dt = InfoDB.GetThirdPersonAccounts(customerNumber);
            return dt;
        }

        /// <summary>
        /// Վերադարձնում է նոր ավանդի հայտի ժամանակ առաջարկվող ավանդատեսակները DIgital-ի համար
        /// </summary>
        /// <returns></returns>
        public static DataTable GetActiveDepositTypesForNewDepositOrderForDigitalBanking(short allowableCustomerType, short allowableForThirdhPerson, short allowableForCooperative)
        {
            DataTable dt = InfoDB.GetActiveDepositTypesForNewDepositOrderForDigitalBanking(allowableCustomerType, allowableForThirdhPerson, allowableForCooperative);
            return dt;
        }

        /// <summary>
        /// Դիջիթալից մուտքագրվող հօգուտ 3-րդ անձի ավանդի համար վերադարձնում է հասանելի արժույթները։
        /// </summary>
        /// <returns></returns>
        public static DataTable GetJointDepositAvailableCurrency(ulong customerNumber, ulong thirdPersonCustomerNumber)
        {
            DataTable dt = InfoDB.GetJointDepositAvailableCurrency(customerNumber, thirdPersonCustomerNumber);
            return dt;
        }

        /// <summary>
        /// Վերադարձնում է միջնորդավճարի չգանձման պատճառը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCommissionNonCollectionReasons()
        {

            string cacheKey = "Info_CommissionNonCollectionReasons";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetCommissionNonCollectionReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        public static DataTable GetDepositoryAccountOperators()
        {
            string cacheKey = "Info_DepositoryAccountOperators";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetDepositoryAccountOperators();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

        /// <summary>
        /// Վերադարձնում է գործարքի տեսակները ըստ Ֆինանսական անվտանգության բաժնի
        /// </summary>
        /// <returns></returns>
        public static DataTable GetTransactionTypes()
        {
            string cacheKey = "Info_TransactionTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetTransactionTypes();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }

        public static int GetBannerVersion() => InfoDB.GetBannerVersion();

        /// <summary>
        /// Վերադարձնում է արժեթղթերի տեսակները
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<short, string>> GetSecuritiesTypes(Languages language)
        {
            string cacheKey = "Info_SecuritiesTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetSecuritiesTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<short, string>> utilityTypes = new List<KeyValuePair<short, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString());
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), dt.Rows[i]["description_eng"].ToString()));
                }
            }

            return utilityTypes;

        }

        public static List<KeyValuePair<short, string>> GetTradingOrderTypes(Languages language)
        {
            string cacheKey = "Info_TradingOrderTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTradingOrderTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<short, string>> utilityTypes = new List<KeyValuePair<short, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString());
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), dt.Rows[i]["description_eng"].ToString()));
                }
            }

            return utilityTypes;

        }

        public static string GenerateBrokerContractNumber() => InfoDB.GenerateBrokerContractNumber();

        public static List<KeyValuePair<short, string>> GetTradingOrderExpirationTypes(Languages language)
        {
            string cacheKey = "Info_TradingOrderExpirationTypes_" + language.ToString();
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = InfoDB.GetTradingOrderExpirationTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            List<KeyValuePair<short, string>> utilityTypes = new List<KeyValuePair<short, string>>();
            if (language == Languages.hy)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description_arm"].ToString());
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), description));
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    utilityTypes.Add(new KeyValuePair<short, string>(Convert.ToInt16(dt.Rows[i]["id"]), dt.Rows[i]["description_eng"].ToString()));
                }
            }

            return utilityTypes;

        }

        public static bool IsACBAGroupEmployee(ulong customerNumber) => InfoDB.IsACBAGroupEmployee(customerNumber);

        public static DateTime? DateOfACBAGroupEmployee(ulong customerNumber) => InfoDB.DateOfACBAGroupEmployee(customerNumber);
        
        /// <summary>
        /// Լիզինգի Ելքային ձևերի հաշվետվությունների տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLeasingReportTypes()
        {
            string cacheKey = "Info_LeasingReportTypes";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLeasingReportTypes();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }


        public static DataTable GetLeasingCredentialClosingReasons()
        {
            string cacheKey = "Info_LeasingCredentialClosingReason";

            DataTable dt = CacheHelper.Get(cacheKey);

            if (dt == null)
            {
                dt = InfoDB.GetLeasingCredentialClosingReasons();
                CacheHelper.Add(dt, cacheKey);
            }

            return dt;
        }

    }
}
