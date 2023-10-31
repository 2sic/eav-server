using System;

namespace ToSic.Eav.Apps.AppSys
{
    /// <summary>
    /// Context object for performing App modifications.
    /// This should help us change all the Read/Parts etc. to be fully functional and not depend on a Parent object. 
    /// </summary>
    public class AppWorkCtx : IAppWorkCtx
    {
        /// <inheritdoc />
        public int ZoneId { get; }

        /// <inheritdoc />
        public int AppId { get; }


        public AppWorkCtx(AppState appState)
        {
            AppId = appState.AppId;
            ZoneId = appState.ZoneId;
            AppState = appState;
        }

        public AppWorkCtx(IAppWorkCtx original, AppState appState = default)
        {
            if (original == null) throw new ArgumentException(@"Original must exist", nameof(original));
            AppId = appState?.AppId ?? original.AppId;
            ZoneId = appState?.ZoneId ?? original.ZoneId;
            AppState = appState ?? original.AppState;
        }


        public AppState AppState { get; }

    }
}
