using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class SearchSwiftCodesDB
    {
        internal static List<SearchSwiftCodes> GetSwiftCodes(SearchSwiftCodes searchParams)
        {
            List<SearchSwiftCodes> swiftCodeList = new List<SearchSwiftCodes>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {


                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    sql = @"SELECT TOP 1000 swift_code, city, bank_name FROM Tbl_world_banks  WHERE 1=1 ";
                    if (searchParams.SwiftCode != null)
                    {
                        sql = sql + " and swift_code like '%" + searchParams.SwiftCode + "%'  ";
                    }

                    if (searchParams.BankName != null)
                    {
                        sql = sql + " and bank_name like '%" + searchParams.BankName + "%'  ";
                    }

                    if (searchParams.City != null)
                    {
                        sql = sql + " and city like '%" + searchParams.City + "%'  ";
                    }

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            swiftCodeList = new List<SearchSwiftCodes>();
                        }
                        while (dr.Read())
                        {
                            SearchSwiftCodes oneResult = new SearchSwiftCodes();
                            oneResult.SwiftCode = dr["swift_code"].ToString();
                            oneResult.City = dr["city"].ToString();
                            oneResult.BankName = dr["bank_name"].ToString();

                            swiftCodeList.Add(oneResult);
                        }



                    }
                }

                return swiftCodeList;
            }
        }
    }
}