using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Dynamic;
using System.Globalization;
using System.Web.Configuration;
using ExternalBanking.Helpers;

namespace ExternalBanking.DBManager
{
    static class AccountDB
    {
        private static string accountSelectScript = @"SELECT 
                                                        a.open_date
                                                        ,type_of_product
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,t.description
                                                        ,a.closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as ac_description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                        ,t.DescriptionEng
                                                    FROM [tbl_all_accounts;] a ";

        private static string aparikAccountSelectScript = @"SELECT 
                                                            a.open_date
                                                            ,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product
							                                ,a.Arm_number
							                                ,a.type_of_account_new
							                                ,a.balance 
							                                ,a.Currency
							                                ,t.description+'('+ convert(nvarchar,p.security_code_2)+')' as description
							                                ,closing_date
                                                            ,a.card_number
                                                            ,a.filialcode
                                                            ,a.description as ac_description
                                                            ,a.account_type
                                                            ,a.freeze_date
                                                            ,a.UnUsed_amount
                                                            ,a.UnUsed_amount_date
                                                            ,a.account_access_group
                                                            ,t.DescriptionEng
                                                        FROM
                                                        Tbl_paid_factoring p
                                                        INNER JOIN
                                                        Tbl_Products_Accounts_Groups g ON p.App_Id = g.App_ID
                                                        INNER JOIN
                                                        Tbl_Products_Accounts pa ON g.Group_ID = pa.group_id
                                                        INNER JOIN
                                                        [tbl_all_accounts;] a ON pa.Account_number = a.Arm_number
                                                        INNER JOIN
                                                        Tbl_type_of_products t ON CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END = t.code
                                                        WHERE p.customer_number = @customerNumber 
                                                        AND  p.loan_type in(33,38)  AND quality <> 10 
                                                        AND pa.Type_of_account in (24,179) ";

        private static string closedAparikAccountSelectScript = @" SELECT 
                                                                        a.open_date
                                                                        ,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product
							                                            ,a.Arm_number
							                                            ,a.type_of_account_new
							                                            ,a.balance 
							                                            ,a.Currency
							                                            ,t.description+'('+ convert(nvarchar,p.security_code_2)+')' as description
							                                            ,closing_date
                                                                        ,a.card_number
                                                                        ,a.filialcode
                                                                        ,a.description as ac_description
                                                                        ,a.account_type
                                                                        ,a.freeze_date
                                                                        ,a.UnUsed_amount
                                                                        ,a.UnUsed_amount_date
                                                                        ,a.account_access_group
                                                                        ,t.DescriptionEng
                                                                        FROM
                                                                        Tbl_closed_paid_factoring p
                                                                        LEFT JOIN
                                                                        Tbl_Products_Accounts_Groups g ON p.App_Id = g.App_ID
                                                                        INNER JOIN
                                                                        Tbl_Products_Accounts pa ON g.Group_ID = pa.group_id
                                                                        INNER JOIN
                                                                        [tbl_all_accounts;] a ON pa.Account_number = a.Arm_number
                                                                        INNER JOIN
                                                                        Tbl_type_of_products t ON CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END = t.code
                                                                        WHERE a.customer_number =  @customerNumber 
																		 and a.Arm_number not in (select gr.Account_number from Tbl_Products_Accounts gr inner join Tbl_Products_Accounts_Groups g on gr.Group_Id=g.Group_ID where g.Group_Status = 1 and gr.Account_number=a.Arm_number ) 
                                                                        AND  p.loan_type in(33,38) AND quality <> 10 
                                                                        AND pa.Type_of_account in (24,179) AND closing_date is null ";



        private static string systemAccountSelectScript = @"SELECT 
                                                        a.open_date
                                                        ,0 as type_of_product
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,'' as description
                                                        ,a.closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as ac_description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                    FROM [tbl_all_accounts;] a ";
        private static string systemAccountSelectScriptExtended = @"SELECT 
                                                        a.open_date
                                                        ,0 as type_of_product
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,'' as description
                                                        ,a.closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as ac_description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                        ,p.type_of_Account
                                                    FROM [tbl_all_accounts;] a ";
        private static string transitAccountsForDebitTransactionsSelectScript = @"
                                                        SELECT 
                                                        a.open_date
                                                        ,21 as type_of_product
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,a.closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                        ,tr.description as ac_description
                                                        FROM [tbl_all_accounts;] a
                                                        INNER JOIN
										                Tbl_transit_accounts_for_debit_transactions tr
                                                        ON a.Arm_number = tr.arm_number";

        private static string onlyAllAccountSelectScript = @"SELECT 
                                                        a.open_date
                                                        ,a.Arm_number
                                                        ,a.type_of_account_new
                                                        ,a.balance
                                                        ,a.Currency
                                                        ,a.closing_date
                                                        ,a.card_number
                                                        ,a.filialcode
                                                        ,a.description as ac_description 
                                                        ,a.account_type
                                                        ,a.freeze_date
                                                        ,a.UnUsed_amount
                                                        ,a.UnUsed_amount_date
                                                        ,a.account_access_group
                                                    FROM [tbl_all_accounts;] a ";


