namespace ToSic.Eav.Context;

public static class ContextOfServiceExtensions
{
    extension(IContextOfAppService ctx)
    {
        public IContextOfApp AppContextRequired(string? errorMessage = null) =>
            ctx.AppContextOrNull ?? throw new InvalidOperationException(errorMessage ?? "App context is required but not available.");
    }
}
