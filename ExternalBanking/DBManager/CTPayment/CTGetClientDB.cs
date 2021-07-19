using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class CTGetClientDB
    {
        internal static Card GetCardByCardAccount(long cardAccount)
        {
            Card card = new Card();
            SqlDataReader dr;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT * 
                                                                              FROM tbl_visa_numbers_accounts 
                                                                              WHERE card_account = @cardAccount", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@cardAccount", SqlDbType.Float).Value = cardAccount;


                dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    card.CardNumber  = dr["card_account"].ToString();
                    if (dr["closing_date"] != DBNull.Value)
                    {
                        card.ClosingDate = Convert.ToDateTime(dr["closing_date"].ToString());
                    }

                    card.Currency = dr["currency"].ToString();
                }
                else
                {
                    card = null;
                }

            }

            return card;
        }

        internal static void  SaveEOGetClientRequestToDB(EOGetClientRequest request)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("pr_insert_CT_get_client_request", conn);
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



     
        }

        internal static int SaveEOGetClientResponseToDB(EOGetClientResponse response)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                int responseID = 0;

                SqlCommand cmd = new SqlCommand("pr_insert_CT_get_client_response", conn);
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

        internal static void SaveCTMakeTransferRequestToDB(EOTransferRequest request)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("pr_insert_CT_make_transfer_request", conn);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.Add("@parentID", SqlDbType.Int).Value = request.ParentID;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = request.Amount ;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = request.Currency;
                cmd.Parameters.Add("@account", SqlDbType.Float).Value = request.Account;
                SqlParameter param = new SqlParameter("@ID", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

            }


        }

        internal static void  SaveCTMakeTransferResponseToDB(EOTransferResponse response)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                int responseID = 0;

                SqlCommand cmd = new SqlCommand("pr_insert_CT_make_transfer_response", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@parentID", SqlDbType.Int).Value = response.ParentID;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = response.Amount;
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
