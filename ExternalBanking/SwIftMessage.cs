using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ExternalBanking
{

    /// <summary>
    ///  SWIFT Message բանկային պրոդուկտ
    /// </summary>
    public class SwiftMessage
    {

        /// <summary>
        /// SWIFT հաղորդագրության ունիկալ համար 
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության ունիկալ համար 
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության ուղղություն ` ելքային կամ մուտքային
        /// 1 - ելքային
        /// 0 - մուտքային 
        /// </summary>
        public int InputOutput { get; set; }


        /// <summary>
        /// SWIFT հաղորդագրության ներքին հաշվառման տեսակ
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության MT (SWIFT կոդավորման) տեսակ 
        /// </summary>
        public int MtCode { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության անալիտիկ հաշիվ  
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության հաճախորդի համար  
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության վերաբերվող բանկի SWIFT CODE
        /// </summary>
        public string SWIFTCode { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության պատկանելիության մասնաճյուղի կոդ
        /// </summary>
        public ulong FilialCode { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության միջնորդավխարի գումար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության միջնորդավխարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության պատկատարողի համար
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության հանար ուղարկվող ֆայլի անունը (MT 950 - SWIFT քաղվածք)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Գործողության համար
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Հղման համար
        /// </summary>
        public string ReferanceNumber { get; set; }

        /// <summary>
        /// Հաստատման ա/թ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Հաստատողի ՊԿ
        /// </summary>
        public int ConfirmationSetNumber { get; set; }

        /// <summary>
        /// Ֆայլի ստեղծման ա/թ
        /// </summary>
        public DateTime? FileCreatedDate { get; set; }

        /// <summary>
        /// Հեռցված լինելու նշում
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Հեռացնողի ՊԿ
        /// </summary>
        public int DeletedSetNumber { get; set; }

        /// <summary>
        /// Ֆայլի պարունակություն
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        /// Ստացողի հաշիվ
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Ստացողի SWIFT
        /// </summary>
        public string ReceiverSwift { get; set; }

        /// <summary>
        /// Ստացող բանկի SWIFT
        /// </summary>
        public string ReceiverBankSwift { get; set; }

        /// <summary>
        /// Միջնորդ բանկի SWIFT կոդ
        /// </summary>
        public string IntermediaryBankSwift { get; set; }

        /// <summary>
        /// Մերժված լինելու նշում
        /// </summary>
        public bool IsRejected { get; set; }

        public SwiftMessage()
        {

        }


        /// <summary>
        /// SWIFT Message-ի տվյալները ըստ ունիկալ համարի
        /// </summary>

        /// <param name="messageUniqNumber">
        /// messageUniqNumber - պարբերական փոխանվման ունիկալ համար
        /// </param>

        /// <returns SwiftMessage>
        /// վերադարձնում է SwiftMessage տոեսակի օբյեկտ
        /// </returns>

        public static SwiftMessage GetSwiftMessage(ulong messageUniqNumber)
        {
            SwiftMessage swiftMessage = SwiftMessageDB.GetSwiftMessage(messageUniqNumber);
            return swiftMessage;
        }


        /// <summary>
        /// Նոր SWIFT Message-ի գեներացուն 
        /// </summary>

        /// <param name="accountNumber">
        /// accountNumber - հաշիվ
        /// </param>

        /// <param name="receiverBankSwiftCode">
        /// receiverBankSwiftCode - ստացող բանկի SWIFT կոդը
        /// </param>

        /// <param name="mtType">
        /// mtType - SWIFT տեսակը
        /// </param>

        /// <param name="filialCode">
        /// filialCode - պատկանող մասնաճյուղի կոդ
        /// </param>

        /// <param name="registrationDate">
        /// registrationDate - գրանցման ամսաթիվ
        /// </param>

        /// <param name="userID">
        /// userID - պատկատարող
        /// </param>

        /// <param name="feeAmount">
        /// feeAmount - միջնորդավճարի գումար
        /// </param>

        /// <param name="feeAccount">
        /// feeAccount - միջնորդավճար գանձելու հաշիվ 
        /// </param>

        /// <returns - SwiftMessage>
        /// վերադարձնում է SwiftMessage տոեսակի օբյեկտ
        /// </returns>


        public ulong GenerateNewOuptutSwiftMessage(Account accountNumber,
                                                                        string receiverBankSwiftCode, ulong mtType, ulong filialCode,
                                                                        DateTime registrationDate, short userID,
                                                                        double feeAmount, Account feeAccount)
        {

            ulong messageUnicNumber = SwiftMessageDB.GenerateNewOuptutSwiftMessage(accountNumber,
                                                                        receiverBankSwiftCode, mtType, filialCode,
                                                                        registrationDate, userID,
                                                                        feeAmount, feeAccount);
            return messageUnicNumber;

        }


        /// <summary>
        /// SWIFT Message-ի գեներացում պարբերական փոխանցման հիմնան վրա
        /// </summary>

        /// <param name="registrationDate">
        /// SWIFT հաղորդագրության գրանցման ամսաթիվ
        /// </param>

        /// <param name="userID">
        /// SWIFT հաղորդագրության գրանցողի (ՊատԿատարողի) կոդ
        /// </param>
        /// 
        /// <param name="periodicTransferId">
        /// Պարբերական փսխանցման ունիկալ համար, ոևի հիմնան վրա գեներացվում է SWIFT հաղորդագրությունը
        /// </param>

        /// <returns SwiftMessage>
        /// վերադարձնում է SwiftMessage տոեսակի օբյեկտ
        /// </returns>
        /// 
        public static SwiftMessage GenerateNewSwiftMessageByPeriodicTransfer(DateTime registrationDate, short userID, ulong periodicTransferId)
        {
            ActionResult result = new ActionResult();

            PeriodicTransfer swiftMessagePeriodicTransfer = new PeriodicTransfer();
            swiftMessagePeriodicTransfer = PeriodicTransfer.GetPeriodicTransfer(periodicTransferId);

            SwiftMessage swiftMessage = null;
            if (swiftMessagePeriodicTransfer != null)
            {
                swiftMessage = new SwiftMessage();
                result.ResultCode = ResultCode.Normal;

                ulong Id = SwiftMessageDB.GenerateNewOuptutSwiftMessage(swiftMessagePeriodicTransfer.DebitAccount,
                    swiftMessagePeriodicTransfer.ReceiverBankSwiftCode, 950, swiftMessagePeriodicTransfer.FilialCode,
                    registrationDate, userID, swiftMessagePeriodicTransfer.FeeAmount,
                    swiftMessagePeriodicTransfer.FeeAccount);
                swiftMessage = SwiftMessage.GetSwiftMessage(Id);
            }
            else
            {
                result.ResultCode = ResultCode.Failed;
            }
            //result.Id = long.Parse(Id.ToString());
            return swiftMessage;
        }

        /// <summary>
        /// SWIFT քաղվածքի գեներացում 
        /// </summary>

        /// <param name="messageUniqNumber">
        /// messageUniqNumber - պարբերական փոխանվման ունիկալ համար
        /// </param>

        /// <param name="dateStatement">
        /// dateStatement - քաղվածքի ամսաթիվ
        /// </param>

        /// <param name="dateFrom">
        /// dateFrom - ժամանակահատվածի սկիզբ,որի համար գեներացվում է քաղվածքը
        /// </param>

        /// <param name="dateTo">
        /// dateTo - ժամանակահատվածի վերջ,որի համար գեներացվում է քաղվածքը
        /// </param>

        /// <param name="operationsCountInOnePart">
        /// operationsCountInOnePart - մակսիմալ գործարքների քանակ, որոնք կազմում են քաղվածքի մեկ մաս 
        /// </param>

        /// <returns ActionResult>
        /// Վերադարձնում է ActionResult տեսակի օբյեկտ, որը ցույց է տալիս արդյոք մուտքագրումը տեղի է ունեցել թե ոչ 
        /// </returns>
        public ActionResult MakeSwiftStatement(DateTime statementDate, DateTime dateFrom, DateTime dateTo)
        {
            ActionResult result = new ActionResult();
            result = SwiftMessageDB.MakeSwiftStatement(this, statementDate, dateFrom, dateTo);
            return result;
        }

        public static ActionResult GenerateAndMakeSwitMessageByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo, short userID, ulong periodicTransferId)
        {
            ActionResult result = new ActionResult();

            PeriodicTransfer swiftMessagePeriodicTransfer = PeriodicTransfer.GetPeriodicTransfer(periodicTransferId);
            bool existingTransactions = AccountDB.CheckForTransactions(swiftMessagePeriodicTransfer.DebitAccount, dateFrom, dateTo);
            if (existingTransactions == true || swiftMessagePeriodicTransfer.ExistenceOfCirculation == false)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                      new TransactionOptions()
                      {
                          IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                      }))
                {

                    SwiftMessage swiftMessage = GenerateNewSwiftMessageByPeriodicTransfer(statementDate, userID, periodicTransferId);
                    if (swiftMessage == null)
                    {
                        result.ResultCode = ResultCode.Failed;
                        scope.Dispose();
                    }
                    else
                    {
                        ActionResult actionResult = swiftMessage.MakeSwiftStatement(statementDate, dateFrom, dateTo);
                        if (actionResult.ResultCode == ResultCode.Normal)
                        {
                            scope.Complete();
                        }

                        result = actionResult;
                    }
                }
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;

        }


        /// <summary>
        /// Վերադարձնում է SwiftMessage-ի քաղվածքը
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static string GetSwiftMessageStatement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source)
        {
            string statement = "";
            DateTime dateStatement = DateTime.Now.Date;
            SwiftMessage message = new SwiftMessage();
            message.Account = new Account(accountNumber);
            message.SWIFTCode = "";
            Array ArrOfExtractParts = MakeOneRowFileContent(message, dateStatement, dateFrom, dateTo, 20, source);
            for (int i = 0; i < ArrOfExtractParts.Length; i++)
            {
                statement = statement + " " + ArrOfExtractParts.GetValue(i).ToString();
            }
            return statement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swiftMessage"></param>
        /// <param name="dateStatement"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="operationsCountInOnePart"></param>
        /// <returns></returns>
        public static Array MakeOneRowFileContent(SwiftMessage swiftMessage, DateTime dateStatement, DateTime dateFrom, DateTime dateTo, int operationsCountInOnePart = 20, SourceType source = 0)
        {
            string receiverBankSwiftCode = swiftMessage.SWIFTCode;
            string accountNumber = swiftMessage.Account.AccountNumber;
            ulong messageUniqNumber = swiftMessage.ID;
            string strSwiftExtractOnePart = "";
            string signForRow62;
            string signForRow60;
            int extractParts = 0;
            int operationsCountInLastPart = 0;
            string transactionDescriptionTranslit = "";

            /// i - Քաղվածքի մասի համար  
            /// currentRowInStatement - Քաղվածքի տողի աբսոլուտ համար  
            /// currentRowInOnePart - Քաղվածքի տողի համար նեկ բասի նեջ

            int i = 0;
            int currentRowInOnePart = 0;
            int currentRowInStatement = 0;
            double operationAmount = 0;

            Account account = Account.GetAccount(accountNumber);
            string accountCurrency = account.Currency;
            if (accountCurrency == "RUR")
            {
                accountCurrency = "RUB";
            }
            /// SWIFT քաղվածքի նկարագրությունները պետք են միայն անգլերեն
            AccountStatement accountStatement = account.GetStatement(dateFrom, dateTo, 1, 0, source);
            Double restBegCalculated = accountStatement.InitialBalance;
            List<AccountStatementDetail> transactionsList =
                accountStatement.Transactions.FindAll(m => m.OperationType != 499 || m.CurrentAccountNumber != 499);
            int extractRowCount = transactionsList.Count;
            Double restEndCalculated = restBegCalculated;


            extractParts = extractRowCount / operationsCountInOnePart;
            operationsCountInLastPart = extractRowCount % operationsCountInOnePart;

            //  Եթե քաղվածքի տողերը չկան կամ կլոր չեն բաժանվում operationsCountInOnePart-ին 
            if (extractRowCount == 0 || operationsCountInLastPart != 0)
                extractParts++;

            //  Եթե քաղվածքի տողերը կան և կլոր բաժանվում են operationsCountInOnePart-ին , անպայաման պետք է լինի նախորդ ստուգումից հետո
            if (extractRowCount != 0 & operationsCountInLastPart == 0)
                operationsCountInLastPart = operationsCountInOnePart;

            //  Եթե քաղվածքը մեկ մաս ունի 
            if (extractParts == 1)
                operationsCountInOnePart = operationsCountInLastPart;

            Array ArrOfExtractParts = Array.CreateInstance(typeof(string), extractParts);



            while (i <= extractParts - 1)
            {
                /// Քաղվածքի մասի առաջին տողը` ունի առանձին ֆորմատ
                if (i == 0)
                    signForRow60 = "F";
                else
                    signForRow60 = "M";

                strSwiftExtractOnePart = "{1:F01AGCAAM22AXXX"
                    + messageUniqNumber.ToString().PadLeft(10, '0') + "}{2:I950" + receiverBankSwiftCode.ToUpper() + "N}{4:";

                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":20:" + dateStatement.ToString("yyMMdd")
                                             + messageUniqNumber.ToString().PadLeft(5, '0');

                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":25:" +
                                             accountNumber.ToString().PadLeft(15, '0');

                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":28C:" +
                                             messageUniqNumber.ToString().Substring(messageUniqNumber.ToString().Length-5).PadLeft(5, '0') + "/" +
                                             (i + 1).ToString().PadLeft(3, '0');


                if (restBegCalculated < 0)

                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":60" + signForRow60 + ":D";

                else

                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":60" + signForRow60 + ":C";


                strSwiftExtractOnePart = strSwiftExtractOnePart +
                                              dateFrom.ToString("yyMMdd") +
                                              accountCurrency +
                                              Math.Abs(restBegCalculated).ToString("#0.00").Replace(".", ",");


                /// Քաղվածքի 1 մասի տողերի ձևավորում 

                currentRowInOnePart = 0;
                while (currentRowInOnePart < operationsCountInOnePart)
                {
                    StringBuilder strSwiftPartAdded = new StringBuilder("", 150);
                    char debitCredit = transactionsList[currentRowInStatement].DebitCredit;
                    //string extractReference = "//nonref";
                    string extractReference = "";
                    string cashOperationNumber =
                        transactionsList[currentRowInStatement].CashOperationNumber.ToString().PadLeft(5, '0');
                    DateTime dateOfAccounting = transactionsList[currentRowInStatement].TransactionDate;
                    string extractWording = transactionsList[currentRowInStatement].Description;
                    if (extractWording != "")
                    {
                        extractReference = Utility.TranslateToEnglish(extractWording, true);
                    }

                    dynamic transactionDescriptionDetails = AccountDB.GetTransactionDescriptionForSwiftMessage(transactionsList[currentRowInStatement].TransactionsGroupNumber, transactionsList[currentRowInStatement].DebitCredit);

                    extractReference = extractReference.Substring(0, (extractReference.Length > 34) ? 34 : extractReference.Length);

                    //operationAmount = 0;
                    //if (accountCurrency == "AMD")
                    //    operationAmount = transactionsList[currentRowInStatement].Amount;
                    //else
                    //    operationAmount = transactionsList[currentRowInStatement].AmountBase;

                    operationAmount = transactionsList[currentRowInStatement].Amount;
                    strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + ":61:");
                    strSwiftPartAdded = strSwiftPartAdded.Append(dateOfAccounting.ToString("yyMMdd"));
                    strSwiftPartAdded = strSwiftPartAdded.Append(debitCredit.ToString().ToUpper());
                    strSwiftPartAdded = strSwiftPartAdded.Append(operationAmount.ToString("#0.00").Replace(".", ","));
                    strSwiftPartAdded = strSwiftPartAdded.Append("NMSC".ToUpper());
                    if (transactionsList[currentRowInStatement].DebitCredit.ToString() == "c" || transactionDescriptionDetails.Reference1 == "")
                    {
                        strSwiftPartAdded = strSwiftPartAdded.Append(cashOperationNumber);
                    }

                    if (strSwiftPartAdded.ToString().Substring(strSwiftPartAdded.ToString().Length - 4) == "NMSC")
                    {
                        if (transactionDescriptionDetails.Reference1 != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append(transactionDescriptionDetails.Reference1);
                        }
                    }
                    else
                    {
                        if (transactionDescriptionDetails.Reference1 != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append("//" + transactionDescriptionDetails.Reference1);
                        }
                        else
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append("//nonref");
                        }
                    }

                    if (transactionDescriptionDetails.IsAmundiAccount == true && transactionDescriptionDetails.Reference2 != "")
                    {
                        strSwiftPartAdded = strSwiftPartAdded.Append("//" + transactionDescriptionDetails.Reference2);
                    }

                    if (transactionDescriptionDetails.IsAmundiAccount == true && transactionDescriptionDetails.Description != "")
                    {
                        transactionDescriptionTranslit = Utility.TranslateToEnglish(transactionDescriptionDetails.Description.ToString());
                        strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + transactionDescriptionTranslit.Substring(0, (transactionDescriptionTranslit.Length > 34) ? 34 : transactionDescriptionTranslit.Length));
                    }
                    else
                    {
                        if (extractReference != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + extractReference);
                        }
                    }


                    if (debitCredit.ToString().ToUpper() == "D")
                        restEndCalculated = restEndCalculated - operationAmount;
                    else
                        restEndCalculated = restEndCalculated + operationAmount;

                    strSwiftExtractOnePart = strSwiftExtractOnePart + strSwiftPartAdded;
                    currentRowInOnePart = currentRowInOnePart + 1;
                    currentRowInStatement = currentRowInStatement + 1;
                }

                /// Քաղվածքի վերջին տողը ` ունի առանձին ֆորմատ

                if (i != extractParts - 1)
                    signForRow62 = "M";
                else
                    signForRow62 = "F";

                if (restEndCalculated < 0)
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":62" + signForRow62 + ":D";
                // strSwiftExtractOnePart = string.Format(@"{0} :62 {1} :D",strSwiftExtractOnePart,signForRow62);
                else
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":62" + signForRow62 + ":C";


                strSwiftExtractOnePart = strSwiftExtractOnePart + dateTo.ToString("yyMMdd") +
                                              accountCurrency + Math.Abs(restEndCalculated).ToString("#0.00").Replace(".", ",");
                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + "-}";
                restBegCalculated = restEndCalculated;
                ArrOfExtractParts.SetValue(strSwiftExtractOnePart, i);
                i = i + 1;
                if (i == extractParts - 1)
                    operationsCountInOnePart = operationsCountInLastPart;
            }
            return ArrOfExtractParts;
        }


        public static Array MakeOneRowFileContentMT940(SwiftMessage swiftMessage, DateTime dateStatement, DateTime dateFrom, DateTime dateTo, int operationsCountInOnePart = 20, SourceType source = 0)
        {
            string receiverBankSwiftCode = swiftMessage.SWIFTCode;
            string accountNumber = swiftMessage.Account.AccountNumber;
            ulong messageUniqNumber = swiftMessage.ID;
            string strSwiftExtractOnePart = "";
            string replacedSymbols = "";
            string signForRow62;
            string signForRow60;
            int extractParts = 0;
            int operationsCountInLastPart = 0;
            string transactionDescription = "";
            string transactionDescriptionTranslit = "";
            DateTime dateOfAccountingFirst = DateTime.Now;
            /// i - Քաղվածքի մասի համար  
            /// currentRowInStatement - Քաղվածքի տողի աբսոլուտ համար  
            /// currentRowInOnePart - Քաղվածքի տողի համար նեկ բասի նեջ
            int i = 0;
            int currentRowInOnePart = 0;
            int currentRowInStatement = 0;
            double operationAmount = 0;
            Account account = Account.GetAccount(accountNumber);
            string accountCurrency = account.Currency;
            if (accountCurrency == "RUR")
            {
                accountCurrency = "RUB";
            }
            /// SWIFT քաղվածքի նկարագրությունները պետք են միայն անգլերեն
            AccountStatement accountStatement = account.GetStatement(dateFrom, dateTo, 1, 0, source);
            Double restBegCalculated = accountStatement.InitialBalance;
            List<AccountStatementDetail> transactionsList =
                accountStatement.Transactions.FindAll(m => m.OperationType != 499 || m.CurrentAccountNumber != 499);
            int extractRowCount = transactionsList.Count;
            Double restEndCalculated = restBegCalculated;
            extractParts = extractRowCount / operationsCountInOnePart;
            operationsCountInLastPart = extractRowCount % operationsCountInOnePart;
            //  Եթե քաղվածքի տողերը չկան կամ կլոր չեն բաժանվում operationsCountInOnePart-ին 
            if (extractRowCount == 0 || operationsCountInLastPart != 0)
                extractParts++;
            //  Եթե քաղվածքի տողերը կան և կլոր բաժանվում են operationsCountInOnePart-ին , անպայաման պետք է լինի նախորդ ստուգումից հետո
            if (extractRowCount != 0 & operationsCountInLastPart == 0)
                operationsCountInLastPart = operationsCountInOnePart;
            //  Եթե քաղվածքը մեկ մաս ունի 
            if (extractParts == 1)
                operationsCountInOnePart = operationsCountInLastPart;
            Array ArrOfExtractParts = Array.CreateInstance(typeof(string), extractParts);
            if (transactionsList.Count > 0)
            {
                dateOfAccountingFirst = transactionsList[0].TransactionDate;
            }
            //experimental
            DataTable bankMailTransactions = SwiftMessageDB.GetBankMailTransactions(transactionsList);
            DataTable swiftTransactions = SwiftMessageDB.GetSwiftTransactions(transactionsList);
            while (i <= extractParts - 1)
            {
                /// Քաղվածքի մասի առաջին տողը` ունի առանձին ֆորմատ
                if (i == 0)
                    signForRow60 = "F";
                else
                    signForRow60 = "M";
                strSwiftExtractOnePart = "{1:F01AGCAAM22AXXX"
                    + (i + 1).ToString().PadLeft(10, '0') + "}{2:I940" + receiverBankSwiftCode.ToUpper() + "N}{4:";
                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":20:" + dateStatement.ToString("yyMMdd")
                                             + messageUniqNumber.ToString().PadLeft(5, '0');
                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":25:" +
                                             accountNumber.ToString().PadLeft(15, '0');
                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":28C:" +
                                             //messageUniqNumber.ToString().PadLeft(5, '0') + "/" +
                                             (dateOfAccountingFirst - new DateTime(1900, 1, 1)).Days + "/" +
                                             (i + 1).ToString().PadLeft(3, '0');
                if (restBegCalculated < 0)
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":60" + signForRow60 + ":D";
                else
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":60" + signForRow60 + ":C";
                strSwiftExtractOnePart = strSwiftExtractOnePart +
                                              dateFrom.ToString("yyMMdd") +
                                              accountCurrency +
                                              Math.Abs(restBegCalculated).ToString("#0.00").Replace(".", ",");
                /// Քաղվածքի 1 մասի տողերի ձևավորում 
                currentRowInOnePart = 0;
                while (currentRowInOnePart < operationsCountInOnePart)
                {
                    StringBuilder strSwiftPartAdded = new StringBuilder("", 150);
                    char debitCredit = transactionsList[currentRowInStatement].DebitCredit;
                    //string extractReference = "//nonref";
                    string extractReference = "";
                    string cashOperationNumber =
                        transactionsList[currentRowInStatement].CashOperationNumber.ToString().PadLeft(5, '0');
                    DateTime dateOfAccounting = transactionsList[currentRowInStatement].TransactionDate;
                    string extractWordingArm = transactionsList[currentRowInStatement].Description;
                    string extractWording = Info.GetOperationTypeDescription(transactionsList[currentRowInStatement].OperationType, Languages.eng);
                    if (extractWording != "")
                        extractReference = extractWording;
                    dynamic transactionDescriptionDetails;
                    //experimental
                    if (bankMailTransactions.AsEnumerable().Any(x => x.ItemArray.Contains(transactionsList[currentRowInStatement].TransactionsGroupNumber)) ||
                        swiftTransactions.AsEnumerable().Any(x => x.ItemArray.Contains(transactionsList[currentRowInStatement].TransactionsGroupNumber)))
                    {
                        transactionDescriptionDetails = AccountDB.GetTransactionDescriptionForSwiftMessage(transactionsList[currentRowInStatement].TransactionsGroupNumber, transactionsList[currentRowInStatement].DebitCredit);
                    }
                    else
                    {
                        transactionDescriptionDetails = new ExpandoObject();
                        transactionDescriptionDetails.IsAmundiAccount = false;
                        transactionDescriptionDetails.Reference1 = "";
                        transactionDescriptionDetails.Reference2 = "";
                        transactionDescriptionDetails.Description = "";
                    }
                    extractReference = extractReference.Substring(0, (extractReference.Length > 34) ? 34 : extractReference.Length);
                    //operationAmount = 0;
                    //if (accountCurrency == "AMD")
                    //    operationAmount = transactionsList[currentRowInStatement].Amount;
                    //else
                    //    operationAmount = transactionsList[currentRowInStatement].AmountBase;
                    operationAmount = transactionsList[currentRowInStatement].Amount;
                    strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + ":61:");
                    strSwiftPartAdded = strSwiftPartAdded.Append(dateOfAccounting.ToString("yyMMdd"));
                    strSwiftPartAdded = strSwiftPartAdded.Append(debitCredit.ToString().ToUpper());
                    strSwiftPartAdded = strSwiftPartAdded.Append(operationAmount.ToString("#0.00").Replace(".", ","));
                    strSwiftPartAdded = strSwiftPartAdded.Append("NMSC".ToUpper());
                    //strSwiftPartAdded = strSwiftPartAdded.Append(docNumber);
                    //if (transactionsList[currentRowInStatement].DebitCredit.ToString() == "c" || transactionDescriptionDetails.Reference1 == "")
                    if (strSwiftPartAdded.ToString().Substring(strSwiftPartAdded.ToString().Length - 4) == "NMSC")
                    {
                        string docNumber;
                        //strSwiftPartAdded = strSwiftPartAdded.Append(cashOperationNumber);
                        //modified
                        if (bankMailTransactions.AsEnumerable().Any(x => x.ItemArray.Contains(transactionsList[currentRowInStatement].TransactionsGroupNumber)))
                        {
                            docNumber = AccountDB.GetDocumentNumber(transactionsList[currentRowInStatement].TransactionsGroupNumber);
                        }
                        else
                        {
                            docNumber = "";
                        }
                        if (string.IsNullOrEmpty(docNumber))
                        {
                            string documentNumber = AccountDB.GetHBDocumentNumber(transactionsList[currentRowInStatement].TransactionsGroupNumber);
                            strSwiftPartAdded = strSwiftPartAdded.Append(documentNumber);
                        }
                        strSwiftPartAdded = strSwiftPartAdded.Append(docNumber);
                    }
                    if (strSwiftPartAdded.ToString().Substring(strSwiftPartAdded.ToString().Length - 4) == "NMSC")
                    {
                        if (transactionDescriptionDetails.Reference1 != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append(transactionDescriptionDetails.Reference1);
                        }
                    }
                    else
                    {
                        if (transactionDescriptionDetails.Reference1 != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append("//" + transactionDescriptionDetails.Reference1);
                        }
                        else
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append("//nonref");
                        }
                    }
                    if (transactionDescriptionDetails.IsAmundiAccount == true && transactionDescriptionDetails.Reference2 != "")
                    {
                        strSwiftPartAdded = strSwiftPartAdded.Append("//" + transactionDescriptionDetails.Reference2);
                    }

                    if (strSwiftPartAdded.ToString().Substring(strSwiftPartAdded.ToString().Length - 4) == "NMSC")
                        strSwiftPartAdded = strSwiftPartAdded.Append("nonref");

                    if (transactionDescriptionDetails.IsAmundiAccount == true && transactionDescriptionDetails.Description != "")
                    {
                        transactionDescriptionTranslit = Utility.TranslateToEnglish(transactionDescriptionDetails.Description.ToString());
                        strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + transactionDescriptionTranslit.Substring(0, (transactionDescriptionTranslit.Length > 34) ? 34 : transactionDescriptionTranslit.Length));
                    }
                    else
                    {
                        if (extractReference != "")
                        {
                            strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + extractReference);
                        }
                    }
                    strSwiftPartAdded = strSwiftPartAdded.Append(Environment.NewLine + ":86:");
                    //modified
                    if (bankMailTransactions.AsEnumerable().Any(x => x.ItemArray.Contains(transactionsList[currentRowInStatement].TransactionsGroupNumber)))
                    {
                        transactionDescription = AccountDB.GetTransactionDescriptionForSwiftMT940(transactionsList[currentRowInStatement].TransactionsGroupNumber, transactionsList[currentRowInStatement].DebitCredit, transactionsList[currentRowInStatement].Description);
                    }
                    else
                    {
                        transactionDescription = "";
                    }
                    if (transactionDescription == "")
                    {
                        transactionDescription = Utility.TranslateToEnglish(transactionsList[currentRowInStatement].Description, true);
                        if (transactionDescription.Length > 65)
                        {
                            string finalString = "";
                            //transactionDescription.Insert(65, Environment.NewLine);
                            //IEnumerable<string> array = Enumerable.Range(0, transactionDescription.Length / 65).Select(i => transactionDescription.Substring(i * 65, 65));
                            for (int z = 0; z < transactionDescription.Length; z += 65)
                            {
                                if ((transactionDescription.Substring(z)).Length > 65)
                                {
                                    finalString += (transactionDescription.Substring(z, 65)) + Environment.NewLine;
                                }
                                else
                                {
                                    finalString += (transactionDescription.Substring(z));
                                }
                            }
                            transactionDescription = finalString;
                        }
                    }
                    if (transactionDescription == "")
                    {
                        transactionDescription = Utility.TranslateToEnglish(extractWordingArm, true);
                        if (transactionDescription.Length > 65)
                        {
                            string finalString = "";
                            //transactionDescription.Insert(65, Environment.NewLine);
                            //IEnumerable<string> array = Enumerable.Range(0, transactionDescription.Length / 65).Select(i => transactionDescription.Substring(i * 65, 65));
                            for (int z = 0; z < transactionDescription.Length; z += 65)
                            {
                                finalString += (transactionDescription.Substring(z, 65)) + Environment.NewLine;
                            }
                            transactionDescription = finalString;
                        }
                    }
                    strSwiftPartAdded = strSwiftPartAdded.Append(transactionDescription);
                    if (debitCredit.ToString().ToUpper() == "D")
                        restEndCalculated = restEndCalculated - operationAmount;
                    else
                        restEndCalculated = restEndCalculated + operationAmount;
                    replacedSymbols = ReplaceSymbols(strSwiftPartAdded.ToString());

                    strSwiftExtractOnePart = strSwiftExtractOnePart + replacedSymbols;
                    currentRowInOnePart = currentRowInOnePart + 1;
                    currentRowInStatement = currentRowInStatement + 1;
                }
                /// Քաղվածքի վերջին տողը ` ունի առանձին ֆորմատ
                if (i != extractParts - 1)
                    signForRow62 = "M";
                else
                    signForRow62 = "F";
                if (restEndCalculated < 0)
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":62" + signForRow62 + ":D";
                // strSwiftExtractOnePart = string.Format(@"{0} :62 {1} :D",strSwiftExtractOnePart,signForRow62);
                else
                    strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + ":62" + signForRow62 + ":C";
                strSwiftExtractOnePart = strSwiftExtractOnePart + dateTo.ToString("yyMMdd") +
                                              accountCurrency + Math.Abs(restEndCalculated).ToString("#0.00").Replace(".", ",");
                strSwiftExtractOnePart = strSwiftExtractOnePart + Environment.NewLine + "-}";
                restBegCalculated = restEndCalculated;
                ArrOfExtractParts.SetValue(strSwiftExtractOnePart, i);
                i = i + 1;
                if (i == extractParts - 1)
                    operationsCountInOnePart = operationsCountInLastPart;
            }
            return ArrOfExtractParts;
        }

        public static bool CheckSentSwiftStatus(int swiftMessageID)
        {
            return SwiftMessageDB.CheckSentSwiftStatus(swiftMessageID);
        }

        public static string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source = 0)
        {
            string statement = "";
            DateTime dateStatement = DateTime.Now.Date;
            SwiftMessage message = new SwiftMessage();
            message.Account = new Account(accountNumber);
            message.SWIFTCode = "";
            Array ArrOfExtractParts = MakeOneRowFileContentMT940(message, dateStatement, dateFrom, dateTo, 20, source);
            for (int i = 0; i < ArrOfExtractParts.Length; i++)
            {
                statement = statement + " " + ArrOfExtractParts.GetValue(i).ToString();
            }
            return statement;
        }
        private static ActionResult GenerateAndMakeSwiftMessageByPeriodicTransfer(DateTime currentOperDay, DateTime statementDate, DateTime dateFrom, DateTime dateTo, short userID, PeriodicTransfer periodicTransfer)
        {
            ActionResult result = new ActionResult();

            //PeriodicTransfer swiftMessagePeriodicTransfer = PeriodicTransfer.GetPeriodicTransfer(periodicTransferId);
            PeriodicTransfer swiftMessagePeriodicTransfer = periodicTransfer;

            bool existingTransactions = AccountDB.CheckForTransactions(swiftMessagePeriodicTransfer.DebitAccount, dateFrom, dateTo);
            if (existingTransactions == true || swiftMessagePeriodicTransfer.ExistenceOfCirculation == false)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                      new TransactionOptions()
                      {
                          IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                      }))
                {

                    SwiftMessage swiftMessage = GenerateNewSwiftMessageByPeriodicTransfer(statementDate, userID, periodicTransfer.ProductId);
                    if (swiftMessage == null)
                    {
                        result.ResultCode = ResultCode.InvalidRequest;
                        swiftMessagePeriodicTransfer.AddPeriodicTransferLog(0, "Պարբերական փոխանցումը գտնված չէ։");
                        scope.Dispose();
                    }
                    else
                    {
                        if (swiftMessagePeriodicTransfer.EndDate <= statementDate || swiftMessagePeriodicTransfer.EndDate <= currentOperDay)
                        {
                            swiftMessagePeriodicTransfer.PeriodicTransfersClosed();
                            swiftMessagePeriodicTransfer.AddPeriodicTransferLog(0, "Պարբերական փոխանցման ժամկետը լրացել է։");
                            result.ResultCode = ResultCode.Normal;
                        }
                        else
                        {
                            ActionResult actionResult = swiftMessage.MakeSwiftStatement(statementDate, dateFrom, dateTo);
                            if (actionResult.ResultCode == ResultCode.Normal)
                            {
                                swiftMessagePeriodicTransfer.SetCompleted(statementDate, statementDate);
                                swiftMessagePeriodicTransfer.AddPeriodicTransferLog(1, "Կատարված է։");
                            }
                            result = actionResult;

                        }
                        scope.Complete();
                    }
                }
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
                swiftMessagePeriodicTransfer.SetCompleted(statementDate, statementDate);//new
                swiftMessagePeriodicTransfer.AddPeriodicTransferLog(1, "Կատարված է։");
            }

            return result;

        }

        public static ActionResult GenerateAndMakeSwiftMessagesByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo, short userID)
        {
            DataTable dt = new DataTable();
            ActionResult result = new ActionResult();
            dt = SwiftMessageDB.GetSWIFTPeriodicTransfers(statementDate);
            DateTime currentOperDay = Utility.GetCurrentOperDay();
            PeriodicTransfer periodicTransfer = new PeriodicTransfer();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    periodicTransfer = PeriodicTransfer.GetPeriodicTransfer(ulong.Parse(dt.Rows[i]["app_id"].ToString()));
                    result = GenerateAndMakeSwiftMessageByPeriodicTransfer(currentOperDay, statementDate, dateFrom, dateTo, userID, periodicTransfer);
                }
                catch (Exception ex)
                {
                    periodicTransfer.AddPeriodicTransferLog(2, ex.Message);
                    result.ResultCode = ResultCode.Failed;
                }
            }

            periodicTransfer.CloseExpiredPeriodicTransfers(currentOperDay, statementDate);
            return result;
        }

        internal static string ReplaceSymbols(string str)
        {
            string[] symbols = new string[] { "~", "!", "@", "«", "»", "#", "$", ";", "%", "^", "&", "*", "=", "{", "}", "[", "]", "_", "<", ">", "№", "“", "”", "”", "\\", "\"" };
            foreach (var item in symbols)
            {
                str = str.Replace(item, " ");
            }
            return str;
        }

    }
}
