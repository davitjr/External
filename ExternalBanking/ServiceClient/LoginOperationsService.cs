using ExternalBanking.ACBAServiceReference;
using System;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{
    public class LoginOperationsService
    {

        public static ClientPermissions GetVarPermissionForPage(ClientPermissionsInfo cpInfo)
        {
            ClientPermissions permissions = new ClientPermissions();

            Use(client =>
            {
                permissions = client.GetVarPermissionForPage(cpInfo);
            });

            return permissions;
        }

        private static void Use(Action<ILoginOperations> action)
        {
            ILoginOperations client = ProxyManager<ILoginOperations>.GetProxy(nameof(ILoginOperations));

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
