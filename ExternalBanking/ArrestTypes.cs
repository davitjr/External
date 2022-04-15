using ExternalBanking.DBManager;

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
