using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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
                posLocation.OpenDate = Convert.ToDateTime(dr["open_date"]);
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
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM Tbl_ARCA_points_list WHERE Location_id=@posLocationId", conn))
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

        internal static ActionResult SaveNewPosLocationOrder(NewPosLocationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_Pos_Terminal_Insert_Order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.NVarChar, 20).Value = order.SubType;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.user.filialCode;
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = order.FullPhoneNumber;
                    cmd.Parameters.Add("@Name_Eng", SqlDbType.NVarChar).Value = order.NameEng;
                    cmd.Parameters.Add("@Name_Arm", SqlDbType.NVarChar).Value = order.NameArm;
                    cmd.Parameters.Add("@Activity_Sphere", SqlDbType.NVarChar, 20).Value = order.ActivitySphere;
                    cmd.Parameters.Add("@E_Mail", SqlDbType.NVarChar).Value = order.Mail;
                    cmd.Parameters.Add("@Contact_Person", SqlDbType.NVarChar, 20).Value = order.ContactPerson;
                    cmd.Parameters.Add("@Contact_Person_Phone", SqlDbType.NVarChar, 20).Value = order.ContactPersonFullPhone;
                    cmd.Parameters.Add("@Pos_Count", SqlDbType.Int).Value = order.PosCount;
                    cmd.Parameters.Add("@Pos_Serial_number", SqlDbType.NVarChar).Value = order.PosSerialNumber;
                    cmd.Parameters.Add("@PosType", SqlDbType.SmallInt).Value = order.PosType;
                    cmd.Parameters.Add("@Pay_Without_Card", SqlDbType.Bit).Value = order.PayWithoutCard;
                    cmd.Parameters.Add("@All_Terminals", SqlDbType.Bit).Value = order.AllTerminals;
                    cmd.Parameters.Add("@SiteOrApp", SqlDbType.NVarChar).Value = order.SiteOrApp;
                    cmd.Parameters.Add("@TerminalType", SqlDbType.SmallInt).Value = order.TerminalType;
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar).Value = order.AccountNumber;
                    cmd.Parameters.Add("@Necessity", SqlDbType.NVarChar).Value = order.Necessity;
                    cmd.Parameters.Add("@NewHdm", SqlDbType.Bit).Value = order.NewHdm;
                    cmd.Parameters.Add("@Pos_Address", SqlDbType.NVarChar).Value = order.PosAddress;
                    cmd.Parameters.Add("@Main_Bank", SqlDbType.NVarChar).Value = order.MainBank;



                    DataTable TableForParam = GetCardSystemForServiceDataTable(order.CardSystemForService);

                    cmd.Parameters.Add("@TerminalServiceCardSystemTypes", SqlDbType.Structured).Value = TableForParam;


                    SqlParameter res = new SqlParameter("@result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    return result;
                }
            }
        }

        private static DataTable GetCardSystemForServiceDataTable(List<NewPosLocationServiceCardTypes> CardSystemForService)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CardsystemId");
            dt.Columns.Add("Fee");
            dt.Columns.Add("FeeInt");

            if (CardSystemForService.Any())
            {             
                foreach (var item in CardSystemForService)
                {
                    DataRow dr = dt.NewRow();                  
                   
                        dr["CardsystemId"] = item.Id;
                        dr["Fee"] = item.Fee;
                        dr["FeeInt"] = item.FeeInt;

                    dt.Rows.Add(dr);
                }          
            }

            return dt;
        }

        internal static NewPosLocationOrder NewPosApplicationOrderDetails(long orderId)
        {
            NewPosLocationOrder result = new NewPosLocationOrder();
            DataTable dt = new DataTable();

            string sql = @"SELECT   hb.customer_number,
                                    hb.registration_date, 
                                    hb.doc_ID,
                                    hb.document_subtype,
                                    hb.quality,
                                    hb.operation_date,
                                    hb.source_type,
                                    hb.document_type,
                                    hb.filial,
                                    hb.sender_phone,
                                    hb.reason_type_description,
                                    dbo.fnc_convertAnsiToUnicode(q.description_arm) description_arm
                                        FROM Tbl_HB_documents hb inner join Tbl_types_of_HB_quality q on q.quality = hb.quality
                                        WHERE hb.Doc_ID = @DocID";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = orderId;

                    dt.Load(cmd.ExecuteReader());

                    result.Id = Convert.ToInt64(dt.Rows[0]["doc_ID"].ToString());
                    result.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    result.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    result.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    result.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    result.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    result.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                    result.QualityDescription = dt.Rows[0]["description_arm"].ToString();
                    result.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["customer_number"].ToString());

                    // result.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"]);
                }

                return result;
            }
        }

        internal static List<string> GetPosTerminalActivitySphere()
        {
            List<string> ActivitySphere = new List<string>();

                DataTable dt = new DataTable();

            string sql = @"SELECT Sphere FROM tbl_Pos_Terminals_Activity_Sphere ORDER BY ID";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    dt.Load(cmd.ExecuteReader());

                    foreach (DataRow dr in dt.Rows)
                    {
                        ActivitySphere.Add(dr["Sphere"].ToString());
                    }
                }

                return ActivitySphere;
            }
        }

       

    }
}
