﻿namespace ToSic.Eav.Apps
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

        public const string ListContent = "ListContent";
        public const string ListPresentation = "ListPresentation";
        #endregion
    }
}
