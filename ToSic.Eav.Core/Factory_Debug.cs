﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ToSic.Eav
{
    public partial class Factory
    {
        /// <summary>
        /// Internal debugging, disabled by default. If set to true, resolves will be counted and logged.
        /// </summary>
        public static bool Debug = false;

        /// <summary>
        /// Counter for internal statistics and debugging. Will only be incremented if Debug = true.
        /// </summary>
        public static int CountResolves;

        public static List<string> ResolvesList = new List<string>();

        public static void LogResolve(Type t, bool generic)
        {
            CountResolves++;

            // Get call stack
            var stackTrace = new StackTrace();

            // Get calling method name
            var mName = stackTrace.GetFrame(2).GetMethod().Name;

            ResolvesList.Add((generic ? "<>" : "()") + t.Name + "..." + mName);

        }
    }
}
