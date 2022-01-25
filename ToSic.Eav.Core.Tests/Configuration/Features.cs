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
            var x = new FeatureListStored();
            x.Features.Add(new FeatureConfig
            {
                Id = new Guid(),
                Enabled = true,
                Expires = DateTime.Today
            });

            x.Features.Add(new FeatureConfig
            {
                Id = new Guid(),
                Enabled = false,
                Expires = DateTime.Today
            });

            x.Features.Add(new FeatureConfig
            {
                Id = new Guid(),
                Enabled = false,
                Expires = DateTime.Today.AddDays(-1)
            });

            var ser = JsonConvert.SerializeObject(x);
            Trace.WriteLine(ser);
        }
    }
}
