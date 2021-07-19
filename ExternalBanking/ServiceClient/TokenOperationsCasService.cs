using ExternalBanking.TokenOperationsCasServiceReference;
using System;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{
    class TokenOperationsCasService
    {

        public static TokenOperationsResult ActivateToken(TokenOperationsInfo servletRequest, ulong customerNumber, string email, int customerQuality, bool isRegistered)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.ActivateToken(servletRequest, customerNumber, email, customerQuality, isRegistered);
            });

            return result;
        }

        public static TokenOperationsResult ActivateMobileToken(TokenOperationsInfo servletRequest, string password, TokenOperationsCasServiceReference.SourceType sourceType, bool isRegistered, string phoneNumber, ulong customerNumber, string email, int customerQuality)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.ActivateMobileToken(servletRequest, password, sourceType, isRegistered, phoneNumber, customerNumber, email, customerQuality);
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
        public static TokenOperationsResult UnlockUser(TokenOperationsInfo servletRequest, string phoneNumber, ulong customerNumber, TokenOperationsCasServiceReference.SourceType sourceType, byte Language)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.UnlockUser(servletRequest, phoneNumber, customerNumber, sourceType, Language);
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

        public static TokenOperationsResult DeactivateUser(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.DeactivateUser(servletRequest);
            });

            return result;
        }
        
        public static TokenOperationsResult UnBlockUser(TokenOperationsInfo servletRequest)
        {
            var result = new TokenOperationsResult();

            Use(client =>
            {
                result = client.UnBlockUser(servletRequest);
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

        private static void Use(Action<ITokenOperationsCas> action)
        {
            ITokenOperationsCas client = ProxyManager<ITokenOperationsCas>.GetProxy(nameof(ITokenOperationsCas));

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
