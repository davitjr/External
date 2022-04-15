using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class SearchCardsDB
    {
        internal static List<SearchCardResult> SearchCards(SearchCards searchParams)
        {
            List<SearchCardResult> cardList = new List<SearchCardResult>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    sql = @"SELECT Top 1000 app_id,visa_number as card_number,currency ,closing_date,
                                t.ApplicationsCardType as Card_Type,
                                isnull(CUST.[description],'') + isnull(CUST.[name],'') + ' ' +  isnull(CUST.[lastname],'') as card_holder,
                                vn.filialcode,vn.customer_number,vn.card_account 
                                FROM Tbl_Visa_Numbers_Accounts vn
                                INNER JOIN tbl_type_of_card t on vn.card_type=t.id
                                INNER JOIN V_CustomerDesription CUST ON VN.customer_number = CUST.customer_number 
                                WHERE 1=1 ";

                    if (!searchParams.includeCloseCards)
                    {
                        sql = sql + " and vn.closing_date is null";
                    }

                    if (searchParams.currency != null)
                    {
                        sql = sql + " and vn.currency =@currency";
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = searchParams.currency;
                    }

                    if (searchParams.filialCode != 0)
                    {
                        sql = sql + " and vn.filialcode =@filialCode";
                        cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = searchParams.filialCode;
                    }

                    if (searchParams.customerNumber != null && searchParams.customerNumber.Length == 12)
                    {
                        sql = sql + " and vn.customer_number = @customerNumber";
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = searchParams.customerNumber;
                    }

                    if (searchParams.cardNumber != null && searchParams.cardNumber != "")
                    {
                        if (searchParams.cardNumber.Length == 15)
                        {
                            if (searchParams.cardNumber.Substring(0, 1) == "3")
                            {
                                sql = sql + " and vn.visa_number=@cardNumber";
                            }
                            else
                            {
                                sql = sql + " and vn.visa_number like '%'+@cardNumber+'' ";
                            }
                        }
                        else if (searchParams.cardNumber.Length == 16)
                        {
                            sql = sql + " and vn.visa_number=@cardNumber";
                        }
                        else
                        {
                            sql = sql + " and vn.visa_number like '%' + @cardNumber ";
                        }

                        cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = searchParams.cardNumber;
                    }

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;


                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            cardList = new List<SearchCardResult>();
                        }

                        while (dr.Read())
                        {
                            SearchCardResult oneResult = new SearchCardResult();
                            oneResult.ProductId = long.Parse(dr["app_id"].ToString());
                            oneResult.CardNumber = dr["card_number"].ToString();
                            oneResult.Currency = dr["currency"].ToString();
                            oneResult.CardType = dr["Card_Type"].ToString();
                            oneResult.FilialCode = int.Parse(dr["filialcode"].ToString());

                            if (!String.IsNullOrEmpty(dr["closing_date"].ToString()))
                            {
                                oneResult.ClosingDate = DateTime.Parse(dr["closing_date"].ToString());
                            }

                            oneResult.CardHolderDescription = dr["card_holder"].ToString();
                            oneResult.CustomerNumber = ulong.Parse(dr["customer_number"].ToString());
                            oneResult.CardAccount = Account.GetAccount(dr["card_account"].ToString());
                            cardList.Add(oneResult);
                        }
                    }
                }
            }

            return cardList;
        }
    }
}
