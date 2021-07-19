using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.ArcaDataServiceReference;

namespace ExternalBanking
{
    public class CashTerminal
    {

        /// <summary>
        /// CashTerminal-ի հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// CashTerminal-ի userName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Արտաքին կանխիկ տերմինալի գործողությունների հասանելիություններ
        /// </summary>
        public List<CTAccessibleAction> AccessibleActions { get; set; }

        /// <summary>
        /// CashTerminal-ի ID
        /// </summary>
        public int ID { get; set; }



        public CashTerminal()
        {
            AccessibleActions = new List<CTAccessibleAction>();
        }

        public CashTerminal(ulong customerNumber, string userName)
        {
            CustomerNumber = customerNumber;
            UserName = userName;
            AccessibleActions = new List<CTAccessibleAction>();
        }

        public PaymentRegistrationResult SaveCTPaymentOrder(CTPaymentOrder order)
        {
            PaymentRegistrationResult result = new PaymentRegistrationResult();
            order.DebitAccount = GetCTDebitAccount();
            order.CTCustomerDescription =Utility.ConvertAnsiToUnicode(Customer.GetCustomerDescription(this.CustomerNumber));
            result = order.SaveCTPaymentOrder(UserName);
            return result;
        }

        public PaymentRegistrationResult SaveCTLoanMatureOrder(CTLoanMatureOrder order)
        {
            PaymentRegistrationResult result = new PaymentRegistrationResult();
            order.DebitAccount = GetCTDebitAccount();
            order.CTCustomerDescription = Utility.ConvertAnsiToUnicode(Customer.GetCustomerDescription(this.CustomerNumber));
            result = order.SaveCTLoanMatureOrder(UserName);
            return result;
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում 
        /// </summary>
        /// <param name="paymentID"></param>
        /// <returns></returns>
        public PaymentStatus GetPaymentStatus(long paymentID)
        {
            return PaymentStatus.GetPaymentStatus(paymentID);
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում 
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public PaymentStatus GetPaymentStatusByOrderID(long orderID)
        {
            return PaymentStatus.GetPaymentStatusByOrderID(orderID);
        }

        /// <summary>
        /// 
        /// -i Նույնականացում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static CashTerminal CheckTerminalPassword(string userName, string password)
        {
            CashTerminal terminal = new CashTerminal();
            terminal= CTPaymentDB.CheckTerminalPassword(userName, password);
            if (terminal!=null && terminal.CustomerNumber!=0)
            {
                terminal.ID = GetTerminalID(terminal.CustomerNumber);
                //Implement you business logic here
            }
            return terminal;
        }

        /// <summary>
        /// Վերադարձնում է Terminal-ի ունիկալ համարը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static int GetTerminalID(ulong customerNumber)
        {
            return CTPaymentDB.GetTerminalID(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է CashTerminal-ի Դեբետ հաշվեհամարը կախված հաճախորդի համարից:
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public Account GetCTDebitAccount()
        {
            return CTPaymentDB.GetCTDebitAccount(this.CustomerNumber);
        }

        /// <summary>
        /// Փոխանցում հաշվին հայտի մանրամասներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static CTPaymentOrder GetCTPaymentOrder(long ID)
        {
            CTPaymentOrder order = new CTPaymentOrder();
            order.Id = ID;
            order.Get();
            Localization.SetCulture(order, new Culture(Languages.hy));
            return order;
        }

        /// <summary>
        /// Վարկի մարում հայտի մանրամասներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static CTLoanMatureOrder GetCTLoanMatureOrder(long ID)
        {
            CTLoanMatureOrder order = new CTLoanMatureOrder();
            order.Id = ID;
            order.Get();
            Localization.SetCulture(order, new Culture(Languages.hy));
            return order;
        }


        public C2CTransferResult C2CTransfer(C2CTransferOrder order)
        {
            C2CTransferResult result = new C2CTransferResult();
            CardIdentificationData sourceCard = new CardIdentificationData();

            sourceCard.Init(CustomerNumber);

            order.SourceCard = sourceCard;

            order.SourceID = CashTerminal.GetTerminalID(CustomerNumber);

            result = order.Save();
            return result;
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում 
        /// </summary>
        /// <param name="transferID"></param>
        /// <returns></returns>
        public C2CTransferStatusResponse GetC2CTransferStatus(long transferID)
        {
            return C2CTransferStatusResponse.GetC2CTransferStatus(transferID);
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում 
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public C2CTransferStatusResponse GetC2CTransferStatusByOrderID(long orderID)
        {
            return C2CTransferStatusResponse.GetC2CTransferStatusByOrderID(orderID, this.ID);
        }

        public static CashTerminal GetTerminal(int terminalID)
        {
            CashTerminal terminal = new CashTerminal();
            terminal = CTPaymentDB.GetTerminalByID(terminalID);
            return terminal;
        }
    }
}
