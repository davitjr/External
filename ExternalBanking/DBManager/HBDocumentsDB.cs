using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking;
using System.IO;
using xbs = ExternalBanking.ACBAServiceReference;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ServiceClient;

namespace ExternalBanking.DBManager
{
    public static class HBDocumentsDB
    {
        internal static List<HBDocuments> GetHBDocumentsList(HBDocumentFilters obj)
        {
            obj.StartDate = null;
            obj.EndDate = null;
            obj.OperationType = 2;
            obj.QualityType = 3;
            //   obj.BankCode = 22000;
            obj.OnlySelectedCustomer = false;
            obj.OnlyACBA = 0;

            List<HBDocuments> documents = GetSearchedHBDocuments(obj);

            return documents;
        }
        internal static List<HBDocuments> GetSearchedHBDocuments(HBDocumentFilters obj)
        {
            List<HBDocuments> documents = new List<HBDocuments>();
            double totalAmount = 0;
            int totalQuantity = 0;
            // obj.BankCode = 22000;

            if (obj.OperationType == null)
            {
                obj.OperationType = 0;
            }

            string filter = GetFilter(obj);


            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string current_sp = "pr_HB_documents_List";
                try
                {
                    _con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(current_sp, _con);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                        da.SelectCommand.CommandTimeout = 220;

                        da.SelectCommand.Parameters.Add("@WHERE_CONDITION_STRING", SqlDbType.NVarChar).Value = filter;
                        da.SelectCommand.Parameters.Add("@view_list", SqlDbType.TinyInt).Value = obj.OperationType;

                        da.SelectCommand.Parameters.Add("@totalAmount", SqlDbType.Float).Direction = ParameterDirection.Output;
                        da.SelectCommand.Parameters.Add("@totalQuantity", SqlDbType.Int).Direction = ParameterDirection.Output;

                        DataSet ds = new DataSet();

                        da.Fill(ds);

                        int rowCount = 0;

                        if (ds.Tables[0].Rows.Count != 0)
                        {
                            DataTable dt = ds.Tables[0];

                            if (dt != null)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    if (rowCount >= obj.firstRow && rowCount <= obj.LastGetRowCount)
                                    {
                                        if ((obj?.DebitAccountCurrencyType != null && obj.DebitAccountCurrencyType == AccountDB.GetAccountCurrency(row["debet_account"].ToString())) || obj.DebitAccountCurrencyType == null)
                                        {
                                            HBDocuments doc = new HBDocuments();
                                            if (Convert.ToString(row["doc_ID"]) != "")
                                            {
                                                doc.TransactionCode = Convert.ToInt32(row["doc_ID"]);
                                            }
                                            if (Convert.ToString(row["change_date"]) != "")
                                            {
                                                doc.TransactionDate = Convert.ToDateTime(row["change_date"]).ToString("dd/MM/yyyy HH:mm:ss");
                                            }
                                            if (Convert.ToString(row["filial"]) != "")
                                            {
                                                doc.FilialCode = Convert.ToInt32(row["filial"]);
                                            }
                                            if (Convert.ToString(row["customer_number"]) != "")
                                            {
                                                doc.CustomerNumber = Convert.ToInt64(row["customer_number"]);
                                            }
                                            if (Convert.ToString(row["document_type"]) != "")
                                            {
                                                doc.DocumentType = Convert.ToInt32(row["document_type"]);
                                                doc.Type = Convert.ToInt32(row["document_type"]);
                                            }
                                            if (Convert.ToString(row["document_subtype"]) != "")
                                            {
                                                doc.DocumentSubtype = Convert.ToInt32(row["document_subtype"]);
                                            }
                                            if (Convert.ToString(row["Amount"]) != "")
                                            {
                                                doc.TransactionAmount = Convert.ToDouble(row["Amount"]);
                                            }
                                            doc.TransactionCurrency = Convert.ToString(row["currency"]);

                                            if (Convert.ToString(row["debet_account"]) != "")
                                            {
                                                doc.DebitAccount = Convert.ToInt64(row["debet_account"]);
                                            }
                                            doc.TransactionDescription = Convert.ToString(row["description"]);
                                            if (Convert.ToString(row["confirmation_date"]) != "")
                                            {
                                                doc.ConfirmationDate = Convert.ToString(row["confirmation_date"]);
                                            }
                                            if (Convert.ToString(row["quality"]) != "")
                                            {
                                                doc.TransactionQuality = Convert.ToInt32(row["quality"]);
                                            }
                                            if (Convert.ToString(row["credit_bank_code"]) != "")
                                            {
                                                doc.CreditBankCode = Convert.ToInt32(row["credit_bank_code"]);
                                            }
                                            if (Convert.ToString(row["urgent"]) != "")
                                            {
                                                doc.Urgent = Convert.ToInt32(row["urgent"]);
                                            }

                                            if (Convert.ToString(row["source_type"]) != "")
                                            {
                                                doc.TransactionSource = Convert.ToInt32(row["source_type"]);
                                            }
                                            if (Convert.ToString(row["for_automat_confirmated"]) != "")
                                            {
                                                doc.ForAutomatConfirmated = Convert.ToBoolean(row["for_automat_confirmated"]);
                                            }
                                            if (Convert.ToString(row["by_job"]) != "")
                                            {
                                                doc.ByJob = Convert.ToBoolean(row["by_job"]);
                                            }
                                            doc.RegistrationDate = Convert.ToString(row["registration_date"]);

                                            if (doc.DocumentType == 3)
                                            {
                                                using SqlCommand _cmd = _con.CreateCommand();
                                                _cmd.CommandText = "select descr_for_payment from Tbl_HB_documents where doc_ID = @docId";
                                                _cmd.CommandType = CommandType.Text;
                                                _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = doc.TransactionCode;

                                                using SqlDataReader reader = _cmd.ExecuteReader();

                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        doc.TransactionDescription = Convert.ToString(reader["descr_for_payment"]);
                                                    }
                                                }
                                            }

                                            using (SqlCommand _cmd = _con.CreateCommand())
                                            {
                                                _cmd.CommandText = "select t.for_automat_confirmated_HB FROM [dbo].[Tbl_HB_documents] D " +
                                                    "INNER JOIN [dbo].[Tbl_types_of_HB_products] T ON T.product_type= D.document_type  where d.doc_ID = @docId";
                                                _cmd.CommandType = CommandType.Text;
                                                _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = doc.TransactionCode;

                                                using SqlDataReader reader = _cmd.ExecuteReader();

                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        doc.TypeIsForAutomatonfirmated = Convert.ToBoolean(reader["for_automat_confirmated_HB"]);
                                                    }
                                                }
                                            }

                                            string query = "SELECT d.doc_ID,customer_number,d.credit_bank_code,d.credit_account, cast(d.credit_bank_code as varchar)+d.credit_account as arm_number," +
                                                    "CASE WHEN d.currency = 'RUR' THEN  d.receiver_name ELSE dbo.getascii(d.receiver_name) END as receiver_name,d.budj_transfer_reg_code,r.Reject_description,d.urgent " +
                                                    "FROM Tbl_HB_documents d  LEFT JOIN Tbl_types_of_HB_rejects r ON d.reject_ID = r.Reject_ID  WHERE d.doc_ID = @docId";
                                            using (SqlCommand _cmd = _con.CreateCommand())
                                            {
                                                _cmd.CommandText = query;
                                                _cmd.CommandType = CommandType.Text;
                                                _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = doc.TransactionCode;

                                                 SqlDataReader reader = _cmd.ExecuteReader();

                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        if (Convert.ToString(reader["receiver_name"]) != "")
                                                        {
                                                            doc.ReceiverName = Convert.ToString(reader["receiver_name"]);
                                                        }
                                                    }
                                                }
                                                reader.Close();

                                                if (doc.CreditBankCode != 0)
                                                {
                                                    string account = string.Empty;
                                                    string queryAccount = "SELECT  CASE WHEN len(credit_account) > 10 THEN credit_account ELSE cast(credit_bank_code as nvarchar(50)) + cast(credit_account as nvarchar(50)) END AS account" +
                                                        " FROM [HBBase].[dbo].[Tbl_HB_documents] where doc_ID = @docId";
                                                    _cmd.CommandText = queryAccount;
                                                    _cmd.CommandType = CommandType.Text;
                                                    //_cmd.Parameters.Add("@docId", SqlDbType.Int).Value = doc.TransactionCode;

                                                    reader = _cmd.ExecuteReader();

                                                    if (reader.HasRows)
                                                    {
                                                        while (reader.Read())
                                                        {
                                                            if (Convert.ToString(reader["account"]) != "")
                                                            {
                                                                account = Convert.ToString(reader["account"]);
                                                            }
                                                        }
                                                    }
                                                    reader.Close();


                                                    if (account != "")
                                                    {
                                                        string queryCustomer = "select [description] as reciver_name from [tbl_all_accounts;] where arm_number = @account";

                                                        _cmd.CommandText = queryCustomer;
                                                        _cmd.CommandType = CommandType.Text;
                                                        _cmd.Parameters.Clear();
                                                        _cmd.Parameters.Add("@account", SqlDbType.Float).Value = account;

                                                        reader = _cmd.ExecuteReader();

                                                        if (reader.HasRows)
                                                        {
                                                            if (reader.Read())
                                                            {
                                                                doc.ReceiverNameInRecords = Convert.ToString(reader["reciver_name"]);
                                                            }
                                                        }


                                                    }


                                                }

                                            }
                                            if (doc.DocumentType == 5)
                                            {
                                                using SqlConnection _conCredit = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                                                string queryCredit = "Select d.customer_number,d.Currency,d.Credit_account,r.Reject_description  " +
                                                    "from tbl_hb_documents d LEFT JOIN Tbl_types_of_HB_rejects  r " +
                                                    "ON d.reject_ID = r.Reject_ID  where doc_id = @docId";
                                                using SqlCommand _cmd = new SqlCommand(queryCredit, _conCredit);
                                                _conCredit.Open();
                                                _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = doc.TransactionCode;
                                                using SqlDataReader reader = _cmd.ExecuteReader();

                                                if (reader.HasRows)
                                                {
                                                    if (reader.Read())
                                                    {
                                                        doc.CreditAccount = Convert.ToString(reader["Credit_account"]);
                                                    }
                                                }

                                                _conCredit.Close();

                                            }


                                            documents.Add(doc);

