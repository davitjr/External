using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Պրոդուկտի ներկայացվող տեղեկատվությունների կարգավորումներ
    /// </summary>
    public class ProductNotificationConfigurations
    {
        /// <summary>
        /// Կարգավորման ունիկալ համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public short ProductType { get; set; }

        /// <summary>
        /// Տեղեկատվության տեսակ
        /// </summary>
        public byte InformationType { get; set; }

        public string InformationTypeDescription { get; set; }

        /// <summary>
        /// Տեղեկատվության տրամադրման եղանակ
        /// </summary>
        public byte NotificationOption { get; set; }

        public string NotificationOptionDescription { get; set; }

        /// <summary>
        /// Տեղեկատվության տրամադրման Հաճախականությունը
        /// </summary>
        public byte NotificationFrequency { get; set; }

        public string NotificationFrequencyDescription { get; set; }


        public byte FileFormat { get; set; }

        /// <summary>
        /// Քաղվածքի տրամադրման լեզուն 
        /// </summary>
        public byte Language { get; set; }
        /// <summary>
        ///  Քաղվածքի մուտքագրման ամսաթիվը
        /// </summary>
        public string LanguageDescription { get; set; }


        public DateTime RegistrationDate { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ApplicationDate { get; set; }

        /// <summary>
        ///  1-Մուտքագրուն, 2-Խմբագրում ,3-Հեռացում
        /// </summary>
        public byte OperationType { get; set; }
        /// <summary>
        /// Email Or Phone Ids
        /// </summary>
        public List<int> CommunicationIds { get; set; }

        /// <summary>
        /// Բոլոր Էլ․ հասցեները կամ հեռախոսահամարները
        /// </summary>
        public string CommunicationsDescription { get; set; }

        /// <summary>
        /// Հեռախոսահամարներ
        /// </summary>
        public string PhoneNumbers { get; set; }

        /// <summary>
        ///Էլ․ հասցեներ
        /// </summary>
        public string Emails { get; set; }

        public bool AllComunications { get; set; }

        public static List<ProductNotificationConfigurations> GetProductNotificationConfigurations(ulong productId)
        {
            return ProductNotificationConfigurationsDB.GetProductNotificationConfigurations(productId);
        }
    }
}
