using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.Acbamat
{
    public static class AcbamatThirdPartyWithdrawalOrderDB
    {
        internal static void SaveAcbamatThirdPartyWithdrawalDetails(AcbamatThirdPartyWithdrawalOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_Save_Acbamat_Third_Party_Withdrawal_Order_Details";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@transaction_id", order.TransactionId);
            cmd.Parameters.AddWithValue("@atm_id", order.AtmId);
            cmd.Parameters.AddWithValue("@user_id", order.UserId);
            cmd.Parameters.AddWithValue("@reference_number", order.ReferenceNumber);
            cmd.Parameters.AddWithValue("@partner_id", order.PartnerId);
            cmd.Parameters.AddWithValue("@reg_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@amount", order.Amount);
            cmd.Parameters.AddWithValue("@organization_type", (short)order.ThirdPartyOrganizationType);

            cmd.ExecuteNonQuery();
        }

        internal static ActionResult SaveAcbamatThirdPartyWithdrawalOrder(AcbamatThirdPartyWithdrawalOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_Save_Acbamat_Third_Party_Withdrawal_Order";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@transaction_id", order.TransactionId);
            cmd.Parameters.AddWithValue("@atm_id", order.AtmId);
            cmd.Parameters.AddWithValue("@reg_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@amount", order.Amount);
            cmd.Parameters.AddWithValue("@currency", order.Currency);
            cmd.Parameters.AddWithValue("@customer_number", (double)order.CustomerNumber);
            cmd.Parameters.AddWithValue("@doc_number", order.OrderNumber);
            cmd.Parameters.AddWithValue("@username", userName);
            cmd.Parameters.AddWithValue("@source_type", order.Source);
            cmd.Parameters.AddWithValue("@operationFilialCode", (short)order.FilialCode);
            cmd.Parameters.AddWithValue("@oper_day", order.OperationDate);
            cmd.Parameters.AddWithValue("@doc_type", order.Type);
            cmd.Parameters.AddWithValue("@document_subtype", order.SubType);
            cmd.Parameters.AddWithValue("@organization_type", (short)order.ThirdPartyOrganizationType);
            cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });

            cmd.ExecuteNonQuery();

            byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            order.Quality = OrderQuality.Draft;

            if (actionResult == 1)
            {
                result.ResultCode = ResultCode.Normal;
                result.Id = order.Id;
            }
            else if (actionResult == 0)
            {
                result.ResultCode = ResultCode.Failed;
                result.Id = -1;
            }

            return result;
        }

        internal static void GetAcbamatThirdPartyWithdrawalOrder(AcbamatThirdPartyWithdrawalOrder order)
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT * FROM tbl_acbamat_third_party_withdrawal_order_details WHERE transaction_id = @transaction_Id", conn);

            cmd.Parameters.AddWithValue("@transaction_Id", order.TransactionId);
            dt.Load(cmd.ExecuteReader());
            if (dt.Rows.Count > 0)
            {
                order.AtmId = dt.Rows[0]["atm_id"].ToString();
                order.PartnerId = dt.Rows[0]["partner_id"].ToString();
                order.TerminalID = dt.Rows[0]["atm_id"].ToString();
                order.UserId = dt.Rows[0]["user_id"].ToString();
                order.ReferenceNumber = dt.Rows[0]["reference_number"].ToString();
                order.Amount = float.Parse(dt.Rows[0]["amount"].ToString());
                order.ThirdPartyOrganizationType = (ThirdPartyOrganizationTypes)Convert.ToInt16(dt.Rows[0]["organization_type"].ToString());
            }
        }
    }
}
