using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class TransactionDetail
    {

        /// <summary>
        /// Գործարքի ամսաթիվ	
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Գործարքի գումար
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Գործարքի տեսակ 'd' -դեբետային, 'c' -կրեդիտային 
        /// </summary>
        public string DebitCredit { get; set; }

        /// <summary>
        /// Գործարքը կատարած քարտի համարը
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Միջնորդավճար	
        /// </summary>
        public decimal TransactionFee { get; set; }

        /// <summary>
        /// Գործարքի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ձևակերպման ամսաթիվ
        /// </summary>
        public DateTime Settlementdate { get; set; }

        /// <summary>
        /// Հավասատագրման կոդ
        /// </summary>
        public string Authcode { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string OrderID { get; set; }


        //public static void SerializeXml(List<TransactionDetail> trDetail, string fileName)
        //{
        //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TransactionDetail>));

        //    Stream writer = new FileStream(fileName, FileMode.Create);
        //    xmlSerializer.Serialize(writer, trDetail);
        //    writer.Close();
        //}

    }

    public class MerchantStatement
    {
        public List<TransactionDetail> Transactions { get; set; }
    }

}

