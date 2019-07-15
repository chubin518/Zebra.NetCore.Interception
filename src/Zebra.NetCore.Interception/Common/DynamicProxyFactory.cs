using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Zebra.NetCore.Interception.Common;

namespace Zebra.NetCore.Interception
{
    public static class DynamicProxyFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> _proxyCache = new ConcurrentDictionary<Type, Type>();
        private static ProxyGeneratorUtilities _generatorUtilities = new ProxyGeneratorUtilities();

        internal static Type GetProxyType(Type type)
        {
            Type proxyType = _proxyCache.GetOrAdd(type, key =>
              {
                  if (key.IsInterface)
                  {
                      return _generatorUtilities.CreateInterfaceProxy(key);
                  }
                  return _generatorUtilities.CreateClassProxy(key);
              });
            return proxyType;
        }

        public static object CreateProxyInstance(IServiceProvider provider, Type type, object target)
        {
            var proxyInstance = ActivatorUtilities.GetServiceOrCreateInstance(provider, GetProxyType(type)) as IProxy;
            proxyInstance.SetProxy(target, provider.GetService<InterceptorInvoker>());
            return proxyInstance;
        }

        internal static void Save()
        {
            _generatorUtilities.Save();
        }
    }
}
