using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Internal
{
    internal interface InterceptorChainBuilder
    {
        IServiceProvider ServiceProvider { get; }

        InterceptorChainBuilder New();

        InterceptorChainBuilder Use(InterceptorMiddleware interception, int order);

        InterceptorMiddleware Build();
    }
}
