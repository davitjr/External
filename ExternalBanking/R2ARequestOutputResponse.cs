using ExternalBanking.ARUSDataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class R2ARequestOutputResponse
    {
        internal R2ARequestOutput R2ARequestOutput { get; set; }

        internal ActionResult ActionResult { get; set; }

        public R2ARequestOutputResponse()
        {
            R2ARequestOutput = new R2ARequestOutput();
            ActionResult = new ActionResult();
        }
    }
}
