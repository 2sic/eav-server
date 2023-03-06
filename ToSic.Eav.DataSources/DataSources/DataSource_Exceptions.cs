using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    public partial class DataSource
    {
        /// <summary>
        /// This variable contains a stream of exceptions to return as a result of the DataSource.
        /// It is available because often some inner call will have to prepare an error and can't return the stream.
        /// So the inner call will set the variable and your primary GetList() can then do a check like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var useMultiLanguage = GetMultiLanguageSetting();
        ///   if (!ErrorStream.IsDefaultOrEmpty) return ErrorStream;
        ///   var result = ...
        ///   return result;
        /// }
        /// </code>
        /// Or if you're using [Call Logging](xref:NetCode.Logging.Index) do something like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var callLog = Log.Call&lt;IImmutableList&lt;IEntity&gt;&gt;();
        ///   var useMultiLanguage = GetMultiLanguageSetting();
        ///   if (!ErrorStream.IsDefaultOrEmpty) return callLog("error", ErrorStream);
        ///   var result = ...
        ///   return callLog("ok", result);
        /// }
        /// </code>
        /// </summary>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected IImmutableList<IEntity> ErrorStream;

        protected GetStream GetErrors() => new GetStream(isError: ErrorStream?.Any() == true, getError: () => ErrorStream);

        /// <summary>
        /// This will generate an Error Stream and return it as well as place it in the ErrorStream.
        /// </summary>
        /// <param name="title">Error title</param>
        /// <param name="message">Error message</param>
        /// <param name="ex">.net exception - would be logged if provided</param>
        /// <returns>A list containing the error entity</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected IImmutableList<IEntity> SetError(string title, string message, Exception ex = null)
        {
            var result = ErrorHandler.CreateErrorList(source: this, title: title, message: message, exception: ex);
            ErrorStream = result;
            return result;
        }

        protected IImmutableList<IEntity> CreateError(string title, string message, Exception ex = null) 
            => ErrorHandler.CreateErrorList(source: this, title: title, message: message, exception: ex);

        protected (IImmutableList<IEntity> ErrorList, string message) CreateErrorResult(string title, string message, Exception ex = null)
            => (CreateError(title, message, ex), "error");

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
        /// <param name="list">The variable which will contain the list or the error-list</param>
        /// <returns>True if the stream exists and is not null, otherwise false</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected GetStream GetRequiredInList() 
            => GetInStream(Constants.DefaultStreamName);

        /// <summary>
        /// Get a required Stream from In.
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
        /// <param name="list">The variable which will contain the list or the error-list</param>
        /// <returns>True if the stream exists and is not null, otherwise false</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        protected GetStream GetInStream(string name)
        {
            if (!In.ContainsKey(name))
                return new GetStream(isError: true,
                    getError: () => ErrorHandler.CreateErrorList(source: this, title: $"Stream '{name}' not found",
                        message: $"This DataSource needs the stream '{name}' on the In to work, but it couldn't find it."));
            var stream = In[name];
            if (stream == null)
                return new GetStream(isError: true, 
                    getError: () => ErrorHandler.CreateErrorList(source: this, title: $"Stream '{name}' is Null", message: $"The Stream '{name}' was found on In, but it's null"));
            
            var list = stream.List?.ToImmutableList();
            if (list == null)
                return new GetStream(isError: true,
                    getError: () => ErrorHandler.CreateErrorList(source: this, title: $"Stream '{name}' is Null",
                        message: $"The Stream '{name}' exists, but the List is null"));
            return new GetStream(isError: false, getList: () => list);
        }
    }
}
