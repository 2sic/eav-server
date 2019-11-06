using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// This Value Provider chains two value providers and tries one or the other to deliver a result
    /// It's mainly used to override values which are given, by a special situation
    /// </summary>
    public class LookUpInLookUps: LookUpBase
    {
        public List<ILookUp> Providers = new List<ILookUp>(); 

        public LookUpInLookUps(string name, ILookUp first, ILookUp second, ILookUp third = null, ILookUp fourth = null)
        {
            Name = name;
            Providers.Add(first);
            Providers.Add(second);
            if(third != null) Providers.Add(third);
            if(fourth != null) Providers.Add(fourth);
        }

        public LookUpInLookUps(string name, List<ILookUp> providers)
        {
            Name = name;
            Providers = providers;
        }

        public override string Get(string key, string format, ref bool propertyNotFound)
        {
            var usedSource = Providers.Find(p => p.Has(key));
            if (usedSource == null)
            {
                propertyNotFound = true;
                return null;
            }
            return usedSource.Get(key, format, ref propertyNotFound);
        }

        public override bool Has(string key)
            => Providers.Any(prov => prov.Has(key));

    }
}
