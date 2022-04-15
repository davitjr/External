using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal static class RemittanceCancellationOrderDB
    {
        internal static ActionResult Save(RemittanceCancellationOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_remittance_cancellation_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = OrderType.RemittanceCancellationOrder;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;

                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@URN", SqlDbType.NVarChar, 10).Value = order.URN;
                    cmd.Parameters.Add("@send_payout_div_code", SqlDbType.NVarChar, 2).Value = order.SendPayoutDivCode;
                    cmd.Parameters.Add("@cancellation_reversal_code", SqlDbType.NVarChar, 2).Value = order.CancellationReversalCode;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar).Value = order.OrderNumber;

                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = order.Transfer.Id;

                    cmd.Parameters.Add("@beneficiary_first_name", SqlDbType.NVarChar).Value = order.BeneficiaryFirstName;
                    cmd.Parameters.Add("@beneficiary_last_name", SqlDbType.NVarChar).Value = order.BeneficiaryLastName;
                    cmd.Parameters.Add("@beneficiary_middle_name", SqlDbType.NVarChar).Value = order.BeneficiaryMiddleName;
                    cmd.Parameters.Add("@NAT_beneficiary_last_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryLastName;
                    cmd.Parameters.Add("@NAT_beneficiary_first_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryFirstName;
                    cmd.Parameters.Add("@NAT_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryMiddleName;
                    cmd.Parameters.Add("@sender_last_name", SqlDbType.NVarChar).Value = order.NATSenderLastName;
                    cmd.Parameters.Add("@sender_first_name", SqlDbType.NVarChar).Value = order.NATSenderFirstName;
                    cmd.Parameters.Add("@sender_middle_name", SqlDbType.NVarChar).Value = order.NATSenderMiddleName;
                    cmd.Parameters.Add("@NAT_sender_last_name", SqlDbType.NVarChar).Value = order.SenderLastName;
                    cmd.Parameters.Add("@NAT_sender_first_name", SqlDbType.NVarChar).Value = order.SenderFirstName;
                    cmd.Parameters.Add("@NAT_sender_middle_name", SqlDbType.NVarChar).Value = order.SenderMiddleName;

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = order.Id;
                        order.Quality = OrderQuality.Draft;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Հայտի վրա թարմացնում է ARUS համակարգից ստացված սխալի հաղորդագրությունը
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="message"></param>
        internal static void UpdateARUSMessage(long orderId, string message)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_remittance_cancellation_order_details SET ARUS_message = @message WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = message;

                    cmd.ExecuteScalar();

                }
            }
        }

        /// <summary>
        /// Նշում է՝ արդյոք փոխանցումը հաջողությամբ չեղարկվել է ARUS համակարգում, թե ոչ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="successValue"></param>
        internal static void UpdateARUSSuccess(long orderId, short successValue, double remittanceFee, double AMDFee, double sendingFeeAMD, double principalAmount)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_remittance_cancellation_order_details 
                                                         SET ARUS_success = @successValue, remittance_fee = @remittance_fee, AMD_fee = @AMD_fee, sending_fee_AMD = @sending_fee_AMD,
                                                         principal_amount = @principal_amount
                                                         WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@successValue", SqlDbType.TinyInt).Value = successValue;
                    cmd.Parameters.Add("@remittance_fee", SqlDbType.Float).Value = remittanceFee;
                    cmd.Parameters.Add("@AMD_fee", SqlDbType.Float).Value = AMDFee;
                    cmd.Parameters.Add("@sending_fee_AMD", SqlDbType.Float).Value = sendingFeeAMD;
                    cmd.Parameters.Add("@principal_amount", SqlDbType.Float).Value = principalAmount;

                    cmd.ExecuteScalar();

                }
            }
        }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման հայտի մանրամասները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static RemittanceCancellationOrder GetRemittanceCancellationOrder(RemittanceCancellationOrder order)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT 
                                        hb.customer_number,
                                        hb.registration_date,
                                        hb.document_type,
                                        hb.document_number,
                                        hb.document_subtype,
                                        hb.quality,
                                        hb.currency, 
                                        hb.source_type,
                                        hb.operationFilialCode,
                                        hb.operation_date,
                                        hb.filial,
                                        d.ARUS_success, 
	                                    d.URN, 	 
                                        d.cancellation_reversal_code,
                                        d.send_payout_div_code,
                                        d.ARUS_message,
                                        d.transfer_id,
                                        d.ARUS_Success,
                                        d.ARUS_message,
                                        d.remittance_fee,
                                        d.AMD_fee,
                                        d.sending_fee_AMD,
                                        d.principal_amount,
                                        d.beneficiary_first_name,
                                        d.beneficiary_last_name,
                                        d.beneficiary_middle_name,
                                        d.NAT_beneficiary_last_name,
                                        d.NAT_beneficiary_first_name,
                                        d.NAT_beneficiary_middle_name,
                                        d.NAT_sender_last_name,
                                        d.NAT_Sender_first_name,
                                        d.NAT_Sender_middle_name,
                                        d.sender_last_name,
                                        d.Sender_first_name,
                                        d.Sender_middle_name
			                            FROM Tbl_HB_documents hb
                                        INNER JOIN Tbl_remittance_cancellation_order_details d  ON hb.doc_ID = d.doc_id                                       
                                        WHERE hb.Doc_ID = @DocID AND hb.customer_number = @customer_number";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.Transfer = new Transfer();
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);

                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);

                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"].ToString());
                    order.URN = dt.Rows[0]["URN"].ToString();
                    order.Transfer.Id = Convert.ToUInt64(dt.Rows[0]["transfer_id"].ToString());
                    order.CancellationReversalCode = dt.Rows[0]["cancellation_reversal_code"].ToString();
                    order.ARUSSuccess = Convert.ToInt16(dt.Rows[0]["ARUS_Success"].ToString());

                    if (dt.Rows[0]["ARUS_message"] != DBNull.Value)
                        order.ARUSErrorMessage = dt.Rows[0]["ARUS_message"].ToString();

                    if (dt.Rows[0]["remittance_fee"] != DBNull.Value)
                    {
                        order.RemittanceFee = Convert.ToDouble(dt.Rows[0]["remittance_fee"].ToString());
                    }

                    if (dt.Rows[0]["AMD_fee"] != DBNull.Value)
                    {
                        order.AMDFee = Convert.ToDouble(dt.Rows[0]["AMD_fee"].ToString());
                    }

                    if (dt.Rows[0]["sending_fee_AMD"] != DBNull.Value)
                    {
                        order.SendingFeeAMD = Convert.ToDouble(dt.Rows[0]["sending_fee_AMD"].ToString());
                    }

                    if (dt.Rows[0]["principal_amount"] != DBNull.Value)
                    {
                        order.PrincipalAmount = Convert.ToDouble(dt.Rows[0]["principal_amount"].ToString());
                    }

                    if (dt.Rows[0]["beneficiary_first_name"] != DBNull.Value)
                        order.BeneficiaryFirstName = dt.Rows[0]["beneficiary_first_name"].ToString();

                    if (dt.Rows[0]["beneficiary_last_name"] != DBNull.Value)
                        order.BeneficiaryLastName = dt.Rows[0]["beneficiary_last_name"].ToString();

                    if (dt.Rows[0]["beneficiary_middle_name"] != DBNull.Value)
                        order.BeneficiaryMiddleName = dt.Rows[0]["beneficiary_middle_name"].ToString();

                    if (dt.Rows[0]["NAT_beneficiary_last_name"] != DBNull.Value)
                        order.NATBeneficiaryLastName = dt.Rows[0]["NAT_beneficiary_last_name"].ToString();

                    if (dt.Rows[0]["NAT_beneficiary_first_name"] != DBNull.Value)
                        order.NATBeneficiaryFirstName = dt.Rows[0]["NAT_beneficiary_first_name"].ToString();

                    if (dt.Rows[0]["NAT_beneficiary_middle_name"] != DBNull.Value)
                        order.NATBeneficiaryMiddleName = dt.Rows[0]["NAT_beneficiary_middle_name"].ToString();

                    if (dt.Rows[0]["NAT_sender_last_name"] != DBNull.Value)
                        order.NATSenderLastName = dt.Rows[0]["NAT_sender_last_name"].ToString();

                    if (dt.Rows[0]["NAT_Sender_first_name"] != DBNull.Value)
                        order.NATSenderFirstName = dt.Rows[0]["NAT_Sender_first_name"].ToString();

                    if (dt.Rows[0]["NAT_Sender_middle_name"] != DBNull.Value)
                        order.NATSenderMiddleName = dt.Rows[0]["NAT_Sender_middle_name"].ToString();

                    if (dt.Rows[0]["sender_last_name"] != DBNull.Value)
                        order.SenderLastName = dt.Rows[0]["sender_last_name"].ToString();

                    if (dt.Rows[0]["Sender_first_name"] != DBNull.Value)
                        order.SenderFirstName = dt.Rows[0]["Sender_first_name"].ToString();

                    if (dt.Rows[0]["Sender_middle_name"] != DBNull.Value)
                        order.SenderMiddleName = dt.Rows[0]["Sender_middle_name"].ToString();

                }
            }


            return order;
        }


        internal static bool ExistsNotConfirmedRemittanceCancellationOrder(ulong customerNumber, byte subType, ulong transferId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @" IF EXISTS(SELECT H.doc_id
					                               FROM tbl_hb_documents H inner join Tbl_remittance_cancellation_order_details C on H.doc_id = C.doc_id
                                                   WHERE document_type = @document_type AND quality = 50 AND customer_number = @customer_number 
                                                   and document_subtype = @document_sub_type and C.transfer_id = @transfer_id) 
		                                                BEGIN
					                                                SELECT 1 result 
		                                                END
                                                            ELSE 
		                                                            SELECT 0 result ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@document_type", SqlDbType.SmallInt).Value = OrderType.RemittanceCancellationOrder;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = subType;
                    cmd.Parameters.Add("@transfer_id", SqlDbType.Int).Value = transferId;


                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

    }
}
