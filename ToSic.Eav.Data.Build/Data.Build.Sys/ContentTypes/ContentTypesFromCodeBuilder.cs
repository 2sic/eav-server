using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Special system to manage and to convert c# classes with their definitions/attributes into content types.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypesFromCodeBuilder(ContentTypesFromCodeBuilder.Dependencies services)
    : ServiceBase<ContentTypesFromCodeBuilder.Dependencies>(services, "Eav.CtFact")
{
    public record Dependencies(ContentTypeAssemblyKit TypeAssemblyKit, EntityAssembler EntityAssembler, AttributeListAssembler AttrListAssembler, IDataFactory DataFactory)
        : DependenciesRecord(connect: [TypeAssemblyKit, EntityAssembler, AttrListAssembler, DataFactory]);

    // TODO: Should probably be something different...?
    public const int NoAppId = -1;
    public const string AnonymousTypeName = "AnonymousType";

    internal IContentType Generate(Type type, string? name = default, string? nameId = default, string? scope = default, int appId = NoAppId)
    {
        var l = Log.Fn<IContentType>(timer: true);

        // 1. Get the content type specs from the attribute (if any) and process
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeAttribute>();
        var ctName = name
                     ?? ctSpecs?.Name.NullIfNoValue()
                     ?? (type.IsAnonymous() ? AnonymousTypeName : type.Name);
        var ctNameId = nameId
                       ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null)
                       ?? Guid.Empty.ToString();
        var ctScope = scope
                      ?? ctSpecs?.Scope.NullIfNoValue()
                      ?? ScopeConstants.Default;

        // 2. Create Description-Metadata for the Content-Type based on the info provided; null if no description provided
        var ctMdItems = ctSpecs?.Description
            .NullOrGetWith(desc =>
            {
                // Edge case: If the type is the same as the one we're about to create, result in a stack overflow
                if (type == typeof(ContentTypeDetails))
                    return null;

                var settings = new ContentTypeDetails { Description = desc };
                var entity = Services.DataFactory.Create(settings);
                return new List<IEntity> { entity };
            });

        var ctMdSource = MetadataProvider.Create(ctMdItems);
        var ctMetadata = new ContentTypeMetadata(typeId: ctNameId, title: ctName, source: ctMdSource);

        // 3. Create Attribute Information for the ContentType
        var attributeHelper = new ContentTypesFromCodeAttributeHelper(Services, Log);
        var (attributes, builtInAttributeDecorator) = attributeHelper.Process(type);

        // 4. Create the ContentType
        var contentType = Services.TypeAssemblyKit.Type.Create(
            appId,
            name: ctName,
            nameId: ctNameId,
            scope: ctScope,
            id: 0,
            metadata: ctMetadata,
            isDynamic: false, // set dynamic to false, as the attributes are known, this type can only have these attributes
            attributes: attributes,
            decorators: builtInAttributeDecorator.ToListOfOneOrNull(),
            repositoryType: ctSpecs == null
                ? RepositoryTypes.CodeReflection
                : RepositoryTypes.CodeConfiguration
        );
        return l.ReturnAndLog(contentType);
    }

}