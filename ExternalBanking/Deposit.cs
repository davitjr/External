using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{


    /// <summary>
    /// Ավանդ
    /// </summary>
    public class Deposit
    {

        /// <summary>
        /// Ավանդի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// Ավանդի տեսակ
        /// </summary>
        public byte DepositType { get; set; }
        /// <summary>
        /// Ավանդի տեսակի նկարագրություն
        /// </summary>
        public string DepositTypeDescription { get; set; }
        /// <summary>
        /// Ավանդի համար
        /// </summary>
        public long DepositNumber { get; set; }
        /// <summary>
        /// Ավանդային հաշիվ
        /// </summary>
        public Account DepositAccount { get; set; }

        /// <summary>
        /// Ավանդի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ավանդի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Ավանդի սկզբնական գումար
        /// </summary>
        public double StartCapital { get; set; }

        /// <summary>
        /// Ավանդի մնացորդ
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// Ավանդի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ավանդի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Ավանդի կուտակված տոկոսագումար
        /// </summary>
        public double CurrentRateValue { get; set; }

        /// <summary>
        /// Ավանդի դադարեցման դեպքում ստացվող տոկոսագումար
        /// </summary>
        public double CancelRateValue { get; set; }

        /// <summary>
        /// Ավանդի դադարեցման տոկոսադրույք
        /// </summary>
        public double CancelRate { get; set; }

        /// <summary>   
        /// Ավանդի վերաձևաերպման նշան: true - վերաձևակերպվող, false-չվերաձևակերպվող
        /// </summary>
        public bool RecontractSign { get; set; }

        /// <summary>
        /// Ավանդի կարգավիճակ
        /// </summary>
        public byte DepositQuality { get; set; }

        /// <summary>
        /// Ավանդի կարգավիճակի նկարագրություն
        /// </summary>
        public string DepositQualityDescription { get; set; }

        /// <summary>
        /// Համատեղության տեսակ
        /// </summary>
        public ushort JointType { get; set; }

        /// <summary>
        /// Համատեղության տեսակի նկարագրություն
        /// </summary>
        public string JointTypeDescription { get; set; }

        /// <summary>
        /// Ավանդի փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Ավանդին կցված ընթացիկ հաշիվ
        /// </summary>
        public Account ConnectAccount { get; set; }
        /// <summary>
        /// Ընդամենը կուտակված տոկոսագումար
        /// </summary>
        public decimal TotalRateValue { get; set; }


        /// <summary>
        /// Արժույթաին ավանդների դեպքում տոկոսագումարի վճարման
        /// համար նախատեսված ընթացիկ հաշիվ
        /// </summary>
        public Account ConnectAccountForPercent { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք
        /// </summary>
        public decimal EffectiveInterestRate { get; set; }

        /// <summary>
        /// Քաղվածքի տրամադրման եղանակ
        /// </summary>
        public short? StatementDeliveryType { get; set; }

        /// <summary>
        /// Քաղվածքի սատցման եղանակի նկարագրություն
        /// </summary>
        public string StatementDeliveryTypeDescription { get; set; }


        /// <summary>
        /// Հաշվարկված տոկոսագումար առ ընթացիկ ամսվա առաջին օրը
        /// </summary>
        public decimal ProfitOnMonthFirstDay { get; set; }

        /// <summary>
        /// Տոկոսագումարի հաշվարկի օր
        /// </summary>
        public DateTime DayOfRateCalculation { get; set; }

        /// <summary>
        /// Սկզբնական ավանդի համար
        /// </summary>
        public long MainDepositNumber { get; set; }

        /// <summary>
        /// Թույլատրվում է ավանդի համալրումը
        /// </summary>
        public bool AllowAmountAddition { get; set; }

        /// <summary>
        /// Ներգրավող ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }

        /// <summary>
        /// Սպասարկող ՊԿ
        /// </summary>
        public int ServicingSetNumber { get; set; }

        /// <summary>
        /// Դադարեցնողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }


        /// <summary>
        /// Ավանդի փակման պատճառի տեսակ
        /// </summary>
        public ushort ClosingReasonType { get; set; }

        /// <summary>
        /// Ավանդի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonTypeDescription { get; set; }

        /// <summary>
        /// Ում կողմից է բացված(ՊԿ)
        /// </summary>
        public int KeeperOpen { get; set; }

        /// <summary>
        /// Բոնուսային տոկոս
        /// </summary>
        public decimal BonusInterestRate { get; set; }

        /// <summary>
        /// Ավանդի օպցիաներ
        /// </summary>
        public List<DepositOption> DepositOption { get; set; }

        /// <summary>
        /// Ավանդի Մ/ճ
        /// </summary>
        public int FilailCode { get; set; }

        /// <summary>
        /// Եկամտային հարկ
        /// </summary>
        public double ProfitTax { get; set; }

        /// <summary>
        /// Գործարքի ունիկալ համար
        /// </summary>
        public long DocID { get; set; }

        /// <summary>
        /// Տվյալների աղբյուր
        /// </summary>
        public SourceType SourceType { get; set; }
        /// <summary>
        /// Ավանդի տեսակի նկարագրություն /անգլերեն/
        /// </summary>
        public string DepositTypeDescriptionEng { get; set; }

        /// <summary>
        /// Հաշվարկված տոկոսագումար առ այսօր
        /// </summary>
        public decimal TaxedProfit { get; set; }

        /// <summary>
        /// Պրոդուկտի նշում
        /// </summary>
        public ProductNote ProductNote { get; set; }

        /// <summary>
        /// Սակագնից շեղում առկա է, թե ոչ
        /// </summary>
        public byte? IsVariation { get; set; }

        /// <summary>
        /// Հաղորդակցման եղանակն ընտրված է Front-ից
        /// </summary>
        public byte HasCommunicationType { get; set; }

        public Deposit()
        {

        }

        public static Deposit GetDeposit(ulong productId, ulong customerNumber)
        {
            return DepositDB.GetDeposit(productId, customerNumber);
        }


        public static List<Deposit> GetDeposits(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Deposit> deposits = new List<Deposit>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                deposits.AddRange(DepositDB.GetDeposits(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                deposits.AddRange(DepositDB.GetClosedDeposits(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                deposits.AddRange(DepositDB.GetDeposits(customerNumber));
                deposits.AddRange(DepositDB.GetClosedDeposits(customerNumber));
            }
            return deposits;
        }

        public static ActionError IsAllowedAmountAddition(string debitAccountNumber, string creditAccountNumber, double amountAMD, double amount, SourceType source)
        {
            return DepositDB.IsAllowedAmountAddition(debitAccountNumber, creditAccountNumber, amountAMD, amount, source);
        }
        /// <summary>
        /// Ավանդի տոկոսադրույք
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static DepositOrderCondition GetDepositOrderCondition(DepositOrder order, SourceType source, bool isEmployeeDeposit = false)
        {
            return DepositDB.GetDepositOrderCondition(order, source, isEmployeeDeposit);
        }
        public static List<ulong> ThirdPersonsCustomerNumbers(ulong customerNumber)
        {
            List<ulong> thirdpersons = DepositDB.GetThirdPersonsCustomerNumbers(customerNumber);
            return thirdpersons;
        }
        /// <summary>
        /// Ավանդի դադարեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondTermination(ulong customerNumber, string orderNumber)
        {
            bool secondTermination = DepositDB.IsSecondTermination(customerNumber, orderNumber);
            return secondTermination;
        }
        /// <summary>
        /// Ավանդի գրաֆիկ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static List<DepositRepayment> GetDepositRepayment(ulong productId)
        {
            return DepositDB.GetDepositRepayment(productId);
        }
        /// <summary>
        /// Վերադարձնում է տվյալ ավանդին կցված երրորդ անձանց
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ulong> GetDepositJointCustomers(ulong productId, ulong customerNumber)
        {
            return DepositDB.GetDepositJointCustomers(productId, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է ավանդը ավանդի համարով
        /// </summary>
        /// <param name="depositNumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static Deposit GetDeposit(long depositNumber, ulong customerNumber)
        {
            return DepositDB.GetDeposit(depositNumber, customerNumber);
        }

        public static bool CheckCustomerForEmployeeDeposit(ulong customerNumber)
        {
            return DepositDB.CheckCustomerForEmployeeDeposit(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է ավանդի տվյալների աղբյուրը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static SourceType GetDepositSource(ulong productId, ulong customerNumber)
        {
            return DepositDB.GetDepositSource(productId, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է ավանդի ընտրված օպցիայի տոկոսադրույքը
        /// </summary>
        /// <param name="depositOption"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static double GetBusinesDepositOptionRate(ushort depositOption, string currency)
        {
            return DepositDB.GetBusinesDepositOptionRate(depositOption, currency);
        }

        /// <summary>
        /// Վերադարձնում է գործող ավանդը ավանդային հաշվեհամարով
        /// </summary>
        /// <param name="depositFullNumber"></param>
        /// <returns></returns>
        public static Deposit GetActiveDeposit(string depositFullNumber)
        {
            return DepositDB.GetActiveDeposit(depositFullNumber);
        }

        /// <summary>
        /// Բիզնես ավանդի օպցիաների տվյալներ
        /// </summary>
        /// <returns></returns>
        public static List<DepositOption> GetBusinesDepositOptions(byte lang = 1)
        {
            List<DepositOption> depositOptions = new List<DepositOption>();
            DataTable dt = Info.GetBusinesDepositOptions();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DepositOption option = new DepositOption();
                option.Type = Convert.ToUInt16(dt.Rows[i]["option_type"]);
                option.TypeDescription = lang == (byte)Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i]["description"].ToString()) : dt.Rows[i]["description_eng"].ToString();
                option.OptionGroup = Convert.ToUInt16(dt.Rows[i]["option_group"]);
                depositOptions.Add(option);
            }
            return depositOptions;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրաֆիկը՝ նախքան ավանդի ձևակերպումը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static List<DepositRepayment> GetDepositRepayment(DepositRepaymentRequest request)
        {
            return DepositDB.GetDepositRepayment(request);
        }

        public static byte[] GetExistingDepositContract(long docId, int type)
        {
            return DepositDB.GetExistingDepositContract(docId, type);
        }

        public static byte[] PrintDepositContract(long docId, bool attachedFile, ulong customerNumber)
        {
            byte[] result;
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;
            string contractName = "DepositContract";
            Customer customer = new Customer();
            customer.CustomerNumber = customerNumber;
            DepositOrder order = customer.GetDepositorder(docId);

            if (order.DepositType == ExternalBanking.DepositType.BusinesDeposit && order.Deposit.DepositOption.Count == 1)
            {
                order.Deposit.DepositOption.Add(new ExternalBanking.DepositOption { OptionGroup = 1, Type = 0 });
            }


            bool recontractpossibility = Convert.ToBoolean(order.RecontractPossibility);
            if (recontractpossibility == true)
            {
                order.RecontractPossibility = YesNo.Yes;
            }
            else
            {
                order.RecontractPossibility = YesNo.No;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(key: "customerNumber", value: customerNumber.ToString());
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "depositNumber", value: order.Deposit.DepositNumber.ToString());// dt.Rows[0]["deposit_number"].ToString());
            parameters.Add(key: "filialCode", value: filialCode.ToString());
            parameters.Add(key: "currencyHB", value: order.Deposit.Currency.ToString()); //dt.Rows[0]["currency"].ToString());
            if (order.ThirdPersonCustomerNumbers.Count == 0)
            {
                parameters.Add(key: "thirdPersonCustomerNumberHB", value: "0");
            }
            else
            {
                parameters.Add(key: "thirdPersonCustomerNumberHB", value: order.ThirdPersonCustomerNumbers[0].Key.ToString());     //dt.Rows[0]["tp_customer_number"].ToString());
            }
            parameters.Add(key: "depositTypeHB", value: ((int)order.DepositType).ToString());// dt.Rows[0]["deposit_type"].ToString());
            parameters.Add(key: "dateOfBeginningHB", value: order.Deposit.StartDate.ToString());//   dt.Rows[0]["start_date"].ToString()); 



            parameters.Add(key: "dateOfNormalEndHB", value: order.Deposit.EndDate.ToString());
            parameters.Add(key: "connectAccountFullNumberHB", value: order.DebitAccount.AccountNumber.ToString());//dt.Rows[0]["capital_account"].ToString());
            parameters.Add(key: "connectAccountAddedHB", value: order.PercentAccount.AccountNumber.ToString());  //dt.Rows[0]["percent_account"].ToString());

            parameters.Add(key: "startCapitalHB", value: order.Amount.ToString());//  dt.Rows[0]["amount"].ToString());

            parameters.Add(key: "interestRateHB", value: order.Deposit.InterestRate.ToString());//dt.Rows[0]["interest_rate"].ToString());
            parameters.Add(key: "forExtractSendingHB", value: order.Deposit.StatementDeliveryType.ToString());//             dt.Rows[0]["statement_by_email"].ToString()); ;
            parameters.Add(key: "recontractPossibilityHB", value: order.RecontractPossibility.ToString());

            parameters.Add(key: "accountTypeHB", value: (order.AccountType - 1).ToString()); //  dt.Rows[0]["repayment_type"].ToString());

            if (order.DepositType == ExternalBanking.DepositType.BusinesDeposit)
            {
                foreach (DepositOption option in order.Deposit.DepositOption)
                {
                    switch (option.Type)
                    {
                        case 1:
                            parameters.Add(key: "allowAdditionOptionHB", value: "1");
                            parameters.Add(key: "allowDecreasingOptionHB", value: "0");
                            break;
                        case 2:
                            parameters.Add(key: "allowAdditionOptionHB", value: "0");
                            parameters.Add(key: "allowDecreasingOptionHB", value: "1");
                            break;
                        case 3:
                            parameters.Add(key: "allowAdditionOptionHB", value: "1");
                            parameters.Add(key: "allowDecreasingOptionHB", value: "1");
                            break;
                        case 4:
                            parameters.Add(key: "repaymentTypeHB", value: "1");
                            parameters.Add(key: "ratePeriodHB", value: "-1");
                            break;
                        case 5:
                            parameters.Add(key: "repaymentTypeHB", value: "2");
                            parameters.Add(key: "ratePeriodHB", value: "1");
                            break;
                        case 6:
                            parameters.Add(key: "repaymentTypeHB", value: "1");
                            parameters.Add(key: "ratePeriodHB", value: "1");
                            break;
                        default:
                            parameters.Add(key: "allowAdditionOptionHB", value: "0");
                            parameters.Add(key: "allowDecreasingOptionHB", value: "0");
                            break;
                    }
                }
            }
            else
            {
                if (order.DepositType == ExternalBanking.DepositType.DepositClassic || order.DepositType == ExternalBanking.DepositType.BusinesDeposit)
                {
                    parameters.Add(key: "repaymentTypeHB", value: "1");
                }
                else
                {
                    parameters.Add(key: "repaymentTypeHB", value: "2");
                }
                parameters.Add(key: "ratePeriodHB", value: "0");
                parameters.Add(key: "allowAdditionOptionHB", value: "0");
                parameters.Add(key: "allowDecreasingOptionHB", value: "0");
            }

            ContractServiceRef.Contract contract = null;

            if (attachedFile)
            {
                contract = new ContractServiceRef.Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 1;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }


            result = Contracts.RenderContract(contractName, parameters, "DepositContract.pdf", contract);
            return result;
        }

    }
}
