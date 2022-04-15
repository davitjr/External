using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class OperDayOptionsFilterDB
    {
        internal static List<OperDayOptions> SearchOperDayOptions(OperDayOptionsFilter searchParams)
        {
            List<OperDayOptions> operDayCheckingsList = new List<OperDayOptions>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    sql = @"SELECT O.*,t.code_description as Description FROM Tbl_oper_day_closing_options O  
                            INNER JOIN  Tbl_type_of_oper_day_closing_options T 
                            ON o.option_number = T.Code";


                    if (searchParams.SearchUserID != 0)
                    {
                        sql = sql + " and O.number_of_set = @numberOfSet";
                        cmd.Parameters.Add("@numberOfSet", SqlDbType.Int).Value = searchParams.SearchUserID;
                    }

                    if (searchParams.StartDate != default(DateTime))
                    {
                        sql = sql + " and registration_date >=  @StartDate ";
                        cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = searchParams.StartDate;
                    }

                    if (searchParams.EndDate != default(DateTime))
                    {
                        sql = sql + " and registration_date <= @EndDate ";
                        cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = searchParams.EndDate;
                    }

                    if (searchParams.OperDay != default(DateTime))
                    {
                        sql = sql + " and oper_day = @oper_day";
                        cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = searchParams.OperDay;
                    }

                    if (searchParams.Option != OperDayOptionsType.None)
                    {
                        sql = sql + " and option_number = @Option";
                        cmd.Parameters.Add("@Option", SqlDbType.Int).Value = searchParams.Option;
                    }

                    sql = sql + " ORDER BY O.id desc";

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            operDayCheckingsList = new List<OperDayOptions>();
                        }
                        while (dr.Read())
                        {
                            OperDayOptions operDayChecking = new OperDayOptions();
                            operDayChecking.OperDay = Convert.ToDateTime((dr["oper_day"]));
                            operDayChecking.Code = (OperDayOptionsType)Convert.ToInt32(dr["option_number"].ToString());
                            operDayChecking.CodeDescription = Utility.ConvertAnsiToUnicode(dr["Description"].ToString());
                            operDayChecking.NumberOfSet = Convert.ToInt32(dr["number_of_set"].ToString());
                            operDayChecking.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            operDayChecking.IsEnabled = Convert.ToBoolean(dr["option_value"].ToString());
                            operDayCheckingsList.Add(operDayChecking);
                        }
                    }
                }

                return operDayCheckingsList;
            }
        }



    }
}
