using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class ArrestTypes
    {
        public int ID { get; set; }
        public string Description { get; set; }

        public static string GetArrestTypesList()
        {

            return CustomerArrestsInfoDB.GetArrestTypesList();
            


        }
    }
}
