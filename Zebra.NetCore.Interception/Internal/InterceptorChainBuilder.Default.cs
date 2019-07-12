using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Zebra.NetCore.Interception.Internal
{
    internal class DefaultInterceptorChainBuilder : InterceptorChainBuilder
    {
        private readonly IList<Tuple<InterceptorMiddleware, int>> _interceptions = new List<Tuple<InterceptorMiddleware, int>>();
        public IServiceProvider ServiceProvider { get; }

        public DefaultInterceptorChainBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public InterceptorMiddleware Build()
        {
            return next =>
            {
                foreach (var interception in _interceptions.OrderBy(item => item.Item2).Select(item => item.Item1).Reverse())
                {
                    next = interception(next);
                }
                return next;
            };
        }

        public InterceptorChainBuilder New()
        {
            return new DefaultInterceptorChainBuilder(ServiceProvider);
        }

        public InterceptorChainBuilder Use(InterceptorMiddleware interception, int order)
        {
            _interceptions.Add(Tuple.Create(interception, order));
            return this;
        }
    }
}
