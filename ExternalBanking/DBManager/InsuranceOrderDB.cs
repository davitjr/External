using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class InsuranceOrderDB
    {


        internal static ActionResult SaveInsuranceOrder(InsuranceOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_add_new_insurance_document";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;


                    if (order.Insurance.InsuranceContractType == 2)
                    {
                        order.Type = OrderType.OtherInsuranceOrder;
                        order.Currency = "AMD";
                    }


                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@compensation_amount", SqlDbType.Money).Value = order.Insurance.CompensationAmount;
                    if (order.Insurance.InsuranceContractType != 2)
                    {
                        cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    }
                    else
                    {
                        cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = 0;
                    }
                    cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 20).Value = order.ReceiverAccount.AccountNumber;
                    cmd.Parameters.Add("@insurance_type", SqlDbType.TinyInt).Value = order.Insurance.InsuranceType;
                    cmd.Parameters.Add("@insurance_company", SqlDbType.TinyInt).Value = order.Insurance.Company;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = order.Insurance.StartDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = order.Insurance.EndDate;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = order.Insurance.InvolvingSetNumber;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@compensation_amount_currency", SqlDbType.NVarChar, 3).Value = order.Insurance.CompensationCurrency;

                    cmd.Parameters.Add("@insurance_contract_type", SqlDbType.Int).Value = order.Insurance.InsuranceContractType;

                    cmd.Parameters.Add("@IdPro", SqlDbType.Float).Value = order.Insurance.IdPro;




                    if (order.Insurance.ConectedProductId != 0)
                        cmd.Parameters.Add("@conected_product_appID", SqlDbType.Float).Value = order.Insurance.ConectedProductId;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;




                    return result;
                }
            }

        }


        internal static InsuranceOrder GetInsuranceOrder(InsuranceOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"select filial,
                                            customer_number,
                                            registration_date,
                                            document_type,
                                            document_number,
                                            document_subtype,
                                            quality,
                                            source_type,
                                            operationFilialCode,
                                            operation_date,
                                            amount,
                                            debet_account,
                                            currency,
                                            description,
                                            insh.*
                                            from Tbl_HB_documents as hb inner join Tbl_insurance_order_details insh 
                                            on hb.doc_ID=insh.Doc_ID
                                            WHERE hb.doc_id=@docID and hb.customer_number=case when @customer_number = 0 then hb.customer_number else @customer_number end";
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
                    order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                    string debitAccount = dr["debet_account"].ToString();
                    if (order.Type == OrderType.InsuranceOrder)
                    {
                        order.DebitAccount = Account.GetAccount(debitAccount);
                    }
                    else
                    {
                        order.DebitAccount = Account.GetSystemAccount(debitAccount);
                    }
                    order.Currency = dr["currency"].ToString();
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.CustomerNumber = Convert.ToUInt64(dr["customer_number"].ToString());
                    order.Insurance = new Insurance();
                    order.Insurance.Company = Convert.ToUInt16(dr["insurance_company"].ToString());
                    order.Insurance.CompensationAmount = Convert.ToDouble(dr["compensation_amount"].ToString());
                    order.Insurance.EndDate = Convert.ToDateTime(dr["end_date"].ToString());
                    order.Insurance.InsuranceType = Convert.ToUInt16(dr["insurance_type"].ToString());
                    order.Insurance.InvolvingSetNumber = Convert.ToInt32(dr["involving_set_number"].ToString());
                    order.Insurance.StartDate = Convert.ToDateTime(dr["start_date"].ToString());

                }


            }
            return order;
        }




    }
}
