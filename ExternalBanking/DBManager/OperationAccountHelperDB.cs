using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.DBManager
{
    class OperationAccountHelperDB
    {
        internal static ushort GetOperationSystemAccountTypeForFee(Order order, short feeType)
        {


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                ushort accountType = 0;
                conn.Open();
                using (SqlCommand cmd =new SqlCommand(@"SELECT account_type FROM tbl_link_order_type_fee_account WHERE  product_type=@orderType AND  fee_type=@feeType",conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@orderType", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@feeType", SqlDbType.Int).Value = feeType;
                    Object ob = cmd.ExecuteScalar();
                    if (ob != null)
                        accountType = Convert.ToUInt16(ob);

                    return accountType;
                }
                   
            }
        }

        internal static Account GetOperationSystemAccountForFee(OperationAccountHelper helper)
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
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = helper.CustomerNumber;
                    cmd.Parameters.Add("@customerType", SqlDbType.Int).Value = helper.CustomerType;
                    cmd.Parameters.Add("@customerResidence", SqlDbType.Int).Value = helper.CustomerResidence;
                    cmd.Parameters.Add("@utilityBranch", SqlDbType.NVarChar, 20).Value = helper.Utilitybranch;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = helper.CardNumber;
                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = helper.DebetAccount;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = helper.CreditAccount;
                    cmd.Parameters.Add("@priceIndex", SqlDbType.Int).Value = helper.PriceIndex;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = helper.AppID;


                    SqlParameter param = new SqlParameter("@account", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    accountNumber = cmd.Parameters["@account"].Value.ToString();
                }
                   
            }

            return Account.GetSystemAccount(accountNumber);
        }

    }
}
