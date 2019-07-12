using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal class PropertyResolver
    {
        private Action<object, object> _setter;
        private Func<IServiceProvider, object> _getInstance;
        public PropertyResolver(Action<object, object> setter, Func<IServiceProvider, object> getInstance)
        {
            _setter = setter;
            _getInstance = getInstance;
        }

        public void Resolve(IServiceProvider provider, object implementationInstance)
        {
            _setter(implementationInstance, _getInstance(provider));
        }
    }
}
