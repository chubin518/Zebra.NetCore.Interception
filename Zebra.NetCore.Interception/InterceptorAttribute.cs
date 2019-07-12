using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class InterceptorAttribute : Attribute, Interceptor
    {
        public virtual int Order { get; set; } = 0;

        public abstract Task Intercept(InvocationContext context, InterceptorDelegate next);
    }
}
