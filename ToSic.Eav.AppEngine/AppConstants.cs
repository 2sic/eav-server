﻿namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";


        #region List / Content / Presentation capabilities
        // todo: move to 2sxc
        public const string Content = "Content";
        public const string ContentLower = "content";

        public const string Presentation = "Presentation";
        public const string PresentationLower = "presentation";


        public const string ListContent = "ListContent";
        public const string ListContentLower = "listcontent";

        public const string ListPresentation = "ListPresentation";
        public const string ListPresentationLower = "listpresentation";



        // Known App Content Types
        public const string TypeAppConfig = "2SexyContent-App";
        public const string TypeAppResources = "App-Resources";
        public const string TypeAppSettings = "App-Settings";

        // App and Content Scopes
        public const string ScopeApp = "2SexyContent-App";
        public const string ScopeContentFuture = "Default";
        public const string ScopeContentOld = "2SexyContent";
        public static readonly string[] ScopesContent = {ScopeContentOld, ScopeContentFuture };

        public const string ScopeContentSystem = "2SexyContent-System";
        public static readonly string[] ScopesSystem = { Constants.ScopeSystem, ScopeApp, ScopeContentSystem };

        #endregion
    }
}
