using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class OperDayOptions
    {
        public DateTime OperDay { get; set; }

        /// <summary>
        /// PK
        /// </summary>
        public int NumberOfSet { get; set; }


        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են ACCOPERBASE ի Tbl_type_of_oper_day_closing_options -ում)
        /// </summary>
        public OperDayOptionsType Code{ get; set; }
        
        
        /// <summary>
        /// նկարագրություն
        /// </summary>
        public string CodeDescription { get; set; }

        /// <summary>
        /// Միացված է թե անջատած
        /// </summary>
        public bool IsEnabled { get; set; }

        public DateTime RegistrationDate { get; set; }



        public static ActionResult SaveOperDayOptions(List<OperDayOptions> list)
        {
            return OperDayOptionsDB.SaveOperDayOptions(list);
        }
    }
}
