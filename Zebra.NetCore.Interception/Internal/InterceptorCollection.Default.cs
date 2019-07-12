using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
namespace Zebra.NetCore.Interception.Internal
{
    internal class DefaultInterceptorCollection : InterceptorCollection
    {
        private readonly static IDictionary<int, InterceptorMiddleware> _interceptors = new Dictionary<int, InterceptorMiddleware>();
        private InterceptorCollector _collector;
        private InterceptorChainBuilder _builder;

        public DefaultInterceptorCollection(InterceptorCollector collector, InterceptorChainBuilder builder)
        {
            this._collector = collector ?? throw new ArgumentNullException(nameof(collector));
            this._builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public bool TryAddValue(MethodBase method, MethodBase targetMethod)
        {
            var interceptors = _collector.Collect(targetMethod);
            if (interceptors.Any())
            {
                var builder = this._builder.New();
                foreach (var item in interceptors)
                {
                    builder.Use(next => context => item.Intercept(context, next), item.Order);
                }
                _interceptors[method.MetadataToken] = builder.Build();
                return true;
            }
            return false;
        }

        public bool TryGetValue(MethodBase method, out InterceptorMiddleware value)
        {
            return _interceptors.TryGetValue(method.MetadataToken, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _interceptors.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<int, InterceptorMiddleware>> GetEnumerator()
        {
            return _interceptors.GetEnumerator();
        }

    }
}
