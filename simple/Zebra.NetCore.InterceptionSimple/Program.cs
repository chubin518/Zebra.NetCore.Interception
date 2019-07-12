using Microsoft.Extensions.DependencyInjection;
using System;
using Zebra.NetCore.Interception;
using Zebra.NetCore.Interception.Common;

namespace Zebra.NetCore.InterceptionSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILog, Log>();
            services.AddSingleton<IOrder>(p =>
            {
                return new Order(p.GetService<ILog>()); ;
            });
            //services.AddSingleton<Order, Order>();
            IServiceProvider provider = services.BuildInterceptServiceProvider();


            provider.GetService<IOrder>().CreateOrder(new Request { });
            Console.Read();
        }
    }
}
