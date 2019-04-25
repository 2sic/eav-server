using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Wrap a log entry - basically log something with a prefix, 
        /// and return a method which can be used for the done-message which will use the same prefix
        /// </summary>
        /// <param name="open"></param>
        /// <returns></returns>
        private Action<string> Wrapper(string open)
        {
            var entry = AddEntry(open);
            WrapDepth++;
            return message =>
            {
                WrapDepth--;
                entry.AppendResult(message);
            };
        }

        private Func<string, T, T> Wrapper<T>(string open)
        {
            var entry = AddEntry(open);
            WrapDepth++;
            return (message, result) =>
            {
                WrapDepth--;
                entry.AppendResult(message);
                return result;
            };
        }
    }
}
