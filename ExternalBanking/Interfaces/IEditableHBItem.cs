using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking;

namespace ExternalBanking.XBManagement.Interfaces
{
    public interface IEditableHBItem
    {
        ActionResult Add(int userID, SourceType source, long orderId, ulong customerNumber=0);
        ActionResult Update(int userID, SourceType source, long orderId, ulong customerNumber=0);
        ActionResult Deactivate(int userID, SourceType source, long orderId, ulong customerNumber=0);

    }

}
