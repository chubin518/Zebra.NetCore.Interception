using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception
{
    /// <summary>
    /// 处理方法拦截的函数
    /// </summary>
    /// <param name="invocation"></param>
    /// <returns></returns>
    public delegate Task InterceptorDelegate(InvocationContext invocation);

    /// <summary>
    /// 处理方法拦截的中间件
    /// </summary>
    /// <param name="next"></param>
    /// <returns></returns>
    public delegate InterceptorDelegate InterceptorMiddleware(InterceptorDelegate next);
}
