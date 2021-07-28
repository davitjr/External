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

namespace ExternalBanking.DBManager
{
    static class LeasingAccountDB
    {
        internal static string GetCurrentLeasingAccountList(ulong customerNumber)
        {
            string account = "";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();



                string script = @" SELECT  top 1  acc.Arm_number FROM V_All_Accounts a 
                       Inner Join [tbl_all_accounts;] acc
	                    On a.Arm_number=acc.Arm_number
                         INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 10) AND(type_of_account = 24)
                           GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
		                     Tbl_type_of_products t ON type_of_product = t.code 
                           WHERE acc.customer_number = @customerNumber And acc.closing_date is null  and  a.customer_number=acc.customer_number and acc.currency = 'AMD'
		                    order by a.open_date desc";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["Arm_number"] != DBNull.Value)
                            {
                                account = dr["Arm_number"].ToString();
                            }
                        }
                    }



                }
                conn.Close();
            }

            return account;

        }
    }
}
