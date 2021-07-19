using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class VehicleViolationResponseDB
    {
        /// <summary>
        /// Վերադարձնում է ՃՈ խախտումների ցանկը
        /// </summary>
        /// <param name="responseId">Հարցման հերթական համար</param>
        /// <returns></returns>
        internal static List<VehicleViolationResponse> GetVehicleViolationResponses(long responseId)
        {
            List<VehicleViolationResponse> responses = new List<VehicleViolationResponse>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT [ID]
                                                        ,[response_ID]
                                                        ,[violation_ID]
                                                        ,[violation_date]
                                                        ,[penalty_sum]
                                                        ,[fine_sum]
                                                        ,[payable_sum]
                                                        ,[requested_sum]
                                                        ,[payed_sum]
                                                        ,[bank_account]
                                                        ,[vehicle_passport]
                                                        ,[vehicle_number]
                                                        ,[vehicle_model]
                                                         from dbo.Tbl_Police_Response_Details WHERE response_ID = @responseID", conn);
                cmd.Parameters.Add("@responseID", SqlDbType.Int).Value = responseId;

                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    VehicleViolationResponse response = SetVehicleViolation(row);
                    responses.Add(response);
                }
            }

            return responses;
        }


        private static VehicleViolationResponse SetVehicleViolation(DataRow row)
        {
            VehicleViolationResponse violationResponse = new VehicleViolationResponse();

            if (row != null)
            {

                violationResponse.Id = long.Parse(row["ID"].ToString());

                violationResponse.ResponseId = long.Parse(row["response_ID"].ToString());

                violationResponse.ViolationNumber = row["violation_ID"].ToString();

                violationResponse.ViolationDate = DateTime.Parse(row["violation_date"].ToString());

                violationResponse.PenaltyAmount = float.Parse(row["penalty_sum"].ToString());

                violationResponse.FineAmount = float.Parse(row["fine_sum"].ToString());

                violationResponse.PayableAmount = float.Parse(row["payable_sum"].ToString());

                violationResponse.RequestedAmount = float.Parse(row["requested_sum"].ToString());

                violationResponse.PayedAmount = float.Parse(row["payed_sum"].ToString());

                violationResponse.PoliceAccount = row["bank_account"].ToString();

                violationResponse.VehiclePassport = row["vehicle_passport"].ToString();

                violationResponse.VehicleNumber = row["vehicle_number"].ToString();

                violationResponse.VehicleModel = row["vehicle_model"].ToString();

            }
            return violationResponse;

        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտուման հարցման պատասխանը
        /// </summary>
        /// <param name="id">Հարցման արդյունքում ստացված պատասխանի ունիկալ համար</param>
        /// <returns></returns>
        internal static VehicleViolationResponse GetVehicleViolationResponseDetails(long id)
        {
            VehicleViolationResponse response = new VehicleViolationResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT [ID]
                                                        ,[response_ID]
                                                        ,[violation_ID]
                                                        ,[violation_date]
                                                        ,[penalty_sum]
                                                        ,[fine_sum]
                                                        ,[payable_sum]
                                                        ,[requested_sum]
                                                        ,[payed_sum]
                                                        ,[bank_account]
                                                        ,[vehicle_passport]
                                                        ,[vehicle_number]
                                                        ,[vehicle_model]
                                                from dbo.Tbl_Police_Response_Details WHERE ID = @id", conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                    DataRow row = dt.Rows[0];
                    response = SetVehicleViolation(row);
            }

            return response;
        }

        public static short CheckVehicleViloationOperation(long policeResponseDetailsId)
        {
            short errorCode = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_police_violations_validation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@policeResponseDetailsID", SqlDbType.Int).Value = policeResponseDetailsId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                       if (dr.Read())
                        {   if (dr["errorHBNumber"]!= DBNull.Value)
                            errorCode = short.Parse(dr["errorHBNumber"].ToString());
                        }
                    }
                }
            }
            return errorCode;
        }

    }

}
