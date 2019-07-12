using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal interface IPropertyResolverFactory
    {
        IEnumerable<PropertyResolver> GetResolvers(Type implementationType);
    }
}
