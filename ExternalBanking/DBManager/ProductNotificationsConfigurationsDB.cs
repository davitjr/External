using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class ProductNotificationConfigurationsDB
    {
        public static List<ProductNotificationConfigurations> GetProductNotificationConfigurations(ulong productId)
        {

            List<ProductNotificationConfigurations> lstProductNotificationConfigurations = new List<ProductNotificationConfigurations>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT n.*,i.Description as notInfoDesc,o.Description as optionDesc,f.Description as frequencyDesc,l.Description as languageDesc,n.customer_number
                                                          FROM [tbl_product_notification_configurations] n                  
                                                          INNER JOIN tbl_types_of_product_notification_information i
                                                          ON i.id=n.information_type
                                                          INNER JOIN tbl_types_of_product_notification_option o
                                                          ON o.id=n.notification_option
                                                          INNER JOIN tbl_types_of_product_notification_frequency f
                                                          ON f.id=n.notification_frequency
                                                          INNER JOIN tbl_types_of_product_notification_language l
                                                          ON l.id=n.language
                                                          WHERE n.product_id=@productID", conn);

                cmd.Parameters.Add("@productID", SqlDbType.Float).Value = productId;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
                foreach (DataRow row in dt.Rows)
                {
                    ProductNotificationConfigurations config = SetProductNotificationConfiguration(row);
                    if (config.NotificationOption == 2 || config.NotificationOption == 5)
                    {
                        config.CommunicationIds = setCommunications(config);
                    }
                    lstProductNotificationConfigurations.Add(config);
                }


            }
            return lstProductNotificationConfigurations;
        }

        private static List<int> setCommunications(ProductNotificationConfigurations config)
        {
            List<int> communicationIds = new List<int>();
            using SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            string cmdText = string.Empty;
            if (config.NotificationOption == 2)
            {
                cmdText = @"SELECT  com.CustomerEmailId as communicationID, e.emailAddress , p.communication_type as comType   
                                        FROM tbl_product_notification_configuration_communications p
                                        INNER JOIN Tbl_Communications_By_Email com
                                        ON com.id=p.communication_ID
                                        INNER JOIN Tbl_Customer_Emails ce
                                        ON com.CustomerEmailId=ce.id
                                        INNER JOIN Tbl_Emails e
                                        ON ce.emailid=e.id
                                        WHERE p.product_notification_configuration_ID=@configID";
            }
            else if (config.NotificationOption == 5)
            {
                cmdText = @"SELECT  com.CustomerPhoneId as communicationID,ph.phoneNumber, p.communication_type as comType  
                                        FROM tbl_product_notification_configuration_communications p
                                        INNER JOIN Tbl_Communications_By_Phone com
                                        ON com.id=p.communication_ID
                                        INNER JOIN Tbl_Customer_Phones cp
                                        ON com.CustomerPhoneId=cp.id
                                        INNER JOIN Tbl_Phones ph
                                        ON cp.phoneid=ph.id
                                        WHERE p.product_notification_configuration_ID=@configID";
            }

            using SqlCommand cmd = new SqlCommand(cmdText, conn);
            cmd.Parameters.Add("@configID", SqlDbType.Float).Value = config.ID;
            DataTable dt = new DataTable();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                dt.Load(dr);
            }
            string comString = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                if (row != null)
                {
                    communicationIds.Add(int.Parse(row["communicationID"].ToString()));
                    //Email
                    if (short.Parse(row["comType"].ToString()) == 1)
                    {
                        comString += row["emailAddress"].ToString() + ",";
                    }
                    //Phone
                    if (short.Parse(row["comType"].ToString()) == 2)
                    {
                        comString += row["phoneNumber"].ToString() + ",";
                    }

                }

            }
            config.CommunicationsDescription = comString;
            return communicationIds;
        }


        private static ProductNotificationConfigurations SetProductNotificationConfiguration(DataRow row)
        {
            ProductNotificationConfigurations config = new ProductNotificationConfigurations();
            if (row != null)
            {
                config.ID = int.Parse(row["ID"].ToString());
                config.ProductId = Convert.ToUInt64(row["product_ID"].ToString());
                config.ProductType = Convert.ToInt16(row["product_type"].ToString());

                config.InformationType = Convert.ToByte(row["information_type"].ToString());
                config.NotificationOption = Convert.ToByte(row["notification_option"].ToString());
                config.NotificationFrequency = Convert.ToByte(row["notification_frequency"].ToString());



                config.FileFormat = Convert.ToByte(row["file_format"].ToString());
                //if (row["CustomerEmailId"] != DBNull.Value)
                //{
                //  config.EmailID = Convert.ToInt32(row["CustomerEmailId"].ToString());
                //}
                //config.EmailAddress = row["emailAddress"].ToString();
                config.Language = Convert.ToByte(row["language"].ToString());
                if (row["registration_date"] != DBNull.Value)
                {
                    config.RegistrationDate = Convert.ToDateTime(row["registration_date"].ToString());
                }
                if (row["application_date"] != DBNull.Value)
                {
                    config.ApplicationDate = Convert.ToDateTime(row["application_date"].ToString());
                }
                config.NotificationOptionDescription = Utility.ConvertAnsiToUnicode(row["optionDesc"].ToString());
                config.InformationTypeDescription = Utility.ConvertAnsiToUnicode(row["notInfoDesc"].ToString());
                config.NotificationFrequencyDescription = Utility.ConvertAnsiToUnicode(row["frequencyDesc"].ToString());
                config.LanguageDescription = Utility.ConvertAnsiToUnicode(row["languageDesc"].ToString());
                config.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
            }
            return config;
        }
    }


}



