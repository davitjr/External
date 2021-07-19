using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class ChangeLogDB
    {
        public static void InsertChangeLog(ChangeLog change)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    conn.Open();
                    command.Connection = conn;
                    command.CommandText = @"sp_Insert_Changed_Log";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@objectType", SqlDbType.SmallInt).Value = (ushort)change.ObjectType;
                    command.Parameters.Add("@objectId", SqlDbType.Float).Value = change.ObjectID;
                    command.Parameters.Add("@actionType", SqlDbType.SmallInt).Value = (ushort)change.Action;
                    command.Parameters.Add("@actionDate", SqlDbType.SmallDateTime).Value = change.ActionDate;
                    command.Parameters.Add("@actionSetNumber", SqlDbType.Int).Value = change.ActionSetNumber;

                    if (change.ActionDescription != null)
                        command.Parameters.Add("@actionDescription", SqlDbType.NVarChar, 255).Value = change.ActionDescription;
                    else
                        command.Parameters.Add("@actionDescription", SqlDbType.NVarChar, 255).Value = DBNull.Value;


                    SqlParameter param = new SqlParameter("@changeLogId", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    command.Parameters.Add(param);

                    command.ExecuteNonQuery();

                    change.Id = ulong.Parse(command.Parameters["@changeLogId"].Value.ToString());

                }
            }
        }

        public static void InsertHBLoginsChangeLog(ChangeLog change)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    conn.Open();
                    command.Connection = conn;
                    command.CommandText = @"sp_Insert_Changed_Log";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@objectType", SqlDbType.SmallInt).Value = (ushort)change.ObjectType;
                    command.Parameters.Add("@objectId", SqlDbType.BigInt).Value = change.ObjectID;
                    command.Parameters.Add("@actionType", SqlDbType.SmallInt).Value = (ushort)change.Action;
                    command.Parameters.Add("@actionDate", SqlDbType.SmallDateTime).Value = change.ActionDate;
                    command.Parameters.Add("@actionSetNumber", SqlDbType.Int).Value = change.ActionSetNumber;

                    if (change.ActionDescription != null)
                        command.Parameters.Add("@actionDescription", SqlDbType.NVarChar, 255).Value = change.ActionDescription;
                    else
                        command.Parameters.Add("@actionDescription", SqlDbType.NVarChar, 255).Value = DBNull.Value;


                    SqlParameter param = new SqlParameter("@changeLogId", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    command.Parameters.Add(param);

                    command.ExecuteNonQuery();

                    change.Id = ulong.Parse(command.Parameters["@changeLogId"].Value.ToString());

                }
            }
        }

    }
}
