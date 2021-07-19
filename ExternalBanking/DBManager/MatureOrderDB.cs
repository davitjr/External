using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; 

namespace ExternalBanking.DBManager
{
    class MatureOrderDB
    {
        internal static ActionResult Save(MatureOrder order,string userName,SourceType source)
        {
            ActionResult result = new ActionResult();

            using(SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using(SqlCommand cmd=new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addLoanRepaymentDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id!=0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value =(int)order.MatureType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.ProductAccount.AccountNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = order.ProductCurrency;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = order.Description;

                    if (order.PercentAccount!=null)
	                {
		                cmd.Parameters.Add("@AccountAMD", SqlDbType.VarChar, 50).Value =order.PercentAccount.AccountNumber;
	                }
                    cmd.Parameters.Add("@AmountAMD", SqlDbType.Float).Value = order.PercentAmount;
                    if (order.Account!=null)
                    {
                        cmd.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = order.Account.AccountNumber;
                    }
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@AmountAMD_in_AMD", SqlDbType.Float).Value =Math.Round(order.PercentAmount * Utility.GetCBKursForDate(Utility.GetCurrentOperDay().Date.AddDays(-1),order.ProductCurrency),1, MidpointRounding.AwayFromZero);
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@isProblematic", SqlDbType.Bit).Value = order.IsProblematic;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@repaymentSource", SqlDbType.SmallInt).Value = order.RepaymentSourceType;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add("@mature_mode", SqlDbType.SmallInt).Value = order.MatureMode;
                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);
                    if (actionResult==0 || actionResult==9 || actionResult==10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                    }
                    else
                    {
                        result.ResultCode = ResultCode.ValidationError;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    if (order.Source == SourceType.SSTerminal)
                    {
                        OrderDB.SaveOrderDetails(order);
                    }
                    return result;


                }
            }
        }
        internal static List<ActionError> CheckMature(MatureOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "LoanRestAfterMature";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@isCreditLine", SqlDbType.TinyInt).Value = order.Type == OrderType.LoanMature? 0:1;
                    cmd.Parameters.Add("@App_Id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@mature_type", SqlDbType.TinyInt).Value =(int)order.MatureType;
                    cmd.Parameters.Add("@lang", SqlDbType.TinyInt).Value = 0;
                    cmd.Parameters.Add("@CurrentAccount", SqlDbType.Float).Value = order.Account == null ? 0 : Convert.ToUInt64(order.Account.AccountNumber);
                    cmd.Parameters.Add("@CurrentAccountAmd", SqlDbType.Float).Value = order.PercentAccount == null ? 0 : Convert.ToUInt64(order.PercentAccount.AccountNumber);
                    cmd.Parameters.Add("@CurrentAccountAmount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@CurrentAccountAMDAmount", SqlDbType.Float).Value = order.PercentAmount;
                    cmd.Parameters.Add("@SentTransactionsAmount", SqlDbType.Float).Value = Order.GetSentOrdersAmount(order.Account == null ? "0" : order.Account.AccountNumber,order.Source);
                    cmd.Parameters.Add("@SentTransactionsAmountAMD", SqlDbType.Float).Value = Order.GetSentOrdersAmount(order.PercentAccount == null ? "0" : order.PercentAccount.AccountNumber,order.Source);
                    cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (int)order.Source;
                    cmd.Parameters.Add(new SqlParameter("@result_code", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@var1", SqlDbType.NVarChar,50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@var2", SqlDbType.NVarChar,50) { Direction = ParameterDirection.Output });
                    dt.Load(cmd.ExecuteReader());

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result_code"].Value);
                    string var1 = cmd.Parameters["@var1"].Value.ToString();
                    string var2 = cmd.Parameters["@var2"].Value.ToString();
                    if (actionResult!=0)
                    {
                        result.Add(new ActionError((short)actionResult,new string[]{var1.ToString(),var2.ToString()}));
                    }

                    return result;

                }
            }
        }
        internal static MatureOrder Get(MatureOrder order)
        {
            using(SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                
                string sqlString= @" SELECT d.registration_date,d.currency,d.document_number,d.document_subtype,d.document_type,d.quality,
                                    d.source_type,isnull(r.Account,0) as Account,isnull(r.AccountAMD,0) as AccountAMD,r.Amount,ISNULL(r.AmountAMD_in_AMD,0) as AmountAMD_in_AMD,
                                    isnull(r.[last_day_of rate calculation],0) as day_of_rate_calculation,r.App_ID,d.operation_date,r.repayment_source,d.order_group_id,d.confirmation_date,
                                    ISNULL(r.AmountAMD,0) as AmountAMD,matureMode
                                    FROM Tbl_HB_documents D inner join 
                                    Tbl_Loan_Repayment_Details R on D.doc_ID=R.Doc_ID
                                    WHERE d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.doc_ID=@DocID";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            order.Currency = dr["currency"].ToString();
                            order.OrderNumber = dr["document_number"].ToString();
                            order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                            order.MatureType = (MatureType)Convert.ToByte(dr["document_subtype"].ToString());
                            order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                            order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                            order.Account = Account.GetAccount(Convert.ToUInt64(dr["Account"].ToString()));
                            if (order.Account != null)
                            {
                                order.Account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.Account.AccountTypeDescription);
                                if (order.Account.IsIPayAccount())
                                {
                                    order.Account.IsAttachedCard = true;
                                    order.Account.BindingId = Account.GetAttachedCardBindingId(order.Id);
                                    order.Account.AttachedCardNumber = Account.GetAttachedCardNumber(order.Id);
                                }
                            }
                            order.PercentAccount = Account.GetAccount(Convert.ToUInt64(dr["AccountAMD"].ToString()));
                            if (order.PercentAccount != null)
                            {
                                order.PercentAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.PercentAccount.AccountTypeDescription);
                            }
                            order.Amount = Convert.ToDouble(dr["Amount"].ToString());
                            order.PercentAmountInAMD = Convert.ToDouble(dr["AmountAMD_in_AMD"].ToString());
                            order.PercentAmount = Convert.ToDouble(dr["AmountAMD"].ToString());
                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                            order.ProductId = Convert.ToUInt64(dr["App_ID"].ToString());
                            order.DayOfProductRateCalculation = DateTime.Parse(dr["day_of_rate_calculation"].ToString());
                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                            if (dr["repayment_source"] != DBNull.Value)
                                order.RepaymentSourceType = Convert.ToUInt16(dr["repayment_source"].ToString());

                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                            order.MatureMode = Convert.ToInt32(dr["matureMode"].ToString());
                        }
                    }

                       
                }
                  

            }
            return order;
        }

        internal static double GetThreeMonthLoanRate(ulong productId)
        {
            double rate = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sqlString = @" select dbo.fn_three_month_loan_rate(@appID) rate";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            rate = double.Parse(dr["rate"].ToString());
                        }
                    }
 
                }
                   

            }
            return rate;
        }

        internal static double GetLoanMatureCapitalPenalty(MatureOrder order, ACBAServiceReference.User user)
        {           
            double loanMatureCapitalPenalty = 0;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "LoanMature";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@App_Id", SqlDbType.Float).Value = order.ProductId;

                    if (order.MatureType == MatureType.PartialRepaymentByGrafik)
                    {
                        order.MatureType = MatureType.PartialRepayment;
                        order.MatureMode = 1;
                    }

                    cmd.Parameters.Add("@mature_type", SqlDbType.TinyInt).Value =(short)order.MatureType;

                    if (order.Account != null)
                    {
                        cmd.Parameters.Add("@CurrentAccount", SqlDbType.Float).Value = order.Account.AccountNumber;
                        cmd.Parameters.Add("@CurrentAccountRest", SqlDbType.Float).Value = order.Account.Balance - order.Amount; 
                    }
                     
                    if (order.PercentAccount != null)
                    {
                        cmd.Parameters.Add("@CurrentAccountAmd", SqlDbType.Float).Value = order.PercentAccount.AccountNumber;
                        cmd.Parameters.Add("@CurrentAccountAMDRest", SqlDbType.Float).Value = order.PercentAccount.Balance - order.PercentAmount; 
                    }

                    cmd.Parameters.Add("@oper_date", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay().Date.ToString("dd/MMM/yy");     
                    cmd.Parameters.Add("@dtnext", SqlDbType.SmallDateTime).Value = null;
                    cmd.Parameters.Add("@filialcode", SqlDbType.SmallInt).Value = user.filialCode;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = user.userID;

                    SqlParameter param = new SqlParameter("@item_num", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    param.Value = 0;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add("@freezednoattention", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@m3_percent", SqlDbType.Float).Value = GetThreeMonthLoanRate(order.ProductId);
                    cmd.Parameters.Add("@CallOption", SqlDbType.SmallInt).Value = 1;
                    cmd.Parameters.Add("@mature_mode", SqlDbType.SmallInt).Value = order.MatureMode;   ///DAVIT

                    dt.Load(cmd.ExecuteReader());

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["Value_name"].ToString()=="Add_capital_penalty")
                            {

                                if(string.IsNullOrEmpty(dt.Rows[i]["Val"].ToString()))
                                {
                                    loanMatureCapitalPenalty = 0;
                                }
                                else
                                {
                                    loanMatureCapitalPenalty = double.Parse(dt.Rows[i]["Val"].ToString());
                                }
                            }
                        }
                    }
                }
                return loanMatureCapitalPenalty;
            }
        }


        internal static ActionResult SaveMatureOrderDetails(MatureOrder matureOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_mature_order_details(order_id, mature_type, day_of_product_rate_calculation) VALUES(@orderId, @matureType, @dayOfProductRateCalculation)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@matureType", SqlDbType.SmallInt).Value = matureOrder.MatureType;
                    cmd.Parameters.Add("@dayOfProductRateCalculation", SqlDbType.DateTime).Value = matureOrder.DayOfProductRateCalculation;                    

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        internal static bool CheckLoanEquipment(ulong productId)
        {
            bool result = true;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sqlString = @" select dbo.fn_check_equipment_for_credit_product_mature(@appID) result";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (Convert.ToInt16(dr["result"].ToString())==0)
                            {
                                result = false;
                            }
                        }
                    }

                }


            }
            return result;
        }

    }
}
