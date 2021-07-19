using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace ExternalBanking.DBManager
{
    internal class PeriodicTransferDataChangeOrderDB
    {
        /// <summary>
        /// Պարբերարական փոխանցման հայտի փոփոխում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicTransferDataChangeOrder(PeriodicTransferDataChangeOrder periodicDataChangeOrder)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPeriodicDocumentChangeOrder";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = periodicDataChangeOrder.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = periodicDataChangeOrder.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = periodicDataChangeOrder.RegistrationDate;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)periodicDataChangeOrder.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = periodicDataChangeOrder.OrderNumber;
                    cmd.Parameters.Add("@doc_subtype", SqlDbType.Int).Value = periodicDataChangeOrder.SubType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = periodicDataChangeOrder.Currency;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = periodicDataChangeOrder.Description;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = periodicDataChangeOrder.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = periodicDataChangeOrder.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = periodicDataChangeOrder.OperationDate;
                    if(periodicDataChangeOrder.ChargeType == 0 )
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = periodicDataChangeOrder.Amount;
                    }
                    else
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 1;
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = 0;
                    }
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = periodicDataChangeOrder.MinAmountLevel;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = periodicDataChangeOrder.MaxAmountLevel;
                    cmd.Parameters.Add("@minimal_rest", SqlDbType.Float).Value = periodicDataChangeOrder.MinDebetAccountRest;
                    if (periodicDataChangeOrder.LastOperationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = periodicDataChangeOrder.LastOperationDate.Value;
                    }
                    cmd.Parameters.Add("@periodicity", SqlDbType.VarChar, 50).Value = periodicDataChangeOrder.Periodicity;
                    cmd.Parameters.Add("@check_days_count", SqlDbType.Int).Value = periodicDataChangeOrder.CheckDaysCount;
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.Int).Value = periodicDataChangeOrder.PayIfNoDebt;
                    cmd.Parameters.Add("@Partial_Payments", SqlDbType.Bit).Value = periodicDataChangeOrder.PartialPayment;
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.DateTime).Value = periodicDataChangeOrder.FirstTransferDate;
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    
                    result.ResultCode = ResultCode.Normal;
                    periodicDataChangeOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = periodicDataChangeOrder.Id;
                   
                    return result;
                }
            }
        }
        /// <summary>
        /// Պարբերարական փոխանցման փոփոխման հայտի տվյալներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PeriodicTransferDataChangeOrder GetPeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
           // order = new PeriodicTransferDataChangeOrder();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT 
                                                        d.document_type,
                                                        o.first_repayment_date,
                                                        d.registration_date,
                                                        d.doc_ID,  
                                                        d.customer_number,
                                                        d.document_number,
                                                        d.document_subtype,
                                                        d.currency,
                                                        o.date_of_normal_end,
                                                        d.amount,
                                                        o.min_amount,
                                                        o.max_amount,
                                                        o.minimal_rest,
                                                        o.period,
                                                        o.check_days_count,
                                                        d.description,
                                                        o.PayIfNoDebt,
                                                        o.Partial_Payments,
                                                        o.total_rest,
                                                        d.quality,
                                                        d.quality,
                                                        d.source_type,
                                                        I.App_id
                                                        FROM Tbl_HB_documents d INNER JOIN Tbl_HB_Operations_by_period o ON d.doc_ID=o.docID
                                                        INNER JOIN Tbl_HB_Products_Identity I ON d.doc_ID = I.HB_Doc_ID
                                                        WHERE d.doc_ID=@doc_ID and d.customer_number=@customer_number", conn);
                cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.Periodicity = Convert.ToInt32(dt.Rows[0]["period"]);
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                if (dt.Rows[0]["date_of_normal_end"].ToString() != "")
                {
                    order.LastOperationDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end"]);
                }
                order.MinAmountLevel = Convert.ToDouble(dt.Rows[0]["min_amount"]);
                order.MaxAmountLevel = Convert.ToDouble(dt.Rows[0]["max_amount"]);
                order.MinDebetAccountRest = Convert.ToDouble(dt.Rows[0]["minimal_rest"]);
                order.CheckDaysCount = Convert.ToUInt16(dt.Rows[0]["check_days_count"]);
                order.Description = dt.Rows[0]["description"].ToString();
                order.PayIfNoDebt = Convert.ToByte(dt.Rows[0]["PayIfNoDebt"]);
                if (dt.Rows[0]["Partial_Payments"].ToString() != "")
                {
                    order.PartialPayment = Convert.ToByte(dt.Rows[0]["Partial_Payments"]);
                }
                if (Convert.ToByte(dt.Rows[0]["total_rest"]) == 1)
                {
                    order.ChargeType = 2;
                }
                else
                {
                    order.ChargeType = 1;
                }
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                if (dt.Rows[0]["first_repayment_date"].ToString() != "")
                {
                    order.FirstTransferDate = Convert.ToDateTime(dt.Rows[0]["first_repayment_date"]);
                }
                order.ProductId = Convert.ToUInt64(dt.Rows[0]["App_id"].ToString());
                order.Type = (OrderType)Convert.ToInt32(dt.Rows[0]["document_type"]);
            }
            return order;
        }
    }
}
