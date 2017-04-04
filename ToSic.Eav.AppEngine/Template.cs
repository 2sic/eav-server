using System;
using System.Linq;
using System.Threading;

namespace ToSic.Eav.Apps
{
    public class Template
    {
	    private readonly IEntity _templateEntity;
	    public Template(IEntity templateEntity)
	    {
            _templateEntity = templateEntity ?? throw new Exception("Template entity is null");
	    }

		public int TemplateId => _templateEntity.EntityId;

        public string Name => (string)_templateEntity.GetBestValue("Name", new [] { Thread.CurrentThread.CurrentUICulture.Name});
        public string Path => (string) _templateEntity.GetBestValue("Path");

        public string ContentTypeStaticName => (string)_templateEntity.GetBestValue("ContentTypeStaticName");
        public IEntity ContentDemoEntity => ((Data.EntityRelationship)_templateEntity.Attributes["ContentDemoEntity"][0]).FirstOrDefault();
        public string PresentationTypeStaticName => (string)_templateEntity.GetBestValue("PresentationTypeStaticName");
        public IEntity PresentationDemoEntity => ((Data.EntityRelationship)_templateEntity.Attributes["PresentationDemoEntity"][0]).FirstOrDefault();
        public string ListContentTypeStaticName => (string)_templateEntity.GetBestValue("ListContentTypeStaticName");
        public IEntity ListContentDemoEntity => ((Data.EntityRelationship)_templateEntity.Attributes["ListContentDemoEntity"][0]).FirstOrDefault();
        public string ListPresentationTypeStaticName => (string)_templateEntity.GetBestValue("ListPresentationTypeStaticName");
        public IEntity ListPresentationDemoEntity => ((Data.EntityRelationship)_templateEntity.Attributes["ListPresentationDemoEntity"][0]).FirstOrDefault();
        public string Type => (string)_templateEntity.GetBestValue("Type");
        public Guid Guid => (Guid)_templateEntity.GetBestValue("EntityGuid");

        public string GetTypeStaticName(string groupPart)
        {
            switch(groupPart.ToLower())
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

	    public bool IsHidden => (bool)(_templateEntity.GetBestValue("IsHidden") ?? false);

        public string Location => (string)_templateEntity.GetBestValue("Location");
        public bool UseForList => (bool) _templateEntity.GetBestValue("UseForList");
        public bool PublishData => (bool)_templateEntity.GetBestValue("PublishData");
        public string StreamsToPublish => (string)_templateEntity.GetBestValue("StreamsToPublish");

        public IEntity Pipeline => ((Data.EntityRelationship)_templateEntity.Attributes["Pipeline"][0]).FirstOrDefault();
        public string ViewNameInUrl => (string)_templateEntity.GetBestValue("ViewNameInUrl");

        /// <summary>
        /// Returns true if the current template uses Razor
        /// </summary>
        public bool IsRazor => Type == "C# Razor" || Type == "VB Razor";
    }
}