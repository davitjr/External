using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Գործարք/գործողություն կատարող անձ (հաճախորդ) Opertion Performing Person
    /// </summary>
   public class OPPerson
    {
       /// <summary>
       /// Հաճախորդի համար (եթե գործարք կատարողը բանկի հաճախորդ է)
       /// </summary>
       public ulong CustomerNumber { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի անուն
       /// </summary>
       public string PersonName { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի ազգանուն
       /// </summary>
       public string PersonLastName { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի անձնագիր
       /// </summary>
       public string PersonDocument { get; set; }

       /// <summary>
       /// Լիազորված անձի ունիկալ համար
       /// </summary>
       public long AssignId { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի սոց քարտի համար
       /// </summary>
       public string PersonSocialNumber { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի սոց քարտից հրաժարվելու տեղեկանք
       /// </summary>
       public string PersonNoSocialNumber { get; set; }

       /// <summary>
       /// Փաստաթղթի տեսակ
       /// </summary>
       public ushort DocumentType { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի հասցե
       /// </summary>
       public string PersonAddress { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի հեռախոս
       /// </summary>
       public string PersonPhone { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի ռեզիդենտություն
       /// </summary>
       public short PersonResidence { get; set; }

       /// <summary>
       /// Գործարքը կատարող անձի ծննդյան ամսաթիվ
       /// </summary>
       public DateTime? PersonBirth { get; set; }

       public string PersonEmail { get; set; }
              
    }
}
