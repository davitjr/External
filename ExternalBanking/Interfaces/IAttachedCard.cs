namespace ExternalBanking.Interfaces
{
    public interface IAttachedCard
    {
        /// <summary>
        /// Identificator for Other bank Arca Cards
        /// </summary>
        bool IsAttachedCard { get; set; }
        /// <summary>
        /// Arca Binding_ID for binding transactions
        /// </summary>
        string BindingId { get; set; }
    }
}
