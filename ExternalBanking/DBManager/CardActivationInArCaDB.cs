using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ExternalBanking.DBManager
{
    static class CardActivationInArCaDB
    {

        internal static List<CardActivationInArCa> GetCardActivationInArCaPaymentDetails(string cardNumber, DateTime startDate,DateTime endDate)
        {

            List<CardActivationInArCa> list = new List<CardActivationInArCa>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 
                                    ar.Id,
                                    ar.Date_of_accounting,
                                    ar.Card_Number,
                                    ar.Currency,
                                    ar.Amount,
                                    ar.Debit_Credit,
                                    ar.Card_payment_file,
                                    fl.name,fl.inputDate
                                    FROM Tbl_CardPaymentsToArca ar
                                    LEFT JOIN Tbl_ArcaFiles fl
                                    ON ar.fileID=fl.id
                                    WHERE
                                    Card_Number=@cardNumber AND ar.Card_payment_file<>'ONLINE'
                                    AND ar.Date_of_accounting>=@startDate AND ar.Date_of_accounting<=@endDate";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate.Date;
                    cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = endDate.Date;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            CardActivationInArCa detail = SetCardActivationInArCaPaymentDetails(dt.Rows[i]);
                            list.Add(detail);
                        }
                    }

                }
            }

            return list;
        }

        private static CardActivationInArCa SetCardActivationInArCaPaymentDetails(DataRow row)
        {
            CardActivationInArCa detail = new CardActivationInArCa();
            if (row != null)
            {
                detail.ActivationType = "Payment ";
                detail.Amount = Convert.ToDecimal(row["Amount"]);
                detail.CardNumber = row["Card_Number"].ToString();
                detail.Currency = row["Currency"].ToString();
                detail.DebitCredit = row["Debit_Credit"].ToString();
                detail.Id = Convert.ToUInt64(row["Id"]);
                if (row["inputDate"] != DBNull.Value)
                {
                    detail.CardActivationInArCaPaymentDetails = new CardActivationInArCaPaymentDetails();
                    detail.CardActivationInArCaPaymentDetails.FileName = row["name"].ToString();
                    detail.CardActivationInArCaPaymentDetails.InputDate = Convert.ToDateTime(row["inputDate"]);
                }
                else
                {
                    detail.StatusDescription = "ԱրՔա ուղարկման ենթակա";
                }
                detail.SendDate= Convert.ToDateTime(row["Date_of_accounting"]);
            }

            return detail;

        }


        internal static DateTime? GetLastSendedPaymentFileDate()
        {
            DateTime? sendDate = null;



            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"     SELECT inputDate FROM (
									SELECT max(fileID) as m_file_id FROM Tbl_CardPaymentsToArca
									WHERE
                                    Card_payment_file<>'ONLINE' ) a
									INNER JOIN Tbl_ArcaFiles fl
                                    ON a.m_file_id=fl.id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    sendDate = Convert.ToDateTime(cmd.ExecuteScalar().ToString());

                }
            }


            return sendDate;
        }


        internal static List<CardActivationInArCa> GetCardActivationInArCaApigateDetails(string cardNumber, DateTime startDate, DateTime endDate)
        {

            List<CardActivationInArCa> list = new List<CardActivationInArCa>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 
                                     ar.Id,
                                     ar.Date_of_accounting,
                                     ar.Card_Number,
                                     ar.Currency,
                                     ar.Amount,
                                     ar.Debit_Credit,
                                     ar.Card_payment_file, 
                                     onl.status,
                                     dt.responseCode,
                                     dt.change_date
                                     FROM Tbl_CardPaymentsToArca ar
                                     LEFT JOIN Tbl_CardPaymentsToArca_Online onl
                                     ON ar.Id=onl.payment_id
                                     LEFT JOIN Tbl_CardPaymentsToArca_details dt
                                     ON onl.detail_id=dt.id AND dt.status=onl.status
                                     WHERE 
                                     ar.Card_payment_file='ONLINE' AND Card_Number=@cardNumber
                                     AND ar.Date_of_accounting>=@startDate AND ar.Date_of_accounting<=@endDate";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate.Date;
                    cmd.Parameters.Add("@endDate", SqlDbType.SmallDateTime).Value = endDate.Date;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            CardActivationInArCa detail = SetCardActivationInArCaApigateDetails(dt.Rows[i]);
                            list.Add(detail);
                        }
                    }

                }
            }

            return list;
        }

        private static CardActivationInArCa SetCardActivationInArCaApigateDetails(DataRow row)
        {
            CardActivationInArCa detail = new CardActivationInArCa();
            if (row != null)
            {
                detail.ActivationType = "APIGate";
                detail.Amount = Convert.ToDecimal(row["Amount"]);
                detail.CardNumber = row["Card_Number"].ToString();
                detail.Currency = row["Currency"].ToString();
                detail.DebitCredit = row["Debit_Credit"].ToString();
                detail.Id = Convert.ToUInt64(row["Id"]);
                detail.SendDate = Convert.ToDateTime(row["Date_of_accounting"]);

                if (row["change_date"] != DBNull.Value)
                {
                    detail.CardActivationInArCaApigateDetails = new CardActivationInArCaApigateDetails();
                    detail.CardActivationInArCaApigateDetails.ChangeDate= Convert.ToDateTime(row["change_date"]);
                    detail.CardActivationInArCaApigateDetails.ResponseCode = row["responseCode"].ToString();
                    detail.Status = Convert.ToUInt16(row["status"]);
                    detail.StatusDescription = Info.GetTypeOfCardPaymentsToArcaDescription(detail.Status);
                }
                
            }

            return detail;

        }


        internal static List<CardActivationInArCaApigateDetails> GetCardActivationInArCaApigateDetail(ulong Id)
        {

            List<CardActivationInArCaApigateDetails> list = new List<CardActivationInArCaApigateDetails>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dt.change_date,dt.status,dt.responseCode FROM Tbl_CardPaymentsToArca_details dt
									 LEFT JOIN Tbl_CardPaymentsToArca ar
									 ON ar.Id=dt.payment_id
									 WHERE ar.Id=@id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.VarChar, 16).Value = Id;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            CardActivationInArCaApigateDetails detail = new CardActivationInArCaApigateDetails();
                            detail.ChangeDate = Convert.ToDateTime(dt.Rows[i]["change_date"]);
                            detail.Status = Convert.ToUInt16(dt.Rows[i]["status"]);
                            if (dt.Rows[i]["responseCode"] != DBNull.Value)
                                detail.ResponseCode = dt.Rows[i]["responseCode"].ToString();
                            detail.StatusDescription= Info.GetTypeOfCardPaymentsToArcaDescription(detail.Status);
                            list.Add(detail);
                        }
                    }

                }
            }

            return list;
        }



    }
}
