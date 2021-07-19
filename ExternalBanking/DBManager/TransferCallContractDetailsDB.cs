using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Linq;
namespace ExternalBanking.DBManager
{
   public class TransferCallContractDetailsDB
    {
        internal static List<TransferCallContractDetails> GetTransferCallContractsDetails(ulong customerNumber)
        {
            List<TransferCallContractDetails> contracts = new List<TransferCallContractDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select contract_id,filialcode,contract_number,contract_date,account_number,card_number,intermediator_set_number,quality,closing_date,contract_password from tbl_contracts_for_transfers_by_call
                                WHERE Customer_Number=@customerNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        TransferCallContractDetails contract = SetTransferCallContractDetails(row);

                        contracts.Add(contract);
                    }

                }
            }

            return contracts;
        }


        internal static TransferCallContractDetails GetTransferCallContractDetails(long contractId,ulong customerNumber)
        {
            TransferCallContractDetails contract = new TransferCallContractDetails();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select contract_id,filialcode,contract_number,contract_date,account_number,card_number,intermediator_set_number,quality,closing_date,contract_password from tbl_contracts_for_transfers_by_call
                                WHERE contract_id=@contractId and customer_number=@customerNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@contractId", SqlDbType.Int).Value = contractId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count>0)
                    {
                        contract = SetTransferCallContractDetails(dt.Rows[0]);
                    }
                         

                }
            }

            return contract;
        }



        private static TransferCallContractDetails SetTransferCallContractDetails(DataRow row)
        {
            TransferCallContractDetails contract = new TransferCallContractDetails();
            if (row != null)
            {
                contract.ContractId = long.Parse(row["contract_id"].ToString());
                contract.ContractNumber=long.Parse(row["contract_number"].ToString());
                contract.Account= Account.GetAccount(ulong.Parse(row["account_number"].ToString()));
                contract.ContractDate = DateTime.Parse(row["contract_date"].ToString());
                contract.ContractPassword = row["contract_password"].ToString();
                contract.Quality = short.Parse(row["quality"].ToString());
                contract.InvolvingSetNumber =row["intermediator_set_number"] != DBNull.Value ? int.Parse(row["intermediator_set_number"].ToString()):0;
                contract.ClosingDate = row["closing_date"] != DBNull.Value ? DateTime.Parse(row["closing_date"].ToString()) : default(DateTime?);
            }
            return contract;
        }


    }
}
