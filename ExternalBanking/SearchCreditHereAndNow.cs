using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions; 

namespace ExternalBanking
{
    public class SearchCreditHereAndNow:SearchParams
    {
        /// <summary>
        /// Խանութի մասնաճյուղ
        /// </summary>
        public int ShopFilial { get; set; }
        /// <summary>
        /// Պրոդուկտի ֆիլտր
        /// </summary>
        public ProductQualityFilter QualityFilter { get; set; } 

    }
}