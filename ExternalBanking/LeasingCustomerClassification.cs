using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LeasingCustomerClassification
    {
        public int Id { get; set; }

        public int LeasingCustomerNumber { get; set; }

        public int LinkedCustomerNumber { get; set; }

        public DateTime DateOfBeginning { get; set; }

        /// <summary>
        /// Դասակարգման համարը
        /// </summary>
        public int ClassificationID { get; set; }

        /// <summary>
        /// Վարկի ունիկալ համարը
        /// </summary>
        public long AppId { get; set; }

        private string customerFullName;
        /// <summary>
        /// Ֆիզ. անձի անուն ազգանունը կամ ոչ  Ֆիզ. անձի նկարագրությունը
        /// </summary>
        public string CustomerFullName
        {
            get { return customerFullName; }
            set { customerFullName = Utility.ConvertAnsiToUnicode(value); }
        }


        private string infoDocument;
        /// <summary>
        /// Ֆիզ. անձի անձնագրի տվյալները կամ ոչ  Ֆիզ. անձի ՀՎՀՀ-ն
        /// </summary>
        public string InfoDocument
        {
            get { return infoDocument; }
            set { infoDocument = Utility.ConvertAnsiToUnicode(value); }
        }



        /// <summary>
        /// Գրանցման ամսաթիվը
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        private string registrationDateString;
        public string RegistrationDateString
        {
            get { return registrationDateString = RegistrationDate == DateTime.MinValue ? "" : RegistrationDate.ToString("dd/MM/yyyy"); }
            set { }
        }


        /// <summary>
        /// Փակման ամսաթիվը
        /// </summary>
        public DateTime ClosingDate { get; set; }

        private string closingDateString;
        public string ClosingDateString
        {
            get { return closingDateString = ClosingDate == DateTime.MinValue ? "" : ClosingDate.ToString("dd/MM/yyyy"); }
            set { }
        }

        private string fileName;
        /// <summary>
        /// Ֆիզ. անձի անձնագրի տվյալները կամ ոչ  Ֆիզ. անձի ՀՎՀՀ-ն
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = Utility.ConvertAnsiToUnicode(value); }
        }



        private string classificationType;
        /// <summary>
        /// Դասակարգման տեսակը
        /// </summary>
        public string ClassificationType
        {
            get { return classificationType; }
            set { classificationType = Utility.ConvertAnsiToUnicode(value); }
        }


        /// <summary>
        /// Դասակարգման ամսաթիվը
        /// </summary>
        public DateTime ClassificationDate { get; set; }

        public int ClassificationTypeID { get; set; }

        private string classificationDateString;
        public string ClassificationDateString
        {
            get { return classificationDateString = ClassificationDate == DateTime.MinValue ? "" : ClassificationDate.ToString("dd/MM/yyyy"); }
            set { }
        }


        /// <summary>
        /// ժամկետանց օրեր
        /// </summary>
        public short OverdueDays { get; set; }


        private string description;
        /// <summary>
        /// Դասակարգման նկարագրություն(հիմք)
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = Utility.ConvertAnsiToUnicode(value); }
        }




        private string additionalDescription;
        /// <summary>
        /// Լրացուցիչ նկարագրություն
        /// </summary>
        public string AdditionalDescription
        {
            get { return additionalDescription; }
            set { additionalDescription = Utility.ConvertAnsiToUnicode(value); }
        }

        private string status;
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public string Status
        {
            get { return status; }
            set { status = Utility.ConvertAnsiToUnicode(value); }
        }


        /// <summary>
        /// Հաճախորդի համարը 
        /// </summary>
        public long? CustomerNumber { get; set; }


        private string reportNumber;
        /// <summary>
        /// Զեկուցագրի համարը
        /// </summary>
        public string ReportNumber
        {
            get { return reportNumber; }
            set { reportNumber = Utility.ConvertAnsiToUnicode(value); }
        }

        private string closingReporNumber;
        /// <summary>
        /// Փակման զեկուցագրի համարը
        /// </summary>
        public string ClosingReporNumber
        {
            get { return closingReporNumber; }
            set { closingReporNumber = Utility.ConvertAnsiToUnicode(value); }
        }



        /// <summary>
        /// Զեկուցագրի ամսաթիվը
        /// </summary>
        public DateTime ReportDate { get; set; }

        private string reportDateString;
        public string ReportDateString
        {
            get { return reportDateString = ReportDate == DateTime.MinValue ? "" : ReportDate.ToString("dd/MM/yyyy"); }
            set { }
        }

        /// <summary>
        /// Փակման զեկուցագրի ամսաթիվը
        /// </summary>
        public DateTime ClosingReportDate { get; set; }

        private string closingReportDateString;
        public string ClosingReportDateString
        {
            get { return closingReportDateString = ClosingReportDate == DateTime.MinValue ? "" : ClosingReportDate.ToString("dd/MM/yyyy"); }
            set { }
        }



        private string riskClassName;
        /// <summary>
        /// Դաս
        /// </summary>
        public string RiskClassName
        {
            get { return riskClassName; }
            set { riskClassName = Utility.ConvertAnsiToUnicode(value); }
        }


        private string classificationReason;
        /// <summary>
        /// Տեսակ (դասակարգման պատճառը)
        /// </summary>
        public string ClassificationReason
        {
            get { return classificationReason; }
            set { classificationReason = Utility.ConvertAnsiToUnicode(value); }
        }

        /// <summary>
        /// Հաշիվ/Հղման համար
        /// </summary>
        public long AccountOrLink { get; set; }

        /// <summary>
        ///  Փոխ. անձ
        /// </summary>
        public long? SubstitutePersonNumber { get; set; }

        /// <summary>
        /// Լիզինգառուներ
        /// </summary>
        public string BorrowerNumber { get; set; }

        /// <summary>
        /// հաշվեհամար
        /// </summary>
        public long Account { get; set; }

        /// <summary>
        /// Աշխատակցի Պ.Կ. -ն
        /// </summary>
        public int? SetNumber { get; set; }

        /// <summary>
        /// Փակող Պ.Կ. -ն
        /// </summary>
        public int? ClosedSetNumber { get; set; }

        public bool CalcByDays { get; set; }

        public static long GetLeasingCustomerNumber(int leasingCustomerNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerNumber(leasingCustomerNumber);
        }
        public static LeasingCustomerClassification GetLeasingCustomerInfo(long customerNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerInfo(customerNumber);
        }

        public static List<LeasingCustomerClassification> GetLeasingCustomerSubjectiveClassificationGrounds(long customerNumber, bool isActive)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerSubjectiveClassificationGrounds(customerNumber, isActive);
        }

        public static Dictionary<string, string> GetLeasingReasonTypes(short classificationType)
        {
            return LeasingCustomerClassificationDB.GetLeasingReasonTypes(classificationType);
        }

        public static Tuple<int, string> GetLeasingRiskDaysCountAndName(int riskClassCode)
        {
            return LeasingCustomerClassificationDB.LeasingRiskDaysCountAndName(riskClassCode);
        }

        public static ActionResult AddLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj, int overdueDays, DateTime registrationDate, int setNumber)
        {
            return LeasingCustomerClassificationDB.AddLeasingCustomerSubjectiveClassificationGrounds(obj, overdueDays, registrationDate, setNumber);
        }

        public static LeasingCustomerClassification GetLeasingCustomerSubjectiveClassificationGroundsByID(int Id)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerSubjectiveClassificationGroundsByID(Id);
        }

        public static ActionResult CloseLeasingCustomerSubjectiveClassificationGrounds(long Id, int userId, DateTime setDate)
        {
            return LeasingCustomerClassificationDB.CloseLeasingCustomerSubjectiveClassificationGrounds(Id, userId, setDate);
        }

        public static List<LeasingCustomerClassification> GetLeasingConnectionGroundsForNotClassifyingWithCustomer(long customerNumber, byte isActive)
        {
            return LeasingCustomerClassificationDB.GetLeasingConnectionGroundsForNotClassifyingWithCustomer(customerNumber, isActive);
        }

        public static List<KeyValuePair<string, string>> GetLeasingInterconnectedPersonNumber(long customerNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingInterconnectedPersonNumber(customerNumber);
        }

        public static ActionResult AddLeasingConnectionGroundsForNotClassifyingWithCustomer(LeasingCustomerClassification obj, DateTime setDate, int userId)
        {
            return LeasingCustomerClassificationDB.AddLeasingConnectionGroundsForNotClassifyingWithCustomer(obj, setDate, userId);
        }

        public static LeasingCustomerClassification GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(int id)
        {
            return LeasingCustomerClassificationDB.GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(id);
        }


        public static ActionResult CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(int userId, DateTime setDate, string docNumber, DateTime docDate, int id)
        {
            return LeasingCustomerClassificationDB.CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(userId, setDate, docNumber, docDate, id);
        }

        public static List<LeasingCustomerClassification> GetLeasingConnectionGroundsForClassifyingWithCustomer(long customerNumber, byte isActive)
        {
            return LeasingCustomerClassificationDB.GetLeasingConnectionGroundsForClassifyingWithCustomer(customerNumber, isActive);

        }


        /// <summary>
        /// Ավելացնում  կամ հեռացնում է փոխկապակցված անձ (Հաճախորդի հետ փոխկապակցված անձանց դասակարգելու հիմքեր աղյուսակում)
        /// </summary>
        /// <param name="custNamber"></param>
        /// <param name="interconnectedPerson3"></param>
        /// <param name="repNumber3"></param>
        /// <param name="date3"></param>
        /// <param name="addORClose">1` ավելացնում, 2` հեռացում </param>
        /// <returns></returns>
        public static ActionResult AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(LeasingCustomerClassification obj, byte addORClose, DateTime setDate, int setNumber)
        {
            return LeasingCustomerClassificationDB.AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(obj, addORClose, setDate, setNumber);
        }

        public static LeasingCustomerClassification GetLeasingConnectionGroundsForClassifyingWithCustomerByID(int id, long customerNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingConnectionGroundsForClassifyingWithCustomerByID(id, customerNumber);
        }

        public static List<LeasingCustomerClassification> GetLeasingCustomerClassificationHistory(int leasingCustomerNumber, DateTime date)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerClassificationHistory(leasingCustomerNumber, date);
        }

        public static LeasingCustomerClassification GetLeasingCustomerClassificationHistoryByID(int id, long loanFullNumber, int lpNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingCustomerClassificationHistoryByID(id, loanFullNumber, lpNumber);
        }

        public static bool LeasingCustomerConnectionResult(int customerNumber1, int customerNumber2)
        {
            return LeasingCustomerClassificationDB.LeasingCustomerConnectionResult(customerNumber1, customerNumber2);
        }

        public static List<LeasingCustomerClassification> GetLeasingGroundsForNotClassifyingCustomerLoan(int leasingCustomerNumber, byte isActive)
        {
            return LeasingCustomerClassificationDB.GetLeasingGroundsForNotClassifyingCustomerLoan(leasingCustomerNumber, isActive);
        }

        public static Dictionary<string, string> GetLeasingLoanInfo(int leasingCustNumber)
        {
            return LeasingCustomerClassificationDB.GetLeasingLoanInfo(leasingCustNumber);
        }

        public static int GetLeasingLoanQuality(long appId)
        {
            return LeasingCustomerClassificationDB.GetLeasingLoanQuality(appId);
        }

        public static bool IsLeasingLoanActive(long appId)
        {
            return LeasingCustomerClassificationDB.IsLeasingLoanActive(appId);
        }

        public static ActionResult AddLeasingGroundsForNotClassifyingCustomerLoan(LeasingCustomerClassification obj, DateTime registrationDate, int setNumber)
        {
            return LeasingCustomerClassificationDB.AddLeasingGroundsForNotClassifyingCustomerLoan(obj, registrationDate, setNumber);
        }

        public static LeasingCustomerClassification GetLeasingGroundsForNotClassifyingCustomerLoanByID(int id)
        {
            return LeasingCustomerClassificationDB.GetLeasingGroundsForNotClassifyingCustomerLoanByID(id);
        }

        public static ActionResult CloseLeasingGroundsForNotClassifyingCustomerLoan(long appId, int id, int userId, DateTime closeDate, string docNumber, DateTime docDate)
        {
            return LeasingCustomerClassificationDB.CloseLeasingGroundsForNotClassifyingCustomerLoan(appId, id, userId, closeDate, docNumber, docDate);
        }

        public static bool IsReportActive(int id)
        {
            return LeasingCustomerClassificationDB.IsReportActive(id);
        }
        public static long GetReportAppId(int id)
        {
            return LeasingCustomerClassificationDB.GetReportAppId(id);
        }

        public static ActionResult EditLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj, int overdueDays, DateTime registrationDate, int setNumber)
        {
            return LeasingCustomerClassificationDB.EditLeasingCustomerSubjectiveClassificationGrounds(obj, overdueDays, registrationDate, setNumber);
        }

        public static LeasingCustomerClassification GetLeasingSubjectiveClassificationGroundsByIDForEdit(int Id)
        {
            return LeasingCustomerClassificationDB.GetLeasingSubjectiveClassificationGroundsByIDForEdit(Id);
        }
    }
}
