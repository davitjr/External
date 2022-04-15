using ExternalBanking.DBManager;
using ExternalBanking.UtilityPaymentsManagment;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Կոմունալ վճարման մանրամասներ
    /// </summary>
    public class CommunalDetails
    {
        /// <summary>
        /// Հերթական համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Արժեք
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Կոմունալ վճարման տվյալները վերադարձնող ֆունկցիա
        /// </summary>
        /// <param name="communalType">Կոմունալի տեսակ</param>
        /// <param name="abonentNumber">Աբոնենտի համար</param>
        /// <param name="checkType">1 հիմնական տվյալներ,2 մանրամասն տվյալներ</param>
        /// <param name="language">տվյալների ցուցադրման լեզու</param>
        /// <returns></returns>
        public static List<CommunalDetails> GetCommunalDetails(CommunalTypes communalType, string abonentNumber, short checkType, byte language, string branchCode, AbonentTypes abonentType, SourceType source)
        {
            List<CommunalDetails> communalDetails = new List<CommunalDetails>();

            if (source != SourceType.MobileBanking || communalType == CommunalTypes.ArmenTel || communalType == CommunalTypes.VivaCell || communalType == CommunalTypes.Orange || communalType == CommunalTypes.BeelineInternet)
            {
                switch (communalType)
                {
                    case CommunalTypes.ENA:
                        communalDetails = CommunalDB.GetENADetails(abonentNumber, checkType, branchCode, abonentType);
                        break;
                    case CommunalTypes.Gas:
                        List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
                        if (abonentType == AbonentTypes.physical)
                        {
                            gazList = CommunalDB.SearchFullCommunalGas(abonentNumber, branchCode);
                        }
                        communalDetails = CommunalDB.GetGasPromDetails(abonentNumber, checkType, branchCode, abonentType, gazList);
                        break;
                    case CommunalTypes.ArmWater:
                        communalDetails = CommunalDB.GetArmWaterDetails(abonentNumber, checkType, branchCode, abonentType);
                        break;
                    case CommunalTypes.YerWater:
                        communalDetails = CommunalDB.GetYerevanJurDetails(abonentNumber, checkType, branchCode, abonentType);
                        break;
                    case CommunalTypes.ArmenTel:
                    case CommunalTypes.BeelineInternet:
                        communalDetails = CommunalDB.GetCommunalDetailsForArmenTel(abonentNumber, communalType);
                        break;
                    case CommunalTypes.VivaCell:
                        communalDetails = CommunalDB.GetCommunalDetailsForVivaCell(abonentNumber);
                        break;
                    case CommunalTypes.Orange:
                        communalDetails = CommunalDB.GetCommunalDetailsForOrange(abonentNumber);
                        break;
                    case CommunalTypes.UCom:
                        communalDetails = CommunalDB.GetCommunalDetailsForUcom(abonentNumber);
                        break;
                    case CommunalTypes.Trash:
                        communalDetails = CommunalDB.CommunalDetailsForTrash(abonentNumber, branchCode);
                        break;
                    case CommunalTypes.COWater:
                        communalDetails = CommunalDB.CommunalDetailsForCOWater(abonentNumber, branchCode, checkType);
                        return communalDetails;
                    default:
                        break;
                }
            }
            else
            {
                if (communalType == CommunalTypes.Gas && abonentType == AbonentTypes.physical)
                {
                    List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
                    if (abonentType == AbonentTypes.physical)
                    {
                        gazList = CommunalDB.SearchFullCommunalGas(abonentNumber, branchCode);
                    }
                    communalDetails = CommunalDB.GetGasPromDetails(abonentNumber, checkType, branchCode, abonentType, gazList);
                }
                else
                {
                    communalDetails = CommunalDB.GetCommunalDetails(communalType, abonentNumber, checkType, branchCode, abonentType);
                }
            }

            Culture Culture = new Culture((Languages)language);
            Localization.SetCulture(communalDetails, Culture);

            return communalDetails;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abonentNumber"></param>
        /// <param name="branchCode"></param>
        /// <param name="num">1 i jamanak online - i save -na ashxatum</param>
        /// <returns></returns>
        public static string SearchFullCommunalGasOnline(string abonentNumber, string branchCode, int num = 0)
        {
            return CommunalDB.SearchFullCommunalGasOnline(abonentNumber, branchCode, num);
        }

        public static Dictionary<string, string> SerchGasPromForReport(string abonentNumber, string branchCode)
        {
            return CommunalDB.SerchGasPromForReport(abonentNumber, branchCode);
        }

    }
}
