﻿using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Persistence.Logging;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {

        private void ImportXmlTemplates(XElement root)
        {
            Log.Add("import xml templates");
            var templates = root.Element(XmlConstants.Templates);
            if (templates == null) return;

            var cache = DataSource.GetCache(ZoneId, AppId);

            foreach (var template in templates.Elements(XmlConstants.Template))
            {
                var name = "";
                try
                {
                    name = template.Attribute(XmlConstants.Name).Value;
                    var path = template.Attribute(AppConstants.TemplatePath).Value;

                    var contentTypeStaticName = template.Attribute(XmlConstants.AttSetStatic).Value;

                    Log.Add($"template:{name}, type:{contentTypeStaticName}, path:{path}");

                    if (!String.IsNullOrEmpty(contentTypeStaticName) && cache.GetContentType(contentTypeStaticName) == null)
                    {
                        Messages.Add(new Message($"Content Type for Template \'{name}\' could not be found. The template has not been imported.",
                                Message.MessageTypes.Warning));
                        continue;
                    }

                    var demoEntityGuid = template.Attribute(XmlConstants.TemplateDemoItemGuid).Value;
                    var demoEntityId = new int?();

                    if (!String.IsNullOrEmpty(demoEntityGuid))
                    {
                        var entityGuid = Guid.Parse(demoEntityGuid);
                        if (_eavContext.Entities.EntityExists(entityGuid))
                            demoEntityId = _eavContext.Entities.GetMostCurrentDbEntity(entityGuid).EntityId;
                        else
                            Messages.Add(new Message($"Demo Entity for Template \'{name}\' could not be found. (Guid: {demoEntityGuid})", Message.MessageTypes.Information));

                    }

                    var type = template.Attribute(XmlConstants.EntityTypeAttribute).Value;
                    var isHidden = Boolean.Parse(template.Attribute(AppConstants.TemplateIsHidden).Value);
                    var location = template.Attribute(AppConstants.TemplateLocation).Value;
                    var publishData =
                        Boolean.Parse(template.Attribute(AppConstants.TemplatePublishEnable) == null
                            ? "False"
                            : template.Attribute(AppConstants.TemplatePublishEnable).Value);
                    var streamsToPublish = template.Attribute(AppConstants.TemplatePublishStreams) == null
                        ? ""
                        : template.Attribute(AppConstants.TemplatePublishStreams).Value;
                    var viewNameInUrl = template.Attribute(AppConstants.TemplateViewName) == null
                        ? null
                        : template.Attribute(AppConstants.TemplateViewName).Value;

                    var queryEntityGuid = template.Attribute(XmlConstants.TemplateQueryGuidField);
                    var queryEntityId = new int?();

                    if (!String.IsNullOrEmpty(queryEntityGuid?.Value))
                    {
                        var entityGuid = Guid.Parse(queryEntityGuid.Value);
                        if (_eavContext.Entities.EntityExists(entityGuid))
                            queryEntityId = _eavContext.Entities.GetMostCurrentDbEntity(entityGuid).EntityId;
                        else
                            Messages.Add(new Message($"Query Entity for Template \'{name}\' could not be found. (Guid: {queryEntityGuid.Value})", Message.MessageTypes.Information));
                    }

                    var useForList = false;
                    if (template.Attribute(AppConstants.TemplateUseList) != null)
                        useForList = Boolean.Parse(template.Attribute(AppConstants.TemplateUseList).Value);

                    var lstTemplateDefaults = template.Elements(XmlConstants.Entity).Select(e =>
                    {
                        var xmlItemType =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateItemType)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlContentTypeStaticName =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateContentTypeId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlDemoEntityGuidString =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateDemoItemId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        if (xmlItemType == null || xmlContentTypeStaticName == null || xmlDemoEntityGuidString == null)
                        {
                            Messages.Add(new Message(
                                $"trouble with template '{name}' - either type, static or guid are null",
                                Message.MessageTypes.Error));
                            return null;
                        }
                        var xmlDemoEntityId = new int?();
                        if (xmlDemoEntityGuidString != "0" && xmlDemoEntityGuidString != "")
                        {
                            var xmlDemoEntityGuid = Guid.Parse(xmlDemoEntityGuidString);
                            if (_eavContext.Entities.EntityExists(xmlDemoEntityGuid))
                                xmlDemoEntityId = _eavContext.Entities.GetMostCurrentDbEntity(xmlDemoEntityGuid)
                                    .EntityId;
                        }

                        return new TemplateDefault
                        {
                            ItemType = xmlItemType,
                            ContentTypeStaticName =
                                xmlContentTypeStaticName == "0" || xmlContentTypeStaticName == ""
                                    ? ""
                                    : xmlContentTypeStaticName,
                            DemoEntityId = xmlDemoEntityId
                        };
                    }).ToList();

                    // note: Array lstTemplateDefaults has null objects, so remove null objects
                    var templateDefaults = lstTemplateDefaults.Where(lstItem => lstItem != null).ToList();

                    var presentationTypeStaticName = "";
                    var presentationDemoEntityId = new int?();
                    //if list templateDefaults would have null objects, we would have an exception
                    var presentationDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.Presentation);
                    if (presentationDefault != null)
                    {
                        presentationTypeStaticName = presentationDefault.ContentTypeStaticName;
                        presentationDemoEntityId = presentationDefault.DemoEntityId;
                    }

                    var listContentTypeStaticName = "";
                    var listContentDemoEntityId = new int?();
                    var listContentDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.ListContent);
                    if (listContentDefault != null)
                    {
                        listContentTypeStaticName = listContentDefault.ContentTypeStaticName;
                        listContentDemoEntityId = listContentDefault.DemoEntityId;
                    }

                    var listPresentationTypeStaticName = "";
                    var listPresentationDemoEntityId = new int?();
                    var listPresentationDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.ListPresentation);
                    if (listPresentationDefault != null)
                    {
                        listPresentationTypeStaticName = listPresentationDefault.ContentTypeStaticName;
                        listPresentationDemoEntityId = listPresentationDefault.DemoEntityId;
                    }

                    new AppManager(_eavContext.ZoneId, _eavContext.AppId, Log).Templates.CreateOrUpdate(
                        null, name, path, contentTypeStaticName, demoEntityId, presentationTypeStaticName,
                        presentationDemoEntityId, listContentTypeStaticName, listContentDemoEntityId,
                        listPresentationTypeStaticName, listPresentationDemoEntityId, type, isHidden, location,
                        useForList, publishData, streamsToPublish, queryEntityId, viewNameInUrl);

                    Messages.Add(new Message($"Template \'{name}\' successfully imported.",
                        Message.MessageTypes.Information));
                }

                catch (Exception)
                {
                    Messages.Add(new Message($"Import for template \'{name}\' failed.",
                        Message.MessageTypes.Information));
                }

            }
            Log.Add("import xml templates - completed");
        }

	}

    internal class TemplateDefault
    {

        public string ItemType;
        public string ContentTypeStaticName;
        public int? DemoEntityId;

    }
}