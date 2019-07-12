using System.Reflection;
using System.Threading.Tasks;
using Zebra.NetCore.Interception;

namespace Zebra.NetCore.InterceptionSimple
{
    public class Product
    {
        public string Name { get; set; }

        public string Get(long id)
        {
            return "Get";
        }

        public virtual Task<string> GetAsync(long id)
        {
            return Task.FromResult("GetAsync");
        }

        public void Save<T>(long id, string name, int age)
        {
        }

        public Task SaveAsync(long id, string name, int age)
        {
            return Task.CompletedTask;
        }

        public int Update(long id, ref int age, out string name, out Request request)
        {
            name = "";
            request = null;
            return 0;
        }
    }

    public class ProductProxy : Product
    {
        private Product _target;
        private InterceptorInvoker _invoker;

        public ProductProxy(InterceptorInvoker invoker) {
            _invoker = invoker;
            _target = this;
        }

        public override Task<string> GetAsync(long id)
        {
            MethodInfo method = null;
            InvocationContext invocationContext = new InvocationContext(this, _target, method, new object[1] { id });
            return _invoker.Invoke<Task<string>>(GetAsync, invocationContext);
        }

        private Task GetAsync(InvocationContext context)
        {
            long id = (long)context.Arguments[0];
            context.Return = base.GetAsync(id);
            return Task.CompletedTask;
        }
    }
}
