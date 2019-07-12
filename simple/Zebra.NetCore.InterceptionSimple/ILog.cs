using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.InterceptionSimple
{
   public interface ILog
    {
        void Write(object obj);
    }

    public class Log : ILog
    {
        public void Write(object obj)
        {
            Console.WriteLine(obj);
        }
    }
}
