using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Zebra.NetCore.Interception.Internal
{
    internal interface InterceptorCollection : IEnumerable, IEnumerable<KeyValuePair<int, InterceptorMiddleware>>
    {
        bool TryGetValue(MethodBase method, out InterceptorMiddleware value);

        bool TryAddValue(MethodBase method,MethodBase targetMethod);
    }
}
