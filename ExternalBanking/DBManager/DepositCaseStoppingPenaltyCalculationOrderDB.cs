using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ExternalBanking.DBManager
{
    internal static class DepositCaseStoppingPenaltyCalculationOrderDB
    {

        internal static ActionResult SaveDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"
                                                    DECLARE @filial AS int
                                                    SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customer_number   
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,quality, 
                                                    source_type,operationFilialCode,operation_date)
                                                    VALUES
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@doc_sub_type,
                                                    1,@source_type,@operation_filial_code,@oper_day)
                                                    SELECT Scope_identity() as ID
                                                     ", conn);
                cmd.CommandType = CommandType.Text;


                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());
                SaveDepositCaseStoppingPenaltyCalculationOrderDetails(order);
                result.ResultCode = ResultCode.Normal;
                return result;
            }

        }

        internal static void SaveDepositCaseStoppingPenaltyCalculationOrderDetails(DepositCaseStoppingPenaltyCalculationOrder order)
        {

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@" INSERT INTO tbl_deposit_case_penalty_stopping_order_details
                                                    (
                                                    Doc_Id,
                                                    app_id,
                                                    date_of_stopping_penalty_calculation,
                                                    document_date,
                                                    changing_reason
                                                    )
                                                    VALUES
                                                    (
                                                    @DocId,
                                                    @appId,
                                                    @dateOfStoppingPenaltyCalculation,
                                                    @documentDate,
                                                    @changingReason
                                                    )", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@DocId", SqlDbType.Int).Value = order.Id;
            cmd.Parameters.Add("@appId", SqlDbType.Float).Value = order.ProductId;
            cmd.Parameters.Add("@dateOfStoppingPenaltyCalculation", SqlDbType.SmallDateTime).Value = order.DateOfStoppingPenaltyCalculation.Date;
            cmd.Parameters.Add("@documentDate", SqlDbType.SmallDateTime).Value = order.DocumentDate.Date;
            cmd.Parameters.Add("@changingReason", SqlDbType.NVarChar, 250).Value = order.ChangingReason;
            cmd.ExecuteScalar();


        }

        internal static bool IsSecondPenaltyStoppingOrder(ulong customerNumber, ulong productId)
        {
            bool secondClosing;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"  SELECT d.doc_ID FROM Tbl_HB_documents d
                                                    INNER JOIN tbl_deposit_case_penalty_stopping_order_details c
                                                    ON d.doc_ID=c.Doc_id
                                                    WHERE d.customer_number=@customerNumber AND c.app_id=@productId
                                                    AND d.quality IN (1,2,3,5,100)", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    secondClosing = true;
                }
                else
                    secondClosing = false;
            }
            return secondClosing;
        }


        internal static DepositCaseStoppingPenaltyCalculationOrder GetDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                        c.*,
                                                        d.filial,
                                                        d.customer_number,
                                                        d.registration_date,
                                                        d.document_type,
                                                        d.document_number,
                                                        d.document_subtype,
                                                        d.quality, 
                                                        d.source_type,
                                                        d.operationFilialCode,
                                                        d.operation_date
                                                        FROM Tbl_HB_documents d
                                                        INNER JOIN tbl_deposit_case_penalty_stopping_order_details c
                                                        ON d.doc_ID=c.Doc_id
                                                        WHERE d.Doc_ID=@DocID and d.customer_number=CASE WHEN @customer_number = 0 THEN d.customer_number ELSE @customer_number END", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.Source=(SourceType)Convert.ToInt16( dt.Rows[0]["source_type"]);
                order.FilialCode= dt.Rows[0]["operationFilialCode"] != DBNull.Value ? Convert.ToUInt16(dt.Rows[0]["operationFilialCode"]) : (ushort)0;
                order.ProductId = Convert.ToUInt64(dt.Rows[0]["app_id"]);
                order.DateOfStoppingPenaltyCalculation = Convert.ToDateTime(dt.Rows[0]["date_of_stopping_penalty_calculation"]);
                order.DocumentDate= Convert.ToDateTime(dt.Rows[0]["document_date"]);
                order.ChangingReason = dt.Rows[0]["changing_reason"].ToString();

            }
            return order;
        }


    }
}
