using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;

namespace Zebra.NetCore.InterceptionSimple.il
{
    public interface IProductGeneric<T>
    {
        void Save();
    }

    public class IProductGeneric_Proxy<T> : IProductGeneric<T>, IProxy
    {
        private IProductGeneric<T> _target;
        private InterceptorInvoker _invoker;
        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SetProxy(object target, InterceptorInvoker invoker)
        {
            this._target = target as IProductGeneric<T>;
            this._invoker = invoker;
        }
    }
}
