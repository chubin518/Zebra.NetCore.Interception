using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Zebra.NetCore.Interception
{
    internal class AttributeInterceptorSelector : InterceptorSelector
    {
        public IEnumerable<Interceptor> Select(MethodBase method)
        {
            foreach (var attribute in method.GetCustomAttributes())
            {
                if (attribute is Interceptor interceptor)
                    yield return interceptor;
            }
        }
    }
}
