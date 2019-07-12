using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception
{
    public interface Interceptor
    {
        int Order { get; set; }

        Task Intercept(InvocationContext context, InterceptorDelegate next);
    }
}
