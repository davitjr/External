using ExternalBanking.EmailMessagingService;

namespace ExternalBanking
{
    public class EmailMessagingOperations
    {
        public static void SendEmail(Email email)
        {
            using EmailMessagingServiceClient client = new EmailMessagingServiceClient();
            client.SendEmailNotification(email);
        }
    }
}
