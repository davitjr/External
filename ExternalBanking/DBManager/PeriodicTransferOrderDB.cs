using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    public class PeriodicTransferOrderDB
    {
        /// <summary>
        /// Կոմունալ պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPeriodicDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = (short)order.UtilityPaymentOrder.CommunalType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    if ((order.ServicePaymentType != 0 && order.UtilityPaymentOrder.CommunalType != CommunalTypes.Gas) || (order.ServicePaymentType != -1 && order.UtilityPaymentOrder.CommunalType == CommunalTypes.Gas))
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = order.ServicePaymentType;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = 0;
                    }
                    if (order.LastOperationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.LastOperationDate.Value;
                    }
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.SmallDateTime).Value = order.FirstTransferDate;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = order.MinAmountLevel;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = order.MaxAmountLevel;
                    cmd.Parameters.Add("@minimal_rest", SqlDbType.Float).Value = order.MinDebetAccountRest;
                    cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@period", SqlDbType.VarChar, 50).Value = order.Periodicity;
                    cmd.Parameters.Add("@check_days_count", SqlDbType.Int).Value = order.CheckDaysCount;
                    if (order.UtilityPaymentOrder.CommunalType == CommunalTypes.ArmWater || order.UtilityPaymentOrder.CommunalType == CommunalTypes.ENA || order.UtilityPaymentOrder.CommunalType == CommunalTypes.Gas || order.UtilityPaymentOrder.CommunalType == CommunalTypes.YerWater)
                    {
                        order.UtilityPaymentOrder.Description = Regex.Replace(order.UtilityPaymentOrder.Description, @"\d", "");
                    }

                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.UtilityPaymentOrder.Description;
                    cmd.Parameters.Add("@FindField1", SqlDbType.VarChar, 50).Value = order.UtilityPaymentOrder.Code.Trim();
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.Int).Value = order.PayIfNoDebt;
                    if (order.PartialPaymentSign != 0)
                    {
                        cmd.Parameters.Add("@Partial_Payments", SqlDbType.TinyInt).Value = order.PartialPaymentSign;
                    }
                    if (order.UtilityPaymentOrder.AbonentType != 0)
                    {
                        cmd.Parameters.Add("@abonent_type", SqlDbType.Int).Value = order.UtilityPaymentOrder.AbonentType;
                    }
                    if (order.UtilityPaymentOrder.CommunalType == CommunalTypes.VivaCell || order.UtilityPaymentOrder.CommunalType == CommunalTypes.UCom)
                    {
                        cmd.Parameters.Add("@FindField2", SqlDbType.NVarChar, 50).Value = order.UtilityPaymentOrder.Description;
                    }
                    else
                    {
                        cmd.Parameters.Add("@FindField2", SqlDbType.VarChar, 50).Value = order.UtilityPaymentOrder.Branch;
                    }
                    if (order.AllDebt == true)
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 1;
                    }
                    else
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = 0;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

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
                    else if (actionResult == 0 || actionResult == 8 || actionResult == 229 || actionResult == 227 || actionResult == 230)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }

        }
        /// <summary>
        /// Սեփական հաշիվների միջև և ՀՀ տարածքում պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPeriodicDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = order.PeriodicType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.PaymentOrder.Currency;
                    if (order.ServicePaymentType != 0)
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = order.ServicePaymentType;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = 0;
                    }
                    if (order.LastOperationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.LastOperationDate.Value;
                    }
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.SmallDateTime).Value = order.FirstTransferDate;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.PaymentOrder.Amount;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = order.MinAmountLevel;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = order.MaxAmountLevel;
                    cmd.Parameters.Add("@minimal_rest", SqlDbType.Float).Value = order.MinDebetAccountRest;
                    cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = order.DebitAccount.AccountNumber;
                    if (order.PaymentOrder.ReceiverAccount != null)
                    {
                        cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.PaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }
                    if (order.PaymentOrder.ReceiverBankCode != 0)
                    {
                        cmd.Parameters.Add("@credit_bank_code", SqlDbType.Int).Value = order.PaymentOrder.ReceiverBankCode;
                    }
                    if (!string.IsNullOrEmpty(order.PaymentOrder.Receiver))
                    {
                        cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 50).Value = order.PaymentOrder.Receiver;
                    }
                    cmd.Parameters.Add("@period", SqlDbType.VarChar, 50).Value = order.Periodicity;
                    cmd.Parameters.Add("@check_days_count", SqlDbType.Int).Value = order.CheckDaysCount;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.PeriodicDescription;
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.Int).Value = order.PayIfNoDebt;
                    if (order.PartialPaymentSign != 0)
                    {
                        cmd.Parameters.Add("@Partial_Payments", SqlDbType.TinyInt).Value = order.PartialPaymentSign;
                    }
                    if (order.ChargeType == 2)
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 1;
                    }
                    else
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 0;

                    if (order.FeeAccount != null)
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    }
                    else
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = -1;
                    }
                    if (order.Fee != 0)
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.Fee;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = 0;
                    }
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

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
                    else if (actionResult == 0 || actionResult == 8 || actionResult == 229 || actionResult == 227 || actionResult == 230)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }

        }
        /// <summary>
        /// Բյուջե պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPeriodicDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = order.PeriodicType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.BudgetPaymentOrder.Currency;
                    if (order.ServicePaymentType != 0)
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = order.ServicePaymentType;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = 0;
                    }
                    if (order.LastOperationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.LastOperationDate.Value;
                    }
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.SmallDateTime).Value = order.FirstTransferDate.Date;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.BudgetPaymentOrder.Amount;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = order.MinAmountLevel;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = order.MaxAmountLevel;
                    cmd.Parameters.Add("@minimal_rest", SqlDbType.Float).Value = order.MinDebetAccountRest;
                    cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = order.DebitAccount.AccountNumber;
                    if (order.BudgetPaymentOrder.ReceiverAccount != null)
                    {
                        cmd.Parameters.Add("@credit_account", SqlDbType.VarChar, 50).Value = order.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }
                    if (order.BudgetPaymentOrder.ReceiverBankCode != 0)
                    {
                        cmd.Parameters.Add("@credit_bank_code", SqlDbType.Int).Value = order.BudgetPaymentOrder.ReceiverBankCode;
                    }
                    if (!string.IsNullOrEmpty(order.BudgetPaymentOrder.Receiver))
                    {
                        cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 50).Value = order.BudgetPaymentOrder.Receiver;
                    }
                    cmd.Parameters.Add("@period", SqlDbType.VarChar, 50).Value = order.Periodicity;
                    cmd.Parameters.Add("@check_days_count", SqlDbType.Int).Value = order.CheckDaysCount;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.PeriodicDescription;
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.Int).Value = order.PayIfNoDebt;
                    if (order.PartialPaymentSign != 0)
                    {
                        cmd.Parameters.Add("@Partial_Payments", SqlDbType.TinyInt).Value = order.PartialPaymentSign;
                    }
                    if (order.ChargeType == 2)
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 1;
                    }
                    else
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 0;

                    if (order.FeeAccount != null)
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    }
                    else
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = -1;
                    }
                    if (order.BudgetPaymentOrder.PoliceCode != 0)
                    {
                        cmd.Parameters.Add("@police_code", SqlDbType.Int).Value = order.BudgetPaymentOrder.PoliceCode;
                    }
                    if (order.BudgetPaymentOrder.LTACode != 0)
                    {
                        cmd.Parameters.Add("@reg_code", SqlDbType.Int).Value = order.BudgetPaymentOrder.LTACode;
                    }
                    if (order.Fee != 0)
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.Fee;
                    }
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

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
                    else if (actionResult == 0 || actionResult == 8 || actionResult == 229 || actionResult == 227 || actionResult == 230)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }

        }


        /// <summary>
        /// Ստուգում է  գոյություն ունի գործող պարբերական թե ոչ
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="amountType"></param>
        /// <param name="code"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public static bool IsAlreadyExistsThisCommunal(int transferType, int amountType, string code, string branch)
        {
            bool check = false;
            string gasCond = "";

            if (transferType == 7 || transferType == 8 || transferType == 10)
            {
                transferType = 12;
            }
            else if (transferType == 4)
            {
                gasCond = " AND isnull(amount_type,0) in (1,2)";
            }
            if (branch == "-1")
            {
                branch = "";
            }
            if (amountType == -1)
            {
                amountType = 0;
            }
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"Select FilialCode, NN, FindField1, FindField2 From Tbl_operations_by_period
                                                             Where Quality = 1 AND isnull(FindField1,' ' ) =@code
                                                             AND Transfer_Type =@transferType" + gasCond, conn);
                cmd.Parameters.Add("@code", SqlDbType.VarChar, 50).Value = code;
                cmd.Parameters.Add("@transferType", SqlDbType.Int).Value = transferType;
                if (cmd.ExecuteScalar() != null)
                {
                    check = true;
                }
            }

            return check;
        }
        /// <summary>
        ///  Ստուգում է կա պարբերականի ուղարկված բայց չհաստատված հայտ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="transferType"></param>
        /// <param name="amountType"></param>
        /// <param name="code"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public static bool IsAlreadyExistsCommunalTransfersHBDocument(ulong customerNumber, int transferType, int amountType, string code, string branch)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT count(*) FROM Tbl_HB_documents HB left join Tbl_HB_Operations_by_period OD on OD.docID = HB.doc_ID
                                                   where customer_number = @customer_number  and document_type = 10 and document_subtype = 2 and
                                                   isnull(FindField1,' ' ) = @code 
                                                   and ISNULL(OD.FindField2,'') = case when Transfer_Type < 6 then @branch else ISNULL(OD.FindField2,'') end 
                                                   and Quality = 3  AND Transfer_Type = @transfer_type  AND isnull(amount_type,0) =@amount_type", conn);
                cmd.Parameters.Add("@code", SqlDbType.VarChar, 50).Value = code;
                cmd.Parameters.Add("@transfer_type", SqlDbType.Int).Value = transferType;

                if (branch == null)
                {
                    branch = "";
                }

                cmd.Parameters.Add("@branch", SqlDbType.VarChar, 50).Value = branch;
                cmd.Parameters.Add("@amount_type", SqlDbType.Int).Value = amountType;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                if ((int)cmd.ExecuteScalar() >= 1)
                {
                    check = true;
                }

            }

            return check;

        }
        /// <summary>
        /// Սեփական հաշիվների միջև և ՀՀ տարածքում պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PeriodicPaymentOrder GetPeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            order.PaymentOrder = new PaymentOrder();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT
                                                        date_of_beginning,
                                                        d.document_number,
                                                        d.document_type,
                                                        d.document_subtype,
                                                        d.registration_date,
                                                        o.transfer_type,
                                                        d.currency,
                                                        o.date_of_normal_end,
                                                        o.amount_type,
                                                        o.first_repayment_date,
                                                        d.amount,
                                                        o.min_amount,
                                                        o.max_amount,
                                                        o.minimal_rest,
                                                        d.debet_account,
                                                        d.credit_account,
                                                        d.credit_bank_code,
                                                        d.receiver_name,
                                                        o.period,
                                                        o.check_days_count,
                                                        d.description,
                                                        o.PayIfNoDebt,
                                                        o.Partial_Payments,
                                                        o.total_rest,
                                                        d.deb_for_transfer_payment,
                                                        d.amount_for_payment,
                                                        d.quality,
                                                        d.source_type,
                                                        d.operation_date,
                                                        d.order_group_id,
                                                        d.confirmation_date
                                                        FROM Tbl_HB_documents d INNER JOIN Tbl_HB_Operations_by_period o ON d.doc_ID=o.docID
                                                        WHERE d.doc_ID=@DocID  and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                order.PaymentOrder.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.FirstTransferDate = Convert.ToDateTime(dt.Rows[0]["first_repayment_date"]);
                order.PeriodicType = Convert.ToInt32(dt.Rows[0]["transfer_type"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                if (dt.Rows[0]["date_of_normal_end"].ToString() != "")
                {
                    order.LastOperationDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end"]);
                }
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.MinAmountLevel = Convert.ToDouble(dt.Rows[0]["min_amount"]);
                order.MaxAmountLevel = Convert.ToDouble(dt.Rows[0]["max_amount"]);
                order.MinDebetAccountRest = Convert.ToDouble(dt.Rows[0]["minimal_rest"]);
                order.PaymentOrder.DebitAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["debet_account"].ToString()));
                if (order.SubType == 3)
                {
                    order.PaymentOrder.ReceiverAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["credit_bank_code"].ToString() + dt.Rows[0]["credit_account"].ToString()));
                }
                else
                {
                    order.PaymentOrder.ReceiverAccount = new Account(dt.Rows[0]["credit_bank_code"].ToString() + dt.Rows[0]["credit_account"].ToString());
                }
                order.PaymentOrder.ReceiverBankCode = Convert.ToInt32(dt.Rows[0]["credit_bank_code"]);
                order.PaymentOrder.Receiver = dt.Rows[0]["receiver_name"].ToString();
                order.Periodicity = Convert.ToInt32(dt.Rows[0]["period"]);
                order.CheckDaysCount = Convert.ToUInt16(dt.Rows[0]["check_days_count"]);
                order.PeriodicDescription = dt.Rows[0]["description"].ToString();
                order.PayIfNoDebt = dt.Rows[0]["PayIfNoDebt"].ToString() == "" ? (ushort)0 : Convert.ToUInt16(dt.Rows[0]["PayIfNoDebt"].ToString());
                if (dt.Rows[0]["Partial_Payments"].ToString() != "")
                {
                    order.PartialPaymentSign = Convert.ToByte(dt.Rows[0]["Partial_Payments"]);
                }
                if (Convert.ToByte(dt.Rows[0]["total_rest"]) == 1)
                {
                    order.ChargeType = 2;
                }
                else
                {
                    order.ChargeType = 1;
                }
                if (dt.Rows[0]["amount_for_payment"].ToString() != "")
                {
                    order.Fee = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                }
                if (dt.Rows[0]["deb_for_transfer_payment"].ToString() != "")
                {
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["deb_for_transfer_payment"]));
                }

                order.Source = (SourceType)(Convert.ToInt16(dt.Rows[0]["source_type"]));
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.StartDate = dt.Rows[0]["date_of_beginning"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]) : default(DateTime);
                order.PaymentOrder.RegistrationDate = order.RegistrationDate;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
            }



            return order;
        }
        /// <summary>
        /// Բյուջե պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PeriodicBudgetPaymentOrder GetPeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order)
        {
            order.BudgetPaymentOrder = new BudgetPaymentOrder();
            using DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                        d.document_number,
                                                        d.document_type,
                                                        d.document_subtype,
                                                        d.registration_date,
                                                        o.transfer_type,
                                                        d.currency,
                                                        o.date_of_normal_end,
                                                        o.amount_type,
                                                        o.first_repayment_date,
                                                        d.amount,
                                                        o.min_amount,
                                                        o.max_amount,
                                                        o.minimal_rest,
                                                        d.debet_account,
                                                        d.credit_account,
                                                        d.credit_bank_code,
                                                        d.receiver_name,
                                                        o.period,
                                                        o.check_days_count,
                                                        d.description,
                                                        o.PayIfNoDebt,
                                                        o.Partial_Payments,
                                                        o.total_rest,
                                                        d.deb_for_transfer_payment,
                                                        d.amount_for_payment,
                                                        d.police_code,
                                                        d.budj_transfer_reg_code,
                                                        d.quality,
                                                        d.source_type,
                                                        d.operation_date,
                                                        d.confirmation_date,
                                                        o.date_of_beginning
                                                        FROM Tbl_HB_documents d INNER JOIN Tbl_HB_Operations_by_period o ON d.doc_ID=o.docID
                                                        WHERE d.doc_ID=@DocID  and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.BudgetPaymentOrder.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.FirstTransferDate = Convert.ToDateTime(dt.Rows[0]["first_repayment_date"]);
                order.PeriodicType = Convert.ToInt32(dt.Rows[0]["transfer_type"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                if (dt.Rows[0]["date_of_normal_end"].ToString() != "")
                {
                    order.LastOperationDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end"]);
                }
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.MinAmountLevel = Convert.ToDouble(dt.Rows[0]["min_amount"]);
                order.MaxAmountLevel = Convert.ToDouble(dt.Rows[0]["max_amount"]);
                order.MinDebetAccountRest = Convert.ToDouble(dt.Rows[0]["minimal_rest"]);
                order.BudgetPaymentOrder.DebitAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["debet_account"].ToString()));
                order.BudgetPaymentOrder.ReceiverAccount = new Account(Convert.ToUInt64(dt.Rows[0]["credit_bank_code"].ToString() + dt.Rows[0]["credit_account"].ToString()));
                order.BudgetPaymentOrder.ReceiverBankCode = Convert.ToInt32(dt.Rows[0]["credit_bank_code"]);
                order.BudgetPaymentOrder.Receiver = dt.Rows[0]["receiver_name"].ToString();
                order.Periodicity = Convert.ToInt32(dt.Rows[0]["period"]);
                order.CheckDaysCount = Convert.ToUInt16(dt.Rows[0]["check_days_count"]);
                order.PeriodicDescription = dt.Rows[0]["description"].ToString();
                order.PayIfNoDebt = Convert.ToUInt16(dt.Rows[0]["PayIfNoDebt"]);
                if (dt.Rows[0]["Partial_Payments"].ToString() != "")
                {
                    order.PartialPaymentSign = Convert.ToByte(dt.Rows[0]["Partial_Payments"]);
                }
                if (Convert.ToByte(dt.Rows[0]["total_rest"]) == 1)
                {
                    order.ChargeType = 2;
                }
                else
                {
                    order.ChargeType = 1;
                }
                if (dt.Rows[0]["amount_for_payment"].ToString() != "")
                {
                    order.Fee = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                }
                if (dt.Rows[0]["deb_for_transfer_payment"].ToString() != "")
                {
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["deb_for_transfer_payment"]));
                }
                if (dt.Rows[0]["police_code"].ToString() != "")
                {
                    order.BudgetPaymentOrder.PoliceCode = Convert.ToInt32(dt.Rows[0]["police_code"]);
                }
                if (dt.Rows[0]["budj_transfer_reg_code"].ToString() != "")
                {
                    order.BudgetPaymentOrder.LTACode = Convert.ToInt16(dt.Rows[0]["budj_transfer_reg_code"]);
                }
                order.Source = (SourceType)(Convert.ToInt16(dt.Rows[0]["source_type"]));
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]);
            }



            return order;
        }
        /// <summary>
        /// Կոմունալ պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PeriodicUtilityPaymentOrder GetPeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            order.UtilityPaymentOrder = new UtilityPaymentOrder();
            using DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                        d.document_number,
                                                        d.document_type,
                                                        d.document_subtype,
                                                        d.registration_date,
                                                        o.transfer_type,
                                                        d.currency,
                                                        o.date_of_normal_end,
                                                        o.amount_type,
                                                        o.first_repayment_date,
                                                        d.amount,
                                                        o.min_amount,
                                                        o.max_amount,
                                                        o.minimal_rest,
                                                        d.debet_account,
                                                        d.credit_account,
                                                        d.credit_bank_code,
                                                        d.receiver_name,
                                                        o.period,
                                                        o.check_days_count,
                                                        d.description,
                                                        o.PayIfNoDebt,
                                                        o.Partial_Payments,
                                                        o.total_rest,
                                                        o.amount_type,
                                                        o.FindField1,
                                                        o.FindField2,
                                                        o.abonent_type,
                                                        d.quality,
                                                        d.source_type,
                                                        d.operation_date,
                                                        d.confirmation_date,
                                                        o.date_of_beginning
                                                        FROM Tbl_HB_documents d INNER JOIN Tbl_HB_Operations_by_period o ON d.doc_ID=o.docID
                                                        WHERE d.doc_ID=@DocID  and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.UtilityPaymentOrder.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.FirstTransferDate = Convert.ToDateTime(dt.Rows[0]["first_repayment_date"]);
                order.UtilityPaymentOrder.CommunalType = (CommunalTypes)Convert.ToInt16(dt.Rows[0]["transfer_type"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                if (dt.Rows[0]["date_of_normal_end"].ToString() != "")
                {
                    order.LastOperationDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end"]);
                }
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.MinAmountLevel = Convert.ToDouble(dt.Rows[0]["min_amount"]);
                order.MaxAmountLevel = Convert.ToDouble(dt.Rows[0]["max_amount"]);
                order.MinDebetAccountRest = Convert.ToDouble(dt.Rows[0]["minimal_rest"]);
                order.UtilityPaymentOrder.DebitAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["debet_account"].ToString()));
                order.Periodicity = Convert.ToInt32(dt.Rows[0]["period"]);
                order.CheckDaysCount = Convert.ToUInt16(dt.Rows[0]["check_days_count"]);
                order.PeriodicDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                order.PayIfNoDebt = Convert.ToUInt16(dt.Rows[0]["PayIfNoDebt"]);
                if (dt.Rows[0]["Partial_Payments"].ToString() != "")
                {
                    order.PartialPaymentSign = Convert.ToByte(dt.Rows[0]["Partial_Payments"]);
                }
                if (Convert.ToByte(dt.Rows[0]["total_rest"]) == 1)
                {
                    order.AllDebt = true;
                    order.ChargeType = 2;
                }
                else
                {
                    order.AllDebt = false;
                    order.ChargeType = 1;
                }
                if (order.UtilityPaymentOrder.CommunalType == CommunalTypes.VivaCell || order.UtilityPaymentOrder.CommunalType == CommunalTypes.UCom)
                {
                    order.UtilityPaymentOrder.Description = dt.Rows[0]["FindField2"].ToString();
                }
                else
                {
                    order.UtilityPaymentOrder.Branch = dt.Rows[0]["FindField2"].ToString();
                }

                if (dt.Rows[0]["abonent_type"].ToString() != "")
                {
                    order.UtilityPaymentOrder.AbonentType = Convert.ToInt32(dt.Rows[0]["abonent_type"]);
                }
                if (dt.Rows[0]["amount_type"].ToString() != "")
                {
                    order.ServicePaymentType = Convert.ToInt16(dt.Rows[0]["amount_type"]);
                }
                if (dt.Rows[0]["FindField1"].ToString() != "")
                {
                    order.UtilityPaymentOrder.Code = dt.Rows[0]["FindField1"].ToString();
                }
                order.Source = (SourceType)(Convert.ToInt16(dt.Rows[0]["source_type"]));
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]);
            }

            return order;
        }

        /// <summary>
        /// պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicOrder order, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_periodic_transfer_order(order_id, service_payment_type, all_debt, min_amount_level, max_amount_level, periodic_type, first_transfer_date,
                                            periodic_description, charge_type, periodicity, check_days_count, last_operation_date, min_debit_account_rest, pay_if_no_debt, partial_payment_sign, 
                                            fee_account, fee, debit_account) 
                                        VALUES(@orderId, @servicePaymentType, @allDebt, @minAmountLevel, @maxAmountLevel, @periodicType, @firstTransferDate, @periodicDescription, @chargeType,
                                            @periodicity, @checkDaysCount, @lastOperationDate, @minDebitAccountRest, @payIfNoDebt, @partialPaymentSign, @feeAccount, @fee, @debitAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@servicePaymentType", SqlDbType.TinyInt).Value = order.ServicePaymentType;
                    cmd.Parameters.Add("@allDebt", SqlDbType.TinyInt).Value = order.AllDebt;
                    cmd.Parameters.Add("@minAmountLevel", SqlDbType.Float).Value = order.MinAmountLevel;
                    cmd.Parameters.Add("@maxAmountLevel", SqlDbType.Float).Value = order.MaxAmountLevel;
                    cmd.Parameters.Add("@periodicType", SqlDbType.Int).Value = order.PeriodicType;
                    cmd.Parameters.Add("@firstTransferDate", SqlDbType.SmallDateTime).Value = order.FirstTransferDate;
                    cmd.Parameters.Add("@periodicDescription", SqlDbType.NVarChar, 4000).Value = (object)order.PeriodicDescription ?? DBNull.Value;
                    cmd.Parameters.Add("@chargeType", SqlDbType.SmallInt).Value = order.ChargeType;
                    cmd.Parameters.Add("@periodicity", SqlDbType.SmallInt).Value = order.Periodicity;
                    cmd.Parameters.Add("@checkDaysCount", SqlDbType.SmallInt).Value = order.CheckDaysCount;
                    cmd.Parameters.Add("@lastOperationDate", SqlDbType.DateTime).Value = order.LastOperationDate == null ? DBNull.Value : (object)order.LastOperationDate.Value;
                    cmd.Parameters.Add("@minDebitAccountRest", SqlDbType.Float).Value = order.MinDebetAccountRest;
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.SmallInt).Value = order.PayIfNoDebt;
                    cmd.Parameters.Add("@partialPaymentSign", SqlDbType.TinyInt).Value = order.PartialPaymentSign == 0 ? DBNull.Value : (object)order.PartialPaymentSign;
                    cmd.Parameters.Add("@feeAccount", SqlDbType.BigInt).Value = order.FeeAccount == null ? DBNull.Value : (object)order.FeeAccount.AccountNumber;
                    cmd.Parameters.Add("@fee", SqlDbType.BigInt).Value = order.Fee;
                    cmd.Parameters.Add("@debitAccount", SqlDbType.BigInt).Value = (object)order.DebitAccount.AccountNumber ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Կոմունալ պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicUtilityPaymentOrderDetails(PeriodicUtilityPaymentOrder order, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_periodic_UP_order_details(order_id, communal_type, abonent_code, abonent_branch) VALUES(@orderId, @communalType, @abonentCode, @abonentBranch)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@communalType", SqlDbType.SmallInt).Value = order.UtilityPaymentOrder.CommunalType;
                    cmd.Parameters.Add("@abonentCode", SqlDbType.NVarChar, 50).Value = order.UtilityPaymentOrder.Code;
                    cmd.Parameters.Add("@abonentBranch", SqlDbType.NVarChar, 50).Value = (object)order.UtilityPaymentOrder.Branch ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Բյուջե պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="budgetOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicBudgetPaymentOrderDetails(PeriodicBudgetPaymentOrder budgetOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_periodic_budget_order_details(order_id, LTA_code, police_code, credit_account) VALUES(@orderId, @LTACode, @policeCode, @creditAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@LTACode", SqlDbType.SmallInt).Value = budgetOrder.BudgetPaymentOrder.LTACode;
                    cmd.Parameters.Add("@policeCode", SqlDbType.Int).Value = budgetOrder.BudgetPaymentOrder.PoliceCode;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.NVarChar, 50).Value = (object)budgetOrder.BudgetPaymentOrder.ReceiverAccount.AccountNumber ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Սեփական հաշիվների միջև և ՀՀ տարածքում պարբերարական հայտի պահպանում
        /// </summary>
        /// <param name="periodicPaymentOrder"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicPaymentOrderDetails(PeriodicPaymentOrder periodicPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_periodic_payment_order_details(order_id, credit_account) VALUES(@orderId, @creditAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.NVarChar, 50).Value = (object)periodicPaymentOrder.PaymentOrder.ReceiverAccount.AccountNumber ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SavePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewPeriodicDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@date_of_beginning", SqlDbType.SmallDateTime).Value = order.StartDate;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = order.PeriodicType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    if (order.ServicePaymentType != 0)
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = order.ServicePaymentType;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_type", SqlDbType.TinyInt).Value = 0;
                    }
                    if (order.LastOperationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_normal_end", SqlDbType.SmallDateTime).Value = order.LastOperationDate.Value;
                    }
                    cmd.Parameters.Add("@first_repayment_date", SqlDbType.SmallDateTime).Value = order.FirstTransferDate;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = order.MinAmountLevel;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = order.MaxAmountLevel;
                    cmd.Parameters.Add("@minimal_rest", SqlDbType.Float).Value = order.MinDebetAccountRest;
                    cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = order.StatementAccount.AccountNumber;


                    cmd.Parameters.Add("@period", SqlDbType.VarChar, 50).Value = order.Periodicity;
                    cmd.Parameters.Add("@check_days_count", SqlDbType.Int).Value = order.CheckDaysCount;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.PeriodicDescription;
                    cmd.Parameters.Add("@PayIfNoDebt", SqlDbType.Int).Value = order.PayIfNoDebt;
                    if (order.PartialPaymentSign != 0)
                    {
                        cmd.Parameters.Add("@Partial_Payments", SqlDbType.TinyInt).Value = order.PartialPaymentSign;
                    }
                    if (order.ChargeType == 2)
                    {
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 1;
                    }
                    else
                        cmd.Parameters.Add("@total_rest", SqlDbType.Bit).Value = 0;

                    if (order.FeeAccount != null)
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = order.FeeAccount.AccountNumber;
                    }
                    else
                    {
                        cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value = -1;
                    }
                    if (order.Fee != 0)
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = order.Fee;
                    }
                    else
                    {
                        cmd.Parameters.Add("@amount_for_payment", SqlDbType.Float).Value = 0;
                    }
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add("@swift_code", SqlDbType.NVarChar, 12).Value = order.ReceiverBankSwiftCode;
                    cmd.Parameters.Add("@existence_of_circulation", SqlDbType.Bit).Value = order.ExistenceOfCirculation;

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
                    else if (actionResult == 0 || actionResult == 8 || actionResult == 229 || actionResult == 227 || actionResult == 230)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }

        }

        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի հայտի տվյալներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static PeriodicSwiftStatementOrder GetPeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                        d.document_number,
                                                        d.document_type,
                                                        d.document_subtype,
                                                        d.registration_date,
                                                        o.transfer_type,
                                                        d.currency,
                                                        o.date_of_normal_end,
                                                        o.amount_type,
                                                        o.first_repayment_date,
                                                        d.amount,
                                                        o.min_amount,
                                                        o.max_amount,
                                                        o.minimal_rest,
                                                        d.debet_account,
                                                        d.credit_account,
                                                        d.credit_bank_code,
                                                        d.receiver_name,
                                                        o.period,
                                                        o.check_days_count,
                                                        d.description,
                                                        o.PayIfNoDebt,
                                                        o.Partial_Payments,
                                                        o.total_rest,
                                                        d.deb_for_transfer_payment,
                                                        d.amount_for_payment,
                                                        d.quality,
                                                        d.source_type,
                                                        d.operation_date,
                                                        d.Receiver_bank_swift,
                                                        o.date_of_beginning
                                                        FROM Tbl_HB_documents d INNER JOIN Tbl_HB_Operations_by_period o ON d.doc_ID=o.docID
                                                        WHERE d.doc_ID=@DocID  and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.FirstTransferDate = Convert.ToDateTime(dt.Rows[0]["first_repayment_date"]);
                order.PeriodicType = Convert.ToInt32(dt.Rows[0]["transfer_type"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                if (dt.Rows[0]["date_of_normal_end"].ToString() != "")
                {
                    order.LastOperationDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end"]);
                }
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.MinAmountLevel = Convert.ToDouble(dt.Rows[0]["min_amount"]);
                order.MaxAmountLevel = Convert.ToDouble(dt.Rows[0]["max_amount"]);
                order.MinDebetAccountRest = Convert.ToDouble(dt.Rows[0]["minimal_rest"]);
                order.StatementAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["debet_account"].ToString()));
                order.Periodicity = Convert.ToInt32(dt.Rows[0]["period"]);
                order.CheckDaysCount = Convert.ToUInt16(dt.Rows[0]["check_days_count"]);
                order.PeriodicDescription = dt.Rows[0]["description"].ToString();
                order.PayIfNoDebt = Convert.ToUInt16(dt.Rows[0]["PayIfNoDebt"]);
                if (dt.Rows[0]["Partial_Payments"].ToString() != "")
                {
                    order.PartialPaymentSign = Convert.ToByte(dt.Rows[0]["Partial_Payments"]);
                }
                if (Convert.ToByte(dt.Rows[0]["total_rest"]) == 1)
                {
                    order.ChargeType = 2;
                }
                else
                {
                    order.ChargeType = 1;
                }
                if (dt.Rows[0]["amount_for_payment"].ToString() != "")
                {
                    order.Fee = Convert.ToDouble(dt.Rows[0]["amount_for_payment"]);
                }
                if (dt.Rows[0]["deb_for_transfer_payment"].ToString() != "")
                {
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["deb_for_transfer_payment"]));
                }

                order.Source = (SourceType)(Convert.ToInt16(dt.Rows[0]["source_type"]));
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.ReceiverBankSwiftCode = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Receiver_bank_swift"].ToString());
                order.StartDate = Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]);
            }



            return order;
        }

        internal static bool CheckForAmountLimit(PeriodicPaymentOrder order)
        {
            double amount;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"DECLARE @operDay as smalldatetime = (SELECT oper_day from tbl_current_oper_Day) 
                                                  SELECT ISNULL(SUM(amount * dbo.fnc_kurs_for_date (OP.currency,@operDay)),0) FROM Tbl_operations_by_period   OP
                                                  INNER JOIN [Tbl_all_accounts;] ACC ON OP.debet_account = ACC.arm_number
                                                  INNER JOIN (SELECT sint_acc_new FROM TbL_define_sint_acc WHERE type_of_product = 11 and type_of_account = 24 GROUP BY sint_acc_new) DEF ON ACC.type_of_account_new = DEF.sint_acc_new
                                                  WHERE OP. quality = 1 and OP.Transfer_Type = 1 AND OP.customer_number = @customerNumber", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.VarChar).Value = order.CustomerNumber;

                amount = Double.Parse(cmd.ExecuteScalar().ToString());
            }

            if (amount + order.Amount * Utility.GetCBKursForDate(order.RegistrationDate, order.Currency) > 1000000)
                return false;
            else
                return true;
        }
    }
}
