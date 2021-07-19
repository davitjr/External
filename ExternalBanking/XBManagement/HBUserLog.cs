using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.XBManagement
{
    public class HBUserLog
    {
       public String UserName { get; set; }
       public String Description { get; set; }
       public DateTime TimeStamp { get; set; }
       public String TokenNumber { get; set; }

    }
}
