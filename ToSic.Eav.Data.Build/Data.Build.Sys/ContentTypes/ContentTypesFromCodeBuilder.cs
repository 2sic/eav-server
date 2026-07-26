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
    public record Dependencies(ContentTypeAssemblyKit CtAssemblyKit, EntityAssembler EntityAssembler, AttributeListAssembler AttributeListAssembler)
        : DependenciesRecord(connect: [CtAssemblyKit, EntityAssembler, AttributeListAssembler]);

    // TODO: Should probably be something different...?
    public const int NoAppId = -1;
    public const string AnonymousTypeName = "AnonymousType";

    internal IContentType Generate(Type type, string? name = default, string? nameId = default, string? scope = default, int appId = NoAppId)
    {
        var l = Log.Fn<IContentType>(timer: true);

        // 1. Get the content type specs from the attribute, if any and process
        var ctSpecs = type.GetDirectlyAttachedAttribute<ContentTypeSpecsAttribute>();
        var ctName = name
                     ?? ctSpecs?.Name.NullIfNoValue()
                     ?? (type.IsAnonymous() ? AnonymousTypeName : type.Name);
        var ctNameId = nameId
                       ?? ctSpecs?.Guid.NullOrGetWith(g => Guid.TryParse(g, out var guid) ? guid.ToString() : null)
                       ?? Guid.Empty.ToString();
        var ctScope = scope
                      ?? ctSpecs?.Scope.NullIfNoValue()
                      ?? ScopeConstants.Default;

        // 2. Create Metadata for the Content-Type based on the info provided
        // Must be null if no metadata, so that it would then assume empty list...?
        var ctMdItems = CreateCtDetailsMetadataEntity(ctSpecs?.Description).ToListOfOneOrNull();
        var ctMdSource = MetadataProvider.Create(ctMdItems);
        var ctMetadata = new ContentTypeMetadata(typeId: ctNameId, title: ctName, source: ctMdSource);

        // 3. Create Attribute Information for the ContentType
        var attributeHelper = new ContentTypesFromCodeAttributeHelper(Services, Log);
        var (attributes, builtInAttributeDecorator) = attributeHelper.Process(type);

        var contentType = Services.CtAssemblyKit.Type.Create(
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

    /// <summary>
    /// Generate a details entity for a content type.
    /// Most properties like icon etc. are not important, so ATM it only does:
    /// - Description
    /// </summary>
    private IEntity? CreateCtDetailsMetadataEntity(string? description)
    {
        var l = Log.Fn<IEntity>();
        if (description == null)
            return l.ReturnNull("no description");

        // All props
        var dic = new Dictionary<string, object?>
        {
            { nameof(ContentTypeDetails.Description), description }
        };
        var attributes = Services.AttributeListAssembler.Finalize(dic);

        // Create a Description entity
        var entity = Services.EntityAssembler.Create(
            NoAppId,
            Services.CtAssemblyKit.Type.Transient(NoAppId, ContentTypeDetails.ContentTypeName),
            attributes: attributes
        );
        return l.Return(entity, "created");
    }

}