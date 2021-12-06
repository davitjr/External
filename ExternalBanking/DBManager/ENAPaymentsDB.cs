using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class ENAPaymentsDB
    {
        public static List<ENAPayments> GetENAPayments(string abonentNumber, string branch)
        {
            List<ENAPayments> list = new List<ENAPayments>();
            ENAPayments paymernt;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT [Date], Summa, [Order], Pac, P.Masn FROM (SELECT cod,masn,id FROM [Utility_payments].[dbo].[ENA_Fiz] UNION 
                    SELECT A_Nom as cod,masn,id FROM [Utility_payments].[dbo].[ENA_Jur] )  F 
                     INNER JOIN Tbl_ENA_Payments P ON F.masn = P.masn AND F.id =P.id_ena WHERE cod=@abonentNumber AND P.masn  = @branch AND P.Deleted <> 1 ORDER BY P.[Date] DESC";
                cmd.Parameters.Add("@abonentNumber", SqlDbType.Float).Value = abonentNumber;
                cmd.Parameters.Add("@branch", SqlDbType.Float).Value = branch;
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    paymernt = new ENAPayments();
                    paymernt.PaymentDate = DateTime.Parse(dr["Date"].ToString());
                    paymernt.PaidAmount = Convert.ToDouble(dr["Summa"].ToString());
                    paymernt.PaymentOrderNumber = int.Parse(dr["Order"].ToString());
                    paymernt.FilialCode = int.Parse(dr["Pac"].ToString());
                    paymernt.Branch = dr["Masn"].ToString();
                    list.Add(paymernt);
                }
            }
            return list;
        }

        public static List<DateTime> GetENAPaymentDates(string abonentNumber, string branch)
        {
            List<DateTime> list = new List<DateTime>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT [Date] FROM (SELECT cod,masn,id FROM [Utility_payments].[dbo].[ENA_Fiz]UNION 
                    SELECT A_Nom as cod,masn,id FROM [Utility_payments].[dbo].[ENA_Jur] )  F INNER JOIN Tbl_ENA_Payments P ON 
                    F.masn = P.masn AND F.id =P.id_ena WHERE cod=@abonentNumber AND P.MASN = @branch AND Deleted <> 1 GROUP BY [Date] ORDER BY [Date] DESC";
                cmd.Parameters.Add("@abonentNumber", SqlDbType.Float).Value = abonentNumber;
                cmd.Parameters.Add("@branch", SqlDbType.Float).Value = branch;
                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    DateTime paymentDate = DateTime.Parse(dr["Date"].ToString());
                    list.Add(paymentDate);
                }
            }
            return list;
        }
    }
}
