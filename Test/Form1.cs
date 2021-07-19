using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Test.ServiceReference1;

namespace Test
{
	public partial class Form1 : Form
	{
		public Contact currentContact { get; set; }

		public Form1()
		{
			InitializeComponent();
			accountNumberTextBox.Text = "101600012567";
		}

		private void getAccountButton_Click(object sender, EventArgs e)
		{

            ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
            string accountDescription = proxyClient.GetAccountDes(cardNumber.Text, DateTime.Parse(dateFrom.Text), DateTime.Parse(dateTo.Text), (byte)1);

            //ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
            //         List<Account> accList = proxyClient.GetAccounts(ulong.Parse(accountNumberTextBox.Text), 1);
            //         Account acc = proxyClient.GetAccount(ulong.Parse(accList[0].AccountNumber), 1);
            //         AccountStatement st = proxyClient.AccountStatement(acc, DateTime.Parse("01-jan-2013"), DateTime.Parse("12-feb-2015"), "hy");
        }

		private void button1_Click(object sender, EventArgs e)
		{
			//ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			//List<Card> cardsList = proxyClient.GetCards();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			//Card oneCard = proxyClient.GetCard(long.Parse(cardNumber.Text));
		}

		private void button3_Click(object sender, EventArgs e)
		{
			ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			CardStatement cardStatement = proxyClient.GetCardStatement(cardNumber.Text, DateTime.Parse(dateFrom.Text), DateTime.Parse(dateTo.Text), (byte)1);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			//ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			//List<Loan> loanList=proxyClient.GetLoans();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			//ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			//Loan oneLoan = proxyClient.GetLoan(long.Parse(loanAppId.Text));
		}

		private void AddContactButton_Click(object sender, EventArgs e)
		{
			ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();
			if (currentContact == null)
			{
				currentContact = new Contact();
				currentContact.ContactAccountList = new List<ContactAccount>();
			}

			currentContact.Description = DescriptionTextBox.Text;

			proxyClient.AddContact(currentContact,100000080350);
		}

		private void UpdateContactButton_Click(object sender, EventArgs e)
		{
			ServiceReference1.ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();

			currentContact.Description = DescriptionTextBox.Text;

			proxyClient.UpdateContact(currentContact);

			getContactButton_Click(sender,e);

		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void getContactButton_Click(object sender, EventArgs e)
		{


			using (ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient())
			{
				ulong contactId = ulong.Parse(ContactIdTextBox.Text);

				currentContact = proxyClient.GetContact(contactId);
			}

			ContactIdTextBox.Text = "";
			DescriptionTextBox.Text = "";


			if (currentContact != null)
			{
				ContactIdTextBox.Text = currentContact.Id.ToString();
				DescriptionTextBox.Text = currentContact.Description;

				InitContactAccountsDataGridView();
			}
			else
				ContactAccountsDataGridView.Rows.Clear();

		}

		private void DeleteContactButton_Click(object sender, EventArgs e)
		{
			using (ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient())
			{
				ulong contactId = ulong.Parse(ContactIdTextBox.Text);

				proxyClient.DeleteContact(contactId);
			}
		}

		private void AddContactAccountbutton_Click(object sender, EventArgs e)
		{
			ContactAccountForm form = new ContactAccountForm();
			form.Action = 1;

			if (currentContact == null)
			{
				currentContact = new Contact();
				currentContact.ContactAccountList = new List<ContactAccount>();
			}

			form.currentContact = currentContact;
			form.ShowDialog();

			InitContactAccountsDataGridView();
		}

		private void UpdateContactAccountButton_Click(object sender, EventArgs e)
		{
			ContactAccountForm form = new ContactAccountForm();
			form.Action = 2;
			form.currentContact = currentContact;
			ulong AccountId = ulong.Parse(ContactAccountsDataGridView.SelectedRows[0].Cells["Id"].Value.ToString());
			form.contactAccount = currentContact.ContactAccountList.Find(m => m.Id == AccountId);
			form.ShowDialog();


			InitContactAccountsDataGridView();
		}
		private void DeleteContactAccountButton_Click(object sender, EventArgs e)
		{
			ulong AccountId = ulong.Parse(ContactAccountsDataGridView.SelectedRows[0].Cells["Id"].Value.ToString());
			currentContact.ContactAccountList.RemoveAll(m => m.Id == AccountId);
			InitContactAccountsDataGridView();
		}

		void InitContactAccountsDataGridView()
		{
			ContactAccountsDataGridView.Rows.Clear();
			foreach (ContactAccount ca in currentContact.ContactAccountList)
			{
				object[] row = new object[] { ca.Id, ca.AccountNumber, ca.Description };
				ContactAccountsDataGridView.Rows.Add(row);
			}
		}

		private void GetContactsButton_Click(object sender, EventArgs e)
		{
			ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();

			List<Contact> list= proxyClient.GetContacts(100000080350);
		}

		private void SaveUtilityPaymentButton_Click(object sender, EventArgs e)
		{
			UtilityPaymentOrder utilityPaymentOrder = new UtilityPaymentOrder();
            utilityPaymentOrder.Quality = OrderQuality.Draft;
			utilityPaymentOrder.Amount = 1000;
			utilityPaymentOrder.Branch = "111";
			utilityPaymentOrder.Code = "671801";
			utilityPaymentOrder.CommunalType = CommunalTypes.VivaCell;
			utilityPaymentOrder.Currency = "AMD";
			Account debitAccount = new Account();
			debitAccount.AccountNumber = 220005160068000;
            utilityPaymentOrder.DebitAccount = debitAccount;
			utilityPaymentOrder.Description = "Communal Payment";
			utilityPaymentOrder.OrderNumber = "2";
			utilityPaymentOrder.Type = OrderType.CommunalPayment;
			utilityPaymentOrder.fizJur = 1;
			utilityPaymentOrder.RegistrationDate = DateTime.Parse("23-mar-2015");

			ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();

			ActionResult actionResult = proxyClient.SaveUtiliyPaymentOrder(utilityPaymentOrder,100000080350,1);

			if (actionResult.ResultCode != ResultCode.Normal && actionResult.Errors.Count > 0)
			{
				MessageBox.Show(actionResult.Errors[0].Description);
			}
			else
			{
				MessageBox.Show("OK");
			}

		}

		private void button6_Click(object sender, EventArgs e)
		{
			ExternalBankingServiceClient proxyClient = new ExternalBankingServiceClient();

            var actionResult = proxyClient.GetUtilityPaymentOrder(583140, 111111111111, 1);

			
		}
	}
}
