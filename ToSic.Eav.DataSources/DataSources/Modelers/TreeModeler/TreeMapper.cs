using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.DataSources.Sys;

namespace ToSic.Eav.DataSources;

internal partial class TreeMapper : ServiceBase, ITreeMapper, ICanDebug
{
    public const string DefaultParentFieldName = "Parent";
    public const string DefaultChildrenFieldName = "Children";

    #region Constructor / DI

    private readonly DataAssembler _dataAssembler;

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="dataAssembler"></param>
    public TreeMapper(DataAssembler dataAssembler): base("DS.TreeMp")
    {
        _dataAssembler = dataAssembler;
        Debug = false;
    }

    #endregion


    public bool Debug { get; set; }
}