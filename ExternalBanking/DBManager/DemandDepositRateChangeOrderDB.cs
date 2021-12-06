using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    internal static class DemandDepositRateChangeOrderDB
    {

        internal static ActionResult SaveDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();

            using SqlCommand cmd = new SqlCommand(@"
                                                    DECLARE @filial AS int
                                                    SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customer_number   
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,quality, 
                                                    source_type,operationFilialCode,operation_date,debet_account,amount,currency)
                                                    VALUES
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@doc_sub_type,
                                                    1,@source_type,@operation_filial_code,@oper_day,@debet_account,@amount,@currency)
                                                    SELECT Scope_identity() as ID
                                                     ", conn);
            cmd.CommandType = CommandType.Text;


            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
            cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
            cmd.Parameters.Add("@debet_account", SqlDbType.Float).Value = order.DemandDepositAccount.AccountNumber;
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
            cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.DemandDepositAccount.Currency;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            SaveDemandDepositRateChangeOrderDetails(order);
            result.ResultCode = ResultCode.Normal;
            return result;

        }

        internal static void SaveDemandDepositRateChangeOrderDetails(DemandDepositRateChangeOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" INSERT INTO Tbl_demand_deposit_rate_change_order_details
                                                    (
                                                    Doc_id,
                                                    tariff_group,
                                                    rate,
                                                    document_number,
                                                    document_date,
                                                    percent_credit_account
                                                    )
                                                    VALUES
                                                    (
                                                    @DocId,
                                                    @tariff_group,
                                                    @rate,
                                                    @document_number,
                                                    @document_date,
                                                    @percent_credit_account
                                                    )", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@DocId", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@tariff_group", SqlDbType.Int).Value = order.TariffGroup;
                cmd.Parameters.Add("@rate", SqlDbType.Float).Value = order.Rate;
                cmd.Parameters.Add("@percent_credit_account", SqlDbType.Float).Value = order.PercentCreditAccount;

                if (!string.IsNullOrEmpty(order.DocumentNumber))
                    cmd.Parameters.Add("@document_number", SqlDbType.NVarChar,1000).Value = order.DocumentNumber;
                else
                    cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 1000).Value = DBNull.Value;
                if(order.DocumentDate!=null)
                    cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = order.DocumentDate?.Date;
                else
                    cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = DBNull.Value;
                cmd.ExecuteNonQuery();

            }


        }

        internal static DemandDepositRateChangeOrder GetDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT 
                                                    hb.filial,
                                                    hb.customer_number,
                                                    hb.registration_date,
                                                    hb.document_type,
                                                    hb.document_number,
                                                    hb.document_subtype,
                                                    hb.quality, 
                                                    hb.source_type,
                                                    hb.operationFilialCode,
                                                    hb.operation_date ,
													dm.tariff_group,
                                                    dm.rate,
                                                    dm.document_number as dm_document_number,
                                                    dm.document_date,
                                                    dm.percent_credit_account,
	                                                hb.debet_account,
													hb.amount
													FROM Tbl_HB_documents hb
                                                    INNER JOIN Tbl_demand_deposit_rate_change_order_details dm
                                                    ON hb.doc_ID=dm.doc_id
                                                    WHERE hb.Doc_ID=@DocID and hb.customer_number=CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                order.FilialCode = dt.Rows[0]["operationFilialCode"] != DBNull.Value ? Convert.ToUInt16(dt.Rows[0]["operationFilialCode"]) : (ushort)0;
                order.DemandDepositAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Rate = Convert.ToDouble(dt.Rows[0]["rate"]);
                order.TariffGroup = Convert.ToInt32(dt.Rows[0]["tariff_group"]);

                if (dt.Rows[0]["dm_document_number"] != DBNull.Value)
                    order.DocumentNumber = dt.Rows[0]["dm_document_number"].ToString();
                if (dt.Rows[0]["document_date"] != DBNull.Value)
                    order.DocumentDate = Convert.ToDateTime(dt.Rows[0]["document_date"]);
                if (dt.Rows[0]["percent_credit_account"] != DBNull.Value)
                    order.PercentCreditAccount = Convert.ToDouble(dt.Rows[0]["percent_credit_account"]);



            }
            return order;
        }

    }
}
