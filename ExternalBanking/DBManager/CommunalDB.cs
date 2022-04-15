using ExternalBanking.UtilityPaymentsManagment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class CommunalDB
    {
        internal static DataTable SearchCommunalData(SearchCommunal cmnl)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_getCommunalSearchedNameForCashTerminalBase", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.CommunalType == CommunalTypes.UCom ? 9 : (short)cmnl.CommunalType;

                    param = cmd.Parameters.Add("@code", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;

                    param = cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.PhoneNumber;

                    param = cmd.Parameters.Add("@fiz_jur", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentType;

                    param = cmd.Parameters.Add("@is_UtilityPayment", SqlDbType.Bit);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Branch == null ? "" : cmnl.Branch;

                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@var", SqlDbType.NVarChar, 30);
                    param.Direction = ParameterDirection.Output;

                    conn.Open();

                    DataSet ds = new DataSet();
                    SqlDataAdapter dtAdapter = new SqlDataAdapter();
                    dtAdapter.SelectCommand = cmd;
                    dtAdapter.Fill(ds, "utility_data");

                    if (ds.Tables["utility_data"] != null)
                    {
                        return ds.Tables["utility_data"];
                    }
                    else
                        return new DataTable();

                }
            }
        }


        internal static List<CommunalDetails> GetCommunalDetails(CommunalTypes comunalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {

            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_Get_Comunal_Data_Details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@type_of_operation", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = (short)comunalType;

                    param = cmd.Parameters.Add("@cod", SqlDbType.NVarChar);
                    param.Direction = ParameterDirection.Input;
                    param.Value = abonentNumber;

                    param = cmd.Parameters.Add("@check_type", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar);
                    param.Direction = ParameterDirection.Input;
                    param.Value = branchCode;

                    param = cmd.Parameters.Add("@abonent_type", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = (ushort)abonentType;

                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                CommunalDetails comunalDetail = new CommunalDetails();
                                comunalDetail.Id = int.Parse(dr["id"].ToString());
                                comunalDetail.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                                if(comunalType == CommunalTypes.YerWater)
                                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(Regex.Replace(dr["field_value"].ToString(), @"\s+", " "));
                                else
                                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(dr["field_value"].ToString());

                                comunalDetails.Add(comunalDetail);
                            }

                            string value = "";
                            switch (comunalType)
                            {
                                case CommunalTypes.None:
                                    break;
                                case CommunalTypes.ENA:
                                    value = "«Հայաստանի Էլեկտրական ցանցեր»";
                                    break;
                                case CommunalTypes.Gas:
                                    value = "«Գազպրոմ Արմենիա»";
                                    break;
                                case CommunalTypes.ArmWater:
                                    break;
                                case CommunalTypes.YerWater:
                                    value = "«Վեոլիա Ջուր»";
                                    break;
                                case CommunalTypes.ArmenTel:
                                    value = "«Բիլայն Արմենիա»";
                                    break;
                                case CommunalTypes.VivaCell:
                                    value = "«ՎիվաՍել-ՄՏՍ»";
                                    break;
                                case CommunalTypes.UCom:
                                    value = "«Յուքոմ» ֆիկսված";
                                    break;
                                case CommunalTypes.Orange:
                                    value = "«Յուքոմ» բջջային";
                                    break;
                                case CommunalTypes.Trash:
                                    value = "Աղբահանության վճար";
                                    break;
                                case CommunalTypes.COWater:
                                    value = "ՋՕԸ ծառայության վճար";
                                    break;
                                default:
                                    break;
                            }
                            comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = value });
                        }
                    }
                }
            }

            return comunalDetails;
        }

        internal static List<CommunalDetails> GetCommunalDetailsForVivaCell(string abonentNumber)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            VivaCellSubscriberSearch vivaCellSubscriber = new VivaCellSubscriberSearch();
            vivaCellSubscriber.GetVivaCellSubscriberDetails(abonentNumber);

            if (vivaCellSubscriber.PhoneNumber != null)
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand("pr_get_vivacell_subscriber_details_with_form", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter param = new SqlParameter();

                        param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                        param.Direction = ParameterDirection.Input;
                        param.Value = vivaCellSubscriber.PhoneNumber;

                        param = cmd.Parameters.Add("@subscriberType", SqlDbType.Int);
                        param.Direction = ParameterDirection.Input;
                        param.Value = (int)vivaCellSubscriber.SubscriberType;

                        param = cmd.Parameters.Add("@balanceSub", SqlDbType.Int);
                        param.Direction = ParameterDirection.Input;
                        param.Value = vivaCellSubscriber.BalanceSub;

                        param = cmd.Parameters.Add("@subscriberName", SqlDbType.NVarChar);
                        param.Direction = ParameterDirection.Input;
                        param.Value = vivaCellSubscriber.SubscriberName;

                        conn.Open();

                        DataTable dt = new DataTable();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            CommunalDetails comunalDetail = new CommunalDetails
                            {
                                Id = int.Parse(row["id"].ToString()),
                                Description = Utility.ConvertAnsiToUnicode(row["description"].ToString()),
                                Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString())
                            };

                            comunalDetails.Add(comunalDetail);
                        }

                    }
                }
            }

            return comunalDetails;
        }

        internal static List<CommunalDetails> GetCommunalDetailsForOrange(string abonentNumber)
        {
            #region Յուքոմի հին հարցում
            //List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            //using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            //{
            //    DataTable dt = GetOrangeData(abonentNumber, 2);
            //    if (dt.Rows.Count > 0)
            //    {
            //        foreach (DataRow row in dt.Rows)
            //        {
            //            CommunalDetails comunalDetail = new CommunalDetails
            //            {
            //                Id = int.Parse(row["id"].ToString()),
            //                Description = Utility.ConvertAnsiToUnicode(row["description"].ToString()),
            //                Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString())
            //            };

            //            comunalDetails.Add(comunalDetail);
            //        }
            //    }
            //}

            //return comunalDetails;
            #endregion



            List<CommunalDetails> list = new List<CommunalDetails>();
            UcomMobileAbonentSearch abonentSearch = new UcomMobileAbonentSearch();

            abonentSearch = abonentSearch.GetUcomAbonentSearch(abonentNumber);


            list.Add(new CommunalDetails { Description = "Հեռախոսահամար", Id = 71, Value = abonentSearch.PhoneNumber });
            list.Add(new CommunalDetails { Description = "Կանխավճար", Id = 72, Value = abonentSearch.Balance });
            list.Add(new CommunalDetails { Description = "Բաժանորդ", Id = 5, Value = abonentSearch.AbonentName });

            return list;
        }

        internal static List<Communal> SearchCommunalENA(SearchCommunal cmnl)
        {
            List<Communal> enaList = new List<Communal>();
            DataTable dt = GetENAData(cmnl, 1, 0);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal ena = new Communal();
                    ena.ComunalType = CommunalTypes.ENA;
                    ena.AbonentNumber = row["cod"].ToString();
                    ena.BranchCode = row["branch_cod"].ToString();
                    ena.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());

                    if (dt.Columns.Contains("phone"))
                    {
                        ena.Description = ena.Description + Environment.NewLine + " Հեռ՝ " + (row["phone"] != DBNull.Value ? row["phone"].ToString() : "-");
                    }

                    ena.Debt = Double.Parse(row["debt"].ToString());
                    enaList.Add(ena);
                }
            }

            return enaList;

        }
        internal static List<Communal> SearchCommunalGas(SearchCommunal cmnl, SourceType source)
        {
            List<Communal> list = new List<Communal>();
            if (cmnl.AbonentType == 1)
            {
                List<GasPromAbonentSearch> abonentSearch = GasPromAbonentSearch.GasPromSearchOutput(cmnl);
                short CommunalDebtSign = GetCommunalDebtSignDB(4, cmnl.AbonentType);

                foreach (var item in abonentSearch)
                {
                    Communal gas = new Communal();
                    gas.ComunalType = CommunalTypes.Gas;
                    gas.AbonentNumber = item.AbonentNumber;
                    gas.BranchCode = item.SectionCode;
                    if (source == SourceType.AcbaOnline)
                        gas.Description = item.LastName.Replace(" ", "") + " " + item.Name + Environment.NewLine + (item.Street.StartsWith(".") ? item.Street.Substring(1) : item.Street).Trim() + " " + item.House.Replace(" ", "") + " " + item.Home.Replace(" ", "");
                    else
                        gas.Description = item.LastName + " " + item.Name + Environment.NewLine + item.Street + " " + item.House + " " + item.Home + Environment.NewLine + " Հեռ՝ " + (item.PhoneNumber != string.Empty ? item.PhoneNumber : "-");

                    gas.Debt = item.CurrentGasDebt * CommunalDebtSign;
                    gas.FeeDebt = item.CurrentServiceFeeDebt;

                    list.Add(gas);

                }
                return list;
            }
            else
            {
                DataTable dt = GetGasPromData(cmnl, 1, 0);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Communal gas = new Communal();
                        gas.ComunalType = CommunalTypes.Gas;
                        gas.AbonentNumber = row["cod"].ToString();
                        gas.BranchCode = row["branch_cod"].ToString();
                        gas.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString() + Environment.NewLine + " Հեռ՝ " + (row["tel"] != DBNull.Value ? row["tel"].ToString() : "-"));
                        gas.Debt = Double.Parse(row["debt"].ToString());
                        list.Add(gas);
                    }
                }
                return list;
            }
        }
        //esi
        internal static List<Communal> SearchCommunalGasForMobile(SearchCommunal cmnl)
        {
            List<Communal> list = new List<Communal>();

            if (cmnl.AbonentType == 1)
            {

                List<GasPromAbonentSearch> abonentSearch = GasPromAbonentSearch.GasPromSearchOutput(cmnl);
                short CommunalDebtSign = GetCommunalDebtSignDB(4, cmnl.AbonentType);

                foreach (var item in abonentSearch)
                {
                    Communal gas = new Communal();
                    gas.ComunalType = CommunalTypes.Gas;
                    gas.AbonentNumber = item.AbonentNumber;
                    gas.BranchCode = item.SectionCode;
                    gas.Description = item.LastName.Replace(" ", "") + " " + item.Name + Environment.NewLine + (item.Street.StartsWith(".") ? item.Street.Substring(1) : item.Street).Trim() + " " + item.House.Replace(" ", "") + " " + item.Home.Replace(" ", "");
                    gas.Debt = item.CurrentGasDebt * CommunalDebtSign;
                    gas.FeeDebt = item.CurrentServiceFeeDebt;

                    list.Add(gas);
                }
                return list;
            }
            else
            {
                DataTable dt = SearchCommunalData(cmnl);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Communal gas = new Communal();
                        gas.ComunalType = CommunalTypes.Gas;
                        gas.AbonentNumber = row["cod"].ToString();
                        gas.BranchCode = row["branch_cod"].ToString();
                        gas.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());
                        gas.Debt = Double.Parse(row["debt"].ToString());
                        list.Add(gas);
                    }
                }
            }
            return list;
        }



        internal static List<Communal> SearchCommunalArmenTel(SearchCommunal searchCommunal)
        {
            List<Communal> list = new List<Communal>();
            DataTable dt = GetArmenTelData(searchCommunal.PhoneNumber, 1);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal armenTel = new Communal();
                    armenTel.ComunalType = CommunalTypes.ArmenTel;
                    armenTel.AbonentNumber = row["cod"].ToString();
                    armenTel.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(armenTel);
                }
            }
            return list;
        }

        internal static List<Communal> SearchCommunalArmenTelForMobile(SearchCommunal cmnl)
        {
            List<Communal> list = new List<Communal>();
            DataTable dt = SearchCommunalData(cmnl);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal armenTel = new Communal();
                    armenTel.ComunalType = CommunalTypes.ArmenTel;
                    armenTel.AbonentNumber = row["cod"].ToString();
                    armenTel.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(armenTel);
                }
            }
            return list;
        }

        internal static List<Communal> SearchCommunalArmWater(SearchCommunal cmnl)
        {

            List<Communal> list = new List<Communal>();
            DataTable dt = GetArmWaterData(cmnl, 1, 0);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal armWater = new Communal();
                    armWater.ComunalType = CommunalTypes.ArmWater;
                    armWater.AbonentNumber = row["cod"].ToString();
                    armWater.BranchCode = row["branch_cod"].ToString();
                    armWater.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());
                    armWater.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(armWater);
                }
            }
            return list;
        }


        internal static List<Communal> SearchCommunalOrangeForMobile(SearchCommunal cmnl)
        {
            List<Communal> list = new List<Communal>();

            DataTable dt = SearchCommunalData(cmnl);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal orange = new Communal();
                    orange.ComunalType = CommunalTypes.Orange;
                    orange.AbonentNumber = row["cod"].ToString();
                    orange.PrepaidSign = (PrepaidSign)Convert.ToInt16(row["prepaid"].ToString());
                    orange.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(orange);
                }
            }
            return list;
        }

        internal static List<Communal> SearchCommunalOrange(SearchCommunal cmnl, CommunalTypes communalType)
        {
            List<Communal> list = new List<Communal>();

            //DataTable dt = GetOrangeData(cmnl.AbonentNumber, 1);
            //if (dt.Rows.Count > 0)
            //{
            //    foreach (DataRow row in dt.Rows)
            //    {
            //        Communal orange = new Communal();
            //        orange.ComunalType = CommunalTypes.Orange;
            //        orange.AbonentNumber = row["phoneNumber"].ToString();
            //        orange.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString());
            //        orange.PrepaidSign = (PrepaidSign)Convert.ToInt16(row["prepaid"].ToString());
            //        orange.Debt = Double.Parse(row["debt"].ToString());
            //        list.Add(orange);
            //    }
            //}
            List<UcomMobileAbonentSearch> ucomAbonentSearch = new List<UcomMobileAbonentSearch>(5);
            UcomMobileAbonentSearch abonentSearch = new UcomMobileAbonentSearch();
            abonentSearch = abonentSearch.GetUcomAbonentSearch(cmnl.AbonentNumber);
            if (!string.IsNullOrEmpty(abonentSearch.PhoneNumber))
            {
                ucomAbonentSearch.Add(abonentSearch.GetUcomAbonentSearch(cmnl.AbonentNumber));
                foreach (var item in ucomAbonentSearch)
                {
                    list.Add(new Communal
                    {
                        ComunalType = communalType,
                        AbonentNumber = item.PhoneNumber,
                        Description = Utility.ConvertAnsiToUnicode(item.AbonentName),
                        PrepaidSign = item.Prepaid == 0 ? PrepaidSign.Prepaid : PrepaidSign.NotPrepaid,
                        Debt = Convert.ToDouble(item.Balance) > 0 ? 0 : Convert.ToDouble(Math.Abs(Convert.ToDecimal(item.Balance)))
                    });
                }
            }
            return list;
        }

        internal static List<Communal> SearchCommunalUComForMobile(SearchCommunal cmnl)
        {

            List<Communal> list = new List<Communal>();

            DataTable dt = SearchCommunalData(cmnl);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal ucom = new Communal();
                    ucom.ComunalType = CommunalTypes.UCom;
                    ucom.AbonentNumber = row["cod"].ToString();
                    ucom.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString());
                    ucom.Debt = Double.Parse(row["total_debt"].ToString());
                    list.Add(ucom);
                }
            }
            return list;
        }

        internal static List<Communal> SearchCommunalUCom(SearchCommunal cmnl)
        {
            bool ucomNewVersion = bool.Parse(WebConfigurationManager.AppSettings["UcomFixNewVersion"].ToString());
            List<Communal> list = new List<Communal>();
            if (ucomNewVersion)
            {
                List<UcomFixAbonentSearch> ucomFixAbonentSearch = new List<UcomFixAbonentSearch>();
                UcomFixAbonentSearch abonentSearch = new UcomFixAbonentSearch();
                ucomFixAbonentSearch.Add(abonentSearch.GetUcomFixAbonentSearch(cmnl.AbonentNumber));

                foreach (var item in ucomFixAbonentSearch)
                {
                    if (item.Balance != null)
                    {
                        Communal ucom = new Communal();
                        ucom.ComunalType = CommunalTypes.UCom;
                        ucom.AbonentNumber = item.AbonentNumber;
                        ucom.Description = Utility.ConvertAnsiToUnicode(item.Client);
                        ucom.Debt = item.Balance.Total;
                        list.Add(ucom);
                    }
                }

            }
            else
            {
                DataTable dt = GetUcomData(cmnl.AbonentNumber, 1);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Communal ucom = new Communal();
                        ucom.ComunalType = CommunalTypes.UCom;
                        ucom.AbonentNumber = row["cod"].ToString();
                        ucom.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString());
                        ucom.Debt = Double.Parse(row["total_debt"].ToString());
                        list.Add(ucom);
                    }
                }

            }
            return list;

        }
        //texapoxvac
        //internal static List<Communal> SearchCommunalVIvaCellForMobile(SearchCommunal cmnl)
        //{
        //    List<Communal> list = new List<Communal>();

        //    DataTable dt = SearchCommunalData(cmnl);
        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            Communal vivaCell = new Communal();
        //            vivaCell.ComunalType = CommunalTypes.VivaCell;
        //            vivaCell.AbonentNumber = row["cod"].ToString();
        //            vivaCell.PrepaidSign = (PrepaidSign)Convert.ToInt16(row["prepaid"].ToString());
        //            vivaCell.Debt = Double.Parse(row["debt"].ToString());
        //            list.Add(vivaCell);
        //        }
        //    }
        //    return list;
        //}

        //texapoxvac
        //internal static List<Communal> SearchCommunalVIvaCell(SearchCommunal cmnl)
        //{
        //    List<Communal> list = new List<Communal>();

        //    DataTable dt = GetVivaCellData(cmnl.AbonentNumber, 1);
        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            Communal vivaCell = new Communal();
        //            vivaCell.ComunalType = CommunalTypes.VivaCell;
        //            vivaCell.AbonentNumber = row["phoneNumber"].ToString();
        //            vivaCell.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString());
        //            vivaCell.PrepaidSign = (PrepaidSign)Convert.ToInt16(row["prepaid"].ToString());
        //            vivaCell.Debt = Double.Parse(row["debt"].ToString());
        //            list.Add(vivaCell);
        //        }
        //    }
        //    return list;
        //}

        internal static List<Communal> SearchCommunalYerWater(SearchCommunal cmnl, bool isSearch = true)
        {
            List<Communal> list = new List<Communal>();
            DataTable dt = GetYerevanJurData(cmnl, 1, 0, isSearch);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal yerWater = new Communal();
                    yerWater.ComunalType = CommunalTypes.YerWater;
                    yerWater.AbonentNumber = row["cod"].ToString();
                    yerWater.BranchCode = row["branch_cod"].ToString();
                    yerWater.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());
                    if (dt.Columns.Contains("telephon"))
                    {
                        yerWater.Description = yerWater.Description + Environment.NewLine + " Հեռ՝ " + row["telephon"].ToString();
                    }
                    yerWater.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(yerWater);
                }
            }
            return list;
        }

        internal static short GetCommunalDebtSignDB(short communalType, short physicalOrLegal)
        {
            short debtSign;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = @" SELECT dbo.Fnc_Get_Communal_Debt_Sign(@Transfer_Type, @Fiz_Jur) AS debtSign ";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@Transfer_Type", SqlDbType.SmallInt).Value = communalType;
                    cmd.Parameters.Add("@Fiz_Jur", SqlDbType.SmallInt).Value = physicalOrLegal;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    DataRow row = dt.Rows[0];

                    debtSign = short.Parse(row["debtSign"].ToString());
                }
            }

            return debtSign;
        }

        internal static List<Communal> SearchCommunalYerWaterForMobile(SearchCommunal cmnl)
        {
            List<Communal> list = new List<Communal>();

            DataTable dt = SearchCommunalData(cmnl);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal yerWater = new Communal
                    {
                        ComunalType = CommunalTypes.YerWater,
                        AbonentNumber = row["cod"].ToString(),
                        BranchCode = row["branch_cod"].ToString(),
                        Description = Utility.ConvertAnsiToUnicode(Regex.Replace(row["abonent_name"].ToString(), @"\s+", " ") + Environment.NewLine + row["abonent_address"].ToString()),
                        Debt = Double.Parse(row["debt"].ToString())
                    };
                    list.Add(yerWater);
                }
            }
            return list;
        }

        internal static DataTable GetArmWaterBranchDescription()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select masn,description from tbl_type_of_waters_bases", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }


        //internal static DataTable GetVivaCellData(string abonentNumber, int checkFor)
        //{
        //    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("pr_request_for_vivacel_payment_details", conn))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            SqlParameter param = new SqlParameter();

        //            param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
        //            param.Direction = ParameterDirection.Input;
        //            param.Value = abonentNumber;

        //            param = cmd.Parameters.Add("@resultType", SqlDbType.Int);
        //            param.Direction = ParameterDirection.Input;
        //            param.Value = checkFor;

        //            param = cmd.Parameters.Add("@result", SqlDbType.Int);
        //            param.Direction = ParameterDirection.Output;

        //            conn.Open();

        //            DataSet ds = new DataSet();
        //            using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
        //            {
        //                dtAdapter.SelectCommand = cmd;
        //                dtAdapter.Fill(ds, "utility_data");

        //                if (ds.Tables["utility_data"] != null)
        //                {
        //                    return ds.Tables["utility_data"];
        //                }
        //                else
        //                    return new DataTable();
        //            }


        //        }
        //    }
        //}

        internal static DataTable GetOrangeData(string abonentNumber, int checkFor)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_request_for_orange_payment_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = abonentNumber;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }




        internal static DataTable GetGasPromData(SearchCommunal cmnl, int checkFor, int checkType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_get_GasProm_Search_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.PhoneNumber == null ? "" : cmnl.PhoneNumber;

                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentType;

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Branch == null ? "" : cmnl.Branch;

                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Street == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Street);

                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.House == null ? "" : cmnl.House;

                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Home == null ? "" : cmnl.Home;

                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Name == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Name);

                    param = cmd.Parameters.Add("@lastName", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.LastName == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.LastName);


                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@var", SqlDbType.NVarChar, 30);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@checkType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }

        }

        internal static List<CommunalDetails> GetGasPromDetails(string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType, List<GasPromAbonentSearch> gazList)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            SearchCommunal searchGas = new SearchCommunal();
            searchGas.AbonentNumber = abonentNumber;
            searchGas.AbonentType = (short)abonentType;
            searchGas.Branch = branchCode;
            DataTable dt = new DataTable();

            if (abonentType == AbonentTypes.physical && gazList.Count > 0)
                dt = GetGasPromDataWithForm(gazList, checkType);
            else if (abonentType == AbonentTypes.legal)
                dt = GetGasPromData(searchGas, 2, checkType);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    CommunalDetails comunalDetail = new CommunalDetails();
                    comunalDetail.Id = int.Parse(row["id"].ToString());
                    comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString());
                    comunalDetails.Add(comunalDetail);
                }
                comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = "«Գազպրոմ Արմենիա»" });

            }


            return comunalDetails;
        }


        internal static List<CommunalDetails> GetENADetails(string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            SearchCommunal searchENA = new SearchCommunal();
            searchENA.AbonentNumber = abonentNumber;
            searchENA.AbonentType = (short)abonentType;
            searchENA.Branch = branchCode;


            DataTable dt = GetENAData(searchENA, 2, checkType);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    CommunalDetails comunalDetail = new CommunalDetails
                    {
                        Id = int.Parse(row["id"].ToString()),
                        Description = Utility.ConvertAnsiToUnicode(row["description"].ToString()),
                        Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString()),
                    };

                    comunalDetails.Add(comunalDetail);
                }
                comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = "«Հայաստանի Էլեկտրական ցանցեր»" });

            }

            return comunalDetails;
        }

        internal static DataTable GetENAData(SearchCommunal cmnl, int checkFor, int checkType)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_get_Ena_Search_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.PhoneNumber == null ? "" : cmnl.PhoneNumber;

                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentType;

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Branch == null ? "" : cmnl.Branch;

                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Street == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Street);

                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.House == null ? "" : cmnl.House;

                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Home == null ? "" : cmnl.Home;

                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Name == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Name);

                    param = cmd.Parameters.Add("@lastName", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.LastName == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.LastName);



                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@var", SqlDbType.NVarChar, 30);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@checkType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }


        internal static DataTable GetYerevanJurData(SearchCommunal cmnl, int checkFor, int checkType, bool isSearch = true)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_Request_For_YerevanJur_Payment_Details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();
                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;
                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.PhoneNumber == null ? "" : cmnl.PhoneNumber;
                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentType;
                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Branch == null || cmnl.Branch == "-1" ? "" : cmnl.Branch;
                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Street == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Street);
                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.House == null ? "" : cmnl.House;
                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Home == null ? "" : cmnl.Home;
                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Name == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.LastName == null ? cmnl.Name : cmnl.LastName + " " + cmnl.Name);
                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;
                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    param = cmd.Parameters.Add("@var", SqlDbType.NVarChar, 30);
                    param.Direction = ParameterDirection.Output;
                    param = cmd.Parameters.Add("@resultType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;
                    param = cmd.Parameters.Add("@checkType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;
                    param = cmd.Parameters.Add("@isSearch", SqlDbType.Bit);
                    param.Direction = ParameterDirection.Input;
                    param.Value = isSearch;
                    conn.Open();
                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");
                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }
                }
            }
        }

        internal static List<CommunalDetails> GetYerevanJurDetails(string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();

            SearchCommunal searchYerJur = new SearchCommunal();
            searchYerJur.AbonentNumber = abonentNumber;
            searchYerJur.AbonentType = (short)abonentType;
            searchYerJur.Branch = branchCode;


            DataTable dt = GetYerevanJurData(searchYerJur, 2, checkType);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    CommunalDetails comunalDetail = new CommunalDetails();
                    comunalDetail.Id = int.Parse(row["id"].ToString());
                    comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString());
                    comunalDetails.Add(comunalDetail);
                }
                comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = "«Վեոլիա Ջուր»" });

            }


            return comunalDetails;
        }


        internal static List<Communal> SearchCommunalENAForMobile(SearchCommunal cmnl)
        {
            List<Communal> list = new List<Communal>();
            DataTable dt = SearchCommunalData(cmnl);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Communal ena = new Communal();
                    ena.ComunalType = CommunalTypes.ENA;
                    ena.AbonentNumber = row["cod"].ToString();
                    ena.BranchCode = row["branch_cod"].ToString();
                    ena.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());
                    ena.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(ena);
                }
            }
            return list;
        }

        internal static DataTable GetArmenTelData(string abonentNumber, int checkFor)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_request_for_armentel_payment_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = abonentNumber == null ? "" : abonentNumber;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }

        internal static List<CommunalDetails> GetCommunalDetailsForArmenTel(string abonentNumber, CommunalTypes communalType)
        {
            BeelineAbonentSearch beelineAbonentSearch = new BeelineAbonentSearch();
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();
            comunalDetails.Add(new CommunalDetails
            {
                Description = communalType == CommunalTypes.ArmenTel ? "Հեռախոսահամար՝" : "Պայմ․ համար՝",
                Id = communalType == CommunalTypes.ArmenTel ? 68 : 83,
                Value = abonentNumber.ToString()
            });

            string debt = beelineAbonentSearch.GetBeelineAbonentBalance(abonentNumber).Balance.ToString();
            double finaldebt = Convert.ToDouble(debt) > 0 ? 0 : Math.Round(Convert.ToDouble(debt), 1);

            comunalDetails.Add(new CommunalDetails
            {
                Description = "Պարտք՝",
                Id = communalType == CommunalTypes.ArmenTel ? 70 : 84,
                Value = finaldebt.ToString()
            });
            return comunalDetails;


        }





        internal static List<CommunalDetails> GetArmWaterDetails(string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();
            CommunalDetails comunalDetail;

            SearchCommunal searchArmWater = new SearchCommunal();
            searchArmWater.AbonentNumber = abonentNumber;
            searchArmWater.AbonentType = (short)abonentType;
            searchArmWater.Branch = branchCode;


            DataTable dt = GetArmWaterData(searchArmWater, 2, checkType);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Id = int.Parse(row["id"].ToString());
                    comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString());

                    comunalDetails.Add(comunalDetail);
                }
            }


            return comunalDetails;
        }


        internal static DataTable GetArmWaterData(SearchCommunal cmnl, int checkFor, int checkType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_get_ArmWater_Search_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.PhoneNumber == null ? "" : cmnl.PhoneNumber;

                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentType;

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Branch == null ? "" : cmnl.Branch;

                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Street == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Street);

                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.House == null ? "" : cmnl.House;

                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Home == null ? "" : cmnl.Home;

                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.Name == null ? "" : Utility.ConvertUnicodeToAnsi(cmnl.LastName == null ? cmnl.Name : cmnl.LastName + " " + cmnl.Name);

                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@var", SqlDbType.NVarChar, 30);
                    param.Direction = ParameterDirection.Output;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@checkType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;


                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }



        internal static List<Communal> SearchCommunalArmWaterForMobile(SearchCommunal cmnl)
        {
            Communal armWater;
            List<Communal> list = new List<Communal>();

            DataTable dt = SearchCommunalData(cmnl);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    armWater = new Communal();
                    armWater.ComunalType = CommunalTypes.ArmWater;
                    armWater.AbonentNumber = row["cod"].ToString();
                    armWater.BranchCode = row["branch_cod"].ToString();
                    armWater.Description = Utility.ConvertAnsiToUnicode(row["abonent_name"].ToString() + Environment.NewLine + row["abonent_address"].ToString());
                    armWater.Debt = Double.Parse(row["debt"].ToString());
                    list.Add(armWater);
                }
            }
            return list;
        }


        internal static DataTable GetUcomData(string abonentNumber, int checkFor)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_request_for_ucom_payment_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = abonentNumber;

                    param = cmd.Parameters.Add("@resultType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkFor;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }


        internal static List<CommunalDetails> GetCommunalDetailsForUcom(string abonentNumber)
        {
            bool ucomNewVersion = bool.Parse(WebConfigurationManager.AppSettings["UcomFixNewVersion"].ToString());
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();
            if (ucomNewVersion)
            {
                UcomFixAbonentSearch ucomFixAbonentSearch = new UcomFixAbonentSearch();
                ucomFixAbonentSearch = ucomFixAbonentSearch.GetUcomFixAbonentSearch(abonentNumber);

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Պայմանագրի համար",
                    Id = 73,
                    Value = abonentNumber.ToString()
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Բաժանորդ",
                    Id = 74,
                    Value = ucomFixAbonentSearch.Client
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Ինտերնետ",
                    Id = 75,
                    Value = ucomFixAbonentSearch.Balance.Internet.ToString("F")
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Հեռախոս",
                    Id = 76,
                    Value = ucomFixAbonentSearch.Balance.Phone.ToString("F")
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Հեռուստատեսություն",
                    Id = 77,
                    Value = ucomFixAbonentSearch.Balance.TV.ToString("F")
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Այլ",
                    Id = 78,
                    Value = ucomFixAbonentSearch.Balance.Other.ToString("F")
                });

                comunalDetails.Add(new CommunalDetails
                {
                    Description = "Ընդամենը պարտք",
                    Id = 79,
                    Value = ucomFixAbonentSearch.Balance.Total.ToString("F")
                });
            }
            else
            {
                CommunalDetails comunalDetail;

                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    DataTable dt = GetUcomData(abonentNumber, 2);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            comunalDetail = new CommunalDetails();
                            comunalDetail.Id = int.Parse(row["id"].ToString());
                            comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                            comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["field_value"].ToString());

                            comunalDetails.Add(comunalDetail);
                        }
                    }
                }

            }

            return comunalDetails;

        }



        internal static DataTable GetTrashData(SearchCommunal cmnl)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_get_Trash_Search_details", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = cmnl.AbonentNumber;

                    param = cmd.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.PhoneNumber) ? "" : cmnl.PhoneNumber;


                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.Branch) ? "" : cmnl.Branch;

                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.Street) ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Street);

                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.House) ? "" : cmnl.House;

                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.Home) ? "" : cmnl.Home;

                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.Name) ? "" : Utility.ConvertUnicodeToAnsi(cmnl.Name);

                    param = cmd.Parameters.Add("@lastName", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = string.IsNullOrEmpty(cmnl.LastName) ? "" : Utility.ConvertUnicodeToAnsi(cmnl.LastName);

                    if (cmnl.FindByEqualAbonentNumberAndBranch)
                    {
                        param = cmd.Parameters.Add("@findByLikeAbonentNumberAndBranch", SqlDbType.Bit);
                        param.Direction = ParameterDirection.Input;
                        param.Value = 1;
                    }


                    conn.Open();

                    dt.Load(cmd.ExecuteReader());



                }
            }
            return dt;
        }


        internal static List<Communal> SearchCommunalDetailsForTrash(SearchCommunal cmnl)
        {
            List<Communal> comunalDetails = new List<Communal>();
            Communal comunalDetail;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                DataTable dt = GetTrashData(cmnl);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        comunalDetail = new Communal();
                        comunalDetail.BranchCode = row["QAXCOD"].ToString();
                        comunalDetail.AbonentNumber = row["COD"].ToString();
                        comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["ANUN"].ToString() + " " + row["AZGANUN"].ToString() +
                            " " + row["PHOXOC"].ToString() + " " + row["SHENQ"].ToString() + " " + row["BNAK"].ToString());

                        comunalDetail.Description = comunalDetail.Description + Environment.NewLine + " Հեռ՝ " + row["HERAXOS"].ToString();
                        comunalDetail.Debt = Convert.ToDouble(row["MNAMVRJ"]);
                        comunalDetail.ComunalType = CommunalTypes.Trash;
                        comunalDetails.Add(comunalDetail);
                    }
                }
            }

            return comunalDetails;
        }


        internal static DataTable GetOneTrashData(string abonentNumber, string branch)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT	QAXCOD,
												COD,
												PHOXOC,
												SHENQ,
												BNAK,
												AZGANUN,
												ANUN,
                                                HERAXOS,
												MNAMVRJ
												FROM Utility_Payments.dbo.Tbl_Trash_Main
                                                WHERE COD=@abonentNumber and QAXCOD=@branch";
                    cmd.Parameters.Add("@abonentNumber", SqlDbType.Int).Value = abonentNumber;
                    cmd.Parameters.Add("@branch", SqlDbType.Int).Value = branch;
                    dt.Load(cmd.ExecuteReader());
                }
            }
            return dt;
        }

        internal static List<CommunalDetails> CommunalDetailsForTrash(string abonentNumber, string branch)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();
            CommunalDetails comunalDetail;
            DataTable dt = new DataTable();
            dt = GetOneTrashData(abonentNumber, branch);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Id = 3;
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["COD"].ToString().Trim());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Id = 74;
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["ANUN"].ToString().Trim() + " " + row["AZGANUN"].ToString().Trim());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Id = 58;
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["PHOXOC"].ToString() + " " + row["SHENQ"].ToString() + " " + row["BNAK"].ToString());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Id = 79;
                    if (!string.IsNullOrEmpty(row["MNAMVRJ"].ToString()))
                    {
                        if (row["MNAMVRJ"].ToString()[0] == '-')
                            comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["MNAMVRJ"].ToString());
                        else
                            comunalDetail.Value = "-" + Utility.ConvertAnsiToUnicode(row["MNAMVRJ"].ToString());
                    }

                    comunalDetails.Add(comunalDetail);
                }
                comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = "Աղբահանության վճար" });

            }
            return comunalDetails;
        }


        /// <summary>
        /// Աղբահանության բազայի ա/թ
        /// </summary>
        /// <returns></returns>
        public static string GetTrashDate()
        {
            string date = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Amis FROM Tbl_Trash_Date";
                    date = cmd.ExecuteScalar().ToString();
                    date = date.Substring(date.Length - 2);
                    DataTable dt = Info.GetMonths();
                    DataRow[] dr = dt.Select("number=" + date.ToString());
                    if (dr.Length > 0)
                    {
                        date = Utility.ConvertAnsiToUnicode(dr[0]["name"].ToString());
                    }

                }
            }

            return date;
        }


        /// <summary>
        /// ENA բազայի ա/թ
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetENADate()
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Date_Readed, Date_Present, GodMes FROM Tbl_ENA_Date GROUP BY Date_Readed, Date_Present, GodMes";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Date_Readed"].ToString()).ToString("dd/MM/yy"), Convert.ToDateTime(dt.Rows[0]["Date_Present"].ToString()).ToString("dd/MM/yy"));
                    }

                }
            }

            return date;
        }


        /// <summary>
        /// Հայ ջրմուղ կոյուղի բազայի ա/թ
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetArmWaterDate()
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Date_Readed, Date_Present, MMYYYY FROM Tbl_ArmWater_Date GROUP BY Date_Readed, Date_Present, MMYYYY, Masn Having Masn = '0'  ORDER BY Date_Readed DESC";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Date_Readed"].ToString()).ToShortDateString(), Convert.ToDateTime(dt.Rows[0]["Date_Present"].ToString()).ToShortDateString());
                    }
                }
            }

            return date;
        }


        /// <summary>
        /// Երևան ջուր բազայի ա/թ
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetYerWaterDate()
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Date_Readed, Date_Present, MMYYYY FROM Tbl_Veolia_Dates GROUP BY Date_Readed, Date_Present, MMYYYY, Masn HAVING Masn = '00' ORDER BY Date_Readed DESC";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Date_Readed"].ToString()).ToString("dd/MM/yy"), Convert.ToDateTime(dt.Rows[0]["Date_Present"].ToString()).ToString("dd/MM/yy"));
                    }
                }
            }

            return date;
        }



        /// <summary>
        /// ՀայՌուսԳազԱրդ բազայի ա/թ իրավաբանական
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetGasDateForLegal()
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Date_Readed, Date_Present, [Date], F_J FROM Tbl_GasProm_Date WHERE F_J = 'J' ORDER BY F_J";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Date_Readed"].ToString()).ToString("dd/MM/yy"), Convert.ToDateTime(dt.Rows[0]["Date_Present"].ToString()).ToString("dd/MM/yy"));
                    }

                }
            }

            return date;
        }

        public static List<WaterCoDetail> GetWaterCoDetails()
        {

            List<WaterCoDetail> waterCoDetails = new List<WaterCoDetail>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @" SELECT ID, Code, Customer_Number, [Name],  FilialCode, [Percent] AS p FROM Tbl_WaterCo_List ";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            WaterCoDetail detail = new WaterCoDetail();
                            detail.Number = Convert.ToUInt16(dt.Rows[i]["ID"].ToString());
                            detail.Code = Convert.ToUInt16(dt.Rows[i]["Code"].ToString()).ToString("000");
                            detail.CustomerNumber = Convert.ToUInt64(dt.Rows[i]["Customer_Number"].ToString());
                            detail.Description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["Name"].ToString());
                            detail.FilialCode = Convert.ToUInt16(dt.Rows[i]["FilialCode"].ToString());
                            detail.Percent = Convert.ToDouble(dt.Rows[i]["p"].ToString());
                            waterCoDetails.Add(detail);

                        }

                    }

                }
            }

            return waterCoDetails;


        }

        public static List<DateTime> GetWaterCoDebtDates(ushort code)
        {
            List<DateTime> dates = new List<DateTime>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Tbl_WaterCo_Main.Debt_ON FROM 
                                            Tbl_WaterCo_Main WHERE Left([Kod], 3) = @code 
                                            GROUP BY Tbl_WaterCo_Main.Debt_ON ORDER BY Tbl_WaterCo_Main.Debt_ON DESC";
                    DataTable dt = new DataTable();
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 3).Value = code.ToString("000");
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DateTime date = Convert.ToDateTime(dt.Rows[i]["Debt_ON"].ToString());
                            dates.Add(date);

                        }

                    }

                }
            }
            return dates;


        }


        public static Dictionary<string, string> GetWaterCoBranches(ushort filialCode)
        {
            Dictionary<string, string> branches = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Code_Branch, Name_Branch
                                            FROM Tbl_WaterCo_Branches WB  INNER JOIN 
                                            Tbl_WaterCo_List WL ON WB.Code_Branch = WL.Code
                                            WHERE WL.FilialCode = @filialCode 
                                            GROUP BY WB.ID, Code_Branch, Name_Branch, Name_Hamaynq ORDER BY Code_Branch";
                    DataTable dt = new DataTable();
                    cmd.Parameters.Add("@filialCode", SqlDbType.NVarChar).Value = filialCode;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            branches.Add(dt.Rows[i]["Code_Branch"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Name_Branch"].ToString()));

                        }

                    }

                }
            }
            return branches;


        }


        public static Dictionary<string, string> GetWaterCoCitys(ushort code)
        {
            Dictionary<string, string> citys = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT City FROM Tbl_WaterCo_Main WHERE Left([Kod], 3) = @code GROUP BY City ORDER BY (Ascii(Left([City],1)))";
                    DataTable dt = new DataTable();
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 3).Value = code.ToString("000");
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            citys.Add(Utility.ConvertAnsiToUnicode(dt.Rows[i]["City"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["City"].ToString()));

                        }

                    }

                }
            }
            return citys;


        }



        /// <summary>
        /// ՋՕԸ բազայի ա/թ
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetCOWaterDate(DateTime debtDate)
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Get_Date from Tbl_WaterCo_Main where Debt_ON = @debtDate GROUP by Get_Date ORDER BY Get_Date DESC";
                    cmd.Parameters.Add("@debtDate", SqlDbType.SmallDateTime).Value = debtDate;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Get_Date"].ToString()).ToString("dd/MM/yy"), Convert.ToDateTime(dt.Rows[0]["Get_Date"].ToString()).ToString("dd/MM/yy"));
                    }

                }
            }

            return date;
        }


        internal static List<Communal> SearchCommunalDetailsForCOWater(SearchCommunal cmnl)
        {
            List<Communal> comunalDetails = new List<Communal>();
            Communal comunalDetail;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                DataTable dt = GetCOWaterData(cmnl);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        comunalDetail = new Communal();
                        comunalDetail.AbonentFilialCode = row["FilialCode"].ToString();
                        comunalDetail.AbonentNumber = row["KOD"].ToString();
                        comunalDetail.Description = Utility.ConvertAnsiToUnicode(row["AAH"].ToString() + " (" + row["City"].ToString() + ")");
                        //comunalDetail.Debt = Convert.ToDouble(row["MNAMVRJ"]);
                        comunalDetail.ComunalType = CommunalTypes.COWater;
                        comunalDetail.BranchCode = cmnl.Branch;
                        comunalDetails.Add(comunalDetail);
                    }
                }
            }

            return comunalDetails;
        }

        internal static DataTable GetCOWaterData(SearchCommunal cmnl)
        {
            DataTable dt = new DataTable();



            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    string sqlText = @"SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY [Get_Date] DESC) AS 'ROW_NUM',
                                        Kod,
                                        AAH,
                                        TotalSkizb AS TotalStart,
                                        WaterS AS WaterStart,
                                        AndamS AS AndamStart,
                                        TotalVerg AS TotalEnd,
                                        WaterV AS WaterEnd,
                                        AndamV AS AndamEnd,
                                        City,
                                        Get_Date,
                                        Debt_ON,
                                        ReadedFileName,
                                        FilialCode 
                                        FROM Tbl_WaterCo_Main M
                                        WHERE 1=1";

                    string sqlFilter = "";

                    if (!string.IsNullOrEmpty(cmnl.FilialCode))
                    {
                        sqlFilter = sqlFilter + " AND  FILIALCODE = @filialCode ";
                        cmd.Parameters.Add("@filialCode", SqlDbType.NVarChar).Value = cmnl.FilialCode;

                    }
                    if (!string.IsNullOrEmpty(cmnl.CoWaterBranch))
                    {
                        sqlFilter = sqlFilter + " AND Left(Kod,3) = @code ";
                        cmd.Parameters.Add("@code", SqlDbType.NVarChar).Value = cmnl.CoWaterBranch;

                    }
                    if (cmnl.DebtListDate != default(DateTime))
                    {
                        sqlFilter = sqlFilter + " AND (Debt_ON = @debt_ON) ";
                        cmd.Parameters.Add("@debt_ON", SqlDbType.SmallDateTime).Value = cmnl.DebtListDate;

                    }

                    if (!string.IsNullOrEmpty(cmnl.AbonentNumber))
                    {
                        sqlFilter = sqlFilter + " AND (Kod LIKE '%" + cmnl.AbonentNumber + "%') ";
                        //cmd.Parameters.Add("@abonentNumber", SqlDbType.Int).Value =cmnl.AbonentNumber;

                    }

                    if (!string.IsNullOrEmpty(cmnl.Name))
                    {
                        sqlFilter = sqlFilter + " AND  (AAH LIKE '%" + Utility.ConvertUnicodeToAnsi(cmnl.Name) + "%') ";
                        //cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = Utility.ConvertUnicodeToAnsi(cmnl.Name);

                    }

                    if (!string.IsNullOrEmpty(cmnl.City))
                    {
                        sqlFilter = sqlFilter + " AND (City LIKE '%" + Utility.ConvertUnicodeToAnsi(cmnl.City) + "%') ";
                        //cmd.Parameters.Add("@city", SqlDbType.NVarChar).Value = Utility.ConvertUnicodeToAnsi(cmnl.City);

                    }
                    sqlText = sqlText + " " + sqlFilter;
                    cmd.CommandText = sqlText;
                    dt.Load(cmd.ExecuteReader());

                }
            }


            return dt;
        }



        internal static List<CommunalDetails> CommunalDetailsForCOWater(string abonentNumber, string branchCode, short checkType)
        {
            List<CommunalDetails> comunalDetails = new List<CommunalDetails>();
            CommunalDetails comunalDetail;
            DataTable dt = new DataTable();
            dt = GetOneCOWaterData(abonentNumber, branchCode);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Description = "Բաժանորդի քարտի համարը";
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["KOD"].ToString().Trim());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Description = "Բաժանորդ";
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["AAH"].ToString());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Description = "Համայնք";
                    comunalDetail.Value = Utility.ConvertAnsiToUnicode(row["City"].ToString());
                    comunalDetails.Add(comunalDetail);
                    comunalDetail = new CommunalDetails();
                    comunalDetail.Description = "Ընդհանուր պարտք առ " + Convert.ToDateTime(row["Debt_ON"].ToString()).ToString("dd/MM/yyyy");
                    comunalDetail.Value = Convert.ToDouble(row["TotalEnd"].ToString()).ToString("#,0.00");
                    comunalDetails.Add(comunalDetail);
                    if (checkType == 2)
                    {
                        comunalDetail = new CommunalDetails();
                        comunalDetail.Description = "Ջրի վարձ առ " + Convert.ToDateTime(row["Debt_ON"].ToString()).ToString("dd/MM/yyyy");
                        comunalDetail.Value = Convert.ToDouble(row["WaterEnd"].ToString()).ToString("#,0.00");
                        comunalDetails.Add(comunalDetail);

                        comunalDetail = new CommunalDetails();
                        comunalDetail.Description = "Անդամավճար " + Convert.ToDateTime(row["Debt_ON"].ToString()).ToString("dd/MM/yyyy");
                        comunalDetail.Value = Convert.ToDouble(row["AndamEnd"].ToString()).ToString("#,0.00");
                        comunalDetails.Add(comunalDetail);

                        comunalDetail = new CommunalDetails();
                        comunalDetail.Description = "Ընդհանուր պարտք առ " + DateTime.Now.ToString("01/01/yyyy");
                        comunalDetail.Value = Convert.ToDouble(row["TotalStart"].ToString()).ToString("#,0.00");
                        comunalDetails.Add(comunalDetail);

                        comunalDetail = new CommunalDetails();
                        comunalDetail.Description = "Ջրի վարձ առ " + DateTime.Now.ToString("01/01/yyyy");
                        comunalDetail.Value = Convert.ToDouble(row["WaterStart"].ToString()).ToString("#,0.00");
                        comunalDetails.Add(comunalDetail);

                        comunalDetail = new CommunalDetails();
                        comunalDetail.Description = "Անդամավճար " + DateTime.Now.ToString("01/01/yyyy");
                        comunalDetail.Value = Convert.ToDouble(row["AndamStart"].ToString()).ToString("#,0.00");
                        comunalDetails.Add(comunalDetail);

                    }
                }
                comunalDetails.Add(new CommunalDetails { Description = "Operator Name", Value = "ՋՕԸ ծառայության վճար" });
            }
            return comunalDetails;
        }


        internal static DataTable GetOneCOWaterData(string abonentNumber, string branchCode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT  TotalSkizb AS TotalStart,
                                                WaterS AS WaterStart,
                                                AndamS AS AndamStart,
                                                TotalVerg AS TotalEnd,
                                                WaterV AS WaterEnd,
                                                AndamV AS AndamEnd,
                                                Debt_ON,
                                                AAH,
                                                KOD,
                                                City
                                                FROM Tbl_WaterCo_Main 
                                                WHERE  Kod = @abonentNumber AND FilialCode=@branchCode";
                    cmd.Parameters.Add("@abonentNumber", SqlDbType.Int).Value = abonentNumber;
                    cmd.Parameters.Add("@branchCode", SqlDbType.Int).Value = branchCode;
                    dt.Load(cmd.ExecuteReader());
                }
            }
            return dt;
        }



        internal static string GetCOWaterBranchID(string code, string filialCode)
        {
            string branchId = "";
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT ID FROM Tbl_WaterCo_List where FilialCode=@filialCode and Code=@code";
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar).Value = code;
                    cmd.Parameters.Add("@filialCode", SqlDbType.NVarChar).Value = filialCode;
                    branchId = cmd.ExecuteScalar().ToString();
                }
            }
            return branchId;
        }

        public static Dictionary<string, string> GetReestrWaterCoBranches(ushort filialCode)
        {
            Dictionary<string, string> branches = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    string sqlText =
                        cmd.CommandText = @"SELECT name, code FROM Tbl_WaterCo_list WHERE FilialCode=@filialCode";
                    DataTable dt = new DataTable();
                    cmd.Parameters.Add("@filialCode", SqlDbType.NVarChar).Value = filialCode;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            branches.Add(dt.Rows[i]["code"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["name"].ToString()));

                        }

                    }

                }
            }
            return branches;


        }

        internal static List<double> GetComunalAmountPaidThisMonth(string code, short comunalType, short abonentType, DateTime DebtListDate, string texCode, int waterCoPaymentType)
        {
            List<double> comunalPayments = new List<double>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_get_comunal_amount_paid_this_month", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@comunalType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = comunalType;

                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = abonentType;

                    param = cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20);
                    param.Direction = ParameterDirection.Input;
                    param.Value = code;

                    param = cmd.Parameters.Add("@debt_ON", SqlDbType.SmallDateTime);
                    param.Direction = ParameterDirection.Input;
                    param.Value = DebtListDate;

                    param = cmd.Parameters.Add("@texCode", SqlDbType.NVarChar, 10);
                    param.Direction = ParameterDirection.Input;
                    param.Value = texCode;

                    param = cmd.Parameters.Add("@waterCoPaymentType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = waterCoPaymentType;

                    conn.Open();

                    using SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        comunalPayments.Add(double.Parse(dr["paid_in_this_month"].ToString()));

                        if (comunalType == 4 && abonentType == 1)
                            comunalPayments.Add(double.Parse(dr["paid_in_this_month_service"].ToString()));
                    }

                }
            }
            return comunalPayments;
        }

        internal static List<Communal> SearchBeeline(SearchCommunal searchCommunal, CommunalTypes communalType)
        {
            List<Communal> list = new List<Communal>();
            List<BeelineAbonentSearch> beelineAbonentSearch = new List<BeelineAbonentSearch>(5);
            BeelineAbonentSearch abonentSearch = new BeelineAbonentSearch();
            beelineAbonentSearch.Add(abonentSearch.GetBeelineAbonentBalance(searchCommunal.AbonentNumber));

            foreach (var item in beelineAbonentSearch)
            {
                Communal armentel = new Communal();
                armentel.ComunalType = communalType;
                armentel.AbonentNumber = item.BeelineAbonentNumber;
                if (item.Balance != null)
                {
                    armentel.Debt = Convert.ToDouble(item.Balance) > 0 ? 0 : Math.Round(Convert.ToDouble(item.Balance), 1);

                    list.Add(armentel);
                }
            }

            return list;
        }

        internal static List<GasPromAbonentSearch> SearchFullCommunalGas(string abonentNumber, string branchCode)
        {
            SearchCommunal cmnl = new SearchCommunal();
            cmnl.AbonentNumber = abonentNumber;
            cmnl.Branch = branchCode;

            return GasPromAbonentSearch.GasPromSearchOutput(cmnl);
        }

        private static DataTable GetGasPromDataWithForm(List<GasPromAbonentSearch> gazList, short checkType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_Get_GasProm_Search_Details_With_Form", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = new SqlParameter();

                    param = cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].AbonentNumber;

                    param = cmd.Parameters.Add("@checkType", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = checkType;

                    param = cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@street", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].Street == null ? "" : Utility.ConvertUnicodeToAnsi((gazList[0].Street.StartsWith(".") == true ? gazList[0].Street.Substring(1) : gazList[0].Street).Trim());

                    param = cmd.Parameters.Add("@house", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].House == null ? "" : gazList[0].House.Trim();

                    param = cmd.Parameters.Add("@home", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].Home == null ? "" : gazList[0].Home.Trim();

                    param = cmd.Parameters.Add("@name", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].Name == null ? "" : Utility.ConvertUnicodeToAnsi(gazList[0].Name.Trim());

                    param = cmd.Parameters.Add("@lastName", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].LastName == null ? "" : Utility.ConvertUnicodeToAnsi(gazList[0].LastName.Replace(" ", "").Trim());

                    param = cmd.Parameters.Add("@round", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = 1;

                    param = cmd.Parameters.Add("@dateg", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].DebtDate;

                    param = cmd.Parameters.Add("@endg", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].CurrentGasDebt;

                    param = cmd.Parameters.Add("@endn", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].CurrentServiceFeeDebt;

                    param = cmd.Parameters.Add("@tel", SqlDbType.NVarChar, 50);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].PhoneNumber;

                    param = cmd.Parameters.Add("@startg", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].GasDebtAtBeginningOfMonth;

                    param = cmd.Parameters.Add("@platg", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].GasPreviousPayment;

                    param = cmd.Parameters.Add("@Rasxg", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].GasExpenseByVolume;

                    param = cmd.Parameters.Add("@rasxdramg", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].GasExpenseByAmount;

                    param = cmd.Parameters.Add("@cuc_nax", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].MeterPreviousTestimony;

                    param = cmd.Parameters.Add("@cuc", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].MeterLastTestimony;

                    param = cmd.Parameters.Add("@startn", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].ServiceFeeDebtAtBeginningOfMonth;

                    param = cmd.Parameters.Add("@platn", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].ServiceFeePreviousPayment;

                    param = cmd.Parameters.Add("@rasxn", SqlDbType.Float);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].ServiceFeeByAmount;

                    param = cmd.Parameters.Add("@texcod", SqlDbType.NVarChar, 5);
                    param.Direction = ParameterDirection.Input;
                    param.Value = gazList[0].SectionCode;

                    Dictionary<string, string> type = Info.GetGasPromSectionCode();

                    param = cmd.Parameters.Add("@branch", SqlDbType.NVarChar, 200);
                    param.Direction = ParameterDirection.Input;
                    param.Value = type.FirstOrDefault(x => x.Key == gazList[0].SectionCode).Value;

                    param = cmd.Parameters.Add("@IsCashTerminal", SqlDbType.Bit);
                    param.Direction = ParameterDirection.Input;
                    param.Value = false;

                    param = cmd.Parameters.Add("@result", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;

                    conn.Open();

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter dtAdapter = new SqlDataAdapter())
                    {
                        dtAdapter.SelectCommand = cmd;
                        dtAdapter.Fill(ds, "utility_data");

                        if (ds.Tables["utility_data"] != null)
                        {
                            return ds.Tables["utility_data"];
                        }
                        else
                            return new DataTable();
                    }


                }
            }
        }


        internal static string SearchFullCommunalGasOnline(string abonentNumber, string branchCode, int num)
        {
            List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
            string Search = "";

            SearchCommunal cmnl = new SearchCommunal();
            cmnl.AbonentNumber = abonentNumber;
            cmnl.Branch = branchCode;
            gazList = GasPromAbonentSearch.GasPromSearchOutput(cmnl);

            short CommunalDebtSign = GetCommunalDebtSignDB(4, 1);

            if (gazList.Count > 0)
            {
                double Debt = CommunalDebtSign * gazList[0].CurrentGasDebt;

                string[] str = gazList[0].Name.Split(' ');
                string name = "";

                StringBuilder nameBuilder = new StringBuilder();
                for (int i = 0; i < str.Length; i++)
                    if (str[i].Length > 1)
                        nameBuilder.Append(str[i] + " ");

                name = nameBuilder.ToString();

                string sname = gazList[0].LastName.Replace(" ", "");
                string street = gazList[0].Street.Replace(" ", "");
                string house = gazList[0].House.Replace(" ", "");
                string home = gazList[0].Home.Replace(" ", "");
                string tel = gazList[0].PhoneNumber.Replace(" ", "");
                double gas_debt_at_beginning_of_month = gazList[0].GasDebtAtBeginningOfMonth;
                double debt_at_end_of_month = gazList[0].DebtAtEndOfMonth;
                double gas_previous_payment = gazList[0].GasPreviousPayment;
                double meter_previous_testimony = gazList[0].MeterPreviousTestimony;
                double gas_expense_by_volume = gazList[0].GasExpenseByVolume;
                double gas_expense_by_amount = gazList[0].GasExpenseByAmount;
                double meter_last_testimony = gazList[0].MeterLastTestimony;
                double paid_amount_in_current_month = gazList[0].PaidAmountInCurrentMonth;
                double service_fee_debt_at_beginning_of_month = gazList[0].ServiceFeeByAmount;
                double service_fee_previous_payment = gazList[0].ServiceFeePreviousPayment;
                double service_fee_by_amount = gazList[0].ServiceFeeByAmount;
                double service_fee_at_end_of_month = gazList[0].ServiceFeeAtEndOfMonth;
                double penalty = gazList[0].Penalty;
                double violation_by_volume = gazList[0].ViolationByVolume;
                double violation_by_amount = gazList[0].ViolationByAmount;
                double tariff = gazList[0].Tariff;
                double expense_by_volume_for_same_month_previous_year = gazList[0].ExpenseByVolumeForSameMonthPreviousYear;
                DateTime debt = gazList[0].DebtDate;
                double CurrentGasDebt = gazList[0].CurrentGasDebt;
                double CurrentServiceFeeDebt = gazList[0].CurrentServiceFeeDebt;



                if (num == 1)
                {
                    Search = name + "|" + sname + "|" + street + "|" + house + "|" + home + "|" + tel + "|" + gas_debt_at_beginning_of_month + "|" +
                       debt_at_end_of_month + "|" + gas_previous_payment + "|" + meter_previous_testimony + "|" + gas_expense_by_volume + "|" +
                         gas_expense_by_amount + "|" + meter_last_testimony + "|" + paid_amount_in_current_month + "|" + service_fee_debt_at_beginning_of_month + "|" +
                           service_fee_previous_payment + "|" + service_fee_by_amount + "|" + service_fee_at_end_of_month + "|" + penalty + "|" + violation_by_volume + "|" +
                             violation_by_amount + "|" + tariff + "|" + expense_by_volume_for_same_month_previous_year + "|" + debt + "|" + CurrentGasDebt + "|" + CurrentServiceFeeDebt;
                }
                else
                {
                    Search = name + " " + sname + "|" + gazList[0].SectionCode + "|" + Debt.ToString() + "|" + gazList[0].CurrentServiceFeeDebt.ToString();
                }

            }

            return Search;
        }

        public static Dictionary<string, string> SerchGasPromForReport(string abonentNumber, string branchCode)
        {
            List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
            Dictionary<string, string> SearchResult = new Dictionary<string, string>();

            SearchCommunal cmnl = new SearchCommunal();
            cmnl.AbonentNumber = abonentNumber;
            cmnl.Branch = branchCode;
            gazList = GasPromAbonentSearch.GasPromSearchOutput(cmnl);

            SearchResult.Add("Name", gazList[0].Name.Replace(" ", "") != "" ? gazList[0].Name.Replace(" ", "") : " ");
            SearchResult.Add("Sname", gazList[0].LastName.Replace(" ", "") != "" ? gazList[0].LastName.Replace(" ", "") : " ");
            SearchResult.Add("Street", gazList[0].Street.Replace(" ", "") != "" ? gazList[0].Street.Replace(" ", "") : " ");
            SearchResult.Add("House", gazList[0].House.Replace(" ", "") != "" ? gazList[0].House.Replace(" ", "") : " ");
            SearchResult.Add("Home", gazList[0].Home.Replace(" ", "") != "" ? gazList[0].Home.Replace(" ", "") : " ");
            SearchResult.Add("Tel", gazList[0].PhoneNumber.Replace(" ", "") != "" ? gazList[0].PhoneNumber.Replace(" ", "") : " ");

            SearchResult.Add("GasDebtAtBeginningOfMonth", gazList[0].GasDebtAtBeginningOfMonth.ToString());
            SearchResult.Add("DebtAtEndOfMonth", gazList[0].CurrentGasDebt.ToString());
            SearchResult.Add("GasPreviousPayment", gazList[0].GasPreviousPayment.ToString());
            SearchResult.Add("MeterPreviousTestimony", gazList[0].MeterPreviousTestimony.ToString());
            SearchResult.Add("GasExpenseByVolume", gazList[0].GasExpenseByVolume.ToString());
            SearchResult.Add("GasExpenseByAmount", gazList[0].GasExpenseByAmount.ToString());
            SearchResult.Add("MeterLastTestimony", gazList[0].MeterLastTestimony.ToString());
            SearchResult.Add("PaidAmountInCurrentMonth", gazList[0].PaidAmountInCurrentMonth.ToString());
            SearchResult.Add("ServiceFeeDebtAtBeginningOfMonth", gazList[0].ServiceFeeDebtAtBeginningOfMonth.ToString());
            SearchResult.Add("ServiceFeeByAmount", gazList[0].ServiceFeeByAmount.ToString());
            SearchResult.Add("ServiceFeeAtEndOfMonth", gazList[0].CurrentServiceFeeDebt.ToString());
            SearchResult.Add("Penalty", gazList[0].Penalty.ToString());
            SearchResult.Add("ViolationByVolume", gazList[0].ViolationByVolume.ToString());
            SearchResult.Add("ViolationByAmount", gazList[0].ViolationByAmount.ToString());
            SearchResult.Add("Tariff", gazList[0].Tariff.ToString());
            SearchResult.Add("ExpenseByVolumeForSameMonthPreviousYear", gazList[0].ExpenseByVolumeForSameMonthPreviousYear.ToString());
            SearchResult.Add("DebtDate", gazList[0].DebtDate.ToString("dd/MMM/yyyy"));

            return SearchResult;
        }


        /// <summary>
        /// ՀայՌուսԳազԱրդ բազայի ա/թ Ֆիզիկական
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetGasDateForPhysical()
        {
            KeyValuePair<string, string> date = new KeyValuePair<string, string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * FROM  ePayments.dbo.Tbl_GasProm_Date WHERE F_J = 'J' ORDER BY F_J";
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        date = new KeyValuePair<string, string>(Convert.ToDateTime(dt.Rows[0]["Date_Readed"].ToString()).ToString("dd/MM/yy"), Convert.ToDateTime(dt.Rows[0]["Date_Present"].ToString()).ToString("dd/MM/yy"));
                    }

                }
            }

            return date;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ կոմունալ վճարման մանրամասն տվյալները
        /// </summary>
        /// <param name="orderId">Գործարքի կոդ</param>
        /// <param name="communalType">Կոմունալի տեսակ</param>
        /// <returns></returns>
        internal static DataTable GetUtilityPaymentData(long orderId, CommunalTypes communalType, int abonentType = 0)
        {
            DataTable dt = new DataTable();
            string tableName = "";
            string condition = "";
            if (communalType == CommunalTypes.ArmenTel || communalType == CommunalTypes.BeelineInternet || communalType == CommunalTypes.VivaCell || communalType == CommunalTypes.Orange || communalType == CommunalTypes.UCom
                || (communalType == CommunalTypes.Gas && abonentType == 1))
            {
                condition = " and order_id  = @orderId";
            }
            else
            {
                condition = " and HB_doc_ID  = @orderId";
            }


            switch (communalType)
            {
                case CommunalTypes.ENA:
                    {
                        tableName = "Tbl_ENA_Payments";
                        break;
                    }
                case CommunalTypes.Gas:
                    {
                        if (abonentType == 1)  //Ֆիզիկական
                        {
                            tableName = " epayments.dbo.tbl_utility_payments_main M inner join tbl_GasProm_payments_details D  on m.id = d.main_id left join HBBase.dbo.Tbl_HB_GasProm_Order_Details hd on m.order_id = hd.docID ";
                        }
                        else   //Իրավաբանական
                        {
                            tableName = " Tbl_GasProm_Payments";
                        }

                        break;
                    }
                case CommunalTypes.ArmWater:
                    {
                        tableName = "Tbl_ArmWater_Payments";
                        break;
                    }
                case CommunalTypes.YerWater:
                    {
                        tableName = "Tbl_ErJur_Payments";
                        break;
                    }
                case CommunalTypes.ArmenTel:
                    {
                        tableName = " epayments.dbo.tbl_utility_payments_main M inner join tbl_beeline_payments_details D  on  m.id = d.main_id";
                        break;
                    }
                case CommunalTypes.VivaCell:
                    {
                        tableName = " epayments.dbo.tbl_utility_payments_main";
                        break;
                    }
                case CommunalTypes.Orange:
                    {
                        tableName = " epayments.dbo.tbl_utility_payments_main M inner join tbl_UcomMobile_payments_details D  on  m.id = d.main_id";
                        break;
                    }
                case CommunalTypes.UCom:
                    {
                        tableName = " epayments.dbo.tbl_utility_payments_main M inner join tbl_UcomFix_payments_details D  on  m.id = d.main_id";
                        break;
                    }
                default: return dt;
            }
            SqlParameter param = new SqlParameter();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string a = "Select * from " + tableName + " WHERE isnull(Deleted,0) <> 1 " + condition;
                using (SqlCommand cmd = new SqlCommand(a, conn))
                {
                    param = cmd.Parameters.Add("@orderId", SqlDbType.Int);
                    param.Direction = ParameterDirection.Input;
                    param.Value = orderId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }

        internal static string GetCommunalDescriptionByType(int type, Languages lang)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                string sql = @"SELECT 
                               CASE WHEN @lang = 1 
                               THEN description_arm 
                               ELSE description END as description
                               FROM tbl_type_of_utility_services
                               WHERE id = @type";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@lang", SqlDbType.Int).Value = (int)lang;
                    cmd.Parameters.Add("@type", SqlDbType.Int).Value = type;
                    conn.Open();
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0]["description"].ToString();
                    }
                    return string.Empty;
                }
            }
        }


    }
}



