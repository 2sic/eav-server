namespace ToSic.Eav.Apps.Decorators
{
    public static class AppStateDecoratorExtensions
    {
        public static bool IsGlobal(this AppState appState)
            => appState.Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
    }
}
