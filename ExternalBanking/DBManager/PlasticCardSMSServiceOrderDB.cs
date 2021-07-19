using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    class PlasticCardSMSServiceOrderDB
    {
        internal static ActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_SMS_service_generation_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = order.ProductID;
                    if (string.IsNullOrEmpty(order.MobilePhone))
                    {
                        cmd.Parameters.Add("@mobilePhone", SqlDbType.NVarChar, 15).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@mobilePhone", SqlDbType.NVarChar, 15).Value = order.MobilePhone;
                    }

                    cmd.Parameters.Add("@operationType", SqlDbType.Int).Value = order.OperationType;
                    cmd.Parameters.Add("@SMSType", SqlDbType.Int).Value = order.SMSType;
                    cmd.Parameters.Add("@SMSFilter", SqlDbType.NVarChar, 15).Value = order.SMSFilter?.Replace(",", "");
                    cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = order.SetNumber;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    return result;
                }
            }

        }

        internal static string GetCardMobilePhone(string CardNumber)
        {
            string mobilePhone = "";


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT CASE WHEN LEN(Mobile_home) = 11 THEN  RIGHT(Mobile_home,(LEN(Mobile_home)-3))
                               ELSE CASE WHEN LEN(Mobile_home) = 6 THEN  '10' + Mobile_home ELSE  RIGHT(Mobile_home,(LEN(Mobile_home)-1)) END END  FROM Tbl_VISA_applications
                                 WHERE Cardnumber =@card_number ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = CardNumber;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row.ItemArray[0] != DBNull.Value)
                        {
                            mobilePhone = row.ItemArray[0].ToString();
                        }


                    }

                }
            }
            return mobilePhone;
        }

        internal static bool IsSecondSMSServiceOrder(ulong productId, short OperationType)
        {
            bool secondService = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT  d.doc_ID                                            
                                                  from Tbl_card_SMS_service_order_details  AS c  INNER join tbl_hb_documents AS d on  d.doc_ID=c.Doc_ID
                                                  where c.app_id=@appID and c.action_type=@actionType  and d.quality in(1,2,3,5)", conn);

                cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@actionType", SqlDbType.Int).Value = OperationType;

                if (cmd.ExecuteReader().HasRows)
                {
                    secondService = true;
                }



            }
            return secondService;
        }

        /// <summary>
        /// Ստանում ենք SMS  ծառայության համապատասխան հայտը իր մանրամասնություւներով
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PlasticCardSMSServiceOrder GetPlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            hb.filial,
                                            hb.customer_number,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number as hb_document_number,
                                            hb.document_subtype,
                                            hb.quality,
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            dt.app_ID,
                                            dt.mobile_Phone,
                                            dt.action_type,
	                                        dt.sms_type,
											dt.sms_filter,
                                            hb.confirmation_date,
                                            Act.[description] AS OperationTypeDescription,
                                            SMS.[description] AS SMSTypeDescription
                                            ,dt.set_number
                                            from Tbl_HB_documents hb inner join Tbl_card_SMS_service_order_details dt
                                            on dt.doc_id=hb.Doc_ID
											INNER JOIN dbo.tbl_type_of_arcaCardSmsServiceActions  Act ON Act.id=dt.action_type
											INNER JOIN dbo.tbl_type_of_cards_SMS SMS ON SMS.id=dt.sms_Type
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=@customer_number";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    order.SetNumber = Convert.ToInt32(dr["set_number"]);
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["hb_document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.ProductID = Convert.ToUInt64(dr["app_Id"].ToString());
                    order.MobilePhone = dr["mobile_phone"].ToString();
                    order.OperationType = short.Parse(dr["action_type"].ToString());
                    order.SMSType = short.Parse(dr["sms_type"].ToString());
                    order.SMSFilter = dr["sms_filter"].ToString();
                    order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                    order.OperationTypeDescription = Utility.ConvertAnsiToUnicode(dr["OperationTypeDescription"].ToString());
                    order.SMSTypeDescription = Utility.ConvertAnsiToUnicode(dr["SMSTypeDescription"].ToString());

                }
            }


            return order;
        }


        /// <summary>
        /// Վերադարձնում է SMS սերվիսի նախորդ ծառայության համապատասխան տվյալները
        /// <returns></returns>

        public static string SMSTypeAndValue(string CardNumber)
        {

            string SMSTypeAndValue = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT TOP 1 value FROM Tbl_cards_SMS_history 									   
                                       WHERE  Card_number = @card_number                                       
                                       ORDER BY id DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = CardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        SMSTypeAndValue = dt.Rows[0]["value"].ToString();
                    }
                }
            }
            return SMSTypeAndValue;
        }

        /// <summary>
        /// Վերադարձնում է պլաստիկ քարտի SMS սերվիսի գործողության տեսակը
        /// </summary>
        /// <returns></returns>

        public static DataTable GetPlasticCardSmsServiceActions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT id,description,description_eng FROM tbl_type_of_arcaCardSmsServiceActions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }


        /// <summary>
        /// Վերադարձնում է պլաստիկ քարտի SMS ծառայության տեսակը
        /// </summary>
        /// <returns></returns>

        public static DataTable GetTypeOfPlasticCardsSMS()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = @"SELECT id,LEFT([description],(LEN([description])-3)),LEFT([description_eng],(LEN([description_eng])-3)) 
                                  FROM tbl_type_of_cards_SMS WHERE ID NOT IN (5,6,9) ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                }


            }
            return dt;
        }

        public static DataTable GetAllTypesOfPlasticCardsSMS()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = @"SELECT id,[description]  FROM tbl_type_of_cards_SMS ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                }


            }
            return dt;
        }



        public static DataTable GetCardMobilePhones(ulong customerNumber, ulong cardNumber)
        {
            int type_of_client = GetTypeOfClient(customerNumber);
            DataTable dt = new DataTable();
            string sqltext = string.Empty;
            ulong mainCustomerNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                if (type_of_client == 6)
                {
                    sqltext = @"select distinct(substring(countryCode,2,3)+areaCode+phoneNumber) as phone from [Tbl_Customers] C 
                                join Tbl_Customer_Phones cp on c.identityId=cp.identityId and phoneType=1 and priority=1 join  Tbl_Phones p on cp.phoneid=p.id
                                where customer_number=@customer_number or customer_number in(select b.customer_number from Tbl_VISA_applications a 
                                left join [Tbl_SupplementaryCards] b on a.app_id=b.app_id
                                where a.customer_number=@customer_number and cardnumber=@cardNumber and CardStatus='NORM' 
                                and Cardnumber<>Maincardnumber and a.customer_number<> b.customer_number)  ";
                }
                else
                {
                    type_of_client = -1;
                    mainCustomerNumber = GetMainCustomerNumber(customerNumber, cardNumber);
                    if (mainCustomerNumber != 0)
                    {
                        type_of_client = GetTypeOfClient(mainCustomerNumber);
                    }
                    if (type_of_client != 6)
                    {
                        sqltext = @"select distinct(substring(countryCode,2,3)+areaCode+phoneNumber) as phone from [Tbl_Customers] C 
                                   join Tbl_Customer_Phones cp on c.identityId = cp.identityId and phoneType=1 and priority=1 
                                   join Tbl_Phones p on cp.phoneid = p.id
                                   where customer_number in(
                                   select distinct clp.lpcustomer_number from Tbl_Customers c
                                   join Tbl_Customers_Link_Persons clp
                                   on    c.customer_number = clp.customer_number
                                   where c.customer_number = @customer_number and quality = 1 and clp.type in(1, 4,8)								   
								   ) ";
                    }
                    else
                    {
                        sqltext = @"select distinct(substring(countryCode,2,3)+areaCode+phoneNumber) as phone from [Tbl_Customers] C 
                                   join Tbl_Customer_Phones cp on c.identityId = cp.identityId and phoneType=1 and priority=1
                                   join Tbl_Phones p on cp.phoneid = p.id
                                   where customer_number=@mainCustomerNumber or customer_number in
                                   (select distinct clp.lpcustomer_number from Tbl_Customers c
                                   join Tbl_Customers_Link_Persons clp
                                   on    c.customer_number = clp.customer_number
                                   where c.customer_number = @customer_number and quality = 1 and clp.type in(1, 4,8))";
                    }

                }


                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.NVarChar, 20).Value = customerNumber;
                    cmd.Parameters.Add("@mainCustomerNumber", SqlDbType.NVarChar, 20).Value = mainCustomerNumber;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        private static int GetTypeOfClient(ulong customerNumber)
        {
            int type;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"select 	type_of_client from	   Tbl_Customers
						where customer_number=@customer_number";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.NVarChar, 20).Value = customerNumber;
                    type = Convert.ToInt16(cmd.ExecuteScalar());
                }
            }
            return type;
        }

        private static ulong GetMainCustomerNumber(ulong customerNumber, ulong cardNumber)
        {
            ulong mainCardCustomerNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"select b.customer_number
                                   from Tbl_VISA_applications a left join [dbo].[Tbl_SupplementaryCards] b on a.app_id=b.app_id
                                   where a.customer_number=@customer_number and CardStatus='NORM'  and cardnumber=@cardNumber 
                                   and Cardnumber<>Maincardnumber 
                                   and a.customer_number<> b.customer_number";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.NVarChar, 20).Value = customerNumber;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;
                    mainCardCustomerNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return mainCardCustomerNumber;
        }


        public static List<PlasticCardSMSServiceHistory> GetPlasticCardAllSMSServiceHistory(ulong cardNumber)
        {
            List<PlasticCardSMSServiceHistory> list = new List<PlasticCardSMSServiceHistory>();
            //DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"select D.description as description, A.Date   as date, A.Value as newval, G.Name as filename, E.responseCode as answer,mobile_Phone,
                                   A.Set_Number as SetNumber 
                                   from Tbl_cards_SMS_history A join V_ArcaRequestHeaders H on H.id =A.headerID 
                                    join Tbl_Type_Of_ArcaCardSmsServiceActions D on A.Action= D.ID 
                                    left join  Tbl_ArcaFiles   G on G.ID =H.fileID 
                                    left join  V_ArcaResponse   E  with(nolock) on E.arcaAppID =H.arcaAppID 
                                    where isnull(A.deleted,0)<>1 and A.card_number= @cardNumber 
                                    order by A.Date desc  ";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        PlasticCardSMSServiceHistory order = new PlasticCardSMSServiceHistory();

                        order.RegDate = Convert.ToDateTime(dr["date"]);
                        order.OperationTypeDescription = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                        order.SmsTypeAndSum = dr["newval"].ToString();
                        order.FileName = Utility.ConvertAnsiToUnicode(dr["filename"].ToString());
                        order.ArcaAnswer = dr["answer"] != DBNull.Value ? (dr["answer"]).ToString() : null;  //dr["answer"].ToString();
                        order.SetNumber = dr["SetNumber"] != DBNull.Value ? Convert.ToUInt16(dr["SetNumber"]) : 0;
                        order.MobilePhone = dr["mobile_Phone"].ToString();
                        list.Add(order);
                    }


                }
                return list;
            }


        }

        public static string GetCurrentPhone(ulong cardNumber)
        {
            string currentPhone = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"select top 1 MOBILE_HOME  from Tbl_VISA_applications
								   where Cardnumber=@cardNumber  
								   order by GivenDate desc  ";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;
                    currentPhone = (cmd.ExecuteScalar()) == DBNull.Value ? null : (cmd.ExecuteScalar()).ToString();//  Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return currentPhone;
        }
        public static void GetResponseCodeAndLastActionFromCardSms(ulong productId, out int responseCode, out int lastAction)
        {
            responseCode = -1; lastAction = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"SELECT TOP 1 E.responseCode, D.id  as lastAction
                                    FROM V_ArcaRequestHeaders A 
		                            INNER JOIN V_ArcaFieldChanges C ON A.id=C.HeaderID 
                                    INNER JOIN Tbl_Type_Of_ArcaCardSmsServiceActions D ON C.NewValue= D.code 
                                    LEFT JOIN  V_ArcaResponse E ON E.arcaAppID =A.arcaAppId 
                                    WHERE  A.commandType=6 and C.fieldID = 123 AND C.Deleted=0 
		                            AND E.responseCode=0  AND A.appId IN   ( SELECT app_id  FROM Tbl_VISA_applications 
		                            WHERE Cardnumber IN (SELECT Cardnumber  FROM Tbl_VISA_applications WHERE app_id= @productId ))
		                            AND C.HeaderID NOT IN (SELECT HeaderID FROM V_ArcaFieldChanges WHERE HeaderID = C.HeaderID AND FieldID =122 AND NewValue ='SRVT1169' AND deleted <> 1)
                                    ORDER BY A.intputDate DESC  ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float, 20).Value = productId;

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        responseCode = dr["responseCode"] != DBNull.Value ? Convert.ToUInt16(dr["responseCode"]) : -1;
                        lastAction = dr["lastAction"] != DBNull.Value ? Convert.ToUInt16(dr["lastAction"]) : -1;
                    }
                }
            }

        }
        public static bool CheckASWACardSMS(ulong ProductID)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"SELECT top 1 1 FROM V_ArcaRequestHeaders H 
						INNER JOIN V_ArcaFieldChanges C ON H.id=C.headerid AND FieldID =122 
						WHERE appID=@ProductID AND NewValue ='SRVT1169' AND fileID IS NULL  ";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ProductID", SqlDbType.Float, 20).Value = ProductID;
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
        public static bool IsBTRTFileCreated(string cardNumber)
        {
            string filename = null; string answer = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"select top 1 G.Name as filename, E.responseCode as answer
                                   from Tbl_cards_SMS_history A join V_ArcaRequestHeaders H on H.id =A.headerID 
                                    join Tbl_Type_Of_ArcaCardSmsServiceActions D on A.Action= D.ID 
                                    left join  Tbl_ArcaFiles   G on G.ID =H.fileID 
                                    left join  V_ArcaResponse   E  with(nolock) on E.arcaAppID =H.arcaAppID 
                                    where isnull(A.deleted,0)<>1 and A.card_number= @cardNumber
                                    order by A.Date desc";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;

                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        filename = dr["filename"] == DBNull.Value ? null : dr["filename"].ToString();
                        answer = dr["answer"] == DBNull.Value ? null : dr["answer"].ToString();
                    }

                    return (filename != null && answer == null) ? true : false;
                }
            }

        }
    }
}

