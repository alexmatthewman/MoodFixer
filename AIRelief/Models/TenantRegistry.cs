using System;
using System.Collections.Generic;
using System.Linq;

namespace AIRelief.Models
{
    public class TenantRegistry
    {
        private readonly Dictionary<string, TenantConfig> _tenants;
        private readonly Dictionary<string, TenantConfig> _hostLookup;

        public TenantRegistry(Dictionary<string, TenantConfig> tenants)
        {
            _tenants = tenants;

            _hostLookup = new Dictionary<string, TenantConfig>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var (_, tenant) in tenants)
            {
                foreach (var host in tenant.HostNames)
                {
                    _hostLookup[host] = tenant;
                }
            }
        }

        public TenantConfig? Resolve(string hostname)
        {
            _hostLookup.TryGetValue(hostname, out var tenant);
            return tenant;
        }

        public TenantConfig GetByCode(string marketCode)
            => _tenants[marketCode];

        public IEnumerable<TenantConfig> All => _tenants.Values;

        public string[] AllLanguages
            => _tenants.Values
                .SelectMany(t => t.SupportedLanguages)
                .Distinct()
                .ToArray();
    }
}
