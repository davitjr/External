using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal static class CredentialOrderDB
    {
        /// <summary>
        /// Հաճախորդի լիազորագրի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(CredentialOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_AddNew_CredentialDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.TinyInt).Value = order.Type;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = order.Description;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operationDate", SqlDbType.DateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    cmd.Parameters.Add(new SqlParameter("@Doc_ID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@Doc_ID"].Value);
                    result.ResultCode = ResultCode.Normal;
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;

                }
                return result;
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static CredentialOrder Get(CredentialOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@" SELECT registration_date,document_number,customer_number,document_type,document_subtype,quality,debet_account,amount,currency, D.source_type,D.order_group_id 
		                                           FROM dbo.Tbl_HB_documents D 
                                                   WHERE Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());

                order.Credential = GetCredentialOrderDetails((ulong)order.Id, order.Source);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

            }
            return order;
        }

        /// <summary>
        /// Վերադարձնում է բոլոր լիազորագրի գործողությունները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static List<AssigneeOperation> GetAllOperations(int typeOfCustomer)
        {
            DataTable dt = new DataTable();
            List<AssigneeOperation> operationList = new List<AssigneeOperation>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string whereCond;
                if (typeOfCustomer == 6)
                    whereCond = " AND tao.forNaturalPerson=1 ";
                else if (typeOfCustomer == 2)
                    whereCond = " AND tao.forPrivateEntrepreneur=1 ";
                else
                    whereCond = " AND tao.forLegalEntity=1 ";

                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT tao.id opType, tao.description opTypeDescr, tao.Groupid, AOG.description opGroupDescription, ISNULL(operationTypeId,0) operationTypeId 
                                                FROM Tbl_type_of_assign_operation tao 
                                                LEFT JOIN (SELECT operationTypeId FROM Tbl_assign_operation_account_types GROUP BY operationTypeId) aoa ON tao.ID=aoa.operationTypeId 
                                                INNER JOIN Tbl_Type_of_assign_operations_groups AOG ON AOG.id = tao.groupId
                                                WHERE tao.closingDate Is Null " + whereCond, conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    AssigneeOperation oneAssignOperation = new AssigneeOperation();

                    if (row != null)
                    {
                        oneAssignOperation.OperationType = ushort.Parse(row["opType"].ToString());
                        oneAssignOperation.OperationTypeDescription = Utility.ConvertAnsiToUnicode(row["opTypeDescr"].ToString());
                        oneAssignOperation.GroupId = ushort.Parse(row["Groupid"].ToString());
                        oneAssignOperation.OperationGroupTypeDescription = Utility.ConvertAnsiToUnicode(row["opGroupDescription"].ToString());
                        if (int.Parse(row["operationTypeId"].ToString()) == 0 && int.Parse(row["opType"].ToString()) != 16 && int.Parse(row["opType"].ToString()) != 17)
                        {
                            oneAssignOperation.AllAccounts = false;
                        }
                        else
                        {
                            oneAssignOperation.AllAccounts = true;
                        }
                    }

                    operationList.Add(oneAssignOperation);
                }
            }
            return operationList;
        }

        /// <summary>
        /// GET CREDENTIAL ORDER DETAILS
        /// </summary>
        internal static Credential GetCredentialOrderDetails(ulong docId, SourceType source)
        {
            Credential oneCredential = new Credential();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT A.doc_Id, A.credential_number, A.start_date, A.end_date, A.credential_type, T.description FROM [HBBase].[dbo].Tbl_credential_order_details A 
                                                INNER JOIN Tbl_type_of_assigns T ON A.credential_type = T.id
                                                WHERE doc_id =@docId", conn);

                cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                DataRow row = dt.Rows[0];

                if (row != null)
                {
                    oneCredential.Id = ulong.Parse(row["doc_Id"].ToString());
                    oneCredential.CredentialNumber = row["credential_number"].ToString();
                    oneCredential.StartDate = DateTime.Parse(row["start_date"].ToString());
                    oneCredential.EndDate = row["end_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["end_date"].ToString());
                    oneCredential.CredentialType = ushort.Parse(row["credential_type"].ToString());
                    oneCredential.CredentialTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());

                    if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                    {
                        oneCredential.AssigneeList = GetCredentialOrderAssigneesAcbaOnline(docId);
                    }
                    else
                    {
                        oneCredential.AssigneeList = GetCredentialOrderAssignees(docId);
                    }

                }
            }

            return oneCredential;
        }

        /// <summary>
        /// GET CREDENTIAL ORDER ASIGNEE DETAILS
        /// </summary>
        internal static List<Assignee> GetCredentialOrderAssignees(ulong docId)
        {
            List<Assignee> assigneeList = new List<Assignee>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT A.id, A.assignee_customer_number, A.signature_type, A.is_employee, isnull(F.unicode_name,'') as Name, isnull(F.unicode_Lastname,'') as LastName, 
                                                isnull(F.unicode_middleName,'') as MiddleName,
                                                D.passport_inf as GivenBy, D.passport_date as GivenDate, D.passport_type as DocType, D.Passport_Number as DocNumber                                                  
                                                FROM [HBBase].[dbo].Tbl_assignees_details A
                                                LEFT JOIN tbl_Customers C ON A.assignee_customer_number = C.Customer_number
												INNER JOIN Tbl_Persons P On C.IdentityId = P.identityId 
                                                INNER JOIN V_FullNames F On F.id = p.fullNameId
                                                INNER JOIN [V_CustomerDescriptionIndividualsDocs] D on C.customer_number = D.customer_number 
                                                WHERE A.doc_id = @docId", conn);

                cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    assigneeList = new List<Assignee>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Assignee oneAssignee = new Assignee();

                    if (row != null)
                    {
                        oneAssignee.Id = uint.Parse(row["id"].ToString());
                        oneAssignee.CustomerNumber = ulong.Parse(row["assignee_customer_number"].ToString());
                        oneAssignee.SignatureType = ushort.Parse(row["signature_type"].ToString());
                        oneAssignee.IsEmployee = ushort.Parse(row["is_employee"].ToString());
                        oneAssignee.AssigneeFirstName = row["Name"].ToString();
                        oneAssignee.AssigneeLastName = row["LastName"].ToString();
                        oneAssignee.AssigneeMiddleName = row["MiddleName"].ToString();

                        oneAssignee.AssigneeDocumentGivenBy = row["GivenBy"].ToString();
                        oneAssignee.AssigneeDocumentGivenDate = DateTime.Parse(row["GivenDate"].ToString());
                        oneAssignee.AssigneeDocumentType = Convert.ToInt32(row["DocType"].ToString());
                        oneAssignee.AssigneeDocumentNumber = row["DocNumber"].ToString();

                        oneAssignee.OperationList = GetOrderAssigneeOperations(oneAssignee.Id);

                    }

                    assigneeList.Add(oneAssignee);
                }

            }

            return assigneeList;
        }


        /// <summary>
        /// GET CREDENTIAL ORDER ASIGNEE DETAILS
        /// </summary>
        internal static List<Assignee> GetCredentialOrderAssigneesAcbaOnline(ulong docId)
        {
            List<Assignee> assigneeList = new List<Assignee>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT A.id, A.assignee_customer_number, A.signature_type, A.is_employee, dbo.fnc_convertAnsiToUnicode(assignee_first_name) as Name, dbo.fnc_convertAnsiToUnicode(assignee_last_name) as LastName, 
                                                dbo.fnc_convertAnsiToUnicode(assignee_middle_name) as MiddleName,
                                                A.assignee_document_given_by as GivenBy, A.assignee_document_given_date as GivenDate, A.assignee_document_type as DocType, A.assignee_document_number as DocNumber                                                  
                                                FROM Tbl_assignees_details A                               
                                             
                                                WHERE A.doc_id = @docId", conn);

                cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    assigneeList = new List<Assignee>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Assignee oneAssignee = new Assignee();

                    if (row != null)
                    {
                        oneAssignee.Id = uint.Parse(row["id"].ToString());
                        oneAssignee.CustomerNumber = ulong.Parse(row["assignee_customer_number"].ToString());
                        oneAssignee.SignatureType = ushort.Parse(row["signature_type"].ToString());
                        oneAssignee.IsEmployee = ushort.Parse(row["is_employee"].ToString());

                        if (row["Name"] != DBNull.Value)
                            oneAssignee.AssigneeFirstName = row["Name"].ToString();

                        if (row["LastName"] != DBNull.Value)
                            oneAssignee.AssigneeLastName = row["LastName"].ToString();

                        if (row["MiddleName"] != DBNull.Value)
                            oneAssignee.AssigneeMiddleName = row["MiddleName"].ToString();

                        if (row["GivenBy"] != DBNull.Value)
                            oneAssignee.AssigneeDocumentGivenBy = row["GivenBy"].ToString();

                        if (row["GivenDate"] != DBNull.Value)
                            oneAssignee.AssigneeDocumentGivenDate = DateTime.Parse(row["GivenDate"].ToString());

                        if (row["DocType"] != DBNull.Value)
                            oneAssignee.AssigneeDocumentType = Convert.ToInt32(row["DocType"].ToString());

                        if (row["DocNumber"] != DBNull.Value)
                            oneAssignee.AssigneeDocumentNumber = row["DocNumber"].ToString();

                        oneAssignee.OperationList = GetOrderAssigneeOperations(oneAssignee.Id);

                    }

                    assigneeList.Add(oneAssignee);
                }

            }

            return assigneeList;
        }

        /// <summary>
        /// GET ORDER ASIGNEE OPERATIONS DETAILS
        /// </summary>
        internal static List<AssigneeOperation> GetOrderAssigneeOperations(uint assigneeId)
        {
            List<AssigneeOperation> availableOperationsList = new List<AssigneeOperation>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT op.id, op.all_account, op.type,t.Description, t.GroupId, G.Description GroupDescription FROM [HBBase].[dbo].Tbl_assignee_operations op 
                                                INNER JOIN Tbl_type_of_assign_operation t on op.type=t.ID 
                                                INNER JOIN Tbl_Type_of_assign_operations_groups G ON t.GroupId = G.Id                                             
                                                WHERE assignee_id=@assigneeId", conn);

                cmd.Parameters.Add("@assigneeId", SqlDbType.Int).Value = assigneeId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    availableOperationsList = new List<AssigneeOperation>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    AssigneeOperation oneOperation = new AssigneeOperation();

                    if (row != null)
                    {
                        oneOperation.Id = uint.Parse(row["id"].ToString());
                        oneOperation.AllAccounts = bool.Parse(row["all_account"].ToString());
                        oneOperation.OperationType = ushort.Parse(row["type"].ToString());
                        oneOperation.OperationTypeDescription = Utility.ConvertAnsiToUnicode(row["Description"].ToString());
                        oneOperation.GroupId = ushort.Parse(row["GroupId"].ToString());
                        oneOperation.OperationGroupTypeDescription = Utility.ConvertAnsiToUnicode(row["GroupDescription"].ToString());
                        oneOperation.AccountList = GetOrderOperationAccounts(oneOperation.Id);
                    }

                    availableOperationsList.Add(oneOperation);
                }

            }

            return availableOperationsList;
        }

        /// <summary>
        /// GET ORDER ASIGNEE OPERATION ACCOUNTS DETAILS
        /// </summary>
        internal static List<Account> GetOrderOperationAccounts(uint operationId)
        {
            List<Account> accountsList = new List<Account>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT op.account_number,acc.type_of_account_new,acc.currency  FROM [HBBase].[dbo].[Tbl_assignee_operation_accounts] op 
					                                                                                INNER JOIN [tbl_all_accounts;] acc on op.account_number=acc.Arm_number  
			                                                                                WHERE operation_id=@operationId", conn);

                cmd.Parameters.Add("@operationId", SqlDbType.Int).Value = operationId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    accountsList = new List<Account>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Account oneAccount = new Account();

                    if (row != null)
                    {
                        oneAccount.AccountNumber = row["account_number"].ToString();
                        oneAccount.Currency = row["currency"].ToString();
                    }

                    accountsList.Add(oneAccount);
                }

            }

            return accountsList;
        }

        internal static ActionResult SaveCredentialOrderDetails(CredentialOrder credentialOrder, long orderId, Action actionType = Action.Add)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_credential_order_details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@credentialNumber", SqlDbType.NVarChar, 50).Value = (object)credentialOrder.Credential.CredentialNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = credentialOrder.CustomerNumber;
                    cmd.Parameters.Add("@startDate", SqlDbType.DateTime).Value = (object)credentialOrder.Credential.StartDate ?? DBNull.Value;
                    cmd.Parameters.Add("@endDate", SqlDbType.DateTime).Value = (object)credentialOrder.Credential.EndDate ?? DBNull.Value;
                    cmd.Parameters.Add("@credentialType", SqlDbType.TinyInt).Value = credentialOrder.Credential.CredentialType;
                    cmd.Parameters.Add("@status", SqlDbType.TinyInt).Value = credentialOrder.Credential.Status;
                    cmd.Parameters.Add("@givenByBank", SqlDbType.Bit).Value = credentialOrder.Credential.GivenByBank;
                    cmd.Parameters.Add("@hasNotarAuthorization", SqlDbType.Bit).Value = (object)credentialOrder.Credential.HasNotarAuthorization ?? DBNull.Value;
                    cmd.Parameters.Add("@credentialGivenDate", SqlDbType.DateTime).Value = (object)credentialOrder.Credential.CredentialGivenDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ledgerNumber", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.LedgerNumber) ?? DBNull.Value;
                    cmd.Parameters.Add("@notary", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.Notary) ?? DBNull.Value;
                    cmd.Parameters.Add("@notaryTerritory", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.NotaryTerritory) ?? DBNull.Value;
                    cmd.Parameters.Add("@translationValidationDate", SqlDbType.DateTime).Value = (object)credentialOrder.Credential.TranslationValidationDate ?? DBNull.Value;
                    cmd.Parameters.Add("@translationOfNotaryTerritory", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.TranslationOfNotaryTerritory) ?? DBNull.Value;
                    cmd.Parameters.Add("@translationOfNotary", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.TranslationOfNotary) ?? DBNull.Value;
                    cmd.Parameters.Add("@translationValidationLedgerNumber", SqlDbType.NVarChar).Value = (object)Utility.ConvertUnicodeToAnsi(credentialOrder.Credential.TranslationValidationLedgerNumber) ?? DBNull.Value;

                    cmd.Parameters.Add("@action_type", SqlDbType.TinyInt).Value = (short)actionType;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static ActionResult SaveAssigneesDetails(Assignee assignee, long orderId, Action actionType = Action.Add)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_assignees_details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@assigneeCustomerNumber", SqlDbType.Float).Value = assignee.CustomerNumber;
                    cmd.Parameters.Add("@signatureType", SqlDbType.TinyInt).Value = assignee.SignatureType;
                    cmd.Parameters.Add("@isEmployee", SqlDbType.TinyInt).Value = assignee.IsEmployee;
                    cmd.Parameters.Add("@assigneeFirstName", SqlDbType.NVarChar, 50).Value = (object)assignee.AssigneeFirstName ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeLastName", SqlDbType.NVarChar, 50).Value = (object)assignee.AssigneeLastName ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeMiddleName", SqlDbType.NVarChar, 50).Value = (object)assignee.AssigneeMiddleName ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeDocumentNumber", SqlDbType.NVarChar, 50).Value = (object)assignee.AssigneeDocumentNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeDocumentType", SqlDbType.Int).Value = (object)assignee.AssigneeDocumentType ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeDocumentGivenDate", SqlDbType.DateTime).Value = (object)assignee.AssigneeDocumentGivenDate ?? DBNull.Value;
                    cmd.Parameters.Add("@assigneeDocumentGivenBy", SqlDbType.NVarChar, 50).Value = (object)assignee.AssigneeDocumentGivenBy ?? DBNull.Value;
                    cmd.Parameters.Add("@action_type", SqlDbType.TinyInt).Value = (short)actionType;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static ActionResult SaveAssigneeOperationsDetails(AssigneeOperation assigneeOperation, ulong assigneeId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_assignee_operations([assignee_id], [type], [all_account])
                                        VALUES(@assigneeId, @type, @allAccount);SELECT @id = Scope_identity()";

                    cmd.Parameters.Add("@assigneeId", SqlDbType.Int).Value = assigneeId;
                    cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = assigneeOperation.OperationType;
                    cmd.Parameters.Add("@allAccount", SqlDbType.Bit).Value = assigneeOperation.AllAccounts;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static ActionResult SaveAssigneeOperationAccountsDetails(ulong accountNumber, uint operationId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_assignee_operation_accounts([operation_id], [account_number])
                                        VALUES(@operationId, @accountNumber);SELECT @id = Scope_identity()";

                    cmd.Parameters.Add("@operationId", SqlDbType.Int).Value = operationId;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = accountNumber;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        /// <summary>
        /// Հաճախորդի լիազորագրի փակում/հեռացում պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveCredentialTerminationOrder(CredentialTerminationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_terminate_credential";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@assignId", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@doc_type", SqlDbType.TinyInt).Value = order.Type;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@operationDate", SqlDbType.DateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@closingReason", SqlDbType.SmallInt).Value = order.ClosingReasonType;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;
                    order.Quality = OrderQuality.Draft;

                }
                return result;
            }
        }

        internal static ActionResult SaveCredentialTerminationOrderDetails(CredentialTerminationOrder credentialTerminationOrder, long docId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_credential_termination_details(doc_id, closing_reason, product_id) VALUES(@docId, @closingReason, @productID)";

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                    cmd.Parameters.Add("@productID", SqlDbType.BigInt).Value = credentialTerminationOrder.ProductId;
                    cmd.Parameters.Add("@closingReason", SqlDbType.Int).Value = credentialTerminationOrder.ClosingReasonType;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static Int16 CheckCredentialTodos(ulong assignId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                Int16 result = 0;
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_CheckCrdentialTodos";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@assignId", SqlDbType.Float).Value = assignId;
                    cmd.Parameters.Add(new SqlParameter("@prResult", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result = Convert.ToInt16(cmd.Parameters["@prResult"].Value);
                }

                return result;
            }
        }


        internal static ActionResult SaveCredentialActivationOrder(CredentialActivationOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();

            using SqlCommand cmd = new SqlCommand(@"	DECLARE @filial AS int
                                                    DECLARE @doc_ID AS int
                                                    SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customer_number
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,amount,currency,debet_account,credit_account,
                                                    [description],quality,
                                                    source_type,operationFilialCode,operation_date)
                                                    VALUES
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,@amount,@currency,
                                                    @debit_acc,@credit_acc,@descr,1,@source_type,@operationFilialCode,@oper_day)
                                                    SELECT @doc_ID= Scope_identity()
                                                    INSERT INTO Tbl_HB_Products_Identity(HB_Doc_ID,App_ID)
                                                    VALUES (@doc_ID,@app_ID)                                                  
                                                    SELECT @doc_ID AS ID", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@app_ID", SqlDbType.Float).Value = order.Credential.Id;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
            if (order.Credential.GivenByBank)
            {

                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = order.ReceiverAccount.AccountNumber;
            }
            else
            {
                cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = DBNull.Value;
                cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = DBNull.Value;
                cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar, 20).Value = DBNull.Value;
            }


            cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description == null ? "" : order.Description;


            cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;

            cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)order.Source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        internal static CredentialActivationOrder GetCredentialActivationOrder(CredentialActivationOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT  
                                            filial,customer_number,registration_date,document_type,
                                            document_number,document_subtype,amount,currency,debet_account,credit_account,
                                            [description],quality,
                                            source_type,operationFilialCode,operation_date
                                            FROM Tbl_HB_documents 
                                            WHERE doc_ID=@docID AND customer_number=case WHEN @customer_number = 0 THEN customer_number ELSE @customer_number END";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();


                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.Amount = double.Parse(dr["amount"].ToString());
                    order.Currency = dr["currency"].ToString();
                    if (dr["debet_account"] != DBNull.Value)
                    {
                        if (order.Type == OrderType.CredentialActivationOrder)
                        {
                            order.DebitAccount = Account.GetAccount(dr["debet_account"].ToString());
                        }
                        else
                        {
                            order.DebitAccount = Account.GetSystemAccount(dr["debet_account"].ToString());
                        }
                    }

                }


            }
            return order;
        }


        internal static ulong GetNextCredentialDocumentNumber(uint filialCode)
        {
            ulong result = 1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT ISNULL(MAX(assign_number),0)+1 AS documentNumber FROM 
                                               Tbl_assigns WHERE ISNUMERIC(assign_number) = 1 
                                               AND filialcode=@filialcode AND registration_date>=@operDay";

                    cmd.Parameters.Add("@filialcode", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    result = Convert.ToUInt64(cmd.ExecuteScalar());
                    return result;
                }
            }
        }
        /// <summary>
        /// Լիազորված անձի նույնականացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult SaveAssigneeIdentificationOrder(AssigneeIdentificationOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();

            using SqlCommand cmd = new SqlCommand(@"	DECLARE @filial AS int
                                                    DECLARE @doc_ID AS int
                                                    SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customer_number
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,
                                                    quality,
                                                    source_type,operationFilialCode,operation_date)
                                                    VALUES
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@document_subtype,
                                                    1,@source_type,@operationFilialCode,@oper_day)
                                                    SELECT @doc_ID= Scope_identity()
                                                    INSERT INTO TBl_Assignee_Identification(doc_ID,assignee_id, assignee_customer_number)
                                                    VALUES (@doc_ID,@assignee_id, @assignee_customer_number)                                                  
                                                    SELECT @doc_ID AS ID", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@app_ID", SqlDbType.Float).Value = order.Credential.Id;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (short)order.Type;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
            cmd.Parameters.Add("@username", SqlDbType.VarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)order.Source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

            cmd.Parameters.Add("@assignee_id", SqlDbType.Float).Value = order.Credential.AssigneeList[0].Id;

            cmd.Parameters.Add("@assignee_customer_number", SqlDbType.Float).Value = order.Credential.AssigneeList[0].CustomerNumber;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());

            order.Quality = OrderQuality.Draft;

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        //internal static ulong GetOrderId(long id)
        //{

        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
        //    {
        //        using (SqlCommand cmd = new SqlCommand())
        //        {
        //            ulong orderId;

        //            conn.Open();
        //            cmd.Connection = conn;
        //            cmd.CommandText = @"SELECT order_id FROM Tbl_link_HB_document_order WHERE document_id = @document_id";

        //            cmd.Parameters.Add("@document_id", SqlDbType.BigInt).Value = id;
        //            orderId = Convert.ToUInt64(cmd.ExecuteScalar());

        //            return orderId;
        //        }
        //    }
        //}
        internal static void RemoveCredentialOrderBODetails(long orderId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_remove_credential_order_BO_details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static void RemoveCredentialOrderDetails(long orderId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_remove_credential_order_details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }

}