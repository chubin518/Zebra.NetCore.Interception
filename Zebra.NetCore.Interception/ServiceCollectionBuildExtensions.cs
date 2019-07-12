using Microsoft.Extensions.DependencyInjection;
using System;
using Zebra.NetCore.Interception.Common;
using Zebra.NetCore.Interception.Injection;
using Zebra.NetCore.Interception.Internal;

namespace Zebra.NetCore.Interception
{
    public static class ServiceCollectionBuildExtensions
    {
        public static IServiceProvider BuildInterceptServiceProvider(this IServiceCollection services)
        {
            return services.WeaveInterceptService().BuildServiceProvider();
        }

        internal static IServiceCollection WeaveInterceptService(this IServiceCollection services)
        {
            IServiceCollection interceptorCollections = new ServiceCollection();
            using (var serviceProvider = services.TryAddInterceptServices().BuildServiceProvider())
            {
                var serviceValidator = serviceProvider.GetRequiredService<InterceptionValidator>();
                ProxyGeneratorUtils generatorUtils = serviceProvider.GetRequiredService<ProxyGeneratorUtils>();
                foreach (var service in services)
                {
                    if (serviceValidator.Validate(service))
                    {
                        interceptorCollections.Add(CreateDynamicProxy(service, generatorUtils));
                    }
                    else
                    {
                        interceptorCollections.Add(service);
                    }
                }
            }
            return interceptorCollections;
        }
        private static ServiceDescriptor CreateDynamicProxy(ServiceDescriptor descriptor, ProxyGeneratorUtils generatorUtils)
        {
            if (null != descriptor.ImplementationInstance)
            {
                return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                 {
                     return ActivatorUtilities.CreateInstance(provider, generatorUtils.CreateProxy(descriptor.ServiceType), descriptor.ImplementationInstance);
                 }, descriptor.Lifetime);
            }
            else if (null != descriptor.ImplementationFactory)
            {
                return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                {
                    return ActivatorUtilities.CreateInstance(provider, generatorUtils.CreateProxy(descriptor.ServiceType), descriptor.ImplementationFactory(provider));
                }, descriptor.Lifetime);
            }
            else
            {
                if (descriptor.ServiceType.IsClass)
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, generatorUtils.CreateProxy(descriptor.ServiceType), descriptor.Lifetime);
                }
                else
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        return ActivatorUtilities.CreateInstance(provider, generatorUtils.CreateProxy(descriptor.ServiceType), ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType));
                    }, descriptor.Lifetime);
                }
            }
        }

        internal static IServiceCollection TryAddInterceptServices(this IServiceCollection services)
        {
            services.AddScoped<InterceptorChainBuilder, DefaultInterceptorChainBuilder>();
            services.AddScoped<InterceptionValidator, DefaultInterceptionValidator>();
            services.AddScoped<IPropertyInjectionFactory, DefaultPropertyInjectionFactory>();
            services.AddScoped<IPropertyResolverFactory, DefaultPropertyResolverFactory>();
            services.AddScoped<InterceptorSelector, AttributeInterceptorSelector>();
            services.AddScoped<InterceptorCollector, DefaultInterceptorCollector>();
            services.AddScoped<ProxyGeneratorUtils, ProxyGeneratorUtils>();

            services.AddSingleton<InterceptorInvoker, DefaultInterceptorInvoker>();
            services.AddSingleton<InterceptorCollection, DefaultInterceptorCollection>();
            return services;
        }
    }
}
