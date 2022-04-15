using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class DepositCaseOrderDB
    {




        internal static ActionResult SaveDepositCaseOrder(DepositCaseOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewDepositCaseDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@case_number", SqlDbType.NVarChar, 50).Value = order.DepositCase.CaseNumber;
                    cmd.Parameters.Add("@contract_number", SqlDbType.Int).Value = order.DepositCase.ContractNumber;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = order.DepositCase.StartDate;
                    cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.DepositCase.EndDate;
                    cmd.Parameters.Add("@servicing_set_number", SqlDbType.Int).Value = order.DepositCase.ServicingSetNumber;

                    cmd.Parameters.Add("@joint_type", SqlDbType.TinyInt).Value = order.DepositCase.JointType;
                    cmd.Parameters.Add("@contract_duration", SqlDbType.TinyInt).Value = order.DepositCase.ContractDuration;
                    if (order.DepositCase.JointType != 0)
                    {
                        cmd.Parameters.Add("@contract_type", SqlDbType.TinyInt).Value = order.DepositCase.ContractType;
                    }
                    if (order.Type == OrderType.DepositCaseActivationOrder)
                    {
                        cmd.Parameters.Add("@conected_account", SqlDbType.VarChar, 20).Value = order.DepositCase.ConnectAccount.AccountNumber;
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.DepositCase.ConnectAccount.Currency;
                    }
                    if (order.Type != OrderType.DepositCaseOrder)
                    {
                        cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.DepositCase.ProductId;
                    }

                    if (order.Type == OrderType.DepositCaseActivationOrder || order.Type == OrderType.DepositCaseOrder)
                    {
                        cmd.Parameters.Add("@recontract_possibility", SqlDbType.Bit).Value = order.DepositCase.RecontractPossibility;
                    }
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    if (order.DepositCase.JointType != 0)
                    {
                        order.DepositCase.JointCustomers.ForEach(m =>
                            Order.SaveOrderJointCustomer(order.Id, m.Key)
                            );
                    }


                    return result;
                }
            }

        }



        /// <summary>
        /// Վերադարձնում է վերջի ազատ պայմանգրի համարը
        /// </summary>
        /// <returns></returns>
        internal static ulong GetContractNumber()
        {
            return Utility.GetLastKeyNumber(71, 22000);

        }

        internal static DepositCaseOrder GetDepositCaseOrder(DepositCaseOrder order)
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
                                             hb.amount
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
                    order.DepositCase = new DepositCase();
                    if (dr["debet_account"] != DBNull.Value)
                        order.DepositCase.ConnectAccount = Account.GetAccount(dr["debet_account"].ToString());
                    order.DepositCase.CaseNumber = dr["case_number"].ToString();
                    order.DepositCase.ContractDuration = Convert.ToInt16(dr["contract_duration"].ToString());
                    order.DepositCase.ContractNumber = Convert.ToUInt64(dr["contract_number"].ToString());
                    if (dr["contract_type"] != DBNull.Value)
                        order.DepositCase.ContractType = Convert.ToInt16(dr["contract_type"].ToString());
                    order.DepositCase.EndDate = Convert.ToDateTime(dr["date_of_normal_end"]);
                    order.DepositCase.JointCustomers = Order.GetOrderJointCustomer(order.Id);
                    order.DepositCase.JointType = Convert.ToInt16(dr["joint_type"].ToString());
                    order.DepositCase.ServicingSetNumber = dr["servicing_set_number"] != DBNull.Value ? Convert.ToInt32(dr["servicing_set_number"]) : default(int);
                    order.DepositCase.StartDate = Convert.ToDateTime(dr["date_of_beginning"]);
                    //if (dr["recontract_possibility"] != DBNull.Value)
                    //    order.DepositCase.RecontractPossibility = Convert.ToBoolean(dr["recontract_possibility"]);
                }


            }
            return order;
        }


        internal static bool HasFilialDepositCase(int filialCode)
        {
            bool hasDepositCase = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = "SELECT ISNULL(deposit_cases, 0)  FROM Name_Fill where Filialcode = @filialCode";

                using SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;

                if (Convert.ToInt32(cmd.ExecuteScalar()) != 0)
                {
                    hasDepositCase = true;
                }

            }

            return hasDepositCase;
        }
    }
}
