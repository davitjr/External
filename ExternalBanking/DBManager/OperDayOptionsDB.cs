using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class OperDayOptionsDB
    {
        internal static ActionResult SaveOperDayOptions(List<OperDayOptions> list)
        {
            ActionResult result = new ActionResult();
            list.ForEach(x =>
            {

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = @"INSERT INTO Tbl_oper_day_closing_options(oper_day,number_of_set,option_number,option_value,registration_date) 
                                                VALUES(@oper_day,@number_of_set,@option_number,@option_value,@registration_date)";

                        cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = x.OperDay;
                        cmd.Parameters.Add("@number_of_set", SqlDbType.Int).Value = x.NumberOfSet;
                        cmd.Parameters.Add("@option_number", SqlDbType.Int).Value = x.Code;
                        cmd.Parameters.Add("@option_value", SqlDbType.Bit).Value = Convert.ToBoolean(x.IsEnabled);
                        cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = x.RegistrationDate;
                        cmd.ExecuteNonQuery();

                    }

                }
            });
            result.ResultCode = ResultCode.Normal;
            return result;
        }

    }
}
