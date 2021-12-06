using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public static class InfoDB
    {
        public static DataTable GetDepositTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code, description, description_Engl from [Tbl_type_of_deposits;]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }
        public static DataTable GetCurrencies()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select currency From [tbl_currency;] where Quality=1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }

        public static DataTable GetLoanTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select code,description,Description_Engl  from [tbl_type_of_loans;]
                                                union all( select code,description,Description_Engl from Tbl_type_of_product_limits)", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        internal static DataTable GetAccountTypes()
        {

            DataTable accountTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT code, Description,descriptionEng from  Tbl_type_of_products ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        accountTypes.Load(dr);
                    }
                }


            }
            return accountTypes;
        }


        public static DataTable GetLoanQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select number as code,quality as description,Description_Engl from [Tbl_loan_list_quality;]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetProblemLoanTaxQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select number as code,quality as description from [Tbl_loan_list_quality;] where number in (0,10,11,12,40,41)", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetProblemLoanTaxCourtDecisionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select Id as code, TaxCourtDecisionDescription as description from Tbl_type_of_problem_loan_tax_court_decision", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetPeriodicTransferTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID_Transfer ,Description,Description_Engl  from Tbl_operations_by_period_Types", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetPeriodicTransfersDurationTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand("SELECT [id],[description],[description_eng] FROM [dbo].[Tbl_Operations_by_period_duration_types]", conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }


        public static DataTable GetPeriodicTransfersChargeTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [id],[description],[description_eng]  FROM [dbo].[Tbl_Operations_by_period_charges_types]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetOrderSubTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [document_type],[document_sub_type],[description_arm],[description_eng],automat_confirm_FRONT_OFFICE_order as autoconfirm FROM [dbo].[Tbl_sub_types_of_HB_products]",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetOrderTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [product_type] as Id,[dbo].[fnc_convertAnsiToUnicode]( [product_description]) as Description ,automat_confirm_FRONT_OFFICE_order as autoconfirm, is_hb_product, hb_product_type, product_description_eng as Description_eng  FROM [dbo].[Tbl_types_of_HB_products] order by Description asc", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetOrderQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [quality],[description_arm],[description_eng] FROM [dbo].[Tbl_types_of_HB_quality]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetTerms()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                //Հարցման մեջ կատարվում է @var1 արտահայտության փոխարինում {0} -ով (@var2 - {1}...):
                using (SqlCommand cmd = new SqlCommand(@"	WITH Terms (TERM_ID,Lang_ID,Description,Step)
		                                                                                    AS
		                                                                                    (
			                                                                                    SELECT TERM_ID,Lang_ID,Replace(Description,'@Var1','{0}') as Description , 0 as Step
			                                                                                    FROM Tbl_HB_terms_definitions 
			                                                                                    Where Description like '%@Var_%'
			                                                                                    Union All
		                                                                                        SELECT TERM_ID,Lang_ID,Replace(Description,'@Var' + CAST(step+2 as varchar(5)),'{'+cast(Step +1 as varchar(5))+'}') as Description,step +1
			                                                                                    FROM Terms 
			                                                                                    Where Description like '%@Var_%'
		                                                                                    )
		                                                                                    select t.Term_Id as Id,tArm.Description,tEng.Description as Description_Eng  From 
		                                                                                    (Select distinct Term_Id from Tbl_HB_terms_definitions) t
		                                                                                    Left Join 
		                                                                                    (Select TERM_ID,Lang_ID,Description From Terms Where Description not like '%@var%' 
		                                                                                    Union All 
		                                                                                    Select TERM_ID,Lang_ID,Description from Tbl_HB_terms_definitions where Description not Like '%@var%') tArm
		                                                                                    on t.Term_Id=tArm.Term_Id
		                                                                                    and tArm.Lang_ID=0
		                                                                                    Left Join  
		                                                                                    (Select TERM_ID,Lang_ID,Description From Terms Where Description not like '%@var%' 
		                                                                                    Union All 
		                                                                                    Select TERM_ID,Lang_ID,Description from Tbl_HB_terms_definitions where Description not Like '%@var%') tEng
		                                                                                    on t.Term_Id=tEng.Term_Id
		                                                                                    and tEng.Lang_ID=1
		                                                                                    Order by  t.Term_Id", conn)
                          )
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetCommunalDetailsTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [id],[description],[description_eng] FROM [dbo].[Tbl_Check]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetCreditLineTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select code as id,description,Description_Engl  from Tbl_type_of_credit_lines ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetEmbassyList(List<ushort> referenceTypes)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_reference_embassy", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (referenceTypes.Count > 1 && referenceTypes.Contains(9))
                    {
                        return dt;
                    }
                    if (referenceTypes.Count == 1 && referenceTypes.Contains(9))
                    {
                        return dt.AsEnumerable()
                            .Where(r => r.Field<int>("ID") == 30).CopyToDataTable();
                    }
                    else
                    {
                        return dt.AsEnumerable()
                            .Where(r => r.Field<int>("ID") != 30).CopyToDataTable();
                    }
                }

            }
        }
        public static DataTable GetReferenceLanguages()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT type_id As id, description_arm,description_eng FROM tbl_type_of_languages", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }

        public static DataTable GetReferenceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT type_id As id,description_arm,description_eng FROM tbl_types_of_reference ORDER BY type_id", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }

        /// <summary>
        /// Գործող մասնաճյուղերի ցակը հասցեներով
        /// </summary>
        /// <returns></returns>
        public static DataTable GetFilialAddressList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT code As id,EnglDescription+', '+EnglishAddress as description_eng,ArmDescription+',  '+RIGHT(Adress,len(adress)-7) As description_arm FROM [Tbl_banks;] WHERE code>=22000 and code<=23000
                                                  and ArmDescription is not null and closing_date is null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }
        public static DataTable GetStatementFrequency()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,description, description_eng FROM [Tbl_type_of_Statement_Frequency]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }
        public static DataTable GetCashOrderTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT document_sub_type As id,description FROM Tbl_sub_types_of_HB_products where document_type=23", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    return dt;
                }

            }
        }
        public static DataTable GetTransferTypes(int isActive)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM Tbl_TransferSystems  WHERE  Instant_money_transfer=1 ";
                if (isActive == 1)
                {
                    sqltext += "and isactive=1 ";
                }

                sqltext += "Order By code ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetAllTransferTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM Tbl_TransferSystems ";

                sqltext += "Order By code ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetTransferCallQuality()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select Id,Description from tbl_type_of_transfer_call_quality";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetBanks()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select description, Description_Engl, code from [Tbl_banks;] order by description asc";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetOperationTypes()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT code,description,descr_extract_english FROM Tbl_type_of_transaction", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }
        public static DataTable GetJointTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select Id,description, description_Eng from Tbl_type_of_co_accounts";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetTransferSystemCurrency()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("Select transfersystem,currency FROM Tbl_TransferSystems_Details", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }



            }
            return dt;
        }
        /// <summary>
        /// Ավանդի արժույթը կախված ավանդի տեսակից
        /// </summary>
        /// <returns></returns>
        public static DataTable GetDepositTypeCurrency()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Tbl_deposit_product_history.currency, [Tbl_type_of_deposits;].code FROM [Tbl_type_of_deposits;]
                                               INNER JOIN Tbl_deposit_product_history ON [Tbl_type_of_deposits;].code = Tbl_deposit_product_history.product_code 
                                               WHERE [Tbl_type_of_deposits;].closing_date Is Null  and Date_Of_Beginning = (Select MAX(Date_Of_Beginning) From Tbl_Deposit_Product_History)  GROUP BY [Tbl_type_of_deposits;].code,
                                               [Tbl_type_of_deposits;].description, Tbl_deposit_product_history.currency ", conn))
                {
                    dt.Load(cmd.ExecuteReader());
                }

            }
            return dt;
        }


        public static DataTable GetActiveDepositTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code, description, description_Engl from [Tbl_type_of_deposits;] where closing_date is null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetActiveDepositTypesForNewDepositOrder(short allowableCustomerType, short allowableForThirdhPerson, short allowableForCooperative)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT code, description, description_Engl,allowable_customer_type,allowable_for_thirdh_person 
                                                  FROM [Tbl_type_of_deposits;] WHERE closing_date is null AND (allowable_customer_type=@allowableCustomerType OR allowable_customer_type=0)
                                                  AND allowable_for_thirdh_person=@allowableForThirdhPerson AND allowable_for_cooperative=@allowableForCooperative", conn);
                cmd.Parameters.Add("@allowableCustomerType", SqlDbType.SmallInt).Value = allowableCustomerType;
                cmd.Parameters.Add("@allowableForThirdhPerson", SqlDbType.Bit).Value = allowableForThirdhPerson;
                cmd.Parameters.Add("@allowableForCooperative", SqlDbType.Bit).Value = allowableForCooperative;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }


        public static DataTable GetCardSystemTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select ID as code,CardSystemType as description from Tbl_type_of_CardSystem ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetCardReportReceivingTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select type_id, description, description_eng from tbl_types_of_card_report_receiving ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetCardPINCodeReceivingTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select type_id, description from tbl_types_of_card_PIN_code_receiving ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetCardTechnologyTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_type_of_card_technology", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }


        public static DataTable GetCardTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select    crd.ID,CardSystemID,
                                                            s.CardSystemType+' '+CardType as CardType,
                                                            ApplicationsCardType from tbl_type_of_card crd
                                                            inner join Tbl_type_of_CardSystem s
                                                            on s.ID=crd.CardSystemID
                                                            where quality=1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }


        public static DataTable GetAllCardTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @" SELECT crd.ID, quality, CardSystemID, s.CardSystemType + ' ' + CardType + ' ' + C_M as CardType, ApplicationsCardType  
                                                                                     FROM tbl_type_of_card crd
                                                                                                    INNER JOIN Tbl_type_of_CardSystem s ON s.ID = crd.CardSystemID ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        /// <summary>
        /// ՏՀՏ կոդեր
        /// </summary>
        /// <returns></returns>
        public static DataTable GetLTACodes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select code, description, description_engl from Tbl_region_tax_codes order by code";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        /// <summary>
        /// Ոստիկանության կոդեր
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPoliceCodes(string accountNumber = "")
        {
            string ackind = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TaxServiceConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT ACKIND FROM [tbl_budget_accounts] WHERE CODE= @account_number ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 50).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            ackind = dr[0].ToString();

                    }
                }

            }

            DataTable dt = new DataTable();
            if (!String.IsNullOrEmpty(ackind))
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    conn.Open();
                    string sqltext = "SELECT * FROM Tbl_budget_militia_transfers WHERE ACKIND =@ackind ORDER BY code";

                    using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@ackind", SqlDbType.NVarChar, 3).Value = ackind;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }
                    }
                }
            }
            return dt;
        }

        internal static DataTable GetReasonForCardTransactionAction(byte actionReasonId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT description_arm, description_eng FROM tbl_type_of_reasons_for_card_transaction_action WHERE id = @reasonid";
                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                cmd.Parameters.Add("@reasonid", SqlDbType.Int).Value = actionReasonId;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Անձի կարգավիճակ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSyntheticStatuses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM Tbl_sint_acc_details WHERE value_for8 not in (12,22) ORDER BY value_for8";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable SearchRelatedOfficeTypes(string officeId, string officeName)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sqlStr = "select office_id,office_name from Tbl_Contract_salary_by_cards where Contract_end is null ";

                if (officeId != "")
                {
                    sqlStr = sqlStr + " and office_id like ''+@officeId+'%'";
                }

                if (officeName != "")
                {
                    officeName = Utility.ConvertUnicodeToAnsi(officeName);
                    sqlStr = sqlStr + " and dbo.armLower(office_name) like '%'+ RTRIM(LTRIM(dbo.armlower(@officeName))) + '%'";
                }

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (officeId != "")
                    {
                        cmd.Parameters.Add("@officeId", SqlDbType.NVarChar).Value = officeId;
                    }

                    if (officeName != "")
                    {
                        cmd.Parameters.Add("@officeName", SqlDbType.NVarChar).Value = officeName;
                    }

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }



            }
            return dt;
        }


        public static DataTable GetPeriodicsSubTypes(Languages language)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT amount_type, CASE WHEN @language = 1 THEN [Description] ELSE [DescriptionEng] END AS Description FROM Tbl_Op_By_Per_Amount_Types";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.Parameters.Add("@language", SqlDbType.Int).Value = (int)language;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }
        public static DataTable GetCommunalBranchList(CommunalTypes communalType, Languages language)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_getCommunalBranchList";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@transfer_type", SqlDbType.SmallInt).Value = communalType;
                    if (language != Languages.hy)
                    {
                        cmd.Parameters.Add("@lang_id", SqlDbType.TinyInt).Value = (short)language;
                    }
                    cmd.Parameters.Add("@unicode", SqlDbType.TinyInt).Value = 1;
                    dt.Load(cmd.ExecuteReader());

                }
            }
            return dt;
        }
        /// <summary>
        /// Պարբերական փոխանցման կոմունալի տեսակները
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPeriodicUtilityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select ID_Transfer,Description,Description_Engl from [Tbl_operations_by_period_Types] WHERE Communal_type <> 0";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }

        public static DataTable GetFactoringTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code, description, description_Engl from [Tbl_types_of_factoring_type]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetFactoringRegresTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code, description, description_Engl from [Tbl_types_of_factoring_regres_type]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }
        public static DataTable GetFilialList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT code As id,Convert(NVARCHAR,code)+','+EnglDescription as description_eng,Convert(NVARCHAR,code)+','+ArmDescription As description_arm FROM [Tbl_banks;] WHERE code>=22000 and code<=23000 and ArmDescription is not null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetCardClosingReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,description from Tbl_Type_Of_CardClosingReasons where is_old=0 order by id_fororderby", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        /// <summary>
        /// Պարբերական փոխանցման պարբերականություն
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPeriodicityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select * from Tbl_operations_by_period_periodicity_types";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        /// <summary>
        /// Հաշվի քաղվածքի ստացման տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetStatementDeliveryTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select * from [Tbl_type_of_extract_sending]";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetCountries()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select CountryCodeN,CountryName, CountryCodeA3 from Tbl_Countries where is_used=1   order by Case When CountryCodeN = 51 THEN '         ' Else CountryName End ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }


        public static string GetCountyRiskQuality(string countryCode)
        {
            string riskQuality = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN Risk_quality =2 THEN CASE  WHEN isnull(Add_inf,'') <> '' THEN  dbo.fnc_convertAnsiToUnicode(Add_inf)  ELSE N'Բարձր ռիսկային երկիր'  END ELSE '' END risk_quality   FROM Tbl_Countries  where CountryCodeN=@country", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 10).Value = countryCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            riskQuality = dr["risk_quality"].ToString();
                        }
                    }
                }
            }

            return riskQuality;
        }

        public static DataTable GetInternationalPaymentCurrencies()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select T.currency from  tbl_prices_for_transfers T join  [tbl_currency;] C on T.currency=C.currency  where T.currency<>'AMD' and Quality=1 Group by T.currency , int_code  order by int_code  ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }


        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountAdditionsTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT AdditionId,AdditionDescription FROM Tbl_type_of_all_acc_additions";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }
        public static DataTable GetOperationsList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT number,description from Tbl_operations_list", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }
        public static DataTable GetBankOperationFeeTypes(int type)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqlString = "SELECT code,description+CASE WHEN code<>0 THEN ' / '+CASE WHEN by_cash =1 THEN 'Ï³ÝËÇÏ' ELSE '³ÝÏ³ÝËÇÏ' END ELSE '' END AS description FROM Tbl_type_of_bank_operation_fee";
                string whereCondition = "";
                if (type == 1)
                {
                    whereCondition = " WHERE manual = 1";
                }
                if (type == 2)
                {
                    whereCondition = " WHERE code in(0,8,9)";
                }
                if (type == 4)
                {
                    whereCondition = " WHERE code in(5,20,21)";
                }
                if (type == 5)
                {
                    whereCondition = " WHERE manual = 1 and code in(0,1)";
                }
                if (type == 6)
                {
                    whereCondition = " WHERE code in(0,28,29)";
                }


                using (SqlCommand cmd = new SqlCommand(sqlString + whereCondition, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static string GetInfoFromSwiftCode(string swiftCode, short type)
        {


            string bankName = "";
            string countryCode = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT bank_name + ',' + city bank_name ,CountryCodeN FROM Tbl_world_banks where swift_code=@swiftCode", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@swiftCode", SqlDbType.NVarChar, 11).Value = swiftCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {

                            bankName = dr["bank_name"].ToString();
                            countryCode = dr["CountryCodeN"].ToString();
                        }
                    }

                }
            }

            if (type == 1)
                return bankName;
            else
                return countryCode;


        }

        public static DataTable GetOrderRejectTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Reject_ID, Reject_description , Reject_description_eng FROM Tbl_types_of_HB_rejects ORDER BY for_orderby", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        /// <summary>
        /// Հաշվի բացման տեսակներ
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAccountStatuses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select * from Tbl_type_of_account_type where ID in (0,1)";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        /// <summary>
        /// Քարտի աշխ. ծրագրի անվանում
        /// </summary>
        /// <returns></returns>
        public static string GetCardRelatedOfficeName(ushort officeId)
        {
            string officeName = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select Office_Name from Tbl_Contract_salary_by_cards  where Office_ID=@officeId";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@officeId", SqlDbType.Int).Value = officeId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            officeName = dr["Office_Name"].ToString();
                        }
                    }

                }


            }
            return officeName;
        }

        public static DataTable GetTransitAccountTypes(bool forLoanMature)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from [tbl_type_of_transit_accounts] where for_loan_mature=@forLoanMature", conn))
                {
                    cmd.Parameters.Add("@forLoanMature", SqlDbType.Bit).Value = forLoanMature;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        public static DataTable GetFreezeReasonTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Description, manual, card_transaction_reason_type FROM Tbl_type_of_freeze_reason ORDER BY Id", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }



        public static DataTable BankOperationFeeTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT code,description+CASE WHEN code<>0 THEN ' / '+CASE WHEN by_cash =1 THEN 'Ï³ÝËÇÏ' ELSE '³ÝÏ³ÝËÇÏ' END ELSE '' END AS description FROM Tbl_type_of_bank_operation_fee", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetCurrentAccountCurrencies()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select currency from [Tbl_currency;] where Quality<>0 and dbo.[fn_check_available_currency_for_account](currency,10,24)=1 order by int_code", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }

        public static DataTable GetServiceProvidedTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Idx_Price as id, Description FROM Tbl_Prices WHERE Add_inf='71' ORDER BY Idx_Price", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;

        }


        public static DataTable GetDisputeResolutions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id_dispute,dispute_description FROM  tbl_dispute_agreements ORDER BY id_dispute", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetCommunicationTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, description FROM Tbl_type_of_Communications", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetOrderRemovingReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select reason_id, reason_description from Tbl_operation_deleting_reasons", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        public static DataTable GetFondDescriptions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from [Tbl_fonds;]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetLoanProgramDescriptions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_type_of_loan_programs", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        public static DataTable GetLoanActionDescriptions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_type_of_loan_actions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetClaimQualities()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_problem_loan_claim_quality", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetClaimPurposes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_problem_loan_claim_purpose", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetClaimEventTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select claim_event,claim_event_description from Tbl_problem_loan_event_types ORDER BY priority", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetClaimEventPurposes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_problem_loan_event_purpose", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetCourtTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select number,description from Tbl_problem_loan_court_type", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetTaxTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select id,tax_descr from Tbl_type_of_tax", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetAssigneeOperationTypes(int groupId, int typeOfCustomer)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string whereCond;
                if (typeOfCustomer == 6)
                    whereCond = " AND forNaturalPerson=1 ";
                else if (typeOfCustomer == 2)
                    whereCond = " AND forPrivateEntrepreneur=1 ";
                else
                    whereCond = " AND forLegalEntity=1 ";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id, description, groupId, forNaturalPerson, forPrivateEntrepreneur, forLegalEntity, bankingOperationGroup, 
                                                CASE WHEN B.operationTypeId is null THEN 0 ELSE 1 END CanChangeAllAccounts, IsNull(description_eng, ' ') AS description_eng
                                                FROM Tbl_type_of_assign_operation A 
												OUTER APPLY 
												(Select top 1 operationTypeId from Tbl_assign_operation_account_types where operationTypeId =A.id) B 
                                                WHERE closingDate IS NULL AND GroupId = @GroupId " + whereCond + " ORDER BY id", conn))
                {
                    cmd.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetAssigneeOperationGroupTypes(int typeOfCustomer)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string whereCond;
                if (typeOfCustomer == 6)
                    whereCond = " AND forNaturalPerson=1 ";
                else if (typeOfCustomer == 2)
                    whereCond = " AND forPrivateEntrepreneur=1 ";
                else
                    whereCond = " AND forLegalEntity=1 ";

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id, description, forNaturalPerson, forPrivateEntrepreneur, forLegalEntity, description_eng 
                                                FROM Tbl_Type_of_assign_operations_groups WHERE closingDate IS NULL " + whereCond + " ORDER BY id", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetCredentialTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id, description FROM Tbl_type_of_assigns ORDER BY id", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetCredentialClosingReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id, description FROM Tbl_type_of_assign_closing_reason ORDER BY id", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetActionPermissionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT ActionID ,ParentID,Description,DescriptionEN,  OrderType, OrderSubType from V_acs_actions_list ORDER BY ActionID", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetAccountClosingReasonTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Idx_Closing, Decsription  FROM Tbl_Acc_Closing_Descriptions WHERE for_HB = 0", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetDepositClosingReasonTypes(bool showinLIst = true)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_type_of_deposit_closing_reason" + (showinLIst ? " where show_in_list=1" : ""), conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        public static DataTable GetTokenTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_type_of_tokens", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetMonths()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT number, name FROM [tbl_mounths;]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }
        internal static DataTable GetServicePaymentNoteReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = @"SELECT Id,Note_description FROM Tbl_type_of_service_notes WHERE is_active=1";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        internal static DataTable GetTransferAmountPurposes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = @"SELECT id,description FROM Tbl_type_of_international_transfer_purpose WHERE closing_date is null";
                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetPensionAppliactionQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_type_of_pension_application_quality", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }


        public static DataTable GetPensionAppliactionServiceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_type_of_pension_application_service", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetPensionAppliactionClosingTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from Tbl_type_of_pension_application_closing", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }




        public static DataTable GetCurNominals(string currency)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Banknot,Nominal  FROM Tbl_cur_nominal WHERE Curensy=@currency AND Desk='k' order by Banknot desc", conn))
                {
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        internal static DataTable GetCardTariffContractReasonDescriptions()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ReasonId,Reason FROM Tbl_Type_of_salcontractsreason ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;

        }


        internal static DataTable GetPosLocationTypeDescriptions()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,description FROM Tbl_type_of_locations ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;

        }

        internal static DataTable GetPosTerminalTypeDescriptions()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,device_description FROM Tbl_type_of_devices ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;

        }
        internal static DataTable GetSMSMessagingStatusTypes()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID,description FROM tbl_types_of_message_status ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;

        }
        public static DataTable GetProvisionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Type_Id,Description FROM Tbl_type_of_all_provision ORDER BY dbo.Fnc_ArmHex(Description)", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetInsuranceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT insurance_type,description,description_eng FROM Tbl_Insurance_Type ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetInsuranceCompanies()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT company_ID,company_name FROM Tbl_type_of_insurance_company group by company_ID,company_name", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetInsuranceCompaniesByInsuranceType(ushort insuranceType)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT company_ID, company_name  FROM Tbl_type_of_insurance_company
                                                  WHERE insurance_type = @insuranceType and company_status = 1", conn))
                {
                    cmd.Parameters.Add("@insuranceType", SqlDbType.SmallInt).Value = insuranceType;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetInsuranceTypesByProductType(bool isLoanProduct, bool isSeparatelyProduct)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT insurance_type,description,description_eng FROM Tbl_Insurance_Type 
                                                WHERE is_loan_product=@isLoanProduct AND is_separately_product=@isSeparatelyProduct", conn))
                {
                    cmd.Parameters.Add("@isLoanProduct", SqlDbType.Bit).Value = isLoanProduct;
                    cmd.Parameters.Add("@isSeparatelyProduct", SqlDbType.Bit).Value = isSeparatelyProduct;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetCardDataChangeFieldTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT field_type,field_type_description,field_column_type FROM Tbl_card_data_change_field_types", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetSubTypesOfTokens()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_SubType_Of_Tokens", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;

        }


        internal static DataTable GetPrintReportTypes()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT number,  (cast( number as varchar(10)))+  '. ' + [name ]    as description from [dbo].[Tbl_print_reports;] where report_source=1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetHBApplicationReportType()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * from  tbl_HB_Application_Report_Type", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetTransferRejectReasonTypes()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM tbl_type_of_transfer_reject_reasons", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetTransferRequestStepTypes()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM DocFlow.dbo.Tbl_type_of_request_step_id where step_id<=6 or step_id=57 or step_id=117 or step_id=135 or step_id=136", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetTransferRequestStatusTypes()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@" SELECT id,description FROM DocFlow.dbo.Tbl_type_of_request_status where id >=1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetBusinesDepositOptions()
        {

            DataTable reportTypes = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM Tbl_Type_Of_Busines_Deposit_Options", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        reportTypes.Load(dr);
                    }
                }


            }
            return reportTypes;
        }

        internal static DataTable GetTransferSessions(DateTime dateStart, DateTime dateEnd, short transferGroup)
        {

            DataTable sessions = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@" SELECT session FROM Tbl_Bank_Mail_IN WHERE registration_date>=@dateStart And registration_date <=@dateEnd And transfer_group =@transferGroup and not(Session is null) GROUP BY session ORDER BY session", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@dateStart", SqlDbType.SmallDateTime).Value = dateStart;
                    cmd.Parameters.Add("@dateEnd", SqlDbType.SmallDateTime).Value = dateEnd;
                    cmd.Parameters.Add("@transferGroup", SqlDbType.SmallInt).Value = transferGroup;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        sessions.Load(dr);
                    }
                }

            }
            return sessions;
        }


        public static DataTable GetTypeOfCardPaymentsToArca()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select * from Tbl_type_of_card_payments_to_arca", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }


        internal static DataTable GetRegions(int country)
        {

            DataTable regions = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT region,name,name_english FROM tbl_all_regions WHERE country=CASE WHEN @country=51 THEN 1 WHEN @country=999 THEN 2 ELSE 3 END", conn))
                {
                    cmd.Parameters.Add("@country", SqlDbType.Int).Value = country;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        regions.Load(dr);
                    }
                }

            }
            return regions;
        }

        internal static DataTable GetArmenianPlaces(int region)
        {

            DataTable places = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT number,description,description_english FROM Tbl_armenian_places WHERE region=@region and number<>0 ORDER BY DescriptionHex", conn))
                {
                    cmd.Parameters.Add("@region", SqlDbType.Int).Value = region;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        places.Load(dr);
                    }
                }

            }
            return places;
        }

        public static DataTable GetTypeOfLoanRepaymentSource()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select * from Tbl_type_of_loan_repayment_source", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static CardTariffAdditionalInformation GetCardTariffAdditionalInformation(int officeID, int cardType)
        {
            CardTariffAdditionalInformation cardTariffAdditionalInformation = new CardTariffAdditionalInformation();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                ISNULL(service_fee_org,0) service_fee_org,
                                ISNULL(period_org,0) period_org,
                                ISNULL(service_fee_total_org,0) service_fee_total_org,
                                ISNULL(deductionstart_org,0) deductionstart_org,
                                ISNULL(min_rest_amd,0) min_rest_amd,
                                ISNULL(replace_fee_org,0) replace_fee_org,
                                ISNULL(replace_fee,0) replace_fee,
                                ISNULL(Renew_fee_org,0) Renew_fee_org				
                       FROM Tbl_cards_rates re INNER JOIN tbl_type_of_card C ON re.cardtype = C.ID
                       WHERE re.Currency = 'AMD' AND re.Office_ID = @Office_ID AND re.CardType = @CardType ", conn))
                {
                    cmd.Parameters.Add("@Office_ID", SqlDbType.Int).Value = officeID;
                    cmd.Parameters.Add("@CardType", SqlDbType.Int).Value = cardType;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            cardTariffAdditionalInformation.ServiceFeeFromOrganization = Convert.ToDouble(dr["service_fee_org"].ToString());
                            cardTariffAdditionalInformation.PeriodFromOrganization = Convert.ToInt32(dr["period_org"].ToString());
                            cardTariffAdditionalInformation.ServiceFeeTotalFromOrganization = Convert.ToDouble(dr["service_fee_total_org"].ToString());
                            cardTariffAdditionalInformation.DeductionStartFromOrganization = Convert.ToInt32(dr["deductionstart_org"].ToString());
                            cardTariffAdditionalInformation.MinRest = Convert.ToDouble(dr["min_rest_amd"].ToString());
                            cardTariffAdditionalInformation.ReplaceFeeFromOrganization = Convert.ToDouble(dr["replace_fee_org"].ToString());
                            cardTariffAdditionalInformation.ReplaceFee = Convert.ToDouble(dr["replace_fee"].ToString());
                            cardTariffAdditionalInformation.ReNewFeeFromOrganization = Convert.ToDouble(dr["Renew_fee_org"].ToString());
                        }

                    }
                }



            }
            return cardTariffAdditionalInformation;
        }

        public static DataTable GetDepositDataChangeFieldTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_deposit_data_change_field_types", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetPhoneBankingContractQuestions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select ID, Question from  [dbo].[Tbl_pBanking_Sec_Questions] order by id asc", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetTypeOfRequestsForFeeCharges()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_Type_Of_Requests_For_Fee_Charges", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        public static DataTable GetLoanMatureTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_types_of_loan_mature", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetCashBookRowTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select ID,case ID when 0 then '´áÉáñÁ' else row_type end as row_type  from tbl_type_of_cash_book_row", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetExternalPaymentTerms()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT t1.code,t1.description,t2.description as description_Eng,t3.description as description_Rus 
                                                    FROM tbl_external_payments_error_codes t1
                                                    LEFT JOIN tbl_external_payments_error_codes t2
                                                    ON t1.code=t2.code
                                                    LEFT JOIN tbl_external_payments_error_codes t3
                                                    ON t1.code=t3.code
                                                    WHERE t1.lang_id=0 AND t2.lang_id=1 AND t3.lang_id=2", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetExternalPaymentProductDetailCodes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT t1.code,t1.description,t2.description as description_Eng,t3.description as description_Rus FROM tbl_external_payments_product_detail_codes t1
                                                LEFT JOIN tbl_external_payments_product_detail_codes t2
                                                ON t1.code=t2.code
                                                LEFT JOIN tbl_external_payments_product_detail_codes t3
                                                ON t1.code=t3.code
                                                WHERE t1.lang_id=0 AND t2.lang_id=1 AND t3.lang_id=2", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetExternalPaymentStatusCodes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT t1.code,t1.description,t2.description as description_Eng,t3.description as description_Rus FROM tbl_external_payments_status_codes t1
                                                LEFT JOIN tbl_external_payments_status_codes t2
                                                ON t1.code=t2.code
                                                LEFT JOIN tbl_external_payments_status_codes t3
                                                ON t1.code=t3.code
                                                WHERE t1.lang_id=0 AND t2.lang_id=1 AND t3.lang_id=2", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }


        public static DataTable GetLoanApplicationQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select number,status from Tbl_application_status", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetLoanApplicationProductTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Q_Union_Types", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetLoanMonitoringTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_loan_monitorings", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetLoanMonitoringFactorGroupes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_monitoring_factor_groups", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetLoanMonitoringFactors(int loanType, int groupId = 0)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sql;
                if (loanType == 0 && groupId == 0)
                    sql = "Select id,description from tbl_type_of_monitoring_factors";
                else if (groupId == 0)
                    sql = "Select id,description from tbl_type_of_monitoring_factors where " + (loanType == 1 ? "agro_loans=1" : "business_loans=1");
                else if (loanType == 0)
                {
                    sql = "Select id,description from tbl_type_of_monitoring_factors WHERE  group_id =@groupId";
                }
                else
                    sql = "Select id,description from tbl_type_of_monitoring_factors WHERE " + (loanType == 1 ? "agro_loans=1" : "business_loans=1") + " and group_id =@groupId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@groupId", SqlDbType.Int).Value = groupId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetProfitReductionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_profits_reduction", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetProvisionCostConclusionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_provision_cost_conclusions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetProvisionQualityConclusionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_provision_quality_conclusions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetLoanMonitoringConclusions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_type_of_monitoring_conclusions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        internal static Dictionary<string, string> GetTransferMethod()
        {
            Dictionary<string, string> TransferMethod = new Dictionary<string, string>();

            TransferMethod.Add("1", "OUR");
            TransferMethod.Add("2", "BEN");
            TransferMethod.Add("3", "OUROUR");

            return TransferMethod;
        }

        public static DataTable GetLoanMonitoringSubTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code,description from Tbl_sub_type_of_loan_monitorings", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetDemandDepositsTariffGroups()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select ID,description from tbl_demand_deposits_tariff_groups", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetAccountRestrictionGroups()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT ROW_NUMBER() OVER(order by code asc) - 1 as row, *  FROM (
                                                            SELECT p.code, t.id, p.description AS prod_descr, t.description AS acc_descr
                                                            FROM tbl_define_sint_acc a
                                                            INNER JOIN Tbl_type_of_accounts t ON a.type_of_account = t.id
                                                            INNER JOIN tbl_type_of_products p ON p.code = a.type_of_product
                                                            WHERE a.type_of_product IN(10, 116, 118, 119)
                                                            AND a.type_of_account = 24
                                                            GROUP BY p.code,t.id,p.description,t.description
												            UNION
													        SELECT 117,24 AS id, 'Ý³ËÁÝïñ³Ï³Ý ÑÇÙÝ³¹ñ³Ù' , 'ÑÇÙÝ³Ï³Ý Ñ³ßÇí'
													        FROM  tbl_type_of_products
                                                        WHERE code = 10) as restriction_groups", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        internal static double GetPenaltyRateOfLoans(int productType, DateTime startDate)
        {
            double penaltyRate = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("exec  sp_get_penalty_rate_of_loans @productType ,	@startDate", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productType", SqlDbType.Int).Value = productType;
                    cmd.Parameters.Add("@startDate", SqlDbType.SmallDateTime).Value = startDate;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            penaltyRate = Convert.ToDouble(dr["penalty_rate"].ToString());
                        }
                    }
                }
            }
            return penaltyRate;
        }

        public static DataTable GetTransferReceiverLivingPlaceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT code,description FROM tbl_transfer_receiver_living_place_types
                                                  WHERE closing_date is null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetTransferSenderLivingPlaceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT code,description FROM tbl_transfer_sender_living_place_types
                                                  WHERE closing_date is null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetTransferAmountTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT code,description FROM tbl_transfer_amount_types
                                                  WHERE closing_date is null", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetOrderQualityTypesForAcbaOnline()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [quality],[action_description_arm],[action_description_eng] FROM [dbo].[Tbl_types_of_HB_quality]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetProductNotificationInformationTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID, Description FROM tbl_types_of_product_notification_information", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetProductNotificationFrequencyTypes(short informationType)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT f.ID, f.Description FROM tbl_product_configuration_types_accordence ac
                                                  INNER JOIN tbl_types_of_product_notification_frequency f
                                                  ON f.Id=ac.frequency_ID
                                                  WHERE ac.frequency_ID IS NOT NULL AND ac.information_ID=@informationType", conn))
                {
                    cmd.Parameters.Add("@informationType", SqlDbType.TinyInt).Value = informationType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetProductNotificationOptionTypes(short informationType)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT o.ID, o.Description FROM tbl_product_configuration_types_accordence ac
                                                  INNER JOIN tbl_types_of_product_notification_option o
                                                  ON o.Id=ac.option_ID
                                                  WHERE ac.option_ID IS NOT NULL AND ac.information_ID=@informationType", conn))
                {
                    cmd.Parameters.Add("@informationType", SqlDbType.TinyInt).Value = informationType;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetProductNotificationLanguageTypes(byte informationType)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT l.ID, l.Description FROM tbl_product_configuration_types_accordence ac
                                                  INNER JOIN tbl_types_of_product_notification_language l
                                                  ON l.Id=ac.language_ID
                                                  WHERE ac.language_ID IS NOT NULL AND ac.information_ID=@informationType", conn))
                {
                    cmd.Parameters.Add("@informationType", SqlDbType.TinyInt).Value = informationType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetProductNotificationFileFormatTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID, Description FROM tbl_types_ofproduct_notification_file_format", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetSwiftMessagMtCodes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                  m.MT, d.description  
                                                  FROM Tbl_SWIFT_messages m 
                                                  LEFT JOIN tbl_SWIFT_MT_descriptions d 
                                                  ON m.MT = d.mt
                                                  WHERE 
                                                  m.message_type = 1
                                                  GROUP BY m.MT, d.description", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }



        public static Dictionary<string, string> GetArcaCardSMSServiceActionTypes()
        {
            Dictionary<string, string> actionTypes = new Dictionary<string, string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,description FROM Tbl_Type_Of_ArcaCardSmsServiceActions", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string key = dr["id"].ToString();
                            string value = Utility.ConvertAnsiToUnicode(dr["description"].ToString());
                            actionTypes.Add(key, value);
                        }
                    }
                }
            }
            return actionTypes;
        }


        public static DataTable GetBondIssueQualities()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select Id,Description from tbl_type_of_bond_issue_quality";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        internal static DataTable GetBondIssuePeriodTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description FROM Tbl_Type_of_Bond_Issues_by_Period";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }


        internal static DataTable GetBondIssuerTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description FROM tbl_type_of_bond_issuer";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        internal static DataTable GetBondRejectReasonTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description FROM Tbl_type_of_bond_reject_reasons";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static DataTable GetBondQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description FROM tbl_type_of_bond_quality";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        public static DataTable GetATSFilials()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = @"SELECT filialcode FROM [tbl_all_accounts;]
                                                                               WHERE
                                                                               account_type = 7
                                                                               AND closing_date IS NULL 
                                    GROUP BY filialcode ";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }


        internal static DataTable GetTypeOfPaymentDescriptions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT type, description FROM tbl_type_of_payment_descriptions";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static DataTable GetTypeofPaymentReasonAboutOutTransfering()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM tbl_type_of_card_debit_reasons";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

            }
            return dt;
        }


        internal static DataTable GetTypeofOperDayClosingOptions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM Tbl_type_of_oper_day_closing_options";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static DataTable GetTypeOf24_7Modes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                connection.Open();
                string SqlQuery = "SELECT * FROM tbl_type_of_24_7_mode";

                using SqlCommand cmd = new SqlCommand(SqlQuery, connection);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }


        internal static DataTable GetTypeOfCommunals()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT * FROM tbl_type_of_utility_services where is_active = 1 ";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        public static DataTable GetCashOrderCurrencies(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT cur.currency, int_code 
                                                        from [Tbl_currency;] cur 
                                                        LEFT JOIN 
                                                        (select a.Arm_number, a.currency from 
                                                        (select * from Tbl_Customers  where customer_number =   @customerNumber) c 
                                                        INNER JOIN 
                                                        (select * from [tbl_all_accounts;] 
                                                        where closing_date is null and customer_number = @customerNumber) a on c.customer_number = a.customer_number 
                                                        INNER JOIN 
                                                        Tbl_define_sint_acc def on c.residence = def.sitizen and c.type_of_client = def.type_of_client and def.link =  (case c.link when 3 then 1 else c.link end) and def.sint_acc_new = a.type_of_account_new 
                                                        LEFT JOIN 
                                                        Tbl_Co_Accounts_Main m on a.Arm_number = m.arm_number 
                                                        where(   c.customer_number = @customerNumber And def.type_of_account = 24 And type_of_product in ( 10,11 )) ) b on cur.currency = b.currency 
                                                        WHERE   b.currency is not null  GROUP BY cur.currency, int_code", conn))
                {
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        internal static DataTable GetActionsForCardTransaction()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description_arm FROM tbl_type_of_card_transaction_action ORDER BY id";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static DataTable GetReasonsForCardTransactionAction(bool useBank)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description_arm, description_eng FROM tbl_type_of_reasons_for_card_transaction_action WHERE " + ((useBank) ? "used_bank = 1" : "used_ibanking = 1 ") + "order by id asc";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static DataTable GetCardLimitChangeOrderActionTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description_arm FROM tbl_type_of_action_card_limit_change_order ORDER BY id";


                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        public static DataTable GetUnFreezeReasonTypesForOrder(int freezeId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select unfreeze_ID, Description from Tbl_type_of_unfreeze_reason R INNER JOIN Tbl_freeze_unfreeze_types_reasons_link link on link.unfreeze_ID=R.ID WHERE freeze_ID=@freezeId", conn))
                {
                    cmd.Parameters.Add("@freezeId", SqlDbType.Int).Value = freezeId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        public static DataTable GetOrderableCardSystemTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select ID as code,CardSystemType as description from Tbl_type_of_CardSystem where CanBeOrdered = 1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetDepositCurrencies()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseNew"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select D.currency from  [Tbl_currency;]  T inner join Tbl_deposit_product_history D  on T.currency=D.currency group by D.currency,  int_code  order by int_code ", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }

        internal static Dictionary<string, string> GetGasPromSectionCode()
        {
            Dictionary<string, string> dt = new Dictionary<string, string>();

            TtResponse GasPromSectionCode = new TtResponse();
            GasPromSectionCodeRequestResponse getSectionCodes = new GasPromSectionCodeRequestResponse();
            getSectionCodes = UtilityOperationService.GetGasPromSectionCodes();
            GasPromSectionCode = getSectionCodes.GasPromSectionCodeOutput.TtResponse;

            foreach (var item in GasPromSectionCode.gazTtField)
            {
                dt.Add(item.kodField, item.anunField.Replace("»", "ե"));
            }

            return dt;
        }

        public static DataTable GetTypeOfDepositStatus()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from [Tbl_type_of_deposit_status]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetTypeOfDeposit()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select code, description from [Tbl_type_of_deposits;]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }



        internal static DataTable GetSwiftPurposeCode()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(" SELECT code, ('('+code +') '+ LEFT(description_rus,90))  AS description_rus,('('+code + ') '+  description_eng)as description_eng FROM Tbl_type_of_swift_purpose_code where description_rus is not null", con))

                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }
        public static DataTable GetSSTOperationTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT provider_name AS providerName, id  AS providerID FROM v_providers
                                                         WHERE id IN (SELECT distinct provider_id FROM [Tbl_SSTerminal_providers_hb_document_types]) ORDER BY providerName", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetSSTerminals(ushort filialCode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT merchant_ID as SSTerminalID, description AS SSTerminalName
                                                        FROM Tbl_ARCA_points_list
                                                        WHERE type_of_point = 4 AND cashin = 1 AND (@filialCode=22000 OR Filial=@filialCode)", conn))
                {
                    cmd.Parameters.Add("@filialCode", SqlDbType.SmallInt).Value = filialCode;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static List<Shop> GetShopList()
        {
            List<Shop> shopList = new List<Shop>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(C.identityId,0) AS identityId,S.shop_id AS shop_id, REPLACE(REPLACE(O.unicode_description,'«',''),'»','')AS legal_name, S.shop_name AS shop_name
                                                         FROM tbl_aparik_shops S
                                                         LEFT JOIN tbl_customers C ON C.customer_number=S.customer_number
                                                         LEFT JOIN Tbl_Organisations O ON C.identityId = O.identityId
														 ORDER BY identityId", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Shop shop = new Shop();
                            shop.ShopID = Convert.ToInt32(dr["shop_id"].ToString());
                            shop.IdentityID = Convert.ToInt64(dr["identityId"].ToString());
                            shop.ShopName = Utility.ConvertAnsiToUnicode(dr["shop_name"].ToString());
                            shop.ShopLegalName = dr["legal_name"].ToString();
                            shopList.Add(shop);
                        }
                    }
                }

            }
            return shopList;
        }
        internal static DataTable GetCreditLineMandatoryInstallmentTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "select * from Tbl_CreditLineMandatoryInstallmentTypes";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }

        public static DataTable GetCurrenciesForReceivedFastTransfer()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select currency From [tbl_currency;] where Quality=1 AND currency IN ('AMD','USD','EUR','RUR')", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;

        }

        public static DataTable GetTemplateDocumentTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT document_type, [description], description_eng " +
                                                       "FROM Tbl_type_of_template_documents", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetAttachedCardTypes(int mainCardType)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT m.attached_card_type, s.CardSystemType+' '+CardType as CardType  
                                                        FROM Tbl_link_of_main_additional_card_types M
                                                        INNER JOIN tbl_type_of_card C
                                                        ON M.attached_card_type = C.id
                                                        INNER JOIN Tbl_type_of_CardSystem s
                                                        ON s.ID = c.CardSystemID
                                                        WHERE main_card_type = @mainCardType", conn))
                {
                    cmd.Parameters.Add("@mainCardType", SqlDbType.Int).Value = mainCardType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;

        }

        public static int GetCardOfficeTypesForIBanking(ushort cardType, short periodicityType)
        {
            int relatedOfficeNumber = 0;


            //ArCa Classic, Visa Electron, Maestro, Visa Classic, MasterCard Standard, Visa Gold, MasterCard Gold,  MasterCard UEFA Champions League, Arca ՄԻՐ, Visa Barerar, 
            //Visa Virtual քարտերի համար՝ 174
            if (cardType == 11 || cardType == 39 || cardType == 16 || cardType == 14
                    || cardType == 47 || cardType == 49 || cardType == 29 || cardType == 46
                    || cardType == 36 || cardType == 37 || cardType == 48 || cardType == 42 || cardType == (uint)PlasticCardType.VISA_VIRTUAL
                    || cardType == 52)
            {
                relatedOfficeNumber = 174;
            }
            //ArCa Credit ` 2014
            else if (cardType == 35)
            {
                relatedOfficeNumber = 2014;
            }
            //Arca Transfer, Maestro Transfer՝ 1098
            else if (cardType == 31 || cardType == 32)
            {
                relatedOfficeNumber = 1098;
            }
            //Arca Pension՝1158
            else if (cardType == 21)
            {
                relatedOfficeNumber = 1158;
            }
            //Visa Student `1291
            else if (cardType == 38)
            {
                relatedOfficeNumber = 1291;
            }
            //American Express Cashback, American Express Blue, American Express Gold` 1665(տարեկան)  կամ 1666(ամսական)
            else if (cardType == 34 || cardType == 40 || cardType == 41 || cardType == 50)
            {
                if (periodicityType == 1)
                    relatedOfficeNumber = 1666;
                else if (periodicityType == 12)
                    relatedOfficeNumber = 1665;
            }
            //Business (Visa Business, Arca Business, Virtual)
            else if (cardType == 22 || cardType == 45 || cardType == 51 || cardType == 45)
            {
                relatedOfficeNumber = 174;
            }



            return relatedOfficeNumber;
        }

        public static DataTable GetCreditLineTypesForOnlineMobile()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select code,description,Description_Engl  from Tbl_type_of_credit_lines WHERE CODE  IN (30,51)", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static double GetCardServiceFee(int cardType, int officeId, string currency)
        {
            double serviceFee = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select Service_fee from Tbl_cards_rates where Office_Id = @officeId and CardType = @cardType and currency = @currency", conn))
                {

                    cmd.Parameters.Add("@officeId", SqlDbType.Int).Value = officeId;
                    cmd.Parameters.Add("@cardType", SqlDbType.Int).Value = cardType;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            serviceFee = Convert.ToInt64(dr["Service_fee"]);
                        }
                    }
                }

            }
            return serviceFee;
        }


        public static DataTable GetAccountClosingReasonsHB()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Idx_Closing, Decsription, Description_eng  FROM Tbl_Acc_Closing_Descriptions WHERE for_HB = 1", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
                return dt;
            }
        }


        public static DataTable GetUtilityAbonentTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select id, description_arm, description_eng from [Tbl_type_of_utility_abonent_types]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetUtilityPaymentTypesOnlineBanking()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select  * from [Tbl_operations_by_period_Types] WHERE isnull(using_from_HB,0) = 1 and Communal_type <> 0", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetPayIfNoDebtTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select  * from Tbl_type_of_periodic_debt_pay_possibility", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetCurrenciesPlasticCardOrder(ushort cardType, int officeId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT DISTINCT currency  FROM Tbl_cards_rates 
                                                         WHERE office_id = @officeId  
                                                         AND  cardtype=@cardType  AND ISNULL(Quality,1)= 1", conn))
                {
                    cmd.Parameters.Add("@officeId", SqlDbType.Int).Value = officeId;
                    cmd.Parameters.Add("@cardType", SqlDbType.Int).Value = cardType;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetLoanMatureTypesForIBanking()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_types_of_loan_mature_for_Ibanking", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static string GetFilialName(int filialCode, Languages language)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT code As id,EnglDescription as description_eng,ArmDescription As description_arm FROM [Tbl_banks;] WHERE code=@filialCode", conn))
                {
                    cmd.Parameters.Add("@filialCode", SqlDbType.Float).Value = filialCode;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return (language == Languages.hy ? Utility.ConvertAnsiToUnicode(dr["description_arm"].ToString()) : dr["description_eng"].ToString());
                        }
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// Վերադարձնում է Քարտի սպասարկման վարձի գանձման պարբերականության ցանկը
        /// </summary>
        /// <returns></returns>
        public static DataTable GetServiceFeePeriodocityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Select * From types_of_service_fee_periodicity", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetCardClosingReasonsForDigitalBanking()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id,description, description_eng from Tbl_Type_Of_CardClosingReasons where is_old=0 and for_digital_banking = 1 order by id_fororderby", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        //HB
        public static DataTable GetHBSourceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT code,(cast(code as nvarchar(5)) + ' ' + description) as description FROM Tbl_type_of_banking_source WHERE code in (1,5,6,8,10)",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetHBQualityTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT quality, (cast(quality as nvarchar(5)) + ' ' + description_arm) as description_arm FROM Tbl_types_of_HB_quality WHERE quality not in (40,1,2,4,5,6,100,55,56,57)",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetHBDocumentTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT product_type, (cast(product_type as nvarchar(5)) + ' ' + [dbo].[fnc_convertAnsiToUnicode](product_description)) as description FROM Tbl_types_of_HB_products WHERE is_hb_product=1 OR for_automat_confirmated_HB = 1",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetHBDocumentSubtypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT document_sub_type, (cast(document_sub_type as nvarchar(5)) + ' ' + description) as description FROM Tbl_sub_types_of_HB_products WHERE  document_type=1 and document_sub_type in (1,2)",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        public static DataTable GetHBRejectTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT  for_orderby, Reject_description FROM Tbl_types_of_HB_rejects where closing_date is null ORDER BY for_orderby asc",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }
        internal static Dictionary<string, string> GetCardRemovalReasons()
        {
            Dictionary<string, string> reasons = new Dictionary<string, string>();

            string sql = "select * from tbl_card_remove_reasons";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            reasons.Add(rd["ID"].ToString(), rd["reason_description"].ToString());
                        }
                    }
                }
            }

            return reasons;
        }


        public static DataTable GetAccountRestTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_types_of_digital_account_rest",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }
        public static DataTable GetTransferReceiverTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Tbl_Types_Of_Transfer_receiver",
                        conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static byte CommunicationTypeExistence(ulong customerNumber)
        {
            byte hasCommunication = 0;
            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
            if (isTestVersion)
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    conn.Open();
                    string sqltext = "SELECT dbo.fn_check_communication_type_existence(@customerNumber)";

                    using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                                hasCommunication = byte.Parse(dr[0].ToString());
                        }
                    }
                }
            }
            //DIANA!!!! Remove
            //hasCommunication = 0;
            //*****
            return hasCommunication;
        }

        public static double GetMaxAvailableAmountForNewCreditLine(double productId, int creditLineType, string provisionCurrency, bool existRequiredEntries, ulong customerNumber)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT dbo.fn_GetMaxAvailableAmountForNewCreditLine(@productId, @creditLineType, @provisionCurrency, @existRequiredEntries, @customerNumber)";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@creditLineType", SqlDbType.Int).Value = creditLineType;
                    cmd.Parameters.Add("@provisionCurrency", SqlDbType.NVarChar).Value = provisionCurrency;
                    cmd.Parameters.Add("@existRequiredEntries", SqlDbType.Bit).Value = existRequiredEntries;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            amount = double.Parse(dr[0].ToString());
                    }
                }
            }
            return amount;
        }
        public static DataTable GetCardReceivingTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select type_id, description from tbl_type_of_card_receiving", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetCardApplicationAcceptanceTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select type_id, description from tbl_type_of_card_application_acceptance", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }


        public static DataTable GetVirtualCardStatusChangeReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select ID as code,description from tbl_type_of_virtual_card_change_reason", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static DataTable GetVirtualCardChangeActions(int status)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select code,description from tbl_type_of_virtual_card_change_actions A INNER JOIN 
														[tbl_virtual_card_allowed_change_actions_for_status] S ON A.code = S.action where change_allow=1 and S.status=@status", conn))
                {
                    cmd.Parameters.Add("@status", SqlDbType.SmallInt).Value = status;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        public static string GetCurrencyLetter(string currency)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select dbo.fnc_convertAnsiToUnicode(letter) as letter from  [tbl_currency;] where currency=@currency", conn))
                {
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;

                    return cmd.ExecuteScalar().ToString();
                }
            }
        }


        internal static string GetMandatoryEntryInfo(byte id, byte languages)
        {
            string mandatoryEntryInfo = string.Empty;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand("SELECT CASE WHEN @lang = 1 THEN description ELSE description_eng END AS description FROM  [Tbl_mandatory_entry_info] WHERE id = @id", conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@lang", SqlDbType.Int).Value = languages;

                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                    mandatoryEntryInfo = dr["description"].ToString();
            }

            return mandatoryEntryInfo;
        }

        internal static DataTable GetReferenceReceiptTypes(byte language)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT id, CASE WHEN @lang = 1 THEN [description] ELSE [description_eng] END AS description FROM Tbl_type_of_reference_receipt", conn);
                cmd.Parameters.Add("@lang", SqlDbType.SmallInt).Value = language;
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);


            }
            return dt;
        }


        internal static DataTable GetCustomerAllPassports(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT document_given_date, document_number, document_given_by FROM tbl_customer_documents_current
	                                                    where identityId IN (SELECT identityId from tbl_customers
	                                                    where customer_number = @customer_number) AND document_valid_date IS NOT NULL
                                                        ORDER BY is_default DESC", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        internal static DataTable GetCustomerAllPhones(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT countryCode + ' ' + areaCode + ' ' + phoneNumber AS phoneNumber FROM [Tbl_Customer_Phones] cp
	                                                    INNER JOIN [dbo].[Tbl_Phones] p ON p.id = cp.phoneId 
	                                                    WHERE cp.[identityId] IN (SELECT identityId FROM tbl_customers
	                                                    WHERE customer_number = @customer_number)", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }


            }
            return dt;
        }

        internal static DataTable GetCardAdditionalDataTypes(string cardNumber, string expiryDate)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select additionID,AdditionDescription from Tbl_Type_of_VisaApp_Add where AdditionID  in (3,4,9,10,14)
                                                         And AdditionID not in (Select AdditionID from tbl_visaAppAdditions 
                                                         where cardnumber = @cardNumber and expirydate = @expiryDate)", conn))
                {
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar).Value = cardNumber;
                    cmd.Parameters.Add("@expiryDate", SqlDbType.VarChar).Value = expiryDate;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        internal static string GetSentCityBySwiftCode(string swiftCode)
        {
            string city = string.Empty;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select CountryCodeN from Tbl_world_banks where swift_code = @swiftCode", conn);
                cmd.Parameters.Add("@swiftCode", SqlDbType.NVarChar).Value = swiftCode;

                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                    city = dr["CountryCodeN"].ToString();
            }
            return city;
        }

        public static DataTable GetCardNotRenewReasons()
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT id,REPLACE(description,'âí»ñ³ÃáÕ³ñÏí³Í ù³ñï:','') AS description FROM Tbl_Type_Of_CardClosingReasons WHERE forNotRnew = 1", conn))
            {
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }

            return dt;
        }

        internal static DataTable GetReasonsForCardTransactionActionForUnblocking()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT id, description_arm, description_eng FROM tbl_type_of_reasons_for_card_transaction_action order by id asc";

                using SqlCommand cmd = new SqlCommand(sqltext, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        internal static bool HasSocialSecurityAccount(ulong customerNumber)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 card_account FROM [dbo].Tbl_Visa_Numbers_Accounts
                                                        where card_type = 21 and closing_date is NULL AND customer_number = @customer_number
                                                        UNION
                                                        SELECT a.Arm_number FROM V_All_Accounts a 
                                                                INNER JOIN(SELECT sint_acc_new, type_of_client, type_of_product FROM  dbo.Tbl_define_sint_acc WHERE(type_of_product = 118) AND(type_of_account = 24)                                                GROUP BY sint_acc_new, type_of_client, type_of_product)s ON a.type_of_account_new = s.sint_acc_new INNER JOIN
		                                                        Tbl_type_of_products t ON type_of_product = t.code 
		                                                        where a.customer_number = @customer_number and a.closing_date is null ", conn);
                cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                    check = true;
            }
            return check;
        }

        internal static DataTable GetTypeOfLoanDelete()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID, Description FROM tbl_types_of_loan_delete ORDER BY ID", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }


        internal static DataTable GetThirdPersonAccounts(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT  acc.* , c.*, C_main.type_of_client as main_customer_type_of_client
                                                                                                FROM [tbl_all_accounts;] a INNER JOIN Tbl_Co_Accounts_Main acc on a.arm_number = acc.arm_number INNER JOIN [Tbl_Co_Accounts] c on c.co_main_id = acc.id 
		                                                                                                INNER JOIN (select sint_acc_new from tbl_define_sint_acc where  type_of_product = 10 and type_of_account = 24 group by sint_acc_new ) sint on sint.sint_acc_new = a.type_of_account_new 
		                                                                                                INNER JOIN (select C.customer_number, P.birth[birth], C.type_of_client from Tbl_Customers C WITH (nolock) LEFT JOIN Tbl_Persons P on C.identityId = P.identityId) third_person_cust 
				                                                                                                on acc.third_person_Customer_number = third_person_cust.customer_number 
		                                                                                                INNER JOIN Tbl_Customers C_main on c.customer_number = C_main.customer_number
                                                                                                WHERE acc.Type = 2 And acc.closing_date Is null and a.closing_date is null 
                                                                                                and c.customer_number = @customer_number", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }

        public static DataTable GetActiveDepositTypesForNewDepositOrderForDigitalBanking(short allowableCustomerType, short allowableForThirdhPerson, short allowableForCooperative)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT code, description, description_Engl,allowable_customer_type,allowable_for_thirdh_person 
                                                  FROM [Tbl_type_of_deposits;] WHERE closing_date is null AND (((allowable_customer_type=@allowableCustomerType OR allowable_customer_type=0)
                                                  AND allowable_for_cooperative=@allowableForCooperative) OR allowable_for_thirdh_person = @allowableForThirdhPerson)", conn);

                cmd.Parameters.Add("@allowableCustomerType", SqlDbType.SmallInt).Value = allowableCustomerType;
                cmd.Parameters.Add("@allowableForThirdhPerson", SqlDbType.Bit).Value = allowableForThirdhPerson;
                cmd.Parameters.Add("@allowableForCooperative", SqlDbType.Bit).Value = allowableForCooperative;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }

        /// <summary>
        /// Դիջիթալից մուտքագրվող հօգուտ 3-րդ անձի ավանդի համար վերադարձնում է հասանելի արժույթները։
        /// </summary>
        /// <returns></returns>
        public static DataTable GetJointDepositAvailableCurrency(ulong customerNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT acc.account_currency
                                                                                    FROM [Tbl_Co_Accounts_Main] acc INNER JOIN Tbl_co_accounts c ON acc.ID = c.co_main_ID 
                                                                                    WHERE type = 2 and acc.closing_date is null
						                                                                                    and acc.account_currency IN ('AMD', 'USD')
                                                                                                            and c.customer_number =  @customer_number
                                                                                    GROUP BY acc.account_currency
                                                                                    ORDER BY acc.account_currency", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }
        internal static DataTable GetCommissionNonCollectionReasons()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [Id],[description] FROM [dbo].[tbl_types_of_commission_nonCollection_reasons]", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                return dt;
            }
        }

        internal static DataTable GetDepositoryAccountOperators()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("select bank_code, description from tbl_depository_account_operators", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }
            }
            return dt;
        }
    }
}
