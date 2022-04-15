using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    static class AccountFreezeDB
    {
        /// <summary>
        /// Վերադարձնում է հաշվի սառեցման գրությունները
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId)
        {
            List<AccountFreezeDetails> freezeHistory = new List<AccountFreezeDetails>();
            string condition = (reasonId != 0) ? " And reason_type = " + reasonId : "";
            switch (freezeStatus)
            {
                case 1:
                    condition = condition + " And closing_date is null Order by ID Desc ";
                    break;
                case 2:
                    condition = condition + " And closing_date is not null Order by ID Desc ";
                    break;
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Id,registration_date,set_number,UnUsed_amount,UnUsed_amount_date,
                                                    freeze_date,reason_type,reason_add_inf,closing_date,closing_set_number,
                                                    closing_reason_type,closing_reason_add_inf,days_calculate_date 
                                                    FROM Tbl_acc_freeze_history 
                                                    WHERE account_number=@accountNumber " + condition, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AccountFreezeDetails oneFreezeHistory = new AccountFreezeDetails();
                            oneFreezeHistory.Id = long.Parse(dr["Id"].ToString());
                            oneFreezeHistory.RegistrationDate = (DateTime)dr["registration_date"];
                            oneFreezeHistory.ReasonId = ushort.Parse(dr["reason_type"].ToString());
                            oneFreezeHistory.ReasonDescription = "";
                            if (!String.IsNullOrEmpty(dr["freeze_date"].ToString()))
                            {
                                oneFreezeHistory.FreezeDate = (DateTime)dr["freeze_date"];
                            }
                            if (!String.IsNullOrEmpty(dr["UnUsed_amount_date"].ToString()))
                            {
                                oneFreezeHistory.AmountFreezeDate = (DateTime)dr["UnUsed_amount_date"];
                            }
                            oneFreezeHistory.FreezeAmount = (dr["UnUsed_amount"] != DBNull.Value) ? Convert.ToDouble(dr["UnUsed_amount"]) : default(double);
                            oneFreezeHistory.FreezeUserId = (dr["set_number"] != DBNull.Value) ? long.Parse(dr["set_number"].ToString()) : default(long);
                            oneFreezeHistory.FreezeDescription = dr["reason_add_inf"].ToString();
                            if (!String.IsNullOrEmpty(dr["closing_date"].ToString()))
                            {
                                oneFreezeHistory.UnfreezeDate = (DateTime)dr["closing_date"];
                            }
                            oneFreezeHistory.UnfreezeUserId = (dr["closing_set_number"] != DBNull.Value) ? long.Parse(dr["closing_set_number"].ToString()) : default(long);
                            oneFreezeHistory.UnfreezeReasonId = (dr["closing_reason_type"] != DBNull.Value) ? ushort.Parse(dr["closing_reason_type"].ToString()) : default(ushort);
                            oneFreezeHistory.UnfreezeDescription = dr["closing_reason_add_inf"].ToString();
                            if (!String.IsNullOrEmpty(dr["days_calculate_date"].ToString()))
                            {
                                oneFreezeHistory.FreezeDateByDocumnet = (DateTime)dr["days_calculate_date"];
                            }

                            freezeHistory.Add(oneFreezeHistory);
                        }
                    }

                }

            }

            return freezeHistory;

        }

        /// <summary>
        /// Վերադարձնում է 1 հաշվի սառեցման գրություն
        /// </summary>
        /// <param name="freezeId"></param>
        /// <returns></returns>
        internal static AccountFreezeDetails GetAccountFreezeDetails(string freezeId)
        {
            AccountFreezeDetails freezeDetails = new AccountFreezeDetails();


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Id,registration_date,set_number,UnUsed_amount,UnUsed_amount_date,
                                                    freeze_date,reason_type,reason_add_inf,closing_date,closing_set_number,
                                                    closing_reason_type,closing_reason_add_inf,days_calculate_date 
                                                    FROM Tbl_acc_freeze_history 
                                                    WHERE Id=@id ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Float).Value = freezeId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            freezeDetails.Id = long.Parse(dr["Id"].ToString());
                            freezeDetails.RegistrationDate = (DateTime)dr["registration_date"];
                            freezeDetails.ReasonId = ushort.Parse(dr["reason_type"].ToString());
                            freezeDetails.ReasonDescription = "";
                            if (!String.IsNullOrEmpty(dr["freeze_date"].ToString()))
                            {
                                freezeDetails.FreezeDate = (DateTime)dr["freeze_date"];
                            }
                            if (!String.IsNullOrEmpty(dr["UnUsed_amount_date"].ToString()))
                            {
                                freezeDetails.AmountFreezeDate = (DateTime)dr["UnUsed_amount_date"];
                            }
                            freezeDetails.FreezeAmount = (dr["UnUsed_amount"] != DBNull.Value) ? Convert.ToDouble(dr["UnUsed_amount"]) : default(double);
                            freezeDetails.FreezeUserId = (dr["set_number"] != DBNull.Value) ? long.Parse(dr["set_number"].ToString()) : default(long);
                            freezeDetails.FreezeDescription = Utility.ConvertAnsiToUnicode(dr["reason_add_inf"].ToString());
                            if (!String.IsNullOrEmpty(dr["closing_date"].ToString()))
                            {
                                freezeDetails.UnfreezeDate = (DateTime)dr["closing_date"];
                            }
                            freezeDetails.UnfreezeUserId = (dr["closing_set_number"] != DBNull.Value) ? long.Parse(dr["closing_set_number"].ToString()) : default(long);
                            freezeDetails.UnfreezeReasonId = (dr["closing_reason_type"] != DBNull.Value) ? ushort.Parse(dr["closing_reason_type"].ToString()) : default(ushort);
                            freezeDetails.UnfreezeDescription = Utility.ConvertAnsiToUnicode(dr["closing_reason_add_inf"].ToString());
                            if (!String.IsNullOrEmpty(dr["days_calculate_date"].ToString()))
                            {
                                freezeDetails.FreezeDateByDocumnet = (DateTime)dr["days_calculate_date"];
                            }

                        }
                    }

                }
            }

            return freezeDetails;

        }



    }
}
