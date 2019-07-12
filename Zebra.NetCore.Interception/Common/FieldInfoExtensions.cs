using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Zebra.NetCore.Interception.Common
{
    public static class FieldInfoExtensions
    {
        private static readonly ConcurrentDictionary<FieldInfo, Func<object, object>> _getters = new ConcurrentDictionary<FieldInfo, Func<object, object>>();
        private static readonly ConcurrentDictionary<FieldInfo, Action<object, object>> _setters = new ConcurrentDictionary<FieldInfo, Action<object, object>>();

        public static Func<object, object> GetValueGetter(this FieldInfo fieldInfo)
        {
            return _getters.GetOrAdd(fieldInfo, key =>
             {
                 var instance = Expression.Parameter(typeof(object), "instance");
                 var castInstance = key.DeclaringType.IsValueType ? Expression.Convert(instance, key.DeclaringType) : Expression.TypeAs(instance, key.DeclaringType);
                 var field = Expression.Field(castInstance, key);
                 var castField = Expression.TypeAs(field, typeof(object));
                 return Expression.Lambda<Func<object, object>>(castField, instance).Compile();
             });
        }

        public static Action<object, object> GetValueSetter(this FieldInfo fieldInfo)
        {
            return _setters.GetOrAdd(fieldInfo, key =>
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var castInstance = key.DeclaringType.IsValueType ? Expression.Convert(instance, key.DeclaringType) : Expression.TypeAs(instance, key.DeclaringType);
                var argument = Expression.Parameter(typeof(object), "value");
                var castField = key.FieldType.IsValueType ? Expression.Convert(argument, key.FieldType) : Expression.TypeAs(argument, key.FieldType);
                return Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Field(castInstance, key), castField), instance, argument).Compile();
            });
        }
    }
}
