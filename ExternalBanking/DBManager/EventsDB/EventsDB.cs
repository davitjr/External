using ExternalBanking.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager.EventsDB
{
    internal class EventsDB
    {
        internal static List<Event> Get(EventTypes eventTypes, Languages Language)
        {
            DataTable dt = new DataTable();
            List<Event> result = new List<Event>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT EV.event_name_am,EV.event_name_en,sEV.*
		                                           FROM Tbl_type_of_events EV 
                                                   INNER JOIN Tbl_sub_type_of_events sEV ON EV.id = sEV.event_type_id                                                 
                                                   WHERE EV.id=@EvID ", conn);

                cmd.Parameters.Add("@EvID", SqlDbType.Int).Value = (int)eventTypes;
                dt.Load(cmd.ExecuteReader());

                foreach (DataRow item in dt.Rows)
                {
                    Event subEvent = new Event();
                    subEvent.Id = (EventTypes)int.Parse(item["event_type_id"].ToString());
                    subEvent.SubTypeId = int.Parse(item["id"].ToString());

                    if (Language == Languages.hy)
                    {
                        subEvent.EventTypeName = item["event_name_am"].ToString();
                        subEvent.EventSubTypeName = item["name_am"].ToString();
                        subEvent.Description = item["description_am"].ToString();
                    }
                    else
                    {
                        subEvent.EventTypeName = item["event_name_en"].ToString();
                        subEvent.EventSubTypeName = item["name_en"].ToString();
                        subEvent.Description = item["description_en"].ToString();
                    }
                    subEvent.Price = double.Parse(item["price"].ToString());
                    subEvent.DiscountRate = double.Parse(item["discount_rate"].ToString());
                    subEvent.DiscountedPrice = subEvent.Price - (subEvent.Price * subEvent.DiscountRate / 100);
                    subEvent.Currency = item["currency"].ToString();
                    subEvent.MaxQuantityPerUser = int.Parse(item["max_quantity_per_user"].ToString());
                    subEvent.StartDate = Convert.ToDateTime(item["start_date"].ToString());
                    subEvent.ReceiverAccount = item["receiver_account_number"].ToString();
                    subEvent.IsActive = Convert.ToBoolean(item["is_active"].ToString());

                    result.Add(subEvent);
                }
            }
            return result;
        }
    }
}
