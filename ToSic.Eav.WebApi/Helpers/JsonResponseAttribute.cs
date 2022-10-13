#if NETFRAMEWORK
using System;
using System.Web.Http.Controllers;

// Special case: this should enforce json formatting
// It's only needed in .net4x where the default is xml
namespace ToSic.Eav.WebApi.Helpers
{
    public class JsonResponseAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.XmlFormatter);
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.JsonFormatter);
        }
    }
}

#endif
