using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
	public class VirtualCardStatusChangeOrder : Order
	{
		public ulong ProductId { get; set; }
		public string VirtualCardId { get; set; }
		public string Status { get; set; }
		public short ChangeReason { get; set; }
		public string ChangeReasonAdd { get; set; }

		private void Complete()
		{
			if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
				this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
			this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

		}

		public ActionResult Validate()
		{
			ActionResult result = new ActionResult();
			return result;
		}

		/// <summary>
		/// Հայտի պահպանում և ուղարկում
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="source"></param>
		/// <param name="user"></param>
		/// <param name="schemaType"></param>
		/// <returns></returns>
		public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
		{

			this.Complete();
			ActionResult result = this.Validate();
			List<ActionError> warnings = new List<ActionError>();

			if (result.Errors.Count > 0)
			{
				result.ResultCode = ResultCode.ValidationError;
				return result;
			}


			Action action = this.Id == 0 ? Action.Add : Action.Update;

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
			{
				result = VirtualCardStatusChangeOrderDB.SaveVirtualCardStatusChangeOrder(this, userName);
				//**********                
				ulong orderId = base.Save(this, source, user);
				Order.SaveLinkHBDocumentOrder(this.Id, orderId);
				//BOOrderPaymentDetails.Save(this, orderId);
				ActionResult res = BOOrderCustomer.Save(this, orderId, user);
				//**********
				if (result.ResultCode != ResultCode.Normal)
				{
					return result;
				}
				else
				{
					base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

				}

				result = base.SaveOrderOPPerson();
				result = base.SaveOrderFee();
				if (result.ResultCode != ResultCode.Normal)
				{
					return result;
				}

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
				else
				{
					return result;
				}
			}
			ActionResult resultConfirm = Confirm(Id,user);
			return resultConfirm;
		}

		public void Get()
		{
			VirtualCardStatusChangeOrderDB.GetVirtualCardStatusChangeOrder(this);
		}

		public static ActionResult Confirm(long orderId,ACBAServiceReference.User user)
		{
			ActionResult result = new ActionResult();
			VirtualCardStatusChangeOrder order = new VirtualCardStatusChangeOrder() { Id = orderId };
				
			order.Get();
			string jsonResult = Utility.DoPostRequestJson(Newtonsoft.Json.JsonConvert.SerializeObject(new {issuerCardRefId=order.ProductId, virtualCardId = order.VirtualCardId, action = order.Status, reason = order.ChangeReason,user = user.userID,reasonAdd=order.ChangeReasonAdd }), "updateCardState","CtokenURL", null);
			ActionResultToken response = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionResultToken>(jsonResult);
			if (response.ErrorResponsecode==321)
			{
				result.ResultCode = ResultCode.Normal;
				result.Id = order.Id;
			}
			else
			{
				result.ResultCode = response.ResultCode;
				if (result.ResultCode != ResultCode.Normal)
				{
						result.Errors.Add(new ActionError() { Description = "Հարցումը ձախողվեց("+response.ErrorResponsecode.ToString()+":"+response.Description+")" });
				}
					
			}

			if (result.ResultCode == ResultCode.Normal)
				OrderDB.UpdateHBdocumentQuality(orderId, user);
			else
			{
				result.ResultCode = ResultCode.SavedNotConfirmed;
			}
			
			return result;
		}
	}
}
