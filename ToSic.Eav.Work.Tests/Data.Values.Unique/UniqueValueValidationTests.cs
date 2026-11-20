using ToSic.Eav.Apps;
using ToSic.Eav.Data.Values.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.Services;
using Xunit.DependencyInjection;

namespace ToSic.Eav.Data.Values.Unique;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class UniqueValueValidationTests(
    DataAssembler dataAssembler,
    ContentTypeAssemblyKit ctAssemblyKit,
    IDataSourcesService dataSourcesService,
    DataSourceBase.Dependencies dataSourceDependencies)
{
    #region Unique Validation

    [Fact]
    public void DuplicateExistingValue_ReturnsDuplicateConflict()
    {
        using var ctx = CreateContext();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String, isUnique: true));

        var existing = ctx.CreateEntity(type, Guid.NewGuid(),
            ctx.InvariantAttribute("Title", ValueTypes.String, "Existing"),
            ctx.InvariantAttribute("Slug", ValueTypes.String, "same-slug"));

        var result = Validate(ctx, [type], [existing], new(type.NameId, "Slug", "same-slug"));

        False(result.IsValid);
        Equal("duplicate", result.Reason);
        Equal(existing.EntityId, result.ConflictEntityId);
        Equal(existing.GetBestTitle(), result.ConflictTitle);
    }

    [Fact]
    public void CurrentEntityWithSameValue_ReturnsOk()
    {
        using var ctx = CreateContext();
        var entityGuid = Guid.NewGuid();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String, isUnique: true));

        var current = ctx.CreateEntity(type, entityGuid,
            ctx.InvariantAttribute("Title", ValueTypes.String, "Current"),
            ctx.InvariantAttribute("Slug", ValueTypes.String, "same-slug"));

        var result = Validate(ctx, [type], [current], new(type.NameId, "Slug", "same-slug", CurrentEntity: current));

        True(result.IsValid);
        Equal("ok", result.Reason);
    }

    [Fact]
    public void BlankValue_ReturnsBlank()
    {
        using var ctx = CreateContext();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String, isUnique: true));

        var result = Validate(ctx, [type], [], new(type.NameId, "Slug", "   "));

        True(result.IsValid);
        Equal("blank", result.Reason);
    }

    [Theory]
    [InlineData("2026-05-20T00:00:00.000Z")]
    [InlineData("2026-05-20T00:00:00-05:00")]
    [InlineData("2026-05-20T00:00:00")]
    public void DuplicateExistingDateValue_WithUiIsoRequest_ReturnsDuplicateConflict(string value)
    {
        using var ctx = CreateContext();
        var date = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Unspecified);
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("PublishDate", ValueTypes.DateTime, isUnique: true));

        var existing = ctx.CreateEntity(type, Guid.NewGuid(),
            ctx.InvariantAttribute("Title", ValueTypes.String, "Existing"),
            ctx.InvariantAttribute("PublishDate", ValueTypes.DateTime, date));

        var result = Validate(ctx, [type], [existing], new(type.NameId, "PublishDate", value));

        False(result.IsValid);
        Equal("duplicate", result.Reason);
        Equal(existing.EntityId, result.ConflictEntityId);
    }

    [Fact]
    public void UrlPathFieldWithoutMetadata_StillValidatesAsUnique()
    {
        using var ctx = CreateContext();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String, uniqueSetting: null, inputType: "string-url-path", includeUniqueMetadata: false));

        var existing = ctx.CreateEntity(type, Guid.NewGuid(),
            ctx.InvariantAttribute("Title", ValueTypes.String, "Existing"),
            ctx.InvariantAttribute("Slug", ValueTypes.String, "same-slug"));

        var result = Validate(ctx, [type], [existing], new(type.NameId, "Slug", "same-slug"));

        False(result.IsValid);
        Equal("duplicate", result.Reason);
    }

    [Fact]
    public void DefaultLanguageWildcardRequest_IsHandledAsInvariant()
    {
        using var ctx = CreateContext();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String, isUnique: true));

        var existing = ctx.CreateEntity(type, Guid.NewGuid(),
            ctx.InvariantAttribute("Title", ValueTypes.String, "Existing"),
            ctx.InvariantAttribute("Slug", ValueTypes.String, "same-slug"));

        var result = Validate(ctx, [type], [existing], new(type.NameId, "Slug", "same-slug", UniqueValueValidationRules.DefaultLanguageWildcard));

        False(result.IsValid);
        Equal("duplicate", result.Reason);
        Equal(existing.EntityId, result.ConflictEntityId);
    }

    [Fact]
    public void PlainNonUniqueField_ReturnsNotApplicable()
    {
        using var ctx = CreateContext();
        var type = ctx.CreateType("Article",
            ctx.CreateField("Title", ValueTypes.String, isTitle: true),
            ctx.CreateField("Slug", ValueTypes.String));

        var result = Validate(ctx, [type], [], new(type.NameId, "Slug", "same-slug"));

        True(result.IsValid);
        Equal("not-applicable", result.Reason);
    }

    [Fact]
    public void MissingContentType_ReturnsTypeNotFound()
    {
        using var ctx = CreateContext();
        var result = Validate(ctx, [], [], new("Missing.Type", "Slug", "same-slug"));

        True(result.IsValid);
        Equal("type-not-found", result.Reason);
    }

    #endregion

    private static UniqueValueValidationResult Validate(
        IsUniqueValidatorTestContext ctx,
        IContentType[] contentTypes,
        IEntity[] existingEntities,
        UniqueValueValidationRequest request)
        => UniqueValueValidation.Validate(
            new TestContentTypes(contentTypes),
            ctx.CreateDataSource(existingEntities),
            ctx.CreateUniqueValueLookup(),
            request);

    private sealed class TestContentTypes(IContentType[] contentTypes) : IAppReadContentTypes
    {
        public IEnumerable<IContentType> ContentTypes => contentTypes;

        public IContentType? TryGetContentType(string name)
            => contentTypes.FirstOrDefault(type => type.Is(name));

        public IContentType GetContentType(string name)
            => TryGetContentType(name)
               ?? throw new ArgumentException($"Can't find content type with name '{name}'", nameof(name));
    }

    private IsUniqueValidatorTestContext CreateContext()
        => IsUniqueValidatorTestContext.Create(dataAssembler, ctAssemblyKit, dataSourcesService, dataSourceDependencies);
}
