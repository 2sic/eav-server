using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class Factory
	{
		private static IUnityContainer _container;

		/// <summary>
		/// The IoC Container responsible for our Inversion of Control
		/// Use this everywhere!
		/// Syntax: Factory.Container.Resolve<Type>
		/// </summary>
		public static IUnityContainer Container
		{
			get
			{
				if (_container == null)
				{
					_container = new UnityContainer();

					var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
					if (section?.Containers["ToSic.Eav"] != null)
						_container.LoadConfiguration("ToSic.Eav");
				}
				return _container;
			}
		}

        public static t Resolve<t>() => Container.Resolve<t>();
	}
}