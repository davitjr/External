using ExternalBanking.DBManager;
using ExternalBanking.UtilityPaymentsManagment;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Configuration;

namespace ExternalBanking
{
    /// <summary>
    /// Կոմունալ վճարումների որոնում
    /// </summary>
    public class SearchCommunal
    {
        /// <summary>
        /// Կոմունալի տեսակ
        /// </summary>
        public CommunalTypes CommunalType { get; set; }

        /// <summary>
        /// Աբոնենտի տեսակ ֆիզ անձ,իրավ.անձ
        /// </summary>
        public short AbonentType { get; set; }
        /// <summary>
        /// Աբոնենտի համար
        /// </summary>
        public string AbonentNumber { get; set; }
        /// <summary>
        /// Հեոախոսի համար
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Աբոնենտի գրանցման փողոց
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Աբոնենտի գրանցման շենք
        /// </summary>
        public string House { get; set; }

        /// <summary>
        /// Աբոնենտի գրանցման տուն
        /// </summary>
        public string Home { get; set; }

        /// <summary>
        /// Աբոնենտի անուն
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Աբոնենտի ազգանուն
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Պարտք
        /// </summary>
        public string Debt { get; set; }

        /// <summary>
        /// Որոշիչ
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Աբոնենտի մասնաճյուղ
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// ՋՕԸ մասնաճյուղ
        /// </summary>
        public string CoWaterBranch { get; set; }

        /// <summary>
        /// Պարտքերի ցուցակի ա/թ
        /// </summary>
        public DateTime DebtListDate { get; set; }

        /// <summary>
        /// Համայնք
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public string FilialCode { get; set; }

        /// <summary>
        /// Վճարման տեսակ օր՝ անդամավճար կամ ոռոգմնան վճար
        /// </summary>
        public ushort PaymentType { get; set; }

        /// <summary>
        /// Աղբահանության որոնման ժամանակ երբ կատարվում է որոնում
        /// ոչ կոմունալի քարտի համար անհրաժեշտ է որ proceduran որոնում կատարի LIKE-ով
        /// իսկ կոմունալի քարտի համար անհրաժեշտ է որ որոնումը լինի =-ով
        /// </summary>
        public bool FindByEqualAbonentNumberAndBranch { get; set; }



        public List<Communal> SearchCommunalByType(SourceType source, bool isSearch = true, int groupId = 0)
        {
            List<Communal> communals = new List<Communal>();



            switch (CommunalType)
            {
                case CommunalTypes.ENA:
                    communals = (source == SourceType.MobileBanking || source == SourceType.AcbaOnline) ? CommunalDB.SearchCommunalENAForMobile(this) : CommunalDB.SearchCommunalENA(this);
                    break;
                case CommunalTypes.Gas:
                    communals = source == SourceType.MobileBanking ? CommunalDB.SearchCommunalGasForMobile(this) : CommunalDB.SearchCommunalGas(this, source);
                    //if ((source == SourceType.MobileBanking || source == SourceType.AcbaOnline) && AbonentType == 1)
                    //{
                    //    foreach (Communal c in communals)
                    //    {
                    //        if (c.Debt < 0)
                    //        {
                    //            c.Debt = Convert.ToDouble(Math.Abs(Convert.ToDecimal(c.Debt)));
                    //        }
                    //        else if(c.Debt > 0)
                    //        {
                    //            c.Debt *= -1;
                    //        }
                    //        else
                    //        {
                    //            if(groupId == 0)
                    //                c.Debt = 0;
                    //        }
                    //    }
                    //}
                    break;
                case CommunalTypes.ArmWater:
                    communals = (source == SourceType.MobileBanking || source == SourceType.AcbaOnline) ? CommunalDB.SearchCommunalArmWaterForMobile(this) : CommunalDB.SearchCommunalArmWater(this);
                    break;
                case CommunalTypes.YerWater:
                    communals = (source == SourceType.MobileBanking || source == SourceType.AcbaOnline) ? CommunalDB.SearchCommunalYerWaterForMobile(this) : CommunalDB.SearchCommunalYerWater(this, isSearch);
                    break;
                case CommunalTypes.ArmenTel:
                case CommunalTypes.BeelineInternet:
                    communals = CommunalDB.SearchBeeline(this, CommunalType);      //  source == SourceType.MobileBanking ? CommunalDB.SearchBeeline(this) : CommunalDB.SearchBeeline(this);
                    break;
                case CommunalTypes.VivaCell:
                    //communals = source == SourceType.MobileBanking ? CommunalDB.SearchCommunalVIvaCellForMobile(this) : CommunalDB.SearchCommunalVIvaCell(this);

                    communals = SearchVivaCellSubscriber(this, source);

                    if (source == SourceType.MobileBanking || source == SourceType.AcbaOnline)
                    {
                        foreach (Communal c in communals)
                        {
                            c.Description = GetCommunalPaymentDescription();
                        }
                    }

                    break;
                case CommunalTypes.Orange:  //7656
                    communals = CommunalDB.SearchCommunalOrange(this, CommunalType);
                    if (source == SourceType.MobileBanking || source == SourceType.AcbaOnline)
                    {
                        foreach (Communal c in communals)
                        {
                            c.Description = GetCommunalPaymentDescription();
                        }
                    }
                    break;
                case CommunalTypes.UCom:
                    bool ucomNewVersion = bool.Parse(WebConfigurationManager.AppSettings["UcomFixNewVersion"].ToString());
                    if (ucomNewVersion)
                    {
                        communals = CommunalDB.SearchCommunalUCom(this);
                    }
                    else
                    {
                        communals = source == SourceType.MobileBanking ? CommunalDB.SearchCommunalUComForMobile(this) : CommunalDB.SearchCommunalUCom(this);
                    }
                    break;
                case CommunalTypes.Trash:
                    communals = CommunalDB.SearchCommunalDetailsForTrash(this);
                    break;
                case CommunalTypes.COWater:
                    communals = CommunalDB.SearchCommunalDetailsForCOWater(this);
                    break;
                default:
                    break;
            }

            return communals;

        }


