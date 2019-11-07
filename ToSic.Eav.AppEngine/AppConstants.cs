namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";


        #region Template fields / constants

        public const string TemplateIcon = "Icon"; // temp public, needed in 2sxc for now...

        // todo: move to Sxc - Template_Constants
        // ATM still used in app, because there
        // is an import/export section that knows
        // about templates, which is not correct
        //public const string TemplateName = "Name";
        public const string TemplatePath = "Path";
        //public const string TemplateContentType = "ContentTypeStaticName";
        //public const string TemplateContentDemo = "ContentDemoEntity";
        //public const string TemplatePresentationType = "PresentationTypeStaticName";
        //public const string TemplatePresentationDemo = "PresentationDemoEntity";
        //public const string TemplateListContentType = "ListContentTypeStaticName";
        //public const string TemplateListContentDemo = "ListContentDemoEntity";
        //public const string TemplateListPresentationType = "ListPresentationTypeStaticName";
        //public const string TemplateListPresentationDemo = "ListPresentationDemoEntity";
        //public const string TemplateType = "Type";
        public const string TemplateIsHidden = "IsHidden";
        public const string TemplateLocation = "Location";
        public const string TemplateUseList = "UseForList";
        public const string TemplatePublishEnable = "PublishData";
        public const string TemplatePublishStreams = "StreamsToPublish";
        public const string TemplateViewName = "ViewNameInUrl";
        //public const string TemplateTypeRazor = "C# Razor";

        // end todo

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
