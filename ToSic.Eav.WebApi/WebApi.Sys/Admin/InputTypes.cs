using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "Input Types",
    NameId = "8c0b688e-c79a-4180-8123-5d1959f3a89f",
    NameIds = ["System.InputTypes"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Input types, data types and reserved field names"
)]
public class InputTypes : CustomDataSource
{
    private readonly GenWorkPlus<WorkInputTypes> _inputTypes;
    private readonly GenWorkDb<WorkAttributesMod> _attributesMod;

    public InputTypes(
        Dependencies services,
        GenWorkPlus<WorkInputTypes> inputTypes,
        GenWorkDb<WorkAttributesMod> attributesMod)
        : base(services, logName: "Sxc.InpTyp", connect: [inputTypes, attributesMod])
    {
        _inputTypes = inputTypes;
        _attributesMod = attributesMod;

        ProvideOutRaw(GetInputTypes, name: "InputTypes", options: () => new()
        {
            AllowUnknownValueTypes = true,
        });

        ProvideOutRaw(GetDataTypes, name: "DataTypes");

        ProvideOutRaw(GetReservedNames, name: "ReservedNames");
    }

    private IEnumerable<InputTypeInfoRaw> GetInputTypes()
    {
        var l = Log.Fn<IEnumerable<InputTypeInfoRaw>>($"{AppId}");

        var entities = _inputTypes.New(AppId)
            .GetInputTypes()
            .Select(inputType => new InputTypeInfoRaw(source: inputType));

        return l.Return(entities, "ok");
    }

    private IEnumerable<NameValueRaw> GetDataTypes()
    {
        var l = Log.Fn<IEnumerable<NameValueRaw>>($"{AppId}");

        var entities = _attributesMod.New(AppId)
            .DataTypes()
            .Select(dataType => new NameValueRaw(Name: dataType));

        return l.Return(entities, "ok");
    }

    private IEnumerable<NameValueRaw> GetReservedNames()
    {
        var l = Log.Fn<IEnumerable<NameValueRaw>>();

        var entities = AttributeNames.ReservedNames
            .Select(reservedName => new NameValueRaw(Name: reservedName.Key, Value: reservedName.Value));

        return l.Return(entities, "ok");
    }
}
