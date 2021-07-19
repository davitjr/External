using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ARUSDataService;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class RemittanceCancellationOrder : Order
    {
        /// <summary>
        /// Ուղարկված/Ստացված փոխանցում
        /// </summary>
        public Transfer Transfer { get; set; }

        /// <summary>
        /// Փոխանցման հսկիչ համար
        /// </summary>
        public string URN { get; set; }

        /// <summary>
        /// Չեղարկման/վերադարձման գործողության կոդ
        /// </summary>
        public string CancellationReversalCode { get; set; }

        /// <summary>
        /// Գործողության տեսակի կոդ (10: Send Money, 20: Payout)
        /// </summary>
        public string SendPayoutDivCode { get; set; }

        /// <summary>
        /// Չեղարկման/վերադարձման գործողության անվանում
        /// </summary>
        public string CancellationReversalCodeName { get; set; }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք փոխանցման չեղարկումը հաջողությամբ կատարվել է ARUS համակարգի կողմից, թե ոչ (1` բարեհաջող կատարում, 0՝ անհաջող ավարտ)
        /// </summary>
        public short ARUSSuccess { get; set; }

        /// <summary>
        /// ARUS համակարգի կողմից ստացված սխալի հաղորդագրություն
        /// </summary>
        public string ARUSErrorMessage { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար՝ փոխանցման արժույթով
        /// </summary>
        public double RemittanceFee { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար՝ ՀՀ դրամով
        /// </summary>
        public double AMDFee { get; set; }

        /// <summary>
        /// Փոխանցման՝ ուղարկող գործակալի միջնորդավճար՝ ՀՀ դրամով
        /// </summary>
        public double SendingFeeAMD { get; set; }

        /// <summary>
        /// Փոխանցման գումար փոխանցման արժույթով
        /// </summary>
        public double PrincipalAmount { get; set; }


        /// <summary>
        ///Ստացողի ազգանուն
        /// </summary>
        public string NATBeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն
        /// </summary>
        public string NATBeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն
        /// </summary>
        public string NATBeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի ազգանուն անգլերենով
        /// </summary>
        public string BeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն անգլերենով
        /// </summary>
        public string BeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն անգլերենով
        /// </summary>
        public string BeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի հասցե
        /// </summary>
        public string BeneficiaryAddress { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն անգլերենով
        /// </summary>
        public string SenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն անգլերենով
        /// </summary>
        public string SenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն անգլերենով
        /// </summary>
        public string SenderFirstName { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով 
        /// </summary>
        public string NATSenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderFirstName { get; set; }



        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string authorizedUserSessionToken, string clientIP)
        {

            this.Complete(userName, authorizedUserSessionToken, clientIP);

            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = RemittanceCancellationOrderDB.Save(this, userName);
                ulong orderId = (ulong)result.Id;

                Order.SaveLinkHBDocumentOrder(this.Id, orderId);


                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }


                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.TransactionLimitApprovement;
                    base.SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
              
            }

            return result;
        }


        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկման հայտի ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = this.ValidateForApprove();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            CancellationRequestResponse ARUSRequestResponse = new CancellationRequestResponse();

            CancellationInput cancellationInput = new CancellationInput();
            if (this.ARUSSuccess != 1)
            {
                cancellationInput.URN = this.URN;
                cancellationInput.CancellationReversalCode = this.CancellationReversalCode;
                cancellationInput.SendPayoutDivCode = this.SendPayoutDivCode;


                ARUSHelper.Use(client =>
                {
                    ARUSRequestResponse = client.CancellationOperation(cancellationInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), this.Id.ToString());
                }, authorizedUserSessionToken, userName, "clientIP");

            }
            else
            {
                ARUSRequestResponse.ActionResult = new ARUSDataService.ActionResult();
                ARUSRequestResponse.ActionResult.ResultCode = ARUSDataService.ResultCode.Normal;
                ARUSRequestResponse.CancellationOutput = new CancellationOutput();
            }


            if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                RemittanceCancellationOrderDB.UpdateARUSMessage(this.Id, result.Errors[0].Description);
                result.ResultCode = ResultCode.SavedNotConfirmed;
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
            }
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
            }
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                RemittanceCancellationOrderDB.UpdateARUSSuccess(this.Id, 1, ARUSRequestResponse.CancellationOutput.RemittanceFee, ARUSRequestResponse.CancellationOutput.AMDFee, ARUSRequestResponse.CancellationOutput.SendingFee, ARUSRequestResponse.CancellationOutput.PrincipalAmount);
                result = base.Confirm(user);

                string infoDescription = Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.CancellationOutput.StatusCodeName +
                                         Environment.NewLine + "Փոխանցման միջնորդավճար՝ " + ARUSRequestResponse.CancellationOutput.RemittanceFee +
                                         Environment.NewLine + "Փոխանցման միջնորդավճար AMD՝ " + ARUSRequestResponse.CancellationOutput.AMDFee +
                                         Environment.NewLine + "Ուղարկող գործակալի միջնորդավճար՝ " + ARUSRequestResponse.CancellationOutput.SendingFee;

                if (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.Failed)
                {
                    ActionError information = new ActionError();
                    information.Code = 0;
                    information.Description = infoDescription;

                    result.Errors.Add(information);
                }
                else   //SaveNotConfirmed
                {
                    result.Errors[0].Description += infoDescription;
                }
            }

            return result;
        }


        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string authorizedUserSessionToken, string clientIP)
        {

            this.Complete(userName, authorizedUserSessionToken, clientIP);

            //Մուտքային դաշտերի ստուգումներ
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;         

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = RemittanceCancellationOrderDB.Save(this, userName);
                ulong orderId = (ulong)result.Id;

                Order.SaveLinkHBDocumentOrder(this.Id, orderId);


                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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
                else
                {
                    return result;
                }
            }


            CancellationRequestResponse ARUSRequestResponse = new CancellationRequestResponse();

            CancellationInput cancellationInput = new CancellationInput();
            cancellationInput.URN = this.URN;
            cancellationInput.CancellationReversalCode = this.CancellationReversalCode;
            cancellationInput.SendPayoutDivCode = this.SendPayoutDivCode;

            //ARUS հարցում
            ARUSHelper.Use(client =>
            {
                ARUSRequestResponse = client.CancellationOperation(cancellationInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), this.Id.ToString());
            }, authorizedUserSessionToken, userName, "clientIP");

            //ARUS համակարգից ստացվել է սխալի հաղորդագրություն
            if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                RemittanceCancellationOrderDB.UpdateARUSMessage(this.Id, result.Errors[0].Description);
                result.ResultCode = ResultCode.SavedNotConfirmed;
            }
            //ARUS համակարգին դիմելու պահին տեղի է ունեցել սխալ
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
            }
            //ARUS համակարգի հարցումն ունեցել է հաջող ավարտ
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                RemittanceCancellationOrderDB.UpdateARUSSuccess(this.Id, 1, ARUSRequestResponse.CancellationOutput.RemittanceFee, ARUSRequestResponse.CancellationOutput.AMDFee, ARUSRequestResponse.CancellationOutput.SendingFee, ARUSRequestResponse.CancellationOutput.PrincipalAmount);
                result = base.Confirm(user);

                string infoDescription = Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.CancellationOutput.StatusCodeName +
                                         Environment.NewLine + "Փոխանցման միջնորդավճար՝ " + ARUSRequestResponse.CancellationOutput.RemittanceFee + 
                                         Environment.NewLine + "Փոխանցման միջնորդավճար AMD՝ " + ARUSRequestResponse.CancellationOutput.AMDFee +
                                         Environment.NewLine + "Ուղարկող գործակալի միջնորդավճար՝ " + ARUSRequestResponse.CancellationOutput.SendingFee;

                if (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.Failed)
                {
                    ActionError information = new ActionError();
                    information.Code = 0;
                    information.Description = infoDescription;

                    result.Errors.Add(information);
                }
                else   //SaveNotConfirmed
                {
                    result.Errors[0].Description += infoDescription;
                }
            }

            return result;
        }

        /// <summary>
        /// Լրացնում է չեղարկման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete(string userName, string authorizedUserSessionToken, string clientIP)
        {
            this.RegistrationDate = DateTime.Now.Date;
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            DataTable divCodes = ARUSInfo.GetSendPayoutDivisionCodes(authorizedUserSessionToken, userName, clientIP);

            //Ուղարկված փոխանցման չեղարկում
            if (this.SubType == 1)
            {
                this.SendPayoutDivCode = divCodes.Select("name like '%Send%'")[0]["code"].ToString();
            }
            //Վճարված փոխանցման չեղարկում
            else if(this.SubType == 2)
            {
                this.SendPayoutDivCode = divCodes.Select("name like '%Pay%'")[0]["code"].ToString();
            }
        }

        /// <summary>
        /// Չեղարկման չեղարկման հայտի դաշտերի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            CancellationInput cancellationInput = new CancellationInput();
            cancellationInput.URN = this.URN;
            cancellationInput.CancellationReversalCode = this.CancellationReversalCode;
            cancellationInput.SendPayoutDivCode = this.SendPayoutDivCode;

            if (ExistsNotConfirmedRemittanceCancellationOrder())
            {
                //Տվյալ փոխանցման համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված չեղարկման հայտ:
                result.Errors.Add(new ActionError(1507));
            }

            result.Errors.AddRange(Validation.ValidateRemittanceCancellationOrder(cancellationInput));

            return result;
        }

        public bool ExistsNotConfirmedRemittanceCancellationOrder()
        {
            return RemittanceCancellationOrderDB.ExistsNotConfirmedRemittanceCancellationOrder(this.CustomerNumber, this.SubType, this.Transfer.Id);
        }

        /// <summary>
        /// Չեղարկման հայտի կատարման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForApprove()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if(this.Quality != OrderQuality.TransactionLimitApprovement)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել։
                result.Errors.Add(new ActionError(35));
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման հայտի մանրամասները
        /// </summary>
        public void Get(string authorizedUserSessionToken, string userName, string clientIP)
        {
            RemittanceCancellationOrderDB.GetRemittanceCancellationOrder(this);
            this.Transfer.Get();

            if (!String.IsNullOrEmpty(CancellationReversalCode))
            {
                DataTable reversalCodes = ARUSInfo.GetCancellationReversalCodes(Transfer.MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                CancellationReversalCodeName = reversalCodes.Select("code = '" + CancellationReversalCode + "'")[0]["name"].ToString();
            }
        }
    }
}
