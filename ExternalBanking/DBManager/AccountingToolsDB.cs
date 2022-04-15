using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace ExternalBanking.DBManager
{
    internal static class AccountingToolsDB
    {
        /// <summary>
        /// Վերադարձում է փոխանցման համար միջնորդավճարը
        /// </summary>
        internal static double GetFeeForInternationalTransfer(string AccountNumber, string DetailsOfCharges, string Currency, double Amount, bool UrgentSign, short countryCode, DateTime operationDate)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "pr_get_SWIFT_transfer_fee";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = Convert.ToInt64(AccountNumber);
                    cmd.Parameters.Add("@our_ben", SqlDbType.VarChar, 10).Value = DetailsOfCharges;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = Amount;
                    cmd.Parameters.Add("@urgent", SqlDbType.SmallInt).Value = Convert.ToInt16(UrgentSign);
                    cmd.Parameters.Add("@country", SqlDbType.Int).Value = countryCode;
                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = operationDate;
                    cmd.Parameters.Add(new SqlParameter("@accountTransferAmount", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result = Convert.ToDouble(cmd.Parameters["@price"].Value);

                }
            }
            return result;
        }
        internal static double GetFeeForLocalTransfer(PaymentOrder order)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT dbo.fn_get_Bank_Mail_fee(@armNumber,@amount,@sourceType,@transferGroup,@transferType,@setDate,@urgent,@taxTransfer,@isOperationByPeriod)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@armNumber", SqlDbType.Float).Value = order.DebitAccount?.AccountNumber;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;
                    cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = order.Source;
                    cmd.Parameters.Add("@transferGroup", SqlDbType.TinyInt).Value = 1;
                    cmd.Parameters.Add("@transferType", SqlDbType.TinyInt).Value = 0;
                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@urgent", SqlDbType.Bit).Value = order.UrgentSign;
                    cmd.Parameters.Add("@taxTransfer", SqlDbType.TinyInt).Value = 0;
                    if (order.Type == OrderType.PeriodicTransfer)
                        cmd.Parameters.Add("@isOperationByPeriod", SqlDbType.TinyInt).Value = 1;
                    else
                        cmd.Parameters.Add("@isOperationByPeriod", SqlDbType.TinyInt).Value = 0;

                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return result;
        }


        /// <summary>
        /// Վերադարձում է  հաճախորդի կատարած փոխանցումների ծավալը նշված փոխանցման արժույթի համար
        /// </summary>
        /// <returns></returns>
        internal static double GetCustomerTransferSum(ulong customerNumber, DateTime transferDateBeg, DateTime transferDateEnd, string operationCurrency)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT dbo.fn_get_customer_transfer_sum(@customerNumber,@transferDateBeg,@transferDateEnd,1,@transferCurency)";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@transferDateBeg", SqlDbType.SmallDateTime).Value = transferDateBeg;
                    cmd.Parameters.Add("@transferDateEnd", SqlDbType.SmallDateTime).Value = transferDateEnd;
                    cmd.Parameters.Add("@transferCurency", SqlDbType.VarChar, 3).Value = operationCurrency;

                    result = double.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է սեփական հաշիվների միջև կատարվող փոխանցման միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        internal static double GetCashPaymentOrderFee(int index, string fieldName, string accountNumber)
        {
            double result = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    DataTable dt = new DataTable();
                    //index 93 կանխիկ մուտք հաշվին RUR ով(հատուկ սակագին)
                    if (index == 93 || index == 930 )
                    {
                        cmd.CommandText = @"Select TOP 1 AdditionValue FROM Tbl_all_accounts_AddInf WHERE arm_number=@accountNumber and AdditionID=14";
                    }
                    //կանխիկ ելք հաշվից(հատուկ սակագին)
                    else
                    {
                        cmd.CommandText = @"Select AdditionValue  FROM Tbl_all_accounts_AddInf WHERE arm_number=@accountNumber and AdditionID=4";
                    }
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        result = double.Parse(dt.Rows[0]["AdditionValue"].ToString());
                    }
                    else
                    {
                        result = Utility.GetPriceInfoByIndex(index, fieldName);
                    }
                }
            }
            return result;
        }





        /// <summary>
        /// Վերադարձնում է միջմասնաճուղային առանց հաշվի բացման փոխանցման ելքի միջնորդավճարի տոկոսը և գումարը:
        /// </summary>
        /// <returns></returns>
        internal static void GetPriceForTransferCashOut(double amount, string currency, DateTime? date, short transferType, ulong transferID, out double percent, out double priceAmount)
        {
            percent = 0;
            priceAmount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_price_for_transfer_cash_out";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = amount;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = currency;
                    cmd.Parameters.Add("@calcDate", SqlDbType.SmallDateTime).Value = date;
                    cmd.Parameters.Add("@transferType", SqlDbType.SmallInt).Value = transferType;
                    cmd.Parameters.Add("@transferID", SqlDbType.BigInt).Value = transferID;
                    cmd.Parameters.Add(new SqlParameter("@percent", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@priceAmount", SqlDbType.Money) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    percent = Convert.ToDouble(cmd.Parameters["@percent"].Value);//Փոխվել է Convert.ToInt64 -ը Convert.ToDouble(02/12/16)
                    priceAmount = Convert.ToDouble(cmd.Parameters["@priceAmount"].Value);


                }
            }

        }

        internal static double GetFromBusinessmanToOwnerAccountTransferRateAmount(PaymentOrder order)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select dbo.fn_get_from_businessman_to_own_card_transfer_rate(@transferCurency)";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@transferCurency", SqlDbType.VarChar, 3).Value = order.Currency;

                    double percent = double.Parse(cmd.ExecuteScalar().ToString());

                    cmd.Parameters.Clear();

                    cmd.CommandText = "select dbo.fn_get_from_businessman_to_own_card_transfer_RateAmount(@amount,@transferCurency,@percent,@oper_day)";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@transferCurency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@percent", SqlDbType.Float).Value = percent;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    result = double.Parse(cmd.ExecuteScalar().ToString());

                }
            }
            return result;
        }


        internal static double GetCashoutOrderFee(PaymentOrder order, int feeType)
        {
            double result = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_price_for_cash_out";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@feeType", SqlDbType.TinyInt).Value = feeType;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;
                    cmd.Parameters.Add("@calcDate", SqlDbType.SmallDateTime).Value = order.OperationDate.Value;
                    cmd.Parameters.Add(new SqlParameter("@percent", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@priceAmount", SqlDbType.Money) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    //Convert.ToDouble(cmd.Parameters["@percent"].Value);
                    result = Convert.ToDouble(cmd.Parameters["@priceAmount"].Value);


                }
            }
            return result;
        }

        internal static double GetCashInOperationFeeForAMDAccount(string accountNumber, decimal amount)
        {
            double result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select dbo.fn_GetCashInOperationFeeForAMDAccount(@accountNumber,@amount)";
                    cmd.CommandType = CommandType.Text;

                    if (!string.IsNullOrEmpty(accountNumber))
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    else
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = 0;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = amount;

                    result = double.Parse(cmd.ExecuteScalar().ToString());



                }
            }
            return result;
        }

    }
}
