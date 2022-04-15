using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;

namespace ExternalBanking
{
    public class VehicleInsuranceOrder : Order
    {
        /// <summary>
        /// Անուն ազգանուն
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Անձը հաստատող փաստաթուղթ/ՀՎՀՀ
        /// </summary>
        public string ID_TPIN { get; set; }

        /// <summary>
        /// Բանկային հաշվեհամար - ցուցադրվում է միայն ՀՎՀՀ–ով որոնումներ կատարելու դեպքում (policyHolderType - Legal)
        /// </summary>
        public string ContractAccountNumber { get; set; }

        /// <summary>
        /// Բանկի ունիկալ համար
        /// </summary>
        public int BankID { get; set; }

        /// <summary>
        /// Բանկի անվանումը Հայերեն
        /// </summary>
        public string BankNameAM { get; set; }

        /// <summary>
        /// Բանկի անվանումը Անգլերեն
        /// </summary>
        public string BankNameEN { get; set; }

        /// <summary>
        /// մեքենայի մակնիշը
        /// </summary>
        public string VehicleMark { get; set; }

        /// <summary>
        /// մեքենայի տիպը
        /// </summary>
        public string VehicleModel { get; set; }

        /// <summary>
        /// Հաշվառման համարանիշ
        /// </summary>
        public string VehicleNumber { get; set; }

        /// <summary>
        /// Շարժիչի հզորություն
        /// </summary>
        public ushort HorsePower { get; set; }

        /// <summary>
        /// ԲՄ դաս
        /// </summary>
        public ushort Bonusmalus { get; set; }

        /// <summary>
        /// Շահագործման նպատակի ունիկալ համար
        /// </summary>
        public short VehicleUseTypeID { get; set; }

        /// <summary>
        /// Շահագործման նպատակը Հայերեն
        /// </summary>
        public string VehicleUseTypeAM { get; set; }

