using System;
using System.Threading.Tasks;
using Zebra.NetCore.Interception.Common;
using Zebra.NetCore.Interception.Internal;

namespace Zebra.NetCore.Interception
{
    internal class DefaultInterceptorInvoker : InterceptorInvoker
    {
        private InterceptorCollection _collections;

        public DefaultInterceptorInvoker(InterceptorCollection collection)
        {
            _collections = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public TResult Invoke<TResult>(InterceptorDelegate interceptor, InvocationContext context)
        {
            Task task = null;
            if (_collections.TryGetValue(context.Method, out InterceptorMiddleware middleware))
            {
                task = middleware(interceptor)(context);
            }
            else
            {
                task = interceptor(context);
            }
            if (task.IsFaulted)
                throw task.Exception.InnerException;
            if (!task.IsCompleted)
            {
                NoSyncContextScope.Run(task);
            }
            return (TResult)context.Return;
        }

        public async Task<TResult> InvokeAsync<TResult>(InterceptorDelegate interceptor, InvocationContext context)
        {
            if (_collections.TryGetValue(context.Method, out InterceptorMiddleware middleware))
            {
                await middleware(interceptor)(context);
            }
            else
            {
                await interceptor(context);
            }

            if (context.Return is Task<TResult> taskWithResult)
            {
                return await taskWithResult;
            }
            else if (context.Return is Task task)
            {
                await task;
                return default(TResult);
            }
            else
            {
                throw new InvalidCastException($"无法将返回值类型转换为{typeof(Task<TResult>)}");
            }
        }
    }
}
