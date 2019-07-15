using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;

namespace Zebra.NetCore.InterceptionSimple
{
    /// <summary>
    /// 继承抽象类：InterceptorAttribute
    /// </summary>
    public class LogInterceptorAttribute : InterceptorAttribute
    {
        /// <summary>
        /// 如果拦截组件依赖于其他外部类，可以通过Autowired特性进行基于属性的注入
        /// </summary>
        [Autowired]
        ILog Log { get; set; }

        public override async Task Intercept(InvocationContext context, InterceptorDelegate next)
        {
            this.Log.Write("方法执行之前，当前参数值：" + string.Join(",", context.Arguments));
            await next(context);
            Thread.Sleep(1000);
            this.Log.Write("方法执行之后，当前返回值：" + context.Return);
        }
    }
}
