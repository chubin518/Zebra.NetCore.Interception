using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;

namespace Zebra.NetCore.InterceptionSimple
{
    public interface IOrder
    {
        void CreateOrder(Request request);

        [LogInterceptor]
        Task UpdateOrder(Request request, long id);

        [LogInterceptor]
        Task<Request> GetOrder(long id);
    }

    public class Order : IOrder
    {
        [Autowired]
        private ILog _log;

        /// <summary>
        /// 当拦截特性打在一个类的实现方法上时，该方法一定要是virtual或abstract方法
        /// </summary>
        /// <param name="request"></param>
        [LogInterceptor]
        public virtual void CreateOrder(Request request)
        {
            _log.Write("执行方法CreateOrder");
        }

        public Task<Request> GetOrder(long id)
        {
            _log.Write("执行方法GetOrder");
            return Task.FromResult(new Request { });
        }

        public Task UpdateOrder(Request request, long id)
        {
            _log.Write("执行方法GetOrder");
            return Task.CompletedTask;
        }
    }
}
