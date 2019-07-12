using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Zebra.NetCore.Interception.Internal
{
    internal interface InterceptorCollector
    {
        IEnumerable<Interceptor> Collect(MethodBase method);
    }
}
