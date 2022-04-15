//using System.Configuration;
//using System.Net;
//using System.IO;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;

namespace ExternalBanking.XBManagement
{

    public class HBServletRequestOrder : HBServletOrder
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
