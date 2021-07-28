using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class CTOrderDB
    {
        internal static PaymentRegistrationResult SaveCTPaymentOrder(CTPaymentOrder order, string userName)
        {
            PaymentRegistrationResult result = new PaymentRegistrationResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"	DECLARE @docId as int
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    [description],quality,
                                                    source_type,operationFilialCode,rate_sell_buy,operation_date)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,@descr,1,@source_type,@operationFilialCode,@rateSellBuy,@operation_date)
                                                    SET @docId= Scope_identity()
                                                    UPDATE Tbl_HB_quality_history 
                                                    SET change_user_name = @username
                                                    WHERE Doc_ID = @docId AND quality = 1
                                                    SELECT @docId
                                            ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = order.CreditAccount.AccountNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 255).Value = order.Description == null ? "" : order.Description;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)order.Source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@filial", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@rateSellBuy", SqlDbType.Float).Value = order.ConvertationRate;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                if (order.OperationDate != null)
                    cmd.Parameters.Add("@operation_date", SqlDbType.SmallDateTime).Value = order.OperationDate;
                else
                    cmd.Parameters.Add("@operation_date", SqlDbType.SmallDateTime).Value = DBNull.Value;


                order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            }
            //OrderDB.SaveCTOrderDetails(order);
            SaveCTOrderDetails(order);
            result.ResultCode = 0;
            result.ResultCode = 0;
            result.PaymentID = order.Id;
            result.OrderID = order.OrderId;
            return result;
        }

        internal static PaymentRegistrationResult SaveCTLoanMatureOrder(CTLoanMatureOrder order, string userName)
        {
            PaymentRegistrationResult result = new PaymentRegistrationResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addLoanRepaymentDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.CreditAccount.AccountNumber.ToString();
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = order.Currency;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = order.Description;

                    //if (order.PercentAccount != null)
                    //{
                    //    cmd.Parameters.Add("@AccountAMD", SqlDbType.VarChar, 50).Value = order.PercentAccount.AccountNumber;
                    //}
                    //cmd.Parameters.Add("@AmountAMD", SqlDbType.Float).Value = order.PercentAmount;
                    if (order.DebitAccount != null)
                    {
                        cmd.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = order.DebitAccount.AccountNumber;
                    }
                    ////test
                    if (order.Currency == "AMD")
                    {
                        cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    }
                    else
                    {
                        Loan loan = CTLoanMatureOrder.GetLoanByLoanFullNumber(order.CreditAccount.AccountNumber);
                        cmd.Parameters.Add("@AccountAMD", SqlDbType.VarChar, 50).Value = AccountDB.GetProductAccountFromCreditCode(loan.CreditCode, 18, 279).AccountNumber;
                        cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = Math.Round(order.Amount / order.ConvertationRate, 2, MidpointRounding.AwayFromZero);
                        // cmd.Parameters.Add("@AmountAMD_in_AMD", SqlDbType.Float).Value = order.Amount;
                    }
                    //  cmd.Parameters.Add("@AmountAMD_in_AMD", SqlDbType.Float).Value = Math.Round(order.PercentAmount * Utility.GetCBKursForDate(Utility.GetCurrentOperDay().Date.AddDays(-1), order.ProductCurrency), 1, MidpointRounding.AwayFromZero);
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = order.Source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    //  cmd.Parameters.Add("@isProblematic", SqlDbType.Bit).Value = order.IsProblematic;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    //   cmd.Parameters.Add("@repaymentSource", SqlDbType.SmallInt).Value = order.sour;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add("@mature_mode", SqlDbType.SmallInt).Value = (int)order.MatureMode;

                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);
                    if (actionResult == 0 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = 0;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;

                        result.PaymentID = order.Id;
                        result.OrderID = order.OrderId;

                        SaveCTOrderDetails(order);
                    }
                    else
                    {
                        result.ResultCode = 7;
                        //result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }
        }

        internal static CTPaymentOrder GetCTPaymentOrder(CTPaymentOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT  
                                                    filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    description,quality,
                                                    source_type,operationFilialCode,rate_sell_buy,pd.*
													FROM Tbl_HB_documents hb
													inner join tbl_payment_registration_request_details pd
													on hb.doc_ID=pd.Doc_ID
                                                    where hb.doc_ID=@DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());


                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["customer_number"]);
                order.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"]);
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.DebitAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                order.CreditAccount = Account.GetAccount(dt.Rows[0]["credit_account"].ToString());
                order.Description = dt.Rows[0]["description"].ToString();
                order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                order.ConvertationRate = Convert.ToDouble(dt.Rows[0]["rate_sell_buy"]);
                order.PaymentDateTime = Convert.ToDateTime(dt.Rows[0]["payment_date"]);
                order.TerminalID = dt.Rows[0]["terminal_id"].ToString();
                order.OrderId = Convert.ToInt64(dt.Rows[0]["order_id"]);
                order.TerminalAddress = dt.Rows[0]["terminal_address"].ToString();
                order.TerminalDescription = dt.Rows[0]["terminal_descrption"].ToString();
            }
            return order;
        }



        internal static CTLoanMatureOrder GetCTLoanMatureOrder(CTLoanMatureOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                                            SELECT  filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,hb.amount,currency,debet_account,credit_account,
                                                    description,quality,
                                                    source_type,operationFilialCode,rate_sell_buy,pd.*,ld.App_ID,ld.withCreditCode
													FROM Tbl_HB_documents hb
													inner join tbl_payment_registration_request_details pd
													on hb.doc_ID=pd.Doc_ID
													inner join Tbl_Loan_Repayment_Details ld
													on ld.Doc_ID=hb.doc_ID
													where hb.doc_ID=@DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());


                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["customer_number"]);
                order.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"]);
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.DebitAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                order.CreditAccount = Account.GetAccount(dt.Rows[0]["credit_account"].ToString());
                order.Description = dt.Rows[0]["description"].ToString();
                order.Source = (SourceType)(dt.Rows[0]["source_type"]);
                order.ConvertationRate = Convert.ToDouble(dt.Rows[0]["rate_sell_buy"]);
                order.PaymentDateTime = Convert.ToDateTime(dt.Rows[0]["payment_date"]);
                order.TerminalID = dt.Rows[0]["terminal_id"].ToString();
                order.OrderId = Convert.ToInt64(dt.Rows[0]["order_id"]);
                order.TerminalAddress = dt.Rows[0]["terminal_address"].ToString();
                order.TerminalDescription = dt.Rows[0]["terminal_descrption"].ToString();
                order.WithCreditCode = Convert.ToBoolean(dt.Rows[0]["withCreditCode"]);
                order.ProductId = Convert.ToUInt64(dt.Rows[0]["App_ID"]);
            }
            return order;
        }

        internal static bool IsPaymentIdUnique(string accountNumber, long paymentId)
        {
            bool isUnique = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                                                        SELECT hb.doc_ID from Tbl_HB_documents hb
                                                        INNER JOIN tbl_payment_registration_request_details req
                                                        ON hb.doc_ID=req.Doc_ID
                                                        WHERE hb.debet_account=@accountNumber and req.order_id=@paymentId", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@paymentId", SqlDbType.BigInt).Value = paymentId;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        isUnique = true;
                    }
                }

            }
            return isUnique;
        }

        internal static void SaveCTOrderDetails(CTOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"	
                                                    INSERT INTO tbl_payment_registration_request_details
                                                    (Doc_ID,order_id,payment_date,
                                                    terminal_id,terminal_address,terminal_descrption,phone_number)
                                                    values
                                                    (@Doc_ID,@order_id,@payment_date,@terminal_id,@terminal_address,@terminal_descrption,@phone_number)
                                                    Select Scope_identity() as ID
                                            ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@order_id", SqlDbType.Int).Value = order.OrderId;
                cmd.Parameters.Add("@payment_date", SqlDbType.SmallDateTime).Value = order.PaymentDateTime;
                cmd.Parameters.Add("@terminal_id", SqlDbType.NVarChar, 255).Value = order.TerminalID;
                cmd.Parameters.Add("@terminal_address", SqlDbType.NVarChar, 255).Value = order.TerminalAddress;
                cmd.Parameters.Add("@terminal_descrption", SqlDbType.NVarChar, 255).Value = order.TerminalDescription;
                if (order.PhoneNumber != null)
                    cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar, 255).Value = order.PhoneNumber;
                else
                    cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar, 255).Value = DBNull.Value;

                cmd.ExecuteScalar();
            }

        }
        internal static ActionResult ConfirmOrderOnline(long docID, int userID)
        {
            ActionResult result = new ActionResult();
            SqlConnection conn;
            using (conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_confirm_online_transfers";
                    cmd.CommandTimeout = 120;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = userID;
                    cmd.ExecuteNonQuery();
                }
            }
            return result;

        }

    }
}
