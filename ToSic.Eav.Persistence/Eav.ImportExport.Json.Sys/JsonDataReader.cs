using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Global;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Json.Sys;

internal partial class JsonDataReader(ITargetTypeService mdTargetSvc, IGlobalDataService globalDataSvc, DataAssembler dataAssembler, ContentTypeAssemblyKit typeAssemblyKit)
    : ServiceWithSetup<IJsonDataReader.Options>("Jsn.DaRead", connect: [mdTargetSvc, globalDataSvc, dataAssembler, typeAssemblyKit]), IJsonDataReader
{
    /// <summary>
    /// Temporary solution to surface this property, so it can be replaced temporarily.
    /// TODO: A better fix should be done in future, basically the caller should use two separate serializers.
    /// </summary>
    IJsonDataReader.Options IJsonDataReader.MyOptions
    {
        get => field ??= base.MyOptions ?? throw new("Can't use DeserializationOptions - you forgot to call Init");
        set;
    }

    public override IJsonDataReader.Options MyOptions => (this as IJsonDataReader).MyOptions;


    private JsonDeserializeEntityHelper JsonDeserializeEntityHelper => field ??= new(globalDataSvc, typeAssemblyKit, MyOptions, LogDsDetails);




    public void ConfigureLogging(LogSettings settings)
    {
        var l = Log.Fn(settings.ToString());
        _disableLogDetails = Log.IfDetails(settings) == null;
        _disableLogSummary = Log.IfSummary(settings) == null;
        l.Done();
    }
    public new ILog Log => base.Log;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDsDetails => _disableLogDetails ? null : Log;
    private bool _disableLogDetails;

    internal ILog? LogDsSummary => _disableLogSummary ? null : Log;
    private bool _disableLogSummary;

}
