namespace ToSic.Testing.Shared.Platforms;

public class TestPlatformNotPatron : TestPlatformPatronPerfectionist
{
    public override string Name => "Test-Not-Patron";

    // Different guid, so it's not a valid license
    public override string Identity => "002776da-5760-403f-9c64-a5609b40c7f0";

}