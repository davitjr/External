using System;
using System.IO;
using System.Xml;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExternalBanking.ServiceClient;
using System.Net;
using System.Configuration;
using System.Web.Configuration;

namespace ExternalBanking
{
    public static class Utility
    {
        public static string ConvertAnsiToUnicode(string str)
        {

            if (str == null)
            {
                return str;
            }

            if (!HasAnsiCharacters(str))
            {
                return str;
            }

            string result = "";
            int strLen = str.Length;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                char uChar;
                if (charCode >= 178 && charCode <= 253)
                {
                    if (charCode % 2 == 0)
                    {
                        uChar = (char)((charCode - 178) / 2 + 1329);
                    }
                    else
                    {
                        uChar = (char)((charCode - 179) / 2 + 1377);
                    }
                }
                else
                {
                    if (charCode >= 32 && charCode <= 126)
                    {
                        uChar = str[i];
                    }
                    else
                    {
                        switch (charCode)
                        {
                            case 162:
                                uChar = (char)1415;
                                break;
                            case 168:
                                uChar = (char)1415;
                                break;
                            case 176:
                                uChar = (char)1371;
                                break;
                            case 175:
                                uChar = (char)1372;
                                break;
                            case 177:
                                uChar = (char)1374;
                                break;
                            case 170:
                                uChar = (char)1373;
                                break;
                            case 173:
                                uChar = '-';
                                break;
                            case 163:
                                uChar = (char)1417;
                                break;
                            case 169:
                                uChar = '.';
                                break;
                            case 166:
                                uChar = '»'; //187
                                break;
                            case 167:
                                uChar = '«'; //171
                                break;
                            case 164:
                                uChar = ')';
                                break;
                            case 165:
                                uChar = '(';
                                break;
                            case 46:
                                uChar = '.';
                                break;
                            default:
                                uChar = str[i];
                                break;
                        }
                    }
                }
                result += uChar;
            }
            return result;
        }

        public static string ConvertAnsiToUnicodeRussian(string str)
        {
            string result = UtilityDB.ConvertAnsiToUnicodeRussianDB(str);
            return result;
        }

        public static string ConvertUnicodeToAnsiRussian(string str)
        {
            string result = UtilityDB.ConvertUnicodeToAnsiRussianDB(str);
            return result;
        }

