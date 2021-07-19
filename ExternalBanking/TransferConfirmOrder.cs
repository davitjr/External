using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.ServiceClient;
using ExternalBanking.ARUSDataService;
using System.Data;

namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class TransferConfirmOrder : Order
    {

        /// <summary>
        /// Ձևակերպվող փոխանցում
        /// </summary>
        public Transfer Transfer { get; set; }

        public new ActionResult Confirm(string filialCode, short allowTransferConfirm, DateTime setDate, short setNumber, string userName, SourceType source, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            this.Complete();
           
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            List<short> errors = new List<short>();
            errors = TransferDB.CheckForConfirm(this.Transfer.Id, filialCode, allowTransferConfirm, setDate, user.userID );
    
            if (errors.Count != 0)
            {
                //for (int i = 0; i < errors.Count; i++)
                //{
                result.Errors.Add(new ActionError(errors[0]));
                //}
            }

            if (ValidationDB.CheckOpDayClosingStatus(Convert.ToInt32(filialCode)))
            {
                // Հնարավոր չէ կատարել գործարք:Գործառնական օրվա կարգավիճակը փակ է:
                result.Errors.Add(new ActionError(766));
            }
            if (result.Errors.Count != 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (this.Transfer.PoliceResponseDetailsID != 0)
                result = PolicePayment();

            if (result.ResultCode == ResultCode.ValidationError)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferDB.SaveConfirmOrder(this, userName, source, allowTransferConfirm);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                //result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            if (result.ResultCode == ResultCode.Normal)
            {
                if (this.Transfer.TransferSystem != 23)
                {
                    result = base.Confirm(user);
                }
                else    //ARUS
                {
                    FastTransferPaymentOrder fastTransferOrder = new FastTransferPaymentOrder();
                    fastTransferOrder.Id = (long)this.Transfer.AddTableUnicNumber;
                    fastTransferOrder.Get(userName, authorizedUserSessionToken, clientIP);


                    SendMoneyRequestResponse ARUSRequestResponse = new SendMoneyRequestResponse();

                    if (fastTransferOrder.ARUSSuccess != 1)
                    {

                        SendMoneyInput sendMoneyInput = new SendMoneyInput();
                        ARUSHelper.ConvertObject(fastTransferOrder, ref sendMoneyInput);


                        if (!String.IsNullOrEmpty(fastTransferOrder.Country))
                        {

                            DataTable dt = Info.GetCountries();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["CountryCodeN"].ToString() == fastTransferOrder.Country)
                                {
                                    sendMoneyInput.BeneficiaryCountryCode = dt.Rows[i]["CountryCodeA3"].ToString();
                                    break;
                                }
                            }
                        }

                        sendMoneyInput.SendAmount = (decimal)fastTransferOrder.Amount;
                        sendMoneyInput.SendCurrencyCode = fastTransferOrder.Currency != "RUR" ? fastTransferOrder.Currency : "RUB";
                        sendMoneyInput.SenderAddressName = fastTransferOrder.SenderAddress;
                        sendMoneyInput.SenderBirthDate = String.Format("{0:yyyyMMdd}", fastTransferOrder.SenderDateOfBirth);
                        sendMoneyInput.SenderEMailName = fastTransferOrder.SenderEmail;
                        sendMoneyInput.SenderPhoneNo = fastTransferOrder.SenderPhone;



                        ARUSHelper.Use(client =>
                        {
                            ARUSRequestResponse = client.SendMoneyOperation(sendMoneyInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), fastTransferOrder.Id.ToString());
                        }, authorizedUserSessionToken, userName, "clientIP");

                    }
                    else
                    {
                        ARUSRequestResponse.ActionResult = new ARUSDataService.ActionResult();
                        ARUSRequestResponse.ActionResult.ResultCode = ARUSDataService.ResultCode.Normal;
                        ARUSRequestResponse.SendMoneyOutput = new SendMoneyOutput();
                        ARUSRequestResponse.SendMoneyOutput.URN = fastTransferOrder.Code;
                    }


                    //ARUS համակարգից ստացվել է սխալի հաղորդագրություն
                    if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
                    {
                        result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                        FastTransferPaymentOrderDB.UpdateARUSMessage(fastTransferOrder.Id, result.Errors[0].Description);
                        result.ResultCode = ResultCode.SavedNotConfirmed;
                    }
                    else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
                    {
                        result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                    }
                    else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
                    {
                        FastTransferPaymentOrderDB.UpdateARUSSuccess(fastTransferOrder.Id, 1, ARUSRequestResponse.SendMoneyOutput.URN);

                        TransferDB.UpdateTransferAfterARUSRequest(this.Transfer.Id, ARUSRequestResponse.SendMoneyOutput.URN);

                        result = base.Confirm(user);

                        string infoDescription = Environment.NewLine + "Փոխանցման հսկիչ համար՝ " + ARUSRequestResponse.SendMoneyOutput.URN +
                                                 Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.SendMoneyOutput.StatusCodeName;

                        if (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.Failed)
                        {
                            ActionError information = new ActionError();
                            information.Code = 0;
                            information.Description = infoDescription;

                            if(result.ResultCode == ResultCode.Normal)
                            {
                                result.ResultCode = ResultCode.NoneAutoConfirm;
                            }

                            result.Errors.Add(information);
                        }
                        else   //SaveNotConfirmed
                        {
                            result.Errors[0].Description += infoDescription;
                        }
                    }


                }
            }

            return result;
        }

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.Transfer.Get();
            this.Type = OrderType.TransferConfirmOrder;
            this.SubType = 1;
            this.RegistrationDate = Convert.ToDateTime(this.OperationDate);
            this.OrderNumber = "1";
            this.Quality = OrderQuality.Draft;

        }


       public new ActionResult PolicePayment()
        {
           ActionResult result = new ActionResult();

            result.ResultCode = ResultCode.Normal ;
            if (!TransferDB.CheckPolicePayment(this.Transfer.PoliceResponseDetailsID))
            {
                CBViolationPayment policePayment = new CBViolationPayment();
                policePayment = TransferDB.GetPolicePayment(this.Transfer.PoliceResponseDetailsID);
                policePayment.PayedSum = Convert.ToDecimal (this.Transfer.Amount);
                ViolationRequestResponse  paymentResult=new ViolationRequestResponse();

                paymentResult = ACBAOperationService.RegisterPayment(policePayment, user);

                if (paymentResult.resultCode != 0)
                {
                    result.Errors.Add(new ActionError(955, new string[] { paymentResult.resultDescription}));
                    result.ResultCode = ResultCode.ValidationError;
                }
                else
                    result.ResultCode = ResultCode.Normal ;
            }
            return result;

        }
    }

}
