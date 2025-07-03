﻿using ToSic.Eav.Apps;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataSource.Sys;
using static ToSic.Eav.DataSource.DataSourceConstants;


namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Keep only entities of a specific content-type
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Type-Filter",
    UiHint = "Only keep items of the specified type",
    Icon = DataSourceIcons.RouteAlt,
    Type = DataSourceType.Filter, 
    NameId = "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [InStreamDefaultRequired],
    ConfigurationType = "|Config ToSic.Eav.DataSources.EntityTypeFilter",
    HelpLink = "https://go.2sxc.org/DsTypeFilter")]

public class EntityTypeFilter : DataSourceBase
{
    #region Configuration-properties

    /// <summary>
    /// The name of the type to filter for. Either the normal name or the 'StaticName' which is usually a GUID.
    /// </summary>
    [Configuration]
    public string TypeName
    {
        get => Configuration.GetThis(fallback: "");
        set => Configuration.SetThisObsolete(value);
    }

    // 2dm 2023-01-22 #maybeSupportIncludeParentApps
    //[PrivateApi("very experimental v15, special edge case")]
    //internal bool IncludeParentApps { get; set; }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityTypeFilter
    /// </summary>
    [PrivateApi]
    public EntityTypeFilter(IAppReaderFactory appReaders, Dependencies services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.TypeF")

    {
        _appReaders = appReaders;
        ProvideOut(GetList);
    }
    private readonly IAppReaderFactory _appReaders;


    private IImmutableList<IEntity> GetList() 
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();
        l.A($"get list with type:{TypeName}");

        // Get original from In-Stream
        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        try
        {
            var appState = _appReaders.Get(this);
            var foundType = appState?.TryGetContentType(TypeName);
            if (foundType != null) // maybe it doesn't find it!
            {
                var result = source.OfType(foundType).ToListOpt();
                return l.Return(result.ToImmutableOpt(), "fast");
            }
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            /* ignore */
        }

        // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
        // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way

        //if (!GetRequiredInList(out var originals2))
        //    return (originals2, "error");

        return l.Return(source.OfType(TypeName).ToImmutableOpt(), "slower");
    }

}