using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class HBActivationOrderDB
    {

        public static List<HBActivationRequest> GetHBRequests(ulong customerNumber)
        {
            List<HBActivationRequest> list = new List<HBActivationRequest>();
            ushort customerType = Customer.GetCustomerType(customerNumber);
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT  rq.* FROM tbl_applications ap
                                                          INNER JOIN 
                                                          Tbl_Requests_For_Fee_Charges rq
                                                          ON ap.ID=rq.global_ID
                                                          WHERE ap.customer_number=@customer_number AND rq.is_complete=0 AND rq.contract_type = 1
                                                    ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.VarChar, 50).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        HBActivationRequest request = new HBActivationRequest();
                        request.Id = Convert.ToInt64(dr["id"]);

                        request.RequestType = Convert.ToInt16(dr["request_type"]);
                        if (request.RequestType == 3)
                        {
                            request.RequestDate = Utility.GetCurrentOperDay();
                        }
                        else if (request.RequestType == 1 || request.RequestType == 2)
                        {
                            request.RequestDate = dr["application_date"] != DBNull.Value ? Convert.ToDateTime(dr["application_date"]) : default(DateTime);
                        }
                        request.RequestTypeDescription = Info.GetTypeOfRequestsForFeeCharge(request.RequestType);
                        //request.ServiceFee = Convert.ToDouble(dr["service_fee"]);
                        request.UserName = dr["user_name"].ToString();
                        request.HBToken = new HBToken();
                        request.HBToken.HBUser = new HBUser();
                        request.HBToken.HBUser.CustomerNumber = Convert.ToUInt64(dr["user_customer_number"]);
                        request.HBToken.HBUser.HBAppID = Convert.ToInt32(dr["global_ID"]);
                        if (request.RequestType == 1 || request.RequestType == 2)
                        {
                            request.HBToken.HBUser.UserFullName = Utility.ConvertAnsiToUnicode(dr["user_full_name"].ToString());
                            request.HBToken.TokenNumber = dr["token_number"].ToString();
                            request.HBToken.TokenType = (HBTokenTypes)Convert.ToInt16(dr["token_type"]);
                            request.HBToken.TokenTypeDescription = Info.GetTokenTypeDescription(request.HBToken.TokenType);
                            request.HBToken.TokenSubType = (HBTokenSubType)Convert.ToInt16(dr["token_sub_type"]);
                        }

                        if (customerType == 6)
                        {
                            request.ServiceFee = HBToken.GetHBServiceFee(request.HBToken.HBUser.CustomerNumber, Utility.GetCurrentOperDay(), (HBServiceFeeRequestTypes)request.RequestType, request.HBToken.TokenType, request.HBToken.TokenSubType);
                        }
                        else
                        {
                            request.ServiceFee = HBToken.GetHBServiceFee(customerNumber, Utility.GetCurrentOperDay(), (HBServiceFeeRequestTypes)request.RequestType, request.HBToken.TokenType, request.HBToken.TokenSubType);
                        }
                        list.Add(request);
                    }
                }
            }
            return list;

        }
        internal static ActionResult SaveHBActivationOrder(HBActivationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"
                                                INSERT INTO Tbl_HB_documents(filial,customer_number,registration_date,document_type,document_subtype,
                                                document_number,amount,currency,debet_account,credit_account,
                                                [description],quality,source_type,operationFilialCode,operation_date)
                                                VALUES
                                                (@operationFilialCode,@customer_number,@reg_date,@doc_type,@document_subtype,@doc_number,
                                                @amount,@currency,@debit_acc,@credit_account,@descr,1,@source_type,@operationFilialCode,@oper_day)
                                                UPDATE Tbl_HB_quality_history SET change_user_name = @username WHERE Doc_ID = Scope_identity() AND quality = 1
                                                SELECT Scope_identity() AS ID
                                                ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.VarChar, 50).Value = order.DebitAccount != null ? order.DebitAccount.AccountNumber : "";
                cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.CreditAccount != null ? order.CreditAccount.AccountNumber : "";
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description == null ? "" : order.Description;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());

            }
            SaveHBActivationOrderDetils(order);
            result.ResultCode = ResultCode.Normal;

            return result;
        }



        internal static HBActivationOrder Get(HBActivationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT registration_date,document_type,document_subtype,document_number,quality,
                                            amount,currency,debet_account,credit_account, [description],quality,source_type,ac.*,hb.operation_date, hb.confirmation_date
										    FROM  Tbl_HB_documents hb inner join Tbl_HB_Activation_Order_Details ac on
											hb.doc_ID=ac.doc_ID
                                            WHERE hb.doc_ID=@docID and hb.customer_number=case when @customer_number = 0 then hb.customer_number else @customer_number end";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();


                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.DebitAccount = Account.GetAccount(dr["debet_account"].ToString());
                    order.Description = dr["description"].ToString();
                    if (!string.IsNullOrEmpty(dr["credit_account"].ToString()))
                    {
                        order.CreditAccount = Account.GetSystemAccount(dr["credit_account"].ToString());
                    }
                    order.Amount = Convert.ToDouble(dr["amount"].ToString());
                    order.Currency = dr["currency"].ToString();
                    //order.HBActivationRequest = new HBActivationRequest();
                    //order.HBActivationRequest.RequestDate = Convert.ToDateTime(dr["request_date"].ToString());
                    //order.HBActivationRequest.RequestType = Convert.ToInt16(dr["request_type"].ToString());
                    //order.HBActivationRequest.Id = Convert.ToInt64(dr["request_id"].ToString());
                    //order.HBActivationRequest.UserName = dr["user_name"].ToString();
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);

                }


            }
            return order;
        }

        internal static ActionResult SaveHBActivationOrderDetails(HBActivationOrder activationHBOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_HB_activation_details(order_id, global_ID, request_type, request_date) 
                                        VALUES(@orderId, @globalID, @requestType, @requestDate)";

                    foreach (HBActivationRequest request in activationHBOrder.HBActivationRequests)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                        cmd.Parameters.Add("@globalID", SqlDbType.Int).Value = request.HBToken.HBUser.HBAppID;
                        cmd.Parameters.Add("@requestType", SqlDbType.SmallInt).Value = request.RequestType;
                        cmd.Parameters.Add("@requestDate", SqlDbType.DateTime).Value = request.RequestDate;
                        cmd.ExecuteNonQuery();
                    }

                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }


        /// <summary>
        /// Ստուգում է գոյություն ունի ուղարկված բայց չհաստատվաց ՀԲ ի ակտիվացման հայտ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        internal static bool HasHBActivationOrder(ulong customerNumber)
        {
            bool check;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=69 and document_subtype=1 and
                                                  customer_number=@customer_number", conn);
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    check = true;
                }
                else
                    check = false;
            }
            return check;
        }

        public static List<HBToken> GetHBToken(long requestId)
        {
            List<HBToken> list = new List<HBToken>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"select token_serial,token_type,user_full_name,customer_number from Tbl_Requests_For_Fee_Charges where global_ID=@globalID", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@globalID", SqlDbType.Int).Value = requestId;


                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        HBToken token = new HBToken();
                        token.TokenNumber = dr["token_serial"].ToString();
                        token.TokenType = (HBTokenTypes)Convert.ToInt16(dr["token_type"].ToString());
                        token.TokenTypeDescription = Info.GetTokenTypeDescription(token.TokenType);
                        token.HBUser = new HBUser();
                        token.HBUser.UserFullName = Utility.ConvertAnsiToUnicode(dr["user_full_name"].ToString());
                        int index = token.UserFullName.IndexOf("  ");
                        token.HBUser.UserFullName = token.UserFullName.Substring(0, index);
                        token.HBUser.CustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());
                        list.Add(token);
                    }
                }
            }


            return list;

        }


        public static void SaveHBActivationOrderDetils(HBActivationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_HB_Activation_Order_Details(doc_id,global_id,request_type,request_date,user_name,token_number,token_type,token_sub_type,service_fee,is_free) 
                                    VALUES (@doc_id,@global_id,@request_type,@request_date,@user_name,@token_number,@token_type,@token_sub_type,@service_fee,@is_free)", conn);

                foreach (HBActivationRequest request in order.HBActivationRequests)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@global_id", SqlDbType.Int).Value = request.HBToken.HBUser.HBAppID;
                    cmd.Parameters.Add("@request_type", SqlDbType.SmallInt).Value = request.RequestType;
                    cmd.Parameters.Add("@request_date", SqlDbType.DateTime).Value = request.RequestDate;
                    cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = request.ServiceFee;
                    cmd.Parameters.Add("@is_free", SqlDbType.Bit).Value = request.IsFree;
                    if (request.RequestType == 1 || request.RequestType == 2)
                    {
                        cmd.Parameters.Add("@user_name", SqlDbType.NVarChar, 50).Value = request.UserName == null ? "" : request.UserName;
                        cmd.Parameters.Add("@token_number", SqlDbType.NVarChar, 50).Value = request.HBToken.TokenNumber;
                        cmd.Parameters.Add("@token_type", SqlDbType.SmallInt).Value = request.HBToken.TokenType;
                        cmd.Parameters.Add("@token_sub_type", SqlDbType.SmallInt).Value = (ushort)request.HBToken.TokenSubType;
                    }
                    else
                    {
                        cmd.Parameters.Add("@user_name", SqlDbType.NVarChar, 50).Value = request.UserName == null ? "" : request.UserName;
                        cmd.Parameters.Add("@token_number", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                        cmd.Parameters.Add("@token_type", SqlDbType.SmallInt).Value = DBNull.Value;
                        cmd.Parameters.Add("@token_sub_type", SqlDbType.SmallInt).Value = DBNull.Value;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Հեռահար բանկինգի ծառայությունների ակտիվացումից հրաժարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveHBActivationRejectionOrder(HBActivationRejectionOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("pr_HB_Activation_Rejection_order", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 255).Value = order.Description == null ? "" : order.Description;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                cmd.Parameters.Add("@hbRequests", SqlDbType.Structured).Value = ConvertHBRequestToDataTable(order.HBActivationRequests);

                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                result.ResultCode = ResultCode.Normal;
                order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                order.Quality = OrderQuality.Draft;

            }
            result.ResultCode = ResultCode.Normal;

            return result;
        }

        private static DataTable ConvertHBRequestToDataTable(List<HBActivationRequest> hbRequests)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("request_ID");
            dt.Columns.Add("global_ID");
            dt.Columns.Add("token_number");
            dt.Columns.Add("request_type");
            if (hbRequests != null)
            {
                foreach (HBActivationRequest r in hbRequests)
                {
                    DataRow dr = dt.NewRow();
                    dr["request_ID"] = (long)r.Id;
                    dr["global_ID"] = (int)r.HBToken.HBUser.HBAppID;
                    dr["token_number"] = (string)r.HBToken.TokenNumber;
                    dr["request_type"] = (int)r.RequestType;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public static PhoneBankingContractActivationRequest GetPhoneBankingRequests(ulong customerNumber)
        {
            PhoneBankingContractActivationRequest request = new PhoneBankingContractActivationRequest();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT rq.id,rq.request_type,rq.application_date,rq.global_ID FROM Tbl_PhoneBanking_Contracts pb
                                                        INNER JOIN 
                                                        Tbl_Requests_For_Fee_Charges rq
                                                        ON pb.ID=rq.global_ID
                                                        WHERE pb.customer_number=@customer_number AND rq.is_complete=0 AND rq.contract_type = 2
                                                ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.VarChar, 50).Value = customerNumber;


                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    if (dr.Read())
                    {
                        request.Id = Convert.ToInt64(dr["id"]);
                        request.RequestType = (RequestType)Convert.ToInt16(dr["request_type"]);
                        if (request.RequestType == RequestType.NewPhoneBankingContract)
                        {
                            request.RequestDate = Convert.ToDateTime(dr["application_date"]);
                        }
                        request.RequestTypeDescription = Info.GetTypeOfRequestsForFeeCharge((short)request.RequestType);
                        request.GlobalID = Convert.ToInt32(dr["global_ID"]);
                        request.ServiceFee = PhoneBankingContract.GetPBServiceFee(customerNumber, Utility.GetCurrentOperDay(), (HBServiceFeeRequestTypes)request.RequestType);
                    }
                }

            }
            return request;

        }
        internal static ActionResult SavePhoneBankingContractActivationOrder(PhoneBankingContractActivationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("pr_Phone_Banking_Contract_Activation_order", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 255).Value = order.Description == null ? "" : order.Description;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                cmd.Parameters.Add("@global_ID", SqlDbType.Int).Value = order.PBActivationRequest.GlobalID;
                cmd.Parameters.Add("@request_type", SqlDbType.TinyInt).Value = order.PBActivationRequest.RequestType;
                cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.Amount;
                if (order.DebitAccount != null)
                    cmd.Parameters.Add("@debit_account", SqlDbType.VarChar, 20).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@is_free", SqlDbType.Bit).Value = order.PBActivationRequest.IsFree;

                SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                param.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                result.ResultCode = ResultCode.Normal;
                order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                order.Quality = OrderQuality.Draft;

            }
            result.ResultCode = ResultCode.Normal;

            return result;
        }

        internal static HBActivationOrder GetHBActivationOrder(ulong customerNumber, OrderType documentType)
        {
            HBActivationOrder order = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_getHBActOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = (int)documentType;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                order = new HBActivationOrder
                                {
                                    Id = Convert.ToInt32(dr["doc_id"].ToString()),
                                    RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString()),
                                    OrderNumber = dr["document_number"].ToString(),
                                    SubType = Convert.ToByte(dr["document_subtype"].ToString()),
                                    Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString()),
                                    Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString()),
                                    Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString()),
                                    DebitAccount = Account.GetAccount(dr["debet_account"].ToString()),
                                    Description = dr["description"].ToString(),
                                    CreditAccount = !string.IsNullOrEmpty(dr["credit_account"].ToString()) ? Account.GetSystemAccount(dr["credit_account"].ToString()) : default,
                                    Amount = Convert.ToDouble(dr["amount"].ToString()),
                                    Currency = dr["currency"].ToString(),
                                    OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?)
                                };
                            }
                        }
                    }
                }
            }
            return order;
        }
    }
}
