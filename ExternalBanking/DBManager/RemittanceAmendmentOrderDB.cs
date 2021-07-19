using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal static class RemittanceAmendementOrderDB
    {
        /// <summary>
        /// Արագ դրամական համակարգերով ուղարկված փոխանցման տվյալների փոփոխման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult Save(RemittanceAmendmentOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_remittance_amendment_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = OrderType.RemittanceAmendmentOrder;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;

                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@URN", SqlDbType.NVarChar, 10).Value = order.URN;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar).Value = order.OrderNumber;

                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = order.Transfer.Id;

                    cmd.Parameters.Add("@amendment_reason_code", SqlDbType.NVarChar).Value = order.AmendmentReasonCode;
                    cmd.Parameters.Add("@amendment_reason_name", SqlDbType.NVarChar).Value = order.AmendmentReasonName;
                    cmd.Parameters.Add("@before_bene_last_name", SqlDbType.NVarChar).Value = order.BeforeBeneLastName;
                    cmd.Parameters.Add("@before_bene_middle_name", SqlDbType.NVarChar).Value = order.BeforeBeneMiddleName;
                    cmd.Parameters.Add("@before_bene_first_name", SqlDbType.NVarChar).Value = order.BeforeBeneFirstName;
                    cmd.Parameters.Add("@beneficiary_last_name", SqlDbType.NVarChar).Value = order.BeneficiaryLastName;
                    cmd.Parameters.Add("@beneficiary_middle_name", SqlDbType.NVarChar).Value = order.BeneficiaryMiddleName;
                    cmd.Parameters.Add("@beneficiary_first_name", SqlDbType.NVarChar).Value = order.BeneficiaryFirstName;
                    cmd.Parameters.Add("@before_NAT_beneficiary_last_name", SqlDbType.NVarChar).Value = order.BeforeNATBeneficiaryLastName;
                    cmd.Parameters.Add("@before_NAT_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.BeforeNATBeneficiaryMiddleName;
                    cmd.Parameters.Add("@before_NAT_bene_first_name", SqlDbType.NVarChar).Value = order.BeforeNATBeneficiaryFirstName;
                    cmd.Parameters.Add("@NAT_beneficiary_last_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryLastName;
                    cmd.Parameters.Add("@NAT_beneficiary_middle_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryMiddleName;
                    cmd.Parameters.Add("@NAT_beneficiary_first_name", SqlDbType.NVarChar).Value = order.NATBeneficiaryFirstName;

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
        /// Ստուգում է՝ արդյոք տվյալ փոխանցման համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված տվյալների փոփոխման հայտ, թե ոչ։
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="subType"></param>
        /// <param name="transferId"></param>
        /// <returns></returns>
        internal static bool ExistsNotConfirmedAmendmentOrder(ulong customerNumber, byte subType, ulong transferId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @" IF EXISTS(SELECT doc_id
					                               FROM tbl_hb_documents
                                                   WHERE document_type = @document_type AND quality = 50 AND customer_number = @customer_number 
                                                   and document_subtype = @document_sub_type and transfer_id = @transfer_id) 
		                                                BEGIN
					                                                SELECT 1 result 
		                                                END
                                                            ELSE 
		                                                            SELECT 0 result ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@document_type", SqlDbType.SmallInt).Value = OrderType.RemittanceAmendmentOrder;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = subType;
                    cmd.Parameters.Add("@transfer_id", SqlDbType.Int).Value = transferId;


                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Նշում է՝ արդյոք փոխանցման տվյալները հաջողությամբ փոփոխվել են ARUS համակարգում, թե ոչ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="successValue"></param>
        internal static void UpdateARUSSuccess(long orderId, short successValue)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE TBl_remittance_amendment_order_details SET ARUS_success = @successValue WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@successValue", SqlDbType.TinyInt).Value = successValue;

                    cmd.ExecuteScalar();

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
                using (SqlCommand cmd = new SqlCommand(@"UPDATE TBl_remittance_amendment_order_details SET ARUS_message = @message WHERE Doc_id=@doc_id ", conn))
                {
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = message;

                    cmd.ExecuteScalar();

                }
            }
        }


        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման հայտի մանրամասները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static RemittanceAmendmentOrder GetRemittanceAmendmentOrder(RemittanceAmendmentOrder order)
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
                                        hb.transfer_id,
                                        d.ARUS_success, 
	                                    d.URN, 	 
                                        d.amendment_reason_code,
                                        d.amendment_reason_name,
                                        d.before_bene_last_name,
                                        d.before_bene_middle_name,
                                        d.before_bene_first_name,
                                        d.beneficiary_last_name,
                                        d.beneficiary_middle_name,
                                        d.beneficiary_first_name,
                                        d.before_NAT_bene_first_name,
                                        d.before_NAT_beneficiary_last_name,
                                        d.before_NAT_beneficiary_middle_name,
                                        d.NAT_beneficiary_first_name,
                                        d.NAT_beneficiary_last_name,
                                        d.NAT_beneficiary_middle_name,
                                        d.ARUS_Success,
                                        d.ARUS_message
			                            FROM Tbl_HB_documents hb
                                        INNER JOIN TBl_remittance_amendment_order_details d  ON hb.doc_ID = d.doc_id                                       
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
                    order.AmendmentReasonCode = dt.Rows[0]["amendment_reason_code"].ToString();
                    order.AmendmentReasonName = dt.Rows[0]["amendment_reason_name"].ToString();

                    order.BeforeBeneLastName = dt.Rows[0]["before_bene_last_name"] != DBNull.Value ? dt.Rows[0]["before_bene_last_name"].ToString() : "";
                    order.BeforeBeneMiddleName = dt.Rows[0]["before_bene_middle_name"] != DBNull.Value ? dt.Rows[0]["before_bene_middle_name"].ToString() : "";
                    order.BeforeBeneFirstName = dt.Rows[0]["before_bene_first_name"] != DBNull.Value ? dt.Rows[0]["before_bene_first_name"].ToString() : "";
                    order.BeneficiaryLastName = dt.Rows[0]["beneficiary_last_name"] != DBNull.Value ? dt.Rows[0]["beneficiary_last_name"].ToString() : "";
                    order.BeneficiaryMiddleName = dt.Rows[0]["beneficiary_middle_name"] != DBNull.Value ? dt.Rows[0]["beneficiary_middle_name"].ToString() : "";
                    order.BeneficiaryFirstName = dt.Rows[0]["beneficiary_first_name"] != DBNull.Value ? dt.Rows[0]["beneficiary_first_name"].ToString() : "";
                    order.BeforeNATBeneficiaryFirstName = dt.Rows[0]["before_NAT_bene_first_name"] != DBNull.Value ? dt.Rows[0]["before_NAT_bene_first_name"].ToString() : "";
                    order.BeforeNATBeneficiaryLastName = dt.Rows[0]["before_NAT_beneficiary_last_name"] != DBNull.Value ? dt.Rows[0]["before_NAT_beneficiary_last_name"].ToString() : "";
                    order.BeforeNATBeneficiaryMiddleName = dt.Rows[0]["before_NAT_beneficiary_middle_name"] != DBNull.Value ? dt.Rows[0]["before_NAT_beneficiary_middle_name"].ToString() : "";
                    order.NATBeneficiaryFirstName = dt.Rows[0]["NAT_beneficiary_first_name"] != DBNull.Value ? dt.Rows[0]["NAT_beneficiary_first_name"].ToString() : "";
                    order.NATBeneficiaryLastName = dt.Rows[0]["NAT_beneficiary_last_name"] != DBNull.Value ? dt.Rows[0]["NAT_beneficiary_last_name"].ToString() : "";
                    order.NATBeneficiaryMiddleName = dt.Rows[0]["NAT_beneficiary_middle_name"] != DBNull.Value ? dt.Rows[0]["NAT_beneficiary_middle_name"].ToString() : "";

                    order.ARUSSuccess = Convert.ToInt16(dt.Rows[0]["ARUS_Success"].ToString());
                    if (dt.Rows[0]["ARUS_Message"] != DBNull.Value)
                    {
                        order.ARUSErrorMessage = dt.Rows[0]["ARUS_Message"].ToString();
                    }

                }
            }


            return order;
        }


        internal static long GetNotConfirmedOrderIdByTransfer(ulong transferId)
        {
            DataTable dt = new DataTable();

            long docId = 0;

            string sql = @"SELECT top 1 doc_id
			               FROM Tbl_HB_documents                                
                           WHERE transfer_id = @transfer_id and document_type = 202 and document_subtype = 1 and quality = 30
                           ORDER BY doc_id desc";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@transfer_id", SqlDbType.BigInt).Value = transferId;
                    dt.Load(cmd.ExecuteReader());

                    docId = Convert.ToInt64(dt.Rows[0]["doc_id"].ToString());
                 
                }
            }


            return docId;
        }

        internal static int GetRemittanceAmendmentCount(ulong transferId)
        {
            int count = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT COUNT(1) + 1 as transfer_count from Tbl_HB_Documents
                                                  WHERE document_type = 202 AND transfer_id = @transfer_id AND quality = 30", conn);

                cmd.Parameters.Add("@transfer_id", SqlDbType.Int).Value = transferId;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                    int.TryParse(dr["transfer_count"].ToString(), out count);
            }

            return count;
        }

    }
}