        /// <summary>
        /// Շահագործման նպատակը Անգլերեն
        /// </summary>
        public string VehicleUseTypeEN { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Տևողություն
        /// </summary>
        public ushort Duration { get; set; }

        /// <summary>
        /// Տևողության տիպ(օր, ամիս)
        /// </summary>
        public ushort DurationType { get; set; }

        /// <summary>
        /// Ավարտ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Ապահովագրական ընկերության ունիկալ համար
        /// </summary>
        public short InsuranceCompanyID { get; set; }

        /// <summary>
        /// Ապահովագրական ընկերության անվանումը
        /// </summary>
        public string InsuranceCompanyName { get; set; }

        /// <summary>
        /// օգտագործե՞լ վարկային գիծ    
        /// </summary>
        public bool UseCreditLine { get; set; }

        /// <summary>
        /// իրավաբանական/ֆիզիկական անձ    
        /// </summary>
        public short PolicyHolderTypeCode { get; set; }

        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false` հակառակ դեպքում
        /// </summary>
        public bool AcknowledgementCheckBox { get; set; }

        /// <summary>
        /// Եթե checkbox-ը նշված է, ապա checkbox-ի դիմաց ցուցադրված տեքստը
        /// </summary>
        public string AcknowledgementText { get; set; }

        /// <summary>
        /// ASWA-ից ստացվող պայմանագրի համարը
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Թարմացման տիպ
        /// </summary>
        public short UpdateType { get; set; } 

        /// <summary>
        /// Կատարում ASWA կողմում
        /// </summary>
        public bool CompletedInASWA { get; set; } = false;

        /// <summary>
        /// Մուտքագրվող (կրեդիտ դրամարկղ) հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Հաճախորդի էլ.հասցե
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// ԲՄ դասի ունիկալ համար
        /// </summary>
        public int PolicyHolderID { get; set; }

        public string Token { get; set; }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = VehicleInsuranceOrderDB.SaveVehicleInsuranceOrder(this, userName, source);

                //**********
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        public ActionResult Update(SourceType source, ACBAServiceReference.User user, short schemaType, string userName)
        {

            this.Complete(source);

            ActionResult result = new ActionResult();
            //result = this.Validate();

            //if (result.Errors.Count > 0)
            //{
            //    result.ResultCode = ResultCode.ValidationError;
            //    return result;
            //}


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                if (UpdateType == 1)
                {
                    ActionResult res = base.Approve(schemaType, userName);
                    if (res.ResultCode == ResultCode.Normal)
                    {
                        this.Quality = OrderQuality.Sent3;
                        this.SubType = 1;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        result = base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                    }
                    else
                    {
                        return res;
                    }
                    //result = base.UpdateQuality(Quality);
                }
                else if (UpdateType == 2)
                {
                    result = VehicleInsuranceOrderDB.UpdateVehicleInsuranceOrderDetails(this);
                }
                else if (UpdateType == 3)
                {
                    base.UpdateQuality(Quality);
                    result = VehicleInsuranceOrderDB.UpdateVehicleInsuranceOrderDetails(this);
                }
                else if (UpdateType == 4)
                {
                    result = VehicleInsuranceOrderDB.UpdateVehicleInsuranceOrder(this);
                }

                //**********
                LogOrderChange(user, Action.Update);
                scope.Complete();
            }

            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateVehicleInsuranceOrder(this));
            return result;
        }

        public ActionResult Approve(ACBAServiceReference.User user, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            this.CompletedInASWA = true;
            VehicleInsuranceOrderDB.UpdateVehicleInsuranceOrderDetails(this);

            return ValidateForSend();
        }

        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            this.Type = OrderType.ConsumeLoanApplicationOrder;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        public void GetVehicleInsuranceOrder()
        {
            VehicleInsuranceOrderDB.GetVehicleInsuranceOrder(this);
        }

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();


            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

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

        internal static ActionResult InsertASWAContractResponseDetails(long DocId, bool IsCompleted, short TypeOfFunction, string Description)
        {
            return VehicleInsuranceOrderDB.InsertASWAContractResponseDetails(DocId, IsCompleted, TypeOfFunction, Description);
        }

        internal static ActionResult CheckInASWA(long Id, ulong CustomerNumber)
        {
            ActionResult result = new ActionResult();
            var _order = VehicleInsuranceOrderDB.GetVehicleInsuranceOrder(new VehicleInsuranceOrder() { Id = Id, CustomerNumber = CustomerNumber });
            if (_order.CompletedInASWA == false)
            {
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>
                                        { new KeyValuePair<string, string>("X-Token", ConfigurationManager.AppSettings["X-Token"].ToString() )};


                string serviceUrl = ConfigurationManager.AppSettings["OnlineInsuranceAPIURL"].ToString();
                using System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();


                client.BaseAddress = new Uri(serviceUrl);

                headers?.ForEach(m =>
                {
                    client.DefaultRequestHeaders.Add(m.Key, m.Value);
                });

                System.Net.Http.HttpResponseMessage response = client.GetAsync("api/payment/" + _order.Id).Result;
                string _result = response.Content.ReadAsStringAsync().Result;
                VehicleInsuranceOrderDB.InsertASWAContractResponseDetails(_order.Id, _result == "true", 4, _result);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (_result == "true")
                    {
                        _order.CompletedInASWA = true;
                        _order.Description = null;
                        VehicleInsuranceOrderDB.UpdateVehicleInsuranceOrderDetails(_order);
                        result.ResultCode = ResultCode.Normal;
                    }
                    else
                    {

                        result.ResultCode = ResultCode.ValidationError;
                        result.Errors.Add(new ActionError(2061, new string[] { _order.QualityDescription, _order.Id.ToString() }));
                    }

                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    result.Errors.Add(new ActionError(35));
                }
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }

        public ActionResult SaveAswaSearchResponseDetails()
        {
            return VehicleInsuranceOrderDB.SaveAswaSearchResponseDetails(this);
        }
        public void GetAswaSearchResponseDetails(long Id)
        {
            VehicleInsuranceOrderDB.GetAswaSearchResponseDetails(this, Id);
        }
    }
}
