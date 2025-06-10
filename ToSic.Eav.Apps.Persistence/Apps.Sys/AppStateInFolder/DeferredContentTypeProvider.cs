using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Sys.Performance;
using ToSic.Sys.Utils;
using static System.StringComparer;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

/// <summary>
/// Special helper class for delayed construction of ContentTypes, so they can be immutable.
///
/// Basically the idea is that all generated Entities only have a wrapper ContentType,
/// which will retrieve its data from this source once they are completely generated. 
/// </summary>
internal class DeferredContentTypeProvider(ILog parentLog)
    : HelperBase(parentLog, "Eav.LdrCTP"), IDeferredContentTypeProvider
{
    /// <summary>
    /// The master list of content-types which is available for assigning later on.
    /// </summary>
    public List<IContentType> Source { get; } = [];

    protected IDictionary<string, IContentType> Lookup => _lookup
        ??= Source.ToDictionary(t => t.NameId, t => t, InvariantCultureIgnoreCase);
    private IDictionary<string, IContentType> _lookup;

    /// <summary>
    /// This will be called by the serializer to generate a delayed/lazy IContentType.
    /// </summary>
    IContentType IDeferredContentTypeProvider.LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback)
    {
        var delayedType = new ContentTypeWrapper(() => Lookup.TryGetValue(nameId, out var type)
            ? type
            : fallback
        );
        return delayedType;
    }

    public void AddTypes(IEnumerable<IContentType> list)
    {
        var deduplicated = EliminateDuplicateTypes(list);
        Source.AddRange(deduplicated);
        _lookup = null;
    }

    private IList<IContentType> EliminateDuplicateTypes(IEnumerable<IContentType> types)
    {
        var l = Log.Fn<IList<IContentType>>();

        // In rare cases there can be a mistake and the same type may be duplicate!
        var typesGrouped = types
            .GroupBy(t => t.NameId.ToLowerInvariant())
            .ToListOpt();

        var badGroups = typesGrouped
            .Where(g => g.Count() > 1);

        foreach (var badGroup in badGroups)
        {
            l.A("Warning: This type exists more than once - possibly defined in more plugins: " +
                $"'{badGroup.First().NameId}' / '{badGroup.First().Name}'");
            foreach (var badType in badGroup)
                l.A($"Source: {badType.RepositoryAddress}");
        }

        var final = typesGrouped
            .Select(g => g.First())
            .ToListOpt();
        return l.ReturnAsOk(final);
    }

    public List<IContentType> SetTypesOfContentTypeParts()
    {
        var l = Log.Fn<List<IContentType>>(timer: true);
        var changeCount = 0;

        try
        {
            var entitiesToRetype = Source
                .SelectMany(t => t.Metadata)
                .ToList();
            l.A($"Metadata found to retype: {entitiesToRetype.Count}");
            changeCount += UpdateTypes("ContentType Metadata", entitiesToRetype);
        }
        catch (Exception ex)
        {
            l.A("Error adding types");
            l.Ex(ex);
        }
        try
        {
            var entitiesToRetype = Source
                .SelectMany(t => t.Attributes.SelectMany(a => a.Metadata))
                .ToList();
            changeCount += UpdateTypes("Attribute Metadata", entitiesToRetype);
        }
        catch (Exception ex)
        {
            l.A("Error adding types");
            l.Ex(ex);
        }

        return l.Return(Source, $"{changeCount}");
    }

    public void SetTypesOfOtherEntities(List<IEntity> entities)
    {
        var l = Log.Fn<List<IContentType>>(timer: true);
        var changeCount = 0;
        try
        {
            changeCount += UpdateTypes("ContentType Metadata", entities);
        }
        catch (Exception ex)
        {
            l.A("Error adding types");
            l.Ex(ex);
        }

        l.Done($"{changeCount} changed");
    }

    private int UpdateTypes(string name, List<IEntity> entitiesToRetype)
    {
        var l = Log.Fn<int>($"For {name}", timer: true);
        var changeCount = 0;
        entitiesToRetype.ForEach(entity =>
        {
            if (entity.Type is ContentTypeWrapper { IsDeferred: true } wrapper)
            {
                l.A($"TypeReset: {entity.Type.NameId}");
                wrapper.Freeze();
                changeCount++;
            }

            l.A("TypeUnchanged:" + entity.Type.NameId);
        });

        return l.ReturnAsOk(changeCount);
    }
}