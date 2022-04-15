using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    class SearchInternationalTransferDB
    {
        internal static List<SearchInternationalTransfer> GetInternationalTransfersDB(SearchInternationalTransfer searchParams)
        {
            List<SearchInternationalTransfer> internationalTransfersList = new List<SearchInternationalTransfer>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    sql = " and deleted_set_number is null ";

                    if (searchParams.DateOfTransfer != DateTime.MinValue)
                    {
                        sql = sql + " and TBM.registration_date = '" + searchParams.DateOfTransfer.ToString("dd/MMM/yyyy") + "'  ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.SenderName))
                    {
                        sql = sql + " and  sender_name like '%" + Utility.ConvertUnicodeToAnsiRussian(searchParams.SenderName) + "%' ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.SenderAccNumber))
                    {
                        sql = sql + " and TBMR.sender_account='" + searchParams.SenderAccNumber + "'";
                    }

                    if (!String.IsNullOrEmpty(searchParams.ReceiverName))
                    {
                        sql = sql + "and receiver_name like '%" + Utility.ConvertUnicodeToAnsiRussian(searchParams.ReceiverName) + "%'  ";
                    }

                    cmd.CommandText = "sp_tamplate_for_transfers_external";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@criteria1", SqlDbType.NVarChar).Value = sql;
                    cmd.Parameters.Add("@startR", SqlDbType.Int).Value = searchParams.BeginRow;
                    cmd.Parameters.Add("@endR", SqlDbType.Int).Value = searchParams.EndRow;
                    cmd.Parameters.Add("@type", SqlDbType.Int).Value = 1;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            internationalTransfersList = new List<SearchInternationalTransfer>();
                        }
                        int count = 0;
                        if (dr.Read())
                        {
                            count = Convert.ToInt32(dr["cnt"]);
                        }

                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                SearchInternationalTransfer oneResult = new SearchInternationalTransfer();
                                oneResult.RowCount = count;
                                oneResult.ReceiverName = Utility.ConvertAnsiToUnicodeRussian(dr["StacoghTvial"].ToString());
                                oneResult.SenderName = Utility.ConvertAnsiToUnicodeRussian(dr["ClientTvial"].ToString());
                                oneResult.SenderAccNumber = dr["sender_account"].ToString();
                                oneResult.DateOfTransfer = Convert.ToDateTime(dr["dat"]);
                                oneResult.ReceiverBank = Utility.ConvertAnsiToUnicodeRussian(dr["receiver_bank"].ToString());
                                oneResult.DescriptionForPayment = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment"].ToString());
                                oneResult.ReceiverAccount = dr["receiver_account"].ToString();
                                oneResult.ReceiverBankSwift = dr["receiver_bank_swift"].ToString();
                                oneResult.IntermidateBankSwift = dr["intermidate_bank_swift"].ToString();
                                oneResult.IntermidateBank = Utility.ConvertAnsiToUnicodeRussian(dr["intermidate_bank"].ToString());
                                oneResult.ReceiverBankAddInf = Utility.ConvertAnsiToUnicodeRussian(dr["receiver_bank_add_inf"].ToString());
                                oneResult.ReceiverSwift = dr["receiver_swift"].ToString();
                                oneResult.SenderAddress = Utility.ConvertAnsiToUnicodeRussian(dr["sender_address"].ToString());
                                oneResult.SenderPhone = dr["sender_phone"].ToString();
                                oneResult.SenderOtherBankAccount = dr["sender_other_bank_account"].ToString();
                                oneResult.ReceiverAddInf = Utility.ConvertAnsiToUnicodeRussian(dr["receiver_add_inf"].ToString());
                                oneResult.BIK = dr["BIK"].ToString();
                                oneResult.KPP = dr["KPP"].ToString();
                                oneResult.INN = dr["INN"].ToString();
                                oneResult.ReceiverType = dr["receiver_type"].ToString();
                                oneResult.CorrAccount = dr["Corr_account"].ToString();
                                oneResult.DescriptionForPaymentRUR1 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_1"].ToString());
                                oneResult.DescriptionForPaymentRUR2 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_2"].ToString());
                                oneResult.DescriptionForPaymentRUR3 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_3"].ToString());
                                oneResult.DescriptionForPaymentRUR4 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_4"].ToString());
                                oneResult.DescriptionForPaymentRUR5 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_5"].ToString());
                                oneResult.DescriptionForPaymentRUR6 = Utility.ConvertAnsiToUnicodeRussian(dr["descr_for_payment_RUR_6"].ToString());
                                oneResult.Country = dr["country"].ToString();
                                internationalTransfersList.Add(oneResult);
                            }

                        }

                    }
                }
                return internationalTransfersList;
            }
        }
    }
}
