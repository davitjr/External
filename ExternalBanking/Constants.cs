using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{

    public static class Constants
    {
        /// <summary>
        /// Անթույլատրելի նիշերի ցուցակ
        /// {Enter,Enter,|,:,',Tab,°,:}
        /// </summary>
        public static readonly short[] FORBIDDEN_CHARACTERS = { 13, 10, 124, 58, 39, 9, 176, 1417, 1371 };
        public static readonly short[] SWIFT_FORBIDDEN_CHARACTERS = { 13, 10, 124, 58, 64, 35, 36, 38, 37 };

        public static readonly ushort HEAD_OFFICE_FILIAL_CODE = 22000;

        public static readonly double TRANSFER_ATTACHMENT_REQUIRED_AMOUNT = 6000000;

        public static readonly double ONE_CUSTOMER_BIG_AMOUNT_LIMIT = 20000000;

        public static readonly string LEASING_ACCOUNT_NUMBER = "220009620224000";

        public static readonly int CARD_SERVICE_FEE_PAYMENT_ADDITIONAL_ID = 99;

        public static readonly string POLICE_ACCOUNT_NUMBER = "900013150058";

        public static readonly string POLICE_ACCOUNT_NUMBER_1 = "900013150025";

        public static readonly ulong AMEX_PAPE_AmEx_CARD_NUMBER = 100000077420;

        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների չհեռացվող տեսակները
        /// </summary>
        public static readonly ushort[] NON_REMOVABLE_ACCOUNT_ADDITION_ID = { 11, 12 };

        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների հեռացվող ID-ներ հասանելիությունով
        /// </summary>
        public static readonly ushort[] ACCOUNT_ADDITION_ID_REMOVABLE_BY_PERMISSION = { 3, 9, 10, 14 };

        /// <summary>
        /// VivaCell-ի ընթացիկ հաշիվ` (h/h 220000126213000) կատարվող վճարումները բիլլինգային համակարգով անցկացնելու համար
        /// </summary>
        public static readonly string VIVACELL_PAYMENT_BY_TRANSFER_ACCOUNT_NUMBER = "220000126213000";

    }
}
