using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public static class UtilityDB
    {
        public static ulong InsertChangedLog(ObjectTypes objectType, ulong objectId, Action actionType, SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"INSERT INTO Tbl_changes_Log	(objectType,objectId,actionType)
										VALUES (@objectType, @objectId, @actionType)
										SELECT SCOPE_IDENTITY() as Id";

                command.Parameters.Add("@objectType", SqlDbType.SmallInt).Value = (ushort)objectType;
                command.Parameters.Add("@objectId", SqlDbType.BigInt).Value = objectId;
                command.Parameters.Add("@actionType", SqlDbType.SmallInt).Value = (ushort)actionType;


                return Convert.ToUInt64(command.ExecuteScalar());
            }
        }

        public static bool IsCorrectAccount(string accountNumber, short typeOfAccount, short typeOfProduct)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT  case dbo.fnc_IsCorrectSintAcc(
                                                                              (select type_of_account_new from [tbl_all_accounts;] where Arm_number = @accountNumber),
                                                                              @typeOfAccount,
                                                                              @typeOfProduct) when 1 then 1 else 0 end as res";
                    cmd.Parameters.Add("@accountNumber", SqlDbType.VarChar, 16).Value = accountNumber;
                    cmd.Parameters.Add("@typeOfAccount", SqlDbType.TinyInt).Value = typeOfAccount;
                    cmd.Parameters.Add("@typeOfProduct", SqlDbType.TinyInt).Value = typeOfProduct;

                    if (cmd.ExecuteScalar().ToString() == "1")
                        result = true;

                }
            }
            return result;

        }

        public static double GetLastExchangeRate(string currency, RateType rateType, ExchangeDirection direction, ushort filialCode = 22000)
        {

            double result;

            string fieldName = "";

            switch (rateType)
            {
                case RateType.Cash:
                    fieldName = "_cash";
                    break;
                case RateType.Card:
                    fieldName = "_ATM";
                    break;
                case RateType.Cross:
                    fieldName = "_For_Cross";
                    break;
                case RateType.Transfer:
                    fieldName = "_ACBA_transfer";
                    break;
                case RateType.NonCash:
                    fieldName = "";
                    break;
                default:
                    return 0;
            }

            if (direction == ExchangeDirection.Buy)
            {
                fieldName = "Buy_Rate" + fieldName;
            }
            else
            {
                fieldName = "Sale_Rate" + fieldName;
            }

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select top 1  " + fieldName + " as rate from Tbl_rates_sale_buy where Source_currency=@currency and filialcode=@filialCode order by With_Date desc ";
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;
                    cmd.Parameters.Add("@filialCode", SqlDbType.SmallInt).Value = filialCode;
                    if (cmd.ExecuteScalar() != null)
                    {
                        result = double.Parse(cmd.ExecuteScalar().ToString());
                    }
                    else
                        result = 0;
                }
            }

            return result;

        }

        public static double GetLastCbExchangeRate(string currency)
        {
            double result;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select top 1  exchange_rate as Rate from tbl_rates_cb where source_currency=@currency order by With_date desc";
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;

                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }

            return result;
        }

        public static string GetCurrencyCode(string currency)
        {
            string currencyCode = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT currencyCodeN  FROM [Tbl_currency;] WHERE currency = @currency";
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;
                    if (cmd.ExecuteScalar() != null)
                    {
                        currencyCode = cmd.ExecuteScalar().ToString();
                    }
                }
            }

            return currencyCode;
        }
        /// <summary>
        /// Աշխատանքային և ոչ աշխատաքային օրեր
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsWorkingDay(DateTime date)
        {
            bool workingday = true;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select dbo.sq_sunday(@myDate)", conn))
                {
                    cmd.Parameters.Add("@myDate", SqlDbType.DateTime).Value = date;
                    int i = Convert.ToInt32(cmd.ExecuteScalar());

                    if (i == 1)
                    {
                        workingday = false;
                    }
                    return workingday;
                }
            }
        }
        public static DateTime GetNextOperDay()
        {
            DateTime nextOperDay;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT ISNULL(next_oper_day,oper_day)  as result_day FROM Tbl_oper_days WHERE oper_day = (select oper_day from Tbl_current_oper_day)";
                    nextOperDay = (DateTime)cmd.ExecuteScalar();
                }
                return nextOperDay;

            }
        }



        public static DateTime GetCurrentOperDay()
        {
            DateTime currentOperDay;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select oper_day from Tbl_current_oper_day";
                    currentOperDay = (DateTime)cmd.ExecuteScalar();
                }
                return currentOperDay;

            }
        }



        /// <summary>
        /// Returns last key number by id and filial code.
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="FilialCode"></param>
        /// <returns></returns>
        internal static ulong GetLastKeyNumber(int keyId, ushort FilialCode)
        {
            ulong lastKeynumber = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_Get_LastKeyNumber", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = keyId;
                    cmd.Parameters.Add("@FilCode", SqlDbType.Int).Value = FilialCode;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            lastKeynumber = ulong.Parse(dr["LastKeyNumber"].ToString());
                        }
                        dr.Close();
                    }
                }
            }
            return lastKeynumber;
        }

        /// <summary>
        /// Վերադարձնում է համապատասխան հայտի հերթական համարը
        /// </summary>
        /// <param name="orderNumberType"></param>
        /// <param name="FilialCode"></param>
        /// <returns></returns>
        internal static ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType, ushort FilialCode)
        {

            ulong lastKeynumber = 0;
            string prName = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                if (orderNumberType == OrderNumberTypes.CashIn)
                {
                    prName = "pr_Get_LastCashInNumber";
                }
                else if (orderNumberType == OrderNumberTypes.CashOut)
                {
                    prName = "pr_Get_LastCashOutNumber";
                }
                else if (orderNumberType == OrderNumberTypes.MemOrder)
                {
                    prName = "pr_Get_LastMemOrderNumber";
                }
                else if (orderNumberType == OrderNumberTypes.InternationalOrder)
                {
                    prName = "pr_Get_LastInternationalTransferNumber";
                }
                else if (orderNumberType == OrderNumberTypes.OutMemOrder)
                {
                    prName = "pr_Get_LastOutMemOrderNumber";
                }
                else if (orderNumberType == OrderNumberTypes.RATransfer)
                {
                    prName = "pr_Get_LastRATransferNumber";
                }
                else if (orderNumberType == OrderNumberTypes.Convertation)
                {
                    prName = "pr_Get_LastConvertationNumber";
                }
                else if (orderNumberType == OrderNumberTypes.CorrectMemOrder)
                {
                    prName = "pr_Get_LastCorrectMemOrderNumber";
                }
                else if (orderNumberType == OrderNumberTypes.OperationByPeriod)
                {
                    prName = "pr_Get_LastOperationByPeriodNumber";
                }
                else if (orderNumberType == OrderNumberTypes.PaymentOrder)
                {
                    prName = "pr_Get_LastPaymentOrderNumber";
                }

                using (SqlCommand cmd = new SqlCommand(prName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (orderNumberType != OrderNumberTypes.InternationalOrder && orderNumberType != OrderNumberTypes.RATransfer)
                    {
                        cmd.Parameters.Add("@FilialCode", SqlDbType.Int).Value = FilialCode;
                    }
                    cmd.Parameters.Add(new SqlParameter("@lastNumber", SqlDbType.BigInt) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    lastKeynumber = Convert.ToUInt64(cmd.Parameters["@lastNumber"].Value);
                }
            }

            return lastKeynumber;
        }

        public static string GetUserFullName(long UserId)
        {
            string fullName = "";
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                DataTable dt = new DataTable();
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select First_Name,Last_Name from v_cashers_list where new_id=@UserId";
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                        fullName = dt.Rows[0]["First_Name"].ToString() + " " + dt.Rows[0]["Last_Name"].ToString();
                }
                return fullName;

            }
        }
        /// <summary>
        /// Վերադարձնում է տվյալ օրվա ԿԲ ի փոխարժեքը
        /// </summary>
        /// <param name="date"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static double GetCBKursForDate(DateTime date, string currency)
        {
            double rate;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fnc_kurs_for_date(@curency,DATEADD(day,DATEDIFF(day,0,@date),0))";
                    cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date;
                    cmd.Parameters.Add("@curency", SqlDbType.NVarChar).Value = currency;
                    rate = (double)cmd.ExecuteScalar();
                }
                return rate;

            }
        }

        /// <summary>
        /// Unicod-ի ստուգում
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static int IsTextUnicode(string text)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select [dbo].[IsTextUnicode] (@text) as IsUnicode";
                    cmd.Parameters.Add("@text", SqlDbType.NVarChar).Value = text;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Քարտի պրոդուկտի տեսակ 
        /// </summary>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public static int GetCardProductType(uint cardType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT p.code FROM Tbl_type_of_card t inner join Tbl_type_of_products p
                                                ON '10' + cast(t.CardSystemID as nvarchar(1)) = p.code
                                        WHERE t.ID=@cardType ";
                    cmd.Parameters.Add("@cardType", SqlDbType.Int).Value = cardType;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        internal static double GetPriceInfoByIndex(int index, string fieldName)
        {
            double result = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "select dbo.get_Price_info_by_index(@index,@fieldName) as price_info";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@index", SqlDbType.Int).Value = index;
                    cmd.Parameters.Add("@fieldName", SqlDbType.NVarChar, 100).Value = fieldName;
                    cmd.Connection = conn;
                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return result;
        }


        internal static string ConvertAnsiToUnicodeRussianDB(string str)
        {
            string result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fnc_convert_ansi_to_unicode_russian(@str)";
                    cmd.Parameters.Add("@str", SqlDbType.NVarChar).Value = str;
                    result = cmd.ExecuteScalar().ToString();
                }
                return result;

            }
        }

        internal static string ConvertUnicodeToAnsiRussianDB(string str)
        {
            string result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {


                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fnc_convert_unicode_to_ansi_russian(@str)";
                    cmd.Parameters.Add("@str", SqlDbType.NVarChar).Value = str;
                    result = cmd.ExecuteScalar().ToString();
                }
                return result;

            }
        }


        public static bool GetBankStatus(int bankCode)
        {
            bool result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select closing_date from [tbl_banks;] where Code=@bankCode";
                    cmd.Parameters.Add("@bankCode", SqlDbType.Int).Value = bankCode;
                    if (cmd.ExecuteScalar() != DBNull.Value)
                    {
                        result = false;
                    }
                    else
                        result = true;
                }
                return result;

            }
        }

        public static void InsertActionLog(string actionName, string productId, int setNumber, string clientIp)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = @"pr_insert_action_log";

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;
                    command.Parameters.Add("@action", SqlDbType.NVarChar, 50).Value = actionName;
                    command.Parameters.Add("@product_id", SqlDbType.NVarChar, 50).Value = productId;
                    if (clientIp != "")
                        command.Parameters.Add("@client_ip", SqlDbType.NVarChar, 50).Value = clientIp;

                    conn.Open();
                    command.ExecuteReader();

                }
            }
        }


        public static bool CanPayTransferByCall(short transferSystem)
        {
            bool result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select  dbo.[fn_can_pay_transfer_by_call](@transferSystem) canPay";
                    cmd.Parameters.Add("@transferSystem", SqlDbType.SmallInt).Value = transferSystem;
                    if (cmd.ExecuteScalar() != DBNull.Value)
                    {
                        result = Convert.ToBoolean(cmd.ExecuteScalar());
                    }
                    else
                        result = false;


                }
                return result;

            }
        }


        public static double GetCurrencyMinCashAmount(string currency)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT  dbo.fn_get_currency_min_cash_amount (@currency) ";
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    double.TryParse(cmd.ExecuteScalar().ToString(), out result);

                }
                return result;

            }
        }

        public static bool IsLatinLetter(string text)
        {
            bool isLatin = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select dbo.[fnc_is_latin_letter_and_number](@text)", conn))
                {
                    cmd.Parameters.Add("@text", SqlDbType.NVarChar, 500).Value = text;
                    int i = Convert.ToInt32(cmd.ExecuteScalar());

                    if (i == 1)
                    {
                        isLatin = true;
                    }
                    return isLatin;
                }



            }
        }

        public static short GetProductTypeFromLoanType(short loanType)
        {
            short result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT  dbo.Fnc_Get_ProductType_From_LoanType(@loanType) ";
                    cmd.Parameters.Add("@loanType", SqlDbType.Int).Value = loanType;
                    short.TryParse(cmd.ExecuteScalar().ToString(), out result);

                }
                return result;

            }
        }

        public static double GetKursForDate(DateTime date, string currency, ushort operationType, ushort filialCode)
        {
            double rate;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fnc_kurs_s_for_date(@tv,DATEADD(day,DATEDIFF(day,0,@dat),0),@oper_type,@fil)";
                    cmd.Parameters.Add("@dat", SqlDbType.DateTime).Value = date;
                    cmd.Parameters.Add("@tv", SqlDbType.NVarChar).Value = currency;
                    cmd.Parameters.Add("@oper_type", SqlDbType.Int).Value = operationType;
                    cmd.Parameters.Add("@fil", SqlDbType.Int).Value = filialCode;
                    rate = (double)cmd.ExecuteScalar();
                }
                return rate;

            }
        }
        public static ActionResult SendSMS(string phoneNumber, string messageText, int messageTypeID, int registrationSetNumber, SourceType sourceType)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_create_one_message", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@phone_number", SqlDbType.VarChar).Value = phoneNumber;
                    cmd.Parameters.Add("@identity_ID", SqlDbType.Int).Value = 0;
                    cmd.Parameters.Add("@message_text", SqlDbType.VarChar).Value = messageText;
                    cmd.Parameters.Add("@meesage_type_ID", SqlDbType.Int).Value = messageTypeID;
                    cmd.Parameters.Add("@toSend", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@sesionDescription", SqlDbType.NVarChar, 250).Value = "";
                    cmd.Parameters.Add("@source", SqlDbType.Int).Value = (int)sourceType;
                    cmd.Parameters.Add("@registrationSetNumber", SqlDbType.Int).Value = registrationSetNumber;
                    SqlParameter prm = new SqlParameter("@messageID", SqlDbType.BigInt);
                    prm.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(prm);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;

                }
            }
        }

        internal static int GetLastKeyNumber(int keyID)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_generate_key_number";

                    cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 50).Value = keyID;
                    cmd.Parameters.Add(new SqlParameter("@lastKey", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    int key = Convert.ToInt32(cmd.Parameters["@lastKey"].Value);
                    return key;
                }
            }

        }

        public static bool validate24_7_mode(OperDayMode operDay)
        {
            bool operType;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.fnc_validate_24_7_mode(@24_7_mode)";
                    cmd.Parameters.Add("@24_7_mode", SqlDbType.Int).Value = operDay.Option;
                    operType = (bool)cmd.ExecuteScalar();

                }
                return operType;
            }
        }

        internal static int GetProductTypeByAppId(ulong appId)
        {
            int productType = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "select dbo.fn_get_product_type_by_app_id(@appID) as productType";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = appId;
                    cmd.Connection = conn;
                    productType = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }
            }
            return productType;
        }

        public static OrderType GetDocumentType(int docId)
        {
            OrderType orderType;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select document_type from tbl_hb_documents where doc_ID = @doc_id";
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
                    try
                    {
                        orderType = (OrderType)cmd.ExecuteScalar();
                    }
                    catch (Exception)
                    {
                        orderType = OrderType.NotDefined;
                    }

                }
                return orderType;

            }
        }

        public static bool ValidateProductId(ulong customerNumber, ulong productId, ProductType productType)
        {
            string dbName = string.Empty;
            string dbName2 = string.Empty;
            switch (productType)
            {
                case ProductType.Loan:
                    dbName = "[dbo].[Tbl_short_time_loans;]";
                    dbName2 = "[dbo].[tbl_Paid_factoring]";
                    break;
                case ProductType.CreditLine:
                    dbName = "[dbo].[Tbl_Credit_lines]";
                    break;
                case ProductType.Card:
                    dbName = "[dbo].[Tbl_VISA_Applications]";
                    break;
                case ProductType.Deposit:
                    dbName = "[dbo].[Tbl_deposits;]";
                    break;
                case ProductType.PeriodicTransfer:
                    dbName = "[dbo].[Tbl_operations_by_period]";
                    break;
                case ProductType.Factoring:
                    dbName = "[dbo].[tbl_factoring]";
                    break;
                case ProductType.Guarantee:
                    dbName = "[dbo].[Tbl_given_guarantee]";
                    break;
                case ProductType.PaidGuarantee:
                    dbName = "[dbo].[Tbl_paid_factoring]";
                    break;
                case ProductType.AllTypeGuarantee:
                    dbName = "[dbo].[Tbl_given_guarantee]";
                    dbName2 = "[dbo].[Tbl_paid_factoring]";
                    break;
                case ProductType.Accreditive:
                    dbName = "[dbo].[Tbl_given_guarantee]";
                    break;
                default:
                    break;
            }

            if (dbName != string.Empty)
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = @"select app_id from " + dbName +
                                            "where app_id = @ProductId and customer_number = @customerNumber";
                        if (productType == ProductType.Loan || productType == ProductType.AllTypeGuarantee)
                        {
                            cmd.CommandText += @" union all
									select app_id from" + dbName2 +
                                    "where app_id = @ProductId and customer_number = @customerNumber";
                        }
                        if (productType == ProductType.Deposit)
                        {
                            cmd.CommandText = @"select app_id from [dbo].[Tbl_deposits;] d INNER JOIN v_all_accounts A
                                                    on d.deposit_full_number = A.arm_number where app_id = @ProductId and 
                                                        a.customer_number = @customerNumber";
                        }
                        cmd.Parameters.Add("@ProductId", SqlDbType.Float).Value = productId;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }

            return false;
        }


        public static bool ValidateDocId(ulong customerNumber, long docId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select doc_id from tbl_hb_documents 
                                        where doc_id = @docId and customer_number = @customerNumber";

                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }

        public static bool ValidateAccountNumber(ulong customerNumber, string accountNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT arm_number 
										FROM [tbl_all_accounts;] 
                                        WHERE arm_number = @accountNumber and customer_number = @customerNumber
                                        UNION ALL
                                        SELECT a.arm_number
                                        FROM V_All_Accounts A
                                        INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
                                        WHERE(type_of_product = 10) AND(type_of_account = 24) 
                                        GROUP BY sint_acc_new, type_of_client, type_of_product)s
                                        ON a.type_of_account_new = s.sint_acc_new 
                                        INNER JOIN Tbl_type_of_products t 
                                        ON type_of_product = t.code 
                                        Inner Join [tbl_all_accounts;] acc
                                        On a.Arm_number=acc.Arm_number
                                        WHERE  a.Closing_date is null 
                                        And a.Customer_Number = @customerNumber AND A.arm_number= @accountNumber   and Co_Type<>0
                                        And a.customer_number<>acc.customer_number
                                        ";

                    cmd.Parameters.Add("@accountNumber", SqlDbType.NVarChar, 20).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }
        public static bool ValidateCardNumber(ulong customerNumber, string cardNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select cardnumber from [tbl_visa_applications] 
                                    where cardnumber = @cardNumber and customer_number = @customerNumber";

                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cardNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }

        public static string GetARUSDocumentTypeCode(int ACBADocumentTypeCode)
        {
            string result;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select ARUS_code from Tbl_ACBA_ARUS_Document_Types where ACBA_code=@ACBADocumentTypeCode";
                    cmd.Parameters.Add("@ACBADocumentTypeCode", SqlDbType.Int).Value = ACBADocumentTypeCode;
                    result = (string)cmd.ExecuteScalar();
                }
                return result;

            }
        }

        public static void SaveCreditLineLogs(ulong productId, string funcName, string message)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO tbl_activate_credit_line_log_details
                                        VALUES(@productId, @funcName, GETDATE(), @message)";

                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@funcName", SqlDbType.NVarChar).Value = funcName;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = message;

                    cmd.ExecuteNonQuery();

                }
            }
        }

        public static DateTime GetLeasingOperDayForStatements()
        {
            DateTime operDay;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT TOP 1 oper_day FROM Tbl_short_time_loans_daily ORDER BY oper_day DESC";
                    operDay = (DateTime)cmd.ExecuteScalar();
                }
                return operDay;

            }
        }

        public static DateTime GetLeasingOperDay()
        {
            DateTime operDay;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT oper_day FROM Tbl_current_oper_day";
                    operDay = (DateTime)cmd.ExecuteScalar();
                }
                return operDay;

            }
        }

        public static double GetBuyKursForDate(string currency, int filialCode)
        {
            double rate;
            DateTime date = Utility.GetCurrentOperDay();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.[fnc_kurs_b_for_date](@curency,DATEADD(day,DATEDIFF(day,0,@date),0),0,@filialCode)";
                    cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date;
                    cmd.Parameters.Add("@curency", SqlDbType.NVarChar).Value = currency;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                    rate = (double)cmd.ExecuteScalar();
                }
                return rate;

            }
        }

    }
}

