using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CustomerArrestInfo
    {
        public int ID { get; set; }
        public int Region { get; set; }
        public int Branch { get; set; }
        public int Village { get; set; }
        public int Number { get; set; }
        public long CustomerNumber { get; set; }
        public string Info { get; set; }
        public string TypeDescription { get; set; }

        public int TypeID { get; set; }
        public int SetNumber { get; set; }

        public string SetPerson { get; set; }

        public int ArrestReasonID { get; set; }
        public string ArrestReasonDescription { get; set; }
        public string Description { get; set; }

        public bool HasArrests { get; set; }

        public string RegistrationDate { get; set; }

        public static string PostNewAddedCustomerArrestInfo(CustomerArrestInfo obj)
        {
            return CustomerArrestsInfoDB.PostNewAddedCustomerArrestInfo(obj);           

        }

        public static string RemoveCustomerArrestInfo(CustomerArrestInfo obj)
        {
           return CustomerArrestsInfoDB.RemoveCustomerArrestInfo(obj);
        }

        public static string GetCustomerArrestsInfo(ulong customerNumber)
        {
            return CustomerArrestsInfoDB.GetCustomerArrestsInfo(customerNumber);
        }

        public static short GetSetNumberInfo(UserInfoForArrests obj)
        {
            return CustomerArrestsInfoDB.GetSetNumberInfo(obj);
        }

        public static CheckCustomerArrests GetCustomerHasArrests(ulong customerNumber)
        {
            return CustomerArrestsInfoDB.GetCustomerHasArrests(customerNumber);
        }
    }
}
