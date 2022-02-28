using ToSic.Eav.Apps;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    public interface IContextOfApp: IContextOfSite
    {
        /// <summary>
        /// The App State which the current context has
        /// </summary>
        AppState AppState { get; }

        /// <summary>
        /// Reset call to change what AppState is in the context.
        /// Internal API to get the context ready
        /// </summary>
        void ResetApp(IAppIdentity appIdentity);

        /// <summary>
        /// Reset call to change what AppState is in the context
        /// Internal API to get the context ready
        /// </summary>
        void ResetApp(int appId);
    }
}
