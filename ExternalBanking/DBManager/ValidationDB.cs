using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Collections.Generic;
using ExternalBanking.ACBAServiceReference;


namespace ExternalBanking.DBManager
{
    public static class ValidationDB
    {


        public static int CheckProductAvailabilityByCustomerCountry(ulong customerNumber, int productType)
        {
            int result = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd =new SqlCommand(@"select dbo.Fnc_Countries_For_checking_By_Customer(@customerNumber,@productType) as result",conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@productType", SqlDbType.Int).Value = productType;

                    result = Convert.ToInt16(cmd.ExecuteScalar());

                    return result;
                }
            }
        }
        /// <summary>
        /// Գործարքի դեպքում կրեդիտ և դեբետ հաշիվների ստուգում
        /// </summary>
        /// <param name="debitAccountNumber"></param>
        /// <param name="creditAccountNumber"></param>
        /// <param name="permissionID">մուտքագրողի թույլատրության համար</param>
        /// <returns></returns>
        public static ActionError CheckAccountOperation(string debitAccountNumber, string creditAccountNumber, int permissionID, double operationAmount)
        {
            ActionError error = new ActionError();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Sp_CheckAccOperation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@debit_account", SqlDbType.Float).Value = debitAccountNumber;
                    cmd.Parameters.Add("@credit_account", SqlDbType.Float).Value = creditAccountNumber;
                    cmd.Parameters.Add("@PermissionID", SqlDbType.Int).Value = permissionID;
                    cmd.Parameters.Add("@flag_for_prix_ord", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@operation_group", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@utility_accounts_permission", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@ENA_accounts_permission", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@soc_purpose", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = 1;
                    cmd.Parameters.Add("@operation_amount", SqlDbType.Float).Value = operationAmount;
                    string errCode = cmd.ExecuteScalar().ToString();
                    if (errCode != "0")
                    {
                        error.Code = Convert.ToInt16(errCode);
                    }

                }
            }
            return error;


        }

        public static bool IsDAHKAvailability(ulong customerNumber)
        {                   
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select dbo.fnc_check_DAHK_availability(@customerNumber)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return !Convert.ToBoolean(dr[0]);
                        else return false;
                    }
                }

            }
        }

        public static bool CheckAccCashPos(string accountNumber, int posStatus = 1)
        {
            bool check = false;
            if (accountNumber != "0")
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    conn.Open();

                    string sql = "";

                    switch (posStatus)
                    {
                        case 1:
                            sql = "Select account_number,account_number_usd, account_number_eur  from tbl_arca_points_list where (account_number=@accountNumber or account_number_usd=@accountNumber or account_number_eur=@accountNumber) and type_of_point in(2,5) and point_place=1 and closing_date is null";
                            break;
                        case 0:
                            sql = "Select account_number,account_number_usd, account_number_eur  from tbl_arca_points_list where (account_number=@accountNumber or account_number_usd=@accountNumber or account_number_eur=@accountNumber) and type_of_point in(2,5) and point_place=1 and not closing_date is null";
                            break;
                        case 2:
                            sql = "Select account_number,account_number_usd, account_number_eur  from tbl_arca_points_list where (account_number=@accountNumber or account_number_usd=@accountNumber or account_number_eur=@accountNumber) and type_of_point in(2,5) and point_place=1";
                            break;
                        default:
                            break;
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                                check = true;
                            else check = false;
                        }
                    }

                }
            }
            return check;
        }

        public static void ValidateCashOperationAvailability(Order order, string debitCredit,string currency, User user)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_check_cash_operations_availability_and_limit_excessed";
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = user.filialCode;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@cur", SqlDbType.VarChar, 3).Value = currency;
                    cmd.Parameters.Add("@debCred", SqlDbType.VarChar, 1).Value = debitCredit;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;
                    cmd.ExecuteScalar();
                }
            }

        }

        public static void ValidateCashOperationAvailability(AccountReOpenOrder order, string debitCredit, string currency, User user)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_check_cash_operations_availability_and_limit_excessed";

                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = user.filialCode;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@cur", SqlDbType.VarChar, 3).Value = currency;
                    cmd.Parameters.Add("@debCred", SqlDbType.VarChar, 1).Value = debitCredit;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Fees.Find(m => m.Type == 12).Amount;
                    cmd.ExecuteScalar();
                }
            }

        }

        /// <summary>
        /// Ստացողի հաշվի համար ստուգում է անհրաժեշտ է տվյալ հաշվի համար ստուգել ստորագրության նմուշի առկայություն
        /// </summary>
        /// <param name="typeOfAccountNew"></param>
        /// <param name="checkGroup">checkGroup-ը 1 ի դեպքում ստուգում է ստորագրության նմուշի համար</param>
        /// <returns></returns>
        public static bool IsRequiredCheckBySintAccNew(string accountNumber, short checkGroup)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"declare @SintAccNew as varchar(10)
                                                  select @SintAccNew=type_of_account_new from [tbl_all_accounts;] where Arm_number=@accountNumber
                                                  select dbo.Fnc_RequiredCheckBySintAccNew(@SintAccNew,@CheckGroup) as Result
                                                ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@CheckGroup", SqlDbType.SmallInt).Value = checkGroup;
                    short result = Convert.ToInt16(cmd.ExecuteScalar());
                    if (result == 1)
                        return true;
                    else
                        return false;
                }


            }
        }

        /// <summary>
        /// Գործառնական օրվա կարգավիճակ
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static bool CheckOpDayClosingStatus(int filialCode)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select dbo.fn_check_op_day_closing_status(@filialCode)", conn))
                {
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;

                    return (bool)cmd.ExecuteScalar();
                }


            }
        }
        public static List<int> CheckReestrPaymentOrderReciverName(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails)
        {
            string comparisonString = "";
            List<int> nonValidateRows = new List<int>();
            DataTable dt = new DataTable();
            foreach (ReestrTransferAdditionalDetails details in reestrTransferAdditionalDetails)
            {
                string creditAccountAccountDescription = Account.GetAccountDescription(details.CreditAccount.AccountNumber);
                comparisonString = comparisonString + details.Index.ToString() + "," + details.Description + "," + creditAccountAccountDescription + "#";

            }
            comparisonString = comparisonString.Substring(0, comparisonString.Length - 1);

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_bulk_compare_string_permutations";

                    cmd.Parameters.Add("@valueArray", SqlDbType.NVarChar).Value = comparisonString;
                    cmd.Parameters.Add("@delimiter", SqlDbType.Char, 1).Value = ' ';
                    cmd.Parameters.Add("@replaceCharacterSet", SqlDbType.NVarChar, 250).Value = @"> < § ¦ - _ . / \ ";
                    cmd.Parameters.Add("@detectLegalEntities", SqlDbType.TinyInt).Value = 1;
                    dt.Load(cmd.ExecuteReader());
                    DataRow[] dr = dt.Select("comparison_result<>1");
                    for (int i = 0; i < dr.Length; i++)
                    {
                        nonValidateRows.Add(Convert.ToInt32(dr[i]["ID"]));
                    }
                }
            }
            return nonValidateRows;
        }


        public static ActionError CheckForDepositAccountDebet(Account debitAccount, double amount, SourceType source, string creditAccountNumber = "0")
        {
            ActionError actionError = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_check_for_deposit_account_debet";

                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = debitAccount.AccountNumber;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = creditAccountNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = amount;
                    cmd.Parameters.Add("@sourceType", SqlDbType.Money).Value = (ushort)source;
                    cmd.Parameters.Add(new SqlParameter("@errorID", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    short errorCode = Convert.ToInt16(cmd.Parameters["@errorID"].Value);
                    if (errorCode != 0)
                    {
                        actionError = new ActionError(errorCode);
                    }
                }
            }
            return actionError;

        }



        /// <summary>
        /// Ստուգում է օրվա փակմնա ժամանակ FrontOffice-ի գործարքների check դնելը
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static bool FrontOfficeAllowTransaction(int filialCode)
        {
            bool check = true;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(isChecked,0) AS isChecked FROM Tbl_op_day_closing_checklist
                                                  WHERE oper_day=@operDay AND checkingID=CASE WHEN @filialCode=22000 THEN 166 ELSE 144  END AND filial=@filialCode​ ", conn))
                {
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            check = Convert.ToBoolean(dr["isChecked"]);
                        }
                        else
                        {
                            check = true;
                        }
                    }
                }

                  


                return check;
            }
        }

        public static bool HasOverdueLoan(Account debitAccount, short strictOverdueLoan, short notStrictDebtType)
        {
            bool hasOverdueLoan = false;
            ActionError actionError = new ActionError();
            try
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "pr_DebetAccountValidation";

                        cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = debitAccount.AccountNumber;
                        cmd.Parameters.Add("@strictOverdueLoan", SqlDbType.TinyInt).Value = strictOverdueLoan;
                        cmd.Parameters.Add("@notStrictDebtType", SqlDbType.TinyInt).Value = notStrictDebtType;
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch
            {
                hasOverdueLoan = true;
                return hasOverdueLoan;
            }

            return hasOverdueLoan;

        }

        public static bool IsBankOpen(double bankCode)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select 1 from [Tbl_banks;] where closing_date is null and code=@code", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@code", SqlDbType.Float).Value = bankCode;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return Convert.ToBoolean(dr[0]);
                        else return false;
                    }
                }

            }
        }

        public static List<ActionError> CheckForRegisterRequestData(double applicationID, double customerNumber, int requestType, SourceType sourceType)
        {
            List<ActionError> result = new List<ActionError>();


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_check_for_register_request_data";

                    cmd.Parameters.Add("@applicationID", SqlDbType.Float).Value = applicationID;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@requestType", SqlDbType.Int).Value = requestType;
                    cmd.Parameters.Add("@bankingSource", SqlDbType.SmallInt).Value = sourceType;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        if (row != null)
                        {
                            ActionError actionError = new ActionError(short.Parse(row["error_id"].ToString()));
                            actionError.Params = new string[] { Utility.ConvertAnsiToUnicode(row["var1"].ToString()), Utility.ConvertAnsiToUnicode(row["var2"].ToString()) };
                            result.Add(actionError);
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Նույն SWIFT հաղորդագրության այլ հաստատման առկայության ստուգում
        /// </summary>
        internal static bool IsSwiftMessageConfirmed(int SwiftMessageID)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_HB_documents HB JOIN [Tbl_HB_document_link_identity] LI ON LI.doc_id=HB.Doc_id
                                                          WHERE  quality in (3,30) and LI.link_ID =@swiftMessageID ", conn))
                {
                    cmd.Parameters.Add("@swiftMessageID", SqlDbType.Int).Value = SwiftMessageID;
                    if (cmd.ExecuteReader().Read())
                    {
                        check = true;
                    }
                }
            }

            return check;
        }

        internal static bool IsLoanProductActive(string AccountNumber)
        {

            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"	SELECT 1 from V_CURRENT_LOANS_CRLINES_GUARANTEES_FACTORINGS
	                                                        where App_Id = (SELECT application_id FROM tbl_credit_codes CC  
                                                            INNER JOIN Tbl_Products_Accounts_Groups PG ON CC.application_id=PG.App_ID
                                                            INNER JOIN Tbl_Products_Accounts PA ON PG.Group_ID=PA.Group_Id
                                                            WHERE PG.group_status = 1 and Account_number = @accountNumber)", conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.VarChar).Value = AccountNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }

                return result;
            }
        }

        internal static bool CheckFor24_7Mode(Order order)
        {
            MakingTransactionIn24_7ModeAllowbility allowbility = MakingTransactionIn24_7ModeAllowbility.NotAllowed;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                string str = @"SELECT [mode_24_7] FROM Tbl_sub_types_of_HB_products WHERE document_type = @type";
                if (order.SubType != 0)
                {
                    str = str + @" AND document_sub_type = @subtype";
                }
                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    cmd.Parameters.Add("@type", SqlDbType.Int).Value = order.Type;
                    if (order.SubType != 0)
                    {
                        cmd.Parameters.Add("@subtype", SqlDbType.Int).Value = order.SubType;
                    }
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            allowbility = (MakingTransactionIn24_7ModeAllowbility)ushort.Parse(dr["mode_24_7"].ToString());
                    }
                }

                if (allowbility == MakingTransactionIn24_7ModeAllowbility.Allowed)
                {
                    return true;
                }
                else if (allowbility == MakingTransactionIn24_7ModeAllowbility.ConditionallyAllowed)
                {
                    return order.CheckConditionForMakingTransactionIn24_7Mode();
                }
                else
                {
                    return false;
                }
            }
        }

        internal static bool ValidateDocumentNumber(ulong customerNumber, DateTime registrationDate, int orderId, string orderNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select dbo.fnc_validate_document_number(@customerNumber, @registrationDate, @orderId, @orderNumber)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = registrationDate;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@orderNumber", SqlDbType.NVarChar).Value = orderNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return Convert.ToBoolean(dr[0]);
                        else return false;
                    }
                }

            }
        }

        internal static Tuple<int, int> Check24_7ModeForHB()
        {
            int operDayStatus = 0;
            int mode24_7 = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string str = @"select opday_status,mode_24_7 from Tbl_current_oper_day";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            operDayStatus = Convert.ToInt32(dr["opday_status"]);
                            mode24_7 = Convert.ToInt32(dr["mode_24_7"]);
                        }
                    }
                }

            }

            return Tuple.Create(operDayStatus, mode24_7);
        }





        internal static bool IsExistAttachedCard(ulong customerNumber, string cardNumber)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select top 1 1 from tbl_other_bank_cards where card_number = @cardNumber and customer_number = @customerNumber and is_completed = 1", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }
                return result;
            }
        }

        internal static bool IsAttachedCardBusiness(string cardNumber)
        {
            bool result = false;
            string bin = cardNumber.TrimStart().Substring(0, 6);
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM (SELECT CASE  WHEN master_card_business IS NULL THEN visa_business ELSE  master_card_business end as bin from tbl_attached_card_business_bin
                                                                                                UNION SELECT CASE  WHEN visa_business is null then master_card_business else  visa_business end as bin from  tbl_attached_card_business_bin ) a
                                                                                                WHERE a.bin = @bin", conn))
                {
                    cmd.Parameters.Add("@bin", SqlDbType.NVarChar).Value = bin;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }
                return result;
            }
        }

        internal static bool CheckBudgetCodeExistance(string AccountNumber, short TypeOfClient)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select code from tbl_budget_accounts where code = @account and case @cust_type 
																	when 2 
																		then IS_ENTREPRENEUR 
																	when 6 
																		then IS_PHYSICAL 
																	else IS_LEGAL 
															   end  = 'true'", conn))
                {
                    cmd.Parameters.Add("@account", SqlDbType.NVarChar).Value = AccountNumber;
                    cmd.Parameters.Add("@cust_type", SqlDbType.NVarChar).Value = TypeOfClient;

                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }
                return result;
            }
        }

        internal static bool CheckBudjetRegCode(long docID)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select budj_transfer_reg_code from tbl_hb_documents where budj_transfer_reg_code = 95 and  doc_id = @doc_id", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.BigInt).Value = docID;

                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }
                return result;
            }
        }
        public static bool IsCustomerConnectedToOurBank(ulong customerNumber)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select 1 from Tbl_customers where customer_number = @customerNumber and link in (1,2,3)", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        result = true;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Գործառնական օրվա կարգավիճակը բանկում
        /// </summary>
        /// <returns></returns>
        public static bool BankOpDayIsClosed()
        {
            bool isClosed = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT opday_status FROM Tbl_current_oper_day", conn))
                {
                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        isClosed = Convert.ToInt16(temp) != 1;
                    }
                    return isClosed;
                }
            }
        }

        public static bool CheckLoanDelete(ulong appId)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select [dbo].[Fnc_check_loan_delete](@appId) result", conn))
                {
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = appId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = Convert.ToBoolean(dr["result"]);
                        }
                    }
                }
                return result;
            }
        }
    }
}
