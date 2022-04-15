using System;
using System.Data;
using System.Web;

namespace ExternalBanking
{
    public static class CacheHelper
    {
        public static DataTable Get(string key)
        {
            return (DataTable)HttpRuntime.Cache[key];
        }

        public static T Get<T>(string key)
        {
            return (T)HttpRuntime.Cache[key];
        }

        public static void Add(DataTable value, string key)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddMinutes(480), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public static void Add<T>(T value, string key)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddMinutes(480), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public static void AddForSTAK(DataTable value, string key)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddMinutes(60), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public static void AddForSTAK<T>(T value, string key)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddMinutes(60), System.Web.Caching.Cache.NoSlidingExpiration);
        }
    }
}
