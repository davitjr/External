using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    class SearchReceivedTransferDB
    {
        internal static List<SearchReceivedTransfer> GetReceivedTransfersDB(SearchReceivedTransfer searchParams, ushort FilialCode)
        {
            List<SearchReceivedTransfer> ReceivedTransfersList = new List<SearchReceivedTransfer>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    sql = " WHERE type = 2 And (id_fill =" + FilialCode.ToString() +" or id_fill =-1) and id_status<>6 and id_status<>7 and id_status<>8 and ntype= "+ searchParams.TransferType.ToString() ;

                    if (searchParams.TotalAmount != 0)
                    {
                        sql = sql + " and totalAmount = " + searchParams.TotalAmount.ToString();
                    }
                    if (!String.IsNullOrEmpty(searchParams.Currency ))
                    {
                        sql = sql + " and currency = '" + searchParams.Currency + "'";
                    }
                    if (!String.IsNullOrEmpty(searchParams.Code ))
                    {
                        sql = sql + " and controlNumber = '" + searchParams.Code + "'";
                    }
                    if (searchParams.DateTransfer != DateTime.MinValue)
                    {
                        sql = sql + " and  CAST(FLOOR(CAST(dateWestern AS float)) AS DATETIME) = '" + searchParams.DateTransfer.ToString("dd/MMM/yyyy") + "'  ";
                    }

                    cmd.CommandText = @" SELECT dateWestern,NameSender + isnull(' ' +LastNameSender, '')  NameSender,NameReceiver + isnull(' ' + LastNameReceiver,'') NameReceiver,(IDSender+','+IssuedByIDSender+','+convert(nvarchar(50),IssuedIDDateSender,103)) AS PassportReceiver,
                                                                                        amount,currency,controlNumber,CountrySender,chargeW,rateW,totalAmount,acbacommision,AddressSender,PhoneSender,AddressReceiver,PhoneReceiver, [file_name], CountryCodeN  , Profit_Percent
                                                          FROM Tbl_western W   left join  Tbl_Countries C on C.CountryName =W.CountrySender join  Tbl_TransferSystems S on  S.code =W.ntype  " + sql + " order by  dateWestern desc";
 
                  
                    conn.Open();

                       dt.Load(cmd.ExecuteReader());
                        int count = 0;
                        if (dt.Rows.Count >0)
                        {
                            ReceivedTransfersList = new List<SearchReceivedTransfer>();
                             
                       
                            //count = (int)dr["Rows"];
                            count = dt.Rows.Count;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                SearchReceivedTransfer oneResult = new SearchReceivedTransfer();
                         

                                oneResult.RowCount = count;
                                oneResult.DateTransfer = Convert.ToDateTime(dt.Rows[i]["dateWestern"]);
                                oneResult.SenderName = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["NameSender"].ToString());
                                oneResult.ReceiverName = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["NameReceiver"].ToString());
                                oneResult.ReceiverPassport = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["PassportReceiver"].ToString());
                                oneResult.Amount =Convert.ToDouble (dt.Rows[i]["amount"]);
                                oneResult.Currency = dt.Rows[i]["currency"].ToString();
                                oneResult.Code = dt.Rows[i]["controlNumber"].ToString();
                                oneResult.Country = dt.Rows[i]["CountrySender"].ToString();
                                oneResult.ChargeW =Convert.ToDouble (dt.Rows[i]["chargeW"]);
                                oneResult.RateW =Convert.ToDouble (dt.Rows[i]["rateW"]);
                                oneResult.TotalAmount = Convert.ToDouble(dt.Rows[i]["totalAmount"]);
                                oneResult.AcbaFee =Convert.ToDouble (dt.Rows[i]["acbacommision"]);
                                oneResult.ProfitPercent = Convert.ToDouble(dt.Rows[i]["Profit_Percent"]);
                                oneResult.SenderAddress = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["AddressSender"].ToString());
                                oneResult.SenderPhone = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["PhoneSender"].ToString());
                                oneResult.ReceiverAddress = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["AddressReceiver"].ToString());
                                oneResult.ReceiverPhone = Utility.ConvertAnsiToUnicodeRussian(dt.Rows[i]["PhoneReceiver"].ToString());
                                oneResult.CountryCode = dt.Rows[i]["CountryCodeN"].ToString();
                                oneResult.FileName = dt.Rows[i]["file_name"].ToString();

                                
                               
                                ReceivedTransfersList.Add(oneResult);
                            }

                        }

                  
                }
                return ReceivedTransfersList;
            }
        }
    }
}
