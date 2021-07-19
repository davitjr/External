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
    public partial class LogIn : Form
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            string userName;
            string password;
            string OTP;
            string IP;

            userName = this.userName.Text;
            password = "680fe7e5dfce3278fc252c8107fb25fe381879a2";
            OTP = this.otp.Text;
            IP = "127.0.0.1";

            DoPostRequest("MobileAuthorization", userName, password, OTP, IP);
        }

        public string DoPostRequest(string url,string userName,string password,string OTP,string IP)
        {
            string json = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:40388/XBRESTService.svc/" + url);

                request.Method = "POST";
                request.ContentType = "text/plain;charset=utf-8";

                request.Headers.Add("UserName", userName);
                request.Headers.Add("Password", password);
                request.Headers.Add("OTP", OTP);
                request.Headers.Add("IpAddress", IP);
                request.ContentLength = 0;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Stream stream = response.GetResponseStream();

                StreamReader reader = new StreamReader(stream);

                json = reader.ReadToEnd();

                this.txtResult.Text = json;

            }
            catch (Exception ex)
            {
                String str = "";
                str = "";
            }

            return json;

        }
    }
}
