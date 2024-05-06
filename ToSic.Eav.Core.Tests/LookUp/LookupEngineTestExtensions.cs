using System.Collections.Generic;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Core.Tests.LookUp
{
    internal static class LookupEngineTestExtensions
    {
        public static void TestAdd(this LookUpEngine engine, ILookUp part) => engine.Add(part);

        //public static void TestAdd(this LookUpEngine engine, IList<ILookUp> part) => engine.Add(part);
    }
}
