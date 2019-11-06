using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.ValueProviders
{
    // This Value Provider chains two value providers and tries one or the other to deliver a result
    // It's mainly used to override values which are given, by a special situation
    public class OverrideValueProvider: BaseValueProvider
    {
        public List<IValueProvider> Providers = new List<IValueProvider>(); 

        public OverrideValueProvider(string name, IValueProvider first, IValueProvider second, IValueProvider third = null, IValueProvider fourth = null)
        {
            Name = name;
            Providers.Add(first);
            Providers.Add(second);
            if(third != null) Providers.Add(third);
            if(fourth != null) Providers.Add(fourth);
        }

        public OverrideValueProvider(string name, List<IValueProvider> providers)
        {
            Name = name;
            Providers = providers;
        }

        public override string Get(string property, string format, ref bool propertyNotFound)
        {
            var usedSource = Providers.Find(p => p.Has(property));
            if (usedSource == null)
            {
                propertyNotFound = true;
                return null;
            }
            return usedSource.Get(property, format, ref propertyNotFound);
        }

        public override bool Has(string property)
            => Providers.Any(prov => prov.Has(property));

    }
}
