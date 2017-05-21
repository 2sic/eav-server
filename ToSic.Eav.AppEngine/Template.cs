using System;
using System.Linq;
using System.Threading;

namespace ToSic.Eav.Apps
{
    public class Template
    {
        // todo: move to central...
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
        internal const string TemplteIsHidden = "IsHidden";
        internal const string TemplateLocation = "Location";
        internal const string TemplateUseList = "UseForList";
        internal const string TemplatePublishEnable = "PublishData";
        internal const string TemplatePublishStreams = "StreamsToPublish";
        internal const string TemplateViewName = "ViewNameInUrl";
        internal const string TemplateTypeRazor = "C# Razor";

        private readonly IEntity _templateEntity;

        public Template(IEntity templateEntity)
        {
            _templateEntity = templateEntity ?? throw new Exception("Template entity is null");
        }

        private string GetBestString(string key) => (string) _templateEntity.GetBestValue(key);
        private IEntity GetBestRelationship(string key) => ((Data.EntityRelationship)_templateEntity.Attributes[key][0]).FirstOrDefault();
        private bool GetBestBool(string key) => (bool)(_templateEntity.GetBestValue(key) ?? false);


        public int TemplateId => _templateEntity.EntityId;

        public string Name => (string) _templateEntity.GetBestValue(TemplateName, new[] {Thread.CurrentThread.CurrentUICulture.Name});

        public string Path => GetBestString(TemplatePath);

        public string ContentTypeStaticName => GetBestString(TemplateContentType);

        public IEntity ContentDemoEntity => GetBestRelationship(TemplateContentDemo);

        public string PresentationTypeStaticName => GetBestString(TemplatePresentationType);

        public IEntity PresentationDemoEntity => GetBestRelationship(TemplatePresentationDemo);

        public string ListContentTypeStaticName => GetBestString(TemplateListContentType);

        public IEntity ListContentDemoEntity => GetBestRelationship(TemplateListContentDemo);

        public string ListPresentationTypeStaticName => GetBestString(TemplateListPresentationType);

        public IEntity ListPresentationDemoEntity => GetBestRelationship(TemplateListPresentationDemo);

        public string Type => GetBestString(TemplateType);
        public Guid Guid => _templateEntity.EntityGuid;

        public string GetTypeStaticName(string groupPart)
        {
            switch (groupPart.ToLower())
            {
                case Constants.ContentKeyLower:
                    return ContentTypeStaticName;
                case Constants.PresentationKeyLower:
                    return PresentationTypeStaticName;
                case "listcontent":
                    return ListContentTypeStaticName;
                case "listpresentation":
                    return ListPresentationTypeStaticName;
                default:
                    throw new NotSupportedException("Unknown group part: " + groupPart);
            }
        }

        public bool IsHidden => GetBestBool(TemplteIsHidden);

        public string Location => GetBestString(TemplateLocation);
        public bool UseForList => GetBestBool(TemplateUseList);
        public bool PublishData => GetBestBool(TemplatePublishEnable);
        public string StreamsToPublish => GetBestString(TemplatePublishStreams);


        public IEntity Pipeline => ((Data.EntityRelationship)_templateEntity.Attributes["Pipeline"][0]).FirstOrDefault();
        public string ViewNameInUrl => GetBestString(TemplateViewName);

        /// <summary>
        /// Returns true if the current template uses Razor
        /// </summary>
        public bool IsRazor => Type == TemplateTypeRazor;
    }
}