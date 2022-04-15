using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class EOTransferRequest
    {
        /// <summary>
        /// Հարցման համար
        /// </summary>
        public int ParentID { get; set; }

        /// <summary>
        /// Փոխանցման գումար
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Բանկային քարտի քարտային հաշվի համար
        /// </summary>
        public long Account { get; set; }

        /// <summary>
        /// Համագործակցող ընկերություն
        /// </summary>
        public CashTerminal Partner { get; set; }

        public EOTransferResponse MakeTransfer()
        {
            EOTransferResponse response = new EOTransferResponse();
            response.TransferID = 0;
            C2CTransferOrder order = new C2CTransferOrder();
            C2CTransferResult result = new C2CTransferResult();

            EOServiceDB.SaveEOMakeTransferRequest(this);

            response.ParentID = this.ParentID;
            response.Amount = Amount;//????

            CardIdentificationData sourceCard = new CardIdentificationData();

            Card destinationCard = new Card();
            Account cardAccount = new Account(this.Account.ToString());
            destinationCard = Card.GetCardWithOutBallance(cardAccount.AccountNumber);



            if (destinationCard.ClosingDate != null)
            {
                response.ErrorCode = 1;
                response.ErrorText = "Closed card";
            }
            else if (destinationCard.Currency != "AMD")
            {
                response.ErrorCode = 1;
                response.ErrorText = "Foreign currency card";
            }
            else
            {
                Account acc = new Account();
                acc.AccountNumber = this.Account.ToString();
                ulong customerNumber = acc.GetAccountCustomerNumber();

                if (Validation.IsDAHKAvailability(customerNumber))
                {
                    response.ErrorCode = 1;
                    response.ErrorText = "Found in stop list";
                }
            }

            if (response.ErrorCode != 1)
            {


                order.OrderID = this.ParentID;
                order.Amount = this.Amount;
                order.Currency = "AMD";


                order.DestinationCardNumber = destinationCard.CardNumber;
                order.Receiver = "";
                sourceCard.Init(Partner.CustomerNumber);
                order.SourceCard = sourceCard;
                order.SourceID = CashTerminal.GetTerminalID(Partner.CustomerNumber);
                order.Status = 0;
                order.TransferID = 0;


                result = order.Save();

                if (result.ResultCode != 0)
                {
                    response.ErrorCode = 1;
                    response.ErrorText = result.ResultCodeDescription;
                }
                else
                {
                    response.ErrorCode = 0;
                    response.ErrorText = "";
                    response.TransferID = result.TransferID;

                }

            }



            EOServiceDB.SaveEOMakeTransferResponse(response);

            return response;
        }
    }
}
