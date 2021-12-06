using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    internal class OperDayModeFilterDB
    {
        internal static List<OperDayMode> GetOperDayModeHistory(OperDayModeFilter operDayMode)
        {
            List<OperDayMode> Result = new List<OperDayMode>();
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";
                using SqlCommand cmd = new SqlCommand();
                connection.Open();

                sql = @"SELECT O.*,t.description as Description FROM tbl_mode_24_7_history O  
                            INNER JOIN  tbl_type_of_24_7_mode T 
                            ON o.mode_type = T.id ";

                if (operDayMode.StartDate != default(DateTime))
                {
                    sql = sql + " and  cast(left(change_date ,11) as DATE) >=  @StartDate";
                    cmd.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = operDayMode.StartDate;
                }

                if (operDayMode.EndDate != default(DateTime))
                {
                    sql = sql + " and  cast(left(change_date ,11) as DATE) <=  @EndDate";
                    cmd.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = operDayMode.EndDate;
                }

                if (operDayMode.SetNumber != 0)
                {
                    sql = sql + " and O.set_number = @SetNumber";
                    cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = operDayMode.SetNumber;
                }

                if (operDayMode.Option != OperDayModeType.None)
                {
                    sql = sql + " and O.mode_type = @ModeType";
                    cmd.Parameters.Add("@ModeType", SqlDbType.Int).Value = operDayMode.Option;
                }

                sql = sql + " ORDER BY O.id desc";

                cmd.CommandText = sql;
                cmd.Connection = connection;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OperDayMode operDay = new OperDayMode();

                        operDay.ChangeDate = (DateTime)dt.Rows[i]["change_date"];
                        operDay.Option = (OperDayModeType)Convert.ToInt32(dt.Rows[i]["mode_type"]);
                        operDay.SetNumber = Convert.ToInt32(dt.Rows[i]["set_number"]);
                        operDay.ID = Convert.ToInt32((int)dt.Rows[i]["ID"]);
                        operDay.ModeTypeText = Utility.ConvertAnsiToUnicode(dt.Rows[i]["Description"].ToString());
                        Result.Add(operDay);
                    }
                }
                cmd.ExecuteNonQuery();
            }

            return Result;
        }
    }
}
