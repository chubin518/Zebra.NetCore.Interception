using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zebra.NetCore.Interception.Internal
{
    internal interface InterceptionValidator
    {
        bool Validate(ServiceDescriptor descriptor);
    }
}
