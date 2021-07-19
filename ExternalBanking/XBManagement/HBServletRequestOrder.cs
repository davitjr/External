using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Configuration;
//using System.Net;
//using System.IO;
using ExternalBanking.DBManager;
using System.Transactions;
using ExternalBanking.Interfaces;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.XBManagement
{
   
 public class HBServletRequestOrder:HBServletOrder 
    {
        override public ActionResult Save(string userName, SourceType source, User user, short schemaType)
        {
            return HBServletRequestOrderDB.SaveHBServletRequestOrder(this, userName, source);
        }
        public void GetHBServletRequestOrder()
        {
            HBServletRequestOrderDB.GetHBServletRequestOrder(this);

        }
    }
}
