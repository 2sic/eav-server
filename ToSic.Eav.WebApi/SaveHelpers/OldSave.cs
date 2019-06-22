using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.SaveHelpers
{
    internal class OldSave: HasLog
    {
        public OldSave(Log parentLog) : base("Eav.OldSav", parentLog) { }


        public Entity CreateEntityFromTransferObject(AppManager appMan, BundleWithHeader<EntityWithLanguages> editInfo)
        {
            var allEntitiesForRelationships = appMan.Package;

            Log.Add($"CreateEntityFromTransferObject(editInfo:{editInfo.Header.ContentTypeName}:{editInfo.Header.Guid}, allEntitiesForRelationships:{allEntitiesForRelationships?.List?.Count()})");
            var toEntity = editInfo.Entity;

            // Attributes
            var attribs = new Dictionary<string, IAttribute>();

            // only transfer the fields / values which exist in the content-type definition
            var type = appMan.Read.ContentTypes.Get(toEntity.Type.StaticName);
            foreach (var attribute in toEntity.Attributes)
            {
                var attDef = type[attribute.Key];
                var attributeType = attDef.Type;

                // don't save anything of the type empty - this is headings-items-only
                if (attributeType == AttributeTypeEnum.Empty.ToString())
                    continue;

                foreach (var value in attribute.Value.Values)
                {
                    var objValue = value.Value;

                    // special situation: if it's an array of Guids, mixed with NULLs, then it's not correctly auto-de-serialized
                    if (attributeType == AttributeTypeEnum.Entity.ToString() && objValue is Newtonsoft.Json.Linq.JArray)
                        objValue = JsonConvert.DeserializeObject<Guid?[]>(objValue.ToString());// manually de-serialize

                    var importValue = attribs.AddValue(attribute.Key, objValue, attributeType,
                        allEntitiesForRelationships: allEntitiesForRelationships);

                    // append languages OR empty language as fallback
                    if (value.Dimensions != null)
                        foreach (var dimension in value.Dimensions)
                            importValue.Languages.Add(new Dimension { Key = dimension.Key, ReadOnly = dimension.Value });
                }
            }


            var importEntity = new Entity(appMan.AppId, toEntity.Id, type, attribs.ToDictionary(x => x.Key, y => (object)y.Value))
            {
                // 2018-09-06 2dm moved this to later processing, full save should still work
                // 2018-09-26 2dm re-enabled this, because the attribute names are different, we must transfer it here
                IsPublished = toEntity.IsPublished,
                PlaceDraftInBranch = toEntity.IsBranch, // if it's not a branch, it should also force no branch...
            };
            // 2018-09-06 2dm moved this to later processing, full save should still work
            //if (toEntity.Guid == Guid.Empty)
            //    throw new Exception("got empty guid - should never happen");
            //importEntity.SetGuid(toEntity.Guid);

            Log.Add($"Import Entity app:{importEntity.AppId} id:{importEntity.EntityId}, "
                    + $"guid:{importEntity.EntityGuid}, "
                    + $"type:{importEntity.Type.Name}, published:{importEntity.IsPublished}, "
                    + $"DraftInBranch:{importEntity.PlaceDraftInBranch}");


            #region Metadata if we have any
            var newMd = editInfo.Header.Metadata;
            if (newMd != null && newMd.HasMetadata)
            {
                Log.Add($"will set metadata based on input type:{newMd.TargetType} guid:{newMd.KeyGuid} #:{newMd.KeyNumber} $:{newMd.KeyString}");
                importEntity.SetMetadata(new MetadataFor
                {
                    TargetType = newMd.TargetType,
                    KeyGuid = newMd.KeyGuid,
                    KeyNumber = newMd.KeyNumber,
                    KeyString = newMd.KeyString
                });
            }
            else
            {
                // must re-attach metadata-info from previous generation of item...
                // this is important because the json must know about this for full serialization
                var original = appMan.Read.Entities.Get(editInfo.Header.Guid); // get again, using guid!
                if (original != null)
                {
                    Log.Add("found original - will use to re-attach metadata");
                    importEntity.SetMetadata(new MetadataFor(original.MetadataFor));
                }
                else
                    Log.Add("no original found, will not attach metadata");
            }
            #endregion

            return importEntity;
        }

    }
}
