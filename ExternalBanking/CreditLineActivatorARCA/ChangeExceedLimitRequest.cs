using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;

namespace ExternalBanking.CreditLineActivatorARCA
{
    public class ChangeExceedLimitRequest
    {
        public static List<ActionError> ActivateCreditLine(ulong customerNumber, ulong productId, long docId, short userId, SourceType source)
        {
            List<ActionError> warnings = new List<ActionError>();
            string cardNumber = "";

            if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
            {
                cardNumber = Card.GetCardNumber(Convert.ToInt64(productId));
            }
            else
            {
                /// <summary>
                /// Քարտի համար
                /// </summary>
                cardNumber = Card.GetCardNumberWithCreditLineAppId(productId, CredilLineActivatorType.ActivateCreditLine);
            }

            var cardAccountNumber = Card.GetCardAccountNumber(cardNumber);

            /// <summary>
            /// Վարկային գծի տվյալներ
            /// </summary>
            var creditLine = CreditLine.GetCardCreditLine(cardNumber);


            /// <summary>
            /// ստեղծել գործարքի համար
            /// </summary>
            var orderId = Utility.GetLastKeyNumber(32, 22000);

            /// <summary>        

            /// ԱՐՔԱ հարցման request
            /// </summary>
            var request = new ChangeExceedLimitOrderData();
            request.DocId = docId;
            request.IPAddress = userId.ToString();
            request.OrderId = orderId;

            request.CardIdentification = new CardIdentification();
            request.CardIdentification.CardNumber = cardNumber;
            var expDate = Card.GetExDate(cardNumber);
            expDate = expDate.Substring(2) + expDate.Substring(0, 2);
            request.CardIdentification.ExpiryDate = expDate;

            request.AccountNumber = cardAccountNumber == null ? cardNumber : cardAccountNumber;
            request.Currency = Convert.ToInt16(Utility.GetCurrencyCode(creditLine.Currency));
            request.Amount = Convert.ToDecimal(creditLine.StartCapital);
            request.ExpDate = creditLine.EndDate;
            request.ProductId = productId;

            /// <summary>
            /// ԱՐՔԱ հարցում
            /// </summary>
            var response = new ChangeExceedLimitResponse();
            response = ArcaDataService.ChangeExceedLimit(request);

            /// <summary>
            /// Հաղորդագրության ուղարկում հաճախորդին վարկային գծի ակտիվացման վերաբերյալ
            /// </summary>
            if (response.ResponseCode == "00" || response.ResponseCode == "-1")
            {
                warnings.AddRange(new List<ActionError> { new ActionError { Description = "Վարկային գիծը ակտիվացել է" } });
                try
                {
                    warnings.Add(CreditLineMessageRequest.SendMessage(customerNumber, creditLine, cardNumber, true, creditLine.Type));
                }
                catch { }
                if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                {

                    productId = (ulong)creditLine.ProductId;
                }

                CreditLine.UpdateCreditLinesFnameOnline(productId);
                CreditLine.SaveCreditLineByApiGate(docId, productId, orderId);
            }
            else
            {
                warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                warnings.Add(CreditLineMessageRequest.SendMessage(customerNumber, creditLine, cardNumber, false, creditLine.Type));
            }

            return warnings;
        }

        public static List<ActionError> CloseCreditLine(ulong customerNumber, ulong productId, long docId, short userId, string expiryDate = "")
        {
            List<ActionError> warnings = new List<ActionError>();

            /// <summary>
            /// Քարտի համար
            /// </summary>
            var cardNumber = Card.GetCardNumberWithCreditLineAppId(productId, CredilLineActivatorType.CloseCreditLine);

            var result = ArcaDataService.GetBalance(cardNumber);

            if (result.AvailableExceedLimit != "0.00")
            {
                var cardAccountNumber = Card.GetCardAccountNumber(cardNumber);

                /// <summary>
                /// Վարկային գծի տվյալներ
                /// </summary>
                var creditLine = new CreditLine();
                creditLine = CreditLine.GetClosedCreditLine(productId, customerNumber);


                /// <summary>
                /// ստեղծել գործարքի համար
                /// </summary>
                var orderId = Utility.GetLastKeyNumber(32, 22000);

                /// <summary>
                /// ԱՐՔԱ հարցման request
                /// </summary>
                var request = new ChangeExceedLimitOrderData();
                request.DocId = docId;
                request.IPAddress = userId.ToString();
                request.OrderId = orderId;


                request.CardIdentification = new CardIdentification();
                string expDate;
                request.CardIdentification.CardNumber = cardNumber;
                if (string.IsNullOrEmpty(expiryDate))
                {
                    expDate = Card.GetExDate(cardNumber);
                }
                else
                {
                    expDate = expiryDate;
                }
                expDate = expDate.Substring(2) + expDate.Substring(0, 2);
                request.CardIdentification.ExpiryDate = expDate;

                request.AccountNumber = cardAccountNumber == null ? cardNumber : cardAccountNumber;
                request.Currency = Convert.ToInt16(Utility.GetCurrencyCode(creditLine.Currency));
                request.Amount = 0;
                request.ExpDate = creditLine.EndDate;
                request.ProductId = productId;

                /// <summary>
                /// ԱՐՔԱ հարցում
                /// </summary>
                var response = new ChangeExceedLimitResponse();
                response = ArcaDataService.ChangeExceedLimit(request);


                // < summary >
                // Հաղորդագրության ուղարկում հաճախորդին վարկային գծի ակտիվացման վերաբերյալ
                // </ summary >

                if (response.ResponseCode == "00" || response.ResponseCode == "-1")
                {
                    try
                    {
                        warnings.AddRange(new List<ActionError> { new ActionError { Description = "Վարկային գիծը դադարել է" } });
                        //warnings.Add(CreditLineMessageRequest.SendMessage(customerNumber, creditLine, cardNumber, true,creditLine.Type));
                    }
                    catch { }
                    CreditLine.UpdateCloseCreditLinesFnameOnline(productId);
                    CreditLine.SaveCreditLineByApiGate(docId, productId, orderId);
                }
                else
                {
                    warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                    //warnings.Add(CreditLineMessageRequest.SendMessage(customerNumber, creditLine, cardNumber, false, creditLine.Type));
                }

            }
            else
            {
                warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
            }
            return warnings;
        }

