using ToSic.Eav.Data.ContentTypes;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.Persistence.File;

/// <summary>
/// Special helper class for delayed construction of ContentTypes so they can be immutable.
///
/// Basically the idea is that all generated Entities only have a wrapper ContentType,
/// which will retrieve it's data from this source once they are completely generated. 
/// </summary>
internal class DeferredContentTypeProvider(ILog parentLog)
    : HelperBase(parentLog, "Eav.LdrCTP"), IDeferredContentTypeProvider
{
    /// <summary>
    /// The master list of content-types which is available for assigning later on.
    /// </summary>
    public List<IContentType> Source { get; } = [];

    protected IDictionary<string, IContentType> Lookup =>
        _lookup.Get(() => Source.ToDictionary(t => t.NameId, t => t, InvariantCultureIgnoreCase));
    private readonly GetOnce<IDictionary<string, IContentType>> _lookup = new();

    /// <summary>
    /// This will be called by the serializer to generate a delayed/lazy IContentType.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="name"></param>
    /// <param name="nameId"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    IContentType IDeferredContentTypeProvider.LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback)
    {
        var delayedType = new ContentTypeWrapper(() => Lookup.TryGetValue(nameId, out var type)
            ? type
            : fallback
        );
        return delayedType;
    }

    public void AddTypes(List<IContentType> list)
    {
        list = EliminateDuplicateTypes(list);
        Source.AddRange(list);
        _lookup.Reset();
    }

    public List<IContentType> ProcessSubEntitiesOnTypes() => SetTypesOfContentTypeParts(Source);


    private List<IContentType> EliminateDuplicateTypes(List<IContentType> types)
    {
        var l = Log.Fn<List<IContentType>>();

        // In rare cases there can be a mistake and the same type may be duplicate!
        var typesGrouped = types
            .GroupBy(t => t.NameId.ToLowerInvariant())
            .ToList();

        var badGroups = typesGrouped.Where(g => g.Count() > 1);
        foreach (var badGroup in badGroups)
        {
            l.A("Warning: This type exists more than once - possibly defined in more plugins: " +
                $"'{badGroup.First().NameId}' / '{badGroup.First().Name}'");
            foreach (var badType in badGroup)
                l.A($"Source: {badType.RepositoryAddress}");
        }

        var final = typesGrouped
            .Select(g => g.First())
            .ToList();
        return l.ReturnAsOk(final);
    }

    private List<IContentType> SetTypesOfContentTypeParts(List<IContentType> typesDistinct)
    {
        var l = Log.Fn<List<IContentType>>(timer: true);
        var changeCount = 0;
        try
        {
            var entitiesToRetype = typesDistinct
                .SelectMany(t => t.Metadata)
                .ToList();
            l.A($"Metadata found to retype: {entitiesToRetype.Count}");
            changeCount += UpdateTypes("ContentType Metadata", entitiesToRetype);

            entitiesToRetype = typesDistinct
                .SelectMany(t => t.Attributes.SelectMany(a => a.Metadata))
                .ToList();
            changeCount += UpdateTypes("Attribute Metadata", entitiesToRetype);
        }
        catch (Exception ex)
        {
            l.A("Error adding types");
            l.Ex(ex);
        }

        return l.Return(typesDistinct, $"{changeCount}");
    }

    private int UpdateTypes(string name, List<IEntity> entitiesToRetype)
    {
        var l = Log.Fn<int>($"For {name}", timer: true);
        var changeCount = 0;
        var sameChanged = entitiesToRetype
            .Select(entity =>
            {
                if (entity.Type is ContentTypeWrapper wrapper && wrapper.IsDeferred)
                {
                    l.A($"TypeReset: {entity.Type.NameId}");
                    wrapper.Freeze();
                    changeCount++;
                    return entity;
                }
                l.A("TypeUnchanged:" + entity.Type.NameId);
                return entity;
            })
            .ToList();

        return l.ReturnAsOk(changeCount);
    }
}