using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ExternalBanking;

namespace ExternalBanking.DocFlowManagement
{
    public static class Utility
    {
        public static List<MemoField> ConstructHBApplicationMemoDocument(HBApplicationOrder order, int memoType)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);
            HBApplication hbApplication = HBApplication.GetHBApplication(order.CustomerNumber);
             
            List<string> hbApplicationStates = order.GetHBApplicationStateBeforeAndAfterConfirm();
           
            foreach (var field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 1)
                    {

                    }

                    if (field.ControlType == 2)
                    {
                        if (field.ControlName == "txt_hb_customer_number")
                        {
                            field.ItemDescription = hbApplication != null ? hbApplication.CustomerNumber.ToString() : order.CustomerNumber.ToString();// hbApplication.CustomerNumber.ToString();
                        }

                        if (field.ControlName == "txt_order_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }
                        if (field.ControlName == "txt_contract_number")
                        {
                            field.ItemDescription = hbApplication != null ? hbApplication.ContractNumber : order.HBApplication.ContractNumber;
                        }
                        if (field.ControlName == "txt_contract_date")
                        {
                            field.ItemDescription = hbApplication != null ? hbApplication.ContractDate.Value.ToString("dd/MM/yyyy") : order.RegistrationDate.ToString("dd/MM/yyyy");
                        }
                        if (field.ControlName == "txt_quality")
                        {
                            field.ItemDescription = hbApplication != null ? hbApplication.QualityDescription : order.HBApplication.QualityDescription;
                        }
                        if (field.ControlName == "txt_set_date")
                        {
                            field.ItemDescription = hbApplication != null ? hbApplication.ApplicationDate.Value.ToString("dd/MM/yyyy") : order.RegistrationDate.ToString("dd/MM/yyyy");
                        }
                        if (field.ControlName == "txt_set_number")
                        {
                            field.ItemDescription += hbApplication != null ? hbApplication.SetID.ToString() : order.user.userID.ToString();
                            var fullName = (hbApplication != null ? ExternalBanking.Utility.GetUserFullName(hbApplication.SetID) : ExternalBanking.Utility.GetUserFullName(order.user.userID));
                            if (fullName == string.Empty)
                            {
                                field.ItemDescription = "Անհայտ պատ․ կատարող";
                            }
                            else
                            {
                                field.ItemDescription += "(" + fullName + ")";
                            }

                        }

                        if (field.ControlName == "txt_tokens_before_approve")
                        {
                            field.ItemDescription = Regex.Replace(hbApplicationStates[0], @"&lt;br/&gt;", "<br/>");
                        }
                        if (field.ControlName == "txt_added_tokens")
                        {
                            field.ItemDescription = Regex.Replace(hbApplicationStates[1], @"&lt;br/&gt;", "<br/>");
                        }
                        if (field.ControlName == "txt_tokens_after_approve")
                        {
                            field.ItemDescription = Regex.Replace(hbApplicationStates[2], @"&lt;br/&gt;", "<br/>");
                        }
                    }
                    if (field.ControlType == 3)
                    {

                    }
                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

        public static DataTable ConvertMemoFieldsToDataTable(List<MemoField> memoFields)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Position_id");
            dt.Columns.Add("ItemDescr");
            dt.Columns.Add("input_type");
            dt.Columns.Add("control_type");

            dt.Columns.Add("control_name");
            dt.Columns.Add("field_value_type");
            dt.Columns.Add("parameter_name");
            dt.Columns.Add("parameter_value");
            if (memoFields != null)
            {
                foreach (MemoField mf in memoFields)
                {
                    DataRow dr = dt.NewRow();
                    dr["Position_id"] = mf.PositionId;
                    dr["ItemDescr"] = mf.ItemDescription;
                    dr["input_type"] = mf.InputType;
                    dr["control_type"] = mf.ControlType;

                    dr["control_name"] = mf.ControlName;
                    dr["field_value_type"] = mf.FieldValueType;
                    dr["parameter_name"] = mf.ParametrName;
                    dr["parameter_value"] = mf.ParametrValue;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public static byte GetCustomerType(ulong customerNumber)
        {
            byte customerType = 0;
            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
            {
                customerType = proxy.GetCustomerType(customerNumber);
            }
            return customerType;
        }

        /// <summary>
        /// Քարտի փակման MemoDocument-ի մշակում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="memoType"></param>
        /// <returns></returns>
        public static List<MemoField> ConstructCardClosingOrderMemoDocument(CardClosingOrder order, short schemaType, int memoType, string clientIp)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);
            Card card = Card.GetCardMainData(order.ProductId, order.CustomerNumber);
            foreach (MemoField field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 2)
                    {

                        if (field.ControlName == "txt_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }

                        if (field.ControlName == "txt_customer_number")
                        {
                            field.ItemDescription = order.CustomerNumber.ToString();
                        }

                        if (field.ControlName == "txt_name")
                        {
                            CustomerMainData mainData;
                            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
                            {
                                mainData = (CustomerMainData)proxy.GetCustomerMainData(order.CustomerNumber);
                            }
                            field.ItemDescription = mainData.CustomerDescription;
                        }

                        if (field.ControlName == "txt_closing_card_number")
                        {
                            if (card != null)
                            {
                                field.ItemDescription = card.CardNumber;
                            }
                        }
                        if (field.ControlName == "txt_closing_reason")
                        {
                            field.ItemDescription = order.ClosingReasonDescription;
                        }
                        if (field.ControlName == "txt_blocking")
                        {

                            try
                            {

                                ArcaCardsTransactionOrder arcaCards = new ArcaCardsTransactionOrder
                                {
                                    CardNumber = card.CardNumber,
                                    ActionType = 1,
                                    ActionReasonId = 1,
                                    CustomerNumber = order.CustomerNumber,
                                    user = order.user,
                                    Source = order.Source,
                                    GroupId = order.GroupId,
                                    IPAddress = clientIp
                                };
                                
                                var saveResult = arcaCards.Save(order.user.userName, order.Source, order.user,schemaType);
                                var approveResult = arcaCards.Approve(order.user.userName, schemaType);
                                if (approveResult.ResultCode == ResultCode.Normal)
                                    field.ItemDescription = "Այո";
                                else
                                    field.ItemDescription = "Ոչ";
                            }
                            catch (Exception e)
                            {
                                field.ItemDescription = "Ոչ";
                            }
                        }
                        if (field.ControlName == "txt_add_closing_reason")
                        {
                            field.ItemDescription = order.ClosingReasonAdd;
                        }
                    }

                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

        /// <summary>
        /// Հիմնական քարտի MemoDocument-ի մշակում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="memoType"></param>
        /// <returns></returns>
        public static List<MemoField> ConstructPlasticCardOrderMemoDocument(PlasticCardOrder order, short schemaType, int memoType)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);

            foreach (MemoField field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 2)
                    {

                        if (field.ControlName == "txt_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }

                        if (field.ControlName == "txt_card")
                        {
                            field.ItemDescription = order.PlasticCard.CardTypeDescription;
                        }

                        if (field.ControlName == "txt_currency")
                        {
                            field.ItemDescription = order.PlasticCard.Currency;
                        }

                        if (field.ControlName == "txt_password")
                        {
                                field.ItemDescription = order.MotherName;
                        }
                        if (field.ControlName == "txt_report_type")
                        {
                            Dictionary<int, string> types = new Dictionary<int, string>();
                            DataTable dt = Info.GetCardReportReceivingTypes();

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string description = dt.Rows[i]["description"].ToString();
                                types.Add(Convert.ToInt32(dt.Rows[i]["type_id"].ToString()), description);
                            }
                                field.ItemDescription = types[order.CardReportReceivingType.Value];

                        }
                        if (field.ControlName == "txt_customer_number")
                        {
                            field.ItemDescription = order.CustomerNumber.ToString();
                        }
                    }
                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

        /// <summary>
        /// Լրացուցիչ քարտի MemoDocument-ի մշակում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="memoType"></param>
        /// <returns></returns>
        public static List<MemoField> ConstructAttachedPlasticCardOrderMemoDocument(PlasticCardOrder order, short schemaType, int memoType)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);

            foreach (MemoField field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 2)
                    {

                        if (field.ControlName == "txt_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }

                        if (field.ControlName == "txt_main_card")
                        {
                            field.ItemDescription = order.PlasticCard.MainCardNumber;
                        }

                        if (field.ControlName == "txt_attached_card_type")
                        {
                            field.ItemDescription = order.PlasticCard.CardTypeDescription;
                        }

                    }
                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

        /// <summary>
        /// Կից քարտի MemoDocument-ի մշակում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="memoType"></param>
        /// <returns></returns>
        public static List<MemoField> ConstructLinkedPlasticCardOrderMemoDocument(PlasticCardOrder order, short schemaType, int memoType)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);

            foreach (MemoField field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 2)
                    {

                        if (field.ControlName == "txt_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }

                        if (field.ControlName == "txt_main_card")
                        {
                            field.ItemDescription = order.PlasticCard.MainCardNumber;
                        }

                        if (field.ControlName == "txt_phone_number")
                        {
                            field.ItemDescription = order.CardSMSPhone;
                        }

                        if (field.ControlName == "txt_password")
                        {
                            field.ItemDescription = order.MotherName;
                        }

                        if (field.ControlName == "txt_limit")
                        {
                            field.ItemDescription = order.LinkedCardLimit == -1? "Հիմնական քարտի գումարի չափով":order.LinkedCardLimit.ToString();
                        }

                    }
                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

        /// <summary>
        /// Հաշվի փակման MemoDocument-ի մշակում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="memoType"></param>
        /// <returns></returns>
        public static List<MemoField> ConstructAccountClosingOrderMemoDocument(AccountClosingOrder order, short schemaType, int memoType)
        {
            List<MemoField> memoFields = MemoDocument.GetMemoTemplate(memoType);
            //Card card = Card.GetCard(order.ProductId, order.CustomerNumber);
            foreach (MemoField field in memoFields)
            {
                if (field.InputType == 2)
                {
                    if (field.ControlType == 2)
                    {

                        if (field.ControlName == "txt_id")
                        {
                            field.ItemDescription = order.Id.ToString();
                        }

                        if (field.ControlName == "txt_customer_number")
                        {
                            field.ItemDescription = order.CustomerNumber.ToString();
                        }

                        if (field.ControlName == "txt_name")
                        {
                            CustomerMainData mainData;
                            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
                            {
                                mainData = (CustomerMainData)proxy.GetCustomerMainData(order.CustomerNumber);
                            }
                            field.ItemDescription = mainData.CustomerDescription;
                        }

                        if (field.ControlName == "txt_closing_account_number")
                        {
                            field.ItemDescription = order.ClosingAccounts[0].AccountNumber;
                        }
                        if (field.ControlName == "txt_closing_reason")
                        {
                            field.ItemDescription = order.ClosingReasonDescription;
                        }
                    }

                }
                else
                {
                    field.ItemDescription = field.FieldValue;
                }
            }
            return memoFields;
        }

    }
        
}
