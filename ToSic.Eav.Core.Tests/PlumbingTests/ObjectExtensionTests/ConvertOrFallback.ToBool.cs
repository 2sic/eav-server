using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ConvertOrFallback
    {
        [TestMethod]
        public void NumberToBool()
        {
            // 0 should false
            AllSameResult(0, true, false);
            AllSameResult(0, false, false);

            // All other numbers should true
            AllSameResult(1, true, true);
            AllSameResult(1, false, true);
            AllSameResult(2, true, true);
            AllSameResult(2, false, true);
            AllSameResult(-1, true, true);
            AllSameResult(-1, false, true);
            AllSameResult(27.3, true, true);
            AllSameResult(27.3, false, true);
            AllSameResult(0.1, true, true);
            AllSameResult(0.1, false, true);
        }

        [TestMethod]
        public void NumberNullableToBool()
        {
            // 0 should false
            AllSameResult(new int?(), true, true);
            AllSameResult(new int?(), false, false);
            AllSameResult(new int?(0), true, false);
            AllSameResult(new int?(0), false, false);

            // All other numbers should true
            AllSameResult(new int?(1), true, true);
            AllSameResult(new int?(1), false, true);
            AllSameResult(new int?(2), true, true);
            AllSameResult(new int?(2), false, true);
            AllSameResult(new int?(-1), true, true);
            AllSameResult(new int?(-1), false, true);
            AllSameResult(new float?(27.3f), true, true);
            AllSameResult(new float?(27.3f), false, true);
            AllSameResult(new double?(0.1), true, true);
            AllSameResult(new double?(0.1), false, true);
        }

        [TestMethod]
        public void ObjectToBool()
        {
            // all objects should default
            AllSameResult(new List<string>(), true, true);
            AllSameResult(new List<string>(), false, false);
        }
        [TestMethod]
        public void StringToBool()
        {
            // Nulls should always false
            AllSameResult(null, true, true);
            AllSameResult(null, false, false);

            // Strange strings should always false
            AllSameResult("", false, false);
            AllSameResult("null", false, false);
            AllSameResult("random", false, false);

            // True strings should true
            AllSameResult("true", false, true);
            AllSameResult("TRUE", false, true);
            AllSameResult(" TRUE", false, true);
            AllSameResult("  TRUE  ", false, true);

            // False strings should always false
            AllSameResult("false", true, false);
            AllSameResult("False", true, false);
            AllSameResult("FALSE", true, false);
            AllSameResult(" false", true, false);
            AllSameResult("  false  ", true, false);
        }

    }
}
