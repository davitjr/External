using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class AccountDataChangeOrderDB
    {
        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondDataChange(ulong customerNumber, string accountNumber)
        {
            bool secondDataChange;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=50 and document_subtype=1 and
                                                debet_account=@accountNumber and customer_number=@customerNumber", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    secondDataChange = true;
                }
                else
                {
                    secondDataChange = false;
                }
                    
            }
            return secondDataChange;
        }

        

        /// <summary>
        /// Հաշվի տվյալների խմբագրման կրկնակի հայտի առկայության ստուգում
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="statementDeliveryType"></param>
        /// <returns></returns>
        internal static bool IsSameAdditionalDataChange(string accountNumber, AdditionalDetails additionalDetails)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_all_accounts_AddInf 
                                                    WHERE AdditionID=@additionType AND  arm_number=@accountNumber AND AdditionValue=@additionValue", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                cmd.Parameters.Add("@additionType", SqlDbType.Int).Value = additionalDetails.AdditionType;
                cmd.Parameters.Add("@additionValue", SqlDbType.NVarChar,200).Value = additionalDetails.AdditionValue;

                return cmd.ExecuteReader().Read();
            }
        }

        /// <summary>
        /// Հաճախորդի մյուս հաշիվներում սպասարկման վարձի գանձման նշանի առկայություն
        /// </summary>
        /// <returns></returns>
        internal static bool IfExistsServiceFeeCharge(AccountDataChangeOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select 1 From Tbl_all_accounts_AddInf 
                                                    WHERE arm_number in (SELECT arm_number FROM [Tbl_all_accounts;] WHERE arm_number<>@accountNumber and customer_number = @customerNumber )  
                                                    and AdditionId =13", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.DataChangeAccount.AccountNumber;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;

                return cmd.ExecuteReader().Read();
               
            }
        }

        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(AccountDataChangeOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[dbo].[pr_account_data_change_order]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar).Value = order.DataChangeAccount.AccountNumber;
                    cmd.Parameters.Add("@addition_type", SqlDbType.SmallInt).Value = order.AdditionalDetails.AdditionType;
                    cmd.Parameters.Add("@addition_value", SqlDbType.NVarChar,200).Value = order.AdditionalDetails.AdditionValue;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add(new SqlParameter("@Doc_ID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@Doc_ID"].Value);
                    result.ResultCode = ResultCode.Normal;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }

        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static AccountDataChangeOrder Get(AccountDataChangeOrder order)
        {
            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string sqlString = @"SELECT registration_date,document_number,document_subtype,document_type,quality,source_type,debet_account,addition_type,isnull(AdditionDescription,'') as AdditionDescription, addition_value,HB.operation_date
                                        FROM dbo.Tbl_HB_documents HB
                                        INNER JOIN dbo.tbl_account_data_change_order_details CD on HB.doc_ID=CD.doc_ID
                                        LEFT JOIN Tbl_type_of_all_acc_additions T on CD.addition_type=T.AdditionId
                                        WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end AND HB.doc_ID=@docID ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    order.AdditionalDetails = new AdditionalDetails();
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.DataChangeAccount = Account.GetAccount(Convert.ToUInt64(dr["debet_account"].ToString()));
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.AdditionalDetails.AdditionType = ushort.Parse(dr["addition_type"].ToString());
                    order.AdditionalDetails.AdditionTypeDescription = Utility.ConvertAnsiToUnicode(dr["AdditionDescription"].ToString());
                    order.AdditionalDetails.AdditionValue = (order.AdditionalDetails.AdditionType == 5) ? Info.GetStatementDeliveryTypeDescription(Convert.ToInt16(dr["addition_value"].ToString()), Languages.hy) : (dr["addition_value"].ToString());
                                        order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                    
                }

            }
            return order;
        }

        internal static Dictionary<string,string> GetAccountAdditionsTypes()
        {
            DataTable dt = Info.GetAccountAdditionsTypes();

            Dictionary<string, string> additionsTypes = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {   
                additionsTypes.Add(dt.Rows[i]["AdditionID"].ToString(), Utility.ConvertAnsiToUnicode(dt.Rows[i]["AdditionDescription"].ToString()));
            }

            return additionsTypes;
        }


        public static AdditionalValueType GetAccountAdditionValueType(ushort additionType)
        {
            
            AdditionalValueType additionalValueType = AdditionalValueType.NotSpecified; 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT DataType FROM Tbl_type_of_all_acc_additions WHERE AdditionId = @additionId", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@additionId", SqlDbType.Int).Value = additionType;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    additionalValueType =(AdditionalValueType) Enum.Parse(typeof(AdditionalValueType),dr["DataType"].ToString());
                }

            }
            return additionalValueType;
        }

        internal static bool IsApprovedByTaxService(string accountNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT account_number FROM Tbl_acc_freeze_history WHERE reason_type=14 AND closing_date IS NULL AND account_number=@accountNumber", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;

                return !cmd.ExecuteReader().Read();
              
            }
        }
    }
}
