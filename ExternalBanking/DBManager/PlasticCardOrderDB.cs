using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class PlasticCardOrderDB
    {
        internal static ActionResult Save(PlasticCardOrder order, ACBAServiceReference.User user, SourceType source, string userName = "")
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNewPlasticCardOrder";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                    {
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                        cmd.Parameters.Add("@serviceFeePeriodicityType", SqlDbType.Int).Value = order.ServiceFeePeriodicityType;
                        if (order.PlasticCard.SupplementaryType == SupplementaryType.Linked)
                        {
                            cmd.Parameters.Add("@linkedcardlimit", SqlDbType.Float).Value = order.LinkedCardLimit;
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = user.userName;
                    }
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;

                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = order.PlasticCard.FilialCode;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar, 16).Value = order.PlasticCard.MainCardNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.PlasticCard.Currency;
                    cmd.Parameters.Add("@motherName", SqlDbType.NVarChar, 24).Value = order.MotherName;
                    cmd.Parameters.Add("@relatedOfficeNumber", SqlDbType.Int).Value = order.PlasticCard.RelatedOfficeNumber;
                    cmd.Parameters.Add("@cardType", SqlDbType.SmallInt).Value = order.PlasticCard.CardType;
                    cmd.Parameters.Add("@cardChangeType", SqlDbType.SmallInt).Value = (short)order.PlasticCard.CardChangeType;
                    cmd.Parameters.Add("@cardSystem", SqlDbType.Int).Value = order.PlasticCard.CardSystem;
                    cmd.Parameters.Add("@involvingSetNumber", SqlDbType.Int).Value = order.InvolvingSetNumber;
                    cmd.Parameters.Add("@servingSetNumber", SqlDbType.Int).Value = order.ServingSetNumber;

                    if (InfoDB.CommunicationTypeExistence(order.CustomerNumber) == 1)
                        cmd.Parameters.Add("@cardReportReceivingType", SqlDbType.Int).Value = DBNull.Value;
                    else
                        cmd.Parameters.Add("@cardReportReceivingType", SqlDbType.Int).Value = order.CardReportReceivingType;





                    cmd.Parameters.Add("@cardPINCodeReceivingType", SqlDbType.Int).Value = order.CardPINCodeReceivingType;
                    cmd.Parameters.Add("@cardReceivingType", SqlDbType.Int).Value = order.CardReceivingType;
                    cmd.Parameters.Add("@cardApplicationAcceptanceType", SqlDbType.Int).Value = order.CardApplicationAcceptanceType;

                    cmd.Parameters.Add("@organisationNameEng", SqlDbType.NVarChar).Value = order.OrganisationNameEng;
                    cmd.Parameters.Add("@cardHolderCustomerNumber", SqlDbType.Float).Value = order.CardHolderCustomerNumber;
                    cmd.Parameters.Add("@cardSMSPhone", SqlDbType.NVarChar).Value = order.CardSMSPhone == string.Empty ? "37400000000" : order.CardSMSPhone;
                    if (order.PlasticCard.SupplementaryType == SupplementaryType.Main)
                    {
                        cmd.Parameters.Add("@reportReceivingEmail", SqlDbType.NVarChar).Value = order.ReportReceivingEmail;
                    }
                    cmd.Parameters.Add("@cardPINCodeReceivingPhone", SqlDbType.NVarChar).Value = order.CardSMSPhone == string.Empty ? "37400000000" : order.CardSMSPhone;
                    cmd.Parameters.Add("@cardSupplementaryType", SqlDbType.SmallInt).Value = order.PlasticCard.SupplementaryType;
                    cmd.Parameters.Add("@adrressEngTranslated", SqlDbType.NVarChar).Value = order.AdrressEngTranslated;
                    cmd.Parameters.Add("@openDate", SqlDbType.DateTime).Value = order.PlasticCard.OpenDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }
                    cmd.Parameters.Add("@ProvidingFilialCode", SqlDbType.Int).Value = order.ProvidingFilialCode;

                     
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
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }
                }
            }

            return result;
        }
        internal static string GetSocSecurityNumber(ulong customerNumber)
        {
            string socSecurityNumber= "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT CASE WHEN c.type_of_client=6 THEN isnull(passport_number,'') + ', ' + isnull(passport_inf,'') + ', ' + isnull(Convert(varchar(10),passport_date,3),'')
                                        ELSE isnull(m.document_number,'') +', ' + isnull(m.document_given_by, '') + ', ' + isnull(Convert(varchar(10), m.document_given_date, 3), '') END AS passport
                                        FROM V_CustomerDesriptionDocs c LEFT JOIN V_LegalCustomerManagers m ON m.customer_number = c.customer_number WHERE c.customer_number = @customerNumber";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        socSecurityNumber = rd["passport"].ToString();
                    }
                }
            }

            return socSecurityNumber;
        }

        internal static bool CheckCustomerDefaultDocument(ulong customerNumber)
        {
            bool hasDefaultDocument = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT pass_inf,default_id_card,ID_card,customer_number 
                                        FROM
                                            (SELECT CUS.customer_number, isnull(L.document_number  ,CUS.passport_number )+' '+ convert(nvarchar,isnull(L.document_given_date ,CUS.passport_date),3) +' ' + isnull(L.document_given_by ,CUS.passport_inf) pass_inf,
                                                    isnull(L.social_number, CUS.social_number) social_number,D.ID_card,D.default_id_card
                                             FROM V_customerDesriptionDocs CUS
                                             LEFT JOIN V_LegalCustomerManagers L ON L.customer_number =CUS.customer_number
                                             LEFT JOIN (SELECT identityId,document_number ID_card, is_default AS default_id_card FROM Tbl_customer_documents_current WHERE document_type=11) D ON D.identityId=CUS.identityId
                                             ) A
                                            WHERE a.customer_number = @customerNumber and ((isnull(pass_inf,'')<>'' and isnull(default_id_card,0)=0) or  (isnull(ID_card,'')<>'' and isnull(default_id_card,0)<>0))";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        hasDefaultDocument = true;
                    }
                }
            }

            return hasDefaultDocument;

        }

        internal static PlasticCardOrder GetPlasticCardOrder(PlasticCardOrder order, SourceType source = SourceType.NotSpecified)
        {
            order.PlasticCard = new PlasticCard();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT HB.document_number document_number, HB.document_type document_type, HB.document_subtype document_subtype, HB.doc_ID, 
                                                  HB.operationFilialCode operationFilialCode, HB.quality quality, registration_date, operation_date, HB.description , CT.ApplicationsCardType ApplicationsCardType,
                                                  VA.Cardnumber, OD.currency, OD.card_PIN_code_receiving_type,HB.source_type,OD.related_office_number,OD.mother_name, isnull(OD.card_report_receiving_type,-1) as card_report_receiving_type , ISNULL(RT.[description],'') AS report_type_description,
                                                  OD.ProvidingFilialCode, Od.card_type , Od.service_Fee_Periodicity_Type, OD.cardSMSPhone, OD.cardPINCodeReceivingPhone, OD.serving_set_number,OD.card_supplementary_type,
                                                  OD.main_card_number,T.armenian card_technology, od.linked_card_limit, HB.confirmation_date, ISNULL(F.fill_description_unicode,'') AS fill_description,HB.order_group_id,
                                                  ISNULL(OD.cardSMSPhone, '') cardSMSPhone , PIN.description pin_description
                                                  FROM Tbl_HB_documents AS HB
                                                  INNER JOIN tbl_card_order_details OD on HB.doc_ID = OD.doc_ID
                                                  INNER JOIN dbo.tbl_type_of_card CT on OD.card_type = CT.id
                                                  INNER JOIN dbo.Tbl_type_of_Card_Technology T on T.abbreviation = CT.C_M	
                                                 INNER JOIN dbo.tbl_types_of_card_PIN_code_receiving PIN on OD.card_PIN_code_receiving_type = PIN.[type_id]                                                  
                                                 LEFT JOIN dbo.Tbl_visa_applications VA on VA.app_id = OD.app_id
                                                  LEFT JOIN dbo.tbl_types_of_card_report_receiving RT on OD.card_report_receiving_type = RT.[type_id]
                                                  LEFT JOIN (
					                                            SELECT 22000 + Fill AS Code_Fill, fill_description_unicode AS fill_description_unicode
					                                            FROM dbo.name_fill
					                                            ) F on F.Code_Fill = OD.ProvidingFilialCode
                                                  WHERE HB.customer_number=CASE WHEN @customer_number = 0 THEN HB.customer_number ELSE @customer_number  END and HB.doc_ID=@doc_id ", conn);

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["operationFilialCode"].ToString());
                    order.PlasticCard.Currency = dt.Rows[0]["currency"].ToString();
                    order.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                    order.CardTechnology = Utility.ConvertAnsiToUnicode(dt.Rows[0]["card_technology"].ToString());
                    order.CardSMSPhone = dt.Rows[0]["cardSMSPhone"].ToString();
                    order.CardPINCodeReceivingTypeDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["pin_description"].ToString());
                    order.CardPINCodeReceivingType = dt.Rows[0]["card_PIN_code_receiving_type"] != DBNull.Value? Convert.ToUInt16(dt.Rows[0]["card_PIN_code_receiving_type"].ToString()):0;
                    order.PlasticCard.SupplementaryType = (SupplementaryType)Convert.ToInt32(dt.Rows[0]["card_supplementary_type"].ToString());
                    order.PlasticCard.CardTypeDescription = dt.Rows[0]["ApplicationsCardType"].ToString();
                    order.PlasticCard.CardNumber = dt.Rows[0]["Cardnumber"].ToString();
                    order.Source = (SourceType)Convert.ToByte(dt.Rows[0]["source_type"].ToString());
                    order.PlasticCard.RelatedOfficeNumber = Convert.ToInt32(dt.Rows[0]["related_office_number"].ToString());
                    order.MotherName = (dt.Rows[0]["mother_name"].ToString());

                    if (Convert.ToInt32(dt.Rows[0]["card_report_receiving_type"]) != -1)
                        order.CardReportReceivingType = Convert.ToInt32(dt.Rows[0]["card_report_receiving_type"].ToString());
                    else
                        order.CardReportReceivingType = null;



                    order.CardReportReceivingTypeDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["report_type_description"].ToString());
                    order.PlasticCard.CardType = Convert.ToUInt32(dt.Rows[0]["card_type"].ToString());
                    order.ProvidingFilialCode = dt.Rows[0]["ProvidingFilialCode"] != DBNull.Value? Convert.ToUInt16(dt.Rows[0]["ProvidingFilialCode"].ToString()): (ushort)0;
                    if (dt.Rows[0]["service_Fee_Periodicity_Type"].ToString() == "")
                        order.ServiceFeePeriodicityType = 0;
                    else
                        order.ServiceFeePeriodicityType = (ServiceFeePeriodicityType)Convert.ToInt32(dt.Rows[0]["service_Fee_Periodicity_Type"].ToString());
                    order.CardSMSPhone = dt.Rows[0]["cardSMSPhone"].ToString();
                    order.CardPINCodeReceivingPhone = dt.Rows[0]["cardPINCodeReceivingPhone"].ToString();
                    order.ServingSetNumber = Convert.ToInt32(dt.Rows[0]["serving_set_number"].ToString());
                    order.PlasticCard.MainCardNumber = dt.Rows[0]["main_card_number"].ToString();
                    if (order.PlasticCard.SupplementaryType == SupplementaryType.Linked)
                    {
                        order.LinkedCardLimit = dt.Rows[0]["linked_card_limit"]!= DBNull.Value ? Convert.ToDouble(dt.Rows[0]["linked_card_limit"].ToString()) : 0;
                    }
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.ProvidingFilialCodeDescription = dt.Rows[0]["fill_description"].ToString();
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                }
            }
            return order;
        }

        internal static string GetCustomerLastMotherName(ulong customerNumber)
        {
            string lastMotherName="";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT TOP 1  MotherName
                                        FROM Tbl_VISA_applications
                                        WHERE customer_number = @customerNumber
                                        ORDER BY RegDate DESC";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        lastMotherName = rd["MotherName"].ToString();
                    }
                }
            }

            return lastMotherName;
        }
        internal static int GetPlasticCardOrderCount(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT doc_id
                                                  FROM Tbl_HB_documents
                                                  WHERE document_type = 210 and quality = 3 and customer_number = @customer_number", conn);

    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                dt.Load(cmd.ExecuteReader());
                return dt.Rows.Count;
            }
        }

        internal static ulong GetCustomerPensionContractID(ulong customerNumber)
        {
            ulong contractID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT contract_id FROM Tbl_pension_application WHERE quality=10 AND closing_date IS NULL AND deleted = 0 AND quality=10 AND service_type IN (1,2) and customer_number = @customerNumber";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        contractID = ulong.Parse(rd["contract_id"].ToString());
                    }
                }
            }

            return contractID;
        }

        internal static bool CheckAdditionalCardAvailability(uint cardType, string mainCardNumber, int subType)
        {
            bool available = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT AC.* 
                                        FROM Tbl_additional_card_conditions_checking AC 
                                        INNER JOIN Tbl_VISA_applications VA ON VA.cardType=AC.main_card_type
                                        WHERE sub_type=@subType AND VA.Cardnumber = @mainCardNumber  AND AC.additional_card_type = @cardType";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar).Value = mainCardNumber;
                    cmd.Parameters.Add("@cardType", SqlDbType.SmallInt).Value = cardType;
                    cmd.Parameters.Add("@subType", SqlDbType.SmallInt).Value = subType;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        available = true;
                    }
                }
            }

            return available;

        }

        internal static PlasticCard GetMainCard(string mainCardNumber)
        {
            PlasticCard plasticCard = new PlasticCard();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Top 1 cardType, BillingCurrency, filial, RelatedOfficeNumber  FROM Tbl_VISA_applications WHERE Cardnumber = @mainCardNumber AND  CardStatus = 'NORM'";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar).Value = mainCardNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        plasticCard.CardType = uint.Parse(rd["cardType"].ToString());
                        plasticCard.Currency = rd["BillingCurrency"].ToString();
                        plasticCard.RelatedOfficeNumber = int.Parse(rd["RelatedOfficeNumber"].ToString());
                        plasticCard.FilialCode = int.Parse(rd["filial"].ToString()) + 22000;
                    }
                }
            }

            return plasticCard;

        }

        internal static List<int> GetRelatedOfficeNumbersForAttachedCards()
        {
            List<int> relOfficeNumbers = new List<int>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT Office_ID
                                        FROM Tbl_Contract_salary_by_cards
                                        WHERE Office_name like '%Éñ³óáõóÇã%' OR Office_name like '%Èñ³óáõóÇã%'";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    while (rd.Read())
                    {
                        relOfficeNumbers.Add(int.Parse(rd["Office_ID"].ToString()));
                    }
                }
            }

            return relOfficeNumbers;
        }

        internal static int GetRelatedOfficeContractStatus(int relatedOfficeID)
        {
            int contractID = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT contractStatus FROM tbl_contract_salary_by_cards WHERE  office_id = @relatedOfficeID";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@relatedOfficeID", SqlDbType.Int).Value = relatedOfficeID;

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        contractID = rd["contractStatus"] == null ? -1: int.Parse(rd["contractStatus"].ToString());
                    }
                }
            }
            return contractID;
        }
        internal static int GetNotGivenCardsCount(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT customer_number
                                                  FROM Tbl_VISA_applications 
                                                  WHERE givendate is null and customer_number = @customer_number", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                dt.Load(cmd.ExecuteReader());
                return dt.Rows.Count;
            }
        }


        internal static bool CheckPlatsicCardRelatedOffice(PlasticCardOrder order)
        {
            bool suitable = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Select *  from Tbl_cards_rates where office_id= @relOfficeID  and cardid=@cardSystem and cardtype=@cardType and currency=@cardCurrency and isnull(Quality,1)= 1";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@cardSystem", SqlDbType.Int).Value = order.PlasticCard.CardSystem;
                    cmd.Parameters.Add("@cardType", SqlDbType.BigInt).Value = order.PlasticCard.CardType;
                    cmd.Parameters.Add("@cardCurrency", SqlDbType.NVarChar, 3).Value = order.PlasticCard.Currency;
                    cmd.Parameters.Add("@relOfficeID", SqlDbType.Int).Value = order.PlasticCard.RelatedOfficeNumber;

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        suitable = true;
                    }
                }
            }

            return suitable;
        }

        internal static string GetCustomerAddressEng(ulong customerNumber)
        {
            string addressEng = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_GetCustomerAddressEng";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        addressEng = rd["adressENG"].ToString();
                    }
                }
            }
         return addressEng;
        }

        internal static int PlasticCardQuantityRestriction(PlasticCardOrder order)
        {
            int quantity = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS ( SELECT Office_ID, Office_Name, Contract_beginning, ReasonId 
                                                    FROM Tbl_Contract_salary_by_cards 
                                                    WHERE Office_ID = @RelatedOfficeNumber AND ReasonId in (1,2) and Contract_beginning > '05/March/2015' and Contract_end is null)
	                                    BEGIN
	                                    SELECT COUNT(*) AS quantity 
	                                    FROM Tbl_VISA_applications 
	                                    WHERE  ( CardStatus like 'NOR%' or CardStatus='NEW' ) and RelatedOfficeNumber=@RelatedOfficeNumber and customer_number=@customerNumber 
	                                              and cardType= @cardType
		                                         and BillingCurrency ='AMD' and BillingCurrency = @cardCurrency
	                                    END";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@RelatedOfficeNumber", SqlDbType.Int).Value = order.PlasticCard.RelatedOfficeNumber;
                    cmd.Parameters.Add("@cardType", SqlDbType.BigInt).Value = order.PlasticCard.CardType;
                    cmd.Parameters.Add("@cardCurrency", SqlDbType.NVarChar, 3).Value = order.PlasticCard.Currency;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = order.CustomerNumber;                
                    

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        quantity = int.Parse(rd["quantity"].ToString());
                    }
                }
            }

            return quantity;
        }

        internal static GPMMemberStatus CheckCustomerGPMStatus(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select TOP 1 date_of_member from Tbl_Customer_GPM WHERE customer_number= @customerNumber ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    

                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        if (rd["date_of_member"] == DBNull.Value)
                        {
                            return GPMMemberStatus.NoMemberDate;
                        }
                        else return GPMMemberStatus.IsMember;
                    }
                    else return GPMMemberStatus.NotMember;
                }
            }
        } 

        internal static bool CheckAdditionalCardConditions(string mainCardNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT AC.* 
                                        FROM Tbl_additional_card_conditions_checking AC 
                                        INNER JOIN Tbl_VISA_applications VA ON VA.cardType=AC.main_card_type
                                        WHERE sub_type=1 AND VA.Cardnumber= @mainCardNumber";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar).Value = mainCardNumber;
                    SqlDataReader rd;

                    rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        internal static ActionResult GeneratePlasticCardOrderMessages(PlasticCardOrder order)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_generate_plastic_card_order_messages";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@template_ID", SqlDbType.Int).Value = 89;
                    cmd.Parameters.Add("@customer_number", SqlDbType.NVarChar).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@card_Type", SqlDbType.NVarChar, 20).Value = order.PlasticCard.CardTypeDescription.ToString();
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = order.PlasticCard.Currency.ToString();
                    cmd.Parameters.Add("@providing_Filial_Code", SqlDbType.Int).Value = order.ProvidingFilialCode;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }

        internal static int GetLinkedCardsCount(string mainCardNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT SUM(a) as summary FROM(
                                                SELECT COUNT(*)  AS a
                                                FROM tbl_visa_applications A
                                                INNER JOIN tbl_supplementarycards S
                                                ON A.app_id = S.app_id
                                                WHERE maincardnumber = @mainCardNumber AND cardstatus = 'norm' AND (A.customer_number <> S.customer_number or S.customer_number is null)
                                                UNION
                                                SELECT COUNT(*) AS a
                                                FROM dbo.tbl_hb_documents D
                                                INNER JOIN dbo.tbl_card_order_details O
                                                ON D.doc_id = O.doc_id
                                                WHERE  O.main_card_number = @mainCardNumber and quality IN (3,50) AND document_type = 212
                                                ) al
                                                ", conn);

                cmd.Parameters.Add("@mainCardNumber", SqlDbType.NVarChar, 50).Value = mainCardNumber;
                dt.Load(cmd.ExecuteReader());
                return Convert.ToInt32(dt.Rows[0]["summary"].ToString());
            }
        }
    }
}

   