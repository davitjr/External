namespace ExternalBanking.Interfaces
{
    public interface IRedirectableToDocFlow
    {
        ActionResult RedirectToDocFlow(ulong orderId);
    }
}
