using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class BondDB
    {
        internal static Bond GetBondByID(int ID)
        {
            Bond bond = new Bond();
            bond.CustomerDepositaryAccount = new DepositaryAccount();
            string sql = "";

            sql = @"  SELECT b.id, b.app_id, b.ISIN, b.bond_count, b.unit_price, b.total_price, b.filialcode, b.doc_id, b.account_number_for_bond, b.bond_issue_id,
                                           b.account_number_for_coupon, b.customer_number, b.amount_charge_date, b.amount_charge_time, E.id as depositary_account_existence_type,
                                           E.[description] as depositary_account_existence_type_description, b.depositary_account, b.depositary_account_description, b.registration_date, b.set_number,
                                           b.currency, b.interest_rate, b.quality,  q.description as bond_description, b.document_number, b.depositary_account_bank_code, b.reject_reason_id, 
                                           (isnull(R.[description], '') + ' ' + isnull(b.reject_reason_description, '')) as reject_reason_description          
                            FROM Tbl_bonds b inner join Tbl_Type_of_Depositary_Account_Existence E on B.depositary_account_existence_type = E.id
                                        INNER JOIN tbl_type_of_bond_quality q ON b.quality = q.id
                                        LEFT JOIN Tbl_type_of_bond_reject_reasons R on b.reject_reason_id = R.id
                            WHERE b.ID = @ID ";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    DataRow row = dt.Rows[0];

                    bond.BondIssueId = int.Parse(row["bond_issue_id"].ToString());
                    bond.ID = int.Parse(row["id"].ToString());
                    bond.AppId = long.Parse(row["app_id"].ToString());
                    bond.ISIN = row["ISIN"].ToString();
                    bond.BondCount = int.Parse(row["bond_count"].ToString());
                    bond.UnitPrice = double.Parse(row["unit_price"].ToString());
                    bond.TotalPrice = double.Parse(row["total_price"].ToString());
                    bond.FilialCode = int.Parse(row["filialcode"].ToString());
                    bond.HBDocId = UInt64.Parse(row["doc_id"].ToString());
                    bond.AccountForBond = Account.GetAccount(ulong.Parse(row["account_number_for_bond"].ToString()));
                    bond.AccountForCoupon = Account.GetAccount(ulong.Parse(row["account_number_for_coupon"].ToString()));
                    bond.CustomerNumber = UInt64.Parse(row["customer_number"].ToString());
                    bond.AmountChargeDate = row["amount_charge_date"] != DBNull.Value ? DateTime.Parse(row["amount_charge_date"].ToString()) : default(DateTime);    
                    bond.AmountChargeTime = row["amount_charge_time"] != DBNull.Value ? (TimeSpan)row["amount_charge_time"] : default(TimeSpan);
                    bond.CustomerDepositaryAccount.AccountNumber = row["depositary_account"] != DBNull.Value ? Convert.ToDouble(row["depositary_account"]) : 0;
                    bond.CustomerDepositaryAccount.Description = row["depositary_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(row["depositary_account_description"].ToString()) : default(string);
                    bond.RegistrationDate = DateTime.Parse(row["registration_date"].ToString());
                    bond.SetNumber = int.Parse(row["set_number"].ToString());
                    bond.Currency = row["currency"].ToString();
                    bond.InterestRate = row["interest_rate"] != DBNull.Value ? double.Parse(row["interest_rate"].ToString()) : default(double);
                    bond.Quality = (BondQuality)byte.Parse(row["quality"].ToString());
                    bond.QualityDescription = row["bond_description"].ToString();
                    bond.DocumentNumber = Convert.ToInt32(row["document_number"].ToString());
                    bond.DepositaryAccountExistenceType = (DepositaryAccountExistence)Convert.ToByte(dt.Rows[0]["depositary_account_existence_type"]);
                    bond.DepositaryAccountExistenceTypeDescription = dt.Rows[0]["depositary_account_existence_type_description"].ToString();
                    bond.CustomerDepositaryAccount.BankCode = dt.Rows[0]["depositary_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["depositary_account_bank_code"]) : 0;

                    if (bond.Quality == BondQuality.Rejected)
                    {
                        bond.RejectReasonId = (BondRejectReason)(dt.Rows[0]["reject_reason_id"] != DBNull.Value ? Convert.ToByte(dt.Rows[0]["reject_reason_id"]) : 0);
                        bond.RejectReasonDescription = dt.Rows[0]["reject_reason_description"] != DBNull.Value ? dt.Rows[0]["reject_reason_description"].ToString() : "";
                    }
                  
                }
            }

            return bond;

        }


        internal static List<Bond> GetBonds(BondFilter filter)
        {
            Bond bond;
            List<Bond> bondList = new List<Bond>();
            string sql = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand command = new SqlCommand(sql, conn))
                {

                    sql = @"  SELECT b.id as bond_id, app_id, b.ISIN, b.bond_count, b.unit_price, b.total_price, b.filialcode, b.doc_id, b.account_number_for_bond,
                                                    b.account_number_for_coupon, b.customer_number, b.amount_charge_date, b.amount_charge_time, 
                                                    b.depositary_account, b.depositary_account_description, b.registration_date, b.set_number, b.quality ,
                                                    b.currency, b.interest_rate, q.description as bond_description, b.document_number,
                                                    b.depositary_account_existence_type, E.[description] as depositary_account_existence_type_description,
                                                    b.depositary_account_bank_code, b.reject_reason_id, 
                                                    (isnull(R.[description], '') + ' ' + isnull(b.reject_reason_description, '')) as reject_reason_description
                                    FROM Tbl_bonds b 
                                                INNER JOIN tbl_type_of_bond_quality q ON b.quality = q.id
                                                INNER JOIN Tbl_Type_of_Depositary_Account_Existence E on B.depositary_account_existence_type = E.id
                                                LEFT JOIN Tbl_type_of_bond_reject_reasons R on b.reject_reason_id = R.id
                                    WHERE 1 = 1  ";

                    if(filter.BondIssueId != 0)
                    {
                        sql = sql + " and b.bond_issue_id = @bondIssueId ";
                        command.Parameters.Add("@bondIssueId", SqlDbType.Int).Value = filter.BondIssueId;
                    }

                    if (!string.IsNullOrEmpty(filter.ISIN))
                    {
                        sql = sql + " and b.ISIN = @ISIN ";
                        command.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = filter.ISIN;
                    }

                    if (!string.IsNullOrEmpty(filter.Currency))
                    {
                        sql = sql + "  and b.currency = @currency ";
                        command.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = filter.Currency;
                    }

                    if (filter.Quality != BondQuality.None)
                    {
                        sql = sql + " and b.quality = @quality ";
                        command.Parameters.Add("@quality", SqlDbType.Int).Value = filter.Quality;
                    }
                                       
                    if (filter.CustomerNumber != 0)
                    {
                        sql = sql + " and b.customer_number = @customer_number ";
                        command.Parameters.Add("@customer_number", SqlDbType.Float).Value = filter.CustomerNumber;
                    }
                    
                    if (filter.StartDate != default(DateTime))
                    {
                        sql = sql + " and b.registration_date >= @StartDate ";
                        command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = filter.StartDate;
                    }

                    if (filter.EndDate != default(DateTime))
                    {
                        sql = sql + " and b.registration_date <= @EndDate ";
                        command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = filter.EndDate;
                    }



                    sql = sql + " ORDER BY b.id desc ";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        bond = new Bond();
                        bond.CustomerDepositaryAccount = new DepositaryAccount();

                        bond.ID = int.Parse(row["bond_id"].ToString());
                        bond.AppId = long.Parse(row["app_id"].ToString());
                        bond.ISIN = row["ISIN"].ToString();
                        bond.BondCount = int.Parse(row["bond_count"].ToString());
                        bond.UnitPrice = double.Parse(row["unit_price"].ToString());
                        bond.TotalPrice = double.Parse(row["total_price"].ToString());
                        bond.FilialCode = int.Parse(row["filialcode"].ToString());
                        bond.HBDocId = UInt64.Parse(row["doc_id"].ToString());

                        bond.AccountForBond = Account.GetAccount(ulong.Parse(row["account_number_for_bond"].ToString()));

                        if (ulong.Parse(row["account_number_for_bond"].ToString()) != ulong.Parse(row["account_number_for_coupon"].ToString()))
                        {
                            bond.AccountForCoupon = Account.GetAccount(ulong.Parse(row["account_number_for_coupon"].ToString()));
                        }
                        else
                        {
                            bond.AccountForCoupon = bond.AccountForBond;
                        }                        

                        bond.CustomerNumber = UInt64.Parse(row["customer_number"].ToString());
                        bond.AmountChargeDate = row["amount_charge_date"] != DBNull.Value ? DateTime.Parse(row["amount_charge_date"].ToString()) : default(DateTime);
                        bond.AmountChargeTime = row["amount_charge_time"] != DBNull.Value ? (TimeSpan)row["amount_charge_time"] : default(TimeSpan);
                        bond.CustomerDepositaryAccount.AccountNumber = row["depositary_account"] != DBNull.Value ? Convert.ToDouble(row["depositary_account"]) : 0;
                        bond.CustomerDepositaryAccount.Description = row["depositary_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(row["depositary_account_description"].ToString()) : "";
                        bond.RegistrationDate = DateTime.Parse(row["registration_date"].ToString());
                        bond.SetNumber = int.Parse(row["set_number"].ToString());
                        bond.Quality = (BondQuality)byte.Parse(row["quality"].ToString());
                        bond.Currency = row["currency"].ToString();
                        bond.InterestRate = row["interest_rate"] != DBNull.Value ? double.Parse(row["interest_rate"].ToString()) : default(double);
                        bond.QualityDescription = row["bond_description"].ToString();
                        bond.DocumentNumber = Convert.ToInt32(row["document_number"].ToString());
                        bond.DepositaryAccountExistenceType = (DepositaryAccountExistence)Convert.ToByte(row["depositary_account_existence_type"]);
                        bond.DepositaryAccountExistenceTypeDescription = row["depositary_account_existence_type_description"] != DBNull.Value ? row["depositary_account_existence_type_description"].ToString() : "";
                        bond.CustomerDepositaryAccount.BankCode = row["depositary_account_bank_code"] != DBNull.Value ? Convert.ToInt32(row["depositary_account_bank_code"]) : 0;

                        if(bond.Quality == BondQuality.Rejected)
                        {
                            bond.RejectReasonId = (BondRejectReason)(row["reject_reason_id"] != DBNull.Value ? Convert.ToByte(row["reject_reason_id"]) : 0);
                            bond.RejectReasonDescription = row["reject_reason_description"] != DBNull.Value ? row["reject_reason_description"].ToString() : "";
                        }

                      

                        bondList.Add(bond);
                    }
                }
            }

            return bondList;

        }
    }
}









