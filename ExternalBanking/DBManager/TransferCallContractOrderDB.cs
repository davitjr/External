using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using ExternalBanking.ACBAServiceReference;


namespace ExternalBanking.DBManager
{
    class TransferCallContractOrderDB
    {
        internal static ActionResult Save(TransferCallContractOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewTransferCallContractDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@contract_number", SqlDbType.Int).Value = order.TransferCallContractDetails.ContractNumber;
                    cmd.Parameters.Add("@contract_password", SqlDbType.NVarChar, 50).Value = order.TransferCallContractDetails.ContractPassword;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar, 50).Value = order.TransferCallContractDetails.Account.AccountNumber;
                    cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime, 50).Value = order.TransferCallContractDetails.ContractDate;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = order.TransferCallContractDetails.InvolvingSetNumber;
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

        /// <summary>
        /// Վերադարձնում է հեռախոսազանգով փոխանցման համաձայնագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static TransferCallContractOrder Get(TransferCallContractOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT registration_date,document_number,customer_number,document_type,document_subtype,quality,D.operation_date,C.*                                             
		                                           FROM Tbl_HB_documents D INNER JOIN tbl_new_transfer_call_contract_documents  C ON D.doc_ID=C.Doc_ID
                                                   WHERE D.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.TransferCallContractDetails = new TransferCallContractDetails();
                order.TransferCallContractDetails.ContractNumber = long.Parse(dt.Rows[0]["contract_number"].ToString());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.TransferCallContractDetails.ContractNumber = long.Parse(dt.Rows[0]["contract_number"].ToString());
                order.TransferCallContractDetails.ContractPassword = dt.Rows[0]["contract_password"].ToString();
                order.TransferCallContractDetails.ContractDate = Convert.ToDateTime(dt.Rows[0]["contract_date"]);
                order.TransferCallContractDetails.Account = Account.GetAccount(dt.Rows[0]["account_number"].ToString());
                order.TransferCallContractDetails.InvolvingSetNumber = int.Parse(dt.Rows[0]["involving_set_number"].ToString());
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

            }
            return order;
        }

        internal static bool IsSecondActivation(TransferCallContractOrder order)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select  C.doc_id from Tbl_HB_documents D INNER JOIN tbl_new_transfer_call_contract_documents C ON D.doc_ID=C.Doc_ID
                                                WHERE quality in (1,2,3,5) and document_type=96 and document_subtype=1 and
                                                customer_number=@customerNumber and C.account_number=@accountNumber", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.TransferCallContractDetails.Account.AccountNumber;

               using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }


        internal static ActionResult SaveTerminationOrder(TransferCallContractTerminationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_addNewTransferCallContractDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = order.TransferCallContractDetails.ContractId;
                    cmd.Parameters.Add("@contract_number", SqlDbType.Int).Value = order.TransferCallContractDetails.ContractNumber;
                    cmd.Parameters.Add("@contract_password", SqlDbType.NVarChar, 50).Value = order.TransferCallContractDetails.ContractPassword;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar, 50).Value = order.TransferCallContractDetails.Account.AccountNumber;
                    cmd.Parameters.Add("@contract_date", SqlDbType.SmallDateTime).Value = order.TransferCallContractDetails.ContractDate;
                    cmd.Parameters.Add("@closing_date", SqlDbType.SmallDateTime).Value = order.TransferCallContractDetails.ClosingDate;
                    cmd.Parameters.Add("@involving_set_number", SqlDbType.Int).Value = order.TransferCallContractDetails.InvolvingSetNumber;
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

        /// <summary>
        /// Վերադարձնում է հեռախոսազանգով փոխանցման համաձայնագրի դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static TransferCallContractTerminationOrder GetTerminationOrder(TransferCallContractTerminationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT registration_date,document_number,customer_number,document_type,document_subtype,quality,D.operation_date,C.*                                             
		                                           FROM Tbl_HB_documents D INNER JOIN tbl_new_transfer_call_contract_documents  C ON D.doc_ID=C.Doc_ID
                                                   WHERE D.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                order.TransferCallContractDetails = new TransferCallContractDetails();
                order.TransferCallContractDetails.ContractNumber = long.Parse(dt.Rows[0]["contract_number"].ToString());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.TransferCallContractDetails.ContractNumber = long.Parse(dt.Rows[0]["contract_number"].ToString());
                order.TransferCallContractDetails.ContractPassword = dt.Rows[0]["contract_password"].ToString();
                order.TransferCallContractDetails.ContractDate = Convert.ToDateTime(dt.Rows[0]["contract_date"]);
                order.TransferCallContractDetails.Account = Account.GetAccount(dt.Rows[0]["account_number"].ToString());
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.TransferCallContractDetails.ClosingDate = dt.Rows[0]["closing_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["closing_date"]) : default(DateTime?);
                order.TransferCallContractDetails.InvolvingSetNumber = int.Parse(dt.Rows[0]["involving_set_number"].ToString());
            }
            return order;
        }

        internal static bool IsSecondTermination(TransferCallContractTerminationOrder order)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select C.doc_id from Tbl_HB_documents D INNER JOIN tbl_new_transfer_call_contract_documents C ON D.doc_ID=C.Doc_ID
                                                WHERE quality in (1,2,3,5) and document_type=97 and document_subtype=1 and
                                                customer_number=@customerNumber and C.account_number=@accountNumber", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.TransferCallContractDetails.Account.AccountNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }

        internal static bool IsSameAccountContract(TransferCallContractOrder order)
        {

            bool check = false;
            using(SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT account_number FROM Tbl_contracts_for_transfers_by_call WHERE account_number =@accountNumber and quality = 0 and deleted = 0 and isnull(card_number, '') = ''", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = order.TransferCallContractDetails.Account.AccountNumber;

              using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }

        internal static bool IsSameNumberContract(TransferCallContractOrder order)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT contract_number FROM Tbl_contracts_for_transfers_by_call WHERE contract_number =@contractNumber and quality = 0 and deleted = 0 and isnull(card_number, '') = '' and filialcode=@operationFilialCode", conn);
                cmd.Parameters.Add("@contractNumber", SqlDbType.Float).Value = order.TransferCallContractDetails.ContractNumber;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.Float).Value = order.FilialCode;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }

    }
}