        public List<KeyValuePair<string, string>> GetCommunalReportParameters()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            DataTable dt = CommunalDB.SearchCommunalData(this);

            if (CommunalType == CommunalTypes.UCom)
            {
                UcomFixAbonentSearch abonentSearch = new UcomFixAbonentSearch();
                abonentSearch = abonentSearch.GetUcomFixAbonentSearch(this.AbonentNumber);

                parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: abonentSearch.Client));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Internet", value: abonentSearch.Balance.Internet.ToString("F2")));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_TV", value: abonentSearch.Balance.TV.ToString("F2")));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Phone", value: abonentSearch.Balance.Phone.ToString("F2")));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Other", value: abonentSearch.Balance.Other.ToString("F2")));
            }
            else if (CommunalType == CommunalTypes.ENA)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: dt.Rows[0]["abonent_name"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "ID_ENA", value: dt.Rows[0]["ID"].ToString()));
            }
            else if (CommunalType == CommunalTypes.VivaCell)
            {
                //DataTable dt2 = CommunalDB.GetVivaCellData(this.AbonentNumber, 1);

                //parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: dt.Rows[0]["abonent_name"].ToString()));
                //parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt", value: (Convert.ToInt32(dt2.Rows[0]["debt"]) * -1).ToString()));
                //parameters.Add(new KeyValuePair<string, string>(key: "IsPrepaid", value: Convert.ToInt32(dt.Rows[0]["prepaid"].ToString()) == 1 ? "0" : "1"));


                // SearchVivaCellSubscriber
                VivaCellSubscriberSearch vivaCellSubscriber = new VivaCellSubscriberSearch();
                vivaCellSubscriber.GetVivaCellSubscriberDetails(this.AbonentNumber);

                if (vivaCellSubscriber.PhoneNumber != null)
                {
                    parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: vivaCellSubscriber.SubscriberName));
                    parameters.Add(new KeyValuePair<string, string>(key: "IsPrepaid", value: Convert.ToInt32(vivaCellSubscriber.SubscriberType) == 1 ? "0" : "1"));

                    if (vivaCellSubscriber.SubscriberType == VivaCellSubscriberType.Postpaid)
                    {
                        short physicalOrLegal = 1;
                        short debtSign = GetCommunalDebtSign((short)CommunalTypes.VivaCell, physicalOrLegal);
                        parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt", value: (Convert.ToInt32(vivaCellSubscriber.BalanceSub * debtSign) * -1).ToString()));
                    }
                    else
                    {
                        parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt", value: (Convert.ToInt32(0).ToString())));
                    }
                }
            }
            else if (CommunalType == CommunalTypes.Orange)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: dt.Rows[0]["abonent_name"].ToString()));
            }

            return parameters;
        }


        public string GetCommunalPaymentDescription()
        {
            string description = "";
            DataTable dt;
            if (this.CommunalType == CommunalTypes.Trash)
            {

                dt = CommunalDB.GetOneTrashData(this.AbonentNumber, this.Branch);

                description = "Աղբահանության վարձի վճար ";
                if (dt.Rows.Count > 0)
                {
                    description = description + " / " + dt.Rows[0]["ANUN"].ToString().Trim() + " " + dt.Rows[0]["AZGANUN"].ToString().Trim() + ", "
                        + dt.Rows[0]["PHOXOC"].ToString().Trim() + ", " + dt.Rows[0]["SHENQ"].ToString().Trim() + ", " + dt.Rows[0]["BNAK"].ToString().Trim() + " ," + dt.Rows[0]["COD"].ToString().Trim() + " /";
                }
                description = Utility.ConvertAnsiToUnicode(description);
                return description;
            }
            else if (this.CommunalType == CommunalTypes.COWater)
            {
                description = "";
                if (this.PaymentType == 0)
                {
                    return description;
                }

                dt = CommunalDB.GetOneCOWaterData(this.AbonentNumber, this.Branch);
                if (dt.Rows.Count == 0 || dt.Rows.Count > 1)
                {
                    return description;
                }

                if (this.PaymentType == 1)
                {
                    description = "Անդամավճար /" + dt.Rows[0]["AAH"].ToString() + "," + dt.Rows[0]["City"] + " / " + " (" + this.AbonentNumber + ")";
                }
                else if (this.PaymentType == 2)
                {
                    description = "Ոռոգ. ջրի վարձ /" + dt.Rows[0]["AAH"].ToString() + "," + dt.Rows[0]["City"] + " / " + " (" + this.AbonentNumber + ")";
                }


                description = Utility.ConvertAnsiToUnicode(description);
                return description;
            }



            if (this.CommunalType == CommunalTypes.ArmenTel)
            {
                description = "Team վճարում/" + this.AbonentNumber.ToString() + "/";
            }
            else if (this.CommunalType == CommunalTypes.BeelineInternet)
            {
                description = "Team ինտերնետ վճարում/" + this.AbonentNumber.ToString() + "/";
            }
            else if (this.CommunalType == CommunalTypes.VivaCell)
            {
                description = "ՄՏՍ ՀԱՅԱՍՏԱՆ վճարում/" + this.AbonentNumber.ToString() + "/";
            }
            else if (this.CommunalType == CommunalTypes.Gas && this.AbonentType == 1)
            {
                List<GasPromAbonentSearch> gazList = CommunalDB.SearchFullCommunalGas(this.AbonentNumber, this.Branch);

                if (gazList.Count != 0)
                {
                    string[] str = gazList[0].Name.Split(' ');
                    string Name = "";
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i].Length > 1)
                        {
                            Name = Name + str[i] + " ";
                        }
                    }

                    description = "Սպառած գազի վճար/" + this.AbonentNumber.ToString() + "," + Name + " " + gazList[0].LastName.Replace(" ", "") + " " + gazList[0].Street.Replace(" ", "") + " " + gazList[0].House.Replace(" ", "") + " " + gazList[0].Home.Replace(" ", "");
                }
                else
                    description = string.Empty;
            }
            else
            {
                dt = CommunalDB.SearchCommunalData(this);
                if (dt.Rows.Count > 0)
                {
                    switch (CommunalType)
                    {
                        case CommunalTypes.ENA:
                            description = "Էլ. էներգիայի վճար/" + dt.Rows[0]["abonent_name"].ToString() + "," + dt.Rows[0]["abonent_address"].ToString() + "," + dt.Rows[0]["ID"];
                            break;
                        case CommunalTypes.Gas:
                            description = "Սպառած գազի վճար/" + dt.Rows[0]["Cod"].ToString() + "," + dt.Rows[0]["abonent_name"].ToString() + "," + dt.Rows[0]["abonent_address"].ToString();
                            break;
                        case CommunalTypes.ArmWater:
                            description = GetArmWaterBranchDescription(int.Parse(dt.Rows[0]["branch_cod"].ToString()), int.Parse(dt.Rows[0]["service"].ToString()));
                            description = description + "/" + dt.Rows[0]["cod"].ToString() + "," + dt.Rows[0]["abonent_name"].ToString() + "," + dt.Rows[0]["abonent_address"].ToString();
                            break;
                        case CommunalTypes.YerWater:
                            description = "§Վեոլիա Ջուր¦  ջրի վճար/" + dt.Rows[0]["abonent_name"].ToString() + "," + dt.Rows[0]["abonent_address"].ToString() + "," + dt.Rows[0]["cod"].ToString();
                            break;
                        //case CommunalTypes.ArmenTel:
                        //    description = "ԱրմենՏել պարտքի վճար/" + dt.Rows[0]["cod"].ToString() + "/";
                        //    break;
                        //case CommunalTypes.VivaCell:
                        //    description = "ՄՏՍ ՀԱՅԱՍՏԱՆ վճարում/" + dt.Rows[0]["cod"].ToString() + "/";
                        //    break;
                        case CommunalTypes.Orange:
                            description = "Յուքոմ բջջ. վճարում/" + dt.Rows[0]["cod"].ToString() + "/";
                            break;
                        case CommunalTypes.UCom:
                            description = "§Յուքոմ¦ վճարում/" + dt.Rows[0]["abonent_name"].ToString() + "," + dt.Rows[0]["cod"].ToString() + "/";
                            break;
                        default:
                            break;
                    }

                }

            }


            return Utility.ConvertAnsiToUnicode(description);
        }




        public static string GetArmWaterBranchDescription(int masn, int service)
        {
            string cacheKey = "Info_ArmWaterBranch";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = CommunalDB.GetArmWaterBranchDescription();
                CacheHelper.Add(dt, cacheKey);
            }
            if (masn != 13 && masn != 24 && masn != 61)
            {
                masn = 0;
            }

            DataRow[] dr = dt.Select("masn=" + masn);

            string description = "";
            switch (service)
            {
                case 0:
                    description = "Ընթացիկ ամսում չի սպասարկվել";
                    break;
                case 1:
                    description = "Ջրամատակարարում և ջրահեռացում";
                    break;
                case 2:
                    description = "Միայն ջրամատակարարում";
                    break;
                case 3:
                    description = "Միայն ջրահեռացում";
                    break;
                case 5:
                    description = "Բոլորը";
                    break;
                default:
                    break;
            }


            description += "(" + dr[0]["description"].ToString() + ")";

            return Utility.ConvertAnsiToUnicode(description);
        }

        public static bool IsPrepaidArmenTel(SearchCommunal searchCommunal)
        {
            bool isPrepaidArmenTel = false;

            if ( //CommunalDB.SearchCommunalVIvaCellForMobile(searchCommunal).Count == 0 && 
                CommunalDB.SearchCommunalOrangeForMobile(searchCommunal).Count == 0 &&
                CommunalDB.SearchCommunalArmenTelForMobile(searchCommunal).Count == 0)
            {
                isPrepaidArmenTel = true;
            }

            return isPrepaidArmenTel;

        }


        /// <summary>
        /// Վերադարձնում է կումալների բազաների ա/թ-երը
        /// </summary>
        /// <param name="cmnlType"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetCommunalDate(CommunalTypes cmnlType, short abonentType = 1)
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            if (cmnlType == CommunalTypes.Trash)
            {
                date = new KeyValuePair<string, string>(CommunalDB.GetTrashDate(), "");
            }
            else if (cmnlType == CommunalTypes.ENA)
            {
                return CommunalDB.GetENADate();
            }
            else if (cmnlType == CommunalTypes.ArmWater)
            {
                return CommunalDB.GetArmWaterDate();
            }
            else if (cmnlType == CommunalTypes.YerWater)
            {
                return CommunalDB.GetYerWaterDate();
            }
            else if (cmnlType == CommunalTypes.Gas)
            {
                if (abonentType == 1)
                    return CommunalDB.GetGasDateForPhysical();
                else
                    return CommunalDB.GetGasDateForLegal();
            }


            return date;
        }


        /// <summary>
        /// Վերադարձնում է ՋՕԸ-ները
        /// </summary>
        /// <returns></returns>
        public static List<WaterCoDetail> GetWaterCoDetails()
        {
            return CommunalDB.GetWaterCoDetails();
        }

        /// <summary>
        /// Վերադարձնում է ՋՕԸ-ների  պարտքերի ցուցակի ա/թ-ները
        /// </summary>
        /// <returns></returns>
        public static List<DateTime> GetWaterCoDebtDates(ushort code)
        {
            return CommunalDB.GetWaterCoDebtDates(code);
        }

        /// <summary>
        /// Վերադարձնում է ՋՕԸ-ների  մասնաճյուղերը
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetWaterCoBranches(ushort filialCode)
        {
            return CommunalDB.GetWaterCoBranches(filialCode);
        }

        /// <summary>
        /// Վերադարձնում է ռեեստրով ՋՕԸ-ների  մասնաճյուղերը
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetReestrWaterCoBranches(ushort filialCode)
        {
            return CommunalDB.GetReestrWaterCoBranches(filialCode);
        }

        /// <summary>
        /// Վերադարձնում է ՋՕԸ-ների  համայնքները
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetWaterCoCitys(ushort code)
        {
            return CommunalDB.GetWaterCoCitys(code);
        }


        /// <summary>
        /// Վերադարձնում է ՋՕԸ-ի մասնաճյուղի ID
        /// </summary>
        /// <returns></returns>
        public static string GetCOWaterBranchID(string code, string filialCode)
        {
            return CommunalDB.GetCOWaterBranchID(code, filialCode);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ամսում կոմունալ ծառայության համար վճարված գումարը
        /// </summary>
        /// <returns></returns>
        public static List<double> GetComunalAmountPaidThisMonth(string code, short comunalType, short abonentType, DateTime DebtListDate, string texCode, int waterCoPaymentType)
        {
            return CommunalDB.GetComunalAmountPaidThisMonth(code, comunalType, abonentType, DebtListDate, texCode, waterCoPaymentType);
        }


        internal static List<Communal> SearchVivaCellSubscriber(SearchCommunal searchCommunal, SourceType source)
        {
            short debtSign;
            short physicalOrLegal = 1;

            List<Communal> communalsList = new List<Communal>();

            VivaCellSubscriberSearch vivaCellSubscriber = new VivaCellSubscriberSearch();
            vivaCellSubscriber.GetVivaCellSubscriberDetails(searchCommunal.AbonentNumber);

            if (vivaCellSubscriber.PhoneNumber != null)
            {
                Communal vivaCell = new Communal();

                vivaCell.ComunalType = CommunalTypes.VivaCell;
                vivaCell.AbonentNumber = searchCommunal.AbonentNumber;
                vivaCell.PrepaidSign = vivaCellSubscriber.SubscriberType ==
                      VivaCellSubscriberType.Postpaid ? PrepaidSign.Prepaid : (vivaCellSubscriber.SubscriberType == VivaCellSubscriberType.Prepaid ? PrepaidSign.NotPrepaid : PrepaidSign.NotDefined);

                if (vivaCellSubscriber.SubscriberType == VivaCellSubscriberType.Postpaid)
                {
                    debtSign = GetCommunalDebtSign((short)CommunalTypes.VivaCell, physicalOrLegal);

                    vivaCell.Description = vivaCellSubscriber.SubscriberName;
                    vivaCell.Debt = Convert.ToDouble(vivaCellSubscriber.BalanceSub * debtSign);
                }
                else if (vivaCellSubscriber.SubscriberType == VivaCellSubscriberType.Prepaid)
                {
                    vivaCell.Description = "Կանխավճարային";
                    vivaCell.Debt = 0;
                }


                communalsList.Add(vivaCell);
            }

            return communalsList;
        }

        public static short GetCommunalDebtSign(short communalType, short physicalOrLegal)
        {
            return CommunalDB.GetCommunalDebtSignDB(communalType, physicalOrLegal);
        }

        public List<KeyValuePair<string, string>> GetCommunalReportParametersIBanking(long orderId, CommunalTypes communalType)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            int abonentType = UtilityPaymentOrder.GetOrderAbonentType(orderId);
            DataTable dt = CommunalDB.GetUtilityPaymentData(orderId, communalType, abonentType);

            if (communalType == CommunalTypes.Gas)
            {

                if (abonentType == 1)  //Ֆիզիկական
                {
                    parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Fillial"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "OrderNum", value: dt.Rows[0]["order_number"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "GasPreviousPayment", value: dt.Rows[0]["gas_previous_payment"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "Cod", value: dt.Rows[0]["Abonent_number"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "TexCod", value: dt.Rows[0]["section_code"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "AmountPaidForGas", value: dt.Rows[0]["Amount"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Oper_day"].ToString()).ToString("dd/MMM/yy")));
                    parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["Transaction_group_number"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "PaymentUniqueNumber", value: dt.Rows[0]["ID"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "F_J", value: dt.Rows[0]["abonent_type"].ToString()));
                }
                else  //Իրավաբանական
                {
                    parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Paid_IN_Bank"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "OrderNum", value: dt.Rows[0]["order"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "Cod", value: dt.Rows[0]["TexCod"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "TexCod", value: dt.Rows[0]["TexCod"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "AmountPaidForGas", value: dt.Rows[0]["VGaz"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Date"].ToString()).ToString("dd/MMM/yy")));
                    parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["id"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "PaymentUniqueNumber", value: dt.Rows[0]["uniquenumber"].ToString()));
                    parameters.Add(new KeyValuePair<string, string>(key: "F_J", value: dt.Rows[0]["Fiz_OR_Jur"].ToString()));
                }


            }
            else if (communalType == CommunalTypes.ENA)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "UniqueNumber", value: dt.Rows[0]["uniquenumber"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "ID_ENA", value: dt.Rows[0]["ID_ENA"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["id"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Date"].ToString()).ToString("dd/MMM/yy")));
                parameters.Add(new KeyValuePair<string, string>(key: "code", value: dt.Rows[0]["B_Cod"] != DBNull.Value ? dt.Rows[0]["B_Cod"].ToString() : dt.Rows[0]["ID_ENA"].ToString()));
            }
            else if (communalType == CommunalTypes.VivaCell)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Fillial"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "id", value: dt.Rows[0]["id"].ToString()));
            }
            else if (communalType == CommunalTypes.Orange)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Branch"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "OrderNum", value: dt.Rows[0]["order_num"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PhoneNumber", value: dt.Rows[0]["Abonent_number"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt", value: dt.Rows[0]["Salin"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountPaid", value: dt.Rows[0]["Amount"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PayerName", value: dt.Rows[0]["Payer_Name"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: dt.Rows[0]["Abonent_Name"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDescription", value: dt.Rows[0]["Pas_Name"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Pay_Date"].ToString()).ToString("dd/MMM/yy")));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentTime", value: dt.Rows[0]["Pay_Time"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["Transaction_group_number"] != DBNull.Value ? dt.Rows[0]["Transaction_group_number"].ToString() : "0"));
            }
            else if (communalType == CommunalTypes.ArmWater)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Reestr"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "UniqueNumber", value: dt.Rows[0]["UniqueNumber"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Date"].ToString()).ToString("dd/MMM/yy")));
                parameters.Add(new KeyValuePair<string, string>(key: "ArmWaterBranch", value: dt.Rows[0]["Masn"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AbonentName", value: dt.Rows[0]["SubName"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountPaid", value: dt.Rows[0]["Paid"].ToString()));
            }

            else if (communalType == CommunalTypes.YerWater)
            {
                //DataTable dt2 = CommunalDB.GetVivaCellData(this.AbonentNumber, 1);
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Reestr"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "UniqueNumber", value: dt.Rows[0]["UniqueNumber"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountPaid", value: dt.Rows[0]["Paid"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Date"].ToString()).ToString("dd/MMM/yy")));
                parameters.Add(new KeyValuePair<string, string>(key: "ErJurBranch", value: dt.Rows[0]["Masn"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "SubNumber", value: dt.Rows[0]["SubNumber"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "F_J", value: dt.Rows[0]["Fiz_OR_Jur"].ToString()));
            }
            else if (communalType == CommunalTypes.ArmenTel)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Fillial"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["Transaction_group_number"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "id", value: dt.Rows[0]["Transaction_group_number"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["oper_day"].ToString()).ToString("dd/MMM/yy")));
            }
            else if (communalType == CommunalTypes.UCom)
            {
                parameters.Add(new KeyValuePair<string, string>(key: "FilialCode", value: dt.Rows[0]["Fillial"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "OrderNum", value: dt.Rows[0]["Order_Num"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "TransactionNumber", value: dt.Rows[0]["Transaction_group_number"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountCurrency", value: "AMD"));
                parameters.Add(new KeyValuePair<string, string>(key: "AbonentNumber", value: dt.Rows[0]["Abonent_number"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Internet", value: dt.Rows[0]["Debt_Internet"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_TV", value: dt.Rows[0]["Debt_TV"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Phone", value: dt.Rows[0]["Debt_Phone"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountDebt_Other", value: dt.Rows[0]["Debt_Other"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "AmountPaid", value: dt.Rows[0]["Amount"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDate", value: Convert.ToDateTime(dt.Rows[0]["Pay_date"].ToString()).ToString("dd/MMM/yy")));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentTime", value: dt.Rows[0]["Pay_Time"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "PaymentDescription", value: dt.Rows[0]["Payment_Descr"].ToString()));
                parameters.Add(new KeyValuePair<string, string>(key: "eFOCode", value: "eFO 75-00-87/1#3"));
            }

            return parameters;
        }

    }
}
