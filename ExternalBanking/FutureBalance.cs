using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{   
    /// <summary>
    /// Ապագա մնացորդ մեկ հաշվի վերաբերյալ
    /// </summary>
    public class FutureBalance
    {
        //Մնացորդ գործարքից առաջ
        public double BalanceBefore { get; set; }

        //Մնացորդ գործարքից հետո
        public double BalanceAfter { get; set; }

        //Մնացորդ գործարքից հետո` ներառյալ չհաստատված գործարքները
        public double BalanceAfterFull { get; set; }
        
    }
}
