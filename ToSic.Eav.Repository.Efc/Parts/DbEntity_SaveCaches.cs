using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {
        private (int ContentTypeId, List<ToSicEavAttributes> Attributes) GetContentTypeAndAttribIds(IEntity newEnt/*, out List<ToSicEavAttributes> attributeDefinition*/, bool logDetails
        ) => Log.Func(l =>
        {
            var saveJson = UseJson(newEnt);
            var contentTypeId = saveJson
                ? RepoIdForJsonEntities
                // : newEnt.Type.Id; 
            
            // 2021-12-14 2dm disable this, because the comment suggest this is leftover code which does nothing
            // 2023-02-28 2dm - need it again, because new things have a 0 id
            // TODO: IF IT WORKS clean up the code and also remove the GetId as noted below
            : newEnt.Type.Id/*.ContentTypeId*/ > 0
                    ? newEnt.Type.Id //.ContentTypeId
                    : DbContext.AttribSet.GetId(newEnt.Type.NameId/*.StaticName*/); // todo: we probably always have the id, so we could drop this only use of GetId

            List<ToSicEavAttributes> attributes = null;
            if (!saveJson)
            {
                if (_ctCache.ContainsKey(contentTypeId)) attributes = _ctCache[contentTypeId];
                else
                {
                    attributes = DbContext.Attributes.GetAttributeDefinitions(contentTypeId).ToList();
                    _ctCache.Add(contentTypeId, attributes);
                }
            }

            if (logDetails)
            {
                l.A($"header checked type:{contentTypeId}, attribDefs⋮{attributes?.Count}");
                if (attributes != null)
                    l.A(l.Try(() => $"attribs: [{string.Join(",", attributes.Select(a => a.AttributeId + ":" + a.StaticName))}]"));
            }
            //attributeDefinition = attributes;
            return (contentTypeId, attributes);
        });

        private readonly Dictionary<int, List<ToSicEavAttributes>> _ctCache = new Dictionary<int, List<ToSicEavAttributes>>();
        private void FlushTypeAttributesCache() => _ctCache.Clear();
    }
}
