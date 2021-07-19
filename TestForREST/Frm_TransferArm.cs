using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestForREST
{
    public partial class Frm_TransferArm : Form
    {
        public Frm_TransferArm()
        {
            InitializeComponent();
            Txt_OrderNumber.Text="123";
            txt_amount.Text = "1000";
            txt_currency.Text="AMD";
            Txt_DebitAccountNumber.Text= "220001957038000";
            Txt_ReceiverAccountNumber.Text="220004305789000";
            Txt_RegistrationDate.Text = DateTime.Now.ToString("dd/MM/yy");
            txt_Description.Text="Տեստային փոխանցում";
            Txt_Receiver.Text="Դավիդ Շանոյան";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            PaymentOrder order = new PaymentOrder();
            order.Id = 0;
            order.Quality = 1;
            order.OrderNumber = Txt_OrderNumber.Text;
            order.Amount = Convert.ToDouble(txt_amount.Text.ToString());
            order.Currency = txt_currency.Text;
            order.DebitAccount = new Account();
            order.DebitAccount.AccountNumber =  ulong.Parse(Txt_DebitAccountNumber.Text);
            //order.DebitAccount.Currency = "AMD";
            order.ReceiverAccount = new Account();
            order.ReceiverAccount.AccountNumber = ulong.Parse(Txt_ReceiverAccountNumber.Text );
            //order.ReceiverAccount.Currency = "AMD";
            //order.ReceiverBankCode = 22000;
            order.ReceiverBankCode = short.Parse(Txt_ReceiverAccountNumber.Text.Substring(0,5));
            order.RegistrationDate = ToUnixEpoch(DateTime.Parse(Txt_RegistrationDate.Text.ToString()));
            order.Description = txt_Description.Text;
            order.Receiver = Txt_Receiver.Text;
            order.Type = 1;
            order.SubType = 1;


            string result = JsonRequest.SavePaymentOrder(order,byte.Parse(languageComboBox.Text));
        }

        public static string ToUnixEpoch(DateTime dateTime)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dateTime;
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return "/Date(" + ts.TotalMilliseconds.ToString("#") + ")/";
        }

        private void Txt_OrderNumber_TextChanged(object sender, EventArgs e)
        {

        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Txt_DebitAccountNumber_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
