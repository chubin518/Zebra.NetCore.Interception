using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal interface IPropertyInjection
    {
        void Inject(object implementationInstance);
    }
}
