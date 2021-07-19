using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class SearchTransferBankMailDB
    {
        internal static List<SearchTransferBankMail> GetTransfersBankMailDB(SearchTransferBankMail searchParams)
        {
            List<SearchTransferBankMail> transfersBankMailList = new List<SearchTransferBankMail>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    sql = " TBM.transfer_group ="+ searchParams.TransferGroup+" and deleted_set_number is null ";
                    if (searchParams.DateOfTransfer != DateTime.MinValue)
                    {
                        sql = sql + " and TBM.registration_date = '" + searchParams.DateOfTransfer.ToString("dd/MMM/yyyy") + "'  ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.SenderAccount))
                    {
                        sql = sql + "  and TBMR.sender_account = '" + searchParams.SenderAccount + "'";
                    }

                    if (!String.IsNullOrEmpty(searchParams.ReceiverAccount))
                    {
                        sql = sql + "  and TBMR.receiver_account like '%" + searchParams.ReceiverAccount + "%' ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.ReceiverName))
                    {
                        sql = sql + "  and receiver_name like  '%" + Utility.ConvertUnicodeToAnsi(searchParams.ReceiverName) + "%'  ";
                    }

                    if (!String.IsNullOrEmpty(searchParams.Amount))
                    {
                        sql = sql + " and  amount = " + searchParams.Amount;
                    }

                    if (!String.IsNullOrEmpty(searchParams.DescriptionForPayment))
                    {
                        sql = sql + " and TBMR.description like  '%" + Utility.ConvertUnicodeToAnsi(searchParams.DescriptionForPayment) + "%'  ";
                    }

                    cmd.CommandText = "sp_tamplate_for_transfers_bankmail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@criteria", SqlDbType.NVarChar).Value = sql;
                    cmd.Parameters.Add("@startR", SqlDbType.Int).Value = searchParams.BeginRow;
                    cmd.Parameters.Add("@endR", SqlDbType.Int).Value = searchParams.EndRow;
                    cmd.Parameters.Add("@isArchive", SqlDbType.Int).Value = searchParams.IsArchive;
                    cmd.Parameters.Add("@isBudget", SqlDbType.Int).Value = searchParams.IsBudget;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        transfersBankMailList = new List<SearchTransferBankMail>();
                    }
                    int count = 0;
                    if (dr.Read())
                    {
                        count = Convert.ToInt32(dr["cnt"]);
                    }
                    int senderRegCode;
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            SearchTransferBankMail oneResult = new SearchTransferBankMail();
                            oneResult.RowCount = count;
                            oneResult.ReceiverName = Utility.ConvertAnsiToUnicode(dr["receiver_name"].ToString());
                            oneResult.SenderAccount = dr["sender_account"].ToString();
                            oneResult.ReceiverAccount = dr["receiver_account"].ToString();
                            oneResult.DateOfTransfer = Convert.ToDateTime(dr["registration_date"]);
                            oneResult.ReceiverBank = dr["credit_bank_code"].ToString();
                            oneResult.DescriptionForPayment = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                            oneResult.Amount = dr["amount"].ToString();
                            int.TryParse(dr["sender_reg_code"].ToString(), out senderRegCode);
                            if (senderRegCode != 0)
                                oneResult.SenderRegCode = GetRegistrationCodeDB(senderRegCode);
                            else
                                oneResult.SenderRegCode = "";
                            transfersBankMailList.Add(oneResult);
                        }

                    }

                }
                return transfersBankMailList;
            }
        }

        internal static string GetRegistrationCodeDB(int regCode)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";
                string result;
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandText = "SELECT code FROM Tbl_region_tax_codes WHERE code=@regCode";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@regCode", SqlDbType.Int).Value = regCode;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                        result= regCode.ToString();
                    else
                       result = "";
                }
                return result;
            }
        }
    }
}
