using ExternalBanking.DBManager;

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
