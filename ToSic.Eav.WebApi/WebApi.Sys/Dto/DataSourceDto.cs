using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class DataSourceDto
{
    public DataSourceDto(DataSourceInfo dsInfo, ICollection<string> outNameList)
    {
        var noError = dsInfo.ErrorOrNull == null;
        Name = noError ? dsInfo.Type.Name : dsInfo.NameId; // will override further down if dsInfo is provided
        Identifier = dsInfo.NameId;

        // If we don't have more information from the attribute, exit early
        var dsAttribute = dsInfo.VisualQuery;
        if (dsAttribute == null)
            return;

        UiHint = dsAttribute.UiHint;
        PrimaryType = dsAttribute.Type.ToString();
        Icon = dsAttribute.Icon; // ?.Replace("_", "-"); // wip: rename "xxx_yyy" icons to "xxx-yyy" - must switch to base64 soon
        HelpLink = dsAttribute.HelpLink;
        In = noError ? dsAttribute.In ?? [] : StreamNamesIfError;
        DynamicOut = dsAttribute.DynamicOut;
        OutMode = dsAttribute.OutMode;
        DynamicIn = dsAttribute.DynamicIn;
        EnableConfig = dsAttribute.EnableConfig;
        ContentType = dsAttribute.ConfigurationType;
        if (!string.IsNullOrEmpty(dsAttribute.NiceName))
            Name = dsAttribute.NiceName;
        Difficulty = (int)dsAttribute.Audience;
        Audience = (int)dsAttribute.Audience;
        IsGlobal = dsInfo.IsGlobal;

        // If we have a substituted error-DS, give it the inner name so connections work
        TypeNameForUi = noError ? dsInfo.Type.FullName : dsInfo.NameId;
        Out = noError ? outNameList : StreamNamesIfError;
        Errors = dsInfo.ErrorOrNull?.Message;

        // WIP try to deprecate
        PartAssemblyAndType = dsInfo.NameId;
    }

    public string TypeNameForUi { get; }

    // old, try to deprecate and replace with Identifier
    public string PartAssemblyAndType { get; }

    public string Identifier { get; }

    public ICollection<string> In { get; }
    public ICollection<string> Out { get; }
    public string ContentType { get; }
    public string PrimaryType { get; }
    public string Icon { get; }
    public bool DynamicOut { get; }

    public string OutMode { get; }

    public bool DynamicIn { get; }
    public string HelpLink { get; }
    public bool EnableConfig { get; }
    public string Name { get; }
    public string UiHint { get; }

    // todo: deprecated, but probably still in use in Visual Query - should be replaced by Audience
    public int Difficulty { get; }
    public int Audience { get; }

    public bool IsGlobal { get; }

    public string Errors { get; }

    private static readonly string[] StreamNamesIfError = [DataSourceConstants.StreamDefaultName];


}