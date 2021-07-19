using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public static class LeasingCustomerClassificationDB
    {

        internal static long GetLeasingCustomerNumber(int leasingCustomerNumber)
        {
            long customerNumber = 0;
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT Customer_number FROM [Tbl_customers;] WHERE number = @leasing_customer_number";
                    cmd.Parameters.Add("@leasing_customer_number", SqlDbType.Int).Value = leasingCustomerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["Customer_number"] != DBNull.Value)
                            {
                                customerNumber = Convert.ToInt64(dr["Customer_number"]);
                            }
                        }
                    }
                }
            }

            return customerNumber;
        }

        /// <summary>
        /// Հաճախորդի մասին տվյալներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingCustomerInfo(long customerNumber)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"  SELECT number,
	                                            customer_number, 
					                            IIF(type_of_client=6,lastname+' '+name,description) AS customer_name,
					                            IIF(type_of_client=6,(passport_number+', '+passport_inf+', '+ CONVERT(NVARCHAR(20),passport_date,11)),code_of_tax)  AS info_Document
	                                      FROM  [Tbl_customers;]
	                                      WHERE customer_number=@customer_number";
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["number"] != DBNull.Value)
                            {
                                customer.LeasingCustomerNumber = Convert.ToInt32(dr["number"]);
                            }
                            if (dr["customer_number"] != DBNull.Value)
                            {
                                customer.CustomerNumber = Convert.ToInt64(dr["customer_number"]);
                            }
                            if (dr["customer_name"] != DBNull.Value)
                            {
                                customer.CustomerFullName = dr["customer_name"].ToString();
                            }
                            if (dr["info_Document"] != DBNull.Value)
                            {
                                customer.InfoDocument = dr["info_Document"].ToString();
                            }

                        }
                    }
                }
            }

            return customer;
        }

        internal static List<LeasingCustomerClassification> GetLeasingCustomerSubjectiveClassificationGrounds(long customerNumber, bool isActive)
        {
            List<LeasingCustomerClassification> list = new List<LeasingCustomerClassification>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = String.Format(@"SELECT sb.id,sb.registration_date AS reg_date, 
                                        tc.description AS classification_type,
                                        sb.date_of_calculation AS classification_date,sb.overdue_days, ssb.description, sb.number, CASE WHEN sb.closed = 0 THEN N'Գործող' ELSE N'Փակված' END AS [status]
                                        FROM Tbl_store_sub sb 
                                        INNER JOIN Tbl_type_of_sub_store_basis ssb ON sb.basis_type = ssb.id
                                        INNER JOIN Tbl_store_new s ON s.number=sb.number
                                        INNER JOIN [Tbl_customers;] c ON c.number = sb.number
                                        INNER JOIN tbl_types_of_classification tc on ssb.classification_type = tc.Id
                                        WHERE c.Customer_number = @customer_number {0}
                                        GROUP BY sb.number,sb.date_of_calculation,sb.registration_date,ssb.id,tc.description,
                                        sb.id,ssb.description,sb.overdue_days,sb.closed", isActive == true ? " AND sb.closed = 0 " : "");
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LeasingCustomerClassification customer = new LeasingCustomerClassification();

                            customer.ClassificationID = Convert.ToInt32(reader["id"]);
                            if (reader["reg_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(reader["reg_date"]);
                            }
                            customer.ClassificationType = Convert.ToString(reader["classification_type"]);
                            customer.ClassificationDate = Convert.ToDateTime(reader["classification_date"]);
                            customer.OverdueDays = Convert.ToInt16(reader["overdue_days"]);
                            customer.Description = Convert.ToString(reader["description"]);
                            customer.Status = Convert.ToString(reader["status"]);

                            list.Add(customer);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Ստանում է տվյալներ (հաճախորդի սուբյեկտիվ դասակարգման հիմքեր աղյուսակի) ավելացնելու հիմքեր դաշտի համար 
        /// </summary>
        /// <param name="reasonType"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> GetLeasingReasonTypes(short classificationType)
        {
            Dictionary<string, string> reasons = new Dictionary<string, string>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT [id] ,[description] FROM [dbo].[Tbl_type_of_sub_store_basis] WHERE classification_type = @classification_type";
                    cmd.Parameters.Add("@classification_type", SqlDbType.TinyInt).Value = classificationType;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string key = dr["id"].ToString();
                            string value = dr["description"].ToString();
                            reasons.Add(key, Utility.ConvertAnsiToUnicode(value));
                        }
                    }
                }
            }
            return reasons;
        }

        /// <summary>
        /// վերադարձնում է Ռիսկաjին օրերի քանակը
        /// </summary>
        /// <param name="writtenOff"></param>
        /// <returns></returns>
        internal static Tuple<int, string> LeasingRiskDaysCountAndName(int riskClassCode)
        {
            Tuple<int, string> tuple = null;

            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT [Days_count_min], [RiskClassName_Unicode] FROM [Leasing_AccOper].[dbo].[Tbl_type_of_Risk] WHERE [RiskClassCode_num]=@RiskClassCode GROUP BY [Days_count_min], [RiskClassName_Unicode]";
                    cmd.Parameters.Add("@RiskClassCode", SqlDbType.TinyInt).Value = riskClassCode + 1;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            tuple = Tuple.Create(Convert.ToInt32(dr["days_count_min"]), (dr["RiskClassName_Unicode"]).ToString());
                        }
                    }
                }
            }
            return (tuple);
        }

        /// <summary>
        /// Ավելացնում է հաճախորդ (հաճախորդի սուբյեկտիվ դասակարգման հիմքեր աղյուսակում)
        /// </summary>
        /// <param name="registrationDate"></param>
        /// <param name="customerNumber"></param>
        /// <param name="setNumber"></param>
        /// <param name="docNumber"></param>
        /// <param name="docDate"></param>
        /// <param name="reasonId"></param>
        /// <param name="reasonType"></param>
        /// <param name="reasonAddText"></param>
        /// <param name="writtenOff"></param>
        /// <param name="reasonDate"></param>
        /// <param name="overdueDays"></param>
        /// <param name="classCalcByDays"></param>
        /// <returns></returns>
        internal static ActionResult AddLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj, int overdueDays, DateTime registrationDate, int setNumber)
        {
            ActionResult actionResult = new ActionResult();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"INSERT INTO [dbo].[Tbl_store_sub]([number],[date_of_calculation],[store_type],[basis_type],
                                        [attached_file],[confirmed],[date_of_confirmed],[overdue_days],[confirmed_sub],[class_calc_by_days],[report_number],[report_date],[additional_description],
                                        [set_number],[registration_date])
                                        VALUES(@number,@reason_date,@written_off,@reason_id,@attached_file,@confirmed,@date_of_confirmed,@overdue_days,@confirmed_sub,
                                        @class_calc_by_days,@document_number,@document_date,@reason_add_text,@set_number,@registration_date)";

                    cmd.Parameters.Add("@number", SqlDbType.BigInt).Value = obj.LeasingCustomerNumber;
                    cmd.Parameters.Add("@reason_date", SqlDbType.SmallDateTime).Value = obj.ClassificationDate;
                    cmd.Parameters.Add("@written_off", SqlDbType.SmallInt).Value = obj.RiskClassName;
                    cmd.Parameters.Add("@reason_id", SqlDbType.SmallInt).Value = Convert.ToInt32(obj.ClassificationReason);
                    cmd.Parameters.Add("@attached_file", SqlDbType.VarBinary).Value = DBNull.Value;
                    cmd.Parameters.Add("@confirmed", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@date_of_confirmed", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.Parameters.Add("@overdue_days", SqlDbType.SmallInt).Value = Convert.ToInt16(overdueDays);
                    cmd.Parameters.Add("@confirmed_sub", SqlDbType.Bit).Value = 0;
                    if (string.IsNullOrEmpty(obj.ReportNumber))
                    {
                        cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 100).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 100).Value = obj.ReportNumber;
                    }
                    if (string.IsNullOrEmpty(obj.ReportDate.ToString()) || obj.ReportDate.Year < 1990)
                    {
                        cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = obj.ReportDate;
                    }
                    if (string.IsNullOrEmpty(obj.AdditionalDescription))
                    {
                        cmd.Parameters.Add("@reason_add_text", SqlDbType.NVarChar, 200).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@reason_add_text", SqlDbType.NVarChar, 200).Value = obj.AdditionalDescription;
                    }
                    //cmd.Parameters.Add("@written_off", SqlDbType.Bit).Value = writtenOff == 5 ? 1 : 0;                    
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = setNumber;
                    cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = registrationDate;
                    cmd.Parameters.Add("@class_calc_by_days", SqlDbType.Bit).Value = obj.CalcByDays;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.Connection = conn;
                    result = cmd.ExecuteNonQuery();
                }
            }
            if (result == 1)
            {
                actionResult.ResultCode = ResultCode.Normal;
                //actionResult.= "Մուտքագրումը հաջողությամբ կատարվել է:";
            }
            return actionResult;
        }

        /// <summary>
        /// Վերադարձնում է  ՀաՃախորդի սուբյեկտիվ դասակարգման հիմքեր աղյուսակի ընտրված տողի տվյալները
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingCustomerSubjectiveClassificationGroundsByID(int id)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT [registration_date],ISNULL([set_number],'') AS set_number,
                                        c.description AS Reason_Type,
                                        ssb.description, ts.type AS store_type, [report_number],[report_date],[additional_description],date_of_calculation,
                                        overdue_days,IIF( closed=0,N'Գործող', N'Փակված') AS [status],ISNULL(closing_date,'') AS closing_date,
                                        ISNULL(closing_set_number,'')AS closing_set_number --,ISNULL(file_name,'')AS file_name
                                        FROM Tbl_store_sub s inner join Tbl_type_of_sub_store_basis ssb on s.basis_type=ssb.id 
										inner join tbl_types_of_classification c on ssb.classification_type = c.Id 
                                        INNER JOIN tbl_type_of_stor ts ON ts.id = s.store_type
                                        where s.id=@id";

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["set_number"] != DBNull.Value && Convert.ToInt32(dr["set_number"]) > 0)
                            {
                                customer.SetNumber = Convert.ToInt32(dr["set_number"]);
                            }
                            if (dr["Reason_Type"] != DBNull.Value)
                            {
                                customer.ClassificationType = dr["Reason_Type"].ToString();
                            }
                            if (dr["description"] != DBNull.Value)
                            {
                                customer.Description = dr["description"].ToString();
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (!string.IsNullOrEmpty(dr["additional_description"].ToString()))
                            {
                                customer.AdditionalDescription = dr["additional_description"].ToString();
                            }
                            else
                            {
                                customer.AdditionalDescription = string.Empty;
                            }
                            if (dr["date_of_calculation"] != DBNull.Value)
                            {
                                customer.ClassificationDate = Convert.ToDateTime(dr["date_of_calculation"]);
                            }
                            if (dr["overdue_days"] != DBNull.Value && Convert.ToUInt16(dr["overdue_days"]) > 0)
                            {
                                customer.OverdueDays = Convert.ToInt16(dr["overdue_days"]);
                            }
                            if (dr["status"] != DBNull.Value)
                            {
                                customer.Status = dr["status"].ToString();
                            }
                            if (dr["closing_date"] != DBNull.Value && Convert.ToDateTime(dr["closing_date"]).Year > 1990)
                            {
                                customer.ClosingReportDate = Convert.ToDateTime(dr["closing_date"]);
                            }
                            if (dr["closing_set_number"] != DBNull.Value && Convert.ToInt32(dr["closing_set_number"]) > 0)
                            {
                                customer.ClosedSetNumber = Convert.ToInt32(dr["closing_set_number"]);
                            }
                            if (dr["store_type"] != DBNull.Value)
                            {
                                customer.RiskClassName = Convert.ToString(dr["store_type"]);
                            }

                        }
                    }
                }
            }

            return customer;
        }

        /// <summary>
        /// Վերադարձնում է true եթե ընտրված դասը փակված է, հակառակ դեպքում ` false:
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static bool IsClosetClassification(long id)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT closed FROM Tbl_store_sub where id = @id";
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    result = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }


            return result;
        }

        /// <summary>
        ///  Հեռացնում է տող հաՃախորդի սուբյեկտիվ դասակարգման հիմքեր  աղյուսակից
        /// </summary>
        /// <param name="deletedId"></param>
        /// <param name="userId"></param>
        /// <param name="setDate"></param>
        /// <returns></returns> 
        internal static ActionResult CloseLeasingCustomerSubjectiveClassificationGrounds(long Id, int userId, DateTime setDate)
        {

            ActionResult actionResult = new ActionResult();
            if (IsClosetClassification(Id))
            {
                ActionError error = new ActionError();
                error.Description = "Ընտրված դասը փակված է:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);
            }
            else
            {
                string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
                int result = 0;
                using (SqlConnection conn = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = @"UPDATE Tbl_store_sub SET closed=1,
                                                                                         closing_set_number=@setNumber,
																		                 closing_date=@oper_day
                                            WHERE id=@ID";

                        cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = Id;
                        cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = setDate;
                        cmd.Connection = conn;
                        conn.Open();

                        cmd.Connection = conn;
                        result = cmd.ExecuteNonQuery();
                    }
                }
                if (result == 1)
                {
                    actionResult.ResultCode = ResultCode.Normal;
                    //actionResult.Message = "Դասակարգումը հաջողությամբ փակվել է:";
                }
            }
            return actionResult;
        }

        /// <summary>
        /// Հաճախորդի հետ փոխկապակցված անձանց չդասակարգելու հիմքեր (աղյուսակ 2)
        /// </summary>
        /// <returns></returns>
        internal static List<LeasingCustomerClassification> GetLeasingConnectionGroundsForNotClassifyingWithCustomer(long customerNumber, byte isActing)
        {
            List<LeasingCustomerClassification> list = new List<LeasingCustomerClassification>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {


                    cmd.CommandText = String.Format(@"SELECT lc.id,lc.registration_date,(CASE WHEN lc.number=@customer_number THEN lc.lp_number ELSE lc.number END) AS customer_number,
                                                    lc.[report_number],lc.[report_date],CASE WHEN lc.closed=0 THEN N'Գործող' ELSE N'Փակված' END AS [status] 
                                                    FROM [tbl_LP_not_classifying] lc inner join [Tbl_customers;] c ON lc.number=c.number 
                                                    WHERE (lc.number=@customer_number or lc.lp_number=@customer_number)  {0}
                                                    ORDER BY lc.registration_date", isActing == 1 ? " AND lc.closed=0 " : "");
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            LeasingCustomerClassification customer = new LeasingCustomerClassification();
                            if (dr["id"] != DBNull.Value)
                            {
                                customer.ClassificationID = Convert.ToInt32(dr["id"]);
                            }
                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["customer_number"] != DBNull.Value)
                            {
                                customer.CustomerNumber = Convert.ToInt64(dr["customer_number"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["status"] != DBNull.Value)
                            {
                                customer.Status = dr["status"].ToString();
                            }
                            list.Add(customer);
                        }
                    }
                }
            }

            return list;
        }


        /// <summary>
        /// Վերադարձնում է ընտրված հաճախորդին փոխկապակցված հաճախորդների տվըալները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<KeyValuePair<string, string>> GetLeasingInterconnectedPersonNumber(long customerNumber)
        {
            var list = new List<KeyValuePair<string, string>>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"select distinct lpnumber,CASE WHEN description IS NOT NULL THEN description ELSE name + ' ' + lastname END AS full_name from [Tbl_Customers_link_person_inf] where number=@customer_number";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Int).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string key = dr["lpnumber"].ToString();
                            string value = dr["lpnumber"].ToString() + " " + dr["full_name"].ToString();
                            list.Add(new KeyValuePair<string, string>(key, Utility.ConvertAnsiToUnicode(value)));

                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Վերադարևձնում է true, եթե տվյալ հաճախորդի համար գոյություն ունի մուտքագրված դասակարգելու հիմք
        /// </summary>
        /// <returns></returns>
        private static bool CheckCustomerClassification(long customerNumber, long customer_number_LP)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT * FROM [dbo].[tbl_LP_classifying] where closed=0 and (number=@customer_number   and lp_number=@customer_number_LP)
	                                                                                                 OR (number=@customer_number_LP and lp_number=@customer_number)";
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Parameters.Add("@customer_number_LP", SqlDbType.BigInt).Value = customer_number_LP;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = true;
                        }
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// Վերադարևձնում է true, եթե տվյալ հաճախորդի համար գոյություն ունի մուտքագրված զեկուցագիր
        /// </summary>
        /// <returns></returns>
        private static bool CheckCustomerReport(long customerNumber, long customer_number_LP)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT * FROM [dbo].[tbl_LP_not_classifying] WHERE closed=0 AND ((number=@customer_number and lp_number=@customer_number_LP) 
                                                                                                   OR   (number=@customer_number_LP and lp_number=@customer_number))";
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Parameters.Add("@customer_number_LP", SqlDbType.BigInt).Value = customer_number_LP;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        internal static ActionResult AddLeasingConnectionGroundsForNotClassifyingWithCustomer(LeasingCustomerClassification obj, DateTime setDate, int userId)
        {
            ActionResult actionResult = new ActionResult();
            if (CheckCustomerClassification(obj.LeasingCustomerNumber, obj.LinkedCustomerNumber))
            {
                ActionError error = new ActionError();
                error.Description = "Տվյալ հաճախորդի համար գոյություն ունի մուտքագրված դասակարգելու հիմք:";

                actionResult.Errors = new List<ActionError>();
                actionResult.ResultCode = ResultCode.ValidationError;
                actionResult.Errors.Add(error);
            }
            else if (CheckCustomerReport(obj.LeasingCustomerNumber, obj.LinkedCustomerNumber))
            {
                ActionError error = new ActionError();
                error.Description = "Տվյալ հաճախորդի համար գոյություն ունի մուտքագրված զեկուցագիր:";

                actionResult.Errors = new List<ActionError>();
                actionResult.ResultCode = ResultCode.ValidationError;
                actionResult.Errors.Add(error);
            }
            else
            {
                string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
                int result = 0;
                using (SqlConnection conn = new SqlConnection(config))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        cmd.CommandText = @"pr_Insert_linked_person_not_classification";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@number", SqlDbType.BigInt).Value = obj.LeasingCustomerNumber;
                        cmd.Parameters.Add("@lp_number", SqlDbType.BigInt).Value = obj.LinkedCustomerNumber;
                        cmd.Parameters.Add("@report_number", SqlDbType.NVarChar, 100).Value = obj.ReportNumber;
                        cmd.Parameters.Add("@report_date", SqlDbType.SmallDateTime).Value = obj.ReportDate;
                        cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = setDate;
                        cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = userId;

                        cmd.Connection = conn;
                        conn.Open();
                        result = cmd.ExecuteNonQuery();
                    }
                }
                if (result == 2)
                {
                    actionResult.ResultCode = ResultCode.Normal;
                    //actionResult.Message = "Մուտքագրումը հաջողությամբ կատարվել է:";
                }

            }

            return actionResult;
        }

        /// <summary>
        /// Վերադարձնում է Հաճախորդի հետ փոխկապակցված անձանց չդասակարգելու հիմքեր աղյուսակի ընտրված տողի տվյալները
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(int id)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT  
                                                registration_date,
			                                    ISNULL(set_number,'') AS set_number,
			                                    lp_number cust_num_LP ,
						                        ISNULL(report_number,'') AS report_number,
							                    ISNULL(report_date,'') AS report_date,
							                    IIF(closed=0,N'Գործող', N'Փակված') AS [status],
							                    ISNULL(closing_set_number,'') AS closing_set_number,
							                    ISNULL(closing_date,'') AS closing_date,
							                    ISNULL(closing_report_number,'') AS closing_report_number,
							                    ISNULL(closing_report_date,'') AS closing_report_date
                                FROM [tbl_LP_not_classifying] where id = @id";
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["set_number"] != DBNull.Value && Convert.ToInt32(dr["set_number"]) > 0)
                            {
                                customer.SetNumber = Convert.ToInt32(dr["set_number"]);
                            }
                            if (dr["cust_num_LP"] != DBNull.Value && Convert.ToInt64(dr["cust_num_LP"]) > 0)
                            {
                                customer.SubstitutePersonNumber = Convert.ToInt64(dr["cust_num_LP"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["status"] != DBNull.Value)
                            {
                                customer.Status = dr["status"].ToString();
                            }
                            if (dr["closing_set_number"] != DBNull.Value && Convert.ToInt32(dr["closing_set_number"]) > 0)
                            {
                                customer.ClosedSetNumber = Convert.ToInt32(dr["closing_set_number"]);
                            }
                            if (dr["closing_date"] != DBNull.Value && Convert.ToDateTime(dr["closing_date"]).Year > 1990)
                            {
                                customer.ClosingDateString = Convert.ToDateTime(dr["closing_date"]).ToString("dd/MM/yyyy");
                            }
                            if (dr["closing_report_number"] != DBNull.Value)
                            {
                                customer.ClosingReporNumber = dr["closing_report_number"].ToString();
                            }
                            if (dr["closing_report_date"] != DBNull.Value && Convert.ToDateTime(dr["closing_report_date"]).Year > 1990)
                            {
                                customer.ClosingReportDate = Convert.ToDateTime(dr["closing_report_date"]);
                            }
                        }
                    }
                }
            }

            return customer;
        }
        private static bool IsClosedClassification(int id)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT closed FROM tbl_LP_not_classifying WHERE ID =@ID";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (Convert.ToBoolean(dr["closed"]))
                            {
                                result = true;
                                return result;
                            }

                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// հեռացնում է  հաճախորդի հետ փոխկապակցված անձանց չդասակարգելու հիմքեր  աղյուսակի ընտրված տողը:
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="setDate"></param>
        /// <param name="docNumber"></param>
        /// <param name="docDate"></param>
        /// <returns></returns>
        internal static ActionResult CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(int userId, DateTime setDate, string docNumber, DateTime docDate, int id)
        {
            ActionResult actionResult = new ActionResult();
            if (IsClosedClassification(id))
            {
                ActionError error = new ActionError();
                error.Description = "Ընտրված տողը փակված է:";

                actionResult.Errors = new List<ActionError>();
                actionResult.ResultCode = ResultCode.ValidationError;
                actionResult.Errors.Add(error);

                return actionResult;
            }

            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @" UPDATE tbl_LP_not_classifying SET closed=1,
                                                               closing_set_number=@set_number,
															   closing_date=@oper_day,
															   closing_report_number=@txt_document_number,
															   closing_report_date=@txt_document_date
                                        WHERE id=@ID";

                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@txt_document_number", SqlDbType.NVarChar, 100).Value = docNumber;
                    cmd.Parameters.Add("@txt_document_date", SqlDbType.SmallDateTime).Value = docDate;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.Connection = conn;
                    result = cmd.ExecuteNonQuery();
                }
            }
            if (result == 1)
            {
                actionResult.ResultCode = ResultCode.Normal;
                //actionResult.Message = "Հաջողությամբ փակվել է:";
            }
            return actionResult;
        }

        /// <summary>
        /// Հաճախորդի հետ փոխկապակցված անձանց դասակարգելու հիմքեր (աղյուսակ 3)
        /// </summary>
        /// <returns></returns>
        internal static List<LeasingCustomerClassification> GetLeasingConnectionGroundsForClassifyingWithCustomer(long customerNumber, byte isActive)
        {

            List<LeasingCustomerClassification> list = new List<LeasingCustomerClassification>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {


                    cmd.CommandText = String.Format(@"SELECT lc.id,lc.registration_date,(CASE WHEN lc.number=@customer_number THEN lc.lp_number ELSE lc.number END) AS customer_number,
                                                    lc.[report_number],lc.[report_date],CASE WHEN lc.closed=0 THEN N'Գործող' ELSE N'Փակված' END AS [status] 
                                                    FROM [tbl_LP_classifying] lc inner join [Tbl_customers;] c ON lc.number=c.number 
                                                    WHERE (lc.number=@customer_number or lc.lp_number=@customer_number)  {0}
                                                    ORDER BY lc.registration_date", isActive == 1 ? " AND lc.closed=0 " : "");
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            LeasingCustomerClassification customer = new LeasingCustomerClassification();
                            if (dr["id"] != DBNull.Value)
                            {
                                customer.ClassificationID = Convert.ToInt32(dr["id"]);
                            }
                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["customer_number"] != DBNull.Value)
                            {
                                customer.CustomerNumber = Convert.ToInt64(dr["customer_number"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["status"] != DBNull.Value)
                            {
                                customer.Status = dr["status"].ToString();
                            }
                            list.Add(customer);
                        }
                    }
                }
            }

            return list;
        }

        internal static ActionResult AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(LeasingCustomerClassification obj, byte addORClose, DateTime setDate, int setNumber)
        {
            ActionResult actionResult = new ActionResult();

            if (addORClose == 1)
            {
                if (CheckCustomerClassification(obj.LeasingCustomerNumber, obj.LinkedCustomerNumber))
                {
                    ActionError error = new ActionError();
                    error.Description = "Տվյալ հաճախորդի համար գոյություն ունի մուտքագրված զեկուցագիր:";

                    actionResult.Errors = new List<ActionError>();
                    actionResult.ResultCode = ResultCode.ValidationError;
                    actionResult.Errors.Add(error);

                    return actionResult;
                }
                else if (CheckCustomerReport(obj.LeasingCustomerNumber, obj.LinkedCustomerNumber))
                {
                    ActionError error = new ActionError();
                    error.Description = "Տվյալ հաճախորդի համար գոյություն ունի մուտքագրված չդասակարգելու հիմք:";

                    actionResult.Errors = new List<ActionError>();
                    actionResult.ResultCode = ResultCode.ValidationError;
                    actionResult.Errors.Add(error);

                    return actionResult;
                }
            }

            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"pr_Insert_Or_Close_linked_person_classification";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (addORClose == 1)
                    {
                        cmd.Parameters.Add("@insert_close", SqlDbType.Bit).Value = 0;
                    }
                    else if (addORClose == 2)
                    {
                        cmd.Parameters.Add("@insert_close", SqlDbType.Bit).Value = 1;
                    }

                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = obj.Id;
                    cmd.Parameters.Add("@number", SqlDbType.BigInt).Value = obj.LeasingCustomerNumber;
                    cmd.Parameters.Add("@lp_number", SqlDbType.BigInt).Value = obj.LinkedCustomerNumber;
                    cmd.Parameters.Add("@report_number", SqlDbType.NVarChar, 100).Value = obj.ReportNumber;
                    cmd.Parameters.Add("@report_date", SqlDbType.SmallDateTime).Value = obj.ReportDate;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;

                    cmd.Connection = conn;
                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }
            if (result == 2)
            {
                if (addORClose == 1)
                {
                    actionResult.ResultCode = ResultCode.Normal;
                    //actionResult.Message = "Մուտքագրումը հաջողությամբ կատարվել է:";
                }
                else if (addORClose == 2)
                {
                    actionResult.ResultCode = ResultCode.Normal;
                    //actionResult.Message = "Հեռացումը հաջողությամբ կատարվել է:";
                }
            }

            return actionResult;
        }

        /// <summary>
        /// Վերադարձնում է  Հաճախորդի հետ փոխկապակցված անձանց դասակարգելու հիմքեր աղյուսակի ընտրված տողի տվըալները 
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingConnectionGroundsForClassifyingWithCustomerByID(int id, long customerNumber)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"   SELECT	    registration_date,
				                                                                    set_number,
				                                                                    report_number,
				                                                                    report_date,
				                                                                    closed,
				                                                                    closing_set_number,
				                                                                    closing_date,
				                                                                    closing_report_number,
				                                                                    closing_report_date,
                                                                                   (case when number=@customerNumber then lp_number else number end) cust_num_link
                                                                    FROM tbl_LP_classifying
                                                                    WHERE  ID= @id";
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["set_number"] != DBNull.Value)
                            {
                                customer.SetNumber = Convert.ToInt32(dr["set_number"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = (dr["report_number"]).ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["closed"] != DBNull.Value)
                            {
                                customer.Status = Convert.ToInt16(dr["closed"]) == 0 ? "Գործող" : "Փակված";
                            }
                            if (dr["closing_set_number"] != DBNull.Value)
                            {
                                customer.ClosedSetNumber = Convert.ToInt32(dr["closing_set_number"]);
                            }
                            if (dr["closing_date"] != DBNull.Value)
                            {
                                customer.ClosingDateString = Convert.ToDateTime(dr["closing_date"]).ToString("dd/MM/yyyy");
                            }
                            if (dr["closing_report_number"] != DBNull.Value)
                            {
                                customer.ClosingReporNumber = (dr["closing_report_number"]).ToString();
                            }
                            if (dr["closing_report_date"] != DBNull.Value)
                            {
                                customer.ClosingReportDate = Convert.ToDateTime(dr["closing_report_date"]);
                            }
                            if (dr["cust_num_link"] != DBNull.Value)
                            {
                                customer.CustomerNumber = Convert.ToInt64(dr["cust_num_link"]);
                            }
                        }
                    }
                }
            }

            return customer;
        }

        /// <summary>
        /// ՀԱՃԱԽՈՐԴԻ ԴԱՍԱԿԱՐԳՄԱՆ ՊԱՏՄՈՒԹՅՈՒՆ (աղյուսակ 4)
        /// </summary>
        /// <returns></returns>
        internal static List<LeasingCustomerClassification> GetLeasingCustomerClassificationHistory(int leasingCustomerNumber, DateTime date)
        {
            List<LeasingCustomerClassification> list = new List<LeasingCustomerClassification>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {


                    cmd.CommandText = @"SELECT S.date_of_calculation, r.RiskClassName, 
										CASE WHEN Q.store = 0.01 THEN N'úµÛ»ÏïÇí' ELSE CASE WHEN Q.store = 0.5 THEN N'êÛáõµ»ÏïÇí' ELSE N'Å/³ÝóÇ å³ï×³éáí' END END AS classification_reason,
										S.loan_full_number, ISNULL(LP.lp_number,0) AS lp_customer_number, S.date_of_beginning, Q.[app_id]
										FROM Tbl_store_new S 
										INNER JOIN [Tbl_customers;] C ON S.number = C.number
										INNER JOIN Q_ALL_LOANS Q ON S.loan_full_number = Q.loan_full_number AND S.date_of_beginning = Q.date_of_beginning 
										INNER JOIN Tbl_type_of_Risk R ON S.store = R.Store 
										LEFT JOIN tbl_LP_classifying LP ON S.number = LP.number
										WHERE S.number = @customer_number AND S.date_of_calculation>=@date
										ORDER BY date_of_calculation, Q.loan_full_number ";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Int).Value = leasingCustomerNumber;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = date;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            LeasingCustomerClassification customer = new LeasingCustomerClassification();
                            if (dr["date_of_calculation"] != DBNull.Value)
                            {
                                customer.ClassificationDate = Convert.ToDateTime(dr["date_of_calculation"]);
                            }
                            if (dr["RiskClassName"] != DBNull.Value)
                            {
                                customer.RiskClassName = dr["RiskClassName"].ToString();
                            }
                            if (dr["classification_reason"] != DBNull.Value)
                            {
                                customer.ClassificationType = dr["classification_reason"].ToString();
                            }
                            if (dr["loan_full_number"] != DBNull.Value)
                            {
                                customer.AccountOrLink = Convert.ToInt64(dr["loan_full_number"]);
                            }
                            if (dr["lp_customer_number"] != DBNull.Value)
                            {
                                customer.SubstitutePersonNumber = Convert.ToInt64(dr["lp_customer_number"]);
                            }
                            if (dr["date_of_beginning"] != DBNull.Value)
                            {
                                customer.DateOfBeginning = Convert.ToDateTime(dr["date_of_beginning"]);
                            }
                            if (dr["app_id"] != DBNull.Value)
                            {
                                customer.AppId = Convert.ToInt64(dr["app_id"]);
                            }

                            using (SqlCommand cmdLeasholders = new SqlCommand())
                            {
                                cmdLeasholders.CommandText = "SELECT co_leasing_number FROM [dbo].[tbl_co_leasholders] WHERE [app_Id] = @appId";
                                cmdLeasholders.Parameters.Add("@appId", SqlDbType.Float).Value = customer.AppId;
                                cmdLeasholders.Connection = conn;

                                SqlDataReader reader = cmdLeasholders.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (string.IsNullOrEmpty(customer.BorrowerNumber))
                                        {
                                            customer.BorrowerNumber += Convert.ToString(reader["co_leasing_number"]);
                                        }
                                        else
                                        {
                                            customer.BorrowerNumber += ", " + Convert.ToString(reader["co_leasing_number"]);
                                        }
                                    }
                                }
                            }

                            list.Add(customer);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Վերադարձնում է  ՀԱՃԱԽՈՐԴԻ ԴԱՍԱԿԱՐԳՄԱՆ ՊԱՏՄՈՒԹՅՈՒՆ աղյուսակի ընտրված տողի տվըալները 
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingCustomerClassificationHistoryByID(int id, long loanFullNumber, int lpNumber)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT S.id,S.date_of_calculation,R.RiskClassName_Unicode AS RiskClassName,
                                        CASE SB.classification_type WHEN 1 THEN N'Օբյեկտիվ' WHEN 2 THEN N'Սուբյեկտիվ' ELSE N'Փոխ. անձի պատճառով' END AS reason_name,
                                        SN.loan_full_number, ISNULL(LP.lp_number,0) AS lp_customer_number, SN.date_of_beginning
                                        FROM Tbl_store_sub S
                                        INNER JOIN Tbl_store_new SN ON S.number = SN.number
                                        LEFT JOIN tbl_LP_classifying LP ON S.number = LP.number
                                        INNER JOIN (SELECT RiskClassName_Unicode,RiskClassCode_num FROM tbl_type_of_Risk 
                                        GROUP BY RiskClassName_Unicode,RiskClassCode_num) R ON S.store_type=R.RiskClassCode_num  
                                        INNER JOIN Tbl_type_of_sub_store_basis SB ON S.basis_type = SB.id
                                        WHERE S.id= @id AND Lp.lp_number = @lp_number AND SN.loan_full_number = @loan_full_number
                                        GROUP BY S.id,S.date_of_calculation,R.RiskClassName_Unicode,
                                        SB.classification_type, SN.loan_full_number, LP.lp_number, SN.date_of_beginning";
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.Add("@lp_number", SqlDbType.Int).Value = lpNumber;
                    cmd.Parameters.Add("@loan_full_number", SqlDbType.Float).Value = loanFullNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["date_of_calculation"] != DBNull.Value)
                            {
                                customer.ClassificationDate = Convert.ToDateTime(dr["date_of_calculation"]);
                            }
                            if (dr["RiskClassName"] != DBNull.Value)
                            {
                                customer.RiskClassName = dr["RiskClassName"].ToString();
                            }
                            if (dr["reason_name"] != DBNull.Value)
                            {
                                customer.ClassificationType = dr["reason_name"].ToString();
                            }
                            if (dr["lp_customer_number"] != DBNull.Value && Convert.ToInt64(dr["lp_customer_number"]) > 0)
                            {
                                customer.SubstitutePersonNumber = Convert.ToInt64(dr["lp_customer_number"]);
                            }
                            if (dr["loan_full_number"] != DBNull.Value)
                            {
                                customer.AccountOrLink = Convert.ToInt64(dr["loan_full_number"]);
                            }
                            if (dr["date_of_beginning"] != DBNull.Value)
                            {
                                customer.DateOfBeginning = Convert.ToDateTime(dr["date_of_beginning"]);
                            }
                            using (SqlCommand cmdLeasholders = new SqlCommand())
                            {
                                cmdLeasholders.CommandText = "SELECT co_leasing_number FROM [dbo].[tbl_co_leasholders] WHERE loan_full_number = @loan AND date_of_beginning = @dateOfBeg";
                                cmdLeasholders.Parameters.Add("@loan", SqlDbType.Float).Value = customer.AccountOrLink;
                                cmdLeasholders.Parameters.Add("@dateOfBeg", SqlDbType.SmallDateTime).Value = customer.DateOfBeginning;
                                cmdLeasholders.Connection = conn;

                                SqlDataReader reader = cmdLeasholders.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (string.IsNullOrEmpty(customer.BorrowerNumber))
                                        {
                                            customer.BorrowerNumber += Convert.ToString(reader["co_leasing_number"]);
                                        }
                                        else
                                        {
                                            customer.BorrowerNumber += ", " + Convert.ToString(reader["co_leasing_number"]);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

            return customer;
        }

        /// <summary>
        /// Նախքան Report-ի  համար exel տպելը ստուգում է արդյոք հաճախորդների միջև կապ կա թե ոչ
        /// </summary>
        /// <param name="CustomerNumberN1"></param>
        /// <param name="CustomerNumberN2"></param>
        /// <returns></returns>
        internal static bool LeasingCustomerConnectionResult(long CustomerNumberN1, long CustomerNumberN2)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT 1 FROM [dbo].[Tbl_Customers_link_person_inf] WHERE number = @customer_number AND lpnumber = @lp_customer_number";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = CustomerNumberN1;
                    cmd.Parameters.Add("@lp_customer_number", SqlDbType.Float).Value = CustomerNumberN2;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///  Հաճախորդի վարկը անհուսալի դասով չդասակարգելու հիմքեր (աղյուսակ 5)
        /// </summary>
        /// <returns></returns>
        internal static List<LeasingCustomerClassification> GetLeasingGroundsForNotClassifyingCustomerLoan(long customerNumber, byte isActive)
        {
            List<LeasingCustomerClassification> list = new List<LeasingCustomerClassification>();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {


                    cmd.CommandText = String.Format(@"SELECT LC.Id,SL.[app_Id],SL.[loan_full_number],SL.[date_of_beginning],[report_number],
                                                    [report_date],[additional_description],[set_number],[registration_date] ,SL.number
                                                    FROM [dbo].[tbl_loan_not_classifying] LC
                                                    INNER JOIN [Tbl_short_time_loans;] SL 
                                                    ON LC.loan_full_number = SL.loan_full_number AND LC.date_of_beginning = SL.date_of_beginning
                                                    WHERE SL.number = @customer_number {0}
                                                    ORDER BY LC.[registration_date]", isActive == 1 ? " AND  LC.closed=0 " : "");
                    //registration_date
                    cmd.Parameters.Add("@customer_number", SqlDbType.Int).Value = customerNumber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            LeasingCustomerClassification customer = new LeasingCustomerClassification();
                            if (dr["Id"] != DBNull.Value)
                            {
                                customer.ClassificationID = Convert.ToInt32(dr["Id"]);
                            }
                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["additional_description"] != DBNull.Value)
                            {
                                customer.Description = dr["additional_description"].ToString();
                            }

                            if (dr["loan_full_number"] != DBNull.Value)
                            {
                                customer.Account = Convert.ToInt64(dr["loan_full_number"]);
                            }
                            if (dr["date_of_beginning"] != DBNull.Value)
                            {
                                customer.DateOfBeginning = Convert.ToDateTime(dr["date_of_beginning"]);
                            }
                            if (dr["app_Id"] != DBNull.Value)
                            {
                                customer.AppId = Convert.ToInt64(dr["app_Id"]);
                            }
                            list.Add(customer);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի վարկերի տվըալները
        /// </summary>
        /// <param name="custNamber"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> GetLeasingLoanInfo(int leasingCustNamber)
        {
            Dictionary<string, string> loansInfo = new Dictionary<string, string>();

            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT SL.app_id, SL.loan_full_number, TL.[description] AS loan_type_descr, SL.currency, SL.start_capital
                                        FROM [Tbl_short_time_loans;] SL
                                        INNER JOIN [Tbl_type_of_loans;] TL ON SL.loan_type = TL.code
                                        WHERE number = @custNamber";
                    cmd.Parameters.Add("@custNamber", SqlDbType.Int).Value = leasingCustNamber;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string info = dr["loan_full_number"].ToString() + " " + Utility.ConvertAnsiToUnicode(dr["loan_type_descr"].ToString()) + "  " + Convert.ToDouble(dr["start_capital"]).ToString() + "  " + dr["currency"].ToString();

                            string appId = dr["app_id"].ToString();
                            loansInfo.Add(key: appId, value: info);
                        }
                    }
                }
            }

            return loansInfo;
        }

        internal static int GetLeasingLoanQuality(long appId)
        {
            int quality = 0;
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT quality FROM [Tbl_short_time_loans;] WHERE app_id = @app_id";
                    cmd.Parameters.Add("@app_id", SqlDbType.BigInt).Value = appId;
                    cmd.Connection = conn;
                    conn.Open();
                    quality = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return quality;
        }

        internal static bool IsLeasingLoanActive(long appId)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;


            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT 1 FROM [dbo].[tbl_loan_not_classifying] LC 
										INNER JOIN  [Tbl_short_time_loans;] SL ON LC.loan_full_number = SL.loan_full_number AND LC.date_of_beginning = SL.date_of_beginning
                                        WHERE LC.closed=0 AND SL.app_id = @app_id";
                    cmd.Parameters.Add("@app_id", SqlDbType.BigInt).Value = appId;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = true;
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        internal static ActionResult AddLeasingGroundsForNotClassifyingCustomerLoan(LeasingCustomerClassification obj, DateTime registrationDate, int setNumber)
        {
            ActionResult actionResult = new ActionResult();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT loan_full_number, date_of_beginning FROM [Tbl_short_time_loans;] WHERE app_id = @appId";
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = obj.AppId;

                    cmd.Connection = conn;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            using (SqlCommand cmd1 = new SqlCommand())
                            {
                                cmd1.CommandText = @"INSERT INTO [dbo].[tbl_loan_not_classifying]([app_Id],[loan_full_number],[date_of_beginning],
                                        [report_number],[report_date],[additional_description],[set_number],[registration_date])
                                        VALUES(@appId,@loan_full_number,@date_of_beginning,@report_number,@report_date,@reason_add_text,@set_number,@set_date)";

                                cmd1.Parameters.Add("@appId", SqlDbType.Float).Value = obj.AppId;
                                cmd1.Parameters.Add("@loan_full_number", SqlDbType.Float).Value = Convert.ToDouble(reader["loan_full_number"]);
                                cmd1.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = Convert.ToDateTime(reader["date_of_beginning"]);
                                cmd1.Parameters.Add("@report_number", SqlDbType.NVarChar, 100).Value = obj.ReportNumber;
                                cmd1.Parameters.Add("@report_date", SqlDbType.SmallDateTime).Value = obj.ReportDate;
                                cmd1.Parameters.Add("@reason_add_text", SqlDbType.NVarChar, 1000).Value = obj.AdditionalDescription;
                                cmd1.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;
                                cmd1.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = registrationDate;

                                cmd1.Connection = conn;
                                result = cmd1.ExecuteNonQuery();
                            }
                        }
                    }


                }
            }
            if (result == 1)
            {
                actionResult.ResultCode = ResultCode.Normal;
                //actionResult.Message = "";
            }

            return actionResult;


        }


        /// <summary>
        /// Վերադարձնում է Հաճախորդի վարկը անհուսալի դասով չդասակարգելու հիմքեր աղյուսակի ընտրված տողի տվըալները 
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingGroundsForNotClassifyingCustomerLoanByID(int id)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT  [report_number]
			                                   ,[report_date]
			                                   ,[additional_description]
			                                   ,[closed]
			                                   ,[closing_report_number]
			                                   ,[closing_report_date]
			                                   ,[closing_set_number]
			                                   ,[closing_date]
			                                   ,[set_number]
			                                   ,[registration_date]
                                        FROM [dbo].[tbl_loan_not_classifying] WHERE Id = @id";
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (dr["registration_date"] != DBNull.Value)
                            {
                                customer.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            }
                            if (dr["set_number"] != DBNull.Value)
                            {
                                customer.SetNumber = Convert.ToInt32(dr["set_number"]);
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = (dr["report_number"]).ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (dr["additional_description"] != DBNull.Value)
                            {
                                customer.Description = (dr["additional_description"]).ToString();
                            }
                            if (dr["closed"] != DBNull.Value)
                            {
                                customer.Status = Convert.ToInt16(dr["closed"]) == 0 ? "Գործող" : "Փակված";
                            }
                            if (dr["closing_set_number"] != DBNull.Value)
                            {
                                customer.ClosedSetNumber = Convert.ToInt32(dr["closing_set_number"]);
                            }
                            if (dr["closing_date"] != DBNull.Value)
                            {
                                customer.ClosingDateString = Convert.ToDateTime(dr["closing_date"]).ToString("dd/MM/yyyy");
                            }
                            if (dr["closing_report_number"] != DBNull.Value)
                            {
                                customer.ClosingReporNumber = (dr["closing_report_number"]).ToString();
                            }
                            if (dr["closing_report_date"] != DBNull.Value)
                            {
                                customer.ClosingReportDate = Convert.ToDateTime(dr["closing_report_date"]);
                            }
                        }
                    }
                }
            }

            return customer;
        }

        internal static ActionResult CloseLeasingGroundsForNotClassifyingCustomerLoan(long appId, int id, int userId, DateTime closeDate, string docNumber, DateTime docDate)
        {
            ActionResult actionResult = new ActionResult();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @" UPDATE tbl_loan_not_classifying SET closed=1
	                                                                         ,closing_set_number=@set_number 
																			 ,closing_date=@set_date
																			 ,closing_report_number=@report_number 
																			 ,closing_report_date=@txt_report_date
                                                             WHERE Id= @ID";

                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = closeDate;
                    cmd.Parameters.Add("@report_number", SqlDbType.NVarChar, 100).Value = docNumber;
                    cmd.Parameters.Add("@txt_report_date", SqlDbType.SmallDateTime).Value = docDate;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

                    cmd.Connection = conn;
                    conn.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            if (result == 1)
            {
                actionResult.ResultCode = ResultCode.Normal;
                //actionResult.Message = "Հաջողությամբ փակվել է:";
            }

            return actionResult;
        }

        internal static bool IsReportActive(int id)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            bool result = false;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT closed FROM tbl_loan_not_classifying WHERE Id=@ID";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (Convert.ToInt16(dr["closed"]) == 1)
                            {
                                result = true;
                                return result;
                            }

                        }
                    }
                }
            }

            return result;
        }

        internal static long GetReportAppId(int id)
        {
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            long appId = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT SL.app_id
                                        FROM [dbo].[tbl_loan_not_classifying] LC 
                                        INNER JOIN [Tbl_short_time_loans;] SL ON LC.[loan_full_number] = SL.loan_full_number AND LC.[date_of_beginning] = SL.date_of_beginning
                                        WHERE LC.Id = @ID";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    appId = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }

            return appId;
        }

        /// <summary>
        /// Ավելացնում է հաճախորդ (հաճախորդի սուբյեկտիվ դասակարգման հիմքեր աղյուսակում)
        /// </summary>
        /// <param name="registrationDate"></param>
        /// <param name="customerNumber"></param>
        /// <param name="setNumber"></param>
        /// <param name="docNumber"></param>
        /// <param name="docDate"></param>
        /// <param name="reasonId"></param>
        /// <param name="reasonType"></param>
        /// <param name="reasonAddText"></param>
        /// <param name="writtenOff"></param>
        /// <param name="reasonDate"></param>
        /// <param name="overdueDays"></param>
        /// <param name="classCalcByDays"></param>
        /// <returns></returns>
        internal static ActionResult EditLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj, int overdueDays, DateTime registrationDate, int setNumber)
        {
            ActionResult actionResult = new ActionResult();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            int result = 0;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"UPDATE [dbo].[Tbl_store_sub]
                                       SET [date_of_calculation] = @reason_date
                                          ,[store_type] = @written_off
                                          ,[class_calc_by_days] = @class_calc_by_days
                                          ,[report_number] = @document_number
                                          ,[report_date] = @document_date
                                          ,[additional_description] = @reason_add_text
                                          ,[set_number] = @set_number
                                          ,[registration_date] = @registration_date
                                          ,[overdue_days] = @overdue_days
                                     WHERE id=@id";

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = obj.Id;
                    cmd.Parameters.Add("@reason_date", SqlDbType.SmallDateTime).Value = obj.ClassificationDate;
                    cmd.Parameters.Add("@written_off", SqlDbType.SmallInt).Value = obj.RiskClassName;
                    if (string.IsNullOrEmpty(obj.ReportNumber))
                    {
                        cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 100).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 100).Value = obj.ReportNumber;
                    }
                    if (string.IsNullOrEmpty(obj.ReportDate.ToString()) || obj.ReportDate.Year < 1990)
                    {
                        cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = obj.ReportDate;
                    }
                    if (string.IsNullOrEmpty(obj.AdditionalDescription))
                    {
                        cmd.Parameters.Add("@reason_add_text", SqlDbType.NVarChar, 200).Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters.Add("@reason_add_text", SqlDbType.NVarChar, 200).Value = obj.AdditionalDescription;
                    }

                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = setNumber;
                    cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = registrationDate;
                    cmd.Parameters.Add("@class_calc_by_days", SqlDbType.Bit).Value = obj.CalcByDays;
                    cmd.Parameters.Add("@overdue_days", SqlDbType.Int).Value = Convert.ToInt16(overdueDays);
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.Connection = conn;
                    result = cmd.ExecuteNonQuery();
                }
            }
            if (result == 1)
            {
                actionResult.ResultCode = ResultCode.Normal;
                //actionResult.= "Մուտքագրումը հաջողությամբ կատարվել է:";
            }
            return actionResult;
        }

        /// <summary>
        /// Վերադարձնում է  ՀաՃախորդի սուբյեկտիվ դասակարգման հիմքեր աղյուսակի ընտրված տողի տվյալները
        /// </summary>
        /// <param name="id">ընտրված տողի ID-ն</param>
        /// <returns></returns>
        internal static LeasingCustomerClassification GetLeasingSubjectiveClassificationGroundsByIDForEdit(int id)
        {
            LeasingCustomerClassification customer = new LeasingCustomerClassification();
            string config = ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(config))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT store_type, basis_type, [report_number],[report_date],[additional_description],
                                        date_of_calculation,[class_calc_by_days],IIf(basis_type in (6,7,8,9,10), 1, 2) AS Reason_Type
                                        FROM Tbl_store_sub where id=@id";

                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["store_type"] != DBNull.Value)
                            {
                                customer.RiskClassName = dr["store_type"].ToString();
                            }
                            if (dr["basis_type"] != DBNull.Value)
                            {
                                customer.Description = dr["basis_type"].ToString();
                            }
                            if (dr["Reason_Type"] != DBNull.Value)
                            {
                                customer.ClassificationType = dr["Reason_Type"].ToString();
                            }
                            if (dr["report_number"] != DBNull.Value)
                            {
                                customer.ReportNumber = dr["report_number"].ToString();
                            }
                            if (dr["report_date"] != DBNull.Value)
                            {
                                customer.ReportDate = Convert.ToDateTime(dr["report_date"]);
                            }
                            if (!string.IsNullOrEmpty(dr["additional_description"].ToString()))
                            {
                                customer.AdditionalDescription = dr["additional_description"].ToString();
                            }
                            else
                            {
                                customer.AdditionalDescription = string.Empty;
                            }
                            if (dr["date_of_calculation"] != DBNull.Value)
                            {
                                customer.ClassificationDate = Convert.ToDateTime(dr["date_of_calculation"]);
                            }

                            if (dr["class_calc_by_days"] != DBNull.Value)
                            {
                                customer.CalcByDays = Convert.ToBoolean(dr["class_calc_by_days"]);
                            }

                        }
                    }
                }
            }

            return customer;
        }

    }
}
