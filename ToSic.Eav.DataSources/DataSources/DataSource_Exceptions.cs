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
        /// Get a required Stream from In.
        /// If it doesn't exist return false and place the error message in the list for returning to the caller.
        ///
        /// Usage usually like this in your GetList() function:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   if (!GetRequiredInList(out var originals)) return originals;
        ///   var result = ...;
        ///   return result;
        /// }
        /// </code>
        /// Or if you're using [Call Logging](xref:NetCode.Logging.Index) do something like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var callLog = Log.Call&lt;IImmutableList&lt;IEntity&gt;&gt;();
        ///   if (!GetRequiredInList(out var originals)) return callLog("error", originals);
        ///   var result = ...
        ///   return callLog("ok", result);
        /// }
        /// </code>
        /// </summary>
        /// <returns>True if the stream exists and is not null, otherwise false</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected ListOrError GetInStream() 
            => GetInStream(Constants.DefaultStreamName);

        /// <summary>
        /// Get a specific Stream from In.
        /// If it doesn't exist return false and place the error message in the list for returning to the caller.
        ///
        /// Usage usually like this in your GetList() function: 
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   if (!GetRequiredInList("Fallback", out var fallback)) return fallback;
        ///   var result = ...;
        ///   return result;
        /// }
        /// </code>
        /// Or if you're using [Call Logging](xref:NetCode.Logging.Index) do something like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var callLog = Log.Call&lt;IImmutableList&lt;IEntity&gt;&gt;();
        ///   if (!GetRequiredInList("Fallback", out var fallback)) return callLog("error", fallback);
        ///   var result = ...
        ///   return callLog("ok", result);
        /// }
        /// </code>
        /// </summary>
        /// <param name="name">Stream name - optional</param>
        /// <returns>True if the stream exists and is not null, otherwise false</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected ListOrError GetInStream(string name)
        {
            if (!In.ContainsKey(name))
                return new ListOrError(isError: true,
                    getError: Error.Create(source: this, title: $"Stream '{name}' not found",
                        message: $"This DataSource needs the stream '{name}' on the In to work, but it couldn't find it."));
            var stream = In[name];
            if (stream == null)
                return new ListOrError(isError: true, 
                    getError: Error.Create(source: this, title: $"Stream '{name}' is Null", message: $"The Stream '{name}' was found on In, but it's null"));
            
            var list = stream.List?.ToImmutableList();
            if (list == null)
                return new ListOrError(isError: true,
                    getError: Error.Create(source: this, title: $"Stream '{name}' is Null",
                        message: $"The Stream '{name}' exists, but the List is null"));
            return new ListOrError(isError: false, getList: list);
        }
    }
}
