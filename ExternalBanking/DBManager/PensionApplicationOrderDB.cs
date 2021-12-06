using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
namespace ExternalBanking.DBManager
{
    public class PensionApplicationOrderDB
    {

        internal static ActionResult SavePensionApplicationOrder(PensionApplicationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPensionApplicationDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    if (order.PensionApplication.Account != null)
                        cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 50).Value = order.PensionApplication.Account.AccountNumber;
                    cmd.Parameters.Add("@service_type", SqlDbType.Int).Value = order.PensionApplication.ServiceType;
                    if (order.PensionApplication.CardType != 0)
                        cmd.Parameters.Add("@card_type", SqlDbType.Int).Value = order.PensionApplication.CardType;

                    cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = order.PensionApplication.ContractDate;
                    cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.PensionApplication.DateOfNormalEnd;
                    if (order.PensionApplication.ContractId != 0)
                        cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = order.PensionApplication.ContractId;

                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
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



        internal static ActionResult SavePensionApplicationOrderDetails(PensionApplicationOrder pensionApplicationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_pension_application_details(order_id,contract_id, service_type, account_number, card_type,contract_date,date_of_normal_end) 
                                        VALUES(@orderId,@contract_id, @service_type, @account_number, @card_type,@contract_date,@date_of_normal_end)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@service_type", SqlDbType.Int).Value = pensionApplicationOrder.PensionApplication.ServiceType;
                    if (pensionApplicationOrder.PensionApplication.Account != null)
                        cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = pensionApplicationOrder.PensionApplication.Account.AccountNumber;
                    else
                        cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = DBNull.Value;
                    if (pensionApplicationOrder.PensionApplication.CardType != 0)
                        cmd.Parameters.Add("@card_type", SqlDbType.Int).Value = pensionApplicationOrder.PensionApplication.CardType;
                    else
                        cmd.Parameters.Add("@card_type", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = pensionApplicationOrder.PensionApplication.ContractDate;
                    cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = pensionApplicationOrder.PensionApplication.DateOfNormalEnd;
                    if (pensionApplicationOrder.PensionApplication.ContractId != 0)
                        cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = pensionApplicationOrder.PensionApplication.ContractId;
                    else
                        cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static ActionResult SavePensionApplicationTerminationOrder(PensionApplicationTerminationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_terminate_pension_application";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = order.PensionApplication.ContractId;
                    cmd.Parameters.Add("@closing_type", SqlDbType.Int).Value = order.ClosingType;
                    cmd.Parameters.Add("@closing_date", SqlDbType.SmallDateTime).Value = order.ClosingDate;
                    if (order.DeathDate != null)
                        cmd.Parameters.Add("@death_date", SqlDbType.SmallDateTime).Value = order.DeathDate;
                    if (!string.IsNullOrEmpty(order.DeathCertificateNumber))
                        cmd.Parameters.Add("@death_certificate_number", SqlDbType.NVarChar, 50).Value = order.DeathCertificateNumber;

                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
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



        internal static PensionApplicationTerminationOrder GetPensionApplicationTerminationOrder(PensionApplicationTerminationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"select hb.doc_ID, 
                                            hb.filial,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number,
                                            hb.document_subtype,
                                            hb.quality, 
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            ap.* 
                                            from 
                                            Tbl_HB_documents hb 
                                            inner join Tbl_pension_application_termination_details ap
                                            on ap.Doc_ID=hb.doc_ID
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
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.ClosingType = Convert.ToInt16(dr["closing_type"].ToString());
                    order.ClosingTypeDescription = Info.GetPensionAppliactionClosingType(order.ClosingType);
                    order.ClosingDate = dr["closing_date"] != DBNull.Value ? Convert.ToDateTime(dr["closing_date"]) : default(DateTime?);
                    order.DeathDate = dr["death_date"] != DBNull.Value ? Convert.ToDateTime(dr["death_date"]) : default(DateTime?);
                    order.DeathCertificateNumber = dr["death_certificate_number"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dr["death_certificate_number"].ToString()) : null;
                    order.PensionApplication = PensionApplication.GetPensionApplication(order.CustomerNumber, Convert.ToUInt64(dr["contract_id"].ToString()));
                }


            }
            return order;
        }



        internal static PensionApplicationOrder GetPensionApplicationOrder(PensionApplicationOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"select hb.doc_ID, 
                                            hb.filial,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number,
                                            hb.document_subtype,
                                            hb.quality, 
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            ap.* 
                                            from 
                                            Tbl_HB_documents hb 
                                            inner join Tbl_pension_application_details ap
                                            on ap.Doc_ID=hb.doc_ID
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
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.PensionApplication = PensionApplication.GetPensionApplication(order.CustomerNumber, Convert.ToUInt64(dr["contract_id"].ToString()));
                }


            }
            return order;
        }


    }
}
