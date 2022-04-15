using ExternalBanking.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{
    public class ACBAOperationService
    {

        public static short GetCustomerType(ulong customerNumber)
        {
            short type = 0;
            ACBAOperationService.Use(client =>
            {
                type = client.GetCustomerType(customerNumber);
            });
            return type;
        }

        public static ACBAServiceReference.KeyValue GetCustomerFilial(ulong customerNumber)
        {
            ACBAServiceReference.KeyValue filial = new KeyValue();
            ACBAOperationService.Use(client =>
            {
                filial = client.GetCustomerFilial(customerNumber);
            }
            );

            return filial;
        }

        public static short GetCustomerLinkType(ulong customerNumber)
        {
            short type = 0;
            ACBAOperationService.Use(client =>
            {
                type = client.GetCustomerLinkType(customerNumber);
            });
            return type;
        }

        public static ulong GetIdentityId(ulong customerNumber)
        {
            ulong identityID = 0;

            ACBAOperationService.Use(client =>
            {
                identityID = client.GetIdentityId(customerNumber);
            });
            return identityID;
        }


        public static string GetCustomerDescription(ulong customerNumber)
        {
            string customerDescription = "";
            ACBAOperationService.Use(client =>
            {
                customerDescription = client.GetCustomerDescription(customerNumber);
            });

            return customerDescription;
        }

        public static byte CheckCustomerUpdateExpired(ulong customerNumber)
        {
            byte updateExpired = 0;

            ACBAOperationService.Use(client =>
            {
                updateExpired = client.CheckCustomerUpdateExpired(customerNumber);
            }
            );

            return updateExpired;
        }

        public static bool HasCustomerBankruptBlockage(ulong customerNumber)
        {
            bool hasBankruptCustomers = false;
            ACBAOperationService.Use(client =>
            {
                hasBankruptCustomers = client.HasCustomerBankruptBlockage(customerNumber);
            });
            return hasBankruptCustomers;
        }

        public static CustomerMainData GetCustomerMainData(ulong customerNumber)
        {
            CustomerMainData customerMainData = new CustomerMainData();
            ACBAOperationService.Use(client =>
            {
                customerMainData = (CustomerMainData)client.GetCustomerMainData(customerNumber);
            }
           );
            return customerMainData;
        }

        public static ulong GetLinkedCustomerNumber(ulong customerNumber)
        {
            ulong linkCustomerNumber = 0;
            ACBAOperationService.Use(client =>
            {
                linkCustomerNumber = client.GetLinkedCustomerNumber(customerNumber);
            });
            return linkCustomerNumber;
        }

        public static ACBAServiceReference.Customer GetCustomer(ulong customerNumber)
        {
            ACBAServiceReference.Customer customer = new ACBAServiceReference.Customer();

            ACBAOperationService.Use(client =>
            {
                customer = (ACBAServiceReference.Customer)(client.GetCustomer(customerNumber));
            }
            );

            return customer;
        }

        public static List<LinkedCustomer> GetCustomerLinkedPersonsList(ulong customerNumber, int quality)
        {
            List<LinkedCustomer> results = new List<LinkedCustomer>();

            ACBAOperationService.Use(client =>
            {
                results = client.GetCustomerLinkedPersonsList(customerNumber, quality);
            });

            return results;
        }

        public static ACBAServiceReference.VipData GetCustomerVipData(ulong customerNumber)
        {
            ACBAServiceReference.VipData results = new ACBAServiceReference.VipData();

            ACBAOperationService.Use(client =>
            {
                results = client.GetCustomerVipData(customerNumber);
            });

            return results;
        }

        public static bool CheckCustomerInvolvingEmployeeFilial(short setNumber, short filialCode)
        {
            var results = false;

            ACBAOperationService.Use(client =>
            {
                results = client.CheckCustomerInvolvingEmployeeFilial(setNumber, filialCode);
            });

            return results;
        }

        public static VerificationData GetIdentityVerificationData(uint identityId)
        {
            var results = new VerificationData();

            ACBAOperationService.Use(client =>
            {
                results = client.GetIdentityVerificationData(identityId);
            });

            return results;
        }

        public static bool HasCustomerArrest(ulong customerNumber)
        {
            var results = true;

            ACBAOperationService.Use(client =>
            {
                results = client.HasCustomerArrest(customerNumber);
            });

            return results;
        }

        public static Dictionary<uint, List<SearchCustomers>> FindCustomers(SearchCustomers search, int pageNumber)
        {
            var results = new Dictionary<uint, List<SearchCustomers>>();

            ACBAOperationService.Use(client =>
            {
                results = client.FindCustomers(search, pageNumber);
            });

            return results;
        }


        public static List<CustomerDocument> GetCustomerDocumentList(ulong customerNumber)
        {
            var result = new List<CustomerDocument>();
            uint identityId = (uint)GetIdentityId(customerNumber);

            ACBAOperationService.Use(client =>
            {
                result = client.GetCustomerDocumentList(identityId, 1);
            });

            return result;
        }

        public static short GetCustomerQuality(ulong customerNumber)
        {
            short results = 0;

            ACBAOperationService.Use(client =>
            {
                results = client.GetCustomerQuality(customerNumber);
            });

            return results;
        }
        public static bool CheckCriminal(string firstName, string lastName)
        {
            bool result = false;
            ACBAOperationService.Use(client =>
            {
                result = client.CheckCriminal(firstName, lastName);
            });

            return result;
        }

        public static ACBAServiceReference.KeyValue GetCustomerRiskQuality(ulong customerNumber)
        {
            ACBAServiceReference.KeyValue riskQuality = new KeyValue();
            ACBAOperationService.Use(client =>
            {
                riskQuality = client.GetCustomerRiskQuality(customerNumber);
            }
            );

            return riskQuality;
        }

        public static DateTime? GetCustomerBirthDate(ulong customerNumber)
        {
            DateTime? result = null;
            ACBAOperationService.Use(client =>
            {
                result = client.GetCustomerBirthDate(customerNumber);
            }
            );

            return result;
        }

        public static CustomerPhone GetCustomerMainMobilePhone(ulong customerNumber)
        {
            CustomerPhone customerPhone = new CustomerPhone();
            Use(client =>
            {
                customerPhone = client.GetCustomerMainMobilePhone(customerNumber);
            });
            return customerPhone;
        }

        private static void Use(Action<ICustomerOperations> action)
        {
            ICustomerOperations client = ProxyManager<ICustomerOperations>.GetProxy(nameof(ICustomerOperations));

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

        public static ViolationRequestResponse RegisterPayment(CBViolationPayment policePayment, ACBAServiceReference.User user)
        {
            var results = new ViolationRequestResponse();

            UseExternalOperations(client =>
            {
                results = client.RegisterPayment(policePayment);
            }, user);

            return results;
        }

        public static ViolationRequestResponse GetVehicleViolationById(string violationId, ACBAServiceReference.User user)
        {
            var results = new ViolationRequestResponse();

            UseExternalOperations(client =>
            {
                results = client.GetVehicleViolationById(violationId);
            }, user);

            return results;
        }

        public static ViolationRequestResponse GetVehicleViolationByPsnVehNum(string psn, string vehNum, ACBAServiceReference.User user)
        {
            var results = new ViolationRequestResponse();

            UseExternalOperations(client =>
            {
                results = client.GetVehicleViolationByPsnVehNum(psn, vehNum);
            }, user);

            return results;
        }

        public static string GetCustomerDescriptionEnglish(ulong customerNumber)
        {
            string customerDescription = "";
            ACBAOperationService.Use(client =>
            {
                customerDescription = client.GetCustomerDescriptionEnglish(customerNumber);
            });

            return customerDescription;
        }

        public static void UseExternalOperations(Action<IExternalOperations> action, ACBAServiceReference.User user)
        {
            IExternalOperations client = ProxyManager<IExternalOperations>.GetProxy(nameof(IExternalOperations));

            client.SetUser(user, "");

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


        /// <summary>
        /// Ստուգում է օրենսդրական ծանուցումների ստացման եղանակների առկայությունը «SAP CRM» ծրագրում
        /// </summary>
        public static bool HasLegalCommunication(ulong customerNumber)
        {
            bool hasLegalCommunication = false;
            Use(client =>
            {
                hasLegalCommunication = client.HasLegalCommunication(customerNumber);
            });
            return hasLegalCommunication;
        }
    }
}