                                            rowCount++;
                                        }
                                    }
                                    else if (rowCount > obj.LastGetRowCount)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        rowCount++;
                                    }
                                }

                            }
                            totalAmount = Convert.ToDouble(da.SelectCommand.Parameters["@totalAmount"].Value);
                            totalQuantity = Convert.ToInt32(da.SelectCommand.Parameters["@totalQuantity"].Value);
                        }
                    }
                    _con.Close();
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }

            }

            if (documents.Count != 0)
            {
                documents[0].TotalAmount = totalAmount;
                documents[0].TotalQuantity = totalQuantity;
            }

            if (documents.Count != 0 && documents[0].CustomerNumber != 0)
            {
                documents[0] = GetCustomerAccountAndInfoDetails(documents[0]);
            }

            return documents;
        }

        internal static HBDocuments GetCustomerAccountAndInfoDetails(HBDocuments obj)
        {
            if (obj != null)
            {
                HBDocumentCustomerDetails customerDetails = new HBDocumentCustomerDetails();
                HBDocumentAccountDetails account = new HBDocumentAccountDetails();

                DateTime operDay = Utility.GetNextOperDay().Date;

                if (obj.DebitAccount == null && obj.CustomerNumber == 0)
                {
                    long[] arr = GetCustomerDebitAccount(obj.TransactionCode);
                    obj.CustomerNumber = arr[0];
                    obj.DebitAccount = arr[1];
                }
                int rowCounts = 0;

                customerDetails.ObjectEmpty = true;

                if (obj.CustomerNumber != 0 && obj.DebitAccount != 0)
                {
                    long customerNum = 0;

                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                    _con.Open();

                    string current_sp = "sp_get_acc_details";
                    try
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = new SqlCommand(current_sp, _con);
                            da.SelectCommand.CommandType = CommandType.StoredProcedure;

                            da.SelectCommand.Parameters.Add("@analitic_acc", SqlDbType.BigInt).Value = obj.DebitAccount;
                            if (operDay != null)
                            {
                                da.SelectCommand.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = operDay;
                            }
                            else
                            {
                                da.SelectCommand.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = DBNull.Value;
                            }
                            if (operDay != null)
                            {
                                da.SelectCommand.Parameters.Add("@set_date1", SqlDbType.SmallDateTime).Value = operDay;
                            }
                            else
                            {
                                da.SelectCommand.Parameters.Add("@set_date1", SqlDbType.SmallDateTime).Value = DBNull.Value;
                            }

                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            if (ds != null)
                            {
                                System.Data.DataTable dt0 = ds.Tables[0];

                                if (dt0.Rows.Count != 0)
                                {
                                    foreach (DataRow row in dt0.Rows)
                                    {
                                        customerDetails.CustomerDescription = Convert.ToString(row["description"]);
                                        customerDetails.PosOnlineDescription = Convert.ToString(row["PosOnlineDescription"]); //21
                                        if (Convert.ToString(row["account_type"]) != "")
                                        {
                                            customerDetails.AccountType = Convert.ToInt32(row["account_type"]);
                                        }
                                        else
                                        {
                                            customerDetails.AccountType = 0;
                                        }
                                        customerDetails.AparikDescription = Convert.ToString(row["AparikDescription"]);
                                        if (Convert.ToString(row["closing_date"]) != "")
                                        {
                                            customerDetails.ClosingDate = Convert.ToDateTime(row["closing_date"]).ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            customerDetails.ClosingDate = string.Empty;
                                        }
                                        if (Convert.ToString(row["freeze_date"]) != "")
                                        {
                                            customerDetails.FreezeDate = Convert.ToDateTime(row["freeze_date"]).ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            customerDetails.FreezeDate = string.Empty;
                                        }
                                        if (Convert.ToString(row["debt"]) != "")
                                        {
                                            customerDetails.Debt = Convert.ToDouble(row["debt"]);
                                        }
                                        else
                                        {
                                            customerDetails.Debt = 0;
                                        }
                                        if (Convert.ToString(row["debt_acc"]) != "")
                                        {
                                            customerDetails.DebtAcc = Convert.ToDouble(row["debt_acc"]);
                                        }
                                        else
                                        {
                                            customerDetails.DebtAcc = 0;
                                        }
                                        if (Convert.ToString(row["debt_hb"]) != "")
                                        {
                                            customerDetails.DebtHb = Convert.ToDouble(row["debt_hb"]);
                                        }
                                        else
                                        {
                                            customerDetails.DebtHb = 0;
                                        }
                                        if (Convert.ToString(row["UnUsed_amount"]) != "")
                                        {
                                            customerDetails.UnUsedAmount = Convert.ToDouble(row["UnUsed_amount"]);
                                        }
                                        else
                                        {
                                            customerDetails.UnUsedAmount = 0;
                                        }
                                        if (Convert.ToString(row["UnUsed_amount_date"]) != "")
                                        {
                                            customerDetails.UnUsedAmountDate = Convert.ToDateTime(row["UnUsed_amount_date"]).ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            customerDetails.UnUsedAmountDate = string.Empty;
                                        }


                                        account.Description = Convert.ToString(row["description"]);
                                        if (Convert.ToString(row["arm_number"]) != "")
                                        {
                                            account.Account = Convert.ToInt64(row["arm_number"]);
                                        }
                                        else
                                        {
                                            account.Account = 0;
                                        }
                                        account.AccountCurrency = Convert.ToString(row["currency"]);

                                        account.BalanceAMD = Convert.ToDouble(row["balance_amd"]);
                                        account.BalanceCurrency = Convert.ToDouble(row["balance_cur"]);

                                        account.DebitAMD = Convert.ToDouble(row["debet_amd"]);
                                        account.DebitCurrency = Convert.ToDouble(row["debet_cur"]);

                                        account.CreditAMD = Convert.ToDouble(row["credit_amd"]);
                                        account.CreditCurrency = Convert.ToDouble(row["credit_cur"]);

                                        account.EntryAMD = Convert.ToDouble(row["entry_amd"]);
                                        account.EntryCurrency = Convert.ToDouble(row["entry_cur"]);


                                        if (Convert.ToString(row["card_number"]) != "")
                                        {
                                            account.CardNumber = Convert.ToString(row["card_number"]);

                                            using (SqlCommand _cmd = new SqlCommand())
                                            {
                                                _cmd.Connection = _con;
                                                _cmd.CommandText = "Select dbo.Fnc_CardTypeFull ( @cardNumber ) AS cardType";
                                                _cmd.CommandType = CommandType.Text;
                                                _cmd.Parameters.Add("@cardNumber", SqlDbType.BigInt).Value = account.CardNumber;

                                                using SqlDataReader reader = _cmd.ExecuteReader();
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        account.CardType = Convert.ToString(reader["cardType"]);
                                                    }
                                                }
                                            }
                                        }

                                        if (Convert.ToString(row["customer_number"]) != "")
                                        {
                                            customerNum = Convert.ToInt64(row["customer_number"]);
                                        }
                                    }
                                }
                                else
                                {
                                    rowCounts++;
                                }

                                System.Data.DataTable dt1 = ds.Tables[1];
                                if (dt1.Rows.Count != 0)
                                {
                                    customerDetails.ExtractsReceivingTypeDescription = new Dictionary<string, string>();

                                    foreach (DataRow row in dt1.Rows)
                                    {
                                        if (Convert.ToString(row["Description"]) != "")
                                        {
                                            customerDetails.ExtractsReceivingTypeDescription.Add(Convert.ToString(row["AdditionDescription"]), Convert.ToString(row["Description"]));
                                        }
                                        else
                                        {
                                            customerDetails.ExtractsReceivingTypeDescription.Add(Convert.ToString(row["AdditionDescription"]), Convert.ToString(row["AdditionValue"]));
                                        }
                                    }
                                }
                                else
                                {
                                    rowCounts++;
                                }

                                System.Data.DataTable dt2 = ds.Tables[2];
                                foreach (DataRow row in dt2.Rows)
                                {
                                    switch (Convert.ToString(row["Currency"]))
                                    {
                                        case "AMD":
                                            customerDetails.ProvisionAMDDescription = Convert.ToString(row["Currency"]); //0
                                            if (Convert.ToString(row["Currency"]) != "")
                                            {
                                                customerDetails.ProvisionAMDAmount = Convert.ToDouble(row["provision_amount"]); //1
                                            }
                                            break;
                                        case "USD":
                                            customerDetails.ProvisionUSDDescription = Convert.ToString(row["Currency"]); //0
                                            if (Convert.ToString(row["provision_amount"]) != "")
                                            {
                                                customerDetails.ProvisionUSDAmount = Convert.ToDouble(row["provision_amount"]); //1
                                            }
                                            break;
                                        case "EUR":
                                            customerDetails.ProvisionEURDescription = Convert.ToString(row["Currency"]); //0
                                            if (Convert.ToString(row["Currency"]) != "")
                                            {
                                                customerDetails.ProvisionEURAmount = Convert.ToDouble(row["provision_amount"]); //1
                                            }
                                            break;
                                        case "RUR":
                                            customerDetails.ProvisionRURDescription = Convert.ToString(row["Currency"]); //0
                                            if (Convert.ToString(row["Currency"]) != "")
                                            {
                                                customerDetails.ProvisionRURAmount = Convert.ToDouble(row["provision_amount"]); //1
                                            }
                                            break;
                                    }

                                }
                                System.Data.DataTable dt3 = ds.Tables[3];
                                foreach (DataRow row in dt3.Rows)
                                {
                                    customerDetails.DAHKDescription = Convert.ToString(row["descr"]);
                                    customerDetails.DAHKAmount = Convert.ToString(row["amount_descr"]);
                                }

                                System.Data.DataTable dt4 = ds.Tables[4];
                                foreach (DataRow row in dt4.Rows)
                                {
                                    customerDetails.MovedAmountDescription = Convert.ToString(row["descr"]);
                                    customerDetails.MovedAmount = Convert.ToString(row["amount_descr"]);
                                }

                                System.Data.DataTable dt5 = ds.Tables[5];
                                if (dt5.Rows.Count != 0)
                                {
                                    foreach (DataRow row in dt5.Rows)
                                    {
                                        customerDetails.LastUpdatedInfoDescription = Convert.ToString(row["descr"]);
                                        customerDetails.LastUpdatedDate = Convert.ToString(row["value"]);
                                    }
                                }
                                else
                                {
                                    rowCounts++;
                                }

                                System.Data.DataTable dt6 = ds.Tables[6];

                                if (dt6.Rows.Count != 0)
                                {
                                    foreach (DataRow row in dt6.Rows)
                                    {
                                        customerDetails.AMLDescription = Convert.ToString(row["descr"]);
                                        customerDetails.AML = Convert.ToString(row["value"]);
                                    }
                                }
                                else
                                {
                                    rowCounts++;
                                }
                                System.Data.DataTable dt7 = ds.Tables[7];
                                foreach (DataRow row in dt7.Rows)
                                {
                                    customerDetails.DocDeficientReasonDescription = Convert.ToString(row["descr"]);
                                    customerDetails.DocDeficientReason = Convert.ToString(row["value"]);
                                    customerDetails.DocDeficientReasonDate = Convert.ToString(row["gvdate"]);
                                }
                            }

                        }

                        if (rowCounts >= 3)
                        {
                            customerDetails.ObjectEmpty = true;
                        }
                        else
                        {
                            customerDetails.ObjectEmpty = false;
                        }

                        if (customerNum == 0)
                        {
                            customerNum = obj.CustomerNumber;
                        }

                        int mark = 0;
                        string query = "Select dbo.fn_getCustomerVipType(@customerNum)";
                        using (SqlCommand _cmd = _con.CreateCommand())
                        {
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@customerNum", SqlDbType.Float).Value = customerNum;

                            SqlDataReader reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (Convert.ToString(reader[0]) != "")
                                    {
                                        mark = Convert.ToInt32(reader[0]);
                                    }
                                }
                            }


                            reader.Close();

                            query = "Select mark FROM Tbl_Type_Of_Vip_Customers WHERE id = @mark";
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@mark", SqlDbType.Int).Value = mark;

                            reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (Convert.ToString(reader[0]) != "")
                                    {
                                        customerDetails.CustomerVIPTypes = Convert.ToString(reader["mark"]);
                                    }
                                }
                            }

                            reader.Close();


                            query = "Select dbo.Fnc_IsBirthDayOfCustomer(0, @customerNum)";
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@customerNum", SqlDbType.Float).Value = customerNum;

                            reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (Convert.ToString(reader[0]) != "")
                                    {
                                        customerDetails.IsCustomerBirthday = Convert.ToBoolean(reader[0]);
                                    }
                                }
                            }

                            reader.Close();

                            query = "Select 1 from tbl_arca_points_list  where account_number in (select arm_number from [tbl_all_accounts;] where customer_number= @customerNum) " +
                                "and type_of_point in (2, 3) and point_place<>1 and closing_date is null";
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@customerNum", SqlDbType.Float).Value = customerNum;

                            reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                customerDetails.CustomerPOS = 1;
                            }

                            reader.Close();

                            query = "SELECT c.member FROM [tbl_all_accounts;] a inner join tbl_customers c on a.customer_number=c.customer_number WHERE a.arm_number = @DebitAccount";
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@DebitAccount", SqlDbType.BigInt).Value = obj.DebitAccount;

                            reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (Convert.ToString(reader[0]) != "")
                                    {
                                        customerDetails.CustomerIsMember = Convert.ToInt32(reader["member"]);
                                    }
                                }
                            }

                        }

                        _con.Close();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }

                }

                obj.AccountDetails = account;
                obj.CustomerDetails = customerDetails;
            }

            return obj;

        }

        internal static HBDocumentTransactionError GetTransactionErrorDetails(long transctionCode)
        {
            HBDocumentTransactionError error = new HBDocumentTransactionError();

            if (transctionCode != 0)
            {
                int sourceID = GetHbDocumentSource(transctionCode);

                if (sourceID != 0)
                {
                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                    {
                        string query = "Select top 1 * from tbl_automated_operations_log where operationID = @transctionCode and externalBankingType = @sourceID order by id desc";
                        using (SqlCommand _cmd = new SqlCommand(query, _con))
                        {
                            _con.Open();
                            _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;
                            _cmd.Parameters.Add("@sourceID", SqlDbType.Int).Value = sourceID;

                            using SqlDataReader reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    error.ID = Convert.ToInt32(reader["ID"]);
                                    error.OperationID = Convert.ToInt32(reader["operationID"]);
                                    error.RegistrationDate = Convert.ToDateTime(reader["registrationDate"]).ToString("dd/MM/yyyy HH:mm");
                                    error.ErrorDescription = Convert.ToString(reader["errorDescription"]);
                                    error.SetNumber = Convert.ToInt32(reader["setNumber"]);
                                    error.ExternalBankingType = Convert.ToInt32(reader["externalBankingType"]);

                                }
                            }

                            _con.Close();
                        }
                    }
                }
                else
                {
                    error.SoftwareError = transctionCode.ToString() + " համարով ինտերնետային փաստաթուղթը գտնված չէ։";
                }

            }
            else
            {
                error.SoftwareError = string.Empty;
            }

            return error;
        }

        internal static List<HBDocumentConfirmationHistory> GetConfirmationHistoryDetails(long transctionCode)
        {
            List<HBDocumentConfirmationHistory> histories = new List<HBDocumentConfirmationHistory>();

            if (transctionCode != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select [change_date],b.description_arm,[change_set_number], isnull(change_user_name,'') as change_user_name FROM [dbo].[Tbl_HB_quality_history] a left join  [HBBase].[dbo].[Tbl_types_of_HB_quality] b on a.quality = b.quality where Doc_ID = @transctionCode order by change_date";
                    using SqlCommand _cmd = new SqlCommand(query, _con);
                    _con.Open();
                    _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;

                    using SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HBDocumentConfirmationHistory history = new HBDocumentConfirmationHistory();
                            history.TransactionCode = transctionCode;
                            history.TransactionDate = Convert.ToDateTime(reader["change_date"]).ToString("dd/MM/yyyy HH:mm");
                            history.Description = Convert.ToString(reader["description_arm"]);
                            if (Convert.ToString(reader["change_set_number"]) != "")
                            {
                                history.SetNumber = Convert.ToString(reader["change_set_number"]);
                            }
                            history.CustomerUsername = Convert.ToString(reader["change_user_name"]);


                            histories.Add(history);
                        }
                    }

                    _con.Close();
                }

            }

            return histories;
        }

        internal static string GetCheckingProductAccordance(long transctionCode)
        {
            string str = string.Empty;

            if (transctionCode != 0)
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                string query = "Select * FROM Tbl_HB_documents WHERE Doc_Id = @transctionCode";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;

                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (Convert.ToInt32(reader["quality"]) != 30)
                        {
                            str = "Կատարված չէ։";
                        }
                    }
                }

                _con.Close();

            }

            return str;
        }

        internal static HBDocumentConfirmationHistory GetProductAccordanceDetails(long transctionCode)
        {
            HBDocumentConfirmationHistory details = new HBDocumentConfirmationHistory();

            if (transctionCode != 0)
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                string query = "Select TOP 1 * FROM Tbl_HB_products_accordance WHERE Doc_Id = @transctionCode ORDER BY uniq_number";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;

                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        details.TransactionCode = Convert.ToInt32(reader["Doc_ID"]);
                        details.TransactionDate = Convert.ToDateTime(reader["accounting_date"]).ToString("dd/MM/yyyy");
                        details.SetNumber = Convert.ToString(reader["set_number"]);
                        details.UniqNumber = Convert.ToInt32(reader["accounting_uniq_number"]);
                        if (Convert.ToString(reader["transactions_group_number"]) != "")
                        {
                            details.InternalTransactionCode = Convert.ToInt64(reader["transactions_group_number"]);
                        }

                    }
                }

                _con.Close();

            }

            return details;
        }

        internal static bool SetHBDocumentAutomatConfirmationSign(HBDocumentFilters obj)
        {
            string filter = string.Empty;
            obj.BankCode = 22000;
            bool done = false;
            try
            {
                if (obj != null)
                {

                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                    string current_sp = "pr_set_hb_document_automat_confirmation_sign";
                    _con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(current_sp, _con);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;

                        da.SelectCommand.Parameters.Add("@condition", SqlDbType.VarChar).Value = filter;

                        da.SelectCommand.ExecuteNonQuery();

                        done = true;
                    }
                    _con.Close();
                }
            }
            catch (Exception ex)
            {
                done = false;
            }

            return done;
        }

        internal static bool ExcludeCardAccountTransactions(HBDocumentFilters obj)
        {
            string filter = string.Empty;
            bool done = false;
            try
            {
                if (obj != null)
                {
                    filter = GetFilter(obj);

                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                    {
                        string current_sp = "pr_set_hb_document_automat_confirmation_sign";
                        _con.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = new SqlCommand(current_sp, _con);
                            da.SelectCommand.CommandType = CommandType.StoredProcedure;

                            da.SelectCommand.Parameters.Add("@condition", SqlDbType.VarChar).Value = filter;
                            da.SelectCommand.Parameters.Add("@exceptCardAccount", SqlDbType.Bit).Value = 1;
                            da.SelectCommand.Parameters.Add("@checkFlag", SqlDbType.Bit).Value = 0;
                            da.SelectCommand.Parameters.Add("@docID", SqlDbType.Int).Value = 0;

                            da.SelectCommand.ExecuteNonQuery();

                            done = true;
                        }
                        _con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                done = false;
            }

            return done;
        }


        internal static bool SelectOrRemoveFromAutomaticExecution(HBDocumentFilters obj)
        {
            string filter = string.Empty;
            bool done = false;

            try
            {
                if (obj != null)
                {
                    filter = GetFilter(obj);

                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                    string current_sp = "pr_set_hb_document_automat_confirmation_sign";
                    _con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(current_sp, _con);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;

                        da.SelectCommand.Parameters.Add("@condition", SqlDbType.VarChar).Value = filter;
                        da.SelectCommand.Parameters.Add("@exceptCardAccount", SqlDbType.Bit).Value = 0;
                        da.SelectCommand.Parameters.Add("@checkFlag", SqlDbType.Bit).Value = obj.TransactionChecked;
                        da.SelectCommand.Parameters.Add("@docID", SqlDbType.Int).Value = obj.DocumentTransactionCode;

                        da.SelectCommand.ExecuteNonQuery();

                        done = true;
                    }
                    _con.Close();
                }
            }
            catch (Exception ex)
            {
                done = false;
            }


            return done;
        }

        internal static string GetHBArCaBalancePermission(long transctionCode, long accountGroup)
        {
            string flag = string.Empty;
            string visaNumber = string.Empty;
            string accountNumber = string.Empty;
            bool access = false;


            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                _con.Open();

                using (SqlCommand _cmd = _con.CreateCommand())
                {
                    string query = "Select  c.visa_number FROM tbl_hb_documents hb INNER JOIN tbl_credit_lines c on hb.document_number = c.app_id  " +
                        "where HB.Doc_ID = @transctionCode";
                    _cmd.CommandText = query;
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;

                    SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToString(reader["visa_number"]) != "")
                            {
                                visaNumber = Convert.ToString(reader["visa_number"]);
                            }
                            else
                            {
                                flag = "Քարտը գտնված չէ";
                                _con.Close();
                                return flag;
                            }
                        }
                    }
                    else
                    {
                        _con.Close();
                        flag = "Քարտի հաշիվը բացակայում է";
                        return flag;
                    }

                    reader.Close();

                    query = "Select card_account from Tbl_visa_numbers_accounts where visa_number= @visa_number";
                    _cmd.CommandText = query;
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.Clear();
                    _cmd.Parameters.Add("@visa_number", SqlDbType.Float).Value = visaNumber;

                    reader = _cmd.ExecuteReader();

                    if (!(reader.HasRows))
                    {
                        _con.Close();
                        flag = "Քարտի հաշիվը բացակայում է";
                        return flag;
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            accountNumber = Convert.ToString(reader["card_account"]);
                        }
                    }
                }
                _con.Close();

            }

            access = FromAccountUserGroupAccess(accountNumber, accountGroup);

            if (access == false)
            {
                flag = "Նշված քարտի համար մնացորդի դիտարկումը թույլատրված չէ";
            }

            return flag;
        }



        static bool FromAccountUserGroupAccess(string accountNumber, long accountGroup)
        {
            bool access = false;
            string UserGroupID_For_Accounts = accountGroup.ToString();
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string query = "Select dbo.fnc_User_account_access (@UserGroupID_For_Accounts, @accountNumber)";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@UserGroupID_For_Accounts", SqlDbType.NVarChar).Value = UserGroupID_For_Accounts;
                _cmd.Parameters.Add("@accountNumber", SqlDbType.NVarChar).Value = accountNumber;
                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        access = Convert.ToBoolean(reader[0]);
                    }
                }

                _con.Close();
            }
            return access;
        }

        internal static string GetHBAccountNumber(string cardNumber)
        {
            string accountNumber = string.Empty;
            if (cardNumber != "")
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                string query = "Select card_account from Tbl_visa_numbers_accounts where visa_number= @cardNumber";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@cardNumber", SqlDbType.BigInt).Value = cardNumber;
                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accountNumber = Convert.ToString(reader["card_account"]);
                    }
                }

                _con.Close();
            }

            return accountNumber;
        }
        static int GetHbDocumentSource(long transctionCode)
        {
            int sourceID = 0;
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string query = "Select dbo.fn_get_HB_document_source(@transctionCode)";

                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;
                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        sourceID = Convert.ToInt32(reader[0]);
                    }
                }

                _con.Close();
            }

            return sourceID;
        }
        static long[] GetCustomerDebitAccount(long transactionCode)
        {
            long[] details = new long[2];
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string query = "Select customer_number,debet_account FROM Tbl_HB_documents WHERE doc_ID = @transactionCode";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                _cmd.Parameters.Add("@transactionCode", SqlDbType.Int).Value = transactionCode;
                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        details[0] = Convert.ToInt64(reader[0]);
                        details[1] = Convert.ToInt64(reader[1]);
                    }
                }

                _con.Close();
            }

            return details;
        }

        internal static string ConfirmTransactionReject(HBDocuments document)
        {
            string done = string.Empty;

            long hbDocID = document.TransactionCode;
            string msgSubject = "N " + hbDocID.ToString() + "  գործարքի մերժում N " + hbDocID.ToString() + " transaction rejection";

            string rejectArm = string.Empty;
            string rejectEng = string.Empty;
            string msgText = string.Empty;
            DateTime operDay = Utility.GetNextOperDay().Date;

            if (document.TransactionQuality == 3 || document.TransactionQuality == 20)
            {
                try
                {
                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                    _con.Open();
                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string query = "Select Reject_description,Reject_description_eng,reject_id from Tbl_types_of_HB_rejects where for_orderby = @id";
                        _cmd.CommandText = query;
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@id", SqlDbType.Int).Value = document.SelectedRejectReason;

                        using SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToString(reader[0]) != "")
                                {
                                    rejectArm = Convert.ToString(reader["Reject_description"]);
                                    rejectEng = Convert.ToString(reader["Reject_description_eng"]);
                                    document.SelectedRejectReason = Convert.ToInt32(reader["reject_id"]);
                                }
                            }
                        }

                        reader.Close();

                        msgText = "Հարգելի հաճախորդ, \n Ձեր N " + hbDocID.ToString() + " գործարքը մերժվել է:\n Մերժման պատճառ` §" + rejectArm + "¦\n  Dear Client,\n  Your N " + hbDocID.ToString() + " transaction have been rejected for the following reason: " + rejectEng + "' ";

                        if (document.TransactionSource == 1 || document.TransactionSource == 5)
                        {

                            string current_sp = "sp_insertMsg";
                            _cmd.CommandText = current_sp;
                            _cmd.CommandType = CommandType.StoredProcedure;
                            _cmd.Parameters.Clear();

                            _cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = document.CustomerNumber;
                            _cmd.Parameters.Add("@reply_id", SqlDbType.Float).Value = hbDocID;
                            _cmd.Parameters.Add("@insert_msg", SqlDbType.NVarChar).Value = msgText;
                            _cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = document.SetNumber;
                            _cmd.Parameters.Add("@sentrecieve", SqlDbType.Int).Value = 3;
                            _cmd.Parameters.Add("@subject", SqlDbType.NVarChar).Value = msgSubject;

                            _cmd.ExecuteNonQuery();
                        }


                    }

                    using (SqlCommand _cmd = _con.CreateCommand())
                    {

                        string current_sp1 = "pr_HB_Rejection_confirm";
                        _cmd.CommandText = current_sp1;
                        _cmd.CommandType = CommandType.StoredProcedure;

                        _cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = hbDocID;
                        _cmd.Parameters.Add("@set_Date", SqlDbType.SmallDateTime).Value = operDay;
                        _cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = Convert.ToInt16(document.SetNumber);
                        _cmd.Parameters.Add("@RejectionReasonID", SqlDbType.Int).Value = document.SelectedRejectReason;
                        _cmd.Parameters.Add("@RejectionQuality", SqlDbType.SmallInt).Value = 31;

                        _cmd.ExecuteNonQuery();


                    }

                    _con.Close();
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    done = error;
                }
            }
            else
            {
                done = "Տվյալ գործարքը մերժված է։";
            }

            return done;
        }

        internal static string ChangeTransactionQuality(long transctionCode)
        {
            string result = string.Empty;
            long uniqNumber = 0;

            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                _con.Open();

                try
                {
                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string query = "Select uniq_number from Tbl_HB_quality_history where doc_id= @transctionCode and quality=2 ";
                        _cmd.CommandText = query;
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@transctionCode", SqlDbType.Int).Value = transctionCode;

                        using SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {
                                if (Convert.ToString(reader[0]) != "")
                                {
                                    uniqNumber = Convert.ToInt64(reader["uniq_number"]);
                                }
                            }
                        }
                        else
                        {
                            result = "Գործարքը գտնված չէ";
                            _con.Close();
                            return result;
                        }
                    }

                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string current_sp = "pr_change_HB_transaction_quality";

                        _cmd.CommandText = current_sp;
                        _cmd.CommandType = CommandType.StoredProcedure;

                        _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = transctionCode;
                        _cmd.Parameters.Add("@uniq_number", SqlDbType.Int).Value = uniqNumber;

                        _cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    result = "Տեղի ունեցավ սխալ";
                }
            }

            return result;
        }


        internal static string ChangeAutomatedConfirmTime(List<string> info)
        {
            string date = string.Empty;

            string setdate = Convert.ToDateTime(info[0]).ToString("dd/MM/yyyy HH:mm");
            string time = setdate.Substring(11);

            int setNumber = Convert.ToInt32(info[1]);

            try
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                _con.Open();

                using (SqlCommand _cmd = _con.CreateCommand())
                {
                    string query = "Update Tbl_oper_days SET set_transaction_confirm_time = @time  , time_set_number = @time_set_number where  oper_day  = (SELECT oper_day FROM  Tbl_current_oper_day)";

                    _cmd.CommandText = query;
                    _cmd.Parameters.Add("@time", SqlDbType.NVarChar).Value = time;
                    _cmd.Parameters.Add("@time_set_number", SqlDbType.Int).Value = setNumber;

                    _cmd.ExecuteNonQuery();


                    query = "select (convert(nvarchar(20),format(oper_day,'yyyy-MM-dd')) + ' ' + convert (nvarchar(20) , format([set_transaction_confirm_time], 'hh\\:mm' ))) as oper_day from Tbl_oper_days WHERE oper_day  = (SELECT oper_day FROM  Tbl_current_oper_day) ";
                    _cmd.CommandText = query;
                    _cmd.Parameters.Clear();

                    using SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            date = Convert.ToDateTime(reader["oper_day"]).ToString("dd/MM/yyyy HH:mm");
                        }
                    }
                }


                _con.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return date;
        }

        internal static string GetAutomatedConfirmTime()
        {
            string date = string.Empty;

            try
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                string query = "select  convert (nvarchar(20) , format([set_transaction_confirm_time], 'hh\\:mm' )) as trans_time " +
                    "from [dbo].[Tbl_oper_days] where oper_day = (SELECT oper_day FROM [Tbl_current_oper_day])";
                using SqlCommand _cmd = new SqlCommand(query, _con);
                _con.Open();
                using SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (Convert.ToString(reader[0]) != "")
                        {
                            date = Convert.ToDateTime(reader["trans_time"]).ToString("dd/MM/yyyy HH:mm");
                        }
                    }

                }
                _con.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return date;
        }

        internal static bool FormulateAllHBDocuments(HBDocumentFilters obj)
        {
            string filter = string.Empty;
            bool done = false;
            DateTime operDay = Utility.GetNextOperDay().Date;
            int itemNumber = 0;
            int lockResult = 0;
            obj.BankCode = 22000;

            try
            {
                if (obj != null)
                {
                    filter = GetFilter(obj);

                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                    {
                        _con.Open();

                        lockResult = LockingTransactions(14);

                        string current_sp = "pr_HB_automat_confirmation";

                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = new SqlCommand(current_sp, _con);
                            da.SelectCommand.CommandTimeout = 300;
                            da.SelectCommand.CommandType = CommandType.StoredProcedure;

                            da.SelectCommand.Parameters.Add("@WHERE_CONDITION_STRING", SqlDbType.NVarChar).Value = filter;
                            da.SelectCommand.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = operDay;
                            da.SelectCommand.Parameters.Add("@setNumber", SqlDbType.Int).Value = obj.SetNumber;
                            da.SelectCommand.Parameters.Add("@itemNumber", SqlDbType.Int).Value = ParameterDirection.Output;

                            da.SelectCommand.ExecuteNonQuery();
                            itemNumber = Convert.ToInt32(da.SelectCommand.Parameters["@itemNumber"].Value);


                        }
                        _con.Close();
                    }
                    done = true;

                }
            }
            catch (Exception ex)
            {
                done = false;

            }
            finally
            {
                UnlockingTransactions(14);
            }

            return done;
        }

        internal static bool GetReestrFromHB(HBDocuments obj)
        {
            bool done = false;
            ulong docNum = Utility.GetLastKeyNumber(6, 22000);

            if (obj != null)
            {
                try
                {
                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                    {
                        _con.Open();

                        using (SqlCommand _cmd = new SqlCommand())
                        {
                            _cmd.Connection = _con;
                            _cmd.CommandText = "Sp_Reestr_From_HB";
                            _cmd.CommandType = CommandType.StoredProcedure;
                            _cmd.Parameters.Add("@HB_doc_ID", SqlDbType.Int).Value = obj.TransactionCode;
                            _cmd.Parameters.Add("@Set_number", SqlDbType.Int).Value = obj.SetNumber;
                            _cmd.Parameters.Add("@Set_date", SqlDbType.SmallDateTime).Value = Utility.GetNextOperDay().Date;
                            _cmd.Parameters.Add("@filialcode", SqlDbType.Int).Value = 22000;
                            _cmd.Parameters.Add("@doc_number", SqlDbType.Int).Value = docNum;
                            _cmd.Parameters.Add("@office_id", SqlDbType.Int).Value = -1;
                            _cmd.Parameters.Add("@HB_server_name", SqlDbType.NVarChar).Value = "";
                            _cmd.Parameters.Add("@HB_base_name", SqlDbType.NVarChar).Value = "";

                            _cmd.ExecuteNonQuery();
                            done = true;
                        }

                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }
            }



            return done;
        }

        internal static void PostReestrPaymentDetails(ReestrTransferOrder order)
        {
            if (order != null)
            {
                order.ReestrTransferAdditionalDetails.ForEach(m =>
                {
                    if (m.PaymentType != 0)
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                        {
                            conn.Open();
                            string query = "Update Tbl_HB_Transfers_Registry SET payment_type = @payment_type WHERE docID = @id AND IndexID = @index AND credit_account = @credit_account";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add("@payment_type", SqlDbType.Int).Value = m.PaymentType;
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                                cmd.Parameters.Add("@index", SqlDbType.Int).Value = m.Index;
                                cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar).Value = m.CreditAccount.AccountNumber;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                        {
                            conn.Open();
                            string query = "Update Tbl_transactions_from_excel SET payment_type = @payment_type WHERE HB_doc_ID = @id AND number_in_list = @index AND credit_account = @credit_account";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add("@payment_type", SqlDbType.Int).Value = m.PaymentType;
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                                cmd.Parameters.Add("@index", SqlDbType.Int).Value = m.Index;
                                cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar).Value = m.CreditAccount.AccountNumber;

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                });
            }
        }

        internal static List<ReestrTransferAdditionalDetails> CheckHBReestrTransferAdditionalDetails(long orderId, List<ReestrTransferAdditionalDetails> details)
        {
            string filter = " Tbl_transactions_from_excel.HB_doc_ID = " + orderId.ToString();

            if (details != null)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    try
                    {
                        _con.Open();

                        using (SqlCommand _cmd = _con.CreateCommand())
                        {
                            string current_sp = "pr_automat_check_transactions_from_excel";
                            _cmd.CommandText = current_sp;
                            _cmd.Connection = _con;
                            _cmd.CommandType = CommandType.StoredProcedure;

                            _cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = 22000;
                            _cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = Utility.GetNextOperDay().Date;
                            _cmd.Parameters.Add("@checkSubsidia", SqlDbType.Int).Value = 0;
                            _cmd.Parameters.Add("@filter", SqlDbType.VarChar).Value = filter;

                            _cmd.ExecuteNonQuery();



                            string query = "Select number_in_list,[check] FROM Tbl_transactions_from_excel Left join (select * from Tbl_acc_freeze_history where " +
                                "closing_date is null and reason_type = 11) f on Tbl_transactions_from_excel.ID = f.add_table_id " +
                                "WHERE Tbl_transactions_from_excel.HB_doc_ID = @orderId ORDER BY number_in_list";

                            _cmd.Connection = _con;
                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Clear();
                            _cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                            using SqlDataReader reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    for (int i = 0; i < details.Count; i++)
                                    {
                                        if (Convert.ToInt32(reader["number_in_list"]) == details[i].Index)
                                        {
                                            details[i].HBCheckResult = Convert.ToString(reader["check"]);

                                            ulong customerNum = AccountDB.GetAccountCustomerNumber(details[i].CreditAccount.AccountNumber);
                                            details[i].HbDAHKCheckResult = ValidationDB.IsDAHKAvailability(customerNum);

                                            break;
                                        }
                                    }
                                }
                            }

                        }
                        _con.Close();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }

                }

                details.ForEach(m =>
                {
                    if (m.PaymentType != 0)
                    {
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                        {
                            conn.Open();
                            string query = "Update Tbl_HB_Transfers_Registry SET payment_type = @payment_type WHERE docID = @id AND IndexID = @index AND credit_account = @credit_account";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Add("@payment_type", SqlDbType.Int).Value = DBNull.Value;
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = orderId;
                                cmd.Parameters.Add("@index", SqlDbType.Int).Value = m.Index;
                                cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar).Value = m.CreditAccount.AccountNumber;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                        {
                            conn.Open();
                            string query = "Update Tbl_transactions_from_excel SET payment_type = @payment_type WHERE HB_doc_ID = @id AND number_in_list = @index AND credit_account = @credit_account";
                            using SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@payment_type", SqlDbType.Int).Value = DBNull.Value;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = orderId;
                            cmd.Parameters.Add("@index", SqlDbType.Int).Value = m.Index;
                            cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar).Value = m.CreditAccount.AccountNumber;

                            cmd.ExecuteNonQuery();
                        }
                    }
                });

            }

            return details;
        }

        internal static List<string> GetTreansactionConfirmationDetails(long docId, long debitAccount)
        {
            List<string> details = new List<string>();

            if (docId != 0 && debitAccount != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    string query = "Select date_of_beginning FROM [Tbl_deposits;] where quality=1 and deposit_full_number= @deposit_full_number";
                    using SqlCommand _cmd = new SqlCommand(query, _con);
                    _con.Open();
                    _cmd.Parameters.Add("@deposit_full_number", SqlDbType.NVarChar).Value = debitAccount;
                    using SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            details.Add(Convert.ToDateTime(reader["date_of_beginning"]).ToString("dd/MM/yyyy"));
                        }
                    }

                    _con.Close();
                }

                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select connected_doc_id, doc.quality as quality FROM tbl_hb_documents hb INNER JOIN Tbl_DeclineRequest d on " +
                        "hb.doc_id = d.doc_id left join Tbl_HB_documents doc on doc.doc_ID = d.Connected_doc_id where hb.document_type = 18  and hb.doc_id = @docId";
                    using SqlCommand _cmd = new SqlCommand(query, _con);
                    _con.Open();
                    _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                    using SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            details.Add(Convert.ToString(reader["connected_doc_id"]));
                            details.Add(Convert.ToString(reader["quality"]));
                        }
                    }

                    _con.Close();
                }
            }

            return details;
        }

        internal static int GetCancelTransactionDetails(long docId)
        {
            int quality = 0;


            if (docId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select doc.quality AS quality FROM tbl_hb_documents hb INNER JOIN Tbl_DeclineRequest d on hb.doc_id = d.doc_id left join Tbl_HB_documents doc on doc.doc_ID = d.Connected_doc_id where hb.document_type = 18  and hb.doc_id = @docId";
                    using SqlCommand _cmd = new SqlCommand(query, _con);
                    _con.Open();
                    _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                    using SqlDataReader reader = _cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            quality = Convert.ToInt32(reader["quality"]);
                        }
                    }

                    _con.Close();
                }
            }

            return quality;
        }

        internal static string ConfirmReestrTransaction(long docId, int bankCode, short setNumber)
        {
            string result = string.Empty;
            DateTime dt = Utility.GetNextOperDay().Date;
            if (docId != 0 && bankCode != 0 && setNumber != 0)
            {
                using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                string current_sp = "pr_HB_order_manual_confirm";
                try
                {
                    _con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = new SqlCommand(current_sp, _con);
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;

                        da.SelectCommand.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docId;
                        da.SelectCommand.Parameters.Add("@bank_code", SqlDbType.Int).Value = bankCode;
                        da.SelectCommand.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = dt;
                        da.SelectCommand.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;

                        da.SelectCommand.ExecuteNonQuery();

                    }


                    _con.Close();
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }

            return result;
        }

        internal static bool SaveInternationalPaymentAddresses(InternationalPaymentOrder order)
        {
            bool done = false;

            if (order != null)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Update [dbo].[Tbl_HB_documents] SET [sender_address] = @senderAddress, [sender_town] = @senderTown WHERE [doc_ID] = @doc_ID AND [customer_number] = @customerNumber";

                    try
                    {
                        using (SqlCommand _cmd = new SqlCommand(query, _con))
                        {
                            _con.Open();
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@senderAddress", SqlDbType.NVarChar).Value = order.SenderAddress;
                            _cmd.Parameters.Add("@senderTown", SqlDbType.NVarChar).Value = order.SenderTown;
                            _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = order.Id;
                            _cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;

                            _cmd.ExecuteNonQuery();

                            done = true;

                            _con.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;

                    }

                }
            }

            return done;
        }

        internal static List<HBMessages> GetHBMessages(ushort filalCode, string WatchAllMessages)
        {
            List<HBMessages> msg = new List<HBMessages>();
            DateTime operDay = Utility.GetNextOperDay().Date;

            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string countQuery = "Select COUNT(id) count from Tbl_messages_with_bank ms " +
                    "WHERE  ms.customer_number <> 0  and convert(datetime, convert(nvarchar(11), sent_date, 103), 103) = @operDay and sentrecieve = '1'  and status = 1";

                string query = "Select ms.*,dbo.getascii(ms.Subject) as Subject_Ascii, dbo.getascii(ms.description) as description_Ascii," +
                    "case when isnull(c.description,'')<> '' then c.description else name + ' ' + lastname end As name_lastname " +
                    "from(select ms.*, dbo.getascii(ms.Subject) as Subject_Ascii, dbo.getascii(ms.description) as description_Ascii from Tbl_messages_with_bank ms " +
                    "WHERE  ms.customer_number <> 0  and convert(datetime, convert(nvarchar(11), sent_date, 103), 103) = @operDay and sentrecieve = '1'  and status = 1) ms " +
                    "CROSS Apply " +
                    "(SELECT * FROM  V_CustomerDesription where customer_number = ms.customer_number and " +
                    "(filialcode = @filalCode or 1 = @WatchAllMessages)) c order by ms.sent_date desc";

                try
                {
                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                    {
                        _con.Open();

                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@operDay", SqlDbType.NVarChar).Value = operDay.ToString("dd/MMM/yyyy");
                        _cmd.Parameters.Add("@WatchAllMessages", SqlDbType.Bit).Value = Convert.ToInt16(WatchAllMessages);
                        _cmd.Parameters.Add("@filalCode", SqlDbType.Int).Value = filalCode;

                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                HBMessages ms = new HBMessages();
                                ms.ID = Convert.ToInt64(reader["Id"]);
                                ms.FullName = Convert.ToString(reader["name_lastname"]);
                                ms.CustomerNumber = Convert.ToUInt64(reader["customer_number"]);
                                ms.CustomerSubject = Convert.ToString(reader["Subject_Ascii"]);
                                if (Convert.ToString(reader["sent_date"]) != "")
                                {
                                    ms.SendDate = Convert.ToDateTime(reader["sent_date"]).ToString("dd/MM/yyyy HH:mm");
                                }
                                ms.CustomerMessage = Convert.ToString(reader["description_Ascii"]);
                                if (Convert.ToString(reader["set_number"]) != "")
                                {
                                    ms.SetNumber = Convert.ToInt32(reader["set_number"]);
                                }
                                if (Convert.ToString(reader["SentRecieve"]) != "")
                                {
                                    ms.SentRecieve = Convert.ToInt32(reader["SentRecieve"]);
                                }
                                ms.MessageStatus = Convert.ToInt32(reader["status"]);

                                msg.Add(ms);
                            }
                        }
                        reader.Close();


                        _cmd.CommandType = CommandType.Text;
                        _cmd.CommandText = countQuery;
                        _cmd.Parameters.Clear();
                        _cmd.Parameters.Add("@operDay", SqlDbType.NVarChar).Value = operDay.ToString("dd/MMM/yyyy");

                        reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToString(reader["count"]) != "")
                                {
                                    msg[0].MessagesCount = Convert.ToInt32(reader["count"]);
                                }
                            }
                        }


                        _con.Close();

                        if (msg.Count != 0)
                        {
                            msg[0].File = GetMessageUploadedFilesList(msg[0].ID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;

                }

            }

            return msg;

        }

        internal static List<HBMessages> GetSearchedHBMessages(HBMessagesSreach obj, ushort filalCode, string WatchAllMessages)
        {
            List<HBMessages> msg = new List<HBMessages>();
            DateTime operDay = Utility.GetNextOperDay().Date;
            string filter = GetFilterstringForHBMessages(obj);

            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string countQuery = GetFilterForHBMessagesCount(obj);

                string query = "Select ms.*,dbo.getascii(ms.Subject) as Subject_Ascii, dbo.getascii(ms.description) as description_Ascii " +
                    ",case when isnull(c.description,'')<> '' then c.description else name + ' ' + lastname end As name_lastname " +
                    " from(  " + filter + " ) ms " +
                    "CROSS APPLY (Select * From V_CustomerDesription " +
                    "where customer_number = ms.customer_number and(filialcode = @filalCode or 1 = @WatchAllMessages)) c order by ms.sent_date desc";


                try
                {
                    using SqlCommand _cmd = new SqlCommand(query, _con);
                    _con.Open();
                    _cmd.CommandType = CommandType.Text;
                    //_cmd.Parameters.Add("@filter", SqlDbType.NVarChar).Value = filter;
                    _cmd.Parameters.Add("@WatchAllMessages", SqlDbType.Bit).Value = Convert.ToInt16(WatchAllMessages);
                    _cmd.Parameters.Add("@filalCode", SqlDbType.Int).Value = filalCode;

                    _cmd.CommandTimeout = 120;
                     SqlDataReader reader = _cmd.ExecuteReader();

                    int rowCount = 0;

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (rowCount >= obj.firstRow && rowCount < obj.LastGetRowCount)
                            {
                                HBMessages ms = new HBMessages();
                                ms.ID = Convert.ToInt64(reader["Id"]);
                                ms.FullName = Convert.ToString(reader["name_lastname"]);
                                ms.CustomerNumber = Convert.ToUInt64(reader["customer_number"]);
                                ms.CustomerSubject = Convert.ToString(reader["Subject_Ascii"]);
                                if (Convert.ToString(reader["sent_date"]) != "")
                                {
                                    ms.SendDate = Convert.ToDateTime(reader["sent_date"]).ToString("dd/MM/yyyy HH:mm");
                                }
                                ms.CustomerMessage = Convert.ToString(reader["description_Ascii"]);
                                if (Convert.ToString(reader["set_number"]) != "")
                                {
                                    ms.SetNumber = Convert.ToInt32(reader["set_number"]);
                                }
                                if (Convert.ToString(reader["SentRecieve"]) != "")
                                {
                                    ms.SentRecieve = Convert.ToInt32(reader["SentRecieve"]);
                                }
                                ms.MessageStatus = Convert.ToInt32(reader["status"]);

                                msg.Add(ms);

                                rowCount++;
                            }
                            else if (rowCount > obj.LastGetRowCount)
                            {
                                break;
                            }
                            else
                            {
                                rowCount++;
                            }
                        }
                    }
                    reader.Close();

                    _cmd.CommandType = CommandType.Text;
                    _cmd.CommandText = countQuery;
                    reader = _cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToString(reader["count"]) != "")
                            {
                                msg[0].MessagesCount = Convert.ToInt32(reader["count"]);
                            }
                        }
                    }


                    _con.Close();

                    if (msg.Count != 0)
                    {

                        msg[0].File = GetMessageUploadedFilesList(msg[0].ID);
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;

                }

            }


            return msg;
        }

        internal static string PostMessageAsRead(long msgId, int setNumber)
        {
            string result = string.Empty;

            if (msgId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select status,set_number,sentrecieve FROM Tbl_messages_with_bank WHERE id = @msgId";

                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                    {
                        _con.Open();
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@msgId", SqlDbType.Int).Value = msgId;

                       using SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToInt32(reader["status"]) == 0 || (Convert.ToInt32(reader["status"]) == -1 && Convert.ToInt32(reader["set_number"]) != 0) && Convert.ToInt32(reader["sentrecieve"]) == 1)
                                {
                                    result = "Հաղորդագրությունը արդեն կարդացված է";
                                    return result;
                                }
                            }
                        }
                    }

                    try
                    {
                        query = "Update Tbl_messages_with_bank set status=0, set_number= @set_number where id= @msgId";

                        using (SqlCommand _cmd = new SqlCommand(query, _con))
                        {
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@msgId", SqlDbType.Int).Value = msgId;
                            _cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;

                            _cmd.ExecuteNonQuery();

                        }
                    }
                    catch (Exception ex)
                    {
                        _con.Close();
                        result = ex.Message;
                        return result;
                    }

                    _con.Close();
                }


            }

            return result;
        }

        internal static string PostSentMessageToCustomer(HBMessages obj)
        {
            string json = string.Empty;
            int receiverType = 0;

            if (obj != null)
            {
                switch (obj.CustomerType)
                {
                    case 1:
                        receiverType = 3;
                        break;
                    case 2:
                        receiverType = 2;
                        break;
                    case 3:
                        receiverType = 1;
                        break;
                }

                switch (obj.OperationType)
                {
                    case 1:
                        if (obj.CustomerNumber != 0)
                        {
                            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                            {
                                string query = "Insert into Tbl_messages_with_bank(customer_number,subject,description,sent_date,reply_id,status,set_number,sentrecieve) " +
                                    "values( @custNum, @subject, @msg, @datetime, @id, 1, @setNum, 2)";

                                try
                                {
                                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                                    {
                                        _con.Open();
                                        _cmd.CommandType = CommandType.Text;
                                        _cmd.Parameters.Add("@custNum", SqlDbType.Float).Value = obj.CustomerNumber;
                                        _cmd.Parameters.Add("@subject", SqlDbType.NVarChar).Value = obj.Subject;
                                        _cmd.Parameters.Add("@msg", SqlDbType.NVarChar).Value = obj.Message;
                                        _cmd.Parameters.Add("@datetime", SqlDbType.NVarChar).Value = DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss");
                                        _cmd.Parameters.Add("@id", SqlDbType.Int).Value = obj.ID;
                                        _cmd.Parameters.Add("@setNum", SqlDbType.Int).Value = obj.SetNumber;

                                        _cmd.ExecuteNonQuery();

                                        _con.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _con.Close();
                                    json = ex.Message;
                                    return json;
                                }

                            }
                        }
                        else
                        {
                            json = "Ընտրեք հաճախորդին";
                            return json;
                        }
                        break;
                    case 2:
                        if (obj.CustomerNumber != 0)
                        {
                            try
                            {
                                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                                {
                                    string query = "select * from dbo.[Tbl_HB_Users] where customer_number= @custNum ";

                                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                                    {
                                        _con.Open();
                                        _cmd.CommandType = CommandType.Text;
                                        _cmd.Parameters.Add("@custNum", SqlDbType.Float).Value = obj.CustomerNumber;
                                        SqlDataReader reader = _cmd.ExecuteReader();

                                        if (!reader.HasRows)
                                        {
                                            _con.Close();
                                            json = "Հաճախորդը գտնված չէ HB հաճախորդների ցուցակում";
                                            return json;
                                        }

                                    }

                                }
                                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                                {
                                    string query = "Insert into dbo.Tbl_messages_with_bank(customer_number,[description],sent_date,reply_id,[status],set_number,sentrecieve,subject)" +
                                        "values( @custNum, @msg, @datetime, 0, 1, @setNum, 2, @subject)";

                                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                                    {
                                        _con.Open();
                                        _cmd.CommandType = CommandType.Text;
                                        _cmd.Parameters.Add("@custNum", SqlDbType.Float).Value = obj.CustomerNumber;
                                        _cmd.Parameters.Add("@msg", SqlDbType.NVarChar).Value = obj.Message;
                                        _cmd.Parameters.Add("@datetime", SqlDbType.NVarChar).Value = DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss");
                                        _cmd.Parameters.Add("@setNum", SqlDbType.Int).Value = obj.SetNumber;
                                        _cmd.Parameters.Add("@subject", SqlDbType.NVarChar).Value = obj.Subject;

                                        _cmd.ExecuteNonQuery();

                                        _con.Close();
                                    }

                                }
                                //using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                                //{
                                //    string query = "pr_insert_push_notification";
                                //    using (SqlCommand _cmd = new SqlCommand(query, _con))
                                //    {
                                //        _con.Open();

                                //        _cmd.CommandType = CommandType.StoredProcedure;
                                //        _cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = DBNull.Value;
                                //        _cmd.Parameters.Add("@notification_type", SqlDbType.Int).Value = 4;
                                //        _cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = obj.CustomerNumber;
                                //        _cmd.Parameters.Add("@send_type", SqlDbType.TinyInt).Value = 4;
                                //        _cmd.Parameters.Add("@new_proj_message", SqlDbType.NVarChar).Value = obj.Message;

                                //        _cmd.ExecuteNonQuery();


                                //        _con.Close();
                                //    }

                                //}
                            }
                            catch (Exception ex)
                            {
                                json = ex.Message;
                                return json;
                            }
                        }
                        else
                        {
                            json = "Ընտրեք հաճախորդին";
                            return json;
                        }
                        break;
                    case 3:
                        try
                        {
                            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                            {

                                string query = "Insert into dbo.[Tbl_messages_with_bank](customer_number,[description],sent_date,reply_id,[status],set_number,sentrecieve,subject) " +
                                    "select u.customer_number, @msg , @datetime ,0,1, @setNum ,2, @subject" +
                                    " from [Tbl_HB_Users] u inner join Tbl_customers c on u.customer_number = c.customer_number" +
                                    " where(@customerType = 2 and c.type_of_client = 6) or( @customerType = 1 and c.type_of_client <> 6)  " +
                                    "or(@customerType = 0)";


                                using (SqlCommand _cmd = new SqlCommand(query, _con))
                                {
                                    _con.Open();
                                    _cmd.CommandType = CommandType.Text;
                                    _cmd.Parameters.Add("@msg", SqlDbType.NVarChar).Value = obj.Message;
                                    _cmd.Parameters.Add("@datetime", SqlDbType.NVarChar).Value = DateTime.Now.ToString("dd/MMM/yyyy hh:mm:ss");
                                    _cmd.Parameters.Add("@setNum", SqlDbType.Int).Value = obj.SetNumber;
                                    _cmd.Parameters.Add("@subject", SqlDbType.NVarChar).Value = obj.Subject;
                                    _cmd.Parameters.Add("@customerType", SqlDbType.Int).Value = obj.CustomerType;

                                    _cmd.ExecuteNonQuery();

                                    _con.Close();
                                }

                            }
                            //using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                            //{

                            //    string query = "pr_insert_push_notification";

                            //    using (SqlCommand _cmd = new SqlCommand(query, _con))
                            //    {
                            //        _con.Open();

                            //        _cmd.CommandType = CommandType.StoredProcedure;
                            //        _cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = DBNull.Value;
                            //        _cmd.Parameters.Add("@notification_type", SqlDbType.Int).Value = 4;
                            //        _cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = DBNull.Value;
                            //        _cmd.Parameters.Add("@send_type", SqlDbType.TinyInt).Value = receiverType;
                            //        _cmd.Parameters.Add("@new_proj_message", SqlDbType.NVarChar).Value = obj.Message;
                            //        _cmd.ExecuteNonQuery();

                            //        _con.Close();
                            //    }

                            //}
                        }
                        catch (Exception ex)
                        {
                            json = ex.Message;
                            return json;
                        }
                        break;
                }
            }

            return json;
        }

        internal static HBMessageFiles GetMsgSelectedFile(int fileId)
        {
            HBMessageFiles file = new HBMessageFiles();
            if (fileId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select Id, FileName, FileContent, FileType, RegDate FROM  Tbl_Messages_Uploaded_files where Id= @fileId";

                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                    {
                        _con.Open();
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@fileId", SqlDbType.Int).Value = fileId;
                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                file.Id = Convert.ToInt32(reader["Id"]);
                                file.FileName = Convert.ToString(reader["FileName"]);
                                file.FileContent = (byte[])reader["FileContent"];
                                file.FileType = Convert.ToString(reader["FileType"]);
                                file.RegistrationDate = Convert.ToDateTime(reader["RegDate"]).ToString("dd/MM/yyyy");


                            }
                        }

                        _con.Close();
                    }

                }
            }

            return file;
        }
        internal static List<HBMessageFiles> GetMessageUploadedFilesList(long msgId, bool showUploadFilesContent = false)
        {
            List<HBMessageFiles> files = new List<HBMessageFiles>();
            if (msgId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    string query = "Select Id, FileName, FileContent, FileType, RegDate FROM  Tbl_Messages_Uploaded_files where Msg_id= @msgId";

                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                    {
                        _con.Open();
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@msgId", SqlDbType.Int).Value = msgId;
                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                HBMessageFiles file = new HBMessageFiles();
                                file.Id = Convert.ToInt32(reader["Id"]);
                                file.FileName = Convert.ToString(reader["FileName"]);
                                if (showUploadFilesContent == true)
                                { file.FileContent = reader["FileContent"] != DBNull.Value ? (byte[])reader["FileContent"] : null; }
                                file.FileType = Convert.ToString(reader["FileType"]);
                                switch (file.FileType.ToLower())
                                {
                                    case "pdf":
                                    case "doc":
                                    case "docx":
                                    case "xls":
                                    case "xlsx":
                                        file.FileType = "." + file.FileType;
                                        break;
                                }
                                file.RegistrationDate = Convert.ToDateTime(reader["RegDate"]).ToString("dd/MM/yyyy");

                                files.Add(file);
                            }
                        }

                        _con.Close();
                    }

                }
            }

            return files;
        }

        internal static bool CheckReestrTransactionIsChecked(long docId)
        {
            bool isChecked = false;
            DateTime operDay = Utility.GetNextOperDay().Date;

            if (docId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    try
                    {
                        _con.Open();

                        using (SqlCommand _cmd = _con.CreateCommand())
                        {
                            string query = "Select 1 FROM Tbl_transactions_from_excel Left join (select * from Tbl_acc_freeze_history where closing_date is null and reason_type = 11) " +
                                "f on Tbl_transactions_from_excel.ID = f.add_table_id WHERE  " +
                                "Tbl_transactions_from_excel.[check] is not null and Tbl_transactions_from_excel.HB_doc_ID = @docId ORDER BY number_in_list";

                            _cmd.CommandText = query;
                            _cmd.CommandType = CommandType.Text;
                            _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;

                            SqlDataReader reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                isChecked = true;
                            }
                        }
                        _con.Close();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }

                }
            }

            return isChecked;
        }

        internal static string GetcheckedReestrTransferDetails(long docId)
        {
            string error = string.Empty;
            List<ReestrTransferAdditionalDetails> details = new List<ReestrTransferAdditionalDetails>();
            if (docId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    _con.Open();

                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string query = "Select 1 FROM Tbl_transactions_from_excel Left join (select * from Tbl_acc_freeze_history where closing_date is null and reason_type = 11) " +
                                "f on Tbl_transactions_from_excel.ID = f.add_table_id WHERE " +
                                "Tbl_transactions_from_excel.[check] is null and Tbl_transactions_from_excel.confirmation_date is null and Tbl_transactions_from_excel.HB_doc_ID = @docId ORDER BY number_in_list";

                        _cmd.CommandText = query;
                        _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                        _cmd.CommandType = CommandType.Text;

                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            error = "Գործարքը ստուգված չէ։";
                            return error;
                        }
                        else
                        {
                            query = "Select number_in_list,[check],credit_account,name_our FROM Tbl_transactions_from_excel Left join " +
                                "(select * from Tbl_acc_freeze_history where closing_date is null and reason_type = 11) f on Tbl_transactions_from_excel.ID = f.add_table_id "
                                + " WHERE  Tbl_transactions_from_excel.confirmation_date is null and Tbl_transactions_from_excel.HB_doc_ID = @docId  ORDER BY number_in_list";

                            reader.Close();

                            _cmd.CommandText = query;
                            _cmd.Parameters.Clear();
                            _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;

                            _cmd.CommandType = CommandType.Text;

                            reader = _cmd.ExecuteReader();

                            if (reader.HasRows)
                            {
                                int i = 0;
                                while (reader.Read())
                                {
                                    ulong customerNum = AccountDB.GetAccountCustomerNumber(Convert.ToString(reader["credit_account"]));
                                    bool hasDAHK = ValidationDB.IsDAHKAvailability(customerNum);

                                    if (Convert.ToString(reader["check"]) == "" && hasDAHK == true)
                                    {
                                        ReestrTransferAdditionalDetails reestr = new ReestrTransferAdditionalDetails();
                                        reestr.Index = Convert.ToInt32(reader["number_in_list"]);
                                        reestr.CreditAccount = new Account();
                                        reestr.CreditAccount.AccountNumber = Convert.ToString(reader["credit_account"]);
                                        reestr.Description = Convert.ToString(reader["name_our"]);
                                        reestr.HbDAHKCheckResult = true;

                                        details.Add(reestr);
                                    }
                                }
                            }
                            else
                            {
                                error = "Գործարքը ստուգված չէ։";
                                return error;
                            }
                        }
                    }

                    _con.Close();

                }

                if (details.Count > 0)
                {
                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                    {
                        _con.Open();
                        for (int i = 0; i < details.Count; i++)
                        {
                            if (details[i].HbDAHKCheckResult == true)
                            {
                                using (SqlCommand _cmd = _con.CreateCommand())
                                {
                                    string query = "Select 1 FROM dbo.Tbl_HB_Transfers_Registry WHERE docID = docID and credit_account = @credit_account AND payment_type IS NULL";

                                    _cmd.CommandText = query;
                                    _cmd.CommandType = CommandType.Text;
                                    _cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docId;
                                    _cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar).Value = details[i].CreditAccount.AccountNumber;

                                    SqlDataReader reader = _cmd.ExecuteReader();

                                    if (reader.HasRows)
                                    {
                                        error = "§" + details[i].Index.ToString() + "¦ տողի արգելադրման նպատակը ընտրված չէ";
                                        return error;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return error;
        }

        internal static List<ReestrTransferAdditionalDetails> GetTransactionIsChecked(long orderId, List<ReestrTransferAdditionalDetails> details)
        {
            DateTime operDay = Utility.GetNextOperDay().Date;

            if (orderId != 0 && details != null)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    _con.Open();

                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string query = "Select number_in_list,cast(credit_account as bigint) as credit_account_bigint,amount,[check] FROM Tbl_transactions_from_excel Left join " +
                            "(select * from Tbl_acc_freeze_history where closing_date is null and reason_type = 11) f on Tbl_transactions_from_excel.ID = f.add_table_id " +
                            " WHERE  Tbl_transactions_from_excel.HB_doc_ID = @orderId ORDER BY number_in_list ";

                        _cmd.CommandText = query;
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;

                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < details.Count; i++)
                                {
                                    if (Convert.ToInt32(reader["number_in_list"]) == details[i].Index && Convert.ToString(reader["credit_account_bigint"]) == details[i].CreditAccount.AccountNumber && Convert.ToDouble(reader["amount"]) == details[i].Amount)
                                    {
                                        details[i].HBCheckResult = Convert.ToString(reader["check"]);
                                        string creditAccount = Convert.ToString(reader["credit_account_bigint"]);
                                        ulong customerNum = AccountDB.GetAccountCustomerNumber(creditAccount);
                                        details[i].HbDAHKCheckResult = ValidationDB.IsDAHKAvailability(customerNum);

                                        break;
                                    }

                                }
                            }
                        }


                    }
                    _con.Close();

                }

                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    _con.Open();

                    using (SqlCommand _cmd = _con.CreateCommand())
                    {
                        string query = "Select IndexID,payment_type FROM Tbl_HB_Transfers_Registry WHERE docID = @docId";

                        _cmd.CommandText = query;
                        _cmd.CommandType = CommandType.Text;
                        _cmd.Parameters.Add("@docId", SqlDbType.Int).Value = orderId;

                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < details.Count; i++)
                                {
                                    if (details[i].Index == Convert.ToInt32(reader["IndexID"]))
                                    {
                                        if (Convert.ToString(reader["payment_type"]) != "")
                                        {
                                            details[i].PaymentType = Convert.ToUInt16(reader["payment_type"]);
                                        }
                                        else
                                        {
                                            details[i].PaymentType = 0;
                                        }
                                    }
                                }

                            }
                        }


                    }
                    _con.Close();

                }

                foreach (ReestrTransferAdditionalDetails detail in details)
                {
                    if (detail.HbDAHKCheckResult == false && detail.PaymentType != 0)
                    {
                        detail.HbDAHKCheckResult = true;
                    }
                    else if (detail.HbDAHKCheckResult == true && detail.PaymentType == 0)
                    {
                        detail.HbDAHKCheckResult = false;
                    }
                }
            }

            return details;
        }


        internal static string PostBypassHistory(HBDocumentBypassTransaction obj)
        {
            string done = string.Empty;


            if (obj != null)
            {
                try
                {
                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString());
                    _con.Open();
                    using (SqlTransaction sqlTrans = _con.BeginTransaction())
                    {
                        obj.HBBypass.ForEach(hb =>
                        {
                            string current_sp = "pr_Insert_Bypass_History";
                            using SqlCommand _cmd = new SqlCommand(current_sp, _con, sqlTrans);
                            _cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = obj.DocID;
                            _cmd.Parameters.Add("@bypass_Id", SqlDbType.Int).Value = hb.ID;
                            _cmd.Parameters.Add("@is_checked", SqlDbType.Bit).Value = hb.IsChecked;
                            _cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = obj.SetNumber;


                            _cmd.ExecuteNonQuery();
                        });

                        sqlTrans.Commit();
                    }

                    _con.Close();
                }
                catch (Exception ex)
                {
                    done = ex.Message;
                }
            }

            return done;

        }

        internal static string PostApproveUnconfirmedOrder(long docId, int setNumber)
        {
            string result = string.Empty;

            if (docId != 0)
            {
                try
                {
                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                    {
                        _con.Open();
                        string current_sp = "pr_Insert_Bypass_History";

                        using (SqlCommand _cmd = new SqlCommand(current_sp, _con))
                        {
                            _cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docId;
                            _cmd.Parameters.Add("@bypass_Id", SqlDbType.Int).Value = 3;
                            _cmd.Parameters.Add("@is_checked", SqlDbType.Bit).Value = 1;
                            _cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;

                            _cmd.ExecuteNonQuery();
                        }
                        _con.Close();
                    }
                    using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                    {
                        _con.Open();
                        using (SqlCommand _cmd = _con.CreateCommand())
                        {
                            string current_sp = "pr_confirm_online_transfers";
                            _cmd.CommandText = current_sp;
                            _cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            _cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetNextOperDay();
                            _cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;
                            _cmd.Parameters.Add("@itemNumber", SqlDbType.Int).Direction = ParameterDirection.Output;
                            _cmd.Parameters.Add("@operationProcessingType", SqlDbType.Int).Value = 0;
                            _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docId;


                            _cmd.ExecuteNonQuery();

                        }

                        _con.Close();
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }

            return result;
        }

        internal static string GetcheckedArmTransferDetails(long docId)
        {
            string flag = string.Empty;
            string accountNumber = string.Empty;
            ulong customerNumber = 0;

            if (docId != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    _con.Open();
                    string query = "SELECT CASE WHEN len(credit_account) > 10 THEN credit_account ELSE cast(credit_bank_code as nvarchar(50)) +cast(credit_account as nvarchar(50)) END AS account" +
                                   " FROM [dbo].[Tbl_HB_documents] where doc_ID = @doc_ID";

                    using (SqlCommand _cmd = new SqlCommand(query, _con))
                    {
                        _cmd.CommandType = System.Data.CommandType.Text;

                        _cmd.Parameters.Add("@doc_ID", SqlDbType.Int).Value = docId;

                        SqlDataReader reader = _cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {
                                if (Convert.ToString(reader["account"]) != "")
                                {
                                    accountNumber = Convert.ToString(reader["account"]);
                                }
                            }
                        }
                    }
                    _con.Close();
                }

                customerNumber = AccountDB.GetAccountCustomerNumber(accountNumber);

                if (ValidationDB.IsDAHKAvailability(customerNumber))
                {

                    using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                    string query = "SELECT 1 FROM [dbo].[tbl_DAHK_bypass_of_HB_transfers] WHERE Doc_Id = @doc_ID";
                    using SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@doc_ID", SqlDbType.BigInt).Value = docId;

                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        flag = "Արգելադրման նպատակը ընտրված չէ";
                    }
                }
            }

            return flag;
        }


        //function

        static string GetFilterstringForHBMessages(HBMessagesSreach obj)
        {
            string filter = "select ms.* from Tbl_messages_with_bank ms where ms.customer_number<>0 ";

            if (obj.CustomerNumber != 0)
            {
                filter += " and ms.customer_number= " + obj.CustomerNumber.ToString();
            }

            if (obj.StartDate != "" && obj.EndDate == "" && obj.StartDate != null && obj.EndDate == null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)='" + Convert.ToDateTime(obj.StartDate).ToString("dd/MMM/yyyy");
            }

            if (obj.StartDate == "" && obj.EndDate != "" && obj.StartDate == null && obj.EndDate != null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)='" + Convert.ToDateTime(obj.EndDate).ToString("dd/MMM/yyyy");
            }

            if (obj.StartDate != "" && obj.EndDate != "" && obj.StartDate != null && obj.EndDate != null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)>='" + Convert.ToDateTime(obj.StartDate).ToString("dd/MMM/yyyy") + "' and convert(datetime,convert(nvarchar,sent_date,103),103)<='" + Convert.ToDateTime(obj.EndDate).ToString("dd/MMM/yyyy") + "'";
            }

            if (obj.ReceivedOrSentMsg != null)
            {
                filter += " and sentrecieve='" + obj.ReceivedOrSentMsg.ToString() + "'";
            }

            if (obj.ReadOrUnReadMsg != null && obj.ReceivedOrSentMsg == 1)
            {
                if (obj.ReadOrUnReadMsg == 0)
                {
                    filter += " and (status =0  or (status =-1 and set_number <> 0)) ";
                }
                else if (obj.ReadOrUnReadMsg == 1)
                {
                    filter += " and status =1 ";
                }
            }

            return filter;
        }

        static string GetFilterForHBMessagesCount(HBMessagesSreach obj)
        {
            string filter = "select COUNT(id) count from Tbl_messages_with_bank ms where ms.customer_number<>0 ";

            if (obj.CustomerNumber != 0)
            {
                filter += " and ms.customer_number= " + obj.CustomerNumber.ToString();
            }

            if (obj.StartDate != "" && obj.EndDate == "" && obj.StartDate != null && obj.EndDate == null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)='" + Convert.ToDateTime(obj.StartDate).ToString("dd/MMM/yyyy");
            }

            if (obj.StartDate == "" && obj.EndDate != "" && obj.StartDate == null && obj.EndDate != null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)='" + Convert.ToDateTime(obj.EndDate).ToString("dd/MMM/yyyy");
            }

            if (obj.StartDate != "" && obj.EndDate != "" && obj.StartDate != null && obj.EndDate != null)
            {
                filter += " and convert(datetime,convert(nvarchar(11),sent_date,103),103)>='" + Convert.ToDateTime(obj.StartDate).ToString("dd/MMM/yyyy") + "' and convert(datetime,convert(nvarchar,sent_date,103),103)<='" + Convert.ToDateTime(obj.EndDate).ToString("dd/MMM/yyyy") + "'";
            }

            if (obj.ReceivedOrSentMsg != null)
            {
                filter += " and sentrecieve='" + obj.ReceivedOrSentMsg.ToString() + "'";
            }

            if (obj.ReadOrUnReadMsg != null && obj.ReceivedOrSentMsg == 1)
            {
                if (obj.ReadOrUnReadMsg == 0)
                {
                    filter += " and (status =0  or (status =-1 and set_number <> 0)) ";
                }
                else if (obj.ReadOrUnReadMsg == 1)
                {
                    filter += " and status =1 ";
                }
            }

            return filter;
        }

        static int LockingTransactions(int fileIndex)
        {
            int lockResult = 0;
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string current_sp = "pr_Locking_Unlocking_transactions";
                _con.Open();
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    da.SelectCommand = new SqlCommand(current_sp, _con);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;

                    da.SelectCommand.Parameters.Add("@rowID", SqlDbType.Int).Value = fileIndex;
                    da.SelectCommand.Parameters.Add("@lockSignNeed", SqlDbType.TinyInt).Value = 1;

                    da.SelectCommand.Parameters.Add("@lockResult", SqlDbType.TinyInt).Value = ParameterDirection.Output;

                    using SqlDataReader reader = da.SelectCommand.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToString(reader["lockResult"]) != "")
                            {
                                lockResult = Convert.ToInt32(reader["lockResult"]);
                            }
                        }
                    }
                }
                _con.Close();
            }
            return lockResult;
        }
        static void UnlockingTransactions(int fileIndex)
        {
            int lockResult = 0;

            using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
            string current_sp = "pr_Locking_Unlocking_transactions";
            _con.Open();
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                da.SelectCommand = new SqlCommand(current_sp, _con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.Add("@rowID", SqlDbType.Int).Value = fileIndex;
                da.SelectCommand.Parameters.Add("@lockSignNeed", SqlDbType.TinyInt).Value = 0;

                da.SelectCommand.Parameters.Add("@lockResult", SqlDbType.TinyInt).Value = ParameterDirection.Output;

                using SqlDataReader reader = da.SelectCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (Convert.ToString(reader["lockResult"]) != "")
                        {
                            lockResult = Convert.ToInt32(reader["lockResult"]);
                        }
                    }
                }
            }
            _con.Close();

        }



        static string GetFilter(HBDocumentFilters obj)
        {
            string filter = string.Empty;

            if (obj.QualityType != null)
            {
                filter = ((obj.QualityType == 3) ? " h.quality in (3,20)" : " h.quality = " + obj.QualityType.ToString());

            }
            else
            {
                filter = " h.quality<>40 and h.quality<>1 ";
            }

            filter += " AND h.document_type not in (79, 209, 210, 211, 212, 77, 207, 29,137,73,30, 223, 138, 135, 132, 69, 120, 119, 238, 228, 242, 245, 246, 247, 254, 191) ";

            if (obj.OnlyACBA == 1)
            {
                if (obj.DocumentType != null)
                {
                    if (obj.DocumentType == 1)
                    {
                        filter += " and h.credit_bank_code>=22000 and h.credit_bank_code<22100 and h.document_subtype<>1 ";
                    }
                    else
                    {
                        if (obj.DocumentType == 3)
                        {
                            filter += " and h.document_type<>3";
                        }
                    }
                }
                else
                {
                    filter += " and ((h.credit_bank_code>=22000 and h.credit_bank_code<22100 and h.document_type=1 and  h.document_subtype<>1 ) or h.document_type not in (1,3,56,64))";
                }
            }

            if (obj.DocumentSubType != null)
            {
                if (obj.DocumentSubType == 2) //Bank Mail
                {
                    filter += " and h.document_type in (1,56) and h.document_subtype in (2,5,6)";
                }
                else
                {
                    filter += " and h.document_type =1 and h.document_subtype =1";
                }
            }

            if (obj.TransactionCode != null)
            {
                filter += " and h.Doc_ID =" + obj.TransactionCode.ToString();
            }

            if (obj.StartDate != "" && obj.StartDate != null)
            {
                DateTime dt = Convert.ToDateTime(obj.StartDate);
                string startDate = dt.ToString("dd/MMM/yy");

                filter += " and q.change_date >='" + startDate + "'";
            }

            if (obj.EndDate != "" && obj.EndDate != null)
            {
                DateTime dt = Convert.ToDateTime(obj.EndDate).AddDays(1);
                string endDate = dt.ToString("dd/MMM/yy");

                filter += " and q.change_date <='" + endDate + "'";
            }

            if (obj.CustomerNumber != null)
            {
                filter += " and h.customer_number =" + obj.CustomerNumber.ToString();
            }

            if (obj.Amount != null)
            {
                filter += " and h.amount =" + obj.Amount.ToString();
            }

            if (obj.DocumentType != null)
            {
                filter += " and h.document_type=" + obj.DocumentType.ToString();
            }

            if (obj.FilialCode != null)
            {
                filter += " and h.filial =" + obj.FilialCode.ToString();
            }

            if (obj.CurrencyType != "" && obj.CurrencyType != null)
            {
                filter += " and h.currency ='" + obj.CurrencyType + "'";
            }

            if (obj.DebitAccount != null)
            {
                filter += " and h.debet_account =" + obj.DebitAccount.ToString();
            }

            if (obj.SourceType != null)
            {
                filter += " and source_type =" + obj.SourceType.ToString();
            }

            if (obj.Description != "" && obj.Description != null)
            {
                filter += " and dbo.getascii(h.[description]) like N'%" + obj.Description.Trim() + "%'  ";
            }

            if (obj.BankCode != 22000 && obj.FilialCode == null)
            {
                filter += " and h.filial =" + obj.BankCode.ToString();
            }

            return filter;
        }

    }
}
