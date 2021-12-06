using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using ExternalBanking.XBManagement;

namespace ExternalBanking.DBManager
{
    class HBTokenDB
    {
        /// <summary>
        /// Վերադարձնում է նշված օգտագործողի բոլոր թոքեները
        /// </summary>
        /// <param name="HBUSerID"></param>
        /// <returns></returns>
        internal static List<HBToken> GetHBTokens(int HBUSerID, ProductQualityFilter filter)
        {
            List<HBToken> HBUserTokens = new List<HBToken> ();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT T.*, TT.token_type as token_type_description, TQ.qualityDescription as quality_description,TS.Description as token_sub_type_description, T.deviceTypeDescription 
                               FROM [tbl_Tokens] T 
                               INNER JOIN [Tbl_type_of_tokens] TT ON T.token_Type = TT.id 
                               LEFT JOIN tbl_type_of_token_quality TQ 
                               ON TQ.id=T.quality 
                               LEFT JOIN Tbl_SubType_Of_Tokens TS
                               ON TS.Id=T.token_sub_type 
                               WHERE t.user_id = @userId"; 
                               //and t.quality=@quality";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = HBUSerID;
                    cmd.Parameters.Add("@quality", SqlDbType.TinyInt).Value =(short)filter;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                HBToken hbToken = new HBToken();
                foreach (DataRow row in dt.Rows)
                {
                    hbToken = SetHBToken(row);
                    HBUserTokens.Add(hbToken);
                }
            }
            return HBUserTokens;
        }
        /// <summary>
        /// Վերադարձնում է նշված օգտագործողի ֆիլտրված թոքեները
        /// </summary>
        /// <param name="HBUSerID"></param>
        /// <returns></returns>
        internal static List<HBToken> GetFilteredHBTokens(int HBUSerID, HBTokenQuality filter)
        {
            List<HBToken> HBUserTokens = new List<HBToken>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                //  if @quality = 0 not defined returns all tokens
                string sql = @"SELECT T.date_of_activation, T.limit_of_day, T.limit_of_per_trans, T.token_serial, IIF(T.token_type = 1, N'Տոկեն սարք', T.deviceTypeDescription) as deviceTypeDescription, T.date_of_activation, T.token_type
                                      FROM [tbl_Tokens] T   
                                      WHERE T.user_id = @userId and (@quality = 0 OR @quality = T.quality)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = HBUSerID;
                    cmd.Parameters.Add("@quality", SqlDbType.TinyInt).Value = (short)filter;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            HBUserTokens.Add(new HBToken
                            {
                                TokenNumber = dr["token_serial"].ToString(),
                                TokenType = (HBTokenTypes)Convert.ToInt16(dr["token_type"].ToString()),
                                DeviceTypeDescription = dr["deviceTypeDescription"] != DBNull.Value ? dr["DeviceTypeDescription"].ToString() : string.Empty,
                                DayLimit = double.Parse(dr["limit_of_day"].ToString()),
                                TransLimit = double.Parse(dr["limit_of_per_trans"].ToString()),
                                ActivationDate = dr["date_of_activation"] != DBNull.Value ? Convert.ToDateTime(dr["date_of_activation"]) : default(DateTime?)
                        });
                        }
                    }
                }     
            }
            return HBUserTokens;
        }

        /// <summary>
        /// Վերադարձնում է նշված մեկ տոկենը
        /// </summary>
        /// <param name="TokenID"></param>
        /// <returns></returns>
        internal static HBToken GetHBToken(int TokenID)
        {
            HBToken HBToken = new HBToken ();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT T.*, TT.token_type as token_type_description, TQ.qualityDescription as quality_description, TS.Description as token_sub_type_description
                                       FROM [tbl_Tokens] T 
                                       INNER JOIN [Tbl_type_of_tokens] TT ON T.token_Type = TT.id 
                                       LEFT JOIN tbl_type_of_token_quality TQ
                                       ON TQ.id=T.quality 
                                       LEFT JOIN Tbl_SubType_Of_Tokens TS
                                       ON TS.Id=T.token_sub_type 
                                       WHERE t.ID = @TokenID";                                      
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@TokenID", SqlDbType.BigInt).Value = TokenID;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        HBToken = SetHBToken(row);
                    }
                    else
                    {
                        HBToken = null;
                    }

                }
            }
            return HBToken;
        }
        internal static HBToken GetHBToken(string TokenSerial)
        {
            HBToken HBToken = new HBToken();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT T.*, TT.token_type as token_type_description, TQ.qualityDescription as quality_description, TS.Description as token_sub_type_description
                                       FROM [tbl_Tokens] T 
                                       INNER JOIN [Tbl_type_of_tokens] TT ON T.token_Type = TT.id 
                                       LEFT JOIN tbl_type_of_token_quality TQ
                                       ON TQ.id=T.quality 
                                       LEFT JOIN Tbl_SubType_Of_Tokens TS
                                       ON TS.Id=T.token_sub_type 
                                       WHERE t.token_serial = @TokenSerial";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@TokenSerial", SqlDbType.NVarChar).Value = TokenSerial;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        HBToken = SetHBToken(row);
                    }
                    else
                    {
                        HBToken = null;
                    }

                }
            }
            return HBToken;
        }
        /// <summary>
        /// HBToken-ի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static HBToken SetHBToken(DataRow row)
        {
            HBToken HBToken = new HBToken();

            if (row != null)
            {
                HBToken.ID = Convert.ToInt32(row["ID"].ToString());
                HBToken.HBUserID = Convert.ToInt32(row["user_id"].ToString());
                HBToken.TokenNumber = row["token_serial"].ToString();

                HBToken.TokenType = (HBTokenTypes)Convert.ToInt16(row["token_type"].ToString());
                HBToken.TokenSubType = (HBTokenSubType)Convert.ToInt16(row["token_sub_type"].ToString());

                HBToken.TokenTypeDescription =Utility.ConvertAnsiToUnicode(row["token_type_description"].ToString());
                HBToken.TokenSubTypeDescription = Utility.ConvertAnsiToUnicode(row["token_sub_type_description"].ToString());

                HBToken.ActivationDate = row["date_of_activation"] != DBNull.Value ? Convert.ToDateTime(row["date_of_activation"]) : default(DateTime?);
                HBToken.DeactivationDate = row["date_of_deactivation"] != DBNull.Value ? Convert.ToDateTime(row["date_of_deactivation"]) : default(DateTime?);
                HBToken.DayLimit = Double.Parse(row["limit_of_day"].ToString());
                HBToken.TransLimit = Double.Parse(row["limit_of_per_trans"].ToString());
                HBToken.IsBlocked = Boolean.Parse(row["blocked"].ToString());
                HBToken.BlockingDate = row["blocking_date"] != DBNull.Value ? Convert.ToDateTime(row["blocking_date"]) : default(DateTime?);
                HBToken.ActivationSetID = row["activation_set_id"] != DBNull.Value ? Convert.ToInt32(row["activation_set_id"]) : default(int); 
                HBToken.DeactivationSetID = row["deactivation_set_id"] != DBNull.Value ? Convert.ToInt32(row["deactivation_set_id"]) : default(int);
                HBToken.Issuer = row["Issuer"].ToString();
                HBToken.HBUser = HBUserDB.GetHBUser(HBToken.HBUserID);
                HBToken.Quality = (HBTokenQuality)Convert.ToInt16(row["quality"]!=DBNull.Value?Convert.ToByte(row["quality"].ToString()):default(byte)); 
                HBToken.QualityDescription = row["quality_description"].ToString();
                HBToken.IsRestorable  = row["IsRestorable"] != DBNull.Value ? Boolean.Parse(row["IsRestorable"].ToString()) : default(Boolean);
                HBToken.HBUser = HBUser.GetHBUser(HBToken.HBUserID);
                HBToken.GID = row["GID"] != DBNull.Value ? row["GID"].ToString() : default(String);
                HBToken.DeviceTypeDescription = row["deviceTypeDescription"] != DBNull.Value ? row["DeviceTypeDescription"].ToString() : default(String);
            }
            return HBToken;
        }

        /// <summary>
        ///Ամրագրում և վերադարձնում է մեկ հատ ազատ տոկեն
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="userFilial"></param>
        /// <returns></returns>
        internal static List<string> GetHBTokenNumers(HBTokenTypes tokenType, int userFilial)
        {
            List<string> HBAvailableTokens = new List<string>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (tokenType == HBTokenTypes.Token)
                    {
                        cmd.CommandText = @"SELECT top 10 token_serial FROM [Tbl_Token_List] WHERE [used] = 0 AND filial_code = @filial AND token_serial NOT IN                                        
                                                           (SELECT token_serial FROM Tbl_HB_Token_Order_Details TD INNER JOIN Tbl_HB_documents D ON TD.doc_Id = D.doc_ID WHERE  D.quality in (3,50))  AND token_serial NOT IN
                                                           (SELECT token_serial FROM tbl_Tokens)
                                          ORDER BY[token_serial] DESC";
                        cmd.Parameters.Add("@filial", SqlDbType.Int).Value = userFilial;
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT top 10 token_serial FROM [Tbl_Mobile_Token_List] WHERE [used] = 0  AND token_serial NOT IN
                                            (SELECT token_serial FROM tbl_Tokens) AND token_serial NOT IN
                                            (SELECT token_serial FROM Tbl_HB_Token_Order_Details TD INNER JOIN Tbl_HB_documents D ON TD.doc_Id = D.doc_ID WHERE  D.quality in (3,50))
                                            ORDER BY[token_serial] DESC";
                    }                    
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    
                }
                foreach (DataRow row in dt.Rows)
                {
                    HBAvailableTokens.Add(row["token_serial"].ToString());
                }
            }
            return HBAvailableTokens;
      }

        /// <summary>
        /// Թոկենի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="hbToken"></param>
        /// <param name="docId"></param>
        internal static void Save(int userID, SourceType source, HBToken hbToken, long docId, Action action)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            //ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBToken_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docId;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = hbToken.ID;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = hbToken.HBUser.ID;
                    cmd.Parameters.Add("@token_serial", SqlDbType.NVarChar, 16).Value = hbToken.TokenNumber;
                    cmd.Parameters.Add("@token_type", SqlDbType.TinyInt).Value = hbToken.TokenType;
                    cmd.Parameters.Add("@token_sub_type", SqlDbType.TinyInt).Value = (HBTokenSubType)hbToken.TokenSubType;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@day_limit", SqlDbType.Float).Value = hbToken.DayLimit;
                    cmd.Parameters.Add("@trans_limit", SqlDbType.Float).Value = hbToken.TransLimit;
                    cmd.Parameters.Add("@GID", SqlDbType.VarChar,2).Value = hbToken.GID;
                    cmd.Parameters.Add("@blocked", SqlDbType.Bit).Value = hbToken.IsBlocked;

                    cmd.ExecuteNonQuery();
                    
                }
                
            }

        }
        /// <summary>
        /// Ամրագրած տոկենների ազատում
        /// </summary>
        /// <param name="token"></param>
        internal static void CancelTokenNumberReservation(HBToken token  )
        { 
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    if (token.TokenType == HBTokenTypes.Token) 
                        cmd.CommandText = "update [Tbl_Token_List] set used = 0 where token_Serial = @token_serial";
                    else
                        cmd.CommandText = "update [Tbl_Mobile_Token_List] set used = 0 where token_Serial = @token_serial";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@token_serial", SqlDbType.VarChar,20).Value = token.TokenNumber;

                    cmd.ExecuteNonQuery();

                }

            }

        }
        /// <summary>
        /// Տոկենի սպասարկման վարձ
        /// </summary>
        internal static double GetHBServiceFee(ulong customerNumber, DateTime date, HBServiceFeeRequestTypes requestType, HBTokenTypes tokenType, HBTokenSubType tokenSubType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "SELECT dbo.[fn_get_ACBA_OnLine_service_Fee](@customerNumber,@date,@requestType,@tokenType,@tokenSubType) as fee";
                    
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float ).Value = customerNumber;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value =date;
                    cmd.Parameters.Add("@requestType", SqlDbType.SmallInt).Value = requestType;
                    cmd.Parameters.Add("@tokenType", SqlDbType.SmallInt).Value = tokenType;
                    cmd.Parameters.Add("@tokenSubType", SqlDbType.SmallInt).Value = tokenSubType;

                    return Convert.ToDouble(cmd.ExecuteScalar());
                }

            }

        }
     public static string GetHBTokenGID(int hbuserID,HBTokenTypes  tokenType)
     {
         using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "SELECT dbo.fn_get_HBToken_GID(@hbuserID,@tokenType)";
                    
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@hbuserID", SqlDbType.Int ).Value = hbuserID; 
                    cmd.Parameters.Add("@tokenType", SqlDbType.SmallInt).Value = tokenType; 

                    return Convert.ToString(cmd.ExecuteScalar());
                }

            }
     }

        /// <summary>
        /// Ստուգվում է տոկենի համարի հասանելիությունը
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckTokenNumberAvailability(HBToken token)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "pr_Check_Token_Number_Availability";

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@token_serial", SqlDbType.NVarChar,16).Value = token.TokenNumber; 

                    bool result = Convert.ToBoolean(cmd.ExecuteScalar());
                    return result;
                }

            }
                      
        }

        /// <summary>
        /// Ստուգվում է տոկենների քանակի հասանելիությունը
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckTokenQuantityAvailability(HBToken token)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "pr_Check_Token_Quantity_Availability";

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = token.HBUser.ID;
                    cmd.Parameters.Add("@token_type", SqlDbType.Int).Value = (int)token.TokenType;
                    
                    bool result = Convert.ToBoolean(cmd.ExecuteScalar());
                    return result;
                }
            }
        }
        
        /// <summary>
        /// Մոբայլ տոկենի գրանցման կոդի վերաուղարկման հայտի պահպանում 
        /// </summary>
        /// <param name="order"></param>
        internal static ActionResult SaveHBRegistrationCodeResendOrder(HBRegistrationCodeResendOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "pr_HB_Registration_Code_Resend_Order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)order.Type;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@token_serial", SqlDbType.VarChar, 20).Value = order.Token.TokenNumber;
                cmd.Parameters.Add("@token_id", SqlDbType.Int).Value = order.Token.ID;
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = order.Token.HBUserID;

                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
                result.ResultCode = ResultCode.Normal;
                order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                order.Quality = OrderQuality.Draft;
            }
                 
            return result;
        }

        public static bool HasCustomerOnlineBanking(ulong customerNumber)
        {
            using SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_Tokens T INNER JOIN Tbl_USers U ON T.user_id = U.id inner join Tbl_applications A on U.global_id = A.id
                                                    WHERE A.customer_number = @customerNumber AND A.type_of_client = 6 and T.quality in (1,5) ", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

            return cmd.ExecuteReader().Read();
        }
        public static bool HasCustomerOneActiveToken(ulong customerNumber)
        {
            using SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_Tokens T INNER JOIN Tbl_Users U ON T.user_id = U.id INNER JOIN Tbl_applications A on U.global_id = A.id
                                                    WHERE A.customer_number = @customerNumber  AND T.quality = 1", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

            return cmd.ExecuteReader().Read();
        }

         /// <summary>
        /// Մոբայլ տոկենի ակտիվացման տվյալների գրանցում հաճախորդի տվյալների ռիսկային փոփոխությունների առկայության դեպքում
        /// </summary>
        /// <param name="TokenSerial"></param>
        public static void SaveMobileTokenActivationDetailsForRiskyChanges(string TokenSerial)
        {
            using (SqlConnection dbConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    dbConn.Open();
                    cmd.Connection = dbConn;
                    cmd.CommandText = @" INSERT INTO Tbl_Mobile_Token_Activation_Details_For_Risky_Changes (token_serial,application_customer_number,balance,activation_date)
                                    SELECT @token_serial, A.customer_number, [dbo].[fn_get_customer_accounts_total_rest_AMD] (A.customer_number),T.date_of_activation
                                    FROM dbo.Tbl_Tokens T
                                    LEFT JOIN dbo.Tbl_Users U ON U.ID=T.user_id
                                    LEFT JOIN dbo.Tbl_Applications A on U.global_id = A.id
                                    WHERE T.token_serial= @token_serial ";

                    cmd.Parameters.Add("@token_serial", SqlDbType.NVarChar).Value = TokenSerial;

                    cmd.ExecuteNonQuery();
                }
            }
        }

    }//end of class

}
