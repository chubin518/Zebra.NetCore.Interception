using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception.Common
{
    public static class ServiceDescriptorExtesions
    {
        public static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance.GetType();
            }
            else if (descriptor.ImplementationFactory != null)
            {
                var typeArguments = descriptor.ImplementationFactory.GetType().GenericTypeArguments;
                return typeArguments[1];
            }
            return descriptor.ImplementationType;
        }

        public static ServiceDescriptor PropertyInjection(this ServiceDescriptor descriptor)
        {
            if (descriptor.GetImplementationType().NeedAutowaired())
            {
                if (null != descriptor.ImplementationInstance)
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        provider.PropertyInjection(descriptor.ImplementationInstance);
                        return descriptor.ImplementationInstance;
                    }, descriptor.Lifetime);
                }
                else if (null != descriptor.ImplementationFactory)
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        var instance = descriptor.ImplementationFactory(provider);
                        provider.PropertyInjection(instance);
                        return instance;
                    }, descriptor.Lifetime);
                }
                else
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        var instance = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);
                        provider.PropertyInjection(instance);
                        return instance;
                    }, descriptor.Lifetime);
                }
            }
            return descriptor;
        }
    }
}
