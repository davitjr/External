using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class ArrestsReasonTypes
    {
        public int ID { get; set; }
        public string Description { get; set; }

        public static string GetArrestsReasonTypesList()
        {

            return CustomerArrestsInfoDB.GetArrestsReasonTypesList();
            

        }
    }
}
