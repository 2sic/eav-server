using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav
{
    /// <summary>
    /// The Eav DI Factory, used to construct various objects through [Dependency Injection](xref:NetCode.DependencyInjection.Index).
    ///
    /// If possible avoid using this, as it's a workaround for code which is outside of the normal Dependency Injection and therefor a bad pattern.
    /// </summary>
    [PublicApi("Careful - obsolete!")]
	public partial class Factory
	{
        [Obsolete("Not used any more, but keep for API consistency in case something calls ActivateNetCoreDi")]
        public delegate void ServiceConfigurator(IServiceCollection service);

        [PrivateApi]
	    public static void ActivateNetCoreDi(ServiceConfigurator configure)
	    {
            new LogHistory().Add("error", new Log(LogNames.Eav + ".DepInj", null, $"{nameof(ActivateNetCoreDi)} was called, but this won't do anything any more."));
        }

        /// <summary>
        /// Dependency Injection resolver with a known type as a parameter.
        /// </summary>
        /// <typeparam name="T">The type / interface we need.</typeparam>
        [Obsolete]
        public static T Resolve<T>()
        {
            throw new NotSupportedException("The Eav.Factory is obsolete. See https://r.2sxc.org/brc-13-eav-factory");
        }
    }
}