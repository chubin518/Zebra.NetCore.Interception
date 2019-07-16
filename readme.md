# Zebra.NetCore.Interception

Zebra.NetCore.Interception是基于Microsoft.Extensions.DependencyInjection的拓展程序

提供了通过类的属性（字段）进行注入的功能

使用IL Emit技术实现了动态代理功能，并在此基础上提供了轻量级的AOP（面向切面编程解决方案）



## 如何使用？


### 使用 Nuget 安装 Zebra.NetCore.Interception 包


```
PM> Install-Package Zebra.NetCore.Interception
```


### 创建拦截器


 ```
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
 ```


### 指定要拦截的类型

```

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
        /// 当拦截器打在类的方法上时，该方法一定要是virtual或abstract方法
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

```

### 配置代理类型的容器并测试

```
IServiceCollection services = new ServiceCollection();
services.AddSingleton<ILog, Log>();
services.AddSingleton<IOrder, Order>();
// 重点BuildInterceptServiceProvider ,创建代理类的容器
IServiceProvider provider = services.BuildInterceptServiceProvider();

// 测试
IOrder order = provider.GetService<IOrder>();
order.CreateOrder(new Request { });
```


           

