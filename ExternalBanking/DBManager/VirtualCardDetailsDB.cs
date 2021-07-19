using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public static class VirtualCardDetailsDB
    {
        internal static VirtualCardDetails GetVirtualCardDetails(VirtualCardDetails virtualCardDetails)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @" select MotherName, toc.ValidityPeriod, 22000 + filial filial, ISNULL([E-mail_home], '') email, R.description reportReceivingType
                                ,cvv.cvv
								from tbl_visa_applications VA 
						        inner join tbl_type_of_card toc on VA.cardType = toc.ID
								inner join tbl_types_of_card_report_receiving R  on VA.Status = R.type_id
								left join tbl_CVV2Infos cvv  on cvv.app_ID = VA.app_id
						        where VA.app_id = @productID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productID", SqlDbType.Float).Value = virtualCardDetails.ProductId;

                    conn.Open();
                    
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            virtualCardDetails.MotherName = dr["MotherName"].ToString();
                            virtualCardDetails.CardValidityPeriod = int.Parse(dr["ValidityPeriod"].ToString());
                            virtualCardDetails.email = dr["email"].ToString();
                            virtualCardDetails.VCReportRecievingType = Utility.ConvertAnsiToUnicode(dr["reportReceivingType"].ToString());
                            virtualCardDetails.Cvv = dr["cvv"].ToString();
                        }
                    }                   
                }
            }
            return virtualCardDetails;
        }
    }
}
