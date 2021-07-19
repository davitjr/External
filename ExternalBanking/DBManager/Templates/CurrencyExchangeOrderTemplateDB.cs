using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager.Templates
{
    internal class CurrencyExchangeOrderTemplateDB
    {
        internal static ActionResult CurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplate template, Action action)
        {
            ActionResult result = new ActionResult();
            int id = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_payment_order_template";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@tmpl_Name", SqlDbType.NVarChar).Value = template.TemplateName;

                    string creditAccountNumber = "";


                    if (template.CurrencyExchangeOrder.ReceiverBankCode == 10300 && template.CurrencyExchangeOrder.ReceiverAccount.AccountNumber.ToString().Substring(0, 1) == "9")
                    {
                        creditAccountNumber = template.CurrencyExchangeOrder.ReceiverAccount.AccountNumber.ToString();
                    }
                    else
                    {
                        creditAccountNumber = template.CurrencyExchangeOrder.ReceiverAccount.AccountNumber.ToString().Substring(5);
                    }


                  
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = template.TemplateCustomerNumber;

                    cmd.Parameters.Add("@debit_acc", SqlDbType.Float).Value = template.CurrencyExchangeOrder.DebitAccount.AccountNumber;

                    cmd.Parameters.Add("@credit_acc", SqlDbType.VarChar).Value = creditAccountNumber;
                    cmd.Parameters.Add("@credit_bank_code", SqlDbType.VarChar).Value = template.CurrencyExchangeOrder.ReceiverBankCode.ToString();
                    cmd.Parameters.Add("@type", SqlDbType.SmallInt).Value = template.TemplateType;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = template.TemplateSourceType;
                    cmd.Parameters.Add("@id_of_template", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = template.CurrencyExchangeOrder.Amount;
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = template.ChangeUserId;
                    cmd.Parameters.Add("@use_credit_line", SqlDbType.Bit).Value = template.CurrencyExchangeOrder.UseCreditLine;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = template.CurrencyExchangeOrder.SubType;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = template.CurrencyExchangeOrder.Type;
                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = template.TemplateGroupId;


                    if (template.CurrencyExchangeOrder.CreditorDocumentType == 1 && template.CurrencyExchangeOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_SocNoSoc", SqlDbType.NVarChar).Value = template.CurrencyExchangeOrder.CreditorDocumentNumber;
                    }
                    else if (template.CurrencyExchangeOrder.CreditorDocumentType == 2 && template.CurrencyExchangeOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_NoSoc_Number", SqlDbType.NVarChar).Value = template.CurrencyExchangeOrder.CreditorDocumentNumber;
                    }
                    else if (template.CurrencyExchangeOrder.CreditorDocumentType == 3 && template.CurrencyExchangeOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@Debitor_Passport_Number", SqlDbType.NVarChar).Value = template.CurrencyExchangeOrder.CreditorDocumentNumber;
                    }

                    else if (template.CurrencyExchangeOrder.CreditorDocumentType == 4 && template.CurrencyExchangeOrder.CreditorDocumentNumber != null)
                    {
                        cmd.Parameters.Add("@DebitorHVHH", SqlDbType.NVarChar).Value = template.CurrencyExchangeOrder.CreditorDocumentNumber;
                    }



                    if (template.CurrencyExchangeOrder.CreditorStatus != 0)
                    {
                        cmd.Parameters.Add("@SintAccDetailsValue", SqlDbType.Int).Value = template.CurrencyExchangeOrder.CreditorStatus;
                    }

                    if (template.CurrencyExchangeOrder.CreditorDescription != null)
                    {
                        cmd.Parameters.Add("@DebitorName", SqlDbType.NVarChar).Value = template.CurrencyExchangeOrder.CreditorDescription;
                    }

                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = template.ID });

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                    template.ID = id;

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
            }

            return result;
        }

        public static void SaveCurrencyExchangeOrderTemplateDetails(CurrencyExchangeOrderTemplate template, string userName, SourceType source)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"Insert Into tbl_convertation_template_details 
                        ( 
                            template_id,
                            amount_amd,
                            amount_cross,
                            short_change,
                            rounding_direction,
                            order_number_for_debet,
                            order_number_for_credit,
                            order_number_for_short_change,
                            is_variation,
                            cross_rate_full,
                            unique_number) 
                            VALUES
                        (
                            @templateId,
                            @amountInAmd,
                            @amountInCrossCurrency,
                            @shortChange,
                            @roundingDirection,
                            @orderNumberForDebet,
                            @orderNumberForCredit,
                            @orderNumberForShortChange,
                            @isVariation,
                            @crossRateFull,
                            @uniqueNumber
                        )";

                    cmd.Parameters.Add("@templateId", SqlDbType.Int).Value = template.ID;
                    cmd.Parameters.Add("@amountInAmd", SqlDbType.Float).Value = template.CurrencyExchangeOrder.AmountInAmd;
                    cmd.Parameters.Add("@amountInCrossCurrency", SqlDbType.Float).Value = template.CurrencyExchangeOrder.AmountInCrossCurrency;
                    cmd.Parameters.Add("@shortChange", SqlDbType.Float).Value = template.CurrencyExchangeOrder.ShortChange;
                    cmd.Parameters.Add("@roundingDirection", SqlDbType.TinyInt).Value = template.CurrencyExchangeOrder.RoundingDirection;
                    cmd.Parameters.Add("@orderNumberForDebet", SqlDbType.NVarChar, 20).Value = template.CurrencyExchangeOrder.OrderNumberForDebet == null ? "" : template.CurrencyExchangeOrder.OrderNumberForDebet;
                    cmd.Parameters.Add("@orderNumberForCredit", SqlDbType.NVarChar, 20).Value = template.CurrencyExchangeOrder.OrderNumberForCredit == null ? "" : template.CurrencyExchangeOrder.OrderNumberForCredit;
                    cmd.Parameters.Add("@orderNumberForShortChange", SqlDbType.NVarChar, 20).Value = template.CurrencyExchangeOrder.OrderNumberForShortChange == null ? "" : template.CurrencyExchangeOrder.OrderNumberForShortChange;
                    cmd.Parameters.Add("@isVariation", SqlDbType.Bit).Value = template.CurrencyExchangeOrder.IsVariation;
                    cmd.Parameters.Add("@crossRateFull", SqlDbType.Float).Value = template.CurrencyExchangeOrder.ConvertationCrossRate;
                    cmd.Parameters.Add("@uniqueNumber", SqlDbType.BigInt).Value = template.CurrencyExchangeOrder.UniqueNumber;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void GetCurrencyExchangeOrderTemplateDetails(CurrencyExchangeOrderTemplate template)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM tbl_convertation_details where doc_ID=@docId";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = template.CurrencyExchangeOrder.Id;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            template.CurrencyExchangeOrder.AmountInAmd = Convert.ToDouble(dr["amount_amd"]);
                            template.CurrencyExchangeOrder.AmountInCrossCurrency = Convert.ToDouble(dr["amount_cross"]);
                            template.CurrencyExchangeOrder.ShortChange = Convert.ToDouble(dr["short_change"]);
                            template.CurrencyExchangeOrder.RoundingDirection = (ExchangeRoundingDirectionType)Convert.ToByte(dr["rounding_direction"]);

                            if (dr["order_number_for_debet"] != DBNull.Value)
                            {
                                template.CurrencyExchangeOrder.OrderNumberForDebet = dr["order_number_for_debet"].ToString();
                            }

                            if (dr["order_number_for_credit"] != DBNull.Value)
                            {
                                template.CurrencyExchangeOrder.OrderNumberForCredit = dr["order_number_for_credit"].ToString();
                            }

                            if (dr["order_number_for_short_change"] != DBNull.Value)
                            {
                                template.CurrencyExchangeOrder.OrderNumberForShortChange = dr["order_number_for_short_change"].ToString();
                            }

                            if (dr["is_variation"] != DBNull.Value)
                            {
                                template.CurrencyExchangeOrder.IsVariation = (ExchangeRateVariationType)(dr["is_variation"]);
                            }

                            if (dr["cross_rate_full"] != DBNull.Value)
                            {
                                template.CurrencyExchangeOrder.ConvertationCrossRate = Convert.ToDouble(dr["cross_rate_full"]);
                            }

                        }
                    }

                }
            }
        }
    }
}
