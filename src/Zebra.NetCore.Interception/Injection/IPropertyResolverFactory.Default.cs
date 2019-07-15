using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Zebra.NetCore.Interception.Common;

namespace Zebra.NetCore.Interception.Injection
{
    internal class DefaultPropertyResolverFactory : IPropertyResolverFactory
    {
        private readonly ConcurrentDictionary<Type, PropertyResolver[]> propertyResolverCache = new ConcurrentDictionary<Type, PropertyResolver[]>();

        public IEnumerable<PropertyResolver> GetResolvers(Type implementationType)
        {
            return propertyResolverCache.GetOrAdd(implementationType, key => GetPropertyResolvers(key).ToArray());
        }

        private IEnumerable<PropertyResolver> GetPropertyResolvers(Type implementationType)
        {
            foreach (var property in implementationType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.CanWrite && property.GetCustomAttributes<AutowiredAttribute>().Any())
                {
                    yield return new PropertyResolver(property.GetValueSetter(), provier => provier.GetService(property.PropertyType));
                }
            }

            foreach (var field in implementationType.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsInitOnly && field.GetCustomAttributes<AutowiredAttribute>().Any())
                {
                    yield return new PropertyResolver(field.GetValueSetter(), provier => provier.GetService(field.FieldType));
                }
            }
        }
    }
}
