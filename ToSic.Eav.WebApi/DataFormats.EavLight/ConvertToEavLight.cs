using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.DataFormats.EavLight.Sys;
using ToSic.Eav.Serialization.Sys.Options;

namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// A helper to serialize various combinations of entities, lists of entities etc.
/// </summary>
[PrivateApi("Hide Implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal partial class ConvertToEavLight(ConvertToEavLight.Dependencies services, ConvertEntityToEavLight convShared)
    : ServiceBase<ConvertToEavLight.Dependencies>(services, "Eav.CnvE2D", connect: [convShared]), IConvertToEavLight
{
    #region Constructor / DI

    public record Dependencies()
        : DependenciesBase(connect: []);

    #endregion

    #region Configuration

    /// <inheritdoc />
    [WorkInProgressApi("Exact name not final yet")]
    public int MaxItems { get => convShared.MaxItems; set => convShared.MaxItems = value; }

    /// <inheritdoc/>
    public bool WithGuid { get => convShared.WithGuid; set => convShared.WithGuid = value; }
    /// <inheritdoc/>
    public bool WithPublishing { get => convShared.WithPublishing; private set => convShared.WithPublishing = value; }

    [PrivateApi("Actually Internal!")]
    public void AddSelectFields(ICollection<string> fields)
        => convShared.PresetFilters = new EntitySerializationDecoratorCreator(fields, WithGuid, Log).Generate();

    #endregion

    #region Language

    public string?[] Languages { get => convShared.Languages; set => convShared.Languages = value; }

    /// <summary>
    /// Separate helper to create lightweight sub entities
    /// </summary>
    [PrivateApi("Actually Internal!")]
    [field: AllowNull, MaybeNull]
    ConvertToJsonBasicLightSubEntities IConvertToEavLight.SubConverter => field ??= new(Languages);


    [PrivateApi("not public ATM")]
    public TypeSerializationOptions Type { get => convShared.Type; set => convShared.Type = value; }

    #endregion

    

}