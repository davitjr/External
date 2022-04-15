using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class TransferAdditionalDataDB
    {
        internal static void Save(TransferAdditionalData data)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_Transfer_Additional_Data (doc_Id,sender_living_place,receiver_living_place,transfer_amount_type) 
                                            VALUES(@docId,@senderLivingPlace,@receiverLivingPlace,@transferAmountType)";

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = data.OrderId;
                    cmd.Parameters.Add("@senderLivingPlace", SqlDbType.Int).Value = data.SenderLivingPlace;
                    cmd.Parameters.Add("@receiverLivingPlace", SqlDbType.Int).Value = data.ReceiverLivingPlace;
                    cmd.Parameters.Add("@transferAmountType", SqlDbType.Int).Value = data.TransferAmountType;
                    cmd.ExecuteNonQuery();

                    string purposesInsertString = @"INSERT INTO Tbl_Transfer_Additional_Data_Purposes (doc_Id,purpose_code,amount,add_info)
                                                        VALUES(@docId,@purposeCode,@amount,@add_info)";
                    foreach (var p in data.TransferAmountPurposes)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = purposesInsertString;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Add("@docId", SqlDbType.Int).Value = data.OrderId;
                        cmd.Parameters.Add("@purposeCode", SqlDbType.Int).Value = p.PurposeCode;
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = p.Amount;
                        cmd.Parameters.Add("@add_info", SqlDbType.NVarChar, 250).Value = p.AddInfo != null ? p.AddInfo : "";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        internal static TransferAdditionalData GetTransferAdditionalData(long orderId)
        {
            var transferAdditionalData = new TransferAdditionalData();
            transferAdditionalData.TransferAmountPurposes = new List<TransferAmountPurposeDetail>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT sender_living_place,receiver_living_place,transfer_amount_type FROM Tbl_Transfer_Additional_Data WHERE doc_Id=@docId";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;
                    DataTable dt = new DataTable();

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            transferAdditionalData.ReceiverLivingPlaceDescription = Info.GetTransferReceiverLivingPlaceTypes().Find(x => x.Key == Convert.ToInt16(row["receiver_living_place"])).Value;
                            transferAdditionalData.SenderLivingPlaceDescription = Info.GetTransferSenderLivingPlaceTypes().Find(x => x.Key == Convert.ToInt16(row["sender_living_place"])).Value;
                            transferAdditionalData.TransferAmountTypeDescription = Info.GetTransferAmountTypes().Find(x => x.Key == Convert.ToInt16(row["transfer_amount_type"])).Value;
                        }
                    }
                    else
                    {
                        return null;
                    }
                    cmd.CommandText = @"SELECT purpose_code,amount,add_info FROM Tbl_Transfer_Additional_Data_Purposes WHERE doc_Id=@docId";
                    cmd.CommandType = CommandType.Text;

                    dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        int purposeCode = 0;
                        double amount = 0;
                        string addInfo = string.Empty;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            purposeCode = Convert.ToInt32(row["purpose_code"].ToString());
                            amount = Convert.ToDouble(row["amount"].ToString());
                            addInfo = row["add_info"].ToString();
                            transferAdditionalData.TransferAmountPurposes.Add(new TransferAmountPurposeDetail
                            {
                                PurposeCode = purposeCode,
                                Amount = amount,
                                AddInfo = addInfo
                            });
                        }
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    string accountsInsertString = @"SELECT description FROM Tbl_type_of_international_transfer_purpose WHERE id=@id";
                    for (int i = 0; i < transferAdditionalData.TransferAmountPurposes.Count; i++)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = accountsInsertString;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = transferAdditionalData.TransferAmountPurposes[i].PurposeCode;
                        transferAdditionalData.TransferAmountPurposes[i].Description = Utility.ConvertAnsiToUnicode(cmd.ExecuteScalar().ToString());
                    }
                }
            }
            return transferAdditionalData;
        }
    }
}
