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
    public class InterestMarginDB
    {
        internal static InterestMargin GetInterestMarginDetails(InterestMarginType marginType)
        {
            InterestMargin InterestMargin = new InterestMargin();

            InterestMargin.marginType = marginType;
            InterestMargin.marginDetails = new List<InterestMarginDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"SELECT currency, interest_rate, margin_date FROM Tbl_Interest_Margins WHERE margin_type=" + (int)marginType + " AND closing_date IS NULL and doc_id = (SELECT top 1 doc_id FROM Tbl_Interest_Margins WHERE margin_type=" + (int)marginType + " ORDER BY margin_date DESC, registration_date DESC)";

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        InterestMarginDetail rate = SetInterestMarginDetail(row);

                        InterestMargin.marginDetails.Add(rate);
                    }

                    InterestMargin.marginDate = Convert.ToDateTime(dt.Rows[0]["margin_date"].ToString());
                }
            }
            return InterestMargin;
        }

        internal static InterestMargin GetInterestMarginDetailsByDate(InterestMarginType marginType, DateTime marginDate)
        {
            InterestMargin InterestMargin = new InterestMargin();

            InterestMargin.marginType = marginType;
            InterestMargin.marginDetails = new List<InterestMarginDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"SELECT currency, interest_rate, margin_date FROM Tbl_Interest_Margins WHERE margin_type=" + (int)marginType +
                        " AND FORMAT(margin_date,'yyyy-MM')=FORMAT(@marginDate,'yyyy-MM') and closing_date IS NULL";
                    cmd.Parameters.Add("@marginDate", SqlDbType.SmallDateTime).Value = marginDate;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }


                    if (dt.Rows.Count == 0)
                    {
                        DataTable currencies = Info.GetCurrencies();
                        foreach (DataRow currency in currencies.Rows)
                        {
                            InterestMarginDetail rate = new InterestMarginDetail();
                            rate.Currency = currency["currency"].ToString();
                            rate.InterestRate = 0;
                            InterestMargin.marginDetails.Add(rate);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];

                            InterestMarginDetail rate = SetInterestMarginDetail(row);

                            InterestMargin.marginDetails.Add(rate);
                        }
                    }
                    InterestMargin.marginDate = marginDate;
                }
            }
            return InterestMargin;
        }

        private static InterestMarginDetail SetInterestMarginDetail(DataRow row)
        {
            InterestMarginDetail rate = new InterestMarginDetail();

            if (row != null)
            {
                rate.Currency = row["currency"].ToString();
                rate.InterestRate = float.Parse(row["interest_rate"].ToString());
            }

            return rate;
        }

        internal static ActionResult SaveInterestMarginOrder(InterestMarginOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_Interest_Margin_document";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@rateType", SqlDbType.Int).Value = order.InterestMargin.marginType;
                    //cmd.Parameters.Add("@fondDescription", SqlDbType.NVarChar, 250).Value = order.Fond.Description;
                    //cmd.Parameters.Add("@isSubsidia", SqlDbType.Bit).Value = order.Fond.IsSubsidia;
                    //cmd.Parameters.Add("@isActive", SqlDbType.Bit).Value = order.Fond.IsActive;

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

        internal static void SaveInterestMarginDetails(InterestMarginOrder order)
        {
            order.InterestMargin.marginDetails?.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_Interest_Margin_Order_Details
                                                            (Doc_ID, margin_type, currency, interest_rate, margin_date)
                                                            VALUES 
                                                            (@DocID, @marginType, @currency, @interestRate, @marginDate)", conn))
                    {
                        cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = order.Id;
                        cmd.Parameters.Add("@marginType", SqlDbType.Int).Value = order.InterestMargin.marginType;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = m.Currency;
                        cmd.Parameters.Add("@interestRate", SqlDbType.Float).Value = Math.Round(m.InterestRate / 100, 4);
                        cmd.Parameters.Add("@marginDate", SqlDbType.DateTime).Value = order.InterestMargin.marginDate;
                        //cmd.Parameters.Add("@providingTerminationDate", SqlDbType.SmallDateTime).Value = m.TerminationDate != null ? (object)m.TerminationDate : DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }
    }
}
