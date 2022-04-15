using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking
{
    class PayerLinkPaymentOrderDB
    {

        public static ActionResult Save(PayerLinkPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "pr_save_link_payment_payer";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@type", SqlDbType.Int).Value = order.Type;
            cmd.Parameters.Add("@sub_type", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@description", SqlDbType.NVarChar, 50).Value = order.Description;
            cmd.Parameters.Add("@short_id", SqlDbType.NVarChar, 20).Value = order.ShortId;
            cmd.Parameters.Add("@check_box", SqlDbType.Bit).Value = order.CheckBox;
            cmd.Parameters.Add("@payer_name", SqlDbType.NVarChar, 20).Value = order.PayerName;
            cmd.Parameters.Add("@payer_card", SqlDbType.NVarChar, 20).Value = order.PayerCard;
            cmd.Parameters.Add("@terminal_id", SqlDbType.NVarChar, 20).Value = order.TerminalId;
            cmd.Parameters.Add("@arca_order_id", SqlDbType.NVarChar, 20).Value = order.ArcaOrderNumber;
            cmd.Parameters.Add("@auth_id", SqlDbType.NVarChar, 20).Value = order.ArcaAuthId;
            cmd.Parameters.Add("@request_number", SqlDbType.NVarChar, 20).Value = order.RRN;
            cmd.Parameters.Add("@bank_name", SqlDbType.NVarChar, 20).Value = order.BankName;
            cmd.Parameters.Add("@bank_country_code", SqlDbType.NVarChar, 20).Value = order.BankCountryCode;
            cmd.Parameters.Add("@bank_country_name", SqlDbType.NVarChar, 20).Value = order.BankCountryName;
            cmd.Parameters.Add("@action_code", SqlDbType.Int).Value = order.ActionCode;
            cmd.Parameters.Add("@order_status", SqlDbType.Int).Value = order.OrderStatus;
            cmd.Parameters.Add("@expiration", SqlDbType.NVarChar, 10).Value = order.Expiration;
            cmd.Parameters.Add("@set_Date", SqlDbType.DateTime).Value = Utility.GetCurrentOperDay();
            cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = order.user.userID;


            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


            cmd.ExecuteNonQuery();

            byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
            int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            order.Quality = OrderQuality.Draft;

            if (actionResult == 1)
            {
                result.ResultCode = ResultCode.Normal;
                result.Id = id;
            }
            else if (actionResult == 0)
            {
                result.ResultCode = ResultCode.Failed;
                result.Id = -1;
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք առկա է տվյալ ArCa վճարման կոդով գործարք, թե ոչ
        /// </summary>
        /// <param name="arCaOrderId"></param>
        /// <param name="shortId"></param>
        /// <returns></returns>
        internal static bool IfExistsLinkPaymentWithArcaOrderId(string arCaOrderId, string shortId)
        {

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"select 1 from Tbl_link_payment where arca_order_id = @arCaOrderId and short_id = @shortId", conn);
            cmd.Parameters.Add("@arCaOrderId", SqlDbType.NVarChar, 20).Value = arCaOrderId;
            cmd.Parameters.Add("@shortId", SqlDbType.NVarChar, 20).Value = shortId;

            return cmd.ExecuteReader().Read();
        }



    }
}
