namespace ToSic.Eav.Apps.Decorators
{
    public static class AppStateDecoratorExtensions
    {
        public static bool IsShared(this AppState appState)
            => appState.Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
    }
}
