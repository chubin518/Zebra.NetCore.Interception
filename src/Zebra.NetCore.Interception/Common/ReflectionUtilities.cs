using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Zebra.NetCore.Interception.Common
{
    public class ReflectionUtilities
    {
        public static MethodInfo GetMethod<TTarget>(Expression<Action<TTarget>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (expression.Body is MethodCallExpression exprMethod)
            {
                return exprMethod.Method;
            }
            throw new InvalidCastException("Cannot be converted to MethodCallExpression");
        }

        public static ConstructorInfo GetConstructor<TTarget>(Expression<Func<TTarget>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (expression.Body is NewExpression exprNew)
                return exprNew.Constructor;
            throw new InvalidCastException("Cannot be converted to NewExpression");
        }

        public static PropertyInfo GetProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (expression.Body is MemberExpression exprMember)
            {
                if (exprMember.Member is PropertyInfo property)
                {
                    return property;
                }
                throw new InvalidCastException("Cannot be converted to PropertyInfo");
            }
            throw new InvalidCastException("Cannot be converted to MemberExpression");
        }

        public static MethodInfo GetMethod<TTarget>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(TTarget).GetTypeInfo().GetMethod(name);
        }
    }
}
