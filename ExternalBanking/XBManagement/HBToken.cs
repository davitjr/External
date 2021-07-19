using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.Interfaces;
using ExternalBanking.XBManagement.Interfaces;

namespace ExternalBanking.XBManagement
{
    public class HBToken:IEditableHBItem
    {
      
        /// <summary>
        /// Հերթական ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// ՀԲ օգտագործողի ID
        /// </summary>
        public int HBUserID { get; set; }        
        /// <summary>
        /// Տոկենի համար
        /// </summary>
        public string TokenNumber { get; set; }
        /// <summary>
        /// Տոկենի տեսակ
        /// </summary>
        public HBTokenTypes TokenType { get; set; }
        /// <summary>
        /// Տոկոնի տեսակի նկարագրություն
        /// </summary>
        public string TokenTypeDescription { get; set; }


        /// <summary>
        /// Տոկենի ենթատեսակ
        /// </summary>
        public HBTokenSubType TokenSubType { get; set; }
        /// <summary>
        /// Տոկոնի ենթատեսակի նկարագրություն
        /// </summary>
        public string TokenSubTypeDescription { get; set; }

        /// <summary>
        /// Ակտիվացման ա/թ
        /// </summary>
        public DateTime? ActivationDate { get; set; }
        /// <summary>
        /// Ապաակտիվացման ա/թ
        /// </summary>
        public DateTime? DeactivationDate { get; set; }
        /// <summary>
        /// Օրական սահմանաչափ
        /// </summary>
        public double DayLimit { get; set; }
        /// <summary>
        /// Գործարքի սահմանաչափ
        /// </summary>
        public double TransLimit { get; set; }
        /// <summary>
        /// Բլոկավորման կարգավիճակ
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Բլոկավորման ա/թ
        /// </summary>
        public DateTime? BlockingDate { get; set; }
        /// <summary>
        /// Ակտիվացումն իրականացրած աշխատակցի ՊԿ
        /// </summary>
        public int? ActivationSetID { get; set; }
        /// <summary>
        /// Ապաակտիվացումն իրականացրած աշխատակցի ՊԿ
        /// </summary>
        public int? DeactivationSetID { get; set; }
        /// <summary>
        /// Նշումներ
        /// </summary>
        public String Issuer { get; set; }
        /// <summary>
        /// Թոկենի կարգավիճակ
        /// </summary>
        public HBTokenQuality Quality {get;set;}
        /// <summary>
        /// Թոկենի կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Տոկենի վերականգնվելու հնարավորություն
        /// </summary>
        public bool IsRestorable { get; set; }
        /// <summary>
        /// Տոկենի GID
        /// </summary>
        public string GID { get; set; }
        /// <summary>
        /// PIN կոդ
        /// </summary>
        public string Pin { get; set; }      
        /// <summary>
        /// Հաճախորդի ռեգիստրացիաի վիճակ
        /// </summary>
        public bool IsRegistered { get; set; }
        /// <summary>
        /// Հաճախորդի համար 
        /// </summary>
        public ulong UserCustomerNumber
        {
            get { return HBUser.CustomerNumber; }
            set { }
        }
        /// <summary>
        /// Հաճախորդի անուն ազգանուն
        /// </summary>
        public string UserFullName
        {
            get { return HBUser.UserFullName; }
            set { }
        }

        /// <summary>
        /// սարքավորման նկարագրություն
        /// </summary>
        public string DeviceTypeDescription { get; set; }

        public HBToken()
        {
            HBUser = new HBUser();
        }

        public HBUser HBUser { get; set; }

        /// <summary>
        /// Վերադարձնում է ՀԲ օգտագործողի տվյալ կարգավիճակով բոլոր թոկենները
        /// </summary>
        /// <param name="HBUserID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<HBToken> GetHBTokens(int HBUserID, ProductQualityFilter filter)
        {
            List<HBToken> HBTokens = new List<HBToken>();
            HBTokens.AddRange(HBTokenDB.GetHBTokens(HBUserID, filter));
            return HBTokens;
        }
        /// <summary>
        /// Վերադարձնում է նշված օգտագործողի ֆիլտրված թոքեները
        /// </summary>
        /// <param name="HBUserID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<HBToken> GetHBTokens(int HBUserID, HBTokenQuality filter)
        {
            return HBTokenDB.GetFilteredHBTokens(HBUserID, filter);
        }
        /// <summary>
        /// Վերադարձնում է նշված մեկ տոկենը
        /// </summary>
        /// <param name="TokenID"></param>
        /// <returns></returns>
        public static HBToken GetHBToken(int TokenID)
        {
            return HBTokenDB.GetHBToken(TokenID);
        }

