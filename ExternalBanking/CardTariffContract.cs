using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի աշխատավարձային ծրագիր
    /// </summary>
    public class CardTariffContract
    {
        /// <summary>
        /// Համար
        /// </summary>
        public long TariffID { get; set; }
        /// <summary>
        /// Անվանում
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Ավարտ
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Հասցե
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Սակագներ
        /// </summary>
        public List<CardTariff> CardTariffs { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Հիմք
        /// </summary>
        public ushort Reason { get; set; }
        /// <summary>
        /// Հիմքի նկարագրություն
        /// </summary>
        public string ReasonDescription { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// աշխատավարձային ծրագրի համար
        /// </summary>
        public ushort RelatedOfficeNumber { get; set; }

        /// <summary>
        /// Ստանում է քարտի աշխատանքային ծրագիրը
        /// </summary>
        public void Get()
        {
            CardTariffContractDB.GetCardTariffContract(this);
            CardTariffContractDB.GetCardTariffs(this);
        }
        /// <summary>
        /// Ստուգում է առկա աշխատավարձային ծրագիր
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static bool HasCardTariffContract(ulong customerNumber)
        {
            return CardTariffContractDB.HasCardTariffContract(customerNumber);
        }
        /// <summary>
        /// Վերադարձնում է աշխատավարձային ծրագրերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<CardTariffContract> GetCustomerCardTariffContracts(ulong customerNumber, ProductQualityFilter filter)
        {
            return CardTariffContractDB.GetCustomerCardTariffContracts(customerNumber, filter);
        }

        /// <summary>
        /// Վերադարձնում է աշխատավարձային ծրագրի ակտիվ քարտերի քանակ
        /// </summary>
        /// <param name="contractID"></param>
        /// <returns></returns>
        public static int GetActiveCardsCount(int contractID)
        {
            return CardTariffContractDB.GetActiveCardsCount(contractID);
        }

        /// <summary>
        /// Վերադարձնում է USSD ծառայության սակագինը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static List<float> GetCardUSSDServiceTariff(ulong productID)
        {
            return CardTariffContractDB.GetCardUSSDServiceTariff(productID);
        }


        /// <summary>
        /// Վերադարձնում է SMS ծառայության սակագինը
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static List<float> GetPlasticCardSMSServiceTariff(ulong productID)
        {
            return CardTariffContractDB.GetPlasticCardSMSServiceTariff(productID);
        }

        /// <summary>
        /// Վերադարձնում է քարտի սակագները
        /// </summary>
        public static CardTariffContract GetCardTariffs(long tariffID)
        {
            CardTariffContract tariffContract = new CardTariffContract();
            tariffContract.TariffID = tariffID;
            CardTariffContractDB.GetCardTariffs(tariffContract);
            return tariffContract;
        }

        /// <summary>
        /// Վերադարձնում է քարտի սակագները՝ տվյալ քարտատեսակի համար
        /// </summary>
        public static CardTariffContract GetCardTariffsByCardType(ushort cardType, short periodicityType)
        {
            int officeNumber = Info.GetCardOfficeTypesForIBanking(cardType, periodicityType);
            CardTariffContract contract = GetCardTariffs(officeNumber);
            if (contract.CardTariffs != null)
            {
                contract.CardTariffs.RemoveAll(c => c.CardType != cardType);
            }
            return contract;
        }

        public static CardTariff GetAttachedCardTariffs(string mainCardNumber, uint cardType)
        {
            Card mainCard = CardDB.GetCard(mainCardNumber);
            int officeNumber = 0;
            if (mainCard.Type == 41)
            {
                officeNumber = 2650;
            }
            else if (mainCard.RelatedOfficeNumber == 174)
            {
                officeNumber = 940;
            }
            else if (mainCard.RelatedOfficeNumber == 24)
            {
                officeNumber = 24;
            }
            else if (mainCard.RelatedOfficeNumber != 174 && (cardType == 34 || cardType == 50))
            {
                officeNumber = 1946;
            }
            else
            {
                if (mainCard.FeeForCashTransaction * 100 == 0.5)
                {
                    officeNumber = 1998;
                }
                else if (mainCard.FeeForCashTransaction * 100 == 0)
                {
                    officeNumber = 1999;
                }
                else if (mainCard.FeeForCashTransaction * 100 == 1)
                {
                    officeNumber = 940;
                }
            }
            CardTariffContract contract = GetCardTariffs(officeNumber);
            if (contract.CardTariffs != null)
            {
                contract.CardTariffs.RemoveAll(c => c.CardType != cardType);
                contract.CardTariffs.RemoveAll(c => c.Currency != mainCard.Currency);
            }
            return contract.CardTariffs.FirstOrDefault();
        }
    }
}
