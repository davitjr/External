using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class AssigneeOperationDB
    {

        /// <summary>
        /// GET ASIGNEE OPERATIONS LIST
        /// </summary>
        internal static List<AssigneeOperation> GetAssigneeOperationsList(uint assigneeId)
        {
            List<AssigneeOperation> availableOperationsList = new List<AssigneeOperation>();
           
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT op.*,t.Description, t.GroupId, G.Description GroupDescription FROM [dbo].[Tbl_assign_operations] op 
                                                INNER JOIN Tbl_type_of_assign_operation t on op.type=t.ID 
                                                INNER JOIN Tbl_Type_of_assign_operations_groups G ON t.GroupId = G.Id                                             
                                                WHERE assigneeId=@assigneeId", conn);

                cmd.Parameters.Add("@assigneeId", SqlDbType.Int).Value = assigneeId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    availableOperationsList = new List<AssigneeOperation>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    AssigneeOperation oneOperation = SetAssignOperation(row);

                    availableOperationsList.Add(oneOperation);
                }

            }                        

            return availableOperationsList;
        }

        private static AssigneeOperation SetAssignOperation(DataRow row)
        {
            AssigneeOperation oneOperation = new AssigneeOperation();

            if (row != null)
            {                
                oneOperation.Id = uint.Parse(row["id"].ToString());
                oneOperation.AllAccounts = bool.Parse(row["allAccounts"].ToString());
                oneOperation.OperationType = ushort.Parse(row["type"].ToString());
                oneOperation.OperationTypeDescription = Utility.ConvertAnsiToUnicode(row["Description"].ToString());
                oneOperation.GroupId = ushort.Parse(row["GroupId"].ToString());
                oneOperation.OperationGroupTypeDescription = Utility.ConvertAnsiToUnicode(row["GroupDescription"].ToString());
                oneOperation.AccountList = GetAssigneeOperationAccounts(oneOperation.Id);
            }
            return oneOperation;
        }

        


        /// <summary>
        /// GET ASIGNEE OPERATION ACCOUNTS LIST
        /// </summary>
        internal static List<Account> GetAssigneeOperationAccounts(uint operationId)
        {
            List<Account> accountsList = new List<Account>();
            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT op.*,acc.type_of_account_new,acc.currency  FROM [Tbl_assign_operation_accounts] op 
					                                                                                INNER JOIN [tbl_all_accounts;] acc on op.account=acc.Arm_number  
			                                                                                WHERE operationId=@operationId", conn);

                cmd.Parameters.Add("@operationId", SqlDbType.Int).Value = operationId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    accountsList = new List<Account>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account oneAccount = SetOperationAccount(row);

                    accountsList.Add(oneAccount);
                }

            }            

            return accountsList;
        }

        private static Account SetOperationAccount(DataRow row)
        {
            Account oneAccount = new Account();

            if (row != null)
            {
                oneAccount.AccountNumber = row["account"].ToString();
                oneAccount.Currency = row["currency"].ToString();
            }
            return oneAccount;
        }

        /// <summary>
        /// GET ACCOUNTS LIST FOR CREDENTIAL
        /// </summary>
        internal static List<Account> GetAccountsForCredential(ulong customerNumber, int operationType)
        {
            List<Account> accountsList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT at.id,at.operationTypeId,acc.Arm_Number,acc.for_print_type,acc.type_of_account_new,acc.currency,acc.closing_date 
                            FROM Tbl_assign_operation_account_types at 
                            INNER JOIN (SELECT type_of_product,type_of_account,sint_acc_new 
                                        FROM Tbl_define_sint_acc group by type_of_product,type_of_account,sint_acc_new) sa ON at.accountType=sa.type_of_account and at.productType=sa.type_of_product 
                            INNER JOIN V_All_Accounts acc ON acc.type_of_account_new=sa.sint_acc_new  
                            WHERE acc.customer_Number = @CustomerNumber and operationTypeId=@OperationType ORDER BY acc.type_of_account_new", conn);

                cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@OperationType", SqlDbType.Int).Value = operationType;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    accountsList = new List<Account>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account oneAccount = SetAccountsForCredential(row);

                    accountsList.Add(oneAccount);
                }

            }

            return accountsList;
        }

        private static Account SetAccountsForCredential(DataRow row)
        {
            Account oneAccount = new Account();

            if (row != null)
            {
                oneAccount.AccountNumber = row["Arm_Number"].ToString();
                oneAccount.Currency = row["currency"].ToString();
            }
            return oneAccount;
        }

    }
}
