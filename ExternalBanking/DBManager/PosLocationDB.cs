using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class PosLocationDB
    {
        internal static List<PosLocation> GetCustomerPosLocations(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PosLocation> posLocations = new List<PosLocation>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT sel.Location_id,sel.location_name,sel.open_date,sel.closing_date,sel.location_adress,sel.location_phone,sel.Location_type,sel.quality,sel.add_inf
                          FROM Tbl_SE_Locations sel
                          INNER JOIN Tbl_Service_Establishment se
                          on se.SE_ID=sel.SE_ID
                          WHERE se.customer_number = @customerNumber";
                string whereCondition = "";
                if (filter == ProductQualityFilter.Opened)
                {
                    whereCondition = " AND sel.closing_date IS NULL ";
                }
                else if (filter == ProductQualityFilter.Closed)
                {
                    whereCondition = " AND sel.closing_date IS NOT NULL ";
                }
                sql += whereCondition;
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
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            PosLocation posLocation = SetPosLocation(row);
                            posLocations.Add(posLocation);
                        }
                    }
                }
            }
            return posLocations;
        }

        private static PosLocation SetPosLocation(DataRow dr)
        {
            PosLocation posLocation = new PosLocation();
            if (dr != null)
            {
                posLocation.Id = Convert.ToInt32(dr["Location_id"]);
                posLocation.Description = Utility.ConvertAnsiToUnicode(dr["location_name"].ToString());
                posLocation.OpenDate=Convert.ToDateTime(dr["open_date"]);
                posLocation.ClosedDate = dr["closing_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["closing_date"]);
                posLocation.Address = Utility.ConvertAnsiToUnicode(dr["location_adress"].ToString());
                posLocation.Phone = Utility.ConvertAnsiToUnicode(dr["location_phone"].ToString());
                posLocation.LocationType = (dr["Location_type"] != DBNull.Value) ? Convert.ToUInt16(dr["Location_type"]) : default(ushort);
                posLocation.Quality = (dr["quality"] != DBNull.Value) ? Convert.ToUInt16(dr["quality"]) : default(ushort);
                posLocation.AdditionalDescription = Utility.ConvertAnsiToUnicode(dr["add_inf"].ToString());
            }
            return posLocation;
        }

        internal static void GetPosLocation(PosLocation posLocation)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT location_name,Location_id,location_descr,open_date,closing_date,location_adress,location_phone,Location_type,quality,add_inf
                                                  FROM Tbl_SE_Locations 
                                                  WHERE Location_id = @posLocationId", conn))
                {
                    cmd.Parameters.AddWithValue("@posLocationId", posLocation.Id);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            posLocation.Id = Convert.ToInt32(dr["Location_id"]);
                            posLocation.Description = Utility.ConvertAnsiToUnicode(dr["location_name"].ToString());
                            posLocation.OpenDate = Convert.ToDateTime(dr["open_date"]);
                            posLocation.ClosedDate = dr["closing_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["closing_date"]);
                            posLocation.Address = Utility.ConvertAnsiToUnicode(dr["location_adress"].ToString());
                            posLocation.Phone = Utility.ConvertAnsiToUnicode(dr["location_phone"].ToString());
                            posLocation.LocationType = (dr["Location_type"] != DBNull.Value) ? Convert.ToUInt16(dr["Location_type"]) : default(ushort);
                            posLocation.Quality = (dr["quality"] != DBNull.Value) ? Convert.ToUInt16(dr["quality"]) : default(ushort);
                            posLocation.AdditionalDescription = Utility.ConvertAnsiToUnicode(dr["add_inf"].ToString());
                        }
                    }
                }
                   
            }
        }


        internal static void GetPosTerminals(PosLocation posLocation)
        {
            posLocation.Posterminals = new List<PosTerminal>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd =new SqlCommand(@"SELECT * FROM Tbl_ARCA_points_list WHERE Location_id=@posLocationId", conn))
                {
                    cmd.Parameters.AddWithValue("@posLocationId", posLocation.Id);

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            PosTerminal posTerminal = new PosTerminal();
                            posTerminal.Id = Convert.ToInt32(dr["code"]);
                            posTerminal.Description = dr["description"].ToString();
                            posTerminal.TerminalID = dr["merchant_ID"].ToString();
                            posTerminal.AmexMID = dr["Amex_terminal_id"].ToString();
                            posTerminal.Quality = Convert.ToUInt16(dr["quality"].ToString());
                            posTerminal.Type = Convert.ToUInt16(dr["type_of_point"].ToString());
                            posTerminal.FilialCode = Convert.ToInt32(dr["Filial"].ToString());
                            posTerminal.OwnerBankCode = Convert.ToInt32(dr["BankCode"].ToString());
                            posTerminal.AccountNumber = dr["account_number"].ToString();
                            posLocation.Posterminals.Add(posTerminal);
                        }
                    }
                }
    
            }
        }

    }
}