        public static string Serialize<T>(this T thisT)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(thisT.GetType());
            using (var writer = new StringWriter())
            using (var stm = new XmlTextWriter(writer))
            {
                serializer.WriteObject(stm, thisT);
                return writer.ToString();
            }
        }

        public static bool Equals(dynamic ob1, dynamic ob2)
        {
            if (ob1 == null && ob2 == null)
                return true;
            else if ((ob1 == null & ob2 != null) || (ob2 == null & ob1 != null))
                return false;

            string str1 = Utility.Serialize(ob1);
            string str2 = Utility.Serialize(ob2);
            return str1 == str2;
        }
        /// <summary>
        /// Ստուգում է արդյոք հաշիվը համապատասխանում հաշվի և պրոդուկտի տեսակին
        /// </summary>
        /// <param name="accountNumber">Հաշվի համար</param>
        /// <param name="typeOfAccount">Հաշվի տեսակա</param>
        /// <param name="typeOfProduct">Պրոդուկտր տեսակ</param>
        /// <returns>true- Եթե հաշիվը համապատասխանում է հաշվի տեսակին և պրոդուկտին, false` հակառակ դեպքում</returns>
        public static bool IsCorrectAccount(string accountNumber, short typeOfAccount, short typeOfProduct)
        {
            bool result = false;
            double accountNumberD = 0;

            double.TryParse(accountNumber, out accountNumberD);
            if (accountNumberD == 0)
            {
                return result;
            }

            result = UtilityDB.IsCorrectAccount(accountNumber, typeOfAccount, typeOfProduct);
            return result;
        }

        /// <summary>
        /// Ստուգում է նշված արժույթով գումարի համար, ստորակետից հետո նիշերի ճիշտ լինելը:
        /// </summary>
        /// <param name="amount">Գումար</param>
        /// <param name="currency">Արժույթ</param>
        /// <returns>true - եթե ստորակետից հետո նիշերի քանակը ճիշտ է, false` հակառակ դեպքում:</returns>
        public static bool IsCorrectAmount(double amount, string currency)
        {
            bool result = false;
            byte decimalsCount;
            decimal amountToCheck = Convert.ToDecimal(amount);

            if (currency == "AMD")
            {
                decimalsCount = 1;
            }
            else
            {
                decimalsCount = 2;
            }

            decimal multiplier = Convert.ToDecimal(Math.Pow(10, decimalsCount));

            if (Math.Truncate(amountToCheck * multiplier) == amountToCheck * multiplier)
            {
                result = true;
            }

            return result;
        }


        /// <summary>
        /// Հաշվակում նշված թվային արտահայտության համար հսկիչ թիվը:
        /// </summary>
        /// <param name="number">Թվային արտահայտություն</param>
        /// <returns>Վերադարձնում է հսկից թվանշանը</returns>
        public static byte ControlDigit(string number)
        {
            int cum = 0;
            bool factor = true;
            for (int i = number.ToString().Length - 1; i >= 0; i--)
            {
                short b = short.Parse(number.ToString()[i].ToString());

                b = (short)(b * (factor == true ? 2 : 1));

                if (b > 9)
                {
                    cum += 1 + b - 10;
                }
                else
                {
                    cum += b;
                }
                factor = !factor;
            }

            return (byte)(Math.Ceiling((double)cum / 10) * 10 - cum);
        }

        /// <summary>
        /// Ստուգում է, արդյոք նշված ՀՀ հաշվի համարի հսկիչ նիշը (12-րդ նիշը) -ճիշտ է:
        /// </summary>
        /// <param name="number">Հաշվի համարի առաջին 12 նիշերը:</param>
        /// <returns>true- եթե հսկիչ նիշը ճիշտ է, false` հակառակ դեպքում</returns>
        public static bool CheckAccountNumberControlDigit(string number)
        {
            bool result = false;

            if (byte.Parse(number.Substring(11)) == ControlDigit(number.Substring(0, 11)))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Ստուգում է արդյոք գործարքի գումարը գերազանցում է գործարքի թույլատելի սահմանաչափերը:
        /// </summary>
        /// <param name="operationAmount">Գործարքի գումար</param>
        /// <param name="currency">Արժույթ</param>
        /// <param name="oneOpeartionAmountLimit">Մեկ գործարքի սահմանաչափ</param>
        /// <returns>true-եթե սահմանաչափը չի գերազանցում,false` եթե գերազանցում է</returns>
        public static ActionResult ValidateOpeartionLimits(double operationAmount, string currency, double oneOpeartionAmountLimit, bool? isAttachedCard = false)
        {
            ActionResult actionResult = new ActionResult()
            {
                ResultCode = ResultCode.Normal
            };
            double attachedCardOpeartionAmountLimit = GetAttachedCardOperationLimit();
            double exchangeRate = GetLastCBExchangeRate(currency);
            double currentOperationAmount = CalculateCurrentOperationAmount(operationAmount, exchangeRate);
            if (isAttachedCard == true && CheckOperationLimit(currentOperationAmount, attachedCardOpeartionAmountLimit) == false)
            {
                actionResult.Errors.Add(new ActionError(1899));
            }
            if (CheckOperationLimit(currentOperationAmount, oneOpeartionAmountLimit) == false)
            {
                actionResult.Errors.Add(new ActionError(66));
            }
            if (actionResult.Errors.Count > 0)
            {
                actionResult.ResultCode = ResultCode.ValidationError;
            }
            return actionResult;
        }
        /// <summary>
        /// Ստուգում է արդյոք գործարքի գումարը գերազանցում է գործարքի թույլատելի սահմանաչափերը:
        /// </summary>
        /// <param name="operationAmount">Գործարքի գումար</param>
        /// <param name="currency">Արժույթ</param>
        /// <param name="oneOpeartionAmountLimit">Մեկ գործարքի սահմանաչափ</param>
        /// <returns>true-եթե սահմանաչափը չի գերազանցում,false` եթե գերազանցում է</returns>
        public static bool ValidateAttachedCardOpeartionLimit(double operationAmount, string currency)
        {
            double attachedCardOpeartionAmountLimit = GetAttachedCardOperationLimit();
            double exchangeRate = GetLastCBExchangeRate(currency);
            double currentOperationAmount = CalculateCurrentOperationAmount(operationAmount, exchangeRate);
            bool isValidLimit = CheckOperationLimit(currentOperationAmount, attachedCardOpeartionAmountLimit);
            return isValidLimit;
        }
        /// <summary>
        /// Ստուգում է արդյոք գործարքի գումարը գերազանցում է մեկ գործարքի թույլատելի սահմանաչափը:
        /// </summary>
        /// <param name="operationAmount">Գործարքի գումար</param>
        /// <param name="currency">Արժույթ</param>
        /// <param name="oneOpeartionAmountLimit">Մեկ գործարքի սահմանաչափ</param>
        /// <returns>true-եթե սահմանաչափը չի գերազանցում,false` եթե գերազանցում է</returns>
        public static bool CheckOperationLimit(double operationAmount, string currency, double oneOpeartionAmountLimit)
        {
            double exchangeRate = GetLastCBExchangeRate(currency);
            double currentOperationAmount = CalculateCurrentOperationAmount(operationAmount, exchangeRate);
            bool isValidLimit = CheckOperationLimit(currentOperationAmount, oneOpeartionAmountLimit);
            return isValidLimit;
        }

        /// <summary>
        /// Ստուգում է արդյոք գործարքի գումարը գերազանցում է մեկ գործարքի թույլատելի սահմանաչափը:
        /// </summary>
        /// <param name="currentOperationAmount">Գործարքի գումար</param>
        /// <param name="oneOpeartionAmountLimit">Մեկ գործարքի սահմանաչափ</param>
        /// <returns>true-եթե սահմանաչափը չի գերազանցում,false` եթե գերազանցում է</returns>
        private static bool CheckOperationLimit(double currentOperationAmount, double oneOpeartionAmountLimit)
        {
            bool isValidLimit = false;
            if (currentOperationAmount <= oneOpeartionAmountLimit)
            {
                isValidLimit = true;
            }
            return isValidLimit;
        }
        private static double CalculateCurrentOperationAmount(double operationAmount, double exchangeRate)
        {
            return operationAmount * exchangeRate;
        }
        private static double GetAttachedCardOperationLimit()
        {
            const double attachedCardOperationLimit = 400000;
            return attachedCardOperationLimit;
        }

        /// <summary>
        /// Վերադարձնում է համապատասխան տեսակի վերջին փոխարժեքը
        /// </summary>
        /// <param name="currency">Արժույթ</param>
        /// <param name="rateType">Փոխարժեքի տեսակ</param>
        /// <param name="direction">Փոխանակման ուղղություն (գնում, վաճառք)</param>
        /// <returns>Փոխարժեք</returns>
        public static double GetLastExchangeRate(string currency, RateType rateType, ExchangeDirection direction)
        {
            return UtilityDB.GetLastExchangeRate(currency, rateType, direction);
        }
        public static double GetLastExchangeRate(string currency, RateType rateType, ExchangeDirection direction, ushort filialCode)
        {
            return UtilityDB.GetLastExchangeRate(currency, rateType, direction, filialCode);
        }

        /// <summary>
        /// Վերադարձնում ՀՀ ԿԲ վերջին փոխարժեքը:
        /// </summary>
        /// <param name="currency">Արժույո</param>
        /// <returns>ՀՀ ԿԲ վերջին փոխարժեք</returns>
        public static double GetLastCBExchangeRate(string currency)
        {
            return UtilityDB.GetLastCbExchangeRate(currency);
        }

        /// <summary>
        /// Վերադարձնում է true եթե նշված տողը պարունակում է անթույլատրելի սիմվոլներ:
        /// </summary>
        /// <param name="str">Տող, որը անհրաժեշտ է ստուգել</param>
        /// <returns></returns>
        public static bool IsExistForbiddenCharacter(string str)
        {

            foreach (int code in Constants.FORBIDDEN_CHARACTERS)
            {
                if (str.Contains(((char)code).ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Վերադարձնում է նշված տողում առկա Swift-ի անթույլատրելի սիմվոլը:
        /// </summary>
        /// <param name="str">Տող, որը անհրաժեշտ է ստուգել</param>
        ///  <param name="excludeApostrophe">Կարող է տողը պարունակել չակերտներ թե ոչ</param>
        /// <returns></returns>
        public static string IsExistSwiftForbiddenCharacter(string str = "", short excludeApostrophe = 0)
        {

            foreach (int code in Constants.SWIFT_FORBIDDEN_CHARACTERS)
            {
                if (str.Contains(((char)code).ToString()))
                {
                    return ((char)code).ToString();
                }
            }
            if (excludeApostrophe == 0)
            {
                if (str.Contains("'"))
                {
                    return "'";
                }
                if (str.Contains("\"\""))
                {
                    return "\"\"";
                }
            }

            return "";
        }

        /// <summary>
        /// Վերադարձնում է արժույթի կոդը
        /// </summary>
        public static string GetCurrencyCode(string currency)
        {
            return UtilityDB.GetCurrencyCode(currency);
        }

        public static double RoundAmount(double amount, string currency, SourceType source = SourceType.Bank)
        {

            if (currency == "AMD")
            {
                amount = Math.Round(amount, 1, MidpointRounding.AwayFromZero);
            }
            else
            {
                if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking || source == SourceType.AcbaOnlineXML || source == SourceType.ArmSoft)
                {
                    amount = Convert.ToDouble(Math.Round(Convert.ToDecimal(amount), 2, MidpointRounding.AwayFromZero));
                }
                else
                {
                    amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
                }
            }

            return amount;
        }
        /// <summary>
        /// Աշխատանքային և ոչ աշխատանքային օր
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsWorkingDay(DateTime date)
        {
            return UtilityDB.IsWorkingDay(date);
        }

        public static DateTime GetNextOperDay()
        {
            return UtilityDB.GetNextOperDay().Date;
        }



        /// <summary>
        /// Վերադարձնում է գործառնական օրը:
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCurrentOperDay()
        {
            return UtilityDB.GetCurrentOperDay().Date;
        }

        /// <summary>
        /// Փոխարկման դեպքում գումարից և արժույթից կախված արժույթի դաժտը փոփոխելու հնարավորության թույլատրում
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static bool ManuallyRateChangingAccess(double amount, string currency, string currencyConvertation, SourceType sourceType)
        {
            bool changeAccess = false;

            if (sourceType == SourceType.Bank)
                return true;

            if (currencyConvertation != "AMD")
                return changeAccess;
            else
            {
                if (currency == "USD")
                    if (amount >= 5000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else if (currency == "EUR")
                    if (amount >= 5000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else if (currency == "RUR")
                    if (amount >= 50000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else if (currency == "GBP")
                    if (amount == 1000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else if (currency == "CHF")
                    if (amount >= 1000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else if (currency == "GEL")
                    if (amount >= 1000)
                        changeAccess = true;
                    else
                        changeAccess = false;
                else
                    changeAccess = true;

                return changeAccess;
            }
        }

        public static string GetUserFullName(long UserId)
        {
            return UtilityDB.GetUserFullName(UserId);
        }

        public static double GetCBKursForDate(DateTime date, string currency)
        {
            return UtilityDB.GetCBKursForDate(date, currency);
        }
        /// <summary>
        /// Unicode-ի ստուգում
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int IsTextUnicode(string text)
        {
            return UtilityDB.IsTextUnicode(text);
        }

        /// <summary>
        /// Քարտի պրոդուկտի տեսակ 
        /// </summary>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public static int GetCardProductType(uint cardType)
        {
            return UtilityDB.GetCardProductType(cardType);
        }


        /// <summary>
        /// Ստեղծել հերթական համար
        /// </summary>
        public static ulong GetLastKeyNumber(int keyId, ushort FilialCode)
        {
            return UtilityDB.GetLastKeyNumber(keyId, FilialCode);
        }

        public static ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType, ushort FilialCode)
        {
            return UtilityDB.GenerateNewOrderNumber(orderNumberType, FilialCode);
        }

        public static double GetPriceInfoByIndex(int index, string fieldName)
        {
            return UtilityDB.GetPriceInfoByIndex(index, fieldName);
        }
        public static string ConvertUnicodeToAnsi(string str)
        {
            string result = "";
            if (str == null)
            {
                return result;
            }

            foreach (char c in str)
            {
                int charCode = (int)c;
                char asciiChar;
                if (charCode >= 1329 && charCode <= 1329 + 37)
                {
                    asciiChar = (char)((charCode - 1240) * 2);
                }
                else if (charCode >= 1329 + 48 && charCode <= 1329 + 48 + 37)
                {
                    asciiChar = (char)((charCode - 1288) * 2 + 1);
                }
                else if (char.IsNumber(c))
                {
                    asciiChar = c;
                }
                else if (charCode == 1415) // և
                {
                    asciiChar = (char)168;
                }
                else if (charCode == 1371) // Շեշտ
                {
                    asciiChar = (char)176;
                }
                else if (charCode == 1372) // Բացականչական
                {
                    asciiChar = (char)175;
                }
                else if (charCode == 1374) // Հարցական
                {
                    asciiChar = (char)177;
                }
                else if (charCode == 1373) // Բութ
                {
                    asciiChar = (char)170;
                }
                else if (charCode == 58) // Վերջակետ
                {
                    asciiChar = (char)163;
                }
                else if (charCode == 32) // Բացատ
                {
                    asciiChar = (char)32;
                }
                else if (c == ')')
                {
                    asciiChar = (char)164;
                }
                else if (c == '(')
                {
                    asciiChar = (char)165;
                }
                else
                {
                    asciiChar = c;
                }

                result += asciiChar;
            }

            return result;
        }

        public static string GenerateAddress(CustomerAddress address)
        {
            string result = "";

            if (address.address.Region != null && address.address.Region.value != "")
            {
                result = address.address.Region.value;
            }

            if (address.address.TownVillage != null && address.address.TownVillage.value != "")
            {
                result += ", " + address.address.TownVillage.value;
            }

            if (address.address.Street != null && address.address.Street.value != "")
            {
                result += "," + address.address.Street.value;
            }

            //if (address.address.Building != null && address.address.Building.value != "")
            //{
            //    result +=", "+ address.address.Building.value;
            //}

            if (address.address.House != null && address.address.House != "")
            {
                result += ", " + address.address.House;
            }

            if (address.address.Appartment != null && address.address.Appartment != "")
            {
                result += ", " + address.address.Appartment;
            }

            if (result[0] == ',')
            {
                result = result.Remove(0, 2);
            }
            return result;

        }

        /// <summary>
        /// Գործարքի ներբանկային հաշվի տեսակ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderAccountType"></param>
        /// <returns></returns>
        public static ushort GetOperationSystemAccountType(Order order, OrderAccountType orderAccountType)
        {
            OrderType orderType = order.Type;
            ushort accountType = 0;

            //Կանխիկ մուտք հաշվին,ելք հաշվից
            if (((orderType == OrderType.CashDebit || orderType == OrderType.CashDebitConvertation || orderType == OrderType.ReestrTransferOrder || orderType == OrderType.CashDepositCasePenaltyMatureOrder
                || orderType == OrderType.CashInsuranceOrder || orderType == OrderType.ReestrCashCommunalPayment) && orderAccountType == OrderAccountType.DebitAccount)
                || ((orderType == OrderType.CashCredit || orderType == OrderType.CashCreditConvertation || orderType == OrderType.CashOutFromTransitAccountsOrder) && orderAccountType == OrderAccountType.CreditAccount)
                || orderType == OrderType.CashConvertation || orderType == OrderType.TransitCashOutCurrencyExchangeOrder || orderType == OrderType.TransitCashOut || orderType == OrderType.ReceivedFastTransferPaymentOrder
                || ((orderType == OrderType.CashCredit || orderType == OrderType.CashDebit || orderType == OrderType.ChequeBookOrder || orderType == OrderType.ChequeBookReceiveOrder
                || orderType == OrderType.ReferenceOrder || orderType == OrderType.SwiftCopyOrder || orderType == OrderType.DepositCaseActivationOrder 
                || orderType == OrderType.CashDebitConvertation
                || orderType == OrderType.CashTransitCurrencyExchangeOrder) && orderAccountType == OrderAccountType.FeeAccount)
                || orderType == OrderType.CashOutFromTransitAccountsOrder)
            {
                accountType = 1; //Դրամարկղի հաշիվ
            }
            else if (orderType == OrderType.TransitPaymentOrder) // Տարանցիկ հաշվին մուտք
            {

                if (orderAccountType == OrderAccountType.CreditAccount)
                {
                    TransitPaymentOrder transitPaymentOrder = (TransitPaymentOrder)order;
                    accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(transitPaymentOrder.TransitAccountType);

                }
                else if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1; //Դրամարկղի հաշիվ
                }

            }
            else if (orderType == OrderType.CashTransitCurrencyExchangeOrder) // Տարանցիկ հաշվին մուտք կոնվերտացիայով
            {
                if (orderAccountType == OrderAccountType.CreditAccount)
                {
                    TransitCurrencyExchangeOrder transitCurrencyExchangeOrder = (TransitCurrencyExchangeOrder)order;
                    accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType((TransitAccountTypes)transitCurrencyExchangeOrder.AccountType);
                }
                else if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1; //Դրամարկղի հաշիվ
                }

            }
            else if (orderType == OrderType.CashForRATransfer && orderAccountType == OrderAccountType.DebitAccount)
            {
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForArmTransfer);
            }
            else if (orderType == OrderType.CashCommunalPayment || orderType == OrderType.CommunalPayment
                || orderType == OrderType.ReestrCashCommunalPayment || orderType == OrderType.ReestrCommunalPayment)
            {
                if ((orderType == OrderType.CashCommunalPayment || orderType == OrderType.ReestrCashCommunalPayment) && orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1; //Դրամարկղի հաշիվ
                }
                else if (orderAccountType == OrderAccountType.CreditAccount)
                {
                    UtilityPaymentOrder utilityPaymentOrder = (UtilityPaymentOrder)order;

                    switch (utilityPaymentOrder.CommunalType)
                    {
                        case CommunalTypes.ENA:
                            accountType = 40;
                            break;
                        case CommunalTypes.Gas:
                            accountType = 41;
                            break;
                        case CommunalTypes.ArmWater:
                            accountType = 42;
                            break;
                        case CommunalTypes.YerWater:
                            accountType = 43;
                            break;
                        case CommunalTypes.ArmenTel:
                        case CommunalTypes.BeelineInternet:
                            accountType = 44;
                            break;
                        case CommunalTypes.VivaCell:
                            accountType = 45;
                            break;
                        case CommunalTypes.Orange:
                            accountType = 47;
                            break;
                        case CommunalTypes.UCom:
                            accountType = 48;
                            break;
                        case CommunalTypes.Trash:
                            accountType = 46;
                            break;
                        case CommunalTypes.COWater:
                            accountType = 3017;
                            break;
                    }

                }
            }
            else if (orderType == OrderType.CashPosPayment)    //Կանխիկացում POS տերմինալով ՝ POS հաշիվ
            {
                if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1001;
                }
                else
                {
                    accountType = 1;
                }
            }
            else if (orderType == OrderType.HBActivation && orderAccountType == OrderAccountType.CreditAccount)
            {
                accountType = 6004;
            }
            else if (orderType == OrderType.CashFeeForServiceProvided)
            {
                if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1;
                }
            }
            else if (orderType == OrderType.CashInternationalTransfer)
            {

                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForArmTransfer);

            }

            else if (orderType == OrderType.FastTransferPaymentOrder)
            {

                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForArmTransfer);

            }
            else if (orderType == OrderType.CurrentAccountReOpen)
            {
                if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = 1;
                }
                else
                {
                    // TO DO
                }
            }
            else if (orderType == OrderType.LoanMature || orderType == OrderType.OverdraftRepayment || orderType == OrderType.CardCreditLineRepayment)
            {
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForProblemLoans);
            }
            else if ((orderType == OrderType.AccountServicePaymentXnd || orderType == OrderType.HBServicePaymentXnd
                || orderType == OrderType.HBServicePayment || orderType == OrderType.AccountServicePayment
                || orderType == OrderType.GuaranteeActivation || orderType == OrderType.PaidGuaranteeActivation || orderType == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount) && orderAccountType == OrderAccountType.DebitAccount)
            {
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForProblemLoans);
            }
            else if (orderType == OrderType.HBActivation && orderAccountType == OrderAccountType.DebitAccount)
            {
                accountType = 8001;
            }
            else if (orderType == OrderType.InterBankTransferCash && orderAccountType != OrderAccountType.FeeAccount)
            {
                if (orderAccountType == OrderAccountType.DebitAccount)
                {
                    accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForArmTransfer);
                }
                else
                {
                    accountType = 3015;
                }
            }
            else if (orderType == OrderType.InterBankTransferNonCash && orderAccountType == OrderAccountType.CreditAccount)
            {
                accountType = 3015;
            }
            else if ((orderType == OrderType.DepositCasePenaltyMatureOrder || orderType == OrderType.CashDepositCasePenaltyMatureOrder) && orderAccountType == OrderAccountType.CreditAccount)
            {
                accountType = 1894;
            }
            else if (orderType == OrderType.InterBankTransferCash && orderAccountType == OrderAccountType.FeeAccount)
            {
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForArmTransfer);
            }
            else if (orderType == OrderType.BondAmountChargeOrder && orderAccountType == OrderAccountType.DebitAccount)     //  Պարտատոմսի գումարի գանձման հայտ
            {
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(TransitAccountTypes.ForBond);
            }
            else if (orderType == OrderType.NonCashTransitPaymentOrder && orderAccountType == OrderAccountType.CreditAccount)     //  Պարտատոմսի գումարի գանձման հայտ
            {
                TransitPaymentOrder transitPaymentOrder = (TransitPaymentOrder)order;
                accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType(transitPaymentOrder.TransitAccountType);
            }
            else if (orderType == OrderType.NonCashTransitCurrencyExchangeOrder && orderAccountType == OrderAccountType.CreditAccount)     //  Պարտատոմսի գումարի գանձման հայտ
            {
                TransitCurrencyExchangeOrder transitPaymentOrder = (TransitCurrencyExchangeOrder)order;
                if (transitPaymentOrder.AccountType == (short)TransitAccountTypes.ForBond)
                {
                    accountType = TransitPaymentOrder.GetTransitPaymentOrderSystemAccountType((TransitAccountTypes)transitPaymentOrder.AccountType);
                }
            }

            return accountType;
        }

        /// <summary>
        /// Գործարքի միջնորդավճարի ներբանկային հաշվի տեսակ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="feeType"></param>
        /// <returns></returns>
        public static ushort GetOperationFeeSystemAccountType(Order order, short feeType)
        {
            OrderType orderType = order.Type;
            ushort accountType = 0;



            return accountType;
        }


        public static bool HasAnsiCharacters(string str)
        {
            bool hasAnsi = false;
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int charCode = (int)str[i];
                if (charCode >= 178 && charCode <= 253 && charCode != 187)
                {
                    hasAnsi = true;
                }
            }

            return hasAnsi;
        }


        public static bool GetBankStatus(int bankCode)
        {
            return UtilityDB.GetBankStatus(bankCode);
        }

        public static string GetCustomerDescription(ulong customerNumber)
        {
            string customerDescription = "";
            customerDescription = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(customerNumber));
            return customerDescription;
        }

        public static bool CanPayTransferByCall(short transferSystem)
        {
            return UtilityDB.CanPayTransferByCall(transferSystem);
        }

        /// <summary>
        /// Վերադարձնում է արտարժույթի դեպքում արտարժույթի նվազագույն անվանական արժեքը
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static double GetCurrencyMinCashAmount(string currency)
        {
            return UtilityDB.GetCurrencyMinCashAmount(currency);
        }


        public static bool IsLatinLetter(string text)
        {
            return UtilityDB.IsLatinLetter(text);
        }


        /// <summary>
        /// Երկու ամսաթվերի ամիսների տարբերություն
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static int GetMonthsBetween(DateTime from, DateTime to)
        {
            if (from > to) return GetMonthsBetween(to, from);

            var monthDiff = Math.Abs((to.Year * 12 + (to.Month - 1)) - (from.Year * 12 + (from.Month - 1)));

            if (from.AddMonths(monthDiff) > to || to.Day < from.Day)
            {
                return monthDiff - 1;
            }
            else if (to.Day > from.Day)
            {
                return monthDiff + 1;
            }
            else
            {
                return monthDiff;
            }
        }

        /// <summary>
        /// Վերադարձնում է պրոդուկտի տեսակը վարկի տեսակից կախված
        /// </summary>
        /// <param name="loanType"></param>
        /// <returns></returns>
        public static short GetProductTypeFromLoanType(short loanType)
        {
            return UtilityDB.GetProductTypeFromLoanType(loanType);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ օրվա փոխարժեքը
        /// </summary>
        /// <param name="date"></param>
        /// <param name="currency"></param>
        /// <param name="operationType"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static double GetKursForDate(DateTime date, string currency, ushort operationType, ushort filialCode = 22000)
        {
            return UtilityDB.GetKursForDate(date, currency, operationType, filialCode);
        }

        public static DateTime GetFastOverdrafEndDate(DateTime startDate)
        {
            int month = (int)GetPriceInfoByIndex(848, "Price");
            DateTime endDate = startDate.AddMonths(month);
            while (IsWorkingDay(endDate) != true)
            {
                endDate = endDate.AddDays(1);
            }

            return endDate;
        }



        public static string TranslateToEnglish(string armString, bool unicode = false)
        {
            string englishString = "";
            bool ret = false;
            string currentStr = "";
            int lenStr = 0;

            string[] ArmAlphabet = { "²", "³", "´", "µ", "¶", "·", "¸", "¹", "º", "»", "¼", "½", "¾", "¿", "À", "Á", "Â", "Ã", "Ä", "Å", "Æ", "Ç", "È", "É", "Ê", "Ë", "Ì", "Í", "Î", "Ï", "Ð", "Ñ", "Ò", "Ó", "Ô", "Õ", "Ö", "×", "Ø", "Ù", "Ú", "Û", "Ü", "Ý", "Þ", "ß", "à", "á", "â", "ã", "ä", "å", "æ", "ç", "è", "é", "ê", "ë", "ì", "í", "î", "ï", "ð", "ñ", "ò", "ó", "ô", "õ", "ö", "÷", "ø", "ù", "ú", "û", "ü", "ý", "àõ", "áõ", "à", "á", "¨" };
            string[] ArmAlphabetUnicode = { "Ա", "ա", "Բ", "բ", "Գ", "գ", "Դ", "դ", "Ե", "ե", "Զ", "զ", "Է", "է", "Ը", "ը", "Թ", "թ", "Ժ", "ժ", "Ի", "ի", "Լ", "լ", "Խ", "խ", "Ծ", "ծ", "Կ", "կ", "Հ", "հ", "Ձ", "ձ", "Ղ", "ղ", "Ճ", "ճ", "Մ", "մ", "Յ", "յ", "Ն", "ն", "Շ", "շ", "Ո", "ո", "Չ", "չ", "Պ", "պ", "Ջ", "ջ", "Ռ", "ռ", "Ս", "ս", "Վ", "վ", "Տ", "տ", "Ր", "ր", "Ց", "ց", "Ւ", "ւ", "Փ", "փ", "Ք", "ք", "Օ", "օ", "Ֆ", "ֆ", "ՈՒ", "ու", "Ո", "ո", "և" };

            if (unicode == true)
            {
                ArmAlphabet = ArmAlphabetUnicode;
            }

            string[] EnglishAlphabet = { "A", "a", "B", "b", "G", "g", "D", "d", "E", "e", "Z", "z", "E", "e", "Y", "y", "T", "t", "Zh", "zh", "I", "i", "L", "l", "Kh", "kh", "Ts", "ts", "K", "k", "H", "h", "Dz", "dz", "Gh", "gh", "Tch", "tch", "M", "m", "Y", "y", "N", "n", "Sh", "sh", "O", "o", "Ch", "ch", "P", "p", "Dj", "dj", "Rr", "rr", "S", "s", "V", "v", "T", "t", "R", "r", "Tc", "tc", "U", "u", "Ph", "ph", "Q", "q", "O", "o", "F", "f", "U", "u", "Vo", "vo", "yev" };

            lenStr = armString.Length;



            for (int m = 0; m < armString.Length; m++)
            {
                ret = false;
                currentStr = armString.Substring(m, 1);

                for (int i = 0; i <= 80; i++)
                {
                    if (string.CompareOrdinal(currentStr, ArmAlphabet[i]) == 0)
                    {
                        ret = true;
                        if (m == lenStr)
                        {
                            englishString = englishString + EnglishAlphabet[i];
                        }
                        else if (m == 0 && armString.Substring(0, 1) == "à" && armString.Substring(0, 1) == "õ" && armString.Substring(0, 1) == "ô")
                        {
                            englishString = englishString + EnglishAlphabet[78];
                        }
                        else if (m == 0 && armString.Substring(0, 1) == "á" && armString.Substring(0, 1) == "õ")
                        {
                            englishString = englishString + EnglishAlphabet[79];
                        }
                        else if (m == 0 && armString.Substring(0, 1) == "º")
                        {
                            englishString = englishString + "Ye";
                        }
                        else if (m == 0 && armString.Substring(0, 1) == "»")
                        {
                            englishString = englishString + "ye";
                        }
                        else if (m + 1 < lenStr && (armString.Substring(m + 1, 1) == "õ" || armString.Substring(m + 1, 1) == "ô") && currentStr == "á")
                        {
                            m = m + 1;
                            englishString = englishString + EnglishAlphabet[77];
                        }
                        else if (m + 1 < lenStr && (armString.Substring(m + 1, 1) == "õ" || armString.Substring(m + 1, 1) == "ô") && currentStr == "à")
                        {
                            m = m + 1;
                            englishString = englishString + EnglishAlphabet[76];
                        }
                        else if (currentStr == "Õ")
                        {
                            englishString = englishString + "gh";
                        }
                        else if (currentStr == "Ô")
                        {
                            englishString = englishString + "Gh";
                        }
                        else
                        {
                            englishString = englishString + EnglishAlphabet[i];
                        }

                        break;
                    }
                }

                if (ret == false)
                {
                    englishString = englishString + currentStr;
                }
            }



            englishString = englishString.Trim();

            return englishString;
        }

        public static double GetLastCrossExchangeRate(string dCur, string cCur, ushort filialCode = 22000)
        {
            var crossVariant = CurrencyExchangeOrder.GetCrossConvertationVariant(dCur, cCur);
            var dRate = GetLastExchangeRate(dCur, RateType.Cross, ExchangeDirection.Buy, filialCode);
            var cRate = GetLastExchangeRate(cCur, RateType.Cross, ExchangeDirection.Sell, filialCode);
            Double rate = 0;
            if (crossVariant == 1)
            {
                rate = (float)dRate / cRate;
            }
            else
            {
                rate = (float)cRate / dRate;
            }
            rate = Math.Round((rate + 0.00000001) * 10000000) / 10000000;
            return rate;
        }
        public static ActionResult SendSMS(string phoneNumber, string messageText, int messageTypeID, int registrationSetNumber, SourceType sourceType)
        {
            return UtilityDB.SendSMS(phoneNumber, messageText, messageTypeID, registrationSetNumber, sourceType);
        }


        public static int GetLastKeyNumber(int keyID)
        {
            return UtilityDB.GetLastKeyNumber(keyID);
        }

        public static bool validate24_7_mode(OperDayMode operDay)
        {
            return UtilityDB.validate24_7_mode(operDay);
        }

        /// <summary>
        /// Ստուգում է՝ արդյոք տրված տեքստում կան անթույլատրելի սիմվոլներ, թե ոչ։
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CheckTextForUnpermittedSymbols(string text)
        {
            string result = "";
            if (text.IndexOf((char)13) != -1)
            {
                result += " (Enter)";
            }

            if (text.IndexOf((char)10) != -1)
            {
                result += " (Enter)";
            }

            if (text.IndexOf("|") != -1)
            {
                result += " (|)";
            }

            if (text.IndexOf(":") != -1)
            {
                result += " (:)";
            }

            if (text.IndexOf("։") != -1)
            {
                result += " (։)";
            }

            if (text.IndexOf("՛") != -1)
            {
                result += " (՛)";
            }

            if (text.IndexOf("'") != -1)
            {
                result += " (')";
            }
            if (text.IndexOf((char)9) != -1)
            {
                result += " (Tab)";
            }

            if (text.IndexOf((char)177) != -1)
            {
                result += " (±)";
            }

            if (text.IndexOf((char)9) != -1)
            {
                result += " (" + (char)9 + ")";
            }

            if (text.IndexOf("＇") != -1)
            {
                result += " (＇)";
            }
            if (text.IndexOf((char)176) != -1)
            {
                result += " (" + (char)176 + ")";
            }
            return result;
        }
        public static OrderType GetDocumentType(int docId)
        {
            return UtilityDB.GetDocumentType(docId);
        }

        public static int GetProductTypeByAppId(ulong appId)
        {
            return UtilityDB.GetProductTypeByAppId(appId);

        }

        public static bool ValidateProductId(ulong customerNumber, ulong productId, ProductType productType)
        {
            return UtilityDB.ValidateProductId(customerNumber, productId, productType);
        }

        public static bool ValidateDocId(ulong customerNumber, long docId)
        {
            return UtilityDB.ValidateDocId(customerNumber, docId);
        }
        public static bool ValidateAccountNumber(ulong customerNumber, string accountNumber)
        {
            return UtilityDB.ValidateAccountNumber(customerNumber, accountNumber);
        }
        public static bool ValidateCardNumber(ulong customerNumber, string cardNumber)
        {
            return UtilityDB.ValidateCardNumber(customerNumber, cardNumber);
        }


        public static bool IsCardAccount(string AccountNumber)
        {
            return AccountDB.IsCardAccount(AccountNumber);
        }


        internal static string DoPostRequestJson(string jsonObject, string url, string baseAddressName, List<KeyValuePair<string, string>> paramList = null)
        {

            string json = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WebConfigurationManager.AppSettings[baseAddressName].ToString() + url);

            request.Method = "POST";
            request.ContentType = "application/json; charset=UTF-8";

            request.Headers.Add("SessionId", "ba0f312d-8487-445e-aee2-d5877ac1d4de");
            string language = paramList?.Find(m => m.Key == "language").Value;
            if (paramList != null)
            {
                foreach (KeyValuePair<string, string> param in paramList)
                {
                    request.Headers.Add(param.Key, param.Value);
                }
            }

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(jsonObject);

            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                // Send the data.
                requestStream.Write(bytes, 0, bytes.Length);
            }


            {
                StreamReader reader;
                Stream stream;
                try
                {
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    stream = response.GetResponseStream();
                }
                catch (WebException ex)
                {
                    stream = ex.Response.GetResponseStream();
                }
                using (reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }

            }
            return json;

        }
        public static void SaveCreditLineLogs(ulong productId, string funcName, string message)
        {
            UtilityDB.SaveCreditLineLogs(productId, funcName, message);
        }

        public static DateTime GetLeasingOperDayForStatements()
        {
            return UtilityDB.GetLeasingOperDayForStatements();
        }

        public static DateTime GetLeasingOperDay()
        {
            return UtilityDB.GetLeasingOperDay();
        }

        public static string GetCustomerDescriptionEnglish(ulong customerNumber)
        {
            string customerDescription = ACBAOperationService.GetCustomerDescriptionEnglish(customerNumber);
            return customerDescription;
        }

        public static double GetBuyKursForDate(string currency, int filialCode)
        {
            return UtilityDB.GetBuyKursForDate(currency, filialCode);
        }
    }

}
