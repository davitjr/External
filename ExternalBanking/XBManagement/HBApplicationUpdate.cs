using ExternalBanking.XBManagement.Interfaces;
using System.Collections.Generic;

namespace ExternalBanking.XBManagement
{
    public class HBApplicationUpdate
    {
        public List<IEditableHBItem> AddedItems { get; set; }
        public List<IEditableHBItem> UpdatedItems { get; set; }
        public List<IEditableHBItem> DeactivatedItems { get; set; }


    }
}
