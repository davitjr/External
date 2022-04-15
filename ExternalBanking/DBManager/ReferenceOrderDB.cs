using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    public class ReferenceOrderDB
    {
        /// <summary>
        /// Տեղեկանքի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(ReferenceOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            DataTable dtAcc = new DataTable();

            dtAcc.Columns.Add("type");
            dtAcc.Columns.Add("uID");
            dtAcc.Columns.Add("status");
            dtAcc.Columns.Add("acc_number");

            if (order.Source == SourceType.Bank)
            {
                if (!(order.ReferenceTypes.Count == 1 && order.ReferenceTypes.Contains(4)))
                {
                    dtAcc = new DataTable();
                    dtAcc = GetAccountDataTable(order.Accounts);
                }
            }
            else
            {
                if (order.ReferenceType != 4)
                {
                    if (order.IncludeCreditLine)
                    {
                        foreach (Account a in order.Accounts)
                        {
                            Account account = Account.GetAccount(a.AccountNumber);
                            if (account.AccountType == 11)
                            {
                                Card card = Card.GetCard(account);
                                if (card.CreditLine != null)
                                {
                                    order.IncludeCreditLine = true;
                                    break;
                                }
                                else
                                {
                                    order.IncludeCreditLine = false;
                                }

                            }
                        }
                    }
                    dtAcc = new DataTable();
                    dtAcc = GetAccountDataTable(order.Accounts);
                }
            }

            DataTable dtRef = new DataTable();
            dtRef = GetReferenceTypeDataTable(order.ReferenceTypes);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewReferenceDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    if (order.Source != SourceType.Bank)
                        cmd.Parameters.Add("@Reference_type", SqlDbType.TinyInt).Value = order.ReferenceType;
                    else
                    {
                        cmd.Parameters.Add("@type_of_reference_receipt", SqlDbType.TinyInt).Value = order.ReferenceReceiptType;
                        cmd.Parameters.Add("@passport", SqlDbType.NVarChar, 50).Value = order.PassportDetails;
                        cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar, 50).Value = order.PhoneNumber;
                        cmd.Parameters.Add("@full_delivery_address", SqlDbType.NVarChar, 100).Value = order.FullDeliveryAddress;
                    }
                    cmd.Parameters.Add("@Reference_embassy", SqlDbType.TinyInt).Value = order.ReferenceEmbasy;
                    cmd.Parameters.Add("@Reference_language", SqlDbType.TinyInt).Value = order.ReferenceLanguage;
                    cmd.Parameters.Add("@Reference_filial", SqlDbType.Int).Value = order.ReferenceFilial;
                    cmd.Parameters.Add("@urgent", SqlDbType.TinyInt).Value = order.Urgent;
                    cmd.Parameters.Add("@service_fee_account", SqlDbType.Float).Value = order.FeeAccount != null ? order.FeeAccount.AccountNumber : "0";
                    cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.FeeAmount;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.Source == SourceType.Bank)
                    {
                        if (order.ReferenceTypes.Contains(3))
                        {
                            cmd.Parameters.Add("@date_from", SqlDbType.DateTime).Value = order.DateFrom;
                            cmd.Parameters.Add("@date_to", SqlDbType.DateTime).Value = order.DateTo;
                        }
                    }
                    else
                    {
                        if (order.ReferenceType == 3)
                        {
                            cmd.Parameters.Add("@date_from", SqlDbType.DateTime).Value = order.DateFrom;
                            cmd.Parameters.Add("@date_to", SqlDbType.DateTime).Value = order.DateTo;
                        }
                        if (order.IncludeCreditLine == true)
                        {
                            cmd.Parameters.Add("@include_crLines", SqlDbType.TinyInt).Value = 1;
                        }

                    }

                    if (order.ReferenceEmbasy == 27)
                    {
                        cmd.Parameters.Add("@Reference_for", SqlDbType.NVarChar, 50).Value = order.ReferenceFor;
                    }


                    if (order.ReferenceTypes.Contains(8))
                    {
                        cmd.Parameters.Add("@other_ref_type_desc", SqlDbType.NVarChar, 100).Value = order.OtherTypeDescription;
                    }

                    if (!String.IsNullOrWhiteSpace(order.OtherRequisites))
                    {
                        cmd.Parameters.Add("@other_requisites", SqlDbType.NVarChar, 100).Value = order.OtherRequisites;
                    }

                    cmd.Parameters.Add("@receive_date", SqlDbType.DateTime).Value = order.ReceiveDate;


                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    SqlParameter prm = new SqlParameter("@dtaccp", SqlDbType.Structured);
                    prm.Value = dtAcc;
                    prm.TypeName = "dbo.Reference_accounts";
                    cmd.Parameters.Add(prm);

                    SqlParameter references = new SqlParameter("@dtrefp", SqlDbType.Structured);
                    references.Value = dtRef;
                    references.TypeName = "dbo.Reference_types";
                    cmd.Parameters.Add(references);

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
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                }
            }

            return result;
        }
        /// <summary>
        /// Վերադարձնում է տեղեկանքի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static ReferenceOrder Get(ReferenceOrder order)
        {

            using DataTable dt = new DataTable();
            order.Accounts = new List<Account>();
            order.ReferenceTypes = new List<ushort>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT d.registration_date,
                                                         d.document_type,
                                                         d.document_number,
                                                         d.amount,
                                                         d.debet_account,
                                                         d.urgent,
                                                         d.quality,
                                                         d.document_subtype,   
                                                         ac.reference_acc_type,
                                                         ac.Reference_account AS Reference_account,
                                                         r.Reference_embassy,
                                                         r.Reference_language,
                                                         r.service_fee_account,
                                                         r.Reference_for,
                                                         r.Date_from,
                                                         r.Date_to,
                                                         r.Include_crLines,
                                                         r.Reference_filial,
                                                         r.other_ref_type_desc,
                                                         r.other_requisites,
                                                         ref.Reference_type AS Reference_type,
                                                         d.operation_date,
                                                         r.receive_date,
                                                         d.order_group_id,
                                                         d.confirmation_date,
                                                         r.passport,
                                                         r.phone_number,
                                                         r.full_delivery_address,
                                                         ref.type_of_reference_receipt
                                                         FROM Tbl_HB_documents AS d 
                                                         INNER JOIN Tbl_New_reference_Documents AS r
                                                         ON d.doc_ID=r.Doc_ID
                                                         LEFT JOIN tbl_RO_selected_accounts AS ac
                                                         ON d.doc_ID=ac.Doc_ID
                                                         INNER JOIN tbl_RO_selected_references AS ref
                                                         ON d.doc_ID=ref.Doc_ID
                                                         WHERE d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end AND d.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.FeeAmount = Convert.ToDouble(dt.Rows[0]["amount"].ToString());
                    order.FeeAccount = Account.GetAccount(Convert.ToUInt64(dt.Rows[0]["service_fee_account"]));
                    if (order.FeeAccount != null)
                    {
                        order.FeeAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.FeeAccount.AccountTypeDescription);
                    }
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    ushort refType = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        refType = Convert.ToUInt16(dt.Rows[i]["Reference_type"]);
                        if (!order.ReferenceTypes.Contains(refType))
                            order.ReferenceTypes.Add(refType);
                    }
                    order.OtherTypeDescription = dt.Rows[0]["other_ref_type_desc"] == DBNull.Value ? null : dt.Rows[0]["other_ref_type_desc"].ToString();
                    order.OtherRequisites = dt.Rows[0]["other_requisites"] == DBNull.Value ? null : dt.Rows[0]["other_requisites"].ToString();
                    order.ReferenceEmbasy = Convert.ToUInt16(dt.Rows[0]["Reference_embassy"]);
                    if (order.ReferenceEmbasy == 27)
                    {
                        order.ReferenceFor = Utility.ConvertAnsiToUnicode(dt.Rows[0]["Reference_for"].ToString());
                    }

                    List<string> accNums = new List<string>();
                    if (!(order.ReferenceTypes.Count == 1 && order.ReferenceTypes.Contains(4)))
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["Reference_account"] != DBNull.Value)
                            {
                                Account account = Account.GetAccount(Convert.ToUInt64(dt.Rows[i]["Reference_account"]));
                                account.AccountTypeDescription = Utility.ConvertAnsiToUnicode(account.AccountTypeDescription);
                                if (!accNums.Contains(account.AccountNumber))
                                {
                                    accNums.Add(account.AccountNumber);
                                    order.Accounts.Add(account);
                                }
                            }
                        }
                    }

                    order.ReferenceType = order.ReferenceTypes[0];
                    if (dt.Rows[0]["receive_date"] != DBNull.Value)
                    {
                        order.ReceiveDate = Convert.ToDateTime(dt.Rows[0]["receive_date"]);
                    }

                    order.ReferenceFilial = Convert.ToInt32(dt.Rows[0]["Reference_filial"]);
                    order.ReferenceLanguage = Convert.ToUInt16(dt.Rows[0]["Reference_language"]);



                    order.DateFrom = dt.Rows[0]["Date_from"] != DBNull.Value ? DateTime.Parse(dt.Rows[0]["Date_from"].ToString()) : default(DateTime?); //Convert.ToDateTime(dt.Rows[0]["Date_from"]);
                    order.DateTo = dt.Rows[0]["Date_to"] != DBNull.Value ? DateTime.Parse(dt.Rows[0]["Date_to"].ToString()) : default(DateTime?);


                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Urgent = Convert.ToUInt16(dt.Rows[0]["urgent"]);
                    if (Convert.ToInt32(dt.Rows[0]["Include_crLines"]) == 1)
                    {
                        order.IncludeCreditLine = true;
                    }
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);


                    order.ReferenceReceiptType = dt.Rows[0]["type_of_reference_receipt"] != DBNull.Value ? (ReferenceReceiptTypes)Convert.ToInt32(dt.Rows[0]["type_of_reference_receipt"]) : ReferenceReceiptTypes.None;

                    if (order.ReferenceReceiptType == ReferenceReceiptTypes.DeliveryService)
                    {
                        order.PassportDetails = dt.Rows[0]["passport"].ToString();
                        order.PhoneNumber = dt.Rows[0]["phone_number"].ToString();
                        order.FullDeliveryAddress = dt.Rows[0]["full_delivery_address"].ToString();
                    }

                }
            }
            return order;
        }
        /// <summary>
        /// Որոշում է հաշվի տեսակը և ավելացնում Datatable-ի մեջ
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        internal static DataTable GetAccountDataTable(List<Account> account)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("type");
            dt.Columns.Add("uID");
            dt.Columns.Add("status");
            dt.Columns.Add("acc_number");

            int i = 1;
            if (account != null)
            {
                foreach (Account a in account)
                {
                    Account accounts = Account.GetAccount(a.AccountNumber);
                    a.AccountType = accounts.AccountType;
                    if (a.AccountType == 11)
                    {
                        dt.Rows.Add("3", i, "1", a.AccountNumber);
                        i++;
                    }
                    if (a.AccountType == 10)
                    {
                        dt.Rows.Add("1", i, "1", a.AccountNumber);
                        i++;
                    }
                    if (a.AccountType == 13)
                    {
                        dt.Rows.Add("2", i, "1", a.AccountNumber);
                        i++;
                    }

                }
            }
            return dt;
        }

        internal static DataTable GetReferenceTypeDataTable(List<ushort> referenceTypes)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Reference_type");
            dt.Columns.Add("status");
            if (referenceTypes != null)
            {
                foreach (ushort referenceType in referenceTypes)
                {
                    dt.Rows.Add(referenceType, "1");
                }
            }
            return dt;
        }
    }
}
