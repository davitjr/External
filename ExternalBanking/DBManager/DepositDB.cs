using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class DepositDB
    {
        private static string depositSelectScript = @"SELECT 
                                                    t.description
                                                    ,d.HB_doc_ID
                                                    ,d.Type
                                                    ,d.App_Id 
                                                    ,d.deposit_number
                                                    ,d.deposit_full_number
                                                    ,d.date_of_beginning
                                                    ,d.date_of_normal_end
                                                    ,d.capital
                                                    ,d.currency
                                                    ,d.interest_rate
                                                    ,d.profit
                                                    ,d.recontract_possibility
                                                    ,d.quality
                                                    ,co_type
                                                    ,date_of_operation
                                                    ,d.start_capital
                                                    ,ISNULL(d.cancel_profit,0) cancel_profit
                                                    ,d.connect_account_full_number
                                                    ,d.connect_account_added
                                                    ,d.total_profit
                                                    ,d.interest_rate_effective
                                                    ,d.for_extract_sending
                                                    ,d.transfer_from_acc
                                                    ,d.main_deposit_number
                                                    ,d.penalty_rate
                                                    ,d.involving_set_number
                                                    ,d.servicing_set_number
                                                    ,d.keeper_open
                                                    ,ISNULL(d.cancel_percent,0) cancel_percent
                                                    ,d.bonus_interest_rate
													,dt.closing_reason_type
													,dt.closing_set_number
                                                    ,d.filialcode
                                                    ,t.Description_Engl
                                                    ,case when d.date_of_beginning > = '01/Jan/2008' then d.tax else 0 end as tax
                                                    ,taxed_profit
                                                    ,isvariation
                                                    ,CASE WHEN d.date_of_beginning >= '02/Oct/2021' THEN 0 ELSE 1 END AS has_communication
                                                    from [tbl_deposits;] d 
													inner join [tbl_type_of_deposits;] t 
                                                    on d.type=t.code 
                                                    INNER JOIN v_all_accounts A
                                                    on d.deposit_full_number = A.arm_number
													LEFT JOIN Tbl_deposits_details dt
													on d.app_id=dt.app_id

";

        public static Deposit GetDeposit(ulong productId, ulong customerNumber)
        {

            Deposit deposit = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(depositSelectScript + " WHERE d.app_id=@app_id and A.customer_number=@customer_number", conn))
                {
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        deposit = SetDeposit(row);
                    }
                }



            }

            return deposit;
        }

        public static List<Deposit> GetDeposits(ulong customerNumber)
        {
            List<Deposit> deposits = new List<Deposit>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = "";


                sql = depositSelectScript + @" WHERE A.customer_number=@customerNumber and d.quality=1
                                                 and not (d.type=14 and d.capital=0) ORDER BY d.date_of_beginning desc ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        deposits = new List<Deposit>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Deposit deposit = SetDeposit(row);

                        deposits.Add(deposit);
                    }
                }


            }


            return deposits;
        }
        /// <summary>
        /// Վերադարձնում է ավանդի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static DepositTerminationOrder GetDepositTerminationOrder(DepositTerminationOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT  acc.closing_reason_type,d.document_number,d.currency,d.debet_account,d.quality,d.description, d.registration_date,
                                        d.document_type, d.document_subtype,d.source_type,d.operation_date,d.customer_number,d.order_group_id,d.confirmation_date
                                                  FROM Tbl_HB_documents as d left join Tbl_account_closing_order_details acc
												  on acc.Doc_ID=d.doc_ID
                                                  where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn))
                {
                    order.DebitAccount = new Account();
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;



                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            long depositNumber = long.Parse(dr["document_number"].ToString());
                            order.ProductId = (ulong)Deposit.GetDeposit(depositNumber, ulong.Parse(dr["customer_number"].ToString())).ProductId;
                            order.DepositNumber = long.Parse(dr["document_number"].ToString());

                            order.DebitAccount.AccountNumber = dr["debet_account"].ToString();
                            order.Currency = dr["currency"].ToString();
                            order.Quality = (OrderQuality)(dr["quality"]);
                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            order.Description = dr["description"].ToString();
                            order.Type = (OrderType)Convert.ToInt16(dr["document_type"]);
                            order.SubType = Convert.ToByte(dr["document_subtype"]);
                            order.Source = (SourceType)int.Parse(dr["source_type"].ToString());
                            order.ClosingReasonType = ushort.Parse(dr["closing_reason_type"].ToString());
                            order.GroupId = dr["order_group_id"] != DBNull.Value ? Convert.ToInt32(dr["order_group_id"]) : 0;
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                        }
                    }

                }
                return order;
            }

        }

        public static List<Deposit> GetClosedDeposits(ulong customerNumber)
        {
            List<Deposit> deposits = new List<Deposit>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();


                string sql = depositSelectScript + @" WHERE A.customer_number=@customerNumber and d.quality=0 and not (d.type=14 and d.capital=0) order by date_of_normal_end desc";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        deposits = new List<Deposit>();

                    for (int i = 0; i < (dt.Rows.Count >= 300 ? 300 : dt.Rows.Count); i++)
                    {

                        DataRow row = dt.Rows[i];

                        Deposit deposit = SetDeposit(row);

                        deposits.Add(deposit);
                    }
                }


            }


            return deposits;
        }


        private static Deposit SetDeposit(DataRow row)
        {
            Deposit deposit = new Deposit();

            if (row != null)
            {
                deposit.ProductId = long.Parse(row["app_id"].ToString());
                deposit.DocID = long.Parse(row["HB_doc_ID"].ToString());
                deposit.DepositType = byte.Parse(row["type"].ToString());
                deposit.DepositTypeDescription = row["description"].ToString();
                deposit.DepositNumber = long.Parse(row["deposit_number"].ToString());
                deposit.MainDepositNumber = long.Parse(row["main_deposit_number"].ToString());
                deposit.DepositAccount = Account.GetAccount(ulong.Parse(row["deposit_full_number"].ToString()));
                deposit.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                deposit.EndDate = row["date_of_normal_end"] != DBNull.Value ? DateTime.Parse(row["date_of_normal_end"].ToString()) : default(DateTime);
                deposit.StartCapital = double.Parse(row["start_capital"].ToString());
                deposit.Balance = double.Parse(row["capital"].ToString());
                deposit.Currency = row["currency"].ToString();
                deposit.InterestRate = double.Parse(row["interest_rate"].ToString());
                deposit.CancelRateValue = double.Parse(row["cancel_profit"].ToString());
                deposit.CancelRate = double.Parse(row["cancel_percent"].ToString());
                deposit.CurrentRateValue = double.Parse(row["profit"].ToString());
                deposit.RecontractSign = Convert.ToBoolean(row["recontract_possibility"]);
                deposit.DepositQuality = byte.Parse(row["Quality"].ToString());
                deposit.ClosingDate = deposit.DepositQuality == 0 ? DateTime.Parse(row["date_of_operation"].ToString()) : default(DateTime?);
                deposit.ProfitTax = double.Parse(row["tax"].ToString());

                deposit.ConnectAccount = Account.GetAccount(Convert.ToUInt64(row["connect_account_full_number"]));

                if (Convert.ToUInt64(row["connect_account_full_number"]) == Convert.ToUInt64(row["connect_account_added"]))
                {
                    deposit.ConnectAccountForPercent = deposit.ConnectAccount;
                }
                else
                {
                    deposit.ConnectAccountForPercent = Account.GetAccount(Convert.ToUInt64(row["connect_account_added"]));
                }


                deposit.TotalRateValue = decimal.Parse(row["total_profit"].ToString());
                deposit.EffectiveInterestRate = Convert.ToDecimal(row["interest_rate_effective"]);

                if (row["for_extract_sending"] != DBNull.Value)
                    deposit.StatementDeliveryType = Convert.ToByte(row["for_extract_sending"]);

                deposit.ProfitOnMonthFirstDay = Convert.ToDecimal(row["transfer_from_acc"]);
                deposit.DayOfRateCalculation = DateTime.Parse(row["date_of_operation"].ToString());
                deposit.AllowAmountAddition = int.Parse(row["penalty_rate"].ToString()) == 10 ? false : true;

                if (row.Table.Columns.Contains("Co_type"))
                    deposit.JointType = ushort.Parse(row["Co_Type"].ToString());
                else
                    deposit.JointType = 0;
                deposit.InvolvingSetNumber = int.Parse(row["involving_set_number"].ToString());

                if (row["servicing_set_number"] != DBNull.Value)
                    deposit.ServicingSetNumber = int.Parse(row["servicing_set_number"].ToString());

                if (row["closing_reason_type"] != DBNull.Value)
                    deposit.ClosingReasonType = ushort.Parse(row["closing_reason_type"].ToString());
                if (row["closing_set_number"] != DBNull.Value)
                    deposit.ClosingSetNumber = int.Parse(row["closing_set_number"].ToString());
                deposit.KeeperOpen = int.Parse(row["keeper_open"].ToString());

                deposit.BonusInterestRate = Convert.ToDecimal(row["bonus_interest_rate"]);
                deposit.FilailCode = int.Parse(row["filialcode"].ToString());

                if (row.Table.Columns.Contains("Description_Engl"))
                {
                    deposit.DepositTypeDescriptionEng = row["Description_Engl"].ToString();
                }
                else
                    deposit.DepositTypeDescriptionEng = "";

                deposit.DepositOption = new List<DepositOption>();

                if (deposit.DocID != 0)
                {
                    deposit.SourceType = Order.GetOrderSourceType(deposit.DocID);
                }
                deposit.TaxedProfit = Convert.ToDecimal(row["taxed_profit"]);

                deposit.IsVariation = row["isvariation"] != DBNull.Value ? byte.Parse(row["isvariation"].ToString()) : (byte)0;

                //հաղորդակցման եղանակ դաշտի հեռացում
                if (row.Table.Columns.Contains("has_communication"))
                    deposit.HasCommunicationType = byte.Parse(row["has_communication"].ToString());


            }
            return deposit;
        }


        internal static ActionError IsAllowedAmountAddition(string debitAccountNumber, string creditAccountNumber, double amountAMD, double amount, SourceType source)
        {
            ActionError error = new ActionError();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_check_for_deposit_account_cred";
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.Add("@account", SqlDbType.Float).Value = creditAccountNumber;
                    cmd.Parameters.Add("@amount_amd", SqlDbType.Money).Value = amountAMD;
                    cmd.Parameters.Add("@amount_cur ", SqlDbType.Money).Value = amount;
                    cmd.Parameters.Add("@deb_account", SqlDbType.Float).Value = debitAccountNumber;
                    cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = (ushort)source;

                    cmd.Parameters.Add(new SqlParameter("@errorID", SqlDbType.Float) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();
                    short errorCode = Convert.ToInt16(cmd.Parameters["@errorID"].Value);
                    if (errorCode != 0)
                        error = new ActionError(errorCode);


                }
            }


            return error;
        }

        /// <summary>
        /// Նոր ավանդի հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(DepositOrder order, string userName, SourceType source)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("currency");
            dt.Columns.Add("rate");

            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewDepositDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    YesNo recontractPossibility = order.RecontractPossibility;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@deposit_type", SqlDbType.SmallInt).Value = (short)order.DepositType;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Deposit.Currency;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = order.Deposit.EndDate;
                    cmd.Parameters.Add("@interest_rate", SqlDbType.Float).Value = order.Deposit.InterestRate;
                    cmd.Parameters.Add("@isVariation", SqlDbType.Bit).Value = order.InterestRateChanged;
                    if (order.DepositAction != null && order.DepositAction.ActionId != 0)
                        cmd.Parameters.Add("@actionId", SqlDbType.Int).Value = order.DepositAction.ActionId;

                    SqlParameter prm = new SqlParameter("@dt", SqlDbType.Structured);
                    prm.Value = dt;
                    prm.TypeName = "dbo.ExchangeRatesTable";
                    cmd.Parameters.Add(prm);

                    cmd.Parameters.Add("@capital_account", SqlDbType.NVarChar, 50).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@percent_account", SqlDbType.NVarChar, 20).Value = order.PercentAccount.AccountNumber;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@recontract_possibility", SqlDbType.Bit).Value = ((short)recontractPossibility) - 1;







                    List<KeyValuePair<ulong, string>> thirdPersonCustomerNumbers = new List<KeyValuePair<ulong, string>>(order.ThirdPersonCustomerNumbers);
                    if (order.AccountType == 3)
                    {
                        KeyValuePair<ulong, string> oneJointCustomer = thirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                        cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = oneJointCustomer.Key;
                        cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = oneJointCustomer.Value;
                        cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;
                        thirdPersonCustomerNumbers.Remove(oneJointCustomer);

                    }
                    else if (order.AccountType == 2)
                    {
                        KeyValuePair<ulong, string> oneJointCustomer = thirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                        cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = oneJointCustomer.Key;
                        cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = oneJointCustomer.Value;
                        cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;
                        thirdPersonCustomerNumbers.Remove(oneJointCustomer);

                    }
                    else
                    {
                        cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;
                        cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = DBNull.Value;

                    }
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@involvingSetNumber", SqlDbType.Int).Value = order.Deposit.InvolvingSetNumber;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    if (order.DepositAccount != null)
                        cmd.Parameters.Add("@deposit_account", SqlDbType.NVarChar, 20).Value = order.DepositAccount.AccountNumber;


                    //SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    //param.Direction = ParameterDirection.Output;
                    //cmd.Parameters.Add(param);

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    if (order.DepositType == DepositType.BusinesDeposit)
                    {
                        foreach (DepositOption option in order.Deposit.DepositOption)
                        {

                            switch (option.Type)
                            {
                                case 1:
                                    cmd.Parameters.Add("@allowAddition", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@allowDecreasing", SqlDbType.Bit).Value = 0;
                                    break;
                                case 2:
                                    cmd.Parameters.Add("@allowDecreasing", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@allowAddition", SqlDbType.Bit).Value = 0;
                                    break;
                                case 3:
                                    cmd.Parameters.Add("@allowDecreasing", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@allowAddition", SqlDbType.Bit).Value = 1;
                                    break;
                                case 4:
                                    cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                    cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = -1;//Տոկոսագումարների վճարում ժամկետի վերջում
                                    break;
                                case 5:
                                    cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 2;
                                    cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                    break;
                                case 6:
                                    cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                    cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (!order.Deposit.DepositOption.Exists(m => m.Type == 1 || m.Type == 2 || m.Type == 3))
                        {
                            cmd.Parameters.Add("@allowAddition", SqlDbType.Bit).Value = 0;
                            cmd.Parameters.Add("@allowDecreasing", SqlDbType.Bit).Value = 0;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add("@allowDecreasing", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@allowAddition", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = (order.DepositType == DepositType.ChildrensDeposit || order.DepositType == DepositType.DepositAccumulative) ? 2 : 1;
                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 0;

                    }

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


                    if (result.ResultCode == ResultCode.Normal && thirdPersonCustomerNumbers.Count > 0 && order.AccountType == 2)
                    {

                        string jointCustomerInsertString = @"UPDATE Tbl_New_Deposit_Documents
                                                            set tp_customer_number2 = @tp_customer_number,
                                                                tp_description2 = @tp_description
                                                            WHERE doc_id=@doc_id";


                        thirdPersonCustomerNumbers.ForEach(m =>
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = jointCustomerInsertString;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                            cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;
                            cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = m.Key;
                            cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = m.Value;

                            cmd.ExecuteNonQuery();

                        });
                    }


                    return result;
                }

            }

        }
        /// <summary>
        /// Ավանդի տոկոսադրույք
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DepositOrderCondition GetDepositOrderCondition(DepositOrder order, SourceType source, bool isEmployeeDeposit = false)
        {

            DepositOrderCondition condition = new DepositOrderCondition();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_get_deposit_percent_from_type";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@deposit_type", SqlDbType.SmallInt).Value = (short)order.DepositType;
                    cmd.Parameters.Add("@curr", SqlDbType.VarChar, 3).Value = order.Deposit.Currency;
                    cmd.Parameters.Add("@date_start", SqlDbType.DateTime).Value = order.Deposit.StartDate;
                    cmd.Parameters.Add("@date_end", SqlDbType.DateTime).Value = order.Deposit.EndDate;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@forEmployee", SqlDbType.Bit).Value = isEmployeeDeposit;

                    if (order.DepositAction != null && order.DepositAction.ActionId != 0)
                        cmd.Parameters.Add("@actionID", SqlDbType.Int).Value = order.DepositAction.ActionId;



                    SqlParameter param = new SqlParameter("@min_amount", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@rate", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@result_code", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@var", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@nominalRate", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@rateFromOptions", SqlDbType.Float) { Direction = ParameterDirection.Output });


                    cmd.Parameters.Add(new SqlParameter("@interestRateForAllowAdditionAndDecreasing", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@interestRateForAllowAddition", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@interestRateForAllowDecreasing", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@bonusInterestRateForRepaymentType", SqlDbType.Float) { Direction = ParameterDirection.Output });

                    if (order.DepositType == DepositType.BusinesDeposit)
                    {
                        if (order.Deposit.DepositOption != null)
                        {
                            foreach (DepositOption option in order.Deposit.DepositOption)
                            {

                                switch (option.Type)
                                {
                                    case 1:
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 0;
                                        break;
                                    case 2:
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 0;
                                        break;
                                    case 3:
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 1;
                                        break;
                                    case 4:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = -1;//Տոկոսագումարների վճարում ժամկետի վերջում
                                        break;
                                    case 5:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 2;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                        break;
                                    case 6:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                        break;
                                    default:
                                        break;
                                }

                            }
                        }
                    }


                    cmd.ExecuteNonQuery();
                    ushort result = Convert.ToUInt16(cmd.Parameters["@result_code"].Value);
                    condition.Percent = Convert.ToDouble(cmd.Parameters["@rate"].Value);
                    condition.MinAmount = Convert.ToDouble(cmd.Parameters["@min_amount"].Value);
                    condition.MinDate = GetDepositMinDate(order);
                    condition.MaxDate = GetDepositMaxDate(order);
                    condition.InterestRateVariationFromOption = Convert.ToDouble(cmd.Parameters["@rateFromOptions"].Value);
                    condition.NominalRate = Convert.ToDouble(cmd.Parameters["@nominalRate"].Value);
                    if (order.DepositType == DepositType.BusinesDeposit)
                    {
                        condition.DepositOption = new List<DepositOption>();
                        condition.DepositOption = Deposit.GetBusinesDepositOptions();
                        condition.DepositOption.Find(m => m.Type == 1).Rate = Convert.ToDouble(cmd.Parameters["@interestRateForAllowAddition"].Value);
                        condition.DepositOption.Find(m => m.Type == 2).Rate = Convert.ToDouble(cmd.Parameters["@interestRateForAllowDecreasing"].Value);
                        condition.DepositOption.Find(m => m.Type == 3).Rate = Convert.ToDouble(cmd.Parameters["@interestRateForAllowAdditionAndDecreasing"].Value);
                        condition.DepositOption.Find(m => m.Type == 4).Rate = Convert.ToDouble(cmd.Parameters["@bonusInterestRateForRepaymentType"].Value);
                        condition.DepositOption.Find(m => m.Type == 5).Rate = 0;
                        condition.DepositOption.Find(m => m.Type == 6).Rate = 0;
                    }

                }

                return condition;
            }
        }




        public static ActionResult CheckDepositOrderCondition(DepositOrder order, bool isEmployeeDeposit = false)
        {
            DataTable dt = new DataTable();
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_get_deposit_percent_from_type";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@deposit_type", SqlDbType.SmallInt).Value = (short)order.DepositType;
                    cmd.Parameters.Add("@curr", SqlDbType.VarChar, 3).Value = order.Deposit.Currency;
                    cmd.Parameters.Add("@date_start", SqlDbType.DateTime).Value = order.Deposit.StartDate;
                    cmd.Parameters.Add("@date_end", SqlDbType.DateTime).Value = order.Deposit.EndDate.Date;
                    cmd.Parameters.Add("@forEmployee", SqlDbType.Bit).Value = isEmployeeDeposit;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)order.Source;
                    SqlParameter param = new SqlParameter("@min_amount", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.Parameters.Add(new SqlParameter("@rate", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@result_code", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@var", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@nominalRate", SqlDbType.Float) { Direction = ParameterDirection.Output });


                    if (order.DepositType == DepositType.BusinesDeposit)
                    {
                        if (order.Deposit.DepositOption != null)
                        {
                            foreach (DepositOption option in order.Deposit.DepositOption)
                            {

                                switch (option.Type)
                                {
                                    case 1:
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 0;
                                        break;
                                    case 2:
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 0;
                                        break;
                                    case 3:
                                        cmd.Parameters.Add("@allowDecreasingOption", SqlDbType.Bit).Value = 1;
                                        cmd.Parameters.Add("@allowAdditionOption", SqlDbType.Bit).Value = 1;
                                        break;
                                    case 4:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = -1;//Տոկոսագումարների վճարում ժամկետի վերջում
                                        break;
                                    case 5:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 2;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                        break;
                                    case 6:
                                        cmd.Parameters.Add("@repaymentType", SqlDbType.SmallInt).Value = 1;
                                        cmd.Parameters.Add("@ratePeriod", SqlDbType.Int).Value = 1;
                                        break;
                                    default:
                                        break;
                                }

                            }
                        }
                    }



                    dt.Load(cmd.ExecuteReader());


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Convert.ToInt16(dt.Rows[i]["result_code"]) != 0)
                        {
                            result.Errors.Add(new ActionError(Convert.ToInt16(dt.Rows[i]["result_code"]), new string[] { (dt.Rows[i]["var1"].ToString()) }));
                        }
                    }


                    // ushort result = Convert.ToUInt16(cmd.Parameters["@result_code"].Value);
                    //condition.Percent = Convert.ToDouble(cmd.Parameters["@rate"].Value);
                    //condition.MinAmount= Convert.ToDouble(cmd.Parameters["@min_amount"].Value);
                    //condition.MinDate = GetDepositMinDate(order);
                    //condition.MaxDate = GetDepositMaxDate(order);

                }
            }
            return result;
        }

        /// <summary>
        /// Ավանդի մինիմալ սկզբնաժամկետ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DateTime GetDepositMinDate(DepositOrder order)
        {
            DataTable dt = new DataTable();
            DateTime depositMinDate;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select  date_of_beginning FROM Tbl_deposit_product_history WHERE product_code=@deposit_type
		                                             and date_of_beginning <=@date_start ORDER BY date_of_beginning DESC", conn))
                {


                    cmd.Parameters.Add("@date_start", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate.Date;
                    cmd.Parameters.Add("@deposit_type", SqlDbType.SmallInt).Value = (short)order.DepositType;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);

                        if (dt.Rows.Count != 0)
                        {
                            DateTime datetime = Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]);

                            dt.Rows.Clear();
                            dt.Columns.Clear();

                            using (SqlCommand cmd1 = new SqlCommand(@"Select top 1 CASE WHEN product_code IN(2,6,12,15) THEN DateAdd(DAY,period_in_days_min, @date_start) ELSE DateAdd(""m"",period_in_months_min, @date_start) END as mindate FROM Tbl_deposit_product_history
                                            WHERE product_code=@deposit_type and currency=@curr and date_of_beginning = @d_max and NOT (product_code=12 and currency='EUR' and period_in_months_min in (1, 3))
                                            ORDER BY period_in_months_min ASC", conn))
                            {
                                cmd1.Parameters.Add("@d_max", SqlDbType.DateTime).Value = datetime;
                                cmd1.Parameters.Add("@deposit_type", SqlDbType.Int).Value = (short)order.DepositType;
                                cmd1.Parameters.Add("@curr", SqlDbType.NVarChar, 3).Value = order.Deposit.Currency;
                                cmd1.Parameters.Add("@date_start", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate.Date;

                                using (SqlDataReader dr1 = cmd1.ExecuteReader())
                                {
                                    dt.Load(dr1);
                                    if (dt.Rows.Count != 0)
                                    {
                                        depositMinDate = Convert.ToDateTime(dt.Rows[0]["mindate"]);
                                    }
                                    else
                                    {
                                        depositMinDate = default(DateTime);
                                    }
                                }

                            }

                        }
                        else
                        {
                            depositMinDate = default(DateTime);
                        }
                    }

                }
            }
            return depositMinDate;

        }
        /// <summary>
        /// Ավանդի մաքսիմալ վերջնաժամկետ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DateTime GetDepositMaxDate(DepositOrder order)
        {

            DataTable dt = new DataTable();
            DateTime depositMaxDate;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select  date_of_beginning FROM Tbl_deposit_product_history WHERE product_code=@deposit_type
		                                             and date_of_beginning <=@date_start ORDER BY date_of_beginning DESC", conn);
                cmd.Parameters.Add("@date_start", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate.Date;
                cmd.Parameters.Add("@deposit_type", SqlDbType.SmallInt).Value = (short)order.DepositType;
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                if (dt.Rows.Count != 0)
                {
                    DateTime datetime = Convert.ToDateTime(dt.Rows[0]["date_of_beginning"]);
                    dt.Rows.Clear();
                    dt.Columns.Clear();
                    cmd = new SqlCommand(@"Select top 1 CASE WHEN product_code IN(2,6,12,15) THEN DateAdd(DAY,  period_in_days_max, @date_start) ELSE DateAdd(""m"",  period_in_months_max, @date_start) END as max_date FROM Tbl_deposit_product_history
                                      WHERE product_code=@deposit_type and currency=@curr and date_of_beginning = @d_max
                                      ORDER BY period_in_months_max DESC", conn);
                    cmd.Parameters.Add("@deposit_type", SqlDbType.Int).Value = (short)order.DepositType;
                    cmd.Parameters.Add("@curr", SqlDbType.NVarChar, 3).Value = order.Deposit.Currency;
                    cmd.Parameters.Add("@d_max", SqlDbType.DateTime).Value = datetime;
                    cmd.Parameters.Add("@date_start", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate.Date;
                    dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    if (dt.Rows.Count != 0)
                    {
                        depositMaxDate = Convert.ToDateTime(dt.Rows[0]["max_date"]);
                    }
                    else
                    {
                        depositMaxDate = default(DateTime);
                    }

                }
                else
                {
                    depositMaxDate = default(DateTime);
                }

            }
            return depositMaxDate;

        }
        /// <summary>
        /// Վերադարձնում է ավանդի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static DepositOrder GetDepositOrder(DepositOrder order)
        {
            DataTable dt = new DataTable();
            order.Deposit = new Deposit();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select n.allow_addition_option,n.allow_decreasing_option,n.repayment_type,n.rate_period, n.deposit_number,d.amount, d.currency, d.registration_date,d.document_number,d.customer_number,h.change_user_name,d.document_type,d.document_subtype,d.quality,d.confirmation_date,
                                                n.Deposit_type, isnull(n.statement_by_email,-1)  as statement_by_email,
                                                n.start_date,
                                                n.end_date,
                                                n.interest_rate,n.capital_account,n.percent_account,n.recontract_possibility,
                                                n.tp_description,n.tp_customer_number,n.tp_description2,n.tp_customer_number2,n.account_type,d.operation_date,d.order_group_id from Tbl_HB_quality_history as h
                                                inner join Tbl_New_Deposit_Documents as n
                                                on h.Doc_ID=n.Doc_ID
                                                inner join Tbl_HB_documents as d
                                                on  d.doc_ID=n.Doc_ID
                                                where n.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows[0]["document_number"] != DBNull.Value)
                        order.OrderNumber = dt.Rows[0]["document_number"].ToString();

                    order.Deposit.Currency = dt.Rows[0]["currency"].ToString();
                    order.DepositType = (DepositType)(dt.Rows[0]["Deposit_type"]);
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Deposit.StartDate = Convert.ToDateTime(dt.Rows[0]["start_date"]);
                    order.Deposit.EndDate = Convert.ToDateTime(dt.Rows[0]["end_date"]);
                    order.Deposit.InterestRate = Convert.ToDouble(dt.Rows[0]["interest_rate"]);
                    order.DebitAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["capital_account"]));
                    order.Deposit.DepositAccount = order.DebitAccount;
                    order.PercentAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["percent_account"]));
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                    order.AccountType = Convert.ToUInt16(dt.Rows[0]["account_type"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);

                    bool recontractpossibility = Convert.ToBoolean(dt.Rows[0]["recontract_possibility"]);
                    if (recontractpossibility == true)
                    {
                        order.RecontractPossibility = ExternalBanking.YesNo.Yes;
                    }
                    else
                    {
                        order.RecontractPossibility = ExternalBanking.YesNo.No;
                    }
                    List<KeyValuePair<ulong, string>> jointList = new List<KeyValuePair<ulong, string>>();

                    if (dt.Rows[0]["tp_customer_number"].ToString() != "")
                    {
                        jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description"].ToString())));
                    }

                    if (dt.Rows[0]["tp_customer_number2"].ToString() != "")
                    {
                        jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number2"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description2"].ToString())));
                    }

                    order.ThirdPersonCustomerNumbers = jointList;

                    if (Convert.ToInt16(dt.Rows[0]["statement_by_email"]) != -1)
                        order.Deposit.StatementDeliveryType = Convert.ToInt16(dt.Rows[0]["statement_by_email"]);
                    else
                        order.Deposit.StatementDeliveryType = null;



                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);


                    order.Deposit.DepositNumber = Convert.ToInt64(dt.Rows[0]["deposit_number"].ToString());



                    short allowAddition = 0;
                    short allowDecreasing = 0;
                    short repaymentType = 0;
                    short ratePeriod = 0;
                    if (dt.Rows[0]["allow_addition_option"] != DBNull.Value)
                    {
                        allowAddition = Convert.ToInt16(dt.Rows[0]["allow_addition_option"]);

                    }
                    if (dt.Rows[0]["allow_decreasing_option"] != DBNull.Value)
                    {
                        allowDecreasing = Convert.ToInt16(dt.Rows[0]["allow_decreasing_option"]);
                    }

                    if (dt.Rows[0]["repayment_type"] != DBNull.Value)
                    {
                        repaymentType = Convert.ToInt16(dt.Rows[0]["repayment_type"]);
                    }
                    if (dt.Rows[0]["rate_period"] != DBNull.Value)
                    {
                        ratePeriod = Convert.ToInt16(dt.Rows[0]["rate_period"]);

                    }

                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

                    List<DepositOption> optionList = new List<DepositOption>();
                    List<DepositOption> businesDepositOptions = Deposit.GetBusinesDepositOptions();

                    if (order.DepositType == DepositType.BusinesDeposit)
                    {
                        if (allowAddition == 1 && allowDecreasing == 0)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 1).First());
                        }
                        if (allowAddition == 0 && allowDecreasing == 1)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 2).First());
                        }
                        if (allowAddition == 1 && allowDecreasing == 1)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 3).First());
                        }
                        if (repaymentType == 1 && ratePeriod == -1)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 4).First());
                        }
                        if (repaymentType == 2 && ratePeriod == 1)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 5).First());
                        }
                        if (repaymentType == 1 && ratePeriod == 1)
                        {
                            optionList.Add(businesDepositOptions.Where(x => x.Type == 6).First());
                        }
                        order.Deposit.DepositOption = optionList;

                    }



                }
            }
            return order;
        }
        /// <summary>
        /// Երրորդ անձանց CustomerNumber-ները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<ulong> GetThirdPersonsCustomerNumbers(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            List<ulong> list = new List<ulong>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select third_person_customer_number 
                                                 from   [Tbl_Co_Accounts_Main] acc 
                                                 INNER JOIN Tbl_co_accounts c 
                                                 on acc.ID = c.co_main_ID where type = 2 and acc.closing_date is null
                                                 and c.customer_number = @customer_number
                                                 GROUP BY third_person_Customer_number
                                                 ", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count == 0)
                    {
                        list.Add(0);

                    }
                    else
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            list.Add(Convert.ToUInt64(dt.Rows[i]["third_person_customer_number"]));
                        }
                    return list;
                }


            }

        }
        /// <summary>
        /// Ավանդի դադարեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult TerminateDepositOrder(DepositTerminationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_terminate_deposit";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@deposit_number", SqlDbType.Float).Value = order.DepositNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Balance;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@deposit_full_number", SqlDbType.Float).Value = order.DepositAccount.AccountNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@closing_reason_type", SqlDbType.SmallInt).Value = order.ClosingReasonType;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    if (order.GroupId != 0)
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }
        /// <summary>
        /// Ավանդի դադարեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondTermination(ulong customerNumber, string orderNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select doc_ID from Tbl_HB_documents where quality in (2,3,5) and document_type=4 and document_subtype=1 and
                                                document_number=@ordernumber and  customer_number=@customer_number", conn))
                {
                    cmd.Parameters.Add("@ordernumber", SqlDbType.Float).Value = orderNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    return cmd.ExecuteReader().Read();
                }

            }
        }

        internal static bool CheckDepositProducdID(long depositNumber, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select deposit_number from [Tbl_deposits;] where customer_number=@customer_number and deposit_number=@deposit_number", conn))
                {
                    cmd.Parameters.Add("@deposit_number", SqlDbType.Float).Value = depositNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    return cmd.ExecuteReader().Read();
                }

            }
        }
        /// <summary>
        /// Ավանդի գրաֆիկ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<DepositRepayment> GetDepositRepayment(ulong productId)
        {
            List<DepositRepayment> repayment = new List<DepositRepayment>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select Date_Of_Repayment,SUM(capital_repayment) as capital_repayment,
                                                  SUM(rate_repayment) as rate_repayment,SUM(Profit_Tax) as Profit_Tax
                                                  From Tbl_repayments_of_deposits where app_id=@appId
                                                  group by Date_Of_Repayment ", conn))
                {
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        cmd.Parameters.Clear();
                        using (SqlCommand cmd1 = new SqlCommand(@"Select Date_Of_Repayment,SUM(capital_repayment) as capital_repayment,
                                                  SUM(rate_repayment) as rate_repayment,SUM(Profit_Tax) as Profit_Tax
                                                  From Tbl_repayments_of_deposits_closed where app_id=@appId
                                                  group by Date_Of_Repayment", conn))
                        {
                            cmd1.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                            using (SqlDataReader dr = cmd1.ExecuteReader())
                            {

                                dt.Load(dr);
                            }
                        }

                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        DepositRepayment depositRepayment = SetDepositRepayment(row);
                        repayment.Add(depositRepayment);
                    }
                }


            }
            repayment = repayment.OrderBy(o => o.DateOfRepayment).ToList();
            return repayment;
        }

        private static DepositRepayment SetDepositRepayment(DataRow row)
        {
            DepositRepayment repayment = new DepositRepayment();

            if (row != null)
            {
                repayment.CapitalRepayment = double.Parse(row["capital_repayment"].ToString());
                repayment.DateOfRepayment = DateTime.Parse(row["Date_Of_Repayment"].ToString());
                repayment.ProfitTax = double.Parse(row["Profit_Tax"].ToString());
                repayment.RateRepayment = double.Parse(row["rate_repayment"].ToString());

            }
            return repayment;
        }
        /// <summary>
        /// Վերադարձնում է տվյալ ավանդին կցված երրորդ անձանց
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ulong> GetDepositJointCustomers(ulong productId, ulong customerNumber)
        {
            List<ulong> list = new List<ulong>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"select a.customer_number from [Tbl_deposits;] as d left join Tbl_Co_Accounts_Main as m on d.deposit_full_number=m.arm_number left join Tbl_co_accounts as a on m.ID=a.co_main_ID where d.App_Id=@app_id", conn))
                {
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    dt.Load(cmd.ExecuteReader());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["customer_number"] != DBNull.Value && Convert.ToUInt64(dt.Rows[i]["customer_number"]) != customerNumber)
                        {
                            list.Add(Convert.ToUInt64(dt.Rows[i]["customer_number"]));
                        }
                    }
                }

            }
            return list;
        }

        public static bool CheckCustomerForEmployeeDeposit(ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "CheckCustomer_For_Employee_Deposit";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_numbers", SqlDbType.VarChar, 50).Value = customerNumber;
                    cmd.Parameters.Add(new SqlParameter("@is_employee_deposit", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    short isEmployeeDeposit = Convert.ToInt16(cmd.Parameters["@is_employee_deposit"].Value);
                    if (isEmployeeDeposit == 1)
                    {
                        check = true;
                    }
                }
                return check;

            }

        }

        internal static ActionResult SaveDepositOrderDetails(DepositOrder depositOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_deposit_order_details(order_id, deposit_type, currency, start_date, end_date, interest_rate, capital_account, percent_account, recontract_possibility, statement_by_email, account_type) 
                                        VALUES(@orderId, @depositType, @currency, @startDate, @endDate, @interestRate, @capitalAccount, @percentAccount, @recontractPossibility, @statementByEmail, @accountType)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@depositType", SqlDbType.SmallInt).Value = depositOrder.DepositType;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = depositOrder.Currency;
                    cmd.Parameters.Add("@startDate", SqlDbType.DateTime).Value = depositOrder.Deposit.StartDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.DateTime).Value = depositOrder.Deposit.EndDate;
                    cmd.Parameters.Add("@interestRate", SqlDbType.Float).Value = depositOrder.Deposit.InterestRate;
                    cmd.Parameters.Add("@capitalAccount", SqlDbType.NVarChar, 50).Value = depositOrder.DebitAccount.AccountNumber.ToString();
                    cmd.Parameters.Add("@percentAccount", SqlDbType.NVarChar, 50).Value = depositOrder.PercentAccount.AccountNumber.ToString();
                    cmd.Parameters.Add("@recontractPossibility", SqlDbType.SmallInt).Value = ((short)depositOrder.RecontractPossibility) - 1;
                    cmd.Parameters.Add("@statementByEmail", SqlDbType.SmallInt).Value = depositOrder.Deposit.StatementDeliveryType;
                    cmd.Parameters.Add("@accountType", SqlDbType.SmallInt).Value = depositOrder.AccountType;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        public static Deposit GetDeposit(long depositNumber, ulong customerNumber)
        {

            Deposit deposit = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(depositSelectScript + " WHERE deposit_number=@depositNumber and A.customer_number=@customer_number", conn))
                {
                    cmd.Parameters.Add("@depositNumber", SqlDbType.Float).Value = depositNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        deposit = SetDeposit(row);
                    }
                }



            }

            return deposit;
        }

        internal static ActionResult SaveDepositTerminationOrderDetails(DepositTerminationOrder depositTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_deposit_termination_order_details(order_id, deposit_number, balance, currency, deposit_account) VALUES(@orderId, @depositNumber, @balance, @currency, @depositAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@depositNumber", SqlDbType.NVarChar, 20).Value = depositTerminationOrder.DepositNumber;
                    cmd.Parameters.Add("@balance", SqlDbType.Float).Value = depositTerminationOrder.Balance;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = depositTerminationOrder.Currency;
                    cmd.Parameters.Add("@depositAccount", SqlDbType.BigInt).Value = depositTerminationOrder.DepositAccount.AccountNumber;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի տվյալների աղբյուրը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static SourceType GetDepositSource(ulong productId, ulong customerNumber)
        {
            SourceType source = new SourceType();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select HB_doc_ID from [Tbl_deposits;] d inner join V_All_Accounts a on
                                                  d.deposit_full_number=a.Arm_number where a.customer_number= @customer_number and App_Id=@app_id", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    ulong HBDocId = Convert.ToUInt64(cmd.ExecuteScalar().ToString());
                    if (HBDocId == 0)
                    {
                        source = SourceType.Bank;
                        return source;
                    }
                    else
                    {
                        using (SqlConnection HBconn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
                        {
                            HBconn.Open();
                            using (SqlCommand HBcmd = new SqlCommand(@"select source_type from Tbl_HB_documents where customer_number=@customer_number and doc_ID=@doc_id", HBconn))
                            {
                                HBcmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                                HBcmd.Parameters.Add("@doc_id", SqlDbType.Float).Value = HBDocId;
                                if (HBcmd.ExecuteScalar() != null)
                                {
                                    source = (SourceType)Convert.ToInt16(HBcmd.ExecuteScalar().ToString());
                                }
                                else
                                {
                                    source = SourceType.Bank;
                                }
                            }
                        }

                    }
                }

            }
            return source;
        }


        public static double GetBusinesDepositOptionRate(ushort depositOption, string currency)
        {
            double rate;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT rate FROM Tbl_Busines_Deposit_Options
                                                  WHERE currency=@currency AND option_type=@optionType", conn))
                {
                    cmd.Parameters.Add("@optionType", SqlDbType.SmallInt).Value = depositOption;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    rate = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            return rate;
        }

        internal static void GetDepositOrderOption(DepositOrder order)
        {

            order.Deposit.DepositOption = new List<DepositOption>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT option_type,option_rate FROM  Tbl_New_Deposit_Document_Options WHERE Doc_ID=@docID", conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = order.Id;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DepositOption option = new DepositOption();
                        option.Type = Convert.ToUInt16(dt.Rows[i]["option_type"]);
                        option.Rate = Convert.ToDouble(dt.Rows[i]["option_rate"]);
                        order.Deposit.DepositOption.Add(option);
                    }
                }
            }
        }

        public static List<DepositAction> GetDepositActions(DepositOrder order)
        {
            List<DepositAction> actions = new List<DepositAction>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"  SELECT 0 as action_number, '' as description, '' as currency,
                                                    NULL as interest_rate, 0 as action_id UNION ALL SELECT AD.action_number,
                                                    TA.description, AD.currency, AD.interest_rate, AD.id as action_id FROM Tbl_deposits_actions_details AD
                                                    LEFT JOIN Tbl_type_of_deposits_actions TA on AD.action_number = TA.code WHERE AD.reason_app_ID
                                                    is null and AD.closing_date is null and AD.customer_number = @customernumber and AD.filialcode = @userFilialCode and AD.deadline >= @startDate ", conn))
                {
                    cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@userFilialCode", SqlDbType.Int).Value = order.user.filialCode;
                    cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = order.Deposit.StartDate.Date;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 1)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DepositAction action = new DepositAction();
                            action.ActionId = Convert.ToInt32(dt.Rows[i]["action_id"]);
                            action.ActionNumber = Convert.ToUInt16(dt.Rows[i]["action_number"]);
                            action.ActionTypeDescription = dt.Rows[i]["action_number"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : "";
                            action.Currency = dt.Rows[i]["currency"] != DBNull.Value ? dt.Rows[i]["currency"].ToString() : "";
                            action.DepositType = DepositType.DepositClassic;
                            action.EndDate = order.Deposit.StartDate.AddMonths(12);
                            action.EndDate = action.EndDate.Value.AddDays(1);
                            while (Utility.IsWorkingDay(action.EndDate.Value) != true)
                            {
                                action.EndDate = action.EndDate.Value.AddDays(1);
                            }


                            action.Percent = dt.Rows[i]["interest_rate"] != DBNull.Value ? Convert.ToDouble(dt.Rows[i]["interest_rate"]) / 100 : 0;
                            actions.Add(action);

                        }
                    }
                }
            }
            return actions;
        }

        public static double GetDepositActionAmountMaxLimit(DepositOrder order)
        {
            double amountMaxLimit;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select amount_max_limit from Tbl_deposits_actions_amount_limits where action_number=@actionNumber and currency=@currency", conn))
                {
                    cmd.Parameters.Add("@actionNumber", SqlDbType.SmallInt).Value = order.DepositAction.ActionNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Deposit.Currency;
                    amountMaxLimit = Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
            return amountMaxLimit;
        }


        public static Deposit GetActiveDeposit(string depositFullNumber)
        {

            Deposit deposit = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(depositSelectScript + " WHERE deposit_full_number=@depositFullNumber and d.quality=1", conn))
                {
                    cmd.Parameters.Add("@depositFullNumber", SqlDbType.Float).Value = depositFullNumber;

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        deposit = SetDeposit(row);
                    }
                }
            }

            return deposit;
        }

        public static List<DepositRepayment> GetDepositRepayment(DepositRepaymentRequest request)
        {
            List<DepositRepayment> repayment = new List<DepositRepayment>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "pr_get_Deposit_repayments";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@date_beg", SqlDbType.Date).Value = request.StartDate;
                    cmd.Parameters.Add("@type", SqlDbType.Int).Value = request.DepositType;
                    cmd.Parameters.Add("@curr", SqlDbType.NVarChar).Value = request.Currency;
                    cmd.Parameters.Add("@start_capital", SqlDbType.Float).Value = request.StartCapital;
                    cmd.Parameters.Add("@date_of_normal_end", SqlDbType.Date).Value = request.EndDate;
                    cmd.Parameters.Add("@Interest_Rate", SqlDbType.Float).Value = request.InterestRate;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = request.CustomerNumber;
                    cmd.Parameters.Add("@bankingSource", SqlDbType.Int).Value = request.Source;
                    cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = request.AccountType;



                    if (request.DepositType == DepositType.BusinesDeposit)
                    {
                        foreach (DepositOption option in request.DepositOption)
                        {

                            switch (option.Type)
                            {
                                case 1:
                                    cmd.Parameters.Add("@depositAllowAdditionOption", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@depositAllowDecreasingOption", SqlDbType.Bit).Value = 0;
                                    break;
                                case 2:
                                    cmd.Parameters.Add("@depositAllowDecreasingOption", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@depositAllowAdditionOption", SqlDbType.Bit).Value = 0;
                                    break;
                                case 3:
                                    cmd.Parameters.Add("@depositAllowDecreasingOption", SqlDbType.Bit).Value = 1;
                                    cmd.Parameters.Add("@depositAllowAdditionOption", SqlDbType.Bit).Value = 1;
                                    break;
                                case 4:
                                    cmd.Parameters.Add("@repayment_type", SqlDbType.SmallInt).Value = 1;
                                    cmd.Parameters.Add("@Rate_Period", SqlDbType.Int).Value = -1;//Տոկոսագումարների վճարում ժամկետի վերջում
                                    break;
                                case 5:
                                    cmd.Parameters.Add("@repayment_type", SqlDbType.SmallInt).Value = 2;
                                    cmd.Parameters.Add("@Rate_Period", SqlDbType.Int).Value = 1;
                                    break;
                                case 6:
                                    cmd.Parameters.Add("@repayment_type", SqlDbType.SmallInt).Value = 1;
                                    cmd.Parameters.Add("@Rate_Period", SqlDbType.Int).Value = 1;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (!request.DepositOption.Exists(m => m.Type == 1 || m.Type == 2 || m.Type == 3))
                        {
                            cmd.Parameters.Add("@depositAllowAdditionOption", SqlDbType.Bit).Value = 0;
                            cmd.Parameters.Add("@depositAllowDecreasingOption", SqlDbType.Bit).Value = 0;
                        }

                        List<KeyValuePair<ulong, string>> thirdPersonCustomerNumbers = new List<KeyValuePair<ulong, string>>();
                        if (request.ThirdPersonCustomerNumbers != null)
                            thirdPersonCustomerNumbers = new List<KeyValuePair<ulong, string>>(request.ThirdPersonCustomerNumbers);

                        if (request.AccountType == 3)
                        {
                            KeyValuePair<ulong, string> oneJointCustomer = thirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                            cmd.Parameters.Add("@thirdPersonCustomerNumber", SqlDbType.Float).Value = oneJointCustomer.Key;

                        }
                        else if (request.AccountType == 2)
                        {
                            KeyValuePair<ulong, string> oneJointCustomer = thirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                            cmd.Parameters.Add("@thirdPersonCustomerNumber", SqlDbType.Float).Value = oneJointCustomer.Key;

                        }
                        else
                        {
                            cmd.Parameters.Add("@thirdPersonCustomerNumber", SqlDbType.Float).Value = DBNull.Value;
                        }
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        DepositRepayment depositRepayment = SetDepositRepayment(row);
                        repayment.Add(depositRepayment);
                    }
                }


            }
            repayment = repayment.OrderBy(o => o.DateOfRepayment).ToList();
            return repayment;
        }

        internal static byte[] GetExistingDepositContract(long docId, int type)
        {

            int attachType = 0;
            attachType = type;


            return UploadedFile.GetAttachedFile(docId, attachType);
        }

        public static async Task<List<Deposit>> GetDepositsAsync(ulong customerNumber)
        {
            List<Deposit> deposits = new List<Deposit>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = "";


                sql = depositSelectScript + @" WHERE A.customer_number=@customerNumber and d.quality=1
                                                 and not (d.type=14 and d.capital=0) ORDER BY d.date_of_beginning desc ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        deposits = new List<Deposit>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        Deposit deposit = await SetDepositAsync(row);
                        deposits.Add(deposit);
                    }
                }


            }


            return deposits;
        }

        private static async Task<Deposit> SetDepositAsync(DataRow row)
        {
            Deposit deposit = new Deposit();

            if (row != null)
            {
                deposit.ProductId = long.Parse(row["app_id"].ToString());
                deposit.DocID = long.Parse(row["HB_doc_ID"].ToString());
                deposit.DepositType = byte.Parse(row["type"].ToString());
                deposit.DepositTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                deposit.DepositNumber = long.Parse(row["deposit_number"].ToString());
                deposit.MainDepositNumber = long.Parse(row["main_deposit_number"].ToString());
                deposit.DepositAccount = Account.GetAccount(ulong.Parse(row["deposit_full_number"].ToString()));
                deposit.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                deposit.EndDate = row["date_of_normal_end"] != DBNull.Value ? DateTime.Parse(row["date_of_normal_end"].ToString()) : default(DateTime);
                deposit.StartCapital = double.Parse(row["start_capital"].ToString());
                deposit.Balance = double.Parse(row["capital"].ToString());
                deposit.Currency = row["currency"].ToString();
                deposit.InterestRate = double.Parse(row["interest_rate"].ToString());
                deposit.CancelRateValue = double.Parse(row["cancel_profit"].ToString());
                deposit.CancelRate = double.Parse(row["cancel_percent"].ToString());
                deposit.CurrentRateValue = double.Parse(row["profit"].ToString());
                deposit.RecontractSign = Convert.ToBoolean(row["recontract_possibility"]);
                deposit.DepositQuality = byte.Parse(row["Quality"].ToString());
                deposit.ClosingDate = deposit.DepositQuality == 0 ? DateTime.Parse(row["date_of_operation"].ToString()) : default(DateTime?);
                deposit.ProfitTax = double.Parse(row["tax"].ToString());

                deposit.ConnectAccount = await Account.GetAccountAsync(Convert.ToUInt64(row["connect_account_full_number"]).ToString());

                if (Convert.ToUInt64(row["connect_account_full_number"]) == Convert.ToUInt64(row["connect_account_added"]))
                {
                    deposit.ConnectAccountForPercent = deposit.ConnectAccount;
                }
                else
                {
                    deposit.ConnectAccountForPercent = await Account.GetAccountAsync(Convert.ToUInt64(row["connect_account_added"]).ToString());
                }


                deposit.TotalRateValue = decimal.Parse(row["total_profit"].ToString());
                deposit.EffectiveInterestRate = decimal.Parse(row["interest_rate_effective"].ToString());

                if (row["for_extract_sending"] != DBNull.Value)
                    deposit.StatementDeliveryType = Convert.ToByte(row["for_extract_sending"]);

                deposit.ProfitOnMonthFirstDay = Convert.ToDecimal(row["transfer_from_acc"]);
                deposit.DayOfRateCalculation = DateTime.Parse(row["date_of_operation"].ToString());
                deposit.AllowAmountAddition = int.Parse(row["penalty_rate"].ToString()) == 10 ? false : true;

                if (row.Table.Columns.Contains("Co_type"))
                    deposit.JointType = ushort.Parse(row["Co_Type"].ToString());
                else
                    deposit.JointType = 0;
                deposit.InvolvingSetNumber = int.Parse(row["involving_set_number"].ToString());

                if (row["servicing_set_number"] != DBNull.Value)
                    deposit.ServicingSetNumber = int.Parse(row["servicing_set_number"].ToString());

                if (row["closing_reason_type"] != DBNull.Value)
                    deposit.ClosingReasonType = ushort.Parse(row["closing_reason_type"].ToString());
                if (row["closing_set_number"] != DBNull.Value)
                    deposit.ClosingSetNumber = int.Parse(row["closing_set_number"].ToString());
                deposit.KeeperOpen = int.Parse(row["keeper_open"].ToString());

                deposit.BonusInterestRate = Convert.ToDecimal(row["bonus_interest_rate"]);
                deposit.FilailCode = int.Parse(row["filialcode"].ToString());

                deposit.DepositOption = new List<DepositOption>();

                if (deposit.DocID != 0)
                {
                    deposit.SourceType = Order.GetOrderSourceType(deposit.DocID);
                }

                deposit.TaxedProfit = Convert.ToDecimal(row["taxed_profit"]);
            }
            return deposit;
        }


        public static DataTable GetRateForBusinessDeposits(string currency, DateTime startDate, DateTime endDate, ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM [fn_get_business_deposit_option_rates] ( @curr, @startDate, @endDate, @customerNumber )"))
                {
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@curr", SqlDbType.VarChar, 3).Value = currency;
                    cmd.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }
            }
        }

        public static List<Deposit> GetDepositsForTotalBalance(ulong customerNumber)
        {
            List<Deposit> deposits = new List<Deposit>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT 
                                                    d.capital
                                                    ,d.profit
                                                    ,d.currency                                  
                                                    from [tbl_deposits;] d 
													WHERE customer_number=@customerNumber and d.quality=1
                                                 and not (d.type=14 and d.capital=0)  ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Deposit deposit = new Deposit
                        {
                            StartCapital = Convert.ToDouble(row["capital"].ToString()),
                            CurrentRateValue = Convert.ToDouble(row["profit"]),
                            Currency = row["currency"].ToString()
                        };

                        deposits.Add(deposit);
                    }
                }
            }
            return deposits;
        }


    }
}
