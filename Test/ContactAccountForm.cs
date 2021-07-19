using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.ServiceReference1;

namespace Test
{
	public partial class ContactAccountForm : Form
	{
		public ushort Action { get; set;}

		public Contact currentContact{ get; set;}

		public ContactAccount contactAccount { get; set;}

		public ContactAccountForm()
		{
			InitializeComponent();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
			this.Dispose();
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			if (Action == 2)
			{
				contactAccount = new ContactAccount();
                currentContact.ContactAccountList.Add(contactAccount);
				contactAccount.AccountNumber = ContactAccountAccountNumberTextBox.Text;
				contactAccount.Description = ContactAccountDescriptionTextBox.Text;
			}
			else if (Action == 2)
			{
				contactAccount.Description = ContactAccountDescriptionTextBox.Text;
                contactAccount.AccountNumber = ContactAccountAccountNumberTextBox.Text;
			}
			this.Close();
			this.Dispose();
		}

		private void ContactAccountForm_Load(object sender, EventArgs e)
		{
			if (Action == 2)
			{
				ContactAccountAccountNumberTextBox.Text = contactAccount.AccountNumber;
				ContactAccountDescriptionTextBox.Text = contactAccount.Description;
				ContactAccountIDTextBox.Text = contactAccount.Id.ToString();

			}

			ContactIdTextBox.Text = currentContact.Id.ToString();
		}
	}
}
