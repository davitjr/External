using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class NewPosLocationOrder : Order
    {
        public int PosType { get; set; }

        public string SiteOrApp { get; set; }

        public string NameEng { get; set; }

        public string NameArm { get; set; }

        public string PosAddress { get; set; }

        private string PhoneCode { get; set; }

        private string Phone { get; set; }

        public string FullPhoneNumber => PhoneCode + Phone;

        public string AccountNumber { get; set; }

        public string ActivitySphere { get; set; }

        public string Mail { get; set; }

        public byte NewHdm { get; set; }

        public string ContactPerson { get; set; }

        public string ContactPersonPhone { get; set; }

        public byte TerminalType { get; set; }

        public int PosCount { get; set; }

        public string PosSerialNumber { get; set; }

        public byte PayWithoutCard { get; set; }

        public string Necessity { get; set; }

        public byte AllTerminals { get; set; }

        public List<int> CardSystemForService { get; set; }



        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result = Save(userName, source);

            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }

            result = Approve(schemaType, userName, user);

            return result;
        }

        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;

            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            OPPerson = SetOrderOPPerson(CustomerNumber);
        }

        public ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);                   
                }
                else
                {
                    return result;
                }
            }

            result = base.Confirm(user);

            return result;
        }


        public ActionResult Save(string userName, SourceType source)
        {
            Complete();

            ActionResult result = new ActionResult();           

                result = PosLocationDB.SaveNewPosLocationOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                //hishenq hanenq ete petq chga Dav
                ActionResult resultOpPerson = SaveOrderOPPerson();

                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }                     

            return result;
        }



        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        public NewPosLocationOrder NewPosApplicationOrderDetails(long orderId)
        {
            return PosLocationDB.NewPosApplicationOrderDetails(orderId);
        }




    }
}
