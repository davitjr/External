using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public class PaymentToARCAOrderDB
    {

        internal static ActionResult Save(PaymentToARCAOrder order, ACBAServiceReference.User user, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"DECLARE @filial as int
                                                    if @customer_number<>0
                                                    begin
                                                        select @filial=filialcode from Tbl_customers where customer_number=@customer_number
                                                    end
                                                    else
                                                    begin 
                                                        set @filial=0
                                                    END 

                                      INSERT INTO  dbo.Tbl_HB_documents 
                                            (filial,customer_number, registration_date, document_type, document_number, document_subtype,amount,currency,debet_account,description, quality, source_type, operationFilialCode, operation_date)
                                            VALUES
                                            (@filial,@customer_number, @registrationDate, @docType, @docNumber, @documentSubtype, @amount,@currency,@debit_acc,(SELECT product_description FROM [dbo].[Tbl_types_of_HB_products] WHERE product_type=@docType),1,@sourceType, @operationFilialCode, @operDay)
                                       
                                       SET @id=Scope_identity()
                                       Select Scope_identity() as ID                                         ";

                    cmd.CommandType = CommandType.Text;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@registrationDate", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = user.userName;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@documentSubtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (short)order.Type;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount + order.CardFee;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;
                }
                if (order.Source == SourceType.SSTerminal)
                {
                    OrderDB.SaveOrderDetails(order);
                }
            }

            return result;
        }
        internal static ActionResult AddTransactionToCardsPayments(PaymentToARCAOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_Add_Transaction_To_Cards_Payments";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@card_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;

                    if (order.Currency == "AMD")
                        cmd.Parameters.Add("@amount_amd", SqlDbType.Float).Value = order.Amount + order.CardFee;
                    else
                        cmd.Parameters.Add("@amount_amd", SqlDbType.Float).Value = order.AmountAMD;

                    cmd.Parameters.Add("@amount_val", SqlDbType.Float).Value = order.Amount + order.CardFee;
                    cmd.Parameters.Add("@date_of_accounting", SqlDbType.DateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@debitCredit", SqlDbType.VarChar).Value = Validation.CashOperationDirection(order);
                    cmd.Parameters.Add("@responseCode", SqlDbType.VarChar).Value = order.ARCAResponseCode;
                    cmd.Parameters.Add("@currentAccountNumber", SqlDbType.SmallInt).Value = 967;
                    cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = order.PaymentToARCAStatus;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);
                }
            }

            return result;
        }
    }
}
