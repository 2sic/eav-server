using System;
using System.Collections.Generic;

namespace ToSic.Eav.Logging.Simple
{
    public class Log
    {
        private const string Separator = " :: ";
        // unique ID of this logger, to not confuse it with other loggers
        private readonly string _id = Guid.NewGuid().ToString().Substring(0, 4);

        private string _container = "unknwn";
        public readonly List<Entry> Entries = new List<Entry>();
        private Log _parent;

        public string Identifier => _container + "(" + _id + ")";

        public Log(string container, Log parent = null, string initialMessage = null)
        {
            Rename(container);
            LinkTo(parent);
            if(initialMessage != null)
                Add(initialMessage);
        }

        /// <summary>
        /// Rename this logger - usually used when a base-class has a logger, 
        /// but the inherited class needs a different name
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name) => _container = name.Substring(0, Math.Min(name.Length, 6));

        /// <summary>
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        public void Add(string message)
        {
            var entry = new Entry(Identifier, message);
            Entries.Add(entry);
            _parent?.Add(entry);
        }


        public void Add(Entry entry)
        {
            var newEntry = new Entry(Identifier + entry.Source, entry.Message);
            Entries.Add(newEntry);
            _parent?.Add(newEntry);
        }

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        public void Add(Func<string> messageMaker)
        {
            try
            {
                Add(messageMaker.Invoke());
            }
            catch (Exception ex)
            {
                Add("Log: failed to add message from code, error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Link this logger to a parent
        /// </summary>
        /// <param name="parent"></param>
        public void LinkTo(Log parent)
        {
            if (parent != null)
                _parent = parent;
        }

        public string Serialize()
        {
            string result = "";
            Entries.ForEach(e => result += Serialize(e) + "¬\n");
            return result;
        }

        private string Serialize(Entry entry)
        {
            return entry.Source + Separator + entry.Message;
            //if (msg.IndexOf(Separator, StringComparison.Ordinal) == -1)
            //    msg = Separator + msg;
            //return Identifier + msg;
        }

        public string SerializeTree() => _parent?.SerializeTree() ?? Serialize();
    }
}
