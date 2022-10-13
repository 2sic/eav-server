using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Configuration;
using ToSic.Eav.Serialization;

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

            var ser = JsonSerializer.Serialize(x, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            Trace.WriteLine(ser);
        }
    }
}
