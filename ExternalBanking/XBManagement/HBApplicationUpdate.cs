using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.XBManagement.Interfaces;

namespace ExternalBanking.XBManagement
{
    public class HBApplicationUpdate
    {
        public List<IEditableHBItem> AddedItems { get; set; }
        public List<IEditableHBItem> UpdatedItems { get; set; }
        public List<IEditableHBItem> DeactivatedItems { get; set; }    

    
    }
}
