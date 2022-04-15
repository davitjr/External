using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class AccountOrderDB
    {
        /// <summary>
        /// Հաշվի բացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(AccountOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewAccountDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;

                    KeyValuePair<uint, uint> res = AccountOrder.GetRestrictionType(order.RestrictionGroup);

                    cmd.Parameters.Add("@type_of_product", SqlDbType.SmallInt).Value = res.Key;
                    cmd.Parameters.Add("@type_of_account", SqlDbType.SmallInt).Value = res.Value;

                    if (order.RestrictionGroup == 1)
                    {
                        cmd.Parameters.Add("@bankruptcy_manager", SqlDbType.Float).Value = order.BankruptcyManager;
                    }


                    if (order.Type == OrderType.CurrentAccountReOpen)
                    {
                        AccountReOpenOrder reOpenOrder = (AccountReOpenOrder)order;

                        cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = reOpenOrder.ReOpeningAccounts[0].AccountNumber;
                        cmd.Parameters.Add("@fee_charge_type", SqlDbType.SmallInt).Value = reOpenOrder.FeeChargeType;
                        cmd.Parameters.Add("@reopen_reason", SqlDbType.NVarChar, 255).Value = reOpenOrder.ReopenReasonDescription;

                        order.JointCustomers = new List<KeyValuePair<ulong, string>>();
                    }
                    else
                    {
                        cmd.Parameters.Add("@debet_account", SqlDbType.VarChar, 50).Value = 0;
                    }


                    cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;

                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }



                    if (order.RestrictionGroup == 2)
                    {
                        cmd.Parameters.Add("@account_status", SqlDbType.Int).Value = 10;

                    }
                    else
                    {
                        cmd.Parameters.Add("@account_status", SqlDbType.Int).Value = order.AccountStatus;

                    }

                    List<KeyValuePair<ulong, string>> jointCustomerList = new List<KeyValuePair<ulong, string>>();
                    if (order.JointCustomers != null)
                    {
                        jointCustomerList.AddRange(order.JointCustomers);
                    }
                    if (order.AccountType == 2 || order.AccountType == 3)
                    {
                        KeyValuePair<ulong, string> oneJointCustomer = jointCustomerList.FindLast(m => m.Key != 0);

                        cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = oneJointCustomer.Key;
                        cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = oneJointCustomer.Value;

                        jointCustomerList.Remove(oneJointCustomer);

                    }
                    else
                    {
                        cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = DBNull.Value;
                    }

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0 || actionResult == 8 || actionResult == 229 || actionResult == 227 || actionResult == 230)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors = new List<ActionError>
                        {
                            new ActionError((short)actionResult)
                        };
                    }

                    if (result.ResultCode == ResultCode.Normal && order.AccountType == 2 && order.Type != OrderType.CurrentAccountReOpen && jointCustomerList.Count > 0)
                    {

                        //Հետագայում փոխվելու է աղյուսակի կառուցվածքը
                        //Ներկա պահին կարող է լինել max 3 հատատեղ հաճախորդ
                        string jointCustomerInsertString = @"UPDATE Tbl_new_account_documents
                                                            set tp_customer_number2 = @tp_customer_number,
                                                                tp_description2 = @tp_description
                                                            WHERE doc_id=@doc_id";


                        jointCustomerList.ForEach(m =>
                        {
                            //Մի քանի համատեղ հաճախորդներ
                            cmd.Parameters.Clear();
                            cmd.CommandText = jointCustomerInsertString;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                            cmd.Parameters.Add("@account_type", SqlDbType.Int).Value = order.AccountType;
                            cmd.Parameters.Add("@tp_customer_number", SqlDbType.Float).Value = m.Key;
                            cmd.Parameters.Add("@tp_description", SqlDbType.VarChar, 50).Value = m.Value;

                            cmd.ExecuteNonQuery();

                        });
                    }

                    if (order.Type == OrderType.CurrentAccountReOpen)
                    {
                        AccountReOpenOrder reOpenOrder = (AccountReOpenOrder)order;
                        string accountsInsertString = @"INSERT INTO Tbl_account_order_selected_accounts(Doc_ID, account_number) VALUES(@Doc_ID, @account_number)";

                        for (int i = 1; i < reOpenOrder.ReOpeningAccounts.Count; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = accountsInsertString;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                            cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = reOpenOrder.ReOpeningAccounts[i].AccountNumber;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return result;
                }

            }
        }
        /// <summary>
        /// Վերադարձնում է հաշվի բացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static AccountOrder GetAccountOrder(AccountOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select d.amount, d.currency, d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                                 isnull(n.tp_description,'') tp_description,isnull(n.tp_customer_number,0) tp_customer_number,isnull(n.account_type,1) account_type,
                                                         isnull(n.tp_description2,'') tp_description2,isnull(n.tp_customer_number2,0) tp_customer_number2,isnull(statement_type,-1)  as statement_type,operation_date,d.order_group_id, d.confirmation_date 
                                                  from Tbl_HB_documents as d left join Tbl_New_Account_Documents as n on  d.doc_ID=n.Doc_ID
                                                  where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end",
                    conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.AccountType = Convert.ToUInt16(dt.Rows[0]["account_type"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);

                    if (Convert.ToInt16(dt.Rows[0]["statement_type"]) != -1)
                    {
                        order.StatementDeliveryType = short.Parse(dt.Rows[0]["statement_type"].ToString());
                    }
                    else
                    {
                        order.StatementDeliveryType = null;
                    }


                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);


                    List<KeyValuePair<ulong, string>> jointList = new List<KeyValuePair<ulong, string>>();

                    if (ulong.Parse(dt.Rows[0]["tp_customer_number"].ToString()) != 0)
                    {
                        jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description"].ToString())));
                    }

                    if (ulong.Parse(dt.Rows[0]["tp_customer_number2"].ToString()) != 0)
                    {
                        jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number2"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description2"].ToString())));
                    }

                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

                    order.JointCustomers = jointList;
                }
            }
            return order;
        }
        /// <summary>
        /// Վերադարձնում է Սահմանափակ հասանելիության հաշիվը
        /// </summary>
        internal static AccountOrder GetRestrictedAccountOrder(ulong customerNumber, OrderType documentType)
        {
            AccountOrder order = null;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select Top 1 d.doc_ID,d.amount, d.currency, d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		              isnull(n.tp_description,'') tp_description,isnull(n.tp_customer_number,0) tp_customer_number,isnull(n.account_type,1) account_type,
                      isnull(n.tp_description2,'') tp_description2,isnull(n.tp_customer_number2,0) tp_customer_number2,isnull(statement_type,-1)  as statement_type,operation_date,d.order_group_id 
                      from Tbl_HB_documents as d left join Tbl_New_Account_Documents as n on  d.doc_ID=n.Doc_ID
                      where  type_of_product = 18 and  type_of_account = 283 and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.document_type=@documentType order by d.doc_ID desc",
                    conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = (int)documentType;
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows?.Count > 0)
                    {
                        order = new AccountOrder();
                        order.Id = Convert.ToUInt32(dt.Rows[0]["doc_ID"]);
                        order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                        order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                        order.Currency = dt.Rows[0]["currency"].ToString();
                        order.AccountType = Convert.ToUInt16(dt.Rows[0]["account_type"]);
                        order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                        order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                        order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                        order.StatementDeliveryType = short.Parse(dt.Rows[0]["statement_type"].ToString());
                        order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                        order.RestrictionGroup = 18;
                        List<KeyValuePair<ulong, string>> jointList = new List<KeyValuePair<ulong, string>>();

                        if (ulong.Parse(dt.Rows[0]["tp_customer_number"].ToString()) != 0)
                        {
                            jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description"].ToString())));
                        }

                        if (ulong.Parse(dt.Rows[0]["tp_customer_number2"].ToString()) != 0)
                        {
                            jointList.Add(new KeyValuePair<ulong, string>(key: ulong.Parse(dt.Rows[0]["tp_customer_number2"].ToString()), value: Utility.ConvertAnsiToUnicode(dt.Rows[0]["tp_description2"].ToString())));
                        }

                        order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

                        order.JointCustomers = jointList;
                    }
                }
            }
            return order;
        }
        internal static ActionResult SaveAccountOrderDetails(AccountOrder accountOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_account_order_details(order_id, currency, account_type, statement_type, account_number, reopen_reason) 
                                        VALUES(@orderId, @currency, @accountType, @statementType, @accountNumber, @reopenReason)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = accountOrder.Currency;
                    cmd.Parameters.Add("@accountType", SqlDbType.SmallInt).Value = accountOrder.AccountType;
                    cmd.Parameters.Add("@statementType", SqlDbType.SmallInt).Value = accountOrder.StatementDeliveryType;

                    if (accountOrder.Type == OrderType.CurrentAccountReOpen)
                    {
                        AccountReOpenOrder reOpenOrder = (AccountReOpenOrder)accountOrder;

                        cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = reOpenOrder.ReOpeningAccounts[0].AccountNumber;
                        cmd.Parameters.Add("@reopenReason", SqlDbType.NVarChar, 255).Value = reOpenOrder.ReopenReasonDescription;
                    }
                    else
                    {
                        cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = DBNull.Value;
                        cmd.Parameters.Add("@reopenReason", SqlDbType.NVarChar, 255).Value = DBNull.Value;
                    }
                    cmd.ExecuteNonQuery();



                    if (accountOrder.Type == OrderType.CurrentAccountReOpen)
                    {
                        AccountReOpenOrder reOpenOrder = (AccountReOpenOrder)accountOrder;
                        string accountsInsertString = @"INSERT INTO Tbl_BO_account_order_details(order_id, currency, account_type, statement_type, account_number, reopen_reason) 
                                                        VALUES(@orderId, @currency, @accountType, @statementType, @accountNumber, @reopenReason)";
                        for (int i = 1; i < reOpenOrder.ReOpeningAccounts.Count; i++)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = accountsInsertString;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                            cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = reOpenOrder.ReOpeningAccounts[i].Currency;
                            cmd.Parameters.Add("@accountType", SqlDbType.SmallInt).Value = accountOrder.AccountType;
                            cmd.Parameters.Add("@statementType", SqlDbType.SmallInt).Value = accountOrder.StatementDeliveryType;

                            cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = reOpenOrder.ReOpeningAccounts[i].AccountNumber;
                            cmd.Parameters.Add("@reopenReason", SqlDbType.NVarChar, 255).Value = reOpenOrder.ReopenReasonDescription;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        internal static byte[] GetOpenedAccountContract(string accountNumber)
        {
            int docid = 0;
            short documentType = 0;
            byte sourceType = 0;
            int type = 0;

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 doc_id,document_type,source_type  
                                                         FROM Tbl_HB_documents where document_type in (7,12,17) and debet_account = @accountNumber  
                                                         AND quality = 30 ORDER BY doc_ID DESC", conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {

                        docid = Convert.ToInt32(dt.Rows[0]["doc_id"].ToString());
                        documentType = Convert.ToInt16(dt.Rows[0]["document_type"].ToString());
                        sourceType = Convert.ToByte(dt.Rows[0]["source_type"].ToString());
                    }
                    else
                    {
                        return null;
                    }


                }
            }

            if (documentType == 7 || documentType == 17)
            {
                type = 3;
            }
            else
            {
                type = 5;
            }

            return UploadedFile.GetAttachedFile(docid, type);

        }
    }
}
