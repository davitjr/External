using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace ExternalBanking
{
    public class DahkDetails
    {
        /// <summary>
        /// Հաղորդագրության ա/թ
        /// </summary>
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// Հաղորդագրության համար
        /// </summary>
        public string RequestNumber { get; set; }

        /// <summary>
        /// Վարույթի համար
        /// </summary>
        public string InquestNumber { get; set; }


        /// <summary>
        /// Վարույթի համար
        /// </summary>
        public string InquestID { get; set; }

        /// <summary>
        /// Վարույթի կոդ
        /// </summary>
        public string InquestCode { get; set; }

        /// <summary>
        /// Արգելանքի/բռնագանձման/ազատման հաղորդագրության համար
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// Տեղադրման ա/թ
        /// </summary>
        public DateTime? SetDate { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Գործողության տեսակ
        /// </summary>
        public int ActionType { get; set; }

        /// <summary>
        /// Գործողության տեսակի նկարագրություն
        /// </summary>
        public string ActionTypeDescription { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ընդհանուր պարտք
        /// </summary>
        public float TotalDebt { get; set; }

        /// <summary>
        /// Ազատել մնացած միջոցները
        /// </summary>
        public bool ReleaseRemaining { get; set; }

        /// <summary>
        /// Ցուցադրվող տարբերակ (օր.` Bold,գույն...)
        /// </summary>
        public int ShowPriority { get; set; }

        /// <summary>
        /// ԴԱՀԿ արգելադրված գումար ՀՀ դրամով
        /// </summary>
        public double BlockedAmountInAMD { get; set; }
        /// <summary>
        /// ԴԱՀԿ արգելադրված գումար
        /// </summary>
        public double BlockedAmount { get; set; }

        /// <summary>
        /// ԴԱՀԿ արգելադրված գումարի արժույթ 
        /// </summary>
        public string BlockedCurency { get; set; }

        public string BlockedAmountWithCurrency { get; set; }


        /// <summary>
        /// Վերադարձնում է հաճախորդի արգելանքները և ազատումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<DahkDetails> GetDahkBlockages(ulong customerNumber)
        {
            return DahkDetailsDB.GetDahkBlockages(customerNumber);
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի բռնագանձումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<DahkDetails> GetDahkCollections(ulong customerNumber)
        {
            return DahkDetailsDB.GetDahkCollections(customerNumber);
        }

        public static double GetTransitAccountNumberFromCardAccount(double cardAccountNumber)
        {
            double cardAccount = 0;

            cardAccount = DahkDetailsDB.GetTransitAccountNumberFromCardAccount(cardAccountNumber);

            return cardAccount;
        }

        public static Dictionary<string, string> GetAccountsForBlockingAvailableAmount(ulong customerNumber)
        {
            Dictionary<string, string> accounts = new Dictionary<string, string>();

            DataTable dt = new DataTable();

            dt = AccountDB.GetCurrentAndCardAccounts(customerNumber);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                accounts.Add(dt.Rows[i]["Arm_number"].ToString(), dt.Rows[i]["Arm_number"].ToString() + "     " + dt.Rows[i]["Currency"].ToString() + "     " + dt.Rows[i]["type_of_account"].ToString() + "     " + dt.Rows[i]["accountDescr"].ToString());

            }
            return accounts;
        }

        public static Dictionary<string, string> GetFreezedAccounts(ulong customerNumber)
        {
            Dictionary<string, string> accounts = new Dictionary<string, string>();

            DataTable dt = new DataTable();

            dt = DahkDetailsDB.GetFreezedAccounts(customerNumber);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //accounts.Add(Utility.ConvertAnsiToUnicode(dt.Rows[i]["Arm_number"].ToString()), Utility.ConvertAnsiToUnicode(dt.Rows[i]["Arm_number"].ToString()) + "     " + Utility.ConvertAnsiToUnicode(dt.Rows[i]["currency"].ToString()) + "     " + Utility.ConvertAnsiToUnicode(dt.Rows[i]["type_of_account"].ToString()));
                accounts.Add(dt.Rows[i]["Arm_number"].ToString(), dt.Rows[i]["Arm_number"].ToString() + "     " + dt.Rows[i]["currency"].ToString() + "     " + dt.Rows[i]["type_of_account"].ToString());

            }
            return accounts;
        }

        public static ActionResult BlockingAmountFromAvailableAccount(double accountNumber, float blockingAmount, List<DahkDetails> inquestDetailsList, int userID, DateTime operationDate)
        {
            ActionResult result = new ActionResult();
            float blockingAmountForThisInquest = 0;

            foreach (DahkDetails d in inquestDetailsList.OrderBy(x => x.RequestDate))
            {
                if (blockingAmount > 0)
                {
                    blockingAmountForThisInquest = blockingAmount - d.Amount;

                    if (blockingAmountForThisInquest >= 0)
                    {
                        result = DahkDetailsDB.BlockingAmountFromAvailableAccount(accountNumber, d.Amount, d.MessageID, d.InquestID, userID, operationDate);
                    }
                    else
                    {
                        result = DahkDetailsDB.BlockingAmountFromAvailableAccount(accountNumber, blockingAmount, d.MessageID, d.InquestID, userID, operationDate);

                    }
                }
                else break;

                blockingAmount = blockingAmount - d.Amount;
            }


            //result = DahkDetailsDB.BlockingAmountFromAvailableAccount(accountNumber, blockingAmount, MessageID, InquestID, userID, operationDate);

            if (result.ResultCode != ResultCode.Normal)
            {
                result.Errors.Add(new ActionError());
                result.Errors[0].Description = "Տեղի ունեցավ սխալ";
                result.ResultCode = ResultCode.Failed;

            }

            return result;

        }

        public static ActionResult MakeAvailable(List<long> freezeIdList, float availableAmount, ushort filialCode, short userId, DateTime operationDate)
        {
            ActionResult result = new ActionResult();

            result = DahkDetailsDB.MakeAvailable(freezeIdList, availableAmount, filialCode, userId, operationDate);

            if (result.ResultCode != ResultCode.Normal)
            {
                result.ResultCode = ResultCode.Failed;
            }

            return result;
        }

        public static List<AccountDAHKfreezeDetails> GetAccountDAHKFreezeDetails(ulong customerNumber, string inquestId, ulong accountNumber)
        {
            return DahkDetailsDB.GetAccountDAHKFreezeDetails(customerNumber, inquestId, accountNumber);
        }

        public static List<AccountDAHKfreezeDetails> GetCurrentInquestDetails(ulong customerNumber)
        {
            return DahkDetailsDB.GetCurrentInquestDetails(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է ԴԱՀԿ գործատուներին
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static List<DahkEmployer> GetDahkEmployers(ulong customerNumber, ProductQualityFilter quality, string inquestId)
        {
            return DahkDetailsDB.GetDahkEmployers(customerNumber, quality, inquestId);
        }

        public static List<ulong> GetDAHKproductAccounts(ulong accountNumber)
        {
            return DahkDetailsDB.GetDAHKproductAccounts(accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է ԴԱՀԿ ընդհանուր գումարները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<DahkAmountTotals> GetDahkAmountTotals(ulong customerNumber)
        {
            return DahkDetailsDB.GetDahkAmountTotals(customerNumber);
        }

        public static List<DahkDetails> GetDahkDetailsForDigital(ulong customerNumber)
        {
            return DahkDetailsDB.GetDahkDetailsForDigital(customerNumber);
        }

        public static ShowNewDahk ShowDAHKMessage(ulong customerNumber)
        {
            return DahkDetailsDB.ShowDAHKMessage(customerNumber);
        }

        public static void MakeDAHKMessageRead(List<string> inquestCodes, ulong customerNumber)
        {
            DahkDetailsDB.MakeDAHKMessageRead(inquestCodes, customerNumber);
        }

    }

    /// <summary>
    /// Գործատուի տվյալներ (ԴԱՀԿ)
    /// </summary>
    public class DahkEmployer
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաղորդագրության համար
        /// </summary>
        public string RequestNumber { get; set; }

        /// <summary>
        /// Վարույթի կոդ
        /// </summary>
        public string InquestCode { get; set; }

        /// <summary>
        /// Գործատուի հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }
    }


    public class DahkAmountTotals
    {
        /// <summary>
        /// Գումար
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Սառեցման տեսակ
        /// </summary>
        public int FreezeType { get; set; }

        /// <summary>
        /// Սառեցման տեսակի նկարագրություն
        /// </summary>
        public string FreezeTypeDescription { get; set; }

        /// <summary>
        /// Արգելանքի տեսակ
        /// </summary>
        public string BlockageTypeDescription { get; set; }

        /// <summary>
        /// Արգելադրված գումար
        /// </summary>
        public float BlockedAmount { get; set; }

        /// <summary>
        /// Չարգելադրված գումար
        /// </summary>
        public float UnBlockedAmount { get; set; }

    }

    public class AccountDAHKfreezeDetails
    {
        /// <summary>
        /// ԴԱՀԿ հաղորդագրության ստացման ամսաթիվ
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// ԴԱՀԿ հաղորդագրության ունիկալ համար
        /// </summary>
        public string MessageID { get; set; }

        /// <summary>
        /// ԴԱՀԿ հաղորդագրության վարույթի կոդ
        /// </summary>
        public string InquestID { get; set; }

        /// <summary>
        /// Արգելադրման ենթակա գումար
        /// </summary>
        public float BlockedAmount { get; set; }

        /// <summary>
        /// Արգելանքի արժույթ
        /// </summary>
        public string AttachCurrency { get; set; }

        /// <summary>
        /// Արգելադրման ենթակա գումարի արժույթ
        /// </summary>
        public string BlockedAmountCurrency { get; set; }

        /// <summary>
        /// Սառեցված գումար
        /// </summary>
        public float FreezedAmount { get; set; }

        /// <summary>
        /// Սառեցված գումար դրամային արտահայտությամբ
        /// </summary>
        public float FreezedAmountAMD { get; set; }

        /// <summary>
        /// Սառեցված հաշվեհամար
        /// </summary>
        public string FreezedAccount { get; set; }

        /// <summary>
        /// Սառեցման ամսաթիվ
        /// </summary>
        public DateTime FreezeDate { get; set; }

        /// <summary>
        /// Սառեցումների աղյուսակի ունիկալ համար
        /// </summary>
        public long FreezeID { get; set; }

    }
}
