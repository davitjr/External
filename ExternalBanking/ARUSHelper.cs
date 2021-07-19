using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ExternalBanking.ARUSDataService;
using System.Net;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{
    /// <summary>
    /// ARUSDataService-ի հետ հաղորդակցման class
    /// </summary>
    public static class ARUSHelper
    {
        /// <summary>
        /// ARUS համակարգում վավերականացում
        /// </summary>
        /// <param name="action"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        public static void Use(Action<IARUSOperationService> action, string authorizedUserSessionToken, string userName, string clientIP)
        {          

            bool success = false;

            IARUSOperationService client = ProxyManager<IARUSOperationService>.GetProxy(nameof(IARUSOperationService));

            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.InitARUSUser(authorizedUserSessionToken, userName, clientIP);

                action(client);
                ((IClientChannel)client).Close();

                success = true;

            }
            catch (FaultException ex)
            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (TimeoutException e)
            {

            }
            catch (WebException ex)
            {

            }
            catch (Exception e)
            {
                ((IClientChannel)client).Abort();

                throw;

            }
            finally
            {
                if (!success)
                {
                    ((IClientChannel)client).Abort();

                }
                ((IClientChannel)client).Close();
                ((IClientChannel)client).Dispose();

            }
        }

        /// <summary>
        /// ARUS համակարգի օբյեկտը վերածում է ExternalBanking համակարգի օբյեկտի և հակառակը
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <param name="destinationObject"></param>
        public static void ConvertObject<I, T>(this I sourceObject, ref T destinationObject)
        {
            if (sourceObject == null || destinationObject == null)
                return;

            Type sourceType = typeof(I);
            Type targetType = typeof(T);

            foreach (PropertyInfo p in sourceType.GetProperties())
            {

                PropertyInfo targetObj = targetType.GetProperty(p.Name);
                if (targetObj == null)
                    continue;

                targetObj.SetValue(destinationObject, p.GetValue(sourceObject, null), null);
            }
        }

        /// <summary>
        /// ARUSDataService-ի ActionResult տեսակի օբյեկտը վերածում է ExternalBanking.ActionResult տեսակի օբյեկտի
        /// </summary>
        /// <param name="ARUSActionResult"></param>
        /// <returns></returns>
        public static ActionResult ConvertARUSActionResultToXBActionResult(ARUSDataService.ActionResult ARUSActionResult)
        {
            ActionResult XBActionResult = new ActionResult();
            if(ARUSActionResult != null)
            {
                XBActionResult.ResultCode = (ResultCode)(short)ARUSActionResult.ResultCode;
                List<ActionError> XBActionErrors = new List<ActionError>();

                foreach(ARUSDataService.ActionError ARUSError in ARUSActionResult.Errors)
                {
                    ActionError XBError = new ActionError();
                    ConvertObject(ARUSError, ref XBError);
                    XBActionErrors.Add(XBError);
                }

                XBActionResult.Errors = XBActionErrors;

            }
            else
            {
                XBActionResult = null;
            }
            return XBActionResult;
        }

        /// <summary>
        /// Ստեղծում է ARUS համակարգին դիմելու համար հաղորադագրության ունիկալ համար
        /// </summary>
        /// <returns></returns>
        public static ulong GenerateMessageUniqueNo()
        {
            return Utility.GetLastKeyNumber(83, 22000);
        }

        /// <summary>
        /// ARUS համակարգին համապատասխան ամսաթիվը (YYYYMMDD ֆորմատով) վերափոխում է dd/MM/yyyy ֆորմատով ամսաթվի
        /// </summary>
        /// <param name="ARUSDateString">ARUS համակարգին համապատասխան ամսաթիվ</param>
        /// <returns></returns>
        public static string ConvertARUSDateStringToString(string ARUSDateString)
        {
            string dateString = "";

            if(!String.IsNullOrEmpty(ARUSDateString))
            {
                dateString = ARUSDateString.Substring(6) + "/" + ARUSDateString.Substring(4, 2) + "/" + ARUSDateString.Substring(0, 4);
            }

            return dateString;
        }




    }
}
