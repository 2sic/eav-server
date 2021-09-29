using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ChangeTypeTests
    {

        [TestMethod]
        public void NullToDateTime()
        {
            ConvT(null, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        }

        [TestMethod]
        public void StringDateToDateTime()
        {
            ConvT("", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            ConvT("2021-01-01", new DateTime(2021, 1, 1), new DateTime(2021, 1, 1));
            ConvT("2021-12-31", new DateTime(2021, 12, 31), new DateTime(2021, 12, 31));
        }

        [TestMethod]
        public void StringToDateTime()
        {
            ConvT("", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            ConvT("2021-01-01 10:42", new DateTime(2021, 1, 1, 10, 42, 00), new DateTime(2021, 1, 1, 10, 42, 00));
            ConvT("2021-01-01 10:42:00", new DateTime(2021, 1, 1, 10, 42, 00), new DateTime(2021, 1, 1, 10, 42, 00));
            ConvT("2021-01-01 10:42:27", new DateTime(2021, 1, 1, 10, 42, 27), new DateTime(2021, 1, 1, 10, 42, 27));
        }
      
    }
}
