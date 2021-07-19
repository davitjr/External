
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForREST
{

    public enum CommunalTypes : short
    {
        /// <summary>
        ///«Հայաստանի Էլեկտրական ցանցեր» ծառայության դիմաց վճար
        /// </summary>
        ENA = 3,
        /// <summary>
        /// «Հայռուսգազարդ» ծառայության սպառած գազի դիմաց վճար
        /// </summary>
        Gas = 4,
        /// <summary>
        /// «ՀայՋրմուղԿոյուղի» ծառայության դիմաց վճար
        /// </summary>
        ArmWater = 5,
        /// <summary>
        /// «Երևան Ջուր» ծառայության դիմաց վճար
        /// </summary>
        YerWater = 6,
        /// <summary>
        /// «ԱրմենՏել» ծառայության դիմաց վճար
        /// </summary>
        ArmenTel = 7,
        /// <summary>
        /// «Ղ-Տելեկոմ» ծառայության դիմաց վճար
        /// </summary>
        VivaCell = 8,
        /// <summary>
        /// «ՅուՔոմ» ծառայության դիմաց վճար
        /// </summary>
        UCom = 9,
        /// <summary>
        /// «Օրանժ» ծառայության դիմաց վճար
        /// </summary>
        Orange = 10,
        /// <summary>
        /// «Հայռուսգազարդ» ծառայության տեխ. սպասարկման դիմաց վճար
        /// </summary>
        GasService = 11
    }

	public class UtilityPaymentOrder : Order
	{
		/// Հաճախորդի համար
		public ulong CustomerNumber { get; set; }

		public double ServiceAmount { get; set; }
		public string Code { get; set; }
		public string Branch { get; set; }
		public int AbonentType { get; set; }
		public Account DebitAccount { get; set; }

		public CommunalTypes CommunalType { get; set; }
		public int fizJur { get; set; }

	}
}
