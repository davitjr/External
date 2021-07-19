using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Պարբերական փոխանցման հայտ
    /// </summary>
    public class PeriodicOrder : Order
    {
        /// <summary>
        /// Կոմունալի վճարման ենթատեսակ օր.՝ սպառած գազի և տեխ.սպասարկման վճար,կամ սպառած գազի վճար
        /// </summary>
        public short ServicePaymentType { get; set; }

        /// <summary>
        /// Ամբողջ պարտքը
        /// </summary>
        public bool AllDebt { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման հնարավոր մաքսիմալ գումարը (նշվում է հաճախորդի կողմից), 
        /// եթե պարտքը մեծ է նշված գումարից, փոխանցումը չի կատարվում 
        /// </summary>
        public double MaxAmountLevel { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման տեսակ 
        /// </summary>
        public int PeriodicType { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պլանային առաջին փոխանցման օր
        /// </summary>
        public DateTime FirstTransferDate { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման նկարագրություն (տեքստը հաճախորդի կողմից)   
        /// </summary>
        public string PeriodicDescription { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման գումարի գանձման եղանակ
        ///  անբողջ պարտք
        ///  անբողջ մնացորդ
        ///  ֆիքսված գումար
        /// </summary>
        public int ChargeType { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պարբերականությունը` քանի օր մեկ է կատարվում
        /// </summary>
        public int Periodicity { get; set; }

        /// <summary>
        /// Հաշվի վրա գումարը չլինելու պատճառով պարբերական փոխանցման կատարման համար
        /// հաճախորդի կողմից նախատեսված օրերի քանակը 
        /// </summary>
        public ushort CheckDaysCount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման վեջին կատարման ամսաթիվը
        /// </summary>
        public DateTime? LastOperationDate { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման առաջին կատարման ամսաթիվը
        /// </summary>
        public DateTime StartDate { get; set; }

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
        /// Կոմունալ վճարման պարբերականի համար` պարտք չլինելու դեպքում կատարել թե ոչ
        /// Կատարման դեպքում ձևավորվում է կանխավճար 
        /// 1 - կատարել, 0 -ոչ
        /// </summary>
        public ushort PayIfNoDebt { get; set; }
        /// <summary>
        /// Պարբերական փոխանցման մասնակի կատարման կամ միայն լրիվ գումարի առկայության դեպքում կատարման նիշը
        /// </summary>
        public byte PartialPaymentSign { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման միջնորդավճար գանձելու համար հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Միջնորդավճարի գումար
        /// </summary>
        public double Fee { get; set; }

        /// <summary>
        /// Կոմունալի վճարման ենթատեսակ 
        /// </summary>
        public string ServicePaymentTypeDescription { get; set; }

        //public virtual Account DebitAccount {get;set;}

        public List<ActionError> ValidatePeriodicOrder()
        {
            List<ActionError> result = new List<ActionError>();

            if (this.PeriodicType == 0)
            {
                //Պարբերական փոխանցման տեսակն ընտրված չէ:
                result.Add(new ActionError(255));
            }
            ////Եթե ընտրված չէ ամբողջ պարտքը տեսակը
            //if (this.ChargeType != 1)
            //{
            //    if (order.Amount == 0)
            //    {
            //        //Գումարը մուտքագրված չէ:
            //        result.Add(new ActionError(20));
            //    }
            //    if (order.Amount < 0)
            //    {
            //        //Մուտքագրված գումարը սխալ է:
            //        result.Add(new ActionError(22));
            //    }

            //}

            // Online/Mobile API նոր ստուգումներ
            //**********************************************************************************
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (this.Fee > 0)
                {
                    if (this.FeeAccount == null || String.IsNullOrEmpty(this.FeeAccount.AccountNumber))
                    {
                        //Միջնորդավճարի հաշիվն ընտրված չէ
                        result.Add(new ActionError(300));
                    }
                }
                if (this.DebitAccount == null || String.IsNullOrEmpty(this.DebitAccount.AccountNumber))
                {
                    //Ելքագրվող (դեբետային) հաշիվը ընտրված չէ: 
                    result.Add(new ActionError(15));
                }

                if (String.IsNullOrEmpty(this.Currency))
                {
                    //Արժույթը ընտրված չէ:
                    result.Add(new ActionError(254));
                }

                //Եթե ընտրված չէ ամբողջ պարտքը տեսակը
                if (this.ChargeType != 2)
                {
                    if (this.Amount == 0)
                    {
                        //Գումարը մուտքագրված չէ:
                        result.Add(new ActionError(20));
                    }
                    if (this.Amount < 0)
                    {
                        //Մուտքագրված գումարը սխալ է:
                        result.Add(new ActionError(22));
                    }
                }

                string checkSymbols = Utility.CheckTextForUnpermittedSymbols(PeriodicDescription);

                //if (!String.IsNullOrEmpty(checkSymbols))
                //{
                //    //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                //    result.Add(new ActionError(78, new string[] { checkSymbols }));
                //}

                byte customerType = Customer.GetCustomerType(this.CustomerNumber);

                if (customerType == (short)CustomerTypes.physical)
                {
                    List<CustomerDocument> customerDocumentsList = Customer.GetCustomerDocumentList(this.CustomerNumber);
                    if (customerDocumentsList.Find(m => m.defaultSign == true) != null)
                    {
                        CustomerDocument defaultDocument = customerDocumentsList.Find(m => m.defaultSign == true);

                        if (defaultDocument.validDate < DateTime.Now.Date && (defaultDocument.validDate < new DateTime(2020, 3, 16).Date || DateTime.Now.Date > new DateTime(2020, 9, 13).Date))
                        {
                            //Ձեր անձը հաստատող փաստաթուղթը ժամկետանց է: Խնդրում ենք թարմացված անձը հաստատող փաստաթղթով մոտենալ 
                            //Ձեզ սպասարկող Բանկի մասնաճյուղ` համապատասխան փոփոխություններ կատարելու համար:
                            result.Add(new ActionError(224));
                        }

                    }
                }

                if (this.FirstTransferDate.Date <= DateTime.Now.Date)
                {
                    //Առաջին փոխանցման օրը պետք է մեծ լինի այօրվանից։
                    result.Add(new ActionError(1692));
                }
                else if(this.FirstTransferDate.Date <= Utility.GetNextOperDay())
                {
                    //«Առաջին փոխանցման օրը» դաշտում ամսաթիվը սխալ է:
                    result.Add(new ActionError(246));
                }
            }
            //****************************************************************************************

            if (this.Periodicity == 0)
            {
                //Պարբերականությունը ընտրված չէ:
                result.Add(new ActionError(247));
            }
            if (this.ChargeType == 2 && this.DebitAccount != null && this.DebitAccount.Currency != this.Currency)
            {
                //Ամբողջ մնացորդի փոխանցման դեպքում ընտրված արժույքը չի կարող տարբերվել դեբետագրվող հաշվի արժույթից:
                result.Add(new ActionError(268));
            }
            if (this.CheckDaysCount == 0)
            {
                //Ստուգման օրերի քանակը նշված չէ:
                result.Add(new ActionError(248));
            }
            else if (this.CheckDaysCount < 0)
            {
                //Ստուգման օրերի քանակը պետք է լինի դրական թիվ:
                result.Add(new ActionError(260));
            }

            if (this.FirstTransferDate.Date < Utility.GetCurrentOperDay().Date && (this.Source == SourceType.Bank || this.Source == SourceType.PhoneBanking))
            {
                //«Առաջին փոխանցման օրը» դաշտում ամսաթիվը սխալ է:
                result.Add(new ActionError(246));
            }
            if (this.LastOperationDate != null && this.LastOperationDate.Value.Date < DateTime.Now.Date)
            {
                //«Վերջ» դաշտում ամսաթիվը սխալ է:
                result.Add(new ActionError(245));
            }
            else
            {
                if (this.LastOperationDate != null && this.FirstTransferDate.Date > this.LastOperationDate.Value.Date)
                {
                    //Առաջին փոխանցման օրը պետք է փոքր լինի պարբերական փոխանցման ավարտից:
                    result.Add(new ActionError(257));
                }
                if (this.LastOperationDate != null && (this.LastOperationDate.Value - this.FirstTransferDate).TotalDays < this.CheckDaysCount)
                {
                    //Ընտրված ժամանակահատվածը (@var1 օր) չի կարող լինել պարբերական փոխանցման տևողությունից (@var2 օր) մեծ:
                    result.Add(new ActionError(265, new string[] { this.CheckDaysCount.ToString(), this.LastOperationDate.Value.AddDays(this.FirstTransferDate.Day * -1).Day.ToString() }));
                }
                if (this.LastOperationDate != null && (this.LastOperationDate.Value - this.FirstTransferDate).TotalDays < this.Periodicity)
                {
                    //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել պարբերական փոխանցման տևողությունը (@var2 օր):
                    result.Add(new ActionError(266, new string[] { this.Periodicity.ToString(), (this.LastOperationDate.Value.Day - this.FirstTransferDate.Day).ToString() }));
                }
                if (this.LastOperationDate != null && (this.LastOperationDate.Value - this.FirstTransferDate).TotalDays < this.CheckDaysCount)
                {
                    //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել ընտրված ժամանակահատվածը (@var2 օր):
                    result.Add(new ActionError(267, new string[] { this.CheckDaysCount.ToString(), (this.LastOperationDate.Value.Day - this.FirstTransferDate.Day).ToString() }));
                }
            }

            if (this.CheckDaysCount > this.Periodicity)
            {
                //Ստուգման օրերի քանակը (@var1 օր) չի կարող գերազանցել ընտրված ժամանակահատվածը (@var2 օր):
                result.Add(new ActionError(267, new string[] { this.CheckDaysCount.ToString(), this.Periodicity.ToString() }));
            }

            if (this.MinAmountLevel < 0 || this.MinDebetAccountRest < 0 || this.MaxAmountLevel < 0)
            {
                //Մուտքագրված գումարը սխալ է
                result.Add(new ActionError(22));
            }
            else
            {
                if (this.MinAmountLevel > this.MaxAmountLevel && this.MaxAmountLevel != 0)
                {
                    //Փոխանցման նվազագույն գումարը պետք է լինի առավելագույն գումարից փոքր:
                    result.Add(new ActionError(264));
                }
                if (this.Amount != 0 && !Utility.IsCorrectAmount(this.Amount, this.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }

                if (this.MinAmountLevel != 0 && !Utility.IsCorrectAmount(this.MinAmountLevel, this.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
                if (this.MaxAmountLevel != 0 && !Utility.IsCorrectAmount(this.MaxAmountLevel, this.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
                if (!Utility.IsCorrectAmount(this.MinDebetAccountRest, this.Currency))
                {
                    //Գումարը սխալ է մուտքագրած:
                    result.Add(new ActionError(25));
                }
            }
            if (this.PeriodicType != 3)
            {
                if (string.IsNullOrEmpty(this.PeriodicDescription))
                {
                    //Վճարման նպատակը մուտքագրված չէ:
                    result.Add(new ActionError(23));
                }
                else
                {
                    if (Utility.IsExistForbiddenCharacter(this.PeriodicDescription))
                    {
                        //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(78));
                    }
                    //if (Utility.IsTextUnicode(this.PeriodicDescription) > 0)
                    //{
                    //    //«Վճարման նպատակ» դաշտում առկա է «Unicode»-ով նշան:
                    //    result.Add(new ActionError(125));
                    //}
                    if (this.PeriodicType == 2 && this.PeriodicDescription.Length > 115)
                    {
                        //«Վճարման նպատակ»  դաշտի արժեքը չպետք է գերազանցի 115
                        result.Add(new ActionError(268, new string[] { "115" }));
                    }
                    else if (this.PeriodicDescription.Length > 130)
                    {
                        //«Վճարման նպատակ»  դաշտի արժեքը չպետք է գերազանցի 130
                        result.Add(new ActionError(268, new string[] { "130" }));
                    }
                }
            }


            result.AddRange(Validation.ValidateCustomerDocument(this.CustomerNumber));

            if (DateTime.DaysInMonth(FirstTransferDate.Year, FirstTransferDate.Month) < this.FirstTransferDate.Day + this.CheckDaysCount)
            {
                //Առաջին փոխանցման օր + ստուգման օրերի քանակ չպետք է գերազանցի ամսվա օրերի քանակը: Պարբերականը կատարվելու է յուրաքանչյուր ամիս` սկսած @var1
                result.Add(new ActionError(350, new string[] { this.FirstTransferDate.Date.ToString() }));
            }
            if (this.PeriodicType == 2)
            {
                if (Fee == -1 && (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft || Source == SourceType.AcbaOnlineXML))
                {
                    //Սակագին նախատեսված չէ:Ստուգեք փոխանցման տվյալները:
                    result.Add(new ActionError(659));
                }
                else if(this.Fee!= 0)
                    result.AddRange(Validation.ValidateFeeAccount(this.CustomerNumber, this.FeeAccount, this.Source));
            }


            if (this.PeriodicType != 3 && this.DebitAccount != null && this.DebitAccount.IsCardAccount())
            {
                Card card = Card.GetCardWithOutBallance(this.DebitAccount.AccountNumber);                                                                                                //CashBack NFC                  
                if (card != null && card.CardSystem == 3 && (card.Type == 20 || card.Type == 23 || card.Type == 34 || card.Type == 40 || card.Type == 41 || card.Type == 50))
                {
                    //AmEx տեսակի քարտերով տվյալ տեսակի պարբերական փոխանցում չի գործում:
                    result.Add(new ActionError(787));
                }
            }
            return result;
        }
        public ActionResult ValidatePeriodicOrderForSend()
        {
            ActionResult result = new ActionResult();

            DateTime nextOperDay = Utility.GetNextOperDay();
            if (this.FirstTransferDate.Date < nextOperDay)
            {
                //Առաջին փոխանցման օրը ուղարկման պահին պետք է փոքր չլինի @var1-ից:
                result.Errors.Add(new ActionError(259, new string[] { nextOperDay.ToShortDateString() }));
            }
            if (this.StartDate != nextOperDay) //պարբերականի save-ի սկզբի ամսաթիվը պետք է approve-ի գործառնական օրվա հետ նույնը լինի
            {
                //Պարբերական փոխանցումը հնարավոր չէ ուղարկել։ Անհրաժեշտ է խմբագրել գործարքը կամ մուտքագրել նոր պարբերական փոխանցում։
                result.Errors.Add(new ActionError(1849));
            }
            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                if (this.FirstTransferDate.Date <= Utility.GetNextOperDay())
                {
                    //«Առաջին փոխանցման օրը» դաշտում ամսաթիվը սխալ է:
                    result.Errors.Add(new ActionError(246));
                }
            }
            return result;
        }

        public ActionResult Save(PeriodicOrder periodicOrder, ulong orderId)
        {
            ActionResult result = PeriodicTransferOrderDB.Save(periodicOrder, orderId);

            return result;
        }
    }
}
