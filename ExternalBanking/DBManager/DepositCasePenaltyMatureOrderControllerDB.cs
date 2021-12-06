using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; 

namespace ExternalBanking.DBManager
{
    class DepositCasePenaltyMatureOrderControllerDB
    {

        internal static ActionResult SaveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"	declare @filial as int
                                                    declare @doc_ID as int
                                                    select @filial=filialcode from Tbl_customers where customer_number=@customer_number
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    [description],quality,
                                                    source_type,operationFilialCode,operation_date)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,@descr,1,@source_type,@operationFilialCode,@oper_day)
                                                    Select @doc_ID= Scope_identity()
                                                    INSERT INTO Tbl_deposit_case_details(Doc_ID,app_id)
                                                    VALUES (@doc_ID,@app_ID)                                                  
                                                    Select @doc_ID as ID", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@app_ID", SqlDbType.Float).Value = order.ProductId;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;

                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = order.ReceiverAccount.AccountNumber;
                cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description == null ? "" : order.Description;
                

                cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
                
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)order.Source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());

                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }


        internal static DepositCasePenaltyMatureOrder GetDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT  dt.*,
                                             hb.filial,
                                             hb.customer_number,
                                             hb.registration_date,
                                             hb.document_type,
                                             hb.document_number,
                                             hb.document_subtype,
                                             hb.quality,
                                             hb.source_type,
                                             hb.operationFilialCode,
                                             hb.operation_date,
                                             hb.debet_account,
                                             hb.credit_account,
                                             hb.amount,
                                             hb.currency,
                                             hb.[description] as description
                                             FROM Tbl_HB_documents hb INNER JOIN Tbl_deposit_case_details dt
                                             ON dt.Doc_ID=hb.doc_ID
                                             WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
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
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.Amount = double.Parse(dr["amount"].ToString());
                    order.Currency = dr["currency"].ToString();
                    order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                    order.ProductId = ulong.Parse(dr["app_id"].ToString());
                    order.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                    if (order.Type == OrderType.DeleteServicePaymentNote)
                    {
                        order.DebitAccount = Account.GetAccount(dr["debet_account"].ToString());
                    }
                    else
                    {
                        order.DebitAccount = Account.GetSystemAccount(dr["debet_account"].ToString());
                    }
                    DepositCase depositCase = DepositCase.GetDepositCase(order.ProductId, order.CustomerNumber);
                    order.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.CreditAccount), "AMD", (ushort)depositCase.FilialCode, 0, "0", "", order.CustomerNumber);
                }


            }
            return order;
        }


    }
}