        public static bool ChekArCaBalance(ulong productId)
        {
            double availableExceedLimit;
            bool isValid = true;


            var cardNumber = Card.GetCardNumberWithCreditLineAppId(productId, CredilLineActivatorType.ActivateCreditLine);

            /// <summary>
            /// Վարկային գծի տվյալներ
            /// </summary>
            var creditLine = CreditLine.GetCardCreditLine(cardNumber);

            var result = ArcaDataService.GetBalance(cardNumber);
            availableExceedLimit = Convert.ToDouble(result.AvailableExceedLimit);

            if (result.AvailableExceedLimit != "0.00" || availableExceedLimit != 0)
            {
                if (creditLine.Type != 50)
                {
                    if (creditLine.StartCapital > availableExceedLimit)
                    {
                        isValid = false;
                    }
                }
                else
                {
                    double arcaLimit;
                    arcaLimit = CreditLine.GetArcaLimitBallance(productId);

                    if (creditLine.Currency == "AMD")
                    {
                        string roundArcaLimit;
                        int index;
                        roundArcaLimit = arcaLimit.ToString();
                        index = roundArcaLimit.IndexOf(".");
                        if (index != -1)
                            roundArcaLimit = roundArcaLimit.Substring(0, index - 1) + "0";
                        else
                            roundArcaLimit = roundArcaLimit.Substring(0, roundArcaLimit.Length - 1) + "0";
                        arcaLimit = Convert.ToDouble(roundArcaLimit);
                    }

                    if (arcaLimit > availableExceedLimit)
                    {
                        isValid = false;
                    }

                }
            }

            return isValid;
        }

        public static List<ActionError> ProlongCreditLine(ulong customerNumber, ulong productId, long docId, short userId, OrderType type)
        {
            List<ActionError> warnings = new List<ActionError>();
            string cardNumber = "";

            /// <summary>
            /// Քարտի համար
            /// </summary>

            if (type == OrderType.CardRenewOrder)
                cardNumber = Card.GetCardNumber(Convert.ToInt64(productId));
            else
                cardNumber = Card.GetCardNumberWithCreditLineAppId(productId, CredilLineActivatorType.ActivateCreditLine);



            var cardAccountNumber = Card.GetCardAccountNumber(cardNumber);

            /// <summary>
            /// Վարկային գծի տվյալներ
            /// </summary>
            var creditLine = CreditLine.GetCardCreditLine(cardNumber);

            if (type == OrderType.CardRenewOrder && IsProlongApiGate((ulong)creditLine.ProductId) != true)
                return warnings;

            /// <summary>
            /// ստեղծել գործարքի համար
            /// </summary>
            var orderId = Utility.GetLastKeyNumber(32, 22000);

            /// <summary>        

            /// ԱՐՔԱ հարցման request
            /// </summary>
            var request = new ChangeExceedLimitOrderData();
            request.DocId = docId;
            request.IPAddress = userId.ToString();
            request.OrderId = orderId;

            request.CardIdentification = new CardIdentification();
            request.CardIdentification.CardNumber = cardNumber;
            var expDate = Card.GetExDate(cardNumber);
            expDate = expDate.Substring(2) + expDate.Substring(0, 2);
            request.CardIdentification.ExpiryDate = expDate;

            request.AccountNumber = cardAccountNumber == null ? cardNumber : cardAccountNumber;
            request.Currency = Convert.ToInt16(Utility.GetCurrencyCode(creditLine.Currency));
            request.Amount = Convert.ToDecimal(creditLine.StartCapital);
            request.ExpDate = creditLine.EndDate;
            request.ProductId = productId;

            /// <summary>
            /// ԱՐՔԱ հարցում
            /// </summary>
            var response = new ChangeExceedLimitResponse();
            response = ArcaDataService.ChangeExceedLimit(request);


            if (response.ResponseCode == "00" || response.ResponseCode == "-1")
            {
                warnings.AddRange(new List<ActionError> { new ActionError { Description = "Վարկային գիծը Երկարաձգվել է" } });
                if (type == OrderType.CardRenewOrder)
                {
                    productId = (ulong)creditLine.ProductId;
                }
                CreditLine.UpdateCreditLinesFnameOnline(productId);
                CreditLine.SaveCreditLineByApiGate(docId, productId, orderId);
            }
            else
            {
                warnings.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
            }

            return warnings;
        }

        public static bool IsProlongApiGate(ulong appId)
        {
            return CreditLine.IsProlongApiGate(appId);
        }
    }
}
