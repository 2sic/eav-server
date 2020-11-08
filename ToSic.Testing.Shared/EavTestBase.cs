using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Core.Tests
{
    public abstract class EavTestBase
    {
        public static T Resolve<T>() => Factory.Resolve<T>();
    }
}
