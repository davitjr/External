using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class SearchBudgetAccountDB
    {
        internal static List<SearchBudgetAccount> GetBudgetAccountsDB(SearchBudgetAccount searchParams)
        {
            List<SearchBudgetAccount> budgetAccountsList = new List<SearchBudgetAccount>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!String.IsNullOrEmpty(searchParams.AccountNumber))
                    {
                        sql = sql + " and code = '" + searchParams.AccountNumber + "' ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.Description))
                    {
                        sql = sql + " and [name] like '%" + Utility.ConvertUnicodeToAnsi(searchParams.Description) + "%' ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.CustomerType))
                    {
                        if (int.Parse(searchParams.CustomerType) == 1)
                            sql = sql + " and is_legal='true' ";
                        if (int.Parse(searchParams.CustomerType) == 2)
                            sql = sql + " and is_entrepreneur='true' ";
                        if (int.Parse(searchParams.CustomerType) == 3)
                            sql = sql + " and is_physical='true' ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.AccountType))
                    {
                        sql = sql + " and acct_type = " + searchParams.AccountType;
                    }

                    if (!String.IsNullOrEmpty(sql))
                        sql = sql.Substring(4);

                    cmd.CommandText = "pr_get_budget_accounts";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@criteria", SqlDbType.NVarChar).Value = sql;
                    cmd.Parameters.Add("@startR", SqlDbType.Int).Value = searchParams.BeginRow;
                    cmd.Parameters.Add("@endR", SqlDbType.Int).Value = searchParams.EndRow;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            budgetAccountsList = new List<SearchBudgetAccount>();
                        }
                        int count = 0;
                        if (dr.Read())
                        {
                            count = Convert.ToInt32(dr["cnt"]);
                        }

                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                SearchBudgetAccount oneResult = new SearchBudgetAccount();
                                oneResult.RowCount = count;
                                oneResult.AccountNumber = dr["accNumber"].ToString();
                                oneResult.Description = Utility.ConvertAnsiToUnicode(dr["name"].ToString());
                                oneResult.AccountType = Utility.ConvertAnsiToUnicode(dr["accountType"].ToString());
                                oneResult.IsLegal = Utility.ConvertAnsiToUnicode(dr["isLegal"].ToString());
                                oneResult.IsEntrepreneur = Utility.ConvertAnsiToUnicode(dr["isEntrepreneur"].ToString());
                                oneResult.IsPhysical = Utility.ConvertAnsiToUnicode(dr["isPhysical"].ToString());
                                oneResult.Soc = Utility.ConvertAnsiToUnicode(dr["soc"].ToString());

                                budgetAccountsList.Add(oneResult);
                            }

                        }

                    }
                }
                return budgetAccountsList;
            }
        }
    }
}
