using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class LoanInterestRateConcessionOrderDB
    {
        internal static ActionResult SaveLoanConcessionOrder(LoanInterestRateConcessionOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_loan_interest_rate_concession_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductAppId;

                    cmd.Parameters.Add("@concession_date", SqlDbType.SmallDateTime).Value = order.ConcessionDate;
                    cmd.Parameters.Add("@number_of_month", SqlDbType.NVarChar, 500).Value = order.NumberOfMonths;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    return result;
                }
            }

        }

        internal static LoanInterestRateConcessionOrder GetLoanInterestRateConcessionDetailsDB(ulong productId)
        {
            var concessionDetails = new LoanInterestRateConcessionOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "Select A.product_app_Id, A.concession_date, A.number_of_months, B.quality " +
                                                    "From[Tbl_loan_interest_rate_concession_order_details] AS A " +
                                                    "Inner Join Tbl_HB_documents AS B " +
                                                    "ON A.Doc_ID = B.doc_ID " +
                                                    "WHERE A.product_app_Id = @productId";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    concessionDetails.ProductAppId = ulong.Parse(dr["product_app_Id"].ToString());
                    concessionDetails.ConcessionDate = DateTime.Parse(dr["concession_date"].ToString());
                    concessionDetails.NumberOfMonths = int.Parse(dr["number_of_months"].ToString());
                    concessionDetails.Quality = (OrderQuality)dr["quality"];
                }

            }

            return concessionDetails;
        }

        internal static LoanInterestRateConcessionOrder GetLoanDetailsForValidation(ulong productId)
        {
            var concessionDetails = new LoanInterestRateConcessionOrder();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand("SELECT filialcode,Current_rate_value, current_fee FROM [V_shortLoans] where app_id = " + productId, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    concessionDetails.LoanFilialCode = Convert.ToInt32(dr["filialcode"]);
                    concessionDetails.CurrentRateValue = Convert.ToDouble(dr["Current_rate_value"]);
                    concessionDetails.CurrentFee = Convert.ToDouble(dr["current_fee"]);
                }
            }
            return concessionDetails;
        }

        internal static LoanInterestRateConcessionOrder GetLoanInterestRateConcessionOrder(LoanInterestRateConcessionOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            hb.filial,
                                            hb.customer_number,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number as hb_document_number,
                                            hb.document_subtype,
                                            hb.quality,
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            ld.*
                                            from Tbl_HB_documents hb inner join Tbl_loan_interest_rate_concession_order_details ld
                                            on ld.doc_id=hb.doc_ID
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["hb_document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.ProductAppId = Convert.ToUInt64(dr["product_app_Id"].ToString());
                    order.ConcessionDate = dr["concession_date"] != DBNull.Value ? Convert.ToDateTime(dr["concession_date"]) : default(DateTime);
                    order.NumberOfMonths = dr["number_of_months"] != DBNull.Value ? Convert.ToInt32(dr["number_of_months"]) : 0;
                }
            }
            return order;
        }
    }
}
