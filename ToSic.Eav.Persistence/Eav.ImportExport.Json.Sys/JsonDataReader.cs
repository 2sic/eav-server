using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Global;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Json.Sys;

public partial class JsonDataReader(ITargetTypeService mdTargetSvc, IGlobalDataService globalDataSvc, DataAssembler dataAssembler, ContentTypeAssemblyKit typeAssemblyKit)
    : ServiceBase("Jsn.DaRead", connect: [mdTargetSvc, globalDataSvc, dataAssembler, typeAssemblyKit]), IJsonDataReader
{
    public JsonDataReadOptions DesOptions
    {
        get => field ?? throw new("Can't use DeserializationOptions - you forgot to call Init");
        set;
    } = null!;


    public void Initialize(JsonDataReadOptions options)
    {
        DesOptions = options;
    }
    public IJsonDataReader Setup(JsonDataReadOptions options)
    {
        DesOptions = options;
        return this;
    }


    private JsonDeserializeEntityHelper JsonDeserializeEntityHelper => field ??= new(globalDataSvc, typeAssemblyKit, DesOptions, LogDsDetails);




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
