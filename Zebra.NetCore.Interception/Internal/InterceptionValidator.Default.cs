using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception.Internal
{
    internal class DefaultInterceptionValidator : InterceptionValidator
    {
        private InterceptorCollection _collections;
        public DefaultInterceptionValidator(InterceptorCollection collections)
        {
            _collections = collections ?? throw new ArgumentNullException(nameof(collections));
        }

        public bool Validate(ServiceDescriptor descriptor)
        {
            bool needProxy = false;
            Type serviceType = descriptor.ServiceType, implementationType = GetImplementationType(descriptor);
            if (serviceType.IsInterface && implementationType.IsClass)
            {
                var mapping = implementationType.GetInterfaceMap(serviceType);
                for (int i = 0; i < mapping.InterfaceMethods.Length; i++)
                {
                    if (_collections.TryAddValue(mapping.InterfaceMethods[i], mapping.TargetMethods[i]))
                    {
                        needProxy = true;
                    }
                }
            }
            else
            {
                foreach (var method in implementationType.GetMethods())
                {
                    if (_collections.TryAddValue(method, method))
                    {
                        needProxy = true;
                    }
                }
            }
            return needProxy;
        }

        private Type GetImplementationType(ServiceDescriptor descriptor)
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
    }
}
