using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class CardLimitChangeOrderDB
    {
        internal static ActionResult Save(CardLimitChangeOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[sp_addNewCardLimitChangeOrder]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = order.Card.CardNumber;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;

                    order.Limits.ForEach(item =>
                    {
                        switch (item.Limit)
                        {
                            case LimitType.DailyCashingAmountLimit:
                                cmd.Parameters.Add("@daily_cashing_amount", SqlDbType.Float).Value = item.LimitValue;
                                break;
                            case LimitType.DailyCashingQuantityLimit:
                                cmd.Parameters.Add("@dailiy_cashing_quantity", SqlDbType.Float).Value = item.LimitValue;
                                break;
                            case LimitType.DailyPaymentsAmountLimit:
                                cmd.Parameters.Add("@daily_payments_amount", SqlDbType.Float).Value = item.LimitValue;
                                break;
                            case LimitType.MonthlyAggregateLimit:
                                cmd.Parameters.Add("@monthly_aggregate_amount", SqlDbType.Float).Value = item.LimitValue;
                                break;
                        }
                    });

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9)
                    {
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 10)
                    {
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
                return result;
            }
        }

        internal static void SetRejectReasonFromArca(long docID, string rejectReason)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string txt = @"UPDATE tbl_card_limit_change_order_details SET reject_reason = @rejectReason WHERE doc_id = @docId";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@rejectReason", SqlDbType.NVarChar).Value = rejectReason;
                    cmd.Parameters.Add("@docId", SqlDbType.BigInt).Value = docID;

                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static CardLimitChangeOrder Get(CardLimitChangeOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT HB.registration_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.quality,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
                                                         HB.currency,
														 DT.card_number,
														 DT.reject_reason,
														 DT.daily_cashing_amount,
														 DT.dailiy_cashing_quantity,
														 DT.daily_payments_amount,
                                                         HB.confirmation_date,
                                                         DT.monthly_aggregate_amount
                                                         FROM Tbl_HB_documents AS HB join tbl_card_limit_change_order_details AS DT ON HB.doc_ID = DT.doc_id
                                                         WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardNumber = dt.Rows[0]["card_number"].ToString();
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.Limits = new List<CardLimit>();
                    string limit = null;

                    limit = dt.Rows[0]["daily_cashing_amount"].ToString();
                    if (!String.IsNullOrEmpty(limit))
                    {
                        CardLimit dailyCashingAmount = new CardLimit();
                        dailyCashingAmount.Limit = LimitType.DailyCashingAmountLimit;
                        dailyCashingAmount.LimitValue = Convert.ToDouble(limit);
                        order.Limits.Add(dailyCashingAmount);
                    }

                    limit = dt.Rows[0]["dailiy_cashing_quantity"].ToString();
                    if (!String.IsNullOrEmpty(limit))
                    {
                        CardLimit dailiyCashingQuantity = new CardLimit();
                        dailiyCashingQuantity.Limit = LimitType.DailyCashingQuantityLimit;
                        dailiyCashingQuantity.LimitValue = Convert.ToDouble(limit);
                        order.Limits.Add(dailiyCashingQuantity);
                    }

                    limit = dt.Rows[0]["daily_payments_amount"].ToString();
                    if (!String.IsNullOrEmpty(limit))
                    {
                        CardLimit dailyPaymentsAmount = new CardLimit();
                        dailyPaymentsAmount.Limit = LimitType.DailyPaymentsAmountLimit;
                        dailyPaymentsAmount.LimitValue = Convert.ToDouble(limit);
                        order.Limits.Add(dailyPaymentsAmount);
                    }
                    limit = dt.Rows[0]["monthly_aggregate_amount"].ToString();
                    if (!String.IsNullOrEmpty(limit))
                    {
                        CardLimit monthlyAggregateAmount = new CardLimit();
                        monthlyAggregateAmount.Limit = LimitType.MonthlyAggregateLimit;
                        monthlyAggregateAmount.LimitValue = Convert.ToDouble(limit);
                        order.Limits.Add(monthlyAggregateAmount);
                    }



                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());

                    order.RejectReasonDescription = dt.Rows[0]["reject_reason"].ToString();
                }
            }
            return order;
        }

        internal static Dictionary<string, string> GetCardLimits(long productId)
        {
            Dictionary<string, string> cardLimits = new Dictionary<string, string>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT typeId,  CLV.limitValue FROM Tbl_CardLimitValues CLV " +
                                 "INNER JOIN Tbl_Type_Of_CardLimits CL " +
                                 "ON CLV.limitType = CL.id " +
                                 "WHERE app_id = @product_id AND(isDeleted = 0 OR isDeleted IS NULL) and ((CLV.limitType in (8, 5, 25) AND CL.periodTypeId = 1) OR (CLV.limitType= 23 AND CL.periodTypeId = 3))";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);

                cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = productId;


                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cardLimits.Add(dt.Rows[i]["typeId"].ToString(), dt.Rows[i]["limitValue"].ToString());
            }
            return cardLimits;
        }
    }
}
