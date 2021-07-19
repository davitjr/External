using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Դրամարկղի մատյանի մուտք/ելք
    /// </summary>
    public class CashBook
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Աշխատակցի համար
        /// </summary>
        public int RegisteredUserID { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Տեսակ
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public int Quality { get; set; }
        /// <summary>
        /// Մուտք/Ելք    1 - Ելք  ;  2 - Մուտք
        /// </summary>
        public int OperationType { get; set; }
        /// <summary>
        /// Լրացուցիչ նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Թղթակից ՊԿ
        /// </summary>
        public int CorrespondentSetNumber { get; set; }
        /// <summary>
        /// Փոծկապակցված (Մուտք/Ելք)
        /// </summary>
        public int CorrespondentID { get; set; }
        /// <summary>
        /// Դրամապահոց
        /// </summary>
        public bool BankVault { get; set; }

        public int LinkedRowID { get; set; }

        /// <summary>
        /// "Ավելցուկի/Պակասորդի փակման" տողի վրա գրվում է այն "Ավելցուկի/Պակասորդի մուտքագրման" ID-Ý, որը պետք է փակել
        /// Այդ ID-ի միջոցով էլ վերադարձվում է "Ավելցուկի/Պակասորդի մուտքագրման" մասնաճյուղը` CashBookFilialCodeForLinkedRow-ն
        /// </summary>
        public int CashBookFilialCodeForLinkedRow { get; set; }

        public bool IsClosed { get; set; }

		public double MaturedAmount {get; set;}

		/// <summary>
		/// Դեբետ հաշիվ
		/// </summary>
		public string DebetAccount { get; set; }

		/// <summary>
		/// Կրեդիտ հաշիվ
		/// </summary>
		public string CreditAccount { get; set; }

		const int CHIEF_ACCOUNTANT_GROUP_1 = 2;
        const int CHIEF_ACCOUNTANT_GROUP_2 = 503;

        public static ActionResult SaveCashBooks(List<CashBook> cashBooks, ExternalBanking.ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.CheckOpDayClosingStatus(user.filialCode));
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            //Գումար մուտքագրված չէ
            if (cashBooks == null)
            {

                result.Errors.Add(new ActionError(772));
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            foreach (CashBook cashBook in cashBooks)
            {
                cashBook.Complete(user);
                result = cashBook.Validate(user);
                if (cashBook.LinkedRowID != 0)
                {
                    result.Errors.AddRange(Validation.CloseCashbook(cashBook));
                    if (result.Errors.Count > 0)
                    {
                        result.ResultCode = ResultCode.ValidationError;
                    }
                }
            }

            if (result.ResultCode != ResultCode.ValidationError)
            {
                result = CashBookDB.Save(cashBooks);
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է թղթակցի համար
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static int GetCorrespondentSetNumber(int filialCode,bool isEncashmentDepartment=false)
        {
            return CashBookDB.GetCorrespondentSetNumber(filialCode, isEncashmentDepartment);
        }

        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="user"></param>
        public void Complete(ExternalBanking.ACBAServiceReference.User user)
        {
            if (Quality == 0)
            {
                if (Type == 2)
                {
                    OperationType = 2;
                }
                else if (Type == 1)
                {
                    OperationType = 1;
                }
            }
            //Կանխիկ դրամաշրջանառության բաժին
            if (user.AdvancedOptions["isEncashmentDepartment"] == "1")
            {
                FillialCode = -1;
            }
            else
            {
                //if (user.AdvancedOptions["isHeadCashBook"] != "1")
                //{
                    FillialCode = user.filialCode;
                //}
            }

            if (ID == 0)
            {
                RegisteredUserID = user.userID;
            }
            else if (Type == 2 || Type == 4)
            {
                CorrespondentSetNumber = CashBook.GetCashBookSetnumber(ID);
            }
         
        }

        /// <summary>
        /// Դրամարկղի մատյանի մուտք/ելք ի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCashBook(this, user));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է թղթակցի մասնաճուղը
        /// </summary>
        /// <param name="correspondentSetNumber"></param>
        /// <returns></returns>
        public static int GetCorrespondentFilialCode(int correspondentSetNumber)
        {
            return CashBookDB.GetCorrespondentFilialCode(correspondentSetNumber);
        }


        /// <summary>
        /// Վերադարձնում է դրամարկղի ավելցուկի/պակասորդի մուտքագրման մասնաճուղը ըստ linked_row_id - ի
        /// </summary>
        /// <param name="linkedRowID"></param>
        /// <returns></returns>
        public static int GetCashBookFilialCodeByLinkedRow(int linkedRowID)
        {
            return CashBookDB.GetCashBookFilialCodeByLinkedRow(linkedRowID);
        }
        

        /// <summary>
        /// Հեռացնել գրառումը
        /// </summary>
        /// <param name="cashBookID"></param>
        /// <returns></returns>
        public static ActionResult RemoveCashBook(int cashBookID)
        {
            return CashBookDB.RemoveCashBook(cashBookID);
        }

        /// <summary>
        /// Վերադարձնում է մուտքագրված և ելքագրված գումարների տարբերությունը
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<KeyValuePair<int, double>> GetRest(SearchCashBook searchParams, ExternalBanking.ACBAServiceReference.User user)
        {
            searchParams.RegisteredUserID = user.userID;
            searchParams.FillialCode = user.filialCode;

            if (!(user.IsChiefAcc || user.AdvancedOptions["isHeadCashBook"] == "1"))
            {
                searchParams.SearchUserID = user.userID;
            }
            

            List<KeyValuePair<int, double>> rest = new List<KeyValuePair<int, double>>();
            rest.Add(new KeyValuePair<int, double>(0, CashBookDB.GetRestTransactions(searchParams)));//մուտքագրված և ելքագրված գումարների տարբերությունը հաշվախահական գործարքներից
            rest.Add(new KeyValuePair<int, double>(1, CashBookDB.GetRestCashBook(searchParams)));//մուտքագրված և ելքագրված գումարների տարբերությունը դրամարկղի մատյանից և հաշվախահական գործարքներից
            return rest;
        }

        /// <summary>
        /// Փոփոխել գրառումը
        /// </summary>
        /// <param name="cashBookID"></param>
        /// <returns></returns>
        public static ActionResult ChangeCashBookStatus(int cashBookID, int setnumber, int newStatus, User user)
        {
            if ((user.GroupID == CHIEF_ACCOUNTANT_GROUP_1 || user.GroupID == CHIEF_ACCOUNTANT_GROUP_2) && (newStatus == -1 || newStatus == 1))
            {
                return CashBookDB.ChangeCashBookStatus(cashBookID, setnumber, newStatus, 0);
            }
            else if (newStatus == 2 || newStatus == -2)
            {
                return CashBookDB.ChangeCashBookStatus(cashBookID, setnumber, newStatus, 1);
            }
            else
            {
                return new ActionResult() { ResultCode = ResultCode.Failed };
            }
        }

        /// <summary>
        /// Վերադարձնում է CashBook ին կցված SetNumber
        /// </summary>
        /// <param name="cashBookID"></param>
        /// <returns></returns>
        public static int GetCashBookSetnumber(int cashBookID)
        {
            return CashBookDB.GetCashBookSetnumber(cashBookID);
        }
        /// <summary>
        /// Գլխավոր հաշվապահի կողմից հեռացվում է չհաստատված գործարքը
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteCashBook(int Id)
        {
             CashBookDB.DeleteCashBook(Id);
        }


		public static double GetCashBookAmount(int cashBookID)
		{
			return CashBookDB.GetCashBookAmount(cashBookID);
		}


		public static bool HasUnconfirmedOrder(int cashBookID)
		{
			return CashBookDB.HasUnconfirmedOrder(cashBookID);
		}
	}
}
