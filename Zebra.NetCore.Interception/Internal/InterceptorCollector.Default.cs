using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zebra.NetCore.Interception.Injection;

namespace Zebra.NetCore.Interception.Internal
{
    internal class DefaultInterceptorCollector : InterceptorCollector
    {
        private IServiceProvider _serviceProvider;
        private IEnumerable<InterceptorSelector> _selectors;
        private IPropertyInjectionFactory _propertyInjectionFactory;

        public DefaultInterceptorCollector(IServiceProvider serviceProvider,
            IEnumerable<InterceptorSelector> selectors,
            IPropertyInjectionFactory propertyInjectionFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _selectors = selectors ?? throw new ArgumentNullException(nameof(selectors));
            _propertyInjectionFactory = propertyInjectionFactory ?? throw new ArgumentNullException(nameof(propertyInjectionFactory));
        }

        public IEnumerable<Interceptor> Collect(MethodBase method)
        {
            return CollectFromSelector(method).Select(item => CreateInterceptorInstance(item));
        }

        private IEnumerable<Interceptor> CollectFromSelector(MethodBase method)
        {
            foreach (var selector in _selectors)
            {
                foreach (var interceptor in selector.Select(method))
                {
                    if (interceptor != null)
                        yield return interceptor;
                }
            }
        }

        private Interceptor CreateInterceptorInstance(Interceptor item)
        {
            Type interceptorType = item.GetType();
            object instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, interceptorType);
            this._propertyInjectionFactory.Create(interceptorType).Inject(instance);
            return instance as Interceptor;
        }
    }
}
