using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
   public class FondDB
    {
        internal static List <Fond> GetFonds()
        {
            List<Fond> fonds = new List<Fond>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM [Tbl_fonds;] WHERE is_active = 1 ";
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Fond fond = SetFond(row);

                        fonds.Add(fond);
                    }

                }

            }
            return fonds;

        }

        internal static List<Fond> GetClosedFonds()
        {
            List<Fond> fonds = new List<Fond>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM [Tbl_fonds;] WHERE is_active = 0 ";
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Fond fond = SetFond(row);

                        fonds.Add(fond);
                    }

                }

            }
            return fonds;

        }

        internal static Fond GetFond(int ID)
        {
            Fond fond = new Fond();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM [Tbl_fonds;] WHERE code = @ID";
                    cmd.Parameters.Add("@ID", SqlDbType.Float).Value = ID;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        fond = SetFond(row);
                    }
                }

            }
            return fond;
        }

        private static Fond SetFond(DataRow row)
        {
            Fond fond = new Fond();

            if (row != null)
            {
                fond.ID = int.Parse(row["code"].ToString());
                fond.Description = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                fond.IsActive = byte.Parse(row["is_active"].ToString());
                fond.IsSubsidia = byte.Parse(row["is_subsidia"].ToString());
                fond.ProvidingDetails = GetFondProvidingDetails(fond.ID);
            }

            return fond;
        }

        private static List<FondProvidingDetail> GetFondProvidingDetails (int ID)
        {
            List<FondProvidingDetail> providingDetails = new List<FondProvidingDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM [Tbl_fonds_providing_details] WHERE  providing_termination_date IS NULL AND fond_code = @ID";
                    cmd.Parameters.Add("@ID", SqlDbType.Float).Value = ID;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        FondProvidingDetail  providingDetail = SetFondProvidingDetail(row);

                        providingDetails.Add(providingDetail);
                    }

                }

            }

            return providingDetails;
        }

        private static FondProvidingDetail SetFondProvidingDetail(DataRow row)
        {
            FondProvidingDetail providingDetail = new FondProvidingDetail();

            if (row != null)
            {
                providingDetail.Id = ulong.Parse(row["ID"].ToString());
                providingDetail.FondID = int.Parse(row["fond_code"].ToString());
                providingDetail.Currency = row["currency"].ToString();
                if (row["filial_interest_rate"] != DBNull.Value)
                    providingDetail.InterestRate = float.Parse(row["filial_interest_rate"].ToString());
                if (row["providing_termination_date"] != DBNull.Value)
                    providingDetail.TerminationDate = Convert.ToDateTime(row["providing_termination_date"]);               
            }

            return providingDetail;
        }

        internal static ActionResult SaveFondOrder(FondOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_fond_document";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@fondID", SqlDbType.Int).Value = order.Fond.ID;
                    cmd.Parameters.Add("@fondDescription", SqlDbType.NVarChar, 250).Value = order.Fond.Description;
                    cmd.Parameters.Add("@isSubsidia", SqlDbType.Bit).Value = order.Fond.IsSubsidia;
                    cmd.Parameters.Add("@isActive", SqlDbType.Bit).Value = order.Fond.IsActive;
                    
                    SqlParameter param = new SqlParameter("@ID", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@ID"].Value);
                    result.ResultCode = ResultCode.Normal;
                    result.Id = order.Id;

                    return result;
                }
            }
        }

        internal static void SaveFondProvidingDetails(FondOrder order)
        {

            order.Fond.ProvidingDetails?.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_fond_providing_order_details
                                                            (Doc_ID, fond_ID, currency, filial_interest_rate, providing_termination_date)
                                                            VALUES 
                                                            (@DocID, @fondID, @currency, @filialInterestRate, @providingTerminationDate)", conn))
                    {
                        cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = order.Id;
                        cmd.Parameters.Add("@fondID", SqlDbType.Int).Value = order.Fond.ID;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = m.Currency;
                        cmd.Parameters.Add("@filialInterestRate", SqlDbType.Float).Value = Math.Round(m.InterestRate/100, 4);
                        cmd.Parameters.Add("@providingTerminationDate", SqlDbType.SmallDateTime).Value = m.TerminationDate != null ? (object)m.TerminationDate : DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                }
            });


        }



    }
}
