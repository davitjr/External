using ExternalBanking.ArcaDataServiceReference;
using System;
using System.ServiceModel;

namespace ExternalBanking.ServiceClient
{
    public class ArcaDataService
    {

        public static ArcaCardsTransactionResponse MakeCardTransaction(CardTransactionType transactionType, ArcaCardsTransactionOrderData data)
        {
            var arcaCardsTransactionResponse = new ArcaCardsTransactionResponse();

            Use(client =>
            {
                arcaCardsTransactionResponse = client.MakeCardTransaction(transactionType, data);
            });

            return arcaCardsTransactionResponse;
        }

        public static ArcaDataServiceReference.ArcaBalanceResponseData GetBalance(string cardNumber)
        {
            var arcaResponseData = new ArcaDataServiceReference.ArcaBalanceResponseData();

            Use(client =>
            {
                arcaResponseData = client.GetBalance(cardNumber);
            });

            return arcaResponseData;
        }

        public static CardLimitChangeResponse ChangeCardLimit(CardLimitChangeOrderData data)
        {
            var cardLimitChangeResponse = new CardLimitChangeResponse();

            Use(client =>
            {
                cardLimitChangeResponse = client.ChangeCardLimit(data);
            });

            return cardLimitChangeResponse;
        }

        public static C2CTransferResponse C2CTransfer(C2CTransferRequest request)
        {
            var response = new C2CTransferResponse();

            Use(client =>
            {
                response = client.C2CTransfer(request);
            });

            return response;
        }

        public static TransactionDetailsBResponse Check(TransactionStatusRequest req)
        {
            var response = new TransactionDetailsBResponse();

            Use(client =>
            {
                response = client.Check(req);
            });

            return response;
        }

        public static TransactionResponse MakeTransaction(TransactionRequest req)
        {
            var response = new TransactionResponse();

            Use(client =>
            {
                response = client.MakeTransaction(req);
            });

            return response;
        }
        public static ChangeExceedLimitResponse ChangeExceedLimit(ChangeExceedLimitOrderData data)
        {
            var response = new ChangeExceedLimitResponse();

            Use(client =>
            {
                response = client.ChangeExceedLimit(data);
            });
            return response;
        }
        public static bool CheckEmbossingName(CardIdentification identification)
        {
            bool response = false;
            Use(client =>
            {
                response = client.CheckCardEmbossingName(identification);
            });

            return response;
        }

        public static CreditCardEcommerceResponse CreditCardEcommerce(CreditCardEcommerceOrderData ecommerce)
        {
            CreditCardEcommerceResponse response = null;
            Use(client =>
            {
                response = client.CreditCardEcommerce(ecommerce);
            });
            return response;

        }
        private static void Use(Action<IArcaDataService> action)
        {
            IArcaDataService client = ProxyManager<IArcaDataService>.GetProxy(nameof(IArcaDataService));

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
        public static int GetCardArCaStatus(CardIdentification cardIdentification)
        {
            int status = 0;

            Use(client =>
            {
                status = client.GetCardArCaStatus(cardIdentification);
            });

            return status;
        }
        public static getCardDataResponseType GetCardData(CardIdentification card)
        {
            getCardDataResponseType response = new getCardDataResponseType();

            getCardDataRequestType cardData = new getCardDataRequestType
            {
                cardIdentificationField = new cardIdentificationType()
            };
            cardData.cardIdentificationField.itemElementNameField = ItemChoiceType.cardNumber;
            cardData.cardIdentificationField.itemField = card.CardNumber;
            cardData.cardIdentificationField.expDateField = card.ExpiryDate;

            Use(client =>
            {
                response = client.GetCardData(cardData);
            });

            return response;
        }
    }
}
