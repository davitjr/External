using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking.XBManagement
{
    public class PhoneBankingContract
    {
        /// <summary>
        /// Հերթական ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հաճ.համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճ. մասնաճյուղի կոդ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime? ApplicationDate { get; set; }

        /// <summary>
        /// Պայմանագրի կարգավիճակ
        /// </summary>
        public short ApplicationStatus { get; set; }

        /// <summary>
        /// Պայմանագրի կարգավիճակի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Մուտքագրողի ՊԿ
        /// </summary>
        public int SetID { get; set; }

        /// <summary>
        /// Մուտքագրողի աշխատողի Անուն, Ազգանուն
        /// </summary>
        public string SetName { get; set; }

        /// <summary>
        /// Կարգավիճակի փոփոխման ա/թ
        /// </summary>
        public DateTime? StatusChangeDate { get; set; }

        /// <summary>
        /// Կարգավիճակը փոփոխող աշխատողի ՊԿ
        /// </summary>
        public int StatusChangeSetID { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public String ContractNumber { get; set; }

        /// <summary>
        /// Պայմանագրի ա/թ
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        ///Մեկ գործարքի սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double OneTransactionLimitToOwnAccount { get; set; }

        /// <summary>
        ///Մեկ գործարքի սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double OneTransactionLimitToAnothersAccount { get; set; }

        /// <summary>
        /// Օրական սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double DayLimitToOwnAccount { get; set; }

        /// <summary>
        /// Օրական սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double DayLimitToAnothersAccount { get; set; }

        /// <summary>
        /// Հաճախորդի էլ. հասցե
        /// </summary>
        public CustomerEmail Email { get; set; }


        /// <summary>
        /// Հաճախորդի հեռախոսահամար
        /// </summary>
        public CustomerPhone Phone { get; set; }


        /// <summary>
        /// Հարցերի պատասխանների ցուցակ
        /// </summary>
        public List<PhoneBankingContractQuestionAnswer> QuestionAnswers { get; set; }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հեռախոսային բանկինգի պայմանագիրը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static PhoneBankingContract Get(ulong customerNumber)
        {

            PhoneBankingContract phoneBankingContract = PhoneBankingContractDB.Get(customerNumber);
            if (phoneBankingContract != null)
            {
                DataTable dt = PhoneBankingContractDB.GetQuestionAnswers(phoneBankingContract.Id);
                phoneBankingContract.QuestionAnswers = new List<PhoneBankingContractQuestionAnswer>();

                foreach (DataRow dr in dt.Rows)
                {
                    PhoneBankingContractQuestionAnswer qa = new PhoneBankingContractQuestionAnswer();
                    qa.Answer = Utility.ConvertAnsiToUnicode(dr["Question_Answer"].ToString());
                    qa.QuestionId = Convert.ToInt32(dr["Question_ID"].ToString());
                    qa.QuestionDescription = dr["Question"].ToString();
                    phoneBankingContract.QuestionAnswers.Add(qa);
                }

                phoneBankingContract.Email = GetPhoneBankingContractEmail(customerNumber);
                phoneBankingContract.Phone = GetPhoneBankingContractPhone(customerNumber);
            }

            return phoneBankingContract;
        }

        public static CustomerEmail GetPhoneBankingContractEmail(ulong customerNumber)
        {
            CustomerEmail email = PhoneBankingContractDB.GetPhoneBankingContractEmail(customerNumber);
            return email;
        }

        public static CustomerPhone GetPhoneBankingContractPhone(ulong customerNumber)
        {
            CustomerPhone phone = PhoneBankingContractDB.GetPhoneBankingContractPhone(customerNumber);
            return phone;
        }
        public static double GetPBServiceFee(ulong customerNumber, DateTime date, HBServiceFeeRequestTypes requestType)
        {
            return PhoneBankingContractDB.GetPBServiceFee(customerNumber, date, requestType);
        }
        /// <summary>
        /// Ստուգվում է գոյություն ունի արդեն դեռրևս չակտիվացված հեռախոսային բանկինգի ծառայության հայտ
        /// </summary>
        public static bool isExistsNotConfirmedPBOrder(ulong customerNumber)
        {
            return PhoneBankingContractDB.isExistsNotConfirmedPBOrder(customerNumber);
        }

    }
}
