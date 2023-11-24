using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data.Build;

public class ContentTypeBuilder
{
    /// <summary>
    /// WIP - constructor shouldn't ever be called because of DI
    /// </summary>
    public ContentTypeBuilder() { }

    public const int DynTypeId = 1;

    public IContentType Create(
        // Basic identifiers (5)
        int appId,
        string name,
        string nameId,
        int id,
        string scope,

        // Contents (1)
        IList<IContentTypeAttribute> attributes = default,

        // Basic specs (2)
        bool isAlwaysShared = default,  // This is a shared type
        bool isDynamic = default,

        // How it's saved / where it's from incl. inheritance (5)
        int? parentTypeId = default,
        int configZoneId = default,
        int configAppId = default,
        RepositoryTypes repositoryType = RepositoryTypes.Sql,
        string repositoryAddress = default,


        // Metadata (2)
        ContentTypeMetadata metadata = default,                 // for clone
        List<IEntity> metadataItems = default,
        Func<IHasMetadataSource> metaSourceFinder = default,    // for find-it-yourself

        // Save Specs (2) Older stuff, should be removed some day
        bool? onSaveSortAttributes = default,
        string onSaveUseParentStaticName = default
    )
    {
        // Prepare decorators - if it's inheriting, add that information
        var decorators = new List<IDecorator<IContentType>>();
        if (parentTypeId != null)
            decorators.Add(new Ancestor<IContentType>(new AppIdentity(configZoneId, configAppId),
                parentTypeId.Value));

        // Prepare metadata retrieval
        metadata = metadata ?? new ContentTypeMetadata(typeId: nameId, items: metadataItems, deferredSource: metaSourceFinder, title: name);

        attributes = attributes ?? new List<IContentTypeAttribute>();

        scope = Scopes.RenameOldScope(scope);

        return new ContentType(
            appId: appId,
            name: name,
            nameId: nameId,
            id: id,
            scope: scope,
            decorators: decorators.ToImmutableList(),
            ctMetadata: metadata,
            isAlwaysShared: isAlwaysShared,
            attributes: attributes.ToImmutableList(),
            isDynamic: isDynamic,
            repositoryType: repositoryType,
            repositoryAddress: repositoryAddress,
            // older stuff
            onSaveSortAttributes: onSaveSortAttributes,
            onSaveUseParentStaticName: onSaveUseParentStaticName
        );
    }

    public IContentType CreateFrom(
        IContentType original,
        // Basic identifiers (5)
        int? appId = default,
        string name = default,
        string nameId = default,
        int? id = default,
        string scope = default,
        // Contents (1)
        IList<IContentTypeAttribute> attributes = default,

        // Basic specs (2)
        bool? isAlwaysShared = default,  // This is a shared type
        bool? isDynamic = default,

        // Where it's from
        RepositoryTypes? repoType = default,
        string repoAddress = default,
        int? parentTypeId = default,
        int? configZoneId = default,
        int? configAppId = default,

        // Metadata (2)
        ContentTypeMetadata metadata = default,                 // for clone
        List<IEntity> metadataItems = default
        //Func<IHasMetadataSource> metaSourceFinder = default

        // Save Specs (2) Older stuff, should be removed some day - ATM not supported
        //bool? onSaveSortAttributes = default,
        //string onSaveUseParentStaticName = default

    )
    {
        if (original == null) throw new ArgumentNullException(nameof(original));

        // If we are reconfiguring where it's from / shared, we must retrieve / update that
        if (parentTypeId != null)
        {
            var ancestorDecorator = original.GetDecorator<IAncestor>();
            if (ancestorDecorator != null)
            {
                configZoneId = configZoneId ?? ancestorDecorator.ZoneId;
                configAppId = configAppId ?? ancestorDecorator.AppId;
            }
        }

        metadata = metadata ?? (metadataItems != default
            ? new ContentTypeMetadata(original.NameId, metadataItems, null, original.Name)
            : original.Metadata);

        return Create(
            // Identifiers (5)
            appId: appId ?? original.AppId,
            name: name ?? original.Name,
            nameId: nameId ?? original.NameId,
            id: id ?? original.Id,
            scope: scope ?? original.Scope,

            // Contents
            attributes: attributes ?? original.Attributes.ToList(),

            // Specs (2)
            isAlwaysShared: isAlwaysShared ?? original.AlwaysShareConfiguration,
            isDynamic: isDynamic ?? original.IsDynamic,

            // Where it's from
            repositoryType: repoType ?? original.RepositoryType,
            repositoryAddress: repoAddress ?? original.RepositoryAddress,
            parentTypeId: parentTypeId,
            configZoneId: configZoneId ?? default,
            configAppId: configAppId ?? default,

            // Metadata
            metadata: metadata
            //metaSourceFinder: metaSourceFinder ?? original.Metadata.SourceForClone

            // Save Specs not implemented
            // onSaveSortAttributes: onSaveSortAttributes ?? original
        );
    }

    public IContentType Transient(string typeName)
        => Transient(Constants.TransientAppId, typeName, typeName);

    public IContentType Transient(int appId, string typeName, string nameId, string scope = null)
        => Create(appId: appId, name: typeName, nameId: nameId, id: DynTypeId,
            scope: scope ?? Scopes.System,
            attributes: new List<IContentTypeAttribute>(), isDynamic: true);

}