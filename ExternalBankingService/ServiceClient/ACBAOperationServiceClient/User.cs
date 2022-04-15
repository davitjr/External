
using System.Collections.Generic;

namespace ACBALibrary
{
    public class User
    {
        public string userName { get; set; }
        public short userID { get; set; }
        public ulong number_of_item { get; set; }
        public int transRight { get; set; }
        public ushort filialCode { get; set; }
        public ulong userCustomerNumber { get; set; }
        public short userPermissionId { get; set; }
        public short DepartmentId { get; set; }
        public int TransactionLimit { get; set; }
        public long AccountGroup { get; set; }
        /// <summary>
        /// Գլխավոր հաշվապահ
        /// </summary>
        public bool IsChiefAcc { get; set; }

        /// <summary>
        /// Կառավարիչ
        /// </summary>
        public bool IsManager { get; set; }

        /// <summary>
        /// Փոփոխություն կատարելու հասանելիություններ
        /// </summary>
        public Dictionary<string, string> AdvancedOptions { get; set; }
    }

    public class AuthorizedUser
    {
        public int transRight { get; set; }
        public ushort filialCode { get; set; }
        public ulong userCustomerNumber { get; set; }
        public short userID { get; set; }
        public short positionID { get; set; }
        public short departmentID { get; set; }
        public string userName { get; set; }
    }


}
