using ExternalBanking.DBManager;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ExternalBanking
{
    public class ContactAccount
    {
        public ulong Id { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public short ObjectType { get; set; }
        public string ObjectValue { get; set; }

        public void Add(ulong contractId, SqlConnection conn)
        {
            ContactAccountDB.Add(this, contractId, conn);
        }

        public void Update(SqlConnection connection)
        {
            ContactAccountDB.Update(this, connection);
        }

        public void Delete(SqlConnection conn)
        {
            ContactAccountDB.Delete(Id, conn);
        }

        public static void DeleteContactAccounts(ulong contractId, SqlConnection connection)
        {
            ContactAccountDB.DeleteContactAccounts(contractId, connection);
        }

        public static List<ContactAccount> GetContactAccounts(ulong contractId)
        {
            return ContactAccountDB.GetContactAccounts(contractId);
        }

        public static List<KeyValuePair<Action, ContactAccount>> GetChangedContactAccounts(List<ContactAccount> newContactAccountList, List<ContactAccount> oldContactAccountList)
        {
            List<KeyValuePair<Action, ContactAccount>> changes = new List<KeyValuePair<Action, ContactAccount>>();

            foreach (ContactAccount newContactAccount in newContactAccountList)
            {
                ContactAccount oldContactAccount = oldContactAccountList.Find(o => o.Id == newContactAccount.Id);
                if (Utility.Equals(oldContactAccount, newContactAccount))
                {
                    oldContactAccountList.Remove(oldContactAccount);
                    continue;
                }
                if (oldContactAccount != null && oldContactAccount.Id != 0)
                {
                    changes.Add(new KeyValuePair<Action, ContactAccount>(Action.Update, newContactAccount));
                    oldContactAccountList.RemoveAll(o => o.Id == newContactAccount.Id);
                }
                else
                    changes.Add(new KeyValuePair<Action, ContactAccount>(Action.Add, newContactAccount));
            }
            if (oldContactAccountList.Count != 0)
                foreach (ContactAccount ob in oldContactAccountList)
                    changes.Add(new KeyValuePair<Action, ContactAccount>(Action.Delete, ob));
            return changes;
        }

    }
}