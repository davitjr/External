using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class EOServiceDB
    {

        internal static void SaveEOGetClientRequest(EOGetClientRequest request)
        {

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            conn.Open();

            using SqlCommand cmd = new SqlCommand("pr_insert_EO_get_client_request", conn);
            cmd.CommandType = CommandType.StoredProcedure;


            cmd.Parameters.Add("@requestID", SqlDbType.Int).Value = request.ID;
            cmd.Parameters.Add("@companyID", SqlDbType.NVarChar).Value = request.CompID;
            cmd.Parameters.Add("@passportNumber", SqlDbType.NVarChar).Value = request.Passport;
            cmd.Parameters.Add("@socialCardNumber", SqlDbType.NVarChar).Value = request.SSN.ToString();
            cmd.Parameters.Add("@productID", SqlDbType.NVarChar).Value = request.ProductID;
            cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = request.Telephone;
            SqlParameter param = new SqlParameter("@ID", SqlDbType.Int);
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();




        }

        internal static int SaveEOGetClientResponse(EOGetClientResponse response)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                int responseID = 0;

                using SqlCommand cmd = new SqlCommand("pr_insert_EO_get_client_response", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@requestID", SqlDbType.Int).Value = response.ParentID;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = response.Account;
                cmd.Parameters.Add("@errorCode", SqlDbType.NVarChar).Value = response.ErrorCode;
                cmd.Parameters.Add("@errorText", SqlDbType.NVarChar).Value = response.ErrorText;
                SqlParameter param = new SqlParameter("@responseID", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                //param.Value = order.TransferID;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();

                responseID = Convert.ToInt32(cmd.Parameters["@responseID"].Value);

                return responseID;

            }




        }

        internal static void SaveEOMakeTransferRequest(EOTransferRequest request)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("pr_insert_EO_make_transfer_request", conn);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.Add("@parentID", SqlDbType.Int).Value = request.ParentID;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = request.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = request.Currency;
                cmd.Parameters.Add("@account", SqlDbType.Float).Value = request.Account;
                SqlParameter param = new SqlParameter("@ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

            }


        }

        internal static void SaveEOMakeTransferResponse(EOTransferResponse response)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                int responseID = 0;

                using SqlCommand cmd = new SqlCommand("pr_insert_EO_make_transfer_response", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@parentID", SqlDbType.Int).Value = response.ParentID;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = response.Amount;
                cmd.Parameters.Add("@transferID", SqlDbType.Int).Value = response.TransferID;
                cmd.Parameters.Add("@errorCode", SqlDbType.NVarChar).Value = response.ErrorCode;
                cmd.Parameters.Add("@errorText", SqlDbType.NVarChar).Value = response.ErrorText;
                SqlParameter param = new SqlParameter("@responseID", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                //param.Value = order.TransferID;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();

                responseID = Convert.ToInt32(cmd.Parameters["@responseID"].Value);

                //return responseID;

            }




        }

    }
}
