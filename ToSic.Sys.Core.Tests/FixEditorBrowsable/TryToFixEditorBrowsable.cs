namespace ToSic.Sys.Core.Tests.FixEditorBrowsable;

public class TryToFixEditorBrowsable
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public string? Something { get; set; }

    private void TestAccess()
    {
        var x = this.Something;
    }
}