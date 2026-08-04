using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
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
            TitleField = nameof(InputTypeInfo.Type),
            TypeName = nameof(InputTypeInfo),
            AllowUnknownValueTypes = true,
        });

        ProvideOutRaw(GetDataTypes, name: "DataTypes", options: () => new()
        {
            TitleField = nameof(NameValueModel.Name),
            AllowUnknownValueTypes = true,
        });

        ProvideOutRaw(GetReservedNames, name: "ReservedNames", options: () => new()
        {
            TitleField = nameof(NameValueModel.Name),
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<InputTypeModel> GetInputTypes()
    {
        var l = Log.Fn<IEnumerable<InputTypeModel>>($"{AppId}");

        var entities = _inputTypes.New(AppId)
            .GetInputTypes()
            .Select(inputType => new InputTypeModel(inputType));

        return l.Return(entities, "ok");
    }

    private IEnumerable<NameValueModel> GetDataTypes()
    {
        var l = Log.Fn<IEnumerable<NameValueModel>>($"{AppId}");

        var entities = _attributesMod.New(AppId)
            .DataTypes()
            .Select(dataType => new NameValueModel(dataType));

        return l.Return(entities, "ok");
    }

    private IEnumerable<NameValueModel> GetReservedNames()
    {
        var l = Log.Fn<IEnumerable<NameValueModel>>();

        var entities = AttributeNames.ReservedNames
            .Select(reservedName => new NameValueModel(reservedName.Key, reservedName.Value));

        return l.Return(entities, "ok");
    }
}
