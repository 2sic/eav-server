using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
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
    Icon = Icons.RouteAlt,
    Type = DataSourceType.Filter,
    Audience = Audience.Advanced,
    NameId = "32369814-8f6d-47d8-a648-ce5372de78a8",
    DynamicOut = false,
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

    private readonly GenWorkPlus<WorkEntities> _workEntities;
    private readonly IUser _user;

    private IEnumerable<IEntity> GetList()
    {
        // Open the log after config-parse, so we have type names
        var l = Log.Fn<IEnumerable<IEntity>>($"get list with type:{TypeNames}");

        var entitiesSvc = _workEntities.New(this);

        // Case 1: No Type Names - return all entities in the Content-Scope
        if (TypeNames.IsEmptyOrWs())
        {
            var entities = entitiesSvc.OnlyContent(withConfiguration: _user.IsSystemAdmin).ToList();
            entities = FilterByIds(entities);
            return l.Return(entities, $"no type filter: {entities.Count}");
        }

        // Case 2: We have 1+ Type Names, let's get these
        var typeNames = TypeNames
            .Split(',')
            .Select(s => s.Trim())
            .Where(t => t.HasValue())
            .ToList();

        l.A($"found {typeNames.Count} type names");

        var appState = entitiesSvc.AppWorkCtx.AppState;
        try
        {
            var types = typeNames
                .Select(t => appState.GetContentType(t))
                .Where(t => t != null)
                .ToList();

            l.A($"found {types.Count} types");

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

    private List<IEntity> FilterByIds(List<IEntity> list)
    {
        var l = Log.Fn<List<IEntity>>($"started with {list.Count}");
        var raw = ItemIds;
        if (raw.IsEmptyOrWs()) return l.Return(list, "no filter, return all");

        var untyped = raw
            .Split(',')
            .Select(s => s.Trim())
            .Where(id => id.HasValue())
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