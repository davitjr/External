namespace ExternalBanking.DocFlowManagement.Interfaces
{
    public interface IHBToDocFlow
    {
        ActionResult LinkHBToDocFlow(long memoId, long hbDocId);
    }
}
