using ExternalBanking.SMSMessagingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.CreditLineActivatorARCA
{
    public class CreditLineMessageRequest
    {
        /// <summary>
        /// Հաղորդագրության մշակում և ուղարկում հաճախորդին
        /// </summary>
        public static ActionError SendMessage(ulong customerNumber, CreditLine creditLine, string cardNumber, bool isActive,int creditLineType)
        {
            var actionError = new ActionError();
            var message = new OneMessage();
            string creditLineReason = "";
            string cardNumberMasked = string.Format("{0}XXXXXX{1}", cardNumber.Substring(0, 6), cardNumber.Substring(cardNumber.Length - 4));
            if (creditLineType == 54)
                creditLineReason = "arag overdrafty";
            else
                creditLineReason = "varkayin gitsy";
            message.PhoneNumber = Customer.GetCustomerPhoneNumber(customerNumber);
            


            if(message.PhoneNumber != null)
            {
                message.MessageType = 39;
                message.CustomerNumber = customerNumber;
                message.ExternalId = "ExternalBanking";
                if (isActive == true)
                    message.Message = $"Hargeli hachakhord, Dzer {cardNumberMasked} hamari qartin {creditLine.StartCapital} {creditLine.Currency} {creditLineReason} aktivacel e, 010318888, acba.am";
                else
                    message.Message = $"Hargeli hachakhord, Dzer {cardNumberMasked} hamari qartin {creditLine.StartCapital} {creditLine.Currency} {creditLineReason} kaktivana aravelaguyny 1 ashkhatanqayin orva yntacqum, 010318888, acba.am";


                using (SMSMessagingServiceClient client = new SMSMessagingServiceClient())
                {
                    try
                    {
                        client.SendOneMessage(message);
                    }
                    catch
                    {
                        actionError.Description = "Հաճախորդին sms հաղորդագրություն չի ուղղարկվել";
                    }

                }
            }
            else
            {
                actionError.Description = "Հաճախորդին sms հաղորդագրություն չի ուղղարկվել";
            }

            
            return actionError;
        }
    }
}
