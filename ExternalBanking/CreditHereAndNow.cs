using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ապառիկ տեղում տեսակի վարկ
    /// </summary>
    public class  CreditHereAndNow : LoanProduct
    {
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Նշված ժամանակահատվածի ապառիկ տեղում տեսակի բոլոր վարկերը համապատասխան կարգավիճակի(պայմանագիր, ոչ պայմանագիր) և մասնաճյուղի
        /// </summary>
        public static List<CreditHereAndNow> GetCreditsHereAndNow(SearchCreditHereAndNow searchParams, out int RowCount)
        {
            List<CreditHereAndNow> credits = new List<CreditHereAndNow>();
             
            credits = CreditHereAndNowDB.GetCreditsHereAndNowForActivate(searchParams,out RowCount); 

            return credits;
        }

 
    }
}
