using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class ServicePaymentNote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// Սպասարկան վարձի գանձման նշման տեսակ
        /// </summary>
        public ServicePaymenteNoteType NoteActionType { get; set; }
        /// <summary>
        /// Սպասարկան վարձի գանձման նշման տեսակի նկարագրություն
        /// </summary>
        public string NoteActionTypeDescription { get; set; }

        /// <summary>
        /// Սպասարկան վարձի գանձման նշման բացատրություն
        /// </summary>
        public ushort NoteReason { get; set; }
        /// <summary>
        ///Սպասարկան վարձի գանձման նշման բացատրության նկարագրություն 
        /// </summary>
        public string NoteReasonDescription { get; set; }

        /// <summary>
        /// Սպասարկան վարձի գանձման նշման լրացուցիչ բացատրություն
        /// </summary>
        public string AdditionalDescription { get; set; }
        /// <summary>
        /// Ա/Թ
        /// </summary>
        public DateTime NoteDate { get; set; }
        /// <summary>
        /// Վերադարձնում է սպասարկման վարձի գանձման նշումները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<ServicePaymentNote> GetServicePaymentNoteList(ulong customerNumber)
        {
            return ServicePaymentNoteDB.GetServicePaymentNoteList(customerNumber);
        }


    }
}
