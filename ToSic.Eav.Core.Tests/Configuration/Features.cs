using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Serialization;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Core.Tests.Configuration;

[TestClass]
public class Features
{
    [TestMethod]
    public void Features_Export_Test()
    {
        var x = new FeatureStatesPersisted();
        x.Features.Add(new()
        {
            Id = new(),
            Enabled = true,
            Expires = DateTime.Today
        });

        x.Features.Add(new()
        {
            Id = new(),
            Enabled = false,
            Expires = DateTime.Today
        });

        x.Features.Add(new()
        {
            Id = new(),
            Enabled = false,
            Expires = DateTime.Today.AddDays(-1)
        });

        var ser = JsonSerializer.Serialize(x, JsonOptions.UnsafeJsonWithoutEncodingHtml);
        Trace.WriteLine(ser);
    }
}