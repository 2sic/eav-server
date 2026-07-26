using ToSic.Eav.Apps;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys.Ancestors;
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
    NameIds = ["System.Scopes"],
    Icon = DataSourceIcons.Scopes,
    NiceName = "Data Scopes",
    Type = DataSourceType.System,
    UiHint = "Data Scopes group Content-Types by topic",
    Audience = Audience.Advanced,
    DataConfidentiality = DataConfidentiality.Confidential
)]
// ReSharper disable once UnusedMember.Global
public sealed class Scopes : CustomDataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Scopes DS
    /// </summary>
    [PrivateApi]
    public Scopes(Dependencies services, IAppReaderFactory appReadFac) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Scopes", connect: [appReadFac])
    {
        ProvideOutRaw(() =>
        {
            var contentTypes = appReadFac
                .Get(AppId).ContentTypes
                .ToListOpt();
            
            return contentTypes
                .GetAllScopesWithLabels()
                .Select(s =>
                {
                    var types = contentTypes
                        .OfScope(s.Key)
                        .ToListOpt();

                    var inheritCount = types.Count(t => t.HasAncestor());

                    return new ScopeModel
                    {
                        NameId = s.Key,
                        Name = s.Value,
                        TypesTotal = types.Count,
                        TypesInherited = inheritCount,
                        TypesOfApp = types.Count - inheritCount,
                    };
                });
        });
    }
}
