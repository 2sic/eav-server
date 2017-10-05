using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    public class Efc11TestBase
    {
        #region test preparations

        internal EavDbContext Db;
        internal Efc11Loader Loader;

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            Db = Factory.Resolve<EavDbContext>();
            Loader = NewLoader();
        }

        internal Efc11Loader NewLoader() => new Efc11Loader(Db);

        #endregion

    }
}
