using ToSic.Eav.Context;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Streams.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Keep only entities of a specific content-type
/// </summary>
[PrivateApi]
[VisualQuery(
    NiceName = "Entity-Picker (internal)",
    UiHint = "Special DataSource for the standard Entity-Picker",
    Icon = DataSourceIcons.RouteAlt,
    Type = DataSourceType.Filter,
    Audience = Audience.Advanced,
    NameId = "32369814-8f6d-47d8-a648-ce5372de78a8",
    DynamicOut = true,
    // ConfigurationType = "not yet defined", // ATM we don't expect a configuration
    HelpLink = "https://go.2sxc.org/todo")]

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntityPicker : DataSourceBase
{
    #region Configuration-properties

    /// <summary>
    /// The name of the types to filter for.
    /// Either the normal name or the 'StaticName' which is usually a GUID.
    ///
    /// Can be many, comma separated
    /// </summary>
    [Configuration(Fallback = "[QueryString:TypeNames]")]
    public string TypeNames => Configuration.GetThis();

    /// <summary>
    /// List of IDs to filter against - reducing the final set to just a few items
    /// </summary>
    [Configuration(Fallback = "[QueryString:ItemIds]")]
    public string ItemIds => Configuration.GetThis();

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityTypeFilter
    /// </summary>
    [PrivateApi]
    public EntityPicker(GenWorkPlus<WorkEntities> workEntities, IUser user, MyServices services) : base(services, "Api.EntPck")
    {
        ConnectServices(
            _workEntities = workEntities
        );
        _user = user;
        ProvideOut(GetList);
    }

    #region DynamicOut

    public override IReadOnlyDictionary<string, IDataStream> Out => _out.Get(() =>
    {
        // 0. If no names specified, then out is same as base out
        var typesWithoutDefault = ContentTypes?.Where(ct => !ct.Name.EqualsInsensitive(StreamDefaultName)).ToList();
        if (TypeNames.IsEmptyOrWs() || ContentTypes.SafeNone()) return base.Out;

        // 1. Create a new StreamDictionary with the Default
        var outList = new StreamDictionary(this);
        outList.Add(StreamDefaultName, base.Out[StreamDefaultName]);

        // 2. Generate additional streams based on the content-types requested
        var list = base.Out[StreamDefaultName].List;
        foreach (var contentType in typesWithoutDefault)
        {
            var name = contentType.Name;
            var outStream = new DataStream(Services.CacheService, this, name, () => list.OfType(contentType), true);
            outList.Add(name, outStream);
        }

        return outList.AsReadOnly();
    });

    private readonly GetOnce<IReadOnlyDictionary<string, IDataStream>> _out = new();


    #endregion

    private readonly GenWorkPlus<WorkEntities> _workEntities;
    private readonly IUser _user;


    private IEnumerable<IEntity> GetList()
    {
        // Open the log after config-parse, so we have type names
        var l = Log.Fn<IEnumerable<IEntity>>($"get list with type:{TypeNames}");

        var entitiesSvc = WorkEntities;

        // Case 1: No Type Names - return all entities in the Content-Scope
        if (TypeNames.IsEmptyOrWs())
        {
            var entities = entitiesSvc.OnlyContent(withConfiguration: _user.IsSystemAdmin).ToList();
            entities = FilterByIds(entities);
            return l.Return(entities, $"no type filter: {entities.Count}");
        }

        // Case 2: We have 1+ Type Names, let's get these
        //var typeNames = TypeNames
        //    .Split(',')
        //    .Select(s => s.Trim())
        //    .Where(t => t.HasValue())
        //    .ToList();

        //l.A($"found {typeNames.Count} type names");

        //var appState = entitiesSvc.AppWorkCtx.AppState;
        try
        {
            var types = ContentTypes;
            //typeNames
            //    .Select(appState.GetContentType)
            //    .Where(t => t != null)
            //    .ToList();

            if (types == null) return l.ReturnAsError(Error.Create(title: "TypeList==null, something threw an error there."));

            if (!types.Any()) return l.Return(new List<IEntity>(), "no valid types found, empty list");

            var result = new List<IEntity>();
            foreach (var type in types)
            {
                var lType = l.Fn($"Adding all of '{type.Name}'");
                var ofType = entitiesSvc.AppWorkCtx.Data.List.OfType(type).ToList();
                result.AddRange(ofType);
                lType.Done($"{ofType.Count}");
            }

            if (result.Any()) result = FilterByIds(result);
            return l.Return(result, $"typed/filtered: {result.Count}");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            /* ignore */
            return l.Return(Error.Create(title: "Something went wrong", message: "Unknown problem", exception: ex), "error");
        }

    }

    private WorkEntities WorkEntities => _workEntitiesReal.Get(() => _workEntities.New(this));
    private readonly GetOnce<WorkEntities> _workEntitiesReal = new();

    private List<IContentType> ContentTypes => _contentTypes.Get(() =>
    {
        var l = Log.Fn<List<IContentType>>();
        // Case 2: We have 1+ Type Names, let's get these

        try
        {
            var typeNames = TypeNames.CsvToArrayWithoutEmpty()
                //.Split(',')
                //.Select(s => s.Trim())
                //.Where(t => t.HasValue())
                .ToList();

            l.A($"found {typeNames.Count} type names, before verifying if they exist");

            if (!typeNames.Any()) return l.Return([]);

            var appState = WorkEntities.AppWorkCtx.AppState;

            var types = typeNames
                .Select(appState.GetContentType)
                .Where(t => t != null)
                .ToList();

            return l.Return(types, $"found {types.Count}");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            /* ignore */
            return l.ReturnAsError(null);
        }
    });
    private readonly GetOnce<List<IContentType>> _contentTypes = new();

    private List<IEntity> FilterByIds(List<IEntity> list)
    {
        var l = Log.Fn<List<IEntity>>($"started with {list.Count}");
        var raw = ItemIds;
        if (raw.IsEmptyOrWs()) return l.Return(list, "no filter, return all");

        var untyped = raw.CsvToArrayWithoutEmpty()
            //.Split(',')
            //.Select(s => s.Trim())
            //.Where(id => id.HasValue())
            .ToList();

        if (!untyped.Any()) return l.Return(list, "empty filter, return all");

        var result = new List<IEntity>();
        foreach (var id in untyped)
        {
            IEntity found = null;
            // check if id is int or guid
            if (Guid.TryParse(id, out var guid)) found = list.One(guid);
            else if (int.TryParse(id, out var intId)) found = list.One(intId);
            if (found != null) result.Add(found);
        }
        return l.Return(result, $"filtered to {result.Count}");
    }

}