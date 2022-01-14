﻿using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    public static class AppStateExtensions
    {
        public static bool IsGlobal(this AppState appState)
            => appState.Metadata.HasType(Decorators.IsGlobalDecoratorId);

        public static bool IsInherited(this AppState appState)
            => appState.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited
    }
}