using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class ReceivedBankMailTransferDB
    {
        /// <summary>
        /// Վերադարձնում է  փոխանցումները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static List<ReceivedBankMailTransfer> GetList(TransferFilter filter, ACBAServiceReference.User user, ulong CustomerNumber = 0)
        {
            List<ReceivedBankMailTransfer> transferList = new List<ReceivedBankMailTransfer>();

            DataTable dt = new DataTable();



            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                List<SqlParameter> prms = new List<SqlParameter>();

                string str = " WHERE 1=1 ";


                using (SqlCommand cmd = new SqlCommand(str, conn))
                {



                    if (CustomerNumber != 0)
                    {
                        str += " and A.customer_number= " + CustomerNumber.ToString();
                    }

                    if (filter.DateFrom != default(DateTime))
                    {
                        str += " and DateGet>= '" + String.Format("{0:dd/MMM/yy}", filter.DateFrom) + "' ";

                    }

                    if (filter.DateTo != default(DateTime))
                    {
                        str += " and DateGet  <= '" + String.Format("{0:dd/MMM/yy}", filter.DateTo) + "' ";
                    }
                                        
                    if (filter.Status == 1)
                    {
                        str += " and TransOK= " + filter.Status.ToString();
                    }
                    else
                        if (filter.Status == 2)
                        {
                            str += " and TransOK=0 " ;
                        }

 
           
                    if (!String.IsNullOrEmpty(filter.Filial) )
                    {
                        if (filter.Filial == "99")
                            str += " and ([card_filial]>22000 or left(cast (cast(AccCredit as bigint ) as nvarchar), 5)>22000)";
                        else
                            str += " and ([card_filial]=" + filter.Filial + " or left(cast (cast(AccCredit as bigint ) as nvarchar), 5)=" + filter.Filial + ")";
                    }

                    if (!String.IsNullOrEmpty(filter.Ident))
                        str += " and ident='" + filter.Ident + "' ";

                    if (filter.Amount !=0)
                        str += " and amount>=" + filter.Amount;

                    if (filter.MaxAmount != 0)
                        str += " and amount<=" + filter.MaxAmount;

                    if (!String.IsNullOrEmpty (filter.Currency ))
                        str += " and Valuta='" + filter.Currency + "' ";
                   
                    if (!String.IsNullOrEmpty(filter.DebetBank))
                        str += " and substring(cast(cast( AccDebetCB as bigint) as nvarchar),1,3)='" + filter.DebetBank + "' ";

                    if (!String.IsNullOrEmpty(filter.DebetAccount ))
                        str += " and IsNull(AccDebetCb,0)=" + filter.DebetAccount  ;

                    if (!String.IsNullOrEmpty(filter.CreditAccount))
                        str += " and IsNull(AccCredit,0) =" + filter.CreditAccount  ;

                    if (!String.IsNullOrEmpty(filter.Description))
                        str += " and DescrPoxancym like '%" + filter.Description + "%' ";

                    if (filter.Verified!=0)
                        str += " and isnull(verified,1)=" + filter.Verified.ToString();

                    if (filter.UnknownTransfer  != 0)
                        str += " and unknown_transfer<>0";

                    if (!String.IsNullOrEmpty(filter.DescrOK ))
                        str += " and file_for_branch='" + filter.DescrOK + "' ";

                    if (filter.AMLCheck != 0)
                        str += "and CASE WHEN isnull(aml_check,0)=0 THEN CASE isnull(Verified_AML,0) WHEN 2 THEN 1 WHEN 3 THEN 2 ELSE 0 END  ELSE aml_check END =" + filter.AMLCheck.ToString();

                    if (filter.IsChecked != 0)
                        str += " and UserCode="+ filter.IsChecked.ToString();
                    cmd.CommandText = "[sp_get_out_transfer_list]";
                    cmd.CommandType = CommandType.StoredProcedure;
          
                    cmd.Parameters.Add("@wherecond", SqlDbType.NVarChar).Value = str;
                    cmd.Parameters.Add("@archive", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@transOK", SqlDbType.Bit).Value = 1;
                   
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        while (dr.Read())
                        {

                            ReceivedBankMailTransfer transfer = new ReceivedBankMailTransfer();

                            transfer.ID = Convert.ToUInt64(dr["ID"]);
                            transfer.DateGet = Convert.ToDateTime(dr["DateGet"]);
                            transfer.TimeGet = Convert.ToDateTime(dr["TimeGet"]);
                            transfer.FName = dr["F_Name"].ToString();
                            transfer.AccDebetCB = dr["AccDebetCB"].ToString();
                            transfer.DescrDebetCB =Utility.ConvertAnsiToUnicode (  dr["DescrDebetCB"].ToString());
                            transfer.AccDebet = dr["AccDebet"].ToString();
                            transfer.AccCredit = dr["AccCredit"].ToString();
                            transfer.DescrCredit =Utility.ConvertAnsiToUnicode ( dr["DescrCredit"].ToString());
                            transfer.DescrPoxancym =Utility.ConvertAnsiToUnicode (  dr["DescrPoxancym"].ToString());
                            transfer.DateTransfer = Convert.ToDateTime(dr["DatePoxancym"]);
                            transfer.Valuta = dr["Valuta"].ToString();
                            transfer.Amount = Convert.ToDouble(dr["amount"]);
                            transfer.TransOK = Convert.ToByte(dr["TransOK"]);
                            transfer.StrFirstLine = dr["strFirstLine"].ToString();
                            transfer.Editing = Convert.ToInt32(dr["ID"]);
                            transfer.DateTrans = Convert.ToDateTime(dr["DateTrans"]);
                            transfer.UserCode = Convert.ToInt32(dr["UserCode"]);
                            transfer.ForPrint = Convert.ToInt32(dr["For_Print"]);

                            if (dr["card_number"] != DBNull.Value)
                            {
                                transfer.CardNumber = dr["card_number"].ToString();
                            }

                            transfer.CardFilial =  Convert.ToInt16(dr["Card_Filial"]);
                            transfer.FileForBranch = dr["File_For_Branch"].ToString();
                            transfer.SocialNumber = dr["Social_Number"].ToString();
                            transfer.DescrDebet = Utility.ConvertAnsiToUnicode ( dr["DescrDebet"].ToString());
                            transfer.Verified = Convert.ToInt16(dr["Verified"]);
                            if (dr["Verifier_Set_Number"] != DBNull.Value)
                            {
                                transfer.VerifierSetNumber = Convert.ToInt16(dr["Verifier_Set_Number"]);
                            }

                            transfer.NotAutomatTrans = Convert.ToInt16(dr["NotAutomatTrans"]);

                            if (dr["Transactions_Group_number"] != DBNull.Value)
                            {
                                transfer.TransactionsGroupNumber = Convert.ToInt64(dr["Transactions_Group_number"]);
                            }

                            transfer.AmlCheck = Convert.ToInt16(dr["aml_check"]);
                            if (dr["aml_check_date"] != DBNull.Value)
                            {
                                transfer.AmlCheckDate =  Convert.ToDateTime(dr["aml_check_date"]);
                                  transfer.AmlCheckSetNumber =Convert.ToInt16(dr["aml_check_set_number"]);
                            }

                            transfer.VerifiedAML = Convert.ToInt16(dr["verified_AML"]);
                            if (dr["verifier_set_date_AML"] != DBNull.Value)
                            {
                                transfer.VerifierSetDateAML = Convert.ToDateTime(dr["verifier_set_date_AML"]);
                                transfer.VerifierSetNumber_AML = Convert.ToInt16(dr["verifier_set_number_AML"]);
                            }

                            transfer.Ident = dr["Ident"].ToString();
                            if (dr["unknown_reason"] != DBNull.Value)
                            {
                                transfer.UnknownReason = dr["unknown_reason"].ToString();
                            }

                            transfer.UnknownTransfer = Convert.ToInt16(dr["unknown_transfer"]);
                            transfer.UnknownTransferSend = Convert.ToInt16(dr["unknown_transfer_send"]);

                            if (dr["payment_code"] != DBNull.Value)
                            {
                                transfer.SenderType = dr["sender_type"].ToString();
                                transfer.ReceiverType = dr["receiver_type"].ToString();
                                transfer.AddInf = dr["add_inf"].ToString();
                                transfer.PaymentCode = dr["payment_code"].ToString();
                            }
                            if (dr["confirmation_date"] != DBNull.Value)
                                transfer.ConfirmationDate = Convert.ToDateTime(dr["confirmation_date"]);
                            if (dr["confirmation_set_number"] != DBNull.Value)
                                transfer.ConfirmationSetNumber = Convert.ToInt16(dr["confirmation_set_number"]);
                            if (dr["confirmation_time"] != DBNull.Value)
                                transfer.ConfirmationTime = (TimeSpan)dr["confirmation_time"];
                            if (dr["payment_order_reference_number"] != DBNull.Value)
                                transfer.PaymentOrderReferenceNumber = dr["payment_order_reference_number"].ToString();


                   
                           
 

                            transferList.Add(transfer);
                        }


                        if (dr.NextResult())
                        {
                            if (dr.Read() && transferList.Count != 0)
                                transferList.First().ListCount = Convert.ToUInt32(dr["qanak"]);
                        }

                    }

                }




            }



            return transferList;
        }




        /// <summary>
        /// Վերադարձնում է  փոխանցումը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static ReceivedBankMailTransfer Get(ReceivedBankMailTransfer transfer)
        {

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string str = @"SELECT  B.*,cast(AccDebetCB as bigint) as AccDebetCB_big ,A.type_of_account,A.type_of_account_new,P.product_type 				                								
				                     FROM  Tbl_Bank_Mail_Out B Left JOIN [Tbl_all_accounts;] A on B.AccCredit= A.arm_number 
                                                OUTER APPLY (SELECT min(type_of_product) as product_type FROM Tbl_define_sint_acc WHERE sint_acc_new=a.type_of_account_new) P
				                     WHERE  id= @id ";



                using SqlCommand cmd = new SqlCommand(str, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = transfer.ID;
                dt.Load(cmd.ExecuteReader());

                if (dt.Rows.Count != 0)
                {

                    transfer.ID = Convert.ToUInt64(dt.Rows[0]["ID"]);
                    transfer.DateGet = Convert.ToDateTime(dt.Rows[0]["DateGet"]);
                    transfer.TimeGet = Convert.ToDateTime(dt.Rows[0]["TimeGet"]);
                    transfer.FName = dt.Rows[0]["F_Name"].ToString();
                    transfer.AccDebetCB = dt.Rows[0]["AccDebetCB"].ToString();
                    transfer.DescrDebetCB = Utility.ConvertAnsiToUnicode(dt.Rows[0]["DescrDebetCB"].ToString());
                    transfer.AccDebet = dt.Rows[0]["AccDebet"].ToString();
                    transfer.AccCredit = dt.Rows[0]["AccCredit"].ToString();
                    transfer.DescrCredit = Utility.ConvertAnsiToUnicode(dt.Rows[0]["DescrCredit"].ToString());
                    transfer.DescrPoxancym = Utility.ConvertAnsiToUnicode(dt.Rows[0]["DescrPoxancym"].ToString());
                    transfer.DateTransfer = Convert.ToDateTime(dt.Rows[0]["DatePoxancym"]);
                    transfer.Valuta = dt.Rows[0]["Valuta"].ToString();
                    transfer.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                    transfer.TransOK = Convert.ToByte(dt.Rows[0]["TransOK"]);
                    transfer.StrFirstLine = dt.Rows[0]["strFirstLine"].ToString();
                    transfer.Editing = Convert.ToInt32(dt.Rows[0]["ID"]);
                    transfer.DateTrans = Convert.ToDateTime(dt.Rows[0]["DateTrans"]);
                    transfer.UserCode = Convert.ToInt32(dt.Rows[0]["UserCode"]);
                    transfer.ForPrint = Convert.ToInt32(dt.Rows[0]["For_Print"]);

                    if (dt.Rows[0]["card_number"] != DBNull.Value)
                    {
                        transfer.CardNumber = dt.Rows[0]["card_number"].ToString();
                    }

                    transfer.CardFilial = Convert.ToInt16(dt.Rows[0]["Card_Filial"]);
                    transfer.FileForBranch = dt.Rows[0]["File_For_Branch"].ToString();
                    transfer.SocialNumber = dt.Rows[0]["Social_Number"].ToString();
                    transfer.DescrDebet = Utility.ConvertAnsiToUnicode(dt.Rows[0]["DescrDebet"].ToString());
                    transfer.Verified = Convert.ToInt16(dt.Rows[0]["Verified"]);
                    if (dt.Rows[0]["Verifier_Set_Number"] != DBNull.Value)
                    {
                        transfer.VerifierSetNumber = Convert.ToInt16(dt.Rows[0]["Verifier_Set_Number"]);
                    }

                    transfer.NotAutomatTrans = Convert.ToInt16(dt.Rows[0]["NotAutomatTrans"]);

                    if (dt.Rows[0]["Transactions_Group_number"] != DBNull.Value)
                    {
                        transfer.TransactionsGroupNumber = Convert.ToInt64(dt.Rows[0]["Transactions_Group_number"]);
                    }

                    transfer.AmlCheck = Convert.ToInt16(dt.Rows[0]["aml_check"]);
                    if (dt.Rows[0]["aml_check_date"] != DBNull.Value)
                    {
                        transfer.AmlCheckDate = Convert.ToDateTime(dt.Rows[0]["aml_check_date"]);
                        transfer.AmlCheckSetNumber = Convert.ToInt16(dt.Rows[0]["aml_check_set_number"]);
                    }

                    transfer.VerifiedAML = Convert.ToInt16(dt.Rows[0]["verified_AML"]);
                    if (dt.Rows[0]["verifier_set_date_AML"] != DBNull.Value)
                    {
                        transfer.VerifierSetDateAML = Convert.ToDateTime(dt.Rows[0]["verifier_set_date_AML"]);
                        transfer.VerifierSetNumber_AML = Convert.ToInt16(dt.Rows[0]["verifier_set_number_AML"]);
                    }

                    transfer.Ident = dt.Rows[0]["Ident"].ToString();
                    if (dt.Rows[0]["unknown_reason"] != DBNull.Value)
                    {
                        transfer.UnknownReason = dt.Rows[0]["unknown_reason"].ToString();
                    }

                    transfer.UnknownTransfer = Convert.ToInt16(dt.Rows[0]["unknown_transfer"]);
                    transfer.UnknownTransferSend = Convert.ToInt16(dt.Rows[0]["unknown_transfer_send"]);

                    if (dt.Rows[0]["payment_code"] != DBNull.Value)
                    {
                        transfer.SenderType = dt.Rows[0]["sender_type"].ToString();
                        transfer.ReceiverType = dt.Rows[0]["receiver_type"].ToString();
                        transfer.AddInf = dt.Rows[0]["add_inf"].ToString();
                        transfer.PaymentCode = dt.Rows[0]["payment_code"].ToString();
                    }
                    if (dt.Rows[0]["confirmation_date"] != DBNull.Value)
                        transfer.ConfirmationDate = Convert.ToDateTime(dt.Rows[0]["confirmation_date"]);
                    if (dt.Rows[0]["confirmation_set_number"] != DBNull.Value)
                        transfer.ConfirmationSetNumber = Convert.ToInt16(dt.Rows[0]["confirmation_set_number"]);
                    if (dt.Rows[0]["confirmation_time"] != DBNull.Value)
                        transfer.ConfirmationTime = (TimeSpan)dt.Rows[0]["confirmation_time"];
                    if (dt.Rows[0]["payment_order_reference_number"] != DBNull.Value)
                        transfer.PaymentOrderReferenceNumber = dt.Rows[0]["payment_order_reference_number"].ToString();





                }

            }
            return transfer;
        }




    }
}
