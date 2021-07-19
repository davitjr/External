using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ExternalBanking.XBManagement
{
    public enum HBProductPermissionType : short
    {
        /// <summary>
        ///Վարկեր
        /// </summary>
        Loan = 1,
        /// <summary>
        /// Առևտրային վարկային գիծ
        /// </summary>
        CommercialCreditLine = 3,
        /// <summary>
        /// Երաշխիք/Ակրեդիտիվ
        /// </summary>
        Guarantee = 4,

        /// <summary>
        /// Ավանդներ
        /// </summary>
        Deposit = 5,

        /// <summary>
        /// Քարտեր
        /// </summary>
        Card = 6,
        /// <summary>
        /// Ընթացիկ հաշիվներ
        /// </summary>
        CurrentAccount = 7,

        /// <summary>
        /// Պարբերական փոխանցումներ
        /// </summary>
        Periodic = 8,

        /// <summary>
        /// Օվերդրաֆտ
        /// </summary>
        Overdraft = 9,

        /// <summary>
        /// Ֆակտորինգ
        /// </summary>
        Factoring = 10,

        /// <summary>
        /// Վճարված ֆակտորինգ
        /// </summary>
        Paidfactoring = 12

    }
    public enum RequestType:short
    {
        NotSpecified = 0,
        /// <summary>
        /// Նոր պայմանագիր (ներառյալ մեկ տոկեն)
        /// </summary>
        NewHBContractIncludingOneToken = 1,

        /// <summary>
        /// Նոր տոկեն
        /// </summary>
        NewToken = 2,

        /// <summary>
        /// Օգտագործողին գործարքների մուտքագրման իրավունքի տրամադրում
        /// </summary>
        UserOperationPermission = 3,

        /// <summary>
        /// Նոր հեռախոսային բանկինգի պայմանագիր
        /// </summary>
        NewPhoneBankingContract = 4,

    }
}