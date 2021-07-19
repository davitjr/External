using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class SearchSwiftMessageDB
    {

        internal static List<SwiftMessage> GetSearchedSwiftMessages(SearchSwiftMessage searchSwiftMessage)
        {

            List<SwiftMessage> list = new List<SwiftMessage>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT   unic_number,
                                        filial,
                                        MT,
                                        registration_date,
                                        account_number,
                                        customer_number,
                                        SWIFT_code,
                                        deb_for_transfer_payment, 
                                        transaction_number,
                                        referance_number, 
                                        confirmation_date
                                        FROM Tbl_SWIFT_messages
                                        WHERE 1=1";



                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    string whereCondition = "";
                    if (!string.IsNullOrEmpty(searchSwiftMessage.Account?.AccountNumber))
                    {
                        whereCondition = whereCondition + " AND account_number=@accountNumber";
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = searchSwiftMessage.Account.AccountNumber;
                    }

                    if (searchSwiftMessage.CustomerNumber != 0)
                    {
                        whereCondition = whereCondition + " AND customer_number=@customerNumber";
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = searchSwiftMessage.CustomerNumber;
                    }

                    if (searchSwiftMessage.FilialCode != 0)
                    {
                        whereCondition = whereCondition + " AND Filial=@filial";
                        cmd.Parameters.Add("@filial", SqlDbType.Int).Value = searchSwiftMessage.FilialCode;
                    }

                    if (searchSwiftMessage.RegistartionDate != null)
                    {
                        whereCondition = whereCondition + " AND registration_date=@registrationDate";
                        cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = searchSwiftMessage.RegistartionDate.Value.Date;
                    }

                    if (searchSwiftMessage.DateFrom != null)
                    {
                        whereCondition = whereCondition + " AND registration_date>=@dateFrom";
                        cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = searchSwiftMessage.DateFrom.Value.Date;
                    }
                    if (searchSwiftMessage.DateTo != null)
                    {
                        whereCondition = whereCondition + " AND registration_date<=@dateTo";
                        cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = searchSwiftMessage.DateTo.Value.Date;
                    }


                    if (searchSwiftMessage.ConfirmationDate != null)
                    {
                        whereCondition = whereCondition + " AND confirmation_date=@confirmationDate";
                        cmd.Parameters.Add("@confirmationDate", SqlDbType.SmallDateTime).Value = searchSwiftMessage.ConfirmationDate.Value.Date;
                    }

                    if (searchSwiftMessage.InputOutput != null)
                    {
                        whereCondition = whereCondition + " AND [I/O]=@inputOutput";
                        cmd.Parameters.Add("@inputOutput", SqlDbType.Int).Value = searchSwiftMessage.InputOutput;
                    }

                    if (!string.IsNullOrEmpty(searchSwiftMessage.SwiftCode))
                    {
                        whereCondition = whereCondition + " AND Swift_code=@swiftCode";
                        cmd.Parameters.Add("@swiftCode", SqlDbType.NVarChar).Value = searchSwiftMessage.SwiftCode;
                    }

                    if (!string.IsNullOrEmpty(searchSwiftMessage.TransactionNumber))
                    {
                        whereCondition = whereCondition + " AND transaction_number LIKE '" + searchSwiftMessage.TransactionNumber + "%'";
                    }

                    if (!string.IsNullOrEmpty(searchSwiftMessage.ReferanceNumber))
                    {
                        whereCondition = whereCondition + " AND referance_number LIKE '" + searchSwiftMessage.ReferanceNumber + "%'";
                    }

                    if (searchSwiftMessage.MessageAttachment != null)
                    {
                        if (searchSwiftMessage.MessageAttachment == 0)
                        {
                            whereCondition = whereCondition + " AND unic_number_bank_mail_in IS NULL";
                        }
                        if (searchSwiftMessage.MessageAttachment == 1)
                        {
                            whereCondition = whereCondition + " AND unic_number_bank_mail_in IS NOT NULL";
                        }
                    }

                    if (searchSwiftMessage.MessageType != null)
                    {
                        whereCondition = whereCondition + " AND message_type=@messageType";
                        cmd.Parameters.Add("@messageType", SqlDbType.Int).Value = searchSwiftMessage.MessageType;
                    }

                    if (searchSwiftMessage.MtCode != 0)
                    {
                        whereCondition = whereCondition + " AND MT=@mtCode";
                        cmd.Parameters.Add("@mtCode", SqlDbType.Int).Value = searchSwiftMessage.MtCode;
                    }


                    cmd.CommandText = sql + whereCondition;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            SwiftMessage message = new SwiftMessage();
                            message.ID = Convert.ToUInt64(dr["unic_number"]);

                            if (dr["filial"] != DBNull.Value)
                                message.FilialCode = Convert.ToUInt64(dr["filial"]);
                            message.MtCode = Convert.ToInt32(dr["MT"]);
                            message.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            message.Account = new Account(dr["account_number"].ToString());
                            message.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                            message.SWIFTCode = dr["SWIFT_code"].ToString();
                            message.FeeAccount = new Account(dr["deb_for_transfer_payment"].ToString());
                            if (dr["transaction_number"] != DBNull.Value)
                                message.TransactionNumber = dr["transaction_number"].ToString();
                            if (dr["referance_number"] != DBNull.Value)
                                message.ReferanceNumber = dr["referance_number"].ToString();
                            if (dr["confirmation_date"] != DBNull.Value)
                                message.ConfirmationDate = Convert.ToDateTime(dr["confirmation_date"]);
                            list.Add(message);

                        }
                    }



                }
            }


            return list;

        }


    }
}
