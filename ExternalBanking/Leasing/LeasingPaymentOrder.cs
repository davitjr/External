using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Լիզինգի Վճարման հանձնարարական
    /// </summary>
    public class LeasingPaymentOrder : PaymentOrder
    {
        public ulong ProductId { get; set; }
        public int InsuranceId { get; set; }
        public string InsuranceDescription { get; set; }
        public new void Get()
        {
            LeasingDB.GetPaymentOrder(this);
        }
    }
}