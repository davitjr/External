using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public static class ConfirmationDB
    {
        public static void WriteToJournal(Confirmation confirmation)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"INSERT INTO Tbl_journal_of_confirmations 
                                            (Filialcode,
                                            number_of_item,
                                            number_of_set,
                                            date_of_accounting,
                                            current_account_full_number,
                                            reverse_account_full_number,
                                            currency,
                                            amount,
                                            amount_currency,
                                            wording,
                                            add_inf,
                                            operation_type,
                                            changed_rate,
                                            unique_number,
                                            confirm_result,
                                            date_of_confirm,
                                            approve_type,
                                            HB_Doc_ID,
                                            confirmation_type) 
                                            VALUES 
                                            (@filialCode,
                                            0,
                                            @numberOfSet,
                                            @dateOfAccounting,
                                            @debetAccount,
                                            @creditAccount,
                                            @currency,
                                            @amount,
                                            @amountCurrency,
                                            @wording,
                                            '',
                                            @operationType,
                                            @changedRate,
                                            @uniqueNumber,
                                            @confirm_result,
                                            @date_of_confirm,
                                            @approveType,
                                            @hbDocId,
                                            @confirmationType)";

                    cmd.Parameters.Add("@filialCode", SqlDbType.SmallInt).Value = confirmation.FilialCode;
                    cmd.Parameters.Add("@numberOfSet", SqlDbType.SmallInt).Value = confirmation.UserId;
                    cmd.Parameters.Add("@dateOfAccounting", SqlDbType.SmallDateTime).Value = confirmation.RegistrationDate;
                    cmd.Parameters.Add("@debetAccount", SqlDbType.Float).Value = confirmation.DebetAccountNumber;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.Float).Value = confirmation.CreditAccountNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = confirmation.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = confirmation.Amount;
                    cmd.Parameters.Add("@amountCurrency", SqlDbType.Float).Value = confirmation.AmountCurrency;
                    cmd.Parameters.Add("@wording", SqlDbType.VarChar).Value = confirmation.Wording;
                    cmd.Parameters.Add("@operationType", SqlDbType.SmallInt).Value = confirmation.OperationType;
                    cmd.Parameters.Add("@changedRate", SqlDbType.Float).Value = confirmation.ChangedRate;
                    cmd.Parameters.Add("@uniqueNumber", SqlDbType.BigInt).Value = confirmation.UniqueNumber;
                    cmd.Parameters.Add("@approveType", SqlDbType.TinyInt).Value = confirmation.ApproveType;
                    cmd.Parameters.Add("@hbDocId", SqlDbType.Int).Value = confirmation.OrderId;
                    cmd.Parameters.Add("@confirmationType", SqlDbType.TinyInt).Value = confirmation.ConfirmationType;
                    if (confirmation.ConfirmationDate != null)
                    {
                        cmd.Parameters.Add("@date_of_confirm", SqlDbType.SmallDateTime).Value = confirmation.ConfirmationDate;
                    }
                    else
                    {
                        cmd.Parameters.Add("@date_of_confirm", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    }
                    cmd.Parameters.Add("@confirm_result", SqlDbType.SmallInt).Value = confirmation.Result;



                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void AutomaticallyConfirmJournal(long orderID, short userID, DateTime OperationDate)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_journal_of_confirmation_request_response";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@dateOfAccounting", SqlDbType.SmallDateTime).Value = OperationDate;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = (int)userID;
                    cmd.Parameters.Add("@itemNumber", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = (int)orderID;
                    cmd.Parameters.Add("@confirmSetNumber", SqlDbType.Int).Value = (int)userID;
                    cmd.Parameters.Add("@confirmDate", SqlDbType.SmallDateTime).Value = OperationDate;
                    cmd.Parameters.Add("@confirmOrReject", SqlDbType.TinyInt).Value = 1; //1 - գլխավոր հաշվապահի հաստատում
                    cmd.Parameters.Add("@minItemNumber", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@transactionIsStarted", SqlDbType.Bit).Value = 1;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void InsertIntoCurrencyMarket(CurrencyExchangeOrder order, int userFilialCode, int status, short direction, bool isCross = false)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Insert_into_currency_market";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@filial_code", SqlDbType.Int).Value = userFilialCode;

                    string legalCustomerDescription = "";
                    if (order.CustomerNumber != 0)
                    {
                        byte customerType = Customer.GetCustomerType(order.CustomerNumber);
                        if (customerType != 6)
                        {
                            legalCustomerDescription = Utility.GetCustomerDescription(order.CustomerNumber);
                            legalCustomerDescription = legalCustomerDescription + " ";
                        }
                    }

                    if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking) && order.Type == OrderType.Convertation)
                    {
                        cmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 200).Value = Utility.ConvertUnicodeToAnsi(Utility.GetCustomerDescription(order.CustomerNumber));
                    }
                    else
                    {
                        cmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 200).Value = Utility.ConvertUnicodeToAnsi(legalCustomerDescription) + Utility.ConvertUnicodeToAnsi(order.OPPerson.PersonName + " " + order.OPPerson.PersonLastName);
                    }

                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = isCross ? order.AmountInCrossCurrency : order.Amount;
                    cmd.Parameters.Add("@ask_or_bid", SqlDbType.TinyInt).Value = direction;
                    cmd.Parameters.Add("@offered_currency", SqlDbType.NVarChar, 3).Value = isCross ? order.ReceiverAccount.Currency : order.Currency;
                    cmd.Parameters.Add("@opposit_currency", SqlDbType.NVarChar, 3).Value = "AMD";
                    cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
                    cmd.Parameters.Add("@offered_rate", SqlDbType.Money).Value = isCross ? order.ConvertationRate1 : order.ConvertationRate;
                    cmd.Parameters.Add("@unique_number", SqlDbType.BigInt).Value = order.UniqueNumber;
                    if (order.ForDillingApprovemnt)
                    {
                        cmd.Parameters.Add("@other", SqlDbType.NVarChar, 100).Value = Utility.ConvertUnicodeToAnsi("ՀԲ գործ. կոդ  ") + order.Id.ToString() + Utility.ConvertUnicodeToAnsi(", Մուտք. Ա/թ. ") + order.RegistrationDate.ToString("dd/MM/yyy") + " " + order.RegistrationTime;
                    }
                    else
                    {
                        cmd.Parameters.Add("@other", SqlDbType.NVarChar, 100).Value = "";
                    }
                    if (order.ConvertationCrossRate != 0)
                    {
                        cmd.Parameters.Add("@offered_cross_rate", SqlDbType.Float).Value = order.ConvertationCrossRate;
                    }
                    cmd.ExecuteNonQuery();
                }
            }

        }

        internal static void InsertIntoCurrencyMarketForInternational(CurrencyExchangeOrder order, int userFilialCode, int status, short direction, bool isCross = false)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Insert_into_currency_market";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@filial_code", SqlDbType.Int).Value = userFilialCode;
                    cmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 200).Value = Utility.ConvertUnicodeToAnsi(order.OPPerson.PersonName + " " + order.OPPerson.PersonLastName);
                    if (order.IsDoulbeExchange)
                        cmd.Parameters.Add("@amount", SqlDbType.Money).Value = isCross ? order.Amount : order.AmountInCrossCurrency;
                    else
                        cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;

                    cmd.Parameters.Add("@ask_or_bid", SqlDbType.TinyInt).Value = direction;
                    if (order.IsDoulbeExchange)
                        cmd.Parameters.Add("@offered_currency", SqlDbType.NVarChar, 3).Value = isCross ? order.Currency : order.DebitAccount.Currency;
                    else
                        cmd.Parameters.Add("@offered_currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@opposit_currency", SqlDbType.NVarChar, 3).Value = "AMD";
                    cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
                    cmd.Parameters.Add("@offered_rate", SqlDbType.Money).Value = isCross ? order.ConvertationRate1 : order.ConvertationRate;
                    cmd.Parameters.Add("@unique_number", SqlDbType.BigInt).Value = order.UniqueNumber;
                    cmd.Parameters.Add("@other", SqlDbType.NVarChar, 100).Value = "";
                    if (order.ConvertationCrossRate != 0)
                    {
                        cmd.Parameters.Add("@offered_cross_rate", SqlDbType.Float).Value = order.ConvertationCrossRate;
                    }
                    cmd.ExecuteNonQuery();





                }
            }

        }
    }
}
