using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class VirtualCardDetails : Card
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double CardAccountBalance { get; set; }
        /// <summary>
        /// Քարտի գործողության ժամկետ
        /// </summary>
        public int CardValidityPeriod { get; set; }
        /// <summary>
        /// Քարտի իսկությունը ստուգող կոդը՝ CVV2
        /// </summary>
        public string Cvv { get; set; }
        /// <summary>
        /// Քարտի կարգավիճակ
        /// </summary>
        public string VCCardStatus { get; set; }
        /// <summary>
        /// Քարտի ARCA մնացորդ
        /// </summary>
        public double ARCABalance { get; set; }
        /// <summary>
        /// Քարտի գաղտնաբառ 
        /// </summary>
        public string MotherName { get; set; }
        /// <summary>
        /// Քարտին կցված էլ. փոստ
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Քաղվածքի ստացման եղանակ
        /// </summary>
        public string VCReportRecievingType { get; set; }

        public VirtualCardDetails() : base()
        { }
        /// <summary>
        /// Վերադարձնում է վիրտուալ քարտի մանրամասները
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public VirtualCardDetails GetVirtualCardDetails(string cardNumber, ulong customerNumber, short userID)
        {
            Card Card = GetCard(cardNumber, customerNumber);

            ProductId = Card.ProductId;
            if (Card != null)
            {
                CardNumber = Card.CardNumber;
                Currency = Card.Currency;
                CardType = Card.CardType;
                AccountNumber = Card.CardAccount.AccountNumber;
                CreditCode = Card.CreditCode;
                CardAccountBalance = Card.CardAccount.Balance;

                var sRCABalance = Card.GetArCaBalance(userID);
                ARCABalance = sRCABalance.Value;
            }

            SupplementaryType = SupplementaryType.Main;

            VCCardStatus = GetCardStatus((ulong)ProductId, customerNumber).StatusDescription;

            VirtualCardDetailsDB.GetVirtualCardDetails(this);

            EncryptionOperations encryptionOperations = new EncryptionOperations();

            if (!String.IsNullOrEmpty(Cvv))
            {
                try
                {
                    Cvv = encryptionOperations.DecryptData(Cvv);
                }
                catch (Exception ex)
                {
                    bool isTestVersion = bool.Parse(ConfigurationManager.AppSettings["TestVersion"].ToString());

                    if (isTestVersion)
                    {

                        Cvv = "123"; // throw ex;
                    }
                    else
                    {
                        Cvv = "";
                    }

                   

                }
            }

            return this;
        }
    }
}
