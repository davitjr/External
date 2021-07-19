using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.EmailMessagingService;


namespace ExternalBanking
{
    public class EmailMessagingOperations
    {
        public static void SendMobileBankingCustomerDetailsRiskyChangeAlert(string TokenSerial)
        {
            string mailContent = EmailMessagingOperationsDB.GetRiskyChangesAlertMailContent(TokenSerial);

            using (EmailMessagingServiceClient client = new EmailMessagingServiceClient())
            {
                Email email = new Email
                {
                    Content = mailContent,
                    Subject = "ACBA DIGITAL alert",
                    From = (int)EmailSenderProfiles.Notifications,
                    To = "digitalmonitoring@acba.am"
                };

                client.SendEmailNotification(email);
            }

        }
    }
}
