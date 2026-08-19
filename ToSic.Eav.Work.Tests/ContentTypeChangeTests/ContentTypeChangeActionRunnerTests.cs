using ToSic.Eav.Data;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;
using ToSic.Sys.HookUp;

namespace ToSic.Eav.ContentTypeChangeTests;

/// <summary>
/// Covers the trigger which regenerates models after field-settings metadata was saved.
/// </summary>
public class ContentTypeChangeActionRunnerTests
{
    private const int AppId = 42;

    [Fact]
    public void FieldMetadataTriggersOwningTypeOncePerType()
    {
        var (runner, action) = NewRunner();

        // 2 fields of Alpha + 1 of Beta => 2 runs, not 3
        runner.RunForFieldMetadata(AppId, ContentTypes(),
            [AttributeTarget(11), AttributeTarget(12), AttributeTarget(21)]);

        Equal(new[] { "Alpha", "Beta" }, action.Types.OrderBy(t => t).ToArray());
    }

    /// <summary>
    /// The caller uses prepareForRun to purge the app-cache, so actions see field settings
    /// from after the save. It must run after resolving (which needs the attribute) and before
    /// the first action - and not at all when there is nothing to generate.
    /// </summary>
    [Fact]
    public void PrepareForRunHappensOnceBeforeTheActions()
    {
        var (runner, action) = NewRunner();
        var prepared = 0;

        runner.RunForFieldMetadata(AppId, ContentTypes(), [AttributeTarget(11), AttributeTarget(21)],
            prepareForRun: () =>
            {
                Empty(action.Types); // nothing generated yet
                prepared++;
            });

        Equal(1, prepared);
        Equal(new[] { "Alpha", "Beta" }, action.Types.OrderBy(t => t).ToArray());
    }

    [Fact]
    public void PrepareForRunSkippedWhenNothingToGenerate()
    {
        var (runner, _) = NewRunner();
        var prepared = 0;

        runner.RunForFieldMetadata(AppId, ContentTypes(), [AttributeTarget(999)],
            prepareForRun: () => prepared++);

        Equal(0, prepared);
    }

    [Fact]
    public void UnknownOrNonFieldTargetsTriggerNothing()
    {
        var (runner, action) = NewRunner();

        runner.RunForFieldMetadata(AppId, ContentTypes(), [
            AttributeTarget(999), // no such field
            new Target((int)TargetTypes.Entity, null, keyNumber: 11), // not a field target
        ]);

        Empty(action.Types);
    }

    [Fact]
    public void DoWithoutActionsSuppressesOnlyInsideTheScope()
    {
        var (runner, action) = NewRunner();

        runner.DoWithoutActions(() =>
            runner.RunForFieldMetadata(AppId, ContentTypes(), [AttributeTarget(11)]));
        Empty(action.Types);

        runner.RunForFieldMetadata(AppId, ContentTypes(), [AttributeTarget(11)]);
        Equal(new[] { "Alpha" }, action.Types.ToArray());
    }

    #region Helpers

    private static ITarget AttributeTarget(int attributeId)
        => new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);

    private static (ContentTypeChangeActionRunner Runner, RecordingAction Action) NewRunner()
    {
        var action = new RecordingAction();
        return (new ContentTypeChangeActionRunner([action]), action);
    }

    private static IEnumerable<IContentType> ContentTypes()
    {
        var ctAssembler = new ContentTypeAssembler();
        ctAssembler.Setup(new());
        var fieldAssembler = new ContentTypeFieldAssembler();
        fieldAssembler.Setup(new());

        IContentTypeField Field(string name, int id)
            => fieldAssembler.Create(appId: AppId, name: name, type: ValueTypes.String, isTitle: false, id: id);

        return
        [
            ctAssembler.Create(appId: AppId, name: "Alpha", nameId: "Alpha", id: 1, scope: "Default",
                attributes: [Field("A1", 11), Field("A2", 12)]),
            ctAssembler.Create(appId: AppId, name: "Beta", nameId: "Beta", id: 2, scope: "Default",
                attributes: [Field("B1", 21)]),
        ];
    }

    private class RecordingAction : IWork<ContentTypeChange>
    {
        public List<string> Types { get; } = [];

        public Task<Package<ContentTypeChange>> Handle(WorkContext mainCtx, Package<ContentTypeChange> package)
        {
            Types.Add(package.Data.ContentTypeNameId);
            return Task.FromResult(package);
        }
    }

    #endregion
}
