using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception.Common
{
    public static class MethodInfoExtensions
    {
        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool IsAsyncMethod(this MethodInfo method)
        {
            if (method.IsTask())
            {
                return true;
            }
            if (method.IsTaskWithResult())
            {
                return true;
            }
            return false;
        }

        public static bool IsTask(this MethodInfo method)
        {
            return method.ReturnType == typeof(Task);
        }

        public static bool IsTaskWithResult(this MethodInfo method)
        {
            var returnType = method.ReturnType;
            return returnType.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);
        }
    }
}
