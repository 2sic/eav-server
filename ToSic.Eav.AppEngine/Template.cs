using System;
using System.Linq;
using System.Threading;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public class Template: HasLog
    {
        public readonly IEntity Entity;

        public Template(IEntity templateEntity, Log parentLog): base("App.Templt", parentLog)
        {
            Entity = templateEntity ?? throw new Exception("Template entity is null");
        }

        private string GetBestString(string key) => (string) Entity.GetBestValue(key);
        private IEntity GetBestRelationship(string key) => ((Data.EntityRelationship)Entity.Attributes[key][0]).FirstOrDefault();
        private bool GetBestBool(string key) => (bool)(Entity.GetBestValue(key) ?? false);


        public int TemplateId => Entity.EntityId;

        public string Name => (string) Entity.GetBestValue(AppConstants.TemplateName, new[] {Thread.CurrentThread.CurrentUICulture.Name});

        public string Path => GetBestString(AppConstants.TemplatePath);

        public string ContentTypeStaticName => GetBestString(AppConstants.TemplateContentType);

        public IEntity ContentDemoEntity => GetBestRelationship(AppConstants.TemplateContentDemo);

        public string PresentationTypeStaticName => GetBestString(AppConstants.TemplatePresentationType);

        public IEntity PresentationDemoEntity => GetBestRelationship(AppConstants.TemplatePresentationDemo);

        public string ListContentTypeStaticName => GetBestString(AppConstants.TemplateListContentType);

        public IEntity ListContentDemoEntity => GetBestRelationship(AppConstants.TemplateListContentDemo);

        public string ListPresentationTypeStaticName => GetBestString(AppConstants.TemplateListPresentationType);

        public IEntity ListPresentationDemoEntity => GetBestRelationship(AppConstants.TemplateListPresentationDemo);

        public string Type => GetBestString(AppConstants.TemplateType);
        public Guid Guid => Entity.EntityGuid;

        public string GetTypeStaticName(string groupPart)
        {
            switch (groupPart.ToLower())
            {
                case AppConstants.ContentLower:
                    return ContentTypeStaticName;
                case AppConstants.PresentationLower:
                    return PresentationTypeStaticName;
                case "listcontent":
                    return ListContentTypeStaticName;
                case "listpresentation":
                    return ListPresentationTypeStaticName;
                default:
                    throw new NotSupportedException("Unknown group part: " + groupPart);
            }
        }

        public bool IsHidden => GetBestBool(AppConstants.TemplateIsHidden);

        public string Location => GetBestString(AppConstants.TemplateLocation);
        public bool UseForList => GetBestBool(AppConstants.TemplateUseList);
        public bool PublishData => GetBestBool(AppConstants.TemplatePublishEnable);
        public string StreamsToPublish => GetBestString(AppConstants.TemplatePublishStreams);


        public IEntity Pipeline => ((Data.EntityRelationship)Entity.Attributes["Pipeline"][0]).FirstOrDefault();
        public string ViewNameInUrl => GetBestString(AppConstants.TemplateViewName);

        /// <summary>
        /// Returns true if the current template uses Razor
        /// </summary>
        public bool IsRazor => Type == AppConstants.TemplateTypeRazor;
    }
}