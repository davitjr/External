using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Կանխիկ տերմինալի վճարման հայտ
    /// </summary>
    public class CTOrder : Order
    {
        ///// <summary>
        ///// Վճարման կատարման օր, ժամ
        ///// </summary>
        //public DateTime PaymentDateTime { get; set; }

        /// <summary>
        /// Նույնականացուցչի տեսակը
        /// </summary>
        public byte IdentifierType { get; set; }

        /// <summary>
        /// Վճարման տեսակ
        /// </summary>
        public byte PaymentType { get; set; }

        ///// <summary>
        ///// Վճարային տերմինալի ունիկալ նունականացուցիչ
        ///// </summary>
        //public string TerminalID { get; set; }

        /// <summary>
        /// Վճարային տերմինալի նկարագրություն
        /// </summary>
        public string TerminalDescription { get; set; }

        /// <summary>
        /// Վճարային տերմինալի հասցեն
        /// </summary>
        public string TerminalAddress { get; set; }

        /// <summary>
        /// Կանխիկ տերմինալի անվանում
        /// </summary>
        public string CTCustomerDescription { get; set; }
        virtual public ActionResult Confirm()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.Source == SourceType.SSTerminal)
            {
                ACBAServiceReference.User user = new ACBAServiceReference.User();
                user.userID = 88;
                result = base.Confirm(user);
            }

            return result;
        }

    }
}
