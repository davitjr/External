using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class DepositCaseDB
    {

        private static string depositCaseSelectScript = @"SELECT
                                                                customer_number,
                                                                app_id,
                                                                date_of_beginning,
                                                                date_of_normal_end,
                                                                case_number,
                                                                filialcode,
                                                                quality,
                                                                closing_date,
                                                                penalty_amount,
                                                                add_customer_number,
                                                                contract_number,
                                                                contract_type,
                                                                servicing_set_number,
                                                                contract_duration,
                                                                recontract_possibility,
                                                                date_of_stopping_penalty_calculation,
                                                                document_date,
                                                                changing_reason,
                                                                change_set_number
                                                                FROM
                                                                Tbl_deposit_case_contracts ";



        internal static List<DepositCase> GetDepositCases(ulong customerNumber)
        {
            List<DepositCase> list = new List<DepositCase>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = depositCaseSelectScript + " WHERE (customer_number=@customer_number or add_customer_number=@customer_number) and quality<>40";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        DepositCase depositCase = SetDepositCase(row);

                        list.Add(depositCase);
                    }

                }

            }
            return list;
        }

        internal static List<DepositCase> GetClosedDepositCases(ulong customerNumber)
        {
            List<DepositCase> list = new List<DepositCase>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = depositCaseSelectScript + " WHERE (customer_number=@customer_number or add_customer_number=@customer_number) and quality=40";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        DepositCase depositCase = SetDepositCase(row);

                        list.Add(depositCase);
                    }

                }

            }
            return list;
        }

        private static DepositCase SetDepositCase(DataRow row)
        {
            DepositCase depositCase = new DepositCase();

            if (row != null)
            {
                depositCase.ProductId = long.Parse(row["app_id"].ToString());
                depositCase.CustomerNumber = ulong.Parse(row["customer_number"].ToString());
                depositCase.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                depositCase.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                depositCase.CaseNumber = row["case_number"].ToString();
                depositCase.FilialCode = short.Parse(row["filialcode"].ToString());
                depositCase.ClosingDate = row["closing_date"] != DBNull.Value ? DateTime.Parse(row["closing_date"].ToString()) : default(DateTime?);
                depositCase.Quality = short.Parse(row["quality"].ToString());
                depositCase.PenaltyAmount = row["penalty_amount"] != DBNull.Value ? double.Parse(row["penalty_amount"].ToString()) : 0;
                depositCase.ContractType = short.Parse(row["contract_type"].ToString());
                depositCase.ContractNumber = ulong.Parse(row["contract_number"].ToString());
                if (row["servicing_set_number"] != DBNull.Value)
                    depositCase.ServicingSetNumber = int.Parse(row["servicing_set_number"].ToString());
                depositCase.JointCustomers = new List<KeyValuePair<ulong, string>>();
                depositCase.JointCustomers.Add(new KeyValuePair<ulong, string>(ulong.Parse(row["customer_number"].ToString()), ""));

                if (row["add_customer_number"] != DBNull.Value)
                    depositCase.JointCustomers.Add(new KeyValuePair<ulong, string>(ulong.Parse(row["add_customer_number"].ToString()), ""));
                if (depositCase.JointCustomers.Count > 1)
                    depositCase.JointType = 1;
                depositCase.ContractDuration = short.Parse(row["contract_duration"].ToString());
                SetDepositCaseConectedAccounts(depositCase);

                if (row["recontract_possibility"] != DBNull.Value)
                    depositCase.RecontractPossibility = Convert.ToBoolean(row["recontract_possibility"]);

                if (row["date_of_stopping_penalty_calculation"] != DBNull.Value)
                    depositCase.DateOfStoppingPenaltyCalculation = Convert.ToDateTime(row["date_of_stopping_penalty_calculation"]);

                if (row["document_date"] != DBNull.Value)
                    depositCase.DocumentDate = Convert.ToDateTime(row["document_date"]);

                if (row["changing_reason"] != DBNull.Value)
                    depositCase.ChangingReason = Utility.ConvertAnsiToUnicode(row["changing_reason"].ToString());

                if (row["change_set_number"] != DBNull.Value)
                    depositCase.ChangeSetNumber = Convert.ToInt32(row["change_set_number"]);


            }
            return depositCase;
        }
        internal static DepositCase GetDepositCase(ulong productId, ulong customerNumber)
        {
            DepositCase depositcase = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = depositCaseSelectScript + " WHERE (customer_number=@customer_number or add_customer_number=@customer_number) and app_id=@product_id";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        depositcase = SetDepositCase(row);
                    }
                }

            }
            return depositcase;
        }

        /// <summary>
        /// Ստուգում է պահատուփի համարի ազատ լինելը
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        internal static bool CheckDepositCaseNumber(string caseNumber, int filialCode)
        {

            bool check = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT case_ID FROM Tbl_deposit_case_map WHERE in_use = 0 AND case_number =@case_number AND filialcode=@filial_code";
                    cmd.Parameters.Add("@case_number", SqlDbType.Float).Value = float.Parse(caseNumber);
                    cmd.Parameters.Add("@filial_code", SqlDbType.Int).Value = filialCode;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            check = true;
                        }

                    }


                }

            }

            return check;

        }


        internal static List<DepositCaseMap> GetDepositCaseMap(int filialCode, short caseSide)
        {
            List<DepositCaseMap> maps = new List<DepositCaseMap>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    string sqlText = @"SELECT * FROM Tbl_deposit_case_map  WHERE filialcode =@filial_code" + (filialCode == 22041 ? " and case_side=@caseSide" : "") + " ORDER BY case_side, case_number";
                    cmd.CommandText = sqlText;
                    cmd.Parameters.Add("@filial_code", SqlDbType.Float).Value = filialCode;
                    if (filialCode == 22041)
                        cmd.Parameters.Add("@caseSide", SqlDbType.Float).Value = caseSide;
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            maps.Add(SetDepositCaseMap(dt.Rows[i]));
                        }
                    }
                    else
                    {
                        maps = null;
                    }

                }

            }
            return maps;
        }


        private static DepositCaseMap SetDepositCaseMap(DataRow row)
        {
            DepositCaseMap depositCaseMap = new DepositCaseMap();

            if (row != null)
            {

                depositCaseMap.CaseId = ulong.Parse(row["case_ID"].ToString());
                depositCaseMap.CaseNumber = ulong.Parse(row["case_number"].ToString());
                if (row["case_side"] != DBNull.Value)
                    depositCaseMap.CaseSide = short.Parse(row["case_side"].ToString());
                depositCaseMap.CaseType = short.Parse(row["case_type"].ToString());
                depositCaseMap.FilialCode = int.Parse(row["filialcode"].ToString());
                depositCaseMap.InUse = bool.Parse(row["in_use"].ToString());

            }
            return depositCaseMap;
        }

        /// <summary>
        /// Վերադարձնում է պահատուփի ակտիվացման համար գումարը
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        internal static double GetDepositCasePrice(string caseNumber, int filialCode, short contractDuration)
        {

            double price = 0;


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT price FROM Tbl_deposit_case_map m INNER JOIN Tbl_deposit_case_prices p ON m.case_type =p.case_type  WHERE filialcode =@filial_code AND case_number =@case_number AND contract_duration = @contract_duration";
                    cmd.Parameters.Add("@case_number", SqlDbType.Float).Value = float.Parse(caseNumber);
                    cmd.Parameters.Add("@filial_code", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@contract_duration", SqlDbType.Int).Value = contractDuration;

                    if (cmd.ExecuteScalar() != null)
                    {
                        price = double.Parse(cmd.ExecuteScalar().ToString());
                    }
                }

            }

            return price;

        }

        internal static void SetDepositCaseConectedAccounts(DepositCase depositCase)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT  p.type_of_account,
                                                p.account_number, 
                                                pt.Description + '` ' + t.Description,  
                                                a.currency,
                                                a.type_of_account,
                                                a.type_of_account_new,
                                                a.description
                                                FROM Tbl_Products_Accounts p
                                                INNER JOIN Tbl_Products_Accounts_Groups g
                                                ON p.Group_ID =g.Group_ID 
                                                INNER JOIN [Tbl_all_accounts;] a 
                                                ON p.account_number = a.arm_number 
                                                INNER JOIN Tbl_type_of_accounts t 
                                                ON p.type_of_account =t.id 
                                                INNER JOIN Tbl_type_of_products pt 
                                                ON p.type_of_product =pt.code 
                                                WHERE app_id = @app_id 
                                                ORDER BY p.type_of_account";
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = depositCase.ProductId;

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i]["type_of_account"]) == 24)
                        {
                            depositCase.ConnectAccount = Account.GetAccount(dt.Rows[i]["account_number"].ToString());
                        }
                        else
                        {
                            depositCase.OutBalanceAccount = Account.GetAccount(dt.Rows[i]["account_number"].ToString());
                        }
                    }
                }

            }


        }






    }
}
