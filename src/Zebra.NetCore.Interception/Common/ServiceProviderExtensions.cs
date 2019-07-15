using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zebra.NetCore.Interception.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace Zebra.NetCore.Interception.Common
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider PropertyInjection(this IServiceProvider provider, object implementationInstance)
        {
            if (implementationInstance != null)
            {
                var propertyResolverFactory = provider.GetRequiredService<IPropertyResolverFactory>();
                Type implementationType = implementationInstance.GetType();
                if (propertyResolverFactory.GetResolvers(implementationType).Any())
                {
                    provider.GetService<IPropertyInjectionFactory>().Create(implementationType).Inject(implementationInstance);
                }
            }
            return provider;
        }
    }
}
