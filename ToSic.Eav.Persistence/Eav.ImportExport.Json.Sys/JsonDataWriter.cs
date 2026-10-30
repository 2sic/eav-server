using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Json.Sys;

internal partial class JsonSerializer(ITargetTypeService mdTargetSvc, LazySvc<IValueConverter> valConverter)
    : ServiceBase("Jsn.DWrite", connect: [mdTargetSvc, valConverter]), IJsonDataWriter
{

    #region Special Loggers

    public void ConfigureLogging(LogSettings settings)
    {
        var l = Log.Fn(settings.ToString());
        _disableLogDetails = Log.IfDetails(settings) == null;
        //_disableLogSummary = Log.IfSummary(settings) == null;
        l.Done();
    }

    public new ILog Log => base.Log;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDsDetails => _disableLogDetails ? null : Log;
    private bool _disableLogDetails;

    //internal ILog? LogDsSummary => _disableLogSummary ? null : Log;
    //private bool _disableLogSummary;

    #endregion


    /// <summary>
    /// WIP test API to ensure content-types serialized for UI resolve any hyperlinks.
    /// This is ATM only relevant to ensure that file-references in the WYSIWYG CSS work
    /// See https://github.com/2sic/2sxc/issues/2930
    /// Otherwise it's not used, so don't publish this API anywhere
    /// </summary>
    [PrivateApi]
    public bool ValueConvertHyperlinks { get; set; } = false;

}
