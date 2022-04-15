using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class Assignee
    {
        public uint Id { get; set; }
        public ulong CustomerNumber { get; set; }
        public ushort SignatureType { get; set; }
        public ushort IsEmployee { get; set; }
        public List<AssigneeOperation> OperationList { get; set; }
        public string AssigneeFirstName { get; set; }
        public string AssigneeLastName { get; set; }
        public string AssigneeMiddleName { get; set; }
        public string AssigneeDocumentNumber { get; set; }
        public int AssigneeDocumentType { get; set; }
        public DateTime? AssigneeDocumentGivenDate { get; set; }
        public string AssigneeDocumentGivenBy { get; set; }

        /// <summary>
        /// Բոլոր լիազորությունների վերաբերյալ նշում
        /// true` ընտրված է բոլոր լիազորությունները
        /// false` ընտրված չէ
        /// Այս պահին մուտքագրվում է և get է արվում միայն Online/Mobile համակարգերից
        /// </summary>
        public bool AllOperations { get; set; }

        public Assignee()
        {
            OperationList = new List<AssigneeOperation>();
        }

        public void Save(long id, Action actionType = Action.Add)
        {
            ActionResult result = new ActionResult();
            result = CredentialOrderDB.SaveAssigneesDetails(this, id, actionType);
            this.Id = (uint)result.Id;

            if (this.OperationList != null)
            {
                this.OperationList.ForEach(oneOperation =>
                {
                    oneOperation.Save(this.Id);
                });
            }
        }
    }
}
