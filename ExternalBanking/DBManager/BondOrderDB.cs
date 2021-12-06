using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;


namespace ExternalBanking.DBManager
{
    static class BondOrderDB
    {

        /// <summary>
        /// Պարտատոմսի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult SaveBondOrder(BondOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_bond_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar).Value = order.Bond.Currency;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@ISIN", SqlDbType.NVarChar).Value = order.Bond.ISIN;
                    cmd.Parameters.Add("@bond_count", SqlDbType.Float).Value = order.Bond.BondCount;
                    cmd.Parameters.Add("@unit_price", SqlDbType.Float).Value = order.Bond.UnitPrice;
                    cmd.Parameters.Add("@account_number_for_bond", SqlDbType.NVarChar).Value = order.Bond.AccountForBond.AccountNumber;
                    cmd.Parameters.Add("@account_number_for_coupon", SqlDbType.NVarChar).Value = order.Bond.AccountForCoupon.AccountNumber;
                    cmd.Parameters.Add("@depositary_account", SqlDbType.BigInt).Value = order.Bond.CustomerDepositaryAccount != null ? order.Bond.CustomerDepositaryAccount.AccountNumber : 0;
                    cmd.Parameters.Add("@depositary_account_description", SqlDbType.NVarChar).Value = order.Bond.CustomerDepositaryAccount != null ? order.Bond.CustomerDepositaryAccount.Description : null;
                    cmd.Parameters.Add("@cashier_filial_code", SqlDbType.Int).Value = order.Bond.FilialCode;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = BondQuality.New;
                    cmd.Parameters.Add("@bond_document_number", SqlDbType.Int).Value = order.Bond.DocumentNumber;
                    cmd.Parameters.Add("@depositary_account_existence_type", SqlDbType.TinyInt).Value = order.Bond.DepositaryAccountExistenceType;
                    cmd.Parameters.Add("@depositary_account_bank_code", SqlDbType.Int).Value = order.Bond.CustomerDepositaryAccount != null ? order.Bond.CustomerDepositaryAccount.BankCode : 0;
                    cmd.Parameters.Add("@bond_issue_id", SqlDbType.Float).Value = order.Bond.BondIssueId;
                    cmd.Parameters.Add("@interest_rate", SqlDbType.NVarChar).Value = order.Bond.InterestRate;
                    cmd.Parameters.Add("@share_type", SqlDbType.Int).Value = order.Bond.ShareType;
                    cmd.Parameters.Add("@stock_income_account", SqlDbType.NVarChar).Value = order.Bond.CustomerDepositaryAccount != null ? order.Bond.CustomerDepositaryAccount.StockIncomeAccountNumber : null;


