using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using System.Linq;
using System.Data;

namespace ExternalBanking
{
    public class CredentialOrder : Order
    {
        /// <summary>
        /// Լիազորագիր
        /// </summary>
        public Credential Credential { get; set; }


        public ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            
            ActionResult result = new ActionResult();

            result = this.ValidateForSend(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            return result;
        }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            //ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                
                List<OrderAttachment> ExistingAttachments = Order.GetOrderAttachments(this.Id);
                result = CredentialOrderDB.Save(this, userName, source);
                long ResultId = result.Id;
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                //**********
                if (action == Action.Add)
                {
                    ulong orderId = base.Save(this, source, user);
                    Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                    BOOrderPaymentDetails.Save(this, orderId);
                    result = BOOrderCustomer.Save(this, orderId, user);
                }
                else
                {
                    ulong orderId = BOOrderPaymentDetails.GetOrderId(this.Id);
                    CredentialOrderDB.RemoveCredentialOrderDetails(this.Id);
                    CredentialOrderDB.RemoveCredentialOrderBODetails(orderId);
                    BOOrderPaymentDetails.Save(this, orderId, Action.Update);
                    result = BOOrderCustomer.Save(this, orderId, user);
                }

                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                {
                    result = base.SaveOrderFee();
                    result = base.SaveOrderAttachments();
                    foreach (var existingItem in ExistingAttachments)
                    {
                        bool HasFile = false;
                        foreach (var item in this.Attachments)
                        {
                            if ((!String.IsNullOrEmpty(item.Id)) && existingItem.Id.ToLower() == item.Id.ToLower())
                            {
                                HasFile = true;
                                break;
                            }
                        }
                        if (HasFile == false)
                            OrderDB.DeleteOrderAttachment(existingItem.Id);
                    }
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result.Id = ResultId;
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }



        private ActionResult ValidateForSend(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

            if (this.Quality != OrderQuality.Draft && this.Quality != OrderQuality.Approved)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }

            if (Source == SourceType.AcbaOnline)
            {
                if (Utility.GetCurrentOperDay().Date > this.RegistrationDate.Date)
                {
                    //Բանկում գործառնական օրը փոխվել է: Խնդրում ենք Ձեզ լիազորագրի ստացման հայտը լրացնել նորից
                    result.Errors.Add(new ActionError(1285));
                    return result;
                }


                if (this.Fees != null && this.Fees.Count > 0)
                {
                    foreach (OrderFee fee in this.Fees)
                    {
                        if (fee.Type == 27)
                        {
                            double feeAmount = this.Fees.FindAll(m => m.Account.AccountNumber == fee.Account.AccountNumber).Sum(m => m.Amount);
                            double feeAccountBalance = Account.GetAcccountAvailableBalance(fee.Account.AccountNumber);

                            if (feeAccountBalance < feeAmount)
                            {                            
                                //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                                result.Errors.Add(new ActionError(788, new string[] { fee.Account.AccountNumber }));                               
                            }
                        }
                    }
                }
            }

