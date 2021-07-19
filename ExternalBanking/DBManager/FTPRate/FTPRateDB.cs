using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class FTPRateDB
    {

       
        internal static FTPRate GetFTPRateDetails(FTPRateType rateType)
        {
            FTPRate FTPRate = new FTPRate();

            FTPRate.rateType = rateType;
            FTPRate.FTPRateDetails = new List<FTPRateDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    if (rateType == FTPRateType.OwnFunds)
                    {
                        cmd.CommandText = @" SELECT DISTINCT ACCL.currency, ACCL.interest_rate
                                                                FROM (SELECT ACC.Arm_number,ACC.currency, NOO.interest_rate 
			                                                                FROM [Tbl_nostro_corr_acc;] NOO 
								                                                                INNER JOIN [tbl_all_accounts;] ACC ON NOO.account_number = ACC.Arm_number  
			                                                                WHERE (((type_of_account_new = '1890000A' OR  type_of_account_new ='1890010A') AND ACC.filialcode = 22000) OR ((type_of_account_new = '3900000P' OR  type_of_account_new ='3900010P') AND ACC.filialcode <> 22000))
					                                                                AND quality =1) ACCL
                                                                LEFT JOIN (SELECT Arm_number 
					                                                                FROM [System_accounts;]  SYST 
								                                                                INNER JOIN Tbl_fonds_system_accounts FOND ON SYST.nn = FOND.correspondent_account_system_number_in_CO) EXCL_1 ON ACCL.arm_number = EXCL_1.arm_number
                                                                LEFT JOIN (SELECT Arm_number 
					                                                                FROM [System_accounts;]  SYST 
								                                                                INNER JOIN Tbl_fonds_system_accounts FOND ON SYST.nn = FOND.correspondent_account_system_number_in_filial) EXCL_2 ON ACCL.arm_number = EXCL_2.arm_number
                                                                WHERE  EXCL_1.arm_number  IS NULL AND EXCL_2.arm_number  IS NULL ";
                    }
                    else if (rateType == FTPRateType.ResourcePayments)
                    {
                        cmd.CommandText = @" SELECT DISTINCT ACCL.currency, ACCL.interest_rate
                                                                FROM (SELECT ACC.Arm_number,ACC.currency, NOO.interest_rate 
			                                                                FROM [Tbl_nostro_corr_acc;] NOO 
								                                                                INNER JOIN [tbl_all_accounts;] ACC ON NOO.account_number = ACC.Arm_number  
			                                                                WHERE (((type_of_account_new = '1890000A' OR  type_of_account_new ='1890010A') AND ACC.filialcode <> 22000) OR ((type_of_account_new = '3900000P' OR  type_of_account_new ='3900010P') AND ACC.filialcode = 22000))
					                                                                AND quality =1) ACCL
                                                                LEFT JOIN (SELECT Arm_number 
					                                                                FROM [System_accounts;]  SYST 
								                                                                INNER JOIN Tbl_fonds_system_accounts FOND ON SYST.nn = FOND.correspondent_account_system_number_in_CO) EXCL_1 ON ACCL.arm_number = EXCL_1.arm_number
                                                                LEFT JOIN (SELECT Arm_number 
					                                                                FROM [System_accounts;]  SYST 
								                                                                INNER JOIN Tbl_fonds_system_accounts FOND ON SYST.nn = FOND.correspondent_account_system_number_in_filial) EXCL_2 ON ACCL.arm_number = EXCL_2.arm_number
                                                                WHERE  EXCL_1.arm_number  IS NULL AND EXCL_2.arm_number  IS NULL ";
                    }
                    else
                    {
                        cmd.CommandText = "";
                    }

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        FTPRateDetail rate = SetFTPRateDetail(row);

                        FTPRate.FTPRateDetails.Add(rate);
                    }

                }

            }
            return FTPRate;

        }

        private static FTPRateDetail SetFTPRateDetail(DataRow row)
        {
            FTPRateDetail  rate = new FTPRateDetail();

            if (row != null)
            {
                rate.Currency = row["currency"].ToString();
                rate.InterestRate = float.Parse(row["interest_rate"].ToString());

            }

            return rate;
        }

        internal static ActionResult SaveFTPRateOrder(FTPRateOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_FTP_Rate_document";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@rateType", SqlDbType.Int).Value = order.FTPRate.rateType;
                    //cmd.Parameters.Add("@fondDescription", SqlDbType.NVarChar, 250).Value = order.Fond.Description;
                    //cmd.Parameters.Add("@isSubsidia", SqlDbType.Bit).Value = order.Fond.IsSubsidia;
                    //cmd.Parameters.Add("@isActive", SqlDbType.Bit).Value = order.Fond.IsActive;

                    SqlParameter param = new SqlParameter("@ID", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@ID"].Value);
                    result.ResultCode = ResultCode.Normal;
                    result.Id = order.Id;

                    return result;
                }
            }

        }


        internal static void SaveFTPRateDetails(FTPRateOrder order)
        {

            order.FTPRate.FTPRateDetails?.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_FTP_rate_order_details
                                                            (Doc_ID, rate_type, currency, interest_rate)
                                                            VALUES 
                                                            (@DocID, @rateType, @currency, @interestRate)", conn))
                    {
                        cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = order.Id;
                        cmd.Parameters.Add("@rateType", SqlDbType.Int).Value = order.FTPRate.rateType;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = m.Currency;
                        cmd.Parameters.Add("@interestRate", SqlDbType.Float).Value = Math.Round(m.InterestRate / 100, 4);
                        //cmd.Parameters.Add("@providingTerminationDate", SqlDbType.SmallDateTime).Value = m.TerminationDate != null ? (object)m.TerminationDate : DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                }
            });


        }

      

    }
}