        /// <summary>
        /// Վերադարձնում է նշված մեկ տոկենը
        /// </summary>
        /// <param name="TokenSerial"></param>
        /// <returns></returns>
        public static HBToken GetHBToken(string TokenSerial)
        {
            return HBTokenDB.GetHBToken(TokenSerial);
        }
        /// <summary>
        /// Վերադարձնում է տոկենի համարը
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public static List<string> GetHBTokenNumbers(HBTokenTypes tokenType,int userFilial)
        {
            return HBTokenDB.GetHBTokenNumers(tokenType, userFilial);
        }

        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBToken(this));
           
            return result;
        }
        public ActionResult ValidateUpdate()
        {

            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBTokenUpdate(this));

            return result;
        }
        /// <summary>
        /// Նոր տոկենի ավելացում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public ActionResult Add(int userID, SourceType source, long docId, ulong customerNumber = 0)
        {
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            HBTokenDB.Save(userID, source, this, docId, Action.Add);

            result.ResultCode = ResultCode.Normal; 
            return result;
        }

        /// <summary>
        /// Տոկենի ապաակտիվացում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public ActionResult Deactivate(int userID, SourceType source, long docId, ulong customerNumber = 0)
        {
            ActionResult result = new ActionResult();
            
            HBTokenDB.Save(userID, source, this, docId, Action.Deactivate);

            result.ResultCode = ResultCode.Normal;
            return result;
        }
        /// <summary>
        /// Տոկենի խմբագրում
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="source"></param>
        /// <param name="docId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public ActionResult Update(int userID, SourceType source, long docId, ulong customerNumber = 0)
        {
            ActionResult result = this.ValidateUpdate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            HBTokenDB.Save(userID, source, this, docId, Action.Update);

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Հեռահար բանկինգի բոլոր գործող տոկեները
        /// </summary>
        /// <param name="HBUserID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<HBToken> GetHBTokens(int hbAppId)
        {
            List<HBToken> hbTokens = new List<HBToken>();
            var hbUsers=HBUser.GetHBUsers(hbAppId, ProductQualityFilter.Opened);
            List<HBToken> tokens;
            foreach (var user in hbUsers)
            {
                tokens =GetHBTokens(user.ID, ProductQualityFilter.Opened);
                hbTokens.AddRange(tokens);
            }           
            return hbTokens;
        }

        /// <summary>
        /// Չեղարկում է ամրագրած տոկենները
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ActionResult CancelTokenNumberReservation(HBToken  token)
        {
            ActionResult result = new ActionResult();
            HBTokenDB.CancelTokenNumberReservation(token);
            result.ResultCode = ResultCode.Normal;
            return result;
            
        }

        /// <summary>
        /// Տոկենի սպասարկման վարձ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="date"></param>
        /// <param name="requestType"></param>
        /// <param name="tokenType"></param>
        /// <param name="tokenSubType"></param>
        /// <returns></returns>
        public static double GetHBServiceFee(ulong customerNumber, DateTime date, HBServiceFeeRequestTypes requestType, HBTokenTypes tokenType, HBTokenSubType tokenSubType)
        {
            return HBTokenDB.GetHBServiceFee(customerNumber,  date,  requestType,  tokenType,  tokenSubType);
        }
        /// <summary>
        /// Վերադարձվում է տոկենի GID դաշտը
        /// </summary>
        /// <param name="hbuserID"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public static string GetHBTokenGID(int hbuserID, HBTokenTypes tokenType)
        {
            return HBTokenDB.GetHBTokenGID(hbuserID, tokenType);
        }
        /// <summary>
        /// Ստուգվում է տոկենի համարի հասանելիությունը
        /// </summary>
        /// <returns></returns>
        public bool CheckTokenNumberAvailability()
        {
            return HBTokenDB.CheckTokenNumberAvailability(this);
        }
        /// <summary>
        /// Ստուգվում է տոկենների քանակի հասանելիությունը
        /// </summary>
        /// <returns></returns>
        public bool CheckTokenQuantityAvailability()
        {
            return HBTokenDB.CheckTokenQuantityAvailability(this);
        }
    
        public static bool HasCustomerOnlineBanking(ulong customerNumber)
        {
            return HBTokenDB.HasCustomerOnlineBanking(customerNumber);
        }
        public static bool HasCustomerOneActiveToken(ulong customerNumber)
        {
            return HBTokenDB.HasCustomerOneActiveToken(customerNumber);
        }       
    }
}
