using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
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
            TitleField = nameof(DataTypeRaw.Name),
            TypeName = nameof(DataTypeRaw),
            AllowUnknownValueTypes = true,
        });

        ProvideOutRaw(GetReservedNames, name: "ReservedNames", options: () => new()
        {
            TitleField = nameof(ReservedNameRaw.Name),
            TypeName = nameof(ReservedNameRaw),
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<IRawEntity> GetInputTypes()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>($"{AppId}");

        var entities = _inputTypes.New(AppId)
            .GetInputTypes()
            .Select(inputType => new RawEntity(new()
            {
            { nameof(InputTypeInfo.Type), inputType.Type },
            { nameof(InputTypeInfo.Label), inputType.Label },
            { nameof(InputTypeInfo.Description), inputType.Description },
            { nameof(InputTypeInfo.DisableI18n), inputType.DisableI18n },
            { nameof(InputTypeInfo.UiAssets), inputType.UiAssets },
            { nameof(InputTypeInfo.UseAdam), inputType.UseAdam },
            { nameof(InputTypeInfo.IsObsolete), inputType.IsObsolete },
            { nameof(InputTypeInfo.ObsoleteMessage), inputType.ObsoleteMessage },
            { nameof(InputTypeInfo.IsRecommended), inputType.IsRecommended },
            { nameof(InputTypeInfo.IsDefault), inputType.IsDefault },
            { nameof(InputTypeInfo.Source), inputType.Source },
            { nameof(InputTypeInfo.ConfigTypes), inputType.ConfigTypes },
            }));

        return l.Return(entities, "ok");
    }

    private IEnumerable<IRawEntity> GetDataTypes()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>($"{AppId}");

        var entities = _attributesMod.New(AppId)
            .DataTypes()
            .Select(dataType => new RawEntity(new()
            {
                { nameof(DataTypeRaw.Name), dataType },
                { nameof(DataTypeRaw.Value), dataType },
            }));

        return l.Return(entities, "ok");
    }

    private IEnumerable<IRawEntity> GetReservedNames()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var entities = AttributeNames.ReservedNames
            .Select(reservedName => new RawEntity(new()
            {
                { nameof(ReservedNameRaw.Name), reservedName.Key },
                { nameof(ReservedNameRaw.Value), reservedName.Value },
            }));

        return l.Return(entities, "ok");
    }

    private class DataTypeRaw
    {
        public string Name { get; init; } = null!;
        public string Value { get; init; } = null!;
    }

    private class ReservedNameRaw
    {
        public string Name { get; init; } = null!;
        public string Value { get; init; } = null!;
    }
}