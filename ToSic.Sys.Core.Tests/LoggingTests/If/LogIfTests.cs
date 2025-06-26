namespace ToSic.Lib.Core.Tests.LoggingTests.If;

public class LogIfTests
{
    [Fact]
    public void LogIfTrue()
        => NotNull(new Log("").If(true));

    [Fact]
    public void LogIfFalse()
        => Null(new Log("").If(false));


    [Fact]
    public void LogIfSettingEnabled()
        => NotNull(new Log("").If(new LogSettings(Enabled: true)));

    [Fact]
    public void LogIfSettingEnabledFalse()
        => Null(new Log("").If(new LogSettings(Enabled: false)));

    [Fact]
    public void LogIfDetailsEnabled()
        => NotNull(new Log("").IfDetails(new(Enabled: true, Details: true)));

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void LogIfDetailsFalse(bool enabled, bool details)
        => Null(new Log("").IfDetails(new(Enabled: enabled, Details: details)));

    [Fact]
    public void LogIfSummaryEnabled()
        => NotNull(new Log("").IfSummary(new(Enabled: true, Summary: true)));

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void LogIfSummaryFalse(bool enabled, bool stats)
        => Null(new Log("").IfSummary(new(Enabled: enabled, Summary: stats)));

}