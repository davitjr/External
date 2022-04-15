using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class OperDayModeDB
    {
        internal static ActionResult SaveOpenMode(OperDayMode operDayMode)
        {
            ActionResult Result = new ActionResult();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "pr_set_mode_24_7";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@modeType", System.Data.SqlDbType.Int).Value = operDayMode.Option;
                    cmd.Parameters.Add("@setNumber", System.Data.SqlDbType.Int).Value = operDayMode.SetNumber;

                    cmd.ExecuteNonQuery();
                }

                Result.ResultCode = ResultCode.Normal;
                return Result;
            }
        }

        internal static KeyValuePair<string, string> GetCurrentOperDay24_7_Mode()
        {
            KeyValuePair<string, string> dictionary = new KeyValuePair<string, string>();
            using DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                connection.Open();
                string SqlQuery = "select M.id, M.[description] from TBl_Current_oper_day C inner join tbl_type_of_24_7_mode M on C.mode_24_7 = M.id";

                using SqlCommand cmd = new SqlCommand(SqlQuery, connection);
                dt.Load(cmd.ExecuteReader());

                string Key = "";
                string Value = "";
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Key = Convert.ToString((int)dt.Rows[i]["id"]);
                        Value = Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString());
                        KeyValuePair<string, string> dictionary2 = new KeyValuePair<string, string>(Key, Value);
                        dictionary = dictionary2;
                    }
                }
            }

            return dictionary;
        }
    }
}
