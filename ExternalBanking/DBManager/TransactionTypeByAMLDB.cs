using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class TransactionTypeByAMLDB
    {
        internal static void SaveTransactionTypeByAML(long Id, TransactionTypeByAML transactionTypeByAML)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = " INSERT INTO tbl_transaction_type_ids (doc_id, transaction_type_id, transaction_additional_info) VALUES(@doc_ID, @transaction_type, @additional_description)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_ID", SqlDbType.BigInt).Value = Id;
                    cmd.Parameters.Add("@transaction_type", SqlDbType.Int).Value = (object)transactionTypeByAML.TransactionType ?? DBNull.Value;
                    cmd.Parameters.Add("@additional_description", SqlDbType.NVarChar).Value = (object)transactionTypeByAML.AdditionalDescription ?? DBNull.Value;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static bool CheckFor5mln(int ordertype, byte ordersubtype)
        {
            bool checkfor5ml = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select validate_for_5mln from tbl_transactionTypeByAML_validations where document_type = @document_type and document_subtype = @document_subtype";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = ordertype;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = ordersubtype;

                    cmd.ExecuteNonQuery();

                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    {
                        checkfor5ml = true;
                    }
                }
            }
            return checkfor5ml;
        }
        internal static bool CheckFor20mln(int ordertype, byte ordersubtype)
        {
            bool checkfor20ml = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select validate_for_20mln from tbl_transactionTypeByAML_validations where document_type = @document_type and document_subtype = @document_subtype";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = ordertype;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = ordersubtype;

                    cmd.ExecuteNonQuery();

                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    {
                        checkfor20ml = true;
                    }
                }
            }
            return checkfor20ml;
        }
        internal static TransactionTypeByAML GetTransactionTypeByAML(long Id)
        {
            TransactionTypeByAML transactionTypeByAML = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT  transaction_type_id, transaction_additional_info FROM tbl_transaction_type_ids WHERE doc_Id = @doc_ID";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_ID", SqlDbType.BigInt).Value = Id;

                    SqlDataReader rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        transactionTypeByAML = new TransactionTypeByAML()
                        {
                            TransactionType = int.Parse(rd["transaction_type_id"].ToString()),
                            AdditionalDescription = rd["transaction_additional_info"].ToString()
                        };
                    }
                }
            }
            return transactionTypeByAML;
        }


    }
}
