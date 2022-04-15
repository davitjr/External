using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class MembershipRewardsDB
    {
        public static MembershipRewards GetCardMembershipRewards(string cardNumber)
        {
            MembershipRewards mr = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "SELECT TOP 1 *,(Select status from Tbl_type_of_MR_status where id=a.status) as status_descr, (SELECT Change_date FROM Tbl_MR_history AS h WHERE a.MR_Id = h.MR_Id AND h.Status = 2) AS fee_payment_date  FROM tbl_MR_applications a where cardnumber=@cardNumber order by mr_id desc";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    mr = new MembershipRewards();
                    mr.Id = Convert.ToInt32(dr["MR_Id"].ToString());
                    mr.Status = Convert.ToUInt16(dr["Status"].ToString());
                    mr.StatusDescription = dr["status_descr"].ToString();
                    mr.RegistrationDate = DateTime.Parse(dr["RegDate"].ToString());
                    mr.EndDate = DateTime.Parse(dr["EndDate"].ToString());
                    mr.ClosingDate = dr["Closing_date"] != DBNull.Value ? DateTime.Parse(dr["Closing_date"].ToString()) : default(DateTime?);
                    mr.ServiceFee = Convert.ToDouble(dr["ServiceFee"].ToString());
                    mr.ServiceFeeReal = Convert.ToDouble(dr["ServiceFeeReal"].ToString());
                    mr.ServiceFeePayed = Convert.ToDouble(dr["ServiceFeePayed"].ToString());
                    mr.SetNumber = Convert.ToInt32(dr["Set_number"].ToString());
                    mr.ValidationDate = dr["Validation_date"] != DBNull.Value ? DateTime.Parse(dr["Validation_date"].ToString()) : default(DateTime?);
                    mr.LastDayOfBonusCalculation = DateTime.Parse(dr["last_day_of_bonus_calculation"].ToString());
                    mr.FeePaymentDate = dr["fee_payment_date"] == DBNull.Value ? (DateTime?)null : DateTime.Parse(dr["fee_payment_date"].ToString());
                    mr.BonusBalance = GetMembershipRewardsBonus(mr.Id);
                }
            }

            return mr;
        }

        public static List<MembershipRewardsBonusHistory> GetCardMembershipRewardsBonusHistory(string cardNumber, DateTime startDate, DateTime endDate)
        {
            List<MembershipRewardsBonusHistory> mrBonusHistory = new List<MembershipRewardsBonusHistory>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT cb.ID, cb.cardnumber, cb.RegDate, cb.MR_Id, cb.debet_credit, cb.bonus_scores, cb.description , cb.reason_id, cb.Set_number,  
                                                        cb.last_day_of_bonus_calculation as last_day_of_bonus_calculation,  cl.transaction_number
                                        FROM Tbl_cards_bonus_history cb LEFT JOIN Tbl_visa_clearing cl ON cb.regdate = cl.dateget AND cb.uniquenumber = cl.unic_number  
                                        WHERE cardnumber = @cardNumber AND regdate>= @startDate AND regdate <= @endDate
                                        ORDER BY regdate ASC ";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate.Date;
                cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = endDate.Date;

                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    MembershipRewardsBonusHistory mrBonus = new MembershipRewardsBonusHistory();
                    mrBonus.ID = Convert.ToInt32(dr["Id"].ToString());
                    mrBonus.CardNumber = dr["CardNumber"].ToString();
                    mrBonus.MRID = Convert.ToInt32(dr["MR_Id"].ToString());
                    mrBonus.RegistrationDate = DateTime.Parse(dr["RegDate"].ToString());
                    mrBonus.BonusScores = float.Parse(dr["bonus_scores"].ToString());
                    mrBonus.DebetCredit = dr["debet_credit"].ToString();
                    mrBonus.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                    mrBonus.ReasonId = Convert.ToUInt16(dr["reason_id"].ToString());
                    mrBonus.SetNumber = Convert.ToInt32(dr["Set_number"].ToString());
                    mrBonus.LastDayOfBonusCalculation = dr["last_day_of_bonus_calculation"] != DBNull.Value ? DateTime.Parse(dr["last_day_of_bonus_calculation"].ToString()) : default(DateTime?);
                    mrBonus.TransactionNumber = dr["transaction_number"].ToString();
                    mrBonusHistory.Add(mrBonus);
                }
            }

            return mrBonusHistory;
        }

        internal static MembershipRewards GetCardMembershipRewardByID(long ID)
        {
            MembershipRewards mr = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT m.* 
                                        FROM tbl_MR_applications m 
                                                    LEFT JOIN tbl_visa_numbers_accounts v ON m.CardNumber = v.visa_number
                                        WHERE m.mr_id = @ID  AND  v.closing_date IS NULL";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    mr = new MembershipRewards();
                    mr.Id = Convert.ToInt32(dr["MR_Id"].ToString());
                    mr.Status = Convert.ToUInt16(dr["Status"].ToString());
                    mr.RegistrationDate = DateTime.Parse(dr["RegDate"].ToString());
                    mr.EndDate = DateTime.Parse(dr["EndDate"].ToString());
                    mr.ClosingDate = dr["Closing_date"] != DBNull.Value ? DateTime.Parse(dr["Closing_date"].ToString()) : default(DateTime?);
                    mr.ServiceFee = Convert.ToDouble(dr["ServiceFee"].ToString());
                    mr.ServiceFeeReal = Convert.ToDouble(dr["ServiceFeeReal"].ToString());
                    mr.ServiceFeePayed = Convert.ToDouble(dr["ServiceFeePayed"].ToString());
                    mr.SetNumber = Convert.ToInt32(dr["Set_number"].ToString());
                    mr.ValidationDate = dr["Validation_date"] != DBNull.Value ? DateTime.Parse(dr["Validation_date"].ToString()) : default(DateTime?);
                    mr.LastDayOfBonusCalculation = DateTime.Parse(dr["last_day_of_bonus_calculation"].ToString());
                    mr.BonusBalance = GetMembershipRewardsBonus(mr.Id);
                    mr.CardNumber = dr["CardNumber"].ToString();
                }
            }

            return mr;

        }

        public static double GetMembershipRewardsBonus(int Id)
        {
            double balanceBonus = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand("Select ISNULL(sum(case debet_credit when 'c' then bonus_scores else -bonus_scores end),0) as bonus from Tbl_cards_bonus_history where mr_id=@MRId", conn);
                cmd.Parameters.Add("@MRId", SqlDbType.Int).Value = Id;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    balanceBonus = Convert.ToDouble(dr["bonus"].ToString());
                }
            }
            return balanceBonus;
        }

        public static List<MembershipRewardsStatusHistory> GetCardMembershipRewardsStatusHistory(string cardNumber)
        {
            List<MembershipRewardsStatusHistory> mr = new List<MembershipRewardsStatusHistory>();


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT a.MR_Id, (select status from Tbl_type_of_MR_status where id=h.[Status]) as descr, h.*
                                        FROM Tbl_MR_Applications a INNER JOIN Tbl_MR_history h ON h.MR_Id=a.MR_Id 
                                        WHERE cardnumber = @cardNumber 
                                        ORDER BY a.mr_id asc, h.Change_date asc";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    MembershipRewardsStatusHistory oneHistory = new MembershipRewardsStatusHistory();
                    oneHistory.Id = Convert.ToInt32(dr["MR_Id"].ToString());
                    oneHistory.Status = Convert.ToUInt16(dr["Status"].ToString());
                    oneHistory.StatusDescription = dr["descr"].ToString();
                    oneHistory.ChangeDate = DateTime.Parse(dr["change_date"].ToString());
                    oneHistory.ChangeSetNumber = Convert.ToInt32(dr["set_number"].ToString());
                    mr.Add(oneHistory);
                }
            }

            return mr;
        }
    }
}
