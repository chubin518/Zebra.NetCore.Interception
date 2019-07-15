using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;
using Zebra.NetCore.Interception.Internal;

namespace Zebra.NetCore.InterceptionSimple
{

    public interface IProduct
    {
        string Name { get; set; }

        Task<string> GetAsync(long id);
        string Get(long id);
        void Save<T>(long id, string name, int age);
        Task SaveAsync(long id, string name, int age);

        int Update(long id, ref int age, out string name, out Request request);
    }

    public class IProductProxy : IProduct, IProxy
    {
        private IProduct _target;
        private InterceptorInvoker _invoker;

        public string Name { get; set; }

        public string Get(long id)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[1] { id });
            return _invoker.Invoke<string>(Get, invocationContext);
        }
        public Task Get(InvocationContext ctx)
        {
            int arg0 = (int)ctx.Arguments[0];
            ctx.Return = _target.Get(arg0);
            return Task.CompletedTask;
        }

        public Task<string> GetAsync(long id)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[1] { id });
            return _invoker.Invoke<Task<string>>(GetAsync, invocationContext);
        }

        private Task GetAsync(InvocationContext context)
        {
            int id = (int)context.Arguments[0];
            context.Return = _target.GetAsync(id);
            return Task.CompletedTask;
        }

        public void Save<T>(long id, string name, int age)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[3] { id, name, age });
            _invoker.Invoke<object>(context =>
            {
                long arg0 = (long)context.Arguments[0];
                string arg1 = (string)context.Arguments[1];
                int arg2 = (int)context.Arguments[2];
                _target.Save<T>(arg0, arg1, arg2);
                return Task.CompletedTask;
            }, invocationContext);
        }

        //public Task Save(InvocationContext context)
        //{
        //    long arg0 = (long)context.Arguments[0];
        //    string arg1 = (string)context.Arguments[1];
        //    int arg2 = (int)context.Arguments[2];
        //    _target.Save(arg0, arg1, arg2);
        //    return Task.CompletedTask;
        //}

        public Task SaveAsync(long id, string name, int age)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[3] { id, name, age });
            _invoker.Invoke<object>(Get, invocationContext);
            return Task.CompletedTask;
        }
        public Task SaveAsync(InvocationContext context)
        {
            long arg0 = (long)context.Arguments[0];
            string arg1 = (string)context.Arguments[1];
            int arg2 = (int)context.Arguments[2];
            context.Return = _target.SaveAsync(arg0, arg1, arg2);
            return Task.CompletedTask;
        }

        public int Update(long id, ref int age, out string name, out Request request)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[4] { id, age, default(string), default(Request) });

            var result = _invoker.Invoke<int>(context =>
              {
                  object[] arguments = context.Arguments;
                  long arg0 = (long)arguments[0];
                  int arg1 = (int)arguments[1];
                  string arg2 = (string)arguments[2];
                  Request arg3 = arguments[3] as Request;
                  context.Return = _target.Update(arg0, ref arg1, out arg2, out arg3);
                  arguments[1] = arg1;
                  arguments[2] = arg2;

                  return Task.CompletedTask;
              }, invocationContext);

            age = (int)invocationContext.Arguments[1];
            name = (string)invocationContext.Arguments[2];
            request = (Request)invocationContext.Arguments[2];
            return result;
        }

        public void SetProxy(object target)
        {
            this._target = target as IProduct;
        }

        public void SetInvoker(InterceptorInvoker invoker)
        {
            this._invoker = invoker;
        }

        public void SetProxy(object target, InterceptorInvoker invoker)
        {
            this._target = target as IProduct;
            this._invoker = invoker;
        }
    }
}
