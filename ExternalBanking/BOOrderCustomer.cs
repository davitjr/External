using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.XBManagement;
using System.Transactions;

namespace ExternalBanking
{
    public enum OrderCustomerType : short
    {
        /// <summary>
        /// Կատարող
        /// </summary>
        Payer = 1,

        /// <summary>
        /// 2 - Հաշվետեր (դեբետ)
        /// </summary>
        DebitAccountHolder = 2,

        /// <summary>
        /// 3 - Հաշվետեր (կրեդիտ)
        /// </summary>
        CreditAccountHolder = 3,

        /// <summary>
        /// 4 - Ստացող
        /// </summary>
        Receiver = 4,

        /// <summary>
        /// 5 - Ուղարկող
        /// </summary>
        Sender = 5,

        /// <summary>
        /// 6 - Պարտատեր
        /// </summary>
        Creditor = 6,

        /// <summary>
        /// 7 - Համատեղ հաշվետեր (Ավանդ)
        /// </summary>
        DepositJointAccountHolder = 7,

        /// <summary>
        /// 8 - Երրորդ անձ (Ավանդ)
        /// </summary>
        DepositThirdPerson = 8,

        /// <summary>
        /// 9 - Լիազորված անձ
        /// </summary>
        Assignee = 9
    }

    public class BOOrderCustomer
    {
        /// <summary>
        /// Հայտի համար
        /// </summary>
        public ulong OrderId { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի Անուն
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Փաստաթղթի համար
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Փաստաթղթի տեսակ
        /// </summary>
        public int documentType { get; set; }


        internal static BOOrderCustomer GetPayer(OPPerson payer , ulong orderId)
        {   
            BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
            oneOrderCustomer.OrderId = orderId;
            oneOrderCustomer.Type = (short)OrderCustomerType.Payer;
            oneOrderCustomer.CustomerNumber = payer.CustomerNumber;
            oneOrderCustomer.CustomerName = payer.PersonLastName + " " + payer.PersonName;
            oneOrderCustomer.DocumentNumber = payer.PersonDocument ;
            oneOrderCustomer.documentType = payer.DocumentType;

            return oneOrderCustomer;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդի պահպանում:
        /// </summary>                
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public ActionResult Save(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            
            result = BOOrderCustomerDB.Save(this, user.userID);

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PaymentOrder paymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            result.ResultCode = ResultCode.Normal;

            if (paymentOrder.OPPerson != null && paymentOrder.OPPerson.CustomerNumber != 0)
            {                
                orderCustomersList.Add(GetPayer(paymentOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(paymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(paymentOrder.ReceiverAccount.AccountNumber, orderId, OrderCustomerType.CreditAccountHolder));

            if (paymentOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = paymentOrder.Receiver;                
                orderCustomersList.Add(oneOrderCustomer);
            }     

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(BudgetPaymentOrder budgetPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>(); 
            ActionResult result = new ActionResult();

            if (budgetPaymentOrder.OPPerson != null && budgetPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(budgetPaymentOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(budgetPaymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));

            if (budgetPaymentOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = budgetPaymentOrder.Receiver;
                orderCustomersList.Add(oneOrderCustomer);
            }     

            if (budgetPaymentOrder.CreditorDescription != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Creditor;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = budgetPaymentOrder.CreditorDescription;
                oneOrderCustomer.DocumentNumber = budgetPaymentOrder.CreditorDocumentNumber;
                oneOrderCustomer.documentType = budgetPaymentOrder.CreditorDocumentType;
                orderCustomersList.Add(oneOrderCustomer);
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(UtilityPaymentOrder utilityPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (utilityPaymentOrder.OPPerson != null && utilityPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(utilityPaymentOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(utilityPaymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(MatureOrder matureOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (matureOrder.OPPerson != null && matureOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(matureOrder.OPPerson, orderId));
            }

            if (matureOrder.Account != null)
            {
                orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(matureOrder.Account.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            }
            else
            {
                orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(matureOrder.PercentAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(DepositOrder depositOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (depositOrder.OPPerson != null && depositOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(depositOrder.OPPerson, orderId));
            }

            if (depositOrder.AccountType == 2 || depositOrder.AccountType == 3)
            {
                depositOrder.ThirdPersonCustomerNumbers.ForEach(m =>
                {
                    BOOrderCustomer orderCustomer = new BOOrderCustomer();
                    orderCustomer.OrderId = orderId;
                    if (depositOrder.AccountType == 2)
                    {
                        orderCustomer.Type = (short)OrderCustomerType.DepositJointAccountHolder;
                    }
                    else if (depositOrder.AccountType == 3)
                    {
                        orderCustomer.Type = (short)OrderCustomerType.DepositThirdPerson;
                    }
                    orderCustomer.CustomerNumber = m.Key;
                    orderCustomer.CustomerName = Utility.ConvertAnsiToUnicode(m.Value);
                    orderCustomer.DocumentNumber = "";
                    orderCustomer.documentType = 0;
                    orderCustomersList.Add(orderCustomer);
                });
            }            

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(AccountOrder accountOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (accountOrder.OPPerson != null && accountOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(accountOrder.OPPerson, orderId));
            }

            if (accountOrder.AccountType == 2)
            {
                accountOrder.JointCustomers.ForEach(m =>
                {
                    BOOrderCustomer orderCustomer = new BOOrderCustomer();
                    orderCustomer.OrderId = orderId;
                    orderCustomer.Type = (short)OrderCustomerType.DepositJointAccountHolder;
                    orderCustomer.CustomerNumber = m.Key;
                    orderCustomer.CustomerName = Utility.ConvertAnsiToUnicode(m.Value);
                    orderCustomer.DocumentNumber = "";
                    orderCustomer.documentType = 0;
                    orderCustomersList.Add(orderCustomer);
                });
            }            

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(AccountClosingOrder accountClosingOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (accountClosingOrder.OPPerson != null && accountClosingOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(accountClosingOrder.OPPerson, orderId));
            }
                        
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicUtilityPaymentOrder periodicUtilityPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (periodicUtilityPaymentOrder.OPPerson != null && periodicUtilityPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(periodicUtilityPaymentOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicBudgetPaymentOrder periodicBudgetPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (periodicBudgetPaymentOrder.OPPerson != null && periodicBudgetPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(periodicBudgetPaymentOrder.OPPerson, orderId));
            }

            if (periodicBudgetPaymentOrder.BudgetPaymentOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = periodicBudgetPaymentOrder.BudgetPaymentOrder.Receiver;
                orderCustomersList.Add(oneOrderCustomer);
            }
            
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicPaymentOrder periodicPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (periodicPaymentOrder.OPPerson != null && periodicPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(periodicPaymentOrder.OPPerson, orderId));
            }

            if (periodicPaymentOrder.PaymentOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = periodicPaymentOrder.PaymentOrder.Receiver;
                orderCustomersList.Add(oneOrderCustomer);
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(InternationalPaymentOrder internationalPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (internationalPaymentOrder.OPPerson != null && internationalPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(internationalPaymentOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(internationalPaymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
          //  orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(internationalPaymentOrder.ReceiverAccount.AccountNumber, orderId, OrderCustomerType.CreditAccountHolder));

            if (internationalPaymentOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = internationalPaymentOrder.Receiver;
                orderCustomersList.Add(oneOrderCustomer);
            }

            if (internationalPaymentOrder.Sender != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Sender;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = internationalPaymentOrder.Sender;
                if (internationalPaymentOrder.SenderType == 6)
                {
                    oneOrderCustomer.DocumentNumber = internationalPaymentOrder.SenderPassport;
                }
                else
                {
                    oneOrderCustomer.DocumentNumber = internationalPaymentOrder.SenderCodeOfTax;
                }
                
                orderCustomersList.Add(oneOrderCustomer);
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(FastTransferPaymentOrder fastTransferPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (fastTransferPaymentOrder.OPPerson != null && fastTransferPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(fastTransferPaymentOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(fastTransferPaymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            //  orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(internationalPaymentOrder.ReceiverAccount.AccountNumber, orderId, OrderCustomerType.CreditAccountHolder));

 
            if (fastTransferPaymentOrder.Sender != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Sender;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = fastTransferPaymentOrder.Sender;
               oneOrderCustomer.DocumentNumber = fastTransferPaymentOrder.SenderPassport;
            

                orderCustomersList.Add(oneOrderCustomer);
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        public static ActionResult Save(ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (receivedFastTransferPaymentOrder.OPPerson != null && receivedFastTransferPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(receivedFastTransferPaymentOrder.OPPerson, orderId));
            }

            //orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(receivedFastTransferPaymentOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(receivedFastTransferPaymentOrder.ReceiverAccount.AccountNumber, orderId, OrderCustomerType.CreditAccountHolder));


            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }
        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(TransitPaymentOrder transitPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (transitPaymentOrder.OPPerson != null && transitPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(transitPaymentOrder.OPPerson, orderId));
            }            

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CardClosingOrder cardClosingOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (cardClosingOrder.OPPerson != null && cardClosingOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(cardClosingOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CreditLineTerminationOrder creditLineTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (creditLineTerminationOrder.OPPerson != null && creditLineTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(creditLineTerminationOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CurrencyExchangeOrder currencyExchangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (currencyExchangeOrder.OPPerson != null && currencyExchangeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(currencyExchangeOrder.OPPerson, orderId));
            }

            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(currencyExchangeOrder.DebitAccount.AccountNumber, orderId, OrderCustomerType.DebitAccountHolder));
            orderCustomersList.AddRange(BOOrderCustomerDB.GetAccountCustomers(currencyExchangeOrder.ReceiverAccount.AccountNumber, orderId, OrderCustomerType.CreditAccountHolder));

            if (currencyExchangeOrder.Receiver != null)
            {
                BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                oneOrderCustomer.OrderId = orderId;
                oneOrderCustomer.Type = (short)OrderCustomerType.Receiver;
                oneOrderCustomer.CustomerNumber = 0;
                oneOrderCustomer.CustomerName = currencyExchangeOrder.Receiver;
                orderCustomersList.Add(oneOrderCustomer);
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(DepositTerminationOrder depositTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (depositTerminationOrder.OPPerson != null && depositTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(depositTerminationOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicTerminationOrder periodicTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (periodicTerminationOrder.OPPerson != null && periodicTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(periodicTerminationOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(ServicePaymentOrder servicePaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (servicePaymentOrder.OPPerson != null && servicePaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(servicePaymentOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CashPosPaymentOrder cashPosPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (cashPosPaymentOrder.OPPerson != null && cashPosPaymentOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(cashPosPaymentOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(AccountFreezeOrder accountFreezeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (accountFreezeOrder.OPPerson != null && accountFreezeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(accountFreezeOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(AccountUnfreezeOrder accountUnfreezeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (accountUnfreezeOrder.OPPerson != null && accountUnfreezeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(accountUnfreezeOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(HBActivationOrder activationHBOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (activationHBOrder.OPPerson != null && activationHBOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(activationHBOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(FeeForServiceProvidedOrder feeForServiceProvidedOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (feeForServiceProvidedOrder.OPPerson != null && feeForServiceProvidedOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(feeForServiceProvidedOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CredentialOrder credentialOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (credentialOrder.OPPerson != null && credentialOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(credentialOrder.OPPerson, orderId));
            }

            if (credentialOrder.Credential.AssigneeList != null)
            {
                credentialOrder.Credential.AssigneeList.ForEach(oneAssignee =>
                {
                    BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                    oneOrderCustomer.OrderId = orderId;
                    oneOrderCustomer.Type = (short)OrderCustomerType.Assignee;
                    oneOrderCustomer.CustomerNumber = oneAssignee.CustomerNumber;
                    oneOrderCustomer.CustomerName = (oneAssignee.AssigneeLastName ?? "") + " " + (oneAssignee.AssigneeFirstName ?? "") + " " + (oneAssignee.AssigneeMiddleName ?? "");
                    oneOrderCustomer.DocumentNumber = oneAssignee.AssigneeDocumentNumber;
                    oneOrderCustomer.documentType = oneAssignee.AssigneeDocumentType;  
                    orderCustomersList.Add(oneOrderCustomer);
                }
                );
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CredentialTerminationOrder credentialTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (credentialTerminationOrder.OPPerson != null && credentialTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(credentialTerminationOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PensionApplicationOrder pensionApplicationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (pensionApplicationOrder.OPPerson != null && pensionApplicationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(pensionApplicationOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }
        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(DepositCaseOrder depositCaseOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (depositCaseOrder.OPPerson != null && depositCaseOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(depositCaseOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(DepositCasePenaltyMatureOrder depositCasePenaltyMatureOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (depositCasePenaltyMatureOrder.OPPerson != null && depositCasePenaltyMatureOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(depositCasePenaltyMatureOrder.OPPerson, orderId));
            }

            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PeriodicSwiftStatementOrder periodicSwiftStatementOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (periodicSwiftStatementOrder.OPPerson != null && periodicSwiftStatementOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(periodicSwiftStatementOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(TransferToShopOrder transferToShopOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (transferToShopOrder.OPPerson != null && transferToShopOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(transferToShopOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(InsuranceOrder insuranceOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (insuranceOrder.OPPerson != null && insuranceOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(insuranceOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CardDataChangeOrder cardServiceFeeDataChangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (cardServiceFeeDataChangeOrder.OPPerson != null && cardServiceFeeDataChangeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(cardServiceFeeDataChangeOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(CardServiceFeeGrafikDataChangeOrder cardServiceFeeGrafikDataChangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (cardServiceFeeGrafikDataChangeOrder.OPPerson != null && cardServiceFeeGrafikDataChangeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(cardServiceFeeGrafikDataChangeOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }



        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(HBApplicationOrder hBApplicationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (hBApplicationOrder.OPPerson != null && hBApplicationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(hBApplicationOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }


        ///// <summary>
        ///// Հանձնարարականին կցված հաճախորդների պահպանում:
        ///// </summary>               
        ///// <param name="orderId">Հայտի համար</param>
        ///// <param name="user">Օգտագործող</param>
        ///// <returns></returns>
        //public static ActionResult Save(HBUserOrder hBUserOrder, ulong orderId, ACBAServiceReference.User user)
        //{
        //    List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
        //    ActionResult result = new ActionResult();

        //    if (hBUserOrder.OPPerson != null && hBUserOrder.OPPerson.CustomerNumber != 0)
        //    {
        //        orderCustomersList.Add(GetPayer(hBUserOrder.OPPerson, orderId));
        //    }
        //    foreach (BOOrderCustomer orderCustomer in orderCustomersList)
        //    {
        //        result = orderCustomer.Save(user);
        //    };

        //    return result;
        //}
    //    /// <summary>
    //    /// Հանձնարարականին կցված հաճախորդների պահպանում:
    //    /// </summary>               
    //    /// <param name="orderId">Հայտի համար</param>
    //    /// <param name="user">Օգտագործող</param>
    //    /// <returns></returns>
    //    internal static ActionResult Save(HBTokenOrder hBTokenOrder, ulong orderId, ACBAServiceReference.User user)
    //    {
    //        List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
    //        ActionResult result = new ActionResult();

        //        if (hBTokenOrder.OPPerson != null && hBTokenOrder.OPPerson.CustomerNumber != 0)
        //        {
        //            orderCustomersList.Add(GetPayer(hBTokenOrder.OPPerson, orderId));
        //        }
        //        foreach (BOOrderCustomer orderCustomer in orderCustomersList)
        //        {
        //            result = orderCustomer.Save(user);
        //        };
        //        return result;
        //    }


        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում:
        /// </summary>               
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ActionResult Save(PhoneBankingContractOrder phoneBankingApplicationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (phoneBankingApplicationOrder.OPPerson != null && phoneBankingApplicationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(phoneBankingApplicationOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        public static ActionResult Save(CardStatusChangeOrder phoneBankingApplicationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (phoneBankingApplicationOrder.OPPerson != null && phoneBankingApplicationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(phoneBankingApplicationOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        public static ActionResult Save(FactoringTerminationOrder factoringTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (factoringTerminationOrder.OPPerson != null && factoringTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(factoringTerminationOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        public static ActionResult Save(LoanProductTerminationOrder guaranteeTerminationOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (guaranteeTerminationOrder.OPPerson != null && guaranteeTerminationOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(guaranteeTerminationOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }

        public static ActionResult Save(DepositDataChangeOrder depositDataChangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (depositDataChangeOrder.OPPerson != null && depositDataChangeOrder.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(depositDataChangeOrder.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }
        /// <summary>
        /// Հանձնարարականին կցված հաճախորդների պահպանում բոլոր տեսակի հայտերի համար:
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ActionResult Save(Order order, ulong orderId, ACBAServiceReference.User user)
        {
            List<BOOrderCustomer> orderCustomersList = new List<BOOrderCustomer>();
            ActionResult result = new ActionResult();

            if (order.OPPerson != null && order.OPPerson.CustomerNumber != 0)
            {
                orderCustomersList.Add(GetPayer(order.OPPerson, orderId));
            }
            foreach (BOOrderCustomer orderCustomer in orderCustomersList)
            {
                result = orderCustomer.Save(user);
            }

            return result;
        }
    }
}