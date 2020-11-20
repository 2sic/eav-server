using System;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {

        /// <inheritdoc />
        public string Add(string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
        {
            AddInternal(message, new CodeRef(cPath, cName, cLine));
            return message;
        }


        /// <summary>
        /// Add a message
        /// </summary>
        private Entry AddInternal(string message, CodeRef code)
        {
            var e = new Entry(this, message, WrapDepth, code);
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
            // prevent parallel threads from updating entries at the same time
            lock (Entries) { Entries.Add(entry); }
            (_parent as Log)?.AddToEntriesAndParent(entry);
        }

        /// <inheritdoc />
        public void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0) 
            => Add(Try(messageMaker), cPath, cName, cLine);

    }
}
