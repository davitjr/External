using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    class ProvisionDB
    {

        internal static List<Provision> GetProductProvisions(ulong productId, ulong customerNumber)
        {
            List<Provision> provisions = new List<Provision>();

            Loan loan = null;
            loan = LoanDB.GetAparikTexumLoan(productId, customerNumber);
            if (loan != null)
            {
                productId = LoanDB.GetPaidFactoringMainProductId((long)productId);
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select case when matured_date is  null then 'Գործող' else 'Փակված' End AS status,Type,out_balance, Moveable_index, currency, amount, provision_number,Tbl_type_of_all_provision.description,pro_description,case when loan_program=0 then Null else loan_program end as loan_program,
                                                    packet_number, packet_weight, provision_licence_number, property_licence_number, activated_date, matured_date, P.IdPro, LP.activated_set_number, LP.matured_set_number, LP.matured_reason,id_contract
                                                    From Tbl_Link_application_Provision LP Inner join Tbl_provision_of_clients P ON LP.IdPro=P.IdPro Inner join Tbl_type_of_all_provision ON P.[type]=Tbl_type_of_all_provision.[type_id]
                                                    Where LP.app_id=cast(cast(@app_Id as bigint) as nvarchar) and matured_date is null and customer_number=@customerNumber
                                                    Order by matured_date, currency,out_balance, activated_date", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Provision provision = SetProvision(row);

                        provisions.Add(provision);
                    }
                }

            }
            return provisions;

        }
        private static Provision SetProvision(DataRow row)
        {
            Provision provision = new Provision();
            if (row != null)
            {
                if (row["IdPro"] != DBNull.Value)
                {
                    provision.Id = Convert.ToInt64(row["IdPro"].ToString());
                }
                if (row.Table.Columns.Contains("provision_number"))
                {
                    provision.ProvisionNumber = row["provision_number"].ToString();
                }

                if (row.Table.Columns.Contains("id_contract") && row["id_contract"] != DBNull.Value)
                {
                    provision.ContractId = Convert.ToInt32(row["id_contract"].ToString());
                }
                if (row.Table.Columns.Contains("out_balance"))
                {
                    provision.OutBalance = row["out_balance"].ToString();
                }

                provision.Currency = row["currency"].ToString();
                provision.Amount = Convert.ToDouble(row["amount"].ToString());
                provision.Type = Convert.ToInt16(row["Type"].ToString());
                if (row.Table.Columns.Contains("description"))
                {
                    provision.TypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                }
                if (row.Table.Columns.Contains("loan_program") && row["loan_program"] != DBNull.Value)
                {
                    provision.LoanProgram = Convert.ToInt16(row["loan_program"].ToString());
                }
                if (row.Table.Columns.Contains("packet_number") && row["packet_number"] != DBNull.Value)
                {
                    provision.PacketNumber = row["packet_number"].ToString();
                }
                if (row.Table.Columns.Contains("provision_licence_number") && row["provision_licence_number"] != DBNull.Value)
                {
                    provision.LicenceNumber = row["provision_licence_number"].ToString();
                }
                if (row.Table.Columns.Contains("property_licence_number") && row["property_licence_number"] != DBNull.Value)
                {
                    provision.PropertyLicenceNumber = row["property_licence_number"].ToString();
                }
                if (row["activated_date"] != DBNull.Value)
                {
                    provision.ActivateDate = Convert.ToDateTime(row["activated_date"].ToString());
                }
                if (row["matured_date"] != DBNull.Value)
                {
                    provision.ClosingDate = Convert.ToDateTime(row["matured_date"].ToString());
                }
                if (row["matured_reason"] != DBNull.Value)
                {
                    provision.ClosingReason = row["matured_reason"].ToString();
                }
                if (row.Table.Columns.Contains("Moveable_index") && row["Moveable_index"] != DBNull.Value)
                {
                    provision.MoveableIndex = Convert.ToInt16(row["Moveable_index"].ToString());
                }

                if (row["pro_description"] != DBNull.Value)
                {
                    provision.Description = Utility.ConvertAnsiToUnicode(row["pro_description"].ToString());
                }

                if (row.Table.Columns.Contains("activated_set_number") && row["activated_set_number"] != DBNull.Value)
                {
                    provision.ActivatedSetNumber = Convert.ToInt32(row["activated_set_number"].ToString());
                }
                if (row.Table.Columns.Contains("matured_set_number") && row["matured_set_number"] != DBNull.Value)
                {
                    provision.MaturedSetNumber = Convert.ToInt32(row["matured_set_number"].ToString());
                }
                if (row.Table.Columns.Contains("status"))
                {
                    provision.QualityDescription = row["status"].ToString();
                }

                if (provision.ClosingDate == null)
                {
                    provision.Quality = 1;
                }
                else
                {
                    provision.Quality = 2;
                }
            }
            return provision;
        }
        internal static List<Provision> GetCustomerProvisions(ulong customerNumber, string currency, ushort type, ushort quality = 1)
        {
            List<Provision> provisions = new List<Provision>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sql = @"SELECT  case when matured_date is  null then N'Գործող' else N'Փակված' End AS status,L.IdPro ,PC.customer_number,TP.description, 
                               PC.Type, PC.Date_of_beginning  as activated_date, PC.Date_of_NormalEnd as matured_date,PC.Pro_Description,PC.Currency, PC.Amount, PC.account_Customer_Number, PC.closing_reason as matured_reason, PO.Customer_number Owner_Customer_number
                               FROM tbl_Provision_Of_Clients PC INNER JOIN Tbl_Link_application_Provision L On     PC.idpro = L.idpro
                               INNER JOIN  Tbl_Provision_Owners PO On PO.IdPro = L.IdPro 
                               INNER JOIN  Tbl_type_of_all_provision TP
                               ON PC.Type = TP.type_id";
                string whereCondition = " Where L.activated_Date Is Not Null and PO.owner_type in (1,2) And PO.customer_number = @customerNumber";
                if (currency != null && currency != string.Empty)
                {
                    whereCondition += " AND PC.Currency=@currency";
                }
                if (type != 0)
                {
                    whereCondition += " AND PC.Type=@type ";
                }
                if (quality == 1)
                {
                    whereCondition += " AND matured_date is  null ";
                }
                else if (quality == 2)
                {
                    whereCondition += " AND matured_date is not  null ";
                }
                sql += whereCondition;
                sql += @" Group by case when matured_date is  null then N'Գործող' else N'Փակված' End ,L.IdPro ,PC.customer_number, PC.Type,TP.description,
                          PC.date_of_beginning , PC.Date_of_NormalEnd, PC.Pro_Description, PC.currency, PC.Amount, PC.account_Customer_Number, PC.closing_reason, PO.Customer_number
                          order by PC.Date_of_beginning";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;
                    if (currency != null && currency != string.Empty)
                    {
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        Provision provision = SetProvision(row);
                        provisions.Add(provision);
                    }
                }

            }
            return provisions;
        }

        public static List<ProvisionLoan> GetProvisionLoans(ulong idProvision)
        {
            List<ProvisionLoan> provisonLoans = new List<ProvisionLoan>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select case when C.type_of_client = 6 then C.name + ' ' + C.lastname else C.description end as fullName, * from 
                                                 (Select  quality,App_Id, loan_full_number ,currency,credit_line_type as loan_type,start_capital,customer_number from Tbl_credit_lines union all 
                                                  Select  quality,App_Id, loan_full_number,currency,loan_type,start_capital,customer_number from [tbl_Short_Time_Loans;] union all 
                                                  Select  quality,App_Id, loan_full_number,currency,loan_type,start_capital,customer_number from Tbl_closed_credit_lines union all 
                                                  Select  quality,App_Id, loan_full_number,currency,loan_type,start_capital,customer_number from Tbl_closed_short_loans union all 
                                                  Select  quality,App_Id, 0 as loan_full_number,currency,loan_type,start_capital,customer_number from Tbl_given_guarantee) un 
                                                  inner join (Select quality as qualityDesc,Description_Engl,number from  [tbl_loan_list_quality;])  ll 
							                      on  ll.number = un.quality 
							                      LEFT JOIN V_CustomerDesription C 
                                                  On C.customer_number = un.customer_number  						 
                                                  Where un.App_id in (Select App_Id from tbl_Link_application_Provision Where Idpro=@idProvision)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@idProvision", SqlDbType.Float).Value = idProvision;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        ProvisionLoan loan = SetLoan(row);
                        provisonLoans.Add(loan);
                    }
                }

            }
            return provisonLoans;
        }

        private static ProvisionLoan SetLoan(DataRow row)
        {
            ProvisionLoan loan = new ProvisionLoan();
            if (row != null)
            {

                if (row["fullName"] != DBNull.Value)
                {
                    loan.LoanCustomerDescription = Utility.ConvertAnsiToUnicode(row["fullName"].ToString());
                }

                if (row["quality"] != DBNull.Value)
                {
                    loan.LoanQuality = Convert.ToInt16(row["quality"].ToString());
                }

                if (row["loan_full_number"] != DBNull.Value)
                {
                    loan.LoanAccount = row["loan_full_number"].ToString();
                }

                if (row["currency"] != DBNull.Value)
                {
                    loan.Currency = row["currency"].ToString();
                }

                if (row["loan_type"] != DBNull.Value)
                {
                    loan.Loantype = Convert.ToInt16(row["loan_type"].ToString());
                }

                if (row["start_capital"] != DBNull.Value)
                {
                    loan.StartCapital = Convert.ToDouble(row["start_capital"].ToString());
                }

                if (row["customer_number"] != DBNull.Value)
                {
                    loan.LoanCustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                }
                if (row["App_Id"] != DBNull.Value)
                {
                    loan.ProductId = Convert.ToUInt64(row["App_Id"].ToString());
                }
            }
            return loan;

        }

        /// <summary>
        /// Վերադարձնում է գրավի երաշխավորների հաճախորդի համարները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static List<ulong> GetProvisionOwners(ulong productId)
        {
            List<ulong> customerNumbers = new List<ulong>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_get_provision_guarantee_list";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@idpro", SqlDbType.VarChar, 50).Value = productId;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            customerNumbers.Add(Convert.ToUInt64(dt.Rows[i]["customer_number"]));
                        }
                    }
                }
            }
            return customerNumbers;
        }

        internal static bool HasPropertyProvision(ulong productId)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT  CONVERT(BIT,[dbo].[fn_Has_Application_Provision_8115](@productId)) result";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.VarChar, 50).Value = productId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = bool.Parse(dr["result"].ToString());
                        }
                    }
                }
            }
            return result;
        }

        internal static Dictionary<string, string> ProvisionContract(long docId)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"SELECT L.*,P.interest_rate_effective,P.interest_rate_effective_without_account_service_fee, P.repayment_kurs FROM 
                                            (SELECT Doc_ID,credit_line_type as loan_type,C.currency,start_date,C.end_date,credit_line_percent as loan_percent,amount,security_code_2,0 as service_fee, credit_line_type, A.visa_number  FROM [dbo].[Tbl_New_Credit_Line_Documents] C INNER JOIN Tbl_Visa_Numbers_Accounts A ON C.credit_line_account = A.card_account WHERE A.main_card_number is null and A.closing_date is null 
                                            UNION ALL 
                                            SELECT Doc_ID,loan_type,currency,start_date,end_date,loan_percent,amount,security_code_2,service_fee, null as credit_line_type, null as visa_number  FROM [dbo].[Tbl_New_Loan_Documents]) L 
                                            INNER JOIN [dbo].[Tbl_HB_loan_precontract_data] P ON L.doc_id = P.doc_id WHERE L.doc_id = @docId";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("docId", SqlDbType.BigInt).Value = docId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            keyValuePairs.Add("repaymentAmountHB", dr["service_fee"].ToString());
                            keyValuePairs.Add("loanTypeHB", dr["loan_type"].ToString());
                            keyValuePairs.Add("generalNumberHB", dr["security_code_2"] != DBNull.Value ? dr["security_code_2"].ToString() : "");
                            keyValuePairs.Add("interestRateHB", dr["loan_percent"] != DBNull.Value ? dr["loan_percent"].ToString() : "");
                            keyValuePairs.Add("interestRateFullHB", dr["interest_rate_effective"] != DBNull.Value ? dr["interest_rate_effective"].ToString() : "");
                            keyValuePairs.Add("interestRateEffectiveWithoutAccountServiceFee", dr["interest_rate_effective_without_account_service_fee"] != DBNull.Value ? dr["interest_rate_effective_without_account_service_fee"].ToString() : "");
                            if (dr["credit_line_type"] != DBNull.Value)
                            {
                                keyValuePairs.Add("creditLineTypeHB", dr["credit_line_type"] != DBNull.Value ? dr["credit_line_type"].ToString() : "");
                                keyValuePairs.Add("visaNumberHB", dr["visa_number"] != DBNull.Value ? dr["visa_number"].ToString() : "");
                                if (Convert.ToInt32(dr["credit_line_type"]) != 50)
                                {
                                    keyValuePairs.Add("decrAmountHB", dr["end_date"] != DBNull.Value ? Convert.ToDateTime(dr["end_date"]).ToString("dd/MMM/yyyy") : "0");
                                }
                            }
                            else
                            {
                                keyValuePairs.Add("dateOfNormalEndHB", dr["end_date"] != DBNull.Value ? Convert.ToDateTime(dr["end_date"]).ToString("dd/MMM/yyyy") : "0");
                            }
                            keyValuePairs.Add("RepaymentKurs", dr["repayment_kurs"] != DBNull.Value ? dr["repayment_kurs"].ToString() : "0");
                        }
                    }
                    return keyValuePairs;
                }
            }
        }

    }
}
