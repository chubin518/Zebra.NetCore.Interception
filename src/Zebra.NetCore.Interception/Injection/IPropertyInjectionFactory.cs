using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal interface IPropertyInjectionFactory
    {
        IPropertyInjection Create(Type implementationType);
    }
}
