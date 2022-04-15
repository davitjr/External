using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;

namespace ExternalBanking
{
    public class Contact : ICloneable
    {
        public ulong Id { get; set; }
        public string Description { get; set; }
        public List<ContactAccount> ContactAccountList { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string PhoneCountryCode { get; set; }

        public Contact()
        {
            ContactAccountList = new List<ContactAccount>();
        }

        public void Add(ulong customerNumber)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString();
                    conn.Open();
                    ContactDB.Add(this, customerNumber, conn);
                    if (ContactAccountList != null)
                        ContactAccountList.ForEach(ac => ac.Add(Id, conn));
                }
                scope.Complete();
            }
        }

        public void Update()
        {
            Contact oldContact = GetContact(this.Id);

            if (!Utility.Equals(oldContact, this))
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    using (SqlConnection connection = new SqlConnection())
                    {
                        connection.ConnectionString = ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString();
                        connection.Open();

                        if (!Equals(oldContact))
                            ContactDB.Update(this, connection);

                        if (this.ContactAccountList != null)
                        {
                            List<KeyValuePair<Action, ContactAccount>> List = ContactAccount.GetChangedContactAccounts(this.ContactAccountList, oldContact.ContactAccountList);

                            foreach (KeyValuePair<Action, ContactAccount> item in List)
                            {
                                if (item.Key == Action.Add)
                                    item.Value.Add(Id, connection);
                                else if (item.Key == Action.Update)
                                    item.Value.Update(connection);
                                else if (item.Key == Action.Delete)
                                    item.Value.Delete(connection);
                            }
                        }
                    }
                    scope.Complete();
                }
        }

        public void Delete()
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString();
                    connection.Open();

                    ContactDB.Delete(this.Id, connection);
                    ContactAccount.DeleteContactAccounts(this.Id, connection);
                }
                scope.Complete();
            }
        }

        public static Contact GetContact(ulong ContactId)
        {
            Contact contact = ContactDB.GetContact(ContactId);
            if (contact != null)
                contact.ContactAccountList = ContactAccount.GetContactAccounts(contact.Id);
            return contact;
        }

        public bool Equals(Contact contact)
        {
            Contact thisCopy = (Contact)this.Clone();
            Contact contactCopy = (Contact)contact.Clone();

            thisCopy.ContactAccountList = null;
            contactCopy.ContactAccountList = null;

            string str1 = Utility.Serialize(thisCopy);
            string str2 = Utility.Serialize(contactCopy);
            return str1 == str2;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public static List<Contact> GetContacts(ulong customerNumber)
        {
            List<Contact> list;
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString();
                connection.Open();

                list = ContactDB.GetContacts(customerNumber, connection);

                foreach (Contact contact in list)
                    contact.ContactAccountList = ContactAccount.GetContactAccounts(contact.Id);
            }
            return list;
        }

        public static int GetContactsCount(ulong customerNumber)
        {
            return ContactDB.GetContactsCount(customerNumber);
        }
    }
}
