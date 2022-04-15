using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտից քարտ փոխանցման հայտ
    /// </summary>
    public class C2CTransferOrder
    {
        public CardIdentificationData SourceCard { get; set; }

        public string DestinationCardNumber { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public long OrderID { get; set; }

        public long SourceID { get; set; }

        public ulong TransferID { get; set; }

        public int Status { get; set; }

        public string Receiver { get; set; }

        public C2CTransferResult Save()
        {
            // 1 նոր գրանցում
            // 5 ուղարկվում է ArCa
            // 7 ուղարկված է ArCa
            // 10 հաջողված փոխանցում
            // 20 արտաքին սխալ
            // 30 ներքին սխալ

            C2CTransferResult result = new C2CTransferResult();
            try
            {

                ActionResult validation = Validate();

                if (validation.ResultCode == ResultCode.Normal)
                {

                    C2CTransferRequest request = new C2CTransferRequest();
                    request.Amount = this.Amount;
                    request.Currency = GetC2CTransferCurrencyCode(this.Currency);



                    this.Status = 1;
                    C2CTransferOrderDB.SaveC2CTransferOrder(this, result);
                    this.TransferID = result.TransferID;

                    C2CTransferResponse response = new C2CTransferResponse();


                    CardIdentification destinationCard = new CardIdentification();
                    destinationCard.CardNumber = this.DestinationCardNumber;

                    request.DestinationCardIdentification = destinationCard;
                    //request.ExtensionData //????
                    request.ExtensionID = this.TransferID;

                    CardIdentification sourceCard = new CardIdentification();
                    sourceCard.CardNumber = this.SourceCard.CardNumber;
                    if (this.SourceID != 0)
                    {
                        sourceCard.ExpiryDate = this.SourceCard.ExpiryDate.Substring(2, 4) + this.SourceCard.ExpiryDate.Substring(0, 2);
                    }
                    request.SourceCardIdentification = sourceCard;

                    this.Status = 5;

                    response = ArcaDataService.C2CTransfer(request);
                    this.Status = 7;

                    result.ResponseCode = response.ResponseCode;
                    result.ResponseCodeDescription = response.ResponseCodeDescription;
                    result.ResultCode = (byte)response.ResultCode;
                    result.ResultCodeDescription = response.ResultCodeDescription;
                    result.ProcessingCode = response.ProcessingCode;
                    result.SystemTraceAuditNumber = response.SystemTraceAuditNumber;
                    result.LocalTransactionDate = response.LocalTransactionDate;
                    result.rrn = response.rrn;
                    result.AuthorizationIdResponse = response.AuthorizationIdResponse;

                    if (result.ResponseCode == "00")
                    {
                        this.Status = 10;
                    }
                    else
                    {
                        this.Status = 20;
                    }

                    C2CTransferOrderDB.SaveC2CTransferOrder(this, result);
                    this.TransferID = result.TransferID;
                }
                else
                {
                    result.ResultCode = 1;
                    result.ResponseCode = "9999";
                    result.ResponseCodeDescription = "Ծառայությունը հասանելի չէ/Service not available";
                    Localization.SetCulture(validation, new Culture(Languages.hy));
                    validation.Errors.ForEach(m => result.ResultCodeDescription += m.Description + "; ");
                    this.Status = 30;
                }



            }
            catch (Exception ex)
            {
                result.ResultCode = 1;
                result.ResponseCode = "9999";
                result.ResponseCodeDescription = "Ծառայությունը հասանելի չէ/Service not available";
                this.Status = 30;
                C2CTransferException c2cex = new C2CTransferException(ex.Message);
                c2cex.TransferResponse = result;
                throw (c2cex);

            }

            return result;
        }

        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            //Սխալ գումար
            if (this.Amount <= 0)
            {
                ActionError err = new ActionError(22);
                result.Errors.Add(err);
            }

            if (C2CTransferOrderDB.CheckOrderID(this))
            {
                //Կրկնակի Order_ID քարտից քարդ փոխանցման ժամանակ
                ActionError err = new ActionError(1387);
                result.Errors.Add(err);
            }

            Card card = new Card();
            card.CardNumber = this.SourceCard.CardNumber;


            if (this.SourceID != 0)
            {
                KeyValuePair<String, double> balance = card.GetArCaBalance(88);
                if ((decimal)balance.Value < this.Amount)
                {
                    //Ելքագրվող քարտի անբավարար միջոցներ
                    ActionError err = new ActionError(1470);
                    result.Errors.Add(err);
                }
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        public int GetC2CTransferCurrencyCode(string currency)
        {
            int code = 0;
            string currencyCodeN = Utility.GetCurrencyCode(currency);
            if (!String.IsNullOrEmpty(currencyCodeN))
            {
                code = Convert.ToInt32(currencyCodeN);
            }

            return code;
        }

    }

    public class CardIdentificationData
    {
        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; }


        public void Init(ulong customerNumber)
        {
            CardIdentificationData cardIdentificationData = C2CTransferOrderDB.GetCTCardIdentificationData(customerNumber);
            this.CardNumber = cardIdentificationData.CardNumber;
            this.ExpiryDate = cardIdentificationData.ExpiryDate;
        }
    }

}