using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class FTPRate
    {

        /// <summary>
        /// FTP տոկոսադրույքի տեսակ
        /// </summary>
        public FTPRateType rateType { get; set; }
       
        /// <summary>
        /// Միջոցների տրամադրման պայմաններ
        /// </summary>
        public List<FTPRateDetail> FTPRateDetails { get; set; }

        public static FTPRate GetFTPRateDetails(FTPRateType rateType)
        {
            FTPRate rate = FTPRateDB.GetFTPRateDetails(rateType);

            return rate;
        }

    }

   

    public class FTPRateDetail
    {
        /// <summary>
        /// Տրմադրման արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Տրմադրման տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }


        /// <summary>
        /// Վերադարձնում է սեփական միջոցների տեղաբաշխման տվյալները
        /// </summary>
        /// <returns></returns>
       
    }
}
