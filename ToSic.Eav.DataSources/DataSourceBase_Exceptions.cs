using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.DataSources.DataSourceErrorHandling;

namespace ToSic.Eav.DataSources
{
    public partial class DataSourceBase
    {
        /// <summary>
        /// This contains a stream of exceptions.
        /// It's used by deeper code that raises an issue, but can't just return a stream of data. 
        /// </summary>
        [PrivateApi]
        protected ImmutableArray<IEntity> ExceptionStream;

        /// <summary>
        /// This will generate an Exception stream for direct return or place it on the ExceptionStream
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        [PrivateApi]
        protected ImmutableArray<IEntity> SetException(string title, string message, Exception ex = null)
        {
            var result = CreateErrorList(source: this, title: title, message: message, exception: ex);
            ExceptionStream = result;
            return result;
        }

        //[PrivateApi]
        protected bool GetStreamOrPrepareExceptionToThrow(string name, out IImmutableList<IEntity> list)
        {
            if (In.ContainsKey(name))
            {
                var stream = In[name];
                if (stream != null)
                {
                    list = stream.List.ToImmutableList();
                    if (list != null) return false;
                    list = SetException($"Stream '{name}' is Null", "The Stream exists, but the List is null");
                    return true;
                }

                list = SetException($"Stream '{name}' is Null", "The Stream was found on In, but it's null");
                return true;
            }
            list = SetException($"Stream '{name}' not found", $"This DataSource needs the stream '{name}' on the In to work, but it couldn't find it.");
            return true;
        }
    }
}
