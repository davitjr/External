using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;



namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class ReceivedBankMailTransfer 
    {

    public ulong ID {get;set;}
	public DateTime  DateGet {get;set;}
    public DateTime TimeGet { get; set; }
	public string FName  {get;set;}
	public string AccDebetCB  {get;set;}
	public string DescrDebetCB  {get;set;}
    public string AccDebet   {get;set;}
    public string AccCredit   {get;set;}
	public string DescrCredit  {get;set;} 
	public string DescrPoxancym  {get;set;}
	public DateTime DateTransfer  {get;set;} 
    public string Valuta  {get;set;}
    public double Amount  {get;set;} 
	public byte TransOK  {get;set;}
	public string StrFirstLine  {get;set;}
	public int Editing   {get;set;}
	public DateTime DateTrans  {get;set;} 
	public int UserCode {get;set;}
	public int ForPrint {get;set;} 
	public string CardNumber  {get;set;}
	public short CardFilial {get;set;}
	public string FileForBranch  {get;set;}
	public string SocialNumber {get;set;}
	public string DescrDebet  {get;set;}
	public short Verified  {get;set;} 
	public short VerifierSetNumber {get;set;}
	public short NotAutomatTrans  {get;set;} 
	public long TransactionsGroupNumber  {get;set;} 
	public short AmlCheck  {get;set;} 
	public DateTime AmlCheckDate  {get;set;} 
	public short AmlCheckSetNumber  {get;set;} 
	public short VerifiedAML  {get;set;} 
	public short VerifierSetNumber_AML  {get;set;} 
	public DateTime VerifierSetDateAML  {get;set;} 
	public string Ident  {get;set;}
	public string UnknownReason  {get;set;}
	public short UnknownTransfer  {get;set;} 
	public short UnknownTransferSend  {get;set;}
	public string SenderType  {get;set;}
	public string ReceiverType  {get;set;}
	public string AddInf  {get;set;}
	public string PaymentCode  {get;set;} 
	public DateTime ConfirmationDate  {get;set;} 
	public short ConfirmationSetNumber  {get;set;} 
	public TimeSpan? ConfirmationTime {get;set;}
    public string PaymentOrderReferenceNumber { get; set; }
    public uint ListCount { get; set; }


    public new void Get()
    {
        ReceivedBankMailTransferDB.Get(this);

    }

 
    
    }

}
