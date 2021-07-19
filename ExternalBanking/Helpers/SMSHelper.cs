using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ServiceClient;
using ExternalBanking.SMSMessagingService;

namespace ExternalBanking.Helpers
{
    public static class SmsHelper
    {
        /// <summary>
        /// Send Sms Helper
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="customerNumber"></param>
        /// <param name="messageText"></param>
        /// <param name="registrationSetNumber"></param>
        /// <param name="messageType"></param>
        public static void SendSms(string phoneNumber, ulong customerNumber, string messageText, int registrationSetNumber, int messageType)
        {
            if (!string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(messageText))
            {
                var message = new OneMessage
                {
                    PhoneNumber = phoneNumber,
                    CustomerNumber = customerNumber,
                    Message = messageText,
                    RegistrationSetNumber = registrationSetNumber,
                    MessageType = messageType,
                    Source = 1
                };

                SMSMessagingOperations.Use(client =>
                {
                    client.SendOneMessage(message);
                });
            }
        }
    }
}