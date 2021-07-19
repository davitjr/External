using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class AssigneeOperation
    {
        public uint Id { get; set; }
        public ushort OperationType { get; set; }
        public string OperationTypeDescription { get; set; }
        public string OperationGroupTypeDescription { get; set; }
        public ushort GroupId { get; set; }
        public bool AllAccounts { get; set; }
        public List<Account> AccountList { get; set; }

        public AssigneeOperation() 
        {
            AccountList = new List<Account>();            
        }

        public void Save(uint id)
        {
            ActionResult result = new ActionResult();
            result = CredentialOrderDB.SaveAssigneeOperationsDetails(this, id);
            this.Id = (uint)result.Id;


            if (this.AccountList != null)
            {
                this.AccountList.ForEach(oneAccount =>
                    {
                        CredentialOrderDB.SaveAssigneeOperationAccountsDetails(ulong.Parse(oneAccount.AccountNumber),this.Id);
                    }
                );
            }
        }

        public static List<Account> GetAccountsForCredential(ulong customerNumber, int operationType)
        {
            return AssigneeOperationDB.GetAccountsForCredential(customerNumber, operationType);
        }

        
    }
}
