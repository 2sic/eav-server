namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// This is for any object which does something, but caries with it an original object which is the type
    /// of the hosting system (environment)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasOriginal<out T>
    {
        /// <summary>
        /// The underlying, original object. Helpful for inner methods which need access to the real, underlying item
        /// </summary>
        T Original { get; }

    }
}
