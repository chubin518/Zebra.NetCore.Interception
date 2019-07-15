using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception
{
    public interface InterceptorInvoker
    {
        TResult Invoke<TResult>(InterceptorDelegate interceptor, InvocationContext context);

        Task<TResult> InvokeAsync<TResult>(InterceptorDelegate interceptor, InvocationContext context);
    }
}
