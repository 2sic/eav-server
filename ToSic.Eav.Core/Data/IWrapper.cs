using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is for any object which does something, but caries with it an original object which is the type
    /// of the hosting system (environment)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IWrapper<out T>
    {
        // 2022-12-23 2dm Removed - use GetContents
        ///// <summary>
        ///// The underlying, original object. Helpful for inner methods which need access to the real, underlying item. <br/>
        ///// It has a lengthy name so that objects which implement the wrapper don't need to fear that another property would have the same name.
        ///// </summary>
        //T UnwrappedContents { get; }

        /// <summary>
        /// The underlying, original object. Helpful for external objects which need the real, underlying item.
        /// <br/>
        /// Avoid using this property for use inside the real object, but create another property for this, so it's easier to spot dependencies.
        /// </summary>
        T GetContents();
    }
}
