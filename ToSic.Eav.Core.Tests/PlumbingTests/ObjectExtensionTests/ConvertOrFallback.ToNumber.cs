using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ConvertOrFallback
    {

        [TestMethod]
        public void ObjectToInt()
        {
            // 0 should always default
            AllSameResult(new List<string>(), 27, 27);
            AllSameResult(new List<string>(), 42, 42);
        }



        [TestMethod]
        public void StringToInt()
        {
            AllSameResult("0", 333, 0);
            AllSameResult("27", 333, 27);
            AllSameResult("-423", 333, -423);
            AllSameResult("", 42, 42);
            AllSameResult("nothing", 42, 42);
        }

        [TestMethod]
        public void StringToFloat()
        {
            AllSameResult("0", 333f, 0f);
            AllSameResult("27", 333f, 27f);
            AllSameResult("-423", 333f, -423f);
            AllSameResult("", 42f, 42f);
            AllSameResult("nothing", 42f, 42f);

            // now with correct decimals
            AllSameResult("0.1", 333f, 0.1f);
            AllSameResult("27.999", 333f, 27.999f);
            AllSameResult("-42.42", 333f, -42.42f);

            // todo: comma decimals - not handled yet!
            //AllSameResult("0,1", 333f, 0.1f);
            //AllSameResult("27,999", 333f, 27.999f);
            //AllSameResult("-42,42", 333f, -42.42f);
        }
    }
}
