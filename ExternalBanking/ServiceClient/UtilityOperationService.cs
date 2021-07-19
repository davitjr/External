using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.ServiceClient
{
    public class UtilityOperationService
    {

        public static BeelineAbonentCheckRequestResponse BeelineAbonentNumberCheck(string abonentNumber, string amount)
        {
            var result = new BeelineAbonentCheckRequestResponse();

            Use(client =>
            {
                result = client.BeelineAbonentNumberCheck(abonentNumber,amount);
            });

            return result;
        }

        public static GasPromSectionCodeRequestResponse GetGasPromSectionCodes()
        {

            var result = new GasPromSectionCodeRequestResponse();

            Use(client =>
            {
                result = client.GetGasPromSectionCodes();
            });

            return result;
        }


        public static GasPromSearchRequestResponse GasPromAbonentSearch(GasPromSearchInput gasSearch)
        {

            var result = new GasPromSearchRequestResponse();

            Use(client =>
            {
                result = client.GasPromAbonentSearch(gasSearch);
            });

            return result;
        }

        public static UcomMobileSearchAccountData UcomMobileAccountData(string phoneNumber)
        {

            var result = new UcomMobileSearchAccountData();

            Use(client =>
            {
                result = client.UcomMobileAccountData(phoneNumber);
            });

            return result;
        }

        public static VivaCellPaymentBTFCheckRequestResponse VivaCellBTFCheck(string transferNote, double amount)
        {

            var result = new VivaCellPaymentBTFCheckRequestResponse();

            Use(client =>
            {
                result = client.VivaCellBTFCheck(transferNote,amount);
            });

            return result;
        }

        public static VivaCellPaymentCheckRequestResponse VivaCellSubscriberCheck(string phoneNumber)
        {

            var result = new VivaCellPaymentCheckRequestResponse();

            Use(client =>
            {
                result = client.VivaCellSubscriberCheck(phoneNumber);
            });

            return result;
        }

        public static UcomFixAbonentCheckRequestResponse UcomFixAbonentNumberCheck(string abonentNumber)
        {
            var result = new UcomFixAbonentCheckRequestResponse();

            Use(client =>
            {
                result = client.UcomFixAbonentNumberCheck(abonentNumber);
            });

            return result;
        }

        private static void Use(Action<IUtilityOperationService> action)
        {
            IUtilityOperationService client = ProxyManager<IUtilityOperationService>.GetProxy(nameof(IUtilityOperationService));

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
