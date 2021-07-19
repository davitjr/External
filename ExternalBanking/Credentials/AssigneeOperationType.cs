using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class AssigneeOperationType
    {
        public uint Id { get; set; }
        public string Description { get; set; }
        public uint GroupId { get; set; }
        public bool ForNaturalPerson { get; set; }
        public bool ForPrivateEntrepreneur { get; set; }
        public bool ForLegalEntity { get; set; }
        public bool AccountNeeded { get; set; }

        public AssigneeOperationType()
        {

        }
    }
}
