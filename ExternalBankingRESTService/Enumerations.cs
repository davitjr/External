namespace ExternalBankingRESTService
{
    public class Enumerations
    {
        /// <summary>
        /// Փաստաթղթերի տեսակներ
        /// </summary>
        public enum DocumentType : short
        {
            /// <summary>
            /// ՀՀ անձնագիր
            /// </summary>
            RApassport = 1,

            /// <summary>
            /// Նույնականացման քարտ
            /// </summary>
            IdentifierCard = 11,

            /// <summary>
            /// Հանրային ծառայության համարանիշ
            /// </summary>
            SocialServiceNumber = 56,

            /// <summary>
            /// ՀՀ բիոմետրիկ անձնագիր
            /// </summary>
            BiometricPassport = 88
        }

        public enum CustomerAuthenticationResult : short
        {
            /// <summary>
            /// Բանկում կարգավիճակ ունեցող հաճախորդ
            /// </summary>
            CustomerWithAttachment = 2,

            /// <summary>
            /// Բանկում կարգավիճակ չունեցող հաճախորդ
            /// </summary>
            NonCustomer = 0,

            /// <summary>
            /// Բանկում կարգավիճակ ունեցող հաճախորդը չունի կցված փաստթղթեր
            /// </summary>
            CustomerWithNoAttachments = 1,

            /// <summary>
            /// Հաճախորդը, որը ունի օնլայն բանկինգ
            /// </summary>
            CustomerWithOnlineBanking = 3
        }

        public enum CustomerAuthenticationInfoType : short
        {
            /// <summary>
            /// Նկար
            /// </summary>
            Photo = 1,

            /// <summary>
            /// Փաստաթուղթ
            /// </summary>
            Document = 2,

            /// <summary>
            /// Դատարկ
            /// </summary>
            Empty = 0
        }

        public enum TypeOfAttachments : short
        {
            jpg = 1,
            pdf = 2
        }
    }
}