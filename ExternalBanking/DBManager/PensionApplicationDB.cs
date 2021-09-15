using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class PensionApplicationDB
    {
        internal static bool HasPensionApplication(ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT contract_id FROM Tbl_pension_application WHERE deleted = 0 and quality not in (40,41,42) and customer_number=@customernumber", conn);

                cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }

            }
            return check;
        }

        internal static List<PensionApplication> GetPensionApplicationHistory(ulong customerNumber)
        {
            List<PensionApplication> list = new List<PensionApplication>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"  select  p.contract_id,
                                                            p.filialcode,
                                                            p.contract_number,
                                                            p.customer_number,
                                                            '' as name,
                                                            p.registration_date,
                                                            p.quality, 
                                                            p.card_number, 
                                                            p.account_number, 
                                                            p.number_of_set, 
                                                            p.contract_date,
                                                            p.date_of_normal_end,
                                                            p.service_type,
                                                            p.card_type,
                                                            p.deleted
                                                            from Tbl_pension_application as p
                                                            WHERE deleted = 0  and p.customer_number = @customernumber and p.quality in(0,10)
                                                            ORDER BY contract_id desc ", conn);

                cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    PensionApplication pensionApplication = SetPensionApplication(row);

                    list.Add(pensionApplication);
                }


            }
            return list;
        }


        internal static List<PensionApplication> GetClosedPensionApplicationHistory(ulong customerNumber)
        {
            List<PensionApplication> list = new List<PensionApplication>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"  select  p.contract_id,
                                                            p.filialcode,
                                                            p.contract_number,
                                                            p.customer_number,
                                                            '' as name,
                                                            p.registration_date,
                                                            p.quality, 
                                                            p.card_number, 
                                                            p.account_number, 
                                                            p.number_of_set, 
                                                            p.contract_date,
                                                            p.date_of_normal_end,
                                                            p.service_type,
                                                            p.card_type,
                                                            p.deleted
                                                            from Tbl_pension_application as p 
                                                            WHERE deleted = 0  and p.customer_number = @customernumber and p.quality not in(0,10)
                                                            ORDER BY contract_id desc ", conn);

                cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    PensionApplication pensionApplication = SetPensionApplication(row);

                    list.Add(pensionApplication);
                }


            }
            return list;
        }

        private static PensionApplication SetPensionApplication(DataRow row)
        {
            PensionApplication pensionApplication = new PensionApplication();

            if (row != null)
            {
                pensionApplication.ContractId = Convert.ToUInt64(row["contract_id"]);
                pensionApplication.ContractNumber = Convert.ToUInt64(row["contract_number"]);
                pensionApplication.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                pensionApplication.FullName = Utility.ConvertAnsiToUnicode(row["name"].ToString());
                pensionApplication.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                pensionApplication.Quality = Convert.ToInt16(row["quality"]);
                pensionApplication.QualityDescription = Utility.ConvertAnsiToUnicode(Info.GetPensionAppliactionQualityType(pensionApplication.Quality));
                if (row["card_number"] != DBNull.Value)
                {
                    pensionApplication.Account=Account.GetAccount(row["account_number"].ToString());
                    pensionApplication.Account.ProductNumber=row["card_number"].ToString();
                }
                else if(row["account_number"] != DBNull.Value)
                {
                    pensionApplication.Account=Account.GetAccount(row["account_number"].ToString());
                    pensionApplication.Account.ProductNumber = null;
                }
                pensionApplication.SetNumber = Convert.ToInt32(row["number_of_set"]);
                pensionApplication.FillialCode = Convert.ToInt32(row["filialcode"]);
                if (row["contract_date"] != DBNull.Value)
                    pensionApplication.ContractDate = Convert.ToDateTime(row["contract_date"]);
                if (row["date_of_normal_end"] != DBNull.Value)
                    pensionApplication.DateOfNormalEnd = Convert.ToDateTime(row["date_of_normal_end"]);
                pensionApplication.ServiceType = Convert.ToInt16(row["service_type"]);
                pensionApplication.ServiceTypeDescription = Utility.ConvertAnsiToUnicode(Info.GetPensionAppliactionServiceType(pensionApplication.ServiceType));
                if (row["card_type"] != DBNull.Value)
                    pensionApplication.CardType = Convert.ToInt16(row["card_type"]);

                if (Convert.ToInt16(row["deleted"]) == 0)
                    pensionApplication.Deleted = false;
                else
                    pensionApplication.Deleted=true;

            }
            return pensionApplication;
        }


        internal static PensionApplication GetPensionApplication(ulong customerNumber, ulong contractId)
        {
            PensionApplication pensionApplication = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                
                SqlCommand cmd = new SqlCommand(@"  select  p.contract_id,
                                                            p.filialcode,
                                                            p.contract_number,
                                                            p.customer_number,
                                                            '' as name,
                                                            p.registration_date,
                                                            p.quality, 
                                                            p.card_number, 
                                                            p.account_number, 
                                                            p.number_of_set, 
                                                            p.contract_date,
                                                            p.date_of_normal_end,
                                                            p.service_type,
                                                            p.card_type,
                                                            p.deleted
                                                            from Tbl_pension_application as p 
                                                            WHERE p.customer_number = @customernumber and p.contract_id=@contract_id
                                                            ORDER BY contract_id desc ", conn);

                cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@contract_id", SqlDbType.Float).Value = contractId;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    pensionApplication = SetPensionApplication(row);

                   
                }


            }
            return pensionApplication;
        }

    }
}
