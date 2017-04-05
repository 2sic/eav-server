using System.Collections.Generic;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Templates manager for the app engine - in charge of importing / modifying templates at app-level
    /// </summary>
    public class TemplatesManager: BaseManager
    {
        public TemplatesManager(AppManager app) : base(app) {}

        #region Template

        /// <summary>
        /// Adds or updates a template - will create a new template if templateId is not specified
        /// </summary>
        public void CreateOrUpdate(int? templateId, string name, string path, string contentTypeStaticName,
            int? contentDemoEntity, string presentationTypeStaticName, int? presentationDemoEntity,
            string listContentTypeStaticName, int? listContentDemoEntity, string listPresentationTypeStaticName,
            int? listPresentationDemoEntity, string templateType, bool isHidden, string location, bool useForList,
            bool publishData, string streamsToPublish, int? pipelineEntity, string viewNameInUrl)
        {
            var values = new Dictionary<string, object>
            {
                { "Name", name },
                { "Path", path },
                { "ContentTypeStaticName", contentTypeStaticName },
                { "ContentDemoEntity", contentDemoEntity.HasValue ? new[] { contentDemoEntity.Value } : new int[]{} },
                { "PresentationTypeStaticName", presentationTypeStaticName },
                { "PresentationDemoEntity", presentationDemoEntity.HasValue ? new[] { presentationDemoEntity.Value } : new int[]{} },
                { "ListContentTypeStaticName", listContentTypeStaticName },
                { "ListContentDemoEntity", listContentDemoEntity.HasValue ? new[] { listContentDemoEntity.Value } : new int[]{} },
                { "ListPresentationTypeStaticName", listPresentationTypeStaticName },
                { "ListPresentationDemoEntity", listPresentationDemoEntity.HasValue ? new[] { listPresentationDemoEntity.Value } : new int[]{} },
                { "Type", templateType },
                { "IsHidden", isHidden },
                { "Location", location },
                { "UseForList", useForList },
                { "PublishData", publishData },
                { "StreamsToPublish", streamsToPublish },
                { "Pipeline", pipelineEntity.HasValue ? new[] { pipelineEntity } : new int?[]{} },
                { "ViewNameInUrl", viewNameInUrl }
            };

            if (templateId.HasValue)
                _appManager.Entities.Update(templateId.Value, values);
            else
                _appManager.Entities.Create(Configuration.TemplateContentType, values);
        }



        #endregion

    }
}
