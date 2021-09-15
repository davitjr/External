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
    class ArcaCardsTransactionOrderDB
    {
        internal static ActionResult Save(ArcaCardsTransactionOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[sp_addNewArcaCardsTransactionOrder]";
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
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = order.Card.CardNumber;
                    cmd.Parameters.Add("@action_type", SqlDbType.Int).Value = order.ActionType;
                    cmd.Parameters.Add("@action_reason", SqlDbType.Int).Value = order.ActionReasonId;
                    cmd.Parameters.Add("@hot_card_status", SqlDbType.Int).Value = order.HotCardStatus;
                    if (String.IsNullOrEmpty(order.Comment))
                        cmd.Parameters.Add("@comment", SqlDbType.NVarChar).Value = DBNull.Value;
                    else
                        cmd.Parameters.Add("@comment", SqlDbType.NVarChar).Value = order.Comment;


                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.Parameters.Add("@validation_date", SqlDbType.SmallDateTime).Value = order.Card.ValidationDate;

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

        internal static ArcaCardsTransactionOrder Get(ArcaCardsTransactionOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT HB.registration_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.source_type,
                                                         HB.quality,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
														 DT.card_number,
                                                         DT.hot_card_status,
														 DT.action_type,
														 DT.action_reason,
                                                         DT.comment,
                                                         DT.reject_reason,
                                                         HB.order_group_id,
                                                         HB.confirmation_date
                                                         FROM Tbl_HB_documents AS HB join tbl_arca_cards_transaction_order_details AS DT ON HB.doc_ID = DT.doc_id
                                                         WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"].ToString());
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardNumber = dt.Rows[0]["card_number"].ToString();
                    if (dt.Rows[0]["hot_card_status"] != DBNull.Value)
                    {
                        order.HotCardStatus = Convert.ToInt32(dt.Rows[0]["hot_card_status"]);
                    }
                    else
                    {
                        order.HotCardStatus = null;
                    }

                    order.ActionType = Convert.ToInt16(dt.Rows[0]["action_type"].ToString());
                    order.ActionReasonId = Convert.ToByte(dt.Rows[0]["action_reason"].ToString());
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.RejectReasonDescription = dt.Rows[0]["reject_reason"].ToString();
                    order.Comment = dt.Rows[0]["comment"].ToString();
                    order.GroupId = int.Parse(dt.Rows[0]["order_group_id"].ToString());
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                }
            }
            return order;
        }

        internal static void SetHotCardStatus(ArcaCardsTransactionOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string txt = @"SELECT " + ((order.ActionType == 1) ? "blocking_hot_card_status" : "unblocking_hot_card_status") + " FROM tbl_type_of_reasons_for_card_transaction_action WHERE id=@id";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.ActionReasonId;

                    conn.Open();

                    var res = cmd.ExecuteScalar();

                    order.HotCardStatus = (res != DBNull.Value) ? Convert.ToInt32(res) : default(int?);
                }
            }
        }

        internal static bool CheckTransactionAvailabilityDependsOnActionReason(ushort filialCode, string cardNumber)
        {
            DataTable dt = new DataTable();
            ushort operationFilialCode = 0;
            byte documentSubType = 0;
            byte actionReason = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string txt = @"SELECT top 1 HB.operationFilialCode, HB.document_subtype, DT.action_reason
                                FROM tbl_hb_documents  AS HB 
                                INNER JOIN tbl_arca_cards_transaction_order_details as DT
                                ON HB.doc_ID = DT.doc_id
                                WHERE HB.quality = 30 and HB.document_type = 206  and DT.card_number = @card_number  ORDER BY HB.doc_ID DESC";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        operationFilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"]);
                        documentSubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                        actionReason = Convert.ToByte(dt.Rows[0]["action_reason"]);
                    }
                }
            }

            return ((documentSubType == 1 && (actionReason == 5 || actionReason == 6) && operationFilialCode == filialCode));
        }

        internal static void SetRejectReason(long docID, string rejectReason)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string txt = @"UPDATE tbl_arca_cards_transaction_order_details SET reject_reason = @rejectReason WHERE doc_id = @docId";

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

        internal static short GetBlockingReasonForBlockedCard(string cardNumber)
        {
            DataTable dt = new DataTable();
            short subType = 0;
            short reasonid = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string txt = @"SELECT document_subtype,action_reason FROM
                                    (SELECT top 1  HB.document_subtype, DT.action_reason,  DT.validation_date ,VN.validation_date AS vn_validation_date
                                    FROM tbl_hb_documents  AS HB 
                                    INNER JOIN tbl_arca_cards_transaction_order_details as DT
                                    ON HB.doc_ID = DT.doc_id
                                    OUTER APPLY (
			                                    SELECT TOP 1 change_set_number, change_date FROM Tbl_HB_quality_history
				                                    WHERE doc_id = HB.doc_ID AND quality=50 AND change_set_number=88
				                                    ORDER BY change_date DESC	
			                                    ) HBQH

                                    OUTER APPLY (SELECT validation_date, visa_number FROM Tbl_Visa_Numbers_Accounts WHERE visa_number=DT.card_number AND closing_date IS NULL) VN
                                    WHERE HB.quality = 30 and HB.document_type = 206  and DT.card_number = @card_number and HBQH.change_set_number IS NULL
                                    ORDER BY HB.doc_ID DESC) R
                               WHERE R.validation_date IS NULL OR R.validation_date=R.vn_validation_date";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        subType = Convert.ToInt16(dt.Rows[0]["document_subtype"]);
                        reasonid = Convert.ToInt16(dt.Rows[0]["action_reason"]);
                    }
                }
            }

            if (subType == 1)
                return reasonid;

            return 0;
        }

        internal static string GetPreviousBlockUnblockOrderComment(string cardNumber)
        {
            DataTable dt = new DataTable();
            string orderComment = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string txt = @"SELECT comment FROM 
                                    (SELECT TOP 1 DT.comment, DT.validation_date, VN.validation_date AS vn_validation_date
                                    FROM tbl_hb_documents AS HB 
                                    INNER JOIN tbl_arca_cards_transaction_order_details as DT
                                    ON HB.doc_ID = DT.doc_id
                                    OUTER APPLY (SELECT validation_date, visa_number FROM Tbl_Visa_Numbers_Accounts WHERE visa_number=DT.card_number AND closing_date IS NULL) VN
                                    WHERE HB.quality = 30 and HB.document_type = 206  and DT.card_number = @card_number
                                    ORDER BY HB.doc_ID DESC) Comment
                               WHERE validation_date IS NULL OR validation_date=vn_validation_date";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        orderComment = dt.Rows[0]["comment"].ToString();
                    }
                }
            }

            return orderComment;
        }

        internal static long? GetPreviousBlockingOrderId(string cardNumber, DateTime? validationDate)
        {
            DataTable dt = new DataTable();
            long? docId = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string txt = @"SELECT TOP 1 IIF(HB.document_subtype = 1,DT.doc_id, NULL) as doc_id
                                FROM tbl_hb_documents AS HB 
                                INNER JOIN tbl_arca_cards_transaction_order_details as DT
                                ON HB.doc_ID = DT.doc_id
                                WHERE HB.quality in (30, 50) 
                                        AND DT.action_reason in (15,16,17,18,19,20,21,22,23)
		                                AND HB.document_type = 206
		                                AND DT.card_number = @card_number" + (validationDate != null ? " AND DT.validation_date = '" + Convert.ToDateTime(validationDate).ToString("dd/MMM/yyyy") + "'" : "") + " ORDER BY HB.doc_ID DESC";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        docId = dt.Rows[0]["doc_id"]!=DBNull.Value ?Convert.ToInt64(dt.Rows[0]["doc_id"]) : (long?)null;
                    }
                }
            }

            return docId;
        }

        internal static long? GetPreviousUnBlockingOrderId(string cardNumber, DateTime? validationDate)
        {
            DataTable dt = new DataTable();
            long? docId = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string txt = @"SELECT TOP 1 IIF(HB.document_subtype = 2,DT.doc_id, NULL) as doc_id
                                FROM tbl_hb_documents AS HB 
                                INNER JOIN tbl_arca_cards_transaction_order_details as DT
                                ON HB.doc_ID = DT.doc_id
                                WHERE HB.quality in (30, 50) 
                                        AND DT.action_reason in (15,16,17,18,19,20,21,22,23)
		                                AND HB.document_type = 206
		                                AND DT.card_number = @card_number" + (validationDate != null ? " AND DT.validation_date = '" + Convert.ToDateTime(validationDate).ToString("dd/MMM/yyyy") + "'" : "") + " ORDER BY HB.doc_ID DESC";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        docId = dt.Rows[0]["doc_id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["doc_id"]) : (long?)null;
                    }
                }
            }

            return docId;
        }
    }
}
