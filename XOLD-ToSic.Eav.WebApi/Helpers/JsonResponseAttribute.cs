using System;
using System.Web.Http.Controllers;

namespace ToSic.Eav.WebApi.Helpers
{
    public class JsonResponseAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor) 
            => controllerSettings.Formatters.Remove(controllerSettings.Formatters.XmlFormatter);
    }
}