using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Plumbing.DI
{
    public class ServiceSwitcher<T>: HasLog, ILazyLike<T> where T : ISwitchableService
    {
        // TODO
        // - constructor with name
        // - add to global log history when regenerating incl. choice
        // create name based overload

        public ServiceSwitcher(IEnumerable<T> allServices) : base(LogNames.Eav + ".SrvSwt")
        {
            AllServices = allServices?.ToList();
        }

        public readonly List<T> AllServices;


        public T Value
        {
            get
            {
                if (_preferredService != null) return _preferredService;

                var all = AllServices;
                if (all == null || !all.Any()) 
                    throw new ArgumentException($"There ware no services for the type '{typeof(T).FullName}', cannot find best option");

                foreach (var svc in all.OrderByDescending(s => s.Priority))
                    if (svc.IsViable())
                        return _preferredService = svc;

                throw new ArgumentException($"No viable services found for type '{typeof(T).FullName}'");
            }
        }
        private T _preferredService;

        public bool IsValueCreated => _preferredService != null;

        public T ByNameId(string nameId) => AllServices.Find(s => s.NameId.Equals(nameId));
    }
}
