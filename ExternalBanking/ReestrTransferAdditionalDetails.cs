using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{
    /// <summary>
    /// Ռեեստրով փոխանցման 
    /// </summary>
    public class ReestrTransferAdditionalDetails
    {
        /// <summary>
        /// Հերթական համար
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Կրեդիտագվող հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Փոխանցման համար
        /// </summary>
        public ulong? TransactionsGroupNumber { get; set; }


        /// <summary>
        /// Արգելադրման նպատակ
        /// </summary>
        public ushort PaymentType { get; set; }

        /// <summary>
        /// HBDocument-ի Ռեեստրով փոխանցման ստուգման արդյունք
        /// </summary>
        public string HBCheckResult { get; set; }

        /// <summary>
        /// HBDocument-ի Ռեեստրով փոխանցման ստուգման արդյունք, True եթե ունի ԴԱՀԿ արգելանք,հակառակ դեպքում՝ False
        /// </summary>
        public bool HbDAHKCheckResult { get; set; }

        /// <summary>
        /// HBDocument-ի Ռեեստրով փոխանցման նկարագրություն
        /// </summary>
        public string TransactionDescription { get; set; }



        public string NameSurename { get; set; }



        public static DataTable ConvertAdditionalReestrDetailsToDataTable(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, Languages languages, long orderId = 0)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("u_ID");
            dt.Columns.Add("IndexID");
            dt.Columns.Add("docID");
            dt.Columns.Add("name_surname");
            dt.Columns.Add("credit_acc");
            dt.Columns.Add("amount");
            dt.Columns.Add("reason");
            dt.Columns.Add("del_status");
            dt.Columns.Add("bank_code");
            dt.Columns.Add("bank_name");

            if (reestrTransferAdditionalDetails != null)
            {
                foreach (ReestrTransferAdditionalDetails detail in reestrTransferAdditionalDetails)
                {
                    DataRow dr = dt.NewRow();
                    dr["u_ID"] = 0;
                    dr["IndexID"] = detail.Index;
                    dr["DocID"] = orderId;
                    dr["name_surname"] = detail.NameSurename.ToString();
                    dr["credit_acc"] = detail.CreditAccount.AccountNumber.ToString();
                    dr["amount"] = (double)detail.Amount;
                    dr["reason"] = !String.IsNullOrEmpty(detail.Description) ? detail.Description.ToString() : "";
                    dr["del_status"] = 0;
                    dr["bank_code"] = detail.CreditAccount.AccountNumber.Substring(0, 5).ToString();
                    dr["bank_name"] = Info.GetBank(Convert.ToInt32(detail.CreditAccount.AccountNumber.Substring(0, 5).ToString()), (Languages)languages);
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
    }

}
