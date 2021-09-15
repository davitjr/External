using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ServiceClient;

namespace ExternalBanking.DBManager
{
    static class CustomerDB
    {

        internal static bool CanUseCardAccountHB(ulong customerNumber)
        {
            bool canUseCardAccount = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id FROM [dbo].[Tbl_HB_Users] WHERE customer_number=@customerNumber and  using_card_accounts=1 ",
                        conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            canUseCardAccount = true;
                        }
                    }
                }

            }

            return canUseCardAccount;
        }

        internal static List<ulong> GetThirdPersonsCustomerNumbers(ulong customerNumber, SourceType Source)
        {
            DataTable dt = new DataTable();
            List<ulong> list = new List<ulong>();
            string str;
            string condition = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                if (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
                {
                    condition = " and acc.account_currency IN ('AMD', 'USD') ";
                }
                else
                {
                    condition = "";
                }

                str = @"select third_person_customer_number 
                                                 from   [Tbl_Co_Accounts_Main] acc 
                                                 INNER JOIN Tbl_co_accounts c 
                                                 on acc.ID = c.co_main_ID where type = 2 and acc.closing_date is null
                                                 and c.customer_number = @customer_number"
                                                + condition +
                                                " GROUP BY third_person_Customer_number ";



                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count == 0)
                    {
                        list = null;

                    }
                    else if (dt.Rows.Count == 1 && customerNumber == Convert.ToUInt64(dt.Rows[0]["third_person_customer_number"]))
                    {
                        list = null;
                    }
                    else
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (customerNumber != Convert.ToUInt64(dt.Rows[i]["third_person_customer_number"]))
                            {
                                list.Add(Convert.ToUInt64(dt.Rows[i]["third_person_customer_number"]));
                            }
                        }
                    return list;
                }
            }

        }

        /// <summary>
        ///  Ունի AcbaOnline թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static HasHB HasACBAOnline(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID FROM Tbl_HB_Users WHERE Closing_Date is null and Customer_Number =@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return HasHB.Yes;
                        }
                    }
                }


            }
            return HasHB.No;
        }

        /// <summary>
        ///  Ունի PhoneBanking թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static HasHB HasPhoneBanking(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select id from Tbl_PhoneBanking_Contracts WHERE application_Status=5 and customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return HasHB.Yes;
                        }
                    }
                }

            }
            return HasHB.No;
        }

        /// <summary>
        ///  PhoneBanking-ում ավտորիզացիա անցնող հաճախորդի հեռախոսահամար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static string GetPhoneBankingAuthorizationPhoneNumber(ulong customerNumber)
        {
            string phoneNumber;
            Customer customer = new Customer();
            ulong identityID = customer.GetIdentityId(customerNumber);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT REPLACE(countryCode,'+','')+ areaCode+phoneNumber as phoneNumber FROM Tbl_Communications_By_Phone CP
                                        INNER JOIN Tbl_Customer_Phones  P on CP.CustomerPhoneId=P.id 
                                        inner join Tbl_Phones ph on ph.id=p.phoneId  
                                        WHERE CP.CommunicationTypeId=2 and cp.identityId=@identityID";
                    cmd.Parameters.Add("@identityID", SqlDbType.Int).Value = identityID;
                    cmd.Connection = conn;
                    conn.Open();
                    phoneNumber = (string)cmd.ExecuteScalar();

                }
                return phoneNumber;
            }
        }
        /// <summary>
        /// Պահպանում է ExternalBanking ի մուտքի և ելքի պատմությունը 
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="action"> Login կամ Logout</param>
        /// <param name="setNumber">Պկ ի համար</param>
        /// <param name="sourceType">Տվյալների աղբյուր</param>
        internal static void SaveExternalBankingLogHistory(ulong customerNumber, ushort action, int setNumber, short sourceType, string sessionID, string clientIp, string onlineUserName, string authorizedUserSessionToken = " ")
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_external_banking_log_history";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@user_set_number", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = sourceType;
                    if (string.IsNullOrEmpty(sessionID))
                        cmd.Parameters.Add("@session_id", SqlDbType.NVarChar, 100).Value = "EXPIRED SESSION";
                    else
                        cmd.Parameters.Add("@session_id", SqlDbType.NVarChar, 100).Value = sessionID;
                    if (!string.IsNullOrEmpty(clientIp))
                        cmd.Parameters.Add("@client_Ip", SqlDbType.NVarChar, 20).Value = clientIp;
                    else
                        cmd.Parameters.Add("@client_Ip", SqlDbType.NVarChar, 20).Value = "EXPIRED SESSION";

                    cmd.Parameters.Add("@online_user_name", SqlDbType.NVarChar, 50).Value = onlineUserName;
                    cmd.Parameters.Add("@authorizedUserSessionToken", SqlDbType.NVarChar, 100).Value = authorizedUserSessionToken;

                    cmd.ExecuteNonQuery();

                }

            }




        }
        public static bool CheckForProvision(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fn_check_for_provision(@customerNumber)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }

            }
        }

        internal static double GetCustomerCashOuts(ulong customerNumber, string currency)
        {
            double cashOut = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [dbo].[Fnc_get_one_customer_cash_payments](@customerNumber,@currency,@date) as cash_outs", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = DateTime.Today.Date;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Double.TryParse(dr["cash_outs"].ToString(), out cashOut);
                        }
                    }
                }

            }

            return cashOut;
        }


        internal static List<CustomerDocument> GetCustomerDocumentList(ulong customerNumber)
        {
            ACBAServiceReference.Customer customer;
            short customerType;
            List<CustomerDocument> customerDocuments;

            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
            {
                customer = (ACBAServiceReference.Customer)proxy.GetCustomer(customerNumber);
                customerType = customer.customerType.key;
            }

            if (customerType == (short)CustomerTypes.physical)
            {
                customerDocuments = (customer as PhysicalCustomer).person.documentList;
            }
            else
            {
                customerDocuments = (customer as LegalCustomer).Organisation.documentList;

            }
            return customerDocuments;
        }

        internal static double GetCustomerAvailableAmount(ulong customerNumber, string currency)
        {
            double availableAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd =
                    new SqlCommand(
                        "SELECT [dbo].[fnc_available_CustomerAmount](@customerNumber,@currency,@date) as available_amount",
                        conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = Utility.GetNextOperDay().Date;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Double.TryParse(dr["available_amount"].ToString(), out availableAmount);
                        }
                    }
                }

            }
            if (currency == "AMD")
            {
                availableAmount = Math.Round(availableAmount, 1);
            }
            else
            {
                availableAmount = Math.Round(availableAmount, 2);
            }

            return availableAmount;
        }

        internal static bool HasCustomerCommitment(ulong customerNumber)
        {
            int commitment = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select dbo.fnc_Has_Customer_Commitment(@customerNumber) as commitment", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Int32.TryParse(dr["commitment"].ToString(), out commitment);
                        }
                    }
                }

            }

            return Convert.ToBoolean(commitment);
        }

        internal static short GetCustomerSyntheticStatus(ulong customerNumber)
        {
            short customerSyntheticStatus = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select dbo.fn_GetCustomerSyntheticStatus(@customerNumber) as syntheticStatus", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Int16.TryParse(dr["syntheticStatus"].ToString(), out customerSyntheticStatus);
                        }
                    }
                }

            }

            return customerSyntheticStatus;
        }

        internal static string GetPasswordForCustomerDataOrder(ulong CustomerNumber)
        {
            string Password;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select mother_name from[Tbl_customers] where customer_number = @CustomerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dr.Read();
                        Password = dr["mother_name"].ToString();
                    }
                }
            }
            return Password;
        }

        internal static string GetEmailForCustomerDataOrder(uint identityId)
        {
            string Email = "";
            SqlDataReader dr;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT top 10
                                                           e.[emailAddress]
                                                          FROM[dbo].[Tbl_Customer_Emails] ce
                                                              LEFT JOIN[dbo].[Tbl_Emails] e ON ce.[emailID] = e.[id]
                                                              where ce.[identityId] = @identityId ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@identityId", SqlDbType.Int).Value = identityId;

                    dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        Email = dr["emailAddress"].ToString();
                    }
                }
            }

            return Email;
        }

        internal static bool IsEmployee(ulong customerNumber)
        {
            bool IsEmployee = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select COUNT(1) as count from tbl_customers where link in (2, 3) and quality<>43 and customer_number=@customer_number";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.NVarChar).Value = customerNumber.ToString();

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        IsEmployee = int.Parse(dr["count"].ToString()) == 1 ? true : false;
                    }
                }
            }
            return IsEmployee;
        }
        internal static string GetCustomerPhoneNumber(ulong customerNumber)
        {
            string phoneNumber;
            Customer customer = new Customer();
            ulong identityID = customer.GetIdentityId(customerNumber);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT REPLACE(countryCode,'+','')+ areaCode+phoneNumber as phoneNumber FROM Tbl_Customer_Phones cp
                                        inner join Tbl_Phones p on cp.phoneId=p.id
                                        WHERE cp.phonetype=1 and cp.priority=1 and  cp.identityId=@identityID";
                    cmd.Parameters.Add("@identityID", SqlDbType.Int).Value = identityID;
                    cmd.Connection = conn;
                    conn.Open();
                    phoneNumber = (string)cmd.ExecuteScalar();

                }
                return phoneNumber;
            }
        }

        public static bool CheckMobileBankingCustomerDetailsRiskyChanges(string TokenSerial)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT dbo.fn_check_mobile_banking_customer_details_risky_changes(@token_serial)", conn);
                cmd.Parameters.Add("@token_serial", SqlDbType.NVarChar).Value = TokenSerial;

                bool hasRisks = Convert.ToBoolean(cmd.ExecuteScalar());

                return hasRisks;
            }

        }

        public static string GetCustomerHVHH(ulong customerNumber)
        {
            string HVHH = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT document_number 
                                                  FROM tbl_customer_documents_current d
                                                  INNER JOIN tbl_customers c
                                                  ON d.identityid = c.identityid
                                                  WHERE customer_number = @customer_number AND document_type = 19",conn);
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                conn.Open();
                HVHH = cmd.ExecuteScalar().ToString();
            }
            return HVHH;
        }

        public static bool HasBankrupt(ulong customerNumber)
        {
            bool hasBankrupt = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT *
                                                FROM v_customers_DAHK_amounts V 
                                                where v.customer_number=@customer_number AND v.blockage_type=1", conn);
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    hasBankrupt = true;
                }
            }
            return hasBankrupt;
        }
    }
}
