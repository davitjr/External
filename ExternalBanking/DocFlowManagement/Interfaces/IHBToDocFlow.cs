using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DocFlowManagement.Interfaces
{
    public interface  IHBToDocFlow
    {
        ActionResult LinkHBToDocFlow(long memoId, long hbDocId);
    }
}
