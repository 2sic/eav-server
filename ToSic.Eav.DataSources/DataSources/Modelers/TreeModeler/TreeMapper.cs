using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Internal;

namespace ToSic.Eav.DataSources;

internal partial class TreeMapper : ServiceBase, ITreeMapper, ICanDebug
{
    public const string DefaultParentFieldName = "Parent";
    public const string DefaultChildrenFieldName = "Children";

    #region Constructor / DI

    private readonly DataBuilder _builder;

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="builder"></param>
    public TreeMapper(DataBuilder builder): base("DS.TreeMp")
    {
        _builder = builder;
        Debug = false;
    }

    #endregion


    public bool Debug { get; set; }
}