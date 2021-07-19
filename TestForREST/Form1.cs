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
	public partial class Form1 : Form
	{

		String Token { get; set; }

		public Form1()
		{
			InitializeComponent();
		}

        private void DoPostRequestJson(string jsonObject,string url)
        {
            try
            {

            
            string json = "";
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://31.47.195.229:86/XBRESTService.svc/" + url);
           HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:40388/XBRESTService.svc/" + url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";

            request.Headers.Add("SessionId", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
            request.Headers.Add("language", this.languageComboBox.SelectedItem.ToString());

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
            catch(Exception ex)
            {
                string strErr = ex.InnerException.Message;
            }
        }

        public string ToUnixEpoch(DateTime dateTime)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dateTime;
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return "/Date(" + ts.TotalMilliseconds.ToString("#") + ")/";
        }

		private void getAccountButton_Click(object sender, EventArgs e)
		{
            string json = "";
            ulong accountNumber =ulong.Parse(this.textBox1.Text) ;

            var jsonObject = JsonConvert.SerializeObject(new { accountNumber = accountNumber },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject,"GetAccount");
		
		}


        public string DoPostRequest(string url)
        {
            string json = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:40388/XBRESTService.svc/" + url);

            request.Method = "POST";
            request.ContentType = "text/plain;charset=utf-8";

                request.Headers.Add("SessionId", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
                request.Headers.Add("language", this.languageComboBox.SelectedItem.ToString());
            request.ContentLength = 0;

			HttpWebResponse response = request.GetResponse() as HttpWebResponse;

			Stream stream = response.GetResponseStream();

			StreamReader reader = new StreamReader(stream);

                json = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                String str = "";
                str = "";
            }

            return json;

        }




		private void Form1_Load(object sender, EventArgs e)
		{
			//accountTextBox.Text = "220000940027002";
            languageComboBox.SelectedIndex = 0;
			Token = "1111111";
        }

		private void AccountLabel_Click(object sender, EventArgs e)
		{

		}

        private void button1_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetAccounts");
            this.txtResult.Text  = requestResult;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetDeposits");
            this.txtResult.Text = requestResult;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetLoans");
            this.txtResult.Text = requestResult;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetCards");
            this.txtResult.Text = requestResult;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetPeriodicTransfers");
            this.txtResult.Text = requestResult;
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
            long productId = long.Parse(this.textBox4.Text);

            var jsonObject = JsonConvert.SerializeObject(new { productId = productId },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject,"GetDeposit");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            long productId = long.Parse(this.textBox4.Text);

            var jsonObject = JsonConvert.SerializeObject(new { productId = productId },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject,"GetLoan");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            long productId = long.Parse(this.textBox4.Text);

            var jsonObject = JsonConvert.SerializeObject(new { productId = productId },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject,"GetCard");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            long productId = long.Parse(this.textBox4.Text);

            var jsonObject = JsonConvert.SerializeObject(new { productId = productId },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetPeriodicTransfer");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetCurrentAccounts");
            this.txtResult.Text = requestResult;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ulong accountNumber = ulong.Parse(this.textBox1.Text);

            var jsonObject = JsonConvert.SerializeObject(new { accountNumber = accountNumber, dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)) },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetAccountStatement");
        }

        private void button12_Click(object sender, EventArgs e)
        {

            string accountNumber = this.textBox1.Text;

            var jsonObject = JsonConvert.SerializeObject(new { cardNumber = accountNumber, dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)) },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetCardStatement");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string accountNumber = this.textBox1.Text;

            var jsonObject = JsonConvert.SerializeObject(new { cardNumber = accountNumber },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetArCaBalance");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            
            var jsonObject = JsonConvert.SerializeObject(new { dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)) },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetDraftOrders");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)) },
        new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetSentOrders");
        }

        private void button16_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)),type=2 },
        new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetMessages");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { dateFrom = ToUnixEpoch(DateTime.Parse(this.dateFrom.Text)), dateTo = ToUnixEpoch(DateTime.Parse(this.dateTo.Text)), type = 1 },
      new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetMessages");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            NewMessage m = new NewMessage();
            m.Show();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { messageId = this.messageId.Text },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "DeleteMessage");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { messageId = this.messageId.Text },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "MarkMessageReaded");
        }

        private void button21_Click(object sender, EventArgs e)
        {
           // SearchCommunal searchCommunal = new SearchCommunal();
          //  searchCommunal.AbonentNumber = this.txtAbonent.Text;
         //   searchCommunal.PhoneNumber = this.txtPhone.Text;
         //   searchCommunal.AbonentType = 1;
         //   searchCommunal.CommunalType = short.Parse(this.txtType.Text);

  
            var jsonObject = JsonConvert.SerializeObject(new { communalType = 2,abonentNumber = "12121",checkType= 2,branchCode= "01" },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetCommunalDetails");
        }

		private void GetContactsButton_Click(object sender, EventArgs e)
		{
			string requestResult;
			requestResult = DoPostRequest("GetContacts");
			this.txtResult.Text = requestResult;

		}

		private void getContactButton_Click(object sender, EventArgs e)
		{
			ulong contactID = 4;

			var jsonObject = JsonConvert.SerializeObject(new { contactId = contactID },
			new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

			DoPostRequestJson(jsonObject, "GetContact");
		}

		private void AddContactButton_Click(object sender, EventArgs e)
		{
			
		}

		private void UpdateContactButton_Click(object sender, EventArgs e)
		{
			Contact currentContact = new Contact();
			currentContact.ContactAccountList = new List<ContactAccount>();
            currentContact.Id = 22;
            currentContact.Description = "Poghosyan Poghos";


			ContactAccount contactAccount = new ContactAccount();
			contactAccount.AccountNumber = "10000001";
			contactAccount.Description = "Test Account Update";
			currentContact.ContactAccountList.Add(contactAccount);


			var jsonObject = JsonConvert.SerializeObject(new { contact = currentContact },
		    new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

			DoPostRequestJson(jsonObject, "UpdateContact");
		}

        private void AddContactButton_Click_1(object sender, EventArgs e)
        {
            Contact currentContact = new Contact();

            ContactAccount contactAccount = new ContactAccount();
            currentContact.Id=18;
            currentContact.Description="Poghosyan Poghos";
            contactAccount.Id=15;
            contactAccount.AccountNumber = "2200033689754688";
            contactAccount.Description = "Test Account";
            currentContact.ContactAccountList.Add(contactAccount);
            currentContact.Description = "TEST";

            // {"contact":{"Id":18,"Description":"Poghosyan Poghos","ContactAccountsList":[{"Id":15,"Description":"Test Account","AccountNumber":"2200033689754688"}]}}

            var jsonObject = JsonConvert.SerializeObject(new { contact = currentContact },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "AddContact");
        }

        private void button22_Click(object sender, EventArgs e)
        {
            long id = long.Parse(this.txtDocId.Text);

            var jsonObject = JsonConvert.SerializeObject(new { id = id },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetPaymentOrder");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            string currency = this.txtRateCurrency.Text;
            byte rateType = byte.Parse(this.txtRateType.Text);
            byte direction = byte.Parse(this.txtDirection.Text);

            var jsonObject = JsonConvert.SerializeObject(new { currency = currency,rateType=rateType,direction=direction },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetLastExhangeRate");

        }

        private void button24_Click(object sender, EventArgs e)
        {
            long id = long.Parse(this.txtDocId.Text);

            var jsonObject = JsonConvert.SerializeObject(new { id = id },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetPaymentOrderFutureBalance");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            short orderType = 15;
            byte orderSubType = 1;
            byte accountType = 1;

            var jsonObject = JsonConvert.SerializeObject(new { orderType = orderType, orderSubType = orderSubType, accountType = accountType },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

       
            DoPostRequestJson(jsonObject, "GetAccountsForOrder");
        }

        private void button29_Click(object sender, EventArgs e)
        {
            long id = long.Parse(this.txtDocId.Text);

            var jsonObject = JsonConvert.SerializeObject(new { id = this.txtDocId.Text },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "GetPaymentOrder");
        }

        private void button30_Click(object sender, EventArgs e)
        {
            long id = long.Parse(this.txtDocId.Text);

            var jsonObject = JsonConvert.SerializeObject(new { id = id },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });

            DoPostRequestJson(jsonObject, "DeletePaymentOrder");
        }

        private void button31_Click(object sender, EventArgs e)
        {
            string requestResult;
            requestResult = DoPostRequest("GetUnreadedMessagesCount");
            this.txtResult.Text = requestResult;
        }

        private void button32_Click(object sender, EventArgs e)
        {
            Frm_TransferArm frm = new Frm_TransferArm();
            frm.Show();
        }

        private void button33_Click(object sender, EventArgs e)
        {
            var jsonObject = JsonConvert.SerializeObject(new { messagesCount = txt_messagesCount.Text, type = txt_messageType.Text });
            DoPostRequestJson(jsonObject, "GetNumberOfMessages");
        }

        private void button34_Click(object sender, EventArgs e)
        {
            short orderType = 1;
            byte orderSubType =3;
            byte accountType = 1;

            var jsonObject = JsonConvert.SerializeObject(new { orderType = orderType, orderSubType = orderSubType, accountType = accountType },
            new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.None });


            DoPostRequestJson(jsonObject, "GetAccountsForOrder");
        }

        private void button35_Click(object sender, EventArgs e)
        {
            DoPostRequest("GetReferenceTypes");
        }


	}
}
