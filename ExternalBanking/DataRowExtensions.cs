using System;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking
{
    public static class DataReadingExtensions
    {
        public static T FieldOrDefault<T>(this DataRow row, string columnName)
        {
            return row.IsNull(columnName) ? default : row.Field<T>(columnName);
        }
        public static T FieldOrDefault<T>(this SqlDataReader dr, string columnName)
        {
            return dr[columnName] == DBNull.Value ? default : (T)dr[columnName];
        }
    }
}
