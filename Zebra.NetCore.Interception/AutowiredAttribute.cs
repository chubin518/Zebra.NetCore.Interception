using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AutowiredAttribute : Attribute
    {

    }
}
