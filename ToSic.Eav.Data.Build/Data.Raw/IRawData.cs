namespace ToSic.Eav.Data.Raw;

/// <summary>
/// A marker interface to specify that an object is a raw data.
/// This means in can be converted to other formats such as <see cref="IEntity"/>.
/// </summary>
/// <remarks>
/// This is a marker interface, and classes/records must implement one of the other interfaces to convent to <see cref="IEntity"/>.
/// This interface should usually not be assigned directly, since it would be missing the converter or raw entity implementation.
/// Interfaces to actually implement include:
/// 
/// * <see cref="IRawEntity"/> - for objects which are already a raw entity
/// * <see cref="IRawEntityConvertible"/> - for objects which can provide a converter for themselves
/// * <see cref="IRawEntityAutoConvert"/> - for objects which can be automatically converted to a raw entity
/// </remarks>
[WorkInProgressApi("v22")]
public interface IRawData;
