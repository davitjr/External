using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking;
using System.Threading.Tasks;
using ExternalBanking.XBManagement;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.DocFlowManagement
{
    public static class DocFlow
    {
        public static ActionResult SendHBApplicationOrderToConfirm(HBApplicationOrder order, ACBAServiceReference.User user)
        {
            var memoType = user.filialCode == 22000 ? 690 : 691;
            var memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructHBApplicationMemoDocument(order, memoType);
            var memoResult = memoDocument.SaveAndSend(user, memoType,user.filialCode);
            return DocFlowDB.SendHBApplicationOrderToConfirm(order.Id, memoResult.Id);

       }

        public static ActionResult SendCardClosingOrderToConfirm(CardClosingOrder order, ACBAServiceReference.User user, short schemaType, string clientIp)
        {
            int filialCode = Card.GetCardServicingFilialCode(order.ProductId);
            if(filialCode == 22000)
            {
                filialCode = 22059;
            }
            var memoType = 769;
            var memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructCardClosingOrderMemoDocument(order, schemaType, memoType, clientIp);
            var memoResult = memoDocument.SaveAndSend(user,memoType,filialCode);
            return DocFlowDB.SendToConfirm(order.Id, memoResult.Id);
        }

        public static ActionResult SendPlasticCardOrderToConfirm(PlasticCardOrder order, ACBAServiceReference.User user, short schemaType)
        {
            ACBAServiceReference.Customer customer;
            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
            {
                customer = (ACBAServiceReference.Customer)proxy.GetCustomer(order.CustomerNumber);
            }
            int filialCode = customer.filial.key;
            if (filialCode == 22000)
            {
                filialCode = 22059;
            }
            int memoType = 772;
            MemoDocument memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructPlasticCardOrderMemoDocument(order, schemaType, memoType);
            var memoResult = memoDocument.SaveAndSend(user, memoType, filialCode);
            for (int i = 0; i < order.Attachments.Count; i++)
            {
                DocFlowDB.SaveUploadedFiles(order.Attachments[i],memoResult.Id);
            }
            return DocFlowDB.SendToConfirm(order.Id, memoResult.Id);
        }

        public static ActionResult SendAttachedPlasticCardOrderToConfirm(PlasticCardOrder order, ACBAServiceReference.User user, short schemaType)
        {
            int filialCode = order.ProvidingFilialCode;
            if (filialCode == 22000)
            {
                filialCode = 22059;
            }
            int memoType = 773;
            MemoDocument memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructAttachedPlasticCardOrderMemoDocument(order, schemaType, memoType);
            var memoResult = memoDocument.SaveAndSend(user, memoType, filialCode);
            return DocFlowDB.SendToConfirm(order.Id, memoResult.Id);
        }

        public static ActionResult SendLinkedPlasticCardOrderToConfirm(PlasticCardOrder order, ACBAServiceReference.User user, short schemaType)
        {
            int filialCode = order.ProvidingFilialCode;
            if (filialCode == 22000)
            {
                filialCode = 22059;
            }
            int memoType = 774;
            MemoDocument memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructLinkedPlasticCardOrderMemoDocument(order, schemaType, memoType);
            var memoResult = memoDocument.SaveAndSend(user, memoType, filialCode);
            for (int i = 0; i < order.Attachments.Count; i++)
            {
                DocFlowDB.SaveUploadedFiles(order.Attachments[i], memoResult.Id);
            }
            return DocFlowDB.SendToConfirm(order.Id, memoResult.Id);
        }

        public static ActionResult SendAccountClosingOrderToConfirm(AccountClosingOrder order, ACBAServiceReference.User user, short schemaType)
        {
            int filialCode = Account.GetAccountServicingFilialCode(order.ClosingAccounts[0].AccountNumber);
            if (filialCode == 22000)
            {
                filialCode = 22059;
            }
            var memoType = 786;
            var memoDocument = new MemoDocument();
            memoDocument.MemoFields = DocFlowManagement.Utility.ConstructAccountClosingOrderMemoDocument(order, schemaType, memoType);
            var memoResult = memoDocument.SaveAndSend(user, memoType, filialCode);
            return DocFlowDB.SendToConfirm(order.Id, memoResult.Id);
        }

    }
}
