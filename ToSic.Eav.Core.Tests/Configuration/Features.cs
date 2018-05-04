using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.Core.Tests.Configuration
{
    [TestClass]
    public class Features
    {
        [TestMethod]
        public void Features_Export_Test()
        {
            var x = new FeatureList();
            x.Features.Add(new Feature()
            {
                Enabled = true,
                Id = new Guid(),
                Expires = DateTime.Today
            });

            x.Features.Add(new Feature()
            {
                Enabled = false,
                Id = new Guid(),
                Expires = DateTime.Today
            });

            x.Features.Add(new Feature()
            {
                Enabled = false,
                Id = new Guid(),
                Expires = DateTime.Today.AddDays(-1)
            });

            var ser = JsonConvert.SerializeObject(x);
            Trace.WriteLine(ser);
        }
    }
}
