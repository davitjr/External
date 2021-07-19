using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Globalization;

namespace ExternalBanking.DBManager
{
    internal class ProductNoteDB
    {
        internal static ActionResult SaveProductNote(ProductNote productNote)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_add_update_notes";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@uID", SqlDbType.Float).Value = productNote.UniqueId;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 255).Value = productNote.Description;


                    cmd.Parameters.Add(new SqlParameter("@ret_code", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    //byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                  
                    result.ResultCode = ResultCode.Normal;
                    
                }
            }

            return result;
        }


        internal static ProductNote GetProductNote(double uniqueId)
        {
            ProductNote productNote = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT Unique_ID,dbo.fnc_convertAnsiToUnicode(Description) as Description, modify_date 
                               FROM Tbl_HB_products_descriptions WHERE Unique_ID = @Unique_ID ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Unique_ID", SqlDbType.Float).Value = uniqueId;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        productNote = new ProductNote();
                        productNote.Description = row["Description"].ToString();
                        productNote.UniqueId = uniqueId;
                        productNote.ModifyDate = Convert.ToDateTime(row["modify_date"].ToString());
                    }
                }
            }

            return productNote;
        }
    }
}
