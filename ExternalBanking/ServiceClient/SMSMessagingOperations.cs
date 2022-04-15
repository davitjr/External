using ExternalBanking.SMSMessagingService;
using System;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{

    class SMSMessagingOperations
    {
        public static void Use(Action<ISMSMessagingService> action)
        {

            ISMSMessagingService client = ProxyManager<ISMSMessagingService>.GetProxy(nameof(ISMSMessagingService));
            bool success = false;

            try
            {
                action(client);
                ((IClientChannel)client).Close();
                success = true;
            }
            catch (FaultException)
            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (TimeoutException)
            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (Exception)
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
                ((IClientChannel)client).Dispose();
            }
        }

    }
}
