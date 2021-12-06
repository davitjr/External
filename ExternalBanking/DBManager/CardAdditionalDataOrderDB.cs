using ExternalBanking.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    static class CardAdditionalDataOrderDB
    {
        internal static ActionResult Save(CardAdditionalDataOrder order, User user, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNewCardAdditionalDataOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = user.userName;

                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.PlasticCard.ProductId;
                    cmd.Parameters.Add("@AdditionID", SqlDbType.SmallInt).Value = order.CardAdditionalData.AdditionID;
                    cmd.Parameters.Add("@AdditionValue", SqlDbType.VarChar, 255).Value = order.CardAdditionalData.AdditionValue;
                    cmd.Parameters.Add("@KeyNum", SqlDbType.Int).Value = order.CardAdditionalData.CardAdditionalDataID;

                    //cmd.Parameters.Add("@KeyNum", SqlDbType.Int).Value

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();
                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9)
                    {
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 10)
                    {
                        result.Id = order.Id;
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0 || actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }
                }
            }

            return result;
        }

        internal static CardAdditionalDataOrder GetCardAdditionalDataOrder(CardAdditionalDataOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT HB.document_number document_number, HB.document_type document_type, HB.document_subtype document_subtype, HB.doc_ID, 
                                                  HB.operationFilialCode operationFilialCode, HB.quality quality, registration_date, operation_date, HB.description , hbs.description subdescription, HB.source_type,
                                                 HB.confirmation_date, HB.order_group_id, ad.AdditionDescription, dd.AdditionVlaue, va.cardnumber
                                        FROM tbl_hb_documents hb
                                        INNER JOIN tbl_card_additional_data_details dd on hb.doc_id = dd.doc_ID
                                        INNER JOIN Tbl_sub_types_of_HB_products hbs on hb.document_subtype = hbs.document_sub_type and hb.document_type = hbs.document_type
                                        INNER JOIN tbl_visa_applications va on dd.app_id = va.app_id
                                        INNER JOIN[dbo].[Tbl_Type_of_VisaApp_Add] ad on ad.AdditionID = dd.AdditionID
                                       WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END and HB.doc_ID= @doc_id ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                   using SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        order.OrderNumber = dr["document_number"].ToString();
                        order.Type = (OrderType)dr["document_type"];
                        order.SubType = Convert.ToByte(dr["document_subtype"]);
                        order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                        order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                        order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                        order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                        order.SubTypeDescription = Utility.ConvertAnsiToUnicode(dr["subdescription"].ToString());
                        order.CardAdditionalData.AdditionDescription = Utility.ConvertAnsiToUnicode(dr["AdditionDescription"].ToString());
                        order.CardAdditionalData.AdditionValue = dr["AdditionVlaue"].ToString();
                        order.PlasticCard.CardNumber = dr["cardnumber"].ToString();
                    }

                }
            }
            return order;
        }
    }
}