        internal static Account GetAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE Arm_number = @accountNumber and customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }
                return account;
            }


        }


        internal static Account GetSystemAccount(string accountNumber)
        {
            Account account = new Account(accountNumber);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(systemAccountSelectScriptExtended + " LEFT JOIN Tbl_Products_Accounts p ON p.account_number = a.arm_number WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

                return account;
            }


        }


        internal static Account GetSystemAccountWithoutBallance(string accountNumber)
        {
            Account account = new Account(accountNumber);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(systemAccountSelectScript + " WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccountWithoutBallance(row);
                    }
                }

                return account;
            }


        }


        internal static Account GetAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"  SELECT distinct a.open_date
                                                            ,s.type_of_product
                                                            ,a.Arm_number
                                                            ,a.type_of_account_new
                                                            ,a.balance
                                                            ,a.Currency
                                                            ,t.description
                                                            ,a.closing_date
                                                            ,a.card_number
                                                            ,a.filialcode
                                                            ,a.description as ac_description 
                                                            ,a.account_type
                                                            ,a.freeze_date
                                                            ,a.UnUsed_amount
                                                            ,a.UnUsed_amount_date
                                                            ,a.account_access_group
                                                            ,s.type_of_Account
                                                            ,t.DescriptionEng          
                                                            FROM [tbl_all_accounts;] a INNER JOIN
                                                            (SELECT sint_acc_new, type_of_client, type_of_product,type_of_Account FROM  dbo.Tbl_define_sint_acc
                                                            GROUP BY sint_acc_new, type_of_client, type_of_product,type_of_Account)s
                                                            ON a.type_of_account_new = s.sint_acc_new INNER JOIN
                                                            Tbl_type_of_products t ON type_of_product = t.code 
                                                            left JOIN Tbl_Products_Accounts PAcc ON PAcc.account_number = a.arm_number
                                                            WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 1)
                    {
                        account = GetCurrentAccount(accountNumber);
                        if (account == null)
                            account = GetCardDahkAccount(accountNumber);
                        if (account == null)
                            account = GetAparikTexumAccount(accountNumber);
                        if (account == null)
                            account = GetClosedAparikTexumAccount(accountNumber);
                        if (account == null)
                            account = GetDahkTransitAccount(accountNumber);

                        if (account != null)
                        {
                            return account;
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

                return account;
            }


        }

        internal static async Task<Account> GetAccountAsync(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 1)
                    {
                        account = await GetCurrentAccountAsync(accountNumber);
                        if (account == null)
                            account = await GetAparikTexumAccountAsync(accountNumber);
                        if (account == null)
                            account = await GetClosedAparikTexumAccountAsync(accountNumber);

                        if (account != null)
                        {
                            return account;
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = await SetAccountAsync(row);
                    }
                }

                return account;
            }


        }

        public static ulong GetAccountCustomerNumber(string accountNumber)
        {
            ulong customerNumber = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT customer_number FROM [tbl_all_accounts;] WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            customerNumber = Convert.ToUInt64(dr["customer_number"]);

                        }
                    }
                }
            }
            return customerNumber;
        }

        internal static List<Account> GetAccounts(ulong customerNumber)
        {
            List<Account> accountList = GetAccountsAsync(customerNumber).Result;
            return accountList;
        }

        internal static async Task<List<Account>> GetAccountsAsync(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                Task<List<Account>> accounts1 = GetCurrentAccountList(customerNumber, conn);
                Task<List<Account>> accounts2 = GetDAHKAccountList(customerNumber, conn);
                Task<List<Account>> accounts3 = GetSocialAccountList(customerNumber, conn);
                Task<List<Account>> accounts4 = GetCardAccountList(customerNumber, conn);
                Task<List<Account>> accounts5 = GetDepositAccountList(customerNumber, conn);
                Task<List<Account>> accounts6 = GetJointDepositAccountList(customerNumber, conn);
                Task<List<Account>> accounts7 = GetAparikAccountList(customerNumber, conn);
                Task<List<Account>> accounts8 = GetJointAccountList(customerNumber, conn);
                Task<List<Account>> accounts9 = GetCardDAHKAccountList(customerNumber, conn);
                Task<List<Account>> accounts10 = GetBankruptAccountList(customerNumber, conn);
                Task<List<Account>> accounts11 = GetCurrentAccountListForNonMobileClients(customerNumber, conn);   //Սահմանափակ հասանելիությամ հաշվիներ
                Task<List<Account>> accounts12 = GetSocialSecurityAccountList(customerNumber, conn);   //Սոցիալական ապահովության հաշիվ
                Task<List<Account>> accounts14 = GetDeveloperSpecialAccountsAsync(customerNumber, conn);   //Կառուցապատողի հատուկ հաշիվներ

                Task<List<Account>> accounts13 = GetDahkTransitAccountList(customerNumber, conn);
                
                accountList.AddRange(await accounts1);

                accountList.AddRange(await accounts2);

                accountList.AddRange(await accounts3);

                accountList.AddRange(await accounts4);

                accountList.AddRange(await accounts5);

                accountList.AddRange(await accounts6);

                accountList.AddRange(await accounts7);

                accountList.AddRange(await accounts8);

                accountList.AddRange(await accounts9);

                accountList.AddRange(await accounts10);

                accountList.AddRange(await accounts11);

                accountList.AddRange(await accounts12);

                accountList.AddRange(await accounts13);

                accountList.AddRange(await accounts14);
                accountList.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null)
                        m.AccountDescription = card.CardNumber + " " + card.CardType;

                });
            }
            return accountList;
        }



        internal static List<Account> GetClosedAccounts(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(AccountDB.GetCurrentClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetDAHKClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetSocialClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetCardClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetDepositClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetAparikClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetJointClosedAccountList(customerNumber, conn));
            }
            return accountList;
        }


        internal static List<Account> GetCurrentAccounts(ulong customerNumber)
        {
            List<Account> accountList = GetCurrentAccountsAsync(customerNumber).Result;

            return accountList;
        }

        internal static async Task<List<Account>> GetCurrentAccountsAsync(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                Task<List<Account>> accountList1 = GetCurrentAccountList(customerNumber, conn);

                Task<List<Account>> accountList2 = GetAparikAccountList(customerNumber, conn);

                Task<List<Account>> accountList3 = GetJointAccountList(customerNumber, conn);

                Task<List<Account>> accountList4 = GetDAHKAccountList(customerNumber, conn);

                Task<List<Account>> accountList5 = GetSocialAccountList(customerNumber, conn);

                Task<List<Account>> accountList6 = GetCardDAHKAccountList(customerNumber, conn);

                Task<List<Account>> accountList7 = GetBankruptAccountList(customerNumber, conn);

                Task<List<Account>> accountList8 = GetCurrentAccountListForNonMobileClients(customerNumber, conn);   //Սահմանափակ հասանելիությամ հաշվիներ

                Task<List<Account>> accountList9 = GetNextPeriodRateAccountList(customerNumber, conn);   //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ

                Task<List<Account>> accountList10 = GetSocialSecurityAccountList(customerNumber, conn);   //Սոցիալական ապահովության հաշիվ


                accountList.AddRange(await accountList1);

                accountList.AddRange(await accountList2);

                accountList.AddRange(await accountList3);

                accountList.AddRange(await accountList4);

                accountList.AddRange(await accountList5);

                accountList.AddRange(await accountList6);

                accountList.AddRange(await accountList7);

                accountList.AddRange(await accountList8);

                accountList.AddRange(await accountList9);

                accountList.AddRange(await accountList10);

                accountList.AddRange(await GetDeveloperSpecialAccountsAsync(customerNumber, conn)); // Կառուցապատողի հատուկ հաշիվներ
            }
            return accountList;
        }



        internal static List<Account> GetCurrentClosedAccounts(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(AccountDB.GetCurrentClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetAparikClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetJointClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetDAHKClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetSocialClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetSocialSecurityClosedAccountList(customerNumber, conn));

                accountList.AddRange(AccountDB.GetDeveloperSpecialAccountsAsync(customerNumber, conn).Result);

            }

            return accountList;
        }

        internal static List<Account> GetOtherBankAttachedCards(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                accountList.AddRange(GetOtherBankAttachedCardsAsync(customerNumber, conn).Result);
            }
            return accountList;
        }

        internal static async Task<List<Account>> GetOtherBankAttachedCardsAsync(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT DISTINCT ob.card_number, ob.binding_Id FROM tbl_other_bank_cards ob WHERE ob.customer_number = @customerNumber And ob.quality in (2,3) AND card_number is not null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                while (dr.Read())
                {
                    accountList.Add(new Account
                    {
                        AccountNumber = dr["card_number"].ToString(),
                        AccountTypeDescription = "Կցված քարտ",
                        AccountTypeDescriptionEng = "Attached Card",
                        BindingId = dr["binding_Id"].ToString(),
                        IsAttachedCard = true
                    });
                }
            }
            return accountList;
        }
        public static string GetAttachedCardBindingId(long docId)
        {
            string bindingId = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT TOP 1 ob.binding_id FROM (SELECT id, binding_Id  FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION ALL SELECT id, binding_Id FROM [tbl_other_bank_cards_deleted]) ob JOIN [dbo].[tbl_other_bank_card_orders] ord ON ord.card_id = ob.id  WHERE ord.doc_id = @doc_id";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        bindingId = result.ToString();
                }
            }
            return bindingId;
        }
        public static string GetAttachedCardNumber(long docId)
        {
            string attachedCardNumber = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT TOP 1 ob.card_number FROM (SELECT id, card_number  FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION ALL SELECT id, card_number FROM [tbl_other_bank_cards_deleted]) ob JOIN [dbo].[tbl_other_bank_card_orders] ord ON ord.card_id = ob.id  WHERE ord.doc_id = @doc_id";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        attachedCardNumber = result.ToString();
                }
            }
            return attachedCardNumber;
        }
        public static double  GetAttachedCardFee(long docId)
        {
           double fee = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string script = @"select top 1 ord.fee_amount from [dbo].[tbl_other_bank_card_orders] ord where ord.doc_id = @doc_id";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        fee = Convert.ToDouble(result.ToString());
                }
            }
            return fee;
        }
        internal static async Task<List<Account>> GetCurrentAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.description as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 10) AND(type_of_account = 24)
                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }



        //Սահմանափակ հասանելիությամ հաշվիներ
        internal static async Task<List<Account>> GetCurrentAccountListForNonMobileClients(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, N'Սահմանափակ հաշիվ' AS description, acc.closing_date,A.Co_Type,a.card_number, N'Սահմանափակ հաշիվ' as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,  s.type_of_account, 'Limited account' AS DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new,  type_of_product, type_of_account FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 18) AND(type_of_account = 283)
                                                GROUP BY sint_acc_new,  type_of_product, type_of_account)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }



        //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ
        internal static async Task<List<Account>> GetNextPeriodRateAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number, N'Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ' as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,  s.type_of_account FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new,  type_of_product, type_of_account FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 18) AND(type_of_account = 282)
                                                GROUP BY sint_acc_new,  type_of_product, type_of_account)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }


        internal static async Task<List<Account>> GetBankruptAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.description as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 116) AND(type_of_account = 24)
                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }



        internal static async Task<List<Account>> GetCardDAHKAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = @"SELECT   a.open_date,115 as type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency,
                                t.description, acc.closing_date,A.Co_Type,v.visa_number as card_number,a.description as ac_description,
                                a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,
                                a.UnUsed_amount_date, a.account_access_group FROM V_All_Accounts a
                                 INNER JOIN [tbl_all_accounts;] acc
                                ON a.Arm_number=acc.Arm_number
                                INNER JOIN
                                (
                                SELECT sint_acc_new, type_of_client, type_of_product,type_of_account FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 11) AND(type_of_account = 73)
                                GROUP BY sint_acc_new, type_of_client, type_of_product,type_of_account
                                 
                                 )s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
                                Tbl_type_of_accounts t ON s.type_of_account = t.ID
                                INNER JOIN
                                (
                                Tbl_Visa_Numbers_Accounts v
                                INNER JOIN
                                Tbl_Products_Accounts_Groups g
                                ON v.app_id=g.app_id
                                INNER JOIN Tbl_Products_Accounts pr
                                ON g.group_id=pr.group_id
                                AND pr.type_of_account=73
                                )
                                ON
                                pr.account_number=acc.arm_number
                                WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number and v.closing_date is null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }

        internal static List<Account> GetCurrentClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT  a.open_date,type_of_product,  acc.Arm_number,acc.account_access_group, acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 10) AND(type_of_account = 24)
                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is not null  and  a.customer_number=acc.customer_number";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);

                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }
        }


        internal static async Task<List<Account>> GetJointAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,acc.customer_number,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng  FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 10) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 Inner Join [tbl_all_accounts;] acc
											 On a.Arm_number=acc.Arm_number
                                              WHERE  a.Closing_date is null 
                                             And a.Customer_Number =@customerNumber  and Co_Type<>0
											 And a.customer_number<>acc.customer_number
                                             Order by a.open_date";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }


        }

        internal static List<Account> GetJointClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = @"SELECT a.open_date,acc.customer_number,type_of_product, a.Arm_number,a.account_access_group, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date  FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 10) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 Inner Join [tbl_all_accounts;] acc
											 On a.Arm_number=acc.Arm_number
                                              WHERE  a.Closing_date is not null 
                                             And a.Customer_Number =@customerNumber  and Co_Type<>0
											 And a.customer_number<>acc.customer_number
                                             Order by a.open_date";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }

        internal static async Task<List<Account>> GetDAHKAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 61) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber and closing_date is null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }

        internal static List<Account> GetDAHKAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 61) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber and closing_date is null";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Account account = SetAccount(row);

                        accountList.Add(account);
                    }
                }


                return accountList;
            }

        }
        internal static List<Account> GetDAHKClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 61) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber and closing_date is not null";
            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }

        internal static async Task<List<Account>> GetSocialAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 57) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber And closing_date is null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }


        }

        internal static List<Account> GetSocialClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 57) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber And closing_date is not null";


            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }


        }

        internal static async Task<List<Account>> GetCardAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";
            script = @"SELECT app_id, a.open_date,type_of_product, Arm_number, type_of_account_new, balance, a.Currency, t.description,V.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,s.type_of_account,t.DescriptionEng FROM [tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product,type_of_account FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 11) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product,type_of_account)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             INNER JOIN Tbl_Visa_Numbers_Accounts V on a.Arm_number=v.card_account
                                             WHERE a.customer_number = @customerNumber and V.closing_date is null and V.attached_card=0 ";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }


                return accountList;
            }

        }

        internal static List<Account> GetCardClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";

            script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 11) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber and closing_date is not null";


            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }


                return accountList;
            }

        }

        internal static List<Account> GetCardAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(GetCardAccountList(customerNumber, conn).Result);
            }

            return accountList;
        }
        internal static List<Account> GetCardClosedAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(AccountDB.GetCardClosedAccountList(customerNumber, conn));
            }

            return accountList;
        }

        internal static async Task<List<Account>> GetDepositAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";

            script = @"SELECT D.app_id, a.open_date, type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description, a.closing_date, ISNULL(accM.type,0) as Co_Type
                                                        ,a.card_number, a.filialcode, a.description as ac_description, a.account_type, a.freeze_date, a.UnUsed_amount, a.UnUsed_amount_date
                                                        ,a.account_access_group, t.DescriptionEng
                                                    FROM [tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             INNER JOIN [Tbl_deposits;] D on a.Arm_number=D.deposit_full_number
                                             LEFT JOIN Tbl_Co_Accounts_Main accM on a.arm_number = accM.arm_number
                                             WHERE a.customer_number = @customerNumber And a.closing_date is null And D.quality=1";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }

        internal static async Task<List<Account>> GetDecreasingDepositAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";

            script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             INNER JOIN [Tbl_deposits;] D on a.Arm_number=D.deposit_full_number
                                             WHERE a.customer_number = @customerNumber And closing_date is null And D.quality=1 AND D.allow_decreasing=1";


            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }

        internal static async Task<List<Account>> GetDecreasingJointDepositAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";

            script = @"SELECT top 100 dep.App_Id, a.open_date,acc.customer_number,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 Inner Join [tbl_all_accounts;] acc
											 INNER JOIN [Tbl_deposits;] dep
											 on dep.deposit_full_number=acc.Arm_number
											 On a.Arm_number=acc.Arm_number
                                              WHERE  a.Closing_date is null 
                                              And a.Customer_Number =@customerNumber and Co_Type<>0
											 And a.customer_number<>acc.customer_number and dep.quality=1 and dep.allow_decreasing=1
                                             Order by a.open_date";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }


        }

        internal static List<Account> GetDepositClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";


            script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber And closing_date is not null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }

        }

        internal static List<Account> GetDepositAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(GetDepositAccountList(customerNumber, conn).Result);
                accountList.AddRange(GetJointDepositAccountList(customerNumber, conn).Result);
            }

            return accountList;
        }

        internal static List<Account> GetDecreasingDepositAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(GetDecreasingDepositAccountList(customerNumber, conn).Result);
                accountList.AddRange(GetDecreasingJointDepositAccountList(customerNumber, conn).Result);
            }

            return accountList;
        }

        internal static List<Account> GetDepositClosedAccountList(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                accountList.AddRange(AccountDB.GetDepositClosedAccountList(customerNumber, conn));
            }

            return accountList;
        }

        internal static async Task<List<Account>> GetAparikAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = aparikAccountSelectScript + " UNION ALL " + closedAparikAccountSelectScript + " AND closing_date is null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                accountList = accountList.GroupBy(i => i.AccountNumber).Select(g => g.First()).ToList();

                return accountList;
            }

        }

        internal static List<Account> GetAparikClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = aparikAccountSelectScript + " AND closing_date is not null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = SetAccount(row);

                    accountList.Add(account);
                }

                return accountList;
            }
        }

        private static Account SetAccount(DataRow row)
        {
            Account account = new Account();

            if (row != null)
            {
                account.OpenDate = default(DateTime?);
                account.ClosingDate = default(DateTime?);
                account.FreezeDate = default(DateTime?);

                account.AccountNumber = row["Arm_number"].ToString();
                account.Balance = double.Parse(row["balance"].ToString());
                account.Currency = row["Currency"].ToString();
                account.AccountType = ushort.Parse(row["type_of_product"].ToString());
                account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                account.ProductNumber = row["card_number"].ToString();
                account.FilialCode = int.Parse(row["filialcode"].ToString());
                account.Status = short.Parse(row["account_type"].ToString());
                account.FreezeDate = row["freeze_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["freeze_date"].ToString());
                account.UnUsedAmount = row["UnUsed_amount"] == DBNull.Value ? default(double?) : double.Parse(row["UnUsed_amount"].ToString());
                account.UnUsedAmountDate = row["UnUsed_amount_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["UnUsed_amount_date"].ToString());

                if (row.Table.Columns.Contains("account_access_group"))
                    account.AccountPermissionGroup = row["account_access_group"].ToString();
                else
                    account.AccountPermissionGroup = "0";

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }

                if (row.Table.Columns.Contains("Co_type"))
                {
                    account.JointType = ushort.Parse(row["Co_Type"].ToString());
                }
                else
                    account.JointType = 0;
                account.OpenDate = (DateTime.Parse(row["open_date"].ToString())).Date;
                if (row.Table.Columns.Contains("ac_description"))
                {
                    if (row["ac_description"] != DBNull.Value)
                    {
                        account.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("ac_description"));
                    }
                    else
                    {
                        if (row["description"] != DBNull.Value)
                            account.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("description"));
                    }
                }

                if (row.Table.Columns.Contains("type_of_account"))
                {
                    if (int.TryParse(row["type_of_account"].ToString(), out int number))
                    {
                        account.TypeOfAccount = number;
                    }
                    else
                        account.TypeOfAccount = 0;
                }
                else
                    account.TypeOfAccount = 0;

                if (row.Table.Columns.Contains("DescriptionEng"))
                {
                    account.AccountTypeDescriptionEng = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(row["DescriptionEng"].ToString());
                }
                else
                    account.AccountTypeDescriptionEng = "";

                account.AvailableBalance = Account.GetAcccountAvailableBalance(account.AccountNumber);


            }
            return account;
        }


        private static Account SetAccountWithoutBallance(DataRow row)
        {
            Account account = new Account();

            if (row != null)
            {
                account.OpenDate = default(DateTime?);
                account.ClosingDate = default(DateTime?);
                account.FreezeDate = default(DateTime?);

                account.AccountNumber = row["Arm_number"].ToString();
                account.Balance = double.Parse(row["balance"].ToString());
                account.Currency = row["Currency"].ToString();
                account.AccountType = ushort.Parse(row["type_of_product"].ToString());
                account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                account.ProductNumber = row["card_number"].ToString();
                account.FilialCode = int.Parse(row["filialcode"].ToString());
                account.Status = short.Parse(row["account_type"].ToString());
                account.FreezeDate = row["freeze_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["freeze_date"].ToString());
                account.UnUsedAmount = row["UnUsed_amount"] == DBNull.Value ? default(double?) : double.Parse(row["UnUsed_amount"].ToString());

                account.UnUsedAmountDate = row["UnUsed_amount_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["UnUsed_amount_date"].ToString());
                if (row.Table.Columns.Contains("account_access_group"))
                    account.AccountPermissionGroup = row["account_access_group"].ToString();
                else
                    account.AccountPermissionGroup = "0";

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }

                if (row.Table.Columns.Contains("Co_type"))
                {
                    account.JointType = ushort.Parse(row["Co_Type"].ToString());
                }
                else
                    account.JointType = 0;
                account.OpenDate = (DateTime.Parse(row["open_date"].ToString())).Date;
                if (row.Table.Columns.Contains("ac_description"))
                {
                    account.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("ac_description"));
                }



            }
            return account;
        }


        private static async Task<Account> SetAccountAsync(DataRow row)
        {
            Account account = new Account();

            if (row != null)
            {
                account.OpenDate = default;
                account.ClosingDate = default;
                account.FreezeDate = default;

                account.AccountNumber = row["Arm_number"].ToString();
                account.Balance = double.Parse(row["balance"].ToString());
                account.Currency = row["Currency"].ToString();
                account.AccountType = ushort.Parse(row["type_of_product"].ToString());
                account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                account.ProductNumber = row["card_number"].ToString();
                account.FilialCode = int.Parse(row["filialcode"].ToString());
                account.Status = short.Parse(row["account_type"].ToString());
                account.FreezeDate = row["freeze_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["freeze_date"].ToString());

                account.UnUsedAmount = row["UnUsed_amount"] == DBNull.Value ? default(double?) : double.Parse(row["UnUsed_amount"].ToString());

                account.UnUsedAmountDate = row["UnUsed_amount_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["UnUsed_amount_date"].ToString());
                if (row.Table.Columns.Contains("account_access_group"))
                    account.AccountPermissionGroup = row["account_access_group"].ToString();
                else
                    account.AccountPermissionGroup = "0";

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }

                if (row.Table.Columns.Contains("Co_type"))
                {
                    account.JointType = ushort.Parse(row["Co_Type"].ToString());
                }
                else
                    account.JointType = 0;
                account.OpenDate = (DateTime.Parse(row["open_date"].ToString())).Date;
                if (row.Table.Columns.Contains("ac_description"))
                {
                    account.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("ac_description"));
                }

                if (row.Table.Columns.Contains("type_of_account"))
                {
                    if (int.TryParse(row["type_of_account"].ToString(), out int number))
                    {
                        account.TypeOfAccount = number;
                    }
                    else
                        account.TypeOfAccount = 0;
                }
                else
                    account.TypeOfAccount = 0;

                if (row.Table.Columns.Contains("DescriptionEng"))
                {
                    account.AccountTypeDescriptionEng = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(row["DescriptionEng"].ToString());
                }
                else
                    account.AccountTypeDescriptionEng = "";


                Task<double> AvailableBalanceAsync = Account.GetAcccountAvailableBalanceAsync(account.AccountNumber);

                account.AvailableBalance = await AvailableBalanceAsync;
                if (row.Table.Columns.Contains("app_id"))
                {
                    account.ProductId = row["app_id"].ToString();
                }


            }
            return account;
        }


        public static AccountStatement GetStatement(string accountNumber, DateTime dateFrom, DateTime dateTo, byte language, SourceType sourceType, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            AccountStatement accountStatement = new AccountStatement();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("Sp_Extract", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@account_gl", SqlDbType.BigInt).Value = accountNumber;
                    cmd.Parameters.Add("@convertation_sign", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@minAmount", SqlDbType.Float).Value = minAmount;
                    cmd.Parameters.Add("@maxAmount", SqlDbType.Float).Value = maxAmount;
                    cmd.Parameters.Add("@debCred", SqlDbType.NVarChar, 1).Value = debCred;
                    cmd.Parameters.Add("@transactionsCount", SqlDbType.Int).Value = transactionsCount;
                    cmd.Parameters.Add("@orderByAmountAscDesc", SqlDbType.TinyInt).Value = orderByAscDesc;
                    cmd.Parameters.Add("@soutceType", SqlDbType.Bit).Value = sourceType;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        string accountCurrency = "";
                        while (dr.Read())
                        {
                            accountCurrency = dr["currency"].ToString();
                            if (dr["type_of_account"] != DBNull.Value)
                            {
                                accountStatement.SyntheticTypeOfAccount = dr["type_of_account"].ToString();
                            }
                        }

                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {

                                AccountStatementDetail statementDetails = new AccountStatementDetail();

                                if (accountCurrency == "AMD")
                                    statementDetails.Amount = Convert.ToDouble(dr["amount"]);
                                else
                                    statementDetails.Amount = Convert.ToDouble(dr["amount_currency"]);

                                statementDetails.AmountBase = Convert.ToDouble(dr["amount"]);
                                statementDetails.CorrespondentAccount = dr["Reverse_account_full_number"].ToString();
                                statementDetails.DebitCredit = Convert.ToChar(dr["debit_credit"]);
                                statementDetails.OperationType = Convert.ToInt32(dr["operation_type"]);
                                statementDetails.Description = dr["Wording"].ToString();
                                statementDetails.TransactionDate = DateTime.Parse(dr["date_of_accounting"].ToString());
                                statementDetails.CashOperationNumber = Convert.ToInt32(dr["cash_operation_number"]);
                                statementDetails.CurrentAccountNumber = Convert.ToInt16(dr["current_account_number"]);
                                statementDetails.MainDebAccount = dr["MainDebAccount"].ToString();
                                statementDetails.MainCredAccount = dr["MainCredAccount"].ToString();
                                if (statementDetails.DebitCredit == 'd' && dr["Receiver"].ToString() != "")
                                {
                                    statementDetails.Correspondent = dr["Receiver"].ToString();
                                    if (sourceType == SourceType.AcbaOnline || sourceType == SourceType.MobileBanking)
                                    {
                                        statementDetails.Description = statementDetails.Description + " ստացող՝ " + statementDetails.Correspondent;
                                    }
                                }
                                else if (statementDetails.DebitCredit == 'c' && dr["Payer"].ToString() != "")
                                {
                                    statementDetails.Correspondent = dr["Payer"].ToString();
                                    if (sourceType == SourceType.AcbaOnline || sourceType == SourceType.MobileBanking)
                                    {
                                        statementDetails.Description = statementDetails.Description + " փոխանցող՝ " + statementDetails.Correspondent;
                                    }
                                }

                                statementDetails.UserId = int.Parse(dr["number_of_set"].ToString());
                                statementDetails.TransactionsGroupNumber =
                                    Convert.ToDouble(dr["transactions_group_number"]);

                                statementDetails.ItemNumber = dr["number_of_item"] != DBNull.Value ? Convert.ToInt32(dr["number_of_item"]) : 0;
                                accountStatement.Transactions.Add(statementDetails);

                            }
                        }

                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                accountStatement.InitialBalance = accountCurrency != "AMD"
                                    ? Convert.ToDouble(dr["rest"])
                                    : Convert.ToDouble(dr["rest_AMD"]);
                                accountStatement.TotalDebitAmount = Convert.ToDouble(0);
                                accountStatement.TotalCreditAmount = Convert.ToDouble(0);
                            }
                        }

                        if (dr.NextResult())
                        {
                            if (dr.NextResult())
                            {
                                while (dr.Read())
                                {
                                    AccountStatementTotalsByDays statementTotalsByDays = new AccountStatementTotalsByDays
                                    {
                                        TransactionDate = Convert.ToDateTime(dr["date_of_accounting"]),
                                        DayTotalDebetAmount = Convert.ToDouble(dr["det_curr"]),
                                        DayTotalDebetAmountBase = Convert.ToDouble(dr["deb"]),
                                        DayTotalCreditAmount = Convert.ToDouble(dr["cred_curr"]),
                                        DayTotalCreditAmountBase = Convert.ToDouble(dr["cred"])
                                    };
                                    accountStatement.TotalsByDays.Add(statementTotalsByDays);
                                    accountStatement.TotalDebitAmount += accountCurrency != "AMD"
                                        ? Convert.ToDouble(statementTotalsByDays.DayTotalDebetAmount)
                                        : Convert.ToDouble(statementTotalsByDays.DayTotalDebetAmountBase);
                                    accountStatement.TotalCreditAmount += accountCurrency != "AMD"
                                        ? Convert.ToDouble(statementTotalsByDays.DayTotalCreditAmount)
                                        : Convert.ToDouble(statementTotalsByDays.DayTotalCreditAmountBase);

                                }


                                accountStatement.FinalBalance =
                                    Math.Round(
                                        accountStatement.InitialBalance + accountStatement.TotalCreditAmount -
                                        accountStatement.TotalDebitAmount, 2);
                            }

                        }
                    }
                }
            }

            return accountStatement;
        }

        internal static bool IsCardAccount(string accountNumber)
        {
            bool isCardAccount = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"Select s.sint_acc
										From [tbl_all_accounts;] a join Tbl_define_sint_acc s
										on a.type_of_account_new = s.sint_acc_new
										Where a.Arm_number=@accountNumber and s.type_of_product=11 and s.type_of_account=24 ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    isCardAccount = true;
                }


            }
            return isCardAccount;

        }
        internal static bool IsIPayAccount(string accountNumber)
        {
            bool IsIPayAccount = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM [tbl_IPay_account_numbers]  WHERE Account_Number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    IsIPayAccount = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            return IsIPayAccount;

        }

        internal static bool IsDepositAccount(string accountNumber)
        {
            bool isDepositAccount = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"Select s.sint_acc
										From [tbl_all_accounts;] a join .Tbl_define_sint_acc s
										on a.type_of_account_new = s.sint_acc_new
										Where a.Arm_number=@accountNumber and s.type_of_product=13 and s.type_of_account=24", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    isDepositAccount = true;
                }


            }
            return isDepositAccount;
        }

        internal static bool IsCurrentAccount(string accountNumber)
        {
            bool isCurrentAccount = false;



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select s.sint_acc
										From [tbl_all_accounts;] a join Tbl_define_sint_acc s
										on a.type_of_account_new = s.sint_acc_new
										Where a.Arm_number=@accountNumber and 
										(((s.type_of_product=10 or s.type_of_product=116) and s.type_of_account=24) or (s.type_of_product=18 and s.type_of_account=24)) 
										and a.closing_date is null", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isCurrentAccount = true;
                        }
                    }
                }
            }
            return isCurrentAccount;
        }
        /// <summary>
        /// Ստուգում է հաշիվը սահմանափակ է թէ ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsRestrictedAccount(string accountNumber)
        {
            bool isRestrictedAccount = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select s.sint_acc
										From [tbl_all_accounts;] a join Tbl_define_sint_acc s
										on a.type_of_account_new = s.sint_acc_new
										Where a.Arm_number=@accountNumber and 
										s.type_of_product=18 and  s.type_of_account=283 
										and a.closing_date is null", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isRestrictedAccount = true;
                        }
                    }
                }
            }
            return isRestrictedAccount;
        }

        internal static bool HaveActiveCard(string accountNumber)
        {
            bool haveActiveCard = false;



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select visa_number from Tbl_Visa_Numbers_Accounts
																 where card_account=@accountNumber and closing_date IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveCard = true;
                        }
                    }

                }
            }
            return haveActiveCard;
        }


        internal static bool HasActiveDeposit(string accountNumber)
        {
            bool hasActiveDeposit = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select NULL from [Tbl_deposits;] where deposit_full_number=@accountNumber and quality=1", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hasActiveDeposit = true;
                        }
                    }
                }


            }
            return hasActiveDeposit;
        }


        internal static double GetAccountBalance(string accountNumber)
        {
            double accountBalance = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DECLARE @oper_day as smalldatetime
                                                  SELECT @oper_day=oper_day FROM Tbl_current_oper_day                
                                                SELECT dbo.fnc_available_AccountAmount_For_24_7(acc.Arm_number,@oper_day,default,default,default, 0) as Balance, 
                                                    isnull(start_capital + current_capital,-1) as OverBalance 
                                                    FROM [tbl_all_accounts;] acc  
                                                LEFT JOIN 
                                                        [dbo].[Tbl_credit_lines] cr 
                                                ON acc.Arm_number = cr.loan_full_number WHERE arm_number=@accountNumber ", conn))

                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = Convert.ToUInt64(accountNumber);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            accountBalance = double.Parse(dr["OverBalance"].ToString()) == -1 ? double.Parse(dr["Balance"].ToString()) : double.Parse(dr["OverBalance"].ToString());
                        }
                    }
                }
            }

            return accountBalance;
        }
        /// <summary>
        /// Ընթացիք հաշվի կամ ապառիկ տեղում հաշվի մանրամասներ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static Account GetCurrentAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                //283 Սահմանափակ հասանելիությամ հաշվիներ
                string sql = @"SELECT a.open_date,a.account_access_group,v.co_type,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,a.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM[tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc   
													 WHERE (type_of_product in (10,61,57,118,119) AND type_of_account = 24) OR type_of_account = 283 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 inner join v_all_accounts v on a.arm_number=v.arm_number 
                                             WHERE v.Arm_number = @accountNumber and v.customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }


        internal static Account GetBankruptAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT a.open_date,a.account_access_group,v.co_type,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,a.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM[tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE type_of_product =116 AND type_of_account = 24 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code  
											 inner join v_all_accounts v on a.arm_number=v.arm_number 
                                             WHERE v.Arm_number = @accountNumber  and v.customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }

        internal static ulong GetBankruptcyManager(string accountNumber)
        {
            ulong bankruptcyManagetCustNumber = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT ad.additionvalue 
                                        FROM [tbl_all_accounts;] AL 
                                        INNER JOIN Tbl_all_accounts_AddInf AD 
                                        on al.arm_number = ad.arm_number 
                                        WHERE  al.arm_number = @accountNumber and ad.additionid = 16";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        bankruptcyManagetCustNumber = Convert.ToUInt64(row["additionvalue"]);
                    }
                }

            }
            return bankruptcyManagetCustNumber;

        }



        internal static Account GetCardDahkAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"	SELECT   a.open_date,115 as type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency,
                                t.description, acc.closing_date,A.Co_Type,v.visa_number as card_number,a.description as ac_description,
                                a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,
                                a.UnUsed_amount_date, a.account_access_group FROM V_All_Accounts a
                                 INNER JOIN [tbl_all_accounts;] acc
                                ON a.Arm_number=acc.Arm_number
                                INNER JOIN
                                (
                                SELECT sint_acc_new, type_of_client, type_of_product,type_of_account FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 11) AND(type_of_account = 73)
                                GROUP BY sint_acc_new, type_of_client, type_of_product,type_of_account
                                 
                                 )s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
                                Tbl_type_of_accounts t ON s.type_of_account = t.ID
                                INNER JOIN
                                (
                                Tbl_Visa_Numbers_Accounts v
                                INNER JOIN
                                Tbl_Products_Accounts_Groups g
                                ON v.app_id=g.app_id
                                INNER JOIN Tbl_Products_Accounts pr
                                ON g.group_id=pr.group_id
                                AND pr.type_of_account=73
                                )
                                ON
                                pr.account_number=acc.arm_number
                                WHERE acc.customer_number = @customerNumber And acc.closing_date is null  
                                and  a.customer_number=acc.customer_number and v.closing_date is null AND acc.arm_number=@accountNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }


        internal static Account GetCardDahkAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"	SELECT   a.open_date,115 as type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency,
                                t.description, acc.closing_date,A.Co_Type,v.visa_number as card_number,a.description as ac_description,
                                a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,
                                a.UnUsed_amount_date, a.account_access_group FROM V_All_Accounts a
                                 INNER JOIN [tbl_all_accounts;] acc
                                ON a.Arm_number=acc.Arm_number
                                INNER JOIN
                                (
                                SELECT sint_acc_new, type_of_client, type_of_product,type_of_account FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 11) AND(type_of_account = 73)
                                GROUP BY sint_acc_new, type_of_client, type_of_product,type_of_account
                                 
                                 )s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
                                Tbl_type_of_accounts t ON s.type_of_account = t.ID
                                INNER JOIN
                                (
                                Tbl_Visa_Numbers_Accounts v
                                INNER JOIN
                                Tbl_Products_Accounts_Groups g
                                ON v.app_id=g.app_id
                                INNER JOIN Tbl_Products_Accounts pr
                                ON g.group_id=pr.group_id
                                AND pr.type_of_account=73
                                )
                                ON
                                pr.account_number=acc.arm_number
                                WHERE acc.closing_date is null  
                                and  a.customer_number=acc.customer_number and v.closing_date is null AND acc.arm_number=@accountNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }

        /// <summary>
        /// Ընթացիք հաշվի կամ ապառիկ տեղում հաշվի մանրամասներ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static Account GetCurrentAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT a.account_access_group,a.open_date,v.co_type,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,a.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM[tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE type_of_product in (10,61,57) AND type_of_account = 24 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 inner join v_all_accounts v on a.arm_number=v.arm_number 
                                             WHERE v.Arm_number = @accountNumber ";



                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }

        internal static async Task<Account> GetCurrentAccountAsync(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT a.account_access_group,a.open_date,v.co_type,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,a.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM[tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE type_of_product in (10,61,57) AND type_of_account = 24 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 inner join v_all_accounts v on a.arm_number=v.arm_number 
                                             WHERE v.Arm_number = @accountNumber ";



                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = await SetAccountAsync(row);
                    }
                }

            }
            return account;

        }

        public static List<KeyValuePair<ulong, double>> GetAccountJointCustomers(string accountNumber)
        {
            List<KeyValuePair<ulong, double>> list = new List<KeyValuePair<ulong, double>>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select a.customer_number,a.part from [tbl_all_accounts;] as d left join Tbl_Co_Accounts_Main as m on d.Arm_number=m.arm_number left join Tbl_co_accounts as a on m.ID=a.co_main_ID where d.arm_number=@account_number", conn))
                {
                    cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = accountNumber;
                    dt.Load(cmd.ExecuteReader());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["customer_number"] != DBNull.Value)
                        {
                            list.Add(new KeyValuePair<ulong, double>(Convert.ToUInt64(dt.Rows[i]["customer_number"]), Convert.ToDouble(dt.Rows[i]["part"])));
                        }
                    }
                }


            }
            return list;
        }
        /// <summary>
        /// Վերադարձնում է հաշվի տեսակը
        /// </summary>
        /// <returns></returns>
        internal static ushort GetType(string accountNumber)
        {
            ushort result = 0;
            Account account = GetCurrentAccount(accountNumber);
            if (account != null)
            {
                result = account.AccountType;
            }



            return result;
        }

        /// <summary>
        /// Հաշիվը ոստիկանության հաշիվ է, թե ոչ
        /// </summary>
        /// <returns></returns>
        public static bool IsPoliceAccount(string accountNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT code FROM Tbl_budget_accounts where isnull(ACkind,'') <>'' and code = @accountNumber";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.NVarChar, 50).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return true;
                        else return false;
                    }
                }

            }
        }



        public static bool CheckAccountForPSN(string accountNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT dbo.fn_check_acc_for_soc(@account_number)";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 50).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return Convert.ToBoolean(dr[0]);
                        else return false;
                    }
                }
            }
        }

        internal static bool HaveActiveProduct(string accountNumber)
        {
            bool haveActiveProduct = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 Account_number from Tbl_Products_Accounts as p Inner join Tbl_Products_Accounts_Groups as g ON p.Group_Id=g.Group_ID 
                                                  where Group_Status=1 and Account_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                   using SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        haveActiveProduct = true;
                    }
                }

            }
            return haveActiveProduct;
        }
        internal static bool HaveActiveDepositForCurrentAccount(string accountNumber)
        {
            bool haveActiveDeposit = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 deposit_number from [tbl_deposits;] 
                                                  where (quality = 1 and (connect_account_full_number = @accountNumber or connect_account_added = @accountNumber))", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveDeposit = true;
                        }
                    }


                }

            }
            return haveActiveDeposit;
        }
        internal static bool HaveActiveLoanForCurrentAccount(string accountNumber)
        {
            bool haveActiveLoan = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 loan_number from [Tbl_short_time_loans;] where connect_account_full_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveLoan = true;
                        }
                    }
                }

            }
            return haveActiveLoan;
        }
        internal static bool HaveActiveCreditLineForCurrentAccount(string accountNumber)
        {
            bool haveActiveCreditLine = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 loan_number from [Tbl_credit_lines] where connect_account_full_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveCreditLine = true;
                        }
                    }

                }

            }
            return haveActiveCreditLine;
        }
        internal static bool HaveActiveSocPackageCreditLineForCurrentAccount(string accountNumber, ulong customerNumber)
        {
            bool haveActiveCreditLine = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 connect_account_full_number from Tbl_credit_lines C where quality<>10 and credit_line_type=36 and customer_number=@customerNumber
                                                  and NOT EXISTS (select NULL
                                                                  from [tbl_all_accounts;] AV INNER JOIN V_CurrentAcc_SintNew V ON AV.type_of_account_new = V.sint_acc_new
                                                                  where AV.customer_number = C.customer_number AND AV.closing_date IS NULL AND AV.currency ='AMD' and arm_number<>@accountNumber)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveCreditLine = true;
                        }
                    }

                }

            }
            return haveActiveCreditLine;
        }
        internal static bool HaveActiveOperationByPeriodForCurrentAccount(string accountNumber)
        {
            bool haveActiveOperationByPeriod = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 nn from [Tbl_Operations_By_Period] 
                                                  where (quality = 1 And (Debet_Account = @accountNumber or Credit_Account = @accountNumber))", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            haveActiveOperationByPeriod = true;
                        }
                    }
                }

            }
            return haveActiveOperationByPeriod;
        }
        internal static bool HaveHBForCurrentAccount(string accountNumber, ulong customerNumber)
        {
            bool haveHB = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select acc.arm_number from [tbl_all_accounts;] acc inner join 
                                                         (Select sint_acc_new from Tbl_define_sint_acc where type_of_client<>6 and  type_of_account = 24 and type_of_product = 10) s on acc.type_of_account_new = s.sint_acc_new 
                                                  where acc.currency='AMD' and closing_date is null and acc.customer_number=@customerNumber and acc.arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            using (SqlCommand cmd2 = new SqlCommand(@"select acc.arm_number from [tbl_all_accounts;] acc inner join 
                                                              (Select sint_acc_new from Tbl_define_sint_acc where type_of_account = 24 and type_of_product = 10) s on acc.type_of_account_new = s.sint_acc_new
                                                       where acc.currency = 'AMD' and closing_date is null and acc.customer_number = @customerNumber and acc.arm_number<>@accountNumber", conn))
                            {
                                cmd2.CommandType = CommandType.Text;
                                cmd2.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                                cmd2.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                                using (SqlDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    if (!dr2.Read())
                                    {
                                        using (SqlCommand cmd3 = new SqlCommand(@"select id from Tbl_HB_Users where customer_number=@customerNumber and closing_date is null", conn))
                                        {
                                            cmd3.CommandType = CommandType.Text;
                                            cmd3.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                                            using (SqlDataReader dr3 = cmd3.ExecuteReader())
                                            {
                                                if (dr3.Read())
                                                {
                                                    haveHB = true;
                                                }
                                            }

                                        }

                                    }
                                }
                            }

                        }
                    }


                }

            }
            return haveHB;
        }
        internal static bool HaveCurrencyCardsForCurrentAccount(string accountNumber, ulong customerNumber)
        {
            bool haveCurrencyCards = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select acc.arm_number from [tbl_all_accounts;] acc inner join 
                                                         (Select sint_acc_new from Tbl_define_sint_acc where type_of_account = 24 and type_of_product = 10) s on acc.type_of_account_new = s.sint_acc_new 
                                                  where acc.currency = 'AMD' and closing_date is null and acc.customer_number = @customerNumber and acc.arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            using (SqlCommand cmd2 = new SqlCommand(
                                @"select acc.arm_number from [tbl_all_accounts;] acc inner join
                                                              (Select sint_acc_new from Tbl_define_sint_acc where type_of_account = 24 and type_of_product = 10) s on acc.type_of_account_new = s.sint_acc_new
                                                      where acc.currency = 'AMD' and closing_date is null and acc.customer_number = @customerNumber and acc.arm_number<>@accountNumber",
                                conn))
                            {
                                cmd2.CommandType = CommandType.Text;
                                cmd2.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                                cmd2.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                                using (SqlDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    if (!dr2.Read())
                                    {
                                        using (SqlCommand cmd3 = new SqlCommand(
                                            @"select top 1 visa_number from [Tbl_Visa_Numbers_Accounts] where customer_number=@customerNumber and currency <>'AMD' and closing_date is null",
                                            conn))
                                        {
                                            cmd3.CommandType = CommandType.Text;
                                            cmd3.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                                            using (SqlDataReader dr3 = cmd3.ExecuteReader())
                                            {
                                                if (dr3.Read())
                                                {
                                                    haveCurrencyCards = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                }

            }
            return haveCurrencyCards;
        }
        internal static bool IsDAHKAvailability(string accountNumber)
        {
            bool DAHKAvailability = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select top 1 Arm_number from [tbl_all_accounts;] ac inner join Tbl_define_sint_acc sa on ac.type_of_account=sa.sint_acc and ac.type_of_account_new=sa.sint_acc_new 
                                                  where Arm_number=@accountNumber and sa.type_of_product=61 and sa.type_of_account=24 and dbo.fnc_check_DAHK_availability(ac.customer_number)=0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DAHKAvailability = true;
                        }
                    }

                }

            }
            return DAHKAvailability;
        }
        internal static bool HaveTaxInspectorateApproval(string accountNumber)
        {
            bool TaxInspectorateApproval = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select account_number from  Tbl_accounts_for_TaxInspectorate_approval where [status]<>1 AND account_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            TaxInspectorateApproval = true;
                        }
                    }
                }

            }
            return TaxInspectorateApproval;
        }


        internal static Account GetProductAccount(ulong productId, ushort productType, ushort accountType)
        {
            Account account = new Account();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fnc_get_product_account_from_group(@appID,@productType,@accountType) account_number", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@productType", SqlDbType.Int).Value = productType;
                    cmd.Parameters.Add("@accountType", SqlDbType.Int).Value = accountType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string accountNumber = dr["account_number"].ToString();
                            if (accountNumber != "0")
                            {
                                account = Account.GetAccount(accountNumber);
                            }

                        }
                    }

                }
            }

            return account;
        }
        internal static bool IsUserAccounts(ulong customerNumber, string debitAccountNumber, string recieverAccountNumber)
        {
            bool checkUser = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select top 1 customer_number from [tbl_all_accounts;] where customer_number=@customerNumber and (arm_number=@debitAccountNumber or arm_number=@receiverAccountNumber)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@debitAccountNumber", SqlDbType.Float).Value = debitAccountNumber;
                    cmd.Parameters.Add("@receiverAccountNumber", SqlDbType.Float).Value = recieverAccountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            checkUser = true;
                        }
                    }
                }
            }
            return checkUser;
        }

        internal static bool CheckAccountForDAHK(string accountNumber)
        {
            bool checkDAHK = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT Arm_number FROM [tbl_all_accounts;] WHERE Arm_number=@accountNumber and dbo.fnc_check_DAHK_availability(customer_number)=0", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            checkDAHK = true;
                        }
                    }
                }
            }
            return checkDAHK;
        }


        internal static List<AdditionalDetails> GetAccountAdditions(string accountNumber)
        {
            List<AdditionalDetails> additions = new List<AdditionalDetails>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT a.Key_num, a.AdditionID, a.AdditionValue 
                                  FROM 
                                          Tbl_all_accounts_AddInf a
                                       INNER JOIN 
                                          Tbl_type_of_all_acc_additions t
                                       ON a.AdditionID = t.AdditionId
                                  WHERE a.arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AdditionalDetails oneAddition = new AdditionalDetails
                            {
                                Id = long.Parse(dr["Key_num"].ToString()),
                                AdditionType = ushort.Parse(dr["AdditionID"].ToString()),
                                AdditionValue = Utility.ConvertAnsiToUnicode(dr["AdditionValue"].ToString())
                            };
                            additions.Add(oneAddition);
                        }
                    }
                }

            }

            return additions;
        }



        /// <summary>
        /// Վերդարձնում է նոր ավանդի համար հաշիվները կախված հաշվի տեսակից
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<Account> GetAccountsForNewDeposit(DepositOrder order)
        {
            List<Account> accounts = new List<Account>();

            //անհատական ավանդ
            if (order.AccountType == 1)
            {

                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd1 = new SqlCommand(@"Select dbo.From_CustomerNumberSintAcc(@customer_number,10,24,1)", conn))
                    {
                        cmd1.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                        var genAccNew = cmd1.ExecuteScalar();
                        using (SqlCommand cmd = new SqlCommand(@" SELECT  a.arm_number  
	                                                            FROM [Tbl_all_accounts;] a 
	                                                            LEFT JOIN V_CurrentAcc_SintNew v ON a.type_of_account_new=v.sint_acc_new
	                                                            LEFT JOIN Tbl_Co_accounts_main co
	                                                            on a.arm_number=co.arm_number
	                                                            WHERE customer_number=@customer_number AND 
				                                                            ((@genAccNew is null and v.sint_acc_new is not null) or type_of_account_new =@genAccNew) AND
				                                                            currency=@currency AND
				                                                            account_type<>4 
				                                                            AND (co.arm_number is null or (co.closing_date is not null and co.type=2))
				                                                            and (a.closing_date is null)
	                                                            ORDER BY 
		                                                            case 
			                                                            when a.closing_date is null then 0 
			                                                            else 1 
		                                                            end,
		                                                            case 
			                                                            when filialcode=@FilialCode then 0
			                                                            else	filialcode
		                                                            end,
		                                                            open_date, a.arm_number", conn))
                        {
                            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                            cmd.Parameters.Add("@genAccNew", SqlDbType.NVarChar, 10).Value = genAccNew.ToString();
                            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                            cmd.Parameters.Add("@FilialCode", SqlDbType.SmallInt).Value = 0;
                            dt.Load(cmd.ExecuteReader());
                            if (dt.Rows.Count != 0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    Account account = GetAccount(dt.Rows[i]["arm_number"].ToString());
                                    accounts.Add(account);
                                }

                            }
                        }


                    }

                }
            }

            //համտեղ և հօգուտ 3-րդ անձի ավանդ
            if (order.AccountType == 2 || order.AccountType == 3)
            {

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    DataTable dt = new DataTable();
                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add("value");
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"Select arm_number From dbo.fn_GetJointAccounts(@jointType,@productType,@currency,@customer_number,@tbl)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                        cmd.Parameters.Add("@jointType", SqlDbType.TinyInt).Value = order.AccountType - 1;
                        cmd.Parameters.Add("@productType", SqlDbType.SmallInt).Value = 10;
                        dt2.Rows.Add(order.CustomerNumber.ToString());
                        for (int i = 0; i < order.ThirdPersonCustomerNumbers.Count; i++)
                        {
                            dt2.Rows.Add(order.ThirdPersonCustomerNumbers[i].Key.ToString());
                        }

                        SqlParameter prm = new SqlParameter("@tbl", SqlDbType.Structured)
                        {
                            Value = dt2,
                            TypeName = "dbo.BigintTable"
                        };
                        cmd.Parameters.Add(prm);
                        dt.Load(cmd.ExecuteReader());
                        if (dt.Rows.Count != 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                Account account = GetAccount(dt.Rows[i]["arm_number"].ToString());
                                accounts.Add(account);
                            }

                        }
                    }

                }

            }
            return accounts;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        internal static decimal GetAcccountAvailableBalance(string accountNumber, string creditAccountNumber = "0")
        {
            decimal accountAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fnc_available_AccountAmount_For_24_7(@account,dbo.get_oper_day(),default,@attentionNotFreezed,@credit_account, 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@attentionNotFreezed", SqlDbType.Bit).Value = true;
                    cmd.Parameters.Add("@credit_account", SqlDbType.Float).Value = creditAccountNumber;
                    accountAmount = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return accountAmount;
        }


        internal static async Task<decimal> GetAcccountAvailableBalanceAsync(string accountNumber, string creditAccountNumber = "0")
        {
            decimal accountAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fnc_available_AccountAmount_For_24_7(@account,dbo.get_oper_day(),default,@attentionNotFreezed,@credit_account, 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@attentionNotFreezed", SqlDbType.Bit).Value = true;
                    cmd.Parameters.Add("@credit_account", SqlDbType.Float).Value = creditAccountNumber;
                    accountAmount = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
                }
            }
            return accountAmount;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լիազորված անձանց
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<OPPerson> GetAccountAssigneeCustomers(string accountNumber, OrderType orderType)
        {
            List<OPPerson> list = new List<OPPerson>();
            short operationGroup = Account.GetAssigneeOperationGroup(orderType);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"pr_GetAssignes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@operationGroup", SqlDbType.TinyInt).Value = operationGroup;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            OPPerson opPerson = new OPPerson
                            {
                                CustomerNumber = ulong.Parse(dr["assignee_CustNumber"].ToString()),
                                PersonName = Utility.ConvertAnsiToUnicode(dr["name"].ToString()),
                                PersonLastName = Utility.ConvertAnsiToUnicode(dr["lastname"].ToString()),
                                PersonDocument = Utility.ConvertAnsiToUnicode(dr["passport"].ToString()),
                                PersonSocialNumber = Utility.ConvertAnsiToUnicode(dr["social_number"].ToString()),
                                AssignId = long.Parse(dr["Assign_Id"].ToString())
                            };
                            list.Add(opPerson);
                        }
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        internal static double GetAcccountAvailableBalance(string accountNumber, DateTime setDate, string currency = null, bool attentionNotFreezed = true)
        {
            double accountAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fnc_available_AccountAmount(@account,@set_date,@currency,@attentionNotFreezed,0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = setDate;
                    cmd.Parameters.Add("@attentionNotFreezed", SqlDbType.Bit).Value = attentionNotFreezed;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar).Value = currency;
                    accountAmount = Convert.ToDouble(cmd.ExecuteScalar());
                }


            }
            return accountAmount;
        }

        public static string GetNotFreezedCurrentAccount(ulong customerNumber, string currency)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.From_CustomerNumberCurrentAccountNotFreezed(@customer_number,@currency)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    return Convert.ToString(cmd.ExecuteScalar());
                }

            }
        }

        public static bool HaveCurrentAccountByCurrency(ulong customerNumber, string currency)
        {
            bool haveCurrentAccount = false;
            string accountNumber = "0";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.From_CustomerNumberCurrentAccount(@customerNumber,@currency)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    accountNumber = cmd.ExecuteScalar().ToString();
                }
            }

            if (accountNumber != "0" && accountNumber != "")
            {
                haveCurrentAccount = true;
            }
            return haveCurrentAccount;
        }

        internal static Account GetOperationSystemAccount(uint accountType, string operationCurrency, ushort operationFilialCode, ushort customerType = 0, string debitAccountNumber = "0", string utilityBranch = "", ulong customerNumber = 0)
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_get_operation_account", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@accountType", SqlDbType.Int).Value = accountType;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = operationCurrency;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = operationFilialCode;
                    cmd.Parameters.Add("@customerType", SqlDbType.Int).Value = customerType;
                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = debitAccountNumber;
                    //Աղբահանության կրեդիտ հաշիվը որոշոլու համար անհրաժեշտ է նաև կոմունալի մասնաճյուղը
                    if (accountType == 46 || accountType == 3017)
                    {
                        cmd.Parameters.Add("@utilityBranch", SqlDbType.NVarChar, 20).Value = utilityBranch;
                    }


                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    SqlParameter param = new SqlParameter("@account", SqlDbType.Float)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    accountNumber = cmd.Parameters["@account"].Value.ToString();
                }

            }

            return GetSystemAccount(accountNumber);
        }


        /// <summary>
        /// Վերադարձնում է ՀՀ հիմնադրամի հաշիվը
        /// </summary>
        /// <returns></returns>
        public static Account GetRAFoundAccount()
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select dbo.system_account(@number,@filialCode,0) as Arm_number", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = 80500;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = 22000;

                    cmd.ExecuteNonQuery();

                    accountNumber = cmd.ExecuteScalar().ToString();
                }

            }

            return GetSystemAccount(accountNumber);

        }

        internal static bool HasPensionApplication(string accountNumber)
        {
            bool HasPensionApplication = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select contract_id from Tbl_pension_application where closing_date is null and quality<>10 and account_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            HasPensionApplication = true;
                        }
                    }

                }

            }
            return HasPensionApplication;
        }

        internal static Account GetOperationSystemAccount(OperationAccountHelper helper)
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_get_operation_account", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@accountType", SqlDbType.Int).Value = helper.AccountType;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = helper.Currency;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = helper.OperationFilialCode;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Int).Value = helper.CustomerNumber;
                    cmd.Parameters.Add("@customerType", SqlDbType.Int).Value = helper.CustomerType;
                    cmd.Parameters.Add("@customerResidence", SqlDbType.Int).Value = helper.CustomerResidence;
                    cmd.Parameters.Add("@utilityBranch", SqlDbType.Int).Value = helper.Utilitybranch;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.Int).Value = helper.CardNumber;
                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = helper.DebetAccount;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Int).Value = helper.CreditAccount;
                    cmd.Parameters.Add("@priceIndex", SqlDbType.Int).Value = helper.PriceIndex;
                    cmd.Parameters.Add("@appID", SqlDbType.Int).Value = helper.AppID;

                    SqlParameter param = new SqlParameter("@account", SqlDbType.Float)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    accountNumber = cmd.Parameters["@account"].Value.ToString();
                }

            }

            return GetSystemAccount(accountNumber);
        }




        /// <summary>
        /// Վերադարձնում է հաշվի հասանելի մնացորդը
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <returns></returns>
        internal static decimal GetAcccountAvailableBalanceNotFreezed(string accountNumber, string currency)
        {
            decimal accountAmount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("fun_return_balance_notfreezed", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@num", SqlDbType.BigInt).Value = accountNumber;
                    cmd.Parameters.Add("@cur", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@st", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();


                    if (cmd.ExecuteScalar() != DBNull.Value)
                    {
                        accountAmount = Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }

            }
            return accountAmount;
        }


        /// <summary>
        /// Վերադարձնում է հաշվի բացման աղբյուրը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static SourceType GetAccountSource(string accountNumber, ulong customerNumber)
        {
            SourceType source = new SourceType();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT source_type FROM tbl_hb_documents HB
                                                    INNER JOIN (SELECT arm_number,AdditionValue FROM Tbl_all_accounts_AddInf WHERE AdditionID=11) AD on HB.doc_id=AD.AdditionValue 
                                                    WHERE customer_number=@customer_number and arm_number=@accountNumber", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    if (cmd.ExecuteScalar() != null)
                    {
                        source = (SourceType)Convert.ToInt16(cmd.ExecuteScalar().ToString());
                    }
                    else
                    {
                        source = SourceType.Bank;
                    }
                }


            }
            return source;
        }

        public static string GetSpesialPriceMessage(string accountNumber, short additionID)
        {
            string message = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fn_get_account_special_price_message(@armNumber,@additionID)", conn))
                {
                    cmd.Parameters.Add("@armNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@additionID", SqlDbType.SmallInt).Value = additionID;
                    if (cmd.ExecuteScalar() != null)
                    {
                        message = cmd.ExecuteScalar().ToString();
                    }
                }

            }
            return Utility.ConvertAnsiToUnicode(message);
        }

        internal static async Task<List<Account>> GetJointDepositAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = "";

            script = @"SELECT a.open_date,acc.customer_number,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 Inner Join [tbl_all_accounts;] acc
											 On a.Arm_number=acc.Arm_number
                                             inner join [tbl_deposits;] d
											 on acc.arm_number = d.deposit_full_number
                                              WHERE  a.Closing_date is null 
                                             And a.Customer_Number =@customerNumber  and Co_Type<>0
											 And a.customer_number<>acc.customer_number and d.quality = 1
                                             Order by a.open_date";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }


        }


        /// <summary>
        /// Ստուգում է օգտագործողի հասանելիությունը հաշվի մնացորդի վերաբերյալ
        /// </summary>
        /// <param name="accountNumber">Հաշվեհամար</param>
        /// <param name="accountGroup">Օգտագործողի հասանելիության խումբ</param>
        /// <returns></returns>
        public static bool AccountAccessible(string accountNumber, long accountGroup)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select NULL From  [tbl_all_accounts;] a 
									                            INNER JOIN
									                            Tbl_Accounts_Groups_Permissions g
									                            on a.account_access_group=g.access_level
									                            where group_id=@access_level  and a.Arm_number=@acc_number", conn))
                {
                    cmd.Parameters.Add("@access_level", SqlDbType.BigInt).Value = accountGroup;
                    cmd.Parameters.Add("@acc_number", SqlDbType.Float).Value = accountNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        return true;
                    }
                    else
                        return false;
                }


            }

        }


        public static List<AccountClosingHistory> GetAccountClosinghistory(ulong customerNumber)
        {
            List<AccountClosingHistory> historys = new List<AccountClosingHistory>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"	SELECT 
                                                           Acc_Number , 
                                                           Tbl_Closing_History.Closing_date,
                                                           Tbl_Closing_History.number_of_set, 
                                                           Tbl_Acc_Closing_Descriptions.Decsription +  ', '  + 
                                                           Tbl_Closing_History.closing_reason_text as description, 
                                                           Tbl_Closing_History.Idx_Closing_Description, 
                                                           Tbl_Closing_History.customer_number,
                                                           Tbl_Closing_History.HB_doc_ID 
                                                           FROM Tbl_Closing_History 
                                                           left join Tbl_Acc_Closing_Descriptions 
                                                           on Tbl_Acc_Closing_Descriptions.Idx_Closing=Tbl_Closing_History.Idx_Closing_Description 
	                                                       WHERE Tbl_Closing_History.customer_number=@customerNumber", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            AccountClosingHistory history = new AccountClosingHistory
                            {
                                AccountNumber = dt.Rows[i]["Acc_Number"].ToString(),
                                ClosingDate = dt.Rows[i]["Closing_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Closing_date"]) : default(DateTime?),
                                CustomerNumber = Convert.ToUInt64(dt.Rows[i]["customer_number"]),
                                Description = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()),
                                ReasonType = Convert.ToInt16(dt.Rows[i]["Idx_Closing_Description"]),
                                SetNumber = Convert.ToInt32(dt.Rows[i]["number_of_set"]),
                                HBDocId = Convert.ToUInt64(dt.Rows[i]["HB_doc_ID"])
                            };
                            historys.Add(history);
                        }

                    }
                }

                return historys;
            }
        }



        internal static Account GetCardServiceFeeAccount(ulong appId, int accountType)
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select	dbo.Fnc_GetCreditLinePercentAccount(@app_id,@accountType)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = appId;
                    cmd.Parameters.Add("@accountType", SqlDbType.Float).Value = accountType;

                    accountNumber = cmd.ExecuteScalar().ToString();
                }
            }

            return GetSystemAccount(accountNumber);
        }

        internal static Account GetCardServiceFeeAccountNew(ulong appId)
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select	dbo.fn_get_profict_account_for_card_service_fee(@app_id)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = appId;

                    accountNumber = cmd.ExecuteScalar().ToString();
                }
            }

            return GetSystemAccount(accountNumber);
        }



        internal static Account GetAparikTexumAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber and a.customer_number=@customerNumber
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;
        }

        internal static Account GetClosedAparikTexumAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_closed_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber and a.customer_number=@customerNumber
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;
        }



        internal static Account GetAparikTexumAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber 
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;
        }

        internal static async Task<Account> GetAparikTexumAccountAsync(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber 
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = await SetAccountAsync(row);
                    }
                }

            }
            return account;
        }

        internal static Account GetClosedAparikTexumAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_closed_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber 
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;
        }

        internal static async Task<Account> GetClosedAparikTexumAccountAsync(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @" Select a.open_date,a.account_access_group,0 as co_type,CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END as type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date From 
                    Tbl_closed_paid_factoring  p
                    Inner Join
                    Tbl_Products_Accounts_Groups g
                    on p.App_Id=g.App_ID
                    Inner Join 
                    Tbl_Products_Accounts pa
                    on g.Group_ID=pa.group_id
                    Inner JOin
                    [tbl_all_accounts;] a
                    on pa.Account_number=a.Arm_number
                    INNER JOIN
                    Tbl_type_of_products t ON 
                     CASE loan_type WHEN 33 THEN 54 WHEN 38 THEN 58 END  = t.code 
                    where pa.account_number=@accountNumber 
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = await SetAccountAsync(row);
                    }
                }

            }
            return account;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի շարժը
        /// </summary>
        /// <param name="accountNumber">հաշվեհամար</param>
        /// <param name="startDate">սկիզբ</param>
        /// <param name="endDate">վերջ</param>
        /// <returns></returns>
        internal static AccountFlowDetails GetAccountFlowDetails(string accountNumber, DateTime startDate, DateTime endDate)
        {
            AccountFlowDetails accountFlowDetails = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string sql = @"  Select  sum( case debit_credit
														when 'c' then amount
														when 'd' then -amount
														end ) as balance_amd ,
	                            sum( case debit_credit
														when 'c' then amount_currency
														when 'd' then -amount_currency
														end ) balance_cur ,
                                sum( case debit_credit
														when 'c' then 
														case 
																	when date_of_accounting>=@end_date and date_of_accounting<=@start_date
																		and not (current_account_number = 999 and operation_type = 975) then amount_currency 
														else 0 end
														when 'd'  then 0
														end ) as credit_cur,

	                            sum( case debit_credit
														when 'd' then 
													    case 
																    when date_of_accounting>=@end_date and date_of_accounting<=@start_date 
																	    and not (current_account_number = 999 and operation_type = 975) then amount_currency 
													    else 0 end
														when 'c' then  0
														end ) as debet_cur,
		                        sum( case debit_credit
														when 'c' then 
														case 
																	when date_of_accounting>=@end_date and date_of_accounting<=@start_date 
																		and not (current_account_number = 999 and operation_type = 975) then amount
														else 0 end
														when 'd'  then 0
														end )  as credit_amd,

	                            sum( case debit_credit
														when 'd' then 
														case 
																	when date_of_accounting>=@end_date and date_of_accounting<=@start_date 
																	and not (current_account_number = 999 and operation_type = 975) then amount
														else 0 end
														when 'c' then  0
														end ) as debet_amd
			                    from [tbl_accounting_operations;]  
			                    where  date_of_accounting<=@start_date and current_account_full_number=@accountNumber  
                   ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = startDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = endDate;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        accountFlowDetails = new AccountFlowDetails();
                        if (dt.Rows[0]["balance_amd"] != DBNull.Value)
                            accountFlowDetails.BallanceAMD = Convert.ToDouble(dt.Rows[0]["balance_amd"].ToString());
                        if (dt.Rows[0]["balance_cur"] != DBNull.Value)
                            accountFlowDetails.BallanceInCurrency = Convert.ToDouble(dt.Rows[0]["balance_cur"].ToString());
                        if (dt.Rows[0]["credit_amd"] != DBNull.Value)
                            accountFlowDetails.CreditAmountAMD = Convert.ToDouble(dt.Rows[0]["credit_amd"].ToString());
                        if (dt.Rows[0]["credit_cur"] != DBNull.Value)
                            accountFlowDetails.CreditInCurrency = Convert.ToDouble(dt.Rows[0]["credit_cur"].ToString());
                        if (dt.Rows[0]["debet_amd"] != DBNull.Value)
                            accountFlowDetails.DebitAmountAMD = Convert.ToDouble(dt.Rows[0]["debet_amd"].ToString());
                        if (dt.Rows[0]["debet_cur"] != DBNull.Value)
                            accountFlowDetails.DebitInCurrency = Convert.ToDouble(dt.Rows[0]["debet_cur"].ToString());
                        accountFlowDetails.InitiativeBallanceAMD = accountFlowDetails.BallanceAMD - accountFlowDetails.CreditAmountAMD + accountFlowDetails.DebitAmountAMD;
                        accountFlowDetails.InitiativeBallanceCurrency = accountFlowDetails.BallanceInCurrency - accountFlowDetails.CreditInCurrency + accountFlowDetails.DebitInCurrency;



                    }
                }
            }
            return accountFlowDetails;
        }




        /// <summary>
        /// Վերադարձնում է փակված ավանդների բաց հաշիվները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<Account> GetClosedDepositAccountList(DepositOrder order)
        {
            List<Account> accounts = new List<Account>();



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                DataTable dt = new DataTable();
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select arm_number From dbo.fn_get_deposit_not_used_account(@customer_number,@cur,@filialcode,@account_type,@cust_numbers)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@cur", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@filialcode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@account_type", SqlDbType.SmallInt).Value = order.AccountType;
                    string customerNumbers = "";
                    if (order.ThirdPersonCustomerNumbers != null && order.ThirdPersonCustomerNumbers.Count > 0)
                    {
                        for (int i = 0; i < order.ThirdPersonCustomerNumbers.Count; i++)
                        {
                            customerNumbers = order.ThirdPersonCustomerNumbers[i].Key.ToString() + " ,";
                        }
                    }
                    if (!string.IsNullOrEmpty(customerNumbers))
                        customerNumbers = customerNumbers.Substring(0, customerNumbers.Length - 1);

                    cmd.Parameters.Add("@cust_numbers", SqlDbType.VarChar, 1000).Value = customerNumbers;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Account account = GetAccount(dt.Rows[i]["arm_number"].ToString());
                            accounts.Add(account);
                        }

                    }
                }

            }
            return accounts;
        }



        internal static List<Account> GetAccountListForCardRegistration(ulong customerNumber, string cardCurrency, int cardFilial)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = "";

                script = @"SELECT  [arm_number] , currency, type_of_account,type_of_account_new, description, account_type, card_number,customer_number,closing_date,filialcode
                            FROM [Tbl_all_accounts;]  acc WHERE 
                            customer_number= @customerNumber   and currency=@currency  and account_type = 4 and closing_date Is Null 
                            and [arm_number] Not In (SELECT card_account FROM tbl_visa_numbers_accounts WHERE card_account=[arm_number] and closing_date is null) 
                            and type_of_account_new= dbo.From_CustomerNumberSintAcc(@customerNumber ,11 ,24, 1)
                            and filialcode - 22000 = @cardFilial";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = cardCurrency;
                    cmd.Parameters.Add("@cardFilial", SqlDbType.Int).Value = cardFilial;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);

                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        //Account account = SetAccount(row);
                        Account account = new Account();

                        if (row != null)
                        {
                            account.OpenDate = default(DateTime?);
                            account.ClosingDate = default(DateTime?);
                            account.FreezeDate = default(DateTime?);

                            account.AccountNumber = row["Arm_number"].ToString();
                            account.Currency = row["Currency"].ToString();
                            account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                            account.ProductNumber = row["card_number"].ToString();
                            account.FilialCode = int.Parse(row["filialcode"].ToString());
                            if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                            {
                                account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                            }

                        }

                        accountList.Add(account);
                    }
                }
            }
            return accountList;
        }

        internal static List<Account> GetOverdraftAccountsForCardRegistration(ulong customerNumber, string cardCurrency, int cardFilial)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = "";

                script = @"SELECT  [arm_number], currency, type_of_account,type_of_account_new, description, account_type, card_number,customer_number,closing_date,filialcode
                                FROM [Tbl_all_accounts;] acc WHERE 
                                customer_number= @customerNumber   and currency= @currency  and account_type = 4 and closing_date Is Null 
                                and Not exists (select overdraft_account from tbl_visa_numbers_accounts where overdraft_account=[arm_number] and closing_date is null) 
                                and Not exists (select loan_account from tbl_visa_numbers_accounts where loan_account=[arm_number] and closing_date is null) 
                                and type_of_account_new= dbo.From_CustomerNumberSintAcc(@customerNumber ,4,1,1)  
                                and filialcode - 22000 = @cardFilial";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = cardCurrency;
                    cmd.Parameters.Add("@cardFilial", SqlDbType.Int).Value = cardFilial;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        //Account account = SetAccount(row);
                        Account account = new Account();

                        if (row != null)
                        {
                            account.OpenDate = default(DateTime?);
                            account.ClosingDate = default(DateTime?);
                            account.FreezeDate = default(DateTime?);

                            account.AccountNumber = row["Arm_number"].ToString();
                            account.Currency = row["Currency"].ToString();
                            account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                            account.ProductNumber = row["card_number"].ToString();
                            account.FilialCode = int.Parse(row["filialcode"].ToString());
                            if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                            {
                                account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                            }

                        }

                        accountList.Add(account);
                    }
                }
            }
            return accountList;
        }



        public static Account GetSystemAccountByNN(uint nn, uint filialCode)
        {
            string accountNumber = "0";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select dbo.system_account(@number,@filialCode,0) as Arm_number", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = nn;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;

                    cmd.ExecuteNonQuery();

                    accountNumber = cmd.ExecuteScalar().ToString();
                }

            }

            return GetSystemAccount(accountNumber);

        }

        internal static List<Account> GetATSSystemAccounts(string currency, uint filialCode)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(systemAccountSelectScript + @" WHERE 
                                                                               account_type=7 AND filialcode=@filialCode 
                                                                               AND currency=@currency
                                                                               AND type_of_account like '1013%' 
                                                                               AND type_of_account_new like '1000200%'
                                                                               AND closing_date IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            Account account = SetAccount(row);

                            accountList.Add(account);
                        }
                    }
                }

            }

            return accountList;

        }

        internal static List<Account> GetATSSystemAccounts(uint filialCode)
        {
            List<Account> accountList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(systemAccountSelectScript + @" WHERE 
                                                                               account_type=7 AND filialcode=@filialCode 
                                                                               AND closing_date IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            Account account = new Account(row["Arm_number"].ToString());

                            accountList.Add(account);
                        }
                    }
                }

            }

            return accountList;

        }


        internal static bool HasATSSystemAccountInFilial(uint filialCode)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT Arm_number FROM [tbl_all_accounts;]
                                                                               WHERE 
                                                                               account_type=7 AND filialcode=@filialCode 
                                                                               AND closing_date IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return true;
                        }
                        else
                            return false;
                    }

                }

            }


        }



        internal static List<Account> GetTransitAccountsForDebitTransactions(uint filialCode)
        {
            List<Account> accounts = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(transitAccountsForDebitTransactionsSelectScript + @" WHERE (tr.filial_code = @filialCode OR tr.for_all_branches=1) and tr.closing_date is null", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            Account account = SetAccount(row);
                            accounts.Add(account);
                        }
                    }
                }

            }

            return accounts;

        }

        internal static Account GetProductAccountFromCreditCode(string creditCode, ushort productType, ushort accountType)
        {
            Account account = new Account();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fn_get_product_account_from_credit_code(@creditCode,@productType,@accountType) account_number", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@creditCode", SqlDbType.VarChar, 16).Value = creditCode;
                    cmd.Parameters.Add("@productType", SqlDbType.Int).Value = productType;
                    cmd.Parameters.Add("@accountType", SqlDbType.Int).Value = accountType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string accountNumber = dr["account_number"].ToString();
                            if (accountNumber != "0")
                            {
                                account = Account.GetAccount(accountNumber);
                            }

                        }
                    }
                }
            }

            return account;
        }


        internal static Account GetJointAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT a.open_date,acc.customer_number,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 10) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											    Tbl_type_of_products t ON type_of_product = t.code 
											    INNER JOIN [tbl_all_accounts;] acc
											    ON a.Arm_number=acc.Arm_number
                                                WHERE  a.Closing_date is null 
                                                AND a.Customer_Number =@customerNumber  and Co_Type<>0 and a.arm_number=@accountNumber 
											    AND a.customer_number<>acc.customer_number
                                                ORDER BY a.open_date", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }



        internal static Account GetJointDepositAccount(string accountNumber, ulong customerNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT a.open_date,acc.customer_number,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,A.Co_Type,a.closing_date,A.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group FROM
                                                V_All_Accounts A INNER JOIN   (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE(type_of_product = 13) AND(type_of_account = 24) GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 Inner Join [tbl_all_accounts;] acc
											 On a.Arm_number=acc.Arm_number
                                              WHERE  a.Closing_date is null 
                                             And a.Customer_Number =@customerNumber  and a.arm_number=@accountNumber and Co_Type<>0
											 And a.customer_number<>acc.customer_number
                                             Order by a.open_date", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }


        internal static string GetAccountDescription(string accountNumber)
        {
            string accountDescription = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT description FROM [tbl_all_accounts;]
                                                    WHERE Arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            accountDescription = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                        }
                    }

                }
            }

            return accountDescription;
        }

        internal static List<AccountOpeningClosingDetail> GetAccountOpeningClosingDetails(string accountNumber)
        {
            List<AccountOpeningClosingDetail> list = new List<AccountOpeningClosingDetail>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT  AL.opened_by,CH.number_of_set,ch.closing_date,CH.Idx_Closing_Description,CD.Decsription as ClossingDescription 
                                                    FROM [tbl_all_accounts;] AL 
                                                    inner JOIN 
                                                    Tbl_Closing_History CH
                                                    ON AL.arm_number = CH.Acc_Number 
                                                    inner JOIN Tbl_Acc_Closing_Descriptions CD
                                                    ON CH.Idx_Closing_Description=CD.Idx_Closing 
                                                    WHERE AL.arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            AccountOpeningClosingDetail acAction = SetAccountOpeningClosingDetail(row);
                            list.Add(acAction);
                        }
                    }
                }

                return list;

            }


        }

        internal static AccountOpeningClosingDetail GetAccountOpeningDetail(string accountNumber)
        {
            AccountOpeningClosingDetail openingDetail = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT  opened_by as number_of_set FROM [tbl_all_accounts;]
                                                    WHERE arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        openingDetail = SetAccountOpeningClosingDetail(row);
                    }
                }


            }
            return openingDetail;



        }




        private static AccountOpeningClosingDetail SetAccountOpeningClosingDetail(DataRow row)
        {
            AccountOpeningClosingDetail acOpeningClosingDetail = new AccountOpeningClosingDetail();
            if (row != null)
            {
                acOpeningClosingDetail.ActionSetNumber = int.Parse(row["number_of_set"].ToString());

                if (row.Table.Columns.Contains("closing_date") && row["closing_date"] != DBNull.Value)
                {
                    acOpeningClosingDetail.ActionDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }
                if (row.Table.Columns.Contains("ClossingDescription") && row["ClossingDescription"] != DBNull.Value)
                {
                    acOpeningClosingDetail.ActionDescription = Utility.ConvertAnsiToUnicode(row["ClossingDescription"].ToString());

                }

            }
            return acOpeningClosingDetail;
        }

        public static object GetTransactionDescriptionForSwiftMessage(double transactionsGroupNumber, char debitCredit)
        {
            dynamic details = new ExpandoObject();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                sqlConnection.Open();


                using SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.CommandText = "SELECT * FROM dbo.fn_get_transaction_description_for_swift_message(@transactionsGroupNumber, @debitCredit)";
                sqlCommand.Connection = sqlConnection;

                sqlCommand.Parameters.Add("@transactionsGroupNumber", SqlDbType.BigInt).Value = transactionsGroupNumber;
                sqlCommand.Parameters.Add("@debitCredit", SqlDbType.VarChar, 1).Value = debitCredit;

                using DataTable dataTable = new DataTable();

                using SqlDataReader dataReader = sqlCommand.ExecuteReader();
                
                    dataTable.Load(dataReader);
                

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    details.IsAmundiAccount = Convert.ToBoolean(row["is_amundi_account"]);
                    details.Reference1 = row["reference1"].ToString();
                    details.Reference2 = row["reference2"].ToString();
                    details.Description = row["description"].ToString();

                }


            }

            return details;
        }

        public static bool CheckForTransactions(Account account, DateTime dateFrom, DateTime dateTo)
        {
            bool result = false;

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                sqlConnection.Open();

                using SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.CommandText = @"DECLARE @result BIT 
                                                        EXEC pr_check_for_transactions @account, @startDate, @endDate, @result OUTPUT 
                                                        SELECT @result as result";
                sqlCommand.Connection = sqlConnection;

                sqlCommand.Parameters.Add("@account", SqlDbType.Float).Value = account.AccountNumber;
                sqlCommand.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = dateFrom;
                sqlCommand.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = dateTo;
                //SqlParameter parameter = new SqlParameter("@result", SqlDbType.Bit);
                //parameter.Direction = ParameterDirection.Output;
                //sqlCommand.Parameters.Add(parameter);

                DataTable dataTable = new DataTable();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    dataTable.Load(dataReader);
                }

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    result = Convert.ToBoolean(row["result"]);

                }
            }

            return result;
        }

        public static string GetTransactionDescriptionForSwiftMT940(double transactionsGroupNumber, char debitCredit, string description)
        {
            string transactionDescription = "";
            string ORDP = "";
            string BENM = "";
            string REMI = "";
            string ORDPFinal = "";
            string BENMFinal = "";
            string REMIFinal = "";
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                sqlConnection.Open();

                using SqlCommand sqlCommand = new SqlCommand();

                sqlCommand.CommandText = "SELECT * FROM dbo.fn_get_transaction_description_for_swift_MT940(@transactionsGroupNumber, @debitCredit)";
                sqlCommand.Connection = sqlConnection;

                sqlCommand.Parameters.Add("@transactionsGroupNumber", SqlDbType.BigInt).Value = transactionsGroupNumber;
                sqlCommand.Parameters.Add("@debitCredit", SqlDbType.VarChar, 1).Value = debitCredit;

                DataTable dataTable = new DataTable();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    dataTable.Load(dataReader);
                }

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    ORDP = "/ORDP/" + Utility.TranslateToEnglish(row["ORDP"].ToString());
                    BENM = "/BENM/" + Utility.TranslateToEnglish(row["BENM"].ToString());

                    if (Convert.ToInt16(row["transfer_group"]) == 3)
                    {
                        REMI = "/REMI/" + Utility.TranslateToEnglish(row["REMI"].ToString());
                    }
                    else
                    {
                        REMI = "/REMI/" + Utility.TranslateToEnglish(description, true);
                    }




                    if (ORDP.Length > 65)
                    {
                        for (int z = 0; z < ORDP.Length; z += 65)
                        {
                            if ((ORDP.Substring(z)).Length > 65)
                            {
                                ORDPFinal += (ORDP.Substring(z, 65)) + Environment.NewLine;
                            }
                            else
                            {
                                ORDPFinal += (ORDP.Substring(z));
                            }
                        }
                    }
                    else
                    {
                        ORDPFinal = ORDP;
                    }

                    if (BENM.Length > 65)
                    {
                        for (int z = 0; z < BENM.Length; z += 65)
                        {
                            if ((BENM.Substring(z)).Length > 65)
                            {
                                BENMFinal += (BENM.Substring(z, 65)) + Environment.NewLine;
                            }
                            else
                            {
                                BENMFinal += (BENM.Substring(z));
                            }
                        }
                    }
                    else
                    {
                        BENMFinal = BENM;
                    }

                    if (REMI.Length > 65)
                    {
                        for (int z = 0; z < REMI.Length; z += 65)
                        {
                            if ((REMI.Substring(z)).Length > 65)
                            {
                                REMIFinal += (REMI.Substring(z, 65)) + Environment.NewLine;
                            }
                            else
                            {
                                REMIFinal += (REMI.Substring(z));
                            }
                        }
                    }
                    else
                    {
                        REMIFinal = REMI;
                    }

                    transactionDescription = ORDPFinal + Environment.NewLine + BENMFinal + Environment.NewLine + REMIFinal;

                }
            }
            return transactionDescription;
        }

        internal static bool CheckAccountIsClosed(string accountNumber)
        {
            bool check = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"select closing_date from [tbl_all_accounts;] where arm_number=@accountNumber", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (dr["closing_date"] != DBNull.Value)
                    {
                        check = true;
                    }
                }
            }
            return check;
        }

        internal static List<Account> GetCustomerTransitAccounts(ulong customerNumber)
        {
            List<Account> accounts = new List<Account>();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(accountSelectScript + @" INNER JOIN
														(SELECT 21 as type_of_product,description,code,DescriptionEng FROM Tbl_type_of_products) t ON 21 = t.code 
														INNER JOIN Tbl_transit_accounts_for_debit_transactions tr
														ON a.Arm_number=tr.Arm_number
														WHERE 
                                                        a.closing_date IS NULL AND tr.customer_number = @customerNumber AND tr.closing_date IS NULL", conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        accounts.Add(SetAccount(dt.Rows[i]));
                    }

                }
            }

            return accounts;


        }

        internal static List<Account> GetClosedCustomerTransitAccounts(ulong customerNumber)
        {
            List<Account> accounts = null;

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(accountSelectScript + @" INNER JOIN
														(SELECT 21 as type_of_product,description,code,DescriptionEng FROM Tbl_type_of_products) t ON 21 = t.code 
														INNER JOIN Tbl_transit_accounts_for_debit_transactions tr
														ON a.Arm_number=tr.Arm_number
														WHERE 
                                                        a.closing_date IS NOT NULL AND a.customer_number = @customerNumber", conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }



                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    accounts.Add(SetAccount(row));
                }
            }

            return accounts;


        }
        internal static Account GetAccountFromAllAccounts(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(onlyAllAccountSelectScript + @" 
                                             WHERE Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 1)
                    {
                        account = GetCurrentAccount(accountNumber);
                        if (account == null)
                            account = GetCardDahkAccount(accountNumber);
                        if (account == null)
                            account = GetAparikTexumAccount(accountNumber);
                        if (account == null)
                            account = GetClosedAparikTexumAccount(accountNumber);


                        if (account != null)
                        {
                            return account;
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccountFromAllAccounts(row);
                    }
                }

                return account;
            }

        }

        private static Account SetAccountFromAllAccounts(DataRow row)
        {
            Account account = new Account();

            if (row != null)
            {
                account.OpenDate = default(DateTime?);
                account.ClosingDate = default(DateTime?);
                account.FreezeDate = default(DateTime?);

                account.AccountNumber = row["Arm_number"].ToString();
                account.Balance = double.Parse(row["balance"].ToString());
                account.Currency = row["Currency"].ToString();
                account.ProductNumber = row["card_number"].ToString();
                account.FilialCode = int.Parse(row["filialcode"].ToString());
                account.Status = short.Parse(row["account_type"].ToString());
                account.FreezeDate = row["freeze_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["freeze_date"].ToString());
                account.UnUsedAmount = row["UnUsed_amount"] == DBNull.Value ? default(double?) : double.Parse(row["UnUsed_amount"].ToString());

                account.UnUsedAmountDate = row["UnUsed_amount_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["UnUsed_amount_date"].ToString());
                if (row.Table.Columns.Contains("account_access_group"))
                    account.AccountPermissionGroup = row["account_access_group"].ToString();
                else
                    account.AccountPermissionGroup = "0";

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    account.ClosingDate = (DateTime.Parse(row["closing_date"].ToString())).Date;
                }

                if (row.Table.Columns.Contains("Co_type"))
                {
                    account.JointType = ushort.Parse(row["Co_Type"].ToString());
                }
                else
                    account.JointType = 0;
                account.OpenDate = (DateTime.Parse(row["open_date"].ToString())).Date;
                if (row.Table.Columns.Contains("ac_description"))
                {
                    account.AccountDescription = Utility.ConvertAnsiToUnicode(row.Field<String>("ac_description"));
                }

                account.AvailableBalance = Account.GetAcccountAvailableBalance(account.AccountNumber);


            }
            return account;
        }
        internal static List<Account> GetCreditCodesTransitAccounts(ulong customerNumber)
        {
            List<Account> accounts = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT distinct PAcc.type_of_product , PAcc.type_of_Account, type_of_account_new, Arm_number, balance, currency, 
                                                        T.description, Acc.description as ac_description, card_number, filialcode,account_type,freeze_date,
                                                        UnUsed_amount,UnUsed_amount_date,account_access_group,closing_date, open_date
                                                        FROM[tbl_all_accounts;] AS Acc
                                                        LEFT JOIN Tbl_Products_Accounts PAcc ON PAcc.account_number = Acc.arm_number
                                                        JOIN Tbl_type_of_products AS T ON PAcc.type_of_product = T.code
                                                        WHERE
                                                        PAcc.type_of_account IN(224,279) 
                                                        AND closing_date IS NULL
                                                        AND balance > 0
                                                        AND PAcc.account_number not in
                                                        (select  PA.Account_number From Tbl_Products_Accounts_Groups PG  INNER JOIN Tbl_Products_Accounts PA ON PG.Group_ID = PA.Group_Id where group_status = 1)
                                                        AND customer_number = @customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }



                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            accounts.Add(SetAccount(dt.Rows[i]));
                        }

                    }
                }

                return accounts;
            }
        }






        internal static bool HasOrHadAccount(ulong customerNumber)
        {

            bool hasOrHadAccount = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select 1 from [tbl_all_accounts;]where customer_number = @customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        hasOrHadAccount = dr.HasRows;
                    }
                }
            }
            return hasOrHadAccount;
        }


        internal static string GetAccountCurrency(string accountNumber)
        {
            string currency = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select currency from [dbo].[tbl_all_accounts;] where arm_number=@accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["currency"] != DBNull.Value)
                            {
                                currency = dr["currency"].ToString();
                            }
                        }
                    }
                }
            }

            return currency;
        }

        internal static string GetHBDocumentNumber(double transactionsGroupNumber)
        {
            string HBDocumentNumber = "";

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                sqlConnection.Open();

                using SqlCommand sqlCommand = new SqlCommand();

                sqlCommand.CommandText = "SELECT document_number FROM Tbl_HB_products_accordance PA INNER JOIN Tbl_HB_documents D ON D.doc_ID = PA.doc_ID WHERE Transactions_Group_number = @transactionsGroupNumber ";
                sqlCommand.Connection = sqlConnection;

                sqlCommand.Parameters.Add("@transactionsGroupNumber", SqlDbType.BigInt).Value = transactionsGroupNumber;

                DataTable dataTable = new DataTable();

                using SqlDataReader dr = sqlCommand.ExecuteReader();
                if (dr.Read())
                {
                    if (dr["document_number"] != DBNull.Value)
                    {
                        HBDocumentNumber = dr["document_number"].ToString();
                    }
                }
            }
            return HBDocumentNumber;
        }

        internal static string GetDocumentNumber(double transactionsGroupNumber)
        {
            string DocumentNumber = "";

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                sqlConnection.Open();

                using SqlCommand sqlCommand = new SqlCommand();

                sqlCommand.CommandText = "select hb.document_number from Tbl_HB_documents hb inner join dbo.tbl_bank_mail_in bm  on hb.doc_ID = bm.Add_tbl_unic_number where bm.transactions_group_number = @transactionGroupNumber and hb.source_type <> 2 and bm.Add_tbl_name = 'Tbl_Hb_documents' ";
                sqlCommand.Connection = sqlConnection;

                sqlCommand.Parameters.Add("@transactionGroupNumber", SqlDbType.BigInt).Value = transactionsGroupNumber;

                DataTable dataTable = new DataTable();

                using SqlDataReader dr = sqlCommand.ExecuteReader();
                if (dr.Read())
                {
                    if (dr["document_number"] != DBNull.Value)
                    {
                        DocumentNumber = dr["document_number"].ToString();
                    }
                }
            }
            return DocumentNumber;
        }
        internal static string GetAccountCustomerFullNameEng(string accountNumber)
        {
            string customerNameEng = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT C.nameEng+ ' ' + C.LastNameEng as fullNameEng FROM [tbl_all_accounts;] A
                                                        LEFT JOIN V_CustomerDesription C ON C.customer_number = A.customer_number
                                                        WHERE A.arm_number=@accountNumber AND closing_date is null", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["fullNameEng"] != DBNull.Value)
                            {
                                customerNameEng = dr["fullNameEng"].ToString();
                            }
                        }
                    }
                }
            }

            return customerNameEng;
        }

        internal static bool ISDahkCardTransitAccount(string accountNumber)
        {
            bool isDahkAccount = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_Products_Accounts  PA
                                                        INNER JOIN Tbl_Products_Accounts_Groups PG ON PA.Group_Id = PG.Group_ID
                                                        WHERE  type_of_product = 11 and PA.type_of_account = 73  and Group_Status = 1 and PA.Account_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        isDahkAccount = true;

                    }

                }

                return isDahkAccount;
            }
        }




        internal static bool IsPOSAccount(string accountNumber)
        {
            bool isPOSAccount = false;



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select 1 from tbl_arca_points_list  where (account_number=@accountNumber or account_number_usd = @accountNumber 
                                                         or account_number_EUR = @accountNumber) and point_place<>1 and closing_date is null", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isPOSAccount = true;
                        }
                    }
                }
            }
            return isPOSAccount;
        }

        internal static List<Account> GetCredentialOrderFeeAccounts(ulong customerNumber)
        {
            List<Account> accounts = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT a.arm_number 
                                                         FROM   (SELECT arm_number, 
                                                                        balance, 
                                                                        type_of_account_new, 
                                                                        type_of_account, 
                                                                        currency, 
                                                                        description, 
                                                                        customer_number, 
                                                                        [tbl_all_accounts;].closing_date, 
                                                                        filialcode 
                                                                FROM   dbo.[tbl_all_accounts;] 
                                                                UNION 
                                                                SELECT A.arm_number, 
                                                                        A.balance, 
                                                                        A.type_of_account_new, 
                                                                        A.type_of_account, 
                                                                        A.currency, 
                                                                        A.description, 
                                                                        D.co_customer_number AS customer_number, 
                                                                        A.closing_date, 
                                                                        filialcode 
                                                                FROM   dbo.[tbl_all_accounts;] AS A 
                                                                        INNER JOIN (SELECT C.arm_number, 
                                                                                            C.type, 
                                                                                            C.third_person_customer_number, 
                                                                                            Co.customer_number AS co_Customer_number 
                                                                                    FROM   dbo.tbl_co_accounts_main AS C 
                                                                                            INNER JOIN dbo.tbl_co_accounts AS Co 
                                                                                                    ON C.id = Co.co_main_id) AS D 
                                                                                ON A.arm_number = D.arm_number 
                                                                                    AND A.customer_number <> D.co_customer_number) a 
                                                                LEFT OUTER JOIN dbo.tbl_co_accounts_main AS G 
                                                                            ON a.arm_number = G.arm_number 
                                                                LEFT JOIN tbl_hb_products_descriptions PD 
                                                                        ON a.arm_number = PD.unique_id 
                                                        WHERE  A.closing_date IS NULL 
                                                                AND type_of_account_new IN(SELECT sint_acc_new 
                                                                                            FROM   tbl_define_sint_acc 
                                                                                            WHERE  type_of_product = 10 
                                                                                                    AND type_of_account = 24) 
                                                                AND customer_number = @customerNumber 
                                                                AND currency = 'AMD' ", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }



                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            accounts.Add(GetAccount(dt.Rows[i]["arm_number"].ToString()));
                        }
                    }
                }

                return accounts;
            }
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հաշվեհամարի համար գոյություն ունի կցված պայմանագիր, թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool HasUploadedAccountContract(string accountNumber)
        {
            bool hasContract = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sql = @" select d.doc_id as doc_id  from tbl_HB_Attached_Documents AD
								inner join Tbl_HB_documents D
								on D.doc_ID = AD.doc_ID
								where document_type in (7, 12, 17) and debet_account =@accountNumber and quality = 30";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (int.Parse(dr["doc_id"].ToString()) != 0)
                            {
                                hasContract = true;
                            }
                        }
                    }
                }
            }
            return hasContract;
        }

        internal static async Task<List<Account>> GetAccountsDigitalBankingAsync(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                Task<List<Account>> currentAccountList = GetCurrentAccountList(customerNumber, conn);
                Task<List<Account>> jointAccountList = GetJointAccountList(customerNumber, conn);
                Task<List<Account>> bankruptAccountList = GetBankruptAccountList(customerNumber, conn);
                Task<List<Account>> limitedAccounts = GetCurrentAccountListForNonMobileClients(customerNumber, conn);   //Սահմանափակ հասանելիությամ հաշվիներ
                Task<List<Account>> transitAccounts = GetTransitAccountList(customerNumber, conn);
                Task<List<Account>> socialAccount = GetSocialSecurityAccountList(customerNumber, conn);   //Սոցիալական ապահովության հաշիվ
                Task<List<Account>> developerSpecialAccounts = GetDeveloperSpecialAccountsAsync(customerNumber, conn);   //Կառուցապատողի հատուկ հաշիվներ

                accountList.AddRange(await currentAccountList);
                accountList.AddRange(await jointAccountList);
                accountList.AddRange(await bankruptAccountList);
                accountList.AddRange(await limitedAccounts);
                accountList.AddRange(await transitAccounts);
                accountList.AddRange(await socialAccount);
                accountList.AddRange(await developerSpecialAccounts);

                accountList.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null)
                        m.AccountDescription = card.CardNumber + " " + card.CardType;

                });
            }
            return accountList;
        }

        internal static async Task<List<Account>> GetCurrentAccountsForTotalBalance(ulong customerNumber)
        {
            List<Account> accountList = new List<Account>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT  acc.Currency, acc.Arm_number
                                                        FROM V_All_Accounts a 
                                                        Inner Join [tbl_all_accounts;] acc
                                                        On a.Arm_number=acc.Arm_number
                                                        INNER JOIN(
					                                                        SELECT sint_acc_new, type_of_client, type_of_product 
					                                                        FROM  dbo.Tbl_define_sint_acc 
					                                                        WHERE(type_of_product = 10) AND(type_of_account = 24)
                                                                            GROUP BY sint_acc_new, type_of_client, type_of_product)s 
                                                        ON a.type_of_account_new = s.sint_acc_new 
                                                        INNER JOIN Tbl_type_of_products t 
                                                        ON type_of_product = t.code 
                                                        WHERE acc.customer_number = @customerNumber 
			                                                        And acc.closing_date is null  
			                                                        and  a.customer_number=acc.customer_number

                                                        UNION ALL

                                                        SELECT acc.Currency, acc.Arm_number			
                                                        FROM V_All_Accounts A 
                                                        INNER JOIN   (
						                                                        SELECT sint_acc_new, type_of_client, type_of_product 
						                                                        FROM  dbo.Tbl_define_sint_acc
						                                                        WHERE(type_of_product = 10) AND(type_of_account = 24) 
						                                                        GROUP BY sint_acc_new, type_of_client, type_of_product)s
                                                        ON a.type_of_account_new = s.sint_acc_new 
                                                        INNER JOIN Tbl_type_of_products t 
                                                        ON type_of_product = t.code 
                                                        Inner Join [tbl_all_accounts;] acc
                                                        On a.Arm_number=acc.Arm_number
                                                        WHERE  a.Closing_date is null 
                                                                    And a.Customer_Number =@customerNumber  and Co_Type<>0
			                                                        And a.customer_number<>acc.customer_number

                                                        UNION ALL

                                                        SELECT acc.Currency, acc.Arm_number
                                                        FROM V_All_Accounts a 
                                                        Inner Join [tbl_all_accounts;] acc
                                                        On a.Arm_number=acc.Arm_number
                                                        INNER JOIN(
					                                                        SELECT sint_acc_new, type_of_client, type_of_product 
					                                                        FROM  dbo.Tbl_define_sint_acc 
					                                                        WHERE(type_of_product = 116) AND(type_of_account = 24)
                                                                            GROUP BY sint_acc_new, type_of_client, type_of_product)s 
                                                        ON a.type_of_account_new = s.sint_acc_new 
                                                        INNER JOIN Tbl_type_of_products t 
                                                        ON type_of_product = t.code 
                                                        WHERE acc.customer_number = @customerNumber 
			                                                        And acc.closing_date is null  
			                                                        and  a.customer_number=acc.customer_number

                                                        UNION ALL

                                                        SELECT  acc.Currency, acc.Arm_number
                                                        FROM V_All_Accounts a 
                                                        Inner Join [tbl_all_accounts;] acc
                                                        On a.Arm_number=acc.Arm_number
                                                        INNER JOIN(
					                                                        SELECT sint_acc_new,  type_of_product, type_of_account 
					                                                        FROM  dbo.Tbl_define_sint_acc 
					                                                        WHERE(type_of_product = 18) AND(type_of_account = 283)
                                                                            GROUP BY sint_acc_new,  type_of_product, type_of_account)s 
                                                        ON a.type_of_account_new = s.sint_acc_new 
                                                        INNER JOIN	Tbl_type_of_products t 
                                                        ON type_of_product = t.code 
                                                        WHERE acc.customer_number = @customerNumber 
			                                                        And acc.closing_date is null  
			                                                        and  a.customer_number=acc.customer_number", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Account account = new Account();
                        account.Currency = row["currency"].ToString();
                        account.AccountNumber = row["Arm_Number"].ToString();
                        account.AvailableBalance = await Account.GetAcccountAvailableBalanceAsync(account.AccountNumber);
                        accountList.Add(account);
                    }
                }
                return accountList;
            }
        }

        internal static List<string> GetAccountsForServicePaymentChecking(ulong customerNumber)
        {
            List<string> accountNumbers = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"pr_get_accounts_for_service_payment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            accountNumbers.Add(dr["Arm_number"].ToString());
                        }
                    }
                }
            }
            return accountNumbers;
        }

        public static int GetAccountServicingFilialCode(string accountNumber)
        {
            int filialCode = default(int);
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"SELECT TOP 1 filialcode
                                FROM [dbo].[tbl_all_accounts;] 
                                WHERE Arm_number = @accountNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.NVarChar).Value = accountNumber;

                    conn.Open();

                    try
                    {
                        filialCode = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception e)
                    {
                        throw;
                    }

                }
            }
            return filialCode;
        }

        internal static bool IsOurAccount(string accountNumber)
        {
            bool isACBAAccount = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"Select arm_number
										                                                        From [tbl_all_accounts;] 
										                                                        Where closing_date is null and Arm_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isACBAAccount = true;
                        }
                    }
                }
            }
            return isACBAAccount;
        }

        internal static bool HasAccountOrder(string currency, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 doc_id 
                                                         FROM Tbl_HB_Documents
                                                         WHERE document_type = 7 
                                                         AND Quality in (2, 3 , 4, 5, 20, 50, 57, 100)
                                                         AND Customer_number = @customerNumber
                                                         AND Currency = @currency", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return true;
                    }
                }
            }
            return false;
        }

        internal static async Task<List<Account>> GetTransitAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();


            string script = accountSelectScript + @" INNER JOIN
													(SELECT 21 as type_of_product,description,code,DescriptionEng FROM Tbl_type_of_products) t ON 21 = t.code 
													INNER JOIN Tbl_transit_accounts_for_debit_transactions tr
													ON a.Arm_number=tr.Arm_number
													WHERE 
                                                    a.closing_date IS NULL AND tr.customer_number = @customerNumber AND tr.closing_date IS NULL";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }



        }

        internal static bool DAHKRestrictionForCardAccount(string accountNumber)
        {
            bool hasDAHKRestrictions = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT * 
                                                        FROM Tbl_dahk_freeze_details FD 
                                                        INNER JOIN Tbl_acc_freeze_history FH ON FH.ID = FD.freeze_ID 
                                                        INNER JOIN 
                                                        ( SELECT Top 1 visa_number 
	                                                        From tbl_visa_numbers_accounts 
	                                                        WHERE (ISNULL(main_card_number,'0')='0' OR attached_card=2) AND card_account = @accountNumber
                                                          ORDER BY open_date DESC  ) Va ON va.visa_number = FD.card_number 
                                                            WHERE FH.closing_date IS NULL", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hasDAHKRestrictions = true;
                        }
                    }
                }
            }
            return hasDAHKRestrictions;
        }


        internal static DataTable GetCurrentAndCardAccounts(ulong customerNumber)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT acc.Arm_number, acc.type_of_account, acc.Currency,  [dbo].[fnc_convertAnsiToUnicode](t.description) accountDescr 
                           FROM [tbl_all_accounts;] acc
                            INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product 
                                        FROM  dbo.Tbl_define_sint_acc 
                                        WHERE type_of_product in (10, 11) AND type_of_account = 24
                                        GROUP BY sint_acc_new, type_of_client, type_of_product) s ON acc.type_of_account_new = s.sint_acc_new 
                            INNER JOIN Tbl_type_of_products t ON type_of_product = t.code 
                           WHERE acc.customer_number = @customerNumber AND acc.closing_date IS NULL";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }

            return dt;
        }

        private static async Task<List<Account>> GetSocialSecurityAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.description as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 118) AND(type_of_account = 24)                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";
            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    Account account = await SetAccountAsync(row);
                    accountList.Add(account);
                }
            }
            return accountList;
        }

        private static List<Account> GetSocialSecurityClosedAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.description as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 118) AND(type_of_account = 24)                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date IS NOT NULL  AND  a.customer_number=acc.customer_number";
            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        Account account = SetAccount(row);
                        accountList.Add(account);
                    }
                }
            }
            return accountList;
        }

        internal static string GetPensionPaymentCreditAccount(ulong customerNumber)
        {
            string creditAccount = string.Empty;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 card_account as credit_account FROM [dbo].Tbl_Visa_Numbers_Accounts
                                                        where card_type = 21 and closing_date is NULL AND customer_number = @customerNumber
                                                        UNION
                                                        SELECT a.Arm_number as credit_account FROM V_All_Accounts a 
                                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 118) AND(type_of_account = 24)                                           
                                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new 
                                                        INNER JOIN Tbl_type_of_products t ON type_of_product = t.code 
		                                                        WHERE closing_date is NULL AND  a.customer_number = @customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            creditAccount = dr["credit_account"].ToString();
                    }
                }
            }

            return creditAccount;
        }

        internal static ActionResult CreditAccountValidation(long creditAccount, short notStrictFreezeChecking, short notStrictDAHKChecking, short notStrictDebtType)
        {
            ActionResult actionResult = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("pr_CreditAccountValidation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = creditAccount;
                    cmd.Parameters.Add("@NotStrictFreezeChecking", SqlDbType.TinyInt).Value = notStrictFreezeChecking;
                    cmd.Parameters.Add("@NotStrictDAHKChecking", SqlDbType.TinyInt).Value = notStrictDAHKChecking;
                    cmd.Parameters.Add("@NotStrictDebtType", SqlDbType.TinyInt).Value = notStrictDebtType;

                    try
                    {
                        cmd.ExecuteNonQuery();
                        actionResult.ResultCode = ResultCode.Normal;
                    }
                    catch (Exception ex)
                    {
                        if (((System.Data.SqlClient.SqlException)ex).Class == 11)
                        {
                            actionResult.Errors.Add(new ActionError(ex.Message));
                            actionResult.ResultCode = ResultCode.Failed;
                        }
                        else
                        {
                            throw;
                        }
                    }

                }
            }

            return actionResult;
        }

        internal static bool CheckStateRevenueCommitteeArrest(string accountNumber)
        {
            bool hasSRCArrest = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 * 
                                                                                                FROM Tbl_acc_freeze_history 
                                                                                                WHERE reason_type = 4 AND closing_date is null AND account_number = @accountNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hasSRCArrest = true;
                        }
                    }
                }
            }
            return hasSRCArrest;
        }

        internal static byte CheckAccessToThisAccounts(string accountNumber)
        {
            byte type_of_product = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 118 AS type_of_product FROM [dbo].Tbl_Visa_Numbers_Accounts
                                                        where card_type = 21 and closing_date is NULL AND card_account = @accountNumber
                                                        UNION
                                                        SELECT type_of_product FROM V_All_Accounts a
                                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE type_of_product IN (118, 119) AND(type_of_account = 24)
                                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new
                                                        INNER JOIN Tbl_type_of_products t ON type_of_product = t.code
		                                                        where a.Arm_number = @accountNumber", conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        if (dr.Read())
                            type_of_product = Convert.ToByte(dr["type_of_product"]);

                }
            }

            return type_of_product;
        }

        internal static Account GetDahkTransitAccount(string accountNumber)
        {
            Account account = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT a.account_access_group,a.open_date,v.co_type,type_of_product, a.Arm_number, a.type_of_account_new, a.balance, a.Currency, t.description,a.closing_date,a.card_number,a.filialcode,a.description as ac_description,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date FROM[tbl_all_accounts;] a INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE type_of_product=18 AND type_of_account = 24 GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
											 inner join v_all_accounts v on a.arm_number=v.arm_number 
                                             WHERE v.Arm_number = @accountNumber ";



                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        account = SetAccount(row);
                    }
                }

            }
            return account;

        }
        internal static async Task<List<Account>> GetDahkTransitAccountList(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();

            string script = accountSelectScript + @" INNER JOIN
										     (SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc
													 WHERE type_of_product=18 AND type_of_account = 24  GROUP BY sint_acc_new, type_of_client, type_of_product)s
													 ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE customer_number = @customerNumber and closing_date is null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account account = await SetAccountAsync(row);

                    accountList.Add(account);
                }

                return accountList;
            }
        }

        private static async Task<List<Account>> GetDeveloperSpecialAccountsAsync(ulong customerNumber, SqlConnection conn)
        {
            List<Account> accountList = new List<Account>();
            string script = @"SELECT a.open_date,type_of_product, acc.Arm_number,  acc.type_of_account_new,  acc.balance,  acc.Currency, t.description, acc.closing_date,A.Co_Type,a.card_number,a.description as ac_description,a.filialcode,a.account_type,a.freeze_date,a.UnUsed_amount,a.UnUsed_amount_date, a.account_access_group,t.DescriptionEng FROM V_All_Accounts a 
                                                 Inner Join [tbl_all_accounts;] acc
											    On a.Arm_number=acc.Arm_number
                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 119) AND(type_of_account = 24)                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
											 Tbl_type_of_products t ON type_of_product = t.code 
                                             WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number";
            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(dr);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    Account account = await SetAccountAsync(row);
                    accountList.Add(account);
                }
            }
            return accountList;
        }

        internal static ulong CheckCustomerFreeFunds(string accountNumber)
        {
            ActionResult actionResult = new ActionResult();
            DateTime operDay = Utility.GetNextOperDay().Date;
            ulong thirdPersonCustomerNumber = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT acc.customer_number, third_person_Customer_number, m.closing_date, acc.balance, acc.currency 
	                        FROM tbl_co_accounts_main m 
	                        INNER JOIN tbl_co_accounts a ON m.id = a.co_main_id 
	                        INNER JOIN [tbl_all_accounts;] acc on acc.Arm_number = m.arm_number 
	                        WHERE m.arm_number = @arm_number 
                            GROUP BY acc.customer_number, third_person_Customer_number, m.closing_date, acc.balance, acc.currency";
                using SqlCommand cmd = new SqlCommand(script, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@arm_number", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string query = @"select dbo.fnc_available_CustomerAmount(@customer_number, @currency, @today) as avalible_amount";
                        using (SqlCommand cmdCheck = new SqlCommand(query, conn))
                        {
                            cmdCheck.CommandType = CommandType.Text;
                            cmdCheck.Parameters.Add("@customer_number", SqlDbType.Float).Value = Convert.ToInt64(reader["customer_number"]);
                            cmdCheck.Parameters.Add("@currency", SqlDbType.NVarChar).Value = Convert.ToString(reader["currency"]);
                            cmdCheck.Parameters.Add("@today", SqlDbType.SmallDateTime).Value = operDay;
                            using SqlDataReader readerCheck = cmdCheck.ExecuteReader();

                            if (readerCheck.HasRows)
                            {
                                while (readerCheck.Read())
                                {
                                    if (Convert.ToDouble(readerCheck["avalible_amount"]) - Convert.ToDouble(reader["balance"]) >= 0)
                                    {
                                        thirdPersonCustomerNumber = Convert.ToUInt64(reader["third_person_Customer_number"]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return thirdPersonCustomerNumber;
        }

        internal static bool GetRightsTransferAvailability(string accountNumber)
        {
            bool exists = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT arm_number FROM Tbl_define_sint_acc  INNER JOIN [tbl_all_accounts;] ON type_of_account_new = sint_acc_new 
                            where type_of_product = 10 AND arm_number = @arm_number";
                using SqlCommand cmd = new SqlCommand(script, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@arm_number", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    exists = true;
                }
            }

            return exists;
        }

        internal static bool GetRightsTransferVisibility(string accountNumber)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT CASE WHEN DATEADD(YEAR,18,P.birth) <= [dbo].[get_oper_day]() THEN 1 ELSE 0 END AS result
                                FROM Tbl_co_accounts  CA
                                inner join tbl_co_accounts_main M on CA.co_main_id = M.id 
                                INNER JOIN [Tbl_customers] C  with (nolock) ON third_person_customer_number = C.customer_number
                                LEFT JOIN Tbl_Persons P   with (nolock) ON C.identityId  = p.identityId 
                                WHERE m.closing_date IS NULL AND M.[arm_number] = @arm_number 
                                GROUP BY M.[arm_number],P.birth";

                using SqlCommand cmd = new SqlCommand(script, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@arm_number", SqlDbType.Float).Value = accountNumber;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result = Convert.ToBoolean(reader["result"]);
                    }
                }
            }

            return result;
        }

        internal static bool GetCheckCustomerIsThirdPerson(string accountNumber, ulong customerNumber)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT acc.customer_number, third_person_Customer_number
                                FROM Tbl_co_accounts  CA
                                inner join tbl_co_accounts_main M on CA.co_main_id = M.id 
                                inner join [tbl_all_accounts;] acc on acc.Arm_number = m.arm_number 
                                WHERE acc.Arm_number = @arm_number AND acc.customer_number = @customer_number";

                using SqlCommand cmd = new SqlCommand(script, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@arm_number", SqlDbType.Float).Value = accountNumber;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    result = true;
                }
            }

            return result;
        }

        internal static string GetClosingCurrentAccountsNumber(ulong customerNumber, string currency)
        {
            string accountNumber = string.Empty;
            string query = @"SELECT Arm_number FROM  [tbl_all_accounts;] acc
                            INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 10) AND(type_of_account = 24) 
                            GROUP BY sint_acc_new, type_of_client, type_of_product) s ON acc.type_of_account_new = s.sint_acc_new 
                            WHERE NOT EXISTS (SELECT co.arm_number FROM Tbl_co_accounts_main co where  co.arm_number = acc.arm_number ) AND customer_number = @customerNumber  AND currency = @currency
                            ORDER BY open_date ASC";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        if (dr.Read())
                            accountNumber = dr["Arm_number"].ToString();
                }
            }
            return accountNumber;
        }



        internal static double GetAccountAvailableBalanceForStocksInAmd(string accountNumber)
        {
            double accountAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT dbo.fnc_available_AccountAmount_For_24_7(@account,dbo.get_oper_day(),@currency,1,0, 0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = "AMD";


                    accountAmount = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            return accountAmount;
        }

    }
}
