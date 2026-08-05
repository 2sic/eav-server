namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Should mark any object/interface which can be converted to a raw entity.
/// </summary>
/// <remarks>
/// This is a marker interface, and must implement one of the other interfaces to work properly.
/// These include:
/// * <see cref="IRawEntity"/> - for objects which are already a raw entity
/// * <see cref="IRawEntityConvertible"/> - for objects which can provide a converter for themselves
/// * <see cref="IRawEntityAutoConvert"/> - for objects which can be automatically converted to a raw entity
///
/// This interface should usually not be assigned directly, since it would be missing the converter or raw entity implementation.
/// </remarks>
[PrivateApi]
public interface IRawEntitySource: IRawData;