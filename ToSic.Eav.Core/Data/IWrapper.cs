namespace ToSic.Eav.Run
{
    /// <summary>
    /// This is for any object which does something, but caries with it an original object which is the type
    /// of the hosting system (environment)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWrapper<out T>
    {
        /// <summary>
        /// The underlying, original object. Helpful for inner methods which need access to the real, underlying item. <br/>
        /// It has a lengthy name so that objects which implement the wrapper don't need to fear that another property would have the same name.
        /// </summary>
        T UnwrappedContents { get; }

    }
}
