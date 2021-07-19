using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    // class PeriodicTransfer նկարագրում է ՊԱՐԲԵՐԱԿԱՆ ՓՈԽԱՆՑՈՒՄ 
    /// </summary>
	public class PeriodicTransfer
    {
        /// <summary>
        ///  Պարբերական փոխանցման ունիկալ համար (App_ID)
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման փաստաթղթի համար 
        /// </summary>
        public ulong DocumentNumber { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման տեսակ 
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման տեսակի նկարագրություն  
        /// </summary>
        public string TypeDescription { get; set; }


        /// <summary>
        /// Պարբերական փոխանցման նկարագրություն (տեկստը հաճախորդի կողմից)   
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման ելքագրվող հաշիվ (դեբետ)
        /// </summary>
        public Account DebitAccount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման բանկի ներսում մուտքագրվող հաշիվ (կրեդիտ)
        /// </summary>
        public Account CreditInternalAccount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման բանկի մուտքագրվող հաշիվ բանկի ներսում կամ արտաքին (կրեդիտ)
        /// </summary>
        public string CreditAccount { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման ժամկետի տեսակ
        ///  1 - անժամկետ, 2 - ունի ժամկետ
        /// </summary>
        public int DurationType { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման ժամկետի տեսակի նկարագրություն
        /// </summary>
        public string DurationTypeDescription { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման սկզբի ամսաթիվ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման վերջի ամսաթիվ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման գումարի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման գումարի գանձման եղանակ
        ///  անբողջ պարտք
        ///  անբողջ մնացորդ
        ///  ֆիքսված գումար
        /// </summary>
        public int ChargeType { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման գումարի գանձման եղանակի նկարագրություն
        /// </summary>
        public string ChargeTypeDescription { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման գումար (եթե ֆիքսված գումարը)
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման աբոնենտի համար (կոմունալ վճարումների դեպքում)
        /// </summary>
        public string AbonentNumber { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման աբոնենտի լրացուցիչ տվյալները`
        /// (մասնաճյուղի կոդ ` եթե աբոնենտի համարը ունիկալ չէ,
        /// կամ աբոնենտի անուն (կոմունալ վճարումների դեպքում)
        /// Ունի կապված հատկություններ ` AbonentFilialCode, AbonentCityCode, AbonentName
        /// </summary>
        public string AbonentAddInf { get; set; }

        /// Պարբերական փոխանցման աբոնենտի լրացուցիչ տվյալները`
        /// (մասնաճյուղի կոդ ` եթե աբոնենտի համարը ունիկալ չէ,
        /// կամ աբոնենտի անուն (կոմունալ վճարումների դեպքում)
        /// Ունի կապված հատկություններ ` AbonentFilialCode, AbonentCityCode, AbonentName
        /// </summary>
        /// 

        /// Պարբերական փոխանցման աբոնենտի մասնաճյուղի կոդ`
        /// (մասնաճյուղի կոդ ` եթե աբոնենտի համարը ունիկալ չէ)
        /// Գոյություն ունի միայն եթե transder_type = 3,4,5,6 
        /// ENA,GASPROM,ARMWATER,ERJUR
        /// </summary>
        public string AbonentFilialCode
        {
            get
            {
                if (this.Type == 3 || this.Type == 4 || this.Type == 5 || this.Type == 6)
                    return this.AbonentAddInf;
                else
                    return "".ToString();
            }
        }

        /// Պարբերական փոխանցման աբոնենտի քաղակի կոդ`
        /// (մասնաճյուղի կոդ ` եթե աբոնենտի համարը ունիկալ չէ)
        /// Գոյություն ունի միայն եթե transder_type = 9 
        /// ԱՂԲ
        /// </summary>
        public string AbonentCityCode
        {
            get
            {
                if (this.Type == 9)
                    return this.AbonentAddInf;
                else
                    return "".ToString();
            }
        }

        /// Պարբերական փոխանցման աբոնենտի անուն`
        /// (մասնաճյուղի կոդ ` եթե աբոնենտի համարը ունիկալ չէ)
        /// Գոյություն ունի միայն եթե transder_type = 11,12 
        /// UCOM,Telephone
        /// </summary>
        public string AbonentName
        {
            get
            {
                if (this.Type == 11 || this.Type == 12)
                    return this.AbonentAddInf;
                else
                    return "".ToString();
            }
        }

        /// <summary>
        /// Պարբերական փոխանցման պլանային առաջին փոխանցման օր
        /// </summary>
        public DateTime FirstTransferDate { get; set; }


        /// <summary>
        /// Պարբերական փոխանցման վեջին կատարման ամսաթիվը
        /// </summary>
        public DateTime? LastOperationDate { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պարբերականությունը` քանի օր մեկ է կատարվում
        /// </summary>
        public int Periodicity { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պատկանելիության մասնաճյուղի կոդը
        /// </summary>
        public ulong FilialCode { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման մասնակի կատարման կամ միայն լրիվ գումարի առկայության դեպքում կատարման նիշը
        /// </summary>
        public byte PartialPaymentSign { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման հնարավոր մաքսիմալ գումարը (նշվում է հաճախորդի կողմից), 
        /// եթե պարտքը մեծ է նշված գումարից, փոխանցումը չի կատարվում 
        /// </summary>
        public double MaxAmountLevel { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման հնարավոր մինիմալ գումարը (նշվում է հաճախորդի կողմից կամ մատակարարի պահանջը), 
        /// եթե պարտքը փոքր է նշված գումարից, փոխանցումը չի կատարվում 
        /// </summary>
        public double MinAmountLevel { get; set; }

        /// <summary>
        /// Պարբերական փոխանցում կատարելուց հետո դեբետագրվող հաշվի պարտադիր մինիմալ մնացորդը,
        /// որը պետք է մնա հաշվի վրա (նշվում է հաճախորդի կողմից), 
        /// եթե մնացորդը հնարաոր չէ ապահովել, փոխանցումը չի կատարվում 
        /// </summary>
        public double MinDebetAccountRest { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման գումարի ենթատեսակը ըստ մուծման նպատակի:
        /// Օրինակ` գազի վճարումը կարա ուղղվի տեխ. սպասարկման վճարի կամ սպառած գազի դիմաց պարտքի մուծմանը 
        /// Գոյություն ունի միայն որոշ պարբերական փոխանցումների համար
        /// </summary>
        public byte AmountSubTypeByPurpose { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման գումարի ենթատեսակի նկարագրություն
        /// </summary>
        public string AmountSubTypeByPurposeDescription { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման միջնորդավճար գանձելու համար հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ուղարկողի ՀՎՀՀ
        /// 8 նիշ
        /// </summary>
        public string SenderCodeOfTax { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ուղարկողի ռեգիոնի կոդը (ՏՀՏ կոդ)
        /// </summary>
        public ulong SenderRegionCode { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ստացող բանկի կոդ  
        /// </summary>
        public ulong ReceiverBankCode { get; set; }

        /// <summary>
        /// SWIFT քաղվածքի պարբերական փոխանցման ատացողի ստացող բանկի SWIFT կոդ
        /// 11 նիշ
        /// </summary>
        public string ReceiverBankSwiftCode { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ստացողի անվանումը  
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ատացողի ՀՎՀՀ
        /// 8 նիշ
        /// </summary>
        public string ReceiverCodeOfTax { get; set; }


        /// <summary>
        /// BANK MAIL պարբերական փոխանցման նպատակի կոդը
        /// </summary>
        public int TransferPurposeCode { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման նպատակի նկարագրությունը
        /// </summary>
        public string TransferPurposeDescription { get; set; }

        /// <summary>
        /// BANK MAIL պարբերական փոխանցման ոստիկանության կոդը
        /// </summary>
        public int TransferPoliceCode { get; set; }

        /// <summary>
        /// Հաշվի վրա գումարը չլինելու պատճառով պարբերական փոխանցման կատարման համար
        /// հաճախորդի կողմից նախատեսված օրերի քանակը 
        /// </summary>
        public ushort CheckDaysCount { get; set; }

        /// <summary>
        /// Կոմունալ վճարման պարբերականի համար` պարտք չլինելու դեպքում կատարել թե ոչ
        /// Կատարման դեպքում ձևավորվում է կանխավճար 
        /// 1 - կատարել, 0 -ոչ
        /// </summary>
        public ushort PayIfNoDebt { get; set; }

        /// <summary>
        /// HOME Banking(Mobile) գործարքի կոդ, որի համաձայն մուտքագվել է պարբերական փոխանցումը 
        /// </summary>
        public ulong HBDocID { get; set; }

        /// <summary>
        /// HOME Banking(Mobile) գործարքի կոդ, որի համաձայն դադարեցվել է պարբերական փոխանցումը 
        /// </summary>
        public ulong TerminationHBDocID { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման համար
        /// </summary>
        public ulong Number { get; set; }

        /// <summary>
        /// Կրեդիտ հաշվի նկարագրություն
        /// </summary>
        public string CrediAccountDescription { get; set; }

        /// <summary>
        /// Շրջանառության առկայություն
        /// </summary>
        public bool ExistenceOfCirculation { get; set; }


        /// <summary>
        ///  Պարբերական փոխանցման վերջի ամսաթիվ
        /// </summary>
        public DateTime? EditingDate { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման ենթատեսակ (Ներբանկային փոխանցման դեպքում երբ SubType = 1՝ փոխանցում հաշիվների միջև, 2՝ ՀՀ տարացքում)
        /// </summary>
        public byte SubType { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման ենթատեսակ (Ներբանկային փոխանցման դեպքում երբ SubType = 1՝ փոխանցում հաշիվների միջև, 2՝ ՀՀ տարացքում)
        /// </summary>
        public byte AbonentType { get; set; }

        public PeriodicTransfer()
        {
            DebitAccount = new Account();
        }

        public static PeriodicTransfer GetPeriodicTransfer(ulong productId)
        {
            return PeriodicTransferDB.GetPeriodicTransfer(productId);
        }

        public static PeriodicTransfer GetPeriodicTransfer(ulong productId, ulong customerNumber)
        {
            return PeriodicTransferDB.GetPeriodicTransfer(productId, customerNumber);
        }

        public static List<PeriodicTransfer> GetPeriodicTransfers(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PeriodicTransfer> transfers = new List<PeriodicTransfer>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                transfers.AddRange(PeriodicTransferDB.GetPeriodicTransfers(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                transfers.AddRange(PeriodicTransferDB.GetClosedPeriodicTransfers(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                transfers.AddRange(PeriodicTransferDB.GetPeriodicTransfers(customerNumber));
                transfers.AddRange(PeriodicTransferDB.GetClosedPeriodicTransfers(customerNumber));
            }
            return transfers;
        }
        public List<PeriodicTransferHistory> GetHistory(long ProductId, DateTime dateFrom, DateTime dateTo)
        {
            return PeriodicTransferDB.GetHistory(ProductId, dateFrom, dateTo);
        }


        public void AddPeriodicTransferLog(int operResult, string denyReason)
        {
            PeriodicTransferDB.AddPeriodicTransferLog(this,operResult,denyReason);

        }
        public void SetCompleted(DateTime dateOfOperation, DateTime dayOfRateCalculation)
        {

            PeriodicTransferDB.PeriodicTransfersComplete(dateOfOperation, dayOfRateCalculation, this.ProductId);
        }
        public void PeriodicTransfersClosed()
        {
            PeriodicTransferDB.PeriodicTransfersClosed(this.ProductId);
        }
        public void CloseExpiredPeriodicTransfers(DateTime currentOperDay, DateTime nextOperDay)
        {
            PeriodicTransferDB.CloseExpiredPeriodicTransfers(currentOperDay, nextOperDay);
        }


        public static int GetTransferTypeByAppId(ulong appId)
        {
            return PeriodicTransferDB.GetTransferTypeByAppId(appId);
        }

        public static int GetPeriodicTransfersCount(ulong customerNumber, PeriodicTransferTypes transferType)
        {
            return PeriodicTransferDB.GetPeriodicTransfersCount(customerNumber, transferType);
        }
    }
}
