using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal class DefaultPropertyInjection : IPropertyInjection
    {
        private IEnumerable<PropertyResolver> _resolvers;
        private IServiceProvider _serviceProvider;
        public DefaultPropertyInjection(IServiceProvider serviceProvider,
            IEnumerable<PropertyResolver> resolvers)
        {
            _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Inject(object implementationInstance)
        {
            if (null == implementationInstance)
                return;
            foreach (var resolve in _resolvers)
            {
                resolve.Resolve(implementationInstance, _serviceProvider);
            }
        }
    }
}
