﻿using ToSic.Eav.Apps;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Interface for the App Root - usually the very first node in any data-delivery or query. <br/>
/// It's just like a normal <see cref="IDataSource"/> but will internally access the <see cref="IAppReader"/> from the Cache/>.
/// </summary>
[VisualQuery(
    NiceName = "App Root Cache",
    UiHint = "All App data from the Cache",
    Icon = DataSourceIcons.TableChart,
    Type = DataSourceType.Source,
    Audience = Audience.Advanced,
    NameId = "ToSic.Eav.DataSources.IAppRoot, ToSic.Eav.DataSources",
    NameIds =
    [
        "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources",
        "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources"
    ],
    HelpLink = "https://go.2sxc.org/DsAppRoot")]
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
public interface IAppRoot : IDataSource;