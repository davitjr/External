using ExternalBanking.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
            catch (FaultException ex)
            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (TimeoutException e)
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
                ((IClientChannel)client).Dispose();
            }
        }
    }
}
