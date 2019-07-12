using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Zebra.NetCore.Interception
{
    public interface InterceptorSelector
    {
        IEnumerable<Interceptor> Select(MethodBase method);
    }
}
