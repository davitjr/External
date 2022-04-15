using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class VehicleInsuranceOrderDB
    {
        internal static ActionResult SaveVehicleInsuranceOrder(VehicleInsuranceOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_vehicle_insurance_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    cmd.Parameters.Add("@full_name", SqlDbType.NVarChar, 40).Value = order.FullName;
                    cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar, 40).Value = order.PhoneNumber;
                    cmd.Parameters.Add("@email_address", SqlDbType.NVarChar, 40).Value = order.EmailAddress;
                    cmd.Parameters.Add("@TPIN", SqlDbType.NVarChar, 40).Value = order.ID_TPIN;
                    cmd.Parameters.Add("@contract_account_number", SqlDbType.NVarChar, 40).Value = order.ContractAccountNumber == null ? string.Empty : order.ContractAccountNumber;
                    cmd.Parameters.Add("@bank_name_am", SqlDbType.NVarChar, 40).Value = order.BankNameAM == null ? string.Empty : order.BankNameAM;
                    cmd.Parameters.Add("@bank_name_en", SqlDbType.NVarChar, 40).Value = order.BankNameEN == null ? string.Empty : order.BankNameEN;
                    cmd.Parameters.Add("@bank_id", SqlDbType.Int).Value = order.BankID;
                    cmd.Parameters.Add("@vehicle_mark", SqlDbType.NVarChar, 40).Value = order.VehicleMark;
                    cmd.Parameters.Add("@vehicle_model", SqlDbType.NVarChar, 40).Value = order.VehicleModel;
                    cmd.Parameters.Add("@vehicle_number", SqlDbType.NVarChar, 40).Value = order.VehicleNumber;
                    cmd.Parameters.Add("@horse_power", SqlDbType.SmallInt).Value = order.HorsePower;
                    cmd.Parameters.Add("@bonusmalus", SqlDbType.SmallInt).Value = order.Bonusmalus;
                    cmd.Parameters.Add("@vehicle_use_type_id", SqlDbType.SmallInt).Value = order.VehicleUseTypeID;
                    cmd.Parameters.Add("@vehicle_use_type_en", SqlDbType.NVarChar, 40).Value = order.VehicleUseTypeEN;
                    cmd.Parameters.Add("@vehicle_use_type_am", SqlDbType.NVarChar, 40).Value = order.VehicleUseTypeAM;
                    cmd.Parameters.Add("@duration", SqlDbType.SmallInt).Value = order.Duration;
                    cmd.Parameters.Add("@duration_type", SqlDbType.SmallInt).Value = order.DurationType;
                    cmd.Parameters.Add("@token", SqlDbType.NVarChar, -1).Value = order.Token;


                    cmd.Parameters.Add("@start_date", SqlDbType.DateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.DateTime).Value = order.EndDate;
                    cmd.Parameters.Add("@policy_holder_type_code", SqlDbType.SmallInt).Value = order.PolicyHolderTypeCode;

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

        internal static ActionResult UpdateVehicleInsuranceOrder(VehicleInsuranceOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string UpdateVehicleInsuranceOrderString = @"DECLARE @credit_bank_code   AS NVARCHAR(5)
	                                                             DECLARE @credit_account   AS NVARCHAR(10)

	                                                            SELECT @credit_bank_code=credit_bank_code,@credit_account=credit_account 
                                                                  FROM tbl_vehicle_insurance_company where id_insurance_company=@insurance_company_id;

                                                                UPDATE TBl_Vehicle_Insurance_Order_Details
                                                                set 
                                                                    insurance_company_id = @insurance_company_id,
                                                                    insurance_company_name = @insurance_company_name,
                                                                    acknowledgement_check_box = @acknowledgement_check_box,
                                                                    acknowledgement_text = @acknowledgement_text
                                                                WHERE doc_id=@doc_id;
                                                                UPDATE Tbl_HB_documents
                                                                set 
                                                                    amount = @amount,
                                                                    use_credit_line = @use_credit_line,
                                                                    debet_account = @debet_account,
                                                                    credit_account = @credit_account,
                                                                    credit_bank_code = @credit_bank_code
                                                                WHERE doc_ID=@doc_id;";
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = UpdateVehicleInsuranceOrderString;
                    cmd.CommandType = CommandType.Text;


                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@insurance_company_id", SqlDbType.Int).Value = order.InsuranceCompanyID;
                    cmd.Parameters.Add("@insurance_company_name", SqlDbType.NVarChar, 40).Value = order.InsuranceCompanyName;
                    cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = order.UseCreditLine;
                    cmd.Parameters.Add("@acknowledgement_check_box", SqlDbType.Bit).Value = order.AcknowledgementCheckBox;
                    cmd.Parameters.Add("@acknowledgement_text", SqlDbType.NVarChar, 40).Value = order.AcknowledgementText;
                    cmd.Parameters.Add("@amount", SqlDbType.NVarChar, 40).Value = order.Amount;
                    cmd.Parameters.Add("@debet_account", SqlDbType.NVarChar, 40).Value = order.DebitAccount.AccountNumber;
                    cmd.ExecuteNonQuery();


                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;


                    return result;
                }

            }
        }

        internal static ActionResult UpdateVehicleInsuranceOrderDetails(VehicleInsuranceOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string UpdateVehicleInsuranceOrderString = @"UPDATE TBl_Vehicle_Insurance_Order_Details
                                                                set 
                                                                    completed_in_aswa = @completed_in_aswa
                                                                WHERE doc_id=@doc_id;";
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    if (!string.IsNullOrEmpty(order.ContractNumber) && !string.IsNullOrEmpty(order.Description))
                    {
                        UpdateVehicleInsuranceOrderString = @"UPDATE Tbl_HB_documents
                                                                set 
                                                                    description = @description
                                                                WHERE doc_ID=@doc_id;
                                                               UPDATE TBl_Vehicle_Insurance_Order_Details
                                                                set 
                                                                    completed_in_aswa = @completed_in_aswa,
                                                                    contract_number = @contract_number
                                                                WHERE doc_id=@doc_id;";
                        cmd.Parameters.Add("@contract_number", SqlDbType.NVarChar, 40).Value = order.ContractNumber;
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 40).Value = order.Description;
                    }
                    cmd.CommandText = UpdateVehicleInsuranceOrderString;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@completed_in_aswa", SqlDbType.Bit).Value = order.CompletedInASWA;
                    cmd.ExecuteNonQuery();


                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;


                    return result;
                }

            }
        }

        internal static VehicleInsuranceOrder GetVehicleInsuranceOrder(VehicleInsuranceOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.amount,d.currency,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.debet_account,d.credit_account,d.quality,n.*,d.operation_date,d.order_group_id,d.confirmation_date,d.use_credit_line                                             
		                                           FROM Tbl_HB_documents as d left join TBl_Vehicle_Insurance_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.DebitAccount = Account.GetSystemAccount(dt.Rows[0]["debet_account"].ToString());//  dt.Rows[0]["debet_account"].ToString();
                order.CreditAccount = Account.GetSystemAccount(dt.Rows[0]["credit_account"].ToString());
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Duration = Convert.ToUInt16(dt.Rows[0]["duration"]);
                order.DurationType = Convert.ToUInt16(dt.Rows[0]["duration_type"]);
                order.UseCreditLine = dt.Rows[0]["use_credit_line"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["use_credit_line"].ToString()) : false;

                order.FullName = dt.Rows[0]["full_name"].ToString();
                order.PhoneNumber = dt.Rows[0]["phone_number"].ToString();
                order.EmailAddress = dt.Rows[0]["email_address"].ToString();
                order.ID_TPIN = dt.Rows[0]["TPIN"].ToString();
                order.ContractAccountNumber = dt.Rows[0]["contract_account_number"].ToString();
                order.BankNameAM = dt.Rows[0]["bank_name_am"].ToString();
                order.BankNameEN = dt.Rows[0]["bank_name_en"].ToString();
                order.BankID = int.Parse(dt.Rows[0]["bank_id"].ToString());
                order.VehicleMark = dt.Rows[0]["vehicle_mark"].ToString();
                order.VehicleModel = dt.Rows[0]["vehicle_model"].ToString();
                order.VehicleNumber = dt.Rows[0]["vehicle_number"].ToString();
                order.HorsePower = ushort.Parse(dt.Rows[0]["horse_power"].ToString());
                order.Bonusmalus = ushort.Parse(dt.Rows[0]["bonusmalus"].ToString());
                order.VehicleUseTypeID = short.Parse(dt.Rows[0]["vehicle_use_type_id"].ToString());
                order.VehicleUseTypeAM = dt.Rows[0]["vehicle_use_type_am"].ToString();
                order.VehicleUseTypeEN = dt.Rows[0]["vehicle_use_type_en"].ToString();
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["start_date"].ToString());
                order.EndDate = Convert.ToDateTime(dt.Rows[0]["end_date"].ToString());
                order.InsuranceCompanyName = dt.Rows[0]["insurance_company_name"] != DBNull.Value ? dt.Rows[0]["insurance_company_name"].ToString() : null;
                order.InsuranceCompanyID = dt.Rows[0]["insurance_company_id"] != DBNull.Value ? short.Parse(dt.Rows[0]["insurance_company_id"].ToString()) : (short)0;
                order.PolicyHolderTypeCode = short.Parse(dt.Rows[0]["policy_holder_type_code"].ToString());
                order.ContractNumber = dt.Rows[0]["contract_number"] != DBNull.Value ? dt.Rows[0]["contract_number"].ToString() : null;
                order.AcknowledgementCheckBox = dt.Rows[0]["acknowledgement_check_box"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["acknowledgement_check_box"].ToString()) : false;
                order.CompletedInASWA = dt.Rows[0]["completed_in_aswa"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["completed_in_aswa"].ToString()) : false;
                order.Token = dt.Rows[0]["token"].ToString();

                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);


            }
            return order;
        }

        internal static ActionResult InsertASWAContractResponseDetails(long DocId, bool IsCompleted, short TypeOfFunction, string Description)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand _cmd = new SqlCommand())
                {
                    _con.Open();
                    _cmd.Connection = _con;
                    _cmd.CommandType = CommandType.Text;

                    _cmd.CommandText = "Insert into Tbl_aswa_contract_response_details(doc_id,registration_date,is_completed,type_of_function,description) values(@doc_id, GETDATE(),@is_completed,@type_of_function,@description);";
                    _cmd.Parameters.Add("@doc_id", SqlDbType.BigInt).Value = DocId;
                    _cmd.Parameters.Add("@is_completed", SqlDbType.Bit).Value = IsCompleted;
                    _cmd.Parameters.Add("@type_of_function", SqlDbType.SmallInt).Value = TypeOfFunction;
                    _cmd.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value = Description;

                    _cmd.ExecuteNonQuery();

                    _con.Close();
                }
            }
            result.ResultCode = ResultCode.Normal;

            return result;
        }

        internal static (long, DateTime) ExistsVehicleInsuranceOrder(ulong customerNumber, List<OrderQuality> qualities)
        {
            string qualitiesCondition = "";
            (long, DateTime) result = (0, DateTime.MinValue);
            if (qualities.Count > 0)
            {
                qualitiesCondition = " and quality in (";


                foreach (OrderQuality quality in qualities)
                {
                    qualitiesCondition += (short)quality + ",";
                }

                qualitiesCondition = qualitiesCondition.Substring(0, qualitiesCondition.Length - 1);
                qualitiesCondition += ") ";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT doc_id, registration_date
		                                           FROM Tbl_HB_documents                                            
                                                   WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end and document_type = 259 " + qualitiesCondition, conn);
                {

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Item1 = Convert.ToInt64(dr["doc_id"].ToString());
                            result.Item2 = Convert.ToDateTime(dr["registration_date"]);
                        }
                    }
                }
            }
            return result;
        }

        internal static ActionResult SaveAswaSearchResponseDetails(VehicleInsuranceOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "Insert into Tbl_aswa_search_response_details" +
                                            "(reg_date,vehicle_mark,vehicle_model,vehicle_number,horse_power,start_date,policy_holder_id,full_name,TPIN,policy_holder_type_code,token) " +
                                      "values(GETDATE(),@vehicle_mark,@vehicle_model,@vehicle_number,@horse_power,@start_date,@policy_holder_id,@full_name,@TPIN,@policy_holder_type_code,@token);" +
                                      "SELECT SCOPE_IDENTITY();";

                    cmd.Parameters.Add("@vehicle_mark", SqlDbType.NVarChar, 40).Value = order.VehicleMark;
                    cmd.Parameters.Add("@vehicle_model", SqlDbType.NVarChar, 40).Value = order.VehicleModel;
                    cmd.Parameters.Add("@vehicle_number", SqlDbType.NVarChar, 40).Value = order.VehicleNumber;
                    cmd.Parameters.Add("@horse_power", SqlDbType.SmallInt).Value = order.HorsePower;
                    cmd.Parameters.Add("@start_date", SqlDbType.DateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@policy_holder_id", SqlDbType.Int).Value = order.PolicyHolderID;
                    cmd.Parameters.Add("@full_name", SqlDbType.NVarChar, 40).Value = order.FullName;
                    cmd.Parameters.Add("@TPIN", SqlDbType.NVarChar, 40).Value = order.ID_TPIN;
                    cmd.Parameters.Add("@policy_holder_type_code", SqlDbType.SmallInt).Value = order.PolicyHolderTypeCode;
                    cmd.Parameters.Add("@token", SqlDbType.NVarChar, -1).Value = order.Token;



                    int returnValue = -1;
                    object returnObj = cmd.ExecuteScalar();

                    if (returnObj != null)
                    {
                        int.TryParse(returnObj.ToString(), out returnValue);
                        result.Id = returnValue;
                    }

                    result.ResultCode = ResultCode.Normal;
                    return result;
                }

            }
        }

        internal static VehicleInsuranceOrder GetAswaSearchResponseDetails(VehicleInsuranceOrder order, long Id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT * FROM Tbl_aswa_search_response_details                                             
                                                   WHERE Id=@Id", conn);

                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = Id;
                dt.Load(cmd.ExecuteReader());

                order.VehicleMark = dt.Rows[0]["vehicle_mark"].ToString();
                order.VehicleModel = dt.Rows[0]["vehicle_model"].ToString();
                order.VehicleNumber = dt.Rows[0]["vehicle_number"].ToString();
                order.HorsePower = ushort.Parse(dt.Rows[0]["horse_power"].ToString());
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["start_date"].ToString());
                order.PolicyHolderID= int.Parse(dt.Rows[0]["policy_holder_id"].ToString());
                order.FullName = dt.Rows[0]["full_name"].ToString();
                order.ID_TPIN = dt.Rows[0]["TPIN"].ToString();
                order.PolicyHolderTypeCode= short.Parse(dt.Rows[0]["policy_holder_type_code"].ToString());
                order.Token= dt.Rows[0]["token"].ToString();


            }
            return order;

        }

    }
}
