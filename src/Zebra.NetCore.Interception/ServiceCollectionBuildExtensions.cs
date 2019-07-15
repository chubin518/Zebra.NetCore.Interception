using Microsoft.Extensions.DependencyInjection;
using System;
using Zebra.NetCore.Interception.Common;
using Zebra.NetCore.Interception.Injection;
using Zebra.NetCore.Interception.Internal;
using System.Linq;

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
                var propertyResolverFactory = serviceProvider.GetRequiredService<IPropertyResolverFactory>();
                foreach (var service in services)
                {
                    if (serviceValidator.Validate(service))
                    {
                        interceptorCollections.Add(CreateDynamicProxy(service));
                    }
                    else
                    {
                        interceptorCollections.Add(SetPropertyInjection(service, propertyResolverFactory));
                    }
                }
            }
            return interceptorCollections;
        }

        private static ServiceDescriptor SetPropertyInjection(ServiceDescriptor descriptor, IPropertyResolverFactory resolverFactory)
        {
            if (resolverFactory.GetResolvers(descriptor.GetImplementationType()).Any())
            {
                if (null != descriptor.ImplementationInstance)
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        provider.PropertyInjection(descriptor.ImplementationInstance);
                        return descriptor.ImplementationInstance;
                    }, descriptor.Lifetime);
                }
                else if (null != descriptor.ImplementationFactory)
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        var instance = descriptor.ImplementationFactory(provider);
                        provider.PropertyInjection(instance);
                        return instance;
                    }, descriptor.Lifetime);
                }
                else
                {
                    return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                    {
                        var instance = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);
                        provider.PropertyInjection(instance);
                        return instance;
                    }, descriptor.Lifetime);
                }
            }
            return descriptor;
        }
        private static ServiceDescriptor CreateDynamicProxy(ServiceDescriptor descriptor)
        {
            if (null != descriptor.ImplementationInstance)
            {
                return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                 {
                     provider.PropertyInjection(descriptor.ImplementationInstance);
                     return DynamicProxyFactory.CreateProxyInstance(provider, descriptor.ServiceType, descriptor.ImplementationInstance);
                 }, descriptor.Lifetime);
            }
            else if (null != descriptor.ImplementationFactory)
            {
                return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                {
                    var instance = descriptor.ImplementationFactory(provider);
                    provider.PropertyInjection(instance);
                    return DynamicProxyFactory.CreateProxyInstance(provider, descriptor.ServiceType, instance);
                }, descriptor.Lifetime);
            }
            else
            {
                return ServiceDescriptor.Describe(descriptor.ServiceType, provider =>
                {
                    var instance = ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);
                    provider.PropertyInjection(instance);
                    return DynamicProxyFactory.CreateProxyInstance(provider, descriptor.ServiceType, instance);
                }, descriptor.Lifetime);
            }
        }

        internal static IServiceCollection TryAddInterceptServices(this IServiceCollection services)
        {
            services.AddScoped<InterceptorChainBuilder, DefaultInterceptorChainBuilder>();
            services.AddScoped<InterceptionValidator, DefaultInterceptionValidator>();
            services.AddScoped<InterceptorSelector, AttributeInterceptorSelector>();
            services.AddScoped<InterceptorCollector, DefaultInterceptorCollector>();

            services.AddSingleton<IPropertyInjectionFactory, DefaultPropertyInjectionFactory>();
            services.AddSingleton<IPropertyResolverFactory, DefaultPropertyResolverFactory>();
            services.AddSingleton<InterceptorInvoker, DefaultInterceptorInvoker>();
            services.AddSingleton<InterceptorCollection, DefaultInterceptorCollection>();
            return services;
        }
    }
}
