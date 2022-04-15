using ExternalBanking.ARUSDataService;

namespace ExternalBanking
{
    public class STAKResponseConfirm
    {
        /// <summary>
        /// Փոխանցման գործարքի ունիկալ համար՝ գեներացված Agent-ի կողմից
        /// </summary>
        public string TransactionCode { get; set; }


        public ActionResult ResponseConfirm(string authorizedUserSessionToken, string userName, string clientIP)
        {
            string transactionCode = "";

            ResponseConfirmInput responseConfirmInput = new ResponseConfirmInput();
            responseConfirmInput.TransactionCode = this.TransactionCode;

            ActionResult response = new ActionResult();

            ARUSDataService.ActionResult arusResult = new ARUSDataService.ActionResult();

            //ARUS հարցում
            ARUSHelper.Use(client =>
            {
                arusResult = client.ResponseConfirm(responseConfirmInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), transactionCode);
            }, authorizedUserSessionToken, userName, clientIP);


            //ARUS համակարգի հարցումն ունեցել է հաջող ավարտ
            if (arusResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                response.ResultCode = ResultCode.Normal;
            }
            else
            {
                response.ResultCode = ResultCode.Failed;
            }

            return response;
        }
    }

    //public class STAKResponseConfirmRequestResponse
    //{
    //    /// <summary>
    //    /// Գործողության արդյունք
    //    /// </summary>
    //    public ActionResult ActionResult { get; set; }

    //    public STAKResponseConfirmRequestResponse()
    //    {
    //        ActionResult = new ActionResult();
    //    }

    //}

}
