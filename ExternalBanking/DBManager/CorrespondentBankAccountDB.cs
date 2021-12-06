using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    class CorrespondentBankAccountDB
    {
        internal static List<CorrespondentBankAccount> GetCorrespondentBankAccounts(CorrespondentBankAccount filter)
        {
            DataTable dt = new DataTable();
            List<CorrespondentBankAccount> accounts = new List<CorrespondentBankAccount>();
            string whereCond = " Where closing_date is null  ";

            if (filter.TransferSystem != 0)
            {
                whereCond = whereCond + " and transfersystem =" + filter.TransferSystem.ToString();
            }
            if (filter.AcbaTransfer == 1)
            {
                whereCond = whereCond + " and Acba_Transfer<>0 ";
            }

            if (!String.IsNullOrEmpty (filter.Currency ))
            {
               
                whereCond = whereCond + " and currency= '" + filter.Currency + "' ";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT Code_Account,Name_of_bank, currency, arm_number, isnull(transfersystem,0) transfersystem , swift_code FROM [Tbl_of_armenians_banks;] " + whereCond + " GROUP BY Code_Account,Name_of_bank, currency, arm_number, transfersystem, swift_code ", conn);
 
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        CorrespondentBankAccount account = new CorrespondentBankAccount();
                        account.CodeAccount = row["Code_Account"].ToString();
                        account.BankName  = Utility.ConvertAnsiToUnicode ( row["Name_of_bank"].ToString());
                        account.Currency  = row["currency"].ToString();
                        account.Account = row["arm_number"].ToString();
                        account.TransferSystem = short.Parse(row["transfersystem"].ToString());
                        account.SwiftCode  = row["swift_code"].ToString(); 

                        accounts.Add(account);
                    }
                }

            }
            return accounts;
        }
    }
}
