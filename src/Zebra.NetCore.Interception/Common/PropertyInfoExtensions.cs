using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Zebra.NetCore.Interception.Common
{
    public static class PropertyInfoExtensions
    {
        private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> _getters = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();
        private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object>> _setters = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo)
        {
            return _getters.GetOrAdd(propertyInfo, key =>
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                UnaryExpression instanceCast = (!key.DeclaringType.IsValueType) ? Expression.TypeAs(instance, key.DeclaringType) : Expression.Convert(instance, key.DeclaringType);
                return Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, key.SetMethod), typeof(object)), instance).Compile();
            });
        }

        public static Action<object, object> GetValueSetter(this PropertyInfo propertyInfo)
        {
            return _setters.GetOrAdd(propertyInfo, property =>
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var value = Expression.Parameter(typeof(object), property.Name);
                UnaryExpression instanceCast = (!property.DeclaringType.IsValueType) ? Expression.TypeAs(instance, property.DeclaringType) : Expression.Convert(instance, property.DeclaringType);
                UnaryExpression valueCast = (!property.PropertyType.IsValueType) ? Expression.TypeAs(value, property.PropertyType) : Expression.Convert(value, property.PropertyType);
                return Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, property.SetMethod, valueCast), new ParameterExpression[] { instance, value }).Compile();
            });
        }
    }
}
