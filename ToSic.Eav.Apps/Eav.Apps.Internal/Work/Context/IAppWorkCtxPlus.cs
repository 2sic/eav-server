using ToSic.Eav.DataSource;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Extended context to work with App Data.
/// Enhances the base class with draft information and a DataSource
/// providing data according to draft.
///
/// In rare cases the Data can also be replaced with further restrictions.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtxPlus : IAppWorkCtx
{
    bool? ShowDrafts { get; }
    IDataSource Data { get; }

    IAppWorkCtxPlus NewWithPresetData(IDataSource data);

}