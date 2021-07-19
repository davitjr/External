using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;
using System.Linq;

namespace ExternalBanking.DBManager
{
    internal class LoanEquipmentDB
    {
        internal static LoanEquipment GetLoanEquipmentDetails(int equipmentID)
        {
            LoanEquipment loanEquipment = new LoanEquipment();
            DataTable dt = new DataTable();

            using (SqlConnection DbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "pr_get_problem_loan_equipments_details";

                using (SqlCommand cmd = new SqlCommand(sql, DbConn))
                {
                    cmd.Parameters.Add("@equipmentID", SqlDbType.Int).Value = equipmentID.ToString();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    DbConn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            loanEquipment = SetLoanEquipmentDetails(dt.Rows[i]);
                        }
                    }
                }
                return loanEquipment;
            }
        }

        internal static string GetWhereCondition(SearchLoanEquipment searchLoanEquipment)
        {
            string whereCondition = "";
            if (searchLoanEquipment.CustomerNumber != 0)
            {
                whereCondition += " AND S.customer_number = " + searchLoanEquipment.CustomerNumber.ToString();
            }
            if (searchLoanEquipment.LoanFullNumber != 0)
            {
                whereCondition += " AND S.loan_full_number = " + searchLoanEquipment.LoanFullNumber.ToString();
            }
            if (searchLoanEquipment.EquipmentDescription != null)
            {
                whereCondition += " AND dbo.fnc_convertAnsiToUnicode(EQ.equipment_description) like N'%" + searchLoanEquipment.EquipmentDescription + "%' ";
            }
            if (searchLoanEquipment.FilialCode != 0)
            {
                whereCondition += " AND S.filialcode = " + searchLoanEquipment.FilialCode.ToString();
            }
            if (searchLoanEquipment.EquipmentAddress != null)
            {
                whereCondition += " AND dbo.fnc_convertAnsiToUnicode(EQ.pro_address) like N'%" + searchLoanEquipment.EquipmentAddress + "%' ";
            }
            if (searchLoanEquipment.EquipmentSalePriceFrom != null)
            {
                whereCondition += " AND ISNULL(EQ.sale_price, 0) >= " + searchLoanEquipment.EquipmentSalePriceFrom.ToString();
            }
            if (searchLoanEquipment.EquipmentSalePriceTo != null)
            {
                whereCondition += " AND ISNULL(EQ.sale_price, 0) <= " + searchLoanEquipment.EquipmentSalePriceTo.ToString();
            }
            if (searchLoanEquipment.AuctionEndDateFrom != null)
            {
                whereCondition += " AND EQ.auction_end_date >= ' " + String.Format("{0:dd/MMM/yy}", searchLoanEquipment.AuctionEndDateFrom) + "' ";
            }
            if (searchLoanEquipment.AuctionEndDateTo != null)
            {
                whereCondition += " AND EQ.auction_end_date <= ' " + String.Format("{0:dd/MMM/yy}", searchLoanEquipment.AuctionEndDateTo) + "' ";
            }
            if (searchLoanEquipment.EquipmentQuality == 1)
            {
                whereCondition += " AND EQ.closing_date IS NULL ";
            }
            else if (searchLoanEquipment.EquipmentQuality == 2)
            {
                whereCondition += " AND EQ.closing_date IS NOT NULL ";
            }
            if (searchLoanEquipment.SaleStage == 1)
            {
                whereCondition += " AND EQ.acquired_by_the_bank = 0 ";
            }
            else if (searchLoanEquipment.SaleStage == 2)
            {
                whereCondition += " AND ISNULL(EQ.acquired_by_the_bank, 0) = 1 AND SP.Purchase_Sale_Date IS NULL ";
            }
            else if (searchLoanEquipment.SaleStage == 3)
            {
                whereCondition += " AND V.inUse = 0 ";
            }
            return whereCondition;
        }
        internal static List<LoanEquipment> GetSearchedLoanEquipments(SearchLoanEquipment searchLoanEquipment)
        {
            List<LoanEquipment> list = new List<LoanEquipment>();
            string whereCondition = "";
            using (SqlConnection DbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "pr_get_problem_loan_equipments";

                using (SqlCommand cmd = new SqlCommand(sql, DbConn))
                {
                    whereCondition = GetWhereCondition(searchLoanEquipment);

                    cmd.Parameters.Add("@whereCondition", SqlDbType.NVarChar).Value = whereCondition;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    DbConn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            LoanEquipment equipment = new LoanEquipment();
                            if (dr["equipment_id"] != DBNull.Value)
                                equipment.EquipmentID = Convert.ToInt32(dr["equipment_id"]);
                            if (dr["customer_number"] != DBNull.Value)
                                equipment.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                            if (dr["app_id"] != DBNull.Value)
                                equipment.AppID = Convert.ToDouble(dr["app_id"]);
                            if (dr["loan_full_number"] != DBNull.Value)
                                equipment.LoanFullNumber = Convert.ToUInt64(dr["loan_full_number"]);
                            if (dr["Name"] != DBNull.Value)
                                equipment.CustomerName = dr["Name"].ToString();
                            if (dr["filialcode"] != DBNull.Value)
                                equipment.FilialCode = Convert.ToInt32(dr["filialcode"]);
                            if (dr["equipment_description"] != DBNull.Value)
                                equipment.EquipmentDescription = dr["equipment_description"].ToString();
                            if (dr["pro_address"] != DBNull.Value)
                                equipment.EquipmentAddress = dr["pro_address"].ToString();
                            if (dr["auction_end_date"] != DBNull.Value)
                                equipment.AuctionEndDate = Convert.ToDateTime(dr["auction_end_date"].ToString());
                            if (dr["equipment_price"] != DBNull.Value)
                                equipment.EquipmentPrice = Convert.ToDouble(dr["equipment_price"]);
                            if (dr["price_limit_by_the_bank"] != DBNull.Value)
                                equipment.PriceLimitByTheBank = Convert.ToDouble(dr["price_limit_by_the_bank"]);
                            if (dr["equipment_sale_price"] != DBNull.Value)
                                equipment.EquipmentSalePrice = Convert.ToDouble(dr["equipment_sale_price"]);
                            if (dr["bank_transferred_money"] != DBNull.Value)
                                equipment.BankTransferredMoney = Convert.ToDouble(dr["bank_transferred_money"]);
                            if (dr["auction_organizer"] != DBNull.Value)
                                equipment.AuctionOrganizer = dr["auction_organizer"].ToString();
                            if (dr["closing_date"] != DBNull.Value)
                                equipment.ClosingDate = Convert.ToDateTime(dr["closing_date"].ToString());
                            list.Add(equipment);
                        }

                        if (dr.NextResult())
                        {
                            if (dr.Read() && list.Count != 0)
                                list.First().ListCount = Convert.ToInt32(dr["qanak"]);
                        }
                    }
                }
                return list;
            }
        }
        internal static LoanEquipment GetSumsOfEquipmentPrices(SearchLoanEquipment searchLoanEquipment)
        {
            LoanEquipment equipment = new LoanEquipment();
            string whereCondition = "";
            using (SqlConnection DbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = "pr_get_sum_of_problem_loan_equipments_prices";

                using (SqlCommand cmd = new SqlCommand(sql, DbConn))
                {
                    whereCondition = GetWhereCondition(searchLoanEquipment);

                    cmd.Parameters.Add("@whereCondition", SqlDbType.NVarChar).Value = whereCondition;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    DbConn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {                        
                        if (dr.Read())
                        {                            
                            if (dr["sum_of_equipment_price"] != DBNull.Value)
                                equipment.SumOfEquipmentPrice = Convert.ToDouble(dr["sum_of_equipment_price"]);
                            if (dr["sum_of_price_limit_by_the_bank"] != DBNull.Value)
                                equipment.SumOfPriceLimitByTheBank = Convert.ToDouble(dr["sum_of_price_limit_by_the_bank"]);
                            if (dr["sum_of_sale_price"] != DBNull.Value)
                                equipment.SumOfEquipmentSalePrice = Convert.ToDouble(dr["sum_of_sale_price"]);
                            if (dr["sum_of_bank_transferred_money"] != DBNull.Value)
                                equipment.SumOfBankTransferredMoney = Convert.ToDouble(dr["sum_of_bank_transferred_money"]);
                        }
                    }
                }
                return equipment;
            }
        }

        internal static string GetEquipmentClosingReason(int equipmentID)
        {
            string closingReason = "";
            DataTable dt = new DataTable();

            using (SqlConnection DbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "SELECT dbo.fn_get_equipment_closing_reason(@equipmentID)";

                using (SqlCommand cmd = new SqlCommand(sql, DbConn))
                {
                    cmd.Parameters.Add("@equipmentID", SqlDbType.Int).Value = equipmentID.ToString();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 300;

                    DbConn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            closingReason = dr[0].ToString();
                    }
                }
                return closingReason;
            }
        }
        internal static ActionResult LoanEquipmentClosing(int equipmentID, int setNumber, string closingReason)
        {
            ActionResult result = new ActionResult();
            result = LoanEquipmentDB.CheckForClose(equipmentID);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_closing_problem_loan_equipment";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@equipmentID", SqlDbType.Int).Value = equipmentID;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add("@closingReason", SqlDbType.NVarChar, 500).Value = closingReason;
                    cmd.Parameters.Add("@returnErrorCodes", SqlDbType.Int).Value = 1;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }

        internal static ActionResult CheckForClose(int EquipmentID)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = "pr_check_appliaction_provisions";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@equipmentID", SqlDbType.Float).Value = EquipmentID;
                    cmd.Parameters.Add("@returnErrorCodes", SqlDbType.TinyInt).Value = 1;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ActionError error = new ActionError();
                            error.Code = short.Parse(dr["code"].ToString());
                            error.Description = Info.GetTerm(short.Parse(dr["code"].ToString()), new string[] { dr["add_inf"].ToString() }, Languages.hy);
                            result.Errors.Add(error);
                        }
                    }


                }

            }
            return result;

        }
        internal static ActionResult ChangeCreditProductMatureRestriction(double appID, int setNumber, int allowMature)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_change_credit_product_mature_restriction";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = appID;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;
                   cmd.Parameters.Add("@allowMature", SqlDbType.Int).Value = allowMature;

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        private static LoanEquipment SetLoanEquipmentDetails(DataRow row)
        {
            LoanEquipment equipment = new LoanEquipment();

            equipment.EquipmentID = Convert.ToInt32(row["equipment_id"]);
            if (row["equipment_price"] != DBNull.Value)
                equipment.EquipmentPrice = Convert.ToDouble(row["equipment_price"]);
            if (row["bank_transferred_money"] != DBNull.Value)
                equipment.BankTransferedMoney = Convert.ToDouble(row["bank_transferred_money"]);
            if (row["equipment_description"] != DBNull.Value)
                equipment.EquipmentDescription = row["equipment_description"].ToString();
            if (row["pro_address"] != DBNull.Value)
                equipment.EquipmentAddress = row["pro_address"].ToString();
            if (row["market_price"] != DBNull.Value)
                equipment.MarketPrice = Convert.ToDouble(row["market_price"]);
            if (row["auction_organizer"] != DBNull.Value)
                equipment.AuctionOrganizer = row["auction_organizer"].ToString();
            if (row["first_auction_initial_price"] != DBNull.Value)
                equipment.FirstAuctionInitialPrice = Convert.ToDouble(row["first_auction_initial_price"]);
            if (row["auction_end_date"] != DBNull.Value)
                equipment.AuctionEndDate = Convert.ToDateTime(row["auction_end_date"].ToString());
            if (row["equipment_price"] != DBNull.Value)
                equipment.EquipmentPrice = Convert.ToDouble(row["equipment_price"]);
            if (row["price_limit_by_the_bank"] != DBNull.Value)
                equipment.PriceLimitByTheBank = Convert.ToDouble(row["price_limit_by_the_bank"]);
            if (row["event_add_inf"] != DBNull.Value)
                equipment.EventAddInf = row["event_add_inf"].ToString();
            if (row["equipment_sale_price"] != DBNull.Value)
                equipment.EquipmentSalePrice = Convert.ToDouble(row["equipment_sale_price"]);
            if (row["buyers_fullname"] != DBNull.Value)
                equipment.BuyersFullName = row["buyers_fullname"].ToString();
            if (row["add_inf"] != DBNull.Value)
                equipment.AddInf = row["add_inf"].ToString();
            if (row["date_of_contract_sale"] != DBNull.Value)
                equipment.DateOfContractSale = Convert.ToDateTime(row["date_of_contract_sale"].ToString());
            if (row["expert_appraised_value"] != DBNull.Value)
                equipment.ExpertAppraisedValue = Convert.ToDouble(row["expert_appraised_value"]);
            if (row["closing_date"] != DBNull.Value)
                equipment.ClosingDate = Convert.ToDateTime(row["closing_date"].ToString());
            if (row["allow_mature"] != DBNull.Value)
                equipment.AllowMature = Convert.ToInt32(row["allow_mature"]);
            if (row["equipment_type"] != DBNull.Value)
                equipment.EquipmentType = row["equipment_type"].ToString();
            if (row["cust_Desc"] != DBNull.Value)
                equipment.EquipmentOwner = row["cust_Desc"].ToString();
            if (row["prov_amount"] != DBNull.Value)
                equipment.ProvisionAmount = Convert.ToDouble(row["prov_amount"]);
            if (row["history_amount"] != DBNull.Value)
                equipment.HistoryAmount = Convert.ToDouble(row["history_amount"]);
            if (row["sale_price"] != DBNull.Value)
                equipment.SalePrice = Convert.ToDouble(row["sale_price"]);
            if (row["last_appraising_date"] != DBNull.Value)
                equipment.LastAppraisingDate = Convert.ToDateTime(row["last_appraising_date"].ToString());
            if (row["appraiser_organisation"] != DBNull.Value)
                equipment.AppraiserOrganisation = row["appraiser_organisation"].ToString();
            if (row["first_auction_start_date"] != DBNull.Value)
                equipment.FirstAuctionStartDate = Convert.ToDateTime(row["first_auction_start_date"].ToString());
            if (row["auction_begin_date"] != DBNull.Value)
                equipment.AuctionStartDate = Convert.ToDateTime(row["auction_begin_date"].ToString());
            if (row["date_of_bank_transferred_money"] != DBNull.Value)
                equipment.DateOfBankTransferredMoney = Convert.ToDateTime(row["date_of_bank_transferred_money"].ToString());
            if (row["acquired_by_the_bank"] != DBNull.Value)
                equipment.AcquiredByTheBank = row["acquired_by_the_bank"].ToString();
            if (row["closing_set_number"] != DBNull.Value)
                equipment.ClosingSetNumber = Convert.ToInt32(row["closing_set_number"]);
            if (row["closing_reason"] != DBNull.Value)
                equipment.ClosingReason = row["closing_reason"].ToString();


            return equipment;
        }

           
        
    }
}
