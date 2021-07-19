using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.ServiceClient
{
    public static class ProxyManager<IType>
    {
        public static ConcurrentDictionary<string, ChannelFactory<IType>> proxies = new ConcurrentDictionary<string, ChannelFactory<IType>>();

        public static IType GetProxy(string key)
        {
            return proxies.GetOrAdd(key, m => new ChannelFactory<IType>("*")).CreateChannel();
        }

        public static bool RemoveProxy(string key)
        {
            ChannelFactory<IType> proxy;
            return proxies.TryRemove(key, out proxy);
        }
    }
}
