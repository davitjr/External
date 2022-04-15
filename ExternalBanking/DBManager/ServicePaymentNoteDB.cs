using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class ServicePaymentNoteDB
    {
        internal static List<ServicePaymentNote> GetServicePaymentNoteList(ulong customerNumber)
        {
            List<ServicePaymentNote> servicePaymentNotes = new List<ServicePaymentNote>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT Tbl_service_payments_notes.ID, Tbl_service_payments_notes.set_number, Tbl_service_payments_notes.date_of_note,Tbl_service_payments_notes.note_action," +
                               "note_action_type_description = case when note_action = 0 then N'Չգանձել' else N'Գանձել' end ," +
                               "Tbl_type_of_service_notes.Note_description, Tbl_service_payments_notes.description " +
                               "FROM Tbl_service_payments_notes LEFT JOIN Tbl_type_of_service_notes ON Tbl_service_payments_notes.note_reason = Tbl_type_of_service_notes.Id " +
                               "WHERE Tbl_service_payments_notes.customer_number =@customerNumber";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                conn.Open();
                using DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    ServicePaymentNote note = SetNote(row);
                    servicePaymentNotes.Add(note);
                }
            }
            return servicePaymentNotes;
        }

        internal static ServicePaymentNoteOrder GetServicePaymentNoteOrder(ServicePaymentNoteOrder order)
        {
            using DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                         d.customer_number,
                                                         d.registration_date,
                                                         d.document_number,
                                                         d.document_type,
                                                         d.quality,
                                                         d.document_subtype ,
                                                         d.registration_date,
                                                         d.operation_date,
                                                         d.source_type,
                                                         

                                                         n.note_action,
                                                         n.note_reason,
                                                         n.note_reason_description,
                                                         n.description
                                                                                                                   
                                                         FROM Tbl_HB_documents AS d
                                                         INNER JOIN Tbl_Service_Payment_Note_Order_Details AS n
                                                         on d.doc_ID=n.doc_Id
                                                         WHERE d.customer_number=case when @customer_number = 0 THEN d.customer_number else @customer_number end AND d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {

                    order.Note = new ServicePaymentNote();
                    order.Note.NoteActionTypeDescription = Convert.ToUInt16(dt.Rows[0]["note_action"].ToString()) == 0 ? "Չգանձել" : "Գանձել";
                    order.Note.AdditionalDescription = dt.Rows[0]["description"].ToString();
                    order.Note.NoteReasonDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["note_reason_description"].ToString());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }


            }
            return order;

        }

        internal static ServicePaymentNoteOrder GetDelatedServicePaymentNoteOrder(ServicePaymentNoteOrder order)
        {
            using DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                         d.customer_number,
                                                         d.registration_date,
                                                         d.document_number,
                                                         d.document_type,
                                                         d.quality,
                                                         d.document_subtype ,
                                                         d.registration_date,
                                                         d.operation_date,
                                                         d.source_type,
                                                         
                                                         n.note_id
                                                                                                                   
                                                         FROM Tbl_HB_documents AS d
                                                         INNER JOIN Tbl_Service_Payment_Note_Order_Details AS n
                                                         on d.doc_ID=n.doc_Id
                                                         WHERE d.customer_number=case when @customer_number = 0 THEN d.customer_number else @customer_number end AND d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {

                    order.Note = new ServicePaymentNote();

                    order.Note.Id = Convert.ToInt32(dt.Rows[0]["document_number"].ToString());
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }


            }
            return order;

        }

        internal static ActionResult Save(ServicePaymentNoteOrder servicePaymentNoteOrder, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Service_Payment_Note_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = servicePaymentNoteOrder.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = servicePaymentNoteOrder.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = servicePaymentNoteOrder.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)servicePaymentNoteOrder.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = servicePaymentNoteOrder.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = servicePaymentNoteOrder.FilialCode;
                    cmd.Parameters.Add("@note_action", SqlDbType.SmallInt).Value = (Int16)servicePaymentNoteOrder.Note.NoteActionType;
                    cmd.Parameters.Add("@note_reason", SqlDbType.SmallInt).Value = (Int16)servicePaymentNoteOrder.Note.NoteReason;
                    cmd.Parameters.Add("@note_reason_description", SqlDbType.NVarChar, 255).Value = servicePaymentNoteOrder.Note.NoteReasonDescription;
                    cmd.Parameters.Add("@note_description", SqlDbType.NVarChar, 255).Value = servicePaymentNoteOrder.Note.AdditionalDescription != null ? servicePaymentNoteOrder.Note.AdditionalDescription : "";

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    servicePaymentNoteOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    servicePaymentNoteOrder.Quality = OrderQuality.Draft;
                }
                return result;
            }

        }

        internal static ActionResult Delete(ServicePaymentNoteOrder servicePaymentNoteOrder, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Delete_Service_Payment_Note_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = servicePaymentNoteOrder.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = servicePaymentNoteOrder.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = servicePaymentNoteOrder.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)servicePaymentNoteOrder.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = servicePaymentNoteOrder.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = servicePaymentNoteOrder.FilialCode;



                    cmd.Parameters.Add("@note_id", SqlDbType.Int).Value = servicePaymentNoteOrder.Note.Id;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    servicePaymentNoteOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    servicePaymentNoteOrder.Quality = OrderQuality.Draft;
                }
                return result;
            }
        }

        private static ServicePaymentNote SetNote(DataRow row)
        {
            ServicePaymentNote note = new ServicePaymentNote();
            if (row != null)
            {
                note.Id = int.Parse(row["ID"].ToString());
                note.UserId = int.Parse(row["set_number"].ToString());
                note.NoteDate = DateTime.Parse(row["date_of_note"].ToString());
                note.NoteActionType = (ServicePaymenteNoteType)Convert.ToInt16(row["note_action"].ToString());

                note.AdditionalDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                note.NoteActionTypeDescription = Utility.ConvertAnsiToUnicode(row["note_action_type_description"].ToString());
                note.NoteReasonDescription = Utility.ConvertAnsiToUnicode(row["Note_description"].ToString());
            }
            return note;
        }

        internal static bool IsExistCurrentDayNote(ServicePaymentNoteOrder order)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT ID FROM Tbl_service_payments_notes WHERE customer_number=@customerNumber AND CAST(date_of_note AS DATE)=CAST(@dateOfNote AS DATE)", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@dateOfNote", SqlDbType.DateTime).Value = order.OperationDate;

                result = Convert.ToInt32(cmd.ExecuteScalar());

            }
            if (result != 0)
                return true;
            else
                return false;
        }

        internal static bool IsRemovableNote(ServicePaymentNoteOrder order)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT ID FROM Tbl_service_payments_notes  WHERE ID=@noteID AND CAST(date_of_note as date)=CAST(@operDay as date)", conn);
                cmd.Parameters.Add("@noteID", SqlDbType.Int).Value = order.Note.Id;
                cmd.Parameters.Add("@operDay", SqlDbType.DateTime).Value = order.OperationDate;
                result = Convert.ToInt32(cmd.ExecuteScalar());

            }
            if (result != 0)
                return true;
            else
                return false;
        }

    }
}
