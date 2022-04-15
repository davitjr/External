using ExternalBanking.DBManager;
using System;

namespace ExternalBanking.XBManagement
{
    public class HBApplication
    {
        /// <summary>
        /// Հերթական ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Հաճ.համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public String ContractNumber { get; set; }
        ///// <summary>
        ///// Հաճ.լրիվ անուն
        ///// </summary>
        public String FullName { get; set; }
        /// <summary>
        /// Հաճ.նկարագրություն
        /// </summary>
        public String Description { get; set; }
        /// <summary>
        /// Պայմանագրի տեսակ
        /// </summary>
        public byte ContractType { get; set; }
        /// <summary>
        /// Պայմանագրի ա/թ
        /// </summary>
        public DateTime? ContractDate { get; set; }
        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime? ApplicationDate { get; set; }
        /// <summary>
        /// Հաճ. մասնաճյուղի համար
        /// </summary>
        public int FilialCode { get; set; }
        /// <summary>
        /// Պաշտոն
        /// </summary>
        public string Position { get; set; }/// <summary>
                                            /// Տնօրենի Ա.Ա
                                            /// </summary>
        public String Manager { get; set; }
        /// <summary>
        /// Գլխ.հաշվապահի Ա.Ա
        /// </summary>
        public String ChiefAcc { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public HBApplicationQuality Quality { get; set; }
        /// <summary>
        /// Կարգավիճակ նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Մուտքագրողի աշխատողի ՊԿ
        /// </summary>
        public int SetID { get; set; }

        /// <summary>
        /// Մուտքագրողի աշխատողի Անուն, Ազգանուն
        /// </summary>
        public string SetName { get; set; }
        /// <summary>
        /// Կարգավիճակի փոփոխման ա/թ
        /// </summary>
        public DateTime? StatusChangeDate { get; set; }
        /// <summary>
        /// Կարգավիճակը փոփոխող աշխատողի ՊԿ
        /// </summary>
        public int StatusChangeSetID { get; set; }
        /// <summary>
        /// Ներգրավող ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }
        /// <summary>
        /// Հաճ.հասանելիության տեսակը
        /// </summary>
        public byte? PermissionType { get; set; }

        public static HBApplication GetHBApplication(ulong customerNumber)
        {
            HBApplication hbApplication = HBApplicationDB.GetHBApplication(customerNumber);

            return hbApplication;
        }


    }
}
