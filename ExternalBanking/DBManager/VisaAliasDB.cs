using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public static class VisaAliasDB
    {
        internal static ActionResult VisaAliasOrder(VisaAliasOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_Visa_Alias_Order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.NVarChar, 20).Value = order.SubType;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.user.filialCode;
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@ReasonTypeDescription", SqlDbType.NVarChar, 20).Value = order.ReasonTypeDescription;
                    cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = order.Alias;
                    cmd.Parameters.Add("@RecipientPrimaryAccountNumber", SqlDbType.NVarChar, 20).Value = order.RecipientPrimaryAccountNumber;

                    SqlParameter res = new SqlParameter("@result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    return result;
                }
            }
        }

        internal static VisaAliasOrder GetVisaAliasOrder(long orderId)
        {
            VisaAliasOrder result = new VisaAliasOrder();
            DataTable dt = new DataTable();

            string sql = @"SELECT   hb.customer_number,
                                    hb.registration_date, 
                                    hb.doc_ID,
                                    hb.document_subtype,
                                    hb.quality,
                                    hb.operation_date,
                                    hb.source_type,
                                    hb.document_type,
                                    hb.filial,
                                    hb.sender_phone,
                                    hb.reason_type_description,
                                    dbo.fnc_convertAnsiToUnicode(q.description_arm) description_arm
                                        FROM Tbl_HB_documents hb inner join Tbl_types_of_HB_quality q on q.quality = hb.quality
                                        WHERE hb.Doc_ID = @DocID";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = orderId;

                    dt.Load(cmd.ExecuteReader());

                    result.Id = Convert.ToInt64(dt.Rows[0]["doc_ID"].ToString());
                    result.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    result.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    result.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    result.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    result.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    result.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                    result.Alias = dt.Rows[0]["sender_phone"].ToString();
                    result.ReasonTypeDescription = dt.Rows[0]["reason_type_description"].ToString();
                    result.QualityDescription = dt.Rows[0]["description_arm"].ToString();

                    // result.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"]);
                }

                return result;
            }
        }

        internal static VisaAliasOrder Get(VisaAliasOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT QH.change_date,
                                                         HB.document_number,
                                                         HB.document_type,
                                                         HB.document_subtype,
                                                         HB.quality,
                                                         HB.doc_ID,
                                                         HB.operation_date ,
                                                         HB.operationFilialCode,
                                                         HB.order_group_id,
                                                         HB.confirmation_date,
                                                         DT.RecipientPrimaryAccountNumber,
                                                         DT.RecipientFulltName,
                                                         DT.Alias,
                                                         DT.CardType										 
                                    FROM Tbl_HB_documents AS HB 
		                            INNER JOIN tbl_visa_alias_order_details AS DT ON HB.doc_ID = DT.doc_id
                                    INNER JOIN tbl_hb_quality_history AS QH on HB.doc_ID = QH.Doc_ID and QH.quality = 1
                                    WHERE customer_number=CASE WHEN @customer_number = 0 THEN customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["change_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = Convert.ToDateTime(dt.Rows[0]["operation_date"]);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.Id = Convert.ToUInt32(dt.Rows[0]["doc_ID"]);
                    order.RecipientPrimaryAccountNumber = dt.Rows[0]["RecipientPrimaryAccountNumber"].ToString();
                    order.RecipientFullName = dt.Rows[0]["RecipientFulltName"].ToString();
                    order.CardType = dt.Rows[0]["CardType"].ToString();
                    order.Alias = dt.Rows[0]["Alias"].ToString();
                }
            }

            return order;
        }

        public static string GetVisaAliasGuidByCutomerNumber(ulong customerNumber)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1  [Guid]							 
                                    FROM tbl_visa_alias
                                    WHERE customernumber= @customer_number ", conn);

            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
            return cmd.ExecuteScalar().ToString();
        }
        public static CardHolderAndCardType GetCardTypeAndCardHolder(string cardNumber)
        {
            DataTable dt = new DataTable();
            CardHolderAndCardType cardHolderAndCardType = new CardHolderAndCardType();
            string sql = @"SELECT TOP 1 c.name_eng, c.lastname_eng, t.ApplicationsCardType FROM Tbl_VISA_applications ap 
                           INNER JOIN tbl_type_of_card t ON ap.cardType = t.ID 
                           INNER JOIN [Tbl_Customers;] c ON c.customer_number = ap.customer_number WHERE Cardnumber = @Cardnumber";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@Cardnumber", SqlDbType.Float).Value = cardNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        cardHolderAndCardType.CardHolderFirsName = dt.Rows[0]["name_eng"].ToString();
                        cardHolderAndCardType.CardHolderLastName = dt.Rows[0]["lastname_eng"].ToString();
                        cardHolderAndCardType.CardTypeDescription = dt.Rows[0]["ApplicationsCardType"].ToString();
                    }
                }
                return cardHolderAndCardType;
            }
        }
    }
}
