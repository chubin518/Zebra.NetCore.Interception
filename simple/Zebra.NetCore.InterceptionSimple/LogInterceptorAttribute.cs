using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;

namespace Zebra.NetCore.InterceptionSimple
{
    public class LogInterceptorAttribute : InterceptorAttribute
    {
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
