using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    class TransferCallContractDB
    {
        internal static TransferCallContract SetTransferContract(DataRow row)
        {

            TransferCallContract transferContract = new TransferCallContract();
            if (row != null)
            {
                transferContract.ContractId = Convert.ToInt64(row["contract_id"]);
                transferContract.ContractNumber = Convert.ToInt64(row["contract_number"]);
                transferContract.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                transferContract.AccountNumber = Convert.ToUInt64(row["account_number"]);
                transferContract.CardNumber = row["card_number"].ToString();
                transferContract.Currency = row["currency"].ToString();
                transferContract.ContractFilialCode = Convert.ToUInt16(row["filialcode"]);
                transferContract.StartCapital = (row["start_capital"] != DBNull.Value) ? Convert.ToDouble(row["start_capital"]) : default(double);
                transferContract.CardType = row["CardType"].ToString();
                transferContract.RelatedOfficeNumber = (row["related_office_number"] != DBNull.Value) ? Convert.ToInt64(row["related_office_number"]) : default(long);
                transferContract.ContractPassword = row["contract_password"].ToString();
                transferContract.MotherName = row["MotherName"].ToString();
                transferContract.CardPhone = row["cardphone"].ToString();
                transferContract.CreditLineQuality = (row["credit_line_quality"] != DBNull.Value) ? Convert.ToInt16(row["credit_line_quality"]) : default(Int16);
                transferContract.AccountDescription = Utility.ConvertAnsiToUnicode(row["account_description"].ToString());
                if (row["validation_date"] != DBNull.Value)
                {
                    transferContract.CardValidationDate = Convert.ToDateTime(row["validation_date"]);
                }

            }
            return transferContract;
        }

        internal static TransferCallContract SetTransferContractForAcbaOnline(DataRow row)
        {

            TransferCallContract transferContract = new TransferCallContract();
            if (row != null)
            {
                transferContract.ContractId = Convert.ToInt64(row["contract_id"]);
                transferContract.ContractNumber = Convert.ToInt64(row["contract_number"]);
                transferContract.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                transferContract.AccountNumber = Convert.ToUInt64(row["account_number"]);
                transferContract.CardNumber = row["card_number"].ToString();
                transferContract.ContractFilialCode = Convert.ToUInt16(row["filialcode"]);
                transferContract.ContractPassword = row["contract_password"].ToString();

            }
            return transferContract;
        }

        internal static List<TransferCallContract> GetTransferCallContracts(string customerNumber, string accountNumber, string cardNumber)
        {
            TransferCallContract transferContract = new TransferCallContract();
            List<TransferCallContract> list = new List<TransferCallContract>();
            List<SqlParameter> prms = new List<SqlParameter>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"SELECT TC.contract_id, TC.contract_number,TC.customer_number, TC.filialcode,AC.currency,TC.account_number,TC.card_number,typc.ApplicationsCardType +'('+ TypC.CardType +')' as CardType, vis.related_office_number,cr.start_capital,TC.contract_password,MotherName,cardphone, credit_line_quality,AC.description as account_description, validation_date
                                                                               FROM tbl_contracts_for_transfers_by_call TC 
                                                                               INNER JOIN [tbl_all_accounts;] AC ON TC.account_number=AC.Arm_number 
                                                                               LEFT JOIN (Select related_office_number,visa_number,card_type,app_id, validation_date FROM Tbl_Visa_Numbers_Accounts WHERE closing_date is null) vis 
                                                                               ON TC.card_number = vis.visa_number 
                                                                               LEFT JOIN (Select visa_number,start_capital FROM  Tbl_credit_lines WHERE loan_type = 8) cr 
                                                                               ON TC.card_number = cr.visa_number 
                                                                                OUTER APPLY (Select  max(quality) credit_line_quality FROM  Tbl_credit_lines where  visa_number= TC.card_number  ) Q                                                                               
                                                                                LEFT JOIN Tbl_type_of_Card  TypC 
                                                                               ON Vis.card_type  = TypC.ID 
                                                                               OUTER APPLY (SELECT Cardnumber, MotherName,Case WHEN isnull(Telephone1_home,'000000')<>'000000' THEN  Telephone1_home + ' ' ELSE '' END  + CASE WHEN isnull(Mobile_home,'00000000000')<>'00000000000' THEN Mobile_home + ' ' ELSE '' END + CASE WHEN ISNULL(Mobile_home,'')<>ISNULL(Fax_home,'') THEN ISNULL(Fax_home,'') ELSE '' END as cardphone
                                                                                                        FROM Tbl_VISA_applications 
                                                                                                        WHERE app_id=vis.app_id) Vapp
                                                                               WHERE TC.deleted = 0 And TC.quality = 0 ";



                if (customerNumber != "" && customerNumber != null)
                {
                    sqltext += " and str(TC.customer_number,12) like '%' + @customerNumber ";
                    SqlParameter prm = new SqlParameter("@customerNumber", SqlDbType.NVarChar);
                    prm.Value = customerNumber;
                    prms.Add(prm);
                }

                if (accountNumber != "0")
                {
                    sqltext += " and TC.account_number=@accountNumber ";
                    SqlParameter prm = new SqlParameter("@accountNumber", SqlDbType.Float);
                    prm.Value = accountNumber;
                    prms.Add(prm);
                }

                if (cardNumber != "" && cardNumber != null)
                {
                    sqltext += " and TC.card_number like '%' + @cardNumber ";
                    SqlParameter prm = new SqlParameter("@cardNumber", SqlDbType.NVarChar);
                    prm.Value = cardNumber;
                    prms.Add(prm);
                }

                sqltext += "ORDER BY TC.customer_number";

                SqlCommand cmd = new SqlCommand(sqltext, conn);
                cmd.Parameters.AddRange(prms.ToArray());

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        transferContract = SetTransferContract(dt.Rows[i]);
                        list.Add(transferContract);
                    }
                }

            }
            return list;
        }

        internal static TransferCallContract GetContractDetails(long contarctId)
        {
            TransferCallContract transferContract = new TransferCallContract();

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"SELECT * FROM tbl_contracts_for_transfers_by_call   WHERE contract_id =@contarctId";

                SqlCommand cmd = new SqlCommand(sqltext, conn);
                cmd.Parameters.Add("@contarctId", SqlDbType.Int).Value = contarctId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                    transferContract = SetTransferContractForAcbaOnline(dt.Rows[0]);
                }

            }
            return transferContract;
        }


    }
}
