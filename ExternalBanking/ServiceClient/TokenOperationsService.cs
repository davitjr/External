using ExternalBanking.TokenOperationsServiceReference;

using System;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{
    class TokenOperationsService
    {

        public static TokenOperationsResult ActivateToken(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.ActivateToken(servletRequest);
            });

            return result;
        }

        public static TokenOperationsResult ActivateMobileToken(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.ActivateMobileToken(servletRequest);
            });

            return result;
        }

        public static TokenOperationsResult UnlockToken(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.UnlockToken(servletRequest);
            });

            return result;
        }

        public static TokenOperationsResult BlockToken(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.BlockToken(servletRequest);
            });

            return result;
        }


        public static TokenOperationsResult BlockUser(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.BlockUser(servletRequest);
            });

            return result;
        }

        public static string GetPinCode(string cardNumber)
        {
            var result = "";

            Use(client =>
            {
                result = client.GetPinCode(cardNumber);
            });

            return result;
        }


        public static TokenOperationsResult ResetUserPasswordManualy(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.ResetUserPasswordManualy(servletRequest);
            });

            return result;
        }

        private static void Use(Action<ITokenOperations> action)
        {
            ITokenOperations client = ProxyManager<ITokenOperations>.GetProxy(nameof(ITokenOperations));

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
