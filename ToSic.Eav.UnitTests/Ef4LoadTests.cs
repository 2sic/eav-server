using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Repository.EF4;

namespace ToSic.Eav.Persistence.EFC11.Tests
{
    [TestClass]
    public class Ef4LoadTests
    {

        [TestMethod]
        public void TryToLoadCtsOf2()
        {
            var dbc = DbDataController.Instance(null, 2);
            var loader = new DbLoadIntoEavDataStructure(dbc);
            var types = loader.GetEavContentTypes(2);

        }
    }
}
