using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;

namespace ExternalBanking.DBManager
{
    /// <summary>
    /// class SwiftMessageDB նկարագրում է ուղարկվող և ստացվող SWIFT հաղորդագրութունների կառուցվածքը ըստ տարբեր տեսակների
    /// </summary>
    class SwiftMessageDB
    {
        /// <summary>
        /// Ուղարկվող SWIFT հաղորդագրության գեներացում և մուտքագրում տվյալների բազայում Tbl_SWIFT_messages table-ի մեջ
        /// </summary>

        /// <param name="messageUniqNumber">
        /// SWIFT հաղորդագրության ունիկալ համար
        /// </param> 

        /// <param name="accountNumber">
        /// SWIFT հաղորդագրության անալիտիկ հաշիվ
        /// </param>

        /// <param name="receiverBankSwiftCode">
        /// SWIFT հաղորդագրություն ստացող բանկի SWIFT կոդ (8-ից 12 նշան) 
        /// </param>
        /// 
        /// <param name="mtType">
        /// SWIFT հաղորդագրության տեսակի կոդը (ըստ SWIFT համակարգի)
        /// </param>

        /// <param name="filialCode">
        /// SWIFT հաղորդագրության պատկանելիության մասնաճյուղի կոդ
        /// </param>

        /// <param name="registrationDate">
        /// SWIFT հաղորդագրության գրանցման ամսաթիվ
        /// </param>

        /// <param name="userID">
        /// SWIFT հաղորդագրության գրանցողի (ՊատԿատարողի) կոդ
        /// </param>

        /// <param name="feeAmount">
        /// SWIFT հաղորդագրության միջնորդավճարի գումարը ազգային արժույթով (AMD)
        /// </param>

        /// <param name="feeAccount">
        /// SWIFT հաղորդագրության միջնորդավճարի գանձման հաշվի համար
        /// </param>

        /// <returns ActionResult>
        /// Վերադարձնում է ActionResult տեսակի օբյեկտ, որը ցույց է տալիս արդյոք մուտքագրումը տեղի է ունեցել թե ոչ 
        /// </returns>

        internal static ulong GenerateNewOuptutSwiftMessage(Account accountNumber,
                                                                        string receiverBankSwiftCode, ulong mtType, ulong filialCode,
                                                                        DateTime registrationDate, short userID,
                                                                        double feeAmount, Account feeAccount)
        {
            ActionResult result = new ActionResult();
            ulong messageUniqNumber = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();


                messageUniqNumber = UtilityDB.GetLastKeyNumber(36, Constants.HEAD_OFFICE_FILIAL_CODE);

                SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_SWIFT_messages(
                                                              unic_number,account_number,customer_number,filial,
                                                              registration_date,registration_set_number,
                                                              type,MT,SWIFT_code,fee_amount,
                                                              deb_for_transfer_payment, message_type,[I/O]) 
                                                              VALUES
                                                            (@MessageUnicNumber,@AccountNumber,@CustomerNumber,@Filial,
                                                             @RegistrationDate,@RegistrationPKNumber,
                                                             @Type,@MT,@SWIFTcode,@feeAmount, 
                                                             @feeAccount,@messageType,@IO)", conn);

                cmd.Parameters.Add("@MessageUnicNumber", SqlDbType.Int).Value = messageUniqNumber;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.Float).Value = accountNumber.AccountNumber;
                cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = accountNumber.GetAccountCustomerNumber();
                cmd.Parameters.Add("@Filial", SqlDbType.Int).Value = filialCode;
                cmd.Parameters.Add("@RegistrationDate", SqlDbType.SmallDateTime).Value = registrationDate;
                cmd.Parameters.Add("@RegistrationPKNumber", SqlDbType.Int).Value = userID;
                cmd.Parameters.Add("@Type", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@MT", SqlDbType.Int).Value = mtType;
                cmd.Parameters.Add("@IO", SqlDbType.Int).Value = 1;
                cmd.Parameters.Add("@SWIFTcode", SqlDbType.NChar, 12).Value = receiverBankSwiftCode.ToString();
                cmd.Parameters.Add("@feeAmount", SqlDbType.Float).Value = feeAmount;
                cmd.Parameters.Add("@feeAccount", SqlDbType.Float).Value = feeAccount.AccountNumber;
                cmd.Parameters.Add("@messageType", SqlDbType.Int).Value = 1;
                cmd.ExecuteNonQuery();

                result.ResultCode = ResultCode.Normal;
            }
            return messageUniqNumber;
        }


        /// <summary>
        /// SWIFT Message-ի տվյալների ստացում տվյալների բազայից ըստ ունիկալ համարի
        /// </summary>

        /// <param name="messageUniqNumber">
        /// messageUniqNumber - պարբերական փոխանվման ունիկալ համար
        /// </param>

