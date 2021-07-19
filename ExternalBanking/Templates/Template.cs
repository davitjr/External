﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ձևանմուշ
    /// </summary>
    public class Template
    {
        #region Properties

        /// <summary>
        /// Ձևանմուշի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Ձևանմուշի ունիկալ համար՝ ըստ հաճախորդի
        /// </summary>
        public int SerialNumber { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong TemplateCustomerNumber { get; set; }

        /// <summary>
        /// Ձևանմուշի անուն
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Ձևանմուշի ստեղծման ամսաթիվ
        /// </summary>
        public DateTime TemplateRegistrationDate { get; set; }

        /// <summary>
        /// Ձևանմուշի կարգավիճակ (0` պասիվ, 1՝ ակտիվ)
        /// </summary>
        public TemplateStatus Status { get; set; }

        /// <summary>
        /// Ձևանմուշի տեսակ (1` հաճախորդի կողմից մուտքագրված ձևանմուշ, 2՝ գործարքների խմբի մեջ ներառված ծառայության ձևանմուշ)
        /// </summary>
        public TemplateType TemplateType { get; set; }

        /// <summary>
        /// Խմբի ունիկալ համար, եթե ձևանմուշը խմբի մեջ ներառված ծառայություն է
        /// </summary>
        public int TemplateGroupId { get; set; }

        /// <summary>
        /// Ձևանմուշի մուտքագրման աղբյուր
        /// </summary>
        public SourceType TemplateSourceType { get; set; }

        /// <summary>
        /// Ձևանմուշը վերջին փոփոխած օգտագործողի ունիկալ համար
        /// </summary>
        public int ChangeUserId { get; set; }

        /// <summary>
        /// Ծառայության տեսակ 
        /// </summary>
        public OrderType TemplateDocumentType { get; set; }

        /// <summary>
        /// Ծառայության ենթատեսակ
        /// </summary>
        public byte TemplateDocumentSubType { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double TemplateAmount { get; set; }

        /// <summary>
        /// Ելքագրվող հաշիվ
        /// </summary>
        public Account TemplateDebetAccount { get; set; }


        /// <summary>
        /// Ծառայության ենթատեսակի նկարագրություն
        /// </summary>
        public string TemplateDocumentSubTypeDescription { get; set; }

        /// <summary>
        /// խմբային վճարման կարճ նկարագրություն
        /// </summary>
        public GroupTemplateShrotInfo GroupTemplateShrotInfo { get; set; }


        #endregion Properties


        /// <summary>
        /// Ձևանմուշի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if(this.TemplateType == TemplateType.CreatedByCustomer)
            {
                if (String.IsNullOrEmpty(this.TemplateName))
                {
                    //Մուտքագրեք ձևանմուշի անունը
                    result.Errors.Add(new ActionError(461));
                    return result;
                }
                else if (ExistsTemplateByName())
                {
                    //Տվյալ անունով ձևանմուշ արդեն գոյություն ունի
                    result.Errors.Add(new ActionError(462));
                }
            }
            if (this.TemplateType == TemplateType.CreatedAsGroupService && this.TemplateGroupId <= 0)
            {
                //Խումբն ընտրված չէ։
                result.Errors.Add(new ActionError(1577));
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հաճախորդն ունի տվյալ անունով ձևանմուշ, թե ոչ
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public bool ExistsTemplateByName()
        {
            return TemplateDB.ExistsTemplateByName(this.TemplateCustomerNumber, this.TemplateName,this.ID);
        }

        /// <summary>
        /// Ձևանմուշի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static ActionResult ChangeTemplateStatus(int id, TemplateStatus status)
        {
            ActionResult result = new ActionResult();
           
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TemplateDB.ChangeTemplateStatus(id, status);

                scope.Complete();
            }
           
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ խմբի ծառայությունների ցանկը
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<Template> GetGroupTemplates(int groupId, TemplateStatus status, Languages lang)
        {
            List<Template> templates = TemplateDB.GetGroupTemplates(groupId);
            foreach (Template template in templates)
            {
                template.GroupTemplateShrotInfo.Currency = Account.GetAccountCurrency(template.GroupTemplateShrotInfo.DebitAccount);
                if (template.TemplateDocumentType == OrderType.CommunalPayment)
                {
                    template.GroupTemplateShrotInfo = GetCommunalTemplateDetails(template.GroupTemplateShrotInfo,template.ID,
                        template.TemplateCustomerNumber,template.TemplateSourceType);

                    Dictionary<string, string> types = new Dictionary<string, string>();

                    template.TemplateDocumentSubTypeDescription = Communal.GetCommunalDescriptionByType((int)template.GroupTemplateShrotInfo.CommunalType, lang);
                }
                else if(template.TemplateDocumentType == OrderType.LoanMature)
                {
                    template.GroupTemplateShrotInfo = GetLoanTemplateDetails(template.GroupTemplateShrotInfo,template.TemplateCustomerNumber);
                    template.TemplateAmount = template.GroupTemplateShrotInfo.Amount;
                    int matureType;
                    if (template.TemplateDocumentSubType == 6)
                    {
                        template.TemplateDocumentSubType = 2;
                        matureType = 2;
                    }
                    else
                    {
                        matureType = 9;
                    }
                    template.TemplateDocumentSubTypeDescription = Info.GetLoanMatureTypeDescriptionForIBankingByMatureType(matureType, (byte)lang);
                }
                else if(template.TemplateDocumentType == OrderType.Convertation)
                {
                    template.GroupTemplateShrotInfo = GetCurrencyExchangeTemplateDetails(template.GroupTemplateShrotInfo);
                }
            }
            return templates;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ձևանմուշների ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<Template> GetCustomerTemplates(ulong customerNumber, TemplateStatus status)
        {
            List<Template> customerTemplates = TemplateDB.GetCustomerTemplates(customerNumber, status);

            return customerTemplates;
        }

        /// <summary>
        /// Պահպանում է խմբային ծառայության միջնորդավճար(ներ)ը
        /// </summary>
        /// <returns></returns>
        internal ActionResult SaveTemplateFee(Order order)
        {
            ActionResult result = new ActionResult();

            if (order.Fees != null && order.Fees.Count > 0)
            {
                TemplateDB.SaveTemplateFee(this, order);
            }

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Վերադարձնում է խմբային ծառայության միջնորդավճարը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static List<OrderFee> GetTemplateFees(int templateId)
        {
            List<OrderFee> fees = new List<OrderFee>();
            fees = TemplateDB.GetTemplateFees(templateId);
            return fees;
        }

        public static int GetCustomerTemplatesCounts(ulong customerNumber)
        {
            return TemplateDB.GetCustomerTemplatesCounts(customerNumber);
        }

        private static GroupTemplateShrotInfo GetCommunalTemplateDetails(GroupTemplateShrotInfo template,int templateId, ulong templateCustomerNumber, SourceType source)
        {
            UtilityPaymentOrderTemplate utility = UtilityPaymentOrderTemplate.Get(templateId, templateCustomerNumber);
            SearchCommunal searchCommunal = new SearchCommunal
            {
                CommunalType = utility.UtilityPaymentOrder.CommunalType,
                AbonentNumber = utility.UtilityPaymentOrder.Code,
                AbonentType = (short)utility.UtilityPaymentOrder.AbonentType
            };


            var search = searchCommunal.SearchCommunalByType(source);

            template.Debt = search[0].Debt.Value;
            template.Amount = 0; //գերավճարի դեպքում պետք է 0 ցուցադրվի 
            switch (utility.UtilityPaymentOrder.CommunalType)
            {
                case CommunalTypes.None:
                    break;
                case CommunalTypes.ENA:
                case CommunalTypes.Gas:
                case CommunalTypes.YerWater:
                case CommunalTypes.BeelineInternet:
                case CommunalTypes.ArmenTel:
                case CommunalTypes.VivaCell:
                    if (template.Debt < 0) template.Amount = Math.Abs(template.Debt); //պարտքը ստանում ենք բացասական թվով
                    break;
                case CommunalTypes.UCom:
                case CommunalTypes.Orange:
                    if (template.Debt < 0) template.Amount = template.Debt; //պարտքը ստանում ենք դրական թվով
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(search[0].Description))
            {
                int descriptionIndex = search[0].Description.IndexOf("\r\n");
                if(descriptionIndex != -1)
                    template.ReceiverName = search[0].Description.Substring(0, descriptionIndex);
            }
            return template;
        }

        private static GroupTemplateShrotInfo GetLoanTemplateDetails(GroupTemplateShrotInfo template, ulong customerNumber)
        {
            Loan loan = Loan.GetLoan(template.LoanAppId, customerNumber);
            template.LoanType = loan.LoanType;
            template.LoanInitialAmount = loan.ContractAmount;
            var nextRepayment = loan.GetLoanNextRepayment();
            template.LoanNextRepayment = nextRepayment.RepaymentDate;
            template.Amount = loan.Currency == "AMD"? nextRepayment.TotalRepayment: nextRepayment.CapitalRepayment;
            if (loan.Currency == "AMD")
            {
                template.Amount = nextRepayment.TotalRepayment;
            }
            else
            {
                template.Amount = nextRepayment.CapitalRepayment;
                template.RateAmount = nextRepayment.RateRepayment;
            }
            return template;
        }
        private static GroupTemplateShrotInfo GetCurrencyExchangeTemplateDetails(GroupTemplateShrotInfo template)
        {
            var rates = ExchangeRate.GetExchangeRates();
            var debitCurrency = Account.GetAccountCurrency(template.DebitAccount);
            var creditCurrency = Account.GetAccountCurrency(template.ReceiverAccount);
            if (debitCurrency == "AMD")
            {
                template.Currency = Account.GetAccountCurrency(template.ReceiverAccount);
            }
            if(debitCurrency == "AMD" && creditCurrency != "AMD")
            {
                ExchangeRate rate = rates.Where(x => x.SourceCurrency == creditCurrency).FirstOrDefault();
                template.Rate = rate.SaleRate;
                template.ConvertationAmount = template.Amount * template.Rate;
            }
            else if (debitCurrency != "AMD" && creditCurrency == "AMD")
            {
                ExchangeRate rate = rates.Where(x => x.SourceCurrency == debitCurrency).FirstOrDefault();
                template.Rate = rate.BuyRate;
                template.ConvertationAmount = template.Amount * template.Rate;
            }
            else if(debitCurrency != "AMD" && creditCurrency != "AMD")
            {
                float debitRate = rates.Where(x => x.SourceCurrency == debitCurrency).FirstOrDefault().BuyRateCross;
                float creditRate = rates.Where(x => x.SourceCurrency == creditCurrency).FirstOrDefault().SaleRateCross;
                var variant = CurrencyExchangeOrder.GetCrossConvertationVariant(debitCurrency, creditCurrency);
                if(variant == 1)
                    template.Rate = Math.Round((debitRate / creditRate),6);
                else if(variant == 2)
                    template.Rate = Math.Round((creditRate / debitRate), 6);
                template.ConvertationAmount = template.Amount * template.Rate;

            }
            return template;
        }
    }
}