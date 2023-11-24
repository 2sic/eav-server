using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that list all data scopes.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    ConfigurationType = "",
    NameId = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
    Icon = Icons.Scopes,
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
    public Scopes(MyServices services, IAppStates appStates) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
    {
        ConnectServices(_appStates = appStates);
        ProvideOutRaw(GetList, options: () => new DataFactoryOptions(typeName: "Scope", titleField: "Name", autoId: false));
    }
    private readonly IAppStates _appStates;

    private IEnumerable<IRawEntity> GetList() => Log.Func(l =>
    {
        var scopes = _appStates.Get(AppId).ContentTypes.GetAllScopesWithLabels();
        var list = scopes
            .Select(s => new RawEntity(new Dictionary<string, object>
            {
                { Data.Attributes.NameIdNiceName, s.Key },
                { "Name", s.Value }
            }))
            .ToList();

        return (list, $"{list.Count}");
    });
}