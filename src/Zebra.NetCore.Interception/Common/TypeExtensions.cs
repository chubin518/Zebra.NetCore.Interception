using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Zebra.NetCore.Interception.Common
{
    public static class TypeExtensions
    {
        public static bool NeedAutowaired(this Type implementationType)
        {
            foreach (var property in implementationType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.CanWrite && property.GetCustomAttributes<AutowiredAttribute>().Any())
                {
                    return true;
                }
            }

            foreach (var field in implementationType.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsInitOnly && field.GetCustomAttributes<AutowiredAttribute>().Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
