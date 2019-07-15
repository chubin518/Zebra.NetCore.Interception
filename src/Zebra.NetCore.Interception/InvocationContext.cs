using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Zebra.NetCore.Interception
{
    /// <summary>
    /// 代理方法的调用上下文
    /// </summary>
    public class InvocationContext
    {
        public InvocationContext(object proxy, object target, MethodBase method, object[] arguments)
        {
            this.Proxy = proxy;
            this.Target = target;
            this.Method = method;
            this.Arguments = arguments;
        }

        public object Proxy { get; }

        public object Target { get; }

        public MethodBase Method { get; }

        public object[] Arguments { get; }

        public object Return { get; set; }
    }
}
