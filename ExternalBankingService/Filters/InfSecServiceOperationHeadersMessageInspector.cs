using xb = ExternalBanking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Web.Configuration;
using System.ServiceModel.Web;
using System.Net;
//using ExternalBanking;

namespace ExternalBankingService.Filters
{
    public class InfSecServiceOperationHeadersMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            try
            {
                string methodName = OperationContext.Current.IncomingMessageHeaders.Action.Split('/').ToList().Last();

                MethodInfo info = instanceContext.GetServiceInstance().GetType().GetMethod(methodName);
                foreach (var item in info.CustomAttributes)
                {
                    if (item.AttributeType.Name == nameof(AllowAnonymous))
                    {
                        return null;
                    }
                }


                string authorizedUserSessionToken = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("myAuthorizedUserSessionToken", "InfSecServiceOperationNamespace");

                xb.Languages languages = xb.Languages.eng;
                string clientIp = "";
                xb.SourceType sourceType = xb.SourceType.STAK;


                var currentService = (STAKService)instanceContext.GetServiceInstance();
               
                if (currentService == null)
                {
                    return null;
                    //throw new WebFaultException(HttpStatusCode.Unauthorized);
                }

                bool checkUserSession = currentService.Initialization(authorizedUserSessionToken, languages, clientIp, sourceType);

                if (!checkUserSession)
                {
                    throw new WebFaultException(HttpStatusCode.Unauthorized);
                }

                return null;
            }
            catch (WebFaultException ex)
            {
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                return null;
                //throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequestHeaderOutputBehaviorInfSecServiceOperation : Attribute, IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        InfSecServiceOperationHeadersMessageInspector inspector = new InfSecServiceOperationHeadersMessageInspector();
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    internal class AllowAnonymous : Attribute
    {

    }








}