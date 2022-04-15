using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class CreditCommitmentForgivenessOrder : Order
    {
        /// <summary>
        /// ունիկալ համար
        /// </summary>
        public ulong AppId { get; set; }

        /// <summary>
        /// Վարկի տեսակ
        /// </summary>
        public int LoanType { get; set; }

        /// <summary>
        /// Տուժանք      
        /// </summary>
        public double PenaltyRate { get; set; }

        /// <summary>
        /// Վարկային մայր գումար
        /// </summary>
        public double CurrentCapital { get; set; }

        /// <summary>
        ///  Տոկոս
        /// </summary>
        public double CurrentRateValue { get; set; }

        /// <summary>
        /// Պետական տուրքի փոխանցումից հետո կուտակված տուժանք
        /// </summary>
        public double JudgmentPenaltyRate { get; set; }

        /// <summary>
        /// Սպասարկման վճար
        /// </summary>
        public double CurrentFee { get; set; }

        /// <summary>
        /// հիմքի ամսաթիվ
        /// </summary>
        public DateTime? DateOfFoundation { get; set; }

        /// <summary>
        /// հիմքի համար
        /// </summary>
        public string NumberOfFoundation { get; set; }

        /// <summary>
        /// Quality = 11 || 12 Current capital-ի փոխարեն outcapital
        /// </summary>
        public double? OutCapital { get; set; }

        /// <summary>
        /// Մահվան ամսաթիվ
        /// </summary>
        public DateTime? DateOfDeath { get; set; }

        /// <summary>
        /// Մահվան ամսաթիվ
        /// </summary>
        public string NumberOfDeath { get; set; }

        /// <summary>
        /// Loan_type
        /// </summary>
        public int LoanQuality { get; set; }

        /// <summary>
        /// Ներման տեսակներ
        /// </summary>
        public string RebateType { get; set; }

        /// <summary>
        ///  loan_type = 18 ավելացնել current_rate_value_nused չօգտագործված մասի դիմաց կուտակված տոկոս դաշտը
        /// </summary>
        public double? CurrentRateValueNused { get; set; }

        /// <summary>
        ///  Ներման հարկ
        /// </summary>
        public double Tax { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Վարկային գիծ է , թե վարկ
        /// </summary>
        public bool IsCreditLine { get; set; }

        /// <summary>
        /// Դուրսգրման ամսաթիվ
        /// </summary>
        public DateTime? OutLoanDate { get; set; }

        /// <summary>
        /// Վարկի մասնաճուղի համար
        /// </summary>
        public short LoanFilialCode { get; set; }


        public ActionResult SaveForgivableLoanCommitment(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                if (creditCommitmentForgiveness.CurrentCapital != 0)
                {
                    creditCommitmentForgiveness.Tax = CreditCommitmentForgivenessOrderDB.GetTax(creditCommitmentForgiveness.CustomerNumber, creditCommitmentForgiveness.CurrentCapital, creditCommitmentForgiveness.RebateType, creditCommitmentForgiveness.Currency);
                }
                else
                {
                    creditCommitmentForgiveness.Tax = CreditCommitmentForgivenessOrderDB.GetTax(creditCommitmentForgiveness.CustomerNumber, creditCommitmentForgiveness.OutCapital, creditCommitmentForgiveness.RebateType, creditCommitmentForgiveness.Currency);
                }

                result = CreditCommitmentForgivenessOrderDB.SaveForgivableLoanCommitment(creditCommitmentForgiveness, userName);

                result = base.SaveOrderOPPerson();
                this.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                //creditCommitmentForgiveness.user = user;
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

            ActionResult resultConfirm = base.Confirm(user);
            return resultConfirm;
        }

        public CreditCommitmentForgivenessOrder Get()
        {
            return CreditCommitmentForgivenessOrderDB.GetForgivableLoanCommitmentDetails(this);
        }

        /// <summary>
        /// Վերադարձնում է ներման ենթակա վարկային պարտավորությունների դաշտերը
        /// </summary>
        /// <param name="CustomerNumber"></param>
        /// <param name="creditCommitmentForgiveness"></param>
        /// <returns></returns>
        public static CreditCommitmentForgivenessOrder GetForgivableLoanCommitment(ulong CustomerNumber, CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            return CreditCommitmentForgivenessOrderDB.GetForgivableLoanCommitment(CustomerNumber, creditCommitmentForgiveness);
        }

        /// <summary>
        /// Գումարի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCreditCommitmentForgivenes(this));
            return result;


        }



        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        public static List<ActionError> ValidateCreditCommitmentForgivenes(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            return CreditCommitmentForgivenessOrderDB.ValidateCreditCommitmentForgivenes(creditCommitmentForgiveness);
        }

        public static double GetTax(ulong customerNumber, double? capital, string RebetType, string currency)
        {
            return CreditCommitmentForgivenessOrderDB.GetTax(customerNumber, capital, RebetType, currency);

        }


    }
}

