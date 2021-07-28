using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class HBDocuments
    {
        public long TransactionCode { get; set; }
        public string TransactionDate { get; set; }
        public int FilialCode { get; set; }
        public long CustomerNumber { get; set; }
        public string CustomerFullName { get; set; }
        public int DocumentType { get; set; }
        public int Type { get; set; }
        public int DocumentSubtype { get; set; }
        public double TransactionAmount { get; set; }
        public string TransactionCurrency { get; set; }
        public long? DebitAccount { get; set; }
        public string TransactionDescription { get; set; }
        public string ConfirmationDate { get; set; }
        public int TransactionQuality { get; set; }
        public int CreditBankCode { get; set; }
        public int Urgent { get; set; }
        public int TransactionSource { get; set; }
        public bool ForAutomatConfirmated { get; set; }
        public bool ByJob { get; set; }
        public string RegistrationDate { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverNameInRecords { get; set; }

        public double TotalAmount { get; set; }
        public int TotalQuantity { get; set; }

        public int LastRow { get; set; }

        public bool TypeIsForAutomatonfirmated { get; set; }

        public string CustomerWroteDescription { get; set; }

        public int SelectedRejectReason { get; set; }

        public HBDocumentCustomerDetails CustomerDetails { get; set; }

        public HBDocumentAccountDetails AccountDetails { get; set; }

        public string CreditAccount { get; set; }

        public int SetNumber { get; set; }


        public static List<HBDocuments> GetSearchedHBDocuments(HBDocumentFilters obj)
        {
            return HBDocumentsDB.GetSearchedHBDocuments(obj);
        }

        public static List<HBDocuments> GetHBDocumentsList(HBDocumentFilters obj)
        {
            return HBDocumentsDB.GetHBDocumentsList(obj);
        }

        public static HBDocumentTransactionError GetTransactionErrorDetails(long transctionCode)
        {
            return HBDocumentsDB.GetTransactionErrorDetails(transctionCode);
        }

        public static List<HBDocumentConfirmationHistory> GetConfirmationHistoryDetails(long transctionCode)
        {
            return HBDocumentsDB.GetConfirmationHistoryDetails(transctionCode);
        }
        public static string GetCheckingProductAccordance(long transctionCode)
        {
            return HBDocumentsDB.GetCheckingProductAccordance(transctionCode);
        }

        public static HBDocumentConfirmationHistory GetProductAccordanceDetails(long transctionCode)
        {
            return HBDocumentsDB.GetProductAccordanceDetails(transctionCode);
        }
        public static bool SetHBDocumentAutomatConfirmationSign(HBDocumentFilters obj)
        {
            return HBDocumentsDB.SetHBDocumentAutomatConfirmationSign(obj);
        }

        public static bool ExcludeCardAccountTransactions(HBDocumentFilters obj)
        {
            return HBDocumentsDB.ExcludeCardAccountTransactions(obj);
        }

        public static bool SelectOrRemoveFromAutomaticExecution(HBDocumentFilters obj)
        {
            return HBDocumentsDB.SelectOrRemoveFromAutomaticExecution(obj);
        }

        public static string GetHBArCaBalancePermission(long transctionCode, long accountGroup)
        {
            return HBDocumentsDB.GetHBArCaBalancePermission(transctionCode, accountGroup);
        }

        public static string GetHBAccountNumber(string cardNumber)
        {
            return HBDocumentsDB.GetHBAccountNumber(cardNumber);
        }


        public static string ConfirmTransactionReject(HBDocuments documents)
        {
            return HBDocumentsDB.ConfirmTransactionReject(documents);
        }

        public static string ChangeTransactionQuality(long transctionCode)
        {
            return HBDocumentsDB.ChangeTransactionQuality(transctionCode);
        }

        //public static string CheckTransactionQualityChangability(int transctionCode)
        //{
        //    return HBDocumentsDB.ChangeTransactionQuality(transctionCode);
        //}

        public static string ChangeAutomatedConfirmTime(List<string> info)
        {
            return HBDocumentsDB.ChangeAutomatedConfirmTime(info);
        }

        public static string GetAutomatedConfirmTime()
        {
            return HBDocumentsDB.GetAutomatedConfirmTime();
        }

        public static bool FormulateAllHBDocuments(HBDocumentFilters obj)
        {
            return HBDocumentsDB.FormulateAllHBDocuments(obj);
        }

        public static List<ReestrTransferAdditionalDetails> CheckHBReestrTransferAdditionalDetails(long orderId, List<ReestrTransferAdditionalDetails> details)
        {
            return HBDocumentsDB.CheckHBReestrTransferAdditionalDetails(orderId, details);
        }

        public static List<string> GetTreansactionConfirmationDetails(long docId, long debitAccount)
        {
            return HBDocumentsDB.GetTreansactionConfirmationDetails(docId, debitAccount);
        }

        public static string ConfirmReestrTransaction(long docId, int bankCode, short setNumber)
        {
            return HBDocumentsDB.ConfirmReestrTransaction(docId, bankCode, setNumber);
        }

        public static bool SaveInternationalPaymentAddresses(InternationalPaymentOrder order)
        {
            return HBDocumentsDB.SaveInternationalPaymentAddresses(order);
        }

        public static List<HBMessages> GetHBMessages(ushort filalCode,string WatchAllMessages)
        {
            return HBDocumentsDB.GetHBMessages(filalCode, WatchAllMessages);
        }

        public static List<HBMessages> GetSearchedHBMessages(HBMessagesSreach obj, ushort filalCode, string WatchAllMessages)
        {
            return HBDocumentsDB.GetSearchedHBMessages(obj, filalCode, WatchAllMessages);
        }

        public static string PostMessageAsRead(long msgId, int setNumber)
        {
            return HBDocumentsDB.PostMessageAsRead(msgId, setNumber);
        }

        public static string PostSentMessageToCustomer(HBMessages obj)
        {
            return HBDocumentsDB.PostSentMessageToCustomer(obj);
        }

        public static List<HBMessageFiles> GetMessageUploadedFilesList(long msgId)
        {
            return HBDocumentsDB.GetMessageUploadedFilesList(msgId);
        }

        public static int GetCancelTransactionDetails(long docId)
        {
            return HBDocumentsDB.GetCancelTransactionDetails(docId);
        }

        public static List<ReestrTransferAdditionalDetails> GetTransactionIsChecked(long docId, List<ReestrTransferAdditionalDetails> details)
        {
            return HBDocumentsDB.GetTransactionIsChecked(docId, details);
        }

        public static bool GetReestrFromHB(HBDocuments obj)
        {
            return HBDocumentsDB.GetReestrFromHB(obj);
        }

        public static string PostBypassHistory(HBDocumentBypassTransaction obj)
        {
            return HBDocumentsDB.PostBypassHistory(obj);
        }

        public static string PostApproveUnconfirmedOrder(long docId, int setNumber)
        {
            return HBDocumentsDB.PostApproveUnconfirmedOrder(docId, setNumber);
        }

        public static bool CheckReestrTransactionIsChecked(long docId)
        {
            return HBDocumentsDB.CheckReestrTransactionIsChecked(docId);
        }

        public static string GetcheckedReestrTransferDetails(long docId)
        {
            return HBDocumentsDB.GetcheckedReestrTransferDetails(docId);
        }

        public static void PostReestrPaymentDetails(ReestrTransferOrder order)
        {
            HBDocumentsDB.PostReestrPaymentDetails(order);
        }
        public static HBMessageFiles GetMsgSelectedFile(int fileId)
        {
            return HBDocumentsDB.GetMsgSelectedFile(fileId);
        }
        public static HBDocuments GetCustomerAccountAndInfoDetails(HBDocuments obj)
        {
            return HBDocumentsDB.GetCustomerAccountAndInfoDetails(obj);
        }

        public static string GetcheckedArmTransferDetails(long docId)
        {
            return HBDocumentsDB.GetcheckedArmTransferDetails(docId);
        }
    }
}
