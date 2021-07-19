using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ServiceClient;

namespace ExternalBanking.ACBAServiceReference
{
    public partial class User
    {
        /// <summary>
        /// Վերադարձնում է օգտագործողի կանխիկի սահմանաչափը նշված արժույթով:
        /// </summary>
        /// <param name="currency">Արժույթ</param>
        /// <returns></returns>
        public decimal CashLimit(string currency)
        {
            return UserDB.CashLimit(this, currency);
        }

        /// <summary>
        /// Վերադարձնում է օգտագործողի դրամարկղի մնացորդը նշված արժույթով: 
        /// </summary>
        /// <param name="currency">Արժույթ</param>
        /// <returns></returns>
        public decimal CashRest(string currency)
        {
            return UserDB.CashRest(this, currency);
        }

        /// <summary>
        /// Վերադարձնում օգտագործողի դրամարկղի ապագա մնացորդը:
        /// </summary>
        /// <param name="currency">Արժույթ</param>
        /// <param name="amount">Գումար</param>
        /// <param name="direction">Գործարքի ուղղություն (մուտթ/ելք)</param>
        /// <returns></returns>
        public decimal FutureCashRest(string currency, decimal amount, string direction)
        {
            decimal result = 0;
            if (direction == "c")
            {
                result = CashRest(currency) - amount;
            }
            else if (direction == "d")
            {
                result = CashRest(currency) + amount;
            }

            return result;
        }

        public List<ActionError> CheckForNextCashOperation(Order order)
        {
            List<ActionError> result = new List<ActionError>();

            string currency = Validation.CashOperationCurrency(order);
            string direction = Validation.CashOperationDirection(order);

            decimal cashLimit = CashLimit(currency);
            decimal cashRest = CashRest(currency);
            if (cashRest > cashLimit)
            {
                ActionError err = new ActionError(621, new string[] { currency, cashRest.ToString("N2"), cashLimit.ToString("N2") });
                result.Add(err);
            }
            if (result.Count > 0)
            {
                result.ForEach(m => Localization.SetCulture(m, new Culture(Languages.hy)));
            }

            return result;
        }

        public ExternalBanking.ActionResult CheckForTransactionLimit(Order order)
        {
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Normal;

            if (TransactionLimit < 0)
                return result;

            if (order.GetType().Name == "PaymentOrder" || order.GetType().Name == "InternationalPaymentOrder"
                || order.GetType().Name == "FastTransferPaymentOrder" || order.GetType().Name == "TransitPaymentOrder"
                || order.GetType().Name == "UtilityPaymentOrder" || order.GetType().Name == "ReestrTransferOrder" || order.GetType().Name == "BudgetPaymentOrder")
            {
                if (TransactionLimit >= 0)
                {
                    double transactionAmount;

                    if (order.Currency == "AMD")
                    {
                        transactionAmount = order.Amount;
                    }
                    else
                    {
                        transactionAmount = order.Amount * Utility.GetLastCBExchangeRate(order.Currency);
                    }

                    if (TransactionLimit < transactionAmount)
                    {
                        result.ResultCode = ResultCode.ValidationError;
                        ActionError err = new ActionError(624);
                        result.Errors.Add(err);
                        Localization.SetCulture(result, new Culture(Languages.hy));
                    }
                }



            }
            else if (order.GetType().Name == "CurrencyExchangeOrder")
            {
                CurrencyExchangeOrder exchangeOrder = (CurrencyExchangeOrder)order;

                if (TransactionLimit < exchangeOrder.AmountInAmd)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    ActionError err = new ActionError(624);
                    result.Errors.Add(err);
                    Localization.SetCulture(result, new Culture(Languages.hy));
                }

            }

            return result;



        }

        public bool isOnlineAcc
        {
            get
            {
                if (userPermissionId == 505 || userPermissionId == 506 || userPermissionId == 88)
                    return true;
                else
                    return false;
            }
        }

        public bool isBranchAccountsDiv
        {
            get
            {
                if (userPermissionId == 35 || userPermissionId == 33 || userPermissionId == 34)
                    return true;
                else
                    return false;
            }
        }
        /// Ստուգում է, թղթակից ՊԿ-ն ակտիվ է, թե՝ ոչ
        /// </summary>
        /// <param name="new_id"></param>
        /// <returns></returns>
        public bool IsActiveUser(int new_id)
        {
            return UserDB.IsActiveUser(new_id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static bool CanSendToCashier(long UserID)
        {
            bool result;
            
            ClientPermissionsInfo clientPermissions = new ClientPermissionsInfo();
            clientPermissions.userID = (short)UserID;
            clientPermissions.pageName = "-1";
            clientPermissions.progName = "-1";
            clientPermissions.varPropertyName = "canSendToCashier";
            ClientPermissions permissions = LoginOperationsService.GetVarPermissionForPage(clientPermissions);

            return bool.TryParse(permissions.valueOfPermission, out result);
            
        }
    }
}
