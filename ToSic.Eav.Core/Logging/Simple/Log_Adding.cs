using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {

        /// <summary>
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        public string Add(string message)
        {
            AddEntry(message);
            return message;
        }

        /// <summary>
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        private Entry AddEntry(string message)
        {
            var e = new Entry(this, message, WrapDepth);
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