            return result;
        }


        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            //ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CredentialOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                //**********                
                if (action == Action.Add)
                {
                    ulong orderId = base.Save(this, source, user);
                    Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                    BOOrderPaymentDetails.Save(this, orderId);
                    result = BOOrderCustomer.Save(this, orderId, user);
                }
                else
                {
                    ulong orderId = BOOrderPaymentDetails.GetOrderId(this.Id);
                    CredentialOrderDB.RemoveCredentialOrderDetails(this.Id);
                    CredentialOrderDB.RemoveCredentialOrderBODetails(orderId);
                    BOOrderPaymentDetails.Save(this, orderId, Action.Update);
                    result = BOOrderCustomer.Save(this, orderId, user);
                }
                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                {
                    result = base.SaveOrderAttachments();
                }
                else
                {
                    result = base.SaveOrderOPPerson();
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);


                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }   
            return result;
        }

        /// <summary>
        /// Լիազորագրի հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCredentialOrder(this, user));
            return result;
        }


        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber))
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(Source ==SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {

                if (this.Credential != null)
                {
                    this.Credential.Status = 10;
                    this.Credential.CredentialType = 0;
                }

                if (this.Fees != null && this.Fees[0] != null)
                {
                    double credentialOrderFee = FeeForServiceProvidedOrder.GetServiceFee(this.CustomerNumber, OrderType.FeeForServiceProvided, 215);

                    this.Fees[0].Amount = credentialOrderFee;
                    this.Fees[0].Type = 27;
                    this.Fees[0].Currency = "AMD";
                }

                this.RegistrationDate = DateTime.Now;
                this.FilialCode = 22000;

                if(Credential.AssigneeList != null)
                {
                    Assignee assignee = Credential.AssigneeList[0];

                    //Բոլոր լիազորությունները
                    if (assignee.AllOperations)
                    {
                        byte customerType;
                        using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
                        {
                            customerType = proxy.GetCustomerType(CustomerNumber);
                        }

                        List<AssigneeOperation> operationList = new List<AssigneeOperation>();

                        //ԳՈՐԾՈՂՈՒԹՅԱՆ ԽՄԲԵՐ
                        DataTable groups = Info.GetAssigneeOperationGroupTypes(customerType);

                        foreach (DataRow group in groups.AsEnumerable())
                        {
                            ushort groupId = Convert.ToUInt16(group["id"].ToString());
                            string groupDescription = Utility.ConvertAnsiToUnicode(group["description"].ToString());
                           
                            //ԳՈՐԾՈՂՈՒԹՅԱՆ ԽՄԲԻ ՏԵՍԱԿՆԵՐ
                            DataTable operationTypes = Info.GetAssigneeOperationTypes(groupId, customerType);

                            List<Tuple<int, int, string, bool>> operationTypesListNew = new List<Tuple<int, int, string, bool>>();

                            foreach (DataRow operationType in operationTypes.AsEnumerable())
                            {

                                if ((int)operationType["id"] != 14 && ((int)operationType["id"] == 16 || (int)operationType["id"] == 17))
                                {

                                    operationTypesListNew.Add(new Tuple<int, int, string, bool>((int)operationType["Groupid"], (int)operationType["id"], Utility.ConvertAnsiToUnicode(operationType["description"].ToString()), false));

                                }
                                else
                                {
                                    operationTypesListNew.Add(new Tuple<int, int, string, bool>((int)operationType["Groupid"], (int)operationType["id"], Utility.ConvertAnsiToUnicode(operationType["description"].ToString()), Convert.ToBoolean(operationType["CanChangeAllAccounts"])));

                                }
                            }


                            foreach (var operationType in operationTypesListNew)
                            {
                                AssigneeOperation operation = new AssigneeOperation();

                                operation.GroupId = groupId;
                                operation.OperationGroupTypeDescription = groupDescription;

                                operation.OperationType = (ushort)operationType.Item2;
                                operation.OperationTypeDescription = operationType.Item3;

                                operation.AllAccounts = operationType.Item4;

                                operationList.Add(operation);
                            }

                           
                        }
                        Credential.AssigneeList[0].OperationList = operationList;
                    }
                }
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            CredentialOrderDB.Get(this);
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                this.Fees = Order.GetOrderFees(this.Id);
                this.Attachments = Order.GetOrderAttachments(this.Id);
                if (Credential.AssigneeList != null)
                {
                    Credential.AssigneeList[0].AllOperations = true;
                    Assignee assignee = Credential.AssigneeList[0];
                    List<AssigneeOperation> credentialOperationList = assignee.OperationList;
                    byte customerType;
                    using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
                    {
                        customerType = proxy.GetCustomerType(CustomerNumber);
                    }

                    List<AssigneeOperation> operationList = new List<AssigneeOperation>();

                    //ԳՈՐԾՈՂՈՒԹՅԱՆ ԽՄԲԵՐ
                    DataTable groups = Info.GetAssigneeOperationGroupTypes(customerType);

                    foreach (DataRow group in groups.AsEnumerable())
                    {
                        ushort groupId = Convert.ToUInt16(group["id"].ToString());
                        string groupDescription = Utility.ConvertAnsiToUnicode(group["description"].ToString());

                        if(credentialOperationList.Find(m => m.GroupId == groupId) == null)
                        {
                            Credential.AssigneeList[0].AllOperations = false;
                            break;
                        }
                        else
                        {
                            //ԳՈՐԾՈՂՈՒԹՅԱՆ ԽՄԲԻ ՏԵՍԱԿՆԵՐ
                            DataTable operationTypes = Info.GetAssigneeOperationTypes(groupId, customerType);

                            List<Tuple<int, int, string, bool>> operationTypesListNew = new List<Tuple<int, int, string, bool>>();

                            foreach (DataRow operationType in operationTypes.AsEnumerable())
                            {

                                if ((int)operationType["id"] != 14 && ((int)operationType["id"] == 16 || (int)operationType["id"] == 17))
                                {

                                    operationTypesListNew.Add(new Tuple<int, int, string, bool>((int)operationType["Groupid"], (int)operationType["id"], Utility.ConvertAnsiToUnicode(operationType["description"].ToString()), false));

                                }
                                else
                                {
                                    operationTypesListNew.Add(new Tuple<int, int, string, bool>((int)operationType["Groupid"], (int)operationType["id"], Utility.ConvertAnsiToUnicode(operationType["description"].ToString()), Convert.ToBoolean(operationType["CanChangeAllAccounts"])));

                                }
                            }


                            foreach (var operationType in operationTypesListNew)
                            {

                                if (credentialOperationList.Find(m => m.OperationType == (ushort)operationType.Item2) == null)
                                {
                                    Credential.AssigneeList[0].AllOperations = false;
                                    break;
                                }
                                else
                                {
                                    if(operationType.Item4)
                                    {
                                        if (credentialOperationList.Find(m => m.OperationType == (ushort)operationType.Item2)?.AllAccounts == false)
                                        {
                                            Credential.AssigneeList[0].AllOperations = false;
                                            break;
                                        }
                                    }                               
                                }
                            }
                        }

                       
                    }
                }

            }
        }

        public static List<AssigneeOperation> GetAllOperations(int typeOfCustomer)
        {
            return CredentialOrderDB.GetAllOperations(typeOfCustomer);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ մ/ճ-ում տվյալ գործառնական օրվա հաջորդ փասթաթղթի համարը
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static ulong GetNextCredentialDocumentNumber(uint filialCode)
        {
            return CredentialOrderDB.GetNextCredentialDocumentNumber(filialCode);
        }


    }
}