        /// <returns SwiftMessage>
        /// վերադարձնում է SwiftMessage տոեսակի օբյեկտ
        /// </returns>

        internal static SwiftMessage GetSwiftMessage(ulong messageUniqNumber)
        {
            SwiftMessage swiftMessage = null;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"SELECT      
                                                                unic_number,
                                                                account_number,
                                                                customer_number,
                                                                filial,
                                                                registration_date,
                                                                registration_set_number,
                                                                [I/O],
                                                                file_name,
                                                                type,
                                                                MT,
                                                                SWIFT_code,
                                                                fee_amount,
                                                                deb_for_transfer_payment, message_type, 
                                                                confirmation_date,
                                                                confirmation_set_number,
                                                                transactions_group_number,
                                                                file_created_date,
                                                                deleted,
                                                                deleted_set_number,
                                                                file_content,
                                                                receiver_account,
                                                                amount,
                                                                currency,
                                                                description,
                                                                receiver_name,
                                                                [receiver_bank_swift],
                                                                [intermidate_bank_swift],
                                                                receiver_swift,
                                                                rejected
                                                                FROM Tbl_SWIFT_messages
                                                                WHERE unic_number=@MessageUniqNumber", conn);

                cmd.Parameters.Add("@MessageUniqNumber", SqlDbType.Int).Value = messageUniqNumber;
                cmd.ExecuteNonQuery();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        swiftMessage = SetSwiftMessage(dt.Rows[i]);
                    }
                }
            }
            return swiftMessage;
        }

        private static SwiftMessage SetSwiftMessage(DataRow row)
        {
            SwiftMessage swiftMessage = new SwiftMessage();

            swiftMessage.ID = Convert.ToUInt64(row["unic_number"]);
            swiftMessage.Account = Account.GetAccount(Convert.ToUInt64(row["account_number"]));
            swiftMessage.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
            if (row["filial"] != DBNull.Value)
                swiftMessage.FilialCode = Convert.ToUInt32(row["filial"]);
            swiftMessage.RegistrationDate = DateTime.Parse(row["registration_date"].ToString());
            swiftMessage.UserId = Convert.ToUInt32(row["registration_set_number"]);
            swiftMessage.InputOutput = Convert.ToInt32(row["I/O"]);
            swiftMessage.MessageType = Convert.ToInt32(row["message_type"]);
            swiftMessage.MtCode = Convert.ToInt32(row["MT"]);
            if (row["SWIFT_code"] != DBNull.Value)
                swiftMessage.SWIFTCode = row["SWIFT_code"].ToString();
            swiftMessage.FeeAmount = Convert.ToDouble(row["fee_amount"]);
            if (row["amount"] != DBNull.Value)
                swiftMessage.Amount = Convert.ToDouble(row["amount"]);
            swiftMessage.FeeAccount = Account.GetAccount(Convert.ToUInt64(row["deb_for_transfer_payment"]));
            swiftMessage.FileName = row["file_name"].ToString();
            swiftMessage.Currency = row["currency"].ToString();

            if (row["confirmation_date"] != DBNull.Value)
                swiftMessage.ConfirmationDate = Convert.ToDateTime(row["confirmation_date"]);
            if (row["confirmation_set_number"] != DBNull.Value)
                swiftMessage.ConfirmationSetNumber = Convert.ToInt32(row["confirmation_set_number"]);
            if (row["transactions_group_number"] != DBNull.Value)
                swiftMessage.TransactionNumber = row["transactions_group_number"].ToString();
            if (row["file_created_date"] != DBNull.Value)
                swiftMessage.FileCreatedDate = Convert.ToDateTime(row["file_created_date"]);
            if (row["deleted"] != DBNull.Value)
                swiftMessage.IsDeleted = Convert.ToBoolean(row["deleted"]);
            if (swiftMessage.IsDeleted && row["deleted_set_number"] != DBNull.Value)
                swiftMessage.DeletedSetNumber = Convert.ToInt32(row["deleted_set_number"]);
            if (row["rejected"] != DBNull.Value)
                swiftMessage.IsRejected = Convert.ToBoolean(row["rejected"]);
            if (row["file_content"] != DBNull.Value)
                swiftMessage.FileContent = Utility.ConvertAnsiToUnicode(row["file_content"].ToString());
            if (row["receiver_account"] != DBNull.Value)
                swiftMessage.ReceiverAccount = row["receiver_account"].ToString();
            if (row["description"] != DBNull.Value)
                swiftMessage.Description = row["description"].ToString();
            else
                swiftMessage.Description = "";
            if (row["receiver_name"] != DBNull.Value)
                swiftMessage.Receiver = row["receiver_name"].ToString();
            if (row["receiver_swift"] != DBNull.Value)
                swiftMessage.ReceiverSwift = row["receiver_swift"].ToString();
            if (row["receiver_bank_swift"] != DBNull.Value)
                swiftMessage.ReceiverBankSwift = row["receiver_bank_swift"].ToString();
            if (row["intermidate_bank_swift"] != DBNull.Value)
                swiftMessage.IntermediaryBankSwift = row["intermidate_bank_swift"].ToString();



            return swiftMessage;
        }

        /// <summary>
        /// SWIFT քաղվածքի գեներացում 
        /// </summary>

        /// <param name="messageUniqNumber">
        /// messageUniqNumber - պարբերական փոխանվման ունիկալ համար
        /// </param>

        /// <param name="dateStatement">
        /// dateStatement - քաղվածքի ամսաթիվ
        /// </param>

        /// <param name="dateFrom">
        /// dateFrom - ժամանակահատվածի սկիզբ,որի համար գեներացվում է քաղվածքը
        /// </param>

        /// <param name="dateTo">
        /// dateTo - ժամանակահատվածի վերջ,որի համար գեներացվում է քաղվածքը
        /// </param>

        /// <param name="operationsCountInOnePart">
        /// operationsCountInOnePart - մակսիմալ գործարքների քանակ, որոնք կազմում են քաղվածքի մեկ մաս 
        /// </param>

        /// <returns ActionResult>
        /// Վերադարձնում է ActionResult տեսակի օբյեկտ, որը ցույց է տալիս արդյոք մուտքագրումը տեղի է ունեցել թե ոչ 
        /// </returns>


        public static ActionResult MakeSwiftStatement(SwiftMessage swiftMessage, DateTime dateStatement, DateTime dateFrom, DateTime dateTo, int operationsCountInOnePart = 20)
        {

            ActionResult result = new ActionResult();
            Array ArrOfExtractParts = SwiftMessage.MakeOneRowFileContent(swiftMessage, dateStatement, dateFrom, dateTo, operationsCountInOnePart);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
            }))
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    conn.Open();
                    for (int i = 0; i < ArrOfExtractParts.Length; i++)
                    {
                        InsertSwiftExtractRow(swiftMessage.ID, swiftMessage.ID.ToString().PadLeft(6, '0'), i + 1, ArrOfExtractParts.GetValue(i).ToString(), conn);
                    }
                    scope.Complete();

                }

                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }

        public static Array MakeSwift940Statement(SwiftMessage swiftMessage, DateTime dateStatement, DateTime dateFrom, DateTime dateTo, int operationsCountInOnePart = 20)
        {
            Array ArrOfExtractParts = SwiftMessage.MakeOneRowFileContentMT940(swiftMessage, dateStatement, dateFrom, dateTo, operationsCountInOnePart);

            return ArrOfExtractParts;
        }


        /// <summary>
        /// SWIFT քաղվածքի մեկ տողի մուտքագրոմ տվյալների բազայի մեջ
        /// </summary>

        /// <param name="messageUniqNumber">
        /// messageUniqNumber - պարբերական փոխանվման ունիկալ համար
        /// </param>

        /// <param name="fileName">
        /// fileName - SWIFT համակարգով ուղարկվող ֆայլի անուն
        /// </param>

        /// <param name="filePartNumber">
        /// fileName - SWIFT համակարգով ուղարկվող ֆայլի մասի համար
        /// </param>

        /// <param name="conn">
        /// conn - SqlConnection
        /// </param>

        /// <returns ActionResult>
        /// Վերադարձնում է ActionResult տեսակի օբյեկտ, որը ցույց է տալիս արդյոք մուտքագրումը տեղի է ունեցել թե ոչ 
        /// </returns>

        internal static ActionResult InsertSwiftExtractRow(ulong messageUnicNumber, string fileName, int filePartNumber, string oneRowFileContent, SqlConnection conn)
        {
            /// <summary>
            /// SWIFT քաղվածքի պահպանում
            /// ամեն մի կտորը` առանձին տողը 

            ActionResult result = new ActionResult();
            SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_SWIFT_messages_details 
                                                    (
                                                            message_unic_number, 
                                                            file_name, 
                                                            file_part_number, 
                                                            file_part_content
                                                                )
                                                            VALUES
                                                (
                                                            @MessageUnicNumber,
                                                            @FileName,
                                                            @FilePartNumber,
                                                            @FilePartContent)", conn);

            cmd.Parameters.Add("@MessageUnicNumber", SqlDbType.Int).Value = messageUnicNumber;
            cmd.Parameters.Add("@FileName", SqlDbType.NVarChar, 50).Value = "Ia" + fileName + "_" + filePartNumber.ToString().PadLeft(3, '0') + ".dos";
            cmd.Parameters.Add("@FilePartNumber", SqlDbType.SmallInt).Value = filePartNumber;
            cmd.Parameters.Add("@FilePartContent", SqlDbType.Text).Value = oneRowFileContent;

            cmd.ExecuteNonQuery();

            result.ResultCode = ResultCode.Normal;
            return result;

        }


        internal static List<SwiftMessage> GetSwiftMessagesStatement(DateTime dateFrom, DateTime dateTo, string accountNumber)
        {

            List<SwiftMessage> swiftMessages = new List<SwiftMessage>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT 
                                                            unic_number,
                                                            account_number,
                                                            customer_number,
                                                            filial,
                                                            registration_date,
                                                            registration_set_number,
                                                            [I/O],
                                                            file_name,
                                                            type,
                                                            MT,
                                                            SWIFT_code,
                                                            fee_amount,
                                                            deb_for_transfer_payment, 
                                                            message_type,
                                                            confirmation_date,
                                                            confirmation_set_number,
                                                            transactions_group_number,
                                                            file_created_date,
                                                            deleted,
                                                            deleted_set_number,
                                                            file_content,
                                                            receiver_account,
                                                            amount,
                                                            currency,
                                                            description,
                                                            receiver_name,
                                                            [receiver_bank_swift],
                                                            [intermidate_bank_swift],
                                                            receiver_swift,
                                                            rejected    
                                                            FROM Tbl_SWIFT_messages
                                                            WHERE registration_date>=@dateFrom and registration_date<=@dateTo
                                                            and account_number=@accountNumber", conn);

                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;
                cmd.ExecuteNonQuery();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        SwiftMessage swiftMessage = null;
                        swiftMessage = SetSwiftMessage(dt.Rows[i]);
                        swiftMessages.Add(swiftMessage);
                    }
                }
            }
            return swiftMessages;
        }

        internal static ActionResult SaveTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_transaction_Swift_Confirm_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@swiftMessageID", SqlDbType.Float).Value = order.SwiftMessageId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }

        internal static ActionResult SaveSwiftMessageRejectOrder(SwiftMessageRejectOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_Swift_Message_Reject_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@swiftMessageID", SqlDbType.Float).Value = order.SwiftMessageId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 500).Value = order.Description;

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }
        internal static TransactionSwiftConfirmOrder GetTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT  d.document_number,d.currency,d.debet_account,d.quality,d.description,
                                                  d.registration_date,d.document_subtype,d.source_type,d.operation_date,L.link_id
                                                  FROM Tbl_HB_documents D
                                                  INNER JOIN Tbl_Hb_Document_link_identity L ON D.doc_id=L.doc_id  
                                                  where D.Doc_ID=@DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());

                order.SwiftMessageId = int.Parse(dt.Rows[0]["link_id"].ToString());
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Description = dt.Rows[0]["description"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                return order;
            }

        }

        public static bool CheckSentSwiftStatus(int swiftMessageID)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"select dbo.fn_check_sent_swift_status(@swiftMessageID) as result", conn);

                cmd.Parameters.Add("@swiftMessageID", SqlDbType.Float).Value = swiftMessageID;

                result = Convert.ToBoolean(cmd.ExecuteScalar());

                return result;

            }
        }

        public static DataTable GetSWIFTPeriodicTransfers(DateTime setDate)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_get_swift_periodic_transfers";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = setDate;
                    dt.Load(cmd.ExecuteReader());
                }
                return dt;
            }
        }
        public static DataTable GetBankMailTransactions(List<AccountStatementDetail> statementDetails)
        {
            DataTable dt = new DataTable();
            DataTable returnTable = new DataTable();
            dt.Columns.Add("TransactionsGroupNumber");
            foreach (var item in statementDetails)
            {
                dt.Rows.Add(item.TransactionsGroupNumber);
            }
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                sqlConnection.Open();
                SqlCommand cmd = new SqlCommand("pr_get_bank_mail_transactions", sqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@table", SqlDbType.Structured).Value = dt;
                returnTable.Load(cmd.ExecuteReader());
            }
            return returnTable;
        }

        public static DataTable GetSwiftTransactions(List<AccountStatementDetail> statementDetails)
        {
            DataTable dt = new DataTable();
            DataTable returnTable = new DataTable();
            dt.Columns.Add("TransactionsGroupNumber");
            foreach (var item in statementDetails)
            {
                dt.Rows.Add(item.TransactionsGroupNumber);
            }
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                sqlConnection.Open();
                SqlCommand cmd = new SqlCommand("pr_get_swift_transactions", sqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@table", SqlDbType.Structured).Value = dt;
                returnTable.Load(cmd.ExecuteReader());
            }
            return returnTable;
        }
    }
}