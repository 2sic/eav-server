using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Tests.Context;

public class ContextOfAppTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToString_DoesNotReadRequiredOrLazyProperties_WithoutApp(bool derived)
    {
        // Unused dependencies stay unset so diagnostic formatting cannot initialize services.
        ContextOfApp context = derived
            ? new DerivedContext { InternalMachinery = null!, SettingsStackSvc = null! }
            : new ContextOfApp(null!, null!, null) { InternalMachinery = null!, SettingsStackSvc = null! };

        var text = context.ToString();

        Equal($"{context.GetType().Name} {{ HasApp = False }}", text);
    }

    [Fact]
    public void AppReaderRequired_Throws_WithoutApp()
    {
        var context = new ContextOfApp(null!, null!, null) { InternalMachinery = null!, SettingsStackSvc = null! };

        Throws<NullReferenceException>(() => context.AppReaderRequired);
    }

    private sealed record DerivedContext() : ContextOfApp(null!, null!, null)
    {
        public object LazyValue => throw new InvalidOperationException("Diagnostic formatting must not read this property.");
    }
}
