using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class CardlessCashoutOrderDB
    {
        internal static CardlessCashoutOrder Get(CardlessCashoutOrder order, Languages lang)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                                        SELECT D.doc_id,
                                               D.filial,
                                               D.registration_date, 
                                               D.document_type,
                                               D.document_subtype, 
                                               D.currency, 
                                               D.[description], 
                                               D.quality, 
                                               D.amount, 
                                               D.deb_for_transfer_payment, 
                                               D.source_type,
                                               D.operation_date, 
                                               D.order_group_id, 
                                               D.amount_for_payment, 
                                               D.rate_sell_buy, 
                                               D.debet_account, 
                                               D.confirmation_date,
                                               D.use_credit_line,
                                               C.mobile_phone_number, 
                                               C.amount_amd, 
                                               C.amount_amd_not_converted, 
                                               C.order_OTP,
                                               C.order_OTP_generation_date,
											   L.Is_Ok,
											   L.operation_Date,
											   L.rejection_msg_arm,
											   L.rejection_msg_eng,
                                               D.customer_number
                                      FROM   tbl_hb_documents D 
                                               INNER JOIN tbl_cardless_cashout_order_details C 
                                               ON D.doc_id = C.doc_id 
											   LEFT JOIN tbl_cardless_cashout_operations_log L
											   ON D.Doc_id = L.Doc_id
                                        WHERE  D.doc_id = @id
                                        ORDER BY L.operation_date DESC";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());

                            order.CustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            order.SubType = Convert.ToByte((dr["document_subtype"]));


                            if (dr["debet_account"] != DBNull.Value)
                            {
                                string debitAccount = dr["debet_account"].ToString();

                                order.DebitAccount = Account.GetAccount(debitAccount);

                                if (order.DebitAccount != null)
                                {
                                    if (order.DebitAccount.IsIPayAccount())
                                    {
                                        order.DebitAccount.IsAttachedCard = true;
                                        order.DebitAccount.BindingId = Account.GetAttachedCardBindingId(order.Id);
                                        order.DebitAccount.AttachedCardNumber = Account.GetAttachedCardNumber(order.Id);
                                    }
                                }

                            }

                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["amount_for_payment"] != DBNull.Value)
                                order.TransferFee = Convert.ToDouble(dr["amount_for_payment"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();

                            order.SubType = Convert.ToByte(dr["document_subtype"]);


                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());


                            if (dr["rate_sell_buy"] != DBNull.Value)
                                order.ConvertationRate = Convert.ToDouble(dr["rate_sell_buy"]);


                            if (dr["deb_for_transfer_payment"] != DBNull.Value)
                                order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dr["deb_for_transfer_payment"]));


                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                            if (dr["source_type"] != DBNull.Value)
                            {
                                order.Source = (SourceType)Convert.ToInt16(dr["source_type"]);
                            }
                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                            order.FilialCode = Convert.ToUInt16(dr["filial"].ToString());

                            if (dr["mobile_phone_number"] != DBNull.Value)
                                order.MobilePhoneNumber = Utility.ConvertAnsiToUnicode(dr["mobile_phone_number"].ToString());

                            if (dr["amount_amd"] != DBNull.Value)
                                order.AmountInAmd = Convert.ToDouble(dr["amount_amd"]);

                            if (dr["amount_amd_not_converted"] != DBNull.Value)
                                order.AmountInAMDNotConverted = Convert.ToDouble(dr["amount_amd_not_converted"]);

                            if (dr["order_OTP"] != DBNull.Value)
                                order.OrderOTP = dr["Order_OTP"].ToString();

                            if (dr["order_OTP_generation_date"] != DBNull.Value)
                                order.OTPGenerationDate = Convert.ToDateTime(dr["order_OTP_generation_date"].ToString());
                            order.UseCreditLine = dr["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dr["use_credit_line"]) : false;

                            order.CashoutAttemptDate = dr.FieldOrDefault<DateTime?>("operation_Date");
                            if (dr["is_Ok"] != DBNull.Value)
                            {
                                bool isOk = Convert.ToBoolean(dr["is_Ok"]);
                                order.AttemptStatus = isOk ? order.AttemptStatus = CardLessCashoutStatus.Completed : CardLessCashoutStatus.Rejected;
                                if (!isOk)
                                {
                                    order.RejectionMessage = lang == Languages.hy ? dr["rejection_msg_arm"].ToString() : dr["rejection_msg_eng"].ToString();
                                }
                            }
                            else
                            {
                                order.AttemptStatus = CardLessCashoutStatus.Waiting;
                            }

                        }
                        else
                            order = null;
                    }
                }
            }



            return order;
        }


        internal static ActionResult Save(CardlessCashoutOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_cardless_cashout_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.DebitAccount.Currency;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 50).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@service_fee_account", SqlDbType.VarChar, 50).Value = order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.TransferFee;
                    cmd.Parameters.Add("@mobile_phone_number", SqlDbType.NVarChar, 50).Value = order.MobilePhoneNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@Amount_In_AMD", SqlDbType.Float).Value = order.AmountInAmd;
                    cmd.Parameters.Add("@Amount_In_AMD_not_converted", SqlDbType.Float).Value = order.AmountInAMDNotConverted;
                    cmd.Parameters.Add("@Convertation_Rate", SqlDbType.Float).Value = order.ConvertationRate;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = order.Description;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = order.UseCreditLine;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0 || actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }
        }

        public static void UpdateInDB(string OTP, long docId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"UPDATE TBl_cardless_cashout_order_details
                               SET order_OTP = @OTP , order_OTP_generation_date = GETDATE()
                               WHERE doc_id = @docId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@OTP", SqlDbType.VarChar, 10).Value = OTP;
                    cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;

                    conn.Open();

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public static CardlessCashoutOrder GetCardlessCashoutOrderForAtmView(string cardlessCashOutCode)
        {
            CardlessCashoutOrder order = new CardlessCashoutOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT hb.doc_id, hb.customer_number ,cod.mobile_phone_number,cod.amount_amd, cod.amount_amd_not_converted FROM tbL_hb_documents HB 
										                                    INNER JOIN 
										                                    TBl_cardless_cashout_order_details COD 
										                                    ON hb.doc_id = cod.doc_id
									                                    	WHERE  cod.order_OTP = @cardlessCashOutCode";
                    cmd.Parameters.Add("@cardlessCashOutCode", SqlDbType.NVarChar).Value = cardlessCashOutCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());
                            order.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                            order.MobilePhoneNumber = (dr["mobile_phone_number"].ToString());
                            order.AmountInAmd = double.Parse(dr["amount_amd_not_converted"].ToString());
                        }
                    }
                }
            }
            return order;
        }

        public static bool IsCardlessCashCodeCorrect(string cardlessCashOutCode)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT 1 FROM TBl_cardless_cashout_order_details c
                                        INNER JOIN (SELECT quality, doc_id FROM TBl_HB_documents) HB ON c.doc_id = HB.doc_id
                                        WHERE quality = 3 AND order_otp = @cardlessCashOutCode";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardlessCashOutCode", SqlDbType.NVarChar).Value = cardlessCashOutCode;
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        public static double GetServiceFee(double amont)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select [dbo].[Get_Cardless_Cashout_fee](@amount)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = amont;
                    return Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
        }


        public static void UpdateTransactionDetails(string atmid, string transactionid, string otp)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"UPDATE TBl_cardless_cashout_order_details
                               SET TransactionID = @TransactionID, ATMID = @AtmID
                               WHERE order_OTP = @OTP";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@TransactionID", SqlDbType.NVarChar).Value = transactionid;
                    cmd.Parameters.Add("@AtmID", SqlDbType.NVarChar).Value = atmid;
                    cmd.Parameters.Add("@OTP", SqlDbType.NVarChar).Value = otp;

                    conn.Open();

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public static ActionResult Confirm(ulong docID, string TransactionId, string AtmId, SourceType source = SourceType.AcbaOnline)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_confirm_cardless_cashout_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@bankingSource", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = 88;
                    cmd.Parameters.Add("@Atm_Id", SqlDbType.NVarChar).Value = AtmId;
                    cmd.Parameters.Add(new SqlParameter("@itemNumber", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    return new ActionResult { ResultCode = ResultCode.Normal };

                }
            }
        }

        internal static CardlessCashoutOrder GetCardlessCashoutOrder(string cardlessCashOutCode)
        {
            CardlessCashoutOrder order = new CardlessCashoutOrder();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT   HB.customer_number, COD.mobile_phone_number, COD.amount_amd, HB.doc_id, HB.quality, order_OTP_generation_date, COD.amount_amd_not_converted
                                                                            FROM tbL_hb_documents HB
										                                    INNER JOIN
										                                    TBl_cardless_cashout_order_details COD
										                                    ON hb.doc_id = cod.doc_id
									                                    	WHERE  cod.order_OTP = @cardlessCashOutCode";
                    cmd.Parameters.Add("@cardlessCashOutCode", SqlDbType.NVarChar).Value = cardlessCashOutCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());
                            if (dr["order_OTP_generation_date"] != DBNull.Value)
                                order.OTPGenerationDate = Convert.ToDateTime(dr["order_OTP_generation_date"].ToString());
                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                            order.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                            order.MobilePhoneNumber = dr["mobile_phone_number"].ToString();
                            order.AmountInAmd = double.Parse(dr["amount_amd_not_converted"].ToString());
                        }
                    }
                }
            }
            return order;
        }



        public static void WriteCardlessCashoutLog(ulong docID, bool isOk, string msgArm, string msgEng, string AtmId, byte step)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_cardless_cashout_logging";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@is_Ok", SqlDbType.Bit).Value = isOk;
                    cmd.Parameters.Add("@operation_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@rejection_msg_arm", SqlDbType.NVarChar, 200).Value = msgArm;
                    cmd.Parameters.Add("@rejection_msg_eng", SqlDbType.NVarChar, 200).Value = msgEng;
                    cmd.Parameters.Add("@step", SqlDbType.TinyInt).Value = step;
                    cmd.Parameters.Add("@ATM_id", SqlDbType.NVarChar, 50).Value = AtmId;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        //return new ActionResult { ResultCode = ResultCode.Failed }; քոմենթ քանի դեռ կատարման կտորը վերջնական պատրաստ չէ
                    }

                }
            }
        }
        public static void SaveCancelNotificationMessage(string request)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_cardless_cashout_notification_message";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@request", SqlDbType.NVarChar).Value = request;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void SaveInProcess(long id, string atmId, string transactionId)
        {
            string query = @"INSERT INTO Tbl_cardless_cashout_order_in_process(doc_id, registration_date, atm_id, transaction_id)
                            VALUES (@doc_id, @registration_date, @atm_id, @transaction_id)";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@atm_id", SqlDbType.NVarChar, 10).Value = atmId;
                    cmd.Parameters.Add("@transaction_id", SqlDbType.NVarChar, 20).Value = transactionId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static bool CheckOrderProcess(long id)
        {
            string query = @"SELECT 1 FROM Tbl_cardless_cashout_order_in_process
                                            WHERE doc_id = @doc_id";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = id;
                    return cmd.ExecuteReader().Read();
                }
            }
        }


    }
}
