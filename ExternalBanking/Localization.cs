using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ExternalBanking
{
    public static class Localization
    {
        /// <summary>
        /// Հաշիվներ
        /// </summary>
        public static void SetCulture(Account account, Culture culture)
        {
            if (account != null && account?.IsAttachedCard != true)
            {

                if (account.AccountType != 18)
                {
                    account.AccountTypeDescription = Info.AccountTypeDescription(account.AccountType, culture.Language);
                }
                else 
                {
                    account.AccountTypeDescription = culture.Language == Languages.hy ? "Սահմանափակ հաշիվ" : "Limited account";
                }

                account.JointTypeDescription = Info.GetJointTypeDescription(account.JointType, culture.Language);
            }
        }

        public static void SetCulture(List<Account> accounts, Culture culture)
        {
            accounts.ForEach(m => SetCulture(m, culture));
        }



        /// <summary>
        /// Ավանդներ
        /// </summary>
        public static void SetCulture(Deposit deposit, Culture culture)
        {
            if (deposit != null)
            {
                deposit.DepositTypeDescription = Info.GetDepositTypeDescription(deposit.DepositType, culture.Language);
                deposit.JointTypeDescription = Info.GetJointTypeDescription(deposit.JointType, culture.Language);
                deposit.StatementDeliveryTypeDescription = Info.GetStatementDeliveryTypeDescription(deposit.StatementDeliveryType, culture.Language);

                if (deposit.ClosingDate != null)
                    deposit.ClosingReasonTypeDescription = Info.GetDepositClosingReasonTypeDescription(deposit.ClosingReasonType, culture.Language);

                SetCulture(deposit.DepositAccount, culture);

            }
        }

        public static void SetCulture(List<Deposit> deposits, Culture culture)
        {
            deposits.ForEach(m => SetCulture(m, culture));

        }

        /// <summary>
        /// Քարտեր
        /// </summary>
        public static void SetCulture(Card card, Culture culture)
        {
            if (card != null)
            {
                SetCulture(card.CardAccount, culture);

                if (card.CreditLine != null)
                {
                    SetCulture(card.CreditLine, culture);
                }

                if (card.Overdraft != null)
                {
                    SetCulture(card.Overdraft, culture);
                }

                card.RelatedOfficeName = Info.GetCardRelatedOfficeName(card.RelatedOfficeNumber);

                if (card.ClosingReasonType != 0)
                {
                    card.ClosingReasonTypeDescription = Info.GetCardClosingReasonDescription((short)card.ClosingReasonType, culture.Language);
                }

                if (!String.IsNullOrEmpty(card.AddInf))
                {
                    card.AddInf = Utility.ConvertAnsiToUnicode(card.AddInf);
                }
            }

        }

        public static void SetCulture(List<Card> cards, Culture culture)
        {
            if (cards != null)
            {
                cards.ForEach(m => SetCulture(m, culture));
            }

        }

        /// <summary>
        /// Վարկեր
        /// </summary>
        public static void SetCulture(Loan loan, Culture culture)
        {
            loan.LoanTypeDescription = Info.GetLoanTypeDescription(loan.LoanType, culture.Language);
            if(!loan.Is_24_7)
            {
                loan.QualityDescription = Info.GetLoanQualityTypeDescription(loan.Quality, culture.Language);
            }
            else
            {
                if (culture.Language == Languages.hy)
                {
                    loan.QualityDescription = "Տրամադրված է 24/7 եղանակով";
                }
                else
                {
                    loan.QualityDescription = "Provided by 24/7 mode";
                }

            }
            
            loan.FondDescription = Info.GetFondDescription(loan.Fond);
            loan.LoanProgramDescription = Info.GetLoanProgramDescription(loan.LoanProgram);
            loan.SaleDescription = Info.GetLoanActionDescription(loan.Sale);

            SetCulture(loan.LoanAccount, culture);
            SetCulture(loan.ConnectAccount, culture);
        }

        public static void SetCulture(List<Loan> loans, Culture culture)
        {
            loans.ForEach(m => SetCulture(m, culture));
        }


        /// <summary>
        /// Պարբերական փոխանցումներ
        /// </summary>
        public static void SetCulture(PeriodicTransfer periodicTransfer, Culture culture)
        {
            periodicTransfer.TypeDescription = Info.GetPeriodicTransferTypeDescription((short)periodicTransfer.Type, culture.Language);
            periodicTransfer.DurationTypeDescription = Info.GetPeriodicTransferDurationTypeDescription((short)periodicTransfer.DurationType, culture.Language);
            periodicTransfer.ChargeTypeDescription = Info.GetPeriodicTransferChargeTypeDescription((short)periodicTransfer.ChargeType, culture.Language);
            if (periodicTransfer.DebitAccount != null)
                SetCulture(periodicTransfer.DebitAccount, culture);
        }

        public static void SetCulture(List<PeriodicTransfer> periodicTransfers, Culture culture)
        {
            periodicTransfers.ForEach(m => SetCulture(m, culture));
        }

        /// <summary>
        /// Վճարման հանձնարարականներ 
        /// </summary>
        public static void SetCulture(Order order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
 
        }

        public static void SetLoanMatureTypeDescription(PaymentOrder order, Culture culture)
        {

            order.MatureTypeDescription = Info.GetLoanMatureTypeDescription(order.MatureType, culture.Language);

        }
        public static void SetCulture(MatureOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            if (order.RepaymentSourceType != 0)
                order.RepaymentSourceTypeDescription = Info.GetTypeOfLoanRepaymentSourceDescription(order.RepaymentSourceType);

        }

        /// <summary>
        /// Տեղեկանքի ստացման հայտ 
        /// </summary>
        public static void SetCulture(ReferenceOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);

            if (order.Accounts != null && order.Accounts.Count > 0)
            {
                order.Accounts.ForEach(m =>
                {
                    SetCulture(m, culture);
                });
            }
        }

        /// <summary>
        /// Միջազգային վճարման հանձնարարականներ 
        /// </summary>
        public static void SetCulture(InternationalPaymentOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
        }

        public static void SetCulture(List<Order> orders, Culture culture)
        {
            orders.ForEach(m => SetCulture(m, culture));
        }

        /// <summary>
        /// Կոմունալ վճարման մանրամասն նկարագրություններ
        /// </summary>
        public static void SetCulture(CommunalDetails communalDetail, Culture culture)
        {
            communalDetail.Description = Info.GetCommunalDetailTypeDescription(communalDetail.Id, culture.Language);
        }

        public static void SetCulture(List<CommunalDetails> communalDetails, Culture culture)
        {
            communalDetails.ForEach(m => SetCulture(m, culture));
        }
        public static void SetCulture(ActionResult result, Culture culture)
        {
            if (result.Errors != null && result.Errors.Count > 0)
            {
                result.Errors.ForEach(m => SetCulture(m, culture));
            }
            result.Errors = result.Errors.GroupBy(i => i.Description).Select(g => g.First()).ToList();
        }

        public static void SetCulture(List<ActionError> result, Culture culture)
        {
            if (result != null && result.Count > 0)
            {
                result.ForEach(m => SetCulture(m, culture));
            }
            result = result.GroupBy(i => i.Description).Select(g => g.First()).ToList();
        }

        public static void SetCulture(ActionError actionError, Culture culture)
        {
            if (actionError.Code != 0)
                actionError.Description = Info.GetTerm(actionError.Code, actionError.Params, culture.Language);
            else
                actionError.Description = Utility.ConvertAnsiToUnicode(actionError.Description);

        }

        /// <summary>
        /// Վարկային գծերի նկարագրություններ
        /// </summary>
        public static void SetCulture(CreditLine creditLine, Culture culture)
        {
            creditLine.TypeDescription = Info.GetCreditLineTypeDescription(creditLine.Type, culture.Language);
            creditLine.QualityDescription = Info.GetLoanQualityTypeDescription(creditLine.Quality, culture.Language);
        }

        public static void SetCulture(List<CreditLine> creditLines, Culture culture)
        {
            creditLines.ForEach(m => SetCulture(m, culture));
        }

        /// Հաշվապահական գործարքի (Քաղվածքի) նկարագրությունը
        /// </summary>
        public static void SetCulture(AccountStatementDetail accountStatementDetail, Culture culture)
        {
            if (culture.Language == Languages.hy)
            {
                accountStatementDetail.Description = Utility.ConvertAnsiToUnicode(accountStatementDetail.Description);

                if (accountStatementDetail.Correspondent != null && accountStatementDetail.Correspondent != "")
                {
                    if (accountStatementDetail.DebitCredit == 'd')
                    {
                        accountStatementDetail.Correspondent = "ստացող " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                    }
                    else if (accountStatementDetail.DebitCredit == 'c')
                    {
                        if (accountStatementDetail.OperationType == 802)
                        {
                            accountStatementDetail.Correspondent = "մուծող " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                        }
                        else
                        {
                            accountStatementDetail.Correspondent = "փոխանցող " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                        }
                    }
                }

            }
            else
            {
                accountStatementDetail.Description = Info.GetOperationTypeDescription(accountStatementDetail.OperationType, culture.Language);


                if (accountStatementDetail.Correspondent != null && accountStatementDetail.Correspondent != "")
                {
                    if (accountStatementDetail.DebitCredit == 'd')
                    {
                        accountStatementDetail.Correspondent = "recipient " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                    }
                    else if (accountStatementDetail.DebitCredit == 'c')
                    {
                        if (accountStatementDetail.OperationType == 802)
                        {
                            accountStatementDetail.Correspondent = "payer " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                        }
                        else
                        {
                            accountStatementDetail.Correspondent = "sender " + Utility.ConvertAnsiToUnicode(accountStatementDetail.Correspondent);
                        }
                    }
                }
            }
        }

        public static void SetCulture(List<AccountStatementDetail> accountStatementDetails, Culture culture)
        {
            accountStatementDetails.ForEach(m => SetCulture(m, culture));
        }
        public static void SetCulture(List<OverdueDetail> overduedetails, Culture culture)
        {
            overduedetails.ForEach(m => SetCulture(m, culture));
        }
        public static void SetCulture(OverdueDetail overdue, Culture culture)
        {
            overdue.ProductTypeDescription = Info.GetOverdueProductTypeDescription((ushort)overdue.ProductType, culture.Language);
            overdue.QualityDescription = Info.GetLoanQualityTypeDescription(overdue.Quality, culture.Language);
        }

        public static void SetCulture(Guarantee guarantee, Culture culture)
        {
            if (guarantee != null)
            {
                guarantee.LoanTypeDescription = Info.GetLoanTypeDescription(guarantee.LoanType, culture.Language);
                guarantee.QualityDescription = Info.GetLoanQualityTypeDescription(guarantee.Quality, culture.Language);
                SetCulture(guarantee.ConnectAccount, culture);
            }
        }

        public static void SetCulture(List<Guarantee> guarantees, Culture culture)
        {
            if (guarantees != null)
            {
                guarantees.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(Accreditive accreditive, Culture culture)
        {
            if (accreditive != null)
            {
                accreditive.LoanTypeDescription = Info.GetLoanTypeDescription(accreditive.LoanType, culture.Language);
                accreditive.QualityDescription = Info.GetLoanQualityTypeDescription(accreditive.Quality, culture.Language);
                SetCulture(accreditive.ConnectAccount, culture);
            }
        }

        public static void SetCulture(List<Accreditive> accreditives, Culture culture)
        {
            if (accreditives != null)
            {
                accreditives.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(PaidGuarantee paidGuarantee, Culture culture)
        {
            if (paidGuarantee != null)
            {
                paidGuarantee.LoanTypeDescription = Info.GetLoanTypeDescription(paidGuarantee.LoanType, culture.Language);
                paidGuarantee.QualityDescription = Info.GetLoanQualityTypeDescription(paidGuarantee.Quality, culture.Language);
                SetCulture(paidGuarantee.LoanAccount, culture);
            }
        }

        public static void SetCulture(List<PaidGuarantee> paidGuarantees, Culture culture)
        {
            if (paidGuarantees != null)
            {
                paidGuarantees.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(PaidAccreditive paidAccreditive, Culture culture)
        {
            if (paidAccreditive != null)
            {
                paidAccreditive.LoanTypeDescription = Info.GetLoanTypeDescription(paidAccreditive.LoanType, culture.Language);
                paidAccreditive.QualityDescription = Info.GetLoanQualityTypeDescription(paidAccreditive.Quality, culture.Language);
                SetCulture(paidAccreditive.LoanAccount, culture);
            }
        }

        public static void SetCulture(List<PaidAccreditive> paidAccreditives, Culture culture)
        {
            if (paidAccreditives != null)
            {
                paidAccreditives.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(Factoring factroing, Culture culture)
        {
            if (factroing != null)
            {
                factroing.TypeDescription = Info.GetLoanTypeDescription(factroing.Type, culture.Language);
                factroing.QualityDescription = Info.GetLoanQualityTypeDescription(factroing.Quality, culture.Language);
                factroing.FactoringTypeDescription = Info.GetFactoringTypeDescription(factroing.FactoringType, culture.Language);
                factroing.FactoringRegresTypeDescription = Info.GetFactoringRegresTypeDescription(factroing.FactoirngRegresType, culture.Language);
            }
        }

        public static void SetCulture(List<Factoring> listFactoring, Culture culture)
        {
            if (listFactoring != null)
            {
                listFactoring.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(PaidFactoring factroing, Culture culture)
        {
            if (factroing != null)
            {
                factroing.LoanTypeDescription = Info.GetLoanTypeDescription(factroing.LoanType, culture.Language);
                factroing.QualityDescription = Info.GetLoanQualityTypeDescription(factroing.Quality, culture.Language);

            }
        }

        public static void SetCulture(List<PaidFactoring> listFactoring, Culture culture)
        {
            if (listFactoring != null)
            {
                listFactoring.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(DepositCase depositCase, Culture culture)
        {
            if (depositCase != null)
            {
                depositCase.QualityDescription = Info.GetLoanQualityTypeDescription(depositCase.Quality, culture.Language);
            }
        }

        public static void SetCulture(List<DepositCase> depositCases, Culture culture)
        {
            if (depositCases != null)
            {
                depositCases.ForEach(m => SetCulture(m, culture));
            }
        }
        public static void SetCulture(CardClosingOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            order.ClosingReasonDescription = Info.GetCardClosingReasonDescription(order.ClosingReason, culture.Language);
        }


        /// <summary>
        /// Հաշիվների որոնում
        /// </summary>
        public static void SetCulture(SearchAccountResult account, Culture culture)
        {
            if (account != null)
            {
                account.AccountTypeDescription = Info.AccountTypeDescription(account.AccountType, culture.Language);
                account.Description = Utility.ConvertAnsiToUnicode(account.Description);
            }
        }

        public static void SetCulture(List<SearchAccountResult> accounts, Culture culture)
        {
            accounts.ForEach(m => SetCulture(m, culture));
        }

        /// <summary>
        /// Քարտերի որոնում
        /// </summary>
        public static void SetCulture(SearchCardResult card, Culture culture)
        {
            if (card != null)
            {
                card.CardHolderDescription = Utility.ConvertAnsiToUnicode(card.CardHolderDescription);
            }
        }

        public static void SetCulture(List<SearchCardResult> cards, Culture culture)
        {
            cards.ForEach(m => SetCulture(m, culture));
        }


        /// <summary>
        ///Հաշվի բացման հայտ
        /// </summary>
        public static void SetCulture(AccountOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);

            if(order.StatementDeliveryType != null)
            {
                order.StatementDeliveryTypeDescription = Info.GetStatementDeliveryTypeDescription(order.StatementDeliveryType, culture.Language);
            }

            
        }

        /// <summary>
        ///Հաշվի տվյալների խմբագրման հայտ
        /// </summary>
        public static void SetCulture(AccountDataChangeOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            if (order.AdditionalDetails != null && order.AdditionalDetails.AdditionType == 101)
            {
                order.AdditionalDetails.AdditionTypeDescription = "Հաշվեհամարի կարգավիճակի փոփոխություն";
                order.AdditionalDetails.AdditionValue = order.AdditionalDetails.AdditionValue.Equals("0") ? "Նորմալ" : "Ժամանակավոր";
            }

            //order.StatementDeliveryTypeDescription = Info.GetStatementDeliveryTypeDescription(order.StatementDeliveryType, culture.Language);
        }

        public static void SetCulture(OrderHistory orderHistory, Culture culture)
        {
            if (orderHistory.Quality == OrderQuality.Declined)
            {
                orderHistory.ReasonDescription = Info.GetOrderRejectTypeDescription(orderHistory.ReasonId, culture.Language);
            }
            orderHistory.ActionDescription = Info.GetOrderQualityTypeDescriptionForAcbaOnline((short)orderHistory.Quality, culture.Language);
        }

        public static void SetCulture(List<OrderHistory> ordersHistory, Culture culture)
        {

            ordersHistory.ForEach(m => SetCulture(m, culture));

        }

        public static void SetCulture(TransitPaymentOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            order.TransitAccountTypeDescription = Info.GetTransitAccountTypeDescription((int)order.TransitAccountType, culture.Language);
        }


        public static void SetCulture(AccountFreezeDetails freezeDetail, Culture culture)
        {
            freezeDetail.ReasonDescription = Info.GetFreezeReasonDescription(freezeDetail.ReasonId);
            freezeDetail.UnfreezeReasonDescription = Info.GetFreezeReasonDescription(freezeDetail.ReasonId);
        }

        public static void SetCulture(List<AccountFreezeDetails> freezes, Culture culture)
        {
            freezes.ForEach(m => SetCulture(m, culture));
        }

        public static void SetCulture(LoanProductOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            if (order.Type == OrderType.CreditLineSecureDeposit || order.Type == OrderType.FastOverdraftApplication)
            {
                order.ProductTypeDescription = Info.GetCreditLineTypeDescription((short)order.ProductType, culture.Language);
            }
            else
            {
                order.ProductTypeDescription = Info.GetLoanTypeDescription((short)order.ProductType, culture.Language);
            }

            order.CommunicationTypeDescription = Info.GetCommunicationTypeDescription(order.CommunicationType, culture.Language);
            order.DisputeResolutionDescription = Info.GetDisputeResolutionDescription(order.DisputeResolution, culture.Language);

        }

        public static void SetCulture(FeeForServiceProvidedOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            order.ServiceTypeDescription = Info.GetServiceProvidedTypeDescription(order.ServiceType, culture.Language);
        }

        /// <summary>
        /// Գործարքի հեռացման հայտ
        /// </summary>
        public static void SetCulture(RemovalOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
            order.RemovingReasonDescription = Info.GetOrderRemovingReasonDescription(order.RemovingReason, culture.Language);
        }

        /// <summary>
        /// Ավանդի հայտ 
        /// </summary>
        public static void SetCulture(DepositOrder order, Culture culture)
        {
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);

            if (order.Deposit.StatementDeliveryType != null)
                order.Deposit.StatementDeliveryTypeDescription = Info.GetStatementDeliveryTypeDescription(order.Deposit.StatementDeliveryType, culture.Language);

            order.Deposit.DepositTypeDescription = Info.GetDepositTypeDescription((short)order.DepositType, culture.Language);


           

            //if (order.Deposit.DepositOption != null)
            //{
            //    order.Deposit.DepositOption.ForEach(m => SetCulture(m, order.Deposit.Currency, culture));
            //}

        }


        public static void SetCulture(DepositOption option, string depositCurrency, Culture culture)
        {
            DepositOption infoOption = Info.GetBusinesDepositOption(option.Type, culture.Language);
            option.OptionGroup = infoOption.OptionGroup;
            option.TypeDescription = infoOption.TypeDescription;
            option.Rate = Deposit.GetBusinesDepositOptionRate(option.Type, depositCurrency);
        }

        public static void SetCulture(List<Claim> claims, Culture culture)
        {
            claims.ForEach(m => SetCulture(m, culture));
        }

        public static void SetCulture(Claim claim, Culture culture)
        {
            claim.PurposeDescription = Info.GetClaimPurposeDescription(claim.Purpose);
            claim.QualityDescription = Info.GetClaimQualityDescription(claim.Quality);
        }

        public static void SetCulture(List<ClaimEvent> events, Culture culture)
        {
            events.ForEach(m => SetCulture(m, culture));
        }

        public static void SetCulture(ClaimEvent claimEvent, Culture culture)
        {
            claimEvent.PurposeDescription = Info.GetClaimEventPurposeDescription(claimEvent.Purpose);
            claimEvent.TypeDescription = Info.GetClaimEventTypeDescription(claimEvent.Type);
            claimEvent.CourtTypeDescription = Info.GetCourtTypeDescription(claimEvent.CourtType);
        }

        public static void SetCulture(Tax tax, Culture culture)
        {
            tax.QualityDescription = Info.GetLoanQualityTypeDescription(tax.Quality, culture.Language);
            tax.TypeDescription = Info.GetTaxTypeDescription(tax.Type);
        }


        public static void SetCulture(List<PensionApplication> pensionApplications, Culture culture)
        {
            if (pensionApplications != null)
            {
                pensionApplications.ForEach(m => SetCulture(m));
            }
        }

        public static void SetCulture(PensionApplication pensionApplication)
        {
            pensionApplication.QualityDescription = Info.GetPensionAppliactionQualityType(pensionApplication.Quality);
        }

        public static void SetCulture(TransferCallContractDetails contract)
        {
            contract.QualityDescription = Info.GetTransferCallContractQualityType(contract.Quality);
        }

        public static void SetCulture(List<TransferCallContractDetails> contracts, Culture culture)
        {
            if (contracts != null)
            {
                contracts.ForEach(m => SetCulture(m));
            }
        }
        internal static void SetCulture(List<RejectedOrderMessage> userRejectedMessages, Culture culture)
        {
            if (userRejectedMessages != null)
            {
                userRejectedMessages.ForEach(m => SetCulture(m.Order, culture));
            }
        }
        internal static void SetCulture(List<CardTariffContract> customerCardTariffContracts, Culture culture)
        {
            if (customerCardTariffContracts != null)
            {
                customerCardTariffContracts.ForEach(m => SetCulture(m, culture));
            }
        }
        public static void SetCulture(CardTariffContract contract, Culture culture)
        {
            contract.QualityDescription = Info.GetCardTariffContractQualityDescription(contract.Quality);
            contract.ReasonDescription = Info.GetCardTariffContractReasonDescription(contract.Reason);
        }


        internal static void SetCulture(List<PosLocation> customerPosLocations, Culture culture)
        {
            if (customerPosLocations != null)
            {
                customerPosLocations.ForEach(m => SetCulture(m, culture));
            }
        }
        public static void SetCulture(PosLocation posLocation, Culture culture)
        {
            posLocation.QualityDescription = Info.GetPosLocationQualityDesc(posLocation.Quality);
            posLocation.LocationTypeDescription = Info.GetPosLocationTypeDescription(posLocation.LocationType);

            if (posLocation.Posterminals != null)
            {
                posLocation.Posterminals.ForEach(m => SetCulture(m, culture));
            }
        }

        private static void SetCulture(PosTerminal posTerminal, Culture culture)
        {
            posTerminal.QualityDescription = Info.GetPosTerminalQualityDesc(posTerminal.Quality);
            posTerminal.TypeDescription = Info.GetPosTerminalTypeDescription(posTerminal.Type);

        }

        internal static void SetCulture(List<ProvisionLoan> provisionLoans, Culture culture)
        {
            if (provisionLoans != null)
            {
                provisionLoans.ForEach(m => SetCulture(m, culture));
            }
        }
        private static void SetCulture(ProvisionLoan m, Culture culture)
        {
            m.LoanTypeDescription = Info.GetLoanTypeDescription(m.Loantype, culture.Language);
            m.LoanQualityDescription = Info.GetLoanQualityTypeDescription(m.LoanQuality, culture.Language);
        }


        public static void SetCulture(List<Insurance> insurance, Culture culture)
        {
            if (insurance != null)
            {
                insurance.ForEach(m => SetCulture(m, culture));
            }
        }
        public static void SetCulture(Insurance insurance, Culture culture)
        {
            insurance.InsuranceTypeDescription = Info.GetInsuranceTypeDescription(insurance.InsuranceType, culture.Language);
            insurance.CompanyName = Info.GetInsuranceCompanyDescription(insurance.Company);
            insurance.QualityDescription = Info.GetLoanQualityTypeDescription(insurance.Quality, culture.Language);
        }
        /// <summary>
        /// Ավանդի դադարեցում
        /// </summary>
        public static void SetCulture(DepositTerminationOrder order, Culture culture)
        {
            order.ClosingReasonTypeDescription = Info.GetDepositClosingReasonTypeDescription(order.ClosingReasonType, culture.Language);
        }


        /// <summary>
        /// Վակային դիմումներ
        /// </summary>
        public static void SetCulture(LoanApplication loanApplication, Culture culture)
        {
            if (loanApplication != null)
            {
                loanApplication.ProductTypeDescription = Info.GetLoanApplicationProductTypeDescription(loanApplication.ProductType, culture.Language);
                loanApplication.QualityDescription = Info.GetLoanApplicationQualityTypeDescription(loanApplication.Quality, culture.Language);
                loanApplication.CommunicationTypeDescription = Info.GetCommunicationTypeDescription(loanApplication.CommunicationType, culture.Language);
            }
        }

        public static void SetCulture(List<LoanApplication> loanApplications, Culture culture)
        {
            loanApplications.ForEach(m => SetCulture(m, culture));
        }

        public static void SetCulture(DemandDepositRateChangeOrder order, Culture culture)
        {
            order.TariffGroupDescription = Info.GetDemandDepositsTariffGroup(order.TariffGroup);
            order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
            order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);

        }

        /// <summary>
        /// Ֆոնդեր
        /// </summary>
        public static void SetCulture(Fond fond, Culture culture)
        {
            if (fond != null)
            {
                fond.Description = Info.GetFondDescription((short)fond.ID);
            }
        }

        public static void SetCulture(List<Fond> fonds, Culture culture)
        {
            fonds.ForEach(m => SetCulture(m, culture));
        }

        //public static void SetCulture(ProductNotificationConfigurationsOrder order, Culture culture)
        //{
        //    order.SubTypeDescription = Info.GetOrderSubTypeDescription(order.Type, order.SubType, culture.Language);
        //    order.QualityDescription = Info.GetOrderQualityTypeDescription((short)order.Quality, culture.Language);
        //}

        public static void SetCulture(ArcaCardsTransactionOrder order, Languages lang)
        {
            DataTable dt;

            dt = Info.GetActionsForCardTransaction();
            foreach (DataRow row in dt.Rows)
            {
                order.ActionTypeDescription = (row["id"].ToString() == order.ActionType.ToString()) ? row["description_arm"].ToString() : null;
                if (!string.IsNullOrEmpty(order.ActionTypeDescription))
                {
                    break;
                }
            }


            dt = Info.GetReasonForCardTransactionAction(order.ActionReasonId);
            order.ActionReasonDescription = lang == Languages.hy ? dt.Rows[0]["description_arm"].ToString() : dt.Rows[0]["description_eng"]?.ToString();
        }

        public static void SetCulture(PensionSystem system, Culture culture)
        {
            if (system != null && system.Errors.Count > 0)
            {
                system.Errors.ForEach(m => SetCulture(m, culture));
            }
        }

        public static void SetCulture(Template template, Culture culture)
        {
            if(template != null)
            {
                template.TemplateDocumentSubTypeDescription = Info.GetOrderSubTypeDescription(template.TemplateDocumentType, template.TemplateDocumentSubType, culture.Language);
            }
        }

        public static void SetCulture(ContentResult<string> result, Culture culture)
        {
            if (result.Errors != null && result.Errors.Count > 0)
            {
                result.Errors.ForEach(m => SetCulture(m, culture));
            }
            result.Errors = result.Errors.GroupBy(i => i.Description).Select(g => g.First()).ToList();
        }
    }
}
