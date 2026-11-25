using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Sys.Logging;

namespace ToSic.Eav.Data.Build.ValueConverter;

public class ValueConverterExtensionsTests
{
    private const string OriginalLink = "/images/sample.png";
    private const string Reference = "file:42";

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void PreConvertReferences_PreservesConversionAndHonorsCallerLogging(bool enabled, bool details)
    {
        var converter = new FakeValueConverter();
        var importLog = new Log("Tst.Import");
        var settings = new LogSettings(Enabled: enabled, Details: details);

        var result = converter.PreConvertReferences(OriginalLink, ValueTypes.Hyperlink, true, importLog.IfDetails(settings));

        Equal(Reference, result);
        Equal(enabled && details, importLog.Entries.Count > 0);
        Empty(converter.Log.Entries);
    }

    [Fact]
    public void PreConvertReferences_PreservesConverterLogging_ExistingOverload()
    {
        var converter = new FakeValueConverter();

        var result = converter.PreConvertReferences(OriginalLink, ValueTypes.Hyperlink, true);

        Equal(Reference, result);
        NotEmpty(converter.Log.Entries);
    }

    [Fact]
    public void PreConvertReferences_LeavesConverterLoggerConnected_AfterQuietCall()
    {
        var converter = new FakeValueConverter();

        converter.PreConvertReferences(OriginalLink, ValueTypes.Hyperlink, true, log: null);
        Empty(converter.Log.Entries);
        converter.PreConvertReferences(OriginalLink, ValueTypes.Hyperlink, true);

        NotEmpty(converter.Log.Entries);
    }

    [Theory]
    [InlineData(ValueTypes.Hyperlink, false)]
    [InlineData(ValueTypes.String, true)]
    public void PreConvertReferences_PreservesValue_WhenConversionIsNotRequired(ValueTypes valueType, bool resolveHyperlink)
    {
        var converter = new FakeValueConverter();

        var result = converter.PreConvertReferences(OriginalLink, valueType, resolveHyperlink, log: null);

        Equal(OriginalLink, result);
    }

    private sealed class FakeValueConverter : IValueConverter, IHasLog
    {
        public Log Log { get; } = new("Tst.Conv");
        ILog IHasLog.Log => Log;

        public string ToReference(string value) => Reference;
        public string? ToValue(string? reference, Guid itemGuid = default) => reference;
    }
}
