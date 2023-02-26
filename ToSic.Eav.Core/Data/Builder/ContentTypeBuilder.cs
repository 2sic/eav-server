using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data.Builder
{
    public class ContentTypeBuilder
    {
        /// <summary>
        /// WIP - constructor shouldn't ever be called because of DI
        /// </summary>
        public ContentTypeBuilder() { }

        public const int DynTypeId = 1;
        // #RemoveContentTypeDescription #2974 - #remove ca. 2023 Q2 if all works
        //public const string DynTypeDefDescription = "Dynamic content type";

        public IContentType Create(int appId,
            string name,
            string nameId,
            int id,
            string scope,
            
            // Reference to original if it inherits something
            int? parentTypeId = default,
            int configZoneId = default,
            int configAppId = default,

            // This is a shared type
            bool isAlwaysShared = default,

            IList<IContentTypeAttribute> attributes = default,

            Func<IHasMetadataSource> metaSourceFinder = default,
            bool isDynamic = default)
        {
            // Prepare decorators - if it's inheriting, add that information
            var decorators = new List<IDecorator<IContentType>>();
            if (parentTypeId != null)
                decorators.Add(new Ancestor<IContentType>(new AppIdentity(configZoneId, configAppId),
                    parentTypeId.Value));

            // Prepare metadata retrieval
            var ctMetadata = new ContentTypeMetadata(nameId, metaSourceFinder, name);

            return new ContentType(
                appId: appId,
                name: name,
                nameId: nameId,
                id: id,
                scope: scope,
                decorators: decorators,
                ctMetadata: ctMetadata,
                //parentTypeId: parentTypeId,
                //configZoneId: configZoneId,
                //configAppId: configAppId,
                isAlwaysShared: isAlwaysShared,
                attributes: attributes,
                //metaSourceFinder: metaSourceFinder,
                isDynamic: isDynamic
            );
        }

        public IContentType Clone(
            IContentType original,
            int? appId = default,
            string name = default,
            string nameId = default,
            int? id = default,
            string scope = default,
            RepositoryTypes? repoType = default
        )
        {
            return Create(
                appId: appId ?? original.AppId,
                name: name ?? original.Name,
                nameId: nameId ?? original.NameId,
                id: id ?? original.Id,
                scope: scope ?? original.Scope
                );
        }

        public IContentType Transient(string typeName)
            => Transient(Constants.TransientAppId, typeName, typeName);

        public IContentType Transient(int appId, string typeName, string nameId, string scope = null)
            // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
            => Create(appId: appId, name: typeName, nameId: nameId, id: DynTypeId,
                scope: scope ?? Scopes.System /*, DynTypeDefDescription*/,
                attributes: new List<IContentTypeAttribute>(), isDynamic: true);

    }
}
