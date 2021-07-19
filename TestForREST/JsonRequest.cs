using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.IO;

namespace TestForREST
{
    public static class JsonRequest
    {
        public static string SavePaymentOrder(PaymentOrder order,byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { order= order },
               new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList=new List<KeyValuePair<string,string>>();
            paramList.Add(new KeyValuePair<string,string>("language",language.ToString()));
            string response = DoPostRequestJson(jsonObject, "SavePaymentOrder", paramList);

            return response;
        }

        public static string SaveLoanMatureOrder(LoanMatureOrder order, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { order = order },
              new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));

            string response = DoPostRequestJson(jsonObject, "SaveLoanMatureOrder", paramList);
            return response;
        }

        public static LoanRequestResponse GetLoan(ulong productId, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { productId = productId},
                 new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));

            LoanRequestResponse response = DoPostRequestJson<LoanRequestResponse>(jsonObject, "GetLoan", paramList);
            return response;

        }
        public static object GetLoanJSON(ulong productId, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { productId = productId },
                 new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));

            string response = DoPostRequestJson(jsonObject, "GetLoan", paramList);
            return response;

        }
        internal static string DoPostRequestJson(string jsonObject, string url, List<KeyValuePair<string, string>> paramList=null)
        {

            string json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/ExternalBankingRESTService/XBRESTService.svc/" + url);

            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";

            request.Headers.Add("SessionId", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
            string language = paramList.Find(m => m.Key == "language").Value;
            if (paramList != null)
            {
                foreach (KeyValuePair<string, string> param in paramList)
                {
                    request.Headers.Add(param.Key, param.Value);
                }
            }

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(jsonObject);

            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                // Send the data.
                requestStream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            Stream stream = response.GetResponseStream();

            StreamReader reader = new StreamReader(stream);

            json = reader.ReadToEnd();


            return json;

        }
        public static string SaveReferenceOrder(ReferenceOrder order, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { order = order },
               new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));
            string response = DoPostRequestJson(jsonObject, "SaveReferenceOrder", paramList);

            return response;
        }

        public static string GetReferenceOrder(long id, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { id = id },
               new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));
            string response = DoPostRequestJson(jsonObject, "GetReferenceOrder", paramList);

            return response;
        }
        public static string ApproveReferenceOrder(long id, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { id = id },
               new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));
            string response = DoPostRequestJson(jsonObject, "ApproveReferenceOrder", paramList);

            return response;
        }
        public static string GetReferenceOrderFee(bool UrgentSign, byte language)
        {
            var jsonObject = JsonConvert.SerializeObject(new { UrgentSign = UrgentSign },
               new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("language", language.ToString()));
            string response = DoPostRequestJson(jsonObject, "GetReferenceOrderFee", paramList);

            return response;
        }

        internal static T DoPostRequestJson<T>(string jsonObject, string url, List<KeyValuePair<string, string>> paramList = null)
        {
            //string json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/ExternalBankingRESTService/XBRESTService.svc/" + url);

            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";

            request.Headers.Add("SessionId", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
            string language = paramList.Find(m => m.Key == "language").Value;
            if (paramList != null)
            {
                foreach (KeyValuePair<string, string> param in paramList)
                {
                    request.Headers.Add(param.Key, param.Value);
                }
            }

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(jsonObject);

            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                // Send the data.
                requestStream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            Stream stream = response.GetResponseStream();

            //StreamReader reader = new StreamReader(stream);
            //json = reader.ReadToEnd();

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T) ser.ReadObject(stream);
        }
        
    }
}
