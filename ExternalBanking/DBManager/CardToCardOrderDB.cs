using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class CardToCardOrderDB
    {
        internal static ActionResult Save(CardToCardOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[sp_addNewCardToCardOrder]";
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
                    //cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = 1;
                    cmd.Parameters.Add("@is_our_card", SqlDbType.Bit).Value = order.IsOurCard;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.DebitCard.Currency;
                    cmd.Parameters.Add("@debet_card_account", SqlDbType.NVarChar, 20).Value = order.DebitCard.CardAccount.AccountNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@debet_card_number", SqlDbType.NVarChar, 20).Value = order.DebitCard.CardNumber;
                    cmd.Parameters.Add("@credit_card_number", SqlDbType.NVarChar, 20).Value = order.CreditCardNumber;
                    cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.FeeAmount;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

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

        internal static CardToCardOrder Get(CardToCardOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT QH.change_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.quality,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
                                                         HB.amount,
                                                         HB.amount_for_payment,
                                                         HB.currency,
                                                         CT.transfer_ID,
                                                         DT.debit_card_number,
                                                         DT.credit_card_number,
                                                         DT.embossing_name,
                                                         CDT.rrn,
                                                         CDT.authorization_ID_response,
														 HB.order_group_id,
                                                         HB.confirmation_date
                                    FROM Tbl_HB_documents AS HB 
		                            LEFT OUTER JOIN [tbl_card_to_card_transfers] AS CT ON HB.doc_ID = CT.order_id
                                    LEFT JOIN [tbl_card_to_card_transfers_details]  AS CDT ON CT.transfer_id = CDT.transfer_id AND CDT.status = 10
		                            JOIN tbl_cardtocard_order_details AS DT ON HB.doc_ID = DT.doc_id
                                    JOIN tbl_hb_quality_history AS QH on HB.doc_ID = QH.Doc_ID and QH.quality = 1
                                    WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id and ISNULL(CT.source_id, 0) =0", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["change_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = Convert.ToDateTime(dt.Rows[0]["operation_date"]);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"].ToString());
                    order.FeeAmount = Convert.ToDouble(dt.Rows[0]["amount_for_payment"].ToString());
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.DebitCardNumber = dt.Rows[0]["debit_card_number"].ToString();
                    order.CreditCardNumber = dt.Rows[0]["credit_card_number"].ToString();
                    order.EmbossingName = dt.Rows[0]["embossing_name"].ToString();
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

                    try
                    {
                        order.RRN = dt.Rows[0]["rrn"].ToString();
                        order.AuthId = dt.Rows[0]["authorization_ID_response"].ToString();
                        order.OrderId = Convert.ToInt64(dt.Rows[0]["transfer_ID"].ToString());
                    }
                    catch
                    {
                        order.RRN = null;
                        order.AuthId = null;
                        order.OrderId = 0;
                    }
                }
            }
            return order;
        }

        internal static AttachedCardTransactionReceipt GetAttachedCardTransactionDetails(AttachedCardTransactionReceipt details)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT OBCO.registration_date,
				 OBCO.transactionName, 
				 OBC.card_number,
				 OBC.carholder_name,
				 CASE WHEN OBCO.order_type = 4 THEN OBCO.credit_card_number WHEN HB.document_type = 1 THEN  IIF(ISNULL(HB.credit_bank_code,0) = 0,'',Convert(nvarchar, HB.credit_bank_code)) + Hb.credit_account    ELSE Hb.credit_account END as credit_account,	
				 CASE WHEN (HB.document_type = 1 or HB.document_type = 2) THEN OBC.carholder_name ELSE HB.receiver_name END as receiver_name,
				 HB.amount,
			     HB.currency,
                 OBCO.fee_amount,
				 HB.description,  		
			     OBCO.merchantId,                  				 			  
			     HB.document_type,
			     OBCO.order_type 
                 FROM Tbl_HB_documents AS HB 
		         JOIN [tbl_other_bank_card_orders] AS OBCO ON HB.doc_ID = OBCO.doc_id
				 JOIN (SELECT id, card_number, carholder_name FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION SELECT id, card_number, carholder_name FROM [tbl_other_bank_cards_deleted]) OBC ON OBC.id = OBCO.card_id
                 WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = details.Doc_Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = details.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    details.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    details.Type = (OrderType)dt.Rows[0]["document_type"];
                    details.Amount = Convert.ToDouble((dt.Rows[0]["amount"] as double?) ?? 0);
                    details.FeeAmount = Convert.ToDouble((dt.Rows[0]["fee_amount"] as double?) ?? 0);
                    details.Currency = dt.Rows[0]["currency"].ToString();
                    details.DebitCardNumber = dt.Rows[0]["card_number"].ToString();
                    details.CreditCardNumber = dt.Rows[0]["credit_account"].ToString();
                    details.SenderName = dt.Rows[0]["carholder_name"].ToString();
                    details.RecieverName = dt.Rows[0]["receiver_name"].ToString();
                    details.TransactionName = dt.Rows[0]["transactionName"].ToString();
                    details.TransactionPurpose = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                    details.TerminalId = dt.Rows[0]["merchantId"].ToString();
                    details.TransactionType = (int)dt.Rows[0]["order_type"];
                }
            }
            return details;
        }


        internal static ActionResult SaveAttachedCardtoCardOrder(CardToCardOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[sp_saveAttachedCardToCardOrder]";
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
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.FeeAmount;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);



                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Sent3;
                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;

                }
                return result;
            }
        }
        public static void SaveAttachedCardToCardArcaResponseData(ulong orderId, ulong arcaExtID, ArcaDataServiceReference.CreditCardEcommerceResponse response)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_attached_card_to_card_arca_response (arca_ext_id, response_code, processing_code, send_date, rrn, auth_id, doc_ID) VALUES (@arcaExtId, @responseCode, @processingCode, GETDATE(), @rrn, @authId, @docId)", conn))
                {
                    cmd.Parameters.Add("@responseCode", SqlDbType.NVarChar, 50).Value = response?.ResponseCode ?? "";
                    cmd.Parameters.Add("@arcaExtId", SqlDbType.BigInt).Value = arcaExtID;
                    cmd.Parameters.Add("@processingCode", SqlDbType.NVarChar, 20).Value = response?.ProcessingCode ?? "";
                    cmd.Parameters.Add("@rrn", SqlDbType.NVarChar, 20).Value = response?.RRN ?? "";
                    cmd.Parameters.Add("@authId", SqlDbType.NVarChar, 20).Value = response?.AuthorizationIdResponse ?? "";
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static CardToCardOrder GetAttachedCardOrder(CardToCardOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT QH.change_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.quality,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
                                                         HB.amount,
                                                         DT.fee_amount,
                                                         HB.currency,
                                                         DT.debit_card_number,
                                                         DT.credit_card_number,
                                                         HB.receiver_name,
														 HB.order_group_id,
                                                         HB.confirmation_date,
                                                         OBC.binding_id,
                                                         CDT.auth_id,
                                                         CDT.rrn,
                                                         DT.id AS transfer_ID
                                    FROM Tbl_HB_documents AS HB 
                                    LEFT JOIN tbl_attached_card_to_card_arca_response  AS CDT ON HB.doc_ID = CDT.doc_ID
		                            JOIN tbl_other_bank_card_orders AS DT ON HB.doc_ID = DT.doc_id
									JOIN (SELECT id, binding_Id  FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION SELECT id, binding_Id FROM [tbl_other_bank_cards_deleted]) OBC ON OBC.id = DT.card_id
                                    JOIN tbl_hb_quality_history AS QH ON HB.doc_ID = QH.Doc_ID
                                    WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END AND HB.doc_ID=@doc_id", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["change_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = Convert.ToDateTime(dt.Rows[0]["operation_date"]);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"].ToString());
                    order.FeeAmount = Convert.ToDouble(dt.Rows[0]["fee_amount"].ToString());
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.DebitCardNumber = dt.Rows[0]["debit_card_number"].ToString();
                    order.CreditCardNumber = dt.Rows[0]["credit_card_number"].ToString();
                    order.EmbossingName = dt.Rows[0]["receiver_name"].ToString();
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.BindingId = dt.Rows[0]["binding_id"].ToString();
                    order.IsAttachedCard = true;
                    order.RRN = dt.Rows[0]["rrn"] != DBNull.Value ? dt.Rows[0]["rrn"].ToString() : string.Empty;
                    order.AuthId = dt.Rows[0]["auth_id"] != DBNull.Value ? dt.Rows[0]["auth_id"].ToString() : string.Empty;
                    order.OrderId = dt.Rows[0]["transfer_ID"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["transfer_ID"].ToString()) : 0;
                }
            }
            return order;
        }
        internal static string GetSourceCardholderName(int docId)
        {
            string attachedCardHolderName = string.Empty;
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            conn.Open();
            string script = @"select b.carholder_name from tbl_other_bank_card_orders a 
                                        inner join [tbl_other_bank_cards]  b on a.debit_card_number = b.card_number 
                                        where a.doc_id = @doc_id ";

            using SqlCommand cmd = new SqlCommand(script, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;
            var result = cmd.ExecuteScalar();
            if (result != null)
                attachedCardHolderName = result.ToString();

            return attachedCardHolderName;
        }
    }
}
