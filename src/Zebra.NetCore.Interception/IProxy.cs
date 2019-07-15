using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception
{
    public interface IProxy
    {
        void SetProxy(object target, InterceptorInvoker invoker);
    }
}
