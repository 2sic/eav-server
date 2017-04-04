using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.AppEngine
{
    /// <summary>
    /// Configuration values used by the runtime as the app is in use
    /// </summary>
    public class AppConfiguration
    {
        // todo: move to some kind of injection thingy?

        /// <summary>
        /// The type name used to store templates in the eav-system
        /// </summary>
        public static string TemplateContentType = "2SexyContent-Template";
    }
}
