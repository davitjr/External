using System;

namespace ExternalBanking
{
    /// <summary>
    /// Գործողության հասանելիություն
    /// </summary>
    public class ActionPermission
    {
        public int ActionID { get; set; }
        public String Description { get; set; }
        public OrderType OrderType { get; set; }
        public byte OrderSubType { get; set; }

        public ActionPermission() { }

        public ActionPermission(int ActionID, String Description, OrderType OrderType, byte OrderSubType)
        {
            this.ActionID = ActionID;
            this.Description = Description;
            this.OrderType = OrderType;
            this.OrderSubType = OrderSubType;
        }
    }
}
