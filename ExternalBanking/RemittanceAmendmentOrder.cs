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
    public class RemittanceAmendmentOrder : Order
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
        /// Փոփոխման գործողության պատճառի կոդ
        /// </summary>
        public string AmendmentReasonCode { get; set; }

        /// <summary>
        /// Փոփոխման գործողության պատճառի անվանում
        /// </summary>
        public string AmendmentReasonName { get; set; }    

        /// <summary>
        /// Ստացողի ազգանունը՝ նախքան փոփոխությունը
        /// </summary>
        public string BeforeBeneLastName { get; set; }

        /// <summary>
        /// Ստացողի հայրանունը՝ նախքան փոփոխությունը
        /// </summary>
        public string BeforeBeneMiddleName { get; set; }

        /// <summary>
        /// Ստացողի անունը՝ նախքան փոփոխությունը
        /// </summary>
        public string BeforeBeneFirstName { get; set; }

        /// <summary>
        /// Ստացողի ազգանունը՝ փոփոխությունից հետո
        /// </summary>
        public string BeneficiaryLastName { get; set; }

        /// <summary>
        /// Ստացողի հայրանունը՝ փոփոխությունից հետո
        /// </summary>
        public string BeneficiaryMiddleName { get; set; }

        /// <summary>
        /// Ստացողի անունը՝ փոփոխությունից հետո
        /// </summary>
        public string BeneficiaryFirstName { get; set; }

        /// <summary>
        ///Հայերեն՝ ստացողի անուն՝ նախքան փոփոխությունը  
        /// </summary>
        public string BeforeNATBeneficiaryFirstName { get; set; }
        /// <summary>
        ///Հայերեն՝ ստացողի ազգանուն՝ նախքան փոփոխությունը 
        /// </summary>
        public string BeforeNATBeneficiaryLastName { get; set; }
        /// <summary>
        ///Հայերեն՝ ստացողի հայրանուն՝ նախքան փոփոխությունը 
        /// </summary>
        public string BeforeNATBeneficiaryMiddleName { get; set; }
        /// <summary>
        ///Հայերեն՝ ստացողի անուն՝ փոփոխությունից հետո
        /// </summary>
        public string NATBeneficiaryFirstName { get; set; }
        /// <summary>
        ///Հայերեն՝ ստացողի ազգանուն՝ փոփոխությունից հետո 
        /// </summary>
        public string NATBeneficiaryLastName { get; set; }
        /// <summary>
        ///Հայերեն՝ ստացողի հայրանուն՝ փոփոխությունից հետո
        /// </summary>
        public string NATBeneficiaryMiddleName { get; set; }


        /// <summary>
        /// Ցույց է տալիս՝ արդյոք փոխանցման չեղարկումը հաջողությամբ կատարվել է ARUS համակարգի կողմից, թե ոչ (1` բարեհաջող կատարում, 0՝ անհաջող ավարտ)
        /// </summary>
        public short ARUSSuccess { get; set; }

        /// <summary>
        /// ARUS համակարգի կողմից ստացված սխալի հաղորդագրություն
        /// </summary>
        public string ARUSErrorMessage { get; set; }



        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման տվյալների փոփոխման հայտի պահպանում
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
                result = RemittanceAmendementOrderDB.Save(this, userName);
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
        /// Տվյալների փոփոխման հայտի դաշտերի ավտոմատ լրացում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        protected void Complete(string userName, string authorizedUserSessionToken, string clientIP)
        {
            this.RegistrationDate = DateTime.Now.Date;
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(!String.IsNullOrEmpty(AmendmentReasonCode))
            {
                DataTable reasons = ARUSInfo.GetAmendmentReasons(Transfer.MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                AmendmentReasonName = reasons.Select("code = '" + AmendmentReasonCode + "'")[0]["name"].ToString();
            }
           
        }

        /// <summary>
        /// Տվյալների փոփոխման հայտի դաշտերի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            AmendmentInput amendmentInput = new AmendmentInput();
            ARUSHelper.ConvertObject(this, ref amendmentInput);

            if (ExistsNotConfirmedAmendmentOrder())
            {
                //Տվյալ փոխանցման համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված տվյալների փոփոխման հայտ:
                result.Errors.Add(new ActionError(1509));
            }

            result.Errors.AddRange(Validation.ValidateRemittanceAmendmentOrder(amendmentInput));

            return result;
        }

        /// <summary>
        /// Ստուգում է՝ արդյոք տվյալ փոխանցման համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված տվյալների փոփոխման հայտ, թե ոչ։
        /// </summary>
        /// <returns></returns>
        public bool ExistsNotConfirmedAmendmentOrder()
        {
            return RemittanceAmendementOrderDB.ExistsNotConfirmedAmendmentOrder(this.CustomerNumber, this.SubType, this.Transfer.Id);
        }

        /// <summary>
        /// Չեղարկման հայտի կատարման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForApprove()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            

            return result;
        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման տվյալների փոփոխման հայտի ուղարկում
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

            AmendmentRequestResponse ARUSRequestResponse = new AmendmentRequestResponse();

            AmendmentInput amendmentInput = new AmendmentInput();

            if (this.ARUSSuccess != 1)
            {
                ARUSHelper.ConvertObject(this, ref amendmentInput);


                ARUSHelper.Use(client =>
                {
                    ARUSRequestResponse = client.AmendmentOperation(amendmentInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), this.Id.ToString());
                }, authorizedUserSessionToken, userName, "clientIP");

            }
            else
            {
                ARUSRequestResponse.ActionResult = new ARUSDataService.ActionResult();
                ARUSRequestResponse.ActionResult.ResultCode = ARUSDataService.ResultCode.Normal;
                ARUSRequestResponse.AmendmentOutput = new AmendmentOutput();
            }




            if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                RemittanceAmendementOrderDB.UpdateARUSMessage(this.Id, result.Errors[0].Description);
                result.ResultCode = ResultCode.SavedNotConfirmed;
            }
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
            }
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                RemittanceAmendementOrderDB.UpdateARUSSuccess(this.Id, 1);
                result = base.Confirm(user);

                string infoDescription = Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.AmendmentOutput.StatusCodeName +
                                         Environment.NewLine + "Ստացողի անուն՝ " + ARUSRequestResponse.AmendmentOutput.BeneficiaryFirstName +
                                         Environment.NewLine + "Ստացողի ազգանուն՝ " + ARUSRequestResponse.AmendmentOutput.BeneficiaryLastName +
                                         Environment.NewLine + "Ստացողի հայրանուն՝ " + ARUSRequestResponse.AmendmentOutput.BeneficiaryMiddleName;


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
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման տվյալների փոփոխման հայտի մանրամասները
        /// </summary>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        public void Get()
        {
            RemittanceAmendementOrderDB.GetRemittanceAmendmentOrder(this);
            this.Transfer.Get();
        }

        public static RemittanceAmendmentOrder GetNotConfirmedOrderByTransfer(ulong transferId)
        {
            long docId = RemittanceAmendementOrderDB.GetNotConfirmedOrderIdByTransfer(transferId);
            RemittanceAmendmentOrder order = new RemittanceAmendmentOrder();
            order.Id = docId;
            order.Get();
            return order;
        }


        /// <summary>
        /// Վերադարձնում է փոխանցման կարգավիճակի փոփոխությունների քանակը
        /// </summary>
        public static int GetRemittanceAmendmentCount(ulong TransferId) => RemittanceAmendementOrderDB.GetRemittanceAmendmentCount(TransferId);
    }
}
