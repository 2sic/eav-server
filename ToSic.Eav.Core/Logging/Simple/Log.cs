﻿using System;
using System.Collections.Generic;

namespace ToSic.Eav.Logging.Simple
{
    public class Log
    {
        private const string Separator = " :: ";
        // unique ID of this logger, to not confuse it with other loggers
        private readonly string _id = Guid.NewGuid().ToString().Substring(0, 4);

        private string _container = "unknwn";
        private readonly List<Entry> _entries = new List<Entry>();
        private Log _parent;

        public Log(string container, Log parent = null, string message = null)
        {
            Rename(container);
            LinkTo(parent);
            if(message != null)
                Add(message);
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
            var entry = new Entry(message);
            _entries.Add(entry);
            _parent?.Add(Serialize(entry));
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
            _entries.ForEach(e => result += Serialize(e) + "¬\n");
            return result;
        }

        private string Serialize(Entry entry)
        {
            var msg = entry.Serialize();
            if (msg.IndexOf(Separator, StringComparison.Ordinal) == -1)
                msg = Separator + msg;
            return _container + "(" + _id + ")" + msg;
        }

        public string SerializeTree() => _parent?.SerializeTree() ?? Serialize();
    }
}
