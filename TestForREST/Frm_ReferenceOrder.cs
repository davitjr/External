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
    public partial class Frm_ReferenceOrder : Form
    {
        public Frm_ReferenceOrder()
        {
            InitializeComponent();
            Txt_Registartion_date.Text = "30/10/15";
            referenceType.Text = "2";
            txt_embassy.Text = "6";
            txt_language.Text = "2";
            txt_account1.Text = "220003185653000";
            //txt_account2.Text = "2200045781236";
            txt_filial.Text = "22001";
            txt_feeamount.Text = "3000";
            txt_feeaccount.Text = "220003185653000";

        }

        private void Frm_ReferenceOrder_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReferenceOrder order = new ReferenceOrder();

            order.Id = 0;
            order.Quality = OrderQuality.Draft;
            
            
            order.ReferenceType =Convert.ToUInt16(referenceType.Text.ToString());
            order.ReferenceEmbasy = Convert.ToUInt16(txt_embassy.Text.ToString());
            order.Accounts=new List<Account>();
            Account k = new Account();
            Account c = new Account();
            k.AccountNumber=ulong.Parse(txt_account1.Text.ToString());
            //c.AccountNumber = ulong.Parse(txt_account2.Text.ToString());
            order.Accounts.Add(k);
            //order.Accounts.Add(c);
            order.ReferenceLanguage = Convert.ToUInt16(txt_language.Text.ToString());
            order.ReferenceFilial = Convert.ToInt32(txt_filial.Text.ToString());
            order.FeeAmount = Convert.ToDouble(txt_feeamount.Text.ToString());
            order.FeeAccount = new Account();
            order.FeeAccount.AccountNumber = ulong.Parse(txt_feeaccount.Text);
            order.Type = OrderType.ReferenceOrder;
            order.SubType = 1;
            order.RegistrationDate = Frm_TransferArm.ToUnixEpoch(DateTime.Parse(Txt_Registartion_date.Text.ToString()));
            order.DateFrom = Frm_TransferArm.ToUnixEpoch(DateTime.Parse("01/01/01"));
             order.DateTo = Frm_TransferArm.ToUnixEpoch(DateTime.Parse("01/01/01"));
            string ddd=JsonRequest.SaveReferenceOrder(order, byte.Parse(languageComboBox.Text));
            textBox1.Text = ddd;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = JsonRequest.GetReferenceOrder((long)951433, byte.Parse(languageComboBox.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {

            textBox1.Text = JsonRequest.ApproveReferenceOrder((long)951433, byte.Parse(languageComboBox.Text));
           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = JsonRequest.GetReferenceOrderFee(checkBox1.Checked, byte.Parse(languageComboBox.Text));

        }
    }
}
