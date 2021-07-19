using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace TestForREST
{
    public partial class NewMessage : Form
    {
        public NewMessage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Message message = new Message();
            message.Subject = subject.Text;
            message.Description = description.Text;
            message.SentDate = ToUnixEpoch(DateTime.Now);

            var jsonObject = JsonConvert.SerializeObject(new { message = message },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented });

            DoPostRequestJson(jsonObject, "AddMessage");
        }

        public string ToUnixEpoch(DateTime dateTime)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dateTime.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return "/Date(" + ts.TotalMilliseconds.ToString("#") + ")/";
        }

        private void DoPostRequestJson(string jsonObject, string url)
        {

            string json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://31.47.195.229:86/XBRESTService.svc/" + url);

            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";

            request.Headers.Add("token", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
            request.Headers.Add("language", "1");

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

            this.txtResult.Text = json;

        }
    }
}
