using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.XBManagement
{   
    /// <summary>
    /// ՀԲ/Մոբայլ ակտիվացման/փոփոխման դիմում
    /// </summary>
    public class HBActivationRequest
    {
        public long Id { get; set; }
        
        /// <summary>
        /// Փոփոխման տեսակ
        /// </summary>
        public short RequestType { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string RequestTypeDescription { get; set; }

        /// <summary>
        /// Փոփոխման ա/թ
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Օգտագործողի անուն
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Տոկեններ
        /// </summary>
        public HBToken HBToken { get; set; }

        /// <summary>
        /// Սպասարկման գումար
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// Անվճար է թե ոչ
        /// </summary>
        public bool IsFree { get; set; }

    }
}
