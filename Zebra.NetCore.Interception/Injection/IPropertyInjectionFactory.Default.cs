using System;
using System.Collections.Concurrent;

namespace Zebra.NetCore.Interception.Injection
{
    internal class DefaultPropertyInjectionFactory : IPropertyInjectionFactory
    {
        private readonly ConcurrentDictionary<Type, IPropertyInjection> _propertyInjections = new ConcurrentDictionary<Type, IPropertyInjection>();
        private IPropertyResolverFactory _resolverFactory;
        private IServiceProvider _serviceProvider;
        public DefaultPropertyInjectionFactory(IServiceProvider serviceProvider,
            IPropertyResolverFactory resolverFactory)
        {
            _resolverFactory = resolverFactory ?? throw new ArgumentNullException(nameof(resolverFactory));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IPropertyInjection Create(Type implementationType)
        {
            return _propertyInjections.GetOrAdd(implementationType, key => new DefaultPropertyInjection(_serviceProvider, _resolverFactory.GetResolvers(implementationType)));
        }
    }
}
