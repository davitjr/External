using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class TransferApproveOrder : Order
    {

        /// <summary>
        /// Ձևակերպվող փոխանցում
        /// </summary>
        public Transfer Transfer { get; set; }
        public DateTime ValueDate { get; set; }
        public string TransactionType26 { get; set; }
        public string AccountAbility77B { get; set; }



        public string UIP { get; set; }  //   8084


        public string AccountInIntermediaryBank { get; set; }  // 8881

        public new ActionResult Approve(string filialCode, DateTime setDate, SourceType source, short schemaType)
        {
            this.Complete();

            ActionResult result = new ActionResult();
            List<short> errors = new List<short>();
            if (this.SubType == 1)
            {
                result.Errors = this.ValidateData();
            }
            errors = TransferDB.CheckForApprove(this.Transfer, setDate, this.SubType);

            if (errors.Count != 0)
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    if (errors[0] == 946)
                    {
                        double balance = Math.Abs(Account.GetAccountBalance(this.Transfer.CreditAccount.AccountNumber));
                        result.Errors.Add(new ActionError(955, new string[] { "Թղթակցային հաշվի մնացորդը կազմում է ընդամենը " + balance.ToString("#,0.00") + this.Transfer.Currency }));
                    }
                    else
                        result.Errors.Add(new ActionError(errors[0]));
                }
            }


            if (result.Errors.Count != 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, new Culture(Languages.hy));

                return result;
            }



            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferDB.TransferApproveOrder(this, user.userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                //result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            result = base.Confirm(user);

            return result;
        }

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected new void Complete()
        {
            this.Type = OrderType.TransferApproveOrder;
            this.RegistrationDate = Convert.ToDateTime(this.OperationDate);
            this.OrderNumber = "1";
            this.Quality = OrderQuality.Draft;


        }
        /// <summary>
        /// Ստուգում է տեքստային տվյլաները
        /// </summary>
        /// <returns></returns>
        public new List<ActionError> ValidateData()
        {
            List<ActionError> result = new List<ActionError>();
            bool isNumeric = true;
            double output;
            if (this.SubType == 1)
            {
                if (this.Transfer.Currency != "RUR")
                {
                    //if (this.Transfer.MT == "103")
                    //{
                    //    if (string.IsNullOrEmpty(this.Transfer.ReceiverBank))
                    //    {
                    //        //Ստացողի բանկի տվյալները բացակայում են:
                    //        result.Add(new ActionError(85));
                    //    }

                    //}
                    //if (string.IsNullOrEmpty(this.Transfer.ReceiverAccount))
                    //{
                    //    //Ստացողի հաշիվը բացակայում է:
                    //    result.Add(new ActionError(86));
                    //}


                    if (!string.IsNullOrEmpty(this.Transfer.IntermediaryBankSwift) && this.Transfer.IntermediaryBankSwift.Length != 11)
                    {
                        //S.W.I.F.T . կոդը պետք է լինի 11 նիշ:
                        result.Add(new ActionError(82));
                    }

                    if (!string.IsNullOrEmpty(this.Transfer.IntermediaryBankSwift) && string.IsNullOrEmpty(this.Transfer.IntermediaryBank))
                    {
                        //Միջնորդ բանկի տվյալները բացակայում են:
                        result.Add(new ActionError(83));
                    }

                    //if (string.IsNullOrEmpty(this.ReceiverBankSwift))

                    if (!string.IsNullOrEmpty(this.Transfer.ReceiverBankSwift) && this.Transfer.MT == "103")
                    {
                        string country = Info.GetInfoFromSwiftCode(this.Transfer.ReceiverBankSwift, 2);
                        if (!String.IsNullOrEmpty(country) && country != "0" && this.Transfer.Country != country)
                        {
                            result.Add(new ActionError(1018));
                        }
                    }

                    if (string.IsNullOrEmpty(this.Transfer.DescriptionForPayment))
                    {
                        //Վճարման մանրամասները բացակայում են:
                        result.Add(new ActionError(88));
                    }

                    string symbol = "";

                    if (!string.IsNullOrEmpty(this.Transfer.ReceiverBank))
                    {
                        symbol = Utility.IsExistSwiftForbiddenCharacter(this.Transfer.ReceiverBank, 1);
                        if (symbol != "")
                        {
                            // Ստացողի բանկի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                            result.Add(new ActionError(588, new string[] { symbol }));
                        }
                    }
                    if (!string.IsNullOrEmpty(this.Transfer.ReceiverBankAddInf))
                    {
                        symbol = Utility.IsExistSwiftForbiddenCharacter(this.Transfer.ReceiverBankAddInf);
                        if (symbol != "")
                        {
                            //Ստացողի բանկի լրացուցիչ տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                            result.Add(new ActionError(589, new string[] { symbol }));
                        }
                    }

                    if (!string.IsNullOrEmpty(this.Transfer.IntermediaryBank))
                    {
                        symbol = Utility.IsExistSwiftForbiddenCharacter(this.Transfer.IntermediaryBank, 1);
                        if (symbol != "")
                        {
                            //Միջնորդ բանկի տվյալներ դաշտի մեջ կա անթույլատրելի նշան`
                            result.Add(new ActionError(590, new string[] { symbol }));
                        }
                    }

                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.Transfer.DescriptionForPayment);
                    if (symbol != "")
                    {
                        //Վճարման մանրամասներ դաշտի մեջ կա անթույլատրելի նշան`  
                        result.Add(new ActionError(592, new string[] { symbol }));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.UIP))
                    {
                        if (this.UIP.Length > 25 && this.Transfer.MT == "103")
                        {
                            // UIP դաշտը պետք է լինի առավելագույնը 25 նիշ:
                            result.Add(new ActionError(1610));
                        }

                    }


                    if (string.IsNullOrEmpty(this.Transfer.VOCode) && this.Transfer.MT == "103")
                    {

                        // VO կոդ դաշտը մուտքագրված չէ
                        result.Add(new ActionError(1017));

                    }

                    if (!string.IsNullOrEmpty(this.Transfer.VOCode))
                    {
                        if (this.Transfer.VOCode.Length != 5)
                        {
                            // VO կոդ դաշտի երկարությունը պետք է լինի 5
                            result.Add(new ActionError(1035));

                        }

                        isNumeric = Double.TryParse(String.IsNullOrEmpty(this.Transfer.VOCode) ? "" : this.Transfer.VOCode.Trim(), out output);
                        if (!isNumeric)
                        {
                            // դաշտը պետք է լինի միայն թվանշաններ:
                            result.Add(new ActionError(205, new string[] { "VO կոդ" }));
                        }
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(this.Description))
                {
                    // Մերժման պատճառը լրացված չէ:
                    result.Add(new ActionError(437));
                }
            }

            if (this.Transfer.Country == "840" && this.Transfer.Currency == "USD")
            {
                if ((!String.IsNullOrEmpty(this.Transfer.ReceiverBankSwift) && !String.IsNullOrEmpty(this.Transfer.FedwireRoutingCode)) || (String.IsNullOrEmpty(this.Transfer.ReceiverBankSwift) && String.IsNullOrEmpty(this.Transfer.FedwireRoutingCode)))
                {
                    //Լրացրեք ստացող բանկի SWIFT կամ Fedwire Routing կոդերից  մեկը
                    result.Add(new ActionError(1282));
                }
                else if (!string.IsNullOrEmpty(this.Transfer.FedwireRoutingCode))
                {
                    isNumeric = Double.TryParse(String.IsNullOrEmpty(this.Transfer.FedwireRoutingCode) ? "" : this.Transfer.FedwireRoutingCode.Trim(), out output);
                    if (this.Transfer.FedwireRoutingCode.Length != 9)
                    {
                        //Fedwire Routing դաշտը պետք է լինի 9 նիշ:
                        result.Add(new ActionError(1283));
                    }
                    else if (!isNumeric)
                    {
                        // դաշտը պետք է լինի միայն թվանշաններ:
                        result.Add(new ActionError(205, new string[] { "Ստացող բանկի Fedwire Routing կոդ" }));
                    }
                }
            }



            if (!string.IsNullOrEmpty(this.TransactionType26))
            {
                if (this.TransactionType26.Length != 3)
                {
                    result.Add(new ActionError(955, new string[] { "Գործարքի տեսակը 26Т դաշտը պետք է պարունակի 3 նիշ կամ լինի դատարկ" }));
                }
            }

            if (!string.IsNullOrEmpty(this.AccountAbility77B))
            {
                if (this.AccountAbility77B.Length > 120)
                {
                    result.Add(new ActionError(955, new string[] { "Հաշվետվողականության կոդ 77В դաշտը չպետք է լինի ավելին քան 120 նիշ։" }));
                }

            }

            return result;
        }
    }

}
