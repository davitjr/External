using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using static ExternalBanking.CardStatement;
using static ExternalBanking.CardAdditionalInfo;
using ExternalBanking.ACBAServiceReference;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using ExternalBanking.Helpers;

namespace ExternalBanking.DBManager
{
    static class CardDB
    {

        internal static List<Card> GetCards(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,cash_rate,
                                loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and  customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Card card = SetCard(row);
                        cardList.Add(card);
                    }
                }
            }
            return cardList;
        }

        internal static async Task<List<Card>> GetCardsAsync(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,cash_rate,
                                loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and  customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Task<Card> cardAsync = SetCardAsync(row);

                        cardList.Add(await cardAsync);

                    }
                }
            }

            return cardList;
        }

        internal static List<Card> GetClosedCards(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,interest_rate_effective,
                                cash_rate,loan_account,overdraft_account,SMSApplicationPresent,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is not null and vn.customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Card card = SetCard(row);

                        cardList.Add(card);
                    }
                }
            }

            cardList = cardList.OrderByDescending(m => m.ClosingDate).ToList();

            cardList.ForEach(m =>
            {
                cardList.FindAll(c => c.CardAccount.AccountNumber == m.CardAccount.AccountNumber && c.ClosingDate < m.ClosingDate).ForEach(x =>
                {
                    x.Balance = 0;
                });
            });

            return cardList;
        }

        internal static Card GetCard(Account account)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT top 1 app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,
                                cash_rate,loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE vn.card_account=@accountNumber and vn.main_card_number is null order by vn.open_date desc ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = account.AccountNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetCard(row);
                    }

                }
            }

            return card;
        }

        internal static Card GetCard(ulong productId, ulong customerNumber)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT vn.app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                 dbo.Fnc_CardTypeFull(visa_number)  as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.fnc_available_AccountAmount(card_account,dbo.get_oper_day(),default,1,'0') as Balance,main_card_number,
                                cash_rate,loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,VA.card_receiving_type,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                INNER JOIN Tbl_VISA_applications VA ON VA.app_id = vn.app_id
                                WHERE vn.app_id=@productId and vn.customer_Number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetCard(row);
                    }

                }
            }

            return card;
        }

        private static Card SetCard(DataRow row)
        {
            Card card = new Card();

            if (row != null)
            {
                card.ProductId = long.Parse(row["app_id"].ToString());
                card.CardNumber = row["card_number"].ToString();

                card.CardAccount = Account.GetAccount(row["card_account"].ToString());
                if (card.CardAccount == null)
                {
                    card.CardAccount = new Account(row["card_account"].ToString());
                }

                card.Currency = row["currency"].ToString();
                card.CardType = row["Card_Type_description"].ToString();
                card.CardSystem = int.Parse(row["CardSystemID"].ToString());

                card.Balance = double.Parse(row["Balance"].ToString());


                card.MainCardNumber = row["main_card_number"].ToString();
                card.ValidationDate = DateTime.Parse(row["validation_date"].ToString());
                card.FilialCode = int.Parse(row["filialcode"].ToString());
                card.EndDate = DateTime.Parse(row["end_date"].ToString());

                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    if (card.CardAccount != null)
                    {
                        if (card.CardAccount.ClosingDate == null)
                        {
                            if (card.CardAccount.HaveActiveProduct())
                            {
                                card.Balance = 0;
                            }
                            else
                            {
                                card.Balance = card.CardAccount.Balance;
                            }
                        }
                    }

                    card.ClosingDate = DateTime.Parse(row["closing_date"].ToString());

                    if (!String.IsNullOrEmpty(row["closedby"].ToString()))
                    {
                        card.ClosingSetNumber = int.Parse(row["closedby"].ToString());
                    }

                    if (!String.IsNullOrEmpty(row["closingreason"].ToString()))
                    {
                        card.ClosingReasonType = ushort.Parse(row["closingreason"].ToString());
                    }

                    card.CreditLine = null;
                    card.Overdraft = null;

                }
                else // Վարկային գծի տվյալներ միայն գործող քարտերի համար
                {
                    card.CreditLine = CreditLine.GetCardCreditLine(card.CardNumber);
                    card.Overdraft = CreditLine.GetCardOverdraft(card.CardNumber);
                }

                card.FeeForCashTransaction = Convert.ToDouble(row["cash_rate"]);
                card.OverdraftAccount = Account.GetAccount(row["overdraft_account"].ToString());
                card.SMSApplicationPresent = bool.Parse(row["SMSApplicationPresent"].ToString());
                card.InterestRateEffective = float.Parse(row["interest_rate_effective"].ToString());
                card.PositiveInterestRate = float.Parse(row["positive_interest"].ToString());
                card.PositiveRate = float.Parse(row["positive_rate"].ToString());
                card.TotalPositiveRate = float.Parse(row["total_positive_rate"].ToString());
                if (!String.IsNullOrEmpty(row["last_day_of_pos_rate_calculation"].ToString()))
                {
                    card.LastDayOfPositiveRateCalculation = (DateTime)row["last_day_of_pos_rate_calculation"];
                }
                if (!String.IsNullOrEmpty(row["last_day_of_pos_rate_repair"].ToString()))
                {
                    card.LastDayOfPositiveRateRepair = (DateTime)row["last_day_of_pos_rate_repair"];
                }
                if (!String.IsNullOrEmpty(row["date_of_stopping_pos_rate_calculation"].ToString()))
                {
                    card.PositiveRateStoppingDay = (DateTime)row["date_of_stopping_pos_rate_calculation"];
                }
                card.Type = short.Parse(row["card_type"].ToString());
                card.RelatedOfficeNumber = ushort.Parse(row["related_office_number"].ToString());
                card.OpenDate = DateTime.Parse(row["open_date"].ToString());
                if (!String.IsNullOrEmpty(row["add_inf"].ToString()))
                {
                    card.AddInf = row["add_inf"].ToString();
                }
                card.CreditCode = LoanProduct.GetCreditCode(card.ProductId, 11);

                if (!String.IsNullOrEmpty(row["debt"].ToString()))
                {
                    card.Debt = Convert.ToDouble(row["debt"].ToString());
                }
                if (row.Table.Columns.Contains("card_receiving_type"))
                {
                    card.CardReceivingType = !String.IsNullOrEmpty(row["card_receiving_type"].ToString()) ? int.Parse(row["card_receiving_type"].ToString()) : 0;
                }
                else
                {
                    card.CardReceivingType = 0;
                }
                bool hasAttachedCard = false;
                if (row.Table.Columns.Contains("attached_card"))
                {
                    hasAttachedCard = byte.Parse(row["attached_card"].ToString()) == 1 ? true : false;
                }
                if (hasAttachedCard)
                    card = CardDB.SetSupplementaryCard(card);
                else
                    card.SupplementaryType = SupplementaryType.Main;
            }
            return card;
        }


        private static async Task<Card> SetCardAsync(DataRow row)
        {
            Card card = new Card();

            if (row != null)
            {
                card.ProductId = long.Parse(row["app_id"].ToString());
                card.CardNumber = row["card_number"].ToString();

                card.CardAccount = await Account.GetAccountAsync(row["card_account"].ToString());
                if (card.CardAccount == null)
                {
                    card.CardAccount = new Account(row["card_account"].ToString());
                }

                card.Currency = row["currency"].ToString();
                card.CardType = row["Card_Type_description"].ToString();
                card.CardSystem = int.Parse(row["CardSystemID"].ToString());

                card.Balance = double.Parse(row["Balance"].ToString());


                card.MainCardNumber = row["main_card_number"].ToString();
                card.ValidationDate = DateTime.Parse(row["validation_date"].ToString());
                card.FilialCode = int.Parse(row["filialcode"].ToString());
                card.EndDate = DateTime.Parse(row["end_date"].ToString());
                if (!String.IsNullOrEmpty(row["closing_date"].ToString()))
                {
                    if (card.CardAccount != null)
                    {
                        if (card.CardAccount.ClosingDate == null)
                        {
                            if (card.CardAccount.HaveActiveProduct())
                            {
                                card.Balance = 0;
                            }
                            else
                            {
                                card.Balance = card.CardAccount.Balance;
                            }
                        }
                    }

                    card.ClosingDate = DateTime.Parse(row["closing_date"].ToString());

                    if (!String.IsNullOrEmpty(row["closedby"].ToString()))
                    {
                        card.ClosingSetNumber = int.Parse(row["closedby"].ToString());
                    }

                    if (!String.IsNullOrEmpty(row["closingreason"].ToString()))
                    {
                        card.ClosingReasonType = ushort.Parse(row["closingreason"].ToString());
                    }

                    card.CreditLine = null;
                    card.Overdraft = null;
                }
                else // Վարկային գծի տվյալներ միայն գործող քարտերի համար
                {
                    card.CreditLine = await CreditLine.GetCardCreditLineAsync(card.CardNumber);
                    card.Overdraft = await CreditLine.GetCardOverdraftAsync(card.CardNumber);
                }

                card.FeeForCashTransaction = Convert.ToDouble(row["cash_rate"]);
                card.OverdraftAccount = await Account.GetAccountAsync(row["overdraft_account"].ToString());
                card.SMSApplicationPresent = bool.Parse(row["SMSApplicationPresent"].ToString());
                card.InterestRateEffective = float.Parse(row["interest_rate_effective"].ToString());
                card.PositiveInterestRate = float.Parse(row["positive_interest"].ToString());
                card.PositiveRate = float.Parse(row["positive_rate"].ToString());
                card.TotalPositiveRate = float.Parse(row["total_positive_rate"].ToString());
                if (!String.IsNullOrEmpty(row["last_day_of_pos_rate_calculation"].ToString()))
                {
                    card.LastDayOfPositiveRateCalculation = (DateTime)row["last_day_of_pos_rate_calculation"];
                }
                if (!String.IsNullOrEmpty(row["last_day_of_pos_rate_repair"].ToString()))
                {
                    card.LastDayOfPositiveRateRepair = (DateTime)row["last_day_of_pos_rate_repair"];
                }
                if (!String.IsNullOrEmpty(row["date_of_stopping_pos_rate_calculation"].ToString()))
                {
                    card.PositiveRateStoppingDay = (DateTime)row["date_of_stopping_pos_rate_calculation"];
                }
                card.Type = short.Parse(row["card_type"].ToString());
                card.RelatedOfficeNumber = ushort.Parse(row["related_office_number"].ToString());
                card.OpenDate = DateTime.Parse(row["open_date"].ToString());

                if (!String.IsNullOrEmpty(row["add_inf"].ToString()))
                {
                    card.AddInf = row["add_inf"].ToString();
                }
                card.CreditCode = LoanProduct.GetCreditCode(card.ProductId, 11);
                bool hasAttachedCard = false;
                if (row.Table.Columns.Contains("attached_card"))
                {
                    hasAttachedCard = byte.Parse(row["attached_card"].ToString()) == 1 ? true : false;
                }
                if (hasAttachedCard)
                    card = CardDB.SetSupplementaryCard(card);
                else
                    card.SupplementaryType = SupplementaryType.Main;
            }
            return card;
        }
        /// <summary>
        /// Քարտի սասարկման գումար
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static double GetCardTotalDebt(string cardNumber)
        {
            double fee = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"Select  currency, sum ( case when isnull(replacementfee,0)-isnull(payedreplacementfee,0)<=0 then 0 else 
                              isnull(replacementfee,0)-isnull(payedreplacementfee,0) end) as replacementfee,sum(isnull(debt,0)) as debt
                              From Tbl_Visa_Numbers_Accounts where (visa_number=@visa_number or main_card_number =@visa_number) 
                            and closing_date is null Group by currency ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@visa_number", SqlDbType.NVarChar).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        fee = (double)dt.Rows[0]["replacementfee"] + (double)dt.Rows[0]["debt"];
                        if (dt.Rows[0]["currency"].ToString() != "AMD")
                        {
                            fee = fee / Utility.GetCBKursForDate(DateTime.Now.AddDays(-1), dt.Rows[0]["currency"].ToString());
                        }
                    }

                    return fee;
                }
            }
        }


        /// <summary>
        /// Քարտի սպասարկման վարձի տվյալներ
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static CardServiceFee GetCardServiceFee(ulong productId)
        {
            CardServiceFee serviceFee = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT Service_Fee_Total, Service_Fee, Period, Service_fee_payed, Last_day_of_service_fee_payment, next_day_of_service_fee_payment,
                             Debt, ReplacementFee, PayedReplacementFee, CASE DeductionStart WHEN NULL THEN '' WHEN 0 THEN 'êÏ½µáõÙ' WHEN 1 THEN 'ì»ñçáõÙ' END as DeductionStart   
                             FROM tbl_visa_numbers_accounts where App_id = @product_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            serviceFee = new CardServiceFee();
                            serviceFee.ServiceFeeTotal = (dr["Service_Fee_Total"] != DBNull.Value) ? Convert.ToDouble(dr["Service_Fee_Total"]) : default(double);
                            serviceFee.ServiceFee = (dr["Service_Fee"] != DBNull.Value) ? Convert.ToDouble(dr["Service_Fee"]) : default(double);
                            serviceFee.Period = (dr["Period"] != DBNull.Value) ? Convert.ToInt32(dr["Period"]) : 0;
                            serviceFee.FirstCharge = Utility.ConvertAnsiToUnicode(dr["DeductionStart"].ToString());
                            serviceFee.ServiceFeePayed = (dr["Service_fee_payed"] != DBNull.Value) ? Convert.ToDouble(dr["Service_fee_payed"]) : default(double);
                            if (!String.IsNullOrEmpty(dr["Last_day_of_service_fee_payment"].ToString()))
                            {
                                serviceFee.LastDayOfServiceFeePayment = (DateTime)dr["Last_day_of_service_fee_payment"];
                            }
                            if (!String.IsNullOrEmpty(dr["next_day_of_service_fee_payment"].ToString()))
                            {
                                serviceFee.NextDayOfServiceFeePayment = (DateTime)dr["next_day_of_service_fee_payment"];
                            }
                            serviceFee.Debt = (dr["Debt"] != DBNull.Value) ? Convert.ToDouble(dr["Debt"]) : default(double);
                            serviceFee.ReplacementFee = (dr["ReplacementFee"] != DBNull.Value) ? Convert.ToDouble(dr["ReplacementFee"]) : default(double);
                            serviceFee.PayedReplacementFee = (dr["PayedReplacementFee"] != DBNull.Value) ? Convert.ToDouble(dr["PayedReplacementFee"]) : default(double);

                        }
                    }


                }

                return serviceFee;
            }
        }

        /// <summary>
        /// AMEX_MR ի սպասարկման գումար
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static double GetMRFee(string cardNumber)
        {
            double fee = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select ServiceFeeReal,ServiceFeePayed from Tbl_MR_Applications where [Status]=1 and ServiceFeeReal<>ServiceFeePayed and cardnumber=@cardNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        fee = double.Parse(dt.Rows[0]["ServiceFeeReal"].ToString()) + double.Parse(dt.Rows[0]["ServiceFeePayed"].ToString());
                    }
                    else
                        fee = -1;
                    return fee;
                }
            }
        }

        /// <summary>
        /// Քարտի պետ տուրք
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static double GetPetTurk(long productId)
        {
            double fee = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT sum(V.tax_amount - isnull(V.tax_matured_amount,0) - isnull(V.tax_concede_amount,0)) as pet_turq
                              FROM V_ProblemLoanTaxes V 
                              WHERE V.tax_quality not in (40, 41, 10) and V.App_Id_loan = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[0]["pet_turq"].ToString()))
                            fee = double.Parse(dt.Rows[0]["pet_turq"].ToString());
                    }
                    else
                        fee = -1;
                    return fee;
                }
            }
        }
        internal static CardStatement GetStatement(string cardNumber, DateTime dateFrom, DateTime dateTo, byte language, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            CardStatement cardStatement = new CardStatement();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_visa_statementFormated", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@card", SqlDbType.NVarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@lang_id", SqlDbType.NVarChar, 2).Value = language == (byte)Languages.hy ? "hy" : "eng";
                    cmd.Parameters.Add("@minAmount", SqlDbType.Float).Value = minAmount;
                    cmd.Parameters.Add("@maxAmount", SqlDbType.Float).Value = maxAmount;
                    cmd.Parameters.Add("@debCred", SqlDbType.NVarChar, 50).Value = debCred;
                    cmd.Parameters.Add("@transactionsCount", SqlDbType.Int).Value = transactionsCount;
                    cmd.Parameters.Add("@orderByAmountAscDesc", SqlDbType.TinyInt).Value = orderByAscDesc;
                    cmd.Parameters.Add("@need24_7Rest", SqlDbType.TinyInt).Value = 1;

                    cmd.CommandTimeout = 120;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CardStatementDetail statementDetails = new CardStatementDetail();
                            if (dr["transaction_date"] != DBNull.Value)
                            {
                                statementDetails.OperationDate = DateTime.Parse(dr["transaction_date"].ToString());
                            }
                            else
                            {
                                statementDetails.OperationDate = new DateTime(2001, 1, 1);
                            }
                            statementDetails.OperationAmount = dr["operation_amount"] == DBNull.Value ? (double)0 : double.Parse(dr["operation_amount"].ToString());
                            statementDetails.OperationCurrency = dr["operation_currency"].ToString();
                            statementDetails.Amount = dr["debet"] != DBNull.Value ? double.Parse(dr["debet"].ToString()) : double.Parse(dr["credit"].ToString());
                            statementDetails.DebitCredit = dr["debet"] != DBNull.Value ? (byte)1 : (byte)2;
                            statementDetails.CommissionFee = dr["fee_transaction"] == DBNull.Value ? (double)0 : double.Parse(dr["fee_transaction"].ToString());

                            statementDetails.Description = dr["description"].ToString();

                            if (dr["date_of_accounting"] != DBNull.Value)
                                statementDetails.TransactionDate = DateTime.Parse(dr["date_of_accounting"].ToString());

                            statementDetails.OperationCardNumber = dr["card_number"].ToString();
                            statementDetails.DateOfVale = DateTime.Parse(dr["date_of_value"].ToString());
                            statementDetails.MR = dr["MR"] != DBNull.Value ? Convert.ToDecimal(dr["MR"]) : 0;
                            statementDetails.CashBack = dr["cashback"] != DBNull.Value ? Convert.ToDecimal(dr["Cashback"]) : 0;
                            statementDetails.ItemNumber = dr["number_of_item"] != DBNull.Value ? Convert.ToInt32(dr["number_of_item"]) : 0;

                            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
                            if (isTestVersion)
                            {
                                if (statementDetails.OperationAmount == 0)
                                    statementDetails.ExchangeRate = 0;
                                else
                                {
                                    if (statementDetails.OperationAmount != statementDetails.Amount)
                                    {
                                        if (statementDetails.Amount > statementDetails.OperationAmount)
                                            statementDetails.ExchangeRate = (decimal)(statementDetails.Amount / statementDetails.OperationAmount);
                                        else
                                            statementDetails.ExchangeRate = (decimal)(statementDetails.OperationAmount / statementDetails.Amount);
                                    }
                                }

                                if (statementDetails.DebitCredit == 1)
                                {
                                    if (statementDetails.CommissionFee == 0)
                                        statementDetails.CommissionFeeString = "կիրառելի չէ";
                                    else
                                        statementDetails.CommissionFeeString = "-" + String.Format(CultureInfo.InvariantCulture, statementDetails.OperationCurrency == "AMD" ? "{0:#,##0.0}" : "{0:#,##0.00}", statementDetails.CommissionFee);
                                }
                                else
                                {
                                    if (statementDetails.CommissionFee == 0)
                                        statementDetails.CommissionFeeString = "";
                                    else
                                        statementDetails.CommissionFeeString = "+" + String.Format(CultureInfo.InvariantCulture, statementDetails.OperationCurrency == "AMD" ? "{0:#,##0.0}" : "{0:#,##0.00}", statementDetails.CommissionFee);
                                }
                            }

                            cardStatement.Transactions.Add(statementDetails);
                        }

                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                cardStatement.ReportingPeriod = dateFrom.ToString() + dateTo.ToString();
                                cardStatement.InitialBalance = double.Parse(dr["begin_capital"].ToString());
                                cardStatement.FinalBalance = double.Parse(dr["rest_end"].ToString());
                                cardStatement.TotalDebitAmount = double.Parse(dr["debet_all"].ToString());
                                cardStatement.TotalCreditAmount = double.Parse(dr["credit_all"].ToString());
                                if (Convert.ToInt32(dr["card_type_number"]) == 20 || Convert.ToInt32(dr["card_type_number"]) == 23)
                                    cardStatement.Transactions.ForEach(x => x.HasMR = true);
                            }
                        }
                    }
                }
            }

            return cardStatement;
        }

        internal static Card GetCard(string cardNumber, ulong customerNumber)
        {
            Card card = null;


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,
                                cash_rate,loan_account,overdraft_account,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,                                
                                SMSApplicationPresent,related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and visa_number=@cardNumber and customer_Number=@customerNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetCard(row);
                    }

                }
            }

            return card;
        }

        internal static bool CheckCardOwner(string cardNumber, ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT app_id,visa_number as card_number
                                FROM Tbl_Visa_Numbers_Accounts vn
                                WHERE  visa_number=@cardNumber and customer_Number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        check = dr.HasRows;
                    }

                }
            }

            return check;
        }

        public static List<Card> GetLinkedCards(string cardNumber)
        {
            List<Card> cardList = new List<Card>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,interest_rate_effective
                                ,cash_rate,loan_account,overdraft_account,SMSApplicationPresent,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and  main_card_number=@cardNumber";



                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Card card = SetCard(row);

                        cardList.Add(card);
                    }



                }

            }
            return cardList;
        }

        internal static Card GetCard(string cardNumber)
        {
            Card card = null;


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,
                                cash_rate,loan_account,overdraft_account,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                SMSApplicationPresent,related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and visa_number=@cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetCard(row);
                    }

                }
            }

            return card;
        }

        public static int GetCardType(string cardNumber, ulong customerNumber)
        {
            int cardType = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT card_type
                                FROM Tbl_Visa_Numbers_Accounts 
                                WHERE  visa_number=@cardNumber and customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    cardType = Convert.ToInt32(cmd.ExecuteScalar());

                }
            }
            return cardType;
        }


        internal static bool IsOurCard(string cardNumber)
        {

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT count(1) FROM Tbl_Visa_Numbers_Accounts WHERE  visa_number=@cardNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                    conn.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                        return true;

                }
            }

            return false;
        }

        /// <summary>
        /// Վերադարձնում է քարտի սպասարկման վարձի գրաֆիկը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="openDate"></param>
        /// <returns></returns>
        public static List<CardServiceFeeGrafik> GetCardServiceFeeGrafik(string cardNumber, DateTime openDate)
        {
            //hard coded open date
            //DateTime hardCodedOpenDate= Convert.ToDateTime("02/24/2011"); 

            List<CardServiceFeeGrafik> cardServiceFeeGrafiklist = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                var sql = @"SELECT * 
                         FROM Tbl_Cards_Service_fee_payments 
                         where card_number = @cardNumber and open_date=@openDate  order by start_date asc";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@openDate", SqlDbType.DateTime).Value = openDate;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        cardServiceFeeGrafiklist = new List<CardServiceFeeGrafik>();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            CardServiceFeeGrafik cardServiceFeeGrafik = SetCardServiceFeeGrafik(row);
                            cardServiceFeeGrafiklist.Add(cardServiceFeeGrafik);
                        }
                    }

                }
            }

            return cardServiceFeeGrafiklist;
        }

        public static CardServiceFeeGrafik SetCardServiceFeeGrafik(DataRow row)
        {

            CardServiceFeeGrafik cardServiceFeeGrafik = new CardServiceFeeGrafik();

            if (row != null)
            {
                cardServiceFeeGrafik.PeriodStart = DateTime.Parse(row["start_date"].ToString());
                cardServiceFeeGrafik.PeriodEnd = DateTime.Parse(row["end_date"].ToString());
                cardServiceFeeGrafik.Currency = Convert.ToString(row["currency"]);
                cardServiceFeeGrafik.ServiceFee = Convert.ToDouble(row["Service_Fee"]);

            }
            return cardServiceFeeGrafik;


        }

        internal static CardTariff GetCardTariff(ulong productId, ulong customerNumber)
        {
            CardTariff cardTariff = new CardTariff();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                cash_rate,
                                cash_rate_other,
                                CashInFeeRate_ACBA, 
                                Cash_rate_int,
                                Min_fee_local ,
                                Min_fee_int,
                                CashInFeeRate_Other,
                                SMSFeeFromClient,
                                SMSFeeFromBank,
                                C2CFeeOur,
                                C2CFeeOther,
                                service_fee,
								open_date,
								validation_date,
                               	t.CardSystemID,
                                period
                                FROM Tbl_visa_numbers_accounts  vn
								INNER JOIN tbl_type_of_card t
								ON vn.card_type=t.id
								WHERE app_id=@productId and customer_Number=@customerNumber ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        cardTariff.CashRateOur = double.Parse(row["cash_rate"].ToString());
                        cardTariff.CashRateOther = double.Parse(row["cash_rate_other"].ToString());
                        cardTariff.CashInFeeRateOur = double.Parse(row["CashInFeeRate_ACBA"].ToString());
                        cardTariff.CashRateInternational = double.Parse(row["Cash_rate_int"].ToString());
                        cardTariff.MinFeeLocal = double.Parse(row["Min_fee_local"].ToString());
                        cardTariff.MinFeeInternational = double.Parse(row["Min_fee_int"].ToString());
                        cardTariff.CashInFeeRateOther = double.Parse(row["CashInFeeRate_Other"].ToString());
                        cardTariff.SMSFeeFromCustomer = double.Parse(row["SMSFeeFromClient"].ToString());
                        cardTariff.SMSFeeFromBank = double.Parse(row["SMSFeeFromBank"].ToString());
                        cardTariff.CardToCardFeeOur = double.Parse(row["C2CFeeOur"].ToString());
                        cardTariff.CardToCardFeeOther = double.Parse(row["C2CFeeOther"].ToString());
                        cardTariff.ServiceFee = row["service_fee"] != DBNull.Value ? float.Parse(row["service_fee"].ToString()) : 0;
                        cardTariff.CardSystem = int.Parse(row["CardSystemID"].ToString());
                        DateTime openDate = Convert.ToDateTime(row["open_date"].ToString());
                        DateTime validDate = Convert.ToDateTime(row["validation_date"].ToString());
                        cardTariff.CardValidityPeriod = (validDate.Year - openDate.Year) + (openDate.Month < validDate.Month ? 1 : 0);
                        cardTariff.Period = Convert.ToInt32(row["period"].ToString());
                    }

                }
            }

            return cardTariff;
        }

        internal static CardStatus GetCardStatus(ulong productId, ulong customerNumber)
        {
            CardStatus cardStatus = new CardStatus();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT GivenDate,GivenBy  FROM Tbl_VISA_applications WHERE app_id=@productId and customer_Number=@customerNumber ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["GivenBy"] != DBNull.Value && row["GivenDate"] != DBNull.Value)
                        {
                            cardStatus.Status = 1;
                            cardStatus.StatusDescription = "ՏՐ";
                            cardStatus.UserId = short.Parse(row["GivenBy"].ToString());
                            cardStatus.RegistrationDate = DateTime.Parse(row["GivenDate"].ToString());
                        }
                        else
                        {
                            cardStatus = initIncompleteCardStatus(productId, customerNumber);
                        }

                    }

                }
            }
            return cardStatus;
        }


        private static CardStatus initIncompleteCardStatus(ulong productId, ulong customerNumber)
        {
            CardStatus cardStatus = new CardStatus();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT va.AdditionValue AS reason,isnull(va.Number_Of_Set,0) AS userId,va1.AdditionValue AS registrationDate
                                 FROM Tbl_visa_numbers_accounts vn 
                                 LEFT JOIN (SELECT app_id,Number_Of_Set,AdditionValue FROM Tbl_VisaAppAdditions WHERE AdditionID=6) va ON vn.App_Id=va.app_id  
                                 LEFT JOIN (SELECT app_id,Number_Of_Set,AdditionValue FROM Tbl_VisaAppAdditions WHERE AdditionID=5) va1 ON vn.App_Id=va1.app_id  
                                WHERE va.app_id=@productId and customer_Number=@customerNumber ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        cardStatus.Status = 2;
                        cardStatus.StatusDescription = "ԹՏՐ";
                        cardStatus.UserId = short.Parse(row["userId"].ToString());
                        cardStatus.RegistrationDate = DateTime.ParseExact(row["registrationDate"].ToString(), "dd/mm/yy", CultureInfo.InvariantCulture);
                        cardStatus.Reason = Utility.ConvertAnsiToUnicode(row["reason"].ToString());
                    }
                    else
                    {
                        cardStatus.Status = 3;
                        cardStatus.StatusDescription = "ՉՏՐ";
                    }

                }
                return cardStatus;
            }
        }



        /// <summary>
        /// Քարտի վերաթողարկման առկայության ստուգում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static bool IsCardChanged(string cardNumber)
        {
            bool isChanged = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 1 FROM Tbl_Visa_Numbers_Accounts V
                                INNER JOIN Tbl_CardChanges C on V.App_Id=C.app_id and typeID=1 
                                WHERE closing_date is null and visa_number=@cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isChanged = true;
                    }
                }
            }
            return isChanged;
        }



        /// <summary>
        /// Քարտի նույն տեսակով վերաթողարկման առկայության ստուգում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static bool IsSameTypeCardChange(string cardNumber)
        {
            bool sameTypeChange = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_visa_applications v  
                                INNER JOIN Tbl_Visa_Numbers_Accounts vn ON v.app_id=vn.App_Id  
                                INNER JOIN Tbl_CardChanges cc ON cc.app_id=v.app_id AND cc.typeID=1  
                                INNER JOIN (SELECT cardType AS oldCardType, app_id FROM Tbl_VISA_applications) vold ON vold.app_id=cc.old_app_id and v.cardType=vold.oldCardType
                                WHERE closing_date is null AND  cardNumber= @cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            sameTypeChange = true;
                    }
                }
            }
            return sameTypeChange;
        }

        internal static bool IsNormCardStatus(string cardNumber)
        {
            bool isNorm = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_visa_applications WHERE cardstatus='NORM' AND cardnumber= @cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isNorm = true;
                    }
                }
            }
            return isNorm;
        }

        internal static bool IsCardRegistered(string cardNumber)
        {
            bool isRegistered = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT 1 FROM Tbl_Visa_Numbers_Accounts WHERE closing_date is null AND visa_number= @cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isRegistered = true;
                    }
                }
            }
            return isRegistered;
        }


        /// <summary>
        /// Վերադարձնում է քարտի տեսակը քարտի համարը և քարտի տեսակի նկարագրությունը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static Card GetCardWithOutBallance(string accountNumber)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT top 1 app_id,visa_number as card_number,card_type,closing_date,currency,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,
                                related_office_number,filialcode,validation_date,end_date,CardSystemID
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE vn.card_account=@accountNumber and vn.main_card_number is null order by vn.open_date desc ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        if (dr.Read())
                        {
                            card = new Card();
                            card.ProductId = long.Parse(dr["app_id"].ToString());
                            card.CardNumber = dr["card_number"].ToString();
                            card.Type = short.Parse(dr["card_type"].ToString());
                            card.CardType = dr["Card_Type_description"].ToString();
                            if (dr["closing_date"] != DBNull.Value)
                                card.ClosingDate = DateTime.Parse(dr["closing_date"].ToString());
                            card.Currency = dr["currency"].ToString();
                            card.RelatedOfficeNumber = ushort.Parse(dr["related_office_number"].ToString());
                            card.ValidationDate = DateTime.Parse(dr["validation_date"].ToString());
                            card.FilialCode = int.Parse(dr["filialcode"].ToString());
                            card.EndDate = DateTime.Parse(dr["end_date"].ToString());
                            card.CardSystem = int.Parse(dr["CardSystemID"].ToString());

                            card.CreditLine = null;
                            card.Overdraft = null;
                            card.SupplementaryType = SupplementaryType.Main; //PrintFastOverdraftContract-ի համար 
                        }

                    }


                }
            }


            return card;
        }

        internal static Card GetCardWithOutBallance(ulong productId)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT top 1 visa_number as card_number,card_type,closing_date,currency,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,filialcode,
                                related_office_number,vn.app_id
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE vn.app_id=@productId order by vn.open_date desc ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = new Card();
                        card.ProductId = long.Parse(row["app_id"].ToString());
                        card.CardNumber = row["card_number"].ToString();
                        card.Type = short.Parse(row["card_type"].ToString());
                        card.CardType = row["Card_Type_description"].ToString();
                        if (row["closing_date"] != DBNull.Value)
                            card.ClosingDate = DateTime.Parse(row["closing_date"].ToString());
                        card.Currency = row["currency"].ToString();
                        card.FilialCode = Convert.ToInt32(row["filialcode"]);
                        card.RelatedOfficeNumber = ushort.Parse(row["related_office_number"].ToString());
                        card.ProductId = Convert.ToInt64(row["app_id"]);

                    }

                }
            }

            return card;
        }
        internal static Card GetCardMainData(ulong productId, ulong customerNumber)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT vn.app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number)  as Card_Type_description,card_type,t.CardSystemID,filialcode,main_card_number,
                                cash_rate,loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,VA.card_receiving_type,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                INNER JOIN Tbl_VISA_applications VA ON VA.app_id = vn.app_id
                                WHERE vn.app_id=@productId and vn.customer_Number=@customerNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            card = new Card();
                            card.ProductId = long.Parse(dr["app_id"].ToString());
                            card.CardNumber = dr["card_number"].ToString();
                            card.Type = short.Parse(dr["card_type"].ToString());
                            card.CardType = dr["Card_Type_description"].ToString();
                            if (dr["closing_date"] != DBNull.Value)
                                card.ClosingDate = DateTime.Parse(dr["closing_date"].ToString());
                            card.Currency = dr["currency"].ToString();
                            card.FilialCode = Convert.ToInt32(dr["filialcode"]);
                            card.RelatedOfficeNumber = ushort.Parse(dr["related_office_number"].ToString());
                            card.PositiveRate = float.Parse(dr["positive_rate"].ToString());
                            if (dr["main_card_number"] != DBNull.Value)
                                card.MainCardNumber = dr["main_card_number"].ToString();
                            card.ValidationDate = DateTime.Parse(dr["validation_date"].ToString());
                            card.OpenDate = DateTime.Parse(dr["open_date"].ToString());
                        }
                    }



                }
            }

            return card;
        }

        /// <summary>
        /// Վերադարձնում է քարտի վրա եղած արգելանքի գումարը և հաշիվը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        internal static DAHKDetail GetCardDAHKDetails(string cardNumber)
        {
            DAHKDetail DAHK_Detail = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT account_number,amount FROM fn_get_card_DAHK_details(@card_number)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.VarChar, 16).Value = cardNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            DAHK_Detail = new DAHKDetail();
                            DAHK_Detail.Account = new Account();
                            DAHK_Detail.Account.AccountNumber = dr["account_number"].ToString();
                            DAHK_Detail.Amount = Convert.ToDouble(dr["amount"]);
                        }
                    }

                }

            }
            return DAHK_Detail;
        }

        internal static ulong GetReNewCardProductId(ulong productId, int filialCode)
        {
            ulong newProductId = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT C.app_id FROM Tbl_VISA_applications V INNER JOIN Tbl_CardChanges C 
                                    ON V.app_id =C.app_id AND typeID = 1 WHERE CardStatus ='NORM' AND V.app_id NOT IN (SELECT app_id FROM Tbl_Visa_Numbers_Accounts)  
                                    AND filial = @userFilial - 22000
                                    AND C.old_app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@userFilial", SqlDbType.Float).Value = filialCode;
                    conn.Open();
                    var temp = cmd.ExecuteScalar();
                    if (temp != null)
                        newProductId = Convert.ToUInt64(temp);
                }
            }
            return newProductId;
        }

        internal static string GetCardMotherName(ulong productId)
        {
            string motherName = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT cardnumber,MotherName  FROM Tbl_VISA_applications WHERE app_id=@productId";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["MotherName"] != DBNull.Value)
                        {
                            motherName = row["MotherName"].ToString();
                        }


                    }

                }
            }
            return motherName;
        }

        internal static short GetCardStatementReceivingType(string cardNumber)
        {
            short communicationType = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select top 1 [status] from tbl_visa_applications WHERE Cardnumber=@cardNumber order by regdate desc";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["status"] != DBNull.Value)
                        {
                            communicationType = Convert.ToByte(row["status"]);
                        }

                    }

                }
            }

            return communicationType;
        }

        internal static List<Card> GetAttachedCards(ulong productId, ulong customerNumber)
        {
            List<Card> cards = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT VV.app_id,V.customer_number FROM tbl_visa_numbers_accounts V 
                                    INNER JOIN tbl_visa_numbers_accounts VV  
                                    ON V.visa_number =VV.main_card_number AND V.closing_date IS NULL AND VV.closing_date IS NULL  
                                    WHERE V.app_id =@productId AND V.customer_number =@customerNumber  order by V.open_date desc ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        ulong cardAppId = ulong.Parse(row["app_id"].ToString());
                        ulong cardCustomerNumber = ulong.Parse(row["customer_number"].ToString());
                        cards.Add(Card.GetCard(cardAppId, cardCustomerNumber));
                    }
                }
            }

            return cards;
        }

        internal static CardServiceQualities GetCardUSSDService(ulong productId)
        {
            CardServiceQualities result = CardServiceQualities.NotRegistered;
            short arcaResponse = -1;
            short action = -1;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT TOP 1 USSD.action,arca_response FROM  Tbl_ArcaRequestHeaders H 
		                               INNER JOIN Tbl_cards_USSD_history USSD ON H.id =USSD.header_id 
		                               WHERE commandType=24 AND appid =@productId 
		                               AND isnull(deleted,0)<>1 
		                               ORDER BY USSD.id DESC ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["action"] != DBNull.Value)
                        {
                            action = short.Parse(row["action"].ToString());
                        }

                        if (row["arca_response"] != DBNull.Value)
                        {
                            arcaResponse = short.Parse(row["arca_response"].ToString());
                        }

                        if (action == -1)
                        {
                            result = CardServiceQualities.NotRegistered;//Քարտի համար USSD գրանցված չէ
                        }

                        if ((action == 1) && arcaResponse != 0)
                        {
                            result = CardServiceQualities.RegistrateNotConfirmedInArca; //Քարտի համար կա USSD Գրանցել հայտ , որը ARCA-ում դեռ հաստատված չէ
                        }

                        if (action == 1 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա USSD գրանցման հայտ , որը ARCA-ում հաստատված է
                        }

                        if ((action == 2) && arcaResponse != 0)
                        {
                            result = CardServiceQualities.TerminateNotConfirmedInArca; //Քարտի համար կա USSD Հանել հայտ , որը ARCA-ում դեռ հաստատված չէ
                        }

                        if (action == 2 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.TerminateConfirmedInArca; //Քարտի համար USSD "Հանել" հայտ, որը ARCA -ում հաստատված է
                        }

                        if (action == 3 && arcaResponse != 0)
                        {
                            result = CardServiceQualities.ChangeNotConfirmedInArca; //Քարտի համար USSD "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
                        }

                        if (action == 3 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.ChangeConfirmedInArca; //Քարտի համար USSD "Փոփոխել" հայտ, որը ARCA -ում հաստատված է
                        }


                    }

                }
            }
            return result;
        }


        public static Card3DSecureService GetCard3DSecureService(ulong productId)
        {

            Card3DSecureService card3DSecure = new Card3DSecureService();
            CardServiceQualities result = CardServiceQualities.NotRegistered;
            short arcaResponse = -1;
            short action = -1;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT TOP 1 s.card_number,s.email,s.action,arca_response FROM  Tbl_ArcaRequestHeaders H 
		                               INNER JOIN Tbl_cards_3DSecure_history s ON H.id =s.header_id 
		                               WHERE commandType=25 AND appid =@productId 
		                               AND isnull(deleted,0)<>1 
		                               ORDER BY s.id DESC ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["action"] != DBNull.Value)
                        {
                            action = short.Parse(row["action"].ToString());
                        }

                        if (row["arca_response"] != DBNull.Value)
                        {
                            arcaResponse = short.Parse(row["arca_response"].ToString());
                        }

                        if (action == -1)
                        {
                            result = CardServiceQualities.NotRegistered;//Քարտի համար USSD գրանցված չէ
                        }

                        if ((action == 1) && arcaResponse != 0)
                        {
                            result = CardServiceQualities.RegistrateNotConfirmedInArca; //Քարտի համար կա 3DSecure Գրանցել հայտ , որը ARCA-ում դեռ հաստատված չէ
                        }

                        if (action == 1 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա 3DSecure գրանցման հայտ , որը ARCA-ում հաստատված է
                        }

                        if ((action == 2) && arcaResponse != 0)
                        {
                            result = CardServiceQualities.TerminateNotConfirmedInArca; //Քարտի համար կա 3DSecure Հանել հայտ , որը ARCA-ում դեռ հաստատված չէ
                        }

                        if (action == 2 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.TerminateConfirmedInArca; //Քարտի համար 3DSecure "Հանել" հայտ, որը ARCA -ում հաստատված է
                        }

                        if (action == 3 && arcaResponse != 0)
                        {
                            result = CardServiceQualities.ChangeNotConfirmedInArca; //Քարտի համար 3DSecure "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
                        }

                        if (action == 3 && arcaResponse == 0)
                        {
                            result = CardServiceQualities.ChangeConfirmedInArca; //Քարտի համար 3DSecure "Փոփոխել" հայտ, որը ARCA -ում հաստատված է
                        }
                        card3DSecure.Quality = result;
                        card3DSecure.ActionType = action;
                        card3DSecure.Email = row["email"].ToString();
                        card3DSecure.CardNumber = row["card_number"].ToString();

                    }

                }
            }
            return card3DSecure;
        }


        /// <summary>
        /// Ստանում ենք SMS ծառայության տեսակը իր մանրամասնություններով
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        //internal static CardServiceQualities GetPlasticCardSMSService(string cardNumber)
        //{
        //    CardServiceQualities result = CardServiceQualities.NotRegistered;
        //    short arcaResponse = -1;
        //    short action = -1;
        //    string value = "";

        //    using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
        //    {
        //        string sql = @"Select  top 1 SMS.action, RES.responseCode, value  from Tbl_cards_SMS_history SMS
        //	   INNER JOIN Tbl_ArcaResponse RES ON RES.requestID=SMS.HeaderID
        //	     WHERE Card_Number = @cardNumber 
        //		 AND isnull(deleted,0)<>1 
        //                         ORDER BY RES.id DESC ";

        //        using (SqlCommand cmd = new SqlCommand(sql, conn))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
        //            conn.Open();

        //            DataTable dt = new DataTable();

        //            using (SqlDataReader dr = cmd.ExecuteReader())
        //            {
        //                dt.Load(dr);
        //            }

        //            if (dt.Rows.Count > 0)
        //            {
        //                DataRow row = dt.Rows[0];
        //                if (row["action"] != DBNull.Value)
        //                    action = short.Parse(row["action"].ToString());

        //                if (row["responseCode"] != DBNull.Value)
        //                    arcaResponse = short.Parse(row["responseCode"].ToString());

        //                if (row["value"] != DBNull.Value)
        //                    value = row["value"].ToString();

        //                if (action == -1 || (action == 1 && arcaResponse == 0 && (value == "W0" || value == "H0" || value == "F0")))
        //                    result = CardServiceQualities.NotRegistered;//Քարտի համար SMS գրանցված չէ
        //                else if ((action == 1) && arcaResponse != 0)
        //                    result = CardServiceQualities.RegistrateNotConfirmedInArca; //Քարտի համար կա SMS Գրանցել հայտ , որը ARCA-ում դեռ հաստատված չէ
        //                else if (action == 1 && arcaResponse == 0)
        //                    result = CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա SMS-ի գրանցման հայտ , որը ARCA-ում հաստատված է
        //                else if ((action == 2 || (action == 3 && (value == "W0" || value == "H0" || value == "F0"))) && arcaResponse != 0)
        //                    result = CardServiceQualities.TerminateNotConfirmedInArca; //Քարտի համար կա SMS Դադարեցնել հայտ , որը ARCA-ում դեռ հաստատված չէ
        //                else if ((action == 2 || (action == 3 && (value == "W0" || value == "H0" || value == "F0"))) && arcaResponse == 0)
        //                    result = CardServiceQualities.TerminateConfirmedInArca; //Քարտի համար SMS "դադարեցնել" հայտ, որը ARCA -ում հաստատված է
        //                else if (action == 3 && arcaResponse != 0)
        //                    result = CardServiceQualities.ChangeNotConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
        //                else if (action == 3 && arcaResponse == 0)
        //                    result = CardServiceQualities.ChangeConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված է

        //            }
        //            else
        //                return CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա SMS-ի գրանցման հայտ , որը ARCA-ում հաստատված է
        //        }
        //    }
        //    return result;
        //}

        internal static CardServiceQualities GetPlasticCardSMSService(string cardNumber, bool IsSourceBank = false)
        {
            CardServiceQualities result = CardServiceQualities.NotRegistered;
            short arcaResponse = -1;
            short action = -1;
            string value = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"Select  top 1 SMS.action, RES.responseCode, value  from Tbl_cards_SMS_history SMS
									   INNER JOIN Tbl_ArcaResponse RES ON RES.requestID=SMS.HeaderID
									     WHERE Card_Number = @cardNumber 
										 AND isnull(deleted,0)<>1 
		                               ORDER BY RES.id DESC ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["action"] != DBNull.Value)
                            action = short.Parse(row["action"].ToString());

                        if (row["responseCode"] != DBNull.Value)
                            arcaResponse = short.Parse(row["responseCode"].ToString());

                        if (row["value"] != DBNull.Value)
                            value = row["value"].ToString();
                        if (!IsSourceBank)
                        {
                            if (action == -1 || (action == 1 && arcaResponse == 0 && (value == "W0" || value == "H0" || value == "F0")))
                                result = CardServiceQualities.NotRegistered;//Քարտի համար SMS գրանցված չէ
                            else if ((action == 1) && arcaResponse != 0)
                                result = CardServiceQualities.RegistrateNotConfirmedInArca; //Քարտի համար կա SMS Գրանցել հայտ , որը ARCA-ում դեռ հաստատված չէ
                            else if (action == 1 && arcaResponse == 0)
                                result = CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա SMS-ի գրանցման հայտ , որը ARCA-ում հաստատված է
                            else if ((action == 2 || (action == 3 && (value == "W0" || value == "H0" || value == "F0"))) && arcaResponse != 0)
                                result = CardServiceQualities.TerminateNotConfirmedInArca; //Քարտի համար կա SMS Դադարեցնել հայտ , որը ARCA-ում դեռ հաստատված չէ
                            else if ((action == 2 || (action == 3 && (value == "W0" || value == "H0" || value == "F0"))) && arcaResponse == 0)
                                result = CardServiceQualities.TerminateConfirmedInArca; //Քարտի համար SMS "դադարեցնել" հայտ, որը ARCA -ում հաստատված է
                            else if (action == 3 && arcaResponse != 0)
                                result = CardServiceQualities.ChangeNotConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
                            else if (action == 3 && arcaResponse == 0)
                                result = CardServiceQualities.ChangeConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված է
                        }
                        else
                        {
                            if (action == -1)
                                result = CardServiceQualities.NotRegistered;//Քարտի համար SMS գրանցված չէ
                            else if ((action == 1) && arcaResponse != 0)
                                result = CardServiceQualities.RegistrateNotConfirmedInArca; //Քարտի համար կա SMS Գրանցել հայտ , որը ARCA-ում դեռ հաստատված չէ
                            else if (action == 1 && arcaResponse == 0)
                                result = CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա SMS-ի գրանցման հայտ , որը ARCA-ում հաստատված է
                            else if (action == 2 && arcaResponse != 0)
                                result = CardServiceQualities.TerminateNotConfirmedInArca; //Քարտի համար կա SMS Դադարեցնել հայտ , որը ARCA-ում դեռ հաստատված չէ
                            else if (action == 2 && arcaResponse == 0)
                                result = CardServiceQualities.TerminateConfirmedInArca; //Քարտի համար SMS "դադարեցնել" հայտ, որը ARCA -ում հաստատված է
                            else if (action == 3 && arcaResponse != 0)
                                result = CardServiceQualities.ChangeNotConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված չէ
                            else if (action == 3 && arcaResponse == 0)
                                result = CardServiceQualities.ChangeConfirmedInArca; //Քարտի համար SMS "Փոփոխել" հայտ, որը ARCA -ում հաստատված է
                        }
                    }
                    else
                        return CardServiceQualities.RegistrateConfirmedInArca; //Քարտի համար կա SMS-ի գրանցման հայտ , որը ARCA-ում հաստատված է
                }
            }
            return result;
        }

        internal static string GetEmbossingName(string cardNumber, ulong productId)
        {
            string cardHolder;
            if (Card.IsOurCard(cardNumber))
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    string sql = @"SELECT  VA.embossingname FROM [dbo].[Tbl_VISA_applications] AS VA 
                                JOIN [Tbl_Visa_Numbers_Accounts] AS VN
                                ON VA.app_id = VN.app_id 
                                WHERE cardNumber = @cardNumber and closing_date IS NULL";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                        if (productId != 0)
                        {
                            sql += "AND productId = @productId";
                            cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                        }
                        conn.Open();

                        try
                        {
                            return cardHolder = cmd.ExecuteScalar().ToString().ToUpper();
                        }
                        catch
                        {
                            cardHolder = null;
                        }
                    }
                }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string sql = @"SELECT TOP 1 embossing_name FROM tbl_cardtocard_order_details D inner join TBl_HB_Documents H on D.doc_id = H.doc_id
									WHERE credit_card_number = @cardNumber and H.quality = 30 ";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                        conn.Open();

                        try
                        {
                            return cardHolder = cmd.ExecuteScalar().ToString().ToUpper();
                        }
                        catch
                        {
                            cardHolder = null;
                        }
                    }
                }
            }
            return cardHolder;
        }
        internal static string GetAttachedCardEmbossingName(string cardNumber)
        {
            string cardHolder;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT TOP 1 ob.carholder_name FROM (SELECT card_number, carholder_name FROM [tbl_other_bank_cards] WHERE is_completed = 1 UNION ALL SELECT card_number, carholder_name FROM [tbl_other_bank_cards_deleted]) ob WHERE card_number = @cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                    conn.Open();

                    try
                    {
                        return cardHolder = cmd.ExecuteScalar().ToString().ToUpper();
                    }
                    catch
                    {
                        cardHolder = null;
                    }
                }
            }

            return cardHolder;
        }
        internal static double GetCardToCardTransferFee(string debitCardNumber, double amount)
        {
            const double accFee = 0.003;

            double fee;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT C2CFeeOur FROM [dbo].[Tbl_Visa_Numbers_Accounts] WHERE visa_number = @visa_number";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@visa_number", SqlDbType.VarChar, 20).Value = debitCardNumber;

                    conn.Open();

                    //                          issfee                            accfee
                    fee = (Convert.ToDouble(cmd.ExecuteScalar()) * amount) + (amount * accFee);
                }
            }
            return fee;
        }


        internal static CardStatementAddInf GetFullCardStatement(CardStatement statement, string cardNumber, DateTime dateFrom, DateTime dateTo, byte language)
        {
            CardStatementAddInf addInf = new CardStatementAddInf();
            addInf.AddInfoCardStatementTransactions = new List<AddInfoCardStatement>();
            addInf.LinkCardStatementTransactions = new List<LinkCardStatement>();
            addInf.MRCardStatementTransactions = new List<MRCardStatement>();
            addInf.SummaryBonusCardStatementTransactions = new List<SummaryBonusCardStatement>();
            addInf.SummaryCreditLineCardStatementTransactions = new List<SummaryCreditLineCardStatement>();
            addInf.UnsettledTransactionsTransactions = new List<UnsettledTransactions>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("card_statement_Main", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@card", SqlDbType.NVarChar, 50).Value = cardNumber;
                    cmd.Parameters.Add("@fil", SqlDbType.SmallInt).Value = 22000;
                    cmd.Parameters.Add("@gu_id", SqlDbType.NVarChar, 100).Value = "0";

                    cmd.CommandTimeout = 120;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dr.NextResult();
                        int i = 0;
                        while (dr.Read())
                        {
                            decimal operationAmount = dr["Operation_amount"] != DBNull.Value ? Convert.ToDecimal(dr["Operation_amount"]) : 0;
                            decimal operationAmountCardCurrency = dr["Operation_amount_card_currency"] != DBNull.Value ? Convert.ToDecimal(dr["Operation_amount_card_currency"]) : 0;
                            decimal feeConvertation = dr["Fee_convertation"] != DBNull.Value ? Convert.ToDecimal(dr["Fee_convertation"]) : 0;
                            if (statement.Transactions.Count > i)
                            {
                                statement.Transactions[i].OperationAmountCardCurrency = operationAmountCardCurrency + feeConvertation;
                                statement.Transactions[i].CashBack = dr["cashback"] != DBNull.Value ? Convert.ToDecimal(dr["cashback"].ToString()) : 0;
                                if (operationAmount == 0)
                                    statement.Transactions[i].ExchangeRate = 0;
                                else
                                {
                                    if (operationAmount != operationAmountCardCurrency)
                                    {
                                        if (operationAmountCardCurrency + feeConvertation > operationAmount)
                                            statement.Transactions[i].ExchangeRate = operationAmountCardCurrency + feeConvertation / operationAmount;
                                        else
                                            statement.Transactions[i].ExchangeRate = operationAmount / operationAmountCardCurrency + feeConvertation;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                            i++;
                        }
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.UnsettledTransactions);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.BankDate);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.Summery);
                        dr.NextResult();
                        dr.NextResult();
                        dr.NextResult();
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.MR);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.SummaryCreditLine);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.SummaryBonus);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.AddInfo);
                        dr.NextResult();
                        addInf = SummeryCardStatement(addInf, dr, CardStatementDataTypes.Link);
                        addInf.MainCardStatement = statement;
                        return addInf;
                    }
                }
            }
        }

        private static CardStatementAddInf SummeryCardStatement(CardStatementAddInf statement, SqlDataReader dr, CardStatementDataTypes metods)
        {


            switch (metods)
            {
                case CardStatementDataTypes.MR:
                    if (dr.Read())
                    {
                        MRCardStatement mr = new MRCardStatement();

                        if (dr["InitialPoints"] != DBNull.Value)
                            mr.InitialPoints = Convert.ToDecimal(dr["InitialPoints"].ToString());

                        if (dr["EarnedPoints"] != DBNull.Value)
                            mr.EarnedPoints = Convert.ToDecimal(dr["EarnedPoints"].ToString());

                        if (dr["RedeemedPoints"] != DBNull.Value)
                            mr.RedeemedPoints = Convert.ToDecimal(dr["RedeemedPoints"].ToString());

                        if (dr["FinalPoints"] != DBNull.Value)
                            mr.FinalPoints = Convert.ToDecimal(dr["FinalPoints"].ToString());

                        statement.MRCardStatementTransactions.Add(mr);
                    }
                    break;
                case CardStatementDataTypes.SummaryCreditLine:
                    while (dr.Read())
                    {
                        SummaryCreditLineCardStatement creditLine = new SummaryCreditLineCardStatement();

                        if (dr["descr_arm"] != DBNull.Value)
                            creditLine.DescrArm = dr["descr_arm"].ToString();

                        if (dr["descr_eng"] != DBNull.Value)
                            creditLine.DescrEng = dr["descr_eng"].ToString();

                        if (dr["link_index_arm"] != DBNull.Value)
                            creditLine.LinkIndexArm = Convert.ToInt16(dr["link_index_arm"].ToString());

                        if (dr["link_index_eng"] != DBNull.Value)
                            creditLine.LinkIndexEng = Convert.ToInt16(dr["link_index_eng"].ToString());

                        if (dr["unit_arm"] != DBNull.Value)
                            creditLine.UnitArm = dr["unit_arm"].ToString();

                        if (dr["unit_eng"] != DBNull.Value)
                            creditLine.UnitEng = dr["unit_eng"].ToString();

                        if (dr["value"] != DBNull.Value)
                            creditLine.Value = dr["value"].ToString();

                        if (dr["isred"] != DBNull.Value)
                            creditLine.IsRed = Convert.ToBoolean(dr["isred"].ToString());

                        statement.SummaryCreditLineCardStatementTransactions.Add(creditLine);
                    }
                    break;
                case CardStatementDataTypes.SummaryBonus:
                    while (dr.Read())
                    {
                        SummaryBonusCardStatement BonusCard = new SummaryBonusCardStatement();

                        if (dr["descr_arm"] != DBNull.Value)
                            BonusCard.DescrArm = dr["descr_arm"].ToString();

                        if (dr["descr_eng"] != DBNull.Value)
                            BonusCard.DescrEng = dr["descr_eng"].ToString();

                        if (dr["link_index_arm"] != DBNull.Value)
                            BonusCard.LinkIndexArm = Convert.ToInt16(dr["link_index_arm"].ToString());

                        if (dr["link_index_eng"] != DBNull.Value)
                            BonusCard.LinkIndexEng = Convert.ToInt16(dr["link_index_eng"].ToString());

                        if (dr["unit_arm"] != DBNull.Value)
                            BonusCard.UnitArm = dr["unit_arm"].ToString();

                        if (dr["unit_eng"] != DBNull.Value)
                            BonusCard.UnitEng = dr["unit_eng"].ToString();

                        if (dr["value"] != DBNull.Value)
                            BonusCard.Value = dr["value"].ToString();

                        statement.SummaryBonusCardStatementTransactions.Add(BonusCard);
                    }
                    break;
                case CardStatementDataTypes.AddInfo:
                    while (dr.Read())
                    {
                        AddInfoCardStatement AddInfo = new AddInfoCardStatement();

                        if (dr["descr_arm"] != DBNull.Value)
                            AddInfo.DescrArm = dr["descr_arm"].ToString();

                        if (dr["descr_eng"] != DBNull.Value)
                            AddInfo.DescrEng = dr["descr_eng"].ToString();

                        if (dr["link_index_arm"] != DBNull.Value)
                            AddInfo.LinkIndexArm = Convert.ToInt16(dr["link_index_arm"].ToString());

                        if (dr["link_index_eng"] != DBNull.Value)
                            AddInfo.LinkIndexEng = Convert.ToInt16(dr["link_index_eng"].ToString());

                        if (dr["period_arm"] != DBNull.Value)
                            AddInfo.PeriodArm = dr["period_arm"].ToString();

                        if (dr["period_eng"] != DBNull.Value)
                            AddInfo.PeriodEng = dr["period_eng"].ToString();

                        if (dr["value"] != DBNull.Value)
                            AddInfo.Value = dr["value"].ToString();

                        if (dr["isred"] != DBNull.Value)
                            AddInfo.IsRed = Convert.ToBoolean(dr["isred"].ToString());

                        statement.AddInfoCardStatementTransactions.Add(AddInfo);
                    }
                    break;
                case CardStatementDataTypes.UnsettledTransactions:
                    while (dr.Read())
                    {
                        UnsettledTransactions unsettled = new UnsettledTransactions();

                        if (dr["operation_date"] != DBNull.Value)
                            unsettled.DateOfTransaction = Convert.ToDateTime(dr["operation_date"].ToString());
                        if (dr["operation_amount"] != DBNull.Value)
                            unsettled.TransactionAmount = Convert.ToDecimal(dr["operation_amount"].ToString());
                        if (dr["operation_currency"] != DBNull.Value)
                            unsettled.Currency = dr["operation_currency"].ToString();
                        if (dr["operation_amount_in_card_currency"] != DBNull.Value)
                            unsettled.TransactionAmountSignIn = Convert.ToDecimal(dr["operation_amount_in_card_currency"].ToString());
                        if (dr["fee_for_conv_total"] != DBNull.Value)
                            unsettled.TransactionAmountOut = Convert.ToDecimal(dr["fee_for_conv_total"].ToString());
                        if (dr["operation_place"] != DBNull.Value)
                            unsettled.OperationPlace = dr["operation_place"].ToString();
                        if (dr["Transaction_type"] != DBNull.Value)
                            unsettled.TransactionType = dr["Transaction_type"].ToString();

                        statement.UnsettledTransactionsTransactions.Add(unsettled);
                    }
                    break;
                case CardStatementDataTypes.Summery:
                    if (dr.Read())
                    {
                        if (dr["WholePenalty"] != DBNull.Value)
                            statement.WholePenalty = Convert.ToInt32(dr["WholePenalty"].ToString());
                        if (dr["WholeInterest"] != DBNull.Value)
                            statement.WholeInterest = Convert.ToDecimal(dr["WholeInterest"].ToString());
                        if (dr["WholeFee"] != DBNull.Value)
                            statement.WholeFee = Convert.ToDecimal(dr["WholeFee"].ToString());
                        if (dr["AvailableAmountTotal"] != DBNull.Value)
                            statement.AvailableAmountTotal = Convert.ToDecimal(dr["AvailableAmountTotal"].ToString());
                        if (dr["addCards"] != DBNull.Value)
                            statement.AddCard = dr["addCards"] != DBNull.Value ? dr["addCards"].ToString() : null;
                        if (dr["oper_day"] != DBNull.Value)
                            statement.OperDay = Convert.ToDateTime(dr["oper_day"]);
                        if (dr["cardSystemId"] != DBNull.Value)
                            statement.CardSystemId = Convert.ToInt32(dr["cardSystemId"]);

                    }
                    break;
                case CardStatementDataTypes.Link:
                    while (dr.Read())
                    {
                        LinkCardStatement Link = new LinkCardStatement();
                        if (dr["index_arm"] != DBNull.Value)
                            Link.IndexArm = Convert.ToInt16(dr["index_arm"].ToString());
                        if (dr["index_eng"] != DBNull.Value)
                            Link.IndexEng = Convert.ToInt16(dr["index_eng"].ToString());
                        if (dr["Value_arm"] != DBNull.Value)
                            Link.ValueArm = dr["Value_arm"].ToString();
                        if (dr["Value_eng"] != DBNull.Value)
                            Link.ValueEng = dr["Value_eng"].ToString();
                        if (dr["link_type"] != DBNull.Value)
                            Link.LinkType = Convert.ToInt32(dr["link_type"].ToString());

                        statement.LinkCardStatementTransactions.Add(Link);
                    }
                    break;
                case CardStatementDataTypes.BankDate:
                    if (dr.Read())
                    {
                        if (dr["name"] != DBNull.Value && dr["lastname"] != DBNull.Value && dr["middlename"] != DBNull.Value)
                            statement.FullName = dr["name"].ToString() + dr["lastname"].ToString() + dr["middlename"].ToString();
                        if (dr["card_type"] != DBNull.Value)
                            statement.CardType = Convert.ToInt32(dr["card_type"].ToString());
                        if (dr["Currency"] != DBNull.Value)
                            statement.AccountCurrency = dr["Currency"].ToString();
                        if (dr["card_account"] != DBNull.Value)
                            statement.CardAccount = dr["card_account"].ToString();
                        if (dr["cardNumber"] != DBNull.Value)
                            statement.CardNumber = dr["cardNumber"].ToString();
                        if (dr["arca_limit_start"] != DBNull.Value)
                            statement.ArcaLimitStart = Convert.ToDecimal(dr["arca_limit_start"].ToString());
                        if (dr["arca_limit_end"] != DBNull.Value)
                            statement.ArcaLimitEnd = Convert.ToDecimal(dr["arca_limit_end"].ToString());
                        if (dr["start_capital"] != DBNull.Value)
                            statement.StartCapital = Convert.ToDecimal(dr["start_capital"].ToString());
                        if (dr["cardGroupHead"] != DBNull.Value)
                            statement.CardGroupHead = Convert.ToInt32(dr["cardGroupHead"]);

                    }
                    break;
                default:
                    break;
            }

            return statement;
        }


        public static int GetCardServicingFilialCode(ulong productID)
        {
            int filialCode = default(int);
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                string sql = @"SELECT TOP 1 filialcode
                                FROM Tbl_Visa_Numbers_Accounts 
                                WHERE app_id = @productID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productID", SqlDbType.Float).Value = productID;

                    conn.Open();

                    try
                    {
                        filialCode = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception e)
                    {
                        throw;
                    }

                }
            }
            return filialCode;
        }

        internal static ulong GetCardCustomerNumber(string cardNumber)
        {
            ulong customerNumber = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT TOP 1 customer_number FROM Tbl_Visa_Numbers_Accounts
								WHERE visa_number=@visa_number order by open_date desc";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@visa_number", SqlDbType.VarChar, 25).Value = cardNumber;

                    conn.Open();

                    customerNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }
            return customerNumber;
        }

        internal static List<Card> GetLinkedCards(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT vn.app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                               dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                               dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,cash_rate,
                               loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                               CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest,
                               last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                               related_office_number,end_date,open_date,add_inf,debt,attached_card
                               FROM Tbl_Visa_Numbers_Accounts vn
                               INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                INNER JOIN [Tbl_SupplementaryCards] S on vn.app_id = S.app_id
                               WHERE closing_date is null and  S.customer_number=@customerNumber and S.customer_number <> vn.customer_number 
                                and s.app_id <> s.Main_app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            Card card = SetCard(row);

                            cardList.Add(card);
                        }
                    }
                }
            }
            return cardList;
        }
        internal static List<Card> GetLinkedAndAttachedCards(ulong productId, ProductQualityFilter productFilter = ProductQualityFilter.Opened)
        {
            List<Card> cardList = new List<Card>();
            string filter = "";
            switch (productFilter)
            {
                case ProductQualityFilter.Opened:
                    filter = "and closing_date is null";
                    break;
                case ProductQualityFilter.Closed:
                    filter = "and closing_date is not null";
                    break;
                case ProductQualityFilter.All:
                    break;
                default:
                    return cardList;
            }
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT vn.app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,cash_rate,
                                loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
								INNER JOIN [dbo].[Tbl_SupplementaryCards] sc 
								on vn.app_id = sc.app_id
                                WHERE sc.main_app_id = @productId and vn.app_id <> sc.main_app_id " + filter;

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            Card card = SetCard(row);

                            cardList.Add(card);
                        }
                    }
                }
            }
            return cardList;
        }

        internal static CardAdditionalInfo GetCardAdditionalInfo(ulong productId, Languages language)
        {
            CardAdditionalInfo info = new CardAdditionalInfo();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select Mobile_home,R.description as report_Receiving_Type, R.description_eng as report_Receiving_Type_ENG, ISNULL([E-mail_home] ,'')  as Email 
                                from tbl_visa_applications A
                                LEFT JOIN tbl_types_of_card_report_receiving R
                                on A.Status = R.type_id
                                where app_id = @productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        info.SMSPhone = dt.Rows[0]["mobile_home"].ToString();
                        info.ReportReceivingType = (Languages)language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[0]["report_Receiving_Type"].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[0]["report_Receiving_Type_ENG"].ToString());
                        info.Email = dt.Rows[0]["Email"].ToString();
                    }
                }
            }
            return info;
        }

        internal static ActionResult SaveCVVNote(ulong productId, string CVVNote)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(CVVNote);
                    var encodedCVV = System.Convert.ToBase64String(plainTextBytes);

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_CVV_note";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@CVVNote", SqlDbType.NVarChar, 255).Value = encodedCVV;

                    cmd.Parameters.Add(new SqlParameter("@ret_code", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        internal static string GetCVVNote(ulong productId)
        {
            string CVVNote = "";
            string CVVNoteDecoded = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT * 
                               FROM Tbl_Customer_CVV_Notes WHERE app_id = @app_id ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        CVVNote = (row["CVV_Note"] != DBNull.Value) ? row["CVV_Note"].ToString() : String.Empty;

                        string s = CVVNote.Trim().Replace(" ", "+");
                        if (s.Length % 4 > 0)
                            s = s.PadRight(s.Length + 4 - s.Length % 4, '=');

                        byte[] data = System.Convert.FromBase64String(s);


                        CVVNoteDecoded = System.Text.ASCIIEncoding.ASCII.GetString(data);
                    }
                }
            }

            return CVVNoteDecoded;
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի հայտի համար հասանելի քարտերի ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        internal static List<Card> GetCardsForNewCreditLine(ulong customerNumber, OrderType orderType)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                //Արագ օվերդրաֆտ , ավանդի գրավով վարկային գիծ
                if (orderType == OrderType.FastOverdraftApplication || orderType == OrderType.CreditLineSecureDeposit)
                {
                    string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,cash_rate,
                                loan_account,overdraft_account,SMSApplicationPresent,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and  customer_number=@customerNumber and (main_card_number Is Null or attached_card = 2)
                                and vn.visa_number not in (select visa_number from Tbl_credit_lines where  loan_type <> 9 and quality <> 10)
                                and card_type not in (38,51)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                        conn.Open();

                        DataTable dt = new DataTable();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {

                            dt.Load(dr);
                        }

                        if (dt.Rows.Count > 0)
                            cardList = new List<Card>();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            Card card = SetCard(row);

                            cardList.Add(card);
                        }
                    }
                }

            }

            return cardList;
        }
        internal static long GetCardProductId(string cardNumber, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT app_id
                                FROM Tbl_Visa_Numbers_Accounts
                                WHERE closing_date is null and visa_number= @visaNumber and customer_Number= @customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@visaNumber", SqlDbType.NVarChar, 20).Value = cardNumber;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return Convert.ToInt64(dr["app_id"].ToString());
                        }
                    }
                }
            }
            return 0;
        }

        internal static Card SetSupplementaryCard(Card card)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT top 1 customer_number
                                   FROM [dbo].[Tbl_SupplementaryCards]
                                   WHERE  app_id = @app_id and app_id <> main_app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = card.ProductId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["customer_number"].ToString() == GetCardCustomerNumber(card.CardNumber).ToString() || string.IsNullOrEmpty(dt.Rows[0]["customer_number"].ToString()))
                        {
                            card.SupplementaryType = SupplementaryType.Attached;
                        }
                        else
                        {
                            card.SupplementaryType = SupplementaryType.Linked;
                            card.SupplementaryCustomerName = Utility.GetCustomerDescription(Convert.ToUInt64(dt.Rows[0]["customer_number"].ToString()));
                        }
                    }
                    else
                    {
                        card.SupplementaryType = SupplementaryType.Main;
                    }

                }
            }
            return card;
        }


        internal static List<Card> GetNotActivatedVirtualCards(ulong customerNumber)
        {
            List<Card> cards = new List<Card>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT A.app_id, A.cardType, ISNULL(cvv.cvv, '') as cvv
                                FROM Tbl_VISA_applications A 
								INNER JOIN tbl_card_order_details D ON A.app_id = D.app_id 
								INNER JOIN Tbl_HB_documents H on D.doc_id = H.doc_id
								LEFT JOIN tbl_CVV2Infos cvv on cvv.app_ID = A.app_id
                                WHERE A.cardtype = 51 AND A.givendate IS NULL AND CardStatus = 'NORM' 
								AND H.quality = 30 AND A.customer_number = @customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Card card = new Card();
                            card.ProductId = Convert.ToInt64(dt.Rows[i]["app_id"].ToString());
                            card.Type = Convert.ToInt16(dt.Rows[i]["cardType"].ToString());
                            card.RealCVV = dt.Rows[i]["CVV"].ToString();
                            card.SupplementaryType = SupplementaryType.Main;
                            cards.Add(card);
                        }
                    }

                }
            }
            return cards;
        }

        internal static ulong GetCardProductIdByAccountNumber(string cardAccountNumber, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT TOP 1 App_Id 
                               FROM tbl_visa_numbers_accounts
                               WHERE closing_date IS NULL 
	                               and customer_number = @customerNumber 
	                               and card_account = @cardAccountNumber
	                               and main_card_number IS NULL";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@cardAccountNumber", SqlDbType.NVarChar, 20).Value = cardAccountNumber;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return Convert.ToUInt64(dr["app_id"].ToString());
                        }
                    }
                }
            }
            return 0;
        }

        internal static List<Card> GetClosedCardsForDigitalBanking(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string sql = @"SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,
                                dbo.from_visa_number_balance(visa_number,dbo.get_oper_day()) as Balance,main_card_number,interest_rate_effective,
                                cash_rate,loan_account,overdraft_account,SMSApplicationPresent,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is not null and 
								( 
									(SELECT balance FROM [tbl_all_accounts;] WHERE arm_number = vn.card_account) > 0
									OR debt <> 0
									OR  vn.visa_number in (select visa_number from tbl_credit_lines)  
								) 
                                AND (vn.card_account not in(select card_Account From Tbl_Visa_Numbers_Accounts Where closing_date is null and customer_number = @customerNumber)
								AND vn.visa_number not in (select visa_number FROM Tbl_Visa_Numbers_Accounts WHERE closing_date is null and customer_number = @customerNumber ))
								AND vn.customer_number=@customerNumber";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Card card = SetCard(row);

                        cardList.Add(card);
                    }
                }
            }

            cardList = cardList.OrderByDescending(m => m.ClosingDate).ToList();

            cardList.ForEach(m =>
            {
                cardList.FindAll(c => c.CardAccount.AccountNumber == m.CardAccount.AccountNumber && c.ClosingDate < m.ClosingDate).ForEach(x =>
                {
                    x.Balance = 0;
                });
            });

            return cardList;
        }

        internal static int GetCardSystem(string cardNumber)
        {
            int cardSystem = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT top 1 t.CardSystemID
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE visa_number=@cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        cardSystem = int.Parse(row["CardSystemID"].ToString());
                    }

                }
            }

            return cardSystem;
        }

        public static string GetCardNumber(long productId)
        {
            string cardNumber = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT top 1 CardNumber
                                FROM Tbl_Visa_applications 
                                WHERE app_id = @productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        cardNumber = row["cardNumber"].ToString();
                    }

                }
            }

            return cardNumber;
        }



        public static bool HasVirtualCard(ulong productId)
        {
            bool has = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_virtual_cards WHERE issuer_card_ref_id = @productID ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productID", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            has = true;
                    }
                }
            }
            return has;
        }

        internal static int SaveVirtualCardUpdateRequest(ulong productId, VirtualCardRequestTypes type, short statusChangeAction, int userId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_save_virtual_card_update_request";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@requestType", SqlDbType.TinyInt).Value = type;
                    cmd.Parameters.Add("@virtualCardStatus", SqlDbType.TinyInt).Value = statusChangeAction;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    return Convert.ToInt32(cmd.Parameters["@id"].Value);
                }
            }
        }

        internal static ActionResult ReSendVirtualCardRequest(int requestId, int userId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = "UPDATE tbl_virtual_cards_update_requests SET send_date = null,set_user = @userId WHERE id = @requestId ";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    cmd.Parameters.Add("@requestId", SqlDbType.Int).Value = requestId;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }

        internal static async Task<List<Card>> GetCardsForTotalBalance(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string sql = @"SELECT visa_number as card_number,currency 
                                FROM Tbl_Visa_Numbers_Accounts vn
                                WHERE closing_date is null and  customer_number=@customerNumber
                                AND main_card_number IS NULL";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<Card>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Card card = new Card();
                        card.Currency = row["Currency"].ToString();
                        card.CardNumber = row["Card_Number"].ToString();
                        cardList.Add(card);
                    }
                }
            }

            return cardList;
        }

        public static string GetExDate(string cardNumber)
        {
            string exDate;

            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandText = "Select dbo.fnc_getCardExpireDate (@cardNumber)";
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    exDate = cmd.ExecuteScalar().ToString();
                }

            }
            return exDate;
        }

        public static string GetCardNumberWithCreditLineAppId(ulong appId, CredilLineActivatorType reason)
        {
            string cardNumber;

            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    if (reason == CredilLineActivatorType.ActivateCreditLine)
                        cmd.CommandText = "SELECT visa_number FROM tbl_credit_lines WHERE App_Id = @appId";
                    else
                        cmd.CommandText = "SELECT visa_number FROM Tbl_closed_credit_lines WHERE App_Id = @appId";


                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = appId;
                    cardNumber = cmd.ExecuteScalar().ToString();
                }

            }
            return cardNumber;
        }
        public static ulong GetCardAppId(string cardNumber)
        {
            ulong appId;

            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandText = "select app_id from Tbl_VISA_applications where cardnumber = @cardNumber and CardStatus ='NORM'";
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    appId = (ulong)Convert.ToInt64(cmd.ExecuteScalar());
                }

            }
            return appId;
        }
        public static string GetCardExpiryDateForArca(string cardNumber)
        {
            string expiryDate = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT FORMAT(DATEADD(MONTH,-1,end_date),'yyyyMM') expiry_date FROM tbl_visa_numbers_accounts WHERE visa_number=@cardNumber";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;

                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    expiryDate = dr["expiry_date"].ToString();
                }
            }

            return expiryDate;
        }

        internal static bool CheckCardIsClosed(string cardNumber)
        {
            bool check = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 closing_date FROM Tbl_Visa_Numbers_Accounts
                                                        WHERE visa_number=@card_number  ORDER BY open_date DESC", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar).Value = cardNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["closing_date"] == DBNull.Value)
                            {
                                check = false;
                            }
                        }
                    }
                }
            }
            return check;
        }
        public static string GetCardAccountNumber(string cardNumber)
        {
            string cardAccountNumber;

            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandText = "select arcaAccountNumber from Tbl_VISA_applications where cardnumber = @cardNumber";
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    cardAccountNumber = cmd.ExecuteScalar().ToString();
                }

            }
            return cardAccountNumber;
        }
        internal static List<Card> GetOtherBankAttachedCards(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                cardList.AddRange(GetOtherBankAttachedCardsAsync(customerNumber, conn).Result);
            }
            return cardList;
        }
        internal static async Task<List<Card>> GetOtherBankAttachedCardsAsync(ulong customerNumber)
        {
            List<Card> cardList = new List<Card>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                cardList.AddRange(await GetOtherBankAttachedCardsAsync(customerNumber, conn));
            }
            return cardList;
        }

        internal static async Task<List<Card>> GetOtherBankAttachedCardsAsync(ulong customerNumber, SqlConnection conn)
        {
            List<Card> cardList = new List<Card>();

            string script = @"SELECT DISTINCT ob.id, ob.card_number, ob.binding_Id, ob.expire_date, ob.carholder_name, ob.bank_name FROM tbl_other_bank_cards ob WHERE ob.customer_number = @customerNumber And ob.quality in (2,3) AND card_number is not null";

            using (SqlCommand cmd = new SqlCommand(script, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
               using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                while (dr.Read())
                {
                    cardList.Add(new Card
                    {
                        CardNumber = dr["card_number"].ToString(),
                        CardType = "Կցված քարտ",
                        AttachedCardBankName = dr["bank_name"].ToString(),
                        AttachedCardId = int.Parse(dr["id"].ToString()),
                        CardAccount = new Account
                        {
                            AccountNumber = dr["card_number"].ToString(),
                            AccountDescription = dr["carholder_name"].ToString().ToUpper(),
                            AccountType = 11,
                            AccountTypeDescription = "Կցված քարտ",
                            AccountTypeDescriptionEng = "Attached Card",
                            BindingId = dr["binding_Id"].ToString(),
                            IsAttachedCard = true
                        },
                        ValidationDate = DateTime.Parse(dr["expire_date"].ToString())
                    });
                }
            }
            return cardList;
        }

        internal static string GetCardTechnology(ulong productId)
        {
            string cardTechnology = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT armenian 
                                    FROM Tbl_type_of_Card_Technology Tech
                                    INNER JOIN tbl_type_of_card T
                                    ON t.C_M = Tech.abbreviation
                                    INNER JOIN tbl_visa_applications VA
                                    ON va.cardType = t.ID
                                    WHERE va.app_id = @productId";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["armenian"] != DBNull.Value)
                        {
                            cardTechnology = Utility.ConvertAnsiToUnicode(row["armenian"].ToString());
                        }
                    }
                }
            }
            return cardTechnology;
        }

        public static string GetCardHolderFullName(ulong productId)
        {
            string fullName = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT  nameEng + ' ' + LastNameEng AS fullName 
                                    FROM V_CustomerDesriptionDocs VD
			                        INNER JOIN Tbl_SupplementaryCards SC
			                        ON SC.customer_number =	VD.customer_number
			                        WHERE SC.app_id= @productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        fullName = row["fullName"].ToString();
                    }

                }
            }
            return fullName;
        }

        public static ulong GetMainCardProductId(string notMainCardNumber)
        {
            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                cmd.CommandText = @"select app_id  from tbl_visa_numbers_accounts
								where visa_number = (
										select top 1 main_card_number 
										from tbl_visa_numbers_accounts
										where visa_number = @cardNumber and  visa_number <> main_card_number  and closing_date is null
								)  and closing_date is null";
                cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = notMainCardNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return Convert.ToUInt64(dr["app_id"].ToString());
                }
            }
            return 0;
        }

        internal static Card GetCardMainData(string cardNumber)
        {
            Card card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT app_id,visa_number as card_number,currency ,validation_date,card_account,closing_date,closedby,closingreason,
                                dbo.Fnc_CardTypeFull(visa_number) as Card_Type_description,card_type,t.CardSystemID,filialcode,main_card_number,
                                cash_rate,loan_account,overdraft_account,interest_rate_effective,
                                CONVERT(float,ROUND(positive_rate,case when currency='AMD' then 1 else 2 end)) positive_rate, CONVERT(float,ROUND(total_positive_rate,2)) total_positive_rate, positive_interest, 
                                last_day_of_pos_rate_calculation, last_day_of_pos_rate_repair, date_of_stopping_pos_rate_calculation,
                                SMSApplicationPresent,related_office_number,end_date,open_date,add_inf,debt,attached_card
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                WHERE closing_date is null and visa_number=@cardNumber ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            card = new Card();
                            card.ProductId = long.Parse(dr["app_id"].ToString());
                            card.CardNumber = dr["card_number"].ToString();
                            card.Type = short.Parse(dr["card_type"].ToString());
                            card.CardType = dr["Card_Type_description"].ToString();
                            if (dr["closing_date"] != DBNull.Value)
                                card.ClosingDate = DateTime.Parse(dr["closing_date"].ToString());
                            card.Currency = dr["currency"].ToString();
                            card.FilialCode = Convert.ToInt32(dr["filialcode"]);
                            card.RelatedOfficeNumber = ushort.Parse(dr["related_office_number"].ToString());
                            card.PositiveRate = float.Parse(dr["positive_rate"].ToString());
                            if (dr["main_card_number"] != DBNull.Value)
                                card.MainCardNumber = dr["main_card_number"].ToString();
                            card.ValidationDate = DateTime.Parse(dr["validation_date"].ToString());
                            card.OpenDate = DateTime.Parse(dr["open_date"].ToString());
                        }
                    }



                }
            }

            return card;
        }

        internal static List<string> GetArmenianCardsBIN()
        {
            List<string> BINs = new List<string>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT card_Range FROM Tbl_armenian_banks_card_ranges WHERE bank_code <> 16000";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    BINs.Add(dr["card_range"].ToString());
                }
            }
            return BINs;
        }
        /// <summary>
        /// Վերադարձնում է քարտապանի customerNumber-ը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static ulong GetCardHolderCustomerNumber(long productId)
        {
            ulong cardHolderCustomerNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT customer_number FROM Tbl_SupplementaryCards WHERE app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();

                    var temp = cmd.ExecuteScalar();

                    if (temp != null)
                    {
                        cardHolderCustomerNumber = Convert.ToUInt64(temp);
                    }

                    return cardHolderCustomerNumber;
                }
            }
        }

        public static string GetCardHolderData(ulong productId, string dataType)
        {
            string holderData = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ";

                if (dataType.Equals("name"))
                {
                    sql += " C.nameEng AS holderData ";
                }
                else if (dataType.Equals("lastName"))
                {
                    sql += " C.LastNameEng AS holderData ";
                }
                else if (dataType.Equals("fullName"))
                {
                    sql += " C.nameEng + ' ' + C.LastNameEng AS holderData ";
                }

                sql += @"FROM Tbl_VISA_applications V 
                                        LEFT JOIN Tbl_SupplementaryCards S 
						                ON V.app_id  = S.app_id
                                        LEFT JOIN (SELECT nameEng , LastNameEng, customer_number FROM V_CustomerDesription ) C 
						                ON C.customer_number = CASE WHEN ISNULL(S.app_id, 0) = 0 THEN V.customer_number ELSE S.customer_number END 
						                WHERE V.app_id = @productId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        holderData = Utility.ConvertAnsiToUnicode(row["holderData"].ToString());
                    }
                }
            }
            return holderData;
        }

        internal static string GetCardExpireDateActivatedInArCa(string cardNumber)
        {
            string exDate;

            using (SqlConnection cnn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandText = "Select dbo.fnc_getCardExpireDateActivatedInArCa (@cardNumber)";
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    exDate = cmd.ExecuteScalar().ToString();
                }

            }
            return exDate;
        }


        internal static List<CardRetainHistory> GetCardRetainHistory(string cardNumber)
        {
            List<CardRetainHistory> historyList = new List<CardRetainHistory>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "pr_get_card_retain_history";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@card_number", SqlDbType.VarChar, 16).Value = cardNumber;
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                CardRetainHistory history = new CardRetainHistory();
                                history.TerminalId = Convert.ToInt32(dr["P_1_TERMINAL_ID"].ToString());
                                history.ATMDescription = dr["description"].ToString();
                                if (dr["Filial"] != DBNull.Value)
                                    history.FillialCode = dr["Filial"].ToString();
                                history.HistoryDate = Convert.ToDateTime(dr["P_4_DATE"]).ToString("dd/MM/yyyy");
                                history.HistoryTime = dr["P_5_TIME"].ToString();
                                history.Amount = Convert.ToDouble(dr["P_18_SUMMA"].ToString());
                                history.CaptStatus = dr["CAPT_STATUS"].ToString();
                                historyList.Add(history);
                            }
                        }
                    }
                }
            }
            return historyList;
        }
        public static string GetCustomerEmailByCardNumber(string cardNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqltext = @"	select ISNULL(emailAddress,'') from tbl_emails e
	                                join Tbl_Customer_Emails ce on e.id=ce.emailId
	                                join Tbl_Customers c on ce.identityId=c.identityId
	                                where customer_number=(	SELECT  distinct case when   b.customer_number is null then a.Customer_Number else b.customer_number end
	                                FROM      Tbl_VISA_applications a
	                                LEFT JOIN [dbo].[Tbl_SupplementaryCards] b on a.app_id=b.app_id	
	                                where cardnumber=@cardNumber ) and priority=1";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                    var email = cmd.ExecuteScalar();
                    return email == null ? null : email.ToString();
                }
            }

        }
    }
}
