using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    class TransferByCallDB
    {
        /// <summary>
        /// Զանգի հայտի պահպանում
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static ActionResult Save(TransferByCall transfer, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_transfers_by_call 
                                                    (
                                                            registration_date, 
                                                            transfer_system, 
                                                            amount, 
                                                            currency,
                                                            card_number,
                                                            account_number,
                                                            customer_number,
                                                            number_of_set,  
                                                            code_word,
                                                            call_time, 
                                                            filialcode,
                                                            contract_id,
                                                            Contact_phone,
                                                            department_id,
                                                            rate_sell,
                                                            rate_buy,
                                                            Registered_by
                                                                    )
                                                            VALUES
                                                           (
                                                            @registration_date,
                                                            @transfer_system,
                                                            @amount,
                                                            @currency,
                                                            @card_number,   
                                                            @account_number,
                                                            @customernumber,
                                                            @number_of_set,
                                                            @code_word,
                                                            @call_time,
                                                            @fillialcode,
                                                            @contract_id,
                                                            @contact_phone,
                                                            @department_id,
                                                            @rate_sell,
                                                            @rate_buy,
                                                            1
                                                            )
                                                            ", conn))
                {
                    cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@transfer_system", SqlDbType.SmallInt).Value = transfer.TransferSystem;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = transfer.Amount;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = transfer.Currency;
                    if (transfer.CardNumber == null)
                        transfer.CardNumber = "";

                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 20).Value = transfer.CardNumber;
                    cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = transfer.AccountNumber;
                    cmd.Parameters.Add("@number_of_set", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@code_word", SqlDbType.NVarChar, 20).Value = transfer.Code;
                    cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = transfer.CustomerNumber;
                    cmd.Parameters.Add("@call_time", SqlDbType.DateTime).Value = transfer.CallTime;
                    cmd.Parameters.Add("@fillialcode", SqlDbType.Int).Value = transfer.ContractFilialCode;
                    cmd.Parameters.Add("@contract_id", SqlDbType.Int).Value = transfer.ContractID;
                    cmd.Parameters.Add("@contact_phone", SqlDbType.NVarChar, 150).Value = transfer.ContactPhone;
                    cmd.Parameters.Add("@department_id", SqlDbType.Int).Value = user.DepartmentId;
                    if (transfer.RateBuy != 0)
                    {
                        cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = transfer.RateBuy;
                    }
                    else cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = DBNull.Value;
                    if (transfer.RateSell != 0)
                    {
                        cmd.Parameters.Add("@rate_sell", SqlDbType.Float).Value = transfer.RateSell;
                    }
                    else cmd.Parameters.Add("@rate_sell", SqlDbType.Float).Value = DBNull.Value;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
                   
            }
            return result;
        }
        /// <summary>
        /// Փոխանցման կոդի մինիմալ երկարություն
        /// </summary>
        /// <param name="transfersystem"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static int CodeMinLenght(int transfersystem)
        {
            int minlenght;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd =new SqlCommand(@"select code_length_min from Tbl_TransferSystems where code=@transfersystem", conn))
                {
                    cmd.Parameters.Add("@transfersystem", SqlDbType.SmallInt).Value = transfersystem;
                    minlenght = Convert.ToInt32(cmd.ExecuteScalar());
                }
                  
            }
            return minlenght; 
        }
        /// <summary>
        /// Փոխանցման կոդի մաքսիմալ երկարություն
        /// </summary>
        /// <param name="transfersystem"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static int CodeMaxLenght(int transfersystem)
        {
            int maxlenght;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd =new SqlCommand(@"select code_length_max from Tbl_TransferSystems where code=@transfertype", conn))
                {
                    cmd.Parameters.Add("@transfertype", SqlDbType.SmallInt).Value = transfersystem;
                    maxlenght = Convert.ToInt32(cmd.ExecuteScalar());
                }
                    
            }
            return maxlenght;
        }
        /// <summary>
        /// Հաճախորդի համաձայնագրի ստուգում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns>true եթե ունի կնքված համաձայնագիր</returns>
        internal static bool HasContract(ulong customerNumber)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select NULL from tbl_contracts_for_transfers_by_call where customer_number=@customernumber", conn))
                {
                    cmd.Parameters.Add("@customernumber", SqlDbType.Float).Value = customerNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        check = true;
                    }
                }
            }

            return check;
        }

         /// <summary>
        /// Նույն տվյալներով փոխանցման առկայության ստուգում
        /// </summary>
        internal static bool CheckExistingTransfer(TransferByCall transfer)
        {

            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT NULL FROM Tbl_transfers_by_call WHERE quality<>1 and cast(cast(call_time as nvarchar(11)) as smalldatetime) =@call_time and transfer_system =@transfer_system and code_word =@code_word And ID <>@Id", conn))
                {

                    cmd.Parameters.Add("@call_time", SqlDbType.DateTime).Value = transfer.CallTime.ToShortDateString();
                    cmd.Parameters.Add("@transfer_system", SqlDbType.SmallInt).Value = transfer.TransferSystem;
                    cmd.Parameters.Add("@code_word", SqlDbType.NVarChar, 20).Value = transfer.Code;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value =transfer.Id;
                    if (cmd.ExecuteReader().Read())
                    {
                        check = true;
                    }
                }
            }

            return check;
        }

        
        /// <summary>
        /// Վերադարձնում է մեկ զանգի հայտ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        internal static TransferByCall Get(long Id)
        {
            TransferByCall transfer = new TransferByCall();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT T.id,
                                                            T.registration_date, 
                                                            transfer_system, 
                                                            T.confirmation_date,
                                                            confirmation_2_date,
                                                            T.amount, 
                                                            T.currency,
                                                            T.card_number,
                                                            account_number,
                                                            T.customer_number,
                                                            number_of_set,  
                                                            code_word,
                                                            call_time, 
                                                            T.filialcode,
                                                            contract_id,
                                                            Contact_phone,
                                                            department_id,
                                                            rate_sell,
                                                            rate_buy ,                                                        
                                                            S.transfersystem,
                                                            B.confirmation_date + '  ' + isnull( CONVERT(datetime,B.confirmation_time),'') as transfer_confirm_date,
                                                            customer_amount,
                                                            regect_descr,
                                                            TQ.description,
                                                            Acc.currency as account_currency,
                                                            T.Confirmation_set_number,
                                                            confirmation_2_set_number ,
                                                            acc.description as acc_description,  
                                                            T.quality, 
                                                            isnull(registered_by,0) registered_by,
                                                            transfer_country,
                                                            transfer_sender_name,
                                                            T.amount_for_payment,
                                                            HB.source_type
                                                            FROM Tbl_transfers_by_call as T with (nolock) LEFT JOIN Tbl_TransferSystems as S on T.transfer_system=S.code
                                                            LEFT JOIN tbl_type_of_transfer_call_quality TQ ON T.quality=TQ.ID 
                                                            LEFT JOIN Tbl_Bank_Mail_IN B with (nolock)  ON  T.transfer_unic_number = B.unic_number and T.transfer_registration_date =B.registration_date 
                                                            LEFT JOIN [tbl_all_accounts;] Acc with (nolock)  ON T.account_number=Acc.arm_number
                                                            outer apply (Select source_type from dbo.TBl_HB_documents with (nolock)  where doc_id=T.doc_id) HB
                                                            WHERE T.id=@id", conn))
                {
                    cmd.Parameters.Add("@id",SqlDbType.Int).Value =Id;
                    dt.Load(cmd.ExecuteReader());
                    transfer = SetTransferByCall(dt.Rows[0]);
                }
            }
            return transfer;
        }
        /// <summary>
        /// Վերադարձնում է զանգով կատարված փոխանցումները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static TransferByCallList GetList(TransferByCallFilter filter)
        {
            TransferByCallList transferList = new TransferByCallList();
            TransferByCall transfer = new TransferByCall();
            List<TransferByCall> list = new List<TransferByCall>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                List<SqlParameter> prms = new List<SqlParameter>();

                string sqltextSelect = "SELECT Top 100 T.id,T.registration_date,transfer_system,T.confirmation_date,confirmation_2_date,T.amount,T.currency,T.card_number,account_number,T.customer_number,number_of_set,code_word,call_time,T.filialcode,contract_id," +
                                       "Contact_phone,department_id,rate_sell,rate_buy,S.transfersystem,B.confirmation_date + '  ' + isnull( CONVERT(datetime,B.confirmation_time),'') as transfer_confirm_date ,customer_amount,regect_descr,TQ.description,Acc.currency as account_currency, " +
                                       "T.Confirmation_set_number,confirmation_2_set_number,acc.description as acc_description, T.quality,  isnull(registered_by,0) registered_by, transfer_country, transfer_sender_name, T.amount_for_payment, HB.source_type ";

               string fromForSelect =  "FROM Tbl_transfers_by_call as T LEFT JOIN Tbl_TransferSystems as S on T.transfer_system=S.code " +
                                       "LEFT JOIN tbl_type_of_transfer_call_quality TQ ON T.quality=TQ.ID " +
                                       "LEFT JOIN Tbl_Bank_Mail_IN B ON  T.transfer_unic_number = B.unic_number and T.transfer_registration_date =B.registration_date " +
                                       "LEFT JOIN [tbl_all_accounts;] Acc ON T.account_number=Acc.arm_number  outer apply (Select source_type from dbo.TBl_HB_documents where doc_id=T.doc_id) HB " ;

                string fromForCount = " FROM Tbl_transfers_by_call as T ";


                string whereCondition = "WHERE deleting_date is null ";
                if (filter.CustomerNumber != 0)
                {
                    whereCondition += " and str(T.customer_number,12) like '%' + @customernumber ";
                    SqlParameter prm = new SqlParameter("@customernumber", SqlDbType.NVarChar);
                    prm.Value =  filter.CustomerNumber;
                    prms.Add(prm);
                }
                if (filter.Quality != TransferCallQuality.Undefined)
                {
                    whereCondition += " and T.quality=@quality";
                    SqlParameter prm = new SqlParameter("@quality", SqlDbType.Int);
                    prm.Value = (short)filter.Quality;
                    prms.Add(prm);
                }
                if (filter.StartDate != default(DateTime))
                {
                    whereCondition += " and cast(T.registration_date as DATE) >=@startdate";
                    SqlParameter prm = new SqlParameter("@startdate", SqlDbType.DateTime);
                    prm.Value = filter.StartDate;
                    prms.Add(prm);
                }
                if (filter.EndDate != default(DateTime))
                {
                    whereCondition += " and cast(T.registration_date as DATE) <=@enddate";
                    SqlParameter  prm = new SqlParameter("@enddate", SqlDbType.DateTime);
                    prm.Value = filter.EndDate;
                    prms.Add(prm);
 
                }

                if (filter.TransferSystem != 0)
                {
                    whereCondition += " and T.transfer_system=@transferSystem";
                    SqlParameter prm = new SqlParameter("@transferSystem", SqlDbType.SmallInt);
                    prm.Value = filter.TransferSystem;
                    prms.Add(prm);
 
                }

                if (filter.Currency != "" && filter.Currency!=null)
                {
                    whereCondition += " and T.currency=@currency";
                    SqlParameter prm = new SqlParameter("@currency", SqlDbType.NVarChar,3);
                    prm.Value = filter.Currency;
                    prms.Add(prm);

                }

                if (filter.Amount != 0)
                {
                    whereCondition += " and T.amount=@amount";
                    SqlParameter prm = new SqlParameter("@amount", SqlDbType.Float);
                    prm.Value = filter.Amount;
                    prms.Add(prm);

                }
                
                if (filter.RegistrationSetNumber != 0)
                {
                    whereCondition += " and number_of_set=@registrationSetNumber";
                    SqlParameter prm = new SqlParameter("@registrationSetNumber", SqlDbType.Float);
                    prm.Value = filter.RegistrationSetNumber;
                    prms.Add(prm);

                }

                if (filter.Filial != 0)
                {
                    whereCondition += " and T.FilialCode=@filialCode";
                    SqlParameter prm = new SqlParameter("@FilialCode", SqlDbType.SmallInt );
                    prm.Value = filter.Filial;
                    prms.Add(prm);

                }
                if (filter.CardNumber != "" && filter.CardNumber != null)
                {
                    whereCondition += " and T.card_number like '%' + @cardnumber ";
                    SqlParameter prm = new SqlParameter("@cardnumber", SqlDbType.NVarChar);
                    prm.Value = filter.CardNumber;
                    prms.Add(prm);
                }

                 if (filter.RegisteredBy != 0)
                {
                    if (filter.RegisteredBy == 1)
                        whereCondition += " and registered_by=1";
                    else
                        whereCondition += " and registered_by<>1";
                }              

                 sqltextSelect += fromForSelect+ whereCondition + " ORDER BY T.id  desc";

                 using (SqlCommand cmd = new SqlCommand(sqltextSelect, conn))
                 {

                     cmd.Parameters.AddRange(prms.ToArray());
                     dt.Load(cmd.ExecuteReader());

                     for (int i = 0; i < dt.Rows.Count; i++)
                     {
                         transfer = SetTransferByCall(dt.Rows[i]);
                         list.Add(transfer);
                     }
                     cmd.Parameters.Clear();
                 }

                sqltextSelect = "SELECT COUNT(*) as transferCount " +fromForCount +whereCondition;


                using (SqlCommand cmd1 = new SqlCommand(sqltextSelect, conn))
                {
                   
                    cmd1.Parameters.AddRange(prms.ToArray());
                    using (SqlDataReader dr = cmd1.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            transferList.TransferCount = Convert.ToInt64(dr["transferCount"]);
                        }
                    }
                                  
                }
            }

            transferList.TransferList = list;

            return transferList;
        }
        
        internal static TransferByCall SetTransferByCall(DataRow row)
        {
            TransferByCall transfer = new TransferByCall();
            if (row != null)
            {
                transfer.Id = Convert.ToInt64(row["id"]);
                if (row["Confirmation_date"] != DBNull.Value)
                {
                    transfer.ConfirmationDate = Convert.ToDateTime(row["Confirmation_date"]);
                }
                 if (row["confirmation_2_date"] != DBNull.Value)
                {
                    transfer.ConfirmationDate2 = Convert.ToDateTime(row["confirmation_2_date"]);
                }
                 if (row["transfer_confirm_date"] != DBNull.Value)
                {
                    transfer.TransferConfirmationDate = Convert.ToDateTime(row["transfer_confirm_date"]);
                }
                transfer.TransferSystemDescription = row["transfersystem"].ToString();
                transfer.AccountNumber = row["account_number"].ToString();
                transfer.Amount = Convert.ToDouble(row["amount"]);
      
                if (row["amount_for_payment"] != DBNull.Value)
                {
                    transfer.AmountForPayment = Convert.ToDouble(row["amount_for_payment"]);
                }
                transfer.CustomerAmount = (row["customer_amount"] != DBNull.Value) ? Convert.ToDouble(row["customer_amount"]) : default(double); 
                transfer.CallTime = (DateTime)row["call_time"];
                transfer.CardNumber = row["card_number"].ToString();
                transfer.Code = row["code_word"].ToString();
                transfer.ContactPhone = row["Contact_phone"].ToString();
                transfer.ContractFilialCode = Convert.ToUInt16(row["filialcode"]);
                transfer.ContractID = Convert.ToUInt64(row["contract_id"]);
                transfer.Currency = row["currency"].ToString();
                transfer.AccountCurrency = row["account_currency"].ToString();                
                transfer.CustomerNumber = Convert.ToUInt64(row["customer_number"]);
                transfer.RateBuy = (row["rate_buy"] != DBNull.Value) ? Convert.ToDouble(row["rate_buy"]) : default(double);
                transfer.RateSell = (row["rate_sell"] != DBNull.Value) ?  Convert.ToDouble(row["rate_sell"]) : default(double);
                transfer.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                transfer.TransferSystem =Convert.ToInt16(row["transfer_system"]);
                transfer.RegistrationSetNumber = Convert.ToInt64(row["number_of_set"]);
                transfer.RegectDescription =Utility.ConvertAnsiToUnicode(row["regect_descr"].ToString());
                transfer.TransferQualityDescription =Utility.ConvertAnsiToUnicode(row["description"].ToString());
                transfer.ConfirmationSetNumber = (row["Confirmation_set_number"] != DBNull.Value) ? Convert.ToInt64(row["Confirmation_set_number"]) : default(Int64);
                transfer.ConfirmationSetNumber2 = (row["confirmation_2_set_number"] != DBNull.Value) ? Convert.ToInt64(row["confirmation_2_set_number"]) : default(Int64);
                transfer.AccountDescription = Utility.ConvertAnsiToUnicode(row["acc_description"].ToString());
                transfer.Quality  = Convert.ToUInt16(row["quality"]);
                transfer.RegisteredBy = Convert.ToInt16(row["registered_by"]);
                if (row["transfer_country"] != DBNull.Value)
                {
                    transfer.Country = row["transfer_country"].ToString();
                }
                if (row["transfer_sender_name"] != DBNull.Value)
                {
                    transfer.Sender =   Utility.ConvertAnsiToUnicodeRussian(row["transfer_sender_name"].ToString());
                }
                if(row["source_type"] != DBNull.Value)
                {
                    transfer.Source = (SourceType)int.Parse(row["source_type"].ToString());
                }
 

                 
                
            }
            return transfer;
        }
      
        /// <summary>
        /// Ստուգում է գոյություն ունի նշված տեսակի ակտիվ փոխանցման համակարգ
        /// </summary>
        /// <param name="transferSystemCode"></param>
        /// <returns></returns>
        internal static bool  CheckActiveTransferSystem(int transferSystemCode)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT transfersystem FROM Tbl_TransferSystems  WHERE  Instant_money_transfer=1 AND isactive=1 AND code=@transferSystemCode", conn))
                {
                    cmd.Parameters.Add("@transferSystemCode",SqlDbType.Int).Value = transferSystemCode;
                    if (cmd.ExecuteReader().Read())
                    {
                        check = true;
                    }
                }
            }
            return check;
 
        }

        /// <summary>
        /// Փոխանցումը ուղարկել վճարման
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static ActionResult SendTransfeerCallForPay(ulong transferID, ACBAServiceReference.User user, DateTime SetDate)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_received_call_fast_transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@callTransferID", SqlDbType.Int).Value = transferID;
                    cmd.Parameters.Add("@setDate", SqlDbType.SmallDateTime).Value = SetDate;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = user.filialCode;
                    cmd.Parameters.Add("@programm", SqlDbType.TinyInt ).Value = 1;
 

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }


        internal static ActionResult ChangeSave(TransferByCallChangeOrder changeOrder, string userName, SourceType source,ushort isCallCenter)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_submit_received_transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = changeOrder.CustomerNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = changeOrder.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@document_type", SqlDbType.Int).Value = (short)changeOrder.Type;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = changeOrder.Id;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = 0;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.SmallInt).Value = changeOrder.FilialCode;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = changeOrder.Description;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = (short)changeOrder.SubType;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = changeOrder.OperationDate;
                    cmd.Parameters.Add("@transferByCallID", SqlDbType.Int).Value = changeOrder.ReceivedFastTransfer.TransferByCallID ;
                    cmd.Parameters.Add("@transfer_contract_id", SqlDbType.BigInt).Value = changeOrder.ReceivedFastTransfer.ContractId;
                    cmd.Parameters.Add("@registerBy", SqlDbType.TinyInt).Value = isCallCenter;
                    if (changeOrder.SubType==3)
                    {
                        cmd.Parameters.Add("@reason_type_description", SqlDbType.NVarChar, 500).Value = changeOrder.ReasonTypeDescription;
                        cmd.Parameters.Add("@reason_type", SqlDbType.SmallInt).Value = changeOrder.ReasonType;
                    }

                    if (changeOrder.SubType == 5 || changeOrder.SubType == 1)
                    {
                        cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = changeOrder.ReceivedFastTransfer.Currency;
                        cmd.Parameters.Add("@amount", SqlDbType.Float).Value = changeOrder.ReceivedFastTransfer.Amount;
                        cmd.Parameters.Add("@sender_name", SqlDbType.NVarChar, 100).Value = changeOrder.ReceivedFastTransfer.Sender;
                        cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = changeOrder.ReceivedFastTransfer.Code;
                     //   cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 50).Value = changeOrder.ReceivedFastTransfer.ReceiverPassport;
                      //  cmd.Parameters.Add("@receiver_account", SqlDbType.NVarChar, 50).Value = changeOrder.ReceivedFastTransfer.ReceiverAccount.AccountNumber;
                        cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 50).Value = changeOrder.ReceivedFastTransfer.Country;
                      //  cmd.Parameters.Add("@receiver_name", SqlDbType.NVarChar, 255).Value = changeOrder.ReceivedFastTransfer.Receiver;
                        cmd.Parameters.Add("@fee", SqlDbType.NVarChar, 400).Value = changeOrder.ReceivedFastTransfer.Fee;
                        cmd.Parameters.Add("@fee_ACBA", SqlDbType.NVarChar, 400).Value = changeOrder.ReceivedFastTransfer.FeeAcba;
                        
                        if (changeOrder.ReceivedFastTransfer.ConvertationRate != 0 && changeOrder.ReceivedFastTransfer.ConvertationRate1 != 0)
                        {
                            cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = changeOrder.ReceivedFastTransfer.ConvertationRate;
                            cmd.Parameters.Add("@rate_sell", SqlDbType.Float).Value = changeOrder.ReceivedFastTransfer.ConvertationRate1;
                        }
                        else if (changeOrder.ReceivedFastTransfer.ConvertationRate != 0)
                        {
                            cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = changeOrder.ReceivedFastTransfer.ConvertationRate;
                        }
                        else if (changeOrder.ReceivedFastTransfer.ConvertationRate1 != 0)
                        {
                            cmd.Parameters.Add("@rate_buy", SqlDbType.Float).Value = changeOrder.ReceivedFastTransfer.ConvertationRate1;
                        }

                    
                    }
      
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    changeOrder.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = changeOrder.Id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return result;
                }
            }

        }


        public static ActionResult SaveTransferByCallChangeOrderDetails(TransferByCallChangeOrder transferByCallChangeOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_payment_received_fast_transfer";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@transfer_system", SqlDbType.TinyInt).Value = transferByCallChangeOrder.ReceivedFastTransfer.SubType;
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 20).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.Code ?? DBNull.Value;

                    cmd.Parameters.Add("@senderName", SqlDbType.NVarChar, 100).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.Sender ?? DBNull.Value;


                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 250).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.Receiver ?? DBNull.Value;
                    cmd.Parameters.Add("@receiver_passport", SqlDbType.NVarChar, 250).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.ReceiverPassport ?? DBNull.Value;

                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 50).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.Country ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_in_currency", SqlDbType.Float).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.Fee ?? DBNull.Value;

                    cmd.Parameters.Add("@fee_Acba", SqlDbType.Float).Value = (object)transferByCallChangeOrder.ReceivedFastTransfer.FeeAcba ?? DBNull.Value;


                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static List<short> CheckForChange(TransferByCallChangeOrder transferChange, ushort isCallCenter)
        {
            List<short> errors = new List<short>();
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string str = "pr_check_for_transfer_by_call_change";

                using (cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = str;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@callTransferID", SqlDbType.BigInt).Value = transferChange.ReceivedFastTransfer.TransferByCallID;
                    cmd.Parameters.Add("@return_error_codes", SqlDbType.TinyInt).Value = 1;
                    cmd.Parameters.Add("@isCallCenter", SqlDbType.TinyInt).Value = isCallCenter;
                    cmd.Parameters.Add("@documentSubType", SqlDbType.TinyInt).Value = transferChange.SubType;
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        errors.Add(short.Parse(dr["code"].ToString()));
                    }
                }

            }
            return errors;

        }

        internal static Dictionary<string, string> GetCallTransferRejectionReasons()
        {
            Dictionary<string, string> reasons = new Dictionary<string, string>();

            using (SqlConnection conn=new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd=new SqlCommand (@"select ID ,LEFT(description_Arm,100) AS description_Arm   from tbl_Call_transfer_rejection_reasons", conn))
                {
                    using (SqlDataReader dapt=cmd.ExecuteReader())
                    {
                        while (dapt.Read())
                        {
                            string key = dapt["id"].ToString();
                            string value = dapt["description_Arm"].ToString();
                            reasons.Add(key, value);
                        }
                    }
                }
            }
            return reasons;
        }

        //9772 Սոցիալական ապահովության հաշիվը կամ ACBA Pension կենսաթոշակային քարտի հաշվեհամար
        internal static Boolean IsPensionAccount(string AccountNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "Select [dbo].[IsPensionAccount](@accountNumber)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = Convert.ToDouble(AccountNumber);
                    conn.Open();
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }

        }

        //9772 Անհատ ձեռնարկատիրոջ հաշիվ
        internal static Boolean IsPrivateEntrepreneurAccount(string AccountNumber)
        {
            Boolean result = false;
            short typeOfClient = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "Select type_of_client From Tbl_Customers " +
                                                        "Where customer_number = " +
                                                        "(Select customer_number From[tbl_all_accounts;] Where Arm_number = @accountNumber)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = Convert.ToDouble(AccountNumber);
                    conn.Open();
                    // typeOfClient = (short)cmd.ExecuteScalar();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Int16.TryParse(dr["type_of_client"].ToString(), out typeOfClient);
                        }
                    }
                    result = typeOfClient == 2;
                }
            }
            return result;
        }
    }
}
