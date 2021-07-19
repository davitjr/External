using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Data;

namespace ExternalBanking
{
    public class PreOrderDetails
    {
        /// <summary>
        /// Նախանական հայտի համար
        /// </summary>
        public int PreOrderID { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Պրոդուկտի հերթական համար
        /// </summary>
        public ulong AppID { get; set; }
        /// <summary>
        /// Նախնական հայտի կարգավիճակ
        /// </summary>
        public PreOrderQuality Quality { get; set; }
        /// <summary>
        /// Նախնական հայտի կարգավիճակի նկարագրություն
        /// </summary>
        public String QualityDescription { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public Double Amount { get; set; }
        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }
        public static DataTable ConvertToDataTable(List<PreOrderDetails> detList)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("customer_number");
            dt.Columns.Add("app_ID");
            //dt.Columns.Add("quality");
            dt.Columns.Add("product_type");
            if (detList != null)
            {
                foreach (PreOrderDetails d in detList)
                {
                    DataRow dr = dt.NewRow();
                    dr["customer_number"] = (ulong)d.CustomerNumber;
                    dr["app_ID"] = d.AppID;
                    //dr["quality"] = (short)d.Quality;
                    dr["product_type"] = (int)d.ProductType;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
    }
}
