using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{
    public class Credential
    {
        /// <summary>
        /// Լիազորագրի ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }
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
        /// Լիազորագրի տեսակ
        /// </summary>
        public ushort CredentialType { get; set; }
        /// <summary>
        /// Լիազորագրի տեսակի նկարագրություն
        /// </summary>
        public string CredentialTypeDescription { get; set; }
        /// <summary>
        /// Ստորագրության նմուշի կարգը
        /// </summary>
        public ushort Status { get; set; }
        /// <summary>
        /// Լիազորագրի ակտիվացման ամսաթիվ
        /// </summary>
        public DateTime? ActivationDate { get; set; }
        /// <summary>
        /// Լիազորագրի ակտիվացնողի ՊԿ
        /// </summary>
        public int ActivationSetNumber { get; set; }
        /// <summary>
        /// Լիազորված անձանց ցուցակ
        /// </summary>
        public List<Assignee> AssigneeList { get; set; }
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
        public ushort ClosingReasonType { get; set; }
        /// <summary>
        /// Լիազորագիրը փակողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }

        /// <summary>
        /// Բանկի կողմից տրվող լիազորագիր
        /// </summary>
        public bool GivenByBank { get; set; }

        /// <summary>
        /// Լիազորագրի վավերացման/տրման ամսաթիվ 
        /// </summary>
        public DateTime? CredentialGivenDate { get; set; }

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

        public Credential()
        {
            AssigneeList = new List<Assignee>();
        }

        public static List<Credential> GetCustomerCredentialsList(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Credential> credentialsList = new List<Credential>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                credentialsList.AddRange(CredentialDB.GetCustomerCredentialsList(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                credentialsList.AddRange(CredentialDB.GetCustomerClosedCredentialsList(customerNumber));
            }

            return credentialsList;
        }

        /// <summary>
        /// Վերադարձնում է տրված լիազորագրին համապատասխան հայտի ունիկալ համարը
        /// </summary>
        /// <param name="credentialId">Լիազորագրի ունիկալ համար</param>
        /// <returns></returns>
        public static int GetCredentialDocId(ulong credentialId)
        {
            return CredentialDB.GetCredentialDocId(credentialId);
        }


        public static DataTable GetAssigneeIdentificationOrderByDocId(long docId)
        {
            return CredentialDB.GetAssigneeIdentificationOrderByDocId(docId);
        }
    }
}
