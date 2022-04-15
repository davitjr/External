using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using System.Linq;
using System.Data;

namespace ExternalBanking
{
    public class LeasingCredential 
    {

        /// <summary>
        /// Լիազորագրի ունիկալ համար
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// Հաճախորդ
        /// </summary>
        public int CustomerNumber { get; set; }


        /// <summary>
        /// Լիազորված անձ
        /// </summary>
        public int OwnerCustomerNumber { get; set; }

        /// <summary>
        /// Լիազորագրի համար
        /// </summary>
        public string CredentialNumber { get; set; }
        /// <summary>
        /// Լիազորագրի սկիզբ
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Լիազորագրի վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Ստորագրության նմուշի կարգը
        /// </summary>
        public ushort Status { get; set; }
        /// <summary>
        /// Լիազորագրը մուտքագրող ՊԿ
        /// </summary>
        public int SetNumber { get; set; }
        ///// <summary>
        ///// Լիազորված անձանց ցուցակ
        ///// </summary>
        //public List<Assignee> AssigneeList { get; set; }
        /// <summary>
        /// Լիազորագրի փակման ամսաթիվ
        /// </summary>
        public DateTime? ClosingDate { get; set; }
        /// <summary>
        /// Լիազորագրի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReason { get; set; }
        /// <summary>
        /// Լիազորագրի փակման պատճառ
        /// </summary>
        public short ClosingReasonType { get; set; }

        /// <summary>
        /// Լիազորագրի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonTypeDescription { get; set; }

        /// <summary>
        /// Լիազորագիրը փակողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }
        /// <summary>
        /// Լիազորագրի վավերացման/տրման ամսաթիվ 
        /// </summary>
        public DateTime? CredentialValidationDate { get; set; }

        /// <summary>
        /// Լիազորագիրը վավերացված է նոտարական կարգով
        /// </summary>
        public bool? HasNotarAuthorization { get; set; }

        /// <summary>
        /// Նոտար
        /// </summary>
        public string Notary { get; set; }

        /// <summary>
        /// Նոտարական տարածք
        /// </summary>
        public string NotaryTerritory { get; set; }

        /// <summary>
        /// Սեղանամատյանի համար
        /// </summary>
        public string LedgerNumber { get; set; }

        /// <summary>
        /// Թարգմանության վավերացման ամսաթիվ
        /// </summary>
        public DateTime? TranslationValidationDate { get; set; }

        /// <summary>
        /// Թարգմանությունը վավերացրած նոտարի Նոտարական տարածք
        /// </summary>
        public string TranslationOfNotaryTerritory { get; set; }

        /// <summary>
        /// Թարգմանությունը վավերացրած նոտար
        /// </summary>
        public string TranslationOfNotary { get; set; }

        /// <summary>
        /// Թարգմանության վավերացման սեղանամատյանի համար
        /// </summary>
        public string TranslationValidationLedgerNumber { get; set; }

        /// <summary>
        /// Մուտքագրման ամսաթիվ
        /// </summary> 
        public DateTime RegistrationDate { get; set; }

        public short ActionType { get; set; }


        public static ActionResult SaveAndEditLeasingCredential(LeasingCredential credential, short userID)
        {
            return LeasingCredentialDB.SaveAndEditLeasingCredential(credential, userID);
            
        }

        public static List<LeasingCredential> GetLeasingCredentials(int customerNumber, ProductQualityFilter filter)
        {
            return LeasingCredentialDB.GetLeasingCredentials(customerNumber, filter);
        }

        public static ActionResult SaveRemovedLeasingCredential(int credentialId, short userID)
        {
            return LeasingCredentialDB.SaveRemovedLeasingCredential(credentialId, userID);
        }

        public static ActionResult SaveClosedLeasingCredential(LeasingCredential credential, short userID)
        {
            return LeasingCredentialDB.SaveClosedLeasingCredential(credential, userID);
        }

        public static Dictionary<string,string> GetLeasingCustomerCredentials(int customerNumber)
        {
            return LeasingCredentialDB.GetLeasingCustomerCredentials(customerNumber);
        }

    }
}
