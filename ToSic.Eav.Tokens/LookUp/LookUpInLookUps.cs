﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// This Value Provider chains two or more LookUps and tries one after another to deliver a result
    /// It's mainly used to override values which are given, by a special situation
    /// </summary>
    [PublicApi]
    public class LookUpInLookUps: LookUpBase
    {
        protected List<ILookUp> Providers = new List<ILookUp>(); 

        /// <summary>
        /// Generate a lookup-of-lookups. 
        /// </summary>
        /// <param name="name">Name to use - if stored in a list</param>
        /// <param name="first">First LookUp source</param>
        /// <param name="second">Second LookUp source</param>
        /// <param name="third">Optional third</param>
        /// <param name="fourth">Optional fourth</param>
        public LookUpInLookUps(string name, ILookUp first, ILookUp second, ILookUp third = null, ILookUp fourth = null)
        {
            Name = name;
            Providers.Add(first);
            Providers.Add(second);
            if(third != null) Providers.Add(third);
            if(fourth != null) Providers.Add(fourth);
        }

        // not sure - doesn't seem used?
        [PrivateApi]
        public LookUpInLookUps(string name, List<ILookUp> providers)
        {
            Name = name;
            Providers = providers;
        }
        
        
        /// <inheritdoc/>
        public override string Get(string key, string format, ref bool notFound)
        {
            var usedSource = Providers.Find(p => p.Has(key));
            if (usedSource == null)
            {
                notFound = true;
                return null;
            }
            return usedSource.Get(key, format, ref notFound);
        }

        /// <inheritdoc/>
        public override bool Has(string key)
            => Providers.Any(prov => prov.Has(key));

    }
}
