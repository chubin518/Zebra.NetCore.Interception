using System;
using System.Collections.Generic;
using System.Text;

namespace Zebra.NetCore.Interception.Injection
{
    internal class PropertyResolver
    {
        private Action<object, object> _propSetter;
        private Func<IServiceProvider, object> _propInstanceFactory;

        public PropertyResolver(Action<object, object> setter, Func<IServiceProvider, object> getInstance)
        {
            _propSetter = setter;
            _propInstanceFactory = getInstance;
        }

        public void Resolve(object implementationInstance, IServiceProvider provider)
        {
            _propSetter(implementationInstance, _propInstanceFactory(provider));
        }
    }
}
