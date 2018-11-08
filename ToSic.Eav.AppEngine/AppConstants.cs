namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";


        #region Template fields / constants

        public const string TemplateIcon = "Icon"; // temp public, needed in 2sxc for now...
        internal const string TemplateName = "Name";
        internal const string TemplatePath = "Path";
        internal const string TemplateContentType = "ContentTypeStaticName";
        internal const string TemplateContentDemo = "ContentDemoEntity";
        internal const string TemplatePresentationType = "PresentationTypeStaticName";
        internal const string TemplatePresentationDemo = "PresentationDemoEntity";
        internal const string TemplateListContentType = "ListContentTypeStaticName";
        internal const string TemplateListContentDemo = "ListContentDemoEntity";
        internal const string TemplateListPresentationType = "ListPresentationTypeStaticName";
        internal const string TemplateListPresentationDemo = "ListPresentationDemoEntity";
        internal const string TemplateType = "Type";
        internal const string TemplateIsHidden = "IsHidden";
        internal const string TemplateLocation = "Location";
        internal const string TemplateUseList = "UseForList";
        internal const string TemplatePublishEnable = "PublishData";
        internal const string TemplatePublishStreams = "StreamsToPublish";
        internal const string TemplateViewName = "ViewNameInUrl";
        internal const string TemplateTypeRazor = "C# Razor";

        #endregion

        #region List / Content / Presentation capabilities
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
