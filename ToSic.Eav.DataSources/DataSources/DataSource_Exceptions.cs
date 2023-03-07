using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    public partial class DataSource
    {
        /// <summary>
        /// Special helper which would probably confuse external developers, so it's protected and internal
        /// </summary>
        /// <returns>A tuple containing both the error and the log-message</returns>
        [PrivateApi]
        protected internal (IImmutableList<IEntity> ErrorList, string message) ErrorResult(string title, string message, Exception exception = null)
            => (Error.Create(source: this, title: title, message: message, exception: exception), $"error: {title}");

        /// <summary>
        /// Get a specific Stream from In.
        /// If it doesn't exist return false and place the error message in the list for returning to the caller.
        ///
        /// Usage usually like this in your GetList() function: 
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var source = TryGetIn();
        ///   if (source is null) return Error.TryGetInFailed(this);
        ///   var result = source.Where(s => ...).ToImmutableList();
        ///   return result;
        /// }
        /// </code>
        /// Or if you're using [Call Logging](xref:NetCode.Logging.Index) do something like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList() => Log.Func(l =>
        /// {
        ///   var source = TryGetIn();
        ///   if (source is null) return (Error.TryGetInFailed(this), "error");
        ///   var result = source.Where(s => ...).ToImmutableList();
        ///   return (result, $"ok - found: {result.Count}");
        /// });
        /// </code>
        /// </summary>
        /// <param name="name">Stream name - optional</param>
        /// <returns>A list containing the data, or null if not found / something breaks.</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected IImmutableList<IEntity> TryGetIn(string name = Constants.DefaultStreamName) => !In.ContainsKey(name) ? null : In[name]?.List?.ToImmutableList();
    }
}