                    if (order.Bond.SecuringMoney != null)
                        cmd.Parameters.Add("@securing_money", SqlDbType.Bit).Value = order.Bond.SecuringMoney;
                    else
                        cmd.Parameters.Add("@securing_money", SqlDbType.Bit).Value = DBNull.Value;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    int id = Convert.ToInt32(cmd.Parameters["@id"].Value);


                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }
                    return result;
                }
            }

        }

        /// <summary>
        /// Պարտատոմսի հայտի  Get
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static BondOrder GetBondOrder(BondOrder order)
        {
            DataTable dt = new DataTable();
            order.Bond = new Bond();
            order.Bond.CustomerDepositaryAccount = new DepositaryAccount();

            string sql = @"SELECT 
                                        hb.customer_number,
                                        hb.registration_date,
                                        hb.document_type,
                                        hb.document_number,
                                        hb.document_subtype,
                                        hb.confirmation_date,
                                        hb.quality,
                                        hb.currency, 
                                        hb.source_type,
                                        hb.operationFilialCode,
                                        hb.operation_date,
                                        hb.amount,
                                        d.securing_money,
                                        d.filialcode,
                                        d.share_type,
                                        d.ISIN, 
	                                    d.bond_count, 
	                                    d.unit_price,
	                                    d.account_number_for_bond, 
	                                    d.account_number_for_coupon, 
	                                    d.depositary_account, 
	                                    d.depositary_account_description,
                                        d.stock_income_account,
                                        d.document_number as bondDocumentNumber,
                                        d.depositary_account_existence_type,
                                        E.[description] as depositary_account_existence_type_description,
                                        d.depositary_account_bank_code,
                                        tbi.description as issuer_type_description
			                            FROM Tbl_HB_documents hb
                                        INNER JOIN tbl_bond_order_details d  ON hb.doc_ID = d.doc_id
                                        INNER JOIN Tbl_Type_of_Depositary_Account_Existence E on d.depositary_account_existence_type = E.id
									    INNER JOIN tbl_bond_issue bi ON bi.id = d.bond_issue_id
                                        INNER JOIN tbl_type_of_bond_issuer tbi ON tbi.id = bi.issuer_type
                                        WHERE hb.Doc_ID = @DocID AND hb.customer_number = CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Amount = dt.Rows[0]["amount"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["amount"]) : 0;

                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);

                    order.Bond.FilialCode = dt.Rows[0]["filialcode"] != DBNull.Value ? Convert.ToUInt16(dt.Rows[0]["filialcode"]) : (ushort)0;
                    order.Bond.ISIN = dt.Rows[0]["ISIN"].ToString();
                    order.Bond.BondCount = dt.Rows[0]["bond_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["bond_count"]) : 0;
                    order.Bond.UnitPrice = dt.Rows[0]["unit_price"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["unit_price"]) : 0;
                    order.Bond.CustomerDepositaryAccount.AccountNumber = dt.Rows[0]["depositary_account"] != DBNull.Value ? Convert.ToDouble(dt.Rows[0]["depositary_account"].ToString()) : 0;
                    order.Bond.CustomerDepositaryAccount.Description = dt.Rows[0]["depositary_account_description"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(dt.Rows[0]["depositary_account_description"].ToString()) : "";
                    order.Bond.CustomerDepositaryAccount.StockIncomeAccountNumber = dt.Rows[0]["stock_income_account"] != DBNull.Value ? dt.Rows[0]["stock_income_account"].ToString() : default;
                    order.Bond.AccountForBond = new Account();
                    order.Bond.AccountForCoupon = new Account();
                    order.Bond.AccountForBond.AccountNumber = dt.Rows[0]["account_number_for_bond"].ToString();
                    order.Bond.AccountForCoupon.AccountNumber = dt.Rows[0]["account_number_for_coupon"].ToString();
                    order.Bond.DocumentNumber = Convert.ToInt32(dt.Rows[0]["bondDocumentNumber"]);
                    order.Bond.DepositaryAccountExistenceType = (DepositaryAccountExistence)Convert.ToByte(dt.Rows[0]["depositary_account_existence_type"]);
                    order.Bond.DepositaryAccountExistenceTypeDescription = dt.Rows[0]["depositary_account_existence_type_description"].ToString();
                    order.Bond.CustomerDepositaryAccount.BankCode = dt.Rows[0]["depositary_account_bank_code"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["depositary_account_bank_code"]) : 0;
                    order.Bond.SecuringMoney = dt.Rows[0]["securing_money"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[0]["securing_money"]) : false;
                    order.Bond.ShareType = (SharesTypes)Convert.ToInt32(dt.Rows[0]["share_type"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["share_type"]) : 0);
                    order.Bond.IssuerTypeDescription = dt.Rows[0]["issuer_type_description"].ToString();
                }
            }

            return order;
        }

        internal static Tuple<bool, bool> GetCustomerTypeAndResidence(ulong customerNumber)
        {
            bool isResident = false;
            bool isPhysical = false;
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"select  residence, type_of_client from Tbl_Customers where customer_number =@customerNumber", conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    isResident = Convert.ToInt32(reader["residence"]) == 1 ? true : false;
                    isPhysical = Convert.ToInt32(reader["type_of_client"]) == 6 ? true : false;
                }
            }

            return Tuple.Create(isResident, isPhysical);
        }

        internal static int GetBondOrderIssueSeria(int bondIssueId)
        {
            int issueSeria = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT [issue_seria] FROM [dbo].[Tbl_bond_issue] WHERE id = @id", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = bondIssueId;
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        issueSeria = Convert.ToInt32(reader["issue_seria"]);
                    }
                }

            }
            return issueSeria;
        }

        internal static ActionResult ConfirmStockOrder(int bondId, int userID)
        {
            ActionResult result = new ActionResult();
            DateTime operDay = Utility.GetCurrentOperDay().Date;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_stock_transactions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@bond_Id", SqlDbType.Int).Value = bondId;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = operDay;
                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = userID;
                    cmd.Parameters.Add("@need_transaction", SqlDbType.Bit).Value = 1;


                    try
                    {
                        cmd.ExecuteNonQuery();
                        result.ResultCode = ResultCode.Normal;
                    }
                    catch (Exception ex)
                    {
                        if (((SqlException)ex).Class == 11)
                        {
                            result.Errors.Add(new ActionError(ex.Message));
                            result.ResultCode = ResultCode.Failed;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return result;
                }
            }

        }

    }
}
