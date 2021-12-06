using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class ProductNotificationConfigurationsOrderDB
    {

        internal static ActionResult Save(ProductNotificationConfigurationsOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"DECLARE @filial AS int                                                    
                                                 SELECT @filial=filialcode FROM dbo.Tbl_customers WHERE customer_number=@customerNumber   
                                                 INSERT INTO Tbl_HB_documents
                                                 (filial,customer_number,registration_date,document_type,
                                                 document_number,document_subtype,quality, 
                                                 source_type,operationFilialCode,operation_date)
                                                 VALUES
                                                 (@filial,@customerNumber,@regDate,@docType,@docNumber,@docSubType,
                                                 1,@sourceType,@operationFilialCode,@operDay)
                                                 UPDATE Tbl_HB_quality_history SET change_user_name = @userName 
                                                 WHERE Doc_ID = Scope_identity() and quality = 1
                                                 SELECT Scope_identity() as ID ", conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@regDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@docSubType", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (short)order.Source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            SaveOrderDetails(order);
            result.ResultCode = ResultCode.Normal;
            order.Quality = OrderQuality.Draft;
            return result;

        }
        internal static void SaveOrderDetails(ProductNotificationConfigurationsOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@" INSERT INTO tbl_product_notification_configurations_order_details
                                                    (
                                                    Doc_id,
                                                    product_notification_configuration_ID,
                                                    product_ID,
                                                    product_type,
                                                    information_type,
                                                    notification_option,
                                                    notification_frequency,
                                                    file_format,
                                                    language,
                                                    operation_type,
                                                    registration_date,
                                                    customer_number
                                                    )
                                                    VALUES
                                                    (
                                                    @DocId,
                                                    @productNotificationConfigurationID,
                                                    @productID,
                                                    @productType,
                                                    @informationType,
                                                    @notificationOption,
                                                    @notificationFrequency,
                                                    @fileFormat,
                                                    @language,
                                                    @operationType,
                                                    @registrationDate,
                                                    @customerNumber
                                                    )", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
            cmd.Parameters.Add("@productID", SqlDbType.Float).Value = order.Configuration.ProductId;
            //Փոփոխման և հեռացման դեպքերում 0 չէ
            //if (order.Configuration.ID != 0)
            //{
            cmd.Parameters.Add("@productNotificationConfigurationID", SqlDbType.Int).Value = order.Configuration.ID;
            //}                
            cmd.Parameters.Add("@productType", SqlDbType.SmallInt).Value = order.Configuration.ProductType;
            cmd.Parameters.Add("@informationType", SqlDbType.TinyInt).Value = order.Configuration.InformationType;
            cmd.Parameters.Add("@notificationOption", SqlDbType.TinyInt).Value = order.Configuration.NotificationOption;
            cmd.Parameters.Add("@notificationFrequency", SqlDbType.TinyInt).Value = order.Configuration.NotificationFrequency;
            cmd.Parameters.Add("@fileFormat", SqlDbType.TinyInt).Value = order.Configuration.FileFormat;
            cmd.Parameters.Add("@language", SqlDbType.Int).Value = order.Configuration.Language;
            cmd.Parameters.Add("@operationType", SqlDbType.TinyInt).Value = order.Configuration.OperationType;
            cmd.Parameters.Add("@registrationDate", SqlDbType.DateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.Configuration.CustomerNumber;
            cmd.ExecuteNonQuery();

            if (order.Configuration.NotificationOption == 2 || order.Configuration.NotificationOption == 5)
            {
                SaveProductNotificationConfigurationCommunicationsOrderDetails(order);
            }
        }

        private static void SaveProductNotificationConfigurationCommunicationsOrderDetails(ProductNotificationConfigurationsOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand();
            byte communicationType = 0;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            string cmdText = @"INSERT INTO tbl_product_notification_configuration_communications_order_details
                                                     (Doc_id,communication_type,communication_ID,all_communications)  
                                                     VALUES
                                                     (@DocId,@communicationType ,@communicationID,@allCommunications)";

            if (order.Configuration.CommunicationIds == null && order.Configuration.AllComunications == true)
            {
                cmd.CommandText = cmdText;
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@communicationID", SqlDbType.Float).Value = DBNull.Value;
                cmd.Parameters.Add("@communicationType", SqlDbType.TinyInt).Value = communicationType;
                cmd.Parameters.Add("@allCommunications", SqlDbType.Bit).Value = order.Configuration.AllComunications;
                cmd.ExecuteNonQuery();
            }
            else if (order.Configuration.CommunicationIds != null && order.Configuration.AllComunications == false)
            {
                foreach (int commId in order.Configuration.CommunicationIds)
                {
                    cmd.CommandText = cmdText;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                    if (order.Configuration.NotificationOption == 2)
                    {
                        communicationType = 1;
                    }

                    else if (order.Configuration.NotificationOption == 5)
                    {
                        communicationType = 2;
                    }
                    cmd.Parameters.Add("@communicationID", SqlDbType.Float).Value = commId;
                    cmd.Parameters.Add("@communicationType", SqlDbType.TinyInt).Value = communicationType;
                    cmd.Parameters.Add("@allCommunications", SqlDbType.Bit).Value = order.Configuration.AllComunications;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool IsExist(ProductNotificationConfigurationsOrder order)
        {

            bool status = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT id FROM tbl_product_notification_configurations                                                         
                                                           WHERE information_type=@informationType AND product_ID=@productId", conn);
                cmd.Parameters.Add("@informationType", SqlDbType.TinyInt).Value = order.Configuration.InformationType;
                cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = order.Configuration.ProductId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    status = true;
                }
            }
            return status;
        }


        internal static ProductNotificationConfigurationsOrder Get(ProductNotificationConfigurationsOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string sqlString = @"SELECT d.registration_date,d.document_number,d.document_subtype,d.document_type,d.quality,
                                    d.source_type,d.operation_date
                                    FROM Tbl_HB_documents D inner join 
                                    tbl_product_notification_configurations_order_details P on D.doc_ID=P.Doc_ID
                                    WHERE d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.doc_ID=@DocID";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            order.OrderNumber = dr["document_number"].ToString();
                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                            order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                            order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                            order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                        }
                    }
                }
            }
            return order;
        }

    }


}



