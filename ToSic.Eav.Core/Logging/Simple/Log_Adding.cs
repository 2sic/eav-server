using System;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {

        /// <inheritdoc />
        public string Add(string message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0)
        {
            AddEntry(message, callerPath, callerName, callerLine);
            return message;
        }


        /// <summary>
        /// Add a message
        /// </summary>
        private Entry AddEntry(string message, string callerPath = "", string callerName = "", int callerLine = 0)
        {
            var e = new Entry(this, message, WrapDepth, callerPath, callerName, callerLine);
            AddToEntriesAndParent(e);
            return e;
        }


        /// <summary>
        /// Try to call a function generating a message. 
        /// This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        private static string Try(Func<string> messageMaker)
        {
            try
            {
                return messageMaker.Invoke();
            }
            catch (Exception ex)
            {
                return "LOG: failed to generate from code, error: " + ex.Message;
            }
        }


        public string Warn(string message) => Add("WARNING: " + message);


        /// <summary>
        /// add an existing entry of another logger
        /// </summary>
        /// <param name="entry"></param>
        private void AddToEntriesAndParent(Entry entry)
        {
            Entries.Add(entry);
            (_parent as Log)?.AddToEntriesAndParent(entry);
        }

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        public void Add(Func<string> messageMaker) => Add(Try(messageMaker));

    }
}
