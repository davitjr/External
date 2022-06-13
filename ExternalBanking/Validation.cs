using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.ARUSDataService;
using ExternalBanking.CreditLineActivatorARCA;
using ExternalBanking.DBManager;
using ExternalBanking.DBManager.Acbamat;
using ExternalBanking.Events;
using ExternalBanking.ServiceClient;
using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using static ExternalBanking.AcbamatThirdPartyWithdrawalOrder;
using static ExternalBanking.CardlessCashoutOrder;

namespace ExternalBanking
{
    public static class Validation
    {
        /// <summary>
        /// Ստուգում է, արդյոք փաստաթղթի համարը կրկնվում է տվյալ օրվա մեջ
        /// </summary>
        /// <returns></returns>
        internal static List<ActionError> ValidateDocumentNumber(Order order, ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.OrderNumber.Trim().Length == 0)
            {
                //Փաստաթղթի համարը լրացված չէ:
                result.Add(new ActionError(12));
            }
            else if (order.OrderNumber.Length > 20)
            {
                //Փաստաթղթի համարը երկար է:
                result.Add(new ActionError(162));
            }
            else if (order.Source != SourceType.Bank && order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                if (!ValidateDocumentNumber(customerNumber, order.RegistrationDate, (int)order.Id, order.OrderNumber))
                {
                    if (order.Source == SourceType.MobileBanking || order.Source == SourceType.AcbaOnline)
                    {
                        //Փաստաթղթի համարը կրկնվում է, օրվա ընթացքում նույն համարով կարող է լինել միայն 1 փաստաթուղթ:
                        while (true)
                        {
                            order.OrderNumber = (Convert.ToInt32(order.OrderNumber) + 1).ToString();
                            if (Validation.ValidateDocumentNumber(order.CustomerNumber, order.RegistrationDate, (int)order.Id, order.OrderNumber))
                                break;
                        }
                    }
                    else
                        //Փաստաթղթի համարը կրկնվում է, օրվա ընթացքում նույն համարով կարող է լինել միայն 1 փաստաթուղթ:
                        result.Add(new ActionError(8));
                }

            }
            if (order.Id != 0)
            {
                bool check = Order.CheckDocumentID(order.Id, customerNumber);
                if (check == false)
                {
                    //Փաստաթուղթը գոյություն չունի
                    result.Add(new ActionError(477));
                }
            }

            return result;
        }

        /// <summary>
        /// Ավանդի հայտի փաստաթղթի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateDepositOrderDocument(DepositOrder order, ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateDraftOrderQuality(order, customerNumber));

            result.AddRange(ValidateDocumentNumber(order, customerNumber));

            List<ActionError> depositOrderDatesValidation = ValidateDepositOrderDates(order);

            result.AddRange(depositOrderDatesValidation);
            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
            {
                if (order.DebitAccount == null || order.DebitAccount.AccountNumber == "0")
                {
                    /// Դեբետ հաշիվը մուտքագրված չէ:
                    result.Add(new ActionError(219));
                }
            }

            if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                if (order.Deposit.InvolvingSetNumber == 0)
                {
                    //Մուտքագրեք ՊԿ-ի տվյալները:
                    result.Add(new ActionError(703));
                }
                else
                {
                    if (!ACBAOperationService.CheckCustomerInvolvingEmployeeFilial((short)order.Deposit.InvolvingSetNumber, (short)order.FilialCode))
                    {
                        //Նշված համարով ՊԿ տվյալ մասնաճյուղում գոյություն չունի:
                        result.Add(new ActionError(704));
                    }
                }
            }

            if (string.IsNullOrEmpty(order.Deposit.Currency))
            {
                //Արժույթը ընտրված չէ
                result.Add(new ActionError(240));
            }

            short customerType;
            List<CustomerEmail> customerEmails;
            List<CustomerDocument> customerDocuments;

            var customer = ACBAOperationService.GetCustomer(customerNumber);
            customerType = customer.customerType.key;

            if (customerType == (short)CustomerTypes.physical)
            {
                customerEmails = (customer as PhysicalCustomer).person.emailList;
                customerDocuments = (customer as PhysicalCustomer).person.documentList;
            }
            else
            {
                customerEmails = (customer as LegalCustomer).Organisation.emailList;
                customerDocuments = (customer as LegalCustomer).Organisation.documentList;
            }

            if (customerType == (short)CustomerTypes.physical)
            {
                DateTime? BirthDate = (customer as PhysicalCustomer).person.birthDate;
                BirthDate = BirthDate.Value.AddYears(14);
                if (BirthDate.Value > DateTime.Now)
                {
                    //Ավանդ կնքելու համար ընրտրեք 18 տարին լրացած հաճախորդ
                    result.Add(new ActionError(543));
                    return result;
                }
            }



            if (order.DepositType == 0)
            {
                //Ավանդի տեսակը ընտրված չէ
                result.Add(new ActionError(215));
            }
            else if (order.DepositType == DepositType.ChildrensDeposit)
            {
                if (order.ThirdPersonCustomerNumbers.Count > 1)
                {
                    //Հօգուտ երրորդ անձի ավանդ ձևակերպելիս թույլատրվում է մուտքագրել 2 հաճախորդ
                    result.Add(new ActionError(541));
                }
                else
                {
                    KeyValuePair<ulong, string> oneJointCustomer = order.ThirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                    if (oneJointCustomer.Key != 0)
                    {

                        if ((oneJointCustomer.Key.ToString()).Length != 12)
                        {
                            //Երրորդ անձը սխալ է ընտրված
                            result.Add(new ActionError(480));
                        }
                        else if (order.CustomerNumber == oneJointCustomer.Key)
                        {
                            //Երրորդ անձը սխալ է ընտրված
                            result.Add(new ActionError(480));
                        }
                        else
                        {
                            List<ulong> thirdpersons = Deposit.ThirdPersonsCustomerNumbers(customerNumber);
                            if (!thirdpersons.Exists(m => m == oneJointCustomer.Key))
                            {
                                //Երրորդ անձը սխալ է ընտրված
                                result.Add(new ActionError(480));
                            }
                        }
                    }
                }
            }
            if (order.AccountType == 2 && order.ThirdPersonCustomerNumbers.Count > 2)
            {
                //3 և ավելի  հաճախորդների համար համատեղ ավանդի ձևակերպում նախատեսված չէ
                result.Add(new ActionError(542));
            }
            else if (order.AccountType == 2)
            {

                var jointResult = order.ThirdPersonCustomerNumbers.GroupBy(x => new { x.Key }).Where(item => item.Count() > 1).ToDictionary(g => g.Key, g => g.Count());
                if (jointResult.Count != 0)
                    //Համատեղ հաշվի հաճախորդը սխալ է ընտրված
                    result.Add(new ActionError(504));

                if (result.Count > 0)
                {
                    return result;
                }

                //Ստուգում է համատեղ հաճախորդների տվյալները:
                order.ThirdPersonCustomerNumbers.ForEach(m =>
                {
                    if (m.Key != 0)
                    {

                        var thirdCustomer = ACBAOperationService.GetCustomer(m.Key);

                        if (thirdCustomer.customerType.key != (short)CustomerTypes.physical)
                        {
                            //համատեղ ավանդ կնքելու համար ընտրեք ֆիզ.անձ:
                            result.Add(new ActionError(577));
                        }
                        else
                        {
                            DateTime? BirthDate = (thirdCustomer as PhysicalCustomer).person.birthDate;
                            BirthDate = BirthDate.Value.AddYears(18);
                            if (BirthDate.Value > DateTime.Now)
                            {
                                //Ավանդ կնքելու համար ընրտրեք 18 տարին լրացած հաճախորդ
                                result.Add(new ActionError(543));
                            }
                            else
                            {
                                //Հաճախորդի ստորագրության նմուշի ստուգում:
                                result.AddRange(ValidateCustomerSignature(thirdCustomer));
                            }
                        }
                    }
                });
            }
            if (order.Amount == 0)
            {
                //Գումարը մուտքագրված չէ
                result.Add(new ActionError(20));
            }
            else if (depositOrderDatesValidation?.Count == 0)
            {
                double minAmount = Deposit.GetDepositOrderCondition(order, order.Source).MinAmount;
                if (order.Amount < minAmount)
                {
                    if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                    {
                        //Ավանդի գումարը չպետք է փոքր լինի նվազագույն գումարից։
                        result.Add(new ActionError(2002));
                    }
                    else
                    {
                        //Ավանդի գումարը չպետք է լինի տվյալ ավանդի տեսակի նվազագույն գումարից փոքր:
                        result.Add(new ActionError(223));
                    }
                }
            }

            if (order.RecontractPossibility == YesNo.None)
            {
                //Ավտոմատ երկարաձգման տարբերակը ընտրված չէ
                result.Add(new ActionError(221));
            }



            if (InfoDB.CommunicationTypeExistenceFromSAP(order.CustomerNumber) == 0 && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                //SAP CRM ծրագրում հաճախորդի հետ հաղորդակցման եղանակը ընտրված չէ։
                result.Add(new ActionError(2034));
            }




            if (customerType == (short)CustomerTypes.physical && customerDocuments.FindAll(m => m.defaultSign).Count > 0)
            {

                CustomerDocument defaultDocument = customerDocuments.Find(m => m.defaultSign);
                if (defaultDocument.validDate < DateTime.Now.Date && (defaultDocument.validDate < new DateTime(2020, 3, 16).Date || DateTime.Now.Date > new DateTime(2020, 9, 13).Date))
                {
                    //Անձը հաստատող փաստաթուղթը ժամկետանց է
                    result.Add(new ActionError(224));
                }
            }


            if (order.AccountType != 1 && customerType != (short)CustomerTypes.physical)
            {
                //Իրավաբանական անձանց համար հնարավոր չէ ձևակերպել համատեղ կամ հօգուտ 3-րդ անձի ավանդ:
                result.Add(new ActionError(643));
            }

            else
            if (order.DepositType == DepositType.DepositFamily && customerType != (short)CustomerTypes.physical)
            {
                //Իրավաբանական անձանց համար հնարավոր է ձևակերպել միայն դասական ժամկետային,կուտակվող կամ փոխարկելի ավանդ:
                result.Add(new ActionError(642));
            }

            //Հաճախորդի ստորագրության նմուշի ստուգում:
            result.AddRange(ValidateCustomerSignature(customer));

            if (result.Count != 0)
            {
                return result;
            }

            List<ActionError> resultDebitAccount = ValidateDebitAccount(order, order.DebitAccount);
            result.AddRange(resultDebitAccount);
            if (resultDebitAccount.Count == 0)
            {
                if (order.DebitAccount.Currency != order.Deposit.Currency)
                {
                    //Ավանդի արժույթը և հաշվեհամարի արժույթը նույնը չեն
                    result.Add(new ActionError(479));
                }
            }
            if (result.Count != 0)
            {
                return result;
            }
            List<ActionError> resultPercentAccount = ValidateDebitAccount(order, order.PercentAccount);
            result.AddRange(resultPercentAccount);
            if (resultPercentAccount.Count == 0)
            {
                if (order.PercentAccount.Currency != "AMD" && order.PercentAccount.Currency != order.Currency)
                {
                    //Տոկոսագումարի հաշիվը դրամային չէ
                    result.Add(new ActionError(478));
                }
            }

            result.AddRange(order.CheckDepositOrderCondition().Errors);

            if (order.DepositAccount != null)
            {

                Account account;

                account = Account.GetAccount(order.DepositAccount.AccountNumber, order.CustomerNumber);
                if (account == null)
                {
                    //Ավանդային հաշվեհամարը գտնված չէ
                    result.Add(new ActionError(844));
                    return result;
                }


            }

            if (order.DepositType == DepositType.ConvertibleDeposit)
            {
                List<Account> accounts = new List<Account>();
                accounts = Account.GetCurrentAccounts(customerNumber, ProductQualityFilter.Opened);
                string missingCurrency = "";


                if (!accounts.Exists(m => m.Currency == "USD"))
                {
                    missingCurrency = "USD,";
                }
                if (!accounts.Exists(m => m.Currency == "AMD"))
                {
                    missingCurrency = missingCurrency + "AMD,";
                }
                if (!accounts.Exists(m => m.Currency == "EUR"))
                {
                    missingCurrency = missingCurrency + "EUR,";
                }
                if (!accounts.Exists(m => m.Currency == "RUR"))
                {
                    missingCurrency = missingCurrency + "RUR,";
                }

                if (missingCurrency.Length != 0)
                {

                    missingCurrency = missingCurrency.Substring(0, missingCurrency.Length - 1);
                    //Տվյալ տեսակի ավանդի հայտ կազմելու համար անհրաժեշտ է բացել missingCurrency արժույթով ընթացիկ հաշիվ
                    result.Add(new ActionError(351, new string[] { missingCurrency }));
                }

            }
            if (!Utility.IsCorrectAmount(order.Amount, order.Deposit.Currency))
            {
                //Գումարը սխալ է մուտքագրված
                result.Add(new ActionError(25));
            }



            string canChangeDepositRate = null;

            if (order.Source == SourceType.Bank)
            {
                order.user.AdvancedOptions.TryGetValue("canChangeDepositRate", out canChangeDepositRate);
            }

            if (order.InterestRateChanged == true && canChangeDepositRate != "1")
            {
                //Ավանդի տոկոսադրույքի փոփոխման գործուղությունը հասանելի չէ:
                result.Add(new ActionError(1033));
            }
            if (order.IsActionDeposit)
            {

                if (order.AccountType == 2)
                {
                    //Ակցիայով հնարավոր է մուտքագրել միայն անհատական ավանդ
                    result.Add(new ActionError(1093));
                }

                double amountMaxLimit = DepositOrder.GetDepositActionAmountMaxLimit(order);
                if (order.Amount > amountMaxLimit)
                {

                    //Գումարը ակցիայով նախատեսված գումարից մեծ է:Ակցիայի գումար ՝ {}
                    result.Add(new ActionError(1091, new string[] { amountMaxLimit.ToString() }));
                }
            }


            if (order.DepositType == DepositType.BusinesDeposit)
            {
                int allowAdditionOption = 0;
                int allowDecreasingOption = 0;
                int repaymentType = 0;
                int ratePeriod = 0;

                foreach (DepositOption option in order.Deposit.DepositOption)
                {

                    switch (option.Type)
                    {
                        case 1:
                            allowAdditionOption = 1;
                            allowDecreasingOption = 0;
                            break;
                        case 2:
                            allowDecreasingOption = 1;
                            allowAdditionOption = 0;
                            break;
                        case 3:
                            allowDecreasingOption = 1;
                            allowAdditionOption = 1;
                            break;
                        case 4:
                            repaymentType = 1;
                            ratePeriod = -1;//Տոկոսագումարների վճարում ժամկետի վերջում
                            break;
                        case 5:
                            repaymentType = 2;
                            ratePeriod = 1;
                            break;
                        case 6:
                            repaymentType = 1;
                            ratePeriod = 1;
                            break;
                        default:
                            break;
                    }

                }


                if (repaymentType == 2 && ratePeriod == -1)
                {
                    //Կապիտալացման դեպքում տոկոսագումարները չեն կարող վճարվել ժամկետի վերջում
                    result.Add(new ActionError(1083));
                }

                int monthDiff = Utility.GetMonthsBetween(order.Deposit.EndDate, order.Deposit.StartDate);
                if (monthDiff <= 3)
                {
                    if (allowAdditionOption == 1)
                    {
                        //Ավելացում կատարել չի թույլատրվում, ավանդի տևողությունը 3 ամսից պակաս է:
                        result.Add(new ActionError(1085));
                    }

                    if (allowDecreasingOption == 1)
                    {
                        //Պակասեցում կատարել չի թույլատրվում, ավանդի տևողությունը 3 ամսից պակաս է:
                        result.Add(new ActionError(1086));
                    }
                }


            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }


            if (CheckCustomerPhoneNumber(order.CustomerNumber))
            {
                //Հաճախորդի համար չկա մուտքագրված հեռախոսահամար:
                result.Add(new ActionError(1904));
            }


            return result;
        }

        internal static IEnumerable<ActionError> ValidateAcbamatThirdPartyWithdrawalOrder(AcbamatThirdPartyWithdrawalOrder acbamatThirdPartyWithdrawalOrder)
        {
            List<ActionError> result = new List<ActionError>();
            double sumAmount = AcbamatThirdPartyWithdrawalOrderDB.GetWithdrawnAmountForToday(acbamatThirdPartyWithdrawalOrder.UserId);
           
            if (sumAmount >= 2000000)
            {
                throw new AcbamatThirdPartyWithdrawalException($"Acbamat {acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType} Գումարի կանխիկացումը հնարավոր չէ կատարել։ ");
            }

            ulong accountNumber = AcbamatThirdPartyWithdrawalOrderDB.GetThirdPartyOrganizationAccount(acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType);
            if (accountNumber is 0)
            {
                throw new AcbamatThirdPartyWithdrawalException($"Acbamat {acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType} կազմակերպության հաշիվը բացակայում է։");
            }

            double availableBalance = Account.GetAcccountAvailableBalance(accountNumber.ToString());
            if (acbamatThirdPartyWithdrawalOrder.Amount > availableBalance)
            {
                string toEmail = AcbamatThirdPartyWithdrawalOrderDB.GetThirdPartyOrganizationEmail(acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType);
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new AcbamatThirdPartyWithdrawalException($"Acbamat {acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType} կազմակերպության էլ. հասցեն բացակայում է։");
                }

                string mailContent =
                       $@"Հարգելի գործընկերներ, <br/><br/>
                        Տեղեկացնում ենք, որ {accountNumber} հաշվի մնացորդը բավարար չէ Ձեր հաճախորդների կանխիկացման գործարքները ապահովելու համար։<br/><br/>
                        Հարգանքով՝<br/>
                        «Ակբա Բանկ» ԲԲԸ";

                EmailMessagingService.Email email = new EmailMessagingService.Email
                {
                    Content = mailContent,
                    Subject = "Կանխիկացման գործարքի մերժում",
                    From = 4,
                    To = toEmail
                };

                EmailMessagingOperations.SendEmail(email);
                throw new AcbamatThirdPartyWithdrawalException($"Acbamat {acbamatThirdPartyWithdrawalOrder.ThirdPartyOrganizationType} կազմակերպության հաշիվը անբավարար է։ կանխիկացվող գումար - {acbamatThirdPartyWithdrawalOrder.Amount}, մնացորդ - {availableBalance}");
            }

            Math.DivRem((int)(decimal)acbamatThirdPartyWithdrawalOrder.Amount, 1000, out int remainder);
            if (acbamatThirdPartyWithdrawalOrder.Amount < 1000 || acbamatThirdPartyWithdrawalOrder.Amount > 399000 || remainder != 0)
            {
                //Հաճախորդին տրամադրվող գումարը փոքր է 1000 ՀՀ դրամից, կամ մեծ է 399 000 ՀՀ դրամից, կամ չի հանդիսանում 1000 դրամի բազմապատիկ։
                result.Add(new ActionError(2060));
            }

            return result;
        }

        internal static IEnumerable<ActionError> ValidateAcbamatExchangeOrder(AcbamatExchangeOrder acbamatExchangeOrder)
        {
            List<ActionError> result = new List<ActionError>();
            ExchangeRate rate = ExchangeRate.GetExchangeRates().Where(x => x.SourceCurrency == acbamatExchangeOrder.Currency).FirstOrDefault();

            int quotient = Math.DivRem((int)((decimal)acbamatExchangeOrder.Amount * (decimal)rate.BuyRateCash), 1000, out int remainder);
            if ((quotient * 1000) != acbamatExchangeOrder.Dispened || remainder != acbamatExchangeOrder.Fee)
            {
                //Գումարը սխալ է հաշվարկված։
                result.Add(new ActionError(2047));
            }
            if (acbamatExchangeOrder.Dispened + acbamatExchangeOrder.Fee > 400000)
            {
                //Փոխարկման մեկ գործարքի գումարը չի կարող գերազանցել 400.000 ՀՀ դրամը։
                result.Add(new ActionError(2048));
            }
            return result;
        }

        internal static List<ActionError> ValidateArcaCardsTransactionOrder(ArcaCardsTransactionOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            //if (order.Card.CardAccount.AccountNumber != "0")
            //{
            //    if (!Card.CheckCardOwner(order.Card.CardNumber, order.CustomerNumber))
            //    {
            //        //{Քարտային} հաշիվը չի պատկանում տվյալ հաճախորդին
            //        result.Add(new ActionError(901, new string[] { "Քարտային" }));
            //    }
            //}

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));


            //*******Հասանելիության ստուգումներ*******//
            string accessToUnblockCardForSpecificReasons = null;
            string accessToMakeArcaCardTransactionForBankInitiative = null;
            string accessToBlockUnblockCardForCourtProceedingsReason = null;

            if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                order.user.AdvancedOptions.TryGetValue("accessToUnblockCardForSpecificReasons", out accessToUnblockCardForSpecificReasons);
                order.user.AdvancedOptions.TryGetValue("accessToMakeArcaCardTransactionForBankInitiative", out accessToMakeArcaCardTransactionForBankInitiative);
                order.user.AdvancedOptions.TryGetValue("accessToBlockUnblockCardForCourtProceedingsReason", out accessToBlockUnblockCardForCourtProceedingsReason);

                if (order.ActionReasonId == 10 && accessToUnblockCardForSpecificReasons != "1" && accessToBlockUnblockCardForCourtProceedingsReason != "1")
                {
                    //Դատական վարույթի հիման վրա պատճառով քարտերը բլոկավորելու և ապաբլոկավորելու հնարավորություն ունի միայն Քարտային գործառնությունների բաժինը
                    result.Add(new ActionError(1549));
                }

                //Սխալ փոխանցում և հաշվից ելք պատճառով բլոկավորված քարտերն ապաբլոկավորելու հնարավորություն ունի միայն քարտը բլոկավորած մասնաճյուղը
                if (order.ActionType == 2 && (order.ActionReasonId == 5 || order.ActionReasonId == 6))
                {
                    if (!order.CheckTransactionAvailabilityDependsOnActionReason())
                    {
                        result.Add(new ActionError(1541));
                    }
                }
            }
            if ((order.ActionReasonId == 1 || order.ActionReasonId == 2) && order.ActionType == 2 && accessToUnblockCardForSpecificReasons != "1")
            {
                //Քարտի փակում և քարտի փոխարինում  պատճառներով բլոկավորված քարտերն ապաբլոկավորելու համար անհրաժեշտ է զանգահարել Քարտային գործառնություների բաժին:
                result.Add(new ActionError(1539));
            }


            int[] cardTransactionActionReasons = { 15, 16, 17, 18, 19, 20, 21, 22 };

            if (cardTransactionActionReasons.Contains(order.ActionReasonId) && accessToUnblockCardForSpecificReasons != "1" && accessToBlockUnblockCardForCourtProceedingsReason != "1")
            {
                //Հնարավոր չէ կատարել:
                result.Add(new ActionError(50));
            }


            if (order.ActionType == 2 && order.ActionReasonId == 11 && accessToUnblockCardForSpecificReasons != "1")
            {
                //Հնարավոր չէ կատարել:
                result.Add(new ActionError(50));
            }
            if (order.ActionReasonId == 12 && accessToMakeArcaCardTransactionForBankInitiative != "1")
            {
                //Բանկի նախաձեռնությամբ պատճառով քարտերը բլոկավորելու և ապաբլոկավորելու հնարավորություն ունի միայն Մոնիթորինգի թիմը:
                result.Add(new ActionError(1552));
            }
            //******************************************

            if (order.ActionReasonId == 4 && (order.Card.FilialCode != order.FilialCode))
            {
                //Ժամկետանց պարտավորություն պատճառով քարտը բլոկավորելու/ապաբլոկավորելու հնարավորություն ունի միայն քարտը թողարկող մասնաճյուղը
                result.Add(new ActionError(1540));
            }




            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking) && order.ActionType == 2)//ապաբլոկավորում 
            {
                Card card = Card.GetCardMainData((ulong)order.Card.ProductId, order.CustomerNumber);
                CardIdentification cardIdentification = new CardIdentification
                {
                    CardNumber = card.CardNumber,
                    ExpiryDate = card.ValidationDate.ToString("yyyyMM")
                };
                var arcaResult = ArcaDataService.GetCardData(cardIdentification);

                short blockingReason = ArcaCardsTransactionOrder.GetBlockingReasonForBlockedCard(order.CardNumber);

                if (!(arcaResult.cardDataField.hotCardStatusField == (int)HotCardStatus.LostCard_Capture ||
                    arcaResult.cardDataField.hotCardStatusField == (int)HotCardStatus.StonelCard_Capture ||
                    (arcaResult.cardDataField.hotCardStatusField == (int)HotCardStatus.DoNotHonor
                    && (blockingReason == 13 || blockingReason == 8))))//այլ
                {
                    //Ապաբլոկավորումը հնարավոր չէ կատարել։ Քարտը ապաբլոկավորելու համար կարող եք զանգահարել 010 31 88 88 հեռախոսահամարով կամ մոտենալ մոտակա մասնաճյուղ:
                    result.Add(new ActionError(1906));
                }
            }

            return result;
        }

        /// <summary>
        /// Դեբետ հաշվի ստուգումներ
        /// </summary>
        /// <returns></returns>
        internal static List<ActionError> ValidateDebitAccount(Order order, Account debitAccount)
        {
            List<ActionError> result = new List<ActionError>();



            if (debitAccount == null || debitAccount.AccountNumber == "0")
            {
                /// Դեբետ հաշիվը մուտքագրված չէ:
                result.Add(new ActionError(15));
            }
            else
            {
                Account account;

                account = Account.GetAccount(debitAccount.AccountNumber);
                //Դեբետային հաշիվը գտնված չէ
                if (account == null)
                {
                    result.Add(new ActionError(1));
                    return result;
                }
                else
                {
                    if (account.ClosingDate != null)
                    {
                        //Դեբետային հաշիվը փակ է:
                        result.Add(new ActionError(554, new string[] { account.AccountNumber.ToString() }));
                        return result;
                    }

                    if (order.Type != OrderType.CashBookSurPlusDeficitClosing && order.Type != OrderType.CashBookSurPlusDeficitClosingApprove && order.Type != OrderType.CashBookSurPlusDeficitPartiallyClosing && order.Type != OrderType.SSTerminalCashInOrder && order.Type != OrderType.SSTerminalCashOutOrder)
                    {
                        List<Account> permittedAccounts = Account.GetAccountsForOrder(order.CustomerNumber, order.Type, order.SubType, OrderAccountType.DebitAccount, order.Source, order.user);
                        //ապառիկ տեղումի տարանցիկ հաշիվը:(3691)

                        if (order.Type == OrderType.LoanMature && order.SubType != 5)
                        {

                            MatureOrder loanMatureOrder = (MatureOrder)order;

                            Loan loan = Loan.GetLoan(loanMatureOrder.ProductId, order.CustomerNumber);

                            if (!permittedAccounts.Exists(m => m.AccountNumber == loan.ConnectAccount.AccountNumber))
                            {
                                permittedAccounts.Add(loan.ConnectAccount);
                            }
                            //ապառիկ տեղումի մարման դեպքում հաճախորդի պարտավորությունները չենք ստուգում
                            if (!(loan.LoanType == 38 && debitAccount.AccountNumber == loan.ConnectAccount.AccountNumber))
                            {
                                result.AddRange(Validation.CheckCustomerDebtsAndDahk(order.CustomerNumber, account));
                            }
                        }
                        else if (order.Type == OrderType.LoanMature && order.SubType == 5)
                        {
                            MatureOrder loanMatureOrder = (MatureOrder)order;

                            bool isAparikTexumClaim = false;
                            if (loanMatureOrder.MatureType == MatureType.ClaimRepayment)
                            {
                                isAparikTexumClaim = Claim.IsAparikTexumClaim(loanMatureOrder.ProductId);
                            }
                            if (!isAparikTexumClaim)
                            {
                                result.AddRange(Validation.CheckCustomerDebtsAndDahk(order.CustomerNumber, account));
                            }
                        }

                        if (debitAccount.IsAttachedCard != true && order.Source != SourceType.AcbaMat)
                            account = permittedAccounts.Find(m => m.AccountNumber == debitAccount.AccountNumber);

                    }
                    //Դեբետային հաշիվը ձեզ չի պատկանում
                    if (account == null)
                    {
                        result.Add(new ActionError(2));
                        return result;
                    }
                    else if (account.Status == 1)
                    {
                        //Ժամանակավոր հաշվով գործարքների իրականացումն արգելված է
                        result.Add(new ActionError(545, new string[] { account.AccountNumber.ToString() }));
                    }
                    else if (account.FreezeDate != null)
                    {
                        //Դեբետային հաշիվը սառեցված է
                        result.Add(new ActionError(550, new string[] { account.AccountNumber.ToString() }));
                    }
                    else if (account.ClosingDate != null)
                    {
                        //Դեբետային հաշիվը փակ է:
                        result.Add(new ActionError(554, new string[] { account.AccountNumber.ToString() }));
                    }


                    if (result.Count > 0)
                    {
                        return result;
                    }
                    else if ((order.Source == SourceType.Bank || order.Source == SourceType.PhoneBanking)
                            && order.Type != OrderType.CashBookSurPlusDeficitClosing && order.Type != OrderType.CashBookSurPlusDeficitPartiallyClosing && order.Type != OrderType.CashBookSurPlusDeficitClosingApprove)
                    {
                        //Հաճախորդի ստորագրության նմուշի ստուգում:
                        result.AddRange(ValidateCustomerSignature(order.CustomerNumber));
                    }

                }


            }

            return result;
        }

        /// <summary>
        /// Ստացողի հաշվի համարի ստուգումներ
        /// </summary>
        /// <returns></returns>
        internal static List<ActionError> ValidateReceiverAccount(PaymentOrder paymentOrder)
        {
            List<ActionError> result = new List<ActionError>();

            if (paymentOrder.ReceiverAccount == null || paymentOrder.ReceiverAccount.AccountNumber == "0")
            {
                ///Մուտքագրվող (կրեդիտ) հաշիվը մուտքագրված չէ:
                result.Add(new ActionError(18));
            }
            else
            {
                //Փոխանցում սեփական հաշիվների մեջ կամ բանկի ներսում
                if ((paymentOrder.Type == OrderType.RATransfer && paymentOrder.SubType != 2 && paymentOrder.SubType != 5 && paymentOrder.SubType != 6)
                    || paymentOrder.Type == OrderType.Convertation || paymentOrder.Type == OrderType.CashDebit || paymentOrder.Type == OrderType.CashDebitConvertation
                    || paymentOrder.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                    || paymentOrder.Type == OrderType.InBankConvertation || paymentOrder.Type == OrderType.ReceivedFastTransferPaymentOrder
                    || paymentOrder.Type == OrderType.TransitNonCashOut || paymentOrder.Type == OrderType.ReestrTransferOrder)
                {
                    Account account;


                    account = Account.GetAccount(paymentOrder.ReceiverAccount.AccountNumber);

                    if (paymentOrder.Source == SourceType.Bank && (account.TypeOfAccount == 224 || account.TypeOfAccount == 279) && ValidationDB.IsLoanProductActive(account.AccountNumber) == false)
                    {
                        result.Add(new ActionError(1527));
                        return result;
                    }
                    //Կրեդիտային հաշիվը գտնված չէ
                    if (account == null)
                    {
                        result.Add(new ActionError(4));
                        return result;
                    }
                    //Եթե սեփական հաշիվների մեջ կամ փոխարկում է կամ Բանկի ներսում կամ արագ համակարգերով փոխանցման ստացում
                    else if ((paymentOrder.Type == OrderType.RATransfer && (paymentOrder.SubType == 3 || paymentOrder.SubType == 1)) || (paymentOrder.Type == OrderType.Convertation) || (paymentOrder.Type == OrderType.ReceivedFastTransferPaymentOrder))
                    {
                        if (!(paymentOrder.Type == OrderType.RATransfer && paymentOrder.SubType == 1))
                        {
                            List<Account> accounts = Account.GetAccounts(paymentOrder.CustomerNumber);
                            if (paymentOrder.Source == SourceType.Bank)
                            {
                                accounts.AddRange(Account.GetCustomerTransitAccounts(paymentOrder.CustomerNumber, ProductQualityFilter.Opened));
                            }
                            if (paymentOrder.DebitAccount.IsAttachedCard != true)
                                account = accounts.Find(m => m.AccountNumber == account.AccountNumber);

                            //Կրեդիտային հաշիվը ձեզ չի պատկանում
                            if (account == null)
                            {
                                result.Add(new ActionError(7));
                                return result;
                            }
                            //if (account.Status == 1)
                            //{
                            //    //Ժամանակավոր հաշվով գործարքների իրականացումն արգելված է
                            //    result.Add(new ActionError(545, new string[] { account.AccountNumber.ToString() }));
                            //}
                            //if (account.FreezeDate != null)
                            //{
                            //    //Կրեդիտային հաշիվը սառեցված է:
                            //    result.Add(new ActionError(551, new string[] { account.AccountNumber.ToString() }));
                            //}
                            if (account.ClosingDate != null)
                            {
                                //Կրեդիտային հաշիվը փակ է:
                                result.Add(new ActionError(555, new string[] { account.AccountNumber.ToString() }));
                            }
                        }

                        if (paymentOrder.Source == SourceType.AcbaOnline || paymentOrder.Source == SourceType.MobileBanking || paymentOrder.Source == SourceType.AcbaOnlineXML || paymentOrder.Source == SourceType.ArmSoft)
                        {
                            if (Account.IsAccountForbiddenForTransfer(account))
                            {
                                //...հաշիվը ժամանակավոր է, խնդրում ենք դիմել Բանկ
                                result.Add(new ActionError(1548, new string[] { account.AccountNumber.ToString() }));
                            }
                        }
                    }
                    if (paymentOrder.Source != SourceType.MobileBanking && paymentOrder.Source != SourceType.AcbaOnline && paymentOrder.Source != SourceType.SSTerminal && paymentOrder.Source != SourceType.CashInTerminal)
                    {
                        if (IsRequiredCheckBySintAccNew(account.AccountNumber))
                        {
                            //Հաճախորդի ստորագրության նմուշի ստուգում:
                            result.AddRange(ValidateCustomerSignature(paymentOrder.CustomerNumber));
                            if (result.Count > 0)
                            {
                                return result;
                            }
                        }
                    }


                    //Եթե քարտային հաշիվ է
                    if (paymentOrder.ReceiverAccount.IsCardAccount())
                    {
                        //Եթե առկա չէ գործող քարտ
                        if (!paymentOrder.ReceiverAccount.HaveActiveCard())
                        {
                            //Տվյալ հաշվին փոխանցում կատարել հնարավոր չէ: Մուտքագրվող (կրեդիտ) հաշիվը գոյություն չունի: 
                            result.Add(new ActionError(452));
                            return result;
                        }
                    }
                    //Եթե ավանդային հաշիվ է
                    else if (paymentOrder.ReceiverAccount.IsDepositAccount())
                    {
                        //Երե առկա չէ գործող ավանդ
                        if (!paymentOrder.ReceiverAccount.HasActiveDeposit())
                        {
                            //Տվյալ հաշվին փոխանցում կատարել հնարավոր չէ: Մուտքագրվող (կրեդիտ) հաշիվը գոյություն չունի: 
                            result.Add(new ActionError(452));
                            return result;
                        }
                        else
                        {

                            //Տվյալ ավանդատեսակը չի նախատեսում ավանդային հաշվին գումարների մուտքագրում:
                            //if (!Deposit.IsAllowedAmountAddition(paymentOrder.ReceiverAccount.AccountNumber))
                            //{
                            //    result.Add(new ActionError(187));
                            //    return result;
                            //}

                        }
                    }
                    //Եթե ոչ ավանդային է և ոչ էլ քարտային , ապա պետք է լինի ընթացիկ
                    else if (paymentOrder.Source != SourceType.Bank && paymentOrder.Source != SourceType.SSTerminal && paymentOrder.DebitAccount?.IsAttachedCard != true && !paymentOrder.ReceiverAccount.IsCurrentAccount() && !paymentOrder.ReceiverAccount.IsRestrictedAccount() && paymentOrder.Source != SourceType.CashInTerminal)
                    {
                        result.Add(new ActionError(452));
                        return result;
                    }


                }


                ///Մուտքային հաշվի ստուգում:
                string recAcc = paymentOrder.ReceiverAccount.AccountNumber.ToString();

                if (paymentOrder.SubType == 1 && (recAcc.Length != 15 ||
                                          !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12))
                                         ))
                {
                    //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                    result.Add(new ActionError(19));
                }
                else if (paymentOrder.ReceiverBankCode == 10300 && recAcc[5] == '9')
                {
                    if (recAcc.Length != 17 ||
                        !Utility.CheckAccountNumberControlDigit(recAcc.Substring(5)))
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Add(new ActionError(19));
                    }
                }
                else if (paymentOrder.ReceiverBankCode.ToString()[0] == '9')
                {
                    if (recAcc.Length != 12 || !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12)))
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Add(new ActionError(19));
                    }
                }
                else if (paymentOrder.SubType == 2 && !(recAcc.Length == 5 && (paymentOrder.Type == OrderType.RATransfer || paymentOrder.Type == OrderType.CashForRATransfer)) && (recAcc.Length < 12 || recAcc.Length > 16 || !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12))))
                {
                    //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                    result.Add(new ActionError(19));
                }


                if (paymentOrder.Source != SourceType.Bank && paymentOrder.Source != SourceType.SSTerminal && paymentOrder.Source != SourceType.CashInTerminal && paymentOrder.SubType == 1 && !(Utility.IsCorrectAccount(recAcc, 24, 10) ||
                                    Utility.IsCorrectAccount(recAcc, 24, 13) ||
                                    Utility.IsCorrectAccount(recAcc, 24, 11) ||
                                    Utility.IsCorrectAccount(recAcc, 24, 18) ||
                                    Utility.IsCorrectAccount(recAcc, 24, 116)
                                    ) && paymentOrder.Type != OrderType.CashTransitCurrencyExchangeOrder
                                      && paymentOrder.Type != OrderType.NonCashTransitCurrencyExchangeOrder && !paymentOrder.ReceiverAccount.IsRestrictedAccount())
                {

                    //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                    result.Add(new ActionError(19));
                }


                // Ստուգումը հանվել է  Babken Makaryan mobile -ից հնարավոր է արդեն կատարել բյուջե փոխանցում
                //if (paymentOrder.Source == SourceType.MobileBanking && ((paymentOrder.ReceiverBankCode == 10300 && recAcc[5] == '9') || paymentOrder.ReceiverBankCode.ToString()[0] == '9'))
                //{
                //    //Բյուջե փոխանցում նախատեսված չէ:Գործարքը կատարեք Home Banking համակարգի միջոցով:
                //    result.Add(new ActionError(965));
                //}
                //Ստուգումը հանվել է  Babken Makaryan mobile -ից հնարավոր է արդեն կատարել բյուջե փոխանցում



                if (paymentOrder.ReceiverAccount.Status == 1)
                {
                    //Ժամանակավոր հաշվով գործարքների իրականացումն արգելված է
                    result.Add(new ActionError(545, new string[] { paymentOrder.ReceiverAccount.AccountNumber.ToString() }));
                }

            }
            return result;
        }


        /// <summary>
        /// Միջնորդավճարի հաշվի ստուգումներ
        /// </summary>
        /// <returns></returns>
        internal static List<ActionError> ValidateFeeAccount(ulong customerNumber, Account feeAccount, SourceType source = SourceType.NotSpecified)
        {
            List<ActionError> result = new List<ActionError>();



            if (feeAccount == null || String.IsNullOrEmpty(feeAccount.AccountNumber) || feeAccount.AccountNumber == "0")
            {
                //Միջնորդավճարի հաշիվը մուտքագրված չէ:
                result.Add(new ActionError(90));
            }
            else
            {
                Account account;

                account = Account.GetAccount(feeAccount.AccountNumber);
                //Միջնորդավճարի հաշիվը գտնված չէ
                if (account == null)
                {
                    result.Add(new ActionError(118));
                    return result;
                }
                else
                {
                    result.AddRange(Validation.CheckCustomerDebtsAndDahk(customerNumber, account));
                    //account = Account.GetAccount(feeAccount.AccountNumber, customerNumber);

                    List<Account> accounts = Account.GetAccounts(customerNumber);
                    if (source == SourceType.Bank)
                    {
                        accounts.AddRange(Account.GetCustomerTransitAccounts(customerNumber, ProductQualityFilter.Opened));
                    }


                    account = accounts.Find(m => m.AccountNumber == account.AccountNumber);

                    //Միջնորդավճարի հաշիվը ձեզ չի պատկանում
                    if (account == null)
                    {
                        result.Add(new ActionError(119));
                        return result;
                    }
                    else if (account.Status == 1)
                    {
                        //Ժամանակավոր հաշվով գործարքների իրականացումն արգելված է
                        result.Add(new ActionError(545, new string[] { account.AccountNumber.ToString() }));
                    }
                    else if (account.FreezeDate != null)
                    {
                        //Դեբետային հաշիվը սառեցված է
                        result.Add(new ActionError(550, new string[] { account.AccountNumber.ToString() }));
                    }
                    else if (account.ClosingDate != null)
                    {
                        //Դեբետային հաշիվը փակ է:
                        result.Add(new ActionError(554, new string[] { account.AccountNumber.ToString() }));
                    }


                    if (account.AccountType == 115)
                    {
                        //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվից փոխանցում հնարավոր է կատարել միայն տվյալ քարտին
                        result.Add(new ActionError(1064));
                    }
                }

            }

            return result;
        }


        internal static List<ActionError> ValidateDraftOrderQuality(Order order, ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.Id != 0)
            {
                Order draftOrder = Order.GetOrder(order.Id, customerNumber);

                if (draftOrder.Quality != OrderQuality.Draft)
                {
                    //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ պահպանել:
                    result.Add(new ActionError(26));
                }
            }

            return result;
        }
        /// <summary>
        /// Ավանդի ժամկետների ստուգում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateDepositOrderDates(DepositOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.DepositType == DepositType.ChildrensDeposit)
            {
                if (order.ThirdPersonCustomerNumbers.Count == 1)
                {
                    KeyValuePair<ulong, string> oneJointCustomer = order.ThirdPersonCustomerNumbers.FindLast(m => m.Key != 0);
                    if (oneJointCustomer.Key == 0)
                    {
                        //Ընտրեք երրորդ անձին:
                        result.Add(new ActionError(391));
                    }
                    else
                    {
                        var thirdCustomer = ACBAOperationService.GetCustomer(oneJointCustomer.Key);

                        if (thirdCustomer.customerType.key != (short)CustomerTypes.physical)
                        {
                            //Հօգուտ երրորդ անձի ավանդ կնքելու համար ընտրեք ֆիզիկական անձ
                            result.Add(new ActionError(578));
                        }
                        else
                        {
                            DateTime? BirthDate = (thirdCustomer as PhysicalCustomer).person.birthDate;

                            if (BirthDate.Value.AddYears(15) <= DateTime.Now)
                            {
                                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                                {
                                    //Երեխայի տարիքը գերազանցում է 15 տարեկանը։
                                    result.Add(new ActionError(2001));
                                }
                                else
                                {
                                    //Ավանդի ձևակերպումը հնարավոր չէ իրականացնել: Երեխայի տարիքը գերազանցում է թույլատրելի տարիքը:
                                    result.Add(new ActionError(556));
                                }

                            }
                            else
                            {
                                BirthDate = BirthDate.Value.AddYears(18);
                                while (Utility.IsWorkingDay(BirthDate.Value) != true)
                                {
                                    BirthDate = BirthDate.Value.AddDays(1);
                                }
                                if (order.Deposit.EndDate.Date != BirthDate.Value.Date)
                                {
                                    //Ավանդի վերջնաժամկետը սխալ է նշված:
                                    result.Add(new ActionError(218));
                                }
                            }
                        }
                    }
                }
            }
            else if (order.Deposit.EndDate <= order.Deposit.StartDate)
            {
                //Ժամկետը սխալ է ընտրված
                result.Add(new ActionError(218));
            }
            else if (order.DepositType != DepositType.ChildrensDeposit && !Utility.IsWorkingDay(order.Deposit.EndDate))
            {
                // Աշխատանքային օրվա ստուգում
                result.Add(new ActionError(232));
            }

            return result;
        }
        /// <summary>
        /// Ավանդի դադարեցման ստուգումներ
        /// </summary>
        ///<param name="ID">Ավանդի դադարեցման հայտի ունիկալ համար (Doc_ID)</param>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="orderNumber">Ավանդի հերթական համար</param>
        /// <returns></returns>
        internal static List<ActionError> ValidateDepositOrderTermination(DepositTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (!order.CheckDepositProducdID())
            {
                //Ավանդը Ձեզ չի պատկանում:
                result.Add(new ActionError(500));
                return result;
            }
            if (Deposit.IsSecondTermination(order.CustomerNumber, order.DepositNumber.ToString()))
            {
                result.Add(new ActionError(127));
            }
            Deposit deposit = new Deposit();
            deposit = Deposit.GetDeposit(order.ProductId, order.CustomerNumber);

            if (deposit.ClosingDate != null)
            {
                //{0} համարով գործող կարգավիճակով ավանդ գտնված չէ:
                result.Add(new ActionError(711, new string[] { deposit.DepositNumber.ToString() }));
                return result;
            }


            if (deposit.StartDate.Date == ((DateTime)order.OperationDate).Date && deposit.MainDepositNumber == 0)
            {
                //Տվյալ պայմանագիրը պետք է հեռացնել դադարեցնել հնարավոր չէ
                result.Add(new ActionError(630));
            }
            else if (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline && deposit.DayOfRateCalculation != Utility.GetCurrentOperDay())
            {
                //Տոկոսագումարների հաշվարկի օրը չի համընկնում գործառնական օրվա հետ:
                result.Add(new ActionError(641));
            }
            if (order.ClosingReasonType == 0)
            {
                //Ավանդի փակման պատճառն ընտրված չէ
                result.Add(new ActionError(968));
            }
            if (order.DepositType == DepositType.DepositGeneral)
            {
                //Ավանդը ցպահանջ է
                result.Add(new ActionError(1038));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }

        /// <summary>
        /// Վարկային գծի դադարեցման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCreditLineTerminationOrder(CreditLineTerminationOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();
            CreditLine creditLine = CreditLineDB.GetCreditLine(order.ProductId, order.CustomerNumber);

            if (creditLine != null)
            {
                //ԱրՔա - ում հաշվի ստուգում
                if (order.Source == SourceType.Bank)
                {
                    try
                    {
                        if (!ChangeExceedLimitRequest.ChekArCaBalance(order.ProductId))
                            result.Add(new ActionError(1829));
                    }
                    catch { }
                }

                if (user.filialCode == 22059 && creditLine.FillialCode == 22000)
                {
                    user.filialCode = (ushort)creditLine.FillialCode;
                }


                if (order.Source != SourceType.PhoneBanking && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking && creditLine.FillialCode != user.filialCode)
                {
                    //Այլ մասնաճյուղի վարկային գիծ հնարավոր չէ դադարեցնել:
                    result.Add(new ActionError(749));
                }
                else
                {
                    if (CreditLine.IsSecondTermination(order.CustomerNumber, order.ProductId.ToString()) == true && order.Id == 0)
                    {
                        //Տվյալ վարկային գծի համար  գոյություն ունի դադարեցման չհաստատված հայտ
                        result.Add(new ActionError(395));
                    }

                    if (creditLine.Quality == 10)
                    {

                    }

                    if (creditLine.CurrentCapital + creditLine.OutCapital + creditLine.CurrentRateValue + creditLine.PenaltyRate != 0)
                    {
                        //Քարտի վրա առկա է բացասական մնացորդ
                        result.Add(new ActionError(562));
                    }
                }

                if (Claim.CheckProductHasClaim(order.ProductId))
                {
                    //Առկա է դատական հայցադիմումի գծով պետտուրք
                    result.Add(new ActionError(1037));
                }

            }
            else
            {
                ///Վարկային գիծը բացակայում է
                result.Add(new ActionError(576));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }
        /// <summary>
        /// Տեղեկանքի ստացման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateReferenceOrder(ReferenceOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (order.Source == SourceType.Bank || order.Source == SourceType.PhoneBanking)
            {
                if (order.Source == SourceType.Bank)
                {
                    if (order.ReceiveDate != null && order.ReceiveDate.Value.Date < DateTime.Now.Date)
                    {
                        //Տեղեկանքի ստացման օրը սխալ է
                        result.Add(new ActionError(1274));
                    }
                }

                if (order.ReferenceTypes == null || order.ReferenceTypes.Count == 0)
                {
                    //Ընտրեք տեղեկանքի տեսակը
                    result.Add(new ActionError(372));
                }
                else
                {
                    if ((order.Accounts == null || order.Accounts.Count() == 0))
                    {
                        foreach (short ReferenceType in order.ReferenceTypes)
                        {
                            if (ReferenceType != 4 && ReferenceType != 8)
                            {
                                //հաշիվները ընտրված չեն կամ ընտրեք պարտք ու պահանջ չունենալու տարբերակը
                                result.Add(new ActionError(377));
                            }
                        }
                    }
                    if (order.ReferenceTypes.Contains(3))
                    {
                        if (order.DateFrom == default(DateTime))
                        {
                            //Ընտրեք շարժի սկզբնաժամկետը
                            result.Add(new ActionError(366));
                        }

                        if (order.DateTo == default(DateTime))
                        {
                            //Ընտրեք շարժի վերջնաժամկետը
                            result.Add(new ActionError(364));
                        }

                        if (order.DateFrom > order.DateTo)
                        {
                            //Վերջնաժամկետը փոքր է սկզբնաժամկետից
                            result.Add(new ActionError(368));
                        }

                        if (order.DateFrom > Utility.GetNextOperDay())
                        {
                            //Սկզբնաժամկետը մեծ է հաջորդ գործառնական օրվանից
                            result.Add(new ActionError(367));
                        }

                        //if (order.DateTo > Utility.GetNextOperDay())
                        //{
                        //    //Վերջնաժամկետը մեծ է գործառնական օրվանից
                        //    result.Add(new ActionError(370, new string[] { Utility.GetNextOperDay().ToShortDateString() }));
                        //}
                    }
                }
            }
            else
            {
                if (order.ReferenceType == 0)
                {
                    //Ընտրեք տեղեկանքի տեսակը
                    result.Add(new ActionError(372));
                }
                else
                {
                    if ((order.Accounts == null || order.Accounts.Count() == 0) && order.ReferenceType != 4)
                    {
                        //հաշիվները ընտրված չեն կամ ընտրեք պարտք ու պահանջ չունենալու տարբերակը
                        result.Add(new ActionError(377));
                    }
                    if (order.ReferenceType == 3)
                    {
                        if (order.DateFrom == default(DateTime))
                        {
                            //Ընտրեք շարժի սկզբնաժամկետը
                            result.Add(new ActionError(366));
                        }
                        if (order.DateTo == default(DateTime))
                        {
                            //Ընտրեք շարժի վերջնաժամկետը
                            result.Add(new ActionError(364));
                        }
                        if (order.DateFrom > order.DateTo)
                        {
                            //Վերջնաժամկետը փոքր է սկզբնաժամկետից
                            result.Add(new ActionError(368));
                        }

                        //if (order.DateTo > Utility.GetNextOperDay())
                        //{
                        //    //Վերջնաժամկետը մեծ է գործառնական օրվանից
                        //    result.Add(new ActionError(370, new string[] { Utility.GetNextOperDay().ToShortDateString() }));
                        //}

                        if (Convert.ToDateTime(order.DateFrom).AddDays(366) < order.DateTo)
                        {
                            //Վերջնաժամկետ-սկզբնաժամկետ>366
                            result.Add(new ActionError(369));
                        }
                    }
                }
            }
            if (order.ReferenceEmbasy == 0)
            {
                //Ընտրեք տեղեկանքը ներկայացնելու վայրը
                result.Add(new ActionError(373));
            }
            if (order.ReferenceEmbasy == 27 && order.ReferenceFor == "")
            {
                //լրացրեք տեղեկանքը ներկայացնելու վայրը
                result.Add(new ActionError(376));
            }
            if (order.ReferenceLanguage == 0)
            {
                //լեզուն ընտրված չէ:
                result.Add(new ActionError(374));
            }
            if (order.ReferenceFilial < 22000 || order.ReferenceFilial > 22300)
            {
                //ընտրեք տեղեկանքի ստացման մասնաճյուղը
                result.Add(new ActionError(375));
            }


            if (order.Fees != null && order.Fees[0].Type == 15)
            {
                result.AddRange(ValidateFeeAccount(order.CustomerNumber, order.FeeAccount));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            if (order.Source == SourceType.Bank && order.ReferenceReceiptType == 0)
            {
                //Տեղեկանքի ստացման եղանակն ընտրված չէ
                result.Add(new ActionError(1780));
            }


            return result;
        }
        internal static List<ActionError> ValidateChequeBookOrder(ChequeBookOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));
            if (order.ChequeBookAccount == null)
            {
                //ընտրեք հաշվեհամարը:
                result.Add(new ActionError(55));
            }
            else
            {
                Account mainAccount = Account.GetAccount(Convert.ToUInt64(order.ChequeBookAccount.AccountNumber));
                if (mainAccount == null)
                {
                    //ընտրեք  հաշվեհամարը:
                    result.Add(new ActionError(300));
                }

            }

            if (order.Fees[0].Type == 17)
            {
                result.AddRange(ValidateFeeAccount(order.CustomerNumber, order.FeeAccount));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }
        /// <summary>
        /// Գումարի ստացման կամ փոխանցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCashOrder(CashOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (order.CashDate == null || order.CashDate < DateTime.Now.Date)
            {
                //Ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(134));
            }
            if (order.Currency == "" || order.Currency == null)
            {
                //Ընտրեք արժույթը:
                result.Add(new ActionError(254));
            }
            if (order.SubType == 0)
            {
                //Նշեք հայտի տեսակը:
                result.Add(new ActionError(431));
            }
            if (order.CashFillial < 22000 || order.CashFillial > 22300)
            {
                //Նշեք թե որ մասնաճյուղիչ եք նախընտրում ստանալ գումարը
                result.Add(new ActionError(430));
            }
            if (order.Amount == 0)
            {
                //Գումարը մուտքագրված չէ:
                result.Add(new ActionError(20));
            }
            else
            {
                double kurs = 0;
                double minAmount = 0;
                double maxAmount = 0;
                short errMinimum = 0;
                short err1day = 0;
                short err2days = 0;
                short errMaxDays = 0;
                if (order.SubType == 1)
                {
                    kurs = Utility.GetLastCBExchangeRate(order.Currency);
                    minAmount = 10000000 / kurs;
                    maxAmount = 100000000 / kurs;
                    errMinimum = 422;
                    err1day = 424;
                    err2days = 426;
                    errMaxDays = 435;
                }
                if (order.SubType == 2)
                {
                    kurs = Utility.GetLastCBExchangeRate(order.Currency);
                    minAmount = 100000000 / kurs;
                    maxAmount = 300000000 / kurs;
                    errMinimum = 423;
                    err1day = 425;
                    err2days = 427;
                    errMaxDays = 436;
                }
                if (order.Amount <= minAmount)
                {
                    //Գումարի կանխիկ(անկանխիկ) ստացման հայտը ուղարկվում է 6.000.000(100.000.000) դրամ կամ համարժեք արտարժույթից ավել գումարի դեպքում 
                    result.Add(new ActionError(errMinimum));
                }
                if (order.Amount > minAmount && order.Amount < maxAmount && Utility.GetNextOperDay().AddDays(1).Date > order.CashDate.Date)
                {
                    //Նշված գումարի կանխիկ(անկանխիկ) փոխանցման(ստացման) համար հայտի նախորոք նորկայացման ժամկետը պետք է լինի 1 օր
                    result.Add(new ActionError(err1day));
                }
                if (order.Amount > maxAmount && Utility.GetNextOperDay().AddDays(2).Date > order.CashDate.Date)
                {
                    //Նշված գումարի կանխիկ(անկանխիկ) փոխանցման(ստացման) համար հայտի նախորոք նորկայացման ժամկետը պետք է լինի 2 օր
                    result.Add(new ActionError(err2days));
                }
                //DIANA BACEL!!!!
                if (Utility.GetNextOperDay().AddDays(9).Date < order.CashDate.Date)
                {
                    //Նշված գումարի կանխիկ(անկանխիկ) փոխանցման(ստացման) համար հայտի նախօրոք նորկայացման ժամկետը պետք է լինի 7 աշխատանքային օր:
                    result.Add(new ActionError(errMaxDays));
                }
            }
            if (!Utility.IsWorkingDay(order.CashDate))
            {
                //Ոչ աշխատանքային օր:
                result.Add(new ActionError(428));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }
        /// <summary>
        /// Քաղվածքների էլեկտրոնային հայտի ստուգումներ:
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateStatmentByEmailOrder(StatmentByEmailOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (String.IsNullOrEmpty(order.MainEmail))
            {
                //Էլ. հասցեն մուտքագրված չէ
                result.Add(new ActionError(446));
            }
            else
            {
                result.AddRange(ValidateEmail(order.MainEmail));
            }
            if (order.SecondaryEmail != "" && order.MainEmail == order.SecondaryEmail)
            {
                //Էլ. հասցենները համընկնում են:
                result.Add(new ActionError(450));
            }
            if (!String.IsNullOrEmpty(order.SecondaryEmail))
            {
                result.AddRange(ValidateEmail(order.SecondaryEmail));
            }
            if (order.Accounts == null)
            {
                //Հաշիվները ընտրված չեն:
                result.Add(new ActionError(448));
            }
            if (order.Periodicity == 0)
            {
                //Պարբերականությոնը ընտրված չէ:
                result.Add(new ActionError(449));
            }
            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;

        }

        internal static List<ActionError> ValidateSwiftCopyOrder(SwiftCopyOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (order.Fees[0].Type == 19)
            {
                result.AddRange(ValidateFeeAccount(order.CustomerNumber, order.FeeAccount));
            }
            if (!order.CheckForSwiftCopy(order.ContractNumber, order.CustomerNumber))
            {
                //Տվյալ գործարքի համարով միջազգային փոխանցում գոյություն չունի
                result.Add(new ActionError(458));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }
        internal static List<ActionError> ValidateCustomerDataOrder(CustomerDataOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            short customerType;
            List<CustomerEmail> customerEmails;
            List<CustomerPhone> customerPhone;
            List<CustomerPhone> HomePhone;
            List<CustomerPhone> MobilePhone;

            var customer = ACBAOperationService.GetCustomerMainData(order.CustomerNumber);
            customerType = (short)customer.CustomerType;

            if (customerType == (short)CustomerTypes.physical)
            {
                customerPhone = customer.Phones;
                customerEmails = customer.Emails;
                HomePhone = customerPhone.FindAll(m => m.phoneType.key == 2);
                MobilePhone = customerPhone.FindAll(m => m.phoneType.key == 1);
                if (MobilePhone.Count != 0 && string.IsNullOrEmpty(order.MobilePhoneNumber))
                {
                    //Բջջային հեռախոսահամարը մուտքագրված չէ
                    result.Add(new ActionError(464));
                }

                if (order.EmailAddress != null)
                {
                    if (customerEmails.Count != 0)
                    {
                        int count = 0;
                        {
                            for (int i = 0; i < order.EmailAddress.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(order.EmailAddress[i]))
                                {
                                    count = 1;
                                }

                            }
                        }
                        if (count == 0)
                        {
                            //Էլ.հասցեն մուտքագրված չէ:
                            result.Add(new ActionError(465));
                        }
                    }
                }
                else
                {
                    result.Add(new ActionError(465));
                }
            }
            if (order.EmailAddress != null)
            {
                for (int i = 0; i < order.EmailAddress.Count; i++)
                {
                    if (!string.IsNullOrEmpty(order.EmailAddress[i]))
                    {
                        result.AddRange(ValidateEmail(order.EmailAddress[i]));
                    }

                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;

        }
        internal static List<ActionError> ValidateTransferbyCall(TransferByCall transfer)
        {
            List<ActionError> result = new List<ActionError>();
            if (transfer.CallTime == default(DateTime))
            {
                //Լրացրեք գրանցման ա/թ:
                result.Add(new ActionError(482));
            }
            if (transfer.AccountNumber == "0" && transfer.CardNumber == "")
            {
                //Լրացրեք հաշվի համարը կամ քարտի համարը
                result.Add(new ActionError(483));
            }
            if (transfer.AccountNumber == "0")
            {
                //Լրացրեք հաշվի համարը 
                result.Add(new ActionError(484));
            }
            if (transfer.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
            }
            if (transfer.ContractID == 0)
            {
                //Լրացրեք համաձայնագրի համարը
                result.Add(new ActionError(486));
            }
            if (transfer.Code == "" || transfer.Code == null)
            {
                //Լրացրեք հսկիչ կոդը
                result.Add(new ActionError(487));
            }

            if (transfer.ContactPhone == "" || transfer.ContactPhone == null)
            {
                //Լրացրեք հեռախոսահամարը
                result.Add(new ActionError(481));
            }
            if (transfer.Amount <= 0)
            {
                //լրացրեք գումարը
                result.Add(new ActionError(488));
            }
            if (transfer.Currency == "" || transfer.Currency == null || transfer.Currency.Length != 3)
            {
                //Լրացրեք արժույթը
                result.Add(new ActionError(489));
            }

            if (!Utility.IsCorrectAmount(transfer.Amount, transfer.Currency))
            {
                //Գումարը սխալ է մուտքագրված
                result.Add(new ActionError(25));
            }

            List<Account> Accounts = Account.GetAccounts(transfer.CustomerNumber);
            if (!Accounts.Exists(m => m.AccountNumber == transfer.AccountNumber))
            {
                //{0} համարի հաշիվը բացակայում է նշած հաճախորդի գործող հաշիվների մեջ
                result.Add(new ActionError(492, new string[] { transfer.AccountNumber.ToString() }));
            }

            //Երբ փոխանցումը քարտի համարով է
            if (transfer.CardNumber != "" && transfer.CardNumber != null)
            {
                List<Card> Cards = Card.GetCards(transfer.CustomerNumber, ProductQualityFilter.NotSet);
                if (!Cards.Exists(m => m.CardNumber == transfer.CardNumber))
                {
                    //{0} համարի քարտը բացակայում է նշած հաճախորդի գործող քարտերի մեջ
                    result.Add(new ActionError(491, new string[] { transfer.CardNumber }));
                }
                if (!Cards.Exists(m => m.CardAccount.AccountNumber == transfer.AccountNumber))
                {
                    //Քարտի համարը և հաշվեհամարը չեն համապատասխանում
                    result.Add(new ActionError(490));
                }
            }

            if (transfer.TransferSystem == 0 || !transfer.CheckActiveTransferSystem())
            {
                //Լրացրեք փոխանցման համակարգը
                result.Add(new ActionError(495));
            }
            else
            {
                if (!Utility.CanPayTransferByCall(transfer.TransferSystem))
                {
                    //Հնարավոր չէ կատարել հեռախոսազանգով փոխանցում MONEYGRAM համակարգով
                    result.Add(new ActionError(1049));
                }
            }
            int minLenght = transfer.CodeMinLenght(transfer.TransferSystem);
            int maxLenght = transfer.CodeMaxLenght(transfer.TransferSystem);
            if (minLenght == maxLenght)
            {
                if (transfer.Code.Length != minLenght)
                {
                    //Գաղտնաբառը պետք է լինի {0} նիշ
                    result.Add(new ActionError(493, new string[] { minLenght.ToString() }));
                }
            }
            else
            {
                if (transfer.Code.Length < minLenght || transfer.Code.Length > maxLenght)
                {
                    //Գաղտանաբառը պետք է լինի ոչ պակաս քան {0} նիշ և ոչ ավել քան {1} նիշ
                    result.Add(new ActionError(494, new string[] { minLenght.ToString(), maxLenght.ToString() }));
                }
            }

            if (!transfer.HasContract())
            {
                //Հաճախորդը չունի կնքված համաձայնագիր
                result.Add(new ActionError(497));
            }

            if (Customer.IsCustomerUpdateExpired(transfer.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            if (transfer.CheckExistingTransfer())
            {
                //Նշված համակարգով և կոդով տվյալ օրը առկա է փոխանցում
                result.Add(new ActionError(498));
            }
            return result;

        }
        /// <summary>
        /// Էլ հասցեի ստուգում
        /// </summary>
        /// <param name="emailAdress"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateEmail(string emailAdress)
        {
            List<ActionError> result = new List<ActionError>();
            Regex regax = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (!regax.IsMatch(emailAdress))
            {
                //Էլ հասցեն սխալ է:
                result.Add(new ActionError(447));
            }
            return result;
        }

        internal static List<ActionError> ValidateMatureOrder(MatureOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (Account.CheckAccessToThisAccounts(order.Account?.AccountNumber) == 119 || (!string.IsNullOrEmpty(order.PercentAccount?.AccountNumber) && Account.CheckAccessToThisAccounts(order.PercentAccount?.AccountNumber) == 119))
            {
                //Նշված հաշվից ելքերն արգելված են
                result.Add(new ActionError(1966));
                return result;
            }

            if (order.MatureType == 0 && order.Type == OrderType.LoanMature)
            {
                //Ընտրեք վարկի մարման տեսակը:
                result.Add(new ActionError(154));
            }

            if (Loan.IsLoan_24_7(order.ProductId))
            {
                //Տվյալ կարգավիճակով վարկի համար հնարավոր չէ իրականացնել մարում
                result.Add(new ActionError(1731));
                return result;
            }

            if (order.Type == OrderType.LoanMature && order.MatureType != MatureType.ClaimRepayment)
            {
                if (Loan.CheckCutomerHasPaidInsurance(order.CustomerNumber, order.ProductId))
                {
                    //Հաճախորդը ունի Վճարված ապահովագրություն վարկ:Անհրաժեշտ է առաջնահերթ մարել նշված վարկը:
                    result.Add(new ActionError(782));
                    return result;
                }
            }

            if (order.MatureType != MatureType.ClaimRepayment && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                if (!MatureOrder.CheckLoanEquipment(order.ProductId))
                {
                    //Աճուրդից վաճառվել է տվյալ վարկի գրավի առարկա հանդիսացող գույք: 
                    //Վարկից մարում կատարելու նպատակով անհրաժեշտ է կապ հաստատել Լոգիստիկայի, 
                    //անշարժ գույքի կառավարման և սպասարկման բաժնի համապատասխան աշխատակիցների հետ:
                    result.Add(new ActionError(1439));
                }
            }
            if (order.Amount + order.PercentAmount == 0)
            {
                //Մուտքագրեք գումար:
                result.Add(new ActionError(153));
            }

            if (!Utility.IsCorrectAmount(order.Amount, order.ProductCurrency))
            {
                result.Add(new ActionError(25));
            }
            if (order.PercentAccount != null)
            {
                if (!Utility.IsCorrectAmount(order.PercentAmount, order.ProductCurrency))
                {
                    result.Add(new ActionError(937, new string[] { "Տոկոսագումար" }));
                }
            }


            if (order.PercentAmount != 0 && order.PercentAccount == null)
            {
                //Հաշիվը ընտրված չէ:
                result.Add(new ActionError(55));
            }

            if (order.Amount != 0 && order.Account == null)
            {
                //Հաշիվը ընտրված չէ:
                result.Add(new ActionError(55));
            }


            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));
            if (!order.IsProblematic)
            {
                if (order.Account != null)
                {
                    result.AddRange(ValidateDebitAccount(order, order.Account));
                }


                if (order.PercentAccount != null && !string.IsNullOrEmpty(order.PercentAccount.AccountNumber))
                {
                    result.AddRange(ValidateDebitAccount(order, order.PercentAccount));
                }
            }



            // if (order.Type == OrderType.LoanMature && order.IsProblematic && order.ProductCurrency == "EUR")
            //  {
            //     result.Add(new ActionError(696));
            //  }

            if (result.Count == 0 && order.SubType != 5 && order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                List<ActionError> checkMature = new List<ActionError>();
                checkMature = MatureOrder.CheckMature(order);
                if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking && order.Source != SourceType.ArmSoft && order.Source != SourceType.AcbaOnlineXML)
                {
                    if (order.OnlySaveAndApprove && checkMature.Contains(checkMature.Find(m => m.Code == 143)))
                    {
                        double debitAccountBalance = Order.GetSentNotConfirmedAmounts(order.Account.AccountNumber, order.IsProblematic ? OrderType.TransitPaymentOrder : OrderType.CashDebit);
                        if (order.Account.Balance + debitAccountBalance >= order.Amount)
                        {
                            checkMature.Remove(checkMature.Find(m => m.Code == 143));
                        }

                    }
                }
                result.AddRange(checkMature);

            }
            short? loanType = null;
            if (order.Type == OrderType.LoanMature && order.MatureType != MatureType.ClaimRepayment)
            {
                loanType = Loan.GetLoanType(order.ProductId);
            }


            bool isAparikTexumClaim = false;
            if (order.MatureType == MatureType.ClaimRepayment)
            {
                isAparikTexumClaim = Claim.IsAparikTexumClaim(order.ProductId);
                if (order.Amount > Tax.GetPetTurk((long)order.ProductId))
                {
                    //Մարվող գումարը մեծ է պետ. տուրքի պարտքից
                    result.Add(new ActionError(1021));
                }
            }


            if (order.MatureType != MatureType.ClaimRepayment)
            {

                if (Claim.CheckProductHasClaim(order.ProductId))
                {
                    //Առկա է դատական հայցադիմումի գծով պետտուրք
                    result.Add(new ActionError(1037));
                }

            }


            if (!order.IsProblematic && ((loanType == null && !isAparikTexumClaim) || (loanType != null && loanType != 38 && loanType != 33 && !isAparikTexumClaim)))
                result.AddRange(Validation.CheckCustomerDebts(order.CustomerNumber));


            if (order.RepaymentSourceType == 0)
            {
                //Վարկի մարման աղբյուրը ընտրված չէ:
                result.Add(new ActionError(1052));
            }

            if (order.Type == OrderType.CardCreditLineRepayment)
            {
                double cardTotalDebt = 0;
                string cardNumber = CreditLine.GetCreditLineCardNumber(order.ProductId);
                Card card = Card.GetCard(cardNumber);

                if (card != null)
                {
                    if (card.CreditLine != null)
                    {
                        cardTotalDebt = Math.Round(card.CreditLine.CurrentRateValue, 0) + Math.Round(card.CreditLine.PenaltyRate, 0) + Math.Round(card.CreditLine.JudgmentRate, 0) +
                            card.CreditLine.OutCapital - card.CreditLine.CurrentCapital;
                    }

                    double mrFee = Card.GetMRFee(card.CardNumber);

                    cardTotalDebt = cardTotalDebt + (mrFee != -1 ? mrFee : 0);

                    if (card.Overdraft != null)
                    {
                        cardTotalDebt = cardTotalDebt + Math.Round(card.Overdraft.CurrentRateValue, 0) + Math.Round(card.Overdraft.PenaltyRate, 0) + Math.Round(card.Overdraft.JudgmentRate, 0) +
                        card.Overdraft.OutCapital - card.Overdraft.CurrentCapital;
                    }
                    double cardServiceFee = Card.GetCardTotalDebt(card.CardNumber);

                    if (cardTotalDebt + cardServiceFee == 0)
                    {
                        //Հաճախորդը չունի պարտավորություններ մարելու համար
                        result.Add(new ActionError(1092));

                    }
                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            //if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking || order.Source ==
            //    SourceType.ArmSoft || order.Source == SourceType.AcbaOnlineXML)
            //{
            //    if (order.MatureMode == 1 && order.ProductType != ProductType.AparikTexum && order.ProductType != ProductType.Loan)
            //    {
            //        //Նշված տեսակի վարկերի համար հնարավոր չէ մարում կատարել online/mobile համակարգերով։
            //        result.Add(new ActionError(1821));
            //    }
            //}

            if (order.MatureType == MatureType.PartialRepayment && order.MatureMode == 1 && loanType == 49)//վճարկավ ապահովագրություն
            {
                //Տվյալ վարկի համար հնարավոր է իրականացնել միայն վարկի մայր գումարի մարում։ 
                result.Add(new ActionError(1907));
            }

            if (InfoDB.CommunicationTypeExistenceFromSAP(order.CustomerNumber) == 0 && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                //SAP CRM ծրագրում հաճախորդի հետ հաղորդակցման եղանակը ընտրված չէ։
                result.Add(new ActionError(2034));
            }


            return result;
        }

        /// <summary>
        /// Հաշվի հայտի փաստաթղթի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccountOrderDocument(AccountOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (result.Count > 0)
            {
                return result;
            }

            if (string.IsNullOrEmpty(order.Currency))
            {
                //Արժույթը ընտրված չէ
                result.Add(new ActionError(240));
            }

            if (order.AccountType == 0)
            {
                //Պատկանելիությունը ընտրված չէ
                result.Add(new ActionError(579));
            }
            //if (order.StatementDeliveryType == -1)
            //{
            //    //Քաղվածքի ստացման եղանակը ընտրված չէ
            //    result.Add(new ActionError(580));
            //}

            if (order.RestrictionGroup == 18 && order.Currency != "AMD")
            {
                //Սահմանափակ հասանելիությամբ հաշվիները կարող են բացվել լինել միայն ՀՀ դրամով
                result.Add(new ActionError(1782));
            }

            List<CustomerDocument> customerDocuments;
            VerificationData verificationData;
            List<CustomerAddress> customerAddresses;
            List<CustomerEmail> customerEmails;

            var customer = ACBAOperationService.GetCustomer(order.CustomerNumber);
            var customerType = customer.customerType.key;
            verificationData = ACBAOperationService.GetIdentityVerificationData(customer.identityId);

            if (order.RestrictionGroup == 18)
            {
                if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
                {
                    //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                    result.Add(new ActionError(496));
                }

            }

            if (customerType == (short)CustomerTypes.physical)
            {
                customerAddresses = (customer as PhysicalCustomer).person.addressList;
                customerEmails = (customer as PhysicalCustomer).person.emailList;
            }
            else
            {
                customerAddresses = (customer as LegalCustomer).Organisation.addressList;
                customerEmails = (customer as LegalCustomer).Organisation.emailList;
            }

            if (order.Currency != "AMD" && AccountDB.HaveCurrentAccountByCurrency(customer.customerNumber, "AMD") == false)
            {
                result.Add(new ActionError(737, new string[] { "դրամային" }));
            }
            customerDocuments = Customer.GetCustomerDocumentList(order.CustomerNumber);
            if (customerType == (short)CustomerTypes.physical)
            {
                if (order.RestrictionGroup != 18)
                {
                    ////Հաճախորդի ստորագրության նմուշի ստուգում:
                    result.AddRange(ValidateCustomerSignature(customer));
                }

                CustomerDocument customerDocument = new CustomerDocument();
                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                {
                    customerDocument = customerDocuments.Find(m => m.defaultSign == true);
                    if (customerDocument.validDate == null)
                    {
                        //Հաճախորդի անձնագրի վավերացման ժամկետը նշված չի:
                        result.Add(new ActionError(547));
                    }
                    else if (customerDocument.validDate < Utility.GetNextOperDay() && (customerDocument.validDate < new DateTime(2020, 3, 16).Date || DateTime.Now.Date > new DateTime(2020, 9, 13).Date))
                    {
                        //Հաճախորդի անձնագրի վավերացման ժամկետը լրացել է:
                        result.Add(new ActionError(224));
                    }
                }

                customerDocuments = (customer as PhysicalCustomer).person.documentList;
                if (!ValidateCustomerPSN(customerDocuments))
                {
                    //Հաճախորդի փաստաթղթերում <հանրային ծառայությունների համարանիշ> կամ <հանրային ծառայությունների համարանիշից հրաժարման տեղեկանք> տեսակով փաստաթուղթը բացակայում է
                    result.Add(new ActionError(585));
                }



                if (order.AccountStatus != 0)
                {
                    //Ֆիզիկական անձանց համար ժամանակավոր հաշիվ բացել չի թույլատրվում: 
                    result.Add(new ActionError(706));
                }


                if (order.RestrictionGroup == 1 && order.AccountType != 1)
                {
                    //Սնանկ ֆիզիկական անձանց դեպքում պատկանելիությունը պետք է լինի անհատական:
                    result.Add(new ActionError(1380));
                }

                if (order.RestrictionGroup == 1 && order.AccountType != 1)
                {
                    //Սնանկ ֆիզիկական անձանց դեպքում պատկանելիությունը պետք է լինի անհատական:
                    result.Add(new ActionError(50));
                }
            }
            else
            {

                if (customer.filial.key != order.user.filialCode)
                {
                    //Ոչ ֆիզիկական անձանց դեպքում հաշիվ հնարավոր է բացել միայն հաճախորդի հիմնական մասնաճյուղում,Մ/Ճ ՝{0}:
                    result.Add(new ActionError(957, new string[] { customer.filial.key.ToString() }));
                }

                if (!(customer as LegalCustomer).linkedCustomerList.Exists(item => item.linkType.key == (short)LinkPersonsTypes.manager))
                {
                    //Բացակայում է հաճախորդի տնօրենի տվյալները
                    result.Add(new ActionError(601, new string[] { customer.customerNumber.ToString() }));
                }

                if (order.AccountType != 1)
                {
                    //Ոչ ֆիզիկական անձանց դեպքում պատկանելիությունը պետք է լինի անհատական
                    result.Add(new ActionError(608));
                }

                if (order.AccountStatus != 1 && order.RestrictionGroup == 0)
                {
                    //Ոչ ֆիզիկական անձանց դեպքում հաշվի տեսակը պետք է լինի ժամանակավոր
                    result.Add(new ActionError(609));
                }


                if (order.RestrictionGroup == 1 && order.AccountStatus != 0)
                {
                    //Սնանկ ոչ ֆիզիկական անձանց դեպքում հաշվի տեսակը պետք է լինի նորմալ:
                    result.Add(new ActionError(1381));
                }


                if (order.RestrictionGroup == 1 && order.AccountType != 1)
                {
                    //Սնանկ ոչ ֆիզիկական անձանց դեպքում հաշվի պատկանելիությունը պետք է լինի անհատական:
                    result.Add(new ActionError(1383));
                }
            }

            if (order.RestrictionGroup == 1)
            {
                if (order.BankruptcyManager != null)
                {
                    short custType = ACBAOperationService.GetCustomerType(order.BankruptcyManager.Value);
                    if (custType != (short)CustomerTypes.physical)
                    {
                        result.Add(new ActionError(1389));
                    }
                }
                else
                {
                    //Սնանկության գործով կառավարիչը մուտքագրված չէ:
                    result.Add(new ActionError(1393));
                }
            }

            if (order.RestrictionGroup == 1)
            {
                if (order.BankruptcyManager != null && (order.CustomerNumber == order.BankruptcyManager))
                {
                    result.Add(new ActionError(1396));
                }
            }

            if (order.RestrictionGroup == 2 && order.Currency == "GEL")
            {
                //{0} արժույթով գործարք կատարել հնարավոր չէ:
                result.Add(new ActionError(1478, new string[] { order.Currency }));
            }


            if (verificationData == null || verificationData.VerificationResultsList.Count == 0)
            {
                //Հաճախորդի համար ահաբեկիչների հետ ստուգումը բացակայում է, անհրաժեշտ է կրկին հաստատել հաճախորդին
                result.Add(new ActionError(581));
            }
            else
            {
                VerificationResult verificationResult = verificationData.VerificationResultsList.Find(m => m.id != 0);
                if (verificationResult.verifyResult.key == 2)
                {
                    //Հաճախորդը նշված է որպես կասկածելի
                    result.Add(new ActionError(582));
                }
            }
            if (order.RestrictionGroup != 18)
            {
                if (customer.quality.key != 1 && customer.quality.key != 57)
                {
                    //Հաշիվը կարելի է բացել միայն <Հաճախորդ> կարգավիճակով անձանց համար
                    result.Add(new ActionError(583));
                }

            }

            //if (ValidationDB.IsDAHKAvailability(order.CustomerNumber))
            //{
            //    Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ
            //    result.Add(new ActionError(516, new string[] { order.CustomerNumber.ToString() }));
            //}

            if (order.Source != SourceType.Bank && order.Source != SourceType.PhoneBanking && order.RestrictionGroup != 18)
            {
                if (Account.HasCurrentAccountsNumber(order.CustomerNumber, order.Currency))
                    //Նշված արժույթով ընթացիկ հաշիվ արդեն առկա է: Նույն արժույթով ևս մեկ ընթացիկ հաշիվ բացելու համար անհրաժեշտ է մոտենալ Ձեզ սպասարկող Բանկի մասնաճյուղ:
                    result.Add(new ActionError(241));

                if (AccountDB.HasAccountOrder(order.Currency, order.CustomerNumber))
                    // Նշված արժույթով ընթացիկ հաշիվ արդեն առկա է: Նույն արժույթով ևս մեկ ընթացիկ հաշիվ բացելու համար անհրաժեշտ է մոտենալ Բանկի Ձեզ սպասարկող մասնաճյուղ:
                    result.Add(new ActionError(1816));
            }

            if (!customerAddresses.Exists(item => item.addressType.key == 2))
            {
                //Բացակայում է հաճախորդի հասցեն
                result.Add(new ActionError(602, new string[] { customer.customerNumber.ToString() }));
            }

            if (order.StatementDeliveryType == 1 || order.StatementDeliveryType == 3 || order.StatementDeliveryType == 4)
            {
                if (!customerEmails.Exists(item => item.emailType.key == 5))
                {
                    //Հաճախորդի Էլեկտրոնային հասցեն բացակայում է
                    result.Add(new ActionError(606));
                }
            }
            if (order.Source == SourceType.Bank && order.RestrictionGroup == 0)
            {
                result.AddRange(ValidateKYCDocument(order.CustomerNumber));

            }

            if (result.Count > 0)
            {
                return result;
            }

            if (order.JointCustomers != null)
            {
                if (order.JointCustomers.Count > 2 && order.AccountType == 2)
                {
                    //3 և ավելի հաճախորդների համար համատեղ հաշվի բացում նախատեսված չէ
                    result.Add(new ActionError(604));
                }

                if (order.JointCustomers.Count > 1 && order.AccountType == 3)
                {
                    //2 և ավելի հաճախորդների համար հօգուտ երրորդ անձի հաշվի բացում նախատեսված չէ
                    result.Add(new ActionError(631));
                }

                var jointResult = order.JointCustomers.GroupBy(x => new { x.Key }).Where(item => item.Count() > 1).ToDictionary(g => g.Key, g => g.Count());
                if (jointResult.Count != 0 && order.AccountType == 2)
                {
                    //Համատեղ հաշվի հաճախորդը սխալ է ընտրված
                    result.Add(new ActionError(504));
                }

                if (jointResult.Count != 0 && order.AccountType == 3)
                {
                    //Հօգուտ երրորդ անձի հաշվի հաճախորդը սխալ է ընտրված
                    result.Add(new ActionError(632));
                }

                if (result.Count > 0)
                {
                    return result;
                }

                //Ստուգում է համատեղ և հօգուտ երրորդ անձի հաճախորդների տվյալները:
                order.JointCustomers.ForEach(m =>
                {
                    if (m.Key != 0)
                    {
                        if ((m.Key.ToString()).Length != 12 || order.CustomerNumber == m.Key)
                        {
                            if (order.AccountType == 2)
                            {
                                //Համատեղ հաշվի հաճախորդը սխալ է ընտրված
                                result.Add(new ActionError(504));
                            }

                            if (order.AccountType == 3)
                            {
                                //Հօգուտ երրորդ անձի հաշվի հաճախորդը սխալ է ընտրված
                                result.Add(new ActionError(632));
                            }
                        }

                        if (result.Count == 0)
                        {
                            var jointCustomer = ACBAOperationService.GetCustomer(m.Key);
                            short jointCustomerType = jointCustomer.customerType.key;

                            if (customer.residence.key == 1 && jointCustomer.residence.key == 2 && order.AccountType == 2)
                            {
                                //Հաշիվը բացեք հիմնական հաճախորդ ընտրելով ոչ ռեզիդենտ հաճախորդին
                                result.Add(new ActionError(611));
                            }

                            if (jointCustomerType != (short)CustomerTypes.physical)
                            {
                                if (order.AccountType == 2)
                                {
                                    //Համատեղ հաշիվ բացելու համար ընտրեք ֆիզիկական անձ
                                    result.Add(new ActionError(603));
                                }

                                if (order.AccountType == 3)
                                {
                                    //Հօգուտ երրորդ անձի հաշիվ բացելու համար ընտրեք ֆիզիկական անձ
                                    result.Add(new ActionError(633));
                                }

                            }
                            else if (jointCustomerType == (short)CustomerTypes.physical)
                            {
                                customerAddresses = (jointCustomer as PhysicalCustomer).person.addressList;
                                if (!customerAddresses.Exists(item => item.addressType.key == 2))
                                {
                                    //Բացակայում է հաճախորդի հասցեն
                                    result.Add(new ActionError(602, new string[] { jointCustomer.customerNumber.ToString() }));
                                }

                                if (order.AccountType == 3)
                                {
                                    DateTime? BirthDate = (jointCustomer as PhysicalCustomer).person.birthDate;
                                    BirthDate = BirthDate.Value.AddYears(18);
                                    if (BirthDate.Value < DateTime.Now)
                                    {
                                        //Հօգուտ երրորդ անձի հաշիվ բացելու համար ընտրեք 18 տարին չլրացած հաճախորդ
                                        result.Add(new ActionError(635));
                                    }
                                }
                            }

                            if (jointCustomer.quality.key != 1)
                            {
                                if (order.AccountType == 2)
                                {
                                    //Համատեղ հաշիվը կարելի է բացել միայն <Հաճախորդ> կարգավիճակով անձանց համար
                                    result.Add(new ActionError(605));
                                }

                                if (order.AccountType == 3 && (jointCustomer.quality.key == 57 || jointCustomer.quality.key == 43))
                                {
                                    //Հօգուտ երրորդ անձի հաշիվը կարելի է բացել միայն <Հաճախորդ> կարգավիճակով անձանց համար
                                    result.Add(new ActionError(634));
                                }
                            }
                            else
                            {
                                if (order.AccountType == 2)
                                {
                                    //Հաճախորդի ստորագրության նմուշի ստուգում:
                                    result.AddRange(ValidateCustomerSignature(jointCustomer));
                                    if (order.Source == SourceType.Bank)
                                    {
                                        result.AddRange(ValidateKYCDocument(m.Key));
                                    }
                                }
                            }
                        }
                    }
                });
            }
            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
            {
                List<Account> accounts = Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.All);

                if (order.Type == OrderType.ThirdPersonDeposit)
                {
                    List<KeyValuePair<ulong, double>> jointCustomers = new List<KeyValuePair<ulong, double>>();
                    List<Account> openedAMDAccount, closedAccounts;
                    Boolean existsOpenedAMDAccount = false;
                    Boolean existsClosedAccounts = false;

                    IEnumerable<Account> queryOpenedAMDAccount = (from account in accounts
                                                                  where account.AccountType == 3 && account.Currency == "AMD" && account.ClosingDate == null
                                                                  select account).ToList();
                    openedAMDAccount = (List<Account>)queryOpenedAMDAccount;

                    openedAMDAccount.ForEach(m =>
                    {
                        jointCustomers = Account.GetAccountJointCustomers(m.AccountNumber);
                        existsOpenedAMDAccount = jointCustomers.Exists(j => order.JointCustomers.Exists(k => k.Key == j.Key));
                        if (existsOpenedAMDAccount)
                            return;
                    });

                    if (!existsOpenedAMDAccount)
                    {
                        //Տվյալ արժույթով հօգուտ երրորդ անձի ընթացիկ հաշիվ բացելու համար պետք է առկա լինի ՀՕԵ ընթացիկ հաշիվ:
                        result.Add(new ActionError(355));
                    }

                    IEnumerable<Account> queryClosedAccounts = (from account in accounts
                                                                where account.AccountType == 3 && account.Currency == order.Currency && account.ClosingDate != null
                                                                select account).ToList();
                    closedAccounts = (List<Account>)queryClosedAccounts;

                    closedAccounts.ForEach(m =>
                    {
                        jointCustomers = Account.GetAccountJointCustomers(m.AccountNumber);
                        existsClosedAccounts = jointCustomers.Exists(j => order.JointCustomers.Exists(k => k.Key == j.Key));
                        if (existsClosedAccounts)
                            return;
                    });

                    if (!existsClosedAccounts)
                    {
                        //Նշված արժույթով առկա է փակված հօգուտ երրորդ անձի ընթացիկ հաշիվ: Խնդրում ենք վերաբացել այն:
                        result.Add(new ActionError(354));
                    }

                    if (order.JointCustomers == null)
                    {
                        //Ընտրեք երրորդ անձին
                        result.Add(new ActionError(391));
                    }
                }

                if (order.Type == OrderType.CurrentAccountOpen && order.RestrictionGroup != 18)
                {
                    IEnumerable<Account> queryClosedAccounts = (from account in accounts
                                                                where account.Currency == order.Currency && account.ClosingDate != null
                                                                select account).ToList();
                    List<Account> closedAccounts = (List<Account>)queryClosedAccounts;
                    if (closedAccounts.Exists(m => m.Currency == order.Currency))
                    {
                        //Նշված արժույթով առկա է փակված ընթացիկ հաշիվ: Խնդրում ենք վերաբացել այն:
                        result.Add(new ActionError(242));
                    }
                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            if (InfoDB.CommunicationTypeExistenceFromSAP(order.CustomerNumber) == 0 && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                //SAP CRM ծրագրում հաճախորդի հետ հաղորդակցման եղանակը ընտրված չէ։
                result.Add(new ActionError(2034));
            }

            return result;
        }

        internal static bool ValidateRestrictionAccountOrder(AccountOrder order)
        {
            bool IsValid = true;
            List<CustomerDocument> customerDocuments = Customer.GetCustomerDocumentList(order.CustomerNumber);
            //Ժամկետը լրաված անձնագիր 
            //Անձնագրի վերջը չկա
            CustomerDocument customerDocument = customerDocuments.Find(m => m.defaultSign == true);
            if (customerDocument.validDate == null)
            {
                return IsValid = false;
            }
            else if (customerDocument.validDate < Utility.GetNextOperDay() && (customerDocument.validDate < new DateTime(2020, 3, 16).Date || DateTime.Now.Date > new DateTime(2020, 9, 13).Date))
            {
                return IsValid = false;
            }
            //ՀԾՀ-ն բացակայում է
            if (!ValidateCustomerPSN(customerDocuments))
            {
                return IsValid = false;
            }

            var customer = ACBAOperationService.GetCustomerMainData(order.CustomerNumber);

            //Հասցեն բացակայում է
            List<CustomerAddress> customerAddresses = customer.Addresses;
            if (!customerAddresses.Exists(item => item.addressType.key == 2))
            {
                return IsValid = false;
            }

            return IsValid;
        }

        /// <summary>
        /// Աշխատանքային և ոչ աշխատաքային օրեր
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="productType">Պրոդուկտի տեսակ</param>
        /// <returns></returns>
        internal static bool CheckProductAvailabilityByCustomerCountry(ulong customerNumber, int productType)
        {
            int result = ValidationDB.CheckProductAvailabilityByCustomerCountry(customerNumber, productType);

            if (result == 1)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Հաշիվների փակման ստուգումներ
        /// </summary>        
        internal static List<ActionError> ValidateAccountClosingOrder(AccountClosingOrder order, bool calledFromCardClosingOrder = false)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.ClosingAccounts != null)
            {
                order.ClosingAccounts.ForEach(m =>
                {

                    Account account = Account.GetAccount(m.AccountNumber);

                    if (AccountClosingOrder.IsSecondClosing(order.CustomerNumber, account.AccountNumber, order.Source) == true && order.Id == 0)
                    {
                        //Տվյալ հաշվի համար գոյություն ունի փակման չհաստատված հայտ
                        result.Add(new ActionError(505, new string[] { account.AccountNumber }));
                    }

                    else if (account.ClosingDate != null)
                    {
                        //Հաշիվը արդեն փակված է
                        result.Add(new ActionError(506, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveActiveProduct())
                    {
                        //Հաշիվը ներառված է գործող պրոդուկների հաշիվների ցանկում
                        result.Add(new ActionError(507, new string[] { account.AccountNumber }));
                    }
                    else if (account.HaveActiveDepositForCurrentAccount())
                    {
                        //Գոյություն ունի գործող ավանդ, ընթացիկ հաշիվը չի թույլատրվում փակել
                        result.Add(new ActionError(508, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveActiveLoanForCurrentAccount())
                    {
                        //Գոյություն ունի գործող վարկ, ընթացիկ հաշիվը չի թույլատրվում փակել
                        result.Add(new ActionError(509, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveActiveCreditLineForCurrentAccount())
                    {
                        //Գոյություն ունի գործող վարկային գիծ, ընթացիկ հաշիվը չի թույլատրվում փակել
                        result.Add(new ActionError(510, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveActiveSocPackageCreditLineForCurrentAccount())
                    {
                        //Գոյություն ունի սոց. փաթեթի վարկային գիծ, ընթացիկ հաշիվը չի թույլատրվում փակել
                        result.Add(new ActionError(511, new string[] { account.AccountNumber }));
                    }
                    else if (account.HaveActiveOperationByPeriodForCurrentAccount())
                    {
                        //Հաշիվը ներառված է պարբերական փոխանցման հանձնարարականում
                        result.Add(new ActionError(512, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveHBForCurrentAccount())
                    {
                        //Հաշվի փակումը հնարավոր չէ, քանի որ հաճախորդը օգտվում է ACBA ON-LINE համակարգից
                        result.Add(new ActionError(513, new string[] { account.AccountNumber }));
                    }
                    else if (!calledFromCardClosingOrder && account.HaveCurrencyCardsForCurrentAccount())
                    {
                        //Հաշվի փակումը հնարավոր չէ, քանի որ հաճախորդը ունի արժույթային քարտեր
                        result.Add(new ActionError(514, new string[] { account.AccountNumber }));
                    }
                    else if (Account.GetAccountBalance(account.AccountNumber) != 0)
                    {
                        if (order.OnlySaveAndApprove)
                        {
                            if (Account.GetAccountBalance(account.AccountNumber) - Order.GetSentNotConfirmedWithdrawalAmount(account.AccountNumber) != 0)
                            {
                                //Հաշիվը մնացորդ ունի
                                result.Add(new ActionError(515, new string[] { account.AccountNumber }));
                            }
                        }
                        else
                        {
                            //Հաշիվը մնացորդ ունի
                            result.Add(new ActionError(515, new string[] { account.AccountNumber }));
                        }
                    }
                    else if (account.IsDAHKAvailability())
                    {
                        //Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ
                        result.Add(new ActionError(516, new string[] { account.AccountNumber }));
                    }
                    else if (account.HaveTaxInspectorateApproval())
                    {
                        //Հաշվի համար գոյություն ունի հարկային տեսչության կողմից հաստատման ենթակա հայտ
                        result.Add(new ActionError(517, new string[] { account.AccountNumber }));
                    }
                    else if (account.Currency == "AMD" && account.AccountType == 10 && (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking))
                    {
                        List<Account> currentAccList = Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened).Where(x => x.AccountType == 10).ToList();
                        if (currentAccList.Where(x => x.Currency == "AMD").Count() == 1 && currentAccList.Where(x => x.Currency != "AMD").Count() > 0)
                        {
                            //Հաշիվը հնարավոր չէ փակել:Հաշիվը փակելու համար անհրաժեշտ է փակել արտարժութային ընթացիկ հաշիվ(ներ)ը:
                            result.Add(new ActionError(1892));
                        }
                    }
                });
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }

        /// <summary>
        /// Քարտի փակման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardClosingOrderDocument(CardClosingOrder order, ushort filialCode)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.ClosingReason == 0)
            {
                //Քարտի փակման պատճառն ընտրված չէ
                result.Add(new ActionError(536));
                return result;
            }

            if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {

                result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
                result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

                Card card = Card.GetCard(order.ProductId, order.CustomerNumber);


                if (card == null)
                {
                    //Քարտը գտնված չէ
                    result.Add(new ActionError(534));
                    return result;
                }

                if (card.ClosingDate != null)
                {
                    //Ընտրված է փակված քարտ
                    result.Add(new ActionError(532));
                    return result;
                }

                if (card.FilialCode == 22000 && filialCode == 22059)
                {
                    filialCode = (ushort)card.FilialCode;
                }

                if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
                {
                    if (card.FilialCode != filialCode && order.user.userPermissionId != 505)
                    {
                        //Այլ մասնաճյուղի քարտ փակել հնարավոր չէ
                        result.Add(new ActionError(533));
                        return result;
                    }
                }


                if (CardClosingOrder.CheckCardPensionApplication(card.CardNumber))
                {
                    //Գոյություն ունի կենսաթոշակի հայտարարագիր
                    result.Add(new ActionError(791));
                    return result;
                }


                if (card.CreditLine != null)
                {
                    //Քարտը ունի վարկային գիծ
                    result.Add(new ActionError(521));
                    return result;
                }

                if (card.MainCardNumber == "")
                {
                    //Հաշիվը ներառված է պարբերական փոխանցման հանձնարարականում
                    result.AddRange(CardClosingOrder.CheckCardPeriodicTransfer(card.CardAccount.AccountNumber));
                    if (result.Count > 0)
                        return result;
                }

                //Քարտն ունի չձևակերպված գործարքներ
                result.AddRange(CardClosingOrder.CheckCardTransactions(card.CardNumber, card.Type));
                if (result.Count > 0)
                    return result;


                result.AddRange(CardClosingOrder.CheckCardClosingReason(order.ProductId, order.ClosingReason));
                if (result.Count > 0)
                    return result;

                List<Card> linkedCards = Card.GetLinkedCards(card.CardNumber);
                if (linkedCards.Count > 0)
                {
                    string linkedCardNumbers = "";
                    foreach (Card linkedCard in linkedCards)
                    {
                        linkedCardNumbers += linkedCard.CardNumber + ",";
                    }
                    //Քարտը հիմնական է հանդիսանում @var1 քարտի(երի) համար
                    result.Add(new ActionError(526, new string[] { linkedCardNumbers }));
                    return result;
                }
                if (Account.GetAccountBalance(card.OverdraftAccount.AccountNumber) != 0)
                {
                    //Քարտի օվերդրաֆտի հաշիվը մնացորդ ունի
                    result.Add(new ActionError(527));
                    return result;
                }


                if (card.PositiveRate > 0)
                {
                    //Քարտին առկա է կուտակված ցպահանջ տոկոսագումար
                    result.Add(new ActionError(529));
                    return result;
                }

                if (card.Overdraft != null)
                {
                    if (card.Overdraft.CurrentRateValue != 0 || card.Overdraft.InpaiedRestOfRate != 0 ||
                        card.Overdraft.PenaltyRate != 0 || card.Overdraft.OutPenalty != 0 || card.Overdraft.OverdueCapital != 0 || card.Overdraft.OutCapital != 0)
                    {
                        //Քարտը ունի չմարված տոկոսագումար կամ տուգանք
                        result.Add(new ActionError(530));
                    }
                }
                if (Card.GetCardTotalDebt(card.CardNumber) != 0 && card.Currency != "AMD")
                {
                    List<Account> account = new List<Account>();
                    account.AddRange(Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened).FindAll(m => m.Currency == "AMD"));
                    if (account.Count == 0)
                    {
                        //Հաճախորդը չունի ընթացիկ դրամային հաշիվ
                        result.Add(new ActionError(531));
                        return result;
                    }
                }

                if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
                {
                    //Նշված խումբը գոյություն չունի։
                    result.Add(new ActionError(1628));
                }

                if (order.CloseCardAccount) // Քարտային հաշվի փակման ստուգումներ
                {
                    Account account = card.CardAccount;

                    if (!String.IsNullOrEmpty(card.MainCardNumber))
                    {
                        //Տվյալ տեսակի քարտի համար հնարավոր չէ իրականացնել քարտային հաշվի փակում
                        result.Add(new ActionError(1828));
                        return result;
                    }

                    if (account.DAHKRestrictionForCardAccount())
                    {
                        //Տվյալ քարտային հաշվի համար առկա են գործող ԴԱՀԿ արգելադրումներ
                        result.Add(new ActionError(1830));
                        return result;
                    }

                    AccountClosingOrder accountClosingOrder = new AccountClosingOrder();
                    accountClosingOrder.ClosingAccounts = new List<Account>();
                    accountClosingOrder.Id = 0;
                    accountClosingOrder.GroupId = 0;
                    accountClosingOrder.ClosingAccounts.Add(account);
                    accountClosingOrder.CustomerNumber = order.CustomerNumber;

                    result.AddRange(ValidateAccountClosingOrder(accountClosingOrder, true));
                }
            }

            return result;
        }


        /// <summary>
        /// Հաշվի վերաբացման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccountReOpenOrder(AccountReOpenOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(order.ReopenReasonDescription))
            {
                //Վերաբացման պատճառի նկարագրությունը մւտքագրված չէ:
                result.Add(new ActionError(620));
            }

            if (order.FeeChargeType < 0)
            {
                //Միջնորդավճարի գանձման եղանակը մուտքագրված չէ:
                result.Add(new ActionError(557));
            }

            if (order.FeeChargeType == 2)
            {
                result.AddRange(Validation.CheckCustomerDebtsAndDahk(order.CustomerNumber, order.Fees.First().Account));

            }
            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (result.Count > 0)
            {
                return result;
            }

            order.ReOpeningAccounts.ForEach(m =>
            {
                if (AccountReOpenOrder.IsSecondReOpen(order.CustomerNumber, m.AccountNumber) == true && order.Id == 0)
                {
                    //Տվյալ հաշվի համար գոյություն ունի վերաբացման չհաստատված հայտ
                    result.Add(new ActionError(558, new string[] { m.AccountNumber }));
                }
                else if (m.ClosingDate == null)
                {
                    //Հաշիվը արդեն բաց է:
                    result.Add(new ActionError(241, new string[] { m.AccountNumber }));
                }
                else if (m.HaveTaxInspectorateApproval())
                {
                    //Հաշվի համար գոյություն ունի հարկային տեսչության կողմից հաստատման ենթակա հայտ
                    result.Add(new ActionError(517, new string[] { m.AccountNumber }));
                }
                else if (!order.CanReOpenAccount(m))
                {
                    //Հաշվի բացումը հնարավոր չէ:Հաշվի սինթետիկ հաշիվը չի համապատասխանում հաճախորդի կարգավիճակին:
                    result.Add(new ActionError(560, new string[] { m.AccountNumber }));
                }

            });

            if (order.FeeChargeType == 2)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened);
                if (!currentAccounts.Exists(m => m.Currency == "AMD"))
                {
                    //Ընթացիկ AMD հաշիվ գտնված չէ, ընտրեք գանձման ուրիշ եղանակ:
                    result.Add(new ActionError(561));
                }


            }

            if (result.Count > 0)
            {
                return result;
            }
            else
            {
                //Եթե վերաբացվում է ԴԱՀԿ հաշիվ , ստուգում արդյոք հաճախորդը ունի ԴԱՀԿ արգելանք
                if (order.ReOpeningAccounts.Exists(m => m.AccountType == 61) && !ValidationDB.IsDAHKAvailability(order.CustomerNumber))
                {
                    order.ReOpeningAccounts.ForEach(m =>
                    {
                        if (m.AccountType == 61)
                        {
                            // Տվյալ տեսակի հաշվի վերաբացումը հնարավոր է միայն ԴԱՀԿ արգելանքի տակ գտնվող հաճախորդների համար:
                            result.Add(new ActionError(619, new string[] { m.AccountNumber }));
                        }

                    });
                }
                else
                {
                    //Հաճախորդի ստորագրության նմուշի ստուգում:
                    result.AddRange(ValidateCustomerSignature(order.CustomerNumber));
                }
            }

            if (order.Source == SourceType.Bank)
            {                
                result.AddRange(ValidateKYCDocument(order.CustomerNumber));
            }

            if (CheckCustomerPhoneNumber(order.CustomerNumber))
            {
                //Հաճախորդի համար չկա մուտքագրված հեռախոսահամար:
                result.Add(new ActionError(1904));
            }

            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                if (order.Currency != "AMD" && AccountDB.HaveCurrentAccountByCurrency(order.CustomerNumber, "AMD") == false)
                    result.Add(new ActionError(243));

            return result;
        }

        /// <summary>
        /// Գործարքի դեպքում կրեդիտ և դեբետ հաշիվների ստուգում
        /// </summary>
        /// <param name="debitAccountNumber"></param>
        /// <param name="creditAccountNumber"></param>
        /// <param name="permissionID"></param>
        /// <returns></returns>
        internal static ActionError CheckAccountOperation(string debitAccountNumber, string creditAccountNumber, int permissionID, double operationAmount)
        {
            return ValidationDB.CheckAccountOperation(debitAccountNumber, creditAccountNumber, permissionID, operationAmount);
        }

        public static List<ActionError> ValidateCustomerDocument(ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();
            List<CustomerDocument> customerDocuments;
            string customerOrManager = "";
            short customerType;

            var customer = ACBAOperationService.GetCustomer(customerNumber);
            customerType = customer.customerType.key;

            if (customer.identityId != 0)
            {
                if (customerType == (short)CustomerTypes.physical)
                {
                    customerDocuments = (customer as PhysicalCustomer).person.documentList;
                    customerOrManager = customerNumber.ToString();
                }
                else if (customerType == (short)CustomerTypes.physCustomerUndertakings)
                {
                    LinkedCustomer lnkManager = (customer as LegalCustomer).linkedCustomerList.Find(item => item.linkType.key == (short)LinkPersonsTypes.manager);

                    var customerManager = ACBAOperationService.GetCustomer(lnkManager.linkedCustomerNumber);
                    customerOrManager = customerManager.customerNumber.ToString();

                    customerDocuments = (customerManager as PhysicalCustomer).person.documentList;
                }
                else
                {
                    return result;
                }

                if (customerDocuments.FindAll(m => m.defaultSign).Count > 0)
                {
                    CustomerDocument defaultDocument = customerDocuments.Find(m => m.defaultSign);
                    if (defaultDocument.validDate == null)
                    {
                        //Հաճախորդի անձնագրի վավերացման ժամկետը նշված չի:
                        result.Add(new ActionError(767, new string[] { customerOrManager }));
                    }
                    else if (defaultDocument.validDate < Utility.GetNextOperDay())
                    {
                        //Հաճախորդի անձնագրի վավերացման ժամկետը լրացել է:
                        result.Add(new ActionError(768, new string[] { customerOrManager }));
                    }
                    else if (((DateTime)defaultDocument.validDate - Utility.GetNextOperDay()).TotalDays < 30)
                    {
                        //Հաճախորդի անձնագրի վավերացման ժամկետի լրացմանը մնացել է մինչև 30 օր:
                        result.Add(new ActionError(769, new string[] { customerOrManager }));
                    }
                }
            }


            return result;
        }


        /// <summary>
        /// Փոխանակման փոխարժեքի արժեքի ստուգում
        /// </summary>
        /// <param name="paymentOrder"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateRate(PaymentOrder paymentOrder)
        {
            List<ActionError> result = new List<ActionError>();
            Double rate = 0, rateTo, rateFrom;
            String operationCurrency;

            if (paymentOrder.DebitAccount.Currency == "AMD" || paymentOrder.ReceiverAccount.Currency == "AMD")
            {
                RateType rateType = new RateType();

                if (paymentOrder.Type == OrderType.Convertation || paymentOrder.Type == OrderType.InternationalTransfer)
                {
                    rateType = RateType.NonCash;
                }

                if (paymentOrder.Source == SourceType.SSTerminal && paymentOrder.DebitAccount.Currency != paymentOrder.ReceiverAccount.Currency)
                {
                    if (paymentOrder.DebitAccount.Currency != "AMD")
                    {
                        if (paymentOrder.Type == OrderType.CashConvertation || paymentOrder.Type == OrderType.CashDebitConvertation)
                        {
                            rateType = RateType.Cash;
                        }
                        else
                        {
                            rateType = RateType.NonCash;
                        }
                    }
                    else
                    {
                        if (paymentOrder.Type == OrderType.CashConvertation || paymentOrder.Type == OrderType.CashCreditConvertation)
                        {
                            rateType = RateType.Cash;
                        }
                        else
                        {
                            rateType = RateType.NonCash;
                        }
                    }
                }

                if (paymentOrder.Source == SourceType.STAK && paymentOrder.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                {
                    rateType = RateType.NonCash;
                }

                if (paymentOrder.DebitAccount.Currency != "AMD")
                {
                    rate = Utility.GetLastExchangeRate(paymentOrder.DebitAccount.Currency, rateType, ExchangeDirection.Buy);
                    rateFrom = rate;
                    rateTo = Utility.GetLastExchangeRate(paymentOrder.DebitAccount.Currency, rateType, ExchangeDirection.Sell);
                    operationCurrency = paymentOrder.DebitAccount.Currency;
                }
                else
                {
                    rate = Utility.GetLastExchangeRate(paymentOrder.ReceiverAccount.Currency, rateType, ExchangeDirection.Sell);
                    rateTo = rate;
                    rateFrom = Utility.GetLastExchangeRate(paymentOrder.ReceiverAccount.Currency, rateType, ExchangeDirection.Buy);
                    operationCurrency = paymentOrder.ReceiverAccount.Currency;
                }

                if (rate != paymentOrder.ConvertationRate && paymentOrder.Quality != OrderQuality.Removed)
                {
                    paymentOrder.ForDillingApprovemnt = true;

                    if (paymentOrder.Source != SourceType.Bank)
                    {
                        if ((operationCurrency == "CHF" && paymentOrder.Amount >= 1000) || (operationCurrency == "GBP" && paymentOrder.Amount >= 1000) || (operationCurrency == "RUR" && paymentOrder.Amount >= 50000) || (operationCurrency == "USD" && paymentOrder.Amount >= 5000) || (operationCurrency == "EUR" && paymentOrder.Amount >= 5000) || (operationCurrency == "GEL" && paymentOrder.Amount >= 1000))
                        {
                            if (paymentOrder.ConvertationRate > rateTo || paymentOrder.ConvertationRate < rateFrom)
                            {
                                //Մուտքագրված է սխալ փոխարժեք:
                                result.Add(new ActionError(57));
                                return result;
                            }
                        }
                        else
                        {
                            //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                            result.Add(new ActionError(121));
                        }
                    }
                    else
                    {
                        if (paymentOrder.ConvertationRate > rateTo || paymentOrder.ConvertationRate < rateFrom)
                        {
                            //Մուտքագրված է սխալ փոխարժեք:
                            result.Add(new ActionError(57));
                            return result;
                        }
                    }

                }
            }

            else
            {
                Double minRate = 0, maxRate = 0;
                String debitCurreny = paymentOrder.DebitAccount.Currency;
                String receiverCurrency = paymentOrder.ReceiverAccount.Currency;
                Double currentCrossRate = 0;

                if ((debitCurreny == "EUR" && receiverCurrency == "USD") || (debitCurreny == "USD" && receiverCurrency == "EUR"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Buy), 6);
                    if (debitCurreny == "EUR" && receiverCurrency == "USD")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }
                else if ((debitCurreny == "USD" && receiverCurrency == "RUR") || (debitCurreny == "RUR" && receiverCurrency == "USD"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("RUR", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("RUR", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "USD" && receiverCurrency == "RUR")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }
                else if ((debitCurreny == "EUR" && receiverCurrency == "RUR") || (debitCurreny == "RUR" && receiverCurrency == "EUR"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("RUR", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("RUR", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "EUR" && receiverCurrency == "RUR")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }
                else if ((debitCurreny == "GBP" && receiverCurrency == "USD") || (debitCurreny == "USD" && receiverCurrency == "GBP"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("GBP", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("GBP", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "GBP" && receiverCurrency == "USD")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }
                else if ((debitCurreny == "EUR" && receiverCurrency == "GBP") || (debitCurreny == "GBP" && receiverCurrency == "EUR"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("GBP", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("GBP", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "GBP" && receiverCurrency == "EUR")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }

                else if ((debitCurreny == "USD" && receiverCurrency == "CHF") || (debitCurreny == "CHF" && receiverCurrency == "USD"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("CHF", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("USD", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("CHF", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "USD" && receiverCurrency == "CHF")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }
                else if ((debitCurreny == "EUR" && receiverCurrency == "CHF") || (debitCurreny == "CHF" && receiverCurrency == "EUR"))
                {
                    minRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Buy) / Utility.GetLastExchangeRate("CHF", RateType.Cross, ExchangeDirection.Sell), 6);
                    maxRate = Math.Round(Utility.GetLastExchangeRate("EUR", RateType.Cross, ExchangeDirection.Sell) / Utility.GetLastExchangeRate("CHF", RateType.Cross, ExchangeDirection.Buy), 6);

                    if (debitCurreny == "EUR" && receiverCurrency == "CHF")
                    {
                        currentCrossRate = minRate;
                    }
                    else
                    {
                        currentCrossRate = maxRate;
                    }
                }


                if (debitCurreny == "EUR")
                    rate = Math.Round(paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1, 6);
                else if (receiverCurrency == "EUR")
                    rate = Math.Round(paymentOrder.ConvertationRate1 / paymentOrder.ConvertationRate, 6);
                else if (debitCurreny == "USD")
                {
                    if (receiverCurrency == "RUR")
                        rate = Math.Round(paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1, 6);
                    else if (receiverCurrency == "GBP")
                        rate = Math.Round(paymentOrder.ConvertationRate1 / paymentOrder.ConvertationRate, 6);
                    else if (receiverCurrency == "CHF")
                        rate = Math.Round(paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1, 6);
                }
                else if (receiverCurrency == "USD")
                {
                    if (debitCurreny == "RUR")
                        rate = Math.Round(paymentOrder.ConvertationRate1 / paymentOrder.ConvertationRate, 6);
                    else if (debitCurreny == "GBP")
                        rate = Math.Round(paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1, 6);
                    else if (debitCurreny == "CHF")
                        rate = Math.Round(paymentOrder.ConvertationRate1 / paymentOrder.ConvertationRate, 6);
                }



                if (rate > maxRate || rate < minRate)
                {
                    //Մուտքագրված է սխալ փոխարժեք:
                    result.Add(new ActionError(57));
                }
                else if (rate != currentCrossRate)
                {
                    if (paymentOrder.Type == OrderType.Convertation)
                    {
                        paymentOrder.ForDillingApprovemnt = true;
                    }
                }

            }

            return result;
        }
        /// <summary>
        /// Հաճախորդի ստորագրության նմուշի առկայության ստուգում
        /// </summary>
        /// <param name="customer">Հաճախորդ</param>
        /// <returns></returns>
        public static List<ActionError> ValidateCustomerSignature(ACBAServiceReference.Customer customer)
        {
            List<ActionError> result = new List<ActionError>();

            short customerType;

            List<CustomerDocument> customerDocuments;

            customerType = customer.customerType.key;

            if (customerType == (short)CustomerTypes.physical)
            {
                customerDocuments = (customer as PhysicalCustomer).person.documentList;
            }
            else
            {
                customerDocuments = (customer as LegalCustomer).Organisation.documentList;
            }

            result.AddRange(ValidateCustomerSignature(customer.customerNumber, customerDocuments, customerType));



            return result;
        }

        /// <summary>
        /// Հաճախորդի ստորագրության նմուշի առկայության ստուգում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateCustomerSignature(ulong customerNumber)
        {

            List<CustomerDocument> customerDocuments = ACBAOperationService.GetCustomerDocumentList(customerNumber);

            short customerType = ACBAOperationService.GetCustomerType(customerNumber);

            return ValidateCustomerSignature(customerNumber, customerDocuments, customerType);
        }
        private static List<ActionError> ValidateCustomerSignature(ulong customerNumber, List<CustomerDocument> customerDocuments, short customerType)
        {
            List<ActionError> result = new List<ActionError>();
            bool hasError = false;


            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
            if (!isTestVersion)
            {
                if (customerType == (short)CustomerTypes.physical)
                {
                    if (customerDocuments.FindAll(m => m.defaultSign).Count > 0)
                    {
                        CustomerDocument defaultDocument = customerDocuments.Find(m => m.defaultSign);

                        if (defaultDocument.attachmentList.Count == 0)
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        hasError = true;
                    }

                }
                else
                {
                    if (customerDocuments.FindAll(m => m.documentType.key == 28).Count > 0)
                    {
                        CustomerDocument defaultDocument = customerDocuments.Find(m => m.documentType.key == 28);

                        if (defaultDocument.attachmentList.Count == 0)
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        hasError = true;
                    }
                }
            }

            if (hasError)
            {
                string customerName = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(customerNumber));
                //Ստորագրության նմուշը բացակայում է
                result.Add(new ActionError(584, new string[] { customerNumber + " ՝ " + customerName }));
            }


            return result;
        }

        /// <summary>
        /// Հաշվի տվյալների խմբագրման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccountDataChangeOrder(AccountDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Account account = AccountDB.GetAccount(order.DataChangeAccount.AccountNumber, order.CustomerNumber);

            string canChangeAllAccountAdditionsTypes = "";
            order.user?.AdvancedOptions?.TryGetValue("canChangeAllAccountAdditionsTypes", out canChangeAllAccountAdditionsTypes);

            if ((order.AdditionalDetails.AdditionType == 3 || order.AdditionalDetails.AdditionType == 4
                || order.AdditionalDetails.AdditionType == 9 || order.AdditionalDetails.AdditionType == 10
                || order.AdditionalDetails.AdditionType == 11 || order.AdditionalDetails.AdditionType == 12
                || order.AdditionalDetails.AdditionType == 14 || order.AdditionalDetails.AdditionType == 17) && canChangeAllAccountAdditionsTypes != "1")
            {
                // Գործողությունը հասանելի չէ:
                result.Add(new ActionError(639));
            }
            if (order.AdditionalDetails.AdditionType == 13)
            {
                if (order.DataChangeAccount.Currency != "AMD")
                {
                    // Գործողությունը հասանելի չէ:
                    result.Add(new ActionError(752));
                }
                if (AccountDataChangeOrder.IfExistsServiceFeeCharge(order) == true)
                {
                    // Գործողությունը հասանելի չէ:
                    result.Add(new ActionError(753));
                }
            }
            if (order.AdditionalDetails.AdditionalValueType == AdditionalValueType.Double || order.AdditionalDetails.AdditionalValueType == AdditionalValueType.Percent)
            {
                double output;
                bool isNumeric = Double.TryParse(String.IsNullOrEmpty(order.AdditionalDetails.AdditionValue) ? "" : order.AdditionalDetails.AdditionValue.Trim().Replace("%", ""), out output);
                if (!isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(205, new string[] { "Արժեք " }));
                }
                else
                {
                    order.AdditionalDetails.AdditionValue = (order.AdditionalDetails.AdditionalValueType == AdditionalValueType.Percent) ? ((output >= 1) ? output / 100 : output).ToString() : output.ToString();
                }
            }
            else if (order.AdditionalDetails.AdditionalValueType == AdditionalValueType.Int)
            {
                int output;
                bool isNumeric = Int32.TryParse(String.IsNullOrEmpty(order.AdditionalDetails.AdditionValue) ? "" : order.AdditionalDetails.AdditionValue.Trim(), out output);
                if (!isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(205, new string[] { "Արժեք " }));
                }
                else
                {
                    order.AdditionalDetails.AdditionValue = output.ToString();
                }
            }
            else if (order.AdditionalDetails.AdditionalValueType == AdditionalValueType.Date)
            {
                DateTime output;
                bool isNumeric = DateTime.TryParse(String.IsNullOrEmpty(order.AdditionalDetails.AdditionValue) ? "" : order.AdditionalDetails.AdditionValue.Trim(), out output);
                if (!isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(629, new string[] { "Արժեք " }));
                }
                else
                {
                    order.AdditionalDetails.AdditionValue = output.ToString("dd/MMM/yy");
                }
            }
            if (AccountDataChangeOrder.IsSecondDataChange(order.CustomerNumber, order.DataChangeAccount.AccountNumber) == true)
            {
                //Տվյալ հաշվի համար գոյություն ունի խմբագրման չհաստատված հայտ
                result.Add(new ActionError(586, new string[] { " լրացուցիչ տվյալների խմբագրման " }));
            }
            if (account == null)
            {
                ///Խմբագրվող հաշիվը գտնված չէ 
                result.Add(new ActionError(492, new string[] { order.DataChangeAccount.AccountNumber.ToString() }));
            }
            if (order.Type != OrderType.AccountAdditionalDataRemovableOrder && AccountDataChangeOrder.IsSameAdditionalDataChange(order.DataChangeAccount.AccountNumber, order.AdditionalDetails) == true)
            {
                ///Խմբագրման ենթակա հաշվի լրացուցիչ տվյալն ու արժեքն արդեն գործում է
                result.Add(new ActionError(587));
            }
            if (order.AdditionalDetails.AdditionType == 5 && (order.AdditionalDetails.AdditionValue == "1" || order.AdditionalDetails.AdditionValue == "3" || order.AdditionalDetails.AdditionValue == "4"))
            {
                List<CustomerEmail> customerEmails;

                var customer = ACBAOperationService.GetCustomerMainData(order.CustomerNumber);
                customerEmails = customer.Emails;

                if (!customerEmails.Exists(item => item.emailType.key == 5))
                {
                    //Հաճախորդի Էլեկտրոնային հասցեն բացակայում է
                    result.Add(new ActionError(606));
                }
            }
            if (order.AdditionalDetails.AdditionType == 101)
            {
                if (!AccountDataChangeOrder.IsApprovedByTaxService(order.DataChangeAccount.AccountNumber))
                {
                    result.Add(new ActionError(1910));
                }

                if (!order.AdditionalDetails.AdditionValue.Equals("1") && !order.AdditionalDetails.AdditionValue.Equals("0"))
                {
                    result.Add(new ActionError(798));
                }

                byte customerType = Customer.GetCustomerType(order.CustomerNumber);
                if (customerType == (short)CustomerTypes.physical)
                {
                    result.Add(new ActionError(808));
                }
                result.AddRange(ValidateCustomerSignature(order.CustomerNumber));
            }

            if (order.Type == OrderType.AccountAdditionalDataRemovableOrder)
            {
                if (Constants.NON_REMOVABLE_ACCOUNT_ADDITION_ID.Contains(order.AdditionalDetails.AdditionType))
                {
                    // Գործողությունը հասանելի չէ:
                    result.Add(new ActionError(639));
                }
            }
            if (order.AdditionalDetails.AdditionType == 16)
            {
                bool isNumeric = false;
                string bankruptcyManager = order.AdditionalDetails.AdditionValue;
                ulong additionValue;
                isNumeric = ulong.TryParse(bankruptcyManager, out additionValue);

                if (!isNumeric)
                {
                    result.Add(new ActionError(1393));
                }
            }

            if (order.AdditionalDetails.AdditionType == 16 && (order.CustomerNumber.ToString() == order.AdditionalDetails.AdditionValue))
            {
                result.Add(new ActionError(1396));
            }

            if (order.AdditionalDetails.AdditionType == 16)
            {
                if (order.AdditionalDetails.AdditionValue != null)
                {
                    short custType = ACBAOperationService.GetCustomerType(Convert.ToUInt64(order.AdditionalDetails.AdditionValue));
                    if (custType != (short)CustomerTypes.physical)
                    {
                        result.Add(new ActionError(1389));
                    }
                }
                else
                {
                    result.Add(new ActionError(1393));
                }

            }

            if (order.AdditionalDetails.AdditionType == 16 && (order.Type == OrderType.AccountAdditionalDataRemovableOrder))
            {
                // Գործողությունը հասանելի չէ:
                result.Add(new ActionError(639));
            }



            return result;
        }


        public static List<ActionError> ValidateOPPerson(Order order, Account creditAccount = null, string debitAccountNumber = "0")
        {

            List<ActionError> result = new List<ActionError>();

            if (order.Source == SourceType.MobileBanking || order.Source == SourceType.AcbaOnline || order.Source == SourceType.SSTerminal || order.Source == SourceType.CashInTerminal)
            {
                return result;
            }

            short customerType = 6;
            if (order.CheckCustomerUpdateExpired && order.CustomerNumber != 0)
            {
                if (order.OPPerson.CustomerNumber != order.CustomerNumber)
                {
                    if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
                    {
                        //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                        result.Add(new ActionError(765, new string[] { order.CustomerNumber.ToString() }));
                        return result;
                    }

                }
            }

            ACBAServiceReference.Customer customer;
            if (order.OPPerson.CustomerNumber != 0)
            {

                customer = ACBAOperationService.GetCustomer(order.OPPerson.CustomerNumber);
                customerType = customer.customerType.key;

                if (customer.identityId == 0)
                {
                    //Գործարք կատարողի հաճախորդի համարը սխալ է մուտքագրված
                    result.Add(new ActionError(600));

                }

                if (customer.quality.key == 43)
                {
                    //Գործարք կատարողը կրկնակի հաճախորդ է
                    result.Add(new ActionError(597));
                    return result;
                }

                if (order.Type != OrderType.CashTransitCurrencyExchangeOrder && order.Type != OrderType.TransitPaymentOrder)
                    result.AddRange(ValidateCustomerSignature(customer));

                if (order.Type == OrderType.CashDebit && customerType != 6)
                {
                    //Գործարք կատարող հաճախորդը պետք է լինի ֆիզիկական անձ
                    result.Add(new ActionError(596));
                    return result;
                }

                if (order.CheckCustomerUpdateExpired && Customer.IsCustomerUpdateExpired(customer.customerNumber))
                {
                    //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                    result.Add(new ActionError(765, new string[] { customer.customerNumber.ToString() }));
                    return result;
                }

                if (order.ValidateForConvertation && customer.residence.key == 0)
                {
                    //Հաճախորդի ռեզիդենտությունը նշված չէ
                    result.Add(new ActionError(964));
                    return result;
                }
            }


            if (!string.IsNullOrEmpty(order.OPPerson.PersonNoSocialNumber) && order.OPPerson.PersonNoSocialNumber.Trim().Length != 10)
            {
                //ՀԾՀ չստանալու մասին տեղեկանքի համար դաշտը սխալ է լրացված:
                result.Add(new ActionError(472));
            }

            if (order.Type == OrderType.CashDebit || order.Type == OrderType.CashPosPayment || order.Type == OrderType.TransitPaymentOrder || order.Type == OrderType.CashForRATransfer || order.Type == OrderType.InterBankTransferCash)
            {

                if (order.Amount * Utility.GetCBKursForDate(order.RegistrationDate, order.Currency) > 400000 && order.OPPerson.CustomerNumber == 0)
                {
                    //Տվյալ գործարքը ձևակերպելու համար անհրաժեշտ է մուտքագրել վճարողի հաճախորդի համարը 
                    result.Add(new ActionError(598));
                }
            }
            else if (order.Type == OrderType.CashConvertation)
            {
                CurrencyExchangeOrder currencyExchangeOrder = (CurrencyExchangeOrder)order;
                if (currencyExchangeOrder.AmountInAmd > 400000 && order.OPPerson.CustomerNumber == 0)
                {
                    //Տվյալ գործարքը ձևակերպելու համար անհրաժեշտ է մուտքագրել վճարողի հաճախորդի համարը 
                    result.Add(new ActionError(598));
                }
            }


            if (order.Type == OrderType.CashCredit)
            {
                if (ValidationDB.CheckAccCashPos(debitAccountNumber))
                {
                    if (order.Amount * Utility.GetCBKursForDate(order.RegistrationDate, order.Currency) > 400000 && order.OPPerson.CustomerNumber == 0)
                    {
                        //Տվյալ գործարքը ձևակերպելու համար անհրաժեշտ է մուտքագրել վճարողի հաճախորդի համարը 
                        result.Add(new ActionError(598));
                    }
                }
                else if (order.OPPerson.CustomerNumber == 0)
                {
                    //Տվյալ գործարքը ձևակերպելու համար անհրաժեշտ է մուտքագրել վճարողի հաճախորդի համարը 
                    result.Add(new ActionError(598));
                }
            }
            if (creditAccount != null)
            {
                if (order.OPPerson.CustomerNumber == 0 && creditAccount.Status == 10)
                {
                    //Տվյալ գործարքը ձևակերպելու համար անհրաժեշտ է մուտքագրել վճարողի հաճախորդի համարը 
                    result.Add(new ActionError(598));
                }
            }

            if (order.OPPerson.PersonDocument != null && (order.Type == OrderType.RATransfer || order.Type == OrderType.CashForRATransfer))
            {
                if (order.OPPerson.PersonDocument.Length > 50)
                {
                    //Գործարք կատարողի անձնագրի տվյալների նիշերի քանակը մեծ է 50-ից
                    result.Add(new ActionError(657));
                }
            }

            ////Սահմանափակ հասանելիությամ հաշվիներ
            if (creditAccount?.TypeOfAccount == 283 && order.Amount >= 400000 && order.Type == OrderType.CashDebit)
            {   //Սահամանափակ հասանելիությամբ հաշիվներից գործարքի գումարը չի կարող գերազանցել 400,000 ՀՀ դրամը
                result.Add(new ActionError(1781));
            }

            if (order.DebitAccount?.TypeOfAccount == 283 && order.Type == OrderType.RATransfer && order.SubType == 2)
            {
                result.Add(new ActionError(1788));
            }

            if (customerType == 6)
            {
                if (string.IsNullOrEmpty(order.OPPerson.PersonName.Trim(' ')))
                {
                    //Գործարք կատարողի անունը լրացված չէ
                    result.Add(new ActionError(648));
                }

                if (string.IsNullOrEmpty(order.OPPerson.PersonLastName.Trim(' ')))
                {
                    //Գործարք կատարողի ազգանունը լրացված չէ
                    result.Add(new ActionError(649));
                }
            }

            if ((order.Type == OrderType.CashForRATransfer && customerType == 6) || order.Type == OrderType.InterBankTransferCash || order.Type == OrderType.InterBankTransferNonCash)
            {
                if (string.IsNullOrEmpty(order.OPPerson.PersonAddress))
                {
                    //Գործարք կատարողի հասցեն լրացված չէ
                    result.Add(new ActionError(651));
                }
                if (string.IsNullOrEmpty(order.OPPerson.PersonPhone))
                {
                    //Գործարք կատարողի հեռախոսը լրացված չէ
                    result.Add(new ActionError(652));
                }
                if (order.OPPerson.PersonResidence == 0)
                {
                    //Գործարք կատարողի կարգավիճակը լրացված չէ
                    result.Add(new ActionError(653));
                }
                if (order.OPPerson.PersonAddress != null)
                {
                    if (order.OPPerson.PersonAddress.Length > 255)
                    {
                        //Գործարք կատարողի հասցեի նիշերի քանակը մեծ է 255-ից
                        result.Add(new ActionError(654));
                    }
                }
                if (order.OPPerson.PersonEmail != null)
                {
                    if (order.OPPerson.PersonEmail.Length > 50)
                    {
                        //Գործարք կատարողի էլ. հասցեի նիշերի քանակը մեծ է 50-ից
                        result.Add(new ActionError(655));
                    }
                }

                if (order.OPPerson.PersonPhone != null)
                {
                    if (order.OPPerson.PersonPhone.Length > 50)
                    {
                        //Գործարք կատարողի հեռախոսի նիշերի քանակը մեծ է 50-ից
                        result.Add(new ActionError(656));
                    }
                }

            }


            return result;

        }


        /// <summary>
        /// Վճարման հանձնարարականի ստուգում, երբ կատարվում է փոխանցման ձևակերպում
        /// </summary>
        /// <returns></returns>
        public static List<ActionError> ValidateForTransfer(PaymentOrder order, User user)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.TransferID != 0)
            {
                Transfer transfer = new Transfer();
                transfer.Id = order.TransferID;
                transfer.Get();

                if (transfer.Verified == 2)
                {
                    //Նշված փոխանցումը կասկածելի է
                    result.Add(new ActionError(799));
                }

                if (transfer.AmlCheck == 1)
                {
                    //Նշված փոխանցումը պահանջում է AML բաժնի կողմից հաստատում
                    result.Add(new ActionError(800));
                }
                if (transfer.AmlCheck == 3 || transfer.VerifiedAml == 4)
                {
                    //Նշված փոխանցումը մերժված է AML բաժնի կողմից
                    result.Add(new ActionError(801));
                }
                if (transfer.AmlCheck == 4)
                {
                    //Նշված փոխանցումը դիտարկվում է AML բաժնի կողմից
                    result.Add(new ActionError(802));
                }
                if (transfer.VerifiedAml == 2)
                {
                    //Նշված փոխանցումը կասկածելի է, պահանջում է AML բաժնի կողմից հաստատում
                    result.Add(new ActionError(803));
                }
                if (transfer.VerifiedAml == 5)
                {
                    //Նշված փոխանցումը կասկածելի է, դիտարկվում է AML բաժնի կողմից 
                    result.Add(new ActionError(804));
                }
                if (transfer.CashOperationDate != null)
                {
                    //Փոխանցման կանխիկ ձևակերպւմն արդեն կատարված է
                    result.Add(new ActionError(805));
                }

                if (order.DebitAccount.Currency != transfer.Currency)
                {
                    //Դեբետ հաշվի արժույթը տարբերվում է փոխանցման արժույթից
                    result.Add(new ActionError(812));
                }

                if (order.OPPerson.CustomerNumber != transfer.CustomerNumber && transfer.TransferGroup != 4)
                {
                    //Գործարք կատարող անձի հաճախորդի համարը տարբերվում է փոխանցման հաճախորդի համարից
                    result.Add(new ActionError(814));
                }



                if (Math.Round(transfer.Amount, 2) < Math.Round(transfer.PaidAmount + order.Amount, 2))
                {
                    //Վճարվող գումարը մեծ է փոխանցման գումարից
                    result.Add(new ActionError(806));
                }


                if (transfer.PaidAmount != 0 && Math.Round(transfer.Amount, 2) != Math.Round(transfer.PaidAmount + order.Amount, 2))
                {
                    //Վճարվող և վճարված գումարների հանրագումարը սխալ է
                    result.Add(new ActionError(807));
                }

                if (transfer.AddTableName == "Tbl_transfers_by_call" && transfer.Amount != order.Amount)
                {
                    //Վճարվող գումարը հավասար չէ փոխանցման գումարին
                    result.Add(new ActionError(809));
                }

                if (transfer.TransferGroup == 4 && transfer.PaidAmount == 0 && transfer.Amount - order.Amount >= (transfer.Currency == "EUR" ? 5 : 1) && order.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && order.Type != OrderType.TransitCashOutCurrencyExchangeOrder)
                {
                    //Վճարվող գումարը հավասար չէ փոխանցման գումարին
                    result.Add(new ActionError(809));
                }

                if (transfer.TransferGroup == 4 && transfer.Currency != "AMD" && transfer.PaidAmount == 0 && order.Amount - Math.Truncate(order.Amount) != 0 && order.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && order.Type != OrderType.TransitCashOutCurrencyExchangeOrder)
                {
                    //Մանրը անհրաժեշտ է ձևակերպել փոխարկումով
                    result.Add(new ActionError(913));
                }


                if (transfer.AddTableName == "Tbl_transfers_by_call" && transfer.CreditAccount.AccountNumber != order.ReceiverAccount.AccountNumber)
                {
                    //Կրեդիտային հաշիվը սխալ է մուտքագրված:
                    result.Add(new ActionError(6));
                }

                if (transfer.AddTableName == "Tbl_transfers_by_call"
                           && (((order.ConvertationRate1 == 0 && order.ConvertationRate != transfer.RateBuy && order.ConvertationRate != transfer.RateSell)) || (order.ConvertationRate1 != 0 && (order.ConvertationRate1 != transfer.RateSell || order.ConvertationRate != transfer.RateBuy))))
                {
                    //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                    result.Add(new ActionError(121));
                }
                if (order.Amount < transfer.Amount && transfer.PaidAmount == 0 && order.Type != OrderType.TransitCashOutCurrencyExchangeOrder && order.Type != OrderType.TransitCashOut)
                {
                    //Առաջին մասնակի վճարումը պետք է լինի կանխիկ, քանի որ երկրորդը կատարվելու է անկանխիկ
                    result.Add(new ActionError(810));
                }

                if (order.Type != OrderType.TransitCashOutCurrencyExchangeOrder && order.Type != OrderType.TransitCashOut)
                {
                    ulong customerNumber = order.ReceiverAccount.GetAccountCustomerNumber();
                    if (customerNumber != transfer.CustomerNumber && transfer.TransferGroup != 4)
                    {
                        //Կրեդիտ հաշիվը փոխանցում ստացող հաճախորդին չի պատկանում
                        result.Add(new ActionError(811));
                    }
                }

            }

            return result;
        }


        public static List<ActionError> ValidateAttachmentDocument(Order order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnlineXML && order.Source != SourceType.ArmSoft)
            {
                double amount;

                if (order.Currency == "AMD")
                {
                    amount = order.Amount;
                }
                else
                {
                    amount = order.Amount * Utility.GetLastCBExchangeRate(order.Currency);
                }

                if (amount > Constants.TRANSFER_ATTACHMENT_REQUIRED_AMOUNT)
                {
                    if (order.Attachments == null || order.Attachments.Count == 0)
                    {
                        //Փոխանցման գումարը մեծ է 6 մլն. դրամից:Խնրում ենք կցել փոխանցման հանձնարարականը:
                        result.Add(new ActionError(607));
                    }
                }
            }




            if (result.Count() > 0)
            {
                return result;
            }


            if (order.Attachments != null)
            {
                order.Attachments.ForEach(m =>
                {
                    int length = m.Attachment.Length;
                    if (length > 307200)
                    {
                        //Կցված փոխանցման հանձնարարականի չափը մեծ է 100kb-ից:
                        result.Add(new ActionError(625));
                    }

                });
            }

            return result;

        }
        public static bool IsDAHKAvailability(ulong customerNumber)
        {
            return ValidationDB.IsDAHKAvailability(customerNumber);
        }

        /// <summary>
        /// Ստուգում դրամարկղի սահմանաչափերը:
        /// </summary>
        /// <param name="order">Ստուգվող հայտը</param>
        /// <param name="user">Օգտագործողը</param>
        /// <returns></returns>
        public static List<ActionError> ValidateCashOperationAvailability(Order order, User user)
        {
            List<ActionError> result = new List<ActionError>();

            if ((order.Source == SourceType.SSTerminal && order.Type != OrderType.CashConvertation) || order.Source == SourceType.CashInTerminal)
            {
                return result;
            }

            try
            {
                string debitCredit = CashOperationDirection(order);

                if (debitCredit != "" && !order.OnlySaveAndApprove)
                {
                    string operationCurrency = CashOperationCurrency(order);
                    if (order.GetType().Name != "AccountReOpenOrder")
                    {
                        ValidationDB.ValidateCashOperationAvailability(order, debitCredit, operationCurrency, user);
                    }
                    else
                    {
                        ValidationDB.ValidateCashOperationAvailability((AccountReOpenOrder)order, debitCredit, operationCurrency, user);
                    }
                }
            }
            catch (Exception ex)
            {

                ActionError actionError = new ActionError();
                actionError.Code = 0;
                actionError.Description = ex.Message;
                result.Add(actionError);
            }

            return result;
        }

        internal static string CashOperationDirection(Order order)
        {
            string result = "";
            switch (order.Type)
            {
                case OrderType.CashDebit:
                case OrderType.ReestrTransferOrder:
                case OrderType.CashInsuranceOrder:
                    result = "d";
                    break;
                case OrderType.CashCredit:
                    result = "c";
                    break;
                case OrderType.CashDebitConvertation:
                    result = "d";
                    break;
                case OrderType.CashCreditConvertation:
                    result = "c";
                    break;
                case OrderType.CashConvertation:
                    result = "d";
                    break;
                case OrderType.CashCommunalPayment:
                case OrderType.ReestrCashCommunalPayment:
                    result = "d";
                    break;
                case OrderType.CashForRATransfer:
                    result = "d";
                    break;
                case OrderType.CurrentAccountReOpen:
                    result = "d";
                    break;
                case OrderType.TransitPaymentOrder:
                    result = "d";
                    break;
                case OrderType.CashPosPayment:
                    result = "c";
                    break;
                case OrderType.CashFeeForServiceProvided:
                    result = "d";
                    break;
                case OrderType.CashTransitCurrencyExchangeOrder:
                    result = "d";
                    break;
                case OrderType.TransitCashOutCurrencyExchangeOrder:
                    result = "c";
                    break;
                case OrderType.TransitNonCashOutCurrencyExchangeOrder:
                    result = "c";
                    break;
                case OrderType.TransitCashOut:
                    result = "c";
                    break;
                case OrderType.TransitNonCashOut:
                    result = "c";
                    break;
                case OrderType.CashDepositCasePenaltyMatureOrder:
                    result = "d";
                    break;
                case OrderType.PaymentToArcaOrder:
                    if (order.SubType == 1)
                    {
                        result = "d";
                    }

                    break;


            }
            return result;
        }

        internal static string CashOperationCurrency(Order order)
        {
            string result = "";
            Type orderType = order.GetType();
            if (orderType.Name == "PaymentOrder" || orderType.Name == "CurrencyExchangeOrder"
                || orderType.Name == "TransitCurrencyExchangeOrder" || orderType.Name == "ReestrTransferOrder")
            {
                PaymentOrder paymentOrder = (PaymentOrder)order;
                string debitCredit = CashOperationDirection(order);
                if (debitCredit == "d")
                {
                    result = paymentOrder.DebitAccount.Currency;
                }
                else if (debitCredit == "c")
                {
                    result = paymentOrder.ReceiverAccount.Currency;
                }
            }
            else if (orderType.Name == "UtilityPaymentOrder" || orderType.Name == "AccountReOpenOrder" || orderType.Name == "InsuranceOrder" || orderType.Name == "ReestrUtilityPaymentOrder")
            {
                result = "AMD";
            }
            else if (orderType.Name == "TransitPaymentOrder")
            {
                TransitPaymentOrder paymentOrder = (TransitPaymentOrder)order;
                result = paymentOrder.DebitAccount.Currency;
            }
            else if (orderType.Name == "CashPosPaymentOrder")
            {
                result = order.Currency;
            }
            else if (orderType.Name == "FeeForServiceProvidedOrder")
            {
                result = "AMD";
            }
            else if (orderType.Name == "DepositCasePenaltyMatureOrder")
            {
                result = "AMD";
            }

            return result;
        }

        internal static List<ActionError> ValidateTransitPaymentOrder(TransitPaymentOrder order, User user)
        {

            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(order.DebitAccount.Currency) || order.TransitAccount.AccountNumber == "0")
            {
                result.Add(new ActionError(712));
                return result;
            }

            result.AddRange(Validation.ValidateCashOperationAvailability(order, user));

            result.AddRange(Validation.ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(Validation.ValidateDocumentNumber(order, order.CustomerNumber));

            result.AddRange(Validation.ValidateOPPerson(order));

            if (string.IsNullOrEmpty(order.Currency))
            {
                result.Add(new ActionError(254));
            }
            if (order.Amount <= 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(order.Amount, order.Currency))
            {
                result.Add(new ActionError(25));
            }
            if (order.Currency != "AMD" && order.Type != OrderType.NonCashTransitPaymentOrder && !Validation.IsCurrencyAmountCorrect(order.Amount, order.Currency))
            {
                //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                result.Add(new ActionError(1053, new string[] { order.Currency }));
            }
            if (order.Type == OrderType.NonCashTransitPaymentOrder && order.DebitTransitAccountType == TransitAccountTypes.None)
            {
                result.AddRange(ValidateDebitAccount(order, order.DebitAccount));
                result.AddRange(CheckCustomerDebtsAndDahk(order.CustomerNumber, order.DebitAccount));
            }

            return result;
        }

        /// <summary>
        /// Սիստեմային հաշիվը  բացակայում է  թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        //public static List<ActionError> ValidateForSystemAccount(ulong customerNumber, int filialCode)
        //{
        //    List<ActionError> result = new List<ActionError>();
        //    bool isDahk = Validation.IsDAHKAvailability(customerNumber);
        //    if (isDahk)
        //    {
        //        double systemAccount = Account.GetSystemAccount(45, filialCode);
        //        if (systemAccount == 0)
        //            result.Add(new ActionError(0));
        //    }
        //    return result;
        //}

        /// <summary>
        /// Հաճախորդի փաստաթղթերում ՀԾՀ կամ ՀԾՀ-ից հրաժարման տեղեկանք տեսակով փաստաթղթի առկայության ստուգում
        /// </summary>
        /// <returns></returns>
        public static bool ValidateCustomerPSN(List<CustomerDocument> customerDocuments)
        {
            bool result = true;

            if (!customerDocuments.Exists(m => m.documentGroup.key == 1) || customerDocuments.Exists(m => m.documentGroup.key == 1 && m.documentType.key != 2))
            {
                if (!customerDocuments.Exists(m => m.documentType.key == 2 || m.documentType.key == 56 || m.documentType.key == 57))
                {
                    //Հաճախորդի փաստաթղթերում <հանրային ծառայությունների համարանիշ> կամ <հանրային ծառայությունների համարանիշից հրաժարման տեղեկանք> տեսակով փաստաթուղթը բացակայում է
                    result = false;
                }
            }

            return result;
        }


        /// <summary>
        /// Հաշվի սառեցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccountFreezeOrder(AccountFreezeOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Account account = AccountDB.GetAccount(order.FreezeAccount.AccountNumber, order.CustomerNumber);
            if (order.FreezeReason == 0)
            {
                // Սառեցման պատճառը մուտքագրված
                result.Add(new ActionError(663, new string[] { "Սառեցման պատճառը" }));
            }
            if (order.FreezeAmount != 0)
            {
                if (order.FreezeAmount.ToString().IndexOf('.') > 0)
                {
                    if (order.FreezeAmount.ToString("R").Split('.')[1].Length > ((account.Currency == "AMD") ? 1 : 2))
                    {
                        // Ստուգեք գումարի ցենտերը (նիշերի քանակը >2)
                        string var1 = (account.Currency == "AMD") ? "լումաները" : "ցենտերը";
                        string var2 = (account.Currency == "AMD") ? "1" : "2";
                        result.Add(new ActionError(662, new string[] { var1, var2 }));
                    }
                }
            }
            if ((order.FreezeAmount == 0) && !order.AmountFreezeDate.HasValue && !order.FreezeDate.HasValue)
            {
                // Տվյալները թերի են մուտքագրված
                result.Add(new ActionError(663, new string[] { "Սառեցման ամսաթիվը կամ գումարի սառեցման ամսաթիվն ու գումարը" }));
            }
            else if ((order.FreezeAmount != 0) && !order.AmountFreezeDate.HasValue)
            {
                // Սառեցման ամսաթիվը մուտքագրված չէ
                result.Add(new ActionError(663, new string[] { "Սառեցման ամսաթիվը" }));
            }
            if ((order.FreezeAmount == 0) && order.AmountFreezeDate.HasValue)
            {
                // Սառեցման ենթակա գումարը մուտքագրված չէ
                result.Add(new ActionError(663, new string[] { "Սառեցման ենթակա գումարը" }));
            }

            if (account == null)
            {
                ///Սառեցվող հաշիվը գտնված չէ 
                result.Add(new ActionError(492, new string[] { order.FreezeAccount.AccountNumber.ToString() }));
            }
            else if (account.AccountType == 13)
            {
                double availableBalance = Account.GetAcccountAvailableBalance(account.AccountNumber);
                if (availableBalance == 0)
                {
                    //Հաշվի մնացորդը չի բավարարում սառեցում կատարելու համար:Հասանելի մնացորդ 0
                    result.Add(new ActionError(784, new string[] { account.Currency }));
                }

            }
            return result;
        }




        /// <summary>
        /// Հաշվի ապասառեցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccountUnfreezeOrder(AccountUnfreezeOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Account account = AccountDB.GetAccount(order.FreezedAccount.AccountNumber, order.CustomerNumber);

            if (order.FreezeId == 0)
            {
                ///Սխալ սառեցման համար
                result.Add(new ActionError(676, new string[] { order.FreezedAccount.AccountNumber.ToString() }));
            }


            if (AccountUnfreezeOrder.IsInaccessibleUnfreeze(order.FreezeId, order.FreezedAccount.AccountNumber, order.user.isOnlineAcc, order.user.isBranchAccountsDiv) == true)
            {
                // Գործողությունը հասանելի չէ:
                result.Add(new ActionError(639));
            }




            if (AccountUnfreezeOrder.IsSecondUnfreeze(order.FreezeId, order.FreezedAccount.AccountNumber) == true)
            {
                //Տվյալ հաշվի համար գոյություն ունի ապասառեցման չհաստատված հայտ
                result.Add(new ActionError(586, new string[] { " ապասառեցման " }));
            }


            if (account == null)
            {
                ///Սառեցվող հաշիվը գտնված չէ 
                result.Add(new ActionError(492, new string[] { order.FreezedAccount.AccountNumber.ToString() }));
            }

            return result;
        }


        public static List<ActionError> ValidateLoanProductOrderDocument(LoanProductOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(Validation.ValidateDocumentNumber(order, order.CustomerNumber));

            result.AddRange(Validation.ValidateOPPerson(order));

            if (LoanProductOrder.CheckLoanRequest(order))
            {
                //Գոյությու ունի չտրված վարկ/վարկային գիծ
                result.Add(new ActionError(302));
            }

            if (order.StartDate == default(DateTime))
            {
                //Սկիզբը նշված չէ
                result.Add(new ActionError(285));
            }

            if (!Utility.IsWorkingDay(order.StartDate))
            {
                //Վարկի սկիզբը աշխատանքային օր չէ
                result.Add(new ActionError(287));
            }

            if (order.EndDate == default(DateTime))
            {
                result.Add(new ActionError(288));
            }


            if (order.Type != OrderType.CreditLineSecureDeposit && !Utility.IsWorkingDay(order.EndDate))
            {
                //Ոչ աշխատանքային օր
                result.Add(new ActionError(290));
            }

            if (order.Type != OrderType.CreditLineSecureDeposit && !Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened).Exists(m => m.Currency == order.Currency))
            {
                //Հաճախորդը չունի վարկի արժույթով ընթացիկ հաշիվ
                result.Add(new ActionError(303));
            }

            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
            {

                if (order.Type == OrderType.CreditLineSecureDeposit)
                {

                    if (order.ProductType == 50 || order.ProductType == 51)
                    {
                        if (order.EndDate.Date > order.StartDate.Date.AddMonths(60))
                        {
                            result.Add(new ActionError(1659));
                        }

                        if (order.StartDate.Month == order.ValidationDate.Value.Month && order.StartDate.Year == order.ValidationDate.Value.Year)
                        {
                            //Դուք չեք կարող դիմել վարկային գծի համար քանի որ քարտի վերջնաժամկետը լրանում է այս ամիս
                            result.Add(new ActionError(1712));
                        }
                    }
                    else
                    {
                        if (order.StartDate.Month == order.EndDate.Month && order.StartDate.Year == order.EndDate.Year)
                        {
                            //Դուք չեք կարող դիմել վարկային գծի համար քանի որ քարտի վերջնաժամկետը լրանում է այս ամիս
                            result.Add(new ActionError(1712));
                        }
                    }


                }

                if (String.IsNullOrEmpty(order.PledgeCurrency))
                {
                    //Գրավադրվող արժույթը ընտրված չէ:
                    result.Add(new ActionError(1648));
                }
                order.ProvisionCurrency = order.PledgeCurrency;

                if (order.Type == OrderType.CreditSecureDeposit)
                {
                    if (order.FeeAccount == null || order.FeeAccount.AccountNumber == null || order.FeeAccount.AccountNumber == "0")
                    {
                        //Միջնորդավճարի հաշիվը մուտքագրված չէ:
                        result.Add(new ActionError(90));
                        return result;
                    }



                    if (order.FeeAccount != null)
                    {
                        Account account;
                        account = Account.GetAccount(order.FeeAccount.AccountNumber);

                        //Միջնորդավճարի հաշիվը գտնված չէ
                        if (account == null)
                        {
                            result.Add(new ActionError(118));
                            return result;
                        }
                    }

                }



                if (order.Type == OrderType.CreditSecureDeposit)
                {
                    double fee = LoanProductOrder.GetCommisionAmountForDepositLoan(order.Amount, order.StartDate, order.EndDate, order.Currency, order.CustomerNumber);
                    if (fee != order.FeeAmount)
                    {
                        //Միջնորդավճարի գումարը սխալ է ։
                        result.Add(new ActionError(1423));
                    }

                }
            }

            else
            {
                if (order.ProvisionAccount == null)
                {
                    //Գրավադրվող հաշիվն ընտրված չէ
                    result.Add(new ActionError(292));
                }
                else if (string.IsNullOrEmpty(order.ProvisionAccount.AccountNumber))
                {
                    //Գրավադրվող հաշիվն ընտրված չէ
                    result.Add(new ActionError(292));
                }

            }


            if (order.Amount == 0)
            {
                //Վարկի գումարը մուտքագրված չէ
                result.Add(new ActionError(293));
            }

            if ((order.Currency != order.ProvisionCurrency) && order.MandatoryPayment != true)
            {
                //Գրավադրվող դրամական միջոցների և վարկային գծի արժույթները տարբեր են: Պարտադիր մուտքերով դաշտը սխալ է լրացված:
                result.Add(new ActionError(1391));
            }




            if (order.InterestRate == 0)
            {
                //Տոկոսադրույքը որոշված չէ
                result.Add(new ActionError(295));
            }

            if (order.Type == OrderType.CreditSecureDeposit)
            {
                if (order.FirstRepaymentDate == default(DateTime))
                {
                    //Առաջին մարման ամսաթիվը նշված չէ
                    result.Add(new ActionError(296));
                }

                if ((order.FirstRepaymentDate - order.StartDate).TotalDays > 45)
                {
                    //Առաջին մարման ամսաթիվը պետք է լինի ոչ ուշ քան վարկի սկզբից 45 օր
                    result.Add(new ActionError(298));
                }

                if (order.FirstRepaymentDate > order.EndDate)
                {
                    //Վերջնաժամկետը փոքր է առաջին մարման ամսաթվից
                    result.Add(new ActionError(304));
                }

                if (order.FirstRepaymentDate <= order.StartDate)
                {
                    //Առաջին մարման ամսաթիվը փոքր է վարկի սկզբից
                    result.Add(new ActionError(305));
                }

                double percent = LoanProductOrder.GetDepositLoanAndProvisionCoefficent(order.Currency, order.ProvisionCurrency);
                if (order.Amount * Utility.GetCBKursForDate(DateTime.Now.Date, order.Currency) / Utility.GetCBKursForDate(DateTime.Now.Date, order.ProvisionCurrency) > Customer.GetCustomerAvailableAmount(order.CustomerNumber, order.ProvisionCurrency) * percent)
                {
                    result.Add(new ActionError(307, new string[] { (percent * 100).ToString() }));
                }

            }
            if (order.Type == OrderType.CreditLineSecureDeposit)
            {
                double percent = LoanProductOrder.GetDepositLoanCreditLineAndProfisionCoefficent(order.Currency, order.ProvisionCurrency, order.MandatoryPayment, order.ProductType);

                if (order.Amount * Utility.GetCBKursForDate(DateTime.Now.Date, order.Currency) / Utility.GetCBKursForDate(DateTime.Now.Date, order.ProvisionCurrency) > Customer.GetCustomerAvailableAmount(order.CustomerNumber, order.ProvisionCurrency) * percent)
                {
                    result.Add(new ActionError(307, new string[] { (percent * 100).ToString() }));
                }

            }

            if (!Utility.IsCorrectAmount(order.Amount, order.Currency))
            {
                result.Add(new ActionError(25));
            }

            if (Customer.HasCommitment(order.CustomerNumber))
            {
                //ունի ժամկետանց պարտավորություն
                result.Add(new ActionError(301));
            }

            if (order.Type == OrderType.CreditLineSecureDeposit)
            {
                Card card = Card.GetCardWithOutBallance(order.ProductAccount.AccountNumber);
                List<CreditLine> creditLines = CreditLine.GetCardClosedCreditLines(order.CustomerNumber, card.CardNumber);



                if (order.FilialCode == 22059 && card.FilialCode == 22000)
                {
                    order.user.filialCode = (ushort)card.FilialCode;
                }


                if (order.Source != SourceType.PhoneBanking && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking && order.user.filialCode != card.FilialCode)
                {
                    //Քարտը գրանցված է այլ մասնաճյուղում
                    result.Add(new ActionError(716));
                }

                //7632 առաջարկ, 165021 case
                //foreach (var line in creditLines)
                //{
                //    if (line.ClosingDate == DateTime.Today)
                //    {
                //        //Այսօր կատարվել է տվյալ քարտին կցված վարկային գծի դադարեցում: Նոր վարկային գծի ձևակերպում հնարավոր է կատարել դադարեցումից մեկ օր հետո
                //        result.Add(new ActionError(396));
                //        break;
                //    }
                //}

                if (order.ProductType == 50)
                {
                    if (card.Type == 6 || card.Type == 20 || card.Type == 25 || card.Type == 26 || card.Type == 38 || card.Type == 39 || card.Type == 41)
                    {
                        result.Add(new ActionError(1412));
                    }
                }

                if (order.ProductType == 51)
                {
                    if (card.Type != 35)
                    {
                        result.Add(new ActionError(1413));
                    }
                }
            }

            if (InfoDB.CommunicationTypeExistenceFromSAP(order.CustomerNumber) == 0 && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                //SAP CRM ծրագրում հաճախորդի հետ հաղորդակցման եղանակը ընտրված չէ։
                result.Add(new ActionError(2034));
            }


            ActionError validResult = LoanProductOrder.ValidateLoanProduct(order);
            if (validResult != null)
            {
                result.Add(validResult);
            }

            if (order.Type == OrderType.CreditLineSecureDeposit)
            {
                string canChangeDepositLoanRate = null;


                Card card = Card.GetCardWithOutBallance(order.ProductAccount.AccountNumber);
                double rate = LoanProductOrder.GetInterestRate(order, card.CardNumber);
                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                {
                    if (rate != order.InterestRate)
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Add(new ActionError(719));
                    }
                }
                else
                {
                    order.user.AdvancedOptions.TryGetValue("canChangeDepositLoanRate", out canChangeDepositLoanRate);
                    if (rate != order.InterestRate && canChangeDepositLoanRate != "1")
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Add(new ActionError(719));
                    }
                }

            }
            else if (order.Type == OrderType.CreditSecureDeposit)
            {
                string canChangeDepositLoanRate = null;
                double rate = LoanProductOrder.GetInterestRate(order, null);

                if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                {
                    if (rate != order.InterestRate)
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Add(new ActionError(719));
                    }
                }
                else
                {
                    order.user.AdvancedOptions.TryGetValue("canChangeDepositLoanRate", out canChangeDepositLoanRate);
                    if (rate != order.InterestRate && canChangeDepositLoanRate != "1")
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Add(new ActionError(719));
                    }

                }

            }

            return result;

        }

        public static List<ActionError> ValidateFastOverdraftApplication(LoanProductOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(Validation.ValidateDocumentNumber(order, order.CustomerNumber));

            result.AddRange(Validation.ValidateOPPerson(order));
            if (order.StartDate == default(DateTime))
            {
                //Սկիզբը նշված չէ
                result.Add(new ActionError(285));
            }

            if (!Utility.IsWorkingDay(order.StartDate))
            {
                //Վարկի սկիզբը աշխատանքային օր չէ
                result.Add(new ActionError(287));
            }

            if (order.EndDate == default(DateTime))
            {
                result.Add(new ActionError(288));
            }


            if (!Utility.IsWorkingDay(order.EndDate))
            {
                //Ոչ աշխատանքային օր
                result.Add(new ActionError(290));
            }

            if (order.Amount == 0)
            {
                //Վարկի գումարը մուտքագրված չէ
                result.Add(new ActionError(293));
            }


            if (!Utility.IsCorrectAmount(order.Amount, order.Currency))
            {
                result.Add(new ActionError(25));
            }


            Card card = null;
            if (order.Type == OrderType.FastOverdraftApplication)
            {
                card = Card.GetCardWithOutBallance(order.ProductId);
            }
            else if (order.Type == OrderType.LoanApplicationConfirmation)
            {
                LoanApplication application = LoanApplication.GetLoanApplication(order.ProductId, order.CustomerNumber);
                if (application != null)
                {
                    card = Card.GetCardMainData(application.CardNumber);
                }
            }
            if (card == null)
            {
                //Քարտը գտնված չէ
                result.Add(new ActionError(534));
                return result;
            }
            else
            {
                result.AddRange(LoanProductOrder.FastOverdraftValidations(order.CustomerNumber, order.Source, card.CardNumber));
            }

            if (order.Type == OrderType.FastOverdraftApplication && LoanProductOrder.CheckLoanApplication(card.CardNumber))
            {
                //Տվյալ քարտի համար արդեն գոյություն ունի վարկային դիմում
                result.Add(new ActionError(1260));
            }


            if (order.FilialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }


            if (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline && order.Source != SourceType.PhoneBanking && order.user.filialCode != card.FilialCode)
            {
                //Քարտը գրանցված է այլ մասնաճյուղում
                result.Add(new ActionError(716));
            }

            if (order.Type == OrderType.FastOverdraftApplication)
            {
                if (CreditLine.GetCreditLines(order.CustomerNumber, ProductQualityFilter.Opened).Where(x => x.Type == 54).Count() >= 3)
                {
                    if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                    {
                        result.Add(new ActionError(1627));
                    }
                    else
                        result.Add(new ActionError(1626));
                }
            }

            if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)order.Type, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationAnalysis, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի վերլուծության չհաստատված հայտ
                result.Add(new ActionError(1262));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.CancelLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հրաժարման չհաստատված հայտ
                result.Add(new ActionError(1263));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.DeleteLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հեռացման չհաստատված հայտ
                result.Add(new ActionError(1264));
            }


            if (order.Amount <= 0)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Add(new ActionError(22));
            }


            if (InfoDB.CommunicationTypeExistenceFromSAP(order.CustomerNumber) == 0 && order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking)
            {
                //SAP CRM ծրագրում հաճախորդի հետ հաղորդակցման եղանակը ընտրված չէ։
                result.Add(new ActionError(2034));
            }


            return result;

        }

        public static List<ActionError> ValidateCancelLoanApplication(LoanProductOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            LoanApplication application = LoanApplication.GetLoanApplication(order.ProductId, order.CustomerNumber);
            if (application.Quality != 1 && application.Quality != 2)
            {
                //Հրաժարվել հնարավոր է միայն Դիմում կամ Վերլուծություն կարգավիճակով վարկային դիմումը
                result.Add(new ActionError(1256));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)order.Type, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հրաժարման չհաստատված հայտ
                result.Add(new ActionError(1263));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationAnalysis, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի վերլուծության չհաստատված հայտ
                result.Add(new ActionError(1262));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.FastOverdraftApplication, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationConfirmation, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.DeleteLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հեռացման չհաստատված հայտ
                result.Add(new ActionError(1264));
            }


            return result;

        }

        public static List<ActionError> ValidateLoanApplicationAnalysis(LoanProductOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            LoanApplication application = LoanApplication.GetLoanApplication(order.ProductId, order.CustomerNumber);
            if (application.Quality != 1)
            {
                //Վերլուծություն հնարավոր է կատարել միայն Դիմում կարգավիճակով վարկային դիմումը
                result.Add(new ActionError(1255));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)order.Type, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի վերլուծության չհաստատված հայտ
                result.Add(new ActionError(1262));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.FastOverdraftApplication, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationConfirmation, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.CancelLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հրաժարման չհաստատված հայտ
                result.Add(new ActionError(1263));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.DeleteLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հեռացման չհաստատված հայտ
                result.Add(new ActionError(1264));
            }



            return result;

        }

        public static List<ActionError> ValidateDeleteLoanApplication(LoanProductOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            CreditLine creditLine = CreditLine.GetCreditLine(order.ProductId, order.CustomerNumber);
            if (creditLine.Quality != 10)
            {
                //Հեռացնել հնարավոր է միայն Պայմանագիր կարգավիճակով վարկային գիծը
                result.Add(new ActionError(1257));

            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)order.Type, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հեռացման չհաստատված հայտ
                result.Add(new ActionError(1264));
            }

            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.CancelLoanApplication, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի հրաժարման չհաստատված հայտ
                result.Add(new ActionError(1263));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationAnalysis, order.Id))
            {
                //Տվյալ պրոդուկտի համար գոյոթյուն ունի վերլուծության չհաստատված հայտ
                result.Add(new ActionError(1262));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.FastOverdraftApplication, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.LoanApplicationConfirmation, order.Id))
            {
                //Տվյալ քարտի համար գոյոթյուն ունի վարկային դիմումի չհաստատված հայտ
                result.Add(new ActionError(1261));
            }
            else if (LoanProductOrder.IsSecontLoanApplication(order.ProductId, (ushort)OrderType.CreditLineActivation, order.Id))
            {
                //Տվյալ վարկային պրոդուկտի համար գոյություն ունի ակտիվացման չհաստատված հայտ
                result.Add(new ActionError(687));
            }

            return result;

        }

        /// <summary>
        /// Գումարի ստուգում հայտի ուղարկման ժամանակ
        /// </summary>
        /// <param name="accounts"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateOrderAmount(User user, SourceType source, List<KeyValuePair<Account, double>> accounts, OrderType type, string creditAccountNumber = "0", string errorText = null, bool useCreditLine = false, bool OnlySaveAndApprove = false)
        {
            List<ActionError> result = new List<ActionError>();
            for (int i = 0; i < accounts.Count; i++)
            {
                double debitAccountBalance = Account.GetAcccountAvailableBalance(accounts[i].Key.AccountNumber, creditAccountNumber);
                double creditLineBalance = 0;
                double sentOrdersAmounts = Order.GetSentOrdersAmount(accounts[i].Key.AccountNumber, source); // ashxatum e miayn source == AcbaOnline AcbaOnlineXML MobileBanking ArmSoft depqerum
                if (OnlySaveAndApprove && (type == OrderType.RATransfer || type == OrderType.InterBankTransferNonCash || type == OrderType.CashCredit
                  || type == OrderType.ReferenceOrder || type == OrderType.InBankConvertation || type == OrderType.CashCreditConvertation
                  || type == OrderType.SwiftCopyOrder || type == OrderType.FeeForServiceProvided || type == OrderType.Convertation || type == OrderType.EventTicketOrder))
                {
                    debitAccountBalance += Order.GetSentNotConfirmedAmounts(accounts[i].Key.AccountNumber, OrderType.CashDebit);
                }
                if (source != SourceType.SSTerminal || (source == SourceType.SSTerminal && type != OrderType.CashCredit && type != OrderType.CashCreditConvertation))
                {
                    if (accounts[i].Key.IsCardAccount())
                    {
                        Card card = new Card();
                        card = Card.GetCard(accounts[i].Key);
                        if (card.ClosingDate == null)
                        {
                            if (card.CreditLine != null && useCreditLine)
                            {
                                ulong customerNumber = accounts[i].Key.GetAccountCustomerNumber();
                                ulong customerType = Customer.GetCustomerType(customerNumber);
                                if (customerType == 6 && card.CreditLine.Quality != 5 && card.CreditLine.Quality != 11 && !IsDAHKAvailability(customerNumber))
                                {
                                    creditLineBalance = Math.Abs(card.CreditLine.StartCapital) - (Math.Abs(card.CreditLine.CurrentCapital) + Math.Abs(card.CreditLine.OutCapital));
                                }
                            }
                            KeyValuePair<String, double> arcaBalance = card.GetArCaBalance(user.userID);
                            if (arcaBalance.Key == "00")
                            {
                                debitAccountBalance = debitAccountBalance + creditLineBalance;
                            }
                            else if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                            {
                                //Հնարավոր չէ կատարել: Arca մնացորդը տվյալ պահին հասանելի չէ:
                                result.Add(new ActionError(1246));
                                return result;
                            }
                            if (arcaBalance.Key == "00" && arcaBalance.Value <= debitAccountBalance)
                            {
                                debitAccountBalance = arcaBalance.Value;
                            }
                        }
                    }
                    else
                    {
                        if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                        {
                            if (accounts[i].Key.IsDepositAccount())
                            {
                                ActionError depositAccountCheck = ValidationDB.CheckForDepositAccountDebet(accounts[i].Key, (sentOrdersAmounts > 0 ? 0 : Math.Abs(sentOrdersAmounts)) + accounts[i].Value, source, creditAccountNumber);
                                if (depositAccountCheck != null)
                                    result.Add(depositAccountCheck);
                                return result;
                            }
                            else if (accounts[i].Key.IsIPayAccount())  // Կցված քարտով գործարքների համար ստուգման անցում
                            {
                                return result;
                            }
                        }
                    }
                }
                double balance = debitAccountBalance + sentOrdersAmounts;
                if (Convert.ToDecimal(balance) < Convert.ToDecimal(accounts[i].Value))
                {
                    if (string.IsNullOrEmpty(errorText))
                    {
                        errorText = "գործարքը կատարելու";
                    }
                    if ((source == SourceType.MobileBanking || Account.AccountAccessible(accounts[i].Key.AccountNumber, user.AccountGroup)) && type != OrderType.CardLessCashOrder)
                    {
                        //{0} հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար(հասանելի գումար՝ {1},մնացորդ ՝ {2})
                        result.Add(new ActionError(1098, new string[] { accounts[i].Key.AccountNumber.ToString(), balance.ToString("#,0.00") + " " + accounts[i].Key.Currency, (accounts[i].Key.Balance + creditLineBalance).ToString("#,0.00") + " " + accounts[i].Key.Currency, errorText }));
                    }
                    else
                    {
                        //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                        result.Add(new ActionError(1099, new string[] { accounts[i].Key.AccountNumber, errorText }));
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Հայտի ուղարկամ ժամանակ գումարների հավաքագրում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> SetAmountsForCheckBalance(Order order)
        {
            List<ActionError> result = new List<ActionError>();
            string errorText = null;
            List<KeyValuePair<Account, double>> list = new List<KeyValuePair<Account, double>>();
            Account creditAccount = new Account("0");
            bool useCreditLine = false;


            if (order.GetType().Name == "CurrencyExchangeOrder")
            {
                CurrencyExchangeOrder exchangeOrder = (CurrencyExchangeOrder)order;
                Account debitAccount = Account.GetAccount(exchangeOrder.DebitAccount.AccountNumber);
                if (exchangeOrder.DebitAccount.Currency != "AMD")
                {

                    list.Add(new KeyValuePair<Account, double>(exchangeOrder.DebitAccount, exchangeOrder.Amount));
                }
                else
                {
                    list.Add(new KeyValuePair<Account, double>(exchangeOrder.DebitAccount, exchangeOrder.AmountInAmd));
                }
                if (exchangeOrder.Fees != null && exchangeOrder.Fees.Count > 0)
                {
                    exchangeOrder.Fees.ForEach(m =>
                        list.Add(new KeyValuePair<Account, double>(m.Account, m.Amount))
                        );
                }
                List<KeyValuePair<Account, double>> list2 = new List<KeyValuePair<Account, double>>();
                double orderAmount = list.FindAll(m => m.Key.AccountNumber == debitAccount.AccountNumber).Sum(m => m.Value);
                list2.Add(new KeyValuePair<Account, double>(debitAccount, orderAmount));
                list.RemoveAll(m => m.Key.AccountNumber == debitAccount.AccountNumber);
                list.AddRange(list2);

            }
            else if (order.GetType().Name == "PaymentOrder" || order.GetType().Name == "BudgetPaymentOrder")
            {
                PaymentOrder paymentOrder = (PaymentOrder)order;
                useCreditLine = paymentOrder.UseCreditLine;
                Account debitAccount = Account.GetAccount(paymentOrder.DebitAccount.AccountNumber);

                if (order.Source != SourceType.Bank && order.Type == OrderType.Convertation)
                {

                    if (paymentOrder.DebitAccount.Currency == "AMD")
                    {
                        list.Add(new KeyValuePair<Account, double>(paymentOrder.DebitAccount, paymentOrder.Amount * paymentOrder.ConvertationRate));
                    }
                    else
                    {
                        list.Add(new KeyValuePair<Account, double>(paymentOrder.DebitAccount, paymentOrder.Amount));
                    }
                }
                else
                {
                    list.Add(new KeyValuePair<Account, double>(paymentOrder.DebitAccount, paymentOrder.Amount));
                }



                if (paymentOrder.Fees != null && paymentOrder.Fees.Count > 0)
                {

                    foreach (OrderFee fee in paymentOrder.Fees)
                    {
                        if (fee.Type != 8 && fee.Type != 1 && fee.Type != 3 && fee.Type != 5 && fee.Type != 6)
                        {
                            list.Add(new KeyValuePair<Account, double>(fee.Account, fee.Amount));
                        }

                    }
                }
                List<KeyValuePair<Account, double>> list2 = new List<KeyValuePair<Account, double>>();
                double orderAmount = list.FindAll(m => m.Key.AccountNumber == debitAccount.AccountNumber).Sum(m => m.Value);
                list2.Add(new KeyValuePair<Account, double>(debitAccount, orderAmount));
                list.RemoveAll(m => m.Key.AccountNumber == debitAccount.AccountNumber);
                list.AddRange(list2);


                if (paymentOrder.Type == OrderType.RATransfer && paymentOrder.SubType == 3 &&
                    (paymentOrder.Source == SourceType.AcbaOnline || paymentOrder.Source == SourceType.MobileBanking || paymentOrder.ReceiverAccount.IsDepositAccount()))
                {
                    creditAccount = paymentOrder.ReceiverAccount;
                }

            }
            else if (order.GetType().Name == "ReferenceOrder")
            {
                ReferenceOrder referenceOrder = (ReferenceOrder)order;
                Account debitAccount = Account.GetAccount(referenceOrder.FeeAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, referenceOrder.FeeAmount));

            }
            else if (order.GetType().Name == "SwiftCopyOrder")
            {
                SwiftCopyOrder swiftCopyOrder = (SwiftCopyOrder)order;
                Account mainAccount = Account.GetAccount(swiftCopyOrder.FeeAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(mainAccount, swiftCopyOrder.Fees[0].Amount));
            }
            else if (order.GetType().Name == "ChequeBookOrder")
            {
                ChequeBookOrder chequeBookOrder = (ChequeBookOrder)order;
                Account mainAccount = Account.GetAccount(chequeBookOrder.FeeAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(mainAccount, chequeBookOrder.Fees[0].Amount));
            }
            else if (order.GetType().Name == "FeeForServiceProvidedOrder")
            {
                FeeForServiceProvidedOrder feeForServiceProvidedOrder = (FeeForServiceProvidedOrder)order;
                Account feeAccount = Account.GetAccount(feeForServiceProvidedOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(feeAccount, feeForServiceProvidedOrder.Amount));
            }
            else if (order.GetType().Name == "HBActivationOrder")
            {
                HBActivationOrder activationOrder = (HBActivationOrder)order;
                Account debitAccount = Account.GetAccount(activationOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, activationOrder.Amount));
            }
            else if (order.GetType().Name == "LoanProductActivationOrder")
            {
                LoanProductActivationOrder loanProductActivationOrder = (LoanProductActivationOrder)order;

                if (loanProductActivationOrder.Type == OrderType.LoanActivation && loanProductActivationOrder.FeeAccount != null)
                {
                    Loan loan = Loan.GetLoan(loanProductActivationOrder.ProductId, loanProductActivationOrder.CustomerNumber);
                    double totalInsuranceAmount = LoanProductActivationOrder.GetLoanTotalInsuranceAmount(loanProductActivationOrder.ProductId);
                    if (loan.ConnectAccount.AccountNumber != loanProductActivationOrder.FeeAccount.AccountNumber)
                    {
                        Account mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccount.AccountNumber);
                        list.Add(new KeyValuePair<Account, double>(mainAccount, loanProductActivationOrder.FeeAmount + totalInsuranceAmount));
                        errorText = " մ/վճար + " + (totalInsuranceAmount != 0 ? "ապահովագրության մ/վճար գանձելու " : "");
                    }
                    else
                    {
                        Account mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccount.AccountNumber);
                        list.Add(new KeyValuePair<Account, double>(mainAccount, totalInsuranceAmount));
                        errorText = totalInsuranceAmount != 0 ? " ապահովագրության մ/վճար գանձելու " : "";
                    }

                }
                if (loanProductActivationOrder.Type == OrderType.CreditLineActivation)
                {
                    Account mainAccount;
                    CreditLine creditLine = CreditLine.GetCreditLine(loanProductActivationOrder.ProductId, loanProductActivationOrder.CustomerNumber);
                    if (loanProductActivationOrder.FeeAccount != null && loanProductActivationOrder.FeeAmount > 0 && creditLine.Type != 54 && creditLine.Type != 51)
                    {
                        mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccount.AccountNumber);
                        list.Add(new KeyValuePair<Account, double>(mainAccount, loanProductActivationOrder.FeeAmount));

                    }

                    if (loanProductActivationOrder.FeeAccountWithTax != null && loanProductActivationOrder.FeeAmountWithTax > 0 && creditLine.Type != 54)
                    {
                        mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccountWithTax.AccountNumber);
                        list.Add(new KeyValuePair<Account, double>(mainAccount, loanProductActivationOrder.FeeAmount));
                    }

                }

                if (loanProductActivationOrder.Type == OrderType.GuaranteeActivation || loanProductActivationOrder.Type == OrderType.AccreditiveActivation
                    || loanProductActivationOrder.Type == OrderType.FactoringActivation
                    || loanProductActivationOrder.Type == OrderType.PaidGuaranteeActivation || loanProductActivationOrder.Type == OrderType.PaidFactoringActivation)
                {
                    Account mainAccount;
                    if (loanProductActivationOrder.FeeAccount != null && loanProductActivationOrder.FeeAmount > 0)
                    {
                        if (!loanProductActivationOrder.FeeForTrasitAccount)
                            mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccount.AccountNumber);
                        else
                            mainAccount = loanProductActivationOrder.FeeAccount;
                        list.Add(new KeyValuePair<Account, double>(mainAccount, loanProductActivationOrder.FeeAmount));

                    }

                    if (loanProductActivationOrder.FeeAccountWithTax != null && loanProductActivationOrder.FeeAmountWithTax > 0)
                    {
                        if (!loanProductActivationOrder.FeeForTrasitAccount)
                            mainAccount = Account.GetAccount(loanProductActivationOrder.FeeAccountWithTax.AccountNumber);
                        else
                            mainAccount = loanProductActivationOrder.FeeAccountWithTax;
                        list.Add(new KeyValuePair<Account, double>(mainAccount, loanProductActivationOrder.FeeAmount));
                    }

                }

            }
            else if (order.GetType().Name == "UtilityPaymentOrder")
            {
                UtilityPaymentOrder utilityPaymentOrder = (UtilityPaymentOrder)order;
                useCreditLine = utilityPaymentOrder.UseCreditLine;
                Account debitAccount = Account.GetAccount(utilityPaymentOrder.DebitAccount.AccountNumber);

                if (utilityPaymentOrder.CommunalType == CommunalTypes.Gas)
                    list.Add(new KeyValuePair<Account, double>(debitAccount, utilityPaymentOrder.Amount + utilityPaymentOrder.ServiceAmount));
                else
                    list.Add(new KeyValuePair<Account, double>(debitAccount, utilityPaymentOrder.Amount));
            }
            else if (order.GetType().Name == "ReestrUtilityPaymentOrder")
            {
                ReestrUtilityPaymentOrder utilityPaymentOrder = (ReestrUtilityPaymentOrder)order;
                Account debitAccount = Account.GetAccount(utilityPaymentOrder.DebitAccount.AccountNumber);

                if (utilityPaymentOrder.CommunalType == CommunalTypes.Gas)
                    list.Add(new KeyValuePair<Account, double>(debitAccount, utilityPaymentOrder.Amount + utilityPaymentOrder.ServiceAmount));
                else
                    list.Add(new KeyValuePair<Account, double>(debitAccount, utilityPaymentOrder.Amount));

            }
            else if (order.GetType().Name == "DepositCaseOrder")
            {

                order.Fees.ForEach(m =>
                {
                    if (m.Type == 24)
                    {
                        DepositCaseOrder depositCaseOrder = (DepositCaseOrder)order;
                        Account feeAccount = Account.GetAccount(m.Account.AccountNumber);
                        list.Add(new KeyValuePair<Account, double>(feeAccount, depositCaseOrder.Amount));

                    }

                });

            }
            else if (order.GetType().Name == "DepositCasePenaltyMatureOrder")
            {
                DepositCasePenaltyMatureOrder depositCasePenaltyMatureOrder = (DepositCasePenaltyMatureOrder)order;
                Account debitAccount = Account.GetAccount(depositCasePenaltyMatureOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, depositCasePenaltyMatureOrder.Amount));
            }
            else if (order.GetType().Name == "InsuranceOrder")
            {
                InsuranceOrder insuranceOrder = (InsuranceOrder)order;
                Account debitAccount = Account.GetAccount(insuranceOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, insuranceOrder.Amount));
            }
            if (order.GetType().Name == "ReestrTransferOrder")
            {
                ReestrTransferOrder reestrTransferOrder = (ReestrTransferOrder)order;
                Account debitAccount = Account.GetAccount(reestrTransferOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(reestrTransferOrder.DebitAccount, reestrTransferOrder.Amount));
                if (reestrTransferOrder.Fees != null && reestrTransferOrder.Fees.Count > 0)
                {
                    reestrTransferOrder.Fees.ForEach(m =>
                        list.Add(new KeyValuePair<Account, double>(m.Account, m.Amount))
                        );
                }
                List<KeyValuePair<Account, double>> list2 = new List<KeyValuePair<Account, double>>();
                double orderAmount = list.FindAll(m => m.Key.AccountNumber == debitAccount.AccountNumber).Sum(m => m.Value);
                list2.Add(new KeyValuePair<Account, double>(debitAccount, orderAmount));
                list.RemoveAll(m => m.Key.AccountNumber == debitAccount.AccountNumber);
                list.AddRange(list2);

            }
            else if (order.GetType().Name == "PhoneBankingContractActivationOrder")
            {
                PhoneBankingContractActivationOrder pbActivationOrder = (PhoneBankingContractActivationOrder)order;
                Account debitAccount = Account.GetAccount(pbActivationOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, pbActivationOrder.Amount));
            }
            else if (order.GetType().Name == "CredentialActivationOrder")
            {
                CredentialActivationOrder credentialActivationOrder = (CredentialActivationOrder)order;
                Account debitAccount = Account.GetAccount(credentialActivationOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, credentialActivationOrder.Amount));
            }
            else if (order.GetType().Name == "CredentialOrder")
            {
                CredentialOrder credentialOrder = (CredentialOrder)order;

                double fee = order.Fees[0].Amount;
                Account feeAccount = Account.GetAccount(credentialOrder.Fees[0].Account.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(feeAccount, fee));
            }

            else if (order.GetType().Name == "TransitPaymentOrder")
            {
                TransitPaymentOrder transitPaymentOrder = (TransitPaymentOrder)order;
                Account debitAccount = Account.GetAccount(transitPaymentOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, transitPaymentOrder.Amount));
            }

            else if (order.GetType().Name == "CardlessCashoutOrder")
            {
                CardlessCashoutOrder cardlessCashoutOrder = (CardlessCashoutOrder)order;
                useCreditLine = cardlessCashoutOrder.UseCreditLine;
                Account debitAccount = Account.GetAccount(cardlessCashoutOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(cardlessCashoutOrder.DebitAccount, cardlessCashoutOrder.Amount));

                if (cardlessCashoutOrder.Fees != null && cardlessCashoutOrder.Fees.Count > 0)
                {

                    foreach (OrderFee fee in cardlessCashoutOrder.Fees)
                    {
                        if (fee.Type != 8 && fee.Type != 1 && fee.Type != 3 && fee.Type != 5 && fee.Type != 6)
                        {
                            list.Add(new KeyValuePair<Account, double>(fee.Account, fee.Amount));
                        }

                    }
                }

                List<KeyValuePair<Account, double>> list2 = new List<KeyValuePair<Account, double>>();
                double orderAmount = list.FindAll(m => m.Key.AccountNumber == debitAccount.AccountNumber).Sum(m => m.Value);
                list2.Add(new KeyValuePair<Account, double>(debitAccount, orderAmount));
                list.RemoveAll(m => m.Key.AccountNumber == debitAccount.AccountNumber);
                list.AddRange(list2);

            }
            else if (order.GetType().Name == "VehicleInsuranceOrder")
            {
                VehicleInsuranceOrder vehicleInsuranceOrder = (VehicleInsuranceOrder)order;
                Account debitAccount = Account.GetAccount(vehicleInsuranceOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, vehicleInsuranceOrder.Amount));
            }
            else if (order.GetType().Name == "EventTicketOrder")
            {
                EventTicketOrder vehicleInsuranceOrder = (EventTicketOrder)order;
                Account debitAccount = Account.GetAccount(vehicleInsuranceOrder.DebitAccount.AccountNumber);
                list.Add(new KeyValuePair<Account, double>(debitAccount, vehicleInsuranceOrder.Amount));
            }
            if (order.Source != SourceType.SberBankTransfer && order.Source != SourceType.AcbaMat)
                result.AddRange(ValidateOrderAmount(order.user, order.Source, list, order.Type, creditAccount.AccountNumber, errorText, useCreditLine, order.OnlySaveAndApprove));
            return result;
        }

        internal static List<ActionError> ValidateFeeForServiceProvidedOrder(FeeForServiceProvidedOrder order, User user)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.Type == OrderType.CashFeeForServiceProvided)
            {
                result.AddRange(Validation.ValidateCashOperationAvailability(order, user));
            }


            if (order.DebitAccount.Currency != "AMD")
            {
                //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                result.Add(new ActionError(669));
                return result;
            }



            if (order.Amount < 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(order.Amount, order.DebitAccount.Currency))
            {
                result.Add(new ActionError(25));
            }

            if (order.Type == OrderType.FeeForServiceProvided)
            {
                int vipType = 0;

                var VipData = ACBAOperationService.GetCustomerVipData(order.CustomerNumber);
                vipType = VipData.vipType.key;

                if (vipType == 7 || vipType == 8 || vipType == 9)
                {
                    if (order.ServiceType == 202 || order.ServiceType == 204 || order.ServiceType == 205 || order.ServiceType == 209 || order.ServiceType == 210 || order.ServiceType == 211 || order.ServiceType == 213 || order.ServiceType == 214 || order.ServiceType == 215 || order.ServiceType == 221 || order.ServiceType == 222)
                    {
                        //Տվյալ հաճախորդի համար այս սակագինը 0-է:
                        result.Add(new ActionError(670));
                    }
                }
            }
            else
            {
                if (order.ServiceType == 207 || order.ServiceType == 601 || order.ServiceType == 603 || order.ServiceType == 604 || order.ServiceType == 607 || order.ServiceType == 608 || order.ServiceType == 609 || order.ServiceType == 612 || order.ServiceType == 613)
                {
                    if (order.CustomerResidence <= 0)
                    {
                        //Ընտրեք ռեզիդենտությունը
                        result.Add(new ActionError(671));
                    }
                }
            }


            if (result.Count > 0)
            {
                return result;
            }

            if (order.Type == OrderType.FeeForServiceProvided)
            {
                result.AddRange(Validation.ValidateDebitAccount(order, order.DebitAccount));
            }

            return result;
        }

        public static List<ActionError> ValidateLoanActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);

            Loan product;
            product = Loan.GetLoan(order.ProductId, order.CustomerNumber);

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            if (customerType == (short)CustomerTypes.physical)
            {
                if (LoanProduct.CheckForEffectiveSign(product.LoanType) == 1)
                {
                    if ((product.InterestRateEffectiveWithOnlyBankProfit == 0 && product.LoanType != 38) || (product.InterestRateEffectiveWithOnlyBankProfit == null && product.LoanType == 38))
                    {
                        result.Add(new ActionError(1630));
                    }
                    if (product.InterestRateEffectiveWithOnlyBankProfit > 0.24)
                    {
                        result.Add(new ActionError(1629));
                    }
                }
            }



            if (product.LoanType != 38 && order.Source != SourceType.EContract)
            {
                result.AddRange(CheckCustomerDebts(order.CustomerNumber));
            }

            if (LoanProductActivationOrder.SignedOutOfBankCheck(order.ProductId))
            {
                //Պայմանագիրը ստորագրվել է Բանկի տարածքից դուրս, անհրաժեշտ է լրացնել Բանկի անունից ներկա գտնված աշխատակցի ՊԿ-ն
                result.Add(new ActionError(1534));
            }

            if (provisions.Count == 0 && product.LoanType != 33 && product.LoanType != 34 && product.LoanType != 35 && product.LoanType != 49)
            {
                ////Գրավը բացակայում է
                result.Add(new ActionError(677));
            }

            if (order.Source == SourceType.EContract)
            {
                if (product.StartDate != Utility.GetNextOperDay())
                {
                    //Ստուգեք վարկի տրման օրը
                    result.Add(new ActionError(678));
                }
            }
            else
            {
                if (product.StartDate != Utility.GetCurrentOperDay())
                {
                    //Ստուգեք վարկի տրման օրը
                    result.Add(new ActionError(678));
                }
            }

            //TO DO: 
            if (order.Source != SourceType.EContract && product.FillialCode != order.FilialCode && product.LoanType != 38)
            {
                ////Վարկը կարելի է ձևակերպել միայն @var1 մասնաճյուղից
                result.Add(new ActionError(679, new string[] { product.FillialCode.ToString() }));
            }

            if (product.InterestRate > LoanProduct.GetPenaltyRateForDate(DateTime.Now.Date) * 2)
            {
                //Տոկոսադրույքը բարձր է քան կրկնակի ԿԲ վերաֆինանսավորման տոկոսադրույքը
                result.Add(new ActionError(684));
            }
            if (order.Source != SourceType.EContract)
            {
                result.AddRange(ValidateCustomerSignature(order.OPPerson.CustomerNumber));
            }

            if (Account.GetAcccountAvailableBalance(product.ConnectAccount.AccountNumber) < 0)
            {
                //Հաշվի մնացորդը փոքր է 0-ից
                result.Add(new ActionError(680));
            }

            List<LoanRepaymentGrafik> grafik = product.GetLoanGrafik();

            if (grafik == null)
            {
                //Մարման գրաֆիկը բացակայում է'
                result.Add(new ActionError(681));
            }
            else
            {
                if (grafik.Count == 0)
                {
                    //Մարման գրաֆիկը բացակայում է'
                    result.Add(new ActionError(681));
                }
                if (grafik.Last().RepaymentDate != product.EndDate)
                {
                    //Վարկի մարման ա/թ-ը չի համապատասխանում գրաֆիկի վերջին ա/թ
                    result.Add(new ActionError(682));
                }
            }

            if (order.IsSecondActivation())
            {
                result.Add(new ActionError(687));
            }

            if (ACBAOperationService.HasCustomerArrest(order.CustomerNumber))
            {
                result.Add(new ActionError(688));
            }

            double kurs = Utility.GetCBKursForDate((DateTime)order.OperationDate, product.Currency);
            if (!LoanProductActivationOrder.CheckLoanDocumentAttachment(product.LoanType, product.ProductId, (int)order.Source, product.StartCapital * kurs, order.CustomerNumber))
            {
                result.Add(new ActionError(750));
            }


            if (!order.user.IsChiefAcc && order.Source != SourceType.EContract)
            {

                if (provisions.Exists(m => m.Currency != product.Currency && m.Type == 13) && product.LoanType != 29)
                {
                    result.Add(new ActionError(756));
                }

                foreach (var provision in provisions)
                {
                    if (provision.OutBalance.Substring(0, 4) == "8121" || provision.OutBalance.Substring(0, 4) == "8122")
                    {
                        if (product.Currency != provision.Currency)
                        {
                            result.Add(new ActionError(757));
                        }

                        if (provision.Amount * kurs > 35000000)
                        {
                            result.Add(new ActionError(758));
                        }
                    }
                }



                // հանվել է համաձայն 7632 առաջարկի
                //result.AddRange(LoanProductActivationOrder.LoanActivationValidation(product, order));
            }


            if (order.Source != SourceType.EContract && product.DayOfRateCalculation != null && product.DayOfRateCalculation != order.OperationDate)
            {
                result.Add(new ActionError(761));
            }

            if (order.FeeAmount > 0 && order.FeeAccount == null)
            {
                result.Add(new ActionError(90));
            }

            if (order.Source == SourceType.EContract)
            {
                if (IsDAHKAvailability(order.CustomerNumber))
                {
                    //Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ
                    result.Add(new ActionError(516, new string[] { "" }));
                }
            }

            if (product.LoanType != 3 && product.LoanType != 7 && product.LoanType != 10 && product.LoanType != 59)
            {
                if (CheckCustomerPhoneNumber(order.CustomerNumber) && order.Source == SourceType.Bank)
                {
                    result.Add(new ActionError(1904));
                }
                List<ulong> loanOwners = LoanProductActivationOrder.GetOwnerCustomerNumbers(product.ProductId, "provision_owners");
                foreach (var item in loanOwners)
                {
                    if (CheckCustomerPhoneNumber(item) && order.Source == SourceType.Bank)
                        result.Add(new ActionError(1904));
                }
            }

            return result;


        }

        public static List<ActionError> ValidateCreditLineActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);

            if (provisions.Count == 0)
            {
                //Գրավը բացակայում է
                result.Add(new ActionError(677));
            }
            CreditLine product;

            product = CreditLine.GetCreditLine(order.ProductId, order.CustomerNumber);

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            if (customerType == (short)CustomerTypes.physical)
            {
                if (LoanProduct.CheckForEffectiveSign(product.Type) == 1)
                {

                    if (product.InterestRateEffectiveWithOnlyBankProfit == 0)
                    {
                        result.Add(new ActionError(1630));
                    }

                    if (product.InterestRateEffectiveWithOnlyBankProfit > 0.24)
                    {
                        result.Add(new ActionError(1629));
                    }
                }

            }




            if (order.Source == SourceType.EContract)
            {
                if (product.StartDate != Utility.GetNextOperDay())
                {
                    //Ստուգեք վարկի տրման օրը
                    result.Add(new ActionError(678));
                }
            }
            else
            {
                if (product.StartDate != Utility.GetCurrentOperDay())
                {
                    //Ստուգեք վարկի տրման օրը
                    result.Add(new ActionError(678));
                }
            }
            if (product.DayOfRateCalculation != null && product.DayOfRateCalculation != Utility.GetNextOperDay())
            {
                //Ստուգեք վարկի տոկոսագումարի հաշվարկի օրը
                result.Add(new ActionError(1014));
            }

            if (order.FilialCode == 22059 && product.FillialCode == 22000)
            {
                order.user.filialCode = (ushort)product.FillialCode;
            }

            //TO DO 2: Ինչի է 2 հատ:
            if (order.Source != SourceType.PhoneBanking && product.FillialCode != order.user.filialCode && order.Source != SourceType.EContract)
            {
                //Վարկը կարելի է ձևակերպել միայն @var1 մասնաճյուղից
                result.Add(new ActionError(679, new string[] { product.FillialCode.ToString() }));
            }

            if (product.InterestRate > LoanProduct.GetPenaltyRateForDate(DateTime.Now) * 2)
            {
                //Տոկոսադրույքը բարձր է քան կրկնակի ԿԲ վերաֆինանսավորման տոկոսադրույքը
                result.Add(new ActionError(684));
            }

            if (order.Source != SourceType.EContract)
            {
                result.AddRange(ValidateCustomerSignature(order.OPPerson.CustomerNumber));
            }

            if (Account.GetAcccountAvailableBalance(product.ConnectAccount.AccountNumber) < 0)
            {
                //Հաշվի մնացորդը փոքր է 0-ից
                result.Add(new ActionError(680, new string[] { product.ConnectAccount.AccountNumber }));
            }

            if (order.SubType == 1)
            {
                Card card = Card.GetCardWithOutBallance(product.ConnectAccount.AccountNumber);

                if (product.Type != 50 && product.Type != 51 && product.Type != 52 && (card.Type != 36 && card.Type != 37) && product.Type != 54
                    && !(card.Type == 11 && card.RelatedOfficeNumber == 2405))//ստուգումը ավելացել է 28/10/17 Լիանա
                {
                    if (product.EndDate != card.EndDate)
                    {
                        //Վարկային գծի վերջի ա/թ չի համապատասխանում քարտի վերջի ա/թ հետ
                        result.Add(new ActionError(685));
                    }

                }
                else if (product.EndDate > card.EndDate)
                {
                    //Վարկային գծի վերջը գերազանցում է քարտի վերջի ա/թ
                    result.Add(new ActionError(686));
                }
            }
            if (product.InterestRateEffective == 0 && product.InterestRate != 0)
            {
                //Էֆֆեկտիվ(փաստացի) տոկոսադրույքը հաշվարկված չի
                result.Add(new ActionError(683));
            }

            if (order.IsSecondActivation())
            {
                result.Add(new ActionError(687));
            }

            if (ACBAOperationService.HasCustomerArrest(order.CustomerNumber))
            {
                result.Add(new ActionError(688));
            }

            double kurs = Utility.GetCBKursForDate((DateTime)order.OperationDate, product.Currency);
            if (!LoanProductActivationOrder.CheckLoanDocumentAttachment(product.Type, product.ProductId, (int)order.Source, product.StartCapital * kurs, order.CustomerNumber))
            {
                result.Add(new ActionError(750));
            }
            List<ulong> customernumbers = LoanProductActivationOrder.GetOwnerCustomerNumbers(product.ProductId, "provision_owners");
            if (customernumbers.Count != 0)
            {
                foreach (var customerNumber in customernumbers)
                {
                    result.AddRange(Validation.ValidateCustomerSignature(customerNumber));
                }
            }

            if (order.Source == SourceType.EContract)
            {
                if (IsDAHKAvailability(order.CustomerNumber))
                {
                    //Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ
                    result.Add(new ActionError(516, new string[] { "" }));
                }
            }

            if (product.Type != 18 && product.Type != 25 && product.Type != 60)
            {
                if (CheckCustomerPhoneNumber(order.CustomerNumber) && order.Source == SourceType.Bank)
                {
                    result.Add(new ActionError(1904));
                }
                List<ulong> loanOwners = LoanProductActivationOrder.GetOwnerCustomerNumbers(product.ProductId, "provision_owners");
                foreach (var item in loanOwners)
                {
                    if (CheckCustomerPhoneNumber(item) && order.Source == SourceType.Bank)
                        result.Add(new ActionError(1904));
                }
            }

            return result;
        }

        /// <summary>
        /// Քարտային հաշվի չվճարված դրական տոկոսագումարի վճարման հայտի ստուգումներ
        /// </summary>
        /// <param name="order">Վճարման հայտ</param>
        /// <returns>Ստուգման արդյունքում ի հայտ եկած անճշտություններ</returns>
        internal static List<ActionError> ValidateCardUnpaidPercentPaymentOrder(CardUnpaidPercentPaymentOrder order)
        {
            List<ActionError> errors = new List<ActionError>();

            if (order.Card == null) //Եթե քարտ գոյություն չունի(գտնված չէ) հայտը անվավեր է
            {
                errors.Add(new ActionError(672));
                return errors;
            }

            if (order.Card.IsSupplementary) //Եթե կից քարտ է հայտը անվավեր է
            {
                errors.Add(new ActionError(673));
                return errors;
            }

            if (order.Account == null || (!String.IsNullOrEmpty(order.Account.Currency) && !order.Account.Currency.Equals("AMD"))) //Եթե դրամային հաշվեհամար չէ հայտը անվավեր է
            {
                errors.Add(new ActionError(674));
                return errors;
            }

            if (order.Card.FilialCode == 22000 && order.user.filialCode == 22059)
            {
                order.user.filialCode = (ushort)order.Card.FilialCode;
            }

            if (order.Card.FilialCode != order.user.filialCode) //Եթե այլ մասնաճուղ է հայտը անվավեր է
            {
                errors.Add(new ActionError(675));
                return errors;
            }

            Card card = Card.GetCardMainData((ulong)order.Card.ProductId, order.CustomerNumber);

            if (card == null) //Նշված քարտը հաճախորդին չի պատկանում
            {
                errors.Add(new ActionError(694, new string[] { order.CustomerNumber.ToString(), order.Card.CardNumber }));
                return errors;
            }


            if (!order.Card.Currency.Equals(card.Currency)) //Մուտքագրված արժույթը սխալ է
            {
                errors.Add(new ActionError(700));
                return errors;
            }

            if (order.Card.PositiveRate != card.PositiveRate) //Վճարվող տոկոսագումարի անհամապատասխանություն
            {
                errors.Add(new ActionError(695, new string[] { Math.Round(order.Card.PositiveRate).ToString(), Math.Round(card.PositiveRate, 1).ToString() }));
                return errors;
            }

            return errors;
        }

        /// <summary>
        /// Գործարքի հեռացման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateRemovalOrder(RemovalOrder order, ushort operationFilialCode)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (result.Count() > 0)
            {
                return result;
            }

            if (order.RemovingReason == 0)
            {
                //Հեռացման պատճառն ընտրված չէ:
                result.Add(new ActionError(689));
                return result;
            }
            else if (order.RemovingReason == 9 && string.IsNullOrEmpty(order.RemovingReasonAdd))
            {
                //Հեռացման այլ պատճառը լրացված չէ:
                result.Add(new ActionError(690));
                return result;
            }
            else if (RemovalOrderDB.IsSecondRemoving(order.CustomerNumber, order.RemovingOrderId))
            {
                //Տվյալ ձևակերպման համար հեռացման հայտն արդեն մուտքագրված է:
                result.Add(new ActionError(691));
                return result;
            }

            Order removableOrder = Order.GetOrder(order.RemovingOrderId);

            if (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline && Order.CheckOrderHasCustomerNumber(order.RemovingOrderId) && order.CustomerNumber == 0 && removableOrder.Type != OrderType.CardToOtherCardsOrder)
            {
                //Տվյալ հայտը անհրաժեշտ է հեռացնել հաճախորդի պրոդուկտներ բաժնից
                result.Add(new ActionError(1046));
                return result;
            }



            if (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline && order.Source != SourceType.AcbaOnlineXML
                && order.Source != SourceType.ArmSoft && removableOrder.FilialCode != operationFilialCode)
            {
                //Հեռացնել կարելի է միայն ձեր մասնաճյուղի ձևակերպումները:
                result.Add(new ActionError(692));
                return result;
            }
            else if (Utility.GetNextOperDay() != removableOrder.ConfirmationDate && order.Type == OrderType.RemoveTransaction && removableOrder.Type != OrderType.LinkPaymentOrder)
            {
                //Հեռացնել կարելի է միայն տվյալ գործառնական օրվա ձևակերպումները:
                result.Add(new ActionError(693));
                return result;
            }

            if (order.Type == OrderType.CancelTransaction && removableOrder.Quality != OrderQuality.Sent3 && removableOrder.Type != OrderType.RemittanceCancellationOrder && removableOrder.Type != OrderType.LinkPaymentOrder && removableOrder.Type != OrderType.ConsumeLoanApplicationOrder && !(removableOrder.Type == OrderType.FastTransferPaymentOrder && removableOrder.SubType == 23))
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                result.Add(new ActionError(921));
                return result;
            }
            else if (order.Type == OrderType.CancelTransaction && removableOrder.Quality != OrderQuality.TransactionLimitApprovement && removableOrder.Type == OrderType.RemittanceCancellationOrder && !(removableOrder.Type == OrderType.FastTransferPaymentOrder && removableOrder.SubType == 23) && !(removableOrder.Quality != OrderQuality.Completed && removableOrder.Type == OrderType.LinkPaymentOrder))
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                result.Add(new ActionError(921));
                return result;
            }
            if (order.Type == OrderType.RemoveTransaction && removableOrder.Quality != OrderQuality.Completed)
            {
                //Հնարավոր չէ կազմել հեռացման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                result.Add(new ActionError(922));
                return result;
            }

            if ((removableOrder.Source == SourceType.MobileBanking || removableOrder.Source == SourceType.AcbaOnline) && removableOrder.Type == OrderType.ReceivedFastTransferPaymentOrder)
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ տեսակի գործարքի համար:
                result.Add(new ActionError(1699));
                return result;
            }

            if (removableOrder.Quality == OrderQuality.Completed && removableOrder.Type == OrderType.ConsumeLoanApplicationOrder && ConsumeLoanApplicationOrder.CheckConsumeLoanApplicationAppId(order.RemovingOrderId))
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ տեսակի գործարքի համար:
                result.Add(new ActionError(1699));
                return result;
            }

            if (removableOrder.Quality == OrderQuality.Draft && (removableOrder.Type == OrderType.ConsumeLoanApplicationOrder || removableOrder.Type == OrderType.ConsumeLoanSettlementOrder))
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                result.Add(new ActionError(921));
                return result;
            }

            if ((removableOrder.Quality == OrderQuality.Draft || removableOrder.Quality == OrderQuality.Canceled || removableOrder.Quality == OrderQuality.Declined) && (removableOrder.Type == OrderType.ConsumeLoanApplicationOrder || removableOrder.Type == OrderType.ConsumeLoanSettlementOrder))
            {
                //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                result.Add(new ActionError(921));
                return result;
            }

            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
            {
                if (removableOrder.CustomerNumber != order.CustomerNumber)
                {
                    //Հնարավոր չէ կազմել հրաժարման հայտ տվյալ կարգավիճակով գործարքի համար,ստուգեք գործարքի կարգավիճակը:
                    result.Add(new ActionError(921));
                    return result;
                }
            }

            if (order.Source == SourceType.AcbaOnline
                || order.Source == SourceType.AcbaOnlineXML
                || order.Source == SourceType.ArmSoft
                || order.Source == SourceType.MobileBanking)
            {
                if (removableOrder.Type == OrderType.RATransfer && removableOrder.SubType == 1)
                {
                    if (removableOrder.Source == SourceType.AcbaOnline
                || removableOrder.Source == SourceType.AcbaOnlineXML
                || removableOrder.Source == SourceType.ArmSoft
                || removableOrder.Source == SourceType.MobileBanking)
                    {
                        if (removableOrder.Quality == OrderQuality.SBQprocessed || removableOrder.Quality == OrderQuality.Completed)
                        {
                            //Հնարավոր չէ հեռացնել
                            result.Add(new ActionError(2064));
                            return result;
                        }
                    }
                }
            }

            return result;

        }

        /// <summary>
        /// Չեկային գրքույկի ստացման  հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateChequeBookReceiveOrder(ChequeBookReceiveOrder order)
        {


            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (order.ChequeBookAccount == null)
            {
                //ընտրեք հաշվեհամարը:
                result.Add(new ActionError(55));
            }
            else
            {
                Account mainAccount = Account.GetAccount(order.ChequeBookAccount.AccountNumber);
                if (mainAccount == null)
                {
                    //ընտրեք  հաշվեհամարը:
                    result.Add(new ActionError(300));
                }
                else
                {
                    if (Order.IsExistRequest(OrderType.ChequeBookReceiveOrder, order.ChequeBookAccount.AccountNumber, order.CustomerNumber) == true)
                    {
                        //Տվյալ ընթացիք հաշվի համար գոյություն ունի չեկային գրքույքի չհաստատված հայտ:
                        result.Add(new ActionError(397));
                    }
                }

            }
            int pageNumberStart = Convert.ToInt32(order.PageNumberStart);
            int pageNumberEnd = Convert.ToInt32(order.PageNumberEnd);

            if (pageNumberStart <= 0)
            {
                // Էջի սկիզբը ընտրված չէ
                result.Add(new ActionError(705));
            }
            if (pageNumberEnd <= 0)
            {
                // Էջի վերջը ընտրված չէ
                result.Add(new ActionError(705));
            }
            if (pageNumberStart >= 0 && pageNumberEnd >= 0)
            {
                if (pageNumberStart > pageNumberEnd)
                {
                    //Էջի սկիզբը  պետք է փոքր լինի էջի վերջից
                    result.Add(new ActionError(707));
                }
            }



            return result;
        }

        /// <summary>
        /// Լիազորագրի հայտի ստուգումներ
        /// </summary>
        /// <param name="order">Լիազորագիր</param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCredentialOrder(CredentialOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();

            short customerFilialcode = Customer.GetCustomerFilial(order.CustomerNumber).key;

            if (order.Credential.StartDate == null)
            {
                //Լիազորագրի սկիզբը նշված չէ:
                result.Add(new ActionError(1288));
            }

            if ((order.Credential.CredentialType == (short)CredentialType.Credentials || order.Credential.CredentialType == (short)CredentialType.Signature) && customerFilialcode != user.filialCode && order.Source != SourceType.AcbaOnline && order.Credential.GivenByBank)
            {
                //Հաճախորդը այլ մասնաճյուղից է։
                result.Add(new ActionError(1075));
            }

            byte customerType = Customer.GetCustomerType(order.CustomerNumber);
            if (order.Type == OrderType.CredentialOrder && order.Credential.GivenByBank && customerFilialcode != user.filialCode && customerType == (int)CustomerTypes.physical)
            {
                result.Add(new ActionError(1686));
            }

            if (order.Credential.StartDate != null && order.Credential.EndDate != null && order.Credential.StartDate > order.Credential.EndDate)
            {
                //Սկիզբը մեծ է վերջից։
                result.Add(new ActionError(728));
            }


            if (order.Credential.StartDate != null && order.Credential.EndDate != null && order.Credential.StartDate.Value.AddYears(3) < order.Credential.EndDate)
            {
                //Լիազորագրի ժամկետը չի կարող լինել երեք տարուց ավել։
                result.Add(new ActionError(729));
            }

            if (order.Credential.CredentialType == 0)
            {
                if (order.Credential.StartDate.Value.AddDays(30) < order.RegistrationDate && order.Credential.GivenByBank)
                {
                    //Լիազորագրի գործողության սկիզբը չի կարող 30 օրից ավել լինել գրանցման ամսաթվից։
                    result.Add(new ActionError(731));
                }

                if (order.Credential.StartDate > order.RegistrationDate)
                {
                    //Լիազորագրի գործողության սկիզբը չի կարող մեծ լինել գրանցման ամսաթվից։
                    result.Add(new ActionError(732));
                }

                if (order.Credential.StartDate != null && order.Credential.EndDate != null && order.Credential.StartDate.Value.Date == order.Credential.EndDate.Value.Date)
                {
                    //Սկիզբը հավասար է վերջին։
                    result.Add(new ActionError(733));
                }
            }

            if (order.Credential.StartDate != null && order.Credential.CredentialType != 0 && order.Credential.StartDate.Value.Date < order.RegistrationDate.Date && order.Credential.GivenByBank)
            {
                //Սկիզբը այսօրվանից փոքր է։
                result.Add(new ActionError(734));
            }

            if (order.Credential.EndDate != null && order.Credential.EndDate.Value.Date < order.RegistrationDate.Date)
            {
                //Վերջը այսօրվանից փոքր է։
                result.Add(new ActionError(735));
            }

            if ((order.Credential.AssigneeList == null || order.Credential.AssigneeList.Count() == 0) && (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline))
            {
                //Լիազորված անձ մուտքագրված չէ։
                result.Add(new ActionError(736));

            }
            else
            {
                if (order.Source == SourceType.AcbaOnline)
                {
                    if (order.Credential.EndDate == null)
                    {
                        //Լիազորագրի ավարտը նշված չէ:
                        result.Add(new ActionError(1289));
                    }

                    if (order.Credential.AssigneeList[0].AssigneeDocumentType == -1)
                    {
                        //Փաստաթղթի տեսակը ընտրված չէ:
                        result.Add(new ActionError(1328));
                    }

                    if (order.Attachments == null || order.Attachments.Count < 1)
                    {
                        //Լիազորված անձի նույնականացման փաստաթուղթը կցված չէ:
                        result.Add(new ActionError(1554));
                    }



                }

            }

            if (order.Credential.AssigneeList != null && order.Credential.AssigneeList.Count() != 0)
            {
                foreach (Assignee assignee in order.Credential.AssigneeList)
                {
                    if (assignee.OperationList == null || assignee.OperationList.Count() == 0)
                    {
                        //Լիազորված անձի ոչ մի գործողություն նշված չէ։
                        result.Add(new ActionError(746));
                    }
                }

            }

            if (order.Source == SourceType.AcbaOnline)
            {
                foreach (OrderFee fee in order.Fees)
                {
                    if (fee.Account == null || String.IsNullOrEmpty(fee.Account.AccountNumber))
                    {
                        //Միջնորդավճարի հաշիվն ընտրված չէ
                        result.Add(new ActionError(300));
                    }
                }
            }

            if (order.Source == SourceType.Bank && order.Credential.CredentialType == 0 && !order.Credential.GivenByBank && order.Credential.AssigneeList.Find(a => a.OperationList.Find(o => o.GroupId == 5 || o.GroupId == 6) != null) != null)
            {
                foreach (Assignee assignee in order.Credential.AssigneeList)
                {
                    //Վարկառուն/Համավարկառուն չի կարող հանդիսանալ Լիազորված անձ
                    if (assignee.CustomerNumber == order.CustomerNumber)
                        result.Add(new ActionError(1601));
                }

                //Լիազորագրի տվյալներում լիզորագրի վավերացման/տրման ամսաթիվը լրացված չէ
                if (order.Credential.CredentialGivenDate == null)
                {
                    result.Add(new ActionError(1602));
                }


                if (customerType == 6 || order.Credential.HasNotarAuthorization == true)
                {
                    //Լիազորագրի տվյալներում "Նոտար" դաշտը լրացված չէ
                    if (order.Credential.Notary == null)
                    {
                        result.Add(new ActionError(1603));
                    }

                    //Լիազորագրի տվյալներում նոտարական տարածքը լրացված չէ
                    if (order.Credential.NotaryTerritory == null)
                    {
                        result.Add(new ActionError(1604));
                    }

                    //Լիազորագրի տվյալներում սեղանամատյանի համարը լրացված չէ
                    if (order.Credential.LedgerNumber == null)
                    {
                        result.Add(new ActionError(1605));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Լիազորագրի դադարեցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order">Լիազորագիր</param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCredentialTerminationOrder(CredentialTerminationOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.Type == OrderType.CredentialTerminationOrder)
            {
                short customerFilialcode = Customer.GetCustomerFilial(order.CustomerNumber).key;

                if ((order.CredentialType == (short)CredentialType.Credentials || order.CredentialType == (short)CredentialType.Signature) && customerFilialcode != user.filialCode)
                {
                    //Հաճախորդը այլ մասնաճյուղից է։
                    result.Add(new ActionError(1073));
                }

                if (order.ClosingReasonType == 1 && order.EndDate != null && order.EndDate > order.ClosingDate)
                {
                    //Սխալ փակման պատճառ, լիազորագրի վերջը մեծ է այսօրվանից։
                    result.Add(new ActionError(1074));
                }
            }

            return result;
        }

        internal static List<ActionError> CheckCustomerDebtsAndDahk(ulong customerNumber, Account debitAccount = null)
        {
            List<ActionError> result = new List<ActionError>();
            if (debitAccount == null)
                debitAccount = new Account();

            List<CustomerDebts> debts = CustomerDebtsDB.GetCustomerDebts(customerNumber, debitAccount.AccountNumber).Where(v => v.AlowTransaction != 1).ToList();
            string DebtCustomerNumber = "";
            if (debitAccount.AccountType != 57 && debitAccount.AccountType != 116)
            {
                foreach (var debt in debts)
                {
                    DebtCustomerNumber = debt.ObjectNumber == customerNumber.ToString() ? "" : "(" + debt.ObjectNumber + ")";
                    if (debt.DebtType == DebtTypes.CurrentAccount && debitAccount.AccountType != 115)
                    {
                        //Հաճախորդը ունի պարտավորություն հաշիվների սպասարկման գծով
                        result.Add(new ActionError(744, new string[] { DebtCustomerNumber }));
                    }
                    else if (debt.DebtType == DebtTypes.HomeBanking && debitAccount.AccountType != 115)
                    {
                        //Հաճախորդը ունի պարտավորություն ՀԲ սպասարկման գծով
                        result.Add(new ActionError(745, new string[] { DebtCustomerNumber }));
                    }
                    else if (debitAccount.AccountType != 61 && debt.DebtType == DebtTypes.Dahk)
                    {
                        // Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ
                        result.Add(new ActionError(743, new string[] { DebtCustomerNumber }));

                    }

                }
            }

            return result;
        }

        internal static List<ActionError> CheckCustomerDebts(ulong customerNumber)
        {
            List<ActionError> result = new List<ActionError>();
            List<DebtTypes> debtTypes = CustomerDebtsDB.GetCustomerServiceFeeDebts(customerNumber);
            foreach (var debtType in debtTypes)
            {
                if (debtType == DebtTypes.CurrentAccount)
                {
                    //Հաճախորդը ունի պարտավորություն հաշիվների սպասարկման գծով
                    result.Add(new ActionError(744, new string[] { "" }));
                }
                else if (debtType == DebtTypes.HomeBanking)
                {
                    //Հաճախորդը ունի պարտավորություն ՀԲ սպասարկման գծով
                    result.Add(new ActionError(745, new string[] { "" }));
                }
            }
            return result;
        }

        /// <summary>
        /// Ստացողի հաշվի համար ստուգում է անհրաժեշտ է տվյալ հաշվի համար ստուգել ստորագրության նմուշի առկայություն
        /// </summary>
        /// <param name="typeOfAccountNew"></param>
        /// <param name="checkGroup">checkGroup-ը 1 ի դեպքում ստուգում է ստորագրության նմուշի համար</param>
        /// <returns></returns>
        internal static bool IsRequiredCheckBySintAccNew(string accountNumber, short checkGroup = 1)
        {
            return ValidationDB.IsRequiredCheckBySintAccNew(accountNumber, checkGroup);
        }

        /// <summary>
        /// Ստուգում է գործարքի կատարման հնարավորությունը 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="User"></param>
        /// <returns></returns>
        public static List<ActionError> CheckOperationAvailability(Order order, ACBAServiceReference.User User)
        {
            //!!!!! ԿՈՄԵՆՏ ՉԱՆԵԼ !!!!!!!
            List<ActionError> result = new List<ActionError>();
            if (order.Source == SourceType.Bank)
            {
                if (User.userCustomerNumber == order.CustomerNumber)
                {
                    //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                    result.Add(new ActionError(544));
                }
                else if (!User.isOnlineAcc)
                {

                    if (ValidationDB.CheckOpDayClosingStatus(User.filialCode) && !((order.Type == OrderType.CancelTransaction || order.Type == OrderType.RemoveTransaction) && User.isOnlineAcc) && !CheckFor24_7Mode(order))
                    {
                        // Հնարավոր չէ կատարել գործարք:Գործառնական օրվա կարգավիճակը փակ է:
                        result.Add(new ActionError(766));
                    }
                    else if (ValidationDB.FrontOfficeAllowTransaction(User.filialCode) && !CheckFor24_7Mode(order))
                    {
                        // Հնարավոր չէ կատարել գործարք:Գործառնական օրվա կարգավիճակը փակ է:

                        //Գործառնական օրվա փակման 144 ստուգումը (Առկա են Front Office համակարգով մուտքագրված չկատարված հայտեր) դրված է: 
                        //Գործարք կատարելը հնարավոր չէ։ Անրհաժեշտության դեպքում կարողեք հանել ստուգումը և կատարել գործարք։ 
                        result.Add(new ActionError(1234));
                    }
                }

            }
            else if (order.Source != SourceType.EContract && order.Source != SourceType.MobileBanking)
            {
                Tuple<int, int> tuple = ValidationDB.Check24_7ModeForHB();

                if (tuple.Item1 != 1 && tuple.Item2 == 2)
                {
                    result.Add(new ActionError(766));
                }

            }

            //case 193455
            if (order.Source == SourceType.SSTerminal)
            {
                result.AddRange(Validation.CheckCustomerDebtsAndDahk(order.CustomerNumber, order.DebitAccount));
            }


            return result;
        }

        /// <summary>
        /// Ստուգում է վարկը ենթակա է հեռացման 
        /// </summary>
        /// <param name="appId">Վարկի ունիկալ համար</param>
        /// <returns>Վերադարձնում է error, եթե վարկը ենթակա չէ հեռացման</returns>
        public static List<ActionError> CheckDeleteAvailability(ulong appId)
        {
            List<ActionError> result = new List<ActionError>();
            if (ValidationDB.CheckLoanDelete(appId) == false)
            {
                //Վարկը տրված է, հնարավոր չէ հեռացնել
                result.Add(new ActionError(1964));
            }
            return result;
        }

        /// <summary>
        /// Դրամարկղի մատյանի մուտք/ելք ի ստուգումներ
        /// </summary>
        /// <param name="cashBook"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateCashBook(CashBook cashBook, User user)
        {
            List<ActionError> errors = new List<ActionError>();

            if (cashBook.Type == 0 && (!user.IsActiveUser(cashBook.CorrespondentSetNumber)))
            {
                //Թղթակից ՊԿ-ն ակտիվ չէ
                errors.Add(new ActionError(1902));
            }

            if ((cashBook.Type == 1 || cashBook.Type == 3) && cashBook.Quality != 0)
            {
                if (!user.IsChiefAcc && (user.AdvancedOptions["isEncashmentDepartment"] != "1" && user.AdvancedOptions["canApproveCashBookSurplusDeficit"] != "1"))
                {
                    errors.Add(new ActionError(1080));
                }
            }

            if (cashBook.Type != 1 && cashBook.Type != 3)
            {
                bool isEncashmentDepartmentChief = false;
                string isHeadCashBook = null;
                if (user.AdvancedOptions["canApproveCashBookSurplusDeficit"] == "1" && user.AdvancedOptions["isEncashmentDepartment"] == "1")
                {
                    isEncashmentDepartmentChief = true;
                }

                user.AdvancedOptions.TryGetValue("isHeadCashBook", out isHeadCashBook);

                if (cashBook.LinkedRowID == 0 && cashBook.CorrespondentSetNumber == 0 && isHeadCashBook != "1" && !user.IsChiefAcc && !isEncashmentDepartmentChief)
                {
                    //Թղթակից նշված չէ
                    errors.Add(new ActionError(773));
                }

                if (cashBook.CorrespondentSetNumber == cashBook.RegisteredUserID)
                {
                    //Թղթակիցը չի կարող համընկնել օգտագործողի ՊԿ-ի հետ
                    errors.Add(new ActionError(774));
                }
                int corSetNumFilialCode = 0;
                if (cashBook.LinkedRowID == 0)
                {
                    corSetNumFilialCode = CashBook.GetCorrespondentFilialCode(cashBook.CorrespondentSetNumber);
                }


                if (cashBook.LinkedRowID == 0 && cashBook.FillialCode != corSetNumFilialCode && cashBook.CorrespondentSetNumber != 0 && user.AdvancedOptions["isEncashmentDepartment"] != "1")
                {
                    if (isHeadCashBook != "1" || (cashBook.Type != 2 && cashBook.Type != 4))
                    {
                        //Թղթակից ՊԿ-ը գրանցված է այլ մասնաճյուղում
                        errors.Add(new ActionError(775));
                    }

                }

            }


            if (cashBook.OperationType != 1 && cashBook.OperationType != 2)
            {
                //Մուտք/Ելք գործողության տեսակը նշված չէ
                errors.Add(new ActionError(776));
            }


            if (cashBook.Currency != "AMD" && cashBook.Currency != "EUR" && cashBook.Currency != "USD" && cashBook.Currency != "GBP" && cashBook.Currency != "CHF" && cashBook.Currency != "GEL" && cashBook.Currency != "RUR")
            {
                //Մուտքագրված արժույթը սխալ է
                errors.Add(new ActionError(777));
            }

            if (!Utility.IsCorrectAmount(cashBook.Amount, cashBook.Currency))
            {
                //Գումարը սխալ է մուտքագրած:
                errors.Add(new ActionError(25));
            }


            if (cashBook.ID != 0 && CashBookOrderDB.IsExistUnconfirmedOrder(cashBook.ID))
            {
                //Գույություն ունի դրամարկղի մատյանի չհաստատված հայտ
                errors.Add(new ActionError(1426));
            }

            return errors;
        }

        /// <summary>
        /// Ստանալ գործառնական օրվա կարգավիճակը
        /// </summary>
        /// <param name="filialCode">մասնաճյուղի համար</param>
        /// <returns></returns>
        public static List<ActionError> CheckOpDayClosingStatus(ushort filialCode)
        {
            List<ActionError> result = new List<ActionError>();
            if (ValidationDB.CheckOpDayClosingStatus(filialCode))
            {
                // Հնարավոր չէ կատարել գործողություն:Գործառնական օրվա կարգավիճակը փակ է:
                result.Add(new ActionError(766));
            }
            return result;
        }

        /// <summary>
        /// Ստանալ գործառնական օրվա կարգավիճակը
        /// </summary>
        /// <param name="filialCode">մասնաճյուղի համար</param>
        /// <returns></returns>
        public static OperDayClosingStatus GetOperDayClosingStatus(ushort filialCode)
        {
            OperDayClosingStatus result = new OperDayClosingStatus();
            if (ValidationDB.CheckOpDayClosingStatus(filialCode))
            {
                result = OperDayClosingStatus.OperDayClosed;
            }
            else
            {
                result = OperDayClosingStatus.OperDayOpened;
            }
            return result;
        }

        public static bool CheckReciverBankStatus(int bankCode)
        {
            return Utility.GetBankStatus(bankCode);
        }


        public static List<ActionError> CloseCashbook(CashBook cashBook)
        {
            List<ActionError> result = new List<ActionError>();
            if (cashBook.Type == 0 || (cashBook.Type == 1 && cashBook.Quality == 0) || (cashBook.Type == 3 && cashBook.Quality == 0))
            {
                result.Add(new ActionError(813));
            }
            return result;
        }
        /// <summary>
        /// Սպասարկան վարձի գանձման նշման մուտքագրման և հեռացման ստուգումներ 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateServicePaymentNoteOrder(ServicePaymentNoteOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.Type == OrderType.ServicePaymentNote)
            {
                //Ստուգում է գոյություն ունի սպասարկման վարձի գանձման նշում տվյալ օրվա համար
                if (ServicePaymentNoteDB.IsExistCurrentDayNote(order))
                {
                    result.Add(new ActionError(845));
                    return result;
                }
            }
            else if (order.Type == OrderType.DeleteServicePaymentNote)
            {
                //Ստուգում է ջնջվող սպասարկման վարձի գանձման նշման ամսաթիվը տվյալ օրվա ամսաթիվն է թե ոչ 
                if (!ServicePaymentNoteDB.IsRemovableNote(order))
                {
                    result.Add(new ActionError(846));
                }
            }
            return result;
        }
        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ ստուգումներ
        /// </summary>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateTransferAdditionalData(TransferAdditionalData additionalData, double orderAmount)
        {

            List<ActionError> result = new List<ActionError>();

            if (additionalData.SenderLivingPlace == 0)
            {
                //Գումար ուղարկողի բնակության վայրը ընտրված չէ
                result.Add(new ActionError(829));
            }

            if (additionalData.ReceiverLivingPlace == 0)
            {
                //Գումար ստացողի բնակության վայրը ընտրված չէ
                result.Add(new ActionError(830));
            }

            if (additionalData.TransferAmountType == 0)
            {
                //Փոխանցվող գումարի բնույթը ընտրված չէ
                result.Add(new ActionError(831));
            }
            if (additionalData.TransferAmountPurposes == null)
            {
                //Փոխանցման Գումարի նպատակները լրացված չեն
                result.Add(new ActionError(832));
            }

            double totalAmount = 0;
            foreach (var p in additionalData.TransferAmountPurposes)
            {
                totalAmount += p.Amount;
            }
            if (totalAmount != orderAmount)
            {
                //Գումարը ամբողջությամբ լրացված չէ
                result.Add(new ActionError(833));
            }
            return result;
        }


        internal static List<ActionError> ValidatePensionApplicationOrder(PensionApplicationOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
                return result;
            }

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));


            if (customerType != (short)CustomerTypes.physical)
            {
                //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                result.Add(new ActionError(726));
                return result;
            }


            if (order.Type == OrderType.PensionApplicationOrder || order.Type == OrderType.PensionApplicationActivationOrder || order.Type == OrderType.PensionApplicationOverwriteOrder)
            {
                if (order.PensionApplication.ServiceType == -1)
                {

                    //Ընտրեք ծառայության տեսակը
                    result.Add(new ActionError(823));
                }
                else if (order.PensionApplication.ServiceType == 1)
                {
                    if (order.PensionApplication.Account != null && !string.IsNullOrEmpty(order.PensionApplication.Account.AccountNumber))
                    {
                        Card card = Card.GetCard(order.PensionApplication.Account.ProductNumber, order.CustomerNumber);
                        if (card != null)
                        {
                            if (card.ClosingDate != null)
                            {
                                //Ընտրված է փակված քարտ
                                result.Add(new ActionError(532));
                                return result;
                            }
                            else if (card.Type != 21)
                            {
                                //Քարտի տեսակը պետք է լինի ARCA PENSION
                                result.Add(new ActionError(827));
                            }
                        }
                    }
                }
                else if (order.PensionApplication.ServiceType == 2 && order.Type == 0)
                {
                    //Ընտրեք քարտի տեսակը
                    result.Add(new ActionError(824));
                }
                else if (order.PensionApplication.ServiceType == 3 && (order.PensionApplication.Account == null || string.IsNullOrEmpty(order.PensionApplication.Account.AccountNumber)))
                {
                    //Լրացրեք քարտի համարը
                    result.Add(new ActionError(825));
                }
                else if (order.PensionApplication.ServiceType == 5 && (order.PensionApplication.Account == null || string.IsNullOrEmpty(order.PensionApplication.Account.AccountNumber)))
                {
                    //Լրացրեք հաշվի համարը 
                    result.Add(new ActionError(484));
                }
                else if (order.PensionApplication.ServiceType == 3)
                {
                    Card card = Card.GetCard(order.PensionApplication.Account.ProductNumber, order.CustomerNumber);


                    if (card == null)
                    {
                        //Քարտը գտնված չէ
                        result.Add(new ActionError(534));
                        return result;
                    }
                    else if (card.ClosingDate != null)
                    {
                        //Ընտրված է փակված քարտ
                        result.Add(new ActionError(532));
                        return result;
                    }
                    else
                    {
                        if (card.Type == 21)
                        {
                            //Քարտի տեսակը չի կարող ARCA PENSION
                            result.Add(new ActionError(828));
                        }
                    }
                }
                else if (order.PensionApplication.ServiceType == 5)
                {
                    Account account = Account.GetAccount(order.PensionApplication.Account.AccountNumber, order.CustomerNumber);
                    if (account == null)
                    {
                        //{0} համարի հաշիվը բացակայում է նշած հաճախորդի գործող հաշիվների մեջ
                        result.Add(new ActionError(492, new string[] { order.PensionApplication.Account.AccountNumber }));
                    }
                    else if (account.ClosingDate != null)
                    {
                        //Հաշիվը փակ է  ( h/h  @var1 ):
                        result.Add(new ActionError(140, new string[] { order.PensionApplication.Account.AccountNumber }));
                    }

                }



                if (order.Type != OrderType.PensionApplicationOverwriteOrder && order.Type != OrderType.PensionApplicationActivationOrder && PensionApplication.HasPensionApplication(order.CustomerNumber))
                {
                    //Տվյալ հաճախորդի համար արդեն գոյություն ունի մուտքագրված դիմում
                    result.Add(new ActionError(826));
                }

                if (order.Type == OrderType.PensionApplicationOverwriteOrder)
                {
                    PensionApplication pensionApplication = PensionApplication.GetPensionApplication(order.CustomerNumber, order.PensionApplication.ContractId);
                    if (pensionApplication == null || pensionApplication.Deleted)
                    {
                        //Կենսաթոշակի պայմանագիրը գտված չէ
                        result.Add(new ActionError(834));
                        return result;
                    }

                    bool checkPensionApplication = false;
                    if (pensionApplication.Quality == 0 || pensionApplication.Quality == 41)
                    {
                        checkPensionApplication = true;
                    }
                    if (!checkPensionApplication)
                    {
                        //Հնարավոր չէ վերագրանցել տվյալ դիմում համաձայնագիրը
                        result.Add(new ActionError(843));
                    }


                }


            }
            else if (order.Type == OrderType.PensionApplicationActivationRemovalOrder)
            {
                PensionApplication pensionApplication = PensionApplication.GetPensionApplication(order.CustomerNumber, order.PensionApplication.ContractId);
                if (pensionApplication == null || pensionApplication.Deleted)
                {
                    //Կենսաթոշակի պայմանագիրը գտված չէ
                    result.Add(new ActionError(834));
                    return result;
                }
                if (pensionApplication.FillialCode != order.FilialCode && order.FilialCode != 22000)
                {
                    //Այլ մասնաճյուղի պայմանագիր հեռացնել հնարավոր չէ
                    result.Add(new ActionError(835));
                }

                bool checkPensionApplication = false;
                if (pensionApplication.Quality == 10)
                {
                    checkPensionApplication = true;
                }
                else if (pensionApplication.ContractDate == Utility.GetCurrentOperDay())
                {
                    checkPensionApplication = true;
                }

                if (!checkPensionApplication)
                {
                    //Հնարավոր չէ հեռացնել տվյալ պայմանագիրը:Ստուգեք կարգավիճակը կամ պայմանագրի սկիզբը
                    result.Add(new ActionError(836));
                }
            }


            return result;

        }



        internal static List<ActionError> ValidatePensionApplicationTerminationOrder(PensionApplicationTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
                return result;
            }

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));


            if (customerType != (short)CustomerTypes.physical)
            {
                //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                result.Add(new ActionError(726));
                return result;
            }


            if (order.ClosingType == 0)
            {
                //Լրացրեք դադարեցման հիմքը
                result.Add(new ActionError(837));
            }
            if (order.ClosingDate == null)
            {
                //Լրացրեք դադարեցման ամսաթիվը
                result.Add(new ActionError(838));
            }
            if (order.ClosingType == 2 && order.DeathDate == null)
            {
                //Լրացրեք մահվան ամսաթիվը
                result.Add(new ActionError(839));
            }
            if (order.ClosingType == 2 && string.IsNullOrEmpty(order.DeathCertificateNumber))
            {
                //Լրացրեք մահվան վկայականը
                result.Add(new ActionError(840));
            }

            PensionApplication pensionApplication = PensionApplication.GetPensionApplication(order.CustomerNumber, order.PensionApplication.ContractId);

            if (pensionApplication == null || pensionApplication.Deleted)
            {
                //Կենսաթոշակի պայմանագիրը գտված չէ
                result.Add(new ActionError(834));
                return result;
            }

            if (pensionApplication.Quality != 0 && !order.user.IsChiefAcc)
            {
                //Դադարեցնել կարելի է միայն գործող պայմանագիրը
                result.Add(new ActionError(841));
            }

            return result;

        }

        internal static List<ActionError> ValidateTransferCallContractOrder(TransferCallContractOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.TransferCallContractDetails.ContractNumber == 0)
            {
                result.Add(new ActionError(854));
            }

            if (string.IsNullOrEmpty(order.TransferCallContractDetails.ContractPassword))
            {
                result.Add(new ActionError(855));
            }

            if (order.TransferCallContractDetails.ContractDate == default(DateTime))
            {
                result.Add(new ActionError(856));
            }

            if (order.TransferCallContractDetails.Account == null)
            {
                result.Add(new ActionError(857));
            }
            else if (string.IsNullOrEmpty(order.TransferCallContractDetails.Account.AccountNumber))
            {
                result.Add(new ActionError(857));
            }

            if (order.TransferCallContractDetails.InvolvingSetNumber == 0)
            {
                result.Add(new ActionError(858));
            }

            if (order.IsSameAccountContract())
            {
                result.Add(new ActionError(859));
            }

            if (order.IsSameNumberContract())
            {
                result.Add(new ActionError(860));
            }

            if (order.IsSecondActivation())
            {
                result.Add(new ActionError(863));
            }

            return result;
        }

        internal static List<ActionError> ValidateTransferCallContractTerminationOrder(TransferCallContractTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.TransferCallContractDetails.ClosingDate == null || order.TransferCallContractDetails.ClosingDate == default(DateTime))
            {
                result.Add(new ActionError(861));
            }
            else if (order.TransferCallContractDetails.ClosingDate < order.TransferCallContractDetails.ContractDate)
            {
                result.Add(new ActionError(862));
            }

            if (order.IsSecondTermination())
            {
                result.Add(new ActionError(864));
            }

            return result;
        }


        internal static List<ActionError> ValidateDepositCaseOrder(DepositCaseOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
                return result;
            }

            if (!DepositCaseOrderDB.HasFilialDepositCase(order.FilialCode))
            {
                //Տվյալ մասնաճյուղում պահատուփ նախատեսված չէ:
                result.Add(new ActionError(884));
                return result;
            }

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));


            if (customerType != (short)CustomerTypes.physical)
            {
                //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                result.Add(new ActionError(726));
                return result;
            }

            if (order.DepositCase.JointType == 1 && order.DepositCase.ContractType == 0)
            {
                //Համատեղ պահատուփերի համար պետք է նշել պայմանագիր տեսակը(միաժամանակյա/ոչ միաժամանակյա):
                result.Add(new ActionError(869));
            }
            else if (order.DepositCase.JointType == 1 && (order.DepositCase.JointCustomers == null || order.DepositCase.JointCustomers.Count == 0))
            {
                //Համատեղ պահատուփերի համար պետք է նշել Պահատու 2 հաճախորդի համար:
                result.Add(new ActionError(870));
            }
            else if (order.DepositCase.JointType == 1 && (order.Type != OrderType.DepositCaseDeleteOrder && order.Type != OrderType.DepositCaseTermiationOrder))
            {
                foreach (KeyValuePair<ulong, string> customer in order.DepositCase.JointCustomers)
                {
                    if (customer.Key == order.CustomerNumber)
                    {
                        //Պահատու 1 և Պահատու 2 հաճախորդի համարները համընկնում են:
                        result.Add(new ActionError(871));
                    }

                }
            }
            if (order.DepositCase.ContractNumber == 0)
            {
                //Մուտքագրեք պայմանգրի համարը:
                result.Add(new ActionError(872));
            }
            if (order.DepositCase.StartDate == default(DateTime))
            {
                //Մուտքագրեք պայմանգրի սկիզբը:
                result.Add(new ActionError(873));
            }
            if (order.DepositCase.EndDate == default(DateTime))
            {
                //Մուտքագրեք պայմանգրի վերջը:
                result.Add(new ActionError(874));
            }
            if (order.DepositCase.CaseNumber == "")
            {
                //Պահատուփի համարը նշված չէ:
                result.Add(new ActionError(875));
            }
            else if (order.Type == OrderType.DepositCaseOrder && !DepositCase.CheckDepositCaseNumber(order.DepositCase.CaseNumber, order.FilialCode))
            {

                //{0} համարի պահատուփը զբաղված է կամ գոյություն չունի:
                result.Add(new ActionError(876, new string[] { order.DepositCase.CaseNumber.ToString() }));
            }
            else if (order.Type != OrderType.DepositCaseOrder)
            {
                DepositCase depositCase = new DepositCase();
                depositCase = DepositCase.GetDepositCase((ulong)order.DepositCase.ProductId, order.CustomerNumber);
                if (depositCase == null)
                {
                    //Պահատուփը գտնված չէ:
                    result.Add(new ActionError(879));
                }

                else if (order.Type == OrderType.DepositCaseActivationOrder)
                {
                    if (depositCase.CaseNumber != order.DepositCase.CaseNumber)
                    {
                        if (!DepositCase.CheckDepositCaseNumber(order.DepositCase.CaseNumber, order.FilialCode))
                        {
                            //{0} համարի պահատուփը զբաղված է կամ գոյություն չունի:
                            result.Add(new ActionError(876, new string[] { "'" + order.DepositCase.CaseNumber.ToString() + "'" }));
                        }
                    }
                    double price = DepositCase.GetDepositCasePrice(order.DepositCase.CaseNumber, order.FilialCode, order.DepositCase.ContractDuration);
                    if (price != order.Amount)
                    {
                        //Գործարքի գումարը չի համապասխանում հաշվարկված գումարին:
                        result.Add(new ActionError(880));
                    }

                    Account account = Account.GetAccount(order.DepositCase.ConnectAccount.AccountNumber, depositCase.CustomerNumber);
                    if (account == null)
                    {
                        //Դեբետային հաշիվը ձեզ չի պատկանում
                        result.Add(new ActionError(2));
                        return result;
                    }



                    foreach (OrderFee fee in order.Fees)
                    {
                        if (fee.Type == 24)
                        {

                            Account feeAccount = Account.GetAccount(fee.Account.AccountNumber, depositCase.CustomerNumber);
                            if (feeAccount == null)
                            {
                                //Միջնորդավճարի հաշիվը ձեզ չի պատկանում
                                result.Add(new ActionError(119));
                                return result;
                            }
                        }
                    }



                }

            }

            if ((order.Type != OrderType.DepositCaseDeleteOrder && order.Type != OrderType.DepositCaseTermiationOrder) && !Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened).Exists(m => m.Currency == "AMD"))
            {
                //Պահատուփ մուտքագրելու համար անհրաժեշտ է ունենալ ընթացիկ դրամային հաշիվ
                result.Add(new ActionError(878));
            }



            return result;

        }

        internal static List<ActionError> ValidateDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order, User user)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.Type == OrderType.DepositCasePenaltyMatureOrder)
            {
                result.AddRange(Validation.ValidateDebitAccount(order, order.DebitAccount));
            }
            else
            {
                result.AddRange(Validation.ValidateCashOperationAvailability(order, user));
            }
            result.AddRange(Validation.ValidateOPPerson(order, order.ReceiverAccount, order.DebitAccount.AccountNumber));

            if (order.Amount < 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(order.Amount, order.Currency))
            {
                result.Add(new ActionError(25));
            }

            result.AddRange(order.ValidateOrderDescription());

            return result;
        }



        /// <summary>
        /// Քարտի մուտքագրման հայտի փաստաթղթի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardRegistrationOrderDocument(CardRegistrationOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();


            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (result.Count > 0)
            {
                return result;
            }

            short customerType = ACBAOperationService.GetCustomerType(order.CustomerNumber);
            uint identityId = (uint)ACBAOperationService.GetIdentityId(order.CustomerNumber);
            var verificationData = ACBAOperationService.GetIdentityVerificationData(identityId);
            KeyValue customerFilial = ACBAOperationService.GetCustomerFilial(order.CustomerNumber);
            short customerQuality = ACBAOperationService.GetCustomerQuality(order.CustomerNumber);


            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber) && order.Card.CardType != (uint)PlasticCardType.VISA_DIGITAL)
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            if (customerType == (short)CustomerTypes.physical)
            {
                if (order.Card.CardType != (uint)PlasticCardType.VISA_DIGITAL)
                {
                    List<CustomerDocument> customerDocuments = ACBAOperationService.GetCustomerDocumentList(order.CustomerNumber);
                    //Հաճախորդի ստորագրության նմուշի ստուգում:
                    result.AddRange(ValidateCustomerSignature(order.CustomerNumber));

                    if (!ValidateCustomerPSN(customerDocuments))
                    {
                        //Հաճախորդի փաստաթղթերում <հանրային ծառայությունների համարանիշ> կամ <հանրային ծառայությունների համարանիշից հրաժարման տեղեկանք> տեսակով փաստաթուղթը բացակայում է
                        result.Add(new ActionError(585));
                    }
                }

            }
            else
            {
                List<LinkedCustomer> linkedCustomerList = ACBAOperationService.GetCustomerLinkedPersonsList(order.CustomerNumber, 1);
                if (!linkedCustomerList.Exists(item => item.linkType.key == (short)LinkPersonsTypes.manager))
                {
                    //Բացակայում է հաճախորդի տնօրենի տվյալները
                    result.Add(new ActionError(601, new string[] { order.CustomerNumber.ToString() }));
                }

            }

            if (verificationData == null || verificationData.VerificationResultsList.Count == 0)
            {
                if (order.Card.CardType == (uint)PlasticCardType.VISA_DIGITAL)

                {
                    result.Add(new ActionError(1719)); //Հաշվի բացումն արգելվում է՝ համաձայն Բանկում գործող սահմանափակումների: Virtual card
                    return result;
                }

                //Հաճախորդի համար ահաբեկիչների հետ ստուգումը բացակայում է, անհրաժեշտ է կրկին հաստատել հաճախորդին
                result.Add(new ActionError(581));
            }
            else
            {
                VerificationResult verificationResult = verificationData.VerificationResultsList.Find(m => m.id != 0);
                if (verificationResult.verifyResult.key == 2)
                {
                    if (order.Card.CardType == (uint)PlasticCardType.VISA_DIGITAL)
                    {
                        result.Add(new ActionError(1719)); //Հաշվի բացումն արգելվում է՝ համաձայն Բանկում գործող սահմանափակումների: Virtual card
                        return result;
                    }
                    //Հաճախորդը նշված է որպես կասկածելի
                    result.Add(new ActionError(582));
                }
            }

            if (CardRegistrationOrder.IsSecondRegistration(order.Card.ProductId) == true)
            {
                //Նշված քարտի համար գոյություն ունի մուտքագրման հայտ
                result.Add(new ActionError(885));
            }


            if (customerQuality != 1 && order.CustomerNumber != 100000003724) //մեր բանկի բիզնես քարտեր
            {
                //Հաշիվը կարելի է բացել միայն <Հաճախորդ> կարգավիճակով անձանց համար
                result.Add(new ActionError(887));
            }

            if (customerType != (short)CustomerTypes.physical && customerFilial.key != order.Card.FilialCode + 22000)
            {
                //Ոչ ֆիզիկական անձանց դեպքում հաշիվ հնարավոր է բացել միայն հաճախորդի հիմնական մասնաճյուղում,Մ/Ճ ՝{0}:
                result.Add(new ActionError(957, new string[] { customerFilial.key.ToString() }));
            }


            if (result.Count > 0)
            {
                return result;
            }

            Card existingCard = Card.GetCardMainData(order.Card.ProductId, order.CustomerNumber);
            if (existingCard != null)
            {
                if (existingCard.ClosingDate != null)
                {
                    //Նշված քարտը Չեք կարող մուտքագրել, անհրաժեշտ է այն վերաբացել
                    result.Add(new ActionError(907));
                }
                else
                {
                    //Նշված քարտը գոյություն ունի
                    result.Add(new ActionError(909));
                }
            }



            if (order.Card.SupplementaryType != SupplementaryType.Main)
            {
                if (String.IsNullOrEmpty(order.Card.MainCardNumber))
                {
                    //Հիմնական քարտը գտնված չէ
                    result.Add(new ActionError(899));
                }

                Card mainCard = Card.GetCardMainData(order.Card.MainCardNumber);
                if (mainCard == null)
                {
                    //Հիմնական քարտը գտնված չէ
                    result.Add(new ActionError(899));
                }

                if (mainCard != null && order.Card.Currency != mainCard.Currency)
                {
                    //Քարտի արժույթը չի համապատասխանում հիմնական քարտի արժույթին
                    result.Add(new ActionError(897));
                }

                if (order.Card.CardNumber == order.Card.MainCardNumber)
                {
                    //Քարտը կցվում է նույն քարտին
                    result.Add(new ActionError(895));
                }

            }

            if (order.Card.SupplementaryType == SupplementaryType.Main && order.Card.CardNumber != order.Card.MainCardNumber)
            {
                //Հիմնական քարտի համարը սխալ է 
                result.Add(new ActionError(886));
            }



            if (result.Count > 0)
            {
                return result;
            }

            CardTariffContract cardTariff = new CardTariffContract();
            cardTariff.TariffID = order.Card.RelatedOfficeNumber;
            CardTariffContractDB.GetCardTariffs(cardTariff);
            if (!cardTariff.CardTariffs.Exists(m => m.CardType == order.Card.CardType && m.Currency == order.Card.Currency))
            {
                //Ընտրված աշխատավարձային ծրագրում նախատեսված չէ տվյալ քարտի տիպը
                result.Add(new ActionError(888));
            }


            int accountType = 1; // 1 - Քարտային հաշիվ, 2 - Գերածախսի հաշիվ

            if (order.IsNewAccount == 2)
                result.AddRange(ValidateCardProductAccounts(order.Card, order.CardAccount, accountType, order.CustomerNumber, user));

            accountType = 2;
            if (order.IsNewOverdraftAccount == 2)
                result.AddRange(ValidateCardProductAccounts(order.Card, order.OverdraftAccount, accountType, order.CustomerNumber, user));

            if (order.Card.FilialCode == 0 && user.filialCode == 22059)
            {
                user.filialCode = (ushort)(order.Card.FilialCode + 22000);
            }

            if (order.Source != SourceType.AcbaOnline && order.Source != SourceType.MobileBanking && order.Card.FilialCode != user.filialCode - 22000)
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
            }

            if (order.Card.Currency != "AMD")
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(order.CustomerNumber, ProductQualityFilter.Opened);
                if (!currentAccounts.Exists(m => m.Currency == "AMD" && m.AccountType == 10) && order.CustomerNumber != 100000003724)
                {
                    //Հաճախորդը չունի ընթացիկ AMD հաշիվ
                    result.Add(new ActionError(890));
                }
            }






            return result;
        }



        /// <summary>
        /// Քարտին կցվող հաշիվների ստուգումներ
        /// </summary>
        /// <returns></returns>
        public static List<ActionError> ValidateCardProductAccounts(PlasticCard plasticCard, Account account, int accountType, ulong customerNumber, User user)
        {
            List<ActionError> result = new List<ActionError>();
            if (account == null && accountType == 3)
                return result;

            if (account == null)
            {
                //{0} հաշիվը բացակայում է
                result.Add(new ActionError(891, new string[] { (accountType == 1) ? "Քարտային" : "Գերածախսի" }));
                return result;
            }

            if (account.Currency != plasticCard.Currency)
            {
                //{0} հաշվի արժույթը չի համապատասխանում քարտի արժույթին
                result.Add(new ActionError(889, new string[] { (accountType == 1) ? "Քարտային" : (accountType == 2) ? "Գերածախսի" : "Վարկային" }));
            }

            if (CardRegistrationOrder.IsAccountUseForAnotherCard(account.AccountNumber, accountType, (plasticCard.SupplementaryType != SupplementaryType.Main) ? plasticCard.MainCardNumber : "") == false)
            {
                //{0} հաշիվը կցված է ուրիշ քարտի
                result.Add(new ActionError(893, new string[] { (accountType == 1) ? "Քարտային" : (accountType == 2) ? "Գերածախսի" : "Վարկային" }));
            }


            Account customerAccount = Account.GetAccount(account.AccountNumber, customerNumber);
            if (customerAccount == null)
            {
                //{0} հաշիվը չի պատկանում տվյալ հաճախորդին
                result.Add(new ActionError(901, new string[] { (accountType == 1) ? "Քարտային" : (accountType == 2) ? "Գերածախսի" : "Վարկային" }));

            }


            if (account.FilialCode - 22000 != plasticCard.FilialCode && (account.FilialCode != 22000 || plasticCard.FilialCode != 59))
            {
                //{0} հաշիվը չի պատկանում քարտի մասնաճյուղին
                result.Add(new ActionError(903, new string[] { (accountType == 1) ? "Քարտային" : (accountType == 2) ? "Գերածախսի" : "Վարկային" }));
            }

            return result;
        }


        /// <summary>
        /// Փոխանցում խանութի հաշվին հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateTransferToShopOrder(TransferToShopOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
                return result;
            }

            byte customerType;
            customerType = (byte)ACBAOperationService.GetCustomerType(order.CustomerNumber);

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));


            if (customerType != (short)CustomerTypes.physical)
            {
                //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                result.Add(new ActionError(726));
                return result;
            }


            if (!TransferToShopOrder.CheckTransferToShopPayment(order.ProductId))
            {
                //Տվյալ գումարը արդեն փոխանցված է
                result.Add(new ActionError(919));
            }



            return result;
        }

        internal static List<ActionError> ValidateInsuranceOrder(InsuranceOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը
                result.Add(new ActionError(485));
                return result;
            }

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            result.AddRange(ValidateCashOperationAvailability(order, order.user));

            if (order.Insurance.InsuranceType == 0)
            {
                //{Ապահովագրության տեսակ} դաշտը լրացված չէ:
                result.Add(new ActionError(932, new string[] { "Ապահովագրության տեսակ" }));
            }

            if (order.Insurance.Company == 0)
            {
                //{Ապահովագրական ընկերություն} դաշտը լրացված չէ:
                result.Add(new ActionError(932, new string[] { "Ապահովագրական ընկերություն" }));
            }

            if (order.Insurance.StartDate.Date == order.Insurance.EndDate.Date)
            {
                //Սկիզբը հավասար է վերջին։
                result.Add(new ActionError(733));
            }

            if (order.Insurance.StartDate.Date > order.Insurance.EndDate.Date)
            {
                //Սկիզբը մեծ է վերջից։
                result.Add(new ActionError(728));
            }

            if (order.Insurance.StartDate == default(DateTime))
            {
                //{Պայմանգրի սկիզբ} դաշտը լրացված չէ:
                result.Add(new ActionError(932, new string[] { "Պայմանգրի սկիզբ" }));
            }

            if (order.Insurance.EndDate == default(DateTime))
            {
                //{Պայմանգրի վերջ} դաշտը լրացված չէ:
                result.Add(new ActionError(932, new string[] { "Պայմանգրի վերջ" }));
            }

            if (order.Insurance.InsuranceContractType != 2)
            {

                if (order.Type == OrderType.InsuranceOrder)
                {
                    result.AddRange(ValidateDebitAccount(order, order.DebitAccount));
                }

                if (order.Insurance.StartDate.Date < DateTime.Now.Date)
                {
                    //Սկիզբը այսօրվանից փոքր է։
                    result.Add(new ActionError(734));
                }

                if (order.Insurance.EndDate.Date < DateTime.Now.Date)
                {
                    //Վերջը այսօրվանից փոքր է։
                    result.Add(new ActionError(735));
                }

                result.AddRange(order.ValidateOrderDescription());


                if (order.Currency != "AMD")
                {
                    //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                    result.Add(new ActionError(669));
                }

                if (order.Insurance.CompensationAmount <= 0)
                {
                    //{Ապահովագրական գումար} դաշտը լրացված չէ:
                    result.Add(new ActionError(932, new string[] { "Ապահովագրական գումար" }));
                }

                else if (!Utility.IsCorrectAmount(order.Insurance.CompensationAmount, order.Currency))
                {
                    //Ապահովագրական գումար դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { "Ապահովագրական գումար" }));
                }

                if (order.Amount <= 0)
                {
                    result.Add(new ActionError(22));
                }

                else if (!Utility.IsCorrectAmount(order.Amount, order.Currency))
                {
                    // Գումար դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { "Գումար" }));
                }

                if (order.Insurance.InvolvingSetNumber == 0)
                {
                    //Ներգրավողի ՊԿ-ն բացակայում է:
                    result.Add(new ActionError(858));
                }

                if (Insurance.GetInsuranceCompanySystemAccountNumber(order.Insurance.Company, order.Insurance.InsuranceType) == 0)
                {
                    //Տվյալ ապահովագրական ընկերության համար տվյալ տեսակի ապահովագրություն նախատեսված չէ
                    result.Add(new ActionError(936));
                }




                if ((order.Insurance.InsuranceContractType == 1 && order.Insurance.InsuranceType == 2) || (order.Insurance.InsuranceContractType == 1 && order.Insurance.InsuranceType == 1))
                {
                    bool existsLoan = Loan.CheckLoanExists(order.CustomerNumber, (ulong)order.Insurance.ConectedProductId);
                    short? loanType = null;
                    if (existsLoan)
                    {
                        loanType = Loan.GetLoanType((ulong)order.Insurance.ConectedProductId);
                    }

                    if (!existsLoan)
                    {
                        //Ընտրեք վարկը
                        result.Add(new ActionError(155));
                    }
                    else if (Insurance.GetInsurances(order.CustomerNumber, ProductQualityFilter.Opened).Exists(m => m.InsuranceType == order.Insurance.InsuranceType && m.InsuranceContractType == order.Insurance.InsuranceContractType))
                    {
                        //Հաճախորդը ունի տվյալ տեսակի գործող ապահովագրություն
                        result.Add(new ActionError(949));
                    }
                    else if (loanType != 14 && loanType != 15)
                    {
                        //Ընտրեք վարկը
                        result.Add(new ActionError(155));
                    }
                }
            }

            if ((order.Insurance.InsuranceContractType == 2 && order.Insurance.InsuranceType == 2) || (order.Insurance.InsuranceContractType == 2 && order.Insurance.InsuranceType == 1))
            {
                if (Insurance.GetInsurances(order.CustomerNumber, ProductQualityFilter.Opened).Exists(m => m.InsuranceType == order.Insurance.InsuranceType && m.InsuranceContractType == order.Insurance.InsuranceContractType))
                {
                    //Հաճախորդը ունի տվյալ տեսակի գործող ապահովագրություն
                    result.Add(new ActionError(949));
                }
            }


            return result;
        }

        /// <summary>
        /// Երաշխիքների ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateGuaranteeActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Guarantee product;
            product = Guarantee.GetGuarantee(order.CustomerNumber, order.ProductId);
            if (product.FillialCode != order.user.filialCode)
            {

                //Այլ մասնաճյուղի երաշխիք  հնարավոր չէ ակտիվացնել:
                result.Add(new ActionError(931));
                return result;
            }
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);



            if (provisions.Count == 0)
            {
                //Գրավը բացակայում է
                result.Add(new ActionError(925));
            }

            List<ulong> customernumbers = LoanProductActivationOrder.GetOwnerCustomerNumbers(product.ProductId, "provision_owners");
            if (customernumbers.Count != 0)
            {
                foreach (var customerNumber in customernumbers)
                {
                    result.AddRange(Validation.ValidateCustomerSignature(customerNumber));
                }
            }
            //ԿԳ կողմից ձևակերպվող պայմանագրի <ստուգված> կարգավիճակի ստուգում
            if (!LoanProductActivationOrder.CheckLoanProductActivationStatus(order.ProductId))
            {
                //ԿԳ կողմից ձևակերպվող պայմանագրի  կարգավիճակը ստուգված չէ
                result.Add(new ActionError(926));
            }

            if (product.StartCapital == 0)
            {
                //Պայմանագրի գումարը սխալ է
                result.Add(new ActionError(927));
            }
            if (product.Quality != 10)
            {
                //Կարգավիճակը պայմանագրային չէ
                result.Add(new ActionError(928));
            }
            if (product.StartDate != order.OperationDate)
            {
                //Սկզբի ա/թ հավասար չէ գործառնական օրվա ա/թ -ի հետ
                result.Add(new ActionError(929));
            }
            //if (!order.user.IsChiefAcc && !order.user.IsManager && !Guarantee.CheckFGuaranteeProvisionCurrency(product.ProductId, product.Currency))
            //{
            //    //Դրամական միջոցներ գրավի արժույթը չի համապատասխանում վարկի արժույթին
            //    result.Add(new ActionError(930));
            //}

            if (Guarantee.HasTransportProvison(product.ProductId))
            {
                if (!LoanProductActivationOrder.IsTranpsortExpensePaid(product.ProductId))
                {
                    //Տրանսպորտային միջոցի/գյուղ․տեխնիկայի գծով ծախսը մուտքագրված չէ
                    result.Add(new ActionError(1806));
                }
            }
            return result;
        }

        /// <summary>
        /// Ակրեդիտիվների ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateAccreditiveActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Accreditive product;
            product = Accreditive.GetAccreditive(order.CustomerNumber, order.ProductId);

            if (product.FillialCode != order.user.filialCode)
            {
                //Այլ մասնաճյուղի ակրեդիտիվը  հնարավոր չէ ակտիվացնել:
                result.Add(new ActionError(933));
                return result;
            }
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);



            if (provisions.Count == 0)
            {
                //Գրավը բացակայում է
                result.Add(new ActionError(925));
            }

            List<ulong> customernumbers = LoanProductActivationOrder.GetOwnerCustomerNumbers(product.ProductId, "provision_owners");
            if (customernumbers.Count != 0)
            {
                foreach (var customerNumber in customernumbers)
                {
                    result.AddRange(Validation.ValidateCustomerSignature(customerNumber));
                }
            }
            //ԿԳ կողմից ձևակերպվող պայմանագրի <ստուգված> կարգավիճակի ստուգում
            if (!LoanProductActivationOrder.CheckLoanProductActivationStatus(order.ProductId))
            {
                //ԿԳ կողմից ձևակերպվող պայմանագրի  կարգավիճակը ստուգված չէ
                result.Add(new ActionError(926));
            }

            if (product.StartCapital == 0)
            {
                //Պայմանագրի գումարը սխալ է
                result.Add(new ActionError(927));
            }
            if (product.Quality != 10)
            {
                //Կարգավիճակը պայմանագրային չէ
                result.Add(new ActionError(928));
            }
            if (product.StartDate != order.OperationDate)
            {
                //Սկզբի ա/թ հավասար չէ գործառնական օրվա ա/թ -ի հետ
                result.Add(new ActionError(929));
            }
            //if (!Accreditive.CheckAccreditiveProvisionCurrency(product.ProductId, product.Currency))
            //{
            //    //Դրամական միջոցներ գրավի արժույթը չի համապատասխանում վարկի արժույթին
            //    result.Add(new ActionError(930));
            //}

            if (Accreditive.HasTransportProvison(product.ProductId))
            {
                if (!LoanProductActivationOrder.IsTranpsortExpensePaid(product.ProductId))
                {
                    //Տրանսպորտային միջոցի/գյուղ․տեխնիկայի գծով ծախսը մուտքագրված չէ
                    result.Add(new ActionError(931));
                }
            }
            return result;
        }
        /// <summary>
        /// Ֆակտորինգի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateFactoringActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Factoring product;
            product = Factoring.GetFactoring(order.CustomerNumber, order.ProductId);

            if (product.FillialCode != order.user.filialCode)
            {
                //Այլ մասնաճյուղի ֆակտորինգ  հնարավոր չէ ակտիվացնել:
                result.Add(new ActionError(934));
                return result;
            }

            if (product.Quality != 10)
            {
                //Կարգավիճակը պայմանագրային չէ
                result.Add(new ActionError(928));
            }

            if (product.StartDate != order.OperationDate)
            {
                //Սկզբի ա/թ հավասար չէ գործառնական օրվա ա/թ -ի հետ
                result.Add(new ActionError(929));
            }

            //if (!Factoring.CheckFactoringProvisionCurrency(product.ProductId))
            //{
            //    //Դրամական միջոցներ գրավի արժույթը չի համապատասխանում վարկի արժույթին
            //    result.Add(new ActionError(930));
            //}

            if (!Factoring.FactoringValidation1(product.ProductId))
            {
                //Ռեգրեսային տեսակի ֆակտորինգի համար պարտադիր է հաճախորդի կողմից երաշխավորության առկայությունը
                result.Add(new ActionError(935));
            }
            return result;
        }

        /// <summary>
        /// Քարտի տվյալների փոփոխման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardDataChangeOrder(CardDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.FieldType == 0)
            {
                //Փոփոխվող տվյալը ընտրված չէ
                result.Add(new ActionError(950));
                return result;
            }
            Card card = Card.GetCardMainData((ulong)order.ProductAppId, order.CustomerNumber);
            if (card == null)
            {
                //Քարտը գտնված չէ:
                result.Add(new ActionError(672));
                return result;
            }
            if (order.FieldType == 20 || order.FieldType == 21)
            {
                string canChangeCardSMSTariff = "";
                order.user.AdvancedOptions.TryGetValue("canChangeCardSMSTariff", out canChangeCardSMSTariff);
                if (canChangeCardSMSTariff != "1")
                {
                    // Գործողությունը հասանելի չէ:
                    result.Add(new ActionError(639));
                    return result;
                }

            }

            else if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }

            else if (order.user.filialCode != card.FilialCode)
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
                return result;
            }





            //if (order.FieldType == 1 || order.FieldType == 7)
            //{
            //    CardServiceFee cardServiceFee = Card.GetCardServiceFee(card.CardNumber);
            //    if (cardServiceFee.Debt + Convert.ToDouble(order.FieldValue) > cardServiceFee.ServiceFeeTotal)
            //    {
            //        //Պարտքի և վճարված սպասարկման վարձի գումարը գերազանցում է տարեկան սպասարկման վարձին:
            //        result.Add(new ActionError(951));
            //    }
            //}

            bool check = CardDataChangeOrder.CheckFieldTypeIsRequaried(order.FieldType);

            if (check && string.IsNullOrEmpty(order.DocumentNumber))
            {
                //Լրացրեք փոփոխության հիմքը:
                result.Add(new ActionError(952));
            }
            else if (!string.IsNullOrEmpty(order.DocumentNumber) && order.DocumentNumber.Length > 100)
            {
                //Փոփոխության հիմքը պետք է լինի առավելագույնը 100 նիշ:
                result.Add(new ActionError(954));
            }
            else if (check && order.DocumentDate == null && order.FieldType != 19)
            {
                //Լրացրեք փաստաթղթի ամսաթիվը
                result.Add(new ActionError(953));
            }



            if (order.ValueType == AdditionalValueType.Date)
            {
                try
                {
                    Convert.ToDateTime(order.FieldValue);
                }
                catch
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }
            }
            if (order.ValueType == AdditionalValueType.Percent)
            {
                try
                {
                    Convert.ToDouble(order.FieldValue);
                }
                catch
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }

                if (Convert.ToDouble(order.FieldValue) * 100 > 100)
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }
            }
            if (order.ValueType == AdditionalValueType.Double)
            {

                try
                {
                    Convert.ToDouble(order.FieldValue);
                }
                catch
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }
                if (!Utility.IsCorrectAmount(Convert.ToDouble(order.FieldValue), "AMD"))
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }
            }
            if (order.ValueType == AdditionalValueType.Int)
            {
                try
                {
                    Convert.ToInt32(order.FieldValue);
                }
                catch
                {
                    //{0} դաշտը սխալ է լրացված:
                    result.Add(new ActionError(937, new string[] { Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType) }));
                    return result;
                }
            }

            if (order.FieldType == 19)
            {
                CardTariffContract cardTariff = new CardTariffContract();
                cardTariff.TariffID = Convert.ToInt64(order.FieldValue);
                CardTariffContractDB.GetCardTariffs(cardTariff);
                if (!cardTariff.CardTariffs.Exists(m => m.CardType == card.Type && m.Currency == card.Currency))
                {
                    //Ընտրված աշխատավարձային ծրագրում նախատեսված չէ տվյալ քարտի տիպը
                    result.Add(new ActionError(888));
                }
            }




            return result;
        }

        /// <summary>
        /// Քարտի տվյալների փոփոխման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardServiceFeeGrafikDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            Card card = Card.GetCard((ulong)order.ProductAppId, order.CustomerNumber);
            if (card == null || card.ClosingDate != null)
            {
                //Քարտը գտնված չէ:
                result.Add(new ActionError(672));
                return result;
            }

            if (order.user.filialCode != card.FilialCode)
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
                return result;
            }

            return result;
        }

        ///// <summary>
        ///// Քարտի վերաթողարկման/փոխարինման հայտի փաստաթղթի ստուգումներ
        ///// </summary>
        ///// <param name="order"></param>
        ///// <param name="user"></param>
        ///// <returns></returns>
        //internal static List<ActionError> ValidateCardReNewRePlaceOrderDocument(CardReNewRePlace order, ACBAServiceReference.User user)
        //{
        //    List<ActionError> result = new List<ActionError>();
        //    if (!order.CheckReNewRePlaceCardProductId())
        //    {
        //        //Քարտը գտնված չի VisaCardApplication-ում
        //        result.Add(new ActionError(995));
        //        return result;
        //    }

        //    if (order.IsSecondReNewRePlace())
        //    {
        //        //Նշված քարտի համար գոյություն ունի վերաթսղարկման/փոխարինման հայտ
        //        result.Add(new ActionError(1001, new string[] { (order.Type == OrderType.CardReNewOrder) ? "վերաթողարկման" : "փոխարինման" }));
        //    }



        //    CardTariffContract cardTariff = new CardTariffContract();
        //    cardTariff.TariffID = order.Card.RelatedOfficeNumber;
        //    CardTariffContractDB.GetCardTariffContract(cardTariff);
        //    if (cardTariff.Quality == 0 || cardTariff.Quality == 2)
        //    {
        //        //Քարտի աշխատավարձային ծրագիրը դադարեցված է կամ սառեցված է:
        //        result.Add(new ActionError(996));
        //        return result;
        //    }
        //    CardTariffContractDB.GetCardTariffs(cardTariff);
        //    if (!cardTariff.CardTariffs.Exists(m => m.CardType == order.Card.CardType && m.Currency == order.Card.Currency))
        //    {
        //        //Ընտրված աշխատավարձային ծրագրում նախատեսված չէ տվյալ քարտի տիպը
        //        result.Add(new ActionError(888));
        //    }
        //    else
        //    {
        //        CardTariff tariff = cardTariff.CardTariffs.Find(m => m.CardType == order.Card.CardType && m.Currency == order.Card.Currency);
        //        if (tariff.Quality == 0)
        //        {
        //            //Այս տեսակի և արժույթի քարտի կարգավիճակը դադարեցված է:
        //            result.Add(new ActionError(1007));
        //            return result;
        //        }
        //    }
        //    ulong oldProductId = order.GetReNewRePlaceCardoldProductId();
        //    if (oldProductId == 0)
        //    {
        //        //Քարտը գտնված չի VisaCardApplication-ում
        //        result.Add(new ActionError(995));
        //        return result;
        //    }

        //    Card oldCard = Card.GetCard(oldProductId, order.CustomerNumber);
        //    if (oldCard == null)
        //    {
        //        //Քարտը գտնված չի VisaCardApplication-ում
        //        result.Add(new ActionError(995));
        //        return result;
        //    }

        //    if (oldCard.ClosingDate != null && order.Type == OrderType.CardReNewOrder && (order.Card.CardType == 23 || order.Card.CardType == 40))
        //    {
        //        //Փակված Amex Blue քարտը հնարավոր չէ վերաթողարկել։
        //        result.Add(new ActionError(1390));
        //    }

        //    List<Card> attachedCards = Card.GetAttachedCards((ulong)oldCard.ProductId, order.CustomerNumber);
        //    if (attachedCards.Count > 0 && (order.Type == OrderType.CardReNewOrder && (oldCard.Type == 23 || oldCard.Type == 20 || (oldCard.Type == 21 && oldCard.CardNumber.Substring(0, 8) == "90513410"))))

        //    {
        //        //Հնարավոր չէ իրականացնել գործողությունը։ Անհրաժեշտ է փակել կից քարտերը։
        //        result.Add(new ActionError(1273));
        //    }

        //    //if ((oldCard.Type == 23 || oldCard.Type == 40))
        //    //{
        //    //    if (CardReNewRePlace.IsAmexWithChangedDate(oldCard.CardNumber))
        //    //    {
        //    //        //AMEX BLUE քարտը, որի ժամկետը փոխված է,{0} ենթակա չէ
        //    //        result.Add(new ActionError(1003, new string[] { (order.Type == OrderType.CardReNewOrder) ? "վերաթողարկման" : "փոխարինման" }));
        //    //    }
        //    //}
        //    if (order.Card.SupplementaryType == SupplementaryType.Main && oldCard.Type != 23 && oldCard.Type != 40)
        //    {
        //        if (oldCard.CreditLine != null)
        //        {
        //            if (oldCard.ClosingDate == null)
        //            {
        //                double accountRest = oldCard.CreditLine.CurrentCapital;
        //                List<LoanProductProlongation> list = LoanProduct.GetLoanProductProlongations((ulong)oldCard.CreditLine.ProductId);
        //                LoanProductProlongation loanProlong = null;
        //                if (list != null)
        //                {
        //                    loanProlong = list.Find(m => m.ActivationDate == null);
        //                }

        //                if (order.Type == OrderType.CardReNewOrder && accountRest != 0 && loanProlong == null)
        //                {
        //                    //Քարտի վարկային հաշիվը մնացորդ ունի
        //                    result.Add(new ActionError(1004));
        //                }
        //                else if (loanProlong != null && loanProlong.ConfirmationDate == null)
        //                {
        //                    //Քարտի վարկային գծի երկարաձգման դիմումը հաստատված չէ
        //                    result.Add(new ActionError(1005));
        //                }
        //            }
        //        }
        //        if (oldCard.Overdraft != null && order.Type == OrderType.CardReNewOrder)
        //        {
        //            double accountRest = oldCard.Overdraft.CurrentCapital;
        //            if (accountRest != 0)
        //                //Քարտի օվերդրաֆտի հաշիվը մնացորդ ունի
        //                result.Add(new ActionError(1006));
        //        }

        //    }

        //    if (oldCard.ClosingDate != null)
        //    {
        //        if (order.Type == OrderType.CardReNewOrder)
        //        {


        //            DateTime openDate = CardReNewOrder.LastClosedCard(oldCard.CardNumber);
        //            if (openDate != oldCard.OpenDate)
        //            {
        //                //Ընտրեք տվյալ համարն ունեցող ամենավերջինը փակված քարտը:
        //                result.Add(new ActionError(999));
        //                return result;
        //            }
        //            Card card = Card.GetCardMainData(oldCard.CardNumber);
        //            if (card != null)
        //            {
        //                //Տվյալ համարով գոյություն ունի գործող քարտ:
        //                result.Add(new ActionError(1000));
        //                return result;
        //            }

        //            if (order.Card.SupplementaryType != SupplementaryType.Main)
        //            {
        //                Card mainCard = Card.GetCardMainData(order.Card.MainCardNumber);
        //                if (mainCard == null)
        //                {
        //                    //Հիմնական քարտը գտնված չէ
        //                    result.Add(new ActionError(899));
        //                }
        //            }


        //            int accountType = 1; // 1 - Քարտային հաշիվ, 2 - Գերածախսի հաշիվ

        //            if (order.IsNewAccount == 2)
        //                result.AddRange(ValidateCardProductAccounts(order.Card, order.CardAccount, accountType,
        //                    order.CustomerNumber, user));

        //            accountType = 2;
        //            if (order.IsNewOverdraftAccount == 2)
        //                result.AddRange(ValidateCardProductAccounts(order.Card, order.OverdraftAccount, accountType,
        //                    order.CustomerNumber, user));

        //            accountType = 3;
        //            if (oldCard.CreditLine != null)
        //                result.AddRange(ValidateCardProductAccounts(order.Card, oldCard.CreditLine.LoanAccount,
        //                    accountType, order.CustomerNumber, user));
        //        }
        //    }
        //    return result;
        //}

        internal static List<ActionError> ValidateCardStatusChangeOrder(CardStatusChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            Card card = Card.GetCardMainData((ulong)order.ProductId, order.CustomerNumber);

            string canChangeCardStatus = null;
            order.user.AdvancedOptions.TryGetValue("canChangeCardStatus", out canChangeCardStatus);

            if (canChangeCardStatus != "1" && order.CardStatus.Status == 3)
            {
                //Քարտի կարգավիճակի փոփոխությունը հասանելի չէ
                result.Add(new ActionError(1386));
                return result;
            }

            if (card == null || card.ClosingDate != null)
            {
                //Քարտը գտնված չէ:
                result.Add(new ActionError(672));
                return result;
            }

            string canChangeCardStatusOtherBranches = null;
            order.user.AdvancedOptions.TryGetValue("canChangeCardStatusOtherBranches", out canChangeCardStatusOtherBranches);

            if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                card.FilialCode = order.user.filialCode;
            }


            if (order.user.filialCode != card.FilialCode && canChangeCardStatusOtherBranches != "1")
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
                return result;
            }

            if (order.CardStatus.Status == 2 && string.IsNullOrEmpty(order.CardStatus.Reason))
            {
                //Լրացրեք փաստաթղթերի թերի համարվելու պատճառը
                result.Add(new ActionError(1034));
            }

            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            CardTariffContract cardTariffContract = new CardTariffContract() { TariffID = card.RelatedOfficeNumber };
            cardTariffContract = CardTariffContractDB.GetCardTariffContract(cardTariffContract);

            long[] officces = { 2644, 2645, 2646, 2647 };

            if (cardTariffContract.Reason == 1 || cardTariffContract.Reason == 2 ||
                (cardTariffContract.Reason == 4 && officces.Contains(cardTariffContract.TariffID)))
            {
                ACBAServiceReference.Customer customer = Customer.GetCustomer(order.CustomerNumber);
                if (customer.customerType.key == (short)CustomerTypes.physical)
                {
                    PhysicalCustomer physicalCustomer = (PhysicalCustomer)customer;
                    Person person = physicalCustomer.person;

                    if (person.SocialGroupsList.Count == 0)
                    {
                        //«Հաճախորդի անձնական տվյալներում Դասակարգիչներ բաժնում պետք է նշված լինի «Աշխատող» տարբերակը
                        result.Add(new ActionError(1840));
                    }
                    else if (!person.SocialGroupsList.Exists(m => m.key == 3))
                    {
                        //«Հաճախորդի անձնական տվյալներում Դասակարգիչներ բաժնում պետք է նշված լինի «Աշխատող» տարբերակը
                        result.Add(new ActionError(1840));
                    }
                    else if (person.SocialGroupsList.Exists(m => m.key == 3))
                    {
                        //Employment emp = person.employmentList.FindAll(m => m.EmploymentType.key == 1).Max(m => m.id);

                        Employment employe = (from empl in person.employmentList
                                              where empl.EmploymentType.key == 1
                                              orderby empl.id descending
                                              select empl).FirstOrDefault();

                        if (employe == null)
                        {
                            employe = (from empl in person.employmentList
                                       where empl.EmploymentType.key == 2
                                       orderby empl.id descending
                                       select empl).FirstOrDefault();
                        }

                        if (employe != null)
                        {
                            if (employe.OrganisationCustomerNumber == 0)
                            {
                                //«Աշխատավայրեր» բաժնում, «Հաճախորդի համար» դաշտը լրացված չէ»
                                result.Add(new ActionError(1841));
                            }
                        }

                    }
                }
            }

            if (CheckCustomerPhoneNumber(order.CustomerNumber))
            {
                //Հաճախորդի համար չկա մուտքագրված հեռախոսահամար:
                result.Add(new ActionError(1904));
            }

            if (order.CustomerNumber != 100000003724 && !ACBAOperationService.HasLegalCommunication(order.CustomerNumber))
            {
                //«SAP CRM» ծրագրում անհրաժեշտ է ընտրել Օրենսդրական ծանուցում(ներ)ի ստացման եղանակ(ներ)ը:
                result.Add(new ActionError(2004));
            }

            return result;
        }

        internal static List<ActionError> ValidateTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();

            if (account == null || account.TransitAccount == null || string.IsNullOrEmpty(account.TransitAccount.AccountNumber))
            {
                //Հաշիվը ընտրված չէ:
                result.Add(new ActionError(55));
                return result;
            }
            if (account.IsCustomerTransitAccount && account.CustomerNumber == 0)
            {
                //Լրացրեք հաճախորդի համարը :
                result.Add(new ActionError(485));
                return result;
            }

            Account transitAccount = Account.GetAccount(account.TransitAccount.AccountNumber);

            if (transitAccount == null)
            {
                //{0} հաշիվը բացակայում է
                result.Add(new ActionError(891, new string[] { account.TransitAccount.AccountNumber }));
            }
            else if (transitAccount.ClosingDate != null)
            {
                //{0} Հաշիվը արդեն փակված է:
                result.Add(new ActionError(506, new string[] { account.TransitAccount.AccountNumber }));
            }

            if (account.ForAllBranches != true && (account.FilialCode == 0 || account.FilialCode.ToString().Length != 5))
            {
                //Մասնաճյուղը ընտրված չէ
                result.Add(new ActionError(1050));
            }
            else
            {
                if (account.ForAllBranches != true && !CheckReciverBankStatus(account.FilialCode))
                {
                    //Ստացողի բանկը փակ է:
                    result.Add(new ActionError(1395, new string[] { account.FilialCode.ToString() }));
                }

            }

            if (result.Count == 0 && !account.IsCustomerTransitAccount)
            {
                TransitAccountForDebitTransactions trAccount = TransitAccountForDebitTransactions.GetTransitAccountsForDebitTransaction(account.TransitAccount.AccountNumber, account.FilialCode);
                if (trAccount != null && !trAccount.ForAllBranches)
                {
                    //Տվյալ հաշվեհամարը նշված մասնաճյուղի համար արդեն մուտքագրված է
                    result.Add(new ActionError(1051));
                }

            }
            if (account.IsCustomerTransitAccount)
            {

                if (!user.IsChiefAcc && user.AdvancedOptions["isOnlineAcc"] != "1" && user.AdvancedOptions["canAddCustomerTransitAccount"] != "1")
                {
                    // Գործողությունը հասանելի չէ:
                    result.Add(new ActionError(639));
                }
                List<Account> currentAccounts = Account.GetCurrentAccounts(account.CustomerNumber, ProductQualityFilter.Opened);
                if (currentAccounts.Exists(m => m.AccountNumber == account.TransitAccount.AccountNumber))
                {
                    //Տվյալ հաշվեհամարը արդեն առկա է հաճախորդի ընթացիկ հաշիվներում։
                    result.Add(new ActionError(1496));
                }

                if (account.TransitAccount.GetAccountCustomerNumber() != account.CustomerNumber)
                {
                    //Տվյալ հաշիվը չի պատկանում տվյալ հաճախորդին։
                    result.Add(new ActionError(1497));
                }

            }


            return result;
        }

        /// <summary>
        /// Ստուգում է արտարժույթային գործարքների ժամանակ գումարի ճիշտ լինելը
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        internal static bool IsCurrencyAmountCorrect(double amount, string currency)
        {
            bool isCorrectAmount = true;
            double currencyMinAmount = Utility.GetCurrencyMinCashAmount(currency);
            if (amount % currencyMinAmount != 0)
            {
                isCorrectAmount = false;
            }
            return isCorrectAmount;

        }

        public static List<ActionError> ValidatePaidGuaranteeActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);

            PaidGuarantee product;
            product = PaidGuarantee.GetPaidGuarantee(order.CustomerNumber, order.ProductId);

            result.AddRange(CheckCustomerDebts(order.CustomerNumber));

            if (product.RequestStatus != 2)
            {
                //Վճարված երաշխիքի կարգավիճակը հաստատված չէ
                result.Add(new ActionError(1066));
            }

            if (product.StartDate != Utility.GetCurrentOperDay())
            {
                //Ստուգեք վարկի տրման օրը
                result.Add(new ActionError(678));
            }

            //TO DO: 
            if (product.FillialCode != order.FilialCode && product.LoanType != 38)
            {
                ////Վարկը կարելի է ձևակերպել միայն @var1 մասնաճյուղից
                result.Add(new ActionError(679, new string[] { product.FillialCode.ToString() }));
            }

            if (product.InterestRate > LoanProduct.GetPenaltyRateForDate(DateTime.Now.Date) * 2)
            {
                //Տոկոսադրույքը բարձր է քան կրկնակի ԿԲ վերաֆինանսավորման տոկոսադրույքը
                result.Add(new ActionError(684));
            }

            result.AddRange(ValidateCustomerSignature(order.OPPerson.CustomerNumber));

            if (Account.GetAcccountAvailableBalance(product.ConnectAccount.AccountNumber) < 0)
            {
                //Հաշվի մնացորդը փոքր է 0-ից
                result.Add(new ActionError(680));
            }

            List<LoanRepaymentGrafik> grafik = product.GetLoanGrafik();

            if (grafik == null)
            {
                //Մարման գրաֆիկը բացակայում է'
                result.Add(new ActionError(681));
            }
            else
            {
                if (grafik.Count == 0)
                {
                    //Մարման գրաֆիկը բացակայում է'
                    result.Add(new ActionError(681));
                }
                if (grafik.Last().RepaymentDate != product.EndDate)
                {
                    //Վարկի մարման ա/թ-ը չի համապատասխանում գրաֆիկի վերջին ա/թ
                    result.Add(new ActionError(682));
                }
            }

            if (order.IsSecondActivation())
            {
                result.Add(new ActionError(687));
            }

            if (ACBAOperationService.HasCustomerArrest(order.CustomerNumber))
            {
                result.Add(new ActionError(688));
            }

            double kurs = Utility.GetCBKursForDate((DateTime)order.OperationDate, product.Currency);
            if (!LoanProductActivationOrder.CheckLoanDocumentAttachment(product.LoanType, product.ProductId, (int)order.Source, product.StartCapital * kurs, order.CustomerNumber))
            {
                result.Add(new ActionError(750));
            }


            if (provisions.Exists(m => m.Currency != product.Currency && m.Type == 13) && product.LoanType != 29)
            {
                result.Add(new ActionError(756));
            }

            foreach (var provision in provisions)
            {
                if (provision.OutBalance.Substring(0, 4) == "8121" || provision.OutBalance.Substring(0, 4) == "8122")
                {
                    if (product.Currency != provision.Currency)
                    {
                        result.Add(new ActionError(757));
                    }

                    if (provision.Amount * kurs > 35000000)
                    {
                        result.Add(new ActionError(758));
                    }
                }
            }

            if (!order.user.IsChiefAcc)
            {
                result.AddRange(LoanProductActivationOrder.LoanActivationValidation(product, order));
            }
            if (product.DayOfRateCalculation != null && product.DayOfRateCalculation != order.OperationDate)
            {
                result.Add(new ActionError(761));
            }

            if (order.FeeAmount > 0 && order.FeeAccount == null)
            {
                result.Add(new ActionError(90));
            }


            return result;


        }

        public static List<ActionError> ValidatePaidFactoringActivationOrderDocument(LoanProductActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            List<Provision> provisions = Provision.GetProductProvisions(order.ProductId, order.CustomerNumber);

            PaidFactoring product;
            product = PaidFactoring.GetPaidFactoring(order.CustomerNumber, order.ProductId);

            result.AddRange(CheckCustomerDebts(order.CustomerNumber));


            if (product.StartDate != Utility.GetCurrentOperDay())
            {
                //Ստուգեք վարկի տրման օրը
                result.Add(new ActionError(678));
            }

            //TO DO: 
            if (product.FillialCode != order.FilialCode && product.LoanType != 38)
            {
                ////Վարկը կարելի է ձևակերպել միայն @var1 մասնաճյուղից
                result.Add(new ActionError(679, new string[] { product.FillialCode.ToString() }));
            }

            if (product.InterestRate > LoanProduct.GetPenaltyRateForDate(DateTime.Now.Date) * 2)
            {
                //Տոկոսադրույքը բարձր է քան կրկնակի ԿԲ վերաֆինանսավորման տոկոսադրույքը
                result.Add(new ActionError(684));
            }


            result.AddRange(ValidateCustomerSignature(order.OPPerson.CustomerNumber));

            if (Account.GetAcccountAvailableBalance(product.ConnectAccount.AccountNumber) < 0)
            {
                //Հաշվի մնացորդը փոքր է 0-ից
                result.Add(new ActionError(680));
            }

            List<LoanRepaymentGrafik> grafik = product.GetLoanGrafik();

            if (grafik == null)
            {
                //Մարման գրաֆիկը բացակայում է'
                result.Add(new ActionError(681));
            }
            else
            {
                if (grafik.Count == 0)
                {
                    //Մարման գրաֆիկը բացակայում է'
                    result.Add(new ActionError(681));
                }
                if (grafik.Last().RepaymentDate != product.EndDate)
                {
                    //Վարկի մարման ա/թ-ը չի համապատասխանում գրաֆիկի վերջին ա/թ
                    result.Add(new ActionError(682));
                }
            }

            if (order.IsSecondActivation())
            {
                result.Add(new ActionError(687));
            }

            if (ACBAOperationService.HasCustomerArrest(order.CustomerNumber))
            {
                result.Add(new ActionError(688));
            }

            double kurs = Utility.GetCBKursForDate((DateTime)order.OperationDate, product.Currency);
            if (!LoanProductActivationOrder.CheckLoanDocumentAttachment(product.LoanType, product.ProductId, (int)order.Source, product.StartCapital * kurs, order.CustomerNumber))
            {
                result.Add(new ActionError(750));
            }


            if (provisions.Exists(m => m.Currency != product.Currency && m.Type == 13) && product.LoanType != 29)
            {
                result.Add(new ActionError(756));
            }

            foreach (var provision in provisions)
            {
                if (provision.OutBalance.Substring(0, 4) == "8121" || provision.OutBalance.Substring(0, 4) == "8122")
                {
                    //if (product.Currency != provision.Currency)
                    //{
                    //    result.Add(new ActionError(757));
                    //}

                    if (!order.user.IsChiefAcc && provision.Amount * kurs > 35000000)
                    {
                        result.Add(new ActionError(758));
                    }
                }
            }

            if (!order.user.IsChiefAcc)
            {
                result.AddRange(LoanProductActivationOrder.LoanActivationValidation(product, order));
            }
            if (product.DayOfRateCalculation != null && product.DayOfRateCalculation != order.OperationDate)
            {
                result.Add(new ActionError(761));
            }

            if (order.FactoringCustomerAccount == 0 || order.FeeAccount.AccountNumber == null)
            {
                //Նշված գործողության մեջ առկա է սխալ։
                result.Add(new ActionError(1416));
            }

            return result;


        }

        public static List<ActionError> ValidateFactoringTerminationOrder(FactoringTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            //if (!FactoringTerminationOrder.IsPaidFactoring(order.ProductId))
            //{
            //    //Գոյություն ունի վճարված ֆակտորինգ
            //    result.Add(new ActionError(1076));
            //}

            Factoring factoring = Factoring.GetFactoring(order.CustomerNumber, order.ProductId);
            if (factoring.StartDate == order.RegistrationDate)
            {
                result.Add(new ActionError(1077));
            }
            if (factoring.FillialCode != order.FilialCode)
            {
                result.Add(new ActionError(1079, new string[] { factoring.FillialCode.ToString() }));
            }

            return result;
        }

        internal static ActionResult ValidateCustomerCommunalCard(CustomerCommunalCard communalCard)
        {
            ActionResult result = new ActionResult();

            if (communalCard.CheckCustomerCommunalCard())
            {
                //Տվյալ կոմունալը արդեն ներառված է կոմունալի քարտում
                result.Errors.Add(new ActionError(1067));
            }


            return result;
        }

        /// <summary>
        /// Երաշխիքի դադարեցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateGuaranteeTerminationOrder(LoanProductTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Guarantee guarantee = Guarantee.GetGuarantee(order.CustomerNumber, order.ProductId);

            if (guarantee == null)
            {
                //Երաշխիքը գտնված չէ
                result.Add(new ActionError(1082));
            }

            if (LoanProductTerminationOrder.IsSecondTermination(order))
            {
                //Առկա է դադարեցման չկատարված հայտ
                result.Add(new ActionError(1081));
            }

            return result;
        }

        /// <summary>
        /// Ակրեդիտիվի դադարեցմնա հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateAccreditiveTerminationOrder(LoanProductTerminationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Accreditive accreditive = Accreditive.GetAccreditive(order.CustomerNumber, order.ProductId);

            if (accreditive == null)
            {
                //Ակրեդիտիվը գտնված չէ:
                result.Add(new ActionError(1152));
            }

            if (LoanProductTerminationOrder.IsSecondTermination(order))
            {
                //Առկա է դադարեցման չկատարված հայտ
                result.Add(new ActionError(1081));
            }

            return result;
        }

        public static List<ActionError> ValidateDepositDataChangeOrder(DepositDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Deposit deposit = Deposit.GetDeposit((ulong)order.Deposit.ProductId, order.CustomerNumber);
            if (deposit == null || deposit.ClosingDate != null)
            {
                //Գործող ավանդը գտնված չէ:
                result.Add(new ActionError(1087));
                return result;
            }

            if (order.FieldType == 0)
            {
                //Փոփոխման տեսակը ընտրված չէ:
                result.Add(new ActionError(1088));
            }

            if (order.FieldType == 1 && string.IsNullOrEmpty(order.FieldValue))
            {
                //Մայր գումարի վճարման հաշիվը ընտրված չէ:
                result.Add(new ActionError(219));
                return result;
            }
            else if (order.FieldType == 2 && string.IsNullOrEmpty(order.FieldValue))
            {
                //Տոկոսագումարի վճարման հաշիվը ընտրված չէ:
                result.Add(new ActionError(1090));
                return result;
            }

            if (order.FieldType == 1)
            {
                Account account = Account.GetAccount(order.FieldValue, order.CustomerNumber);
                if (account == null)
                {
                    //Մայր գումարի վճարման հաշիվը ընտրված չէ:
                    result.Add(new ActionError(228));
                }

            }
            else if (order.FieldType == 2)
            {
                Account account = Account.GetAccount(order.FieldValue, order.CustomerNumber);
                if (account == null)
                {
                    //Տոկոսագումարի հաշիվը գտնված չէ:
                    result.Add(new ActionError(230));
                }

            }
            else if (order.FieldType == 3 || order.FieldType == 4)
            {

                if (order.FilialCode != deposit.FilailCode)
                {
                    //Ավանդը պատկանում է այլ մասնաճյուղի։
                    result.Add(new ActionError(1506));
                }
                else
                {
                    short setNumber = 0;
                    setNumber = Convert.ToInt16(order.FieldValue);

                    if (!ACBAOperationService.CheckCustomerInvolvingEmployeeFilial(setNumber, (short)deposit.FilailCode))
                    {
                        //Նշված համարով ՊԿ տվյալ մասնաճյուղում գոյություն չունի:
                        result.Add(new ActionError(704));
                    }
                }

            }




            return result;
        }

        /// <summary>
        /// Ստացողի հաշվի համարի ստուգումներ
        /// </summary>
        /// <returns></returns>
        internal static List<ActionError> ValidateCashBookReceiverAccount(CashBookOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.CreditAccount == null || order.CreditAccount.AccountNumber == "0")
            {
                ///Մուտքագրվող (կրեդիտ) հաշիվը մուտքագրված չէ:
                result.Add(new ActionError(18));
            }
            else
            {
                if (order.CreditAccount.ClosingDate != null)
                {
                    //Կրեդիտային հաշիվը փակ է:
                    result.Add(new ActionError(555, new string[] { order.CreditAccount.AccountNumber.ToString() }));
                }

                //Եթե քարտային հաշիվ է
                if (order.CreditAccount.IsCardAccount())
                {
                    //Եթե առկա չէ գործող քարտ
                    if (!order.CreditAccount.HaveActiveCard())
                    {
                        //Տվյալ հաշվին փոխանցում կատարել հնարավոր չէ: Մուտքագրվող (կրեդիտ) հաշիվը գոյություն չունի: 
                        result.Add(new ActionError(452));
                        return result;
                    }
                }
                //Եթե ավանդային հաշիվ է
                else if (order.CreditAccount.IsDepositAccount())
                {
                    //Երե առկա չէ գործող ավանդ
                    if (!order.CreditAccount.HasActiveDeposit())
                    {
                        //Տվյալ հաշվին փոխանցում կատարել հնարավոր չէ: Մուտքագրվող (կրեդիտ) հաշիվը գոյություն չունի: 
                        result.Add(new ActionError(452));
                        return result;
                    }
                }
                //Եթե ոչ ավանդային է և ոչ էլ քարտային , ապա պետք է լինի ընթացիկ
                else if (!order.CreditAccount.IsCurrentAccount())
                {
                    result.Add(new ActionError(452));
                    return result;
                }

                ///Մուտքային հաշվի ստուգում:
                string recAcc = order.CreditAccount.AccountNumber.ToString();

                if (order.SubType == 1 && !(Utility.IsCorrectAccount(recAcc, 24, 10) ||
                    Utility.IsCorrectAccount(recAcc, 24, 13) ||
                    Utility.IsCorrectAccount(recAcc, 24, 11) ||
                    Utility.IsCorrectAccount(recAcc, 24, 18)
                    ))
                {

                    //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                    result.Add(new ActionError(19));
                }

                if (order.CreditAccount.Status == 1)
                {
                    //Ժամանակավոր հաշվով գործարքների իրականացումն արգելված է
                    result.Add(new ActionError(545, new string[] { order.CreditAccount.AccountNumber.ToString() }));
                }
            }
            return result;
        }

        internal static bool HasOverdueLoan(Account debitAccount, short strictOverdueLoan, short notStrictDebtType)
        {
            return ValidationDB.HasOverdueLoan(debitAccount, strictOverdueLoan, notStrictDebtType);
        }


        /// <summary>
        /// Պահատուփի տուժանքի դադարեցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            DepositCase depositCase = new DepositCase();
            depositCase = DepositCase.GetDepositCase(order.ProductId, order.CustomerNumber);
            if (depositCase == null)
            {
                //Պահատուփը գտնված չէ:
                result.Add(new ActionError(879));
            }
            else
            {
                if (DepositCaseStoppingPenaltyCalculationOrder.IsSecondPenaltyStoppingOrder(order.CustomerNumber, order.ProductId))
                {
                    //Տվյալ պահատուփի համար գոյություն ունի տույժի հաշվեգրման դադարեցման չհաստատված հայտ:
                    result.Add(new ActionError(1276));

                }
                else if (depositCase.Quality != 5)
                {
                    //Տույժի հաշվեգրումը դադարեցնել հնարավոր է միայն ժամկետանց կարգավիճակով պահատուփերի համար:
                    result.Add(new ActionError(1275));
                }
            }


            return result;
        }

        internal static bool IsBankOpen(double bankCode)
        {
            return ValidationDB.IsBankOpen(bankCode);
        }


        internal static List<ActionError> CheckForRegisterRequestData(double applicationID, double customerNumber, int requestType, SourceType sourceType)
        {
            return ValidationDB.CheckForRegisterRequestData(applicationID, customerNumber, requestType, sourceType);
            //return ValidationDB.CheckForRegisterRequestData(applicationID, 100000007716, requestType, sourceType);

        }

        internal static List<ActionError> ValidateLoanMonitoringConclusion(long monitoringId)
        {
            List<ActionError> result = new List<ActionError>();
            if (!LoanMonitoringConclusion.IsSetProvisionConclusions(monitoringId))
            {
                result.Add(new ActionError(1368));
            }

            return result;
        }

        /// <summary>
        /// Լիազորագրի ատիվացման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateCredentialActivationOrder(CredentialActivationOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.Type == OrderType.CredentialActivationOrder && order.Credential.GivenByBank)
            {
                result.AddRange(ValidateDebitAccount(order, order.DebitAccount));
            }
            return result;
        }

        /// <summary>
        /// Ցպահանջ ավանդի տոկոսադրույքի փոփոխման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.DemandDepositAccount == null || order.DemandDepositAccount.AccountNumber == "0"
                || string.IsNullOrEmpty(order.DemandDepositAccount.AccountNumber))
            {
                //Հաշիվը ընտրված չէ:
                result.Add(new ActionError(55));
                return result;
            }
            if (order.DemandDepositAccount.Currency != "AMD" && order.TariffGroup != 2 && order.TariffGroup != 1)
            {
                //Հաշիվը դրամային չէ:
                result.Add(new ActionError(752));
            }
            else if (order.DemandDepositAccount.ClosingDate != null)
            {
                //Հաշիվը արդեն փակված է
                result.Add(new ActionError(506, new string[] { order.DemandDepositAccount.AccountNumber }));
            }
            else
            {
                DemandDepositRate demandDepositRate = DemandDepositRate.GetDemandDepositRate(order.DemandDepositAccount.AccountNumber);
                if (order.TariffGroup != 1 && demandDepositRate.TariffGroup != 1)
                {
                    //Անհրաժեշտ է նախ անցում կատարել Հիմնական սակագնին
                    result.Add(new ActionError(1397));
                }
            }

            byte customerType = Customer.GetCustomerType(order.CustomerNumber);
            if (customerType == (byte)CustomerTypes.physical)
            {
                //Գործողությունը հասանաելի է միայն ոչ ֆիզիկական անձանց համար
                result.Add(new ActionError(1384));
            }

            //short customerFilialcode = Customer.GetCustomerFilial(order.CustomerNumber).key;
            //if (customerFilialcode!=order.user.filialCode)
            //{
            //    //Այլ մասնաճյուղից է։
            //    result.Add(new ActionError(1073));
            //}

            if (order.DocumentDate == null || string.IsNullOrEmpty(order.DocumentNumber))
            {
                //Հրամանի ա/թ-ն կամ հրամանի համարը լրացված չէ
                result.Add(new ActionError(1398));
            }


            return result;
        }

        /// <summary>
        /// Դասակարգված վարկերի հետ բերման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateLoanProductClassificationRemoveOrder(LoanProductClassificationRemoveOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.IsExistsNotConfirmedOrder())
                result.Add(new ActionError(1399)); //Տվյալ վարկի համար արդեն գոյություն ունի հետ բերման չկատարված հայտ:

            if (order.IsQualityWrong())
                result.Add(new ActionError(1402)); //Տվյալ կարգավիճակով  վարկը հնարավոր չէ հետ բերել:

            return result;
        }
        /// <summary>
        /// Դասակարգված վարկի դուրսգրման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateLoanProductMakeOutOrder(LoanProductMakeOutOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.IsExistsNotConfirmedOrder())
                result.Add(new ActionError(1400)); //Տվյալ վարկի համար արդեն գոյություն ունի դուրսգրման չկատարված հայտ:

            if (order.IsQualityWrong())
                result.Add(new ActionError(1401)); //Տվյալ կարգավիճակով  վարկը հնարավոր չէ դուրս գրել:

            return result;
        }

        /// <summary>
        ///  MR ծառայություն հայտերի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateMembershipRewardsOrder(MembershipRewardsOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            MembershipRewards membershipReward = MembershipRewards.GetCardMembershipRewardByID(order.ProductId);
            Card card = Card.GetCardMainData(membershipReward.CardNumber);

            if (order.Type == OrderType.CardMRReNewOrder)
            {
                if (membershipReward.Status != 2)
                {
                    //Հնարավոր չէ վերաթողարկել: Խնդրում ենք գրանցել նոր MR ծառայություն:
                    result.Add(new ActionError(1420));
                    return result;
                }

                if (membershipReward.EndDate >= card.ValidationDate)
                {
                    //Հնարավոր չէ վերաթողարկել տվյալ MR ծառայությունը: Քարտի գործողության ժամկետը փոքր է MR ծառայության ժամկետի վերջից:
                    result.Add(new ActionError(1422));
                    return result;
                }

            }
            else if (order.Type == OrderType.CardMRCancelOrder)
            {
                if (membershipReward.Status != 1 && membershipReward.Status != 2)
                {
                    //Տվյալ կարգավիճակով MR ծառայությունը հնարավոր չէ դադարեցնել:
                    result.Add(new ActionError(1421));
                    return result;
                }
            }

            return result;
        }

        internal static List<ActionError> ValidateCardUSSDServiceOrder(CardUSSDServiceOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            if (order.ActionType != 1 && order.ActionType != 2 && order.ActionType != 3)
            {
                //Գործողության տեսակը սխալ է։
                result.Add(new ActionError(1435));
            }

            if (CardUSSDServiceOrder.IsSecondUSSDServiceOrder(order.ProductID) == true)
            {
                //Նշված քարտի համար արդեն մուտքագրված է տվյալ տեսակի հայտ։
                result.Add(new ActionError(1434));
            }

            string canChangeCardService = null;
            order.user.AdvancedOptions.TryGetValue("canChangeCardService", out canChangeCardService);


            Card card = Card.GetCardWithOutBallance(order.ProductID);

            if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }

            if (canChangeCardService != "1" && order.user.filialCode != card.FilialCode)
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
                return result;
            }

            if (string.IsNullOrEmpty(order.MobilePhone) || order.MobilePhone.Substring(0, 3) != "374" || order.MobilePhone.Length < 11)
            {
                //Բջջային հեռախոսի ֆորմատը սխալ է (ճիշտ ֆորմատը՝ 374XXYYYYYY)
                result.Add(new ActionError(1433));
            }

            CardServiceQualities response = Card.GetCardUSSDService(order.ProductID);

            if ((response == CardServiceQualities.NotRegistered || response == CardServiceQualities.TerminateConfirmedInArca) && (order.ActionType == 2 || order.ActionType == 3))
            {
                //Հնարավոր չէ կատարել ընտրված գործողությունը, քանի որ տվյալ քարտի համար դեռևս չի կատարվել գրանցել գործողությունը:
                result.Add(new ActionError(1436));
            }

            if ((response == CardServiceQualities.RegistrateNotConfirmedInArca && order.ActionType != 1) || (response == CardServiceQualities.TerminateNotConfirmedInArca && order.ActionType != 2) || (response == CardServiceQualities.ChangeNotConfirmedInArca && order.ActionType != 3))
            {
                //Քարտի համար կա մուտքագրված Գրանցել/Հանել հայտ, որը դեռևս հաստատված չէ
                result.Add(new ActionError(1437));
            }

            if (((response == CardServiceQualities.RegistrateConfirmedInArca || response == CardServiceQualities.ChangeConfirmedInArca) && order.ActionType == 1) || (response == CardServiceQualities.TerminateConfirmedInArca && order.ActionType == 2))
            {
                //Քարտի համար արդեն կատարվել է {0} գործողությունը
                result.Add(new ActionError(1438, new string[] { (order.ActionType == 1) ? "Գրանցել" : "Հանել" }));
            }


            if (card != null && card.ClosingDate != null)
            {
                //Ընտրված է փակված քարտ
                result.Add(new ActionError(532));
            }

            return result;
        }



        internal static List<ActionError> ValidatePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            int responseCode; int lastAction;

            if (string.IsNullOrEmpty(order.MobilePhone) && order.OperationType != 2)
            {
                //Հեռախոսահամարը մուտքագրված չէ:
                result.Add(new ActionError(1027));
            }

            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496, new string[] { "Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում" }));
            }

            if (order.OperationType != 1 && order.OperationType != 2 && order.OperationType != 3)
            {
                //Գործողության տեսակը սխալ է։
                result.Add(new ActionError(1435, new string[] { "Գործողության տեսակը սխալ է" }));
            }

            if (PlasticCardSMSServiceOrder.IsSecondSMSServiceOrder(order.ProductID, order.OperationType) == true && order.Id == 0)
            {
                //Նշված քարտի համար արդեն մուտքագրված է տվյալ տեսակի հայտ։
                result.Add(new ActionError(1434, new string[] { "Նշված քարտի համար արդեն մուտքագրված է տվյալ տեսակի հայտ" }));
            }

            Card card = Card.GetCardWithOutBallance(order.ProductID);

            if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }


            if (order.Source == SourceType.Bank)
            {
                if (order.IsArmenia && order.OperationType != 2 && (string.IsNullOrEmpty(order.MobilePhone) || order.MobilePhone.Substring(0, 3) != "374" || order.MobilePhone.Length < 11))
                {
                    //Բջջային հեռախոսի ֆորմատը սխալ է (ճիշտ ֆորմատը՝ 374XXYYYYYY)
                    result.Add(new ActionError(1433, new string[] { "Բջջային հեռախոսի ֆորմատը սխալ է(ճիշտ ֆորմատը՝ 374XXYYYYYY) " }));
                }
                if (!order.IsArmenia)
                {
                    order.MobilePhone = "00" + order.MobilePhone;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(order.MobilePhone) || order.MobilePhone.Substring(0, 3) != "374" || order.MobilePhone.Length < 11)
                {
                    //Բջջային հեռախոսի ֆորմատը սխալ է (ճիշտ ֆորմատը՝ 374XXYYYYYY)
                    result.Add(new ActionError(1433, new string[] { "Բջջային հեռախոսի ֆորմատը սխալ է(ճիշտ ֆորմատը՝ 374XXYYYYYY) " }));
                }
            }
            CardServiceQualities response;
            if (order.Source == SourceType.Bank)
            {
                response = CardDB.GetPlasticCardSMSService(order.CardNumber, true);
            }
            else
            {
                response = Card.GetPlasticCardSMSService(order.CardNumber);
            }

            if (response == CardServiceQualities.NotRegistered && order.OperationType != 1)
            {
                //Հնարավոր չէ կատարել ընտրված գործողությունը, քանի որ տվյալ քարտի համար դեռևս չի կատարվել գրանցել գործողությունը:
                result.Add(new ActionError(1436));
            }

            if (order.Source != SourceType.Bank && ((response == CardServiceQualities.RegistrateNotConfirmedInArca && order.OperationType != 1) || (response == CardServiceQualities.TerminateNotConfirmedInArca && order.OperationType != 2) || (response == CardServiceQualities.ChangeNotConfirmedInArca && order.OperationType != 3)))
            {
                //Քարտի համար կա մուտքագրված Գրանցել/դադարեցնել հայտ, որը դեռևս հաստատված չէ
                result.Add(new ActionError(1437));
            }

            if (((response == CardServiceQualities.RegistrateConfirmedInArca || response == CardServiceQualities.ChangeConfirmedInArca) && order.OperationType == 1) || (response == CardServiceQualities.TerminateConfirmedInArca && order.OperationType == 2))
            {
                //Քարտի համար արդեն կատարվել է {0} գործողությունը
                result.Add(new ActionError(1438, new string[] { (order.OperationType == 1) ? "Գրանցել" : "Դադարեցնել" }));
            }
            if (order.Source == SourceType.Bank)
            {
                PlasticCardSMSServiceOrderDB.GetResponseCodeAndLastActionFromCardSms(order.ProductID, out responseCode, out lastAction);
                //(@operationType in (2,3)  and ((@lastAction = 2 and @responseCode = 0) or @lastAction = -1)) 
                if (order.OperationType != 1 && ((lastAction == 2 && responseCode == 0) || lastAction == -1))
                {
                    //Հնարավոր չէ կատարել ընտրված գործողությունը, քանի որ տվյալ քարտի համար դեռևս չի կատարվել գրանցել գործողությունը
                    result.Add(new ActionError(1920));
                }
                if (PlasticCardSMSServiceOrderDB.CheckASWACardSMS(order.ProductID))
                {//'Քարտի համար գեներացված է այլ տեսակի ծառայություն, որը դեռևս հաստատված չէ։'
                    result.Add(new ActionError(1922));
                }
                if (PlasticCardSMSServiceOrderDB.IsBTRTFileCreated(order.CardNumber))
                {//'Քարտի համար գեներացված է SMS ծառայություն, որը դեռևս հաստատված չէ'
                    result.Add(new ActionError(1921));
                }
            }

            if (card != null && card.ClosingDate != null)
            {
                //Ընտրված է փակված քարտ
                result.Add(new ActionError(532));
            }
            //«Նվազագույն գումար»-ը սխալ է լրացված: 
            if (!double.TryParse(order.SMSFilter, out double price))
            {
                if (!(order.Source == SourceType.Bank))
                {
                    result.Add(new ActionError(1897));
                }
                else if (!(order.SMSType == 4 && order.SMSFilter == "N" && order.OperationType == 2))
                {
                    result.Add(new ActionError(1897));
                }
            }
            else
                order.SMSFilter = ((int)price).ToString();

            return result;
            ////«Նվազագույն գումար»-ը սխալ է լրացված: 
            //if (!Regex.IsMatch(order.SMSFilter, @"^\d+$"))
            //{
            //    if (!(order.Source == SourceType.Bank))
            //    {
            //        result.Add(new ActionError(1897));
            //    }
            //    else if (!(order.SMSType == 4 && order.SMSFilter == "N"))
            //    {
            //        result.Add(new ActionError(1897));
            //    }
            //}
            //else
            //    order.SMSFilter = (int.Parse(order.SMSFilter)).ToString();
            //return result;
        }


        public static List<ActionError> ValidateProductNotificationConfigurationsOrder(ProductNotificationConfigurationsOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.Type == OrderType.ProductNotificationConfigurationsOrder && ProductNotificationConfigurationsOrderDB.IsExist(order))
            {
                //Գույություն ունի կարգավորում տվյալ ինֆորմացիայի  տեսակի համար 
                result.Add(new ActionError(1431));
            }

            if (order.Type == OrderType.ProductNotificationConfigurationsOrder && order.Configuration.ProductType == 6 && (order.Configuration.InformationType != 1 && order.Configuration.InformationType != 2))
            {
                //Գոյություն չունի տվյալ ինֆորմացիայի տեսակ այս պրոդուկտի համար
                result.Add(new ActionError(1494));
            }

            if (order.Type == OrderType.ProductNotificationConfigurationsUpdateOrder && order.Configuration.ProductType == 6 && (order.Configuration.InformationType != 1 && order.Configuration.InformationType != 2))
            {
                //Գոյություն չունի տվյալ ինֆորմացիայի տեսակ այս պրոդուկտի համար
                result.Add(new ActionError(1494));

            }
            return result;
        }



        public static List<ActionError> ValidateTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (!SwiftMessage.CheckSentSwiftStatus(order.SwiftMessageId))
            {
                result.Add(new ActionError(1442));
            }

            if (IsSwiftMessageConfirmed(order.SwiftMessageId))
            {
                result.Add(new ActionError(1450));
            }


            return result;
        }


        public static bool IsSwiftMessageConfirmed(int SwiftMessageID)
        {
            return ValidationDB.IsSwiftMessageConfirmed(SwiftMessageID);
        }

        public static List<ActionError> ValidateCard3DSecureServiceOrder(Card3DSecureServiceOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            if (order.Card3DSecureService.ActionType != 1 && order.Card3DSecureService.ActionType != 2 && order.Card3DSecureService.ActionType != 3)
            {
                //Գործողության տեսակը սխալ է։
                result.Add(new ActionError(1435));
            }

            if (Card3DSecureServiceOrderDB.IsSecondCard3DServiceOrder(order.Card3DSecureService.ProductID) == true)
            {
                //Նշված քարտի համար արդեն մուտքագրված է տվյալ տեսակի հայտ։
                result.Add(new ActionError(1434));
            }



            CardServiceQualities response = (Card.GetCard3DSecureService(order.Card3DSecureService.ProductID)).Quality;

            if ((response == CardServiceQualities.NotRegistered || response == CardServiceQualities.TerminateConfirmedInArca) && (order.Card3DSecureService.ActionType == 2 || order.Card3DSecureService.ActionType == 3))
            {
                //Հնարավոր չէ կատարել ընտրված գործողությունը, քանի որ տվյալ քարտի համար դեռևս չի կատարվել գրանցել գործողությունը:
                result.Add(new ActionError(1436));
            }

            if ((response == CardServiceQualities.RegistrateNotConfirmedInArca && order.Card3DSecureService.ActionType != 1) || (response == CardServiceQualities.TerminateNotConfirmedInArca && order.Card3DSecureService.ActionType != 2) || (response == CardServiceQualities.ChangeNotConfirmedInArca && order.Card3DSecureService.ActionType != 3))
            {
                //Քարտի համար կա մուտքագրված Գրանցել/Հանել հայտ, որը դեռևս հաստատված չէ
                result.Add(new ActionError(1437));
            }

            if (((response == CardServiceQualities.RegistrateConfirmedInArca || response == CardServiceQualities.ChangeConfirmedInArca) && order.Card3DSecureService.ActionType == 1) || (response == CardServiceQualities.TerminateConfirmedInArca && order.Card3DSecureService.ActionType == 2))
            {
                //Քարտի համար արդեն կատարվել է {0} գործողությունը
                result.Add(new ActionError(1438, new string[] { (order.Card3DSecureService.ActionType == 1) ? "Գրանցել" : "Հանել" }));
            }

            Card card = Card.GetCardWithOutBallance(order.Card3DSecureService.ProductID);

            if (card != null && card.ClosingDate != null)
            {
                //Ընտրված է փակված քարտ
                result.Add(new ActionError(532));
            }
            string canChangeCardService = null;
            order.user.AdvancedOptions.TryGetValue("canChangeCardService", out canChangeCardService);

            if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }

            if (canChangeCardService != "1" && order.user.filialCode != card.FilialCode)
            {
                //Քարտը պատկանում է այլ մասնաճյուղի
                result.Add(new ActionError(905));
                return result;
            }

            return result;
        }

        /// <summary>
        /// Պարտատոմսի թողարկման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondIssue(BondIssue bondIssue)
        {
            List<ActionError> result = new List<ActionError>();

            Action action = bondIssue.ID == 0 ? Action.Add : Action.Update;

            if (bondIssue.IssuerType == BondIssuerType.ACBA)
            {
                BondIssueFilter bondIssueFilter = new BondIssueFilter();
                bondIssueFilter.ISIN = bondIssue.ISIN;
                bondIssueFilter.Quality = BondIssueQuality.Approved;

                List<BondIssue> bondIssues = BondIssueFilter.SearchBondIssues(bondIssueFilter);
                if (bondIssues.Count > 0 && bondIssue.ID == 0)
                {
                    //Տվյալ ԱՄՏԾ-ով պարտատոմսի թողարկում արդեն գոյություն ունի։
                    result.Add(new ActionError(1454));
                    return result;
                }

                if (bondIssue.MinSaleQuantity > bondIssue.MaxSaleQuantity)
                {
                    //Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող պարտատոմսերի նվազագույն ձեռք բերման քանակը պետք է չգերազանցի առավելագույն քանակը։
                    result.Add(new ActionError(1427));
                }
                if (bondIssue.MaxSaleQuantity > bondIssue.TotalCount)
                {
                    //Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող պարտատոմսերի առավելագույն ձեռք բերման քանակը մեծ է պարտատոմսերի ընդհանուր քանակից
                    result.Add(new ActionError(1477));
                }

                if (bondIssue.TotalVolume % bondIssue.NominalPrice != 0)
                {
                    //Թողարկման ընդհանուր ծավալը և/կամ մեկ պարտատոմսի անվանական արժեքը սխալ է մուտքագրված։
                    result.Add(new ActionError(1441));
                }

                if (bondIssue.CouponPaymentPeriodicity != 1 && bondIssue.CouponPaymentPeriodicity != 2 && bondIssue.CouponPaymentPeriodicity != 4)
                {
                    //Արժեկտրոնների վճարման պարբերականությունը սխալ է մուտքագրված։
                    result.Add(new ActionError(1440));
                }

                if (bondIssue.RepaymentDate < bondIssue.ReplacementEndDate)
                {
                    //Պարտատոմսերի մարման օրը փոքր է տեղաբաշխման ավարտից։
                    result.Add(new ActionError(1430));
                }

                if (bondIssue.ReplacementEndDate < bondIssue.ReplacementDate)
                {
                    //Տեղաբաշխման ավարտը փոքր է տեղաբաշխման սկզբից։
                    result.Add(new ActionError(1456));
                }
                if (bondIssue.CouponPaymentCount == 0)
                {
                    //Հնարավոր չէ հաշվարկել արժեկտրոնների հաշվարկման օրերը։ Խնդրում ենք ստուգել մուտքագրված տվյալները։
                    result.Add(new ActionError(1479));
                }

                if (bondIssue.BankAccount == null || bondIssue.BankAccount.AccountNumber == null)
                {
                    //Պարտատոմսերի ձեռքբերման Բանկի հաշվեհամարը մուտքագրված չէ։
                    result.Add(new ActionError(1480));
                }

                else
                {
                    Account bankAccount = Account.GetAccountFromAllAccounts(bondIssue.BankAccount.AccountNumber);
                    if (bankAccount == null)
                    {
                        //Բանկի հաշվեհամարը սխալ է մուտքագրված:
                        result.Add(new ActionError(1481));
                    }
                    else
                    {
                        if (bankAccount.Currency != bondIssue.Currency)
                        {
                            //Բանկի հաշվեհամարի արժույթը չի համապատասխանում թողարկման արժույթին։
                            result.Add(new ActionError(1490));
                        }
                    }
                }

                if (action == Action.Add && bondIssue.ReplacementDate < DateTime.Now.Date)
                {
                    //Տեղաբաշխման սկիզբը փոքր է այսօրվա ամսաթվից:
                    result.Add(new ActionError(1489));
                }

            }

            if (bondIssue.PeriodType == BondIssuePeriod.LongTerm && bondIssue.InterestRate == 0)
            {
                //Տարեկան արժեկտրոնային դրույքը լրացված չէ։
                result.Add(new ActionError(1444));
            }

            if (bondIssue.RepaymentDate < bondIssue.IssueDate)
            {
                //Մարման օրը սխալ է մուտքագրված։
                result.Add(new ActionError(1443));
            }

            if (bondIssue.ID != 0 && bondIssue.IssuerType != BondIssuerType.ACBA && bondIssue.Quality != BondIssueQuality.New)
            {
                //Հնարավոր չէ խմբագրել տվյալ կարգավիճակով պարտատոմսի թողարկումը։
                result.Add(new ActionError(1459));
            }

            if (bondIssue.Currency != "AMD" && bondIssue.Currency != "USD")
            {
                //Մուտքագրված արժույթը սխալ է
                result.Add(new ActionError(777));
            }

            return result;
        }

        /// <summary>
        /// Պարտատոմսի ակտիվացման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondIssueForApprove(BondIssue bondIssue)
        {
            List<ActionError> result = new List<ActionError>();

            if (bondIssue.Quality != BondIssueQuality.New)
            {
                //Տվյալ կարգավիճակով պարտատոմսը հնարավոր չէ ակտիվացնել։
                result.Add(new ActionError(1428));
                return result;
            }

            return result;
        }

        /// <summary>
        /// Պարտատոմսի հեռացման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondIssueForDelete(BondIssue bondIssue)
        {
            List<ActionError> result = new List<ActionError>();

            if (bondIssue.Quality != BondIssueQuality.New)
            {
                //Տվյալ կարգավիճակով պարտատոմսը հնարավոր չէ հեռացնել։
                result.Add(new ActionError(1429));
                return result;
            }

            return result;
        }

        /// <summary>
        ///  Պարտատոմսի ձեռքբերման(վաճառքի) գրանցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondOrder(BondOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            BondIssueFilter bondIssueFilter = new BondIssueFilter();
            bondIssueFilter.ISIN = order.Bond.ISIN;
            bondIssueFilter.Quality = BondIssueQuality.Approved;

            List<BondIssue> bondIssues = BondIssueFilter.SearchBondIssues(bondIssueFilter);
            if (bondIssues.Count == 0)
            {
                //Տվյալ ԱՄՏԾ-ով գործող պարտատոմսի թողարկում գոյություն չունի։
                result.Add(new ActionError(1445));
                return result;
            }
            else
            {
                BondIssue bondIssue = bondIssues.First();


                BondFilter bondFilter = new BondFilter();
                bondFilter.BondIssueId = order.Bond.BondIssueId;
                bondFilter.Quality = BondQuality.None;
                bondFilter.CustomerNumber = order.CustomerNumber;
                List<Bond> customerBonds = Bond.GetBonds(bondFilter);

                customerBonds.RemoveAll(b => b.Quality == BondQuality.Deleted || b.Quality == BondQuality.Rejected);

                if (order.Bond.ShareType == SharesTypes.Stocks)
                {
                    customerBonds.RemoveAll(b => b.Quality == BondQuality.AvailableForApproveDiling || b.Quality == BondQuality.AvailableForApproveDilingBackOffice);
                }

                int customerBondsCount = 0;
                if (customerBonds.Count > 0)
                {
                    customerBondsCount = customerBonds.Sum(m => m.BondCount);
                }

                int notDistibutedBondsCount = BondIssue.GetNonDistributedBondsCount(order.Bond.BondIssueId);

                if (notDistibutedBondsCount == 0)
                {
                    //Տվյալ թողարկման համար գոյություն չունեն չտեղաբաշխված պարտատոմսեր։
                    result.Add(new ActionError(2040));
                }
                else
                {
                    if (customerBondsCount + order.Bond.BondCount < bondIssue.MinSaleQuantity)
                    {
                        //Ձեռք բերվող պարտատոմսերի քանակը փոքր է տվյալ թողարկման համար նախատեսված նվազագույն ձեռք բերման քանակից։
                        result.Add(new ActionError(1446));
                    }
                    if (customerBondsCount + order.Bond.BondCount > bondIssue.MaxSaleQuantity)
                    {
                        //Ձեռք բերվող պարտատոմսերի քանակը մեծ է տվյալ թողարկման համար նախատեսված առավելագույն ձեռք բերման քանակից։
                        result.Add(new ActionError(1447));
                    }

                    if ((notDistibutedBondsCount - order.Bond.BondCount != 0) && (notDistibutedBondsCount - order.Bond.BondCount) < bondIssue.MinSaleQuantity)
                    {
                        //Նշված քանակի պարտատոմսերի տրամադրման դեպքում չտեղաբաշխված պարտատոմսերի քանակը փոքր կլինի մեկ ներդրողի նկատմամբ կիրառվող ձեռքբերվող 
                        //պարտատոմսերի ծավալի նվազագույն շեմից։
                        result.Add(new ActionError(1448));
                    }

                    if (DateTime.Now.Date > bondIssue.ReplacementEndDate || DateTime.Now.Date < bondIssue.ReplacementDate)
                    {
                        //Նշված թողարկումը ենթակա չէ տեղաբաշխման՝ համաձայն տեղաբաշխման ժամկետի։
                        result.Add(new ActionError(1457));
                    }
                }

                if (order.Bond.ShareType == SharesTypes.Bonds && (order.Attachments == null || order.Attachments.Count < 1) && order.Bond.DepositaryAccountExistenceType != DepositaryAccountExistence.ExistsInBank)
                {
                    //Փաստաթղթերը կցված չեն։
                    result.Add(new ActionError(1462));
                }

                if (order.Attachments != null)
                {
                    if (order.Attachments.Count > 30)
                    {
                        //Կցված փաստաթղթերի քանակը գերազանցում է թույլատրելի քանակը։
                        result.Add(new ActionError(1463));
                    }
                    else
                    {
                        if (order.Attachments.Exists(att => att.FileExtension != ".jpg" && att.FileExtension != ".png" && att.FileExtension != ".pdf" && att.FileExtension != ".jpeg"))
                        {
                            //Փաստաթղթերը պետք է լինեն .jpg/.jpeg/.pdf/.png տեսակի
                            result.Add(new ActionError(1465));
                        }
                    }
                }



                if (DepositaryAccount.HasCustomerDepositaryAccount(order.CustomerNumber) && order.Bond.ShareType == SharesTypes.Bonds)
                {

                    if (order.Bond.DepositaryAccountExistenceType != DepositaryAccountExistence.ExistsInBank)
                    {
                        //Նշված հաճախորդի համար առկա է արժեթղթերի հաշիվ Բանկի բազայում։
                        result.Add(new ActionError(1466));
                    }
                }
                else
                {
                    if (order.Bond.ShareType != SharesTypes.Stocks)
                    {
                        if (order.Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.Exists)
                        {
                            if (order.Bond.CustomerDepositaryAccount == null || order.Bond.CustomerDepositaryAccount.AccountNumber == 0 || order.Bond.CustomerDepositaryAccount.BankCode == 0)
                            {
                                //Արժեթղթերի հաշվի տվյալները լիարժեք լրացված չեն։
                                result.Add(new ActionError(1467));
                            }
                        }
                    }

                }
                if (order.Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.None)
                {
                    //Արժեթղթերի հաշվի առկայության տեսակը մուտքագրված չէ։
                    result.Add(new ActionError(1469));
                }

                if (order.Bond.ShareType == SharesTypes.Stocks)
                {
                    if (order.Bond.SecuringMoney == true)
                    {
                        double debitAccountBalance = Account.GetAccountAvailableBalanceForStocksInAmd(order.Bond.AccountForBond.AccountNumber);
                        if (order.Bond.TotalPrice > debitAccountBalance)
                        {
                            //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                            result.Add(new ActionError(1099, new string[] { order.Bond.AccountForBond.AccountNumber, "գործարքը կատարելու" }));
                        }
                    }
                }

            }

            return result;
        }


        /// <summary>
        ///  Պարտատոմսի կարգավիճակի փոփոխման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondQualityUpdateOrder(BondQualityUpdateOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            Bond bond = Bond.GetBondByID(order.BondId);

            //Պարտատոմսի հեռացման հայտ
            if (order.SubType == 1)
            {
                if (bond == null || bond.Quality != BondQuality.New)
                {
                    //Տվյալ կարգավիճակով պարտատոմսը հնարավոր չէ հեռացնել։
                    result.Add(new ActionError(1461));
                }

                if (BondQualityUpdateOrder.ExistsNotConfirmedBondQualityUpdateOrder(order.CustomerNumber, order.SubType, order.BondId))
                {
                    //Տվյալ պարտատոմսի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված հեռացման հայտ:
                    result.Add(new ActionError(1473));
                }

            }
            //Պարտատոմսի հաստատման հայտ
            else if (order.SubType == 2)
            {
                if (bond.Quality != BondQuality.AvailableForApprove)
                {
                    //Տվյալ կարգավիճակով պարտատոմսը հնարավոր չէ հաստատել։
                    result.Add(new ActionError(1471));
                }
                else if (!DepositaryAccount.HasCustomerDepositaryAccount(order.CustomerNumber))
                {
                    //Առկա չէ արժեթղթերի հաշիվ Բանկի բազայում։
                    result.Add(new ActionError(1472));
                }

                if (BondQualityUpdateOrder.ExistsNotConfirmedBondQualityUpdateOrder(order.CustomerNumber, order.SubType, order.BondId))
                {
                    //Տվյալ պարտատոմսի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված հաստատման հայտ:
                    result.Add(new ActionError(1474));
                }
            }
            //Մերժման հայտ
            else if (order.SubType == 3)
            {
                if (bond.Quality != BondQuality.AvailableForApprove && bond.Quality != BondQuality.New && bond.ShareType != SharesTypes.Stocks)
                {
                    //Տվյալ կարգավիճակով պարտատոմսը հնարավոր չէ մերժել։
                    result.Add(new ActionError(1491));
                }
                else
                {
                    if (order.ReasonId == 0 || (order.ReasonId == BondRejectReason.Other && String.IsNullOrEmpty(order.ReasonDescription)))
                    {
                        //Մերժման պատճառը/նկարագրությունը մուտքագրված չէ։
                        result.Add(new ActionError(1492));
                    }

                    if (BondQualityUpdateOrder.ExistsNotConfirmedBondQualityUpdateOrder(order.CustomerNumber, order.SubType, order.BondId))
                    {
                        //Տվյալ պարտատոմսի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված մերժման հայտ:
                        result.Add(new ActionError(1493));
                    }
                }

            }



            return result;
        }


        /// <summary>
        ///  Պարտատոմսի գումարի գանձման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateBondAmountChargeOrder(BondAmountChargeOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            Bond BondDetails = Bond.GetBondByID(order.Bond.ID);

            if (BondDetails == null || BondDetails.Quality != BondQuality.New)
            {
                //Տվյալ կարգավիճակով պարտատոմսի համար գումարի գանձումը հնարավոր չէ։
                result.Add(new ActionError(1476));
                return result;
            }


            Account transitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.DebitAccount), order.Currency, order.user.filialCode);

            double debitAccountBalance = Account.GetAcccountAvailableBalance(transitAccount.AccountNumber);

            if (BondDetails.Currency != "AMD")
            {
                if (order.IsCashInTransit == 1)
                {
                    if (debitAccountBalance < Math.Round(BondDetails.TotalPrice, 0))
                    {
                        //Տարանցիկ հաշվի մնացորդը բավարար չէ գործարք կատարելու համար:
                        result.Add(new ActionError(1484));
                    }
                }
                else
                {
                    if (debitAccountBalance < Math.Round(BondDetails.TotalPrice, 2))
                    {
                        //Տարանցիկ հաշվի մնացորդը բավարար չէ գործարք կատարելու համար:
                        result.Add(new ActionError(1484));
                    }
                }
            }
            else
            {
                if (debitAccountBalance < Math.Round(BondDetails.TotalPrice, 1))
                {
                    //Տարանցիկ հաշվի մնացորդը բավարար չէ գործարք կատարելու համար:
                    result.Add(new ActionError(1484));
                }
            }





            if (BondAmountChargeOrder.ExistsNotConfirmedBondAmountChargeOrder(order.CustomerNumber, order.SubType, order.Bond.ID))
            {
                //Տվյալ պարտատոմսի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված գումարի գանձման հայտ:
                result.Add(new ActionError(1486));
            }

            /////////Փաստաթղթեր           
            if (order.Attachments == null || order.Attachments.Count < 1)
            {
                //Փաստաթղթերը կցված չեն։
                result.Add(new ActionError(1462));
            }

            else if (order.Attachments.Count > 15)
            {
                //Կցված փաստաթղթերի քանակը գերազանցում է թույլատրելի քանակը։
                result.Add(new ActionError(1463));
            }
            else
            {
                if (order.Attachments.Exists(att => att.FileExtension != ".jpg" && att.FileExtension != ".png" && att.FileExtension != ".pdf" && att.FileExtension != ".jpeg"))
                {
                    //Փաստաթղթերը պետք է լինեն .jpg/.jpeg/.pdf/.png տեսակի
                    result.Add(new ActionError(1465));
                }
            }

            if (order.Bond.AmountChargeDate > DateTime.Now.Date)
            {
                //Դրամական միջոցների մուտքագրման ամսաթիվը սխալ է լրացված
                result.Add(new ActionError(1482));
            }


            if (order.IsCashInTransit != 0 && order.IsCashInTransit != 1)
            {
                //Նշված գործողության մեջ առկա է սխալ։
                result.Add(new ActionError(1416));
            }

            return result;
        }

        internal static List<ActionError> ValidateDepositaryAccountOrder(DepositaryAccountOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            //if (DepositaryAccount.HasCustomerDepositaryAccount(order.CustomerNumber) == true)
            //{
            //    //Հաճախորդն արդեն ունի արժեթղթերի հաշիվ:
            //    result.Add(new ActionError(1483));
            //}

            if (DepositaryAccountOrder.ExistsNotConfirmedDepositaryAccountOrder(order.CustomerNumber, order.SubType))
            {
                //Տվյալ պարտատոմսի համար արդեն գոյություն ունի ուղարկված և դեռևս չհաստատված հաստատման հայտ:
                result.Add(new ActionError(1485));
            }
            return result;
        }

        /// <summary>
        /// Դասակարգված վարկի արտաբալանսից հանման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateLoanProductMakeOutBalanceOrder(LoanProductMakeOutBalanceOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.IsExistsNotConfirmedOrder())
                result.Add(new ActionError(1499)); //Տվյալ վարկի համար արդեն գոյություն ունի արտաբալանսից հանման չկատարված հայտ:

            if (order.IsQualityWrong())
                result.Add(new ActionError(1500)); //Տվյալ կարգավիճակով  վարկը հնարավոր չէ արտաբալանսից հանել:

            return result;
        }


        /// <summary>
        /// Վարկային գծի երկարաձգման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCreditLineProlongationOrder(CreditLineProlongationOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            string cardNumber = CreditLine.GetCreditLineCardNumber(order.ProductAppID);

            bool isCreditLine = CreditLineProlongationOrder.IsCreditLineProlongation(order.ProductAppID);


            Card card = Card.GetCardMainData(cardNumber);

            if (CreditLineProlongationOrder.IsSecontCreditLineProlongationApplication(order.ProductAppID, (ushort)order.Type, order.Id))
            {
                //Տվյալ վարկային գծի համար գոյոթյուն ունի երկարաձգման չհաստատված հայտ
                result.Add(new ActionError(1520));
            }


            if (order.user.filialCode == 22059 && card.FilialCode == 22000)
            {
                order.user.filialCode = (ushort)card.FilialCode;
            }

            if (card.FilialCode != order.user.filialCode)
            {
                // Այլ մասնաճյուղի քարտ
                result.Add(new ActionError(1515));
                return result;
            }


            if (isCreditLine == false)
            {
                // Տվյալ վարկային գծի համար գոյություն չունի երկարաձգման դիմում
                result.Add(new ActionError(1517));
                return result;

            }

            CreditLine creditLineOverdraft = CreditLine.GetCardOverdraft(cardNumber);

            if (Math.Abs(creditLineOverdraft.CurrentCapital) + Math.Abs(creditLineOverdraft.OutCapital) + Math.Abs(creditLineOverdraft.CurrentRateValue) != 0)
            {
                //Առկա է գերածախս:Վարկային գիծը երկարաձգելու համար գերածախը պետք է նախապես ամբողջությամբ մարվի   
                result.Add(new ActionError(1516));

            }



            return result;

        }


        /// <summary>
        /// Ստուգում է պարբերական փոխանցման փոփոխման տվյալների կոռեկտությունը
        /// </summary>
        /// <param name="periodicDataChangeOrder"></param>
        /// <returns></returns>
        public static List<ActionError> ValidatePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder periodicDataChangeOrder)
        {
            List<ActionError> result = new List<ActionError>();

            if (periodicDataChangeOrder.Periodicity == 0)
            {
                //Պարբերականությունը ընտրված չէ:
                result.Add(new ActionError(247));
            }
            if (periodicDataChangeOrder.ChargeType == 0 && periodicDataChangeOrder.Amount == 0)
            {
                // Գումարը նշված չէ:
                result.Add(new ActionError(419));
            }

            PeriodicTransfer periodic = PeriodicTransfer.GetPeriodicTransfer(periodicDataChangeOrder.ProductId);

            if (periodic.ReceiverBankCode >= 22000 && periodic.ReceiverBankCode < 22300)
            {
                string creditAccountCurrency = Account.GetAccountCurrency(periodic.CreditAccount);
                if (creditAccountCurrency != periodicDataChangeOrder.Currency && periodic.DebitAccount.Currency != periodicDataChangeOrder.Currency)
                {
                    //Արժույթը պետք է համապատասխանի դեբետ կամ կրեդիտ հաշվին:
                    result.Add(new ActionError(263));
                }
            }
            if (periodic.Type != 3)
            {
                if (periodicDataChangeOrder.ChargeType == 2 && periodic.DebitAccount.Currency != periodicDataChangeOrder.Currency)
                {
                    //Ամբողջ մնացորդի փոխանցման դեպքում ընտրված արժույքը չի կարող տարբերվել դեբետագրվող հաշվի արժույթից:
                    result.Add(new ActionError(268));
                }
            }
            if (periodicDataChangeOrder.CheckDaysCount == 0)
            {
                //Ստուգման օրերի քանակը նշված չէ:
                result.Add(new ActionError(248));
            }
            else if (periodicDataChangeOrder.CheckDaysCount < 0)
            {
                //Ստուգման օրերի քանակը պետք է լինի դրական թիվ:
                result.Add(new ActionError(260));
            }
            if (periodicDataChangeOrder.LastOperationDate != null && periodicDataChangeOrder.LastOperationDate.Value.Date < DateTime.Now.Date)
            {
                //«Վերջ» դաշտում ամսաթիվը սխալ է:
                result.Add(new ActionError(245));
            }
            else
            {
                if (periodicDataChangeOrder.LastOperationDate != null && periodicDataChangeOrder.FirstTransferDate.Date > periodicDataChangeOrder.LastOperationDate.Value.Date)
                {
                    //Պարբերական փոխանցման վերջին օրը պետք է մեծ լինի առաջին փոխանցման օրվանից։
                    result.Add(new ActionError(1508));
                }
                if (periodicDataChangeOrder.LastOperationDate != null && (periodicDataChangeOrder.LastOperationDate.Value - periodicDataChangeOrder.FirstTransferDate).TotalDays < periodicDataChangeOrder.CheckDaysCount)
                {
                    //Ընտրված ժամանակահատվածը (@var1 օր) չի կարող լինել պարբերական փոխանցման տևողությունից (@var2 օր) մեծ:
                    result.Add(new ActionError(265, new string[] { periodicDataChangeOrder.CheckDaysCount.ToString(), periodicDataChangeOrder.LastOperationDate.Value.AddDays(periodicDataChangeOrder.FirstTransferDate.Day * -1).Day.ToString() }));
                }
                if (periodicDataChangeOrder.LastOperationDate != null && (periodicDataChangeOrder.LastOperationDate.Value - periodicDataChangeOrder.FirstTransferDate).TotalDays < periodicDataChangeOrder.Periodicity)
                {
                    //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել պարբերական փոխանցման տևողությունը (@var2 օր):
                    result.Add(new ActionError(266, new string[] { periodicDataChangeOrder.Periodicity.ToString(), (periodicDataChangeOrder.LastOperationDate.Value.Day - periodicDataChangeOrder.FirstTransferDate.Day).ToString() }));
                }
                if (periodicDataChangeOrder.LastOperationDate != null && (periodicDataChangeOrder.LastOperationDate.Value - periodicDataChangeOrder.FirstTransferDate).TotalDays < periodicDataChangeOrder.CheckDaysCount)
                {
                    //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել ընտրված ժամանակահատվածը (@var2 օր):
                    result.Add(new ActionError(267, new string[] { periodicDataChangeOrder.CheckDaysCount.ToString(), (periodicDataChangeOrder.LastOperationDate.Value.Day - periodicDataChangeOrder.FirstTransferDate.Day).ToString() }));
                }
            }
            if (periodicDataChangeOrder.CheckDaysCount > periodicDataChangeOrder.Periodicity)
            {
                //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել ընտրված ժամանակահատվածը (@var2 օր):
                result.Add(new ActionError(267, new string[] { periodicDataChangeOrder.CheckDaysCount.ToString(), periodicDataChangeOrder.Periodicity.ToString() }));
            }
            if (periodicDataChangeOrder.MinAmountLevel < 0 || periodicDataChangeOrder.MinDebetAccountRest < 0 || periodicDataChangeOrder.MaxAmountLevel < 0)
            {
                //Մուտքագրված գումարը սխալ է
                result.Add(new ActionError(22));
            }
            else
            {
                if (periodicDataChangeOrder.MinAmountLevel > periodicDataChangeOrder.MaxAmountLevel && periodicDataChangeOrder.MaxAmountLevel != 0)
                {
                    //Փոխանցման նվազագույն գումարը պետք է լինի առավելագույն գումարից փոքր:
                    result.Add(new ActionError(264));
                }
                if (periodicDataChangeOrder.Amount != 0 && !Utility.IsCorrectAmount(periodicDataChangeOrder.Amount, periodicDataChangeOrder.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
                if (periodicDataChangeOrder.MinAmountLevel != 0 && !Utility.IsCorrectAmount(periodicDataChangeOrder.MinAmountLevel, periodicDataChangeOrder.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
                if (periodicDataChangeOrder.MaxAmountLevel != 0 && !Utility.IsCorrectAmount(periodicDataChangeOrder.MaxAmountLevel, periodicDataChangeOrder.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
                if (!Utility.IsCorrectAmount(periodicDataChangeOrder.MinDebetAccountRest, periodicDataChangeOrder.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
            }
            if (periodicDataChangeOrder.PeriodicType != 3)
            {
                if (string.IsNullOrEmpty(periodicDataChangeOrder.Description))
                {
                    //Վճարման նպատակը մուտքագրված չէ:
                    result.Add(new ActionError(23));
                }
                else
                {
                    if (Utility.IsExistForbiddenCharacter(periodicDataChangeOrder.Description))
                    {
                        //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(78));
                    }
                    if (periodicDataChangeOrder.PeriodicType == 2 && periodicDataChangeOrder.Description.Length > 115)
                    {
                        //«Վճարման նպատակ»  դաշտի արժեքը չպետք է գերազանցի 115
                        result.Add(new ActionError(268, new string[] { "115" }));
                    }
                    else if (periodicDataChangeOrder.Description.Length > 130)
                    {
                        //«Վճարման նպատակ»  դաշտի արժեքը չպետք է գերազանցի 130
                        result.Add(new ActionError(268, new string[] { "130" }));
                    }
                }
            }
            result.AddRange(Validation.ValidateCustomerDocument(periodicDataChangeOrder.CustomerNumber));

            if (DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) < periodicDataChangeOrder.FirstTransferDate.Day + periodicDataChangeOrder.CheckDaysCount)
            {
                //Առաջին փոխանցման օր + ստուգման օրերի քանակ չպետք է գերազանցի ամսվա օրերի քանակը: Պարբերականը կատարվելու է յուրաքանչյուր ամիս` սկսած @var1
                result.Add(new ActionError(350, new string[] { periodicDataChangeOrder.FirstTransferDate.Date.ToString() }));
            }
            return result;
        }

        internal static List<ActionError> ValidateLoanProductDataChangeOrder(LoanProductDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();


            if (LoanProductDataChangeOrder.ExistsLoanProductDataChange((ulong)order.ProductAppId) == true)
            {
                //Գոյություն ունի վաղաժամկետ մարման վճարի հայտ:
                result.Add(new ActionError(1521));
            }


            bool existsLoan = Loan.CheckLoanExists(order.CustomerNumber, (ulong)order.ProductAppId);
            if (!existsLoan)
            {
                //Վարկը գտնված չէ:
                result.Add(new ActionError(1522));
            }


            return result;
        }

        internal static List<ActionError> ValidateClassifiedLoanActionOrders(ClassifiedLoanActionOrders classifiedLoanActionOrders)
        {
            List<ActionError> result = new List<ActionError>();

            if (PreOrderDB.IsExistIncompletePreOrders(classifiedLoanActionOrders.PreOrderType))
            {
                //Նախորդ խմբաքանակում դեռևս կան չձևավորված հայտեր 
                result.Add(new ActionError(1532));
            }

            return result;
        }
        internal static List<ActionError> ValidateCreditHereAndNowActivationOrders(CreditHereAndNowActivationOrders classifiedLoanActionOrders)
        {
            List<ActionError> result = new List<ActionError>();

            if (PreOrderDB.IsExistIncompletePreOrders(classifiedLoanActionOrders.PreOrderType))
            {
                //Նախորդ խմբաքանակում դեռևս կան չձևավորված հայտեր 
                result.Add(new ActionError(1532));
            }

            return result;
        }

        /// <summary>
        /// 24/7 Ռեժիմի ակտիվացման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> Validate24_7_mode(OperDayMode operday)
        {
            List<ActionError> result = new List<ActionError>();
            bool x = Utility.validate24_7_mode(operday);

            if (x != true)
            {
                //Տվյալ կարգավիճակով հնարավոր չէ ակտիվացնել։
                result.Add(new ActionError(1533));
                return result;
            }
            return result;
        }

        /// <summary>
        /// Տվյալ տեսակի գործարքի համար 24/7 ռեժիմով կատարելու հնարավորության ստուգում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private static bool CheckFor24_7Mode(Order order)
        {
            return ValidationDB.CheckFor24_7Mode(order);
        }



        /// <summary>
        /// Գումարի ստուգում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCreditCommitmentForgivenes(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            List<ActionError> result = new List<ActionError>();
            result = CreditCommitmentForgivenessOrder.ValidateCreditCommitmentForgivenes(creditCommitmentForgiveness);

            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ փաստաթղթի համարը թույլատրելի է, թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="registrationDate"></param>
        /// <param name="orderId"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        public static bool ValidateDocumentNumber(ulong customerNumber, DateTime registrationDate, int orderId, string orderNumber)
        {
            return ValidationDB.ValidateDocumentNumber(customerNumber, registrationDate, orderId, orderNumber);
        }

        /// <summary>
        /// Քարտի հեռացման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardRemovalOrder(PlasticCardRemovalOrder order, User user)
        {
            List<ActionError> result = new List<ActionError>();
            order.Card.CardChangeType = (CardChangeType)Card.GetCardChangeType(order.Card.ProductId);

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (order.RemovalReason == 0)
            {
                //Քարտի հեռացման պատճառն ընտրված չէ
                result.Add(new ActionError(1637));
                // return result;
            }

            CardStatus cardStatus = new CardStatus();
            cardStatus = Card.GetCardStatus((ulong)order.Card.ProductId, order.CustomerNumber);

            if (cardStatus.Status != 3)
            {
                //Քարտը տրամադրված է
                result.Add(new ActionError(1641));
                //return result;
            }

            if (order.Card.CardChangeType == CardChangeType.New)
            {
                if (PlasticCardRemovalOrder.CheckForArcaFileGenerationProcess())
                {
                    //Քարտի պատվերի ֆայլն ուղարկված է ԱրՔա։ Հեռացումը հնարավոր չէ իրականացնել
                    result.Add(new ActionError(1640));
                    //return result;
                }

                if (PlasticCardRemovalOrder.PlasticCardSentToArca(order.Card.ProductId, order.Card.CardChangeType) == PlasticCardSentToArcaStatus.NoInfo)
                {
                    //ԱրՔա ուղարկված լինելու (չլինելու) վերաբերյալ տվյալներ հայտնաբերված չեն:
                    result.Add(new ActionError(1639));
                    //return result;
                }

                if (PlasticCardRemovalOrder.PlasticCardSentToArca(order.Card.ProductId, order.Card.CardChangeType) == PlasticCardSentToArcaStatus.SentToArca)
                {
                    //Քարտի պատվերի ֆայլն ուղարկված է ԱրՔա։ Հեռացումը հնարավոր չէ իրականացնել
                    result.Add(new ActionError(1640));
                    //return result;
                }
            }

            Card card = Card.GetCard((ulong)order.Card.ProductId, order.CustomerNumber);
            user.AdvancedOptions.TryGetValue("isCardDepartment", out string isCardDepartment);

            if (card != null)
            {
                order.Card = card; //do not remove

                if (isCardDepartment != "1" && order.Card.FilialCode != user.filialCode)
                {
                    //Այլ մասնաճյուղի քարտ փակել հնարավոր չէ
                    result.Add(new ActionError(1653));
                    //return result;
                }

                if (order.Card.OpenDate.Date != DateTime.Now.Date)
                {
                    //Հաշվի բացման օրը չի կարող տարբերվել գործառնական օրվանից։
                    result.Add(new ActionError(1638));
                    //return result;
                }

                if (order.Card.ClosingDate != null)
                {
                    //Ընտրված է փակված քարտ
                    result.Add(new ActionError(532));
                    //return result;
                }
                if (order.Card.CardChangeType == CardChangeType.New && order.Card.CreditLine != null)
                {
                    //Քարտը ունի վարկային գիծ
                    result.Add(new ActionError(521));
                    //return result;
                }

                if (order.Card.MainCardNumber == "")
                {
                    //Հաշիվը ներառված է պարբերական փոխանցման հանձնարարականում
                    result.AddRange(CardClosingOrder.CheckCardPeriodicTransfer(order.Card.CardAccount.AccountNumber));
                    //if (result.Count > 0)
                    //    return result;
                }

                //Քարտն ունի չձևակերպված գործարքներ
                result.AddRange(CardClosingOrder.CheckCardTransactions(order.Card.CardNumber, card.Type));
                //if (result.Count > 0)
                //    return result;


                List<Card> linkedCards = Card.GetLinkedCards(order.Card.CardNumber);
                if (linkedCards.Count > 0)
                {
                    string linkedCardNumbers = "";
                    foreach (Card linkedCard in linkedCards)
                    {
                        linkedCardNumbers += linkedCard.CardNumber + ",";
                    }
                    //Քարտը հիմնական է հանդիսանում @var1 քարտի(երի) համար
                    result.Add(new ActionError(526, new string[] { linkedCardNumbers }));
                    //return result;
                }
                if (Account.GetAccountBalance(order.Card.OverdraftAccount.AccountNumber) != 0)
                {
                    //Քարտի օվերդրաֆտի հաշիվը մնացորդ ունի
                    result.Add(new ActionError(527));
                    //return result;
                }

                if (order.Card.PositiveRate > 0)
                {
                    //Քարտին առկա է կուտակված ցպահանջ տոկոսագումար
                    result.Add(new ActionError(529));
                    //return result;
                }

                if (order.Card.Overdraft != null)
                {
                    if (card.Overdraft.CurrentRateValue != 0 || card.Overdraft.InpaiedRestOfRate != 0 ||
                        card.Overdraft.PenaltyRate != 0 || card.Overdraft.OutPenalty != 0 || card.Overdraft.OverdueCapital != 0 || card.Overdraft.OutCapital != 0)
                    {
                        //Քարտը ունի չմարված տոկոսագումար կամ տուգանք
                        result.Add(new ActionError(530));
                    }
                }
            }
            else
            {
                List<PlasticCard> plasticCards = PlasticCard.GetCustomerPlasticCards(order.CustomerNumber);
                PlasticCard plasticCard = plasticCards.Find(m => m.ProductId == (ulong)order.Card.ProductId);

                if (plasticCard == null)
                {
                    //Քարտը գտնված չէ
                    result.Add(new ActionError(534));
                    //return result;
                }
                else if (isCardDepartment != "1" && plasticCard.FilialCode != user.filialCode)
                {
                    //Այլ մասնաճյուղի քարտ փակել հնարավոր չէ
                    result.Add(new ActionError(1653));
                    //return result;
                }
            }
            if (order.Card.CardChangeType == CardChangeType.RenewWithNewType || order.Card.CardChangeType == CardChangeType.RenewWithSameType)
            {
                if (isCardDepartment != "1")
                {
                    if (PlasticCardRemovalOrder.PlasticCardSentToArca(order.Card.ProductId, order.Card.CardChangeType) == PlasticCardSentToArcaStatus.SentToArca)
                    {
                        //Քարտի պատվերի ֆայլն ուղարկված է ԱրՔա։ Հեռացումը հնարավոր չէ իրականացնել
                        result.Add(new ActionError(2052));
                        //return result;
                    }
                }
            }

            if (order.Card.CardChangeType == CardChangeType.CreditLineCardReplace || order.Card.CardChangeType == CardChangeType.RenewWithNewType || order.Card.CardChangeType == CardChangeType.RenewWithSameType)
            {
                Card newCard = Card.GetCard((ulong)order.Card.ProductId, order.CustomerNumber);
                //ulong oldProductId = Card.GetCardOldProductId(order.Card.ProductId, (short)order.Card.CardChangeType);
                ulong oldProductId = Card.GetCardOldProductId(order.Card.ProductId, order.Card.CardChangeType == CardChangeType.CreditLineCardReplace ? 3 : 1);

                Card oldCard = Card.GetCard(oldProductId, order.CustomerNumber);
                if (oldCard != null)
                {
                    if (oldCard.CreditLine != null)
                    {
                        List<LoanProductProlongation> list = LoanProduct.GetLoanProductProlongations((ulong)oldCard.CreditLine.ProductId);
                        LoanProductProlongation loanProlong = null;
                        if (list != null)
                        {
                            loanProlong = list.Find(m => m.ActivationDate == null);
                        }
                        if (loanProlong != null && newCard is null)
                        {
                            // Հնարավոր չէ պահպանել: Անհրաժեշտ է հեռացնել վարկային գծի երկարաձգումը:
                            result.Add(new ActionError(2049));
                        }
                    }

                }
                if (newCard != null)
                {
                    if ((order.Card.CardChangeType == CardChangeType.RenewWithNewType || order.Card.CardChangeType == CardChangeType.RenewWithSameType) && newCard != null)
                    {
                        // Հնարավոր չէ հեռացնել: Հաշիվը կցված է նոր քարտին:
                        result.Add(new ActionError(2051));
                    }
                    if (newCard.CreditLine != null)
                    {
                        List<LoanProductProlongation> list = LoanProduct.GetLoanProductProlongations((ulong)newCard.CreditLine.ProductId);
                        if (list != null)
                        {
                            if (order.Card.CardChangeType == CardChangeType.CreditLineCardReplace && newCard != null)
                            {
                                // Հնարավոր չէ հեռացնել: Վարկային գիծը երկարաձգված է և հաշիվը կցված է նոր քարտին:
                                result.Add(new ActionError(2050));
                            }
                        }
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Քարտային հաշվի հեռացման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardAccountRemovalOrder(CardAccountRemovalOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            CardStatus cardStatus = new CardStatus();
            cardStatus = Card.GetCardStatus((ulong)order.Card.ProductId, order.CustomerNumber);

            if (cardStatus.Status != 3)
            {
                //Քարտը տրամադրված է
                result.Add(new ActionError(1641));
                return result;
            }

            Card card = Card.GetCard((ulong)order.Card.ProductId, order.CustomerNumber);

            if (card != null)
            {
                order.Card = card; //do not remove

                if (order.Card.FilialCode != order.FilialCode)
                {
                    //Այլ մասնաճյուղի քարտ փակել հնարավոր չէ
                    result.Add(new ActionError(533));
                    return result;
                }

                if (order.Card.OpenDate.Date != DateTime.Now.Date)
                {//Հաշվի բացման օրը չի կարող տարբերվել գործառնական օրվանից։
                    result.Add(new ActionError(1638));
                    return result;
                }

                if (order.Card.ClosingDate != null)
                {
                    //Ընտրված է փակված քարտ
                    result.Add(new ActionError(532));
                    return result;
                }

                if (order.Card.CreditLine != null)
                {
                    //Քարտը ունի վարկային գիծ
                    result.Add(new ActionError(521));
                    return result;
                }

                List<Card> linkedCards = Card.GetLinkedCards(order.Card.CardNumber);
                if (linkedCards.Count > 0)
                {
                    string linkedCardNumbers = "";
                    foreach (Card linkedCard in linkedCards)
                    {
                        linkedCardNumbers += linkedCard.CardNumber + ",";
                    }
                    //Քարտը հիմնական է հանդիսանում @var1 քարտի(երի) համար
                    result.Add(new ActionError(526, new string[] { linkedCardNumbers }));
                    return result;
                }

                if (CardAccountRemovalOrderDB.CheckCardReissuement(card.ProductId))
                {
                    //Քարտը փոխարինված է կամ վերաթողարկված է այլ քարտատեսակով։
                    result.Add(new ActionError(1654));
                    return result;
                }
            }
            else
            { // Քարտը գտնված չէ
                result.Add(new ActionError(534));
                return result;
            }
            return result;
        }

        public static List<ActionError> ValidatePaymentToARCAOrder(PaymentToARCAOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (!(order.Source == SourceType.SSTerminal && Utility.IsCardAccount(order.DebitAccount.AccountNumber)))
            {
                //Տվյալ տեսակի գործարք նախատեսված չէ։
                result.Add(new ActionError(1645));
            }

            return result;
        }



        /// <summary>
        /// Քարտից քարտ փոխանցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static IEnumerable<ActionError> ValidateCardToCardOrder(CardToCardOrder order, TemplateType templateType = TemplateType.None)
        {
            List<ActionError> result = new List<ActionError>();

            if (!Card.CheckCardOwner(order.DebitCard.CardNumber, order.CustomerNumber))
            {
                //{Քարտային} հաշիվը չի պատկանում տվյալ հաճախորդին
                result.Add(new ActionError(901, new string[] { "Քարտային" }));
            }

            if (templateType == TemplateType.None)
            {
                result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

                result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));
            }




            if (order.DebitCard.CardNumber == order.CreditCardNumber)
            {
                //Ելքագրվող և մուտքագրվող քարտերը կրկնվում են։
                result.Add(new ActionError(1557));
            }



            if (templateType == TemplateType.None)
            {
                Card card = new Card();
                card.CardNumber = order.DebitCard.CardNumber;
                KeyValuePair<String, double> balance = card.GetArCaBalance(order.user.userID);

                if ((order.Amount + order.FeeAmount) > balance.Value)
                {
                    //Անբավարար միջոցներ:
                    result.Add(new ActionError(1470));
                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }
        /// <summary>
        /// Քարտից քարտ փոխանցման հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static IEnumerable<ActionError> ValidateAttachedCardToCardOrder(CardToCardOrder order, TemplateType templateType = TemplateType.None)
        {
            List<ActionError> result = new List<ActionError>();

            if (templateType == TemplateType.None)
            {
                result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

                result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));
            }

            if (order.DebitCardNumber == order.CreditCardNumber)
            {
                //Ելքագրվող և մուտքագրվող քարտերը կրկնվում են։
                result.Add(new ActionError(1557));
            }

            return result;
        }
        /// <summary>
        /// Քարտի լիմիտների փոփոխության հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardLimitChangeOrder(CardLimitChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (!Card.CheckCardOwner(order.Card.CardNumber, order.CustomerNumber))
            {
                //{Քարտային} հաշիվը չի պատկանում տվյալ հաճախորդին
                result.Add(new ActionError(901, new string[] { "Քարտային" }));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                result.Add(new ActionError(1628));
            }

            result.AddRange(ValidateDraftOrderQuality(order, order.CustomerNumber));

            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            Dictionary<string, string> cardLimits = CardLimitChangeOrderDB.GetCardLimits(order.Card.ProductId);

            if (order.Limits.Exists(limit => (limit.LimitValue < 0) || limit.LimitValue % 1 != 0 || (limit.LimitValue == 0 && !(limit.Limit == LimitType.MonthlyAggregateLimit && order.SubType == 2 && cardLimits.ContainsKey("7")))))
            {
                //Գործողությունը հնարավոր չէ իրականացնել, սահմանաչափի արժեքը կարող է լինել միայն դրական ամբողջ թիվ:
                result.Add(new ActionError(1553));
            }

            //***----"Մեկ օրվա ընթացքում կանխիկացման գործարքների գումար" - տեսակի լիմիտի ստուգումներ ----***
            if (order.Limits.Exists(limit => limit.Limit == LimitType.DailyCashingAmountLimit))
            {
                var dailyCashingAmountLimit = order.Limits.Find(limit => limit.Limit == LimitType.DailyCashingAmountLimit);

                if (order.Card.Currency == "AMD" && dailyCashingAmountLimit.LimitValue > 20000000) // 20,000,000 AMD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "USD" && dailyCashingAmountLimit.LimitValue > 45000) // 45,000 USD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "EUR" && dailyCashingAmountLimit.LimitValue > 40000) //40,000 EUR
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }
            }

            //***----"Մեկ օրվա ընթացքում կանխիկացման գործարքների քանակ" - տեսակի լիմիտի ստուգումներ ----***
            if (order.Limits.Exists(limit => limit.Limit == LimitType.DailyCashingQuantityLimit))
            {
                var dailyCashingQuantityLimit = order.Limits.Find(limit => limit.Limit == LimitType.DailyCashingQuantityLimit);
                if (dailyCashingQuantityLimit.LimitValue > 50)
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }
            }


            //***----"Մեկ օրվա ընթացքում վճարային գործարքների գումար" - տեսակի լիմիտի ստուգումներ ----***
            if (order.Limits.Exists(limit => limit.Limit == LimitType.DailyPaymentsAmountLimit))
            {
                var dailyPaymentsAmountLimit = order.Limits.Find(limit => limit.Limit == LimitType.DailyPaymentsAmountLimit);
                if (order.Card.Currency == "AMD" && dailyPaymentsAmountLimit.LimitValue > 40000000) // 40,000,000 AMD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "USD" && dailyPaymentsAmountLimit.LimitValue > 90000) // 90,000 USD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "EUR" && dailyPaymentsAmountLimit.LimitValue > 80000) // 80,000 EUR
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }
            }

            //***----"Կից քարտի գործարքների ամսական ընդհանուր գումար" - տեսակի լիմիտի ստուգումներ ----***
            if (order.Limits.Exists(limit => limit.Limit == LimitType.MonthlyAggregateLimit))
            {
                var aggregateLimit = order.Limits.Find(limit => limit.Limit == LimitType.MonthlyAggregateLimit);
                if (order.Card.Currency == "AMD" && aggregateLimit.LimitValue > 5000000) // 5,000,000 AMD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "USD" && aggregateLimit.LimitValue > 10000) // 10,000 USD
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }

                if (order.Card.Currency == "EUR" && aggregateLimit.LimitValue > 10000) // 10,000 EUR
                {
                    //Առավելագույն թույլատրելի արժեքը գերազանցված է
                    result.Add(new ActionError(1551));
                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }

        public static bool IsBase64String(string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }



        public static ActionResult ValidateAttachCard(string cardNumber, ulong customerNumber, string cardHolderName)
        {
            ActionResult actionResult = new ActionResult()
            {
                ResultCode = ResultCode.Normal
            };
            if (Card.IsOurCard(cardNumber))
            {
                List<Card> cards = Card.GetCards(customerNumber, ProductQualityFilter.Opened);
                if (cards.Exists(x => x.CardNumber == cardNumber))
                {
                    actionResult.Errors.Add(new ActionError(1735));//Հնարավոր չէ կցել Բանկում գործող Ձեր քարտը: Ձեր քարտով գործարքների իրականացումը հնարավոր է առանց կցման՝ Քարտեր բաժնից:
                }
                else
                {
                    actionResult.Errors.Add(new ActionError(1734)); //Քարտը հնարավոր չէ կցել: Քարտը պետք է թողարկված լինի Ձեր անունով:
                }
            }
            else
            {
                using (ArcaDataServiceClient proxy = new ArcaDataServiceClient())
                {
                    CardIdentification cardIdentification = new CardIdentification
                    {
                        CardNumber = cardNumber,
                        EmbossedName = cardHolderName
                    };
                    bool result = proxy.CheckCardEmbossingName(cardIdentification);
                    if (!result)
                    {
                        actionResult.Errors.Add(new ActionError(1736));//Քարտը գտնված չէ: Քարտը պետք է թողարկված լինի Ձեր անունով՝ ԱրՔա անդամ բանկի կողմից: 
                    }
                }
                if (actionResult.Errors.Count == 0)
                {
                    if (ValidationDB.IsExistAttachedCard(customerNumber, cardNumber))
                    {
                        actionResult.Errors.Add(new ActionError(1738)); //Տվյալ քարտն արդեն կցված է:
                    }
                    else if (ValidationDB.IsAttachedCardBusiness(cardNumber))
                    {
                        actionResult.Errors.Add(new ActionError(1737)); //Քարտը հնարավոր չէ կցել:Կցվող քարտը պետք է թողարկված լինի ֆիզիկական անձի համար:
                    }
                    else
                    {
                        CustomerMainData customerMainData = ACBAOperationService.GetCustomerMainData(customerNumber);
                        if (customerMainData != null)
                        {
                            if (!ValidateCardholderName(cardHolderName.Trim().ToLower(), customerMainData?.CustomerDescriptionEng.Trim().ToLower()))
                            {
                                actionResult.Errors.Add(new ActionError(1734)); //Քարտը հնարավոր չէ կցել: Քարտը պետք է թողարկված լինի Ձեր անունով:
                            }
                        }
                    }
                }
            }
            if (actionResult.Errors.Count > 0)
            {
                actionResult.ResultCode = ResultCode.ValidationError;
            }
            return actionResult;
        }

        /// <summary>
        /// Payout գործողության տվյալների ստուգումներ
        /// </summary>
        /// <param name="payOut"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidatePayOutOperation(PayOutInput payOut)
        {
            List<ActionError> result = new List<ActionError>();
            DateTime outputDate;

            result.AddRange(ValidateURN(payOut.URN));


            if (payOut.BeneficiaryLastName.Length > 200)
            {
                //Ստացողի ազգանունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1179));
            }

            if (payOut.BeneficiaryFirstName.Length > 200)
            {
                //Ստացողի անունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1180));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiarySexCode))
            {
                //Ստացողի սեռը մուտքագրված չէ։
                result.Add(new ActionError(1120));
            }
            else if (payOut.BeneficiarySexCode.Length != 1)
            {
                //Ստացողի սեռի կոդը պետք է լինի 1 նիշ։
                result.Add(new ActionError(1210));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryCountryCode))
            {
                //Ստացողի երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1121));
            }
            else if (payOut.BeneficiaryCountryCode.Length != 3)
            {
                //Ստացողի երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1164));
            }

            //Հանել S7 թեստի համար
            //if (String.IsNullOrEmpty(payOut.BeneficiaryAddressName))
            //{
            //    //Ստացողի հասցեն մուտքագրված չէ։
            //    result.Add(new ActionError(1122));
            //}
            else if (!String.IsNullOrEmpty(payOut.BeneficiaryAddressName) && payOut.BeneficiaryAddressName.Length > 200)
            {
                //Ստացողի հասցեն պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1211));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryDocumentTypeCode))
            {
                //Ստացողի փաստաթղթի տեսակը մուտքագրված չէ։
                result.Add(new ActionError(1123));
            }
            else if (payOut.BeneficiaryDocumentTypeCode.Length > 10)
            {
                //Ստացողի փաստաթղթի տեսակի կոդը պետք է լինի առավելագույնը 10 նիշ։
                result.Add(new ActionError(1212));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryOccupationName))
            {
                //Ստացողի զբաղվածությունը մուտքագրված չէ։
                result.Add(new ActionError(1124));
            }
            else if (payOut.BeneficiaryOccupationName.Length > 200)
            {
                //Ստացողի զբաղվածությունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1213));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryIssueCountryCode))
            {
                //Ստացողի փաստաթղթի թողարկման երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1125));
            }
            else if (payOut.BeneficiaryIssueCountryCode.Length > 3)
            {
                //Ստացողի փաստաթղթի թողարկման երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1214));
            }

            //if (String.IsNullOrEmpty(payOut.BeneficiaryIssueCityCode))
            //{
            //    //Ստացողի փաստաթղթի թողարկման քաղաքը մուտքագրված չէ։
            //    result.Add(new ActionError(1126));
            //}

            if (String.IsNullOrEmpty(payOut.BeneficiaryIssueIDNo))
            {
                //Ստացողի ID համարը մուտքագրված չէ։
                result.Add(new ActionError(1127));
            }
            else if (payOut.BeneficiaryIssueIDNo.Length > 50)
            {
                //Ստացողի ID համարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1215));
            }


            if (String.IsNullOrEmpty(payOut.BeneficiaryIssueDate))
            {
                //Ստացողի փաստաթղթի տրման ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1128));
            }
            else if (payOut.BeneficiaryIssueDate.Length != 8
                || !DateTime.TryParseExact(payOut.BeneficiaryIssueDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
                || outputDate > DateTime.Now)
            {
                //Ստացողի փաստաթղթի տրման ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(1216));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryExpirationDate))
            {
                //Ստացողի փաստաթղթի ավարտման ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1129));
            }
            else if (payOut.BeneficiaryExpirationDate.Length != 8
               || !DateTime.TryParseExact(payOut.BeneficiaryExpirationDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
               || outputDate < DateTime.Now)
            {
                //Ստացողի փաստաթղթի ավարտման ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(1217));
            }

            if (String.IsNullOrEmpty(payOut.BeneficiaryBirthDate))
            {
                //Ստացողի ծննդյան ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1130));
            }
            //Հանել S3 թեստի համար
            //else if (payOut.BeneficiaryBirthDate.Length != 8
            //        || !DateTime.TryParseExact(payOut.BeneficiaryBirthDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
            //        || outputDate > DateTime.Now)
            //{
            //    //Ստացողի ծննդյան ամսաթիվը սխալ է մուտքագրված:
            //    result.Add(new ActionError(1218));
            //}

            if (String.IsNullOrEmpty(payOut.BeneficiaryMobileNo))
            {
                //Ստացողի բջջային հեռախոսահամարը մուտքագրված չէ։
                result.Add(new ActionError(1131));
            }
            else if (payOut.BeneficiaryMobileNo.Length > 50)
            {
                //Ստացողի բջջային հեռախոսահամարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1178));
            }


            //**************************************************************************
            //Ոչ պարտադիր տվյալների ստուգումներ
            if (!String.IsNullOrEmpty(payOut.BeneficiaryMiddleName) && payOut.BeneficiaryMiddleName.Length > 200)
            {
                //Ստացողի հայրանունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1191));
            }

            if (!String.IsNullOrEmpty(payOut.BeneficiaryStateCode) && payOut.BeneficiaryStateCode.Length > 50)
            {
                //Ստացողի մարզի կոդը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1189));
            }

            if (!String.IsNullOrEmpty(payOut.BeneficiaryCityCode) && payOut.BeneficiaryCityCode.Length > 100)
            {
                //Ստացողի քաղաքի կոդը պետք է լինի առավելագույնը 100 նիշ։
                result.Add(new ActionError(1190));
            }

            if (!String.IsNullOrEmpty(payOut.BeneficiaryZipCode) && payOut.BeneficiaryZipCode.Length > 20)
            {
                //Ստացողի փոստային կոդը պետք է լինի առավելագույնը 20 նիշ։
                result.Add(new ActionError(1224));
            }

            if (!String.IsNullOrEmpty(payOut.BeneficiaryEMailName))
            {

                //Ստացողի էլեկտրոնային հասցեն 
                result.AddRange(ValidateEmail(payOut.BeneficiaryEMailName));

                if (payOut.BeneficiaryEMailName.Length > 50)
                {
                    //Ստացողի էլեկտրոնային հասցեն պետք է լինի առավելագույնը 50 նիշ։
                    result.Add(new ActionError(1226));
                }
            }

            if (!String.IsNullOrEmpty(payOut.BeneficiaryPhoneNo) && payOut.BeneficiaryPhoneNo.Length > 50)
            {
                //Ստացողի հեռախոսահամարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1227));
            }


            return result;
        }



        /// <summary>
        /// SendMoney գործողության տվյալների ստուգումներ
        /// </summary>
        /// <param name="sendMoney"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateSendMoneyOperation(SendMoneyInput sendMoney)
        {
            List<ActionError> result = new List<ActionError>();

            if (String.IsNullOrEmpty(sendMoney.MTOAgentCode))
            {
                //Դրամական Փոխանցման Օպերատորի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1151));
            }
            else if (sendMoney.MTOAgentCode.Length > 15)
            {
                //Դրամական Փոխանցման Օպերատորի կոդը պետք է լինի առավելագույնը 15 նիշ։
                result.Add(new ActionError(1162));
            }

            if (String.IsNullOrEmpty(sendMoney.PayoutDeliveryCode))
            {
                //Փոխանցման ստացման տեսակի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1132));
            }
            else if (sendMoney.PayoutDeliveryCode.Length != 2)
            {
                //Փոխանցման ստացման տեսակի կոդը պետք է լինի 2 նիշ։
                result.Add(new ActionError(1163));
            }
            else
            {
                if (sendMoney.PayoutDeliveryCode != Convert.ToInt32(ARUSInfo.DeliveryTypeCode.Cash).ToString())
                {
                    if (String.IsNullOrEmpty(sendMoney.BeneficiaryAgentCode))
                    {
                        //Ստացող Agent-ի կոդը մուտքագրված չէ։
                        result.Add(new ActionError(1185));
                    }
                    //else if (sendMoney.BeneficiaryAgentCode.Length > 15)
                    //{
                    //    //Ստացող Agent-ի կոդի երկարությունը պետք է լինի առավելագույնը 15 նիշ։
                    //    result.Add(new ActionError(1186));
                    //}

                    if (String.IsNullOrEmpty(sendMoney.AccountNo))
                    {
                        //Ստացողի հաշվեհամարը մուտքագրված չէ։
                        result.Add(new ActionError(1187));
                    }
                    else if (sendMoney.AccountNo.Length > 20)
                    {
                        //Ստացողի հաշվեհամարի երկարությունը պետք է լինի առավելագույնը 20 նիշ։
                        result.Add(new ActionError(1188));
                    }

                }

            }


            if (String.IsNullOrEmpty(sendMoney.BeneficiaryCountryCode))
            {
                //Ստացողի երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1121));
            }
            else if (sendMoney.BeneficiaryCountryCode.Length != 3)
            {
                //Ստացողի երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1164));
            }

            if (String.IsNullOrEmpty(sendMoney.SendCurrencyCode))
            {
                //Փոխանցման արժույթի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1133));
            }
            else if (sendMoney.SendCurrencyCode.Length != 3)
            {
                //Փոխանցման արժույթի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1165));
            }

            if (sendMoney.SendAmount <= 0)
            {
                //Փոխանցման գումարը մուտքագրված չէ։
                result.Add(new ActionError(1134));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderLastName))
            {
                //Ուղարկողի ազգանունը անգլերենով մուտքագրված չէ։
                result.Add(new ActionError(1137));
            }
            else if (sendMoney.SenderLastName.Length > 200)
            {
                //Ուղարկողի ազգանունը անգլերենով պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1168));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderFirstName))
            {
                //Ուղարկողի անունը անգլերենով մուտքագրված չէ։
                result.Add(new ActionError(1138));
            }
            else if (sendMoney.SenderFirstName.Length > 200)
            {
                //Ուղարկողի անունը անգլերենով պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1169));
            }

            if (sendMoney.NATSenderLastName.Length > 200)
            {
                //Ուղարկողի ազգանունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1166));
            }

            if (sendMoney.NATSenderFirstName.Length > 200)
            {
                //Ուղարկողի անունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1167));
            }


            if (String.IsNullOrEmpty(sendMoney.SenderCountryCode))
            {
                //Ուղարկողի երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1139));
            }
            else if (sendMoney.SenderCountryCode.Length != 3)
            {
                //Ուղարկողի երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1170));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderDocumentTypeCode))
            {
                //Ուղարկողի փաստաթղթի տեսակը մուտքագրված չէ։
                result.Add(new ActionError(1140));
            }
            else if (sendMoney.SenderDocumentTypeCode.Length > 10)
            {
                //Ուղարկողի փաստաթղթի տեսակի կոդը պետք է լինի առավելագույնը 10 նիշ։
                result.Add(new ActionError(1171));
            }

            DateTime outputDate;
            if (String.IsNullOrEmpty(sendMoney.SenderIssueDate))
            {
                //Ուղարկողի ID համարի տրման ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1141));
            }
            else if (sendMoney.SenderIssueDate.Length != 8
                || !DateTime.TryParseExact(sendMoney.SenderIssueDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
                || outputDate > DateTime.Now)
            {
                //Ուղարկողի ID համարի տրման ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(1172));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderExpirationDate))
            {
                //Ուղարկողի փաստաթղթի ավարտման ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1142));
            }
            else if (sendMoney.SenderExpirationDate.Length != 8
                || !DateTime.TryParseExact(sendMoney.SenderExpirationDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
                || outputDate < DateTime.Now)
            {
                //Ուղարկողի փաստաթղթի ավարտման ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(1173));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderIssueCountryCode))
            {
                //Ուղարկողի փաստաթղթի թողարկման երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1143));
            }
            else if (sendMoney.SenderIssueCountryCode.Length > 3)
            {
                //Ուղարկողի փաստաթղթի թողարկման երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1174));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderIssueIDNo))
            {
                //Ուղարկողի ID համարը մուտքագրված չէ։
                result.Add(new ActionError(1144));
            }
            else if (sendMoney.SenderIssueIDNo.Length > 50)
            {
                //Ուղարկողի ID համարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1175));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderBirthDate))
            {
                //Ուղարկողի ծննդյան ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1145));
            }
            //else if (sendMoney.SenderBirthDate.Length != 8 || !DateTime.TryParse(sendMoney.SenderExpirationDate, out outputDate) || Convert.ToDateTime(sendMoney.SenderBirthDate) < DateTime.Now)
            else if (sendMoney.SenderBirthDate.Length != 8
                     || !DateTime.TryParseExact(sendMoney.SenderBirthDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate)
                     || outputDate > DateTime.Now)
            {
                //Ուղարկողի ծննդյան ամսաթիվը սխալ է մուտքագրված:
                result.Add(new ActionError(1176));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderSexCode))
            {
                //Ուղարկողի սեռը մուտքագրված չէ։
                result.Add(new ActionError(1146));
            }
            else if (sendMoney.SenderSexCode.Length != 1)
            {
                //Ուղարկողի սեռի կոդը պետք է լինի 1 նիշ։
                result.Add(new ActionError(1177));
            }

            if (String.IsNullOrEmpty(sendMoney.SenderMobileNo))
            {
                //Ուղարկողի բջջային հեռախոսահամարը մուտքագրված չէ։
                result.Add(new ActionError(1147));
            }
            else if (sendMoney.SenderMobileNo.Length > 50)
            {
                //Ուղարկողի բջջային հեռախոսահամարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1178));
            }

            if (sendMoney.NATBeneficiaryLastName.Length > 200)
            {
                //Ստացողի ազգանունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1179));
            }

            if (sendMoney.NATBeneficiaryFirstName.Length > 200)
            {
                //Ստացողի անունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1180));
            }

            if (String.IsNullOrEmpty(sendMoney.BeneficiaryLastName))
            {
                //Ստացողի ազգանունը անգլերենով մուտքագրված չէ։
                result.Add(new ActionError(1148));
            }
            else if (sendMoney.BeneficiaryLastName.Length > 200)
            {
                //Ստացողի ազգանունը անգլերենով պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1181));
            }


            if (String.IsNullOrEmpty(sendMoney.BeneficiaryFirstName))
            {
                //Ստացողի անունը անգլերենով մուտքագրված չէ։
                result.Add(new ActionError(1149));
            }
            else if (sendMoney.BeneficiaryFirstName.Length > 200)
            {
                //Ստացողի անունը անգլերենով պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1182));
            }

            if (String.IsNullOrEmpty(sendMoney.BeneficiaryMobileNo))
            {
                //Ստացողի բջջային հեռախոսահամարը մուտքագրված չէ։
                result.Add(new ActionError(1131));
            }
            else if (sendMoney.BeneficiaryMobileNo.Length > 50)
            {
                //Ստացողի բջջային հեռախոսահամարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1183));
            }

            if (String.IsNullOrEmpty(sendMoney.PurposeRemittanceCode))
            {
                //Փոխանցման նպատակը մուտքագրված չէ։
                result.Add(new ActionError(1150));
            }
            else if (sendMoney.PurposeRemittanceCode.Length != 2)
            {
                //Փոխանցման նպատակի կոդը պետք է լինի 2 նիշ։
                result.Add(new ActionError(1184));
            }


            //*************************************************************************************************************************
            //Ոչ պարտադիր տվյալների ստուգումներ
            if (!String.IsNullOrEmpty(sendMoney.BeneficiaryStateCode) && sendMoney.BeneficiaryStateCode.Length > 50)
            {
                //Ստացողի մարզի կոդը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1189));
            }

            if (!String.IsNullOrEmpty(sendMoney.BeneficiaryCityCode) && sendMoney.BeneficiaryCityCode.Length > 100)
            {
                //Ստացողի քաղաքի կոդը պետք է լինի առավելագույնը 100 նիշ։
                result.Add(new ActionError(1190));
            }

            if (!String.IsNullOrEmpty(sendMoney.BeneficiaryMiddleName) && sendMoney.BeneficiaryMiddleName.Length > 200)
            {
                //Ստացողի հայրանունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1191));
            }

            if (!String.IsNullOrEmpty(sendMoney.BeneficiaryMiddleName) && sendMoney.BeneficiaryMiddleName.Length > 200)
            {
                //Ստացողի հայրանունը անգլերենով պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1192));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderStateCode) && sendMoney.SenderStateCode.Length > 50)
            {
                //Ուղարկողի մարզի կոդը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1193));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderCityCode) && sendMoney.SenderCityCode.Length > 100)
            {
                //Ուղարկողի քաղաքի կոդը պետք է լինի առավելագույնը 100 նիշ։
                result.Add(new ActionError(1194));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderAddressName) && sendMoney.SenderAddressName.Length > 200)
            {
                //Ուղարկողի հասցեն պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1195));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderZipCode) && sendMoney.SenderZipCode.Length > 20)
            {
                //Ուղարկողի փոստային կոդը պետք է լինի առավելագույնը 20 նիշ։
                result.Add(new ActionError(1196));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderOccupationName) && sendMoney.SenderOccupationName.Length > 200)
            {
                //Ուղարկողի զբաղվածությունը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1197));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderIssueCityCode) && sendMoney.SenderIssueCityCode.Length > 100)
            {
                //Ուղարկողի փաստաթղթի թողարկման քաղաքը պետք է լինի առավելագույնը 100 նիշ։
                result.Add(new ActionError(1198));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderBirthPlaceName) && sendMoney.SenderBirthPlaceName.Length > 200)
            {
                //Ուղարկողի ծննդավայրը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1199));
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderEMailName))
            {

                if (sendMoney.SenderEMailName.Length > 50)
                {
                    //Ուղարկողի էլեկտրոնային հասցեն պետք է լինի առավելագույնը 50 նիշ։
                    result.Add(new ActionError(1201));
                }
            }

            if (!String.IsNullOrEmpty(sendMoney.SenderPhoneNo) && sendMoney.SenderPhoneNo.Length > 50)
            {
                //Ուղարկողի հեռախոսահամարը պետք է լինի առավելագույնը 50 նիշ։
                result.Add(new ActionError(1202));
            }

            if (!String.IsNullOrEmpty(sendMoney.ControlQuestionName) && sendMoney.ControlQuestionName.Length > 200)
            {
                //Ստուգիչ հարցը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1203));
            }

            if (!String.IsNullOrEmpty(sendMoney.ControlAnswerName) && sendMoney.ControlAnswerName.Length > 200)
            {
                //Ստուգիչ հարցի պատասխանը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1204));
            }

            if (!String.IsNullOrEmpty(sendMoney.PromotionCode) && sendMoney.PromotionCode.Length > 20)
            {
                //Promo կոդը պետք է լինի առավելագույնը 20 նիշ։
                result.Add(new ActionError(1205));
            }

            if (!String.IsNullOrEmpty(sendMoney.DestinationText) && sendMoney.DestinationText.Length > 200)
            {
                //Նշումը պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1206));
            }


            return result;
        }

        /// <summary>
        /// Միջնորդավճարի հարցման տվյալների ստուգումներ
        /// </summary>
        /// <param name="feeInformation"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateRemittanceFeeInformation(RemittanceFeeInformationInput feeInformation)
        {
            List<ActionError> result = new List<ActionError>();

            if (String.IsNullOrEmpty(feeInformation.MTOAgentCode))
            {
                //Դրամական Փոխանցման Օպերատորի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1151));
            }
            else if (feeInformation.MTOAgentCode.Length > 15)
            {
                //Դրամական Փոխանցման Օպերատորի կոդը պետք է լինի առավելագույնը 15 նիշ։
                result.Add(new ActionError(1162));
            }

            if (String.IsNullOrEmpty(feeInformation.PayoutDeliveryCode))
            {
                //Փոխանցման ստացման տեսակի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1132));
            }
            else if (feeInformation.PayoutDeliveryCode.Length != 2)
            {
                //Փոխանցման ստացման տեսակի կոդը պետք է լինի 2 նիշ։
                result.Add(new ActionError(1163));
            }

            if (String.IsNullOrEmpty(feeInformation.CurrencyCode))
            {
                //Փոխանցման արժույթի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1133));
            }
            else if (feeInformation.CurrencyCode.Length != 3)
            {
                //Փոխանցման արժույթի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1165));
            }

            if (String.IsNullOrEmpty(feeInformation.SendingCountryCode))
            {
                //Ուղարկողի երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1139));
            }
            else if (feeInformation.SendingCountryCode.Length != 3)
            {
                //Ուղարկողի երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1170));
            }

            if (String.IsNullOrEmpty(feeInformation.BeneficiaryCountryCode))
            {
                //Ստացողի երկիրը մուտքագրված չէ։
                result.Add(new ActionError(1121));
            }
            else if (feeInformation.BeneficiaryCountryCode.Length != 3)
            {
                //Ստացողի երկրի կոդը պետք է լինի 3 նիշ։
                result.Add(new ActionError(1164));
            }

            if (feeInformation.PrincipalAmount == 0)
            {
                //Փոխանցման գումարը մուտքագրված չէ։
                result.Add(new ActionError(1134));
            }

            //*************************************************************************************************************************
            //Ոչ պարտադիր տվյալների ստուգումներ
            if (!String.IsNullOrEmpty(feeInformation.PromotionCode) && feeInformation.PromotionCode.Length > 20)
            {
                //Promo կոդը պետք է լինի առավելագույնը 20 նիշ։
                result.Add(new ActionError(1205));
            }

            if (!String.IsNullOrEmpty(feeInformation.PayOutAgentCode) && feeInformation.PayOutAgentCode.Length > 20)
            {
                //Փոխանցման ստացումն իրականացնող Agent-ի կոդը պետք է լինի առավելագույնը 20 նիշ։
                result.Add(new ActionError(1207));
            }

            return result;
        }

        /// <summary>
        /// Փոփոխման գործողության տվյալների ստուգումներ
        /// </summary>
        /// <param name="amendment"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateRemittanceAmendmentOrder(AmendmentInput amendment)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateURN(amendment.URN));

            if (String.IsNullOrEmpty(amendment.AmendmentReasonCode))
            {
                //Փոփոխման գործողության պատճառը մուտքագրված չէ։
                result.Add(new ActionError(1153));
            }

            //*************************************************************************************************************************
            //Ոչ պարտադիր տվյալների ստուգումներ

            if (!String.IsNullOrEmpty(amendment.BeforeBeneLastName) && amendment.BeforeBeneLastName.Length > 200)
            {
                //Ստացողի ազգանունը՝ նախքան փոփոխությունը, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1228));
            }

            if (!String.IsNullOrEmpty(amendment.BeforeBeneMiddleName) && amendment.BeforeBeneMiddleName.Length > 200)
            {
                //Ստացողի հայրանունը՝ նախքան փոփոխությունը, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1229));
            }

            if (!String.IsNullOrEmpty(amendment.BeforeBeneFirstName) && amendment.BeforeBeneFirstName.Length > 200)
            {
                //Ստացողի անունը՝ նախքան փոփոխությունը, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1230));
            }

            if (!String.IsNullOrEmpty(amendment.BeneficiaryLastName) && amendment.BeneficiaryLastName.Length > 200)
            {
                //Ստացողի ազգանունը՝ փոփոխությունից հետո, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1231));
            }

            if (!String.IsNullOrEmpty(amendment.BeneficiaryMiddleName) && amendment.BeneficiaryMiddleName.Length > 200)
            {
                //Ստացողի հայրանունը՝ փոփոխությունից հետո, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1232));
            }

            if (!String.IsNullOrEmpty(amendment.BeforeBeneFirstName) && amendment.BeforeBeneFirstName.Length > 200)
            {
                //Ստացողի անունը՝ փոփոխությունից հետո, պետք է լինի առավելագույնը 200 նիշ։
                result.Add(new ActionError(1233));
            }

            return result;
        }

        /// <summary>
        /// Փոխանցման ստուգիչ նիշի ստուգում
        /// </summary>
        /// <param name="URN"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateURN(string URN)
        {
            List<ActionError> result = new List<ActionError>();

            if (String.IsNullOrEmpty(URN))
            {
                //Փոխանցման ստուգիչ նիշը մուտքագրված չէ։
                result.Add(new ActionError(1117));
            }
            else if (URN.Length != 10)
            {
                //Փոխանցման ստուգիչ նիշը պետք է լինի 10 նիշ։
                result.Add(new ActionError(1208));
            }
            else if (!URN.Substring(0, 1).All(Char.IsLetter))
            {
                //Փոխանցման ստուգիչ նիշի առաջին նիշը պետք է լինի տառային։
                result.Add(new ActionError(1271));
            }
            else if (!URN.Substring(1, 9).All(Char.IsDigit))
            {
                //Փոխանցման ստուգիչ նիշի առաջինից տարբեր նիշերը պետք է լինեն թվային։
                result.Add(new ActionError(1209));
            }


            return result;
        }

        /// <summary>
        /// Չեղարկման/վերադարձման գործողության տվյալների ստուգումներ
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateRemittanceCancellationOrder(CancellationInput cancellation)
        {
            List<ActionError> result = new List<ActionError>();

            result.AddRange(ValidateURN(cancellation.URN));

            if (String.IsNullOrEmpty(cancellation.CancellationReversalCode))
            {
                //Չեղարկման/վերադարձման գործողության պատճառը մուտքագրված չէ։
                result.Add(new ActionError(1154));
            }

            //*************************************************************************************************************************
            //Ոչ պարտադիր տվյալների ստուգումներ
            if (!String.IsNullOrEmpty(cancellation.SendPayoutDivCode) && cancellation.SendPayoutDivCode.Length != 2)
            {
                //Գործողության տեսակը մուտքագրված չէ:
                result.Add(new ActionError(1155));
            }

            return result;
        }
        /// <summary>
        /// Ստուգում է հաճախորդի KYC փաստաթղթի ժամկետը
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static List<ActionError> ValidateKYCDocument(ulong customerNumber, SourceType sourceType = SourceType.Bank, bool isCheckAttachment = false)
        {
            List<ActionError> result = new List<ActionError>();
            List<CustomerDocument> customerDocuments; 

            customerDocuments = Customer.GetCustomerDocumentList(customerNumber);

            if (customerDocuments == null ||
                customerDocuments?.FindAll(m => m.documentType?.key == 39) == null ||
                customerDocuments?.FindAll(m => m.documentType?.key == 39).Count == 0 ||
                customerDocuments?.FindAll(m => m.documentType?.key == 39).OrderByDescending(o => o.givenDate).FirstOrDefault()?.givenDate < new DateTime(2019, 6, 5))
            {

                //Ավելացվել է տվյալ թասկի շրջանակում
                //https://www.wrike.com/open.htm?id=496217109


                if (sourceType == SourceType.AcbaOnline || sourceType == SourceType.MobileBanking)
                {
                    //Հեռահար եղանակով հաշվի բացումը հնարավոր չէ իրականացնել։ Խնդրում ենք մոտենալ Բանկի մոտակա մասնաճյուղ։
                    result.Add(new ActionError(2066, new string[] { customerNumber.ToString() }));
                }
                else
                {
                    //Գործընթացը շարունակելու համար խնդրում ենք թարմացնել հաճախորդի ` {0} KYC հարցաթերթիկը
                    result.Add(new ActionError(1733, new string[] { customerNumber.ToString() }));
                }


            }
            else if (isCheckAttachment && customerDocuments?.FindAll(m => m.documentType?.key == 39).Any(p => p.attachmentList.Count != 0) == false)
            {
              
                //Հեռահար եղանակով հաշվի բացումը հնարավոր չէ իրականացնել։ Խնդրում ենք մոտենալ Բանկի մոտակա մասնաճյուղ։
                result.Add(new ActionError(2066, new string[] { customerNumber.ToString() }));
            } 

            return result;
        }



        //internal static List<ActionError> ValidateR2AAccount(string r2aAccount, out Account currentAccount, out Card cardNumber)
        //{
        //    currentAccount = null;
        //    cardNumber = null;

        //    List<ActionError> result = new List<ActionError>();

        //    if (string.IsNullOrEmpty(r2aAccount) || r2aAccount == "0")
        //    {
        //        /// ---------------------- ՍՏԱԿ համակարգով փոխանցված գումարը ստացողի հաշիվը մուտքագրված չէ:
        //        result.Add(new ActionError(1746));
        //    }

        //    //if (Convert.ToUInt16(r2aAccount.Substring(0, 5)) >= 22000 && Convert.ToUInt16(r2aAccount.Substring(0, 5)) <= 22300)
        //    if (Account.IsOurAccount(r2aAccount))                
        //    {
        //        currentAccount = Account.GetAccount(r2aAccount);

        //        //if (currentAccount == null)
        //        //    {
        //        //    /// ---------------------- 
        //        //    result.Add(new ActionError(1111111111111111111));
        //        //    }
        //    }
        //    else if (Card.IsOurCard(r2aAccount))
        //    {
        //        cardNumber = Card.GetCard(r2aAccount);

        //        //if (cardAccount == null)
        //        //{
        //        //    /// ---------------------- 
        //        //    result.Add(new ActionError(111111111111111));
        //        //}
        //    }
        //    else
        //    {
        //        /// ՍՏԱԿ համակարգով փոխանցված գումարը ստացողի հաշիվը ԱԿԲԱ բանկի հաշիվ/քարտի համար չէ։
        //        result.Add(new ActionError(1747));
        //    }

        //    return result;
        //}


        public static List<ActionError> CheckRussianLetters(string Receiver, string ReceiverBank, string Sender, string SenderAddress)
        {
            List<ActionError> result = new List<ActionError>();


            if (!String.IsNullOrEmpty(Receiver))
            {
                if (!(Regex.IsMatch(Receiver, @"^[а-яА-Я0-9\s\p{P}]*$")))
                {
                    //<Ստացող> դաշտն անհրաժեշտ է լրացնել ռուսերեն:
                    result.Add(new ActionError(1775));
                }
            }

            if (!String.IsNullOrEmpty(ReceiverBank))
            {
                if (!(Regex.IsMatch(ReceiverBank, @"^[а-яА-Я0-9\s\p{P}]*$")))
                {
                    //<Ստացող բանկի անվանում> դաշտն անհրաժեշտ է լրացնել ռուսերեն:
                    result.Add(new ActionError(1776));
                }
            }


            if (!String.IsNullOrEmpty(Sender))
            {
                if (!(Regex.IsMatch(Sender, @"^[а-яА-Я0-9\s\p{P}]*$")))
                {
                    //<Ուղարկողի անուն/անվանում> դաշտն անհրաժեշտ է լրացնել ռուսերեն:
                    result.Add(new ActionError(1778));
                }
            }

            if (!String.IsNullOrEmpty(SenderAddress))
            {
                if (!(Regex.IsMatch(SenderAddress, @"^[а-яА-Я0-9\s\p{P}]*$")))
                {
                    //<Ուղարկողի հասցե> դաշտն անհրաժեշտ է լրացնել ռուսերեն:
                    result.Add(new ActionError(1779));
                }
            }

            return result;
        }
        /// <summary>
        /// Վարկի ամսաթվի փոփոխման  հայտի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateLoanDelayOrder(LoanDelayOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            Loan loan = new Loan();

            loan = Loan.GetLoan(Convert.ToUInt64(order.ProductAppId), order.CustomerNumber);

            if (loan.LoanType == 7 || loan.LoanType == 13 || loan.LoanType == 15 && loan.Fond == 44)
            {
                result.Add(new ActionError(1740));
            }

            if (loan.Fond == 22 || loan.Fond == 54 || loan?.SubsidiaInterestRate > 0 && loan.NextRepayment.RepaymentDate == loan.EndDate)
            {
                result.Add(new ActionError(1740));
            }

            if (loan.Fond == 55)
            {
                result.Add(new ActionError(1740));
            }

            return result;
        }
        /// <summary>
        /// Հետաձգված հայտի չեղարկման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateLoanCancelDelayOrder(CancelDelayOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.DelayReason == "1")
            {
                if (!LoanDelayOrderDB.CheckValidLoanDelayCancelOrder(order.ProductAppId))
                    result.Add(new ActionError(1743));
            }

            return result;
        }
        public static List<ActionError> CheckSymbolsExistance(string SenderAddress, string SenderTown, string SenderOtherBankAccount,
                    string IntermediaryBankSwift, string ReceiverBankAddInf, string DescriptionForPayment, string Receiver,
                    string AccountNumber, string ReceiverAddInf, string ReceiverBankSwift, int ReceiverBankCode)
        {
            List<ActionError> result = new List<ActionError>();
            var symbols = new string[] { "~", "!", "@", "«", "»", "#", "$", ";", "%", "^", "&", "*", "=", "{", "}", "[", "]", "_", "<", ">", "№", "\\", "±", "“", "”", "|", ":", "°", "'", "\"" };
            List<string> termSymbols = new List<string>();



            foreach (var symbol in symbols)
            {
                if (!String.IsNullOrEmpty(SenderAddress))
                {
                    if (SenderAddress.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ուղարկողի հասցե> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1789, termSymbols.ToArray()));

                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(SenderTown))
                {
                    if (SenderTown.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ուղարկողի քաղաք> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1790, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(SenderOtherBankAccount))
                {
                    if (SenderOtherBankAccount.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Այլ բանկերում ուղարկողի հաշվեհամարներ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1791, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(IntermediaryBankSwift))
                {
                    if (IntermediaryBankSwift.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Միջնորդ բանկի swift կոդ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1792, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(ReceiverBankAddInf))
                {
                    if (ReceiverBankAddInf.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացող բանկի լրացուցիչ տվյալներ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1793, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(DescriptionForPayment))
                {
                    if (DescriptionForPayment.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<ՎՃարման մանրամասներ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1794, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(Receiver))
                {
                    if (Receiver.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացող> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1795, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(AccountNumber))
                {
                    if (AccountNumber.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացողի հաշվեհամար> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1796, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(ReceiverAddInf))
                {
                    if (ReceiverAddInf.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացողի լրացուցիչ տվյալներ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1797, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(ReceiverBankSwift))
                {
                    if (ReceiverBankSwift.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացող բանկի SWIFT կոդ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1798, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(ReceiverBankCode.ToString()))
                {
                    if (Convert.ToString(ReceiverBankCode).Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացող բանկի կոդ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1799, termSymbols.ToArray()));
                        termSymbols.Clear();
                    }
                }

            }

            return result;

        }

        public static List<ActionError> CheckSymbolsExistanceForRATransfer(string CreditorDescription, string CreditorDocumentNumber, string Receiver, string Description)
        {
            List<ActionError> result = new List<ActionError>();
            var symbols = new string[] { "~", "!", "@", "«", "»", "#", "$", ";", "%", "&", "*", "=", "{", "}", "[", "]", "_", "<", ">", "№", "\\", "±", "“", "”", "'", "՛", "|", "°", "\"" };
            List<string> termSymbols = new List<string>();



            foreach (var symbol in symbols)
            {
                if (!String.IsNullOrEmpty(CreditorDescription))
                {
                    if (CreditorDescription.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Այլ անձի (պարտատիրոջ) անուն ազգանուն / անվանում> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1800, termSymbols.ToArray()));

                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(CreditorDocumentNumber))
                {
                    if (CreditorDocumentNumber.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Այլ անձի(պարտատիրոջ) ՀՎՀՀ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1801, termSymbols.ToArray()));

                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(Receiver))
                {
                    if (Receiver.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Ստացող> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1795, termSymbols.ToArray()));

                        termSymbols.Clear();
                    }
                }

                if (!String.IsNullOrEmpty(Description))
                {
                    if (Description.Contains(symbol))
                    {
                        termSymbols.Add(symbol);
                        //<Վճարման նպատակ> դաշտի մաջ կա անթույլատրելի նշան՝ (@):
                        result.Add(new ActionError(1802, termSymbols.ToArray()));

                        termSymbols.Clear();
                    }
                }


            }

            return result;
        }
        internal static List<ActionError> ValidateCardToOtherCardsOrder(CardToOtherCardsOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(order.SenderCardNumber))
            {
                //Ընտրեք ելքագրվող քարտը
                result.Add(new ActionError(1726));
            }

            if (string.IsNullOrEmpty(order.ReceiverCardNumber))
            {
                //Մուտքագրեք ստացողի քարտի համարը
                result.Add(new ActionError(1727));
            }

            if (string.IsNullOrEmpty(order.ReceiverName))
            {
                //Մուտքագրեք ստացողի անուն ազգանունը
                result.Add(new ActionError(1728));
            }

            if (result.Count > 0)
            {
                return result;
            }

            var checkLimit = order.CheckLimits();
            if (checkLimit.Count > 0)
            {
                //Սահմանաչափերի ստուգումներ
                result.AddRange(checkLimit);
            }

            if (order.IsNotAllowedCardType())
            {
                //Փոխանցումը հնարավոր չէ իրականացնել։Տվյալ տեսակի քարտից արգելվում է
                result.Add(new ActionError(1730));
            }
            if (order.IsBinUnderSanctions())
            {
                //Հնարավոր չէ կատարել փոխանցում տվյալ երկրի կողմից թողարկված քարտերին:
                result.Add(new ActionError(1744));
            }

            List<string> notAllowedBIN = new List<string>
            {
                "22",
                "34",
                "37"
            };

            if (notAllowedBIN.Contains(order.ReceiverCardNumber.Substring(0, 2)))
            {
                //Հնարավոր չէ կատարել փոխանցում տվյալ վճարային համակարգի քարտերին
                result.Add(new ActionError(1909));
            }
            if (Card.GetArmenianCardsBIN().Contains(order.ReceiverCardNumber.Substring(0, 6)))
            {
                //Նշված տեսակի փոխանցումը կատարելու համար անհրաժեշտ է ընտրել "Իմ քարտերի միջև" կամ "ՀՀ բանկերի քարտերին" տեսակը:
                result.Add(new ActionError(1908));
            }

            return result;
        }

        internal static List<ActionError> ValidateChangeBranchOrder(ChangeBranchOrder order)
        {
            List<ActionError> errors = new List<ActionError>();

            if (order.Card == null) //Եթե քարտ գոյություն չունի(գտնված չէ) հայտը անվավեր է
            {
                errors.Add(new ActionError(672));
                return errors;
            }

            if (order.Card.IsSupplementary) //Եթե կից քարտ է հայտը անվավեր է
            {
                errors.Add(new ActionError(673));
                return errors;
            }



            Card card = Card.GetCardMainData((ulong)order.Card.ProductId, order.CustomerNumber);

            if (card == null) //Նշված քարտը հաճախորդին չի պատկանում
            {
                errors.Add(new ActionError(694, new string[] { order.CustomerNumber.ToString(), order.Card.CardNumber }));
                return errors;
            }



            return errors;
        }

        internal static IEnumerable<ActionError> ValidatePensionPaymentOrder(PensionPaymentOrder order)
        {
            List<ActionError> errors = new List<ActionError>();

            PhysicalCustomer customer = (PhysicalCustomer)ACBAOperationService.GetCustomer(order.CustomerNumber);

            OPPerson opPerson = new OPPerson();
            opPerson.CustomerNumber = order.CustomerNumber;
            opPerson.PersonName = Utility.ConvertAnsiToUnicode(customer.person.fullName.firstName);
            opPerson.PersonLastName = Utility.ConvertAnsiToUnicode(customer.person.fullName.lastName);

            if (string.IsNullOrEmpty(order.CreditAccount))
                errors.Add(new ActionError(1879));

            if (order.FirstName != Utility.ConvertAnsiToUnicode(customer.person.fullName.firstName)
                || order.LastName != Utility.ConvertAnsiToUnicode(customer.person.fullName.lastName)
                || (!string.IsNullOrEmpty(order.FatherName) && order.FatherName != Utility.ConvertAnsiToUnicode(customer.person.fullName.MiddleName)))
                errors.Add(new ActionError(1880));

            return errors;
        }
        public static bool IsCustomerConnectedToOurBank(ulong customerNumber)
        {
            return ValidationDB.IsCustomerConnectedToOurBank(customerNumber);
        }

        public static bool CheckCustomerPhoneNumber(ulong customerNumber) => Info.GetCustomerAllPhones(customerNumber).Rows.Count > 0 ? false : true;

        /// <summary>
        /// Գործառնական օրվա կարգավիճակը բանկում
        /// </summary>
        /// <returns></returns>
        public static bool BankOpDayIsClosed()
        {
            return ValidationDB.BankOpDayIsClosed();
        }

        internal static SvipErrorResponse ValidateGetCardlessCashoutOrderWithVerification(CardlessCashoutOrder cardlessCashoutOrder)
        {
            SvipErrorResponse response = new SvipErrorResponse();

            if (cardlessCashoutOrder.Id == 0)
            {
                response = new SvipErrorResponse { ErrorCode = "14", ErrorDescription = "Incorrect Order_Id" };
            }
            else if (cardlessCashoutOrder.Quality == OrderQuality.Completed)
            {
                response = new SvipErrorResponse { ErrorCode = "AA", ErrorDescription = "Order_Id is already used" };
            }
            else if (cardlessCashoutOrder?.OTPGenerationDate.Value.AddHours(cardlessCashoutOrder.ValidHours) < DateTime.Now)
            {
                response = new SvipErrorResponse { ErrorCode = "95", ErrorDescription = "Order_Id is expired" };
            }
            return response;
        }

        ///<summary>
        ///Հաշվի հեռացման ստուգումների իրականացում 
        /// </summary>
        internal static List<ActionError> ValidateAccountRemovingOrder(AccountRemovingOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.RemovingAccount != null)
            {

                Account account = Account.GetAccount(order.RemovingAccount.AccountNumber);

                if (AccountClosingOrder.IsSecondClosing(order.CustomerNumber, account.AccountNumber, order.Source) == true && order.Id == 0)
                {
                    //Տվյալ հաշվի համար գոյություն ունի հեռացման չհաստատված հայտ
                    result.Add(new ActionError(505, new string[] { account.AccountNumber }));
                }

                else if (account.HaveActiveProduct())
                {
                    //Հաշիվը ներառված է գործող պրոդուկների հաշիվների ցանկում
                    result.Add(new ActionError(507, new string[] { account.AccountNumber }));
                }
                else if (account.HaveActiveDepositForCurrentAccount())
                {
                    //Գոյություն ունի գործող ավանդ, ընթացիկ հաշիվը չի թույլատրվում հեռացնել
                    result.Add(new ActionError(508, new string[] { account.AccountNumber }));
                }
                else if (account.HaveActiveLoanForCurrentAccount())
                {
                    //Գոյություն ունի գործող վարկ, ընթացիկ հաշիվը չի թույլատրվում հեռացնել
                    result.Add(new ActionError(509, new string[] { account.AccountNumber }));
                }
                else if (account.HaveActiveCreditLineForCurrentAccount())
                {
                    //Գոյություն ունի գործող վարկային գիծ, ընթացիկ հաշիվը չի թույլատրվում հեռացնել
                    result.Add(new ActionError(510, new string[] { account.AccountNumber }));
                }
                else if (account.HaveActiveOperationByPeriodForCurrentAccount())
                {
                    //Հաշիվը ներառված է պարբերական փոխանցման հանձնարարականում
                    result.Add(new ActionError(512, new string[] { account.AccountNumber }));
                }
                else if (Account.GetAcccountAvailableBalance(account.AccountNumber) != 0)
                {
                    result.Add(new ActionError(515, new string[] { account.AccountNumber }));
                }
                else if (Account.GetAccountBalance(account.AccountNumber) != 0)
                {
                    if (order.OnlySaveAndApprove)
                    {
                        if (Account.GetAccountBalance(account.AccountNumber) - Order.GetSentNotConfirmedWithdrawalAmount(account.AccountNumber) != 0)
                        {
                            //Հաշիվը մնացորդ ունի
                            result.Add(new ActionError(515, new string[] { account.AccountNumber }));
                        }
                    }
                    else
                    {
                        //Հաշիվը մնացորդ ունի
                        result.Add(new ActionError(515, new string[] { account.AccountNumber }));
                    }
                }

                else if (account.HaveTaxInspectorateApproval())
                {
                    //Հաշվի համար գոյություն ունի հարկային տեսչության կողմից հաստատման ենթակա հայտ
                    result.Add(new ActionError(517, new string[] { account.AccountNumber }));
                }
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;
        }



        /// <summary>
        /// Քարտի վերաբացման հայտի փաստաթղթի ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateCardReOpenOrder(CardReOpenOrder order, ACBAServiceReference.User user)
        {
            List<ActionError> result = new List<ActionError>();

            short customerType = ACBAOperationService.GetCustomerType(order.CustomerNumber);

            if (Customer.IsCustomerUpdateExpired(order.CustomerNumber))
            {
                //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                result.Add(new ActionError(496));
            }

            //if (customerType == (short)CustomerTypes.physical)
            //{
            //    List<CustomerDocument> customerDocuments = ACBAOperationService.GetCustomerDocumentList(order.CustomerNumber);
            //    //Հաճախորդի ստորագրության նմուշի ստուգում:
            //    //result.AddRange(ValidateCustomerSignature(order.CustomerNumber));

            //    if (!ValidateCustomerPSN(customerDocuments))
            //    {
            //        //Հաճախորդի փաստաթղթերում <հանրային ծառայությունների համարանիշ> կամ <հանրային ծառայությունների համարանիշից հրաժարման տեղեկանք> տեսակով փաստաթուղթը բացակայում է
            //        result.Add(new ActionError(585));
            //    }

            //}



            if (order.Currency != "AMD" && !CardReOpenOrderDB.IsOpenAMDCurrentAccout(order.CustomerNumber))
            {
                //Բացակայում է բաց կարգավիճակով ընթացիկ դրամային հաշիվը։
                result.Add(new ActionError(1958));
            }

            if (CardReOpenOrderDB.IsCardOpen(order.CardNumber))
            {
                //Նշված քարտի համարով առկա է բաց քարտ:
                result.Add(new ActionError(1959));
            }

            if ((order.Filial != order.FilialCode) && !((order.FilialCode == 22000 && order.Filial == 22059) || (order.FilialCode == 22059 && order.Filial == 22000)))
            {
                //Այլ մասնաճյուղի քարտ վերաբացել հնարավոր չէ:
                result.Add(new ActionError(1960));
            }

            if ((order.MainCardNumber == null && !CardReOpenOrderDB.IsExistsOverdraftAppId(order.CardNumber)) || (order.MainCardNumber != null && !CardReOpenOrderDB.IsExistsOverdraftAppId(order.MainCardNumber) && !CardReOpenOrderDB.IsCardOpen(order.MainCardNumber)))
            {
                //Տվյալ քարտը հնարավոր չէ վերաբացել։                
                result.Add(new ActionError(1961));
            }

            ushort typeId = CardReOpenOrderDB.GetTypeOfCardChanges(order.ProductID);
            if (typeId > 0)
            {
                //Տվյալ քարտը  {0} է '
                result.Add(new ActionError(1963, new string[] { typeId == 3 ? "փոխարինված " : typeId == 1 ? "վերաթողարկված " : "տեղափոխված " }));
            }


            return result;
        }

        ///<summary>
        ///Երրորդ անձի իրավունքի փոխանցման ստուգումների իրականացում 
        /// </summary>
        internal static List<ActionError> ValidateThirdPersonAccountRightsTransfer(ThirdPersonAccountRightsTransferOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            if (order.JointAccount != null)
            {

                Account account = Account.GetAccount(order.JointAccount.AccountNumber);

                if (ThirdPersonAccountRightsTransferOrder.IsSecondClosing(order.CustomerNumber, account.AccountNumber, order.Source) == true && order.Id == 0)
                {
                    //Տվյալ հաշվի համար գոյություն ունի հեռացման չհաստատված հայտ
                    result.Add(new ActionError(1988, new string[] { account.AccountNumber }));
                }
                else if (AccountDB.CheckCustomerFreeFunds(account.AccountNumber) == 0)
                {
                    //Հաճախորդի ազատ միջոցները բավարար չեն
                    result.Add(new ActionError(1989, new string[] { order.CustomerNumber.ToString() }));
                }
                else if (ThirdPersonAccountRightsTransferOrder.CheckRightsWereTransferred(account.AccountNumber))
                {
                    //Իրավունքն արդեն փոխանցված է III անձին
                    result.Add(new ActionError(1990));
                }
                else if (ThirdPersonAccountRightsTransferOrder.CheckAccountHasArrest(order.CustomerNumber))
                {
                    //Հաճախորդը ունի հաշիվներ արգելանքի տակ
                    result.Add(new ActionError(1991, new string[] { order.ThirdPersonCustomerNumber.ToString() }));
                }
                else if (!ThirdPersonAccountRightsTransferOrder.CheckThirdPersonIsCustomer(order.ThirdPersonCustomerNumber))
                {
                    //Երրորդ անձին նախ պետք է դարձնել հաճախորդ կարգավիճակ նոր իրավունքը փոխանցել իրեն
                    result.Add(new ActionError(1992, new string[] { order.ThirdPersonCustomerNumber.ToString() }));
                }

            }

            return result;
        }

        internal static List<ActionError> ValidateMRDataChangeOrder(MRDataChangeOrder order)
        {
            List<ActionError> result = new List<ActionError>();

            if (order.DataChangeCard != null)
            {
                if (MRDataChangeOrderDB.GetCardMRStatus(order.MRId) != MRStatus.NORM)
                {
                    //Հնարավոր չէ կատարել տվյալ կարգավիճակով MR ծառայության համար
                    result.Add(new ActionError(1993));
                }
            }

            return result;
        }

        /// <summary>
        /// Բաժնետոմսի թողարկման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateStockIssue(BondIssue bondIssue)
        {
            List<ActionError> result = new List<ActionError>();

            Action action = bondIssue.ID == 0 ? Action.Add : Action.Update;

            if (bondIssue.IssuerType == BondIssuerType.ACBA)
            {
                BondIssueFilter bondIssueFilter = new BondIssueFilter();
                bondIssueFilter.ISIN = bondIssue.ISIN;
                bondIssueFilter.Quality = BondIssueQuality.Approved;

                List<BondIssue> bondIssues = BondIssueFilter.SearchBondIssues(bondIssueFilter);

                if (action == Action.Add && BondIssueDB.GetCheckISINandSeria(bondIssue.ISIN, bondIssue.IssueSeria.Value))
                {
                    //Տվյալ ԱՄՏԾ-ով և թողարկամն սերիաով բաժնետոմսի թողարկում արդեն գոյություն ունի։
                    result.Add(new ActionError(2020));
                }

                if (bondIssue.MinSaleQuantity > bondIssue.MaxSaleQuantity)
                {
                    //Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող բաժնետոմսերի նվազագույն ձեռք բերման քանակը պետք է չգերազանցի առավելագույն քանակը։
                    result.Add(new ActionError(2021));
                }

                if (bondIssue.TotalCount * bondIssue.NominalPrice != bondIssue.TotalVolume)
                {
                    //Թողարկման ընդհանուր անվանական արժեքը հավասար չէ մեկ բաժնետոմսի անվանական արժեքի և տեղաբաշխման ենթակա քանակի արտադրյալին
                    result.Add(new ActionError(2033));
                }
                if (bondIssue.MaxSaleQuantity > bondIssue.TotalCount)
                {
                    //Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող բաժնետոմսերի առավելագույն ձեռք բերման քանակը մեծ է բաժնետոմսերի ընդհանուր քանակից
                    result.Add(new ActionError(2022));
                }

                if (bondIssue.TotalVolume % bondIssue.NominalPrice != 0)
                {
                    //Թողարկման ընդհանուր անվանական արժեքը սխալ է մուտքագրված։
                    result.Add(new ActionError(2023));
                }

                if (bondIssue.ReplacementEndDate < bondIssue.ReplacementDate)
                {
                    //Տեղաբաշխման նախատեսվող ավարտը փոքր է տեղաբաշխման սկզբից։
                    result.Add(new ActionError(2024));
                }

                if (bondIssue.IssueDate < bondIssue.RegistrationDate)
                {
                    //Թողարկման ամսաթիվը փոքր է գրանցման ամսաթվից։
                    result.Add(new ActionError(2025));
                }

                if (bondIssue.ReplacementDate < bondIssue.RegistrationDate)
                {
                    //Տեղաբաշխման սկիզբը փոքր է գրանցման ամսաթվից։
                    result.Add(new ActionError(2026));
                }

                if (bondIssue.BankAccount == null || bondIssue.BankAccount.AccountNumber == null)
                {
                    //Բաժնետոմսի ձեռքբերման Բանկի Ռեզիդենտների հաշվեհամար մուտքագրված չէ։
                    result.Add(new ActionError(2027));
                }
                else
                {
                    Account bankAccount = Account.GetAccountFromAllAccounts(bondIssue.BankAccount.AccountNumber);
                    if (bankAccount == null)
                    {
                        //Բանկի Ռեզիդենտների հաշվեհամարը գոյություն չունի:
                        result.Add(new ActionError(2028));
                    }
                    else
                    {
                        if (bankAccount.Currency != bondIssue.Currency)
                        {
                            //Բանկի Ռեզիդենտների հաշվեհամարի արժույթը չի համապատասխանում թողարկման արժույթին։
                            result.Add(new ActionError(2029));
                        }
                    }
                }

                if (bondIssue.BankAccountForNonResident == null || bondIssue.BankAccountForNonResident.AccountNumber == null)
                {
                    //Բաժնետոմսի ձեռքբերման Բանկի Ոչ Ռեզիդենտների հաշվեհամար մուտքագրված չէ։
                    result.Add(new ActionError(2030));
                }
                else
                {
                    Account bankAccount = Account.GetAccountFromAllAccounts(bondIssue.BankAccountForNonResident.AccountNumber);
                    if (bankAccount == null)
                    {
                        //Բանկի Ոչ Ռեզիդենտների հաշվեհամարը գոյություն չունի:
                        result.Add(new ActionError(2031));
                    }
                    else
                    {
                        if (bankAccount.Currency != bondIssue.Currency)
                        {
                            //Բանկի Ոչ Ռեզիդենտների հաշվեհամարի արժույթը չի համապատասխանում թողարկման արժույթին։
                            result.Add(new ActionError(2032));
                        }
                    }
                }

                if (action == Action.Add && bondIssue.ReplacementDate < DateTime.Now.Date)
                {
                    //Տեղաբաշխման սկիզբը փոքր է այսօրվա ամսաթվից:
                    result.Add(new ActionError(1489));
                }

            }

            if (bondIssue.Currency != "AMD" && bondIssue.Currency != "USD")
            {
                //Մուտքագրված արժույթը սխալ է
                result.Add(new ActionError(777));
            }

            return result;
        }

        public static List<ActionError> ValidateBondIssueReplacementDate(int bondIssueId)
        {
            List<ActionError> result = new List<ActionError>();

            if (!BondIssueDB.CheckBondIssueReplacementDate(bondIssueId))
                result.Add(new ActionError(2041));

            return result;
        }

        /// <summary>
        /// Քարտապանի անուն ազգանվան երկու տարբերակների համեմատում։
        /// </summary>
        /// <param name="InputText"></param>
        /// <param name="BaseText"></param>
        /// <returns></returns>
        internal static bool ValidateCardholderName(string InputText, string BaseText)
        {

            InputText = InputText.Replace("  ", " ").Replace("mr.", string.Empty).Replace("ms.", string.Empty).Replace("mrs.", string.Empty).Trim();


            List<string> list = new List<string>();
            string[] inputArr = InputText.Split(' ');
            Utility.MakeCombination(inputArr, 0, inputArr.Length - 1, list);

            foreach (var item in list)
            {
                int errorCount = Utility.LevenshteinDistance(item, BaseText);
                if (errorCount <= 3)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ԱՊՊԱ պայմանագրի հայտի պահպանման ստուգումներ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<ActionError> ValidateVehicleInsuranceOrder(VehicleInsuranceOrder order)
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(ValidateDocumentNumber(order, order.CustomerNumber));

            if (!string.IsNullOrEmpty(order.EmailAddress))
            {
                result.AddRange(ValidateEmail(order.EmailAddress));
            }
            else
            {
                //Էլ.հասցեն մուտքագրված չէ:
                result.Add(new ActionError(465));
            }

            if ((order.GroupId != 0) ? !OrderGroup.CheckGroupId(order.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Add(new ActionError(1628));
            }

            return result;

        }
        
        /// <summary>
        /// Ստուգում է հաճախորդի քարտի գառտնաբառի առկայությունը SAP CRM ում, կից քարտերի դեպքում ստուգումը կատարվում է կից քարտապան հաճախորդի համար
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static bool CustomerHasMotherName(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            bool CustomerHasMotherName = true;
                      
            if (order.Source == SourceType.Bank && ACBAOperationService.GetCustomerType(order.CustomerNumber) == (short)CustomerTypes.physical)
            {
                Customer customer = new Customer();
                
                long productId = 0;
                SupplementaryType SupplementaryType = SupplementaryType.NotDefined;

                switch (order)
                {
                    case CardRenewOrder CardRenewOrder:
                        SupplementaryType = CardRenewOrder.Card.SupplementaryType;
                        productId = CardRenewOrder.Card.ProductId;
                        break;
                    case NonCreditLineCardReplaceOrder NonCreditLineCardReplaceOrder:
                        SupplementaryType = NonCreditLineCardReplaceOrder.Card.SupplementaryType;
                        productId = NonCreditLineCardReplaceOrder.Card.ProductId;
                        break;
                    case CreditLineCardReplaceOrder CreditLineCardReplaceOrder:
                        SupplementaryType = CreditLineCardReplaceOrder.Card.SupplementaryType;
                        productId = CreditLineCardReplaceOrder.Card.ProductId;
                        break;
                }


                if ( SupplementaryType!= SupplementaryType.Linked)
                {
                    customer.CustomerNumber = order.CustomerNumber;
                }
                else
                {
                    customer.CustomerNumber = Card.GetCardHolderCustomerNumber(productId);
                }

                string motherName = customer.GetPasswordForCustomerDataOrder();

                if (String.IsNullOrWhiteSpace(motherName))
                {
                    CustomerHasMotherName = false;
                }
            }
            return CustomerHasMotherName;
        }
    }
}
