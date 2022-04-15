using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class InsuranceDB
    {



        public static List<Insurance> GetInsurances(ulong customerNumber)
        {
            List<Insurance> insurances = new List<Insurance>();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT    insurance_amount,
                                                            company_ID,
                                                            product_app_id,
                                                            main_app_id,
                                                            app_id,
                                                            quality,
                                                            filialcode,
                                                            insurance_type,
                                                            date_of_beginning,
                                                            date_of_normal_end,
                                                            keeper_open,
                                                            currency,
                                                            compensation_amount,
                                                            compensation_amount_currency,
                                                            contract_type
                                                            FROM tbl_insurance_contracts
                                                            WHERE customer_number=@customerNumber AND quality<>40", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

            DataTable dt = new DataTable();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {

                dt.Load(dr);
            }

            if (dt.Rows.Count > 0)
                insurances = new List<Insurance>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                DataRow row = dt.Rows[i];

                Insurance insurance = SetInsurance(row);

                insurances.Add(insurance);

            }


            return insurances;







        }

        public static List<Insurance> GetClosedInsurances(ulong customerNumber)
        {
            List<Insurance> insurances = new List<Insurance>();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT    insurance_amount,
                                                            company_ID,
                                                            product_app_id,
                                                            main_app_id,
                                                            app_id,
                                                            quality,
                                                            filialcode,
                                                            insurance_type,
                                                            date_of_beginning,
                                                            date_of_normal_end,
                                                            keeper_open,
                                                            currency,
                                                            compensation_amount,
                                                            compensation_amount_currency,
                                                            contract_type,
                                                            CASE WHEN (SELECT GETDATE()) < date_of_normal_end AND quality = 40 THEN 1 ELSE 0 END AS manual_deleted
                                                            FROM tbl_insurance_contracts
                                                            WHERE customer_number=@customerNumber AND quality=40", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

            DataTable dt = new DataTable();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {

                dt.Load(dr);
            }

            if (dt.Rows.Count > 0)
                insurances = new List<Insurance>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                DataRow row = dt.Rows[i];

                Insurance insurance = SetInsurance(row);

                insurances.Add(insurance);

            }


            return insurances;







        }

        public static Insurance GetInsurance(ulong customerNumber, ulong productId)
        {
            Insurance insurance = null;

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT    insurance_amount,
                                                            company_ID,
                                                            product_app_id,
                                                            main_app_id,
                                                            app_id,
                                                            quality,
                                                            filialcode,
                                                            insurance_type,
                                                            date_of_beginning,
                                                            date_of_normal_end,
                                                            keeper_open,
                                                            currency,
                                                            compensation_amount,
                                                            compensation_amount_currency,
                                                            contract_type
                                                            FROM tbl_insurance_contracts
                                                            WHERE customer_number=@customerNumber AND app_id=@productId", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
            cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

            using DataTable dt = new DataTable();

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                dt.Load(dr);
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                insurance = SetInsurance(row);
            }


            return insurance;







        }


        public static List<Insurance> GetPaidInsurance(ulong customerNumber, ulong loanProductId)
        {
            List<Insurance> insurances = new List<Insurance>();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"
                                                    SELECT  ct.insurance_amount,
                                                            ct.company_ID,
                                                            ct.product_app_id,
                                                            ct.main_app_id,
                                                            ct.app_id,
                                                            ct.quality,
                                                            ct.filialcode,
                                                            ct.insurance_type,
                                                            ct.date_of_beginning,
                                                            ct.date_of_normal_end,
                                                            ct.keeper_open,
                                                            ct.currency,
                                                            ct.compensation_amount,
                                                            ct.compensation_amount_currency,
                                                            ct.contract_type
                                                            from Tbl_insurance_contracts ct
                                                            INNER JOIN Tbl_paid_factoring pa
                                                            ON ct.app_id=pa.main_app_id
                                                            WHERE ct.customer_number=@customerNumber
                                                            AND ct.quality<>40 AND pa.App_Id=@productId", conn);
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
            cmd.Parameters.Add("@productId", SqlDbType.Float).Value = loanProductId;

            using DataTable dt = new DataTable();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {

                dt.Load(dr);
            }

            if (dt.Rows.Count > 0)
                insurances = new List<Insurance>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                DataRow row = dt.Rows[i];

                Insurance insurance = SetInsurance(row);

                insurances.Add(insurance);

            }


            return insurances;







        }

        private static Insurance SetInsurance(DataRow row)
        {
            Insurance insurance = new Insurance();

            if (row != null)
            {
                insurance.Amount = double.Parse(row["insurance_amount"].ToString());
                insurance.InsuranceContractType = int.Parse(row["contract_type"].ToString());
                insurance.Company = ushort.Parse(row["company_ID"].ToString());
                if (row["product_app_id"] != DBNull.Value)
                    insurance.ConectedProductId = long.Parse(row["product_app_id"].ToString());
                if (row["main_app_id"] != DBNull.Value)
                    insurance.MainProductId = long.Parse(row["main_app_id"].ToString());
                insurance.ProductId = long.Parse(row["app_id"].ToString());
                insurance.Quality = short.Parse(row["quality"].ToString());
                insurance.FillialCode = int.Parse(row["filialcode"].ToString());
                insurance.InsuranceType = ushort.Parse(row["insurance_type"].ToString());
                insurance.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                insurance.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                insurance.InvolvingSetNumber = int.Parse(row["keeper_open"].ToString());
                if (row.Table.Columns.Contains("manual_deleted"))
                {
                    insurance.IsManualDeleted = Convert.ToBoolean(row["manual_deleted"]);
                }
                insurance.Currency = row["currency"].ToString();
                if (row["compensation_amount"] != DBNull.Value)
                    insurance.CompensationAmount = double.Parse(row["compensation_amount"].ToString());

                if (row["compensation_amount_currency"] != DBNull.Value)
                    insurance.CompensationCurrency = row["compensation_amount_currency"].ToString();
                else
                    insurance.CompensationCurrency = "AMD";

            }
            return insurance;
        }

        internal static uint GetInsuranceCompanySystemAccountNumber(ushort companyID, ushort insuranceType)
        {
            uint result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT company_system_account_number 
                                        FROM Tbl_type_of_insurance_company 
                                        WHERE company_ID=@companyID AND insurance_type=@insuranceType AND company_status=1";
                cmd.Parameters.Add("@companyID", SqlDbType.SmallInt).Value = companyID;
                cmd.Parameters.Add("@insuranceType", SqlDbType.SmallInt).Value = insuranceType;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result = Convert.ToUInt32(dr["company_system_account_number"].ToString());
                }
            }

            return result;
        }
    }
}
