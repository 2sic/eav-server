namespace ToSic.Eav.Models;

public enum NullHandling
{
    /// <summary>
    /// Represents an undefined state.
    /// Will usually behave the same as <see cref="ReturnNull"/>.
    /// </summary>
    /// <remarks>
    /// In rare cases the behavior might be different, since the system can detect if it was explicitly set or not.
    /// </remarks>
    Default = 0,

    /// <summary>
    /// If data is null, just return null.
    /// No conversion will be attempted, and no exception will be thrown.
    /// This is the default behavior.
    /// </summary>
    ReturnNull = 1 << 1,

    /// <summary>
    /// If data is null, force conversion, even if the model may not be able to handle it.
    /// </summary>
    /// <remarks>
    /// This should only be used if you are sure that the model can handle null sources,
    /// or if you want to force it to do so for testing purposes.
    ///
    /// Exceptions during conversion raised by the model itself will still be thrown.
    /// </remarks>
    ReturnModel = 1 << 2,
    
    /// <summary>
    /// If data is null, throw an exception.
    /// </summary>
    Throw = 1 << 3,

    /// <summary>
    /// If data is null, try to convert.
    /// If the model does not accept null, it will return null.
    /// If the model setup operation throws an exception, it will be raised, as it shows bad model design or a very unusual situation.
    /// </summary>
    TryOrNull = 1 << 4,

    /// <summary>
    /// If data is null, try to convert. If the model does not accept null, it will throw an exception.
    /// </summary>
    TryOrThrow = 1 << 5,
}