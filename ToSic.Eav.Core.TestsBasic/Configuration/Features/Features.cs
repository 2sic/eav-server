using System.Text.Json;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Sys.Capabilities.Features;
using Xunit.Abstractions;

namespace ToSic.Eav.Configuration.Features;

public class Features(ITestOutputHelper output)
{
    [Fact]
    public void Features_Export_Test()
    {
        var x = new FeatureStatesPersisted();
        x.Features.Add(new()
        {
            Id = Guid.Empty,
            Enabled = true,
            Expires = DateTime.Today
        });

        x.Features.Add(new()
        {
            Id = Guid.Empty,
            Enabled = false,
            Expires = DateTime.Today
        });

        x.Features.Add(new()
        {
            Id = Guid.Empty,
            Enabled = false,
            Expires = DateTime.Today.AddDays(-1)
        });

        var ser = JsonSerializer.Serialize(x, JsonOptions.UnsafeJsonWithoutEncodingHtml);
        output.WriteLine(ser);
    }
}