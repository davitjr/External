using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;
using System.Data;
using System.Linq;

namespace ExternalBanking
{
    public class Loan : LoanProduct
    {
        /// <summary>
        /// Կուտակված սպասարկման վճար
        /// </summary>
        public double TotalFee { get; set; }

        /// <summary>
        /// Սուբսիդվող տոկոս
        /// </summary>
        public double SubsidiaCurrentRateValue { get; set; }

        /// <summary>
        /// Սպասարկման վճար
        /// </summary>
        public double CurrentFee { get; set; }

        /// <summary>
        /// Վճարված սպասարկման վճար
        /// </summary>
        public double MaturedCurrentFee { get; set; }

        /// <summary>
        /// Վարկի տեսակ
        /// </summary>
        public short LoanType { get; set; }

        /// <summary>
        /// Վարկի տեսակի նկարագրություն
        /// </summary>
        public string LoanTypeDescription { get; set; }

        /// <summary>
        /// Տարեկան սուբսիդվող տոկոս
        /// </summary>
        public double SubsidiaInterestRate { get; set; }

        /// <summary>
        /// Վաղաժամկետ մարման դեպքում տույժ
        /// </summary>
        public double AdvancedRepaymentRate { get; set; }

        /// <summary>
        /// Փոփոխվող
        /// </summary>
        public short ChangeRate { get; set; }

        /// <summary>
        /// Առաջին փուլի գումար
        /// </summary>
        public double FirstStartCapital { get; set; }

        /// <summary>
        /// Երկրորդ փուլի սկիզբ 
        /// </summary>
        public DateTime? DateOfBeginningSecondPeriod { get; set; }

        /// <summary>
        /// Հերթական մարման տվյալներ
        /// </summary>
        public LoanRepaymentGrafik NextRepayment { get; set; }


        /// <summary>
        /// Էֆֆեկտիվ տոկոսադրույք առանց հաշվի սպասարկման միջնորդավճարի
        /// </summary>
        public double InterestRateEffectiveWithoutAccountServiceFee { get; set; }

        /// <summary>
        /// Վաղաժամկետ մարման դեպքում տւույժի իսկություն
        /// </summary>
        public bool CheckAdvancedRepaymentRate { get; set; }

        /// <summary>
        /// Վարկի տեսակի նկարագրություն /անգլերեն/
        /// </summary>
        public string LoanTypeDescriptionEng { get; set; }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք վարկը տրամադրված է 24/7 եղանակով, թե ոչ
        /// </summary>
        public bool Is_24_7 { get; set; }

      

        public Loan()
        {
            LoanAccount = new Account();
        }

        public static Loan GetLoan(ulong productId, ulong customerNumber)
        {
            Loan loan = LoanDB.GetLoan(productId, customerNumber);

            if (loan == null)
                loan = LoanDB.GetAparikTexumLoan(productId, customerNumber);

            if (loan != null)
            {
                loan.NextRepayment = loan.GetLoanNextRepayment();
            }

            if (loan == null)
                loan = Loan.GetLoans(customerNumber, ProductQualityFilter.Closed).Find(m => m.ProductId == (long)productId);

            return loan;
        }

