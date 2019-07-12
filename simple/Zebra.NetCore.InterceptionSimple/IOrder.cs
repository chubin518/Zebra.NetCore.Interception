using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.InterceptionSimple
{
    public interface IOrder
    {
        void CreateOrder(Request request);

        Task UpdateOrder(Request request, long id);

        Task<Request> GetOrder(long id);
    }

    public class Order : IOrder
    {
        private ILog _log;
        public Order(ILog log)
        {
            _log = log;
        }
        [LogInterceptor]
        public virtual void CreateOrder(Request request)
        {
            _log.Write("CreateOrder");
        }

        [LogInterceptor]
        public Task<Request> GetOrder(long id)
        {
            _log.Write("GetOrder");
            return Task.FromResult(new Request { });
        }

        [LogInterceptor]
        public Task UpdateOrder(Request request, long id)
        {
            _log.Write("GetOrder");
            return Task.CompletedTask;
        }
    }
}
