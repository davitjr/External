using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class CustomerCommunalCardDB
    {

        internal static List<CustomerCommunalCard> GetCustomerCommunalCards(ulong customerNumber)
        {

            List<CustomerCommunalCard> list = new List<CustomerCommunalCard>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_Communal_Customer_Card WHERE customer_number=@customerNumber AND quality=1", conn);
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CustomerCommunalCard communalcard = SetCustomerCommunalCard(dt.Rows[i]);
                    list.Add(communalcard);
                }

                return list;
            }
        }

        private static CustomerCommunalCard SetCustomerCommunalCard(DataRow row)
        {
            CustomerCommunalCard communalcard = new CustomerCommunalCard();
            communalcard.AbonentNumber = row["FindField1"] != DBNull.Value ? string.IsNullOrEmpty(row["FindField1"].ToString()) != true ? row["FindField1"].ToString() : null : null;
            communalcard.BranchCode = row["FindField2"] != DBNull.Value ? string.IsNullOrEmpty(row["FindField2"].ToString())!=true? row["FindField2"].ToString():null : null;
            communalcard.AbonentType = (AbonentTypes)Convert.ToUInt16(row["F_J"]);
            communalcard.CommunalType = Convert.ToUInt16(row["Communal_Type"]);
            communalcard.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
            communalcard.EditingDate = row["Editing_Date"] != DBNull.Value ? Convert.ToDateTime(row["Editing_Date"]) : default(DateTime?);
            communalcard.EditorSetNumber= row["Editor_Set_Number"] != DBNull.Value ? Convert.ToUInt16(row["Editor_Set_Number"]) : (ushort)0;
            communalcard.Id= Convert.ToUInt64(row["ID"]);
            communalcard.OpenDate = Convert.ToDateTime(row["Open_Date"]);
            communalcard.OpenerFilialCode= Convert.ToUInt16(row["Opener_FilialCode"]);
            communalcard.OpenerSetNumber= Convert.ToUInt16(row["Opener_Set_Number"]);
            communalcard.Quality= Convert.ToUInt16(row["Quality"]);
            return communalcard;
        }

        internal static ActionResult SaveCustomerCommunalCard(CustomerCommunalCard customerCommunalCard)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_Communal_Customer_Card
                                                (
                                                     ID
                                                    ,Communal_Type
                                                    ,F_J
                                                    ,FindField1
                                                    ,FindField2
                                                    ,Customer_Number
                                                    ,Open_Date
                                                    ,Opener_Set_Number
                                                    ,Editing_Date
                                                    ,Editor_Set_Number
                                                    ,Quality
                                                    ,Opener_FilialCode
                                                 )
                                                 VALUES
                                                (
                                                     (SELECT Max(ID)+1 FROM Tbl_Communal_Customer_Card)
                                                    ,@communalType
                                                    ,@аbonentType
                                                    ,@abonentNumber
                                                    ,@branchCode
                                                    ,@CustomerNumber
                                                    ,@openDate
                                                    ,@openerSetNumber
                                                    ,@editingDate
                                                    ,@editorSetNumber
                                                    ,@quality
                                                    ,@openerFilialCode
                                                )
                                                

                    ", conn);
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerCommunalCard.CustomerNumber;
                    cmd.Parameters.Add("@communalType", SqlDbType.TinyInt).Value = customerCommunalCard.CommunalType;
                    cmd.Parameters.Add("@аbonentType", SqlDbType.TinyInt).Value = (ushort)customerCommunalCard.AbonentType;
                    cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar,50).Value = string.IsNullOrEmpty(customerCommunalCard.AbonentNumber)?"": customerCommunalCard.AbonentNumber;
                    cmd.Parameters.Add("@branchCode", SqlDbType.NVarChar, 50).Value = string.IsNullOrEmpty(customerCommunalCard.BranchCode) ? "" : customerCommunalCard.BranchCode;
                    cmd.Parameters.Add("@openDate", SqlDbType.SmallDateTime).Value = customerCommunalCard.OpenDate.Date;
                    cmd.Parameters.Add("@openerSetNumber", SqlDbType.Int).Value = customerCommunalCard.OpenerSetNumber;
                    cmd.Parameters.Add("@editingDate", SqlDbType.SmallDateTime).Value = DBNull.Value;
                    cmd.Parameters.Add("@editorSetNumber", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@quality", SqlDbType.TinyInt).Value = customerCommunalCard.Quality;
                    cmd.Parameters.Add("@openerFilialCode", SqlDbType.Int).Value = customerCommunalCard.OpenerFilialCode;
                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    return result;
            }
        }

        internal static ActionResult ChangeCustomerCommunalCardQuality(CustomerCommunalCard customerCommunalCard)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"UPDATE Tbl_Communal_Customer_Card 
                                                    SET Quality=@quality,
                                                    Editor_Set_Number=@editorSetNumber,
                                                    Editing_Date=@editingDate
                                                    WHERE ID=@id", conn);
                cmd.Parameters.Add("@quality", SqlDbType.Float).Value = customerCommunalCard.Quality;
                cmd.Parameters.Add("@id", SqlDbType.Float).Value = customerCommunalCard.Id;
                cmd.Parameters.Add("@editorSetNumber", SqlDbType.Int).Value = customerCommunalCard.EditorSetNumber;
                cmd.Parameters.Add("@editingDate", SqlDbType.SmallDateTime).Value = customerCommunalCard.EditingDate.Value;
                cmd.ExecuteNonQuery();
                result.ResultCode = ResultCode.Normal;
            }


            return result;
        }

        internal static bool CheckCustomerCommunalCard(CustomerCommunalCard customerCommunalCard)
        {
            bool result = false;
           
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT ID FROM Tbl_Communal_Customer_Card
                                                  WHERE Communal_Type=@communalType AND FindField1=@abonentNumber 
                                                  AND FindField2=@branchCode AND Quality=1 AND F_J=@аbonentType
                                                  AND Customer_number=@customerNumber", conn);
                cmd.Parameters.Add("@communalType", SqlDbType.TinyInt).Value = customerCommunalCard.CommunalType;
                cmd.Parameters.Add("@аbonentType", SqlDbType.TinyInt).Value = (ushort)customerCommunalCard.AbonentType;
                cmd.Parameters.Add("@abonentNumber", SqlDbType.NVarChar, 50).Value = string.IsNullOrEmpty(customerCommunalCard.AbonentNumber) ? "" : customerCommunalCard.AbonentNumber;
                cmd.Parameters.Add("@branchCode", SqlDbType.NVarChar, 50).Value = string.IsNullOrEmpty(customerCommunalCard.BranchCode) ? "" : customerCommunalCard.BranchCode;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerCommunalCard.CustomerNumber;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result = true;
                }

            }
            return result;
        }

    }
}
