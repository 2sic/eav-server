﻿using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that list all features.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Features",
    UiHint = "List all features",
    Icon = DataSourceIcons.TableChart,
    Type = DataSourceType.System,
    NameId = "398d0b9f-044f-48f7-83ef-307872f7ed93",
    Audience = Audience.Advanced,
    DynamicOut = false
)]
// ReSharper disable once UnusedMember.Global
public sealed class Features : CustomDataSource
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Scopes DS
    /// </summary>
    [PrivateApi]
    public Features(MyServices services, IEavFeaturesService featuresService) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Feats")
    {
        ConnectLogs([featuresService]);
        ProvideOutRaw(() => featuresService.All.OrderBy(f => f.NameId), options: () => new() { TypeName = "Feature" });
    }
}