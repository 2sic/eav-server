using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Run.Startup;
using ToSic.Eav.WebApi.Sys.Context;
using ToSic.Sys.DI;

namespace ToSic.Eav.WebApi.Tests.Context;

public class UiContextBuilderTests
{
    [Fact]
    public void Get_OmitsApp_WhenNoAppIsSelected()
    {
        var services = new ServiceCollection();
        services.AddEavAll().AddEavAllFallbacks();
        using var provider = services.BuildServiceProvider();
        var builder = provider.GetRequiredService<Generator<IUiContextBuilder, IAppReader?>>()
            .New(null);

        var context = builder.Get(Ctx.AppBasic, CtxEnable.None);

        Null(context.App);
    }
}
