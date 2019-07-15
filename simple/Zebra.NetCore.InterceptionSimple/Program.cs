using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
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
            services.AddSingleton<IOrder, Order>();
            // 重点BuildInterceptServiceProvider ,创建代理类的容器
            IServiceProvider provider = services.BuildInterceptServiceProvider();

            // 测试
            IOrder order = provider.GetService<IOrder>();
            order.CreateOrder(new Request { });

            Console.Read();
        }
    }
}
