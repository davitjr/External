using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Հայտի/երի որոնման պարամետրեր
    /// </summary>
    public class TransferFilter
    {
        /// <summary>
        /// Սկիզբ 
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Վերջ  
        /// </summary> 
        public DateTime? DateTo { get; set; }

        /// <summary>
        // Ստացված/ուղարկված
        /// </summary>
        public short SendOrReceived { get; set; }

        /// <summary>
        /// Փոխանցման տեսակ
        /// </summary>
        public short TransferGroup { get; set; }

        /// <summary>
        /// Համակարգ
        /// </summary>
        public short TransferSystem { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public string Filial { get; set; }

        /// <summary>
        ///  Վճարված/Չվճարված
        /// </summary>
        public byte IsPayed { get; set; }


        /// <summary>
        ///Գրացող ՊԿ
        /// </summary>
        public short RegisteredUserID { get; set; }

        /// <summary>
        ///Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        ///Հեռացվածները
        /// </summary>
        public byte Deleted { get; set; }

        /// <summary>
        ///Հեռախոսազանգով
        /// </summary>
        public byte IsCallTranasfer { get; set; }

        /// <summary>
        /// Փոխանցումը ՀԲ-ով է թե ոչ
        /// </summary>
        public byte IsHBTransfer { get; set; }

        /// <summary>
        /// Ֆայլ
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// Մաքսիմալ գումար
        /// </summary>
        public double MaxAmount { get; set; }

        /// <summary>
        /// Փոխանցող բանկ
        /// </summary>
        public string DebetBank { get; set; }

        /// <summary>
        /// Դեբետ հաշիվ
        /// </summary>
        public string DebetAccount { get; set; }

        /// <summary>
        /// Կրեդիտ հաշիվ
        /// </summary>
        public string CreditAccount { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Համընկնում
        /// </summary>
        public short Verified { get; set; }

        /// <summary>
        /// AML Հաստատում
        /// </summary>
        public short AMLCheck { get; set; }

        /// <summary>
        /// Պարզված
        /// </summary>
        public byte UnknownTransfer { get; set; }

        /// <summary>
        ///  Նկ. ստուգում
        /// </summary>
        public string DescrOK { get; set; }

        /// <summary>
        /// Ստուգված
        /// </summary>
        public short IsChecked { get; set; }

        /// <summary>
        /// Փոխանցման տեսակ (Հաճախորդի թե բանկի)
        /// </summary>
        public short TransferType { get; set; }

        /// <summary>
        ///  Փոխանցման փուլ
        /// </summary>
        public short TransferRequestStep { get; set; }

        /// <summary>
        /// Փոխանցման կարգավիճակ
        /// </summary>
        public short TransferRequestStatus { get; set; }

        /// <summary>
        /// Փոխանցման կարգավիճակ
        /// </summary>
        public int DocumentNumber { get; set; }

        /// <summary>
        /// Փոխանցման Սեանս
        /// </summary>
        public int Session { get; set; }


        /// <summary>
        // Հեռախոսային կենտրոն է թե ոչ
        /// </summary>
        public short RegisteredBy { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Swift փոխանցման ունիկալ համար
        /// </summary>
        public string UETR { get; set; }

        /// <summary>
        /// Կարգավիճակ (Նորմալ, ետ վերադարձրած, Ուղարկված/Վերադարձված, Ստացված/Չեղարկված, Փոփոխությունը հաստատված է)
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// Փոխանցման աղբյուր
        /// </summary>
        public short TransferSource { get; set; }

        /// <summary>
        /// Վերադարձնում է  փոխանցումները ըստ ֆիլտրերի
        /// </summary> 
        public List<Transfer> GetList(ACBAServiceReference.User user, ulong CustomerNumber = 0)
        {
            //if (String.IsNullOrEmpty(this.Filial) && user.filialCode != 22000)
            //    this.Filial = user.filialCode.ToString();

            //if (!String.IsNullOrEmpty(this.Filial) && this.Filial != user.filialCode.ToString() && user.filialCode != 22000)
            //    this.Filial = user.filialCode.ToString();

            return TransferDB.GetList(this, user, CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է  փոխանցումները ըստ ֆիլտրերի
        /// </summary> 
        public List<ReceivedBankMailTransfer> GetReceivedBankMailTransfersList(ACBAServiceReference.User user, ulong CustomerNumber = 0)
        {
            return ReceivedBankMailTransferDB.GetList(this, user, CustomerNumber);
        }

    }
}
