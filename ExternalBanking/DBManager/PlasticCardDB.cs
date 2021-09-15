using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    static class PlasticCardDB
    {

        internal static List<PlasticCard> GetCardsForRegistration(ulong customerNumber, int filialCode)
        {
            List<PlasticCard> cardList = new List<PlasticCard>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_get_cards_for_registration";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Float).Value = filialCode;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        //Card card1 = SetCard(row);
                        PlasticCard card = new PlasticCard();

                        if (row != null)
                        {
                            card.CardChangeType = CardChangeType.New;
                            card.ProductId = ulong.Parse(row["app_id"].ToString());
                            card.CardNumber = row["Cardnumber"].ToString();
                            card.Currency = row["BillingCurrency"].ToString();
                            card.MainCardNumber = row["Maincardnumber"].ToString();
                            card.FilialCode = int.Parse(row["Filial"].ToString());
                            card.ExpiryDate = row["ExpiryDate"].ToString();
                            card.SupplementaryType = (SupplementaryType)short.Parse(row["supplementary_type"].ToString());
                            card.CardType = uint.Parse(row["cardType"].ToString());
                            card.CardTypeDescription = row["Card_Type_description"].ToString();
                            card.CardSystem = int.Parse(row["CardSystemId"].ToString());
                            card.CardHolderName = Utility.ConvertAnsiToUnicode(row["name"].ToString());
                            card.CardHolderLastName = Utility.ConvertAnsiToUnicode(row["lastname"].ToString());
                            cardList.Add(card);
                        }
                    }
                }
            }

            return cardList;
        }

        internal static PlasticCard GetPlasticCard(string cardNumber)
        {
            PlasticCard card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql =
                    @"SELECT Cardnumber as card_number,BillingCurrency,ExpiryDate, ValidFrom,  V.CardType, V.customer_number, MainCardnumber,  Filial,V.app_id ,RelatedOfficeNumber,ss.CardSystemType + ' ' + Upper(tp.CardType)  as Card_Type_description,tp.CardSystemId,C.name,C.lastname,
                        CASE WHEN ISNULL(S.customer_number,0)=0 THEN 1 ELSE CASE WHEN  S.customer_number <> v.customer_number THEN 2 ELSE 3 END END AS supplementary_type, typeID, add_inf
                        FROM Tbl_VISA_applications V 
                        INNER JOIN tbl_type_of_card  tp
                        ON V.cardType=tp.id 
                        INNER JOIN tbl_type_of_CardSystem ss
                        ON tp.CardSystemId=ss.Id 
                        LEFT JOIN Tbl_SupplementaryCards S on V.app_id  = S.app_id and S.app_id<>S.main_app_id 
                        LEFT JOIN (SELECT name , lastname ,customer_number FROM V_CustomerDesription ) C ON C.customer_number = CASE WHEN ISNULL(S.customer_number,0)=0 THEN V.customer_number ELSE S.customer_number END 
                        LEFT JOIN tbl_cardChanges CC ON CC.app_id = V.app_id                        
                        LEFT JOIN Tbl_Visa_Numbers_Accounts  vn ON vn.App_Id = CC.old_app_id 
                        WHERE V.Cardnumber = @cardNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetPlasticCard(row);
                    }
                }
            }

            return card;
        }

        internal static List<PlasticCard> GetSupplementaryPlasticCards(string mainCardNumber)
        {
            List<PlasticCard> cards = new List<PlasticCard>();
            

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql =
                    @"SELECT Cardnumber as card_number,BillingCurrency,ExpiryDate, ValidFrom,  V.CardType, V.customer_number, MainCardnumber,  Filial,V.app_id ,RelatedOfficeNumber,ss.CardSystemType + ' ' + Upper(tp.CardType)  as Card_Type_description,tp.CardSystemId,C.name,C.lastname,
                        CASE WHEN ISNULL(S.customer_number,0)=0 THEN 1 ELSE CASE WHEN  S.customer_number <> v.customer_number THEN 2 ELSE 3 END END AS supplementary_type, typeID, add_inf
                        FROM Tbl_VISA_applications V 
                        INNER JOIN tbl_type_of_card  tp
                        ON V.cardType=tp.id 
                        INNER JOIN tbl_type_of_CardSystem ss
                        ON tp.CardSystemId=ss.Id 
                        LEFT JOIN Tbl_SupplementaryCards S on V.app_id  = S.app_id and S.app_id<>S.main_app_id 
                        LEFT JOIN (SELECT name , lastname ,customer_number FROM V_CustomerDesription ) C ON C.customer_number = CASE WHEN ISNULL(S.customer_number,0)=0 THEN V.customer_number ELSE S.customer_number END 
                        LEFT JOIN tbl_cardChanges CC ON CC.app_id = V.app_id                        
                        LEFT JOIN Tbl_Visa_Numbers_Accounts vn ON vn.App_Id = V.app_id  
                        WHERE V.MainCardnumber = @mainCardNumber AND Cardnumber<>MainCardnumber AND V.cardStatus='NORM' AND ISNULL(GivenDate, 0) = 0 AND ISNULL(vn.App_id,0)=0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar).Value = mainCardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    foreach(DataRow row in dt.Rows)
                    {
                        PlasticCard card = SetPlasticCard(row);
                        cards.Add(card);
                    }
                }
            }

            return cards;
        }

        internal static PlasticCard GetPlasticCard(ulong productId ,bool productidType)
        {
            PlasticCard card = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql =
                    @"SELECT Cardnumber as card_number,BillingCurrency,ExpiryDate, ValidFrom,  V.CardType, V.customer_number, MainCardnumber,  Filial,V.app_id ,RelatedOfficeNumber,ss.CardSystemType + ' ' + Upper(tp.CardType)  as Card_Type_description,tp.CardSystemId,C.name,C.lastname,
                        CASE WHEN ISNULL(S.customer_number,0)=0 THEN 1 ELSE CASE WHEN  S.customer_number <> v.customer_number THEN 2 ELSE 3 END END AS supplementary_type, typeID, add_inf
                        FROM Tbl_VISA_applications V 
                        INNER JOIN tbl_type_of_card  tp
                        ON V.cardType=tp.id 
                        INNER JOIN tbl_type_of_CardSystem ss
                        ON tp.CardSystemId=ss.Id 
                        LEFT JOIN Tbl_SupplementaryCards S on V.app_id  = S.app_id and S.app_id<>S.main_app_id 
                        LEFT JOIN (SELECT name , lastname ,customer_number FROM V_CustomerDesription ) C ON C.customer_number = CASE WHEN ISNULL(S.customer_number,0)=0 THEN V.customer_number ELSE S.customer_number END 
                        LEFT JOIN tbl_cardChanges CC ON CC.app_id = V.app_id                        
                        LEFT JOIN Tbl_Visa_Numbers_Accounts  vn ON vn.App_Id = CC.old_app_id 
                        ";
                if (productidType)
                {
                    sql += " WHERE V.app_id = @productId  ";
                }
                else
                {
                    sql += " WHERE CC.old_app_id = @productId";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    //cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        card = SetPlasticCard(row);

                    }



                }
            }

            return card;
        }

        private static PlasticCard SetPlasticCard(DataRow row)
        {
            PlasticCard card = new PlasticCard();

            if (row != null)
            {
                card.ProductId = ulong.Parse(row["app_id"].ToString());
                card.CardNumber = row["card_number"].ToString();
                card.Currency = row["BillingCurrency"].ToString();
                card.ExpiryDate = row["ExpiryDate"].ToString();
                card.CardType = uint.Parse(row["CardType"].ToString());
                card.MainCardNumber = row["MainCardnumber"].ToString();
                card.FilialCode = int.Parse(row["filial"].ToString());
                card.RelatedOfficeNumber = int.Parse(row["RelatedOfficeNumber"].ToString());
                card.SupplementaryType = (SupplementaryType)short.Parse(row["supplementary_type"].ToString());
                card.CardType = uint.Parse(row["cardType"].ToString());
                card.CardTypeDescription = row["Card_Type_description"].ToString();
                card.CardSystem = int.Parse(row["CardSystemId"].ToString());
                card.CardHolderName = Utility.ConvertAnsiToUnicode(row["name"].ToString());
                card.CardHolderLastName = Utility.ConvertAnsiToUnicode(row["lastname"].ToString());
                
                if (row["typeID"] == DBNull.Value)
                {
                    card.CardChangeType = CardChangeType.New;
                }
                else
                {
                    card.CardChangeType = (CardChangeType)short.Parse(row["typeID"].ToString());
                }
                card.AddInf = Utility.ConvertAnsiToUnicode(row["add_inf"].ToString());

            }
            return card;
        }
        
        internal static int UpdateCardStatusWithoutOrder(ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"UPDATE tbl_visa_applications
				                    SET CardStatus='NORM',
				                    GivenDate=@operDay,
				                    GivenBy=@setNumber
				                    WHERE app_id=@app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.AddWithValue("@setNumber", 88);
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    return cmd.ExecuteNonQuery();                    
                }
            }
        }

        internal static bool IsThirdAttachedCard(PlasticCard plasticCard)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT COUNT(*) AS linked_card_count FROM Tbl_Visa_Numbers_Accounts WHERE main_card_number = @mainCardNumber AND closing_date IS NULL  ";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.Float).Value = plasticCard.MainCardNumber;

                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        int linked_card_count = int.Parse(row["linked_card_count"].ToString());
                        if (linked_card_count >= 2)
                            return true;

                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի NORM ստատուսով հիմնական քարտերը
        /// </summary>
        /// <param name="CustomerNumber"></param>
        /// <returns></returns>
        internal static List<PlasticCard> GetCustomerMainCards(ulong CustomerNumber, bool getAll = false, bool getNew = false)
        {
            List<PlasticCard> cardList = new List<PlasticCard>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT VA.app_id, VA.cardnumber as card_number, VA.BillingCurrency as currency, ts.CardSystemType + ' ' + Upper(t.CardType)  as Card_Type_description, 
                                VA.cardtype as card_type, VA.RelatedOfficeNumber,
                                CASE WHEN vn.open_date IS NULL THEN VA.RegDate ELSE vn.open_date END open_date,
                                CASE WHEN vn.filialcode IS NULL THEN (filial + 22000) ELSE vn.filialcode END filialcode
                                FROM tbl_visa_applications VA
                                LEFT JOIN Tbl_Visa_Numbers_Accounts vn on VA.app_id = vn.App_Id
                                INNER JOIN tbl_type_of_card t on VA.cardType=t.id
                                INNER JOIN tbl_type_of_CardSystem ts on t.CardSystemID = ts.ID
                                WHERE (CardStatus = 'NORM' OR CardStatus = @getNew) AND VA.customer_number=@customerNumber";

                if (!getAll)
                    sql += " AND ISNULL(cardnumber,0) = ISNULL(MaincardNumber,0)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = CustomerNumber;
                    cmd.Parameters.Add("@getNew", SqlDbType.NVarChar, 15).Value = getNew ? "NEW" : "";
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<PlasticCard>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        PlasticCard card = new PlasticCard();

                        card.ProductId = ulong.Parse(row["app_id"].ToString());
                        card.CardNumber = row["card_number"] == DBNull.Value ? "" : row["card_number"].ToString();
                        card.Currency = row["currency"].ToString();
                        card.CardType = uint.Parse(row["card_type"].ToString());
                        card.SupplementaryType = SupplementaryType.Main;
                        card.CardTypeDescription = row["Card_Type_description"].ToString();
                        card.RelatedOfficeNumber = int.Parse(row["RelatedOfficeNumber"].ToString());
                        card.OpenDate = DateTime.Parse(row["open_date"].ToString());
                        card.FilialCode = int.Parse(row["filialcode"].ToString());
                        cardList.Add(card);
                    }
                }
            }

            return cardList;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի NORM կամ RNEW ստատուսով քարտերը
        /// </summary>
        /// <param name="CustomerNumber"></param>
        /// <returns></returns>
        internal static List<PlasticCard> GetCustomerCards(ulong CustomerNumber)
        {
            List<PlasticCard> cardList = new List<PlasticCard>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT VA.app_id, VA.cardnumber as card_number, VA.BillingCurrency as currency, ts.CardSystemType + ' ' + Upper(t.CardType)  as Card_Type_description, 
                                VA.cardtype as card_type, VA.RelatedOfficeNumber,
                                CASE WHEN vn.open_date IS NULL THEN VA.RegDate ELSE vn.open_date END open_date,
                                CASE WHEN vn.filialcode IS NULL THEN (filial + 22000) ELSE vn.filialcode END filialcode
                                FROM tbl_visa_applications VA
                                LEFT JOIN Tbl_Visa_Numbers_Accounts vn on VA.app_id = vn.App_Id
                                INNER JOIN tbl_type_of_card t on VA.cardType=t.id
                                INNER JOIN tbl_type_of_CardSystem ts on t.CardSystemID = ts.ID
                                WHERE VA.customer_number=@customerNumber 
                                        AND (CardStatus = 'NORM' OR (CardStatus = 'RNEW' AND vn.validation_date>=CAST( GETDATE() AS Date) AND vn.closing_date IS NULL))";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = CustomerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<PlasticCard>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        PlasticCard card = new PlasticCard();

                        card.ProductId = ulong.Parse(row["app_id"].ToString());
                        card.CardNumber = row["card_number"].ToString();
                        card.Currency = row["currency"].ToString();
                        card.CardType = uint.Parse(row["card_type"].ToString());
                        card.SupplementaryType = SupplementaryType.Main;
                        card.CardTypeDescription = row["Card_Type_description"].ToString();
                        card.RelatedOfficeNumber = int.Parse(row["RelatedOfficeNumber"].ToString());
                        card.OpenDate = DateTime.Parse(row["open_date"].ToString());
                        card.FilialCode = int.Parse(row["filialcode"].ToString());
                        cardList.Add(card);
                    }
                }
            }

            return cardList;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտերը լրացուցիչ տվյալների հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<PlasticCard> GetCustomerPlasticCardsForAdditionalData(ulong customerNumber, bool IsClosed)
        {
            List<PlasticCard> plasticCards = new List<PlasticCard>();
            string sql = @"SELECT VA.app_id, VA.cardnumber as card_number, ExpiryDate
                                FROM tbl_visa_applications VA ";
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                if (IsClosed)
                {
                    sql += @"  WHERE  VA.customer_number=@customerNumber and CardStatus <> 'NORM' and  VA.cardnumber is not null ";
                }
                else
                {
                    sql += @"  WHERE  CardStatus = 'NORM'  AND VA.customer_number=@customerNumber";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        PlasticCard card = new PlasticCard();

                        card.ProductId = ulong.Parse(row["app_id"].ToString());
                        card.CardNumber = row["card_number"].ToString();
                        card.ExpiryDate = row["ExpiryDate"].ToString();

                        plasticCards.Add(card);
                    }
                }
            }
            return plasticCards;
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդի հիմնական քարտերը, լրացուցիչ քարտի հայտի համար 
        /// </summary>
        /// <param name="CustomerNumber"></param>
        /// <returns></returns>
        internal static List<PlasticCard> GetCustomerMainCardsForAttachedCardOrder(ulong CustomerNumber)
        {
            List<PlasticCard> cardList = new List<PlasticCard>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT VA.app_id, VA.cardnumber as card_number, VA.BillingCurrency as currency, ts.CardSystemType + ' ' + Upper(t.CardType)  as Card_Type_description, 
                                VA.cardtype as card_type, VA.RelatedOfficeNumber,
                                CASE WHEN vn.open_date IS NULL THEN VA.RegDate ELSE vn.open_date END open_date,
                                CASE WHEN vn.filialcode IS NULL THEN (filial + 22000) ELSE vn.filialcode END filialcode
                                FROM tbl_visa_applications VA
                                LEFT JOIN Tbl_Visa_Numbers_Accounts vn on VA.app_id = vn.App_Id
                                INNER JOIN tbl_type_of_card t on VA.cardType=t.id
                                INNER JOIN tbl_type_of_CardSystem ts on t.CardSystemID = ts.ID
                                INNER JOIN (SELECT main_card_type
											FROM Tbl_additional_card_conditions_checking
											WHERE  sub_type = 1
											GROUP BY main_card_type ) acc on acc.main_card_type =t.ID
                                WHERE  CardStatus = 'NORM' AND VA.customer_number=@customerNumber AND cardnumber = MaincardNumber";           

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = CustomerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        cardList = new List<PlasticCard>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        PlasticCard card = new PlasticCard();

                        card.ProductId = ulong.Parse(row["app_id"].ToString());
                        card.CardNumber = row["card_number"].ToString();
                        card.Currency = row["currency"].ToString();
                        card.CardType = uint.Parse(row["card_type"].ToString());
                        card.SupplementaryType = SupplementaryType.Main;
                        card.CardTypeDescription = row["Card_Type_description"].ToString();
                        card.RelatedOfficeNumber = int.Parse(row["RelatedOfficeNumber"].ToString());
                        card.OpenDate = DateTime.Parse(row["open_date"].ToString());
                        card.FilialCode = int.Parse(row["filialcode"].ToString());
                        cardList.Add(card);
                    }
                }
            }

            return cardList;
        }
    }
}
