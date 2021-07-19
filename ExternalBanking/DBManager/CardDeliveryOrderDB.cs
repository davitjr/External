using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class CardDeliveryOrderDB
    {
        public static ActionResult DownloadOrderXMLs(DateTime DateFrom, DateTime DateTo)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Get_XML_Files_By_Setdate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DateFrom", SqlDbType.SmallDateTime).Value = DateFrom;
                    cmd.Parameters.Add("@DateTo", SqlDbType.SmallDateTime).Value = DateTo;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;


                    return result;
                }
            }

        }



        //public static ActionResult DownloadCardDeliveryXMLs(List<DateTime> setDateList)
        //{
        //    string CommandStr = @" ";

        //    setDateList?.ForEach(item =>
        //    {
        //        CommandStr += $" EXECUTE  pr_ Get_XML_Files_By_Setdate @setDate = '{SqlDateTime.Parse(item.ToString())}'   ";
        //    });
        //    ActionResult result = new ActionResult() { ResultCode = ResultCode.Normal };
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
        //        {
        //            using (SqlCommand cmd = new SqlCommand(CommandStr, conn))
        //            {

        //                cmd.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.ResultCode = ResultCode.Failed;
        //        result.Errors.Add(new ActionError() { Code = 1, Description = ex.Message });
        //    }
        //    return result;
        //}


        //public static List<DateTime> GetOperDaysByPeriod(DateTime DateTo, DateTime DateFrom)
        //{
        //    List<DateTime> Operdays = new List<DateTime>();
        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
        //    {

        //        using (SqlCommand cmd = new SqlCommand())
        //        {
        //            conn.Open();
        //            cmd.Connection = conn;
        //            cmd.CommandText = "SELECT oper_day WHERE oper_day in (@DateTo,@DateFrom)";
        //            DataTable dt = new DataTable();
        //            cmd.Parameters.Add("@DateTo", SqlDbType.SmallDateTime).Value = DateTo;
        //            cmd.Parameters.Add("@DateFrom", SqlDbType.SmallDateTime).Value = DateFrom;
        //            dt.Load(cmd.ExecuteReader());
        //            if (dt.Rows.Count > 0)
        //            {
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    DateTime date = Convert.ToDateTime(dt.Rows[i]["oper_day"].ToString());
        //                    Operdays.Add(date);

        //                }
        //            }
        //        }

        //        return Operdays;

        //    }
        //}







    }





}
