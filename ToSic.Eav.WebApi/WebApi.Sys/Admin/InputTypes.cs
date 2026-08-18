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
    public InputTypes(
        Dependencies services,
        AppWorkContextService appWorkContextService,
        AppWorkChain<WorkInputTypes> inputTypes,
        AppWorkChain<WorkFieldsDataTypes> fieldsDataTypes)
        : base(services, logName: "Sxc.InpTyp", connect: [appWorkContextService, inputTypes, fieldsDataTypes])
    {
        var ctx = new Lazy<IAppWorkContext>(() => appWorkContextService.ContextNew(AppId));
        
        ProvideOutRaw(() => GetInputTypes(inputTypes.New(ctx.Value)),
            name: "InputTypes",
            options: () => new()
            {
                AllowUnknownValueTypes = true,
            });

        ProvideOutRaw(() => GetDataTypes(fieldsDataTypes.New(ctx.Value)), name: "DataTypes");

        ProvideOutRaw(GetReservedNames, name: "ReservedNames");
    }

    private IEnumerable<InputTypeInfoRaw> GetInputTypes(WorkInputTypes inputTypes)
    {
        var l = Log.Fn<IEnumerable<InputTypeInfoRaw>>($"{AppId}");

        var entities = inputTypes
            .GetInputTypes()
            .Select(inputType => new InputTypeInfoRaw
            {
                Type = inputType.Type,
                Label = inputType.Label,
                Description = inputType.Description,
                DisableI18n = inputType.DisableI18n,
                UiAssets = inputType.UiAssets,
                UseAdam = inputType.UseAdam,
                IsObsolete = inputType.IsObsolete ?? false,
                ObsoleteMessage = inputType.ObsoleteMessage,
                IsRecommended = inputType.IsRecommended ?? false,
                IsDefault = inputType.IsDefault ?? false,
                Source = inputType.Source,
                ConfigTypes = inputType.ConfigTypes == null ? null : [inputType.ConfigTypes],
            });

        return l.Return(entities, "ok");
    }

    private IEnumerable<NameValueRaw> GetDataTypes(WorkFieldsDataTypes fieldsDataTypes)
    {
        var l = Log.Fn<IEnumerable<NameValueRaw>>($"{AppId}");

        var entities = fieldsDataTypes
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
