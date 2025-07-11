﻿using ToSic.Eav.Apps;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.DataSource.Sys;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that list all data scopes.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    ConfigurationType = "",
    NameId = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
    Icon = DataSourceIcons.Scopes,
    NiceName = "Data Scopes",
    Type = DataSourceType.System,
    UiHint = "Data Scopes group Content-Types by topic",
    Audience = Audience.Advanced,
    DynamicOut = false
)]
// ReSharper disable once UnusedMember.Global
public sealed class Scopes : CustomDataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Scopes DS
    /// </summary>
    [PrivateApi]
    public Scopes(Dependencies services, IAppReaderFactory appReadFac) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Scopes")
    {
        ConnectLogs([_appReadFac = appReadFac]);
        ProvideOutRaw(GetList, options: () => new()
        {
            AutoId = false,
            TitleField = "Name",
            TypeName = "Scope",
        });
    }
    private readonly IAppReaderFactory _appReadFac;

    private IEnumerable<IRawEntity> GetList()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var scopes = _appReadFac.Get(AppId).ContentTypes.GetAllScopesWithLabels();
        var list = scopes
            .Select(s => new RawEntity(new()
            {
                { AttributeNames.NameIdNiceName, s.Key },
                { "Name", s.Value }
            }))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}