        public static List<Loan> GetLoans(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Loan> loans = new List<Loan>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                loans.AddRange(LoanDB.GetLoans(customerNumber));
                loans.AddRange(LoanDB.GetAparikTexumLoans(customerNumber));

            }
            if (filter == ProductQualityFilter.Closed)
            {
                loans.AddRange(LoanDB.GetClosedLoans(customerNumber));
                loans.AddRange(LoanDB.GetAparikTexumClosedLoans(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                loans.AddRange(LoanDB.GetLoans(customerNumber));
                loans.AddRange(LoanDB.GetAparikTexumLoans(customerNumber));
                loans.AddRange(LoanDB.GetClosedLoans(customerNumber));
                loans.AddRange(LoanDB.GetAparikTexumClosedLoans(customerNumber));
            }

            loans.FindAll(m => m.Quality != 40).ForEach(loan =>
            {
                loan.NextRepayment = loan.GetLoanNextRepayment();
            });

            return loans;
        }

        public static List<Loan> GetAparikTexumLoans(ulong customerNumber)
        {
            List<Loan> loans = new List<Loan>();
            loans = LoanDB.GetAparikTexumLoans(customerNumber);
            return loans;
        }
        /// <summary>
        /// Վարկի գրաֆիկ
        /// </summary>
        /// <returns></returns>
        public List<LoanRepaymentGrafik> GetLoanGrafik()
        {
            return LoanDB.GetLoanGrafik(this);
        }

        /// <summary>
        /// Վարկի սկզբնական գրաֆիկ
        /// </summary>
        /// <returns></returns>
        public List<LoanRepaymentGrafik> GetLoanInceptiveGrafik(ulong customerNumber)
        {
            return LoanDB.GetLoanInceptiveGrafik(this, customerNumber);
        }

        /// <summary>
        /// Վարկի մարման գրաֆիկ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public List<LoanRepaymentGrafik> GetLoanRepayments(ulong customerNumber)
        {
            return LoanDB.GetLoanRepayments(this, customerNumber);
        }

        /// <summary>
        /// Վարկի հերթական մարման տվյալներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public LoanRepaymentGrafik GetLoanNextRepayment()
        {
            List<LoanRepaymentGrafik> loanRepayments = LoanDB.GetLoanGrafik(this);

            if (loanRepayments == null)
            {
                return new LoanRepaymentGrafik();
            }

            DateTime operDay = Utility.GetCurrentOperDay();
            return loanRepayments.Find(m => m.RepaymentDate >= operDay);
        }


        /// <summary>
        /// Թողնվող մնացորդ
        /// </summary>
        /// <returns></returns>
        public static double GetLoanCalculatedRest(Loan loan, ulong customerNumber, MatureType matureType)
        {
            double leftBalance = 0, rateRepayment = 0, capitalRepayment = 0, balance = 0, rateRepaymentAMD = 0;
            DateTime repaymentDate;

            loan = Loan.GetLoan((ulong)loan.ProductId, customerNumber);

            if (matureType == MatureType.RateRepayment)
            {
                rateRepayment = Math.Round(loan.InpaiedRestOfRate) + Math.Round(loan.PenaltyRate * -1) + Math.Round(loan.PenaltyAdd);
                if (loan.Currency != "AMD")
                {
                    double cbKurs = Utility.GetCBKursForDate(Utility.GetCurrentOperDay(), loan.Currency);
                    rateRepaymentAMD = (Math.Round(loan.InpaiedRestOfRate) * cbKurs) + (Math.Round(loan.PenaltyRate * -1) * cbKurs) + (Math.Round(loan.PenaltyAdd) * cbKurs);
                }

                if (loan.LoanType == 38 || !Validation.IsDAHKAvailability(customerNumber))
                    balance = Account.GetAcccountAvailableBalance(loan.ConnectAccount.AccountNumber.ToString(), Utility.GetCurrentOperDay(), loan.Currency);
                leftBalance = balance - capitalRepayment - rateRepayment;

                if (leftBalance < 0)
                    leftBalance = 0;
                if (loan.Quality == 11 || loan.Quality == 5 || loan.Quality == 2 || loan.OutCapital != 0)
                {
                    return 0;
                }

                return leftBalance;
            }



            if (loan.Quality == 2)
            {

                rateRepayment = Math.Round(loan.InpaiedRestOfRate) + Math.Round(loan.PenaltyRate * -1) + Math.Round(loan.PenaltyAdd) + Math.Round(loan.PenaltyAdd);
                if (loan.Currency != "AMD")
                {
                    double cbKurs = Utility.GetCBKursForDate(Utility.GetCurrentOperDay(), loan.Currency);
                    rateRepaymentAMD = (Math.Round(loan.InpaiedRestOfRate) * cbKurs) + (Math.Round(loan.PenaltyRate * -1) * cbKurs) + (Math.Round(loan.PenaltyAdd) * cbKurs) + (Math.Round(loan.PenaltyAdd) * cbKurs);
                }

                if (loan.LoanType == 38 || !Validation.IsDAHKAvailability(customerNumber))
                    balance = Account.GetAcccountAvailableBalance(loan.ConnectAccount.AccountNumber.ToString(), Utility.GetCurrentOperDay(), loan.Currency);
                leftBalance = balance - capitalRepayment - rateRepayment;

                if (leftBalance < 0)
                    leftBalance = 0;



                return leftBalance;

            }
            else
            {
                List<LoanRepaymentGrafik> listLoanRepaymentGrafik = loan.GetLoanRepayments(customerNumber);
                DateTime operDay = Utility.GetCurrentOperDay();
                LoanRepaymentGrafik lrg = listLoanRepaymentGrafik.Find(m => m.RepaymentDate >= operDay);
                if (lrg == null)
                {
                    lrg = new LoanRepaymentGrafik();
                    lrg.RepaymentDate = operDay;
                }

                repaymentDate = lrg.RepaymentDate;

                rateRepayment = Math.Abs(Math.Round(loan.CurrentRateValue * -1) + Math.Round(loan.InpaiedRestOfRate)) +
                    Math.Round(Math.Abs((loan.PenaltyRate * -1) + loan.PenaltyAdd)) + loan.PenaltyAdd;


                capitalRepayment = listLoanRepaymentGrafik.Where(item => item.RepaymentDate <= repaymentDate).Sum(item => item.CapitalRepayment) -
                    listLoanRepaymentGrafik.Where(item => item.RepaymentDate <= loan.EndDate).Sum(item => item.CapitalRepayment) + loan.CurrentCapital;

                balance = Account.GetAcccountAvailableBalance(loan.ConnectAccount.AccountNumber.ToString(), operDay, loan.Currency, false);

                if (lrg != null && matureType == MatureType.PartialRepaymentByGrafik)
                {
                    lrg.CapitalRepayment = 0;
                    lrg.RateRepayment = 0;
                    lrg.FeeRepayment = 0;
                    balance = 0;
                }

                if (capitalRepayment < 0)
                {
                    if (repaymentDate != operDay)
                    {
                        double daysCount = (repaymentDate - operDay).TotalDays;
                        balance = balance - rateRepayment;
                        if (balance < 0)
                        {
                            return leftBalance = 0;
                        }
                        if (Math.Abs(loan.CurrentCapital) < balance)
                        {
                            return leftBalance = 0;
                        }
                        else
                        {
                            leftBalance = Convert.ToInt64(((Math.Abs(loan.CurrentCapital) - balance) * daysCount * loan.InterestRate) / (365 - daysCount * loan.InterestRate));
                            return leftBalance;
                        }
                    }


                }
                else if (capitalRepayment == 0)
                {
                    return 0;
                }



                leftBalance = balance - capitalRepayment - rateRepayment - (loan != null ? Math.Round(loan.CurrentFee, 0) : 0);
                if (leftBalance < 0)
                    leftBalance = 0;


            }

            return leftBalance;
        }

        /// <summary>
        /// Վարկի գլխավոր պայմանագիր
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static LoanMainContract GetLoanMainContract(ulong productId, ulong customerNumber)
        {
            return LoanDB.GetLoanMainContract(productId, customerNumber);
        }

        /// <summary>
        /// Վարկի միջնորդավճար և այլ մուծումներ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<ProductOtherFee> GetProductOtherFees(ulong productId, ulong customerNumber)
        {
            return LoanDB.GetProductOtherFees(productId, customerNumber);
        }


        /// <summary>
        ///  Ապառիկի տվյալներ
        /// </summary>
        /// <returns></returns>
        public static List<GoodsDetails> GetGoodsDetails(ulong productId, ulong customerNumber)
        {
            return LoanDB.GetGoodsDetails(productId, customerNumber);
        }

        /// <summary>
        /// Վարկի տվյալներ
        /// </summary>
        /// <param name="loanFullNumber"></param>
        /// <returns></returns>
        public static Loan GetLoan(string loanFullNumber)
        {
            Loan loan = LoanDB.GetLoan(loanFullNumber);
            return loan;
        }

        /// <summary>
        /// Վարկի տվյալներ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Loan GetLoan(ulong productId)
        {
            Loan loan = LoanDB.GetLoan(productId);
            return loan;
        }

        public static ulong GetSurchargeAppId(ulong productId)
        {
            return LoanDB.GetSurchargeAppId(productId);
        }
        public static string GetLiabilitiesAccountNumberByAppId(ulong appId)
        {
            return LoanDB.GetLiabilitiesAccountNumberByAppId(appId);
        }
        public static string GetLoanTypeDescriptionEng(string loanFullNumber)
        {
            return LoanDB.GetLoanTypeDescriptionEng(loanFullNumber);
        }

        /// <summary>
        /// Դրամական միջոցների գրավի պայմանագիր
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static byte[] LoansDramContract(string accountNumber)
        {
            return LoanDB.LoansDramContract(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է վարկի քաղվածքը նշված ժամանակահատվածում և նշված լեզվով
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static LoanStatement GetStatement(string account, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0, byte langId = 1)
        {
            LoanStatement loanStatement = LoanDB.GetStatement(account, dateFrom, dateTo, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc, langId);
            return loanStatement;
        }

        public static bool HasUploadedLoanContract(string loanAccountNumber)
        {
            return LoanDB.HasUploadedLoanContract(loanAccountNumber);
        }
        /// <summary>
        /// Ստուգում է արդյոք հաճախորդը ունի Վճարված ապահովագրություն վարկ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckCutomerHasPaidInsurance(ulong customerNumber, ulong productId)
        {
            return LoanDB.CheckCutomerHasPaidInsurance(customerNumber, productId);
        }

        /// <summary>
        /// Վերադարձնում է վարկի տեսակը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static short? GetLoanType(ulong productId)
        {
            return LoanDB.GetLoanType(productId);
        }

        /// <summary>
        /// Ստուգում է վարկ գոյություն ունի թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckLoanExists(ulong customerNumber, ulong productId)
        {
            return LoanDB.CheckLoanExists(customerNumber, productId);
        }

        public static bool IsAbleToApplyForLoan(ulong customerNumber, LoanProductType type)
        {
            bool isAble = false;
            if (type == LoanProductType.Loan)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);
                if (currentAccounts.Count > 0) { isAble = true; return isAble; }
            }
            else if (type == LoanProductType.FastOverdraft)
            {
                List<Card> cards = Card.GetCards(customerNumber, ProductQualityFilter.Opened);
                cards.RemoveAll(m => m.Type == 6 || m.Type == 38 || m.Type == 52);
                if (cards.Count > 0) { isAble = true; return isAble; }
            }
            else if (type == LoanProductType.CreditLine)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);
                List<Card> cards = Card.GetCards(customerNumber, ProductQualityFilter.Opened);
                cards.RemoveAll(m => m.Type == 6 || m.Type == 38);
                if (cards.Count > 0 && currentAccounts.Count > 0) { isAble = true; return isAble; }
            }
            return isAble;
        }
        public static void PostResetEarlyRepaymentFee(ulong productId, string description, bool recovery, short setNumber)
        {
            LoanDB.PostResetEarlyRepaymentFee(productId, description, recovery, setNumber);
        }

        public static bool GetResetEarlyRepaymentFeePermission(ulong productId)
        {
            return LoanDB.GetResetEarlyRepaymentFeePermission(productId);
        }

        public static bool IsLoan_24_7(ulong productId)
        {
            return LoanDB.IsLoan_24_7(productId);
        }

        public static double GetMaxAvailableAmountForNewLoan(string provisionCurrency, ulong customerNumber)
        {
            return LoanDB.GetMaxAvailableAmountForNewLoan(provisionCurrency, customerNumber);
        }

        public static string GetLoanAccountNumber(ulong productId, ulong customerNumber)
        {
            return LoanDB.GetLoanAccountNumber(productId, customerNumber);
        }

        public static double GetLoanOrderAcknowledgement(long docId) => LoanDB.GetLoanOrderAcknowledgement(docId);

    }
}
