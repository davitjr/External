using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;
using System.Threading.Tasks;
using System.Data;

namespace ExternalBanking
{
    public class CreditLine:LoanProduct
    {
        /// <summary>
        /// Վարկային գծի տեսակ
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// Վարկային գծի տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Վարկային գծի փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Չօգտագործված գումարի դիմաց կուտակված տոկոս
        /// </summary>
        public double CurrentRateValueUnused { get; set; }

        /// <summary>
        /// Չօգտագործված գումարի դիմաց կուտակված տոկոս որից վճարված
        /// </summary>
        public double MaturedCurrentRateValueUnused { get; set; }

        /// <summary>
        /// Տարեկան տոկոսադրույք չօգտ․մասի դիմաց
        /// </summary>
        public double InterestRateNused { get; set; }

        /// <summary>
        /// Վարկային գծի ավարտի ա/թ
        /// </summary>
        public DateTime? DateOfCreditLineStopping { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }


        /// <summary>
        /// Քարտի վարկային գիծ
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static CreditLine GetCardCreditLine(string cardNumber)
        {
            return CreditLineDB.GetCardCreditLine(cardNumber);
        }

        public static async Task<CreditLine> GetCardCreditLineAsync(string cardNumber)
        {
            return await CreditLineDB.GetCardCreditLineAsync(cardNumber);
        }

        /// <summary>
        /// Հաճախորդի վարկային գիծ/գծեր
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<CreditLine> GetCreditLines(ulong customerNumber,ProductQualityFilter filter)
        {
            List<CreditLine> creditLines = new List<CreditLine>();
            if (filter==ProductQualityFilter.Opened || filter==ProductQualityFilter.NotSet)
            {
                creditLines.AddRange(CreditLineDB.GetCreditLines(customerNumber));
            }

            if (filter==ProductQualityFilter.Closed)
            {
                creditLines.AddRange(CreditLineDB.GetClosedCreditLines(customerNumber));
            }

            if (filter==ProductQualityFilter.All)
            {
                creditLines.AddRange(CreditLineDB.GetCreditLines(customerNumber));
                creditLines.AddRange(CreditLineDB.GetClosedCreditLines(customerNumber));
            }

            return creditLines;
        }
        /// <summary>
        /// Քարտի վարկային գծի գրաֆիկ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<CreditLineGrafik> GetCreditLineGrafik(ulong productId)
        {
            return CreditLineDB.GetCreditLineGrafik(productId);
        }
        /// <summary>
        /// Մեկ Վարկային գծի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static CreditLine GetCreditLine(ulong productId, ulong customerNumber)
        {
            return CreditLineDB.GetCreditLine(productId, customerNumber);
        }

        /// <summary>
        /// Մեկ Վարկային գծի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static CreditLine GetCreditLine(string loanFullNumber)
        {
            return CreditLineDB.GetCreditLine(loanFullNumber);
        }
        /// <summary>
        /// Քարտի գերածախսի տվյալներ
        /// </summary>
        /// <param name="cardNumber">քարտի համար</param>
        /// <returns></returns>
        public static CreditLine GetCardOverdraft(string cardNumber)
        {
            return CreditLineDB.GetCardOverDraft(cardNumber);

        }

        public static async Task<CreditLine> GetCardOverdraftAsync(string cardNumber)
        {
            return await CreditLineDB.GetCardOverDraftAsync(cardNumber);

        }

        /// <summary>
        /// Վարկային գծի դադարեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondTermination(ulong customerNumber, string orderNumber)
        {
            bool secondTermination = CreditLineDB.IsSecondTermination(customerNumber, orderNumber);
            return secondTermination;
        }

        public static List<CreditLine> GetCardClosedCreditLines(ulong customerNumber, string cardNumber)
        {
            return CreditLineDB.GetCardClosedCreditLines(customerNumber,cardNumber);
        }

        public static List<LoanMainContract> GetCreditLineMainContract(ulong customerNumber)
        {
            return CreditLineDB.GetCreditLineMainContract(customerNumber);
        }

        /// <summary>
        /// Դադարեցված վարկային գծի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static CreditLine GetClosedCreditLine(ulong productId, ulong customerNumber)
        {
            return CreditLineDB.GetClosedCreditLine(productId, customerNumber);
        }


        /// <summary>
        /// Վարկային գծի սահմանաչափի նվազեցումներ
        /// </summary>
        /// <returns></returns>
        public List<LoanRepaymentGrafik> GetDecreaseLoanGrafik()
        {
            return CreditLineDB.GetDecreaseLoanGrafik(this.ProductId);
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի քարտի համարը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static string GetCreditLineCardNumber(ulong productId)
        {
            return CreditLineDB.GetCreditLineCardNumber(productId);
        }



        public static DataTable GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType)
        {
            return CreditLineDB.GetCreditLinePrecontractData(startDate, endDate, interestRate, repaymentPercent, cardNumber, currency, amount, loanType);
        }
        public static bool HasActiveCreditLineForCardAccount(string cardAccount)
        {
            return CreditLineDB.HasActiveCreditLineForCardAccount(cardAccount);
        }

        public static string GetCardTypeName(string cardNumber)
        {
            return CreditLineDB.GetCardTypeName(cardNumber);
        }

        public static byte[] GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber)
        {
            return CreditLineDB.GetLoansDramContract(docId, productType, fromApprove, customerNumber);
        }

        public static bool HasUploadedCreditLineContract(string loanAccountNumber)
        {
            return CreditLineDB.HasUploadedCreditLineContract(loanAccountNumber);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void UpdateCreditLinesFnameOnline(ulong productId)
        {
            CreditLineDB.UpdateCreditLinesFnameOnline(productId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void UpdateCloseCreditLinesFnameOnline(ulong productId)
        {
            CreditLineDB.UpdateCloseCreditLinesFnameOnline(productId);
        }
        public static double GetCreditLineBallance(ulong productId)
        {
            return CreditLineDB.GetCreditLineballance(productId);
        }
        
        public static void SaveCreditLineByApiGate(long docId, double productId,ulong orderId)
        {
            CreditLineDB.SaveCreditLineByApiGate(docId, productId,orderId);
        }
        public static bool IsCreditLineActivateOnApiGate(long docId)
        {
            return CreditLineDB.IsCreditLineActivateOnApiGate(docId);
        }
        public static void UpdateCreditLinesFnameNull(ulong appId)
        {
            CreditLineDB.UpdateCreditLinesFnameNull(appId);
        }
        public static bool IsProlongApiGate(ulong appId)
        {
            return CreditLineDB.IsProlongApiGate(appId);
        }
        public static double GetArcaLimitBallance(ulong appId)
        {
            return CreditLineDB.GetArcaLimitBallance(appId);
        }

    }

}
