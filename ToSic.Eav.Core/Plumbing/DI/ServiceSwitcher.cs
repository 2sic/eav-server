using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Plumbing.DI
{
    public class ServiceSwitcher<T> where T : ISwitchableService
    {
        public ServiceSwitcher(IEnumerable<T> allServices) => AllServices = allServices?.ToList();
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
    }
}
