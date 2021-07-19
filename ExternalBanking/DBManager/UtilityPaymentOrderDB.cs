using ExternalBanking.UtilityPaymentsManagment;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public static class UtilityPaymentOrderDB
    {
        public static KeyValuePair<ActionResult, List<string>> GetCommunalSearchedName(CommunalTypes communalType, bool isUtilityPayment, int abonentType, string code, string branch, bool getAmount = false)
        {
            ActionResult actionResult = new ActionResult();
            List<string> resultList = new List<string>();

            ActionError actionError = new ActionError();
            actionResult.Errors.Add(actionError);
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString();
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "sp_getCommunalSearchedName";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@transfer_type", SqlDbType.SmallInt)).Value = communalType;
                    command.Parameters.Add(new SqlParameter("@is_UtilityPayment", SqlDbType.Bit)).Value = isUtilityPayment;
                    command.Parameters.Add(new SqlParameter("@fiz_jur", SqlDbType.SmallInt)).Value = abonentType;
                    command.Parameters.Add(new SqlParameter("@code", SqlDbType.NVarChar, 50)).Value = code;
                    command.Parameters.Add(new SqlParameter("@branch", SqlDbType.NVarChar, 5)).Value = branch;

                    SqlParameter resultParameter = command.Parameters.Add(new SqlParameter("@result", SqlDbType.Int));
                    resultParameter.Direction = ParameterDirection.Output;

                    SqlParameter varParameter = command.Parameters.Add(new SqlParameter("@var", SqlDbType.NVarChar, 30));
                    varParameter.Direction = ParameterDirection.Output;

                    using (SqlDataReader dataReader = command.ExecuteReader())
                        dataTable.Load(dataReader);

                    short result = Convert.ToInt16(resultParameter.Value);

                    if (result == 0)
                        actionError.Code = 1;
                    else
                        actionError.Code = result;

                    actionError.Params = new string[] { varParameter.Value.ToString() };
                }
            }

            if (actionError.Code == 1)
                if (dataTable.Rows.Count != 0)
                {
                    resultList.Add(dataTable.Rows[0]["searched_name"].ToString());
                    resultList.Add(dataTable.Rows[0]["branch"].ToString());

                    if (getAmount)
                    {
                        if (communalType == CommunalTypes.Gas)
                        {
                            resultList.Add(dataTable.Rows[0]["debtGas"].ToString());
                            resultList.Add(dataTable.Rows[0]["debtService"].ToString());
                        }
                        else
                            resultList.Add(dataTable.Rows[0]["debt"].ToString());
                    }
                    else
                    {
                        if (communalType == CommunalTypes.VivaCell && isUtilityPayment)
                            resultList.Add(dataTable.Rows[0]["maxAmount"].ToString());
                        else if (communalType == CommunalTypes.Orange && !isUtilityPayment)
                            resultList.Add(dataTable.Rows[0]["PayMethod"].ToString());
                    }
                }
                else
                    actionError.Code = 316;

            return new KeyValuePair<ActionResult, List<string>>(actionResult, resultList);
        }

        internal static ActionResult SaveUtilityPaymentOrder(UtilityPaymentOrder utilityPaymentOrder, string userName, SourceType source)
        {
            ActionResult acctionResult = new ActionResult();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    //Attempt to do stuff in the database
                    //potentially throw an exception
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "sp_addNewUtilityPayment";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Transaction = transaction;
                        if (utilityPaymentOrder.Branch == null)
                        {
                            utilityPaymentOrder.Branch = "";
                        }

                        if (utilityPaymentOrder.Id != 0)
                            command.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = utilityPaymentOrder.Id;

                        command.Parameters.Add("@doc_type", SqlDbType.Int).Value = utilityPaymentOrder.Type;
                        command.Parameters.Add("@customer_number", SqlDbType.Float).Value = utilityPaymentOrder.CustomerNumber;
                        command.Parameters.Add("@doc_number", SqlDbType.VarChar, 20).Value = utilityPaymentOrder.OrderNumber;
                        command.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = utilityPaymentOrder.RegistrationDate;
                        command.Parameters.Add("@currency", SqlDbType.VarChar, 50).Value = utilityPaymentOrder.Currency;
                        command.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = utilityPaymentOrder.DebitAccount.AccountNumber;
                        command.Parameters.Add("@amount", SqlDbType.Float).Value = utilityPaymentOrder.Amount;
                        command.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = utilityPaymentOrder.Description;
                        command.Parameters.Add("@service_amount", SqlDbType.Float).Value = utilityPaymentOrder.ServiceAmount;
                        command.Parameters.Add("@cod", SqlDbType.VarChar, 50).Value = utilityPaymentOrder.Code;
                        command.Parameters.Add("@comunal_type", SqlDbType.Int).Value = (int)utilityPaymentOrder.CommunalType;
                        command.Parameters.Add("@branch", SqlDbType.VarChar, 50).Value = utilityPaymentOrder.Branch;
                        command.Parameters.Add("@abonent_type", SqlDbType.TinyInt).Value = utilityPaymentOrder.AbonentType;
                        command.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                        command.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                        command.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = utilityPaymentOrder.FilialCode;
                        command.Parameters.Add("@credit_account", SqlDbType.VarChar, 15).Value = utilityPaymentOrder.ReceiverAccount.AccountNumber;
                        command.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = utilityPaymentOrder.OperationDate;
                        command.Parameters.Add("@isPrepaid", SqlDbType.Bit).Value = utilityPaymentOrder.PrepaidSign;
                        command.Parameters.Add("@useCreditLine", SqlDbType.Bit).Value = utilityPaymentOrder.UseCreditLine;
                        if (utilityPaymentOrder.GroupId != 0)
                        {
                            command.Parameters.Add("@group_id", SqlDbType.Int).Value = utilityPaymentOrder.GroupId;
                        }

                        if (utilityPaymentOrder.CommunalType == CommunalTypes.COWater)
                        {
                            command.Parameters.Add("@service_provided_filialcode", SqlDbType.SmallInt).Value = utilityPaymentOrder.AbonentFilialCode;
                            command.Parameters.Add("@payment_type", SqlDbType.SmallInt).Value = utilityPaymentOrder.PaymentType;
                        }

                        if (utilityPaymentOrder.AbonentType == 1 && utilityPaymentOrder.CommunalType == CommunalTypes.Gas)
                        {
                            List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();

                            SearchCommunal cmnl = new SearchCommunal();
                            cmnl.AbonentNumber = utilityPaymentOrder.Code.ToString();
                            cmnl.Branch = utilityPaymentOrder.Branch;
                            gazList = GasPromAbonentSearch.GasPromSearchOutput(cmnl);

                            command.Parameters.Add("@name", SqlDbType.NVarChar, 200).Value = Utility.ConvertAnsiToUnicode(gazList[0].Name);
                            command.Parameters.Add("@last_name", SqlDbType.NVarChar, 200).Value = Utility.ConvertAnsiToUnicode(gazList[0].LastName);
                            command.Parameters.Add("@street", SqlDbType.NVarChar, 200).Value = Utility.ConvertAnsiToUnicode(gazList[0].Street);
                            command.Parameters.Add("@house", SqlDbType.NVarChar, 200).Value = Utility.ConvertAnsiToUnicode(gazList[0].House);
                            command.Parameters.Add("@home", SqlDbType.NVarChar, 200).Value = Utility.ConvertAnsiToUnicode(gazList[0].Home);
                            command.Parameters.Add("@phone_number", SqlDbType.NVarChar, 200).Value = gazList[0].PhoneNumber;

                            command.Parameters.Add("@gas_debt_at_beginning_of_month", SqlDbType.Float).Value = gazList[0].GasDebtAtBeginningOfMonth;
                            command.Parameters.Add("@debt_at_end_of_month", SqlDbType.Float).Value = gazList[0].DebtAtEndOfMonth;
                            command.Parameters.Add("@gas_previous_payment", SqlDbType.Float).Value = gazList[0].GasPreviousPayment;
                            command.Parameters.Add("@meter_previous_testimony", SqlDbType.Float).Value = gazList[0].MeterPreviousTestimony;
                            command.Parameters.Add("@gas_expense_by_volume", SqlDbType.Float).Value = gazList[0].GasExpenseByVolume;
                            command.Parameters.Add("@gas_expense_by_amount", SqlDbType.Float).Value = gazList[0].GasExpenseByAmount;
                            command.Parameters.Add("@meter_last_testimony", SqlDbType.Float).Value = gazList[0].MeterLastTestimony;
                            command.Parameters.Add("@paid_amount_in_current_month", SqlDbType.Float).Value = gazList[0].PaidAmountInCurrentMonth;
                            command.Parameters.Add("@service_fee_debt_at_beginning_of_month", SqlDbType.Float).Value = gazList[0].ServiceFeeDebtAtBeginningOfMonth;
                            command.Parameters.Add("@service_fee_previous_payment", SqlDbType.Float).Value = gazList[0].ServiceFeePreviousPayment;
                            command.Parameters.Add("@service_fee_by_amount", SqlDbType.Float).Value = gazList[0].ServiceFeeByAmount;
                            command.Parameters.Add("@service_fee_at_end_of_month", SqlDbType.Float).Value = gazList[0].ServiceFeeAtEndOfMonth;
                            command.Parameters.Add("@penalty", SqlDbType.Float).Value = gazList[0].Penalty;
                            command.Parameters.Add("@violation_by_volume", SqlDbType.Float).Value = gazList[0].ViolationByVolume;
                            command.Parameters.Add("@violation_by_amount", SqlDbType.Float).Value = gazList[0].ViolationByAmount;
                            command.Parameters.Add("@tariff", SqlDbType.Float).Value = gazList[0].Tariff;
                            command.Parameters.Add("@expense_by_volume_for_same_month_previous_year", SqlDbType.Float).Value = gazList[0].ExpenseByVolumeForSameMonthPreviousYear;
                            command.Parameters.Add("@debt_date", SqlDbType.DateTime).Value = gazList[0].DebtDate;
                            command.Parameters.Add("@current_gas_debt", SqlDbType.Float).Value = gazList[0].CurrentGasDebt;
                            command.Parameters.Add("@current_service_fee_debt", SqlDbType.Float).Value = gazList[0].CurrentServiceFeeDebt;
                        }

                        SqlParameter msg = new SqlParameter("@msg", SqlDbType.VarChar, 4000);
                        msg.Direction = ParameterDirection.Output;
                        command.Parameters.Add(msg);

                        SqlParameter result = new SqlParameter("@result", SqlDbType.Int);
                        result.Direction = ParameterDirection.Output;
                        command.Parameters.Add(result);

                        SqlParameter id = new SqlParameter("@id", SqlDbType.Int);
                        id.Direction = ParameterDirection.Output;
                        command.Parameters.Add(id);

                        command.ExecuteNonQuery();

                        utilityPaymentOrder.Id = id.Value != DBNull.Value && Convert.ToInt64(id.Value) != -1 ? Convert.ToInt64(id.Value) : 0;


                        int resultCode = Convert.ToInt16(result.Value);
                        if (resultCode == 9 || resultCode == 10)
                        {
                            acctionResult.ResultCode = ResultCode.Normal;
                            acctionResult.Id = (long)utilityPaymentOrder.Id;
                        }
                        else
                        {
                            ActionError actionError = new ActionError(Convert.ToInt16(result.Value), new string[] { msg.ToString() });
                            acctionResult.Errors.Add(actionError);
                            acctionResult.ResultCode = ResultCode.Failed;
                        }
                    }
                    if (source == SourceType.SSTerminal || source == SourceType.CashInTerminal)
                    {
                        OrderDB.SaveOrderDetails(utilityPaymentOrder);
                    }
                    transaction.Commit();
                }
                return acctionResult;
            }

        }

        internal static UtilityPaymentOrder GetUtilityPaymentOrder(UtilityPaymentOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT hb.*,u.*,hs.change_date FROM Tbl_Hb_Documents hb INNER JOIN Tbl_HB_UtilityPayments u ON hb.doc_ID = u.docID left join Tbl_HB_quality_history hs on hb.doc_ID=hs.Doc_ID and hs.quality=3 where hb.doc_id=@id and hb.customer_number=case when @customerNumber = 0 then hb.customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            order.CommunalType = (CommunalTypes)Convert.ToUInt32(dr["comunal_type"]);

                            if (dr["debet_account"] != DBNull.Value && order.Type != OrderType.CashCommunalPayment && order.Type != OrderType.ReestrCashCommunalPayment)
                            {
                                order.DebitAccount = Account.GetAccount(dr["debet_account"].ToString());
                                order.DebitAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.DebitAccount.AccountTypeDescription);
                            }
                            else
                            {
                                order.DebitAccount = Account.GetSystemAccount(dr["debet_account"].ToString());
                            }
                            if (order.DebitAccount != null)
                            {
                                if (order.DebitAccount.IsIPayAccount())
                                {
                                    order.DebitAccount.IsAttachedCard = true;
                                    order.DebitAccount.BindingId = Account.GetAttachedCardBindingId(order.Id);
                                    order.DebitAccount.AttachedCardNumber = Account.GetAttachedCardNumber(order.Id);
                                }
                            }

                            if (dr["credit_account"] != DBNull.Value)
                            {
                                order.ReceiverAccount = Account.GetSystemAccount(dr["credit_account"].ToString());
                            }


                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();

                            order.Code = dr["cod"].ToString();
                            if (dr["abonent_type"] != DBNull.Value)
                                order.AbonentType = int.Parse(dr["abonent_type"].ToString());
                            if (dr["service_amount"] != DBNull.Value)
                                order.ServiceAmount = double.Parse(dr["service_amount"].ToString());
                            order.Branch = dr["branch"].ToString();

                            order.SubType = Convert.ToByte(dr["document_subtype"]);

                            order.OrderNumber = dr["document_number"].ToString();

                            if (dr["service_provided_filialcode"] != DBNull.Value)
                                order.AbonentFilialCode = ushort.Parse(dr["service_provided_filialcode"].ToString());

                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                            if (dr["change_date"] != DBNull.Value)
                                order.PaymentTime = Convert.ToDateTime(dr["change_date"].ToString());
                            else
                                order.PaymentTime = Convert.ToDateTime(dr["registration_date"]);

                            if(dr["use_credit_line"] != DBNull.Value)
                            {
                                order.UseCreditLine = Convert.ToBoolean(dr["use_credit_line"]);
                            }
                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;

                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                        }
                        else
                        {
                            order = null;
                        }
                    }
                }
            }
            return order;
        }

        internal static ActionResult SaveUPOrderPaymentDetails(UtilityPaymentOrder utilityPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_payment_UP_details(order_id, abonent_code, abonent_branch, abonent_type) VALUES(@orderId, @abonentCode, @abonentBranch, @abonentType)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@abonentCode", SqlDbType.NVarChar, 50).Value = utilityPaymentOrder.Code;
                    cmd.Parameters.Add("@abonentBranch", SqlDbType.NVarChar, 50).Value = utilityPaymentOrder.Branch;
                    cmd.Parameters.Add("@abonentType", SqlDbType.SmallInt).Value = utilityPaymentOrder.AbonentType;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }



        internal static double GetCOWaterPaymentAmount(string abonentNumber, string branchCode, ushort paymentType)
        {
            DataTable dt = new DataTable();
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT  WaterV AS WaterEnd,
                                                AndamV AS AndamEnd
                                                FROM Tbl_WaterCo_Main 
                                                WHERE  Kod = @abonentNumber AND FilialCode=@branchCode";
                    cmd.Parameters.Add("@abonentNumber", SqlDbType.Int).Value = abonentNumber;
                    cmd.Parameters.Add("@branchCode", SqlDbType.Int).Value = branchCode;
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count == 1)
                    {
                        if (paymentType == 1)
                        {
                            amount = Math.Abs(Convert.ToDouble(dt.Rows[0]["AndamEnd"]));
                        }
                        else if (paymentType == 2)
                        {
                            amount = Math.Abs(Convert.ToDouble(dt.Rows[0]["WaterEnd"]));
                        }
                    }

                }
            }
            return amount;
        }


        internal static bool CheckCOWaterAbonent(string abonentNumber, string branchCode)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["UtilityPaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT  WaterV AS WaterEnd,
                                                AndamV AS AndamEnd
                                                FROM Tbl_WaterCo_Main 
                                                WHERE  Kod = @abonentNumber AND FilialCode=@branchCode";
                    cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar).Value = abonentNumber;
                    cmd.Parameters.Add("@branchCode", SqlDbType.NVarChar).Value = branchCode;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        check = true;
                    }

                }
            }
            return check;
        }

        public static void SaveReestrUtilityPaymentOrderDetails(ReestrUtilityPaymentOrder order)
        {
            order.COWaterReestrDetails.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.Tbl_COWater_Reestr_Details
                                                             (Order_Number, Doc_Id, Abonent_Number, City,AAH,Total_Charge, Water_Payment,Membership_Fee,File_Name)
                                                             VALUES
                                                             (@Order_Number,@Doc_Id, @Abonent_Number,  @City,@AAH,@Total_Charge, @Water_Payment,@Membership_Fee,@File_Name)",
                                                       conn))
                    {

                        cmd.Parameters.Add("@Order_Number", SqlDbType.Int).Value = m.OrderNumber;
                        cmd.Parameters.Add("@Doc_Id", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@Abonent_Number", SqlDbType.NVarChar, 10).Value = m.AbonentNumber;

                        cmd.Parameters.Add("@City", SqlDbType.NVarChar, 50).Value = Utility.ConvertUnicodeToAnsi(m.City);
                        cmd.Parameters.Add("@AAH", SqlDbType.NVarChar, 100).Value = Utility.ConvertUnicodeToAnsi(m.FullName);
                        cmd.Parameters.Add("@Total_Charge", SqlDbType.Money).Value = m.TotalCharge;
                        cmd.Parameters.Add("@Water_Payment", SqlDbType.Money).Value = m.WaterPayment;
                        cmd.Parameters.Add("@Membership_Fee", SqlDbType.Money).Value = m.MembershipFee;
                        cmd.Parameters.Add("@File_Name", SqlDbType.NVarChar, 70).Value = m.FileName;
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        internal static ActionResult SaveReestrUtilityPaymentOrder(ReestrUtilityPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"  declare @filial as int
                                                    declare @DocID as int
                                                    if @customer_number<>0
                                                    begin
                                                       select @filial=filialcode from dbo.Tbl_customers where customer_number=@customer_number
                                                    end
                                                    else
                                                    begin 
                                                       set @filial=0
                                                    end    
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    quality,source_type,operationFilialCode,operation_date)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,1,@source_type,@operationFilialCode,@oper_day)
                                                    Select @DocID= Scope_identity()
                                                    INSERT INTO Tbl_HB_UtilityPayments(docID,branch,service_provided_filialcode,comunal_type)
                                                    VALUES
                                                    (@DocID,@branch,@service_provided_filialcode,@comunal_type)
                                                    Select @DocID as ID
                                                     ", conn);
                cmd.CommandType = CommandType.Text;


                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@comunal_type", SqlDbType.Int).Value = (int)order.CommunalType;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = order.ReceiverAccount.AccountNumber;
                cmd.Parameters.Add("@branch", SqlDbType.NVarChar).Value = order.Branch;
                cmd.Parameters.Add("@service_provided_filialcode", SqlDbType.NVarChar).Value = order.AbonentFilialCode;
               

                order.Id = Convert.ToInt64(cmd.ExecuteScalar());
                result.ResultCode = ResultCode.Normal;
                return result;
            }

        }



        internal static UtilityPaymentOrder GetReestrUtilityPaymentOrderDetails(ReestrUtilityPaymentOrder order)
        {
            order.COWaterReestrDetails = new List<COWaterReestrDetails>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from Tbl_COWater_Reestr_Details where Doc_Id=@Doc_id   ";
                    cmd.Parameters.Add("@Doc_id", SqlDbType.Int).Value = order.Id;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dr);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            COWaterReestrDetails details = new COWaterReestrDetails();
                            details.AbonentNumber = dt.Rows[i]["Abonent_Number"].ToString();
                            details.City = Utility.ConvertAnsiToUnicode(dt.Rows[i]["City"].ToString());
                            details.FullName = Utility.ConvertAnsiToUnicode(dt.Rows[i]["AAH"].ToString());
                            details.OrderNumber = Convert.ToInt32(dt.Rows[i]["Order_Number"].ToString());
                            details.MembershipFee = Convert.ToDouble(dt.Rows[i]["Membership_Fee"].ToString());
                            details.TotalCharge = Convert.ToDouble(dt.Rows[i]["Total_Charge"].ToString());
                            details.WaterPayment = Convert.ToDouble(dt.Rows[i]["Water_Payment"].ToString());
                            order.COWaterReestrDetails.Add(details);
                        }
                    }
                }
            }
            return order;
        }

        internal static int GetOrderAbonentType(long orderId)
        {
            DataTable dt = new DataTable();
            int abonentType = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT  abonent_type FROM Tbl_HB_UtilityPayments
                                        WHERE  docId = @docId";

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count == 1)
                    {
                        if (dt.Rows[0]["abonent_type"] != DBNull.Value)
                            abonentType = int.Parse(dt.Rows[0]["abonent_type"].ToString());
                    }

                }
            }
            return abonentType;
        }

        internal static double? GetGasServiceFeeAmount(long orderId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT service_amount FROM Tbl_HB_UtilityPayments
                                        WHERE comunal_type = 4 AND docid = @doc_id";
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count == 1)
                    {
                        return double.Parse(dt.Rows[0]["service_amount"].ToString());
                    }
                }
            }
            return null;
        }
    }
}
