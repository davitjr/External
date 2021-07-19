using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class SafekeepingItemDB
    {

        public static List<SafekeepingItem> GetSafekeepingItems(ulong customerNumber)
        {

            List<SafekeepingItem> items = new List<SafekeepingItem>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT    key_num,
                                                            currency,
                                                            amount,
                                                            date_of_beginning,
                                                            date_of_end,
                                                            packet_number,
                                                            quality,
                                                            closing_date,
                                                            customer_number
                                                            FROM
                                                            Tbl_gold_list
                                                            WHERE quality=1 and  customer_number=@customer_number
                                                            ORDER BY date_of_end DESC", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    SafekeepingItem item = SetSafekeepingItem(row);
                    items.Add(item);
                }

            }

            return items;
        }

        private static SafekeepingItem SetSafekeepingItem(DataRow row)
        {
            SafekeepingItem item = new SafekeepingItem();
            item.ProductId = Convert.ToUInt64(row["key_num"]);
            item.Currency = row["currency"].ToString();

            if (row["date_of_beginning"] != DBNull.Value)
                item.StartDate = Convert.ToDateTime(row["date_of_beginning"]);
            item.Amount = Convert.ToDouble(row["amount"]);

            if(row["packet_number"] != DBNull.Value)
                item.PacketNumber=row["packet_number"].ToString();

            if (row["date_of_end"] != DBNull.Value)
                item.EndDate = Convert.ToDateTime(row["date_of_end"]);

            if (row["closing_date"] != DBNull.Value)
                item.ClosingDate = Convert.ToDateTime(row["closing_date"]);

            item.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
            if (row["quality"] != DBNull.Value)
                item.Quality = Convert.ToUInt16(row["quality"]);

            return item;
        }

        public static List<SafekeepingItem> GetClosedSafekeepingItems(ulong customerNumber)
        {

            List<SafekeepingItem> items = new List<SafekeepingItem>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT    key_num,
                                                            currency,
                                                            amount,
                                                            date_of_beginning,
                                                            date_of_end,
                                                            packet_number,
                                                            quality,
                                                            closing_date,
                                                            customer_number
                                                            FROM
                                                            Tbl_gold_list
                                                            WHERE quality=0 and  customer_number=@customer_number
                                                            ORDER BY date_of_end DESC", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    SafekeepingItem item = SetSafekeepingItem(row);
                    items.Add(item);
                }

            }

            return items;
        }


        public static SafekeepingItem GetSafekeepingItem(ulong customerNumber,ulong productId)
        {

            SafekeepingItem item = new SafekeepingItem();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT    key_num,
                                                            currency,
                                                            amount,
                                                            date_of_beginning,
                                                            date_of_end,
                                                            packet_number,
                                                            quality,
                                                            closing_date,
                                                            customer_number
                                                            FROM
                                                            Tbl_gold_list
                                                            WHERE  customer_number=@customer_number AND key_num=@product_id
                                                            ORDER BY date_of_end DESC", conn);

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = productId;

                DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    item = SetSafekeepingItem(row);
                   
                }

            }

            return item;
        }



    }
}
