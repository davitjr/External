using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class ContactsRequestResponse
    {
        public List<Contact> Contacts { get; set; }
        public Result Result { get; set; }

        public ContactsRequestResponse()
        {
            Result = new Result();
        }
    }
}