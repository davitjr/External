using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class TransactionTypeByAML
    {
        /// <summary>
        /// Գործարքի տեսակ
        /// </summary>
        public int? TransactionType { get; set; }

        /// <summary>
        /// Լրացուցիչ նկարագրություն
        /// </summary>
        public string AdditionalDescription { get; set; }

        public void SaveTransactionTypeByAML(long order_id)
        {
            TransactionTypeByAMLDB.SaveTransactionTypeByAML(order_id, this);
        }


        public static ActionResult Validate(Order order)
        {
            ActionResult result = new ActionResult();

            if (TransactionTypeByAMLDB.CheckFor5mln((int)order.Type, order.SubType))
            {
                result.Errors.Add(ValidateForTransactionType5mil(order));
            }
            else if (TransactionTypeByAMLDB.CheckFor20mln((int)order.Type, order.SubType))
            {
                result.Errors.Add(ValidateForTransactionType20mil(order));
            }

            if (order.TransactionTypeByAML != null)
            {
                result.Errors.AddRange(TransactionTypeAdditionalValidations(order));
            }


            if (!result.Errors.Exists(m => m.Code != 0))
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.Errors.RemoveAll(m => m.Code == 0);
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        public static ActionError ValidateForTransactionType5mil(Order order)
        {
            ActionError error = new ActionError();

            if (order.TransactionTypeByAML == null)
            {
                if (order is ReestrUtilityPaymentOrder)
                {
                    if (((ReestrUtilityPaymentOrder)order).COWaterReestrDetails.Exists(m => m.TotalCharge >= 5000000))
                    {
                        //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                        error = new ActionError(1967);
                    }
                }
                else if (order is ReestrTransferOrder)
                {
                    if (((ReestrTransferOrder)order).ReestrTransferAdditionalDetails.Exists(m => (order.Currency != "AMD" ? m.Amount * Utility.GetCBKursForDate(DateTime.Now, order.Currency) : m.Amount) >= 5000000))
                    {
                        //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                        error = new ActionError(1967);
                    }
                }
                else if (order is CurrencyExchangeOrder)
                {
                    if (((CurrencyExchangeOrder)order).AmountInAmd >= 5000000 || order.Amount * Utility.GetCBKursForDate(DateTime.Now, order.Currency) >= 5000000)
                    {
                        //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                        error = new ActionError(1967);
                    }
                }
                else if (order.Currency != "AMD" ? (order.Amount * Utility.GetCBKursForDate(DateTime.Now, order.Currency)) >= 5000000 : order.Amount >= 5000000)
                {
                    //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                    error = new ActionError(1967);
                }
            }
            return error;
        }
        public static ActionError ValidateForTransactionType20mil(Order order)
        {
            ActionError error = new ActionError();

            if (order.TransactionTypeByAML == null)
            {
                if (order is ReestrTransferOrder)
                {
                    if (((ReestrTransferOrder)order).ReestrTransferAdditionalDetails.Exists(m => m.Amount >= 20000000))
                    {
                        //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                        error = new ActionError(1967);
                    }
                }
                else if (order is CurrencyExchangeOrder)
                {
                    if (((CurrencyExchangeOrder)order).AmountInAmd >= 20000000 || order.Amount * Utility.GetCBKursForDate(DateTime.Now, order.Currency) >= 20000000)
                    {
                        //«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                        error = new ActionError(1967);
                    }
                }
                else if (order.Currency != "AMD" ? (order.Amount * Utility.GetCBKursForDate(DateTime.Now, order.Currency)) >= 20000000 : order.Amount >= 20000000)
                {//«Գործարքի տեսակ» դաշտը պարտադիր է լրացման համար
                    error = new ActionError(1967);
                }
            }
            return error;
        }

        public static List<ActionError> TransactionTypeAdditionalValidations(Order order)
        {
            List<ActionError> errors = new List<ActionError>();

            if (order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.RealEstate && string.IsNullOrEmpty(order.TransactionTypeByAML.AdditionalDescription))
            {//Տվյալ տեսակի գործարքի դեպքում պարտադիր է «Լրացուցիչ նկարագրություն» դաշտում լրացնել գույքի հասցեն
                errors.Add(new ActionError(1972));
            }

            if (order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.RealEstate && TransactionTypeByAMLDB.CheckFor5mln((int)order.Type, order.SubType)) // օգտագործում ենք 5մլն ի ստուգումը ստուգելու համար կանխիկ է թե ոչ
            {
                if (order.Currency != "AMD" ? order.Amount * Utility.GetKursForDate(DateTime.Now, order.Currency, 0, order.FilialCode) >= 50000000 : order.Amount >= 50000000)
                {//Տվյալ տեսակի գործարքի դեպքում պարտադիր է 50 մլն ՀՀ դրամը գերազանցող մասը գնորդին վճարել անկանխիկ եղանակով, այլապես գործարքը կարող է առոչինչ համարվել
                    errors.Add(new ActionError(1973));
                }
            }

            if ((order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.PaymentForService ||
                order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.PaymentForProperty ||
                order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.RealEstate) && order.Currency != "AMD")
            {//Տվյալ տեսակի գործարքը պետք է կատարվի ՀՀ դրամով

                if (order.Type == OrderType.InternationalTransfer)
                {
                    if (((InternationalPaymentOrder)order).Country == "51")
                    {
                        errors.Add(new ActionError(1974));
                    }
                }
                else
                {
                    errors.Add(new ActionError(1974));
                }
            }

            if (order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.Loan && string.IsNullOrEmpty(order.TransactionTypeByAML.AdditionalDescription))
            {//Տվյալ տեսակի գործարքի դեպքում պարտադիր է «Լրացուցիչ նկարագրություն» դաշտում լրացնել փոխառության պայմանագրի ամսաթիվը և կողմերին
                errors.Add(new ActionError(1975));
            }

            if (order.TransactionTypeByAML.TransactionType == (int)TransactionTypesByAML.Other && string.IsNullOrEmpty(order.TransactionTypeByAML.AdditionalDescription))
            {//Տվյալ տեսակի գործարքի դեպքում պարտադիր է «Լրացուցիչ նկարագրություն» դաշտում բացահայտել գործարքի բուն նպատակը և/կամ միջոցների ծագման աղբյուրը։
             //Օրինակ՝ «Կանխիկացում, հարսանեկան ծախսերի համար» կամ «Հաշվին մուծում, բիզնես գործունեության հասույթի մուտքագրում հաշվին»
                errors.Add(new ActionError(1976));
            }

            return errors;
        }
        public static TransactionTypeByAML GetTransactionTypeByAML(long doc_ID)
        {
            return TransactionTypeByAMLDB.GetTransactionTypeByAML(doc_ID);
        }

    }

}
