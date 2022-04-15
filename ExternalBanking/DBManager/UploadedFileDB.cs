using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;

namespace ExternalBanking.DBManager
{
    public class UploadedFileDB
    {
        internal static string SaveUploadedFile(UploadedFile uploadedFile)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();


                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = "sp_SaveUploadedFile";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SystemNumber", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@FileType", SqlDbType.VarChar, 4).Value = uploadedFile.FileType;
                    cmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 50).Value = uploadedFile.FileName;
                    cmd.Parameters.Add("@FileSize", SqlDbType.BigInt).Value = uploadedFile.FileSize;

                    cmd.Parameters.Add("@File", SqlDbType.VarBinary).Value = !String.IsNullOrEmpty(uploadedFile.FileInBase64) ?
                                                                             Convert.FromBase64String(uploadedFile.FileInBase64) : uploadedFile.File;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = uploadedFile.CustomerNumber;

                    SqlParameter parameter = new SqlParameter("@fileID", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(parameter);

                    cmd.ExecuteNonQuery();

                    uploadedFile.FileID = (cmd.Parameters["@fileID"].Value).ToString();

                }


                return uploadedFile.FileID;
            }

        }



        internal static ReadXmlFileAndLog ReadXmlFile(string fileId, short filial, ulong customerNumber, string userName)
        {
            ReadXmlFileAndLog readXmlFileAndLog = new ReadXmlFileAndLog();
            readXmlFileAndLog.actionResult = new ActionResult();
            readXmlFileAndLog.paymentOrders = new List<PaymentOrder>();

            List<PaymentOrder> list = new List<PaymentOrder>();
            ActionResult result = new ActionResult();



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("ReadXMLFile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filial;
                    cmd.Parameters.Add("@file_id", SqlDbType.NVarChar, 200).Value = fileId;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {

                                PaymentOrder paymentOrder = new PaymentOrder();
                                paymentOrder.DebitAccount = new Account();
                                paymentOrder.ReceiverAccount = new Account();
                                paymentOrder.Id = Convert.ToInt64(dr["doc_id"]);
                                paymentOrder.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                                paymentOrder.OrderNumber = dr["document_number"].ToString();
                                paymentOrder.DebitAccount.AccountNumber = dr["debet_account"].ToString();
                                paymentOrder.ReceiverAccount.AccountNumber = dr["receiver_acc"].ToString();
                                paymentOrder.Amount = Convert.ToDouble(dr["amount"]);
                                paymentOrder.Currency = dr["currency"].ToString();
                                paymentOrder.SubTypeDescription = Utility.ConvertAnsiToUnicode(dr["transfer_type_descr"].ToString());
                                list.Add(paymentOrder);


                            }
                            readXmlFileAndLog.paymentOrders = list;
                            result = GetUploadedFileLog(fileId);

                            readXmlFileAndLog.actionResult = result;

                        }
                        else
                        {

                            result = GetUploadedFileLog(fileId);
                            result.Errors.Add(new ActionError(1595));
                            readXmlFileAndLog.actionResult = result;
                        }
                    }
                }

                return readXmlFileAndLog;
            }
        }



        internal static ActionResult GetUploadedFileLog(string fileId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("GetUploadLog", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@file_id", SqlDbType.NVarChar, 200).Value = fileId;


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {

                                result.Errors.Add(new ActionError(1501, new string[] { dr.GetString(2) + " (" + Utility.ConvertAnsiToUnicode(dr.GetString(4)) + ")" }));
                            }

                        }
                    }

                }


            }


            return result;
        }

        internal static byte[] GetAttachedFile(long docID, int type)
        {

            byte[] arr = null;
            string path = string.Empty;
            string fileType = string.Empty;

            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "sp_GetAttachedFile";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Transaction = transaction;
                        command.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;

                        command.Parameters.Add("@type", SqlDbType.SmallInt).Value = type;

                        SqlParameter extension = new SqlParameter("@extension", SqlDbType.VarChar, 10);
                        extension.Direction = ParameterDirection.Output;
                        command.Parameters.Add(extension);

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            dataTable.Load(dr);
                        }
                        if (dataTable.Rows.Count > 0)
                        {
                            path = dataTable.Rows[0]["column1"].ToString();
                            fileType = dataTable.Rows[0]["attachment_type"].ToString();


                            command.CommandText = "SELECT GET_FILESTREAM_TRANSACTION_CONTEXT()";
                            command.CommandType = CommandType.Text;

                            byte[] objContext = (byte[])command.ExecuteScalar();

                            SqlFileStream objSqlFileStream = new SqlFileStream(path, objContext, FileAccess.Read);

                            arr = new byte[Convert.ToInt32(objSqlFileStream.Length) + 1];
                            objSqlFileStream.Read(arr, 0, arr.Length);
                            objSqlFileStream.Close();
                        }
                    }

                    transaction.Commit();
                }


                return arr;


            }
        }
    }

}



