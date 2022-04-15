using System.Collections.Concurrent;
using System.ServiceModel;

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
